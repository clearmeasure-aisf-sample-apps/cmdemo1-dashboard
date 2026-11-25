using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>
/// The three paths that reach the "waiting for the health checks" tile (TileState.Checking) all go through one
/// private helper, WaitingTile, since issue #1667. These tests pin the whole tile on each path: state, label,
/// line, tone, and the full title sentence. The existing tests checked the line but not the title or label.
/// </summary>
public class RuntimePayloadWaitingTileTests
{
    private static readonly Uri Page = new("http://localhost:5210/");
    private static readonly Topology Topology = TopologyParser.Parse(WebAppDiagram.Topology).Topology!;

    [Fact]
    public void ADependencyOfAPendingWebAppWaitsForItsHealthChecks()
    {
        // No checks recorded yet: every target is HealthState.Pending.
        var environment = new EnvironmentStatus(Topology.Environments.Single(env => env.Name == "uat"));
        var manifest = ManifestWithDependency();

        var payload = RuntimePayloadBuilder.Build(manifest, environment, Page, TimeZoneInfo.Utc);
        var tile = payload.Nodes.Single(t => t.Alias == "dep_ui_LLM_gateway");

        Assert.Equal("checking", tile.State);
        Assert.Equal("Checking", tile.Label);
        Assert.Equal(new RuntimeTileLine("waiting for the health checks", "muted"), Assert.Single(tile.Lines));
        Assert.Null(tile.Facts);
        Assert.Null(tile.History);
        Assert.Equal("LLM gateway: waiting for the health checks of the web apps that use it.", tile.Title);
    }

    [Fact]
    public void ADatabaseOfAPendingWebAppWaitsForItsHealthChecks()
    {
        // The tdd environment has no checks recorded yet: targets are HealthState.Pending.
        var environment = new EnvironmentStatus(Topology.Environments.Single(env => env.Name == "tdd"));
        var manifest = RuntimeManifestParser.ParseManifest(WebAppDiagram.Sample("tdd.json")).Value!;

        var payload = RuntimePayloadBuilder.Build(manifest, environment, Page, TimeZoneInfo.Utc);
        var tile = payload.Nodes.Single(t => t.Alias == "sqldb");

        Assert.Equal("checking", tile.State);
        Assert.Equal("Checking", tile.Label);
        Assert.Equal(new RuntimeTileLine("waiting for the health checks", "muted"), Assert.Single(tile.Lines));
        Assert.Null(tile.Facts);
        Assert.Null(tile.History);
        Assert.Equal("sqldb-cmdemo2-tdd: waiting for the health checks of the web apps that use it.", tile.Title);
    }

    [Fact]
    public void ADatabaseOfAProbingContainerAppWaitsForItsHealthChecks()
    {
        // No container app probe yet: status is null, which maps to ContainerAppState.Probing.
        var manifest = RuntimeManifestParser.ParseManifest(RuntimeManifestParserTests.Sample("tdd.json")).Value!;

        var payload = RuntimePayloadBuilder.Build(manifest, null, Page, TimeZoneInfo.Utc);
        var tile = payload.Nodes.Single(t => t.Alias == "sqldb");

        Assert.Equal("checking", tile.State);
        Assert.Equal("Checking", tile.Label);
        Assert.Equal(new RuntimeTileLine("waiting for the health checks", "muted"), Assert.Single(tile.Lines));
        Assert.Null(tile.Facts);
        Assert.Null(tile.History);
        Assert.Equal("sqldb-cmdemo1-tdd: waiting for the health checks of the container apps that use it.", tile.Title);
    }

    /// <summary>The uat manifest with a dependency node and edges from the web apps to it.</summary>
    private static RuntimeManifest ManifestWithDependency()
    {
        var baseManifest = RuntimeManifestParser.ParseManifest(WebAppDiagram.Sample("uat.json")).Value!;
        return baseManifest with
        {
            Nodes =
            [
                .. baseManifest.Nodes,
                new RuntimeNode("dep_ui_LLM_gateway", RuntimeNodeKind.Dependency, "LLM gateway", Deployable: "ui", HealthCheck: "LlmGateway", DependencyKind: "external"),
            ],
            Edges =
            [
                .. baseManifest.Edges,
                new RuntimeEdge("dep_ui_LLM_gateway-to-app_ui_primary", "app_ui_primary", "dep_ui_LLM_gateway", RuntimeEdgeKind.Dependency),
                new RuntimeEdge("dep_ui_LLM_gateway-to-app_ui_standby", "app_ui_standby", "dep_ui_LLM_gateway", RuntimeEdgeKind.Dependency),
            ],
        };
    }
}
