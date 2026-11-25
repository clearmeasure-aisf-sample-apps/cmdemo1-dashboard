using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// The one opener of the files of <c>runtime/</c>, <c>RuntimeManifestParser.TryOpen</c>, and the three other
/// readers that now go through it rather than write the open again: the database view's
/// <see cref="DatabaseParser.ParseManifest" /> and <see cref="DatabaseParser.ParseLive" />, and the application view's
/// <see cref="ApplicationParser.Parse" />. What stands here is the shared step itself, and nothing of what the readers
/// make of a file once it is open - which <see cref="DatabaseViewTests" /> and <see cref="ApplicationViewTests" />
/// already pin.
/// <para>
/// Three things are pinned of the open, of every reader of the folder alike: the three refusals said in one set of
/// words (an empty file, a file that is no JSON - at the very line and position the reader of the runtime manifest
/// names for the same text - and a root that is no object), the two licenses those words are given (a trailing comma
/// and a comment), and that a reader that only answers <see langword="null" /> still answers it for each of the three.
/// The guard at the bottom reads the folder's source, since the whole point of the change is that the sentences, the
/// <see cref="System.Text.Json.JsonDocumentOptions" /> and the call to
/// <see cref="System.Text.Json.JsonDocument.Parse(string, System.Text.Json.JsonDocumentOptions)" /> are each written
/// once there: a fourth parser that hand-rolled the open again, or a second set of options, would read files that
/// differ from the runtime diagram's and nothing would say so.
/// </para>
/// </summary>
public class RuntimeOpenerSharedTests
{
    // ----- The empty file, the unopenable file and the file of another root -----

    /// <summary>The word of no letters, the word of nothing but room and the <see langword="null" /> a caller that read
    /// nothing hands over are the empty file to the database view's manifest reader, in the runtime parser's own words,
    /// and it says that before it looks for a member of a file it never opened.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TheDatabaseManifestReaderSaysTheOpenersOwnWordsForAnEmptyFile(string? json)
    {
        var result = DatabaseParser.ParseManifest(json);

        Assert.False(result.IsValid);
        Assert.Equal(["The file is empty."], result.Errors);
    }

    /// <summary>A cut-off object is no JSON, and the place the database view names of it is the place the runtime
    /// manifest reader names of the very same text - one sentence, brought by both readers from the one open, so the
    /// database view and the diagram cannot start telling a file where its fault lies.</summary>
    [Theory]
    [InlineData("""{ "environment": """)]
    [InlineData("""{ "environment": "tdd", "database": "db", "tables": [ { """)]
    [InlineData("""<!DOCTYPE html><html></html>""")]
    public void TheDatabaseManifestReaderNamesTheCutOfTheSameFileTheRuntimeReaderNames(string json)
    {
        var database = DatabaseParser.ParseManifest(json);
        var runtime = RuntimeManifestParser.ParseManifest(json);

        Assert.False(database.IsValid);
        Assert.Equal(runtime.Errors, database.Errors);
        var problem = Assert.Single(database.Errors);
        Assert.StartsWith("The file is not valid JSON (line 1, position ", problem, StringComparison.Ordinal);
        Assert.EndsWith(").", problem, StringComparison.Ordinal);
    }

    /// <summary>The exact sentence of the cut the issue names, pinned as a number rather than only as the sentence
    /// another reader happens to say: line and position are counted from one, and the database view counts them the
    /// same way the runtime manifest reader does.</summary>
    [Fact]
    public void TheCutIsNamedByLineAndPositionCountedFromOne()
    {
        Assert.Equal(
            ["The file is not valid JSON (line 1, position 18)."],
            DatabaseParser.ParseManifest("""{ "environment": """).Errors);
    }

    /// <summary>A file of a root that is no object is refused by the opener rather than by the reader, so the database
    /// view says the runtime parser's sentence for a list and for a number alike - the same two files
    /// <see cref="RuntimeManifestParserJsonEdgeCaseTests" /> pins of the runtime reader.</summary>
    [Theory]
    [InlineData("[]")]
    [InlineData("42")]
    [InlineData("\"tdd\"")]
    [InlineData("null")]
    public void TheDatabaseManifestReaderRefusesARootThatIsNoObject(string json)
    {
        var result = DatabaseParser.ParseManifest(json);

        Assert.False(result.IsValid);
        Assert.Equal(["The file must contain a JSON object."], result.Errors);
        Assert.Equal(result.Errors, RuntimeManifestParser.ParseManifest(json).Errors);
    }

    // ----- The two licenses of the open, of each reader -----

    /// <summary>A manifest with a trailing comma and a comment between its members is still the manifest it was: the
    /// database view reads it through the same opener that gives the runtime diagram its trailing commas and its
    /// comments, so a deployment that annotated one file annotates both.</summary>
    [Fact]
    public void TheDatabaseManifestReaderStillReadsAFileWithATrailingCommaAndAComment()
    {
        var result = DatabaseParser.ParseManifest("""
            {
              // written by deploy-staticwebapp.ps1
              "environment": "tdd",
              "database": "sqldb-cmdemo1-tdd",
              "tables": [
                { "alias": "t_dbo_Employee", "qualifiedName": "db.t_dbo_Employee", "schema": "dbo", "name": "Employee", },
              ],
            }
            """);

        Assert.True(result.IsValid, string.Join(' ', result.Errors));
        var manifest = result.Value!;
        Assert.Equal("sqldb-cmdemo1-tdd", manifest.Database);
        Assert.Equal(["Employee"], manifest.Tables.Select(table => table.Name));
    }

    /// <summary>The same of the live file, which answers <see langword="null" /> rather than a message: a file of
    /// trailing commas and comments is a file it reads, tables and all.</summary>
    [Fact]
    public void TheLiveFileReaderStillReadsAFileWithATrailingCommaAndAComment()
    {
        var live = DatabaseParser.ParseLive("""
            {
              /* read again by the page */
              "generated": "2026-10-12T11:00:00Z",
              "tables": [ { "schema": "dbo", "name": "Employee", "rows": 130, } ],
            }
            """);

        var table = Assert.Single(live!.Tables);
        Assert.Equal("Employee", table.Name);
        Assert.Equal(130, table.Rows);
    }

    /// <summary>And the same of the application file, which answers <see langword="null" /> too: the comma after the
    /// last assembly and the comment before the first member are read past, and the assemblies stand.</summary>
    [Fact]
    public void TheApplicationFileReaderStillReadsAFileWithATrailingCommaAndAComment()
    {
        var architecture = ApplicationParser.Parse("""
            {
              // the repository's own file
              "entry": "Ui.Server",
              "assemblies": [ { "name": "Ui.Server", "references": [ "Core", ], }, ],
            }
            """);

        Assert.Equal("Ui.Server", architecture!.Entry);
        Assert.Equal(["Ui.Server"], architecture.Assemblies.Select(assembly => assembly.Name));
        Assert.Equal(["Core"], Assert.Single(architecture.Assemblies).References);
    }

    /// <summary>That a reader answers <see langword="null" /> instead of a message is the only difference left between
    /// the openers of the two files a caller cannot refuse: the empty file, the file that is no JSON and the file whose
    /// root is no object each end the read of the live file and of the application file, at the same three places the
    /// manifest reader says its message about.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("""{ "tables": """)]
    [InlineData("404: Not Found")]
    [InlineData("[]")]
    [InlineData("42")]
    public void TheTwoReadersThatAnswerNothingRefuseWhatTheManifestReaderRefuses(string? json)
    {
        Assert.Null(DatabaseParser.ParseLive(json));
        Assert.Null(ApplicationParser.Parse(json));
    }

    /// <summary>The refusal of a live file of no <c>tables</c> array is still the live file's own rule and not the
    /// opener's: an object the opener let through is no live file, while the manifest reader of the very same object
    /// gets as far as naming its own missing members - so the two rules are two, and only the one about opening the
    /// file is shared.</summary>
    [Theory]
    [InlineData("{ }", "environment: missing or empty.", "database: missing or empty.")]
    [InlineData("{ \"tables\": { } }", "environment: missing or empty.", "database: missing or empty.", "tables: not an array.")]
    [InlineData("{ \"tables\": 3 }", "environment: missing or empty.", "database: missing or empty.", "tables: not an array.")]
    public void ALiveFileWithoutItsOwnTablesArrayIsNoFileThoughTheOpenerOpenedIt(string json, params string[] problems)
    {
        Assert.Null(DatabaseParser.ParseLive(json));

        var result = DatabaseParser.ParseManifest(json);

        Assert.False(result.IsValid);
        Assert.Equal(problems, result.Errors);
    }

    // ----- The guard: the folder writes the open once -----

    /// <summary>The three refusal sentences, the <see cref="System.Text.Json.JsonDocumentOptions" /> the folder parses
    /// with and the call that opens the text are each written once in <c>src/Dashboard/Runtime</c>, in the one opener
    /// they belong to - so a reword, a new option (a <c>MaxDepth</c>, say) or a fourth hand-rolled open cannot make the
    /// database view and the runtime diagram read a file differently. The guard prints every line it refuses, the way
    /// the markup guards print the tag they refuse.</summary>
    [Fact]
    public void TheRuntimeFolderOpensAFileInOnePlaceOnly()
    {
        var root = MarkupGuards.SourcePath("Runtime");
        var sentences = new[] { "The file is empty.", "The file is not valid JSON", "The file must contain a JSON object." };
        var offenders = new List<string>();

        foreach (var path in Directory.EnumerateFiles(root, "*.cs", SearchOption.TopDirectoryOnly).OrderBy(p => p, StringComparer.Ordinal))
        {
            var text = File.ReadAllText(path);
            var opener = Path.GetFileName(path).Equals("RuntimeManifestParser.cs", StringComparison.Ordinal);

            foreach (var sentence in sentences)
            {
                var found = Regex.Count(text, Regex.Escape(sentence));
                if (found != (opener ? 1 : 0))
                {
                    offenders.Add($"{Path.GetFileName(path)}: {found} of \"{sentence}\" (the opener owns it alone)");
                }
            }

            var parses = Regex.Count(text, @"\bJsonDocument\s*\.\s*Parse\s*\(");
            if (parses != (opener ? 1 : 0))
            {
                offenders.Add($"{Path.GetFileName(path)}: {parses} call(s) to JsonDocument.Parse (the opener owns the one)");
            }

            var options = Regex.Count(text, @"\bJsonDocumentOptions\b");
            if (options != (opener ? 1 : 0))
            {
                offenders.Add($"{Path.GetFileName(path)}: {options} mention(s) of JsonDocumentOptions (the folder has one set)");
            }
        }

        Assert.Empty(offenders);
    }

    /// <summary>A guard that found nothing has seen nothing: the opener it holds the folder to is the one that is
    /// really there, with its three sentences, its options and its call to parse.</summary>
    [Fact]
    public void TheGuardReadsTheOpenerItHoldsTheFolderTo()
    {
        var opener = File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("Runtime", "RuntimeManifestParser.cs")));

        Assert.Contains("internal static bool TryOpen(", opener, StringComparison.Ordinal);
        Assert.Contains("The file is empty.", opener, StringComparison.Ordinal);
        Assert.Contains("The file is not valid JSON", opener, StringComparison.Ordinal);
        Assert.Contains("The file must contain a JSON object.", opener, StringComparison.Ordinal);
        Assert.Contains("AllowTrailingCommas = true", opener, StringComparison.Ordinal);
        Assert.Contains("CommentHandling = JsonCommentHandling.Skip", opener, StringComparison.Ordinal);
    }
}
