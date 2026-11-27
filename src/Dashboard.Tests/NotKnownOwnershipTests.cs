namespace Dashboard.Tests;

/// <summary>
/// The single owner of the word <c>Not known</c> (#2055): <see cref="HealthDetailText.NotKnown" /> is the one place
/// the word is written, and <c>ResourceHealthText</c> and <c>RuntimePayloadBuilder</c> read it from there instead of
/// carrying their own copy. These tests pin the agreement through public behaviour, because the tests have no
/// <c>InternalsVisibleTo</c>: the badge word of a box with no report, the label of a reading that is not there, and
/// the label of a dependency tile whose worst health-check entry is <see cref="CheckState.Unknown" />, must all
/// equal <see cref="HealthDetailText.Label" /> of <see cref="CheckState.Unknown" />.
/// </summary>
public class NotKnownOwnershipTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private static readonly Uri Page = new("http://localhost:5210/");

    private static readonly Topology SampleTopology = TopologyParser.Parse(WebAppDiagram.Topology).Topology!;
    private static readonly string[] WebApps = ["app_ui_primary", "app_ui_standby"];
    private static readonly RuntimeManifest Sample = RuntimeManifestParser.ParseManifest(WebAppDiagram.Sample("uat.json")).Value!;

    // ---------------------------------------------------------------- the resource-health box

    /// <summary>
    /// A box with no report at all carries the word <see cref="HealthDetailText.NotKnown" /> as its badge, and a
    /// reading that is not there (null) is labelled the same word: the two homes of the word in
    /// <c>ResourceHealthText</c> (<see cref="ResourceHealthText.Of" /> and <see cref="ResourceHealthText.Label" />)
    /// both stand on <see cref="HealthDetailText.Label" /> of <see cref="CheckState.Unknown" />, so a typo in one
    /// cannot quietly split them.
    /// </summary>
    [Fact]
    public void TheResourceHealthBoxWithNoReportAndTheLabelOfNoReadingBothSayTheOwnersWord()
    {
        var ownerWord = HealthDetailText.Label(CheckState.Unknown);

        // ResourceHealthText.Label(null) — the fallback arm of the switch.
        Assert.Equal(ownerWord, ResourceHealthText.Label((ResourceHealthReading?)null));

        // ResourceHealthText.Of(ids, null, …) — the box with no report at all.
        var words = ResourceHealthText.Of(["/subscriptions/s/resourceGroups/rg/providers/Microsoft.Web/sites/app"], null, Now, TimeZoneInfo.Utc);
        Assert.NotNull(words);
        Assert.Equal(ownerWord, words.Label);
    }

    // ---------------------------------------------------------------- the dependency tile

    /// <summary>
    /// A dependency tile whose worst health-check entry is <see cref="CheckState.Unknown" /> carries the label
    /// <see cref="HealthDetailText.Label" /> of <see cref="CheckState.Unknown" />: the <c>CheckState.Unknown =>
    /// NeutralTile(node, NotKnown, …)</c> arm of <c>RuntimePayloadBuilder.DependencyTile</c> reads its word from
    /// <see cref="HealthDetailText.NotKnown" />, so the tile's label and the health-check's own word for the same
    /// state cannot drift apart. The fixtures are the same uat web-app diagram and object mothers the other runtime
    /// tile tests use.
    /// </summary>
    [Fact]
    public void ADependencyTileWhoseWorstEntryIsUnknownCarriesTheOwnersWord()
    {
        var ownerWord = HealthDetailText.Label(CheckState.Unknown);

        var manifest = Sample with
        {
            Nodes =
            [
                .. Sample.Nodes,
                new RuntimeNode("dep_ui_LLM_gateway", RuntimeNodeKind.Dependency, "LLM gateway", Deployable: "ui", HealthCheck: "LlmGateway", DependencyKind: "external"),
            ],
            Edges =
            [
                .. Sample.Edges,
                .. from app in WebApps select new RuntimeEdge($"dep_ui_LLM_gateway-to-{app}", app, "dep_ui_LLM_gateway", RuntimeEdgeKind.Dependency),
            ],
        };

        var environment = new EnvironmentStatus(SampleTopology.Environments.Single(environment => environment.Name == "uat"));
        var ui = environment.Deployables[0];
        ui.FrontDoor!.Record(Answer(200, ProbeKind.Health));
        ui.Nodes[0].Record(Answer(200, ProbeKind.Health));
        ui.Nodes[1].Record(Answer(200, ProbeKind.Health));
        // Both web apps' detailed health checks name LlmGateway with a status the dashboard does not know.
        ui.Nodes[0].RecordHealthDetail(HealthDetailTests.Detail(("API", "Healthy"), ("LlmGateway", "Paused")));
        ui.Nodes[1].RecordHealthDetail(HealthDetailTests.Detail(("API", "Healthy"), ("LlmGateway", "Paused")));

        var tile = RuntimePayloadBuilder.Build(manifest, environment, Page, TimeZoneInfo.Utc)
            .Nodes.Single(tile => tile.Alias == "dep_ui_LLM_gateway");

        Assert.Equal("neutral", tile.State);
        Assert.Equal(ownerWord, tile.Label);
    }

    // ---------------------------------------------------------------- the machinery

    private static ProbeResult Answer(int status, ProbeKind probe) =>
        new(HealthClassifier.FromStatusCode(status), status, 40, Now, "2.4.21", Probe: probe);
}
