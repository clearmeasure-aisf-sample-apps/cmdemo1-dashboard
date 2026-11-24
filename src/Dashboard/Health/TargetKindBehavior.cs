namespace Dashboard.Health;

/// <summary>
/// What a <see cref="TargetKind" /> means for a check, answered once per kind: whether its tile reads the node's
/// telemetry, whether it reads the detailed health check, whether its events are a node's, and what the tile and its
/// events call it. A third kind is declared here beside the two, answering every question, and
/// <see cref="Declared" /> lists them all.
/// </summary>
/// <param name="Kind">The kind the answers are for.</param>
/// <param name="ReadsTelemetry">A regional node also reports its calls; a Front Door address would only answer for the node behind it.</param>
/// <param name="ReadsHealthDetail">The detailed health check is read from the node itself, never through the public address.</param>
/// <param name="IsNode">Whether the events of the tile are a node's.</param>
/// <param name="FixedLabel">The one word the tile is called whatever its name and region; null to call it by its region or else its name.</param>
public sealed record TargetKindBehavior(TargetKind Kind, bool ReadsTelemetry, bool ReadsHealthDetail, bool IsNode, string? FixedLabel)
{
    /// <summary>The public address of a deployable: probed only, and called <see cref="TargetStatus.FrontDoorName" />.</summary>
    public static readonly TargetKindBehavior FrontDoor = new(
        TargetKind.FrontDoor, ReadsTelemetry: false, ReadsHealthDetail: false, IsNode: false, FixedLabel: TargetStatus.FrontDoorName);

    /// <summary>A regional web app: probed, its telemetry and detailed health read, called by its region or name.</summary>
    public static readonly TargetKindBehavior Node = new(
        TargetKind.Node, ReadsTelemetry: true, ReadsHealthDetail: true, IsNode: true, FixedLabel: null);

    /// <summary>Every kind the enum declares, each with its answers.</summary>
    public static IReadOnlyList<TargetKindBehavior> Declared { get; } = [FrontDoor, Node];

    /// <summary>
    /// The answers for a kind. A number the enum does not name reads nothing but the probe, is no node and is called
    /// by its region or name - what a tile of no declared kind has always been.
    /// </summary>
    public static TargetKindBehavior For(TargetKind kind) => kind switch
    {
        TargetKind.FrontDoor => FrontDoor,
        TargetKind.Node => Node,
        _ => new TargetKindBehavior(kind, ReadsTelemetry: false, ReadsHealthDetail: false, IsNode: false, FixedLabel: null),
    };

    /// <summary>What a tile of this kind with the given name and region is called.</summary>
    public string LabelFor(string name, string? region) => FixedLabel ?? TargetStatus.NodeLabel(name, region);
}
