namespace Dashboard.Tests;

/// <summary>
/// Boundary, null/empty and fixed-value cases of <see cref="RuntimePayloadBuilder"/>'s public members that the other
/// runtime payload tests do not pin: the exact values of its constants, its null guards, an empty manifest, and the
/// edges of <see cref="RuntimePayloadBuilder.Deployables"/>, <see cref="RuntimePayloadBuilder.Watch"/> and
/// <see cref="RuntimePayloadBuilder.MarkOf"/>.
/// </summary>
public sealed class RuntimePayloadBuilderEdgeCaseTests
{
    private static readonly Uri App = new("https://ca-demo-uat-scorecard.example.net");

    private static RuntimeManifest Empty() => new("uat", [], [], []);

    private static RuntimeManifest Of(params RuntimeNode[] nodes) => new("uat", nodes, [], []);

    private static RuntimeNode Node(string alias, RuntimeNodeKind kind, string? deployable = null, Uri? url = null, IReadOnlyList<string>? ids = null) =>
        new(alias, kind, alias, url, deployable, Ids: ids);

    private static DeployableStatus Deployable() =>
        new(new DeployableInfo("ui", null, "/_healthcheck", "/alive", "/_version", []));

    // ---------------------------------------------------------------- constants

    [Fact]
    public void TheStateWordsAreFixedLowercaseWords()
    {
        Assert.Equal("healthy", RuntimePayloadBuilder.Healthy);
        Assert.Equal("unhealthy", RuntimePayloadBuilder.Unhealthy);
        Assert.Equal("unreachable", RuntimePayloadBuilder.Unreachable);
        Assert.Equal("checking", RuntimePayloadBuilder.Checking);
        Assert.Equal("neutral", RuntimePayloadBuilder.Neutral);
    }

    [Fact]
    public void TheNumberPlaceholderIsASingleEnDash()
    {
        Assert.Equal("–", RuntimePayloadBuilder.NoNumber);
        Assert.Single(RuntimePayloadBuilder.NoNumber);
    }

    [Fact]
    public void TheUnitOfACountIsCallsPerMinute() =>
        Assert.Equal("calls/min", RuntimePayloadBuilder.CallsUnit);

    [Fact]
    public void ALineHoldsEightMarksAndADescriptionThirtySixCharacters()
    {
        Assert.Equal(8, RuntimePayloadBuilder.MostMarks);
        Assert.Equal(36, RuntimePayloadBuilder.DescriptionLength);
    }

    // ---------------------------------------------------------------- MarkOf

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    [InlineData(999)]
    public void AStateOutsideTheEnumIsMarkedUnknownAndNotRefused(int value) =>
        Assert.Equal("unknown", RuntimePayloadBuilder.MarkOf((CheckState)value));

    // ---------------------------------------------------------------- Build

    [Fact]
    public void BuildRefusesNoManifestAndNoZone()
    {
        Assert.Equal("manifest", Assert.Throws<ArgumentNullException>(() => RuntimePayloadBuilder.Build(null!, null, null, TimeZoneInfo.Utc)).ParamName);
        Assert.Equal("zone", Assert.Throws<ArgumentNullException>(() => RuntimePayloadBuilder.Build(Empty(), null, null, null!)).ParamName);
    }

    [Fact]
    public void AnEmptyManifestBuildsAnEmptyPayloadWithNoNames()
    {
        var payload = RuntimePayloadBuilder.Build(Empty(), null, null, TimeZoneInfo.Utc);

        Assert.Empty(payload.Nodes);
        Assert.Empty(payload.Regions);
        Assert.Empty(payload.Edges);
        Assert.Null(payload.Names);
    }

    [Fact]
    public void APersonWithoutALinkIsNeitherATileNorAName()
    {
        var payload = RuntimePayloadBuilder.Build(Of(Node("user", RuntimeNodeKind.Person)), null, null, TimeZoneInfo.Utc);

        Assert.Empty(payload.Nodes);
        Assert.Null(payload.Names);
    }

    // ---------------------------------------------------------------- Deployables

    [Fact]
    public void DeployablesRefusesNoManifest() =>
        Assert.Equal("manifest", Assert.Throws<ArgumentNullException>(() => RuntimePayloadBuilder.Deployables(null!)).ParamName);

    [Fact]
    public void AnEmptyManifestHasNoDeployables() =>
        Assert.Empty(RuntimePayloadBuilder.Deployables(Empty()));

    [Fact]
    public void ADeployableNamedTwiceInAnyCaseIsListedOnceAsFirstNamed() =>
        Assert.Equal(
            ["Scorecard"],
            RuntimePayloadBuilder.Deployables(Of(
                Node("a", RuntimeNodeKind.ContainerApp, "Scorecard"),
                Node("b", RuntimeNodeKind.WebApp, "scorecard"),
                Node("c", RuntimeNodeKind.StaticSite, "SCORECARD"))));

    [Fact]
    public void APersonADependencyAndANodeWithoutADeployableAreNoDeployables() =>
        Assert.Empty(RuntimePayloadBuilder.Deployables(Of(
            Node("user", RuntimeNodeKind.Person, "ui"),
            Node("api", RuntimeNodeKind.Dependency, "ui"),
            Node("web", RuntimeNodeKind.WebApp))));

    // ---------------------------------------------------------------- Watch

    [Fact]
    public void WatchRefusesNoManifest() =>
        Assert.Equal("manifest", Assert.Throws<ArgumentNullException>(() => RuntimePayloadBuilder.Watch(null!)).ParamName);

    [Fact]
    public void AnEmptyManifestWatchesNothing()
    {
        var watch = RuntimePayloadBuilder.Watch(Empty());

        Assert.Empty(watch.Apps);
        Assert.False(watch.ResourceHealth);
    }

    [Fact]
    public void AnEmptyListOfIdsDoesNotAskForResourceHealth()
    {
        var manifest = Of(Node("db", RuntimeNodeKind.Resource, ids: [])) with { Unplaced = [new RuntimeUnplaced("kv", Ids: [])] };

        Assert.False(RuntimePayloadBuilder.Watch(manifest).ResourceHealth);
    }

    [Fact]
    public void AnUnplacedEntryWithIdsAloneAsksForResourceHealth()
    {
        var manifest = Empty() with { Unplaced = [new RuntimeUnplaced("kv", Ids: ["/subscriptions/x/kv"])] };

        Assert.True(RuntimePayloadBuilder.Watch(manifest).ResourceHealth);
    }

    [Fact]
    public void TheSameContainerAppDrawnTwiceIsProbedOnce() =>
        Assert.Equal(
            [new ContainerAppTarget(App)],
            RuntimePayloadBuilder.Watch(Of(
                Node("ca1", RuntimeNodeKind.ContainerApp, url: App),
                Node("ca2", RuntimeNodeKind.ContainerApp, url: App),
                Node("ca3", RuntimeNodeKind.ContainerApp))).Apps);

    // ---------------------------------------------------------------- VersionLink

    [Fact]
    public void VersionLinkRefusesNoDeployable() =>
        Assert.Equal("deployable", Assert.Throws<ArgumentNullException>(() => RuntimePayloadBuilder.VersionLink(null!, "2.4.15")).ParamName);

    [Theory]
    [InlineData("")]
    [InlineData("2.4.15")]
    public void ADeployableWithNoProjectAndNoBuildLeadsNowhere(string version) =>
        Assert.Null(RuntimePayloadBuilder.VersionLink(Deployable(), version));
}
