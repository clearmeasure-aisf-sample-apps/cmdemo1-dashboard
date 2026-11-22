using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>
/// The boundaries of the records <c>src/Dashboard/Runtime/RuntimePayload.cs</c> declares, at the ends its other files
/// leave open: what the writer makes of a part rather than the part itself. A payload decides nothing of its own - no
/// state, no word, no address, no moment - so what stands here is the bytes one update is turned into: the words that
/// need escaping and the reader that speaks another language, the list that holds nothing in a slot, the one link the
/// update hands over in four places, the height that is no finite number, the diagram of a thousand boxes, and the
/// bytes read back through the same context the page was compiled with.
/// <para>
/// What is already pinned is left alone. <see cref="RuntimePayloadEdgeCaseTests" /> pins every part of every record
/// coming back as the part it went in as, the lists held rather than copied or weighed, the parts a caller may leave
/// out standing absent, the empty word that is still a word, the members every record is asked - compare, hash, fall
/// apart, copy and say the parts - and the whole public surface of each record; it also pins a tile's parts written
/// under their own names, the nothing left out and the empty list written, the address a <see cref="Uri" /> escapes
/// and normalises into a link, and the words a line is made of. <see cref="RuntimeTrendEdgeCaseTests" /> and
/// <see cref="RuntimeTrendAndNameTests" /> pin <see cref="RuntimeTrend" /> and <see cref="RuntimeName" /> whole, the
/// copy out of a health trend, the heights as they stand, a height that is no number at all being refused by the
/// writer, and the empty list of names being written while no list of them is left out.
/// <see cref="RuntimeNameEdgeCaseTests" /> pins the writer's escapes of a name's own two parts,
/// <see cref="LinkTests" /> pins that a line of pieces is its pieces in a row and that the pieces are kept only when
/// one of them leads, and <see cref="RuntimePayloadBuilderTests" /> pins the keys of the payload a sample manifest
/// builds.
/// </para>
/// <para>
/// What follows is therefore the escape of every other record in one update, the same bytes to a reader of German, the
/// two heights that are no finite number and the three that are negative zero, enormous or tiny, a list that holds
/// nothing where a part should be, one link written in full wherever it is handed, a thousand boxes in the order they
/// were given, the words a line is given rather than the words it is made of, the addresses a <see cref="Uri" /> says
/// of the forms a builder never builds, the two operators of every record against nothing, and an update read back out
/// of its own bytes.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every value below is one the test brought, and no
/// payload is built out of a manifest.
/// </para>
/// <para>
/// The name: the record set's own boundary file was written by an earlier turn of this work as
/// <see cref="RuntimePayloadEdgeCaseTests" />, so this is the companion the repo already uses where a type is asked
/// twice - <see cref="RuntimeManifestBoundaryEdgeCaseTests" />, <see cref="ServingStateBoundaryEdgeCaseTests" />,
/// <see cref="DeployableStatusBoundaryEdgeCaseTests" /> and <see cref="DeploymentBoundaryEdgeCaseTests" /> - and it
/// keeps to what those files leave open rather than restating them.
/// </para>
/// </summary>
public class RuntimePayloadBoundaryEdgeCaseTests
{
    private const string TaskUrl = "https://octopus.example.net/app#/Spaces-1/tasks/ServerTasks-1";
    private const string TaskWords = "The deployment's task in Octopus Deploy (opens in a new tab; Octopus Deploy asks you to sign in)";

    /// <summary>An address and some words that between them hold every character the writer thinks a page might open a
    /// tag with: an angle bracket, an ampersand, a quote, an apostrophe, a letter outside ASCII and a snowman.</summary>
    private const string AwkwardUrl = "https://octopus.example.net/app?spm=a&x=1#/Spaces-1/tasks/ServerTasks-1";
    private const string AwkwardWords = "The <deployment's> \"task\" & the sign-in";

    /// <summary>The raw form the writer gives that address: the <c>&amp;</c> of its query escaped, its fragment left
    /// where the file that named it put it, and its words escaped through the apostrophe, the brackets and the quotes.</summary>
    private const string AwkwardHref = """{"href":"https://octopus.example.net/app?spm=a\u0026x=1#/Spaces-1/tasks/ServerTasks-1","title":"The \u003Cdeployment\u0027s\u003E \u0022task\u0022 \u0026 the sign-in"}""";

    private static readonly RuntimeLink Awkward = new(AwkwardUrl, AwkwardWords);
    private static readonly RuntimeLink ToTask = new(TaskUrl, TaskWords);

    /// <summary>A line whose every part needs escaping: its words, the place one of its pieces leads, and the tooltip
    /// of the one mark of a detailed health check it carries.</summary>
    private static readonly RuntimeTileLine AwkwardLine = new(
        "pinned 2.4.43: in \"sync\"", "insync",
        [new RuntimeTextPart("pinned 2.4.43: in \"sync\" "), new RuntimeTextPart("& differs", Awkward)],
        null, [new RuntimeCheckMark("healthy", "API: Healthy <200 OK> \"once\" & twice")]);

    /// <summary>A tile that says nothing but awkward words, and carries a deployment whose sentence and link need
    /// escaping as well.</summary>
    private static RuntimeTile AwkwardTile => new(
        "app_ui_primary", "healthy", "</script>", "HTTP 200\t\u00B7 85 ms\n", [AwkwardLine], ["healthy", "<checking>"],
        "cmdemo2-ui primary: \u2603 \U0001F600 \u00E9", Awkward, null,
        new RuntimeDeployment("executing", "deploying \"demo & co\" to uat", Awkward));

    /// <summary>One update that holds every record of the file at once, each of them saying words the writer has to
    /// escape: a tile, a line of pieces with a mark, a region's mark, and a relationship's mark with a sparkline.</summary>
    private static RuntimePayload AwkwardUpdate => new(
        [AwkwardTile],
        [new RuntimeRegionMark("region_primary", "serving", "serving traffic: <1 region>", Awkward)],
        [new RuntimeEdgeMark("fd_ui-to-app_ui_primary", "active", "12", "req/min", "</script>", "The Front Door & the web app", Awkward, new RuntimeTrend([0.5], "0.5 req/min"))]);

    /// <summary>The bytes of one update, as the script reads them.</summary>
    private static JsonDocument Written(RuntimePayload payload) => JsonDocument.Parse(payload.ToJson());

    /// <summary>An update of nothing but one relationship's mark, which is how these tests hand the writer a trend.</summary>
    private static RuntimePayload OfTrend(RuntimeTrend trend) => new([], [], [new RuntimeEdgeMark("e1", "active", null, null, null, "t", null, trend)]);

    /// <summary>Every property of what the writer made, the ones of a nested object and of an object inside a list
    /// alike, so that a test can say that no reference is hidden anywhere in an update.</summary>
    private static IEnumerable<JsonProperty> EveryPropertyOf(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject().SelectMany(property => new JsonProperty[] { property }.Concat(EveryPropertyOf(property.Value))),
            JsonValueKind.Array => element.EnumerateArray().SelectMany(EveryPropertyOf),
            _ => [],
        };

    // ---------------------------------------------------------------------------- the bytes the writer makes

    /// <summary>The bytes of one update are the bytes the writer makes and not the ones the reader speaks: to a page
    /// reading German, a height of <c>1.25</c> is still written with the point the script parses, the comma a builder
    /// put inside a word is still that comma, and every key is still the camelCase name the record declares - while
    /// <c>1.25</c> said in German really is <c>1,25</c>, so that the run is in the culture it claims to be in.</summary>
    [Fact]
    public void TheBytesOfAnUpdateAreTheSameWordsWhateverTheReaderSpeaks()
    {
        var points = new double[] { 0, 0.5, 1.25 };
        var lines = new List<RuntimeTileLine> { new("1.25 req/min", "plain", null, new RuntimeTrend(points, "Requests per minute: 0 to 1.25, now 0.5")) };
        var nodes = new List<RuntimeTile> { new("app_ui_primary", "healthy", "Healthy", "HTTP 200 \u00B7 85 ms", lines, new List<string> { "healthy", "checking" }, "cmdemo2-ui primary: healthy.") };
        var edges = new List<RuntimeEdgeMark> { new("fd_ui-to-app_ui_primary", "active", "1.25", "req/min", "1,25 calls", "The Front Door to the web app.", null, new RuntimeTrend(points, "0,5 to 1,25")) };
        var update = new RuntimePayload(nodes, new List<RuntimeRegionMark>(), edges);
        var english = update.ToJson();

        Assert.Equal(
            """{"nodes":[{"alias":"app_ui_primary","state":"healthy","label":"Healthy","facts":"HTTP 200 \u00B7 85 ms","lines":[{"text":"1.25 req/min","tone":"plain","trend":{"points":[0,0.5,1.25],"title":"Requests per minute: 0 to 1.25, now 0.5"}}],"history":["healthy","checking"],"title":"cmdemo2-ui primary: healthy."}],"regions":[],"edges":[{"id":"fd_ui-to-app_ui_primary","state":"active","number":"1.25","unit":"req/min","text":"1,25 calls","title":"The Front Door to the web app.","trend":{"points":[0,0.5,1.25],"title":"0,5 to 1,25"}}]}""",
            english);

        ViewerCulture.German(() =>
        {
            // The reader really does speak German, and the update handed to it does not change by a byte.
            Assert.Equal("1,25", (1.25).ToString(CultureInfo.CurrentCulture));
            Assert.Equal(english, update.ToJson());
        });

        using var written = Written(update);

        Assert.Equal("""{"points":[0,0.5,1.25],"title":"0,5 to 1,25"}""", written.RootElement.GetProperty("edges")[0].GetProperty("trend").GetRawText());
        Assert.Equal(
            """{"text":"1.25 req/min","tone":"plain","trend":{"points":[0,0.5,1.25],"title":"Requests per minute: 0 to 1.25, now 0.5"}}""",
            written.RootElement.GetProperty("nodes")[0].GetProperty("lines")[0].GetRawText());
    }

    /// <summary>Every word of an update goes to the script escaped, whichever record carries it and whichever part of
    /// it it is: an angle bracket, an ampersand, a quote and an apostrophe are written as the writer writes them, a
    /// letter outside ASCII and a snowman as the bytes they are, a tab and a newline as the two escapes - and read
    /// back they are the words that went in, so that a sentence out of a task log can neither close a tag nor end the
    /// update that carries it.</summary>
    [Fact]
    public void EveryWordOfAnUpdateIsWrittenEscapedAndReadBackAsItWentIn()
    {
        var bytes = AwkwardUpdate.ToJson();

        // Nothing that could open or close anything is left standing in the bytes, and no letter outside ASCII either.
        Assert.DoesNotContain("<", bytes, StringComparison.Ordinal);
        Assert.DoesNotContain(">", bytes, StringComparison.Ordinal);
        Assert.DoesNotContain("&", bytes, StringComparison.Ordinal);
        Assert.DoesNotContain("'", bytes, StringComparison.Ordinal);
        Assert.DoesNotContain("</script>", bytes, StringComparison.Ordinal);
        Assert.DoesNotContain("\u2603", bytes, StringComparison.Ordinal);
        Assert.DoesNotContain("\U0001F600", bytes, StringComparison.Ordinal);
        Assert.DoesNotContain("\u00E9", bytes, StringComparison.Ordinal);
        Assert.DoesNotContain("\u00B7", bytes, StringComparison.Ordinal);

        using var written = Written(AwkwardUpdate);
        var node = written.RootElement.GetProperty("nodes")[0];
        var line = node.GetProperty("lines")[0];

        Assert.Equal(""" "\u003C/script\u003E" """.Trim(), node.GetProperty("label").GetRawText());
        Assert.Equal(""" "HTTP 200\t\u00B7 85 ms\n" """.Trim(), node.GetProperty("facts").GetRawText());
        Assert.Equal(""" "cmdemo2-ui primary: \u2603 \uD83D\uDE00 \u00E9" """.Trim(), node.GetProperty("title").GetRawText());
        Assert.Equal("""["healthy","\u003Cchecking\u003E"]""", node.GetProperty("history").GetRawText());
        Assert.Equal("""{"state":"healthy","title":"API: Healthy \u003C200 OK\u003E \u0022once\u0022 \u0026 twice"}""", line.GetProperty("marks")[0].GetRawText());
        Assert.Equal(AwkwardHref, node.GetProperty("link").GetRawText());
        Assert.Equal(
            """{"state":"executing","title":"deploying \u0022demo \u0026 co\u0022 to uat","link":{"href":"https://octopus.example.net/app?spm=a\u0026x=1#/Spaces-1/tasks/ServerTasks-1","title":"The \u003Cdeployment\u0027s\u003E \u0022task\u0022 \u0026 the sign-in"}}""",
            node.GetProperty("deployment").GetRawText());
        Assert.Equal(
            """{"alias":"region_primary","state":"serving","label":"serving traffic: \u003C1 region\u003E","nameLink":{"href":"https://octopus.example.net/app?spm=a\u0026x=1#/Spaces-1/tasks/ServerTasks-1","title":"The \u003Cdeployment\u0027s\u003E \u0022task\u0022 \u0026 the sign-in"}}""",
            written.RootElement.GetProperty("regions")[0].GetRawText());
        Assert.Equal(
            """{"text":"\u0026 differs","link":{"href":"https://octopus.example.net/app?spm=a\u0026x=1#/Spaces-1/tasks/ServerTasks-1","title":"The \u003Cdeployment\u0027s\u003E \u0022task\u0022 \u0026 the sign-in"}}""",
            line.GetProperty("parts")[1].GetRawText());
        Assert.Equal(
            """{"id":"fd_ui-to-app_ui_primary","state":"active","number":"12","unit":"req/min","text":"\u003C/script\u003E","title":"The Front Door \u0026 the web app","link":{"href":"https://octopus.example.net/app?spm=a\u0026x=1#/Spaces-1/tasks/ServerTasks-1","title":"The \u003Cdeployment\u0027s\u003E \u0022task\u0022 \u0026 the sign-in"},"trend":{"points":[0.5],"title":"0.5 req/min"}}""",
            written.RootElement.GetProperty("edges")[0].GetRawText());

        // Read back, the words are the ones that went in - the escapes are the writer's and not the record's.
        Assert.Equal("</script>", node.GetProperty("label").GetString());
        Assert.Equal("HTTP 200\t\u00B7 85 ms\n", node.GetProperty("facts").GetString());
        Assert.Equal("cmdemo2-ui primary: \u2603 \U0001F600 \u00E9", node.GetProperty("title").GetString());
        Assert.Equal("<checking>", node.GetProperty("history")[1].GetString());
        Assert.Equal("pinned 2.4.43: in \"sync\"", line.GetProperty("text").GetString());
        Assert.Equal("& differs", line.GetProperty("parts")[1].GetProperty("text").GetString());
        Assert.Equal("API: Healthy <200 OK> \"once\" & twice", line.GetProperty("marks")[0].GetProperty("title").GetString());
        Assert.Equal(AwkwardWords, node.GetProperty("link").GetProperty("title").GetString());
        Assert.Equal(AwkwardUrl, node.GetProperty("link").GetProperty("href").GetString());
        Assert.Equal("deploying \"demo & co\" to uat", node.GetProperty("deployment").GetProperty("title").GetString());
        Assert.Equal("serving traffic: <1 region>", written.RootElement.GetProperty("regions")[0].GetProperty("label").GetString());
        Assert.Equal("The Front Door & the web app", written.RootElement.GetProperty("edges")[0].GetProperty("title").GetString());
        Assert.Equal("</script>", written.RootElement.GetProperty("edges")[0].GetProperty("text").GetString());

        // And the escaped update is that update, twice over: the writer keeps no state of its own between the calls.
        Assert.Equal(bytes, AwkwardUpdate.ToJson());
    }

    /// <summary>A height that is no finite number cannot be written at all - the writer refuses the whole update rather
    /// than hand the script an <c>Infinity</c> it cannot parse, which is the same refusal as the <c>NaN</c> another
    /// file pins - while the heights that are numbers are written exactly as they stand: a negative zero with its
    /// sign, the biggest and the smallest the type reaches in the writer's own short form.</summary>
    [Fact]
    public void AHeightThatIsNoFiniteNumberIsRefusedAndTheRestAreWrittenAsTheyStand()
    {
        var above = new double[] { double.PositiveInfinity };
        var below = new double[] { double.NegativeInfinity };
        var mixed = new double[] { 0, double.NegativeInfinity, 1 };

        Assert.Throws<ArgumentException>(() => OfTrend(new RuntimeTrend(above, "requests/min")).ToJson());
        Assert.Throws<ArgumentException>(() => OfTrend(new RuntimeTrend(below, "requests/min")).ToJson());
        Assert.Throws<ArgumentException>(() => OfTrend(new RuntimeTrend(mixed, "requests/min")).ToJson());

        // The refusal writes no part of an update: the script is handed nothing rather than half a diagram.
        var boxes = new List<RuntimeTile> { new("app_ui_primary", "healthy", "Healthy", null, [], null, "t") };
        var edges = new List<RuntimeEdgeMark> { new("e1", "active", null, null, null, "t", null, new RuntimeTrend(above, "requests/min")) };
        var partly = new RuntimePayload(boxes, new List<RuntimeRegionMark>(), edges);

        Assert.Throws<ArgumentException>(() => partly.ToJson());

        var finite = new double[] { -0.0, 1e300, 5e-324, 0.5, 1 };

        using var written = Written(OfTrend(new RuntimeTrend(finite, "requests/min")));
        var points = written.RootElement.GetProperty("edges")[0].GetProperty("trend").GetProperty("points");

        Assert.Equal("""{"points":[-0,1E+300,5E-324,0.5,1],"title":"requests/min"}""", written.RootElement.GetProperty("edges")[0].GetProperty("trend").GetRawText());
        Assert.Equal(new double[] { -0.0, 1e300, 5e-324, 0.5, 1 }, points.EnumerateArray().Select(point => point.GetDouble()).ToList());
        Assert.True(double.IsNegative(points[0].GetDouble()));
        Assert.Equal(5, points.GetArrayLength());

        // The record itself refuses neither: a height is a number the health side weighed, and it is the writer that
        // says whether the script can read it.
        var held = new RuntimeTrend(above, "requests/min");

        Assert.Same(above, held.Points);
        Assert.Equal("requests/min", held.Title);
        Assert.Equal(held, new RuntimeTrend(above, "requests/min"));
        Assert.Single(held.Points);
    }

    /// <summary>The very same link an update hands over in four places is written in full in every one of them: the
    /// context beside the record keeps no references, so the script never sees an <c>$id</c> to look up or a
    /// <c>$ref</c> to follow, and the same box written twice is the same bytes twice rather than one box and a
    /// pointer to it.</summary>
    [Fact]
    public void TheVerySameLinkIsWrittenInFullWhereverTheUpdateHandsIt()
    {
        var parts = new List<RuntimeTextPart> { new("2.4.43 ", Awkward), new("reaches uat") };
        var lines = new List<RuntimeTileLine> { new("2.4.43 reaches uat", "deploying", parts) };
        var tile = new RuntimeTile("app_ui_primary", "healthy", "Healthy", null, lines, null, "cmdemo2-ui primary: healthy.", Awkward, Awkward, new RuntimeDeployment("executing", "deploying cmdemo2-ui 2.4.43 to uat", Awkward));
        var nodes = new List<RuntimeTile> { tile, tile };
        var regions = new List<RuntimeRegionMark> { new("region_primary", "serving", "serving traffic", Awkward) };
        var update = new RuntimePayload(nodes, regions, new List<RuntimeEdgeMark>());
        var bytes = update.ToJson();

        using var written = JsonDocument.Parse(bytes);
        var properties = EveryPropertyOf(written.RootElement).ToList();

        // Nine places for the address, and no back reference anywhere among them.
        Assert.Equal(9, properties.Count(property => property.NameEquals("href")));
        Assert.All(properties.Where(property => property.NameEquals("href")), property => Assert.Equal(AwkwardUrl, property.Value.GetString()));
        Assert.DoesNotContain(properties, property => property.Name.StartsWith('$'));
        Assert.DoesNotContain("$id", bytes, StringComparison.Ordinal);
        Assert.DoesNotContain("$ref", bytes, StringComparison.Ordinal);
        Assert.DoesNotContain("$values", bytes, StringComparison.Ordinal);

        // The box written twice is written the same way twice, and the piece that leads carries a whole link too.
        var boxes = written.RootElement.GetProperty("nodes");

        Assert.Equal(boxes[0].GetRawText(), boxes[1].GetRawText());
        Assert.Equal(AwkwardHref, boxes[0].GetProperty("lines")[0].GetProperty("parts")[0].GetProperty("link").GetRawText());
        Assert.Equal(AwkwardHref, boxes[0].GetProperty("nameLink").GetRawText());
        Assert.Equal(AwkwardHref, written.RootElement.GetProperty("regions")[0].GetProperty("nameLink").GetRawText());

        // It is the very link in every place, and the update written again is the same bytes again.
        Assert.Same(Awkward, tile.Link);
        Assert.Same(Awkward, tile.NameLink);
        Assert.Same(Awkward, tile.Deployment!.Link);
        Assert.Same(Awkward, tile.Lines[0].Parts![0].Link);
        Assert.Equal(bytes, update.ToJson());
    }

    /// <summary>A list the payload holds may carry nothing in one of its slots, and no record looks inside or refuses
    /// it: the writer puts a <c>null</c> exactly where a slot is nothing and writes the parts beside it as they stand -
    /// which is what a page has to read as a slot nobody filled, and what no builder of this deployment ever makes.</summary>
    [Fact]
    public void AListOfThePayloadThatHoldsNothingInItsPlaceIsWrittenWithNothingInItsPlace()
    {
        var marks = new List<RuntimeCheckMark> { null!, new("failed", "DataAccess: Unhealthy.") };
        var parts = new List<RuntimeTextPart> { null!, new("p") };
        var lines = new List<RuntimeTileLine> { null!, new("x", "plain", parts, null, marks) };
        var history = new List<string> { null!, "healthy" };
        var tile = new RuntimeTile("a", "neutral", "N", null, lines, history, "t");

        Assert.Same(lines, tile.Lines);
        Assert.Same(history, tile.History);
        Assert.Same(parts, tile.Lines[1].Parts);
        Assert.Same(marks, tile.Lines[1].Marks);
        Assert.Null(tile.Lines[0]);
        Assert.Null(tile.History![0]);
        Assert.Null(tile.Lines[1].Parts![0]);
        Assert.Null(tile.Lines[1].Marks![0]);

        Assert.Equal(
            """{"nodes":[{"alias":"a","state":"neutral","label":"N","lines":[null,{"text":"x","tone":"plain","parts":[null,{"text":"p"}],"marks":[null,{"state":"failed","title":"DataAccess: Unhealthy."}]}],"history":[null,"healthy"],"title":"t"}],"regions":[],"edges":[]}""",
            new RuntimePayload(new List<RuntimeTile> { tile }, new List<RuntimeRegionMark>(), new List<RuntimeEdgeMark>()).ToJson());

        // The four lists of the update itself behave the same way: a box that is nothing is a null in the row of them.
        Assert.Equal("""{"nodes":[null,null],"regions":[null],"edges":[null]}""", new RuntimePayload(new List<RuntimeTile> { null!, null! }, new List<RuntimeRegionMark> { null! }, new List<RuntimeEdgeMark> { null! }).ToJson());
        Assert.Equal("""{"nodes":[],"regions":[],"edges":[],"names":[null]}""", new RuntimePayload(new List<RuntimeTile>(), new List<RuntimeRegionMark>(), new List<RuntimeEdgeMark>(), new List<RuntimeName> { null! }).ToJson());
    }

    /// <summary>The order of a list is the order the script draws in, and it survives the writer whole: a thousand
    /// boxes fall out as the thousand they went in as, each under its own alias, a region and a relationship keeping
    /// their places beside them, and the hundred checks of one node their hundred states oldest first.</summary>
    [Fact]
    public void AThousandBoxesAreWrittenInTheOrderThePayloadWasGivenThem()
    {
        var named = new string[1_000];
        var boxes = new List<RuntimeTile>(named.Length);
        var checks = new List<string>(Enumerable.Repeat("healthy", 99).Append("unreachable"));

        for (var i = 0; i < named.Length; i++)
        {
            named[i] = "n" + i.ToString(CultureInfo.InvariantCulture);
            boxes.Add(new RuntimeTile(named[i], "neutral", "Neutral", null, new List<RuntimeTileLine> { new("not probed") }, i == 7 ? checks : null, "box"));
        }

        var regions = new List<RuntimeRegionMark> { new("r0", "serving", "serving"), new("r1", "standby", "standby"), new("r2", "down", "down") };
        var edges = new List<RuntimeEdgeMark> { new("e0", "active", null, null, null, "t"), new("e1", "idle", null, null, null, "t"), new("e2", "down", null, null, null, "t") };
        var update = new RuntimePayload(boxes, regions, edges);

        using var written = Written(update);
        var nodes = written.RootElement.GetProperty("nodes");

        Assert.Equal(1_000, nodes.GetArrayLength());
        Assert.Equal(named, nodes.EnumerateArray().Select(node => node.GetProperty("alias").GetString()).ToList());
        Assert.Equal("n0", nodes[0].GetProperty("alias").GetString());
        Assert.Equal("n500", nodes[500].GetProperty("alias").GetString());
        Assert.Equal("n999", nodes[999].GetProperty("alias").GetString());
        Assert.False(nodes[0].TryGetProperty("history", out _));
        Assert.Equal(100, nodes[7].GetProperty("history").GetArrayLength());
        Assert.Equal("healthy", nodes[7].GetProperty("history")[0].GetString());
        Assert.Equal("unreachable", nodes[7].GetProperty("history")[99].GetString());
        Assert.Equal(["r0", "r1", "r2"], written.RootElement.GetProperty("regions").EnumerateArray().Select(region => region.GetProperty("alias").GetString()));
        Assert.Equal(["e0", "e1", "e2"], written.RootElement.GetProperty("edges").EnumerateArray().Select(edge => edge.GetProperty("id").GetString()));
        Assert.False(written.RootElement.TryGetProperty("names", out _));
        Assert.Equal(1_000, update.Nodes.Count);
    }

    // ------------------------------------------------------------------- the two factories, at their open ends

    /// <summary>A line is given its own words and keeps them: it is never worked out of the pieces a caller hands
    /// beside it, and a piece that says nothing adds nothing to the row of them - not the word <c>null</c>, not a
    /// space. The pieces are still kept when one of them leads, whichever one it is, and the line of a thousand
    /// pieces of one letter is a thousand letters of words and no pieces at all, since none of them leads anywhere.</summary>
    [Fact]
    public void ALineIsGivenItsOwnWordsAndKeepsThemAndNotTheOnesItsPiecesSay()
    {
        var whole = new RuntimeTileLine("whole", "muted", new List<RuntimeTextPart> { new("a"), new("b") });

        Assert.Equal("whole", whole.Text);
        Assert.Equal(2, whole.Parts!.Count);
        Assert.Equal("ab", string.Concat(whole.Parts.Select(part => part.Text)));
        Assert.Equal("""{"text":"whole","tone":"muted","parts":[{"text":"a"},{"text":"b"}]}""", Bytes(whole));

        // A piece of no words is a piece of no words: the line made of them says nothing, and not "null".
        var silent = RuntimeTileLine.Of("muted", new List<RuntimeTextPart> { new(null!), new(string.Empty) });

        Assert.Equal(string.Empty, silent.Text);
        Assert.Null(silent.Parts);
        Assert.Equal("muted", silent.Tone);
        Assert.Null(silent.Trend);
        Assert.Null(silent.Marks);
        Assert.Equal("""{"text":"","tone":"muted"}""", Bytes(silent));
        Assert.Equal("{}", Bytes(new RuntimeTextPart(null!)));

        // The piece that leads may be the one that says nothing: the words of the line are the other piece's, and the
        // place the silent piece leads is written beside no words at all.
        var led = RuntimeTileLine.Of("plain", new List<RuntimeTextPart> { new(null!, ToTask), new("b") });

        Assert.Equal("b", led.Text);
        Assert.Equal(2, led.Parts!.Count);
        Assert.Null(led.Parts[0].Text);
        Assert.Same(ToTask, led.Parts[0].Link);
        Assert.Equal(
            """{"text":"b","tone":"plain","parts":[{"link":{"href":"https://octopus.example.net/app#/Spaces-1/tasks/ServerTasks-1","title":"The deployment\u0027s task in Octopus Deploy (opens in a new tab; Octopus Deploy asks you to sign in)"}},{"text":"b"}]}""",
            Bytes(led));

        // A thousand pieces of one letter are a thousand letters, and no piece of them leads.
        var thick = RuntimeTileLine.Of("plain", [.. Enumerable.Repeat(new RuntimeTextPart("x"), 1_000)]);

        Assert.Equal(new string('x', 1_000), thick.Text);
        Assert.Null(thick.Parts);
        Assert.Equal("plain", thick.Tone);
    }

    /// <summary>The address a link is made of is the one the <see cref="Uri" /> says and not the one the file spelled,
    /// for the forms the builder never builds either: a host with no path gets the <c>/</c> the script reads as a
    /// root, an address that is already escaped is escaped once and never twice, an empty fragment stands, the zone of
    /// an IPv6 address falls out of it, and a scheme that is neither <c>http</c> nor <c>https</c> is handed over whole
    /// - because deciding what a link may lead to is nobody's business here but the manifest's.</summary>
    [Fact]
    public void ALinkIsTheAddressTheUriSaysEvenOfTheFormsABuilderNeverMakes()
    {
        Assert.Equal("https://a.example.net/", RuntimeLink.To(new Uri("https://a.example.net"), "a")!.Href);
        Assert.Equal("https://a.example.net/A%20B", RuntimeLink.To(new Uri("https://a.example.net/A%20B"), "a")!.Href);
        Assert.Equal("https://a.example.net/x?q=a%2Bb", RuntimeLink.To(new Uri("https://a.example.net/x?q=a%2Bb"), "a")!.Href);
        Assert.Equal("https://a.example.net/#", RuntimeLink.To(new Uri("https://a.example.net/#"), "a")!.Href);
        Assert.Equal("https://[fe80::1]:8080/p", RuntimeLink.To(new Uri("https://[fe80::1%25eth0]:8080/p"), "a")!.Href);
        Assert.Equal("about:blank", RuntimeLink.To(new Uri("about:blank"), "a")!.Href);
        Assert.Equal("file:///tmp/a%20b", RuntimeLink.To(new Uri("file:///tmp/a%20b"), "a")!.Href);
        Assert.Equal("mailto:a@b.example", RuntimeLink.To(new Uri("mailto:a@b.example"), "a")!.Href);
        Assert.DoesNotContain("%2520", RuntimeLink.To(new Uri("https://a.example.net/A%20B"), "a")!.Href, StringComparison.Ordinal);
        Assert.DoesNotContain("eth0", RuntimeLink.To(new Uri("https://[fe80::1%25eth0]:8080/p"), "a")!.Href, StringComparison.Ordinal);

        // The words go with the address as they stand, and two calls over one address are one link and two objects.
        Assert.Equal("  the task  ", RuntimeLink.To(new Uri(TaskUrl), "  the task  ")!.Title);
        Assert.Equal(TaskWords, RuntimeLink.To(new Uri(TaskUrl), TaskWords)!.Title);
        var first = RuntimeLink.To(new Uri(TaskUrl), TaskWords);
        var second = RuntimeLink.To(new Uri(TaskUrl), TaskWords);

        Assert.Equal(first, second);
        Assert.Equal(first!.GetHashCode(), second!.GetHashCode());
        Assert.NotSame(first, second);
        Assert.Equal(first, first with { });
        Assert.NotSame(first, first with { });

        // And the link of an address the file never gave is still no link, whoever the words were for.
        Assert.Null(RuntimeLink.To(null, "a"));
    }

    // ------------------------------------------------------------- the two operators of every record, and nothing

    /// <summary>The records <c>RuntimePayload.cs</c> declares, bar <see cref="RuntimeTrend" /> and
    /// <see cref="RuntimeName" />, whose <c>==</c> and <c>!=</c> against nothing <see cref="RuntimeTrendEdgeCaseTests" />
    /// and <see cref="RuntimeNameEdgeCaseTests" /> already pin.</summary>
    private static readonly Type[] Records =
    [
        typeof(RuntimePayload), typeof(RuntimeTile), typeof(RuntimeTileLine), typeof(RuntimeTextPart),
        typeof(RuntimeCheckMark), typeof(RuntimeRegionMark), typeof(RuntimeEdgeMark), typeof(RuntimeDeployment),
        typeof(RuntimeLink),
    ];

    /// <summary>One row per record of the payload, so that the members the compiler writes for it are asked of all of
    /// them at once.</summary>
    public static IEnumerable<object[]> TheRecordsOfOneUpdate() => Records.Select(record => new object[] { record });

    /// <summary>Nothing is weighed against nothing the same way by every record of an update, and none of them is one
    /// with a record that says nothing: the two operators the compiler writes answer <c>null</c> on either side before
    /// a part of the record is looked at, so that a payload a caller never filled in is no payload rather than a
    /// <see cref="NullReferenceException" />, and the record of nothing at all - which no builder makes and no record
    /// refuses - is one record, equal to itself made by another call, and one slot in a set.</summary>
    [Theory]
    [MemberData(nameof(TheRecordsOfOneUpdate))]
    public void TheNothingOfEveryRecordIsNoRecordAndEachIsOneWithItsOwnNothing(Type record)
    {
        var one = NothingOf(record);
        var two = NothingOf(record);

        Assert.True(Weighed(record, "op_Equality", null, null));
        Assert.False(Weighed(record, "op_Inequality", null, null));
        Assert.False(Weighed(record, "op_Equality", one, null));
        Assert.True(Weighed(record, "op_Inequality", one, null));
        Assert.False(Weighed(record, "op_Equality", null, two));
        Assert.True(Weighed(record, "op_Inequality", null, two));
        Assert.True(Weighed(record, "op_Equality", one, two));
        Assert.False(Weighed(record, "op_Inequality", one, two));
        Assert.True(Weighed(record, "op_Equality", one, one));

        // The same answers through the members of the record itself, and nothing else is ever one with it.
        Assert.True(one.Equals(one));
        Assert.True(one.Equals(two));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Single(new HashSet<object> { one, two });
        Assert.False(one.Equals(null));
        Assert.False(one.Equals("alias"));

        // And the nothing of one record is no nothing of another, not even the nothing of a name's two parts.
        Assert.False(one.Equals(NothingOf(typeof(RuntimeName))));
    }

    // --------------------------------------------------------------------------- the bytes, read back

    /// <summary>What the context beside the record writes, the same context reads back and writes again as one: the
    /// bytes of an update carry every part of it, a part that was left out comes back as the absence the constructor
    /// gives it - the tone of a line nobody named comes back <c>plain</c> - and the update read back is no update
    /// with the one that was written, since a record weighs the lists it holds and not what stands in them.</summary>
    [Fact]
    public void WhatTheContextWritesTheSameContextReadsBackAndWritesAgainAsOne()
    {
        var bytes = AwkwardUpdate.ToJson();
        var read = JsonSerializer.Deserialize(bytes, RuntimePayloadJson.Default.RuntimePayload)!;

        Assert.Equal(bytes, read.ToJson());
        Assert.NotSame(AwkwardUpdate, read);
        Assert.NotEqual(AwkwardUpdate, read);
        Assert.Null(read.Names);
        Assert.Single(read.Nodes);
        var node = read.Nodes[0];
        var line = node.Lines[0];

        Assert.Equal("</script>", node.Label);
        Assert.Null(node.NameLink);
        Assert.Equal(AwkwardUrl, node.Link!.Href);
        Assert.Equal(AwkwardWords, node.Link!.Title);
        Assert.Equal("insync", line.Tone);
        Assert.Equal(2, line.Parts!.Count);
        Assert.Equal("API: Healthy <200 OK> \"once\" & twice", line.Marks![0].Title);
        Assert.Equal("deploying \"demo & co\" to uat", node.Deployment!.Title);
        Assert.Equal(0.5, read.Edges[0].Trend!.Points[0]);
        Assert.Equal("serving traffic: <1 region>", read.Regions[0].Label);
        Assert.Equal(Awkward, line.Parts![1].Link);

        // The parts the writer left out are the absences the constructor names, and an empty list stays an empty list.
        var bare = JsonSerializer.Deserialize("""{"alias":"a","state":"s","label":"L","lines":[{"text":"x"}],"title":"t"}""", RuntimePayloadJson.Default.RuntimeTile)!;

        Assert.Null(bare.Facts);
        Assert.Null(bare.History);
        Assert.Null(bare.Link);
        Assert.Null(bare.NameLink);
        Assert.Null(bare.Deployment);
        Assert.Single(bare.Lines);
        Assert.Equal("plain", bare.Lines[0].Tone);
        Assert.Null(bare.Lines[0].Parts);
        Assert.Null(bare.Lines[0].Trend);
        Assert.Null(bare.Lines[0].Marks);
        Assert.Equal("""{"nodes":[{"alias":"a","state":"s","label":"L","lines":[{"text":"x","tone":"plain"}],"title":"t"}],"regions":[],"edges":[]}""", new RuntimePayload(new List<RuntimeTile> { bare }, new List<RuntimeRegionMark>(), new List<RuntimeEdgeMark>()).ToJson());

        // An update of no part at all reads back as the four absences it was written as, and not as four empty lists.
        var nothing = JsonSerializer.Deserialize("{}", RuntimePayloadJson.Default.RuntimePayload)!;

        Assert.Null(nothing.Nodes);
        Assert.Null(nothing.Regions);
        Assert.Null(nothing.Edges);
        Assert.Null(nothing.Names);
        Assert.Equal("{}", nothing.ToJson());
        Assert.Equal("{}", JsonSerializer.Serialize(nothing, RuntimePayloadJson.Default.RuntimePayload));

        var empties = JsonSerializer.Deserialize("""{"nodes":[],"regions":[],"edges":[]}""", RuntimePayloadJson.Default.RuntimePayload)!;

        Assert.Empty(empties.Nodes);
        Assert.Empty(empties.Regions);
        Assert.Empty(empties.Edges);
        Assert.NotEqual(nothing, empties);
        Assert.Equal("""{"nodes":[],"regions":[],"edges":[]}""", empties.ToJson());
    }

    // ---------------------------------------------------------------------------------------- the machinery

    /// <summary>The bytes of one part of an update, on its own and not inside the tile it belongs to, as the context
    /// beside the record writes it.</summary>
    private static string Bytes(RuntimeTileLine line) => JsonSerializer.Serialize(line, RuntimePayloadJson.Default.RuntimeTileLine);

    /// <summary>The bytes of one part of an update, on its own, whatever the part.</summary>
    private static string Bytes(RuntimeTextPart part) => JsonSerializer.Serialize(part, RuntimePayloadJson.Default.RuntimeTextPart);

    /// <summary>The record of nothing at all: every part the constructor names, named as nothing. A record of the
    /// payload decides nothing of its own, so it lets all of them through, and a test can ask the same question of
    /// nine records at once by handing the constructor the nulls it never asks about.</summary>
    private static object NothingOf(Type record)
    {
        var constructor = Assert.Single(record.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

        return constructor.Invoke([.. constructor.GetParameters().Select(_ => (object?)null)]);
    }

    /// <summary>The <c>==</c> and the <c>!=</c> a caller writes, asked of the record by name - which is the only way
    /// to put nothing on one side of them for nine records at once.</summary>
    private static bool Weighed(Type record, string asked, object? left, object? right)
    {
        var operatorOf = Assert.Single(record.GetMember(asked, BindingFlags.Public | BindingFlags.Static));

        return (bool)((MethodInfo)operatorOf).Invoke(null, new object?[] { left, right })!;
    }
}
