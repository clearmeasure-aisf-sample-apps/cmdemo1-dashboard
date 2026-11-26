using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="BehindFirst" /> is how far behind an environment is of the first one, in the two numbers the file
/// gives it - how many releases the first environment is ahead in the project's list of releases, and how many days
/// have passed since the two last ran the same release. Both numbers are absent where the file does not say them,
/// and the record says nothing of its own: the words of the distance, <c>same as tdd</c> and <c>2 versions, 3 days
/// behind tdd</c>, are the page's and are pinned where they are said.
/// <para>
/// What is pinned where it belongs is walked past here on purpose: <see cref="DeliveryTests" /> reads the shipped
/// file's distances into the entries that hold them - <c>2</c> versions and <c>3.4</c> days for the sample's
/// <c>ui</c> in <c>uat</c>, nothing at all for <c>tdd</c>, which is compared with nobody - and pins the nine
/// sayings of the distance's words; <see cref="DeliveryReportEdgeCaseTests" /> pins the reading of a distance at its
/// ends - the numbers that are negative or words being no numbers, the section of no numbers saying nothing at all,
/// the section that is no section having no distance - and the days being whole before they are said;
/// <see cref="DeliveryEntryEdgeCaseTests" /> pins the entry holding the distance as the very record it stands in,
/// the entry's equality while the distance is one part of it, and the distance's words as they stand in the entry's
/// words; <see cref="DeployableFactsTests" /> pins the distance of the monitor's card, and
/// <see cref="ClusterDeliveryHelperTests" /> the tooltip that says what the distance counts.
/// </para>
/// <para>
/// What follows is the record of the two numbers itself, at the ends the tests of the file leave open. First the two
/// parts by the names of their own properties: the constructor handing each number to the property of its name and
/// holding it exactly - the fixed numbers the page reads, the two at the ends of <see cref="int" /> and of
/// <see cref="double" />, the negative numbers no file gives, and the number that is no number and the two endless
/// the record holds as it is given. Then the two parts coming out again in the order they went in; two distances
/// being one while their two parts are - the number that is no number being one with itself among them - and a part
/// turned or turned to nothing being another distance; the nothing on either side of the members every record is
/// asked, and the distances that say nothing standing apart; the copy that turns one part while it keeps the other;
/// the words the distance says itself - its name, the two parts in the order the constructor names them, said by
/// whoever reads them - and the public surface, so that the day a third number is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a distance holds two numbers, neither
/// a moment nor an address, and calls nothing at all - every number below is one the test brought.
/// </para>
/// </summary>
public class BehindFirstEdgeCaseTests
{
    // ------------------------------------------------ the two parts, by the names of their own properties

    /// <summary>The constructor's two numbers, position by position, come back as the property of the same name -
    /// the versions first and the days last, in the order the page says them - and are held exactly as they are
    /// given: the fixed numbers the file gives, the two at the nothing the counts stand on, and the two numbers
    /// turned about being another distance and not the same one read the other way. The parts that are not there
    /// stand not there, and no number is invented for them.</summary>
    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        Assert.Equal(7, new BehindFirst(7, 1.5).Versions);
        Assert.Equal(1.5, new BehindFirst(7, 1.5).Days);
        Assert.Equal(1, new BehindFirst(1, 0.25).Versions);
        Assert.Equal(0.25, new BehindFirst(1, 0.25).Days);
        Assert.Equal(3, new BehindFirst(3, 7.75).Versions);
        Assert.Equal(7.75, new BehindFirst(3, 7.75).Days);
        Assert.Equal(0, new BehindFirst(0, 0).Versions);
        Assert.Equal(0d, new BehindFirst(0, 0).Days);

        // The two numbers turned about: fifteen versions ahead by seven days, which is not seven versions ahead by
        // fifteen days.
        var turnedAbout = new BehindFirst(15, 7.0);

        Assert.Equal(15, turnedAbout.Versions);
        Assert.Equal(7.0, turnedAbout.Days);
        Assert.NotEqual(new BehindFirst(7, 15.0), turnedAbout);

        // The parts that are not there stand not there, at either of the two places.
        Assert.Null(new BehindFirst(null, null).Versions);
        Assert.Null(new BehindFirst(null, null).Days);
        Assert.Null(new BehindFirst(null, 1.5).Versions);
        Assert.Null(new BehindFirst(7, null).Days);
    }

    /// <summary>Each number is held at either end of the count it is - the versions at the two ends of
    /// <see cref="int" />, the days at the ends <see cref="double" /> reaches, one end beside the other - and the
    /// negative numbers no file gives stand where they stand, since the record holds what it is given and it is the
    /// reading that decides what a file may give, which is pinned where the file is read. A part at one end is not
    /// the part at the other while the other part stands where it does.</summary>
    [Theory]
    [InlineData(int.MinValue, 0)]
    [InlineData(int.MinValue, double.MinValue)]
    [InlineData(int.MaxValue, 0.5)]
    [InlineData(int.MaxValue, double.MaxValue)]
    [InlineData(-1, -1.5)]
    [InlineData(0, double.Epsilon)]
    [InlineData(1, -double.Epsilon)]
    [InlineData(7, 24.5)]
    public void EachPartIsHeldAtEitherEndOfItsNumber(int versions, double days)
    {
        var behind = new BehindFirst(versions, days);

        Assert.Equal(versions, behind.Versions);
        Assert.Equal(days, behind.Days);
    }

    /// <summary>The number that is no number and the two endless the file cannot give are held as they are given:
    /// the days of the distance are the number that is no number, and the record holds it and does not turn it -
    /// the comparison of two of them being the record's own, which the test of that name below says what it is.</summary>
    [Fact]
    public void TheNumberThatIsNoNumberAndTheTwoEndlessAreHeldAsTheyAreGiven()
    {
        var notANumber = new BehindFirst(1, double.NaN);

        Assert.Equal(1, notANumber.Versions);
        Assert.True(double.IsNaN(notANumber.Days!.Value));
        Assert.False(notANumber.Days!.Value > 0);
        Assert.False(notANumber.Days!.Value < 0);
        Assert.False(notANumber.Days!.Value == notANumber.Days!.Value);

        var up = new BehindFirst(1, double.PositiveInfinity);
        Assert.Equal(double.PositiveInfinity, up.Days);
        var down = new BehindFirst(1, double.NegativeInfinity);
        Assert.Equal(double.NegativeInfinity, down.Days);
        Assert.NotEqual(up, down);
    }

    // ------------------------------------------------------------ the two parts coming out again

    /// <summary>The two parts of a distance come out of it in the order they went in - the versions, then the days -
    /// whether the distance is asked for them at once or one after the other, and what came out goes back in the
    /// same way: the distance the two numbers fall apart into is the distance they were, of one hash and one entry
    /// of a set. The distance that says nothing falls apart to nothing and is a distance still, and the two numbers
    /// at the top of their counts fall apart at the top.</summary>
    [Fact]
    public void TheTwoPartsComeOutOfADistanceInTheOrderTheyWentIn()
    {
        var (versions, days) = new BehindFirst(7, 1.5);

        Assert.Equal(7, versions);
        Assert.Equal(1.5, days);
        Assert.Equal(new BehindFirst(7, 1.5), new BehindFirst(versions, days));
        Assert.Equal(new BehindFirst(7, 1.5).GetHashCode(), new BehindFirst(versions, days).GetHashCode());

        var top = new BehindFirst(int.MaxValue, double.MaxValue);
        top.Deconstruct(out var versionsAtTheTop, out var daysAtTheTop);

        Assert.Equal(int.MaxValue, versionsAtTheTop);
        Assert.Equal(double.MaxValue, daysAtTheTop);
        Assert.Equal(top, new BehindFirst(versionsAtTheTop, daysAtTheTop));

        var (nothing, none) = new BehindFirst(null, null);

        Assert.Null(nothing);
        Assert.Null(none);
        Assert.Equal(new BehindFirst(null, null), new BehindFirst(nothing, none));
    }

    // ------------------------------------------------ the distance, and the distance that is not there

    /// <summary>A distance is one with another while its two parts are one, whichever of the four ways of asking is
    /// used: the record's own <c>Equals</c>, the one of <see cref="object" />, <c>==</c> and <c>!=</c>. The hash
    /// follows the equality, boxed or unboxed and from one call to the next, and the parts are compared as numbers
    /// rather than as references - two distances with the same two parts, made by two different calls, rebuilt out
    /// of their own fallen-apart parts, or at the very top of the counts, are one distance and one entry of a set.</summary>
    [Fact]
    public void TwoDistancesAreOneWhileTheirTwoPartsAre()
    {
        var one = new BehindFirst(7, 1.5);
        var two = new BehindFirst(7, 1.5);

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
        Assert.Single(new HashSet<BehindFirst> { one, two });

        // The two parts asked out and put back in make the same distance again: the equality is of the numbers, not
        // of the call that gathered them.
        var (versions, days) = one;

        Assert.Equal(one, new BehindFirst(versions, days));
        Assert.Equal(one.GetHashCode(), new BehindFirst(versions, days).GetHashCode());

        // The top of the counts is compared as numbers too, and a set of them holds them once.
        var top = new BehindFirst(int.MaxValue, double.MaxValue);

        Assert.True(top == new BehindFirst(int.MaxValue, double.MaxValue));
        Assert.Equal(top.GetHashCode(), new BehindFirst(int.MaxValue, double.MaxValue).GetHashCode());
        Assert.Single(new HashSet<BehindFirst> { top, new BehindFirst(int.MaxValue, double.MaxValue) });
    }

    /// <summary>The record holds the number that is no number to be one with itself: the two distances whose days
    /// are both the number that is no number are one distance, of one hash and one entry of a set, although no
    /// comparison of two such numbers says so - which is the difference between the record's equality, which is of
    /// the parts it holds, and the comparison of the numbers it holds, which is the page's and is pinned where it is
    /// said. A distance of the endless is not one of the number that is no number, and one more version is another
    /// distance still.</summary>
    [Fact]
    public void TwoDistancesOfTheNumberThatIsNoNumberAreOneDistance()
    {
        var one = new BehindFirst(1, double.NaN);
        var two = new BehindFirst(1, double.NaN);

        Assert.True(one.Equals(two));
        Assert.True(one == two);
        Assert.False(one != two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Single(new HashSet<BehindFirst> { one, two });

        Assert.NotEqual(one, new BehindFirst(2, double.NaN));
        Assert.NotEqual(one, new BehindFirst(1, double.PositiveInfinity));
        Assert.NotEqual(one, new BehindFirst(1, 0.5));
        Assert.False(one.Equals((object)double.NaN));
    }

    /// <summary>A turn of either part is another distance: one version more or one version fewer, a tenth of a day
    /// more, the days turned negative in the way no file gives them, a part turned to nothing, both parts turned to
    /// nothing, or the two numbers turned about - the comparison is of the two parts one after the other, so a
    /// distance a part apart is another distance whatever the two are, and the four of them stand apart in a set.</summary>
    [Fact]
    public void APartTurnedOrTurnedToNothingIsAnotherDistance()
    {
        var one = new BehindFirst(7, 1.5);

        Assert.NotEqual(one, new BehindFirst(8, 1.5));
        Assert.NotEqual(one, new BehindFirst(6, 1.5));
        Assert.NotEqual(one, new BehindFirst(7, 1.51));
        Assert.NotEqual(one, new BehindFirst(7, 0));
        Assert.NotEqual(one, new BehindFirst(7, -1.5));
        Assert.NotEqual(one, new BehindFirst(null, 1.5));
        Assert.NotEqual(one, new BehindFirst(7, null));
        Assert.NotEqual(one, new BehindFirst(null, null));
        Assert.NotEqual(one, new BehindFirst(15, 7.0));

        Assert.True(one != new BehindFirst(8, 1.5));
        Assert.False(one == new BehindFirst(7, 1.51));
        Assert.False(one.Equals(new BehindFirst(null, 1.5)));
        Assert.False(one.Equals((object)new BehindFirst(7, null)));

        // The distances that differ from the one by a part or by a nothing are four apart.
        Assert.Equal(
            4,
            new HashSet<BehindFirst>
            {
                one,
                one with { Versions = 8 },
                one with { Days = 1.51 },
                new BehindFirst(null, 1.5),
            }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two distances that are no
    /// distance at all are one, a distance and no distance are two - the equality and the inequality each saying it,
    /// from either side - and the distance asked about itself says no to the nothing, to the words the page says of
    /// it, to a part standing alone, to the other records that hold a number and to the anonymous thing that happens
    /// to carry its two parts - while the distances that say nothing are each their own: the one that says nothing
    /// at all, the one that says only how far in days, the one that says only how far in versions, the three of them
    /// standing apart in a set.</summary>
    [Fact]
    public void NoDistanceIsOneWithNothingAndTheDistancesThatSayNothingAreEachTheirOwn()
    {
        BehindFirst? nothing = null;
        BehindFirst? alsoNothing = null;
        var there = new BehindFirst(7, 1.5);

        Assert.Null(default(BehindFirst));
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == nothing);
        Assert.True(there != nothing);
        Assert.False(there.Equals(nothing));
        Assert.True(there.Equals(there));
        Assert.False(there.Equals(null));
        Assert.False(there.Equals((BehindFirst?)null));
        Assert.False(there.Equals((object?)null));

        Assert.False(there.Equals("2 versions, 3 days behind tdd"));
        Assert.False(there.Equals(7));
        Assert.False(there.Equals(1.5));
        Assert.False(there.Equals(new { Versions = 7, Days = 1.5 }));
        Assert.False(there.Equals(new HealthWindow(7, 7)));
        Assert.False(there.Equals(new BehindFirst(null, null)));
        Assert.True(there.Equals((object)there));

        var none = new BehindFirst(null, null);
        var onlyDays = new BehindFirst(null, 1.5);
        var onlyVersions = new BehindFirst(7, null);

        Assert.Null(none.Versions);
        Assert.Null(none.Days);
        Assert.Null(onlyDays.Versions);
        Assert.Equal(1.5, onlyDays.Days);
        Assert.Equal(7, onlyVersions.Versions);
        Assert.Null(onlyVersions.Days);

        Assert.NotEqual(none, onlyDays);
        Assert.NotEqual(none, onlyVersions);
        Assert.NotEqual(onlyDays, onlyVersions);
        Assert.True(none != onlyDays);
        Assert.False(none == onlyVersions);
        Assert.Equal(3, new HashSet<BehindFirst> { none, onlyDays, onlyVersions }.Count);
    }

    // ------------------------------------------------------------------ the copy, turning the part it names

    /// <summary>Either part may be turned as the distance is copied: the copy that names nothing is the distance it
    /// started from in another reference, of one hash, the copy that names one part keeps the other exactly as it
    /// stood, and the original is left where it stood. A distance whose two parts are turned to nothing is the
    /// distance that says nothing - one with the one made of the two nothings, and another from the one that said
    /// both.</summary>
    [Fact]
    public void ACopyTurnsThePartItNamesAndKeepsTheOther()
    {
        var original = new BehindFirst(7, 1.5);

        var unchanged = original with { };

        Assert.Equal(original, unchanged);
        Assert.NotSame(original, unchanged);
        Assert.Equal(original.GetHashCode(), unchanged.GetHashCode());

        var moreVersions = original with { Versions = 9 };

        Assert.Equal(9, moreVersions.Versions);
        Assert.Equal(1.5, moreVersions.Days);
        Assert.NotEqual(original, moreVersions);
        Assert.NotSame(original, moreVersions);
        Assert.Equal(7, original.Versions);
        Assert.Equal(1.5, original.Days);

        var noDays = original with { Days = null };

        Assert.Equal(7, noDays.Versions);
        Assert.Null(noDays.Days);
        Assert.NotEqual(original, noDays);

        var silent = original with { Versions = null, Days = null };

        Assert.Null(silent.Versions);
        Assert.Null(silent.Days);
        Assert.NotEqual(original, silent);
        Assert.Equal(silent, new BehindFirst(null, null));
        Assert.Equal(silent.GetHashCode(), new BehindFirst(null, null).GetHashCode());
    }

    // ------------------------------------------------------------ the words the distance says itself

    /// <summary>The words the distance says its two parts in: its own name at the front, the versions first and the
    /// days last as they are named, each number said as it stands - the fixed numbers with the decimal they carry,
    /// the two at the very top of their counts, the negative numbers with their sign, the number that is no number
    /// and the two endless in their own words - and the part that is not there the empty place between the mark and
    /// the comma or the closing brace. These are the record's words and not the page's: the page says
    /// <c>2 versions, 3 days behind tdd</c>, and the distance says the two numbers it holds.</summary>
    [Fact]
    public void TheDistanceSaysItsTwoPartsInTheOrderTheyAreNamed()
    {
        Assert.Equal("BehindFirst { Versions = 7, Days = 1.5 }", Words(new BehindFirst(7, 1.5)));
        Assert.Equal("BehindFirst { Versions = 1, Days = 0.25 }", Words(new BehindFirst(1, 0.25)));
        Assert.Equal("BehindFirst { Versions = 0, Days = 0 }", Words(new BehindFirst(0, 0)));
        Assert.Equal("BehindFirst { Versions = -2147483648, Days = -0.5 }", Words(new BehindFirst(int.MinValue, -0.5)));
        Assert.Equal(
            "BehindFirst { Versions = 2147483647, Days = 1.7976931348623157E+308 }",
            Words(new BehindFirst(int.MaxValue, double.MaxValue)));
        Assert.Equal("BehindFirst { Versions = 1, Days = NaN }", Words(new BehindFirst(1, double.NaN)));
        Assert.Equal("BehindFirst { Versions = 1, Days = Infinity }", Words(new BehindFirst(1, double.PositiveInfinity)));
        Assert.Equal("BehindFirst { Versions = 1, Days = -Infinity }", Words(new BehindFirst(1, double.NegativeInfinity)));
        // The part that is not there leaves the space the mark had, so the last one is said with two before the brace.
        Assert.Equal("BehindFirst { Versions = 7, Days =  }", Words(new BehindFirst(7, null)));
        Assert.Equal("BehindFirst { Versions = , Days = 1.5 }", Words(new BehindFirst(null, 1.5)));
        Assert.Equal("BehindFirst { Versions = , Days =  }", Words(new BehindFirst(null, null)));
    }

    /// <summary>The distance's words are the record's own, and they follow whoever reads them: a reader of German is
    /// told <c>1,5</c> and <c>0,25</c> by the distance, and the invariant reader <c>1.5</c> and <c>0.25</c> - while
    /// the words the page says of the distance, <c>2 versions, 3 days behind tdd</c>, are the invariant culture's,
    /// written for the dashboard and not for whoever looks, and are pinned where they are said. The distance that
    /// says nothing has no number in it, and its words are the same for whichever reader.</summary>
    [Fact]
    public void TheDistancesOwnWordsFollowWhoeverReadsThem()
    {
        var distance = new BehindFirst(7, 1.5);

        ViewerCulture.German(() => Assert.Equal("BehindFirst { Versions = 7, Days = 1,5 }", distance.ToString()));
        ViewerCulture.German(() => Assert.Equal("BehindFirst { Versions = 1, Days = 0,25 }", new BehindFirst(1, 0.25).ToString()));
        ViewerCulture.German(() => Assert.Equal("BehindFirst { Versions = 1, Days = NaN }", new BehindFirst(1, double.NaN).ToString()));

        // No number in it, and the words the same for whichever reader: no culture to turn them.
        var silent = new BehindFirst(null, null);

        ViewerCulture.German(() => Assert.Equal(Words(silent), silent.ToString()));
        Assert.Equal("BehindFirst { Versions = , Days =  }", silent.ToString());
    }

    // ------------------------------------------------------------------------ the record's own shape

    /// <summary>The distance's whole public surface, as it stands: its two parts, named in the order the constructor
    /// names them and of the type the constructor says - the versions as the count of releases, the days as the
    /// number a count of days may be - each read and the place a <c>with</c> may turn it, and the members every
    /// record is asked - falling apart, comparing, hashing and saying itself - and nothing else to call. A third
    /// number added to the distance has to be written here as well, which is the point: the page cannot be shown a
    /// part nobody decided what it means.</summary>
    [Fact]
    public void TheDistanceHasTwoPartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Versions", "Days"],
            [.. typeof(BehindFirst)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["Days", "Deconstruct", "Equals", "GetHashCode", "ToString", "Versions", "op_Equality", "op_Inequality"],
            PublicSurface());

        Assert.Equal([typeof(IEquatable<BehindFirst>)], typeof(BehindFirst).GetInterfaces());
        Assert.True(typeof(BehindFirst).IsSealed);

        var properties = typeof(BehindFirst)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(int?), properties["Versions"].PropertyType);
        Assert.Equal(typeof(double?), properties["Days"].PropertyType);
        Assert.True(properties["Versions"].CanRead);
        Assert.True(properties["Versions"].CanWrite);
        Assert.True(properties["Days"].CanRead);
        Assert.True(properties["Days"].CanWrite);

        // Two numbers, and the one constructor that takes them in that order.
        var constructors = typeof(BehindFirst).GetConstructors();

        Assert.Single(constructors);
        var parameters = constructors[0].GetParameters();

        Assert.Equal(["Versions", "Days"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal(typeof(int?), parameters[0].ParameterType);
        Assert.Equal(typeof(double?), parameters[1].ParameterType);
        Assert.False(parameters[0].HasDefaultValue);
        Assert.False(parameters[1].HasDefaultValue);
    }

    /// <summary>The public members the distance declares itself: its properties and the methods that are not the
    /// get and the set of one - the two operators kept although the compiler names them specially, since they are
    /// the <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(BehindFirst)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words a distance says its two parts in, taken in the invariant culture whatever the reader of
    /// this test happens to be: the record's own words follow whoever reads them, which is pinned on its own further
    /// up this file, so the strings there are the point and the culture only the way to say them once.</summary>
    private static string Words(BehindFirst behind) => ViewerCulture.Invariant(() => behind.ToString()!);
}
