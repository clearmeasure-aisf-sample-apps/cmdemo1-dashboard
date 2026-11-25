using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="WorkWeek" />, the record declared in
/// <c>src/Dashboard/Work/WorkItemsReport.cs</c> that names one week of the board: when it started, how many pull
/// requests merged in it, and how many of its items reached production. All three parts may be unsaid, and the
/// weeks of a board's <c>stats.perWeek</c> are what the week chart is made of.
/// <para>
/// What the other files already pin is left alone. The taking of a week out of <c>workitems.json</c> is
/// <c>WorkWeek.Read</c>, which is internal to an app that declares no <c>InternalsVisibleTo</c> and is therefore
/// pinned through <see cref="WorkItemsReport.Parse" />: <see cref="WorkItemsStatsPartsTests" /> says that a week is
/// read from its own three words - <c>weekStart</c>, <c>merged</c>, <c>deployedProd</c> - and not from another
/// part's, and <see cref="WorkItemsTests.EveryPartOfAnItemIsOptionalAndABadPartIsLeftOut" /> that an element of
/// <c>perWeek</c> which says nothing, and one which is no object at all, are left out while a week that says only a
/// count is still a week. What a count or a moment may be in the file itself - rounding, a number below nought, a
/// number past an <see cref="int" />, text that is no date - is <see cref="JsonReadTests" />'s, since
/// <c>JsonRead.Count</c> and <c>JsonRead.Time</c> answer for every record and are pinned there once. What a week is
/// worth to the page - the two counts shown as nought when the file left them out, the <c>Week of 2026-11-02</c>
/// of a label, a bar's height, a count below nought and a count at the top of an <see cref="int" /> - is
/// <see cref="WorkItemsText.Chart" /> and <see cref="WorkItemsText.LatestWeek" />, and is pinned in
/// <see cref="WorkWeekChartTests" /> and <see cref="WorkItemsTextOpenEdgeCaseTests" />. The weeks a board's own
/// items are summarised into, which stand on a clock and are no member of this record, are
/// <see cref="WorkBotTextWeekEdgeCaseTests" />'s. And that a week is no duration is pinned where the duration is
/// asked, in
/// <see cref="WorkDurationEdgeCaseTests.NoDurationIsOneWithNoDurationAndNotWithAnyOtherThing" />.
/// </para>
/// <para>
/// What is left, and is pinned here, is the record itself: the constructor handing each of its three arguments to
/// the property of its name; the two counts standing at the ends an <see cref="int" /> runs to and the moment
/// standing at the ends a <see cref="DateTimeOffset" /> runs to, with the offset it was written with kept; the same
/// instant written under another offset being one week and one entry of a set, while the same clock under another
/// offset is another week; a count nobody said not being a count of nought, and a moment nobody said not being the
/// first moment; the copy that turns the part it names and keeps the other two; the three parts coming back out in
/// the order they went in; two weeks being one while their three parts are, with the hash, the set and the two
/// operators following that, and nothing on either side of each - the hash of an unsaid part and of a part of
/// nought being one number, which is what the equality and the set are there to settle and not the hash; the words
/// the week says its three parts in, which
/// say no fraction of a second, follow whoever reads them, and are no measure of the week; and the public surface,
/// so that the day a fourth part is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a week holds the moment and the two
/// counts it was given and calls nothing at all - no week is measured here, only held - so every value below is one
/// the test brought, and every call says the same thing twice.
/// </para>
/// </summary>
public class WorkWeekEdgeCaseTests
{
    /// <summary>A Monday, as the file writes one.</summary>
    private static readonly DateTimeOffset WeekStart = new(2026, 11, 2, 0, 0, 0, TimeSpan.Zero);

    /// <summary>The very instant of <see cref="WeekStart" />, written by a reader two hours east of Greenwich.</summary>
    private static readonly DateTimeOffset SameInstantFurtherEast = new(2026, 11, 2, 2, 0, 0, TimeSpan.FromHours(2));

    /// <summary>The same clock as <see cref="WeekStart" />, written under another offset: another instant.</summary>
    private static readonly DateTimeOffset TheSameClockFurtherEast = new(2026, 11, 2, 0, 0, 0, TimeSpan.FromHours(2));

    /// <summary>A week with all three parts said: the week the sample file's <c>perWeek</c> is read as.</summary>
    private static WorkWeek Full => new(WeekStart, 4, 2);

    /// <summary>A week whose every part is the nothing a file that says nothing lands on.</summary>
    private static WorkWeek Unsaid => new(null, null, null);

    /// <summary>A week whose two counts are both said as nought: a week the board crossed with nothing merged and
    /// nothing deployed, which is not the week nobody counted.</summary>
    private static WorkWeek Zeroed => new(WeekStart, 0, 0);

    // --------------------------------- the three parts, by the names of their own properties

    /// <summary>The constructor's three arguments, position by position, come back as the property of the same name:
    /// the moment the week starts first, the count of merged pull requests beside it, the count that reached prod
    /// last - the order the file's <c>weekStart</c>, <c>merged</c> and <c>deployedProd</c> are named in and the
    /// order a bar says them in. Each is held as the value the week was built of, never as a copy of it, and a new
    /// week of the same three parts is another week, not the one it was built of.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var week = new WorkWeek(WeekStart, 4, 2);

        Assert.Equal(WeekStart, week.WeekStart);
        Assert.Equal(4, week.Merged);
        Assert.Equal(2, week.DeployedProd);

        var again = new WorkWeek(WeekStart, 4, 2);

        Assert.Equal(week, again);
        Assert.NotSame(week, again);

        // The two counts stand on their own properties: a week that says only merged says nothing about prod, and a
        // week that says only a moment says nothing about either count.
        var mergedOnly = new WorkWeek(null, 1, null);

        Assert.Null(mergedOnly.WeekStart);
        Assert.Equal(1, mergedOnly.Merged);
        Assert.Null(mergedOnly.DeployedProd);
        Assert.NotEqual(week, mergedOnly);
    }

    /// <summary>The two counts are <see cref="int" />?s and stand at the ends an <see cref="int" /> runs to: nought,
    /// one, a count below nought, and either end of the range - which the record keeps as it was given and never
    /// refuses, never clamps and never rounds. Whoever reads the file decides what a count may be (the reader
    /// refuses what is below nought or past the end, and that is pinned in <see cref="JsonReadTests" />); this
    /// record only holds what it is handed, so a page built straight from a week may see the ends.</summary>
    [Theory]
    [InlineData(null, null)]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(0, 1)]
    [InlineData(4, 2)]
    [InlineData(-1, -1)]
    [InlineData(int.MaxValue, int.MaxValue)]
    [InlineData(int.MinValue, int.MinValue)]
    [InlineData(int.MaxValue, 0)]
    [InlineData(null, 3)]
    [InlineData(3, null)]
    public void TheTwoCountsStandAtTheEndsAnIntRunsTo(int? merged, int? deployedProd)
    {
        var week = new WorkWeek(WeekStart, merged, deployedProd);

        Assert.Equal(merged, week.Merged);
        Assert.Equal(deployedProd, week.DeployedProd);
        Assert.Equal(WeekStart, week.WeekStart);
        Assert.Equal(week, new WorkWeek(WeekStart, merged, deployedProd));
        Assert.Equal(week, week with { });
        Assert.Equal(week.GetHashCode(), new WorkWeek(WeekStart, merged, deployedProd).GetHashCode());

        // A week's counts say nothing about each other's place: the same two numbers the other way round are
        // another week, which is how a bar that merged four and deployed two is never read as the opposite.
        if (merged != deployedProd)
        {
            Assert.NotEqual(week, new WorkWeek(WeekStart, deployedProd, merged));
        }
    }

    /// <summary>The moment the week starts is a <see cref="DateTimeOffset" /> and is held with the offset it was
    /// written with: the first moment there is, the last, a Monday at midnight and a Monday at a quarter past five,
    /// east of Greenwich and west of it. Nothing is moved to UTC and nothing is normalised: a week of the board's
    /// own time zone stays a week of that time zone, and the chart's label is cut from the clock as held
    /// (pinned in <see cref="WorkWeekChartTests" />), not from a moment the record rewrote.</summary>
    [Theory]
    [MemberData(nameof(Moments))]
    public void TheMomentIsHeldWithTheOffsetItWasWritten(DateTimeOffset weekStart, TimeSpan offset)
    {
        var week = new WorkWeek(weekStart, 1, 1);

        Assert.Equal(weekStart, week.WeekStart);
        Assert.Equal(offset, Assert.IsType<DateTimeOffset>(week.WeekStart).Offset);
        Assert.Equal(week, new WorkWeek(weekStart, 1, 1));
        Assert.Equal(week.GetHashCode(), new WorkWeek(weekStart, 1, 1).GetHashCode());
    }

    /// <summary>The moments a week may be started at: the two ends of the range, a Monday at midnight and the same
    /// Monday at a quarter past five, and each of them written east of Greenwich and west of it.</summary>
    public static TheoryData<DateTimeOffset, TimeSpan> Moments() =>
        new()
        {
            { WeekStart, TimeSpan.Zero },
            { new DateTimeOffset(2026, 11, 9, 17, 15, 0, TimeSpan.Zero), TimeSpan.Zero },
            { SameInstantFurtherEast, TimeSpan.FromHours(2) },
            { TheSameClockFurtherEast, TimeSpan.FromHours(2) },
            { new DateTimeOffset(2026, 11, 2, 0, 0, 0, TimeSpan.FromHours(-5)), TimeSpan.FromHours(-5) },
            { DateTimeOffset.MinValue, TimeSpan.Zero },
            { DateTimeOffset.MaxValue, TimeSpan.Zero },
        };

    // --------------------------------- the nothing each part may be

    /// <summary>A count nobody said is not a count of nought, on either of the two counts and on both at once. The
    /// first has no part at all; the second has a part that says nought - a week the board crossed with nothing
    /// merged, which the page shows as a bar of no height while a week nobody counted is no bar at all. The record
    /// keeps the difference; only the page decides what to do with it.</summary>
    [Fact]
    public void ACountNobodySaidIsNotACountOfNought()
    {
        var unsaid = new WorkWeek(WeekStart, null, null);
        var zeroed = new WorkWeek(WeekStart, 0, 0);

        Assert.NotEqual(unsaid, zeroed);
        Assert.True(unsaid != zeroed);
        Assert.False(unsaid == zeroed);
        Assert.False(unsaid.Equals((object)zeroed));
        Assert.Null(unsaid.Merged);
        Assert.Null(unsaid.DeployedProd);
        Assert.Equal(0, zeroed.Merged);
        Assert.Equal(0, zeroed.DeployedProd);

        // Half-said weeks are three weeks apart: only merged said, only prod said, and both said as nought.
        Assert.NotEqual(unsaid, new WorkWeek(WeekStart, 0, null));
        Assert.NotEqual(unsaid, new WorkWeek(WeekStart, null, 0));
        Assert.NotEqual(new WorkWeek(WeekStart, 0, null), new WorkWeek(WeekStart, null, 0));

        // A count of nought hashes as an unsaid count does, so it is the equality - and the set that asks it -
        // which keeps the two weeks apart: this is the pair the hash is not there to settle.
        Assert.Equal(unsaid.GetHashCode(), zeroed.GetHashCode());
        var counted = new HashSet<WorkWeek> { unsaid, zeroed, new(WeekStart, 0, null) };
        Assert.Equal(3, counted.Count);
        Assert.Contains(unsaid, counted);
        Assert.Contains(zeroed, counted);
        Assert.Equal(
            "WorkWeek { WeekStart = 11/02/2026 00:00:00 +00:00, Merged = , DeployedProd =  }",
            CultureScope.Words(unsaid));
        Assert.Equal(
            "WorkWeek { WeekStart = 11/02/2026 00:00:00 +00:00, Merged = 0, DeployedProd = 0 }",
            CultureScope.Words(zeroed));
    }

    /// <summary>A moment nobody said is not the first moment there is, and the first moment is a week of its own.
    /// The nothing is the part a file that never named a week lands on; <see cref="DateTimeOffset.MinValue" /> is a
    /// moment - the first of the range - and a week started at it is a week that says when it starts.</summary>
    [Fact]
    public void AMomentNobodySaidIsNotTheFirstMoment()
    {
        var noMoment = new WorkWeek(null, 1, 1);
        var firstMoment = new WorkWeek(DateTimeOffset.MinValue, 1, 1);

        Assert.NotEqual(noMoment, firstMoment);
        Assert.False(noMoment.Equals((object)firstMoment));
        Assert.Null(noMoment.WeekStart);
        Assert.Equal(DateTimeOffset.MinValue, firstMoment.WeekStart);

        // The two ends of the moment are two weeks apart, and neither of them is the nothing. The first moment
        // hashes as the nothing does - its ticks are nought, and so is the hash of a part nobody said - so again
        // the equality and the set, not the hash, are what hold the two apart.
        var lastMoment = new WorkWeek(DateTimeOffset.MaxValue, 1, 1);
        Assert.NotEqual(firstMoment, lastMoment);
        Assert.Equal(noMoment.GetHashCode(), firstMoment.GetHashCode());
        var started = new HashSet<WorkWeek> { noMoment, firstMoment, lastMoment };
        Assert.Equal(3, started.Count);
        Assert.Contains(noMoment, started);
        Assert.Contains(firstMoment, started);
    }

    /// <summary>A week whose every part is unsaid is still a week: it is a value, not the nothing a file that said
    /// nothing lands on. What turns <c>{ }</c> into no week at all is the reading of the file - pinned through
    /// <see cref="WorkItemsReport.Parse" /> in
    /// <see cref="WorkItemsTests.EveryPartOfAnItemIsOptionalAndABadPartIsLeftOut" /> - and not the record.</summary>
    [Fact]
    public void AWeekWithNoPartSaidIsStillAWeekAndNotTheNothing()
    {
        WorkWeek? nothing = null;

        Assert.NotNull(Unsaid);
        Assert.NotSame(Unsaid, Unsaid);
        Assert.False(Unsaid.Equals(nothing));
        Assert.False(Unsaid.Equals((object?)null));
        Assert.True(Unsaid == new WorkWeek(null, null, null));
        Assert.False(Unsaid != new WorkWeek(null, null, null));
        Assert.NotEqual(Unsaid, Full);
        Assert.NotEqual(Unsaid, Zeroed);
    }

    // --------------------------------- the moment, its offset, and the instant under it

    /// <summary>The same instant written under another offset is one week: the equality of the moment, which the
    /// record asks on its behalf, compares the instant and not the way it was written, so a week that started at
    /// midnight GMT and a week that started at two in the morning GMT+2 are one week, of one hash, and one entry
    /// of a set. The same clock under another offset is another instant and therefore another week - which is why
    /// a week read from a file that names its own offset is found again by the week the test writes, and a week
    /// whose clock was shifted is not.</summary>
    [Fact]
    public void TheSameInstantWrittenAtAnotherOffsetIsOneWeekWhileTheSameClockAtAnotherOffsetIsAnother()
    {
        var utc = new WorkWeek(WeekStart, 4, 2);
        var east = new WorkWeek(SameInstantFurtherEast, 4, 2);
        var shiftedClock = new WorkWeek(TheSameClockFurtherEast, 4, 2);

        Assert.Equal(utc, east);
        Assert.True(utc == east);
        Assert.False(utc != east);
        Assert.Equal(utc.GetHashCode(), east.GetHashCode());
        Assert.Single(new HashSet<WorkWeek> { utc, east });

        Assert.NotEqual(utc, shiftedClock);
        Assert.True(utc != shiftedClock);
        Assert.Equal(2, new HashSet<WorkWeek> { utc, shiftedClock }.Count);

        // The offset itself is held as it was written, so the two weeks that are one week are differently worded.
        Assert.Equal(TimeSpan.Zero, Assert.IsType<DateTimeOffset>(utc.WeekStart).Offset);
        Assert.Equal(TimeSpan.FromHours(2), Assert.IsType<DateTimeOffset>(east.WeekStart).Offset);
        Assert.NotEqual(CultureScope.Words(utc), CultureScope.Words(east));
    }

    // --------------------------------- the copy, and the parts falling apart

    /// <summary>The copy turns the part it names and keeps the other two: a week with a count of prod filled in,
    /// with its merged count turned to nought, with its moment moved a week on, with its unsaid parts filled and
    /// with its parts emptied. The week it was copied from stands as it stood, and a copy that names every part is
    /// another week that is one with the week it was made from.</summary>
    [Fact]
    public void ACopyTurnsThePartItNamesAndKeepsTheOtherTwo()
    {
        var week = Full;

        var prodFilled = week with { DeployedProd = 3 };
        Assert.Equal(WeekStart, prodFilled.WeekStart);
        Assert.Equal(4, prodFilled.Merged);
        Assert.Equal(3, prodFilled.DeployedProd);
        Assert.NotEqual(week, prodFilled);

        var mergedToNought = week with { Merged = 0 };
        Assert.Equal(WeekStart, mergedToNought.WeekStart);
        Assert.Equal(0, mergedToNought.Merged);
        Assert.Equal(2, mergedToNought.DeployedProd);

        var movedAWeekOn = week with { WeekStart = WeekStart.AddDays(7) };
        Assert.Equal(WeekStart.AddDays(7), movedAWeekOn.WeekStart);
        Assert.Equal(4, movedAWeekOn.Merged);
        Assert.Equal(2, movedAWeekOn.DeployedProd);

        // Every part may be turned to the nothing it started as, and the week that turns no part is another week
        // that is one with the week it was copied from.
        Assert.Equal(Unsaid, Full with { WeekStart = null, Merged = null, DeployedProd = null });
        var unchanged = week with { WeekStart = week.WeekStart, Merged = week.Merged, DeployedProd = week.DeployedProd };
        Assert.Equal(week, unchanged);
        Assert.NotSame(week, unchanged);
        Assert.Equal(week.GetHashCode(), unchanged.GetHashCode());
        Assert.Equal(CultureScope.Words(week), CultureScope.Words(unchanged));
        Assert.Equal(WeekStart, week.WeekStart);
        Assert.Equal(4, week.Merged);
        Assert.Equal(2, week.DeployedProd);
    }

    /// <summary>The three parts come back out in the order they went in - the moment, the merged count, the count
    /// that reached prod - and the parts nobody gave come out as the nothing they are, not as nought and not as the
    /// first moment. What came out goes back in the same way: the week the parts fall apart into is the week they
    /// were, which is what lets the chart and a test both take a week to pieces rather than reach into it.</summary>
    [Fact]
    public void TheThreePartsComeOutAgainInTheOrderTheyWentIn()
    {
        var (weekStart, merged, deployedProd) = Full;

        Assert.Equal(WeekStart, weekStart);
        Assert.Equal(4, merged);
        Assert.Equal(2, deployedProd);
        Assert.Equal(Full, new WorkWeek(weekStart, merged, deployedProd));

        var (noStart, noMerged, noDeployed) = Unsaid;

        Assert.Null(noStart);
        Assert.Null(noMerged);
        Assert.Null(noDeployed);
        Assert.Equal(Unsaid, new WorkWeek(noStart, noMerged, noDeployed));

        var (first, least, greatest) = new WorkWeek(DateTimeOffset.MinValue, int.MinValue, int.MaxValue);

        Assert.Equal(DateTimeOffset.MinValue, first);
        Assert.Equal(int.MinValue, least);
        Assert.Equal(int.MaxValue, greatest);
    }

    // --------------------------------- the equality, the hash, and the two operators

    /// <summary>Two weeks are one while their three parts are - the moment as the instant it stands on and each
    /// count as the number it stands on, the nothing among them included - whichever of the ways of asking is used:
    /// the record's own <c>Equals</c>, the one of <see cref="object" />, <c>==</c> and <c>!=</c>, from either side.
    /// The hash follows the equality, boxed or unboxed and from one call to the next, so the weeks that are one are
    /// one week of a set and one key of a dictionary: which is how a week the test writes is found again among the
    /// weeks the file was read into.</summary>
    [Fact]
    public void TwoWeeksAreOneWhileTheirThreePartsAre()
    {
        var one = Full;
        var two = new WorkWeek(WeekStart, 4, 2);

        Assert.True(one.Equals(two));
        Assert.True(one.Equals(one));
        Assert.True(one.Equals((object)two));
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.Equal(one, two);
        Assert.NotSame(one, two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Single(new HashSet<WorkWeek> { one, two });
        Assert.Contains(two, new HashSet<WorkWeek> { one });

        var byWeek = new Dictionary<WorkWeek, string> { [one] = "week 45" };
        Assert.Equal("week 45", byWeek[two]);

        // The week the file was read into is one with the week the test writes - the reading itself is pinned
        // through WorkItemsReport.Parse, and the equality that settles it is this one.
        var read = WorkItemsReport.Parse("""{ "stats": { "perWeek": [ { "weekStart": "2026-11-02T00:00:00Z", "merged": 4, "deployedProd": 2 } ] } }""")!;
        Assert.Equal(one, Assert.Single(read.Stats.PerWeek));
    }

    /// <summary>A difference in one part is another week: the merged count turned by one, the count of prod turned
    /// to the nothing, the moment moved an hour or moved to another offset of a different clock, the unsaid parts
    /// each filled, and the two counts swapped into each other's places. A set of the weeks that differ in one part
    /// each holds them all apart.</summary>
    [Fact]
    public void AWeekThatDiffersInOnePartIsAnotherWeek()
    {
        var one = Full;

        Assert.NotEqual(one, one with { Merged = 5 });
        Assert.NotEqual(one, one with { DeployedProd = 3 });
        Assert.NotEqual(one, one with { Merged = null });
        Assert.NotEqual(one, one with { DeployedProd = null });
        Assert.NotEqual(one, one with { WeekStart = null });
        Assert.NotEqual(one, one with { WeekStart = WeekStart.AddHours(1) });
        Assert.NotEqual(one, new WorkWeek(WeekStart, 2, 4));
        Assert.NotEqual(Zeroed, new WorkWeek(WeekStart, 0, -1));
        Assert.NotEqual(one, Zeroed);
        Assert.NotEqual(one, Unsaid);

        Assert.True(one != (one with { Merged = 5 }));
        Assert.False(one == (one with { Merged = 5 }));
        Assert.False(one.Equals((object)(one with { DeployedProd = null })));
        Assert.False(one.Equals(one with { WeekStart = TheSameClockFurtherEast }));

        // The two ends of a count, and the two ends of the moment, are weeks apart.
        Assert.Equal(
            4,
            new HashSet<WorkWeek>
            {
                one,
                new WorkWeek(WeekStart, int.MinValue, 2),
                new WorkWeek(WeekStart, int.MaxValue, 2),
                new WorkWeek(DateTimeOffset.MaxValue, 4, 2),
            }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two weeks that are no week at
    /// all are one, a week and no week are two - the equality and the inequality each saying it, from either side -
    /// and the week asked about itself says no to the nothing, to its own words as a name, to a bare number, to the
    /// anonymous thing that happens to carry its three parts, to the duration the same file counts hours with and
    /// to the bar the chart is made of.</summary>
    [Fact]
    public void NoWeekIsOneWithNoWeekAndNotWithAnyOtherThing()
    {
        WorkWeek? nothing = null;
        WorkWeek? alsoNothing = null;
        var there = Full;

        Assert.Null(default(WorkWeek));
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == (WorkWeek?)null);
        Assert.True(there != (WorkWeek?)null);
        Assert.True((WorkWeek?)null == nothing);
        Assert.False(there.Equals(nothing));
        Assert.False(there.Equals((object?)null));
        Assert.True(there.Equals(there));

        Assert.False(there.Equals("WorkWeek { WeekStart = 11/02/2026 00:00:00 +00:00, Merged = 4, DeployedProd = 2 }"));
        Assert.False(there.Equals((object)4));
        Assert.False(there.Equals((object)2));
        Assert.False(there.Equals(WeekStart));
        Assert.False(there.Equals(new { WeekStart, Merged = 4, DeployedProd = 2 }));
        Assert.False(there.Equals(new WorkDuration(4, 2, null)));
        Assert.False(there.Equals(new WorkWeekBar("Week of 2026-11-02", 4, 2, "Week of 2026-11-02: 4 merged, 2 deployed to prod")));
        Assert.Equal(2, new HashSet<WorkWeek?> { there, nothing, alsoNothing }.Count);
    }

    /// <summary>The hash says the three parts and nothing else: the same parts hashed again are the same hash, a
    /// part turned is another hash, and the ends of a count and of a moment are hashed with the part and not
    /// instead of it. The three weeks of nothing, of nought and of every part said stand apart in a set of their
    /// own, and the two weeks that are one instant written twice hash as one hash because they are one week.</summary>
    [Fact]
    public void TheHashFollowsThePartsAndStandsStillFromOneCallToTheNext()
    {
        var full = Full;
        var least = new WorkWeek(DateTimeOffset.MinValue, int.MinValue, int.MinValue);
        var greatest = new WorkWeek(DateTimeOffset.MaxValue, int.MaxValue, int.MaxValue);

        Assert.Equal(full.GetHashCode(), new WorkWeek(WeekStart, 4, 2).GetHashCode());
        Assert.Equal(Unsaid.GetHashCode(), Unsaid.GetHashCode());
        Assert.Equal(least.GetHashCode(), new WorkWeek(DateTimeOffset.MinValue, int.MinValue, int.MinValue).GetHashCode());
        Assert.NotEqual(full.GetHashCode(), least.GetHashCode());
        Assert.NotEqual(least.GetHashCode(), greatest.GetHashCode());
        Assert.NotEqual(full.GetHashCode(), (full with { Merged = 5 }).GetHashCode());
        Assert.NotEqual(full.GetHashCode(), (full with { DeployedProd = null }).GetHashCode());
        Assert.NotEqual(full.GetHashCode(), (full with { WeekStart = WeekStart.AddHours(1) }).GetHashCode());
        Assert.Equal(3, new HashSet<WorkWeek> { full, Unsaid, Zeroed }.Count);
        Assert.Equal(2, new HashSet<WorkWeek> { least, greatest }.Count);

        // The hash of the week and the hash of the same week held as a plain object are one hash - a dictionary
        // keyed by a week the file was read into is found by a week the test writes, boxed or not.
        Assert.Equal(full.GetHashCode(), ((object)new WorkWeek(WeekStart, 4, 2)).GetHashCode());
    }

    // --------------------------------- the words the week says itself

    /// <summary>The words the week says its three parts in: its own name at the front, the parts in the order the
    /// constructor names them, the moment as a reader of the invariant culture says it and each part nobody said as
    /// the empty place between the mark and the comma or the closing brace. These are the record's words and not
    /// the page's: no <c>Week of</c>, no <c>merged</c>, no <c>deployed to prod</c> and no <c>:</c> of a sentence -
    /// all of those are <see cref="WorkItemsText.Chart" />'s and are pinned in
    /// <see cref="WorkWeekChartTests" />.</summary>
    [Fact]
    public void TheRecordSaysItsThreePartsInTheOrderTheyAreNamed()
    {
        Assert.Equal("WorkWeek { WeekStart = 11/02/2026 00:00:00 +00:00, Merged = 4, DeployedProd = 2 }", Words(Full));
        Assert.Equal("WorkWeek { WeekStart = , Merged = , DeployedProd =  }", Words(Unsaid));
        Assert.Equal("WorkWeek { WeekStart = 11/02/2026 00:00:00 +00:00, Merged = 0, DeployedProd = 0 }", Words(Zeroed));
        Assert.Equal("WorkWeek { WeekStart = 11/02/2026 00:00:00 +00:00, Merged = , DeployedProd = 2 }", Words(new WorkWeek(WeekStart, null, 2)));
        Assert.Equal("WorkWeek { WeekStart = 11/09/2026 17:15:00 +00:00, Merged = 1, DeployedProd = 1 }", Words(new WorkWeek(new DateTimeOffset(2026, 11, 9, 17, 15, 0, TimeSpan.Zero), 1, 1)));
        Assert.Equal("WorkWeek { WeekStart = 11/02/2026 02:00:00 +02:00, Merged = 1, DeployedProd = 1 }", Words(new WorkWeek(SameInstantFurtherEast, 1, 1)));
        Assert.Equal("WorkWeek { WeekStart = 11/02/2026 00:00:00 -05:00, Merged = 1, DeployedProd = 1 }", Words(new WorkWeek(new DateTimeOffset(2026, 11, 2, 0, 0, 0, TimeSpan.FromHours(-5)), 1, 1)));
        Assert.Equal("WorkWeek { WeekStart = 01/01/0001 00:00:00 +00:00, Merged = 0, DeployedProd = 2147483647 }", Words(new WorkWeek(DateTimeOffset.MinValue, 0, int.MaxValue)));
        Assert.Equal("WorkWeek { WeekStart = 12/31/9999 23:59:59 +00:00, Merged = -1, DeployedProd = -2147483648 }", Words(new WorkWeek(DateTimeOffset.MaxValue, -1, int.MinValue)));

        // The words are no measure of the week: an unsaid part and a part of nought are said apart, the two weeks
        // that are one instant are worded apart, and the page's words for a week are words this record has never
        // heard of.
        Assert.NotEqual(Words(Unsaid), Words(Zeroed));
        Assert.NotEqual(Words(new WorkWeek(WeekStart, 4, 2)), Words(new WorkWeek(SameInstantFurtherEast, 4, 2)));
        foreach (var word in new[] { "Week of", "merged", "deployed", " prod", " · ", "n/a", "—", "<" })
        {
            Assert.DoesNotContain(word, Words(Full), StringComparison.Ordinal);
        }
    }

    /// <summary>The words of the moment at either end of its range and under either offset, and the words of the
    /// counts at either end of theirs: the record says each as the value it stands on, with no label and no dash.
    /// A fraction of a second is held and not said - the moment's own words stop at the second - so two weeks a
    /// thousandth of a second apart are another pair of weeks in every other member here and one pair of weeks in
    /// these words.</summary>
    [Fact]
    public void TheWordsSayThePartsAtWhateverEndTheyStandOnAndSayNoFractionOfASecond()
    {
        Assert.Contains("WeekStart = 01/01/0001 00:00:00 +00:00", Words(new WorkWeek(DateTimeOffset.MinValue, null, null)), StringComparison.Ordinal);
        Assert.Contains("WeekStart = 12/31/9999 23:59:59 +00:00", Words(new WorkWeek(DateTimeOffset.MaxValue, null, null)), StringComparison.Ordinal);
        Assert.Contains("Merged = -2147483648, DeployedProd = 2147483647", Words(new WorkWeek(null, int.MinValue, int.MaxValue)), StringComparison.Ordinal);

        var whole = new WorkWeek(new DateTimeOffset(2026, 11, 2, 0, 0, 0, TimeSpan.Zero), 1, 1);
        var partOfASecond = new WorkWeek(new DateTimeOffset(2026, 11, 2, 0, 0, 0, 1, TimeSpan.Zero), 1, 1);

        Assert.Equal(Words(whole), Words(partOfASecond));
        Assert.NotEqual(whole, partOfASecond);
        Assert.NotEqual(whole.GetHashCode(), partOfASecond.GetHashCode());
        Assert.Equal(2, new HashSet<WorkWeek> { whole, partOfASecond }.Count);
        Assert.Equal(TimeSpan.Zero, Assert.IsType<DateTimeOffset>(partOfASecond.WeekStart).Offset);
    }

    /// <summary>The words in the moment follow whoever reads them, which is what sets them apart from the parts
    /// themselves: a German reader of the record is told <c>02.11.2026</c> and an English one <c>11/2/2026</c>,
    /// while the part names stay the same letters to whichever reader - and two weeks the two readers see
    /// differently worded are one week still, for the words are no part of the equality.</summary>
    [Fact]
    public void TheRecordsOwnWordsFollowTheReaderWhileThePartsThemselvesDoNot()
    {
        var week = Full;

        Assert.Equal("WorkWeek { WeekStart = 11/02/2026 00:00:00 +00:00, Merged = 4, DeployedProd = 2 }", Words(week));

        var english = CultureScope.Says(new CultureInfo("en-US"), () => week.ToString()!);
        Assert.Contains("11/2/2026", english, StringComparison.Ordinal);

        var german = CultureScope.Says(new CultureInfo("de-DE"), () => week.ToString()!);
        Assert.Contains("02.11.2026", german, StringComparison.Ordinal);
        Assert.Contains("Merged = 4, DeployedProd = 2", german, StringComparison.Ordinal);
        Assert.NotEqual(english, german);

        // The words apart by a date, the week the same: whoever reads the file, and whoever shows it, decide how a
        // week is written; the record only holds the three parts, and hashes them the same whichever reader is asked.
        Assert.Equal(week, new WorkWeek(WeekStart, 4, 2));
        Assert.Equal(week.GetHashCode(), CultureScope.Says(new CultureInfo("de-DE"), () => new WorkWeek(WeekStart, 4, 2).GetHashCode()));
    }

    // --------------------------------- the surface

    /// <summary>The week's whole public surface, as the running assembly holds it: its three parts named in the
    /// order the constructor names them, and the members every record is asked - falling apart, comparing, hashing
    /// and saying itself - and nothing else to call. The way in is one: the three parts, none of them left out, for
    /// a week with no parts at all is the nothing the reading of a file calls for and is had from
    /// <c>WorkWeek.Read</c>, not from a shorter way in. A fourth part added to the record has to be written here as
    /// well, which is the point: the chart cannot be shown a part nobody decided what it means. And nothing on it is
    /// a clock to ask, an address to follow or a length of time to measure - one moment held and two counts held,
    /// which is why no test in this file needs a clock of its own.</summary>
    [Fact]
    public void TheRecordHasThreePartsAndTheMembersEveryRecordIsAsked()
    {
        var type = typeof(WorkWeek);

        Assert.True(type.IsSealed);
        Assert.False(type.IsValueType);
        Assert.Equal(typeof(object), type.BaseType);
        Assert.Equal([typeof(IEquatable<WorkWeek>)], type.GetInterfaces());

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.Equal(["WeekStart", "Merged", "DeployedProd"], properties.Select(property => property.Name));
        Assert.Equal(typeof(DateTimeOffset?), properties[0].PropertyType);
        Assert.Equal(typeof(int?), properties[1].PropertyType);
        Assert.Equal(typeof(int?), properties[2].PropertyType);
        Assert.All(properties, property =>
        {
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.NotNull(property.SetMethod);
            Assert.True(property.GetMethod!.IsPublic);

            // The parts are init-only: they are said once, at the making or at the copy, and never turned after.
            Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());

            // And each of the three is a part that may be unsaid - never a reference to follow, never a span of time.
            Assert.NotNull(Nullable.GetUnderlyingType(property.PropertyType));
            Assert.DoesNotContain(
                property.PropertyType,
                new[] { typeof(TimeSpan), typeof(Uri), typeof(string), typeof(IReadOnlyList<int>) });
        });

        // The contract every record compares by is the compiler's private one: no public part of the week.
        Assert.Null(type.GetProperty("EqualityContract", BindingFlags.Public | BindingFlags.Instance));
        Assert.NotNull(type.GetProperty("EqualityContract", BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.DoesNotContain("EqualityContract", PublicSurface(), StringComparer.Ordinal);

        Assert.Equal(
            ["Deconstruct", "DeployedProd", "Equals", "GetHashCode", "Merged", "ToString", "WeekStart", "op_Equality", "op_Inequality"],
            PublicSurface());

        var arguments = type.GetConstructors().Single().GetParameters();

        Assert.Single(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.Equal(3, arguments.Length);
        Assert.Equal(["WeekStart", "Merged", "DeployedProd"], arguments.Select(argument => argument.Name));
        Assert.Equal([typeof(DateTimeOffset?), typeof(int?), typeof(int?)], arguments.Select(argument => argument.ParameterType));
        Assert.All(arguments, argument => Assert.False(argument.HasDefaultValue));

        // The copy the with walks is the compiler's private one, and the reading of the file is no public member.
        var copy = Assert.Single(type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.Equal(type, copy.GetParameters().Single().ParameterType);
        Assert.Null(type.GetMethod("Read", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));

        // The two overloads of the equality every record is asked with, and the one Deconstruct.
        Assert.Equal(2, type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Count(method => method.Name == "Equals"));
        var parts = type.GetMethod("Deconstruct")!;
        Assert.Equal(["WeekStart", "Merged", "DeployedProd"], parts.GetParameters().Select(parameter => parameter.Name));
        Assert.All(parts.GetParameters(), parameter => Assert.True(parameter.IsOut));

        // And no member of it is given a length of time, an address or a text to go and ask about.
        var other = type
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .SelectMany(method => method.GetParameters())
            .Select(parameter => parameter.ParameterType)
            .Where(parameter => parameter != type)
            .ToList();
        Assert.DoesNotContain(typeof(TimeSpan), other);
        Assert.DoesNotContain(typeof(DateTime), other);
        Assert.DoesNotContain(typeof(Uri), other);
        Assert.DoesNotContain(typeof(string), other);
    }

    // --------------------------------------------------------------------------- the machinery

    /// <summary>The public members the week declares itself: its three properties and the methods that are not the
    /// get and the set of one - the two operators kept although the compiler names them specially, since they are
    /// the <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the test of the surface rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(WorkWeek)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words a week says its three parts in, taken in the invariant culture whatever the reader of
    /// this test happens to be: the record's own words follow whoever reads them, which is pinned on its own
    /// further up this file, so the strings there are the point and the culture only the way to say them once.</summary>
    private static string Words(WorkWeek week) => CultureScope.Words(week);
}
