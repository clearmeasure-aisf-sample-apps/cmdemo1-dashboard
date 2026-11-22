using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The boundaries, the null/empty cases and the fixed values of <see cref="RuntimeManifest" /> that its other files
/// leave open: the record of <c>runtime/&lt;env&gt;.json</c>, the environment's name and the five lists of what the
/// diagram draws - its boxes, its regions, its edges, its frames - and of what the deployment lists beneath it.
/// <para>
/// What is already pinned is left alone. <see cref="RuntimeManifestEdgeCaseTests" /> pins that each part comes back as
/// the part it went in as, that the two parts a caller may leave out stand as nothing rather than as an empty list,
/// that the lists are compared as the lists they stand on and not by what they hold, that the environment is weighed
/// ordinally and kept as it is written, that <c>with</c> changes only the part it names, that the six parts fall out
/// in the order they went in, the words of a manifest that names four of its parts, and the six parts and the members
/// every record is asked. The records the lists hold - <see cref="RuntimeUnplaced" />, <see cref="RuntimeNode" />,
/// <see cref="RuntimeRegion" />, <see cref="RuntimeEdge" /> and <see cref="RuntimeFrame" /> - are pinned by
/// <see cref="RuntimeManifestEdgeCaseTests" />, <see cref="RuntimeNodeEdgeCaseTests" />,
/// <see cref="RuntimeRegionEdgeCaseTests" />, <see cref="RuntimeEdgeEdgeCaseTests" /> and
/// <see cref="RuntimeFrameEdgeCaseTests" />; the reading of a manifest out of a file by
/// <see cref="RuntimeManifestParserTests" />, <see cref="RuntimeManifestParserEdgeCaseTests" /> and
/// <see cref="RuntimeManifestParserBoundaryEdgeCaseTests" />; and what the page makes of a manifest by
/// <see cref="RuntimePayloadBuilderTests" />, <see cref="UnplacedTextEdgeCaseTests" /> and
/// <see cref="ApplicationViewTests" />.
/// </para>
/// <para>
/// What stands here is the shape of the six parts as members and not only as arguments - read at will, written only as
/// the manifest is made or copied, annotated nullable at exactly the two a caller may leave out; the lists held and
/// never copied nor opened, so that the box a page reads back is the box it was given and a manifest is as steady as
/// the list it stands on; the words a manifest says of itself when all six parts are named, when none is, and when the
/// holds are of one another kind, the same words to a reader of German; a manifest compared with itself, with nothing,
/// and with every other record of the same diagram; its hash standing from call to call while each of the six parts
/// sets one manifest apart; the empty environment, the environment of a space, the environment of two of them and the
/// environment that is no word being four environments, the nothing reached by the copy as well as by the constructor;
/// the manifest that draws nothing standing apart from the manifest that was handed no parts at all, and the emptiness
/// of a list being one shared hold the record cannot tell from another; and a manifest refusing nothing it is handed,
/// not even a list of boxes that are no box.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: a manifest holds a word and five lists, calls
/// nothing of its own, and every value below is one the test brought.
/// </para>
/// </summary>
public class RuntimeManifestBoundaryEdgeCaseTests
{
    private const string Environment = "uat";

    /// <summary>The line a manifest of six named parts says of itself: the six holds named, and none of them walked,
    /// which is why the same words serve a diagram of one box and a diagram of none.</summary>
    private const string SixHolds =
        "RuntimeManifest { Environment = uat, Nodes = Dashboard.Runtime.RuntimeNode[], "
        + "Regions = Dashboard.Runtime.RuntimeRegion[], Edges = Dashboard.Runtime.RuntimeEdge[], "
        + "Frames = Dashboard.Runtime.RuntimeFrame[], Unplaced = Dashboard.Runtime.RuntimeUnplaced[] }";

    /// <summary>The line a manifest of no part at all says of itself: every part named, every part blank.</summary>
    private const string SixBlanks =
        "RuntimeManifest { Environment = , Nodes = , Regions = , Edges = , Frames = , Unplaced =  }";

    private static readonly RuntimeNode[] Nodes = [new("web_primary", RuntimeNodeKind.WebApp, "app-uat")];
    private static readonly RuntimeRegion[] Regions = [new("region_primary", "southcentralus", ["primary"])];
    private static readonly RuntimeEdge[] Edges = [new("browser-to-web_primary", "browser", "web_primary", RuntimeEdgeKind.Public)];
    private static readonly RuntimeFrame[] Frames = [new("sub", "subscription", "demo")];
    private static readonly RuntimeUnplaced[] Unplaced = [new("stray", "Storage account", "rg-demo", "no environment tag")];

    private static readonly RuntimeNode[] NoNodes = [];
    private static readonly RuntimeRegion[] NoRegions = [];
    private static readonly RuntimeEdge[] NoEdges = [];
    private static readonly RuntimeFrame[] NoFrames = [];
    private static readonly RuntimeUnplaced[] NoUnplaced = [];

    /// <summary>A manifest with all six parts named, each of them a hold of its own, so that a part landing on another
    /// is seen instead of quietly matching a second part of the record.</summary>
    private static RuntimeManifest Full => new(Environment, Nodes, Regions, Edges, Frames, Unplaced);

    /// <summary>A manifest of the four parts a caller must name: one from before the frames and the unplaced list were
    /// listed, the two later parts standing as the absence they are.</summary>
    private static RuntimeManifest Bare => new(Environment, Nodes, Regions, Edges);

    /// <summary>The manifest of no parts at all: the environment no word, the lists nothing - the manifest the record
    /// will hold although no file may say it.</summary>
    private static RuntimeManifest NoParts => new(null!, null!, null!, null!);

    // ------------------------------------------- the six parts as members, not only as arguments

    /// <summary>The six parts as the page compiles against them: each read at will, and each given its value only as
    /// the manifest is made or copied, the record handing out no setter to walk back through a diagram that is being
    /// drawn. Each part stands annotated the nullability the record declares - the environment and the four lists of
    /// the diagram always something to read, the frames and the unplaced list only where the file listed them - which
    /// is what the page reads a manifest through, <c>Frames is null</c> being the difference between an older manifest
    /// and one that lists no frame at all. That the constructor's six arguments carry the same annotations is pinned by
    /// <see cref="RuntimeManifestEdgeCaseTests" />; the parts themselves are pinned here.</summary>
    [Fact]
    public void TheSixPartsAreReadAtWillAndWrittenOnlyAsTheManifestIsMadeOrCopied()
    {
        var properties = typeof(RuntimeManifest)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToArray();

        Assert.Equal(
            ["Environment", "Nodes", "Regions", "Edges", "Frames", "Unplaced"],
            [.. properties.Select(property => property.Name)]);

        var nullability = new NullabilityInfoContext();

        Assert.Equal(
            [false, false, false, false, true, true],
            [.. properties.Select(property => nullability.Create(property).ReadState == NullabilityState.Nullable)]);
        Assert.Equal(
            [false, false, false, false, true, true],
            [.. properties.Select(property => nullability.Create(property).WriteState == NullabilityState.Nullable)]);

        Assert.All(
            properties,
            property =>
            {
                Assert.True(property.CanRead);
                Assert.NotNull(property.GetMethod);
                Assert.True(property.GetMethod!.IsPublic);
                Assert.True(property.CanWrite);
                Assert.NotNull(property.SetMethod);
                Assert.True(property.SetMethod!.IsPublic);

                // Init only: the part is given its value as the manifest is made or copied, and never again after.
                Assert.Contains(typeof(IsExternalInit), property.SetMethod.ReturnParameter.GetRequiredCustomModifiers());
            });

        // Nothing else to call, to set, to subscribe to or to inherit from: the record decides nothing of its own.
        Assert.Empty(typeof(RuntimeManifest).GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Empty(typeof(RuntimeManifest).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Empty(typeof(RuntimeManifest).GetEvents());
        Assert.Equal(typeof(object), typeof(RuntimeManifest).BaseType);
        Assert.True(typeof(RuntimeManifest).IsSealed);
        Assert.False(typeof(RuntimeManifest).IsAbstract);
    }

    // ------------------------------------------- the lists held, never copied nor opened

    /// <summary>A manifest holds the very lists it was handed and copies nothing out of them: the box read back is the
    /// box that was handed over, the empty list a list the manifest stands on, and a copy carrying the same hold
    /// rather than a picture of it. Because the manifest holds the list and not what stands in it, what a manifest
    /// draws is what its list holds at the moment it is asked - a box added to the list after the manifest was made is
    /// a box the page draws out of that manifest - while the manifest itself rides on the hold and stays one with the
    /// manifest of the same five lists, in equality and in hash, as it was before the box was added. That two lists of
    /// one and the same content are two manifests is pinned by <see cref="RuntimeManifestEdgeCaseTests" />; what
    /// stands here is that the record makes no copy of the one, so a manifest is as steady as the list the reader
    /// built for it - and as two reads of one file are two manifests, being two lists.</summary>
    [Fact]
    public void AManifestHoldsTheListsItIsHandedAndCopiesNothingOutOfThem()
    {
        var boxes = new List<RuntimeNode> { new("web_primary", RuntimeNodeKind.WebApp, "app-uat") };
        var theBox = boxes[0];
        var manifest = new RuntimeManifest(Environment, boxes, Regions, Edges, Frames, Unplaced);
        var askedBefore = manifest.GetHashCode();
        var alsoManifest = new RuntimeManifest(Environment, boxes, Regions, Edges, Frames, Unplaced);

        Assert.Same(boxes, manifest.Nodes);
        Assert.Same(theBox, Assert.Single(manifest.Nodes));
        Assert.Same(theBox, manifest.Nodes[0]);
        Assert.Same(theBox, (manifest with { }).Nodes[0]);
        Assert.Same(boxes, (manifest with { Environment = "prod" }).Nodes);

        // The empty lists are holds too, and each of them the very one that was handed over.
        var nothingDrawn = new RuntimeManifest(Environment, NoNodes, NoRegions, NoEdges, NoFrames, NoUnplaced);

        Assert.Same(NoNodes, nothingDrawn.Nodes);
        Assert.Same(NoRegions, nothingDrawn.Regions);
        Assert.Same(NoEdges, nothingDrawn.Edges);
        Assert.Same(NoFrames, nothingDrawn.Frames);
        Assert.Same(NoUnplaced, nothingDrawn.Unplaced);

        // The hold and not what stands on it: the list grows, the manifest draws another diagram, and the manifest is
        // the manifest it was - its equality and its hash weighing the hold they both stand on.
        boxes.Add(new RuntimeNode("web_standby", RuntimeNodeKind.WebApp, "app-uat-standby"));

        Assert.Equal(2, manifest.Nodes.Count);
        Assert.Equal("web_standby", manifest.Nodes[1].Alias);
        Assert.Same(boxes, manifest.Nodes);
        Assert.True(manifest == alsoManifest);
        Assert.Equal(manifest, alsoManifest);
        Assert.Equal(askedBefore, manifest.GetHashCode());
        Assert.Single(new HashSet<RuntimeManifest> { manifest, alsoManifest });

        // A second list of the same boxes is a second manifest, although the record never opens either list to find
        // out what stands in them.
        Assert.NotEqual(manifest, new RuntimeManifest(Environment, [.. boxes], Regions, Edges, Frames, Unplaced));
    }

    // ------------------------------------------- the words a manifest says of itself

    /// <summary>The words a manifest says of itself: its name, then its six parts in the order they are declared, each
    /// named and standing blank where there is nothing of it. The line names the holds and never walks them, so the
    /// manifest of one box in each list and the manifest of an empty list in each are said in one and the same words,
    /// and a list that is a list rather than an array says the list it stands on. The environment is said as it was
    /// written - spaces and markup and all, the record escaping nothing - and no part of a manifest is a number a
    /// reader spells, so a reader of German is given the very line the invariant reader is given. The words the page
    /// is given out of a manifest are elsewhere, and are pinned with the builder.</summary>
    [Fact]
    public void AManifestSaysItsSixPartsInTheirOrderAndOpensNoneOfThem()
    {
        var full = Full;
        var boxes = new List<RuntimeNode>(Nodes);

        Assert.Equal(SixHolds, full.ToString());

        // The holds are named and never walked: nothing in them is asked, so an empty list says the same words as a
        // full one, and a manifest of a box and a manifest of none are told apart by nothing of the line.
        Assert.Equal(SixHolds, new RuntimeManifest(Environment, NoNodes, NoRegions, NoEdges, NoFrames, NoUnplaced).ToString());

        // The two parts a caller leaves out are named and say nothing, which is how the words of an older manifest
        // tell it from one that lists them empty.
        Assert.Equal(
            "RuntimeManifest { Environment = uat, Nodes = Dashboard.Runtime.RuntimeNode[], "
            + "Regions = Dashboard.Runtime.RuntimeRegion[], Edges = Dashboard.Runtime.RuntimeEdge[], Frames = , "
            + "Unplaced =  }",
            Bare.ToString());

        // The manifest of nothing at all still says its six parts, and asking them dereferences nothing.
        Assert.Equal(SixBlanks, NoParts.ToString());

        // A list says the list and not the boxes it holds - the same line asks nothing of either, whether the hold is
        // a list the caller made or the array the record was handed above.
        Assert.Contains(
            "Nodes = System.Collections.Generic.List`1[Dashboard.Runtime.RuntimeNode], ",
            new RuntimeManifest(Environment, boxes, NoRegions, NoEdges, null, null).ToString(),
            StringComparison.Ordinal);

        // The environment is said exactly as it stands, the word of markup and the spaces about it included.
        Assert.Contains(
            "RuntimeManifest { Environment =  <b>uat</b> , ",
            (Bare with { Environment = " <b>uat</b> " }).ToString()!,
            StringComparison.Ordinal);

        ViewerCulture.German(
            () =>
            {
                Assert.Equal(SixHolds, full.ToString());
                Assert.Equal(SixBlanks, NoParts.ToString());
            });
    }

    // ------------------------------------------- a manifest, nothing else, and nothing at all

    /// <summary>The nothing on either side of the members every record is asked, and the records a manifest is made
    /// of: two manifests that are no manifest are one, a manifest and no manifest are two - the equality and the
    /// inequality each saying it from either side - and a manifest is never one with the environment's name it
    /// carries, with the list of boxes it holds, with a kind of box, or with any other record of the one diagram. That
    /// is what keeps a manifest, a box, a region, an edge, a frame, an unplaced resource and the index that lists the
    /// environments out of one another in a page that reads them all.</summary>
    [Fact]
    public void AManifestIsOneWithItselfAndWithNoOtherRecordOfTheDiagram()
    {
        var manifest = Full;
        var itself = manifest;
        RuntimeManifest? gone = null;
        RuntimeManifest? alsoGone = null;

        Assert.True(manifest.Equals(manifest));
        Assert.True(manifest.Equals((object)manifest));
        Assert.True(manifest == itself);
        Assert.False(manifest != itself);

        Assert.True(gone == alsoGone);
        Assert.False(gone != alsoGone);
        Assert.Null(default(RuntimeManifest));
        Assert.False(manifest == gone);
        Assert.False(gone == manifest);
        Assert.True(manifest != gone);
        Assert.True(gone != manifest);
        Assert.False(manifest.Equals(gone));
        Assert.False(manifest.Equals((RuntimeManifest?)null));
        Assert.False(manifest.Equals((object?)null));

        Assert.False(manifest.Equals(new RuntimeNode("web_primary", RuntimeNodeKind.WebApp, "app-uat")));
        Assert.False(manifest.Equals(new RuntimeRegion("region_primary", "southcentralus", ["primary"])));
        Assert.False(manifest.Equals(new RuntimeEdge("browser-to-web_primary", "browser", "web_primary", RuntimeEdgeKind.Public)));
        Assert.False(manifest.Equals(new RuntimeFrame("sub", "subscription", "demo")));
        Assert.False(manifest.Equals(new RuntimeUnplaced("stray")));
        Assert.False(manifest.Equals(new RuntimeDatabaseFiles("db.svg", "db.json")));
        Assert.False(manifest.Equals(new RuntimeIndex([new RuntimeIndexEntry("uat", "uat.json", "uat.svg")], "1.2026.8")));
        Assert.False(manifest.Equals(new RuntimeIndexEntry("uat", "uat.json", "uat.svg", new RuntimeDatabaseFiles("db.svg", "db.json"))));
        Assert.False(manifest.Equals(Environment));
        Assert.False(manifest.Equals(Nodes));
        Assert.False(manifest.Equals(RuntimeNodeKind.WebApp));
        Assert.False(manifest.Equals(RuntimeEdgeKind.Public));

        // The box of the diagram is no manifest of it, whichever of the two ways the box is reached.
        Assert.False(manifest.Equals(manifest.Nodes[0]));
    }

    // ------------------------------------------- the hash, and the parts that set it

    /// <summary>The hash of a manifest: standing from call to call, and one hash for every manifest made of one set of
    /// parts by whichever of the three ways of making one - the constructor, the copy that names nothing, and the
    /// parts asked back out of the manifest itself. Nine manifests - the one of six named parts, the copy of it that
    /// changes nothing, the seven changed each in one part, and the manifest of the four parts a caller must name -
    /// are eight in a set, the copy that names nothing standing with the manifest it was copied from. That the six
    /// parts each tell two manifests apart is pinned by <see cref="RuntimeManifestEdgeCaseTests" />; what stands here
    /// is that the hash follows them and does not wander between the askings of one manifest.</summary>
    [Fact]
    public void TheHashOfAManifestStandsFromCallToCallAndFollowsItsSixParts()
    {
        foreach (var manifest in new[] { Full, Bare, NoParts })
        {
            var hash = manifest.GetHashCode();

            Assert.Equal(hash, manifest.GetHashCode());
            Assert.Equal(hash, (manifest with { }).GetHashCode());
            Assert.Equal(hash, ((object)manifest).GetHashCode());
            Assert.Equal(
                hash,
                new RuntimeManifest(
                    manifest.Environment, manifest.Nodes, manifest.Regions, manifest.Edges, manifest.Frames, manifest.Unplaced).GetHashCode());
        }

        var full = Full;
        RuntimeManifest[] variants =
        [
            full,
            full with { },
            full with { Environment = "prod" },
            full with { Nodes = [.. Nodes] },
            full with { Regions = [.. Regions] },
            full with { Edges = [.. Edges] },
            full with { Frames = [.. Frames] },
            full with { Unplaced = [.. Unplaced] },
            Bare,
        ];

        Assert.Equal(8, variants.Distinct().Count());
        Assert.Equal(8, new HashSet<RuntimeManifest>(variants).Count);
    }

    // ------------------------------------------- the environment at its ends

    /// <summary>The environment at the ends of the word it is: the empty word, the word of one space, the word of two
    /// of them and the nothing that is no word are four environments and, with the word that is a word, five
    /// manifests. The copy is the road to all four - and to the nothing of them: the record annotates the part as one
    /// a caller must name and refuses it nothing, so <c>with { Environment = null! }</c> lands the null on the
    /// property exactly as the constructor's first argument does, which is why it is the reader of the file that
    /// decides a manifest has an environment and not the record. That the constructor holds the nothing, and that a
    /// word is weighed ordinally and kept as it is written, is pinned by
    /// <see cref="RuntimeManifestEdgeCaseTests" />.</summary>
    [Fact]
    public void TheEmptyEnvironmentTheSpacesAndTheNothingAreFourEnvironments()
    {
        var there = Bare;
        var blank = there with { Environment = string.Empty };
        var oneSpace = there with { Environment = " " };
        var twoSpaces = there with { Environment = "  " };
        var nothing = there with { Environment = null! };

        Assert.Equal(string.Empty, blank.Environment);
        Assert.Equal(" ", oneSpace.Environment);
        Assert.Equal("  ", twoSpaces.Environment);
        Assert.Null(nothing.Environment);

        Assert.False(there.Equals(blank));
        Assert.False(blank.Equals(oneSpace));
        Assert.False(oneSpace.Equals(twoSpaces));
        Assert.False(there.Equals(nothing));
        Assert.Equal(
            5,
            new HashSet<RuntimeManifest> { there, blank, oneSpace, twoSpaces, nothing }.Count);

        // The nothing that is no environment arrives the same manifest however it is reached: by a copy that blanks
        // the word of a manifest of four parts, or by the manifest of no parts at all.
        Assert.Equal(nothing, NoParts with { Nodes = Nodes, Regions = Regions, Edges = Edges });
        Assert.Equal(nothing.GetHashCode(), (NoParts with { Nodes = Nodes, Regions = Regions, Edges = Edges }).GetHashCode());
    }

    // ------------------------------------------- the manifest that draws nothing, and the one with no parts

    /// <summary>The two absences a manifest stands on: the file that draws nothing is a manifest of an environment and
    /// of five lists with nothing in them, and the manifest that was handed no parts at all is another manifest
    /// altogether - its lists being nothing and not empty. That difference is the one the page reads as <c>a diagram
    /// with no box</c> and <c>a manifest that says nothing of either</c>, and it is not a difference the words of the
    /// line tell: both name their six parts, one blanking the holds and the other naming five empty ones. A fresh
    /// empty list is a fresh hold, so the copy that empties every list makes another manifest rather than the same one
    /// emptied.</summary>
    [Fact]
    public void TheManifestThatDrawsNothingIsAnotherThanTheManifestThatWasHandedNoParts()
    {
        var drawn = new RuntimeManifest(Environment, NoNodes, NoRegions, NoEdges, NoFrames, NoUnplaced);
        var noParts = NoParts;

        Assert.NotEqual(drawn, noParts);
        Assert.False(drawn.Equals(noParts));
        Assert.False(noParts.Equals(drawn));
        Assert.True(drawn != noParts);
        Assert.Equal(2, new HashSet<RuntimeManifest> { drawn, noParts }.Count);

        Assert.Equal(Environment, drawn.Environment);
        Assert.Empty(drawn.Nodes);
        Assert.Empty(drawn.Regions);
        Assert.Empty(drawn.Edges);
        Assert.Empty(drawn.Frames!);
        Assert.Empty(drawn.Unplaced!);
        Assert.Null(noParts.Environment);
        Assert.Null(noParts.Nodes);
        Assert.Null(noParts.Regions);
        Assert.Null(noParts.Edges);
        Assert.Null(noParts.Frames);
        Assert.Null(noParts.Unplaced);

        // The line tells them nothing of the difference: five empty holds are said as the five full ones are, and the
        // five absences as blanks.
        Assert.Equal(SixHolds, drawn.ToString());
        Assert.Equal(SixBlanks, noParts.ToString());

        // The empty list of a copy is the one empty array the runtime hands out, so the record - which weighs the hold
        // and never walks it - cannot tell a manifest that draws nothing from the same manifest emptied by a copy: two
        // references, one manifest. A second empty list written by hand, being a list and not that array, is another
        // manifest. That is the whole of what the record knows about an emptiness it is never asked to look into.
        var emptied = drawn with { Nodes = [], Regions = [], Edges = [], Frames = [], Unplaced = [] };

        Assert.Equal(drawn, emptied);
        Assert.True(drawn == emptied);
        Assert.NotSame(drawn, emptied);
        Assert.Same(NoNodes, emptied.Nodes);
        Assert.Equal(drawn.GetHashCode(), emptied.GetHashCode());
        Assert.Single(new HashSet<RuntimeManifest> { drawn, emptied });
        Assert.NotEqual(emptied, noParts);
        Assert.NotEqual(drawn, drawn with { Nodes = [.. NoNodes] });
        Assert.True(drawn == (drawn with { }));
        Assert.NotSame(drawn, drawn with { });

        // And an empty list is none of them the absence of one, part by part.
        Assert.NotEqual(drawn, drawn with { Nodes = null! });
        Assert.NotEqual(drawn, drawn with { Regions = null! });
        Assert.NotEqual(drawn, drawn with { Edges = null! });
        Assert.NotEqual(drawn, drawn with { Frames = null });
        Assert.NotEqual(drawn, drawn with { Unplaced = null });
    }

    // ------------------------------------------- the record that refuses nothing

    /// <summary>A manifest refuses nothing it is handed, not even a list of boxes that are no box: the record asks
    /// nothing of what its lists hold, since it never opens them, so a manifest of no environment and of lists of
    /// nothing compares, hashes, copies and says itself without a word of complaint. That no file may say such things
    /// is the reader's, and is pinned with the reader; what is pinned here is that the record is not what stops
    /// them.</summary>
    [Fact]
    public void AManifestRefusesNothingItIsHanded()
    {
        RuntimeNode[] boxes = [null!, new RuntimeNode("web_primary", RuntimeNodeKind.WebApp, "app-uat")];
        RuntimeRegion[] regions = [null!];
        RuntimeEdge[] edges = [null!];
        RuntimeFrame[] frames = [null!];
        RuntimeUnplaced[] unplaced = [null!];

        var manifest = new RuntimeManifest(null!, boxes, regions, edges, frames, unplaced);

        Assert.Null(manifest.Environment);
        Assert.Null(manifest.Nodes[0]);
        Assert.Equal(2, manifest.Nodes.Count);
        Assert.Same(boxes, manifest.Nodes);
        Assert.Same(boxes, (manifest with { }).Nodes);
        Assert.Null(Assert.Single(manifest.Regions));
        Assert.Null(Assert.Single(manifest.Edges));
        Assert.Null(Assert.Single(manifest.Frames!));
        Assert.Null(Assert.Single(manifest.Unplaced!));

        Assert.NotNull(manifest.ToString());
        Assert.Contains("Environment = , Nodes = Dashboard.Runtime.RuntimeNode[], ", manifest.ToString()!, StringComparison.Ordinal);
        Assert.Equal(manifest.GetHashCode(), manifest.GetHashCode());
        Assert.Equal(manifest, manifest with { });
        Assert.True(manifest == (manifest with { }));

        // A second list of the nothing and of the box is a second manifest, and a list with nothing in it is a third.
        Assert.NotEqual(manifest, new RuntimeManifest(null!, [.. boxes], regions, edges, frames, unplaced));
        Assert.NotEqual(manifest, new RuntimeManifest(null!, NoNodes, regions, edges, frames, unplaced));
    }
}
