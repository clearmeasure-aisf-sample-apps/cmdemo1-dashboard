namespace Dashboard;

/// <summary>
/// The dashboard's one rule for cutting a line short at its end: keep one character fewer than the line holds, trim
/// the whitespace that would sit before the mark, and end with the one-character <see cref="Mark" />, so the cut
/// never runs past the length. <see cref="Health.HealthDetailText.Brief" /> (the diagram's health lines) and
/// <see cref="Health.DeploymentActivity.Cut" /> (a node's deployment lines) both cut this way, each behind its own
/// "does it fit" test and its own guard. The cut has no guard of its own: a length below 1 is refused by the slice
/// itself, with the length less one as the actual value, which is the refusal <c>Brief</c> has always given. The middle
/// ellipsis of a node's name in <see cref="Cluster.ClusterText" /> keeps both ends of the name and is a different rule;
/// <see cref="Cluster.ClusterText.ShortNode" /> borrows the <see cref="Mark" /> for that rule.
/// </summary>
internal static class Ellipsis
{
    /// <summary>The one character that says the words go on: <c>…</c>.</summary>
    internal const string Mark = "…";

    /// <summary>
    /// The start of <paramref name="text" /> cut to <paramref name="length" /> characters with the
    /// <see cref="Mark" /> as the last of them: the first <c>length - 1</c> characters, trailing whitespace trimmed,
    /// then the mark. The caller decides whether the text needs cutting at all.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="length" /> is below 1, from the slice.</exception>
    internal static string Cut(string text, int length) => $"{text[..(length - 1)].TrimEnd()}{Mark}";
}
