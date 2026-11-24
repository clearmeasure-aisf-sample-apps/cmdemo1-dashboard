namespace Dashboard.Health;

/// <summary>The system the dashboard shows: the content of <c>topology.json</c>.</summary>
/// <param name="Cluster">
/// The Kubernetes cluster the system runs in, for the cluster view; null for a system without one, which is then shown
/// without that view.
/// </param>
public sealed record Topology(SystemInfo System, DateTimeOffset? Generated, IReadOnlyList<EnvironmentInfo> Environments, ClusterInfo? Cluster = null)
{
    /// <summary>
    /// True when a deployable of any environment has a Front Door endpoint: the page's help then speaks of Front Door.
    /// </summary>
    public bool HasFrontDoor => Environments.Any(environment => environment.Deployables.Any(deployable => deployable.FrontDoor is not null));
}
