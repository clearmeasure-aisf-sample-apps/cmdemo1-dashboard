namespace Dashboard.Tests;

/// <summary>
/// The named part words of <see cref="AvailabilityText" />: the line's part separator and the lead words the parts
/// hang their numbers after, held in the text the line says rather than in the const's name alone.
/// </summary>
public class AvailabilityTextPartWordsTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 24, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void TheLineSaysThePartsJoinedByTheDotWithTheLeadWords()
    {
        var reports = new HealthReports(
            new HealthWindow(24, 23),
            new HealthWindow(166, 164),
            Now.AddHours(-9));

        Assert.Equal("Healthy in 23 of 24 hourly checks (95.8 %) in 24 hours · 164 of 166 in 7 days · last failure 9 h ago",
            AvailabilityText.Line(reports, Now));
    }

    [Fact]
    public void AHealthyWeekAloneEndsWithNoFailureInTheWeek()
    {
        var reports = new HealthReports(null, new HealthWindow(168, 168), null);

        var parts = AvailabilityText.Parts(reports, Now);

        Assert.Equal(new AvailabilityPart("Healthy in ", "168 of 168", " hourly checks (100 %) in 7 days"), parts[0]);
        Assert.Equal(new AvailabilityPart("no failure in ", "7 days", string.Empty), parts[1]);
    }

    [Fact]
    public void AWeekWithNoEndedReportAfterTheDaySaysNoneInTheWeek()
    {
        var reports = new HealthReports(new HealthWindow(24, 24), new HealthWindow(0, 0), null);

        Assert.Equal(new AvailabilityPart(string.Empty, "none", " in 7 days"), AvailabilityText.Parts(reports, Now)[1]);
    }

    [Fact]
    public void ADayWithNoEndedReportSaysNoCheckEndedInTheDay()
    {
        var reports = new HealthReports(new HealthWindow(0, 0), null, null);

        Assert.Equal(new AvailabilityPart("No hourly check ended in ", "24 hours", string.Empty), AvailabilityText.Parts(reports, Now).Single());
    }
}
