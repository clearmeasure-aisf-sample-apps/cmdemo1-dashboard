using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// The singular/plural rule lives in one place: <see cref="Dashboard.Health.Plural" />. A <c>+ "s"</c> concatenation,
/// an interpolated <c>"{noun}s"</c> suffix, or a <c>n == 1 ? "word" : "words"</c> ternary anywhere else in the app
/// is the rule written a second time, and the second time it is written it can drift - one noun gets the <c>s</c>,
/// the next is spelled out, the next is forgotten. This guard walks every <c>.cs</c> under <c>src/Dashboard</c>
/// and refuses any hand-written plural outside <c>Plural.cs</c>, the way <see cref="InlineStyleGuardTests" /> refuses
/// inline styles outside the allowlist and <see cref="ServingAssessmentFileLayoutTests" /> refuses types in the
/// wrong file.
/// <para>
/// The rule itself is said in <c>Plural.cs</c>: a count of exactly one takes the singular, every other count takes
/// the given plural or, with none given, the singular with an <c>s</c>. No caller needs to write that again.
/// </para>
/// </summary>
public class HandwrittenPluralGuardTests
{
    /// <summary>The file that owns the rule, as the walk prints it.</summary>
    private const string PluralFile = "Health/Plural.cs";

    /// <summary>
    /// A <c>+ "s"</c> or <c>+ "es"</c> concatenation that looks like a default plural. The suffix stands alone in
    /// its own quoted string.
    /// </summary>
    private static readonly Regex PlusSuffix = new(@"\+\s*""(s|es)""(?!\w)", RegexOptions.Compiled);

    /// <summary>
    /// An interpolated string where a hole is immediately followed by a plural suffix and the closing quote,
    /// e.g. <c>$"{noun}s"</c> or <c>$"The {name}s"</c>.
    /// </summary>
    private static readonly Regex InterpolatedSuffix = new(@"""[^""]*\{[^}]*\}(s|es)""", RegexOptions.Compiled);

    /// <summary>
    /// A ternary that compares a variable to 1 and chooses between string branches.
    /// The <c>(?s)</c> flag lets the match span lines. Branches can be plain, interpolated, or verbatim strings.
    /// </summary>
    private static readonly Regex PluralTernary = new(
        "(?s)(\\w+)\\s*(==|!=|>|<|>=|<=)\\s*1\\s*\\?\\s*(\\$?@?\")([^\"]*)\"\\s*:\\s*(\\$?@?\")([^\"]*)\"",
        RegexOptions.Compiled);

    /// <summary>
    /// The reversed form where 1 is on the left: <c>1 == count ? "word" : "words"</c>.
    /// </summary>
    private static readonly Regex ReversedPluralTernary = new(
        "(?s)1\\s*(==|!=|>|<|>=|<=)\\s*(\\w+)\\s*\\?\\s*(\\$?@?\")([^\"]*)\"\\s*:\\s*(\\$?@?\")([^\"]*)\"",
        RegexOptions.Compiled);

    /// <summary>
    /// Known verb/pronoun agreement pairs that are NOT noun plurals. These are allowed
    /// in ternaries that compare counts to 1.
    /// </summary>
    private static readonly HashSet<string> VerbPronounPairs = new(StringComparer.Ordinal)
    {
        "is/are",
        "are/is",
        "was/were",
        "were/was",
        "has/have",
        "have/has",
        "it/they",
        "they/it",
        "its/their",
        "their/its",
        "this/these",
        "these/this",
        "that/those",
        "those/that",
        " is/ are",
        " are/ is",
        " was/ were",
        " were/ was",
        " has/ have",
        " have/ has",
        " it/ they",
        " they/ it",
        " its/ their",
        " their/ its",
        " this/ these",
        " these/ this",
        " that/ those",
        " those/ that",
        "its answer/their answers",
        "their answers/its answer",
    };

    /// <summary>
    /// Every <c>.cs</c> under <c>src/Dashboard</c> is read once, as the path the assertion prints and its text.
    /// The build's copies under <c>bin</c> and <c>obj</c> are walked past.
    /// </summary>
    private static IEnumerable<(string File, string Text)> SourceFiles()
    {
        var root = DashboardRoot();

        foreach (var path in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
        {
            var relative = Path.GetRelativePath(root, path);
            var normalized = relative.Replace('\\', '/');

            if (normalized.Split('/').Any(part =>
                    part.Equals("bin", StringComparison.OrdinalIgnoreCase) || part.Equals("obj", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            yield return (normalized, File.ReadAllText(path));
        }
    }

    /// <summary>
    /// Scans a single source file (given by name and text) for hand-written plural patterns and returns
    /// findings as <c>"file:line: message"</c> strings. This is the core guard logic, factored out so
    /// it can be tested directly with in-memory source as well as used for the production walk.
    /// </summary>
    private static List<string> ScanFile(string fileName, string sourceText)
    {
        var findings = new List<string>();

        // Check for + "s" / + "es" concatenation
        foreach (Match match in PlusSuffix.Matches(sourceText))
        {
            var line = LineNumberAt(sourceText, match.Index);
            findings.Add($"{fileName}:{line}: hand-written plural suffix added with concatenation; use Dashboard.Health.Plural");
        }

        // Check for interpolated suffixes like $"{noun}s"
        foreach (Match match in InterpolatedSuffix.Matches(sourceText))
        {
            var line = LineNumberAt(sourceText, match.Index);
            findings.Add($"{fileName}:{line}: hand-written plural via interpolated suffix; use Dashboard.Health.Plural");
        }

        // Check for noun-plural ternaries: count == 1 ? "word" : "words"
        foreach (Match match in PluralTernary.Matches(sourceText))
        {
            if (IsHandwrittenPluralTernary(match))
            {
                var line = LineNumberAt(sourceText, match.Index);
                findings.Add($"{fileName}:{line}: hand-written noun plural ternary; use Dashboard.Health.Plural");
            }
        }

        // Check for reversed form: 1 == count ? "word" : "words"
        foreach (Match match in ReversedPluralTernary.Matches(sourceText))
        {
            if (IsHandwrittenPluralTernary(match))
            {
                var line = LineNumberAt(sourceText, match.Index);
                findings.Add($"{fileName}:{line}: hand-written noun plural ternary; use Dashboard.Health.Plural");
            }
        }

        return findings;
    }

    /// <summary>
    /// Determines if a matched ternary is a hand-written noun plural (vs verb/pronoun agreement).
    /// Flags ternaries comparing a count to 1 with two differing short, space-free string branches,
    /// unless the pair is a known verb/pronoun agreement pair. Longer or multi-word branches are
    /// treated as whole-sentence choices, not simple noun plurals.
    /// </summary>
    private static bool IsHandwrittenPluralTernary(Match match)
    {
        // Branch content is in groups 4 and 6
        var branch1 = match.Groups[4].Value;
        var branch2 = match.Groups[6].Value;

        // Same text in both branches is not a plural
        if (branch1 == branch2) return false;

        // Check for known verb/pronoun agreement pairs
        var pair = $"{branch1}/{branch2}";
        if (VerbPronounPairs.Contains(pair)) return false;

        // Long branches are whole-sentence choices, not simple noun plurals
        if (branch1.Length > 15 || branch2.Length > 15) return false;

        return true;
    }

    /// <summary>
    /// Strips string delimiters (" or @ or $) to get the raw content for comparison.
    /// </summary>
    private static string StripStringDelimiters(string s)
    {
        s = s.Trim();
        // Remove opening delimiter: optionally $ or @, then "
        if (s.StartsWith("$@", StringComparison.Ordinal) || s.StartsWith("@$", StringComparison.Ordinal))
        {
            s = s.Substring(2);
        }
        else if (s.StartsWith('$') || s.StartsWith('"'))
        {
            s = s.Substring(1);
        }
        // Remove closing quote if present
        if (s.EndsWith('"'))
        {
            s = s.Substring(0, s.Length - 1);
        }
        return s;
    }

    private static int LineNumberAt(string text, int position)
    {
        var line = 1;
        for (var i = 0; i < position; i++)
        {
            if (text[i] == '\n')
            {
                line++;
            }
        }
        return line;
    }

    /// <summary>
    /// No <c>+ "s"</c> default plural stands outside <c>Plural.cs</c>: the rule is said once, in the one
    /// member that owns it, and every other count-and-noun sentence comes through that member.
    /// </summary>
    [Fact]
    public void NoPlusSDefaultPluralOutsidePluralCs()
    {
        var offenders = new List<string>();

        foreach (var (file, text) in SourceFiles())
        {
            if (file == PluralFile) continue;

            var findings = ScanFile(file, text);
            offenders.AddRange(findings);
        }

        Assert.Empty(offenders);
    }

    /// <summary>
    /// A guard that found nothing has seen nothing: the walk must reach every production <c>.cs</c> of the app,
    /// including the file it is meant to trust (<c>Plural.cs</c>). If the walk is short, the clean verdicts above
    /// mean nothing.
    /// </summary>
    [Fact]
    public void TheGuardWalksEverySourceFile()
    {
        var files = SourceFiles().Select(f => f.File).ToList();

        Assert.True(files.Count > 50, $"only {files.Count} .cs files found under {DashboardRoot()}: the walk is not reaching the app");
        Assert.Contains(PluralFile, files);
    }

    /// <summary>
    /// The <c>+ "s"</c> regex catches the default plural and nothing else: a bare <c>+ "s"</c> or <c>+ "es"</c>
    /// is a plural, but <c>+ "something"</c>, <c>+ "ss"</c>, and the <c>s</c> inside another string are not.
    /// </summary>
    [Fact]
    public void ThePlusSuffixRegexIsPrecise()
    {
        // Should match
        Assert.Matches(PlusSuffix, "var word = singular + \"s\";");
        Assert.Matches(PlusSuffix, "var word = singular + \"es\";");
        Assert.Matches(PlusSuffix, "return base + \"s\";");
        Assert.Matches(PlusSuffix, "return base + \"es\";");

        // Should not match
        Assert.DoesNotMatch(PlusSuffix, "var word = singular + \"something\";");
        Assert.DoesNotMatch(PlusSuffix, "var word = singular + \"ss\";");
        Assert.DoesNotMatch(PlusSuffix, "var word = singular + \"\";");
        Assert.DoesNotMatch(PlusSuffix, "var msg = \"Items: \" + count;");
    }

    /// <summary>
    /// The interpolated-suffix regex catches <c>$"{noun}s"</c> but not ordinary interpolations.
    /// </summary>
    [Fact]
    public void TheInterpolatedSuffixRegexIsPrecise()
    {
        // Should match
        Assert.Matches(InterpolatedSuffix, "\"{noun}s\"");
        Assert.Matches(InterpolatedSuffix, "\"The {name}s\"");
        Assert.Matches(InterpolatedSuffix, "\"{box}es\"");

        // Should not match
        Assert.DoesNotMatch(InterpolatedSuffix, "\"{noun}\"");
        Assert.DoesNotMatch(InterpolatedSuffix, "\"{name}'s\"");
        Assert.DoesNotMatch(InterpolatedSuffix, "\"{noun} is here\"");
    }

    /// <summary>
    /// The ternary rule catches noun plurals in all comparison forms and string styles,
    /// and lets through verb/pronoun agreement and non-1 comparisons.
    /// </summary>
    [Fact]
    public void ThePluralTernaryRuleDiscriminatesNounFromVerb()
    {
        // Noun plural: singular + "s"
        Assert.True(IsPluralTernaryIn("var n = count == 1 ? \"check\" : \"checks\";"));
        // Noun plural: different plural spelling (y -> ies)
        Assert.True(IsPluralTernaryIn("var n = count == 1 ? \"assembly\" : \"assemblies\";"));
        Assert.True(IsPluralTernaryIn("var n = count == 1 ? \"entry\" : \"entries\";"));
        // Multi-word noun plural
        Assert.True(IsPluralTernaryIn("var n = count == 1 ? \"work item\" : \"work items\";"));
        // Interpolated string branches
        Assert.True(IsPluralTernaryIn("var n = count == 1 ? $\"check\" : $\"checks\";"));
        // Verbatim string branches
        Assert.True(IsPluralTernaryIn("var n = count == 1 ? @\"check\" : @\"checks\";"));
        // != comparison, plural in first branch
        Assert.True(IsPluralTernaryIn("var n = count != 1 ? \"people\" : \"person\";"));
        // Suffix form: empty true branch
        Assert.True(IsPluralTernaryIn("var n = count == 1 ? \"\" : \"s\";"));
        // Suffix form: empty false branch with !=
        Assert.True(IsPluralTernaryIn("var n = count != 1 ? \"s\" : \"\";"));
        // Using > comparison
        Assert.True(IsPluralTernaryIn("var n = count > 1 ? \"checks\" : \"check\";"));
        // Reversed form
        Assert.True(IsPluralTernaryIn("var n = 1 == count ? \"check\" : \"checks\";"));

        // Verb agreement: not a noun plural
        Assert.False(IsPluralTernaryIn("var v = count == 1 ? \"is\" : \"are\";"));
        Assert.False(IsPluralTernaryIn("var v = count == 1 ? \"was\" : \"were\";"));
        Assert.False(IsPluralTernaryIn("var v = count == 1 ? \"has\" : \"have\";"));
        // Pronoun agreement: not a noun plural
        Assert.False(IsPluralTernaryIn("var p = count == 1 ? \"it\" : \"they\";"));
        Assert.False(IsPluralTernaryIn("var p = count == 1 ? \"its\" : \"their\";"));
        Assert.False(IsPluralTernaryIn("var p = count == 1 ? \"this\" : \"these\";"));
        Assert.False(IsPluralTernaryIn("var p = count == 1 ? \"that\" : \"those\";"));
        // Same text in both branches
        Assert.False(IsPluralTernaryIn("var s = count == 1 ? \"x\" : \"x\";"));
        // Non-1 comparisons
        Assert.False(IsPluralTernaryIn("var o = ok ? \"yes\" : \"no\";"));
        Assert.False(IsPluralTernaryIn("var t = count == 2 ? \"a\" : \"b\";"));
        Assert.False(IsPluralTernaryIn("var t = count == 10 ? \"a\" : \"b\";"));
        Assert.False(IsPluralTernaryIn("var t = count == 11 ? \"a\" : \"b\";"));
    }

    private static bool IsPluralTernaryIn(string code)
    {
        var findings = ScanFile("test.cs", code);
        return findings.Any(f => f.Contains("ternary"));
    }

    /// <summary>
    /// A planted offender in production code is detected with file and line in the failure message.
    /// </summary>
    [Fact]
    public void PlantedOffenderIsDetectedWithFileAndLine()
    {
        var source = """
            using System;

            public class Test
            {
                public string Format(int n)
                {
                    return n + " item" + (n == 1 ? "" : "s");
                }
            }
            """;

        var findings = ScanFile("Test.cs", source);
        Assert.Single(findings);
        Assert.Contains("Test.cs:7", findings[0]);
    }

    /// <summary>
    /// A multi-line ternary offender is detected and reports the line where the ternary/condition starts.
    /// </summary>
    [Fact]
    public void MultiLinePlantedOffenderReportsStartLine()
    {
        var source = """
            using System;

            public class Test
            {
                public string Format(int n)
                {
                    var w = n == 1
                        ? "check"
                        : "checks";
                    return n + " " + w;
                }
            }
            """;

        var findings = ScanFile("Test.cs", source);
        Assert.Single(findings);
        Assert.Contains("Test.cs:7", findings[0]);
    }

    private static string DashboardRoot() => MarkupGuards.DashboardRoot();
}
