namespace Dashboard.Tests;

/// <summary>
/// The tile of a node the monitor has no entry for, one test per arm of the switch at the end of
/// <c>RuntimePayloadBuilder.Tile</c>: a static site that is not this page (without an address, with one, and with
/// Resource Health), a Front Door endpoint without an address, a web app or Front Door the topology does not have, a
/// node of another kind with an address, and the fall-through. Driven only through <see cref="RuntimeManifestParser"/>
/// and <see cref="RuntimePayloadBuilder.Build"/>, so the tiles are pinned whatever the builder's methods are called.
/// </summary>
public class RuntimeUnmonitoredTileTests
{
    private static readonly Uri Page = new("http://localhost:5210/");
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private const string VaultId = "/subscriptions/0000/resourceGroups/rg-demo-uat/providers/Microsoft.KeyVault/vaults/kv-demo";

    [Fact]
    public void AStaticSiteWithoutAnAddressSaysItsAddressIsNotInThisDeployment()
    {
        var tile = TileOf("""{ "alias": "swa", "kind": "staticsite", "name": "swa-uat-dashboard" }""");

        Assert.Equal(("neutral", "Not probed"), (tile.State, tile.Label));
        Assert.Equal([new RuntimeTileLine("its address is not in this deployment", "muted")], tile.Lines);
        Assert.Equal("swa-uat-dashboard: the dashboard of this environment. This page does not check it.", tile.Title);
    }

    [Fact]
    public void AStaticSiteWithAnAddressThatIsNotThisPageShowsItsHost()
    {
        var tile = TileOf("""{ "alias": "swa", "kind": "staticsite", "name": "swa-uat-dashboard", "url": "https://dashboard.uat.example.net" }""");

        Assert.Equal(("neutral", "Not probed"), (tile.State, tile.Label));
        Assert.Equal([new RuntimeTileLine("dashboard.uat.example.net", "muted")], tile.Lines);
        Assert.Equal("swa-uat-dashboard: the dashboard of this environment (dashboard.uat.example.net). This page does not check it.", tile.Title);
    }

    [Fact]
    public void AStaticSiteWithResourceHealthSaysWhatItSaidAndWhyThisPageDoesNotCheckIt()
    {
        var tile = TileOf($$"""{ "alias": "swa", "kind": "staticsite", "name": "swa-uat-dashboard", "url": "https://dashboard.uat.example.net", "ids": ["{{VaultId}}"] }""", Readings());
        var bare = TileOf($$"""{ "alias": "swa", "kind": "staticsite", "name": "swa-uat-dashboard", "ids": ["{{VaultId}}"] }""", Readings());

        Assert.Equal(("neutral", "Available"), (tile.State, tile.Label));
        Assert.Equal(["Resource Health, 40 min ago"], tile.Lines.Select(line => line.Text));
        Assert.StartsWith("swa-uat-dashboard: the dashboard of this environment (dashboard.uat.example.net). This page does not check it.\n", tile.Title, StringComparison.Ordinal);
        Assert.StartsWith("swa-uat-dashboard: the dashboard of this environment. This page does not check it.\n", bare.Title, StringComparison.Ordinal);
    }

    [Fact]
    public void AFrontDoorEndpointWithoutAnAddressIsNotDeployedYet()
    {
        var tile = TileOf("""{ "alias": "fd", "kind": "frontdoor", "name": "fd-uat-ui", "deployable": "ui" }""");

        Assert.Equal(("neutral", "No address"), (tile.State, tile.Label));
        Assert.Equal([new RuntimeTileLine("endpoint not deployed yet", "muted")], tile.Lines);
        Assert.Equal("fd-uat-ui: the deployment found no such endpoint in the Front Door profile. Deploy the dashboard again once the environment has it.", tile.Title);
    }

    [Theory]
    [InlineData("webapp", "app-uat-ui-westeurope", "https://app-uat-ui-westeurope.azurewebsites.net")]
    [InlineData("frontdoor", "fd-uat-ui", "https://fd-uat-ui.z01.azurefd.net")]
    public void AnAddressTheTopologyDoesNotHaveIsNotChecked(string kind, string name, string url)
    {
        var tile = TileOf($$"""{ "alias": "n", "kind": "{{kind}}", "name": "{{name}}", "url": "{{url}}" }""");

        Assert.Equal(("neutral", "Not checked"), (tile.State, tile.Label));
        Assert.Equal([new RuntimeTileLine("not in topology.json", "muted")], tile.Lines);
        Assert.Equal($"{name}: the topology this page loaded has no such address, so it is not checked. Reload the topology, or deploy the dashboard again.", tile.Title);
    }

    [Fact]
    public void AWebAppWithoutAnAddressIsNotCheckedEither()
    {
        var tile = TileOf("""{ "alias": "n", "kind": "webapp", "name": "app-uat-ui" }""");

        Assert.Equal(("neutral", "Not checked"), (tile.State, tile.Label));
        Assert.Equal([new RuntimeTileLine("not in topology.json", "muted")], tile.Lines);
    }

    [Fact]
    public void ANodeOfAnotherKindWithAnAddressIsDrawnWithItsHost()
    {
        var tile = TileOf("""{ "alias": "n", "kind": "gateway", "name": "ai-gateway", "url": "https://ai.example.net" }""");

        Assert.Equal(("neutral", "Not probed"), (tile.State, tile.Label));
        Assert.Equal([new RuntimeTileLine("ai.example.net", "muted")], tile.Lines);
        Assert.Equal("ai-gateway (ai.example.net): the dashboard does not check it.", tile.Title);
    }

    [Fact]
    public void ANodeOfAnotherKindWithoutAnAddressFallsThroughToNotProbedUnderItsName()
    {
        var tile = TileOf("""{ "alias": "n", "kind": "gateway", "name": "ai-gateway" }""");

        Assert.Equal(("neutral", "Not probed"), (tile.State, tile.Label));
        Assert.Empty(tile.Lines);
        Assert.Equal("ai-gateway", tile.Title);
    }

    [Fact]
    public void ANodeOfAnotherKindWithoutAnAddressSaysWhatResourceHealthSaid()
    {
        var tile = TileOf($$"""{ "alias": "n", "kind": "gateway", "name": "ai-gateway", "ids": ["{{VaultId}}"] }""", Readings());

        Assert.Equal(("neutral", "Available"), (tile.State, tile.Label));
        Assert.Equal(["Resource Health, 40 min ago"], tile.Lines.Select(line => line.Text));
        Assert.StartsWith("ai-gateway\n", tile.Title, StringComparison.Ordinal);
    }

    /// <summary>The tile of the one node of a manifest, built with no topology entry for it.</summary>
    private static RuntimeTile TileOf(string node, ResourceHealthReport? resources = null)
    {
        var parsed = RuntimeManifestParser.ParseManifest($$"""{ "environment": "uat", "nodes": [ {{node}} ], "edges": [] }""");
        Assert.True(parsed.IsValid);
        var payload = RuntimePayloadBuilder.Build(parsed.Value!, null, Page, TimeZoneInfo.Utc, now: Now, resources: resources);
        return Assert.Single(payload.Nodes);
    }

    /// <summary>What Resource Health said of a vault, read 40 minutes ago.</summary>
    private static ResourceHealthReport Readings() => ResourceHealthReport.Parse($$"""
        { "generated": "2026-10-04T21:20:00Z", "resources": { "nonprod": [
            { "id": "{{VaultId}}", "health": { "status": "available", "source": "resourceHealth", "checkedAt": "2026-10-04T21:20:00Z", "summary": "There aren't any known Azure platform problems affecting this key vault." } } ] } }
        """)!;
}
