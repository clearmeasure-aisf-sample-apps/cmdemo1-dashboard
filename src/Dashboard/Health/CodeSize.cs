using System.Text.Json;

namespace Dashboard.Health;

/// <param name="LinesOfCode">Lines of code of the repository.</param>
/// <param name="Files">Its files.</param>
/// <param name="Languages">The lines by language, largest first.</param>
public sealed record CodeSize(long? LinesOfCode, int? Files, IReadOnlyList<LanguageSize> Languages)
{
    internal static CodeSize? Read(JsonElement? section)
    {
        if (section is null)
        {
            return null;
        }

        var languages = JsonRead.Items(section, "languages")
            .Select(item => (Name: JsonRead.Text(item, "name"), Lines: JsonRead.Long(item, "lines"), Files: JsonRead.Count(item, "files")))
            .Where(item => item.Name is not null && item.Lines > 0)
            .Select(item => new LanguageSize(item.Name!, item.Lines!.Value, item.Files))
            .OrderByDescending(language => language.Lines)
            .ToList();
        var lines = JsonRead.Long(section, "linesOfCode") ?? (languages.Count > 0 ? languages.Sum(language => language.Lines) : null);
        var code = new CodeSize(lines, JsonRead.Count(section, "files"), languages);
        return MeasuredFacts.OrNone(code, code.MeasuredNothing);
    }

    /// <summary>No lines, no files and no languages: the build measured no code.</summary>
    internal bool MeasuredNothing => MeasuredFacts.None(LinesOfCode, Files) && Languages.Count == 0;
}
