namespace Dashboard.Tests;

/// <summary>
/// The label of the grey tiles this page cannot fill, one branch apiece: a node of a kind the dashboard draws but does
/// not check, although the deployment gave it an address, and the static site of the environment that is not the page
/// being read. Neither branch had a test, and both write the label the builder keeps as its private <c>NotProbed</c>.
/// </summary>
public class RuntimePayloadNeutralLabelTests
{
    private static readonly Uri Page = new("http://localhost:5210/");

    [Fact]
    public void ANodeOfAKindThisPageDoesNotCheckIsDrawnWithItsAddressAndSaysItIsNotProbed()
    {
        var manifest = new RuntimeManifest(
            "uat",
            [
                new RuntimeNode("browser", RuntimeNodeKind.Person, "Browser"),
                new RuntimeNode("other_service", RuntimeNodeKind.Other, "ai-factory-gateway", new Uri("https://ai-factory.example.net")),
            ],
            [],
            [new RuntimeEdge("browser-to-other_service", "browser", "other_service", RuntimeEdgeKind.Other)]);

        var tile = Tile(RuntimePayloadBuilder.Build(manifest, null, Page, TimeZoneInfo.Utc), "other_service");

        Assert.Equal(("neutral", "Not probed"), (tile.State, tile.Label));
        Assert.Equal(new RuntimeTileLine("ai-factory.example.net", "muted"), Assert.Single(tile.Lines));
        Assert.Equal("ai-factory-gateway (ai-factory.example.net): the dashboard does not check it.", tile.Title);
    }

    [Fact]
    public void TheStaticSiteOfTheEnvironmentThatIsNotThisPageIsNotProbed()
    {
        var manifest = new RuntimeManifest(
            "uat",
            [
                new RuntimeNode("browser", RuntimeNodeKind.Person, "Browser"),
                new RuntimeNode("swa_dashboard", RuntimeNodeKind.StaticSite, "dashboard-uat", new Uri("https://dashboard.uat.example.net")),
            ],
            [],
            [new RuntimeEdge("browser-to-swa_dashboard", "browser", "swa_dashboard", RuntimeEdgeKind.Dashboard)]);

        // The page is read somewhere else, and the topology holds no deployable for the site, so nothing checks it.
        var tile = Tile(RuntimePayloadBuilder.Build(manifest, null, Page, TimeZoneInfo.Utc), "swa_dashboard");

        Assert.Equal(("neutral", "Not probed"), (tile.State, tile.Label));
        Assert.Equal(new RuntimeTileLine("dashboard.uat.example.net", "muted"), Assert.Single(tile.Lines));
        Assert.Equal("dashboard-uat: the dashboard of this environment (dashboard.uat.example.net). This page does not check it.", tile.Title);
    }

    /// <summary>The tile of one node of the payload.</summary>
    private static RuntimeTile Tile(RuntimePayload payload, string alias) => payload.Nodes.Single(tile => tile.Alias == alias);
}
