using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ComplexityFacts" />: the measured cyclomatic
/// complexity of a build's methods in the three parts the record keeps of it - the average the methods make,
/// the worst of them and the count of the methods they are of - each part said when the build measured it and
/// left unsaid when it did not. <see cref="BuildInfoTests" /> pins the three parts said at once, read from the
/// build endpoint's answer, the none said when the answer has no complexity at all, and the words the card
/// says the said parts in, and <see cref="HealthHelperCoverageTests" /> pins the words of the facts the
/// dashboard's own build reports; what they leave open is pinned here, and only that: the parts at the ends
/// the numbers run to, the zero and the sign and the not-a-number the record keeps the reader would never let
/// through, the words the record says the parts in, the parts that make two records one record or two, the
/// falling apart and the copy, and the facts a build file says at the ends of its numbers, or not at all.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every number is one the test brings.
/// </para>
/// </summary>
public class ComplexityFactsEdgeCaseTests
{
    // ----- the record's parts and the values they stand on

    /// <summary>The facts keep the very parts they are given, each as the value it stands on - the average as
    /// the number it is, the worst as the count it is, the methods as the count it is - and the part left
    /// unsaid stands the nothing it is, kept rather than made a zero.</summary>
    [Fact]
    public void TheFactsKeepThePartsTheyAreGiven()
    {
        var facts = new ComplexityFacts(1.9, 34, 5210);

        Assert.Equal(1.9, facts.Average);
        Assert.Equal(34, facts.Max);
        Assert.Equal(5210, facts.Methods);

        var worstOnly = new ComplexityFacts(null, 34, null);
        Assert.Null(worstOnly.Average);
        Assert.Equal(34, worstOnly.Max);
        Assert.Null(worstOnly.Methods);

        var averageOnly = new ComplexityFacts(2.5, null, null);
        Assert.Equal(2.5, averageOnly.Average);
        Assert.Null(averageOnly.Max);
        Assert.Null(averageOnly.Methods);

        var methodsOnly = new ComplexityFacts(null, null, 1024);
        Assert.Null(methodsOnly.Average);
        Assert.Null(methodsOnly.Max);
        Assert.Equal(1024, methodsOnly.Methods);
    }

    /// <summary>The two counts stand at the two ends of the count and at the zero that is a count, and the
    /// average stands at the two ends of the number it is, above them and below them, keeping the number it is
    /// given the sign and all: the record is not the reader of a build file, which would never let the negative
    /// of them through.</summary>
    [Theory]
    [InlineData(0.0, 0, 0)]
    [InlineData(0.1, 1, 1)]
    [InlineData(double.MaxValue, int.MaxValue, int.MaxValue)]
    [InlineData(double.MinValue, int.MinValue, int.MinValue)]
    [InlineData(-1.5, -34, -1)]
    public void ThePartsStandAtTheEndsOfTheNumbersTheyRunTo(double average, int max, int methods)
    {
        var facts = new ComplexityFacts(average, max, methods);

        Assert.Equal(average, facts.Average);
        Assert.Equal(max, facts.Max);
        Assert.Equal(methods, facts.Methods);
    }

    /// <summary>The average is a number the record keeps the not-a-number and the endless as well, beside the
    /// zero they are not: the nothing that is not a number stands where it is put, kept, asked and said.</summary>
    [Fact]
    public void TheAverageKeepsTheNotANumberAndTheEndlessTheZeroIsNot()
    {
        var notANumber = new ComplexityFacts(double.NaN, null, null);
        Assert.True(notANumber.Average is { } average && double.IsNaN(average));
        Assert.Null(notANumber.Max);
        Assert.Null(notANumber.Methods);

        var endless = new ComplexityFacts(double.PositiveInfinity, 34, null);
        Assert.True(endless.Average is { } endlessAverage && double.IsPositiveInfinity(endlessAverage));
        Assert.Equal(34, endless.Max);
        Assert.Null(endless.Methods);

        var neverEnding = new ComplexityFacts(double.NegativeInfinity, 0, 0);
        Assert.True(neverEnding.Average is { } neverEndingAverage && double.IsNegativeInfinity(neverEndingAverage));
        Assert.Equal(0, neverEnding.Max);
        Assert.Equal(0, neverEnding.Methods);
    }

    // ----- the words the record says its parts in

    /// <summary>The words the record says its parts in: its own name at the front, the parts in the order they
    /// are named, each as the number it stands on - the average as the number it is, at the ends of the number
    /// it runs to and with the sign it carries, the two counts as the plain numbers they are - and the part
    /// left unsaid said as nothing.</summary>
    [Fact]
    public void TheFactsSayTheirPartsInTheWordsTheRecordSaysThem()
    {
        var culture = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            Assert.Equal("ComplexityFacts { Average = 1.9, Max = 34, Methods = 5210 }", new ComplexityFacts(1.9, 34, 5210).ToString());
            Assert.Equal("ComplexityFacts { Average = 1, Max = 4, Methods = 2 }", new ComplexityFacts(1.0, 4, 2).ToString());
            Assert.Equal("ComplexityFacts { Average = 0, Max = 0, Methods = 0 }", new ComplexityFacts(0.0, 0, 0).ToString());
            Assert.Equal("ComplexityFacts { Average = 1.7976931348623157E+308, Max = 2147483647, Methods = 2147483647 }", new ComplexityFacts(double.MaxValue, int.MaxValue, int.MaxValue).ToString());
            Assert.Equal("ComplexityFacts { Average = -1.7976931348623157E+308, Max = -2147483648, Methods = -2147483648 }", new ComplexityFacts(double.MinValue, int.MinValue, int.MinValue).ToString());
            Assert.Equal("ComplexityFacts { Average = NaN, Max = , Methods =  }", new ComplexityFacts(double.NaN, null, null).ToString());
            Assert.Equal("ComplexityFacts { Average = , Max = 34, Methods =  }", new ComplexityFacts(null, 34, null).ToString());
            Assert.Equal("ComplexityFacts { Average = , Max = , Methods =  }", new ComplexityFacts(null, null, null).ToString());
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = culture;
        }
    }

    /// <summary>The zero and the negative zero the average may stand on are two words the record says and one
    /// record it is: the words differ in the sign they carry, and the records are one the way the numbers are
    /// one, the hash following them.</summary>
    [Fact]
    public void TheZeroAndTheNegativeZeroAreTwoWordsAndOneRecord()
    {
        var culture = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            var zero = new ComplexityFacts(0.0, 34, 5210);
            var negativeZero = new ComplexityFacts(-0.0, 34, 5210);

            Assert.Equal("ComplexityFacts { Average = 0, Max = 34, Methods = 5210 }", zero.ToString());
            Assert.Equal("ComplexityFacts { Average = -0, Max = 34, Methods = 5210 }", negativeZero.ToString());
            Assert.Equal(zero, negativeZero);
            Assert.Equal(zero.GetHashCode(), negativeZero.GetHashCode());
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = culture;
        }
    }

    // ----- the records that are one

    /// <summary>Two facts are one while their three parts are - the nothing among them included - and the two
    /// are not the one reference they are one with: the equality of the record, of the object and of the
    /// operators, the hash that follows them, and the set that keeps one entry for the facts that are one.</summary>
    [Fact]
    public void TwoFactsAreOneWhileTheirPartsAre()
    {
        var one = new ComplexityFacts(1.9, 34, 5210);
        var two = new ComplexityFacts(1.9, 34, 5210);

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.False(one != two);
        Assert.True(one.Equals(two as object));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.NotSame(one, two);
        Assert.True(one.Equals(one));

        var none = new ComplexityFacts(null, null, null);
        Assert.Equal(none, new ComplexityFacts(null, null, null));
        Assert.Equal(none.GetHashCode(), new ComplexityFacts(null, null, null).GetHashCode());

        Assert.Single(new HashSet<ComplexityFacts> { one, two, new ComplexityFacts(1.9, 34, 5210) });
        Assert.Equal(2, new HashSet<ComplexityFacts> { one, new ComplexityFacts(1.9, 35, 5210) }.Count);
    }

    /// <summary>The not-a-number the average may stand on is one with the not-a-number and not with the number:
    /// the way the numbers are one, the records are one and the hash follows them, and the number beside them
    /// is another record they are not one with.</summary>
    [Fact]
    public void TheNotANumberIsOneWithTheNotANumberAndNotWithANumber()
    {
        var one = new ComplexityFacts(double.NaN, 34, 5210);
        var two = new ComplexityFacts(double.NaN, 34, 5210);

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.NotEqual(one, new ComplexityFacts(1.9, 34, 5210));
    }

    /// <summary>A difference in any one of the three parts is another record: the average turned by a hair,
    /// turned to the zero it was not, turned about in sign or turned to the nothing it was not; the worst or
    /// the methods turned by one, turned to the zero the nothing is not, turned about in sign or turned to the
    /// nothing the zero is not - each another record, the operators saying it and the hash following.</summary>
    [Fact]
    public void APartTurnedIsAnotherRecord()
    {
        var one = new ComplexityFacts(1.9, 34, 5210);

        Assert.NotEqual(one, new ComplexityFacts(1.91, 34, 5210));
        Assert.NotEqual(one, new ComplexityFacts(0.0, 34, 5210));
        Assert.NotEqual(one, new ComplexityFacts(-1.9, 34, 5210));
        Assert.NotEqual(one, new ComplexityFacts(null, 34, 5210));
        Assert.NotEqual(one, new ComplexityFacts(1.9, 35, 5210));
        Assert.NotEqual(one, new ComplexityFacts(1.9, 0, 5210));
        Assert.NotEqual(one, new ComplexityFacts(1.9, -34, 5210));
        Assert.NotEqual(one, new ComplexityFacts(1.9, null, 5210));
        Assert.NotEqual(one, new ComplexityFacts(1.9, 34, 5211));
        Assert.NotEqual(one, new ComplexityFacts(1.9, 34, 0));
        Assert.NotEqual(one, new ComplexityFacts(1.9, 34, null));
        Assert.NotEqual(one, new ComplexityFacts(null, null, null));

        Assert.True(one != new ComplexityFacts(1.9, 34, null));
        Assert.False(one == new ComplexityFacts(1.9, 34, null));
        Assert.Equal(3, new HashSet<ComplexityFacts> { one, one with { Methods = null }, one with { Methods = 0 } }.Count);
    }

    /// <summary>Facts that are not there are no facts: the record's default is no facts, and facts are never
    /// equal to no facts however the question is asked - the operators, and <c>Equals</c> asked about the no
    /// facts and about a word it is not - while the facts that are there are facts to both of them.</summary>
    [Fact]
    public void FactsThatAreNotThereAreNoFacts()
    {
        Assert.Null(default(ComplexityFacts));

        ComplexityFacts? none = null;
        var there = new ComplexityFacts(0.0, 0, 0);

        Assert.False(none == there);
        Assert.True(none != there);
        Assert.False(there.Equals(none));
        Assert.False(there == none);
        Assert.True(there != none);
        Assert.False(there.Equals(null));
        Assert.False(there.Equals("ComplexityFacts"));
        Assert.True(there is not null);
    }

    // ----- the parts the record falls apart into

    /// <summary>The record falls apart into its three parts, in the order it names them and each as the value
    /// it stands on - the number and the two counts - and the record that says nothing of its parts falls
    /// apart into the nothing, the nothing and the nothing, and the record that says the zero of them all
    /// falls apart into the zero, the zero and the zero.</summary>
    [Fact]
    public void TheFactsFallApartIntoTheirParts()
    {
        var (average, max, methods) = new ComplexityFacts(1.9, 34, 5210);

        Assert.Equal(1.9, average);
        Assert.Equal(34, max);
        Assert.Equal(5210, methods);

        var (noAverage, noMax, noMethods) = new ComplexityFacts(null, null, null);

        Assert.Null(noAverage);
        Assert.Null(noMax);
        Assert.Null(noMethods);

        var (zeroAverage, zeroMax, zeroMethods) = new ComplexityFacts(0.0, 0, 0);

        Assert.Equal(0.0, zeroAverage);
        Assert.Equal(0, zeroMax);
        Assert.Equal(0, zeroMethods);
    }

    // ----- the copies

    /// <summary>The copy that turns nothing is the record again though not the record itself, and the copy
    /// that turns one of the three parts keeps the two it does not name: the average turned alone, the worst
    /// turned alone, the methods turned to the nothing and from it - each a copy of the rest.</summary>
    [Fact]
    public void ACopyThatTurnsOnePartKeepsTheOtherTwo()
    {
        var one = new ComplexityFacts(1.9, 34, 5210);

        var copy = one with { };
        Assert.Equal(one, copy);
        Assert.NotSame(one, copy);

        var averaged = one with { Average = 2.5 };
        Assert.Equal(2.5, averaged.Average);
        Assert.Equal(34, averaged.Max);
        Assert.Equal(5210, averaged.Methods);
        Assert.NotEqual(one, averaged);

        var worsened = one with { Max = 35 };
        Assert.Equal(35, worsened.Max);
        Assert.Equal(1.9, worsened.Average);
        Assert.Equal(5210, worsened.Methods);

        var counted = new ComplexityFacts(1.9, null, null) with { Methods = 1024 };
        Assert.Equal(1024, counted.Methods);
        Assert.Equal(1.9, counted.Average);
        Assert.Null(counted.Max);

        var uncounted = one with { Methods = null };
        Assert.Null(uncounted.Methods);
        Assert.Equal(1.9, uncounted.Average);
        Assert.Equal(34, uncounted.Max);
        Assert.NotEqual(one, uncounted);
    }

    // ----- the facts a build file says

    /// <summary>The zero the counts run to and the zero the number runs to are facts a build file says: each
    /// of the three said as its zero is the record the zeros stand for and not the no facts, and the build
    /// the answer holds is a build of them.</summary>
    [Fact]
    public void TheZerosTheNumbersRunToAreFactsTheFileSays()
    {
        var build = BuildInfo.Parse("""{ "complexity": { "average": 0, "max": 0, "methods": 0 } }""");

        Assert.Equal(new ComplexityFacts(0.0, 0, 0), build!.Complexity);
    }

    /// <summary>The worst and the methods stand at the top of the count the counts run to, and the one past it
    /// the count does not reach: the top of the count said for both is the top, the one past it said as the
    /// nothing the count is not, and the average beside them stands at the top of the number it runs to.</summary>
    [Fact]
    public void TheCountsStandAtTheTopOfTheCountAndNotOnePastIt()
    {
        var atTheTop = BuildInfo.Parse("""{ "complexity": { "average": 1.9, "max": 2147483647, "methods": 2147483647 } }""");
        Assert.Equal(new ComplexityFacts(1.9, int.MaxValue, int.MaxValue), atTheTop!.Complexity);

        var onePast = BuildInfo.Parse("""{ "complexity": { "average": 1.9, "max": 2147483648, "methods": 2147483648 } }""");
        Assert.Equal(new ComplexityFacts(1.9, null, null), onePast!.Complexity);

        var averageAtTheTop = BuildInfo.Parse("""{ "complexity": { "average": 1.7976931348623157E+308, "max": 34, "methods": 5210 } }""");
        Assert.Equal(new ComplexityFacts(double.MaxValue, 34, 5210), averageAtTheTop!.Complexity);
    }

    /// <summary>The count the file says as a number that is not a whole count is counted whole, the way the
    /// number is counted whole: the count above the half between two whole counts counted to the next whole
    /// above, and the count at the half between two whole counts counted to the even whole between them.</summary>
    [Fact]
    public void ACountSaidAsANumberThatIsNotWholeIsCountedWhole()
    {
        var build = BuildInfo.Parse("""{ "complexity": { "average": 1, "max": 3.6, "methods": 2.5 } }""");

        Assert.Equal(new ComplexityFacts(1.0, 4, 2), build!.Complexity);
    }

    /// <summary>The part the file does not say as a number is no part: the number that is negative, the number
    /// that is a word, the nothing said as a number, each left unsaid - all three of them left unsaid saying
    /// no facts, the version beside them keeping the build there, and the one of them left unsaid the other
    /// two said making the record the two said stand for.</summary>
    [Fact]
    public void APartTheFileDoesNotSayAsANumberIsNoPart()
    {
        var negative = BuildInfo.Parse("""{ "version": "1.0.0", "complexity": { "average": -1.5, "max": -34, "methods": -1 } }""");
        Assert.Equal("1.0.0", negative!.Version);
        Assert.Null(negative.Complexity);

        var word = BuildInfo.Parse("""{ "version": "1.0.0", "complexity": { "average": "1.9", "max": "34", "methods": "5210" } }""");
        Assert.Null(word!.Complexity);

        var saidAndNotSaid = BuildInfo.Parse("""{ "complexity": { "average": 1.9, "max": "34", "methods": null } }""");
        Assert.Equal(new ComplexityFacts(1.9, null, null), saidAndNotSaid!.Complexity);
    }

    /// <summary>The section that says nothing says no facts: the section that is the empty object, that is the
    /// nothing said as a section, that is a word in the place of the section - each the facts left unsaid, the
    /// version beside them keeping the build there for the facts to be read out of.</summary>
    [Fact]
    public void ASectionThatSaysNothingSaysNoFacts()
    {
        var empty = BuildInfo.Parse("""{ "version": "1.0.0", "complexity": { } }""");
        Assert.Null(empty!.Complexity);

        var nothing = BuildInfo.Parse("""{ "version": "1.0.0", "complexity": null }""");
        Assert.Null(nothing!.Complexity);

        var word = BuildInfo.Parse("""{ "version": "1.0.0", "complexity": "none" }""");
        Assert.Null(word!.Complexity);
    }

    /// <summary>The facts that are not said and nothing else said are no build at all: the endpoint's answer
    /// that holds the complexity's place and says none of it, and says nothing else, is the no build the
    /// answer that says nothing is, while the answer that says one of the facts is a build of it.</summary>
    [Fact]
    public void ASectionThatSaysNothingAndNothingElseSaidIsNoBuild()
    {
        Assert.Null(BuildInfo.Parse("""{ "complexity": { } }"""));
        Assert.Null(BuildInfo.Parse("""{ "complexity": { "average": null, "max": null, "methods": null } }"""));

        var build = BuildInfo.Parse("""{ "complexity": { "methods": 1024 } }""");
        Assert.Equal(new ComplexityFacts(null, null, 1024), build!.Complexity);
    }
}
