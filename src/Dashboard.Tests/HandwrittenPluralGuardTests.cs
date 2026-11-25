using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// The singular/plural rule of the repository lives in one production member, <c>Plural.NounOf</c>, asked through
/// <c>Plural.Noun</c> and <c>Plural.Of</c> (#999, #1677, #1678). This guard keeps it there (#1597): it walks every
/// <c>.cs</c> file under <c>src/Dashboard</c> and refuses a hand-written plural anywhere but <c>Plural.cs</c>.
/// <para>
/// A hand-written plural is any of: a ternary whose condition compares a value with a bare literal <c>1</c>
/// (<c>n == 1</c>, <c>n != 1</c>, <c>n &gt; 1</c>, <c>1 &lt; n</c>, <c>n is 1</c>, parenthesised or not) or the
/// boundary <c>n &gt;= 2</c> / <c>n &lt; 2</c>, whose two branches are string literals of any kind (plain,
/// interpolated, verbatim) that differ; a default plural suffix appended with <c>+ "s"</c> or <c>+ "es"</c>; or an
/// interpolation hole followed by an <c>s</c>/<c>es</c> suffix (<c>$"{noun}s"</c>). The whole file text is read at once, so
/// a ternary split over lines is caught, and the report gives the file and the line on which the condition starts.
/// </para>
/// <para>
/// There is no allowlist: master passes with none, and the one exempt file is <c>Plural.cs</c>, the rule's home.
/// A named constant (<c>Priority == FirstPriority</c>) is not a comparison with one; only the bare literal is.
/// </para>
/// </summary>
public class HandwrittenPluralGuardTests
{
    /// <summary>The one file allowed to write the singular/plural choice: the home of the rule itself.</summary>
    private static readonly string PluralHome = Path.Combine("Health", "Plural.cs");

    /// <summary>A C# string literal of any prefix: verbatim (<c>@"..."</c>, <c>$@"..."</c>, <c>@$"..."</c>) or
    /// regular (<c>"..."</c>, <c>$"..."</c>).</summary>
    private const string StringLiteral =
        @"(?:(?:\$@|@\$|@)""(?:[^""]|"""")*""|\$?""(?:[^""\\\r\n]|\\.)*"")";

    /// <summary>An interpolated literal, regular or verbatim, whose holes may hide a plural suffix.</summary>
    private const string InterpolatedLiteral =
        @"(?:(?:\$@|@\$)""(?:[^""]|"""")*""|\$""(?:[^""\\\r\n]|\\.)*"")";

    /// <summary>A bare literal, not the start or the end of a longer number or name.</summary>
    private static string Bare(char digit) => $@"(?<![\w.]){digit}(?![\w.])";

    /// <summary>
    /// A condition that asks "is it one?": any comparison with a bare <c>1</c>, from either side or as a pattern,
    /// and the boundary of "two or more" written against a bare <c>2</c>. <c>n == 2</c> asks about two, not about
    /// the plural, and is not counted.
    /// </summary>
    private static readonly string AgainstOne = string.Join('|',
        $@"[\w\])]\s*(?:==|!=|>=|<=|>|<)\s*{Bare('1')}",
        $@"{Bare('1')}\s*(?:==|!=|>=|<=|>|<)\s*[\w(]",
        $@"\bis\s+(?:not\s+)?{Bare('1')}",
        $@"[\w\])]\s*(?:>=|<)\s*{Bare('2')}",
        $@"{Bare('2')}\s*(?:<=|>)\s*[\w(]");

    private static readonly Regex CountTernary = new(
        $@"(?<condition>{AgainstOne})\s*\)*\s*\?\s*(?<one>{StringLiteral})\s*:\s*(?<other>{StringLiteral})",
        RegexOptions.Compiled);

    private static readonly Regex AppendedSuffix = new(
        @"\+\s*(?:\$@|@\$|\$|@)?""e?s""", RegexOptions.Compiled);

    private static readonly Regex Interpolated = new(InterpolatedLiteral, RegexOptions.Compiled);

    private static readonly Regex SuffixAfterHole = new(@"\}e?s(?![\w{])", RegexOptions.Compiled);

    /// <summary>One hand-written plural: the file as the walk prints it, the line it starts on, and what it is.</summary>
    public sealed record Finding(string File, int Line, string What)
    {
        /// <summary>Where the fault stands and what it wrote.</summary>
        public string Report => $"{File}:{Line} {What}";
    }

    /// <summary>
    /// Every hand-written plural of one source text, in the order it writes them. Pure, so the tests below can plant
    /// offenders without a file on disk; the exemption of <c>Plural.cs</c> belongs to the walk, not to the scan.
    /// </summary>
    public static IReadOnlyList<Finding> Scan(string fileName, string sourceText)
    {
        var findings = new List<Finding>();

        foreach (Match ternary in CountTernary.Matches(sourceText))
        {
            if (ternary.Groups["one"].Value != ternary.Groups["other"].Value)
            {
                findings.Add(new Finding(fileName, LineOf(sourceText, ternary.Groups["condition"].Index),
                    $"count-against-1 ternary choosing {ternary.Groups["one"].Value} or {ternary.Groups["other"].Value}"));
            }
        }

        foreach (Match suffix in AppendedSuffix.Matches(sourceText))
        {
            findings.Add(new Finding(fileName, LineOf(sourceText, suffix.Index), $"default plural appended: {suffix.Value}"));
        }

        foreach (Match literal in Interpolated.Matches(sourceText))
        {
            foreach (Match hole in SuffixAfterHole.Matches(literal.Value))
            {
                findings.Add(new Finding(fileName, LineOf(sourceText, literal.Index + hole.Index),
                    $"plural suffix after an interpolation hole: {literal.Value}"));
            }
        }

        return findings.OrderBy(finding => finding.Line).ToList();
    }

    [Fact]
    public void NoProductionFileButPluralWritesItsOwnPlural()
    {
        var offenders = SourceFiles()
            .Where(file => file.File != PluralHome)
            .SelectMany(file => Scan(file.File, file.Text))
            .Select(finding => $"{finding.Report}: ask Plural.Noun or Plural.Of instead")
            .ToList();

        Assert.Empty(offenders);
    }

    [Fact]
    public void TheWalkReachesEveryCsFileOfTheAppIncludingPlural()
    {
        var root = MarkupGuards.DashboardRoot();
        var expected = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(root, path))
            .Where(relative => !relative.StartsWith("bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                && !relative.StartsWith("obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToList();

        var walked = SourceFiles().Select(file => file.File).ToList();

        Assert.Equal(expected, walked);
        Assert.Contains(PluralHome, walked);
        Assert.Contains(Path.Combine("Health", "HistoryText.cs"), walked);
    }

    [Fact]
    public void ThePluralHomeIsScannedAndWouldBeRefusedWereItNotExempt()
    {
        var plural = SourceFiles().Single(file => file.File == PluralHome);

        Assert.Contains(Scan(plural.File, plural.Text), finding => finding.What.Contains("+ \"s\"", StringComparison.Ordinal));
    }

    [Fact]
    public void ASingleLineTernaryIsReportedByFileAndLine()
    {
        const string source = "class C\n{\n    string W(int n) => n == 1 ? \"check\" : \"checks\";\n}\n";

        var finding = Assert.Single(Scan("Health/Planted.cs", source));

        Assert.StartsWith("Health/Planted.cs:3 ", finding.Report, StringComparison.Ordinal);
    }

    [Fact]
    public void AMultiLineTernaryIsReportedOnTheLineItsConditionStarts()
    {
        var source = string.Join('\n',
            "namespace N;",
            "",
            "class C",
            "{",
            "    string W(int n)",
            "    {",
            "        var w = n == 1",
            "            ? \"check\"",
            "            : \"checks\";",
            "        return w;",
            "    }",
            "}");

        var finding = Assert.Single(Scan("Planted.cs", source));

        Assert.StartsWith("Planted.cs:7 ", finding.Report, StringComparison.Ordinal);
    }

    public static TheoryData<string> PlantedOffenders() => new()
    {
        "var w = n == 1 ? \"check\" : \"checks\";",
        "var w = n != 1 ? \"checks\" : \"check\";",
        "var w = n > 1 ? \"checks\" : \"check\";",
        "var w = n >= 2 ? \"checks\" : \"check\";",
        "var w = n < 2 ? \"check\" : \"checks\";",
        "var w = 1 == n ? \"check\" : \"checks\";",
        "var w = 1 != n ? \"checks\" : \"check\";",
        "var w = 1 < n ? \"checks\" : \"check\";",
        "var w = items.Count==1?\"check\":\"checks\";",
        "var w = (n == 1) ? \"check\" : \"checks\";",
        "var w = n is 1 ? \"check\" : \"checks\";",
        "var w = n == 1 ? \"assembly\" : \"assemblies\";",
        "var w = n == 1 ? \"work item\" : \"work items\";",
        "var w = n == 1 ? \"dependency check\" : \"dependency checks\";",
        "var w = n == 1 ? $\"{count} work item\" : $\"{count} work items\";",
        "var w = n == 1 ? $\"check\" : $\"checks\";",
        "var w = n == 1 ? @\"check\" : @\"checks\";",
        "var w = n == 1 ? $@\"check\" : @$\"checks\";",
        "var w = n == 1 ? \"\" : \"s\";",
        "var w = n != 1 ? \"s\" : \"\";",
        "var w = n == 1 ? \"is\" : \"are\";",
        "var w = noun + \"s\";",
        "var w = noun + \"es\";",
        "var w = $\"{noun}s\";",
        "var w = $\"{count} {noun}s ago\";",
        "var w = $\"{noun}es\";",
    };

    [Theory]
    [MemberData(nameof(PlantedOffenders))]
    public void APlantedHandwrittenPluralIsRefusedWithFileAndLine(string line)
    {
        var source = "class C\n{\n    void M(int n, int count, string noun, System.Collections.Generic.List<int> items)\n    {\n        "
            + line + "\n    }\n}\n";

        var finding = Assert.Single(Scan("Work/Planted.cs", source));

        Assert.StartsWith("Work/Planted.cs:5 ", finding.Report, StringComparison.Ordinal);
    }

    public static TheoryData<string> InnocentLines() => new()
    {
        "var w = ok ? \"yes\" : \"no\";",
        "var w = n == 2 ? \"a\" : \"b\";",
        "var w = n == 10 ? \"a\" : \"b\";",
        "var w = n == 11 ? \"a\" : \"b\";",
        "var w = n == 1.5 ? \"a\" : \"b\";",
        "var w = n == 1 ? \"x\" : \"x\";",
        "var w = edge.Priority == FirstPriority ? \"a\" : \"b\";",
        "var w = Plural.Noun(n, \"check\");",
        "var w = Plural.Of(n, \"assembly\", \"assemblies\");",
        "var w = n == 1 ? \"Last\" : string.Create(CultureInfo.InvariantCulture, $\"Last {n}\");",
        "var w = \"s\";",
        "var w = $\"{n} items\";",
        "var w = $\"{n}seconds\";",
    };

    [Theory]
    [MemberData(nameof(InnocentLines))]
    public void WhatIsNotAHandwrittenPluralIsNotRefused(string line)
    {
        var source = "class C\n{\n    void M()\n    {\n        " + line + "\n    }\n}\n";

        Assert.Empty(Scan("Planted.cs", source));
    }

    private static int LineOf(string text, int at) => text.AsSpan(0, at).Count('\n') + 1;

    /// <summary>Every <c>.cs</c> file of the app, as the path the report prints and its text, in ordinal order. The
    /// build's <c>bin</c> and <c>obj</c> directories hold generated copies, so the walk passes them by.</summary>
    private static IEnumerable<(string File, string Text)> SourceFiles()
    {
        var root = MarkupGuards.DashboardRoot();

        foreach (var path in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
        {
            var relative = Path.GetRelativePath(root, path);

            if (relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(part =>
                    part.Equals("bin", StringComparison.OrdinalIgnoreCase) || part.Equals("obj", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            yield return (relative, File.ReadAllText(path));
        }
    }
}
