namespace Dashboard.Tests;

/// <summary>
/// The words of the database's unconfirmed sentence (#1790) name the same two client sources the plural nouns name
/// (#1708): the app that uses the database, and the other possible cause of a failed check. As the label words
/// (#1672), each word is declared by exactly one <c>private const</c> in <c>RuntimePayloadBuilder</c>, next to the
/// sources it belongs to, and no call site writes a word as a quoted literal: a typo at a call site would otherwise
/// split the page's vocabulary silently, with a sentence that says a word the legend, the tests and the reader have
/// never heard.
/// </summary>
public class DatabaseUnconfirmedWordsTests
{
    /// <summary>The singular words of the unconfirmed sentence, as the builder names them.</summary>
    private static readonly string[] Words = ["web app", "container app", "app"];

    /// <summary>Every word has exactly one home in the builder: one <c>private const</c> per word.</summary>
    [Fact]
    public void EveryUnconfirmedWordIsNamedOnce()
    {
        var lines = File.ReadAllLines(BuilderSource);

        foreach (var word in Words)
        {
            Assert.Equal(1, lines.Count(line => IsDeclaration(line, word)));
        }
    }

    /// <summary>No line of the builder quotes an unconfirmed word without naming it: only the declaration itself, and
    /// a comment that explains the vocabulary, may hold the word in quotes.</summary>
    [Fact]
    public void NoCallSiteWritesAnUnconfirmedWordAsALiteral()
    {
        var lines = File.ReadAllLines(BuilderSource);

        // A guard that read nothing proved nothing: the builder is a thousand lines of tile and arrow.
        Assert.True(lines.Length > 500, $"only {lines.Length} line(s) read from {BuilderSource}: the guard is checking nothing");

        var offenders =
            from line in lines
            from word in Words
            where line.Contains($"\"{word}\"", StringComparison.Ordinal)
            where !IsDeclaration(line, word) && !IsComment(line)
            select $"{word}: {line.Trim()}";

        Assert.Empty(offenders);
    }

    /// <summary>The line that names one word: the one <c>private const</c> holding it.</summary>
    private static bool IsDeclaration(string line, string word) =>
        !IsComment(line) && line.TrimStart().StartsWith("private const string", StringComparison.Ordinal)
        && line.Contains($"= \"{word}\";", StringComparison.Ordinal);

    /// <summary>A line that says nothing to the compiler: it is allowed to quote a word while it explains it.</summary>
    private static bool IsComment(string line) => line.TrimStart().StartsWith("//", StringComparison.Ordinal);

    private static string BuilderSource => MarkupGuards.SourcePath(Path.Combine("Runtime", "RuntimePayloadBuilder.cs"));
}
