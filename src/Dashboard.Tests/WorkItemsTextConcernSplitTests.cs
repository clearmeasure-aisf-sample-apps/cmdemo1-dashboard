using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="WorkItemsText" /> is decomposed by concern (#1473) while staying the work domain's one door for the
/// page, in the house style of one "Text" class per domain. Its pinned surface - eight fixed words and forty members,
/// held by <see cref="WorkItemsTextOpenEdgeCaseTests" /> - is unchanged; what moved is where each member's body
/// lives. These tests re-home the pins: each concern class declares exactly its own members, together with the
/// three the door keeps for itself they account for every one of the door's forty, and the door says what the
/// concern class says for the same question, refusals included.
/// </summary>
public class WorkItemsTextConcernSplitTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 22, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ThreeHoursAgo = new(2026, 11, 9, 19, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Sunday = new(2026, 11, 8, 0, 0, 0, TimeSpan.Zero);

    /// <summary>Each concern class and the names it declares, in ordinal order.</summary>
    private static readonly (Type Type, string[] Names)[] Concerns =
    [
        (typeof(WorkAudienceText), ["Comparison", "Hash", "Label", "NoneOf", "Of", "Select", "Slice", "WorkerLabel"]),
        (typeof(WorkBoardText), ["Active", "ColumnId", "Entered", "RecentlyDone", "Since", "Wip"]),
        (typeof(WorkMeasureText), ["Cost", "Duration", "HasMeteredCost", "ItemCost", "Rework"]),
        (typeof(WorkWeekText), ["BarHeight", "Chart", "LatestWeek"]),
        (typeof(WorkScopeText), ["Carries", "Scope"]),
        (
            typeof(WorkRowText),
            [
                "Agent", "Assignee", "CandidateTitle", "IsMerged", "IssueTitle", "Number", "PlaceLabel", "PlaceTitle",
                "Places", "PullLabel", "PullTitle", "SessionLabel", "SessionTitle",
            ]
        ),
    ];

    /// <summary>The door's name for a member whose concern class says it shorter.</summary>
    private static readonly Dictionary<string, string> DoorName = new(StringComparer.Ordinal)
    {
        ["Label"] = "AudienceLabel",
        ["Of"] = "AudienceOf",
        ["Hash"] = "AudienceHash",
    };

    private static WorkItem Item(
        int? number = null,
        string? column = null,
        IReadOnlyList<ColumnVisit>? history = null,
        string? assignee = null,
        string? agent = null,
        string? state = null,
        double? costUsd = null,
        WorkWorker worker = WorkWorker.Unknown,
        WorkItemEnvironments? environments = null) =>
        new(number, "An item", null, state, column, history ?? [], assignee, agent, [], [], null, environments ?? WorkItemEnvironments.None, costUsd, worker);

    private static List<MethodInfo> PublicStatics(Type type) =>
        [.. type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(method => !method.IsSpecialName)];

    // ---- the surface, re-homed ----

    [Fact]
    public void EachConcernClassIsStaticAndDeclaresExactlyItsOwnMembers()
    {
        foreach (var (type, names) in Concerns)
        {
            Assert.True(type.IsAbstract && type.IsSealed, $"{type.Name} is not static");
            Assert.Equal("Dashboard.Work", type.Namespace);
            Assert.Empty(type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
            Assert.Empty(type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
            Assert.Equal(names, PublicStatics(type).Select(method => method.Name).Order(StringComparer.Ordinal));
        }
    }

    [Fact]
    public void TheConcernClassesAndTheDoorsOwnThreeAccountForEveryOneOfTheDoorsFortyMembers()
    {
        var moved = Concerns
            .SelectMany(concern => concern.Names.Select(name => DoorName.GetValueOrDefault(name, name)))
            .ToList();
        IReadOnlyList<string> kept = ["Availability", "Unavailable", "Wip"];

        Assert.Equal(37, moved.Count);
        Assert.Equal(moved.Count, moved.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(
            PublicStatics(typeof(WorkItemsText)).Select(method => method.Name).Order(StringComparer.Ordinal),
            moved.Concat(kept).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void EveryMovedMemberKeepsTheDoorsParametersAndReturnType()
    {
        var door = PublicStatics(typeof(WorkItemsText));
        foreach (var (type, _) in Concerns)
        {
            foreach (var method in PublicStatics(type))
            {
                var parameters = method.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
                var match = door.SingleOrDefault(candidate =>
                    candidate.Name == DoorName.GetValueOrDefault(method.Name, method.Name)
                    && candidate.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual(parameters));

                Assert.True(match is not null, $"{type.Name}.{method.Name} has no door of the same shape on WorkItemsText");
                Assert.Equal(method.ReturnType, match!.ReturnType);
            }
        }
    }

    [Fact]
    public void ThePageAsksOnlyTheDoorAndNeverAConcernClass()
    {
        var view = MarkupGuards.ComponentText("WorkItemsView");

        Assert.Contains("WorkItemsText.", view, StringComparison.Ordinal);
        foreach (var (type, _) in Concerns)
        {
            Assert.DoesNotContain(type.Name + ".", view, StringComparison.Ordinal);
        }
    }

    // ---- the audience: WorkAudienceText ----

    [Theory]
    [InlineData(WorkAudience.All)]
    [InlineData(WorkAudience.Bot)]
    [InlineData(WorkAudience.Team)]
    public void TheAudienceWordsAreTheDoorsWords(WorkAudience audience)
    {
        IReadOnlyList<WorkItem> items = [Item(1, worker: WorkWorker.Bot), Item(2, worker: WorkWorker.Team), Item(3)];

        Assert.Equal(WorkItemsText.AudienceLabel(audience), WorkAudienceText.Label(audience));
        Assert.Equal(WorkItemsText.AudienceHash(audience), WorkAudienceText.Hash(audience));
        Assert.Equal(audience, WorkAudienceText.Of(WorkAudienceText.Hash(audience)));
        Assert.Equal(WorkItemsText.NoneOf(audience), WorkAudienceText.NoneOf(audience));
        Assert.Equal(
            WorkItemsText.Select(items, audience).Select(item => item.Number),
            WorkAudienceText.Select(items, audience).Select(item => item.Number));
    }

    [Theory]
    [InlineData(null, WorkAudience.All)]
    [InlineData(" BOT ", WorkAudience.Bot)]
    [InlineData("team", WorkAudience.Team)]
    [InlineData("people", WorkAudience.All)]
    public void ASegmentSettlesOnTheSameFilterThroughEitherDoor(string? segment, WorkAudience expected)
    {
        Assert.Equal(expected, WorkAudienceText.Of(segment));
        Assert.Equal(WorkItemsText.AudienceOf(segment), WorkAudienceText.Of(segment));
    }

    [Theory]
    [InlineData(WorkWorker.Unknown, null)]
    [InlineData(WorkWorker.Bot, "bot")]
    [InlineData(WorkWorker.Team, "team")]
    public void AWorkersWordIsTheDoorsWord(WorkWorker worker, string? word)
    {
        Assert.Equal(word, WorkAudienceText.WorkerLabel(worker));
        Assert.Equal(WorkItemsText.WorkerLabel(worker), WorkAudienceText.WorkerLabel(worker));
    }

    [Fact]
    public void ASliceAndAComparisonAreTheDoorsSliceAndComparison()
    {
        IReadOnlyList<ColumnVisit> worked =
        [
            new(WorkColumns.InProgress, new DateTimeOffset(2026, 11, 8, 12, 0, 0, TimeSpan.Zero)),
            new(WorkColumns.Done, new DateTimeOffset(2026, 11, 9, 0, 0, 0, TimeSpan.Zero)),
        ];
        var bot = WorkItemsStats.Empty with { CycleTimeHours = new WorkDuration(3, 4, 2), PerWeek = [new WorkWeek(Sunday, 2, 1)] };
        var report = new WorkItemsReport(null, null, WorkItemsStats.Empty, [Item(2, WorkColumns.Done, worked, worker: WorkWorker.Team)], [], bot);

        Assert.Same(bot, WorkAudienceText.Slice(report, WorkAudience.Bot));
        Assert.Equal(WorkItemsText.Slice(report, WorkAudience.Team).CycleTimeHours, WorkAudienceText.Slice(report, WorkAudience.Team).CycleTimeHours);
        Assert.Same(report.Stats, WorkAudienceText.Slice(report, WorkAudience.All));

        var door = WorkItemsText.Comparison(report)!;
        var concern = WorkAudienceText.Comparison(report)!;

        Assert.Equal(door.BotCycle, concern.BotCycle);
        Assert.Equal(door.TeamCycle, concern.TeamCycle);
        Assert.Equal(new WorkDuration(12, 12, 1), concern.TeamCycle);
        Assert.Equal(door.BotWeeks!.Title, concern.BotWeeks!.Title);
        Assert.Null(concern.TeamWeeks);
        Assert.Null(WorkAudienceText.Comparison(new WorkItemsReport(null, null, WorkItemsStats.Empty, [Item(1)], [])));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void TheAudienceRefusalsNameTheArgumentAndTheirReason(int number)
    {
        var audience = (WorkAudience)number;
        var report = new WorkItemsReport(null, null, WorkItemsStats.Empty, [], []);

        Refused("audience", "A work filter has no name.", () => WorkAudienceText.Label(audience));
        Refused("audience", "A work filter has no address.", () => WorkAudienceText.Hash(audience));
        Refused("audience", "A work filter has no empty state.", () => WorkAudienceText.NoneOf(audience));
        Refused("audience", "A work filter has no items.", () => WorkAudienceText.Select([Item(1)], audience));
        Refused("audience", "A work filter has no stats.", () => WorkAudienceText.Slice(report, audience));
        Refused("worker", "A worker has no name.", () => WorkAudienceText.WorkerLabel((WorkWorker)number));

        static void Refused(string parameter, string reason, Func<object?> ask)
        {
            var refused = Assert.Throws<ArgumentOutOfRangeException>(ask);
            Assert.Equal(parameter, refused.ParamName);
            Assert.Contains(reason, refused.Message, StringComparison.Ordinal);
        }
    }

    // ---- the board: WorkBoardText ----

    [Fact]
    public void TheBoardsCountsAndGroupsAreTheDoors()
    {
        IReadOnlyList<WorkItem> items =
        [
            Item(1, WorkColumns.Todo),
            Item(2, "Elsewhere"),
            Item(3, WorkColumns.Done, [new ColumnVisit(WorkColumns.Done, ThreeHoursAgo)]),
            Item(4, WorkColumns.Done, [new ColumnVisit(WorkColumns.Done, Now)]),
            Item(5, WorkColumns.Review, [new ColumnVisit(WorkColumns.Review, ThreeHoursAgo)]),
        ];
        var report = new WorkItemsReport(null, null, WorkItemsStats.Empty, items, []);

        Assert.Equal(WorkItemsText.Wip(report), WorkBoardText.Wip(report.Stats, items));
        Assert.Equal(WorkItemsText.Wip(report.Stats, items), WorkBoardText.Wip(report.Stats, items));
        Assert.Contains(new WorkWip(WorkColumns.Unplaced, 1), WorkBoardText.Wip(report.Stats, items));

        Assert.Equal(
            WorkItemsText.Active(items).Select(group => (group.Column, string.Join(",", group.Items.Select(item => item.Number)))),
            WorkBoardText.Active(items).Select(group => (group.Column, string.Join(",", group.Items.Select(item => item.Number)))));
        Assert.Equal([4, 3], WorkBoardText.RecentlyDone(items).Select(item => item.Number));
        Assert.Equal(WorkItemsText.RecentlyDone(items), WorkBoardText.RecentlyDone(items));

        Assert.Equal(ThreeHoursAgo, WorkBoardText.Entered(items[4], WorkColumns.Review));
        Assert.Equal(WorkItemsText.Entered(items[0], WorkColumns.Todo), WorkBoardText.Entered(items[0], WorkColumns.Todo));
        Assert.Equal("since 3 h ago", WorkBoardText.Since(items[4], Now));
        Assert.Equal(WorkItemsText.Since(items[0], Now), WorkBoardText.Since(items[0], Now));

        Assert.Equal("work-col-in-progress", WorkBoardText.ColumnId(" In Progress "));
        Assert.Equal(WorkItemsText.ColumnId("Ready / Review"), WorkBoardText.ColumnId("Ready / Review"));
    }

    // ---- the measures: WorkMeasureText ----

    [Fact]
    public void TheMeasuresAreSaidInTheDoorsWords()
    {
        IReadOnlyList<WorkItem> metered = [Item(1, costUsd: 6), Item(2, costUsd: 4)];

        Assert.Equal("median 18.5 h · p85 3 d · 11 items", WorkMeasureText.Duration(new WorkDuration(18.5, 72, 11)));
        Assert.Equal(WorkItemsText.Duration(null), WorkMeasureText.Duration(null));
        Assert.Equal(WorkItemsText.NotMetered, WorkMeasureText.Duration(new WorkDuration(null, null, null)));

        Assert.Equal("2 functional-test failures · 1 revert · 12.5 %", WorkMeasureText.Rework(new WorkRework(2, 1, 0.125)));
        Assert.Equal(WorkItemsText.Rework(new WorkRework(null, null, 3.456)), WorkMeasureText.Rework(new WorkRework(null, null, 3.456)));
        Assert.Equal(WorkItemsText.NotMetered, WorkMeasureText.Rework(null));

        Assert.True(WorkMeasureText.HasMeteredCost(metered));
        Assert.Equal(WorkItemsText.HasMeteredCost([Item(1)]), WorkMeasureText.HasMeteredCost([Item(1)]));
        Assert.Equal("$5.00 per item · $10.00 total", WorkMeasureText.Cost(null, metered));
        Assert.Equal(WorkItemsText.Cost(new WorkCost(20, "why"), metered), WorkMeasureText.Cost(new WorkCost(20, "why"), metered));
        Assert.Null(WorkMeasureText.Cost(new WorkCost(20, "why"), [Item(1)]));
        Assert.Equal("$1,234.57", WorkMeasureText.ItemCost(1234.567));
        Assert.Equal(WorkItemsText.ItemCost(null), WorkMeasureText.ItemCost(null));
    }

    // ---- the weekly chart: WorkWeekText ----

    [Fact]
    public void TheChartIsTheDoorsChart()
    {
        IReadOnlyList<WorkWeek> weeks = [new WorkWeek(Sunday, 3, 4), new WorkWeek(null, null, 8)];
        var door = WorkItemsText.Chart(weeks)!;
        var concern = WorkWeekText.Chart(weeks)!;

        Assert.Equal(door.Bars, concern.Bars);
        Assert.Equal(door.Scale, concern.Scale);
        Assert.Equal(door.Title, concern.Title);
        Assert.Null(WorkWeekText.Chart([]));
        Assert.Equal("Week: 0 merged, 8 deployed to prod", WorkWeekText.LatestWeek(concern));
        Assert.Equal(WorkItemsText.LatestWeek(door), WorkWeekText.LatestWeek(concern));
        Assert.Equal(WorkItemsText.ChartHeight, WorkWeekText.BarHeight(8, 8));
        Assert.Equal(WorkItemsText.BarHeight(3, 8), WorkWeekText.BarHeight(3, 8));
        Assert.Equal(0d, WorkWeekText.BarHeight(0, 8));
    }

    // ---- the scope and a release's carries: WorkScopeText ----

    [Fact]
    public void TheScopeLineAndTheCarriesLineAreTheDoors()
    {
        var scope = new WorkItemsScope("https://github.com/org/repo", 7, null);
        var report = new WorkItemsReport(null, scope, WorkItemsStats.Empty, [], [new WorkRelease("2.4.10+abc", null, [15, 12, 12])]);

        Assert.Equal(WorkItemsText.Scope(scope), WorkScopeText.Scope(scope));
        Assert.Equal("org/repo", WorkScopeText.Scope(scope)!.Label);
        Assert.Null(WorkScopeText.Scope(null));
        Assert.Equal("carries #12, #15", WorkScopeText.Carries(report, "2.4.10"));
        Assert.Equal(WorkItemsText.Carries(report, "2.4.11"), WorkScopeText.Carries(report, "2.4.11"));
        Assert.Null(WorkScopeText.Carries(null, "2.4.10"));
    }

    // ---- one row: WorkRowText ----

    [Fact]
    public void ARowsWordsAreTheDoorsWords()
    {
        var item = Item(
            490,
            state: "open",
            assignee: "jo",
            agent: "the bot",
            environments: new WorkItemEnvironments(new WorkDeployment("2.4.1", ThreeHoursAgo), null, new WorkDeployment(null, null)));
        var pull = new WorkPullRequest(12, null, "merged", ThreeHoursAgo, null);
        var candidate = new ReleaseCandidate("2.4.1", ThreeHoursAgo);

        Assert.Equal("#490", WorkRowText.Number(item));
        Assert.Equal(WorkItemsText.Number(Item()), WorkRowText.Number(Item()));
        Assert.Equal(WorkItemsText.IssueTitle(item), WorkRowText.IssueTitle(item));
        Assert.Equal("the bot", WorkRowText.Agent(item));
        Assert.Equal(WorkItemsText.Agent(Item()), WorkRowText.Agent(Item()));
        Assert.Equal("jo", WorkRowText.Assignee(item));
        Assert.Equal(WorkItemsText.Assignee(Item(agent: "x")), WorkRowText.Assignee(Item(agent: "x")));
        Assert.Equal("#12", WorkRowText.PullLabel(pull));
        Assert.Equal(WorkItemsText.PullTitle(pull), WorkRowText.PullTitle(pull));
        Assert.True(WorkRowText.IsMerged("MERGED"));
        Assert.Equal(WorkItemsText.IsMerged("open"), WorkRowText.IsMerged("open"));
        Assert.Equal(WorkItemsText.SessionTitle(1, 1), WorkRowText.SessionTitle(1, 1));
        Assert.Equal("Session 2", WorkRowText.SessionLabel(2, 3));
        Assert.Equal(WorkItemsText.SessionLabel(1, 1), WorkRowText.SessionLabel(1, 1));

        var places = WorkRowText.Places(item);
        Assert.Equal(WorkItemsText.Places(item), places);
        Assert.Equal(["tdd", "prod"], places.Select(place => place.Name));
        Assert.Equal("tdd 2.4.1", WorkRowText.PlaceLabel(places[0]));
        Assert.Equal(WorkItemsText.PlaceLabel(places[1]), WorkRowText.PlaceLabel(places[1]));
        Assert.Equal("Deployed to tdd as 2.4.1, 3 h ago", WorkRowText.PlaceTitle(places[0], Now));
        Assert.Equal(WorkItemsText.PlaceTitle(places[1], Now), WorkRowText.PlaceTitle(places[1], Now));
        Assert.Equal("Octopus release, cut 3 h ago", WorkRowText.CandidateTitle(candidate, Now));
        Assert.Equal(WorkItemsText.CandidateTitle(candidate with { CreatedAt = null }, Now), WorkRowText.CandidateTitle(candidate with { CreatedAt = null }, Now));
    }
}
