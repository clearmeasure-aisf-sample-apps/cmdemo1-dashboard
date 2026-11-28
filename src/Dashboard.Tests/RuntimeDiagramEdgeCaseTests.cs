using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="RuntimeDiagram" />: the record
/// <see cref="RuntimeLoader.LoadDiagramAsync" /> hands over with the one environment's two files - the environment it
/// is named for, the <see cref="RuntimeManifest" /> and the SVG it read, and the reasons there is none of either - and
/// the one verdict the record draws of its own, <see cref="RuntimeDiagram.IsValid" />.
/// <para>
/// What the other files already pin is left alone. <see cref="RuntimeLoaderTests" /> pins the outcomes of the read
/// itself over HTTP: that a diagram is its manifest and its SVG, that a diagram whose two files are 404 is no diagram
/// and says both answers, that a file that is no SVG is reported as one, and which address the loader asks for.
/// <see cref="RuntimeFilesPairTests" /> pins what the runtime diagram and the database view share since they read
/// their pair through one helper: both parts and no errors, or neither part and every reason, in the same words and
/// order for both views, and that a pair asks for exactly its two files.
/// <see cref="RuntimeManifestEdgeCaseTests" />, <see cref="RuntimeManifestBoundaryEdgeCaseTests" /> and
/// <see cref="RuntimeManifestSurfaceEdgeCaseTests" /> pin the manifest part, <see cref="RuntimeIndexEntryEdgeCaseTests" />
/// the entry the loader names the diagram from, and <see cref="RuntimeIndexResultEdgeCaseTests" /> the sibling record
/// of the index above.
/// </para>
/// <para>
/// What follows is the record on its own, at the ends those tests leave open: the four parts by the names of their own
/// properties, each held as the very instance it was given and none of them refused - the manifest that is not there,
/// the picture that is not, the reasons that are not a list at all, the word that is nothing but spaces - the
/// environment kept as the caller's word and never as the word the manifest recorded, and the picture kept as the word
/// the record was handed whether or not it draws anything (saying what counts as a picture is the loader's job, and is
/// pinned there). Then <see cref="RuntimeDiagram.IsValid" /> at its four corners and at the boundaries around them:
/// true of both parts standing and of nothing else, empty picture standing where a picture of no drawing stands, the
/// manifest of an environment that draws nothing standing, the reasons and the name moving it not at all - and the
/// invariant the page leans on, that a diagram the record calls valid always has the two parts the page reads out of it
/// and that every shape the loader builds as invalid has a reason to say. Then the members every record is asked:
/// compare, hash, fall apart, copy and say the four parts and the verdict drawn of them; and the record's whole public
/// surface, so that the day a
/// fifth part or a second verdict is added the lists below say so.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: no loader is built, no file of <c>runtime/</c> is
/// fetched, and every value below is one the test brought - the manifest read from the shipped sample included, which
/// is a read of words the deployment wrote and nothing else.
/// </para>
/// </summary>
public class RuntimeDiagramEdgeCaseTests
{
    /// <summary>The environment the caller names the diagram for - the word of <c>runtime/index.json</c>.</summary>
    private const string Environment = "uat";

    /// <summary>The environment the manifest itself recorded: another word from the caller's, so that a part landing
    /// on the manifest's own name is seen instead of quietly matching the diagram's.</summary>
    private const string Recorded = "prod";

    /// <summary>A picture, as short as one may be.</summary>
    private const string Picture = "<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>";

    /// <summary>The one reason the loader says when a file of the pair could not be read.</summary>
    private const string Reason = "The server answered HTTP 404 for runtime/uat.svg.";

    /// <summary>The manifest of the diagram, named for an environment that is not the diagram's own.</summary>
    private static readonly RuntimeManifest Manifest = new(Recorded, [], [], []);

    /// <summary>No reason at all, held once: every diagram below that stands with no reason stands on this array.</summary>
    private static readonly string[] NoReasons = [];

    /// <summary>One reason, held once, for the same reason.</summary>
    private static readonly string[] OneReason = [Reason];

    /// <summary>Three reasons, held once: what a read of a pair has to say when both files are bad and the manifest
    /// itself had two things to say about its JSON.</summary>
    private static readonly string[] ThreeReasons = [Reason, Reason, Reason];

    /// <summary>The line a manifest says of itself, quoted whole into the lines below.</summary>
    private const string ManifestWords =
        "RuntimeManifest { Environment = prod, Nodes = Dashboard.Runtime.RuntimeNode[], "
        + "Regions = Dashboard.Runtime.RuntimeRegion[], Edges = Dashboard.Runtime.RuntimeEdge[], Frames = , Unplaced =  }";

    /// <summary>The line a diagram with both parts and no reason says of itself: its four parts named in the order
    /// they are declared - the manifest opening into its own line, the reasons named as the array they are - and the
    /// verdict named last, saying that both parts stand.</summary>
    private const string FullWords =
        "RuntimeDiagram { Environment = uat, Manifest = " + ManifestWords + ", Svg = " + Picture
        + ", Errors = System.String[], IsValid = True }";

    /// <summary>The line a diagram of neither part says of itself: all four parts named, the two that are not there
    /// saying nothing, the empty reasons named as the array they are, and the verdict saying that they do not stand.</summary>
    private const string BlankWords =
        "RuntimeDiagram { Environment = , Manifest = , Svg = , Errors = System.String[], IsValid = False }";

    /// <summary>A diagram with every part named, the name and the manifest's name apart, so that a part landing on
    /// another property is seen instead of quietly matching a second part of the record.</summary>
    private static RuntimeDiagram Full => new(Environment, Manifest, Picture, NoReasons);

    /// <summary>A diagram whose manifest is there and whose picture is not - the shape the loader never builds, since
    /// it keeps neither part when one of them is missing, and the first of the two corners of
    /// <see cref="RuntimeDiagram.IsValid" /> the read leaves untested.</summary>
    private static RuntimeDiagram NoPicture => new(Environment, Manifest, null, OneReason);

    /// <summary>A diagram whose picture is there and whose manifest is not - the other corner.</summary>
    private static RuntimeDiagram NoManifest => new(Environment, null, Picture, OneReason);

    /// <summary>A diagram of neither part, with the reasons that say why.</summary>
    private static RuntimeDiagram NeitherPart => new(Environment, null, null, OneReason);

    /// <summary>The diagram of nothing at all: no name, neither part, and not even a list of reasons.</summary>
    private static RuntimeDiagram Nothing => new(null!, null, null, null!);

    /// <summary>The diagram of empty words: a name and a picture said as nothing, which the record keeps as words and
    /// not as the absence of one.</summary>
    private static RuntimeDiagram EmptyWords => new(string.Empty, null, string.Empty, []);

    // ------------------------------------------------- the four parts, as the names they are

    /// <summary>The one constructor a caller may use hands each part to the property of its own name, in the order the
    /// record declares them: the environment it is named for, the manifest, the picture and the reasons. Each part is
    /// read back as the very instance it went in as, and nothing of one is worked out of another.</summary>
    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        var manifest = new RuntimeManifest(Recorded, [], [], []);
        var reasons = new List<string> { Reason };

        var diagram = new RuntimeDiagram(Environment, manifest, Picture, reasons);

        Assert.Equal(Environment, diagram.Environment);
        Assert.Same(manifest, diagram.Manifest);
        Assert.Equal(Picture, diagram.Svg);
        Assert.Same(reasons, diagram.Errors);

        // The four positions are the four properties and no other: an environment named for one environment and
        // drawn from another's manifest keeps both, and neither is read out of the other.
        var crossed = new RuntimeDiagram("tdd", manifest, "other.svg", []);

        Assert.Equal("tdd", crossed.Environment);
        Assert.Same(manifest, crossed.Manifest);
        Assert.Equal("other.svg", crossed.Svg);
        Assert.Empty(crossed.Errors);
    }

    /// <summary>The record refuses nothing it is given: the manifest that is not there, the picture that is not, the
    /// reasons that are not a list at all, the empty list of reasons, the name that is nothing but spaces and the
    /// picture that is nothing at all - every part is held as it stands, and the reference it was handed is the
    /// reference it keeps.</summary>
    [Fact]
    public void TheRecordRefusesNothingItIsGiven()
    {
        var nothing = Nothing;

        Assert.Null(nothing.Environment);
        Assert.Null(nothing.Manifest);
        Assert.Null(nothing.Svg);
        Assert.Null(nothing.Errors);

        var empty = EmptyWords;

        Assert.Equal(string.Empty, empty.Environment);
        Assert.Equal(string.Empty, empty.Svg);
        Assert.Empty(empty.Errors);

        var words = new List<string> { "  ", string.Empty };
        var padded = new RuntimeDiagram("  ", null, " <svg></svg> ", words);

        Assert.Same(words, padded.Errors);
        Assert.Equal("  ", padded.Environment);
        Assert.Equal(" <svg></svg> ", padded.Svg);
    }

    /// <summary>The environment a diagram is named for is the caller's word and nothing the manifest recorded: the
    /// loader hands over the name of the entry it read (<c>RuntimeLoader.LoadDiagramAsync</c>) and the page weighs
    /// that name against the entry it is showing (<c>RuntimeView.razor</c>), so a diagram drawn from another
    /// environment's manifest keeps the name it was given, and the manifest keeps its own.</summary>
    [Fact]
    public void TheEnvironmentIsTheCallersWordAndNotTheWordTheManifestRecorded()
    {
        var diagram = Full;

        Assert.Equal(Environment, diagram.Environment);
        Assert.Equal(Recorded, diagram.Manifest!.Environment);

        var swapped = new RuntimeDiagram(Recorded, Manifest, Picture, NoReasons);

        Assert.Equal(Recorded, swapped.Environment);
        Assert.Equal(Recorded, swapped.Manifest!.Environment);
        Assert.NotEqual(diagram, swapped);

        // The word is held as it was handed, spaces and markup and all, and the record escapes nothing of it.
        var markup = new RuntimeDiagram(" <b>uat</b> ", null, null, NoReasons);

        Assert.Equal(" <b>uat</b> ", markup.Environment);
        Assert.Equal(" ", new RuntimeDiagram(" ", null, null, NoReasons).Environment);

        // A word is a word even when it is written the other way round: the record weighs the environment as an
        // ordinary word and knows nothing of the two file names a diagram is conventionally drawn from.
        var named = new RuntimeDiagram("uat.json", null, null, NoReasons);

        Assert.Equal("uat.json", named.Environment);
        Assert.NotEqual(Full, named);
    }

    /// <summary>The picture is the word the record was handed, whether or not it draws anything: the record is told
    /// what a picture is by nobody, so an empty word, a word of spaces, a word of another markup and a word that only
    /// mentions an SVG are all kept as the picture of the diagram. Which of them the loader calls a picture is the
    /// loader's decision, and is pinned over there.</summary>
    [Fact]
    public void ThePictureIsTheWordTheRecordWasHandedAndNothingMore()
    {
        foreach (var svg in new string?[] { string.Empty, " ", "not a picture", "<svg", "</svg>", "<svg></svg>", "svg" })
        {
            var diagram = new RuntimeDiagram(Environment, Manifest, svg, NoReasons);

            Assert.Equal(svg, diagram.Svg);
        }

        // The word that is not a word at all is held as the absence of one, and not as the empty word: the two are two
        // diagrams, two hashes, and - the tests below say it - two verdicts.
        var absent = new RuntimeDiagram(Environment, Manifest, null, NoReasons);
        var blank = new RuntimeDiagram(Environment, Manifest, string.Empty, NoReasons);

        Assert.Null(absent.Svg);
        Assert.Equal(string.Empty, blank.Svg);
        Assert.NotEqual(absent, blank);
        Assert.NotEqual(absent.GetHashCode(), blank.GetHashCode());
    }

    // ------------------------------------------------------------------------------ IsValid

    /// <summary>The two corners and the two edges of <see cref="RuntimeDiagram.IsValid" />: the verdict holds of the
    /// manifest and the picture standing together and of nothing else - neither of the two alone, and neither of them
    /// at all. The read leaves three of these four shapes untested, since a pair that is missing one of its two files
    /// keeps neither of them; the record is what says what the shapes mean.</summary>
    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, false)]
    public void IsValidHoldsOfBothPartsStandingAndOfNothingElse(bool hasManifest, bool hasSvg, bool valid)
    {
        var diagram = new RuntimeDiagram(Environment, hasManifest ? Manifest : null, hasSvg ? Picture : null, NoReasons);

        Assert.Equal(valid, diagram.IsValid);

        // The two parts stand or do not stand as the test handed them, which is all the verdict is read of.
        Assert.Equal(hasManifest, diagram.Manifest is not null);
        Assert.Equal(hasSvg, diagram.Svg is not null);
    }

    /// <summary>The verdict reads its two parts and nothing else: the name moving it not at all - the name that is
    /// nothing, the name that is spaces, the name that is the manifest's own - and the reasons moving it not at all
    /// either, whether none stand, one stands or the list of them is no list at all. A diagram of a name the page will
    /// never match and both parts standing is a diagram to draw; a diagram of the right name and one part missing is
    /// not, however much it has to say about it.</summary>
    [Fact]
    public void TheVerdictReadsItsTwoPartsAndNeitherTheNameNorTheReasons()
    {
        Assert.True(new RuntimeDiagram(null!, Manifest, Picture, null!).IsValid);
        Assert.True(new RuntimeDiagram(string.Empty, Manifest, Picture, []).IsValid);
        Assert.True(new RuntimeDiagram(Recorded, Manifest, Picture, OneReason).IsValid);
        Assert.True(new RuntimeDiagram("no such environment", Manifest, Picture, ThreeReasons).IsValid);

        Assert.False(new RuntimeDiagram(Environment, null, null, null!).IsValid);
        Assert.False(Nothing.IsValid);
        Assert.False(EmptyWords.IsValid);
        Assert.False(NeitherPart.IsValid);
    }

    /// <summary>The boundary the verdict draws between a word and no word: a picture of no drawing is still a picture,
    /// and a manifest of an environment that draws nothing is still a manifest. Both standing is a valid diagram, so
    /// the page draws an empty canvas out of them and does not open the alert of a read that failed - the absence of
    /// a drawing and the absence of a file are two different answers, and only the second of them is a failure.</summary>
    [Fact]
    public void APartOfNothingToDrawIsStillAPartAndTheVerdictHolds()
    {
        var nothingDrawn = new RuntimeManifest(Environment, [], [], [], [], []);
        var drawn = new RuntimeDiagram(Environment, nothingDrawn, string.Empty, NoReasons);

        Assert.True(drawn.IsValid);
        Assert.Empty(nothingDrawn.Nodes);
        Assert.Empty(nothingDrawn.Regions);
        Assert.Empty(nothingDrawn.Edges);
        Assert.Equal(string.Empty, drawn.Svg);

        // The manifest of nothing at all is still a manifest: the verdict asks whether the part stands, not what it
        // holds - a caller that wants to know whether anything was drawn asks the manifest, and the page does.
        var noPartsAtAll = new RuntimeManifest(null!, null!, null!, null!);

        Assert.True(new RuntimeDiagram(Environment, noPartsAtAll, " ", NoReasons).IsValid);
        Assert.Null(noPartsAtAll.Nodes);
        Assert.Null(noPartsAtAll.Environment);
    }

    /// <summary>The invariant the page leans on, in both directions. <c>RuntimeView.razor</c> reads
    /// <c>_diagram is { IsValid: true, Manifest: { } }</c> and draws out of what that gives it, and shows
    /// <c>_diagram is { IsValid: false }</c> with the reasons under it: so a diagram the verdict calls valid always has
    /// the manifest and the picture to read out of it - the verdict never lets the page into a diagram of no parts -
    /// and every shape the loader builds of a failed pair has at least one reason to list. The record itself refuses
    /// neither: an invalid diagram of no reasons is one it will hold, and nothing but the loader's own pair-building
    /// (<see cref="RuntimeFilesRead{TManifest}.Of" />) keeps that shape away from the page.</summary>
    [Fact]
    public void AValidDiagramAlwaysHasTheTwoPartsThePageReadsOutOfIt()
    {
        foreach (var diagram in new[] { Full, NoPicture, NoManifest, NeitherPart, Nothing, EmptyWords })
        {
            if (diagram.IsValid)
            {
                Assert.NotNull(diagram.Manifest);
                Assert.NotNull(diagram.Svg);

                // The pattern the page writes, written again here: a valid diagram is always one it can draw.
                Assert.True(diagram is { IsValid: true, Manifest: not null, Svg: not null });
            }
        }

        Assert.True(Full.IsValid);
        Assert.False(NoPicture.IsValid);
        Assert.False(NoManifest.IsValid);
        Assert.False(NeitherPart.IsValid);

        // The shape the loader never builds, and the record allows: no parts and nothing to say about it, which would
        // reach the page as an alert with nothing under it.
        var silent = new RuntimeDiagram(Environment, null, null, NoReasons);

        Assert.False(silent.IsValid);
        Assert.Empty(silent.Errors);
        Assert.Null(silent.Manifest);
        Assert.Null(silent.Svg);
    }

    /// <summary>The verdict is drawn from its two parts at every ask and is nothing the record keeps: two diagrams of
    /// the same parts answer it the same way, and a diagram whose parts are copied over into a new diagram carries the
    /// verdict with them - it is a reading of the record, and the record's one reading.</summary>
    [Fact]
    public void TheVerdictIsReadOfThePartsAndNotKeptBesideThem()
    {
        var diagram = Full;

        Assert.Equal(diagram.IsValid, Full.IsValid);
        Assert.Equal(diagram.IsValid, new RuntimeDiagram(diagram.Environment, diagram.Manifest, diagram.Svg, diagram.Errors).IsValid);
        Assert.Equal(diagram.IsValid, (diagram with { Environment = "prod" }).IsValid);

        // Every part turned moves the verdict with it, in both directions.
        Assert.False((diagram with { Manifest = null }).IsValid);
        Assert.False((diagram with { Svg = null }).IsValid);
        Assert.True((NeitherPart with { Manifest = Manifest, Svg = Picture }).IsValid);
    }

    // ------------------------------------------------------------------- the members every record is asked

    /// <summary>Two diagrams of the same four parts are one diagram: equal by the record's own <c>Equals</c>, by the
    /// <see cref="object" /> one, by the <c>==</c> and the <c>!=</c> a caller writes, by the comparer the collections
    /// use, one hash, one in a set. The parts are weighed as the record weighs them, which for the two lists and the
    /// manifest is the reference and the parts standing in them - pinned on the manifest itself elsewhere.</summary>
    [Fact]
    public void TwoDiagramsOfTheSameFourPartsAreOneDiagram()
    {
        var reasons = new List<string> { Reason };
        var diagram = new RuntimeDiagram(Environment, Manifest, Picture, reasons);
        var again = new RuntimeDiagram(Environment, Manifest, Picture, reasons);

        Assert.Equal(diagram, again);
        Assert.True(diagram.Equals(again));
        Assert.True(diagram.Equals((object)again));
        Assert.True(diagram == again);
        Assert.False(diagram != again);
        Assert.Equal(diagram.GetHashCode(), again.GetHashCode());
        Assert.Single(new HashSet<RuntimeDiagram> { diagram, again });

        var comparer = EqualityComparer<RuntimeDiagram>.Default;

        Assert.True(comparer.Equals(diagram, again));
        Assert.False(comparer.Equals(diagram, null));
        Assert.Equal(diagram.GetHashCode(), comparer.GetHashCode(again));
    }

    /// <summary>A diagram differs in any one of its four parts: in the name, in the manifest, in the picture, in the
    /// list of reasons - the list weighed as the very list it holds, and not by the words in it - and in the
    /// <see langword="null" /> that is not the empty word. The verdict follows the parts and is never weighed on its
    /// own: two diagrams that differ only in what they hold differ, and two that hold the same answer alike.</summary>
    [Fact]
    public void ADiagramDiffersInAnyOneOfItsFourParts()
    {
        var diagram = Full;

        Assert.NotEqual(diagram, diagram with { Environment = "prod" });
        Assert.NotEqual(diagram, diagram with { Environment = "UAT" });
        Assert.NotEqual(diagram, diagram with { Environment = string.Empty });
        Assert.NotEqual(diagram, diagram with { Environment = null! });
        Assert.NotEqual(diagram, diagram with { Manifest = null });
        Assert.NotEqual(diagram, diagram with { Manifest = new RuntimeManifest("tdd", [], [], []) });
        Assert.NotEqual(diagram, diagram with { Svg = null });
        Assert.NotEqual(diagram, diagram with { Svg = string.Empty });
        Assert.NotEqual(diagram, diagram with { Errors = OneReason });
        Assert.NotEqual(diagram, diagram with { Errors = new List<string>() });
        Assert.NotEqual(diagram, diagram with { Errors = null! });

        Assert.True(diagram != diagram with { Svg = null });
        Assert.False(diagram == diagram with { Svg = null });

        // Two reasons of the same words in two lists are two diagrams: the list is weighed as the list it is.
        Assert.NotEqual(
            new RuntimeDiagram(Environment, null, null, new List<string> { Reason }),
            new RuntimeDiagram(Environment, null, null, new List<string> { Reason }));
        Assert.Equal(
            new RuntimeDiagram(Environment, null, null, OneReason),
            new RuntimeDiagram(Environment, null, null, OneReason));
    }

    /// <summary>A part of another kind is no diagram's equal: the <c>Equals</c> asked of an object answers false to the
    /// word of an environment, to the manifest the diagram holds, to the database view's diagram standing with the same
    /// three parts, to a number and to a thing of no kind at all - the diagram counts only the four parts of its own
    /// kind, and no other record of the runtime view is one of them.</summary>
    [Fact]
    public void ADiagramIsNeverEqualToAnythingOfAnotherKind()
    {
        var diagram = Full;

        Assert.False(diagram.Equals("uat"));
        Assert.False(diagram.Equals(Manifest));
        Assert.False(diagram.Equals(new DatabaseDiagram(null, null, NoReasons)));
        Assert.False(diagram.Equals(0));
        Assert.False(diagram.Equals(new object()));
    }

    /// <summary>Nothing is no diagram: the <c>==</c> and the <c>!=</c> a caller writes both answer that a diagram is
    /// something and nothing is not, from either side of the operator, and the <c>Equals</c> asked of a diagram answers
    /// false to nothing - as the object it is and as the diagram it is not. A diagram that holds nothing at all is
    /// still a diagram, and equal to none of the nothing around it.</summary>
    [Fact]
    public void ADiagramIsNeverTheSameAsNothingAndNeverEqualToIt()
    {
        var diagram = Full;

        Assert.True(diagram != null);
        Assert.False(diagram == null);
        Assert.False(null == diagram);
        Assert.True(null != diagram);
        Assert.False(diagram.Equals((object?)null));
        Assert.False(diagram.Equals((RuntimeDiagram?)null));
        Assert.False(((object)diagram).Equals(null));
    }

    /// <summary>The hash a diagram says is the hash it says again: asked twice of the diagram whose parts are all
    /// there, twice of the diagram of nothing at all and twice of the diagram whose reasons are no list, so that the
    /// record neither answers the nothing with a word of its own nor changes its mind between the two asks of one
    /// diagram. Two diagrams of the same parts say one hash, and the hash is asked of every part without
    /// dereferencing any of them.</summary>
    [Fact]
    public void TheHashItSaysIsTheHashItSaysAgain()
    {
        var served = Full;
        var nothing = Nothing;

        Assert.Equal(served.GetHashCode(), served.GetHashCode());
        Assert.Equal(nothing.GetHashCode(), nothing.GetHashCode());
        Assert.Equal(served.GetHashCode(), Full.GetHashCode());
        Assert.Equal(nothing.GetHashCode(), new RuntimeDiagram(null!, null, null, null!).GetHashCode());

        // The empty word and the nothing are two answers and hash apart, and a diagram of neither part still hashes.
        Assert.NotEqual(EmptyWords.GetHashCode(), nothing.GetHashCode());
        Assert.Equal(NeitherPart.GetHashCode(), new RuntimeDiagram(Environment, null, null, OneReason).GetHashCode());
    }

    // -------------------------------------------------------------------------------------------- copies

    /// <summary>The <c>with</c> a caller writes copies the diagram and turns only the part it names: the copy keeps the
    /// references of the parts it does not turn, the verdict is read of the parts the copy stands on, and the part
    /// turned is the one the caller names - the empty picture that is a picture and the manifest that is not. The
    /// verdict itself is nothing a <c>with</c> may name, which the surface test at the end of this file reads off the
    /// type: a caller cannot make a diagram valid by saying so.</summary>
    [Fact]
    public void AWithCopiesTheDiagramAndTurnsOnlyThePartItNames()
    {
        var reasons = new List<string> { Reason };
        var diagram = new RuntimeDiagram(Environment, Manifest, null, reasons);

        var copy = diagram with { };

        Assert.Equal(copy, diagram);
        Assert.Same(Manifest, copy.Manifest);
        Assert.Same(reasons, copy.Errors);
        Assert.Equal(diagram.IsValid, copy.IsValid);

        var drawn = diagram with { Svg = Picture };

        Assert.True(drawn.IsValid);
        Assert.Same(Manifest, drawn.Manifest);
        Assert.Same(reasons, drawn.Errors);
        Assert.NotEqual(drawn, diagram);

        var unnamed = diagram with { Environment = string.Empty };

        Assert.Equal(string.Empty, unnamed.Environment);
        Assert.Same(Manifest, unnamed.Manifest);
        Assert.Equal(diagram.IsValid, unnamed.IsValid);

        var silent = diagram with { Errors = [] };

        Assert.Empty(silent.Errors);
        Assert.NotSame(silent.Errors, reasons);
        Assert.False(silent.IsValid);
    }

    /// <summary>The four parts fall out of the diagram in the order they are declared - the name, the manifest, the
    /// picture and the reasons, each as the very reference the diagram holds - and the parts read out are the parts of
    /// the one diagram they came out of: the diagram of nothing at all falls apart into the <see langword="null" /> it
    /// was given, and the parts of a valid diagram rebuilt make the same diagram again.</summary>
    [Fact]
    public void TheFourPartsFallOutOfTheDiagramInTheirOrder()
    {
        var reasons = new List<string> { Reason };
        var diagram = new RuntimeDiagram(Environment, Manifest, Picture, reasons);

        var (name, manifest, svg, errors) = diagram;

        Assert.Equal(Environment, name);
        Assert.Same(Manifest, manifest);
        Assert.Equal(Picture, svg);
        Assert.Same(reasons, errors);

        var (nothingName, nothingManifest, nothingSvg, nothingErrors) = Nothing;

        Assert.Null(nothingName);
        Assert.Null(nothingManifest);
        Assert.Null(nothingSvg);
        Assert.Null(nothingErrors);

        Assert.Equal(diagram, new RuntimeDiagram(name, manifest, svg, errors));
        Assert.True(new RuntimeDiagram(name, manifest, svg, errors).IsValid);
    }

    // ------------------------------------------------------------------------------------------------ words

    /// <summary>The words a diagram says about itself - its name, then its parts in the order they are declared, each
    /// named and saying its part's own words: the environment as it was written, the manifest opening into its own
    /// line or nothing, the picture as it stands, and the reasons as the array they are or nothing - are the record's
    /// and not the page's. The line names five things where the constructor takes four: the verdict is said last,
    /// because the record's own walk names every public property it has and the verdict is one, drawn of the two parts
    /// before it. A log of a diagram therefore says what the page would have made of it, and says the verdict as a
    /// <see cref="bool" /> says it - the same two words in every culture.</summary>
    [Fact]
    public void TheDiagramSaysItsFourPartsAndItsVerdictInTheirOrder()
    {
        Assert.Equal(FullWords, Full.ToString());

        Assert.Equal(
            $"RuntimeDiagram {{ Environment = uat, Manifest = , Svg = {Picture}, Errors = System.String[], IsValid = False }}",
            NoManifest.ToString());

        // The nothing at all says its five names too, and asking them dereferences nothing: the reasons that are no
        // list at all leave the name of the part and no word after it.
        Assert.Equal(
            "RuntimeDiagram { Environment = , Manifest = , Svg = , Errors = , IsValid = False }",
            Nothing.ToString());

        // The empty word and the nothing are two lines: an environment and a picture said as nothing name themselves
        // as the words they are, and the reasons that are a list say the array even when it holds nothing.
        Assert.Equal(BlankWords, EmptyWords.ToString());

        // The verdict is the last word of the line, and it is said of the two parts and of nothing else - the reasons
        // saying nothing about it, and a name the page will never match saying the verdict all the same.
        Assert.EndsWith(", IsValid = True }", Full.ToString(), StringComparison.Ordinal);
        Assert.EndsWith(", IsValid = False }", NeitherPart.ToString(), StringComparison.Ordinal);
        Assert.EndsWith(
            ", Errors = System.String[], IsValid = True }",
            new RuntimeDiagram("no such environment", Manifest, Picture, ThreeReasons).ToString(),
            StringComparison.Ordinal);

        // A picture that draws nothing says nothing, and the manifest of no boxes says its six holds just the same.
        Assert.Contains(
            "Svg = , ",
            new RuntimeDiagram(Environment, null, null, NoReasons).ToString(),
            StringComparison.Ordinal);
        Assert.Contains("Frames = , Unplaced =  }", Full.ToString(), StringComparison.Ordinal);
    }

    /// <summary>A diagram's words are nothing but words, so no reader's culture turns them: a viewer of German is told
    /// the same diagram as the invariant reader - the same name, the same manifest, the same picture, the same
    /// reasons - which is what a log of one of these diagrams may count on whatever the reader of the machine
    /// happens to be.</summary>
    [Fact]
    public void TheDiagramSaysTheSameThingWhateverTheReaderSpeaks()
    {
        ViewerCulture.German(() =>
        {
            Assert.Equal(FullWords, Full.ToString());
            Assert.Equal(BlankWords, EmptyWords.ToString());
            Assert.EndsWith(", IsValid = True }", Full.ToString(), StringComparison.Ordinal);
            Assert.Equal(Full.GetHashCode(), new RuntimeDiagram(Environment, Manifest, Picture, NoReasons).GetHashCode());
        });
    }

    /// <summary>The name is weighed ordinally and not as a reader spells it: <c>uat</c> and <c>UAT</c> are two
    /// environments whatever culture the thread runs under, and the dotted and the dotless <c>i</c>, which one
    /// reader's capital of a small letter turns into the other's, are two names here - which is what the page's own
    /// ordinal match of the environment against the entry it shows leans on.</summary>
    [Fact]
    public void TheNameIsWeighedOrdinallyWhateverTheReaderSpeaks()
    {
        var upper = new RuntimeDiagram("UAT", null, null, NoReasons);

        ViewerCulture.German(() =>
        {
            Assert.NotEqual(Full, upper);
            Assert.True(Full != upper);
            Assert.False(Full.Equals((object)upper));
            Assert.Equal(Full, new RuntimeDiagram(Environment, Manifest, Picture, NoReasons));
        });

        Assert.NotEqual(Full, Full with { Environment = "UAT" });
        Assert.NotEqual(Full, Full with { Environment = "uat " });

        var dotted = new RuntimeDiagram("ist", null, null, NoReasons);
        var dotless = new RuntimeDiagram("ıst", null, null, NoReasons);

        Assert.NotEqual(dotted, dotless);
        Assert.NotEqual(dotted, new RuntimeDiagram("İst", null, null, NoReasons));
    }

    // ---------------------------------------------------------------------------- the record's own shape

    /// <summary>The parts a caller may set are init-only, and the verdict is read once and written never: the four
    /// parts stand on the record with an <c>init</c> and no <c>set</c>, so a diagram cannot be turned into another
    /// after it was handed over, and <see cref="RuntimeDiagram.IsValid" /> has no write of any kind - it is drawn of
    /// the two parts every time it is asked, and there is nothing to write.</summary>
    [Fact]
    public void TheFourPartsAreInitOnlyAndTheVerdictIsReadOnceAndNeverWritten()
    {
        RecordSurface.AssertPartsAreInitOnly(typeof(RuntimeDiagram), "Environment", "Manifest", "Svg", "Errors");

        var verdict = RecordSurface.PropertiesOf<RuntimeDiagram>()["IsValid"];

        Assert.Equal(typeof(bool), verdict.PropertyType);
        Assert.True(verdict.CanRead);
        Assert.Null(verdict.SetMethod);
        Assert.DoesNotContain("IsValid", RecordSurface.ParametersOf<RuntimeDiagram>().Select(parameter => parameter.Name!), StringComparer.Ordinal);
    }

    /// <summary>The diagram's whole public surface as it stands: its four parts, named in the order the constructor
    /// names them and of the type the constructor says, all four a caller must name and none of them defaulted, the
    /// one verdict the record draws, and the members every record is asked - falling apart, comparing, hashing and
    /// saying itself - and nothing else to call. There is no third part of a picture, no moment, no address and no
    /// error type to hand over, and the walk is the shared one of <see cref="RecordSurface" /> rather than a copy of
    /// the private ones the older files still hand-roll. A fifth part, or a second verdict, has to be written down
    /// here as well, which is the point: a diagram cannot carry a part nobody decided what it means.</summary>
    [Fact]
    public void TheDiagramHasFourPartsOneVerdictAndTheMembersEveryRecordIsAsked()
    {
        Assert.True(RecordShape.IsPlainSealedRecord<RuntimeDiagram>());
        Assert.Same(typeof(object), typeof(RuntimeDiagram).BaseType);

        Assert.Equal(
            ["Environment", "Manifest", "Svg", "Errors", "IsValid"],
            [.. RecordSurface.PropertyNamesOf<RuntimeDiagram>()]);

        var surface = RecordSurface.Of<RuntimeDiagram>();

        Assert.Equal(
            [
                "Deconstruct", "Environment", "Equals", "Errors", "GetHashCode", "IsValid", "Lacks", "Manifest", "Svg",
                "ToString", "op_Equality", "op_Inequality",
            ],
            surface);
        Assert.DoesNotContain("EqualityContract", surface, StringComparer.Ordinal);

        var properties = RecordSurface.PropertiesOf<RuntimeDiagram>();

        Assert.Equal(typeof(string), properties["Environment"].PropertyType);
        Assert.Equal(typeof(RuntimeManifest), properties["Manifest"].PropertyType);
        Assert.Equal(typeof(string), properties["Svg"].PropertyType);
        Assert.Equal(typeof(IReadOnlyList<string>), properties["Errors"].PropertyType);
        Assert.Equal(typeof(bool), properties["IsValid"].PropertyType);

        // The one constructor a caller may use: one parameter per part, in the order the record names them, of the
        // type of the part, none of them defaulted, and the verdict named nowhere - it is no part of a diagram.
        var parameters = RecordSurface.ParametersOf<RuntimeDiagram>();

        Assert.Equal(["Environment", "Manifest", "Svg", "Errors"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal(
            [typeof(string), typeof(RuntimeManifest), typeof(string), typeof(IReadOnlyList<string>)],
            [.. parameters.Select(parameter => parameter.ParameterType)]);
        Assert.All(parameters, parameter => Assert.False(parameter.HasDefaultValue));

        // The nullable annotations stand as the record declares them: the manifest and the picture may be nothing -
        // that is what the verdict is for - and the name and the list of reasons may not, which is what the record
        // itself does not enforce (a caller may hand it the nothing of either, pinned above).
        Assert.Equal([false, true, true, false], RecordSurface.NullableParts<RuntimeDiagram>());

        var parts = new[] { properties["Environment"], properties["Manifest"], properties["Svg"], properties["Errors"] };

        Assert.Equal(
            [false, true, true, false],
            [.. parts.Select(property => new NullabilityInfoContext().Create(property).ReadState == NullabilityState.Nullable)]);

        // The copy constructor a <c>with</c> turns through is not a member a caller may call, and the compiler-named
        // clone is the record's clone and nothing else hidden away.
        var copyConstructor = typeof(RuntimeDiagram).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).Single();

        Assert.Equal(typeof(RuntimeDiagram), copyConstructor.GetParameters().Single().ParameterType);
        Assert.NotNull(typeof(RuntimeDiagram).GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance));

        Assert.Single(typeof(RuntimeDiagram).GetInterfaces(), interfaceType => interfaceType == typeof(IEquatable<RuntimeDiagram>));
        Assert.Equal(
            2,
            typeof(RuntimeDiagram)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Count(method => method.Name == "Equals"));

        // The diagram's one static sentence: the words under the picture naming the boxes the manifest has and the
        // SVG lacks. It is a static method, read of nothing the record keeps, so it is declared only and is no part.
        var lacks = typeof(RuntimeDiagram).GetMethod("Lacks", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.NotNull(lacks);
        Assert.Equal(typeof(string), lacks.ReturnType);
        Assert.Equal(typeof(IReadOnlyList<string>), lacks.GetParameters().Single().ParameterType);

        // The static members a record may carry are pinned here: no static property, and the one static method is
        // Lacks - the words of the mismatch, said once in tested C# and read by the page.
        Assert.Empty(typeof(RuntimeDiagram).GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Equal(
            ["Lacks"],
            typeof(RuntimeDiagram)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(method => !method.IsSpecialName && !method.Name.StartsWith("op_", StringComparison.Ordinal))
                .Select(method => method.Name)
                .Order(StringComparer.Ordinal));

        // The four parts fall out in their order, as the out parameters the record names them, and the verdict is no
        // fifth of them.
        var deconstruct = typeof(RuntimeDiagram).GetMethod("Deconstruct")!;

        Assert.Equal(["Environment", "Manifest", "Svg", "Errors"], [.. deconstruct.GetParameters().Select(parameter => parameter.Name!)]);
        Assert.All(deconstruct.GetParameters(), parameter =>
        {
            Assert.True(parameter.IsOut);
            Assert.Equal(typeof(void), deconstruct.ReturnType);
        });

        // Nothing else to stand on: no static part of a diagram, no event, no public field.
        Assert.Empty(typeof(RuntimeDiagram).GetEvents());
        Assert.Empty(typeof(RuntimeDiagram).GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Empty(typeof(RuntimeDiagram).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }
}
