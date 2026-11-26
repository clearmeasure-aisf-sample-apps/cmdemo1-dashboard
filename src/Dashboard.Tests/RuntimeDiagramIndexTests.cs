namespace Dashboard.Tests;

/// <summary>
/// The payload builder asks its lookups of one index built per update (#1510) rather than scanning the manifest per
/// edge and per region. These pin what the scans answered and the index must keep answering: the first node of an
/// alias names an arrow, a web app's dependency arrows are counted apart only when it has one, wherever its other
/// ones stand in the manifest, and a web app of another region is no web app of this one.
/// </summary>
public class RuntimeDiagramIndexTests
{
    private static readonly Uri Page = new("http://localhost:5210/");

    private static RuntimeEdgeMark Edge(RuntimePayload payload, string id) => payload.Edges.Single(edge => edge.Id == id);

    [Fact]
    public void AnAliasTheManifestNamesTwiceIsNamedByItsFirstNode()
    {
        var manifest = Manifests.Of(
            "uat",
            [ManifestNodes.WebApp("app_1", "app1"), ManifestNodes.Dependency("api", "Weather API"), ManifestNodes.Dependency("api", "Maps API")],
            [ManifestEdges.ToDependency("app_1", "api")]);

        var payload = RuntimePayloadBuilder.Build(manifest, null, Page, TimeZoneInfo.Utc);

        Assert.StartsWith("app_1 to Weather API. ", Edge(payload, ManifestEdges.Id("api", "app_1")).Title, StringComparison.Ordinal);
    }

    [Fact]
    public void AWebAppsDependenciesAreCountedTogetherWhereverTheyStandInTheManifest()
    {
        var manifest = Manifests.Of(
            "uat",
            [ManifestNodes.WebApp("app_1", "app1"), ManifestNodes.WebApp("app_2", "app2"), ManifestNodes.Dependency("api", "Weather API"), ManifestNodes.Dependency("maps", "Maps API")],
            [ManifestEdges.ToDependency("app_1", "api"), ManifestEdges.ToDependency("app_2", "api"), ManifestEdges.ToDependency("app_1", "maps")]);

        var payload = RuntimePayloadBuilder.Build(manifest, null, Page, TimeZoneInfo.Utc);

        Assert.Equal("not counted apart", Edge(payload, ManifestEdges.Id("api", "app_1")).Text);
        Assert.Equal("not counted apart", Edge(payload, ManifestEdges.Id("maps", "app_1")).Text);
        Assert.Equal("outgoing HTTP calls", Edge(payload, ManifestEdges.Id("api", "app_2")).Text);
    }

    [Fact]
    public void AWebAppOfAnotherRegionLeavesThisRegionUnprobed()
    {
        var manifest = Manifests.Of(
            "uat",
            [ManifestNodes.WebApp("app_east", "app1", "https://app1.example.net", "ui", "primary", "East US", "east")],
            [],
            [new RuntimeRegion("east", "East US", [RuntimeRegionRole.Primary]), new RuntimeRegion("west", "West US", [RuntimeRegionRole.Standby])]);

        var payload = RuntimePayloadBuilder.Build(manifest, null, Page, TimeZoneInfo.Utc);

        Assert.Equal("not checked", payload.Regions.Single(region => region.Alias == "east").Label);
        Assert.Equal("standby: not probed", payload.Regions.Single(region => region.Alias == "west").Label);
    }
}
