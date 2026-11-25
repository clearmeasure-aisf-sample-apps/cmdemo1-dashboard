namespace Dashboard.Tests;

/// <summary>
/// The four arms of the builder that share its private <c>UnprobedUnder</c> (#1709): a database whose web apps only run
/// the Liveness probe, a database no checked app uses, a database whose container apps answered without a health check,
/// and a container app with no address the browser can ask. Each says what Resource Health said of it when the manifest
/// gives it ids to be found by, with the reason this page worked out behind its name; each says its own not-probed line
/// (or, for the app without an address, "No address") when the manifest gives it none. The reason-free case
/// (<c>UnprobedNodeTile</c>, #1671) keeps the bare name as its title, so the shared helper adds no colon there.
/// </summary>
public class RuntimePayloadUnprobedUnderTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private static readonly Uri Page = new("http://localhost:5210/");

    private const string Group = "/subscriptions/0000/resourceGroups/rg-demo-uat/providers";
    private const string SqlId = Group + "/Microsoft.Sql/servers/sql-demo/databases/sqldb-demo-uat";
    private const string VaultId = Group + "/Microsoft.KeyVault/vaults/kv-demo";
    private const string DatabaseName = "sqldb-demo-uat";

    /// <summary>What Resource Health said of the database and a vault, read 40 minutes before <see cref="Now"/>.</summary>
    private static ResourceHealthReport Readings() => ResourceHealthReport.Parse($$"""
        { "generated": "2026-10-04T21:20:00Z", "resources": { "uat": [
            { "id": "{{SqlId}}", "health": { "status": "degraded", "source": "resourceHealth", "checkedAt": "2026-10-04T21:20:00Z" } },
            { "id": "{{VaultId}}", "health": { "status": "available", "source": "resourceHealth", "checkedAt": "2026-10-04T21:20:00Z" } } ] } }
        """)!;

    private static readonly string[] DatabaseIds = [SqlId];
    private static readonly string[] VaultIds = [VaultId];

    private static RuntimeTile Tile(
        RuntimeManifest manifest,
        string alias,
        ResourceHealthReport? report,
        EnvironmentStatus? environment = null,
        Func<ContainerAppTarget, ContainerAppStatus?>? probes = null) =>
        RuntimePayloadBuilder.Build(manifest, environment, Page, TimeZoneInfo.Utc, now: Now, containerApps: probes, resources: report)
            .Nodes.Single(tile => tile.Alias == alias);

    private static RuntimeNode Database(string? id) =>
        new("sqldb", RuntimeNodeKind.Sql, DatabaseName, Ids: id is null ? null : [id]);

    // ------------------------------------------- the database whose web apps only run the Liveness probe

    /// <summary>The diagram and environment of one web app that checks the database and answers with the Liveness probe.</summary>
    private static (RuntimeManifest Manifest, EnvironmentStatus Environment) Liveness(string? databaseId)
    {
        var environment = new EnvironmentStatus(TopologyParser.Parse("""
            { "environments": [ { "name": "uat", "deployables": [ { "name": "ui", "frontDoor": null, "nodes": [
                { "name": "app-demo-uat", "role": "primary", "url": "https://app-demo-uat.example.net" } ] } ] } ] }
            """).Topology!.Environments[0]);
        environment.Deployables[0].Nodes[0].Record(
            new ProbeResult(HealthState.Healthy, 200, 40, Now, "2.4.21") with { Probe = ProbeKind.Liveness });

        var manifest = new RuntimeManifest(
            "uat",
            [
                new RuntimeNode("app_ui", RuntimeNodeKind.WebApp, "app-demo-uat", new Uri("https://app-demo-uat.example.net"), "ui"),
                Database(databaseId),
            ],
            [],
            [new RuntimeEdge("app_ui-to-sqldb", "app_ui", "sqldb", RuntimeEdgeKind.Sql)]);
        return (manifest, environment);
    }

    private const string LivenessWhy =
        "the probe is Liveness, which does not connect to the database. Choose Health check to see whether it answers.";

    [Fact]
    public void ADatabaseOfLivenessProbesSaysWhatResourceHealthSaidBehindItsReason()
    {
        var (manifest, environment) = Liveness(SqlId);

        var database = Tile(manifest, "sqldb", Readings(), environment);

        Assert.Equal(("neutral", "Degraded"), (database.State, database.Label));
        Assert.Equal([("Resource Health, 40 min ago", "plain")], database.Lines.Select(line => (line.Text, line.Tone)));
        Assert.StartsWith($"{DatabaseName}: {LivenessWhy}\n", database.Title, StringComparison.Ordinal);
    }

    [Fact]
    public void ADatabaseOfLivenessProbesWithoutIdsSaysTheProbeLeavesItAlone()
    {
        var (manifest, environment) = Liveness(null);

        var database = Tile(manifest, "sqldb", Readings(), environment);

        Assert.Equal(("neutral", "Not probed"), (database.State, database.Label));
        Assert.Equal([("probe Liveness leaves it alone", "muted")], database.Lines.Select(line => (line.Text, line.Tone)));
        Assert.Equal($"{DatabaseName}: {LivenessWhy}", database.Title);
    }

    // ------------------------------------------- the database no checked app uses

    private static RuntimeManifest NoAppUses(string? databaseId) => new("uat", [Database(databaseId)], [], []);

    private const string UnprobedWhy =
        "the database takes no call from a browser, and this page checks no web app that uses it.";

    [Fact]
    public void ADatabaseNoCheckedAppUsesSaysWhatResourceHealthSaidBehindItsReason()
    {
        var database = Tile(NoAppUses(SqlId), "sqldb", Readings());

        Assert.Equal(("neutral", "Degraded"), (database.State, database.Label));
        Assert.StartsWith($"{DatabaseName}: {UnprobedWhy}\n", database.Title, StringComparison.Ordinal);
    }

    [Fact]
    public void ADatabaseNoCheckedAppUsesWithoutIdsSaysItIsNotProbedFromTheBrowser()
    {
        var database = Tile(NoAppUses(null), "sqldb", Readings());

        Assert.Equal(("neutral", "Not probed"), (database.State, database.Label));
        Assert.Equal([("not probed from the browser", "muted")], database.Lines.Select(line => (line.Text, line.Tone)));
        Assert.Equal($"{DatabaseName}: {UnprobedWhy}", database.Title);
    }

    /// <summary>Ids but no file: still Resource Health's words, and still the reason behind the name.</summary>
    [Fact]
    public void ADatabaseWithIdsAndNoReadingKeepsItsReasonBehindTheName()
    {
        var database = Tile(NoAppUses(SqlId), "sqldb", null);

        Assert.Equal(("neutral", "Not known"), (database.State, database.Label));
        Assert.Equal([("no Resource Health read yet", "muted")], database.Lines.Select(line => (line.Text, line.Tone)));
        Assert.StartsWith($"{DatabaseName}: {UnprobedWhy}\n", database.Title, StringComparison.Ordinal);
    }

    // ------------------------------------------- the database whose container apps answered without a health check

    private static RuntimeManifest ContainerAppsUse(string? databaseId) =>
        new(
            "uat",
            [
                new RuntimeNode("ca_scorecard", RuntimeNodeKind.ContainerApp, "ca-demo-uat-scorecard", new Uri("https://ca-demo-uat-scorecard.example.net"), "scorecard"),
                Database(databaseId),
            ],
            [],
            [new RuntimeEdge("ca_scorecard-to-sqldb", "ca_scorecard", "sqldb", RuntimeEdgeKind.Sql)]);

    private static ContainerAppStatus Answered(ContainerAppState state, string path) =>
        new(state, new ContainerAppAnswer(path, state == ContainerAppState.Healthy, 200, false, 40, Now));

    private const string NotReadWhy =
        "the container app that uses it answered without a health check (the probe was Liveness, or the app does not "
        + "allow this page's origin), so this page cannot tell whether the database answers.";

    [Fact]
    public void ADatabaseWhoseContainerAppsAnsweredWithoutAHealthCheckSaysWhatResourceHealthSaid()
    {
        var database = Tile(
            ContainerAppsUse(SqlId), "sqldb", Readings(), probes: _ => Answered(ContainerAppState.Responding, "/liveness"));

        Assert.Equal(("neutral", "Degraded"), (database.State, database.Label));
        Assert.StartsWith($"{DatabaseName}: {NotReadWhy}\n", database.Title, StringComparison.Ordinal);
    }

    [Fact]
    public void ADatabaseWhoseAppsAnsweredWithoutAHealthCheckWithoutIdsSaysTheCheckWasNotRead()
    {
        var database = Tile(
            ContainerAppsUse(null), "sqldb", Readings(), probes: _ => Answered(ContainerAppState.Responding, "/liveness"));

        Assert.Equal(("neutral", "Not probed"), (database.State, database.Label));
        Assert.Equal([("health check not read", "muted")], database.Lines.Select(line => (line.Text, line.Tone)));
        Assert.Equal($"{DatabaseName}: {NotReadWhy}", database.Title);
    }

    // ------------------------------------------- the container app with no address the browser can ask

    private static RuntimeManifest WithoutAnAddress(string? id) =>
        new("uat", [new RuntimeNode("ca_internal", RuntimeNodeKind.ContainerApp, "ca-demo-uat-internal", Ids: id is null ? null : [id])], [], []);

    private const string NoAddressWhy =
        "a container app without a public address this page knows: the browser cannot ask it.";

    [Fact]
    public void AContainerAppWithoutAnAddressSaysWhatResourceHealthSaidBehindItsReason()
    {
        var app = Tile(WithoutAnAddress(VaultId), "ca_internal", Readings());

        Assert.Equal(("neutral", "Available"), (app.State, app.Label));
        Assert.Equal([("Resource Health, 40 min ago", "plain")], app.Lines.Select(line => (line.Text, line.Tone)));
        Assert.StartsWith($"ca-demo-uat-internal: {NoAddressWhy}\n", app.Title, StringComparison.Ordinal);
    }

    [Fact]
    public void AContainerAppWithoutAnAddressAndWithoutIdsSaysNoAddressIsKnown()
    {
        var app = Tile(WithoutAnAddress(null), "ca_internal", Readings());

        Assert.Equal(("neutral", "No address"), (app.State, app.Label));
        Assert.Equal([("no public address known", "muted")], app.Lines.Select(line => (line.Text, line.Tone)));
        Assert.Equal($"ca-demo-uat-internal: {NoAddressWhy}", app.Title);
    }

    // ------------------------------------------- the reason-free case of the same helper

    /// <summary>
    /// <c>UnprobedNodeTile</c> is the shared fallback with no reason: the tile is titled by the node's name alone, so
    /// the helper's <c>"{name}: {reason}"</c> adds no colon and no line here.
    /// </summary>
    [Fact]
    public void ABoxWithNothingOfItsOwnToSayIsTitledByItsNameAlone()
    {
        var manifest = new RuntimeManifest("uat", [new RuntimeNode("res_1", RuntimeNodeKind.Resource, "kv-demo-uat")], [], []);

        var box = Tile(manifest, "res_1", Readings());

        Assert.Equal(("neutral", "Not probed"), (box.State, box.Label));
        Assert.Empty(box.Lines);
        Assert.Equal("kv-demo-uat", box.Title);
    }
}
