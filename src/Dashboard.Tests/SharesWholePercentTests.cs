namespace Dashboard.Tests;

/// <summary>
/// <see cref="Shares.WholePercent" /> is the one word of a percent rounded to the even whole: the meters of the
/// cluster view (<see cref="ClusterText.Percent" />) and the language bar of the "Code" card
/// (<see cref="BuildText.SharePercent" />) both say it, with the one <see cref="Shares.PercentSign" />.
/// </summary>
public class SharesWholePercentTests
{
    [Fact]
    public void TheSignIsASpaceAndThePercentSign() => Assert.Equal(" %", Shares.PercentSign);

    [Theory]
    [InlineData(0.5, "0 %")]
    [InlineData(1.5, "2 %")]
    [InlineData(2.5, "2 %")]
    [InlineData(62.5, "62 %")]
    [InlineData(63.5, "64 %")]
    [InlineData(-1.5, "-2 %")]
    public void AHalfRoundsToTheEvenWhole(double percent, string words) =>
        Assert.Equal(words, Shares.WholePercent(percent));

    [Theory]
    [InlineData(0d, "0 %")]
    [InlineData(-0d, "-0 %")]
    [InlineData(-0.4, "-0 %")]
    [InlineData(double.Epsilon, "0 %")]
    [InlineData(99.9, "100 %")]
    [InlineData(double.NaN, "NaN %")]
    public void TheEdgesKeepTheirWords(double percent, string words) =>
        Assert.Equal(words, Shares.WholePercent(percent));

    [Fact]
    public void AGermanViewerReadsTheInvariantWords() =>
        ViewerCulture.German(() =>
        {
            Assert.Equal("2 %", Shares.WholePercent(1.5));
            Assert.Equal("-0 %", Shares.WholePercent(-0d));
            Assert.Equal("1234568 %", Shares.WholePercent(1234567.8));
            Assert.Equal("Infinity %", Shares.WholePercent(double.PositiveInfinity));
            Assert.Equal("NaN %", Shares.WholePercent(double.NaN));
        });

    [Theory]
    [InlineData(1d)]
    [InlineData(1.5)]
    [InlineData(2.5)]
    [InlineData(62.5)]
    [InlineData(72.6)]
    [InlineData(100d)]
    [InlineData(1234567.8)]
    public void TheClusterAndTheCodeCardSayTheSameWholePercent(double percent)
    {
        var words = Shares.WholePercent(percent);

        Assert.Equal(words, ClusterText.Percent(percent));
        Assert.Equal(words, BuildText.SharePercent(percent));
    }

    [Theory]
    [InlineData(double.Epsilon, "<1 %")]
    [InlineData(0.5, "<1 %")]
    [InlineData(0d, "0 %")]
    [InlineData(-0d, "-0 %")]
    [InlineData(1d, "1 %")]
    public void TheCodeCardKeepsItsSmallShareArm(double percent, string words) =>
        Assert.Equal(words, BuildText.SharePercent(percent));
}
