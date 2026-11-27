namespace Dashboard.Tests;

/// <summary>
/// The words the percent of the availability line says when it says nothing or says a whole or says one
/// decimal: the dash of the window with no report, the whole number without the decimal that has nothing to
/// say, and the one decimal under a culture that writes the decimal with a comma rather than a point - the
/// three words the rounding of the share and the format it is said with must agree on, the same agreement the
/// rounding digits and the format keep elsewhere (#2021).
/// <para>
/// What is pinned where it belongs: <see cref="AvailabilityTests" /> pins the seven percents of the line and the
/// window without a report read from a file; <see cref="DeliveryReportEdgeCaseTests" /> pins the percent the
/// rounding carries to all or none and the percent of the impossible window; <see cref="AvailabilityTextEdgeCaseTests" />
/// pins the half of the second decimal, the whole the rounding carries to, the dash whatever the healthy count,
/// and the percent at the ends of the count - each under a German reader. Left open by all of that, and pinned
/// here: the dash of the empty window as the one word it is, the whole said without the trailing decimal, and
/// the one decimal under the comma of the Finnish culture - the format the percent is said with, and the
/// invariant culture it is said in, whatever the reader's own.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: the windows are the ones the
/// tests bring.
/// </para>
/// </summary>
public class AvailabilityTextPercentWordsTests
{
    // --------------------------------------------------------------- the dash of the empty window

    /// <summary>The window with no report says the dash, the one word the percent has when the count no report
    /// stands behind is no share: the same word whatever the healthy count that is not there says, and the word
    /// a reader of a culture that writes its numbers its own way still reads as the dash and not a digit.</summary>
    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(0, -3)]
    public void TheWindowWithNoReportSaysTheDash(int reports, int healthy)
    {
        Assert.Equal("–", AvailabilityText.Percent(new HealthWindow(reports, healthy)));

        ViewerCulture.German(() => Assert.Equal("–", AvailabilityText.Percent(new HealthWindow(reports, healthy))));
    }

    // ------------------------------------------------------- the whole, without the decimal that has nothing to say

    /// <summary>A whole percent is said as the whole: the hundred at the top of the count, the ninety-nine point
    /// nine the rounding of the one fewer report would not cross onto, and the zero the rounding of the one report
    /// in the whole never says as none - each of them without the trailing decimal the format leaves out when it
    /// has nothing to say, and with the space of the unit that separates the word from the words around it.</summary>
    [Theory]
    [InlineData(24, 24, "100 %")]
    [InlineData(100, 99, "99 %")]
    [InlineData(2000, 1, "0.1 %")]
    public void TheWholeIsSaidWithoutTheTrailingDecimal(int reports, int healthy, string expected) =>
        Assert.Equal(expected, AvailabilityText.Percent(new HealthWindow(reports, healthy)));

    /// <summary>One decimal is said as one decimal under a culture that writes the decimal with a comma rather
    /// than a point: the Finnish reader of the dashboard's words sees the point the invariant culture writes and
    /// not the comma the reader's own numbers take, and the tenth of the percent is the tenth the rounding
    /// decided and nothing the reader's culture decides.</summary>
    [Fact]
    public void TheOneDecimalIsSaidWithThePointUnderACultureThatWritesTheComma()
    {
        CultureScope.Runs("fi-FI", () =>
        {
            Assert.Equal("95.8 %", AvailabilityText.Percent(new HealthWindow(24, 23)));
            Assert.Equal("6.2 %", AvailabilityText.Percent(new HealthWindow(16, 1)));
            Assert.Equal("0.1 %", AvailabilityText.Percent(new HealthWindow(1000, 1)));
            Assert.Equal("–", AvailabilityText.Percent(new HealthWindow(0, 0)));
        });
    }
}
