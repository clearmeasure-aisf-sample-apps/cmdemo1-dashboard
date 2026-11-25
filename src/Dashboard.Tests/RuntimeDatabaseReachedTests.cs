namespace Dashboard.Tests;

/// <summary>
/// The database has two sources that can say it was reached, the web apps of the monitor and the container apps the
/// page probes, and each is now read once into one small reading of an app that was asked about the database
/// (<c>ReachedOf</c>, in <c>RuntimePayloadBuilder</c>, #1750): the name the title gives the app, the name the line
/// gives it, and when it last answered. One method (<c>DatabaseReached</c>) then owns the reachable branch over the
/// readings of whichever source spoke, and which source speaks is one choice over the two rather than a nested pair of
/// count checks. These tests pin the two readings (a web app's name and label are its own two words, a container app
/// has one name for both), the shared "last checked" of the readings, and that where a diagram has both kinds it is
/// the web apps that speak, in health and in failure alike.
/// </summary>
public class RuntimeDatabaseReachedTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private static readonly Uri Page = new("http://localhost:5210/");

    private static readonly Topology SampleTopology =
        TopologyParser.Parse(WebAppDiagram.Topology).Topology!;

    /// <summary>uat as the deployment draws it: two web apps, a Front Door endpoint, the database, the static site.</summary>
    private static RuntimeManifest WebApps() =>
        RuntimeManifestParser.ParseManifest(WebAppDiagram.Sample("uat.json")).Value!;

    /// <summary>uat of the shipped sample diagram: a container app and the database, no web app.</summary>
    private static RuntimeManifest Shipped() =>
        RuntimeManifestParser.ParseManifest(RuntimeManifestParserTests.Sample("uat.json")).Value!;

    private static EnvironmentStatus Uat(int primary = 200, int standby = 200)
    {
        var uat = new EnvironmentStatus(SampleTopology.Environments.Single(environment => environment.Name == "uat"));
        var ui = uat.Deployables[0];
        ui.FrontDoor!.Record(new ProbeResult(HealthClassifier.FromStatusCode(200), 200, 40, Now, "2.4.21"));
        ui.Nodes[0].Record(new ProbeResult(HealthClassifier.FromStatusCode(primary), primary, 40, Now, "2.4.21"));
        ui.Nodes[1].Record(
            standby == 0
                ? new ProbeResult(HealthState.Unreachable, null, null, Now, null, "No answer within 10 s.")
                : new ProbeResult(HealthClassifier.FromStatusCode(standby), standby, 40, Now, "2.4.20"));
        return uat;
    }

    /// <summary>A container app's probe, answered healthily at the moment given.</summary>
    private static ContainerAppStatus ProbeAt(DateTimeOffset at) =>
        new(ContainerAppState.Healthy, new ContainerAppAnswer("/_healthcheck", true, 200, false, 40, at));

    /// <summary>The database's tile of a payload.</summary>
    private static RuntimeTile Database(RuntimePayload payload) => payload.Nodes.Single(tile => tile.Alias == "sqldb");

    /// <summary>The names the reachable database's title counts, between "the health check of" and "connected to it".</summary>
    private static string Named(RuntimeTile tile)
    {
        var from = tile.Title.IndexOf("the health check of ", StringComparison.Ordinal) + "the health check of ".Length;
        return tile.Title[from..tile.Title.IndexOf(" connected to it", StringComparison.Ordinal)];
    }

    [Fact]
    public void AWebAppReadingNamesItsTargetInTheTitleAndItsLabelOnTheLine()
    {
        // One web app answering: the line shows the label a reader of the diagram knows it by, the title the address.
        var database = Database(RuntimePayloadBuilder.Build(WebApps(), Uat(standby: 0), Page, TimeZoneInfo.Utc));

        Assert.Equal(("healthy", "Reachable"), (database.State, database.Label));
        Assert.Equal(new RuntimeTileLine("health check of westus3 passed", "ok"), Assert.Single(database.Lines));
        Assert.Equal("app-cmdemo2-uat-ui", Named(database));
        Assert.DoesNotContain("westus3", database.Title, StringComparison.Ordinal);
    }

    [Fact]
    public void AContainerAppReadingNamesItsNodeOnTheLineAndInTheTitleAlike()
    {
        // A container app has no label apart from its name, so the one reading gives the same name to both its words.
        var database = Database(
            RuntimePayloadBuilder.Build(Shipped(), null, Page, TimeZoneInfo.Utc, containerApps: _ => ProbeAt(Now)));

        Assert.Equal(new RuntimeTileLine("health check of ca-cmdemo1-uat-ui passed", "ok"), Assert.Single(database.Lines));
        Assert.Equal("ca-cmdemo1-uat-ui", Named(database));
    }

    /// <summary>
    /// The "last" the title says is the latest of the readings, whichever source they came from: here two container
    /// apps probed half a minute apart.
    /// </summary>
    [Fact]
    public void TheReachableDatabaseIsSaidToHaveBeenCheckedWhenTheLatestOfItsAppsWas()
    {
        var manifest = TwoContainerApps();

        var database = Database(
            RuntimePayloadBuilder.Build(
                manifest,
                null,
                Page,
                TimeZoneInfo.Utc,
                containerApps: target => ProbeAt(target.Url.Host == "ca-ui" ? Now : Now.AddSeconds(45))));

        Assert.Equal(new RuntimeTileLine("health checks of 2 container apps passed", "ok"), Assert.Single(database.Lines));
        Assert.Equal("ca-ui, ca-worker", Named(database));
        Assert.Contains("connected to it (last 22:00:45)", database.Title, StringComparison.Ordinal);
    }

    /// <summary>
    /// Where a diagram has both kinds of app, the web apps speak for the database and the container apps are not
    /// asked, even when their own probe is healthy too.
    /// </summary>
    [Fact]
    public void WhereTheDiagramHasWebAppsTheyAreTheOnesThatSayTheDatabaseIsReachable()
    {
        var payload = RuntimePayloadBuilder.Build(
            WebAppsWithAContainerApp(),
            Uat(standby: 0),
            Page,
            TimeZoneInfo.Utc,
            containerApps: _ => ProbeAt(Now));

        var database = Database(payload);

        Assert.Equal(("healthy", "Reachable"), (database.State, database.Label));
        Assert.Equal(new RuntimeTileLine("health check of westus3 passed", "ok"), Assert.Single(database.Lines));
        Assert.Equal("app-cmdemo2-uat-ui", Named(database));
        Assert.DoesNotContain("ca-scorecard", payload.Nodes.Single(tile => tile.Alias == "sqldb").Title, StringComparison.Ordinal);
    }

    /// <summary>
    /// The same choice when the web apps fail: a healthy container app does not overrule them, because the web apps
    /// are the source - the tile is the web apps' "not confirmed", not the container apps' reachable one.
    /// </summary>
    [Fact]
    public void WhereTheDiagramHasWebAppsAHealthyContainerAppDoesNotOverruleAFailedWebApp()
    {
        var database = Database(
            RuntimePayloadBuilder.Build(
                WebAppsWithAContainerApp(),
                Uat(primary: 503, standby: 0),
                Page,
                TimeZoneInfo.Utc,
                containerApps: _ => ProbeAt(Now)));

        Assert.Equal(("neutral", "Not confirmed"), (database.State, database.Label));
        Assert.Equal(new RuntimeTileLine("no health check of its apps passes", "muted"), Assert.Single(database.Lines));
        Assert.Contains("no web app that uses it passes its health check", database.Title, StringComparison.Ordinal);
        Assert.DoesNotContain("ca-scorecard", database.Title, StringComparison.Ordinal);
    }

    /// <summary>
    /// The shape the issue asked for (#1750): each source is projected once, both hand their readings to one
    /// reachable branch, and that branch is the only one that says a reached database in words.
    /// </summary>
    [Fact]
    public void EachSourceIsReadOnceAndBothHandTheirReadingsToOneReachableBranch()
    {
        var source = File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("Runtime", "RuntimePayloadBuilder.cs")));
        Assert.True(source.Length > 20_000, "the builder read empty: the guard is checking nothing");

        // One reading per source, asked of each source's list once.
        Assert.Equal(2, MarkupGuards.CountOccurrences(source, "private static Reached? ReachedOf("));
        Assert.Equal(2, MarkupGuards.CountOccurrences(source, ".Select(ReachedOf)"));

        // One reachable branch, called by both sources; its sentence is written once, and said nowhere else.
        Assert.Equal(1, MarkupGuards.CountOccurrences(source, "private static RuntimeTile DatabaseReached("));
        Assert.Equal(2, MarkupGuards.CountOccurrences(source, "return DatabaseReached(node, reached, PassedLine("));
        Assert.Equal(2, MarkupGuards.CountOccurrences(source, "DatabaseReachedTitle("));
        Assert.Equal(1, MarkupGuards.CountOccurrences(source, "reachable. The database takes no call from a browser"));
    }

    /// <summary>A diagram of two container apps, each with its arrow to the database.</summary>
    private static RuntimeManifest TwoContainerApps() =>
        new(
            "uat",
            [
                new RuntimeNode("ca_ui", RuntimeNodeKind.ContainerApp, "ca-ui", new Uri("https://ca-ui.example.io"), "ui", HealthPath: "/_healthcheck"),
                new RuntimeNode("ca_worker", RuntimeNodeKind.ContainerApp, "ca-worker", new Uri("https://ca-worker.example.io"), "worker", HealthPath: "/_healthcheck"),
                new RuntimeNode("sqldb", RuntimeNodeKind.Sql, "sqldb-uat"),
            ],
            [],
            [
                new RuntimeEdge("ca_ui-to-sqldb", "ca_ui", "sqldb", RuntimeEdgeKind.Sql),
                new RuntimeEdge("ca_worker-to-sqldb", "ca_worker", "sqldb", RuntimeEdgeKind.Sql),
            ]);

    /// <summary>The web apps' uat diagram with a container app added, also using the database.</summary>
    private static RuntimeManifest WebAppsWithAContainerApp()
    {
        var manifest = WebApps();
        return manifest with
        {
            Nodes =
            [
                .. manifest.Nodes,
                new RuntimeNode("ca_scorecard", RuntimeNodeKind.ContainerApp, "ca-scorecard", new Uri("https://ca-scorecard.example.io"), "scorecard", HealthPath: "/_healthcheck"),
            ],
            Edges = [.. manifest.Edges, new RuntimeEdge("ca_scorecard-to-sqldb", "ca_scorecard", "sqldb", RuntimeEdgeKind.Sql)],
        };
    }
}
