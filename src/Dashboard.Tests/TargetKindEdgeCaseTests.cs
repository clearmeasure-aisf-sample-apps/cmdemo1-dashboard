namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="TargetKind" />, the two words for what end of a
/// deployable a tile stands for, declared in <c>src/Dashboard/Health/TargetStatus.cs</c>: the public address of the
/// deployable and the regional web app behind it.
/// <para>
/// What is pinned where it belongs: <see cref="TargetStatusTests" /> pins the kind a tile is built with - a
/// deployable's Front Door endpoint carrying <see cref="TargetKind.FrontDoor" /> and listed first, its nodes carrying
/// <see cref="TargetKind.Node" /> in the order of the topology, and a deployable without a <c>frontDoorUrl</c> holding
/// nothing but nodes; <see cref="TargetTileClusterCardEnvironmentDeliveryTests" /> pins the tile's markup that reads
/// the kind - the title, the subtitle, the state link and the name link a Front Door tile takes and the ones a node's
/// tile takes; <see cref="HealthHelperCoverageTests" /> pins the observation of a Front Door tile; and
/// <see cref="RuntimePayloadBuilderTests" /> pins the node of the diagram a tile is found for by its kind and address.
/// <see cref="TargetKindBoundaryEdgeCaseTests" /> pins the kinds apart, sorted and stepped, combined as numbers, the
/// kind a tile stands on and the kind that is absent; <see cref="TargetKindSurfaceEdgeCaseTests" /> pins the surface
/// the enum declares itself; and <see cref="TargetStatusEdgeCaseTests" /> pins the tile a kind is carried on.
/// </para>
/// <para>
/// What all of that leaves open, and what follows: the two kinds as the numbers, the names and the order they are
/// declared in, and nothing outside them; the unset kind, which is the public address rather than a kind of its own;
/// the two names as words, neither of them empty, neither a word of spaces, neither the other's in any case, and
/// neither of them the two words the page says out loud for Front Door; and text that is or is not a kind, in the
/// enum's case and in none, with what a refused parse leaves behind.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a kind is a word the file wrote, and
/// every value below is one the test brought.
/// </para>
/// </summary>
public class TargetKindEdgeCaseTests
{
    /// <summary>The two names, in the order the enum declares them: the public address first, the node behind it.</summary>
    private static readonly string[] Names = ["FrontDoor", "Node"];

    /// <summary>The number each name in <see cref="Names" /> stands on, in the same order.</summary>
    private static readonly int[] Numbers = [0, 1];

    /// <summary>One past the last kind: the number the node steps to that the enum does not name.</summary>
    private const TargetKind OneAboveTheTwo = (TargetKind)2;

    /// <summary>One below the first kind: the number the public address steps to that the enum does not name.</summary>
    private const TargetKind OneBelowTheFirst = (TargetKind)(-1);

    /// <summary>The smallest number an <see cref="int" /> holds, read as a kind the enum does not name.</summary>
    private const TargetKind LowestNumber = (TargetKind)int.MinValue;

    /// <summary>The largest number an <see cref="int" /> holds, read as a kind the enum does not name.</summary>
    private const TargetKind HighestNumber = (TargetKind)int.MaxValue;

    // ----- the two kinds, as the numbers they are -----

    /// <summary>The two kinds in the order the enum declares them, on the numbers 0 and 1, of an
    /// <see cref="int" /> underneath, standing one after another with no number left out and none twice. A kind is a
    /// choice and not a set: the enum wears no <see cref="FlagsAttribute" />, so a tile is one end or the other and
    /// never both.</summary>
    [Fact]
    public void TheTwoTargetKindsAreTheWholeVocabularyInTheirDeclaredOrder()
    {
        var kinds = Enum.GetValues<TargetKind>();

        Assert.Equal(Names, Enum.GetNames<TargetKind>());
        Assert.Equal(Numbers, kinds.Select(kind => (int)kind));
        Assert.Equal(2, kinds.Length);
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(typeof(TargetKind)));
        Assert.False(typeof(TargetKind).IsDefined(typeof(FlagsAttribute), false));

        // The list of the values is a fresh array each time it is asked for, and the same two kinds in it.
        var kindsFirstAsk = Enum.GetValues<TargetKind>();
        var kindsSecondAsk = Enum.GetValues<TargetKind>();
        Assert.NotSame(kindsFirstAsk, kindsSecondAsk);
        Assert.Equal(kindsFirstAsk, kindsSecondAsk);
    }

    /// <summary>Each kind on its own number and its own word, both ways round: the number is the one the name stands
    /// on, the word is the one that number answers to, the word being what the kind says of itself - and both of the
    /// two are defined however they are arrived at.</summary>
    [Theory]
    [InlineData(TargetKind.FrontDoor, 0, "FrontDoor")]
    [InlineData(TargetKind.Node, 1, "Node")]
    public void EachTargetKindStandsOnItsOwnNumberAndItsOwnWord(TargetKind kind, int number, string name)
    {
        Assert.Equal(number, (int)kind);
        Assert.Equal(kind, (TargetKind)number);
        Assert.Equal(name, kind.ToString());
        Assert.Equal(name, Enum.GetName(kind));
        Assert.True(Enum.IsDefined(kind));
    }

    /// <summary>An unset kind is the first one, <c>FrontDoor</c>: a value type carries no state of its own, so the
    /// kind nobody chose is the public address - which is why a tile is never built without saying which end it
    /// stands for. Outside the two there is nothing: neither one below the first nor one above the last nor either
    /// end of <see cref="int" /> is a defined kind, and a value the enum does not name says the number it stands
    /// on.</summary>
    [Fact]
    public void AnUnsetTargetKindIsFrontDoorAndNothingOutsideTheTwoIsAKind()
    {
        Assert.Equal(TargetKind.FrontDoor, default);
        Assert.Equal(TargetKind.FrontDoor, (TargetKind)0);

        Assert.True(Enum.IsDefined(TargetKind.FrontDoor));
        Assert.True(Enum.IsDefined(TargetKind.Node));
        Assert.False(Enum.IsDefined(OneBelowTheFirst));
        Assert.False(Enum.IsDefined(OneAboveTheTwo));
        Assert.False(Enum.IsDefined(LowestNumber));
        Assert.False(Enum.IsDefined(HighestNumber));

        Assert.Null(Enum.GetName(OneAboveTheTwo));
        Assert.Null(Enum.GetName(OneBelowTheFirst));
        Assert.Equal("2", OneAboveTheTwo.ToString());
        Assert.Equal("-1", OneBelowTheFirst.ToString());
        Assert.Equal("-2147483648", LowestNumber.ToString());
        Assert.Equal("2147483647", HighestNumber.ToString());
    }

    // ----- the two names, as the words they are: none empty, none another's -----

    /// <summary>Every one of the two names is a word: not the empty word, not a word of nothing but spaces, nothing
    /// before it or after it, one word with no space in it and no letter in it but a letter of the alphabet, the
    /// first of them capitalized since that is the case the enum spells its kinds in. The two are apart in any case,
    /// and a third word - <c>Front Door</c>, the two words the page says out loud for the endpoint and the name a
    /// Front Door tile carries - is no name of the enum.</summary>
    [Fact]
    public void NoTargetKindNameIsTheEmptyWordOrAnotherName()
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

        Assert.Equal(2, new HashSet<string>(Names, StringComparer.Ordinal).Count);
        Assert.Equal(2, new HashSet<string>(Names, StringComparer.OrdinalIgnoreCase).Count);
        Assert.Distinct(Names);
        Assert.DoesNotContain(string.Empty, Names, StringComparer.Ordinal);
        Assert.DoesNotContain("Front Door", Names, StringComparer.Ordinal);
        Assert.DoesNotContain("frontdoor", Names, StringComparer.Ordinal);
    }

    // ----- text that is or is not a kind -----

    /// <summary>Text that names a kind in the enum's case, its surrounding spaces walked past, or holds its number is
    /// that kind; a name is read whole, and a number with a cipher in front of it is still the number it says.</summary>
    [Theory]
    [InlineData("FrontDoor", TargetKind.FrontDoor)]
    [InlineData("Node", TargetKind.Node)]
    [InlineData("  Node  ", TargetKind.Node)]
    [InlineData("\tFrontDoor", TargetKind.FrontDoor)]
    [InlineData("0", TargetKind.FrontDoor)]
    [InlineData("1", TargetKind.Node)]
    [InlineData("01", TargetKind.Node)]
    public void ATargetKindTextIsTheKindItNamesOrTheKindItNumbers(string text, TargetKind expected)
    {
        Assert.True(Enum.TryParse<TargetKind>(text, out var kind));
        Assert.Equal(expected, kind);
        Assert.True(Enum.IsDefined(kind));
        Assert.Equal(expected, Enum.Parse<TargetKind>(text));
    }

    /// <summary>Text that is no kind is refused - <see langword="null" />, the empty string, a blank of spaces or of a
    /// tab, the page's own two words for the endpoint, a name in another case, a name with something before it or
    /// behind it, a fraction, a hexadecimal, and the word of a health state that names no end of a deployable - and a
    /// refused parse leaves its answer standing on <see cref="TargetKind.FrontDoor" />: the <c>false</c> is the whole
    /// answer, and the kind it leaves behind is the unset one rather than the one the text was near. A number the enum
    /// does not name is no kind either, but it is read as one, which is what
    /// <see cref="ANumberOutsideTheTwoIsReadAsAKindThatIsNoKind" /> says.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("Front Door")]
    [InlineData("frontdoor")]
    [InlineData("FRONTDOOR")]
    [InlineData("node")]
    [InlineData("NODE")]
    [InlineData("Node!")]
    [InlineData("FrontDoor.")]
    [InlineData("FrontDo")]
    [InlineData("Frontdoorish")]
    [InlineData("Pending")]
    [InlineData("Healthy")]
    [InlineData("3.0")]
    [InlineData("0x1")]
    public void TextThatIsNoTargetKindIsRefusedAndLeavesFrontDoorBehind(string? text)
    {
        Assert.False(Enum.TryParse<TargetKind>(text, out var kind));
        Assert.Equal(TargetKind.FrontDoor, kind);
        Assert.False(Enum.TryParse<TargetKind>(text, ignoreCase: false, out _));
        Assert.True(Enum.IsDefined(kind));
    }

    /// <summary><see cref="Enum.Parse{TEnum}(string?, bool)" /> says what is wrong with text that is no kind: null is
    /// no text at all, the empty string and a blank are text without a kind in them, and a word is a word in the case
    /// the enum spells its kinds in and with no space in the middle of it.</summary>
    [Fact]
    public void ParsingTextThatIsNoTargetKindThrows()
    {
        Assert.Throws<ArgumentNullException>(() => Enum.Parse<TargetKind>(null!));
        Assert.Throws<ArgumentException>(() => Enum.Parse<TargetKind>(string.Empty));
        Assert.Throws<ArgumentException>(() => Enum.Parse<TargetKind>("   "));
        Assert.Throws<ArgumentException>(() => Enum.Parse<TargetKind>("frontdoor"));
        Assert.Throws<ArgumentException>(() => Enum.Parse<TargetKind>("Front Door"));
    }

    /// <summary>A number outside the two is read as a kind that is no kind - the top of <see cref="int" /> among them
    /// - while a name in another case is a kind as soon as the parse is told to forgive the case, and nothing until it
    /// is: what the forgiveness reaches is the case of a name, a number being read the same either way - and read
    /// whole, since a number with a sign or a cipher in front of it is still the number it says.</summary>
    [Fact]
    public void ANumberOutsideTheTwoIsReadAsAKindThatIsNoKind()
    {
        Assert.True(Enum.TryParse<TargetKind>("2", out var above));
        Assert.Equal(OneAboveTheTwo, above);
        Assert.False(Enum.IsDefined(above));
        Assert.Null(Enum.GetName(above));

        Assert.True(Enum.TryParse<TargetKind>("-1", out var below));
        Assert.Equal(OneBelowTheFirst, below);
        Assert.False(Enum.IsDefined(below));
        Assert.Null(Enum.GetName(below));

        Assert.True(Enum.TryParse<TargetKind>("2147483647", out var top));
        Assert.Equal(HighestNumber, top);
        Assert.False(Enum.IsDefined(top));
        Assert.Null(Enum.GetName(top));

        Assert.False(Enum.TryParse<TargetKind>("node", out _));
        Assert.True(Enum.TryParse<TargetKind>("node", ignoreCase: true, out var forgiven));
        Assert.Equal(TargetKind.Node, forgiven);
        Assert.True(Enum.TryParse<TargetKind>("FRONTDOOR", ignoreCase: true, out var address));
        Assert.Equal(TargetKind.FrontDoor, address);
        Assert.True(Enum.TryParse<TargetKind>("2", ignoreCase: true, out var forgivenNumber));
        Assert.Equal(above, forgivenNumber);
        Assert.False(Enum.IsDefined(forgivenNumber));
        Assert.True(Enum.TryParse<TargetKind>("-01", ignoreCase: true, out var cipher));
        Assert.Equal(OneBelowTheFirst, cipher);
    }
}
