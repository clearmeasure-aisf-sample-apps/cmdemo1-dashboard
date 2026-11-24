using System.Globalization;
using System.Text.Json;

namespace Dashboard.Health;

/// <summary>
/// What a deployable says about the build it runs, at its build endpoint (<c>buildPath</c> of the topology, for
/// example <c>/_build</c>): the commit, and the facts the build measured about the code. Every part is optional: an
/// app reports what its build measured, and the card shows what is there.
/// </summary>
/// <param name="Version">The version of the build, without build metadata.</param>
/// <param name="Commit">The commit the build was made from.</param>
/// <param name="CommitUrl">The commit's page.</param>
/// <param name="BuiltAt">When the build ran.</param>
/// <param name="BuildUrl">The build's run.</param>
public sealed record BuildInfo(
    string? Version,
    string? Commit,
    Uri? CommitUrl,
    DateTimeOffset? BuiltAt,
    Uri? BuildUrl,
    CodeSize? Code,
    TestCounts? Tests,
    CoverageFacts? Coverage,
    ComplexityFacts? Complexity,
    CrapFacts? Crap,
    int? QodanaProblems)
{
    /// <summary>The endpoint's answer; null when it is not a JSON object or holds none of the parts (an error page).</summary>
    public static BuildInfo? Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var build = new BuildInfo(
                VersionText.Display(JsonRead.Text(root, "version")),
                JsonRead.Text(root, "commit"),
                JsonRead.Address(root, "commitUrl"),
                JsonRead.Time(root, "builtAt"),
                JsonRead.Address(root, "buildUrl"),
                CodeSize.Read(JsonRead.Section(root, "code")),
                TestCounts.Read(JsonRead.Section(root, "tests")),
                CoverageFacts.Read(JsonRead.Section(root, "coverage")),
                ComplexityFacts.Read(JsonRead.Section(root, "complexity")),
                CrapFacts.Read(JsonRead.Section(root, "crap")),
                JsonRead.Count(JsonRead.Section(root, "analysis"), "qodanaProblems"));
            return MeasuredFacts.OrNone(build, build.MeasuredNothing);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// True when the answer holds none of the parts: no build. The commit's page is not one of them - a page without
    /// its commit says no build.
    /// </summary>
    private bool MeasuredNothing =>
        MeasuredFacts.None(Version, Commit, BuiltAt, BuildUrl, Code, Tests, Coverage, Complexity, Crap, QodanaProblems);
}

/// <summary>The one rule of the build's facts for "nothing measured": every part came back without a value.</summary>
internal static class MeasuredFacts
{
    /// <summary>
    /// The record as read; null when it measured nothing, so the card shows nothing for it: a record of nothing is no
    /// record. The record says what "nothing" is for its parts; this says what follows from it, once.
    /// </summary>
    internal static T? OrNone<T>(T facts, bool measuredNothing)
        where T : class => measuredNothing ? null : facts;

    /// <summary>True when every part is null: the build measured nothing there, and there is no record.</summary>
    internal static bool None(params ReadOnlySpan<object?> parts)
    {
        foreach (var part in parts)
        {
            if (part is not null)
            {
                return false;
            }
        }

        return true;
    }
}

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

public sealed record LanguageSize(string Name, long Lines, int? Files);

/// <summary>One part of the language bar: a language, or "Other" for the small ones together.</summary>
/// <param name="Slot">The part's colour, by position: 1 for the largest language; 0 for "Other".</param>
/// <param name="Percent">Its share of the lines of all languages.</param>
public sealed record LanguageShare(string Name, long Lines, double Percent, int Slot);

public sealed record TestCounts(int? Unit, int? Integration, int? Acceptance)
{
    public int Total => (Unit ?? 0) + (Integration ?? 0) + (Acceptance ?? 0);

    internal static TestCounts? Read(JsonElement? section)
    {
        var tests = new TestCounts(JsonRead.Count(section, "unit"), JsonRead.Count(section, "integration"), JsonRead.Count(section, "acceptance"));
        return MeasuredFacts.OrNone(tests, tests.MeasuredNothing);
    }

    internal bool MeasuredNothing => MeasuredFacts.None(Unit, Integration, Acceptance);
}

public sealed record CoverageFacts(double? LinePercent, double? BranchPercent)
{
    internal static CoverageFacts? Read(JsonElement? section)
    {
        var coverage = new CoverageFacts(JsonRead.Number(section, "linePercent"), JsonRead.Number(section, "branchPercent"));
        return MeasuredFacts.OrNone(coverage, coverage.MeasuredNothing);
    }

    internal bool MeasuredNothing => MeasuredFacts.None(LinePercent, BranchPercent);
}

/// <summary>Cyclomatic complexity of the methods.</summary>
public sealed record ComplexityFacts(double? Average, int? Max, int? Methods)
{
    internal static ComplexityFacts? Read(JsonElement? section)
    {
        var complexity = new ComplexityFacts(JsonRead.Number(section, "average"), JsonRead.Count(section, "max"), JsonRead.Count(section, "methods"));
        return MeasuredFacts.OrNone(complexity, complexity.MeasuredNothing);
    }

    internal bool MeasuredNothing => MeasuredFacts.None(Average, Max, Methods);
}

/// <summary>The CRAP score (change risk anti-patterns): complexity that tests do not cover.</summary>
public sealed record CrapFacts(double? Max, double? Threshold, int? OverThreshold)
{
    internal static CrapFacts? Read(JsonElement? section)
    {
        var crap = new CrapFacts(JsonRead.Number(section, "max"), JsonRead.Number(section, "threshold"), JsonRead.Count(section, "overThreshold"));
        return MeasuredFacts.OrNone(crap, crap.MeasuredNothing);
    }

    internal bool MeasuredNothing => MeasuredFacts.None(Max, Threshold, OverThreshold);
}

/// <summary>The build's facts in the words of the "Code" card.</summary>
public static class BuildText
{
    /// <summary>How many languages the bar names before the rest becomes "Other": one per colour of the bar.</summary>
    public const int NamedLanguages = 5;

    public const string Other = "Other";

    /// <summary>What the page calls itself next to the dashboard's name: the chip of its heading.</summary>
    public const string ThisPage = "this page";

    /// <summary>What the dashboard's own "Code" card is, as the hover text of its section's chip.</summary>
    public const string DashboardHelp =
        "The build that serves this page, as its own site publishes it: the same facts every app of the system reports about its build.";

    /// <summary>Whose code the dashboard's own "Code" card is about: <c>dashboard (this page)</c>.</summary>
    public static string DashboardContext(string name) => $"{name} ({ThisPage})";

    /// <summary>The first seven characters of a commit.</summary>
    public static string ShortCommit(string commit)
    {
        ArgumentNullException.ThrowIfNull(commit);
        return commit.Length > 7 ? commit[..7] : commit;
    }

    /// <summary><c>84,210</c>.</summary>
    public static string Count(long number) => number.ToString("N0", CultureInfo.InvariantCulture);

    /// <summary><c>81.2 %</c>.</summary>
    public static string Percent(double percent) => string.Create(CultureInfo.InvariantCulture, $"{percent:0.#} %");

    public static string Number(double number) => number.ToString("0.#", CultureInfo.InvariantCulture);

    /// <summary><c>84,210 lines in 1,203 files</c>.</summary>
    public static string? Size(CodeSize code)
    {
        ArgumentNullException.ThrowIfNull(code);
        return (code.LinesOfCode, code.Files) switch
        {
            ({ } lines, { } files) => $"{Count(lines)} lines in {Count(files)} files",
            ({ } lines, null) => $"{Count(lines)} lines",
            (null, { } files) => $"{Count(files)} files",
            _ => null,
        };
    }

    /// <summary>
    /// The parts of the language bar: the largest languages by name, the rest as "Other", each with its share of the
    /// lines of all languages. Empty without languages.
    /// </summary>
    public static IReadOnlyList<LanguageShare> Shares(CodeSize code)
    {
        ArgumentNullException.ThrowIfNull(code);
        var total = code.Languages.Sum(language => language.Lines);
        if (total <= 0)
        {
            return [];
        }

        var shares = code.Languages
            .Take(NamedLanguages)
            .Select((language, index) => new LanguageShare(language.Name, language.Lines, 100d * language.Lines / total, index + 1))
            .ToList();
        var rest = code.Languages.Skip(NamedLanguages).Sum(language => language.Lines);
        if (rest > 0)
        {
            shares.Add(new LanguageShare(Other, rest, 100d * rest / total, 0));
        }

        return shares;
    }

    /// <summary>The bar in words: <c>C# 73 %, Razor 12 %, Other 15 %</c>.</summary>
    public static string Describe(IEnumerable<LanguageShare> shares) =>
        string.Join(", ", shares.Select(share => $"{share.Name} {SharePercent(share.Percent)}"));

    /// <summary>A share as a whole percentage; a share below one as <c>&lt;1 %</c>.</summary>
    public static string SharePercent(double percent) =>
        percent is > 0 and < 1 ? "<1 %" : string.Create(CultureInfo.InvariantCulture, $"{Math.Round(percent):0} %");

    /// <summary><c>1,417: 1,009 unit, 240 integration, 168 acceptance</c>.</summary>
    public static string Tests(TestCounts tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        var parts = new CardParts()
            .Add(Counted(tests.Unit, "unit"))
            .Add(Counted(tests.Integration, "integration"))
            .Add(Counted(tests.Acceptance, "acceptance"));
        return parts.Count == 1 ? parts.Join() : $"{Count(tests.Total)}: {parts.Join()}";
    }

    /// <summary><c>81.2 % of lines, 70.1 % of branches</c>.</summary>
    public static string Coverage(CoverageFacts coverage)
    {
        ArgumentNullException.ThrowIfNull(coverage);
        return new CardParts()
            .Add(coverage.LinePercent is { } lines ? $"{Percent(lines)} of lines" : null)
            .Add(coverage.BranchPercent is { } branches ? $"{Percent(branches)} of branches" : null)
            .Join();
    }

    /// <summary><c>average 1.9, worst 34 (5,210 methods)</c>.</summary>
    public static string Complexity(ComplexityFacts complexity)
    {
        ArgumentNullException.ThrowIfNull(complexity);
        return new CardParts()
            .Add(complexity.Average is { } average ? $"average {Number(average)}" : null)
            .Add(complexity.Max is { } max ? $"worst {Count(max)}" : null)
            .Join(complexity.Methods is { } methods ? $"({Count(methods)} methods)" : null);
    }

    /// <summary><c>worst 28.5, none over 30</c>; <c>worst 41, 3 methods over 30</c>.</summary>
    public static string Crap(CrapFacts crap)
    {
        ArgumentNullException.ThrowIfNull(crap);
        return new CardParts()
            .Add(crap.Max is { } max ? $"worst {Number(max)}" : null)
            .Add(CrapOver(crap) ?? (crap.Threshold is { } threshold ? $"threshold {Number(threshold)}" : null))
            .Join();
    }

    /// <summary><c>1,009 unit</c>; null when the build did not count that kind.</summary>
    private static string? Counted(int? count, string kind) => count is { } value ? $"{Count(value)} {kind}" : null;

    /// <summary><c>none over 30</c>, <c>1 method over 30</c>, <c>3 methods over the threshold</c>; null when not counted.</summary>
    private static string? CrapOver(CrapFacts crap)
    {
        var limit = crap.Threshold is { } threshold ? $"over {Number(threshold)}" : "over the threshold";
        return crap.OverThreshold switch
        {
            null => null,
            0 => $"none {limit}",
            1 => $"1 method {limit}",
            { } over => $"{Count(over)} methods {limit}",
        };
    }

    /// <summary>True when a method's CRAP score is over the threshold: the card marks it.</summary>
    public static bool CrapIsOver(CrapFacts crap)
    {
        ArgumentNullException.ThrowIfNull(crap);
        return crap.OverThreshold > 0 || (crap.OverThreshold is null && crap.Max > crap.Threshold);
    }

    /// <summary><c>no problems</c>, <c>1 problem</c>, <c>12 problems</c>.</summary>
    public static string Problems(int problems) => problems switch
    {
        0 => "no problems",
        1 => "1 problem",
        _ => $"{Count(problems)} problems",
    };
}
