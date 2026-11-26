using Dashboard.Runtime;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="RuntimeClients" />: the one walk over a manifest's edges for a node's clients, which the database tile
/// (its checked web apps, its container apps) and the dependency tile ask (#1789).
/// </summary>
public class RuntimeClientsTests
{
    private static readonly RuntimeNode Database = ManifestNodes.Database("db", "sql-db");
    private static readonly RuntimeNode Dependency = ManifestNodes.Dependency("ext", "External");

    /// <summary>The last edge keeps the raw constructor on purpose: its id breaks the <c>-to-</c> convention, so a
    /// second edge between the same two aliases stays a line of its own.</summary>
    private static RuntimeManifest Manifest() => Manifests.Of(
        "tst",
        [
            ManifestNodes.WebApp("web1", "app-1"),
            ManifestNodes.WebApp("web2", "app-2"),
            ManifestNodes.ContainerApp("job", "ca-job"),
            Database,
            Dependency,
        ],
        [
            ManifestEdges.Between("web2", "db", RuntimeEdgeKind.Sql),
            ManifestEdges.Between("web1", "ext", RuntimeEdgeKind.Dependency),
            ManifestEdges.Between("job", "db", RuntimeEdgeKind.Sql),
            ManifestEdges.Between("web1", "db", RuntimeEdgeKind.Sql),
            ManifestEdges.Between("db", "web1", RuntimeEdgeKind.Sql),
            new RuntimeEdge("web2-to-db-other", "web2", "db", RuntimeEdgeKind.Other),
        ]);

    [Fact]
    public void TheAliasesAreTheFromsOfTheEdgesOfThatKindIntoTheNodeInTheManifestsOrder()
    {
        Assert.Equal(["web2", "job", "web1"], new RuntimeClients(Manifest(), Database, RuntimeEdgeKind.Sql).Aliases);
        Assert.Equal(["web1"], new RuntimeClients(Manifest(), Dependency, RuntimeEdgeKind.Dependency).Aliases);
    }

    [Fact]
    public void AnEdgeOfAnotherKindOrOutOfTheNodeIsNoClient()
    {
        Assert.Empty(new RuntimeClients(Manifest(), Dependency, RuntimeEdgeKind.Sql).Aliases);
        Assert.Equal(["web2"], new RuntimeClients(Manifest(), Database, RuntimeEdgeKind.Other).Aliases);
    }

    [Fact]
    public void OfKeepsWhatTheLookupFindsAndLeavesOutTheRest()
    {
        var manifest = Manifest();
        var containerApps = new RuntimeClients(manifest, Database, RuntimeEdgeKind.Sql)
            .Of(alias => manifest.Nodes.FirstOrDefault(node => node.Alias == alias && node.Kind == RuntimeNodeKind.ContainerApp));

        Assert.Equal(["job"], containerApps.Select(node => node.Alias));
    }

    [Fact]
    public void OfALookupThatFindsNothingIsEmpty() =>
        Assert.Empty(new RuntimeClients(Manifest(), Database, RuntimeEdgeKind.Sql).Of<string>(_ => null));

    [Fact]
    public void OfRefusesNoLookup() =>
        Assert.Throws<ArgumentNullException>(() => new RuntimeClients(Manifest(), Database, RuntimeEdgeKind.Sql).Of<string>(null!));
}
