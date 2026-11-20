using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, the null/empty and the fixed-value tests of <see cref="RuntimeManifestParser" />: the two limits it
/// cuts the unplaced list by, and what its two readers do with a file that is nothing, no JSON, no object, a member of
/// another type, an element of another type, a word of no letters, an address of another scheme and a number that is no
/// whole one. What the app's own sample files give back is pinned in <see cref="RuntimeManifestParserTests" />, the
/// parse result and the frames in <see cref="RuntimeIndexFrameAndParseResultTests" />, the unplaced entries as rows in
/// <see cref="UnplacedTests" />, an environment's database in <see cref="DatabaseViewTests" />, the links of a box in
/// <see cref="RuntimeBoxLinkTests" /> and the architecture of a deployable in <see cref="ApplicationViewTests" /> - and
/// only what those leave open stands here. Every case names the message or the value the reader actually hands back,
/// the odd ones included (<c>regions</c> keeps an empty role and a role written over its own spaces while every other
/// word of the file is trimmed, and a frame with no kind is a frame of the empty word rather than of none), because
/// the deployment writes these files and the dashboard only reads what arrives.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: both readers take a string of JSON and hand back a
/// record, and every address, file name, id and number is one the test brings.
/// </para>
/// </summary>
public class RuntimeManifestParserEdgeCaseTests
{
    /// <summary>The beginning of the addresses the tests below pad to an exact length.</summary>
    private const string AnAddressRoot = "https://demo.example.net/";

    // ----- The file itself, which both readers open the same way -----

    /// <summary>
    /// A file that is no file at all is refused by both readers with the same one message, before either looks for a
    /// member of it: the empty text (of which a tab and a newline are as empty as a space), the JSON that will not
    /// open, and the JSON that opens into anything but an object.
    /// </summary>
    [Theory]
    [InlineData("", "The file is empty.")]
    [InlineData("\t\n ", "The file is empty.")]
    [InlineData("[1, 2", "The file is not valid JSON (line 1, position 6).")]
    [InlineData("{\n \"environment\": \"tdd\",\n \"nodes\": [ ]", "The file is not valid JSON (line 3, position 14).")]
    [InlineData("null", "The file must contain a JSON object.")]
    [InlineData("7", "The file must contain a JSON object.")]
    [InlineData("true", "The file must contain a JSON object.")]
    [InlineData("\"tdd\"", "The file must contain a JSON object.")]
    [InlineData("[[]]", "The file must contain a JSON object.")]
    public void BothReadersRefuseTheSameFileWithTheSameWords(string? json, string message)
    {
        var index = RuntimeManifestParser.ParseIndex(json);
        var manifest = RuntimeManifestParser.ParseManifest(json);

        Assert.False(index.IsValid);
        Assert.False(manifest.IsValid);
        Assert.Null(index.Value);
        Assert.Null(manifest.Value);
        Assert.Equal([message], index.Errors);
        Assert.Equal([message], manifest.Errors);
    }

    /// <summary>The deployment's own files carry the trailing comma and the comment a person annotates a copy with:
    /// both readers skip them, and neither says a word about them.</summary>
    [Fact]
    public void AFileTheDeploymentAnnotatedIsReadAsPlainlyAsOneWithoutNotes()
    {
        var index = Index("""
            { // the note a person leaves in a copy of the file
              "environments": [ { "name": "tdd" }, ],
              "plantuml": "1.2026.8", }
            """);

        Assert.Equal(new RuntimeIndexEntry("tdd", "tdd.json", "tdd.svg"), Assert.Single(index.Environments));
        Assert.Equal("1.2026.8", index.PlantUml);

        var manifest = Manifest("""
            { "environment": "tdd", /* between two members of the object */
              "nodes": [ { "alias": "ca_ui", "name": "ca-demo-tdd-ui" } /* the box itself */, ] }
            """);

        Assert.Equal("ca-demo-tdd-ui", Assert.Single(manifest.Nodes).Name);
    }

    // ----- The index: its one required member, its entries and its two files -----

    /// <summary>The <c>environments</c> of an index must be an array or there is no index - and, unlike every member of
    /// a manifest, a null one is as refused as a member of another type.</summary>
    [Theory]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("7")]
    [InlineData("true")]
    [InlineData("\"tdd\"")]
    public void TheEnvironmentsOfAnIndexAreAnArrayOrTheIndexIsNothing(string environments)
    {
        var result = RuntimeManifestParser.ParseIndex($$"""{ "environments": {{environments}}, "plantuml": "1.2026.8" }""");

        Assert.False(result.IsValid);
        Assert.Null(result.Value);
        Assert.Equal(["environments: missing or not an array."], result.Errors);
    }

    /// <summary>An index of no environments is an index: nothing to show and nothing to say about it. The version that
    /// rendered the diagrams is kept as a trimmed word, and is no version at all when it is another type of JSON or a
    /// pair of spaces.</summary>
    [Fact]
    public void AnIndexOfNoEnvironmentsIsReadAndSaysNothing()
    {
        var empty = RuntimeManifestParser.ParseIndex("""{ "environments": [] }""");

        Assert.True(empty.IsValid);
        Assert.Empty(empty.Errors);
        Assert.Empty(empty.Value!.Environments);
        Assert.Null(empty.Value.PlantUml);

        Assert.Null(Index("""{ "environments": [], "plantuml": "   " }""").PlantUml);
        Assert.Null(Index("""{ "environments": [], "plantuml": "" }""").PlantUml);
        Assert.Null(Index("""{ "environments": [], "plantuml": 7 }""").PlantUml);
        Assert.Null(Index("""{ "environments": [], "plantuml": null }""").PlantUml);
        Assert.Equal("1.2026.8", Index("""{ "environments": [], "plantuml": " 1.2026.8\n" }""").PlantUml);
    }

    /// <summary>An entry that is no object has no name to be read, and is refused by the place it stands in: every one
    /// of them is named, in the order of the file, and the entry that has a name beside them is read all the same -
    /// which is no reason to hand over half an index.</summary>
    [Fact]
    public void AnEntryThatIsNoObjectIsRefusedByItsPlaceAndEveryOneOfThemIsNamed()
    {
        var result = RuntimeManifestParser.ParseIndex("""
            { "environments": [ "tdd", 7, null, [], { }, { "name": "uat" } ] }
            """);

        Assert.False(result.IsValid);
        Assert.Null(result.Value);
        Assert.Equal(
            [
                "environments[0].name: missing or empty.",
                "environments[1].name: missing or empty.",
                "environments[2].name: missing or empty.",
                "environments[3].name: missing or empty.",
                "environments[4].name: missing or empty.",
            ],
            result.Errors);
    }

    /// <summary>A file of an entry is a plain name of <c>runtime/</c>: a backslash, a leading dot, a colon and a
    /// leading slash each name a file the dashboard will not open, and each is the one message of the member it stands
    /// in.</summary>
    [Theory]
    [InlineData(@"sub\tdd.json")]
    [InlineData(".hidden.svg")]
    [InlineData("tdd:latest")]
    [InlineData("/tdd.json")]
    public void AFileThatLeavesRuntimeIsNamedByTheMemberThatNamedIt(string file)
    {
        var json = JsonSerializer.Serialize(file);

        Assert.Equal(
            ["environments[0].manifest: not a file name in runtime/."],
            RuntimeManifestParser.ParseIndex($$"""{ "environments": [ { "name": "tdd", "manifest": {{json}} } ] }""").Errors);
        Assert.Equal(
            ["environments[0].svg: not a file name in runtime/."],
            RuntimeManifestParser.ParseIndex($$"""{ "environments": [ { "name": "tdd", "svg": {{json}} } ] }""").Errors);
    }

    /// <summary>The rule is the three characters and the leading dot, and nothing else: a dot in the middle of a name,
    /// a dash, a space and a name of no extension at all are plain names of <c>runtime/</c>, kept exactly as written.</summary>
    [Fact]
    public void APlainNameOfRuntimeIsKeptAsTheDeploymentWroteIt()
    {
        var index = Index("""
            { "environments": [
                { "name": "tdd", "manifest": "tdd.v2.json", "svg": "tdd-prod.svg" },
                { "name": "prod", "manifest": "prod final.json", "svg": "prod" } ] }
            """);

        Assert.Equal(
            [
                new RuntimeIndexEntry("tdd", "tdd.v2.json", "tdd-prod.svg"),
                new RuntimeIndexEntry("prod", "prod final.json", "prod"),
            ],
            index.Environments);
    }

    /// <summary>The two files of an entry are optional, and so is the text of them: a member of another type, an empty
    /// string and a pair of spaces all fall back to the environment's own name - which is itself read trimmed, so the
    /// fallback of <c>" prod "</c> is <c>prod.json</c> rather than <c> prod .json</c>.</summary>
    [Fact]
    public void AFileThatIsNoTextFallsBackToTheEnvironmentsOwnName()
    {
        var index = Index("""
            { "environments": [ { "name": " prod ", "manifest": "", "svg": "   " },
                                { "name": "uat", "manifest": 7, "svg": null } ] }
            """);

        Assert.Equal(
            [new RuntimeIndexEntry("prod", "prod.json", "prod.svg"), new RuntimeIndexEntry("uat", "uat.json", "uat.svg")],
            index.Environments);
    }

    // ----- The manifest: the shape of its four lists -----

    /// <summary>A manifest of nothing but its environment is the empty one: four empty lists, the frames among them
    /// empty rather than absent (the reader hands back a list whatever the file holds), and no unplaced list at all -
    /// the one member the reader leaves out rather than emptying.</summary>
    [Fact]
    public void AManifestOfNothingButItsEnvironmentIsTheEmptyOne()
    {
        var manifest = Manifest("""{ "environment": "tdd" }""");

        Assert.Equal("tdd", manifest.Environment);
        Assert.Empty(manifest.Nodes);
        Assert.Empty(manifest.Regions);
        Assert.Empty(manifest.Edges);
        Assert.NotNull(manifest.Frames);
        Assert.Empty(manifest.Frames!);
        Assert.Null(manifest.Unplaced);
    }

    /// <summary>Of a member that is not there, no word: a list written as null is as absent as one left out, and gives
    /// the empty list.</summary>
    [Theory]
    [InlineData("nodes")]
    [InlineData("regions")]
    [InlineData("edges")]
    [InlineData("frames")]
    public void ANullMemberOfTheManifestIsNoElementAtAll(string member)
    {
        var nulled = RuntimeManifestParser.ParseManifest($$"""{ "environment": "tdd", "{{member}}": null }""");

        Assert.True(nulled.IsValid, string.Join(' ', nulled.Errors));
        Assert.Empty(nulled.Errors);
        Assert.Equal(0, Count(nulled.Value!, member));
    }

    /// <summary>Of a member that is there as another thing, the one message that names it - and the whole manifest with
    /// it, however the other three members are written.</summary>
    [Theory]
    [InlineData("nodes", "{}")]
    [InlineData("nodes", "7")]
    [InlineData("nodes", "\"tdd\"")]
    [InlineData("nodes", "true")]
    [InlineData("regions", "{}")]
    [InlineData("regions", "7")]
    [InlineData("regions", "\"tdd\"")]
    [InlineData("regions", "true")]
    [InlineData("edges", "{}")]
    [InlineData("edges", "7")]
    [InlineData("edges", "\"tdd\"")]
    [InlineData("edges", "true")]
    [InlineData("frames", "{}")]
    [InlineData("frames", "7")]
    [InlineData("frames", "\"tdd\"")]
    [InlineData("frames", "true")]
    public void AMemberOfTheManifestThatIsNoArrayIsNamedAndRefusesTheFile(string member, string written)
    {
        var result = RuntimeManifestParser.ParseManifest($$"""{ "environment": "tdd", "{{member}}": {{written}} }""");

        Assert.False(result.IsValid);
        Assert.Null(result.Value);
        Assert.Equal([$"{member}: not an array."], result.Errors);
    }

    /// <summary>An element of one of the four lists that is no object is named by its place - and every one of them is
    /// named, in the order of the list: the file is refused rather than read half, so the good element beside them is
    /// kept by the reader and never handed over.</summary>
    [Theory]
    [InlineData("nodes")]
    [InlineData("regions")]
    [InlineData("edges")]
    [InlineData("frames")]
    public void AnElementThatIsNoObjectIsNamedByItsPlaceInTheList(string member)
    {
        var result = RuntimeManifestParser.ParseManifest($$"""
            { "environment": "tdd", "{{member}}": [ 7, "x", null, [ ] ] }
            """);

        Assert.False(result.IsValid);
        Assert.Null(result.Value);
        Assert.Equal(
            [
                $"{member}[0]: not an object.",
                $"{member}[1]: not an object.",
                $"{member}[2]: not an object.",
                $"{member}[3]: not an object.",
            ],
            result.Errors);
    }

    /// <summary>The environment is the one member a manifest cannot do without, and it is missing whichever way it is
    /// worn: another type of JSON, no text at all, or a word of nothing but spaces.</summary>
    [Theory]
    [InlineData("null")]
    [InlineData("7")]
    [InlineData("true")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("\"\"")]
    [InlineData("\"   \"")]
    public void AnEnvironmentThatIsNoWordIsMissingWhicheverWayItIsWorn(string environment)
    {
        var result = RuntimeManifestParser.ParseManifest($$"""
            { "environment": {{environment}}, "nodes": [ { "alias": "a" } ] }
            """);

        Assert.False(result.IsValid);
        Assert.Equal(["environment: missing or empty."], result.Errors);
    }

    /// <summary>Every problem of a manifest is said, in the order the members are read - nodes, regions, frames and
    /// then edges, which is not the order of the file - and the unplaced list never joins in: an entry of no name there
    /// is skipped, not refused.</summary>
    [Fact]
    public void EveryProblemOfAManifestIsNamedInTheOrderItsMembersAreRead()
    {
        var result = RuntimeManifestParser.ParseManifest("""
            { "environment": "tdd",
              "nodes": [ { "kind": "webapp" } ],
              "regions": [ { "name": "westus3" } ],
              "edges": [ { "id": "a-to-b", "from": "a" } ],
              "frames": [ { "kind": "plan" } ],
              "unplaced": [ { "type": "Vault" }, 7, { "name": "kept" } ] }
            """);

        Assert.Null(result.Value);
        Assert.Equal(
            [
                "nodes[0].alias: missing or empty.",
                "regions[0].alias: missing or empty.",
                "frames[0].alias: missing or empty.",
                "edges[0]: id, from and to are required.",
            ],
            result.Errors);
    }

    // ----- A node: its name, its words, its address and its kind -----

    /// <summary>A node with no name is called by its alias, and a name that is no word is as absent as no name at all;
    /// a name that is a word is kept trimmed. Without anything else the node is the bare record, every other member of
    /// it null and its kind the one the dashboard does not know.</summary>
    [Fact]
    public void ANodeWithoutItsNameIsCalledByItsAliasAndANameIsKeptAsWritten()
    {
        Assert.Equal(new RuntimeNode("app_ui", RuntimeNodeKind.Other, "app_ui"), Node("""{ "alias": "app_ui" }"""));

        Assert.Equal("app_ui", Node("""{ "alias": "app_ui", "name": "   " }""").Name);
        Assert.Equal("app_ui", Node("""{ "alias": "app_ui", "name": "" }""").Name);
        Assert.Equal("app_ui", Node("""{ "alias": "app_ui", "name": 7 }""").Name);
        Assert.Equal("app_ui", Node("""{ "alias": "app_ui", "name": null }""").Name);
        Assert.Equal("ca-demo-tdd-ui", Node("""{ "alias": "app_ui", "name": "  ca-demo-tdd-ui  " }""").Name);
    }

    /// <summary>Of the words a node carries, only the role is brought to lower case, since the manifest writes it in
    /// the deployment's own case and the page compares it with <c>primary</c>; every other word is kept as written, and
    /// trimmed. A word of spaces is no word.</summary>
    [Fact]
    public void OnlyTheRoleIsLoweredOfTheWordsANodeCarries()
    {
        var node = Node("""
            { "alias": "app_ui", "kind": "webapp", "deployable": "UI", "role": " PRIMARY ", "region": "WESTUS3",
              "regionAlias": "Region_Primary", "healthCheck": " LlmGateway ", "dependencyKind": "External" }
            """);

        Assert.Equal("UI", node.Deployable);
        Assert.Equal("primary", node.Role);
        Assert.Equal("WESTUS3", node.Region);
        Assert.Equal("Region_Primary", node.RegionAlias);
        Assert.Equal("LlmGateway", node.HealthCheck);
        Assert.Equal("External", node.DependencyKind);

        Assert.Null(Node("""{ "alias": "a", "role": "   ", "deployable": 7, "healthCheck": null }""").Role);
        Assert.Null(Node("""{ "alias": "a", "role": "   " }""").Role);
        Assert.Null(Node("""{ "alias": "a", "deployable": 7 }""").Deployable);
        Assert.Null(Node("""{ "alias": "a", "healthCheck": null }""").HealthCheck);
        Assert.Null(Node("""{ "alias": "a", "dependencyKind": [] }""").DependencyKind);
    }

    /// <summary>The address a box is checked at is kept for <c>http</c> too, unlike a link - a deployment may well
    /// publish an app in the clear inside a network - and a query, a fragment and the case of the scheme are none of
    /// the reader's business here.</summary>
    [Theory]
    [InlineData("https://demo.example.net/", "https", "demo.example.net")]
    [InlineData("http://demo.example.net/", "http", "demo.example.net")]
    [InlineData("HTTPS://demo.example.net/a?b=1#two", "https", "demo.example.net")]
    [InlineData("  https://demo.example.net/spaced  ", "https", "demo.example.net")]
    [InlineData("https://10.0.0.8:8080/_health", "https", "10.0.0.8")]
    public void ANodeAddressIsKeptWhenItIsAnAbsoluteHttpOrHttpsOne(string url, string scheme, string host)
    {
        var node = Node($$"""{ "alias": "a", "url": "{{url}}" }""");

        Assert.Equal(scheme, node.Url?.Scheme);
        Assert.Equal(host, node.Url?.Host);
    }

    /// <summary>A string that is no http address is the one thing about a node's address that is refused; a member that
    /// is no string at all, an empty one and a pair of spaces are simply no address, and say nothing.</summary>
    [Theory]
    [InlineData("//demo.example.net/x")]
    [InlineData("demo.example.net/x")]
    [InlineData("ftp://demo.example.net/x")]
    [InlineData("about:blank")]
    [InlineData("file:///etc/passwd")]
    public void ANodeAddressThatIsAStringAndNoHttpAddressIsAnError(string url)
    {
        var result = RuntimeManifestParser.ParseManifest($$"""
            { "environment": "tdd", "nodes": [ { "alias": "a", "url": "{{url}}" } ] }
            """);

        Assert.False(result.IsValid);
        Assert.Equal(["nodes[0].url: not an absolute http or https address."], result.Errors);
    }

    /// <summary>A url that is no text at all is no address and no offence either: the member is left out, as it is when
    /// the file writes it as null, as a number, as an object or as a pair of spaces.</summary>
    [Theory]
    [InlineData("null")]
    [InlineData("7")]
    [InlineData("true")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("\"\"")]
    [InlineData("\"   \"")]
    public void ANodeAddressThatIsNoTextIsLeftOutAndSaysNothing(string url)
    {
        var manifest = Manifest($$"""{ "environment": "tdd", "nodes": [ { "alias": "a", "url": {{url}} } ] }""");

        Assert.Null(Assert.Single(manifest.Nodes).Url);
    }

    /// <summary>A node's kind is the deployment's word, trimmed, brought to lower case and matched whole: a word the
    /// dashboard has never heard, a word spelled with a dash or an underscore in it, an empty one and no word at all
    /// are all the one kind that is drawn and not updated.</summary>
    [Theory]
    [InlineData("person", RuntimeNodeKind.Person)]
    [InlineData("FrontDoor", RuntimeNodeKind.FrontDoor)]
    [InlineData("WEBAPP", RuntimeNodeKind.WebApp)]
    [InlineData("Sql", RuntimeNodeKind.Sql)]
    [InlineData("staticSITE", RuntimeNodeKind.StaticSite)]
    [InlineData("DEPENDENCY", RuntimeNodeKind.Dependency)]
    [InlineData("ContainerApp", RuntimeNodeKind.ContainerApp)]
    [InlineData("resource", RuntimeNodeKind.Resource)]
    [InlineData(" webapp ", RuntimeNodeKind.WebApp)]
    [InlineData("web app", RuntimeNodeKind.Other)]
    [InlineData("WEB_APP", RuntimeNodeKind.Other)]
    [InlineData("container-app", RuntimeNodeKind.Other)]
    public void EveryKindOfNodeIsReadFromItsWordInAnyCase(string kind, RuntimeNodeKind expected) =>
        Assert.Equal(expected, Node($$"""{ "alias": "a", "kind": "{{kind}}" }""").Kind);

    /// <summary>A kind that is no word at all is the kind the dashboard does not know, whichever way the file wears it
    /// - and a word written over its own spaces is the same word it is without them, since every text of the file is
    /// trimmed before it is read.</summary>
    [Theory]
    [InlineData("null")]
    [InlineData("7")]
    [InlineData("{}")]
    [InlineData("\"\"")]
    [InlineData("\"   \"")]
    public void ANodeThatNamesNoKindNamesNoneOfTheWrongOnesEither(string kind) =>
        Assert.Equal(RuntimeNodeKind.Other, Node($$"""{ "alias": "a", "kind": {{kind}} }""").Kind);

    // ----- A relationship: its kind and its priority -----

    /// <summary>A relationship's kind is the same read as a node's: the deployment's word, lowered and matched whole,
    /// and any other word is a line the dashboard draws without a meaning.</summary>
    [Theory]
    [InlineData("public", RuntimeEdgeKind.Public)]
    [InlineData("ORIGIN", RuntimeEdgeKind.Origin)]
    [InlineData("SQL", RuntimeEdgeKind.Sql)]
    [InlineData("Dashboard", RuntimeEdgeKind.Dashboard)]
    [InlineData("dependency", RuntimeEdgeKind.Dependency)]
    [InlineData("amqp", RuntimeEdgeKind.Other)]
    [InlineData("origins", RuntimeEdgeKind.Other)]
    [InlineData("public-1", RuntimeEdgeKind.Other)]
    public void EveryKindOfRelationshipIsReadFromItsWordInAnyCase(string kind, RuntimeEdgeKind expected) =>
        Assert.Equal(expected, Edge($$"""{ "id": "a-to-b", "from": "a", "to": "b", "kind": "{{kind}}" }""").Kind);

    /// <summary>A priority is kept when the JSON holds a whole number of an <c>int</c> - zero, negative and the two
    /// ends of it included - and left out when the number is a fraction or too big for an <c>int</c>. None of that is
    /// an error: a line without its priority is still a line.</summary>
    [Theory]
    [InlineData("0", 0)]
    [InlineData("1", 1)]
    [InlineData("-1", -1)]
    [InlineData("2147483647", 2147483647)]
    [InlineData("-2147483648", -2147483648)]
    [InlineData("2147483648", null)]
    [InlineData("-2147483649", null)]
    [InlineData("1.5", null)]
    [InlineData("1.0", null)]
    [InlineData("1e2", null)]
    public void APriorityIsKeptOnlyWhenTheNumberIsAWholeOneOfAnInt(string priority, int? expected) =>
        Assert.Equal(expected, Edge($$"""{ "id": "a-to-b", "from": "a", "to": "b", "priority": {{priority}} }""").Priority);

    /// <summary>What the reader does today with a priority that is no number at all - the member written as text, as
    /// null, as a bool, as an object or as a list: it asks <c>TryGetInt32</c> of the element, and that refuses an
    /// element of another type rather than answering no. This is the one optional member of the manifest the file may
    /// not write wrongly, where every other is left out in silence; pinned as it behaves today, since the deployment
    /// writes these files and a manifest that throws is a page that shows nothing.</summary>
    [Theory]
    [InlineData("\"1\"")]
    [InlineData("null")]
    [InlineData("true")]
    [InlineData("{}")]
    [InlineData("[]")]
    public void APriorityThatIsNoNumberStopsTheManifestBeingReadAtAll(string priority)
    {
        Assert.Throws<InvalidOperationException>(
            () => RuntimeManifestParser.ParseManifest($$"""{ "environment": "tdd", "edges": [ { "id": "a-to-b", "from": "a", "to": "b", "priority": {{priority}} } ] }"""));
    }

    // ----- The three optional parts of a node that the reader counts -----

    /// <summary>A health path is a plain path: the root alone, and every character the rule lets stand - a letter, a
    /// digit and the five of <c>/ . _ ~ -</c> - kept trimmed and as written.</summary>
    [Theory]
    [InlineData("/", "/")]
    [InlineData("/a_b.c~d-e/1", "/a_b.c~d-e/1")]
    [InlineData("  /_healthcheck  ", "/_healthcheck")]
    [InlineData("/v2.0/status", "/v2.0/status")]
    public void AHealthPathOfTheLettersDigitsAndMarksAllowedIsKept(string path, string kept) =>
        Assert.Equal(kept, Node($$"""{ "alias": "a", "kind": "containerapp", "healthPath": "{{path}}" }""").HealthPath);

    /// <summary>Two hundred characters of path, one under and one over: the one over is left out rather than cut, and
    /// the manifest is read all the same.</summary>
    [Theory]
    [InlineData(199, true)]
    [InlineData(200, true)]
    [InlineData(201, false)]
    public void AHealthPathIsKeptToItsTwoHundredthCharacter(int length, bool kept)
    {
        var path = $"/{new string('a', length - 1)}";
        Assert.Equal(length, path.Length);

        var node = Node($$"""{ "alias": "a", "kind": "containerapp", "healthPath": "{{path}}" }""");

        if (kept)
        {
            Assert.Equal(path, node.HealthPath);
        }
        else
        {
            Assert.Null(node.HealthPath);
        }
    }

    /// <summary>The ids a box stands for end at two hundred of them: one under the most, the most, and one over, which
    /// is the one that goes - the first two hundred are kept, in the order the listing wrote them.</summary>
    [Theory]
    [InlineData(199, 199)]
    [InlineData(200, 200)]
    [InlineData(201, 200)]
    public void ABoxCarriesAtMostTwoHundredResourceIds(int written, int kept)
    {
        var ids = string.Join(',', Enumerable.Range(0, written).Select(number => $$""" "/i{{number}}" """));
        var node = Node($$"""{ "alias": "res_1", "kind": "resource", "ids": [{{ids}}] }""");

        Assert.Equal(kept, node.Ids!.Count);
        Assert.Equal("/i0", node.Ids[0]);
        Assert.Equal($"/i{kept - 1}", node.Ids[^1]);
    }

    /// <summary>Where a deployable publishes its diagrams is kept to four hundred characters - the health path's two
    /// hundred, doubled - and a fragment is as much left out as the query the tests above pin; the manifest is read
    /// either way, since the box only behaves as without one.</summary>
    [Theory]
    [InlineData(399, true)]
    [InlineData(400, true)]
    [InlineData(401, false)]
    public void AnArchitectureIsKeptToItsFourHundredthCharacter(int length, bool kept)
    {
        var address = AnAddressRoot + new string('a', length - AnAddressRoot.Length);
        Assert.Equal(length, address.Length);

        var node = Node($$"""{ "alias": "a", "kind": "webapp", "architecture": "{{address}}" }""");

        if (kept)
        {
            Assert.Equal(address, node.Architecture!.AbsoluteUri);
        }
        else
        {
            Assert.Null(node.Architecture);
        }
    }

    /// <summary>An architecture may end in a path and nothing else: a fragment is as much left out as the query the
    /// tests of <see cref="ApplicationViewTests" /> pin, and the node beside it keeps its own address.</summary>
    [Fact]
    public void AnArchitectureWithAFragmentIsLeftOutAndTheManifestIsRead()
    {
        var manifest = Manifest("""
            { "environment": "tdd", "nodes": [
                { "alias": "a", "kind": "containerapp", "architecture": "https://demo.example.net/ui#drawer" },
                { "alias": "b", "kind": "webapp", "architecture": "https://demo.example.net/ui" } ] }
            """);

        Assert.Null(manifest.Nodes[0].Architecture);
        Assert.Equal(new Uri("https://demo.example.net/ui"), manifest.Nodes[1].Architecture);
    }

    // ----- A region and a frame: the two names they fall back to -----

    /// <summary>A region with no name is called by its alias, and its roles are the strings of its list exactly as
    /// written: a role of no letters and a role written over its own spaces both stand, since nothing here is a word
    /// the page compares - only a JSON value that is no string at all is left out, and never an error.</summary>
    [Fact]
    public void ARegionWearsItsAliasAsItsNameAndKeepsEveryWordOfItsRoles()
    {
        var bare = Region("""{ "alias": "region_data" }""");

        Assert.Equal("region_data", bare.Name);
        Assert.Empty(bare.Roles);
        Assert.Null(bare.Links);

        var named = Region("""{ "alias": "region_data", "name": "  centralus  ", "roles": [ "primary", 7, null, "", " standby " ] }""");

        Assert.Equal("centralus", named.Name);
        Assert.Equal(["primary", "", " standby "], named.Roles);

        Assert.Equal("r", Region("""{ "alias": "r", "name": "   " }""").Name);
        Assert.Equal("r", Region("""{ "alias": "r", "name": 7 }""").Name);
        Assert.Equal("r", Region("""{ "alias": "r", "name": null }""").Name);
        Assert.Empty(Region("""{ "alias": "r", "roles": {} }""").Roles);
        Assert.Empty(Region("""{ "alias": "r", "roles": null }""").Roles);
        Assert.Empty(Region("""{ "alias": "r", "roles": [ ] }""").Roles);
    }

    /// <summary>A frame with no kind is a frame of the empty word rather than of no word at all - the kind is a
    /// <c>string</c>, and the reader gives it nothing to say - and a frame with no name is called by its alias. The
    /// kind is trimmed like every other word and lowered like none of them.</summary>
    [Fact]
    public void AFrameWithNoKindOrNoNameIsStillReadWithWhatItHas()
    {
        var bare = Frame("""{ "alias": "own_web" }""");

        Assert.Equal("own_web", bare.Alias);
        Assert.Equal(string.Empty, bare.Kind);
        Assert.Equal("own_web", bare.Name);
        Assert.Null(bare.Links);

        Assert.Equal(string.Empty, Frame("""{ "alias": "f", "kind": "   " }""").Kind);
        Assert.Equal(string.Empty, Frame("""{ "alias": "f", "kind": 7 }""").Kind);
        Assert.Equal(string.Empty, Frame("""{ "alias": "f", "kind": null }""").Kind);
        Assert.Equal("Runtime", Frame("""{ "alias": "f", "kind": " Runtime " }""").Kind);
        Assert.Equal("f", Frame("""{ "alias": "f", "name": "   " }""").Name);
        Assert.Equal("f", Frame("""{ "alias": "f", "name": [] }""").Name);
    }

    // ----- The two public limits of the unplaced list, at their edges -----

    /// <summary>The most entries the reader keeps is five hundred. The <c>500</c> of it was only ever reached through a
    /// list of five hundred and forty entries before this file, so the constant stands here on its own.</summary>
    [Fact]
    public void TheMostUnplacedEntriesAreFiveHundred() => Assert.Equal(500, RuntimeManifestParser.MostUnplaced);

    /// <summary>And the list ends exactly there: one under the most, the most, and the one over it that goes - the
    /// first five hundred entries are kept, in the order the manifest wrote them, and the reader stops without a
    /// word about the rest.</summary>
    [Theory]
    [InlineData(1, 1)]
    [InlineData(499, 499)]
    [InlineData(500, 500)]
    [InlineData(501, 500)]
    [InlineData(502, 500)]
    public void TheUnplacedListOfTheManifestEndsAtTheMostEntries(int written, int kept)
    {
        var entries = string.Join(',', Enumerable.Range(0, written).Select(number => $$"""{ "name": "r{{number}}" }"""));
        var list = Manifest($$"""{ "environment": "tdd", "unplaced": [{{entries}}] }""").Unplaced!;

        Assert.Equal(kept, list.Count);
        Assert.Equal("r0", list[0].Name);
        Assert.Equal($"r{kept - 1}", list[^1].Name);
    }

    /// <summary>A text of an unplaced entry is cut at the longest one kept, one character under it whole and one over
    /// it short: the name, the type, the group and the words are all cut the same way, by the public constant
    /// <see cref="RuntimeManifestParser.LongestUnplacedText" /> itself.</summary>
    [Theory]
    [InlineData(299, 299)]
    [InlineData(300, 300)]
    [InlineData(301, 300)]
    [InlineData(1200, 300)]
    public void ATextOfAnUnplacedEntryIsCutAtTheLongestOneKept(int written, int kept)
    {
        var text = new string('x', written);
        var entry = Unplaced($$"""{ "name": "{{text}}", "type": "{{text}}", "group": "{{text}}", "words": "{{text}}" }""");

        Assert.Equal(kept, entry.Name.Length);
        Assert.All(new[] { entry.Type, entry.Group, entry.Words }, word => Assert.Equal(kept, word!.Length));
    }

    /// <summary>The id an unplaced entry is found by is cut at twice the longest text - six hundred characters, since
    /// an id is a path through Azure and no name is - and, unlike a text of the entry, an id over the cut leaves as the
    /// whole first six hundred characters of it rather than nothing.</summary>
    [Theory]
    [InlineData(599, 599)]
    [InlineData(600, 600)]
    [InlineData(601, 600)]
    [InlineData(1200, 600)]
    public void TheIdOfAnUnplacedEntryIsCutAtTwiceTheLongestText(int written, int kept)
    {
        Assert.Equal(600, RuntimeManifestParser.LongestUnplacedText * 2);

        var id = "/s/" + new string('a', written - 3);
        Assert.Equal(written, id.Length);

        var entry = Unplaced($$"""{ "name": "acrdemo1", "ids": [ "{{id}}" ] }""");

        Assert.Equal(kept, Assert.Single(entry.Ids!).Length);
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

    private static RuntimeEdge Edge(string edge) =>
        Assert.Single(Manifest($$"""{ "environment": "tdd", "edges": [ {{edge}} ] }""").Edges);

    private static RuntimeRegion Region(string region) =>
        Assert.Single(Manifest($$"""{ "environment": "tdd", "regions": [ {{region}} ] }""").Regions);

    private static RuntimeFrame Frame(string frame) =>
        Assert.Single(Manifest($$"""{ "environment": "tdd", "frames": [ {{frame}} ] }""").Frames!);

    private static RuntimeUnplaced Unplaced(string entry) =>
        Assert.Single(Manifest($$"""{ "environment": "tdd", "unplaced": [ {{entry}} ] }""").Unplaced!);

    private static int Count(RuntimeManifest manifest, string member) => member switch
    {
        "nodes" => manifest.Nodes.Count,
        "regions" => manifest.Regions.Count,
        "edges" => manifest.Edges.Count,
        _ => manifest.Frames!.Count,
    };
}
