namespace Dashboard.Tests;

/// <summary>
/// #1829 (step 2 of #534): every edge mark and its helpers live in <c>Runtime/RuntimeEdgeMarks.cs</c>, and
/// <c>RuntimePayloadBuilder</c> keeps the tiles and the regions and asks <c>RuntimeEdgeMarks.Edge</c> for each arrow.
/// The source-scan guards of the edges (#1511, #1734) read the new file; these tests pin that the builder holds no
/// second copy for them to miss, and that neither class reaches into the other's private members.
/// </summary>
public class RuntimeEdgeMarksFileTests
{
    private static readonly string[] Moved =
    [
        "Edge", "OriginEdge", "SqlEdge", "DependencyEdge", "PublicEdge", "DashboardEdge", "UnknownEdge", "ContainerSqlEdge",
        "CallerState", "AnswerState", "PublicCalls", "PerMinute", "ReportsNone", "Counted", "SqlState", "Carries", "SameSite",
    ];

    private static string Builder => File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("Runtime", "RuntimePayloadBuilder.cs")));

    private static string EdgeMarks => File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("Runtime", "RuntimeEdgeMarks.cs")));

    [Fact]
    public void EveryMovedMethodIsDeclaredInTheEdgeMarksAndNotInTheBuilder()
    {
        var builder = Builder;
        var edgeMarks = EdgeMarks;

        Assert.Contains("internal static class RuntimeEdgeMarks", edgeMarks, StringComparison.Ordinal);
        Assert.All(Moved, name =>
        {
            Assert.Matches($@"static [\w<>?,() ]+ {name}\(", edgeMarks);
            Assert.DoesNotMatch($@"static [\w<>?,() ]+ {name}\(", builder);
        });
    }

    [Fact]
    public void TheBuilderMakesNoEdgeMarkAndSaysNoCountingSentence()
    {
        var builder = Builder;

        Assert.True(builder.Length > 10_000, "only a few characters read from the builder: the guard is checking nothing");
        Assert.Equal(0, MarkupGuards.CountOccurrences(builder, "new RuntimeEdgeMark("));
        Assert.Equal(0, MarkupGuards.CountOccurrences(builder, "the web app reports none"));
        Assert.Equal(0, MarkupGuards.CountOccurrences(builder, "Last minute, counted by the web app:"));
        Assert.Equal(0, MarkupGuards.CountOccurrences(builder, " per minute: {"));
        Assert.Equal(1, MarkupGuards.CountOccurrences(builder, "RuntimeEdgeMarks.Edge(edge, index, probed)"));
    }

    [Fact]
    public void TheEdgeMarksMakeAllSevenMarksTheBuilderOnceMade()
    {
        Assert.Equal(7, MarkupGuards.CountOccurrences(EdgeMarks, "new RuntimeEdgeMark("));
    }

    [Fact]
    public void TheEntryIsOneSharedInternalTypeAndTheEdgeMarksReachNoPrivateOfTheBuilder()
    {
        var builder = Builder;
        var edgeMarks = EdgeMarks;
        var entry = typeof(RuntimePayloadBuilder).GetNestedType("Entry", System.Reflection.BindingFlags.NonPublic);

        // Internal (assembly), not private: the one entry type the builder and the edge marks share.
        Assert.NotNull(entry);
        Assert.True(entry.IsNestedAssembly, "RuntimePayloadBuilder.Entry is not internal");
        Assert.Equal(1, MarkupGuards.CountOccurrences(builder, "internal sealed record Entry("));
        Assert.DoesNotContain("record Entry(", edgeMarks, StringComparison.Ordinal);

        // The edge marks read the builder's entry type and its public payload words, never one of its private helpers.
        var reaches = System.Text.RegularExpressions.Regex.Matches(edgeMarks, @"RuntimePayloadBuilder\.(\w+)")
            .Select(match => match.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal);
        Assert.Equal(["BackgroundSql", "CallsUnit", "Entry", "NoNumber", "PartSeparator"], reaches);
        Assert.Equal("–", RuntimePayloadBuilder.NoNumber);
        Assert.Equal("calls/min", RuntimePayloadBuilder.CallsUnit);
        Assert.Equal("in the background (mostly the message bus polling the database)", RuntimePayloadBuilder.BackgroundSql);
    }
}
