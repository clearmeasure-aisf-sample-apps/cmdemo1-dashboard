namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="SummaryLevel" />, the four words for what the counts
/// behind the header add up to, declared in <c>src/Dashboard/Health/HealthSummary.cs</c>. What each level is worth in
/// a sentence and in a mark is pinned where the summary is read: <see cref="HealthSummaryTests" /> pins the words of
/// every level - the topology of no nodes, the problems counted before the checks that are still out, the checks
/// running with nothing answered and with some answered, the whole topology healthy - in the plural and of one node;
/// <see cref="DashboardMonitorTests" /> and <see cref="CostTests" /> pin the same words as the monitor's own rounds
/// return them. What all of that leaves open is pinned here: the four levels as the numbers, the names and the order
/// they are declared in, the level a value of the type that was never set stands on, nothing outside the four, the
/// levels combined as numbers, text that is or is not a level - and the boundaries of the counts themselves, which
/// level answers where the totals are past what a round of probes could hand out, where <c>Empty</c> answers before
/// the other three are asked, and where a remainder of the difference is a problem although no node was ever
/// unreachable.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: a level is a word the counts settled on, and
/// every count below is one the test brings. The one member of <see cref="HealthSummary" /> that a caller could hand
/// nothing at all is refused by name, which no other test says; the empty source a caller may hand it instead is
/// pinned by <see cref="HealthSummaryTests" />.
/// </para>
/// </summary>
public class SummaryLevelEdgeCaseTests
{
    // ----- the four levels, as the numbers they are

    /// <summary>The four levels in the order the enum declares them, on the numbers 0 to 3, of an <see cref="int" />
    /// underneath, and a choice and not a set: the enum wears no <see cref="FlagsAttribute" />.</summary>
    [Fact]
    public void TheFourSummaryLevelsAreTheWholeVocabularyInTheirDeclaredOrder()
    {
        Assert.Equal(["Empty", "Pending", "AllHealthy", "Problems"], Enum.GetNames<SummaryLevel>());
        Assert.Equal([0, 1, 2, 3], Enum.GetValues<SummaryLevel>().Select(level => (int)level));
        Assert.Equal(4, Enum.GetValues<SummaryLevel>().Length);
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(typeof(SummaryLevel)));
        Assert.False(typeof(SummaryLevel).IsDefined(typeof(FlagsAttribute), false));
    }

    /// <summary>Each level on its own number and its own word, both ways round: the number is the one the name
    /// stands on, the name is the one that number answers to, and every one of the four is defined.</summary>
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

    /// <summary>An unset level is the first one, <c>Empty</c> - the level of a topology of no endpoint, which is what
    /// a field of the type holds before a round has counted anything. Neither one below the first nor one above the
    /// last nor either end of <see cref="int" /> is a level, and a value the enum does not name says its
    /// number.</summary>
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
        Assert.Equal("2147483647", ((SummaryLevel)int.MaxValue).ToString());
    }

    /// <summary>The four levels are apart from one another and sort by their numbers, as the declaration orders them:
    /// the order the numbers fall in - the topology with nothing in it first, the problems last - and nothing that
    /// ranks how much each one worries the page, since a topology still being checked stands between the empty one
    /// and the healthy one. A boxed <see cref="int" /> on the same number is no level's equal.</summary>
    [Fact]
    public void TheSummaryLevelsAreApartFromOneAnotherAndSortByTheirNumber()
    {
        var levels = Enum.GetValues<SummaryLevel>();

        Assert.Equal(levels.Length, levels.Distinct().Count());
        Assert.Equal(levels, levels.Order());
        Assert.True(SummaryLevel.Empty < SummaryLevel.Pending);
        Assert.True(SummaryLevel.Pending < SummaryLevel.AllHealthy);
        Assert.True(SummaryLevel.AllHealthy < SummaryLevel.Problems);
        Assert.Equal(SummaryLevel.Empty, levels.Min());
        Assert.Equal(SummaryLevel.Problems, levels.Max());
        Assert.Equal(1, SummaryLevel.Problems.CompareTo(SummaryLevel.AllHealthy));
        Assert.Equal(-1, SummaryLevel.Empty.CompareTo(SummaryLevel.Pending));
        Assert.Equal(0, SummaryLevel.AllHealthy.CompareTo((object)SummaryLevel.AllHealthy));

        Assert.False(SummaryLevel.Problems.Equals(null));
        Assert.False(SummaryLevel.Problems.Equals((object)3));
        Assert.Equal(SummaryLevel.Problems.GetHashCode(), ((object)SummaryLevel.Problems).GetHashCode());
    }

    /// <summary>A level is a choice and not a set: the operators work on the numbers underneath, so two levels can
    /// combine into a third - <c>Pending</c> with <c>AllHealthy</c> answers as <c>Problems</c>, which is the trap the
    /// enum's <see cref="FlagsAttribute" />-less shape is worth pinning - can answer as one of the two they stand
    /// between, and, past <c>Problems</c>, lead to no level at all.</summary>
    [Fact]
    public void TwoSummaryLevelsCombinedAreAnotherLevelOrNoLevelAtAll()
    {
        Assert.Equal(SummaryLevel.Problems, SummaryLevel.Pending | SummaryLevel.AllHealthy);
        Assert.Equal(SummaryLevel.Pending, SummaryLevel.Pending | SummaryLevel.Empty);
        Assert.Equal(SummaryLevel.Problems, SummaryLevel.Problems | SummaryLevel.Empty);
        Assert.Equal(SummaryLevel.AllHealthy, SummaryLevel.Problems & SummaryLevel.AllHealthy);
        Assert.Equal(SummaryLevel.Pending, SummaryLevel.Problems & SummaryLevel.Pending);
        Assert.Equal(SummaryLevel.Empty, SummaryLevel.Pending & SummaryLevel.AllHealthy);
        Assert.Equal(SummaryLevel.AllHealthy, SummaryLevel.AllHealthy & SummaryLevel.Problems);
        Assert.Equal(SummaryLevel.Pending, SummaryLevel.Pending & SummaryLevel.Problems);

        Assert.Equal((SummaryLevel)4, SummaryLevel.Problems + 1);
        Assert.False(Enum.IsDefined(SummaryLevel.Problems + 1));
        Assert.Equal((SummaryLevel)(-1), SummaryLevel.Empty - 1);
        Assert.False(Enum.IsDefined(SummaryLevel.Empty - 1));
    }

    // ----- text that is or is not a level

    /// <summary>Text that names a level in the enum's case, surrounding spaces walked past, or holds its number is
    /// that level; <c>Empty</c> is read whole and never as the start of a longer word.</summary>
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

    /// <summary>Text that is no level is refused - null, empty, blank, another case, a name with something behind it,
    /// a prefix of a name, a fraction, a hexadecimal - and a refused parse leaves <c>Empty</c> behind: the word the
    /// page speaks, <c>Healthy</c>, is not the name the enum spells (<c>AllHealthy</c> is), and a word read as no
    /// level at all answers as the level of a topology with nothing in it.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("empty")]
    [InlineData("ALLHEALTHY")]
    [InlineData("Healthy")]
    [InlineData("NotHealthy")]
    [InlineData("PendingChecks")]
    [InlineData("Problem")]
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
        Assert.Throws<ArgumentException>(() => Enum.Parse<SummaryLevel>("Healthy"));
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

        Assert.False(Enum.TryParse<SummaryLevel>("allhealthy", out _));
        Assert.True(Enum.TryParse<SummaryLevel>("allhealthy", ignoreCase: true, out var forgiven));
        Assert.Equal(SummaryLevel.AllHealthy, forgiven);
    }

    // ----- the counts every level is answered from

    /// <summary>The source of every level refuses the states that are not there, by name, before it counts a single
    /// one of them - the one null of <see cref="HealthSummary" /> no other test says. An empty source is no such
    /// fault: it is answered, and answered with the first level, which is what <see cref="HealthSummaryTests" />
    /// says of it.</summary>
    [Fact]
    public void NoStatesAtAllAreRefusedByName()
    {
        var error = Assert.Throws<ArgumentNullException>(() => HealthSummary.Of(null!));

        Assert.Equal("states", error.ParamName);
    }

    /// <summary>Of a summary whose total is nothing, <c>Empty</c> is the answer before the other three are asked:
    /// the total is read first, so counts that a round could never have handed out - healthy nodes and checks still
    /// out of a topology of none - say no nodes at all, say the one sentence of the empty topology, and leave the
    /// difference standing below zero.</summary>
    [Theory]
    [InlineData(0, 1, 0, -1)]
    [InlineData(0, 2, 1, -3)]
    [InlineData(0, 0, 4, -4)]
    public void EmptyAnswersBeforeTheOtherThreeAreAsked(int total, int healthy, int pending, int notHealthy)
    {
        var summary = new HealthSummary(total, healthy, pending);

        Assert.Equal(SummaryLevel.Empty, summary.Level);
        Assert.Equal("No nodes in the topology", summary.Text);
        Assert.Equal(notHealthy, summary.NotHealthy);
    }

    /// <summary>The difference is weighed against nothing and not against anything else, so a remainder below zero -
    /// more nodes counted healthy and still out than there are nodes - is no problem: the answer is
    /// <c>Pending</c>, and the sentence is the one of the checks that are out.</summary>
    [Fact]
    public void CountsPastTheTotalAreNoProblemAndAnswerAsPending()
    {
        var past = new HealthSummary(3, 3, 1);

        Assert.Equal(-1, past.NotHealthy);
        Assert.Equal(SummaryLevel.Pending, past.Level);
        Assert.Equal("3 of 3 nodes healthy, 1 being checked", past.Text);
    }

    /// <summary>The step from one level to the next, taken at the smallest counts that reach it - the level alone,
    /// since the words each step answers with are pinned by <see cref="HealthSummaryTests" />: nothing to count is
    /// <c>Empty</c>, one node answered for is <c>AllHealthy</c> and one node unaccounted for is
    /// <c>Problems</c>, a check still out is <c>Pending</c> and the difference clearing back to nothing leaves
    /// <c>Pending</c> rather than <c>Problems</c>.</summary>
    [Theory]
    [InlineData(0, 1, 1, SummaryLevel.Empty)]
    [InlineData(1, 0, 0, SummaryLevel.Problems)]
    [InlineData(1, 1, 0, SummaryLevel.AllHealthy)]
    [InlineData(1, 0, 1, SummaryLevel.Pending)]
    [InlineData(2, 2, 0, SummaryLevel.AllHealthy)]
    [InlineData(2, 1, 1, SummaryLevel.Pending)]
    [InlineData(2, 1, 0, SummaryLevel.Problems)]
    [InlineData(3, 2, 1, SummaryLevel.Pending)]
    [InlineData(3, 1, 1, SummaryLevel.Problems)]
    [InlineData(3, 1, 0, SummaryLevel.Problems)]
    [InlineData(int.MaxValue, int.MaxValue, 0, SummaryLevel.AllHealthy)]
    [InlineData(int.MaxValue, int.MaxValue - 1, 1, SummaryLevel.Pending)]
    public void TheLevelTurnsOnTheTotalFirstOnTheDifferenceThenOnTheChecksOut(
        int total,
        int healthy,
        int pending,
        SummaryLevel expected) =>
        Assert.Equal(expected, new HealthSummary(total, healthy, pending).Level);

    /// <summary>A state the page has never read is neither an answer nor a check still out: it is counted in the
    /// total and in nothing else, so the node it leaves over the two is a problem and one such node is enough for
    /// <c>Problems</c> - the level <see cref="HealthSummary" /> answers with for a state outside the four of
    /// <see cref="HealthState" />, which is what an answer that invented a status would come to.</summary>
    [Fact]
    public void AStateOutsideTheFourIsCountedAsNeitherHealthyNorPending()
    {
        HealthState[] states = [HealthState.Healthy, (HealthState)9];
        var summary = HealthSummary.Of(states);

        Assert.Equal(2, summary.Total);
        Assert.Equal(1, summary.Healthy);
        Assert.Equal(0, summary.Pending);
        Assert.Equal(1, summary.NotHealthy);
        Assert.Equal(SummaryLevel.Problems, summary.Level);
    }

    /// <summary>The level a summary answers with is one of the four however the counts stand, at every end of them:
    /// nothing, one, four, and the largest total an <see cref="int" /> carries - each of them a defined level, and
    /// each of them said in words that are not empty.</summary>
    [Theory]
    [InlineData(0, 4, 4)]
    [InlineData(1, 1, 0)]
    [InlineData(1, 0, 1)]
    [InlineData(4, 2, 1)]
    [InlineData(4, 4, 0)]
    [InlineData(int.MaxValue, 0, 0)]
    [InlineData(int.MaxValue, int.MaxValue, int.MaxValue)]
    public void EverySummaryAnswersWithOneOfTheFourLevels(int total, int healthy, int pending)
    {
        var summary = new HealthSummary(total, healthy, pending);

        Assert.True(Enum.IsDefined(summary.Level));
        Assert.Contains(summary.Level, Enum.GetValues<SummaryLevel>());
        Assert.False(string.IsNullOrWhiteSpace(summary.Text));
    }
}
