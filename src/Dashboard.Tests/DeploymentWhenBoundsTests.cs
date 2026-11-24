namespace Dashboard.Tests;

/// <summary>
/// The day bounds of <see cref="DeploymentActivity.When" />: the time of day on the same calendar day, the day of the
/// week below seven calendar days, then the date. The bounds count calendar dates in the zone given, not spans of
/// twenty-four hours, so they hold across a change of daylight saving time too.
/// <para>Nothing here asks a clock what time it is: every moment is one the test brings.</para>
/// </summary>
public class DeploymentWhenBoundsTests
{
    /// <summary>Friday midnight, UTC.</summary>
    private static readonly DateTimeOffset Midnight = new(2026, 10, 9, 0, 0, 0, TimeSpan.Zero);

    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    private static readonly TimeZoneInfo Chicago = TimeZoneInfo.FindSystemTimeZoneById("America/Chicago");

    /// <summary>Daylight time in Chicago: five hours behind UTC.</summary>
    private static readonly TimeSpan Cdt = TimeSpan.FromHours(-5);

    /// <summary>Standard time in Chicago: six hours behind UTC.</summary>
    private static readonly TimeSpan Cst = TimeSpan.FromHours(-6);

    [Fact]
    public void AMomentLaterTheSameDaySaysTheTimeOfDay() =>
        Assert.Equal("23:59", DeploymentActivity.When(Midnight.AddHours(23).AddMinutes(59), Midnight, Utc));

    [Fact]
    public void AMomentOneMinuteIntoTheNextDaySaysTheWeekday() =>
        Assert.Equal("Sat 00:01", DeploymentActivity.When(Midnight.AddDays(1).AddMinutes(1), Midnight.AddHours(23).AddMinutes(59), Utc));

    [Fact]
    public void AMomentOneMinuteIntoThePreviousDaySaysTheWeekday() =>
        Assert.Equal("Thu 23:59", DeploymentActivity.When(Midnight.AddMinutes(-1), Midnight, Utc));

    [Fact]
    public void AMomentSixDaysAndTwentyThreeHoursAwaySaysTheWeekday() =>
        Assert.Equal("Thu 23:00", DeploymentActivity.When(Midnight.AddDays(6).AddHours(23), Midnight, Utc));

    [Fact]
    public void AMomentSevenCalendarDaysAwaySaysTheDate() =>
        Assert.Equal("16 Oct 00:01", DeploymentActivity.When(Midnight.AddDays(7).AddMinutes(1), Midnight.AddHours(23), Utc));

    /// <summary>1 November 2026 in Chicago has twenty-five hours; its last minute is still the same day.</summary>
    [Fact]
    public void TheLongDayOfTheChangeInChicagoSaysTheTimeOfDayToItsLastMinute()
    {
        var now = new DateTimeOffset(2026, 11, 1, 0, 0, 0, Cdt);
        var last = new DateTimeOffset(2026, 11, 1, 23, 59, 0, Cst);

        Assert.Equal("23:59", DeploymentActivity.When(last, now, Chicago));
    }

    [Fact]
    public void AMomentOneMinuteIntoTheDayOfTheChangeInChicagoSaysTheWeekday()
    {
        var now = new DateTimeOffset(2026, 10, 31, 23, 59, 0, Cdt);
        var moment = new DateTimeOffset(2026, 11, 1, 0, 1, 0, Cdt);

        Assert.Equal("Sun 00:01", DeploymentActivity.When(moment, now, Chicago));
    }

    /// <summary>Seven times twenty-four hours across the change, yet six calendar days in Chicago: still the weekday.</summary>
    [Fact]
    public void AMomentSixCalendarDaysAwayAcrossTheChangeInChicagoSaysTheWeekday()
    {
        var now = new DateTimeOffset(2026, 10, 31, 0, 0, 0, Cdt);
        var moment = new DateTimeOffset(2026, 11, 6, 23, 0, 0, Cst);

        Assert.Equal(TimeSpan.FromDays(7), moment - now);
        Assert.Equal("Fri 23:00", DeploymentActivity.When(moment, now, Chicago));
    }

    /// <summary>Fewer than seven times twenty-four hours across the change, yet seven calendar days in Chicago: the date.</summary>
    [Fact]
    public void AMomentSevenCalendarDaysAwayAcrossTheChangeInChicagoSaysTheDate()
    {
        var now = new DateTimeOffset(2026, 10, 31, 23, 0, 0, Cdt);
        var moment = new DateTimeOffset(2026, 11, 7, 0, 30, 0, Cst);

        Assert.True(moment - now < TimeSpan.FromDays(7));
        Assert.Equal("7 Nov 00:30", DeploymentActivity.When(moment, now, Chicago));
    }
}
