using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests left open for <see cref="SummaryLevel" />, the four words the header
/// weighs what it has checked against, declared in <c>src/Dashboard/Health/HealthSummary.cs</c>.
/// <para>
/// <see cref="SummaryLevelEdgeCaseTests" /> already holds the enum's own edge cases - and holds them under the name
/// this issue asked for, so what is left over is set down under a name of its own rather than over that file, whose
/// every test stands; the precedent is <see cref="DeploymentBoundaryEdgeCaseTests" />, set down the same way over
/// <see cref="DeploymentEdgeCaseTests" />. Pinned there, and not pinned again here: the four levels as the numbers 0 to
/// 3, their names, the order they are declared in, the <see cref="int" /> underneath them and the fact that they are no
/// set; the unset level being <see cref="SummaryLevel.Empty" />; nothing outside the four being no level and saying its
/// own number; the text that is a level and the text that is not, through <c>Enum.TryParse</c> and <c>Enum.Parse</c>;
/// and the level of the eight exact count triples, the adding-up ones and the one triple that cannot.
/// <see cref="HealthSummaryTests" /> pins the level and the words of the counts a round really brings;
/// <see cref="HealthSummaryEdgeCaseTests" /> pins the words of counts that cannot be - the empty reading taken on the
/// total alone among them - and the difference subtracted at the ends of <see cref="int" />, but never the level those
/// counts fall on.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the four levels as values - how every one of them compares with every
/// other and with nothing at all, how they sort, that they stand apart from one another as keys and from the numbers and
/// from the node's states that share their numbers, and how each one hashes; the words each level says of itself when
/// asked how - named, numbered, or in eight hexadecimal digits which come capital whatever the case of the ask, and the
/// name again under a request for flags the enum does not carry, with every other format refused; what the levels are
/// added, and-ed, or-ed and negated as, which is nothing but the numbers under them; what the framework turns a level
/// into - an <see cref="int" />, a <see cref="TypeCode" />, a word, and a boolean no level except the first says true to;
/// a level that is nothing at all, which says no word and defaults to the reading of a topology with no endpoint in it;
/// and the level a summary falls to at the counts no round ever brings, where a total of nothing outranks a problem and a
/// pending check, a total of less than nothing is a total that is not zero, and both of the level's asks are strict.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every level, count, format and culture
/// below is one the test brings.
/// </para>
/// </summary>
public class SummaryLevelBoundaryEdgeCaseTests
{
    // ----- the four levels as values that compare

    /// <summary>Every level against every level, and every answer is the number under it speaking: the order is the
    /// order of the declaration, so <see cref="SummaryLevel.Empty" /> - the topology with nothing in it - comes first
    /// and <see cref="SummaryLevel.Problems" /> last, with nothing between the two meaning anything to the comparison.</summary>
    [Theory]
    [InlineData(SummaryLevel.Empty, SummaryLevel.Empty, 0)]
    [InlineData(SummaryLevel.Empty, SummaryLevel.Pending, -1)]
    [InlineData(SummaryLevel.Empty, SummaryLevel.AllHealthy, -1)]
    [InlineData(SummaryLevel.Empty, SummaryLevel.Problems, -1)]
    [InlineData(SummaryLevel.Pending, SummaryLevel.Empty, 1)]
    [InlineData(SummaryLevel.Pending, SummaryLevel.Pending, 0)]
    [InlineData(SummaryLevel.Pending, SummaryLevel.AllHealthy, -1)]
    [InlineData(SummaryLevel.Pending, SummaryLevel.Problems, -1)]
    [InlineData(SummaryLevel.AllHealthy, SummaryLevel.Empty, 1)]
    [InlineData(SummaryLevel.AllHealthy, SummaryLevel.Pending, 1)]
    [InlineData(SummaryLevel.AllHealthy, SummaryLevel.AllHealthy, 0)]
    [InlineData(SummaryLevel.AllHealthy, SummaryLevel.Problems, -1)]
    [InlineData(SummaryLevel.Problems, SummaryLevel.Empty, 1)]
    [InlineData(SummaryLevel.Problems, SummaryLevel.Pending, 1)]
    [InlineData(SummaryLevel.Problems, SummaryLevel.AllHealthy, 1)]
    [InlineData(SummaryLevel.Problems, SummaryLevel.Problems, 0)]
    public void EveryLevelComparesWithEveryLevelAsTheNumberUnderIt(
        SummaryLevel first,
        SummaryLevel second,
        int expected)
    {
        Assert.Equal(expected, first.CompareTo(second));
        Assert.Equal(-expected, second.CompareTo(first));
    }

    /// <summary>A level against nothing at all wins - even the first of the four does - and a level against a number of
    /// another type is refused outright rather than read as that number. A number outside the four is no level but
    /// compares all the same, standing below the first and above the last.</summary>
    [Theory]
    [InlineData(SummaryLevel.Empty)]
    [InlineData(SummaryLevel.Pending)]
    [InlineData(SummaryLevel.AllHealthy)]
    [InlineData(SummaryLevel.Problems)]
    public void ALevelStandsAgainstNothingAgainstItselfAndAgainstAnotherType(SummaryLevel level)
    {
        Assert.Equal(1, level.CompareTo(null));
        Assert.Equal(0, level.CompareTo((object)level));
        Assert.Equal(0, level.CompareTo(level));

        var fault = Assert.Throws<ArgumentException>(() => level.CompareTo((object)(int)level));
        Assert.Contains("same type", fault.Message, StringComparison.Ordinal);
    }

    /// <summary>The ends of the numbers outside the four keep their place against the four: one below the first level
    /// is below every level, and one above the last is above every level, whatever the four are taken to mean.</summary>
    [Fact]
    public void ANumberOutsideTheFourStillComparesWithTheFour()
    {
        Assert.Equal(-1, ((SummaryLevel)(-1)).CompareTo(SummaryLevel.Empty));
        Assert.Equal(1, ((SummaryLevel)4).CompareTo(SummaryLevel.Problems));
        Assert.Equal(-1, ((SummaryLevel)int.MinValue).CompareTo(SummaryLevel.Empty));
        Assert.Equal(1, ((SummaryLevel)int.MaxValue).CompareTo(SummaryLevel.Problems));

        Assert.True((SummaryLevel)4 > SummaryLevel.Problems);
        Assert.True((SummaryLevel)(-1) < SummaryLevel.Empty);
    }

    // ----- the four levels as keys

    /// <summary>The four sort as they are declared, stand apart from one another and serve as four keys: a level never
    /// set finds the <see cref="SummaryLevel.Empty" /> entry, because the unset level is that one, and a number outside
    /// the four is a key of its own that no dictionary of levels was ever given.</summary>
    [Fact]
    public void TheFourLevelsSortAsTheyAreDeclaredAndStandApartAsKeys()
    {
        var levels = Enum.GetValues<SummaryLevel>();

        Assert.Equal(levels, levels.Order());
        Assert.Equal(levels.Reverse().ToList(), levels.OrderDescending());
        Assert.Equal(4, levels.Distinct().Count());
        Assert.Equal(SummaryLevel.Empty, levels.Min());
        Assert.Equal(SummaryLevel.Problems, levels.Max());
        Assert.Equal(SummaryLevel.AllHealthy, levels[2]);

        var words = new Dictionary<SummaryLevel, string>
        {
            [SummaryLevel.Empty] = "empty",
            [SummaryLevel.Pending] = "pending",
            [SummaryLevel.AllHealthy] = "healthy",
            [SummaryLevel.Problems] = "problems",
        };

        Assert.Equal(4, words.Count);
        Assert.Equal("empty", words[default]);
        Assert.Equal("empty", words[(SummaryLevel)0]);
        Assert.Equal("problems", words[levels.Max()]);
        Assert.False(words.ContainsKey((SummaryLevel)4));
        Assert.False(words.ContainsKey((SummaryLevel)(-1)));
        Assert.Equal(4, new HashSet<SummaryLevel>([SummaryLevel.Empty, (SummaryLevel)0, SummaryLevel.Pending, SummaryLevel.Problems, (SummaryLevel)4]).Count);
    }

    /// <summary>A level is equal to itself and to nothing else - not to nothing, not to the <see cref="int" /> under it,
    /// and not to the same number under another name - and it hashes to the hash of that number, boxed or not.</summary>
    [Theory]
    [InlineData(SummaryLevel.Empty, 0)]
    [InlineData(SummaryLevel.Pending, 1)]
    [InlineData(SummaryLevel.AllHealthy, 2)]
    [InlineData(SummaryLevel.Problems, 3)]
    public void ALevelIsOneLevelAndNothingElse(SummaryLevel level, int number)
    {
        Assert.True(level.Equals(level));
        Assert.True(((object)level).Equals(level));
        Assert.False(level.Equals(null));
        Assert.False(level.Equals((object)number));
        Assert.False(level.Equals((object)level.ToString()));
        Assert.True(level == (SummaryLevel)number);
        Assert.False(level != (SummaryLevel)number);

        Assert.Equal(number, (int)level);
        Assert.Equal(((int)level).GetHashCode(), level.GetHashCode());
        Assert.Equal(((object)level).GetHashCode(), level.GetHashCode());
        Assert.NotEqual(SummaryLevel.Empty.GetHashCode(), SummaryLevel.Pending.GetHashCode());
    }

    /// <summary>A level and a node's state are two vocabularies that happen to be numbered from nothing: the first of
    /// the four stands on the same number as a state nobody has checked, and the last on the same number as a node
    /// that got no answer at all, and neither is the other's equal - only the number is.</summary>
    [Fact]
    public void ALevelIsNotTheStateOfANodeEvenWhereTheTwoStandOnOneNumber()
    {
        Assert.Equal((int)SummaryLevel.Empty, (int)HealthState.Pending);
        Assert.Equal((int)SummaryLevel.Problems, (int)HealthState.Unreachable);

        Assert.False(SummaryLevel.Empty.Equals(HealthState.Pending));
        Assert.False(HealthState.Pending.Equals(SummaryLevel.Empty));
        Assert.False(SummaryLevel.AllHealthy.Equals(HealthState.Unhealthy));
        Assert.False(SummaryLevel.Problems.Equals(HealthState.Unreachable));
        Assert.Equal(SummaryLevel.Problems.GetHashCode(), HealthState.Unreachable.GetHashCode());
        Assert.NotEqual(SummaryLevel.Empty, (SummaryLevel)(int)HealthState.Unhealthy);
    }

    // ----- the words a level says of itself, asked how

    /// <summary>The three ways a level words itself: as its own word, as its number, and as that number in eight
    /// hexadecimal digits. A request for flags is answered with the word, because the enum carries none and has nothing
    /// to spell out, and so are a format of nothing at all - no format and the empty one.</summary>
    [Theory]
    [InlineData(SummaryLevel.Empty, "Empty", "0", "00000000")]
    [InlineData(SummaryLevel.Pending, "Pending", "1", "00000001")]
    [InlineData(SummaryLevel.AllHealthy, "AllHealthy", "2", "00000002")]
    [InlineData(SummaryLevel.Problems, "Problems", "3", "00000003")]
    public void ALevelSaysItsOwnWordOrItsOwnNumberWhenAskedHow(
        SummaryLevel level,
        string word,
        string number,
        string hex)
    {
        var formattable = (IFormattable)level;

        Assert.Equal(word, formattable.ToString("g", CultureInfo.InvariantCulture));
        Assert.Equal(word, formattable.ToString("G", CultureInfo.InvariantCulture));
        Assert.Equal(word, formattable.ToString("F", CultureInfo.InvariantCulture));
        Assert.Equal(word, formattable.ToString("f", CultureInfo.InvariantCulture));
        Assert.Equal(word, formattable.ToString(null, CultureInfo.InvariantCulture));
        Assert.Equal(word, formattable.ToString(string.Empty, CultureInfo.InvariantCulture));
        Assert.Equal(word, level.ToString());
        Assert.Equal(word, $"{level}");

        Assert.Equal(number, formattable.ToString("d", CultureInfo.InvariantCulture));
        Assert.Equal(number, formattable.ToString("D", CultureInfo.InvariantCulture));

        Assert.Equal(hex, formattable.ToString("x", CultureInfo.InvariantCulture));
        Assert.Equal(hex, formattable.ToString("X", CultureInfo.InvariantCulture));
        Assert.Equal(hex, $"{level:x}");
        Assert.Equal(hex, $"{level:X}");
    }

    /// <summary>A number the four do not name says itself in every format, and the low case of the hexadecimal ask
    /// asks in vain: the digits a level is numbered in come capital whichever way the case was put, because the only
    /// letters they can be are the six above nine.</summary>
    [Fact]
    public void AValueOutsideTheFourSaysItsOwnNumberInEveryFormat()
    {
        Assert.Equal("4", ((IFormattable)(SummaryLevel)4).ToString("g", CultureInfo.InvariantCulture));
        Assert.Equal("00000004", ((IFormattable)(SummaryLevel)4).ToString("x", CultureInfo.InvariantCulture));
        Assert.Equal("-1", ((IFormattable)(SummaryLevel)(-1)).ToString("G", CultureInfo.InvariantCulture));
        Assert.Equal("FFFFFFFF", ((IFormattable)(SummaryLevel)(-1)).ToString("x", CultureInfo.InvariantCulture));
        Assert.Equal("000000FF", ((IFormattable)(SummaryLevel)255).ToString("x", CultureInfo.InvariantCulture));
        Assert.Equal("000000FF", $"{(SummaryLevel)255:x}");
        Assert.Equal("2147483647", ((IFormattable)(SummaryLevel)int.MaxValue).ToString("d", CultureInfo.InvariantCulture));
        Assert.Equal("7FFFFFFF", ((IFormattable)(SummaryLevel)int.MaxValue).ToString("x", CultureInfo.InvariantCulture));
        Assert.Equal("-2147483648", ((IFormattable)(SummaryLevel)int.MinValue).ToString("D", CultureInfo.InvariantCulture));
        Assert.Equal("80000000", ((IFormattable)(SummaryLevel)int.MinValue).ToString("x", CultureInfo.InvariantCulture));
    }

    /// <summary>Every other format is refused, by the level itself and by <c>Enum.Format</c> alike: the ask is one of
    /// eight letters and nothing else - not a length, not a space, not a second letter, and not the empty format
    /// <see cref="Enum.Format" /> refuses although a level forgives it.</summary>
    [Theory]
    [InlineData("n")]
    [InlineData("N")]
    [InlineData("0")]
    [InlineData("x2")]
    [InlineData("G ")]
    [InlineData("g ")]
    [InlineData("dd")]
    [InlineData("gg")]
    [InlineData(" ")]
    [InlineData("@")]
    public void AFormatThatIsNoFormatOfALevelIsRefused(string format)
    {
        Assert.Throws<FormatException>(() => ((IFormattable)SummaryLevel.Problems).ToString(format, CultureInfo.InvariantCulture));
        Assert.Throws<FormatException>(() => Enum.Format(typeof(SummaryLevel), SummaryLevel.Problems, format));
    }

    /// <summary>The one place the two ways of asking leave hold of each other: <c>Enum.Format</c> refuses the empty
    /// format, although a level asked with no format at all says its word - and <c>Enum.Format</c> does not care
    /// whether what it was handed to format is a level or the number one of the four stands on.</summary>
    [Fact]
    public void EnumFormatRefusesWhatALevelForgivesAndAcceptsTheNumberAlone()
    {
        Assert.Throws<FormatException>(() => Enum.Format(typeof(SummaryLevel), SummaryLevel.Problems, string.Empty));
        Assert.Equal("Problems", ((IFormattable)SummaryLevel.Problems).ToString(string.Empty, CultureInfo.InvariantCulture));

        Assert.Equal("Pending", Enum.Format(typeof(SummaryLevel), 1, "g"));
        Assert.Equal("00000003", Enum.Format(typeof(SummaryLevel), 3, "X"));
        Assert.Equal("4", Enum.Format(typeof(SummaryLevel), (SummaryLevel)4, "g"));
        Assert.Equal("Problems", Enum.Format(typeof(SummaryLevel), SummaryLevel.Problems, "G"));
    }

    /// <summary>None of the four turns for another reader: a German one is told the same word, the same number and the
    /// same eight digits - even when the ask is left to that reader's own culture, which is where a number would turn.</summary>
    [Fact]
    public void TheLevelsWordsAndNumbersDoNotTurnForAnotherReader()
    {
        ViewerCulture.German(() =>
        {
            Assert.Equal("Problems", ((IFormattable)SummaryLevel.Problems).ToString("g", CultureInfo.CurrentCulture));
            Assert.Equal("3", ((IFormattable)SummaryLevel.Problems).ToString("d", CultureInfo.CurrentCulture));
            Assert.Equal("00000003", ((IFormattable)SummaryLevel.Problems).ToString("x", CultureInfo.CurrentCulture));
            Assert.Equal("2147483647", ((IFormattable)(SummaryLevel)int.MaxValue).ToString("d", CultureInfo.CurrentCulture));
            Assert.Equal("Problems", SummaryLevel.Problems.ToString());
        });
    }

    // ----- what the levels are turned into

    /// <summary>What the framework makes of a level: the number it stands on, the word it answers to, an
    /// <see cref="TypeCode" /> of <see cref="int" />, and the level itself again - every one of them coming back the
    /// same from the level, from its number and from the object that number was put in.</summary>
    [Theory]
    [InlineData(SummaryLevel.Empty, 0, "Empty")]
    [InlineData(SummaryLevel.Pending, 1, "Pending")]
    [InlineData(SummaryLevel.AllHealthy, 2, "AllHealthy")]
    [InlineData(SummaryLevel.Problems, 3, "Problems")]
    public void ALevelTurnedIntoAnythingElseIsItsNumberOrItsWord(SummaryLevel level, int number, string word)
    {
        Assert.Equal(number, Convert.ToInt32(level, CultureInfo.InvariantCulture));
        Assert.Equal(word, Convert.ToString(level, CultureInfo.InvariantCulture));
        Assert.Equal(number, (int)Convert.ChangeType(level, typeof(int), CultureInfo.InvariantCulture));
        Assert.Equal(word, Convert.ChangeType(level, typeof(string), CultureInfo.InvariantCulture));
        Assert.Equal(level, Convert.ChangeType(level, typeof(SummaryLevel), CultureInfo.InvariantCulture));

        var convertible = (IConvertible)level;
        Assert.Equal(TypeCode.Int32, convertible.GetTypeCode());
        Assert.Equal(number, convertible.ToInt32(CultureInfo.InvariantCulture));
        Assert.Equal(word, convertible.ToString(CultureInfo.InvariantCulture));
        Assert.Equal(number, (int)convertible.ToType(typeof(int), CultureInfo.InvariantCulture)!);
        Assert.Equal(word, (string)convertible.ToType(typeof(string), CultureInfo.InvariantCulture)!);
        Assert.Equal(level, (SummaryLevel)convertible.ToType(typeof(SummaryLevel), CultureInfo.InvariantCulture)!);

        Assert.Equal<object>(level, Enum.ToObject(typeof(SummaryLevel), number));
        Assert.Equal<object>(level, Enum.ToObject(typeof(SummaryLevel), (long)number));
    }

    /// <summary>The two ways the framework is asked about a level's number answer differently, and only one of them
    /// checks anything: <c>Enum.ToObject</c> refuses nothing that is a number - a <see cref="long" /> is cut down to the
    /// four's own size, so five billion falls on 705032704 and the largest <see cref="uint" /> there is on the number
    /// below the first level - while refusing nothing at all, a word (even the word of a level) and a type that is no
    /// enum. <c>Enum.IsDefined</c> is the strict one: it refuses a number of another size outright, yet forgives a
    /// word, whether or not the word names one of the four.</summary>
    [Fact]
    public void ALevelIsMadeFromItsOwnNumbersAndFromNothingElse()
    {
        Assert.Throws<ArgumentNullException>(() => Enum.ToObject(typeof(SummaryLevel), null!));
        Assert.Throws<ArgumentException>(() => Enum.ToObject(typeof(SummaryLevel), string.Empty));
        Assert.Throws<ArgumentException>(() => Enum.ToObject(typeof(SummaryLevel), "Problems"));
        Assert.Throws<ArgumentNullException>(() => Enum.ToObject(null!, 1));
        Assert.Throws<ArgumentException>(() => Enum.ToObject(typeof(string), 1));

        // A number of another size is not refused; it is cut down to the size the four stand on.
        Assert.Equal<object>(SummaryLevel.Problems, Enum.ToObject(typeof(SummaryLevel), (long)3));
        Assert.Equal<object>(SummaryLevel.AllHealthy, Enum.ToObject(typeof(SummaryLevel), (byte)2));
        Assert.Equal<object>(SummaryLevel.AllHealthy, Enum.ToObject(typeof(SummaryLevel), (uint)2));
        Assert.Equal<object>((SummaryLevel)4, Enum.ToObject(typeof(SummaryLevel), 4L));
        Assert.Equal<object>((SummaryLevel)705032704, Enum.ToObject(typeof(SummaryLevel), 5_000_000_000L));
        Assert.Equal<object>((SummaryLevel)(-1), Enum.ToObject(typeof(SummaryLevel), uint.MaxValue));

        Assert.Throws<ArgumentNullException>(() => Enum.IsDefined(null!, 1));
        Assert.Throws<ArgumentException>(() => Enum.IsDefined(typeof(SummaryLevel), (short)3));
        Assert.Throws<ArgumentException>(() => Enum.IsDefined(typeof(SummaryLevel), 4L));
        Assert.True(Enum.IsDefined(typeof(SummaryLevel), 3));
        Assert.False(Enum.IsDefined(typeof(SummaryLevel), 4));
        Assert.True(Enum.IsDefined(typeof(SummaryLevel), "Problems"));
        Assert.False(Enum.IsDefined(typeof(SummaryLevel), "problem"));
        Assert.False(Enum.IsDefined(typeof(SummaryLevel), string.Empty));

        Assert.Equal((SummaryLevel)4, (SummaryLevel)Enum.ToObject(typeof(SummaryLevel), 4));
        Assert.False(Enum.IsDefined((SummaryLevel)Enum.ToObject(typeof(SummaryLevel), 4)));
    }

    /// <summary>A level weighed as a boolean is nothing only for the first of the four - which is the trap the empty
    /// reading sets, since the reading of a topology with no endpoint in it says "no" as loudly as a number outside the
    /// four, and every level a round can fall on says "yes".</summary>
    [Fact]
    public void OnlyTheEmptyLevelWeighedAsABooleanSaysNo()
    {
        Assert.False(((IConvertible)SummaryLevel.Empty).ToBoolean(CultureInfo.InvariantCulture));

        foreach (var level in new[] { SummaryLevel.Pending, SummaryLevel.AllHealthy, SummaryLevel.Problems, (SummaryLevel)(-1), (SummaryLevel)4 })
        {
            Assert.True(((IConvertible)level).ToBoolean(CultureInfo.InvariantCulture), level.ToString());
        }
    }

    // ----- the levels as an arithmetic of numbers, and the level that is not there

    /// <summary>The four are a reading and not a set, but the operators underneath know only numbers: the pending check
    /// and the healthy round combine into <see cref="SummaryLevel.Problems" />, two levels and-ed fall back to
    /// <see cref="SummaryLevel.Empty" />, one step past the last is no level at all, and the negation of the first is
    /// the number below it - nonsense the type cannot refuse, which is why the header weighs its level out of the counts
    /// and never combines two of them.</summary>
    [Fact]
    public void TwoLevelsCombinedAreTheNumbersUnderThemCombined()
    {
        Assert.Equal(SummaryLevel.Problems, SummaryLevel.Pending | SummaryLevel.AllHealthy);
        Assert.Equal(SummaryLevel.Empty, SummaryLevel.Pending & SummaryLevel.AllHealthy);
        Assert.Equal(SummaryLevel.Pending, SummaryLevel.Problems & SummaryLevel.Pending);
        Assert.Equal(SummaryLevel.Problems, SummaryLevel.Empty | SummaryLevel.Problems);
        Assert.Equal(SummaryLevel.Empty, SummaryLevel.Problems ^ SummaryLevel.Problems);

        Assert.Equal(SummaryLevel.AllHealthy, SummaryLevel.Pending + 1);
        Assert.Equal(SummaryLevel.Pending, SummaryLevel.AllHealthy - 1);
        Assert.Equal(3, SummaryLevel.Problems - SummaryLevel.Empty);
        Assert.Equal((SummaryLevel)4, SummaryLevel.Problems + 1);
        Assert.False(Enum.IsDefined(SummaryLevel.Problems + 1));
        Assert.Equal((SummaryLevel)(-1), SummaryLevel.Empty - 1);
        Assert.Equal((SummaryLevel)(-1), ~SummaryLevel.Empty);
        Assert.Equal((SummaryLevel)(-4), ~SummaryLevel.Problems);
    }

    /// <summary>A level that is not there at all says no word, is no level, and is not even the first of the four until
    /// it is asked gently - at which point it is <see cref="SummaryLevel.Empty" />, the reading of a topology with no
    /// endpoint in it rather than the reading of no topology. Put in an object it is either a plain level or nothing,
    /// never a level with a wrapper.</summary>
    [Fact]
    public void ALevelThatIsNothingAtAllSaysNoWordAtAll()
    {
        SummaryLevel? none = null;
        var alsoNone = default(SummaryLevel?);
        SummaryLevel? pending = SummaryLevel.Pending;

        Assert.False(none.HasValue);
        Assert.True(none == null);
        Assert.True(alsoNone == null);
        Assert.False(none == SummaryLevel.Empty);
        Assert.True(pending == SummaryLevel.Pending);
        Assert.Equal(string.Empty, none.ToString());
        Assert.Equal(SummaryLevel.Empty, none.GetValueOrDefault());
        Assert.Equal(SummaryLevel.Problems, none.GetValueOrDefault(SummaryLevel.Problems));
        Assert.Null((object?)none);
        Assert.Equal(typeof(SummaryLevel), ((object)pending).GetType());

        var unset = none;
        Assert.Throws<InvalidOperationException>(() => _ = unset!.Value);
    }

    // ----- the level a summary falls to at the counts no round brings

    /// <summary>The level a summary falls to at counts no round ever brings, which is all that is left of the four's
    /// boundaries: a total of nothing is read first and outranks a difference of two and a pending check alike, the
    /// ends of <see cref="int" /> among them; a total of less than nothing is a total that is not zero, so it is weighed
    /// as a problem, as a pending check or as everything healthy on the difference alone; and both asks are strict, so a
    /// difference of nothing is no problem and a pending count of nothing is no check still owed.</summary>
    [Theory]
    // The empty topology reads the total and nothing else, even where the two counts behind it make a problem.
    [InlineData(0, -1, -1, SummaryLevel.Empty)]
    [InlineData(0, 0, 1, SummaryLevel.Empty)]
    [InlineData(0, 1, 0, SummaryLevel.Empty)]
    [InlineData(0, 0, int.MinValue, SummaryLevel.Empty)]
    [InlineData(0, int.MaxValue, int.MaxValue, SummaryLevel.Empty)]
    // A total of less than nothing is not a total of nothing: the difference and the pending count decide it.
    [InlineData(-1, 0, 0, SummaryLevel.AllHealthy)]
    [InlineData(-1, -1, 0, SummaryLevel.AllHealthy)]
    [InlineData(-1, 0, 2, SummaryLevel.Pending)]
    [InlineData(-1, 0, -2, SummaryLevel.Problems)]
    // Both asks are strict: a difference of nothing is no problem, and no pending count is no check owed.
    [InlineData(5, 5, 0, SummaryLevel.AllHealthy)]
    [InlineData(5, 4, 1, SummaryLevel.Pending)]
    [InlineData(5, 4, 0, SummaryLevel.Problems)]
    [InlineData(5, 5, 5, SummaryLevel.Pending)]
    // The ends of int, whose words HealthSummaryEdgeCaseTests pins and whose level nobody did.
    [InlineData(int.MaxValue, int.MaxValue, 0, SummaryLevel.AllHealthy)]
    [InlineData(int.MinValue, 0, 1, SummaryLevel.Problems)]
    [InlineData(1, int.MaxValue, int.MaxValue, SummaryLevel.Problems)]
    public void TheLevelFallsOnTheCountsAndNotOnWhatTheyCouldEverHaveCounted(
        int total,
        int healthy,
        int pending,
        SummaryLevel expected)
    {
        Assert.Equal(expected, new HealthSummary(total, healthy, pending).Level);
    }

    /// <summary>However the three counts stand, the level that falls out of them is one of the four: swept over the
    /// ends of <see cref="int" /> and the numbers about them - 343 triples, every one of them a level a page could be
    /// asked to say - no counts land outside the vocabulary, whatever they make of the difference.</summary>
    [Fact]
    public void EveryThreeCountsFallOnOneOfTheFourLevels()
    {
        int[] edges = [int.MinValue, -2, -1, 0, 1, 2, int.MaxValue];
        var four = Enum.GetValues<SummaryLevel>();
        var seen = new HashSet<SummaryLevel>();

        foreach (var total in edges)
        {
            foreach (var healthy in edges)
            {
                foreach (var pending in edges)
                {
                    var level = new HealthSummary(total, healthy, pending).Level;

                    Assert.Contains(level, four);
                    seen.Add(level);
                }
            }
        }

        // Every one of the four is reached, and only the four: nothing is a level that the enum does not name.
        Assert.Equal(4, seen.Count);
        Assert.Equal(four, seen.Order().ToArray());
    }
}
