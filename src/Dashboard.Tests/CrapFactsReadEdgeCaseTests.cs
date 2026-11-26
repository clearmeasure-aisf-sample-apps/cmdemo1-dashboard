namespace Dashboard.Tests;

/// <summary>
/// The read-path boundaries of <see cref="CrapFacts" /> left open by <see cref="CrapFactsEdgeCaseTests" /> and
/// <see cref="BuildInfoTests" />: each part said alone, the score kept to its fraction while the count is rounded, the
/// numbers that are negative, past the end of the number or too small for it, the count across the top of the count
/// and at the half, the negative zero, the parts said as text, the section that is not an object, and the part named
/// in another case. Every answer is a fixed text the test brings; nothing here reaches the network or the clock.
/// </summary>
public class CrapFactsReadEdgeCaseTests
{
    /// <summary>Each part said alone is facts of its own, the two parts not said left unsaid rather than made zero.</summary>
    [Fact]
    public void EachPartSaidAloneIsFacts()
    {
        Assert.Equal(new CrapFacts(28.5, null, null), CrapOf("""{ "max": 28.5 }"""));
        Assert.Equal(new CrapFacts(null, 30, null), CrapOf("""{ "threshold": 30 }"""));
        Assert.Equal(new CrapFacts(null, null, 0), CrapOf("""{ "overThreshold": 0 }"""));
    }

    /// <summary>The worst score and the threshold keep their fraction; the count of the methods over is rounded to a
    /// whole count.</summary>
    [Fact]
    public void TheScoresKeepTheirFractionAndTheCountIsRounded()
    {
        var crap = CrapOf("""{ "max": 41.75, "threshold": 30.25, "overThreshold": 2.6 }""");

        Assert.Equal(new CrapFacts(41.75, 30.25, 3), crap);
    }

    /// <summary>A negative number is no part, whichever part it is said for; with all three negative there are no
    /// facts.</summary>
    [Fact]
    public void ANegativeNumberIsNoPart()
    {
        Assert.Equal(new CrapFacts(null, 30, 0), CrapOf("""{ "max": -0.1, "threshold": 30, "overThreshold": 0 }"""));
        Assert.Equal(new CrapFacts(28.5, null, 0), CrapOf("""{ "max": 28.5, "threshold": -30, "overThreshold": 0 }"""));
        Assert.Equal(new CrapFacts(28.5, 30, null), CrapOf("""{ "max": 28.5, "threshold": 30, "overThreshold": -1 }"""));
        Assert.Null(CrapOf("""{ "max": -1, "threshold": -1, "overThreshold": -1 }"""));
    }

    /// <summary>A score past the end of the number reads as endless, which is no score; one too small for the number
    /// reads as the zero it rounds to, and the zero is a score.</summary>
    [Fact]
    public void AScoreThatOverflowsIsNoScoreAndOneThatUnderflowsIsZero()
    {
        Assert.Equal(new CrapFacts(null, null, 3), CrapOf("""{ "max": 1e400, "threshold": 1e400, "overThreshold": 3 }"""));
        Assert.Equal(new CrapFacts(0.0, 0.0, null), CrapOf("""{ "max": 1e-400, "threshold": 1e-400 }"""));
    }

    /// <summary>The count is checked against the top of the count before it is rounded: just under the top it rounds up
    /// to the top, and a hair past the top is no count even though it would round down to it.</summary>
    [Theory]
    [InlineData("2147483646.6", int.MaxValue)]
    [InlineData("2147483647", int.MaxValue)]
    [InlineData("2147483647.4", null)]
    [InlineData("1e10", null)]
    public void TheCountIsCheckedAgainstTheTopBeforeItIsRounded(string count, int? expected)
    {
        Assert.Equal(new CrapFacts(41, 30, expected), CrapOf($$"""{ "max": 41, "threshold": 30, "overThreshold": {{count}} }"""));
    }

    /// <summary>A count said at the half rounds away from zero (#1328), so a half of a method over is one over.</summary>
    [Theory]
    [InlineData("0.4", 0)]
    [InlineData("0.5", 1)]
    [InlineData("1.5", 2)]
    [InlineData("2.5", 3)]
    public void ACountAtTheHalfRoundsUpToTheNextWhole(string count, int expected)
    {
        Assert.Equal(new CrapFacts(null, null, expected), CrapOf($$"""{ "overThreshold": {{count}} }"""));
    }

    /// <summary>The negative zero a file may write is not negative: it is kept as the zero, and the facts it makes are
    /// the facts of the zero.</summary>
    [Fact]
    public void TheNegativeZeroIsKeptAsTheZero()
    {
        var crap = CrapOf("""{ "max": -0, "threshold": -0.0, "overThreshold": -0 }""");

        Assert.Equal(new CrapFacts(0, 0, 0), crap);
        Assert.Equal(0, crap!.OverThreshold);
    }

    /// <summary>Numbers said as text, true or false, a list or a section are no parts, while the number beside them is
    /// kept; all three said that way are no facts.</summary>
    [Fact]
    public void APartThatIsNotANumberIsNoPart()
    {
        Assert.Equal(new CrapFacts(null, 30, null), CrapOf("""{ "max": "28.5", "threshold": 30, "overThreshold": "0" }"""));
        Assert.Equal(new CrapFacts(28.5, null, null), CrapOf("""{ "max": 28.5, "threshold": [30], "overThreshold": { "count": 0 } }"""));
        Assert.Null(CrapOf("""{ "max": true, "threshold": false, "overThreshold": null }"""));
    }

    /// <summary>A section that is a list, a number, text or true is no section, and no facts are read from it, even
    /// when the list holds a section that would say them.</summary>
    [Theory]
    [InlineData("""[ { "max": 28.5, "threshold": 30, "overThreshold": 0 } ]""")]
    [InlineData("[]")]
    [InlineData("28.5")]
    [InlineData("\"worst 28.5\"")]
    [InlineData("true")]
    public void ASectionThatIsNotAnObjectIsNoFacts(string section)
    {
        Assert.Null(CrapOf(section));
    }

    /// <summary>The parts are named in the case the build endpoint writes them: a part named in another case is no part,
    /// so a section that names all three that way is no facts.</summary>
    [Fact]
    public void PartsNamedInAnotherCaseAreNoParts()
    {
        Assert.Null(CrapOf("""{ "Max": 28.5, "THRESHOLD": 30, "OverThreshold": 0 }"""));
        Assert.Equal(new CrapFacts(null, 30, null), CrapOf("""{ "Max": 28.5, "threshold": 30, "overthreshold": 0 }"""));
    }

    /// <summary>The CRAP facts one answer reports, as the dashboard reads them: the version stands so that the answer is
    /// a build whatever the section says.</summary>
    private static CrapFacts? CrapOf(string crap) =>
        BuildInfo.Parse($$"""{ "version": "2.4.15", "crap": {{crap}} }""")!.Crap;
}
