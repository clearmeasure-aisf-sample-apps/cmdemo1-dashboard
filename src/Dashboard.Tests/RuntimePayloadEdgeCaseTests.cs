using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of the records <c>src/Dashboard/Runtime/RuntimePayload.cs</c> hands
/// to <c>js/runtime.js</c>: the payload of one update, a node's tile, a line of it, a piece of a line, a mark of a
/// detailed health check, a region's mark, a relationship's mark, a deployment's mark and a link. None of them decides
/// anything of its own - no state, no word, no address, no moment - so what is pinned here is that every part comes
/// back as the part it went in as, that a record refuses nothing it is handed, that a list is <em>held</em> rather
/// than copied or weighed entry for entry, that the members every record is asked compare, hash, fall apart, copy and
/// say the parts, and that what the script is given is the parts in the order they are declared.
/// <para>
/// What the other files already pin is left alone. <see cref="RuntimeTrendAndNameTests" /> and
/// <see cref="RuntimeTrendEdgeCaseTests" /> pin <see cref="RuntimeTrend" /> and <see cref="RuntimeName" /> whole: the
/// copy out of a health trend, the empty and the missing line, the heights kept as they stand, the two property names,
/// the JSON of both, and that an empty list of names is written while no list of them is left out.
/// <see cref="RuntimePayloadBuilderTests" /> pins that a whole payload is camelCase without nulls, the keys of a tile
/// and of an edge the sample builds, and the keys of a deployment that carries a link;
/// <see cref="RuntimeBoxLinkTests" /> pins the keys of a region's mark with and without its name link;
/// <see cref="DependencyTests" /> pins the marks of a detailed health check as the builder makes them;
/// <see cref="LinkTests" /> pins that a line of pieces is its words in a row, that the pieces are kept only when one
/// is a link, and that <see cref="RuntimeLink.To" /> of no address is no link. What the words and the states
/// <em>mean</em> - which node is <c>unreachable</c>, which line says <c>differs</c>, where a badge leads - is the
/// builder's and is pinned with the builder.
/// </para>
/// <para>
/// What follows is the records on their own, at the ends those tests leave open: the parts by the names of their own
/// properties, the parts a caller may leave out standing absent, the empty word that is a word and the spaces and the
/// letters that only look alike, the nothing a record will hold although the builder never gives it, the list that is
/// the very list it was handed, the address a <see cref="Uri" /> says about itself rather than the one it was built
/// from, and the whole public surface of each record, so that the day a part is added the list of them says so.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every value below is one the test brought, and no
/// payload is built out of a manifest.
/// </para>
/// </summary>
public class RuntimePayloadEdgeCaseTests
{
    private const string Href = "https://octopus.example.net/app#/Spaces-1/tasks/ServerTasks-1";
    private const string Portal = "https://portal.example.net/#subscriptions/00000000-0000-0000-0000-000000000000";
    private const string TaskTitle = "The deployment's task in Octopus Deploy (opens in a new tab; Octopus Deploy asks you to sign in)";
    private const string Sentence = "deploying cmdemo2-ui 2.4.43 to uat (3 min so far)";

    private static readonly RuntimeLink ToTask = new(Href, TaskTitle);
    private static readonly RuntimeLink ToPortal = new(Portal, "The subscription cmdemo1 in the Azure portal");

    /// <summary>The lines of a tile, held as one array so that the fixtures below are tiles of the very same lines.</summary>
    private static readonly IReadOnlyList<RuntimeTileLine> Lines = [new RuntimeTileLine("2.4.43 reaches uat", "deploying"), new RuntimeTileLine("pinned 2.4.43: in sync", "insync")];

    /// <summary>The last checks of a node, held as one array for the same reason.</summary>
    private static readonly IReadOnlyList<string> History = ["checking", "healthy", "healthy"];

    /// <summary>A mark of a deployment with every part named.</summary>
    private static readonly RuntimeDeployment Deployment = new("executing", Sentence, ToTask);

    /// <summary>A sparkline, as a line of a tile or a number line may carry one.</summary>
    private static readonly RuntimeTrend Trend = new([0, 0.5, 1], "Requests per minute, last 3 checks: 0 to 40, now 20");

    // ------------------------------------------------------------------------------------- the fixtures

    /// <summary>A tile with every part named, the words apart from one another so that a part landing on another
    /// property is seen instead of quietly matching.</summary>
    private static RuntimeTile FullTile => new(
        "app_ui_primary", "healthy", "Healthy", "HTTP 200 · 85 ms", Lines, History,
        "cmdemo2-ui primary: healthy.", ToTask, ToPortal, Deployment);

    /// <summary>A tile named by nothing but the seven parts every caller must name.</summary>
    private static RuntimeTile BareTile => new("sqldb", "neutral", "Neutral", null, Lines, null, "The database: not probed.");

    /// <summary>The tile of nothing at all: no words, no lists, no links - the parts a record will hold although a
    /// manifest never gives them.</summary>
    private static RuntimeTile NoTile => new(null!, null!, null!, null, null!, null, null!);

    /// <summary>A payload of one tile and nothing else, which is how these tests hand the writer a single part.</summary>
    private static RuntimePayload OfTile(RuntimeTile tile) => new([tile], [], []);

    /// <summary>A payload of one relationship's mark and nothing else.</summary>
    private static RuntimePayload OfEdge(RuntimeEdgeMark mark) => new([], [], [mark]);

    /// <summary>A payload of one region's mark and nothing else.</summary>
    private static RuntimePayload OfRegion(RuntimeRegionMark mark) => new([], [mark], []);

    /// <summary>The tile the writer gave back, as the script reads it.</summary>
    private static JsonElement Written(RuntimeTile tile)
    {
        using var json = JsonDocument.Parse(OfTile(tile).ToJson());
        return json.RootElement.GetProperty("nodes")[0].Clone();
    }

    /// <summary>A line, written out as the script reads it inside the tile it belongs to.</summary>
    private static JsonElement Written(RuntimeTileLine line) => Written(new RuntimeTile("a", "neutral", "Neutral", null, [line], null, "t")).GetProperty("lines")[0];

    /// <summary>A tile carrying this deployment, written out.</summary>
    private static JsonElement Written(RuntimeDeployment mark) => Written(new RuntimeTile("a", "neutral", "Neutral", null, [], null, "t", Deployment: mark));

    /// <summary>A tile whose badge leads this way, written out.</summary>
    private static JsonElement Written(RuntimeLink link) => Written(new RuntimeTile("a", "neutral", "Neutral", null, [], null, "t", Link: link));

    /// <summary>This mark of a relationship, written out.</summary>
    private static JsonElement WrittenEdge(RuntimeEdgeMark mark)
    {
        using var json = JsonDocument.Parse(OfEdge(mark).ToJson());
        return json.RootElement.GetProperty("edges")[0].Clone();
    }

    /// <summary>This mark of a region, written out.</summary>
    private static JsonElement WrittenRegion(RuntimeRegionMark mark)
    {
        using var json = JsonDocument.Parse(OfRegion(mark).ToJson());
        return json.RootElement.GetProperty("regions")[0].Clone();
    }

    /// <summary>A piece of a line, written out inside a line that is made of nothing else.</summary>
    private static string Written(RuntimeTextPart part) => Written(new RuntimeTileLine("x", "plain", [part])).GetProperty("parts")[0].GetRawText();

    /// <summary>A mark of a detailed health check, written out inside a line that carries nothing else.</summary>
    private static string Written(RuntimeCheckMark mark) => Written(new RuntimeTileLine("x", "plain", null, null, [mark])).GetProperty("marks")[0].GetRawText();

    // --------------------------------------------------------------------------------- the payload's parts

    /// <summary>The four lists of one update come back as the very lists the caller named - the payload copies nothing
    /// and weighs nothing - and the fourth, which a caller may leave out, stands absent when it is: absent, and not the
    /// empty list a page would read as boxes it has looked at and found nothing to name.</summary>
    [Fact]
    public void ThePayloadHandsEachListToThePropertyOfItsName()
    {
        var nodes = new List<RuntimeTile> { BareTile };
        var regions = new List<RuntimeRegionMark> { new("region_primary", "serving", "serving traffic") };
        var edges = new List<RuntimeEdgeMark> { new("browser-to-fd_ui", "neutral", null, null, null, "The browser to the Front Door.") };
        var names = new List<RuntimeName> { new("sub", ToPortal) };
        var payload = new RuntimePayload(nodes, regions, edges, names);

        Assert.Same(nodes, payload.Nodes);
        Assert.Same(regions, payload.Regions);
        Assert.Same(edges, payload.Edges);
        Assert.Same(names, payload.Names);
        Assert.Single(payload.Nodes);
        Assert.Equal("sqldb", payload.Nodes[0].Alias);

        // No copy is made, so a list that grows grows the payload the script is about to be handed.
        nodes.Add(FullTile);
        names.Add(new RuntimeName("rg_tier", ToPortal));

        Assert.Equal(2, payload.Nodes.Count);
        Assert.Equal("app_ui_primary", payload.Nodes[1].Alias);
        Assert.Equal(2, payload.Names!.Count);

        // The fourth part a caller does not name is the absence of it, not an empty list of it.
        Assert.Null(new RuntimePayload([], [], []).Names);
        Assert.Null(new RuntimePayload([], [], [], null).Names);
        Assert.Empty(new RuntimePayload([], [], [], []).Names!);
    }

    /// <summary>The payload refuses nothing it is given: no list of boxes, no list of regions, no list of lines and no
    /// list of names are parts it asks about, and the payload of nothing at all is one payload, equal to itself made by
    /// another call and standing alone in a set.</summary>
    [Fact]
    public void ThePayloadRefusesNothingItIsGiven()
    {
        var nothing = new RuntimePayload(null!, null!, null!, null);

        Assert.Null(nothing.Nodes);
        Assert.Null(nothing.Regions);
        Assert.Null(nothing.Edges);
        Assert.Null(nothing.Names);
        Assert.Equal(nothing, new RuntimePayload(null!, null!, null!, null));
        Assert.Equal(nothing.GetHashCode(), new RuntimePayload(null!, null!, null!, null).GetHashCode());
        Assert.Single(new HashSet<RuntimePayload> { nothing, new RuntimePayload(null!, null!, null!, null) });

        // A box list that holds no box, and a box list that is not there at all, are two payloads.
        Assert.NotEqual(nothing, new RuntimePayload([], [], [], null));
        Assert.True(nothing != new RuntimePayload([], [], [], null));

        // The writer tells the two apart the same way: what is no list is left out, what is an empty one is written.
        Assert.Equal("{}", nothing.ToJson());
        Assert.Equal("""{"nodes":[null],"regions":[],"edges":[]}""", new RuntimePayload([null!], [], []).ToJson());
    }

    /// <summary>Two payloads are one while their four parts are, and a part is weighed as the list it is held in: two
    /// calls over the same lists make one payload, two lists of the very same tiles make two payloads, and any one of
    /// the four parts tells two payloads apart - which is why a builder that hands the writer a list hands it once.</summary>
    [Fact]
    public void TwoPayloadsAreOneWhileTheirFourListsAreTheLists()
    {
        var nodes = new List<RuntimeTile> { BareTile };
        var regions = new List<RuntimeRegionMark>();
        var edges = new List<RuntimeEdgeMark>();
        var one = new RuntimePayload(nodes, regions, edges, null);
        var two = new RuntimePayload(nodes, regions, edges, null);

        Assert.True(one == two);
        Assert.True(one.Equals(two));
        Assert.True(one.Equals((object)two));
        Assert.False(one != two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.NotSame(one, two);
        Assert.Single(new HashSet<RuntimePayload> { one, two });

        // The same box in a list built apart is another payload, and so is an empty list built apart: nothing here is
        // compared entry for entry.
        Assert.NotEqual(one, new RuntimePayload([BareTile], regions, edges, null));
        Assert.NotEqual(one, new RuntimePayload(nodes, new List<RuntimeRegionMark>(), edges, null));
        Assert.False(one == new RuntimePayload([BareTile], regions, edges, null));

        // The part is the list, not what is in it: the same list grown by a box is the same part, and hashes as one.
        var grown = new List<RuntimeTile> { BareTile };
        var before = new RuntimePayload(grown, regions, edges, null).GetHashCode();

        grown.Add(FullTile);
        Assert.Equal(before, new RuntimePayload(grown, regions, edges, null).GetHashCode());

        // Every one of the four parts tells two payloads apart.
        Assert.Equal(
            4,
            new HashSet<RuntimePayload>
            {
                new(nodes, regions, edges, null),
                new(nodes, [new RuntimeRegionMark("r", "neutral", "not checked")], edges, null),
                new(nodes, regions, [new RuntimeEdgeMark("e", "neutral", null, null, null, "t")], null),
                new(nodes, regions, edges, [new RuntimeName("sub", ToPortal)]),
            }.Count);
    }

    /// <summary>Nothing on either side of the members every record is asked: two payloads that are no payload are one,
    /// a payload and no payload are two, and a payload says no to the nothing, to a word, to a tile and to the very
    /// list its own boxes are held in.</summary>
    [Fact]
    public void NoPayloadIsOneWithNoPayloadAndAPayloadIsNotItsParts()
    {
        RuntimePayload? none = null;
        RuntimePayload? alsoNone = null;
        var nodes = new List<RuntimeTile>();
        var there = new RuntimePayload(nodes, new List<RuntimeRegionMark>(), new List<RuntimeEdgeMark>());

        Assert.True(none == alsoNone);
        Assert.False(none != alsoNone);
        Assert.False(none == there);
        Assert.True(there != none);
        Assert.False(there.Equals((object?)null));
        Assert.False(there.Equals(null));
        Assert.False(there.Equals("nodes"));
        Assert.False(there.Equals(Array.Empty<RuntimeTile>()));
        Assert.False(there.Equals(BareTile));
        Assert.True(there.Equals((object)new RuntimePayload(nodes, there.Regions, there.Edges)));
        Assert.True(there == new RuntimePayload(nodes, there.Regions, there.Edges));
    }

    /// <summary>The four lists fall out of a payload in the order they went in, and what came out goes back in the
    /// same way; the fourth falls out as the nothing it is when nobody named it.</summary>
    [Fact]
    public void TheFourPartsFallOutOfAPayloadInTheOrderTheyWentIn()
    {
        var names = new List<RuntimeName> { new("sub", ToPortal) };
        var payload = new RuntimePayload([BareTile], [new RuntimeRegionMark("region_primary", "serving", "serving traffic")], [new RuntimeEdgeMark("e", "active", "12", "req/min", null, "t")], names);

        var (nodes, regions, edges, named) = payload;

        Assert.Same(payload.Nodes, nodes);
        Assert.Same(payload.Regions, regions);
        Assert.Same(payload.Edges, edges);
        Assert.Same(names, named);
        Assert.Equal(payload, new RuntimePayload(nodes, regions, edges, named));

        var (_, _, _, none) = new RuntimePayload([], [], []);

        Assert.Null(none);
    }

    /// <summary><c>with</c> names the part it changes and carries the other three: a copy that names nothing is one
    /// payload in another reference, the original stands where it stood, and a part may be put on or taken off without
    /// the lists behind the others being touched.</summary>
    [Fact]
    public void WithChangesOnlyThePartOfAPayloadItNames()
    {
        var nodes = new List<RuntimeTile> { BareTile };
        var payload = new RuntimePayload(nodes, [], []);
        var copy = payload with { };

        Assert.Equal(payload, copy);
        Assert.NotSame(payload, copy);
        Assert.Same(nodes, copy.Nodes);

        var named = payload with { Names = [new RuntimeName("sub", ToPortal)] };

        Assert.Single(named.Names!);
        Assert.Null(payload.Names);
        Assert.Same(nodes, named.Nodes);
        Assert.True(named.Nodes == payload.Nodes);

        var unnamed = named with { Names = null };

        Assert.Null(unnamed.Names);
        Assert.Equal(payload, unnamed);
        Assert.Equal(payload, payload with { Nodes = nodes });
    }

    /// <summary>The words a payload says about itself are its name and its four parts in the order they are declared,
    /// each named and standing blank when there is nothing of it - the record's own words, not the page's, and the
    /// same ones whatever the reader of the machine speaks.</summary>
    [Fact]
    public void APayloadSaysItsFourPartsInTheirOrder()
    {
        const string empty = "RuntimePayload { Nodes = Dashboard.Runtime.RuntimeTile[], Regions = Dashboard.Runtime.RuntimeRegionMark[], Edges = Dashboard.Runtime.RuntimeEdgeMark[], Names =  }";

        Assert.Equal(empty, new RuntimePayload([], [], [], null).ToString());
        Assert.Equal(
            "RuntimePayload { Nodes = , Regions = , Edges = , Names =  }",
            new RuntimePayload(null!, null!, null!, null).ToString());
        Assert.Contains("Nodes = System.Collections.Generic.List`1[Dashboard.Runtime.RuntimeTile]", new RuntimePayload(new List<RuntimeTile> { BareTile }, [], []).ToString());

        ViewerCulture.German(() => Assert.Equal(empty, new RuntimePayload([], [], [], null).ToString()));
    }

    // ----------------------------------------------------------------------- the payload as the script reads it

    /// <summary>The words of one update are the ones <see cref="RuntimePayloadJson" /> declares beside the record: the
    /// same bytes <see cref="RuntimePayload.ToJson" /> says, written camelCase with the parts that are nothing left
    /// out, and the four lists in the order the record declares them.</summary>
    [Fact]
    public void ThePayloadIsWrittenByTheContextDeclaredBesideIt()
    {
        var payload = new RuntimePayload([BareTile with { Lines = [] }], [], [], [new RuntimeName("sub", ToPortal)]);

        Assert.Equal(JsonSerializer.Serialize(payload, RuntimePayloadJson.Default.RuntimePayload), payload.ToJson());
        Assert.Equal(typeof(RuntimePayload), RuntimePayloadJson.Default.RuntimePayload.Type);
        Assert.Same(RuntimePayloadJson.Default.Options, RuntimePayloadJson.Default.RuntimePayload.Options);
        Assert.Same(JsonNamingPolicy.CamelCase, RuntimePayloadJson.Default.Options.PropertyNamingPolicy);
        Assert.Equal(JsonIgnoreCondition.WhenWritingNull, RuntimePayloadJson.Default.Options.DefaultIgnoreCondition);

        // What is written is the parts in the order they are declared, and nothing else at the top of it.
        Assert.Equal(
            """{"nodes":[{"alias":"sqldb","state":"neutral","label":"Neutral","lines":[],"title":"The database: not probed."}],"regions":[],"edges":[],"names":[{"alias":"sub","nameLink":{"href":"https://portal.example.net/#subscriptions/00000000-0000-0000-0000-000000000000","title":"The subscription cmdemo1 in the Azure portal"}}]}""",
            payload.ToJson());
    }

    /// <summary>Every part of a tile the script draws is written under the name the record gives it, in the order the
    /// constructor names them - the ten read straight out of the box the page fills in.</summary>
    [Fact]
    public void EveryPartOfATileIsWrittenUnderItsOwnNameAndInItsOwnPlace()
    {
        var tile = FullTile with { Facts = "HTTP 200 and 85 ms" };

        using var written = JsonDocument.Parse(Written(tile).GetRawText());
        var drawn = written.RootElement;

        Assert.Equal(
            ["alias", "state", "label", "facts", "lines", "history", "title", "link", "nameLink", "deployment"],
            [.. drawn.EnumerateObject().Select(property => property.Name)]);
        Assert.Equal("app_ui_primary", drawn.GetProperty("alias").GetString());
        Assert.Equal("healthy", drawn.GetProperty("state").GetString());
        Assert.Equal("Healthy", drawn.GetProperty("label").GetString());
        Assert.Equal("HTTP 200 and 85 ms", drawn.GetProperty("facts").GetString());
        Assert.Equal(["2.4.43 reaches uat", "pinned 2.4.43: in sync"], drawn.GetProperty("lines").EnumerateArray().Select(line => line.GetProperty("text").GetString()));
        Assert.Equal(["checking", "healthy", "healthy"], drawn.GetProperty("history").EnumerateArray().Select(state => state.GetString()));
        Assert.Equal("cmdemo2-ui primary: healthy.", drawn.GetProperty("title").GetString());
        Assert.Equal(Href, drawn.GetProperty("link").GetProperty("href").GetString());
        Assert.Equal(Portal, drawn.GetProperty("nameLink").GetProperty("href").GetString());
        Assert.Equal("executing", drawn.GetProperty("deployment").GetProperty("state").GetString());
    }

    /// <summary>The parts a tile does not say are left out of what the script is handed, and the parts it says as
    /// nothing or as an empty list are written: an empty word is a word the page shows, an empty list a row of marks
    /// the page has looked at and found nothing in, and neither is the absence the writer passes by.</summary>
    [Fact]
    public void ThePartsATileDoesNotSayAreLeftOutAndTheOnesItSaysAsNothingAreWritten()
    {
        Assert.Equal(["alias", "state", "label", "lines", "title"], Written(BareTile).EnumerateObject().Select(property => property.Name));

        var said = BareTile with { Facts = string.Empty, History = [] };

        Assert.Equal(["alias", "state", "label", "facts", "lines", "history", "title"], Written(said).EnumerateObject().Select(property => property.Name));
        Assert.Equal(string.Empty, said.Facts);
        Assert.Equal("[]", Written(said).GetProperty("history").GetRawText());

        // The three parts a caller may leave out are left out; named as nothing, they are the same tile.
        Assert.Equal(Written(BareTile).GetRawText(), Written(BareTile with { Link = null, NameLink = null, Deployment = null }).GetRawText());
        Assert.Equal(Written(BareTile).GetRawText(), Written(new RuntimeTile("sqldb", "neutral", "Neutral", null, Lines, null, "The database: not probed.", null, null, null)).GetRawText());
    }

    // --------------------------------------------------------------------------------------------- a node's tile

    /// <summary>The constructor's ten parts, position by position, come back as the property of the same name, and
    /// none of them is worked out of the others: a tile whose badge and whose name lead to one address keeps two
    /// links, and a state of <c>neutral</c> with a label of <c>Healthy</c> stands as it is given.</summary>
    [Fact]
    public void TheConstructorHandsEachPartOfATileToThePropertyOfItsName()
    {
        var tile = FullTile;

        Assert.Equal("app_ui_primary", tile.Alias);
        Assert.Equal("healthy", tile.State);
        Assert.Equal("Healthy", tile.Label);
        Assert.Equal("HTTP 200 · 85 ms", tile.Facts);
        Assert.Same(Lines, tile.Lines);
        Assert.Same(History, tile.History);
        Assert.Equal("cmdemo2-ui primary: healthy.", tile.Title);
        Assert.Same(ToTask, tile.Link);
        Assert.Same(ToPortal, tile.NameLink);
        Assert.Same(Deployment, tile.Deployment);

        // One link on the badge and none on the name is a tile with one link, not a tile with that link twice.
        var badge = BareTile with { Link = ToTask };

        Assert.Equal(ToTask, badge.Link);
        Assert.Null(badge.NameLink);
        Assert.Null(badge.Deployment);
    }

    /// <summary>The last three parts are the ones a caller may leave out, and they stand absent when it does: no place
    /// for the badge to lead, no place for the name to lead and no deployment to mark - the absence of each, and not
    /// the empty word or a mark of no state.</summary>
    [Fact]
    public void TheThreePartsOfATileNobodyNamesStandAbsent()
    {
        var bare = new RuntimeTile("sqldb", "neutral", "Neutral", null, [], null, "t");

        Assert.Null(bare.Link);
        Assert.Null(bare.NameLink);
        Assert.Null(bare.Deployment);
        Assert.Null(bare.Facts);
        Assert.Null(bare.History);
        Assert.Empty(bare.Lines);

        // Naming a part as nothing is the same tile as not naming it at all.
        Assert.Equal(bare, bare with { Link = null });
        Assert.Equal(bare, bare with { NameLink = null });
        Assert.Equal(bare, bare with { Deployment = null });
    }

    /// <summary>The word of a tile is kept exactly as it is handed: the empty word is a word, a word of spaces is a
    /// word with spaces about it, a word of one letter is a word, and a word of letters that only look like the Latin
    /// ones is kept letter for letter. Nothing is trimmed and nothing is refused - the builder is what decides what a
    /// page is shown, and the record only carries it.</summary>
    [Fact]
    public void ATileKeepsItsWordsExactlyAsTheyAreGiven()
    {
        var empty = new RuntimeTile(string.Empty, string.Empty, string.Empty, string.Empty, [], [], string.Empty);

        Assert.Equal(string.Empty, empty.Alias);
        Assert.Equal(string.Empty, empty.State);
        Assert.Equal(string.Empty, empty.Label);
        Assert.Equal(string.Empty, empty.Facts);
        Assert.Equal(string.Empty, empty.Title);
        Assert.Empty(empty.History!);

        var oneLetter = new RuntimeTile("a", "b", "c", "d", [new RuntimeTileLine("e")], ["f"], "g");

        Assert.Equal("a", oneLetter.Alias);
        Assert.Equal("b", oneLetter.State);
        Assert.Equal("c", oneLetter.Label);
        Assert.Equal("d", oneLetter.Facts);
        Assert.Equal("g", oneLetter.Title);
        Assert.Equal(["f"], oneLetter.History!);

        var spaced = BareTile with { Alias = "  sqldb  ", State = " healthy ", Label = "\tNeutral\n" };

        Assert.Equal("  sqldb  ", spaced.Alias);
        Assert.Equal(" healthy ", spaced.State);
        Assert.Equal("\tNeutral\n", spaced.Label);

        var written = BareTile with { Label = "ＢＲＯＷＳＥＲ", Title = "brōwser: healthy." };

        Assert.Equal("ＢＲＯＷＳＥＲ", written.Label);
        Assert.Equal("brōwser: healthy.", written.Title);

        // A word of a thousand letters is a word, and a link of an address of nothing but a scheme stands.
        Assert.Equal(new string('x', 1_000), (BareTile with { Title = new string('x', 1_000) }).Title);
        Assert.Equal("h:", new RuntimeLink("h:", "t").Href);
    }

    /// <summary>The record refuses nothing it is given: a tile will hold no words, no lists and no links at all, since
    /// it is the builder that decides what a node says and the record that carries it. The tile of nothing is one tile,
    /// two calls of nothing are one tile, and the nothing of a word is no word - which is what a page must not be
    /// handed, and what no manifest of this deployment ever gives.</summary>
    [Fact]
    public void ATileRefusesNothingItIsGiven()
    {
        var nothing = NoTile;

        Assert.Null(nothing.Alias);
        Assert.Null(nothing.State);
        Assert.Null(nothing.Label);
        Assert.Null(nothing.Facts);
        Assert.Null(nothing.Lines);
        Assert.Null(nothing.History);
        Assert.Null(nothing.Title);
        Assert.Null(nothing.Link);
        Assert.Null(nothing.NameLink);
        Assert.Null(nothing.Deployment);
        Assert.Equal(nothing, NoTile);
        Assert.Single(new HashSet<RuntimeTile> { nothing, NoTile });

        // The nothing of a word and the empty word are two parts, and a word of spaces a third.
        Assert.NotEqual(nothing, new RuntimeTile(string.Empty, string.Empty, string.Empty, string.Empty, [], [], string.Empty));
        Assert.NotEqual(nothing, new RuntimeTile(" ", " ", " ", " ", [new RuntimeTileLine(" ")], [" "], " "));
        Assert.False(nothing.Equals(new RuntimeTileLine("t")));
        Assert.False(nothing.Equals("app_ui_primary"));
    }

    /// <summary>The lines and the history are held as the lists they are handed, never copied and never weighed entry
    /// for entry: two tiles of the very same lines are one tile, two tiles of the same two lines in two arrays are
    /// two, and a list and an array of one word are two parts - which is why a builder that hands a tile its lines
    /// hands them once.</summary>
    [Fact]
    public void ATileWeighsItsListsAsTheListsTheyAreHeldIn()
    {
        var lines = new List<RuntimeTileLine> { new("2.4.43 reaches uat", "deploying") };
        var history = new List<string> { "checking" };
        var one = new RuntimeTile("a", "healthy", "Healthy", null, lines, history, "t");
        var two = new RuntimeTile("a", "healthy", "Healthy", null, lines, history, "t");

        Assert.Same(lines, one.Lines);
        Assert.Same(history, one.History);
        Assert.True(one == two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Single(new HashSet<RuntimeTile> { one, two });

        // The same words in a list built apart are another tile: nothing here is weighed entry for entry.
        var apart = new RuntimeTile("a", "healthy", "Healthy", null, [new RuntimeTileLine("2.4.43 reaches uat", "deploying")], ["checking"], "t");

        Assert.Equal(one.Lines, apart.Lines);
        Assert.Equal(one.History, apart.History);
        Assert.False(one.Equals(apart));
        Assert.Equal(2, new HashSet<RuntimeTile> { one, apart }.Count);

        // And the very list, grown by a line, is still the part it was: both tiles grew with it.
        lines.Add(new RuntimeTileLine("pinned 2.4.43: in sync", "insync"));

        Assert.Equal(2, one.Lines.Count);
        Assert.Equal(2, two.Lines.Count);
        Assert.Single(apart.Lines);
        Assert.Null(NoTile.History);
    }

    /// <summary>Two tiles are one while their ten parts are, whichever way of asking is used - and the copy made by
    /// taking the parts out and putting them back is one tile, since the parts that are lists come back as the very
    /// lists they went in as.</summary>
    [Fact]
    public void TwoTilesAreOneWhileTheirTenPartsAre()
    {
        var one = FullTile;
        var two = FullTile;

        Assert.True(one.Equals(two));
        Assert.True(one.Equals((object)two));
        Assert.False(one != two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.NotSame(one, two);
        Assert.Single(new HashSet<RuntimeTile> { one, two });

        one.Deconstruct(out var alias, out var state, out var label, out var facts, out var lines, out var history, out var title, out var link, out var nameLink, out var deployment);

        Assert.Equal(one, new RuntimeTile(alias, state, label, facts, lines, history, title, link, nameLink, deployment));
        Assert.Equal(one.GetHashCode(), new RuntimeTile(alias, state, label, facts, lines, history, title, link, nameLink, deployment).GetHashCode());
    }

    /// <summary>Any one of the ten parts tells two tiles apart, and nothing is compared loosely: the words ordinally,
    /// so a state in another case or with a space on it is another state, and a link of the same address and another
    /// title is another link.</summary>
    [Fact]
    public void ATileDiffersInAnyOneOfItsParts()
    {
        var tile = FullTile;

        Assert.NotEqual(tile, tile with { Alias = string.Empty });
        Assert.NotEqual(tile, tile with { Alias = null! });
        Assert.NotEqual(tile, tile with { Alias = "SQDB" });
        Assert.NotEqual(tile, tile with { State = "Healthy" });
        Assert.NotEqual(tile, tile with { Label = "neutral" });
        Assert.NotEqual(tile, tile with { Facts = null });
        Assert.NotEqual(tile, tile with { Facts = string.Empty });
        Assert.NotEqual(tile, tile with { History = null });
        Assert.NotEqual(tile, tile with { Deployment = null });
        Assert.NotEqual(tile, tile with { Lines = [] });
        Assert.NotEqual(tile, tile with { Title = "cmdemo2-ui primary: healthy " });
        Assert.NotEqual(tile, tile with { Link = ToPortal });
        Assert.NotEqual(tile, tile with { NameLink = ToTask });
        Assert.NotEqual(tile, tile with { Deployment = Deployment with { Link = null } });

        // A link's title is part of the link, so two addresses that are one and two titles that are not make two tiles.
        Assert.NotEqual(tile with { Link = ToTask }, tile with { Link = new RuntimeLink(Href, TaskTitle + ".") });
        Assert.Equal(tile with { Link = ToTask }, tile with { Link = new RuntimeLink(Href, TaskTitle) });
    }

    /// <summary><c>with</c> names the part it changes and carries the other nine: the builder puts a name link on a
    /// database's tile this way, a copy that names nothing is one tile in another reference, the lists are carried by
    /// reference and not renewed, and the tile the copy was made from stands where it stood.</summary>
    [Fact]
    public void WithChangesOnlyThePartOfATileItNames()
    {
        var tile = BareTile;
        var copy = tile with { };

        Assert.Equal(tile, copy);
        Assert.NotSame(tile, copy);
        Assert.Same(Lines, copy.Lines);

        var named = tile with { NameLink = ToPortal };

        Assert.Equal(ToPortal, named.NameLink);
        Assert.Null(tile.NameLink);
        Assert.Same(Lines, named.Lines);
        Assert.Null(named.Facts);
        Assert.Null(named.History);

        var probed = named with { State = "unreachable", Label = "No answer", Facts = "no answer", History = ["healthy", "unreachable"], Link = ToTask };

        Assert.Equal("unreachable", probed.State);
        Assert.Equal("No answer", probed.Label);
        Assert.Equal("no answer", probed.Facts);
        Assert.Equal(["healthy", "unreachable"], probed.History!);
        Assert.Equal(ToTask, probed.Link);
        Assert.Equal(ToPortal, probed.NameLink);
        Assert.Equal("neutral", tile.State);
        Assert.Null(tile.History);
        Assert.Equal(2, Lines.Count);
        Assert.Equal(3, History.Count);
    }

    /// <summary>The ten parts fall out of a tile in the order they went in, the last three as the nothing they are when
    /// nobody named them, and they go back in the same way.</summary>
    [Fact]
    public void TheTenPartsFallOutOfATileInTheOrderTheyWentIn()
    {
        var (alias, state, label, facts, lines, history, title, link, nameLink, deployment) = FullTile;

        Assert.Equal("app_ui_primary", alias);
        Assert.Equal("healthy", state);
        Assert.Equal("Healthy", label);
        Assert.Equal("HTTP 200 · 85 ms", facts);
        Assert.Same(Lines, lines);
        Assert.Same(History, history);
        Assert.Equal("cmdemo2-ui primary: healthy.", title);
        Assert.Same(ToTask, link);
        Assert.Same(ToPortal, nameLink);
        Assert.Same(Deployment, deployment);

        var (_, _, _, bareFacts, bareLines, bareHistory, _, bareLink, bareNameLink, bareDeployment) = BareTile;

        Assert.Null(bareFacts);
        Assert.Same(Lines, bareLines);
        Assert.Null(bareHistory);
        Assert.Null(bareLink);
        Assert.Null(bareNameLink);
        Assert.Null(bareDeployment);
    }

    /// <summary>The words a tile says about itself are its name and its ten parts in the order they are declared, the
    /// parts that are lists said by the type that holds them and the parts that are nothing said blank - the record's
    /// own words, which no reader's culture turns.</summary>
    [Fact]
    public void ATileSaysItsTenPartsInTheirOrder()
    {
        Assert.Equal(
            "RuntimeTile { Alias = a, State = neutral, Label = Neutral, Facts = , Lines = System.Collections.Generic.List`1[Dashboard.Runtime.RuntimeTileLine], History = System.String[], Title = t, Link = , NameLink = , Deployment =  }",
            new RuntimeTile("a", "neutral", "Neutral", string.Empty, new List<RuntimeTileLine> { new("t") }, [], "t").ToString());
        Assert.Equal(
            "RuntimeTile { Alias = , State = , Label = , Facts = , Lines = , History = , Title = , Link = , NameLink = , Deployment =  }",
            NoTile.ToString());

        var named = BareTile with { NameLink = ToPortal };

        Assert.Contains("Alias = sqldb", named.ToString(), StringComparison.Ordinal);
        Assert.Contains("NameLink = RuntimeLink { Href = " + Portal + ", Title = The subscription cmdemo1 in the Azure portal }", named.ToString(), StringComparison.Ordinal);
        ViewerCulture.German(() => Assert.Equal("RuntimeTile { Alias = , State = , Label = , Facts = , Lines = , History = , Title = , Link = , NameLink = , Deployment =  }", NoTile.ToString()));
    }

    // ------------------------------------------------------------------------------- a line under a tile's badge

    /// <summary>A line carries its words, the tone they are drawn in, the pieces they may be split into, the sparkline
    /// that may follow them and the marks that may stand before them - each part coming back as the part it went in
    /// as; only the words give way to the pieces when there are pieces, which here spell the same words.</summary>
    [Fact]
    public void TheConstructorHandsEachPartOfALineToThePropertyOfItsName()
    {
        var parts = PiecesOf(("2.4.43 ", null), ("reaches uat", ToTask));
        var marks = new List<RuntimeCheckMark> { new("healthy", "API: Healthy. Took under 0.1 ms.") };
        var trend = RuntimeTrend.Of(new Trend([10d, 40d], "Requests per minute"))!;
        var line = new RuntimeTileLine("2.4.43 reaches uat", "deploying", parts, trend, marks);

        Assert.Equal("2.4.43 reaches uat", line.Text);
        Assert.Equal("deploying", line.Tone);
        Assert.Same(parts, line.Parts);
        Assert.Same(trend, line.Trend);
        Assert.Same(marks, line.Marks);

        // A line of no tone at all is a line of no tone: the record falls back on nothing but its own default.
        Assert.Equal("plain", new RuntimeTileLine("waiting for the health checks").Tone);
        Assert.Null(new RuntimeTileLine("t", null!).Tone);
        Assert.Null(new RuntimeTileLine("t", "strong", null, null, null).Parts);
        Assert.Equal(string.Empty, new RuntimeTileLine(string.Empty, string.Empty).Text);
    }

    /// <summary>The line a caller makes of pieces is its words in a row, and <see cref="RuntimeTileLine.Of" /> hands
    /// over the very pieces it was given - it neither copies the list nor renews the trend beside it. The pieces are
    /// kept when one of them leads somewhere, whichever piece that is, and left out when none does.</summary>
    [Fact]
    public void ALineOfPiecesIsDrawnFromTheSamePiecesItWasMadeOf()
    {
        var trend = RuntimeTrend.Of(new Trend([10d, 40d], "Requests per minute"))!;
        var plain = PiecesOf(("12 ", null), ("req/min", null));
        var linked = PiecesOf(("12 ", null), ("req/min", ToTask));

        Assert.Same(linked, RuntimeTileLine.Of("plain", linked).Parts);
        Assert.Null(RuntimeTileLine.Of("plain", plain).Parts);

        // The one piece that leads may be the first, the last or the only one; the line keeps all of them either way.
        Assert.Equal(2, RuntimeTileLine.Of("plain", [new RuntimeTextPart("a", ToTask), new RuntimeTextPart("b")]).Parts!.Count);
        Assert.Equal(2, RuntimeTileLine.Of("plain", [new RuntimeTextPart("a"), new RuntimeTextPart("b", ToTask)]).Parts!.Count);
        Assert.Single(RuntimeTileLine.Of("plain", [new RuntimeTextPart("a", ToTask)]).Parts!);
        Assert.Same(trend, RuntimeTileLine.Of("plain", linked, trend).Trend);
        Assert.Null(RuntimeTileLine.Of("plain", linked).Trend);

        // The tone goes with the line and not with its pieces.
        Assert.Equal("waiting", RuntimeTileLine.Of("waiting", plain).Tone);
    }

    /// <summary>The smallest lines <see cref="RuntimeTileLine.Of" /> is given, and what it refuses: no piece
    /// at all is a line of no words, which the page draws as an empty row, and no line is what no caller ever meant -
    /// the only argument of the payload's that is asked for by name.</summary>
    [Fact]
    public void ALineOfNoPiecesIsALineOfNoWordsAndALineOfNoListAtAllIsRefused()
    {
        var empty = RuntimeTileLine.Of("plain", []);

        Assert.Equal(string.Empty, empty.Text);
        Assert.Null(empty.Parts);
        Assert.Null(empty.Trend);
        Assert.Null(empty.Marks);
        Assert.Equal("plain", empty.Tone);
        Assert.Equal(new RuntimeTileLine(string.Empty), empty);

        // A piece of no words is still a piece, and a line of pieces of no words is a line of no words.
        var silent = RuntimeTileLine.Of("muted", [new RuntimeTextPart(string.Empty), new RuntimeTextPart(string.Empty)]);

        Assert.Equal(string.Empty, silent.Text);
        Assert.Null(silent.Parts);
        Assert.Equal("muted", silent.Tone);

        // The one refusal the payload makes of its own, naming the part it missed.
        var error = Assert.Throws<ArgumentNullException>(() => RuntimeTileLine.Of("plain", null!));

        Assert.Equal("parts", error.ParamName);
        Assert.Throws<ArgumentNullException>(() => RuntimeTileLine.Of(null!, null!));
    }

    /// <summary>The five parts of a line are weighed as the values they are, with the two lists held as the lists they
    /// are handed: two lines of the same words, the same tone, the same marks on the same list and the same trend are
    /// one line, and a mark list built apart makes two - so a test that compares a whole tile compares its lines.</summary>
    [Fact]
    public void TwoLinesAreOneWhileTheirFivePartsAre()
    {
        var marks = new List<RuntimeCheckMark> { new("healthy", "API: Healthy."), new("failed", "DataAccess: Unhealthy.") };
        var trend = new RuntimeTrend([0, 1], "Requests per minute");
        var one = new RuntimeTileLine("checks", "ok", null, trend, marks);
        var two = new RuntimeTileLine("checks", "ok", null, trend, marks);

        Assert.True(one == two);
        Assert.True(one.Equals((object)two));
        Assert.False(one != two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.NotSame(one, two);
        Assert.Single(new HashSet<RuntimeTileLine> { one, two });

        var apart = new RuntimeTileLine("checks", "ok", null, trend, [new RuntimeCheckMark("healthy", "API: Healthy."), new RuntimeCheckMark("failed", "DataAccess: Unhealthy.")]);

        Assert.Equal(one.Marks, apart.Marks);
        Assert.NotEqual(one, apart);
        Assert.False(one.Equals(apart));
        Assert.False(one.Equals("checks"));
        Assert.False(one.Equals(new RuntimeCheckMark("healthy", "API: Healthy.")));
        Assert.False(one.Equals(marks));

        // Any one of the five parts tells two lines apart.
        Assert.NotEqual(one, one with { Text = string.Empty });
        Assert.NotEqual(one, one with { Text = null! });
        Assert.NotEqual(one, one with { Tone = "plain" });
        Assert.NotEqual(one, one with { Tone = "OK" });
        Assert.NotEqual(one, one with { Trend = null });
        Assert.NotEqual(one, one with { Trend = new RuntimeTrend([0, 1], "SQL commands per minute") });
        Assert.NotEqual(one, one with { Marks = null });
        Assert.NotEqual(one, one with { Parts = [new RuntimeTextPart("checks")] });

        var (text, tone, parts, lineTrend, lineMarks) = one;

        Assert.Equal("checks", text);
        Assert.Equal("ok", tone);
        Assert.Null(parts);
        Assert.Same(trend, lineTrend);
        Assert.Same(marks, lineMarks);
        Assert.Equal(one, new RuntimeTileLine(text, tone, parts, lineTrend, lineMarks));

        var retitled = one with { };

        Assert.Equal(one, retitled);
        Assert.NotSame(one, retitled);
        Assert.Same(marks, retitled.Marks);
        Assert.Same(marks, one.Marks);
    }

    /// <summary>A line of nothing at all is still a line, and the writer is the one that draws a difference between
    /// the parts that are words and the parts that are lists: the empty word is written, the nothing is left out, and
    /// a list of no marks is written as an empty row of marks the page has looked at and found nothing in.</summary>
    [Fact]
    public void ALineRefusesNothingItIsGivenAndIsWrittenAsItStands()
    {
        var nothing = new RuntimeTileLine(null!, null!);

        Assert.Null(nothing.Text);
        Assert.Null(nothing.Tone);
        Assert.Null(nothing.Parts);
        Assert.Null(nothing.Trend);
        Assert.Null(nothing.Marks);
        Assert.Equal(nothing, new RuntimeTileLine(null!, null!));

        // A part that is nothing at all is left out of what the script is handed: a line of no words and no tone is
        // written as an object with nothing in it, and the page has nothing to draw.
        Assert.Equal("{}", Written(nothing).GetRawText());

        // A line of empty tone and empty words says both, and a line of no marks says no marks.
        Assert.Equal(
            """{"text":"x","tone":"plain","marks":[]}""",
            Written(new RuntimeTileLine("x", "plain", null, null, [])).GetRawText());
        Assert.Equal(
            """{"text":"x","tone":"plain","parts":[{"text":"x"}]}""",
            Written(new RuntimeTileLine("x", "plain", [new RuntimeTextPart("x")])).GetRawText());
        Assert.Equal(
            ["text", "tone", "parts", "trend", "marks"],
            [.. Written(new RuntimeTileLine("x", "plain", [new RuntimeTextPart("x", ToTask)], Trend, [new RuntimeCheckMark("healthy", "API")])).EnumerateObject().Select(property => property.Name)]);
    }

    /// <summary>The words a line says about itself are its name and its five parts in the order they are declared,
    /// the pieces and the marks named by the list that holds them and the parts that are nothing said blank.</summary>
    [Fact]
    public void ALineSaysItsFivePartsInTheirOrder()
    {
        Assert.Equal("RuntimeTileLine { Text = , Tone = , Parts = , Trend = , Marks =  }", new RuntimeTileLine(null!, null!).ToString());
        Assert.Equal(
            "RuntimeTileLine { Text = checks, Tone = ok, Parts = , Trend = RuntimeTrend { Points = System.Collections.Generic.List`1[System.Double], Title = Requests per minute }, Marks = System.Collections.Generic.List`1[Dashboard.Runtime.RuntimeCheckMark] }",
            new RuntimeTileLine("checks", "ok", null, new RuntimeTrend(new List<double> { 0, 1 }, "Requests per minute"), new List<RuntimeCheckMark>()).ToString());

        ViewerCulture.German(() => Assert.Equal("RuntimeTileLine { Text = , Tone = , Parts = , Trend = , Marks =  }", new RuntimeTileLine(null!, null!).ToString()));
    }

    // ------------------------------------------------------------------- a piece of a line, and its marks

    /// <summary>A piece of a line is its words and the place they lead; the place is the part a caller may leave out,
    /// and the words are kept exactly as they are handed - the piece says nothing about whether it is a link or a
    /// sentence, and the line above it is what decides what is drawn.</summary>
    [Fact]
    public void APieceOfALineCarriesItsWordsAndThePlaceTheyLead()
    {
        var linked = new RuntimeTextPart("2.4.43 reaches uat", ToTask);
        var plain = new RuntimeTextPart("2.4.43 reaches uat");

        Assert.Equal("2.4.43 reaches uat", linked.Text);
        Assert.Same(ToTask, linked.Link);
        Assert.Null(plain.Link);
        Assert.NotEqual(linked, plain);
        Assert.Equal(plain, new RuntimeTextPart("2.4.43 reaches uat", null));
        Assert.Equal(2, new HashSet<RuntimeTextPart> { linked, plain, new RuntimeTextPart("2.4.43 reaches uat") }.Count);

        // The record refuses nothing: no words and no place to lead are parts it will hold.
        var nothing = new RuntimeTextPart(null!, null!);

        Assert.Null(nothing.Text);
        Assert.Null(nothing.Link);
        Assert.Equal(nothing, new RuntimeTextPart(null!, null!));
        Assert.Equal("{}", Written(nothing));
        Assert.Equal("""{"text":""}""", Written(new RuntimeTextPart(string.Empty)));
        Assert.Equal("RuntimeTextPart { Text = , Link =  }", nothing.ToString());
        // A piece that leads is written as its words and the link, in that order, and nothing else about it.
        using var led = JsonDocument.Parse(Written(plain with { Link = ToTask }));

        Assert.Equal(["text", "link"], led.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.Equal("2.4.43 reaches uat", led.RootElement.GetProperty("text").GetString());
        Assert.Equal(Href, led.RootElement.GetProperty("link").GetProperty("href").GetString());
        Assert.Equal(TaskTitle, led.RootElement.GetProperty("link").GetProperty("title").GetString());

        // An empty word is a word the page shows, and a word of spaces is a word with spaces about it.
        Assert.Equal(string.Empty, new RuntimeTextPart(string.Empty).Text);
        Assert.Equal("  ", new RuntimeTextPart("  ").Text);
        Assert.NotEqual(new RuntimeTextPart(string.Empty), new RuntimeTextPart("  "));
        Assert.NotEqual(new RuntimeTextPart(string.Empty), nothing);
    }

    /// <summary>A mark of a detailed health check is its shape and its tooltip, and nothing else: the four words a
    /// state may be - <c>healthy</c>, <c>degraded</c>, <c>failed</c>, <c>unknown</c> - are the builder's choice and
    /// are pinned with the builder; what is pinned here is that a mark carries the word it was given, that a title of
    /// several lines is one title, and that two marks are one while their two parts are.</summary>
    [Fact]
    public void AMarkOfACheckCarriesItsStateAndItsWordsAndNothingElse()
    {
        var mark = new RuntimeCheckMark("degraded", "LlmGateway: Degraded. Chat client answered slowly: 4.2 s (threshold 2 s). Took 44.1 ms.");

        Assert.Equal("degraded", mark.State);
        Assert.Contains("44.1 ms", mark.Title, StringComparison.Ordinal);
        Assert.Equal(new RuntimeCheckMark("degraded", mark.Title), mark);

        // A tooltip that spans lines is one title, and a mark that differs by one of its two parts is another mark.
        var twoLines = new RuntimeCheckMark("failed", "DataAccess: Unhealthy. Database connection failed.\nTook 15 s.");

        Assert.Contains('\n', twoLines.Title);
        Assert.Equal(twoLines, new RuntimeCheckMark("failed", "DataAccess: Unhealthy. Database connection failed.\nTook 15 s."));
        Assert.NotEqual(twoLines, new RuntimeCheckMark("failed", "DataAccess: Unhealthy. Database connection failed. Took 15 s."));
        Assert.NotEqual(mark, mark with { State = "healthy" });
        Assert.NotEqual(mark, mark with { Title = mark.Title + "." });
        Assert.Equal(
            "RuntimeCheckMark { State = degraded, Title = LlmGateway: Degraded. Chat client answered slowly: 4.2 s (threshold 2 s). Took 44.1 ms. }",
            mark.ToString());

        // A mark that says nothing is a mark the page still draws, and one whose tooltip spans lines keeps them.
        var nothing = new RuntimeCheckMark(null!, null!);

        Assert.Null(nothing.State);
        Assert.Null(nothing.Title);
        Assert.Equal("""{"state":"","title":"one\n two"}""", Written(new RuntimeCheckMark("", "one\n two")));
        Assert.Equal("{}", Written(nothing));
        Assert.Equal(
            ["state", "title"],
            [.. JsonDocument.Parse(Written(new RuntimeCheckMark("healthy", "API"))).RootElement.EnumerateObject().Select(property => property.Name)]);
    }

    // ----------------------------------------------------------------------------- a region's and an edge's mark

    /// <summary>A region's mark carries the alias the box is drawn by, the state of the region, the words of its mark
    /// and where its name leads - the fourth being the part a caller may leave out, since a region whose address the
    /// manifest does not give has nothing to lead to.</summary>
    [Fact]
    public void ARegionsMarkCarriesItsAliasItsStateAndTheWordsOfItsMark()
    {
        var mark = new RuntimeRegionMark("region_primary", "serving", "serving traffic", ToPortal);

        Assert.Equal("region_primary", mark.Alias);
        Assert.Equal("serving", mark.State);
        Assert.Equal("serving traffic", mark.Label);
        Assert.Same(ToPortal, mark.NameLink);

        var bare = new RuntimeRegionMark("region_data", "neutral", "database, static sites: not probed");

        Assert.Null(bare.NameLink);
        Assert.Equal(bare, bare with { NameLink = null });
        Assert.NotEqual(bare, mark with { Alias = "region_data" });

        // Any one of the four parts tells two marks apart, and the words are weighed ordinally.
        Assert.NotEqual(mark, mark with { Alias = string.Empty });
        Assert.NotEqual(mark, mark with { Alias = null! });
        Assert.NotEqual(mark, mark with { Alias = "region_standby" });
        Assert.NotEqual(mark, mark with { State = "Serving" });
        Assert.NotEqual(mark, mark with { State = "standby" });
        Assert.NotEqual(mark, mark with { Label = "serving traffic " });
        Assert.NotEqual(mark, mark with { NameLink = ToTask });
        Assert.Equal(mark, new RuntimeRegionMark("region_primary", "serving", "serving traffic", new RuntimeLink(Portal, ToPortal.Title)));
        Assert.Single(new HashSet<RuntimeRegionMark> { mark, new RuntimeRegionMark("region_primary", "serving", "serving traffic", ToPortal) });

        var (alias, state, label, nameLink) = mark;

        Assert.Equal("region_primary", alias);
        Assert.Equal("serving", state);
        Assert.Equal("serving traffic", label);
        Assert.Same(ToPortal, nameLink);
        Assert.Equal(mark, new RuntimeRegionMark(alias, state, label, nameLink));
    }

    /// <summary>A region's mark refuses nothing it is handed, and says its four parts in their order: the empty word
    /// written, the nothing left out, and the mark of nothing at all - which no builder makes and no record refuses.</summary>
    [Fact]
    public void ARegionsMarkRefusesNothingItIsGivenAndSaysItsFourPartsInTheirOrder()
    {
        var nothing = new RuntimeRegionMark(null!, null!, null!);

        Assert.Null(nothing.Alias);
        Assert.Null(nothing.State);
        Assert.Null(nothing.Label);
        Assert.Null(nothing.NameLink);
        Assert.Equal(nothing, new RuntimeRegionMark(null!, null!, null!));
        Assert.Equal("RuntimeRegionMark { Alias = , State = , Label = , NameLink =  }", nothing.ToString());
        Assert.Equal(
            """{"alias":"","state":"","label":""}""",
            WrittenRegion(new RuntimeRegionMark(string.Empty, string.Empty, string.Empty)).GetRawText());

        using var named = JsonDocument.Parse(WrittenRegion(new RuntimeRegionMark("sub", "down", "down: no web app answers", ToPortal)).GetRawText());

        Assert.Equal(["alias", "state", "label", "nameLink"], named.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.Equal(Portal, named.RootElement.GetProperty("nameLink").GetProperty("href").GetString());
        Assert.Equal("down: no web app answers", named.RootElement.GetProperty("label").GetString());

        ViewerCulture.German(() => Assert.Equal("RuntimeRegionMark { Alias = , State = , Label = , NameLink =  }", nothing.ToString()));
    }

    /// <summary>A relationship's mark carries its id, its state, the number and the unit it is drawn with, the role
    /// under the number, the tooltip, and the two parts a caller may leave out: where the number leads and the
    /// sparkline beside it. The number that is nothing and the number that is a dash are two parts - the first has no
    /// slot to draw in, the second is a slot that is waiting for a source of calls per minute.</summary>
    [Fact]
    public void ARelationshipsMarkCarriesItsNumberAndTheWordsAboutIt()
    {
        var mark = new RuntimeEdgeMark("app_ui_primary-to-sqldb", "active", "12", "sql/min", "queries", "The web app to the database.", ToTask, Trend);

        Assert.Equal("app_ui_primary-to-sqldb", mark.Id);
        Assert.Equal("active", mark.State);
        Assert.Equal("12", mark.Number);
        Assert.Equal("sql/min", mark.Unit);
        Assert.Equal("queries", mark.Text);
        Assert.Equal("The web app to the database.", mark.Title);
        Assert.Same(ToTask, mark.Link);
        Assert.Same(Trend, mark.Trend);

        // The six parts every caller names, with the two it may leave out standing absent.
        var bare = new RuntimeEdgeMark("browser-to-fd_ui", "neutral", null, null, null, "The browser to the Front Door.");

        Assert.Null(bare.Number);
        Assert.Null(bare.Unit);
        Assert.Null(bare.Text);
        Assert.Null(bare.Link);
        Assert.Null(bare.Trend);

        // The dash is a word, and the empty unit is a word: neither is the nothing the writer passes by.
        var dashed = bare with { Number = "-", Unit = string.Empty };

        Assert.Equal("-", dashed.Number);
        Assert.Equal(string.Empty, dashed.Unit);
        Assert.Equal(["id", "state", "number", "unit", "title"], WrittenEdge(dashed).EnumerateObject().Select(property => property.Name));
        Assert.Equal(["id", "state", "title"], WrittenEdge(bare).EnumerateObject().Select(property => property.Name));
        Assert.NotEqual(bare, dashed);
        Assert.NotEqual(dashed, dashed with { Number = null });
    }

    /// <summary>Every part of a relationship's mark is kept exactly, weighed ordinally and refused by nobody: the
    /// record neither requires an id the manifest knows nor a state the script can draw, and two marks are one while
    /// their eight parts are.</summary>
    [Fact]
    public void ARelationshipsMarkRefusesNothingItIsGivenAndSaysItsEightPartsInTheirOrder()
    {
        var nothing = new RuntimeEdgeMark(null!, null!, null!, null!, null!, null!);

        Assert.Null(nothing.Id);
        Assert.Null(nothing.State);
        Assert.Null(nothing.Number);
        Assert.Null(nothing.Unit);
        Assert.Null(nothing.Text);
        Assert.Null(nothing.Title);
        Assert.Null(nothing.Link);
        Assert.Null(nothing.Trend);
        Assert.Equal(nothing, new RuntimeEdgeMark(null!, null!, null!, null!, null!, null!));
        Assert.Equal("RuntimeEdgeMark { Id = , State = , Number = , Unit = , Text = , Title = , Link = , Trend =  }", nothing.ToString());
        Assert.False(nothing.Equals(new RuntimeEdge("browser-to-fd_ui", "browser", "fd_ui", RuntimeEdgeKind.Public)));
        Assert.False(nothing.Equals(RuntimeNodeKind.Sql));

        var mark = new RuntimeEdgeMark("app_ui_primary-to-sqldb", "active", "12", "sql/min", "queries", "The web app to the database.", ToTask, Trend);

        Assert.NotEqual(mark, mark with { Id = "app_ui_primary-to-sqldb " });
        Assert.NotEqual(mark, mark with { Id = "sqldb-to-app_ui_primary" });
        Assert.NotEqual(mark, mark with { State = "Active" });
        Assert.NotEqual(mark, mark with { State = "idle" });
        Assert.NotEqual(mark, mark with { Text = string.Empty });
        Assert.NotEqual(mark, mark with { Title = mark.Title + " " });
        Assert.NotEqual(mark, mark with { Link = null });
        Assert.NotEqual(mark, mark with { Link = ToPortal });
        Assert.NotEqual(mark, mark with { Trend = null });
        Assert.NotEqual(mark, mark with { Trend = new RuntimeTrend(Trend.Points, "SQL commands per minute") });
        Assert.Single(new HashSet<RuntimeEdgeMark> { mark, mark with { } });

        var (id, state, number, unit, text, title, link, trend) = mark;

        Assert.Equal("app_ui_primary-to-sqldb", id);
        Assert.Equal("active", state);
        Assert.Equal("12", number);
        Assert.Equal("sql/min", unit);
        Assert.Equal("queries", text);
        Assert.Equal("The web app to the database.", title);
        Assert.Same(ToTask, link);
        Assert.Same(Trend, trend);

        // The whole mark, every part named, is written in the order the record declares them.
        using var full = JsonDocument.Parse(WrittenEdge(mark).GetRawText());

        Assert.Equal(["id", "state", "number", "unit", "text", "title", "link", "trend"], full.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.Equal("queries", full.RootElement.GetProperty("text").GetString());
        Assert.Equal([0, 0.5, 1], full.RootElement.GetProperty("trend").GetProperty("points").EnumerateArray().Select(point => point.GetDouble()));

        ViewerCulture.German(() => Assert.Equal("RuntimeEdgeMark { Id = , State = , Number = , Unit = , Text = , Title = , Link = , Trend =  }", nothing.ToString()));
    }

    // -------------------------------------------------------------------- a deployment's mark, and a link

    /// <summary>A deployment's mark carries the word of its state, the sentence of it and where it leads - the last
    /// being the part a caller may leave out, since the file may name no address for the task. The state says the
    /// shape of the dot and the sentence its tooltip: the record joins neither.</summary>
    [Fact]
    public void ADeploymentsMarkCarriesItsStateItsSentenceAndWhereItLeads()
    {
        var mark = new RuntimeDeployment("waiting", "cmdemo2-ui 2.4.43 waits for a sign-off in uat (1 min so far)", ToTask);

        Assert.Equal("waiting", mark.State);
        Assert.Equal("cmdemo2-ui 2.4.43 waits for a sign-off in uat (1 min so far)", mark.Title);
        Assert.Same(ToTask, mark.Link);

        // The dot of a deployment the file gives no address for is still a dot.
        var silent = new RuntimeDeployment("ended", "cmdemo2-dashboard 1.0.8 reached uat 2 min ago");

        Assert.Null(silent.Link);
        Assert.Equal(silent, silent with { Link = null });
        Assert.NotEqual(silent, mark with { State = "ended", Title = silent.Title });
        Assert.NotEqual(mark, mark with { State = "queued" });
        Assert.NotEqual(mark, mark with { Title = mark.Title.ToUpperInvariant() });
        Assert.NotEqual(mark, mark with { Link = ToPortal });
        Assert.Equal(4, new HashSet<RuntimeDeployment> { mark, silent, mark with { State = "queued" }, mark with { Link = ToPortal } }.Count);

        var (state, title, link) = mark;

        Assert.Equal("waiting", state);
        Assert.Equal(mark.Title, title);
        Assert.Same(ToTask, link);
        Assert.Equal(mark, new RuntimeDeployment(state, title, link));
        Assert.Equal("RuntimeDeployment { State = waiting, Title = cmdemo2-ui 2.4.43 waits for a sign-off in uat (1 min so far), Link = RuntimeLink { Href = https://octopus.example.net/app#/Spaces-1/tasks/ServerTasks-1, Title = " + TaskTitle + " } }", mark.ToString());

        // A mark of nothing at all stands, and is written as nothing at all: the writer leaves both its parts out.
        var nothing = new RuntimeDeployment(null!, null!);

        Assert.Equal("RuntimeDeployment { State = , Title = , Link =  }", nothing.ToString());
        Assert.Equal("{}", Written(nothing).GetProperty("deployment").GetRawText());
        Assert.Equal(["state", "title"], Written(silent).GetProperty("deployment").EnumerateObject().Select(property => property.Name));
        Assert.Equal(
            ["state", "title", "link"],
            Written(mark).GetProperty("deployment").EnumerateObject().Select(property => property.Name));
    }

    /// <summary>A link is an address and the words that say where it goes; both are kept exactly as they are handed,
    /// and <see cref="RuntimeLink.To" /> makes one out of a <see cref="Uri" /> by asking the address what it is - the
    /// escaped form the browser is given, not the string the manifest happened to spell.</summary>
    [Fact]
    public void ALinkCarriesTheAddressItWasGivenAndTheWordsAboutIt()
    {
        var link = new RuntimeLink(Href, TaskTitle);

        Assert.Equal(Href, link.Href);
        Assert.Equal(TaskTitle, link.Title);
        Assert.Equal(link, new RuntimeLink(Href, TaskTitle));
        Assert.NotEqual(link, link with { Href = string.Empty });
        Assert.NotEqual(link, link with { Href = null! });
        Assert.NotEqual(link, link with { Title = null! });
        Assert.NotEqual(link, link with { Title = "  " });
        Assert.NotEqual(link, link with { Href = Href.ToUpperInvariant() });
        Assert.True(link == new RuntimeLink(Href, TaskTitle));
        Assert.Equal(link.GetHashCode(), new RuntimeLink(Href, TaskTitle).GetHashCode());

        var (href, title) = link;

        Assert.Equal(Href, href);
        Assert.Equal(TaskTitle, title);
        Assert.Equal("RuntimeLink { Href = , Title =  }", new RuntimeLink(null!, null!).ToString());
        Assert.False(link.Equals(Href));
        Assert.False(link.Equals(new RuntimeTextPart(Href, link)));
        Assert.False(link.Equals(new RuntimeName("sub", link)));

        // The empty address is an address the record keeps; only the writer's rule about nothing at all leaves it out.
        Assert.Equal("""{"href":"","title":""}""", Written(new RuntimeLink(string.Empty, string.Empty)).GetProperty("link").GetRawText());
    }

    /// <summary>The address a <see cref="Uri" /> is asked for is the one the script is handed, escaped and normalised
    /// the way <see cref="Uri" /> says it is: a space written as <c>%20</c>, a letter outside the Latin alphabet
    /// written as the bytes it is, a host put down to lower case and a <c>..</c> walked out of the path - while a
    /// plus in a path and a fragment after the <c>#</c> stand exactly where the manifest put them.</summary>
    [Fact]
    public void ALinkIsMadeOfTheAddressTheUriSaysAndNotTheOneItWasBuiltFrom()
    {
        Assert.Equal(
            "https://octopus.example.net/app#/Spaces-1/projects/demo%20ui/releases/2.4.15+7",
            RuntimeLink.To(new Uri("https://octopus.example.net/app#/Spaces-1/projects/demo ui/releases/2.4.15+7"), TaskTitle)!.Href);
        Assert.Equal(
            "https://a.example.net/a%20b?x=%C3%A4#fr%C3%A4",
            RuntimeLink.To(new Uri("https://a.example.net/a b?x=ä#frä"), "a")!.Href);
        Assert.Equal(
            "https://a.example.net/y",
            RuntimeLink.To(new Uri("HTTPS://A.example.net:443/x/../y"), "a")!.Href);

        // The title goes with the link as it stands, in more than one line and in no words at all.
        Assert.Equal(
            new RuntimeLink("https://a.example.net/", "one line\nanother line"),
            RuntimeLink.To(new Uri("https://a.example.net/"), "one line\nanother line"));
        Assert.Equal(TaskTitle, RuntimeLink.To(new Uri(Portal), TaskTitle)!.Title);

        // The words are handed over as they stand, and no words at all is a title the record will hold.
        Assert.Null(RuntimeLink.To(new Uri(Portal), null!)!.Title);
        Assert.Equal(string.Empty, RuntimeLink.To(new Uri(Portal), string.Empty)!.Title);

        // No address is no link, whichever words it was given; a relative one is no address to lead anywhere.
        Assert.Null(RuntimeLink.To(null, TaskTitle));
        Assert.Null(RuntimeLink.To(null, null!));
        var error = Assert.Throws<InvalidOperationException>(() => RuntimeLink.To(new Uri("/health", UriKind.Relative), "the health path"));

        Assert.Contains("relative", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The decision held once for every link of the page (#1556), written out beside the looser line above:
    /// a relative address is refused by <see cref="Uri.AbsoluteUri" /> itself, so the caller of
    /// <see cref="RuntimeLink.To" /> hears the address's own sentence and not which argument was wrong - <c>href</c>
    /// is named nowhere in it, and a guard of this record would have been one of the <see cref="ArgumentException" />
    /// family, which is why the type is pinned as exactly <see cref="InvalidOperationException" />. The sentence is
    /// pinned word for word, as the very sentence the same <see cref="Uri" /> says when its <c>AbsoluteUri</c> is read
    /// directly, and as the one sentence <see cref="LinkText.Release" /> says for a release page - the two builders of
    /// a link agreeing rather than each inventing its own refusal. A <em>probe's</em> base address is the caller's own
    /// argument, and #1544 may name it; nothing here holds <see cref="ProbeUrl" /> to either wording.</summary>
    [Fact]
    public void TheRefusalOfARelativeAddressIsTheAddresssOwnSentenceInBothPlaces()
    {
        var relative = new Uri("/health", UriKind.Relative);

        var refused = Assert.Throws<InvalidOperationException>(() => RuntimeLink.To(relative, "the health path"));

        // Exactly the framework's exception, and so not one of the ArgumentException family - the tree a guard of this
        // record naming its parameter would have come from - since neither of the two can derive from the other.
        Assert.IsType<InvalidOperationException>(refused);
        Assert.Equal("This operation is not supported for a relative URI.", refused.Message);
        Assert.DoesNotContain("href", refused.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("address", refused.Message, StringComparison.Ordinal);

        // The address's own words: read the same relative Uri directly and the sentence does not change by a letter.
        Assert.Equal(Record.Exception(() => _ = relative.AbsoluteUri)!.Message, refused.Message);

        // One decision, one sentence: the release page refuses a relative project address with the very same words.
        var releaseRefused = Assert.Throws<InvalidOperationException>(() => LinkText.Release(relative, "2.4.15"));

        Assert.IsType<InvalidOperationException>(releaseRefused);
        Assert.Equal(refused.Message, releaseRefused.Message);
    }

    // ---------------------------------------------------------------------- the surface of every record

    /// <summary>The declared shape of one record of the payload - a row of the table below, not the surface
    /// <see cref="RecordSurface" /> reads - with the parts it is named by in the order it declares them, the words
    /// that say which of them a caller may leave out and which of them may be nothing, and the methods it has besides
    /// the four every record is asked.</summary>
    /// <param name="Type">The record type itself.</param>
    /// <param name="Parts">Its parts, in the order the constructor names them.</param>
    /// <param name="NamedByNobody">The parts a caller may leave out, which the constructor gives a default.</param>
    /// <param name="MayBeNothing">The parts that may be nothing at all, as the record declares them.</param>
    /// <param name="Methods">The methods written beside the parts, and not the members every record is asked.</param>
    public sealed record DeclaredShape(
        Type Type,
        string[] Parts,
        string[] NamedByNobody,
        string[] MayBeNothing,
        string[] Methods);

    /// <summary>The records <c>RuntimePayload.cs</c> declares, bar <see cref="RuntimeTrend" /> and
    /// <see cref="RuntimeName" />, whose whole surface - parts, copy and all - <see cref="RuntimeTrendEdgeCaseTests" />
    /// and <see cref="RuntimeTrendAndNameTests" /> already pin.</summary>
    private static readonly DeclaredShape[] Records =
    [
        new(
            typeof(RuntimeTile),
            ["Alias", "State", "Label", "Facts", "Lines", "History", "Title", "Link", "NameLink", "Deployment"],
            ["Link", "NameLink", "Deployment"],
            ["Facts", "History", "Link", "NameLink", "Deployment"],
            []),
        new(
            typeof(RuntimeTileLine),
            ["Text", "Tone", "Parts", "Trend", "Marks"],
            ["Tone", "Parts", "Trend", "Marks"],
            ["Parts", "Trend", "Marks"],
            ["Of"]),
        new(typeof(RuntimeTextPart), ["Text", "Link"], ["Link"], ["Link"], []),
        new(typeof(RuntimeCheckMark), ["State", "Title"], [], [], []),
        new(typeof(RuntimeRegionMark), ["Alias", "State", "Label", "NameLink"], ["NameLink"], ["NameLink"], []),
        new(
            typeof(RuntimeEdgeMark),
            ["Id", "State", "Number", "Unit", "Text", "Title", "Link", "Trend"],
            ["Link", "Trend"],
            ["Number", "Unit", "Text", "Link", "Trend"],
            []),
        new(typeof(RuntimeDeployment), ["State", "Title", "Link"], ["Link"], ["Link"], []),
        new(typeof(RuntimeLink), ["Href", "Title"], [], [], ["To"]),
        new(typeof(RuntimePayload), ["Nodes", "Regions", "Edges", "Names"], ["Names"], ["Names"], ["ToJson"]),
    ];

    /// <summary>One row per record of <c>RuntimePayload.cs</c>, as the fixture above describes it.</summary>
    public static IEnumerable<object[]> TheRecordsOfOneUpdate() => [.. Records.Select(record => new object[] { record })];

    /// <summary>A part of the payload cannot be added, renamed or made settable from outside without this list saying
    /// so: each record is sealed, falls straight out of <see cref="object" />, is one of nothing but itself, carries
    /// exactly the parts it is named by - in the order the constructor names them, of the type the constructor says,
    /// readable and initable and no more - and has no public member besides its parts, the four every record is asked,
    /// and the few methods written down beside them. The one default that is a word is a line's tone, <c>plain</c>;
    /// every other default is the nothing a caller leaves out of the update.</summary>
    [Theory]
    [MemberData(nameof(TheRecordsOfOneUpdate))]
    public void EveryRecordOfThePayloadCarriesJustThePartsItIsNamedBy(DeclaredShape record)
    {
        var nullability = new NullabilityInfoContext();
        var properties = record.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.Equal(record.Parts.ToList(), [.. properties.Select(property => property.Name)]);

        // The one public constructor, naming its parts as the properties are named; the copy constructor the compiler
        // writes for a record is no caller's to call.
        var constructor = Assert.Single(record.Type.GetConstructors());
        var parameters = constructor.GetParameters();

        Assert.Equal(record.Parts.ToList(), [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal(properties.Select(property => property.PropertyType).ToList(), [.. parameters.Select(parameter => parameter.ParameterType)]);
        Assert.Equal(
            [.. record.Parts.Select(part => record.NamedByNobody.Contains(part, StringComparer.Ordinal))],
            [.. parameters.Select(parameter => parameter.HasDefaultValue)]);
        Assert.All(
            parameters.Where(parameter => parameter.HasDefaultValue),
            parameter => Assert.Equal(parameter.Name == "Tone" ? "plain" : null, parameter.DefaultValue));
        Assert.Equal(
            [.. record.Parts.Select(part => record.MayBeNothing.Contains(part, StringComparer.Ordinal))],
            [.. parameters.Select(parameter => nullability.Create(parameter).WriteState == NullabilityState.Nullable)]);
        Assert.Equal(
            [.. record.Parts.Select(part => record.MayBeNothing.Contains(part, StringComparer.Ordinal))],
            [.. properties.Select(property => nullability.Create(property).ReadState == NullabilityState.Nullable)]);

        // A part is read, and given a new value only by the copy a caller asks for: nothing about a payload is
        // changed under the script's feet.
        Assert.All(
            properties,
            property =>
            {
                Assert.True(property.CanRead);
                Assert.NotNull(property.SetMethod);
                Assert.True(property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(System.Runtime.CompilerServices.IsExternalInit)));
            });

        Assert.True(record.Type.IsSealed);
        Assert.Equal(typeof(object), record.Type.BaseType);
        Assert.Equal([typeof(IEquatable<>).MakeGenericType(record.Type)], [.. record.Type.GetInterfaces()]);
        Assert.Empty(record.Type.GetEvents());
        Assert.Equal(Expected(record.Parts, record.Methods), PublicSurface(record.Type));

        // A part of the payload is a word, another of its records, or a read-only list of one of them: nothing else
        // is handed to the script, and no part of it is a number, a moment or a verdict the page would have to make.
        var carried = new[] { typeof(RuntimePayload), typeof(RuntimeTile), typeof(RuntimeTileLine), typeof(RuntimeTextPart), typeof(RuntimeCheckMark), typeof(RuntimeRegionMark), typeof(RuntimeEdgeMark), typeof(RuntimeDeployment), typeof(RuntimeLink), typeof(RuntimeName), typeof(RuntimeTrend) };

        Assert.All(
            properties,
            property =>
            {
                var held = property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)
                    ? property.PropertyType.GetGenericArguments()[0]
                    : property.PropertyType;

                Assert.True(
                    held == typeof(string) || carried.Contains(held),
                    $"{record.Type.Name}.{property.Name} carries {held}, which is no word and no part of the payload.");
            });
    }

    /// <summary>The words the script is handed are made by the context declared beside the record and by nothing else:
    /// the payload's own type info, and the type info of every part of it, come from that one context and share its
    /// one set of options - so an update is written by what was compiled with the page, and a part the context has
    /// never heard of could not be added to the payload without being written down here as well.</summary>
    [Fact]
    public void TheContextBesideThePayloadKnowsEveryPartOfIt()
    {
        var known = new JsonTypeInfo[]
        {
            RuntimePayloadJson.Default.RuntimePayload,
            RuntimePayloadJson.Default.RuntimeTile,
            RuntimePayloadJson.Default.RuntimeTileLine,
            RuntimePayloadJson.Default.RuntimeTextPart,
            RuntimePayloadJson.Default.RuntimeCheckMark,
            RuntimePayloadJson.Default.RuntimeRegionMark,
            RuntimePayloadJson.Default.RuntimeEdgeMark,
            RuntimePayloadJson.Default.RuntimeDeployment,
            RuntimePayloadJson.Default.RuntimeLink,
            RuntimePayloadJson.Default.RuntimeName,
            RuntimePayloadJson.Default.RuntimeTrend,
        };

        Assert.Equal(
            [
                typeof(RuntimePayload), typeof(RuntimeTile), typeof(RuntimeTileLine), typeof(RuntimeTextPart),
                typeof(RuntimeCheckMark), typeof(RuntimeRegionMark), typeof(RuntimeEdgeMark), typeof(RuntimeDeployment),
                typeof(RuntimeLink), typeof(RuntimeName), typeof(RuntimeTrend),
            ],
            [.. known.Select(info => info.Type)]);
        Assert.All(known, info => Assert.Same(RuntimePayloadJson.Default.Options, info.Options));
        Assert.All(known, info => Assert.Equal(JsonTypeInfoKind.Object, info.Kind));
        Assert.Same(RuntimePayloadJson.Default.RuntimePayload, RuntimePayloadJson.Default.RuntimePayload);
    }

    // ---------------------------------------------------------------------------------------- the machinery

    /// <summary>The public members a record declares itself: its parts, the methods that are not the get and the set of
    /// one, and the two operators kept although the compiler names them specially - since they are the <c>==</c> and
    /// the <c>!=</c> a caller writes - with the record's compiler-named clone and its print walk past. The constructor
    /// is no method, so the test above names it.</summary>
    private static List<string> PublicSurface(Type type) =>
        [.. type
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The list the members of one record fall into once they are ordered: its parts, the few methods written
    /// beside them, and the four every record is asked - falling apart, comparing, hashing and saying itself.</summary>
    private static List<string> Expected(string[] parts, string[] methods) =>
        [.. parts.Concat(methods).Concat(["Deconstruct", "Equals", "GetHashCode", "ToString", "op_Equality", "op_Inequality"]).Order(StringComparer.Ordinal)];

    /// <summary>The pieces a line is made of, each with the words it says and the place it leads, so that a test about
    /// how a line keeps its pieces says the pieces and not the record they are held in.</summary>
    private static IReadOnlyList<RuntimeTextPart> PiecesOf(params (string Text, RuntimeLink? Link)[] pieces) =>
        [.. pieces.Select(piece => new RuntimeTextPart(piece.Text, piece.Link))];
}
