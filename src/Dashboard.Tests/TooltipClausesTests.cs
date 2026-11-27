using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="TooltipClauses" /> is the one place the rule "a tooltip's detail is its clauses joined with <c>"; "</c>,
/// and no tooltip detail (<c>null</c>) when there are no clauses" is written: the tooltip's counterpart of
/// <see cref="CardParts" />, which collects the parts of a card line and joins them with <c>", "</c>.
/// </summary>
public class TooltipClausesTests
{
    [Fact]
    public void NoClausesGiveNull() =>
        Assert.Null(TooltipClauses.Join(Array.Empty<string>()));

    [Fact]
    public void OneClauseGivesThatClauseUnchanged() =>
        Assert.Equal("Working set 412 MB", TooltipClauses.Join(["Working set 412 MB"]));

    [Fact]
    public void TwoClausesAreJoinedWithASemicolon() =>
        Assert.Equal("a; b", TooltipClauses.Join(["a", "b"]));

    [Fact]
    public void ThreeClausesKeepTheirOrder() =>
        Assert.Equal("the sign-off is with demo approvers; asked 2 h ago; started by jeffrey",
            TooltipClauses.Join(["the sign-off is with demo approvers", "asked 2 h ago", "started by jeffrey"]));

    [Fact]
    public void ANullArgumentThrows() =>
        Assert.Throws<ArgumentNullException>(() => TooltipClauses.Join(null!));

    [Fact]
    public void AListWithClausesJoinsThem()
    {
        var parts = new List<string> { "Working set 412 MB, of which managed heap 96 MB", "41 threads" };

        Assert.Equal("Working set 412 MB, of which managed heap 96 MB; 41 threads", TooltipClauses.Join(parts));
    }

    [Fact]
    public void TheSeparatorIsASemicolonFollowedByASpace()
    {
        // The separator is "; " (semicolon + space), not ";" alone or " ; " with a leading space.
        var result = TooltipClauses.Join(["a", "b", "c"]);

        Assert.Equal("a; b; c", result);
        Assert.DoesNotContain(";", result!.Replace("; ", ""), StringComparison.Ordinal);
    }
}
