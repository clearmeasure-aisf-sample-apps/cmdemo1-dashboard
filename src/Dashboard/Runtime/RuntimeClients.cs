namespace Dashboard.Runtime;

/// <summary>
/// The node's clients: what stands at the other end of each edge of one kind that comes into the node, in the
/// manifest's order of its edges. The walk over the edges is this one; what a client is (a checked web app, a container
/// app) is the lookup the caller hands, which answers null for an end that is no client of its kind.
/// </summary>
/// <param name="Manifest">The manifest whose edges are walked.</param>
/// <param name="Node">The node the edges come into (the database, a dependency).</param>
/// <param name="Kind">The kind of relationship (SQL, dependency).</param>
public sealed record RuntimeClients(RuntimeManifest Manifest, RuntimeNode Node, RuntimeEdgeKind Kind)
{
    /// <summary>The aliases the edges of that kind into the node come from, in the manifest's order.</summary>
    public IEnumerable<string> Aliases =>
        Manifest.Edges
            .Where(edge => edge.Kind == Kind && edge.To == Node.Alias)
            .Select(edge => edge.From);

    /// <summary>The clients the lookup finds for those aliases; an alias it finds nothing for is left out.</summary>
    public IEnumerable<T> Of<T>(Func<string, T?> lookup)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(lookup);
        return Aliases.Select(lookup).OfType<T>();
    }
}
