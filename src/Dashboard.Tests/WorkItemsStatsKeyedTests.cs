using Dashboard.Work;

namespace Dashboard.Tests;

/// <summary>
/// The three keyed objects of <see cref="WorkItemsStats" /> - <c>wipByColumn</c>, <c>hoursByColumn</c> and
/// <c>throughputByAgent</c> - share one walk (#1127): a blank name is dropped, a key is trimmed, a value that does not
/// read is left out. Each keeps its own answer for "nothing survived" and its own rule for case.
/// </summary>
public class WorkItemsStatsKeyedTests
{
    private static WorkItemsStats StatsOf(string body) => WorkItemsReport.Parse($$"""{ "stats": { {{body}} } }""")!.Stats;

    [Fact]
    public void EachKeyedObjectDropsABlankNameTrimsAKeyAndLeavesOutAValueThatDoesNotRead()
    {
        var stats = StatsOf("""
            "wipByColumn": { "  Todo  ": 2, " ": 7, "Doing": "3" },
            "hoursByColumn": { "  Review  ": 4, "": 5, "Test": "6" },
            "throughputByAgent": { "  cursor  ": 9, "\t": 1, "bot": true }
            """);

        Assert.Equal(new Dictionary<string, int> { ["Todo"] = 2 }, stats.WipByColumn);
        Assert.Equal(new Dictionary<string, WorkDuration> { ["Review"] = new(4, null, null) }, stats.TimePerColumnHours);
        Assert.Equal([new WorkThroughput("cursor", null, null, 9)], stats.Throughput);
    }

    [Fact]
    public void NothingSurvivingIsAnEmptyCountButNoColumnTimesAndNoThroughput()
    {
        var stats = StatsOf("""
            "wipByColumn": { "": 1 },
            "hoursByColumn": { "": 1 },
            "throughputByAgent": { "": 1 }
            """);

        Assert.NotNull(stats.WipByColumn);
        Assert.Empty(stats.WipByColumn);
        Assert.Null(stats.TimePerColumnHours);
        Assert.Null(stats.Throughput);
    }

    [Fact]
    public void ColumnsAreLookedUpWithoutCaseButAgentsDifferingInCaseStayTwoRows()
    {
        var stats = StatsOf("""
            "wipByColumn": { "Todo": 2 },
            "hoursByColumn": { "Review": 4 },
            "throughputByAgent": { "Bot": 3, "bot": 3 }
            """);

        Assert.Equal(2, stats.WipByColumn["TODO"]);
        Assert.Equal(new WorkDuration(4, null, null), stats.TimePerColumnHours!["review"]);
        Assert.Equal([new WorkThroughput("Bot", null, null, 3), new WorkThroughput("bot", null, null, 3)], stats.Throughput);
    }

    [Fact]
    public void AColumnTimeWrittenAsAnObjectKeepsItsP85AndCountAndAPlainNumberCarriesNeither()
    {
        var stats = StatsOf("""
            "hoursByColumn": { "Review": { "median": 1, "p85": 2, "n": 3 }, "Test": 4, "Build": { } }
            """);

        Assert.Equal(
            new Dictionary<string, WorkDuration> { ["Review"] = new(1, 2, 3), ["Test"] = new(4, null, null) },
            stats.TimePerColumnHours);
    }
}
