namespace Dashboard.Tests;

/// <summary>
/// The boundary, the null/empty and the fixed-value tests of <see cref="RuntimeManifestParser" /> that its other test
/// files leave open: <see cref="RuntimeManifestParserEdgeCaseTests" />, <see cref="RuntimeManifestParserBoundaryEdgeCaseTests" />,
/// <see cref="RuntimeManifestParserJsonEdgeCaseTests" />, <see cref="RuntimeManifestParserTests" /> and
/// <see cref="DatabaseViewTests" />. What stands here: the two file names of an environment's database view, trimmed
/// and weighed by the same rule as the entry's own files (a backslash, a colon, a leading dot and the empty word); that
/// a database view the reader leaves out never adds a word to the messages of an index it refuses; that a list of ids
/// of nothing but empty words is no list, for a box and for an unplaced entry alike; and that the manifest's own
/// environment is kept trimmed.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every file name, id and word is one the test
/// brings.
/// </para>
/// </summary>
public class RuntimeManifestParserDatabaseAndIdsEdgeCaseTests
{
    // ----- The database view of an index entry -----

    /// <summary>The two names of a database view are kept as the file trimmed them, and a name with a dot inside it or
    /// no extension at all is still a plain name of <c>runtime/</c>.</summary>
    [Theory]
    [InlineData("  tdd.database.svg  ", "\ttdd.database.json\n", "tdd.database.svg", "tdd.database.json")]
    [InlineData("tdd..svg", "tdd", "tdd..svg", "tdd")]
    [InlineData("a", "b", "a", "b")]
    public void ADatabaseViewKeepsItsTwoNamesTrimmed(string svg, string manifest, string keptSvg, string keptManifest)
    {
        var entry = Entry($$"""{ "name": "tdd", "database": { "svg": {{Json(svg)}}, "manifest": {{Json(manifest)}} } }""");

        Assert.Equal(new RuntimeDatabaseFiles(keptSvg, keptManifest), entry.Database);
    }

    /// <summary>Either name of a database view that is no plain name - a backslash, a colon, a leading dot, a leading
    /// slash, or a word of no letters - leaves the whole view out, and the entry is read with its own two files as if
    /// it named no database at all.</summary>
    [Theory]
    [InlineData(@"db\tdd.svg", "tdd.database.json")]
    [InlineData("tdd.database.svg", @"db\tdd.json")]
    [InlineData("c:tdd.svg", "tdd.database.json")]
    [InlineData("tdd.database.svg", "tdd:db.json")]
    [InlineData(".tdd.svg", "tdd.database.json")]
    [InlineData("tdd.database.svg", ".tdd.json")]
    [InlineData("/tdd.svg", "tdd.database.json")]
    [InlineData("", "tdd.database.json")]
    [InlineData("tdd.database.svg", "   ")]
    public void ADatabaseNameThatIsNoPlainNameLeavesTheViewOut(string svg, string manifest)
    {
        var entry = Entry($$"""{ "name": "tdd", "database": { "svg": {{Json(svg)}}, "manifest": {{Json(manifest)}} } }""");

        Assert.Equal(new RuntimeIndexEntry("tdd", "tdd.json", "tdd.svg"), entry);
        Assert.Null(entry.Database);
    }

    /// <summary>A database view the reader leaves out says nothing even of an index it refuses: the messages are those
    /// of the entry's own files, exactly, and the database of the entry beside it is no reason to hand the index over.</summary>
    [Fact]
    public void ADatabaseViewLeftOutAddsNoMessageToAnIndexThatIsRefused()
    {
        var result = RuntimeManifestParser.ParseIndex("""
            { "environments": [
              { "name": "tdd", "svg": "../tdd.svg", "database": { "svg": "../db.svg", "manifest": "c:db.json" } },
              { "name": "uat", "database": { "svg": "uat.database.svg", "manifest": "uat.database.json" } } ] }
            """);

        Assert.False(result.IsValid);
        Assert.Null(result.Value);
        Assert.Equal(["environments[0].svg: not a file name in runtime/."], result.Errors);
    }

    // ----- Lists of ids of nothing but empty words -----

    /// <summary>A box's ids of nothing but the empty word, spaces and values that are no string are no ids at all:
    /// the one <c>null</c>, not an empty list, and the manifest is read.</summary>
    [Theory]
    [InlineData("""[ "" ]""")]
    [InlineData("""[ "   ", "\t" ]""")]
    [InlineData("""[ "", 7, null, true, {}, [] ]""")]
    public void TheIdsOfABoxOfOnlyEmptyWordsAreNoIds(string ids)
    {
        var node = Assert.Single(Manifest($$"""{ "environment": "tdd", "nodes": [ { "alias": "res_1", "kind": "resource", "ids": {{ids}} } ] }""").Nodes);

        Assert.Null(node.Ids);
    }

    /// <summary>The same of an unplaced entry: its ids of nothing but empty words are null, and the entry is kept by
    /// its name with everything else it has.</summary>
    [Fact]
    public void TheIdsOfAnUnplacedEntryOfOnlyEmptyWordsAreNoIds()
    {
        var entry = Assert.Single(Manifest("""
            { "environment": "tdd", "unplaced": [ { "name": "kv-demo", "type": "Key vault", "ids": [ "", "  ", 4 ] } ] }
            """).Unplaced!);

        Assert.Equal("kv-demo", entry.Name);
        Assert.Equal("Key vault", entry.Type);
        Assert.Null(entry.Ids);
    }

    // ----- The manifest's own environment -----

    /// <summary>The environment a manifest names is kept trimmed, and in the case the file wrote it.</summary>
    [Theory]
    [InlineData("  tdd  ", "tdd")]
    [InlineData("\tUAT\n", "UAT")]
    [InlineData("prod", "prod")]
    public void TheEnvironmentOfAManifestIsKeptTrimmedAndAsCased(string written, string kept) =>
        Assert.Equal(kept, Manifest($$"""{ "environment": {{Json(written)}} }""").Environment);

    // ----- The readers of this file -----

    private static string Json(string text) => System.Text.Json.JsonSerializer.Serialize(text);

    private static RuntimeIndexEntry Entry(string entry)
    {
        var found = RuntimeManifestParser.ParseIndex($$"""{ "environments": [ {{entry}} ] }""");

        Assert.True(found.IsValid, string.Join(' ', found.Errors));
        Assert.Empty(found.Errors);
        return Assert.Single(found.Value!.Environments);
    }

    private static RuntimeManifest Manifest(string json)
    {
        var found = RuntimeManifestParser.ParseManifest(json);

        Assert.True(found.IsValid, string.Join(' ', found.Errors));
        Assert.Empty(found.Errors);
        return found.Value!;
    }
}
