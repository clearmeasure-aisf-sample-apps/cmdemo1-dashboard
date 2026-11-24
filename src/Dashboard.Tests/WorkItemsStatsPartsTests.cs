using Dashboard.Work;

namespace Dashboard.Tests;

/// <summary>
/// Each part of <see cref="WorkItemsStats" /> is read from its own word of <c>workitems.json</c> (#1466). Every part
/// below carries a value no other part carries, so a stats reader that stood two parts in each other's place - in
/// particular the three durations <c>cycleTimeHours</c>, <c>leadTimeHours</c> and <c>mergeToProdHours</c>, which are
/// the same type - turns one of these red.
/// </summary>
public class WorkItemsStatsPartsTests
{
    private const string Parts = """
        "wipByColumn": { "Todo": 3 },
        "cycleTimeHours": { "median": 1, "p85": 2, "n": 3 },
        "leadTimeHours": { "median": 11, "p85": 12, "n": 13 },
        "perWeek": [ { "weekStart": "2026-11-02T00:00:00Z", "merged": 4, "deployedProd": 2 } ],
        "rework": { "ftFailures": 5, "reverts": 6, "rate": 0.25 },
        "cost": { "totalUsd": 7.5 },
        "hoursByColumn": { "Review": 8 },
        "throughputByAgent": { "cursor": 9 },
        "mergeToProdHours": { "median": 21, "p85": 22, "n": 23 }
        """;

    [Fact]
    public void EachDurationOfTheOlderShapeIsReadFromItsOwnWord()
    {
        var stats = WorkItemsReport.Parse($$"""{ "stats": { {{Parts}} } }""")!.Stats;

        AssertEachPartFromItsOwnWord(stats);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("bot")]
    [InlineData("team")]
    public void EachDurationOfANestedSliceIsReadFromItsOwnWord(string slice)
    {
        var report = WorkItemsReport.Parse($$"""{ "stats": { "{{slice}}": { {{Parts}} } } }""")!;

        var stats = slice switch
        {
            "all" => report.Stats,
            "bot" => report.BotStats,
            _ => report.TeamStats,
        };

        AssertEachPartFromItsOwnWord(Assert.IsType<WorkItemsStats>(stats));
    }

    private static void AssertEachPartFromItsOwnWord(WorkItemsStats stats)
    {
        Assert.Equal(new Dictionary<string, int> { ["Todo"] = 3 }, stats.WipByColumn);
        Assert.Equal(new WorkDuration(1, 2, 3), stats.CycleTimeHours);
        Assert.Equal(new WorkDuration(11, 12, 13), stats.LeadTimeHours);
        Assert.Equal(new WorkDuration(21, 22, 23), stats.MergeToProdHours);
        var week = Assert.Single(stats.PerWeek);
        Assert.Equal(new WorkWeek(new DateTimeOffset(2026, 11, 2, 0, 0, 0, TimeSpan.Zero), 4, 2), week);
        Assert.Equal(new WorkRework(5, 6, 0.25), stats.Rework);
        Assert.Equal(new WorkCost(7.5, null), stats.Cost);
        Assert.Equal(new WorkDuration(8, null, null), Assert.Single(stats.TimePerColumnHours!).Value);
        Assert.Equal(new WorkThroughput("cursor", null, null, 9), Assert.Single(stats.Throughput!));
    }
}
