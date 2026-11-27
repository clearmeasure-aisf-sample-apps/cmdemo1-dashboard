namespace Dashboard.Tests;

/// <summary>
/// Pins the rule that the file word which turns <see cref="DeploymentsReport.SaysActivity" /> on is the same word
/// the report reads its list from: <c>recent</c>, <c>freezes</c> and <c>missing</c> each name their own list, and
/// <c>deployments</c> is the guard a file must pass to parse at all. A file with only one of the activity lists
/// says activity and has exactly that list populated; a file without a <c>deployments</c> array is no report.
/// </summary>
public class DeploymentsReportListWordsTests
{
    /// <summary>A file with only a <c>freezes</c> list says activity and has its freezes read.</summary>
    [Fact]
    public void AFileWithOnlyFreezesSaysActivityAndPopulatesFreezes()
    {
        var report = Parse(json: $$"""{ "deployments": [ ], "freezes": [ { "name": "prod weekend", "from": "2026-10-09T00:00:00Z", "to": "2026-10-12T00:00:00Z", "active": true, "environments": [ "prod" ], "projects": [ "demo-ui" ] } ] }""");

        Assert.NotNull(report);
        Assert.True(report!.SaysActivity);
        Assert.Single(report.Freezes);
        Assert.Empty(report.Recent);
        Assert.Empty(report.Missing);
    }

    /// <summary>A file with only a <c>recent</c> list says activity and has its recent read.</summary>
    [Fact]
    public void AFileWithOnlyRecentSaysActivityAndPopulatesRecent()
    {
        var report = Parse(json: $$"""{ "deployments": [ ], "recent": [ { "project": "demo-ui", "environment": "uat", "release": "2.4.43", "result": "succeeded", "finished": "2026-10-09T07:00:00Z" } ] }""");

        Assert.NotNull(report);
        Assert.True(report!.SaysActivity);
        Assert.Single(report.Recent);
        Assert.Empty(report.Freezes);
        Assert.Empty(report.Missing);
    }

    /// <summary>A file with only a <c>missing</c> list says activity and has its missing read.</summary>
    [Fact]
    public void AFileWithOnlyMissingSaysActivityAndPopulatesMissing()
    {
        var report = Parse(json: $$"""{ "deployments": [ ], "missing": [ "freezes" ] }""");

        Assert.NotNull(report);
        Assert.True(report!.SaysActivity);
        var missing = Assert.Single(report.Missing);
        Assert.Equal("freezes", missing);
        Assert.Empty(report.Recent);
        Assert.Empty(report.Freezes);
    }

    /// <summary>A file without a <c>deployments</c> property parses to null: the guard and the read share the word.</summary>
    [Fact]
    public void AFileWithoutDeploymentsParsesToNull()
    {
        var report = DeploymentsReport.Parse($$"""{ "generated": "2026-10-09T11:58:00Z", "recent": [ { "project": "demo-ui", "environment": "uat", "release": "2.4.43", "result": "succeeded", "finished": "2026-10-09T07:00:00Z" } ] }""");

        Assert.Null(report);
    }

    /// <summary>A file whose <c>deployments</c> is not an array parses to null: the guard and the read share the word.</summary>
    [Fact]
    public void AFileWhoseDeploymentsIsNotAnArrayParsesToNull()
    {
        var report = DeploymentsReport.Parse($$"""{ "deployments": { "project": "demo-ui" }, "recent": [ ] }""");

        Assert.Null(report);
    }

    private static DeploymentsReport? Parse(string deployments = "", string recent = "", string freezes = "", string? json = null) =>
        json is { } whole
            ? DeploymentsReport.Parse(whole)
            : DeploymentsReport.Parse($$"""{ "deployments": [ {{deployments}} ], "recent": [ {{recent}} ], "freezes": [ {{freezes}} ], "missing": [] }""");
}
