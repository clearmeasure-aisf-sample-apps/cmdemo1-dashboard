namespace Dashboard.Tests;

/// <summary>
/// What the <see cref="RuntimeManifestParser" /> does with the <em>JSON itself</em>, which its other files leave open.
/// <see cref="RuntimeManifestParserEdgeCaseTests" /> pins how both readers refuse a file that is nothing (the empty
/// word included), no JSON or no object, the shape of the index's one required member and of a manifest's four lists,
/// the words a node, a region and a frame fall back to, the two public limits <see
/// cref="RuntimeManifestParser.LongestUnplacedText" /> and <see cref="RuntimeManifestParser.MostUnplaced" /> at 299,
/// 300, 301, 499, 500 and 501, the health path's two hundredth character, a box's two hundred ids and an
/// architecture's four hundred; <see cref="RuntimeManifestParserBoundaryEdgeCaseTests" /> pins the order of the
/// reader's own work, the trimming of every word it weighs, that the most unplaced entries are the entries kept, and
/// an unplaced address weighed at its two thousandth character once trimmed;
/// <see cref="RuntimeManifestParserTests" /> pins the deployment's own sample files,
/// <see cref="RuntimeIndexFrameAndParseResultTests" /> every part of <see cref="RuntimeParseResult{T}" /> - the value,
/// the messages and <see cref="RuntimeParseResult{T}.IsValid" /> - <see cref="RuntimeBoxLinkTests" /> and
/// <see cref="UnplacedTests" /> the links of a box and the entries of the unplaced list,
/// <see cref="DatabaseViewTests" /> the two files of a database view and <see cref="ApplicationViewTests" /> which
/// kinds may carry an architecture and the address parts that refuse one.
/// <para>
/// What stands here is the file as a file, and the values the readers make out of the words in it: the file that is no
/// text for either reader to open; the member written
/// twice, which both readers read as the last of the two (and read last even when the last is the one that refuses
/// the file); the sixty-four nestings <c>System.Text.Json</c> opens and the sixty-fifth it refuses whole; a member
/// name weighed in its own case and with no room about it while every word of the file is trimmed - unbroken space
/// and a line separator included; an escape decoded into a character before the rule of a file name, of a kind, of a
/// health path or of an address weighs it; an address kept as the <see cref="System.Uri" /> made it, host lowered, dot
/// segments walked, a default port and a root path written for the file that had neither, a space escaped, and a
/// question mark of nothing still a query of one character; a link of a key written twice keeping the last address
/// the rule let stand, and a key of nothing or of spaces being a key of itself; a priority of <c>-0</c> being zero and
/// the same value written with a mark or an exponent being no number; a box's ids capped by the elements that stand
/// rather than the elements written; and the site of an unplaced entry falling back to its <c>url</c> where its own
/// link is refused or is no map at all.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: both readers take a string of JSON and hand back
/// a record, and every address, file name, id, escape and number below is one the test brought.
/// </para>
/// </summary>
public class RuntimeManifestParserJsonEdgeCaseTests
{
    // ----- The file as a file: no file, a member written twice, and a file nested too deep to open -----

    /// <summary>The <c>null</c> a caller that read nothing hands over is the empty file to both readers, as the word of
    /// no letters and the word of nothing but room are: one message, said before either reader looks for a member of
    /// it - which is what <see cref="RuntimeManifestParserEdgeCaseTests" /> pins of <c>""</c> and of a space, the
    /// <c>null</c> alone never named.</summary>
    [Fact]
    public void AFileThatIsNoTextAtAllIsTheEmptyFileToBothReaders()
    {
        foreach (var nothing in new string?[] { null, string.Empty, "   ", "\t\r\n" })
        {
            Assert.Equal(["The file is empty."], RuntimeManifestParser.ParseIndex(nothing).Errors);
            Assert.Equal(["The file is empty."], RuntimeManifestParser.ParseManifest(nothing).Errors);
        }
    }

    /// <summary>A file that names one member twice is not a file of two of them, and not an error either: the reader
    /// asks <c>JsonElement</c> for the member by its name and gets the last of the two, so the first never weighed
    /// anything - the index's environments and its version, the manifest's environment and its lists, and the parts
    /// of one element all the same.</summary>
    [Fact]
    public void AMemberWrittenTwiceIsReadAsTheLastOfTheTwoByBothReaders()
    {
        var index = Index("""
            { "environments": [ { "name": "tdd" } ], "plantuml": "1.1",
              "environments": [ { "name": "uat" } ], "plantuml": "1.2026.8" }
            """);

        Assert.Equal(new RuntimeIndexEntry("uat", "uat.json", "uat.svg"), Assert.Single(index.Environments));
        Assert.Equal("1.2026.8", index.PlantUml);

        var manifest = Manifest("""
            { "environment": "tdd", "environment": "uat",
              "nodes": [ { "alias": "a" } ], "nodes": [ { "alias": "b" } ] }
            """);

        Assert.Equal("uat", manifest.Environment);
        Assert.Equal("b", Assert.Single(manifest.Nodes).Alias);

        Assert.Equal("b", Node("""{ "alias": "a", "alias": "b", "name": "n", "name": "m" }""").Alias);
        Assert.Equal("m", Node("""{ "alias": "a", "alias": "b", "name": "n", "name": "m" }""").Name);
    }

    /// <summary>And the last of two is weighed even when it is the one that refuses the file: an environment's list
    /// written after a good one as a word of text is no list, and the index is nothing - which is the same message
    /// <see cref="RuntimeManifestParserEdgeCaseTests" /> pins for a file that never had a good one.</summary>
    [Fact]
    public void TheLastOfTwoMembersIsWeighedEvenWhenItIsTheOneThatRefusesTheFile()
    {
        var index = RuntimeManifestParser.ParseIndex("""
            { "environments": [ { "name": "tdd" } ], "environments": "gone" }
            """);

        Assert.False(index.IsValid);
        Assert.Equal(["environments: missing or not an array."], index.Errors);

        var manifest = RuntimeManifestParser.ParseManifest("""
            { "environment": "tdd", "nodes": [ { "alias": "a" } ], "nodes": 7 }
            """);

        Assert.False(manifest.IsValid);
        Assert.Equal(["nodes: not an array."], manifest.Errors);
    }

    /// <summary>A relationship of two priorities is a relationship of the last of them, refusal included: a priority
    /// written after a good one as a word of text or as nothing at all stops the manifest being read at all, which is
    /// what <see cref="RuntimeManifestParserEdgeCaseTests" /> pins of a file that wrote no good one before it.</summary>
    [Theory]
    [InlineData("7", 7)]
    [InlineData("\"7\"", null)]
    [InlineData("null", null)]
    public void ARelationshipWrittenWithTwoPrioritiesKeepsTheLastOfThem(string last, int? kept)
    {
        var json = $$"""{ "environment": "tdd", "edges": [ { "id": "a-to-b", "from": "a", "to": "b", "priority": 1, "priority": {{last}} } ] }""";

        if (kept is null)
        {
            Assert.Throws<InvalidOperationException>(() => RuntimeManifestParser.ParseManifest(json));
            return;
        }

        Assert.Equal(kept, Assert.Single(Manifest(json).Edges).Priority);
    }

    /// <summary>The depth <c>System.Text.Json</c> opens is sixty-four nestings of a file and no more, and both readers
    /// weigh it before they look for a member of it: sixty-two brackets written within the environments list make the
    /// sixty-fourth of them, which opens - and both readers are then still naming a problem of the file's own contents,
    /// each its own. One bracket more is the whole file refused by the one message of the open file, which the two of
    /// them say in the same words about the same file, at the place of the sixty-third of them.</summary>
    [Fact]
    public void AFileNestedSixtyFiveDeepIsNoJsonForEitherReader()
    {
        var asFarAsItGoes = DeeplyNested(62);

        Assert.Equal(["environments[0].name: missing or empty."], RuntimeManifestParser.ParseIndex(asFarAsItGoes).Errors);
        Assert.Equal(["environment: missing or empty."], RuntimeManifestParser.ParseManifest(asFarAsItGoes).Errors);

        var oneDeeper = DeeplyNested(63);

        Assert.Equal(["The file is not valid JSON (line 1, position 82)."], RuntimeManifestParser.ParseIndex(oneDeeper).Errors);
        Assert.Equal(["The file is not valid JSON (line 1, position 82)."], RuntimeManifestParser.ParseManifest(oneDeeper).Errors);
    }

    // ----- The name of a member, and the word of a value -----

    /// <summary>A member is found by its name exactly: another case of it, or a space about it, is another member
    /// nobody wrote, so the file says the one message of the member that is missing - while every word <em>of</em> a
    /// member is trimmed before it is weighed (<see cref="RuntimeManifestParserBoundaryEdgeCaseTests" />).</summary>
    [Theory]
    [InlineData("""{ "Environment": "tdd" }""", "environment: missing or empty.")]
    [InlineData("""{ "ENVIRONMENT": "tdd", "nodes": [ { "alias": "a" } ] }""", "environment: missing or empty.")]
    [InlineData("""{ "environment ": "tdd", "nodes": [ { "alias": "a" } ] }""", "environment: missing or empty.")]
    [InlineData("""{ " environment": "tdd", "nodes": [ ] }""", "environment: missing or empty.")]
    public void AMemberOfTheManifestIsFoundByItsNameExactly(string json, string message)
    {
        var result = RuntimeManifestParser.ParseManifest(json);

        Assert.False(result.IsValid);
        Assert.Equal([message], result.Errors);
    }

    /// <summary>The same of an index: no <c>environments</c> of that name is no index at all, and no <c>name</c> of
    /// that name is no entry - however the deployment capitalized the copy of the file it annotated.</summary>
    [Fact]
    public void AMemberOfTheIndexIsFoundByItsNameExactly()
    {
        Assert.Equal(
            ["environments: missing or not an array."],
            RuntimeManifestParser.ParseIndex("""{ "Environments": [ { "name": "tdd" } ], "PlantUml": "1.2026.8" }""").Errors);
        Assert.Equal(
            ["environments[0].name: missing or empty."],
            RuntimeManifestParser.ParseIndex("""{ "environments": [ { "Name": "tdd", "Manifest": "own-tdd.json" } ] }""").Errors);
    }

    /// <summary>The room about a word is whatever <c>string.Trim</c> calls room: an unbroken space (U+00A0) and a line
    /// separator (U+2028) are as much trimmed as a space, so a name worn between them is the name without them - and
    /// the files of that entry are named off the trimmed name. A word of nothing but them is no word at all.</summary>
    [Fact]
    public void AWordPaddedWithTheSpaceNoLetterBringsIsTrimmedLikeOnePaddedWithSpaces()
    {
        var index = Index("""{ "environments": [ { "name": "\u00A0tdd\u2028" } ], "plantuml": "\u00A0\u00A0" }""");

        Assert.Equal(new RuntimeIndexEntry("tdd", "tdd.json", "tdd.svg"), Assert.Single(index.Environments));
        Assert.Null(index.PlantUml);

        Assert.Equal("app_ui", Node("""{ "alias": "\u2028app_ui\u00A0", "name": "\u00A0\u00A0" }""").Alias);
        Assert.Equal("app_ui", Node("""{ "alias": "\u2028app_ui\u00A0", "name": "\u00A0\u00A0" }""").Name);
        Assert.Null(Node("""{ "alias": "a", "deployable": "\u2028\u2028", "role": "\u00A0 PRIMARY \u00A0" }""").Deployable);
        Assert.Equal("primary", Node("""{ "alias": "a", "deployable": "\u2028\u2028", "role": "\u00A0 PRIMARY \u00A0" }""").Role);
    }

    // ----- The escape of the file becomes a character before any rule weighs the word -----

    /// <summary>A file name is weighed on the characters the escape makes and not on the six of the escape: the slash
    /// of <c>runtime/</c> written as <c>\u002F</c> leaves the directory as surely as the slash itself, and a colon, a
    /// backslash and a leading dot written the other way round are the three other refusals.</summary>
    [Theory]
    [InlineData(@"a\u002Fb.json")]
    [InlineData(@"\u002Ehidden.json")]
    [InlineData(@"tdd\u003Alatest.json")]
    [InlineData(@"sub\u005Ctdd.json")]
    public void AFileNamedByAnEscapeOfTheFileIsWeighedOnceTheEscapeIsACharacter(string manifest)
    {
        Assert.Equal(
            ["environments[0].manifest: not a file name in runtime/."],
            RuntimeManifestParser.ParseIndex($$"""{ "environments": [ { "name": "tdd", "manifest": "{{manifest}}" } ] }""").Errors);
        Assert.Equal(
            ["environments[0].svg: not a file name in runtime/."],
            RuntimeManifestParser.ParseIndex($$"""{ "environments": [ { "name": "tdd", "svg": "{{manifest}}" } ] }""").Errors);
    }

    /// <summary>A kind is the word the escapes made, trimmed and lowered as every other word: a <c>w</c> written as
    /// <c>\u0077</c> is a web app's box, with its architecture kept and its kind named - and the same of a
    /// relationship's kind, which the reader lowers off the very same read.</summary>
    [Fact]
    public void AKindWrittenWithAnEscapeIsReadAsTheWordItMakes()
    {
        Assert.Equal(RuntimeNodeKind.WebApp, Node("""{ "alias": "a", "kind": "\u0077ebapp" }""").Kind);
        Assert.Equal(RuntimeNodeKind.WebApp, Node("""{ "alias": "a", "kind": "WEB\u0041PP" }""").Kind);
        Assert.Equal(RuntimeNodeKind.ContainerApp, Node("""{ "alias": "a", "kind": "\u0043ONTAINERAPP" }""").Kind);
        Assert.Equal(
            new Uri("https://demo.example.net/ui"),
            Node("""{ "alias": "a", "kind": "\u0077ebapp", "architecture": "https://demo.example.net/ui" }""").Architecture);

        Assert.Equal(RuntimeEdgeKind.Origin, Edge("""{ "id": "a-to-b", "from": "a", "to": "b", "kind": "\u006Frigin" }""").Kind);
        Assert.Equal(RuntimeEdgeKind.Other, Edge("""{ "id": "a-to-b", "from": "a", "to": "b", "kind": "\u006Frigins" }""").Kind);
    }

    /// <summary>The rule of a health path is asked of the characters the file made: a leading slash written as
    /// <c>\u002F</c> starts a path, a percent written as <c>\u0025</c> refuses one whole, two slashes in the middle of
    /// a path stand (the rule walks only the beginning of it) and the same two at its beginning, made of escapes,
    /// are the address of another host and no path.</summary>
    [Theory]
    [InlineData("""{ "alias": "a", "healthPath": "\u002F_health" }""", "/_health")]
    [InlineData("""{ "alias": "a", "healthPath": "/a\u002F\u002Fb" }""", "/a//b")]
    [InlineData("""{ "alias": "a", "healthPath": "/a\u0025b" }""", null)]
    [InlineData("""{ "alias": "a", "healthPath": "\u002F\u002Fx" }""", null)]
    public void AHealthPathIsWeighedOnceTheFileMadeItsCharacters(string node, string? kept) =>
        Assert.Equal(kept, Node(node).HealthPath);

    // ----- The address the readers keep is the address they made, not the word the file wrote -----

    /// <summary>A box's address is handed over as a <see cref="System.Uri" />, which rewrites it: the host and the scheme
    /// brought to lower case, a dot segment walked, a port of the scheme's own left out, a path of none written as
    /// <c>/</c>, a space of the file's escaped and an escape of the file made a character before any of that is asked
    /// - the deployment's copy of the file none the worse for any of it, and the page asking the address it asks
    /// either way.</summary>
    [Theory]
    [InlineData("https://DEMO.Example.Net/a/../b", "https://demo.example.net/b")]
    [InlineData("https://demo.example.net", "https://demo.example.net/")]
    [InlineData("https://demo.example.net:443/x", "https://demo.example.net/x")]
    [InlineData("https://demo.example.net/a b", "https://demo.example.net/a%20b")]
    [InlineData("HTTPS://demo.example.net/x", "https://demo.example.net/x")]
    [InlineData("http://demo.example.net:80/", "http://demo.example.net/")]
    [InlineData("""https://demo.example.net/\u0041 b""", "https://demo.example.net/A%20b")]
    public void TheAddressOfABoxIsKeptAsTheAddressItMadeAndNotAsTheFileWroteIt(string url, string kept) =>
        Assert.Equal(kept, Node($$"""{ "alias": "a", "kind": "webapp", "url": "{{url}}" }""").Url!.AbsoluteUri);

    /// <summary>The four hundred of an architecture are weighed on the word and its gate on the address the
    /// <see cref="System.Uri" /> made: the same address written in another case, on its own default port or with a dot
    /// segment in the way is the one address the page asks, while a question mark and a hash of nothing behind them are
    /// still a query and a fragment of a character apiece and refuse the member whole (the reader weighs the length of
    /// each, never its worth). An unplaced entry's two addresses are made the same way and are weighed by neither mark
    /// at all, and the site of one falls back to its <c>url</c> where its own link is refused or is no map.</summary>
    [Fact]
    public void AnArchitectureIsKeptAsTheAddressItMadeAndItsQueryAndFragmentWeighedOnIt()
    {
        Assert.Equal("https://demo.example.net/ui", Node("""{ "alias": "a", "kind": "webapp", "architecture": "HTTPS://DEMO.Example.Net/./ui" }""").Architecture!.AbsoluteUri);
        Assert.Equal("https://demo.example.net/ui", Node("""{ "alias": "a", "kind": "containerapp", "architecture": "https://demo.example.net:443/ui" }""").Architecture!.AbsoluteUri);
        Assert.Null(Node("""{ "alias": "a", "kind": "webapp", "architecture": "https://demo.example.net/ui?" }""").Architecture);
        Assert.Null(Node("""{ "alias": "a", "kind": "webapp", "architecture": "https://demo.example.net/ui#" }""").Architecture);
    }

    /// <summary>The two addresses of an unplaced entry are kept as the addresses made, in the three places they come
    /// from, and a question mark or a hash of nothing behind them stands in them (the reader of an entry weighs
    /// neither, only the scheme, the user name and the length); and a <c>site</c> that the rule refuses, a <c>site</c>
    /// no map holds and a map that is no map leave the entry's own <c>url</c> to stand as its site, which the reader
    /// asks after the map and never instead of it.</summary>
    [Fact]
    public void AnUnplacedAddressIsKeptAsTheAddressItMadeAndFallsBackToTheUrlTheMapRefused()
    {
        Assert.Equal("https://demo.example.net/b", Unplaced("""{ "name": "kv", "url": "HTTPS://DEMO.Example.Net/a/../b" }""").Site!.AbsoluteUri);
        Assert.Equal("https://demo.example.net/b", Unplaced("""{ "name": "kv", "links": { "portal": "https://demo.example.net:443/a/../b" } }""").Portal!.AbsoluteUri);
        Assert.Equal("https://demo.example.net/ui?", Unplaced("""{ "name": "kv", "url": "https://demo.example.net/ui?" }""").Site!.AbsoluteUri);

        var refused = Unplaced("""{ "name": "ca-x", "url": "https://ok.example.net/", "links": { "site": "http://clear.example.net/" } }""");

        Assert.Equal("https://ok.example.net/", refused.Site!.AbsoluteUri);
        Assert.Null(refused.Portal);

        var noMap = Unplaced("""{ "name": "ca-x", "url": "https://ok.example.net/", "links": [ "https://ok.example.net/" ] }""");

        Assert.Equal("https://ok.example.net/", noMap.Site!.AbsoluteUri);
        Assert.Null(noMap.Portal);

        var portalOnly = Unplaced("""{ "name": "ca-x", "url": "https://ok.example.net/", "links": { "portal": "https://portal.example.net/" } }""");

        Assert.Equal("https://portal.example.net/", portalOnly.Portal!.AbsoluteUri);
        Assert.Equal("https://ok.example.net/", portalOnly.Site!.AbsoluteUri);
    }

    // ----- The links of an element: a key written twice, a key of nothing and a key of spaces -----

    /// <summary>A key written twice in the map is one link, of the last address the rule let stand: an address the
    /// rule refuses never overwrites the one before it, since the reader asks the rule before it names the key -
    /// and a word of no letters, a number and the clear address of <c>http</c> are each refused in turn.</summary>
    [Theory]
    [InlineData("""{ "portal": "https://one.example.net/", "portal": "https://two.example.net/" }""", "https://two.example.net/")]
    [InlineData("""{ "portal": "https://one.example.net/", "portal": "http://two.example.net/" }""", "https://one.example.net/")]
    [InlineData("""{ "portal": "http://one.example.net/", "portal": "https://two.example.net/" }""", "https://two.example.net/")]
    [InlineData("""{ "portal": "https://one.example.net/", "portal": "   " }""", "https://one.example.net/")]
    [InlineData("""{ "portal": "https://one.example.net/", "portal": 7 }""", "https://one.example.net/")]
    public void ALinkWrittenTwiceKeepsTheLastAddressTheRuleLetsStand(string links, string kept)
    {
        var found = Frame($$"""{ "alias": "f", "links": {{links}} }""").Links!;

        Assert.Equal(kept, found[LinkSet.Portal]!.AbsoluteUri);
        Assert.Equal(1, found.Count);
    }

    /// <summary>A link's key is a member name and weighed as one: the empty key is a key of the empty word, and a key
    /// of spaces is no answer to a page that asks <c>portal</c> - which is the other side of the rule the tests above
    /// pin, that the address of a link is trimmed while its key never is.</summary>
    [Fact]
    public void ALinkKeyIsWeighedExactlyAndNeverTrimmed()
    {
        var empty = Frame("""{ "alias": "f", "links": { "": "https://demo.example.net/" } }""").Links!;

        Assert.Equal(1, empty.Count);
        Assert.Equal([string.Empty], empty.Keys.ToList());
        Assert.Equal("https://demo.example.net/", empty[string.Empty]!.AbsoluteUri);
        Assert.Null(empty[LinkSet.Portal]);

        var spaced = Frame("""{ "alias": "f", "links": { " portal ": "  https://portal.example.net/  " } }""").Links!;

        Assert.Equal("https://portal.example.net/", spaced[" portal "]!.AbsoluteUri);
        Assert.Null(spaced[LinkSet.Portal]);
    }

    // ----- The numbers of a relationship, and the two lists the reader counts -----

    /// <summary>A priority of <c>-0</c> is a zero, kept as one; the same nothing written as a fraction or with an
    /// exponent is no <c>int</c> at all and goes - the reader asks for a whole number of an <c>int</c> and nothing of
    /// what the number is worth.</summary>
    [Theory]
    [InlineData("-0", 0)]
    [InlineData("0", 0)]
    [InlineData("0.0", null)]
    [InlineData("-0.0", null)]
    [InlineData("0e0", null)]
    [InlineData("2.00", null)]
    public void APriorityIsKeptAsTheWholeIntTheFileWroteAndNotAsTheNumberItIsWorth(string priority, int? kept) =>
        Assert.Equal(kept, Edge($$"""{ "id": "a-to-b", "from": "a", "to": "b", "priority": {{priority}} }""").Priority);

    /// <summary>A box's ids are a list or nothing: the member written as a number, a word, a bool, an object, the
    /// empty list and no list at all are each the one <c>null</c>, with no word said about the file - the rule
    /// <see cref="UnplacedTests" /> pins for an entry of the unplaced list, read here of a box.</summary>
    [Theory]
    [InlineData("null")]
    [InlineData("7")]
    [InlineData("true")]
    [InlineData("\"/subscriptions/s\"")]
    [InlineData("{}")]
    [InlineData("[]")]
    public void TheIdsOfABoxThatAreNoListAreLeftOutAndSayNothing(string ids)
    {
        var manifest = Manifest($$"""{ "environment": "tdd", "nodes": [ { "alias": "res_1", "kind": "resource", "ids": {{ids}} } ] }""");

        Assert.Null(Assert.Single(manifest.Nodes).Ids);
    }

    /// <summary>The two hundred ids of a box are the two hundred that stand, not the two hundred first elements of
    /// the list: an element that is no word of its own takes no place, so a list of four hundred and two elements of
    /// which two hundred and one are ids ends at the two hundredth of them - where the unplaced list is capped by the
    /// entries kept the same way (<see cref="RuntimeManifestParserBoundaryEdgeCaseTests" />).</summary>
    [Fact]
    public void TheIdsOfABoxAreCappedByTheIdsThatStandAndNotTheElementsWritten()
    {
        var written = string.Join(',', Enumerable.Range(0, 201).Select(number => $$""" 7, "/i{{number}}" """));
        var ids = Node($$"""{ "alias": "res_1", "kind": "resource", "ids": [{{written}}] }""").Ids!;

        Assert.Equal(200, ids.Count);
        Assert.Equal("/i0", ids[0]);
        Assert.Equal("/i199", ids[^1]);
        Assert.DoesNotContain("/i200", ids);
    }

    /// <summary>Two entries of one name are two entries of the index: the reader keeps the environments in the order
    /// the deployment wrote them, trims a name and compares nothing - so <c>tdd</c> twice and <c>TDD</c> beside them
    /// are three entries, each with its own files, and no message about the file.</summary>
    [Fact]
    public void TwoEntriesOfOneNameAreTwoEntriesAndTheReaderComparesNoNameOfThem()
    {
        var index = Index("""{ "environments": [ { "name": "tdd" }, { "name": " tdd " }, { "name": "TDD" } ] }""");

        Assert.Equal(["tdd", "tdd", "TDD"], index.Environments.Select(entry => entry.Name));
        Assert.Equal(
            ["tdd.json", "tdd.json", "TDD.json"],
            index.Environments.Select(entry => entry.Manifest));
    }

    // ----- The readers of this file -----

    /// <summary>The index of a file of <c>nestings</c> open brackets, which the reader of a file either opens as deep
    /// as <c>System.Text.Json</c> goes or refuses whole.</summary>
    private static string DeeplyNested(int nestings) =>
        $$"""{ "environments": [{{new string('[', nestings)}}{{new string(']', nestings)}}] }""";

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

    private static RuntimeEdge Edge(string edge) =>
        Assert.Single(Manifest($$"""{ "environment": "tdd", "edges": [ {{edge}} ] }""").Edges);

    private static RuntimeFrame Frame(string frame) =>
        Assert.Single(Manifest($$"""{ "environment": "tdd", "frames": [ {{frame}} ] }""").Frames!);

    private static RuntimeUnplaced Unplaced(string entry) =>
        Assert.Single(Manifest($$"""{ "environment": "tdd", "unplaced": [ {{entry}} ] }""").Unplaced!);
}
