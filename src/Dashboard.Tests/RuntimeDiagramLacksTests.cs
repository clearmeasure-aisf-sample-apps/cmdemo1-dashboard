namespace Dashboard.Tests;

/// <summary>
/// The words <see cref="RuntimeDiagram.Lacks" /> says of the boxes the manifest has and the SVG lacks: the one static
/// sentence the runtime view shows under the picture when the two of a deployment do not match. It joins the missing
/// names with <c>", "</c> (never an <c>and</c>, so it is not routed through <see cref="NamedList.Join"/>) and says
/// nothing at all when nothing is missing - null, the null-for-nothing convention the runtime sentences keep.
/// </summary>
public class RuntimeDiagramLacksTests
{
    /// <summary>Nothing missing is no sentence: the diagram says nothing, and the page shows nothing.</summary>
    [Fact]
    public void NothingMissingSaysNothing()
    {
        Assert.Null(RuntimeDiagram.Lacks([]));
    }

    /// <summary>One missing box is named as the word it is, with no separator and no <c>and</c>.</summary>
    [Fact]
    public void OneMissingBoxIsNamedAlone()
    {
        Assert.Equal(
            "The diagram lacks api: the manifest and the SVG of this deployment do not match.",
            RuntimeDiagram.Lacks(["api"]));
    }

    /// <summary>Two missing boxes are joined with a comma and a space, in order, with no <c>and</c> before the last.</summary>
    [Fact]
    public void TwoMissingBoxesAreJoinedWithACommaAndASpace()
    {
        Assert.Equal(
            "The diagram lacks api, db: the manifest and the SVG of this deployment do not match.",
            RuntimeDiagram.Lacks(["api", "db"]));
    }

    /// <summary>Three missing boxes stay in order, each joined with a comma and a space, with no <c>and</c>.</summary>
    [Fact]
    public void ThreeMissingBoxesAreJoinedInOrderWithNoAnd()
    {
        Assert.Equal(
            "The diagram lacks api, db, cache: the manifest and the SVG of this deployment do not match.",
            RuntimeDiagram.Lacks(["api", "db", "cache"]));
    }

    /// <summary>The words are exactly the sentence the page renders today - the lead, the separator, the colon and the
    /// tail - so moving the sentence into C# changes nothing the reader sees.</summary>
    [Fact]
    public void TheWordsAreTheSentenceThePageRenderedToday()
    {
        var two = RuntimeDiagram.Lacks(["api", "db"]);

        Assert.StartsWith("The diagram lacks ", two, StringComparison.Ordinal);
        Assert.Contains("api, db", two, StringComparison.Ordinal);
        Assert.EndsWith(": the manifest and the SVG of this deployment do not match.", two, StringComparison.Ordinal);
        // The separator is a comma and a space, never an and - NamedList would have written "api and db".
        Assert.DoesNotContain("api and db", two, StringComparison.Ordinal);
    }
}
