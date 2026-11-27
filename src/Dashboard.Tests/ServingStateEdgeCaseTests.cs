using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ServingState" />, the five words for which node of a
/// deployable is expected to serve its traffic, declared in <c>src/Dashboard/Health/ServingAssessment.cs</c>. What
/// each state is worth in words and in lists is pinned where the assessments are made:
/// <see cref="ServingAssessmentTests" /> says which state <see cref="ServingAssessment.Assess" /> lands a set of nodes
/// in - the healthy primary, the standby that took over, the unchecked primary, the node that is nothing, the
/// deployable with no nodes at all - and the headline, the failover detail and the Front Door sentence that go with
/// them; <see cref="RotationTests" /> pins the same five states as a rotation comes to them and goes out of them,
/// through the events they make and the runtime view that draws them.
/// <para>
/// What all of that leaves open, and what follows: the five states as the numbers, the names and the order they are
/// declared in, and nothing outside them; the unset state, which is the absence of nodes rather than a check that has
/// not run; the five names as words, none of them empty, none of them a word of spaces, none of them another's in any
/// case; the states combined as numbers, which a choice and not a set neither offers nor forbids; text that is or is
/// not a state, in the enum's case and in none; the surface the enum declares itself, so that the day a sixth state is
/// added the list below says so; and the state an assessment stands on, which the record keeps as it is handed - even
/// one <see cref="ServingAssessment.Assess" /> never returns, whose words then fall to the file's last ones - with
/// <see cref="ServingState.FailedOver" /> the one state of the five that has failed over.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a state is a word the file wrote, the
/// assessment decides on the states it is handed and takes no moment and no address, and every value below is one the
/// test brought.
/// </para>
/// </summary>
public class ServingStateEdgeCaseTests
{
    /// <summary>The five names, in the order the enum declares them.</summary>
    private static readonly string[] Names = ["NoNodes", "Pending", "Primary", "FailedOver", "Down"];

    /// <summary>The node an assessment below is expected to serve traffic, whoever it is that serves it.</summary>
    private static readonly NodeHealth West = new("app-west", "westus3", true, HealthState.Unhealthy);

    // ----- the five states, as the numbers they are -----

    /// <summary>The five states in the order the enum declares them, on the numbers 0 to 4, of an
    /// <see cref="int" /> underneath, the values standing one after another with no number left out and none twice.
    /// A state is a choice and not a set: the enum wears no <see cref="FlagsAttribute" />.</summary>
    [Fact]
    public void TheFiveServingStatesAreTheWholeVocabularyInTheirDeclaredOrder()
    {
        Assert.Equal(Names, EnumVocabulary<ServingState>.Names);
        Assert.Equal([0, 1, 2, 3, 4], EnumVocabulary<ServingState>.Numbers);
        Assert.Equal(5, EnumVocabulary<ServingState>.Count);
        Assert.Equal(typeof(int), EnumVocabulary<ServingState>.UnderlyingType);
        Assert.False(EnumVocabulary<ServingState>.IsFlags);

        // The list of the values is a fresh array each time it is asked for, and the same five states in it.
        Assert.NotSame(Enum.GetValues<ServingState>(), Enum.GetValues<ServingState>());
        Assert.Equal(Enum.GetValues<ServingState>(), Enum.GetValues<ServingState>());
    }

    /// <summary>Each state on its own number and its own word, both ways round: the number is the one the name
    /// stands on, the word is the one that number answers to, and every one of the five is defined however it is
    /// arrived at.</summary>
    [Theory]
    [InlineData(ServingState.NoNodes, 0, "NoNodes")]
    [InlineData(ServingState.Pending, 1, "Pending")]
    [InlineData(ServingState.Primary, 2, "Primary")]
    [InlineData(ServingState.FailedOver, 3, "FailedOver")]
    [InlineData(ServingState.Down, 4, "Down")]
    public void EachServingStateStandsOnItsOwnNumberAndItsOwnWord(ServingState state, int number, string name)
    {
        Assert.Equal(number, (int)state);
        Assert.Equal(state, (ServingState)number);
        Assert.Equal(name, state.ToString());
        Assert.Equal(name, Enum.GetName(state));
        Assert.True(Enum.IsDefined(state));
    }

    /// <summary>An unset state is the first one, <c>NoNodes</c>: a state that was never settled is the absence of the
    /// nodes the decision is made of, and not the check that has not run - which is <c>Pending</c>, the state
    /// <see cref="ServingAssessment.Assess" /> gives a deployable whose nodes are there and unchecked. Outside the five
    /// there is nothing: neither one below the first nor one above the last nor either end of <see cref="int" /> is a
    /// defined state, and a value the enum does not name says the number it stands on.</summary>
    [Fact]
    public void AnUnsetServingStateIsNoNodesAndNothingOutsideTheFiveIsAState()
    {
        var outside = EnumVocabulary<ServingState>.OutsideOf(5);

        Assert.Equal(ServingState.NoNodes, default);
        Assert.Equal(ServingState.NoNodes, EnumVocabulary<ServingState>.Of(0));

        Assert.True(Enum.IsDefined(ServingState.NoNodes));
        Assert.True(Enum.IsDefined(ServingState.Down));
        Assert.False(Enum.IsDefined(outside.Below));
        Assert.False(Enum.IsDefined(outside.Above));
        Assert.False(Enum.IsDefined(outside.Lowest));
        Assert.False(Enum.IsDefined(outside.Highest));

        Assert.Null(Enum.GetName(outside.Above));
        Assert.Null(Enum.GetName(outside.Below));
        Assert.Null(Enum.GetName(outside.Lowest));
        Assert.Null(Enum.GetName(outside.Highest));
        Assert.Equal("5", outside.Above.ToString());
        Assert.Equal("-1", outside.Below.ToString());
        Assert.Equal("-2147483648", outside.Lowest.ToString());
        Assert.Equal("2147483647", outside.Highest.ToString());
    }

    /// <summary>The five states are apart from one another and sort by their numbers, as the declaration orders
    /// them; a boxed <see cref="int" /> on the same number is no state's equal, and neither is a box of
    /// nothing.</summary>
    [Fact]
    public void TheServingStatesAreApartFromOneAnotherAndSortByTheirNumber()
    {
        var states = Enum.GetValues<ServingState>();

        Assert.Equal(states.Length, states.Distinct().Count());
        Assert.Equal(states, states.Order());
        Assert.True(ServingState.NoNodes < ServingState.Pending);
        Assert.True(ServingState.FailedOver < ServingState.Down);
        Assert.Equal(ServingState.NoNodes, states.Min());
        Assert.Equal(ServingState.Down, states.Max());
        Assert.Equal(1, ServingState.Down.CompareTo(ServingState.NoNodes));
        Assert.Equal(-1, ServingState.Pending.CompareTo(ServingState.Down));
        Assert.Equal(0, ServingState.Primary.CompareTo((object)ServingState.Primary));
        Assert.Single(new HashSet<ServingState> { ServingState.Primary, (ServingState)2, ServingState.Primary });

        Assert.True(((object)ServingState.Down).Equals(ServingState.Down));
        Assert.False(ServingState.Down.Equals(null));
        Assert.False(ServingState.Down.Equals((object)4));
        Assert.Equal(ServingState.Down.GetHashCode(), ((object)ServingState.Down).GetHashCode());
    }

    // ----- the five names, as the words they are: none empty, none another's -----

    /// <summary>Every one of the five names is a word: not the empty word, not a word of nothing but spaces, nothing
    /// before it or after it, one word with no space in it and no letter in it but a letter of the alphabet, the first
    /// of them capitalized since that is the case the enum spells its states in. No two of the five are one word in
    /// any case, and a sixth word - the state's own name with a space in the middle of it, which is what the page says
    /// of a failover - is no name of the enum.</summary>
    [Fact]
    public void NoServingStateNameIsTheEmptyWordOrAnotherName()
    {
        Assert.All(Names, name =>
        {
            Assert.False(string.IsNullOrEmpty(name));
            Assert.False(string.IsNullOrWhiteSpace(name));
            Assert.Equal(name, name.Trim());
            Assert.Single(name.Split(' '));
            Assert.All(name, letter => Assert.True(char.IsAsciiLetter(letter)));
            Assert.Equal(char.ToUpperInvariant(name[0]), name[0]);
            Assert.NotEqual(name, name.ToUpperInvariant());
            Assert.NotEqual(name, name.ToLowerInvariant());
        });

        Assert.Equal(5, new HashSet<string>(Names, StringComparer.Ordinal).Count);
        Assert.Equal(5, new HashSet<string>(Names, StringComparer.OrdinalIgnoreCase).Count);
        Assert.Distinct(Names);
        Assert.DoesNotContain(string.Empty, Names, StringComparer.Ordinal);
        Assert.DoesNotContain("No Nodes", Names, StringComparer.Ordinal);
        Assert.DoesNotContain("Failed Over", Names, StringComparer.Ordinal);
    }

    // ----- the boundary: the states combined as numbers -----

    /// <summary>A state is a choice and not a set: the operators work on the numbers underneath, so two states can
    /// combine into a third - <c>Pending</c> with <c>Primary</c> is <c>FailedOver</c>, at least a word the page has -
    /// or, their bits landing anywhere but on one of the five, into no state at all. A union with <c>NoNodes</c> adds
    /// nothing, whose number is zero, and an intersection lands on the bits the two share, which can be the absence of
    /// nodes that neither of them was.</summary>
    [Fact]
    public void TwoServingStatesCombinedAreAnotherStateOrNoStateAtAll()
    {
        Assert.Equal(ServingState.FailedOver, ServingState.Pending | ServingState.Primary);
        Assert.Equal(ServingState.FailedOver, ServingState.Primary | ServingState.Pending);
        Assert.Equal(ServingState.Primary, ServingState.Primary | ServingState.NoNodes);
        Assert.Equal(ServingState.Down, ServingState.Down | ServingState.NoNodes);
        Assert.False(Enum.IsDefined(ServingState.Pending | ServingState.Down));
        Assert.False(Enum.IsDefined(ServingState.Primary | ServingState.Down));
        Assert.Equal((ServingState)7, ServingState.FailedOver | ServingState.Down);

        Assert.Equal(ServingState.Primary, ServingState.FailedOver & ServingState.Primary);
        Assert.Equal(ServingState.Pending, ServingState.FailedOver & ServingState.Pending);
        Assert.Equal(ServingState.NoNodes, ServingState.FailedOver & ServingState.Down);
        Assert.Equal(ServingState.NoNodes, ServingState.Primary & ServingState.Down);
        Assert.Equal(ServingState.NoNodes, ServingState.Down & ServingState.NoNodes);
        Assert.Equal(ServingState.NoNodes, ServingState.NoNodes & ServingState.NoNodes);

        // One past the last state and one before the first are no state, and the states step from one to the next.
        Assert.Equal((ServingState)5, ServingState.Down + 1);
        Assert.False(Enum.IsDefined(ServingState.Down + 1));
        Assert.Equal((ServingState)(-1), ServingState.NoNodes - 1);
        Assert.False(Enum.IsDefined(ServingState.NoNodes - 1));
        Assert.Equal(ServingState.FailedOver, ServingState.Down - 1);
        Assert.Equal(ServingState.Pending, ServingState.Primary - 1);
    }

    // ----- text that is or is not a state -----

    /// <summary>Text that names a state in the enum's case, its surrounding spaces walked past, or holds its number is
    /// that state; a name is read whole, and a number with a cipher in front of it is still the small number it
    /// says.</summary>
    [Theory]
    [InlineData("NoNodes", ServingState.NoNodes)]
    [InlineData("Pending", ServingState.Pending)]
    [InlineData("Primary", ServingState.Primary)]
    [InlineData("FailedOver", ServingState.FailedOver)]
    [InlineData("Down", ServingState.Down)]
    [InlineData("  Down  ", ServingState.Down)]
    [InlineData("0", ServingState.NoNodes)]
    [InlineData("4", ServingState.Down)]
    [InlineData("02", ServingState.Primary)]
    public void AServingStateTextIsTheStateItNamesOrTheStateItNumbers(string text, ServingState expected)
    {
        Assert.True(Enum.TryParse<ServingState>(text, out var state));
        Assert.Equal(expected, state);
        Assert.True(Enum.IsDefined(state));
        Assert.Equal(expected, Enum.Parse<ServingState>(text));
    }

    /// <summary>Text that is no state is refused - <see langword="null" />, the empty string, a blank of spaces or of a
    /// tab, a name in lower case, a name with something behind it, two words of the page rather than one of the enum,
    /// a fraction, a hexadecimal, the page's own word for the state - and a refused parse leaves its answer standing on
    /// <see cref="ServingState.NoNodes" />: the <c>false</c> is the whole answer, and the state it leaves behind is the
    /// unset one rather than the one the text was near.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("primary")]
    [InlineData("NODENODES")]
    [InlineData("No Nodes")]
    [InlineData("Failed Over")]
    [InlineData("FailedOver!")]
    [InlineData("Pending.")]
    [InlineData("3.0")]
    [InlineData("0x3")]
    [InlineData("Serving")]
    public void TextThatIsNoServingStateIsRefusedAndLeavesNoNodesBehind(string? text)
    {
        Assert.False(Enum.TryParse<ServingState>(text, out var state));
        Assert.Equal(ServingState.NoNodes, state);
        Assert.False(Enum.TryParse<ServingState>(text, ignoreCase: false, out _));
    }

    /// <summary><see cref="Enum.Parse{TEnum}(string?, bool)" /> says what is wrong with text that is no state: null is
    /// no text at all, the empty string and a blank are text without a state in them, and a word is a word in the case
    /// the enum spells it and with no space in the middle of it.</summary>
    [Fact]
    public void ParsingTextThatIsNoServingStateThrows()
    {
        Assert.Throws<ArgumentNullException>(() => Enum.Parse<ServingState>(null!));
        Assert.Throws<ArgumentException>(() => Enum.Parse<ServingState>(string.Empty));
        Assert.Throws<ArgumentException>(() => Enum.Parse<ServingState>("   "));
        Assert.Throws<ArgumentException>(() => Enum.Parse<ServingState>("primary"));
        Assert.Throws<ArgumentException>(() => Enum.Parse<ServingState>("Failed Over"));
    }

    /// <summary>A number outside the five is read as a state that is no state - the top of <see cref="int" /> among
    /// them - and a name in another case is a state as soon as the parse is told to forgive the case, and nothing until
    /// it is.</summary>
    [Fact]
    public void ANumberOutsideTheFiveIsReadAsAStateThatIsNoState()
    {
        Assert.True(Enum.TryParse<ServingState>("5", out var above));
        Assert.Equal((ServingState)5, above);
        Assert.False(Enum.IsDefined(above));
        Assert.Null(Enum.GetName(above));

        Assert.True(Enum.TryParse<ServingState>("-1", out var below));
        Assert.Equal((ServingState)(-1), below);
        Assert.False(Enum.IsDefined(below));

        Assert.True(Enum.TryParse<ServingState>("2147483647", out var top));
        Assert.Equal((ServingState)int.MaxValue, top);
        Assert.False(Enum.IsDefined(top));

        Assert.False(Enum.TryParse<ServingState>("primary", out _));
        Assert.True(Enum.TryParse<ServingState>("primary", ignoreCase: true, out var forgiven));
        Assert.Equal(ServingState.Primary, forgiven);
        Assert.True(Enum.TryParse<ServingState>("failedover", ignoreCase: true, out var taken));
        Assert.Equal(ServingState.FailedOver, taken);
    }

    // ----- the surface the enum declares -----

    /// <summary>The whole of what <see cref="ServingState" /> declares itself, as the running assembly holds it: an
    /// <see cref="int" /> underneath, sealed and no set, and exactly its five states and nothing else - each one a
    /// public static literal of its own type, in the order the declaration writes them, on the number it stands on; no
    /// instance member of its own, so a state carries nothing beyond the number it is.</summary>
    [Fact]
    public void TheEnumDeclaresItsFiveStatesAndNothingElse()
    {
        var type = typeof(ServingState);

        Assert.True(type.IsEnum);
        Assert.True(type.IsValueType);
        Assert.True(type.IsSealed);
        Assert.False(type.IsPrimitive);

        // The only instance member a state carries of its own is the storage the compiler gives every value type: the
        // <c>int</c> the enum is of, and nothing else.
        var instance = EnumVocabulary<ServingState>.InstanceMembers;

        Assert.Single(instance);
        Assert.Equal("value__", instance[0].Name);
        Assert.Equal(typeof(int), Assert.IsAssignableFrom<FieldInfo>(instance[0]).FieldType);

        var fields = EnumVocabulary<ServingState>.Literals;

        Assert.Equal(Names, fields.Select(field => field.Name));
        Assert.Equal([0, 1, 2, 3, 4], EnumVocabulary<ServingState>.LiteralNumbers);
        Assert.All(fields, field =>
        {
            Assert.Equal(type, field.FieldType);
            Assert.True(field.IsPublic);
            Assert.True(field.IsLiteral);
            Assert.False(field.IsInitOnly);
        });
    }

    // ----- the state an assessment stands on -----

    /// <summary>The record keeps the state it is handed, however far outside the five it stands, and says its last
    /// word for the state it does not know: <see cref="ServingAssessment.State" /> is the member the headlines, the
    /// lists and the detail are read off, and each of them falls through to the word it holds for everything the file
    /// does not name rather than to a word of its own for a state there is none. The deployable that is neither a
    /// rotation nor one node says that nothing can serve traffic; the rotation says that no region is healthy; and an
    /// assessment that is not in <see cref="ServingState.FailedOver" /> has no failover to account for, however it was
    /// arrived at.</summary>
    [Fact]
    public void AnAssessmentKeepsTheStateItIsGivenEvenOneOutsideTheFive()
    {
        var none = ServingAssessment.Assess([], frontDoor: null);
        var outside = none with { State = (ServingState)5 };

        Assert.Equal(ServingState.NoNodes, none.State);
        Assert.Equal((ServingState)5, outside.State);
        Assert.False(Enum.IsDefined(outside.State));
        Assert.NotEqual(none, outside);
        Assert.Equal(none, outside with { State = ServingState.NoNodes });

        Assert.False(outside.IsFailedOver);
        Assert.False(outside.IsRotation);
        Assert.Empty(outside.Serving);
        Assert.Empty(outside.OutOfRotation);
        Assert.Equal("No healthy node: nothing can serve traffic", outside.Headline);
        Assert.Null(outside.FailoverDetail);
        Assert.Null(outside.FrontDoorText);

        var rotation = outside with { Rotation = [West] };

        Assert.True(rotation.IsRotation);
        Assert.Equal([West.Label], [.. rotation.OutOfRotation.Select(node => node.Label)]);
        Assert.Equal("No region is healthy", rotation.Headline);
    }

    /// <summary>Of the five states, and of a sixth that is none, <c>FailedOver</c> alone has failed over: the member
    /// answers to its state and to nothing else - not to the node expected to serve, not to the standby handed it as
    /// the only node, not to a Front Door that agrees or disagrees - so the two states the page cannot yet decide, and
    /// the state that names no node at all, are each of them no failover.</summary>
    [Theory]
    [InlineData(ServingState.NoNodes, false)]
    [InlineData(ServingState.Pending, false)]
    [InlineData(ServingState.Primary, false)]
    [InlineData(ServingState.FailedOver, true)]
    [InlineData(ServingState.Down, false)]
    [InlineData((ServingState)5, false)]
    public void FailedOverIsTheOneServingStateThatHasFailedOver(ServingState state, bool failedOver)
    {
        var assessment = new ServingAssessment(state, West, null, null);

        Assert.Equal(state, assessment.State);
        Assert.Equal(failedOver, assessment.IsFailedOver);

        // The other parts of the record say nothing to the member: without the node expected to serve, with the same
        // node handed as the only one, and behind a healthy Front Door endpoint, the state is the whole answer.
        var otherParts = assessment with { Expected = null, OnlyNode = West, FrontDoorState = HealthState.Healthy };

        Assert.Equal(failedOver, otherParts.IsFailedOver);
    }
}
