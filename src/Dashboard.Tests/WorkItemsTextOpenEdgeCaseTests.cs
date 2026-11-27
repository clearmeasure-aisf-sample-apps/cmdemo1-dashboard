using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="WorkItemsText" /> that its four sibling files leave
/// open. The file the issue names, <see cref="WorkItemsTextEdgeCaseTests" />, was written for an earlier pass at this
/// same class (and joined by <see cref="WorkItemsTextBoundaryTests" />,
/// <see cref="WorkItemsTextRemainingEdgeCaseTests" /> and <see cref="WorkItemsTextTests" />), so this pass lands
/// beside it rather than over it: nothing below repeats a row those four already pin.
/// <para>
/// What is left, and is pinned here: the whole declared surface of the class - its eight fixed words and its forty
/// members - so that a member added without a sentence is noticed; the team's half of <see cref="WorkItemsText.Slice" />,
/// which was only ever asked of the bot; <see cref="WorkItemsText.Entered" /> asked with spaces around the column,
/// blank, or nothing at all; <see cref="WorkItemsText.Since" /> for an item that arrived after the page's clock and
/// for one standing in a blank column; the words that only a number at the bottom of an <see cref="int" /> or below
/// zero brings out of <see cref="WorkItemsText.Number" />, <see cref="WorkItemsText.PullLabel" />,
/// <see cref="WorkItemsText.Duration" /> and <see cref="WorkItemsText.Rework" />; the two items that reached Done at
/// one moment and the group that holds the very instances it was given; <see cref="WorkItemsText.Chart" />
/// labelling the day the file named rather than the Monday it belongs to, and scaling a week whose counts are all
/// below zero; the half-cent of <see cref="WorkItemsText.BarHeight" /> that falls on a midpoint; a cost word that is
/// no word, a who-word that is no word, and an amount below zero; a repo word of spaces; and the refusals that name
/// the argument and the sentence they refused. Last, the words of a quantity read the same to a reader of German as to
/// one of English - no number on this class waits for the reader's culture.
/// </para>
/// <para>
/// Fixed inputs throughout: no network, and no clock asked what time it is - every moment is one the test brings and
/// is handed to the member that needs it. The members that read a live address or a real wall clock are not here.
/// </para>
/// </summary>
public class WorkItemsTextOpenEdgeCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 22, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ThreeHoursAgo = new(2026, 11, 9, 19, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Monday = new(2026, 10, 26, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Sunday = new(2026, 11, 8, 0, 0, 0, TimeSpan.Zero);

    /// <summary>An item that was worked through: opened in Todo on Sunday, taken on Sunday afternoon, done on Monday.</summary>
    private static readonly IReadOnlyList<ColumnVisit> WorkedThrough =
    [
        new ColumnVisit(WorkColumns.Todo, new DateTimeOffset(2026, 11, 8, 0, 0, 0, TimeSpan.Zero)),
        new ColumnVisit(WorkColumns.InProgress, new DateTimeOffset(2026, 11, 8, 12, 0, 0, TimeSpan.Zero)),
        new ColumnVisit(WorkColumns.Done, new DateTimeOffset(2026, 11, 9, 0, 0, 0, TimeSpan.Zero)),
    ];

    private static WorkItem Item(
        int? number = null,
        string? column = null,
        IReadOnlyList<ColumnVisit>? history = null,
        string? assignee = null,
        string? agent = null,
        string? state = null,
        double? costUsd = null,
        WorkWorker worker = WorkWorker.Unknown) =>
        new(new WorkItemIdentity(number, "An item", null, state, column), history ?? [], assignee, agent, [], [], null, WorkItemEnvironments.None, costUsd, worker);

    private static WorkItemsReport Report(IReadOnlyList<WorkItem> items, WorkItemsStats? bot = null, WorkItemsStats? team = null) =>
        new(null, null, WorkItemsStats.Empty, items, [], bot, team);

    /// <summary><c>stats.wipByColumn</c> as a caller hands it: the column's own spelling, matched however it is cased.</summary>
    private static WorkItemsStats Counts(params (string Column, int Count)[] given) =>
        new(given.ToDictionary(pair => pair.Column, pair => pair.Count, StringComparer.OrdinalIgnoreCase), null, null, [], null, null);

    // ---- the surface: every fixed word, and every member the class has ----

    /// <summary>
    /// The whole of what <see cref="WorkItemsText" /> declares of itself: nine fixed words (eight of them the
    /// sentence, the mark a cell shows, or the format a bar coordinate is written with, and one of them the height of
    /// the chart), and exactly forty members - the thirty-nine a caller can ask, <see cref="WorkItemsText.Wip(WorkItemsStats, IReadOnlyList{WorkItem})" /> asked
    /// of a report and of a strip being the one name with two doors. Nothing of its own is held by an instance,
    /// because there is no instance: the class is the static one the compiler seals. A member added without a
    /// sentence, or a word renamed, has to be written here as well - which is the point.
    /// </summary>
    [Fact]
    public void TheClassDeclaresTheseWordsAndExactlyTheseMembers()
    {
        var type = typeof(WorkItemsText);

        Assert.True(type.IsAbstract && type.IsSealed);
        Assert.Empty(type.GetConstructors());
        Assert.Empty(type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Empty(type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Empty(type.GetEvents(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        Assert.Equal(
            ["Loading", "NoAddress", "NotPublished", "None", "NoActive", "ChartHeight", "PixelFormat", "NotMetered", "Missing"],
            fields.Select(field => field.Name));
        Assert.All(fields, field =>
        {
            Assert.True(field.IsPublic);
            Assert.True(field.IsStatic);
            Assert.True(field.IsLiteral);
            Assert.Equal(field.Name == "ChartHeight" ? typeof(double) : typeof(string), field.FieldType);
        });

        var methods = type
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .ToList();

        Assert.Equal(42, methods.Count);
        Assert.All(methods, method => Assert.True(method.IsStatic));
        Assert.Equal(
            [
                "Active", "Agent", "Assignee", "AudienceHash", "AudienceLabel", "AudienceOf", "Availability", "Availability",
                "BarHeight", "CandidateTitle", "Carries", "Chart", "ColumnId", "Comparison", "Cost", "Duration",
                "Entered", "HasMeteredCost", "IsMerged", "IssueTitle", "ItemCost", "LatestWeek", "NoneOf", "Number",
                "PlaceLabel", "PlaceTitle", "Places", "PullLabel", "PullSummary", "PullTitle", "RecentlyDone", "Rework", "Scope",
                "Select", "SessionLabel", "SessionTitle", "Since", "Slice", "Unavailable", "Wip", "Wip", "WorkerLabel",
            ],
            methods.Select(method => method.Name).Order(StringComparer.Ordinal));
    }

    // ---- Slice: the team's half, which was only ever asked of the bot ----

    [Fact]
    public void APublishedTeamSliceWinsAndAllNeverAsksTheItems()
    {
        var team = WorkItemsStats.Empty with { CycleTimeHours = new WorkDuration(5, 8, 2) };
        var report = Report([Item(2, worker: WorkWorker.Team)], team: team);

        Assert.Same(team, WorkItemsText.Slice(report, WorkAudience.Team));
        Assert.Same(report.Stats, WorkItemsText.Slice(report, WorkAudience.All));
        Assert.NotSame(team, WorkItemsText.Slice(report, WorkAudience.All));
    }

    [Fact]
    public void ATeamSliceTheFileDoesNotPublishIsCountedFromTheTeamsOwnItems()
    {
        var report = Report([Item(2, WorkColumns.Done, WorkedThrough, worker: WorkWorker.Team), Item(9, WorkColumns.Todo, worker: WorkWorker.Bot)]);

        var slice = WorkItemsText.Slice(report, WorkAudience.Team);

        Assert.Null(report.TeamStats);
        Assert.Equal(new WorkDuration(12, 12, 1), slice.CycleTimeHours);
        Assert.Equal(new WorkDuration(24, 24, 1), slice.LeadTimeHours);
        Assert.Equal("median 12 h · p85 12 h · 1 item", WorkItemsText.Duration(slice.CycleTimeHours));
    }

    [Fact]
    public void AFilterWithNoItemsOfItsOwnHasTheEmptySliceAndNoInventedOne()
    {
        var report = Report([Item(1, WorkColumns.Todo, worker: WorkWorker.Bot)]);

        var slice = WorkItemsText.Slice(report, WorkAudience.Team);

        Assert.Same(WorkItemsStats.Empty, slice);
        Assert.Null(slice.CycleTimeHours);
        Assert.Equal(WorkItemsText.NotMetered, WorkItemsText.Duration(slice.CycleTimeHours));
    }

    [Fact]
    public void ASideWithNoItemsOfItsOwnIsComparedAgainstNothing()
    {
        var comparison = WorkItemsText.Comparison(Report([Item(2, WorkColumns.Done, WorkedThrough, worker: WorkWorker.Team)]))!;

        Assert.Null(comparison.BotCycle);
        Assert.Equal(new WorkDuration(12, 12, 1), comparison.TeamCycle);
        Assert.Null(comparison.BotWeeks);
        Assert.Null(comparison.TeamWeeks);
        Assert.Equal("n/a", WorkItemsText.Duration(comparison.BotCycle));
    }

    // ---- Entered and Since: the column asked is the column spelled ----

    [Fact]
    public void AColumnAskedWithSpacesOrPunctuationAroundItIsNotTheColumnTheHistoryNames()
    {
        var item = Item(1, WorkColumns.Review, [new ColumnVisit(WorkColumns.Review, ThreeHoursAgo)]);

        Assert.Equal(ThreeHoursAgo, WorkItemsText.Entered(item, WorkColumns.Review));
        Assert.Null(WorkItemsText.Entered(item, " Review "));
        Assert.Null(WorkItemsText.Entered(item, "Review."));
        Assert.Null(WorkItemsText.Entered(item, string.Empty));
        Assert.Null(WorkItemsText.Since(Item(1, " Review ", [new ColumnVisit(WorkColumns.Review, ThreeHoursAgo)]), Now));
    }

    [Fact]
    public void AVisitThatNamesNoColumnAnswersOnlyAnAskThatNamesNoColumn()
    {
        var nameless = Item(1, null, [new ColumnVisit(null, ThreeHoursAgo)]);

        Assert.Equal(ThreeHoursAgo, WorkItemsText.Entered(nameless, null!));
        Assert.Null(WorkItemsText.Entered(nameless, string.Empty));
        Assert.Null(WorkItemsText.Entered(Item(1, null, [new ColumnVisit(WorkColumns.Todo, ThreeHoursAgo)]), null!));
    }

    [Fact]
    public void AnItemThatArrivedAfterTheClockWasAskedHasArrivedJustNow() =>
        Assert.Equal(
            "since just now",
            WorkItemsText.Since(Item(1, WorkColumns.Review, [new ColumnVisit(WorkColumns.Review, Now + TimeSpan.FromHours(3))]), Now));

    [Fact]
    public void ABlankColumnIsAColumnWhenTheHistoryBlanksItToo() =>
        Assert.Equal("since 3 h ago", WorkItemsText.Since(Item(1, string.Empty, [new ColumnVisit(string.Empty, ThreeHoursAgo)]), Now));

    // ---- Active and RecentlyDone: the grouping the file's order decides ----

    [Fact]
    public void AColumnSpelledWithSpacesAroundItIsNotTheBoardsColumnAndIsHeldByTheVeryItems()
    {
        IReadOnlyList<WorkItem> items = [Item(1, " Todo "), Item(2, "TODO"), Item(3, WorkColumns.Unplaced), Item(4, WorkColumns.Done)];

        var groups = WorkItemsText.Active(items);

        Assert.Equal([WorkColumns.Todo, WorkColumns.Unplaced], groups.Select(group => group.Column));
        Assert.Equal([2], groups[0].Items.Select(item => item.Identity.Number));
        Assert.Equal([1, 3], groups[1].Items.Select(item => item.Identity.Number));
        Assert.Same(items[0], groups[1].Items[0]);
        Assert.Same(items[2], groups[1].Items[1]);
    }

    [Fact]
    public void TwoItemsThatReachedDoneTogetherAreSaidTheHigherNumberFirst()
    {
        IReadOnlyList<WorkItem> items =
        [
            Item(1, WorkColumns.Done, [new ColumnVisit(WorkColumns.Done, ThreeHoursAgo)]),
            Item(20, WorkColumns.Done, [new ColumnVisit(WorkColumns.Done, ThreeHoursAgo)]),
            Item(3, WorkColumns.Done, [new ColumnVisit(WorkColumns.Done, Now)]),
        ];

        Assert.Equal([3, 20, 1], WorkItemsText.RecentlyDone(items).Select(item => item.Identity.Number));
    }

    // ---- Wip: a count the file gives below zero ----

    [Fact]
    public void ACountBelowZeroIsShownAsTheFileGaveItAndLeavesNoUnplacedRow()
    {
        var rows = WorkItemsText.Wip(Counts(("Todo", -3), ("Blocked", -2)), [Item(1, WorkColumns.Todo), Item(2, "Blocked")]);

        Assert.Equal(WorkColumns.Active, rows.Select(row => row.Column));
        Assert.Equal([-3, 0, 0, 0, 0, 0], rows.Select(row => row.Count));
        Assert.DoesNotContain(rows, row => row.Column == WorkColumns.Unplaced);
    }

    // ---- Chart: the day the file named, and counts that are not ones a bar can hold ----

    [Fact]
    public void AWeekIsNamedForTheDayTheFileGaveInUtcHoweverTheWeekFalls()
    {
        var wednesday = WorkItemsText.Chart([new WorkWeek(new DateTimeOffset(2026, 11, 4, 0, 0, 0, TimeSpan.Zero), 1, 1)])!;
        var eastOfUtc = WorkItemsText.Chart([new WorkWeek(new DateTimeOffset(2026, 11, 9, 0, 30, 0, TimeSpan.FromHours(2)), 1, 1)])!;
        var first = WorkItemsText.Chart([new WorkWeek(DateTimeOffset.MinValue, 1, 1)])!;

        Assert.Equal("Week of 2026-11-04", wednesday.Bars[0].Label);
        Assert.Equal("Week of 2026-11-08", eastOfUtc.Bars[0].Label);
        Assert.Equal("Week of 0001-01-01", first.Bars[0].Label);
    }

    [Fact]
    public void AWeekOfNothingToDrawStillHasAScaleAndTheLargestCountIsTheScaleItself()
    {
        var below = WorkItemsText.Chart([new WorkWeek(Monday, -5, -2)])!;

        Assert.Equal(1, below.Scale);
        Assert.Equal([-5, -2], [below.Bars[0].Merged, below.Bars[0].DeployedProd]);
        Assert.Equal("Week of 2026-10-26: -5 merged, -2 deployed to prod", WorkItemsText.LatestWeek(below));
        Assert.Equal(0d, WorkItemsText.BarHeight(below.Bars[0].Merged, below.Scale));

        var top = WorkItemsText.Chart([new WorkWeek(Sunday, 0, int.MaxValue)])!;

        Assert.Equal(int.MaxValue, top.Scale);
        Assert.Equal("Week of 2026-11-08: 0 merged, 2147483647 deployed to prod", WorkItemsText.LatestWeek(top));
    }

    [Theory]
    [InlineData(1, 32, 1.12d)]
    [InlineData(3, 32, 3.38d)]
    [InlineData(5, 32, 5.62d)]
    [InlineData(7, 32, 7.88d)]
    public void AShareThatLandsExactlyOnHalfAHundredthIsRoundedToTheEvenSide(int value, int scale, double height) =>
        Assert.Equal(height, WorkItemsText.BarHeight(value, scale));

    [Theory]
    [InlineData(int.MinValue, 1)]
    [InlineData(1, int.MinValue)]
    [InlineData(int.MinValue, int.MinValue)]
    public void TheBottomOfAnIntIsNeitherABarNorAScale(int value, int scale) =>
        Assert.Equal(0d, WorkItemsText.BarHeight(value, scale));

    // ---- The counts and the amounts a file can get wrong ----

    [Fact]
    public void OnlyACountOfOneIsSingularAndACountBelowZeroKeepsItsMark()
    {
        Assert.Equal("-1 functional-test failures · -3 reverts", WorkItemsText.Rework(new WorkRework(-1, -3, null)));
        Assert.Equal("-1 reverts", WorkItemsText.Rework(new WorkRework(null, -1, null)));
    }

    [Theory]
    [InlineData(1000, "1000 items")]
    [InlineData(int.MaxValue, "2147483647 items")]
    [InlineData(int.MinValue, "-2147483648 items")]
    public void ACountIsSaidDigitByDigitWithNoSeparator(int count, string text) =>
        Assert.Equal(text, WorkItemsText.Duration(new WorkDuration(null, null, count)));

    [Theory]
    [InlineData(-4d, "-$4.00")]
    [InlineData(1234.567d, "$1,234.57")]
    [InlineData(-0.001d, "$0.00")]
    public void AnItemAmountIsSaidToTheCentAndWithItsMark(double cost, string text) =>
        Assert.Equal(text, WorkItemsText.ItemCost(cost));

    [Fact]
    public void AnAmountBelowZeroIsMeteredAndTurnsTheCostLineOn()
    {
        Assert.True(WorkItemsText.HasMeteredCost([Item(1, costUsd: -6)]));
        Assert.Equal("-$3.00 per item · -$6.00 total", WorkItemsText.Cost(new WorkCost(-6, null), [Item(1, costUsd: -2), Item(2)]));
    }

    [Fact]
    public void ANoteThatIsNoWordIsStillANoteAndLeavesTheDotItFollows()
    {
        Assert.Equal("$6.00 per item · $6.00 total · ", WorkItemsText.Cost(new WorkCost(6, string.Empty), [Item(1, costUsd: 6)]));
        Assert.Equal("$6.00 per item · $6.00 total", WorkItemsText.Cost(new WorkCost(6, null), [Item(1, costUsd: 6)]));
    }

    // ---- The numbers a word is made of, at the bottom of an int ----

    [Fact]
    public void ANumberAtTheBottomOfAnIntIsSaidInFull()
    {
        Assert.Equal("#-2147483648", WorkItemsText.Number(Item(int.MinValue)));
        Assert.Equal("#-2147483648", WorkItemsText.PullLabel(new WorkPullRequest(int.MinValue, null, null, null, null)));
    }

    [Fact]
    public void AStateThatIsNoWordIsStillAStateAndTakesTheComma() =>
        Assert.Equal("Work item #490,  (opens in a new tab)", WorkItemsText.IssueTitle(Item(490, state: string.Empty)));

    [Fact]
    public void OnlyNothingAboutWhoWorksTheItemIsShownAsMissing()
    {
        // An empty word is a word the file wrote, so it is shown as it was written; only nothing at all falls
        // through to the em dash, which WorkItemsTests already pins for an item with neither part.
        Assert.Equal(string.Empty, WorkItemsText.Agent(Item(1, agent: string.Empty)));
        Assert.Equal(string.Empty, WorkItemsText.Agent(Item(1, assignee: string.Empty)));
        Assert.Equal(string.Empty, WorkItemsText.Assignee(Item(1, assignee: string.Empty)));
        Assert.Null(WorkItemsText.Assignee(Item(1, assignee: string.Empty, agent: string.Empty)));
    }

    // ---- Scope: a repo word that is spaces, and an address in another person's capitals ----

    [Fact]
    public void ARepoWordThatIsNothingButSpacesIsStillALineAndSaysTheSpaces()
    {
        var line = WorkItemsText.Scope(new WorkItemsScope("   ", null, null));

        Assert.Equal("   ", line!.Label);
        Assert.Null(line.Url);
        Assert.Null(line.Project);
        Assert.Null(line.Title);
    }

    [Fact]
    public void AnAddressIsLabelledByItsPathInItsOwnLettersAndAnEmptyPathByItsHost()
    {
        var path = WorkItemsText.Scope(new WorkItemsScope("HTTPS://GitHub.com/Org/Repo", null, null))!;

        Assert.Equal("Org/Repo", path.Label);
        Assert.Equal("github.com", path.Url!.Host);
        Assert.Null(path.Project);
        Assert.Equal("Org/Repo (opens in a new tab)", path.Title);

        var empty = WorkItemsText.Scope(new WorkItemsScope("https://github.com///", null, null))!;

        Assert.Equal("github.com", empty.Label);
        Assert.Equal(new Uri("https://github.com///"), empty.Url);
        Assert.Null(empty.Project);
    }

    // ---- The refusals that name the argument and say why ----

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    [InlineData(int.MaxValue)]
    public void AFilterThatIsNoFilterIsRefusedByItsOwnNameAndWithItsOwnReason(int number)
    {
        var audience = (WorkAudience)number;
        var report = Report([]);

        Assert.Equal("audience", Assert.Throws<ArgumentOutOfRangeException>(() => WorkItemsText.AudienceLabel(audience)).ParamName);
        Assert.Contains("A work filter has no name.", Assert.Throws<ArgumentOutOfRangeException>(() => WorkItemsText.AudienceLabel(audience)).Message, StringComparison.Ordinal);

        Assert.Equal("audience", Assert.Throws<ArgumentOutOfRangeException>(() => WorkItemsText.AudienceHash(audience)).ParamName);
        Assert.Contains("A work filter has no address.", Assert.Throws<ArgumentOutOfRangeException>(() => WorkItemsText.AudienceHash(audience)).Message, StringComparison.Ordinal);

        Assert.Equal("audience", Assert.Throws<ArgumentOutOfRangeException>(() => WorkItemsText.NoneOf(audience)).ParamName);
        Assert.Contains("A work filter has no empty state.", Assert.Throws<ArgumentOutOfRangeException>(() => WorkItemsText.NoneOf(audience)).Message, StringComparison.Ordinal);

        Assert.Equal("audience", Assert.Throws<ArgumentOutOfRangeException>(() => WorkItemsText.Select([Item(1)], audience)).ParamName);
        Assert.Contains("A work filter has no items.", Assert.Throws<ArgumentOutOfRangeException>(() => WorkItemsText.Select([Item(1)], audience)).Message, StringComparison.Ordinal);

        Assert.Equal("audience", Assert.Throws<ArgumentOutOfRangeException>(() => WorkItemsText.Slice(report, audience)).ParamName);
        Assert.Contains("A work filter has no stats.", Assert.Throws<ArgumentOutOfRangeException>(() => WorkItemsText.Slice(report, audience)).Message, StringComparison.Ordinal);

        var worker = (WorkWorker)number;
        var refused = Assert.Throws<ArgumentOutOfRangeException>(() => WorkItemsText.WorkerLabel(worker));
        Assert.Equal("worker", refused.ParamName);
        Assert.Equal<object>(worker, refused.ActualValue!);
        Assert.Contains("A worker has no name.", refused.Message, StringComparison.Ordinal);
    }

    // ---- the words that follow no reader's culture ----

    [Fact]
    public void TheWordsOfAQuantityAreTheSameWordsToAReaderOfGerman()
    {
        var english = Said();

        Assert.Equal(
            [
                "median 18.5 h · p85 3 d · 11 items",
                "2 functional-test failures · 1 revert · 12.5 %",
                "$1,234.57",
                "$5.00 per item · $10.00 total",
                "Week of 2026-11-08: 3 merged, 4 deployed to prod",
                "#-1234",
                "Cursor session 2 (opens in a new tab)",
                "Session 2",
            ],
            english);

        ViewerCulture.German(() => Assert.Equal(english, Said()));
        Assert.Equal(english, Said());

        static IReadOnlyList<string> Said() =>
        [
            WorkItemsText.Duration(new WorkDuration(18.5, 72, 11)),
            WorkItemsText.Rework(new WorkRework(2, 1, 0.125)),
            WorkItemsText.ItemCost(1234.567),
            WorkItemsText.Cost(new WorkCost(10, null), [Item(1, costUsd: 6), Item(2, costUsd: 4)])!,
            WorkItemsText.LatestWeek(WorkItemsText.Chart([new WorkWeek(Sunday, 3, 4)])!),
            WorkItemsText.Number(Item(-1234)),
            WorkItemsText.SessionTitle(2, 3),
            WorkItemsText.SessionLabel(2, 3),
        ];
    }
}
