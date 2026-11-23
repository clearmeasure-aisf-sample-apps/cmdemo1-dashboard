using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of the public members of
/// <c>src/Dashboard/Runtime/RuntimePayload.cs</c> that the payload's own two files leave open:
/// <see cref="RuntimePayloadJson" />, the writer declared beside the records, and the two records no other file has
/// ever let fall apart, <see cref="RuntimeTextPart" /> and <see cref="RuntimeCheckMark" />. The context is the one
/// thing in the file that decides
/// anything on its own - what a part is called in the bytes, whether a word that is nothing is written, which types the
/// script may be handed at all - so what stands here is its own surface by reflection, the type each of its members
/// writes, the rows it knows being exactly the row-shaped parts of the records, the answers it gives a caller who asks
/// it for a type, the lock on the options it was declared with, the bytes a context of other options makes of the very
/// same record, the names its reader looks for, every one of the eleven records written with all its parts and with
/// none, and each part read back through the member written for it.
/// <para>
/// What the other files already pin is left alone. <see cref="RuntimePayloadEdgeCaseTests" /> pins every part of every
/// record coming back as the part it went in as, the lists held rather than copied, the parts a caller may leave out
/// standing absent, the members every record is asked, the public surface of nine of the eleven records, that a payload
/// is written by <c>RuntimePayloadJson.Default.RuntimePayload</c> and that its options are camelCase without nulls, and
/// the eleven <see cref="JsonTypeInfo" /> members it happens to name.
/// <see cref="RuntimePayloadBoundaryEdgeCaseTests" /> pins the escapes of one whole update, the order of a thousand
/// boxes, a height that is no finite number being refused, the same link written in full in nine places, the two
/// operators of every record against nothing, and an update read back through the payload's own member.
/// <see cref="RuntimeTrendEdgeCaseTests" />, <see cref="RuntimeTrendAndNameTests" /> and
/// <see cref="RuntimeNameEdgeCaseTests" /> pin <see cref="RuntimeTrend" /> and <see cref="RuntimeName" /> whole.
/// </para>
/// <para>
/// What follows is therefore the writer, which none of those files asks a question of beyond the members it happens to
/// call: the list of them, the types behind them, the door of <c>Default</c>, the two constructors a caller may walk
/// past, the answers to <c>GetTypeInfo</c>, the lock on the options, and the two records whose parts nobody has let
/// fall. Nothing here reaches the network or asks a clock what time it is: every value below is one the test brought,
/// and no update is built out of a manifest.
/// </para>
/// <para>
/// The name: <c>RuntimePayloadEdgeCaseTests</c>, which is what this file was asked to be called, was written by an
/// earlier turn of the same work and pins the records; so this is the companion the repo already uses where a type is
/// asked twice more - <see cref="RuntimeManifestSurfaceEdgeCaseTests" /> and
/// <see cref="ComplexityFactsSurfaceEdgeCaseTests" /> - and it keeps to what those files leave open rather than
/// restating them.
/// </para>
/// </summary>
public class RuntimePayloadSurfaceEdgeCaseTests
{
    private const string Word = "cmdemo2-ui 2.4.43 reaches uat";
    private const string Href = "https://octopus.example.net/app#/Spaces-1/tasks/ServerTasks-1";
    private const string TaskWords = "The deployment's task in Octopus Deploy (opens in a new tab; Octopus Deploy asks you to sign in)";
    private const string TrendWords = "Requests per minute, last 3 checks: 0 to 40, now 20";

    /// <summary>A tile with the seven parts every caller must name and nothing beside them: the bytes of one written in
    /// the records' own PascalCase show every part the writer otherwise passes by.</summary>
    private static readonly RuntimeTile Seven = new("a", "s", "L", null, [], null, "t");

    private static readonly RuntimeLink ToTask = new(Href, TaskWords);
    private static readonly RuntimeTrend Trend = new(new List<double> { 0, 0.5, 1 }, TrendWords);
    private static readonly RuntimeCheckMark Mark = new("healthy", "API: Healthy. Took under 0.1 ms.");
    private static readonly RuntimeTextPart Piece = new("2.4.43 ", ToTask);
    private static readonly RuntimeTileLine Line = new(Word, "deploying", [Piece], Trend, [Mark]);
    private static readonly RuntimeDeployment Deployment = new("executing", "deploying cmdemo2-ui 2.4.43 to uat", ToTask);
    private static readonly RuntimeTile Tile = new("app_ui_primary", "healthy", "Healthy", "HTTP 200", [Line], ["checking", "healthy"], "cmdemo2-ui primary: healthy.", ToTask, ToTask, Deployment);
    private static readonly RuntimeRegionMark Region = new("region_primary", "serving", "serving traffic", ToTask);
    private static readonly RuntimeEdgeMark Edge = new("fd_ui-to-app_ui_primary", "active", "12", "req/min", "carries the traffic", "The Front Door to the web app.", ToTask, Trend);
    private static readonly RuntimeName Boxed = new("sub", ToTask);
    private static readonly RuntimePayload Payload = new([Tile], [Region], [Edge], [Boxed]);

    /// <summary>The eleven records of the file, each with the value this test brings for it - every part of it named, so
    /// that one walk may ask the writer for all of them.</summary>
    private static readonly (Type Type, object Value)[] TheEleven =
    [
        (typeof(RuntimePayload), Payload),
        (typeof(RuntimeTile), Tile),
        (typeof(RuntimeTileLine), Line),
        (typeof(RuntimeTextPart), Piece),
        (typeof(RuntimeCheckMark), Mark),
        (typeof(RuntimeRegionMark), Region),
        (typeof(RuntimeEdgeMark), Edge),
        (typeof(RuntimeDeployment), Deployment),
        (typeof(RuntimeLink), ToTask),
        (typeof(RuntimeName), Boxed),
        (typeof(RuntimeTrend), Trend),
    ];

    /// <summary>The records that hold a row, their own or a part's: a record weighs a row as the row it is <em>held</em>
    /// in, and a reader builds its own, so these five are pinned across the bytes as bytes that do not move rather than
    /// as one record on either side of them.</summary>
    private static readonly Type[] HoldRows =
    [
        typeof(RuntimePayload), typeof(RuntimeTile), typeof(RuntimeTileLine), typeof(RuntimeEdgeMark), typeof(RuntimeTrend),
    ];

    /// <summary>The three parts of an update that stand one by one or not at all: no record holds a row of them, and no
    /// member of the writer is a row of them either.</summary>
    private static readonly Type[] NeverARow = [typeof(RuntimeDeployment), typeof(RuntimeLink), typeof(RuntimeTrend)];

    // ---------------------------------------------------------------------------- the context's own surface

    /// <summary>The writer beside the records is sealed, falls straight out of <see cref="JsonSerializerContext" />, is
    /// made by two public constructors and says four things: the one door of <c>Default</c>, the one method that answers
    /// a type, and one member for every type it was told about - the eleven records, the nine rows they hold, and the two
    /// kinds of thing a part is made of, a word and a height. A twelfth record, a tenth row or a third number type
    /// joining an update has to be written down here as well, which is the point.</summary>
    [Fact]
    public void TheContextBesideTheRecordsDeclaresJustTheTypesAnUpdateIsMadeOf()
    {
        var context = typeof(RuntimePayloadJson);
        var declared = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        Assert.True(context.IsSealed);
        Assert.Equal(typeof(JsonSerializerContext), context.BaseType);
        Assert.Empty(context.GetEvents(declared));
        Assert.Empty(context.GetFields(declared));
        Assert.All(context.GetConstructors(), constructor => Assert.True(constructor.IsPublic));
        Assert.Equal([0, 1], [.. context.GetConstructors().Select(constructor => constructor.GetParameters().Length).Order()]);
        Assert.Equal(typeof(JsonSerializerOptions), context.GetConstructors().Single(constructor => constructor.GetParameters().Length == 1).GetParameters()[0].ParameterType);

        // Nothing to call besides the one question a caller asks a context.
        Assert.Equal(["GetTypeInfo"], [.. context.GetMethods(declared).Where(method => !method.IsSpecialName).Select(method => method.Name)]);

        var properties = DeclaredProperties();

        Assert.Equal(
            [
                "Default", "Double",
                "IReadOnlyListDouble", "IReadOnlyListRuntimeCheckMark", "IReadOnlyListRuntimeEdgeMark", "IReadOnlyListRuntimeName",
                "IReadOnlyListRuntimeRegionMark", "IReadOnlyListRuntimeTextPart", "IReadOnlyListRuntimeTile", "IReadOnlyListRuntimeTileLine",
                "IReadOnlyListString",
                "RuntimeCheckMark", "RuntimeDeployment", "RuntimeEdgeMark", "RuntimeLink", "RuntimeName", "RuntimePayload",
                "RuntimeRegionMark", "RuntimeTextPart", "RuntimeTile", "RuntimeTileLine", "RuntimeTrend",
                "String",
            ],
            [.. properties.Select(property => property.Name)]);

        // One of them is the door, and it is the only static thing there is to call.
        var door = Assert.Single(properties, property => property.Name == "Default");

        Assert.True(door!.GetMethod!.IsStatic);
        Assert.Equal(typeof(RuntimePayloadJson), door.PropertyType);
        Assert.Same(RuntimePayloadJson.Default, door.GetValue(null));
        Assert.Same(RuntimePayloadJson.Default, RuntimePayloadJson.Default);
        Assert.All(
            properties.Where(property => property.Name != "Default"),
            property =>
            {
                Assert.False(property.GetMethod!.IsStatic);
                Assert.True(typeof(JsonTypeInfo).IsAssignableFrom(property.PropertyType));
            });

        // And the context a caller may build by hand is no <c>Default</c>: another instance, other options, other bytes.
        Assert.NotSame(RuntimePayloadJson.Default, new RuntimePayloadJson());
    }

    /// <summary>Each member of the writer is named after the very type it writes and answers with a
    /// <see cref="JsonTypeInfo" /> standing on it - a record by its own name, a row by the name of what stands in it, a
    /// word and a height by theirs - and the same type info is what the one door hands out when it is asked.</summary>
    [Fact]
    public void EachMemberOfTheWriterWritesTheTypeItsOwnNameSpells()
    {
        var infos = DeclaredInfos();

        Assert.Equal(22, infos.Count);
        Assert.All(
            DeclaredProperties().Where(property => property.Name != "Default"),
            property =>
            {
                var info = (JsonTypeInfo)property.GetValue(RuntimePayloadJson.Default)!;

                Assert.Equal(NameOf(info.Type), property.Name);
                Assert.Same(info, RuntimePayloadJson.Default.GetTypeInfo(info.Type));
                Assert.Same(RuntimePayloadJson.Default.Options, info.Options);
                Assert.Equal(info.Kind == JsonTypeInfoKind.Enumerable, info.ElementType is not null);
            });

        // The eleven records are written as objects, the nine rows as arrays, the word and the height as themselves.
        Assert.Equal(11, infos.Count(info => info.Kind == JsonTypeInfoKind.Object));
        Assert.Equal(9, infos.Count(info => info.Kind == JsonTypeInfoKind.Enumerable));
        Assert.Equal(
            [typeof(double), typeof(string)],
            [.. infos.Where(info => info.Kind == JsonTypeInfoKind.None).Select(info => info.Type)]);

        // The row of each part holds the record its name says, and no row is of a type the file does not declare: the
        // nine rows of the writer are the nine rows of the update, in the order the writer names them.
        Assert.Equal(
            [typeof(double), typeof(RuntimeCheckMark), typeof(RuntimeEdgeMark), typeof(RuntimeName), typeof(RuntimeRegionMark),
                typeof(RuntimeTextPart), typeof(RuntimeTile), typeof(RuntimeTileLine), typeof(string)],
            [.. infos.Where(info => info.Kind == JsonTypeInfoKind.Enumerable).Select(info => info.ElementType!)]);
    }

    /// <summary>The rows the writer knows are exactly the row-shaped parts the records hold, not one more and not one
    /// less: an update has nine lists between its eleven records, so a tenth row, or a row of a type this file does not
    /// declare, cannot arrive without this comparison saying so. No row of deployments, of links or of sparklines
    /// exists: those three parts of an update stand one by one, or not at all.</summary>
    [Fact]
    public void TheRowsTheWriterKnowsAreTheRowShapedPartsOfTheRecords()
    {
        List<Type> held =
            [.. TheEleven
                .SelectMany(part => part.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .Select(property => property.PropertyType)
                .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
                .Distinct()
                .OrderBy(type => NameOf(type), StringComparer.Ordinal)];
        List<Type> known =
            [.. DeclaredInfos()
                .Where(info => info.Kind == JsonTypeInfoKind.Enumerable)
                .Select(info => info.Type)
                .OrderBy(type => NameOf(type), StringComparer.Ordinal)];

        Assert.Equal(9, held.Count);
        Assert.Equal(held, known);
        Assert.Equal(
            [
                "IReadOnlyListDouble", "IReadOnlyListRuntimeCheckMark", "IReadOnlyListRuntimeEdgeMark",
                "IReadOnlyListRuntimeName", "IReadOnlyListRuntimeRegionMark", "IReadOnlyListRuntimeTextPart",
                "IReadOnlyListRuntimeTile", "IReadOnlyListRuntimeTileLine", "IReadOnlyListString",
            ],
            [.. known.Select(NameOf)]);
        Assert.DoesNotContain(known, type => NeverARow.Contains(type.GetGenericArguments()[0]));
    }

    // --------------------------------------------------------------------------- what the writer answers

    /// <summary>The one door answers nothing else of the app: no part of a manifest, no trend of the health side, no
    /// number but the heights' own, no moment, no verdict and no row of any of them - every type the update is made of
    /// is answered by the member <see cref="EachMemberOfTheWriterWritesTheTypeItsOwnNameSpells" /> names. A caller who
    /// asks for a type the file does not carry is answered with no type info at all rather than with a writer of last
    /// resort, which is what keeps a part nobody decided the bytes of out of the script's hands.</summary>
    [Fact]
    public void TheWriterAnswersEveryTypeOfAnUpdateAndNoOther()
    {
        // Nothing of the file's neighbours - the manifest, the health side, the page - is written by this context.
        foreach (var outside in new[]
                 {
                     typeof(RuntimeManifest), typeof(RuntimeNode), typeof(RuntimeRegion), typeof(RuntimeEdge),
                     typeof(RuntimeFrame), typeof(RuntimeUnplaced), typeof(RuntimeNodeKind), typeof(RuntimeEdgeKind),
                     typeof(Trend), typeof(int), typeof(long), typeof(bool), typeof(decimal), typeof(char),
                     typeof(Guid), typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan), typeof(Uri),
                     typeof(RuntimeTile[]), typeof(List<string>), typeof(IEnumerable<string>), typeof(IDictionary<string, string>),
                 })
        {
            Assert.Null(RuntimePayloadJson.Default.GetTypeInfo(outside));
        }

        var error = Assert.Throws<ArgumentNullException>(() => RuntimePayloadJson.Default.GetTypeInfo(null!));

        Assert.Equal("type", error.ParamName);
    }

    /// <summary>The options that make an update camelCase with its nothing left out are locked the day they are handed
    /// out: the context's own cannot be moved once the writer stands on them, and a context built on a caller's own
    /// options locks those instead - so no page, no monitor and no test can re-decide the bytes of the next update
    /// between one paint and the next.</summary>
    [Fact]
    public void TheOptionsThatMakeTheScriptsBytesAreLockedOnceTheWriterStandsOnThem()
    {
        var declared = RuntimePayloadJson.Default.Options;

        Assert.True(declared.IsReadOnly);
        Assert.Throws<InvalidOperationException>(() => declared.DefaultIgnoreCondition = JsonIgnoreCondition.Never);

        // Handing a context fresh options locks the caller's own instance, whichever way it is filled in.
        var handed = new JsonSerializerOptions();

        Assert.False(handed.IsReadOnly);

        var made = new RuntimePayloadJson(handed);

        Assert.True(handed.IsReadOnly);
        Assert.Same(handed, made.Options);
        Assert.NotSame(declared, made.Options);
        Assert.Throws<InvalidOperationException>(() => handed.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
        Assert.Equal(
            """{"Alias":"a","State":"s","Label":"L","Facts":null,"Lines":[],"History":null,"Title":"t","Link":null,"NameLink":null,"Deployment":null}""",
            JsonSerializer.Serialize(Seven, made.RuntimeTile));
    }

    /// <summary>The records carry no opinion about their bytes: the tile the update hands the script in camelCase with
    /// its absences left out is written in the records' own PascalCase with every nothing spelled out by a context of
    /// plain options, and in the very same bytes as the script's by a context of the two options the file declares
    /// beside it. What the page reads is that declaration, and nothing else.</summary>
    [Fact]
    public void AContextOfAnotherSetOfOptionsWritesAnotherUpdate()
    {
        const string declared = """{"alias":"a","state":"s","label":"L","lines":[],"title":"t"}""";
        var agreed = new RuntimePayloadJson(
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            });

        Assert.Equal(declared, JsonSerializer.Serialize(Seven, RuntimePayloadJson.Default.RuntimeTile));
        Assert.Equal(declared, JsonSerializer.Serialize(Seven, agreed.RuntimeTile));

        using var whole = JsonDocument.Parse(new RuntimePayload([Seven], [], []).ToJson());

        Assert.Equal(declared, whole.RootElement.GetProperty("nodes")[0].GetRawText());

        // A context keeps its own metadata, once per instance, and never lends it to another.
        Assert.Same(agreed.RuntimeTile, agreed.RuntimeTile);
        Assert.NotSame(agreed.RuntimeTile, RuntimePayloadJson.Default.RuntimeTile);
        Assert.NotSame(agreed.RuntimePayload, RuntimePayloadJson.Default.RuntimePayload);
        Assert.Same(agreed.Options, agreed.RuntimePayload.Options);
        Assert.Same(agreed.Options, agreed.IReadOnlyListRuntimeTile.Options);
    }

    // ---------------------------------------------------------------------------------------- the bytes read

    /// <summary>The reader looks for the names the writer gives and for no others: a part spelled the way the record
    /// spells it is passed by, a name the update does not carry is walked past rather than refused, and each of the four
    /// lists reads back as the row it was written as or as the nothing it was left out for.</summary>
    [Fact]
    public void TheReaderLooksForTheNamesTheWriterGivesAndPassesByTheRest()
    {
        // The records' own PascalCase is no name to the reader: nothing of it is filled in, so nothing is written back.
        Assert.Equal("{}", JsonSerializer.Deserialize("""{"Nodes":[],"REGIONS":[],"Edges":[],"Names":[]}""", RuntimePayloadJson.Default.RuntimePayload)!.ToJson());

        // A word the update does not carry is neither refused nor kept: the payload of a future page still reads.
        Assert.Equal(
            """{"nodes":[],"regions":[],"edges":[]}""",
            JsonSerializer.Deserialize("""{"nodes":[],"regions":[],"edges":[],"stray":1,"more":{"a":[1,2]},"Alias":"a"}""", RuntimePayloadJson.Default.RuntimePayload)!.ToJson());

        // The four names, as a row of nothing and as no row at all.
        var rows = JsonSerializer.Deserialize("""{"nodes":[],"regions":[],"edges":[],"names":[]}""", RuntimePayloadJson.Default.RuntimePayload)!;

        Assert.Empty(rows.Nodes);
        Assert.Empty(rows.Regions);
        Assert.Empty(rows.Edges);
        Assert.Empty(rows.Names!);

        var absences = JsonSerializer.Deserialize("""{"nodes":null,"regions":null,"edges":null,"names":null}""", RuntimePayloadJson.Default.RuntimePayload)!;

        Assert.Null(absences.Nodes);
        Assert.Null(absences.Regions);
        Assert.Null(absences.Edges);
        Assert.Null(absences.Names);
        Assert.Equal("{}", absences.ToJson());

        // A row whose slot holds nothing keeps the empty place it was written in.
        Assert.Equal("""{"nodes":[null],"regions":[],"edges":[]}""", new RuntimePayload([null!], [], []).ToJson());
        Assert.Single(JsonSerializer.Deserialize("""{"nodes":[null],"regions":[],"edges":[]}""", RuntimePayloadJson.Default.RuntimePayload)!.Nodes);
    }

    /// <summary>Every record of the file at both of its ends at once, and by the member written for it rather than by
    /// the payload it belongs to: named in every part, it is written with every part under the camelCase name of that
    /// part and in the order the record declares them; named in none, it is an empty object, because every part of an
    /// update may be nothing and none of them may be written as <c>null</c>; and every part said as the empty word or
    /// the empty row is written, because a word said as nothing is still a word the page shows.</summary>
    [Fact]
    public void EveryRecordOfTheFileIsWrittenWithEveryPartAndWithNone()
    {
        foreach (var (type, value) in TheEleven)
        {
            List<string> parts = [.. type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => JsonNamingPolicy.CamelCase.ConvertName(property.Name))];
            var info = RuntimePayloadJson.Default.GetTypeInfo(type)!;

            Assert.Equal(parts, KeysOf(JsonSerializer.Serialize(value, info)));
            Assert.Equal(parts, KeysOf(JsonSerializer.Serialize(ValueOf(type), info)));

            // Every part said as the empty word, the zero height or the row of nothing is still a part the page is told
            // about: none of them is the absence below.
            Assert.Equal(parts, KeysOf(JsonSerializer.Serialize(ValueOf(type, empty: true), info)));
            Assert.Equal("{}", JsonSerializer.Serialize(NothingOf(type), info));
        }
    }

    /// <summary>What the member written for one part makes, that same member reads back: each of the eleven records goes
    /// to the bytes and returns as the record it went as, and the bytes of what came back are the bytes that were
    /// written. A row, a word and a height written on their own are the very bytes the update writes in their place -
    /// the writer keeps one rule for a part however it is reached.</summary>
    [Fact]
    public void WhatTheMemberOfAPartWritesTheSameMemberReadsBack()
    {
        foreach (var (type, _) in TheEleven)
        {
            var info = RuntimePayloadJson.Default.GetTypeInfo(type)!;
            var value = ValueOf(type);
            var bytes = JsonSerializer.Serialize(value, info);
            var again = JsonSerializer.Deserialize(bytes, info)!;

            Assert.Equal(type, again.GetType());
            Assert.Equal(bytes, JsonSerializer.Serialize(again, info));

            if (!HoldRows.Contains(type))
            {
                Assert.Equal(value, again);
            }
        }

        // The rows, the word and the height, written by their own member, are the bytes inside the update.
        using var whole = JsonDocument.Parse(Payload.ToJson());
        var node = whole.RootElement.GetProperty("nodes")[0];
        var line = node.GetProperty("lines")[0];

        Assert.Equal("""["checking","healthy"]""", node.GetProperty("history").GetRawText());
        Assert.Equal(node.GetProperty("history").GetRawText(), JsonSerializer.Serialize(Tile.History!, RuntimePayloadJson.Default.IReadOnlyListString));
        Assert.Equal(line.GetProperty("parts").GetRawText(), JsonSerializer.Serialize(Line.Parts!, RuntimePayloadJson.Default.IReadOnlyListRuntimeTextPart));
        Assert.Equal(line.GetProperty("marks").GetRawText(), JsonSerializer.Serialize(Line.Marks!, RuntimePayloadJson.Default.IReadOnlyListRuntimeCheckMark));
        Assert.Equal(line.GetProperty("trend").GetProperty("points").GetRawText(), JsonSerializer.Serialize(Trend.Points, RuntimePayloadJson.Default.IReadOnlyListDouble));
        Assert.Equal(node.GetProperty("lines").GetRawText(), JsonSerializer.Serialize(Tile.Lines, RuntimePayloadJson.Default.IReadOnlyListRuntimeTileLine));
        Assert.Equal(whole.RootElement.GetProperty("nodes").GetRawText(), JsonSerializer.Serialize(Payload.Nodes, RuntimePayloadJson.Default.IReadOnlyListRuntimeTile));
        Assert.Equal(whole.RootElement.GetProperty("regions").GetRawText(), JsonSerializer.Serialize(Payload.Regions, RuntimePayloadJson.Default.IReadOnlyListRuntimeRegionMark));
        Assert.Equal(whole.RootElement.GetProperty("edges").GetRawText(), JsonSerializer.Serialize(Payload.Edges, RuntimePayloadJson.Default.IReadOnlyListRuntimeEdgeMark));
        Assert.Equal(whole.RootElement.GetProperty("names").GetRawText(), JsonSerializer.Serialize(Payload.Names!, RuntimePayloadJson.Default.IReadOnlyListRuntimeName));
        Assert.Equal(line.GetProperty("parts")[0].GetProperty("text").GetRawText(), JsonSerializer.Serialize(Piece.Text, RuntimePayloadJson.Default.String));
        Assert.Equal(line.GetProperty("trend").GetProperty("points")[1].GetRawText(), JsonSerializer.Serialize(0.5, RuntimePayloadJson.Default.Double));
        Assert.Equal("\"a\\u0022b\"", JsonSerializer.Serialize("a\"b", RuntimePayloadJson.Default.String));
        Assert.Equal(Word, JsonSerializer.Deserialize(JsonSerializer.Serialize(Word, RuntimePayloadJson.Default.String), RuntimePayloadJson.Default.String));
        Assert.Equal(0.5, JsonSerializer.Deserialize("0.5", RuntimePayloadJson.Default.Double));
    }

    // ------------------------------------------------------------------ the two records nobody has fallen apart

    /// <summary>The two records of the file no other test has let fall apart: a piece of a line falls into its words and
    /// the place they lead, a mark of a detailed health check into its shape and its tooltip, each in the order the
    /// constructor names them, and what fell out goes back in the same way - the nothing of a part included, which both
    /// records hold as calmly as the writer leaves it out.</summary>
    [Fact]
    public void APieceOfALineAndAMarkOfACheckFallApartInTheirOwnOrder()
    {
        var (text, link) = Piece;

        Assert.Equal("2.4.43 ", text);
        Assert.Same(ToTask, link);
        Assert.Equal(Piece, new RuntimeTextPart(text, link));
        Assert.Equal(Piece.GetHashCode(), new RuntimeTextPart(text, link).GetHashCode());

        Piece.Deconstruct(out var spoken, out var leads);

        Assert.Equal(Piece.Text, spoken);
        Assert.Same(Piece.Link, leads);
        Assert.Null(new RuntimeTextPart("x").Link);

        var (noText, noLink) = new RuntimeTextPart(null!, null!);

        Assert.Null(noText);
        Assert.Null(noLink);
        Assert.Equal("{}", JsonSerializer.Serialize(new RuntimeTextPart(null!, null!), RuntimePayloadJson.Default.RuntimeTextPart));
        Assert.Equal("""{"text":""}""", JsonSerializer.Serialize(new RuntimeTextPart(string.Empty), RuntimePayloadJson.Default.RuntimeTextPart));

        var (state, title) = Mark;

        Assert.Equal("healthy", state);
        Assert.Equal("API: Healthy. Took under 0.1 ms.", title);
        Assert.Equal(Mark, new RuntimeCheckMark("healthy", "API: Healthy. Took under 0.1 ms."));
        Assert.Single(new HashSet<RuntimeCheckMark> { Mark, new("healthy", "API: Healthy. Took under 0.1 ms.") });

        Mark.Deconstruct(out var shape, out var words);

        Assert.Equal(Mark.State, shape);
        Assert.Equal(Mark.Title, words);

        var (noState, noWords) = new RuntimeCheckMark(null!, null!);

        Assert.Null(noState);
        Assert.Null(noWords);
        Assert.Equal("""{"state":"","title":""}""", JsonSerializer.Serialize(new RuntimeCheckMark(string.Empty, string.Empty), RuntimePayloadJson.Default.RuntimeCheckMark));
    }

    // ---------------------------------------------------------------------------------------- the machinery

    /// <summary>The public properties the writer declares itself, in the order of their names.</summary>
    private static List<PropertyInfo> DeclaredProperties() =>
        [.. typeof(RuntimePayloadJson)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .OrderBy(property => property.Name, StringComparer.Ordinal)];

    /// <summary>The type info behind each of them: every type an update may be written as.</summary>
    private static List<JsonTypeInfo> DeclaredInfos() =>
        [.. DeclaredProperties()
            .Where(property => typeof(JsonTypeInfo).IsAssignableFrom(property.PropertyType))
            .Select(property => (JsonTypeInfo)property.GetValue(RuntimePayloadJson.Default)!)];

    /// <summary>The name a member of the writer carries for one type: a record by its own name, a row by the name of
    /// what stands in it, a word and a height by theirs - the rule the generated context follows, pinned as a rule.</summary>
    private static string NameOf(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)
            ? $"IReadOnlyList{type.GetGenericArguments()[0].Name}"
            : type.Name;

    /// <summary>The names the writer gave the parts of what it wrote, in the order it wrote them.</summary>
    private static List<string> KeysOf(string bytes)
    {
        using var json = JsonDocument.Parse(bytes);

        return [.. json.RootElement.EnumerateObject().Select(property => property.Name)];
    }

    /// <summary>A value of one part of an update, built by walking the constructor that names it: a word as this test
    /// brings it, a height as the health side scales one, a row of one entry, a record of its own parts - every part
    /// named, so that a test may ask the writer for all of a record's names at once. With <paramref name="empty" /> the
    /// same walk says each part as the empty word, the zero height and the row of nothing, which is a part the page is
    /// still told about and not the absence below.</summary>
    private static object ValueOf(Type part, bool empty = false)
    {
        if (part == typeof(string))
        {
            return empty ? string.Empty : Word;
        }

        if (part == typeof(double))
        {
            return empty ? 0d : 0.5;
        }

        if (part.IsGenericType && part.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
        {
            var held = part.GetGenericArguments()[0];
            var row = Activator.CreateInstance(typeof(List<>).MakeGenericType(held))!;

            if (!empty)
            {
                row.GetType().GetMethod("Add")!.Invoke(row, [ValueOf(held)]);
            }

            return row;
        }

        var constructor = Assert.Single(part.GetConstructors());

        return constructor.Invoke([.. constructor.GetParameters().Select(parameter => ValueOf(parameter.ParameterType, empty))])!;
    }

    /// <summary>The record of nothing at all: every part the constructor names, named as nothing - the record no builder
    /// makes, that no record refuses and no writer shows.</summary>
    private static object NothingOf(Type record)
    {
        var constructor = Assert.Single(record.GetConstructors());

        return constructor.Invoke([.. constructor.GetParameters().Select(_ => (object?)null)])!;
    }
}
