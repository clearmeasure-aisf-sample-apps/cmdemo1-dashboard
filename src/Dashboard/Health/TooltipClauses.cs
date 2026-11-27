namespace Dashboard.Health;

/// <summary>
/// The clauses of a tooltip's detail, joined once, here: a tooltip's detail is its clauses joined with <c>"; "</c>,
/// and no tooltip detail (<c>null</c>) when there are no clauses - <c>Working set 412 MB; 41 threads</c>. The
/// tooltip's counterpart of <see cref="CardParts" />, which collects the parts of a card line and joins them with
/// <c>", "</c>.
/// </summary>
public static class TooltipClauses
{
    /// <summary>The separator between the clauses of a tooltip's detail.</summary>
    private const string ClauseSeparator = "; ";

    /// <summary>
    /// The clauses of a tooltip's detail joined with <c>"; "</c> - <c>Working set 412 MB; 41 threads</c> - and
    /// <c>null</c> when there are none, since a tooltip with no detail says nothing.
    /// </summary>
    public static string? Join(IReadOnlyCollection<string> clauses)
    {
        ArgumentNullException.ThrowIfNull(clauses);
        return clauses.Count == 0 ? null : string.Join(ClauseSeparator, clauses);
    }
}
