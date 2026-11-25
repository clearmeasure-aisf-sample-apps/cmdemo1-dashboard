namespace Dashboard.Tests;

/// <summary>
/// The words this page labels its tiles with are one vocabulary (#1672): <c>RuntimePayloadBuilder</c> names each of
/// them (<c>NotProbed</c>, <c>NoAddress</c> from #1662, and the rest named for this issue). A guard therefore reads
/// the builder's own source and refuses any line that writes one of these words as a quoted literal without naming
/// it: a typo such as <c>"Not Checked"</c> at a new call site would otherwise split the page's vocabulary silently,
/// with a tile that looks like a word the legend, the tests and the reader have never heard. The words stay whole:
/// each is declared by exactly one <c>private const</c>, and the words of a region mark or of the state at the JSON
/// boundary are other vocabularies this guard does not speak of.
/// </summary>
public class RuntimePayloadLabelVocabularyTests
{
    private static readonly Uri Page = new("http://localhost:5210/");

    /// <summary>The label words of the runtime page's tiles, as the builder names them.</summary>
    private static readonly string[] Labels = ["Not probed", "No address", "Not known", "Not confirmed", "Not checked", "This page", "Checking", "Reachable"];

    /// <summary>Every label word has exactly one home in the builder: one <c>private const</c> per word.</summary>
    [Fact]
    public void EveryLabelWordIsNamedOnce()
    {
        var lines = File.ReadAllLines(BuilderSource);

        foreach (var word in Labels)
        {
            Assert.Equal(1, lines.Count(line => IsDeclaration(line, word)));
        }
    }

    /// <summary>No line of the builder quotes a label word without naming it: only the declaration itself, and a
    /// comment that explains the vocabulary, may hold the word in quotes.</summary>
    [Fact]
    public void NoCallSiteWritesALabelWordAsALiteral()
    {
        var lines = File.ReadAllLines(BuilderSource);

        // A guard that read nothing proved nothing: the builder is a thousand lines of tile and arrow.
        Assert.True(lines.Length > 500, $"only {lines.Length} line(s) read from {BuilderSource}: the guard is checking nothing");

        var offenders =
            from line in lines
            from word in Labels
            where line.Contains($"\"{word}\"", StringComparison.Ordinal)
            where !IsDeclaration(line, word) && !IsComment(line)
            select $"{word}: {line.Trim()}";

        Assert.Empty(offenders);
    }

    /// <summary>The half of the <c>WebApp or FrontDoor</c> arm the other tests never walked: the endpoint the loaded
    /// topology does not name is not checked, in the one word the arm names.</summary>
    [Fact]
    public void AFrontDoorEndpointTheTopologyDoesNotHaveIsNotChecked()
    {
        var manifest = new RuntimeManifest(
            "uat",
            [
                new RuntimeNode("browser", RuntimeNodeKind.Person, "Browser"),
                new RuntimeNode("fd_ui", RuntimeNodeKind.FrontDoor, "fd-cmdemo2-uat", new Uri("https://fd-cmdemo2-uat.azurefd.net"), "ui"),
            ],
            [],
            [new RuntimeEdge("browser-to-fd_ui", "browser", "fd_ui", RuntimeEdgeKind.Public)]);

        var tile = Tile(RuntimePayloadBuilder.Build(manifest, null, Page, TimeZoneInfo.Utc), "fd_ui");

        Assert.Equal(("neutral", "Not checked"), (tile.State, tile.Label));
        Assert.Equal(new RuntimeTileLine("not in topology.json", "muted"), Assert.Single(tile.Lines));
        Assert.StartsWith("fd-cmdemo2-uat: the topology this page loaded has no such address", tile.Title, StringComparison.Ordinal);
    }

    /// <summary>The tile of one node of the payload.</summary>
    private static RuntimeTile Tile(RuntimePayload payload, string alias) => payload.Nodes.Single(tile => tile.Alias == alias);

    /// <summary>The line that names one label word: the one <c>private const</c> holding it.</summary>
    private static bool IsDeclaration(string line, string word) =>
        !IsComment(line) && line.TrimStart().StartsWith("private const string", StringComparison.Ordinal)
        && line.Contains($"= \"{word}\";", StringComparison.Ordinal);

    /// <summary>A line that says nothing to the compiler: it is allowed to quote a label word while it explains it.</summary>
    private static bool IsComment(string line) => line.TrimStart().StartsWith("//", StringComparison.Ordinal);

    private static string BuilderSource => MarkupGuards.SourcePath(Path.Combine("Runtime", "RuntimePayloadBuilder.cs"));
}
