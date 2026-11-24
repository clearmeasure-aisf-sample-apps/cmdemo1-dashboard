using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The one read of a deployable's build endpoint (#1015): not asked is due for every version, a read is due again only
/// for another version (ignoring case), a read that answered nothing still counts as a read, and the deployable's
/// three parts and its <see cref="DeployableStatus.BuildIsDue" /> say what the read says.
/// </summary>
public class BuildReadTests
{
    private static readonly BuildInfo SomeBuild = new("2.4.15", "abc1234", null, null, null, null, null, null, null, null, null);

    private static DeployableStatus Deployable() =>
        new(new DeployableInfo(
            "ui",
            null,
            DeployableInfo.DefaultHealthPath,
            DeployableInfo.DefaultAlivePath,
            DeployableInfo.DefaultVersionPath,
            [new NodeInfo("app-west", "westus3", NodeInfo.PrimaryRole, new Uri("https://app-west.example.net"))],
            BuildPath: "/_build"));

    [Fact]
    public void NotYetIsNothingAskedAndDueForEveryVersionAndForNone()
    {
        var read = BuildRead.NotYet;

        Assert.False(read.WasRead);
        Assert.Null(read.Build);
        Assert.Null(read.Version);
        Assert.True(read.IsDue(null));
        Assert.True(read.IsDue(string.Empty));
        Assert.True(read.IsDue("2.4.15"));
        Assert.Same(BuildRead.NotYet, BuildRead.NotYet);
    }

    [Theory]
    [InlineData("2.4.15", "2.4.15", false)]
    [InlineData("v2.4.15-RC", "V2.4.15-rc", false)]
    [InlineData(null, null, false)]
    [InlineData("2.4.15", "2.4.16", true)]
    [InlineData("2.4.15", null, true)]
    [InlineData(null, "2.4.15", true)]
    [InlineData("", null, true)]
    [InlineData(" 2.4.15 ", "2.4.15", true)]
    public void AReadIsDueAgainOnlyForAnotherVersion(string? readFor, string? version, bool due)
    {
        Assert.Equal(due, BuildRead.Read(SomeBuild, readFor).IsDue(version));
        Assert.Equal(due, BuildRead.Read(null, readFor).IsDue(version));
    }

    [Fact]
    public void AReadKeepsWhatItAnsweredAndTheVersionItWasAskedFor()
    {
        var read = BuildRead.Read(SomeBuild, "2.4.15");

        Assert.True(read.WasRead);
        Assert.Same(SomeBuild, read.Build);
        Assert.Equal("2.4.15", read.Version);

        var nothing = BuildRead.Read(null, null);

        Assert.True(nothing.WasRead);
        Assert.Null(nothing.Build);
        Assert.Null(nothing.Version);
        Assert.NotEqual(BuildRead.NotYet, nothing);
        Assert.False(nothing.IsDue(null));
    }

    [Fact]
    public void AReadIsMadeOnlyByItsTwoNamedWays()
    {
        Assert.Empty(typeof(BuildRead).GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.All(
            typeof(BuildRead).GetProperties(BindingFlags.Public | BindingFlags.Instance),
            property => Assert.Null(property.SetMethod));
    }

    [Fact]
    public void TheDeployableSaysWhatItsBuildReadSays()
    {
        var deployable = Deployable();
        string?[] versions = [null, string.Empty, "2.4.15", "2.4.16", "V2.4.16"];

        foreach (var version in versions)
        {
            Assert.Equal(BuildRead.NotYet.IsDue(version), deployable.BuildIsDue(version));
        }

        deployable.RecordBuild(SomeBuild, "2.4.16");
        var expected = BuildRead.Read(SomeBuild, "2.4.16");

        Assert.Same(SomeBuild, deployable.Build);
        Assert.True(deployable.BuildWasRead);
        Assert.Equal("2.4.16", deployable.BuildReadForVersion);
        foreach (var version in versions)
        {
            Assert.Equal(expected.IsDue(version), deployable.BuildIsDue(version));
        }

        deployable.RecordBuild(null, null);

        Assert.Null(deployable.Build);
        Assert.True(deployable.BuildWasRead);
        Assert.Null(deployable.BuildReadForVersion);
        Assert.False(deployable.BuildIsDue(null));
        Assert.True(deployable.BuildIsDue("2.4.16"));
    }
}
