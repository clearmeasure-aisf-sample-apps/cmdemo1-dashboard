using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="TimeText" /> is the whole of the dashboard's talk about a moment: the clock a viewer reads
/// (<c>15:04:05</c>), the same moment with its day and its distance from UTC
/// (<c>2026-10-04 17:00 -05:00</c>), the machine-readable instant of a <c>&lt;time&gt;</c> element
/// (<c>2026-10-04T22:00:05Z</c>), a length of time in one whole unit (<c>3 min</c>) and the age of a moment
/// (<c>just now</c>, <c>5 h ago</c>). Two of the five take a time zone, which makes the zone the one argument of
/// the class that can be absent, that can stand on half an hour, that can keep a daylight rule of its own and that
/// can be too far west or too far east to hold the moment at all.
/// <para>
/// The thresholds and the ordinary readings are pinned where they were found first and are not moved here:
/// <see cref="TimeTextHelperTests" /> and <see cref="EdgeCaseTextTests" /> pin every unit change of
/// <c>TimeText.Span</c> - seconds to sixty, minutes to two hours, hours to two days - the clamp of a negative
/// length, the ends of <see cref="TimeSpan" /> and the fraction that is dropped instead of rounded;
/// <see cref="EdgeCaseTextTests" /> and <see cref="DeliveryTests" /> pin the minute <c>TimeText.Ago</c> turns on
/// and the "just now" of a moment ahead of the clock; <see cref="TextTests" />,
/// <see cref="EdgeCaseTextTests" />, <see cref="DeployableFactsTests" /> and
/// <see cref="TargetTileClusterCardEnvironmentDeliveryTests" /> pin the clock, the dated line and the instant in
/// UTC, in a zone five hours west and in one five and a half hours east, at the edges of a day and of a year, and
/// from every offset the file may hand over.
/// </para>
/// <para>
/// What follows is the edge those leave standing. First the zone: the one argument of the class a caller may not
/// have, and the zones nobody has written a test in yet - one that stands on half an hour, whose minutes move with
/// the hour and whose offset is written with them; one the moment arrived in already, whose own offset is not what
/// the viewer is shown; and one that keeps a daylight rule, where the hour the zone forgets in March is said in one
/// step and the hour it lives twice in October is told apart by its offset and by the machine-readable instant
/// rather than by the clock. Then the two ends of the calendar, where a zone too far west for the first moment or
/// too far east for the last cannot hold it and the moment is said where it stands. Then the fraction of a second
/// at the top of an hour and at the end of a day, and the four words of a moment as a viewer whose culture writes
/// them otherwise reads them. Then the thresholds again at the smallest step a <see cref="TimeSpan" /> has: one
/// tick under a minute, two hours and two days is still said in the unit below, and one tick over is not - the
/// whole numbers the page prints are the whole numbers a drifting clock cannot move. Then the age of the whole
/// calendar, and the age of two moments an hour apart on a day whose clock moved two. Then the five members
/// themselves, so that a sixth way of saying a moment has to be written here as well.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every moment is one the test brought,
/// and every zone is one the test made with <c>TimeZoneInfo.CreateCustomTimeZone</c>, so no machine's time zone
/// database decides an expected value. No member of the class has a null or an empty argument besides the zone - the
/// moments and the length are values, and the smallest of each, at the top of this file and in
/// <see cref="EdgeCaseTextTests" />, is what "absent" would have to mean for them.
/// </para>
/// </summary>
public class TimeTextEdgeCaseTests
{
    /// <summary>The moment this rendering happens, and the instant every age below is counted from.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>A zone five hours west, whole hours and named nothing the machine knows.</summary>
    private static readonly TimeZoneInfo WestFive = TimeZoneInfo.CreateCustomTimeZone("west-five", TimeSpan.FromHours(-5), "West Five", "West Five");

    /// <summary>A zone five hours east: the other side of the same instant.</summary>
    private static readonly TimeZoneInfo EastFive = TimeZoneInfo.CreateCustomTimeZone("east-five", TimeSpan.FromHours(5), "East Five", "East Five");

    /// <summary>An hour east of UTC that keeps a daylight rule of its own - see <see cref="SummerZone" />.</summary>
    private static readonly TimeZoneInfo Summer = SummerZone();

    // --------------------------------------------------------------------------------- the zone a caller has not got

    /// <summary>The zone is the one argument of the class that can be absent, and both members that take it refuse
    /// it by the name they pass it under - the name of the helper they hand it to rather than their own, which is
    /// what a caller of <c>TimeText.Clock</c> reads and has to recognise. No other member can be refused anything:
    /// a moment and a length are values, and a moment that is all zeroes is a moment, not nothing.</summary>
    [Fact]
    public void AZoneThatIsNotThereIsRefusedByName()
    {
        var clock = Assert.Throws<ArgumentNullException>(() => TimeText.Clock(Now, null!));
        var dateAndTime = Assert.Throws<ArgumentNullException>(() => TimeText.DateAndTime(Now, null!));

        Assert.Equal("destinationTimeZone", clock.ParamName);
        Assert.Equal("destinationTimeZone", dateAndTime.ParamName);

        // The two ends of the calendar are refused a zone in the same words as an ordinary moment.
        Assert.Equal("destinationTimeZone", Assert.Throws<ArgumentNullException>(() => TimeText.Clock(DateTimeOffset.MinValue, null!)).ParamName);
        Assert.Equal("destinationTimeZone", Assert.Throws<ArgumentNullException>(() => TimeText.DateAndTime(DateTimeOffset.MaxValue, null!)).ParamName);
    }

    // --------------------------------------------------------------------------- a zone that stands off the hour

    /// <summary>A viewer a half or a quarter of an hour off the hour is moved in their minutes, not only in their
    /// hours, and the dated line writes the odd offset as it is - with its sign, its hours and its minutes - on the
    /// day it lands them on, which is the day before for a zone west of UTC and the day after for one east of it.</summary>
    [Theory]
    [InlineData(-540, "15:00:00", "2026-10-04 15:00 -09:00")]
    [InlineData(-330, "18:30:00", "2026-10-04 18:30 -05:30")]
    [InlineData(-210, "20:30:00", "2026-10-04 20:30 -03:30")]
    [InlineData(345, "05:45:00", "2026-10-05 05:45 +05:45")]
    [InlineData(750, "12:30:00", "2026-10-05 12:30 +12:30")]
    public void AZoneOfAnOddOffsetMovesTheMinutesAndWritesThem(int offsetMinutes, string clock, string dateAndTime)
    {
        var midnight = new DateTimeOffset(2026, 10, 5, 0, 0, 0, TimeSpan.Zero);
        var zone = ZoneAt(offsetMinutes);

        Assert.Equal(clock, TimeText.Clock(midnight, zone));
        Assert.Equal(dateAndTime, TimeText.DateAndTime(midnight, zone));
    }

    // -------------------------------------------------------------------------- the offset the moment arrived in

    /// <summary>A moment written by a file in an offset of its own is shown in the viewer's zone and not in the one
    /// it came in: five hours west, it is the evening of the day before. A zone of no offset that is not named UTC
    /// converts too - the helper follows where the zone stands, not what it is called - and the instant underneath
    /// is the one second the moment is, whoever wrote it in whichever offset.</summary>
    [Fact]
    public void TheOffsetAMomentArrivesInMovesWhatTheViewerIsShownAndNotTheInstant()
    {
        var arrived = new DateTimeOffset(2026, 10, 5, 3, 0, 5, TimeSpan.FromHours(2));

        Assert.Equal("20:00:05", TimeText.Clock(arrived, WestFive));
        Assert.Equal("2026-10-04 20:00 -05:00", TimeText.DateAndTime(arrived, WestFive));

        // The same instant in the offset it arrived in, and in a zone of no offset that is not named UTC: the
        // viewer's own hours, wherever the zone stands - and the day the instant belongs to at that stand.
        Assert.Equal("03:00:05", TimeText.Clock(arrived, ZoneAt(120)));
        Assert.Equal("2026-10-05 03:00 +02:00", TimeText.DateAndTime(arrived, ZoneAt(120)));
        Assert.Equal(("01:00:05", "2026-10-05 01:00 +00:00"), SaidIn(arrived, ZoneAt(0)));

        // And the instant underneath all three viewers.
        Assert.Equal("2026-10-05T01:00:05Z", TimeText.Iso(arrived));
    }

    // ---------------------------------------------------------------------------- a zone that keeps a daylight rule

    /// <summary>The hour a zone forgets in March is said in one step: the second before the transition is
    /// <c>01:59:59</c> with the standard offset written under it, the second after is <c>03:00:00</c> with the
    /// daylight one, and nothing in between is shown at all - which is what a page that printed the local time of
    /// the answer instead of the instant would get wrong. The dated line moves with the clock, because the offset it
    /// names is the zone's at that moment.</summary>
    [Fact]
    public void TheHourAZoneForgetsIsSaidInOneStep()
    {
        var before = new DateTimeOffset(2026, 3, 29, 0, 59, 59, TimeSpan.Zero);
        var after = new DateTimeOffset(2026, 3, 29, 1, 0, 0, TimeSpan.Zero);

        Assert.Equal(("01:59:59", "2026-03-29 01:59 +01:00"), SaidIn(before, Summer));
        Assert.Equal(("03:00:00", "2026-03-29 03:00 +02:00"), SaidIn(after, Summer));

        // The instant underneath is a minute apart, whatever the wall clock jumped over.
        Assert.Equal("2026-03-29T00:59:59Z", TimeText.Iso(before));
        Assert.Equal("2026-03-29T01:00:00Z", TimeText.Iso(after));
    }

    /// <summary>The hour a zone lives twice in October is the other half of the same rule, and the one a
    /// <c>&lt;time&gt;</c> element exists to settle: two instants an hour apart are shown as the very same wall time
    /// - which is true, and is not enough to tell a reader which of the two they are being told - while the dated
    /// line names the offset each stands in and the machine-readable instant says them apart outright.</summary>
    [Fact]
    public void TwoInstantsTheClockSaysAlikeAreToldApartByTheOffsetAndTheInstant()
    {
        var inDaylightTime = new DateTimeOffset(2026, 10, 24, 23, 30, 0, TimeSpan.Zero);
        var inStandardTime = new DateTimeOffset(2026, 10, 25, 0, 30, 0, TimeSpan.Zero);

        // One wall time, said of both of them.
        Assert.Equal("01:30:00", TimeText.Clock(inDaylightTime, Summer));
        Assert.Equal("01:30:00", TimeText.Clock(inStandardTime, Summer));

        Assert.Equal("2026-10-25 01:30 +02:00", TimeText.DateAndTime(inDaylightTime, Summer));
        Assert.Equal("2026-10-25 01:30 +01:00", TimeText.DateAndTime(inStandardTime, Summer));

        Assert.Equal("2026-10-24T23:30:00Z", TimeText.Iso(inDaylightTime));
        Assert.Equal("2026-10-25T00:30:00Z", TimeText.Iso(inStandardTime));
    }

    // -------------------------------------------------------------------------------- a day that cannot hold a moment

    /// <summary>At the two ends of the calendar a zone sometimes has nowhere to put a moment: the first instant
    /// there is cannot stand five hours west of itself, and the last cannot stand five hours east of itself.
    /// <see cref="TimeZoneInfo" /> moves nothing that would fall off the end, so the moment is said where it stands,
    /// in the offset it came in - the clock reads its UTC hour and the dated line writes <c>+00:00</c> under it
    /// rather than the viewer's own. A moment with room on the side it is moved to is moved exactly, and its offset
    /// is written as the viewer's.</summary>
    [Fact]
    public void AZoneWithNoRoomForTheFirstOrTheLastMomentSaysItWhereItStands()
    {
        Assert.Equal(("00:00:00", "0001-01-01 00:00 +00:00"), SaidIn(DateTimeOffset.MinValue, WestFive));
        Assert.Equal(("23:59:59", "9999-12-31 23:59 +00:00"), SaidIn(DateTimeOffset.MaxValue, EastFive));

        // The same two moments in a zone that has room for them: moved, and said with the viewer's offset.
        Assert.Equal(("05:00:00", "0001-01-01 05:00 +05:00"), SaidIn(DateTimeOffset.MinValue, EastFive));
        Assert.Equal(("18:59:59", "9999-12-31 18:59 -05:00"), SaidIn(DateTimeOffset.MaxValue, WestFive));
    }

    // ---------------------------------------------------------------------- the fraction of a second, cut and not rounded

    /// <summary>The two members that show a moment to a person stop at the whole second they are given, and they cut
    /// rather than round: the last millisecond of <c>17:59</c> is still <c>17:59</c>, the first tick of
    /// <c>19:00</c> is already <c>19:00</c>, and a minute that reaches its last second at the end of a day in one
    /// zone is still the day before in that zone and the day after in the next. Nothing here rolls a viewer into an
    /// hour or a day the instant never reached.</summary>
    [Fact]
    public void TheFractionOfASecondIsCutAndNeverRoundedIntoTheNextUnit()
    {
        Assert.Equal("17:00:05", TimeText.Clock(new DateTimeOffset(2026, 10, 4, 22, 0, 5, 999, TimeSpan.Zero), WestFive));
        Assert.Equal("2026-10-04 17:59 -05:00", TimeText.DateAndTime(new DateTimeOffset(2026, 10, 4, 22, 59, 59, 999, TimeSpan.Zero), WestFive));

        // The last second of a day, in a zone east of UTC: the day it lands on is the next one, the second it lands
        // in is the one the instant is in.
        var endOfTheDay = new DateTimeOffset(2026, 10, 4, 23, 59, 59, 999, TimeSpan.Zero);

        Assert.Equal(("04:59:59", "2026-10-05 04:59 +05:00"), SaidIn(endOfTheDay, EastFive));
        Assert.Equal(("18:59:59", "2026-10-04 18:59 -05:00"), SaidIn(endOfTheDay, WestFive));

        // The first tick of the next day is the hour the day opened with, in both of them.
        var startOfTheDay = new DateTimeOffset(2026, 10, 5, 0, 0, 0, 1, TimeSpan.Zero);

        Assert.Equal(("19:00:00", "2026-10-04 19:00 -05:00"), SaidIn(startOfTheDay, WestFive));
        Assert.Equal(("05:00:00", "2026-10-05 05:00 +05:00"), SaidIn(startOfTheDay, EastFive));
    }

    // ---------------------------------------------------------------------------- the viewer's own culture

    /// <summary>The four words a moment is said in - the clock, the dated line, the instant and the age - are the
    /// dashboard's own and not the reader's: a viewer of German, whose culture writes <c>17,00</c> and a dotted
    /// date, is shown the same colons, the same hyphens and the same <c>Z</c> as anyone else, and the same words as
    /// the test itself reads outside the culture.</summary>
    [Fact]
    public void AMomentIsSaidTheSameWayWhateverTheViewerSpeaks()
    {
        var moment = new DateTimeOffset(2026, 10, 4, 22, 0, 5, TimeSpan.Zero);
        var older = moment.AddHours(-5);

        // Read by the invariant culture first, so that the German half below has something to be the same as.
        var invariant = (
            Clock: TimeText.Clock(moment, WestFive),
            Dated: TimeText.DateAndTime(moment, WestFive),
            Instant: TimeText.Iso(moment),
            Age: TimeText.Ago(older, moment));

        Assert.Equal(("17:00:05", "2026-10-04 17:00 -05:00", "2026-10-04T22:00:05Z", "5 h ago"), invariant);

        ViewerCulture.German(() =>
            Assert.Equal(
                invariant,
                (TimeText.Clock(moment, WestFive), TimeText.DateAndTime(moment, WestFive), TimeText.Iso(moment), TimeText.Ago(older, moment))));
    }

    // ------------------------------------------------------------------- the thresholds, at the smallest step there is

    /// <summary>Every unit change of <c>TimeText.Span</c> holds at the tick: one tick under sixty seconds is still
    /// said in seconds, one tick over is minutes; one tick under two hours of minutes is still said in minutes, one
    /// tick over is hours; one tick under two days of hours is still said in hours, one tick over is days. The whole
    /// numbers the page prints turn on the exact threshold and on nothing near it - which is the same rule the
    /// rounded seconds of <see cref="TimeTextHelperTests" /> pin, at the step a clock cannot round away.</summary>
    [Theory]
    [InlineData(599_999_999, "59 s")]
    [InlineData(600_000_000, "1 min")]
    [InlineData(600_000_001, "1 min")]
    [InlineData(71_999_999_999, "119 min")]
    [InlineData(72_000_000_000, "2 h")]
    [InlineData(72_000_000_001, "2 h")]
    [InlineData(1_727_999_999_999, "47 h")]
    [InlineData(1_728_000_000_000, "2 d")]
    [InlineData(1_728_000_000_001, "2 d")]
    public void AUnitChangesAtTheThresholdToTheTick(long ticks, string expected) =>
        Assert.Equal(expected, TimeText.Span(TimeSpan.FromTicks(ticks)));

    /// <summary>The minute <c>TimeText.Ago</c> stops saying "just now" at holds at the tick: a moment one tick
    /// short of a minute old is still "just now", one tick past it is a minute ago. The whole numbers of the age are
    /// the whole numbers of the length it is said in, so the units change at the same ticks <see cref="AUnitChangesAtTheThresholdToTheTick" /> counts.</summary>
    [Theory]
    [InlineData(599_999_999, "just now")]
    [InlineData(600_000_000, "1 min ago")]
    [InlineData(600_000_001, "1 min ago")]
    [InlineData(71_999_999_999, "119 min ago")]
    [InlineData(72_000_000_001, "2 h ago")]
    [InlineData(1_727_999_999_999, "47 h ago")]
    [InlineData(1_728_000_000_001, "2 d ago")]
    public void AnAgeTurnsAtTheThresholdToTheTick(long ticks, string expected) =>
        Assert.Equal(expected, TimeText.Ago(Now - TimeSpan.FromTicks(ticks), Now));

    /// <summary>The far side of that same minute: a moment exactly a minute ahead of the clock - the drift a file
    /// gives when it stamps its answer from the future - is still "just now", since the length it would be counted
    /// in is below zero and the page says no length below zero at all. A second ahead and a day ahead are pinned by
    /// <see cref="EdgeCaseTextTests" />; what is new here is the exact minute the clamp reaches before it is used.</summary>
    [Fact]
    public void AMomentExactlyAMinuteAheadOfTheClockIsStillSaidAsNow() =>
        Assert.Equal("just now", TimeText.Ago(Now + TimeSpan.FromMinutes(1), Now));

    /// <summary>An age is the instant between two moments and never the wall clock they are read by: on the day the
    /// zone jumps forward, half past one and half past three are sixty minutes apart although the clock stands two
    /// hours further on, and half past twelve and half past three are two hours apart although it stands three.
    /// The local time the jump skipped over - half past two, written with the offset it would have had - is the very
    /// instant half past three is, and so has no age at all. The hour a viewer gains or loses is the zone's
    /// business; the page counts the seconds between the two moments it is given.</summary>
    [Theory]
    [InlineData(1, 30, 3, 30, "60 min ago")]
    [InlineData(0, 30, 3, 30, "2 h ago")]
    [InlineData(2, 30, 3, 30, "just now")]
    public void TheAgeOfAMomentIsTheInstantBetweenAndNotTheWallClock(
        int thenHour, int thenMinute, int nowHour, int nowMinute, string expected)
    {
        // 2026-03-29: daylight time begins at two in the morning local, so the local clock steps from 02:00 +01:00
        // to 03:00 +02:00. The two moments below are written in the offsets the zone itself gives them.
        var then = new DateTimeOffset(2026, 3, 29, thenHour, thenMinute, 0, TimeSpan.FromHours(1));
        var now = new DateTimeOffset(2026, 3, 29, nowHour, nowMinute, 0, TimeSpan.FromHours(2));

        Assert.Equal(expected, TimeText.Ago(then, now));
    }

    /// <summary>The oldest age the page can be asked for, in the unit it has at the top: the first moment of the
    /// calendar counted to the last - <c>3652059 d</c>, days to the end and never a year, because the class stops at
    /// a day - and the same first moment counted to the rendering this page stands at, two thousand years of days in
    /// one word. Neither is thrown, left unsaid or rounded into the next unit above: there is none.</summary>
    [Fact]
    public void TheAgeOfTheWholeCalendarIsSaidInDays()
    {
        Assert.Equal("3652059 d ago", TimeText.Ago(DateTimeOffset.MinValue, DateTimeOffset.MaxValue));
        Assert.Equal("739892 d ago", TimeText.Ago(default, Now));
    }

    // --------------------------------------------------------------------------------------------- the members

    /// <summary>The whole public surface of the class: five ways of saying a moment, all static, all returning a
    /// <see cref="string" />, each taking exactly the arguments it needs - the two that face a viewer take the zone,
    /// the instant, the length and the two ages take none. A static holder with nothing to construct, nothing to hold
    /// and no nested type to hide. A sixth member has to be written here as well, which is the point: a new way of
    /// saying a moment on this page is a decision somebody has to have made.</summary>
    [Fact]
    public void TheHelperIsFiveWaysOfSayingAMomentAndNothingElse()
    {
        var type = typeof(TimeText);

        Assert.True(type.IsAbstract && type.IsSealed, "TimeText is a static holder: nothing of it is created");
        Assert.Empty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic));
        Assert.Empty(type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));
        Assert.Empty(type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));
        Assert.Empty(type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));
        Assert.Empty(type.GetNestedTypes(BindingFlags.Public));
        Assert.Empty(type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.Equal(
            [
                "String Ago(DateTimeOffset then, DateTimeOffset now)",
                "String Clock(DateTimeOffset time, TimeZoneInfo zone)",
                "String DateAndTime(DateTimeOffset time, TimeZoneInfo zone)",
                "String Iso(DateTimeOffset time)",
                "String Span(TimeSpan span)",
            ],
            [.. methods.Select(Signature).Order(StringComparer.Ordinal)]);

        Assert.All(methods, method => Assert.True(method.IsStatic, $"{method.Name} is called through an instance"));
    }

    // ------------------------------------------------------------------------------------------- the machinery

    /// <summary>The two words a viewer standing in this zone is shown for one moment: the clock they read and the
    /// dated line the page gives them as a tooltip. The two members that take a zone are asked together because a
    /// zone moves both of them at once, and a test that pinned only one of them would let the other drift.</summary>
    private static (string Clock, string DateAndTime) SaidIn(DateTimeOffset moment, TimeZoneInfo zone) =>
        (TimeText.Clock(moment, zone), TimeText.DateAndTime(moment, zone));

    /// <summary>A zone standing at the offset named, of no name the machine knows and keeping no daylight rule: the
    /// offset is what the two members are shown to follow, so the test brings it in minutes and lets nothing else of
    /// a zone be in the question.</summary>
    private static TimeZoneInfo ZoneAt(int offsetMinutes) =>
        TimeZoneInfo.CreateCustomTimeZone(
            $"offset-{offsetMinutes.ToString(CultureInfo.InvariantCulture)}",
            TimeSpan.FromMinutes(offsetMinutes),
            $"Offset {offsetMinutes.ToString(CultureInfo.InvariantCulture)}",
            $"Offset {offsetMinutes.ToString(CultureInfo.InvariantCulture)}");

    /// <summary>A zone one hour east of UTC that steps into and out of daylight time on the last Sunday of March and
    /// of October, at two in the morning. Every transition the tests of this file count on is one the test wrote, so
    /// no machine's time zone database and no clock decides an expected value.</summary>
    private static TimeZoneInfo SummerZone()
    {
        var rule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
            new DateTime(2026, 1, 1),
            DateTime.MaxValue.Date,
            TimeSpan.FromHours(1),
            TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 3, 5, DayOfWeek.Sunday),
            TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 5, DayOfWeek.Sunday));

        return TimeZoneInfo.CreateCustomTimeZone("summer-two-hours", TimeSpan.FromHours(1), "Summer two hours", "Summer standard", "Summer daylight", [rule]);
    }

    /// <summary>A member as the surface is written: what it gives back, what it is called, and its arguments in the
    /// order a caller names them.</summary>
    private static string Signature(MethodInfo method) =>
        $"{method.ReturnType.Name} {method.Name}({string.Join(", ", method.GetParameters().Select(parameter => $"{parameter.ParameterType.Name} {parameter.Name}"))})";
}
