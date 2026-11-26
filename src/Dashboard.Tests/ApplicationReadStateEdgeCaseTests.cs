namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ApplicationReadState" />, the three outcomes of asking
/// for a version's <c>&lt;version&gt;.json</c>, declared in <c>src/Dashboard/Runtime/ApplicationView.cs</c>.
/// <para>
/// What the other files already pin is left alone. <see cref="ApplicationViewTests" /> and
/// <see cref="ApplicationTextEdgeCaseTests" /> pin the line <see cref="ApplicationText.ProblemLine" /> says for each
/// state and its silence for <c>Read</c> and for numbers outside the three; <see cref="ApplicationViewTests" /> pins
/// which <see cref="ApplicationRead" /> the loader answers with for an answered, a missing and a failed file.
/// </para>
/// <para>
/// What follows is what those leave open: the three names in their declared order on the numbers 0 to 2, the
/// <see cref="int" /> underneath, that the unset state is <c>Read</c>, that the states are a choice and not flags, a
/// number outside the three having no name, the state each of <see cref="ApplicationRead.Missing" /> and
/// <see cref="ApplicationRead.Failed" /> carries, and text that is or is not a state when read by <see cref="Enum" />.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is.
/// </para>
/// </summary>
public class ApplicationReadStateEdgeCaseTests
{
    // ----- The three states, as the numbers and the names they are -----

    /// <summary>A state is one choice: the enum is public, not marked <see cref="FlagsAttribute" />, and an <see cref="int" />.</summary>
    [Fact]
    public void TheReadStatesAreAChoiceOfIntsAndNotASetOfFlags()
    {
        Assert.True(typeof(ApplicationReadState).IsEnum);
        Assert.True(typeof(ApplicationReadState).IsPublic);
        Assert.False(typeof(ApplicationReadState).IsDefined(typeof(FlagsAttribute), false));
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(typeof(ApplicationReadState)));
    }

    /// <summary>Exactly three states, in their declared order.</summary>
    [Fact]
    public void ThereAreExactlyThreeReadStatesInTheirDeclaredOrder()
    {
        Assert.Equal(["Read", "Missing", "Failed"], Enum.GetNames<ApplicationReadState>());
        Assert.Equal(
            [ApplicationReadState.Read, ApplicationReadState.Missing, ApplicationReadState.Failed],
            Enum.GetValues<ApplicationReadState>());
    }

    /// <summary>Each state on its own number and its own name, both ways round.</summary>
    [Theory]
    [InlineData(ApplicationReadState.Read, 0, "Read")]
    [InlineData(ApplicationReadState.Missing, 1, "Missing")]
    [InlineData(ApplicationReadState.Failed, 2, "Failed")]
    public void EachReadStateStandsOnItsOwnNumberAndItsOwnName(ApplicationReadState state, int number, string name)
    {
        Assert.Equal(number, (int)state);
        Assert.Equal(state, (ApplicationReadState)number);
        Assert.Equal(name, state.ToString());
        Assert.Equal(name, Enum.GetName(state));
        Assert.True(Enum.IsDefined(state));
        Assert.Equal(state, Enum.Parse<ApplicationReadState>(name));
    }

    /// <summary>The unset state is <c>Read</c>: a default value, and an <see cref="ApplicationRead" /> made of one, says the file was read.</summary>
    [Fact]
    public void TheUnsetReadStateIsRead()
    {
        Assert.Equal(ApplicationReadState.Read, default(ApplicationReadState));
        Assert.Equal(ApplicationReadState.Read, new ApplicationReadState());

        var read = new ApplicationRead(default, null, []);
        Assert.Equal(ApplicationReadState.Read, read.State);
        Assert.Null(read.Architecture);
        Assert.Empty(read.Problems);
    }

    /// <summary>One below the first, one above the last and either end of <see cref="int" /> have no name and say their number.</summary>
    [Fact]
    public void AValueOutsideTheThreeHasNoNameAndSaysItsNumber()
    {
        Assert.Null(Enum.GetName((ApplicationReadState)(-1)));
        Assert.Null(Enum.GetName((ApplicationReadState)3));
        Assert.Null(Enum.GetName((ApplicationReadState)int.MinValue));
        Assert.Null(Enum.GetName((ApplicationReadState)int.MaxValue));

        Assert.False(Enum.IsDefined((ApplicationReadState)(-1)));
        Assert.False(Enum.IsDefined((ApplicationReadState)3));
        Assert.Equal("-1", ((ApplicationReadState)(-1)).ToString());
        Assert.Equal("3", ((ApplicationReadState)3).ToString());
        Assert.Equal("2147483647", ((ApplicationReadState)int.MaxValue).ToString());
    }

    /// <summary>The three are apart from one another and sort by their numbers; a boxed <see cref="int" /> is no state's equal.</summary>
    [Fact]
    public void TheReadStatesAreApartAndSortByTheirNumber()
    {
        var states = Enum.GetValues<ApplicationReadState>();

        Assert.Equal(states.Length, states.Distinct().Count());
        Assert.Equal(states, states.Order());
        Assert.True(ApplicationReadState.Read < ApplicationReadState.Missing);
        Assert.True(ApplicationReadState.Missing < ApplicationReadState.Failed);
        Assert.NotEqual(ApplicationReadState.Missing, ApplicationReadState.Failed);
        Assert.False(ApplicationReadState.Failed.Equals(null));
        Assert.False(ApplicationReadState.Failed.Equals((object)2));
    }

    // ----- The outcomes that carry a state -----

    /// <summary>
    /// The named outcome <see cref="ApplicationRead.Missing" /> carries its state, no architecture and no problems, and
    /// is the same object every time; a failed read carries no architecture, and one made without reasons is the
    /// silence of <see cref="ApplicationReadState.Failed" />.
    /// </summary>
    [Fact]
    public void TheSharedOutcomesCarryTheStateOfTheirNameAndNoArchitecture()
    {
        Assert.Equal(ApplicationReadState.Missing, ApplicationRead.Missing.State);
        Assert.Null(ApplicationRead.Missing.Architecture);
        Assert.Empty(ApplicationRead.Missing.Problems);
        Assert.Same(ApplicationRead.Missing, ApplicationRead.Missing);

        var failed = ApplicationRead.Failed();
        Assert.Equal(ApplicationReadState.Failed, failed.State);
        Assert.Null(failed.Architecture);
        Assert.Empty(failed.Problems);

        Assert.NotEqual(ApplicationRead.Missing, failed);
        Assert.Equal(new ApplicationRead(ApplicationReadState.Missing, null, []), ApplicationRead.Missing);
        Assert.Equal(new ApplicationRead(ApplicationReadState.Failed, null, []), failed);
    }

    // ----- Text that is or is not a state -----

    /// <summary>Text that names a state in the enum's case, surrounding spaces walked past, or holds its number is that state.</summary>
    [Theory]
    [InlineData("Read", ApplicationReadState.Read)]
    [InlineData("Missing", ApplicationReadState.Missing)]
    [InlineData("Failed", ApplicationReadState.Failed)]
    [InlineData("  Missing  ", ApplicationReadState.Missing)]
    [InlineData("0", ApplicationReadState.Read)]
    [InlineData("2", ApplicationReadState.Failed)]
    public void AReadStateTextIsTheStateItNamesOrNumbers(string text, ApplicationReadState expected)
    {
        Assert.True(Enum.TryParse<ApplicationReadState>(text, out var state));
        Assert.Equal(expected, state);
    }

    /// <summary>Text that is no state is refused - null, empty, blank, another case, a near-word - and leaves <c>Read</c> behind.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("missing")]
    [InlineData("FAILED")]
    [InlineData("NotFound")]
    [InlineData("Failed!")]
    public void TextThatIsNoReadStateIsRefusedAndLeavesReadBehind(string? text)
    {
        Assert.False(Enum.TryParse<ApplicationReadState>(text, out var state));
        Assert.Equal(ApplicationReadState.Read, state);
    }

    /// <summary>A number outside the three is read as a state that is no state.</summary>
    [Fact]
    public void ANumberOutsideTheThreeIsReadAsAStateThatIsNoState()
    {
        Assert.True(Enum.TryParse<ApplicationReadState>("3", out var above));
        Assert.False(Enum.IsDefined(above));
        Assert.True(Enum.TryParse<ApplicationReadState>("-1", out var below));
        Assert.False(Enum.IsDefined(below));
    }
}
