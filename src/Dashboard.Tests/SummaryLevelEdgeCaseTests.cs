namespace Dashboard.Tests;

/// <summary>
/// The boundary, degenerate and fixed-value tests of <see cref="SummaryLevel" />, the four words the header
/// uses to weigh what it has checked, declared in <c>src/Dashboard/Health/HealthSummary.cs</c>. How each count
/// shapes the level is pinned where the counts live - <see cref="HealthSummaryTests" /> - and what they leave
/// open is pinned here: the four levels as the numbers, the names and the order they are declared in, the unset
/// level, nothing outside the four, and text that is or is not a level.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every count is one the test brings.
/// </para>
/// </summary>
public class SummaryLevelEdgeCaseTests
{
    // ----- The four levels, as the numbers they are -----

    /// <summary>The four levels in the order the enum declares them, on the numbers 0 to 3, of an <see cref="int" />
    /// underneath.</summary>
    [Fact]
    public void TheFourSummaryLevelsAreTheWholeVocabularyInTheirDeclaredOrder()
    {
        Assert.Equal(["Empty", "Pending", "AllHealthy", "Problems"], Enum.GetNames<SummaryLevel>());
        Assert.Equal([0, 1, 2, 3], Enum.GetValues<SummaryLevel>().Select(level => (int)level));
        Assert.Equal(4, Enum.GetValues<SummaryLevel>().Length);
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(typeof(SummaryLevel)));
        Assert.False(typeof(SummaryLevel).IsDefined(typeof(FlagsAttribute), false));
    }

    /// <summary>Each level on its own number and its own word, both ways round.</summary>
    [Theory]
    [InlineData(SummaryLevel.Empty, 0, "Empty")]
    [InlineData(SummaryLevel.Pending, 1, "Pending")]
    [InlineData(SummaryLevel.AllHealthy, 2, "AllHealthy")]
    [InlineData(SummaryLevel.Problems, 3, "Problems")]
    public void EachSummaryLevelStandsOnItsOwnNumberAndItsOwnWord(SummaryLevel level, int number, string name)
    {
        Assert.Equal(number, (int)level);
        Assert.Equal(level, (SummaryLevel)number);
        Assert.Equal(name, level.ToString());
        Assert.Equal(name, Enum.GetName(level));
        Assert.True(Enum.IsDefined(level));
    }

    /// <summary>An unset level is the first one, <c>Empty</c>; neither one below the first nor one above the last nor
    /// either end of <see cref="int" /> is a level, and a value the enum does not name says its number.</summary>
    [Fact]
    public void AnUnsetSummaryLevelIsEmptyAndNothingOutsideTheFourIsALevel()
    {
        Assert.Equal(SummaryLevel.Empty, default);
        Assert.Equal(SummaryLevel.Empty, (SummaryLevel)0);

        Assert.True(Enum.IsDefined(SummaryLevel.Empty));
        Assert.True(Enum.IsDefined(SummaryLevel.Problems));
        Assert.False(Enum.IsDefined((SummaryLevel)(-1)));
        Assert.False(Enum.IsDefined((SummaryLevel)4));
        Assert.False(Enum.IsDefined((SummaryLevel)int.MinValue));
        Assert.False(Enum.IsDefined((SummaryLevel)int.MaxValue));

        Assert.Null(Enum.GetName((SummaryLevel)4));
        Assert.Null(Enum.GetName((SummaryLevel)(-1)));
        Assert.Equal("4", ((SummaryLevel)4).ToString());
        Assert.Equal("-1", ((SummaryLevel)(-1)).ToString());
        Assert.Equal("-2147483648", ((SummaryLevel)int.MinValue).ToString());
        Assert.Equal("2147483647", ((SummaryLevel)int.MaxValue).ToString());
    }

    // ----- Text that is or is not a level -----

    /// <summary>Text that names a level in the enum's case, surrounding spaces walked past, or holds its number is
    /// that level; <c>Problem</c> is refused and never read as the start of <c>Problems</c>.</summary>
    [Theory]
    [InlineData("Empty", SummaryLevel.Empty)]
    [InlineData("Pending", SummaryLevel.Pending)]
    [InlineData("AllHealthy", SummaryLevel.AllHealthy)]
    [InlineData("Problems", SummaryLevel.Problems)]
    [InlineData("  Problems  ", SummaryLevel.Problems)]
    [InlineData("0", SummaryLevel.Empty)]
    [InlineData("3", SummaryLevel.Problems)]
    [InlineData("02", SummaryLevel.AllHealthy)]
    public void ASummaryLevelTextIsTheLevelItNamesOrTheLevelItNumbers(string text, SummaryLevel expected)
    {
        Assert.True(Enum.TryParse<SummaryLevel>(text, out var level));
        Assert.Equal(expected, level);
        Assert.True(Enum.IsDefined(level));
        Assert.Equal(expected, Enum.Parse<SummaryLevel>(text));
    }

    /// <summary>Text that is no level is refused - null, empty, blank, another case, a name with something behind
    /// it, a prefix of a name, a fraction, a hexadecimal - and a refused parse leaves <c>Empty</c> behind.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("empty")]
    [InlineData("ALLHEALTHY")]
    [InlineData("Problem")]
    [InlineData("All healthy")]
    [InlineData("Pendi")]
    [InlineData("3.0")]
    [InlineData("0x3")]
    public void TextThatIsNoSummaryLevelIsRefusedAndLeavesEmptyBehind(string? text)
    {
        Assert.False(Enum.TryParse<SummaryLevel>(text, out var level));
        Assert.Equal(SummaryLevel.Empty, level);
        Assert.False(Enum.TryParse<SummaryLevel>(text, ignoreCase: false, out _));
    }

    /// <summary><c>Enum.Parse</c> says what is wrong with text that is no level: null is no text at all, the empty
    /// string and a blank are text without a level in it, and an unknown word is an unknown word.</summary>
    [Fact]
    public void ParsingTextThatIsNoSummaryLevelThrows()
    {
        Assert.Throws<ArgumentNullException>(() => Enum.Parse<SummaryLevel>(null!));
        Assert.Throws<ArgumentException>(() => Enum.Parse<SummaryLevel>(string.Empty));
        Assert.Throws<ArgumentException>(() => Enum.Parse<SummaryLevel>("   "));
        Assert.Throws<ArgumentException>(() => Enum.Parse<SummaryLevel>("Problem"));
    }

    /// <summary>A number outside the four is read as a level that is no level, and a name in another case is a level
    /// only once the parse is told to forgive the case.</summary>
    [Fact]
    public void ANumberOutsideTheFourIsReadAsALevelThatIsNoLevel()
    {
        Assert.True(Enum.TryParse<SummaryLevel>("4", out var above));
        Assert.Equal((SummaryLevel)4, above);
        Assert.False(Enum.IsDefined(above));

        Assert.True(Enum.TryParse<SummaryLevel>("-1", out var below));
        Assert.Equal((SummaryLevel)(-1), below);
        Assert.False(Enum.IsDefined(below));

        Assert.False(Enum.TryParse<SummaryLevel>("problems", out _));
        Assert.True(Enum.TryParse<SummaryLevel>("problems", ignoreCase: true, out var forgiven));
        Assert.Equal(SummaryLevel.Problems, forgiven);
    }

    // ----- The level a summary takes from its counts -----

    /// <summary>The level the public constructor takes from counts that add up: an empty topology, every answer
    /// healthy, healthy ones waiting on the rest, and a problem - a problem outranking a pending check - each on
    /// two nodes, and the same boundaries on the single node.</summary>
    [Theory]
    [InlineData(0, 0, 0, SummaryLevel.Empty)]
    [InlineData(2, 2, 0, SummaryLevel.AllHealthy)]
    [InlineData(2, 1, 1, SummaryLevel.Pending)]
    [InlineData(2, 1, 0, SummaryLevel.Problems)]
    [InlineData(2, 0, 1, SummaryLevel.Problems)]
    [InlineData(1, 1, 0, SummaryLevel.AllHealthy)]
    [InlineData(1, 0, 1, SummaryLevel.Pending)]
    [InlineData(1, 0, 0, SummaryLevel.Problems)]
    public void TheLevelHealthSummaryTakesFromExactCounts(int total, int healthy, int pending, SummaryLevel level)
    {
        var summary = new HealthSummary(total, healthy, pending);

        Assert.Equal(total - healthy - pending, summary.NotHealthy);
        Assert.Equal(level, summary.Level);
    }

    /// <summary>Counts that do not add up are kept as given: a healthy count above the total leaves
    /// <c>NotHealthy</c> negative, and the level still falls on the pending branch, exactly as the siblings pin
    /// their degenerate cases.</summary>
    [Fact]
    public void CountsThatDoNotAddUpAreKeptAsGivenAndStillReadAsPending()
    {
        var summary = new HealthSummary(10, 20, 5);

        Assert.Equal(-15, summary.NotHealthy);
        Assert.Equal(SummaryLevel.Pending, summary.Level);
    }
}
