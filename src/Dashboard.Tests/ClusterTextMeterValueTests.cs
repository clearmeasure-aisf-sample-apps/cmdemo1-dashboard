namespace Dashboard.Tests;

/// <summary>
/// The meter's scale ends and the rule for where its bar stands: <see cref="ClusterText.MeterEmpty" /> and
/// <see cref="ClusterText.MeterFull" /> name the two ends the bar is clamped to, and
/// <see cref="ClusterText.MeterValue" /> clamps a share to that scale, rounds it to a whole number, and says it in
/// the invariant culture. The rounding is pinned to <see cref="Math.Round" />'s default to-even rule on purpose.
/// Every assertion is taken under <c>fi-FI</c> through the shared <see cref="CultureScope" /> so a number that slips
/// back into the thread's culture - a Finnish group separator, a different minus - is caught.
/// </summary>
public class ClusterTextMeterValueTests
{
    private const string Culture = "fi-FI";

    /// <summary>
    /// Where a meter's bar stands, in words: the share clamped to the scale and rounded to a whole number, said in
    /// the invariant culture whatever the thread's culture is.
    /// </summary>
    [Theory]
    [InlineData(21.4, "21")]
    // 21.5 rounds to the even side (22) under Math.Round's default to-even rule - written down on purpose.
    [InlineData(21.5, "22")]
    [InlineData(0, "0")]
    [InlineData(100, "100")]
    // A share past either end stands on that end: above the whole fills the bar, below empty draws it empty.
    [InlineData(150, "100")]
    [InlineData(-5, "0")]
    public void TheMeterValueIsClampedToTheScaleAndRoundedToAWholeNumber(double percent, string words) =>
        Assert.Equal(words, CultureScope.Says(Culture, () => ClusterText.MeterValue(percent)));

    /// <summary>
    /// The scale ends are the ends of a percentage scale: the empty end is zero, the full end is one hundred, and the
    /// mark that calls a meter high stands strictly between the two.
    /// </summary>
    [Fact]
    public void TheMeterScaleEndsAreTheEndsOfAPercentageScale() =>
        CultureScope.Runs(Culture, () =>
        {
            Assert.Equal(0, ClusterText.MeterEmpty);
            Assert.Equal(100, ClusterText.MeterFull);
            Assert.True(ClusterText.MeterEmpty < ClusterText.HighPercent);
            Assert.True(ClusterText.HighPercent <= ClusterText.MeterFull);
        });
}
