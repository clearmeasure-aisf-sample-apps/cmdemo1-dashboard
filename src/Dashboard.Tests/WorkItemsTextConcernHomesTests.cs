using System.Reflection;
using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="WorkItemsText" /> stays the one public <c>Text</c> class of the work page, the house style, and
/// <see cref="WorkItemsTextOpenEdgeCaseTests" /> still pins its forty names. The concerns behind them now have homes
/// of their own: the audience words beside the report model, the board's counting beside <see cref="WorkColumns" />,
/// the duration and rework measures, the cost wording, and the weekly chart. These tests pin those homes. They are
/// internal, and the app declares no <c>InternalsVisibleTo</c>, so the tests read them by reflection and check that
/// the facade says what its home says.
/// </summary>
public class WorkItemsTextConcernHomesTests
{
    private static readonly Assembly App = typeof(WorkItemsText).Assembly;

    private static Type Home(string name) =>
        App.GetType("Dashboard.Work." + name, throwOnError: true)!;

    private static WorkItem Item(int number, string? column, double? costUsd = null) =>
        new(number, $"Item {number}", null, null, column, [], null, null, [], [], null, WorkItemEnvironments.None, costUsd);

    private static object? Call(string home, string method, params object?[] arguments) =>
        Home(home).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)!.Invoke(null, arguments);

    [Theory]
    [InlineData("WorkAudienceWords", new[] { "AudienceOf", "Hash", "Label", "WorkerLabel", "WorkerOf" })]
    [InlineData("WorkBoard", new[] { "Active", "Entered", "RecentlyDone", "Wip" })]
    [InlineData("WorkMeasureText", new[] { "Duration", "Rework" })]
    [InlineData("WorkCostText", new[] { "Cost", "HasMeteredCost", "ItemCost" })]
    [InlineData("WorkWeekCharts", new[] { "BarHeight", "Chart", "LatestWeek" })]
    public void EachConcernHasAnInternalStaticHomeWithItsOwnMembers(string name, string[] members)
    {
        var type = Home(name);

        Assert.False(type.IsPublic);
        Assert.True(type.IsAbstract && type.IsSealed, $"{name} is a static class");
        Assert.Empty(type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Equal(
            members,
            type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(method => method.IsAssembly && !method.IsSpecialName)
                .Select(method => method.Name)
                .Order(StringComparer.Ordinal));
    }

    [Fact]
    public void TheFacadeKeepsNoInternalWorkerReaderOfItsOwn()
    {
        Assert.Null(typeof(WorkItemsText).GetMethod("WorkerOf", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly));
    }

    [Fact]
    public void TheChartHeightIsTheChartsOwnHeight()
    {
        var height = Home("WorkWeekCharts").GetField("Height", BindingFlags.NonPublic | BindingFlags.Static)!;

        Assert.True(height.IsLiteral);
        Assert.Equal(WorkItemsText.ChartHeight, (double)height.GetRawConstantValue()!);
        Assert.Equal(36, WorkItemsText.ChartHeight);
    }

    [Theory]
    [InlineData(WorkAudience.All)]
    [InlineData(WorkAudience.Bot)]
    [InlineData(WorkAudience.Team)]
    public void TheAudienceWordsOfTheFacadeAreTheTablesWords(WorkAudience audience)
    {
        Assert.Equal(Call("WorkAudienceWords", "Label", audience), WorkItemsText.AudienceLabel(audience));
        Assert.Equal(Call("WorkAudienceWords", "Hash", audience), WorkItemsText.AudienceHash(audience));
        Assert.Equal(audience, WorkItemsText.AudienceOf(WorkItemsText.AudienceHash(audience)));
    }

    [Theory]
    [InlineData(WorkWorker.Bot)]
    [InlineData(WorkWorker.Team)]
    public void TheParserReadsAWorkerByTheSameWordTheRowShows(WorkWorker worker)
    {
        var word = WorkItemsText.WorkerLabel(worker)!;
        var json = $$"""{ "items": [ { "number": 1, "title": "One", "worker": {{JsonSerializer.Serialize(word.ToUpperInvariant())}} } ] }""";

        Assert.Equal(worker, Assert.Single(WorkItemsReport.Parse(json)!.Items).Worker);
        Assert.Equal(worker, Call("WorkAudienceWords", "WorkerOf", word));
        Assert.Null(WorkItemsText.WorkerLabel(WorkWorker.Unknown));
    }

    [Fact]
    public void TheMeasuresAndTheCostOfTheFacadeAreTheirHomesWords()
    {
        var duration = new WorkDuration(18.5, 72, 11);
        var rework = new WorkRework(2, 1, 0.125);
        WorkItem[] items = [Item(1, null, 4), Item(2, null)];

        Assert.Equal("median 18.5 h · p85 3 d · 11 items", WorkItemsText.Duration(duration));
        Assert.Equal(Call("WorkMeasureText", "Duration", duration), WorkItemsText.Duration(duration));
        Assert.Equal("2 functional-test failures · 1 revert · 12.5 %", WorkItemsText.Rework(rework));
        Assert.Equal(Call("WorkMeasureText", "Rework", rework), WorkItemsText.Rework(rework));
        Assert.Equal(WorkItemsText.NotMetered, WorkItemsText.Duration(new WorkDuration(null, null, null)));
        Assert.Equal(WorkItemsText.NotMetered, WorkItemsText.Rework(new WorkRework(null, null, null)));

        Assert.Equal(Call("WorkCostText", "Cost", null, items), WorkItemsText.Cost(null, items));
        Assert.Equal(Call("WorkCostText", "ItemCost", 4.0), WorkItemsText.ItemCost(4));
        Assert.Equal(WorkItemsText.NotMetered, WorkItemsText.ItemCost(null));
        Assert.Equal(true, Call("WorkCostText", "HasMeteredCost", (object)items));
    }

    [Fact]
    public void ABlankAndAnUnknownColumnAreUnplacedAlikeInTheCountAndTheGroups()
    {
        WorkItem[] items =
        [
            Item(1, null),
            Item(2, "Parked"),
            Item(3, WorkColumns.Done),
            Item(4, WorkColumns.Todo),
        ];

        var unplacedRow = Assert.Single(WorkItemsText.Wip(WorkItemsStats.Empty, items), row => row.Column == WorkColumns.Unplaced);
        var unplacedGroup = Assert.Single(WorkItemsText.Active(items), group => group.Column == WorkColumns.Unplaced);

        Assert.Equal(2, unplacedRow.Count);
        Assert.Equal([1, 2], unplacedGroup.Items.Select(item => item.Number));
        Assert.Equal(WorkItemsText.Active(items).Select(group => group.Column), ((IReadOnlyList<WorkColumnGroup>)Call("WorkBoard", "Active", (object)items)!).Select(group => group.Column));
    }
}
