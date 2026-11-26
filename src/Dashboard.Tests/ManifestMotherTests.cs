namespace Dashboard.Tests;

/// <summary>
/// The manifest ObjectMother in <c>ManifestMother.cs</c>, pinned against the records written out with every part
/// named, so a value landing in the wrong slot of <see cref="RuntimeNode" />'s fourteen shows here rather than as a
/// puzzling failure in <see cref="LinkTests" />, <see cref="RotationTests" />, <see cref="TrendTests" />,
/// <see cref="RuntimeClientsTests" />, <see cref="DependencyTests" /> or <see cref="RuntimeEdgeEdgeCaseTests" />.
/// </summary>
public class ManifestMotherTests
{
    [Fact]
    public void AnEdgeIdIsTheTwoAliasesWithToBetweenThem()
    {
        Assert.Equal("browser-to-fd_ui", ManifestEdges.Id("browser", "fd_ui"));
        Assert.Equal(
            new RuntimeEdge("fd_ui-to-app_ui_primary", "fd_ui", "app_ui_primary", RuntimeEdgeKind.Origin, 1),
            ManifestEdges.Between("fd_ui", "app_ui_primary", RuntimeEdgeKind.Origin, 1));
        Assert.Null(ManifestEdges.Between("app_ui_primary", "sqldb", RuntimeEdgeKind.Sql).Priority);
    }

    [Fact]
    public void ADependencysEdgeRunsFromTheAppButItsIdNamesTheDependencyFirst()
    {
        Assert.Equal(
            new RuntimeEdge("dep_ui_LLM_gateway-to-app_ui_primary", "app_ui_primary", "dep_ui_LLM_gateway", RuntimeEdgeKind.Dependency, null),
            ManifestEdges.ToDependency("app_ui_primary", "dep_ui_LLM_gateway"));
    }

    [Fact]
    public void EachNodeIsTheRecordWithItsPartsInTheirOwnSlots()
    {
        Assert.Equal(new RuntimeNode(Alias: "browser", Kind: RuntimeNodeKind.Person, Name: "Browser"), ManifestNodes.Browser());
        Assert.Equal(
            new RuntimeNode(Alias: "fd_ui", Kind: RuntimeNodeKind.FrontDoor, Name: "demo-uat-ui", Url: new Uri("https://fd-uat.example.net"), Deployable: "ui"),
            ManifestNodes.FrontDoor("fd_ui", "demo-uat-ui", "https://fd-uat.example.net", "ui"));
        Assert.Equal(
            new RuntimeNode(Alias: "app_ui_primary", Kind: RuntimeNodeKind.WebApp, Name: "uat-west", Url: new Uri("https://uat-west.example.net"), Deployable: "ui", Role: "primary", Region: "westus3", RegionAlias: "region_primary"),
            ManifestNodes.WebApp("app_ui_primary", "uat-west", "https://uat-west.example.net", "ui", "primary", "westus3", "region_primary"));
        Assert.Equal(new RuntimeNode(Alias: "web1", Kind: RuntimeNodeKind.WebApp, Name: "app-1"), ManifestNodes.WebApp("web1", "app-1"));
        Assert.Equal(new RuntimeNode(Alias: "job", Kind: RuntimeNodeKind.ContainerApp, Name: "ca-job"), ManifestNodes.ContainerApp("job", "ca-job"));
        Assert.Equal(new RuntimeNode(Alias: "db", Kind: RuntimeNodeKind.Sql, Name: "sql-db"), ManifestNodes.Database("db", "sql-db"));
        Assert.Equal(
            new RuntimeNode(Alias: "sqldb", Kind: RuntimeNodeKind.Sql, Name: "sqldb-demo-uat", Region: "centralus", RegionAlias: "region_data"),
            ManifestNodes.Database("sqldb", "sqldb-demo-uat", "centralus", "region_data"));
        Assert.Equal(new RuntimeNode(Alias: "ext", Kind: RuntimeNodeKind.Dependency, Name: "External"), ManifestNodes.Dependency("ext", "External"));
        Assert.Equal(
            new RuntimeNode(Alias: "dep_ui_LLM_gateway", Kind: RuntimeNodeKind.Dependency, Name: "LLM gateway", Deployable: "ui", HealthCheck: "LlmGateway", DependencyKind: "external"),
            ManifestNodes.ExternalDependency("dep_ui_LLM_gateway", "LLM gateway", "ui", "LlmGateway"));
    }

    [Fact]
    public void AManifestHasNoRegionsUnlessTheTestNamesThem()
    {
        RuntimeNode[] nodes = [ManifestNodes.Browser()];
        RuntimeEdge[] edges = [ManifestEdges.Between("browser", "fd_ui", RuntimeEdgeKind.Public)];
        RuntimeRegion[] regions = [new RuntimeRegion("region_west", "westus3", ["primary"])];

        var bare = Manifests.Of("uat", nodes, edges);
        var placed = Manifests.Of("prod", nodes, edges, regions);

        Assert.Equal(("uat", nodes, edges), (bare.Environment, bare.Nodes, bare.Edges));
        Assert.Empty(bare.Regions);
        Assert.Null(bare.Frames);
        Assert.Null(bare.Unplaced);
        Assert.Same(regions, placed.Regions);
    }
}
