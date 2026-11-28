namespace Dashboard.Tests;

/// <summary>
/// <see cref="Shares.WholePercent" /> is the one word of the dashboard for a percent rounded to the even whole: the
/// language bar of the "Code" card and the meters of the cluster view both say it. Pinned here are the rounding,
/// the culture, the sign, and the two sites that route through it.
/// </summary>
public class SharesWholePercentTests
{
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
    [InlineData(99.9, "100 %")]
    [InlineData(1234567.8, "1234568 %")]
    [InlineData(double.NaN, "NaN %")]
    [InlineData(double.PositiveInfinity, "Infinity %")]
    public void TheEdgesKeepTheirWords(double percent, string words) =>
        Assert.Equal(words, Shares.WholePercent(percent));

    [Theory]
    [InlineData(1.5, "2 %")]
    [InlineData(-0d, "-0 %")]
    [InlineData(1234567.8, "1234568 %")]
    [InlineData(double.PositiveInfinity, "Infinity %")]
    public void AGermanViewerReadsTheInvariantWords(double percent, string words) =>
        ViewerCulture.German(() => Assert.Equal(words, Shares.WholePercent(percent)));

    [Fact]
    public void ThePercentSignIsASpaceAndTheSign() => Assert.Equal(" %", Shares.PercentSign);

    [Theory]
    [InlineData(1d)]
    [InlineData(1.5)]
    [InlineData(2.5)]
    [InlineData(62.5)]
    [InlineData(99.9)]
    [InlineData(150d)]
    public void TheBarAndTheClusterSayTheWholePercent(double percent)
    {
        Assert.Equal(Shares.WholePercent(percent), ClusterText.Percent(percent));
        Assert.Equal(Shares.WholePercent(percent), BuildText.SharePercent(percent));
    }

    [Theory]
    [InlineData(double.Epsilon, "<1 %")]
    [InlineData(0.5, "<1 %")]
    [InlineData(0d, "0 %")]
    [InlineData(-0d, "-0 %")]
    [InlineData(1d, "1 %")]
    public void TheBarKeepsItsSmallShareWindow(double percent, string words) =>
        Assert.Equal(words, BuildText.SharePercent(percent));

    /// <summary>Neither caller writes the rounding or the sign by hand again.</summary>
    [Theory]
    [InlineData("Cluster/ClusterText.cs")]
    [InlineData("Health/BuildText.cs")]
    public void TheCallersDoNotRetypeTheRule(string file)
    {
        var source = File.ReadAllText(MarkupGuards.SourcePath(file));

        Assert.Contains("Shares.WholePercent(percent)", source, StringComparison.Ordinal);
        Assert.DoesNotContain("\" %\"", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Math.Round(percent).ToString(", source, StringComparison.Ordinal);
    }
}
