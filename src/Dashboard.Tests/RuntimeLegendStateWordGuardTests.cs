namespace Dashboard.Tests;

/// <summary>
/// <c>RuntimeLegend.razor</c> explains the runtime diagram's colours by giving each swatch a
/// <c>data-rt-state</c> attribute. The attribute's word is owned by <c>RuntimeStateWords</c> at the JSON
/// boundary, so the legend must not retype the words itself: if a word is renamed in
/// <c>RuntimePayloadBuilder</c> or <c>RuntimeStateWords</c>, a hand-typed legend swatch silently loses its
/// colour and nothing catches it. The tables in the component are keyed by enum (<see cref="Dashboard.Runtime.TileState"/>,
/// <see cref="Dashboard.Runtime.RegionState"/>, <see cref="Dashboard.Runtime.EdgeState"/>) and the markup writes
/// <c>@state.Word()</c>, so this guard scans the source for any quoted state word that would mean the enum key was
/// replaced by a string again.
/// </summary>
public sealed class RuntimeLegendStateWordGuardTests
{
    /// <summary>The words <c>RuntimeStateWords</c> and <c>RuntimePayloadBuilder</c> own at the JSON boundary. None of
    /// them may appear quoted in the legend's source.</summary>
    private static readonly string[] QuotedStateWords =
    [
        "\"healthy\"",
        "\"unhealthy\"",
        "\"unreachable\"",
        "\"checking\"",
        "\"neutral\"",
        "\"serving\"",
        "\"standby\"",
        "\"down\"",
        "\"active\"",
        "\"idle\"",
    ];

    /// <summary>No quoted state word may stand in the legend's source: the tables are keyed by enum and the markup
    /// writes <c>@state.Word()</c>, so a hand-typed word would mean the enum key was replaced by a string again.</summary>
    [Fact]
    public void TheLegendHoldsNoQuotedStateWords()
    {
        var source = MarkupGuards.ComponentText("RuntimeLegend");
        var offenders = QuotedStateWords
            .Where(word => source.Contains(word, StringComparison.Ordinal))
            .ToList();

        Assert.Empty(offenders);
    }

    /// <summary>The number-line rows read <c>RuntimePayloadBuilder.CallsUnit</c> and
    /// <c>RuntimePayloadBuilder.NoNumber</c>, not the hand-typed <c>calls/min</c> or the en-dash <c>–</c>.</summary>
    [Fact]
    public void TheLegendNumberLinesUseTheNamedConstants()
    {
        var source = MarkupGuards.ComponentText("RuntimeLegend");

        Assert.Contains("RuntimePayloadBuilder.CallsUnit", source, StringComparison.Ordinal);
        Assert.Contains("RuntimePayloadBuilder.NoNumber", source, StringComparison.Ordinal);
    }

    /// <summary>The tables are keyed by enum, not by string: the source names the enum member tuples.</summary>
    [Fact]
    public void TheLegendTablesAreKeyedByEnum()
    {
        var source = MarkupGuards.ComponentText("RuntimeLegend");

        Assert.Contains("(TileState State", source, StringComparison.Ordinal);
        Assert.Contains("(RegionState State", source, StringComparison.Ordinal);
        Assert.Contains("(EdgeState State", source, StringComparison.Ordinal);
        Assert.Contains("TileState.Healthy", source, StringComparison.Ordinal);
        Assert.Contains("RegionState.Serving", source, StringComparison.Ordinal);
        Assert.Contains("EdgeState.Active", source, StringComparison.Ordinal);
    }

    /// <summary>The three swatch loops write <c>data-rt-state="@state.Word()"</c>: one per table, no more, no less.</summary>
    [Fact]
    public void TheLegendSwatchesWriteTheStateWordThroughTheExtension()
    {
        var source = MarkupGuards.ComponentText("RuntimeLegend");

        Assert.Equal(3, MarkupGuards.CountOccurrences(source, "data-rt-state=\"@state.Word()\""));
    }
}
