using System.Text.Json;

namespace Dashboard.Health;

/// <param name="LinesOfCode">Lines of code of the repository.</param>
/// <param name="Files">Its files.</param>
/// <param name="Languages">The lines by language, largest first.</param>
public sealed record CodeSize(long? LinesOfCode, int? Files, IReadOnlyList<LanguageSize> Languages)
{
    /// <summary>The word a build file says the list of its languages in.</summary>
    private const string LanguagesWord = "languages";

    /// <summary>The word a build file says the total lines of code in.</summary>
    private const string LinesOfCodeWord = "linesOfCode";

    internal static CodeSize? Read(JsonElement? section)
    {
        if (section is null)
        {
            return null;
        }

        var languages = JsonRead.Items(section, LanguagesWord)
            .Select(LanguageSize.Read)
            .OfType<LanguageSize>()
            .OrderByDescending(language => language.Lines)
            .ToList();
        var lines = JsonRead.Long(section, LinesOfCodeWord) ?? (languages.Count > 0 ? languages.Sum(language => language.Lines) : null);
        var code = new CodeSize(lines, JsonRead.Count(section, LanguageSize.FilesWord), languages);
        return MeasuredFacts.OrNone(code, code.MeasuredNothing);
    }

    /// <summary>No lines, no files and no languages: the build measured no code.</summary>
    internal bool MeasuredNothing => MeasuredFacts.None(LinesOfCode, Files) && Languages.Count == 0;
}
