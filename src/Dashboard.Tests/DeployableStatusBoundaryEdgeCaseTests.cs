using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// The boundaries of <see cref="DeployableStatus" /> that <see cref="DeployableStatusEdgeCaseTests" /> leaves open:
/// a build path that is named but empty or blank, a role that only looks like the primary one, the version words
/// <see cref="DeployableStatus.BuildIsDue" /> compares under a culture with its own idea of an I, the pin a
/// deployable with a <c>pinUrl</c> starts on, and the tile <see cref="DeployableStatus.Assess(out TargetStatus?)" />
/// expects matching the first of <see cref="DeployableStatus.AssessServing" />. Nothing reaches the network and
/// nothing reads the clock.
/// </summary>
public class DeployableStatusBoundaryEdgeCaseTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    private static NodeInfo Node(string name, string role = NodeInfo.PrimaryRole) =>
        new(name, "westus3", role, new Uri($"https://{name}.example.net"));

    private static DeployableStatus Deployable(NodeInfo[] nodes, string? buildPath = null, Uri? pinUrl = null) =>
        new(new DeployableInfo(
            "ui",
            null,
            DeployableInfo.DefaultHealthPath,
            DeployableInfo.DefaultAlivePath,
            DeployableInfo.DefaultVersionPath,
            nodes,
            BuildPath: buildPath,
            PinUrl: pinUrl));

    private static ProbeResult Answer(HealthState state) => new(state, 200, 12, Start);

    private static T Under<T>(string culture, Func<T> read)
    {
        var before = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            return read();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = before;
        }
    }

    /// <summary>Only a build path that is not there leaves the deployable without a build source: the empty word and
    /// a blank one are paths the file named, and the primary node is asked for them.</summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("/_build")]
    public void AnyBuildPathTheFileNamesEvenEmptyGivesTheDeployableABuildSource(string buildPath)
    {
        var deployable = Deployable([Node("app-west", NodeInfo.StandbyRole), Node("app-east")], buildPath);

        Assert.Same(deployable.Nodes[1], deployable.BuildSource);
        Assert.Null(Deployable([Node("app-west", NodeInfo.StandbyRole), Node("app-east")]).BuildSource);
    }

    /// <summary>A role that is the primary one except for a space or a letter is no primary: the build source falls
    /// back to the first node of the file.</summary>
    [Theory]
    [InlineData(" primary")]
    [InlineData("primary ")]
    [InlineData("primaries")]
    [InlineData("")]
    public void ARoleThatOnlyLooksPrimaryLeavesTheFirstNodeAsTheBuildSource(string role)
    {
        var deployable = Deployable([Node("app-west", NodeInfo.StandbyRole), Node("app-east", role)], "/_build");

        Assert.False(deployable.Nodes[1].IsPrimary);
        Assert.Same(deployable.Nodes[0], deployable.BuildSource);
    }

    /// <summary>The version words are compared ordinally without regard to case, whatever culture the thread runs
    /// under: in Turkish an I is still the same version as an i, and a dotted İ or a dotless ı is another one.</summary>
    [Theory]
    [InlineData("tr-TR")]
    [InlineData("en-US")]
    [InlineData("")]
    public void TheBuildIsDueByOrdinalCaseFoldingUnderEveryCulture(string culture)
    {
        var deployable = Deployable([Node("app-west")], "/_build");
        deployable.RecordBuild(null, "rel-i");

        Assert.False(Under(culture, () => deployable.BuildIsDue("REL-I")));
        Assert.True(Under(culture, () => deployable.BuildIsDue("REL-İ")));
        Assert.True(Under(culture, () => deployable.BuildIsDue("rel-ı")));
    }

    /// <summary>A deployable whose row names a <c>pinUrl</c> starts on the one shared pending pin and remembers no
    /// good reading; one that names none starts on nothing.</summary>
    [Fact]
    public void ADeployableWithAPinUrlStartsOnThePendingPinAndRemembersNone()
    {
        var pinned = Deployable([Node("app-west")], pinUrl: new Uri("https://raw.example.net/uat/ui/kustomization.yaml"));

        Assert.Same(PinnedVersions.Pending, pinned.Pinned);
        Assert.Equal(PinnedVersionsState.Pending, pinned.Pinned!.State);
        Assert.Null(pinned.LastReadPinned);
        Assert.Null(Deployable([Node("app-west")]).Pinned);
    }

    /// <summary>The three ways of deciding agree: the tile <see cref="DeployableStatus.Assess(out TargetStatus?)" />
    /// expects is the first one <see cref="DeployableStatus.AssessServing" /> names, and the state is the same.</summary>
    [Fact]
    public void TheExpectedTileIsTheFirstOfTheServingTiles()
    {
        var deployable = Deployable([Node("app-west", NodeInfo.StandbyRole), Node("app-east")]);
        deployable.Nodes[0].Record(Answer(HealthState.Healthy));
        deployable.Nodes[1].Record(Answer(HealthState.Healthy));

        var decided = deployable.AssessServing(out var serving);
        var assessed = deployable.Assess(out var expected);

        Assert.Equal(ServingState.Primary, decided.State);
        Assert.Equal(decided.State, assessed.State);
        Assert.Equal(decided.State, deployable.Assess().State);
        Assert.Same(deployable.Nodes[1], Assert.Single(serving));
        Assert.Same(serving[0], expected);
    }
}
