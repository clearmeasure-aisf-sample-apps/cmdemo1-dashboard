namespace Dashboard.Tests;

/// <summary>
/// The numbers of a sparkline's title are written with at most one decimal and a whole reading without one,
/// whatever the reader's culture.
/// </summary>
public class TrendNumberEdgeCaseTests
{
    [Fact]
    public void WholeReadingsAreWrittenWithoutADecimal() =>
        Assert.Equal("cpu, last 2 checks: 0 to 45, now 45", Trend.Of([0, 45], "cpu")!.Title);

    [Fact]
    public void AFractionIsWrittenWithOneDecimalAMidpointRoundingAwayFromZero() =>
        Assert.Equal("cpu, last 2 checks: 2.3 to 4, now 2.3", Trend.Of([4, 2.25], "cpu")!.Title);

    [Fact]
    public void AFractionThatRoundsUpToAWholeIsWrittenWithoutADecimal() =>
        Assert.Equal("cpu, last 2 checks: 1 to 3, now 3", Trend.Of([1, 2.96], "cpu")!.Title);

    [Fact]
    public void AFractionThatRoundsDownToAWholeIsWrittenWithoutADecimal() =>
        Assert.Equal("cpu, last 2 checks: 2 to 5, now 2", Trend.Of([5, 2.04], "cpu")!.Title);

    [Fact]
    public void ALargeWholeReadingIsWrittenInFull() =>
        Assert.Equal("bytes, last 2 checks: 1 to 1000000000 B, now 1000000000 B", Trend.Of([1, 1e9], "bytes", " B")!.Title);

    [Fact]
    public void ANegativeZeroReadingIsWrittenAsMinusZero() =>
        Assert.Equal("cpu, last 2 checks: -0 to 4, now 4", Trend.Of([-0.0, 4], "cpu")!.Title);

    [Theory]
    [InlineData("de-DE")]
    [InlineData("fr-FR")]
    public void AReaderWithACommaDecimalStillSeesAPoint(string culture) =>
        Assert.Equal(
            "cpu, last 3 checks: 0.5 to 12.3 %, now 2.3 %",
            CultureScope.Says(culture, () => Trend.Of([0.5, 12.34, 2.25], "cpu", " %")!.Title));
}
