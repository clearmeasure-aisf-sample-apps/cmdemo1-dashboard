namespace Dashboard.Tests;

/// <summary>
/// The words a <see cref="SummaryLevel" /> says, which now live on one type and nowhere else: the sentence the
/// header announces, the badge's health state and the summary's CSS class. These tests pin each of those sayings
/// against counts that a viewer would recognise, so a change to one of them is seen here before it is seen on the
/// page.
/// </summary>
public class SummaryWordsTests
{
    // ----- Text -----

    /// <summary>The sentence each level says of itself, for the counts that land on it: no nodes, everything
    /// healthy, a problem found, and the two shapes of a pending check (nothing healthy yet, some of them healthy).
    /// The singular is kept for one node, the plural for more, and the not-healthy count is the rest.</summary>
    [Theory]
    [InlineData(SummaryLevel.Empty, 0, 0, 0, "No nodes in the topology")]
    [InlineData(SummaryLevel.AllHealthy, 7, 7, 0, "All 7 nodes healthy")]
    [InlineData(SummaryLevel.AllHealthy, 1, 1, 0, "The only node is healthy")]
    [InlineData(SummaryLevel.Problems, 7, 5, 0, "2 of 7 nodes not healthy")]
    [InlineData(SummaryLevel.Problems, 1, 0, 0, "1 of 1 node not healthy")]
    [InlineData(SummaryLevel.Pending, 3, 0, 3, "Checking 3 nodes")]
    [InlineData(SummaryLevel.Pending, 1, 0, 1, "Checking 1 node")]
    [InlineData(SummaryLevel.Pending, 3, 1, 2, "1 of 3 nodes healthy, 2 being checked")]
    public void TheTextEachLevelSaysOfItself(SummaryLevel level, int total, int healthy, int pending, string expected) =>
        Assert.Equal(expected, SummaryWords.Text(level, total, healthy, pending));

    /// <summary>A level that is not one of the four is refused by <see cref="SummaryWords.Text" />, so a count that
    /// should not exist is seen here before it is seen on the page.</summary>
    [Fact]
    public void ATextForALevelThatIsNotOneOfTheFourIsRefused() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => SummaryWords.Text((SummaryLevel)9, 1, 1, 0));

    // ----- BadgeState -----

    /// <summary>The badge shows the level's meaning: healthy when every node is healthy, unhealthy when a problem
    /// is found, and pending while the checks run. When the topology could not be read, the badge is unhealthy
    /// regardless of the level.</summary>
    [Theory]
    [InlineData(SummaryLevel.Empty, false, HealthState.Pending)]
    [InlineData(SummaryLevel.Empty, true, HealthState.Unhealthy)]
    [InlineData(SummaryLevel.Pending, false, HealthState.Pending)]
    [InlineData(SummaryLevel.Pending, true, HealthState.Unhealthy)]
    [InlineData(SummaryLevel.AllHealthy, false, HealthState.Healthy)]
    [InlineData(SummaryLevel.AllHealthy, true, HealthState.Healthy)]
    [InlineData(SummaryLevel.Problems, false, HealthState.Unhealthy)]
    [InlineData(SummaryLevel.Problems, true, HealthState.Unhealthy)]
    public void TheBadgeStateEachLevelSaysOfItself(SummaryLevel level, bool hasErrors, HealthState expected) =>
        Assert.Equal(expected, SummaryWords.BadgeState(level, hasErrors));

    // ----- CssClass -----

    /// <summary>The summary's CSS class is a named word for each level, not the level's own name lowered: renaming a
    /// level would not rename the style. The four words are the ones the stylesheet matches, and nothing else.</summary>
    [Theory]
    [InlineData(SummaryLevel.Empty, "empty")]
    [InlineData(SummaryLevel.Pending, "pending")]
    [InlineData(SummaryLevel.AllHealthy, "allhealthy")]
    [InlineData(SummaryLevel.Problems, "problems")]
    public void TheCssClassEachLevelSaysOfItself(SummaryLevel level, string expected) =>
        Assert.Equal(expected, SummaryWords.CssClass(level));

    /// <summary>A level that is not one of the four is refused by <see cref="SummaryWords.CssClass" />.</summary>
    [Fact]
    public void ACssClassForALevelThatIsNotOneOfTheFourIsRefused() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => SummaryWords.CssClass((SummaryLevel)9));
}
