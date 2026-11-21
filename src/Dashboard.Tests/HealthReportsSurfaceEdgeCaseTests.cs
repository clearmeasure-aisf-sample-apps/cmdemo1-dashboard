using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests that <see cref="HealthReports" /> still owed: the shape of the
/// record itself, the report as the key of a set and of a lookup, the four parts asked out of it one after the
/// other, the copy leaving what it copied standing, and the words the record says about itself in a viewer's hours.
/// <see cref="HealthReports" /> holds four parts - the window of the last 24 hours, the window of the last seven
/// days, the moment the last report that failed ended, and the entries it keeps for the environment's container
/// apps - and counts, weighs and says nothing else; the counting is <c>HealthWindow</c>'s, the words are
/// <see cref="AvailabilityText" />'s, and the reading of the <c>health</c> section is <c>HealthReports.Read</c>,
/// internal to an app that declares no <c>InternalsVisibleTo</c> and so pinned through
/// <see cref="DeliveryReport.Parse" />.
/// </summary>
/// <remarks>
/// <see cref="HealthReportsEdgeCaseTests" /> - the file this one finishes rather than repeats, added for the same
/// record under an earlier issue - already pins the four parts as the record holds them (the arguments landing on
/// the property of the same name, the very instances, no apps and a kept empty list of apps), the record counting
/// nothing it is given (the zero window, more healthy than there were, the negative count, the count at the end of
/// <see cref="int" />), the moment of the failure held to the tick at the ends of its number and the same instant
/// spelled in other hours, the equality of two reports and the difference in any one part, the apps compared as the
/// list they stand in, the reports that are not there, the copy a <c>with</c> makes, the four parts fallen apart in
/// the order they stand in, and the invariant words of the record. <see cref="AvailabilityTests" />,
/// <see cref="SharedCostAvailabilityTests" /> and <see cref="DeliveryEnvironmentTests" /> pin what the file reads
/// into a report and the words the page makes of it.
/// <para>
/// Left open by all of that, and pinned here: that the record is its four parts and nothing more, by the shape of
/// it - the four properties in the constructor's order and of the constructor's types, each readable and each
/// writable only as a copy is made, the one constructor with the apps the only part it
/// will do without, the interface a record is compared through and the sealedness of it, and the public surface as
/// the members it answers to, so that the day a fifth part is added the list below says so; the report in a set and
/// in a lookup, boxed and unboxed hashed alike, and a report the page read meeting the key a test wrote out; the
/// reading of one and the same file giving two reports, because each reading brought its own list of apps along;
/// the report that says nothing at all being one report, one hash and one entry, and another than the report of a
/// part that is only as good as nothing - the zero window, the empty list, the first moment there are; the four
/// parts asked one after the other and put back in, at the ends of their numbers; the original still standing, in
/// its very instances, after a copy turned each part of it, and a part told to a fresh report through its own name;
/// and the record's words in a German reader's hours - the moment the reader's to say, the names, the windows and
/// the kind of list not, and the list saying what it stands in rather than what it counts.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every moment below is one the test
/// brought, every file is words the test wrote, and the hours the words are said in are the ones the test stood in.
/// </para>
public class HealthReportsSurfaceEdgeCaseTests
{
    /// <summary>The moment the last report that failed ended, on the meridian: the fixture's moment of <see cref="AvailabilityTests" />.</summary>
    private static readonly DateTimeOffset Failed = new(2026, 10, 6, 16, 13, 0, TimeSpan.Zero);

    /// <summary>The very instant of <see cref="Failed" />, five hours east of the meridian.</summary>
    private static readonly DateTimeOffset FailedInOtherHours = new(2026, 10, 6, 21, 13, 0, TimeSpan.FromHours(5));

    /// <summary>A report of the parts it is told, the parts left out the way the file leaves them out.</summary>
    private static HealthReports Reports(HealthWindow? day = null, HealthWindow? week = null, DateTimeOffset? failure = null,
        IReadOnlyList<ContainerAppReports>? apps = null) => new(day, week, failure, apps);

    /// <summary>The delivery file of one environment whose <c>health</c> says what it is told and nothing else.</summary>
    private static HealthReports Read(string health) =>
        DeliveryReport.Parse($$"""{ "environments": [ { "name": "prod", "health": {{health}} } ] }""")!.HealthOf("prod")!;

    // ---------------------------------------------------------------- the record's own shape

    /// <summary>
    /// The report's whole public surface, as it stands: its four parts, named in the order the constructor names
    /// them - the day, the week, the moment of the last failure, the apps - each of the type the constructor takes
    /// it as, each readable and each writable as a copy is made, and nothing else to call but the members every
    /// record is asked: falling apart, comparing, hashing and saying itself, with the <c>==</c> and the <c>!=</c> a
    /// caller writes. A fifth part, or one of the four turned away from the type it is held as, has to be written
    /// here as well, which is the point: the page cannot be shown a part nobody decided what it means.
    /// </summary>
    [Fact]
    public void TheReportHasItsFourPartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Last24Hours", "Last7Days", "LastFailure", "ContainerApps"],
            [.. typeof(HealthReports)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["ContainerApps", "Deconstruct", "Equals", "GetHashCode", "Last24Hours", "Last7Days", "LastFailure", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());

        Assert.Equal([typeof(IEquatable<HealthReports>)], typeof(HealthReports).GetInterfaces());
        Assert.True(typeof(HealthReports).IsSealed);

        var properties = typeof(HealthReports)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(HealthWindow), properties["Last24Hours"].PropertyType);
        Assert.Equal(typeof(HealthWindow), properties["Last7Days"].PropertyType);
        Assert.Equal(typeof(DateTimeOffset?), properties["LastFailure"].PropertyType);
        Assert.Equal(typeof(IReadOnlyList<ContainerAppReports>), properties["ContainerApps"].PropertyType);

        // Every part is read, and every part is a place a copy may turn it - and none of the four is a verdict
        // drawn from another, which is what HealthWindow.AllHealthy is and why that one is not written.
        Assert.All(properties.Values, property =>
        {
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
        });

        // Four parts, and the one constructor that takes them in that order: the apps the only part it will do
        // without, and the nothing it leaves them as is no list at all.
        var constructors = typeof(HealthReports).GetConstructors();

        Assert.Single(constructors);
        var parameters = constructors[0].GetParameters();

        Assert.Equal(["Last24Hours", "Last7Days", "LastFailure", "ContainerApps"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal(typeof(HealthWindow), parameters[0].ParameterType);
        Assert.Equal(typeof(HealthWindow), parameters[1].ParameterType);
        Assert.Equal(typeof(DateTimeOffset?), parameters[2].ParameterType);
        Assert.Equal(typeof(IReadOnlyList<ContainerAppReports>), parameters[3].ParameterType);
        Assert.All(parameters.Take(3), parameter => Assert.False(parameter.HasDefaultValue));
        Assert.True(parameters[3].HasDefaultValue);
        Assert.Null(parameters[3].DefaultValue);
    }

    // ------------------------------------------------------------- the report as a key

    /// <summary>
    /// The equality and the hash the record answers with are good for a set and a lookup: two reports of one part
    /// each are one entry and one key, the report found by its twin which is no reference of it, the hash the same
    /// boxed as unboxed and from one call to the next, and the very reference meeting itself. The moment spelled in
    /// other hours is the same key, while the instant a second later is another entry.
    /// </summary>
    [Fact]
    public void AReportMeetsItsTwinInASetAndInALookup()
    {
        var one = Reports(new HealthWindow(24, 23), new HealthWindow(166, 164), Failed);
        var two = Reports(new HealthWindow(24, 23), new HealthWindow(166, 164), Failed);

        Assert.Single(new HashSet<HealthReports> { one, two });
        Assert.Contains(one, new HashSet<HealthReports> { two });
        Assert.Contains(two, new HashSet<HealthReports> { one });
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)two).GetHashCode());
        Assert.Equal(((object)one).GetHashCode(), ((object)two).GetHashCode());

        var rows = new Dictionary<HealthReports, string> { [one] = "prod" };

        Assert.Equal("prod", rows[two]);
        Assert.True(rows.ContainsKey(Reports(new HealthWindow(24, 23), new HealthWindow(166, 164), FailedInOtherHours)));
        Assert.False(rows.ContainsKey(one with { LastFailure = Failed.AddSeconds(1) }));
        Assert.Equal(2, new HashSet<HealthReports> { one, one with { LastFailure = Failed.AddSeconds(1) } }.Count);
        Assert.Equal([one], rows.Keys);
    }

    /// <summary>
    /// A report the page read out of a file meets the report a test wrote out: the same two windows, the same
    /// moment, the apps the file does not say being no apps on either side, so that one key of the lookups the page
    /// keeps holds whatever wrote the report. The lookup is asked by name in any case, as the file's lookups are.
    /// </summary>
    [Fact]
    public void AReportTheFileReadIsTheReportOfTheSameParts()
    {
        var read = Read("""
            { "last24Hours": { "reports": 24, "healthy": 23 }, "last7Days": { "reports": 166, "healthy": 164 }, "lastFailure": "2026-10-06T16:13:00Z" }
            """);
        var written = Reports(new HealthWindow(24, 23), new HealthWindow(166, 164), Failed);

        Assert.Equal(written, read);
        Assert.True(written == read);
        Assert.Equal(written.GetHashCode(), read.GetHashCode());
        Assert.Single(new HashSet<HealthReports> { read, written });
        Assert.Equal(read, DeliveryReport.Parse("""{ "environments": [ { "name": "PROD", "health": { "last24Hours": { "reports": 24, "healthy": 23 }, "last7Days": { "reports": 166, "healthy": 164 }, "lastFailure": "2026-10-06T16:13:00Z" } } ] }""")!.HealthOf("prod"));
    }

    /// <summary>
    /// The apps being compared as the list they stand in has a length the file's own words do not: the very same
    /// text read twice gives two reports, because each reading brought a list of its own along - one window, one
    /// moment, one app and two reports, two entries of a set and no key meeting its twin. A reading of the same
    /// text that names no apps is one report again, the no apps being no list rather than a list nobody filled.
    /// </summary>
    [Fact]
    public void TheSameFileReadTwiceIsTwoReportsWhenItNamesApps()
    {
        const string WithApps = """{ "last24Hours": { "reports": 24, "healthy": 23 }, "containerApps": [ { "name": "ui" } ] }""";

        var first = Read(WithApps);
        var again = Read(WithApps);

        Assert.Equal(first.Last24Hours, again.Last24Hours);
        Assert.Equal(first.LastFailure, again.LastFailure);
        Assert.Equal(Assert.Single(first.ContainerApps!), Assert.Single(again.ContainerApps!));
        Assert.NotSame(first.ContainerApps, again.ContainerApps);
        Assert.NotEqual(first, again);
        Assert.False(first == again);
        Assert.True(first != again);
        Assert.Equal(2, new HashSet<HealthReports> { first, again }.Count);
        Assert.DoesNotContain(first, new HashSet<HealthReports> { again });

        const string WithoutApps = """{ "last24Hours": { "reports": 24, "healthy": 23 } }""";

        Assert.Equal(Read(WithoutApps), Read(WithoutApps));
        Assert.Single(new HashSet<HealthReports> { Read(WithoutApps), Read(WithoutApps) });
    }

    // ---------------------------------------------------------- the nothing, and the part that says nothing

    /// <summary>
    /// The report that was told of nothing is a report: one with itself and with another of nothing at all, of one
    /// hash and one entry of a set, its four parts asked out as four nothings - and another report than the one of
    /// a part that is only as good as nothing: the window that counted no report, the empty list of apps, the first
    /// moment there are standing where no moment is. Asked about the nothing, the report says no: to no report at
    /// all under either type, to the boxed nothing, to the windows it is made of, to a moment and to words.
    /// </summary>
    [Fact]
    public void AReportOfNothingIsOneReportAndAnotherThanThePartThatSaysNothing()
    {
        var nothing = Reports();

        Assert.Equal(nothing, Reports());
        Assert.True(nothing == Reports());
        Assert.False(nothing != Reports());
        Assert.True(nothing.Equals(nothing));
        Assert.Equal(nothing.GetHashCode(), Reports().GetHashCode());
        Assert.Single(new HashSet<HealthReports> { nothing, Reports() });
        Assert.Equal("HealthReports { Last24Hours = , Last7Days = , LastFailure = , ContainerApps =  }", Words(nothing));

        Assert.NotEqual(nothing, Reports(day: new HealthWindow(0, 0)));
        Assert.NotEqual(nothing, Reports(week: new HealthWindow(0, 0)));
        Assert.NotEqual(nothing, Reports(apps: Array.Empty<ContainerAppReports>()));
        Assert.NotEqual(nothing, Reports(failure: DateTimeOffset.MinValue));
        Assert.NotEqual(nothing, Reports(failure: DateTimeOffset.MaxValue));
        Assert.True(nothing != Reports(day: new HealthWindow(0, 0)));

        HealthReports? absent = null;

        Assert.False(nothing.Equals(absent));
        Assert.False(nothing.Equals((HealthReports?)null));
        Assert.False(nothing.Equals((object?)null));
        Assert.False(nothing.Equals(new HealthWindow(0, 0)));
        Assert.False(nothing.Equals(Failed));
        Assert.False(nothing.Equals("HealthReports { }"));
        Assert.False(nothing.Equals(DeliveryReport.Parse("""{ "failover": { "seconds": 44 } }""")!.Failover));
    }

    // ------------------------------------------------------------------- the parts asked out

    /// <summary>
    /// The four parts come out of a report asked one after the other as they come out of it fallen apart at once -
    /// the day, the week, the moment, the apps - and what came out goes back in the same way: the report they fall
    /// apart into is the report they were, of one hash. At the ends of their numbers the parts come out at the ends:
    /// the count at the top of <see cref="int" />, the moment of the first and the last hour there are, and the
    /// apps of one app coming out as the very list that held it.
    /// </summary>
    [Fact]
    public void TheFourPartsComeOutOfAReportAskedOneAfterTheOther()
    {
        var apps = new[] { new ContainerAppReports("ui", null, null, null) };
        var reports = Reports(new HealthWindow(24, 23), new HealthWindow(166, 164), Failed, apps);

        reports.Deconstruct(out var day, out var week, out var failure, out var read);

        Assert.Equal(new HealthWindow(24, 23), day);
        Assert.Equal(new HealthWindow(166, 164), week);
        Assert.Equal(Failed, failure);
        Assert.Same(apps, read);
        Assert.Equal(reports, new HealthReports(day, week, failure, read));
        Assert.Equal(reports.GetHashCode(), new HealthReports(day, week, failure, read).GetHashCode());

        var (fallenDay, fallenWeek, fallenFailure, fallenApps) = reports;

        Assert.Equal(day, fallenDay);
        Assert.Equal(week, fallenWeek);
        Assert.Equal(failure, fallenFailure);
        Assert.Same(read, fallenApps);

        var atTheEnds = Reports(new HealthWindow(int.MaxValue, int.MaxValue), new HealthWindow(int.MinValue, int.MinValue), DateTimeOffset.MaxValue, apps);

        atTheEnds.Deconstruct(out var endDay, out var endWeek, out var endFailure, out var endApps);

        Assert.Equal(int.MaxValue, endDay!.Reports);
        Assert.Equal(int.MaxValue, endDay.Healthy);
        Assert.Equal(int.MinValue, endWeek!.Reports);
        Assert.Equal(int.MinValue, endWeek.Healthy);
        Assert.Equal(DateTimeOffset.MaxValue, endFailure);
        Assert.Same(apps, endApps);

        Reports().Deconstruct(out var noDay, out var noWeek, out var noFailure, out var noApps);

        Assert.Null(noDay);
        Assert.Null(noWeek);
        Assert.Null(noFailure);
        Assert.Null(noApps);
    }

    // --------------------------------------------------------------------- the copy, and the original

    /// <summary>
    /// A copy turns the part it names and leaves the report it was made from standing: after each of the four parts
    /// has been turned, the original still holds the very windows, the very moment and the very list it was given,
    /// and the copy of no part named is the original again in another reference and under one hash. A part told to
    /// a fresh report by its own name - the way a copy is made - lands on the property of that name and holds the
    /// instance it was told, the three parts the report was constructed with left where they stood.
    /// </summary>
    [Fact]
    public void ACopyTurnsAPartAndLeavesTheReportItCopiedStanding()
    {
        var day = new HealthWindow(24, 23);
        var week = new HealthWindow(166, 164);
        var apps = new[] { new ContainerAppReports("ui", null, null, null) };
        var original = Reports(day, week, Failed, apps);

        var turned = original with { Last24Hours = new HealthWindow(24, 24) };

        Assert.Equal(new HealthWindow(24, 24), turned.Last24Hours);
        Assert.Same(day, original.Last24Hours);
        Assert.NotSame(day, turned.Last24Hours);
        Assert.Same(week, turned.Last7Days);

        var weekless = original with { Last7Days = null };

        Assert.Null(weekless.Last7Days);
        Assert.Same(week, original.Last7Days);
        Assert.Same(day, weekless.Last24Hours);
        Assert.Equal(Failed, weekless.LastFailure);

        var undated = original with { LastFailure = null };

        Assert.Null(undated.LastFailure);
        Assert.Equal(Failed, original.LastFailure);
        Assert.Same(apps, original.ContainerApps);
        Assert.Same(apps, undated.ContainerApps);

        var appless = original with { ContainerApps = null };

        Assert.Null(appless.ContainerApps);
        Assert.Same(apps, original.ContainerApps);
        Assert.NotEqual(original, appless);
        Assert.Equal(2, new HashSet<HealthReports> { original, appless }.Count);

        // The copy that names nothing is the original again: another reference, one report, one hash.
        var unchanged = original with { };

        Assert.Equal(original, unchanged);
        Assert.NotSame(original, unchanged);
        Assert.Equal(original.GetHashCode(), unchanged.GetHashCode());
        Assert.Same(day, unchanged.Last24Hours);
        Assert.Same(apps, unchanged.ContainerApps);

        // The four parts told in one copy: the report of nothing at all, the original standing behind it.
        Assert.Equal(Reports(), original with { Last24Hours = null, Last7Days = null, LastFailure = null, ContainerApps = null });
        Assert.Same(day, original.Last24Hours);
        Assert.Same(week, original.Last7Days);

        // A part told by its own name to a fresh report is the part of the report that was made.
        var built = new HealthReports(null, null, null)
        {
            Last24Hours = day,
            Last7Days = week,
            LastFailure = FailedInOtherHours,
            ContainerApps = apps,
        };

        Assert.Same(day, built.Last24Hours);
        Assert.Same(week, built.Last7Days);
        Assert.Equal(Failed, built.LastFailure);
        Assert.Same(apps, built.ContainerApps);
        Assert.Equal(original, built);
        Assert.Equal(original.GetHashCode(), built.GetHashCode());

        // The last part told alone is the report the constructor would have made without it.
        Assert.Equal(Reports(day, week, Failed), new HealthReports(day, week, Failed) { ContainerApps = null });
    }

    // ---------------------------------------------------------------------- the record's own words

    /// <summary>
    /// The report's own words are the viewer's where a moment is said and nobody else's elsewhere: a reader of
    /// German is told the failure as <c>06.10.2026 16:13:00 +00:00</c> where the reader of the invariant culture is
    /// told <c>10/06/2026 16:13:00 +00:00</c>, while the four names, the windows inside them, the hours the moment
    /// stands on and the kind of list the apps stand in are said the same to every reader - and the parts a report
    /// does not have say their names and nothing more in either hours. The two readers are told one word but the
    /// moment, and a moment bent is no longer for the bending. These are the record's words, which no card shows:
    /// the invariant words of these very reports are pinned by <see cref="HealthReportsEdgeCaseTests" />, the line
    /// the page says by <see cref="AvailabilityText" />.
    /// </summary>
    [Fact]
    public void TheReportSaysItsMomentInTheHoursTheViewerSpeaks()
    {
        var apps = new[] { new ContainerAppReports("ui", null, null, null) };
        var reports = Reports(new HealthWindow(24, 23), new HealthWindow(166, 164), Failed, apps);
        var zero = Reports(new HealthWindow(0, 0), null, DateTimeOffset.MinValue, null);

        var forTheGerman = string.Empty;
        ViewerCulture.German(() => forTheGerman = reports.ToString());

        Assert.Equal(
            "HealthReports { Last24Hours = HealthWindow { Reports = 24, Healthy = 23, AllHealthy = False }, "
            + "Last7Days = HealthWindow { Reports = 166, Healthy = 164, AllHealthy = False }, LastFailure = 06.10.2026 16:13:00 +00:00, "
            + "ContainerApps = Dashboard.Health.ContainerAppReports[] }",
            forTheGerman);

        // One word but the moment: the invariant reader is told the same letters, the meridian kept, the apps
        // named by the array they stand in - and nothing of the reader's is longer than the other's.
        Assert.Contains("LastFailure = 10/06/2026 16:13:00 +00:00", Words(reports), StringComparison.Ordinal);
        Assert.EndsWith(", ContainerApps = Dashboard.Health.ContainerAppReports[] }", Words(reports), StringComparison.Ordinal);
        Assert.EndsWith(", ContainerApps = Dashboard.Health.ContainerAppReports[] }", forTheGerman, StringComparison.Ordinal);
        Assert.NotEqual(Words(reports), forTheGerman);
        Assert.Equal(Words(reports).Length, forTheGerman.Length);

        // The window that counted nothing and the first moment there are are said as they are in either hours.
        var zeroForTheGerman = string.Empty;
        ViewerCulture.German(() => zeroForTheGerman = zero.ToString());

        Assert.Equal(
            "HealthReports { Last24Hours = HealthWindow { Reports = 0, Healthy = 0, AllHealthy = True }, Last7Days = , "
            + "LastFailure = 01.01.0001 00:00:00 +00:00, ContainerApps =  }",
            zeroForTheGerman);
        Assert.Contains("LastFailure = 01/01/0001 00:00:00 +00:00", Words(zero), StringComparison.Ordinal);

        // The hours the moment stands on are no reader's to bend: five hours east, the reader's order and the
        // writer's hours; the parts the report does not have say their names and nothing more.
        var undated = Reports(failure: FailedInOtherHours);

        ViewerCulture.German(() => Assert.Equal(
            "HealthReports { Last24Hours = , Last7Days = , LastFailure = 06.10.2026 21:13:00 +05:00, ContainerApps =  }",
            undated.ToString()));
        Assert.Contains("LastFailure = 10/06/2026 21:13:00 +05:00", Words(undated), StringComparison.Ordinal);
    }

    /// <summary>
    /// The apps say what list they stand in rather than what they count: the array of one app and the list of that
    /// very app are said in two words - <c>Dashboard.Health.ContainerAppReports[]</c> and
    /// <c>System.Collections.Generic.List`1[Dashboard.Health.ContainerAppReports]</c> - and are two reports, the
    /// one list being no place the other stands in. Neither word names an app.
    /// </summary>
    [Fact]
    public void TheAppsSayWhatListTheyStandInRatherThanWhatTheyCount()
    {
        var app = new ContainerAppReports("ui", null, null, null);
        var asArray = Reports(apps: new[] { app });
        var asList = Reports(apps: new List<ContainerAppReports> { app });

        Assert.Contains("ContainerApps = Dashboard.Health.ContainerAppReports[] }", Words(asArray), StringComparison.Ordinal);
        Assert.Contains("ContainerApps = System.Collections.Generic.List`1[Dashboard.Health.ContainerAppReports] }", Words(asList), StringComparison.Ordinal);
        Assert.DoesNotContain("ContainerAppReports[] }", Words(asList), StringComparison.Ordinal);
        Assert.DoesNotContain("ui", Words(asArray), StringComparison.Ordinal);
        Assert.DoesNotContain("ui", Words(asList), StringComparison.Ordinal);
        Assert.NotEqual(asArray, asList);
        Assert.Equal(Assert.Single(asArray.ContainerApps!), Assert.Single(asList.ContainerApps!));
    }

    /// <summary>
    /// A moment's smallest part is no part of the words: the report of the failure at the tick and the report of
    /// that failure one tick later say one line and are two reports, of two hashes - so the words are no way of
    /// telling two readings of one second apart apart, which is what the record's equality is for.
    /// </summary>
    [Fact]
    public void TwoReportsATickApartSayOneLineAndAreTwoReports()
    {
        var atTheTick = Reports(failure: Failed);
        var oneTickLater = Reports(failure: Failed.AddTicks(1));

        Assert.Equal(Words(atTheTick), Words(oneTickLater));
        Assert.NotEqual(atTheTick, oneTickLater);
        Assert.Equal(2, new HashSet<HealthReports> { atTheTick, oneTickLater }.Count);
    }

    // ---------------------------------------------------------------------------------- the machinery

    /// <summary>The public members the report declares itself: its parts and the methods that are not the get and
    /// the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(HealthReports)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words a report says, taken in the invariant culture whatever the reader of this test happens
    /// to be: the moment is the reader's to say, and the record's own words are only comparable one way.</summary>
    private static string Words(HealthReports reports)
    {
        ArgumentNullException.ThrowIfNull(reports);

        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            return reports.ToString();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }
}
