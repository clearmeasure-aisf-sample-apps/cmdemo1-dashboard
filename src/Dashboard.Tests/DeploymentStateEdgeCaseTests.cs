using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="DeploymentState" />, the seven words for the
/// state of one deployment task, declared in <c>src/Dashboard/Health/DeploymentsReport.cs</c>. What a state is
/// worth in a mark - the sentence, the shape of the dot, the order the marks come in, and whether the task is
/// still in flight - is pinned where the marks are drawn: <see cref="DeploymentsTests" /> pins the sentence for
/// every state, the shape of the dot for every state, the order the states are said in and the state the page
/// does not know said in the file's own word; <see cref="RecentDeploymentEdgeCaseTests" /> pins a result kept as
/// the state it is given, however outside the seven it stands. What all of that leaves open is pinned here: the
/// seven states as the numbers, the names and the order they are declared in, the unset state, nothing outside
/// the seven, the states combined as numbers, and text that is or is not a state - the file's word <c>queued</c>
/// in the case the file writes it, which is not the case the enum spells it, among them.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: a state is a word the file wrote, and every
/// word here is one the test brings.
/// </para>
/// </summary>
public class DeploymentStateEdgeCaseTests
{
    // ----- the seven states, as the numbers they are

    /// <summary>The seven states in the order the enum declares them, on the numbers 0 to 6, of an
    /// <see cref="int" /> underneath, and a choice and not a set: the enum wears no <see cref="FlagsAttribute" />
    /// and is sealed, so no state is built on and none on another.</summary>
    [Fact]
    public void TheSevenDeploymentStatesAreTheWholeVocabularyInTheirDeclaredOrder()
    {
        Assert.Equal(
            ["Unknown", "Queued", "Executing", "Waiting", "Succeeded", "Failed", "Canceled"],
            Enum.GetNames<DeploymentState>());
        Assert.Equal([0, 1, 2, 3, 4, 5, 6], Enum.GetValues<DeploymentState>().Select(state => (int)state));
        Assert.Equal(7, Enum.GetValues<DeploymentState>().Length);
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(typeof(DeploymentState)));
        Assert.False(typeof(DeploymentState).IsDefined(typeof(FlagsAttribute), false));
    }

    /// <summary>Each state on its own number and its own word, both ways round: the number is the one the name
    /// stands on, the word is the one that number answers to, and every one of the seven is defined however it is
    /// arrived at.</summary>
    [Theory]
    [InlineData(DeploymentState.Unknown, 0, "Unknown")]
    [InlineData(DeploymentState.Queued, 1, "Queued")]
    [InlineData(DeploymentState.Executing, 2, "Executing")]
    [InlineData(DeploymentState.Waiting, 3, "Waiting")]
    [InlineData(DeploymentState.Succeeded, 4, "Succeeded")]
    [InlineData(DeploymentState.Failed, 5, "Failed")]
    [InlineData(DeploymentState.Canceled, 6, "Canceled")]
    public void EachDeploymentStateStandsOnItsOwnNumberAndItsOwnWord(DeploymentState state, int number, string name)
    {
        Assert.Equal(number, (int)state);
        Assert.Equal(state, (DeploymentState)number);
        Assert.Equal(name, state.ToString());
        Assert.Equal(name, Enum.GetName(state));
        Assert.True(Enum.IsDefined(state));
    }

    /// <summary>An unset state is the first one, <c>Unknown</c>: the state a word the page does not know stands
    /// on, which is why a state no word settles on is the one that says the file's own word. Outside the seven
    /// there is nothing: neither one below the first nor one above the last nor either end of <see cref="int" />
    /// is a defined state, and a value the enum does not name says the number it stands on.</summary>
    [Fact]
    public void AnUnsetDeploymentStateIsUnknownAndNothingOutsideTheSevenIsAState()
    {
        Assert.Equal(DeploymentState.Unknown, default);
        Assert.Equal(DeploymentState.Unknown, (DeploymentState)0);

        Assert.True(Enum.IsDefined(DeploymentState.Unknown));
        Assert.True(Enum.IsDefined(DeploymentState.Canceled));
        Assert.False(Enum.IsDefined((DeploymentState)(-1)));
        Assert.False(Enum.IsDefined((DeploymentState)7));
        Assert.False(Enum.IsDefined((DeploymentState)int.MinValue));
        Assert.False(Enum.IsDefined((DeploymentState)int.MaxValue));

        Assert.Null(Enum.GetName((DeploymentState)7));
        Assert.Null(Enum.GetName((DeploymentState)(-1)));
        Assert.Equal("7", ((DeploymentState)7).ToString());
        Assert.Equal("-1", ((DeploymentState)(-1)).ToString());
        Assert.Equal("-2147483648", ((DeploymentState)int.MinValue).ToString());
        Assert.Equal("2147483647", ((DeploymentState)int.MaxValue).ToString());
    }

    /// <summary>The seven states are apart from one another and sort by their numbers, as the declaration orders
    /// them; a boxed <see cref="int" /> on the same number is no state's equal, and neither is a box of nothing.</summary>
    [Fact]
    public void TheDeploymentStatesAreApartFromOneAnotherAndSortByTheirNumber()
    {
        var states = Enum.GetValues<DeploymentState>();

        Assert.Equal(states.Length, states.Distinct().Count());
        Assert.Equal(states, states.Order());
        Assert.True(DeploymentState.Unknown < DeploymentState.Queued);
        Assert.True(DeploymentState.Failed < DeploymentState.Canceled);
        Assert.Equal(DeploymentState.Unknown, states.Min());
        Assert.Equal(DeploymentState.Canceled, states.Max());
        Assert.Equal(1, DeploymentState.Canceled.CompareTo(DeploymentState.Unknown));
        Assert.Equal(-1, DeploymentState.Queued.CompareTo(DeploymentState.Canceled));
        Assert.Equal(0, DeploymentState.Waiting.CompareTo((object)DeploymentState.Waiting));

        Assert.True(((object)DeploymentState.Failed).Equals(DeploymentState.Failed));
        Assert.False(DeploymentState.Failed.Equals(null));
        Assert.False(DeploymentState.Failed.Equals((object)5));
        Assert.Equal(DeploymentState.Failed.GetHashCode(), ((object)DeploymentState.Failed).GetHashCode());
    }

    /// <summary>A state is a choice and not a set: the operators work on the numbers underneath, so two states
    /// can combine into a third - <c>Queued</c> with <c>Executing</c> is <c>Waiting</c>, at least a word the page
    /// has - or, past <c>Canceled</c>, into no state at all. Nothing is added by a union with <c>Unknown</c>,
    /// whose number is zero, and an intersection lands on the bits the two states share, which can be a state the
    /// neither was.</summary>
    [Fact]
    public void TwoDeploymentStatesCombinedAreAnotherStateOrNoStateAtAll()
    {
        Assert.Equal(DeploymentState.Waiting, DeploymentState.Queued | DeploymentState.Executing);
        Assert.Equal(DeploymentState.Failed, DeploymentState.Queued | DeploymentState.Succeeded);
        Assert.Equal(DeploymentState.Canceled, DeploymentState.Executing | DeploymentState.Succeeded);
        Assert.Equal(DeploymentState.Canceled, DeploymentState.Succeeded | DeploymentState.Canceled);
        Assert.Equal(DeploymentState.Waiting, DeploymentState.Executing | DeploymentState.Waiting);
        Assert.Equal(DeploymentState.Canceled, DeploymentState.Canceled | DeploymentState.Unknown);

        Assert.Equal(DeploymentState.Queued, DeploymentState.Queued & DeploymentState.Failed);
        Assert.Equal(DeploymentState.Executing, DeploymentState.Executing & DeploymentState.Canceled);
        Assert.Equal(DeploymentState.Succeeded, DeploymentState.Succeeded & DeploymentState.Canceled);
        Assert.Equal(DeploymentState.Succeeded, DeploymentState.Failed & DeploymentState.Canceled);
        Assert.Equal(DeploymentState.Unknown, DeploymentState.Queued & DeploymentState.Executing);
        Assert.Equal(DeploymentState.Unknown, DeploymentState.Waiting & DeploymentState.Succeeded);
        Assert.Equal(DeploymentState.Unknown, DeploymentState.Canceled & DeploymentState.Unknown);

        Assert.Equal((DeploymentState)7, DeploymentState.Canceled + 1);
        Assert.False(Enum.IsDefined(DeploymentState.Canceled + 1));
        Assert.Equal((DeploymentState)(-1), DeploymentState.Unknown - 1);
        Assert.False(Enum.IsDefined(DeploymentState.Unknown - 1));
    }

    // ----- text that is or is not a state

    /// <summary>Text that names a state in the enum's case, its surrounding spaces walked past, or holds its
    /// number is that state; a name is read whole, and a number with a cipher in front of it is still the small
    /// number it says. The file writes its words in lower case - <c>queued</c>, not <c>Queued</c> - and the page
    /// settles them on their states where it reads the file, pinned in <see cref="DeploymentsTests" />; the enum
    /// itself spells them with the capital.</summary>
    [Theory]
    [InlineData("Unknown", DeploymentState.Unknown)]
    [InlineData("Queued", DeploymentState.Queued)]
    [InlineData("Executing", DeploymentState.Executing)]
    [InlineData("Waiting", DeploymentState.Waiting)]
    [InlineData("Succeeded", DeploymentState.Succeeded)]
    [InlineData("Failed", DeploymentState.Failed)]
    [InlineData("Canceled", DeploymentState.Canceled)]
    [InlineData("  Canceled  ", DeploymentState.Canceled)]
    [InlineData("0", DeploymentState.Unknown)]
    [InlineData("6", DeploymentState.Canceled)]
    [InlineData("03", DeploymentState.Waiting)]
    public void ADeploymentStateTextIsTheStateItNamesOrTheStateItNumbers(string text, DeploymentState expected)
    {
        Assert.True(Enum.TryParse<DeploymentState>(text, out var state));
        Assert.Equal(expected, state);
        Assert.True(Enum.IsDefined(state));
        Assert.Equal(expected, Enum.Parse<DeploymentState>(text));
    }

    /// <summary>Text that is no state is refused - <see langword="null" />, the empty string, a blank of spaces
    /// or of a tab, a name in the case the file writes it and in no case, a name with something behind it, a
    /// fraction, a hexadecimal, a near-word - and a refused parse leaves its answer standing on
    /// <see cref="DeploymentState.Unknown" />: the <c>false</c> is the whole answer, and the state it leaves is
    /// the one that says the file's own word.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("queued")]
    [InlineData("QUEUED")]
    [InlineData("Queued?")]
    [InlineData("waiting time")]
    [InlineData("Succeeded.")]
    [InlineData("4.0")]
    [InlineData("0x2")]
    [InlineData("cancelled")]
    public void TextThatIsNoDeploymentStateIsRefusedAndLeavesUnknownBehind(string? text)
    {
        Assert.False(Enum.TryParse<DeploymentState>(text, out var state));
        Assert.Equal(DeploymentState.Unknown, state);
        Assert.False(Enum.TryParse<DeploymentState>(text, ignoreCase: false, out _));
    }

    /// <summary><see cref="Enum.Parse{TEnum}(string?, bool)" /> says what is wrong with text that is no state:
    /// null is no text at all, the empty string and a blank are text without a state in them, and a word is a
    /// word in the case the enum spells it - the file's lower-case <c>queued</c> and the British near-word
    /// <c>cancelled</c> are both refused.</summary>
    [Fact]
    public void ParsingTextThatIsNoDeploymentStateThrows()
    {
        Assert.Throws<ArgumentNullException>(() => Enum.Parse<DeploymentState>(null!));
        Assert.Throws<ArgumentException>(() => Enum.Parse<DeploymentState>(string.Empty));
        Assert.Throws<ArgumentException>(() => Enum.Parse<DeploymentState>("   "));
        Assert.Throws<ArgumentException>(() => Enum.Parse<DeploymentState>("queued"));
        Assert.Throws<ArgumentException>(() => Enum.Parse<DeploymentState>("cancelled"));
    }

    /// <summary>A number outside the seven is read as a state that is no state - the top and the bottom of
    /// <see cref="int" /> among them - and a name in the case the file writes it is a state as soon as the parse
    /// is told to forgive the case, and nothing until it is.</summary>
    [Fact]
    public void ANumberOutsideTheSevenIsReadAsAStateThatIsNoState()
    {
        Assert.True(Enum.TryParse<DeploymentState>("7", out var above));
        Assert.Equal((DeploymentState)7, above);
        Assert.False(Enum.IsDefined(above));
        Assert.Null(Enum.GetName(above));

        Assert.True(Enum.TryParse<DeploymentState>("-1", out var below));
        Assert.Equal((DeploymentState)(-1), below);
        Assert.False(Enum.IsDefined(below));

        Assert.True(Enum.TryParse<DeploymentState>("2147483647", out var top));
        Assert.Equal((DeploymentState)int.MaxValue, top);
        Assert.False(Enum.IsDefined(top));

        Assert.False(Enum.TryParse<DeploymentState>("queued", out _));
        Assert.True(Enum.TryParse<DeploymentState>("queued", ignoreCase: true, out var forgiven));
        Assert.Equal(DeploymentState.Queued, forgiven);
        Assert.True(Enum.TryParse<DeploymentState>("canceled", ignoreCase: true, out var ended));
        Assert.Equal(DeploymentState.Canceled, ended);
    }

    // ----- the surface the enum declares

    /// <summary>The whole of what <see cref="DeploymentState" /> declares itself, as the running assembly holds
    /// it: an <see cref="int" /> underneath, sealed and no set, and exactly its seven states and nothing else -
    /// each one a public static literal of its own type, in the order the declaration writes them, on the number
    /// it stands on; no instance member of its own, so a state carries nothing beyond the number it is.</summary>
    [Fact]
    public void TheEnumDeclaresItsSevenStatesAndNothingElse()
    {
        var type = typeof(DeploymentState);

        Assert.True(type.IsEnum);
        Assert.True(type.IsValueType);
        Assert.True(type.IsSealed);
        Assert.False(type.IsPrimitive);

        // The only instance member a state carries of its own is the storage the compiler gives every value type:
        // the <c>int</c> the enum is of, and nothing else.
        var instance = type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Single(instance);
        Assert.Equal("value__", instance[0].Name);
        Assert.Equal(typeof(int), ((FieldInfo)instance[0]).FieldType);

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.Equal(
            ["Unknown", "Queued", "Executing", "Waiting", "Succeeded", "Failed", "Canceled"],
            fields.Select(field => field.Name));
        Assert.Equal([0, 1, 2, 3, 4, 5, 6], fields.Select(field => (int)field.GetRawConstantValue()!));
        Assert.All(fields, field =>
        {
            Assert.Equal(type, field.FieldType);
            Assert.True(field.IsPublic);
            Assert.True(field.IsLiteral);
            Assert.False(field.IsInitOnly);
        });
    }
}
