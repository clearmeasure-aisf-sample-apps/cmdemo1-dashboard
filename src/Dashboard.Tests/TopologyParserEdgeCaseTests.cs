using System.Reflection;
using static Dashboard.Tests.TopologyReading;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests left open for <see cref="TopologyParser" />, the reader of
/// <c>topology.json</c> (<c>src/Dashboard/Health/TopologyParser.cs</c>). That file declares two public types, and both
/// are here, each from the outside: <see cref="TopologyParser.Parse" />, the one member of the reader, which is given
/// the text of a file and hands back a <see cref="TopologyParseResult" />, and the record it hands back.
/// <para>
/// What is already pinned stands in other files and is not pinned again here. <see cref="TopologyParserTests" /> holds
/// the shape of the file - an empty one, one that will not open, one that is no object, an <c>environments</c> that is
/// no array, the defaults of a bare topology, the unknown field, the comment and the trailing comma, the address that
/// is no address and the place it is named with, the role read in any case, the path given its leading slash, the
/// nodes, the pages, the pins and the cluster of a whole file. <see cref="TopologyParseResultTests" /> holds the result
/// as the page reads it, <see cref="LinkTests" /> the links of an environment, a deployable and a node,
/// <see cref="TelemetryTests" /> the telemetry path and the traffic paths of a working file,
/// <see cref="HealthDetailTests" /> and <see cref="BuildInfoTests" /> the detailed health and the build path,
/// <see cref="DashboardBuildTests" /> <c>system.dashboard</c>, and <see cref="DeliveryTests" />,
/// <see cref="CostTests" /> and <see cref="DeploymentsTests" /> the three published addresses of the system.
/// </para>
/// <para>
/// What is left, and set down here: the whitespace no person types on purpose - a tab, a newline, a non-breaking space
/// - as the empty file it is; the byte a position is counted in rather than the letter; the file that opens into a
/// <c>null</c>, a truth or a list of objects; the <c>system</c> of another shape, which is no system and no error;
/// every word of the file read over its own spaces, and a word of spaces as the nothing it is rather than as the empty
/// string it was written as; the moment of the file in the shapes a workflow writes it, and the shape that is a
/// calendar that never was; the deployable's three required-by-nobody paths falling back to the constants their record
/// declares; the optional paths, the traffic paths, the nodes, the pages and the cluster refused by their place with
/// the exact words the page prints; and the two types' own surface, which is one way in, two parts out, a clock-free
/// and call-free read of a string.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: <see cref="TopologyParser.Parse" /> takes the
/// text and hands back the record, and every address, moment, word and number in it is one the test brings.
/// </para>
/// </summary>
public class TopologyParserEdgeCaseTests
{
    /// <summary>The seven shapes a member of the file comes in when it says nothing of itself: <c>null</c>, the empty
    /// word, a word of spaces, a number, a truth, a list and an object. Every one of them is what the file means by
    /// "not written", and the reader is asked to say so the same way it does when the member is not there at all -
    /// which <see cref="TopologyParserTests" /> already pins for the absent one. The cases below put these seven into
    /// whichever member they are about rather than spell each of them out seven times.</summary>
    public static IEnumerable<object[]> Nothing => [.. NothingShapes.Select(shape => new object[] { shape })];

    /// <summary>The four shapes a list the file names comes in when it is no list. A <c>null</c> is not among them:
    /// that one is the empty list the file describes, and is read rather than refused
    /// (<see cref="NodesOfNothingAreNoNodesAndNoError" />).</summary>
    public static IEnumerable<object[]> NotAnArray => [.. NotAnArrayShapes.Select(shape => new object[] { shape })];

    private static readonly string[] NothingShapes = ["null", "\"\"", "\"   \"", "7", "true", "[]", "{}"];

    private static readonly string[] NotAnArrayShapes = ["7", "true", "\"none\"", "{}"];

    // ----- The text the reader is handed -----

    /// <summary>The blank a hand-edited file carries from a web page or a paste is as empty as a space: the reader
    /// trims nothing away to find a document in it, and says the one thing it says of a file with nothing in it.</summary>
    [Theory]
    [InlineData("\t\n\r ")]
    [InlineData("\u00A0")]
    [InlineData("\u2028\u2029")]
    public void WhitespaceOfEveryKindIsTheEmptyFile(string json) =>
        Assert.Equal("The file is empty.", Assert.Single(Errors(json)));

    /// <summary>The place of the problem is the byte the reader stopped at, not the letter: an <c>é</c> is two bytes of
    /// UTF-8 and moves the position of everything after it by one. A byte-order mark is refused where it stands, the
    /// note a person leaves in a copy of a file with nothing else in it is a file of no JSON at its last line, and the
    /// word after the document is refused at the letter it begins on.</summary>
    [Theory]
    [InlineData("\uFEFF{ \"environments\": [] }", "The file is not valid JSON (line 1, position 1).")]
    [InlineData("// nothing here", "The file is not valid JSON (line 1, position 16).")]
    [InlineData("{ \"environments\": [] } trailing", "The file is not valid JSON (line 1, position 24).")]
    [InlineData("{ \"environments\": [", "The file is not valid JSON (line 1, position 20).")]
    [InlineData("{ \"system\": { \"slug\": \"café\" }, \"environments\": [ { \"name\": } ] }", "The file is not valid JSON (line 1, position 62).")]
    public void ThePlaceOfTheProblemIsSaidInBytesAndInTheWordsOfThePage(string json, string message) =>
        Assert.Equal(message, Assert.Single(Errors(json)));

    /// <summary>A file that opens into a <c>null</c>, a truth or a list of objects is as little a topology as a number
    /// is: one message, and no word about what it found instead.</summary>
    [Theory]
    [InlineData("null")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("[{}]")]
    [InlineData("[[]]")]
    public void AnyOtherShapeAtTheTopIsNoObject(string json) =>
        Assert.Equal("The file must contain a JSON object.", Assert.Single(Errors(json)));

    // ----- system -----

    /// <summary>A <c>system</c> that is no object says nothing, and says it quietly: it is the system of the defaults,
    /// the empty slug and the word <c>System</c>, with no address and no dashboard of its own.</summary>
    [Theory]
    [InlineData("null")]
    [InlineData("7")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("[]")]
    [InlineData("[ { \"slug\": \"demo\" } ]")]
    [InlineData("{ }")]
    public void ASystemOfNoObjectIsTheSystemOfTheDefaults(string system)
    {
        var read = Valid($$"""{ "system": {{system}}, "environments": [] }""").System;

        Assert.Equal(new SystemInfo(string.Empty, "System"), read);
        Assert.Equal(string.Empty, read.Slug);
        Assert.Equal("System", read.Name);
        Assert.Null(read.Repository);
        Assert.Null(read.DeliveryUrl);
        Assert.Null(read.CostUrl);
        Assert.Null(read.DeploymentsUrl);
        Assert.Null(read.Dashboard);
    }

    /// <summary>Every word of the system is read over its own spaces, and the name it does not give itself is its slug
    /// - or, when the slug is spaces too, the one word the page has for a system that is nameless.</summary>
    [Fact]
    public void TheWordsOfTheSystemAreReadWithoutTheirSpaces()
    {
        var system = Valid("""{ "system": { "slug": "  cmdemo1  ", "name": "  Demo One  " }, "environments": [] }""").System;

        Assert.Equal("cmdemo1", system.Slug);
        Assert.Equal("Demo One", system.Name);
    }

    [Theory]
    [MemberData(nameof(Nothing))]
    public void ANameTheSystemDoesNotGiveIsItsSlugOrCreate(string name) =>
        Assert.Equal("demo", Valid($$"""{ "system": { "slug": "demo", "name": {{name}} }, "environments": [] }""").System.Name);

    [Fact]
    public void ASlugOfSpacesNamesNothingAndLeavesTheSystemACreate() =>
        Assert.Equal("System", Valid("""{ "system": { "slug": "   ", "name": "" }, "environments": [] }""").System.Name);

    // ----- generated -----

    /// <summary>The moment the file was written is kept as it is written: in Zulu, in another hour, with its fraction,
    /// with its spaces around it, in lower case, and as the midnight of a bare day. An hour the file names is kept
    /// rather than moved, so the page can say the time the collector said it in.</summary>
    [Fact]
    public void TheMomentTheFileWasWrittenIsKeptAsTheFileWritesIt()
    {
        Assert.Equal(new DateTimeOffset(2026, 10, 4, 22, 0, 0, TimeSpan.Zero), Generated("\"2026-10-04T22:00:00Z\""));
        Assert.Equal(new DateTimeOffset(2026, 10, 4, 22, 0, 0, TimeSpan.Zero), Generated("\" 2026-10-04T22:00:00Z \""));
        Assert.Equal(new DateTimeOffset(2026, 10, 4, 22, 0, 0, TimeSpan.Zero), Generated("\"2026-10-04t22:00:00z\""));
        Assert.Equal(new DateTimeOffset(2026, 10, 4, 22, 0, 0, 250, TimeSpan.Zero), Generated("\"2026-10-04T22:00:00.250Z\""));
        Assert.Equal(new DateTimeOffset(2026, 10, 4, 22, 0, 0, TimeSpan.FromHours(2)), Generated("\"2026-10-04T22:00:00+02:00\""));
        Assert.Equal(new DateTimeOffset(2026, 10, 4, 5, 0, 0, TimeSpan.Zero), Generated("\"2026-10-04 05:00:00\""));
        Assert.Equal(new DateTimeOffset(2026, 10, 4, 0, 0, 0, TimeSpan.Zero), Generated("\"2026-10-04\""));
    }

    /// <summary>A moment that is no moment - another type of JSON, a word, a date that never was, a pair of spaces - is
    /// no moment, and the file is shown all the same: the time the file was written is the one thing the page says
    /// nothing about when it is missing.</summary>
    [Theory]
    [MemberData(nameof(Nothing))]
    [InlineData("\"yesterday\"")]
    [InlineData("\"2026-13-04T22:00:00Z\"")]
    public void ATimeThatIsNoTimeIsNoTimeAndNoError(string generated) =>
        Assert.Null(Generated(generated));

    [Fact]
    public void AFileThatNamesNoMomentAtAllIsAFileWithoutOne() =>
        Assert.Null(Valid("""{ "environments": [] }""").Generated);

    // ----- environments[] -----

    /// <summary>The name is the one word an environment must have, and every shape of it that says nothing is the
    /// absence of it: no letters, nothing but spaces, <c>null</c>, a number, a truth, a list, an object. Each of them is
    /// refused with the place it stands in, and each of them refuses the whole file rather than the environment - the
    /// page shows no dashboard at all. A name that is not there is the same refusal
    /// (<see cref="TopologyParserTests" />).</summary>
    [Theory]
    [MemberData(nameof(Nothing))]
    public void AnEnvironmentWithoutItsNameIsRefusedByItsPlace(string name) =>
        Assert.Equal(["environments[0].name: missing or empty."], Errors($$"""{ "environments": [ { "name": {{name}} } ] }"""));

    /// <summary>An entry that is no object says so by its place, and the places are counted in the file rather than
    /// among the entries that were read: the good environment after three bad ones is still the fourth.</summary>
    [Theory]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("[{}]")]
    [InlineData("7")]
    [InlineData("true")]
    public void AnEntryThatIsNoObjectIsRefusedByItsPlace(string environment) =>
        Assert.Equal(["environments[0]: not an object."], Errors($$"""{ "environments": [ {{environment}} ] }"""));

    [Fact]
    public void ThePlacesOfTheEnvironmentsAreCountedInTheFileRatherThanAmongTheGoodOnes()
    {
        var errors = Errors("""{ "environments": [ "tdd", 7, null, { "name": "uat" }, { } ] }""");

        Assert.Equal(
            [
                "environments[0]: not an object.",
                "environments[1]: not an object.",
                "environments[2]: not an object.",
                "environments[4].name: missing or empty.",
            ],
            errors);
    }

    /// <summary>The tier and the namespace are read over their spaces, and are the nothing they are when they are
    /// spaces, <c>null</c> or another type of JSON: an environment does not fail a file by saying too little.</summary>
    [Theory]
    [MemberData(nameof(Nothing))]
    public void TheWordsAnEnvironmentDoesNotGiveAreNothingRatherThanProblems(string tier)
    {
        var environment = Environment($$"""{ "name": "tdd", "tier": {{tier}}, "namespace": {{tier}} }""");

        Assert.Null(environment.Tier);
        Assert.Null(environment.Namespace);
    }

    [Fact]
    public void TheWordsOfAnEnvironmentAreReadWithoutTheirSpaces()
    {
        var environment = Environment("""{ "name": "  tdd  ", "tier": "  nonprod  ", "namespace": " cmdemo1-tdd " }""");

        Assert.Equal("tdd", environment.Name);
        Assert.Equal("nonprod", environment.Tier);
        Assert.Equal("cmdemo1-tdd", environment.Namespace);
    }

    /// <summary>A list of the wrong type is a problem, a <c>null</c> is not: an environment whose deployables are
    /// <c>null</c> has none, and the page is drawn without any.</summary>
    [Fact]
    public void DeployablesOfNothingAreNoDeployablesAndNoError() =>
        Assert.Empty(Environment("""{ "name": "tdd", "deployables": null }""").Deployables);

    // ----- deployables[] -----

    /// <summary>The name a deployable does not give itself is <c>app</c>: the word of a deployable nobody named, whatever
    /// shape the nobody-named member comes in.</summary>
    [Theory]
    [MemberData(nameof(Nothing))]
    public void ADeployableWithoutANameIsCalledApp(string name) =>
        Assert.Equal("app", Deployable($$"""{ "name": {{name}} }""").Name);

    [Fact]
    public void TheNameOfADeployableIsReadWithoutItsSpaces() =>
        Assert.Equal("ui", Deployable("""{ "name": "  ui  " }""").Name);

    /// <summary>The three paths a node is called at are the constants their record declares whenever the file gives
    /// nothing of them - another type of JSON, an empty word, a word of spaces, <c>null</c> - and the path it does give
    /// is read over its spaces and given its leading slash (<see cref="APathKeepsItsOwnFrontAndTakesTheOneItLacks" />).
    /// The member that is not there at all is pinned in <see cref="TopologyParserTests" />.</summary>
    [Theory]
    [MemberData(nameof(Nothing))]
    public void APathOfNothingIsThePathTheRecordNames(string path)
    {
        var read = Deployable($$"""{ "healthPath": {{path}}, "alivePath": {{path}}, "versionPath": {{path}} }""");

        Assert.Equal(DeployableInfo.DefaultHealthPath, read.HealthPath);
        Assert.Equal(DeployableInfo.DefaultAlivePath, read.AlivePath);
        Assert.Equal(DeployableInfo.DefaultVersionPath, read.VersionPath);
        Assert.Equal("/_healthcheck", read.HealthPath);
        Assert.Equal("/alive", read.AlivePath);
        Assert.Equal("/_version", read.VersionPath);
    }

    [Theory]
    [InlineData("\" /health \"", "/health")]
    [InlineData("\"health\"", "/health")]
    [InlineData("\"//health\"", "//health")]
    [InlineData("\"/health/x\"", "/health/x")]
    [InlineData("\"/~\"", "/~")]
    public void APathKeepsItsOwnFrontAndTakesTheOneItLacks(string path, string expected) =>
        Assert.Equal(expected, Deployable($$"""{ "healthPath": {{path}} }""").HealthPath);

    /// <summary>The three paths the page reads only when an app says it has them are nothing when the file gives
    /// nothing of them - and, unlike the three above, they have no default to fall back to: an app that names no
    /// telemetry path is an app that has none.</summary>
    [Theory]
    [MemberData(nameof(Nothing))]
    public void AnOptionalPathOfNothingIsNoPathAndNoError(string path)
    {
        var read = Deployable($$"""{ "telemetryPath": {{path}}, "buildPath": {{path}}, "healthDetailPath": {{path}} }""");

        Assert.Null(read.TelemetryPath);
        Assert.Null(read.BuildPath);
        Assert.Null(read.HealthDetailPath);
    }

    [Theory]
    [InlineData("\"_telemetry\"", "/_telemetry")]
    [InlineData("\" _build \"", "/_build")]
    [InlineData("\"healthcheck/detailed\"", "/healthcheck/detailed")]
    [InlineData("\"/already/here\"", "/already/here")]
    public void AnOptionalPathTakesItsSlashAndItsTrimButNoOtherHelp(string path, string expected)
    {
        var read = Deployable($$"""{ "telemetryPath": {{path}}, "buildPath": {{path}}, "healthDetailPath": {{path}} }""");

        Assert.Equal(expected, read.TelemetryPath);
        Assert.Equal(expected, read.BuildPath);
        Assert.Equal(expected, read.HealthDetailPath);
    }

    /// <summary>The traffic paths are the paths of the file in the order of the file, each with its front slash - and
    /// each as it was written: this is the one list of words the reader does not trim, so a path with a space before it
    /// is a path with a space before it, and the traffic button asks for that path.</summary>
    [Fact]
    public void TheTrafficPathsAreThePathsOfTheFileInTheirOrder()
    {
        var read = Deployable("""{ "trafficPaths": [ "/", "api/workorders/summary", "/_version" ] }""");

        Assert.Equal(["/", "/api/workorders/summary", "/_version"], read.TrafficPaths!);
        Assert.Equal(["/ api ", "/"], Deployable("""{ "trafficPaths": [ " api ", "/" ] }""").TrafficPaths!);
    }

    /// <summary>The traffic paths the file gives are read in the order of the file; the list that is <c>null</c> is no
    /// list at all and the list of no members is the empty one - the traffic button has nothing to send, which is not
    /// the same thing as having no traffic button.</summary>
    [Fact]
    public void TrafficPathsOfNothingAreNoTrafficPathsAndNoError()
    {
        Assert.Null(Deployable("""{ "trafficPaths": null }""").TrafficPaths);
        Assert.Empty(Deployable("""{ "trafficPaths": [] }""").TrafficPaths!);
    }

    [Theory]
    [MemberData(nameof(NotAnArray))]
    public void TrafficPathsThatAreNoListAreRefusedByTheirPlace(string paths) =>
        Assert.Equal(
            ["environments[0].deployables[0].trafficPaths: not an array."],
            InvalidDeployable($$"""{ "trafficPaths": {{paths}} }"""));

    /// <summary>A member of the list that is no path - a number, a truth, a <c>null</c>, a list, an object, a word of
    /// nothing - refuses the whole list, and says so once: the paths are read together or not at all, because half a
    /// plan of traffic is not a smaller probe but a different one.</summary>
    [Theory]
    [MemberData(nameof(Nothing))]
    public void ATrafficPathThatIsNoPathRefusesTheListOnce(string path) =>
        Assert.Equal(
            ["environments[0].deployables[0].trafficPaths: every entry must be a path."],
            InvalidDeployable($$"""{ "trafficPaths": [ {{path}} ] }"""));

    [Fact]
    public void TheFirstPathThatIsNoPathRefusesTheListAndTheRestIsNotEvenRead() =>
        Assert.Equal(
            ["environments[0].deployables[0].trafficPaths: every entry must be a path."],
            InvalidDeployable("""{ "trafficPaths": [ "/", 7, false, " ", [] ] }"""));

    // ----- nodes[] -----

    /// <summary>The address is the one word a node must have, and every shape but an http or https address is the
    /// absence of it - the member missing, <c>null</c>, an empty word, a word of spaces, another type of JSON. The
    /// place it stands in is named, and the file is refused.</summary>
    [Theory]
    [MemberData(nameof(Nothing))]
    public void ANodeWithoutAnAddressIsRefusedByItsPlace(string url) =>
        Assert.Equal(
            ["environments[0].deployables[0].nodes[0].url: missing or not an absolute http or https address."],
            InvalidDeployable($$"""{ "nodes": [ { "url": {{url}} } ] }"""));

    /// <summary>The address is trimmed before it is read and then held as an address is held: the scheme and the host
    /// in lower case, the path in the letters it was written in, and the port and the path kept.</summary>
    [Fact]
    public void TheAddressOfANodeIsTrimmedAndThenReadAsAnAddress()
    {
        var node = Node("""{ "url": "  HTTPS://UI.example.net:8443/App  " }""");

        Assert.Equal("https://ui.example.net:8443/App", node.Url.AbsoluteUri);
        Assert.Equal("ui.example.net", node.Url.Host);
        Assert.Equal(8443, node.Url.Port);
    }

    /// <summary>A node the file does not name is named by the host its address answers to - without its port, its path
    /// or its spaces - and a name of nothing at all is no name either.</summary>
    [Theory]
    [MemberData(nameof(Nothing))]
    public void ANodeWithoutANameTakesTheHostOfItsAddress(string name) =>
        Assert.Equal("ui.example.net", Node($$"""{ "name": {{name}}, "url": "https://ui.example.net:8443/app" }""").Name);

    [Fact]
    public void TheNameAndTheRegionOfANodeAreReadWithoutTheirSpaces()
    {
        var node = Node("""{ "name": "  uat-west  ", "region": " westus3 ", "url": "https://a.example.net" }""");

        Assert.Equal("uat-west", node.Name);
        Assert.Equal("westus3", node.Region);
    }

    /// <summary>A region of spaces is no region: the tile is drawn without the word rather than with two spaces in it.
    /// The name is not asked the same question, because a name of nothing takes the host of the address.</summary>
    [Theory]
    [MemberData(nameof(Nothing))]
    public void ARegionTheFileDoesNotGiveIsNoRegion(string region) =>
        Assert.Null(Node($$"""{ "url": "https://a.example.net", "region": {{region}} }""").Region);

    /// <summary>A node with no role takes the place it stands in as its role: the first is the primary
    /// (<see cref="NodeInfo.PrimaryRole" />), every other the standby (<see cref="NodeInfo.StandbyRole" />) - and a
    /// role of <c>null</c>, of spaces or of another type is as much no role as a role that is not there.</summary>
    [Theory]
    [MemberData(nameof(Nothing))]
    public void ANodeWithoutItsRoleTakesThePlaceItStandsIn(string role)
    {
        var nodes = Deployable($$"""
            { "nodes": [
              { "url": "https://one.example.net", "role": {{role}} },
              { "url": "https://two.example.net", "role": {{role}} },
              { "url": "https://three.example.net", "role": {{role}} } ] }
            """).Nodes;

        Assert.Equal([NodeInfo.PrimaryRole, NodeInfo.StandbyRole, NodeInfo.StandbyRole], nodes.Select(node => node.Role).ToArray());
        Assert.Equal([true, false, false], nodes.Select(node => node.IsPrimary).ToArray());
    }

    /// <summary>A role is the word it is written in, without its spaces and without its caps - and it stays that word:
    /// a deployable whose nodes are canary and blue has a canary and a blue, neither turned into a standby of somebody
    /// else and neither an error, and no tile of it is drawn as the primary.</summary>
    [Fact]
    public void ARoleIsReadWithoutItsSpacesAndItsCapsAndStaysTheWordItIs()
    {
        var nodes = Deployable("""
            { "nodes": [
              { "url": "https://a.example.net", "role": "  Canary  " },
              { "url": "https://b.example.net", "role": "BLUE" } ] }
            """).Nodes;

        Assert.Equal(["canary", "blue"], nodes.Select(node => node.Role).ToArray());
        Assert.All(nodes, node => Assert.False(node.IsPrimary));
    }

    /// <summary>The index of a node is the index in the file, so a standby named first is a standby: the place that
    /// decides a role is the place in the file, not the place among the nodes that have no role of their own.</summary>
    [Fact]
    public void ThePlaceThatDecidesARoleIsThePlaceInTheFile()
    {
        var nodes = Deployable("""
            { "nodes": [
              { "url": "https://a.example.net", "role": "standby" },
              { "url": "https://b.example.net" } ] }
            """).Nodes;

        Assert.Equal(["standby", "standby"], nodes.Select(node => node.Role).ToArray());
        Assert.All(nodes, node => Assert.False(node.IsPrimary));
    }

    /// <summary>A list of nodes that is <c>null</c> is the empty list it describes: a deployable that names no node is
    /// read all the same, with nothing to probe.</summary>
    [Fact]
    public void NodesOfNothingAreNoNodesAndNoError() =>
        Assert.Empty(Deployable("""{ "nodes": null }""").Nodes);

    [Theory]
    [MemberData(nameof(NotAnArray))]
    public void AListOfNodesThatIsNoListIsRefusedByItsPlace(string nodes) =>
        Assert.Equal(
            ["environments[0].deployables[0].nodes: not an array."],
            InvalidDeployable($$"""{ "nodes": {{nodes}} }"""));

    // ----- environments[].pages -----

    /// <summary>The pages of an environment are the one list that is a courtesy all the way down: a member of another
    /// shape than an array is no pages, and says nothing about it - like a link, and unlike the traffic paths, which
    /// are read by the page rather than clicked on by a person.</summary>
    [Theory]
    [InlineData("{}")]
    [InlineData("7")]
    [InlineData("true")]
    [InlineData("\"Scorecard\"")]
    [InlineData("null")]
    public void PagesOfAnotherShapeAreNoPagesAndNoError(string pages) =>
        Assert.Null(Pages(pages));

    [Fact]
    public void APageOfNothingIsLeftOutAndTheRestOfThemKeepTheirOrder()
    {
        var pages = Pages("""
            [ { "name": " ", "url": "https://a.example.net/" },
              { "name": "No address", "url": null },
              { "name": "Spaces for an address", "url": "   " },
              { "name": "A page of its own", "url": "https://b.example.net/", "deployable": 7 },
              { "name": "Scorecard", "url": "https://c.example.net/scorecard", "deployable": " scorecard " } ]
            """)!;

        Assert.Equal(
            [
                new PageLink("A page of its own", new Uri("https://b.example.net/")),
                new PageLink("Scorecard", new Uri("https://c.example.net/scorecard"), "scorecard"),
            ],
            pages);
    }

    /// <summary>Pages none of which has both its words and its address are no pages at all: the environment is drawn
    /// without the group rather than with an empty one.</summary>
    [Fact]
    public void PagesOfNothingToKeepAreNoPages() =>
        Assert.Null(Pages("""[ { "name": "No address" }, { "url": "https://a.example.net/" }, 7, null, [], "scorecard" ]"""));

    // ----- cluster -----

    /// <summary>A <c>cluster</c> that is neither absent nor <c>null</c> nor an object is refused by its name, whatever
    /// shape it comes in: the page has no cluster view to draw and says which part of the file stopped it.</summary>
    [Theory]
    [InlineData("7")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("\"aks-cmdemo1\"")]
    [InlineData("[]")]
    [InlineData("[ { \"name\": \"aks\" } ]")]
    public void AClusterThatIsNoObjectIsRefusedByItsName(string cluster) =>
        Assert.Equal("cluster: not an object.", Assert.Single(Errors($$"""{ "environments": [], "cluster": {{cluster}} }""")));

    /// <summary>The links of a cluster are read under two keys and only those two: a <c>links</c> of another shape is
    /// no links, and a key the page does not read is no links either, however good its address is. None of them is an
    /// error, because a cluster without a portal link is a cluster the page can still show.</summary>
    [Theory]
    [InlineData("""{ "links": [] }""")]
    [InlineData("""{ "links": "portal" }""")]
    [InlineData("""{ "links": 7 }""")]
    [InlineData("""{ "links": { "grafana": "https://grafana.example.net/" } }""")]
    public void LinksTheClusterDoesNotReadAreNoLinksAndNoError(string links)
    {
        var cluster = Valid($$"""{ "environments": [], "cluster": {{links}} }""").Cluster!;

        Assert.Null(cluster.Links);
        Assert.Equal(new ClusterInfo(null), cluster);
    }

    /// <summary>A key the page knows is read and a key it does not is passed over, in the same object and in the order
    /// of the two names the page asks for - so a cluster that knows its workloads is shown a link for them and says
    /// nothing about Grafana.</summary>
    [Fact]
    public void OnlyTheTwoLinksThePageKnowsComeOutOfTheCluster() =>
        Assert.Equal(
            [LinkSet.Workloads],
            Valid("""
                { "environments": [], "cluster": { "links": {
                    "grafana": "https://grafana.example.net/",
                    "workloads": "https://portal.example.net/#workloads" } } }
                """).Cluster!.Links!.Keys);

    [Fact]
    public void TheNameOfTheClusterIsReadWithoutItsSpaces() =>
        Assert.Equal("aks-cmdemo1", TopologyReading.Cluster("""{ "name": "  aks-cmdemo1  " }""")!.Name);

    // ----- the result itself -----

    /// <summary>The messages a failure carries are the messages, in the order they were given, none of them lost -
    /// which is what the page prints, one under the other, when there is no topology to show.</summary>
    [Fact]
    public void AFailureKeepsEveryMessageItWasGivenInItsOrder()
    {
        var result = TopologyParseResult.Failed(
            "The server answered HTTP 502 for topology.json.",
            "environments[0].name: missing or empty.",
            "cluster: not an object.");

        Assert.False(result.IsValid);
        Assert.Null(result.Topology);
        Assert.Equal(
            [
                "The server answered HTTP 502 for topology.json.",
                "environments[0].name: missing or empty.",
                "cluster: not an object.",
            ],
            result.Errors);
    }

    /// <summary>The two parts of a result come back out in the order they went in, whether the read succeeded or
    /// failed: the page asks one and prints the other.</summary>
    [Fact]
    public void TheTwoPartsOfAResultComeBackOutInTheirOrder()
    {
        var (topology, errors) = TopologyParser.Parse("""{ "environments": [] }""");

        Assert.NotNull(topology);
        Assert.Empty(errors);

        var (read, messages) = TopologyParseResult.Failed("The file is empty.");

        Assert.Null(read);
        Assert.Equal("The file is empty.", Assert.Single(messages));
    }

    /// <summary>A read that succeeded carries the empty list, not nothing: the page counts the messages of a failure
    /// and never has to ask whether there are any.</summary>
    [Fact]
    public void ASuccessfulReadCarriesNoMessagesRatherThanNoListOfThem()
    {
        var result = TopologyParser.Parse("""{ "environments": [] }""");

        Assert.NotNull(result.Errors);
        Assert.Empty(result.Errors);
        Assert.True(result.IsValid);
    }

    // ----- the surface of the two types -----

    /// <summary>The parser has exactly one way in, and it is the one that needs neither a network nor a clock: a
    /// static read of the text, handing back the record. Nothing of it waits for an answer and nothing of it waits for
    /// a moment - so every case above is a string the test wrote and a value the parser answered with.</summary>
    [Fact]
    public void TheParserHasTheOneWayInAndNothingThatCallsOrWaits()
    {
        var parser = typeof(TopologyParser);

        Assert.True(parser.IsAbstract);
        Assert.True(parser.IsSealed);
        Assert.Empty(parser.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.Empty(parser.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Empty(parser.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Empty(parser.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));

        var parse = Assert.Single(parser.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));

        Assert.Equal("Parse", parse.Name);
        Assert.True(parse.IsPublic);
        Assert.True(parse.IsStatic);
        Assert.Equal(typeof(TopologyParseResult), parse.ReturnType);
        Assert.Equal([typeof(string)], parse.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(["json"], parse.GetParameters().Select(parameter => parameter.Name));
        Assert.DoesNotContain("Task", parse.ReturnType.Name, StringComparison.Ordinal);
        Assert.DoesNotContain(parse.GetParameters(), parameter => parameter.ParameterType == typeof(CancellationToken));
        Assert.DoesNotContain(parse.GetParameters(), parameter => parameter.ParameterType == typeof(HttpClient));
    }

    /// <summary>The result is the record it looks like: sealed, holding a topology that may be nothing and the
    /// messages that say why it is, with the one way the loaders make one and the one question the page asks.</summary>
    [Fact]
    public void TheResultHoldsATopologyAndTheMessagesAndAsksForNothingElse()
    {
        var result = typeof(TopologyParseResult);
        var properties = result.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.True(result.IsSealed);
        Assert.Equal(["Errors", "IsValid", "Topology"], properties.Select(property => property.Name).Order().ToArray());
        Assert.All(properties, property => Assert.NotNull(property.GetMethod));
        Assert.Equal(
            [typeof(Topology), typeof(IReadOnlyList<string>)],
            [.. properties.Where(property => property.Name != "IsValid").Select(property => property.PropertyType)]);
        Assert.Equal(typeof(bool), properties.Single(property => property.Name == "IsValid").PropertyType);

        // The one way the loaders make a failure, and the one word the page asks: the params of the making, named as
        // the record's two parts and the messages that stand in for the topology it does not hold.
        var constructor = Assert.Single(result.GetConstructors(BindingFlags.Public | BindingFlags.Instance));

        Assert.Equal(["Topology", "Errors"], constructor.GetParameters().Select(parameter => parameter.Name));

        var methods = result.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.Equal(
            ["Failed", "op_Equality", "op_Inequality"],
            methods.Select(method => method.Name).Order().ToArray());
        // The two operators are the compiler's, marked as the symbols they stand for; the one member this record
        // declares for itself is the way a caller says "there is no topology, and here is why".
        Assert.Equal(["Failed"], methods.Where(method => !method.IsSpecialName).Select(method => method.Name));

        var failed = Assert.Single(methods, method => method.Name == "Failed");
        var errors = Assert.Single(failed.GetParameters());

        Assert.True(failed.IsStatic);
        Assert.True(failed.IsPublic);
        Assert.Equal(typeof(TopologyParseResult), failed.ReturnType);
        Assert.Equal(typeof(string[]), errors.ParameterType);
        Assert.Equal("errors", errors.Name);
        Assert.True(errors.IsDefined(typeof(ParamArrayAttribute), false));
    }
}
