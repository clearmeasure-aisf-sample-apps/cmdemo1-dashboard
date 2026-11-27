namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of the eleven public members of <see cref="WorkBotText" />
/// that the other files leave open.
/// <para>
/// What is pinned elsewhere is left alone: <see cref="WorkBotTextTests" /> pins the Wip split, the cycle and
/// lead times, the rework and the cost of <see cref="WorkBotText.FromItems" />, the words of a stay, the word
/// of a merged pull and the heading of a note; <see cref="WorkBotTextPercentileTests" /> pins the median and
/// p85 every duration is summarized by; <see cref="WorkBotTextWeekEdgeCaseTests" /> pins the week a merge or a
/// deployment falls into; <see cref="ColumnWaitTests" /> pins what <see cref="WorkBotText.ColumnWaits" /> and
/// <see cref="WorkBotText.Longest" /> put in their lists and the stays of a <see cref="WorkBotText.Timeline" />;
/// and <see cref="WorkThroughputAndReleaseTests" /> pins the rows of <see cref="WorkBotText.Throughput" /> and
/// the line <see cref="WorkBotText.ThroughputLine" /> prints of them. What is left is pinned here with fixed
/// inputs: no network, no clock.
/// </para>
/// </summary>
public class WorkBotTextEdgeCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 22, 0, 0, TimeSpan.Zero);

    private static DateTimeOffset At(int day, int hour = 0) => new(2026, 11, day, hour, 0, 0, TimeSpan.Zero);

    private static WorkItem Item(
        int number,
        string? column = null,
        string? agent = null,
        IReadOnlyList<ColumnVisit>? history = null,
        IReadOnlyList<WorkPullRequest>? pulls = null,
        WorkItemEnvironments? environments = null,
        double? cost = null) =>
        new(new WorkItemIdentity(number, $"Item {number}", null, null, column), history ?? [], null, agent, [], pulls ?? [], null, environments ?? WorkItemEnvironments.None, cost);

    private static WorkPullRequest Pull(int? number = null, string? state = null, DateTimeOffset? mergedAt = null, string? checks = null, string? sha = null) =>
        new(number, null, state, mergedAt, sha, checks);

    private static WorkItemsStats PublishedTimes(params (string Column, WorkDuration Duration)[] times) =>
        new(new Dictionary<string, int>(), null, null, [], null, null, times.ToDictionary(pair => pair.Column, pair => pair.Duration, StringComparer.OrdinalIgnoreCase));

    // ---- FromItems ----

    /// <summary>The guard of the one argument says the name of it, as <see cref="WorkBotTextTests" /> asserts the throw
    /// without the name.</summary>
    [Fact]
    public void NoItemsAreRefusedByTheNameOfTheArgument() =>
        Assert.Equal("items", Assert.Throws<ArgumentNullException>(() => WorkBotText.FromItems(null!)).ParamName);

    /// <summary>Items that are all in Done are no work in progress: every active column counts zero, Done is not
    /// among the columns and nothing is unplaced.</summary>
    [Fact]
    public void ItemsThatAreAllDoneAreEveryColumnAtZeroAndNoUnplaced()
    {
        var stats = WorkBotText.FromItems([Item(1, "Done"), Item(2, "done"), Item(3, "DONE")]);

        Assert.Equal(6, stats.WipByColumn.Count);
        Assert.All(stats.WipByColumn, pair => Assert.Equal(0, pair.Value));
        Assert.False(stats.WipByColumn.ContainsKey(WorkColumns.Unplaced));
        Assert.False(stats.WipByColumn.ContainsKey(WorkColumns.Done));
    }

    /// <summary>The time an item took is read out of its history, not out of the column it sits in: an item that
    /// sits in Done but whose history never says when it entered it is in neither the cycle nor the lead.</summary>
    [Fact]
    public void AnItemSittingInDoneButWhoseHistoryNeverSaysWhenIsInNoTime()
    {
        var stats = WorkBotText.FromItems([Item(1, "Done", history: [new ColumnVisit("Review", At(1, 2))])]);

        Assert.Null(stats.CycleTimeHours);
        Assert.Null(stats.LeadTimeHours);
    }

    /// <summary>A metered cost that is below zero is totalled as it stands, like the positive ones beside it.</summary>
    [Fact]
    public void ACostThatGoesNegativeIsTotalledAsItStands()
    {
        var stats = WorkBotText.FromItems([Item(1, "Review", cost: 1.5), Item(2, "Review", cost: -0.5)]);

        Assert.Equal(new WorkCost(1.0, null), stats.Cost);
    }

    // ---- ColumnWaits ----

    /// <summary>A file that gives a duration with no median for every column is still a file: its columns are
    /// listed, the items' own stays are not read, and with no median there is no wait to mark longest.</summary>
    [Fact]
    public void WaitsWithNoMedianAtAllAreListedButMarkNothing()
    {
        var published = PublishedTimes(("Review", new WorkDuration(null, 5, 2)), ("UAT", new WorkDuration(null, 7, 1)));

        var waits = WorkBotText.ColumnWaits(published, [Item(1, "Review", history: [new ColumnVisit("Review", At(9, 12))])], Now);

        Assert.Equal(
        [
            new ColumnWait("Review", new WorkDuration(null, 5, 2), false),
            new ColumnWait("UAT", new WorkDuration(null, 7, 1), false),
        ],
            waits);
        Assert.Null(WorkBotText.Longest(waits));
    }

    /// <summary>The sort stands an unsaid median on the hour below the hours: a column whose median is not said
    /// stands above one whose median is a negative hour, and a list whose top is unsaid marks nothing.</summary>
    [Fact]
    public void AnUnmeasuredWaitStandsAboveANegativeOneAndNeitherIsLongest()
    {
        var published = PublishedTimes(("Review", new WorkDuration(null, 5, 2)), ("UAT", new WorkDuration(-5, -3, 1)));

        var waits = WorkBotText.ColumnWaits(published, [], Now);

        Assert.Equal(["Review", "UAT"], waits.Select(wait => wait.Column));
        Assert.All(waits, wait => Assert.False(wait.Longest));
        Assert.Null(WorkBotText.Longest(waits));
    }

    /// <summary>The clock the waits are asked with moves only the stay the item is still in: a stay the file left
    /// closed stands on the same hours a day later, and the open one stands longer by the day.</summary>
    [Fact]
    public void TheClockMovesOnlyTheStayTheItemIsStillIn()
    {
        var open = Item(1, "UAT", history: [new ColumnVisit("UAT", At(9, 12))]);
        var closed = Item(2, "Done", history: [new ColumnVisit("Review", At(9, 8), At(9, 13), 5.0)]);

        var now = WorkBotText.ColumnWaits(null, [open, closed], Now);
        var later = WorkBotText.ColumnWaits(null, [open, closed], Now.AddHours(5));

        Assert.Equal(new ColumnWait("Review", new WorkDuration(5, 5, 1), false), now.Single(wait => wait.Column == "Review"));
        Assert.Equal(new ColumnWait("Review", new WorkDuration(5, 5, 1), false), later.Single(wait => wait.Column == "Review"));
        Assert.Equal(new ColumnWait("UAT", new WorkDuration(10, 10, 1), true), now.Single(wait => wait.Column == "UAT"));
        Assert.Equal(new ColumnWait("UAT", new WorkDuration(15, 15, 1), true), later.Single(wait => wait.Column == "UAT"));
    }

    // ---- Longest ----

    /// <summary>The wait that carries the mark is the one that comes back where ever it stands in the list, not
    /// only when it stands first.</summary>
    [Fact]
    public void TheMarkedWaitIsReturnedWhereverItStandsInTheList()
    {
        ColumnWait[] waits =
        [
            new ColumnWait("Review", new WorkDuration(2, 2, 1), false),
            new ColumnWait("UAT", new WorkDuration(9, 9, 1), true),
            new ColumnWait("TDD", new WorkDuration(1, 1, 1), false),
        ];

        Assert.Equal(waits[1], WorkBotText.Longest(waits));
    }

    /// <summary>Two waits that carry the mark give the one first in the list, not the first by the name of the
    /// column: the list is not sorted again on the way out.</summary>
    [Fact]
    public void TwoMarkedWaitsGiveTheOneFirstInTheListNotTheFirstByName()
    {
        var uat = new ColumnWait("UAT", new WorkDuration(4, 4, 1), true);
        var tdd = new ColumnWait("TDD", new WorkDuration(4, 4, 1), true);

        Assert.Equal(uat, WorkBotText.Longest([uat, tdd]));
        Assert.Equal(tdd, WorkBotText.Longest([tdd, uat]));
    }

    /// <summary>The guard of the one argument says the name of it, as <see cref="ColumnWaitTests" /> asserts the
    /// throw without the name.</summary>
    [Fact]
    public void WaitsThatAreNothingAreRefusedByTheNameOfTheArgument() =>
        Assert.Equal("waits", Assert.Throws<ArgumentNullException>(() => WorkBotText.Longest(null!)).ParamName);

    // ---- Throughput ----

    /// <summary>A production deployment the file gave no time of is still a deployment: the row counts it, and
    /// the item's own pulls stand beside it at zero.</summary>
    [Fact]
    public void AProductionDeploymentWithoutATimeStillCountsAsDeployed()
    {
        var row = Assert.Single(WorkBotText.Throughput(null, [Item(1, agent: "alpha", environments: new WorkItemEnvironments(null, null, new WorkDeployment(null, null)))]));

        Assert.Equal(1, row.DeployedProd);
        Assert.Equal(0, row.Merged);
    }

    /// <summary>Only an agent that is not there at all is unnamed: an agent that is only spaces keeps the spaces
    /// it was given.</summary>
    [Fact]
    public void AnAgentThatIsOnlySpacesKeepsTheSpaces()
    {
        var row = Assert.Single(WorkBotText.Throughput(null, [Item(1, agent: "  ")]));

        Assert.Equal("  ", row.Agent);
    }

    /// <summary>An item counts for its agent once, whether the word or the time says it merged, and an item whose
    /// pulls are all open stands beside them at nothing: the count is of the items, not of the pulls.</summary>
    [Fact]
    public void AnItemCountsOncePerMergedPullAndAnOpenOnlyItemCountsNothing()
    {
        var mergedByWord = Item(1, agent: "alpha", pulls: [Pull(11, "merged"), Pull(13, "open")]);
        var mergedByTime = Item(2, agent: "alpha", pulls: [Pull(12, null, At(9, 3))]);
        var openOnly = Item(3, agent: "alpha", pulls: [Pull(14, "open")]);

        var row = Assert.Single(WorkBotText.Throughput(null, [mergedByWord, mergedByTime, openOnly]));

        Assert.Equal(2, row.Merged);
    }

    /// <summary>The guard of the items says the name of the argument, whether or not the file's rows are there.</summary>
    [Fact]
    public void ItemsThatAreNothingAreRefusedByTheNameOfTheArgument()
    {
        Assert.Equal("items", Assert.Throws<ArgumentNullException>(() => WorkBotText.Throughput(null, null!)).ParamName);
        Assert.Equal("items", Assert.Throws<ArgumentNullException>(() => WorkBotText.Throughput(WorkItemsStats.Empty, null!)).ParamName);
    }

    // ---- ThroughputLine ----

    /// <summary>The done count is said only when the file gave neither merges nor deployments: with either of
    /// them there, the line stands on the merged and deployed numbers and the done count is not read.</summary>
    [Theory]
    [InlineData("alpha", 5, null, 2, "alpha: 0 merged, 2 deployed to prod")]
    [InlineData("alpha", 5, 3, null, "alpha: 3 merged, 0 deployed to prod")]
    [InlineData("", 1, null, null, ": 1 done")]
    public void TheDoneCountIsSilentWheneverTheFileGaveMergesOrDeploys(string agent, int? done, int? merged, int? deployed, string line) =>
        Assert.Equal(line, WorkBotText.ThroughputLine(new WorkThroughput(agent, merged, deployed, done)));

    /// <summary>The guard of the one argument says the name of it, as <see cref="WorkThroughputAndReleaseTests" />
    /// asserts the throw without the name.</summary>
    [Fact]
    public void ARowThatIsNothingIsRefusedByTheNameOfTheArgument() =>
        Assert.Equal("row", Assert.Throws<ArgumentNullException>(() => WorkBotText.ThroughputLine(null!)).ParamName);

    // ---- NoteHeading ----

    /// <summary>The heading reads the kind of the note, and a note that is not there has no kind to read: the
    /// read fails, and there is no guard to name an argument.</summary>
    [Fact]
    public void ANoteThatIsNothingSaysNothing() =>
        Assert.Throws<NullReferenceException>(() => WorkBotText.NoteHeading(null!));

    /// <summary>The word is said exactly as the file says it: a blocker with a space in it is a status, the
    /// leading space beside the trailing one <see cref="WorkBotTextTests" /> already pins.</summary>
    [Theory]
    [InlineData("blocker ")]
    [InlineData("BLOCKER ")]
    public void ABlockerWithASpaceInItIsOnlyAStatus(string kind) =>
        Assert.Equal("Status", WorkBotText.NoteHeading(new WorkNote("Waiting on review", null, null, kind)));

    // ---- MergeToProd ----

    /// <summary>The guard of the items says the name of the argument, as <see cref="WorkItemsTests" /> and
    /// <see cref="WorkBotTextPercentileTests" /> assert the happy path without it.</summary>
    [Fact]
    public void TheWaitFromMergeToProdRefusesNoItemsByTheNameOfTheArgument() =>
        Assert.Equal("items", Assert.Throws<ArgumentNullException>(() => WorkBotText.MergeToProd(null, null!)).ParamName);

    /// <summary>The file's hours win verbatim: the very duration it gave comes back, unsaid median and all, and
    /// the items are not read at all.</summary>
    [Fact]
    public void TheFilesHoursWinVerbatim()
    {
        var given = new WorkDuration(null, 7, 9);
        var published = new WorkItemsStats(new Dictionary<string, int>(), null, null, [], null, null, null, null, given);

        Assert.Same(given, WorkBotText.MergeToProd(published, [Item(1, "Done", pulls: [Pull(11, null, At(2, 0))], environments: new WorkItemEnvironments(null, null, new WorkDeployment("2.4.1", At(2, 4))))]));
    }

    /// <summary>A production deployment that came before the pull merged is no wait at all: the item is not
    /// counted, and an item with nothing else to count is nothing.</summary>
    [Fact]
    public void ADeploymentThatCameBeforeTheMergeIsNoWait()
    {
        var item = Item(1, "Done", pulls: [Pull(11, null, At(2, 12))], environments: new WorkItemEnvironments(null, null, new WorkDeployment("2.4.1", At(2, 9))));

        Assert.Null(WorkBotText.MergeToProd(null, [item]));
    }

    /// <summary>A production deployment in the one moment the pull merged is zero hours, not no wait.</summary>
    [Fact]
    public void ADeploymentAtTheMergeIsZeroHours()
    {
        var item = Item(1, "Done", pulls: [Pull(11, null, At(2, 12))], environments: new WorkItemEnvironments(null, null, new WorkDeployment("2.4.1", At(2, 12))));

        Assert.Equal(new WorkDuration(0, 0, 1), WorkBotText.MergeToProd(null, [item]));
    }

    /// <summary>The wait starts at the first pull that merged, when an item merged more than once.</summary>
    [Fact]
    public void TheEarliestMergedPullStartsTheWait()
    {
        var item = Item(
            1,
            "Done",
            pulls: [Pull(11, null, At(2, 6)), Pull(12, null, At(2, 0))],
            environments: new WorkItemEnvironments(null, null, new WorkDeployment("2.4.1", At(2, 18))));

        Assert.Equal(new WorkDuration(18, 18, 1), WorkBotText.MergeToProd(null, [item]));
    }

    /// <summary>The word of a merge without a time and the deployment without a time each fail on their own: an
    /// item that says one or the other, or both without the moments, is no wait.</summary>
    [Fact]
    public void AWordOfMergedWithoutATimeAndADeploymentWithoutATimeAreNoWait()
    {
        var wordOnly = Item(1, "Done", pulls: [Pull(11, "merged")], environments: new WorkItemEnvironments(null, null, new WorkDeployment("2.4.1", At(2))));
        var timeOnly = Item(2, "Done", pulls: [Pull(12, null, At(2))], environments: new WorkItemEnvironments(null, null, new WorkDeployment("2.4.1", null)));

        Assert.Null(WorkBotText.MergeToProd(null, [wordOnly, timeOnly]));
    }

    /// <summary>An item that has only one side of the pair adds nothing to the count: the wait stands on the
    /// items that have both a moment of the merge and a moment of the deployment.</summary>
    [Fact]
    public void AnItemWithOnlyOneSideOfThePairAddsNothing()
    {
        var both = Item(1, "Done", pulls: [Pull(11, null, At(2, 0))], environments: new WorkItemEnvironments(null, null, new WorkDeployment("2.4.1", At(2, 4))));
        var pullOnly = Item(2, "Done", pulls: [Pull(12, null, At(2, 0))]);
        var deployOnly = Item(3, "Done", environments: new WorkItemEnvironments(null, null, new WorkDeployment("2.4.1", At(2, 4))));

        Assert.Equal(new WorkDuration(4, 4, 1), WorkBotText.MergeToProd(null, [both, pullOnly, deployOnly]));
    }

    // ---- Timeline ----

    /// <summary>The guard of the one argument says the name of it, as <see cref="ColumnWaitTests" /> asserts the
    /// throw without the name.</summary>
    [Fact]
    public void AnItemThatIsNothingIsRefusedByTheNameOfTheArgument() =>
        Assert.Equal("item", Assert.Throws<ArgumentNullException>(() => WorkBotText.Timeline(null!, Now)).ParamName);

    /// <summary>The hours the file gave a visit stand above the entered and left it gave beside them, when the
    /// two do not agree.</summary>
    [Fact]
    public void TheHoursTheFileGivesWinOverTheTimesItGivesBesideThem()
    {
        var item = Item(1, "Done", history: [new ColumnVisit("Review", At(9, 8), At(9, 9), 5.0)]);

        var stay = Assert.Single(WorkBotText.Timeline(item, Now));

        Assert.Equal(TimeSpan.FromHours(5), stay.Spent);
        Assert.False(stay.Open);
    }

    /// <summary>A visit the file gave hours for and no leaving is open: the stay is still in the column, on the
    /// hours the file said, with the words to match.</summary>
    [Fact]
    public void AStayTheFileGaveHoursForIsOpenWhileItHasNoLeftAt()
    {
        var item = Item(1, "Review", history: [new ColumnVisit("Review", At(9, 8), null, 3.0)]);

        var stay = Assert.Single(WorkBotText.Timeline(item, Now));

        Assert.True(stay.Open);
        Assert.Equal(TimeSpan.FromHours(3), stay.Spent);
        Assert.Equal("Review · 3 h so far", WorkBotText.Stay(stay));
    }

    /// <summary>A stay that began and ended in the one moment is the zero of an hour, not the unsaid hour the
    /// moments that disagree stand on.</summary>
    [Fact]
    public void AStayThatBeganAndEndedInTheOneMomentIsZeroHours()
    {
        var item = Item(1, "Review", history: [new ColumnVisit("Todo", At(9, 8)), new ColumnVisit("Review", At(9, 8))]);

        var stays = WorkBotText.Timeline(item, Now);

        Assert.Equal(TimeSpan.Zero, stays[0].Spent);
        Assert.False(stays[0].Open);
        Assert.Equal(TimeSpan.FromHours(14), stays[1].Spent);
        Assert.True(stays[1].Open);
    }

    /// <summary>A stay whose next visit says no time of its own is unmeasured, and the visit that says only when
    /// it was entered, with no hours beside it, is the column alone.</summary>
    [Fact]
    public void AStayWhoseNextVisitSaysNoTimeStaysUnmeasured()
    {
        var item = Item(1, "Review", history: [new ColumnVisit("Todo", At(9, 8)), new ColumnVisit("Review", null)]);

        var stays = WorkBotText.Timeline(item, Now);

        Assert.Equal("Todo", stays[0].Column);
        Assert.Null(stays[0].Spent);
        Assert.False(stays[0].Open);
        Assert.Equal("Review", stays[1].Column);
        Assert.Null(stays[1].Spent);
        Assert.False(stays[1].Open);
    }

    /// <summary>A stay the item entered after the clock stands is open, because the item is still in the
    /// column, but unmeasured, because the clock has not reached it: the stay is the column alone.</summary>
    [Fact]
    public void AStayThatBeganAfterTheClockIsOpenButUnmeasured()
    {
        var item = Item(1, "Review", history: [new ColumnVisit("Review", At(9, 23))]);

        var stay = Assert.Single(WorkBotText.Timeline(item, Now));

        Assert.True(stay.Open);
        Assert.Null(stay.Spent);
        Assert.Equal("Review", WorkBotText.Stay(stay));
    }

    /// <summary>A visit that says only when it ended, with no hours and no entering, is the column alone.</summary>
    [Fact]
    public void AVisitThatSaysOnlyWhenItEndedIsTheColumnAlone()
    {
        var item = Item(1, "Review", history: [new ColumnVisit("Review", null, At(9, 9))]);

        var stay = Assert.Single(WorkBotText.Timeline(item, Now));

        Assert.Null(stay.Spent);
        Assert.False(stay.Open);
        Assert.Equal("Review", WorkBotText.Stay(stay));
    }

    // ---- Stay ----

    /// <summary>The guard of the one argument says the name of it, as <see cref="WorkBotTextTests" /> asserts the
    /// throw without the name.</summary>
    [Fact]
    public void AStayThatIsNothingIsRefusedByTheNameOfTheArgument() =>
        Assert.Equal("stay", Assert.Throws<ArgumentNullException>(() => WorkBotText.Stay(null!)).ParamName);

    /// <summary>The words of a stay never ask when it began: a stay with no entering is said by its hours and
    /// its open mark the same way a stay with an entering is.</summary>
    [Fact]
    public void TheWordsOfAStayNeverAskWhenItBegan()
    {
        Assert.Equal("Review · 45 min", WorkBotText.Stay(new ColumnStay("Review", null, TimeSpan.FromMinutes(45), false)));
        Assert.Equal("Review · 45 min so far", WorkBotText.Stay(new ColumnStay("Review", null, TimeSpan.FromMinutes(45), true)));
    }

    /// <summary>The column of a stay is kept in its own spaces: the words are built on the name as it stands,
    /// not on a trimmed copy of it.</summary>
    [Fact]
    public void TheColumnOfAStayIsKeptInItsOwnSpaces() =>
        Assert.Equal("  Review   · 45 min", WorkBotText.Stay(new ColumnStay("  Review  ", At(1), TimeSpan.FromMinutes(45), false)));

    // ---- PullLine ----

    /// <summary>The guard of the one argument says the name of it, as <see cref="WorkItemsTests" /> pins the full
    /// line without it.</summary>
    [Fact]
    public void APullThatIsNothingIsRefusedByTheNameOfTheArgument() =>
        Assert.Equal("pull", Assert.Throws<ArgumentNullException>(() => WorkBotText.PullLine(null!, Now)).ParamName);

    /// <summary>A pull the file gave no state, checks, time or commit of is its number alone.</summary>
    [Fact]
    public void TheLineOfAPullThatSaysOnlyItsNumberIsTheNumber() =>
        Assert.Equal("#11", WorkBotText.PullLine(Pull(11), Now));

    /// <summary>A pull the file did not number is a pull request, whatever else the file said of it.</summary>
    [Fact]
    public void APullTheFileDidNotNumberIsOnlyAPullRequest() =>
        Assert.Equal("Pull request · open", WorkBotText.PullLine(Pull(null, "open"), Now));

    /// <summary>The word of the checks stands between the pull and its merge, in the letters the file gave.</summary>
    [Fact]
    public void ACheckTheFileSaysFailedIsSaidSo() =>
        Assert.Equal("#12 · checks failed", WorkBotText.PullLine(Pull(12, checks: "failed"), Now));

    /// <summary>The commit the pull closed with is cut to seven letters, and a commit shorter than that is kept
    /// as it is.</summary>
    [Fact]
    public void ACommitLongerThanSevenLettersIsCutToSevenAndAShorterOneKept()
    {
        Assert.Equal("#13 · a1b2c3d", WorkBotText.PullLine(Pull(13, sha: "a1b2c3d4e5"), Now));
        Assert.Equal("#15 · abcd", WorkBotText.PullLine(Pull(15, sha: "abcd"), Now));
    }

    /// <summary>A pull the file merged in the moment the clock stands is merged just now, and with no state and
    /// no checks the line stands on the number and the moment.</summary>
    [Fact]
    public void APullMergedWithTheClockStandingIsJustMerged() =>
        Assert.Equal("#14 · merged just now", WorkBotText.PullLine(Pull(14, null, Now), Now));

    // ---- Merged ----

    /// <summary>Merging is read out of the pull, and a pull that is not there has no state and no time to read:
    /// the read fails, and there is no guard to name an argument.</summary>
    [Fact]
    public void APullThatIsNothingSaysNothingAboutMerging() =>
        Assert.Throws<NullReferenceException>(() => WorkBotText.Merged(null!));
}
