using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// Characterization of the node-kind switch in <see cref="RuntimePayloadBuilder.Build"/> and the one fallback branch of its
/// <c>Tile</c> helper the other tests do not pin: each test pins the exact <see cref="RuntimeTile"/> a branch produces, built
/// only through <see cref="RuntimeManifestParser"/> and <see cref="RuntimePayloadBuilder.Build"/> with the same WebAppDiagram
/// fixtures as <see cref="RuntimePayloadEdgeRegionTests"/>. Step 1 of #1456: pin the behavior before the builder is split, so
/// the split can be checked against it.
/// <para>
/// The other fallback branches of <c>Tile</c> (an unchecked static site, a Front Door endpoint without an address, an address
/// the topology does not have, a node of another kind with an address, and the Resource Health fallback) are pinned in
/// <see cref="RuntimeUnmonitoredTileTests"/> and <see cref="RuntimePayloadUnprobedNodeTests"/>, and the words of the Sql,
/// ContainerApp and Resource tiles are pinned in <see cref="RuntimePayloadBuilderTests"/>; those tests leave the tiles' link
/// and shape fields (Link, NameLink, Deployment, History) loose, and this file pins them. The tile's words are compared field
/// by field (its lines as a sequence, its links by their own words), because a record's list field compares by reference, not
/// by content.
/// </para>
/// </summary>
public class RuntimeTileCharacterizationTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private static readonly Uri Page = new("http://localhost:5210/");

    private static readonly Topology SampleTopology =
        TopologyParser.Parse(WebAppDiagram.Topology).Topology!;

    private static RuntimeManifest Manifest(string environment) =>
        RuntimeManifestParser.ParseManifest(WebAppDiagram.Sample($"{environment}.json")).Value!;

    private static EnvironmentStatus Environment(string name) =>
        new(SampleTopology.Environments.Single(environment => environment.Name == name));

    private static ProbeResult Answer(int status, string version = "2.4.21") =>
        new(HealthClassifier.FromStatusCode(status), status, 40, Now, version);

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

    /// <summary>uat with Front Door and primary answering; <paramref name="standby"/> answers too unless it is 0.</summary>
    private static EnvironmentStatus Uat(int standby = 200)
    {
        var uat = Environment("uat");
        uat.Deployables[0].FrontDoor!.Record(Answer(200));
        uat.Deployables[0].Nodes[0].Record(Answer(200));
        uat.Deployables[0].Nodes[1].Record(standby == 0
            ? new ProbeResult(HealthState.Unreachable, null, null, Now, null, "No answer within 10 s.")
            : Answer(standby, "2.4.20"));
        return uat;
    }

    /// <summary>
    /// The <c>Sql</c> arm of the node-kind switch: the database a passing web app health check reached. The arm's words are
    /// pinned in <see cref="RuntimePayloadBuilderTests"/>; this pins the database's own name link, which those tests leave
    /// loose - the name leads to the database link the topology writes for the environment, not the node.
    /// </summary>
    [Fact]
    public void TheSqlArmOfTheNodeKindSwitchLeadsTheDatabasesNameToTheTopologiesDatabaseLink()
    {
        var manifest = Manifest("uat");
        var environment = Uat(standby: 0); // only the primary answers, so one web app names the database
        var node = manifest.Nodes.Single(n => n.Alias == "sqldb");
        var tile = RuntimePayloadBuilder.Build(manifest, environment, Page, TimeZoneInfo.Utc).Nodes.Single(n => n.Alias == "sqldb");

        Words(tile, "healthy", "Reachable", null, [("health check of westus3 passed", "ok")]);
        Assert.Equal(
            "sqldb-cmdemo2-uat: reachable. The database takes no call from a browser; the health check of app-cmdemo2-uat-ui connected to it (last 22:00:00).",
            tile.Title);
        Assert.Null(tile.Link);
        Assert.Equal(RuntimeLink.To(environment.Info.Links![LinkSet.Database], LinkText.For(LinkSet.Database, node.Name)), tile.NameLink);
        Assert.Null(tile.Deployment);
    }

    /// <summary>
    /// The <c>Dependency</c> arm of the node-kind switch, where no web app the page checks uses the dependency: the arm's
    /// not-probed tile, pinned in full - no skip-listed test pins this arm's not-probed words or its links. (The arm's other
    /// branches - reachable, not known, not confirmed, waiting - are pinned in <see cref="DependencyTests"/> and
    /// <see cref="RuntimePayloadWaitingTileTests"/>.)
    /// </summary>
    [Fact]
    public void TheDependencyArmOfTheNodeKindSwitchReturnsNotProbedWhenNoCheckedWebAppUsesIt()
    {
        var mail = new RuntimeNode("dep_mail", RuntimeNodeKind.Dependency, "Mail", null, "Mail");
        var manifest = Manifest("uat") with { Nodes = [.. Manifest("uat").Nodes, mail] };
        var tile = RuntimePayloadBuilder.Build(manifest, Uat(), Page, TimeZoneInfo.Utc).Nodes.Single(n => n.Alias == "dep_mail");

        Words(tile, "neutral", "Not probed", null, [("no web app that uses it is checked", "muted")]);
        Assert.Equal("Mail: this page checks no web app that uses it.", tile.Title);
        Unlinked(tile);
    }

    /// <summary>
    /// The <c>ContainerApp</c> arm of the node-kind switch, before the page has asked the app: the arm's probing tile carries
    /// no links and no deployment mark - the fields <see cref="RuntimePayloadBuilderTests"/> leaves loose when it pins the
    /// tile's words.
    /// </summary>
    [Fact]
    public void TheContainerAppArmOfTheNodeKindSwitchCarriesNoLinksOrMarkBeforeThePageAsks()
    {
        const string Host = "ca-demo-uat-scorecard.graybush-1.southcentralus.azurecontainerapps.io";
        var app = new RuntimeNode("ca_scorecard", RuntimeNodeKind.ContainerApp, "ca-demo-uat-scorecard", new Uri($"https://{Host}"), "scorecard");
        var manifest = Manifest("uat") with { Nodes = [.. Manifest("uat").Nodes, app] };
        var tile = RuntimePayloadBuilder.Build(manifest, Uat(), Page, TimeZoneInfo.Utc).Nodes.Single(n => n.Alias == "ca_scorecard");

        Words(tile, "checking", "Probing", "not probed yet", [(Host, "muted")]);
        Assert.StartsWith($"ca-demo-uat-scorecard ({Host}): Probing.", tile.Title, StringComparison.Ordinal);
        Unlinked(tile);
    }

    /// <summary>
    /// The <c>Resource</c> arm of the node-kind switch: a box of the system's resources says what Resource Health last said
    /// of the resources it stands for, in neutral words, and carries no links and no deployment mark - the fields
    /// <see cref="RuntimePayloadBuilderTests"/> leaves loose when it pins the tile's words.
    /// </summary>
    [Fact]
    public void TheResourceArmOfTheNodeKindSwitchCarriesNoLinksOrMarkAroundTheReading()
    {
        const string VaultId = "/subscriptions/0000/resourceGroups/rg-demo-uat/providers/Microsoft.KeyVault/vaults/kv-demo";
        var vault = new RuntimeNode("res_1", RuntimeNodeKind.Resource, "kv-demo", Ids: [VaultId]);
        var manifest = Manifest("uat") with { Nodes = [.. Manifest("uat").Nodes, vault] };
        var readings = ResourceHealthReport.Parse($$"""
            { "generated": "2026-10-04T21:20:00Z", "resources": { "nonprod": [
              { "id": "{{VaultId}}", "health": { "status": "available", "source": "resourceHealth", "checkedAt": "2026-10-04T21:20:00Z", "summary": "There aren't any known Azure platform problems affecting this key vault." } } ] } }
            """)!;
        var tile = RuntimePayloadBuilder.Build(manifest, Uat(), Page, TimeZoneInfo.Utc, now: Now, resources: readings).Nodes.Single(n => n.Alias == "res_1");

        Words(tile, "neutral", "Available", null, [("Resource Health, 40 min ago", "plain")]);
        Assert.StartsWith("kv-demo\n", tile.Title, StringComparison.Ordinal);
        Assert.Contains("available (by Resource Health), read 2026-10-04 21:20 +00:00 (40 min ago)", tile.Title, StringComparison.Ordinal);
        Unlinked(tile);
    }

    /// <summary>
    /// The default arm of the node-kind switch is the <c>Tile</c> helper, and its same-site branch: the static site that
    /// serves this very dashboard says so, even where the topology lists it as a node. The existing tests pin only its label;
    /// this pins the tile's state, line, facts, history, title and its name link.
    /// </summary>
    [Fact]
    public void TheDefaultArmSameSiteStaticSiteSaysItServesThisDashboard()
    {
        // The sample tdd dashboard is http://localhost:5210, the page of the build: same host and port as Page.
        var manifest = Manifest("tdd");
        var node = manifest.Nodes.Single(n => n.Alias == "swa_dashboard");
        Assert.Equal(Page.Host, node.Url!.Host);
        Assert.Equal(Page.Port, node.Url.Port);

        var tile = RuntimePayloadBuilder.Build(manifest, Environment("tdd"), Page, TimeZoneInfo.Utc).Nodes.Single(n => n.Alias == "swa_dashboard");

        Words(tile, "neutral", "This page", null, [("serves this dashboard", "muted")]);
        Assert.Equal("swa-cmdemo2-tdd-dashboard (localhost) serves the page you are looking at.", tile.Title);
        Assert.Null(tile.Link);
        Assert.Null(tile.Deployment);
        // The name of the site leads to the portal link the manifest writes for it.
        Assert.NotNull(tile.NameLink);
        Assert.StartsWith("https://portal.azure.com/", tile.NameLink!.Href, StringComparison.Ordinal);
        Assert.Contains("The Static Web App swa-cmdemo2-tdd-dashboard in the Azure portal", tile.NameLink.Title, StringComparison.Ordinal);
    }
}
