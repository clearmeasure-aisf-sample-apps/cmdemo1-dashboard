namespace Dashboard.Health;

/// <summary>What one regional node says about its version, as the comparison with the pinned version needs it.</summary>
/// <param name="Label">The region, or the node's name when the topology gives no region.</param>
/// <param name="State">The node's last check.</param>
/// <param name="Version">The version the node last reported.</param>
public sealed record NodeVersion(string Label, HealthState State, string? Version)
{
    /// <summary>
    /// The version the node runs now, without build metadata; null when that is not known. A node that did not answer
    /// its last check is not known, although the dashboard remembers the version it reported before.
    /// </summary>
    public string? Running => State is HealthState.Healthy or HealthState.Unhealthy ? VersionText.Display(Version) : null;

    /// <summary>Why the running version is not known, as the end of a sentence that starts with the label.</summary>
    public string UnknownReason => State switch
    {
        HealthState.Pending => "is being checked",
        HealthState.Unreachable => "is unreachable",
        _ => "reports no version",
    };

    /// <summary>Why the running version is not known, as a whole clause: <c>eastus2 is unreachable</c>.</summary>
    public string UnknownClause => $"{Label} {UnknownReason}";
}
