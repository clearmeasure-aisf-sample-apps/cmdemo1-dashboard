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
}
