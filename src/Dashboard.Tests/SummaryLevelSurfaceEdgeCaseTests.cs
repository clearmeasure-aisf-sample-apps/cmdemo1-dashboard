using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests still open for <see cref="SummaryLevel" />, the four words the
/// header weighs what it has checked against, declared in <c>src/Dashboard/Health/HealthSummary.cs</c>: a level read
/// out of text and written into text with no string in between, a level weighed as a flag it was never declared to
/// be, and the four as the metadata the type is compiled into.
/// <para>
/// The name this issue asked for is held. <see cref="SummaryLevelEdgeCaseTests" /> already pins the four as the
/// numbers 0 to 3, their names, the order they are declared in, the <see cref="int" /> underneath them, the unset
/// level, everything outside the four, and the reads of <c>Enum.TryParse</c> and <c>Enum.Parse</c> taken on a
/// <see cref="string" />; <see cref="SummaryLevelBoundaryEdgeCaseTests" /> pins them as values - comparing, sorting,
/// standing apart as keys, hashing, saying themselves in every format, turning into <see cref="int" />,
/// <see cref="string" /> and <see cref="bool" />, combining as the numbers they are, and the nullable level that is
/// nothing at all; and <see cref="HealthSummaryTests" />, <see cref="HealthSummaryEdgeCaseTests" /> and both of those
/// pin the level a <see cref="HealthSummary" />'s counts fall on and the words built out of it. What none of them
/// reads - <c>HasFlag</c>, the <see cref="ReadOnlySpan{T}" /> overloads of those same two reads, the overloads told
/// which enum on the side of the call rather than by a type argument, <c>Enum.TryFormat</c> and the
/// <see cref="ISpanFormattable" /> the four implement, and everything reflection sees of the type - is set down here,
/// a file of its own over a name that stands, as <see cref="SummaryLevelBoundaryEdgeCaseTests" /> stands over
/// <see cref="SummaryLevelEdgeCaseTests" /> and <see cref="DeploymentBoundaryEdgeCaseTests" /> over
/// <see cref="DeploymentEdgeCaseTests" />.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every text, slice, buffer, format,
/// reader, boxed number and field is one the test brings.
/// </para>
/// </summary>
public class SummaryLevelSurfaceEdgeCaseTests
{
    /// <summary>Every binding the declaration scan needs: only what the type itself declares, public and otherwise,
    /// instance and static.</summary>
    private const BindingFlags DeclaredOnly =
        BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    /// <summary>The ten characters <see cref="SummaryLevel.AllHealthy" /> is written in: the longest word of the
    /// four, and the one a buffer is measured against when a test asks whether one character less would do.</summary>
    private const string AllHealthyWord = "AllHealthy";

    // ----- helpers

    /// <summary>The four numbers the levels stand on, in the order the enum declares them.</summary>
    private static readonly int[] LevelNumbers = [0, 1, 2, 3];

    /// <summary>The number one of the levels stands on, in every whole size the framework carries: the type-taking
    /// name of a level takes the number in whichever size it arrives.</summary>
    private static readonly object[] ATwoInEveryWholeSize = [(sbyte)2, (byte)2, (short)2, (ushort)2, 2, 2u, (long)2, (ulong)2];

    /// <summary>Things that are no number an enum stands on: a <see cref="double" />, a <see cref="float" /> and a
    /// <see cref="decimal" /> are numbers of another kind, a <see cref="bool" /> is neither, and two of them are
    /// words rather than numbers at all.</summary>
    private static readonly object[] NotWholeNumbers = ["2", "Problems", 2.0, 2.0f, 2m, true];

    /// <summary>Whether <paramref name="level" /> carries <paramref name="flag" />, asked through the base type as
    /// the compiler will not have it asked in place: a flag of another enum's own, or none at all, is exactly the ask
    /// under test, and <c>CA2248</c> refuses to let it be written where it is made. The declaration promises a flag
    /// that is not nothing and hands back an <see cref="ArgumentNullException" /> when it is nothing, which is the
    /// other half of what is pinned below - hence the <c>!</c> the promise asks for.</summary>
    private static bool Weighed(SummaryLevel level, Enum? flag) => level.HasFlag(flag!);

    /// <summary>The same ask taken on a level that is already in a box, as any <see cref="Enum" /> is.</summary>
    private static bool Weighed(Enum level, Enum flag) => level.HasFlag(flag);

    /// <summary><c>Enum.Parse</c> told which enum on the side of the call, held behind a <see cref="Type" />
    /// parameter: <c>CA2263</c> would rather the generic overload were called, and the type-taking one is what is
    /// under test here - the boxed level it answers with, or the fault it refuses with.</summary>
    private static object Parsed(Type enumType, string? text) => Enum.Parse(enumType, text!);

    /// <summary>The same ask over a span of text, which is the overload that reads a name out of the middle of a
    /// longer text without ever cutting a string out of it.</summary>
    private static object Parsed(Type enumType, ReadOnlySpan<char> text) => Enum.Parse(enumType, text);

    /// <summary>What <c>Enum.TryFormat</c> writes into a buffer big enough for it, as the text it wrote.</summary>
    private static string Written(SummaryLevel level, string? format, int room = 64)
    {
        var buffer = new char[room];

        Assert.True(
            Enum.TryFormat(level, buffer, out var written, format),
            $"{level} in format \"{format}\" wrote nothing into {room} character(s)");

        return new string(buffer, 0, written);
    }

    /// <summary>Whether the ask fits in <paramref name="room" /> characters, and how many it wrote when it did.</summary>
    private static (bool Wrote, int Written) Fit(SummaryLevel level, string format, int room)
    {
        var buffer = new char[room];

        return (Enum.TryFormat(level, buffer, out var written, format), written);
    }

    // ----- a level weighed as a flag, which is the one ask the four were never declared to answer

    /// <summary>The four weighed as a set of bits: every one of them carries <see cref="SummaryLevel.Empty" />, which
    /// is no bit at all, and only <see cref="SummaryLevel.Problems" /> - the number three, the word of a round that
    /// found something wrong - carries another level beside itself: the pending check and the healthy round, which
    /// are the numbers one and two. The four are a reading and not a set, and the ask knows only bits.</summary>
    [Theory]
    [InlineData(SummaryLevel.Empty, SummaryLevel.Empty, true)]
    [InlineData(SummaryLevel.Empty, SummaryLevel.Pending, false)]
    [InlineData(SummaryLevel.Empty, SummaryLevel.AllHealthy, false)]
    [InlineData(SummaryLevel.Empty, SummaryLevel.Problems, false)]
    [InlineData(SummaryLevel.Pending, SummaryLevel.Empty, true)]
    [InlineData(SummaryLevel.Pending, SummaryLevel.Pending, true)]
    [InlineData(SummaryLevel.Pending, SummaryLevel.AllHealthy, false)]
    [InlineData(SummaryLevel.Pending, SummaryLevel.Problems, false)]
    [InlineData(SummaryLevel.AllHealthy, SummaryLevel.Empty, true)]
    [InlineData(SummaryLevel.AllHealthy, SummaryLevel.Pending, false)]
    [InlineData(SummaryLevel.AllHealthy, SummaryLevel.AllHealthy, true)]
    [InlineData(SummaryLevel.AllHealthy, SummaryLevel.Problems, false)]
    [InlineData(SummaryLevel.Problems, SummaryLevel.Empty, true)]
    [InlineData(SummaryLevel.Problems, SummaryLevel.Pending, true)]
    [InlineData(SummaryLevel.Problems, SummaryLevel.AllHealthy, true)]
    [InlineData(SummaryLevel.Problems, SummaryLevel.Problems, true)]
    public void ALevelWeighedAsAFlagAnswersTheBitsAndNotTheReading(SummaryLevel level, SummaryLevel flag, bool expected) =>
        Assert.Equal(expected, level.HasFlag(flag));

    /// <summary>The same bits weighed on numbers the four do not name: one above the last carries itself and the
    /// nothing of <see cref="SummaryLevel.Empty" /> and nothing else, the largest <see cref="int" /> there is carries
    /// every one of the four at once, the smallest carries the first of them and no other, and two levels that share
    /// no bit share nothing however near they stand.</summary>
    [Theory]
    [InlineData((SummaryLevel)4, (SummaryLevel)4, true)]
    [InlineData((SummaryLevel)4, SummaryLevel.Empty, true)]
    [InlineData((SummaryLevel)4, SummaryLevel.Problems, false)]
    [InlineData((SummaryLevel)4, SummaryLevel.Pending, false)]
    [InlineData((SummaryLevel)2, SummaryLevel.Problems, false)]
    [InlineData((SummaryLevel)(-1), (SummaryLevel)4, true)]
    [InlineData((SummaryLevel)(-1), SummaryLevel.AllHealthy, true)]
    [InlineData((SummaryLevel)(-1), SummaryLevel.Problems, true)]
    [InlineData((SummaryLevel)int.MaxValue, SummaryLevel.Problems, true)]
    [InlineData((SummaryLevel)int.MinValue, SummaryLevel.Empty, true)]
    [InlineData((SummaryLevel)int.MinValue, SummaryLevel.Pending, false)]
    [InlineData((SummaryLevel)int.MinValue, SummaryLevel.AllHealthy, false)]
    public void ANumberOutsideTheFourIsWeighedAsAFlagByItsBitsAlone(SummaryLevel level, SummaryLevel flag, bool expected) =>
        Assert.Equal(expected, level.HasFlag(flag));

    /// <summary>Two asks the weighing refuses outright rather than answers no: nothing at all as the flag, which is
    /// the argument's own name in the fault, and another enum's own level - a node's state standing on the same
    /// number as a reading of the whole topology is refused by the type it belongs to. A plain number handed over in
    /// a box is refused sooner still, being no <see cref="Enum" /> at all, and the weighing of a level through the
    /// base type answers exactly what the level answers itself.</summary>
    [Fact]
    public void AFlagThatIsNothingOrAnotherEnumsOwnIsRefused()
    {
        var noFlag = Assert.Throws<ArgumentNullException>(() => Weighed(SummaryLevel.Problems, null));

        Assert.Equal("flag", noFlag.ParamName);

        var other = Assert.Throws<ArgumentException>(() => Weighed(SummaryLevel.Problems, (Enum)(object)HealthState.Healthy));

        Assert.Contains("HealthState", other.Message, StringComparison.Ordinal);
        Assert.Contains("SummaryLevel", other.Message, StringComparison.Ordinal);

        // A number in a box is no <see cref="Enum" />: the hand-over fails before a single bit is weighed.
        Assert.Throws<InvalidCastException>(() => Weighed(SummaryLevel.Problems, (Enum)(object)1));

        // And weighed in a box, on the base type rather than the level, the answer is the answer itself.
        Assert.True(Weighed((Enum)(object)SummaryLevel.Problems, (Enum)(object)SummaryLevel.Pending));
        Assert.False(Weighed((Enum)(object)SummaryLevel.Pending, (Enum)(object)SummaryLevel.Problems));
    }

    // ----- a level read out of a span of text

    /// <summary>The four read straight out of a span, with no string cut out of it first: each by its own word and
    /// each by its own number, both walked past the spaces on either side of them and past the line break a file ends
    /// a line with, a number past either end of the four but still a number an <see cref="int" /> holds, and a number
    /// in sign or in leading zeros. The string overloads answer the same way - pinned in
    /// <see cref="SummaryLevelEdgeCaseTests" /> - and this is the same reading on the span that copied nothing.</summary>
    [Theory]
    [InlineData("Empty", SummaryLevel.Empty)]
    [InlineData("Pending", SummaryLevel.Pending)]
    [InlineData("AllHealthy", SummaryLevel.AllHealthy)]
    [InlineData("Problems", SummaryLevel.Problems)]
    [InlineData("  Problems  ", SummaryLevel.Problems)]
    [InlineData("\n3\n", SummaryLevel.Problems)]
    [InlineData("+3", SummaryLevel.Problems)]
    [InlineData("-0", SummaryLevel.Empty)]
    [InlineData("0000000002", SummaryLevel.AllHealthy)]
    [InlineData(" 4 ", (SummaryLevel)4)]
    [InlineData(" -1 ", (SummaryLevel)(-1))]
    [InlineData("2147483647", (SummaryLevel)int.MaxValue)]
    [InlineData("-2147483648", (SummaryLevel)int.MinValue)]
    public void ASpanOfTextIsTheLevelItNamesOrTheLevelItNumbers(string text, SummaryLevel expected)
    {
        Assert.True(Enum.TryParse<SummaryLevel>(text.AsSpan(), out var read), $"\"{text}\" was refused");
        Assert.Equal(expected, read);
        Assert.Equal(expected, Enum.Parse<SummaryLevel>(text.AsSpan()));
    }

    /// <summary>A name read out of the middle of a longer text is that name still, and the very same characters from
    /// the start of the same text are no level at all: the span is the read, and a caller that points it at the word
    /// gets the word without a copy. A slice asked for past the end of the text is the caller's own mistake, told it
    /// before anything at all is read.</summary>
    [Fact]
    public void ANameCutFromTheMiddleOfLongerTextIsThatNameStill()
    {
        const string Text = "xxProblemsyy";

        Assert.Equal(SummaryLevel.Problems, Enum.Parse<SummaryLevel>(Text.AsSpan(2, 8)));

        Assert.True(Enum.TryParse<SummaryLevel>(Text.AsSpan(2, 8), out var cut));
        Assert.Equal(SummaryLevel.Problems, cut);

        Assert.False(Enum.TryParse<SummaryLevel>(Text.AsSpan(0, 8), out _));
        Assert.False(Enum.TryParse<SummaryLevel>(Text.AsSpan(0, 4), out _));

        var outside = Assert.Throws<ArgumentOutOfRangeException>(() => _ = Enum.Parse<SummaryLevel>(Text.AsSpan(12, 8)));

        Assert.Equal("start", outside.ParamName);
    }

    /// <summary>A span of text that is no level, in any case and however it is spelled: nothing at all, a space, a
    /// tab, a prefix of a name, a name with something behind it, a name with a space inside it, a fraction, a
    /// hexadecimal, an exponent, a list with a number in it, and a number past either end of the
    /// <see cref="int" /> the four stand on - the largest number the type holds read, the one past it refused. Every
    /// refusal leaves <see cref="SummaryLevel.Empty" /> behind in the out parameter: the first of the four, and the
    /// one a refused read cannot be told apart from.</summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("Pendi")]
    [InlineData("Problems!")]
    [InlineData("All healthy")]
    [InlineData("3.0")]
    [InlineData("0x3")]
    [InlineData("1e1")]
    [InlineData("1,2")]
    [InlineData("Pending,Nope")]
    [InlineData("2147483648")]
    [InlineData("-2147483649")]
    [InlineData("4294967296")]
    public void ASpanOfTextThatIsNoLevelIsRefusedAndLeavesEmptyBehind(string text)
    {
        Assert.False(Enum.TryParse<SummaryLevel>(text.AsSpan(), out var read));
        Assert.Equal(SummaryLevel.Empty, read);

        Assert.False(Enum.TryParse<SummaryLevel>(text.AsSpan(), ignoreCase: false, out read));
        Assert.Equal(SummaryLevel.Empty, read);
    }

    /// <summary>The case, forgiven on the ask and on the ask alone: a word in another case is no level to a span read
    /// that was not told otherwise, and is the level it names to one that was - part by part in a list as much as on
    /// its own, and forgiven through the spaces on either side of it as well.</summary>
    [Theory]
    [InlineData("problems", SummaryLevel.Problems)]
    [InlineData("ALLHEALTHY", SummaryLevel.AllHealthy)]
    [InlineData("pending, allhealthy", SummaryLevel.Problems)]
    public void OnlyASpanReadToldToForgiveTheCaseForgivesIt(string text, SummaryLevel expected)
    {
        Assert.False(Enum.TryParse<SummaryLevel>(text.AsSpan(), out _));

        Assert.True(Enum.TryParse<SummaryLevel>(text.AsSpan(), ignoreCase: true, out var forgiven));
        Assert.Equal(expected, forgiven);

        Assert.Equal(expected, Enum.Parse<SummaryLevel>(text.AsSpan(), ignoreCase: true));

        Assert.True(Enum.TryParse<SummaryLevel>($"  {text}  ".AsSpan(), ignoreCase: true, out var padded));
        Assert.Equal(expected, padded);
    }

    /// <summary>What <c>Enum.Parse</c> over a span says of text that is no level, which is the two ways it says it
    /// over a string: nothing in the way of a level at all - no text, spaces, a tab - is text without a level in it,
    /// and a word that is no level is a value that was not found, said back in the fault as it was written.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void ParsingASpanWithNothingInTheWayOfALevelThrows(string text)
    {
        var fault = Assert.Throws<ArgumentException>(() => _ = Enum.Parse<SummaryLevel>(text.AsSpan()));

        Assert.Equal("value", fault.ParamName);
        Assert.Contains("Must specify valid information", fault.Message, StringComparison.Ordinal);
    }

    /// <summary>A word in the right case that is still no level, a word in the wrong one refused by a read that
    /// forgives nothing, and a list with a number in it are all <see cref="ArgumentException" />s naming the value
    /// they could not find.</summary>
    [Theory]
    [InlineData("Problem")]
    [InlineData("problems")]
    [InlineData("Pending,2")]
    public void ParsingASpanThatNamesNoLevelSaysWhichValueWasNotFound(string text)
    {
        var fault = Assert.Throws<ArgumentException>(() => _ = Enum.Parse<SummaryLevel>(text.AsSpan()));

        Assert.Contains(text, fault.Message, StringComparison.Ordinal);
        Assert.Contains("was not found", fault.Message, StringComparison.Ordinal);
    }

    // ----- names joined with a comma, which the four are no set of

    /// <summary>A list of names joined with a comma is added together and the sum read as a level: the pending check
    /// and the healthy round make <see cref="SummaryLevel.Problems" />, which is nothing of the sort, because the
    /// comma is an or of numbers and the four carry no <see cref="FlagsAttribute" /> to make it mean anything else
    /// (<see cref="SummaryLevelEdgeCaseTests" /> pins that they carry none). Spaces stand between the parts as they
    /// stand around the whole, and a name written twice is the name written once.</summary>
    [Theory]
    [InlineData("Empty,Pending", SummaryLevel.Pending, 1)]
    [InlineData("Pending,Empty", SummaryLevel.Pending, 1)]
    [InlineData("Pending,AllHealthy", SummaryLevel.Problems, 3)]
    [InlineData("AllHealthy,Pending", SummaryLevel.Problems, 3)]
    [InlineData("Empty,Problems", SummaryLevel.Problems, 3)]
    [InlineData("Problems, Problems", SummaryLevel.Problems, 3)]
    [InlineData("Pending,Pending", SummaryLevel.Pending, 1)]
    [InlineData("Empty,Empty", SummaryLevel.Empty, 0)]
    [InlineData("Empty,Pending,AllHealthy", SummaryLevel.Problems, 3)]
    [InlineData("  Pending ,  AllHealthy  ", SummaryLevel.Problems, 3)]
    public void NamesJoinedWithACommaAreAddedTogether(string text, SummaryLevel expected, int number)
    {
        Assert.True(Enum.TryParse<SummaryLevel>(text, out var read), $"\"{text}\" was refused");
        Assert.Equal(expected, read);
        Assert.Equal(number, (int)read);

        Assert.Equal(expected, Enum.Parse<SummaryLevel>(text.AsSpan()));
    }

    /// <summary>However two of the four are joined they land back on one of the four, because the numbers they stand
    /// on - 0, 1, 2 and 3 - are two bits that only ever make each other: sixteen pairs, every one of them the or-sum
    /// of the two numbers, and none of them a level the enum does not name.</summary>
    [Fact]
    public void EveryPairOfTheFourJoinedWithACommaLandsOnOneOfTheFour()
    {
        var four = Enum.GetValues<SummaryLevel>();

        foreach (var first in four)
        {
            foreach (var second in four)
            {
                Assert.True(Enum.TryParse<SummaryLevel>($"{first},{second}", out var joined), $"{first},{second} was refused");

                Assert.Contains(joined, four);
                Assert.Equal((int)first | (int)second, (int)joined);
            }
        }
    }

    /// <summary>A list the read refuses: a comma with nothing on one side of it, a number written among the names -
    /// the list is a list of names, and the numbers are no part of it, not even the number one of the names stands
    /// on - and a name the four do not answer to anywhere in it. The refusal is the same out of a span as out of a
    /// string, and forgiving the case forgives none of it.</summary>
    [Theory]
    [InlineData("Empty,")]
    [InlineData(",Empty")]
    [InlineData("Pending, ")]
    [InlineData("Problems,")]
    [InlineData("4,Empty")]
    [InlineData("Pending,2")]
    [InlineData("0,4")]
    [InlineData("1,2")]
    [InlineData("Empty,NothingAtAll")]
    public void AListThatIsNoNamesOfTheFourIsRefused(string text)
    {
        Assert.False(Enum.TryParse<SummaryLevel>(text, out var read));
        Assert.Equal(SummaryLevel.Empty, read);

        Assert.False(Enum.TryParse<SummaryLevel>(text.AsSpan(), out _));
        Assert.False(Enum.TryParse<SummaryLevel>(text.AsSpan(), ignoreCase: true, out _));
    }

    // ----- the same two reads taken on the type rather than on a type argument

    /// <summary>The overloads told which enum on the side of the call: they hand back a boxed level on a read that
    /// answered, whatever the text was - the word, the number, a number in sign, a number in a list - and box a
    /// number outside the four the same way as one inside them, of the type itself and not of another.</summary>
    [Theory]
    [InlineData("Pending")]
    [InlineData("Problems")]
    [InlineData("1")]
    [InlineData("+2")]
    [InlineData(" Pending,AllHealthy ")]
    [InlineData("4")]
    [InlineData("-1")]
    [InlineData("2147483647")]
    public void TheTypeTakingReadHandsBackABoxedLevelEvenWhereTheLevelIsNoLevel(string text)
    {
        Assert.True(Enum.TryParse(typeof(SummaryLevel), text, out var read), $"\"{text}\" was refused");

        Assert.NotNull(read);
        Assert.IsType<SummaryLevel>(read);
        Assert.Equal(Enum.Parse<SummaryLevel>(text), read);
        Assert.Equal(Enum.IsDefined((SummaryLevel)read!), Enum.IsDefined(typeof(SummaryLevel), read));
        Assert.Equal<object>(read!, Enum.ToObject(typeof(SummaryLevel), (int)(SummaryLevel)read!));
    }

    /// <summary>A type-taking read that answers no hands back <c>nothing at all</c> rather than the first of the four:
    /// where the generic read leaves <see cref="SummaryLevel.Empty" /> in its out parameter and no caller can tell a
    /// refusal from the empty reading, this one says so in the reference itself. Nothing at all as the text, a tab of
    /// a text, another case and a number past either end are all refused alike.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("\t")]
    [InlineData("problem")]
    [InlineData("problems")]
    [InlineData("2147483648")]
    public void ARefusedTypeTakingReadHandsBackNothingAtAll(string? text)
    {
        Assert.False(Enum.TryParse(typeof(SummaryLevel), text, out var read));
        Assert.Null(read);
    }

    /// <summary>What the type-taking reads forgive and what they refuse: the case, once told to; a name cut out of
    /// padded text by a span; a number in sign and a number outside the four - and, refused outright rather than
    /// answered no, a type that is no type at all (<c>enumType</c>), a type that is no enum (<c>enumType</c> again,
    /// by the read of a value as much as by the count of the four) and nothing at all as the text of a
    /// <c>Parse</c> (<c>value</c>), which the same nothing in a <c>TryParse</c> only answers no.</summary>
    [Fact]
    public void TheTypeTakingReadsForgiveWhatTheyAreToldToAndRefuseTheRest()
    {
        Assert.False(Enum.TryParse(typeof(SummaryLevel), "problems", out _));
        Assert.True(Enum.TryParse(typeof(SummaryLevel), "problems", ignoreCase: true, out var forgiven));
        Assert.Equal(SummaryLevel.Problems, forgiven);

        Assert.Equal(SummaryLevel.Problems, Parsed(typeof(SummaryLevel), "  Problems  ".AsSpan(1, 10)));
        Assert.Equal(SummaryLevel.Problems, Parsed(typeof(SummaryLevel), "+3"));
        Assert.Equal((SummaryLevel)4, Parsed(typeof(SummaryLevel), "4"));

        Assert.False(Enum.TryParse(typeof(SummaryLevel), default(ReadOnlySpan<char>), out var nothing));
        Assert.Null(nothing);

        var noType = Assert.Throws<ArgumentNullException>(() => _ = Parsed(null!, "Problems"));
        Assert.Equal("enumType", noType.ParamName);

        Assert.Throws<ArgumentNullException>(() => Enum.TryParse(null!, "Problems", out _));

        var notAnEnum = Assert.Throws<ArgumentException>(() => _ = Parsed(typeof(int), "Problems"));
        Assert.Equal("enumType", notAnEnum.ParamName);
        Assert.Contains("must be an Enum", notAnEnum.Message, StringComparison.Ordinal);

        Assert.Throws<ArgumentException>(() => Enum.TryParse(typeof(int), "Problems", out _));

        var noText = Assert.Throws<ArgumentNullException>(() => _ = Parsed(typeof(SummaryLevel), (string?)null));
        Assert.Equal("value", noText.ParamName);

        var nothingToParse = Assert.Throws<ArgumentException>(() => _ = Parsed(typeof(SummaryLevel), ReadOnlySpan<char>.Empty));
        Assert.Equal("value", nothingToParse.ParamName);
    }

    // ----- a level written into a span of text

    /// <summary>The same three sayings of a level - its own word, its own number, its own number in eight hexadecimal
    /// digits - written straight into a buffer instead of out as a string, and saying the very words
    /// <see cref="SummaryLevelBoundaryEdgeCaseTests" /> pins of the string route: the eight formats in pairs, no
    /// format at all and the empty one forgiven as the word, a number outside the four saying its own number in
    /// every one of them, and the interface the four implement writing what the static ask writes.</summary>
    [Theory]
    [InlineData(SummaryLevel.Empty, "Empty", "0", "00000000")]
    [InlineData(SummaryLevel.Pending, "Pending", "1", "00000001")]
    [InlineData(SummaryLevel.AllHealthy, AllHealthyWord, "2", "00000002")]
    [InlineData(SummaryLevel.Problems, "Problems", "3", "00000003")]
    [InlineData((SummaryLevel)4, "4", "4", "00000004")]
    [InlineData((SummaryLevel)(-1), "-1", "-1", "FFFFFFFF")]
    [InlineData((SummaryLevel)255, "255", "255", "000000FF")]
    [InlineData((SummaryLevel)int.MinValue, "-2147483648", "-2147483648", "80000000")]
    [InlineData((SummaryLevel)int.MaxValue, "2147483647", "2147483647", "7FFFFFFF")]
    public void ALevelWritesItsWordItsNumberOrItsDigits(SummaryLevel level, string word, string number, string hex)
    {
        foreach (var ask in new[] { "g", "G", "f", "F" })
        {
            Assert.Equal(word, Written(level, ask));
        }

        foreach (var ask in new[] { "d", "D" })
        {
            Assert.Equal(number, Written(level, ask));
        }

        foreach (var ask in new[] { "x", "X" })
        {
            Assert.Equal(hex, Written(level, ask));
        }

        // No format at all, and the empty one, are the word - and the word is the one the level says of itself.
        Assert.Equal(word, Written(level, null));
        Assert.Equal(word, Written(level, string.Empty));
        Assert.Equal(level.ToString(), Written(level, "g"));
        Assert.Equal(((IFormattable)level).ToString("d", CultureInfo.InvariantCulture), Written(level, "d"));

        // The interface the four implement, taken on the box, writes the same characters in the same number.
        var buffer = new char[64];

        Assert.True(((ISpanFormattable)level).TryFormat(buffer, out var written, "x", CultureInfo.InvariantCulture));
        Assert.Equal(hex, new string(buffer, 0, written));
    }

    /// <summary>Room enough, or room one character short: the writing is all of the word or none of it, so a buffer
    /// of exactly the length the ask needs is worth as much as a buffer of sixty-four and one character less is worth
    /// nothing at all - the count it answers with being the length of what it wrote, and nothing when it wrote
    /// nothing.</summary>
    [Theory]
    [InlineData(SummaryLevel.Empty, "g", 5)]
    [InlineData(SummaryLevel.AllHealthy, "g", 10)]
    [InlineData(SummaryLevel.Problems, "d", 1)]
    [InlineData(SummaryLevel.Pending, "x", 8)]
    [InlineData((SummaryLevel)(-1), "d", 2)]
    [InlineData((SummaryLevel)int.MinValue, "d", 11)]
    [InlineData((SummaryLevel)int.MaxValue, "x", 8)]
    public void ABufferOfExactlyWhatTheAskNeedsIsEnoughAndOneLessIsNot(SummaryLevel level, string format, int needed)
    {
        var fits = Fit(level, format, needed);

        Assert.True(fits.Wrote);
        Assert.Equal(needed, fits.Written);

        var tooSmall = Fit(level, format, needed - 1);

        Assert.False(tooSmall.Wrote);
        Assert.Equal(0, tooSmall.Written);

        Assert.False(Fit(level, format, 0).Wrote);
    }

    /// <summary>A buffer that is too small is left exactly as it was found: nothing written into it, nothing counted
    /// as written, and the characters that stood in it standing there still - the writing measures the word first and
    /// only then puts it down. With room in it, the same buffer takes the word and only the word, and what stood
    /// behind the ten characters stands behind them still.</summary>
    [Fact]
    public void ABufferTooSmallIsLeftAsItWasFound()
    {
        var buffer = new string('X', 16).ToCharArray();

        Assert.False(Enum.TryFormat(SummaryLevel.AllHealthy, buffer.AsSpan(0, AllHealthyWord.Length - 1), out var written, "g"));
        Assert.Equal(0, written);
        Assert.Equal("XXXXXXXXXXXXXXXX", new string(buffer));

        Assert.True(Enum.TryFormat(SummaryLevel.AllHealthy, buffer.AsSpan(), out written, "g"));
        Assert.Equal(AllHealthyWord.Length, written);
        Assert.Equal(AllHealthyWord, new string(buffer, 0, written));
        Assert.Equal("XXXXXX", new string(buffer, AllHealthyWord.Length, 16 - AllHealthyWord.Length));
    }

    /// <summary>A format that is none of the eight is refused the way it is refused out of a string - with a
    /// <see cref="FormatException" /> and not with a no - by the static ask and by the interface alike, and before
    /// the room in the buffer is ever weighed: an empty buffer and a wrong format is a wrong format.</summary>
    [Theory]
    [InlineData("n")]
    [InlineData("N")]
    [InlineData("0")]
    [InlineData("x2")]
    [InlineData("g ")]
    [InlineData("dd")]
    [InlineData(" ")]
    [InlineData("@")]
    public void ASpanWrittenIntoRefusesAFormatThatIsNoFormat(string format)
    {
        var buffer = new char[64];

        var fault = Assert.Throws<FormatException>(() => Enum.TryFormat(SummaryLevel.Problems, buffer, out _, format));
        Assert.Contains("can be only", fault.Message, StringComparison.Ordinal);

        Assert.Throws<FormatException>(() => ((ISpanFormattable)SummaryLevel.Problems).TryFormat(buffer, out _, format, CultureInfo.InvariantCulture));
        Assert.Throws<FormatException>(() => Enum.TryFormat(SummaryLevel.Problems, [], out _, format));
    }

    /// <summary>None of it turns for another reader: a reader whose own digits are Arabic-Indic, one whose negative
    /// sign carries a mark of its own, the German reader the rest of the tests ask, and the invariant one are all
    /// written the same characters - the same hyphen-minus, the same six letters above nine - because the word and
    /// the number of a level are the page's own and not the viewer's.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("de-DE")]
    [InlineData("fr-FR")]
    [InlineData("ar-SA")]
    public void ALevelWritesTheSameCharactersForEveryReader(string reader)
    {
        var culture = new CultureInfo(reader);
        var buffer = new string('X', 64).ToCharArray();

        Assert.True(((ISpanFormattable)(SummaryLevel)int.MinValue).TryFormat(buffer, out var written, "d", culture));
        Assert.Equal("-2147483648", new string(buffer, 0, written));
        Assert.Equal('-', buffer[0]);

        Assert.True(((ISpanFormattable)(SummaryLevel)255).TryFormat(buffer, out written, "x", culture));
        Assert.Equal("000000FF", new string(buffer, 0, written));

        Assert.True(((ISpanFormattable)SummaryLevel.AllHealthy).TryFormat(buffer, out written, "g", culture));
        Assert.Equal(AllHealthyWord, new string(buffer, 0, written));

        Assert.Equal("-2147483648", ((IFormattable)(SummaryLevel)int.MinValue).ToString("d", culture));
        Assert.Equal("Problems", ((IFormattable)SummaryLevel.Problems).ToString("g", culture));
    }

    // ----- the four as the type is compiled

    /// <summary>What reflection sees of the four: an enum of <see cref="int" /> standing on <see cref="Enum" />,
    /// carrying the framework's four interfaces and nothing the file adds - no method of its own, no property, no
    /// constructor, no event, no nested type, no attribute on the type or on any of its four names, and no instance
    /// field but the <c>value__</c> the compiler puts there. The levels are the four public static literal fields,
    /// their raw constant values the numbers 0 to 3, and every one of them the very level its own name says - boxed
    /// out of the field and named back by the number it holds.</summary>
    [Fact]
    public void TheLevelTypeDeclaresNothingOfItsOwnBeyondItsFourNames()
    {
        var type = typeof(SummaryLevel);

        Assert.True(type.IsEnum);
        Assert.True(type.IsValueType);
        Assert.False(type.IsAbstract);
        Assert.False(type.IsInterface);
        Assert.Equal(typeof(Enum), type.BaseType);
        Assert.Equal(TypeCode.Int32, Type.GetTypeCode(type));
        Assert.Equal(typeof(int), type.GetEnumUnderlyingType());
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(type));
        Assert.Equal(
            ["IComparable", "IConvertible", "IFormattable", "ISpanFormattable"],
            type.GetInterfaces().Select(iface => iface.Name).Order(StringComparer.Ordinal));

        Assert.Empty(type.GetCustomAttributes());
        Assert.Empty(type.GetNestedTypes());
        Assert.Empty(type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        Assert.Empty(type.GetMethods(DeclaredOnly));
        Assert.Empty(type.GetProperties(DeclaredOnly));
        Assert.Empty(type.GetEvents(DeclaredOnly));

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

        Assert.Equal(["Empty", "Pending", "AllHealthy", "Problems"], fields.Select(field => field.Name));
        Assert.Equal([0, 1, 2, 3], fields.Select(field => (int)field.GetRawConstantValue()!));

        foreach (var field in fields)
        {
            Assert.True(field.IsLiteral);
            Assert.True(field.IsStatic);
            Assert.False(field.IsInitOnly);
            Assert.Equal(type, field.FieldType);
            Assert.Empty(field.GetCustomAttributes());

            var boxed = field.GetValue(null);

            Assert.IsType<SummaryLevel>(boxed);
            Assert.Equal(field.Name, ((Enum)boxed!).ToString());
            Assert.Equal(field.Name, type.GetEnumName(boxed));
        }

        var storage = type.GetField("value__", BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(storage);
        Assert.Equal(typeof(int), storage!.FieldType);
        Assert.False(storage.IsLiteral);
        Assert.False(storage.IsStatic);
    }

    /// <summary>The four handed out as arrays the caller may do what it likes with: a <c>SummaryLevel[]</c> of the
    /// levels themselves, an <c>int[]</c> of the numbers under them and a <c>string[]</c> of the names - every one of
    /// them a fresh copy on every ask, so overwriting what was handed out changes nothing about the next call and
    /// nothing about the type. The array of the levels is one-dimensional, of the length of the four and of their own
    /// type, and reading past its end is the reader's own fault told as such.</summary>
    [Fact]
    public void TheFourAreHandedOutAsCopiesTheCallerMayRearrange()
    {
        var type = typeof(SummaryLevel);

        var levels = (SummaryLevel[])type.GetEnumValues();

        Assert.Equal([SummaryLevel.Empty, SummaryLevel.Pending, SummaryLevel.AllHealthy, SummaryLevel.Problems], levels);

        levels[0] = SummaryLevel.Problems;

        Assert.Equal(SummaryLevel.Empty, Enum.GetValues<SummaryLevel>()[0]);
        Assert.Equal(SummaryLevel.Empty, ((SummaryLevel[])type.GetEnumValues())[0]);
        Assert.NotSame(levels, (SummaryLevel[])type.GetEnumValues());

        var numbers = (int[])Enum.GetValuesAsUnderlyingType<SummaryLevel>();

        Assert.Equal([0, 1, 2, 3], numbers);

        numbers[3] = 9;

        Assert.Equal([0, 1, 2, 3], (int[])type.GetEnumValuesAsUnderlyingType());
        Assert.Equal(typeof(int), ((Array)type.GetEnumValuesAsUnderlyingType()).GetType().GetElementType());

        var names = Enum.GetNames(type);

        names[0] = "NothingAtAll";

        Assert.Equal(["Empty", "Pending", "AllHealthy", "Problems"], Enum.GetNames(type));
        Assert.Equal(["Empty", "Pending", "AllHealthy", "Problems"], type.GetEnumNames());

        var handed = (Array)Enum.GetValues(type);

        Assert.Equal(1, handed.Rank);
        Assert.Equal(4, handed.Length);
        Assert.Equal(typeof(SummaryLevel), handed.GetType().GetElementType());
        Assert.Equal(SummaryLevel.Problems, handed.GetValue(3));
        Assert.Throws<IndexOutOfRangeException>(() => _ = handed.GetValue(4));
    }

    /// <summary>The name of a level, asked of the type with the number in it: the number is taken in whichever whole
    /// size it arrives - <see cref="sbyte" />, <see cref="byte" />, <see cref="short" />, <see cref="ushort" />,
    /// <see cref="int" />, <see cref="uint" />, <see cref="long" /> or <see cref="ulong" /> - where
    /// <c>Enum.IsDefined</c> is the strict one that refuses another size outright (pinned in
    /// <see cref="SummaryLevelBoundaryEdgeCaseTests" />). A number the four do not name has no name and is answered
    /// with <c>null</c> rather than with the number, and a text, a fraction or a <see cref="bool" /> is refused as no
    /// number an enum was ever built on.</summary>
    [Fact]
    public void ALevelIsNamedFromAnyWholeNumberAndRefusesAnythingElse()
    {
        var type = typeof(SummaryLevel);

        Assert.Equal(
            ["Empty", "Pending", "AllHealthy", "Problems"],
            LevelNumbers.Select(number => type.GetEnumName(number)!));

        Assert.Equal("Problems", Enum.GetName(typeof(SummaryLevel), 3));
        Assert.Null(type.GetEnumName(4));
        Assert.Null(type.GetEnumName(-1));
        Assert.Null(type.GetEnumName(int.MaxValue));
        Assert.Null(Enum.GetName(typeof(SummaryLevel), 4));

        foreach (var number in ATwoInEveryWholeSize)
        {
            Assert.Equal(AllHealthyWord, Enum.GetName(type, number));
        }

        Assert.Equal(AllHealthyWord, type.GetEnumName((long)2));

        foreach (var notANumber in NotWholeNumbers)
        {
            var fault = Assert.Throws<ArgumentException>(() => Enum.GetName(type, notANumber));

            Assert.Equal("value", fault.ParamName);
            Assert.Contains("must be an enum base or an underlying type", fault.Message, StringComparison.Ordinal);
        }

        Assert.Throws<ArgumentNullException>(() => _ = Enum.GetName(null!, 3));
        Assert.Throws<ArgumentException>(() => _ = Enum.GetName(typeof(int), 3));
        Assert.Throws<ArgumentNullException>(() => Enum.GetUnderlyingType(null!));
        Assert.Throws<ArgumentException>(() => Enum.GetValues(typeof(int)));
        Assert.Throws<ArgumentException>(() => Enum.GetNames(typeof(int)));
        Assert.Throws<ArgumentException>(() => Enum.GetUnderlyingType(typeof(int)));
    }
}
