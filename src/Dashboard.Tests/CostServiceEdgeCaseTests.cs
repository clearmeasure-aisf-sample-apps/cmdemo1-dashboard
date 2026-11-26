using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="CostService" /> is the one line of an environment's list of the services that cost most this month:
/// the name Azure Cost Management gives the service and the amount it cost in the month so far, which Azure may
/// simply not have given, in which case its part is nothing and the page says a dash. It is the element an entry's
/// <c>TopServices</c> holds - most expensive first, as the file's reading ordered it - and the line the page says
/// of what the environments share where the shared entry names a service of its own.
/// <para>
/// What the file makes of it is already pinned where the reading happens: <see cref="CostReportTests" /> reads a
/// hand-written <c>cost.json</c> and says that a line of <c>topServices</c> is the name it is given with the month
/// the number it is given, that a line the file gave no number to stands at nothing rather than at zero, and that
/// a line of a kind that is no object names no service; <see cref="CostReportEdgeCaseTests" /> pins the same
/// reading at its ends - the <c>topServices</c> of a kind that is no array and the line whose name is only
/// whitespace naming no service, the ones with their names standing in the order of the file; <see cref="CostTests"
/// /> pins the same record as the <c>uat</c> and <c>tdd</c> services of the file the optics read; and
/// <see cref="EdgeCaseTextTests" /> and <see cref="CostTests" /> pin the words the page says of the lines - the
/// most expensive first in its money and the one without its number as the dash. <see cref="CostEntryEdgeCaseTests"
/// /> pins that the entry holds the very list its lines stand in, in the order of the list, at the ends the number
/// runs to, which is where the entry is the entry and the line is walked past.
/// </para>
/// <para>
/// What follows is the record itself, which all of that walks past: the two parts read by the names of their own
/// properties rather than as one whole compared with another, the name kept exactly as it is given - empty, in its
/// whitespace and its case, which no part of the record turns - and the month at every end of the number it stands
/// on and at the nothing that is not a zero; a part turned as the record is made and the copy that turns one part
/// while it keeps the other; the two parts coming back out in the order they went in; and the public surface, so
/// that the day a third part is added the list below says so. Then the members every assertion above leans on
/// without ever saying a word about them: the equality of the typed record, of the boxed object and of <c>==</c>
/// and <c>!=</c>, the hash that follows them, the null on either side of each, the name compared in its case, the
/// month that is no number at all being one line with itself and nothing else, and the one number written two ways
/// - the zero and the zero below it - being one record with two sets of words. Then the words the record says
/// itself, which follow whoever reads them, unlike the page's.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: the record holds a name and one number, no
/// moment and no address, and calls nothing at all - every value below is one the test brought. The member that
/// would need a <c>now</c> is <see cref="CostText.Services" />, and it is pinned on the words it says, not on the
/// record it reads.
/// </para>
/// </summary>
public class CostServiceEdgeCaseTests
{
    /// <summary>The service the shipped sample file lists for <c>uat</c>: the container app that runs the
    /// dashboard and the month it cost - the name the file names it and the number it gives it, and beside them
    /// the names and numbers they are not, so that a part landing on the other is seen rather than quietly
    /// matching it.</summary>
    private static CostService Full => new("Azure Container Apps", 6.1);

    // ------------------------------------------- the two parts, by the names of their own properties

    /// <summary>The constructor's two arguments, position by position, come back as the property of the same
    /// name: the name the service is known by first, the month it cost last - the order the file names them in
    /// and the order the page says them. The sample's own line stands at the front, and beside it the second
    /// service of the same file's <c>tdd</c> entry, the whole of a month's bill and the line the number Azure
    /// did not give is still standing on.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var uat = new CostService("Azure Container Apps", 6.1);

        Assert.Equal("Azure Container Apps", uat.Name);
        Assert.Equal(6.1, uat.MonthToDate);

        // The second line of the sample file's <c>tdd</c> entry: another name, another number.
        var tdd = new CostService("SQL Database", 0.97);

        Assert.Equal("SQL Database", tdd.Name);
        Assert.Equal(0.97, tdd.MonthToDate);

        // A day of nothing is a day Azure gave a number for; the month Azure did not give is still the line.
        var idle = new CostService("Idle", 0);
        Assert.Equal("Idle", idle.Name);
        Assert.Equal(0d, idle.MonthToDate);

        var unsaid = new CostService("SQL Database", null);
        Assert.Equal("SQL Database", unsaid.Name);
        Assert.Null(unsaid.MonthToDate);

        // A new line of the same two parts is another line, not the one it was made of: the parts are compared
        // where they are compared, and the line itself is the place they stand in.
        var again = new CostService("Azure Container Apps", 6.1);
        Assert.Equal(uat, again);
        Assert.NotSame(uat, again);
    }

    /// <summary>The name is kept in its exact letters: the empty name and the name that is only whitespace stand
    /// where they stand, the whitespace around a name is not trimmed and the case is not turned. The trimming is
    /// the file's reading, pinned where the reading happens, and the finding of an entry is without a care for
    /// case, which is the finding and not the line - the line the record is built of says the name the builder
    /// gave it, nothing more and nothing less.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("  SQL Database  ")]
    [InlineData("SQL DATABASE")]
    [InlineData("sQl dAtAbAsE")]
    public void TheNameIsKeptExactlyAsItIsGiven(string name) =>
        Assert.Equal(name, new CostService(name, 6.1).Name);

    // ------------------------------------------------- the month, at the ends the number runs to

    /// <summary>The month the service cost stands where it stands, at the ends the number runs to: the zero, a
    /// refund below it, a half cent, the whole of a month's bill, the smallest number above zero, the largest
    /// there are and the least, and the two infinities. The record adds nothing to a sum, rounds nothing, caps
    /// nothing and refuses nothing - it is the reading of the file, and the page's words, that decide what a
    /// number may stand for.</summary>
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
    public void TheMonthStandsAtTheEndsTheNumberRunsTo(double value)
    {
        var service = new CostService("SQL Database", value);

        Assert.Equal(value, service.MonthToDate);
        Assert.Equal("SQL Database", service.Name);
    }

    /// <summary>The month Azure did not give stands unsaid, which is not the month standing at zero: the record
    /// whose month is nothing is a record and not no record, the name beside it stands, and the caller that has
    /// no record at all has the reference that is none. It is what lets the page say a dash for the month Azure
    /// did not bill rather than a <c>$0.00</c> the file never wrote.</summary>
    [Fact]
    public void AMonthThatIsNotGivenStandsUnsaidAndNotAtZero()
    {
        var unsaid = new CostService("SQL Database", null);

        Assert.NotNull(unsaid);
        Assert.Equal("SQL Database", unsaid.Name);
        Assert.Null(unsaid.MonthToDate);

        var idle = new CostService("SQL Database", 0);
        Assert.Equal(0d, idle.MonthToDate);

        // Zero is a number Azure gave; the nothing is the number that is not there.
        Assert.NotEqual(idle, unsaid);
        Assert.Null(default(CostService));
    }

    /// <summary>The one number a month may stand on that is no number: a sum said as not-a-number is kept as
    /// that, is one with the other sum said the same way and hashed with it, and is another record than the one
    /// at zero - the record compares its parts instead of ordering them, so nothing here is allowed to fall
    /// quietly into a total.</summary>
    [Fact]
    public void AMonthThatIsNoNumberIsStillTheValueItStandsOn()
    {
        var missing = Full with { MonthToDate = double.NaN };
        var also = Full with { MonthToDate = double.NaN };

        Assert.True(double.IsNaN(missing.MonthToDate!.Value));
        Assert.Equal("Azure Container Apps", missing.Name);

        Assert.Equal(missing, also);
        Assert.True(missing == also);
        Assert.False(missing != also);
        Assert.Equal(missing.GetHashCode(), also.GetHashCode());

        Assert.NotEqual(missing, Full);
        Assert.NotEqual(missing, Full with { MonthToDate = 0 });
        Assert.False(missing.Equals(Full with { MonthToDate = 0 }));
    }

    // ------------------------------------------------------------- a part turned, and the copy

    /// <summary>Each part may be turned as the record is made - the name to another name, the month to another
    /// number, to the zero and to the nothing - and stands where it is turned to, while the part the turn does
    /// not name stays the one it was given: a turned record is another record and never the same reference.</summary>
    [Fact]
    public void APartTurnedAsTheRecordIsMadeStandsWhereItIsTurnedTo()
    {
        var renamed = new CostService("Azure Container Apps", 6.1) { Name = "SQL Database" };

        Assert.Equal("SQL Database", renamed.Name);
        Assert.Equal(6.1, renamed.MonthToDate);
        Assert.NotSame(Full, renamed);

        var noMonth = new CostService("Azure Container Apps", 6.1) { MonthToDate = null };

        Assert.Equal("Azure Container Apps", noMonth.Name);
        Assert.Null(noMonth.MonthToDate);
        Assert.Equal(new CostService("Azure Container Apps", null), noMonth);

        var said = new CostService("Idle", null) { MonthToDate = 0 };

        Assert.Equal(new CostService("Idle", 0), said);
        Assert.NotEqual(new CostService("Idle", null), said);

        Assert.NotEqual(Full, renamed);
        Assert.NotEqual(Full, noMonth);
    }

    /// <summary>The copy that turns nothing is the same two parts in a new record, and the copy that turns one
    /// part keeps the other: the original stands as it stood while the copy carries the turned part, the copy
    /// that names both parts as they stand is the record it started from without being it, and a copy made of a
    /// record whose month is nothing keeps the month it does not name unsaid.</summary>
    [Fact]
    public void ACopyChangesThePartItNamesAndKeepsTheOther()
    {
        var original = Full;

        var unchanged = original with { };

        Assert.Equal(original, unchanged);
        Assert.NotSame(original, unchanged);

        var cheaper = original with { MonthToDate = 0.97 };

        Assert.Equal(6.1, original.MonthToDate);
        Assert.Equal(0.97, cheaper.MonthToDate);
        Assert.Equal("Azure Container Apps", cheaper.Name);
        Assert.NotEqual(original, cheaper);
        Assert.NotSame(original, cheaper);

        var repacked = original with { Name = "Azure Container Apps", MonthToDate = 6.1 };

        Assert.Equal(original, repacked);
        Assert.NotSame(original, repacked);

        var fromNothing = new CostService("SQL Database", null) with { Name = "SQL Database" };

        Assert.Equal("SQL Database", fromNothing.Name);
        Assert.Null(fromNothing.MonthToDate);
        Assert.Equal(new CostService("SQL Database", null), fromNothing);
    }

    // ---------------------------------------- the two parts coming back out, and the record's own shape

    /// <summary>The two parts come back out in the order they went in - the name, the month - and the month that
    /// is nothing comes out as nothing rather than as the zero it is not.</summary>
    [Fact]
    public void TheTwoPartsComeOutAgainInTheOrderTheyWentIn()
    {
        var (name, month) = Full;

        Assert.Equal("Azure Container Apps", name);
        Assert.Equal(6.1, month);

        var (noName, noMonth) = new CostService("SQL Database", null);

        Assert.Equal("SQL Database", noName);
        Assert.Null(noMonth);

        var (refund, monthOfRefund) = new CostService("Refund", -0.3);

        Assert.Equal("Refund", refund);
        Assert.Equal(-0.3, monthOfRefund);

        // What came out goes back in the same way: the record the two parts fall apart into is the record they
        // were.
        Assert.Equal(Full, new CostService(name, month));
        Assert.Equal(new CostService("Refund", -0.3), new CostService(refund, monthOfRefund));
    }

    /// <summary>The record's whole public surface, as it stands: the two parts named in the order the file names
    /// them, and the members every assertion in this file leans on - falling apart, comparing, hashing and
    /// saying itself - and nothing else to call. A third part added to the record has to be written here as
    /// well, which is the point: the page cannot be shown a part nobody decided what it means.</summary>
    [Fact]
    public void TheServiceHasTwoPartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Name", "MonthToDate"],
            [.. typeof(CostService)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["Deconstruct", "Equals", "GetHashCode", "MonthToDate", "Name", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());

        var properties = typeof(CostService)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(string), properties["Name"].PropertyType);
        Assert.Equal(typeof(double?), properties["MonthToDate"].PropertyType);
    }

    // --------------------------------------------------------------- the words the record says itself

    /// <summary>The words the record says its two parts in: its own name at the front, the parts in the order
    /// they are named, each as the plain words it stands on - the zero as <c>0</c>, a refund with its sign, the
    /// largest number there are in the digits that round-trip it, the endless as <c>Infinity</c> and the number
    /// that is none as <c>NaN</c> - and the part nobody said as the empty place between the mark and the closing
    /// brace. These are the record's words and not the page's: no dash, no currency and no thousand's
    /// separator.</summary>
    [Fact]
    public void TheServiceSaysItsTwoPartsInTheOrderTheyAreNamed()
    {
        Assert.Equal("CostService { Name = Azure Container Apps, MonthToDate = 6.1 }", Words(Full));
        Assert.Equal("CostService { Name = SQL Database, MonthToDate =  }", Words(new CostService("SQL Database", null)));
        Assert.Equal("CostService { Name = Idle, MonthToDate = 0 }", Words(new CostService("Idle", 0)));
        Assert.Equal("CostService { Name = Refund, MonthToDate = -0.3 }", Words(new CostService("Refund", -0.3)));
        Assert.Equal("CostService { Name = Whole, MonthToDate = 1204.8 }", Words(new CostService("Whole", 1204.8)));
        Assert.Equal("CostService { Name = Endless, MonthToDate = 1.7976931348623157E+308 }", Words(new CostService("Endless", double.MaxValue)));
        Assert.Equal("CostService { Name = Least, MonthToDate = -1.7976931348623157E+308 }", Words(new CostService("Least", double.MinValue)));
        Assert.Equal("CostService { Name = Hair, MonthToDate = 5E-324 }", Words(new CostService("Hair", double.Epsilon)));
        Assert.Equal("CostService { Name = NotANumber, MonthToDate = NaN }", Words(new CostService("NotANumber", double.NaN)));
        Assert.Equal("CostService { Name = Infinity, MonthToDate = Infinity }", Words(new CostService("Infinity", double.PositiveInfinity)));
        Assert.Equal("CostService { Name = -Infinity, MonthToDate = -Infinity }", Words(new CostService("-Infinity", double.NegativeInfinity)));
        Assert.Equal("CostService { Name =   , MonthToDate = 1.52 }", Words(new CostService("  ", 1.52)));

        // The words are no measure of the record: the month that is unsaid and the month of nothing are said
        // apart, and the name that is only air stands in its words.
        Assert.NotEqual(Words(new CostService("Idle", 0)), Words(new CostService("Idle", null)));
        Assert.DoesNotContain("—", Words(new CostService("SQL Database", null)), StringComparison.Ordinal);
        Assert.DoesNotContain("$", Words(Full), StringComparison.Ordinal);
    }

    /// <summary>The numbers of these words follow whoever reads them, which is what sets the record's own words
    /// apart from the page's: a reader of German is told <c>6,1</c> by the line and <c>$6.10</c> by
    /// <see cref="CostText.Money" />, whose words are written for the dashboard and not for whoever looks.</summary>
    [Fact]
    public void TheServicesOwnWordsFollowTheReaderWhileThePagesWordsDoNot()
    {
        ViewerCulture.German(() =>
        {
            Assert.Equal("CostService { Name = Azure Container Apps, MonthToDate = 6,1 }", Full.ToString());
            Assert.Equal("$6.10", CostText.Money(Full.MonthToDate, "USD"));
        });

        Assert.Equal("CostService { Name = Azure Container Apps, MonthToDate = 6.1 }", Words(Full));
    }

    // ------------------------------------------------------------------- the equality, the hash, the two

    /// <summary>Two lines are one while their two parts are - the name as the letters it is and the month as the
    /// number it stands on, the nothing among them included - whichever of the four ways of asking is used: the
    /// record's own <c>Equals</c>, the one of <see cref="object" />, <c>==</c> and <c>!=</c>. The hash follows
    /// the equality, boxed or unboxed and from one call to the next, so the lines that are one are one entry of a
    /// set and the line a file's reading hands over is interchangeable with the same two parts written out by
    /// hand.</summary>
    [Fact]
    public void TwoServicesAreOneWhileTheirTwoPartsAre()
    {
        var one = Full;
        var two = new CostService("Azure Container Apps", 6.1);

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

        Assert.Equal(new CostService("SQL Database", null), new CostService("SQL Database", null));
        Assert.Equal(new CostService("SQL Database", null).GetHashCode(), new CostService("SQL Database", null).GetHashCode());
        Assert.True(new CostService("SQL Database", null) == new CostService("SQL Database", null));

        var handRepacked = new CostService("SQL Database", null) with { Name = "Azure Container Apps", MonthToDate = 6.1 };

        Assert.Single(new HashSet<CostService> { one, two, handRepacked });

        // The line the file's reading hands over is the line the same two parts written by hand: the reading is
        // pinned where it happens, and here it is said to stand where it stands.
        var read = CostReport.Parse("""{ "environments": [ { "name": "uat", "topServices": [ { "name": "Azure Container Apps", "monthToDate": 6.1 } ] } ] }""")!;

        Assert.Equal(one, read.Find("uat")!.TopServices[0]);
        Assert.Equal(one.GetHashCode(), read.Find("uat")!.TopServices[0].GetHashCode());
    }

    /// <summary>A difference in either part is another record, and so is the same two parts told in the case
    /// they are not: the name in its case, which the finding of an entry is not, the month turned by a cent, a
    /// month said left out and a month unsaid filled in with the zero it is not, and the name and the month
    /// traded to kinds that are not their own. A set of the lines that differ in one part each holds them all
    /// apart.</summary>
    [Fact]
    public void APartTurnedOrTurnedAboutIsAnotherRecord()
    {
        var one = Full;

        Assert.NotEqual(one, new CostService("Azure Container App", 6.1));
        Assert.NotEqual(one, new CostService("AZURE CONTAINER APPS", 6.1));
        Assert.NotEqual(one, new CostService("Azure Container Apps", 6.11));
        Assert.NotEqual(one, new CostService("Azure Container Apps", null));
        Assert.NotEqual(one, new CostService("Azure Container Apps", 0));
        Assert.NotEqual(one, new CostService("", 6.1));

        Assert.True(one != new CostService("Azure Container Apps", null));
        Assert.False(one == new CostService("Azure Container Apps", null));
        Assert.False(one.Equals((object)new CostService("Azure Container Apps", 0)));
        Assert.False(one.Equals(one with { MonthToDate = null }));

        // The two parts are two places: turning either of them alone is another record, and all four stand
        // apart.
        Assert.Equal(
            4,
            new HashSet<CostService> { one, one with { Name = "SQL Database" }, one with { MonthToDate = null }, one with { MonthToDate = 0 } }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two lines that are no line
    /// at all are one, a line and no line are two - the equality and the inequality each saying it, from either
    /// side - and the line asked about itself says no to the nothing, to a word it is not, to a bare number, to
    /// the anonymous thing that happens to carry its two parts and to another cost record that says the same
    /// number of the other thing it is not.</summary>
    [Fact]
    public void NoServiceIsOneWithNoServiceAndNotWithAnyOtherThing()
    {
        CostService? nothing = null;
        CostService? alsoNothing = null;
        var there = Full;

        Assert.Null(default(CostService));
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == (CostService?)null);
        Assert.True(there != (CostService?)null);
        Assert.False(there.Equals(nothing));
        Assert.False(there.Equals((object?)null));
        Assert.True(there.Equals(there));

        Assert.False(there.Equals("CostService { Name = Azure Container Apps, MonthToDate = 6.1 }"));
        Assert.False(there.Equals("Azure Container Apps"));
        Assert.False(there.Equals((object)6.1));
        Assert.False(there.Equals(new CostAmounts(6.1, null, null)));
        Assert.False(there.Equals(new CostAmounts(null, null, 6.1)));
        Assert.False(there.Equals(new CostEstimate(0.2048, new CostAmounts(null, null, 6.1))));
        Assert.False(there.Equals(new CostEntry("uat", new CostAmounts(null, null, 6.1), [])));
        Assert.False(there.Equals(new { Name = "Azure Container Apps", MonthToDate = 6.1 }));
    }

    /// <summary>One number written two ways is one record said two ways: the zero and the zero below it are the
    /// same two parts, equal with one hash, and the words that say them are not the same words - so the words
    /// are no measure of the record. A refund keeps its sign for all that, and stands apart from the number it
    /// is the opposite of and from the nothing a month Azure did not bill.</summary>
    [Fact]
    public void OneNumberWrittenTwoWaysIsOneRecordSaidTwoWays()
    {
        var zero = new CostService("Idle", 0.0);
        var signed = new CostService("Idle", -0.0);

        Assert.Equal(zero, signed);
        Assert.True(zero == signed);
        Assert.False(zero != signed);
        Assert.Equal(zero.GetHashCode(), signed.GetHashCode());
        Assert.Equal("CostService { Name = Idle, MonthToDate = 0 }", Words(zero));
        Assert.Equal("CostService { Name = Idle, MonthToDate = -0 }", Words(signed));
        Assert.Single(new HashSet<CostService> { zero, signed });

        var refund = new CostService("Refund", -0.3);

        Assert.NotEqual(refund, new CostService("Refund", 0.3));
        Assert.NotEqual(refund, zero);
        Assert.NotEqual(refund, new CostService("Refund", null));
        Assert.Equal("CostService { Name = Refund, MonthToDate = -0.3 }", Words(refund));
    }

    // ------------------------------------------------------------------------- the machinery

    /// <summary>The public members the record declares itself: its properties and the methods that are not the
    /// get and the set of one - the two operators kept although the compiler names them specially, since they
    /// are the <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past.
    /// The constructor is no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(CostService)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words a record says its two parts in, taken in the invariant culture whatever the reader of
    /// this test happens to be: the record's own words follow whoever reads them, which is pinned on its own
    /// further up this file, so the strings there are the point and the culture only the way to say them
    /// once.</summary>
    private static string Words(CostService service) => ViewerCulture.Invariant(() => service.ToString()!);
}
