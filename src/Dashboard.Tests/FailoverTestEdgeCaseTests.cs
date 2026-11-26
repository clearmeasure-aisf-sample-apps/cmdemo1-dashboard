using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="FailoverTest" /> is the last test of the failover to the standby region, in the three parts the
/// <c>failover</c> object of <c>delivery.json</c> gives it: the environment it was tested in, when it happened, and
/// how long the public address took to answer from the standby. Three parts, held and nothing else: the record
/// counts nothing against a clock, draws no verdict and says no word of its own for the page - the age of the
/// moment is counted later, from the moment the page is asked, and the line is the card's.
/// <para>
/// What is pinned where it belongs is walked past here on purpose: <see cref="DeliveryTests" /> and
/// <see cref="DeliveryEnvironmentTests" /> pin the test as the file reads it - the fixture's three parts, a file of
/// nothing but the failover, the moment the file cannot say leaving the seconds standing, and the object that says
/// nothing at all being no test while a report's failover is compared to a test the test wrote out, which leans on
/// the record's <c>Equals</c> without ever saying a word about it; <see cref="DeliveryTests" /> and
/// <see cref="DeliveryReportEdgeCaseTests" /> pin the words the card makes of the three parts, in every
/// combination of their being there and not, the seconds rounded to the tenth the line says them in, the invariant
/// point a reader of German is given there, and the refusal of a test that is not there; the strips the line stands
/// in are pinned by <see cref="TimeElementGuardTests" />.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the record of the three parts itself, at the ends the tests of the
/// file leave open. First the parts by the names of their own properties: the constructor handing each part to the
/// property of its name and holding it exactly - the fixed parts the fixture gives, the very instance of the name,
/// the seconds kept to the last digit while the card rounds them, and the numbers at the ends of what a real
/// number holds. Then the name kept as it was written, empty, padded and in its own case - where the lookups of a
/// report fold the case; the moment held to the tick at the two ends of its number and a tick beside it, and the
/// same instant spelled in other hours. Then the test that is its parts: two tests one while their three parts
/// are, the hash that follows, a part turned making another, the tests that are not there, the not-a-number and the
/// negative zero of the seconds; the copy a <c>with</c> makes, the three parts read out in the order they stand in,
/// the line the record says about itself - its name, its three parts and the nothing among them, the moment a tick
/// apart saying one line while it is two tests, and the moment and the tenth of a second said in the hours the
/// viewer speaks, which the card does not do - and the public surface, so that the day a fourth part is added the
/// list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a test holds three parts, calls
/// nothing at all, and every part below is one the test brought. Reading the <c>failover</c> object is
/// <c>FailoverTest.Read</c>, internal to an app that declares no <c>InternalsVisibleTo</c> (as the test project's
/// own file notes of <c>JsonRead</c>), and so is pinned through <c>DeliveryReport.Parse</c> by the two files named
/// above; the age of the moment is <c>TimeText.Ago</c>'s and is pinned with it, in the card's line.
/// </para>
/// </summary>
public class FailoverTestEdgeCaseTests
{
    /// <summary>The moment the test happened, on the meridian: the fixture's moment, and the tests below stand on it.</summary>
    private static readonly DateTimeOffset At = new(2026, 10, 6, 8, 30, 0, TimeSpan.Zero);

    /// <summary>The very instant of <see cref="At" />, five and a half hours east of the meridian.</summary>
    private static readonly DateTimeOffset AtInOtherHours = new(2026, 10, 6, 14, 0, 0, TimeSpan.FromMinutes(330));

    /// <summary>A test of the parts it is told, the parts left out the way the file leaves them out.</summary>
    private static FailoverTest Test(string? environment = null, DateTimeOffset? at = null, double? seconds = null) =>
        new(environment, at, seconds);

    // ------------------------------------------------- the three parts, by the names of their own properties

    /// <summary>
    /// The constructor's three parts, position by position, come back as the property of the same name: the
    /// environment first, the moment next, the seconds last - the order the file writes them in and the order the
    /// card says them in. Each part named for itself lands in its own place with the other two left out, and the
    /// name the record is told it holds is the very instance it was given: a test the file read and a test the card
    /// makes stand on the same word and not on a copy of it.
    /// </summary>
    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        var name = new string("uat".ToCharArray());
        var test = new FailoverTest(name, At, 44);

        Assert.Equal("uat", test.Environment);
        Assert.Equal(At, test.At);
        Assert.Equal(44d, test.Seconds);
        Assert.Same(name, test.Environment);

        // Each part by itself: the other two left out the way the file leaves them out, not said as anything.
        Assert.Equal("prod", Test(environment: "prod").Environment);
        Assert.Null(Test(environment: "prod").At);
        Assert.Null(Test(environment: "prod").Seconds);

        Assert.Equal(At, Test(at: At).At);
        Assert.Null(Test(at: At).Environment);
        Assert.Null(Test(at: At).Seconds);

        Assert.Equal(44.5d, Test(seconds: 44.5).Seconds);
        Assert.Null(Test(seconds: 44.5).Environment);
        Assert.Null(Test(seconds: 44.5).At);
    }

    /// <summary>
    /// The seconds are held to the last digit of the number they were given as and not to the tenth the card says
    /// them in: <c>44.49999</c> stands while the line rounds it to <c>44.5 s</c>, and <c>0.04</c> stands while the
    /// line says it as none. The two ends of what a real number holds, the smallest number there is, the
    /// not-a-number and the two infinities a file may write and no stopwatch answers, and the negative seconds no
    /// stopwatch runs backwards into are all held as they came - the record measures nothing and refuses nothing,
    /// which is the file's question and is pinned where the file is read.
    /// </summary>
    [Theory]
    [InlineData(44d)]
    [InlineData(44.5d)]
    [InlineData(44.49999d)]
    [InlineData(0.04d)]
    [InlineData(0d)]
    [InlineData(-1d)]
    [InlineData(1e-7d)]
    [InlineData(double.Epsilon)]
    [InlineData(double.MinValue)]
    [InlineData(double.MaxValue)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void TheSecondsAreHeldToTheLastDigitOfTheNumberTheyWereGivenAs(double seconds)
    {
        var test = Test(seconds: seconds);

        Assert.NotNull(test.Seconds);
        Assert.Equal(seconds, test.Seconds!.Value);
        Assert.Null(test.Environment);
        Assert.Null(test.At);
    }

    /// <summary>
    /// The name of the environment is kept as it was written: the empty name is a name, the whitespace the file may
    /// pad it with stays with it, a name of ten thousand letters is neither shortened nor measured, and the case is
    /// the case - the name in another case being another test, although the lookups of a report find an environment
    /// whatever case they are asked in. A test names an environment rather than looking one up, so nothing of it is
    /// matched here: the word goes in as it came.
    /// </summary>
    [Fact]
    public void TheEnvironmentIsHeldAsItWasWritten()
    {
        Assert.Equal(string.Empty, Test(environment: string.Empty).Environment);
        Assert.Equal(" ", Test(environment: " ").Environment);
        Assert.Equal(" uat ", Test(environment: " uat ").Environment);
        Assert.Equal("system", Test(environment: "system").Environment);
        Assert.Equal("SYSTEM", Test(environment: "SYSTEM").Environment);
        Assert.Equal("westus3 → eastus2", Test(environment: "westus3 → eastus2").Environment);

        var longest = new string('u', 10_000);

        Assert.Same(longest, Test(environment: longest).Environment);
        Assert.Equal(10_000, Test(environment: longest).Environment!.Length);

        // The record does not fold the case the file's lookups fold: the same environment in another case is
        // another test, and one letter more is another still.
        Assert.NotEqual(Test(environment: "uat"), Test(environment: "UAT"));
        Assert.NotEqual(Test(environment: "uat"), Test(environment: "uats"));
        Assert.NotEqual(Test(environment: string.Empty), Test(environment: " "));
    }

    // --------------------------------------------------------- the moment, at the ends of its number

    /// <summary>
    /// The moment the test stands on is held to the tick: the first moment there are and the last, the fixture's
    /// moment a tick either side of it, and a moment written in the hours the file gives it. The record compares
    /// the tick, so the test one tick later is another test - though the words it says are the words of the second
    /// it stands in, and though the card calls both of them the same age.
    /// </summary>
    [Fact]
    public void TheMomentTheTestStoodOnIsHeldToTheTick()
    {
        Assert.Equal(DateTimeOffset.MinValue, Test(at: DateTimeOffset.MinValue).At);
        Assert.Equal(DateTimeOffset.MaxValue, Test(at: DateTimeOffset.MaxValue).At);
        Assert.Equal(AtInOtherHours, Test(at: AtInOtherHours).At);

        var tickBefore = At.AddTicks(-1);
        var tickAfter = At.AddTicks(1);

        Assert.Equal(tickBefore, Test(at: tickBefore).At);
        Assert.Equal(tickAfter, Test(at: tickAfter).At);
        Assert.Equal(At.Ticks, Test(at: At).At!.Value.Ticks);
        Assert.Equal(TimeSpan.FromMinutes(330), Test(at: AtInOtherHours).At!.Value.Offset);

        Assert.NotEqual(Test(at: At), Test(at: tickAfter));
        Assert.NotEqual(Test(at: At), Test(at: tickBefore));
        Assert.NotEqual(Test(at: tickAfter), Test(at: tickBefore));
    }

    /// <summary>
    /// The very instant, whichever hours write it, is the same test: the moment the file writes on the meridian and
    /// that same moment five and a half hours east of it are one test and one entry of a set - the operator,
    /// <c>Equals</c> boxed, and the hash that goes with them - while the instant one second later in those other
    /// hours is another. The hours the moment stands in are kept for the words, which is what the page dates its
    /// tooltip with; they are no part of which moment it is.
    /// </summary>
    [Fact]
    public void TheSameInstantInOtherHoursIsTheSameTest()
    {
        var onTheMeridian = Test("uat", At, 44);
        var inOtherHours = Test("uat", AtInOtherHours, 44);

        Assert.Equal(onTheMeridian, inOtherHours);
        Assert.True(onTheMeridian == inOtherHours);
        Assert.True(inOtherHours.Equals((object)onTheMeridian));
        Assert.False(onTheMeridian != inOtherHours);
        Assert.Equal(onTheMeridian.GetHashCode(), inOtherHours.GetHashCode());
        Assert.Single(new HashSet<FailoverTest> { onTheMeridian, inOtherHours });

        // The hours are kept while the instant is compared: the words of the two are not one word.
        Assert.Equal(At, inOtherHours.At);
        Assert.Equal(AtInOtherHours, onTheMeridian.At!.Value.ToOffset(AtInOtherHours.Offset));
        Assert.NotEqual(onTheMeridian, Test("uat", AtInOtherHours.AddSeconds(1), 44));
    }

    // ------------------------------------------------------------ the three parts read out

    /// <summary>
    /// The three parts come out of a test in the order they went in - the environment, the moment, the seconds -
    /// whether the test is fallen apart at once or asked for them one after the other, and what came out goes back
    /// in the same way: the test the parts fall apart into is the test they were. A test of nothing falls apart
    /// into three nothings, which are still three parts of a test that happened, and the number at the end of what
    /// a real number holds comes out at the end of it, not turned into anything else.
    /// </summary>
    [Fact]
    public void TheThreePartsComeOutOfATestInTheOrderTheyWentIn()
    {
        var (environment, at, seconds) = Test("uat", At, 44.5);

        Assert.Equal("uat", environment);
        Assert.Equal(At, at);
        Assert.Equal(44.5d, seconds);
        Assert.Equal(Test("uat", At, 44.5), new FailoverTest(environment, at, seconds));

        var asked = Test("prod", AtInOtherHours, 12.5);
        asked.Deconstruct(out var nameAsked, out var momentAsked, out var numberAsked);

        Assert.Equal("prod", nameAsked);
        Assert.Equal(AtInOtherHours, momentAsked);
        Assert.Equal(12.5d, numberAsked);

        var (noName, noMoment, noNumber) = Test();

        Assert.Null(noName);
        Assert.Null(noMoment);
        Assert.Null(noNumber);

        var (_, atTheEnd, theSmallestNumber) = Test("uat", DateTimeOffset.MaxValue, double.MinValue);

        Assert.Equal(DateTimeOffset.MaxValue, atTheEnd);
        Assert.Equal(double.MinValue, theSmallestNumber);
    }

    // ------------------------------------------------------------- the test, and the test not there

    /// <summary>
    /// A test is one with another while its three parts are, whichever of the ways of asking is used: the record's
    /// own <c>Equals</c>, the one of <see cref="object" />, <c>==</c> and <c>!=</c>. The hash follows the equality,
    /// boxed or unboxed and from one call to the next, and the parts are compared rather than the references - two
    /// tests of the same three parts, made by two calls, rebuilt out of their own fallen-apart parts, or of one of
    /// nothing at all, are one test and one entry of a set. A part of another kind is no test's equal.
    /// </summary>
    [Fact]
    public void TwoTestsAreOneWhileTheirThreePartsAre()
    {
        var one = Test("uat", At, 44.5);
        var two = Test("uat", At, 44.5);

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
        Assert.Single(new HashSet<FailoverTest> { one, two });

        // The parts asked out and put back in make the same test again.
        one.Deconstruct(out var environment, out var at, out var seconds);

        Assert.Equal(one, new FailoverTest(environment, at, seconds));
        Assert.Equal(one.GetHashCode(), new FailoverTest(environment, at, seconds).GetHashCode());

        // The test that says nothing at all is a test, and two of them are one.
        Assert.Equal(Test(), Test());
        Assert.True(Test() == Test());
        Assert.Equal(Test().GetHashCode(), Test().GetHashCode());
        Assert.Single(new HashSet<FailoverTest> { Test(), Test() });
    }

    /// <summary>
    /// A difference in either part is another test: one letter more in the name, the name in another case, the name
    /// the file pads with whitespace, the name that is not there where the empty name is; the moment a second or a
    /// tick apart, the moment not there where it is, and the first moment where the last one is; the seconds by a
    /// tenth, onto the nothing, onto the zero the nothing is not, and about into the negative. The comparison is of
    /// the three parts one after the other, and the tests that differ stand apart in a set.
    /// </summary>
    [Fact]
    public void APartTurnedIsAnotherTest()
    {
        var one = Test("uat", At, 44.5);

        Assert.NotEqual(one, Test("uat ", At, 44.5));
        Assert.NotEqual(one, Test("UAT", At, 44.5));
        Assert.NotEqual(one, Test(string.Empty, At, 44.5));
        Assert.NotEqual(one, Test(null, At, 44.5));
        Assert.NotEqual(one, Test("uat", At.AddSeconds(1), 44.5));
        Assert.NotEqual(one, Test("uat", null, 44.5));
        Assert.NotEqual(one, Test("uat", DateTimeOffset.MinValue, 44.5));
        Assert.NotEqual(one, Test("uat", At, 44.6));
        Assert.NotEqual(one, Test("uat", At, 0d));
        Assert.NotEqual(one, Test("uat", At, null));

        Assert.True(one != Test("uat", At, 44));
        Assert.False(one == Test("prod", At, 44.5));
        Assert.False(one.Equals((object)Test("uat", At, 44)));

        // The empty name and no name are two tests that say one line: see the words below.
        Assert.NotEqual(Test(environment: string.Empty), Test());

        Assert.Equal(
            4,
            new HashSet<FailoverTest>
            {
                one,
                one with { Environment = "prod" },
                one with { At = At.AddSeconds(1) },
                one with { Seconds = 44 },
            }.Count);
    }

    /// <summary>
    /// The nothing on either side of the members every record is asked with: two tests that are no test at all are
    /// one, a test and no test are two - the equality and the inequality each saying it, from either side - and the
    /// test asked about itself says no to the nothing, to the number it was made of and to the words of its parts.
    /// The test that was told of nothing is still a test: it is one with itself and another than the test of a part
    /// that is only as good as nothing - the empty name, the zero of the seconds, the first moment there are.
    /// </summary>
    [Fact]
    public void NoTestIsOneWithNoTestAndTheTestOfNothingIsATest()
    {
        FailoverTest? nothing = null;
        FailoverTest? alsoNothing = null;
        var there = Test("uat", At, 44);

        Assert.Null(default(FailoverTest));
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == nothing);
        Assert.True(there != nothing);
        Assert.False(there.Equals(nothing));
        Assert.True(there.Equals(there));

        Assert.False(there.Equals(null));
        Assert.False(there.Equals((FailoverTest?)null));
        Assert.False(there.Equals((object?)null));
        Assert.False(there.Equals(44d));
        Assert.False(there.Equals("uat"));
        Assert.False(there.Equals(At));
        Assert.False(there.Equals(new HealthWindow(24, 23)));
        Assert.False(there.Equals(new DeliveryEnvironment("uat", [])));

        Assert.NotEqual(there, Test());
        Assert.NotEqual(Test(), Test(environment: string.Empty));
        Assert.NotEqual(Test(), Test(seconds: 0d));
        Assert.NotEqual(Test(), Test(at: DateTimeOffset.MinValue));
    }

    /// <summary>
    /// The not-a-number and the negative zero of the seconds, at the ends of what a real number is: two tests on
    /// the not-a-number are one test and one entry of a set - the record comparing its parts as the numbers
    /// themselves are compared, and two numbers no number is being one with themselves - while the not-a-number is
    /// another test than either infinity and than the largest number a real number holds. The zero and the
    /// negative zero are one test, the record not asking the sign of a nothing: the words of the two are two words,
    /// which is what a reader of the line would see and what the card rounds away.
    /// </summary>
    [Fact]
    public void TheNotANumberAndTheNegativeZeroOfTheSecondsAreHeldAsNumbersAreHeld()
    {
        var notANumber = Test("uat", At, double.NaN);
        var alsoNotANumber = Test("uat", At, double.NaN);

        Assert.Equal(notANumber, alsoNotANumber);
        Assert.True(notANumber == alsoNotANumber);
        Assert.False(notANumber != alsoNotANumber);
        Assert.Equal(notANumber.GetHashCode(), alsoNotANumber.GetHashCode());
        Assert.Single(new HashSet<FailoverTest> { notANumber, alsoNotANumber });

        // The not-a-number is another test than every number it is not one with.
        Assert.NotEqual(notANumber, Test("uat", At, double.PositiveInfinity));
        Assert.NotEqual(notANumber, Test("uat", At, double.NegativeInfinity));
        Assert.NotEqual(notANumber, Test("uat", At, double.MaxValue));
        Assert.NotEqual(notANumber, Test("uat", At, null));
        Assert.NotEqual(Test("uat", At, double.PositiveInfinity), Test("uat", At, double.NegativeInfinity));

        var zero = Test("uat", At, 0d);
        var negativeZero = Test("uat", At, -0d);

        Assert.Equal(zero, negativeZero);
        Assert.True(zero == negativeZero);
        Assert.Equal(zero.GetHashCode(), negativeZero.GetHashCode());
        Assert.Single(new HashSet<FailoverTest> { zero, negativeZero });

        // One test, two words: the sign of the nothing is kept and said.
        Assert.Equal("FailoverTest { Environment = uat, At = 10/06/2026 08:30:00 +00:00, Seconds = 0 }", Words(zero));
        Assert.Equal("FailoverTest { Environment = uat, At = 10/06/2026 08:30:00 +00:00, Seconds = -0 }", Words(negativeZero));
    }

    // ---------------------------------------------------------------- the copy, and the part it turns

    /// <summary>
    /// Either part may be turned as the test is made and either may be turned in a copy: the copy that names
    /// nothing is the test it started from in another reference, the copy that names one part keeps the other two
    /// exactly as they stood, and the original is left where it stood. A part turned onto the part it was is the
    /// same test again - the hours of the moment included, since the instant is what the test stands on - and a
    /// part turned to the nothing is the test the file would have left out.
    /// </summary>
    [Fact]
    public void ACopyTurnsThePartItNamesAndKeepsTheOthers()
    {
        var original = Test("uat", At, 44.5);

        var unchanged = original with { };

        Assert.Equal(original, unchanged);
        Assert.NotSame(original, unchanged);
        Assert.Equal(original.GetHashCode(), unchanged.GetHashCode());

        var elsewhere = original with { Environment = "prod" };

        Assert.Equal("prod", elsewhere.Environment);
        Assert.Equal(At, elsewhere.At);
        Assert.Equal(44.5d, elsewhere.Seconds);
        Assert.NotEqual(original, elsewhere);

        var undated = original with { At = null };

        Assert.Equal("uat", undated.Environment);
        Assert.Null(undated.At);
        Assert.Equal(44.5d, undated.Seconds);
        Assert.Equal(At, original.At);

        var uncounted = original with { Seconds = null };

        Assert.Null(uncounted.Seconds);
        Assert.Equal("uat", uncounted.Environment);
        Assert.Equal(At, uncounted.At);

        // The three parts turned in one copy: the test the file says nothing of, and the original still standing.
        Assert.Equal(Test(), original with { Environment = null, At = null, Seconds = null });
        Assert.Equal(44.5d, original.Seconds);

        // A part turned onto what it was is the same test - the moment in the other hours included.
        Assert.Equal(original, original with { Seconds = 44.5 });
        Assert.Equal(original, original with { At = AtInOtherHours });
    }

    // ------------------------------------------------------------ the words the test says itself

    /// <summary>
    /// The words the test says its three parts in: its own name at the front, then the environment, the moment and
    /// the seconds as they are named - the moment in the hours the file wrote it and the seconds to the last digit
    /// they were given as. A part the test does not have says its name and nothing more, so the test of nothing at
    /// all is three names and three blanks; the empty name says no more than no name does, although the two are two
    /// tests; and the moment a whole second apart in its smallest parts is one word the test does not say.
    /// </summary>
    [Fact]
    public void TheTestSaysItsThreePartsInTheirOrder()
    {
        Assert.Equal(
            "FailoverTest { Environment = uat, At = 10/06/2026 08:30:00 +00:00, Seconds = 44 }",
            Words(Test("uat", At, 44)));
        Assert.Equal("FailoverTest { Environment = , At = , Seconds =  }", Words(Test()));
        Assert.Equal("FailoverTest { Environment = , At = , Seconds =  }", Words(Test(environment: string.Empty)));
        Assert.Equal("FailoverTest { Environment =  , At = , Seconds =  }", Words(Test(environment: " ")));
        Assert.Equal("FailoverTest { Environment = uat, At = , Seconds =  }", Words(Test(environment: "uat")));
        Assert.Equal("FailoverTest { Environment = , At = , Seconds = 44.5 }", Words(Test(seconds: 44.5)));
        Assert.Equal("FailoverTest { Environment = , At = , Seconds = 0.04 }", Words(Test(seconds: 0.04)));
        Assert.Equal("FailoverTest { Environment = , At = , Seconds = 44.49999 }", Words(Test(seconds: 44.49999)));
        Assert.Equal("FailoverTest { Environment = , At = , Seconds = 1E-07 }", Words(Test(seconds: 1e-7)));
        Assert.Equal("FailoverTest { Environment = , At = , Seconds = 5E-324 }", Words(Test(seconds: double.Epsilon)));
        Assert.Equal(
            "FailoverTest { Environment = , At = , Seconds = 1.7976931348623157E+308 }",
            Words(Test(seconds: double.MaxValue)));
        Assert.Equal(
            "FailoverTest { Environment = , At = , Seconds = -1.7976931348623157E+308 }",
            Words(Test(seconds: double.MinValue)));
        Assert.Equal("FailoverTest { Environment = , At = , Seconds = NaN }", Words(Test(seconds: double.NaN)));
        Assert.Equal("FailoverTest { Environment = , At = , Seconds = Infinity }", Words(Test(seconds: double.PositiveInfinity)));
        Assert.Equal("FailoverTest { Environment = , At = , Seconds = -Infinity }", Words(Test(seconds: double.NegativeInfinity)));
        Assert.Equal(
            "FailoverTest { Environment = , At = 01/01/0001 00:00:00 +00:00, Seconds =  }",
            Words(Test(at: DateTimeOffset.MinValue)));
        Assert.Equal(
            "FailoverTest { Environment = , At = 12/31/9999 23:59:59 +00:00, Seconds =  }",
            Words(Test(at: DateTimeOffset.MaxValue)));
        Assert.Equal(
            "FailoverTest { Environment = , At = 10/06/2026 14:00:00 +05:30, Seconds =  }",
            Words(Test(at: AtInOtherHours)));

        // A tick apart is another test in the same words: the line says the second and the record holds the tick.
        Assert.Equal(Words(Test(at: At)), Words(Test(at: At.AddTicks(1))));
        Assert.NotEqual(Test(at: At), Test(at: At.AddTicks(1)));

        // The empty name and no name say one line and are two tests.
        Assert.Equal(Words(Test()), Words(Test(environment: string.Empty)));
        Assert.NotEqual(Test(), Test(environment: string.Empty));
    }

    /// <summary>
    /// The test's own words are the viewer's, and the page's are not: a reader of German is told the moment in the
    /// order the reader counts it and the tenths of a second with the comma the reader writes them with -
    /// <c>06.10.2026 08:30:00 +00:00</c> and <c>44,5</c> where the invariant reader is told
    /// <c>10/06/2026 08:30:00 +00:00</c> and <c>44.5</c> - the name of the environment being the one part a reader
    /// does not bend. This is the record's word, which no card shows: the line the page says keeps the point the
    /// delivery file is read with for every reader, and is pinned with it.
    /// </summary>
    [Fact]
    public void TheTestSaysItsMomentAndItsTenthsInTheHoursTheViewerSpeaks()
    {
        var test = Test("uat", At, 44.5);

        Assert.Equal(
            "FailoverTest { Environment = uat, At = 10/06/2026 08:30:00 +00:00, Seconds = 44.5 }",
            Words(test));

        ViewerCulture.German(() =>
        {
            Assert.Equal(
                "FailoverTest { Environment = uat, At = 06.10.2026 08:30:00 +00:00, Seconds = 44,5 }",
                test.ToString());
            Assert.Equal("FailoverTest { Environment = , At = , Seconds = 0,04 }", Test(seconds: 0.04).ToString());
            Assert.Equal("FailoverTest { Environment = system, At = , Seconds =  }", Test(environment: "system").ToString());
        });

        // Under the reader the test happens to run as, the words are the words the invariant reader is told.
        Assert.Equal(Words(test), test.ToString());
    }

    // ------------------------------------------------------------------------ the record's own shape

    /// <summary>
    /// The test's whole public surface, as it stands: the three parts named in the order the constructor names
    /// them - the environment, the moment, the seconds - and the members every record is asked, falling apart,
    /// comparing, hashing and saying itself, with the <c>==</c> and the <c>!=</c> a caller writes. All three parts
    /// are named and none of them has a default: a fourth part, or one turned optional, has to be written here as
    /// well, which is the point - the card cannot be shown a part nobody decided what it means.
    /// </summary>
    [Fact]
    public void TheTestHasItsThreePartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Environment", "At", "Seconds"],
            [.. typeof(FailoverTest)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["At", "Deconstruct", "Environment", "Equals", "GetHashCode", "Seconds", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());

        var properties = typeof(FailoverTest)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(string), properties["Environment"].PropertyType);
        Assert.Equal(typeof(DateTimeOffset?), properties["At"].PropertyType);
        Assert.Equal(typeof(double?), properties["Seconds"].PropertyType);

        foreach (var property in properties.Values)
        {
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
        }

        // Three parts, and the one constructor that takes them in that order, none of them left out by default.
        var constructors = typeof(FailoverTest).GetConstructors();

        Assert.Single(constructors);
        var parameters = constructors[0].GetParameters();

        Assert.Equal(["Environment", "At", "Seconds"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal(typeof(string), parameters[0].ParameterType);
        Assert.Equal(typeof(DateTimeOffset?), parameters[1].ParameterType);
        Assert.Equal(typeof(double?), parameters[2].ParameterType);
        Assert.All(parameters, parameter => Assert.False(parameter.HasDefaultValue));
    }

    // ---------------------------------------------------------------------------------- the machinery

    /// <summary>The public members the test declares itself: its parts and the methods that are not the get and the
    /// set of one - the two operators kept although the compiler names them specially, since they are the <c>==</c>
    /// and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The constructor is
    /// no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(FailoverTest)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words a test says, taken in the invariant culture whatever the reader of this test happens to
    /// be: the moment and the tenths of a second are the reader's to say, and the record's own words are only
    /// comparable one way.</summary>
    private static string Words(FailoverTest test) => ViewerCulture.Invariant(() => test.ToString()!);
}
