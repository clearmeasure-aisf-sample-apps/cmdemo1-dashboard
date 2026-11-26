using System.Globalization;

namespace Dashboard.Health;

/// <summary>The build's facts in the words of the "Code" card.</summary>
public static class BuildText
{
    /// <summary>How many languages the bar names before the rest becomes "Other": one per colour of the bar.</summary>
    public const int NamedLanguages = 5;

    /// <summary>How many characters of a commit the page shows: git's own short form.</summary>
    private const int ShortCommitLength = 7;

    public const string Other = "Other";

    /// <summary>
    /// The scale of a share: the hundred the percent of the lines of all languages is cut from, as
    /// <see cref="LanguageShare.Percent" /> says it - the one <see cref="Dashboard.Shares.PercentScale" /> every
    /// share of the dashboard stands on.
    /// </summary>
    private const double PercentScale = Dashboard.Shares.PercentScale;

    /// <summary>
    /// The slot the first named language stands on, as the <see cref="LanguageShare.Slot" /> doc says it: 1 for the largest
    /// language, the bar colour <c>langbar__part--1</c>, each of the rest on the slot after it.
    /// </summary>
    private const int FirstNamedSlot = 1;

    /// <summary>
    /// The slot the "Other" part stands on, as the <see cref="LanguageShare.Slot" /> doc says it: 0, the bar colour
    /// <c>langbar__part--0</c>.
    /// </summary>
    private const int OtherSlot = 0;

    /// <summary>
    /// The upper edge of the small-share window <see cref="SharePercent" /> says as <c>&lt;1 %</c>: a share above zero
    /// and below this bound is a language that is there and small, not one that is not; zero itself and the bound itself
    /// are said as whole percentages.
    /// </summary>
    private const double SmallShareBound = 1d;

    /// <summary>The format of a count: a whole number with group separators, <c>84,210</c>.</summary>
    private const string CountFormat = "N0";

    /// <summary>
    /// The format of a percent and of a number: at most one decimal, none for a whole number - <c>81.2</c>, <c>80</c>.
    /// <see cref="Percent" /> and <see cref="Number" /> both read it, so they round the same way.
    /// </summary>
    private const string OneDecimalFormat = "0.#";

    /// <summary>The format of the whole percentage <see cref="SharePercent" /> writes after rounding: <c>73</c>.</summary>
    private const string WholeFormat = "0";

    /// <summary>What every percent word ends with: a space and the sign, <c>81.2 %</c>.</summary>
    private const string PercentSign = " %";

    /// <summary>What the page calls itself next to the dashboard's name: the chip of its heading.</summary>
    public const string ThisPage = "this page";

    /// <summary>What the dashboard's own "Code" card is, as the hover text of its section's chip.</summary>
    public const string DashboardHelp =
        "The build that serves this page, as its own site publishes it: the same facts every app of the system reports about its build.";

    /// <summary>Whose code the dashboard's own "Code" card is about: <c>dashboard (this page)</c>.</summary>
    public static string DashboardContext(string name) => $"{name} ({ThisPage})";

    /// <summary>The first <see cref="ShortCommitLength" /> characters of a commit.</summary>
    public static string ShortCommit(string commit)
    {
        ArgumentNullException.ThrowIfNull(commit);
        return commit.Length > ShortCommitLength ? commit[..ShortCommitLength] : commit;
    }

    /// <summary><c>84,210</c>.</summary>
    public static string Count(long number) => number.ToString(CountFormat, CultureInfo.InvariantCulture);

    /// <summary><c>81.2 %</c>.</summary>
    public static string Percent(double percent) => Number(percent) + PercentSign;

    /// <summary><c>4.2</c>.</summary>
    public static string Number(double number) => number.ToString(OneDecimalFormat, CultureInfo.InvariantCulture);

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
            .Select((language, index) => new LanguageShare(language.Name, language.Lines, ShareOf(language.Lines, total), index + FirstNamedSlot))
            .ToList();
        var rest = code.Languages.Skip(NamedLanguages).Sum(language => language.Lines);
        if (rest > 0)
        {
            shares.Add(new LanguageShare(Other, rest, ShareOf(rest, total), OtherSlot));
        }

        return shares;
    }

    /// <summary>
    /// A part's share of the lines of all languages, as the <see cref="LanguageShare.Percent" /> doc says it: the
    /// <see cref="Dashboard.Shares" /> rule of the percent a part holds of a whole - the part's lines over the
    /// whole, then the <see cref="PercentScale" /> - in the one order the dashboard's percents of a part of a whole
    /// are cut.
    /// </summary>
    private static double ShareOf(long lines, long total) => Dashboard.Shares.Of(lines, total);

    /// <summary>The bar in words: <c>C# 73 %, Razor 12 %, Other 15 %</c>.</summary>
    public static string Describe(IEnumerable<LanguageShare> shares) =>
        string.Join(", ", shares.Select(share => $"{share.Name} {SharePercent(share.Percent)}"));

    /// <summary>A share as a whole percentage; a share above zero and below the <see cref="SmallShareBound" /> as <c>&lt;1 %</c>.</summary>
    public static string SharePercent(double percent) =>
        percent is > 0 and < SmallShareBound
            ? "<1" + PercentSign
            : Math.Round(percent).ToString(WholeFormat, CultureInfo.InvariantCulture) + PercentSign;

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
