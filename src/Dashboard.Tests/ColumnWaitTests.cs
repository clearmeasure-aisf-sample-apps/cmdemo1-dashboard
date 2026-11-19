namespace Dashboard.Tests;

/// <summary>
/// Where items wait in a column, counted by <see cref="WorkBotText.ColumnWaits(WorkItemsStats?, IReadOnlyList{WorkItem}, DateTimeOffset)"/>
/// and written by the view's "Time in column" tile: the file's <c>timePerColumnHours</c> wins, otherwise the items'
/// column history is timed. The wait the factory should look at first is the one the list marks
/// <see cref="ColumnWait.Longest"/>, and the words of a wait are its column and
/// <see cref="WorkItemsText.Duration(WorkDuration?)"/>.
/// </summary>
public class ColumnWaitTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 22, 0, 0, TimeSpan.Zero);

    private static DateTimeOffset At(int day, int hour = 0) => new(2026, 11, day, hour, 0, 0, TimeSpan.Zero);

    private static WorkItem Item(int number, string? column = null, IReadOnlyList<ColumnVisit>? history = null) =>
        new(number, $"Item {number}", null, null, column, history ?? [], null, null, [], [], null, WorkItemEnvironments.None, null);

    /// <summary>A finished stay of <paramref name="hours"/> hours in <paramref name="column"/>, starting at 08:00 on <paramref name="day"/>.</summary>
    private static ColumnVisit Visit(string column, int day, double hours) =>
        new(column, At(day, 8), At(day, 8).AddHours(hours), hours);

    /// <summary>Stays of 2, 6 and 10 hours in UAT and of 1 and 2 in Review, across five items.</summary>
    private static IReadOnlyList<WorkItem> ManyItems() =>
    [
        Item(1, WorkColumns.Done, [Visit(WorkColumns.Uat, 5, 2)]),
        Item(2, WorkColumns.Done, [Visit(WorkColumns.Uat, 6, 6)]),
        Item(3, WorkColumns.Done, [Visit(WorkColumns.Uat, 7, 10)]),
        Item(4, WorkColumns.Done, [Visit(WorkColumns.Review, 8, 1)]),
        Item(5, WorkColumns.Done, [Visit(WorkColumns.Review, 8, 2)]),
    ];

    private static WorkItemsStats WithColumnTimes(params (string Column, WorkDuration Duration)[] times) =>
        new(new Dictionary<string, int>(), null, null, [], null, null,
            times.ToDictionary(pair => pair.Column, pair => pair.Duration, StringComparer.OrdinalIgnoreCase));

    // ---- No items, no stays, no waits ----

    [Fact]
    public void NoItemsAreNoWaitsAndNoLongestWait()
    {
        var noWaits = WorkBotText.ColumnWaits(null, [], Now);

        Assert.Empty(noWaits);
        Assert.Empty(WorkBotText.ColumnWaits(WorkItemsStats.Empty, [], Now));
        Assert.Null(WorkBotText.Longest(noWaits));
    }

    [Fact]
    public void AnItemThatTimedNoStayAtAllIsNoWait()
    {
        var waits = WorkBotText.ColumnWaits(null,
        [
            Item(1, WorkColumns.Review),
            Item(2, WorkColumns.Todo, [new ColumnVisit(WorkColumns.Review, null)]),
            Item(3, null, [new ColumnVisit(null, At(1)), new ColumnVisit(null, At(2), null, 4.0)]),
        ], Now);

        Assert.Empty(waits);
    }

    [Fact]
    public void WaitsRefuseItemsThatAreNothing()
    {
        var noItems = Assert.Throws<ArgumentNullException>(() => WorkBotText.ColumnWaits(null, null!, Now));

        Assert.Equal("items", noItems.ParamName);
    }

    // ---- One item ----

    [Fact]
    public void OneItemIsOneWaitPerColumnItTimedLongestFirst()
    {
        var item = Item(1, WorkColumns.Review,
        [
            new ColumnVisit(WorkColumns.Todo, At(9, 8)),
            new ColumnVisit(WorkColumns.Review, At(9, 12)),
        ]);

        var waits = WorkBotText.ColumnWaits(null, [item], Now);

        Assert.Equal(
        [
            new ColumnWait(WorkColumns.Review, new WorkDuration(10, 10, 1), true),
            new ColumnWait(WorkColumns.Todo, new WorkDuration(4, 4, 1), false),
        ],
            waits);
        Assert.Equal(WorkColumns.Review, WorkBotText.Longest(waits)!.Column);
    }

    [Fact]
    public void TheWaitOfOneItemIsWrittenAsOneItem()
    {
        var item = Item(1, WorkColumns.Review, [new ColumnVisit(WorkColumns.Review, At(9, 12))]);

        Assert.Equal("median 10 h · p85 10 h · 1 item", WorkItemsText.Duration(WorkBotText.ColumnWaits(null, [item], Now).Single().Duration));
        Assert.Equal("Review · 10 h so far", WorkBotText.Stay(WorkBotText.Timeline(item, Now).Single()));
    }

    // ---- Many items ----

    [Fact]
    public void ManyItemsAreOneWaitPerColumnOfMedianAndP85LongestFirst()
    {
        var waits = WorkBotText.ColumnWaits(null, ManyItems(), Now);

        Assert.Equal(
        [
            new ColumnWait(WorkColumns.Uat, new WorkDuration(6, 10, 3), true),
            new ColumnWait(WorkColumns.Review, new WorkDuration(1, 2, 2), false),
        ],
            waits);
    }

    [Fact]
    public void TheWaitTextNamesTheColumnTheMedianAndHowManyItems()
    {
        var waits = WorkBotText.ColumnWaits(null, ManyItems(), Now);
        var longest = WorkBotText.Longest(waits)!;

        Assert.Equal("UAT", longest.Column);
        Assert.Equal("median 6 h · p85 10 h · 3 items", WorkItemsText.Duration(longest.Duration));
        Assert.Equal("Longest wait: UAT, median 6 h · p85 10 h · 3 items", $"Longest wait: {longest.Column}, {WorkItemsText.Duration(longest.Duration)}");
        Assert.Equal("Review · median 1 h · p85 2 h · 2 items", $"{waits[1].Column} · {WorkItemsText.Duration(waits[1].Duration)}");
    }

    [Fact]
    public void WaitsTiedOnTheirMedianAreAllMarkedLongestAndTheOneReturnedIsFirstByColumnName()
    {
        var waits = WorkBotText.ColumnWaits(null,
        [
            Item(1, WorkColumns.Done, [Visit(WorkColumns.Uat, 5, 4)]),
            Item(2, WorkColumns.Done, [Visit(WorkColumns.Tdd, 5, 4)]),
        ], Now);

        Assert.Equal(
        [
            new ColumnWait(WorkColumns.Tdd, new WorkDuration(4, 4, 1), true),
            new ColumnWait(WorkColumns.Uat, new WorkDuration(4, 4, 1), true),
        ],
            waits);
        Assert.Equal(WorkColumns.Tdd, WorkBotText.Longest(waits)!.Column);
    }

    // ---- The file's timePerColumnHours wins ----

    [Fact]
    public void TheFilesColumnTimesWinOverTheItemsOwnStays()
    {
        var item = Item(1, WorkColumns.Review, [new ColumnVisit(WorkColumns.Review, At(9, 8))]);
        var waits = WorkBotText.ColumnWaits(WithColumnTimes(("review", new WorkDuration(3, 5, 2))), [item], Now);

        Assert.Equal([new ColumnWait(WorkColumns.Review, new WorkDuration(3, 5, 2), true)], waits);
        Assert.Equal("Review · median 3 h · p85 5 h · 2 items", $"{waits[0].Column} · {WorkItemsText.Duration(waits[0].Duration)}");
    }

    [Fact]
    public void APublishedListWithNoColumnsFallsBackToTheItems()
    {
        var published = new WorkItemsStats(new Dictionary<string, int>(), null, null, [], null, null, new Dictionary<string, WorkDuration>());
        var waits = WorkBotText.ColumnWaits(published, [Item(1, WorkColumns.Review, [new ColumnVisit(WorkColumns.Review, At(9, 12))])], Now);

        Assert.Equal([new ColumnWait(WorkColumns.Review, new WorkDuration(10, 10, 1), true)], waits);
    }

    [Fact]
    public void APublishedColumnThatSaidNoMedianIsLastAndIsNoLongest()
    {
        var waits = WorkBotText.ColumnWaits(WithColumnTimes(("UAT", new WorkDuration(null, 5, 2)), ("Review", new WorkDuration(3, 5, 1))), [], Now);

        Assert.Equal(
        [
            new ColumnWait(WorkColumns.Review, new WorkDuration(3, 5, 1), true),
            new ColumnWait(WorkColumns.Uat, new WorkDuration(null, 5, 2), false),
        ],
            waits);
        Assert.Equal(WorkColumns.Review, WorkBotText.Longest(waits)!.Column);
        Assert.Equal("p85 5 h · 2 items", WorkItemsText.Duration(waits[1].Duration));
    }

    // ---- The longest wait ----

    [Fact]
    public void TheListMarksExactlyOneWaitLongestAndItIsTheOldest()
    {
        var waits = WorkBotText.ColumnWaits(null, ManyItems(), Now);

        Assert.Single(waits, wait => wait.Longest);
        Assert.Equal(waits[0], WorkBotText.Longest(waits));
    }

    [Fact]
    public void NoWaitsAreNoLongestWait() => Assert.Null(WorkBotText.Longest([]));

    [Fact]
    public void AListThatMarksNothingIsNoLongestWait() =>
        Assert.Null(WorkBotText.Longest([new ColumnWait(WorkColumns.Review, new WorkDuration(2, 2, 1), false)]));

    [Fact]
    public void WaitsThatAreNothingAreRefused() => Assert.Throws<ArgumentNullException>(() => WorkBotText.Longest(null!));

    // ---- Stays with no time in them ----

    [Fact]
    public void AnItemWithNoHistoryStaysInNoColumn()
    {
        var item = Item(1, WorkColumns.Review);

        Assert.Empty(WorkBotText.Timeline(item, Now));
        Assert.Empty(WorkBotText.ColumnWaits(null, [item], Now));
    }

    [Fact]
    public void AnItemThatIsNothingHasNoTimeline() => Assert.Throws<ArgumentNullException>(() => WorkBotText.Timeline(null!, Now));

    [Fact]
    public void AVisitThatSaysNeitherWhenNorHowLongIsTheColumnAlone()
    {
        var item = Item(1, WorkColumns.Review, [new ColumnVisit(WorkColumns.Review, null)]);
        var stay = WorkBotText.Timeline(item, Now).Single();

        Assert.Null(stay.Spent);
        Assert.False(stay.Open);
        Assert.Equal("Review", WorkBotText.Stay(stay));
        Assert.Empty(WorkBotText.ColumnWaits(null, [item], Now));
    }

    [Fact]
    public void TheLastStayIsOpenOnlyWhileTheItemIsStillInThatColumn()
    {
        var item = Item(1, WorkColumns.Review, [new ColumnVisit(WorkColumns.Todo, At(9, 8))]);
        var stay = WorkBotText.Timeline(item, Now).Single();

        Assert.False(stay.Open);
        Assert.Null(stay.Spent);
        Assert.Equal("Todo", WorkBotText.Stay(stay));
        Assert.Empty(WorkBotText.ColumnWaits(null, [item], Now));
    }

    [Fact]
    public void AStayThatStartedAfterItEndedIsNoWait()
    {
        var item = Item(1, WorkColumns.Review,
        [
            new ColumnVisit(WorkColumns.Todo, At(9, 12)),
            new ColumnVisit(WorkColumns.Review, At(9, 8)),
        ]);
        var stays = WorkBotText.Timeline(item, Now);

        Assert.Null(stays[0].Spent);
        Assert.Equal("Todo", WorkBotText.Stay(stays[0]));
        Assert.Equal([new ColumnWait(WorkColumns.Review, new WorkDuration(14, 14, 1), true)], WorkBotText.ColumnWaits(null, [item], Now));
    }

    [Fact]
    public void ABlankColumnIsUnplacedAndAColumnTheBoardDoesNotKnowKeepsItsName()
    {
        var item = Item(1, "Blocked",
        [
            new ColumnVisit("", At(1, 8), At(1, 13), 5.0),
            new ColumnVisit("Blocked", At(1, 13), null, 3.0),
        ]);

        var waits = WorkBotText.ColumnWaits(null, [item], Now);

        Assert.Equal(
        [
            new ColumnWait(WorkColumns.Unplaced, new WorkDuration(5, 5, 1), true),
            new ColumnWait("Blocked", new WorkDuration(3, 3, 1), false),
        ],
            waits);
        Assert.Equal(WorkColumns.Unplaced, WorkBotText.Longest(waits)!.Column);
    }
}
