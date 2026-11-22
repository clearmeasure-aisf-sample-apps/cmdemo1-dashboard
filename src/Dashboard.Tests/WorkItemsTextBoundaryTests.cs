namespace Dashboard.Tests;

/// <summary>
/// The boundaries of <see cref="WorkItemsText" /> that <see cref="WorkItemsTextEdgeCaseTests" />,
/// <see cref="WorkItemsTextTests" />, <see cref="WorkItemsTests" />, <see cref="WorkWeekChartTests" /> and
/// <see cref="WorkWipTests" /> leave open: where a rework rate stops being a fraction and how it rounds, where a
/// duration turns from minutes to hours to days, the scope line of a file that names little, the unplaced group of
/// <see cref="WorkItemsText.Active" />, a published zero cost with its note, and the out-of-range states that have no
/// words. Fixed inputs only: no network, no clock.
/// </summary>
public class WorkItemsTextBoundaryTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 22, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ThreeHoursAgo = new(2026, 11, 9, 19, 30, 0, TimeSpan.Zero);

    private static WorkItem Item(int? number, string? column = null, IReadOnlyList<ColumnVisit>? history = null, double? costUsd = null) =>
        new(number, "An item", null, null, column, history ?? [], null, null, [], [], null, WorkItemEnvironments.None, costUsd);

    // ---- Rework rate ----

    [Theory]
    [InlineData(0.12345, "12.3 %")]
    [InlineData(0.0004, "0 %")]
    [InlineData(0.999, "99.9 %")]
    [InlineData(0.9999, "100 %")]
    [InlineData(-0.25, "-25 %")]
    [InlineData(1.0001, "1")]
    [InlineData(1.234, "1.23")]
    [InlineData(1.5, "1.5")]
    [InlineData(250, "250")]
    public void ARateRoundsToOneDecimalAsAPercentAndToTwoAsANumber(double rate, string text) =>
        Assert.Equal(text, WorkItemsText.Rework(new WorkRework(null, null, rate)));

    [Fact]
    public void FailuresAndARateWithoutRevertsLeaveTheRevertsOut() =>
        Assert.Equal("3 functional-test failures · 50 %", WorkItemsText.Rework(new WorkRework(3, null, 0.5)));

    [Fact]
    public void RevertsAloneAreSaidAlone() =>
        Assert.Equal("2 reverts", WorkItemsText.Rework(new WorkRework(null, 2, null)));

    // ---- Duration ----

    [Theory]
    [InlineData(0d, "median 0 min")]
    [InlineData(0.5, "median 30 min")]
    [InlineData(1d, "median 1 h")]
    [InlineData(18.5, "median 18.5 h")]
    [InlineData(47.9, "median 47.9 h")]
    [InlineData(48d, "median 2 d")]
    [InlineData(72d, "median 3 d")]
    public void AMedianAloneIsSaidInMinutesHoursOrDays(double hours, string text) =>
        Assert.Equal(text, WorkItemsText.Duration(new WorkDuration(hours, null, null)));

    [Fact]
    public void AP85AloneIsSaidAlone() =>
        Assert.Equal("p85 3 d", WorkItemsText.Duration(new WorkDuration(null, 72, null)));

    [Fact]
    public void TheThreePartsAreSaidInOrderJoinedByADot() =>
        Assert.Equal("median 18.5 h · p85 3 d · 11 items", WorkItemsText.Duration(new WorkDuration(18.5, 72, 11)));

    // ---- Scope ----

    [Fact]
    public void NoScopeIsNoLine() => Assert.Null(WorkItemsText.Scope(null));

    [Fact]
    public void AScopeWithNoRepoAndNoProjectIsNoLine() =>
        Assert.Null(WorkItemsText.Scope(new WorkItemsScope(null, null, "worker bot")));

    [Fact]
    public void ARepoThatIsNotAnAddressIsShownAsGivenWithNoLink()
    {
        var line = WorkItemsText.Scope(new WorkItemsScope("clearmeasure/cmdemo1", null, null));

        Assert.Equal(new WorkScopeLine("clearmeasure/cmdemo1", null, null), line);
        Assert.Null(line!.Title);
    }

    [Fact]
    public void AnAddressWithNoPathIsLabelledByItsHost()
    {
        var line = WorkItemsText.Scope(new WorkItemsScope("https://github.com/", 0, null));

        Assert.Equal("github.com", line!.Label);
        Assert.Equal(new Uri("https://github.com/"), line.Url);
        Assert.Equal("0", line.Project);
        Assert.Equal("github.com (opens in a new tab)", line.Title);
    }

    [Fact]
    public void AProjectWithNoRepoIsStillALine() =>
        Assert.Equal(new WorkScopeLine(null, null, "-1"), WorkItemsText.Scope(new WorkItemsScope(null, -1, null)));

    // ---- Active ----

    [Fact]
    public void ABlankOrInventedColumnIsUnplacedAfterTheBoardsColumnsAndDoneIsLeftOut()
    {
        IReadOnlyList<WorkItem> items =
        [
            Item(1, "  "),
            Item(2, WorkColumns.Todo),
            Item(3, null),
            Item(4, "Parked"),
            Item(5, WorkColumns.Done),
        ];

        var groups = WorkItemsText.Active(items);

        Assert.Equal([WorkColumns.Todo, WorkColumns.Unplaced], groups.Select(group => group.Column));
        Assert.Equal([1, 3, 4], groups[1].Items.Select(item => item.Number));
    }

    // ---- Since ----

    [Fact]
    public void SinceMatchesTheColumnInAnyCase() =>
        Assert.Equal(
            "since 3 h ago",
            WorkItemsText.Since(Item(1, "review", [new ColumnVisit(WorkColumns.Review, ThreeHoursAgo)]), Now));

    // ---- Cost ----

    [Fact]
    public void APublishedZeroTotalKeepsItsNote() =>
        Assert.Equal(
            "$0.00 per item · $0.00 total · Cursor agent usage",
            WorkItemsText.Cost(new WorkCost(0, "Cursor agent usage"), [Item(1, costUsd: 2), Item(2)]));

    // ---- Out of range ----

    [Fact]
    public void AnAvailabilityThatIsNoneOfTheFourHasNoSentence() =>
        Assert.Equal(
            "availability",
            Assert.Throws<ArgumentOutOfRangeException>(() => WorkItemsText.Unavailable((WorkItemsAvailability)9)).ParamName);
}
