namespace Dashboard.Tests;

/// <summary>
/// The last boundaries of <see cref="WorkItemsText" /> that <see cref="WorkItemsTextEdgeCaseTests" />,
/// <see cref="WorkItemsTextBoundaryTests" />, <see cref="WorkItemsTextTests" />, <see cref="WorkWeekChartTests" />,
/// <see cref="WorkWipTests" /> and <see cref="WorkThroughputAndReleaseTests" /> leave open: a rework rate of exactly
/// one, a bar taller than its scale, a later visit with no time, a cost that does not divide evenly, a blank or
/// unnamed release version, column ids of non-ASCII names, and the exact spelling a merged pull must have.
/// Fixed inputs only: no network, no clock.
/// </summary>
public class WorkItemsTextRemainingEdgeCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 22, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ThreeHoursAgo = new(2026, 11, 9, 19, 30, 0, TimeSpan.Zero);

    private static WorkItem Item(
        int? number = null,
        string? column = null,
        IReadOnlyList<ColumnVisit>? history = null,
        double? costUsd = null,
        WorkWorker worker = WorkWorker.Unknown) =>
        new(number, "An item", null, null, column, history ?? [], null, null, [], [], null, WorkItemEnvironments.None, costUsd, worker);

    private static WorkItemsReport Releases(params WorkRelease[] releases) =>
        new(null, null, WorkItemsStats.Empty, [], releases, null, null);

    // ---- Rework: where a fraction stops ----

    [Theory]
    [InlineData(1d, "100 %")]
    [InlineData(0d, "0 %")]
    [InlineData(0.5, "50 %")]
    public void ARateOfAtMostOneIsAPercentAndOneItselfIsAHundred(double rate, string text) =>
        Assert.Equal(text, WorkItemsText.Rework(new WorkRework(null, null, rate)));

    // ---- Duration: the parts a median leaves out ----

    [Fact]
    public void AP85AndACountWithoutAMedianAreSaidTogether() =>
        Assert.Equal("p85 3 d · 4 items", WorkItemsText.Duration(new WorkDuration(null, 72, 4)));

    [Fact]
    public void AMedianAndACountWithoutAP85AreSaidTogether() =>
        Assert.Equal("median 30 min · 1 item", WorkItemsText.Duration(new WorkDuration(0.5, null, 1)));

    // ---- BarHeight: above the scale ----

    [Theory]
    [InlineData(2, 1, 72d)]
    [InlineData(9, 6, 54d)]
    [InlineData(int.MaxValue, int.MaxValue, 36d)]
    [InlineData(1, int.MaxValue, 0d)]
    public void ABarIsItsShareOfTheScaleEvenPastTheTopAndRoundsToTwoPlaces(int value, int scale, double height) =>
        Assert.Equal(height, WorkItemsText.BarHeight(value, scale));

    // ---- Entered and Since: a later visit with no time ----

    [Fact]
    public void ALaterVisitWithoutATimeLeavesTheEarlierTimeStanding()
    {
        var item = Item(1, WorkColumns.Review, [new ColumnVisit(WorkColumns.Review, ThreeHoursAgo), new ColumnVisit(WorkColumns.Review, null)]);

        Assert.Equal(ThreeHoursAgo, WorkItemsText.Entered(item, WorkColumns.Review));
        Assert.Equal("since 3 h ago", WorkItemsText.Since(item, Now));
    }

    [Fact]
    public void AnItemInAColumnItsHistoryOnlyNamesElsewhereHasNoSince() =>
        Assert.Null(WorkItemsText.Since(Item(1, WorkColumns.Review, [new ColumnVisit(WorkColumns.Todo, ThreeHoursAgo)]), Now));

    // ---- Cost: amounts that do not divide evenly ----

    [Fact]
    public void APerItemAmountIsRoundedToTheCent() =>
        Assert.Equal("$3.33 per item · $10.00 total", WorkItemsText.Cost(new WorkCost(10, null), [Item(1, costUsd: 1), Item(2), Item(3)]));

    [Fact]
    public void SmallMeteredAmountsSumWithoutFloatingNoise() =>
        Assert.Equal("$0.15 per item · $0.30 total", WorkItemsText.Cost(null, [Item(1, costUsd: 0.1), Item(2, costUsd: 0.2)]));

    [Fact]
    public void APublishedTotalWithItsNoteIsSaidLast() =>
        Assert.Equal(
            "$1.50 per item · $3.00 total · Cursor agent usage",
            WorkItemsText.Cost(new WorkCost(3, "Cursor agent usage"), [Item(1, costUsd: 1), Item(2)]));

    [Fact]
    public void NoMeteredItemIsNoCostEvenWithAPublishedTotal() =>
        Assert.Null(WorkItemsText.Cost(new WorkCost(10, "Cursor agent usage"), [Item(1), Item(2)]));

    // ---- Carries: blank and unnamed versions ----

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("+abc")]
    public void AVersionThatIsBlankCarriesNothing(string version) =>
        Assert.Null(WorkItemsText.Carries(Releases(new WorkRelease("2.4.90", null, [12])), version));

    [Fact]
    public void AReleaseWithNoVersionIsNeverTheOneAsked() =>
        Assert.Null(WorkItemsText.Carries(Releases(new WorkRelease(null, "abc", [12])), "2.4.90"));

    [Fact]
    public void TheSameItemInTwoBuildsOfOneVersionIsCarriedOnce() =>
        Assert.Equal(
            "carries #0, #12",
            WorkItemsText.Carries(Releases(new WorkRelease("2.4.90+a", null, [12, 0]), new WorkRelease("2.4.90+b", null, [12])), "2.4.90"));

    // ---- ColumnId: names that are not plain ASCII ----

    [Theory]
    [InlineData("Révision", "work-col-r-vision")]
    [InlineData("in_progress", "work-col-in-progress")]
    [InlineData("42", "work-col-42")]
    [InlineData("-Todo-", "work-col-todo")]
    [InlineData(WorkColumns.InProgress, "work-col-in-progress")]
    [InlineData(WorkColumns.Unplaced, "work-col-unplaced")]
    public void EveryCharacterThatIsNotAnAsciiLetterOrDigitSplitsTheId(string column, string id) =>
        Assert.Equal(id, WorkItemsText.ColumnId(column));

    // ---- PullLabel: numbers at the edges ----

    [Theory]
    [InlineData(-1, "#-1")]
    [InlineData(int.MaxValue, "#2147483647")]
    public void APullNumberIsShownAsTheFileGaveIt(int number, string label) =>
        Assert.Equal(label, WorkItemsText.PullLabel(new WorkPullRequest(number, null, null, null, null)));

    // ---- IsMerged: exact word only ----

    [Theory]
    [InlineData(" merged ")]
    [InlineData("merged\n")]
    [InlineData("merge")]
    [InlineData("mergedd")]
    public void OnlyTheWordMergedItselfIsMerged(string state) => Assert.False(WorkItemsText.IsMerged(state));

    // ---- Select and Active ----

    [Fact]
    public void AFilterNoItemBelongsToIsEmpty()
    {
        IReadOnlyList<WorkItem> items = [Item(1, worker: WorkWorker.Team), Item(2)];

        Assert.Empty(WorkItemsText.Select(items, WorkAudience.Bot));
        Assert.Equal([1], WorkItemsText.Select(items, WorkAudience.Team).Select(item => item.Number));
    }

    [Fact]
    public void AColumnSpelledInAnyCaseIsGroupedUnderTheBoardsSpelling()
    {
        var group = Assert.Single(WorkItemsText.Active([Item(1, "todo"), Item(2, "TODO"), Item(3, "done")]));

        Assert.Equal(WorkColumns.Todo, group.Column);
        Assert.Equal([1, 2], group.Items.Select(item => item.Number));
    }
}
