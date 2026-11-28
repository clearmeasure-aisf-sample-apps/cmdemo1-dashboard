namespace Dashboard.Tests;

/// <summary>
/// The file words the parser reads by its named consts (#2246): a manifest without <c>environment</c> is refused
/// with the refusal that names the word, an index entry whose <c>database</c> object gives plain <c>svg</c> and
/// <c>manifest</c> names yields those names, a <c>database</c> value that is not an object yields no database files
/// and no error, a <c>database</c> object whose <c>svg</c> or <c>manifest</c> is not a plain file name yields no
/// database files, and an index entry without <c>manifest</c> or <c>svg</c> falls back to <c>{name}.json</c> /
/// <c>{name}.svg</c>.
/// </summary>
public class RuntimeManifestParserFileWordTests
{
    /// <summary>A manifest without <c>environment</c> is refused with exactly the refusal that names the word.</summary>
    [Theory]
    [InlineData("""{ "nodes": [] }""")]
    [InlineData("""{ "environment": null, "nodes": [] }""")]
    [InlineData("""{ "environment": "", "nodes": [] }""")]
    [InlineData("""{ "environment": "   ", "nodes": [] }""")]
    public void AManifestWithoutEnvironmentIsRefusedWithTheRefusalThatNamesTheWord(string json)
    {
        var result = RuntimeManifestParser.ParseManifest(json);

        Assert.False(result.IsValid);
        Assert.Equal([FileProblems.MissingOrEmpty("environment")], result.Errors);
    }

    /// <summary>A manifest whose <c>environment</c> is a plain word is read with that word, trimmed.</summary>
    [Theory]
    [InlineData("tdd", "tdd")]
    [InlineData("  uat  ", "uat")]
    [InlineData("\tprod\n", "prod")]
    public void AManifestWithEnvironmentIsReadWithThatWord(string written, string kept)
    {
        var result = RuntimeManifestParser.ParseManifest($$"""{ "environment": {{Json(written)}} }""");

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Equal(kept, result.Value!.Environment);
    }

    /// <summary>An index entry whose <c>database</c> object gives plain <c>svg</c> and <c>manifest</c> names yields
    /// <see cref="RuntimeDatabaseFiles" /> with those names, trimmed.</summary>
    [Theory]
    [InlineData("tdd.database.svg", "tdd.database.json", "tdd.database.svg", "tdd.database.json")]
    [InlineData("  db.svg  ", "db.json", "db.svg", "db.json")]
    [InlineData("a", "b", "a", "b")]
    public void AnIndexEntryWithPlainDatabaseNamesYieldsThoseFiles(string svg, string manifest, string keptSvg, string keptManifest)
    {
        var result = RuntimeManifestParser.ParseIndex($$"""{ "environments": [ { "name": "tdd", "database": { "svg": {{Json(svg)}}, "manifest": {{Json(manifest)}} } } ] }""");

        Assert.True(result.IsValid);
        var entry = Assert.Single(result.Value!.Environments);
        Assert.Equal(new RuntimeDatabaseFiles(keptSvg, keptManifest), entry.Database);
    }

    /// <summary>A <c>database</c> value that is not an object — a string, an array, a number, a bool or null — yields
    /// no database files and no error.</summary>
    [Theory]
    [InlineData("\"a string\"")]
    [InlineData("[\"svg\", \"manifest\"]")]
    [InlineData("42")]
    [InlineData("true")]
    [InlineData("null")]
    public void ADatabaseValueThatIsNotAnObjectYieldsNoDatabaseFilesAndNoError(string databaseValue)
    {
        var result = RuntimeManifestParser.ParseIndex($$"""{ "environments": [ { "name": "tdd", "database": {{databaseValue}} } ] }""");

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        var entry = Assert.Single(result.Value!.Environments);
        Assert.Null(entry.Database);
    }

    /// <summary>A <c>database</c> object whose <c>svg</c> or <c>manifest</c> is not a plain file name — it contains
    /// <c>/</c>, <c>\</c> or <c>:</c>, or starts with <c>.</c> — yields no database files.</summary>
    [Theory]
    [InlineData("../tdd.svg", "tdd.database.json")]
    [InlineData(@"db\tdd.svg", "tdd.database.json")]
    [InlineData("c:tdd.svg", "tdd.database.json")]
    [InlineData(".tdd.svg", "tdd.database.json")]
    [InlineData("tdd.database.svg", "../tdd.json")]
    [InlineData("tdd.database.svg", @"db\tdd.json")]
    [InlineData("tdd.database.svg", "c:tdd.json")]
    [InlineData("tdd.database.svg", ".tdd.json")]
    [InlineData("", "tdd.database.json")]
    [InlineData("tdd.database.svg", "")]
    [InlineData("   ", "tdd.database.json")]
    [InlineData("tdd.database.svg", "   ")]
    public void ADatabaseNameThatIsNoPlainFileNameYieldsNoDatabaseFiles(string svg, string manifest)
    {
        var result = RuntimeManifestParser.ParseIndex($$"""{ "environments": [ { "name": "tdd", "database": { "svg": {{Json(svg)}}, "manifest": {{Json(manifest)}} } } ] }""");

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        var entry = Assert.Single(result.Value!.Environments);
        Assert.Null(entry.Database);
    }

    /// <summary>An index entry without <c>manifest</c> or <c>svg</c> falls back to <c>{name}.json</c> /
    /// <c>{name}.svg</c>, as today.</summary>
    [Theory]
    [InlineData("tdd", "tdd.json", "tdd.svg")]
    [InlineData("uat", "uat.json", "uat.svg")]
    [InlineData("my-env", "my-env.json", "my-env.svg")]
    public void AnIndexEntryWithoutManifestOrSvgFallsBackToTheConventionalNames(string name, string fallbackJson, string fallbackSvg)
    {
        var result = RuntimeManifestParser.ParseIndex($$"""{ "environments": [ { "name": {{Json(name)}} } ] }""");

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        var entry = Assert.Single(result.Value!.Environments);
        Assert.Equal(name, entry.Name);
        Assert.Equal(fallbackJson, entry.Manifest);
        Assert.Equal(fallbackSvg, entry.Svg);
        Assert.Null(entry.Database);
    }

    /// <summary>An index entry with only <c>manifest</c> (no <c>svg</c>) keeps the given manifest and falls back for
    /// the SVG to <c>{name}.svg</c>, and the reverse holds.</summary>
    [Theory]
    [InlineData("tdd", "custom.json", "tdd.svg")]
    [InlineData("tdd", "custom.svg", "tdd.json")]
    public void AnIndexEntryWithOnlyOneFileKeepsTheGivenOneAndFallsBackForTheOther(string name, string given, string otherFallback)
    {
        var hasManifest = given.EndsWith(".json", StringComparison.Ordinal);
        var json = hasManifest
            ? $$"""{ "environments": [ { "name": {{Json(name)}}, "manifest": {{Json(given)}} } ] }"""
            : $$"""{ "environments": [ { "name": {{Json(name)}}, "svg": {{Json(given)}} } ] }""";

        var result = RuntimeManifestParser.ParseIndex(json);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        var entry = Assert.Single(result.Value!.Environments);
        if (hasManifest)
        {
            Assert.Equal(given, entry.Manifest);
            Assert.Equal(otherFallback, entry.Svg);
        }
        else
        {
            Assert.Equal(otherFallback, entry.Manifest);
            Assert.Equal(given, entry.Svg);
        }
    }

    private static string Json(string text) => System.Text.Json.JsonSerializer.Serialize(text);
}
