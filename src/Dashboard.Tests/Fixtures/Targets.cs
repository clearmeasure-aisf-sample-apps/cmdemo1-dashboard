using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// One home for the target tiles the tests stand on: a node of the topology and the public
/// address of a deployable, each said in the words the dashboard reads and writes.
/// </summary>
internal static class Targets
{
    /// <summary>The address no check is made to and no page shows: the one every tile below is built with.</summary>
    private static readonly Uri West = new("https://app-west.example.net");

    /// <summary>A primary node of the westus3 region, the tile the page shows most often.</summary>
    public static TargetStatus PrimaryNode() => Node("app-west", region: "westus3", role: NodeInfo.PrimaryRole);

    /// <summary>A standby node of the westus3 region, the tile the page shows when the primary falls over.</summary>
    public static TargetStatus StandbyNode() => Node("app-west", region: "westus3", role: NodeInfo.StandbyRole);

    /// <summary>
    /// A node tile with the name, region and role named; the address stands apart from them, as the topology file
    /// writes them.
    /// </summary>
    public static TargetStatus Node(string name, string? region = "westus3", string? role = NodeInfo.PrimaryRole) =>
        new(TargetKind.Node, name, new Uri($"https://{name}.example.net"), region, role);

    /// <summary>
    /// A node with the telemetry of the rounds named: one sample per check, a null where the node reported none.
    /// </summary>
    public static TargetStatus NodeWithTelemetry(params TelemetrySnapshot?[] samples)
    {
        var node = Node("uat-west");
        foreach (var sample in samples)
        {
            node.RecordTelemetry(sample);
        }

        return node;
    }

    /// <summary>The public address of a deployable, with no region, role or node behind it.</summary>
    public static TargetStatus FrontDoor() =>
        new(TargetKind.FrontDoor, TargetStatus.FrontDoorName, new Uri("https://demo.azurefd.net"));

    /// <summary>A tile of the kind named, built the way the enum names it.</summary>
    public static TargetStatus Of(TargetKind kind) => kind switch
    {
        TargetKind.Node => PrimaryNode(),
        TargetKind.FrontDoor => FrontDoor(),
        _ => new TargetStatus(kind, "app-west", West, "westus3", "primary"),
    };
}
