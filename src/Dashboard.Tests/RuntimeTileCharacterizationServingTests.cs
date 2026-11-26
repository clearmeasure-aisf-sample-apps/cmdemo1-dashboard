using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// Characterization of the two checked tiles of <see cref="RuntimePayloadBuilder.Build"/>, in the style of steps 1 and 2
/// (<see cref="RuntimeTileCharacterizationTests"/>, <see cref="RuntimeTileCharacterizationDataTests"/>): the Front Door tile for
/// each arm of its <see cref="ServingState"/> switch and of its <see cref="FrontDoorAgreement"/> switch, and the web app tile's
/// lines - the role line (<c>RoleLine</c>, <c>SingleNodeLine</c>), the health check line (<c>ChecksLine</c>) and the telemetry
/// lines (<c>TelemetryLines</c>). Step 3 of #1456: pin the behavior before the builder is split, so the split can be checked
/// against it.
/// <para>
/// Each tile is built only through <see cref="RuntimePayloadBuilder.Build"/>, from one deployable <c>ui</c> written here: a
/// Front Door endpoint (or none) in front of the nodes each test names. The tiles' lines are pinned as a whole sequence, words
/// and tone, where the older tests (<see cref="RuntimePayloadBuilderTests"/>, <see cref="RotationTests"/>,
/// <see cref="ProcessVitalsTests"/>, <see cref="DependencyTests"/>) pin one line or one word at a time. What
/// <see cref="FrontDoorAgreementEdgeCaseTests"/> pins - the enum and the cells of the agreement decision - is left to it.
/// </para>
/// </summary>
public class RuntimeTileCharacterizationServingTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private static readonly Uri FrontDoorUrl = new("https://fd-uat.example.net");
    private static readonly string[] Regions = ["westus3", "eastus2", "centralus"];

    /// <summary>What a check found: an HTTP status, <see cref="NoAnswer"/>, or null for not checked yet.</summary>
    private const int NoAnswer = 0;

    /// <summary>uat with the deployable ui: a Front Door endpoint when <paramref name="frontDoor"/>, and a node per role.</summary>
    private static EnvironmentStatus Uat(bool frontDoor, params string[] roles)
    {
        var nodes = string.Join(",", roles.Select((role, index) =>
            $$"""{ "name": "uat-{{index}}", "region": "{{Regions[index]}}", "role": "{{role}}", "url": "https://uat-{{index}}.example.net" }"""));
        var address = frontDoor ? $"\"{FrontDoorUrl}\"" : "null";
        return new EnvironmentStatus(TopologyParser.Parse($$"""
            { "environments": [ { "name": "uat", "deployables": [ {
                "name": "ui", "frontDoor": {{address}}, "nodes": [ {{nodes}} ] } ] } ] }
            """).Topology!.Environments[0]);
    }

    private static void Record(TargetStatus target, int? status)
    {
        if (status is { } answered)
        {
            target.Record(answered == NoAnswer
                ? new ProbeResult(HealthState.Unreachable, null, null, Now, null, "No answer within 10 s.")
                : new ProbeResult(HealthClassifier.FromStatusCode(answered), answered, 40, Now, "2.4.21"));
        }
    }

    /// <summary>uat behind its Front Door endpoint, with every target checked as given (nodes in the order of the roles).</summary>
    private static EnvironmentStatus Checked(int? frontDoor, params (string Role, int? Status)[] nodes)
    {
        var uat = Uat(true, [.. nodes.Select(node => node.Role)]);
        Record(uat.Deployables[0].FrontDoor!, frontDoor);
        foreach (var (node, index) in nodes.Select((node, index) => (node, index)))
        {
            Record(uat.Deployables[0].Nodes[index], node.Status);
        }

        return uat;
    }

    /// <summary>The payload of the environment: the Front Door node <c>fd_ui</c> and a web app <c>app_{index}</c> per node.</summary>
    private static RuntimePayload Build(EnvironmentStatus environment)
    {
        var nodes = environment.Deployables[0].Nodes.Select((node, index) => new RuntimeNode(
            $"app_{index}", RuntimeNodeKind.WebApp, $"uat-{index}", node.Url, "ui", node.Role, Regions[index], $"region_{index}"));
        var frontDoor = new RuntimeNode("fd_ui", RuntimeNodeKind.FrontDoor, "fd-uat", FrontDoorUrl, "ui");
        return RuntimePayloadBuilder.Build(new RuntimeManifest("uat", [frontDoor, .. nodes], [], []), environment, null, TimeZoneInfo.Utc);
    }

    private static RuntimeTile Tile(EnvironmentStatus environment, string alias) => Build(environment).Nodes.Single(tile => tile.Alias == alias);

    private static (string Text, string Tone)[] Words(RuntimeTile tile) => [.. tile.Lines.Select(line => (line.Text, line.Tone))];

    public static TheoryData<string, int?, (string Role, int? Status)[], string, string> ServingArms => new()
    {
        { "Primary, a rotation every primary serves", 200, [("primary", 200), ("primary", 200)], "rotates over 2 regions", RuntimeLineTone.Plain },
        { "Primary, a rotation one primary left", 200, [("primary", 200), ("primary", 503), ("primary", 200)], "rotates over 2 of 3 regions", RuntimeLineTone.Plain },
        { "Primary, one primary", 200, [("primary", 200), ("standby", 200)], "routes to westus3 (priority 1)", RuntimeLineTone.Plain },
        { "FailedOver", 200, [("primary", NoAnswer), ("standby", 200)], "routes to eastus2 (failed over)", RuntimeLineTone.Serving },
        { "Down", 503, [("primary", 503), ("standby", NoAnswer)], "no healthy origin", RuntimeLineTone.Plain },
        { "NoNodes", 200, [], "no origins in the topology", RuntimeLineTone.Muted },
        { "Pending", 200, [("primary", null), ("standby", null)], "origins being checked", RuntimeLineTone.Muted },
    };

    /// <summary>Every arm of the <see cref="ServingState"/> switch: the second of the Front Door tile's three lines.</summary>
    [Theory]
    [MemberData(nameof(ServingArms))]
    public void TheFrontDoorTileSaysWhereItRoutesForEveryServingState(string arm, int? frontDoor, (string Role, int? Status)[] nodes, string line, string tone)
    {
        var tile = Tile(Checked(frontDoor, nodes), "fd_ui");

        Assert.False(string.IsNullOrEmpty(arm));
        Assert.Equal(3, tile.Lines.Count);
        Assert.Equal(("version 2.4.21", RuntimeLineTone.Strong), Words(tile)[0]);
        Assert.Equal((line, tone), Words(tile)[1]);
    }

    public static TheoryData<string, int?, (string Role, int? Status)[], string, string, string> AgreementArms => new()
    {
        { "Agrees", 200, [("primary", 200), ("standby", 200)], "agrees with the web apps", RuntimeLineTone.Ok, "Front Door agrees: it is healthy." },
        { "Disagrees", 503, [("primary", 200), ("standby", 200)], "disagrees with the web apps", RuntimeLineTone.Warn, "Front Door disagrees: it is unhealthy although westus3 is healthy." },
        { "Pending", null, [("primary", 200), ("standby", 200)], "being compared with the web apps", RuntimeLineTone.Muted, "Front Door is being checked." },
    };

    /// <summary>
    /// Every arm of the <see cref="FrontDoorAgreement"/> switch: the last of the Front Door tile's lines, and the agreement in
    /// words as the last line of its tooltip. The tile has no link of its own, and its name leads nowhere in a topology without links.
    /// </summary>
    [Theory]
    [MemberData(nameof(AgreementArms))]
    public void TheFrontDoorTileSaysWhetherItAgreesWithTheWebAppsForEveryAgreement(string arm, int? frontDoor, (string Role, int? Status)[] nodes, string line, string tone, string note)
    {
        var tile = Tile(Checked(frontDoor, nodes), "fd_ui");

        Assert.False(string.IsNullOrEmpty(arm));
        Assert.Equal((line, tone), Words(tile)[^1]);
        Assert.EndsWith($"\n{note}", tile.Title, StringComparison.Ordinal);
        Assert.Null(tile.Link);
        Assert.Null(tile.NameLink);
        Assert.Null(tile.Deployment);
    }

    /// <summary>A Front Door endpoint not checked yet: its tile in full, version not known and both switches waiting.</summary>
    [Fact]
    public void AFrontDoorTileBeforeAnyCheckIsPinnedInFull()
    {
        var tile = Tile(Checked(null, ("primary", null)), "fd_ui");

        Assert.Equal(("checking", "Checking", "not checked yet"), (tile.State, tile.Label, tile.Facts));
        Assert.Equal(
            [("version not known", RuntimeLineTone.Muted), ("origins being checked", RuntimeLineTone.Muted), ("being compared with the web apps", RuntimeLineTone.Muted)],
            Words(tile));
        Assert.Empty(tile.History!);
    }

    public static TheoryData<string, int?, string, string> RoleArms => new()
    {
        { "primary", 200, "primary: serves traffic", RuntimeLineTone.Serving },
        { "standby", 200, "standby: ready, no traffic", RuntimeLineTone.Muted },
        { "standby", null, "standby", RuntimeLineTone.Muted },
        { "standby", 503, "standby: not serving", RuntimeLineTone.Plain },
        { "standby", NoAnswer, "standby: not serving", RuntimeLineTone.Plain },
    };

    /// <summary>
    /// <c>RoleLine</c>, behind a Front Door endpoint with a healthy primary: the node's role and whether it serves, as the last
    /// line of a tile with no telemetry and no detailed health check.
    /// </summary>
    [Theory]
    [MemberData(nameof(RoleArms))]
    public void TheWebAppTilesRoleLineSaysWhetherTheNodeServes(string role, int? status, string line, string tone)
    {
        var uat = role == "primary" ? Checked(200, ("primary", status)) : Checked(200, ("primary", 200), ("standby", status));
        var tile = Tile(uat, role == "primary" ? "app_0" : "app_1");

        Assert.Equal(line, tile.Lines[^1].Text);
        Assert.Equal(tone, tile.Lines[^1].Tone);
        Assert.Equal(status is null or NoAnswer ? "version not known" : "version 2.4.21", tile.Lines[0].Text);
        Assert.Equal(2, tile.Lines.Count);
    }

    /// <summary>
    /// <c>SingleNodeLine</c>: the only node of a deployable without a Front Door endpoint has no role in its line, and its tile
    /// has the version line and that line only.
    /// </summary>
    [Theory]
    [InlineData(null, "version not known", "checking", RuntimeLineTone.Muted)]
    [InlineData(200, "version 2.4.21", "serves traffic", RuntimeLineTone.Serving)]
    [InlineData(503, "version 2.4.21", "not serving", RuntimeLineTone.Plain)]
    [InlineData(NoAnswer, "version not known", "not serving", RuntimeLineTone.Plain)]
    public void TheOnlyNodesTileHasItsVersionAndTheSingleNodeLine(int? status, string version, string line, string tone)
    {
        var uat = Uat(false, "primary");
        Record(uat.Deployables[0].Nodes[0], status);

        Assert.Equal([(version, status is null or NoAnswer ? RuntimeLineTone.Muted : RuntimeLineTone.Strong), (line, tone)], Words(Tile(uat, "app_0")));
    }

    /// <summary>
    /// <c>ChecksLine</c>: the last line of the tile, the summary in words, plain when every entry is healthy and a warning
    /// otherwise, with a mark per entry in the order the check gave them (eight or fewer).
    /// </summary>
    [Theory]
    [InlineData("Healthy", "Healthy", "2 checks healthy", RuntimeLineTone.Plain, "healthy,healthy")]
    [InlineData("Healthy", "Degraded", "LlmGateway degraded", RuntimeLineTone.Warn, "healthy,degraded")]
    [InlineData("Unhealthy", "Healthy", "API unhealthy", RuntimeLineTone.Warn, "failed,healthy")]
    [InlineData("Unhealthy", "Degraded", "2 of 2 checks not healthy", RuntimeLineTone.Warn, "failed,degraded")]
    public void TheWebAppTilesChecksLineSummarisesTheDetailedHealthCheck(string api, string gateway, string line, string tone, string marks)
    {
        var uat = Checked(200, ("primary", 200));
        uat.Deployables[0].Nodes[0].RecordHealthDetail(HealthDetailTests.Detail(("API", api), ("LlmGateway", gateway)));

        var checks = Tile(uat, "app_0").Lines[^1];

        Assert.Equal((line, tone), (checks.Text, checks.Tone));
        Assert.Equal(marks.Split(','), checks.Marks!.Select(mark => mark.State));
        Assert.Equal("primary: serves traffic", Tile(uat, "app_0").Lines[^2].Text);
    }

    private static TelemetrySnapshot Telemetry(int requests, int errors, int? p95, ProcessVitals? vitals) =>
        new(requests, requests, 0, errors, p95, 0, 0, 0, null, 0, Now) { Process = vitals };

    public static TheoryData<string, TelemetrySnapshot, (string Text, string Tone)[]> TelemetryArms => new()
    {
        {
            "no p95, no vitals", Telemetry(5, 0, null, null),
            [("5 req/min", RuntimeLineTone.Plain), ("0 errors", RuntimeLineTone.Plain)]
        },
        {
            "errors without vitals warn", Telemetry(5, 2, 120, null),
            [("5 req/min · p95 120 ms", RuntimeLineTone.Plain), ("2 errors", RuntimeLineTone.Warn)]
        },
        {
            "vitals without a value: no process line, no uptime", Telemetry(5, 0, null, new ProcessVitals(null, null, null, null, null, null, null)),
            [("5 req/min", RuntimeLineTone.Plain), ("0 errors", RuntimeLineTone.Plain)]
        },
        {
            "exceptions warn with no errors; the heap stands in for the working set", Telemetry(5, 0, null, new ProcessVitals(null, null, 96, null, null, 1, null)),
            [("5 req/min", RuntimeLineTone.Plain), ("0 errors · 1 exceptions/min", RuntimeLineTone.Warn), ("96 MB", RuntimeLineTone.Plain)]
        },
        {
            "uptime only: no process line, a recent restart warns", Telemetry(5, 0, null, new ProcessVitals(null, null, null, null, null, null, 60)),
            [("5 req/min", RuntimeLineTone.Plain), ("0 errors", RuntimeLineTone.Plain), ("restarted 1 min ago", RuntimeLineTone.Warn)]
        },
        {
            "every vital, up long enough to be muted", Telemetry(5, 0, 85, new ProcessVitals(41.7, 1433.6, 96, 41, 0, 0, 7300)),
            [("5 req/min · p95 85 ms", RuntimeLineTone.Plain), ("0 errors · 0 exceptions/min", RuntimeLineTone.Plain), ("CPU 42 % · 1.4 GB · 0 in flight", RuntimeLineTone.Plain), ("up 2 h", RuntimeLineTone.Muted)]
        },
    };

    /// <summary>
    /// <c>TelemetryLines</c>: between the version line and the role line, traffic, failures, the process line when a vital has a
    /// value, and the uptime. One reading has no trend, and in a topology without links the request count is plain words (no pieces).
    /// </summary>
    [Theory]
    [MemberData(nameof(TelemetryArms))]
    public void TheWebAppTilesTelemetryLinesFollowWhatTheAppReported(string arm, TelemetrySnapshot telemetry, (string Text, string Tone)[] lines)
    {
        var uat = Checked(200, ("primary", 200));
        uat.Deployables[0].Nodes[0].RecordTelemetry(telemetry);

        var tile = Tile(uat, "app_0");

        Assert.False(string.IsNullOrEmpty(arm));
        Assert.Equal([("version 2.4.21", RuntimeLineTone.Strong), .. lines, ("primary: serves traffic", RuntimeLineTone.Serving)], Words(tile));
        Assert.All(tile.Lines, line => Assert.Null(line.Trend));
        Assert.Null(tile.Lines[1].Parts);
    }
}
