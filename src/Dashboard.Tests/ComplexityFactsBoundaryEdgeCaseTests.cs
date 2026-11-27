using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="ComplexityFacts" /> again, from the side its two files leave open.
/// <see cref="ComplexityFactsEdgeCaseTests" /> pins the three parts at the ends of the number and the count they run
/// to, the not-a-number and the endless, the words they are said in when nobody's culture stands in the way, the
/// equality, the hash and the operators, the falling apart and the <c>with</c>; <see cref="ComplexityFactsReadEdgeCaseTests" />
/// pins the read path - the overflow, the underflow, the rounding at the top of the count and at the half, the
/// exponent, the negative zero, the section that is no section and the part named in another case; and
/// <see cref="BuildInfoTests" /> pins the sample's <c>1.9</c>, <c>34</c> and <c>5210</c> read from the build file the
/// dashboard serves, the nothing where an answer holds no complexity, and the card's line, which
/// <see cref="BuildTextEdgeCaseTests" /> pins part by part and <see cref="HealthHelperCoverageTests" /> pins as the
/// card's own words.
/// <para>
/// What none of them pins, and this file does, is the rest of the number the average stands on: the smallest positive
/// double and the largest that is not the endless, the last digit of the number and the neighbour one hair above
/// <c>1.9</c> - one average written two ways being one record, its neighbour being another. Then the three parts
/// turned as the record is made, which is the <c>init</c> of the properties rather than the <c>with</c> of a copy;
/// the culture the record's own words follow while the card's words refuse to; the facts being no other record of the
/// build however it is numbered, and the comparer and the dictionary key that are what the build's equality of a
/// section really rests on; and the parts standing where a build file says them however the file says them - in
/// another order, with a part of its own beside them, over several lines, or as a whole number without a point.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: <see cref="ComplexityFacts" /> holds no moment,
/// calls nothing, and every number is one the test brings or one it reads from an answer of its own making.
/// </para>
/// </summary>
public class ComplexityFactsBoundaryEdgeCaseTests
{
    // ----- the number the average stands on

    /// <summary>The average stands at the numbers of the number line the first file leaves open, and is kept as it
    /// is put there: the smallest positive number a double holds, the least normal one, a millionth and a fortieth,
    /// the last digit of the number, a whole number said without a point, a thousand and a half, the largest number
    /// short of the endless and the largest there is - the record keeping the sign and the exponent of what it is
    /// given, and keeping the nothing of an unsaid part unsaid beside them.</summary>
    [Theory]
    [InlineData(double.Epsilon)]
    [InlineData(2.2250738585072014E-308)]
    [InlineData(1e-06)]
    [InlineData(0.04)]
    [InlineData(1.94)]
    [InlineData(81.234567)]
    [InlineData(2d)]
    [InlineData(-1234.5)]
    [InlineData(1E+300)]
    [InlineData(1.7976931348623157E+308)]
    public void TheAverageStandsAtTheNumbersTheFirstFileLeavesOpen(double average)
    {
        var facts = new ComplexityFacts(average, null, null);

        Assert.NotNull(facts.Average);
        Assert.Equal(average, facts.Average);
        Assert.Null(facts.Max);
        Assert.Null(facts.Methods);
        Assert.Equal(new ComplexityFacts(average, null, null), facts);
    }

    /// <summary>The words the record says of the average carry the number to the last digit of it, in the figures
    /// the invariant reader writes - the smallest positive number as <c>5E-324</c>, the millionth as <c>1E-06</c>,
    /// the largest as <c>1E+300</c>, the sum of a tenth and a fifth in all seventeen of its digits, the whole number
    /// without a point, and the neighbour of <c>1.9</c> told from it by the digit at the end. The two counts are the
    /// plain integers they are, the sign keeps its place in front of the least of them, and the part nobody said
    /// leaves its place between the mark and the comma empty. The card, said the same moment over the same record,
    /// rounds the average away: the record keeps what the build measured, the card writes what a reader reads.</summary>
    [Fact]
    public void TheFactsSayTheAverageToTheLastDigitOfTheNumberItStandsOn()
    {
        Assert.Equal("ComplexityFacts { Average = 5E-324, Max = , Methods =  }", Words(new ComplexityFacts(double.Epsilon, null, null)));
        Assert.Equal("ComplexityFacts { Average = 1E-06, Max = , Methods =  }", Words(new ComplexityFacts(1e-06, null, null)));
        Assert.Equal("ComplexityFacts { Average = 1E+300, Max = , Methods =  }", Words(new ComplexityFacts(1E+300, null, null)));
        Assert.Equal("ComplexityFacts { Average = 0.30000000000000004, Max = 34, Methods = 5210 }", Words(new ComplexityFacts(0.1 + 0.2, 34, 5210)));
        Assert.Equal("ComplexityFacts { Average = 0.04, Max = , Methods =  }", Words(new ComplexityFacts(0.04, null, null)));
        Assert.Equal("ComplexityFacts { Average = 1.9000000000000001, Max = 34, Methods = 5210 }", Words(new ComplexityFacts(double.BitIncrement(1.9), 34, 5210)));
        Assert.Equal("ComplexityFacts { Average = 81.234567, Max = 34, Methods = 5210 }", Words(new ComplexityFacts(81.234567, 34, 5210)));
        Assert.Equal("ComplexityFacts { Average = 2, Max = 2, Methods = 2 }", Words(new ComplexityFacts(2d, 2, 2)));
        Assert.Equal("ComplexityFacts { Average = -1234.5, Max = -34, Methods = -5210 }", Words(new ComplexityFacts(-1234.5, -34, -5210)));

        // The card cuts the last digit away, and the two digits before it: a fourth of a hundredth of a method
        // complex is said as <c>average 0</c>, which is the card's rounding and not the record losing the number.
        Assert.Equal("average 1.9, worst 34 (5,210 methods)", BuildText.Complexity(new ComplexityFacts(double.BitIncrement(1.9), 34, 5210)));
        Assert.Equal("average 0", BuildText.Complexity(new ComplexityFacts(0.04, null, null)));
        Assert.Equal(0.04, new ComplexityFacts(0.04, null, null).Average);
    }

    /// <summary>One average written two ways is one record: the sum of a tenth and a fifth and the seventeen digits
    /// that name it are one record with one hash and one word, while the tenth of them rounded to a third is a hair
    /// below and therefore another record - and <c>19e-1</c> is the one average again, while the double a hair above
    /// it and the double a hair below it are records of their own.
    /// The record compares the numbers it stands on and does not order them, so the set counts the records apart only
    /// where the double is apart.</summary>
    [Fact]
    public void OneAverageWrittenTwoWaysIsOneRecordAndItsNeighbourIsAnother()
    {
        var summed = new ComplexityFacts(0.1 + 0.2, 34, 5210);
        var written = new ComplexityFacts(0.30000000000000004, 34, 5210);
        var rounded = new ComplexityFacts(0.3, 34, 5210);

        Assert.Equal(summed, written);
        Assert.True(summed == written);
        Assert.Equal(summed.GetHashCode(), written.GetHashCode());
        Assert.Equal(Words(summed), Words(written));
        Assert.NotEqual(summed, rounded);
        Assert.True(summed != rounded);
        Assert.Equal(2, new HashSet<ComplexityFacts> { summed, written, rounded }.Count);

        var measured = new ComplexityFacts(1.9, 34, 5210);
        var inTenths = new ComplexityFacts(19e-1, 34, 5210);
        var above = new ComplexityFacts(double.BitIncrement(1.9), 34, 5210);
        var below = new ComplexityFacts(double.BitDecrement(1.9), 34, 5210);

        Assert.Equal(measured, inTenths);
        Assert.NotEqual(measured, above);
        Assert.NotEqual(measured, below);
        Assert.NotEqual(Words(measured), Words(above));
        Assert.Equal(3, new HashSet<ComplexityFacts> { measured, inTenths, above, below }.Count);
    }

    // ----- the parts turned as the record is made

    /// <summary>Each part may be turned as the facts are made, and stands where it is turned to - to another number,
    /// to the zero that is a count and to the nothing - while the parts the turn does not name stay the numbers the
    /// record was given. A record made that way is another record than the one it would have stood as, and never the
    /// one reference; and the record made by turning every part to what another record already stands on is that
    /// other record, though still not itself.</summary>
    [Fact]
    public void APartTurnedAsTheFactsAreMadeStandsWhereItIsTurnedTo()
    {
        var measured = new ComplexityFacts(1.9, 34, 5210);

        var averaged = new ComplexityFacts(1.9, 34, 5210) { Average = 2.5 };

        Assert.Equal(2.5, averaged.Average);
        Assert.Equal(34, averaged.Max);
        Assert.Equal(5210, averaged.Methods);
        Assert.NotEqual(measured, averaged);
        Assert.NotSame(measured, averaged);

        var unsaid = new ComplexityFacts(1.9, 34, 5210) { Max = null, Methods = null };

        Assert.Equal(1.9, unsaid.Average);
        Assert.Null(unsaid.Max);
        Assert.Null(unsaid.Methods);
        Assert.Equal(new ComplexityFacts(1.9, null, null), unsaid);
        Assert.NotEqual(measured, unsaid);

        var zeroed = new ComplexityFacts(null, null, null) { Average = 0.0, Max = 0, Methods = 0 };

        Assert.Equal(new ComplexityFacts(0.0, 0, 0), zeroed);
        Assert.NotEqual(new ComplexityFacts(null, null, null), zeroed);
        Assert.Equal("ComplexityFacts { Average = 0, Max = 0, Methods = 0 }", Words(zeroed));

        var retyped = new ComplexityFacts(null, 0, null) { Average = measured.Average, Max = measured.Max, Methods = measured.Methods };

        Assert.Equal(measured, retyped);
        Assert.NotSame(measured, retyped);
        Assert.Equal(measured.GetHashCode(), retyped.GetHashCode());
    }

    // ----- the culture the words follow

    /// <summary>The words the record says of itself follow whoever reads them, which is what parts them from the
    /// card's: a reader of German is told <c>1234,5</c> by the record and <c>average 1234.5</c> by the page, whose
    /// line is written for the build and not for whoever looks - the same fact, said twice, once to the reader and
    /// once to the dashboard. Turn the reader back and the record says the page's figures again.</summary>
    [Fact]
    public void TheFactsSayTheirWordsInTheReadersOwnCultureWhileTheCardSaysItsOwn()
    {
        var facts = new ComplexityFacts(1234.5, 34, 5210);

        ViewerCulture.German(() =>
        {
            Assert.Equal("ComplexityFacts { Average = 1234,5, Max = 34, Methods = 5210 }", facts.ToString());
            Assert.Equal("average 1234.5, worst 34 (5,210 methods)", BuildText.Complexity(facts));
        });

        Assert.Equal("ComplexityFacts { Average = 1234.5, Max = 34, Methods = 5210 }", Words(facts));

        // The counts take no separator from the reader either way: they are integers in both mouths.
        ViewerCulture.German(() => Assert.Equal("ComplexityFacts { Average = , Max = 5210, Methods =  }", new ComplexityFacts(null, 5210, null).ToString()));
    }

    // ----- what the facts are one with, and what they are not

    /// <summary>The facts are a value, and the build's equality of a section rests on it: the record answers
    /// <see cref="IEquatable{T}" />, so the default comparer, the hash and a dictionary keyed by the facts all see
    /// two measurements of the same three numbers as one key - and none of them sees the same three numbers of
    /// another record, or the parts turned to nothing, or no facts at all. That the type is sealed is part of the
    /// same promise: nothing underneath can be given an equality of its own and let a build compare unequal to
    /// itself.</summary>
    [Fact]
    public void TheFactsAreOneWithThemselvesAndNotWithTheOtherRecordsOfTheBuild()
    {
        var facts = new ComplexityFacts(1.9, 34, 5210);

        Assert.True(typeof(ComplexityFacts).IsSealed);
        Assert.IsAssignableFrom<IEquatable<ComplexityFacts>>(facts);
        Assert.True(facts.Equals(new ComplexityFacts(1.9, 34, 5210)));

        var measured = new Dictionary<ComplexityFacts, string> { [facts] = "measured" };

        Assert.Equal("measured", measured[new ComplexityFacts(1.9, 34, 5210)]);
        Assert.Equal("measured", measured[new ComplexityFacts(19e-1, 34, 5_210)]);
        Assert.False(measured.ContainsKey(new ComplexityFacts(1.9, 34, 5211)));
        Assert.False(measured.ContainsKey(new ComplexityFacts(null, 34, 5210)));

        // Another record of the same build, wearing the same numbers or a few of them, is not these facts.
        Assert.False(facts.Equals((object)new CrapFacts(1.9, 34, 5210)));
        Assert.False(facts.Equals((object)new CoverageFacts(1.9, 34)));
        Assert.False(facts.Equals((object)new TestCounts(1, 34, 5210)));
        Assert.False(facts.Equals((object)1.9));
        Assert.False(facts.Equals((object)34));
        Assert.False(facts.Equals((object)"average 1.9"));
        Assert.True(facts.Equals((object)facts));

        // The no facts that is no record at all is a key of its own: the comparer says two of them are one, and
        // hashes the nothing to zero rather than asking it for its parts - which is how a build that measured no
        // complexity still compares equal to a build that measured no complexity.
        Assert.True(EqualityComparer<ComplexityFacts>.Default.Equals(null, null));
        Assert.False(EqualityComparer<ComplexityFacts>.Default.Equals(facts, null));
        Assert.Equal(0, EqualityComparer<ComplexityFacts>.Default.GetHashCode(null!));
        Assert.Equal(
            new ComplexityFacts(null, null, null).GetHashCode(),
            EqualityComparer<ComplexityFacts>.Default.GetHashCode(new ComplexityFacts(null, null, null)));
    }

    // ----- the parts a build file says

    /// <summary>The parts stand where the file says them, however the file says them: named in the other order, with
    /// a part of the file's own beside them that the dashboard knows nothing of, written over several lines, or said
    /// as a whole number without a point - each the record the same three numbers stand for. The last digit of a
    /// double and the smallest number there is survive the answer whole, so a build that measures an average of
    /// <c>1.9000000000000001</c> is not a build that measured <c>1.9</c>.</summary>
    [Fact]
    public void ThePartsStandWhereTheFileSaysThemHoweverTheFileSaysThem()
    {
        var about = ComplexityOf("""{ "methods": 5210, "p95": 40, "max": 34, "average": 1.9 }""");

        Assert.Equal(new ComplexityFacts(1.9, 34, 5210), about);

        var whole = ComplexityOf("""
            {
              "average": 2,
              "max": 2,
              "methods": 2
            }
            """);

        Assert.Equal(new ComplexityFacts(2d, 2, 2), whole);
        Assert.NotEqual(new ComplexityFacts(2d, 2, 2), ComplexityOf("""{ "average": 2, "max": 2, "methods": 3 }"""));

        var lastDigit = ComplexityOf("""{ "average": 1.9000000000000001 }""");

        Assert.Equal(1.9000000000000001, lastDigit!.Average);
        Assert.NotEqual(new ComplexityFacts(1.9, null, null), lastDigit);

        var least = ComplexityOf("""{ "average": 5e-324 }""");

        Assert.Equal(new ComplexityFacts(double.Epsilon, null, null), least);

        // The file that says a part as nothing says no part, and the file that says none of them says no facts.
        Assert.Equal(new ComplexityFacts(null, 34, null), ComplexityOf("""{ "average": null, "max": 34, "methods": null }"""));
        Assert.Null(ComplexityOf("{ }"));
    }

    /// <summary>The complexity of the build file the dashboard serves, as the very record it stands for: the three
    /// numbers the file writes, the parts they are kept in, the record the file's build says inside its own words,
    /// the parts the record falls apart into, the words it says them in and the set that holds them once beside the
    /// record a test makes of the same numbers - every one of them a fixed value of that file rather than a number
    /// this test made up.</summary>
    [Fact]
    public void TheComplexityOfTheBuildFileTheDashboardServesIsTheRecordItStandsFor()
    {
        var build = BuildInfo.Parse(Optics.Build)!;
        var facts = build.Complexity!;

        Assert.Equal(new ComplexityFacts(1.9, 34, 5210), facts);
        Assert.Equal(1.9, facts.Average);
        Assert.Equal(34, facts.Max);
        Assert.Equal(5210, facts.Methods);

        var (average, max, methods) = facts;

        Assert.Equal(1.9, average);
        Assert.Equal(34, max);
        Assert.Equal(5210, methods);

        Assert.Equal("ComplexityFacts { Average = 1.9, Max = 34, Methods = 5210 }", Words(facts));
        Assert.Single(new HashSet<ComplexityFacts> { facts, new(1.9, 34, 5_210) });

        // The build says its sections in the record's own words: the facts are held by name, not flattened away.
        Assert.Contains("Complexity = ComplexityFacts { Average = 1.9, Max = 34, Methods = 5210 }", build.ToString(), StringComparison.Ordinal);
        Assert.Contains(
            "Complexity = ComplexityFacts { Average = , Max = , Methods =  }",
            (build with { Complexity = new ComplexityFacts(null, null, null) }).ToString(),
            StringComparison.Ordinal);

        // Turning one part off leaves the two the file says, and the line the card makes of them.
        Assert.Equal("average 1.9, worst 34", BuildText.Complexity(facts with { Methods = null }));
    }

    // ----- the machinery

    /// <summary>The complexity one answer reports, as the dashboard reads it, the section being the text the test brings.</summary>
    private static ComplexityFacts? ComplexityOf(string complexity) => BuildAnswers.Of("complexity", complexity).Complexity;

    /// <summary>The words a record says its three parts in, taken in the invariant culture whoever reads this test
    /// happens to stand in: the record's own words follow the reader, which is pinned on its own above, so the
    /// strings here are the point and the culture only the way to say them once.</summary>
    private static string Words(ComplexityFacts complexity) => ViewerCulture.Invariant(() => complexity.ToString()!);
}
