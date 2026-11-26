namespace Dashboard.Tests;

/// <summary>
/// #1829 (step 2 of #534): the edge marks moved out of <c>RuntimePayloadBuilder</c> into <c>RuntimeEdgeMarks</c>
/// with no change of behaviour, so the payload the page reads stays byte for byte what it was. Each scenario below
/// builds a whole payload (tiles, regions, every kind of edge, each state of a container app's SQL arrow) and holds
/// its JSON to the text captured from the builder before the move.
/// </summary>
public class RuntimePayloadJsonGoldenTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private static readonly Uri Page = new("http://localhost:5210/");

    private static readonly ProbeResult NoAnswer = new(HealthState.Unreachable, null, null, Now, null, "No answer within 10 s.");

    private static ProbeResult Answer(int status, string version = "2.4.21") =>
        new(HealthClassifier.FromStatusCode(status), status, 40, Now, version);

    private static RuntimeManifest Uat() =>
        RuntimeManifestParser.ParseManifest(WebAppDiagram.Sample("uat.json")).Value!;

    private static EnvironmentStatus UatStatus() =>
        new(TopologyParser.Parse(WebAppDiagram.Topology).Topology!.Environments.Single(environment => environment.Name == "uat"));

    /// <summary>One node of each kind an arrow can end at, and one arrow of each kind, plus a container app's SQL arrow.</summary>
    private static readonly RuntimeManifest Kinds = new(
        "uat",
        [
            new RuntimeNode("browser", RuntimeNodeKind.Person, "Browser"),
            new RuntimeNode("app_1", RuntimeNodeKind.WebApp, "app1", new Uri("https://app1.example.net"), "ui"),
            new RuntimeNode("app_2", RuntimeNodeKind.WebApp, "app2", new Uri("https://app2.example.net"), "api"),
            new RuntimeNode("sqldb", RuntimeNodeKind.Sql, "sqldb-uat"),
            new RuntimeNode("api", RuntimeNodeKind.Dependency, "Weather API"),
            new RuntimeNode("api_2", RuntimeNodeKind.Dependency, "Maps API"),
            new RuntimeNode("api_3", RuntimeNodeKind.Dependency, "Mail API"),
            new RuntimeNode("ca", RuntimeNodeKind.ContainerApp, "ca-demo-uat", new Uri("https://ca-demo-uat.example.io"), "scorecard"),
        ],
        [],
        [
            new RuntimeEdge("browser-to-app_1", "browser", "app_1", RuntimeEdgeKind.Public),
            new RuntimeEdge("fd-to-app_1", "fd", "app_1", RuntimeEdgeKind.Origin),
            new RuntimeEdge("app_1-to-sqldb", "app_1", "sqldb", RuntimeEdgeKind.Sql),
            new RuntimeEdge("app_1-to-api", "app_1", "api", RuntimeEdgeKind.Dependency),
            new RuntimeEdge("app_2-to-api_2", "app_2", "api_2", RuntimeEdgeKind.Dependency),
            new RuntimeEdge("app_2-to-api_3", "app_2", "api_3", RuntimeEdgeKind.Dependency),
            new RuntimeEdge("ca-to-sqldb", "ca", "sqldb", RuntimeEdgeKind.Sql),
            new RuntimeEdge("browser-to-app_2", "browser", "app_2", RuntimeEdgeKind.Dashboard),
            new RuntimeEdge("browser-to-swa", "browser", "swa", RuntimeEdgeKind.Dashboard),
            new RuntimeEdge("a-to-b", "a", "b", RuntimeEdgeKind.Other),
        ]);

    private static EnvironmentStatus KindsStatus(HealthState? state, TelemetrySnapshot? telemetry)
    {
        var environment = new EnvironmentStatus(TopologyParser.Parse("""
            { "environments": [ { "name": "uat", "deployables": [
                { "name": "ui", "frontDoor": null, "nodes": [ { "name": "app1", "role": "primary", "url": "https://app1.example.net" } ] },
                { "name": "api", "frontDoor": null, "nodes": [ { "name": "app2", "role": "primary", "url": "https://app2.example.net" } ] } ] } ] }
            """).Topology!.Environments[0]);
        if (state is { } answered)
        {
            foreach (var deployable in environment.Deployables)
            {
                deployable.Nodes[0].Record(new ProbeResult(answered, answered == HealthState.Healthy ? 200 : null, 40, Now, "2.4.21"));
                if (telemetry is not null)
                {
                    deployable.Nodes[0].RecordTelemetry(telemetry);
                }
            }
        }

        return environment;
    }

    private static ContainerAppStatus Probed(ContainerAppState state) =>
        new(state, new ContainerAppAnswer("/health", true, state == ContainerAppState.Healthy ? 200 : 503, false, 85, Now.AddSeconds(-20), null), false);

    private static RuntimePayload Scenario(string name) => name switch
    {
        "uat-unchecked" => RuntimePayloadBuilder.Build(Uat(), null, Page, TimeZoneInfo.Utc),
        "uat-counting" => RuntimePayloadBuilder.Build(Uat(), Counting(), Page, TimeZoneInfo.Utc),
        "uat-failed-over" => RuntimePayloadBuilder.Build(Uat(), FailedOver(), Page, TimeZoneInfo.Utc),
        "kinds-unchecked" => RuntimePayloadBuilder.Build(Kinds, null, Page, TimeZoneInfo.Utc),
        "kinds-pending" => RuntimePayloadBuilder.Build(Kinds, KindsStatus(null, null), Page, TimeZoneInfo.Utc),
        "kinds-healthy" => RuntimePayloadBuilder.Build(Kinds, KindsStatus(HealthState.Healthy, TelemetrySnapshots.WithHttp(12, 7, Now) with { Direct = 5 }), Page, TimeZoneInfo.Utc),
        "kinds-down" => RuntimePayloadBuilder.Build(Kinds, KindsStatus(HealthState.Unreachable, null), Page, TimeZoneInfo.Utc),
        _ when name.StartsWith("container-", StringComparison.Ordinal) => RuntimePayloadBuilder.Build(
            Kinds,
            null,
            Page,
            TimeZoneInfo.Utc,
            containerApps: _ => name == "container-none" ? null : Probed(Enum.Parse<ContainerAppState>(name["container-".Length..]))),
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, null),
    };

    /// <summary>uat where Front Door and the primary answer and count, and the standby does not answer.</summary>
    private static EnvironmentStatus Counting()
    {
        var uat = UatStatus();
        var ui = uat.Deployables[0];
        ui.FrontDoor!.Record(Answer(200));
        ui.Nodes[0].Record(Answer(200));
        ui.Nodes[0].RecordTelemetry(TelemetrySnapshots.WithSplitSql(30, 20, 15, 5, Now) with { FromFrontDoor = 25, FrontDoorProbes = 3, Direct = 2, Http = 4, SqlP95Ms = 12 });
        ui.Nodes[1].Record(NoAnswer);
        return uat;
    }

    /// <summary>uat where the primary does not answer and the standby serves, counting its SQL as one number.</summary>
    private static EnvironmentStatus FailedOver()
    {
        var uat = UatStatus();
        var ui = uat.Deployables[0];
        ui.FrontDoor!.Record(Answer(200));
        ui.Nodes[0].Record(NoAnswer);
        ui.Nodes[1].Record(Answer(200, "2.4.20"));
        ui.Nodes[1].RecordTelemetry(TelemetrySnapshots.WithSql(9, 4, Now) with { FromFrontDoor = 9 });
        return uat;
    }

    private static readonly string[] Names =
    [
        "uat-unchecked",
        "uat-counting",
        "uat-failed-over",
        "kinds-unchecked",
        "kinds-pending",
        "kinds-healthy",
        "kinds-down",
        "container-none",
        .. Enum.GetNames<ContainerAppState>().Select(state => $"container-{state}"),
    ];

    public static TheoryData<string> Scenarios() => new(Names);

    [Theory]
    [MemberData(nameof(Scenarios))]
    public void ThePayloadJsonIsByteIdenticalToTheOneBuiltBeforeTheEdgeMarksMoved(string scenario)
    {
        Assert.True(File.Exists(GoldenPath(scenario)), $"no golden JSON for {scenario} at {GoldenPath(scenario)}");
        Assert.Equal(File.ReadAllText(GoldenPath(scenario)), Scenario(scenario).ToJson());
    }

    [Fact]
    public void EveryKindOfEdgeAndEveryContainerAppStateIsPinned()
    {
        var ids = Names.SelectMany(scenario => Scenario(scenario).Edges).Select(edge => edge.Id).ToHashSet(StringComparer.Ordinal);

        Assert.Equal(
            Names.Order(StringComparer.Ordinal),
            Directory.GetFiles(GoldenRoot(), "*.json").Select(Path.GetFileNameWithoutExtension).Order(StringComparer.Ordinal));
        Assert.Contains("fd_ui-to-app_ui_primary", ids);
        Assert.Contains("browser-to-fd_ui", ids);
        Assert.Contains("app_1-to-sqldb", ids);
        Assert.Contains("app_1-to-api", ids);
        Assert.Contains("ca-to-sqldb", ids);
        Assert.Contains("browser-to-app_2", ids);
        Assert.Contains("a-to-b", ids);
        Assert.All(Enum.GetNames<ContainerAppState>(), state => Assert.True(File.Exists(GoldenPath($"container-{state}")), state));
    }

    /// <summary>
    /// The folder of the payload JSON of each scenario, as the builder wrote it before the edge marks moved (#1829):
    /// <c>Fixtures/runtime-golden/&lt;scenario&gt;.json</c>, read from the source tree, so no build step rewrites it.
    /// </summary>
    private static string GoldenRoot() => Path.Combine(MarkupGuards.TestsRoot(), "Fixtures", "runtime-golden");

    private static string GoldenPath(string scenario) => Path.Combine(GoldenRoot(), scenario + ".json");
}
