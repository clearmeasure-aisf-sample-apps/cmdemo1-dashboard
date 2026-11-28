namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="WorkItemsText" /> that the other files leave open.
/// <para>
/// What is pinned elsewhere is left alone: availability and its sentences (<see cref="WorkItemsAvailabilityEdgeCaseTests" />),
/// the audience words and the item, pull, session, place and release titles (<see cref="WorkItemsTextTests" />), the
/// chart and bar heights (<see cref="WorkWeekChartTests" />), the strip of work in progress (<see cref="WorkWipTests" />),
/// the scope line (<see cref="WorkItemsScopeEdgeCaseTests" />) and the release a version carries
/// (<see cref="WorkThroughputAndReleaseTests" />). What is left is pinned here with fixed inputs: no network, no clock.
/// </para>
/// </summary>
public class WorkItemsTextEdgeCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 22, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ThreeHoursAgo = new(2026, 11, 9, 19, 30, 0, TimeSpan.Zero);

    private static WorkItem Item(
        int? number = null,
        string? column = null,
        IReadOnlyList<ColumnVisit>? history = null,
        string? assignee = null,
        string? agent = null,
        WorkItemEnvironments? environments = null,
        double? costUsd = null,
        WorkWorker worker = WorkWorker.Unknown) =>
        new(new WorkItemIdentity(number, "An item", null, null, column), history ?? [], assignee, agent, [], [], null, environments ?? WorkItemEnvironments.None, costUsd, worker);

    private static WorkItemsReport Report(IReadOnlyList<WorkItem> items, WorkItemsStats? bot = null, WorkItemsStats? team = null) =>
        new(null, null, WorkItemsStats.Empty, items, [], bot, team);

    // ---- the fixed sentences ----

    [Fact]
    public void TheFixedSentencesAreTheOnesAReaderSees()
    {
        Assert.Equal("Loading work items", WorkItemsText.Loading);
        Assert.Equal("No work item is in progress.", WorkItemsText.NoActive);
        Assert.Equal("n/a", WorkItemsText.NotMetered);
        Assert.Equal("—", WorkItemsText.Missing);
        Assert.Equal(36d, WorkItemsText.ChartHeight);
    }

    // ---- NoneOf ----

    [Theory]
    [InlineData(WorkAudience.All, "The work items file lists no items.")]
    [InlineData(WorkAudience.Bot, "No bot work items.")]
    [InlineData(WorkAudience.Team, "No team work items.")]
    public void EveryFilterHasItsOwnEmptySentence(WorkAudience audience, string sentence) =>
        Assert.Equal(sentence, WorkItemsText.NoneOf(audience));

    [Fact]
    public void AFilterThatIsNoneOfTheThreeHasNoEmptySentence() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => WorkItemsText.NoneOf((WorkAudience)7));

    // ---- Select ----

    [Fact]
    public void AllIsTheVeryListAndAnItemWithNoWorkerIsOnlyInAll()
    {
        IReadOnlyList<WorkItem> items = [Item(1, worker: WorkWorker.Bot), Item(2), Item(3, worker: WorkWorker.Team), Item(4, worker: WorkWorker.Bot)];

        Assert.Same(items, WorkItemsText.Select(items, WorkAudience.All));
        Assert.Equal([1, 4], WorkItemsText.Select(items, WorkAudience.Bot).Select(item => item.Identity.Number));
        Assert.Equal([3], WorkItemsText.Select(items, WorkAudience.Team).Select(item => item.Identity.Number));
    }

    [Theory]
    [InlineData(WorkAudience.All)]
    [InlineData(WorkAudience.Bot)]
    [InlineData(WorkAudience.Team)]
    public void NoItemsAreNoItemsInEveryFilter(WorkAudience audience) =>
        Assert.Empty(WorkItemsText.Select([], audience));

    [Fact]
    public void SelectRefusesNothingAndAFilterThatIsNoneOfTheThree()
    {
        Assert.Equal("items", Assert.Throws<ArgumentNullException>(() => WorkItemsText.Select(null!, WorkAudience.All)).ParamName);
        Assert.Throws<ArgumentOutOfRangeException>(() => WorkItemsText.Select([Item(1)], (WorkAudience)7));
    }

    // ---- Slice ----

    [Fact]
    public void SliceRefusesNothingAndAFilterThatIsNoneOfTheThree()
    {
        Assert.Equal("report", Assert.Throws<ArgumentNullException>(() => WorkItemsText.Slice(null!, WorkAudience.All)).ParamName);
        Assert.Throws<ArgumentOutOfRangeException>(() => WorkItemsText.Slice(Report([]), (WorkAudience)7));
    }

    [Fact]
    public void AllIsTheBoardsOwnStatsAndAPublishedSliceWins()
    {
        var bot = WorkItemsStats.Empty with { Rework = new WorkRework(1, null, null) };
        var report = Report([Item(1, worker: WorkWorker.Team)], bot: bot);

        Assert.Same(report.Stats, WorkItemsText.Slice(report, WorkAudience.All));
        Assert.Same(bot, WorkItemsText.Slice(report, WorkAudience.Bot));
    }

    // ---- Comparison ----

    [Fact]
    public void NoSplitAndNoWorkerIsNoComparison() =>
        Assert.Null(WorkItemsText.Comparison(Report([Item(1), Item(2)])));

    [Fact]
    public void ASplitWithNoCycleTimeAndNoWeekIsNoComparison() =>
        Assert.Null(WorkItemsText.Comparison(Report([Item(1, worker: WorkWorker.Bot)])));

    [Fact]
    public void APublishedCycleTimeOnOneSideIsEnoughForAComparison()
    {
        var hours = new WorkDuration(5, 8, 2);
        var comparison = WorkItemsText.Comparison(Report([], bot: WorkItemsStats.Empty with { CycleTimeHours = hours }));

        Assert.NotNull(comparison);
        Assert.Equal(hours, comparison.BotCycle);
        Assert.Null(comparison.TeamCycle);
        Assert.Null(comparison.BotWeeks);
        Assert.Null(comparison.TeamWeeks);
    }

    // ---- Active ----

    [Fact]
    public void NoItemsAreNoGroups() => Assert.Empty(WorkItemsText.Active([]));

    [Fact]
    public void OnlyDoneItemsAreNoActiveGroup() =>
        Assert.Empty(WorkItemsText.Active([Item(1, WorkColumns.Done), Item(2, "done")]));

    [Fact]
    public void ActiveRefusesNothing() =>
        Assert.Equal("items", Assert.Throws<ArgumentNullException>(() => WorkItemsText.Active(null!)).ParamName);

    // ---- RecentlyDone ----

    [Fact]
    public void NoItemsAreNothingDone() => Assert.Empty(WorkItemsText.RecentlyDone([]));

    [Fact]
    public void WithoutArrivalTimesTheHigherNumberIsFirstAndNoNumberIsLast()
    {
        IReadOnlyList<WorkItem> items = [Item(null, WorkColumns.Done), Item(3, WorkColumns.Done), Item(9, WorkColumns.Done), Item(5, WorkColumns.Todo)];

        Assert.Equal([9, 3, null], WorkItemsText.RecentlyDone(items).Select(item => item.Identity.Number));
    }

    [Fact]
    public void AnArrivalTimeComesBeforeNone()
    {
        IReadOnlyList<WorkItem> items =
        [
            Item(50, WorkColumns.Done),
            Item(1, WorkColumns.Done, [new ColumnVisit(WorkColumns.Done, ThreeHoursAgo)]),
            Item(2, WorkColumns.Done, [new ColumnVisit(WorkColumns.Done, Now)]),
        ];

        Assert.Equal([2, 1, 50], WorkItemsText.RecentlyDone(items).Select(item => item.Identity.Number));
    }

    [Fact]
    public void RecentlyDoneRefusesNothing() =>
        Assert.Equal("items", Assert.Throws<ArgumentNullException>(() => WorkItemsText.RecentlyDone(null!)).ParamName);

    // ---- Since ----

    [Fact]
    public void AnItemInNoColumnHasNoSince() =>
        Assert.Null(WorkItemsText.Since(Item(1, null, [new ColumnVisit(WorkColumns.Todo, ThreeHoursAgo)]), Now));

    [Fact]
    public void AnItemWhoseHistoryNeverSaysWhenItArrivedHasNoSince() =>
        Assert.Null(WorkItemsText.Since(Item(1, WorkColumns.Review, [new ColumnVisit(WorkColumns.Todo, ThreeHoursAgo)]), Now));

    [Fact]
    public void SinceSaysHowLongAgoTheItemEnteredItsColumn() =>
        Assert.Equal("since 3 h ago", WorkItemsText.Since(Item(1, WorkColumns.Review, [new ColumnVisit(WorkColumns.Review, ThreeHoursAgo)]), Now));

    // ---- Duration ----

    [Fact]
    public void ADurationThatSaysNothingIsNotMetered() =>
        Assert.Equal("n/a", WorkItemsText.Duration(new WorkDuration(null, null, null)));

    [Theory]
    [InlineData(0, "0 items")]
    [InlineData(1, "1 item")]
    [InlineData(2, "2 items")]
    public void ACountAloneIsSaidAsACount(int count, string text) =>
        Assert.Equal(text, WorkItemsText.Duration(new WorkDuration(null, null, count)));

    [Fact]
    public void AMedianAndACountLeaveTheP85OutOfTheLine() =>
        Assert.Equal("median 18.5 h · 11 items", WorkItemsText.Duration(new WorkDuration(18.5, null, 11)));

    // ---- Rework ----

    [Fact]
    public void AReworkThatSaysNothingIsNotMetered() =>
        Assert.Equal("n/a", WorkItemsText.Rework(new WorkRework(null, null, null)));

    [Fact]
    public void OneOfEachIsSingularAndNoneIsPlural()
    {
        Assert.Equal("1 functional-test failure · 1 revert", WorkItemsText.Rework(new WorkRework(1, 1, null)));
        Assert.Equal("0 functional-test failures · 0 reverts", WorkItemsText.Rework(new WorkRework(0, 0, null)));
    }

    [Fact]
    public void AFailureAndTwoRevertsKeepTheirSingularsAndPluralsApart() =>
        Assert.Equal("1 functional-test failure · 2 reverts", WorkItemsText.Rework(new WorkRework(1, 2, null)));

    [Fact]
    public void TwoFailuresAndARevertKeepTheirSingularsAndPluralsApart() =>
        Assert.Equal("2 functional-test failures · 1 revert", WorkItemsText.Rework(new WorkRework(2, 1, null)));

    // ---- HasMeteredCost ----

    [Fact]
    public void NoItemsHaveNoMeteredCost() => Assert.False(WorkItemsText.HasMeteredCost([]));

    [Fact]
    public void AZeroAmountIsStillMetered() => Assert.True(WorkItemsText.HasMeteredCost([Item(1), Item(2, costUsd: 0)]));

    [Fact]
    public void HasMeteredCostRefusesNothing() =>
        Assert.Equal("items", Assert.Throws<ArgumentNullException>(() => WorkItemsText.HasMeteredCost(null!)).ParamName);

    // ---- Cost ----

    [Fact]
    public void WithoutAPublishedTotalTheMeteredAmountsAreSummedAndDividedByEveryItem() =>
        Assert.Equal("$2.00 per item · $6.00 total", WorkItemsText.Cost(null, [Item(1, costUsd: 4), Item(2, costUsd: 2), Item(3)]));

    [Fact]
    public void ANoteWithoutAPublishedTotalIsNotShown() =>
        Assert.Equal("$3.00 per item · $3.00 total", WorkItemsText.Cost(new WorkCost(null, "Cursor agent usage"), [Item(1, costUsd: 3)]));

    [Fact]
    public void APublishedTotalWinsOverTheItems() =>
        Assert.Equal("$5.00 per item · $10.00 total", WorkItemsText.Cost(new WorkCost(10, null), [Item(1, costUsd: 1), Item(2)]));

    [Fact]
    public void CostRefusesNothing() =>
        Assert.Equal("items", Assert.Throws<ArgumentNullException>(() => WorkItemsText.Cost(null, null!)).ParamName);

    // ---- ItemCost ----

    [Theory]
    [InlineData(0d, "$0.00")]
    [InlineData(18d, "$18.00")]
    public void AnItemsCostIsTheAmountInDollars(double cost, string text) =>
        Assert.Equal(text, WorkItemsText.ItemCost(cost));

    [Fact]
    public void AnUnmeteredItemIsNotMetered() => Assert.Equal("n/a", WorkItemsText.ItemCost(null));

    // ---- ColumnId ----

    [Theory]
    [InlineData("Todo", "work-col-todo")]
    [InlineData("  Code  Review! ", "work-col-code-review")]
    [InlineData("TDD", "work-col-tdd")]
    [InlineData("a--b", "work-col-a-b")]
    [InlineData("Stage 2", "work-col-stage-2")]
    [InlineData("!!!", "work-col-")]
    public void AColumnIdIsTheNameInLowerCaseWordsJoinedByOneDash(string column, string id) =>
        Assert.Equal(id, WorkItemsText.ColumnId(column));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ABlankColumnHasNoId(string column) =>
        Assert.Throws<ArgumentException>(() => WorkItemsText.ColumnId(column));

    [Fact]
    public void NoColumnHasNoId() =>
        Assert.Throws<ArgumentNullException>(() => WorkItemsText.ColumnId(null!));

    // ---- Number ----

    [Theory]
    [InlineData(0, "#0")]
    [InlineData(-1, "#-1")]
    [InlineData(int.MaxValue, "#2147483647")]
    public void ANumberIsHashThenDigits(int number, string text) =>
        Assert.Equal(text, WorkItemsText.Number(Item(number)));

    // ---- Agent and Assignee ----

    [Fact]
    public void WithoutABotTheAgentIsTheAssignee() =>
        Assert.Equal("someone", WorkItemsText.Agent(Item(1, assignee: "someone")));

    [Fact]
    public void TheBotWinsOverTheAssignee() =>
        Assert.Equal("dashboard-bot", WorkItemsText.Agent(Item(1, assignee: "someone", agent: "dashboard-bot")));

    [Theory]
    [InlineData(null, "dashboard-bot", null)]
    [InlineData("dashboard-bot", "dashboard-bot", null)]
    [InlineData("Dashboard-Bot", "dashboard-bot", "Dashboard-Bot")]
    [InlineData("someone", null, "someone")]
    [InlineData("someone", "dashboard-bot", "someone")]
    public void TheAssigneeIsShownOnlyWhenItIsNotTheBot(string? assignee, string? agent, string? shown) =>
        Assert.Equal(shown, WorkItemsText.Assignee(Item(1, assignee: assignee, agent: agent)));

    // ---- SessionLabel and SessionTitle ----

    [Theory]
    [InlineData(1, 1, "Session")]
    [InlineData(1, 2, "Session 1")]
    [InlineData(0, 0, "Session 0")]
    [InlineData(12, 12, "Session 12")]
    public void OneSessionIsJustASessionAndAnyOtherCountIsNumbered(int index, int count, string label) =>
        Assert.Equal(label, WorkItemsText.SessionLabel(index, count));

    [Fact]
    public void AZeroCountStillNumbersTheSessionTitle() =>
        Assert.Equal("Cursor session 0 (opens in a new tab)", WorkItemsText.SessionTitle(0, 0));

    // ---- Places and PlaceLabel ----

    [Fact]
    public void AnItemDeployedNowhereHasNoPlace() => Assert.Empty(WorkItemsText.Places(Item(1)));

    [Fact]
    public void OnlyTheEnvironmentsReachedArePlaces()
    {
        var item = Item(1, environments: new WorkItemEnvironments(null, null, new WorkDeployment("2.4.86", ThreeHoursAgo)));

        var place = Assert.Single(WorkItemsText.Places(item));
        Assert.Equal(new WorkPlace("prod", "2.4.86", ThreeHoursAgo), place);
    }

    [Fact]
    public void PlacesRefusesNothing() =>
        Assert.Equal("item", Assert.Throws<ArgumentNullException>(() => WorkItemsText.Places(null!)).ParamName);

    [Theory]
    [InlineData("uat", "2.4.86", "uat 2.4.86")]
    [InlineData("uat", null, "uat")]
    [InlineData("prod", "", "prod ")]
    public void APlaceIsItsNameAndTheVersionWhenThereIsOne(string name, string? version, string label) =>
        Assert.Equal(label, WorkItemsText.PlaceLabel(new WorkPlace(name, version, null)));
}
