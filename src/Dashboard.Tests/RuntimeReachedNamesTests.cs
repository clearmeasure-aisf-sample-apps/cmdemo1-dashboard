using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// The list of app names a reachable tile's title speaks: two apps are named with a comma and space, one app is
/// named alone with no trailing separator. The names are built by <c>DependencyReadTitle</c> and
/// <c>DatabaseReachedTitle</c>, which keep their own words; the name list they both take is the same rule.
/// </summary>
public class RuntimeReachedNamesTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// A manifest with two web apps (with URLs matching the topology) and one dependency with a health check name.
    /// </summary>
    private static RuntimeManifest DependencyManifest() =>
        Manifests.Of(
            "uat",
            [
                new RuntimeNode("app1", RuntimeNodeKind.WebApp, "app1", new Uri("https://app1.example.net"), "ui"),
                new RuntimeNode("app2", RuntimeNodeKind.WebApp, "app2", new Uri("https://app2.example.net"), "ui"),
                new RuntimeNode("dep", RuntimeNodeKind.Dependency, "Weather API", HealthCheck: "Weather", DependencyKind: "external"),
            ],
            [
                ManifestEdges.ToDependency("app1", "dep"),
                ManifestEdges.ToDependency("app2", "dep"),
            ]);

    /// <summary>An environment with two web app nodes, both healthy, both with a detailed health check entry "Weather" in Healthy state.</summary>
    private static EnvironmentStatus WebAppsEnvironment()
    {
        var parsed = TopologyParser.Parse("""
            { "environments": [ { "name": "uat", "deployables": [
                { "name": "ui", "frontDoor": null, "nodes": [
                  { "name": "app1", "role": "primary", "url": "https://app1.example.net" },
                  { "name": "app2", "role": "standby", "url": "https://app2.example.net" } ] } ] } ] }
            """).Topology!.Environments[0];
        var environment = new EnvironmentStatus(parsed);
        var ui = environment.Deployables[0];
        var detail = HealthDetailTests.Detail(("Weather", "Healthy"));
        ui.Nodes[0].Record(new ProbeResult(HealthState.Healthy, 200, 40, Now, "2.4.21"));
        ui.Nodes[1].Record(new ProbeResult(HealthState.Healthy, 200, 40, Now, "2.4.21"));
        ui.Nodes[0].RecordHealthDetail(detail);
        ui.Nodes[1].RecordHealthDetail(detail);
        return environment;
    }

    /// <summary>A healthy container app probe.</summary>
    private static ContainerAppStatus HealthyProbe() =>
        new(ContainerAppState.Healthy, new ContainerAppAnswer("/health", true, 200, false, 85, Now.AddSeconds(-20), null, HealthyPayload: true), false);

    /// <summary>
    /// A manifest with container apps (with URLs) and one database, SQL edges from the apps to the database.
    /// </summary>
    private static RuntimeManifest ContainerAppsDatabaseManifest(params string[] appNames)
    {
        var nodes = new List<RuntimeNode>();
        var edges = new List<RuntimeEdge>();
        foreach (var name in appNames)
        {
            var alias = $"ca_{name}";
            nodes.Add(new RuntimeNode(alias, RuntimeNodeKind.ContainerApp, name, new Uri($"https://{name}.example.io")));
            edges.Add(ManifestEdges.Between(alias, "sqldb", RuntimeEdgeKind.Sql));
        }

        nodes.Add(ManifestNodes.Database("sqldb", "sqldb"));
        return Manifests.Of("uat", nodes, edges);
    }

    /// <summary>A probe dictionary mapping each container app's target to a healthy probe.</summary>
    private static Func<ContainerAppTarget, ContainerAppStatus?> ProbesFor(params string[] appNames) =>
        target => appNames.Any(name => string.Equals(target.Url.Host, $"{name}.example.io", StringComparison.Ordinal))
            ? HealthyProbe()
            : null;

    [Fact]
    public void ADependencyReachedByTwoWebAppsNamesBothInItsTitle()
    {
        var manifest = DependencyManifest();
        var environment = WebAppsEnvironment();

        var payload = RuntimePayloadBuilder.Build(manifest, environment, null, TimeZoneInfo.Utc);

        var tile = payload.Nodes.Single(tile => tile.Alias == "dep");
        Assert.Contains("app1, app2", tile.Title, StringComparison.Ordinal);
    }

    [Fact]
    public void ADatabaseReachedByTwoContainerAppsNamesBothInItsTitle()
    {
        var manifest = ContainerAppsDatabaseManifest("ca-one", "ca-two");

        var payload = RuntimePayloadBuilder.Build(manifest, null, null, TimeZoneInfo.Utc, containerApps: ProbesFor("ca-one", "ca-two"));

        var tile = payload.Nodes.Single(tile => tile.Alias == "sqldb");
        Assert.Contains("ca-one, ca-two", tile.Title, StringComparison.Ordinal);
    }

    [Fact]
    public void ADatabaseReachedByOneContainerAppNamesItAloneWithNoTrailingSeparator()
    {
        var manifest = ContainerAppsDatabaseManifest("ca-one");

        var payload = RuntimePayloadBuilder.Build(manifest, null, null, TimeZoneInfo.Utc, containerApps: ProbesFor("ca-one"));

        var tile = payload.Nodes.Single(tile => tile.Alias == "sqldb");
        Assert.Contains("ca-one", tile.Title, StringComparison.Ordinal);
        Assert.DoesNotContain("ca-one, ", tile.Title, StringComparison.Ordinal);
    }
}
