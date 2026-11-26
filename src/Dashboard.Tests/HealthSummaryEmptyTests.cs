namespace Dashboard.Tests;

/// <summary>
/// <see cref="HealthSummary.Empty" /> is what the header shows before anything is counted. It is the same summary an
/// empty topology counts to, and it says the same sentence, CSS class and badge that App.razor showed when it spelled
/// the summary out as <c>new HealthSummary(0, 0, 0)</c>.
/// </summary>
public class HealthSummaryEmptyTests
{
    /// <summary>The empty summary counts no node, and its level is the empty level.</summary>
    [Fact]
    public void TheEmptySummaryCountsNothing()
    {
        var empty = HealthSummary.Empty;

        Assert.Equal(0, empty.Total);
        Assert.Equal(0, empty.Healthy);
        Assert.Equal(0, empty.Pending);
        Assert.Equal(0, empty.NotHealthy);
        Assert.Equal(SummaryLevel.Empty, empty.Level);
    }

    /// <summary>The empty summary equals the zero triple it replaces and the summary an empty topology counts to.</summary>
    [Fact]
    public void TheEmptySummaryIsTheZeroTripleAndTheCountOfNothing()
    {
        Assert.Equal(new HealthSummary(0, 0, 0), HealthSummary.Empty);
        Assert.Equal(HealthSummary.Of([]), HealthSummary.Empty);
        Assert.Same(HealthSummary.Empty, HealthSummary.Empty);
    }

    /// <summary>The header still says what it said for the zero triple: the sentence and the CSS class.</summary>
    [Fact]
    public void TheEmptySummarySaysWhatTheHeaderShowed()
    {
        Assert.Equal("No nodes in the topology", HealthSummary.Empty.Text);
        Assert.Equal("empty", SummaryWords.CssClass(HealthSummary.Empty.Level));
    }

    /// <summary>The badge of the empty summary is pending while the topology loads, and unhealthy when it could not be read.</summary>
    [Theory]
    [InlineData(false, HealthState.Pending)]
    [InlineData(true, HealthState.Unhealthy)]
    public void TheEmptySummaryBadgeFollowsTheErrors(bool hasErrors, HealthState expected) =>
        Assert.Equal(expected, SummaryWords.BadgeState(HealthSummary.Empty.Level, hasErrors));
}
