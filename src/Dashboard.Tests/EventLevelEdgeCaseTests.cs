namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="EventLevel" />, the four words for how the page
/// weighs what it observed, declared in <c>src/Dashboard/Health/DashboardEvents.cs</c>. Which level each detector
/// hands out is pinned where the detectors are - <see cref="EventDetectorTests" />, <see cref="ClusterEventDetectorTests" />,
/// <see cref="RotationTests" /> and <see cref="KustomizationPinMonitorTests" /> - and what they leave open is pinned
/// here: the four levels as the numbers, the names and the order they are declared in, the unset level, nothing
/// outside the four, the levels combined as numbers, and text that is or is not a level.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every moment is one the test brings.
/// </para>
/// </summary>
public class EventLevelEdgeCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 19, 12, 0, 0, TimeSpan.Zero);

    // ----- The four levels, as the numbers they are -----

    /// <summary>The four levels in the order the enum declares them, on the numbers 0 to 3, of an <see cref="int" />
    /// underneath.</summary>
    [Fact]
    public void TheFourEventLevelsAreTheWholeVocabularyInTheirDeclaredOrder()
    {
        Assert.Equal(["Info", "Good", "Warning", "Problem"], Enum.GetNames<EventLevel>());
        Assert.Equal([0, 1, 2, 3], Enum.GetValues<EventLevel>().Select(level => (int)level));
        Assert.Equal(4, Enum.GetValues<EventLevel>().Length);
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(typeof(EventLevel)));
        Assert.False(typeof(EventLevel).IsDefined(typeof(FlagsAttribute), false));
    }

    /// <summary>Each level on its own number and its own word, both ways round.</summary>
    [Theory]
    [InlineData(EventLevel.Info, 0, "Info")]
    [InlineData(EventLevel.Good, 1, "Good")]
    [InlineData(EventLevel.Warning, 2, "Warning")]
    [InlineData(EventLevel.Problem, 3, "Problem")]
    public void EachEventLevelStandsOnItsOwnNumberAndItsOwnWord(EventLevel level, int number, string name)
    {
        Assert.Equal(number, (int)level);
        Assert.Equal(level, (EventLevel)number);
        Assert.Equal(name, level.ToString());
        Assert.Equal(name, Enum.GetName(level));
        Assert.True(Enum.IsDefined(level));
    }

    /// <summary>An unset level is the first one, <c>Info</c>; neither one below the first nor one above the last nor
    /// either end of <see cref="int" /> is a level, and a value the enum does not name says its number.</summary>
    [Fact]
    public void AnUnsetEventLevelIsInfoAndNothingOutsideTheFourIsALevel()
    {
        Assert.Equal(EventLevel.Info, default);
        Assert.Equal(EventLevel.Info, (EventLevel)0);

        Assert.True(Enum.IsDefined(EventLevel.Info));
        Assert.True(Enum.IsDefined(EventLevel.Problem));
        Assert.False(Enum.IsDefined((EventLevel)(-1)));
        Assert.False(Enum.IsDefined((EventLevel)4));
        Assert.False(Enum.IsDefined((EventLevel)int.MinValue));
        Assert.False(Enum.IsDefined((EventLevel)int.MaxValue));

        Assert.Null(Enum.GetName((EventLevel)4));
        Assert.Null(Enum.GetName((EventLevel)(-1)));
        Assert.Equal("4", ((EventLevel)4).ToString());
        Assert.Equal("-1", ((EventLevel)(-1)).ToString());
        Assert.Equal("2147483647", ((EventLevel)int.MaxValue).ToString());
    }

    /// <summary>The four levels are apart from one another and sort by their numbers, as the declaration orders
    /// them; a boxed <see cref="int" /> on the same number is no level's equal.</summary>
    [Fact]
    public void TheEventLevelsAreApartFromOneAnotherAndSortByTheirNumber()
    {
        var levels = Enum.GetValues<EventLevel>();

        Assert.Equal(levels.Length, levels.Distinct().Count());
        Assert.Equal(levels, levels.Order());
        Assert.True(EventLevel.Info < EventLevel.Good);
        Assert.True(EventLevel.Warning < EventLevel.Problem);
        Assert.Equal(EventLevel.Info, levels.Min());
        Assert.Equal(EventLevel.Problem, levels.Max());
        Assert.Equal(1, EventLevel.Problem.CompareTo(EventLevel.Info));
        Assert.Equal(-1, EventLevel.Info.CompareTo(EventLevel.Problem));
        Assert.Equal(0, EventLevel.Good.CompareTo((object)EventLevel.Good));

        Assert.False(EventLevel.Problem.Equals(null));
        Assert.False(EventLevel.Problem.Equals((object)3));
        Assert.Equal(EventLevel.Problem.GetHashCode(), ((object)EventLevel.Problem).GetHashCode());
    }

    /// <summary>A level is a choice and not a set: the operators work on the numbers underneath, so two levels can
    /// combine into a third - or, past <c>Problem</c>, into no level at all.</summary>
    [Fact]
    public void TwoEventLevelsCombinedAreAnotherLevelOrNoLevelAtAll()
    {
        Assert.Equal(EventLevel.Problem, EventLevel.Good | EventLevel.Warning);
        Assert.Equal(EventLevel.Problem, EventLevel.Good | EventLevel.Problem);
        Assert.Equal(EventLevel.Problem, EventLevel.Info | EventLevel.Problem);
        Assert.Equal(EventLevel.Good, EventLevel.Problem & EventLevel.Good);
        Assert.Equal(EventLevel.Warning, EventLevel.Problem & EventLevel.Warning);
        Assert.Equal(EventLevel.Info, EventLevel.Good & EventLevel.Warning);

        Assert.Equal((EventLevel)4, EventLevel.Problem + 1);
        Assert.False(Enum.IsDefined(EventLevel.Problem + 1));
        Assert.Equal((EventLevel)(-1), EventLevel.Info - 1);
        Assert.False(Enum.IsDefined(EventLevel.Info - 1));
    }

    // ----- Text that is or is not a level -----

    /// <summary>Text that names a level in the enum's case, surrounding spaces walked past, or holds its number is
    /// that level; <c>Warn</c> is refused and never read as the start of <c>Warning</c>.</summary>
    [Theory]
    [InlineData("Info", EventLevel.Info)]
    [InlineData("Good", EventLevel.Good)]
    [InlineData("Warning", EventLevel.Warning)]
    [InlineData("Problem", EventLevel.Problem)]
    [InlineData("  Problem  ", EventLevel.Problem)]
    [InlineData("0", EventLevel.Info)]
    [InlineData("3", EventLevel.Problem)]
    [InlineData("02", EventLevel.Warning)]
    public void AnEventLevelTextIsTheLevelItNamesOrTheLevelItNumbers(string text, EventLevel expected)
    {
        Assert.True(Enum.TryParse<EventLevel>(text, out var level));
        Assert.Equal(expected, level);
        Assert.True(Enum.IsDefined(level));
        Assert.Equal(expected, Enum.Parse<EventLevel>(text));
    }

    /// <summary>Text that is no level is refused - null, empty, blank, another case, a name with something behind
    /// it, a prefix of a name, a fraction, a hexadecimal - and a refused parse leaves <c>Info</c> behind.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("info")]
    [InlineData("PROBLEM")]
    [InlineData("Problematic")]
    [InlineData("Good morning")]
    [InlineData("Warn")]
    [InlineData("3.0")]
    [InlineData("0x3")]
    public void TextThatIsNoEventLevelIsRefusedAndLeavesInfoBehind(string? text)
    {
        Assert.False(Enum.TryParse<EventLevel>(text, out var level));
        Assert.Equal(EventLevel.Info, level);
        Assert.False(Enum.TryParse<EventLevel>(text, ignoreCase: false, out _));
    }

    /// <summary><c>Enum.Parse</c> says what is wrong with text that is no level: null is no text at all, the empty
    /// string and a blank are text without a level in it, and an unknown word is an unknown word.</summary>
    [Fact]
    public void ParsingTextThatIsNoEventLevelThrows()
    {
        Assert.Throws<ArgumentNullException>(() => Enum.Parse<EventLevel>(null!));
        Assert.Throws<ArgumentException>(() => Enum.Parse<EventLevel>(string.Empty));
        Assert.Throws<ArgumentException>(() => Enum.Parse<EventLevel>("   "));
        Assert.Throws<ArgumentException>(() => Enum.Parse<EventLevel>("Problematic"));
    }

    /// <summary>A number outside the four is read as a level that is no level, and a name in another case is a level
    /// only once the parse is told to forgive the case.</summary>
    [Fact]
    public void ANumberOutsideTheFourIsReadAsALevelThatIsNoLevel()
    {
        Assert.True(Enum.TryParse<EventLevel>("4", out var above));
        Assert.Equal((EventLevel)4, above);
        Assert.False(Enum.IsDefined(above));

        Assert.True(Enum.TryParse<EventLevel>("-1", out var below));
        Assert.Equal((EventLevel)(-1), below);
        Assert.False(Enum.IsDefined(below));

        Assert.False(Enum.TryParse<EventLevel>("problem", out _));
        Assert.True(Enum.TryParse<EventLevel>("problem", ignoreCase: true, out var forgiven));
        Assert.Equal(EventLevel.Problem, forgiven);
    }

    // ----- The level an event carries -----

    /// <summary>An event carries the level it is given, whatever it is: a level no detector hands out, a number
    /// outside the four, and the default <c>Info</c>, each kept and each making another event.</summary>
    [Fact]
    public void AnEventKeepsTheLevelItIsGivenEvenOneOutsideTheFour()
    {
        var info = new DashboardEvent(Now, EventKind.Health, default, null, null, string.Empty);
        var outside = info with { Level = (EventLevel)99 };

        Assert.Equal(EventLevel.Info, info.Level);
        Assert.Equal((EventLevel)99, outside.Level);
        Assert.NotEqual(info, outside);
        Assert.Equal(info, outside with { Level = EventLevel.Info });
        Assert.EndsWith(", Kind = Health, Level = 99, Environment = , Node = , Text =  }", outside.ToString(), StringComparison.Ordinal);
    }
}
