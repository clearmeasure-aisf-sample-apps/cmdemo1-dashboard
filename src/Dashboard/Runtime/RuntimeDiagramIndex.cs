namespace Dashboard.Runtime;

/// <summary>
/// The manifest's lookups the runtime payload asks over and over, built once per update (#1510): the node of an
/// alias, what the monitor checked of it, the Front Door endpoint's origin edges, the number of dependencies a web app
/// calls, and the web apps of each region. Each answers what the linear scan it replaces answered: the first node of
/// an alias, the last entry found for one, the edges and nodes in the manifest's order.
/// </summary>
/// <typeparam name="TEntry">What the monitor checked of a node (the builder's own entry).</typeparam>
internal sealed class RuntimeDiagramIndex<TEntry>
    where TEntry : class
{
    private readonly Dictionary<string, RuntimeNode> nodes = new(StringComparer.Ordinal);
    private readonly Dictionary<string, RuntimeNode> containerApps = new(StringComparer.Ordinal);
    private readonly Dictionary<string, TEntry> entries = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<RuntimeEdge>> origins = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> dependencies = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<RuntimeNode>> webApps = new(StringComparer.Ordinal);

    /// <param name="manifest">The environment's manifest.</param>
    /// <param name="find">What the monitor checked of a node; null for a node it did not check.</param>
    public RuntimeDiagramIndex(RuntimeManifest manifest, Func<RuntimeNode, TEntry?> find)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(find);
        Manifest = manifest;
        foreach (var node in manifest.Nodes)
        {
            nodes.TryAdd(node.Alias, node);
            if (node.Kind == RuntimeNodeKind.ContainerApp)
            {
                containerApps.TryAdd(node.Alias, node);
            }

            if (find(node) is { } entry)
            {
                entries[node.Alias] = entry;
            }

            if (node.Kind == RuntimeNodeKind.WebApp && node.RegionAlias is { } region)
            {
                Add(webApps, region, node);
            }
        }

        foreach (var edge in manifest.Edges)
        {
            if (edge.Kind == RuntimeEdgeKind.Origin)
            {
                Add(origins, edge.From, edge);
            }
            else if (edge.Kind == RuntimeEdgeKind.Dependency)
            {
                dependencies[edge.From] = dependencies.GetValueOrDefault(edge.From) + 1;
            }
        }
    }

    /// <summary>The manifest the index was built from.</summary>
    public RuntimeManifest Manifest { get; }

    /// <summary>The first node of the manifest with the alias; null when it has none.</summary>
    public RuntimeNode? Node(string alias) => nodes.GetValueOrDefault(alias);

    /// <summary>The first container app of the manifest with the alias; null when it has none.</summary>
    public RuntimeNode? ContainerApp(string alias) => containerApps.GetValueOrDefault(alias);

    /// <summary>What the monitor checked of the node with the alias; null when it checked none.</summary>
    public TEntry? Entry(string alias) => entries.GetValueOrDefault(alias);

    /// <summary>The origin edges out of the Front Door endpoint with the alias, in the manifest's order.</summary>
    public IReadOnlyList<RuntimeEdge> OriginsOf(string alias) => origins.GetValueOrDefault(alias) ?? [];

    /// <summary>How many dependency edges go out of the node with the alias.</summary>
    public int DependenciesFrom(string alias) => dependencies.GetValueOrDefault(alias);

    /// <summary>The web apps of the region with the alias, in the manifest's order.</summary>
    public IReadOnlyList<RuntimeNode> WebAppsIn(string region) => webApps.GetValueOrDefault(region) ?? [];

    private static void Add<T>(Dictionary<string, List<T>> lists, string key, T item)
    {
        if (!lists.TryGetValue(key, out var list))
        {
            list = [];
            lists[key] = list;
        }

        list.Add(item);
    }
}
