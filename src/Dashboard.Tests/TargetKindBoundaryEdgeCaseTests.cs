namespace Dashboard.Tests;

/// <summary>
/// The boundary tests left open for <see cref="TargetKind" />, the two words for what end of a deployable a tile
/// stands for, declared in <c>src/Dashboard/Health/TargetStatus.cs</c>: the two kinds apart from one another, the
/// kinds combined as numbers, the kind a tile stands on and the kind that is absent.
/// <para>
/// <see cref="TargetKindEdgeCaseTests" /> already pins the enum's own edge cases - the two kinds as the numbers, the
/// names and the order they are declared in, the unset kind, the names as words, and the text that is or is not a
/// kind - and <see cref="TargetKindSurfaceEdgeCaseTests" /> pins the surface the enum declares itself; the precedent
/// is <see cref="SummaryLevelBoundaryEdgeCaseTests" /> set down over <see cref="SummaryLevelEdgeCaseTests" />. What
/// follows is the rest: the two kinds apart, sorted and stepped, and the identity the kind carries with nothing
/// else; the two kinds combined by the number operators, and the same operators stepping past the two or off the
/// vocabulary; the kind a tile keeps as it is handed, even one the enum does not name; and the kind that is absent,
/// which matches no tile at all - the answer of the runtime view for a box the topology holds no endpoint for.
/// <see cref="TargetStatusEdgeCaseTests" /> pins the tile a kind is carried on, and
/// <see cref="DeployableStatusEdgeCaseTests" /> the deployable the tile is one part of.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every kind, number and tile below is
/// one the test brought.
/// </para>
/// </summary>
public class TargetKindBoundaryEdgeCaseTests
{
    /// <summary>An address no check is made to and no page shows: the one every tile below is built with.</summary>
    private static readonly Uri Url = new("https://app-west.example.net");

    /// <summary>One past the last kind: the number the node steps to that the enum does not name.</summary>
    private const TargetKind OneAboveTheTwo = (TargetKind)2;

    /// <summary>One below the first kind: the number the public address steps to that the enum does not name.</summary>
    private const TargetKind OneBelowTheFirst = (TargetKind)(-1);

    /// <summary>The bits of the two kinds combined: a third number, neither kind's.</summary>
    private const TargetKind BothEndsBits = (TargetKind)3;

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

    /// <summary>The two kinds combined by or and and are one of the two: <c>FrontDoor</c> adds nothing, its number
    /// being zero, so the or of the two lands on the node and the and on the public address, in either order. The
    /// other three facts of the boundary - <see cref="AKindCombinedWithANumberOutsideTheTwoIsANumberTheEnumDoesNotName"
    /// />, <see cref="AnExclusiveOrAndAComplementStepOffTheVocabulary" /> and <see cref="AStepPastEitherEndIsNoKind" />
    /// - say what the same operators do when they step past the two or off the vocabulary altogether.</summary>
    [Fact]
    public void TheTwoKindsCombinedByOrAndAndAreOneOfTheTwo()
    {
        Assert.Equal(TargetKind.Node, TargetKind.FrontDoor | TargetKind.Node);
        Assert.Equal(TargetKind.Node, TargetKind.Node | TargetKind.FrontDoor);
        Assert.Equal(TargetKind.FrontDoor, TargetKind.FrontDoor | TargetKind.FrontDoor);
        Assert.Equal(TargetKind.Node, TargetKind.Node | TargetKind.Node);
        Assert.Equal(TargetKind.FrontDoor, TargetKind.FrontDoor & TargetKind.Node);
        Assert.Equal(TargetKind.FrontDoor, TargetKind.Node & TargetKind.FrontDoor);
        Assert.Equal(TargetKind.Node, TargetKind.Node & TargetKind.Node);
    }

    /// <summary>A kind combined with a number the enum does not name - the one above the two and the bits of the two
    /// together - is a number the enum does not name: the or lands on a third number with no word of the enum on it,
    /// and reading the bits back one at a time gives the node, the number itself, and the public address neither of
    /// the two was. The other facts of the boundary - <see cref="TheTwoKindsCombinedByOrAndAndAreOneOfTheTwo" />,
    /// <see cref="AnExclusiveOrAndAComplementStepOffTheVocabulary" /> and <see cref="AStepPastEitherEndIsNoKind" /> -
    /// say what the same operators do over the two kinds and past their ends.</summary>
    [Fact]
    public void AKindCombinedWithANumberOutsideTheTwoIsANumberTheEnumDoesNotName()
    {
        // A number the enum does not name combines with a kind it does: the bits land on a third number, and reading
        // them back one at a time gives the node, the number itself, and the public address neither of the two was.
        Assert.Equal(BothEndsBits, TargetKind.Node | OneAboveTheTwo);
        Assert.False(Enum.IsDefined(TargetKind.Node | OneAboveTheTwo));
        Assert.Equal(TargetKind.Node, BothEndsBits & TargetKind.Node);
        Assert.Equal(OneAboveTheTwo, BothEndsBits & OneAboveTheTwo);
        Assert.Equal(TargetKind.FrontDoor, OneAboveTheTwo & TargetKind.Node);
    }

    /// <summary>An exclusive or of a kind with itself is zero - the public address, though what it says is the absence
    /// of any bit - and the complement of a kind steps off the vocabulary: a number the enum does not name, with no
    /// word of the two for it. The other facts of the boundary - <see cref="TheTwoKindsCombinedByOrAndAndAreOneOfTheTwo" />
    /// , <see cref="AKindCombinedWithANumberOutsideTheTwoIsANumberTheEnumDoesNotName" /> and
    /// <see cref="AStepPastEitherEndIsNoKind" /> - say what the same operators do over the two kinds and past their
    /// ends.</summary>
    [Fact]
    public void AnExclusiveOrAndAComplementStepOffTheVocabulary()
    {
        // An exclusive or of a kind with itself is zero - the public address, though what it says is the absence of any bit.
        Assert.Equal(TargetKind.FrontDoor, TargetKind.Node ^ TargetKind.Node);
        Assert.Equal(TargetKind.Node, TargetKind.FrontDoor ^ TargetKind.Node);
        Assert.Equal((TargetKind)(-2), ~TargetKind.Node);
        Assert.False(Enum.IsDefined(~TargetKind.Node));
    }

    /// <summary>One step past either end is a number the enum does not name - the node stepping up and the public
    /// address stepping down - while the step from one kind to the other lands on the other kind: one apart, and not
    /// one beyond. The other facts of the boundary - <see cref="TheTwoKindsCombinedByOrAndAndAreOneOfTheTwo" />,
    /// <see cref="AKindCombinedWithANumberOutsideTheTwoIsANumberTheEnumDoesNotName" /> and
    /// <see cref="AnExclusiveOrAndAComplementStepOffTheVocabulary" /> - say what the same operators do when the
    /// numbers are combined rather than stepped.</summary>
    [Fact]
    public void AStepPastEitherEndIsNoKind()
    {
        // One past either end, and the step from one kind to the other.
        Assert.Equal(OneAboveTheTwo, TargetKind.Node + 1);
        Assert.False(Enum.IsDefined(TargetKind.Node + 1));
        Assert.Equal(OneBelowTheFirst, TargetKind.FrontDoor - 1);
        Assert.False(Enum.IsDefined(TargetKind.FrontDoor - 1));
        Assert.Equal(TargetKind.Node, TargetKind.FrontDoor + 1);
        Assert.Equal(TargetKind.FrontDoor, TargetKind.Node - 1);
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
        var outside = new TargetStatus(OneAboveTheTwo, "app-west", Url, "westus3", "primary");

        Assert.Equal(TargetKind.FrontDoor, address.Kind);
        Assert.Equal(TargetKind.Node, node.Kind);
        Assert.Equal(OneAboveTheTwo, outside.Kind);
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

    /// <summary>The kind a tile is handed is the part of the tile it is: <see cref="TargetStatus.Kind" /> reads it
    /// back exactly, the named kinds by their names and the kind the enum does not name by its number, and an
    /// undefined kind stays undefined, saying only the number it stands on. The kind touches nothing else of the
    /// tile: the name, the address, the region and the role of a tile built with the undefined kind stand where the
    /// words of a node tile put them, and nowhere else does the kind reach.</summary>
    [Fact]
    public void TheKindOfATileReadsBackWhatItWasGivenAndLeavesTheRestUntouched()
    {
        var frontDoor = new TargetStatus(TargetKind.FrontDoor, "Front Door", new Uri("https://demo.azurefd.net"));
        var node = new TargetStatus(TargetKind.Node, "app-west", Url, "westus3", "primary");
        var undefined = new TargetStatus(OneAboveTheTwo, "app-west", Url, "westus3", "primary");

        Assert.Equal(TargetKind.FrontDoor, frontDoor.Kind);
        Assert.Equal(TargetKind.Node, node.Kind);
        Assert.Equal(OneAboveTheTwo, undefined.Kind);

        Assert.False(Enum.IsDefined(undefined.Kind));
        Assert.Equal("2", undefined.Kind.ToString());

        Assert.Equal(node.Name, undefined.Name);
        Assert.Equal(node.Url, undefined.Url);
        Assert.Equal(node.Region, undefined.Region);
        Assert.Equal(node.Role, undefined.Role);
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
        Assert.Equal(OneAboveTheTwo, absent ?? OneAboveTheTwo);
        Assert.False(Enum.IsDefined(absent ?? OneAboveTheTwo));
        Assert.False(((TargetKind?)TargetKind.FrontDoor).Equals(absent));
    }
}
