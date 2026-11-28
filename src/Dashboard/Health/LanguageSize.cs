using System.Text.Json;

namespace Dashboard.Health;

/// <summary>
/// One language of a build's code: its name, the lines the build counted for it, and the files it holds -
/// the lines and the files the build said for it, or the nothing it did not. The record is not the reader of
/// an answer: <see cref="Read" /> is, and it is the one place that says which of a build file's language
/// entries count - a name and more than zero lines, and no other rule.
/// </summary>
/// <param name="Name">The language's name.</param>
/// <param name="Lines">The lines the build counted for it.</param>
/// <param name="Files">The files it holds, or the nothing the build did not count them.</param>
public sealed record LanguageSize(string Name, long Lines, int? Files)
{
    /// <summary>The word a build file says a language's name in.</summary>
    internal const string NameWord = "name";

    /// <summary>The word a build file says a language's lines in.</summary>
    internal const string LinesWord = "lines";

    /// <summary>The word a build file says a language's files in.</summary>
    internal const string FilesWord = "files";

    /// <summary>
    /// The language the entry stands for; null when it is no language of the build's code - without a name, or
    /// without more than zero lines. The one rule of whether a language entry counts, said on the record it
    /// makes: a name and more than zero lines, and the files the build counted for it, or the nothing it did not.
    /// </summary>
    internal static LanguageSize? Read(JsonElement item) =>
        (JsonRead.Text(item, NameWord), JsonRead.Long(item, LinesWord), JsonRead.Count(item, FilesWord)) is
        ({ } name, { } lines and > 0, var files)
            ? new LanguageSize(name, lines, files)
            : null;
}
