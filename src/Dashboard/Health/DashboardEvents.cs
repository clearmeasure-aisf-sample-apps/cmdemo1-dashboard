namespace Dashboard.Health;

public enum EventKind
{
    /// <summary>An endpoint's health state changed.</summary>
    Health,

    /// <summary>An entry of a web app's detailed health check changed its state.</summary>
    HealthCheck,

    /// <summary>A web app's process started again.</summary>
    Restart,

    /// <summary>A web app reports another version: a deployment.</summary>
    Version,

    /// <summary>
    /// The nodes expected to serve a deployable's traffic changed: a failover or a failback, or a node left a rotation
    /// or is back in it.
    /// </summary>
    Serving,

    /// <summary>The version pinned in Git changed.</summary>
    Pinned,

    /// <summary>The traffic button was started or stopped.</summary>
    Traffic,

    /// <summary>Something changed in the cluster the system runs in: its status file, a node, a pod, the AKS service.</summary>
    Cluster,
}

public enum EventLevel
{
    Info,
    Good,
    Warning,
    Problem,
}

/// <summary>Something this page observed: when, where and in words.</summary>
/// <param name="Environment">The environment; null for an event of the page itself.</param>
/// <param name="Node">
/// The node (its region, or its name), "Front Door", or the deployable for an event of all its nodes; "cluster" or
/// "AKS" for an event of the cluster view.
/// </param>
public sealed record DashboardEvent(DateTimeOffset At, EventKind Kind, EventLevel Level, string? Environment, string? Node, string Text);

/// <summary>What a round of the cluster view says, at what level: the words of a <see cref="DashboardEvent"/> before it has a time and a place.</summary>
internal sealed record ClusterNotice(EventLevel Level, string Text);
