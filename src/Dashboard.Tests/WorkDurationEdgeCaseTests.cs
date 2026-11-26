using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="WorkDuration" />, the one duration of hours declared
/// in <c>src/Dashboard/Work/WorkItemsReport.cs</c>: a median, an 85th percentile and how many items were counted, the
/// three numbers a cycle time, a lead time, a merge-to-prod and every column's wait stand on.
/// <para>
/// What the other files already pin is left alone. The taking of the three numbers out of <c>workitems.json</c> is
/// <c>WorkDuration.Read</c>, which is internal to an app that declares no <c>InternalsVisibleTo</c> (as
/// <c>Dashboard.Tests.csproj</c> notes of <c>JsonRead</c>) and is therefore pinned through
/// <see cref="WorkItemsReport.Parse" />: <see cref="WorkItemsTests.AnOlderFileWithoutASplitStaysOneBoard" /> says that
/// a <c>cycleTimeHours</c> of a median and a count and nothing else arrives with its median of five and its p85
/// unsaid, and <see cref="WorkItemsTests.TheSampleFileIsTheBoardTheViewShows" /> that the shipped sample's durations
/// come through as the file writes them. The hours a board's own items are summarised into are
/// <c>WorkBotText.Durations</c>' and <c>WorkBotText.Summarize</c>' - those stand on a clock, are no member of this
/// record, and are pinned with the moments the tests bring in <see cref="WorkBotTextTests" />. The words the page
/// makes of a duration - <c>median</c>, <c>p85</c>, the hours turned to days, the <c>·</c>, the <c>n/a</c> of nothing
/// at all - are <see cref="WorkItemsText.Duration(WorkDuration?)" />'s and are pinned in
/// <see cref="ColumnWaitTests" /> and <see cref="WorkItemsTests" />. And the duration stood on inside a column's wait,
/// both as a part of that wait's equality and as a part of that wait's words, is
/// <see cref="ColumnWaitEdgeCaseTests" />'s: this record's own members are nowhere among them.
/// </para>
/// <para>
/// What is left, and is pinned here, is the record itself: the constructor handing each of its three arguments to the
/// property of its name; the two numbers held as the <see cref="double" /> they are at the ends a number runs to - the
/// least subnormal, the greatest, the negative zero with its sign kept, and the number that is no number and the two
/// beyond the ends, all of them kept rather than refused - and the count of items held as the <see cref="int" /> it
/// is, below nought and at either end; a part nobody gave standing unsaid, which is no zero and no empty count; the
/// three parts coming back out in the order they went in; the two numbers being in their own places, so that a median
/// of ten on a p85 of six is another duration than the other way round; the two durations being one while their three
/// parts are, with the hash, the set and the operators following that, the equal-by-value standing of the number that
/// is no number, and the nothing on either side of each; the copy that turns one number while it keeps the other two;
/// the words the duration says its three parts in and those words following whoever reads a number; and the public
/// surface, so that the day a fourth number is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a duration is three numbers and calls
/// nothing at all - no hour is measured here, only held - so every value below is one the test brought and every call
/// says the same thing twice.
/// </para>
/// </summary>
public class WorkDurationEdgeCaseTests
{
    /// <summary>A duration with all three numbers said: the hours the sample's cycle time is read as.</summary>
    private static WorkDuration Full => new(6, 10.5, 3);

    /// <summary>A duration whose every part is the nothing a file that says nothing lands on.</summary>
    private static WorkDuration Unsaid => new(null, null, null);

    /// <summary>A duration whose every part is said as nought: an empty count of items and no hours at all.</summary>
    private static WorkDuration Zeroed => new(0, 0, 0);

    // --------------------------------- the three numbers, by the names of their own properties

    /// <summary>The constructor's three arguments, position by position, come back as the property of the same name:
    /// the median first, the 85th percentile beside it, the count of items last - the order the file's
    /// <c>median</c>, <c>p85</c> and <c>n</c> are named in and the order the tile says them in. A new duration of the
    /// same three numbers is another duration, not the one it was built of, and the numbers are held in their own
    /// types: a whole number given to a <see cref="double" /> stands as that number and nothing else.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var hours = new WorkDuration(18.5, 26, 11);

        Assert.Equal(18.5, hours.Median);
        Assert.Equal(26, hours.P85);
        Assert.Equal(11, hours.N);

        // The p85 the file leaves out is left out, and the whole hour is a whole hour, not a rounded one.
        var plain = new WorkDuration(5, null, 1);

        Assert.Equal(5, plain.Median);
        Assert.Null(plain.P85);
        Assert.Equal(1, plain.N);

        var again = new WorkDuration(18.5, 26, 11);

        Assert.Equal(hours, again);
        Assert.NotSame(hours, again);
    }

    /// <summary>The two hours are a <see cref="double" /> and stand on it at the ends it runs to, and the count of
    /// items is an <see cref="int" /> and stands at the ends it runs to: nought, a thousandth of an hour, the least
    /// number there is, the greatest, a negative wait - which the record keeps as it is given and never refuses,
    /// never clamps and never rounds. Whoever reads the file decides what a number can be; this record only holds
    /// it.</summary>
    [Theory]
    [InlineData(0.0, 0.0, 0)]
    [InlineData(1.0, 1.0, 1)]
    [InlineData(6.0, 10.0, 3)]
    [InlineData(18.5, 26.0, 11)]
    [InlineData(0.001, 0.002, 2)]
    [InlineData(-6.5, -10.0, -3)]
    [InlineData(5e-324, 1e-323, 0)]
    [InlineData(-1.7976931348623157e+308, 1.7976931348623157e+308, -2147483648)]
    [InlineData(1.7976931348623157e+308, 5e-324, 2147483647)]
    [InlineData(null, 6.0, null)]
    [InlineData(6.0, null, null)]
    [InlineData(null, null, null)]
    public void TheNumbersStandAtTheEndsTheyRunTo(double? median, double? p85, int? n)
    {
        var hours = new WorkDuration(median, p85, n);

        Assert.Equal(median, hours.Median);
        Assert.Equal(p85, hours.P85);
        Assert.Equal(n, hours.N);
        Assert.Equal(hours, new WorkDuration(median, p85, n));
        Assert.Equal(hours, hours with { });
        Assert.Equal(hours.GetHashCode(), new WorkDuration(median, p85, n).GetHashCode());
    }

    /// <summary>The numbers a finite hour is not: the number that is no number and the two beyond the ends of the
    /// exponent. The record holds each as the very <see cref="double" /> it was given - the equality of the record
    /// asks a <see cref="double" /> of its <c>Equals</c>, in which the number that is no number is one with itself,
    /// so two durations built of <see cref="double.NaN" /> are one duration and one entry of a set, whatever the
    /// <c>==</c> of the raw numbers says.</summary>
    [Fact]
    public void ANumberThatIsNoNumberAndTheTwoBeyondTheEndsAreHeld()
    {
        var none = new WorkDuration(double.NaN, double.NaN, null);
        var alsoNone = new WorkDuration(double.NaN, double.NaN, null);
        var beyond = new WorkDuration(double.PositiveInfinity, double.NegativeInfinity, 1);

        Assert.True(double.IsNaN(none.Median!.Value));
        Assert.True(double.IsNaN(none.P85!.Value));
        Assert.Null(none.N);
        Assert.True(double.IsPositiveInfinity(beyond.Median!.Value));
        Assert.True(double.IsNegativeInfinity(beyond.P85!.Value));
        Assert.Equal(1, beyond.N);

        Assert.Equal(none, alsoNone);
        Assert.True(none == alsoNone);
        Assert.False(none != alsoNone);
        Assert.Equal(none.GetHashCode(), alsoNone.GetHashCode());
        Assert.Single(new HashSet<WorkDuration> { none, alsoNone });

        Assert.Equal(beyond, new WorkDuration(double.PositiveInfinity, double.NegativeInfinity, 1));
        Assert.NotEqual(none, beyond);
        Assert.NotEqual(beyond, Full);

        // The number that is no number in one part does not carry over to the parts that are numbers.
        Assert.NotEqual(none, new WorkDuration(double.NaN, 10.5, null));
        Assert.NotEqual(none, Unsaid);
    }

    /// <summary>The sign of a nought is kept: a median written as <c>-0.0</c> and one written as <c>0.0</c> are one
    /// duration to the record, which asks a <see cref="double" />'s own equality, and they are two durations in its
    /// words, which say the number as it stands. Neither is worth more than the other to a board - a wait of negative
    /// nought hours is a wait of no hours - but nothing in the record settles that, and the page reads both through
    /// <see cref="WorkItemsText.Duration(WorkDuration?)" />.</summary>
    [Fact]
    public void TheSignOfANoughtIsKeptWhileTheNoughtItselfIsOne()
    {
        var below = new WorkDuration(-0.0, -0.0, 0);
        var above = new WorkDuration(0.0, 0.0, 0);

        Assert.Equal(0.0, below.Median);
        Assert.Equal(below, above);
        Assert.True(below == above);
        Assert.Equal(below.GetHashCode(), above.GetHashCode());
        Assert.Single(new HashSet<WorkDuration> { below, above });

        Assert.Contains("Median = -0,", Words(below), StringComparison.Ordinal);
        Assert.DoesNotContain("-0", Words(above), StringComparison.Ordinal);
    }

    /// <summary>A part nobody gave stands unsaid, which is no zero, no nought count and no empty string: the record
    /// whose three parts are unsaid is a record, and it is another record than the one whose three parts are nought -
    /// told apart by its equality, its operators and its words. That is what lets the page leave a duration out
    /// altogether rather than tell a reader that a cycle time is nought hours over nought items. The two are hashed
    /// alike, because a nullable number with nothing in it hashes as the number nought and every number here that is
    /// nought hashes to the same place: a set still holds them apart, on their equality.</summary>
    [Fact]
    public void APartNobodyGaveStandsUnsaidAndNotAtNought()
    {
        Assert.Null(Unsaid.Median);
        Assert.Null(Unsaid.P85);
        Assert.Null(Unsaid.N);

        Assert.NotEqual(Unsaid, Zeroed);
        Assert.False(Unsaid == Zeroed);
        Assert.True(Unsaid != Zeroed);
        Assert.False(Unsaid.Equals((object)Zeroed));
        Assert.NotEqual(Words(Unsaid), Words(Zeroed));
        Assert.Equal(2, new HashSet<WorkDuration> { Unsaid, Zeroed }.Count);

        // The same hash, and two entries all the same: the set separates them on their equality, not on their hash.
        Assert.Equal(Unsaid.GetHashCode(), Zeroed.GetHashCode());

        // One part unsaid is enough, and the unsaid thing is not the empty count either.
        Assert.NotEqual(Unsaid, new WorkDuration(null, null, 0));
        Assert.NotEqual(Unsaid, new WorkDuration(0.0, null, null));
        Assert.NotEqual(Unsaid, new WorkDuration(null, 0.0, null));
        Assert.Equal(
            4,
            new HashSet<WorkDuration>
            {
                Unsaid,
                Unsaid with { Median = 0.0 },
                Unsaid with { P85 = 0.0 },
                Unsaid with { N = 0 },
            }.Count);

        var turnedOff = Full with { Median = null, P85 = null, N = null };

        Assert.Equal(Unsaid, turnedOff);
        Assert.Null(turnedOff.Median);
    }

    // ------------------------------------------------- a part turned, and the copy that turns it

    /// <summary>Each number may be turned as the duration is made or copied - the median to another median, the p85 to
    /// the nothing a file that gives no percentile leaves, the count to nought - and stands where it is turned to,
    /// while the numbers the turn does not name stay what they were. A copy is another duration and never the same
    /// reference; a copy that names no part at all is the duration it was copied from, in its parts, in its hash and
    /// in its words.</summary>
    [Fact]
    public void ACopyTurnsThePartItNamesAndKeepsTheOtherTwo()
    {
        var hours = Full;

        var medianTurned = hours with { Median = 20 };

        Assert.Equal(20, medianTurned.Median);
        Assert.Equal(10.5, medianTurned.P85);
        Assert.Equal(3, medianTurned.N);
        Assert.NotSame(hours, medianTurned);
        Assert.NotEqual(hours, medianTurned);

        var p85TurnedOff = hours with { P85 = null };

        Assert.Null(p85TurnedOff.P85);
        Assert.Equal(6, p85TurnedOff.Median);
        Assert.Equal(3, p85TurnedOff.N);
        Assert.Equal(new WorkDuration(6, null, 3), p85TurnedOff);

        var countTurnedToNought = hours with { N = 0 };

        Assert.Equal(0, countTurnedToNought.N);
        Assert.NotEqual(hours, countTurnedToNought);

        var unchanged = hours with { };

        Assert.Equal(hours, unchanged);
        Assert.NotSame(hours, unchanged);
        Assert.Equal(hours.GetHashCode(), unchanged.GetHashCode());
        Assert.Equal(Words(hours), Words(unchanged));

        // The written-as-made form turns the same part the copy turns, and the original stands as it stood.
        var asMade = new WorkDuration(20, 10.5, 3) { N = 5 };

        Assert.Equal(20, asMade.Median);
        Assert.Equal(5, asMade.N);
        Assert.Equal(6, hours.Median);
        Assert.Equal(3, hours.N);

        var repacked = hours with { Median = hours.Median, P85 = hours.P85, N = hours.N };

        Assert.Equal(hours, repacked);
        Assert.NotSame(hours, repacked);
    }

    // --------------------------------------------- the duration falling apart, and its own declared shape

    /// <summary>The three numbers come back out in the order they went in - the median, the p85, the count - and the
    /// numbers nobody gave come out as the nothing they are, not as nought. What came out goes back in the same way:
    /// the duration the three parts fall apart into is the duration they were.</summary>
    [Fact]
    public void TheThreePartsComeOutAgainInTheOrderTheyWentIn()
    {
        var (median, p85, n) = Full;

        Assert.Equal(6, median);
        Assert.Equal(10.5, p85);
        Assert.Equal(3, n);
        Assert.Equal(Full, new WorkDuration(median, p85, n));

        var (noMedian, noP85, noN) = Unsaid;

        Assert.Null(noMedian);
        Assert.Null(noP85);
        Assert.Null(noN);
        Assert.Equal(Unsaid, new WorkDuration(noMedian, noP85, noN));

        var (least, greatest, count) = new WorkDuration(double.MinValue, double.MaxValue, int.MinValue);

        Assert.Equal(double.MinValue, least);
        Assert.Equal(double.MaxValue, greatest);
        Assert.Equal(int.MinValue, count);
    }

    /// <summary>The duration's whole public surface, as the running assembly holds it: its three numbers named in the
    /// order the constructor names them, and the members every record is asked - falling apart, comparing, hashing and
    /// saying itself - and nothing else to call. The way in is one: the three parts, none of them left out, for a
    /// duration with no parts at all is the nothing the reading of a file calls for and is had from
    /// <c>WorkDuration.Read</c>, not from a shorter way in. A fourth number added to the record has to be written here
    /// as well, which is the point: the tile cannot be shown a part nobody decided what it means. And nothing on it is
    /// a moment, an address or a length of time the record would have to go and look up: three numbers, and no clock
    /// among them - which is why no test in this file needs one.</summary>
    [Fact]
    public void TheRecordHasThreePartsAndTheMembersEveryRecordIsAsked()
    {
        var type = typeof(WorkDuration);

        Assert.True(type.IsSealed);
        Assert.False(type.IsValueType);
        Assert.Equal(typeof(object), type.BaseType);
        Assert.Equal([typeof(IEquatable<WorkDuration>)], type.GetInterfaces());

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.Equal(["Median", "P85", "N"], properties.Select(property => property.Name));
        Assert.Equal(typeof(double?), properties[0].PropertyType);
        Assert.Equal(typeof(double?), properties[1].PropertyType);
        Assert.Equal(typeof(int?), properties[2].PropertyType);
        Assert.All(properties, property =>
        {
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.NotNull(property.SetMethod);
            Assert.True(property.GetMethod!.IsPublic);

            // The parts are init-only: they are said once, at the making or at the copy, and never turned after.
            Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());

            // And each of the three is a number that may be unsaid, never a moment and never a reference to follow.
            Assert.NotNull(Nullable.GetUnderlyingType(property.PropertyType));
            Assert.DoesNotContain(
                property.PropertyType,
                new[] { typeof(DateTimeOffset), typeof(DateTime), typeof(TimeSpan), typeof(Uri), typeof(string) });
        });

        // The contract every record compares by is the compiler's private one: no public part of the duration.
        Assert.Null(type.GetProperty("EqualityContract", BindingFlags.Public | BindingFlags.Instance));
        Assert.NotNull(type.GetProperty("EqualityContract", BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.DoesNotContain("EqualityContract", PublicSurface(), StringComparer.Ordinal);

        Assert.Equal(
            ["Deconstruct", "Equals", "GetHashCode", "Median", "N", "P85", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());

        var arguments = type.GetConstructors().Single().GetParameters();

        Assert.Single(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.Equal(3, arguments.Length);
        Assert.Equal(["Median", "P85", "N"], arguments.Select(argument => argument.Name));
        Assert.Equal([typeof(double?), typeof(double?), typeof(int?)], arguments.Select(argument => argument.ParameterType));
        Assert.All(arguments, argument => Assert.False(argument.HasDefaultValue));

        // The copy the with walks is the compiler's private one, and the reading of the file is no public member.
        var copy = Assert.Single(type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.Equal(type, copy.GetParameters().Single().ParameterType);
        Assert.Null(type.GetMethod("Read", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));

        // The two overloads of the equality every record is asked with, and the one Deconstruct.
        Assert.Equal(2, type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Count(method => method.Name == "Equals"));
        var parts = type.GetMethod("Deconstruct")!;
        Assert.Equal(["Median", "P85", "N"], parts.GetParameters().Select(parameter => parameter.Name));
        Assert.All(parts.GetParameters(), parameter => Assert.True(parameter.IsOut));

        // And no member of it is given a moment, an address or a length of time to go and ask about.
        var other = type
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .SelectMany(method => method.GetParameters())
            .Select(parameter => parameter.ParameterType)
            .Where(parameter => parameter != type)
            .ToList();
        Assert.DoesNotContain(typeof(DateTimeOffset), other);
        Assert.DoesNotContain(typeof(DateTime), other);
        Assert.DoesNotContain(typeof(TimeSpan), other);
        Assert.DoesNotContain(typeof(Uri), other);
    }

    // -------------------------------------------------------------- the equality, the hash, and the two

    /// <summary>Two durations are one while their three numbers are - each number as the number it stands on, the
    /// nothing among them included - whichever of the ways of asking is used: the record's own <c>Equals</c>, the one
    /// of <see cref="object" />, <c>==</c> and <c>!=</c>, from either side. The hash follows the equality, boxed or
    /// unboxed and from one call to the next, so the durations that are one are one duration of a set: which is how a
    /// duration the test writes is found again among the ones the file was read into, and how the longest wait is
    /// picked out of a list of them.</summary>
    [Fact]
    public void TwoDurationsAreOneWhileTheirThreePartsAre()
    {
        var one = Full;
        var two = new WorkDuration(6, 10.5, 3);

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
        Assert.Single(new HashSet<WorkDuration> { one, two });
        Assert.Contains(two, new HashSet<WorkDuration> { one });

        var byHours = new Dictionary<WorkDuration, string> { [one] = "cycle" };
        Assert.Equal("cycle", byHours[two]);

        // A whole number given to a double and the same number written with its point are one number, and one duration.
        Assert.Equal(one, new WorkDuration(6.0, 10.5, 3));
        Assert.Equal(Unsaid, new WorkDuration(null, null, null));
        Assert.True(Unsaid == Unsaid);
        Assert.False(Unsaid != Unsaid);
        Assert.Equal(Unsaid.GetHashCode(), ((object)Unsaid).GetHashCode());
    }

    /// <summary>A difference in one part is another duration: the median turned by the least subnormal number there
    /// is, the two numbers swapped into each other's places, the p85 the file did not give turned to the one it did,
    /// the count at nought where it was unsaid and at one where it was three, and the whole duration stood in another
    /// order of its own making. A set of the durations that differ in one part each holds them all apart.</summary>
    [Fact]
    public void ADurationThatDiffersInOnePartIsAnotherDuration()
    {
        var one = Full;

        Assert.NotEqual(one, one with { Median = 6.0000001 });
        Assert.NotEqual(one, one with { Median = null });
        Assert.NotEqual(one, one with { P85 = null });
        Assert.NotEqual(one, one with { N = null });
        Assert.NotEqual(one, new WorkDuration(10.5, 6, 3));
        Assert.NotEqual(one, new WorkDuration(6, 10.5, 4));
        Assert.NotEqual(one, new WorkDuration(6, 10.5, 0));
        Assert.NotEqual(Zeroed, new WorkDuration(0, 0, -1));
        Assert.NotEqual(one, Zeroed);

        Assert.True(one != (one with { N = 4 }));
        Assert.False(one == (one with { N = 4 }));
        Assert.False(one.Equals((object)(one with { P85 = null })));
        Assert.False(one.Equals(one with { Median = null }));

        // The two ends of the exponent, and the two ends of a count, are four durations apart.
        Assert.Equal(
            4,
            new HashSet<WorkDuration>
            {
                one,
                new WorkDuration(double.MinValue, 10.5, 3),
                new WorkDuration(double.MaxValue, 10.5, 3),
                new WorkDuration(6, 10.5, int.MinValue),
            }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two durations that are no
    /// duration at all are one, a duration and no duration are two - the equality and the inequality each saying it,
    /// from either side - and the duration asked about itself says no to the nothing, to its own words as a name, to a
    /// bare number, to the anonymous thing that happens to carry its three parts, to the wait it stands inside and to
    /// the week the chart is made of.</summary>
    [Fact]
    public void NoDurationIsOneWithNoDurationAndNotWithAnyOtherThing()
    {
        WorkDuration? nothing = null;
        WorkDuration? alsoNothing = null;
        var there = Full;

        Assert.Null(default(WorkDuration));
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == (WorkDuration?)null);
        Assert.True(there != (WorkDuration?)null);
        Assert.True((WorkDuration?)null == nothing);
        Assert.False(there.Equals(nothing));
        Assert.False(there.Equals((object?)null));
        Assert.True(there.Equals(there));

        // A duration whose every part is unsaid is still a duration: what turns it into no duration at all is the
        // reading of the file, pinned through WorkItemsReport.Parse, and not the record.
        Assert.NotNull(Unsaid);
        Assert.False(there.Equals(Unsaid));

        Assert.False(there.Equals("WorkDuration { Median = 6, P85 = 10.5, N = 3 }"));
        Assert.False(there.Equals((object)6));
        Assert.False(there.Equals((object)6.0));
        Assert.False(there.Equals((object)3));
        Assert.False(there.Equals(new { Median = 6d, P85 = 10.5d, N = 3 }));
        Assert.False(there.Equals(new ColumnWait(WorkColumns.Uat, Full, true)));
        Assert.False(there.Equals(new WorkWeek(null, null, null)));
        Assert.Equal(2, new HashSet<WorkDuration?> { there, nothing, alsoNothing }.Count);
    }

    /// <summary>The hash says the three numbers and nothing else: the same numbers hashed again are the same hash, a
    /// part turned is another hash, and the ends of a number are hashed with the number and not instead of it. The
    /// three durations of nothing, of nought and of every part said stand apart in a set of their own even though two
    /// of the three share a hash - which is what the set, and not the hash, is there to settle.</summary>
    [Fact]
    public void TheHashFollowsThePartsAndStandsStillFromOneCallToTheNext()
    {
        var full = Full;
        var least = new WorkDuration(double.MinValue, double.MinValue, int.MinValue);
        var greatest = new WorkDuration(double.MaxValue, double.MaxValue, int.MaxValue);

        Assert.Equal(full.GetHashCode(), new WorkDuration(6, 10.5, 3).GetHashCode());
        Assert.Equal(Unsaid.GetHashCode(), Unsaid.GetHashCode());
        Assert.Equal(least.GetHashCode(), new WorkDuration(double.MinValue, double.MinValue, int.MinValue).GetHashCode());
        Assert.NotEqual(full.GetHashCode(), least.GetHashCode());
        Assert.NotEqual(least.GetHashCode(), greatest.GetHashCode());
        Assert.NotEqual(full.GetHashCode(), (full with { Median = 6.5 }).GetHashCode());
        Assert.NotEqual(full.GetHashCode(), (full with { P85 = 11 }).GetHashCode());
        Assert.NotEqual(full.GetHashCode(), (full with { N = 4 }).GetHashCode());
        Assert.Equal(3, new HashSet<WorkDuration> { full, Unsaid, Zeroed }.Count);
        Assert.Equal(2, new HashSet<WorkDuration> { least, greatest }.Count);

        // A duration of nought hours and a duration of negative nought hours are one duration, and hash as one.
        Assert.Equal(new WorkDuration(0.0, 0.0, 0).GetHashCode(), new WorkDuration(-0.0, -0.0, 0).GetHashCode());
    }

    // ------------------------------------------------------------ the words the record says itself

    /// <summary>The words the duration says its three parts in: its own name at the front, the parts in the order the
    /// constructor names them, each number as the plain number it stands on and each part nobody said as the empty
    /// place between the mark and the comma or the closing brace. These are the record's words and not the page's: no
    /// <c>median</c> in its lower case, no <c>h</c>, no <c> d</c>, no <c>·</c>, no <c> items</c> and no
    /// <c>n/a</c> - all of those are <see cref="WorkItemsText.Duration(WorkDuration?)" />'s and are pinned there.</summary>
    [Fact]
    public void TheRecordSaysItsThreePartsInTheOrderTheyAreNamed()
    {
        Assert.Equal("WorkDuration { Median = 6, P85 = 10.5, N = 3 }", Words(Full));
        Assert.Equal("WorkDuration { Median = , P85 = , N =  }", Words(Unsaid));
        Assert.Equal("WorkDuration { Median = 0, P85 = 0, N = 0 }", Words(Zeroed));
        Assert.Equal("WorkDuration { Median = 18.5, P85 = 26, N = 11 }", Words(new WorkDuration(18.5, 26, 11)));
        Assert.Equal("WorkDuration { Median = 5, P85 = , N = 1 }", Words(new WorkDuration(5, null, 1)));
        Assert.Equal("WorkDuration { Median = -6.5, P85 = -10, N = -3 }", Words(new WorkDuration(-6.5, -10, -3)));
        Assert.Equal("WorkDuration { Median = 1, P85 = 1, N = -2147483648 }", Words(new WorkDuration(1, 1, int.MinValue)));
        Assert.Equal("WorkDuration { Median = 1, P85 = 1, N = 2147483647 }", Words(new WorkDuration(1, 1, int.MaxValue)));

        // The words are no measure of the duration: an unsaid part and a part of nought are said apart, and the
        // page's word for a duration that is nothing at all is a word this record has never heard of.
        Assert.NotEqual(Words(Unsaid), Words(Zeroed));
        foreach (var word in new[] { "median ", "p85 ", " items", " item", " · ", " h", " d", "n/a", "—", "<" })
        {
            Assert.DoesNotContain(word, Words(Full), StringComparison.Ordinal);
        }
    }

    /// <summary>The words at the ends the numbers run to, and beyond them: the record says each as the number it
    /// stands on, with no unit and no dash - the least subnormal, the greatest, the whole exponent, the number that is
    /// no number by its word and the two beyond the ends by theirs.</summary>
    [Theory]
    [InlineData(6, 10, "Median = 6, P85 = 10")]
    [InlineData(0.001, 0.002, "Median = 0.001, P85 = 0.002")]
    [InlineData(5e-324, 1e-323, "Median = 5E-324, P85 = 1E-323")]
    [InlineData(-1.7976931348623157e+308, 1.7976931348623157e+308, "Median = -1.7976931348623157E+308, P85 = 1.7976931348623157E+308")]
    public void TheWordsSayTheNumbersAtWhateverEndTheyStandOn(double median, double p85, string said) =>
        Assert.Contains(said, Words(new WorkDuration(median, p85, 1)), StringComparison.Ordinal);

    /// <summary>The words of the three numbers that are no ordinary number, and of the count at either end of an
    /// <see cref="int" />: <c>NaN</c> by its word, the two beyond the ends by theirs, and the negative nought with its
    /// sign - the words the equality asks nothing of, since two durations of <see cref="double.NaN" /> are one
    /// duration however they are said.</summary>
    [Fact]
    public void TheWordsSayTheNumbersThatAreNoOrdinaryNumber()
    {
        Assert.Equal(
            "WorkDuration { Median = NaN, P85 = NaN, N =  }",
            Words(new WorkDuration(double.NaN, double.NaN, null)));
        Assert.Equal(
            "WorkDuration { Median = Infinity, P85 = -Infinity, N = 1 }",
            Words(new WorkDuration(double.PositiveInfinity, double.NegativeInfinity, 1)));
        Assert.Equal(
            "WorkDuration { Median = -2147483648, P85 = 0, N = 2147483647 }",
            Words(new WorkDuration(int.MinValue, 0, int.MaxValue)));
        Assert.Equal("WorkDuration { Median = -0, P85 = , N =  }", Words(new WorkDuration(-0.0, null, null)));

        // A count of nought items is said as a nought; a count nobody gave is the empty place, which is the difference
        // the page reads as nothing to report rather than as an empty one.
        Assert.NotEqual(Words(new WorkDuration(6, 10, 0)), Words(new WorkDuration(6, 10, null)));
    }

    /// <summary>The numbers in the duration's own words follow whoever reads them, which is what sets them apart from
    /// the page's words and from the numbers themselves: a German reader of the record is told <c>P85 = 10,5</c> and
    /// the infinity beyond the ends as <c>∞</c>, while the part names stay the same letters to whichever reader - and
    /// two durations the two readers see differently worded are one duration still, for the words are no part of the
    /// equality.</summary>
    [Fact]
    public void TheRecordsOwnWordsFollowTheReaderWhileThePartsThemselvesDoNot()
    {
        var hours = Full;

        Assert.Equal("WorkDuration { Median = 6, P85 = 10.5, N = 3 }", Words(hours));

        ViewerCulture.German(() =>
        {
            Assert.Equal("WorkDuration { Median = 6, P85 = 10,5, N = 3 }", hours.ToString());
            Assert.Equal(
                "WorkDuration { Median = ∞, P85 = 10,5, N = 3 }",
                new WorkDuration(double.PositiveInfinity, 10.5, 3).ToString());
        });

        // The words apart by a comma, the duration the same: whoever reads the file, and whoever shows it, decide what
        // a duration means; the record only holds the three numbers, and hashes them the same whichever reader is asked.
        Assert.Equal(hours, new WorkDuration(6, 10.5, 3));
        ViewerCulture.German(() => Assert.Equal(hours.GetHashCode(), new WorkDuration(6, 10.5, 3).GetHashCode()));
    }

    // --------------------------------------------------------------------------- the machinery

    /// <summary>The public members the duration declares itself: its three properties and the methods that are not the
    /// get and the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the test of the surface rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(WorkDuration)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words a duration says its three parts in, taken in the invariant culture whatever the reader of
    /// this test happens to be: the record's own words follow whoever reads them, which is pinned on its own further
    /// up this file, so the strings there are the point and the culture only the way to say them once.</summary>
    private static string Words(WorkDuration duration) => ViewerCulture.Invariant(() => duration.ToString()!);
}
