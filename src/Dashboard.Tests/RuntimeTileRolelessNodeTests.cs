using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// Characterization of <c>RoleLine</c> for a node that names no role (a <c>null</c> <see cref="TargetStatus.Role"/>, which
/// the topology never hands out but a hand-built node can carry): such a node is not primary
/// (<see cref="NodeRole.IsPrimary"/> of <c>null</c> is false), so its line says it as the standby in every state.
/// </summary>
public class RuntimeTileRolelessNodeTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private static readonly Uri FrontDoorUrl = new("https://fd-uat.example.net");
    private static readonly string[] Regions = ["westus3", "eastus2"];

    /// <summary>What a check found: an HTTP status, <see cref="NoAnswer"/>, or null for not checked yet.</summary>
    private const int NoAnswer = 0;

    /// <summary>uat with the deployable ui behind a Front Door endpoint: a primary node, then a node with no role.</summary>
    private static EnvironmentStatus Uat()
    {
        var parsed = TopologyParser.Parse($$"""
            { "environments": [ { "name": "uat", "deployables": [ {
                "name": "ui", "frontDoor": "{{FrontDoorUrl}}", "nodes": [
                  { "name": "uat-0", "region": "{{Regions[0]}}", "role": "primary", "url": "https://uat-0.example.net" },
                  { "name": "uat-1", "region": "{{Regions[1]}}", "role": "standby", "url": "https://uat-1.example.net" } ] } ] } ] }
            """).Topology!.Environments[0];
        var deployable = parsed.Deployables[0];
        var roleless = deployable with { Nodes = [deployable.Nodes[0], deployable.Nodes[1] with { Role = null! }] };
        return new EnvironmentStatus(parsed with { Deployables = [roleless] });
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

    private static RuntimeTile RolelessTile(int? status)
    {
        var uat = Uat();
        var deployable = uat.Deployables[0];
        Record(deployable.FrontDoor!, 200);
        Record(deployable.Nodes[0], 200);
        Record(deployable.Nodes[1], status);
        var nodes = deployable.Nodes.Select((node, index) => new RuntimeNode(
            $"app_{index}", RuntimeNodeKind.WebApp, $"uat-{index}", node.Url, "ui", node.Role, Regions[index], $"region_{index}"));
        var frontDoor = new RuntimeNode("fd_ui", RuntimeNodeKind.FrontDoor, "fd-uat", FrontDoorUrl, "ui");
        var payload = RuntimePayloadBuilder.Build(new RuntimeManifest("uat", [frontDoor, .. nodes], [], []), uat, null, TimeZoneInfo.Utc);
        return payload.Nodes.Single(tile => tile.Alias == "app_1");
    }

    [Fact]
    public void TheNodeHasNoRoleAndIsNotPrimary()
    {
        var node = Uat().Deployables[0].Nodes[1];

        Assert.Null(node.Role);
        Assert.False(node.IsPrimary);
    }

    [Theory]
    [InlineData(200, "standby: ready, no traffic", RuntimeLineTone.Muted)]
    [InlineData(null, "standby", RuntimeLineTone.Muted)]
    [InlineData(503, "standby: not serving", RuntimeLineTone.Plain)]
    [InlineData(NoAnswer, "standby: not serving", RuntimeLineTone.Plain)]
    public void ANodeThatNamesNoRoleIsSaidAsTheStandby(int? status, string line, string tone)
    {
        var tile = RolelessTile(status);

        Assert.Equal(line, tile.Lines[^1].Text);
        Assert.Equal(tone, tile.Lines[^1].Tone);
        Assert.StartsWith(NodeInfo.StandbyRole, tile.Lines[^1].Text, StringComparison.Ordinal);
    }
}
