using Dashboard.Work;

namespace Dashboard.Tests;

/// <summary>
/// The "kept parts" rule of <see cref="WorkItemsReport" />: a <c>Read</c> keeps a record when at least one of the
/// parts it weighs was given, and drops one when every named part is unsaid. The rule is named once, in
/// <c>ReadParts</c> in <c>src/Dashboard/Work/WorkItemsReport.cs</c>. These tests pin the rule through the public
/// <see cref="WorkItemsReport.Parse" />, using the same <c>items</c> and <c>prs</c> shapes the other
/// <c>WorkItems</c> tests use.
/// </summary>
public class WorkItemsReportKeptPartsTests
{
    [Fact]
    public void AnItemThatSaysOnlyACostIsLeftOutOfItems()
    {
        var report = WorkItemsReport.Parse("""{ "items": [ { "costUsd": 3.5 } ] }""")!;

        Assert.Empty(report.Items);
    }

    [Fact]
    public void AnItemThatSaysOnlyAPullRequestWithANumberIsLeftOutOfItems()
    {
        var report = WorkItemsReport.Parse("""{ "items": [ { "prs": [ { "number": 7 } ] } ] }""")!;

        Assert.Empty(report.Items);
    }

    [Fact]
    public void AnItemThatSaysOnlyAColumnIsKept()
    {
        var report = WorkItemsReport.Parse("""{ "items": [ { "column": "Todo" } ] }""")!;

        Assert.Single(report.Items);
    }

    [Fact]
    public void APullRequestThatSaysOnlyMergedAtAndMergeShaIsLeftOutAndOneThatSaysOnlyChecksIsKept()
    {
        var report = WorkItemsReport.Parse("""
            { "items": [ { "number": 1, "prs": [ { "mergedAt": "2026-11-09T00:00:00Z", "mergeSha": "abc123" }, { "checks": "passed" } ] } ] }
            """)!;
        var item = Assert.Single(report.Items);

        Assert.Equal(1, item.Number);
        var pull = Assert.Single(item.PullRequests);
        Assert.Equal("passed", pull.Checks);
    }
}
