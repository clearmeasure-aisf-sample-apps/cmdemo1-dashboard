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
/// - a freeze that begins after it ends is in force at no moment, and the instant asked about is compared as the
/// instant, not as the hour it is spelled in, which is the same asking at every hour.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every moment is one the test brings.
/// </para>
/// </summary>
public class DeploymentFreezeEdgeCaseTests
{
    /// <summary>Friday noon, UTC: the moment the freeze is asked about.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 9, 12, 0, 0, TimeSpan.Zero);

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

        DateTimeOffset[] moments = [Now, End - TimeSpan.FromSeconds(1), End, End + TimeSpan.FromSeconds(1), DateTimeOffset.MaxValue];

        Assert.All(moments, moment => Assert.False(active.ComesAt(moment)));
        Assert.All(moments, moment => Assert.False(notActive.ComesAt(moment)));
    }

    /// <summary>A start the instant asked for stands at is not a coming - the freeze that begins at the instant is
    /// one in force, not one that comes - and a start a tick after the instant is, however close it is, the
    /// coming: the asking wants a start after the instant, and one tick is after.</summary>
    [Fact]
    public void AStartOnTheInstantAskedForIsNotAComingAndATickAfterItIs()
    {
        var startsNow = Of(Now, End, false, ["prod"], ["demo-ui"]);
        var startsATickAfter = Of(Now + TimeSpan.FromTicks(1), End, false, ["prod"], ["demo-ui"]);

        Assert.False(startsNow.ComesAt(Now));
        Assert.True(startsNow.InForceAt(Now));

        Assert.True(startsATickAfter.ComesAt(Now));
        Assert.False(startsATickAfter.InForceAt(Now));
    }

    /// <summary>The seventy-two hours <see cref="DeploymentFreeze.Ahead" /> names, held to the tick: the start at
    /// them is a coming, and the start a tick further is not - the distance the start stands from the instant is
    /// the whole of the asking, and one tick over the length the page names is over it.</summary>
    [Fact]
    public void TheSeventyTwoHoursAreHeldToTheTick()
    {
        var atSeventyTwoHours = Of(Now + DeploymentFreeze.Ahead, End.AddDays(30), false, ["prod"], ["demo-ui"]);
        var aTickFurther = Of(Now + DeploymentFreeze.Ahead + TimeSpan.FromTicks(1), End.AddDays(30), false, ["prod"], ["demo-ui"]);

        Assert.True(atSeventyTwoHours.ComesAt(Now));
        Assert.False(aTickFurther.ComesAt(Now));
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
    /// later of them - at no moment, at either end of the clock or between, is the freeze in force, and it is no
    /// coming either, a start after its end being one that has not begun and never will.</summary>
    [Fact]
    public void AFreezeThatBeginsAfterItEndsIsInForceAtNoMoment()
    {
        var broken = Of(Now.AddHours(2), Now, true, ["prod"], ["demo-ui"]);

        DateTimeOffset[] moments = [DateTimeOffset.MinValue, Now, Now.AddHours(1), broken.From!.Value, Now.AddHours(2), DateTimeOffset.MaxValue];

        Assert.All(moments, moment => Assert.False(broken.InForceAt(moment)));
        Assert.All(moments, moment => Assert.False(broken.ComesAt(moment)));
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
        // the end Sun 08:00 +13, and the record holds the moments, not the spelling.
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

    // ------------------------------------------------------------- Covers: the environment, and the project

    /// <summary>A list that names nothing covers nothing: an environment the freeze does not name is not covered,
    /// with or without a project asked for, and a project asked for that the freeze does not name is not covered
    /// while the environment it is asked for is - the environment is always asked for, and the project only when
    /// one is.</summary>
    [Fact]
    public void AListThatNamesNothingCoversNothing()
    {
        var noEnvironments = Of(Now, End, false, [], ["demo-ui"]);
        var noProjects = Of(Now, End, false, ["prod"], []);
        var neither = Of(Now, End, false, [], []);

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
        var namesAProject = Of(Now, End, false, ["prod"], ["demo-ui"]);

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
        var empty = Of(Now, End, false, ["", "prod"], ["", "demo-ui"]);

        Assert.True(empty.Covers("", ""));
        Assert.True(empty.Covers("prod", ""));
        Assert.True(empty.Covers("", "demo-ui"));
        Assert.False(empty.Covers("", "demo-db"));
    }
}
