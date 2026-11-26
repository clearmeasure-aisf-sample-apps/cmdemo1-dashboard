namespace Dashboard.Tests;

/// <summary>
/// <see cref="RuntimeRegionRole" />: the one owner of the four words a deployment writes for a region's roles, and of
/// the words the region's box says for each.
/// </summary>
public class RuntimeRegionRoleTests
{
    [Fact]
    public void TheFourRolesAreSpelledAsTheDeploymentWritesThem()
    {
        Assert.Equal("primary", RuntimeRegionRole.Primary);
        Assert.Equal("standby", RuntimeRegionRole.Standby);
        Assert.Equal("data", RuntimeRegionRole.Data);
        Assert.Equal("static", RuntimeRegionRole.Static);
    }

    [Theory]
    [InlineData("data", "database")]
    [InlineData("static", "static sites")]
    [InlineData("primary", "primary")]
    [InlineData("standby", "standby")]
    [InlineData("edge", "edge")]
    [InlineData("Data", "Data")]
    [InlineData("", "")]
    public void ARoleIsSaidInThePagesWordsAndAnUnknownOneAsWritten(string role, string words)
    {
        Assert.Equal(words, RuntimeRegionRole.Words(role));
    }

    [Fact]
    public void ARegionWithNoWebAppSaysItsRolesInThePagesWords()
    {
        var region = new RuntimeRegion("r1", "eastus", [RuntimeRegionRole.Data, RuntimeRegionRole.Static, "edge"]);
        var manifest = new RuntimeManifest("uat", [], [region], []);

        var payload = RuntimePayloadBuilder.Build(manifest, null, null, TimeZoneInfo.Utc);

        var mark = Assert.Single(payload.Regions);
        Assert.Contains("database, static sites, edge: not probed", mark.Label, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(new[] { "data", "static" }, true, "database: reachable; static sites: not probed")]
    [InlineData(new[] { "data", "static" }, false, "database, static sites: not probed")]
    [InlineData(new[] { "data" }, true, "database: reachable")]
    [InlineData(new[] { "data" }, false, "database: not probed")]
    [InlineData(new[] { "static", "data", "edge" }, true, "database: reachable; static sites, edge: not probed")]
    [InlineData(new[] { "data", "data" }, true, "database: reachable")]
    [InlineData(new[] { "Data" }, true, "database: reachable; Data: not probed")]
    [InlineData(new string[0], true, "database: reachable")]
    [InlineData(new string[0], false, "")]
    public void ARegionWithNoWebAppSaysTheReachableDatabaseThenTheUnprobedRoles(string[] roles, bool databaseReachable, string label)
    {
        Assert.Equal(label, RuntimeRegionRole.Unprobed(roles, databaseReachable));
    }

    [Fact]
    public void TheWordsOfTheUnprobedLabelAreNamed()
    {
        Assert.Equal("database: reachable", RuntimeRegionRole.DatabaseReachable);
        Assert.Empty(RuntimeRegionRole.NothingToSay);
        Assert.Equal("database, static sites, edge: not probed", RuntimeRegionRole.NotProbed(["data", "static", "edge"]));
    }

    [Fact]
    public void ARegionOfNoRolesAndNoWebAppSaysNothing()
    {
        var region = new RuntimeRegion("r1", "eastus", []);
        var manifest = new RuntimeManifest("uat", [], [region], []);

        var payload = RuntimePayloadBuilder.Build(manifest, null, null, TimeZoneInfo.Utc);

        var mark = Assert.Single(payload.Regions);
        Assert.Equal(new RuntimeRegionMark("r1", "neutral", ""), mark with { NameLink = null });
    }

    [Fact]
    public void ARegionWhoseOnlyRoleIsDataSaysExactlyThatItsDatabaseIsReachable()
    {
        var shipped = RuntimeManifestParser.ParseManifest(RuntimeManifestParserTests.Sample("uat.json")).Value!;
        var manifest = shipped with
        {
            Regions = shipped.Regions
                .Select(region => region.Alias == "region_data" ? region with { Roles = [RuntimeRegionRole.Data] } : region)
                .ToList(),
        };
        var now = new DateTimeOffset(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
        var healthy = new ContainerAppStatus(ContainerAppState.Healthy, new ContainerAppAnswer("/_healthcheck", true, 200, false, 40, now));

        var payload = RuntimePayloadBuilder.Build(manifest, null, null, TimeZoneInfo.Utc, containerApps: _ => healthy);

        Assert.Equal("healthy", payload.Nodes.Single(tile => tile.Alias == "sqldb").State);
        Assert.Equal("database: reachable", payload.Regions.Single(region => region.Alias == "region_data").Label);
    }
}
