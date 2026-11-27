namespace Dashboard.Tests;

/// <summary>
/// The factory detail of bot work, counted from the items when the file publishes no slice of it: the Wip split,
/// cycle and lead time, the weeks, rework and cost, plus the short lines the view prints.
/// </summary>
public class WorkBotTextTests
{
    private static DateTimeOffset At(int day, int hour = 0, int minute = 0) => new(2026, 11, day, hour, minute, 0, TimeSpan.Zero);

    private static WorkItem Item(
        int number,
        string? column = null,
        IReadOnlyList<ColumnVisit>? history = null,
        double? cost = null,
        WorkRework? rework = null,
        IReadOnlyList<WorkPullRequest>? pulls = null,
        WorkItemEnvironments? environments = null) =>
        new(new WorkItemIdentity(number, $"Item {number}", null, null, column), history ?? [], null, null, [], pulls ?? [], null, environments ?? WorkItemEnvironments.None, cost, WorkWorker.Unknown, rework);

    private static WorkPullRequest Pull(int? number = null, string? state = null, DateTimeOffset? mergedAt = null) =>
        new(number, null, state, mergedAt, null);

    // ---- WorkColumns, as the work text uses them ----

    [Fact]
    public void NothingFiledAndNothingKnownIsAnEmptySetOfCounts()
    {
        Assert.Equal(WorkItemsStats.Empty, WorkBotText.FromItems([]));
    }

    [Fact]
    public void NoSliceOfItemsAtAllIsRefused() => Assert.Throws<ArgumentNullException>(() => WorkBotText.FromItems(null!));

    [Fact]
    public void EveryActiveColumnIsCountedEvenWhenNothingWaitsThere()
    {
        var stats = WorkBotText.FromItems(
        [
            Item(1, "Todo"),
            Item(2, "in progress"),
            Item(3, "UAT"),
            Item(4, "Done"),
            Item(5, "done"),
        ]);

        Assert.Equal(6, stats.WipByColumn.Count);
        Assert.Equal(1, stats.WipByColumn[WorkColumns.Todo]);
        Assert.Equal(1, stats.WipByColumn[WorkColumns.InProgress]);
        Assert.Equal(0, stats.WipByColumn[WorkColumns.Review]);
        Assert.Equal(0, stats.WipByColumn[WorkColumns.Merged]);
        Assert.Equal(0, stats.WipByColumn[WorkColumns.Tdd]);
        Assert.Equal(0, stats.WipByColumn["tdd"]);
        Assert.Equal(1, stats.WipByColumn[WorkColumns.Uat]);
        Assert.False(stats.WipByColumn.ContainsKey(WorkColumns.Unplaced));
        Assert.False(stats.WipByColumn.ContainsKey(WorkColumns.Done));
    }

    [Fact]
    public void WorkThatIsNeitherKnownNorDoneIsCountedAsUnplaced()
    {
        var stats = WorkBotText.FromItems([Item(1, null), Item(2, ""), Item(3, "Blocked"), Item(4, "Review")]);

        Assert.Equal(7, stats.WipByColumn.Count);
        Assert.Equal(3, stats.WipByColumn[WorkColumns.Unplaced]);
        Assert.Equal(1, stats.WipByColumn[WorkColumns.Review]);
    }

    [Fact]
    public void CycleStartsWhereTheWorkStartsAndLeadStartsAtTheFirstVisit()
    {
        var stats = WorkBotText.FromItems(
        [
            Item(1, "Done", [new ColumnVisit(WorkColumns.Todo, At(1)), new ColumnVisit(WorkColumns.InProgress, At(1, 2)), new ColumnVisit(WorkColumns.Done, At(1, 6))]),
        ]);

        Assert.Equal(new WorkDuration(4, 4, 1), stats.CycleTimeHours);
        Assert.Equal(new WorkDuration(6, 6, 1), stats.LeadTimeHours);
    }

    [Fact]
    public void AnItemThatNeverReachedDoneIsNotInEitherTime()
    {
        var stats = WorkBotText.FromItems([Item(1, "Review", [new ColumnVisit(WorkColumns.Todo, At(1)), new ColumnVisit(WorkColumns.InProgress, At(1, 2))])]);

        Assert.Null(stats.CycleTimeHours);
        Assert.Null(stats.LeadTimeHours);
    }

    [Fact]
    public void WorkMeasuredInNoTimeAtAllIsStillOneItem()
    {
        var stats = WorkBotText.FromItems([Item(1, "Done", [new ColumnVisit(WorkColumns.Done, At(1, 6))])]);

        Assert.Equal(new WorkDuration(0, 0, 1), stats.CycleTimeHours);
        Assert.Equal(new WorkDuration(0, 0, 1), stats.LeadTimeHours);
    }

    [Fact]
    public void AReopenedItemIsTimedFromItsFirstWorkingVisitToItsLastDoneVisit()
    {
        var stats = WorkBotText.FromItems(
        [
            Item(1, "Done",
            [
                new ColumnVisit(WorkColumns.Todo, At(1)),
                new ColumnVisit(WorkColumns.Done, At(1, 6)),
                new ColumnVisit(WorkColumns.Review, At(1, 8)),
                new ColumnVisit(WorkColumns.Done, At(1, 14)),
            ]),
        ]);

        Assert.Equal(new WorkDuration(8, 8, 1), stats.CycleTimeHours);
        Assert.Equal(new WorkDuration(14, 14, 1), stats.LeadTimeHours);
    }

    [Fact]
    public void MedianAndP85ComeFromTheCompletedItemsOnly()
    {
        var stats = WorkBotText.FromItems(
        [
            Item(1, "Review", [new ColumnVisit(WorkColumns.Review, At(1))]),
            Done(2, 2),
            Done(3, 8),
            Done(4, 6),
            Done(5, 4),
        ]);

        Assert.Equal(new WorkDuration(4, 8, 4), stats.CycleTimeHours);
        Assert.Equal(new WorkDuration(4, 8, 4), stats.LeadTimeHours);
    }

    [Fact]
    public void AWaitOfNinetyMinutesIsWrittenInHours()
    {
        var stats = WorkBotText.FromItems([Done(1, 1.5)]);

        Assert.Equal(new WorkDuration(1.5, 1.5, 1), stats.CycleTimeHours);
    }

    [Fact]
    public void WeeksFollowMergeTimeAndTheProductionDeployment()
    {
        var stats = WorkBotText.FromItems(
        [
            Item(1, "Done", [], pulls: [Pull(11, mergedAt: At(4, 10))], environments: new WorkItemEnvironments(null, null, new WorkDeployment("2.4.1", At(5, 9)))),
            Item(2, "Done", [], pulls: [Pull(12, mergedAt: At(1, 23))], environments: new WorkItemEnvironments(null, null, new WorkDeployment("2.4.2", At(9, 8)))),
        ]);

        Assert.Equal(
        [
            new WorkWeek(new DateTimeOffset(2026, 10, 26, 0, 0, 0, TimeSpan.Zero), 1, 0),
            new WorkWeek(new DateTimeOffset(2026, 11, 2, 0, 0, 0, TimeSpan.Zero), 1, 1),
            new WorkWeek(new DateTimeOffset(2026, 11, 9, 0, 0, 0, TimeSpan.Zero), 0, 1),
        ],
            stats.PerWeek);
    }

    [Fact]
    public void NothingDeployedAndNothingMergedIsNoWeek() => Assert.Empty(WorkBotText.FromItems([Item(1, "Todo")]).PerWeek);

    [Fact]
    public void ReworkIsCountedOverEveryItemNotJustTheOnesThatSay()
    {
        var stats = WorkBotText.FromItems(
        [
            Item(1, "Done", rework: new WorkRework(1, 0, null)),
            Item(2, "Done", rework: new WorkRework(0, 2, null)),
            Item(3, "Review", rework: new WorkRework(null, null, null)),
            Item(4, "Review"),
        ]);

        Assert.Equal(new WorkRework(1, 2, 0.5), stats.Rework);
    }

    [Fact]
    public void CleanWorkIsZeroAndANothingSaidFileHasNoReworkAtAll()
    {
        Assert.Equal(new WorkRework(0, 0, 0), WorkBotText.FromItems([Item(1, "Review", rework: new WorkRework(0, 0, null))]).Rework);
        Assert.Null(WorkBotText.FromItems([Item(1, "Review"), Item(2, "Done")]).Rework);
    }

    [Fact]
    public void CostIsTotalledWhenAnyItemWasMeteredAndAbsentWhenNoneWas()
    {
        Assert.Equal(new WorkCost(3.5, null), WorkBotText.FromItems([Item(1, "Review", cost: 1.5), Item(2, "Review"), Item(3, "Review", cost: 2)]).Cost);
        Assert.Equal(new WorkCost(0, null), WorkBotText.FromItems([Item(1, "Review", cost: 0)]).Cost);
        Assert.Null(WorkBotText.FromItems([Item(1, "Review"), Item(2, "Review", cost: null)]).Cost);
    }

    [Fact]
    public void TheCountsFromTheItemsLeaveThePublishedSlicesEmpty()
    {
        var stats = WorkBotText.FromItems([Item(1, "Review")]);

        Assert.Null(stats.TimePerColumnHours);
        Assert.Null(stats.Throughput);
        Assert.Null(stats.MergeToProdHours);
    }

    // ---- Merged ----

    [Theory]
    [InlineData("merged", true)]
    [InlineData("Merged", true)]
    [InlineData("MERGED", true)]
    [InlineData("merged ", false)]
    [InlineData("Merged by bot", false)]
    [InlineData("open", false)]
    [InlineData("closed", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void AMergedPullIsTheWordMergedOrATime(string? state, bool merged) =>
        Assert.Equal(merged, WorkBotText.Merged(Pull(1, state)));

    [Fact]
    public void APullThatCarriesAMergeTimeIsMergedWhateverItsStateSays()
    {
        Assert.True(WorkBotText.Merged(Pull(1, "open", At(4))));
        Assert.True(WorkBotText.Merged(new WorkPullRequest(1, null, null, At(4), "a1b2c3d")));
        Assert.False(WorkBotText.Merged(new WorkPullRequest(1, null, null, null, "a1b2c3d")));
        Assert.False(WorkBotText.Merged(Pull(1)));
    }

    // ---- NoteHeading ----

    [Theory]
    [InlineData("blocker", "Blocker")]
    [InlineData("Blocker", "Blocker")]
    [InlineData("BLOCKER", "Blocker")]
    [InlineData(" blocker", "Status")]
    [InlineData("status", "Status")]
    [InlineData("STATUS", "Status")]
    [InlineData("update", "Status")]
    [InlineData("", "Status")]
    [InlineData(null, "Status")]
    public void OnlyANoteTheFeedCallsABlockerIsABlocker(string? kind, string heading) =>
        Assert.Equal(heading, WorkBotText.NoteHeading(new WorkNote("Waiting on review", null, null, kind)));

    [Fact]
    public void TheHeadingIgnoresTheWordsOfTheNote() =>
        Assert.Equal("Status", WorkBotText.NoteHeading(new WorkNote("Blocker: the pipeline is red", null, null, null)));

    // ---- Stay ----

    [Theory]
    [InlineData("TDD", null, false, "TDD")]
    [InlineData("Todo", null, true, "Todo")]
    [InlineData("Review", 45, false, "Review · 45 min")]
    [InlineData("Review", 45, true, "Review · 45 min so far")]
    [InlineData("UAT", 180, false, "UAT · 3 h")]
    [InlineData("Merged", 2880, true, "Merged · 2 d so far")]
    public void AStayIsTheColumnAndHowLongItTook(string column, int? minutes, bool open, string expected) =>
        Assert.Equal(expected, WorkBotText.Stay(new ColumnStay(column, At(1), minutes is null ? null : TimeSpan.FromMinutes(minutes.Value), open)));

    [Theory]
    [InlineData(0.0, "0 s")]
    [InlineData(-300.0, "0 s")]
    [InlineData(59.0, "59 s")]
    [InlineData(60.0, "1 min")]
    [InlineData(7199.0, "119 min")]
    [InlineData(7200.0, "2 h")]
    [InlineData(172799.0, "47 h")]
    [InlineData(172800.0, "2 d")]
    public void TheLengthOfAStayIsRoundedDownToAWholeUnit(double seconds, string expected) =>
        Assert.Equal($"Review · {expected}", WorkBotText.Stay(new ColumnStay("Review", At(1), TimeSpan.FromSeconds(seconds), false)));

    [Fact]
    public void AStayWithNoTimeIsJustTheColumnOfTheItem()
    {
        Assert.Equal("Unplaced", WorkBotText.Stay(new ColumnStay(WorkColumns.Unplaced, null, null, false)));
        Assert.Throws<ArgumentNullException>(() => WorkBotText.Stay(null!));
    }

    /// <summary>An item that reached Done after <paramref name="hours"/> of work in Review.</summary>
    private static WorkItem Done(int number, double hours) =>
        Item(number, "Done", [new ColumnVisit(WorkColumns.Review, At(1)), new ColumnVisit(WorkColumns.Done, At(1).AddHours(hours))]);
}
