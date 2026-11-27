namespace Dashboard.Tests;

/// <summary>
/// The read-path boundaries of <see cref="ComplexityFacts" /> left open by <see cref="ComplexityFactsEdgeCaseTests" />:
/// the average past the end of the number, the counts that round across the top of the count, the counts that round
/// up from the half, the exponent and the negative zero a build file may write, the section that is a list, the part
/// that is true or false, and the part named in another case. Every answer is a fixed text the test brings; nothing
/// here reaches the network or the clock.
/// </summary>
public class ComplexityFactsReadEdgeCaseTests
{
    /// <summary>An average past the end of the number reads as endless, which is no average; the two counts beside it
    /// are kept.</summary>
    [Fact]
    public void AnAverageThatOverflowsIsNoAverage()
    {
        Assert.Equal(new ComplexityFacts(null, 34, 5210), ComplexityOf("""{ "average": 1e400, "max": 34, "methods": 5210 }"""));
    }

    /// <summary>An average too small for the number reads as the zero it rounds to, and the zero is an average.</summary>
    [Fact]
    public void AnAverageThatUnderflowsIsZero()
    {
        Assert.Equal(new ComplexityFacts(0.0, 34, null), ComplexityOf("""{ "average": 1e-400, "max": 34 }"""));
    }

    /// <summary>A count is checked against the top of the count before it is rounded: just under the top it rounds up
    /// to the top, and a hair past the top is no count even though it would round down to it.</summary>
    [Theory]
    [InlineData("2147483646.6", int.MaxValue)]
    [InlineData("2147483647", int.MaxValue)]
    [InlineData("2147483647.4", null)]
    [InlineData("2147483647.5", null)]
    public void ACountIsCheckedAgainstTheTopBeforeItIsRounded(string count, int? expected)
    {
        var facts = ComplexityOf($$"""{ "average": 1.9, "max": {{count}}, "methods": {{count}} }""");

        Assert.Equal(new ComplexityFacts(1.9, expected, expected), facts);
    }

    /// <summary>A count said at the half rounds away from zero, up to the next whole on both sides of it (#1328), and below the half rounds to
    /// the zero that is a count.</summary>
    [Theory]
    [InlineData("0.4", 0)]
    [InlineData("0.5", 1)]
    [InlineData("1.5", 2)]
    [InlineData("33.5", 34)]
    [InlineData("34.5", 35)]
    public void ACountAtTheHalfRoundsUpToTheNextWhole(string count, int expected)
    {
        Assert.Equal(new ComplexityFacts(null, expected, expected), ComplexityOf($$"""{ "max": {{count}}, "methods": {{count}} }"""));
    }

    /// <summary>The numbers may be written with an exponent: they read as the numbers they write.</summary>
    [Fact]
    public void NumbersWrittenWithAnExponentReadAsTheyWrite()
    {
        Assert.Equal(new ComplexityFacts(1.9, 34, 5210), ComplexityOf("""{ "average": 19e-1, "max": 3.4E1, "methods": 5.21e+3 }"""));
    }

    /// <summary>The negative zero a file may write is not negative: it is kept, and it is the record of the zero.</summary>
    [Fact]
    public void TheNegativeZeroIsKeptAsTheZero()
    {
        var facts = ComplexityOf("""{ "average": -0, "max": -0, "methods": -0.0 }""");

        Assert.Equal(new ComplexityFacts(0.0, 0, 0), facts);
        Assert.Equal(0, facts!.Max);
        Assert.Equal(0, facts.Methods);
    }

    /// <summary>A section that is a list, a number or true is no section, and the facts are not read from it, even when
    /// the list holds a section that would say them.</summary>
    [Theory]
    [InlineData("""[ { "average": 1.9, "max": 34, "methods": 5210 } ]""")]
    [InlineData("[]")]
    [InlineData("34")]
    [InlineData("true")]
    public void ASectionThatIsNotAnObjectIsNoFacts(string section)
    {
        Assert.Null(ComplexityOf(section));
    }

    /// <summary>A part that is true or false, a list or a section is no part, while the part said as a number beside it
    /// is kept; all three said that way are no facts.</summary>
    [Fact]
    public void APartThatIsNotANumberIsNoPart()
    {
        Assert.Equal(new ComplexityFacts(null, 34, null), ComplexityOf("""{ "average": true, "max": 34, "methods": false }"""));
        Assert.Equal(new ComplexityFacts(1.9, null, null), ComplexityOf("""{ "average": 1.9, "max": [34], "methods": { "count": 5210 } }"""));
        Assert.Null(ComplexityOf("""{ "average": false, "max": [], "methods": {} }"""));
    }

    /// <summary>The parts are named in the case the build endpoint writes them: a part named in another case is no part,
    /// so a section that names all three that way is no facts.</summary>
    [Fact]
    public void PartsNamedInAnotherCaseAreNoParts()
    {
        Assert.Null(ComplexityOf("""{ "Average": 1.9, "MAX": 34, "Methods": 5210 }"""));
        Assert.Equal(new ComplexityFacts(null, 34, null), ComplexityOf("""{ "Average": 1.9, "max": 34, "Methods": 5210 }"""));
    }

    /// <summary>The complexity one answer reports, as the dashboard reads it.</summary>
    private static ComplexityFacts? ComplexityOf(string complexity) => BuildAnswers.Of("complexity", complexity).Complexity;
}
