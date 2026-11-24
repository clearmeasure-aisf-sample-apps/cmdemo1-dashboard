namespace Dashboard.Tests;

/// <summary>
/// The precedence <see cref="HealthSummary.Level" /> asks its counts in - a total of nothing, then a problem, then a
/// pending check, then all healthy - pinned as one ordered table, and the declaration guarded to read as that ordered
/// switch rather than a chain of nested conditional operators.
/// </summary>
public class HealthSummaryLevelPrecedenceTests
{
    private static readonly string[] PrecedenceAsks =
        ["Total == 0", "NotHealthy > 0", "Pending > 0", "_ => SummaryLevel.AllHealthy"];

    /// <summary>Each row sets the earlier asks false and the one under test true, with every later ask also true, so
    /// the row only passes when the earlier ask wins over every later one.</summary>
    [Theory]
    [InlineData(0, -5, 3, SummaryLevel.Empty)] // total of nothing, with a problem (-(-5)-3 = 2) and a pending check
    [InlineData(4, 1, 2, SummaryLevel.Problems)] // one not healthy, with a pending check
    [InlineData(4, 1, 3, SummaryLevel.Pending)] // nothing not healthy, three pending
    [InlineData(4, 4, 0, SummaryLevel.AllHealthy)] // nothing left to ask
    public void LevelTakesTheFirstAskThatHolds(int total, int healthy, int pending, SummaryLevel expected)
    {
        Assert.Equal(expected, new HealthSummary(total, healthy, pending).Level);
    }

    [Fact]
    public void LevelIsDeclaredAsAnOrderedSwitchAndNotNestedTernaries()
    {
        var source = File.ReadAllText(Path.Combine(RepoRoot(), "src", "Dashboard", "Health", "HealthSummary.cs"));

        var start = source.IndexOf("public SummaryLevel Level", StringComparison.Ordinal);
        Assert.True(start >= 0, "no Level property was found in HealthSummary.cs");
        var end = source.IndexOf("};", start, StringComparison.Ordinal);
        Assert.True(end > start, "the Level property is not a switch expression closed by '};'");
        var body = source[start..end];

        Assert.Contains("this switch", body, StringComparison.Ordinal);
        Assert.DoesNotContain(" ? ", body, StringComparison.Ordinal);

        var asks = PrecedenceAsks
            .Select(ask => body.IndexOf(ask, StringComparison.Ordinal))
            .ToArray();
        Assert.All(asks, index => Assert.True(index >= 0, "an ask of the precedence is missing from Level"));
        Assert.Equal(asks.Order().ToArray(), asks);
    }

    private static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Dashboard.sln")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory.FullName;
    }
}
