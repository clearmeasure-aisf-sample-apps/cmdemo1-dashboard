namespace Dashboard.Tests;

/// <summary>
/// The two records of <c>workitems.json</c> that say what the factory finished: <see cref="WorkThroughput"/>, how
/// much one bot identity did (the file's own counts, or the same counts taken from the items), and
/// <see cref="WorkRelease"/>, one Octopus release and the work items it carries. Every time here is fixed; nothing
/// reads the clock, and a release carries no time of its own even when the file writes one beside it.
/// </summary>
public class WorkThroughputAndReleaseTests
{
    private static DateTimeOffset At(int day, int hour = 0, int minute = 0) => new(2026, 11, day, hour, minute, 0, TimeSpan.Zero);

    private static WorkItem Item(
        int number,
        string? agent = null,
        IReadOnlyList<WorkPullRequest>? pulls = null,
        bool toProd = false) =>
        new(
            number,
            $"Item {number}",
            null,
            null,
            null,
            [],
            null,
            agent,
            [],
            pulls ?? [],
            null,
            toProd ? new WorkItemEnvironments(null, null, new WorkDeployment("2.4.90", At(9, 6))) : WorkItemEnvironments.None,
            null);

    private static WorkPullRequest Pull(int number, string? state = null, DateTimeOffset? mergedAt = null) =>
        new(number, null, state, mergedAt, null);

    // ---- WorkThroughput: what the record holds ----

    [Fact]
    public void ARowIsAnIdentityItsMergesItsDeploysAndTheDoneCountTheNewerFileGives()
    {
        var row = new WorkThroughput("cm-ai-ops-bot", 4, 2, 7);

        Assert.Equal("cm-ai-ops-bot", row.Agent);
        Assert.Equal(4, row.Merged);
        Assert.Equal(2, row.DeployedProd);
        Assert.Equal(7, row.Done);
    }

    [Fact]
    public void TheDoneCountIsTheOnlyOneAnOlderFileHasNotGot()
    {
        var older = new WorkThroughput("alpha", 1, 0);

        Assert.Null(older.Done);
        Assert.Equal(0, older.DeployedProd);

        var newer = new WorkThroughput("alpha", 1, 0, 0);

        Assert.Equal(0, newer.Done);
        Assert.NotEqual(older, newer);
    }

    [Fact]
    public void TwoRowsOfTheSameFourValuesAreTheSameRow()
    {
        Assert.Equal(new WorkThroughput("alpha", 1, 2, 3), new WorkThroughput("alpha", 1, 2, 3));
        Assert.Equal(new WorkThroughput("alpha", 1, 2), new WorkThroughput("alpha", 1, 2, null));
        Assert.NotEqual(new WorkThroughput("alpha", 1, 2, 3), new WorkThroughput("beta", 1, 2, 3));
        Assert.NotEqual(new WorkThroughput("alpha", 1, 2, 3), new WorkThroughput("alpha", 1, 2, 4));
        IReadOnlyList<WorkThroughput> twice = [new WorkThroughput("alpha", 1, 2, 3), new WorkThroughput("alpha", 1, 2, 3)];
        Assert.Single(twice.Distinct());
    }

    // ---- WorkThroughput: the line the view prints ----

    [Theory]
    [InlineData("alpha", 1, null, null, "alpha: 1 done")]
    [InlineData("alpha", 3, null, null, "alpha: 3 done")]
    [InlineData("alpha", 0, null, null, "alpha: 0 done")]
    [InlineData("alpha", null, 4, null, "alpha: 4 merged, 0 deployed to prod")]
    [InlineData("alpha", 9, 4, 1, "alpha: 4 merged, 1 deployed to prod")]
    [InlineData("alpha", null, null, 2, "alpha: 0 merged, 2 deployed to prod")]
    [InlineData("alpha", null, null, null, "alpha: 0 merged, 0 deployed to prod")]
    [InlineData("unnamed", 2, null, null, "unnamed: 2 done")]
    public void ARowIsSaidOnceByItsCountsAndItsAgent(string agent, int? done, int? merged, int? deployed, string line) =>
        Assert.Equal(line, WorkBotText.ThroughputLine(new WorkThroughput(agent, merged, deployed, done)));

    [Fact]
    public void ARowThatIsNothingAtAllIsRefused() => Assert.Throws<ArgumentNullException>(() => WorkBotText.ThroughputLine(null!));

    // ---- WorkThroughput: the file's list ----

    [Fact]
    public void TheListOfThroughputKeepsTheFilesOrderAndItsOwnNames()
    {
        var report = WorkItemsReport.Parse("""
            { "stats": { "throughput": [
                { "agent": "zeta", "merged": 1, "deployedProd": 4 },
                { "name": "alpha", "merged": 9 },
                { "identity": "beta", "deployedProd": 2.6 },
                { "merged": 3, "deployedProd": 1 },
                { "agent": "  spaced  " },
                { "agent": "nonsense", "merged": -2, "deployedProd": "many" },
                { },
                7,
                "alpha" ] } }
            """)!;

        Assert.Equal(
        [
            new WorkThroughput("zeta", 1, 4),
            new WorkThroughput("alpha", 9, null),
            new WorkThroughput("beta", null, 3),
            new WorkThroughput("unnamed", 3, 1),
            new WorkThroughput("spaced", null, null),
            new WorkThroughput("nonsense", null, null),
        ],
            report.Stats.Throughput!);
        Assert.True(report.Stats.HasAny);
    }

    [Fact]
    public void AThroughputThatIsNotAnArrayIsNoRows()
    {
        var report = WorkItemsReport.Parse("""{ "stats": { "throughput": { "alpha": 2 } } }""")!;

        Assert.Null(report.Stats.Throughput);
        Assert.False(report.Stats.HasAny);
    }

    [Fact]
    public void NoThroughputAnywhereIsNoRowsRatherThanOneZeroRow()
    {
        var report = WorkItemsReport.Parse("""{ "stats": { "throughput": [], "throughputByAgent": { } } }""")!;

        Assert.Null(report.Stats.Throughput);
        Assert.False(report.Stats.HasAny);
        Assert.Null(report.BotStats);
        Assert.Empty(WorkBotText.Throughput(report.Stats, []));
    }

    // ---- WorkThroughput: the file's counts per agent ----

    [Fact]
    public void TheCountsByAgentWinOverTheListAndComeOutMostFinishedFirst()
    {
        var report = WorkItemsReport.Parse("""
            { "stats": {
                "throughput": [ { "agent": "listed", "merged": 99, "deployedProd": 99 } ],
                "throughputByAgent": { "beta": 2, "alpha": 2, "Zeta": 5, "  spaced  ": 1, "": 9, "negative": -1, "text": "4", "nothing": null } } }
            """)!;

        Assert.Equal(["Zeta", "alpha", "beta", "spaced"], report.Stats.Throughput!.Select(row => row.Agent));
        Assert.Equal([5, 2, 2, 1], report.Stats.Throughput!.Select(row => row.Done));
        Assert.All(report.Stats.Throughput!, row => Assert.Null(row.Merged));
        Assert.All(report.Stats.Throughput!, row => Assert.Null(row.DeployedProd));
        Assert.Equal("Zeta: 5 done", WorkBotText.ThroughputLine(report.Stats.Throughput![0]));
        Assert.True(report.Stats.HasAny);
    }

    [Fact]
    public void AnAgentMapThatCountsNothingFallsBackToTheList()
    {
        var report = WorkItemsReport.Parse("""
            { "stats": { "throughputByAgent": { "": 3, "negative": -1 }, "throughput": [ { "agent": "listed", "merged": 1 } ] } }
            """)!;

        Assert.Equal([new WorkThroughput("listed", 1, null)], report.Stats.Throughput!);
    }

    [Fact]
    public void EachSliceOfTheBoardHasItsOwnThroughputAndAnEmptySliceIsNoSlice()
    {
        var report = WorkItemsReport.Parse("""
            { "stats": {
                "all": { "throughputByAgent": { "cm-ai-ops-bot": 3 } },
                "bot": { "throughputByAgent": { "cm-ai-ops-bot": 3, "alpha": 1 } },
                "team": { "throughput": [] } } }
            """)!;

        Assert.Equal([new WorkThroughput("cm-ai-ops-bot", null, null, 3)], report.Stats.Throughput!);
        Assert.Equal([new WorkThroughput("cm-ai-ops-bot", null, null, 3), new WorkThroughput("alpha", null, null, 1)], report.BotStats!.Throughput!);
        Assert.True(report.BotStats.HasAny);
        Assert.Null(report.TeamStats);
    }

    [Fact]
    public void AnAllSliceOfNothingLeavesTheCountsWrittenBesideIt()
    {
        var report = WorkItemsReport.Parse("""{ "stats": { "all": { }, "throughput": [ { "agent": "legacy", "merged": 4 } ] } }""")!;

        Assert.Equal([new WorkThroughput("legacy", 4, null)], report.Stats.Throughput!);
    }

    // ---- WorkThroughput: counted from the items when the file does not say ----

    [Fact]
    public void TheFilesRowsAreShownWhicheverItemsAreOnTheBoard()
    {
        var report = WorkItemsReport.Parse("""{ "stats": { "bot": { "throughputByAgent": { "cm-ai-ops-bot": 3 } } } }""")!;

        Assert.Equal(
            [new WorkThroughput("cm-ai-ops-bot", null, null, 3)],
            WorkBotText.Throughput(report.BotStats, [Item(1, "alpha", [Pull(11, "merged", At(9, 2))]), Item(2, "alpha", toProd: true)]));
    }

    [Fact]
    public void WithoutTheFilesRowsTheItemsAreCountedByAgentMostMergedFirst()
    {
        IReadOnlyList<WorkItem> items =
        [
            Item(1, "alpha", [Pull(11, "merged", At(9, 2))], toProd: true),
            Item(2, "alpha", [Pull(12, "open", At(9, 3))]),
            Item(3, "Alpha", [Pull(13, "closed")]),
            Item(4, null, [Pull(14, "merged")]),
            Item(5, "beta"),
            Item(6, "beta", toProd: true),
        ];

        var rows = WorkBotText.Throughput(null, items);

        Assert.Equal(["alpha", "unnamed", "beta"], rows.Select(row => row.Agent));
        Assert.Equal([2, 1, 0], rows.Select(row => row.Merged));
        Assert.Equal([1, 0, 1], rows.Select(row => row.DeployedProd));
        Assert.All(rows, row => Assert.Null(row.Done));
        Assert.Equal("alpha: 2 merged, 1 deployed to prod", WorkBotText.ThroughputLine(rows[0]));
        Assert.Equal("beta: 0 merged, 1 deployed to prod", WorkBotText.ThroughputLine(rows[^1]));
    }

    [Fact]
    public void TwentyItemsAcrossFourIdentitiesMakeFourRowsOfTheCountsTheyReallyHold()
    {
        IReadOnlyList<WorkItem> items =
        [
            .. Enumerable.Range(1, 20).Select(index => Item(
                index,
                $"bot-{index % 4}",
                index % 2 == 0 ? [Pull(index, "merged", At(9, index % 12))] : null,
                toProd: index % 7 == 0)),
        ];

        var rows = WorkBotText.Throughput(null, items);

        Assert.Equal(["bot-0", "bot-2", "bot-1", "bot-3"], rows.Select(row => row.Agent));
        Assert.Equal([5, 5, 0, 0], rows.Select(row => row.Merged));
        Assert.Equal([0, 1, 0, 1], rows.Select(row => row.DeployedProd));
        Assert.Equal("bot-0: 5 merged, 0 deployed to prod", WorkBotText.ThroughputLine(rows[0]));
    }

    [Fact]
    public void AnItemWithNoPullRequestAndNoDeployIsCountedInItsAgentAtZero()
    {
        Assert.Equal([new WorkThroughput("alpha", 0, 0)], WorkBotText.Throughput(null, [Item(1, "alpha"), Item(2, "alpha")]));
    }

    [Fact]
    public void NoItemsAtAllIsNoRowsAndItemsThatAreNothingAreRefused()
    {
        Assert.Empty(WorkBotText.Throughput(null, []));
        Assert.Empty(WorkBotText.Throughput(WorkItemsStats.Empty, []));
        Assert.Throws<ArgumentNullException>(() => WorkBotText.Throughput(null, null!));
        Assert.Throws<ArgumentNullException>(() => WorkBotText.Throughput(WorkItemsStats.Empty, null!));
    }

    // ---- WorkRelease: what the record holds ----

    [Fact]
    public void AReleaseIsAVersionACommitAndTheItemsItCarriesInItsOwnOrder()
    {
        var report = WorkItemsReport.Parse("""
            { "releases": [
                { "version": "2.4.86+bbccdd", "sha": "bbccddeeff00", "items": [462, 458] },
                { "version": "2.4.90" },
                { "sha": "eeff001122", "items": [7] },
                { "version": "  2.4.91  ", "items": [12, 3, 12] } ] }
            """)!;

        Assert.Equal(["2.4.86", "2.4.90", null, "2.4.91"], report.Releases.Select(release => release.Version));

        Assert.Equal("bbccddeeff00", report.Releases[0].Sha);
        Assert.Equal([462, 458], report.Releases[0].Items);

        Assert.Null(report.Releases[1].Sha);
        Assert.Empty(report.Releases[1].Items);

        Assert.Equal("eeff001122", report.Releases[2].Sha);
        Assert.Equal([7], report.Releases[2].Items);

        Assert.Null(report.Releases[3].Sha);
        Assert.Equal([12, 3, 12], report.Releases[3].Items);
    }

    [Fact]
    public void AReleaseCarriesNoTimeEvenWhenTheFileWritesOneBesideIt()
    {
        var report = WorkItemsReport.Parse("""
            { "generatedAt": "2026-11-09T22:00:00Z",
              "releases": [ { "version": "2.4.2060", "sha": "dfb1740a", "createdAt": "2026-11-09T23:02:28Z", "items": [1555, 1560] } ] }
            """)!;

        var release = Assert.Single(report.Releases);

        Assert.Equal(new DateTimeOffset(2026, 11, 9, 22, 0, 0, TimeSpan.Zero), report.GeneratedAt);
        Assert.Equal("2.4.2060", release.Version);
        Assert.Equal("dfb1740a", release.Sha);
        Assert.Equal([1555, 1560], release.Items);
    }

    [Fact]
    public void AReleaseOfNoVersionAndNoItemsIsNoReleaseAtAll()
    {
        var report = WorkItemsReport.Parse("""
            { "releases": [ { "sha": "a1b2c3d" }, { "version": "+buildonly" }, { "version": "   ", "items": [] }, { }, 4, "2.4.90", [ ] ] }
            """)!;

        Assert.Empty(report.Releases);
    }

    [Fact]
    public void AReleaseIsShownWhateverItsItemCountFromNoneToOneItemToAVeryFullOne()
    {
        var report = WorkItemsReport.Parse("""
            { "releases": [
                { "version": "2.4.90", "items": [] },
                { "version": "2.4.91", "items": [21] },
                { "version": "2.4.92", "items": [22, 23, 24, 25, 26, 27, 28, 29] } ] }
            """)!;

        Assert.Empty(report.Releases[0].Items);
        Assert.Equal([21], report.Releases[1].Items);
        Assert.Equal([22, 23, 24, 25, 26, 27, 28, 29], report.Releases[2].Items);
        Assert.Null(WorkItemsText.Carries(report, "2.4.90"));
        Assert.Equal("carries #21", WorkItemsText.Carries(report, "2.4.91"));
        Assert.Equal("carries #22, #23, #24, #25, #26, #27, #28, #29", WorkItemsText.Carries(report, "2.4.92"));
    }

    [Fact]
    public void AnItemNumberTheFileCannotCountIsLeftOutOfTheRelease()
    {
        var report = WorkItemsReport.Parse("""
            { "releases": [ { "version": "2.4.90", "items": [12, 0, -1, 4.2, 4.8, "7", null, [8], { "n": 9 }, true, 2147483646, 3000000000] } ] }
            """)!;

        Assert.Equal([12, 0, 4, 5, 2147483646], Assert.Single(report.Releases).Items);
    }

    [Fact]
    public void ReleasesThatAreNotThereAreAnEmptyListRatherThanNothingToCheck()
    {
        Assert.Empty(WorkItemsReport.Parse("{ }")!.Releases);
        Assert.Empty(WorkItemsReport.Parse("""{ "releases": [] }""")!.Releases);
        Assert.Empty(WorkItemsReport.Parse("""{ "releases": { "2.4.90": [12] } }""")!.Releases);
        Assert.Empty(WorkItemsReport.Parse("""{ "releases": null }""")!.Releases);
    }

    // ---- WorkRelease: the line the delivery cards print ----

    [Fact]
    public void AReleaseIsFoundByItsVersionWithoutTheBuildAndItsItemsComeOutLowestFirstOnceEach()
    {
        var report = WorkItemsReport.Parse("""
            { "releases": [
                { "version": "2.4.90+aaa", "items": [15, 3] },
                { "version": "2.4.90+bbb", "items": [3, 7] },
                { "version": "2.4.91-RC", "items": [21] },
                { "version": "2.4.92", "items": [] } ] }
            """)!;

        Assert.Equal("carries #3, #7, #15", WorkItemsText.Carries(report, "2.4.90"));
        Assert.Equal("carries #3, #7, #15", WorkItemsText.Carries(report, "2.4.90+ccc"));
        Assert.Equal("carries #3, #7, #15", WorkItemsText.Carries(report, " 2.4.90 "));
        Assert.Equal("carries #21", WorkItemsText.Carries(report, "2.4.91-rc"));
        Assert.Null(WorkItemsText.Carries(report, "2.4.92"));
        Assert.Null(WorkItemsText.Carries(report, "2.4.99"));
        Assert.Null(WorkItemsText.Carries(report, "+buildonly"));
        Assert.Null(WorkItemsText.Carries(report, null));
        Assert.Null(WorkItemsText.Carries(null, "2.4.90"));
    }

    [Fact]
    public void TheReleaseOfTheVersionNobodyPublishedCarriesNothing()
    {
        var report = WorkItemsReport.Parse("""{ "releases": [ { "version": "2.4.90", "items": [12] } ] }""")!;

        Assert.Equal("carries #12", WorkItemsText.Carries(report, "2.4.90"));
        Assert.Null(WorkItemsText.Carries(WorkItemsReport.Parse("{ }")!, "2.4.90"));
    }

    // ---- The two records in the files the page really reads ----

    [Fact]
    public void TheShippedSampleIsFourReleasesAndItsBotSliceNamesTheFiveIdentities()
    {
        var sample = WorkItemsReport.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "workitems.sample.json")))!;

        Assert.Equal(["2.4.88", "2.4.86", "2.4.80", "2.4.70"], sample.Releases.Select(release => release.Version));
        Assert.Equal("a1b2c3d4e5f6789012345678abcdef9012345678", sample.Releases[0].Sha);
        Assert.Equal([470], sample.Releases[0].Items);
        Assert.Equal([462, 458], sample.Releases[1].Items);
        Assert.Equal([455], sample.Releases[2].Items);
        Assert.Equal([440], sample.Releases[3].Items);
        Assert.Equal("carries #458, #462", WorkItemsText.Carries(sample, "2.4.86+bbccdd"));

        // The whole board gives no rows; the bot slice gives them in the file's own order, and they win over the items.
        Assert.Null(sample.Stats.Throughput);
        Assert.Null(sample.TeamStats!.Throughput);
        Assert.Equal(
        [
            new WorkThroughput("dashboard-bot", 2, 1),
            new WorkThroughput("cluster-bot", 1, 1),
            new WorkThroughput("cost-bot", 1, 1),
            new WorkThroughput("delivery-bot", 1, 0),
            new WorkThroughput("runtime-bot", 1, 0),
        ],
            sample.BotStats!.Throughput!);
        Assert.Equal(sample.BotStats.Throughput, WorkBotText.Throughput(sample.BotStats, sample.Items));

        // Without the file's rows the same eleven items are counted by the bot that handled them.
        var rows = WorkBotText.Throughput(null, sample.Items);

        Assert.Equal(["unnamed", "cluster-bot", "cost-bot", "delivery-bot", "runtime-bot", "board-bot", "dashboard-bot", "design-bot"], rows.Select(row => row.Agent));
        Assert.Equal([2, 1, 1, 1, 1, 0, 0, 0], rows.Select(row => row.Merged));
        Assert.Equal([1, 1, 1, 0, 0, 0, 0, 0], rows.Select(row => row.DeployedProd));
        Assert.All(rows, row => Assert.Null(row.Done));
    }

    [Fact]
    public void TheFeedFixtureCountsItsBotWorkByAgentNameAndCarriesItsItemsByRelease()
    {
        var feed = WorkItemsReport.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "feed-workitems.json")))!;

        Assert.Equal([new WorkThroughput("cm-ai-ops-bot", null, null, 1)], feed.BotStats!.Throughput!);
        Assert.Equal("cm-ai-ops-bot: 1 done", WorkBotText.ThroughputLine(Assert.Single(feed.BotStats.Throughput!)));
        Assert.Null(feed.Stats.Throughput);

        Assert.Equal(["2.4.2064", "2.4.2062", "2.4.2060"], feed.Releases.Select(release => release.Version));
        Assert.Equal([1555, 1560, 1565], feed.Releases[2].Items);
        Assert.Equal("carries #1555, #1560, #1565", WorkItemsText.Carries(feed, "2.4.2060"));
        Assert.Equal("carries #1564", WorkItemsText.Carries(feed, "2.4.2064"));
    }
}
