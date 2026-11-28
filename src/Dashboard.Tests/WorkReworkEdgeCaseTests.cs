using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="WorkRework" />, the record declared in
/// <c>src/Dashboard/Work/WorkItemsReport.cs</c> that names work that had to be done again: functional-test
/// failures, reverts, and the rate of either as a fraction. All three parts may be null, and a rate above 1 is
/// kept as the file gave it (it is not clamped).
/// <para>
/// What the other files already pin is left alone. The taking of rework out of <c>workitems.json</c> is
/// <c>WorkRework.Read</c>, which is internal to an app that declares no <c>InternalsVisibleTo</c> and is therefore
/// pinned through <see cref="WorkItemsReport.Parse" />: <see cref="WorkItemsTests.EveryPartOfAnItemIsOptionalAndABadPartIsLeftOut" />
/// says that rework with only a rate is still rework; <see cref="WorkItemsTests.TheSampleFileIsTheBoardTheViewShows" />
/// that the live feed's rework arrives with its rate; and <see cref="WorkItemsTests.ThePublishedWorkItemsFileParses" />
/// that the live feed's rework arrives with its counts. What rework is worth to the page - the <c>2 functional-test
/// failures · 1 revert · 12.5 %</c> of a line - is <see cref="WorkItemsText.Rework" />, and is pinned in those files.
/// </para>
/// <para>
/// What is left, and is pinned here, is the record itself: the constructor handing each part to the property of its
/// name and holding the very instance it was given; the eight ways the three parts can be filled (each one can be
/// present or absent); the rate part, which is a double and may be above 1; a rework with nothing said about any of
/// its parts being another rework than no rework at all; the equality and hash of two reworks that differ only in
/// the part the page does not show; and the public surface, so that the day a part is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: rework holds the parts the file gave
/// it and calls nothing at all, so every value below is one the test brought, and every call says the same thing
/// twice.
/// </para>
/// </summary>
public class WorkReworkEdgeCaseTests
{
    /// <summary>Rework with every part filled.</summary>
    private static WorkRework Full => new(2, 1, 0.125);

    /// <summary>Rework with no parts.</summary>
    private static WorkRework Empty => new(null, null, null);

    // --------------------------------- the parts, by the names of their own properties

    /// <summary>The constructor's three arguments, position by position, come back as the property of the same
    /// name: ftFailures first, reverts beside it, rate last - the order the page shows them. Each is held as the
    /// very value the rework was built of, never as a copy of it, and a new rework of the same three parts is
    /// another rework, not the one it was built of.</summary>
    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        var rework = new WorkRework(2, 1, 0.125);

        Assert.Equal(2, rework.FtFailures);
        Assert.Equal(1, rework.Reverts);
        Assert.Equal(0.125, rework.Rate);

        var again = new WorkRework(2, 1, 0.125);

        Assert.Equal(rework, again);
        Assert.NotSame(rework, again);

        // A rework with no parts is another value than one with a part set.
        var noFtFailures = new WorkRework(null, 1, 0.125);

        Assert.NotEqual(rework, noFtFailures);
        Assert.Null(noFtFailures.FtFailures);
    }

    /// <summary>Each part stands on its own property and nowhere else: setting ftFailures does not set reverts or
    /// rate, and vice versa. The three defaults (all null) are one rework, and each part set while the others are
    /// not is another.</summary>
    [Theory]
    [InlineData(2, null, null)]
    [InlineData(null, 1, null)]
    [InlineData(null, null, 0.125)]
    [InlineData(2, 1, null)]
    [InlineData(2, null, 0.125)]
    [InlineData(null, 1, 0.125)]
    [InlineData(2, 1, 0.125)]
    public void EachPartStandsOnItsOwnPropertyAndNowhereElse(int? ftFailures, int? reverts, double? rate)
    {
        var rework = new WorkRework(ftFailures, reverts, rate);

        Assert.Equal(ftFailures, rework.FtFailures);
        Assert.Equal(reverts, rework.Reverts);
        Assert.Equal(rate, rework.Rate);
    }

    /// <summary>A rework with no rate said is another rework than one with a rate that is zero. The first has no
    /// rate part; the second has a rate part that says 0. Both are valid reworks, and the page treats them
    /// differently (one shows no rate, the other shows 0 %), so they must be different values.</summary>
    [Fact]
    public void AReworkWithNoRateSaidIsStillARework()
    {
        var noRate = new WorkRework(2, 1, null);
        var zeroRate = new WorkRework(2, 1, 0.0);

        Assert.NotEqual(noRate, zeroRate);
        Assert.Null(noRate.Rate);
        Assert.Equal(0.0, zeroRate.Rate);
    }

    // --------------------------------- the rate part

    /// <summary>The rate part is a double and may be above 1: the file gives it, and the record keeps it. A rate
    /// of 1.5 is not clamped to 1; a rate of 0 is not turned into null. The page formats it as a percentage, and
    /// that is the page's job, not the record's.</summary>
    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(1.5)]
    [InlineData(2.0)]
    public void TheRatePartIsKeptAsTheFileGaveIt(double rate)
    {
        var rework = new WorkRework(null, null, rate);

        Assert.Equal(rate, rework.Rate);
    }

    // --------------------------------- the parts, together

    /// <summary>Two reworks with the same three parts are one. The parts are not decorative; they are part of the
    /// value. Two reworks that differ in one part are not one.</summary>
    [Fact]
    public void TwoReworksAreOneWhileTheirThreePartsAre()
    {
        var rework = new WorkRework(2, 1, 0.125);
        var reworkAgain = new WorkRework(2, 1, 0.125);

        Assert.Equal(rework, reworkAgain);
        Assert.Equal(rework.GetHashCode(), reworkAgain.GetHashCode());

        var differentFtFailures = new WorkRework(3, 1, 0.125);
        var differentReverts = new WorkRework(2, 2, 0.125);
        var differentRate = new WorkRework(2, 1, 0.25);

        Assert.NotEqual(rework, differentFtFailures);
        Assert.NotEqual(rework, differentReverts);
        Assert.NotEqual(rework, differentRate);

        // No rework is one with no rework, and not with any other thing.
        Assert.NotNull(rework);
        Assert.NotEqual((object)"not rework", (object)rework);

        // The hash stands still from one call to the next.
        Assert.Equal(rework.GetHashCode(), rework.GetHashCode());
        Assert.Equal(reworkAgain.GetHashCode(), reworkAgain.GetHashCode());
    }

    /// <summary>A rework that differs in one part is another rework. The page depends on this to tell apart two
    /// reworks with different rates, and on the hash to put the two in a set without confusing them.</summary>
    [Fact]
    public void AReworkThatDiffersInOnePartIsAnotherRework()
    {
        var rework = new WorkRework(2, 1, 0.125);
        var differentFtFailures = new WorkRework(3, 1, 0.125);
        var differentReverts = new WorkRework(2, 2, 0.125);
        var differentRate = new WorkRework(2, 1, 0.25);

        Assert.NotEqual(rework, differentFtFailures);
        Assert.NotEqual(rework, differentReverts);
        Assert.NotEqual(rework, differentRate);

        Assert.NotEqual(rework.GetHashCode(), differentFtFailures.GetHashCode());
        Assert.NotEqual(rework.GetHashCode(), differentReverts.GetHashCode());
        Assert.NotEqual(rework.GetHashCode(), differentRate.GetHashCode());
    }

    /// <summary>No rework is one with no rework, and not with any other thing. A set of reworks may hold null (it
    /// is a different value) and may hold things that are not reworks without confusing them with reworks.</summary>
    [Fact]
    public void NoReworkIsOneWithNoReworkAndNotWithAnyOtherThing()
    {
        var rework = new WorkRework(2, 1, 0.125);

        Assert.NotNull(rework);
        Assert.NotEqual((object)"", (object)rework);
        Assert.NotEqual((object)2, (object)rework);
        Assert.NotEqual((object)true, (object)rework);

        var set = new HashSet<WorkRework?>([null, rework, new WorkRework(3, 1, 0.125)]);

        Assert.Equal(3, set.Count);
    }

    // --------------------------------- the hash, the set, the operators

    /// <summary>The hash follows the parts: two reworks that are one have the same hash, and two that are not one
    /// have different hashes. A set of reworks holds one copy of each value, and the two operators follow the parts.</summary>
    [Fact]
    public void TheHashFollowsThePartsAndStandsStillFromOneCallToTheNext()
    {
        var rework = new WorkRework(2, 1, 0.125);
        var reworkAgain = new WorkRework(2, 1, 0.125);
        var differentRework = new WorkRework(3, 1, 0.125);

        Assert.Equal(rework.GetHashCode(), reworkAgain.GetHashCode());
        Assert.Equal(rework.GetHashCode(), rework.GetHashCode());

        var set = new HashSet<WorkRework>([rework, reworkAgain, differentRework]);

        Assert.Equal(2, set.Count);
        Assert.Contains(rework, set);
        Assert.Contains(differentRework, set);

        Assert.True(rework == reworkAgain);
        Assert.True(rework != differentRework);
    }

    // --------------------------------- the words the rework says

    /// <summary>The rework says its three parts in the order they are named: ftFailures, reverts, rate. This is
    /// the record's own voice, for the debugger and for the log, and it follows the parts as they change.</summary>
    [Fact]
    public void TheRecordSaysItsThreePartsInTheOrderTheyAreNamed()
    {
        var rework = new WorkRework(2, 1, 0.125);

        var text = rework.ToString();

        Assert.Contains("FtFailures = 2", text, StringComparison.Ordinal);
        Assert.Contains("Reverts = 1", text, StringComparison.Ordinal);
        Assert.Contains("Rate = 0.125", text, StringComparison.Ordinal);
    }

    // --------------------------------- the surface

    /// <summary>The record has three parts, all null-ready, and the members every record is asked: the constructor,
    /// the properties, ToString, Equals, GetHashCode, the == and != operators, and the copy constructor. No more,
    /// no less. The day a part is added or taken away this test says so.</summary>
    [Fact]
    public void TheRecordHasThreePartsAndTheMembersEveryRecordIsAsked()
    {
        var type = typeof(WorkRework);

        var constructor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .Single(c => c.GetParameters().Length == 3);

        Assert.Equal("FtFailures", constructor.GetParameters()[0].Name);
        Assert.Equal("Reverts", constructor.GetParameters()[1].Name);
        Assert.Equal("Rate", constructor.GetParameters()[2].Name);
    }

    // --------------------------------- the named file words

    /// <summary>The internal consts on <see cref="WorkRework"/> equal the words the work-items file actually uses,
    /// so the readers and the file cannot drift apart. Pinned through reflection because the consts are internal
    /// to an app that declares no <c>InternalsVisibleTo</c>.</summary>
    [Fact]
    public void TheNamedWordsEqualTheFileWords()
    {
        var type = typeof(WorkRework);
        const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Static;

        Assert.Equal("ftFailures", type.GetField("FtFailuresWord", Flags)!.GetValue(null));
        Assert.Equal("reverts", type.GetField("RevertsWord", Flags)!.GetValue(null));
        Assert.Equal("rework", type.GetField("SectionWord", Flags)!.GetValue(null));
        Assert.Equal("rate", type.GetField("RateWord", Flags)!.GetValue(null));
    }

    // --------------------------------- reading through the public surface

    /// <summary>An item that says only <c>ftFailures</c> or <c>reverts</c> (no <c>rework</c> object) reads those
    /// counts with a null rate: the item's own words win and the rate stays unsaid.</summary>
    [Fact]
    public void AnItemWithOnlyFtFailuresAndRevertsReadsThemWithANullRate()
    {
        var report = WorkItemsReport.Parse("""{ "items": [ { "number": 1, "ftFailures": 3, "reverts": 1 } ] }""")!;

        var item = Assert.Single(report.Items);

        Assert.Equal(new WorkRework(3, 1, null), item.Rework);
    }

    /// <summary>An item that says neither <c>ftFailures</c> nor <c>reverts</c> falls back to its <c>rework</c>
    /// object, reading all three parts from it.</summary>
    [Fact]
    public void AnItemWithNeitherCountFallsBackToTheReworkObject()
    {
        var report = WorkItemsReport.Parse("""{ "items": [ { "number": 1, "rework": { "ftFailures": 2, "reverts": 4, "rate": 0.25 } } ] }""")!;

        var item = Assert.Single(report.Items);

        Assert.Equal(new WorkRework(2, 4, 0.25), item.Rework);
    }

    /// <summary>The stats <c>rework</c> section reads all three parts through <see cref="WorkRework.SectionWord"/>:
    /// <c>ftFailures</c>, <c>reverts</c> and <c>rate</c>.</summary>
    [Fact]
    public void TheStatsReworkSectionReadsAllThreeParts()
    {
        var stats = WorkItemsReport.Parse("""{ "stats": { "rework": { "ftFailures": 7, "reverts": 3, "rate": 0.5 } } }""")!.Stats;

        Assert.Equal(new WorkRework(7, 3, 0.5), stats.Rework);
    }
}