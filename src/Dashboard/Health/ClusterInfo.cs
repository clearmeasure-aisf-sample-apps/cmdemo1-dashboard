namespace Dashboard.Health;

/// <summary>
/// The Kubernetes cluster of the system (<c>cluster</c> of <c>topology.json</c>): the two public files the cluster
/// view reads, and where the cluster is in the Azure portal. Every part is optional.
/// </summary>
/// <param name="Name">The cluster's name; null when the topology does not say.</param>
/// <param name="StatusUrl">
/// Where the browser reads the live status a collector inside the cluster writes (<c>cluster.json</c>): nodes,
/// namespaces, pods and volumes.
/// </param>
/// <param name="ServiceUrl">
/// Where the browser reads Azure's own facts about the AKS service (<c>aks.json</c>), which a scheduled workflow
/// publishes.
/// </param>
/// <param name="Links">Where the cluster is in the Azure portal (<see cref="LinkSet"/>); null without links.</param>
public sealed record ClusterInfo(string? Name, Uri? StatusUrl = null, Uri? ServiceUrl = null, LinkSet? Links = null);
