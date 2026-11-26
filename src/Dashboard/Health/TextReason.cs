namespace Dashboard.Health;

/// <summary>
/// The one rule a detail of the health page is joined with the words it explains, in the shape the cluster settled
/// it (#1167): the spaces around the detail and the full stops it ends in - a stop followed by a space is still a
/// stop - are not said, so the one that closes the sentence is the only one said, and a detail that is nothing but
/// spaces or stops says nothing at all, not even the colon.
/// </summary>
internal static class TextReason
{
    /// <summary>
    /// A detail as the end of what comes before it: <c>: Login failed</c>, trimmed by <see cref="Words"/>; empty
    /// when there is nothing to say.
    /// </summary>
    internal static string Reason(string? detail) => Words(detail) is { Length: > 0 } words ? $": {words}" : string.Empty;

    /// <summary>
    /// The words of a detail without the spaces around them and without the full stops they end in - a stop
    /// followed by a space is still a stop - so the one that closes the sentence is the only one said.
    /// </summary>
    private static string Words(string? detail)
    {
        var words = (detail ?? string.Empty).Trim();
        while (words.EndsWith('.'))
        {
            words = words.TrimEnd('.').TrimEnd();
        }

        return words;
    }
}
