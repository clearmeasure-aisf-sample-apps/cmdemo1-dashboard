using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="DeploymentFreeze" /> - one entry of the deployments
/// file's <c>freezes</c> - on the members the tests that already pin the freeze leave open.
/// <para>
/// Pinned where it belongs: <see cref="DeploymentFreezeTests" /> pins the clock - in force from its start till its
/// end, the file's <c>active</c> deciding where no start is given, and the seventy-two hours
/// <see cref="DeploymentFreeze.Ahead" /> names, which is the one fixed value the record has and is not pinned again
/// here; <see cref="DeploymentActivityTests" /> pins what the page makes of a freeze - that the coming is named at
/// the seventy-two hours and not one minute before, that it is in force at its start and over at its end, and that
/// it covers the environment and the project it names in any case.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: <see cref="DeploymentFreeze.ComesAt" /> - with no start a freeze is
/// one that never comes, whatever the file says it is and however far its end still runs; a start the instant asked
/// for stands at is not a coming, a start a tick after it is; the seventy-two hours held to the tick - the start at
/// them a coming, the start a tick further not; and a start within the seventy-two hours that is no coming because
/// the freeze has already ended, at the end and after it. <see cref="DeploymentFreeze.Covers" /> - a list that
/// names nothing covers nothing, the project asked for by its name alone being the asking the default leaves out,
/// and the empty name held as the name it is rather than as no name. And <see cref="DeploymentFreeze.InForceAt" />
/// - a freeze that begins after it ends is in force at no moment, a freeze that ends at the earliest moment the
/// clock can hold in force at no moment either, and the instant asked about is compared as the instant, not as the
/// hour it is spelled in, which is the same asking at every hour.
/// </para>
/// <para>
/// And the record itself, whose members the compiler names - <c>Deconstruct</c>, <c>Equals</c>, <c>GetHashCode</c>,
/// <c>ToString</c>, <c>op_Equality</c> and <c>op_Inequality</c> - no test of the clock or of the file has ever
/// asked: its six arguments standing on the properties of their names, with the two name lists held as the very
/// references they were given and the moments kept in the hours they were carried in; its six parts falling apart
/// in the order they were made, the lists and the nothing of no start coming back as they went in; the words it
/// says itself, in the invariant culture, naming the six parts and the two lists as the lists each of them is -
/// named by its type and never opened - and none of what they name; its equality, in which the
/// <see cref="DeploymentFreeze.Environments" /> and the <see cref="DeploymentFreeze.Projects" /> stand as the
/// references they are rather than as what they hold - two lists saying the same thing making two freezes, one list
/// handed to two freezes making one freeze; and its public surface, the six properties, the three questions and the
/// <see cref="DeploymentFreeze.Ahead" /> the field names, so that a seventh part of the file has to be written here
/// as well.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every moment is one the test brings.
/// </para>
/// </summary>
public class DeploymentFreezeEdgeCaseTests
{
    /// <summary>Saturday 19:00, UTC: the start the test's freezes stand on.</summary>
    private static readonly DateTimeOffset Start = new(2026, 10, 10, 19, 0, 0, TimeSpan.Zero);

    /// <summary>Sunday 19:00, UTC: the end the test's freezes stand on.</summary>
    private static readonly DateTimeOffset End = new(2026, 10, 11, 19, 0, 0, TimeSpan.Zero);

    /// <summary>The hours thirteen east of the meridian, to spell one instant the other way round.</summary>
    private static readonly TimeSpan East = TimeSpan.FromHours(13);

    private static DeploymentFreeze Of(DateTimeOffset? from, DateTimeOffset to, bool active, string[] environments, string[] projects) =>
        new("weekend", from, to, active, environments, projects);

    // ------------------------------------------------------- ComesAt: the freeze that begins soon

    /// <summary>With no start a freeze is one that never comes: the file's <c>active</c> has no say in a coming,
    /// and however far its end still runs, no asking at any moment finds it beginning soon - not before its end,
    /// not at it and not after it.</summary>
    [Fact]
    public void AFreezeWithoutAStartIsOneThatNeverComes()
    {
        var active = Of(null, End, true, ["prod"], ["demo-ui"]);
        var notActive = Of(null, End, false, ["prod"], ["demo-ui"]);

        DateTimeOffset[] moments = [ActivityLines.Now, End - TimeSpan.FromSeconds(1), End, End + TimeSpan.FromSeconds(1), DateTimeOffset.MaxValue];

        Assert.All(moments, moment => Assert.False(active.ComesAt(moment)));
        Assert.All(moments, moment => Assert.False(notActive.ComesAt(moment)));
    }

    /// <summary>A start the instant asked for stands at is not a coming - the freeze that begins at the instant is
    /// one in force, not one that comes - and a start a tick after the instant is, however close it is, the
    /// coming: the asking wants a start after the instant, and one tick is after.</summary>
    [Fact]
    public void AStartOnTheInstantAskedForIsNotAComingAndATickAfterItIs()
    {
        var startsNow = Of(ActivityLines.Now, End, false, ["prod"], ["demo-ui"]);
        var startsATickAfter = Of(ActivityLines.Now + TimeSpan.FromTicks(1), End, false, ["prod"], ["demo-ui"]);

        Assert.False(startsNow.ComesAt(ActivityLines.Now));
        Assert.True(startsNow.InForceAt(ActivityLines.Now));

        Assert.True(startsATickAfter.ComesAt(ActivityLines.Now));
        Assert.False(startsATickAfter.InForceAt(ActivityLines.Now));
    }

    /// <summary>The seventy-two hours <see cref="DeploymentFreeze.Ahead" /> names, held to the tick: the start at
    /// them is a coming, and the start a tick further is not - the distance the start stands from the instant is
    /// the whole of the asking, and one tick over the length the page names is over it.</summary>
    [Fact]
    public void TheSeventyTwoHoursAreHeldToTheTick()
    {
        var atSeventyTwoHours = Of(ActivityLines.Now + DeploymentFreeze.Ahead, End.AddDays(30), false, ["prod"], ["demo-ui"]);
        var aTickFurther = Of(ActivityLines.Now + DeploymentFreeze.Ahead + TimeSpan.FromTicks(1), End.AddDays(30), false, ["prod"], ["demo-ui"]);

        Assert.True(atSeventyTwoHours.ComesAt(ActivityLines.Now));
        Assert.False(aTickFurther.ComesAt(ActivityLines.Now));
    }

    /// <summary>A start within the seventy-two hours is no coming where the freeze has already ended: the end is
    /// the whole of the other asking, and at the end and after it the start that stands after the instant has
    /// already come - the freeze is over, and not one that comes.</summary>
    [Fact]
    public void AStartWithinSeventyTwoHoursIsNoComingWhereTheFreezeHasEnded()
    {
        var startedAfterItsEnd = Of(End.AddHours(1), End, true, ["prod"], ["demo-ui"]);

        Assert.False(startedAfterItsEnd.ComesAt(End));
        Assert.False(startedAfterItsEnd.ComesAt(End + TimeSpan.FromSeconds(1)));
        Assert.False(startedAfterItsEnd.InForceAt(End));
    }

    // ------------------------------------------------------- InForceAt: the asking the clock leaves open

    /// <summary>A freeze that begins after it ends is in force at no moment: to be in force the asking wants an
    /// end after the instant and a start at or before it, and the two stand no moment apart where the start is the
    /// later of them - at no moment, at either end of the clock or between, is the freeze in force. The six moments
    /// below are each either at the end or past it, where the end ends both questions, or further from the start
    /// than the seventy-two hours, where the start is no longer a coming; the coming is not emptied by the break
    /// alone, so the moments that stand between the two are pinned the other way as well - while its end has not
    /// been reached and its start stands within the seventy-two hours, the broken freeze is still named as a coming,
    /// since the asking reads its start and its end apart from one another and never compares the two.</summary>
    [Fact]
    public void AFreezeThatBeginsAfterItEndsIsInForceAtNoMoment()
    {
        var broken = Of(ActivityLines.Now.AddHours(2), ActivityLines.Now, true, ["prod"], ["demo-ui"]);

        DateTimeOffset[] moments = [DateTimeOffset.MinValue, ActivityLines.Now, ActivityLines.Now.AddHours(1), broken.From!.Value, ActivityLines.Now.AddHours(2), DateTimeOffset.MaxValue];

        Assert.All(moments, moment => Assert.False(broken.InForceAt(moment)));
        Assert.All(moments, moment => Assert.False(broken.ComesAt(moment)));

        // Between the two: the end not yet reached and the start within the seventy-two hours, the freeze is still
        // a coming - held to the tick at the far side of them, as the coming of a whole freeze is.
        Assert.False(broken.InForceAt(ActivityLines.Now - TimeSpan.FromHours(1)));
        Assert.True(broken.ComesAt(ActivityLines.Now - TimeSpan.FromHours(1)));
        Assert.True(broken.ComesAt(broken.From!.Value - DeploymentFreeze.Ahead));
        Assert.False(broken.ComesAt(broken.From!.Value - DeploymentFreeze.Ahead - TimeSpan.FromTicks(1)));
    }

    /// <summary>The instant asked about is compared as the instant and not as the hour it is spelled in: the start
    /// and the end spelled thirteen hours east of the meridian are the same moments, and the freeze is in force
    /// from the start till the end of them, in force a second before the end and over at it, in whichever hour the
    /// asking is spelled in - and a coming counted from the same end is a coming at the same instant spelled
    /// anywhere.</summary>
    [Fact]
    public void TheInstantIsComparedAsTheInstantNotTheHourItIsSpelledIn()
    {
        var start = new DateTimeOffset(2026, 10, 10, 6, 0, 0, TimeSpan.Zero);
        var freeze = Of(start.ToOffset(East), End.ToOffset(East), false, ["prod"], ["demo-ui"]);

        // Spelled thirteen hours east of the meridian the two are the same moments: the start is Sat 19:00 +13 and
        // the end Mon 08:00 +13, and the record holds the moments, not the spelling.
        Assert.Equal(start.UtcTicks, freeze.From!.Value.UtcTicks);
        Assert.Equal(End.UtcTicks, freeze.To.UtcTicks);

        Assert.True(freeze.InForceAt(start));
        Assert.True(freeze.InForceAt(End - TimeSpan.FromSeconds(1)));
        Assert.False(freeze.InForceAt(End));
        Assert.False(freeze.InForceAt(End + TimeSpan.FromSeconds(1)));

        // The same instants, spelled thirteen hours east of the meridian, are the same asking.
        Assert.True(freeze.InForceAt(start.ToOffset(East)));
        Assert.True(freeze.InForceAt((End - TimeSpan.FromSeconds(1)).ToOffset(East)));
        Assert.False(freeze.InForceAt(End.ToOffset(East)));
        Assert.False(freeze.InForceAt((End + TimeSpan.FromSeconds(1)).ToOffset(East)));

        // A coming counted from the same end is a coming at the end, however the end is spelled.
        var coming = Of(End + DeploymentFreeze.Ahead, End.AddHours(100), false, ["prod"], ["demo-ui"]);

        Assert.True(coming.ComesAt(End));
        Assert.True(coming.ComesAt(End.ToOffset(East)));
    }

    /// <summary>An end at the earliest moment the clock can hold ends the freeze before the clock begins: the asking
    /// wants an instant before the end, and no instant stands before <see cref="DateTimeOffset.MinValue" /> - so
    /// such a freeze is in force at no moment, whether it was told a start at the same earliest moment, told none
    /// and said active by the file, or said inactive, and it is no coming at any moment either, the asking for a
    /// coming reading the very end that is never reached. The end at the other end of the clock, and the ends
    /// <see cref="DateTimeOffset.MaxValue" /> asked about, the tests above walk.</summary>
    [Fact]
    public void AFreezeThatEndsAtTheEarliestMomentIsInForceAtNoMoment()
    {
        var startedAtTheEarliestMoment = Of(DateTimeOffset.MinValue, DateTimeOffset.MinValue, true, ["prod"], ["demo-ui"]);
        var unstarted = Of(null, DateTimeOffset.MinValue, true, ["prod"], ["demo-ui"]);
        var notActive = Of(null, DateTimeOffset.MinValue, false, ["prod"], ["demo-ui"]);

        DeploymentFreeze[] endingNowhere = [startedAtTheEarliestMoment, unstarted, notActive];

        DateTimeOffset[] moments =
        [
            DateTimeOffset.MinValue,
            DateTimeOffset.MinValue + TimeSpan.FromTicks(1),
            ActivityLines.Now,
            DateTimeOffset.MaxValue - TimeSpan.FromTicks(1),
            DateTimeOffset.MaxValue,
        ];

        Assert.All(endingNowhere, freeze => Assert.All(moments, moment => Assert.False(freeze.InForceAt(moment))));
        Assert.All(endingNowhere, freeze => Assert.All(moments, moment => Assert.False(freeze.ComesAt(moment))));
    }

    // ------------------------------------------------------------- Covers: the environment, and the project

    /// <summary>A list that names nothing covers nothing: an environment the freeze does not name is not covered,
    /// with or without a project asked for, and a project asked for that the freeze does not name is not covered
    /// while the environment it is asked for is - the environment is always asked for, and the project only when
    /// one is.</summary>
    [Fact]
    public void AListThatNamesNothingCoversNothing()
    {
        var noEnvironments = Of(ActivityLines.Now, End, false, [], ["demo-ui"]);
        var noProjects = Of(ActivityLines.Now, End, false, ["prod"], []);
        var neither = Of(ActivityLines.Now, End, false, [], []);

        Assert.False(noEnvironments.Covers("prod", "demo-ui"));
        Assert.False(noEnvironments.Covers("prod"));

        Assert.False(noProjects.Covers("prod", "demo-ui"));
        Assert.True(noProjects.Covers("prod"));
        Assert.True(noProjects.Covers("prod", null));

        Assert.False(neither.Covers("prod"));
        Assert.False(neither.Covers("prod", "demo-ui"));
    }

    /// <summary>The default asks no project at all: with the project left out the environment is the whole of the
    /// asking and the project list is not looked at - the freeze that names <c>demo-ui</c> covers the environment
    /// asked for with no project, as it is covered asked for with the project left out as null - while the same
    /// freeze asked for with a project it does not name covers nothing.</summary>
    [Fact]
    public void TheDefaultAsksNoProjectAtAll()
    {
        var namesAProject = Of(ActivityLines.Now, End, false, ["prod"], ["demo-ui"]);

        Assert.True(namesAProject.Covers("prod"));
        Assert.True(namesAProject.Covers("prod", null));
        Assert.Equal(namesAProject.Covers("prod"), namesAProject.Covers("prod", null));
        Assert.False(namesAProject.Covers("prod", "demo-db"));
    }

    /// <summary>The empty name is held as the name it is, and not as no name: the record keeps the list it was
    /// built of, and the reading that lets no empty name through is not the record - so a freeze built of an
    /// empty environment and an empty project covers the empty environment asked for with the empty project,
    /// and not the project it does not name.</summary>
    [Fact]
    public void AnEmptyNameIsTheNameItIs()
    {
        var empty = Of(ActivityLines.Now, End, false, ["", "prod"], ["", "demo-ui"]);

        Assert.True(empty.Covers("", ""));
        Assert.True(empty.Covers("prod", ""));
        Assert.True(empty.Covers("", "demo-ui"));
        Assert.False(empty.Covers("", "demo-db"));
    }

    // --------------------------------------------------------------- the record the compiler made around them

    /// <summary>Every argument of the constructor standing on the property of its name, and on that property alone:
    /// the name, the start or the nothing of no start, the end, the file's own word, and the two lists as the very
    /// references they were handed - the record copies no list and makes none of its own, so the empty list a caller
    /// built is the empty list the property holds. The two moments keep the hours they were carried in and stand at
    /// either end of the clock; the record normalises nothing and refuses nothing.</summary>
    [Fact]
    public void EveryArgumentStandsOnThePropertyOfItsName()
    {
        string[] environments = ["prod", "uat"];
        string[] projects = ["demo-ui"];

        var freeze = new DeploymentFreeze("weekend", Start, End, false, environments, projects);

        Assert.Equal("weekend", freeze.Name);
        Assert.Equal(Start, freeze.From);
        Assert.Equal(End, freeze.To);
        Assert.False(freeze.Active);
        Assert.Same(environments, freeze.Environments);
        Assert.Same(projects, freeze.Projects);
        Assert.Equal(2, freeze.Environments.Count);
        Assert.Single(freeze.Projects);

        // The nothing of no start, the empty name and the two empty lists are kept as they were given, and the two
        // ends of the clock are the two moments the properties answer with.
        var empty = new DeploymentFreeze("", null, DateTimeOffset.MinValue, true, [], []);

        Assert.Equal("", empty.Name);
        Assert.Null(empty.From);
        Assert.Equal(DateTimeOffset.MinValue, empty.To);
        Assert.True(empty.Active);
        Assert.Empty(empty.Environments);
        Assert.Empty(empty.Projects);

        var latest = new DeploymentFreeze("release week", DateTimeOffset.MaxValue, DateTimeOffset.MaxValue, false, environments, projects);

        Assert.Equal(DateTimeOffset.MaxValue, latest.From);
        Assert.Equal(DateTimeOffset.MaxValue, latest.To);

        // A moment carried in thirteen hours east of the meridian is held in those hours, not turned to UTC.
        var spelledEast = new DeploymentFreeze("weekend", Start.ToOffset(East), End.ToOffset(East), false, environments, projects);

        Assert.Equal(Start.UtcTicks, spelledEast.From!.Value.UtcTicks);
        Assert.Equal(East, spelledEast.From!.Value.Offset);
        Assert.Equal(End.UtcTicks, spelledEast.To.UtcTicks);
        Assert.Equal(East, spelledEast.To.Offset);
    }

    /// <summary>The six parts falling apart in the order they were made: the name, the start, the end, the file's
    /// word, the environments and the projects - the nothing of no start and the two very list references coming
    /// back as they went in - and what came out going back in, in that order, to the one freeze it came from. A
    /// freeze that says nothing falls apart to the empty name, the nothing, the earliest moment, the word said and
    /// the two empty lists.</summary>
    [Fact]
    public void TheSixPartsFallApartInTheOrderTheyWereMade()
    {
        string[] environments = ["prod", "uat"];
        string[] projects = ["demo-ui"];
        var freeze = new DeploymentFreeze("weekend", Start, End, false, environments, projects);

        var (name, from, to, active, covered, covering) = freeze;

        Assert.Equal("weekend", name);
        Assert.Equal(Start, from);
        Assert.Equal(End, to);
        Assert.False(active);
        Assert.Same(environments, covered);
        Assert.Same(projects, covering);
        Assert.Equal(freeze, new DeploymentFreeze(name, from, to, active, covered, covering));

        var (noName, noStart, nowhere, wasActive, noEnvironments, noProjects) =
            new DeploymentFreeze("", null, DateTimeOffset.MinValue, true, [], []);

        Assert.Equal("", noName);
        Assert.Null(noStart);
        Assert.Equal(DateTimeOffset.MinValue, nowhere);
        Assert.True(wasActive);
        Assert.Empty(noEnvironments);
        Assert.Empty(noProjects);

        // The start read out is the start asked about, and the end read out ends the freeze read out.
        Assert.True(freeze.InForceAt(from!.Value));
        Assert.False(freeze.InForceAt(to));
    }

    /// <summary>The words a freeze says about itself name its six parts, in the order they stand in, in the
    /// invariant culture: its own name at the front, the two moments as the culture they are read in spells them,
    /// the file's word as the boolean it is, and the two name lists as the lists they are - named by their type and
    /// never opened, a <c>string[]</c> no differently from the <c>List&lt;string&gt;</c> the reading of the file
    /// hands over - and not one environment or project of what they hold, which is what lets a log line carry a
    /// freeze without carrying the file. The moments follow whoever reads them, the rest of the sentence not.</summary>
    [Fact]
    public void TheFreezeSaysItsSixPartsAndNotWhatTheyName()
    {
        const string Spoken =
            "DeploymentFreeze { Name = weekend, From = 10/10/2026 19:00:00 +00:00, To = 10/11/2026 19:00:00 +00:00, "
            + "Active = False, Environments = System.String[], Projects = System.String[] }";

        string[] environments = ["prod", "uat"];
        string[] projects = ["demo-ui"];
        var freeze = new DeploymentFreeze("weekend", Start, End, false, environments, projects);

        Assert.Equal(Spoken, Words(freeze));

        // The list the file's reading builds is named as the list it is, and opened no more than the array is.
        Assert.Equal(
            "DeploymentFreeze { Name = weekend, From = 10/10/2026 19:00:00 +00:00, To = 10/11/2026 19:00:00 +00:00, "
            + "Active = False, Environments = System.Collections.Generic.List`1[System.String], Projects = System.Collections.Generic.List`1[System.String] }",
            Words(new DeploymentFreeze("weekend", Start, End, false, new List<string>(environments), new List<string>(projects))));

        Assert.Equal(
            "DeploymentFreeze { Name = , From = , To = 01/01/0001 00:00:00 +00:00, Active = True, Environments = System.String[], Projects = System.String[] }",
            Words(new DeploymentFreeze("", null, DateTimeOffset.MinValue, true, [], [])));

        Assert.Equal(
            "DeploymentFreeze { Name = release week, From = 12/31/9999 23:59:59 +00:00, To = 12/31/9999 23:59:59 +00:00, "
            + "Active = False, Environments = System.String[], Projects = System.String[] }",
            Words(new DeploymentFreeze("release week", DateTimeOffset.MaxValue, DateTimeOffset.MaxValue, false, environments, projects)));

        foreach (var word in new[] { "prod", "uat", "demo-ui", "frozen", "In force", "72" })
        {
            Assert.DoesNotContain(word, Words(freeze), StringComparison.Ordinal);
        }

        // One part different, another sentence.
        Assert.NotEqual(Words(freeze), Words(freeze with { Name = "release week" }));
        Assert.NotEqual(Words(freeze), Words(freeze with { Active = true }));
        Assert.NotEqual(Words(freeze), Words(freeze with { From = null }));
        Assert.NotEqual(Words(freeze), Words(freeze with { To = End.AddSeconds(1) }));

        // Nothing of the two lists but their type is in the sentence: the freeze that names one environment and the
        // one that names two say the very same words, and are still two freezes - the equality the lists decide is
        // not to be read out of them.
        string[] oneEnvironment = ["prod"];
        string[] oneProject = ["demo-ui"];
        var ofOneNameEach = Of(Start, End, false, oneEnvironment, oneProject);

        Assert.Equal(Words(freeze), Words(ofOneNameEach));
        Assert.NotEqual(freeze, ofOneNameEach);

        // The moments are the parts a reader turns; the name, the flag and the two lists say the same in German.
        ViewerCulture.German(() =>
        {
            Assert.Contains("From = 10.10.2026 19:00:00 +00:00", freeze.ToString(), StringComparison.Ordinal);
            Assert.Contains("To = 11.10.2026 19:00:00 +00:00", freeze.ToString(), StringComparison.Ordinal);
            Assert.Contains("Name = weekend", freeze.ToString(), StringComparison.Ordinal);
            Assert.Contains("Active = False", freeze.ToString(), StringComparison.Ordinal);
            Assert.Contains("Environments = System.String[]", freeze.ToString(), StringComparison.Ordinal);
        });
    }

    /// <summary>Two freezes are one when all six of their parts are, by every way of asking - the record's own
    /// <c>Equals</c>, the one borrowed from <see cref="object" />, <c>==</c> and <c>!=</c> - and the two name lists
    /// standing in that equality as the references they are, never by what they hold: one list handed to two freezes
    /// makes one freeze, of one hash and one entry of a set and one key of a dictionary, while two lists naming the
    /// same environments in the same order make two freezes, two entries of that set and no key the other finds.
    /// Any one of the six parts moved, dropped to nothing or raised from nothing is another freeze - the two lists
    /// of one freeze dropped to nothing being one freeze with themselves, two nothings being the same reference;
    /// nothing that is no freeze is one with one, and the two nothings are one with each other.</summary>
    [Fact]
    public void TwoFreezesAreOneWhenTheirSixPartsAreAndTheirListsAreTheSameLists()
    {
        string[] environments = ["prod", "uat"];
        string[] projects = ["demo-ui"];
        var one = new DeploymentFreeze("weekend", Start, End, false, environments, projects);
        var ofTheSameLists = new DeploymentFreeze("weekend", Start, End, false, one.Environments, one.Projects);
        var ofAlikeLists = new DeploymentFreeze("weekend", Start, End, false, ["prod", "uat"], ["demo-ui"]);

        Assert.True(one.Equals(one));
        Assert.True(one.Equals(ofTheSameLists));
        Assert.True(one.Equals((object)ofTheSameLists));
        Assert.True(one == ofTheSameLists);
        Assert.False(one != ofTheSameLists);
        Assert.Equal(one, ofTheSameLists);
        Assert.NotSame(one, ofTheSameLists);
        Assert.Equal(one.GetHashCode(), ofTheSameLists.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Single(new HashSet<DeploymentFreeze> { one, ofTheSameLists });

        // The two lists say the same names in the same order and are still two lists: two freezes and two entries
        // of one set.
        Assert.False(one.Equals(ofAlikeLists));
        Assert.False(one.Equals((object)ofAlikeLists));
        Assert.False(one == ofAlikeLists);
        Assert.True(one != ofAlikeLists);
        Assert.NotEqual(one, ofAlikeLists);
        Assert.Equal(2, new HashSet<DeploymentFreeze> { one, ofAlikeLists }.Count);

        var keyed = new Dictionary<DeploymentFreeze, string> { [one] = "the weekend" };

        keyed[ofTheSameLists] = "still the weekend";

        Assert.Single(keyed);
        Assert.False(keyed.ContainsKey(ofAlikeLists));
        Assert.Equal("still the weekend", keyed[ofAlikeLists with { Environments = one.Environments, Projects = one.Projects }]);

        // The two references dropped to nothing are one freeze with themselves: no list is the same no list twice.
        var bothNothing = new DeploymentFreeze("weekend", Start, End, false, null!, null!);
        var alsoBothNothing = new DeploymentFreeze("weekend", Start, End, false, null!, null!);

        Assert.Equal(bothNothing, alsoBothNothing);
        Assert.True(bothNothing == alsoBothNothing);
        Assert.False(bothNothing != alsoBothNothing);
        Assert.Equal(bothNothing.GetHashCode(), alsoBothNothing.GetHashCode());
        Assert.NotEqual(one, bothNothing);

        // A copy carries the two references it was not asked about, and turns only the part it names.
        var copied = one with { Active = true };

        Assert.Same(environments, copied.Environments);
        Assert.Same(projects, copied.Projects);
        Assert.NotEqual(one, copied);

        DeploymentFreeze[] different =
        [
            one with { Name = "release week" },
            one with { Name = "" },
            one with { From = null },
            one with { From = Start.AddSeconds(1) },
            one with { From = DateTimeOffset.MinValue },
            one with { To = End.AddSeconds(1) },
            one with { To = DateTimeOffset.MinValue },
            one with { Active = true },
            one with { Environments = new[] { "prod", "uat" } },
            one with { Environments = [] },
            one with { Environments = null! },
            one with { Projects = new[] { "demo-ui" } },
            one with { Projects = [] },
            one with { Projects = null! },
        ];

        foreach (var other in different)
        {
            Assert.NotEqual(one, other);
            Assert.False(one.Equals(other));
            Assert.False(one.Equals((object)other));
            Assert.True(one != other);
        }

        Assert.False(one.Equals(null));
        Assert.False(one.Equals((DeploymentFreeze?)null));
        Assert.False(one.Equals("weekend"));
        Assert.False(one.Equals((object)environments));

        DeploymentFreeze? nowhere = null;
        DeploymentFreeze? alsoNowhere = null;

        Assert.True(nowhere == alsoNowhere);
        Assert.False(nowhere != alsoNowhere);
        Assert.False(nowhere == one);
        Assert.True(nowhere != one);
        Assert.False(one.Equals(nowhere));
    }

    /// <summary>The whole public surface of the freeze, as it stands: the six properties it is made of, the three
    /// questions the page asks it, the seventy-two hours as the static field it is, and the members every record is
    /// asked - comparing, hashing, saying itself and falling apart - and nothing else to call. The type is sealed,
    /// each of its six properties has one getter and one setter of the names the compiler gives them and no index,
    /// and the constructor asks for the six in the order they are declared with none of them left out: a seventh
    /// part of the file has to be written here as well, which is the point - the page cannot be shown a part of the
    /// file nobody decided what it means.</summary>
    [Fact]
    public void TheSurfaceIsTheSixPropertiesTheThreeQuestionsAndTheLength()
    {
        Assert.Equal(
            [
                "Active",
                "Ahead",
                "ComesAt",
                "Covers",
                "Deconstruct",
                "Environments",
                "Equals",
                "From",
                "GetHashCode",
                "InForceAt",
                "Name",
                "Projects",
                "To",
                "ToString",
                "op_Equality",
                "op_Inequality",
            ],
            PublicSurface(typeof(DeploymentFreeze)));

        var properties = typeof(DeploymentFreeze)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(
            ["Name", "From", "To", "Active", "Environments", "Projects"],
            [.. typeof(DeploymentFreeze)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            [typeof(string), typeof(DateTimeOffset?), typeof(DateTimeOffset), typeof(bool), typeof(IReadOnlyList<string>), typeof(IReadOnlyList<string>)],
            [.. properties.Values.Select(property => property.PropertyType)]);

        Assert.All(properties.Values, property =>
        {
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.True(property.GetMethod!.IsPublic);
            Assert.False(property.GetMethod!.IsStatic);
            Assert.NotNull(property.SetMethod);
            Assert.True(property.SetMethod!.IsPublic);
            Assert.True(property.SetMethod!.IsSpecialName);
            Assert.Equal($"get_{property.Name}", property.GetMethod!.Name);
            Assert.Equal($"set_{property.Name}", property.SetMethod!.Name);
            Assert.Empty(property.GetIndexParameters());
        });

        // The one length the type names, and the only public field it has: the seventy-two hours of the coming, in
        // a field no one may turn and no instance carries.
        var ahead = typeof(DeploymentFreeze).GetField(nameof(DeploymentFreeze.Ahead), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.NotNull(ahead);
        Assert.Equal(typeof(TimeSpan), ahead!.FieldType);
        Assert.True(ahead.IsPublic);
        Assert.True(ahead.IsStatic);
        Assert.True(ahead.IsInitOnly);
        Assert.False(ahead.IsLiteral);
        Assert.Equal(DeploymentFreeze.Ahead, ahead.GetValue(null));
        Assert.Single(typeof(DeploymentFreeze).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly));

        // The six parts the constructor asks for, in the order it asks for them, none of them optional - which is
        // what makes a nothing it is handed the caller's nothing rather than a default.
        var arguments = typeof(DeploymentFreeze).GetConstructors().Single().GetParameters();

        Assert.Equal(["Name", "From", "To", "Active", "Environments", "Projects"], [.. arguments.Select(argument => argument.Name!)]);
        Assert.All(arguments, argument => Assert.False(argument.IsOptional));
        Assert.All(arguments, argument => Assert.False(argument.HasDefaultValue));
        Assert.Equal(
            [typeof(string), typeof(DateTimeOffset?), typeof(DateTimeOffset), typeof(bool), typeof(IReadOnlyList<string>), typeof(IReadOnlyList<string>)],
            [.. arguments.Select(argument => argument.ParameterType)]);

        // The six come out again as the six types they went in, in the order they went in.
        var deconstruct = typeof(DeploymentFreeze).GetMethod(nameof(DeploymentFreeze.Deconstruct))!;

        Assert.True(deconstruct.IsPublic);
        Assert.False(deconstruct.IsStatic);
        Assert.Equal(
            [typeof(string).MakeByRefType(), typeof(DateTimeOffset?).MakeByRefType(), typeof(DateTimeOffset).MakeByRefType(), typeof(bool).MakeByRefType(),
                typeof(IReadOnlyList<string>).MakeByRefType(), typeof(IReadOnlyList<string>).MakeByRefType()],
            [.. deconstruct.GetParameters().Select(parameter => parameter.ParameterType)]);

        // The three questions the page asks, each with its answer and none of them a static one: the two that ask
        // about a moment of the browser's clock are asked the one moment, and the covering takes its project by
        // default rather than of necessity.
        var inForce = typeof(DeploymentFreeze).GetMethod(nameof(DeploymentFreeze.InForceAt))!;

        Assert.True(inForce.IsPublic);
        Assert.False(inForce.IsStatic);
        Assert.Equal(typeof(bool), inForce.ReturnType);
        Assert.Equal([typeof(DateTimeOffset)], [.. inForce.GetParameters().Select(parameter => parameter.ParameterType)]);
        Assert.Equal(["now"], [.. inForce.GetParameters().Select(parameter => parameter.Name!)]);

        var comes = typeof(DeploymentFreeze).GetMethod(nameof(DeploymentFreeze.ComesAt))!;

        Assert.True(comes.IsPublic);
        Assert.False(comes.IsStatic);
        Assert.Equal(typeof(bool), comes.ReturnType);
        Assert.Equal([typeof(DateTimeOffset)], [.. comes.GetParameters().Select(parameter => parameter.ParameterType)]);
        Assert.Equal(["now"], [.. comes.GetParameters().Select(parameter => parameter.Name!)]);

        var covers = typeof(DeploymentFreeze).GetMethod(nameof(DeploymentFreeze.Covers))!;

        Assert.True(covers.IsPublic);
        Assert.False(covers.IsStatic);
        Assert.Equal(typeof(bool), covers.ReturnType);
        Assert.Equal([typeof(string), typeof(string)], [.. covers.GetParameters().Select(parameter => parameter.ParameterType)]);
        Assert.Equal(["environment", "project"], [.. covers.GetParameters().Select(parameter => parameter.Name!)]);
        Assert.False(covers.GetParameters()[0].HasDefaultValue);
        Assert.True(covers.GetParameters()[1].HasDefaultValue);
        Assert.Null(covers.GetParameters()[1].DefaultValue);

        // A record of values: sealed, compared by what it holds, with no state to ask a network or a clock about.
        Assert.True(typeof(DeploymentFreeze).IsSealed);
        Assert.Equal(typeof(object), typeof(DeploymentFreeze).BaseType);
        Assert.IsAssignableFrom<IEquatable<DeploymentFreeze>>(new DeploymentFreeze("weekend", Start, End, false, [], []));
    }

    // ------------------------------------------------------------------------------------ the machinery

    /// <summary>The public members a type declares itself: its properties, its public fields and the methods that
    /// are not the get and the set of one - the two operators kept although the compiler names them specially,
    /// since they are the <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone and
    /// its printing walked past. The constructor is no method of this list, so it is named by the test above rather
    /// than here.</summary>
    private static List<string> PublicSurface(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return [.. type
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is FieldInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
    }

    /// <summary>The words a freeze says its six parts in, taken in the invariant culture whatever the reader of
    /// this test happens to be: the two moments follow whoever reads them and the sentence is the point, so the
    /// culture is only the way to say it once - which of them a German reader turns is pinned on its own above.</summary>
    private static string Words(DeploymentFreeze freeze) => ViewerCulture.Invariant(() => freeze.ToString()!);
}
