namespace Dashboard.Tests;

/// <summary>
/// The two windows of <see cref="AvailabilityText.Parts" /> say the same words with only the window changed, and
/// <see cref="DeliveryText.LeadTime" /> says the numbers no comparison holds in days (#618).
/// </summary>
public class AvailabilityTextWindowWordsTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 24, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(1, 0)]
    [InlineData(24, 23)]
    public void TheDayAloneAndTheWeekAloneSayTheSameWordsButTheWindow(int reports, int healthy)
    {
        var window = new HealthWindow(reports, healthy);
        var day = AvailabilityText.Parts(new HealthReports(window, null, null), Now).Single();
        var week = AvailabilityText.Parts(new HealthReports(null, window, null), Now)[0];

        Assert.Equal(day.Before, week.Before);
        Assert.Equal(day.Title, week.Title);
        Assert.Equal(day.When, week.When);
        Assert.Equal(day.Strong.Replace("24 hours", "7 days", StringComparison.Ordinal), week.Strong);
        Assert.Equal(day.After.Replace("24 hours", "7 days", StringComparison.Ordinal), week.After);
    }

    [Fact]
    public void TheWeekAfterTheDayIsShortWhetherOrNotAReportEnded()
    {
        var none = AvailabilityText.Parts(new HealthReports(new HealthWindow(1, 1), new HealthWindow(0, 0), null), Now);
        Assert.Equal(new AvailabilityPart(string.Empty, "none", " in 7 days"), none[1]);

        var one = AvailabilityText.Parts(new HealthReports(new HealthWindow(0, 0), new HealthWindow(1, 1), null), Now);
        Assert.Equal(new AvailabilityPart(string.Empty, "1 of 1", " in 7 days", "100 % of the hourly checks of the last 7 days"), one[1]);
    }

    [Theory]
    [InlineData(double.NaN, "NaN d")]
    [InlineData(double.PositiveInfinity, "Infinity d")]
    [InlineData(0.9999, "60 min")]
    [InlineData(47.99, "48 h")]
    public void TheLeadTimeKeepsItsBoundsWhereTheyWere(double hours, string expected) =>
        Assert.Equal(expected, DeliveryText.LeadTime(hours));
}
