using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="AvailabilityText" /> - the words of the
/// availability line of the "Delivery" card, at the ends the tests of the words as a whole leave open.
/// <para>
/// What is pinned where it belongs: <see cref="AvailabilityTests" /> pins the line and the parts of a day and a
/// week read from a file, the parts with their tooltip and their moment, the window without a report, the seven
/// percents, and the note beside the numbers; <see cref="DeliveryReportEdgeCaseTests" /> pins the percent the
/// rounding would carry to all or none, the window the file could not give, the week counted alone and all
/// healthy, the failure said with no window behind it, the tooltip in the hours of a zone five and a half east of
/// the meridian, and the tooltip, the parts and the percent refusing what is not there;
/// <see cref="HealthHelperCoverageTests" /> pins the count to the letter - the two orders, the invariant point,
/// the window that is not there; and <c>AvailabilityLineTests</c> in <c>SharedCostAvailabilityTests.cs</c> pins
/// the note, the parts and the tooltip where the line component says them.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the tooltip without a moment, word for word, and the moment it is
/// given written five hours west of the meridian, where the instant falls on the day before; the day counted
/// alone, in its own words and with its zero reports, and the claim of no failure the day alone is not entitled
/// to; the week counted alone that is not all healthy, in the one report and in the whole, without the tooltip
/// the day before it would take; the day without a report taking the short week with the claim of no failure, and
/// the last failure said instead of the claim while the week is all healthy; the failure at the two changes of
/// unit - the minute and the two days; the line without a part and the line refusing reports that are not there
/// by the name the guard keeps; the count at the two ends of <see cref="int" /> and with the signs no file gives;
/// the percent of the half that rounds to the even tenth, the percent the rounding carries to a whole below the
/// ninety-nine point nine it never crosses onto, the dash of the window with no reports whatever the healthy
/// count, and the percent at the ends of the count - each of them the same for a reader of German; and the
/// failure of the last second of its minute still <c>just now</c>, and the forty-eighth hour the second day.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every moment is one the test
/// brings. The note beside the numbers has its fixed words pinned where the line says it, so it is walked past
/// here.
/// </para>
/// </summary>
public class AvailabilityTextEdgeCaseTests
{
    /// <summary>The moment the page is asked, and every "ago" below is counted from: a fixed moment, not the clock.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 7, 1, 30, 0, TimeSpan.Zero);

    /// <summary>A moment nine hours and seventeen minutes before it: <c>9 h ago</c>, and not a whole number of hours.</summary>
    private static readonly DateTimeOffset Failed = new(2026, 10, 6, 16, 13, 0, TimeSpan.Zero);

    /// <summary>The moment the delivery facts last changed, in the hours the file writes them in.</summary>
    private static readonly DateTimeOffset Generated = new(2026, 10, 7, 1, 0, 0, TimeSpan.Zero);

    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    private static readonly TimeZoneInfo WestFive =
        TimeZoneInfo.CreateCustomTimeZone("west-five", TimeSpan.FromHours(-5), "West Five", "West Five");

    /// <summary>Who counts and what a check is: the sentence the tooltip says with and without a moment.</summary>
    private const string What =
        "Counted by the pipeline: once an hour the runbook \"Health report\" in Octopus Deploy asks every node of "
        + "the environment and its public address, and the report fails when one is not healthy. It is no continuous "
        + "monitoring: an outage between two reports is not counted.";

    // ---------------------------------------------------------------- the tooltip, word for word

    /// <summary>The tooltip without a moment is the sentence that says who counts, word for word, and the zone
    /// it was asked with changes nothing while no moment is written in it.</summary>
    [Fact]
    public void TheTooltipWithoutAMomentIsTheSentenceThatSaysWhoCounts()
    {
        Assert.Equal(What, AvailabilityText.Help(null, Utc));
        Assert.Equal(What, AvailabilityText.Help(null, WestFive));
    }

    /// <summary>The tooltip with a moment says the change of the delivery facts word for word, in the hours of
    /// the zone it is given: on the meridian the moment stands as it is written, and five hours west of it the
    /// same instant is the evening of the day before.</summary>
    [Fact]
    public void TheTooltipSaysTheChangeOfTheFactsWordForWordInTheHoursOfTheZoneGiven()
    {
        Assert.Equal(
            What + " As of the last change of the delivery facts: 2026-10-07 01:00 +00:00.",
            AvailabilityText.Help(Generated, Utc));
        Assert.Equal(
            What + " As of the last change of the delivery facts: 2026-10-06 20:00 -05:00.",
            AvailabilityText.Help(Generated, WestFive));
    }

    /// <summary>The tooltip refuses a zone that is not there by the name the guard keeps, while no moment is
    /// written in it: the sentence that says who counts still needs the zone it would write the moment in.</summary>
    [Fact]
    public void TheTooltipWithoutAMomentStillRefusesAZoneThatIsNotThere() =>
        Assert.Equal("zone", Assert.Throws<ArgumentNullException>(() => AvailabilityText.Help(null, null!)).ParamName);

    // ------------------------------------------------------------ the day, counted alone

    /// <summary>The day counted alone says what it counted in its own words - the long form the week takes
    /// after a day it takes before, with the count in the middle that carries it - and names no failure, since
    /// the week is what the claim of no failure in seven days covers: the one that is all healthy as the one
    /// that found nothing healthy of the reports it counted.</summary>
    [Fact]
    public void TheDayCountedAloneSaysWhatItCountedInItsOwnWords()
    {
        var day = new HealthReports(new HealthWindow(24, 23), null, null);

        Assert.Equal(
            [new AvailabilityPart("Healthy in ", "23 of 24", " hourly checks (95.8 %) in 24 hours")],
            AvailabilityText.Parts(day, Now));
        Assert.Equal("Healthy in 23 of 24 hourly checks (95.8 %) in 24 hours", AvailabilityText.Line(day, Now));

        var allHealthy = new HealthReports(new HealthWindow(24, 24), null, null);

        Assert.Equal(
            [new AvailabilityPart("Healthy in ", "24 of 24", " hourly checks (100 %) in 24 hours")],
            AvailabilityText.Parts(allHealthy, Now));

        // None passed: the count is said as it stands, with the percent of the none.
        Assert.Equal(
            "Healthy in 0 of 24 hourly checks (0 %) in 24 hours",
            AvailabilityText.Line(new HealthReports(new HealthWindow(24, 0), null, null), Now));
    }

    /// <summary>The day that counted no report says so in its own words, and claims nothing: the part is the
    /// words and the empty count, the line is that part and nothing after it, and the claim of no failure is
    /// not made on the strength of a day nobody checked.</summary>
    [Fact]
    public void TheDayThatCountedNoReportSaysSoAndClaimsNothing()
    {
        var day = new HealthReports(new HealthWindow(0, 0), null, null);

        Assert.Equal(
            [new AvailabilityPart("No hourly check ended in ", "24 hours", string.Empty)],
            AvailabilityText.Parts(day, Now));
        Assert.Equal("No hourly check ended in 24 hours", AvailabilityText.Line(day, Now));
    }

    // ------------------------------------------------------------ the week, counted alone

    /// <summary>The week counted alone and not all healthy says what it counted in its own words - the long
    /// form it takes without a day before it, in the one report and in the whole - and carries no tooltip,
    /// since the tooltip is the percent of the week the day part has already said.</summary>
    [Fact]
    public void TheWeekCountedAloneAndNotAllHealthySaysWhatItCounted()
    {
        var week = new HealthReports(null, new HealthWindow(168, 167), null);

        Assert.Equal(
            [new AvailabilityPart("Healthy in ", "167 of 168", " hourly checks (99.4 %) in 7 days")],
            AvailabilityText.Parts(week, Now));
        Assert.Equal("Healthy in 167 of 168 hourly checks (99.4 %) in 7 days", AvailabilityText.Line(week, Now));

        var one = new HealthReports(null, new HealthWindow(1, 0), null);

        Assert.Equal(
            [new AvailabilityPart("Healthy in ", "0 of 1", " hourly check (0 %) in 7 days")],
            AvailabilityText.Parts(one, Now));
        Assert.Equal("Healthy in 0 of 1 hourly check (0 %) in 7 days", AvailabilityText.Line(one, Now));
    }

    /// <summary>The week counted alone and all healthy, in the one report, says the one check and the claim of
    /// no failure it is entitled to: the singular the count takes in the one, and the claim the week alone
    /// makes - which the day alone is not entitled to.</summary>
    [Fact]
    public void TheWeekCountedAloneAndAllHealthyInTheOneReportSaysTheClaim()
    {
        var week = new HealthReports(null, new HealthWindow(1, 1), null);

        Assert.Equal(
            [
                new AvailabilityPart("Healthy in ", "1 of 1", " hourly check (100 %) in 7 days"),
                new AvailabilityPart("no failure in ", "7 days", string.Empty),
            ],
            AvailabilityText.Parts(week, Now));
        Assert.Equal("Healthy in 1 of 1 hourly check (100 %) in 7 days · no failure in 7 days", AvailabilityText.Line(week, Now));
    }

    // -------------------------------------------------- the day and the week, at the ends of what is counted

    /// <summary>The day without a report takes the short week behind it, with the tooltip the week alone
    /// carries not, and the claim of no failure the all-healthy week is entitled to: the three parts in the
    /// order the line says them, each in the words it takes with a day before it.</summary>
    [Fact]
    public void TheDayWithoutAReportTakesTheShortWeekAndTheClaimOfNoFailure()
    {
        var reports = new HealthReports(new HealthWindow(0, 0), new HealthWindow(168, 168), null);

        Assert.Equal(
            [
                new AvailabilityPart("No hourly check ended in ", "24 hours", string.Empty),
                new AvailabilityPart(string.Empty, "168 of 168", " in 7 days", "100 % of the hourly checks of the last 7 days"),
                new AvailabilityPart("no failure in ", "7 days", string.Empty),
            ],
            AvailabilityText.Parts(reports, Now));
        Assert.Equal(
            "No hourly check ended in 24 hours · 168 of 168 in 7 days · no failure in 7 days",
            AvailabilityText.Line(reports, Now));
    }

    /// <summary>The last failure is said instead of the claim while the week is all healthy: the failure the
    /// report names is the part the line ends with, and the claim of no failure in seven days is not made on
    /// the strength of the week while a report of it failed.</summary>
    [Fact]
    public void TheLastFailureIsSaidInsteadOfTheClaimWhileTheWeekIsAllHealthy()
    {
        var reports = new HealthReports(new HealthWindow(24, 24), new HealthWindow(168, 168), Failed);

        Assert.Equal(
            [
                new AvailabilityPart("Healthy in ", "24 of 24", " hourly checks (100 %) in 24 hours"),
                new AvailabilityPart(string.Empty, "168 of 168", " in 7 days", "100 % of the hourly checks of the last 7 days"),
                new AvailabilityPart("last failure ", "9 h ago", string.Empty, When: Failed),
            ],
            AvailabilityText.Parts(reports, Now));
        Assert.Equal(
            "Healthy in 24 of 24 hourly checks (100 %) in 24 hours · 168 of 168 in 7 days · last failure 9 h ago",
            AvailabilityText.Line(reports, Now));
    }

    // -------------------------------------------------------- the last failure, at the ends of its units

    /// <summary>The last failure is said in whole units at the changes of unit the count makes: the failure
    /// of the last second of its first minute is still <c>just now</c>, the forty-seventh hour is still an hour
    /// while the forty-eighth is the second day.</summary>
    [Fact]
    public void TheLastFailureIsSaidInWholeUnitsAtTheChangesOfUnit()
    {
        Assert.Equal("last failure just now", AvailabilityText.Line(new HealthReports(null, null, Now.AddSeconds(-59)), Now));
        Assert.Equal("last failure 2 h ago", AvailabilityText.Line(new HealthReports(null, null, Now.AddHours(-2)), Now));
        Assert.Equal("last failure 47 h ago", AvailabilityText.Line(new HealthReports(null, null, Now.AddHours(-47)), Now));
        Assert.Equal("last failure 2 d ago", AvailabilityText.Line(new HealthReports(null, null, Now.AddHours(-48)), Now));
    }

    // ------------------------------------------------------------ the line, and the line that is not there

    /// <summary>The line without a part is no line: the parts are empty and the line that joins them is the
    /// empty text the page leaves out rather than a dot of nothing.</summary>
    [Fact]
    public void TheLineWithoutAPartIsNoLine() =>
        Assert.Equal(string.Empty, AvailabilityText.Line(new HealthReports(null, null, null), Now));

    /// <summary>The line refuses reports that are not there, by the name the guard keeps: the parts the line
    /// is joined out of are not there either, and the line does not say the empty words of the none.</summary>
    [Fact]
    public void TheLineRefusesReportsThatAreNotThere() =>
        Assert.Equal("reports", Assert.Throws<ArgumentNullException>(() => AvailabilityText.Line(null!, Now)).ParamName);

    // ------------------------------------------------------------ the count, at the ends of a number

    /// <summary>The count refuses a window that is not there, by the name the guard keeps.</summary>
    [Fact]
    public void TheCountRefusesAWindowThatIsNotThere() =>
        Assert.Equal("window", Assert.Throws<ArgumentNullException>(() => AvailabilityText.Count(null!)).ParamName);

    /// <summary>The two counts are said at the ends of what a number may be, and with the signs the counts no
    /// file gives: the healthy count first and the reports second, at either end of <see cref="int" /> and in
    /// either of the two orders, and negative alike - the numbers as they stand, with no thousands separator a
    /// viewer's own formatting would add, the same for a reader of German.</summary>
    [Fact]
    public void TheCountSaysTheTwoCountsAtTheEndsOfANumber()
    {
        Assert.Equal("-2147483648 of 2147483647", AvailabilityText.Count(new HealthWindow(int.MaxValue, int.MinValue)));
        Assert.Equal("2147483647 of -2147483648", AvailabilityText.Count(new HealthWindow(int.MinValue, int.MaxValue)));
        Assert.Equal("-2 of -5", AvailabilityText.Count(new HealthWindow(-5, -2)));

        ViewerCulture.German(() =>
            Assert.Equal("-2147483648 of 2147483647", AvailabilityText.Count(new HealthWindow(int.MaxValue, int.MinValue))));
    }

    // ------------------------------------------------------------ the percent, at the ends of the rounding

    /// <summary>The half that lands on the second decimal is rounded to the even tenth - the <c>5</c> of
    /// <c>6.25</c> going to the <c>2</c> and not the <c>3</c>, the <c>5</c> of <c>43.75</c> going to the
    /// <c>8</c> - the rounding the invariant culture makes and nothing else deciding it.</summary>
    [Theory]
    [InlineData(16, 1, "6.2 %")]
    [InlineData(16, 3, "18.8 %")]
    [InlineData(16, 7, "43.8 %")]
    [InlineData(16, 9, "56.2 %")]
    [InlineData(16, 11, "68.8 %")]
    [InlineData(16, 13, "81.2 %")]
    public void TheHalfOfTheSecondDecimalRoundsToTheEvenTenth(int reports, int healthy, string expected) =>
        Assert.Equal(expected, AvailabilityText.Percent(new HealthWindow(reports, healthy)));

    /// <summary>The percent the rounding carries to a whole below the ninety-nine point nine is said as the
    /// whole: the hundredth of a whole that is not the whole stays under the cap that keeps it from being said
    /// as all, and the quarter, the three quarters and the seven tenths are said without the decimal that has
    /// nothing to say.</summary>
    [Theory]
    [InlineData(100, 99, "99 %")]
    [InlineData(99, 98, "99 %")]
    [InlineData(4, 1, "25 %")]
    [InlineData(4, 3, "75 %")]
    [InlineData(10, 7, "70 %")]
    public void ThePercentTheRoundingCarriesToAWholeIsSaidAsTheWhole(int reports, int healthy, string expected) =>
        Assert.Equal(expected, AvailabilityText.Percent(new HealthWindow(reports, healthy)));

    /// <summary>The window with no reports is the dash whatever the healthy count says: the count no report
    /// stands behind is no share, and the healthy count that is not there, that is over the reports or that is
    /// negative alike leaves the dash standing.</summary>
    [Theory]
    [InlineData(0, 1)]
    [InlineData(0, 5)]
    [InlineData(0, -3)]
    public void TheWindowWithNoReportsIsTheDashWhateverTheHealthyCount(int reports, int healthy) =>
        Assert.Equal("–", AvailabilityText.Percent(new HealthWindow(reports, healthy)));

    /// <summary>The percent at the ends of the count keeps the rules it keeps at the small numbers: the whole
    /// at the very top of the count, the ninety-nine point nine the one fewer report would round onto, and the
    /// tenth of a percent the one report in the whole never says as none - the same for a reader of German,
    /// with the point the invariant culture writes and not the comma.</summary>
    [Fact]
    public void ThePercentKeepsTheRulesAtTheEndsOfACount()
    {
        Assert.Equal("100 %", AvailabilityText.Percent(new HealthWindow(int.MaxValue, int.MaxValue)));
        Assert.Equal("99.9 %", AvailabilityText.Percent(new HealthWindow(int.MaxValue, int.MaxValue - 1)));
        Assert.Equal("0.1 %", AvailabilityText.Percent(new HealthWindow(int.MaxValue, 1)));

        ViewerCulture.German(() =>
        {
            Assert.Equal("6.2 %", AvailabilityText.Percent(new HealthWindow(16, 1)));
            Assert.Equal("99.9 %", AvailabilityText.Percent(new HealthWindow(int.MaxValue, int.MaxValue - 1)));
        });
    }

    // ------------------------------------------------------------------ the class's own shape

    /// <summary>The whole public surface of the words of the availability line, as it stands: the note beside
    /// the numbers, and the five sayers - the tooltip, the parts, the line, the count and the percent the
    /// parts are said in - and nothing else to call. A member added to the class has to be written here as
    /// well, which is the point: the page cannot be shown a word nobody decided what it means.</summary>
    [Fact]
    public void TheSurfaceIsTheNoteAndTheFiveSayers() =>
        Assert.Equal(
            ["Count", "Help", "Line", "Note", "Parts", "Percent"],
            PublicSurface());

    /// <summary>The public members the class declares itself: the note as the constant it is and the five
    /// static methods, in the order the names are said, and nothing else to call.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(AvailabilityText)
            .GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is FieldInfo || member is MethodInfo method && !method.IsSpecialName)
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
