using Dashboard.Health;
using Dashboard.Runtime;

namespace Dashboard.Tests;

/// <summary>
/// The values the reader of a runtime file gets before the diagram: <see cref="RuntimeIndex"/>, the <see
/// cref="RuntimeFrame"/>s of a <see cref="RuntimeManifest"/>, and <see cref="RuntimeParseResult{T}"/>, the one thing
/// a caller of <see cref="RuntimeManifestParser"/> reads. The page branches on the parse result's value and IsValid,
/// so the values they carry, and the words the failures print, are what the reader sees.
/// </summary>
public class RuntimeIndexFrameAndParseResultTests
{
    private const string AnIndex = """
        { "environments": [
            { "name": "tdd", "database": { "svg": "tdd-db.svg", "manifest": "tdd-db.json" } },
            { "name": "uat", "manifest": "uat-prod.json", "svg": "uat-prod.svg" } ],
          "plantuml": "1.2026.8" }
        """;

    private const string AManifestWithFrames = """
        { "environment": "tdd",
          "frames": [
            { "alias": "sub", "kind": "subscription", "name": "demo",
              "links": { "portal": "https://portal.example.net/#demo", "plain": "http://portal.example.net/plain" } },
            { "alias": "afd", "kind": "frontDoorProfile", "name": "afd-demo" } ] }
        """;

    [Fact]
    public void TheIndexHoldsTheEnvironmentsInTheOrderOfSystemJsonAndTheVersionThatRenderedThem()
    {
        var index = new RuntimeIndex(
            [
                new RuntimeIndexEntry("tdd", "tdd.json", "tdd.svg", new RuntimeDatabaseFiles("tdd-db.svg", "tdd-db.json")),
                new RuntimeIndexEntry("uat", "uat-prod.json", "uat-prod.svg"),
            ],
            "1.2026.8");

        Assert.Equal(["tdd", "uat"], index.Environments.Select(entry => entry.Name));
        Assert.Equal(new RuntimeDatabaseFiles("tdd-db.svg", "tdd-db.json"), index.Environments[0].Database);
        Assert.Null(index.Environments[1].Database);
        Assert.Equal("1.2026.8", index.PlantUml);
    }

    [Fact]
    public void TheIndexWithoutAVersionHasNoVersion()
    {
        Assert.Null(new RuntimeIndex([new RuntimeIndexEntry("tdd", "tdd.json", "tdd.svg")]).PlantUml);
    }

    [Fact]
    public void TheIndexIsItsValues()
    {
        IReadOnlyList<RuntimeIndexEntry> environments = [new RuntimeIndexEntry("tdd", "tdd.json", "tdd.svg")];
        var index = new RuntimeIndex(environments, "1.2026.8");

        Assert.Equal(index, index with { Environments = environments });
        Assert.True(index.Equals(new RuntimeIndex(environments, "1.2026.8")));
        Assert.NotEqual(index, index with { PlantUml = null });
    }

    [Fact]
    public void TheFrameHoldsTheAliasAndTheWordOfItsKindAndTheNameAsDrawn()
    {
        var frame = new RuntimeFrame(
            "sub",
            "subscription",
            "demo",
            new LinkSet([new KeyValuePair<string, Uri>(LinkSet.Portal, new Uri("https://portal.example.net/#demo"))]));

        Assert.Equal("sub", frame.Alias);
        Assert.Equal("subscription", frame.Kind);
        Assert.Equal("demo", frame.Name);
        Assert.Equal("https://portal.example.net/#demo", frame.Links![LinkSet.Portal]!.AbsoluteUri);
        Assert.Null(frame.Links["liveMetrics"]);
    }

    [Fact]
    public void TheFrameWithoutLinksLeadsNowhere()
    {
        var frame = new RuntimeFrame("afd", "frontDoorProfile", "afd-demo");

        Assert.Null(frame.Links);
        Assert.Null(frame.Links?[LinkSet.Portal]);
    }

    [Fact]
    public void TheFrameIsItsValues()
    {
        var links = new LinkSet([new KeyValuePair<string, Uri>(LinkSet.Portal, new Uri("https://portal.example.net/#demo"))]);
        var frame = new RuntimeFrame("sub", "subscription", "demo", links);

        Assert.Equal(frame, frame with { });
        Assert.True(frame.Equals(new RuntimeFrame("sub", "subscription", "demo", links)));
        Assert.NotEqual(frame, frame with { Links = null });
        Assert.NotEqual(frame, frame with { Name = "demo prod" });
    }

    [Fact]
    public void ASuccessfulReadCarriesTheValueAndNoMessage()
    {
        var index = new RuntimeIndex([new RuntimeIndexEntry("tdd", "tdd.json", "tdd.svg")], "1.2026.8");
        var result = new RuntimeParseResult<RuntimeIndex>(index, []);

        Assert.True(result.IsValid);
        Assert.Same(index, result.Value);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void AFailedReadCarriesNoValueAndTheMessagesOfTheProblem()
    {
        var result = new RuntimeParseResult<RuntimeIndex>(null, ["environments[0].name: missing or empty."]);

        Assert.False(result.IsValid);
        Assert.Null(result.Value);
        Assert.Equal(["environments[0].name: missing or empty."], result.Errors);
    }

    [Fact]
    public void IsValidOnlyAsksForTheValue()
    {
        var index = new RuntimeIndex([new RuntimeIndexEntry("tdd", "tdd.json", "tdd.svg")]);

        Assert.True(new RuntimeParseResult<RuntimeIndex>(index, []).IsValid);
        Assert.True(new RuntimeParseResult<RuntimeIndex>(index, ["A message the parser never gave."]).IsValid);
        Assert.False(new RuntimeParseResult<RuntimeIndex>(null, []).IsValid);
    }

    [Fact]
    public void TheParseResultIsItsValueAndItsMessages()
    {
        var index = new RuntimeIndex([new RuntimeIndexEntry("tdd", "tdd.json", "tdd.svg")]);
        var messages = (IReadOnlyList<string>)["environments: missing or not an array."];

        Assert.Equal(new RuntimeParseResult<RuntimeIndex>(index, messages), new RuntimeParseResult<RuntimeIndex>(index, messages));
        Assert.NotEqual(new RuntimeParseResult<RuntimeIndex>(index, messages), new RuntimeParseResult<RuntimeIndex>(null, messages));
        Assert.NotEqual(new RuntimeParseResult<RuntimeIndex>(index, messages), new RuntimeParseResult<RuntimeIndex>(index, ["a different message"]));
    }

    [Fact]
    public void TheParserHandsOverTheIndexItReadOrItsMessages()
    {
        var found = RuntimeManifestParser.ParseIndex(AnIndex);

        Assert.True(found.IsValid, string.Join(' ', found.Errors));
        Assert.Empty(found.Errors);
        Assert.Equal(
            [
                new RuntimeIndexEntry("tdd", "tdd.json", "tdd.svg", new RuntimeDatabaseFiles("tdd-db.svg", "tdd-db.json")),
                new RuntimeIndexEntry("uat", "uat-prod.json", "uat-prod.svg"),
            ],
            found.Value!.Environments);
        Assert.Equal("1.2026.8", found.Value.PlantUml);

        var missing = RuntimeManifestParser.ParseIndex(null);

        Assert.False(missing.IsValid);
        Assert.Null(missing.Value);
        Assert.Equal(["The file is empty."], missing.Errors);
    }

    [Fact]
    public void TheParserHandsOverTheFramesItReadOrItsMessages()
    {
        var found = RuntimeManifestParser.ParseManifest(AManifestWithFrames);

        Assert.True(found.IsValid, string.Join(' ', found.Errors));
        Assert.Empty(found.Errors);
        var manifest = found.Value!;
        Assert.Equal("tdd", manifest.Environment);
        Assert.Equal(
            [
                ("sub", "subscription", "demo", "https://portal.example.net/#demo"),
                ("afd", "frontDoorProfile", "afd-demo", (string?)null),
            ],
            manifest.Frames!.Select(frame => (frame.Alias, frame.Kind, frame.Name, frame.Links?[LinkSet.Portal]?.AbsoluteUri)));
        // Only the https address is a link; the http one is left out, never an error.
        Assert.Equal([LinkSet.Portal], manifest.Frames![0].Links!.Keys);
        Assert.Null(manifest.Frames[1].Links);
        Assert.Null(manifest.Unplaced);

        var missing = RuntimeManifestParser.ParseManifest("{ }");

        Assert.False(missing.IsValid);
        Assert.Null(missing.Value);
        Assert.Equal(["environment: missing or empty."], missing.Errors);
    }

    [Theory]
    [InlineData(null, "The file is empty.")]
    [InlineData("   ", "The file is empty.")]
    [InlineData("[]", "The file must contain a JSON object.")]
    [InlineData("{ }", "environments: missing or not an array.")]
    public void AnIndexThatIsNoObjectOrHasNoEnvironmentsIsAFailureWithOneMessage(string? json, string message)
    {
        var result = RuntimeManifestParser.ParseIndex(json);

        Assert.False(result.IsValid);
        Assert.Null(result.Value);
        Assert.Equal(message, Assert.Single(result.Errors));
    }

    [Theory]
    [InlineData(null, "The file is empty.")]
    [InlineData("[]", "The file must contain a JSON object.")]
    [InlineData("{ }", "environment: missing or empty.")]
    [InlineData("""{ "environment": "tdd", "frames": [ { "kind": "plan" } ] }""", "frames[0].alias: missing or empty.")]
    public void AManifestThatIsNoObjectOrHasNoEnvironmentIsAFailureWithOneMessage(string? json, string message)
    {
        var result = RuntimeManifestParser.ParseManifest(json);

        Assert.False(result.IsValid);
        Assert.Null(result.Value);
        Assert.Equal(message, Assert.Single(result.Errors));
    }
}
