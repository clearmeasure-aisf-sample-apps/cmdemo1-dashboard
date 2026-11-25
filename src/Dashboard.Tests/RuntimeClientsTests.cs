using Dashboard.Runtime;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="RuntimeClients" />: the one walk over a manifest's edges for a node's clients, which the database tile
/// (its checked web apps, its container apps) and the dependency tile ask (#1789).
/// </summary>
public class RuntimeClientsTests
{
    private static readonly RuntimeNode Database = new("db", RuntimeNodeKind.Sql, "sql-db");
    private static readonly RuntimeNode Dependency = new("ext", RuntimeNodeKind.Dependency, "External");

    private static RuntimeManifest Manifest() => new(
        "tst",
        [
            new RuntimeNode("web1", RuntimeNodeKind.WebApp, "app-1"),
            new RuntimeNode("web2", RuntimeNodeKind.WebApp, "app-2"),
            new RuntimeNode("job", RuntimeNodeKind.ContainerApp, "ca-job"),
            Database,
            Dependency,
        ],
        [],
        [
            new RuntimeEdge("web2-to-db", "web2", "db", RuntimeEdgeKind.Sql),
            new RuntimeEdge("web1-to-ext", "web1", "ext", RuntimeEdgeKind.Dependency),
            new RuntimeEdge("job-to-db", "job", "db", RuntimeEdgeKind.Sql),
            new RuntimeEdge("web1-to-db", "web1", "db", RuntimeEdgeKind.Sql),
            new RuntimeEdge("db-to-web1", "db", "web1", RuntimeEdgeKind.Sql),
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
