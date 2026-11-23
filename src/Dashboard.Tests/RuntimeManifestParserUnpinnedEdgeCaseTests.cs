using System.Globalization;
using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, the null/empty and the fixed-value tests of <see cref="RuntimeManifestParser" /> that its own five
/// files leave unweighed: <see cref="RuntimeManifestParserTests" /> pins the deployment's sample files,
/// <see cref="RuntimeManifestParserEdgeCaseTests" /> the two readers refusing the same file, the shape of the index's
/// one required member and of a manifest's four lists, the words their elements fall back to and the two public limits
/// of the unplaced list, <see cref="RuntimeManifestParserBoundaryEdgeCaseTests" /> the reader's own arithmetic and the
/// order of its work, <see cref="RuntimeManifestParserJsonEdgeCaseTests" /> the file as a file - a member written
/// twice, an escape, the address as the <see cref="Uri" /> made it - and
/// <see cref="RuntimeManifestParserDatabaseAndIdsEdgeCaseTests" /> the two names of a database view and the lists of
/// ids of nothing but empty words. The parse result, the index and the frames are
/// <see cref="RuntimeIndexFrameAndParseResultTests" />, the links of a box are <see cref="RuntimeBoxLinkTests" />, a
/// database entry of an index is <see cref="DatabaseViewTests" /> and the architecture of a deployable is
/// <see cref="ApplicationViewTests" />.
/// <para>
/// What stands here is the rest of both public readers and of the two public limits: a file the deployment's own
/// editor saved with a mark before its first brace, which neither reader opens; that an index entry with no name is
/// refused by that alone, before either of its two files is weighed; that the shape of an entry's <c>database</c> is
/// asked of the members nobody wrote as much as of the two names <see cref="DatabaseViewTests" /> weighed; that the health
/// path's two hundred and an architecture's four hundred are weighed on the word once the room about it is gone; that
/// no length at all is weighed on the address a box is checked at, on the links of any element, or on the ids of a
/// box, where an unplaced address, an unplaced id and twice the longest unplaced text each carry one; that the user
/// mark of an address is no user's name where a user's name is refused; that a word of no host is the one address a
/// box is named for and no other element is weighed for it at all; that the reader's <c>TryGetInt32</c> is asked of a
/// relationship alone; and that every word the reader folds to lower is folded in the invariant culture, so a reader
/// whose language folds an <c>I</c> to a dotless one reads the same box.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: both readers take a string of JSON and hand back
/// a record, and every address, file name, id, word and number below is one the test brought.
/// </para>
/// </summary>
public class RuntimeManifestParserUnpinnedEdgeCaseTests
{
    /// <summary>The beginning of the addresses the tests below pad to an exact length.</summary>
    private const string AnAddressRoot = "https://demo.example.net/";

    /// <summary>An address of more than the two thousand an unplaced entry's address may not pass, and of all the
    /// four hundred an architecture may: a box is checked at it and a link leads to it all the same.</summary>
    private static readonly string ALongAddress = AnAddressRoot + new string('a', 3000);

    /// <summary>The mark <c>Set-Content -Encoding utf8BOM</c> and every editor that saves by its own habit write
    /// before the first character of a file, and the number of the character it makes in the text the reader is
    /// given.</summary>
    private const string Mark = "\uFEFF";

    // ----- The file itself, which both readers open the same way -----

    /// <summary>A file that begins with the mark a UTF-8 editor saved it with is no file to either reader, and both
    /// say it in the one message at the one place - the mark is not room the reader may skip (unlike a space, a tab
    /// and a newline, which make the empty file instead, and unlike the comment and the trailing comma, which it
    /// skips). The deployment writes both files next to the SVG it rendered, and a mark before the first brace is a
    /// page that shows nothing: this is the shape of that answer.</summary>
    [Theory]
    [InlineData(Mark + "{ \"environments\": [] }")]
    [InlineData(Mark + "{ \"environment\": \"tdd\" }")]
    [InlineData(Mark)]
    [InlineData(Mark + Mark + "{ }")]
    public void AFileTheDeploymentSavedWithAMarkIsNoFileToEitherReader(string file)
    {
        Assert.Equal(["The file is not valid JSON (line 1, position 1)."], RuntimeManifestParser.ParseIndex(file).Errors);
        Assert.Equal(["The file is not valid JSON (line 1, position 1)."], RuntimeManifestParser.ParseManifest(file).Errors);
    }

    // ----- The index: what an entry that is refused never has weighed -----

    /// <summary>An entry with no name is refused by that alone: the reader names the entry and hands it back as
    /// nothing before it asks either of its two files, so the two words of no file that stand in the same entry add
    /// no message to the file - while the place in the list is counted all the same, and the entry that has a name
    /// beside them is read. The whole index is refused either way: half an index of environments is no index to
    /// show.</summary>
    [Fact]
    public void AnEntryWithNoNameIsRefusedBeforeEitherOfItsTwoFilesIsWeighed()
    {
        var result = RuntimeManifestParser.ParseIndex("""
            { "environments": [ { "name": "  ", "manifest": "c:tdd.json", "svg": "../tdd.svg" },
                                { "name": "" },
                                { "name": "uat", "manifest": "uat.json", "svg": "uat.svg" } ] }
            """);

        Assert.False(result.IsValid);
        Assert.Null(result.Value);
        Assert.Equal(
            [
                "environments[0].name: missing or empty.",
                "environments[1].name: missing or empty.",
            ],
            result.Errors);
    }

    /// <summary>The <c>database</c> of an entry is an object of two plain names or it is no view at all: a number, a
    /// bool and a list are as little a view as nothing, one name without the other is no view, a name that is no word
    /// of its own is no name, and a member of the map spelled in another case is a member nobody wrote - the same
    /// rule <see cref="DatabaseViewTests" /> pins for the shapes and the two names it weighed, read here of the rest
    /// of them. None of it is a word said about the index.</summary>
    [Theory]
    [InlineData("7")]
    [InlineData("true")]
    [InlineData("[]")]
    [InlineData("""{ "manifest": "tdd.database.json" }""")]
    [InlineData("""{ "svg": 7, "manifest": "tdd.database.json" }""")]
    [InlineData("""{ "SVG": "tdd.database.svg", "Manifest": "tdd.database.json" }""")]
    public void ADatabaseThatNamesNoTwoPlainFilesIsNoViewAndSaysNothing(string database)
    {
        var entry = Entry($$"""{ "name": "tdd", "database": {{database}} }""");

        Assert.Equal(new RuntimeIndexEntry("tdd", "tdd.json", "tdd.svg"), entry);
        Assert.Null(entry.Database);
    }

    // ----- The two lengths the reader weighs once the room about the word is gone -----

    /// <summary>A health path is weighed at two hundred characters of the path, not of the text the file wrote about
    /// it: the room is trimmed first, so the two hundredth of them stands written between two pairs of spaces and the
    /// two hundred and first goes whole, cut by nothing (<see cref="RuntimeManifestParserEdgeCaseTests" /> pins the
    /// same three lengths of a word written with no room about it at all).</summary>
    [Theory]
    [InlineData(199, true)]
    [InlineData(200, true)]
    [InlineData(201, false)]
    public void AHealthPathIsWeighedOnceTheRoomAboutItIsGone(int length, bool kept)
    {
        var path = Padded("/" + new string('a', length - 1));

        Assert.Equal(kept ? (int?)length : null, Node($$"""{ "alias": "ca_ui", "kind": "containerapp", "healthPath": {{Json(path)}} }""").HealthPath?.Length);
    }

    /// <summary>The same of where a deployable publishes its diagrams: four hundred characters of the address, the
    /// room about it weighed by nothing, and the one over the limit left out whole. The limit is the health path's
    /// two hundred doubled and the address of an unplaced entry's two thousand are three rules of the reader's own,
    /// and none of them refuses the manifest.</summary>
    [Theory]
    [InlineData(399, true)]
    [InlineData(400, true)]
    [InlineData(401, false)]
    public void AnArchitectureIsWeighedOnceTheRoomAboutItIsGone(int length, bool kept)
    {
        var address = Padded(AnAddressRoot + new string('a', length - AnAddressRoot.Length));

        Assert.Equal(kept ? (int?)length : null, Node($$"""{ "alias": "app_ui", "kind": "webapp", "architecture": {{Json(address)}} }""").Architecture?.AbsoluteUri.Length);
    }

    // ----- Where the reader weighs no length at all -----

    /// <summary>Three of a manifest's words carry no length of their own, and the same long address proves it of all
    /// three: a box is checked at three thousand characters of it, and a frame, a region and a box each keep the same
    /// text as a link - while an unplaced entry's address is over its two thousand by the thousand and more, and goes
    /// whole (<see cref="RuntimeManifestParserBoundaryEdgeCaseTests" /> pins that limit at its own edge).</summary>
    [Fact]
    public void TheAddressABoxIsCheckedAtAndTheLinksOfEveryElementAreWeighedByNoLength()
    {
        Assert.Equal(3025, ALongAddress.Length);

        var manifest = Manifest($$"""
            { "environment": "tdd",
              "nodes": [ { "alias": "ca_ui", "kind": "containerapp", "url": {{Json(ALongAddress)}},
                           "links": { "portal": {{Json(ALongAddress)}} } } ],
              "regions": [ { "alias": "region_primary", "links": { "portal": {{Json(ALongAddress)}} } } ],
              "frames": [ { "alias": "sub", "kind": "subscription", "links": { "portal": {{Json(ALongAddress)}} } } ] }
            """);

        Assert.Equal(ALongAddress, Assert.Single(manifest.Nodes).Url!.AbsoluteUri);
        Assert.Equal(
            [3025, 3025, 3025],
            new[] { Assert.Single(manifest.Nodes).Links, Assert.Single(manifest.Regions).Links, Assert.Single(manifest.Frames!).Links }
                .Select(links => links![LinkSet.Portal]!.AbsoluteUri.Length));

        var entry = Unplaced($$"""{ "name": "kv", "url": {{Json(ALongAddress)}} }""");

        Assert.Null(entry.Site);
        Assert.Null(entry.Portal);
    }

    /// <summary>A box's resource ids are capped in their number and cut in nothing of their length: three thousand
    /// characters of an id stand whole, where the very same text as the id of an unplaced entry leaves as the first
    /// six hundred characters of it - twice the public <see cref="RuntimeManifestParser.LongestUnplacedText" />
    /// (<see cref="RuntimeManifestParserEdgeCaseTests" /> pins that cut at its own edge, and neither file pinned the
    /// box beside it).</summary>
    [Fact]
    public void TheResourceIdsOfABoxAreCappedInTheirNumberAndCutInNothingOfTheirLength()
    {
        var id = "/subscriptions/s/" + new string('a', 3000);
        Assert.Equal(3017, id.Length);

        Assert.Equal(id, Assert.Single(Node($$"""{ "alias": "res_1", "kind": "resource", "ids": [ {{Json(id)}} ] }""").Ids!));

        var entry = Unplaced($$"""{ "name": "kv", "ids": [ {{Json(id)}} ] }""");

        Assert.Equal(id[..(RuntimeManifestParser.LongestUnplacedText * 2)], Assert.Single(entry.Ids!));
    }

    // ----- The user an address may not name -----

    /// <summary>An unplaced address may name no user, and the rule is asked of what a user's name actually is: the
    /// mark of one with nobody behind it is no name and the address stands as the <see cref="Uri" /> made it, while a
    /// name of one letter and a name with a secret behind it are each refused whole, in all three places an entry's
    /// two addresses come from. A page asks nothing of the first and everything of the two.</summary>
    [Theory]
    [InlineData("https://@demo.example.net/x", "https://@demo.example.net/x")]
    [InlineData("https://user@demo.example.net/x", null)]
    [InlineData("https://user:secret@demo.example.net/x", null)]
    public void TheMarkOfAUserIsNoUserNameWhereAUserNameIsRefusedWhole(string address, string? kept)
    {
        var entry = Unplaced($$"""{ "name": "kv", "url": "{{address}}", "links": { "portal": "{{address}}", "site": "{{address}}" } }""");

        Assert.Equal(kept, entry.Portal?.AbsoluteUri);
        Assert.Equal(kept, entry.Site?.AbsoluteUri);

        var only = Unplaced($$"""{ "name": "kv", "links": { "site": "{{address}}" }, "url": "https://kept.example.net/" }""");

        Assert.Equal(kept is null ? "https://kept.example.net/" : kept, only.Site!.AbsoluteUri);
    }

    /// <summary>The address a box is checked at is weighed for its scheme and for nothing else: a user's name of one
    /// letter, a user's name with a secret and the bare mark of a user all stand in it as written, since the page
    /// opens nothing behind it - a deployment's private endpoint is checked where it is published.</summary>
    [Theory]
    [InlineData("https://@demo.example.net/x")]
    [InlineData("https://user@demo.example.net/x")]
    [InlineData("https://user:secret@demo.example.net/x")]
    public void TheAddressABoxIsCheckedAtKeepsTheUserAnUnplacedAddressMayNotName(string address) =>
        Assert.Equal(address, Node($$"""{ "alias": "ca_ui", "kind": "containerapp", "url": "{{address}}" }""").Url!.AbsoluteUri);

    // ----- Which element the reader weighs, and for what -----

    /// <summary>A word that is no address is the one thing of a box the reader refuses by name: a scheme of nothing
    /// behind it, a host of nothing before a port and a host that is no host at all are each the box's own message,
    /// which is the reader's words and no answer of the <see cref="Uri" />. Every other element of the file wears the
    /// same word in silence: no <c>url</c>, no <c>architecture</c> and no <c>healthPath</c> is asked of a region, a
    /// frame or a relationship, and an entry of the unplaced list keeps its own name and loses its own address.</summary>
    [Theory]
    [InlineData("https://")]
    [InlineData("http://")]
    [InlineData("https://:8080/x")]
    [InlineData("ftp://demo.example.net/x")]
    public void AWordThatIsNoAddressIsNamedOfABoxAloneAndKeptQuietOfEveryOtherElement(string address)
    {
        var box = RuntimeManifestParser.ParseManifest($$"""
            { "environment": "tdd", "nodes": [ { "alias": "ca_ui", "url": "{{address}}" } ] }
            """);

        Assert.False(box.IsValid);
        Assert.Equal(["nodes[0].url: not an absolute http or https address."], box.Errors);

        var manifest = Manifest($$"""
            { "environment": "tdd",
              "regions": [ { "alias": "region_primary", "url": "{{address}}", "architecture": "{{address}}", "healthPath": "{{address}}" } ],
              "frames": [ { "alias": "sub", "url": "{{address}}", "architecture": "{{address}}", "healthPath": "{{address}}" } ],
              "edges": [ { "id": "r-to-f", "from": "region_primary", "to": "sub", "url": "{{address}}", "architecture": "{{address}}" } ],
              "unplaced": [ { "name": "kv", "url": "{{address}}", "architecture": "{{address}}" } ] }
            """);

        Assert.Equal("region_primary", Assert.Single(manifest.Regions).Alias);
        Assert.Null(Assert.Single(manifest.Regions).Links);
        Assert.Equal("sub", Assert.Single(manifest.Frames!).Alias);
        Assert.Equal("r-to-f", Assert.Single(manifest.Edges).Id);
        var entry = Assert.Single(manifest.Unplaced!);
        Assert.Equal("kv", entry.Name);
        Assert.Null(entry.Site);
    }

    /// <summary>A host of six groups is a host the reader knows: a box checked on the loopback of <c>::1</c> or on a
    /// documented address keeps the brackets, the port and the path, and only the letters of the host and of the
    /// scheme come to lower case - the address a deployment of a private endpoint publishes itself with.</summary>
    [Theory]
    [InlineData("https://[::1]:8443/_health", "https://[::1]:8443/_health")]
    [InlineData("https://[2001:DB8::1]/_health", "https://[2001:db8::1]/_health")]
    public void ABoxCheckedOnAnAddressOfSixGroupsIsCheckedAllTheSame(string address, string kept) =>
        Assert.Equal(kept, Node($$"""{ "alias": "ca_ui", "kind": "containerapp", "url": "{{address}}" }""").Url!.AbsoluteUri);

    /// <summary>The <c>priority</c> a relationship is ordered by is the one member a file may not write as a word -
    /// and the reader asks it of a relationship's element alone: the same word written of a box, a region, a frame or
    /// an unplaced entry is a member nobody reads, and the manifest is read whole with every element of it
    /// (<see cref="RuntimeManifestParserEdgeCaseTests" /> pins what the same member written of a relationship does
    /// instead, which stops the file in its tracks).</summary>
    [Theory]
    [InlineData("\"1\"")]
    [InlineData("null")]
    [InlineData("{}")]
    public void AWordWrittenWhereNoReaderWeighsItIsNoTroubleToTheFile(string priority)
    {
        var manifest = Manifest($$"""
            { "environment": "tdd",
              "nodes": [ { "alias": "ca_ui", "kind": "containerapp", "priority": {{priority}} } ],
              "regions": [ { "alias": "region_primary", "priority": {{priority}} } ],
              "frames": [ { "alias": "sub", "kind": "subscription", "priority": {{priority}} } ],
              "edges": [ { "id": "r-to-s", "from": "region_primary", "to": "sub" } ],
              "unplaced": [ { "name": "kv", "priority": {{priority}} } ] }
            """);

        Assert.Equal("ca_ui", Assert.Single(manifest.Nodes).Alias);
        Assert.Equal("region_primary", Assert.Single(manifest.Regions).Alias);
        Assert.Equal("sub", Assert.Single(manifest.Frames!).Alias);
        Assert.Null(Assert.Single(manifest.Edges).Priority);
        Assert.Equal("kv", Assert.Single(manifest.Unplaced!).Name);
    }

    // ----- The culture the reader folds its words in -----

    /// <summary>Every word the reader matches against its own list of kinds is folded in the invariant culture and
    /// not in the reader's: a language that folds an <c>I</c> to a dotless one - and a language that folds nothing -
    /// read the same box and the same line, since a manifest is written by the deployment and not by whoever is
    /// looking at the page. A word folded the other way would be a kind the dashboard does not know, and a box that
    /// is no kind is a box that is never updated; and a capital <c>I</c> with a dot over it is the same word to no
    /// reader at all, which the invariant fold leaves as it is.</summary>
    [Theory]
    [InlineData("tr-TR")]
    [InlineData("de-DE")]
    [InlineData("")]
    public void TheWordsTheReaderFoldsToLowerAreFoldedInTheInvariantCulture(string reader)
    {
        // a word of no letters is the invariant culture's own name, which is the reader every machine brings to the file
        var culture = reader.Length == 0 ? CultureInfo.InvariantCulture : new CultureInfo(reader);

        Assert.Equal("istanbul", Under(culture, () => Node("""{ "alias": "a", "role": "ISTANBUL" }""").Role));
        Assert.Equal("i", Under(culture, () => Node("""{ "alias": "a", "role": "I" }""").Role));

        Assert.Equal(RuntimeNodeKind.StaticSite, Under(culture, () => Node("""{ "alias": "a", "kind": "STATICSITE" }""").Kind));
        Assert.Equal(RuntimeNodeKind.ContainerApp, Under(culture, () => Node("""{ "alias": "a", "kind": "CONTAINERAPP" }""").Kind));
        Assert.Equal(RuntimeNodeKind.WebApp, Under(culture, () => Node("""{ "alias": "a", "kind": "WEBAPP" }""").Kind));
        // the JSON escape of a capital I with a dot over it, which the invariant fold leaves as the one character it is
        Assert.Equal(RuntimeNodeKind.Other, Under(culture, () => Node("""{ "alias": "a", "kind": "STATI\u0130CSITE" }""").Kind));

        Assert.Equal(RuntimeEdgeKind.Origin, Under(culture, () => Edge("""{ "id": "a-to-b", "from": "a", "to": "b", "kind": "ORIGIN" }""").Kind));
        Assert.Equal(RuntimeEdgeKind.Public, Under(culture, () => Edge("""{ "id": "a-to-b", "from": "a", "to": "b", "kind": "PUBLIC" }""").Kind));
        Assert.Equal(RuntimeEdgeKind.Dependency, Under(culture, () => Edge("""{ "id": "a-to-b", "from": "a", "to": "b", "kind": "DEPENDENCY" }""").Kind));

        // and the architecture the kind opens stands or goes by the very same fold
        Assert.NotNull(Under(culture, () => Node("""{ "alias": "a", "kind": "CONTAINERAPP", "architecture": "https://demo.example.net/ui" }""").Architecture));
    }

    // ----- The readers of this file -----

    private static RuntimeIndex Index(string json)
    {
        var found = RuntimeManifestParser.ParseIndex(json);

        Assert.True(found.IsValid, string.Join(' ', found.Errors));
        Assert.Empty(found.Errors);
        return found.Value!;
    }

    private static RuntimeManifest Manifest(string json)
    {
        var found = RuntimeManifestParser.ParseManifest(json);

        Assert.True(found.IsValid, string.Join(' ', found.Errors));
        Assert.Empty(found.Errors);
        return found.Value!;
    }

    private static RuntimeIndexEntry Entry(string entry) => Assert.Single(Index($$"""{ "environments": [ {{entry}} ] }""").Environments);

    private static RuntimeNode Node(string node) => Assert.Single(Manifest($$"""{ "environment": "tdd", "nodes": [ {{node}} ] }""").Nodes);

    private static RuntimeEdge Edge(string edge) =>
        Assert.Single(Manifest($$"""{ "environment": "tdd", "edges": [ {{edge}} ] }""").Edges);

    private static RuntimeUnplaced Unplaced(string entry) =>
        Assert.Single(Manifest($$"""{ "environment": "tdd", "unplaced": [ {{entry}} ] }""").Unplaced!);

    /// <summary>A word of the file as the file wrote it: the quotes of JSON around it, and nothing of its own room
    /// taken away, since the reader trims a word before it weighs anything else of it.</summary>
    private static string Json(string text) => JsonSerializer.Serialize(text);

    /// <summary>The room a deployment's own editor leaves about a word of the file.</summary>
    private static string Padded(string word) => $"  {word}  ";

    /// <summary>Asking the reader for a file under another reader - and handing the thread's own culture back
    /// afterwards, whatever the asking did.</summary>
    private static T Under<T>(CultureInfo culture, Func<T> what)
    {
        ArgumentNullException.ThrowIfNull(culture);
        ArgumentNullException.ThrowIfNull(what);

        var reader = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = culture;
            return what();
        }
        finally
        {
            CultureInfo.CurrentCulture = reader;
        }
    }
}
