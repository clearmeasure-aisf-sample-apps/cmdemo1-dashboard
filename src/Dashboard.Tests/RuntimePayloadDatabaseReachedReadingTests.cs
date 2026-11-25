namespace Dashboard.Tests;

/// <summary>
/// The two exits that reach the database (#1752) project their clients to one reading of an app that uses it,
/// <c>Reached</c> in <c>RuntimePayloadBuilder</c>, and one method, <c>DatabaseReachedTile</c>, owns the reachable
/// branch over that reading: the names the title speaks and the latest of the health checks are read off it, while
/// the words each client source contributes (its app noun for <c>PassedLine</c>, its <c>queries/min</c> fact) stay
/// with the source. These tests pin that shape in the builder's own source, the way
/// <see cref="OnlyProbesLivenessTests.TheLivenessQuestionIsWrittenOnceAndAskedByBothArms"/> pins the liveness
/// question.
/// </summary>
public class RuntimePayloadDatabaseReachedReadingTests
{
    [Fact]
    public void BothClientSourcesReachTheDatabaseThroughOneReading()
    {
        var source = File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("Runtime", "RuntimePayloadBuilder.cs")));

        // A guard that read nothing proved nothing: the builder is a thousand lines of tile and arrow.
        Assert.True(source.Length > 10_000, "only a few characters read from the builder: the guard is checking nothing");

        // The reading is one small shape...
        Assert.Equal(1, Count(source, "record struct Reached("));
        // ...both client sources project their clients to it...
        Assert.Equal(2, Count(source, "new Reached("));
        // ...and the names and the latest of the reachable branch are read off it, written once.
        Assert.Equal(1, Count(source, "reached.Select(app => app.Name)"));
        Assert.Equal(1, Count(source, "reached.Max(app => app.CheckedAt)"));
        // The tile of a reached database is written once and built by both exits.
        Assert.Equal(3, Count(source, "DatabaseReachedTile("));
    }

    private static int Count(string text, string part) =>
        (text.Length - text.Replace(part, string.Empty, StringComparison.Ordinal).Length) / part.Length;
}
