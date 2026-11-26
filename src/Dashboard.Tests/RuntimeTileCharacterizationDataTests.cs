using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// Characterization of the four data tiles of <see cref="RuntimePayloadBuilder.Build"/>, in the style of step 1
/// (<see cref="RuntimeTileCharacterizationTests"/>): each test pins the exact <see cref="RuntimeTile"/> a branch produces, built
/// only through <see cref="RuntimeManifestParser"/> and <see cref="RuntimePayloadBuilder.Build"/> with the same fixtures. Step 2
/// of #1456: pin the behavior before the builder is split, so the split can be checked against it.
/// <para>
/// Left out, because another test already pins the whole tile: the three "waiting for the health checks" exits
/// (<see cref="RuntimePayloadWaitingTileTests"/>), the Sql arm's web apps' reached exit and the ContainerApp arm before the page
/// has asked (step 1). The words <see cref="DependencyTests"/> and <see cref="RuntimePayloadBuilderTests"/> pin while leaving the
/// tile's link and shape fields loose are pinned here in full, words and links together.
/// </para>
/// </summary>
public class RuntimeTileCharacterizationDataTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private static readonly Uri Page = new("http://localhost:5210/");

    private static readonly Topology SampleTopology =
        TopologyParser.Parse(WebAppDiagram.Topology).Topology!;

    private static readonly RuntimeManifest Sample =
        RuntimeManifestParser.ParseManifest(WebAppDiagram.Sample("uat.json")).Value!;

    private static readonly RuntimeManifest Shipped =
        RuntimeManifestParser.ParseManifest(RuntimeManifestParserTests.Sample("uat.json")).Value!;

    private static readonly string[] WebApps = ["app_ui_primary", "app_ui_standby"];

    private static ProbeResult Answer(int status, ProbeKind probe = ProbeKind.Health) =>
        new(HealthClassifier.FromStatusCode(status), status, 40, Now, "2.4.21", Probe: probe);

    private static readonly ProbeResult NoAnswer = new(HealthState.Unreachable, null, null, Now, null, "No answer within 10 s.");

    /// <summary>Pins the words of a tile: the state, label, facts, lines (as a sequence) and history.</summary>
    private static void Words(RuntimeTile tile, string state, string label, string? facts, (string Text, string Tone)[] lines)
    {
        Assert.Equal(state, tile.State);
        Assert.Equal(label, tile.Label);
        Assert.Equal(facts, tile.Facts);
        Assert.Equal(lines.Select(line => line.Text), tile.Lines.Select(line => line.Text));
        Assert.Equal(lines.Select(line => line.Tone), tile.Lines.Select(line => line.Tone));
        Assert.Null(tile.History);
    }

    /// <summary>Pins the tile's links and deployment mark: none, for a tile this page does not check or lead anywhere.</summary>
    private static void Unlinked(RuntimeTile tile)
    {
        Assert.Null(tile.Link);
        Assert.Null(tile.NameLink);
        Assert.Null(tile.Deployment);
    }

    /// <summary>
    /// uat as the health checks read it: the HTTP status each endpoint answers (0: no answer at all), probed with
    /// <paramref name="probe"/>, and what the detailed health check says of LlmGateway (null: no detailed health check).
    /// </summary>
    private static EnvironmentStatus Uat(int frontDoor = 200, int primary = 200, int standby = 200, string? primaryCheck = "Healthy", string? standbyCheck = "Healthy", ProbeKind probe = ProbeKind.Health)
    {
        var uat = new EnvironmentStatus(SampleTopology.Environments.Single(environment => environment.Name == "uat"));
        var ui = uat.Deployables[0];
        ui.FrontDoor!.Record(frontDoor == 0 ? NoAnswer : Answer(frontDoor, probe));
        ui.Nodes[0].Record(primary == 0 ? NoAnswer : Answer(primary, probe));
        ui.Nodes[1].Record(standby == 0 ? NoAnswer : Answer(standby, probe));
        ui.Nodes[0].RecordHealthDetail(primaryCheck is null ? null : HealthDetailTests.Detail(("API", "Healthy"), ("LlmGateway", primaryCheck)));
        ui.Nodes[1].RecordHealthDetail(standbyCheck is null ? null : HealthDetailTests.Detail(("API", "Healthy"), ("LlmGateway", standbyCheck)));
        return uat;
    }

    /// <summary>The uat manifest with one dependency node of ui and a relationship from each web app to it.</summary>
    private static RuntimeManifest WithDependency(string alias, string name, string? healthCheck) => Sample with
    {
        Nodes = [.. Sample.Nodes, new RuntimeNode(alias, RuntimeNodeKind.Dependency, name, Deployable: "ui", HealthCheck: healthCheck, DependencyKind: "external")],
        Edges =
        [
            .. Sample.Edges,
            .. from app in WebApps select new RuntimeEdge($"{alias}-to-{app}", app, alias, RuntimeEdgeKind.Dependency),
        ],
    };

    private static RuntimeTile Dependency(EnvironmentStatus environment, string? healthCheck = "LlmGateway") =>
        RuntimePayloadBuilder.Build(WithDependency("dep_ui_LLM_gateway", "LLM gateway", healthCheck), environment, Page, TimeZoneInfo.Utc)
            .Nodes.Single(tile => tile.Alias == "dep_ui_LLM_gateway");

    /// <summary>The database tile of the uat manifest, with its node and the environment's database link its name leads to.</summary>
    private static (RuntimeTile Tile, RuntimeNode Node, Uri? DatabaseLink) Database(EnvironmentStatus environment, Func<RuntimeManifest, RuntimeManifest>? manifest = null)
    {
        var node = Sample.Nodes.Single(n => n.Alias == "sqldb");
        var built = RuntimePayloadBuilder.Build(manifest?.Invoke(Sample) ?? Sample, environment, Page, TimeZoneInfo.Utc);
        return (built.Nodes.Single(n => n.Alias == "sqldb"), node, environment.Info.Links?[LinkSet.Database]);
    }

    /// <summary>Pins the name link the Sql arm's wrapper adds: the environment's database link, with its words.</summary>
    private static void DatabaseNameLink(RuntimeTile tile, RuntimeNode node, Uri? databaseLink)
    {
        Assert.Null(tile.Link);
        Assert.Equal(RuntimeLink.To(databaseLink, LinkText.For(LinkSet.Database, node.Name)), tile.NameLink);
        Assert.Null(tile.Deployment);
    }

    /// <summary>The <see cref="RuntimeNodeKind.Dependency"/> arm where the node names no health check: pinned in full.</summary>
    [Fact]
    public void ADependencyThatNamesNoHealthCheckIsNotProbedWithTheWholeTile()
    {
        var tile = Dependency(Uat(), healthCheck: null);

        Words(tile, "neutral", "Not probed", null, [("no health check names it", "muted")]);
        Assert.Equal(
            "LLM gateway: the system names no entry of the web apps' detailed health check for it (dependencies[].healthCheck in system.json), so this page cannot tell its state.",
            tile.Title);
        Unlinked(tile);
    }

    /// <summary>The <see cref="RuntimeNodeKind.Dependency"/> arm's liveness exit, pinned in full: a Liveness probe reads none of
    /// what the detailed health checks found, so the tile cannot tell.</summary>
    [Fact]
    public void TheLivenessProbeLeavesTheDependencyAloneWithTheWholeTile()
    {
        var tile = Dependency(Uat(primaryCheck: null, standbyCheck: null, probe: ProbeKind.Liveness));

        Words(tile, "neutral", "Not probed", null, [("probe Liveness leaves it alone", "muted")]);
        Assert.Equal(
            "LLM gateway: the probe is Liveness, which does not read what the web apps' health checks found. Choose Health check to see its state.",
            tile.Title);
        Unlinked(tile);
    }

    /// <summary>
    /// The <see cref="RuntimeNodeKind.Dependency"/> arm's exit for an entry whose state this dashboard does not know
    /// (<see cref="CheckState.Unknown"/>): pinned in full, the one exit no other test reaches.
    /// </summary>
    [Fact]
    public void AnEntryInAStateThePageDoesNotKnowLeavesTheDependencyNotKnownWithTheWholeTile()
    {
        // "Paused" is no state this dashboard knows, so the entry reads Unknown.
        var tile = Dependency(Uat(primaryCheck: "Paused", standbyCheck: "Paused"));

        Words(tile, "neutral", "Not known", null, [("LlmGateway says paused", "muted")]);
        Assert.Equal(
            "LLM gateway: by the health check of app-cmdemo2-uat-ui (last 22:00:00). The browser does not call it. LlmGateway: Not known. LlmGateway says paused. Took 1.5 ms.",
            tile.Title);
        Unlinked(tile);
    }

    /// <summary>
    /// The <see cref="RuntimeNodeKind.Dependency"/> arm's not-confirmed exit: the entry is healthy, but no web app that reports
    /// it passes its own health check. <see cref="DependencyTests"/> pins its state, label and line; this pins the rest.
    /// </summary>
    [Fact]
    public void AHealthyEntryNoWebAppPassesIsNotConfirmedWithTheWholeTile()
    {
        var tile = Dependency(Uat(standbyCheck: null, primary: 503));

        Words(tile, "neutral", "Not confirmed", null, [("no web app that reports it passes", "muted")]);
        Assert.Equal(
            "LLM gateway: the entry LlmGateway is healthy, but no web app that reports it passes its own health check, so this page does not call it reachable.",
            tile.Title);
        Unlinked(tile);
    }

    /// <summary>
    /// The <see cref="RuntimeNodeKind.Dependency"/> arm's reachable exit, one web app reaching it and the other reporting no
    /// detailed health check: <see cref="DependencyTests"/> pins the words of the other exits; this pins this title in full.
    /// </summary>
    [Fact]
    public void ADependencyOneWebAppReachesIsReachableWithTheWholeTile()
    {
        var tile = Dependency(Uat(standbyCheck: null));

        Words(tile, "healthy", "Reachable", null, [("LlmGateway says healthy", "ok")]);
        Assert.Equal(
            "LLM gateway: reachable, by the health check of app-cmdemo2-uat-ui (last 22:00:00). The browser does not call it. LlmGateway: Healthy. LlmGateway says healthy. Took 1.5 ms.",
            tile.Title);
        Unlinked(tile);
    }

    /// <summary>
    /// The <see cref="RuntimeNodeKind.Sql"/> arm, web apps' exit, where no web app the page checks uses the database:
    /// <see cref="RuntimePayloadBuilderTests"/> pins the state, label and line; this pins the title and the name link the arm's
    /// wrapper adds from the environment's database link.
    /// </summary>
    [Fact]
    public void TheDatabaseNoCheckedWebAppUsesIsNotProbedFromTheBrowserWithTheWholeTile()
    {
        var (tile, node, databaseLink) = Database(Uat(), manifest: m => m with { Edges = [.. m.Edges.Where(edge => edge.Kind != RuntimeEdgeKind.Sql)] });

        Words(tile, "neutral", "Not probed", null, [("not probed from the browser", "muted")]);
        Assert.Equal("sqldb-cmdemo2-uat: the database takes no call from a browser, and this page checks no web app that uses it.", tile.Title);
        DatabaseNameLink(tile, node, databaseLink);
    }

    /// <summary>
    /// The <see cref="RuntimeNodeKind.Sql"/> arm, web apps' exit, where the web apps are probed for liveness only: a Liveness
    /// probe never connects to the database. Pinned in full: <see cref="RuntimePayloadBuilderTests"/> pins only the state, label
    /// and line.
    /// </summary>
    [Fact]
    public void TheLivenessProbeLeavesTheDatabaseAloneWithTheWholeTile()
    {
        var (tile, node, databaseLink) = Database(Uat(primaryCheck: null, standbyCheck: null, probe: ProbeKind.Liveness));

        Words(tile, "neutral", "Not probed", null, [("probe Liveness leaves it alone", "muted")]);
        Assert.Equal(
            "sqldb-cmdemo2-uat: the probe is Liveness, which does not connect to the database. Choose Health check to see whether it answers.",
            tile.Title);
        DatabaseNameLink(tile, node, databaseLink);
    }

    /// <summary>
    /// The <see cref="RuntimeNodeKind.Sql"/> arm, web apps' exit, where no web app that uses the database passes: a failed check
    /// may be the web app, not the database. <see cref="RuntimePayloadBuilderTests"/> pins the title; this pins the rest of the
    /// tile with it.
    /// </summary>
    [Fact]
    public void TheDatabaseNoWebAppPassesIsNotConfirmedWithTheWholeTile()
    {
        var (tile, node, databaseLink) = Database(Uat(frontDoor: 503, primary: 503, standby: 0, primaryCheck: null, standbyCheck: null));

        Words(tile, "neutral", "Not confirmed", null, [("no health check of its apps passes", "muted")]);
        Assert.Equal(
            "sqldb-cmdemo2-uat: no web app that uses it passes its health check, so this page cannot tell whether it answers: the database or the web app may be the cause.",
            tile.Title);
        DatabaseNameLink(tile, node, databaseLink);
    }

    /// <summary>What the page's probe last saw of a container app, one answer per state for the tiles below.</summary>
    private static ContainerAppStatus Probed(ContainerAppState state, int? status = null, bool opaque = false, DateTimeOffset? checkedAt = null) =>
        new(state, new ContainerAppAnswer(opaque || status is null ? "/" : "/health", status is not null, status, opaque, status is null && !opaque ? null : 85, checkedAt ?? Now.AddSeconds(-20), status is null && !opaque ? "No answer within 10 s." : null), opaque);

    /// <summary>
    /// The <see cref="RuntimeNodeKind.Sql"/> arm's container apps' exit, where a container app's healthy health-path probe
    /// connected to the database: the one exit no other test pins. Without an environment the page has not checked anything,
    /// and the diagram the page ships gives the database no name link.
    /// </summary>
    [Fact]
    public void AContainerAppsHealthyHealthPathProbeSaysTheDatabaseIsReachableWithTheWholeTile()
    {
        var tile = RuntimePayloadBuilder.Build(Shipped, null, Page, TimeZoneInfo.Utc, containerApps: _ => Probed(ContainerAppState.Healthy, 200, checkedAt: Now)).Nodes.Single(n => n.Alias == "sqldb");

        Words(tile, "healthy", "Reachable", null, [("health check of ca-cmdemo1-uat-ui passed", "ok")]);
        Assert.Equal(
            "sqldb-cmdemo1-uat: reachable. The database takes no call from a browser; the health check of ca-cmdemo1-uat-ui connected to it (last 22:00:00).",
            tile.Title);
        Unlinked(tile);
    }

    /// <summary>
    /// The container apps' exit, where every container app answered without a health check (a Liveness probe, or an app that
    /// refuses this page's origin): the answer is no proof the database answers, so the tile says the health check was not read.
    /// </summary>
    [Fact]
    public void ContainerAppsThatAnswerWithoutAHealthCheckLeaveTheDatabaseNotProbedWithTheWholeTile()
    {
        var tile = RuntimePayloadBuilder.Build(Shipped, null, Page, TimeZoneInfo.Utc, containerApps: _ => Probed(ContainerAppState.Responding, opaque: true)).Nodes.Single(n => n.Alias == "sqldb");

        Words(tile, "neutral", "Not probed", null, [("health check not read", "muted")]);
        Assert.Equal(
            "sqldb-cmdemo1-uat: the container app that uses it answered without a health check (the probe was Liveness, or the app does not allow this page's origin), so this page cannot tell whether the database answers.",
            tile.Title);
        Unlinked(tile);
    }

    /// <summary>
    /// The container apps' exit, where no container app that uses the database passes: <see cref="RuntimePayloadBuilderTests"/>
    /// pins the title; this pins the rest of the tile with it.
    /// </summary>
    [Fact]
    public void TheDatabaseNoContainerAppPassesIsNotConfirmedWithTheWholeTile()
    {
        var tile = RuntimePayloadBuilder.Build(Shipped, null, Page, TimeZoneInfo.Utc, containerApps: _ => Probed(ContainerAppState.Unhealthy, 503)).Nodes.Single(n => n.Alias == "sqldb");

        Words(tile, "neutral", "Not confirmed", null, [("no health check of its apps passes", "muted")]);
        Assert.Equal(
            "sqldb-cmdemo1-uat: no container app that uses it passes its health check, so this page cannot tell whether it answers: the database or the app may be the cause.",
            tile.Title);
        Unlinked(tile);
    }

    /// <summary>
    /// The <see cref="RuntimeNodeKind.ContainerApp"/> arm, the node without a public address: the browser cannot ask it, and the
    /// tile says so in full - <see cref="RuntimePayloadBuilderTests"/> pins only the state and the label.
    /// </summary>
    [Fact]
    public void AContainerAppWithoutAAddressIsNotProbedWithTheWholeTile()
    {
        var app = new RuntimeNode("ca_internal", RuntimeNodeKind.ContainerApp, "ca-internal");
        var tile = RuntimePayloadBuilder.Build(Sample with { Nodes = [.. Sample.Nodes, app] }, Uat(), Page, TimeZoneInfo.Utc).Nodes.Single(n => n.Alias == "ca_internal");

        Words(tile, "neutral", "No address", null, [("no public address known", "muted")]);
        Assert.Equal("ca-internal: a container app without a public address this page knows: the browser cannot ask it.", tile.Title);
        Unlinked(tile);
    }

    /// <summary>The host the container app answers from, in the tile's one line.</summary>
    private const string ContainerHost = "ca-demo-uat-scorecard.graybush-1.southcentralus.azurecontainerapps.io";

    /// <summary>
    /// The <see cref="RuntimeNodeKind.ContainerApp"/> arm, once the page has asked: the tile takes the probe's state in the
    /// colours of the other states. <see cref="RuntimePayloadBuilderTests"/> pins each arm's state, label and facts; this pins
    /// the arms' titles in full and their lines, history and links with them. <paramref name="tail"/> is the tooltip after the
    /// sentence of how often the page asks.
    /// </summary>
    [Theory]
    [InlineData(ContainerAppState.Waking, "checking", "Waking", "no answer yet", null, false,
        " Last probe 21:59:40: /, no answer. No answer within 10 s. An app that scaled to zero starts on the first request: the page waits up to 90 seconds for its first answer before it says Not responding.")]
    [InlineData(ContainerAppState.Responding, "healthy", "Responding", "answered · 85 ms", null, true,
        " Last probe 21:59:40: / answered. The app does not allow this page's origin (CORS), so the page cannot read its answer or its health; it asks without reading the answer. The page could not classify the answer as a health check.")]
    [InlineData(ContainerAppState.Healthy, "healthy", "Healthy", "HTTP 200 · 85 ms", 200, false,
        " Last probe 21:59:40: /health answered HTTP 200.")]
    [InlineData(ContainerAppState.Unhealthy, "unhealthy", "Unhealthy", "HTTP 503 · 85 ms", 503, false,
        " Last probe 21:59:40: /health answered HTTP 503.")]
    [InlineData(ContainerAppState.NotResponding, "unreachable", "Not responding", "no answer", null, false,
        " Last probe 21:59:40: /, no answer. No answer within 10 s.")]
    public void AContainerAppThePageHasAskedTakesTheProbesStateWithTheWholeTile(ContainerAppState state, string drawn, string label, string facts, int? status, bool opaque, string tail)
    {
        var app = new RuntimeNode("ca_scorecard", RuntimeNodeKind.ContainerApp, "ca-demo-uat-scorecard", new Uri($"https://{ContainerHost}"), "scorecard");
        var tile = RuntimePayloadBuilder.Build(Sample with { Nodes = [.. Sample.Nodes, app] }, Uat(), Page, TimeZoneInfo.Utc, containerApps: _ => Probed(state, status, opaque)).Nodes.Single(n => n.Alias == "ca_scorecard");

        Words(tile, drawn, label, facts, [(ContainerHost, "muted")]);
        Assert.Equal(
            $"ca-demo-uat-scorecard ({ContainerHost}): {label}. This page asks it once a minute while it shows this diagram, and not while it is paused or hidden; nothing asks it from here while no page is open, so it can scale to zero.{tail}",
            tile.Title);
        Unlinked(tile);
    }
}
