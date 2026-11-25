namespace Dashboard.Tests;

/// <summary>
/// One arrow of each kind the runtime payload draws (origin, SQL, dependency, public, dashboard, and any other),
/// pinned by kind so the method each kind has in the builder keeps its words and its state: a caller that is down
/// leaves its SQL and dependency arrows idle, and the browser's arrows say whether their end answers.
/// </summary>
public class RuntimePayloadEdgeKindTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private static readonly Uri Page = new("http://localhost:5210/");

    private static readonly RuntimeManifest Manifest = new(
        "uat",
        [
            new RuntimeNode("browser", RuntimeNodeKind.Person, "Browser"),
            new RuntimeNode("app_1", RuntimeNodeKind.WebApp, "app1", new Uri("https://app1.example.net"), "ui"),
            new RuntimeNode("sqldb", RuntimeNodeKind.Sql, "sqldb-uat"),
            new RuntimeNode("api", RuntimeNodeKind.Dependency, "Weather API"),
            new RuntimeNode("api_2", RuntimeNodeKind.Dependency, "Maps API"),
        ],
        [],
        [
            new RuntimeEdge("browser-to-app_1", "browser", "app_1", RuntimeEdgeKind.Public),
            new RuntimeEdge("fd-to-app_1", "fd", "app_1", RuntimeEdgeKind.Origin),
            new RuntimeEdge("app_1-to-sqldb", "app_1", "sqldb", RuntimeEdgeKind.Sql),
            new RuntimeEdge("app_1-to-api", "app_1", "api", RuntimeEdgeKind.Dependency),
            new RuntimeEdge("browser-to-swa", "browser", "swa", RuntimeEdgeKind.Dashboard),
            new RuntimeEdge("a-to-b", "a", "b", RuntimeEdgeKind.Other),
        ]);

    private static EnvironmentStatus Environment(HealthState state)
    {
        var environment = new EnvironmentStatus(TopologyParser.Parse("""
            { "environments": [ { "name": "uat", "deployables": [ { "name": "ui", "frontDoor": null, "nodes": [ { "name": "app1", "role": "primary", "url": "https://app1.example.net" } ] } ] } ] }
            """).Topology!.Environments[0]);
        environment.Deployables[0].Nodes[0].Record(new ProbeResult(state, state == HealthState.Healthy ? 200 : null, 40, Now, "2.4.21"));
        return environment;
    }

    private static readonly string[] ToTheWebApp = ["app_1-to-sqldb", "app_1-to-api", "fd-to-app_1", "browser-to-app_1"];

    private static RuntimeEdgeMark Edge(RuntimePayload payload, string id) => payload.Edges.Single(edge => edge.Id == id);

    [Fact]
    public void WithoutAnEnvironmentEveryArrowIsNeutralAndSaysSoInItsOwnWords()
    {
        var payload = RuntimePayloadBuilder.Build(Manifest, null, Page, TimeZoneInfo.Utc);

        Assert.All(payload.Edges, edge => Assert.Equal(EdgeState.Neutral.Word(), edge.State));
        Assert.Equal(
            "Front Door to app_1, origin priority not known: when priority 1 is down. Not checked. Calls per minute: the web app reports none.",
            Edge(payload, "fd-to-app_1").Title);
        Assert.Equal(
            "app_1 to the database. Not checked. Queries per minute: the web app reports none.",
            Edge(payload, "app_1-to-sqldb").Title);
        Assert.Equal("app queries", Edge(payload, "app_1-to-sqldb").Text);
        Assert.Equal(
            "app_1 to Weather API. Not checked. Calls per minute: the web app reports none.",
            Edge(payload, "app_1-to-api").Title);
        Assert.Equal("outgoing HTTP calls", Edge(payload, "app_1-to-api").Text);
        Assert.StartsWith("The browser to app_1: Not checked. ", Edge(payload, "browser-to-app_1").Title, StringComparison.Ordinal);
    }

    [Fact]
    public void ADashboardArrowWhoseEndTheTopologyDoesNotKnowAndAnArrowOfAnyOtherKindAreNamedByTheirEnds()
    {
        var payload = RuntimePayloadBuilder.Build(Manifest, null, Page, TimeZoneInfo.Utc);

        Assert.Equal(new RuntimeEdgeMark("browser-to-swa", EdgeState.Neutral.Word(), null, null, null, "browser to swa"), Edge(payload, "browser-to-swa"));
        Assert.Equal(new RuntimeEdgeMark("a-to-b", EdgeState.Neutral.Word(), null, null, null, "a to b"), Edge(payload, "a-to-b"));
    }

    [Fact]
    public void ADependencyOfAWebAppWithMoreThanOneIsNotCountedApart()
    {
        var manifest = Manifest with { Edges = [.. Manifest.Edges, new RuntimeEdge("app_1-to-api_2", "app_1", "api_2", RuntimeEdgeKind.Dependency)] };

        var payload = RuntimePayloadBuilder.Build(manifest, null, Page, TimeZoneInfo.Utc);
        var dependency = Edge(payload, "app_1-to-api_2");

        Assert.Equal((RuntimePayloadBuilder.NoNumber, "not counted apart"), (dependency.Number, dependency.Text));
        Assert.EndsWith("Calls per minute: the web app counts its outgoing HTTP calls as one number, and it has more than one dependency.", dependency.Title, StringComparison.Ordinal);
        Assert.StartsWith("app_1 to Maps API. ", dependency.Title, StringComparison.Ordinal);
    }

    [Fact]
    public void AWebAppThatIsDownLeavesItsCallsIdleAndTheBrowsersArrowDown()
    {
        var payload = RuntimePayloadBuilder.Build(Manifest, Environment(HealthState.Unreachable), Page, TimeZoneInfo.Utc);

        Assert.Equal(EdgeState.Idle.Word(), Edge(payload, "app_1-to-sqldb").State);
        Assert.Equal(EdgeState.Idle.Word(), Edge(payload, "app_1-to-api").State);
        Assert.Equal(EdgeState.Down.Word(), Edge(payload, "fd-to-app_1").State);
        Assert.Equal(EdgeState.Down.Word(), Edge(payload, "browser-to-app_1").State);
    }

    [Fact]
    public void AHealthyWebAppThatServesCarriesItsCallsAndTheBrowsersArrowIsActive()
    {
        var payload = RuntimePayloadBuilder.Build(Manifest, Environment(HealthState.Healthy), Page, TimeZoneInfo.Utc);

        Assert.Equal(EdgeState.Active.Word(), Edge(payload, "app_1-to-sqldb").State);
        Assert.Equal(EdgeState.Active.Word(), Edge(payload, "app_1-to-api").State);
        Assert.Equal(EdgeState.Active.Word(), Edge(payload, "fd-to-app_1").State);
        Assert.Equal(EdgeState.Active.Word(), Edge(payload, "browser-to-app_1").State);
    }

    [Fact]
    public void AWebAppNotCheckedYetLeavesEveryArrowToItBeingChecked()
    {
        var environment = new EnvironmentStatus(TopologyParser.Parse("""
            { "environments": [ { "name": "uat", "deployables": [ { "name": "ui", "frontDoor": null, "nodes": [ { "name": "app1", "role": "primary", "url": "https://app1.example.net" } ] } ] } ] }
            """).Topology!.Environments[0]);

        var payload = RuntimePayloadBuilder.Build(Manifest, environment, Page, TimeZoneInfo.Utc);

        Assert.All(
            ToTheWebApp,
            id => Assert.Equal(EdgeState.Checking.Word(), Edge(payload, id).State));
    }
}
