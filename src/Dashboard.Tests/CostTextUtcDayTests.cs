using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// The one rule the two period words of a cost share: <see cref="CostText.Day" /> says <c>yesterday</c> of the UTC day
/// before the rendering, and <see cref="CostText.Month" /> says <c>this month</c> of the UTC month the rendering
/// stands in - the same day, the one <see cref="CostText.DayZone" /> names, counted the same way by both of them.
/// Nothing of either word is the viewer's own calendar date.
/// <para>
/// What <see cref="CostTests" /> and <see cref="CostTextEdgeCaseTests" /> already pin is walked past here: that
/// <c>yesterday</c> is true of exactly one day and <c>this month</c> of the current month only, and that a
/// <c>+9 h</c> (or <c>-8 h</c>) viewer still gets <c>yesterday</c> and <c>this month</c> from a fixed
/// <c>2026-10-04T22:00Z</c>. What none of them held is the pair agreeing at a moment where the viewer's date and the
/// UTC date fall in different <em>months</em> - the shape a cost takes at 00:30 in the small hours somewhere east, or
/// at 23:30 somewhere west. That is what follows: three moments fixed to the minute, each one where counting from the
/// viewer's own day would answer with a different word, and a theory that the two words never count from different
/// days over the hours around both midnights, in either direction.
/// </para>
/// <para>
/// Nothing here reads a clock or the network: every moment is brought, its offset written out in its string, and the
/// words are the ones the page prints.
/// </para>
/// </summary>
public class CostTextUtcDayTests
{
    /// <summary>The moment of a rendering, exactly as its offset was written: a <c>+02:00</c> stays a
    /// <c>+02:00</c>, so the test says what a viewer stands in rather than what the machine running it happens to.</summary>
    private static DateTimeOffset Moment(string iso) =>
        DateTimeOffset.Parse(iso, CultureInfo.InvariantCulture, DateTimeStyles.None);

    /// <summary>The issue's moment: half past midnight two hours east, which is still the evening of the 6th in UTC.
    /// The file's 5th is yesterday and its October is this month, because both are counted from the 6th.</summary>
    [Fact]
    public void AMomentPastTheViewersMidnightIsStillTheUtcDayBeforeIt()
    {
        var now = Moment("2026-10-07T00:30+02:00"); // 2026-10-06 22:30 UTC

        Assert.Equal("yesterday", CostText.Day(new DateOnly(2026, 10, 5), now));
        Assert.Equal("this month", CostText.Month(new DateOnly(2026, 10, 1), now));
    }

    /// <summary>November has begun for the viewer and not for the cost: the 31st in UTC is what both words count from,
    /// so the 30th is yesterday, October is this month, and a file that already names November is a month still to
    /// come - which is the word a viewer whose own date is the 1st of November would least expect, and the one that
    /// is true of the days the numbers cover.</summary>
    [Fact]
    public void AViewerWhoseLocalDateHasAlreadyTurnedToTheNextMonthStillSeesTheUtcMonth()
    {
        var now = Moment("2026-11-01T00:30+02:00"); // 2026-10-31 22:30 UTC

        Assert.Equal("yesterday", CostText.Day(new DateOnly(2026, 10, 30), now));
        Assert.Equal("this month", CostText.Month(new DateOnly(2026, 10, 1), now));
        Assert.Equal("in November", CostText.Month(new DateOnly(2026, 11, 1), now));
    }

    /// <summary>The other side of the same line: a viewer two hours west whose own date is still the last of
    /// September while UTC has reached the first of October. Both words follow UTC, so the 30th is yesterday and
    /// September is a month that has ended - the pair of answers a local calendar would contradict in both words at
    /// once.</summary>
    [Fact]
    public void AViewerWhoseLocalDateHasNotYetTurnedToTheNextMonthSeesTheUtcOne()
    {
        var now = Moment("2026-09-30T23:30-02:00"); // 2026-10-01 01:30 UTC

        Assert.Equal("yesterday", CostText.Day(new DateOnly(2026, 9, 30), now));
        Assert.Equal("this month", CostText.Month(new DateOnly(2026, 10, 1), now));
        Assert.Equal("in September", CostText.Month(new DateOnly(2026, 9, 1), now));
    }

    /// <summary>The two words of a whole line, at the moment the viewer's month has turned and the UTC one has not:
    /// <c>yesterday</c> and <c>this month</c> are true of the same day, and that day is the one the line's own
    /// <c>as of</c> names.</summary>
    [Fact]
    public void AWholeLineSaysBothWordsOfTheOneUtcDay()
    {
        var amounts = new CostAmounts(1.52, 9.8, 11.02);
        var report = new CostReport(null, "USD", new DateOnly(2026, 10, 30), amounts, []);

        Assert.Equal(
            "$1.52 yesterday · $9.80 in 7 days · $11.02 this month · as of 2026-10-30 (UTC)",
            CostText.Line(amounts, report, Moment("2026-11-01T00:30+02:00")));
    }

    /// <summary>The rule the two words share, over the hours on either side of both midnights and in each direction of
    /// the viewer's offset: whichever day UTC says it is, the day before it is yesterday and the first of its month is
    /// this month, and the day after is a date named by its date. The same instant written in two zones gives the
    /// same words twice.</summary>
    /// <param name="iso">The moment of the rendering, with the zone the viewer stands in.</param>
    [Theory]
    [InlineData("2026-10-06T22:30+02:00")]  // the 6th in UTC, the 7th for the viewer
    [InlineData("2026-10-06T21:30-02:00")]  // already the 7th in UTC, still the 6th for the viewer
    [InlineData("2026-10-31T23:59+00:00")]  // the last minute of October
    [InlineData("2026-11-01T00:01+00:00")]  // the first minute of November
    [InlineData("2026-11-01T01:00+02:00")]  // the first minute of November in UTC, seen from November's second hour
    [InlineData("2026-10-31T22:00-04:00")]  // October for the viewer, November's first minute in UTC
    [InlineData("2027-01-01T00:30+05:45")]  // a quarter-hour zone, the year turning for the viewer only
    public void TheTwoWordsNeverCountFromDifferentDays(string iso)
    {
        var now = Moment(iso);
        var utcDay = DateOnly.FromDateTime(now.UtcDateTime); // the day the page's days are of: DayZone, whatever the viewer carries

        Assert.Equal("yesterday", CostText.Day(utcDay.AddDays(-1), now));
        Assert.Equal("this month", CostText.Month(new DateOnly(utcDay.Year, utcDay.Month, 1), now));
        Assert.Equal($"on {CostText.Date(utcDay.AddDays(1))}", CostText.Day(utcDay.AddDays(1), now));

        // The same instant in another zone is the same instant: the words do not move with how it is written.
        var elsewhere = now.ToOffset(TimeSpan.FromHours(-11));
        Assert.Equal("yesterday", CostText.Day(utcDay.AddDays(-1), elsewhere));
        Assert.Equal("this month", CostText.Month(new DateOnly(utcDay.Year, utcDay.Month, 1), elsewhere));
    }
}
