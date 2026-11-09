using System.Net;

namespace Dashboard.Tests;

public class WorkItemsTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 22, 30, 0, TimeSpan.Zero);

    private static readonly WorkItemsReport Sample = WorkItemsReport.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "workitems.sample.json")))!;

    [Fact]
    public void TheSampleFileIsTheBoardTheViewShows()
    {
        Assert.Equal(new DateTimeOffset(2026, 11, 9, 22, 0, 0, TimeSpan.Zero), Sample.GeneratedAt);
        Assert.Equal(new WorkItemsScope("https://github.com/clearmeasure-aisf-sample-apps/cmdemo1-system", 679, "A bot may take an item from Todo. Done means the release is deployed to prod."), Sample.Scope);
        Assert.Equal(
            new WorkScopeLine("clearmeasure-aisf-sample-apps/cmdemo1-system", new Uri("https://github.com/clearmeasure-aisf-sample-apps/cmdemo1-system"), "679"),
            WorkItemsText.Scope(Sample.Scope));

        Assert.Equal([1, 1, 1, 2, 1, 1], WorkItemsText.Wip(Sample).Select(column => column.Count));
        Assert.Equal(WorkColumns.Active, WorkItemsText.Wip(Sample).Select(column => column.Column));
        Assert.Equal(
            ["Todo", "In Progress", "Technical Design", "Development", "Functional Testing", "Release Queue"],
            WorkItemsText.Active(Sample.Items).Select(group => group.Column));
        Assert.Equal([476, 458], WorkItemsText.Active(Sample.Items).Single(group => group.Column == WorkColumns.Development).Items.Select(item => item.Number));
        Assert.Equal([455, 440], WorkItemsText.RecentlyDone(Sample.Items).Select(item => item.Number));

        var queued = Sample.Items.Single(item => item.Number == 462);
        Assert.Equal("2.4.86", queued.ReleaseCandidate!.Version);
        Assert.Equal(["tdd", "uat"], WorkItemsText.Places(queued).Select(place => place.Name));
        Assert.Equal("tdd 2.4.86", WorkItemsText.PlaceLabel(WorkItemsText.Places(queued)[0]));
        Assert.Equal(["Session 1", "Session 2"], Enumerable.Range(1, queued.CursorSessions.Count).Select(index => WorkItemsText.SessionLabel(index, queued.CursorSessions.Count)));

        var rework = Sample.Items.Single(item => item.Number == 458);
        Assert.Equal("dashboard-bot", WorkItemsText.Agent(rework));
        Assert.Equal("cm-ai-ops", WorkItemsText.Assignee(rework));
        Assert.Equal("closed", Assert.Single(rework.PullRequests).State);
        Assert.Equal("since 13 h ago", WorkItemsText.Since(rework, Now));

        Assert.Equal("carries #458, #462", WorkItemsText.Carries(Sample, "2.4.86+bbccdd"));
        Assert.Equal("carries #470", WorkItemsText.Carries(Sample, "2.4.88"));
        Assert.Null(WorkItemsText.Carries(Sample, "2.4.14"));
        Assert.Null(WorkItemsText.Carries(null, "2.4.88"));

        Assert.Equal("median 18.5 h · p85 3 d · 11 items", WorkItemsText.Duration(Sample.Stats.CycleTimeHours));
        Assert.Equal("median 26 h · p85 4 d · 9 items", WorkItemsText.Duration(Sample.Stats.LeadTimeHours));
        Assert.Equal("2 functional-test failures · 1 revert · 12.5 %", WorkItemsText.Rework(Sample.Stats.Rework));
        Assert.Equal("n/a", WorkItemsText.Duration(null));
        Assert.Equal("n/a", WorkItemsText.Rework(null));
        Assert.Equal(
            "$2.00 per item · $18.00 total · Cursor agent usage for these items, last 30 days",
            WorkItemsText.Cost(Sample.Stats.Cost, Sample.Items.Count));
        Assert.Equal("n/a", WorkItemsText.ItemCost(Sample.Items.Single(item => item.Number == 501).CostUsd));
        Assert.Equal("$4.50", WorkItemsText.ItemCost(4.5));

        var chart = WorkItemsText.Chart(Sample.Stats.PerWeek)!;
        Assert.Equal(5, chart.Scale);
        Assert.Equal(4, chart.Bars.Count);
        Assert.Equal(36d, WorkItemsText.BarHeight(5, chart.Scale));
        Assert.Equal(14.4, WorkItemsText.BarHeight(2, chart.Scale));
        Assert.Equal(0d, WorkItemsText.BarHeight(0, chart.Scale));
        Assert.Equal("Week of 2026-11-09: 5 merged, 2 deployed to prod", WorkItemsText.LatestWeek(chart));
        Assert.Contains("Week of 2026-10-19: 2 merged, 1 deployed to prod", chart.Title, StringComparison.Ordinal);
    }

    [Fact]
    public void TheWorkItemsFileIsReadFromBesideTheDeliveryFacts()
    {
        var system = TopologyParser.Parse(Optics.Topology).Topology!.System;

        Assert.Equal("https://raw.example.net/org/demo-system/status/workitems.json", system.WorkItemsUrl!.AbsoluteUri);
        Assert.EndsWith(WorkItemsReport.FileName, system.WorkItemsUrl.AbsoluteUri, StringComparison.Ordinal);
        Assert.Null(TopologyParser.Parse("""{ "system": { }, "environments": [] }""").Topology!.System.WorkItemsUrl);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("404: Not Found")]
    [InlineData("[]")]
    [InlineData("not json")]
    public void AnythingButAnObjectIsNoReport(string? body) => Assert.Null(WorkItemsReport.Parse(body));

    [Fact]
    public void AnEmptyObjectIsAReportWithNothingInIt()
    {
        var report = WorkItemsReport.Parse("{ }")!;

        Assert.Null(report.GeneratedAt);
        Assert.Null(report.Scope);
        Assert.Empty(report.Items);
        Assert.Empty(report.Releases);
        Assert.DoesNotContain(WorkItemsText.Wip(report), column => column.Count > 0);
        Assert.Equal("n/a", WorkItemsText.Cost(report.Stats.Cost, 0));
        Assert.Null(WorkItemsText.Chart(report.Stats.PerWeek));
        Assert.Equal(WorkItemsText.None, "The work items file lists no items.");
    }

    [Fact]
    public void AGeneratedFieldFromAnOlderShapeIsStillTheTime() =>
        Assert.Equal(new DateTimeOffset(2026, 11, 9, 22, 0, 0, TimeSpan.Zero), WorkItemsReport.Parse("""{ "generated": "2026-11-09T22:00:00Z" }""")!.GeneratedAt);

    [Fact]
    public void EveryPartOfAnItemIsOptionalAndABadPartIsLeftOut()
    {
        var report = WorkItemsReport.Parse("""
            { "items": [
                { "number": 12, "title": "  Keep it  ", "url": "/relative", "cursorSessions": ["javascript:alert(1)", "https://cursor.com/agents/bc-1"], "prs": [7, { "state": "open" }, { "url": "pulls/1" }], "releaseCandidate": { "version": "2.4.3+sha" }, "environments": { "TDD": { }, "prod": null }, "costUsd": -4 },
                { "version": "no number" },
                7 ],
              "releases": [ { "items": [12, -1, 4.2] }, { "version": "2.4.3+sha", "items": [12, "no"] } ],
              "stats": { "wipByColumn": { "todo": 3, "Done": 9, "Review": 1 }, "rework": { "rate": "high" }, "cost": { "totalUsd": null, "note": " not metered " }, "perWeek": [ { }, 4, { "merged": 1 } ] } }
            """)!;

        var item = Assert.Single(report.Items);
        Assert.Equal(12, item.Number);
        Assert.Equal("Keep it", item.Title);
        Assert.Null(item.Url);
        Assert.Equal(new Uri("https://cursor.com/agents/bc-1"), Assert.Single(item.CursorSessions));
        Assert.Equal("open", Assert.Single(item.PullRequests).State);
        Assert.Equal("2.4.3", item.ReleaseCandidate!.Version);
        Assert.Equal(["tdd"], WorkItemsText.Places(item).Select(place => place.Name));
        Assert.Null(item.CostUsd);
        Assert.Equal("n/a", WorkItemsText.ItemCost(item.CostUsd));

        Assert.Equal([3, 0, 0, 0, 0, 0, 1], WorkItemsText.Wip(report).Select(column => column.Count));
        Assert.Equal("Review", WorkItemsText.Wip(report)[^1].Column);
        Assert.Equal("n/a · not metered", WorkItemsText.Cost(report.Stats.Cost, report.Items.Count));
        Assert.Equal("n/a", WorkItemsText.Rework(report.Stats.Rework));
        Assert.Equal("carries #12", WorkItemsText.Carries(report, "2.4.3"));
        Assert.Equal(1, Assert.Single(WorkItemsText.Chart(report.Stats.PerWeek)!.Bars).Merged);
    }

    [Fact]
    public void AColumnTheFileInventsIsShownAndIsNotDone()
    {
        var report = WorkItemsReport.Parse("""
            { "items": [
                { "number": 1, "column": "Review" },
                { "number": 2, "column": " review " },
                { "number": 3 },
                { "number": 4, "column": "done", "columnHistory": [ { "column": "Done", "enteredAt": "2026-11-09T18:00:00Z" } ] } ] }
            """)!;

        Assert.Equal(["Review", WorkColumns.Unplaced], WorkItemsText.Active(report.Items).Select(group => group.Column));
        Assert.Equal([1, 2], WorkItemsText.Active(report.Items)[0].Items.Select(item => item.Number));
        Assert.Equal([4], WorkItemsText.RecentlyDone(report.Items).Select(item => item.Number));
        Assert.Equal("work-col-in-progress", WorkItemsText.ColumnId("In Progress"));
        Assert.Equal("work-col-unplaced", WorkItemsText.ColumnId(WorkColumns.Unplaced));
    }

    [Fact]
    public void WithoutColumnCountsTheStripIsCountedFromTheItems()
    {
        var report = WorkItemsReport.Parse("""{ "items": [ { "number": 1, "column": "Todo" }, { "number": 2, "column": "Todo" }, { "number": 3, "column": "Done" } ] }""")!;

        Assert.Equal(2, WorkItemsText.Wip(report).Single(column => column.Column == WorkColumns.Todo).Count);
        Assert.Equal(0, WorkItemsText.Wip(report).Single(column => column.Column == WorkColumns.Development).Count);
        Assert.DoesNotContain(WorkItemsText.Wip(report), column => column.Column == WorkColumns.Done);
    }

    [Theory]
    [InlineData(0, "0 %")]
    [InlineData(1, "100 %")]
    [InlineData(0.125, "12.5 %")]
    [InlineData(2, "2")]
    public void ARateUpToOneIsAPercentAndALargerNumberIsKept(double rate, string expected) =>
        Assert.Equal(expected, WorkItemsText.Rework(new WorkRework(null, null, rate)));

    [Fact]
    public void CostWithoutATotalSaysSoAndKeepsTheNote()
    {
        Assert.Equal("n/a · Cursor spend is not published", WorkItemsText.Cost(new WorkCost(null, "Cursor spend is not published"), 4));
        Assert.Equal("$18.00 total", WorkItemsText.Cost(new WorkCost(18, null), 0));
        Assert.Equal("—", WorkItemsText.Agent(new WorkItem(1, null, null, null, null, [], null, null, [], [], null, WorkItemEnvironments.None, null)));
    }

    [Theory]
    [InlineData(false, false, WorkItemsAvailability.NoAddress, WorkItemsText.NoAddress)]
    [InlineData(true, false, WorkItemsAvailability.Loading, WorkItemsText.Loading)]
    [InlineData(true, true, WorkItemsAvailability.NotPublished, WorkItemsText.NotPublished)]
    public void TheEmptyStateSaysWhyThereIsNothing(bool hasAddress, bool asked, WorkItemsAvailability availability, string text)
    {
        Assert.Equal(availability, WorkItemsText.Availability(hasAddress, asked, null));
        Assert.Equal(text, WorkItemsText.Unavailable(availability));
    }

    [Fact]
    public void AReadReportIsReadyEvenBeforeTheAskIsRemembered() =>
        Assert.Equal(WorkItemsAvailability.Ready, WorkItemsText.Availability(true, false, Sample));

    [Fact]
    public async Task TheFileIsReadWithTheFirstRoundAndThenEveryFiveMinutes()
    {
        var time = new SignallingTimeProvider();
        var handler = new StubHandler(request => Optics.Answer(request));
        var monitor = Optics.Monitor(handler, time);
        const string Path = "/org/demo-system/status/workitems.json";

        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);

        Assert.True(monitor.WorkItemsAsked);
        Assert.Null(monitor.WorkItems);
        Assert.Equal(1, Optics.Count(handler, Path));

        for (var round = 0; round < 9; round++)
        {
            time.Advance(TimeSpan.FromSeconds(30));
            await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);
        }

        Assert.Equal(1, Optics.Count(handler, Path));

        time.Advance(TimeSpan.FromSeconds(30));
        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);

        Assert.Equal(2, Optics.Count(handler, Path));
    }

    [Fact]
    public async Task AFileThatIsNotThereYetLeavesTheViewEmptyAndFailsNoCheck()
    {
        var handler = new StubHandler(request => request.RequestUri!.AbsolutePath.EndsWith(WorkItemsReport.FileName, StringComparison.Ordinal)
            ? StubHandler.Answer(HttpStatusCode.NotFound, "404: Not Found")
            : Optics.Answer(request));
        var monitor = Optics.Monitor(handler, new SignallingTimeProvider());

        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);

        Assert.True(monitor.WorkItemsAsked);
        Assert.Null(monitor.WorkItems);
        Assert.Equal("All 4 nodes healthy", monitor.Summary.Text);
        Assert.Equal(WorkItemsText.NotPublished, WorkItemsText.Unavailable(WorkItemsText.Availability(true, monitor.WorkItemsAsked, monitor.WorkItems)));
    }

    [Fact]
    public async Task AFailedReadingKeepsTheLastGoodOne()
    {
        var time = new SignallingTimeProvider();
        var offline = false;
        var handler = new StubHandler(request =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith(WorkItemsReport.FileName, StringComparison.Ordinal))
            {
                if (offline)
                {
                    throw new HttpRequestException("offline");
                }

                return StubHandler.Answer(HttpStatusCode.OK, """{ "generatedAt": "2026-11-09T22:00:00Z", "items": [ { "number": 7, "column": "Todo", "title": "One" } ] }""");
            }

            return Optics.Answer(request);
        });
        var monitor = Optics.Monitor(handler, time);

        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);
        Assert.Equal(7, Assert.Single(monitor.WorkItems!.Items).Number);

        offline = true;
        time.Advance(DashboardMonitor.DeliveryInterval);
        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);

        Assert.Equal(7, Assert.Single(monitor.WorkItems.Items).Number);
        Assert.Equal(2, Optics.Count(handler, "/org/demo-system/status/workitems.json"));
    }

    [Fact]
    public async Task WithoutADeliveryAddressNothingIsReadForWorkItems()
    {
        var handler = new StubHandler(request => Optics.Answer(request));
        var monitor = Optics.Monitor(handler, new SignallingTimeProvider(), topology: Optics.Topology.Replace("deliveryUrl", "ignored", StringComparison.Ordinal));

        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);

        Assert.False(monitor.WorkItemsAsked);
        Assert.Null(monitor.WorkItems);
        Assert.DoesNotContain(handler.Requests, request => request.RequestUri!.AbsolutePath.EndsWith(WorkItemsReport.FileName, StringComparison.Ordinal));
        Assert.Equal(WorkItemsAvailability.NoAddress, WorkItemsText.Availability(monitor.Topology.System.WorkItemsUrl is not null, monitor.WorkItemsAsked, monitor.WorkItems));
    }
}
