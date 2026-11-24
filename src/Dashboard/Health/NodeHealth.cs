namespace Dashboard.Health;

/// <summary>The state of one regional node, as the serving decision needs it.</summary>
public sealed record NodeHealth(string Name, string? Region, bool IsPrimary, HealthState State)
{
    /// <summary>The region, or the node's name when the topology gives no region.</summary>
    public string Label => TargetStatus.NodeLabel(Name, Region);
}
