using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="CheckState" />, the four words for the state of one
/// entry of a detailed health check, declared in <c>src/Dashboard/Health/HealthDetail.cs</c>. What a state is worth
/// in a mark is pinned where the marks are drawn: <see cref="HealthDetailTests" /> pins the word of every state -
/// <c>Not known</c> for the one the dashboard does not know - and the shape of every state, the state the answer's
/// status words are settled on, <c>Healthy</c>, <c>Unhealthy</c>, <c>Degraded</c> and the lower-case
/// <c>degraded</c> among them, a status the dashboard does not know and a status left out on the state that is not
/// known, and the entries that are not healthy, the worst first; <see cref="HealthDetailTextHelperTests" /> pins
/// the shape of every state again and the order the states are said in, the worst first; <see cref="RuntimePayloadHelperTests" />
/// the word the mark of a check carries. What all of that leaves open is pinned here: the four states as the
/// numbers, the names and the order they are declared in, the state a value of the type that was never set stands
/// on, nothing outside the four, the states combined as numbers, and text that is or is not a state - the file's
/// lower-case <c>healthy</c> in the case the file writes it, which is not the case the enum spells it, among them.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: a state is a word the file wrote, and every
/// word here is one the test brings.
/// </para>
/// </summary>
public class CheckStateEdgeCaseTests
{
    /// <summary>The four words of the enum, as it declares and spells them.</summary>
    private static readonly string[] Words = ["Healthy", "Degraded", "Unhealthy", "Unknown"];

    // ----- the four states, as the numbers they are

    /// <summary>The four states in the order the enum declares them, on the numbers 0 to 3, of an <see cref="int" />
    /// underneath, and a choice and not a set: the enum wears no <see cref="FlagsAttribute" /> and is sealed, so no
    /// state is built on and none on another.</summary>
    [Fact]
    public void TheFourCheckStatesAreTheWholeVocabularyInTheirDeclaredOrder()
    {
        Assert.Equal(Words, EnumVocabulary<CheckState>.Names);
        Assert.Equal([0, 1, 2, 3], EnumVocabulary<CheckState>.Numbers);
        Assert.Equal(4, EnumVocabulary<CheckState>.Count);
        Assert.Equal(typeof(int), EnumVocabulary<CheckState>.UnderlyingType);
        Assert.False(EnumVocabulary<CheckState>.IsFlags);
    }

    /// <summary>Each state on its own number and its own word, both ways round: the number is the one the name
    /// stands on, the word is the one that number answers to, and every one of the four is defined however it is
    /// arrived at.</summary>
    [Theory]
    [InlineData(CheckState.Healthy, 0, "Healthy")]
    [InlineData(CheckState.Degraded, 1, "Degraded")]
    [InlineData(CheckState.Unhealthy, 2, "Unhealthy")]
    [InlineData(CheckState.Unknown, 3, "Unknown")]
    public void EachCheckStateStandsOnItsOwnNumberAndItsOwnWord(CheckState state, int number, string name)
    {
        Assert.Equal(number, (int)state);
        Assert.Equal(state, (CheckState)number);
        Assert.Equal(name, state.ToString());
        Assert.Equal(name, Enum.GetName(state));
        Assert.True(Enum.IsDefined(state));
    }

    /// <summary>The first state is <see cref="CheckState.Healthy" />: the state a value of the type that was never
    /// set stands on - though the file never settles an answer's status that way, for a status it does not know and
    /// a status left out alike are the state that is not known, pinned in <see cref="HealthDetailTests" />. Outside
    /// the four there is nothing: neither one below the first nor one above the last nor either end of
    /// <see cref="int" /> is a defined state, and a value the enum does not name says the number it stands on.</summary>
    [Fact]
    public void AnUnsetCheckStateIsHealthyAndNothingOutsideTheFourIsAState()
    {
        var outside = EnumVocabulary<CheckState>.OutsideOf(4);

        Assert.Equal(CheckState.Healthy, default);
        Assert.Equal(CheckState.Healthy, EnumVocabulary<CheckState>.Of(0));

        Assert.True(Enum.IsDefined(CheckState.Healthy));
        Assert.True(Enum.IsDefined(CheckState.Unknown));
        Assert.False(Enum.IsDefined(outside.Below));
        Assert.False(Enum.IsDefined(outside.Above));
        Assert.False(Enum.IsDefined(outside.Lowest));
        Assert.False(Enum.IsDefined(outside.Highest));

        Assert.Null(Enum.GetName(outside.Above));
        Assert.Null(Enum.GetName(outside.Below));
        Assert.Null(Enum.GetName(outside.Lowest));
        Assert.Null(Enum.GetName(outside.Highest));
        Assert.Equal("4", outside.Above.ToString());
        Assert.Equal("-1", outside.Below.ToString());
        Assert.Equal("-2147483648", outside.Lowest.ToString());
        Assert.Equal("2147483647", outside.Highest.ToString());
    }

    /// <summary>The four states are apart from one another and sort by their numbers, as the declaration orders
    /// them; a boxed <see cref="int" /> on the same number is no state's equal, and neither is a box of nothing.</summary>
    [Fact]
    public void TheFourCheckStatesAreApartFromOneAnotherAndSortByTheirNumber()
    {
        CheckState[] states = [CheckState.Healthy, CheckState.Degraded, CheckState.Unhealthy, CheckState.Unknown];

        Assert.All(states, state => Assert.Single(states, other => other == state));

        Assert.True(CheckState.Healthy < CheckState.Degraded);
        Assert.True(CheckState.Degraded < CheckState.Unhealthy);
        Assert.True(CheckState.Unhealthy < CheckState.Unknown);
        Assert.False(CheckState.Unknown < CheckState.Unhealthy);
        Assert.Equal(1, CheckState.Unknown.CompareTo(CheckState.Unhealthy));
        Assert.Equal(-1, CheckState.Healthy.CompareTo(CheckState.Unknown));
        Assert.Equal(0, CheckState.Degraded.CompareTo((object)CheckState.Degraded));

        Assert.Equal((object)CheckState.Unhealthy, (object)CheckState.Unhealthy);
        Assert.False(((object)CheckState.Unhealthy).Equals(null));
        Assert.False(CheckState.Unhealthy.Equals(null));
        Assert.NotEqual((object)CheckState.Unhealthy, (object)2);
        Assert.Equal(CheckState.Unhealthy.GetHashCode(), ((object)CheckState.Unhealthy).GetHashCode());
    }

    /// <summary>A state is a choice and not a set: the enum wears no <see cref="FlagsAttribute" />, so the
    /// operators work on the numbers underneath, and because those numbers are the four that stand from 0 to 3,
    /// every union of two states lands on a state again while the intersection of two of them can be a state the
    /// neither was - <c>Degraded</c> with <c>Unhealthy</c> shares only the number <c>Healthy</c> stands on, so their
    /// intersection is the state that says nothing is wrong, at least by number.</summary>
    [Fact]
    public void TwoCheckStatesCombinedAreAnotherStateOrNoStateAtAll()
    {
        Assert.False(EnumVocabulary<CheckState>.IsFlags);

        Assert.Equal(CheckState.Degraded, CheckState.Healthy | CheckState.Degraded);
        Assert.Equal(CheckState.Unknown, CheckState.Degraded | CheckState.Unhealthy);
        Assert.Equal(CheckState.Unknown, CheckState.Unhealthy | CheckState.Unknown);
        Assert.Equal(CheckState.Unknown, CheckState.Degraded | CheckState.Unknown);

        Assert.Equal(CheckState.Healthy, CheckState.Degraded & CheckState.Unhealthy);
        Assert.Equal(CheckState.Healthy, CheckState.Unhealthy & CheckState.Degraded);
        Assert.Equal(CheckState.Degraded, CheckState.Unknown & CheckState.Degraded);
        Assert.Equal(CheckState.Unhealthy, CheckState.Unknown & CheckState.Unhealthy);

        Assert.Equal((CheckState)4, CheckState.Unknown + 1);
        Assert.False(Enum.IsDefined(CheckState.Unknown + 1));
        Assert.Equal((CheckState)(-1), CheckState.Healthy - 1);
        Assert.False(Enum.IsDefined(CheckState.Healthy - 1));
    }

    // ----- text that is or is not a state

    /// <summary>Text that names a state in the enum's case, its surrounding spaces walked past, or holds its number
    /// is that state; a name is read whole, and a number with a cipher in front of it is still the small number it
    /// says. The answer of the health check writes its words in lower case - <c>degraded</c>, not <c>Degraded</c> -
    /// and the file settles them on their states where it reads the answer, pinned in <see cref="HealthDetailTests" />;
    /// the enum itself spells them with the capital, and it alone is asked here.</summary>
    [Theory]
    [InlineData("Healthy", CheckState.Healthy)]
    [InlineData("Degraded", CheckState.Degraded)]
    [InlineData("Unhealthy", CheckState.Unhealthy)]
    [InlineData("Unknown", CheckState.Unknown)]
    [InlineData("  Unknown  ", CheckState.Unknown)]
    [InlineData("0", CheckState.Healthy)]
    [InlineData("3", CheckState.Unknown)]
    [InlineData("01", CheckState.Degraded)]
    public void ACheckStateTextIsTheStateItNamesOrTheStateItNumbers(string text, CheckState expected)
    {
        Assert.True(Enum.TryParse<CheckState>(text, out var state));
        Assert.Equal(expected, state);
        Assert.True(Enum.IsDefined(state));
        Assert.Equal(expected, Enum.Parse<CheckState>(text));
    }

    /// <summary>Text that is no state is refused - <see langword="null" />, the empty string, a blank of spaces or
    /// of a tab, a name in the case the file writes it and in no case, a name with something behind it, a fraction,
    /// a hexadecimal, a word of the file's own that is no word of the enum - and a refused parse leaves its answer
    /// standing on <see cref="CheckState.Healthy" />, the first of the four: the <c>false</c> is the whole answer,
    /// and a caller that reads the value anyway is shown a check as healthy as one can be.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("healthy")]
    [InlineData("UNKNOWN")]
    [InlineData("Degraded?")]
    [InlineData("unhealthy state")]
    [InlineData("Healthy.")]
    [InlineData("1.0")]
    [InlineData("0x2")]
    [InlineData("paused")]
    public void TextThatIsNoCheckStateIsRefusedAndLeavesHealthyBehind(string? text)
    {
        Assert.False(Enum.TryParse<CheckState>(text, out var state));
        Assert.Equal(CheckState.Healthy, state);
        Assert.False(Enum.TryParse<CheckState>(text, ignoreCase: false, out _));
    }

    /// <summary><see cref="Enum.Parse{TEnum}(string?, bool)" /> says what is wrong with text that is no state:
    /// <see langword="null" /> is no text at all, the empty string and a blank are text without a state in them, and
    /// a word is a word in the case the enum spells it - the file's lower-case <c>healthy</c> and the word
    /// <c>paused</c> the answer may say are both refused.</summary>
    [Fact]
    public void ParsingTextThatIsNoCheckStateThrows()
    {
        Assert.Throws<ArgumentNullException>(() => Enum.Parse<CheckState>(null!));
        Assert.Throws<ArgumentException>(() => Enum.Parse<CheckState>(string.Empty));
        Assert.Throws<ArgumentException>(() => Enum.Parse<CheckState>("   "));
        Assert.Throws<ArgumentException>(() => Enum.Parse<CheckState>("healthy"));
        Assert.Throws<ArgumentException>(() => Enum.Parse<CheckState>("paused"));
    }

    /// <summary>A number outside the four is read as a state that is no state - the top of <see cref="int" /> among
    /// them - and a name in the case the file writes it is a state as soon as the parse is told to forgive the case,
    /// and nothing until it is.</summary>
    [Fact]
    public void ANumberOutsideTheFourIsReadAsAStateThatIsNoState()
    {
        Assert.True(Enum.TryParse<CheckState>("4", out var above));
        Assert.Equal((CheckState)4, above);
        Assert.False(Enum.IsDefined(above));
        Assert.Null(Enum.GetName(above));

        Assert.True(Enum.TryParse<CheckState>("-1", out var below));
        Assert.Equal((CheckState)(-1), below);
        Assert.False(Enum.IsDefined(below));

        Assert.True(Enum.TryParse<CheckState>("2147483647", out var top));
        Assert.Equal((CheckState)int.MaxValue, top);
        Assert.False(Enum.IsDefined(top));

        Assert.False(Enum.TryParse<CheckState>("healthy", out _));
        Assert.True(Enum.TryParse<CheckState>("healthy", ignoreCase: true, out var forgiven));
        Assert.Equal(CheckState.Healthy, forgiven);
        Assert.True(Enum.TryParse<CheckState>("UNHEALTHY", ignoreCase: true, out var bad));
        Assert.Equal(CheckState.Unhealthy, bad);
    }

    // ----- the surface the enum declares

    /// <summary>The whole of what <see cref="CheckState" /> declares itself, as the running assembly holds it: an
    /// <see cref="int" /> underneath, sealed and no set, and exactly its four states and nothing else - each one a
    /// public static literal of its own type, in the order the declaration writes them, on the number it stands on;
    /// no instance member of its own, so a state carries nothing beyond the number it is.</summary>
    [Fact]
    public void TheEnumDeclaresItsFourStatesAndNothingElse()
    {
        var type = typeof(CheckState);

        Assert.True(type.IsEnum);
        Assert.True(type.IsValueType);
        Assert.True(type.IsSealed);
        Assert.False(type.IsPrimitive);

        // The only instance member a state carries of its own is the storage the compiler gives every value type:
        // the <c>int</c> the enum is of, and nothing else.
        var instance = EnumVocabulary<CheckState>.InstanceMembers;
        Assert.Single(instance);
        Assert.Equal("value__", instance[0].Name);
        Assert.Equal(typeof(int), Assert.IsAssignableFrom<FieldInfo>(instance[0]).FieldType);

        var fields = EnumVocabulary<CheckState>.Literals;

        Assert.Equal(Words, fields.Select(field => field.Name));
        Assert.Equal([0, 1, 2, 3], EnumVocabulary<CheckState>.LiteralNumbers);
        Assert.All(fields, field =>
        {
            Assert.Equal(type, field.FieldType);
            Assert.True(field.IsPublic);
            Assert.True(field.IsLiteral);
            Assert.False(field.IsInitOnly);
        });
    }
}
