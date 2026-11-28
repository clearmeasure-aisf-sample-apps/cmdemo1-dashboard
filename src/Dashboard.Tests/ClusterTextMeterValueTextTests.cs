namespace Dashboard.Tests;

/// <summary>
/// The meter's value text: the number in words, then its share when there is one (<c>0.81 of 3.86 cores, 21 %</c>).
/// <see cref="ClusterText.MeterValueText" /> is the one place that builds the text the meter says to a reader -
/// both the <c>aria-valuetext</c> and the second half of the <c>title</c> of <c>Meter.razor</c> - and the two
/// components that feed it, <c>UseRow.razor</c> and <c>UseCell.razor</c>, call it rather than interpolating
/// <see cref="ClusterText.Percent" /> into a <c>ValueText</c> of their own.
/// </summary>
public class ClusterTextMeterValueTextTests
{
    private const string Reader = "fi-FI";

    [Theory]
    [InlineData("0.81 of 3.86 cores", 21.4, "0.81 of 3.86 cores, 21 %")]
    [InlineData("258 MiB", 50.4, "258 MiB, 50 %")]
    [InlineData("9.2 GiB", 92.0, "9.2 GiB, 92 %")]
    public void AValueWithAShareSaysTheNumberThenItsShare(string value, double share, string words)
    {
        Assert.Equal(words, CultureScope.Says(Reader, () => ClusterText.MeterValueText(value, share)));
    }

    [Theory]
    [InlineData("0.81 of 3.86 cores", "0.81 of 3.86 cores")]
    [InlineData("258 MiB", "258 MiB")]
    [InlineData("47 min", "47 min")]
    public void AValueWithoutAShareSaysTheNumberAlone(string value, string words)
    {
        Assert.Equal(words, ClusterText.MeterValueText(value, null));
    }

    [Fact]
    public void NeitherAValueNorAShareSaysNothing()
    {
        Assert.Equal(string.Empty, ClusterText.MeterValueText(null, null));
    }

    [Fact]
    public void AShareWithoutAValueKeepsTheSeparatorTheValueLeftOpen()
    {
        // Exactly as UseRow's interpolation does today: the number is missing, the share is not.
        Assert.Equal(", 63 %", ClusterText.MeterValueText(null, 63));
    }

    [Theory]
    [InlineData(21.5, "0.81 of 3.86 cores, 22 %")]
    [InlineData(22.5, "0.81 of 3.86 cores, 22 %")]
    public void TheShareIsRoundedToEven(double share, string words)
    {
        // 21.5 and 22.5 both round to 22 under Math.Round's default to-even rule, in the invariant culture.
        Assert.Equal(words, CultureScope.Says(Reader, () => ClusterText.MeterValueText("0.81 of 3.86 cores", share)));
    }

    [Fact]
    public void AShareAboveTheWholeIsNotClampedHere()
    {
        // The share is not clamped: 150 gives 150 %. Only ClusterText.MeterValue clamps, for the bar's scale.
        Assert.Equal("14 of 500 m, 150 %", ClusterText.MeterValueText("14 of 500 m", 150));
    }

    [Fact]
    public void TheWordsAreShapedInTheInvariantCulture()
    {
        // fi-FI writes the decimal comma and the minus sign as U+2212; the value text never lets the reader's
        // culture shape it, because Percent writes the share in the invariant culture.
        var words = CultureScope.Says(Reader, () => ClusterText.MeterValueText("0.81 of 3.86 cores", 21.5));
        Assert.Equal("0.81 of 3.86 cores, 22 %", words);
    }

    [Theory]
    [InlineData("UseRow")]
    [InlineData("UseCell")]
    public void TheComponentsCallMeterValueTextForTheirMeter(string component)
    {
        var text = MarkupGuards.ComponentText(component);

        Assert.Contains("ClusterText.MeterValueText", text, StringComparison.Ordinal);

        // The old rule - ClusterText.Percent(share) interpolated into a ValueText - is gone from both.
        Assert.DoesNotContain("ValueText=\"@(Percent is { } share", text, StringComparison.Ordinal);
    }
}
