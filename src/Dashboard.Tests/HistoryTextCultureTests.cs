using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// The history counts of <see cref="HistoryText.Describe(IReadOnlyCollection{ProbeResult})" /> are said in the
/// invariant culture whatever the reader's culture is: a thousand checks read <c>1000</c> for an invariant, an
/// en-US, a de-DE and a fr-FR reader alike, and the thread is left holding the culture it had.
/// </summary>
public class HistoryTextCultureTests
{
    private static readonly DateTimeOffset Time = new(2026, 10, 4, 22, 0, 5, TimeSpan.Zero);

    private static ProbeResult Check(HealthState state) => new(state, null, null, Time);

    /// <summary>2236 checks - 1000 healthy, 2 unhealthy and 1234 unreachable - counts large enough that a
    /// culture's group separator would show if the counts were said in it.</summary>
    private static IReadOnlyCollection<ProbeResult> ManyChecks() =>
    [
        .. Enumerable.Repeat(Check(HealthState.Healthy), 1000),
        .. Enumerable.Repeat(Check(HealthState.Unhealthy), 2),
        .. Enumerable.Repeat(Check(HealthState.Unreachable), 1234),
    ];

    [Theory]
    [InlineData("")]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("fr-FR")]
    public void TheCountsAreSaidInTheInvariantCultureForEveryReader(string reader)
    {
        var before = CultureInfo.CurrentCulture;

        var text = CultureScope.Says(reader, () => HistoryText.Describe(ManyChecks()));

        Assert.Equal("Last 2236 checks: 1000 healthy, 2 unhealthy, 1234 unreachable", text);
        Assert.Equal(before, CultureInfo.CurrentCulture);
    }

    [Theory]
    [InlineData("de-DE")]
    [InlineData("fr-FR")]
    public void AReaderCultureSaysTheSameWordsAsTheInvariantCulture(string reader)
    {
        var history = ManyChecks();

        Assert.Equal(
            CultureScope.Says(CultureInfo.InvariantCulture, () => HistoryText.Describe(history)),
            CultureScope.Says(reader, () => HistoryText.Describe(history)));
    }

    [Fact]
    public void OneCheckIsSaidInTheSingularForAGermanReader() =>
        Assert.Equal("Last check: 1 healthy", CultureScope.Says("de-DE", () => HistoryText.Describe([Check(HealthState.Healthy)])));
}
