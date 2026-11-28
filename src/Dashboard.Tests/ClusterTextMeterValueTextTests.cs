namespace Dashboard.Tests;

/// <summary>
/// The words a screen reader hears for a cluster meter's value (<c>aria-valuetext</c>, and the second half of the
/// meter's <c>title</c>): the number in words, then its share when there is one - <c>0.81 of 3.86 cores, 21 %</c> -
/// said by <see cref="ClusterText.MeterValueText" />, the one place that builds them. The assertions are taken under
/// fi-FI - Finnish writes the decimal comma - through the shared invariant-culture scope, against invariant-culture
/// literal text the culture was never asked to shape. The rule is shown to live in the two components that draw the
/// meters: <c>UseRow.razor</c> and <c>UseCell.razor</c> both hand the words to
/// <c>ClusterText.MeterValueText</c> and neither builds them in markup.
/// </summary>
public class ClusterTextMeterValueTextTests
{
    private const string Reader = "fi-FI";

    [Theory]
    [InlineData("0.81 of 3.86 cores", 21.0, "0.81 of 3.86 cores, 21 %")]
    [InlineData("258 of 512 MiB", 50.4, "258 of 512 MiB, 50 %")]
    public void AValueWithAShareSaysTheValueThenTheShare(string value, double percent, string words)
    {
        Assert.Equal(words, CultureScope.Says(Reader, () => ClusterText.MeterValueText(value, percent)));
    }

    [Fact]
    public void AValueWithoutAShareSaysTheValueAlone()
    {
        Assert.Equal("0.81 of 3.86 cores", CultureScope.Says(Reader, () => ClusterText.MeterValueText("0.81 of 3.86 cores", null)));
    }

    [Fact]
    public void NeitherValueNorShareSaysTheEmptyString()
    {
        Assert.Equal(string.Empty, CultureScope.Says(Reader, () => ClusterText.MeterValueText(null, null)));
    }

    [Fact]
    public void AShareWithoutAValueSaysTheShareAfterTheSeparator()
    {
        // Exactly as the old markup interpolation did: a missing value leaves the leading separator and the share.
        Assert.Equal(", 63 %", ClusterText.MeterValueText(null, 63));
    }

    [Theory]
    [InlineData(21.5)]
    [InlineData(22.5)]
    public void TheShareRoundsToEven(double percent)
    {
        // 21.5 and 22.5 both round to 22 under Math.Round's default to-even rule, written down on purpose.
        Assert.Equal("3, 22 %", ClusterText.MeterValueText("3", percent));
    }

    [Fact]
    public void AShareAboveHundredIsNotClampedHere()
    {
        // Only MeterValue clamps the bar; the words say the share as given.
        Assert.Equal("3, 150 %", ClusterText.MeterValueText("3", 150));
    }

    [Fact]
    public void BothComponentsBuildTheValueTextWithMeterValueText()
    {
        var row = MarkupGuards.ComponentText("UseRow");
        var cell = MarkupGuards.ComponentText("UseCell");

        Assert.Contains("ClusterText.MeterValueText(Value, ShowPercent ? Percent : null)", row, StringComparison.Ordinal);
        Assert.Contains("ClusterText.MeterValueText(value, Percent)", cell, StringComparison.Ordinal);
        Assert.DoesNotContain("ClusterText.Percent(share)", row, StringComparison.Ordinal);
        Assert.DoesNotContain("ClusterText.Percent(share)", cell, StringComparison.Ordinal);
    }
}
