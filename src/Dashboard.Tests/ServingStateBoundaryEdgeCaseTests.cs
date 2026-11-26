using System.Globalization;

using static Dashboard.Tests.HealthStates;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ServingState" /> that the files beside it leave open,
/// declared in <c>src/Dashboard/Health/ServingAssessment.cs</c>. <see cref="ServingStateEdgeCaseTests" /> already pins
/// the five as the numbers, the names and the order the declaration writes them, the unset state, the text that is and
/// is not a state, the arithmetic of the numbers underneath and the surface the enum declares of itself;
/// <see cref="ServingAssessmentTests" />, <see cref="RotationTests" /> and
/// <see cref="ServingAssessmentEdgeCaseTests" /> pin what each state is worth - the headline, the failover detail and
/// the lists an assessment reads off it; and <see cref="FrontDoorAgreementEdgeCaseTests" /> pins the five as the rows of
/// the table a Front Door answer is compared against, each of them reached by a shape of nodes.
/// <para>
/// What none of those says, and what follows: how one of the five words itself when it is asked how - its own word, its
/// number, that number in the eight digits an <see cref="int" /> wears - and every other format refused, by the state
/// and by <c>Enum.Format</c> alike, which forgives a bare number, refuses the word handed to it to format, and refuses
/// the empty format the state itself forgives; the one format that reads the bits rather than the five words, which
/// spells a number the five do not name as a combination no caller meant and leaves a number reaching outside them to
/// say its number after all; the words and the numbers that do not turn for a German reader; the
/// number, the word, the <see cref="TypeCode" /> and the state again that the framework makes of one, and the
/// <see langword="false" /> that only the absence of nodes makes of a weighing; the two ways of making a state out of a
/// number that answer differently, one cutting a <see cref="long" /> down to the size the five stand on and forgiving a
/// <see cref="bool" /> besides, while the other refuses a number of another size outright; a name spelled with another
/// size, which <c>Enum.GetName</c> reads and <c>Enum.IsDefined</c> will not; the asks that name no type at all, which
/// say the parameter they stumbled on; text spelling two states with a comma between them, read as one state the enum
/// never named; a state weighed against nothing at all by the one comparison it carries; the state that is not there at
/// all, which no assessment can hold; and the sweep over the shapes a topology takes, which says that a decision lands
/// on one of the five and never on anything else.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every format, number, word and node below
/// is one the test brought, and the four hundred and eighty decisions swept at the end are made of fixed nodes and
/// fixed endpoint answers.
/// </para>
/// </summary>
public class ServingStateBoundaryEdgeCaseTests
{
    /// <summary>The five states, in the order the enum declares them.</summary>
    private static readonly ServingState[] Five =
        [ServingState.NoNodes, ServingState.Pending, ServingState.Primary, ServingState.FailedOver, ServingState.Down];

    /// <summary>Every answer an endpoint can bring back, and the absence of an endpoint among them.</summary>
    private static readonly HealthState?[] Endpoints = [null, Pending, Healthy, Unhealthy, Unreachable];

    private static NodeHealth Primary(HealthState state, string region = "westus3") => new($"app-{region}", region, true, state);

    private static NodeHealth Standby(HealthState state, string region = "eastus2") => new($"app-{region}", region, false, state);

    private static NodeHealth Only(HealthState state) => new("app-one", null, true, state);

    /// <summary>The one <see cref="Type" /> a caller holds when it knows the vocabulary by reflection and has no state
    /// in hand: the type a state names of itself, which is the whole of what the asks that take a type are given.</summary>
    private static readonly Type Vocabulary = ServingState.Down.GetType();

    // ----- how a state words itself, asked how

    /// <summary>The three ways of saying a state: the word the enum spells it, the number it stands on, and that number
    /// in eight hexadecimal digits. A state that wears no flags answers an ask for flags with its plain word, since
    /// there is no set to spell out - and so does it when it is asked with no format at all, none and the empty one. A
    /// state the five do not name has no word of its own and says its number, except to the one ask that reads the bits
    /// instead of the five words, which spells even that as a combination - pinned cell by cell below.</summary>
    [Theory]
    [InlineData(ServingState.NoNodes, "NoNodes", "0", "00000000", "NoNodes")]
    [InlineData(ServingState.Pending, "Pending", "1", "00000001", "Pending")]
    [InlineData(ServingState.Primary, "Primary", "2", "00000002", "Primary")]
    [InlineData(ServingState.FailedOver, "FailedOver", "3", "00000003", "FailedOver")]
    [InlineData(ServingState.Down, "Down", "4", "00000004", "Down")]
    [InlineData((ServingState)5, "5", "5", "00000005", "Pending, Down")]
    [InlineData((ServingState)(-1), "-1", "-1", "FFFFFFFF", "-1")]
    public void AStateSaysItsWordItsNumberOrThoseDigitsWhenAskedHow(
        ServingState state,
        string word,
        string number,
        string digits,
        string flags)
    {
        var asked = (IFormattable)state;

        Assert.Equal(word, state.ToString());
        Assert.Equal(word, $"{state}");
        Assert.Equal(word, asked.ToString(null, CultureInfo.InvariantCulture));
        Assert.Equal(word, asked.ToString(string.Empty, CultureInfo.InvariantCulture));
        Assert.Equal(word, asked.ToString("G", CultureInfo.InvariantCulture));
        Assert.Equal(word, asked.ToString("g", CultureInfo.InvariantCulture));
        Assert.Equal(word, Enum.Format(typeof(ServingState), state, "G"));

        Assert.Equal(number, asked.ToString("D", CultureInfo.InvariantCulture));
        Assert.Equal(number, asked.ToString("d", CultureInfo.InvariantCulture));

        Assert.Equal(digits, asked.ToString("X", CultureInfo.InvariantCulture));
        Assert.Equal(digits, asked.ToString("x", CultureInfo.InvariantCulture));
        // The case of the format letter is not looked at: the digits come capital whichever way the ask was spelt.
        Assert.Equal(digits, $"{state:X}");
        Assert.Equal(digits, $"{state:x}");

        // The one format that looks at the bits rather than at the five: pinned cell by cell below.
        Assert.Equal(flags, asked.ToString("F", CultureInfo.InvariantCulture));
        Assert.Equal(flags, asked.ToString("f", CultureInfo.InvariantCulture));
        Assert.Equal(flags, Enum.Format(typeof(ServingState), state, "F"));
    }

    /// <summary>The ask for flags is the one format that reads the bits instead of the five words, and it does so
    /// although the enum wears no <see cref="FlagsAttribute" />: a number the five do not name is spelled out as the
    /// names its bits are made of, the longest of them first - seven is <c>FailedOver, Down</c> rather than the three
    /// names its bits could also be read as, and five is <c>Pending, Down</c>, a word no caller meant - while a number
    /// the five do name keeps its own word rather than being taken apart. A number whose bits reach past everything the
    /// four words that are not <see cref="ServingState.NoNodes" /> cover - the number below the first, the top of
    /// <see cref="int" /> - is left to say its number after all, in the general format and in that one alike.</summary>
    [Theory]
    [InlineData(0, "NoNodes", "NoNodes")]
    [InlineData(1, "Pending", "Pending")]
    [InlineData(3, "FailedOver", "FailedOver")]
    [InlineData(4, "Down", "Down")]
    [InlineData(5, "Pending, Down", "5")]
    [InlineData(6, "Primary, Down", "6")]
    [InlineData(7, "FailedOver, Down", "7")]
    [InlineData(-1, "-1", "-1")]
    [InlineData(int.MaxValue, "2147483647", "2147483647")]
    public void TheAskForFlagsSpellsOutTheBitsAndNotTheVocabulary(int number, string flags, string plain)
    {
        var state = (ServingState)number;

        Assert.Equal(flags, ((IFormattable)state).ToString("F", CultureInfo.InvariantCulture));
        Assert.Equal(flags, Enum.Format(typeof(ServingState), number, "F"));
        Assert.Equal(plain, state.ToString());
        Assert.Equal(plain, ((IFormattable)state).ToString("G", CultureInfo.InvariantCulture));
    }

    /// <summary>The formats a state answers are those eight letters and nothing besides: a width is refused, a space is
    /// refused, a second letter of the same kind is refused - by the state itself and by <c>Enum.Format</c> alike, so
    /// neither of the two ways of asking is kinder than the other about a format nobody wrote.</summary>
    [Theory]
    [InlineData("n")]
    [InlineData("N")]
    [InlineData("0")]
    [InlineData("x2")]
    [InlineData("G4")]
    [InlineData("D2")]
    [InlineData("X2")]
    [InlineData("dd")]
    [InlineData("gg")]
    [InlineData("G ")]
    [InlineData("g ")]
    [InlineData(" ")]
    [InlineData("@")]
    public void AFormatThatIsNoFormatOfAStateIsRefusedBothWays(string format)
    {
        Assert.Throws<FormatException>(() => ((IFormattable)ServingState.FailedOver).ToString(format, CultureInfo.InvariantCulture));
        Assert.Throws<FormatException>(() => Enum.Format(typeof(ServingState), ServingState.FailedOver, format));
    }

    /// <summary>The three places the two ways of asking leave hold of each other: <c>Enum.Format</c> refuses the empty
    /// format a state forgives, takes the bare number under a state as readily as the state, and refuses a word handed
    /// to it to format - which is an ask about the number, not about the word.</summary>
    [Fact]
    public void EnumFormatRefusesWhatAStateForgivesAndAsksTheNumberNotTheWord()
    {
        Assert.Throws<FormatException>(() => Enum.Format(typeof(ServingState), ServingState.Down, string.Empty));
        Assert.Equal("Down", ((IFormattable)ServingState.Down).ToString(string.Empty, CultureInfo.InvariantCulture));

        Assert.Equal("Pending", Enum.Format(typeof(ServingState), 1, "g"));
        Assert.Equal("00000003", Enum.Format(typeof(ServingState), 3, "X"));
        Assert.Equal("5", Enum.Format(typeof(ServingState), (ServingState)5, "g"));
        Assert.Equal("FFFFFFFF", Enum.Format(typeof(ServingState), (ServingState)(-1), "X"));
        Assert.Equal("FailedOver", Enum.Format(typeof(ServingState), (object)ServingState.FailedOver, "G"));

        Assert.Throws<ArgumentException>(() => Enum.Format(typeof(ServingState), "Down", "g"));
    }

    /// <summary>The ends of <see cref="int" /> and the numbers about them, said in the two formats of number: the
    /// decimal spells the sign and the digits as they stand, the hexadecimal the thirty-two bits, and nothing at either
    /// end can be mistaken for one of the five words.</summary>
    [Fact]
    public void TheEndsOfTheNumberUnderTheFiveAreSaidAsTheNumberTheyAre()
    {
        Assert.Equal("255", ((IFormattable)(ServingState)255).ToString("G", CultureInfo.InvariantCulture));
        Assert.Equal("000000FF", ((IFormattable)(ServingState)255).ToString("x", CultureInfo.InvariantCulture));
        Assert.Equal("2147483647", ((IFormattable)(ServingState)int.MaxValue).ToString("d", CultureInfo.InvariantCulture));
        Assert.Equal("7FFFFFFF", ((IFormattable)(ServingState)int.MaxValue).ToString("X", CultureInfo.InvariantCulture));
        Assert.Equal("-2147483648", ((IFormattable)(ServingState)int.MinValue).ToString("D", CultureInfo.InvariantCulture));
        Assert.Equal("80000000", ((IFormattable)(ServingState)int.MinValue).ToString("x", CultureInfo.InvariantCulture));
    }

    /// <summary>None of the five turns for another reader: a German one is told the same word, the same number and the
    /// same eight digits, whether the ask carries the invariant culture, the reader's own or nothing at all - and reads
    /// the same word back out of text, which is the one place a number could have turned, since a German decimal mark is
    /// a comma and the comma between two names is read as a combination. The case is still forgiven only when the ask
    /// says so, whichever culture the reader brought.</summary>
    [Fact]
    public void TheWordsAndTheNumbersOfAStateDoNotTurnForAnotherReader()
    {
        ViewerCulture.German(() =>
        {
            Assert.Equal("FailedOver", ((IFormattable)ServingState.FailedOver).ToString("g", CultureInfo.CurrentCulture));
            Assert.Equal("3", ((IFormattable)ServingState.FailedOver).ToString("d", CultureInfo.CurrentCulture));
            Assert.Equal("00000003", ((IFormattable)ServingState.FailedOver).ToString("x", CultureInfo.CurrentCulture));
            Assert.Equal("-2147483648", ((IFormattable)(ServingState)int.MinValue).ToString("D", CultureInfo.CurrentCulture));
            Assert.Equal("Down", ServingState.Down.ToString());
            Assert.Equal(4, Convert.ToInt32(ServingState.Down, CultureInfo.CurrentCulture));
            Assert.Equal("Down", Convert.ToString(ServingState.Down, CultureInfo.CurrentCulture));
            Assert.Equal(ServingState.Down, Enum.Parse<ServingState>("down", ignoreCase: true));
            Assert.False(Enum.TryParse<ServingState>("down", out var refused));
            Assert.Equal(ServingState.NoNodes, refused);
        });
    }

    // ----- what a state is turned into

    /// <summary>What the framework makes of a state: the number it stands on, the word it answers to, a
    /// <see cref="TypeCode" /> of <see cref="int" />, and the state itself again - each of them coming back the same
    /// from the state, from its number, and from the object that number was put in. Every whole number a state stands on
    /// is reached the same way, so a state widened to a <see cref="long" /> and narrowed to a <see cref="short" /> says
    /// one thing, and a state changed into itself, or into an <see cref="object" />, is the very same state and not a
    /// wrapper of it.</summary>
    [Theory]
    [InlineData(ServingState.NoNodes, 0, "NoNodes")]
    [InlineData(ServingState.Pending, 1, "Pending")]
    [InlineData(ServingState.Primary, 2, "Primary")]
    [InlineData(ServingState.FailedOver, 3, "FailedOver")]
    [InlineData(ServingState.Down, 4, "Down")]
    public void AStateTurnedIntoAnythingElseIsItsNumberOrItsWord(ServingState state, int number, string word)
    {
        var convertible = (IConvertible)state;

        Assert.Equal(TypeCode.Int32, convertible.GetTypeCode());
        Assert.Equal(TypeCode.Int32, Convert.GetTypeCode(state));
        Assert.Equal(number, Convert.ToInt32(state, CultureInfo.InvariantCulture));
        Assert.Equal(word, Convert.ToString(state, CultureInfo.InvariantCulture));
        Assert.Equal((decimal)number, Convert.ToDecimal(state, CultureInfo.InvariantCulture));

        Assert.Equal(number, convertible.ToInt32(CultureInfo.InvariantCulture));
        Assert.Equal((long)number, convertible.ToInt64(CultureInfo.InvariantCulture));
        Assert.Equal((short)number, convertible.ToInt16(CultureInfo.InvariantCulture));
        Assert.Equal((sbyte)number, convertible.ToSByte(CultureInfo.InvariantCulture));
        Assert.Equal((uint)number, convertible.ToUInt32(CultureInfo.InvariantCulture));
        Assert.Equal((ulong)number, convertible.ToUInt64(CultureInfo.InvariantCulture));
        Assert.Equal((double)number, convertible.ToDouble(CultureInfo.InvariantCulture));
        Assert.Equal(word, convertible.ToString(CultureInfo.InvariantCulture));
        Assert.Equal(word, convertible.ToString(null));
        Assert.Equal(number, (int)convertible.ToType(typeof(int), CultureInfo.InvariantCulture)!);
        Assert.Equal(word, (string)convertible.ToType(typeof(string), CultureInfo.InvariantCulture)!);
        Assert.Equal(state, (ServingState)convertible.ToType(typeof(ServingState), CultureInfo.InvariantCulture)!);
        Assert.Equal(state, (ServingState)convertible.ToType(typeof(object), CultureInfo.InvariantCulture)!);

        Assert.Equal(number, (int)Convert.ChangeType(state, typeof(int), CultureInfo.InvariantCulture)!);
        Assert.Equal((long)number, Convert.ChangeType(state, typeof(long), CultureInfo.InvariantCulture));
        Assert.Equal(word, Convert.ChangeType(state, typeof(string), CultureInfo.InvariantCulture));
        Assert.Equal(state, (ServingState)Convert.ChangeType(state, typeof(ServingState), CultureInfo.InvariantCulture)!);

        Assert.Equal<object>(state, Enum.ToObject(typeof(ServingState), number));
        Assert.Equal<object>(state, Enum.ToObject(typeof(ServingState), (long)number));
    }

    /// <summary>A state weighed as a boolean is nothing only for <see cref="ServingState.NoNodes" />, whose number is
    /// zero - the trap the absence of nodes sets, since a deployable with nothing in its topology says "no" as loudly as
    /// a state nobody settled on, while every state a round can land on, the wait and the outage among them, says
    /// "yes". A state that is no state at all says "yes" as well: the weighing looks at the number and never at the
    /// five, which is why the file compares states rather than weighing them.</summary>
    [Fact]
    public void OnlyTheStateOfNoNodesSaysNoWhenItIsWeighedAsABoolean()
    {
        Assert.False(((IConvertible)ServingState.NoNodes).ToBoolean(CultureInfo.InvariantCulture));
        Assert.False(Convert.ToBoolean(ServingState.NoNodes, CultureInfo.InvariantCulture));

        foreach (var state in new[]
                 {
                     ServingState.Pending, ServingState.Primary, ServingState.FailedOver, ServingState.Down,
                     (ServingState)5, (ServingState)(-1), (ServingState)int.MinValue,
                 })
        {
            Assert.True(((IConvertible)state).ToBoolean(CultureInfo.InvariantCulture), state.ToString());
            Assert.True(Convert.ToBoolean(state, CultureInfo.InvariantCulture), state.ToString());
        }
    }

    /// <summary>The two ways the framework is asked to make a state out of a number answer differently, and only one of
    /// them checks anything: <c>Enum.ToObject</c> refuses nothing that is a number, cutting a <see cref="long" /> down
    /// to the <see cref="int" /> the five stand on - five billion falls on 705032704, and the largest
    /// <see cref="uint" /> there is on the number below the first state - and taking a <see cref="bool" /> for the
    /// <see cref="int" /> it is not, while refusing a word, even the word of a state. <c>Enum.IsDefined</c> is the
    /// strict one: it forgives a word, in the enum's own case only, and refuses a number of another size outright, a
    /// <see cref="double" /> unkindly enough to stumble over rather than answer. <c>Enum.GetName</c> is neither,
    /// forgiving every size of number and saying no word at all for a number the five do not wear.</summary>
    [Fact]
    public void AStateIsMadeFromItsOwnNumbersAndRefusesEveryOtherAsk()
    {
        Assert.Throws<ArgumentNullException>(() => Enum.ToObject(typeof(ServingState), null!));
        Assert.Throws<ArgumentException>(() => Enum.ToObject(typeof(ServingState), string.Empty));
        Assert.Throws<ArgumentException>(() => Enum.ToObject(typeof(ServingState), "Primary"));
        Assert.Throws<ArgumentException>(() => Enum.ToObject(typeof(string), 1));

        // A number of another size is not refused; it is cut down to the size the five stand on.
        Assert.Equal<object>(ServingState.Primary, Enum.ToObject(typeof(ServingState), (byte)2));
        Assert.Equal<object>(ServingState.FailedOver, Enum.ToObject(typeof(ServingState), (short)3));
        Assert.Equal<object>(ServingState.Down, Enum.ToObject(typeof(ServingState), 4L));
        Assert.Equal<object>((ServingState)705032704, Enum.ToObject(typeof(ServingState), 5_000_000_000L));
        Assert.Equal<object>((ServingState)(-1), Enum.ToObject(typeof(ServingState), uint.MaxValue));
        Assert.Equal<object>(ServingState.Pending, Enum.ToObject(typeof(ServingState), true));
        Assert.Equal<object>((ServingState)5, Enum.ToObject(typeof(ServingState), 5));
        Assert.False(Enum.IsDefined((ServingState)Enum.ToObject(typeof(ServingState), 5)));

        Assert.Throws<ArgumentNullException>(() => Enum.IsDefined(typeof(ServingState), null!));
        Assert.Throws<ArgumentException>(() => Enum.IsDefined(typeof(ServingState), (short)4));
        Assert.Throws<ArgumentException>(() => Enum.IsDefined(typeof(ServingState), 4L));
        Assert.Throws<ArgumentException>(() => Enum.IsDefined(typeof(ServingState), (object)HealthState.Pending));
        Assert.Throws<InvalidOperationException>(() => Enum.IsDefined(typeof(ServingState), 1.0));
        Assert.True(Enum.IsDefined(typeof(ServingState), 0));
        Assert.True(Enum.IsDefined(typeof(ServingState), 4));
        Assert.True(Enum.IsDefined(typeof(ServingState), (object)ServingState.FailedOver));
        Assert.False(Enum.IsDefined(typeof(ServingState), 5));
        Assert.False(Enum.IsDefined(typeof(ServingState), -1));
        Assert.True(Enum.IsDefined(typeof(ServingState), "FailedOver"));
        Assert.False(Enum.IsDefined(typeof(ServingState), "failedover"));
        Assert.False(Enum.IsDefined(typeof(ServingState), "5"));
        Assert.False(Enum.IsDefined(typeof(ServingState), string.Empty));

        Assert.Equal("FailedOver", Enum.GetName(typeof(ServingState), 3));
        Assert.Equal("FailedOver", Enum.GetName(typeof(ServingState), (short)3));
        Assert.Equal("FailedOver", Enum.GetName(typeof(ServingState), (uint)3));
        Assert.Null(Enum.GetName(typeof(ServingState), 5));
        Assert.Throws<ArgumentException>(() => Enum.GetName(typeof(ServingState), "3"));
        Assert.Throws<ArgumentNullException>(() => Enum.GetName(typeof(ServingState), null!));
    }

    /// <summary>The type read the way a caller holding only a <see cref="Type" /> reads it - the type a state names of
    /// itself, which is all a reader that came upon the vocabulary by reflection has to go on: the five names and the
    /// five values, in the order the declaration writes them, out of a fresh array at every ask; a name read whole, in
    /// the enum's case, with the spaces about it walked past - and refused unless the ask is told to forgive the case.
    /// Two names of the five spelled with a comma between them are read by the type exactly as the generic read them
    /// (<c>Pending,Primary</c> is <c>FailedOver</c>), since it is the bits underneath doing the work in both.</summary>
    [Fact]
    public void TheTypeReadsItsOwnNamesAndRefusesWhatIsNotOne()
    {
        var values = Enum.GetValues(Vocabulary);

        Assert.Equal(5, values.Length);
        Assert.Equal(Five, values.Cast<ServingState>().ToArray());
        Assert.NotSame(values, Enum.GetValues(Vocabulary));
        Assert.NotSame(Enum.GetNames(Vocabulary), Enum.GetNames(Vocabulary));
        Assert.Equal(Five, Enum.GetNames(Vocabulary).Select(name => (ServingState)Enum.Parse(Vocabulary, name)!).ToArray());

        Assert.Equal(ServingState.Down, Enum.Parse(Vocabulary, "Down"));
        Assert.Equal(ServingState.Down, Enum.Parse(Vocabulary, "down", ignoreCase: true));
        Assert.Equal(ServingState.Down, Enum.Parse(Vocabulary, " Down "));
        Assert.Equal((int)ServingState.FailedOver, (int)Enum.Parse(Vocabulary, "Pending,Primary"));
        Assert.True(Enum.TryParse(Vocabulary, "FailedOver", out var read));
        Assert.Equal((object)ServingState.FailedOver, read);
        Assert.False(Enum.TryParse(Vocabulary, "failedover", out _));
        Assert.False(Enum.TryParse(Vocabulary, "failedover", ignoreCase: false, out _));
        Assert.False(Enum.TryParse(Vocabulary, "No Nodes", out _));

        Assert.Throws<ArgumentNullException>(() => Enum.Parse(Vocabulary, null!));
        Assert.Throws<ArgumentException>(() => Enum.Parse(Vocabulary, string.Empty));
        Assert.Throws<ArgumentException>(() => Enum.Parse(Vocabulary, "   "));
        Assert.Throws<ArgumentException>(() => Enum.Parse(Vocabulary, "No Nodes"));
        Assert.Throws<ArgumentException>(() => Enum.Parse(Vocabulary, "Healthy"));
    }

    /// <summary>The asks about a state that are handed nothing to stand on: no type at all, which every one of them
    /// says the parameter of by name, and a type that is no enum - <see cref="string" />, <see cref="int" />, the
    /// record a node of the topology is - which is refused as what it is rather than read as a vocabulary with no
    /// members. The type a state names of itself is the type the five stand on, and asked for its names and its values
    /// it is handed five of each rather than an empty list.</summary>
    [Fact]
    public void AnAskThatNamesNoTypeOrNoEnumIsRefusedByName()
    {
        Assert.Throws<ArgumentNullException>(() => Enum.GetValues(null!));
        Assert.Throws<ArgumentNullException>(() => Enum.GetNames(null!));
        Assert.Throws<ArgumentNullException>(() => Enum.GetName(null!, 1));
        Assert.Throws<ArgumentNullException>(() => Enum.Format(null!, 1, "G"));
        Assert.Throws<ArgumentNullException>(() => Enum.Parse(null!, "Down"));
        Assert.Throws<ArgumentNullException>(() => Enum.TryParse(null!, "Down", out _));
        Assert.Throws<ArgumentNullException>(() => Enum.ToObject(null!, 1));
        Assert.Throws<ArgumentNullException>(() => Enum.IsDefined(null!, 1));

        Assert.Throws<ArgumentException>(() => Enum.GetValues(typeof(string)));
        Assert.Throws<ArgumentException>(() => Enum.GetNames(typeof(int)));
        Assert.Throws<ArgumentException>(() => Enum.GetName(typeof(NodeHealth), 1));
        Assert.Throws<ArgumentException>(() => Enum.ToObject(typeof(NodeHealth), 1));
        Assert.Throws<ArgumentException>(() => Enum.IsDefined(typeof(NodeHealth), 1));

        Assert.Equal(typeof(ServingState), Vocabulary);
        Assert.NotEmpty(Enum.GetNames(Vocabulary));
        Assert.NotEmpty(Enum.GetValues(Vocabulary));
    }

    /// <summary>Two states spelled with a comma between them are read as one state the enum never named: the five are a
    /// choice and wear no <see cref="FlagsAttribute" />, but the text a parse reads goes by the bits underneath -
    /// <c>Pending</c> with <c>Primary</c> comes back as <c>FailedOver</c>, a word no caller meant, and <c>Primary</c>
    /// with <c>Down</c> as a number outside the five that says its number when it is asked what it is.
    /// <c>NoNodes</c> adds nothing to either side of a comma, the spaces between two names are walked past, and a
    /// number spelled with a comma or a name there is none of is refused, leaving the unset state behind.</summary>
    [Fact]
    public void TwoStatesSpelledWithACommaBetweenThemAreOneStateNobodyMeant()
    {
        Assert.Equal(ServingState.FailedOver, Enum.Parse<ServingState>("Pending,Primary"));
        Assert.Equal(ServingState.FailedOver, Enum.Parse<ServingState>("Primary , Pending"));
        Assert.Equal(ServingState.Pending, Enum.Parse<ServingState>("Pending,NoNodes"));
        Assert.Equal(ServingState.NoNodes, Enum.Parse<ServingState>("NoNodes,NoNodes"));
        Assert.Equal(ServingState.Primary, Enum.Parse<ServingState>("Primary,NoNodes"));

        var combined = Enum.Parse<ServingState>("Primary,Down");

        Assert.Equal((ServingState)6, combined);
        Assert.False(Enum.IsDefined(combined));
        Assert.Equal("6", combined.ToString());
        Assert.Equal(6, (int)combined);

        Assert.Throws<ArgumentException>(() => Enum.Parse<ServingState>("Primary,Sideways"));
        Assert.Throws<ArgumentException>(() => Enum.Parse<ServingState>("1,2"));
        Assert.False(Enum.TryParse<ServingState>("Primary,Sideways", out var refused));
        Assert.Equal(ServingState.NoNodes, refused);
    }

    /// <summary>A state wears no flags, yet the ask forgives every state all the same and answers out of the bits:
    /// <see cref="ServingState.FailedOver" /> carries <see cref="ServingState.Primary" />'s bit and says
    /// <see langword="true" /> of a state it is not, every state carries the nothing of
    /// <see cref="ServingState.NoNodes" /> - whose number is zero, so nothing can be missing from it - and a number
    /// outside the five is asked as though it were one of the five. Nothing of the page reads a state this way, which is
    /// the point: the members of a choice answer to <c>=</c> and to nothing else.</summary>
    [Fact]
    public void AStateAskedAsAFlagAnswersTheBitsAndNotTheVocabulary()
    {
        Assert.False(typeof(ServingState).IsDefined(typeof(FlagsAttribute), false));

        Assert.True(ServingState.FailedOver.HasFlag(ServingState.Primary));
        Assert.True(ServingState.FailedOver.HasFlag(ServingState.Pending));
        Assert.False(ServingState.Primary.HasFlag(ServingState.FailedOver));
        Assert.True(ServingState.Down.HasFlag(ServingState.Down));
        Assert.True(ServingState.Down.HasFlag(ServingState.NoNodes));
        Assert.True(ServingState.NoNodes.HasFlag(ServingState.NoNodes));
        Assert.False(ServingState.Down.HasFlag((ServingState)(-1)));
        Assert.True(((ServingState)5).HasFlag(ServingState.Pending));
    }

    /// <summary>Every state outranks nothing at all: the one comparison a state carries answers <c>1</c> against
    /// <see langword="null" /> whichever of the five asks, and the number below the first is no exception. The same
    /// comparison refuses a number it was not told in the state's own type - whether that number is one the five stand
    /// on or a member of the vocabulary next door, <see cref="HealthState.Pending" />, which shares the word and the
    /// number and is neither the same state nor comparable to one.</summary>
    [Fact]
    public void AStateWeighedAgainstNothingOutranksItAndRefusesAnotherVocabulary()
    {
        Assert.All(Five, state => Assert.Equal(1, ((IComparable)state).CompareTo(null)));
        Assert.Equal(1, ((IComparable)(ServingState)(-1)).CompareTo(null));
        Assert.Equal(1, ((IComparable)(ServingState)int.MinValue).CompareTo(null));

        Assert.Equal(1, ((IComparable)ServingState.Down).CompareTo((object)ServingState.Pending));
        Assert.Equal(-1, ((IComparable)ServingState.NoNodes).CompareTo((object)ServingState.Pending));

        Assert.Throws<ArgumentException>(() => ((IComparable)ServingState.Down).CompareTo(4));
        Assert.Throws<ArgumentException>(() => ((IComparable)ServingState.Pending).CompareTo(HealthState.Pending));
    }

    /// <summary>The state that is not there at all, which is the shape a caller stands in before a decision and the one
    /// shape no assessment can be handed: no word, no value, and the first of the five as soon as it is asked for
    /// gently - the absence of nodes rather than a check that has not run, exactly as an unset state reads. Boxed it is
    /// nothing at all, and every lifted comparison with it answers <see langword="false" />, so no ordering can be read
    /// out of a deployable nobody has decided yet; only a <c>!=</c> says the two are apart.</summary>
    [Fact]
    public void AStateThatIsNotThereAtAllSaysNoWordAndWeighsNothingAgainstAny()
    {
        ServingState? none = null;
        var alsoNone = default(ServingState?);
        ServingState? down = ServingState.Down;

        Assert.False(none.HasValue);
        Assert.True(none == null);
        Assert.True(alsoNone == null);
        Assert.False(none == ServingState.NoNodes);
        Assert.True(down == ServingState.Down);
        Assert.Equal(string.Empty, none.ToString());
        Assert.Equal(ServingState.NoNodes, none.GetValueOrDefault());
        Assert.Equal(ServingState.Primary, none.GetValueOrDefault(ServingState.Primary));
        Assert.Equal(0, none.GetHashCode());
        Assert.Null((object?)none);
        Assert.Equal(typeof(ServingState), ((object)down).GetType());
        Assert.Equal("00000004", ((IFormattable)down.Value).ToString("x", CultureInfo.InvariantCulture));

        Assert.False(down > none);
        Assert.False(down < none);
        Assert.False(down >= none);
        Assert.False(none >= down);
        Assert.False(none == down);
        Assert.True(none != down);

        var unset = none;
        Assert.Throws<InvalidOperationException>(() => _ = unset!.Value);
    }

    // ----- the state a decision lands on

    /// <summary>The shapes a topology takes between them: no nodes at all, the one node of a deployable with no Front
    /// Door endpoint, a primary and its standby in either order, and the two primaries a Front Door rotates over with
    /// and without a standby behind them.</summary>
    private static IEnumerable<NodeHealth[]> Shapes(HealthState first, HealthState second) =>
    [
        [],
        [Only(first)],
        [Primary(first), Standby(second)],
        [Standby(first), Primary(second)],
        [Primary(first), Primary(second)],
        [Primary(first), Primary(second), Standby(second)],
    ];

    /// <summary>The whole of what a decision can land on, swept over fixed nodes and fixed endpoint answers - four
    /// hundred and eighty assessments, no clock and no address among them: every one of them a state the enum names, so
    /// nothing outside the five can ever come out of <see cref="ServingAssessment.Assess" /> to be shown as a
    /// deployable's state; every one of them saying its own word back to a reader that asks it again; and all five
    /// landed on by some shape of the sweep, the standby listed before the primary and the rotation without its standby
    /// among them. What the five are worth in words once landed on is pinned with the assessments themselves; what is
    /// pinned here is that the decision never steps outside them.</summary>
    [Fact]
    public void EveryDecisionLandsOnOneOfTheFiveAndEveryOneOfTheFiveIsLandedOn()
    {
        var seen = new HashSet<ServingState>();
        var decisions = 0;

        foreach (var first in new[] { Pending, Healthy, Unhealthy, Unreachable })
        {
            foreach (var second in new[] { Pending, Healthy, Unhealthy, Unreachable })
            {
                foreach (var nodes in Shapes(first, second))
                {
                    foreach (var endpoint in Endpoints)
                    {
                        var state = ServingAssessment.Assess(nodes, endpoint).State;

                        Assert.True(Enum.IsDefined(state), $"{state} ({(int)state}) is no state of the five");
                        Assert.Contains(state, Five);
                        Assert.Equal(state, Enum.Parse<ServingState>(state.ToString()));

                        seen.Add(state);
                        decisions++;
                    }
                }
            }
        }

        // The sweep really swept: six shapes of nodes, each of them read against five endpoint answers, four pairs over.
        Assert.Equal(480, decisions);
        Assert.Equal(5, seen.Count);
        Assert.Equal(Five, seen.Order().ToArray());
    }
}
