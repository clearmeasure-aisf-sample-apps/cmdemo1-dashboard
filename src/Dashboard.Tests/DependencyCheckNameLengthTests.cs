namespace Dashboard.Tests;

/// <summary>
/// How much of a health check entry's name a dependency's tile line holds where no description stands in for it:
/// 14 characters, a longer name cut to 13 and an ellipsis. Built like <see cref="DependencyTests"/>: uat's web app
/// diagram with one dependency added.
/// </summary>
public class DependencyCheckNameLengthTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private static readonly Uri Page = new("http://localhost:5210/");
    private const string Gateway = "dep_ui_LLM_gateway";
    private const string LongName = "LlmGatewayClient";
    private const string FourteenName = "LlmGatewayChat";

    private static readonly Topology SampleTopology =
        TopologyParser.Parse(WebAppDiagram.Topology).Topology!;

    private static readonly string[] WebApps = ["app_ui_primary", "app_ui_standby"];

    private static readonly RuntimeManifest Sample = RuntimeManifestParser.ParseManifest(WebAppDiagram.Sample("uat.json")).Value!;

    private static RuntimeManifest Manifest(string healthCheck) => Sample with
    {
        Nodes = [.. Sample.Nodes, new RuntimeNode(Gateway, RuntimeNodeKind.Dependency, "LLM gateway", Deployable: "ui", HealthCheck: healthCheck, DependencyKind: "external")],
        Edges = [.. Sample.Edges, .. WebApps.Select(app => new RuntimeEdge($"{Gateway}-to-{app}", app, Gateway, RuntimeEdgeKind.Dependency))],
    };

    private static ProbeResult Answer() => new(HealthClassifier.FromStatusCode(200), 200, 40, Now, "2.4.21", Probe: ProbeKind.Health);

    /// <summary>uat with both web apps healthy, each reporting the one entry given, without words of its own.</summary>
    private static EnvironmentStatus Uat(string entry, CheckState state)
    {
        var uat = new EnvironmentStatus(SampleTopology.Environments.Single(environment => environment.Name == "uat"));
        var ui = uat.Deployables[0];
        ui.FrontDoor!.Record(Answer());
        foreach (var node in ui.Nodes)
        {
            node.Record(Answer());
            node.RecordHealthDetail(new HealthDetail([new HealthCheckEntry(entry, state)], Now));
        }

        return uat;
    }

    private static RuntimeTile Tile(string healthCheck, EnvironmentStatus uat) =>
        RuntimePayloadBuilder.Build(Manifest(healthCheck), uat, Page, TimeZoneInfo.Utc).Nodes.Single(tile => tile.Alias == Gateway);

    [Fact]
    public void ALongNameWithoutAnEntryIsCutOnTheNotKnownLine()
    {
        var gateway = Tile(LongName, Uat("LlmGateway", CheckState.Healthy));

        Assert.Equal(("neutral", "Not known"), (gateway.State, gateway.Label));
        Assert.Equal(new RuntimeTileLine("no health check entry LlmGatewayCli…", "muted"), Assert.Single(gateway.Lines));
    }

    [Fact]
    public void ALongNameIsCutOnTheUnhealthyLine()
    {
        var gateway = Tile(LongName, Uat(LongName, CheckState.Unhealthy));

        Assert.Equal(("unhealthy", "Unhealthy"), (gateway.State, gateway.Label));
        Assert.Equal(new RuntimeTileLine("health check LlmGatewayCli…: unhealthy", "warn"), Assert.Single(gateway.Lines));
    }

    [Fact]
    public void ALongNameIsCutOnThePassedLine()
    {
        var gateway = Tile(LongName, Uat(LongName, CheckState.Healthy));

        Assert.Equal(new RuntimeTileLine("health check LlmGatewayCli… passed", "ok"), Assert.Single(gateway.Lines));
    }

    [Fact]
    public void ANameOfFourteenCharactersComesThroughWhole()
    {
        Assert.Equal(14, FourteenName.Length);
        Assert.Equal(new RuntimeTileLine($"no health check entry {FourteenName}", "muted"), Assert.Single(Tile(FourteenName, Uat("LlmGateway", CheckState.Healthy)).Lines));
        Assert.Equal(new RuntimeTileLine($"health check {FourteenName}: unhealthy", "warn"), Assert.Single(Tile(FourteenName, Uat(FourteenName, CheckState.Unhealthy)).Lines));
        Assert.Equal(new RuntimeTileLine($"health check {FourteenName} passed", "ok"), Assert.Single(Tile(FourteenName, Uat(FourteenName, CheckState.Healthy)).Lines));
    }
}
