namespace Dashboard.Health;

/// <summary>
/// The grammar of one line of a Kustomize file that pins a version: a <c>newTag</c> entry (<c>newTag: 2.4.15</c>, or
/// the first key of a list item, <c>- newTag: 2.4.15</c>), its value in double or single quotes or without, a trailing
/// comment left out. <see cref="PinnedVersions.ParseKustomization"/> asks it of each line in turn.
/// <para>
/// It depends on nothing else internal to the app, so the tests compile this one file themselves (as they do
/// <c>JsonRead</c>) and pin the grammar line by line.
/// </para>
/// </summary>
internal static class KustomizationPinLine
{
    /// <summary>The key whose value is the pinned version.</summary>
    public const string Key = "newTag";

    /// <summary>
    /// The value of a line that is a <c>newTag</c> entry, empty when it has none; null for any other line (a comment,
    /// another key).
    /// </summary>
    public static string? Of(string line)
    {
        var text = line.AsSpan().Trim();

        // The first key of a list item: "- newTag: 2.4.15".
        if (text.Length > 1 && text[0] == '-' && char.IsWhiteSpace(text[1]))
        {
            text = text[1..].TrimStart();
        }

        if (!text.StartsWith(Key, StringComparison.Ordinal))
        {
            return null;
        }

        text = text[Key.Length..].TrimStart();
        if (text.IsEmpty || text[0] != ':' || (text.Length > 1 && !char.IsWhiteSpace(text[1])))
        {
            return null;
        }

        text = text[1..].Trim();
        if (text.IsEmpty || text[0] == '#')
        {
            return string.Empty;
        }

        if (text[0] is '"' or '\'')
        {
            var rest = text[1..];
            var end = rest.IndexOf(text[0]);
            return (end < 0 ? rest : rest[..end]).ToString();
        }

        // Without quotes, a comment starts at a "#" that follows a space.
        for (var index = 1; index < text.Length; index++)
        {
            if (text[index] == '#' && char.IsWhiteSpace(text[index - 1]))
            {
                text = text[..index].TrimEnd();
                break;
            }
        }

        return text.ToString();
    }
}
