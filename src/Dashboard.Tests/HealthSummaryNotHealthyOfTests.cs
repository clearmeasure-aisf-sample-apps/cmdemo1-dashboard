namespace Dashboard.Tests;

/// <summary>
/// The one home the not-healthy subtraction lives in, <see cref="HealthSummary" />'s
/// <c>NotHealthyOf</c>: the count that is neither healthy nor pending, said once and read from both sides of the
/// header - <c>HealthSummary.NotHealthy</c>, which <c>Level</c> weighs against, and <c>SummaryWords.Text</c>, which
/// prints it in the Problems sentence. The member is <c>internal</c> and the app declares no
/// <c>InternalsVisibleTo</c>, so the tests reach it by reflection, the way the other internal reads are pinned.
/// </summary>
public class HealthSummaryNotHealthyOfTests
{
    /// <summary>The rest, for the ordinary counts: whatever is neither healthy nor pending.</summary>
    [Theory]
    [InlineData(5, 2, 1, 2)]
    [InlineData(0, 0, 0, 0)]
    [InlineData(1, 0, 0, 1)]
    [InlineData(7, 7, 0, 0)]
    [InlineData(3, 1, 1, 1)]
    public void NotHealthyOfReturnsTheRestForOrdinaryCounts(int total, int healthy, int pending, int expected) =>
        Assert.Equal(expected, NotHealthyOf(total, healthy, pending));

    /// <summary>Counts that do not add up are kept as given, exactly as <see cref="HealthSummary" /> keeps them today:
    /// a healthy count above the total leaves the result negative, unclamped.</summary>
    [Theory]
    [InlineData(10, 20, 5, -15)]
    [InlineData(2, 5, 1, -4)]
    [InlineData(0, 3, 1, -4)]
    public void NotHealthyOfKeepsInconsistentCountsAsGiven(int total, int healthy, int pending, int expected) =>
        Assert.Equal(expected, NotHealthyOf(total, healthy, pending));

    /// <summary>The property and the rule agree: for the same counts, <c>new HealthSummary(t, h, p).NotHealthy</c>
    /// equals <c>HealthSummary.NotHealthyOf(t, h, p)</c>, so the badge's level and the count it weighs cannot drift.</summary>
    [Theory]
    [InlineData(5, 2, 1)]
    [InlineData(0, 0, 0)]
    [InlineData(1, 0, 0)]
    [InlineData(7, 7, 0)]
    [InlineData(3, 1, 1)]
    [InlineData(10, 20, 5)]
    [InlineData(100, 30, 10)]
    public void NotHealthyPropertyEqualsNotHealthyOfForTheSameCounts(int total, int healthy, int pending)
    {
        var summary = new HealthSummary(total, healthy, pending);

        Assert.Equal(NotHealthyOf(total, healthy, pending), summary.NotHealthy);
    }

    /// <summary>The Problems sentence the header prints starts with the very count the rule owns, followed by
    /// <c>" of "</c>: the number the badge decides on is the number the sentence says out loud.</summary>
    [Theory]
    [InlineData(5, 2, 1)]
    [InlineData(1, 0, 0)]
    [InlineData(100, 30, 10)]
    public void TheProblemsSentenceStartsWithNotHealthyOfFollowedByOf(int total, int healthy, int pending)
    {
        var sentence = SummaryWords.Text(SummaryLevel.Problems, total, healthy, pending);

        Assert.StartsWith($"{NotHealthyOf(total, healthy, pending)} of ", sentence);
    }

    /// <summary>The member the tests pin: <c>internal static int NotHealthyOf(int, int, int)</c> on
    /// <see cref="HealthSummary" />, reached by name under its own type.</summary>
    private static int NotHealthyOf(int total, int healthy, int pending)
    {
        var method = typeof(HealthSummary).GetMethod(
            "NotHealthyOf",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        Assert.NotNull(method);
        Assert.Equal(typeof(int), method!.ReturnType);
        Assert.Equal(typeof(int), method.GetParameters()[0].ParameterType);

        return (int)method.Invoke(null, [total, healthy, pending])!;
    }
}
