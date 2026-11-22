using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>
/// The boundaries, the null/empty cases and the fixed values of <see cref="RuntimeManifestParser" /> that its other
/// files leave open. <see cref="RuntimeManifestParserEdgeCaseTests" /> pins how both readers refuse a file that is
/// nothing, no JSON or no object, the shape of the index's one required member and of a manifest's four lists, the
/// words a node, a region and a frame fall back to, the two public limits of the unplaced list read through a list of
/// nothing but names, the health path's two hundredth character, a box's two hundred ids and an architecture's four
/// hundred; <see cref="RuntimeManifestParserTests" /> pins the deployment's own sample files,
/// <see cref="RuntimeIndexFrameAndParseResultTests" /> the parse result, <see cref="RuntimeBoxLinkTests" /> the links
/// of a box that is refused whole, <see cref="UnplacedTests" /> the entries of the unplaced list with their address
/// rules, <see cref="DatabaseViewTests" /> the two files of a database view and <see cref="ApplicationViewTests" />
/// what kinds may carry an architecture.
/// <para>
/// What stands here is what those leave unweighed, and it is all of it the reader's own arithmetic and the order of
/// its own work: a file name made from an environment's name that is no file name, the two members of an entry said
/// in their own order rather than the file's; that a box with no alias is refused once and its address never weighed,
/// that the manifest's own word is named before every list of it, and that the words of the four lists are kept
/// trimmed; that a link's key stands in the case the file spelled it and its address as the file trimmed it, while a
/// box's address keeps the user name and the query an unplaced address may not; that the health path's rule is the
/// character and not the kind, and an architecture's gate the kind the reader made and not the word it saw; and of
/// the unplaced list, that its two thousand of an address is weighed on the trimmed word, that its most entries are
/// the entries kept and not the entries read, and that its texts are trimmed before they are cut.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: both readers take a string of JSON and hand back
/// a record, and every address, file name, id and number below is one the test brought.
/// </para>
/// </summary>
public class RuntimeManifestParserBoundaryEdgeCaseTests
{
    /// <summary>The beginning of the addresses the tests below pad to an exact length.</summary>
    private const string AnAddressRoot = "https://demo.example.net/";

    /// <summary>An address with everything an unplaced resource may not bring behind its name: a user, a query and a
    /// fragment. A box is checked at it and a link leads to it all the same.</summary>
    private const string AnAddressOfAUser = "https://user:secret@ca-ui.example.net/_health?code=7#top";

    /// <summary>An entry of the unplaced list with a type written and no name at all: the reader skips it, and so it
    /// takes no place of the list it is written in.</summary>
    private const string Nameless = """{ "type": "Vault" }""";

    // ----- The index: the two files an entry does not name itself -----

    /// <summary>
    /// The two files of an entry are weighed by the same rule whether the file wrote them or the reader made them
    /// out of the environment's own name - and a name that is itself no file name makes both of them refuse, a slash,
    /// a leading dot and a colon each in their turn. The two members are named in the reader's order, the manifest
    /// before the diagram, whichever way the file wrote them.
    /// </summary>
    [Fact]
    public void AFileNameMadeOutOfTheEnvironmentOwnNameIsWeighedByTheSameRule()
    {
        var result = RuntimeManifestParser.ParseIndex("""
            { "environments": [
                { "name": "a/b", "svg": "../secret.svg", "manifest": "https://elsewhere.example.net/tdd.json" },
                { "name": ".hidden" },
                { "name": "tdd:latest" } ] }
            """);

        Assert.False(result.IsValid);
        Assert.Null(result.Value);
        Assert.Equal(
            [
                "environments[0].manifest: not a file name in runtime/.",
                "environments[0].svg: not a file name in runtime/.",
                "environments[1].manifest: not a file name in runtime/.",
                "environments[1].svg: not a file name in runtime/.",
                "environments[2].manifest: not a file name in runtime/.",
                "environments[2].svg: not a file name in runtime/.",
            ],
            result.Errors);
    }

    /// <summary>The readers read what they know and say nothing about the rest: a member of either file that no code
    /// looks at is neither kept nor refused, and an entry's <c>database</c> that is no object is as absent as one
    /// nobody wrote (the rule <see cref="DatabaseViewTests" /> pins for the other shapes of it).</summary>
    [Fact]
    public void AMemberNobodyReadsIsNoErrorOfEitherReader()
    {
        var index = Index("""
            { "environments": [ { "name": "tdd", "manifest": "tdd.json", "svg": "tdd.svg",
                                  "database": "gone", "later": 1, "notes": null } ],
              "plantuml": "1.2026.8", "written": "2026-10-12T09:00:00Z" }
            """);

        Assert.Equal([new RuntimeIndexEntry("tdd", "tdd.json", "tdd.svg")], index.Environments);
        Assert.Equal("1.2026.8", index.PlantUml);

        var manifest = Manifest("""
            { "environment": "tdd", "written": "2026-10-12T09:00:00Z",
              "nodes": [ { "alias": "app_ui", "kind": "webapp", "type": "Microsoft.Web/sites", "count": 2, "names": [ "app_ui" ] } ],
              "regions": [ { "alias": "region_data", "role": "primary" } ],
              "frames": [ { "alias": "plan_primary", "kind": "plan", "note": 7 } ],
              "edges": [ { "id": "app_ui-to-region_data", "from": "app_ui", "to": "region_data", "weight": 3 } ],
              "unplaced": [ { "name": "kv-demo", "tags": { "env": "tdd" } } ] }
            """);

        Assert.Equal(new RuntimeNode("app_ui", RuntimeNodeKind.WebApp, "app_ui"), Assert.Single(manifest.Nodes));
        var region = Assert.Single(manifest.Regions);

        Assert.Equal("region_data", region.Alias);
        Assert.Equal("region_data", region.Name);
        Assert.Empty(region.Roles);
        Assert.Null(region.Links);
        Assert.Equal(new RuntimeFrame("plan_primary", "plan", "plan_primary"), Assert.Single(manifest.Frames!));
        Assert.Equal(new RuntimeEdge("app_ui-to-region_data", "app_ui", "region_data", RuntimeEdgeKind.Other), Assert.Single(manifest.Edges));
        Assert.Equal([new RuntimeUnplaced("kv-demo")], manifest.Unplaced!);
    }

    // ----- The manifest: the order of its work, and the words that open it -----

    /// <summary>The environment is the first thing the reader looks for and the first thing it names, before every
    /// problem of the four lists - and its own message is the one written before the others, not after them, since a
    /// file of no environment says all of its problems at once.</summary>
    [Fact]
    public void TheEnvironmentThatIsNoWordIsNamedBeforeEveryProblemOfEveryList()
    {
        var result = RuntimeManifestParser.ParseManifest("""
            { "environment": 7,
              "nodes": [ { "kind": "webapp" } ],
              "regions": [ { "name": "westus3" } ],
              "frames": [ { "kind": "plan" } ],
              "edges": [ { "id": "a-to-b", "from": "a" } ] }
            """);

        Assert.False(result.IsValid);
        Assert.Null(result.Value);
        Assert.Equal(
            [
                "environment: missing or empty.",
                "nodes[0].alias: missing or empty.",
                "regions[0].alias: missing or empty.",
                "frames[0].alias: missing or empty.",
                "edges[0]: id, from and to are required.",
            ],
            result.Errors);
    }

    /// <summary>A box with no alias is refused by that alone: the reader asks for the alias first and hands the
    /// element back as nothing, so the address, the health path and the architecture of the same element are never
    /// weighed and add no message to the file.</summary>
    [Fact]
    public void ABoxWithNoAliasIsRefusedOnceAndItsAddressIsNeverWeighed()
    {
        var result = RuntimeManifestParser.ParseManifest("""
            { "environment": "tdd", "nodes": [ { "kind": "webapp", "url": "ftp://demo.example.net/x",
                "healthPath": "health", "architecture": "nonsense" } ] }
            """);

        Assert.False(result.IsValid);
        Assert.Equal(["nodes[0].alias: missing or empty."], result.Errors);
    }

    /// <summary>The word that opens an element is trimmed in all four lists, and so are the two a relationship is
    /// drawn between: an alias with spaces about it is the alias of the box the SVG names, and a relationship of
    /// three words with spaces about them is one line of the three words without.</summary>
    [Fact]
    public void EveryAliasAndEveryEndOfARelationshipIsKeptTrimmed()
    {
        var manifest = Manifest("""
            { "environment": "tdd",
              "nodes": [ { "alias": "  app_ui  " } ],
              "regions": [ { "alias": "  region_data  " } ],
              "frames": [ { "alias": "  sub  " } ],
              "edges": [ { "id": "  a-to-b  ", "from": "  browser  ", "to": "  app_ui  " } ] }
            """);

        Assert.Equal("app_ui", Assert.Single(manifest.Nodes).Alias);
        Assert.Equal("region_data", Assert.Single(manifest.Regions).Alias);
        Assert.Equal("sub", Assert.Single(manifest.Frames!).Alias);
        Assert.Equal(new RuntimeEdge("a-to-b", "browser", "app_ui", RuntimeEdgeKind.Other), Assert.Single(manifest.Edges));
    }

    // ----- The addresses a box is checked at and the links its name leads to -----

    /// <summary>A box's address is the address a deployment published, and the reader asks only that it be absolute
    /// and of the two schemes: a user name, a query and a fragment stand in it as written, where the same text as the
    /// address of an unplaced resource is left out whole (<see cref="UnplacedTests" />). A link keeps them too: only
    /// the scheme is weighed there.</summary>
    [Fact]
    public void TheAddressOfABoxKeepsTheUserInfoAndTheQueryAnUnplacedAddressMayNot()
    {
        var node = Node($$"""{ "alias": "ca_ui", "kind": "containerapp", "url": "{{AnAddressOfAUser}}" }""");
        var link = Frame($$"""{ "alias": "f", "links": { "portal": "{{AnAddressOfAUser}}" } }""").Links!;

        Assert.NotNull(node.Url);
        Assert.Equal("user:secret", node.Url.UserInfo);
        Assert.Equal("?code=7", node.Url.Query);
        Assert.Equal("#top", node.Url.Fragment);
        Assert.Equal("/_health", node.Url.AbsolutePath);
        Assert.Equal("ca-ui.example.net", node.Url.Host);
        Assert.Equal(AnAddressOfAUser, link[LinkSet.Portal]!.AbsoluteUri);

        var entry = Unplaced($$"""{ "name": "ca-x", "url": "{{AnAddressOfAUser}}" }""");

        Assert.Null(entry.Site);
        Assert.Null(entry.Portal);
    }

    /// <summary>A link is a key of the file's own and an address: the key stands in the case the file spelled it, so
    /// the <c>Portal</c> of one deployment's copy is no answer to a page that asks <c>portal</c>; the address is
    /// trimmed before it is weighed, and everything the rule does not let stand - a word that is no text, a word of
    /// nothing, an address of another scheme and a member that is another thing - goes, while the map beside it keeps
    /// what it has.</summary>
    [Fact]
    public void ALinkKeepsItsKeyAsTheFileSpelledItAndItsAddressAsTheFileTrimmedIt()
    {
        var links = Frame("""
            { "alias": "sub", "kind": "subscription",
              "links": { "Portal": "  https://portal.example.net/#sub  ", "portal": "https://portal.example.net/other",
                         "site": "https://demo.example.net/site", "junk": 7, "note": "   ",
                         "nested": { "a": "https://demo.example.net" }, "plain": "http://demo.example.net" } }
            """).Links!;

        Assert.Equal(3, links.Count);
        Assert.Equal("https://portal.example.net/#sub", links["Portal"]!.AbsoluteUri);
        Assert.Equal("https://portal.example.net/other", links[LinkSet.Portal]!.AbsoluteUri);
        Assert.Equal("https://demo.example.net/site", links["site"]!.AbsoluteUri);
        Assert.Null(links["junk"]);
        Assert.Null(links["note"]);
        Assert.Null(links["nested"]);
        Assert.Null(links["plain"]);
    }

    /// <summary>A member of links that names no https address is no links at all rather than an empty hold of them:
    /// the map of nothing, a map of only refused addresses, and the member worn as another thing of JSON are the one
    /// null, and none of them a word said about the file.</summary>
    [Theory]
    [InlineData("{}")]
    [InlineData("""{ "plain": "http://demo.example.net" }""")]
    [InlineData("""{ "note": "", "junk": 7 }""")]
    [InlineData("[]")]
    [InlineData("null")]
    public void LinksThatNameNoHttpsAddressAreNoLinksAtAll(string links) =>
        Assert.Null(Frame($$"""{ "alias": "f", "links": {{links}} }""").Links);

    // ----- The two members a node carries that the reader weighs by rule -----

    /// <summary>The health path is a rule of characters and nothing else: the reader asks no question of the kind, so
    /// a database, a person, a box of a kind nobody has heard of and a box that names no kind at all keep the path
    /// the deployment wrote for them as an app does.</summary>
    [Theory]
    [InlineData("containerapp")]
    [InlineData("webapp")]
    [InlineData("sql")]
    [InlineData("person")]
    [InlineData("servicebus")]
    [InlineData(null)]
    public void AHealthPathIsReadOfEveryKindSinceTheReaderAsksNothingOfIt(string? kind) =>
        Assert.Equal("/_healthcheck", Node($$"""{ "alias": "a", "healthPath": "/_healthcheck"{{KindWord(kind)}} }""").HealthPath);

    /// <summary>The rule lets a letter of the ASCII alphabet, a digit and the five of <c>/ . _ ~ -</c> stand, and a
    /// path that is a word of those with one other character in it is left out whole - neither cut nor refused. A
    /// percent, a plus, a colon, a space, a vertical bar, a semicolon, an at, a brace, a backslash and a letter out
    /// of the alphabet are each of them the one path that goes.</summary>
    [Theory]
    [InlineData("/a%20")]
    [InlineData("/a+b")]
    [InlineData("/a:b")]
    [InlineData("/hea lth")]
    [InlineData("/a|b")]
    [InlineData("/a;b")]
    [InlineData("/a@b")]
    [InlineData("/a{b}")]
    [InlineData("/a\\b")]
    [InlineData("/café")]
    public void AHealthPathOfACharacterTheRuleLetsNotStandIsLeftOutAndSaysNothing(string path)
    {
        var json = JsonSerializer.Serialize(path);
        var result = RuntimeManifestParser.ParseManifest($$"""
            { "environment": "tdd", "nodes": [ { "alias": "a", "kind": "containerapp", "healthPath": {{json}} } ] }
            """);

        Assert.True(result.IsValid, string.Join(' ', result.Errors));
        Assert.Null(Assert.Single(result.Value!.Nodes).HealthPath);
    }

    /// <summary>The gate is the kind the reader made of the word, not the word: a deployment that writes
    /// <c>WEBAPP</c> gets its architecture kept as surely as one that writes <c>webapp</c>, and a person is no
    /// deployable, the sixth kind the rule refuses beside the five that
    /// <see cref="ApplicationViewTests" /> names.</summary>
    [Fact]
    public void TheArchitectureStandsOnTheKindTheReaderMadeAndNotTheWordTheFileWrote()
    {
        var node = Node("""{ "alias": "a", "kind": " WEBAPP ", "deployable": "ui", "architecture": "https://demo.example.net/ui" }""");

        Assert.Equal(RuntimeNodeKind.WebApp, node.Kind);
        Assert.Equal(new Uri("https://demo.example.net/ui"), node.Architecture);

        Assert.Null(Node("""{ "alias": "a", "kind": "person", "deployable": "ui", "architecture": "https://demo.example.net/ui" }""").Architecture);
    }

    // ----- The unplaced list: the three limits the reader sets itself -----

    /// <summary>An address of an entry of the unplaced list is kept to two thousand characters and the entry beside it
    /// read all the same: one under the limit, the limit, and the one over it that goes whole - and the three places
    /// an address comes from, <c>links.portal</c>, <c>links.site</c> and <c>url</c>, are weighed by the one rule.</summary>
    [Theory]
    [InlineData(1999, true)]
    [InlineData(2000, true)]
    [InlineData(2001, false)]
    public void AnAddressOfAnUnplacedEntryIsKeptToItsTwoThousandthCharacter(int length, bool kept)
    {
        var address = AnAddressRoot + new string('a', length - AnAddressRoot.Length);
        Assert.Equal(length, address.Length);

        var portal = Unplaced($$"""{ "name": "kv", "links": { "portal": "{{address}}" } }""");
        var site = Unplaced($$"""{ "name": "kv", "links": { "site": "{{address}}" } }""");
        var url = Unplaced($$"""{ "name": "kv", "url": "{{address}}" }""");
        var expected = kept ? address : null;

        Assert.Equal(expected, portal.Portal?.AbsoluteUri);
        Assert.Equal(expected, site.Site?.AbsoluteUri);
        Assert.Equal(expected, url.Site?.AbsoluteUri);
        Assert.Equal("kv", url.Name);
    }

    /// <summary>And the length is weighed on the word once it is trimmed: an address of the limit with spaces about
    /// it, longer than the limit as the file wrote it and the limit itself as the reader read it, stands whole.</summary>
    [Fact]
    public void TheLengthOfAnUnplacedAddressIsWeighedOnceTheWordIsTrimmed()
    {
        var address = AnAddressRoot + new string('a', 2000 - AnAddressRoot.Length);

        Assert.Equal(address, Unplaced($$"""{ "name": "kv", "url": "  {{address}}  " }""").Site!.AbsoluteUri);
    }

    /// <summary>The most entries the reader keeps is the most it keeps of the entries it kept, not of the entries it
    /// read: an entry with no name is skipped before the count is asked, so the five hundred of them written between
    /// the names take no place of the list - and the list ends at the most, so the names that come after it are
    /// never read at all.</summary>
    [Fact]
    public void TheMostUnplacedAreTheEntriesKeptAndNotTheEntriesRead()
    {
        var most = RuntimeManifestParser.MostUnplaced;
        var interleaved = string.Join(',', Enumerable.Range(0, most * 2).Select(number => number % 2 == 0 ? $$"""{ "name": "r{{number}}" }""" : Nameless));
        var list = Manifest($$"""{ "environment": "tdd", "unplaced": [{{interleaved}}] }""").Unplaced!;

        Assert.Equal(most, list.Count);
        Assert.Equal("r0", list[0].Name);
        Assert.Equal($"r{most * 2 - 2}", list[^1].Name);

        var written = Enumerable.Range(0, most).Select(number => $$"""{ "name": "kept-{{number}}" }""")
            .Concat(Enumerable.Repeat(Nameless, 40))
            .Concat(Enumerable.Range(0, 5).Select(number => $$"""{ "name": "late-{{number}}" }"""));

        var capped = Manifest($$"""{ "environment": "tdd", "unplaced": [{{string.Join(',', written)}}] }""").Unplaced!;

        Assert.Equal(most, capped.Count);
        Assert.Equal($"kept-{most - 1}", capped[^1].Name);
        Assert.DoesNotContain(capped, entry => entry.Name.StartsWith("late-", StringComparison.Ordinal));
    }

    /// <summary>A text of an entry is trimmed and then cut, in that order: the spaces a deployment pads a name with
    /// cost the name nothing of its own characters, and an entry of nothing but spaces has no name and goes. The cut
    /// keeps the first three hundred characters of the word, so what stands at its beginning stands.</summary>
    [Fact]
    public void ATextOfAnUnplacedEntryIsTrimmedBeforeItIsCutAtTheLongestOneKept()
    {
        var longest = RuntimeManifestParser.LongestUnplacedText;
        var word = new string('x', longest);

        var entry = Unplaced($$"""
            { "name": "  {{word}}  ", "type": "zz{{word}}", "group": " {{new string('y', longest - 2)}} ", "words": "   " }
            """);

        Assert.Equal(word, entry.Name);
        Assert.Equal($"zz{new string('x', longest - 2)}", entry.Type);
        Assert.Equal(new string('y', longest - 2), entry.Group);
        Assert.Null(entry.Words);
        Assert.Equal(longest, entry.Name.Length);
    }

    /// <summary>An entry's ids are the ids of a box, read by the one rule and cut by both of its own: two hundred of
    /// them at the most, in the order the listing wrote them, each of them cut at twice the longest text - so an
    /// entry of two hundred and one ids of seven hundred characters ends with two hundred of six hundred.</summary>
    [Fact]
    public void TheIdsOfAnUnplacedEntryAreCappedAndCutByTheRulesOfABox()
    {
        var written = string.Join(',', Enumerable.Range(0, 201).Select(number => $$""" "/i{{number}}" """));
        var capped = Unplaced($$"""{ "name": "kv", "ids": [{{written}}] }""");

        Assert.Equal(200, capped.Ids!.Count);
        Assert.Equal("/i0", capped.Ids[0]);
        Assert.Equal("/i199", capped.Ids[^1]);

        var cut = Unplaced($$"""{ "name": "kv", "ids": [ "/s/{{new string('a', 700 - 3)}}", 7, "", "  /kept  " ] }""");

        Assert.Equal(["/s/" + new string('a', RuntimeManifestParser.LongestUnplacedText * 2 - 3), "/kept"], cut.Ids!);
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

    private static RuntimeNode Node(string node) =>
        Assert.Single(Manifest($$"""{ "environment": "tdd", "nodes": [ {{node}} ] }""").Nodes);

    /// <summary>The <c>kind</c> of a box, written after the members a test names and nowhere at all where the test
    /// brings none, since the member is optional and the reader asks nothing of a box that wears no kind.</summary>
    private static string KindWord(string? kind) =>
        kind is null ? string.Empty : $", \"kind\": \"{kind}\"";

    private static RuntimeFrame Frame(string frame) =>
        Assert.Single(Manifest($$"""{ "environment": "tdd", "frames": [ {{frame}} ] }""").Frames!);

    private static RuntimeUnplaced Unplaced(string entry) =>
        Assert.Single(Manifest($$"""{ "environment": "tdd", "unplaced": [ {{entry}} ] }""").Unplaced!);
}
