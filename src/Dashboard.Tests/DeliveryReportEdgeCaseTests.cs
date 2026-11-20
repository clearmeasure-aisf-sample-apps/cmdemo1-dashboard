namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="DeliveryReport" /> and everything
/// <c>DeliveryReport.cs</c> declares with it - the lookups of an environment and its entries, the reading of the
/// file, <see cref="HealthWindow.AllHealthy" />, and the words of <see cref="AvailabilityText" /> and
/// <see cref="DeliveryText" /> - at the ends the tests of the shipped file leave open.
/// <para>
/// What is pinned where it belongs: <see cref="DeliveryTests" /> pins the delivery file the optics read entry by
/// entry, a file that is no delivery file at all, every part of an entry being optional, a sign-off by a person with
/// and without its parts, the four units of a lead time, the nine sayings of "behind", the counts of the last seven
/// days, the failover test in its three parts, the others of an environment, and the monitor reading the file over
/// the network; <see cref="DeliveryEnvironmentTests" /> pins an environment's three parts, the case the lookups are
/// asked in, a name no environment has, the file without environments and without a failover test, and the name of
/// the system project; <see cref="AvailabilityTests" /> pins the reading of the health reports, the counts that
/// cannot be, the line of a day and a week with the last failure, the parts with their tooltip and their moment, the
/// window without a report, the seven percents, and the container apps of an environment;
/// <see cref="HealthHelperCoverageTests" /> pins <c>AvailabilityText.Count</c> to the letter - the two orders, the
/// invariant point, the window that is not there - and <see cref="ClusterDeliveryHelperTests" /> pins
/// <c>DeliveryText.BehindHelp</c>, <c>DeliveryReport.IsSystem</c> in eleven names and <c>DeliveryReport.SystemName</c>.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the file whose root is a number, a word, a boolean, the word for
/// nothing or a document cut off, the <c>generated</c> that is no moment while the facts stand, the
/// <c>deployables</c> of a kind that is no array, the <c>failover</c> object that says nothing, the numbers a file
/// gives that are negative, fractional or too big for a real number, the names and the versions read with their
/// whitespace and their build metadata; the two entries that share a name and the one the lookups answer with, the
/// names that are not there at all, the shown list that is not there, the system entry named in another case among
/// the others that keep the file's order, and the health section that names its container apps while counting no
/// window; that a report is its three parts and that two readings of one file are not one report; that a window is
/// all healthy by its two counts and nothing else, that a percent the rounding would carry to all is said as just
/// under all, and that a window the file could not give is still counted and still said; the week counted alone and
/// all healthy, the last failure said with no window behind it, the tooltip and the two counts refusing what is not
/// there, the hours of a viewer five and a half east of the meridian, and the point a German reader sees no comma at;
/// the lead time at its two changes of unit, the failed deployments among one deployment and among none, the
/// failover test that says nothing, the days rounded whole before they are counted, and the words of a card whose
/// entry is named in another case or not named at all.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every moment is one the test brings.
/// The reading of the file over HTTP is the monitor's, and <see cref="DeliveryTests" /> pins it with fake time.
/// </para>
/// </summary>
public class DeliveryReportEdgeCaseTests
{
    /// <summary>The moment the page is asked, and every "ago" below is counted from: a fixed moment, not the clock.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 7, 1, 30, 0, TimeSpan.Zero);

    /// <summary>A moment nine hours and seventeen minutes before it: <c>9 h ago</c>, and not a whole number of hours.</summary>
    private static readonly DateTimeOffset Failed = new(2026, 10, 6, 16, 13, 0, TimeSpan.Zero);

    /// <summary>The moment the delivery facts last changed, in the hours the file writes them in.</summary>
    private static readonly DateTimeOffset Generated = new(2026, 10, 7, 1, 0, 0, TimeSpan.Zero);

    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    private static readonly TimeZoneInfo EastFiveThirty =
        TimeZoneInfo.CreateCustomTimeZone("east-five-thirty", TimeSpan.FromMinutes(330), "East Five Thirty", "East Five Thirty");

    private const string Delivery = """
        { "generated": "2026-10-07T01:00:00Z",
          "environments": [
            { "name": "tdd", "deployables": [ { "name": "ui", "version": "2.4.15" } ] },
            { "name": "uat",
              "deployables": [
                { "name": "dashboard", "version": "1.0.7" },
                { "name": "system", "version": "1.0.33" },
                { "name": "ui", "version": "2.4.14" } ],
              "health": { "last24Hours": { "reports": 24, "healthy": 23 }, "last7Days": { "reports": 166, "healthy": 164 },
                          "lastFailure": "2026-10-06T16:13:00Z" } } ],
          "failover": { "environment": "uat", "at": "2026-10-06T08:30:00Z", "seconds": 44 } }
        """;

    private static DeliveryReport Report(string file) => DeliveryReport.Parse(file)!;

    private static DeliveryEntry Entry(int? deployments, int? failed) =>
        new("ui", null, null, null, null, null, null, null, null, deployments, failed, null);

    // ---------------------------------------------------------------- the file, at the ends of what a file may say

    /// <summary>
    /// A delivery file is the object it is written as: the document cut off, the root that is a number, a word, a
    /// boolean or the word for nothing, and the half-written list are no report rather than an error - the page keeps
    /// showing the last file it could read.
    /// </summary>
    [Theory]
    [InlineData("{")]
    [InlineData("{ \"environments\": [")]
    [InlineData("[1, 2")]
    [InlineData("nul")]
    [InlineData("123")]
    [InlineData("1.5")]
    [InlineData("true")]
    [InlineData("null")]
    [InlineData("\"delivery\"")]
    public void ADocumentThatIsNoObjectIsNoReport(string file) => Assert.Null(DeliveryReport.Parse(file));

    /// <summary>
    /// The moment the file cannot say is no moment, and nothing else is lost with it: <c>generated</c> spelled as a
    /// word and <c>generated</c> written as a number leave the environments, their entries and the failover test
    /// standing, so the line is said without the facts ever being dated.
    /// </summary>
    [Theory]
    [InlineData("""{ "generated": "yesterday", "environments": [ { "name": "uat", "deployables": [ { "name": "ui", "version": "2.4.14" } ] } ] }""")]
    [InlineData("""{ "generated": 21, "environments": [ { "name": "uat", "deployables": [ { "name": "ui", "version": "2.4.14" } ] } ] }""")]
    [InlineData("""{ "generated": "2026-13-04T21:00:00Z", "environments": [ { "name": "uat", "deployables": [ { "name": "ui", "version": "2.4.14" } ] } ] }""")]
    public void AMomentThatIsNoMomentLeavesTheFactsStandingWithoutOne(string file)
    {
        var report = Report(file);

        Assert.Null(report.Generated);
        Assert.Equal("2.4.14", Assert.Single(report.Environments).Deployables.Single().Version);
        Assert.Equal("2.4.14", report.Find("uat", "ui")!.Version);
    }

    /// <summary>
    /// The deployables of a kind that is no array are no deployables: an environment whose <c>deployables</c> is the
    /// word for nothing, an object, a number or a word keeps its own name, its health and its place in the file, and
    /// holds no entry - which is what a page shows as an environment that delivered nothing.
    /// </summary>
    [Theory]
    [InlineData("""{ "name": "uat", "deployables": null, "health": { "last24Hours": { "reports": 24, "healthy": 24 } } }""")]
    [InlineData("""{ "name": "uat", "deployables": { "ui": "2.4.14" } }""")]
    [InlineData("""{ "name": "uat", "deployables": 7 }""")]
    [InlineData("""{ "name": "uat", "deployables": "ui" }""")]
    [InlineData("""{ "name": "uat", "deployables": [ null, 7, "ui", [ ] ] }""")]
    public void DeployablesOfAKindThatIsNoArrayAreNoDeployables(string environment)
    {
        var report = Report($$"""{ "environments": [ {{environment}} ] }""");

        var uat = Assert.Single(report.Environments);
        Assert.Equal("uat", uat.Name);
        Assert.Empty(uat.Deployables);
        Assert.Null(report.Find("uat", "ui"));
        Assert.Empty(report.Others("uat", []));
    }

    /// <summary>
    /// The environment that holds them reads its deployables trimmed of the whitespace the file may pad them with,
    /// and its version without the build metadata after the <c>+</c>: a version of nothing but the mark a release
    /// carries is no version, and neither is the whitespace that only looks like one.
    /// </summary>
    [Fact]
    public void TheNamesAndTheVersionsOfAnEntryAreReadTrimmedAndWithoutTheirBuild()
    {
        var report = Report("""
            { "environments": [ { "name": " uat ", "deployables": [
              { "name": " ui ", "version": " 2.4.14+abc123 " },
              { "name": "api", "version": "+" },
              { "name": "worker", "version": "   " } ] } ] }
            """);

        Assert.Equal("uat", Assert.Single(report.Environments).Name);
        Assert.Equal("2.4.14", report.Find("uat", "UI")!.Version);
        Assert.Equal("ui", report.Find("uat", "ui")!.Name);
        Assert.Null(report.Find("uat", "api")!.Version);
        Assert.Null(report.Find("uat", "worker")!.Version);
    }

    /// <summary>
    /// A failover test that names no environment, no moment and no seconds is no failover test, and the object that
    /// holds nothing at all is one too: the report is read for what it says, and the card leaves the line out rather
    /// than saying a test happened at some unknown time.
    /// </summary>
    [Fact]
    public void AFailoverTestThatSaysNothingIsNoFailoverTest()
    {
        var report = Report("""{ "environments": [ { "name": "uat", "deployables": [] } ], "failover": { } }""");

        Assert.Null(report.Failover);
        Assert.Equal("uat", Assert.Single(report.Environments).Name);

        Assert.Equal(
            new FailoverTest("uat", null, null),
            Report("""{ "failover": { "environment": "uat", "at": null, "seconds": null } }""").Failover);
        Assert.Equal(
            new FailoverTest(null, null, 44),
            Report("""{ "failover": { "at": "not a moment", "seconds": 44 } }""").Failover);
        Assert.Null(DeliveryReport.Parse("""{ "failover": { } }"""));
    }

    /// <summary>
    /// The numbers a file gives that cannot be are no numbers: a lead time, a count of deployments or a distance
    /// behind that is negative, that is written as a word, or that is too big for a real number to hold is left out
    /// with the same blank the file leaves by not writing the key at all - while a count the file writes as a
    /// fraction is read as the whole it rounds to, which is what a count of deployments has to be.
    /// </summary>
    [Fact]
    public void TheNumbersAFileCannotGiveAreNoNumbers()
    {
        var report = Report("""
            { "environments": [ { "name": "uat", "deployables": [
              { "name": "negative", "leadTimeHours": -1, "deploymentsLast7Days": -2, "failedLast7Days": -1,
                "behindFirst": { "versions": "2", "days": -0.5 } },
              { "name": "written", "leadTimeHours": "5.2", "deploymentsLast7Days": "4", "failedLast7Days": true },
              { "name": "infinite", "leadTimeHours": 1e1000, "deploymentsLast7Days": 1e1000 },
              { "name": "whole", "behindFirst": 7 },
              { "name": "fraction", "leadTimeHours": 0.4, "deploymentsLast7Days": 3.6, "failedLast7Days": 0.4 } ] } ] }
            """);

        var negative = report.Find("uat", "negative")!;
        Assert.Null(negative.LeadTimeHours);
        Assert.Null(negative.DeploymentsLast7Days);
        Assert.Null(negative.FailedLast7Days);
        Assert.Equal(new BehindFirst(null, null), negative.Behind);
        Assert.Null(DeliveryText.Behind(negative.Behind, "uat", "tdd"));

        Assert.Null(report.Find("uat", "written")!.LeadTimeHours);
        Assert.Null(report.Find("uat", "written")!.DeploymentsLast7Days);
        Assert.Null(report.Find("uat", "written")!.FailedLast7Days);

        // The number no real number holds and the count no count of anything is are both read past.
        Assert.Null(report.Find("uat", "infinite")!.LeadTimeHours);
        Assert.Null(report.Find("uat", "infinite")!.DeploymentsLast7Days);

        // A section that is no section is no distance, while a section of no numbers is a distance of nothing said.
        Assert.Null(report.Find("uat", "whole")!.Behind);

        var fraction = report.Find("uat", "fraction")!;
        Assert.Equal(0.4, fraction.LeadTimeHours!.Value);
        Assert.Equal(4, fraction.DeploymentsLast7Days!.Value);
        Assert.Equal(0, fraction.FailedLast7Days!.Value);
        Assert.Equal("4 deployments, none failed", DeliveryText.Frequency(fraction));
    }

    // ---------------------------------------------------------------- the lookups, at the ends of a name

    /// <summary>
    /// A name that is not there finds no entry: the name that is no name, the name that is empty and the name of an
    /// environment the file holds none of are all asked the same way, and none of them is answered with the first
    /// entry that comes - the lookup compares whole names, in any case, and nothing else.
    /// </summary>
    [Fact]
    public void ANameThatIsNotThereFindsNoEntry()
    {
        var report = Report(Delivery);

        Assert.Null(report.Find(null!, null!));
        Assert.Null(report.Find(null!, "ui"));
        Assert.Null(report.Find("uat", null!));
        Assert.Null(report.Find(string.Empty, string.Empty));
        Assert.Null(report.Find(" uat", "ui"));
        Assert.Null(report.Find("uat ", "ui"));
        Assert.Null(report.Find("uat", "u"));
        Assert.Null(report.Find("uat", "ui2"));
        Assert.Null(report.HealthOf(null!));
        Assert.Null(report.HealthOf(string.Empty));
        Assert.Null(report.ContainerAppsOf(" uat"));
        Assert.Null(report.ContainerAppsOf("uat "));
    }

    /// <summary>
    /// Two environments that share a name are the one that comes first in the file, and so are two entries of one
    /// environment: the lookup walks the file and answers with what it meets first, and the second stays in
    /// <c>Environments</c> where a reader can see it - it is not quietly dropped on the way in.
    /// </summary>
    [Fact]
    public void TheEntriesThatShareANameAreKeptInTheOrderOfTheFile()
    {
        var report = Report("""
            { "environments": [
              { "name": "uat", "deployables": [ { "name": "ui", "version": "2.4.14" }, { "name": "UI", "version": "2.4.15" } ] },
              { "name": "UAT", "deployables": [ { "name": "ui", "version": "9.9.9" } ],
                "health": { "last24Hours": { "reports": 24, "healthy": 24 } } } ] }
            """);

        Assert.Equal(["uat", "UAT"], report.Environments.Select(environment => environment.Name));

        var first = report.Environments[0].Deployables[0];
        Assert.Same(first, report.Find("uat", "ui"));
        Assert.Same(first, report.Find("UAT", "UI"));
        Assert.Equal("2.4.14", report.Find("uat", "ui")!.Version);

        // The second environment of the name is asked for nothing: the health and the others come from the first.
        Assert.Null(report.HealthOf("uat"));
        Assert.Null(report.ContainerAppsOf("uat"));
        Assert.Equal(["ui", "UI"], report.Others("uat", []).Select(entry => entry.Name));
    }

    /// <summary>
    /// The others put the entry of the system project first whichever case it is named in, keep the file's order for
    /// the rest, and are asked with a list that may repeat names, name them in another case or name the empty one -
    /// while a shown list of nothing shows all of them and a shown list of all of them leaves nothing to show.
    /// </summary>
    [Fact]
    public void TheOthersPutTheSystemFirstAndKeepTheFilesOrderAmongTheRest()
    {
        var report = Report("""
            { "environments": [ { "name": "uat", "deployables": [
              { "name": "api" }, { "name": "SYSTEM" }, { "name": "ui" }, { "name": "worker" } ] } ] }
            """);

        Assert.Equal(["SYSTEM", "api", "ui", "worker"], report.Others("uat", []).Select(entry => entry.Name));
        Assert.Equal(["SYSTEM", "api", "worker"], report.Others("uat", ["UI"]).Select(entry => entry.Name));
        Assert.Equal(["SYSTEM", "worker"], report.Others("uat", ["ui", "Api", "UI", string.Empty]).Select(entry => entry.Name));
        Assert.Equal(["api", "worker"], report.Others("uat", ["system", "SYSTEM", "System", "ui"]).Select(entry => entry.Name));
        Assert.Empty(report.Others("uat", ["api", "system", "ui", "worker"]));
        Assert.Empty(report.Others("Uat", ["API", "System", "UI", "WORKER"]));
    }

    /// <summary>
    /// The shown list that is not there is refused by the name of the parameter the guard keeps: a page that cannot
    /// say which entries it shows has no answer to give, and the report does not take a missing list for one that
    /// shows nothing.
    /// </summary>
    [Fact]
    public void TheShownListThatIsNotThereIsRefusedByName() =>
        Assert.Equal("shown", Assert.Throws<ArgumentNullException>(() => Report(Delivery).Others("uat", null!)).ParamName);

    /// <summary>
    /// The two windows are what makes a health section a health report: a section that names what its reports found
    /// for every container app while counting neither the day nor the week is no report at all, and the container
    /// apps are said with it and not without it - the file that has no counts has no verdict either.
    /// </summary>
    [Fact]
    public void AHealthSectionThatCountsNoWindowIsNoHealthReport()
    {
        var report = Report("""
            { "environments": [ { "name": "uat", "deployables": [],
              "health": { "lastFailure": "2026-10-06T16:13:00Z",
                          "containerApps": [ { "name": "ui", "last": { "result": "healthy", "at": "2026-10-07T01:02:13Z" } } ] } } ] }
            """);

        Assert.Null(report.HealthOf("uat"));
        Assert.Null(report.ContainerAppsOf("uat"));
        Assert.Null(report.Environments[0].Health);
        Assert.Empty(report.Environments[0].Deployables);
    }

    // ---------------------------------------------------------------- what a report is

    /// <summary>
    /// A report is its three parts and nothing else: the <c>with</c> that names no part copies it whole, the copy is
    /// not the same report standing in the same place, and a difference in any one of the three parts is another
    /// report - while two readings of one file are not one report, since the environments and the entries they carry
    /// are the lists that file was read into, compared as lists and not as their contents.
    /// </summary>
    [Fact]
    public void TwoReadingsOfOneFileAreTwoReportsOfTheSameFacts()
    {
        var one = Report(Delivery);
        var again = Report(Delivery);

        Assert.Equal(one.Generated, again.Generated);
        Assert.Equal(one.Failover, again.Failover);
        Assert.Equal(one.Environments.Count, again.Environments.Count);

        // An entry is its parts and is read equal again; an environment keeps the list it was read into, and a
        // report keeps the environments it was read into - so two readings of one file are two reports.
        Assert.Equal(one.Environments[1].Deployables, again.Environments[1].Deployables);
        Assert.NotEqual(one.Environments[0], again.Environments[0]);
        Assert.NotEqual(one.Environments, again.Environments);
        Assert.NotSame(one.Environments, again.Environments);
        Assert.NotEqual(one, again);

        var copy = one with { };
        Assert.Equal(one, copy);
        Assert.True(one == copy);
        Assert.False(one != copy);
        Assert.NotSame(one, copy);
        Assert.Equal(one.GetHashCode(), copy.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.False(one.Equals(Delivery));

        Assert.NotEqual(one, one with { Generated = null });
        Assert.NotEqual(one, one with { Failover = null });
        Assert.NotEqual(one, one with { Environments = [] });

        var (generated, environments, failover) = one;
        Assert.Equal(Generated, generated);
        Assert.Same(one.Environments, environments);
        Assert.Equal(one.Failover, failover);
    }

    /// <summary>
    /// Saying nothing about the container apps and saying that there are none are two different reports: the default
    /// of the part the file leaves out is no list, the list the file writes empty is an empty one, and a report is
    /// equal to another only while the two parts are - which is why the page says "the file does not say" and not
    /// "this system runs no container app" out of the same words.
    /// </summary>
    [Fact]
    public void AHealthReportThatSaysNothingAndOneThatSaysNoneAreTwo()
    {
        var day = new HealthWindow(24, 23);
        var week = new HealthWindow(166, 164);

        var silent = new HealthReports(day, week, Failed);
        var written = new HealthReports(day, week, Failed, null);

        Assert.Null(silent.ContainerApps);
        Assert.Equal(silent, written);

        var none = new HealthReports(day, week, Failed, []);
        Assert.Empty(none.ContainerApps!);
        Assert.NotEqual(silent, none);
        Assert.NotEqual(silent, silent with { LastFailure = null });
        Assert.NotEqual(silent, silent with { Last24Hours = null });
        Assert.NotEqual(silent, silent with { Last7Days = new HealthWindow(166, 165) });

        // The two windows are what a health report is: either of them, and not both, is a report still.
        Assert.Equal(new HealthReports(day, null, null), new HealthReports(day, null, null, null));
        Assert.NotEqual(new HealthReports(day, null, null), new HealthReports(null, week, null));
    }

    // ---------------------------------------------------------------- a window's own verdict

    /// <summary>
    /// A window is all healthy while every report it counted passed, which is the two counts being one number: the
    /// window that counted nothing is all healthy with it, since nothing failed - and the line does not draw the
    /// conclusion out of it until it knows reports were asked, which <see cref="AvailabilityTests" /> pins.
    /// </summary>
    [Theory]
    [InlineData(24, 24, true)]
    [InlineData(1, 1, true)]
    [InlineData(0, 0, true)]
    [InlineData(168, 168, true)]
    [InlineData(24, 23, false)]
    [InlineData(1, 0, false)]
    [InlineData(0, 1, false)]
    [InlineData(24, 25, false)]
    public void AWindowIsAllHealthyWhileItsTwoCountsAreOneNumber(int reports, int healthy, bool expected) =>
        Assert.Equal(expected, new HealthWindow(reports, healthy).AllHealthy);

    /// <summary>
    /// The window the file refuses to read is still a window to whoever holds one: more healthy than there were
    /// reports is counted as it is written and said as it is counted - <c>25 of 24</c>, and a percent over all -
    /// since nothing of the words pretends to be a check on the numbers.
    /// </summary>
    [Fact]
    public void AWindowTheFileCouldNotGiveIsCountedAndSaidAsItStands()
    {
        var impossible = new HealthWindow(24, 25);

        Assert.Equal("25 of 24", AvailabilityText.Count(impossible));
        Assert.Equal("104.2 %", AvailabilityText.Percent(impossible));
        Assert.False(impossible.AllHealthy);
    }

    /// <summary>
    /// The percent a single failure would round up to is not said as all: ten thousand reports of which one failed
    /// are <c>99.9 %</c> and never <c>100 %</c>, and a share of a whole that is not whole is said with its decimal
    /// kept - the rounding of the invariant culture and nothing else deciding it.
    /// </summary>
    [Theory]
    [InlineData(10_000, 9_999, "99.9 %")]
    [InlineData(6, 2, "33.3 %")]
    [InlineData(3, 2, "66.7 %")]
    [InlineData(8, 7, "87.5 %")]
    [InlineData(7, 7, "100 %")]
    [InlineData(1, 1, "100 %")]
    [InlineData(2, 0, "0 %")]
    public void APercentIsRoundedToADecimalAndNeverOntoAllOrNone(int reports, int healthy, string expected) =>
        Assert.Equal(expected, AvailabilityText.Percent(new HealthWindow(reports, healthy)));

    /// <summary>
    /// The two counts and the line refuse what is not there by the name of the parameter the guard keeps: a window
    /// no report was read for is not <c>0 of 0</c>, and reports the file holds no health section for are not the
    /// reports of an environment that saw nothing.
    /// </summary>
    [Fact]
    public void TheCountsAndTheLineRefuseWhatIsNotThere()
    {
        Assert.Equal("window", Assert.Throws<ArgumentNullException>(() => AvailabilityText.Percent(null!)).ParamName);
        Assert.Equal("reports", Assert.Throws<ArgumentNullException>(() => AvailabilityText.Parts(null!, Now)).ParamName);
    }

    /// <summary>
    /// The tooltip refuses a zone the caller has not got, and writes the moment it is given in the hours of the one
    /// it was given: the same change of the facts is <c>01:00 +00:00</c> on the meridian and <c>06:30 +05:30</c>
    /// five and a half hours east of it, in the sentence that says who counted.
    /// </summary>
    [Fact]
    public void TheTooltipWritesTheMomentItIsGivenInTheHoursOfTheZoneItIsGiven()
    {
        Assert.Equal("zone", Assert.Throws<ArgumentNullException>(() => AvailabilityText.Help(Generated, null!)).ParamName);

        Assert.EndsWith(
            "As of the last change of the delivery facts: 2026-10-07 01:00 +00:00.",
            AvailabilityText.Help(Generated, Utc),
            StringComparison.Ordinal);
        Assert.Equal(
            "Counted by the pipeline: once an hour the runbook \"Health report\" in Octopus Deploy asks every node of "
            + "the environment and its public address, and the report fails when one is not healthy. It is no continuous "
            + "monitoring: an outage between two reports is not counted. As of the last change of the delivery facts: "
            + "2026-10-07 06:30 +05:30.",
            AvailabilityText.Help(Generated.ToOffset(TimeSpan.FromMinutes(330)), EastFiveThirty));

        // The same instant spelled in another viewer's hours is the same sentence: the moment travels, the words do not.
        Assert.Equal(
            AvailabilityText.Help(Generated, EastFiveThirty),
            AvailabilityText.Help(Generated.ToOffset(TimeSpan.FromHours(-5)), EastFiveThirty));
    }

    /// <summary>
    /// The week counted alone and all healthy says what it counted, in its own words and not in the short form it
    /// takes after the day - and the failure no report of the seven days names is said as none, which is the part
    /// <see cref="AvailabilityTests" /> pins only with the day counted as well.
    /// </summary>
    [Fact]
    public void TheWeekAloneAndAllHealthyCountsItselfAndNamesNoFailure()
    {
        var reports = new HealthReports(null, new HealthWindow(168, 168), null);

        Assert.Equal(
            [
                new AvailabilityPart("Healthy in ", "168 of 168", " hourly checks (100 %) in 7 days"),
                new AvailabilityPart("no failure in ", "7 days", string.Empty),
            ],
            AvailabilityText.Parts(reports, Now));
        Assert.Equal(
            "Healthy in 168 of 168 hourly checks (100 %) in 7 days · no failure in 7 days",
            AvailabilityText.Line(reports, Now));

        // The day alone and all healthy names no failure: the week is what the claim covers.
        Assert.Single(AvailabilityText.Parts(new HealthReports(new HealthWindow(24, 24), null, null), Now));
    }

    /// <summary>
    /// A last failure with no window behind it is said for itself: the part carries the moment it names, the line is
    /// that moment and nothing before it, and the claim of no failure in seven days is not made on the strength of
    /// seven days nobody counted.
    /// </summary>
    [Fact]
    public void ALastFailureWithoutAnyWindowIsSaidAlone()
    {
        var reports = new HealthReports(null, null, Failed);

        Assert.Equal(
            [new AvailabilityPart("last failure ", "9 h ago", string.Empty, When: Failed)],
            AvailabilityText.Parts(reports, Now));
        Assert.Equal("last failure 9 h ago", AvailabilityText.Line(reports, Now));

        // A failure of the last minute is not yet a minute old, and one ahead of the moment asked is "just now" too.
        Assert.Equal(
            "last failure just now",
            AvailabilityText.Line(new HealthReports(null, null, Now.AddSeconds(-30)), Now));
        Assert.Equal(
            "last failure just now",
            AvailabilityText.Line(new HealthReports(null, null, Now.AddHours(3)), Now));
        Assert.Equal(
            "last failure 1 min ago",
            AvailabilityText.Line(new HealthReports(null, null, Now.AddMinutes(-1)), Now));
    }

    /// <summary>
    /// The numbers are written for the dashboard and not for the viewer: a reader of German sees <c>95.8 %</c>,
    /// <c>5.2 h</c> and <c>44.5 s</c> with the point the delivery file is read with, and the thousand that carries no
    /// separator at all.
    /// </summary>
    [Fact]
    public void TheNumbersKeepTheInvariantPointForAGermanReader() =>
        ViewerCulture.German(() =>
        {
            Assert.Equal("95.8 %", AvailabilityText.Percent(new HealthWindow(24, 23)));
            Assert.Equal("1234 of 10000", AvailabilityText.Count(new HealthWindow(10_000, 1_234)));
            Assert.Equal("5.2 h", DeliveryText.LeadTime(5.2));
            Assert.Equal(
                "the standby answered after 44.5 s",
                DeliveryText.Failover(new FailoverTest(null, null, 44.5), Now));
            Assert.Equal("2 versions, 3 days behind tdd", DeliveryText.Behind(new BehindFirst(2, 3.4), "uat", "tdd"));
        });

    // ---------------------------------------------------------------- the words of an entry, at the ends of their units

    /// <summary>
    /// A lead time changes its unit at the hour and at the two days: the minutes of what is under an hour, rounding
    /// up to a whole minute and to the sixty of them, the hours of what is under two days with the tenth that
    /// disappears into the whole, and the days of everything from forty-eight hours on - which is the hour the file
    /// writes as <c>48</c> read as two days and not as the forty-eighth hour.
    /// </summary>
    [Theory]
    [InlineData(0, "0 min")]
    [InlineData(0.008, "0 min")]
    [InlineData(0.01, "1 min")]
    [InlineData(0.5, "30 min")]
    [InlineData(0.999, "60 min")]
    [InlineData(1, "1 h")]
    [InlineData(1.04, "1 h")]
    [InlineData(1.5, "1.5 h")]
    [InlineData(47.999, "48 h")]
    [InlineData(48, "2 d")]
    [InlineData(48.1, "2 d")]
    [InlineData(71.9, "3 d")]
    public void ALeadTimeChangesItsUnitAtTheHourAndAtTheDay(double hours, string expected) =>
        Assert.Equal(expected, DeliveryText.LeadTime(hours));

    /// <summary>
    /// The seven days are counted with the failures among them in the one deployment and in the none at all:
    /// <c>1 deployment, none failed</c> keeps the singular it is said with, <c>0 deployments, 1 failed</c> says the
    /// failure the count of none does not hide, and the failed ones the file does not give leave the deployments to
    /// stand alone - while an entry that is not there is refused by the name of the parameter the guard keeps.
    /// </summary>
    [Theory]
    [InlineData(1, 0, "1 deployment, none failed")]
    [InlineData(1, 1, "1 deployment, 1 failed")]
    [InlineData(0, 1, "0 deployments, 1 failed")]
    [InlineData(0, null, "0 deployments")]
    [InlineData(2, null, "2 deployments")]
    [InlineData(2, 0, "2 deployments, none failed")]
    [InlineData(2, 2, "2 deployments, 2 failed")]
    public void TheDeploymentsOfTheLastSevenDaysAreSaidWithWhateverFailed(int? deployments, int? failed, string expected) =>
        Assert.Equal(expected, DeliveryText.Frequency(Entry(deployments, failed)));

    /// <summary>
    /// The frequency refuses an entry that is not there: no deployments and no failed ones are not the same as
    /// nothing was deployed, and the page says nothing rather than guessing which of the two the file meant.
    /// </summary>
    [Fact]
    public void TheFrequencyRefusesAnEntryThatIsNotThere() =>
        Assert.Equal("entry", Assert.Throws<ArgumentNullException>(() => DeliveryText.Frequency(null!)).ParamName);

    /// <summary>
    /// The days behind are whole before they are counted: what is under half a day is said as the two environments
    /// running the same release, what is over it as a day, and the halves rounded to the even whole the page has
    /// always said with - so that <c>1.5</c> and <c>2.5</c> are both days that were lived through and neither is
    /// invented a second one. A negative distance, which no file can give, counts no versions at all and says
    /// nothing when no days are there with it.
    /// </summary>
    [Theory]
    [InlineData(null, 0.5, "same as tdd")]
    [InlineData(null, 0.4999, "same as tdd")]
    [InlineData(null, 0.6, "1 day behind tdd")]
    [InlineData(null, 1.4, "1 day behind tdd")]
    [InlineData(null, 1.5, "2 days behind tdd")]
    [InlineData(null, 2.5, "2 days behind tdd")]
    [InlineData(null, 3.5, "4 days behind tdd")]
    [InlineData(-3, null, null)]
    [InlineData(-3, 0.5, "same as tdd")]
    [InlineData(-3, 5d, "5 days behind tdd")]
    [InlineData(1, 0.99, "1 version, 1 day behind tdd")]
    [InlineData(2, 0.5, "2 versions behind tdd")]
    public void TheDaysBehindAreWholeBeforeTheyAreSaid(int? versions, double? days, string? expected) =>
        Assert.Equal(expected, DeliveryText.Behind(new BehindFirst(versions, days), "uat", "tdd"));

    /// <summary>
    /// The behind refuses the comparison it has no ground for, by the same rule at either end of it: no distance,
    /// no first environment named, and the environment asked for being the first one in any case - which is what
    /// leaves the first environment's own card without a word about being behind itself.
    /// </summary>
    [Theory]
    [InlineData(null, "tdd", "tdd")]
    [InlineData(2, "uat", null)]
    [InlineData(2, "TDD", "tdd")]
    [InlineData(2, "tdd", "TDD")]
    public void NothingIsBehindTheFirstEnvironmentOrWithoutOne(int? versions, string? environment, string? first) =>
        Assert.Null(DeliveryText.Behind(new BehindFirst(versions, 3), environment!, first));

    /// <summary>
    /// A failover test says what it has and nothing else: the test that named no environment, no moment and no
    /// seconds is the empty text the card leaves out, the empty name says nothing more than no name does, the
    /// seconds alone keep their own sentence, and a tenth of a second no stopwatch resolves is said as none.
    /// </summary>
    [Fact]
    public void AFailoverTestSaysWhatItHasAndNothingElse()
    {
        Assert.Equal(string.Empty, DeliveryText.Failover(new FailoverTest(null, null, null), Now));
        Assert.Equal(string.Empty, DeliveryText.Failover(new FailoverTest(string.Empty, null, null), Now));
        Assert.Equal("the standby answered after 44 s", DeliveryText.Failover(new FailoverTest(string.Empty, null, 44), Now));
        Assert.Equal("the standby answered after 0 s", DeliveryText.Failover(new FailoverTest(null, null, 0.04), Now));
        Assert.Equal("just now", DeliveryText.Failover(new FailoverTest(null, Now, null), Now));
        Assert.Equal(
            "9 h ago: the standby answered after 44.5 s",
            DeliveryText.Failover(new FailoverTest(null, Failed, 44.5), Now));
        Assert.Equal(
            "uat, 9 h ago: the standby answered after 44.5 s",
            DeliveryText.Failover(new FailoverTest("uat", Failed, 44.5), Now));
        Assert.Equal("uat, just now", DeliveryText.Failover(new FailoverTest("uat", Now.AddSeconds(-30), null), Now));
    }

    /// <summary>
    /// The failover test refuses a test that is not there: the card says nothing about a failover it was not told
    /// of, and does not ask for the moment to say of.
    /// </summary>
    [Fact]
    public void TheFailoverTestRefusesATestThatIsNotThere() =>
        Assert.Equal("test", Assert.Throws<ArgumentNullException>(() => DeliveryText.Failover(null!, Now)).ParamName);

    /// <summary>
    /// Whose delivery a card is told by the whole name of its entry and not by a trimmed or shortened one: the
    /// system project in any case is the system, a name that only looks like it with its whitespace is one of the
    /// others, and the environment is said as the file wrote it - the empty name and the empty environment
    /// included, which no file gives and every card would have to say of.
    /// </summary>
    [Theory]
    [InlineData("SYSTEM", "pre-prod", "the system (infrastructure and pipeline) in pre-prod")]
    [InlineData("sYsTeM", "uat", "the system (infrastructure and pipeline) in uat")]
    [InlineData("System ", "uat", "System  in uat")]
    [InlineData(" system", "uat", " system in uat")]
    [InlineData("systems", "uat", "systems in uat")]
    [InlineData("demo-system", "uat", "demo-system in uat")]
    [InlineData("ui", "pre-prod", "ui in pre-prod")]
    [InlineData("UI", "UAT", "UI in UAT")]
    [InlineData("dashboard", "", "dashboard in ")]
    public void WhoseDeliveryIsToldByTheWholeNameOfTheEntry(string name, string environment, string expected) =>
        Assert.Equal(expected, DeliveryText.Context(name, environment));

    /// <summary>
    /// The report of the fixture the whole class is read from is the file it was read from, part for part: the
    /// moment the facts changed, the two environments in the order the file writes them and the failover test they
    /// name - so that every end above is asked of a report a reader can check against the text at the top of the
    /// file, and not of one the test made up on the way.
    /// </summary>
    [Fact]
    public void TheFixtureIsTheFileItWasReadFrom()
    {
        var report = Report(Delivery);

        Assert.Equal(Generated, report.Generated);
        Assert.Equal(["tdd", "uat"], report.Environments.Select(environment => environment.Name));
        Assert.Equal(new FailoverTest("uat", new DateTimeOffset(2026, 10, 6, 8, 30, 0, TimeSpan.Zero), 44), report.Failover);
        Assert.Equal("17 h ago", TimeText.Ago(report.Failover!.At!.Value, Now));
        Assert.Equal(new HealthWindow(24, 23), report.HealthOf("UAT")!.Last24Hours);
        Assert.Equal("2.4.14", report.Find("uat", "ui")!.Version);
        Assert.Equal(["system", "dashboard"], report.Others("uat", ["ui"]).Select(entry => entry.Name));
    }
}
