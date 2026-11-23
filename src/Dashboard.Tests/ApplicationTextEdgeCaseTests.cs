using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ApplicationText" /> - the words of the application
/// view, and what it decides - at the ends the tests of the view as a whole leave open.
/// <para>
/// What is pinned where it belongs is walked past here on purpose: <see cref="RuntimeTextHelperTests" /> pins
/// <see cref="ApplicationText.IsName" /> whole - the letters, the digits, the dash and the underscore, the sixty-four
/// and the sixty-fifth, the nothing and the empty word - along with <see cref="ApplicationText.RefusedLine" />, the
/// two titles of <see cref="ApplicationText.VersionTitle" /> and <see cref="ApplicationText.Package" /> (with its
/// nothing version, its empty version and its refusal of no package at all);
/// <see cref="ApplicationViewTests" /> pins which node's version <see cref="ApplicationText.Version(IEnumerable{ValueTuple{string, string}}, string)" />
/// shows, the three lines of <see cref="ApplicationText.VersionLine" />, the reading of a file and the four branches
/// of <see cref="ApplicationText.KindLine" /> that a file can reach, the two lines of
/// <see cref="ApplicationText.ProblemLine" /> and its silence when the file was read, the asking of
/// <see cref="ApplicationText.File" /> for a plain version and its refusal at the forty-first letter, and the words
/// <see cref="ApplicationText.AltText" /> says of a diagram that lists assemblies, one of them and none; and
/// <see cref="ViewLinksTests" /> pins the boxes of a deployable, the heading row's links for one box and for several,
/// a site link standing in for an address and an address that is no https one, and
/// <see cref="ApplicationText.DiagramLink" /> word for word.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: which side of the forty letters a version is named by - the forty whole,
/// the marks a version is spelled with, and the letters that are no ASCII ones; that the boxes of a diagram keep its
/// order while the boxes of one deployable stand the primary first, and that a deployable's name is matched without
/// heeding its case whatever the reader speaks; that a manifest and a node that are not there are refused by name;
/// that the version a node reported is trimmed as the pin is, that the first of several primaries is the one shown
/// and that a role with whitespace about it is no primary; the same decision taken through the boxes of a diagram and
/// their environment - a box with no address, a node that reported only spaces, a deployable named in another case
/// than the address spells it, and a deployable whose own pin file has been named but not yet read; the file of the
/// forty letters and the address with the slashes it ends with trimmed off; the alternative text of a diagram whose
/// file names no entry, of one with no assemblies and no process, and of thousands of assemblies a reader of German
/// would otherwise count in threes; the empty word of a kind, a process and a name, which is a word and not the
/// nothing the view answers with; a state that is no state; the link a scheme other than https refuses even where it
/// stands in for an address, and the topology's portal beating the box's own; and the whole public surface of the
/// class, so that the day a word is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: <see cref="ApplicationText" /> decides
/// words, addresses and order out of what it is handed, and every value below is one the test brought. The one member
/// of the file that asks the repository anything is <see cref="RuntimeLoader" />, pinned with a stubbed answer in
/// <see cref="ApplicationViewTests" />.
/// </para>
/// </summary>
public class ApplicationTextEdgeCaseTests
{
    private const string Base = "https://raw.githubusercontent.com/org/app/architecture/ui";

    /// <summary>A fixed moment: the version a node reported is asked of a check the test itself made.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 12, 16, 0, 0, TimeSpan.Zero);

    // -------------------------------------------------------------------------------- the version's forty letters

    /// <summary>The word of the given number of <c>7</c>: the length a version is bounded by, asked at either side of
    /// the bound and at the bound itself.</summary>
    private static string At(int letters) => new('7', letters);

    /// <summary>Every word a file may be named by is taken: one letter, digits only, and each of the four marks a
    /// version is spelled with - the dot, the dash, the underscore and the plus - alone, in a row and between the
    /// parts of a version. The address refuses a version that is no plain one; which of them the view asks a file
    /// for is decided here and nowhere else.</summary>
    [Theory]
    [InlineData("1")]
    [InlineData("7")]
    [InlineData("a")]
    [InlineData("A")]
    [InlineData("zZ09")]
    [InlineData("0.1.2")]
    [InlineData("1.0.42-beta.1+build_7")]
    [InlineData("1.0.42+0a1b2c3")]
    [InlineData(".")]
    [InlineData("-")]
    [InlineData("_")]
    [InlineData("+")]
    [InlineData("....")]
    [InlineData("----")]
    [InlineData("____")]
    [InlineData("++++")]
    [InlineData("1--2__3..4++5")]
    public void EveryWordAFileMayBeNamedByIsTaken(string version) => Assert.True(ApplicationText.IsVersion(version), $"refused: {version}");

    /// <summary>The bound itself: thirty-nine letters and forty are two whole versions the page asks a file for - the
    /// forty of them named by <see cref="ApplicationText.File" /> as it stands - and forty-one is the first it refuses,
    /// so that the longest file name the view ever asks is the base, a slash, forty letters and the extension.</summary>
    [Fact]
    public void AVersionOfFortyLettersNamesAFileAndTheFortyFirstNamesNone()
    {
        Assert.True(ApplicationText.IsVersion(At(1)));
        Assert.True(ApplicationText.IsVersion(At(39)));
        Assert.True(ApplicationText.IsVersion(At(40)));
        Assert.False(ApplicationText.IsVersion(At(41)));
        Assert.False(ApplicationText.IsVersion(At(42)));
        Assert.False(ApplicationText.IsVersion(At(4_000)));

        Assert.Equal($"{Base}/{At(40)}.svg", ApplicationText.File(new Uri(Base), At(40), "svg")!.AbsoluteUri);
        Assert.Equal($"{Base}/{At(40)}.json", ApplicationText.File(new Uri(Base), At(40), "json")!.AbsoluteUri);
        Assert.Equal($"{Base}/{At(39)}.svg", ApplicationText.File(new Uri(Base), At(39), "svg")!.AbsoluteUri);
        Assert.Null(ApplicationText.File(new Uri(Base), At(41), "svg"));
    }

    /// <summary>The two guards share one decision but keep their own bound and their own marks: a word of forty-one to
    /// sixty-four letters is a name and no version, the dot and the plus spell a version and no name, and the dash and
    /// the underscore spell both.</summary>
    [Theory]
    [InlineData(41, true, false)]
    [InlineData(64, true, false)]
    [InlineData(65, false, false)]
    public void ANameAndAVersionAreBoundedEachByItsOwnLength(int letters, bool isName, bool isVersion)
    {
        Assert.Equal(isName, ApplicationText.IsName(At(letters)));
        Assert.Equal(isVersion, ApplicationText.IsVersion(At(letters)));
    }

    /// <summary>The marks of the one guard do not leak into the other.</summary>
    [Theory]
    [InlineData("1.0", false, true)]
    [InlineData("1+0", false, true)]
    [InlineData("1-0", true, true)]
    [InlineData("1_0", true, true)]
    public void ANameAndAVersionAreSpelledEachWithItsOwnMarks(string word, bool isName, bool isVersion)
    {
        Assert.Equal(isName, ApplicationText.IsName(word));
        Assert.Equal(isVersion, ApplicationText.IsVersion(word));
    }

    /// <summary>What is refused although it is spelled like a version: the letters outside the ASCII - an accented
    /// letter, a full-width digit, a superscript, the two letters an English <c>i</c> turns into in Turkish - a space
    /// or a tab or a newline at either end, and the comma, the colon, the slash and the percent sign a hostile address
    /// is built out of. Nothing here is trimmed, escaped or shortened to make it fit: a word that is not the forty
    /// letters of the marks a version is spelled with names no file at all.</summary>
    [Theory]
    [InlineData("1.0.42 ")]
    [InlineData(" 1.0.42")]
    [InlineData("\t1.0.42")]
    [InlineData("1.0.42\n")]
    [InlineData("1.0.42\u00a0")]
    [InlineData("café")]
    [InlineData("1.0.42-é")]
    [InlineData("１.0.42")]
    [InlineData("1.0.4²")]
    [InlineData("İ")]
    [InlineData("ı")]
    [InlineData("1.0,42")]
    [InlineData("1.0:42")]
    [InlineData("1.0.42/")]
    [InlineData("1.0.42%")]
    [InlineData("1.0.42;")]
    [InlineData("")]
    [InlineData(null)]
    public void AWordThatIsNoPlainVersionNamesNothing(string? version) => Assert.False(ApplicationText.IsVersion(version));

    // ---------------------------------------------------------------------------- the boxes that lead into the view

    /// <summary>A box of the diagram: a node with an architecture, which is all the view asks of it. A node without
    /// one is written out, since the helper gives every node the address of the diagrams.</summary>
    private static RuntimeNode Box(
        string alias,
        string? deployable,
        string? url = null,
        string? role = null,
        LinkSet? links = null) =>
        new(alias, RuntimeNodeKind.ContainerApp, alias, url is null ? null : new Uri(url), deployable, role, Links: links, Architecture: new Uri(Base));

    /// <summary>A manifest of the given nodes: the view asks nothing else of it than their order.</summary>
    private static RuntimeManifest Diagram(params RuntimeNode[] nodes) => new("uat", [.. nodes], [], []);

    /// <summary>The boxes of a diagram are every node with an architecture and a name the address takes, in the order
    /// the diagram itself puts them in - the one-argument view sorts nothing, since the deployable it belongs to is
    /// not decided until a name is given - and a node without an architecture or without a name is no box whatever
    /// else it carries. That only a web app or a container app is ever given an architecture is decided where the
    /// manifest is read, and is pinned on <c>RuntimeManifestParser</c>.</summary>
    [Fact]
    public void TheBoxesOfADiagramKeepTheOrderTheDiagramPutsThemIn()
    {
        var manifest = Diagram(
            Box("ca_api", "api", "https://ca-api.example.io"),
            new RuntimeNode("browser", RuntimeNodeKind.Person, "Browser", Deployable: "ui"),
            Box("ca_ui_two", "ui", "https://ca-ui-2.example.io"),
            Box("ca_ui_one", "ui", "https://ca-ui.example.io", "primary"),
            new RuntimeNode("no_architecture", RuntimeNodeKind.ContainerApp, "ca-x", Deployable: "ui"),
            Box("no_deployable", null));

        Assert.Equal(["ca_api", "ca_ui_two", "ca_ui_one"], ApplicationText.Boxes(manifest).Select(box => box.Alias));

        // The same nodes seen one deployable at a time; the one box of the other deployable is that box.
        Assert.Equal(["ca_api"], ApplicationText.Boxes(manifest, "api").Select(box => box.Alias));
        Assert.Equal(["ca_api"], ApplicationText.Boxes(manifest, "API").Select(box => box.Alias));

        // The view of one deployable is the one that puts the primary first, whatever the order of the diagram was.
        Assert.Equal(["ca_ui_one", "ca_ui_two"], ApplicationText.Boxes(manifest, "ui").Select(box => box.Alias));
    }

    /// <summary>The boxes of one deployable stand the primary first, and the rest of them exactly as the diagram put
    /// them: the choice of the primary is the only reordering the view does, and it is a stable one - the standby that
    /// came before the primary in the diagram still comes before the standby that came after it. A deployable the
    /// address refuses, and a deployable the diagram holds no box of, have no box at all.</summary>
    [Fact]
    public void TheBoxesOfOneDeployableStandThePrimaryFirstAndTheRestAsTheyWere()
    {
        var manifest = Diagram(
            Box("ca_one", "ui", "https://ca-one.example.io", "standby"),
            Box("ca_two", "ui", "https://ca-two.example.io", "primary"),
            Box("ca_three", "ui", "https://ca-three.example.io", "standby"),
            Box("ca_four", "ui", "https://ca-four.example.io"),
            Box("ca_five", "ui", "https://ca-five.example.io", "Primary"));

        Assert.Equal(["ca_two", "ca_five", "ca_one", "ca_three", "ca_four"], ApplicationText.Boxes(manifest, "ui").Select(box => box.Alias));

        // Both of these roles are the primary's, so both lead, in the order the diagram held them.
        var second = Diagram(
            Box("ca_two", "ui", "https://ca-two.example.io", "primary"),
            Box("ca_five", "ui", "https://ca-five.example.io", "Primary"));

        Assert.Equal(["ca_two", "ca_five"], ApplicationText.Boxes(second, "ui").Select(box => box.Alias));

        var third = Diagram(
            Box("ca_five", "ui", "https://ca-five.example.io", "PRIMARY"),
            Box("ca_two", "ui", "https://ca-two.example.io", "primary"));

        Assert.Equal(["ca_five", "ca_two"], ApplicationText.Boxes(third, "ui").Select(box => box.Alias));

        // Nothing is reordered by a diagram of one box, and nothing is asked of a name the address refuses.
        var single = Diagram(Box("ca_ui", "ui"));

        Assert.Equal(["ca_ui"], ApplicationText.Boxes(single, "ui").Select(box => box.Alias));
        Assert.Empty(ApplicationText.Boxes(manifest, "ui/../x"));
        Assert.Empty(ApplicationText.Boxes(manifest, "ui.server"));
        Assert.Empty(ApplicationText.Boxes(manifest, At(41)));
        Assert.Empty(ApplicationText.Boxes(manifest, string.Empty));
        Assert.Empty(ApplicationText.Boxes(manifest, " "));
        Assert.Empty(ApplicationText.Boxes(manifest, null));
        Assert.Empty(ApplicationText.Boxes(Diagram(), "ui"));
    }

    /// <summary>The view of one deployable and the view of the whole diagram take one walk and decide a box alike: for
    /// every deployable on a mixed diagram, asked in any case, the boxes of that deployable are exactly the diagram's
    /// boxes whose deployable is that name, the primary first and the rest in the diagram's order - no node without an
    /// architecture or with a name the address refuses slips into the one and not the other.</summary>
    [Fact]
    public void TheBoxesOfOneDeployableAreTheDiagramsBoxesOfThatName()
    {
        var manifest = Diagram(
            Box("ca_ui_standby", "ui", "https://ca-ui-2.example.io", "standby"),
            Box("ca_api", "API", "https://ca-api.example.io"),
            new RuntimeNode("no_architecture", RuntimeNodeKind.ContainerApp, "ca-x", Deployable: "ui"),
            Box("ca_refused", "ui/../x"),
            Box("ca_ui_primary", "UI", "https://ca-ui.example.io", "primary"),
            Box("ca_api_two", "api", "https://ca-api-2.example.io"));
        var all = ApplicationText.Boxes(manifest);

        foreach (var deployable in new[] { "ui", "Ui", "api", "API", "worker" })
        {
            var expected = all
                .Where(box => string.Equals(box.Deployable, deployable, StringComparison.OrdinalIgnoreCase))
                .OrderBy(box => string.Equals(box.Role, "primary", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .Select(box => box.Alias);

            Assert.Equal(expected, ApplicationText.Boxes(manifest, deployable).Select(box => box.Alias));
        }

        Assert.Equal(["ca_ui_primary", "ca_ui_standby"], ApplicationText.Boxes(manifest, "ui").Select(box => box.Alias));
        Assert.Equal(["ca_api", "ca_api_two"], ApplicationText.Boxes(manifest, "api").Select(box => box.Alias));
        Assert.Empty(ApplicationText.Boxes(manifest, "worker"));
    }

    /// <summary>The name of a deployable is matched by its letters without heeding their case - ordinally, so it is
    /// the same match whatever the reader speaks: a diagram that names <c>ui</c> is found by <c>UI</c> and by
    /// <c>uI</c> for a reader of German exactly as for a reader of English, and a name that differs by more than its
    /// case is a different deployable.</summary>
    [Fact]
    public void TheDeployablesNameIsMatchedWithoutHeedingItsCaseWhateverTheReaderSpeaks()
    {
        var manifest = Diagram(Box("ca_ui", "ui"), Box("ca_aisf", "aisf"));

        Assert.Equal(["ca_ui"], ApplicationText.Boxes(manifest, "UI").Select(box => box.Alias));
        Assert.Equal(["ca_ui"], ApplicationText.Boxes(manifest, "uI").Select(box => box.Alias));
        Assert.Equal(["ca_ui"], ApplicationText.Boxes(manifest, "Ui").Select(box => box.Alias));
        Assert.Empty(ApplicationText.Boxes(manifest, "u1"));

        ViewerCulture.German(() =>
        {
            Assert.Equal(["ca_ui"], ApplicationText.Boxes(manifest, "UI").Select(box => box.Alias));
            Assert.Equal(["ca_aisf"], ApplicationText.Boxes(manifest, "AISF").Select(box => box.Alias));
            Assert.Empty(ApplicationText.Boxes(manifest, "U"));
        });
    }

    /// <summary>Without a diagram there is nothing to look at: both views of the boxes refuse the nothing by the name
    /// of the argument they were given, rather than answering "no boxes" - a manifest that is not there is a page that
    /// was built wrongly, not a diagram with nothing on it.</summary>
    [Fact]
    public void AViewOfTheBoxesWithoutAManifestSaysWhichArgumentItWanted()
    {
        var manifest = Assert.Throws<ArgumentNullException>(() => ApplicationText.Boxes(null!));
        var ofOne = Assert.Throws<ArgumentNullException>(() => ApplicationText.Boxes(null!, "ui"));

        Assert.Equal("manifest", manifest.ParamName);
        Assert.Equal("manifest", ofOne.ParamName);
    }

    /// <summary>The accessible name of a box says the deployable's word as it stands and guards nothing: the name is
    /// built for a box that <see cref="ApplicationText.Boxes(RuntimeManifest)" /> chose, so a node held out by hand -
    /// one whose deployable the address would refuse, or has none at all - is named with that word, empty or hostile,
    /// and with the nothing of a deployable that is not there as the empty place between the two spaces. What may be
    /// a box is decided where the boxes are chosen.</summary>
    [Theory]
    [InlineData("ui", "Application ui: open its assemblies")]
    [InlineData("UI", "Application UI: open its assemblies")]
    [InlineData("ui-server", "Application ui-server: open its assemblies")]
    [InlineData("Work-Order_api2", "Application Work-Order_api2: open its assemblies")]
    [InlineData("ui/../x", "Application ui/../x: open its assemblies")]
    [InlineData("ui server", "Application ui server: open its assemblies")]
    [InlineData("", "Application : open its assemblies")]
    [InlineData(" ", "Application  : open its assemblies")]
    [InlineData("<b>no deployable</b>", "Application <b>no deployable</b>: open its assemblies")]
    public void TheNameOfABoxSaysItsDeployablesWordAsItStands(string deployable, string label) =>
        Assert.Equal(label, ApplicationText.BoxLabel(Box("ca_ui", deployable)));

    /// <summary>The words of a box whose deployable the manifest does not name at all: the nothing is said as the
    /// empty place, as a deployable named by nothing is - the label is one sentence with two fixed parts and the
    /// word between them, whatever that word is.</summary>
    [Fact]
    public void ABoxWithoutADeployableIsNamedWithAnEmptyPlace()
    {
        Assert.Equal("Application : open its assemblies", ApplicationText.BoxLabel(Box("ca_ui", null)));
        Assert.Equal(ApplicationText.BoxLabel(Box("ca_ui", string.Empty)), ApplicationText.BoxLabel(Box("ca_ui", null)));
    }

    /// <summary>Without a node there is no box to name: the refusal carries the name of the argument, which is the
    /// only thing the label has to say about a box that is not there.</summary>
    [Fact]
    public void TheNameOfNoBoxIsRefused() =>
        Assert.Equal("node", Assert.Throws<ArgumentNullException>(() => ApplicationText.BoxLabel(null!)).ParamName);

    // ------------------------------------------------------------- the version the view shows, of the nodes

    /// <summary>The version a node reported is trimmed as the pin is, and the word the view shows is the trimmed one:
    /// an answer that arrives with spaces, a tab or a newline about it is the version inside them, and the pin beside
    /// it is left alone - one report is enough for the environment's pin to say nothing at all.</summary>
    [Theory]
    [InlineData(" 1.0.42 ", "1.0.42")]
    [InlineData("\t1.0.42", "1.0.42")]
    [InlineData("1.0.42\n", "1.0.42")]
    [InlineData("  1.0.42-beta.1+build_7  ", "1.0.42-beta.1+build_7")]
    [InlineData("1.0.42", "1.0.42")]
    public void AReportedVersionIsSaidWithoutTheWhitespaceItArrivedWith(string reported, string shown) =>
        Assert.Equal(new ApplicationVersion(shown, Runs: true), ApplicationText.Version([("primary", reported)], "9.9.9"));

    /// <summary>One node reporting is enough to refuse the pin whatever the role that reported: the environment's
    /// word is not asked at all while any node has a version, so the standby's answer and the answer of a node whose
    /// role the manifest does not name are shown in place of the pin, with
    /// <see cref="ApplicationVersion.Runs" /> true - which is what tells the two lines of the view apart.</summary>
    [Theory]
    [InlineData("standby")]
    [InlineData(null)]
    [InlineData("data")]
    [InlineData("primary")]
    public void OneReportIsEnoughToRefuseThePinWhateverTheRoleThatMadeIt(string? role) =>
        Assert.Equal(new ApplicationVersion("1.0.41", Runs: true), ApplicationText.Version([(role, " 1.0.41 ")], "1.0.40"));

    /// <summary>The pin is trimmed as a report is - the whitespace it arrives with is no part of the version - and a
    /// pin of nothing but whitespace is no pin at all: the view answers with the nothing rather than with a version of
    /// spaces, which would name no file and say nothing.</summary>
    [Theory]
    [InlineData("\t1.0.40\n", "1.0.40")]
    [InlineData("  1.0.40  ", "1.0.40")]
    [InlineData("1.0.40", "1.0.40")]
    public void APinIsTrimmedToItsWord(string pinned, string shown) =>
        Assert.Equal(new ApplicationVersion(shown, Runs: false), ApplicationText.Version([("primary", null)], pinned));

    /// <summary>The first of several primaries is the one the view shows - the order of the nodes decides between two
    /// nodes that both claim the primary's role - and a role with whitespace about it is no primary at all: the role
    /// is compared as it stands, unlike the version, which is trimmed. A standby that is not primary never wins over
    /// an earlier node of another role.</summary>
    [Fact]
    public void TheFirstOfSeveralPrimariesIsTheOneShownAndARoleOfSpacesIsNoPrimary()
    {
        Assert.Equal("1.0.1", ApplicationText.Version([("primary", "1.0.1"), ("primary", "1.0.2")], null)!.Text);
        Assert.Equal("1.0.2", ApplicationText.Version([("standby", "1.0.1"), ("primary", "1.0.2")], null)!.Text);

        // Neither of these roles is the primary's, so the first node that reported is shown.
        Assert.Equal("1.0.1", ApplicationText.Version([(null, "1.0.1"), (" primary ", "1.0.2")], null)!.Text);
        Assert.Equal("1.0.1", ApplicationText.Version([("PRIMARY ", "1.0.1"), ("primary\t", "1.0.2")], null)!.Text);
        Assert.Equal("1.0.1", ApplicationText.Version([("standby", "1.0.1"), ("", "1.0.2")], null)!.Text);

        // The primary that reported nothing is not a report: the node that did speak is shown, and its role does not matter.
        Assert.Equal("1.0.2", ApplicationText.Version([("primary", null), ("primary", "1.0.2")], "1.0.9")!.Text);
        Assert.Equal("1.0.2", ApplicationText.Version([("primary", "   "), ("standby", "1.0.2")], "1.0.9")!.Text);
    }

    /// <summary>Nothing reported and nothing pinned is nothing known - the empty sequence of nodes, the pin of nothing,
    /// of spaces, of a tab and of the empty word are each no version - and a node whose report is whitespace is a node
    /// that did not report at all, so that the view never shows a version of nothing while a word is known.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void WithoutAReportAndWithoutAPinNothingIsKnown(string? pinned)
    {
        IEnumerable<(string? Role, string? Version)> noNodes = [];

        Assert.Null(ApplicationText.Version(noNodes, pinned));
        Assert.Null(ApplicationText.Version([("primary", null)], pinned));
        Assert.Null(ApplicationText.Version([("primary", " "), ("standby", "\t")], pinned));
        Assert.Null(ApplicationText.Version([("primary", string.Empty)], pinned));
        Assert.Null(ApplicationText.Version([("primary", "   ")], pinned));
    }

    /// <summary>Without nodes there is nothing to ask: the nothing of a sequence is refused by the name of the
    /// argument rather than answered as "no node reported", which would let a page built wrongly show a pin it was
    /// never asked to show.</summary>
    [Fact]
    public void AVersionOfNoNodesAtAllIsRefused()
    {
        IEnumerable<(string? Role, string? Version)>? none = null;

        Assert.Equal("nodes", Assert.Throws<ArgumentNullException>(() => ApplicationText.Version(none!, "1.0.42")).ParamName);
    }

    // --------------------------------------------------------- the version of a deployable, through its boxes

    /// <summary>The environment the boxes of a diagram are checked against: the first of the topologies the test
    /// writes, with its nodes as the file names them.</summary>
    private static EnvironmentStatus Uat(string topology) =>
        new(TopologyParser.Parse(topology).Topology!.Environments[0]);

    /// <summary>A check the test itself made, of a node that answered the given version.</summary>
    private static ProbeResult Answer(string? version) => new(HealthState.Healthy, 200, 40, Now, version);

    /// <summary>The version the view shows of a deployable is the one its primary box's node reported, whichever box
    /// of the diagram the primary is and whichever order the boxes are looked at in: a node is found by its address,
    /// as its tile is, so the standby's report never stands in for the primary's, and the primary's is shown while any
    /// node reported.</summary>
    [Fact]
    public void TheVersionOfTheBoxesIsTheOneTheirPrimaryNodeReported()
    {
        var standby = Box("ca_api", "ui", "https://ca-api.example.io", "standby");
        var primary = Box("ca_ui", "ui", "https://ca-ui.example.io", "primary");
        var uat = Uat("""
            { "environments": [ { "name": "uat", "deployables": [ { "name": "ui", "nodes": [
                { "name": "ca-api", "role": "standby", "url": "https://ca-api.example.io" },
                { "name": "ca-ui", "role": "primary", "url": "https://ca-ui.example.io" } ] } ] } ] }
            """);
        uat.Record(PinnedVersions.Parse("""{ "ui": "1.0.40" }"""));
        uat.Deployables[0].Nodes[0].Record(Answer("1.0.1"));
        uat.Deployables[0].Nodes[1].Record(Answer("1.0.2"));

        Assert.Equal(new ApplicationVersion("1.0.2", Runs: true), ApplicationText.Version([standby, primary], uat));
        Assert.Equal(new ApplicationVersion("1.0.2", Runs: true), ApplicationText.Version([primary, standby], uat));

        // A box whose address the topology holds no node of reported nothing: the box that was found speaks.
        var alien = Box("ca_other", "ui", "https://ca-other.example.io", "primary");

        Assert.Equal(new ApplicationVersion("1.0.1", Runs: true), ApplicationText.Version([alien, standby], uat));
        Assert.Equal(new ApplicationVersion("1.0.2", Runs: true), ApplicationText.Version([alien, primary], uat));
    }

    /// <summary>A box with no address can report nothing, since nothing is asked of it: the version the view shows is
    /// then the environment's pin, and while the environment pins nothing - no file, or a file that names no pin of
    /// this deployable - nothing is known and no file is asked for.</summary>
    [Fact]
    public void ABoxWithNoAddressReportsNothingSoThePinIsShown()
    {
        var box = Box("ca_ui", "ui");
        var uat = Uat("""
            { "environments": [ { "name": "uat", "deployables": [ { "name": "ui", "nodes": [
                { "name": "ca-ui", "role": "primary", "url": "https://ca-ui.example.io" } ] } ] } ] }
            """);
        uat.Deployables[0].Nodes[0].Record(Answer("1.0.1"));

        Assert.Null(ApplicationText.Version([box], uat));
        uat.Record(PinnedVersions.Parse("""{ "ui": "1.0.40" }"""));
        Assert.Equal(new ApplicationVersion("1.0.40", Runs: false), ApplicationText.Version([box], uat));

        // A box that is not the environment's deployable at all is no report either.
        var other = Box("ca_aisf", "aisf", "https://ca-aisf.example.io");

        Assert.Null(ApplicationText.Version([other], uat));
    }

    /// <summary>A node that reported only spaces reported nothing: the whitespace of an answer is no version, and the
    /// pin stands until a real one arrives - the same rule the version of a sequence of nodes keeps.</summary>
    [Fact]
    public void ANodeThatReportedOnlyWhitespaceReportedNothing()
    {
        var box = Box("ca_ui", "ui", "https://ca-ui.example.io", "primary");
        var uat = Uat("""
            { "environments": [ { "name": "uat", "deployables": [ { "name": "ui", "nodes": [
                { "name": "ca-ui", "role": "primary", "url": "https://ca-ui.example.io" } ] } ] } ] }
            """);
        uat.Record(PinnedVersions.Parse("""{ "ui": "1.0.40" }"""));
        uat.Deployables[0].Nodes[0].Record(Answer("   "));

        Assert.Equal(new ApplicationVersion("1.0.40", Runs: false), ApplicationText.Version([box], uat));

        uat.Deployables[0].Nodes[0].Record(Answer(null));

        Assert.Equal(new ApplicationVersion("1.0.40", Runs: false), ApplicationText.Version([box], uat));

        uat.Deployables[0].Nodes[0].Record(Answer("1.0.42"));

        Assert.Equal(new ApplicationVersion("1.0.42", Runs: true), ApplicationText.Version([box], uat));
    }

    /// <summary>The deployable the pin is asked for is the one the first box names, found in the topology without
    /// heeding its case and asked of the pin file by the spelling the topology gives it: a diagram that writes
    /// <c>ui</c> reads the pin of the environment's <c>UI</c>, and the pin file's entry under <c>ui</c> is not the
    /// entry for a deployable the topology spells <c>UI</c>, since the names of that file are matched by their
    /// letters.</summary>
    [Fact]
    public void ThePinIsFoundByTheDeployablesNameAsTheTopologySpellsIt()
    {
        IReadOnlyList<RuntimeNode> boxes = [Box("ca_ui", "ui")];
        var uat = Uat("""
            { "environments": [ { "name": "uat", "deployables": [ { "name": "UI", "nodes": [] } ] } ] }
            """);
        uat.Record(PinnedVersions.Parse("""{ "UI": "1.0.40" }"""));

        Assert.Equal(new ApplicationVersion("1.0.40", Runs: false), ApplicationText.Version(boxes, uat));

        // The same file keyed by the address's spelling instead of the topology's holds no pin for that deployable.
        var keyed = Uat("""
            { "environments": [ { "name": "uat", "deployables": [ { "name": "UI", "nodes": [] } ] } ] }
            """);
        keyed.Record(PinnedVersions.Parse("""{ "ui": "1.0.40" }"""));

        Assert.Null(ApplicationText.Version(boxes, keyed));
    }

    /// <summary>A deployable whose own pin file the topology names has its pin from that file and from nowhere else:
    /// until it has been read the view knows no version, although the environment's <c>versions.json</c> pins one,
    /// and once it is read it is the deployable's own word that is shown. A page that fell back to the environment's
    /// file here would show a version the deployable is not pinned to.</summary>
    [Fact]
    public void ThePinOfADeployableThatNamesItsOwnFileAwaitsThatFile()
    {
        IReadOnlyList<RuntimeNode> boxes = [Box("ca_ui", "ui")];
        var uat = Uat("""
            { "environments": [ { "name": "uat", "versionsUrl": "https://example.net/versions.json", "deployables": [
                { "name": "ui", "pinUrl": "https://example.net/overlays/uat/kustomization.yaml", "nodes": [] } ] } ] }
            """);
        uat.Record(PinnedVersions.Parse("""{ "ui": "1.0.40" }"""));

        // The deployable's own file is named but not read: the environment's pin does not stand in for it.
        Assert.Null(ApplicationText.Version(boxes, uat));

        uat.Deployables[0].RecordPin(PinnedVersions.Parse("""{ "ui": "2.0.7" }"""));

        Assert.Equal(new ApplicationVersion("2.0.7", Runs: false), ApplicationText.Version(boxes, uat));

        // What a node reported still outranks either pin.
        var box = Box("ca_ui", "ui", "https://ca-ui.example.io", "primary");
        var running = Uat("""
            { "environments": [ { "name": "uat", "versionsUrl": "https://example.net/versions.json", "deployables": [
                { "name": "ui", "pinUrl": "https://example.net/overlays/uat/kustomization.yaml", "nodes": [
                    { "name": "ca-ui", "role": "primary", "url": "https://ca-ui.example.io" } ] } ] } ] }
            """);
        running.Deployables[0].Nodes[0].Record(Answer("3.1.0"));

        Assert.Equal(new ApplicationVersion("3.1.0", Runs: true), ApplicationText.Version([box], running));
    }

    /// <summary>Nothing to look at and nowhere to look: no box, no environment, and no environment at all are each no
    /// version known, and the boxes that are there are not asked which node of an environment that is not there
    /// reported. Without the boxes themselves the view refuses the argument by its name, as it does a manifest.</summary>
    [Fact]
    public void WithoutBoxesOrWithoutAnEnvironmentNothingIsKnown()
    {
        IReadOnlyList<RuntimeNode> boxes = [Box("ca_ui", "ui", "https://ca-ui.example.io")];
        var uat = Uat("""{ "environments": [ { "name": "uat", "deployables": [] } ] }""");
        IReadOnlyList<RuntimeNode> noBoxes = [];

        Assert.Null(ApplicationText.Version(noBoxes, uat));
        Assert.Null(ApplicationText.Version(boxes, null));
        Assert.Null(ApplicationText.Version(noBoxes, null));

        IReadOnlyList<RuntimeNode>? none = null;

        Assert.Equal("boxes", Assert.Throws<ArgumentNullException>(() => ApplicationText.Version(none!, uat)).ParamName);
    }

    // -------------------------------------------------------------------------------------------- the file of a version

    /// <summary>The address of the diagram is the one thing the file is built onto, and it is taken as it stands with
    /// the slashes it ends with trimmed off, so that a base written with none, one or two of them names the same
    /// file; the version's own marks are escaped - the plus of build metadata above all, which a path would otherwise
    /// read as a space - and the extension is what the caller says it is.</summary>
    [Theory]
    [InlineData("https://example.net/arch/ui", "https://example.net/arch/ui/1.0.42.svg")]
    [InlineData("https://example.net/arch/ui/", "https://example.net/arch/ui/1.0.42.svg")]
    [InlineData("https://example.net/arch/ui//", "https://example.net/arch/ui/1.0.42.svg")]
    [InlineData("https://example.net/", "https://example.net/1.0.42.svg")]
    [InlineData("https://example.net", "https://example.net/1.0.42.svg")]
    public void TheFileOfAVersionIsBuiltOnTheAddressAsItStands(string architecture, string file)
    {
        var asked = ApplicationText.File(new Uri(architecture), "1.0.42", "svg");

        Assert.NotNull(asked);
        Assert.Equal(file, asked!.AbsoluteUri);
    }

    /// <summary>A version of the marks a file name takes is escaped into one: the plus of build metadata, which a
    /// server reads as a space if it is asked as it stands, and nothing else - the dot, the dash and the underscore
    /// are whole in a name already. Without the forty letters of a version there is no address at all, and no part of
    /// one is invented for it.</summary>
    [Theory]
    [InlineData("1.0.42", "1.0.42")]
    [InlineData("1.0.42-beta.1", "1.0.42-beta.1")]
    [InlineData("1.0.42_build_7", "1.0.42_build_7")]
    [InlineData("1.0.42+build_7", "1.0.42%2Bbuild_7")]
    [InlineData("1.0.42+b+c", "1.0.42%2Bb%2Bc")]
    public void AVersionsMarksAreEscapedIntoAFileName(string version, string named) =>
        Assert.Equal($"{Base}/{named}.json", ApplicationText.File(new Uri(Base), version, "json")!.AbsoluteUri);

    /// <summary>Without an address there is no repository to ask: the refusal carries the name of the argument, and no
    /// version - however whole - is joined onto a base that is not there.</summary>
    [Fact]
    public void AFileOfNoAddressIsRefused()
    {
        var threw = Assert.Throws<ArgumentNullException>(() => ApplicationText.File(null!, "1.0.42", "svg"));

        Assert.Equal("architecture", threw.ParamName);
    }

    // --------------------------------------------------------------------------------- the words of the diagram

    /// <summary>An architecture of the given number of assemblies, with the entry, the kind, the process and the
    /// truncation a test brings: the file's parts as the view holds them, without reading a file.</summary>
    private static ApplicationArchitecture Architecture(
        int assemblies,
        string? entry = null,
        string? kind = null,
        string? process = null,
        bool truncated = false) =>
        new(kind, process, entry, [.. Enumerable.Range(0, assemblies).Select(number => new ApplicationAssembly($"A{number}", Entry: false, [], []))], truncated);

    /// <summary>The alternative text of a diagram that lists no assembly says so, and says it without the entry the
    /// file named: the entry of an empty list is no help to a reader, and the nothing of a process is a diagram with
    /// nothing about it. A process that is the empty word is still a process the file named, and is said as the empty
    /// place after its word for it.</summary>
    [Theory]
    [InlineData(null, "Architecture of ui 1.0.42: no assemblies")]
    [InlineData("", "Architecture of ui 1.0.42: no assemblies, process ")]
    [InlineData("dotnet Ui.Server.dll", "Architecture of ui 1.0.42: no assemblies, process dotnet Ui.Server.dll")]
    public void TheTextOfADiagramWithNoAssembliesSaysSo(string? process, string alt) =>
        Assert.Equal(alt, ApplicationText.AltText("ui", "1.0.42", Architecture(0, entry: "Ui.Server", process: process)));

    /// <summary>The alternative text of a diagram whose file names no entry says the count and stops there, and the
    /// count is the only word for it: one assembly in the words one assembly is, two and a thousand in the words
    /// two and two thousand assemblies are. The entry that is the empty word is an entry the file named, and is said as the empty
    /// place after the word for it.</summary>
    [Theory]
    [InlineData(1, null, "Assemblies of ui 1.0.42: 1 assembly")]
    [InlineData(2, null, "Assemblies of ui 1.0.42: 2 assemblies")]
    [InlineData(3, "Ui.Server", "Assemblies of ui 1.0.42: 3 assemblies, entry Ui.Server")]
    [InlineData(1, "Ui.Server", "Assemblies of ui 1.0.42: 1 assembly, entry Ui.Server")]
    [InlineData(2000, "Ui.Server", "Assemblies of ui 1.0.42: 2000 assemblies, entry Ui.Server")]
    [InlineData(1, "", "Assemblies of ui 1.0.42: 1 assembly, entry ")]
    [InlineData(0, null, "Architecture of ui 1.0.42: no assemblies")]
    public void TheTextOfADiagramCountsItsAssemblies(int assemblies, string? entry, string alt) =>
        Assert.Equal(alt, ApplicationText.AltText("ui", "1.0.42", Architecture(assemblies, entry)));

    /// <summary>The count is the invariant culture's, so no reader puts a mark of thousands in it: the twelve thousand
    /// three hundred and forty-five assemblies a reader of German would count <c>12.345</c> are said in the five
    /// letters they are spelled with, in the one alternative text the page gives an image - and the count of the most
    /// assemblies a file ever lists is said whole, not cut and not counted in threes.</summary>
    [Fact]
    public void TheCountOfAssembliesIsTheInvariantOneWhateverTheReaderSpeaks()
    {
        var many = Architecture(12_345, "Ui.Server");

        Assert.Equal("Assemblies of ui 1.0.42: 12345 assemblies, entry Ui.Server", ApplicationText.AltText("ui", "1.0.42", many));
        Assert.Equal("Assemblies of ui 1.0.42: 1000 assemblies", ApplicationText.AltText("ui", "1.0.42", Architecture(1_000)));

        ViewerCulture.German(() =>
        {
            Assert.Equal("Assemblies of ui 1.0.42: 12345 assemblies, entry Ui.Server", ApplicationText.AltText("ui", "1.0.42", many));
            Assert.Equal(
                "Architecture of api 2.0.0: no assemblies, process node server.js",
                ApplicationText.AltText("api", "2.0.0", Architecture(0, process: "node server.js")));
        });
    }

    /// <summary>An alternative text names the deployable and the version it was built for as they are said to it, and
    /// guards neither: a name of nothing is the empty place between the words, and the version of a diagram the page
    /// was shown is whatever word the view was taken with - the two are the page's own words, not a file's.</summary>
    [Theory]
    [InlineData("ui", "1.0.42", "Assemblies of ui 1.0.42: 1 assembly")]
    [InlineData("", "1.0.42", "Assemblies of  1.0.42: 1 assembly")]
    [InlineData("ui", "", "Assemblies of ui : 1 assembly")]
    [InlineData("", "", "Assemblies of  : 1 assembly")]
    [InlineData("Work-Order_api2", "1.0.42-beta.1", "Assemblies of Work-Order_api2 1.0.42-beta.1: 1 assembly")]
    public void TheAlternativeTextSaysTheNamesItWasGiven(string deployable, string version, string alt) =>
        Assert.Equal(alt, ApplicationText.AltText(deployable, version, Architecture(1)));

    /// <summary>Without a diagram there is no alternative text: the count, the entry and the process are all of a file
    /// that has to be there, and the refusal says which argument was wanted.</summary>
    [Fact]
    public void ADiagramThatIsNotThereIsRefusedByName()
    {
        Assert.Equal("architecture", Assert.Throws<ArgumentNullException>(() => ApplicationText.AltText("ui", "1.0.42", null!)).ParamName);
        Assert.Equal("architecture", Assert.Throws<ArgumentNullException>(() => ApplicationText.KindLine(null!)).ParamName);
        Assert.Equal("architecture", Assert.Throws<ArgumentNullException>(() => ApplicationText.TruncatedLine(null!)).ParamName);
    }

    // --------------------------------------------------------------- the lines the file answers with, or says nothing

    /// <summary>An empty word of a kind or of a process is a word the file said: the line distinguishes it from the
    /// nothing it answers with silence to, and says it as the empty place it is - the kind alone, the process alone
    /// with its word for it, and both with the two of them joined. <c>ApplicationParser</c> never yields an empty word
    /// of either, so it is only a hand-built architecture - the <c>with</c> of a line the page turns - that says one.</summary>
    [Theory]
    [InlineData(null, null, null)]
    [InlineData("", null, "")]
    [InlineData(null, "", "process: ")]
    [InlineData("", "", ", process: ")]
    [InlineData("container image", null, "container image")]
    [InlineData(null, "sh", "process: sh")]
    [InlineData("", "sh", ", process: sh")]
    [InlineData("container image", "", "container image, process: ")]
    [InlineData("container image", "dotnet Ui.Server.dll", "container image, process: dotnet Ui.Server.dll")]
    public void TheKindLineSaysAnEmptyWordAsAWordAndANothingAsNothing(string? kind, string? process, string? line) =>
        Assert.Equal(line, ApplicationText.KindLine(Architecture(0, kind: kind, process: process)));

    /// <summary>The line of a file that lists only a part of the assemblies is one fixed sentence, whatever the count
    /// it stands for and whatever the names of the deployable are, and a file that lists all of them is answered with
    /// the nothing rather than with an empty line: the note is either there or not there.</summary>
    [Theory]
    [InlineData(true, 1)]
    [InlineData(true, 2_000)]
    [InlineData(false, 0)]
    [InlineData(false, 7)]
    public void TheLineOfATruncatedFileIsTheOneSentenceItAlwaysIs(bool truncated, int assemblies)
    {
        var line = ApplicationText.TruncatedLine(Architecture(assemblies, truncated: truncated));

        Assert.Equal(truncated ? "The diagram and the list show only a part of the assemblies." : null, line);
    }

    /// <summary>The three states of a file are the three lines the view has: the missing diagram, the unreadable one,
    /// and the silence of the one that was read. A value that is no state at all - the number of an enum the page was
    /// never given - is answered with that same silence, so a state added later shows no line until it is decided
    /// what it says.</summary>
    [Theory]
    [InlineData(ApplicationReadState.Read)]
    [InlineData((ApplicationReadState)3)]
    [InlineData((ApplicationReadState)(-1))]
    [InlineData((ApplicationReadState)int.MaxValue)]
    public void AStateThatSaysNothingAboutAFileIsAnsweredWithSilence(ApplicationReadState state) =>
        Assert.Null(ApplicationText.ProblemLine("ui", "1.0.42", state));

    /// <summary>The two lines that do say something say the deployable and the version as they are given, with no
    /// guard and no placeholder: a name of nothing is the empty place between the words of the sentence.</summary>
    [Theory]
    [InlineData("ui", "1.0.42")]
    [InlineData("", "")]
    [InlineData("Work-Order_api2", "1.0.42-beta.1")]
    public void TheTwoLinesOfAFileSayTheNamesTheyWereGiven(string deployable, string version)
    {
        Assert.Equal(
            $"No architecture diagram of {deployable} {version} yet: its repository publishes one with each release.",
            ApplicationText.ProblemLine(deployable, version, ApplicationReadState.Missing));
        Assert.Equal(
            $"The architecture diagram of {deployable} {version} could not be read.",
            ApplicationText.ProblemLine(deployable, version, ApplicationReadState.Failed));
    }

    /// <summary>The line under the heading and its tooltip, and the line of a refused version: the words the view says
    /// of a deployable are said with the name as it was given, and the version of the line is the word the view holds,
    /// trimmed or not - the line changes nothing of it. The tooltip names the deployable and the environment and never
    /// the version, so the same two names say the same sentence whatever the version beside them is.</summary>
    [Fact]
    public void TheLinesOfAVersionSayItsWordAsTheVersionHoldsIt()
    {
        Assert.Equal("Version  1.0.42 , as it runs", ApplicationText.VersionLine("ui", new ApplicationVersion(" 1.0.42 ", Runs: true)));
        Assert.Equal("Version , as pinned", ApplicationText.VersionLine("ui", new ApplicationVersion(string.Empty, Runs: false)));
        Assert.Equal("Version 1.0.42, as it runs", ApplicationText.VersionLine(string.Empty, new ApplicationVersion("1.0.42", Runs: true)));
        Assert.Equal("The version of  is not known yet.", ApplicationText.VersionLine(string.Empty, null));
        Assert.Equal("The version of ui names no file: no diagram is asked for.", ApplicationText.RefusedLine("ui"));
        Assert.Equal("The version of  names no file: no diagram is asked for.", ApplicationText.RefusedLine(string.Empty));

        // The version is nowhere in the tooltip: two versions of one word and one of another say the same thing.
        Assert.Equal(
            ApplicationText.VersionTitle("ui", "uat", new ApplicationVersion("1.0.42", Runs: false)),
            ApplicationText.VersionTitle("ui", "uat", new ApplicationVersion(string.Empty, Runs: false)));
        Assert.Equal(
            "No node of  in  reported a version, and the environment pins none: no diagram is asked for.",
            ApplicationText.VersionTitle(string.Empty, string.Empty, null));
    }

    // ----------------------------------------------------------------------------------- the links of the heading row

    /// <summary>The links of a box, as the heading row takes them: <c>links.site</c> of the node stands in for its
    /// address, and only one of the two is asked - so an address of another scheme than https opens nothing, even
    /// beside an address that is https, since a link the deployment wrote wrong is not silently exchanged for another
    /// one. A box with nothing to open is no link, and the row of no boxes is empty.</summary>
    [Fact]
    public void ALinkOfAnotherSchemeOpensNothingEvenBesideAnAddressThatWould()
    {
        var written = Box("ca_ui", "ui", "https://ca-ui.example.io", links: Links((RuntimeBoxLink.Site, "http://ca-ui.example.io")));
        var plain = Box("ca_api", "ui", "http://ca-api.example.io");
        var site = Box("ca_web", "ui", "http://ca-web.example.io", links: Links((RuntimeBoxLink.Site, "https://ca-web.example.io")));

        // The site link is the box's own address as the deployment wrote it: the https one is asked for, the http one refused.
        Assert.Equal(["Open the app"], ApplicationText.Links([site], null).Select(link => link.Text));
        Assert.Equal(["https://ca-web.example.io/"], ApplicationText.Links([site], null).Select(link => link.Href));
        Assert.Empty(ApplicationText.Links([written], null));
        Assert.Empty(ApplicationText.Links([plain], null));
        Assert.Empty(ApplicationText.Links([written, plain], null));
        Assert.Empty(ApplicationText.Links([], null));

        // A box that opens nothing is no link at all, and no name is wasted on it: of the two boxes only the one
        // with an https address to open is asked for, and with two boxes in the row its name is said in its link.
        var two = ApplicationText.Links([written, site], null);

        Assert.Equal(["Open ca_web"], two.Select(link => link.Text));
        Assert.DoesNotContain("ca_ui", string.Join(' ', two.Select(link => link.Text)), StringComparison.Ordinal);
    }

    /// <summary>A box's page in the Azure portal is the one its node has in the topology, found by its address; the
    /// link the manifest wrote for the box stands only where the topology has none. One box says "Open in the Azure
    /// portal", and the row of its links is the address of the app and the address of the portal, each with its own
    /// name and its own tooltip.</summary>
    [Fact]
    public void TheTopologyPortalIsTheOneShownAndTheBoxesOwnStandsAlone()
    {
        var box = Box("ca_ui", "ui", "https://ca-ui.example.io", "primary", Links((LinkSet.Portal, "https://portal.example.net/#from-manifest")));
        var uat = Uat("""
            { "environments": [ { "name": "uat", "deployables": [ { "name": "ui", "nodes": [
                { "name": "ca-ui", "role": "primary", "url": "https://ca-ui.example.io", "links": { "portal": "https://portal.example.net/#from-topology" } } ] } ] } ] }
            """);

        Assert.Equal(
            [
                new ViewLink("Open the app", "https://ca-ui.example.io/", "ca_ui: ca-ui.example.io, as a browser opens it (opens in a new tab)"),
                new ViewLink("Open in the Azure portal", "https://portal.example.net/#from-topology", LinkText.Portal("ca_ui in the Azure portal")),
            ],
            ApplicationText.Links([box], uat));

        // The topology holds no node at that address: the manifest's own link is the one there is.
        var other = Uat("""
            { "environments": [ { "name": "uat", "deployables": [ { "name": "ui", "nodes": [
                { "name": "ca-api", "url": "https://ca-api.example.io", "links": { "portal": "https://portal.example.net/#another" } } ] } ] } ] }
            """);

        Assert.Equal(
            [
                new ViewLink("Open the app", "https://ca-ui.example.io/", "ca_ui: ca-ui.example.io, as a browser opens it (opens in a new tab)"),
                new ViewLink("Open in the Azure portal", "https://portal.example.net/#from-manifest", LinkText.Portal("ca_ui in the Azure portal")),
            ],
            ApplicationText.Links([box], other));

        // A box with no address has no node to be found: its own link stands, and nothing is asked of the topology.
        Assert.Equal(
            ["https://portal.example.net/#from-manifest"],
            ApplicationText.Links([Box("ca_ui", "ui", links: Links((LinkSet.Portal, "https://portal.example.net/#from-manifest")))], uat).Select(link => link.Href));
    }

    /// <summary>The version and the portal link find the node a box stands for by one rule: the first node of the
    /// topology at the box's address, compared as <see cref="Uri" /> equality compares them - the fragment is not
    /// heeded, so a box at <c>#one</c> stands for a node at <c>#two</c>, and of two nodes at one address the first in
    /// the topology's order is the one both members speak of.</summary>
    [Fact]
    public void TheVersionAndThePortalFindTheSameNodeByOneRule()
    {
        var box = Box("ca_ui", "ui", "https://ca-ui.example.io/a#one", "primary");
        var uat = Uat("""
            { "environments": [ { "name": "uat", "deployables": [
                { "name": "ui", "nodes": [
                    { "name": "ca-ui", "role": "primary", "url": "https://ca-ui.example.io/a#two", "links": { "portal": "https://portal.example.net/#first" } } ] },
                { "name": "aisf", "nodes": [
                    { "name": "ca-ui-again", "url": "https://ca-ui.example.io/a", "links": { "portal": "https://portal.example.net/#second" } } ] } ] } ] }
            """);
        uat.Deployables[0].Nodes[0].Record(Answer("1.0.1"));
        uat.Deployables[1].Nodes[0].Record(Answer("9.9.9"));

        Assert.Equal(new ApplicationVersion("1.0.1", Runs: true), ApplicationText.Version([box], uat));
        Assert.Equal(
            ["https://ca-ui.example.io/a#one", "https://portal.example.net/#first"],
            ApplicationText.Links([box], uat).Select(link => link.Href));

        // An address that differs by more than its fragment is another node: neither member finds one.
        var elsewhere = Box("ca_ui", "ui", "https://ca-ui.example.io/b#one", "primary");

        Assert.Null(ApplicationText.Version([elsewhere], uat));
        Assert.Equal(["https://ca-ui.example.io/b#one"], ApplicationText.Links([elsewhere], uat).Select(link => link.Href));
    }

    /// <summary>Every link of the heading row goes where the reader is told to expect them - the app itself and the
    /// portal - and each of them keeps its referrer, since nothing is hidden from those two. The one link of the view
    /// that asks for nothing to be sent with it is the diagram's own file on another origin, pinned with
    /// <see cref="ApplicationText.DiagramLink" />.</summary>
    [Fact]
    public void TheLinksOfTheHeadingRowTellTheirReferrer()
    {
        var box = Box("ca_ui", "ui", "https://ca-ui.example.io", links: Links((LinkSet.Portal, "https://portal.example.net/#ca-ui")));

        Assert.All(ApplicationText.Links([box], null), link => Assert.False(link.NoReferrer));
    }

    /// <summary>Without boxes there is no row to build: the nothing of a list is refused by the name of the argument,
    /// as a manifest is, so a page that lost its boxes says so rather than showing a heading row with nothing on it.</summary>
    [Fact]
    public void AHeadingRowOfNoBoxesIsRefused()
    {
        IReadOnlyList<RuntimeNode>? none = null;

        Assert.Equal("boxes", Assert.Throws<ArgumentNullException>(() => ApplicationText.Links(none!, null)).ParamName);
    }

    /// <summary>The diagram at full size is the image's own address, told to the view with no guard on the words it
    /// interpolates - the deployable and the version are the ones the page was taken with - and it is always the link
    /// that asks for nothing to be sent with it.</summary>
    [Fact]
    public void TheFullSizeLinkSaysTheNamesItWasGivenAndSendsNoReferrer()
    {
        var image = new Uri($"{Base}/{At(40)}.svg");
        var link = ApplicationText.DiagramLink(image, "ui", At(40));

        Assert.Equal(
            new ViewLink(
                "Open the diagram at full size",
                $"{Base}/{At(40)}.svg",
                $"The diagram of ui {At(40)} as its repository published it (opens in a new tab)",
                NoReferrer: true),
            link);
        // The two names are said with the one space that stands between them, whatever their length: nothing here
        // collapses a name that is not there into the space beside it.
        Assert.Equal("The diagram of   as its repository published it (opens in a new tab)", ApplicationText.DiagramLink(new Uri(Base), string.Empty, string.Empty).Title);

        var threw = Assert.Throws<ArgumentNullException>(() => ApplicationText.DiagramLink(null!, "ui", "1.0.42"));

        Assert.Equal("image", threw.ParamName);
    }

    /// <summary>A link of the topology, as the manifest writes them: the key and the address, in the order the file
    /// gives them.</summary>
    private static LinkSet Links(params (string Key, string Address)[] links) =>
        new(links.Select(link => KeyValuePair.Create(link.Key, new Uri(link.Address))));

    // ---------------------------------------------------------------------------- the class's own shape, and nothing else

    /// <summary>The whole public surface of the words of the application view, as it stands: the two guards of the
    /// names a file is addressed by, the two views of the boxes and their name, the two views of the version and the
    /// two lines it is said in, the file of a version and the diagram's own, the alternative text and the three lines
    /// a file answers with, the package as the list names it, the link of a refused version and the row of links - and
    /// nothing else to call. A word added to the class has to be written here as well, which is the point: the page
    /// cannot be shown a word nobody decided what it means.</summary>
    [Fact]
    public void TheSurfaceIsTheSixteenWordsAndGuardsOfTheView() =>
        Assert.Equal(
            [
                "AltText", "BoxLabel", "Boxes", "DiagramLink", "File", "IsName", "IsVersion", "KindLine",
                "Links", "Package", "ProblemLine", "RefusedLine", "TruncatedLine", "Version", "VersionLine", "VersionTitle",
            ],
            PublicSurface());

    /// <summary>The two members that are asked in two ways are asked in exactly those two ways: the boxes of a diagram
    /// and of one deployable, the version of a sequence of nodes and of the boxes with their environment - and a third
    /// way of either would be a decision nobody wrote down.</summary>
    [Fact]
    public void TheTwoWordsAskedInTwoWaysAreAskedInExactlyTwo()
    {
        Assert.Equal(2, Overloads("Boxes"));
        Assert.Equal(2, Overloads("Version"));
        Assert.All(Single, name => Assert.Equal(1, Overloads(name)));
    }

    /// <summary>The words the class says in exactly one way - everything but the two views of the boxes and the two
    /// views of the version.</summary>
    private static readonly string[] Single =
    [
        "AltText", "BoxLabel", "DiagramLink", "File", "IsName", "IsVersion", "KindLine",
        "Links", "Package", "ProblemLine", "RefusedLine", "TruncatedLine", "VersionLine", "VersionTitle",
    ];

    /// <summary>How many public methods of the class carry the given name.</summary>
    private static int Overloads(string name) =>
        typeof(ApplicationText)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Count(method => method.Name.Equals(name, StringComparison.Ordinal));

    /// <summary>The public members the class declares itself: the methods, the overloads counted once, and nothing
    /// else - no field, no property and no constant is public, since the two lengths a name and a version are bounded
    /// by are the class's own business.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(ApplicationText)
            .GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is FieldInfo || member is MethodInfo method && !method.IsSpecialName)
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
