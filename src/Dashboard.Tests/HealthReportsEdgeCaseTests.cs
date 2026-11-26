using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="HealthReports" />: what the pipeline's hourly
/// health reports found in an environment, as the file leaves it - the two windows they count (the last 24 hours
/// and the last seven days), when the last report that failed ended, and the entries they keep for the
/// environment's container apps. The record holds its four parts and nothing else; all the counting and the words
/// of the line are in the reading and in <see cref="AvailabilityText" />.
/// <para>
/// Where a <c>health</c> section of <c>delivery.json</c> is a report at all is already pinned where the file is
/// read: <see cref="AvailabilityTests" /> says that a count that is not there or cannot be is no report - a window
/// alone, a window of more healthy than there were, a window of negative reports - and that the line and its parts
/// say a part the file does not have is left out, a window without a report says so, and a report with neither
/// window has no parts at all; <see cref="SharedCostAvailabilityTests" /> says the same of a report without a
/// window and says a known report's parts in its own words; <see cref="DeliveryEnvironmentTests" /> says the
/// environment keeps the health the file read as the record of its four parts and the health set by hand keeps its
/// window. What each part then is said as - the words of the count, the percent that is never 100 while a report
/// failed and never 0 while one passed, the failure's "ago" - is pinned there, so none of that is said again here.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the four parts as the record holds them - the constructor's
/// arguments landing on the property of the same name, the very instances it was given, the apps the file does not
/// say being no apps and an empty list of apps being kept and differing from no list; the record counting nothing
/// it is given - the zero window, the window with more healthy than there were, the negative count and the window
/// at the end of its number, all kept as given, since it is the reading, not the record, that turns a count into no
/// window; the moment the last failure ended, held to the tick at the ends of its number, the same instant written
/// in other hours being the same failure and a tick away being another; the equality of the record - two reports
/// one while their four parts are, the hash that follows, the difference in any one of them, the apps compared as
/// the list they stand in, the reports that are not there - and the members every assertion above leans on without
/// ever saying a word about them: the copy a <c>with</c> makes, the four parts read out in the order they stand in
/// and the line the record says about itself.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every moment is one the test
/// brought, and the hours it is written in are the ones the test wrote.
/// </para>
/// </summary>
public class HealthReportsEdgeCaseTests
{
    /// <summary>The moment the last report that failed ended, as the file writes it on the meridian.</summary>
    private static readonly DateTimeOffset Failed = new(2026, 10, 6, 16, 13, 0, TimeSpan.Zero);

    /// <summary>The very instant of <see cref="Failed" />, five hours east of the meridian.</summary>
    private static readonly DateTimeOffset FailedInOtherHours = new(2026, 10, 6, 21, 13, 0, TimeSpan.FromHours(5));

    /// <summary>A report of the parts it is told, the parts left out the way the test leaves them out.</summary>
    private static HealthReports Reports(HealthWindow? day = null, HealthWindow? week = null, DateTimeOffset? failure = null,
        IReadOnlyList<ContainerAppReports>? apps = null) => new(day, week, failure, apps);

    // ---------------------------------------------------------------- the four parts as the record holds them

    /// <summary>
    /// A report is its four parts and nothing else: the constructor's arguments each land on the property of the
    /// same name, and the two windows and the list of apps the record holds the very instances it was given - the
    /// report the page reads and the words it says of it standing on the same facts, not on copies of them. The
    /// moment it holds is held as the moment it is, at the offset it stands on.
    /// </summary>
    [Fact]
    public void TheReportKeepsTheTwoWindowsTheLastFailureAndTheApps()
    {
        var day = new HealthWindow(24, 23);
        var week = new HealthWindow(168, 160);
        var apps = new[] { new ContainerAppReports("ui", null, null, null) };
        var reports = new HealthReports(day, week, Failed, apps);

        Assert.Same(day, reports.Last24Hours);
        Assert.Same(week, reports.Last7Days);
        Assert.Equal(Failed, reports.LastFailure);
        Assert.Same(apps, reports.ContainerApps);
        Assert.Equal("ui", Assert.Single(reports.ContainerApps!).Name);
    }

    /// <summary>
    /// The part the file does not say is no part: the apps of a report the fourth part is not told of are no apps
    /// - the record's own default - and an explicit <see langword="null" /> is the same no apps. The windows and
    /// the moment the file does not say stand in <see cref="AvailabilityTests" />, read as no window and no
    /// failure; here the property itself is said to hold nothing.
    /// </summary>
    [Fact]
    public void TheAppsTheFileDoesNotSayAreNoApps()
    {
        Assert.Null(new HealthReports(new HealthWindow(24, 23), new HealthWindow(168, 160), Failed).ContainerApps);
        Assert.Null(Reports(new HealthWindow(24, 23), new HealthWindow(168, 160), Failed, null).ContainerApps);
        Assert.Null(Reports().ContainerApps);
    }

    /// <summary>
    /// An empty list of apps is kept as the empty list it is: not <see langword="null" />, holding nothing, the
    /// very array it was given - and differing from no list at all, each way the question is asked: the empty list
    /// is a part the report has and no list is a part the report does not have.
    /// </summary>
    [Fact]
    public void AnEmptyListOfAppsIsKeptAndDiffersFromNoList()
    {
        var empty = Array.Empty<ContainerAppReports>();
        var withEmpty = Reports(apps: empty);
        var withNone = Reports();

        Assert.NotNull(withEmpty.ContainerApps);
        Assert.Same(empty, withEmpty.ContainerApps);
        Assert.Empty(withEmpty.ContainerApps);
        Assert.Null(withNone.ContainerApps);
        Assert.NotEqual(withEmpty, withNone);
        Assert.True(withEmpty != withNone);
        Assert.False(withNone.Equals(withEmpty));
    }

    // ---------------------------------------------------------------- the parts the record does not count

    /// <summary>
    /// The record counts nothing it is given: the window that counted no report, the window with more healthy
    /// than there were, the window of negative reports and the window at the end of its number are all kept as
    /// the very instances they are. It is <c>HealthWindow.Read</c>, not the record, that turns a count that cannot
    /// be into no window - the healthy more than the reports and the negative reports refused at the file, which
    /// <see cref="AvailabilityTests" /> pins.
    /// </summary>
    [Fact]
    public void AWindowIsKeptAsItIsGivenWithoutBeingCounted()
    {
        var none = new HealthWindow(0, 0);
        var moreHealthyThanReports = new HealthWindow(24, 25);
        var negative = new HealthWindow(-1, 0);
        var atTheEndOfItsNumber = new HealthWindow(int.MaxValue, int.MaxValue);

        Assert.Same(none, Reports(day: none).Last24Hours);
        Assert.Same(moreHealthyThanReports, Reports(week: moreHealthyThanReports).Last7Days);
        Assert.Same(negative, Reports(day: negative).Last24Hours);
        Assert.Same(atTheEndOfItsNumber, Reports(day: atTheEndOfItsNumber, week: atTheEndOfItsNumber).Last7Days);
        Assert.Equal(none, Reports(day: none).Last24Hours);
        Assert.Equal(moreHealthyThanReports, Reports(week: moreHealthyThanReports).Last7Days);
    }

    /// <summary>
    /// The moment the last failure ended is held to the tick, at the ends of its own number: the first and the
    /// last moment there are, and a tick away from the moment the file writes either side of it - the record
    /// compares the tick, so a report of the moment one tick later is another report, though it is the same
    /// second the failure is said in.
    /// </summary>
    [Fact]
    public void TheLastFailureIsHeldToTheTickAtTheEndsOfItsNumber()
    {
        Assert.Equal(DateTimeOffset.MinValue, Reports(failure: DateTimeOffset.MinValue).LastFailure);
        Assert.Equal(DateTimeOffset.MaxValue, Reports(failure: DateTimeOffset.MaxValue).LastFailure);

        var tickBefore = Failed.AddTicks(-1);
        var tickAfter = Failed.AddTicks(1);

        Assert.Equal(tickBefore, Reports(failure: tickBefore).LastFailure);
        Assert.Equal(tickAfter, Reports(failure: tickAfter).LastFailure);
        Assert.NotEqual(Reports(failure: Failed), Reports(failure: tickAfter));
        Assert.NotEqual(Reports(failure: Failed), Reports(failure: tickBefore));
        Assert.NotEqual(Reports(failure: tickAfter), Reports(failure: tickBefore));
    }

    /// <summary>
    /// The same instant, whichever hours write it, is the same failure: the moment the file writes on the
    /// meridian and the very moment five hours east of it are one report - the equality of the operator, of
    /// <c>Equals</c> boxed, and the hash that goes with it - while the instant one second later in the other
    /// hours is another.
    /// </summary>
    [Fact]
    public void TheSameInstantInOtherHoursIsTheSameFailure()
    {
        var onTheMeridian = Reports(failure: Failed);
        var inOtherHours = Reports(failure: FailedInOtherHours);

        Assert.True(onTheMeridian == inOtherHours);
        Assert.True(onTheMeridian.Equals((object)inOtherHours));
        Assert.Equal(onTheMeridian, inOtherHours);
        Assert.Equal(onTheMeridian.GetHashCode(), inOtherHours.GetHashCode());
        Assert.NotEqual(onTheMeridian, Reports(failure: FailedInOtherHours.AddSeconds(1)));
    }

    // ---------------------------------------------------------------- the record itself

    /// <summary>
    /// Two reports are one while their four parts are - each a part's own equality, the moment of the failure as
    /// the instant it is and the list of apps as the reference it holds - the equality of the operator, of
    /// <c>Equals</c> boxed and unboxed, and the hash that follows them and stands from call to call, so a report
    /// is usable as the key of a row. A part of another kind is no report's equal.
    /// </summary>
    [Fact]
    public void TwoReportsAreOneWhileTheirFourPartsAre()
    {
        var apps = new[] { new ContainerAppReports("ui", null, new HealthWindow(24, 24), new HealthWindow(168, 167)) };
        var one = Reports(new HealthWindow(24, 23), new HealthWindow(166, 164), Failed, apps);
        var two = Reports(new HealthWindow(24, 23), new HealthWindow(166, 164), Failed, apps);

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.True(one.Equals(two));
        Assert.True(two.Equals(one));
        Assert.True(one.Equals((object)two));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.False(one.Equals("23 of 24"));
        Assert.False(one.Equals(Failed));
        Assert.False(one.Equals(new HealthWindow(24, 23)));
    }

    /// <summary>
    /// The apps are compared as the list they stand in: two reports hold one list while they hold the very list
    /// - the same reference, whatever its contents - and they do not hold one while each holds a list of its own,
    /// even a list of the same app: the record asks the reference of the list and not the count of its apps.
    /// </summary>
    [Fact]
    public void TheAppsAreComparedAsTheListTheyStandIn()
    {
        var app = new ContainerAppReports("ui", null, null, null);
        var reports = Reports(apps: new[] { app });
        var sameList = Reports(apps: reports.ContainerApps);
        var sameContents = Reports(apps: new[] { app });

        Assert.Equal(reports, sameList);
        Assert.Same(reports.ContainerApps, sameList.ContainerApps);
        Assert.NotEqual(reports, sameContents);
        Assert.NotSame(reports.ContainerApps, sameContents.ContainerApps);
        Assert.Equal(app, Assert.Single(sameContents.ContainerApps!));
    }

    /// <summary>
    /// A report differs in any one of its parts: either window to another count of its own or to no window at
    /// all, the failure one second later or no failure, the apps to no list or to a list of its own, empty or of
    /// the same app - and the difference stands whichever way the question is asked.
    /// </summary>
    [Fact]
    public void AReportDiffersInAnyOneOfItsParts()
    {
        var day = new HealthWindow(24, 23);
        var week = new HealthWindow(166, 164);
        var apps = new[] { new ContainerAppReports("ui", null, null, null) };
        var reports = Reports(day, week, Failed, apps);

        Assert.NotEqual(reports, reports with { Last24Hours = new HealthWindow(24, 22) });
        Assert.NotEqual(reports, reports with { Last24Hours = null });
        Assert.NotEqual(reports, reports with { Last7Days = new HealthWindow(166, 165) });
        Assert.NotEqual(reports, reports with { Last7Days = null });
        Assert.NotEqual(reports, reports with { LastFailure = Failed.AddSeconds(1) });
        Assert.NotEqual(reports, reports with { LastFailure = null });
        Assert.NotEqual(reports, reports with { ContainerApps = null });
        Assert.NotEqual(reports, reports with { ContainerApps = Array.Empty<ContainerAppReports>() });
        Assert.NotEqual(reports, Reports(day, week, Failed, new[] { new ContainerAppReports("ui", null, null, null) }));
    }

    /// <summary>
    /// Reports that are not there are no reports: the record's default is no reports, and no reports are never
    /// equal to a report however the question is asked - the operators and <c>Equals</c> asked about the report
    /// that is - while the report that is, with every part left out, is a report.
    /// </summary>
    [Fact]
    public void ReportsThatAreNotThereAreNoReports()
    {
        Assert.Null(default(HealthReports));

        HealthReports? none = null;
        var there = Reports();

        Assert.False(none == there);
        Assert.True(none != there);
        Assert.False(there.Equals(none));
        Assert.False(there == none);
        Assert.True(there != none);
        Assert.True(there is not null);
    }

    /// <summary>
    /// A copy of a report is the report again: the <c>with</c> that names no part and the <c>with</c> that names
    /// every part keep the rest - the very instances, the windows and the moment held where they stood - and not
    /// the same reference. A part turned to no part keeps the other three and is another report.
    /// </summary>
    [Fact]
    public void ACopyOfAReportIsTheReportAgain()
    {
        var day = new HealthWindow(24, 23);
        var reports = Reports(day, new HealthWindow(166, 164), Failed);

        var copy = reports with { };
        Assert.Equal(reports, copy);
        Assert.NotSame(reports, copy);

        var repack = reports with
        {
            Last24Hours = reports.Last24Hours,
            Last7Days = reports.Last7Days,
            LastFailure = reports.LastFailure,
            ContainerApps = reports.ContainerApps,
        };
        Assert.Equal(reports, repack);
        Assert.NotSame(reports, repack);

        var weekless = reports with { Last7Days = null };
        Assert.Same(day, weekless.Last24Hours);
        Assert.Null(weekless.Last7Days);
        Assert.Equal(Failed, weekless.LastFailure);
        Assert.Null(weekless.ContainerApps);
        Assert.NotEqual(reports, weekless);
    }

    /// <summary>
    /// The four parts read out of the report in the order they stand in it: the very instances the record was
    /// given, and a report without its parts falling apart as absent - the absent parts coming back out as
    /// <see langword="null" />, each of them.
    /// </summary>
    [Fact]
    public void TheFourPartsReadOutOfTheReport()
    {
        var day = new HealthWindow(24, 23);
        var week = new HealthWindow(166, 164);
        var apps = new[] { new ContainerAppReports("ui", null, null, null) };

        var (oneDay, oneWeek, oneFailure, oneApps) = new HealthReports(day, week, Failed, apps);

        Assert.Same(day, oneDay);
        Assert.Same(week, oneWeek);
        Assert.Equal(Failed, oneFailure);
        Assert.Same(apps, oneApps);

        var (noDay, noWeek, noFailure, noApps) = new HealthReports(null, null, null);

        Assert.Null(noDay);
        Assert.Null(noWeek);
        Assert.Null(noFailure);
        Assert.Null(noApps);
    }

    /// <summary>
    /// The report says its four parts in their order, the name of each with the value it stands on: the windows
    /// as the windows say themselves, the moment in the hours it stands on - the other hours the same instant
    /// was written in saying themselves - and the list of apps as the list it holds, whatever it counts. The
    /// parts the report does not have say their name and nothing more. (The words the moment says are the
    /// viewer's, so they are checked in the hours every viewer shares.)
    /// </summary>
    [Fact]
    public void TheReportSaysItsFourPartsInTheirOrder()
    {
        CultureScope.Runs(CultureInfo.InvariantCulture, () =>
        {
            Assert.Equal(
                "HealthReports { Last24Hours = HealthWindow { Reports = 24, Healthy = 23, AllHealthy = False }, "
                + "Last7Days = HealthWindow { Reports = 166, Healthy = 164, AllHealthy = False }, LastFailure = 10/06/2026 16:13:00 +00:00, "
                + "ContainerApps = Dashboard.Health.ContainerAppReports[] }",
                Reports(new HealthWindow(24, 23), new HealthWindow(166, 164), Failed, new[] { new ContainerAppReports("ui", null, null, null) }).ToString());
            Assert.Equal(
                "HealthReports { Last24Hours = HealthWindow { Reports = 0, Healthy = 0, AllHealthy = True }, Last7Days = , "
                + "LastFailure = 01/01/0001 00:00:00 +00:00, ContainerApps =  }",
                Reports(new HealthWindow(0, 0), null, DateTimeOffset.MinValue, null).ToString());
            Assert.Equal(
                "HealthReports { Last24Hours = , Last7Days = , LastFailure = 10/06/2026 21:13:00 +05:00, ContainerApps =  }",
                Reports(failure: FailedInOtherHours).ToString());
            Assert.Equal(
                "HealthReports { Last24Hours = , Last7Days = , LastFailure = , ContainerApps =  }",
                Reports().ToString());
        });
    }
}
