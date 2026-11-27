using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// The three words of <see cref="AvailabilityText.Line" /> that were once bare literals - the part separator, the
/// last-failure lead and the no-failure lead - pinned to the letter in the line and in the parts, so that a
/// re-spelling of any of them is seen here.
/// </summary>
public class AvailabilityTextLineWordsTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 7, 1, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Failed = new(2026, 10, 6, 16, 13, 0, TimeSpan.Zero);

    /// <summary>A day + week + last failure <see cref="HealthReports" /> gives the full line with the separator
    /// and the last-failure lead, using <see cref="TimeText.Ago" /> as the oracle for the moment.</summary>
    [Fact]
    public void TheDayWeekAndLastFailureSayTheFullLineWithSeparatorAndLead()
    {
        var reports = new HealthReports(new HealthWindow(24, 23), new HealthWindow(166, 164), Failed);

        Assert.Equal(
            "Healthy in 23 of 24 hourly checks (95.8 %) in 24 hours · 164 of 166 in 7 days · last failure 9 h ago",
            AvailabilityText.Line(reports, Now));
    }

    /// <summary>A week with all healthy and no failure gives a last part of
    /// <c>Before == "no failure in "</c>, <c>Strong == "7 days"</c>, <c>After == ""</c>.</summary>
    [Fact]
    public void TheAllHealthyWeekSaysNoFailureWithTheNamedLead()
    {
        var reports = new HealthReports(new HealthWindow(1, 1), new HealthWindow(168, 168), null);
        var parts = AvailabilityText.Parts(reports, Now);

        var last = parts[^1];

        Assert.Equal("no failure in ", last.Before);
        Assert.Equal("7 days", last.Strong);
        Assert.Equal(string.Empty, last.After);
        Assert.Null(last.Title);
        Assert.Null(last.When);
    }

    /// <summary>The last-failure part has <c>Before == "last failure "</c>, <c>When == failed</c> and no
    /// <c>Title</c>.</summary>
    [Fact]
    public void TheLastFailurePartHasTheLeadTheMomentAndNoTitle()
    {
        var reports = new HealthReports(new HealthWindow(24, 23), new HealthWindow(166, 164), Failed);
        var parts = AvailabilityText.Parts(reports, Now);

        var last = parts[^1];

        Assert.Equal("last failure ", last.Before);
        Assert.Equal(TimeText.Ago(Failed, Now), last.Strong);
        Assert.Equal(string.Empty, last.After);
        Assert.Null(last.Title);
        Assert.Equal(Failed, last.When);
    }

    /// <summary>A single part gives a line with no separator.</summary>
    [Fact]
    public void ASinglePartGivesALineWithNoSeparator()
    {
        var reports = new HealthReports(new HealthWindow(24, 23), null, null);

        Assert.Equal("Healthy in 23 of 24 hourly checks (95.8 %) in 24 hours", AvailabilityText.Line(reports, Now));
    }
}
