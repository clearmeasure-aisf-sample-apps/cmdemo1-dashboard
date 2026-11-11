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

        Assert.Equal([1, 2, 2, 1, 1, 1], WorkItemsText.Wip(Sample).Select(column => column.Count));
        Assert.Equal(WorkColumns.Active, WorkItemsText.Wip(Sample).Select(column => column.Column));
        Assert.Equal(
            ["Todo", "In Progress", "Review", "Merged", "TDD", "UAT"],
            WorkItemsText.Active(Sample.Items).Select(group => group.Column));
        Assert.Equal([490, 476], WorkItemsText.Active(Sample.Items).Single(group => group.Column == WorkColumns.Review).Items.Select(item => item.Number));
        Assert.Equal([455, 421, 440], WorkItemsText.RecentlyDone(Sample.Items).Select(item => item.Number));
        Assert.Equal(WorkWorker.Team, Sample.Items.Single(item => item.Number == 501).Worker);
        Assert.Equal(WorkWorker.Bot, Sample.Items.Single(item => item.Number == 476).Worker);
        Assert.Equal("failed", Sample.Items.Single(item => item.Number == 458).PullRequests[0].Checks);
        Assert.Equal("Functional tests failed on the release build", Sample.Items.Single(item => item.Number == 458).Blocker!.Text);
        Assert.Equal(8, WorkItemsText.Select(Sample.Items, WorkAudience.Bot).Count);
        Assert.Equal(3, WorkItemsText.Select(Sample.Items, WorkAudience.Team).Count);
        Assert.Equal("median 14 h · p85 2 d · 8 items", WorkItemsText.Duration(Sample.BotStats!.CycleTimeHours));
        Assert.Equal("TDD", WorkBotText.Longest(WorkBotText.ColumnWaits(Sample.BotStats, WorkItemsText.Select(Sample.Items, WorkAudience.Bot), Now))!.Column);
        Assert.NotNull(WorkItemsText.Comparison(Sample));

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
            "$1.64 per item · $18.00 total · Cursor agent usage for these items, last 30 days",
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

        Assert.Equal([3, 0, 1, 0, 0, 0], WorkItemsText.Wip(report).Select(column => column.Count));
        Assert.Equal(1, WorkItemsText.Wip(report).Single(column => column.Column == WorkColumns.Review).Count);
        Assert.Equal("n/a · not metered", WorkItemsText.Cost(report.Stats.Cost, report.Items.Count));
        Assert.Equal("n/a", WorkItemsText.Rework(report.Stats.Rework));
        Assert.Equal("carries #12", WorkItemsText.Carries(report, "2.4.3"));
        Assert.Equal(1, Assert.Single(WorkItemsText.Chart(report.Stats.PerWeek)!.Bars).Merged);
    }

    [Fact]
    public void AColumnTheFileInventsIsUnplacedAndIsNotDone()
    {
        var report = WorkItemsReport.Parse("""
            { "items": [
                { "number": 1, "column": "Blocked" },
                { "number": 2, "column": " blocked " },
                { "number": 3 },
                { "number": 4, "column": "done", "columnHistory": [ { "column": "Done", "enteredAt": "2026-11-09T18:00:00Z" } ] } ] }
            """)!;

        Assert.Equal([WorkColumns.Unplaced], WorkItemsText.Active(report.Items).Select(group => group.Column));
        Assert.Equal([1, 2, 3], WorkItemsText.Active(report.Items)[0].Items.Select(item => item.Number));
        Assert.Equal([4], WorkItemsText.RecentlyDone(report.Items).Select(item => item.Number));
        Assert.Equal("work-col-in-progress", WorkItemsText.ColumnId("In Progress"));
        Assert.Equal("work-col-unplaced", WorkItemsText.ColumnId(WorkColumns.Unplaced));
    }

    [Fact]
    public void TheBoardShowsExactlyTheSevenColumnsInOrder()
    {
        Assert.Equal(
            ["Todo", "In Progress", "Review", "Merged", "TDD", "UAT", "Done"],
            WorkColumns.Order);
        Assert.Equal(
            ["Todo", "In Progress", "Review", "Merged", "TDD", "UAT"],
            WorkColumns.Active);
        Assert.Equal(WorkColumns.Active, WorkItemsText.Wip(Sample).Select(column => column.Column));

        var report = WorkItemsReport.Parse("""
            { "items": [
                { "number": 1, "column": "Todo" },
                { "number": 2, "column": "In Progress" },
                { "number": 3, "column": "Review" },
                { "number": 4, "column": "Merged" },
                { "number": 5, "column": "TDD" },
                { "number": 6, "column": "UAT" },
                { "number": 7, "column": "Done", "columnHistory": [ { "column": "Done", "enteredAt": "2026-11-09T18:00:00Z" } ] },
                { "number": 8, "column": "Technical Design" },
                { "number": 9, "column": "Development" },
                { "number": 10, "column": "Functional Testing" },
                { "number": 11, "column": "Release Queue" } ] }
            """)!;

        Assert.Equal(
            ["Todo", "In Progress", "Review", "Merged", "TDD", "UAT", WorkColumns.Unplaced],
            WorkItemsText.Active(report.Items).Select(group => group.Column));
        Assert.Equal(1, WorkItemsText.Active(report.Items).Single(group => group.Column == WorkColumns.Todo).Items.Single().Number);
        Assert.Equal(2, WorkItemsText.Active(report.Items).Single(group => group.Column == WorkColumns.InProgress).Items.Single().Number);
        Assert.Equal(3, WorkItemsText.Active(report.Items).Single(group => group.Column == WorkColumns.Review).Items.Single().Number);
        Assert.Equal(4, WorkItemsText.Active(report.Items).Single(group => group.Column == WorkColumns.Merged).Items.Single().Number);
        Assert.Equal(5, WorkItemsText.Active(report.Items).Single(group => group.Column == WorkColumns.Tdd).Items.Single().Number);
        Assert.Equal(6, WorkItemsText.Active(report.Items).Single(group => group.Column == WorkColumns.Uat).Items.Single().Number);
        Assert.Equal([8, 9, 10, 11], WorkItemsText.Active(report.Items).Single(group => group.Column == WorkColumns.Unplaced).Items.Select(item => item.Number));
        Assert.Equal([7], WorkItemsText.RecentlyDone(report.Items).Select(item => item.Number));
        Assert.Equal(WorkColumns.Active.Append(WorkColumns.Unplaced), WorkItemsText.Wip(report).Select(column => column.Column));
        Assert.DoesNotContain(WorkItemsText.Wip(report), column => column.Column is "Technical Design" or "Development" or "Functional Testing" or "Release Queue");
        Assert.DoesNotContain(WorkColumns.Order, column => column is "Technical Design" or "Development" or "Functional Testing" or "Release Queue");
    }

    [Fact]
    public void WithoutColumnCountsTheStripIsCountedFromTheItems()
    {
        var report = WorkItemsReport.Parse("""{ "items": [ { "number": 1, "column": "Todo" }, { "number": 2, "column": "Todo" }, { "number": 3, "column": "Done" } ] }""")!;

        Assert.Equal(2, WorkItemsText.Wip(report).Single(column => column.Column == WorkColumns.Todo).Count);
        Assert.Equal(0, WorkItemsText.Wip(report).Single(column => column.Column == WorkColumns.Merged).Count);
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

    [Fact]
    public void AnOlderFileWithoutASplitStaysOneBoard()
    {
        var report = WorkItemsReport.Parse("""{ "stats": { "cycleTimeHours": { "median": 5, "n": 1 } }, "items": [ { "number": 1, "column": "Todo", "title": "Old" } ] }""")!;

        Assert.Equal(5, report.Stats.CycleTimeHours!.Median);
        Assert.Null(report.BotStats);
        Assert.Null(report.TeamStats);
        Assert.Equal(WorkWorker.Unknown, Assert.Single(report.Items).Worker);
        Assert.Null(WorkItemsText.Comparison(report));
        Assert.Equal("No bot work items.", WorkItemsText.NoneOf(WorkAudience.Bot));
    }

    [Fact]
    public void AMissingSliceIsCountedFromTheItems()
    {
        var report = WorkItemsReport.Parse("""
            { "items": [
                { "number": 1, "worker": "bot", "agent": "alpha", "column": "Done",
                  "columnHistory": [
                    { "column": "In Progress", "enteredAt": "2026-11-08T00:00:00Z" },
                    { "column": "Review", "enteredAt": "2026-11-08T12:00:00Z" },
                    { "column": "Done", "enteredAt": "2026-11-09T00:00:00Z" } ],
                  "prs": [ { "number": 9, "state": "merged", "mergedAt": "2026-11-09T00:00:00Z", "mergeSha": "abcdef1234567890", "checks": "success" } ],
                  "environments": { "prod": { "version": "2.4.1", "deployedAt": "2026-11-09T06:00:00Z" } },
                  "rework": { "ftFailures": 1, "reverts": 0 }, "costUsd": 2 },
                { "number": 2, "worker": "team", "column": "Todo", "title": "A person", "assignee": "pat.morgan" } ] }
            """)!;
        var bot = WorkItemsText.Select(report.Items, WorkAudience.Bot);
        var slice = WorkItemsText.Slice(report, WorkAudience.Bot);

        Assert.Null(report.BotStats);
        Assert.Equal(24, slice.CycleTimeHours!.Median);
        Assert.Equal(0, slice.WipByColumn["In Progress"]);
        Assert.Equal("passed", Assert.Single(Assert.Single(bot).PullRequests).Checks);
        Assert.Equal(["In Progress · 12 h", "Review · 12 h", "Done · 22 h so far"], WorkBotText.Timeline(Assert.Single(bot), Now).Select(WorkBotText.Stay));
        Assert.Equal("Done", WorkBotText.Longest(WorkBotText.ColumnWaits(null, bot, Now))!.Column);
        Assert.Equal("alpha: 1 merged, 1 deployed to prod", WorkBotText.ThroughputLine(Assert.Single(WorkBotText.Throughput(null, bot))));
        Assert.Equal(6, WorkBotText.MergeToProd(null, bot)!.Median);
        Assert.Equal("#9 · merged · checks passed · merged 22 h ago · abcdef1", WorkBotText.PullLine(Assert.Single(bot[0].PullRequests), Now));
        Assert.Contains("Bot", WorkItemsText.Comparison(report)!.BotCycle is not null ? "Bot" : string.Empty, StringComparison.Ordinal);
        Assert.Equal(WorkWorker.Team, WorkItemsText.Select(report.Items, WorkAudience.Team)[0].Worker);
    }

    [Fact]
    public void TheFeedSampleUsesThePublishedNames()
    {
        var feed = WorkItemsReport.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "feed-workitems.json")))!;
        var bot = feed.Items.Single(item => item.Number == 1565);
        var failed = feed.Items.Single(item => item.Number == 852);

        Assert.Equal("clearmeasure-aisf-sample-apps/cmdemo1-workorders", feed.Scope!.Repo);
        Assert.Contains("worker", feed.Scope.Rules, StringComparison.Ordinal);
        Assert.Equal(2.1, feed.BotStats!.TimePerColumnHours!["Done"].Median);
        Assert.Equal("Done", WorkBotText.Longest(WorkBotText.ColumnWaits(feed.BotStats, feed.Items, Now))!.Column);
        Assert.Equal("cm-ai-ops-bot: 1 done", WorkBotText.ThroughputLine(Assert.Single(feed.BotStats.Throughput!)));
        Assert.Equal(4.8, feed.BotStats.MergeToProdHours!.Median);
        Assert.Equal(WorkWorker.Bot, bot.Worker);
        Assert.Equal(0, bot.Rework!.FtFailures);
        Assert.Equal(0, bot.Rework.Reverts);
        Assert.Equal(1, failed.Rework!.FtFailures);
        Assert.Equal("passed", Assert.Single(bot.PullRequests).Checks);
        Assert.Equal("status", bot.Blocker!.Kind);
        Assert.Equal(new DateTimeOffset(2026, 11, 9, 23, 4, 14, TimeSpan.Zero), bot.Environments.Tdd!.DeployedAt);
        Assert.Equal("2.4.2060", bot.Environments.Tdd.Version);
        Assert.Equal(
            ["In Progress · 6 min", "Review · 12 min", "Merged · 30 min", "TDD · 84 min so far"],
            WorkBotText.Timeline(bot, Now).Select(WorkBotText.Stay));
        Assert.Equal(WorkWorker.Team, feed.Items.Single(item => item.Number == 1543).Worker);
    }

    [Fact]
    public async Task ThePublishedWorkItemsFileParses()
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        using var response = await http.GetAsync(new Uri("https://raw.githubusercontent.com/clearmeasure-aisf-sample-apps/cmdemo1-system/status/workitems.json"));
        response.EnsureSuccessStatusCode();
        var feed = WorkItemsReport.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.True(feed.Items.Count > 100);
        Assert.Contains(feed.Items, item => item.Worker == WorkWorker.Bot);
        Assert.Contains(feed.Items, item => item.Worker == WorkWorker.Team);
        Assert.NotNull(feed.BotStats!.TimePerColumnHours);
        Assert.NotEmpty(feed.BotStats.Throughput!);
        Assert.All(feed.BotStats.Throughput!, row => Assert.NotNull(row.Done));
        Assert.NotNull(feed.BotStats.MergeToProdHours);
        Assert.Contains(feed.Items, item => item.PullRequests.Any(pull => pull.Checks is not null));
        Assert.Contains(feed.Items, item => item.ColumnHistory.Any(visit => visit.Hours is not null));
        Assert.Contains(feed.Items, item => item.Blocker?.Kind is "status" or "blocker");
    }
}
