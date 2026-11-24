namespace Dashboard.Health;

/// <param name="Name">The environment's name.</param>
/// <param name="Tier">The tier, for example <c>nonprod</c>.</param>
/// <param name="Deployables">The deployables the dashboard checks in this environment.</param>
/// <param name="VersionsUrl">
/// Where the browser reads the environment's <c>versions.json</c>: the versions the deployments pinned in Git.
/// </param>
/// <param name="VersionsHistoryUrl">The page with the history of that file.</param>
/// <param name="Links">Where the environment's resources are (<see cref="LinkSet"/>); null without links.</param>
/// <param name="Namespace">
/// The namespace of the cluster that holds the environment's pods: the cluster view lists its pods under the
/// environment. Null where the system runs in no cluster.
/// </param>
/// <param name="Pages">Pages of the system itself in this environment (<see cref="PageLink"/>); null without any.</param>
public sealed record EnvironmentInfo(
    string Name,
    string? Tier,
    IReadOnlyList<DeployableInfo> Deployables,
    Uri? VersionsUrl = null,
    Uri? VersionsHistoryUrl = null,
    LinkSet? Links = null,
    string? Namespace = null,
    IReadOnlyList<PageLink>? Pages = null);
