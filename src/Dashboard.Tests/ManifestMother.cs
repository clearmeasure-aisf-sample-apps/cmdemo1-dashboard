namespace Dashboard.Tests;

/// <summary>
/// The ObjectMother of a manifest's edges: the one place the tests write the <c>&lt;from&gt;-to-&lt;to&gt;</c> id
/// convention the deployment builds a manifest on. A test that means an id breaking the convention calls the raw
/// five-part <see cref="RuntimeEdge" /> constructor instead.
/// </summary>
public static class ManifestEdges
{
    /// <summary>The id of the edge from one alias to another: the two aliases with <c>-to-</c> between them.</summary>
    public static string Id(string from, string to) => $"{from}-to-{to}";

    /// <summary>The edge of the given kind from one alias to another, its id the convention's.</summary>
    public static RuntimeEdge Between(string from, string to, RuntimeEdgeKind kind, int? priority = null) =>
        new(Id(from, to), from, to, kind, priority);

    /// <summary>The edge from a web app to a dependency of its deployable, as the deployment writes it: the line runs
    /// from the app to the dependency, but its id names the dependency first.</summary>
    public static RuntimeEdge ToDependency(string app, string dependency) =>
        new(Id(dependency, app), app, dependency, RuntimeEdgeKind.Dependency);
}

/// <summary>
/// The ObjectMother of a manifest's nodes: each returns the <see cref="RuntimeNode" /> the app declares, the parts
/// named by what they are rather than by the slot they land in, and the parts a test does not name left absent.
/// </summary>
public static class ManifestNodes
{
    /// <summary>The person at the left of every diagram: <c>browser</c>, named <c>Browser</c>.</summary>
    public static RuntimeNode Browser() => new("browser", RuntimeNodeKind.Person, "Browser");

    /// <summary>A Front Door in front of the deployable's web apps.</summary>
    public static RuntimeNode FrontDoor(string alias, string name, string url, string deployable) =>
        new(alias, RuntimeNodeKind.FrontDoor, name, new Uri(url), Deployable: deployable);

    /// <summary>A web app of a deployable, with its role and the region it is placed in.</summary>
    public static RuntimeNode WebApp(string alias, string name, string url, string deployable, string role, string region, string regionAlias) =>
        new(alias, RuntimeNodeKind.WebApp, name, new Uri(url), Deployable: deployable, Role: role, Region: region, RegionAlias: regionAlias);

    /// <summary>A web app with nothing but its alias and name.</summary>
    public static RuntimeNode WebApp(string alias, string name) => new(alias, RuntimeNodeKind.WebApp, name);

    /// <summary>A container app with nothing but its alias and name.</summary>
    public static RuntimeNode ContainerApp(string alias, string name) => new(alias, RuntimeNodeKind.ContainerApp, name);

    /// <summary>A SQL database, placed in a region when the test names one.</summary>
    public static RuntimeNode Database(string alias, string name, string? region = null, string? regionAlias = null) =>
        new(alias, RuntimeNodeKind.Sql, name, Region: region, RegionAlias: regionAlias);

    /// <summary>A dependency with nothing but its alias and name.</summary>
    public static RuntimeNode Dependency(string alias, string name) => new(alias, RuntimeNodeKind.Dependency, name);

    /// <summary>A dependency outside the subscription, as the deployment writes it: an <c>external</c> one of a
    /// deployable, checked through the named entry of the web apps' detailed health check.</summary>
    public static RuntimeNode ExternalDependency(string alias, string name, string deployable, string? healthCheck) =>
        new(alias, RuntimeNodeKind.Dependency, name, Deployable: deployable, HealthCheck: healthCheck, DependencyKind: "external");
}

/// <summary>The ObjectMother of a whole manifest: the environment, its nodes and edges, and the regions only when the
/// test places nodes in them.</summary>
public static class Manifests
{
    /// <summary>The manifest of the environment with the given nodes and edges and, unless named, no regions.</summary>
    public static RuntimeManifest Of(string environment, IReadOnlyList<RuntimeNode> nodes, IReadOnlyList<RuntimeEdge> edges, IReadOnlyList<RuntimeRegion>? regions = null) =>
        new(environment, nodes, regions ?? [], edges);
}
