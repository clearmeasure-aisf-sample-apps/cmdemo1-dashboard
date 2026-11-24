namespace Dashboard.Tests;

/// <summary>
/// Every branch of <see cref="AvailabilityText" /> that carries a window word keeps saying <c>24 hours</c> and
/// <c>7 days</c> byte for byte, now that the words have one private name each (#1663).
/// </summary>
public class AvailabilityTextWindowNamesTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 24, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void TheDayAloneWithNoReportEndedSays24Hours()
    {
        var parts = AvailabilityText.Parts(new HealthReports(new HealthWindow(0, 0), null, null), Now);
        Assert.Equal(new AvailabilityPart("No hourly check ended in ", "24 hours", string.Empty), Assert.Single(parts));
    }

    [Fact]
    public void TheWeekAloneWithNoReportEndedSays7Days()
    {
        var parts = AvailabilityText.Parts(new HealthReports(null, new HealthWindow(0, 0), null), Now);
        Assert.Equal(new AvailabilityPart("No hourly check ended in ", "7 days", string.Empty), Assert.Single(parts));
    }

    [Fact]
    public void TheWeekAloneWithReportsEndsIn7Days()
    {
        var parts = AvailabilityText.Parts(new HealthReports(null, new HealthWindow(24, 23), null), Now);
        Assert.EndsWith(" in 7 days", parts[0].After, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAllHealthyWeekWithNoFailureSaysNoFailureIn7Days()
    {
        var parts = AvailabilityText.Parts(new HealthReports(null, new HealthWindow(2, 2), null), Now);
        Assert.Equal(new AvailabilityPart("no failure in ", "7 days", string.Empty), parts[^1]);
    }

    [Fact]
    public void TheDayAndTheWeekWithNoFailureJoinToTheWholeLine() =>
        Assert.Equal(
            "Healthy in 2 of 2 hourly checks (100 %) in 24 hours · 2 of 2 in 7 days · no failure in 7 days",
            AvailabilityText.Line(new HealthReports(new HealthWindow(2, 2), new HealthWindow(2, 2), null), Now));
}
