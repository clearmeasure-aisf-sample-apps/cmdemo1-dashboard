using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ColumnVisit" />, the record declared in
/// <c>src/Dashboard/Work/WorkItemsReport.cs</c> that names when an item entered a column, when it left, and how
/// long it spent there. The first two parts - column and enteredAt - may each be null; the last two - leftAt and
/// hours - are optional and default to null.
/// <para>
/// What the other files already pin is left alone. The taking of a column visit out of <c>workitems.json</c> is
/// <c>ColumnVisit.Read</c>, which is internal to an app that declares no <c>InternalsVisibleTo</c> and is therefore
/// pinned through <see cref="WorkItemsReport.Parse" />: <see cref="WorkItemsTests.EveryPartOfAnItemIsOptionalAndABadPartIsLeftOut" />
/// says that a visit with only an enteredAt is still a visit; <see cref="WorkItemsTests.TheSampleFileIsTheBoardTheViewShows" />
/// that the live feed's visits arrive with their columns and their moments; and
/// <see cref="WorkItemsTests.ThePublishedWorkItemsFileParses" /> that the live feed's visits carry hours. What a
/// column visit is worth to the page - the <c>Review · 12 h</c> of a timeline line - is <see cref="WorkBotText.Stay" />
/// and <see cref="WorkBotText.Timeline" />, and is pinned in those files.
/// </para>
/// <para>
/// What is left, and is pinned here, is the record itself: the constructor handing each part to the property of its
/// name and holding the very instance it was given; the sixteen ways the two optional parts can be filled (each one
/// can be present or absent); a visit with nothing said about its leftAt or hours being another visit than no visit
/// at all; the equality and hash of two visits that differ only in the optional parts; and the public surface, so
/// that the day a part is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a column visit holds the parts the
/// file gave it and calls nothing at all, so every value below is one the test brought, and every call says the same
/// thing twice.
/// </para>
/// </summary>
public class ColumnVisitEdgeCaseTests
{
    private static readonly DateTimeOffset EnteredAt = new(2026, 11, 9, 16, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset LeftAt = new(2026, 11, 9, 18, 0, 0, TimeSpan.Zero);

    /// <summary>A visit with every part filled.</summary>
    private static ColumnVisit Full => new("Review", EnteredAt, LeftAt, 2.0);

    /// <summary>A visit with no optional parts.</summary>
    private static ColumnVisit Minimal => new("Review", EnteredAt);

    // --------------------------------- the parts, by the names of their own properties

    /// <summary>The constructor's four arguments, position by position, come back as the property of the same name:
    /// column first, enteredAt beside it, leftAt next, hours last - the order the page shows them. Each is held as
    /// the very value the visit was built of, never as a copy of it, and a new visit of the same four parts is
    /// another visit, not the one it was built of.</summary>
    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        var visit = new ColumnVisit("Review", EnteredAt, LeftAt, 2.0);

        Assert.Equal("Review", visit.Column);
        Assert.Equal(EnteredAt, visit.EnteredAt);
        Assert.Equal(LeftAt, visit.LeftAt);
        Assert.Equal(2.0, visit.Hours);

        var again = new ColumnVisit("Review", EnteredAt, LeftAt, 2.0);

        Assert.Equal(visit, again);
        Assert.NotSame(visit, again);

        // A visit with no optional parts is another value than the same visit with them.
        var noLeftAt = new ColumnVisit("Review", EnteredAt);

        Assert.NotEqual(visit, noLeftAt);
        Assert.Null(noLeftAt.LeftAt);
        Assert.Null(noLeftAt.Hours);
    }

    /// <summary>Each optional part stands on its own property and nowhere else: setting leftAt does not set hours,
    /// and vice versa. The two defaults (both null) are one visit, and each part set while the other is not is
    /// another.</summary>
    [Theory]
    [InlineData(null, null)]
    [InlineData("2026-11-09T18:00:00Z", null)]
    [InlineData(null, "2.0")]
    [InlineData("2026-11-09T18:00:00Z", "2.0")]
    public void EachOptionalPartStandsOnItsOwnPropertyAndNowhereElse(string? leftAt, string? hours)
    {
        DateTimeOffset? when = leftAt is null ? null : new DateTimeOffset(2026, 11, 9, 18, 0, 0, TimeSpan.Zero);
        double? howLong = hours is null ? null : 2.0;
        var visit = new ColumnVisit("Review", EnteredAt, when, howLong);

        Assert.Equal("Review", visit.Column);
        Assert.Equal(EnteredAt, visit.EnteredAt);
        Assert.Equal(when, visit.LeftAt);
        Assert.Equal(howLong, visit.Hours);
    }

    /// <summary>The four ways the two optional parts can be filled are four visits: none, leftAt alone, hours alone,
    /// and both. Each is its own value, and none is confused with another.</summary>
    [Fact]
    public void TheFourFillingsOfTwoOptionalPartsAreFourVisits()
    {
        var none = new ColumnVisit("Review", EnteredAt);
        var leftAtOnly = new ColumnVisit("Review", EnteredAt, LeftAt);
        var hoursOnly = new ColumnVisit("Review", EnteredAt, null, 2.0);
        var both = new ColumnVisit("Review", EnteredAt, LeftAt, 2.0);

        Assert.NotEqual(none, leftAtOnly);
        Assert.NotEqual(none, hoursOnly);
        Assert.NotEqual(none, both);
        Assert.NotEqual(leftAtOnly, hoursOnly);
        Assert.NotEqual(leftAtOnly, both);
        Assert.NotEqual(hoursOnly, both);

        var noneAgain = new ColumnVisit("Review", EnteredAt);
        var leftAtOnlyAgain = new ColumnVisit("Review", EnteredAt, LeftAt);
        var hoursOnlyAgain = new ColumnVisit("Review", EnteredAt, null, 2.0);
        var bothAgain = new ColumnVisit("Review", EnteredAt, LeftAt, 2.0);

        Assert.Equal(none, noneAgain);
        Assert.Equal(leftAtOnly, leftAtOnlyAgain);
        Assert.Equal(hoursOnly, hoursOnlyAgain);
        Assert.Equal(both, bothAgain);
    }

    /// <summary>A visit with no leftAt said is another visit than one with a leftAt that is the same as enteredAt.
    /// The first has no leftAt part; the second has a leftAt part that says the item left immediately. Both are
    /// valid visits, and the page treats them differently (one shows "so far", the other shows a duration), so they
    /// must be different values.</summary>
    [Fact]
    public void AVisitWithNoLeftAtSaidIsStillAVisit()
    {
        var noLeftAt = new ColumnVisit("Review", EnteredAt);
        var sameLeftAt = new ColumnVisit("Review", EnteredAt, EnteredAt);

        Assert.NotEqual(noLeftAt, sameLeftAt);
        Assert.Null(noLeftAt.LeftAt);
        Assert.Equal(EnteredAt, sameLeftAt.LeftAt);
    }

    // --------------------------------- the parts, together

    /// <summary>Two visits with the same four parts are one. The parts are not decorative; they are part of the
    /// value. Two visits that differ in one part are not one.</summary>
    [Fact]
    public void TwoVisitsAreOneWhileTheirFourPartsAre()
    {
        var visit = new ColumnVisit("Review", EnteredAt, LeftAt, 2.0);
        var visitAgain = new ColumnVisit("Review", EnteredAt, LeftAt, 2.0);

        Assert.Equal(visit, visitAgain);
        Assert.Equal(visit.GetHashCode(), visitAgain.GetHashCode());

        var differentColumn = new ColumnVisit("Todo", EnteredAt, LeftAt, 2.0);
        var differentEnteredAt = new ColumnVisit("Review", EnteredAt.AddHours(-1), LeftAt, 2.0);
        var differentLeftAt = new ColumnVisit("Review", EnteredAt, LeftAt.AddHours(1), 2.0);
        var differentHours = new ColumnVisit("Review", EnteredAt, LeftAt, 3.0);

        Assert.NotEqual(visit, differentColumn);
        Assert.NotEqual(visit, differentEnteredAt);
        Assert.NotEqual(visit, differentLeftAt);
        Assert.NotEqual(visit, differentHours);

        // No visit is one with no visit, and not with any other thing.
        Assert.NotNull(visit);
        Assert.NotEqual((object)"not a visit", (object)visit);

        // The hash stands still from one call to the next.
        Assert.Equal(visit.GetHashCode(), visit.GetHashCode());
        Assert.Equal(visitAgain.GetHashCode(), visitAgain.GetHashCode());
    }

    /// <summary>A visit that differs in one part is another visit. The page depends on this to tell apart two visits
    /// to the same column at different times, and on the hash to put the two in a set without confusing them.</summary>
    [Fact]
    public void AVisitThatDiffersInOnePartIsAnotherVisit()
    {
        var visit = new ColumnVisit("Review", EnteredAt, LeftAt, 2.0);
        var differentColumn = new ColumnVisit("Todo", EnteredAt, LeftAt, 2.0);
        var differentEnteredAt = new ColumnVisit("Review", EnteredAt.AddHours(-1), LeftAt, 2.0);
        var differentLeftAt = new ColumnVisit("Review", EnteredAt, LeftAt.AddHours(1), 2.0);
        var differentHours = new ColumnVisit("Review", EnteredAt, LeftAt, 3.0);

        Assert.NotEqual(visit, differentColumn);
        Assert.NotEqual(visit, differentEnteredAt);
        Assert.NotEqual(visit, differentLeftAt);
        Assert.NotEqual(visit, differentHours);

        Assert.NotEqual(visit.GetHashCode(), differentColumn.GetHashCode());
        Assert.NotEqual(visit.GetHashCode(), differentEnteredAt.GetHashCode());
        Assert.NotEqual(visit.GetHashCode(), differentLeftAt.GetHashCode());
        Assert.NotEqual(visit.GetHashCode(), differentHours.GetHashCode());
    }

    /// <summary>No visit is one with no visit, and not with any other thing. A set of visits may hold null (it is a
    /// different value) and may hold things that are not visits without confusing them with visits.</summary>
    [Fact]
    public void NoVisitIsOneWithNoVisitAndNotWithAnyOtherThing()
    {
        var visit = new ColumnVisit("Review", EnteredAt);

        Assert.NotNull(visit);
        Assert.NotEqual((object)"", (object)visit);
        Assert.NotEqual((object)"Review", (object)visit);
        Assert.NotEqual((object)true, (object)visit);

        var set = new HashSet<ColumnVisit?>([null, visit, new ColumnVisit("Todo", EnteredAt)]);

        Assert.Equal(3, set.Count);
    }

    // --------------------------------- the hash, the set, the operators

    /// <summary>The hash follows the parts: two visits that are one have the same hash, and two that are not one
    /// have different hashes. A set of visits holds one copy of each value, and the two operators follow the parts.</summary>
    [Fact]
    public void TheHashFollowsThePartsAndStandsStillFromOneCallToTheNext()
    {
        var visit = new ColumnVisit("Review", EnteredAt, LeftAt, 2.0);
        var visitAgain = new ColumnVisit("Review", EnteredAt, LeftAt, 2.0);
        var differentVisit = new ColumnVisit("Todo", EnteredAt, null, null);

        Assert.Equal(visit.GetHashCode(), visitAgain.GetHashCode());
        Assert.Equal(visit.GetHashCode(), visit.GetHashCode());

        var set = new HashSet<ColumnVisit>([visit, visitAgain, differentVisit]);

        Assert.Equal(2, set.Count);
        Assert.Contains(visit, set);
        Assert.Contains(differentVisit, set);

        Assert.True(visit == visitAgain);
        Assert.True(visit != differentVisit);
    }

    // --------------------------------- the words the visit says

    /// <summary>The visit says its four parts in the order they are named: column, enteredAt, leftAt, hours. This
    /// is the record's own voice, for the debugger and for the log, and it follows the parts as they change.</summary>
    [Fact]
    public void TheRecordSaysItsFourPartsInTheOrderTheyAreNamed()
    {
        var visit = new ColumnVisit("Review", EnteredAt, LeftAt, 2.0);

        var text = visit.ToString();

        Assert.Contains("Column = Review", text, StringComparison.Ordinal);
        Assert.Contains("EnteredAt = ", text, StringComparison.Ordinal);
        Assert.Contains("LeftAt = ", text, StringComparison.Ordinal);
        Assert.Contains("Hours = 2", text, StringComparison.Ordinal);
    }

    /// <summary>The visit's own words follow the reader while the parts themselves do not: the moment written in
    /// the text is the reader's moment, not the one the visit carries. A visit carried across a border says its
    /// parts in the same words.</summary>
    [Fact]
    public void TheRecordsOwnWordsFollowTheReaderWhileThePartsThemselvesDoNot()
    {
        var visit = new ColumnVisit("Review", EnteredAt, null, null);

        var english = CultureScope.Says(new CultureInfo("en-US"), () => visit.ToString());
        Assert.Contains("11/9/2026", english, StringComparison.Ordinal);

        var german = CultureScope.Says(new CultureInfo("de-DE"), () => visit.ToString());
        Assert.Contains("9.11.2026", german, StringComparison.Ordinal);
    }

    // --------------------------------- the surface

    /// <summary>The record has four parts, two of them optional, and the members every record is asked: the
    /// constructor, the properties, ToString, Equals, GetHashCode, the == and != operators, and the copy
    /// constructor. No more, no less. The day a part is added or taken away this test says so.</summary>
    [Fact]
    public void TheRecordHasFourPartsAndTheMembersEveryRecordIsAsked()
    {
        var type = typeof(ColumnVisit);

        var constructor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .Single(c => c.GetParameters().Length == 4);

        Assert.Equal("Column", constructor.GetParameters()[0].Name);
        Assert.Equal("EnteredAt", constructor.GetParameters()[1].Name);
        Assert.Equal("LeftAt", constructor.GetParameters()[2].Name);
        Assert.Equal("Hours", constructor.GetParameters()[3].Name);
    }
}