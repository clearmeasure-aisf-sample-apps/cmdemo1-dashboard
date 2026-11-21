using System.Reflection;

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
/// </para>
/// <para>
/// What all of that leaves open, and what follows: the two kinds as the numbers, the names and the order they are
/// declared in, and nothing outside them; the unset kind, which is the public address rather than a kind of its own;
/// the two names as words, neither of them empty, neither a word of spaces, neither the other's in any case, and
/// neither of them the two words the page says out loud for Front Door; kinds apart from one another, sorted and
/// stepped, and combined as numbers, which a choice and not a set neither offers nor forbids; text that is or is not
/// a kind, in the enum's case and in none, with what a refused parse leaves behind; the surface the enum declares
/// itself, so that the day a third kind is added the list below says so; the kind a tile keeps as it is handed, even
/// one the enum does not name; and the kind that is absent, which matches no tile at all - the answer of the runtime
/// view for a box the topology holds no endpoint for.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a kind is a word the file wrote, and
/// every value below, the tiles included, is one the test brought.
/// </para>
/// </summary>
public class TargetKindEdgeCaseTests
{
    /// <summary>The two names, in the order the enum declares them: the public address first, the node behind it.</summary>
    private static readonly string[] Names = ["FrontDoor", "Node"];

    /// <summary>An address no check is made to and no page shows: the one every tile below is built with.</summary>
    private static readonly Uri Url = new("https://app-west.example.net");

    // ----- the two kinds, as the numbers they are -----

    /// <summary>The two kinds in the order the enum declares them, on the numbers 0 and 1, of an
    /// <see cref="int" /> underneath, standing one after another with no number left out and none twice. A kind is a
    /// choice and not a set: the enum wears no <see cref="FlagsAttribute" />, so a tile is one end or the other and
    /// never both.</summary>
    [Fact]
    public void TheTwoTargetKindsAreTheWholeVocabularyInTheirDeclaredOrder()
    {
        Assert.Equal(Names, Enum.GetNames<TargetKind>());
        Assert.Equal([0, 1], Enum.GetValues<TargetKind>().Select(kind => (int)kind));
        Assert.Equal(2, Enum.GetValues<TargetKind>().Length);
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(typeof(TargetKind)));
        Assert.False(typeof(TargetKind).IsDefined(typeof(FlagsAttribute), false));

        // The list of the values is a fresh array each time it is asked for, and the same two kinds in it.
        Assert.NotSame(Enum.GetValues<TargetKind>(), Enum.GetValues<TargetKind>());
        Assert.Equal(Enum.GetValues<TargetKind>(), Enum.GetValues<TargetKind>());
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
        Assert.False(Enum.IsDefined((TargetKind)(-1)));
        Assert.False(Enum.IsDefined((TargetKind)2));
        Assert.False(Enum.IsDefined((TargetKind)int.MinValue));
        Assert.False(Enum.IsDefined((TargetKind)int.MaxValue));

        Assert.Null(Enum.GetName((TargetKind)2));
        Assert.Null(Enum.GetName((TargetKind)(-1)));
        Assert.Equal("2", ((TargetKind)2).ToString());
        Assert.Equal("-1", ((TargetKind)(-1)).ToString());
        Assert.Equal("-2147483648", ((TargetKind)int.MinValue).ToString());
        Assert.Equal("2147483647", ((TargetKind)int.MaxValue).ToString());
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

    // ----- the two kinds apart from one another -----

    /// <summary>The two kinds are apart and sort by their numbers, as the declaration orders them: the public address
    /// the first of the two and the node the last, one step apart. A boxed <see cref="int" /> on the same number is
    /// no kind's equal, the box of nothing is nobody's, and a kind is its own - so a set of the two holds two, and
    /// the same kind handed twice is one.</summary>
    [Fact]
    public void TheTargetKindsAreApartFromOneAnotherAndSortByTheirNumber()
    {
        var kinds = Enum.GetValues<TargetKind>();

        Assert.Equal(kinds.Length, kinds.Distinct().Count());
        Assert.Equal(kinds, kinds.Order());
        Assert.True(TargetKind.FrontDoor < TargetKind.Node);
        Assert.False(TargetKind.Node <= TargetKind.FrontDoor);
        Assert.Equal(TargetKind.FrontDoor, kinds.Min());
        Assert.Equal(TargetKind.Node, kinds.Max());
        Assert.Equal(1, TargetKind.Node.CompareTo(TargetKind.FrontDoor));
        Assert.Equal(-1, TargetKind.FrontDoor.CompareTo(TargetKind.Node));
        Assert.Equal(0, TargetKind.Node.CompareTo((object)TargetKind.Node));
        Assert.Single(new HashSet<TargetKind> { TargetKind.Node, (TargetKind)1, TargetKind.Node });
        Assert.Equal(2, new HashSet<TargetKind>(kinds).Count);

        Assert.True(((object)TargetKind.Node).Equals(TargetKind.Node));
        Assert.False(TargetKind.Node.Equals(null));
        Assert.False(TargetKind.Node.Equals((object)1));
        Assert.False(TargetKind.FrontDoor.Equals((object)0));
        Assert.Equal(TargetKind.Node.GetHashCode(), ((object)TargetKind.Node).GetHashCode());
        Assert.Equal(typeof(TargetKind), TargetKind.Node.GetType());
    }

    // ----- the boundary: the kinds combined as numbers -----

    /// <summary>A kind is a choice and not a set: the operators work on the numbers underneath, so the two combine
    /// into the node - <c>FrontDoor</c> adding nothing, its number being zero - while an intersection of the two lands
    /// on the public address, the one end neither of them was jointly. An exclusive or of a kind with itself steps
    /// back to zero, the complement of a kind lands on no kind at all, and one step past either end is a number the
    /// enum does not name.</summary>
    [Fact]
    public void TwoTargetKindsCombinedAreAnotherKindOrNoKindAtAll()
    {
        Assert.Equal(TargetKind.Node, TargetKind.FrontDoor | TargetKind.Node);
        Assert.Equal(TargetKind.Node, TargetKind.Node | TargetKind.FrontDoor);
        Assert.Equal(TargetKind.FrontDoor, TargetKind.FrontDoor | TargetKind.FrontDoor);
        Assert.Equal(TargetKind.Node, TargetKind.Node | TargetKind.Node);
        Assert.Equal(TargetKind.FrontDoor, TargetKind.FrontDoor & TargetKind.Node);
        Assert.Equal(TargetKind.FrontDoor, TargetKind.Node & TargetKind.FrontDoor);
        Assert.Equal(TargetKind.Node, TargetKind.Node & TargetKind.Node);

        // A number the enum does not name combines with a kind it does: the bits land on a third number, and reading
        // them back one at a time gives the node, the number itself, and the public address neither of the two was.
        Assert.Equal((TargetKind)3, TargetKind.Node | (TargetKind)2);
        Assert.False(Enum.IsDefined(TargetKind.Node | (TargetKind)2));
        Assert.Equal(TargetKind.Node, (TargetKind)3 & TargetKind.Node);
        Assert.Equal((TargetKind)2, (TargetKind)3 & (TargetKind)2);
        Assert.Equal(TargetKind.FrontDoor, (TargetKind)2 & TargetKind.Node);

        // An exclusive or of a kind with itself is zero - the public address, though what it says is the absence of any bit.
        Assert.Equal(TargetKind.FrontDoor, TargetKind.Node ^ TargetKind.Node);
        Assert.Equal(TargetKind.Node, TargetKind.FrontDoor ^ TargetKind.Node);
        Assert.Equal((TargetKind)(-2), ~TargetKind.Node);
        Assert.False(Enum.IsDefined(~TargetKind.Node));

        // One past either end, and the step from one kind to the other.
        Assert.Equal((TargetKind)2, TargetKind.Node + 1);
        Assert.False(Enum.IsDefined(TargetKind.Node + 1));
        Assert.Equal((TargetKind)(-1), TargetKind.FrontDoor - 1);
        Assert.False(Enum.IsDefined(TargetKind.FrontDoor - 1));
        Assert.Equal(TargetKind.Node, TargetKind.FrontDoor + 1);
        Assert.Equal(TargetKind.FrontDoor, TargetKind.Node - 1);
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
        Assert.Equal((TargetKind)2, above);
        Assert.False(Enum.IsDefined(above));
        Assert.Null(Enum.GetName(above));

        Assert.True(Enum.TryParse<TargetKind>("-1", out var below));
        Assert.Equal((TargetKind)(-1), below);
        Assert.False(Enum.IsDefined(below));

        Assert.True(Enum.TryParse<TargetKind>("2147483647", out var top));
        Assert.Equal((TargetKind)int.MaxValue, top);
        Assert.False(Enum.IsDefined(top));

        Assert.False(Enum.TryParse<TargetKind>("node", out _));
        Assert.True(Enum.TryParse<TargetKind>("node", ignoreCase: true, out var forgiven));
        Assert.Equal(TargetKind.Node, forgiven);
        Assert.True(Enum.TryParse<TargetKind>("FRONTDOOR", ignoreCase: true, out var address));
        Assert.Equal(TargetKind.FrontDoor, address);
        Assert.True(Enum.TryParse<TargetKind>("2", ignoreCase: true, out var forgivenNumber));
        Assert.Equal(above, forgivenNumber);
        Assert.False(Enum.IsDefined(forgivenNumber));
        Assert.True(Enum.TryParse<TargetKind>("-01", ignoreCase: true, out var cipher));
        Assert.Equal((TargetKind)(-1), cipher);
    }

    // ----- the surface the enum declares -----

    /// <summary>The whole of what <see cref="TargetKind" /> declares itself, as the running assembly holds it: an
    /// <see cref="int" /> underneath, sealed and no set, and exactly its two kinds and nothing else - each one a
    /// public static literal of its own type, in the order the declaration writes them, on the number it stands on;
    /// and no instance member of its own beyond the storage every value type is given, so a kind carries nothing but
    /// the number it is. A third kind added to the file has to be written here as well, which is the point: a tile
    /// cannot be shown for an end of a deployable nobody decided what it means.</summary>
    [Fact]
    public void TheEnumDeclaresItsTwoKindsAndNothingElse()
    {
        var type = typeof(TargetKind);

        Assert.True(type.IsEnum);
        Assert.True(type.IsValueType);
        Assert.True(type.IsSealed);
        Assert.False(type.IsPrimitive);
        Assert.Equal(typeof(TargetKind), TargetKind.Node.GetType());

        // The only instance member a kind carries of its own is the storage the compiler gives every value type.
        var instance = type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.Single(instance);
        Assert.Equal("value__", instance[0].Name);
        Assert.Equal(typeof(int), ((FieldInfo)instance[0]).FieldType);

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.Equal(Names, fields.Select(field => field.Name));
        Assert.Equal([0, 1], fields.Select(field => (int)field.GetRawConstantValue()!));
        Assert.All(fields, field =>
        {
            Assert.Equal(type, field.FieldType);
            Assert.True(field.IsPublic);
            Assert.True(field.IsLiteral);
            Assert.False(field.IsInitOnly);
        });
    }

    // ----- the kind a tile stands on, and the kind that is absent -----

    /// <summary>The tile keeps the kind it is handed, however far outside the two it stands, and says nothing of its
    /// own about it: <see cref="TargetStatus.Kind" /> is the member the title, the subtitle and the links of the tile
    /// are read off, so the end a tile is built for is the end it stays for, exactly as it was given - a kind the enum
    /// does not name is carried along and reads back as the number it stands on, the rest of the tile being nothing
    /// the kind touched.</summary>
    [Fact]
    public void ATargetKeepsTheKindItIsGivenEvenOneTheTwoAreNot()
    {
        var address = new TargetStatus(TargetKind.FrontDoor, "Front Door", new Uri("https://demo.azurefd.net"));
        var node = new TargetStatus(TargetKind.Node, "app-west", Url, "westus3", "primary");
        var outside = new TargetStatus((TargetKind)2, "app-west", Url, "westus3", "primary");

        Assert.Equal(TargetKind.FrontDoor, address.Kind);
        Assert.Equal(TargetKind.Node, node.Kind);
        Assert.Equal((TargetKind)2, outside.Kind);
        Assert.False(Enum.IsDefined(outside.Kind));
        Assert.Equal("2", outside.Kind.ToString());

        // The kind is only a part of the tile, and the tile says nothing of its own about it: an undefined kind is
        // carried along, read back as the number it stands on and no kind the enum names.
        Assert.NotEqual(address, node);
        Assert.NotEqual(node, outside);
        Assert.Equal(node.Name, outside.Name);
        Assert.Equal(node.Url, outside.Url);
        Assert.Equal(outside.Kind, new TargetStatus(outside.Kind, outside.Name, outside.Url, outside.Region, outside.Role).Kind);
    }

    /// <summary>The kind of a box the topology holds no endpoint for is no kind at all: the runtime view looks for a
    /// tile by a kind that may be absent, and the comparison is lifted, so an absent kind matches neither the public
    /// address nor a node - not even the unset kind, whose number is zero. Asked for a kind, the absence of one gives
    /// nothing back until it is told what to stand for.</summary>
    [Fact]
    public void AnAbsentTargetKindMatchesNoTileAtAll()
    {
        TargetKind? absent = null;
        TargetKind? address = TargetKind.FrontDoor;

        Assert.False(absent.HasValue);
        Assert.Null(absent);
        Assert.False(TargetKind.FrontDoor == absent);
        Assert.False(TargetKind.Node == absent);
        Assert.True(TargetKind.FrontDoor == address);
        Assert.True(absent != address);
        Assert.Equal(TargetKind.FrontDoor, absent ?? TargetKind.FrontDoor);
        Assert.Null(absent ?? (TargetKind?)null);
        Assert.Equal("Node", (absent ?? TargetKind.Node).ToString());

        // A kind that is absent and a kind that is undefined are two different nothings: the one holds no number at
        // all, the other a number the enum does not name.
        Assert.Equal((TargetKind)2, absent ?? (TargetKind)2);
        Assert.False(Enum.IsDefined(absent ?? (TargetKind)2));
        Assert.False(((TargetKind?)TargetKind.FrontDoor).Equals(absent));
    }
}
