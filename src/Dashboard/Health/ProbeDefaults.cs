namespace Dashboard.Health;

/// <summary>
/// The defaults every probe of the dashboard shares: a node's check (<see cref="NodeProber" />), the pinned versions
/// (<see cref="PinnedVersionsReader" />) and the cluster reading (<see cref="Cluster.ClusterReader" />) all wait as long.
/// </summary>
internal static class ProbeDefaults
{
    /// <summary>How long a probe may take to answer before it counts as no answer.</summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
}
