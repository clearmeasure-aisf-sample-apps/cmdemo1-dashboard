using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="CostAmounts" /> is three sums of complete UTC days - the last one, the seven that end with it, and the
/// month so far up to it - each of which Azure may simply not have given, in which case its part is nothing and the
/// page says a dash. It is the record every other cost type stands on: an entry's, the shared entry's, the whole
/// system's and the part of what is shared that an environment is estimated to carry.
/// <para>
/// What the file makes of it is already pinned where the reading happens: <see cref="CostReportTests" /> reads a
/// <c>cost.json</c> written by hand and says that an entry Azure gave no number to has no amount, that one number of
/// three still stands beside the two that are missing, and that a refund keeps its sign - each of them as one whole
/// record compared with a hand-built one; and <see cref="CostTests" /> pins the same record as the sample file's
/// <c>system</c>, <c>uat</c> and <c>shared</c> amounts and as the sum a monitor keeps between two rounds. The two
/// members that fill the record, <c>Read</c> and <c>Amount</c>, are internal to an app that declares no
/// <c>InternalsVisibleTo</c> (as <c>Dashboard.Tests.csproj</c> notes of <c>JsonRead</c>), so they are pinned through
/// <see cref="CostReport.Parse" /> there rather than here. <see cref="EdgeCaseTextTests" /> and
/// <see cref="CostTests" /> pin the words the page says of the three parts, which is where a part that is nothing
/// becomes <see cref="CostText.NoNumber" /> - and which of the three properties carries which sum is said only
/// through those words, so it is pinned here by the names themselves.
/// </para>
/// <para>
/// What follows is the record itself, which all of that walks past: the three parts read by the names of their own
/// properties rather than as one whole compared with another, at every end of the number they stand on and at the
/// nothing that is not a zero; a number that is no number at all being kept as that; a part turned as the record is
/// made and the copy that turns one part while it keeps the other two; the three parts coming back out in the order
/// they went in; and the public surface, so that the day a fourth part is added the list below says so. Then the
/// members every assertion above leans on without ever saying a word about them: the equality of the typed record, of
/// the boxed object and of <c>==</c> and <c>!=</c>, the hash that follows them, the null on either side of each, and
/// the one number written two ways - the zero and the zero below it - being one record with two sets of words. Then
/// the words the record says itself, which follow whoever reads them, unlike the page's.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: the record holds three numbers, no moment and no
/// address, and calls nothing at all - every value below is one the test brought. The members that would need a
/// <c>now</c> are <see cref="CostText.Parts" /> and <see cref="CostText.Line" />, and they are pinned on the words
/// they say, not on the record they read.
/// </para>
/// </summary>
public class CostAmountsEdgeCaseTests
{
    /// <summary>Three sums, each another number, so that a part landing on another part's name is seen rather than
    /// quietly matching a third of the same record - the <c>uat</c> entry of the shipped sample file.</summary>
    private static CostAmounts Full => new(1.52, 9.8, 11.02);

    // ------------------------------------------- the three parts, by the names of their own properties

    /// <summary>The constructor's three arguments, position by position, come back as the property of the same name:
    /// the last complete day first, the seven that end with it, and the month so far last - the order the file names
    /// them in, the order <c>Read</c> hands them over, and the order the page says them.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var uat = new CostAmounts(1.52, 9.8, 11.02);

        Assert.Equal(1.52, uat.Yesterday);
        Assert.Equal(9.8, uat.Last7Days);
        Assert.Equal(11.02, uat.MonthToDate);

        // The whole system of the same file: three other numbers, and no two of them alike either.
        var system = new CostAmounts(3.41, 22.1, 1204.8);

        Assert.Equal(3.41, system.Yesterday);
        Assert.Equal(22.1, system.Last7Days);
        Assert.Equal(1204.8, system.MonthToDate);

        // A day of nothing is a day Azure gave a number for.
        var idle = new CostAmounts(0, 1.5, 1.59);

        Assert.Equal(0d, idle.Yesterday);
        Assert.Equal(1.5, idle.Last7Days);
        Assert.Equal(1.59, idle.MonthToDate);
    }

    /// <summary>Every part is the number it was given, at the ends the number runs to: the zero, a refund below it, a
    /// half cent, the smallest number above zero, the largest there are and the least, the two infinities and the
    /// whole of a month's bill. The record adds nothing to a sum, rounds nothing, caps nothing and refuses nothing -
    /// it is the reading of the file, and the page's words, that decide what a number may stand for.</summary>
    [Theory]
    [InlineData(0d)]
    [InlineData(-0.3d)]
    [InlineData(0.005d)]
    [InlineData(1204.8d)]
    [InlineData(1e300)]
    [InlineData(double.Epsilon)]
    [InlineData(double.MaxValue)]
    [InlineData(double.MinValue)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void EachPartStandsAtTheEndsTheNumberRunsTo(double value)
    {
        var amounts = new CostAmounts(value, value, value);

        Assert.Equal(value, amounts.Yesterday);
        Assert.Equal(value, amounts.Last7Days);
        Assert.Equal(value, amounts.MonthToDate);
    }

    /// <summary>A part the file never gave stands unsaid, which is not the part standing at zero: the record whose
    /// three parts are nothing is a record and not no record, one part said beside two unsaid is the one said, and the
    /// caller that has no record at all has the reference that is none. It is what lets the page say a dash for the
    /// day Azure did not bill rather than a <c>$0.00</c> the file never wrote.</summary>
    [Fact]
    public void APartThatIsNotGivenStandsUnsaidAndNotAtZero()
    {
        var unsaid = new CostAmounts(null, null, null);

        Assert.NotNull(unsaid);
        Assert.Null(unsaid.Yesterday);
        Assert.Null(unsaid.Last7Days);
        Assert.Null(unsaid.MonthToDate);

        var half = new CostAmounts(1.52, null, 11.02);

        Assert.Equal(1.52, half.Yesterday);
        Assert.Null(half.Last7Days);
        Assert.Equal(11.02, half.MonthToDate);

        var weekOnly = new CostAmounts(null, 9.8, null);

        Assert.Null(weekOnly.Yesterday);
        Assert.Equal(9.8, weekOnly.Last7Days);
        Assert.Null(weekOnly.MonthToDate);

        // Zero is a number Azure gave; the nothing is the number that is not there.
        Assert.NotEqual(new CostAmounts(0, 0, 0), unsaid);
        Assert.NotEqual(half, new CostAmounts(1.52, 0, 11.02));
        Assert.Null(default(CostAmounts));
    }

    /// <summary>The one number a part may stand on that is no number: a sum said as not-a-number is kept as that, is
    /// one with the other sum said the same way and hashed with it, and is another record than the one at zero - the
    /// record compares its parts instead of ordering them, so nothing here is allowed to fall quietly into a total.</summary>
    [Fact]
    public void APartThatIsNoNumberIsStillTheValueItStandsOn()
    {
        var missing = Full with { Yesterday = double.NaN };
        var also = Full with { Yesterday = double.NaN };

        Assert.True(double.IsNaN(missing.Yesterday!.Value));
        Assert.Equal(9.8, missing.Last7Days);
        Assert.Equal(11.02, missing.MonthToDate);

        Assert.Equal(missing, also);
        Assert.True(missing == also);
        Assert.False(missing != also);
        Assert.Equal(missing.GetHashCode(), also.GetHashCode());

        Assert.NotEqual(missing, Full);
        Assert.NotEqual(missing, Full with { Yesterday = 0 });
        Assert.False(missing.Equals(Full with { Yesterday = 0 }));
    }

    // ------------------------------------------------------------- a part turned, and the copy

    /// <summary>Each part may be turned as the record is made - to another number, to the zero and to the nothing -
    /// and stands where it is turned to, while the two the turn does not name stay the numbers they were given: a
    /// turned record is another record and never the same reference.</summary>
    [Fact]
    public void APartTurnedAsTheRecordIsMadeStandsWhereItIsTurnedTo()
    {
        var day = new CostAmounts(1.52, 9.8, 11.02) { Yesterday = 3.41 };

        Assert.Equal(3.41, day.Yesterday);
        Assert.Equal(9.8, day.Last7Days);
        Assert.Equal(11.02, day.MonthToDate);
        Assert.NotSame(Full, day);

        var noWeek = new CostAmounts(1.52, 9.8, 11.02) { Last7Days = null };

        Assert.Equal(1.52, noWeek.Yesterday);
        Assert.Null(noWeek.Last7Days);
        Assert.Equal(11.02, noWeek.MonthToDate);
        Assert.Equal(new CostAmounts(1.52, null, 11.02), noWeek);

        var said = new CostAmounts(null, null, null) { Yesterday = 0, Last7Days = 0, MonthToDate = 0 };

        Assert.Equal(new CostAmounts(0, 0, 0), said);
        Assert.NotEqual(new CostAmounts(null, null, null), said);

        Assert.NotEqual(Full, day);
        Assert.NotEqual(Full, noWeek);
    }

    /// <summary>The copy that turns nothing is the same three sums in a new record, and the copy that turns one part
    /// keeps the other two: the original stands as it stood while the copy carries the turned part, the copy that
    /// names all three parts as they stand is the record it started from without being it, and a copy made of a
    /// record that says nothing keeps the two parts it does not name unsaid.</summary>
    [Fact]
    public void ACopyChangesThePartItNamesAndKeepsTheOtherTwo()
    {
        var original = Full;

        var unchanged = original with { };

        Assert.Equal(original, unchanged);
        Assert.NotSame(original, unchanged);

        var week = original with { Last7Days = 13.44 };

        Assert.Equal(9.8, original.Last7Days);
        Assert.Equal(13.44, week.Last7Days);
        Assert.Equal(1.52, week.Yesterday);
        Assert.Equal(11.02, week.MonthToDate);
        Assert.NotEqual(original, week);
        Assert.NotSame(original, week);

        var repacked = original with { Yesterday = 1.52, Last7Days = 9.8, MonthToDate = 11.02 };

        Assert.Equal(original, repacked);
        Assert.NotSame(original, repacked);

        var fromNothing = new CostAmounts(null, null, null) with { MonthToDate = 0 };

        Assert.Null(fromNothing.Yesterday);
        Assert.Null(fromNothing.Last7Days);
        Assert.Equal(0d, fromNothing.MonthToDate);
    }

    // ----------------------------------------------------- the record falling apart, and its own shape

    /// <summary>The three parts come back out in the order they went in - the day, the seven, the month - and the
    /// parts that are nothing come out as nothing rather than as the zero they are not.</summary>
    [Fact]
    public void TheThreePartsComeOutAgainInTheOrderTheyWentIn()
    {
        var (yesterday, week, month) = Full;

        Assert.Equal(1.52, yesterday);
        Assert.Equal(9.8, week);
        Assert.Equal(11.02, month);

        var (noDay, noWeek, noMonth) = new CostAmounts(null, null, null);

        Assert.Null(noDay);
        Assert.Null(noWeek);
        Assert.Null(noMonth);

        var (refund, weekOfRefund, monthOfRefund) = new CostAmounts(-0.3, null, null);

        Assert.Equal(-0.3, refund);
        Assert.Null(weekOfRefund);
        Assert.Null(monthOfRefund);

        // What came out goes back in the same way: the record the three parts fall apart into is the record they were.
        Assert.Equal(Full, new CostAmounts(yesterday, week, month));
        Assert.Equal(new CostAmounts(-0.3, null, null), new CostAmounts(refund, weekOfRefund, monthOfRefund));
    }

    /// <summary>The record's whole public surface, as it stands: the three parts named in the order the file names
    /// them, and the members every assertion in this file leans on - falling apart, comparing, hashing and saying
    /// itself - and nothing else to call. A fourth sum added to the record has to be written here as well, which is
    /// the point: the page cannot be shown a number nobody decided what it means.</summary>
    [Fact]
    public void TheRecordHasThreePartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Yesterday", "Last7Days", "MonthToDate"],
            [.. typeof(CostAmounts)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["Deconstruct", "Equals", "GetHashCode", "Last7Days", "MonthToDate", "ToString", "Yesterday", "op_Equality", "op_Inequality"],
            PublicSurface());

        Assert.All(
            typeof(CostAmounts).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            property => Assert.True(property.PropertyType == typeof(double?), $"{property.Name} is not a sum that may be missing"));
    }

    // --------------------------------------------------------------- the words the record says itself

    /// <summary>The words the record says its three parts in: its own name at the front, the parts in the order they
    /// are named, each as the plain number it stands on - the zero as <c>0</c>, a refund with its sign, the largest
    /// number there are in the digits that round-trip it, the endless as <c>Infinity</c> and the number that is none
    /// as <c>NaN</c> - and the part nobody said as the empty place between the mark and the comma. These are the
    /// record's words and not the page's: no dash, no currency and no thousand's separator.</summary>
    [Fact]
    public void TheRecordSaysItsThreePartsInTheOrderTheyAreNamed()
    {
        Assert.Equal("CostAmounts { Yesterday = 1.52, Last7Days = 9.8, MonthToDate = 11.02 }", Words(Full));
        Assert.Equal("CostAmounts { Yesterday = , Last7Days = , MonthToDate =  }", Words(new CostAmounts(null, null, null)));
        Assert.Equal("CostAmounts { Yesterday = 0, Last7Days = 0, MonthToDate = 0 }", Words(new CostAmounts(0, 0, 0)));
        Assert.Equal("CostAmounts { Yesterday = -0.3, Last7Days = 22.1, MonthToDate = 1204.8 }", Words(new CostAmounts(-0.3, 22.1, 1204.8)));
        Assert.Equal("CostAmounts { Yesterday = 5E-324, Last7Days = 1E+300, MonthToDate = 1.7976931348623157E+308 }", Words(new CostAmounts(double.Epsilon, 1e300, double.MaxValue)));
        Assert.Equal("CostAmounts { Yesterday = NaN, Last7Days = Infinity, MonthToDate = -Infinity }", Words(new CostAmounts(double.NaN, double.PositiveInfinity, double.NegativeInfinity)));
        Assert.Equal("CostAmounts { Yesterday = 1.52, Last7Days = , MonthToDate = 11.02 }", Words(new CostAmounts(1.52, null, 11.02)));

        // The words are no measure of the record: the sum that is unsaid and the sum of nothing are said apart.
        Assert.NotEqual(Words(new CostAmounts(0, 0, 0)), Words(new CostAmounts(null, null, null)));
        Assert.DoesNotContain("—", Words(new CostAmounts(null, null, null)), StringComparison.Ordinal);
        Assert.DoesNotContain("$", Words(Full), StringComparison.Ordinal);
    }

    /// <summary>The numbers of these words follow whoever reads them, which is what sets the record's own words apart
    /// from the page's: a reader of German is told <c>1,52</c> by the record and <c>$1.52</c> by
    /// <see cref="CostText.Money" />, whose words are written for the dashboard and not for whoever looks.</summary>
    [Fact]
    public void TheRecordsOwnWordsFollowTheReaderWhileThePagesWordsDoNot()
    {
        var amounts = Full;

        ViewerCulture.German(() =>
        {
            Assert.Equal("CostAmounts { Yesterday = 1,52, Last7Days = 9,8, MonthToDate = 11,02 }", amounts.ToString());
            Assert.Equal("$1.52", CostText.Money(amounts.Yesterday, "USD"));
        });

        Assert.Equal("CostAmounts { Yesterday = 1.52, Last7Days = 9.8, MonthToDate = 11.02 }", Words(amounts));
    }

    // ------------------------------------------------------------------- the equality, the hash, the two

    /// <summary>Two records are one while their three parts are, each as the number it stands on - the nothing among
    /// them included - whichever of the four ways of asking is used: the record's own <c>Equals</c>, the one of
    /// <see cref="object" />, <c>==</c> and <c>!=</c>. The hash follows the equality, boxed or unboxed and from one
    /// call to the next, so the records that are one are one entry of a set and the sum a file's reading hands over
    /// is interchangeable with the same three numbers written out by hand.</summary>
    [Fact]
    public void TwoAmountsAreOneWhileTheirThreePartsAre()
    {
        var one = Full;
        var two = new CostAmounts(1.52, 9.8, 11.02);

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

        Assert.Equal(new CostAmounts(null, null, null), new CostAmounts(null, null, null));
        Assert.Equal(new CostAmounts(null, null, null).GetHashCode(), new CostAmounts(null, null, null).GetHashCode());
        Assert.True(new CostAmounts(null, null, null) == new CostAmounts(null, null, null));

        var handRepacked = new CostAmounts(null, null, null) with { Yesterday = 1.52, Last7Days = 9.8, MonthToDate = 11.02 };

        Assert.Single(new HashSet<CostAmounts> { one, two, handRepacked });

        var read = CostReport.Parse("""{ "environments": [ { "name": "uat", "yesterday": 1.52, "last7Days": 9.8, "monthToDate": 11.02 } ] }""")!;

        Assert.Equal(one, read.Find("uat")!.Amounts);
        Assert.Equal(one.GetHashCode(), read.Find("uat")!.Amounts.GetHashCode());
    }

    /// <summary>A difference in either part is another record, and so is the same three numbers told in another
    /// order: the day turned by a cent, the week turned to the zero a missing week is not, a part said left out and a
    /// part unsaid filled in, and the first two sums exchanged - which is the record saying another thing, not the
    /// same thing again. A set of the four records that differ in one part each holds four of them.</summary>
    [Fact]
    public void APartTurnedOrTurnedAboutIsAnotherRecord()
    {
        var one = Full;

        Assert.NotEqual(one, new CostAmounts(1.53, 9.8, 11.02));
        Assert.NotEqual(one, new CostAmounts(1.52, 9.9, 11.02));
        Assert.NotEqual(one, new CostAmounts(1.52, 9.8, 11.03));
        Assert.NotEqual(one, new CostAmounts(null, 9.8, 11.02));
        Assert.NotEqual(one, new CostAmounts(1.52, null, 11.02));
        Assert.NotEqual(one, new CostAmounts(1.52, 9.8, null));
        Assert.NotEqual(one, new CostAmounts(1.52, 0, 11.02));
        Assert.NotEqual(one, new CostAmounts(0, 0, 0));
        Assert.NotEqual(one, new CostAmounts(null, null, null));
        Assert.NotEqual(one, new CostAmounts(9.8, 1.52, 11.02));
        Assert.NotEqual(one, new CostAmounts(1.52, 11.02, 9.8));

        Assert.True(one != new CostAmounts(1.52, 9.8, null));
        Assert.False(one == new CostAmounts(1.52, 9.8, null));
        Assert.False(one.Equals((object)new CostAmounts(null, 9.8, 11.02)));
        Assert.False(one.Equals(one with { MonthToDate = null }));

        // The three sums are three places: turning either of them alone is another record, and all four stand apart.
        Assert.Equal(
            4,
            new HashSet<CostAmounts> { one, one with { Yesterday = null }, one with { Last7Days = null }, one with { MonthToDate = null } }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two records that are no record
    /// at all are one, a record and no record are two - the equality and the inequality each saying it, from either
    /// side - and the record asked about itself says no to the nothing, to a word it is not, to a bare number, to the
    /// anonymous thing that happens to carry its three numbers and to another cost record of the same three.</summary>
    [Fact]
    public void NoRecordIsOneWithNoRecordAndNotWithARecord()
    {
        CostAmounts? nothing = null;
        CostAmounts? alsoNothing = null;
        CostAmounts? there = Full;

        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == nothing);
        Assert.True(there != nothing);
        Assert.False(there!.Equals(nothing));
        Assert.True(there.Equals(there));
        Assert.Null(default(CostAmounts));

        Assert.False(Full.Equals(null));
        Assert.False(Full.Equals((CostAmounts?)null));
        Assert.False(Full.Equals((object?)null));
        Assert.False(Full.Equals("CostAmounts { Yesterday = 1.52, Last7Days = 9.8, MonthToDate = 11.02 }"));
        Assert.False(Full.Equals((object)1.52));
        Assert.False(Full.Equals(new { Yesterday = 1.52, Last7Days = 9.8, MonthToDate = 11.02 }));
        Assert.False(Full.Equals(new CostService("uat", 1.52)));
    }

    /// <summary>One number written two ways is one record said two ways: the zero and the zero below it are the same
    /// three sums, equal with one hash, and the words that say them are not the same words - so the words are no
    /// measure of the record. A refund keeps its sign for all that, and stands apart from the number it is the
    /// opposite of and from the nothing a day Azure did not bill.</summary>
    [Fact]
    public void OneNumberWrittenTwoWaysIsOneRecordSaidTwoWays()
    {
        var zero = new CostAmounts(0, 0, 0);
        var signed = new CostAmounts(-0d, -0d, -0d);

        Assert.Equal(zero, signed);
        Assert.True(zero == signed);
        Assert.False(zero != signed);
        Assert.Equal(zero.GetHashCode(), signed.GetHashCode());
        Assert.Equal("CostAmounts { Yesterday = 0, Last7Days = 0, MonthToDate = 0 }", Words(zero));
        Assert.Equal("CostAmounts { Yesterday = -0, Last7Days = -0, MonthToDate = -0 }", Words(signed));
        Assert.Single(new HashSet<CostAmounts> { zero, signed });

        var refund = new CostAmounts(-0.3, null, null);

        Assert.NotEqual(refund, new CostAmounts(0.3, null, null));
        Assert.NotEqual(refund, zero);
        Assert.NotEqual(refund, new CostAmounts(null, null, null));
        Assert.Equal("CostAmounts { Yesterday = -0.3, Last7Days = , MonthToDate =  }", Words(refund));
    }

    // ------------------------------------------------------------------------- the machinery

    /// <summary>The public members the record declares itself: its properties and the methods that are not the get
    /// and the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(CostAmounts)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words a record says its three parts in, taken in the invariant culture whatever the reader of
    /// this test happens to be: the record's own words follow whoever reads them, which is pinned on its own further
    /// up this file, so the strings there are the point and the culture only the way to say them once.</summary>
    private static string Words(CostAmounts amounts)
    {
        ArgumentNullException.ThrowIfNull(amounts);

        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            return amounts.ToString()!;
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }
}
