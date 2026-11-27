namespace Dashboard.Tests;

public class WorkItemsReportKeptPartsTests
{
    [Fact]
    public void AnItemWithOnlyACostIsLeftOutOfTheItems()
    {
        var report = WorkItemsReport.Parse("""{ "items": [ { "costUsd": 3.5 } ] }""")!;

        Assert.Empty(report.Items);
    }

    [Fact]
    public void AnItemWithOnlyAPullRequestThatHasANumberIsLeftOutOfTheItems()
    {
        var report = WorkItemsReport.Parse("""{ "items": [ { "prs": [ { "number": 9 } ] } ] }""")!;

        Assert.Empty(report.Items);
    }

    [Fact]
    public void AnItemWithOnlyAColumnIsKept()
    {
        var report = WorkItemsReport.Parse("""{ "items": [ { "column": "Todo" } ] }""")!;

        var item = Assert.Single(report.Items);
        Assert.Equal("Todo", item.Identity.Column);
        Assert.Null(item.Identity.Number);
        Assert.Null(item.Identity.Title);
        Assert.Null(item.Identity.Url);
    }

    [Fact]
    public void APullRequestThatSaysOnlyMergedAtAndMergeShaIsLeftOutAndOneThatSaysOnlyChecksIsKept()
    {
        var report = WorkItemsReport.Parse("""
            { "items": [
                { "number": 7, "prs": [ { "mergedAt": "2026-11-09T18:00:00Z", "mergeSha": "bbccdd" }, { "checks": "passed" } ] } ] }
            """)!;

        var item = Assert.Single(report.Items);
        Assert.Equal(7, item.Identity.Number);
        var pull = Assert.Single(item.PullRequests);
        Assert.Equal("passed", pull.Checks);
        Assert.Null(pull.MergedAt);
        Assert.Null(pull.MergeSha);
    }
}
