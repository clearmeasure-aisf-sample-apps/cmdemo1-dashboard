namespace Dashboard;

/// <summary>
/// The dashboard's one rule for cutting a line short at its end <em>without</em> a mark: the text as it stands when
/// it fits, otherwise its first <paramref name="length" /> characters. No mark, no trim - the head is kept whole.
/// This is the cut <see cref="Ellipsis.Cut" /> is not: <see cref="Ellipsis.Cut" /> keeps one character fewer, trims
/// the whitespace that would sit before the mark, and ends with the one-character <see cref="Ellipsis.Mark" />. The
/// two cuts agree at the boundary: a text of exactly <paramref name="length" /> characters is kept whole here and is
/// the shortest text <see cref="Ellipsis.Cut" /> can mark, so a caller that chooses by length picks the one that fits.
/// The no-mark cut has no guard of its own: a length of 0 keeps no characters, and a negative length is refused by the
/// slice itself, as <see cref="Ellipsis.Cut" /> refuses a length below 1.
/// </summary>
internal static class Clip
{
    /// <summary>
    /// <paramref name="text" /> as it stands when it is at most <paramref name="length" /> characters, otherwise its
    /// first <paramref name="length" /> characters - no mark. A text of exactly <paramref name="length" /> characters
    /// is kept whole; one character longer comes back as its first <paramref name="length" />. The caller decides
    /// whether the text needs cutting at all, and this never trims.
    /// </summary>
    internal static string At(string text, int length) => text.Length <= length ? text : text[..length];
}
