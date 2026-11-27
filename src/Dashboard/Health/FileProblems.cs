namespace Dashboard.Health;

/// <summary>
/// The three sentences every reader of a file the dashboard does not own says when what it was pointed at is no file:
/// nothing to read, text that is no JSON object, text that is no JSON at all. They are copy a viewer reads, not
/// diagnostics - <see cref="Dashboard.Cluster.Parsed{T}.Problem" /> carries them to
/// <see cref="Dashboard.Cluster.SourceReading{T}.Detail" /> and to the card under it, and
/// <see cref="PinnedVersions.Detail" /> to the version line - so they are held in the one place
/// <see cref="JsonRead" /> already owns the rest of what those readers have in common. A reader that words one of them
/// a second way is a card that says "the file is empty" for one source and something else for another, which is a
/// difference no reader can do anything with.
/// <para>
/// Each is the end of a sentence, lower case and with no full stop, the way every other refusal of the page is worded.
/// The tests that pin a refusal quote these names rather than typing the words again, so a rewording is one edit and
/// no drift; the page-facing tests that name what the card shows (<c>ClusterReaderEdgeCaseTests</c>,
/// <c>ClusterMonitorEdgeCaseTests</c>) keep the words themselves, so that a change here is a change they see.
/// </para>
/// </summary>
internal static class FileProblems
{
    /// <summary>There was nothing to read: no content at all, or content of nothing but blanks.</summary>
    public const string Empty = "the file is empty";

    /// <summary>There was content and it is JSON, but not the one shape every such file holds: an object.</summary>
    public const string NotAnObject = "the file does not contain a JSON object";

    /// <summary>There was content and it is not JSON: cut off, mistyped, or not text at all.</summary>
    public const string Broken = "the file is not valid JSON";

    /// <summary>
    /// A required word of the file is absent, null, or blank at <paramref name="place"/> (a dotted path such as
    /// <c>environments[0].name</c>). Unlike the three ends above, this is a whole refusal of a runtime reader: the place,
    /// a colon, and the words ending with a full stop, the way those readers' refusals are worded.
    /// </summary>
    public static string MissingOrEmpty(string place) => $"{place}: missing or empty.";
}
