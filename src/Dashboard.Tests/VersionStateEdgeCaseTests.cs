namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="VersionState" />, the six words for how the pinned
/// version of a deployable stands next to what its nodes run, declared in <c>src/Dashboard/Health/VersionState.cs</c>
/// and settled by <see cref="VersionAssessment.Assess" />. The names, numbers, declared order and underlying type of
/// the six, and that 6 and -1 are not states, are pinned in <see cref="VersionAssessmentEdgeCaseTests" />. What that
/// leaves open is pinned here: the state an unset value stands on, the word each state says, the far ends of
/// <see cref="int" />, that the enum is a choice and not a set, and text that is or is not a state.
/// </summary>
public class VersionStateEdgeCaseTests
{
    /// <summary>An unset state is <see cref="VersionState.Pending" />: a value nobody assigned reads as the pin not
    /// being read yet, never as in sync or differing.</summary>
    [Fact]
    public void AnUnsetVersionStateIsPending()
    {
        Assert.Equal(VersionState.Pending, default);
        Assert.Equal(VersionState.Pending, (VersionState)0);
        Assert.Equal(VersionState.Pending, new VersionState());
        Assert.NotEqual(VersionState.InSync, default);
        Assert.NotEqual(VersionState.Differs, default);
    }

    /// <summary>Each state says its own name, both through <see cref="Enum.ToString()" /> and
    /// <see cref="Enum.GetName{TEnum}(TEnum)" />, and the name parses back to the same state.</summary>
    [Theory]
    [InlineData(VersionState.Pending, 0, "Pending")]
    [InlineData(VersionState.PinnedUnknown, 1, "PinnedUnknown")]
    [InlineData(VersionState.NotDeployed, 2, "NotDeployed")]
    [InlineData(VersionState.NodesUnknown, 3, "NodesUnknown")]
    [InlineData(VersionState.InSync, 4, "InSync")]
    [InlineData(VersionState.Differs, 5, "Differs")]
    public void EachVersionStateStandsOnItsOwnNumberAndItsOwnWord(VersionState state, int number, string name)
    {
        Assert.Equal(state, (VersionState)number);
        Assert.Equal(name, state.ToString());
        Assert.Equal(name, Enum.GetName(state));
        Assert.Equal(state, Enum.Parse<VersionState>(name));
        Assert.Equal(state, Enum.Parse<VersionState>(number.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        Assert.True(Enum.IsDefined(state));
    }

    /// <summary>The six names are told apart without regard to case, and are six distinct words whichever case
    /// they are read in.</summary>
    [Fact]
    public void TheSixNamesAreDistinctWithoutRegardToCase()
    {
        var names = Enum.GetNames<VersionState>();

        Assert.Equal(6, names.Length);
        Assert.Equal(6, names.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(6, Enum.GetValues<VersionState>().Distinct().Count());
    }

    /// <summary>The enum is a choice and not a set: it wears no <see cref="FlagsAttribute" />, so two states put
    /// together are no state, and a value it does not name says the number it stands on.</summary>
    [Fact]
    public void NothingOutsideTheSixIsAStateAtTheFarEndsOfInt()
    {
        Assert.False(typeof(VersionState).IsDefined(typeof(FlagsAttribute), false));

        Assert.False(Enum.IsDefined((VersionState)int.MinValue));
        Assert.False(Enum.IsDefined((VersionState)int.MaxValue));
        Assert.False(Enum.IsDefined(VersionState.NotDeployed | VersionState.InSync));

        Assert.Null(Enum.GetName((VersionState)6));
        Assert.Null(Enum.GetName((VersionState)(-1)));
        Assert.Equal("6", ((VersionState)6).ToString());
        Assert.Equal("-1", ((VersionState)(-1)).ToString());
        Assert.Equal("-2147483648", ((VersionState)int.MinValue).ToString());
        Assert.Equal("2147483647", ((VersionState)int.MaxValue).ToString());
    }

    /// <summary>A name in another case is a state only when the case is ignored on purpose.</summary>
    [Theory]
    [InlineData("insync", VersionState.InSync)]
    [InlineData("DIFFERS", VersionState.Differs)]
    [InlineData("pinnedunknown", VersionState.PinnedUnknown)]
    public void ANameInAnotherCaseIsAStateOnlyWhenCaseIsIgnored(string text, VersionState expected)
    {
        Assert.False(Enum.TryParse<VersionState>(text, out _));
        Assert.True(Enum.TryParse<VersionState>(text, ignoreCase: true, out var state));
        Assert.Equal(expected, state);
    }

    /// <summary>Text that is no state's name is not read as one: the empty text, blanks, a name with a space inside
    /// or a word the enum does not use - each fails and leaves the unset state behind.</summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("In Sync")]
    [InlineData("Unknown")]
    [InlineData("Differ")]
    [InlineData("InSync,Bogus")]
    public void TextThatIsNoStatesNameIsNotAState(string text)
    {
        Assert.False(Enum.TryParse<VersionState>(text, ignoreCase: true, out var state));
        Assert.Equal(default, state);
        Assert.ThrowsAny<ArgumentException>(() => Enum.Parse<VersionState>(text));
    }

    /// <summary>No text at all is no state either: <see langword="null" /> fails to parse and is refused by name.</summary>
    [Fact]
    public void NullTextIsNotAState()
    {
        Assert.False(Enum.TryParse<VersionState>(null, out var state));
        Assert.Equal(default, state);
        Assert.Throws<ArgumentNullException>(() => Enum.Parse<VersionState>((string)null!));
    }

    /// <summary>A number parses to a value even where the enum names no state there - it is
    /// <see cref="Enum.IsDefined{TEnum}(TEnum)" /> that tells the two apart.</summary>
    [Theory]
    [InlineData("6")]
    [InlineData("-1")]
    [InlineData("2147483647")]
    public void ANumberOutsideTheSixParsesButIsNotDefined(string text)
    {
        Assert.True(Enum.TryParse<VersionState>(text, out var state));
        Assert.False(Enum.IsDefined(state));
    }
}
