namespace Dashboard.Health;

/// <summary>
/// An index over the environments of a topology keyed by their Kubernetes namespace, built once for the cluster
/// view and the cluster event detector. Keeps the first environment named when two environments share a namespace,
/// as the detector's words depend on that decision.
/// </summary>
public sealed record EnvironmentIndex(
    IReadOnlyList<EnvironmentInfo> Named,
    HashSet<string> Namespaces,
    Dictionary<string, string> NameForNamespace)
{
    /// <param name="environments">The environments of the topology, any order.</param>
    public static EnvironmentIndex Of(IEnumerable<EnvironmentInfo> environments)
    {
        ArgumentNullException.ThrowIfNull(environments);
        var named = environments.Where(environment => environment.Namespace is not null).ToList();
        var namespaces = named.Select(environment => environment.Namespace!).ToHashSet(StringComparer.Ordinal);
        var nameForNamespace = named
            .GroupBy(environment => environment.Namespace!, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First().Name, StringComparer.Ordinal);
        return new EnvironmentIndex(named, namespaces, nameForNamespace);
    }
}