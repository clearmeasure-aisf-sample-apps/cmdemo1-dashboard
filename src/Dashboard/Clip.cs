namespace Dashboard;

/// <summary>
/// The dashboard's one rule for cutting a line short at its head without a mark: the text as it stands when it
/// fits, otherwise its first <paramref name="length"/> characters. <see cref="Ellipsis.Cut"/> is the rule with a
/// mark, which keeps one character fewer than the line holds and ends with the one-character ellipsis.
/// </summary>
internal static class Clip
{
    /// <summary>
    /// The text as it stands when it fits in <paramref name="length"/> characters, otherwise its first
    /// <paramref name="length"/> characters, with no mark. A text of exactly <paramref name="length"/> characters
    /// is kept whole; a length of 0 gives the empty string.
    /// </summary>
    internal static string At(string text, int length) => text.Length <= length ? text : text[..length];
}
