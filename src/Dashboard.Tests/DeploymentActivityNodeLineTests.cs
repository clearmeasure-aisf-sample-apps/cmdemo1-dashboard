namespace Dashboard.Tests;

/// <summary>
/// The lines of a node (<see cref="DeploymentActivity.OfNode"/>), one kind at a time: a mark in flight, a wait, a
/// freeze, the last deployment that ended, and the order of them. Each kind has a helper of its own in the class,
/// and each helper is pinned here alone, with a file that holds nothing but that kind; the order is pinned with a
/// file that holds every kind at once.
/// </summary>
public class DeploymentActivityNodeLineTests
{
    /// <summary>Friday noon, UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 9, 12, 0, 0, TimeSpan.Zero);

    private const string Executing = """{ "project": "demo-ui", "environment": "uat", "release": "2.4.44", "state": "executing", "since": "2026-10-09T11:57:00Z", "startedBy": "ai-ops" }""";

    private const string Waiting = """{ "project": "demo-ui", "environment": "uat", "release": "2.4.43", "state": "waiting", "since": "2026-10-09T09:58:00Z", "waitsFor": { "kind": "sign-off", "since": "2026-10-09T10:00:00Z", "responsible": "ops" } }""";

    private const string InForce = """{ "name": "uat lock", "from": "2026-10-09T00:00:00Z", "to": "2026-10-09T19:00:00Z", "active": true, "environments": [ "uat" ], "projects": [ "demo-ui" ] }""";

    private const string Coming = """{ "name": "release week", "from": "2026-10-10T19:00:00Z", "to": "2026-10-11T19:00:00Z", "active": false, "environments": [ "uat" ], "projects": [ "demo-ui" ] }""";

    private const string Succeeded = """{ "project": "demo-ui", "release": "2.4.42", "environment": "uat", "result": "succeeded", "finished": "2026-10-09T07:00:00Z", "startedBy": "ai-ops" }""";

    private static IReadOnlyList<ActivityLine> Node(string deployments = "", string recent = "", string freezes = "") =>
        DeploymentActivity.OfNode(
            DeploymentsReport.Parse($$"""{ "deployments": [ {{deployments}} ], "recent": [ {{recent}} ], "freezes": [ {{freezes}} ], "missing": [] }""")!,
            "uat",
            "demo",
            "ui",
            Now,
            TimeZoneInfo.Utc);

    private static (string Kind, string Text)[] Said(IReadOnlyList<ActivityLine> lines) => [.. lines.Select(line => (line.Kind, line.Text))];

    [Fact]
    public void AMarkInFlightIsSaidWithItsAgeAndWhoStartedIt() =>
        Assert.Equal([(ActivityKind.Deploying, "deploying 2.4.44, 3 min, by ai-ops")], Said(Node(deployments: Executing)));

    [Fact]
    public void AWaitIsSaidWithWhoIsResponsibleAndNotTheAgeOfTheQuestion() =>
        Assert.Equal([(ActivityKind.Waiting, "2.4.43 waits for ops")], Said(Node(deployments: Waiting)));

    [Fact]
    public void AWaitWithoutAResponsibleIsSaidWithTheAgeOfTheQuestion() =>
        Assert.Equal(
            [(ActivityKind.Waiting, "2.4.43 waits for a sign-off, 2 h")],
            Said(Node(deployments: """{ "project": "demo-ui", "environment": "uat", "release": "2.4.43", "state": "waiting", "since": "2026-10-09T09:00:00Z", "waitsFor": { "kind": "sign-off", "since": "2026-10-09T10:00:00Z" } }""")));

    [Fact]
    public void AFreezeIsSaidBrieflyTheOneInForceBeforeTheOneThatComes() =>
        Assert.Equal(
            [(ActivityKind.Frozen, "frozen until 19:00: uat lock"), (ActivityKind.Freeze, "freeze from Sat 19:00: release week")],
            Said(Node(freezes: $"{Coming}, {InForce}")));

    [Fact]
    public void TheLastDeploymentIsSaidBrieflyWithWhoStartedIt()
    {
        var line = Assert.Single(Node(recent: Succeeded));
        Assert.Equal((ActivityKind.Deployed, "2.4.42 deployed 5 h ago by ai-ops"), (line.Kind, line.Text));
        Assert.StartsWith("The last deployment of demo-ui to uat that ended:", line.Title, StringComparison.Ordinal);
    }

    [Fact]
    public void OnlyTheFirstLastDeploymentOfTheNodeIsSaid() =>
        Assert.Equal(
            [(ActivityKind.Deployed, "2.4.42 deployed 5 h ago by ai-ops")],
            Said(Node(recent: $$"""{ "project": "demo-api", "release": "9.9.9", "environment": "uat", "result": "failed", "finished": "2026-10-09T11:00:00Z" }, {{Succeeded}}, { "project": "demo-ui", "release": "2.4.41", "environment": "uat", "result": "failed", "finished": "2026-10-09T06:00:00Z" }""")));

    [Fact]
    public void TheOrderIsMarksThenFreezesThenTheLast() =>
        Assert.Equal(
            [ActivityKind.Frozen, ActivityKind.Deployed],
            Node(recent: Succeeded, freezes: InForce).Select(line => line.Kind));

    [Fact]
    public void TheOrderPutsAWaitBeforeADeploymentAndCutsTheRestAtTheMostLines() =>
        Assert.Equal(
            [ActivityKind.Waiting, ActivityKind.Deploying],
            Node(deployments: $"{Executing}, {Waiting}", recent: Succeeded, freezes: InForce).Select(line => line.Kind));

    [Fact]
    public void TheOrderCutsEveryLineToWhatANodeHolds()
    {
        var lines = Node(freezes: """{ "name": "a freeze with a name far too long for a node", "from": "2026-10-09T00:00:00Z", "to": "2026-10-09T19:00:00Z", "active": true, "environments": [ "uat" ], "projects": [ "demo-ui" ] }""");
        var line = Assert.Single(lines);
        Assert.Equal(DeploymentActivity.NodeLength, line.Text.Length);
        Assert.EndsWith("…", line.Text, StringComparison.Ordinal);
    }
}
