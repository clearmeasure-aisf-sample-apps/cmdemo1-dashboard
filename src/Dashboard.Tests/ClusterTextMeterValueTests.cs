namespace Dashboard.Tests;

/// <summary>
/// Where the meter's bar stands: <see cref="ClusterText.MeterValue" /> clamps the share to the scale
/// (<see cref="ClusterText.MeterEmpty" /> to <see cref="ClusterText.MeterFull" />), rounds it to a whole number
/// and writes it in the invariant culture, so a share past either end stands on that end and the words never
/// depend on the reader's culture. The assertions are taken under fi-FI - Finnish writes the minus sign as
/// U+2212 and the decimal comma - through the shared invariant-culture scope, against invariant-culture literal
/// text the culture was never asked to shape.
/// </summary>
public class ClusterTextMeterValueTests
{
    private const string Reader = "fi-FI";

    [Theory]
    [InlineData(21.4, "21")]
    [InlineData(21.5, "22")]
    [InlineData(0, "0")]
    [InlineData(100, "100")]
    [InlineData(150, "100")]
    [InlineData(-5, "0")]
    public void TheBarStandsOnTheWholeNumberOfItsScale(double percent, string words)
    {
        // 21.5 rounds to 22 under Math.Round's default to-even rule, written down on purpose.
        Assert.Equal(words, CultureScope.Says(Reader, () => ClusterText.MeterValue(percent)));
    }

    [Fact]
    public void TheScaleEndsAreZeroAndHundredPercent()
    {
        Assert.Equal(0, ClusterText.MeterEmpty);
        Assert.Equal(100, ClusterText.MeterFull);
    }

    [Fact]
    public void TheHighMarkStandsOnTheScale()
    {
        Assert.True(ClusterText.MeterEmpty < ClusterText.HighPercent);
        Assert.True(ClusterText.HighPercent <= ClusterText.MeterFull);
    }
}
