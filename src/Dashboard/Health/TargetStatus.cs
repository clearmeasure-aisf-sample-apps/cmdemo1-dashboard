namespace Dashboard.Health;

/// <summary>One tile of the dashboard: an endpoint, its last check and the checks before it.</summary>
public sealed class TargetStatus(TargetKind kind, string name, Uri url, string? region = null, string? role = null, LinkSet? links = null)
{
    public const int HistoryLength = 30;

    /// <summary>The one word for a deployable's public address: the name and the label of its tile.</summary>
    public const string FrontDoorName = "Front Door";

    /// <summary>The tile of a node of the topology: its name, region, role, address and links, each from its own part.</summary>
    public static TargetStatus ForNode(NodeInfo node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return new TargetStatus(TargetKind.Node, node.Name, node.Url, region: node.Region, role: node.Role, links: node.Links);
    }

    /// <summary>
    /// The tile of a deployable's Front Door endpoint, with the deployable's links; null when the deployable has none.
    /// </summary>
    public static TargetStatus? ForFrontDoor(DeployableInfo deployable)
    {
        ArgumentNullException.ThrowIfNull(deployable);
        return deployable.FrontDoor is null
            ? null
            : new TargetStatus(TargetKind.FrontDoor, FrontDoorName, deployable.FrontDoor, links: deployable.Links);
    }

    /// <summary>Where the tile's numbers lead: a node's own links; for the Front Door endpoint, the deployable's.</summary>
    public LinkSet Links { get; } = links ?? LinkSet.None;

    public TargetKind Kind { get; } = kind;

    public string Name { get; } = name;

    public Uri Url { get; } = url;

    public string? Region { get; } = region;

    public string? Role { get; } = role;

    public bool IsPrimary => NodeRole.IsPrimary(Role);

    /// <summary>What the tile and its events call it: <see cref="FrontDoorName"/> for the endpoint, a node's region or else its name.</summary>
    public string Label => Kind == TargetKind.FrontDoor ? FrontDoorName : NodeLabel(Name, Region);

    /// <summary>The word a node answers to: the region the topology wrote, or the node's own name when it wrote none.</summary>
    public static string NodeLabel(string name, string? region) => region ?? name;

    /// <summary>The last <see cref="HistoryLength"/> checks, oldest first.</summary>
    public HistoryBuffer<ProbeResult> History { get; } = new(HistoryLength);

    public ProbeResult? Last { get; private set; }

    public HealthState State => Last?.State ?? HealthState.Pending;

    /// <summary>The version the endpoint last reported: a node that stops answering keeps its last known version.</summary>
    public string? Version { get; private set; }

    /// <summary>The node's own count of its calls at the last check; null when it reported none.</summary>
    public TelemetrySnapshot? Telemetry { get; private set; }

    /// <summary>
    /// The entries of the node's detailed health check at the last check; null when it was not read (the deployable
    /// has no <c>healthDetailPath</c>, the probe is Liveness) or gave no entries. Never the answer of an earlier check.
    /// </summary>
    public HealthDetail? HealthDetail { get; private set; }

    /// <summary>
    /// The last answer of the detailed health check that had entries: what a new answer is compared with to find an
    /// entry that changed its state. Null until the node answered one.
    /// </summary>
    public HealthDetail? LastReadHealthDetail { get; private set; }

    /// <summary>Records the detailed health check of one check; null replaces what an earlier check read.</summary>
    public void RecordHealthDetail(HealthDetail? detail)
    {
        HealthDetail = detail;
        LastReadHealthDetail = detail ?? LastReadHealthDetail;
    }

    /// <summary>
    /// The last <see cref="Trend.Length"/> readings of the node's telemetry, oldest first, for the sparklines: one per
    /// check, null where the node reported none.
    /// </summary>
    public HistoryBuffer<TelemetrySnapshot?> Samples { get; } = new(Trend.Length);

    /// <summary>Records the telemetry of one check. A node that never reported any keeps no readings.</summary>
    public void RecordTelemetry(TelemetrySnapshot? telemetry)
    {
        Telemetry = telemetry;
        if (telemetry is not null || Samples.Count > 0)
        {
            Samples.Add(telemetry);
        }
    }

    /// <summary>The trend of one number of the telemetry over the readings kept; null with fewer than two.</summary>
    public Trend? TrendOf(Func<TelemetrySnapshot, double?> number, string what, string unit = "")
    {
        ArgumentNullException.ThrowIfNull(number);
        return Trend.Of(Samples.Select(sample => sample is null ? null : number(sample)), what, unit);
    }

    public void Record(ProbeResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        Last = result;
        Version = result.Version ?? Version;
        History.Add(result);
    }

    public NodeHealth ToNodeHealth() => new(Name, Region, IsPrimary, State);

    public NodeVersion ToNodeVersion() => new(Label, State, Version);
}
