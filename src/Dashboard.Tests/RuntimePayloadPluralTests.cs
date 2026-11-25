namespace Dashboard.Tests;

/// <summary>
/// The words the runtime payload chooses where a number is compared with one, pinned so no such word choice is
/// written by hand in the builder again: the database's line when its apps' health checks pass (the web apps' and
/// the container apps'), the database's words for the container apps that use it, and an origin's words by its
/// priority.
/// </summary>
public class RuntimePayloadPluralTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private static readonly Uri Page = new("http://localhost:5210/");

    private static ProbeResult Answer() => new(HealthState.Healthy, 200, 40, Now, "2.4.21");

    // ------------------------------------------------------------ the database's line of its passed checks

    /// <summary>The diagram and environment of <paramref name="count"/> web apps that all check the database and pass.</summary>
    private static (RuntimeManifest Manifest, EnvironmentStatus Environment) WebApps(int count)
    {
        var nodes = string.Join(", ", Enumerable.Range(1, count)
            .Select(index => $$"""{ "name": "app{{index}}", "role": "primary", "url": "https://app{{index}}.example.net" }"""));
        var environment = new EnvironmentStatus(TopologyParser.Parse($$"""
            { "environments": [ { "name": "uat", "deployables": [ { "name": "ui", "frontDoor": null, "nodes": [ {{nodes}} ] } ] } ] }
            """).Topology!.Environments[0]);
        foreach (var node in environment.Deployables[0].Nodes)
        {
            node.Record(Answer());
        }

        var manifest = new RuntimeManifest(
            "uat",
            [
                new RuntimeNode("browser", RuntimeNodeKind.Person, "Browser"),
                .. Enumerable.Range(1, count).Select(index => new RuntimeNode($"app_{index}", RuntimeNodeKind.WebApp, $"app{index}", new Uri($"https://app{index}.example.net"), "ui")),
                new RuntimeNode("sqldb", RuntimeNodeKind.Sql, "sqldb-uat"),
            ],
            [],
            [.. Enumerable.Range(1, count).Select(index => new RuntimeEdge($"app_{index}-to-sqldb", $"app_{index}", "sqldb", RuntimeEdgeKind.Sql))]);
        return (manifest, environment);
    }

    [Theory]
    [InlineData(1, "health check of app1 passed")]
    [InlineData(2, "health checks of 2 web apps passed")]
    [InlineData(3, "health checks of 3 web apps passed")]
    public void TheDatabaseNamesTheOneWebAppThatPassesAndCountsTheRest(int count, string line)
    {
        var (manifest, environment) = WebApps(count);

        var payload = RuntimePayloadBuilder.Build(manifest, environment, Page, TimeZoneInfo.Utc);
        var database = payload.Nodes.Single(tile => tile.Alias == "sqldb");

        Assert.Equal(new RuntimeTileLine(line, "ok"), Assert.Single(database.Lines));
    }

    /// <summary>The diagram of a database that only <paramref name="count"/> container apps use.</summary>
    private static RuntimeManifest ContainerApps(int count) =>
        new(
            "uat",
            [
                .. Enumerable.Range(1, count).Select(index => new RuntimeNode($"ca_{index}", RuntimeNodeKind.ContainerApp, $"ca-app{index}", new Uri($"https://ca-app{index}.example.net"), $"app{index}")),
                new RuntimeNode("sqldb", RuntimeNodeKind.Sql, "sqldb-uat"),
            ],
            [],
            [.. Enumerable.Range(1, count).Select(index => new RuntimeEdge($"ca_{index}-to-sqldb", $"ca_{index}", "sqldb", RuntimeEdgeKind.Sql))]);

    private static ContainerAppStatus Probed(ContainerAppState state, string path = "/_healthcheck") =>
        new(state, new ContainerAppAnswer(path, state == ContainerAppState.Healthy, 200, false, 40, Now));

    [Theory]
    [InlineData(1, "health check of ca-app1 passed")]
    [InlineData(2, "health checks of 2 container apps passed")]
    [InlineData(3, "health checks of 3 container apps passed")]
    public void TheDatabaseNamesTheOneContainerAppThatPassesAndCountsTheRest(int count, string line)
    {
        var payload = RuntimePayloadBuilder.Build(ContainerApps(count), null, Page, TimeZoneInfo.Utc, containerApps: _ => Probed(ContainerAppState.Healthy));
        var database = payload.Nodes.Single(tile => tile.Alias == "sqldb");

        Assert.Equal(new RuntimeTileLine(line, "ok"), Assert.Single(database.Lines));
    }

    [Theory]
    [InlineData(1, "the container app that uses it")]
    [InlineData(2, "the container apps that use it")]
    [InlineData(3, "the container apps that use it")]
    public void TheDatabaseCountsTheContainerAppsThatAnsweredWithoutAHealthCheck(int count, string who)
    {
        var payload = RuntimePayloadBuilder.Build(ContainerApps(count), null, Page, TimeZoneInfo.Utc, containerApps: _ => Probed(ContainerAppState.Responding, "/liveness"));
        var database = payload.Nodes.Single(tile => tile.Alias == "sqldb");

        Assert.Contains($"{who} answered without a health check", database.Title, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------ the origin's words by its priority

    /// <summary>The diagram and environment of a Front Door in front of <paramref name="count"/> healthy origins: the first is the primary, the rest stand by.</summary>
    private static (RuntimeManifest Manifest, EnvironmentStatus Environment) Origins(int count)
    {
        var nodes = string.Join(", ", Enumerable.Range(1, count)
            .Select(index => $$"""{ "name": "origin{{index}}", "role": "{{(index == 1 ? "primary" : "standby")}}", "url": "https://origin{{index}}.example.net" }"""));
        var environment = new EnvironmentStatus(TopologyParser.Parse($$"""
            { "environments": [ { "name": "uat", "deployables": [ { "name": "ui", "frontDoor": "https://fd.example.net", "nodes": [ {{nodes}} ] } ] } ] }
            """).Topology!.Environments[0]);
        environment.Deployables[0].FrontDoor!.Record(Answer());
        foreach (var node in environment.Deployables[0].Nodes)
        {
            node.Record(Answer());
        }

        var manifest = new RuntimeManifest(
            "uat",
            [
                new RuntimeNode("browser", RuntimeNodeKind.Person, "Browser"),
                new RuntimeNode("fd_ui", RuntimeNodeKind.FrontDoor, "fd-uat", new Uri("https://fd.example.net"), "ui"),
                .. Enumerable.Range(1, count).Select(index => new RuntimeNode($"app_{index}", RuntimeNodeKind.WebApp, $"origin{index}", new Uri($"https://origin{index}.example.net"), "ui")),
            ],
            [],
            [.. Enumerable.Range(1, count).Select(index => new RuntimeEdge($"fd_ui-to-app_{index}", "fd_ui", $"app_{index}", RuntimeEdgeKind.Origin, index))]);
        return (manifest, environment);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void TheFirstPriorityServesFirstAndTheRestWaitForIt(int count)
    {
        var (manifest, environment) = Origins(count);

        var payload = RuntimePayloadBuilder.Build(manifest, environment, Page, TimeZoneInfo.Utc);

        for (var index = 1; index <= count; index++)
        {
            var expected = index == 1 ? "first, while healthy" : "when priority 1 is down";
            var edge = payload.Edges.Single(marked => marked.Id == $"fd_ui-to-app_{index}");
            Assert.Equal(expected, edge.Text);
            Assert.Contains($"origin priority {index}: {expected}", edge.Title, StringComparison.Ordinal);
        }
    }
}
