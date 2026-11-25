using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="BuildText" /> names its number formats and its percent sign once: these pin that every number word
/// still says the same characters, that <c>Percent</c> and <c>Number</c> round the same input to the same one
/// decimal, and that none of them follows the current culture.
/// </summary>
public sealed class BuildTextFormatTests
{
    [Theory]
    [InlineData(81.25)]
    [InlineData(0.05)]
    [InlineData(99.95)]
    [InlineData(80d)]
    [InlineData(4.25)]
    public void PercentIsTheNumberWithThePercentSign(double value) =>
        Assert.Equal(BuildText.Number(value) + " %", BuildText.Percent(value));

    [Fact]
    public void EveryNumberWordKeepsItsCharactersUnderACommaDecimalCulture()
    {
        var before = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

            Assert.Equal("84,210", BuildText.Count(84210));
            Assert.Equal("81.2 %", BuildText.Percent(81.24));
            Assert.Equal("81.3 %", BuildText.Percent(81.25));
            Assert.Equal("80 %", BuildText.Percent(80));
            Assert.Equal("4.2", BuildText.Number(4.24));
            Assert.Equal("4.3", BuildText.Number(4.25));
            Assert.Equal("4", BuildText.Number(4));
            Assert.Equal("<1 %", BuildText.SharePercent(0.4));
            Assert.Equal("0 %", BuildText.SharePercent(0));
            Assert.Equal("1 %", BuildText.SharePercent(1));
            Assert.Equal("73 %", BuildText.SharePercent(72.6));
        }
        finally
        {
            CultureInfo.CurrentCulture = before;
        }
    }
}
