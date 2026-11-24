using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// The boundaries of <see cref="HealthDetailText.Duration" /> and <see cref="HealthDetailText.Summary" /> under a
/// non-invariant culture (fi-FI) - the cases that are pinned for the invariant culture in
/// <see cref="HealthDetailTextEdgeCaseTests" /> and <see cref="HealthDetailTests" />, read by a Finnish reader, to
/// confirm the words are written in the invariant culture and say the same as they do to everyone else. The
/// boundaries themselves are pinned there; these confirm the culture does not change them.
/// </summary>
public class HealthDetailTextCultureTests
{
    private static readonly DateTimeOffset ReadAt = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>The same as the edge case tests, read as a Finnish reader: the duration words do not change with the reader.</summary>
    [Theory]
    [InlineData(0.0499, "under 0.1 ms")]
    [InlineData(0.05, "0.1 ms")]
    [InlineData(999.9, "999.9 ms")]
    [InlineData(1000, "1 s")]
    [InlineData(1500, "1.5 s")]
    public void DurationBoundariesAreTheSameForAFinnishReader(double milliseconds, string expected)
    {
        var said = CultureScope.Says("fi-FI", () => HealthDetailText.Duration(milliseconds));
        Assert.Equal(expected, said);
    }

    /// <summary>A name longer than the twenty characters the line holds is cut with an ellipsis, and one of exactly
    /// twenty is kept whole - the same for a Finnish reader as for everyone else.</summary>
    [Fact]
    public void SummaryNameCuttingIsTheSameForAFinnishReader()
    {
        var longName = "ThisNameIsDefinitelyLongerThanTwenty";
        var exactlyTwenty = "ExactlyTwentyChars!!";

        var longCut = CultureScope.Says("fi-FI", () => HealthDetailText.Summary(
            new HealthDetail([new HealthCheckEntry(longName, CheckState.Unhealthy)], ReadAt)));
        var exactlyKept = CultureScope.Says("fi-FI", () => HealthDetailText.Summary(
            new HealthDetail([new HealthCheckEntry(exactlyTwenty, CheckState.Degraded)], ReadAt)));

        Assert.Equal($"{longName[..19]}… unhealthy", longCut);
        Assert.Equal($"{exactlyTwenty} degraded", exactlyKept);
    }
}
