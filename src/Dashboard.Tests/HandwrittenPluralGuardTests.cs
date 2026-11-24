using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// The singular/plural rule lives in one place: <see cref="Dashboard.Health.Plural" />. A <c>+ "s"</c> concatenation
/// or a <c>X == 1 ? "word" : "words"</c> ternary anywhere else in the app is the rule written a second time, and
/// the second time it is written it can drift - one noun gets the <c>s</c>, the next is spelled out, the next is
/// forgotten. This guard walks every <c>.cs</c> under <c>src/Dashboard</c> and refuses any hand-written plural
/// outside <c>Plural.cs</c>, the way <see cref="InlineStyleGuardTests" /> refuses inline styles outside the
/// allowlist and <see cref="ServingAssessmentFileLayoutTests" /> refuses types in the wrong file.
/// <para>
/// The rule itself is said in <c>Plural.cs</c>: a count of exactly one takes the singular, every other count
/// takes the given plural or, with none given, the singular with an <c>s</c>. No caller needs to write that
/// again.
/// </para>
/// </summary>
public class HandwrittenPluralGuardTests
{
    /// <summary>The file that owns the rule, as the walk prints it.</summary>
    private const string PluralFile = "Health/Plural.cs";

    /// <summary>
    /// A <c>+ "s"</c> concatenation that looks like a default plural. The <c>s</c> stands alone in its own
    /// quoted string. The regex is deliberately loose on the left side - any expression followed by <c>+ "s"</c>
    /// is suspicious - but it does not match <c>+ "something else"</c> or <c>+ "ss"</c>.
    /// </summary>
    private static readonly Regex PlusES = new(@"\+\s*""s""(?!\w)", RegexOptions.Compiled);

    /// <summary>
    /// A <c>X == 1 ? "word" : "words"</c> ternary that looks like a hand-written noun plural: the count is
    /// compared to one, the true branch is a quoted noun, and the false branch is the same noun with an
    /// <c>s</c> on it (or a different plural spelling). The regex captures the true-branch noun so the test
    /// can check whether the false branch is its <c>s</c> plural.
    /// </summary>
    private static readonly Regex PluralTernary = new(@"==\s*1\s*\?\s*""([^""]+)""\s*:\s*""([^""]+)""", RegexOptions.Compiled);

    /// <summary>
    /// Every <c>.cs</c> under <c>src/Dashboard</c> is read once, as the path the assertion prints and its
    /// text. The build's copies under <c>bin</c> and <c>obj</c> are walked past.
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
    /// No <c>+ "s"</c> default plural stands outside <c>Plural.cs</c>: the rule is said once, in the one
    /// member that owns it, and every other count-and-noun sentence comes through that member.
    /// </summary>
    [Fact]
    public void NoPlusSDefaultPluralOutsidePluralCs()
    {
        var offenders = new List<string>();

        foreach (var (file, text) in SourceFiles())
        {
            if (file == PluralFile)
            {
                continue;
            }

            var lines = text.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                if (PlusES.IsMatch(lines[i]))
                {
                    offenders.Add($"{file}:{i + 1}: hand-written plural \"s\" added with concatenation; use Dashboard.Health.Plural");
                }
            }
        }

        Assert.Empty(offenders);
    }

    /// <summary>
    /// No <c>X == 1 ? "word" : "words"</c> noun ternary stands outside <c>Plural.cs</c>: the singular/plural
    /// choice is the rule itself, and it is said once, in the one member that owns it. A ternary that chooses
    /// between two different words - <c>"is" / "are"</c>, <c>"its" / "their"</c> - is verb or pronoun
    /// agreement, not a noun plural, and is not this guard's fault line.
    /// </summary>
    [Fact]
    public void NoHandwrittenNounPluralTernaryOutsidePluralCs()
    {
        var offenders = new List<string>();

        foreach (var (file, text) in SourceFiles())
        {
            if (file == PluralFile)
            {
                continue;
            }

            var lines = text.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                foreach (Match match in PluralTernary.Matches(lines[i]))
                {
                    var singular = match.Groups[1].Value;
                    var plural = match.Groups[2].Value;

                    // The false branch is the singular with an "s" added, or a different plural spelling.
                    // Either way, the ternary is making the singular/plural choice itself.
                    if (plural == singular + "s" || LooksLikePluralOf(singular, plural))
                    {
                        offenders.Add($"{file}:{i + 1}: hand-written noun plural ternary; use Dashboard.Health.Plural");
                    }
                }
            }
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
    /// The <c>+ "s"</c> regex catches the default plural and nothing else: a bare <c>+ "s"</c> is a plural, but
    /// <c>+ "something"</c>, <c>+ "ss"</c>, and the <c>s</c> inside another string are not.
    /// </summary>
    [Fact]
    public void ThePlusESRegexIsPrecise()
    {
        Assert.Matches(PlusES, "var word = singular + \"s\";");
        Assert.Matches(PlusES, "var word = singular + \"s\"");
        Assert.Matches(PlusES, "return base + \"s\";");
        Assert.DoesNotMatch(PlusES, "var word = singular + \"something\";");
        Assert.DoesNotMatch(PlusES, "var word = singular + \"ss\";");
        Assert.DoesNotMatch(PlusES, "var word = singular + \"\";");
        // The 's' inside another string is not a + "s" plural
        Assert.DoesNotMatch(PlusES, "var msg = \"Items: \" + count;");
    }

    /// <summary>
    /// The ternary regex catches noun plurals and not verb agreement: <c>"word" / "words"</c> is a noun plural,
    /// but <c>"is" / "are"</c> and <c>"its" / "their"</c> are verb and pronoun agreement.
    /// </summary>
    [Fact]
    public void ThePluralTernaryRegexDiscriminatesNounFromVerb()
    {
        // Noun plural: singular + "s"
        Assert.True(IsNounPluralTernary("var n = count == 1 ? \"check\" : \"checks\";"));
        // Noun plural: different plural spelling
        Assert.True(IsNounPluralTernary("var n = count == 1 ? \"box\" : \"boxes\";"));
        // Verb agreement: not a noun plural
        Assert.False(IsNounPluralTernary("var v = count == 1 ? \"is\" : \"are\";"));
        // Pronoun agreement: not a noun plural
        Assert.False(IsNounPluralTernary("var p = count == 1 ? \"its\" : \"their\";"));
    }

    private static bool IsNounPluralTernary(string line)
    {
        foreach (Match match in PluralTernary.Matches(line))
        {
            var singular = match.Groups[1].Value;
            var plural = match.Groups[2].Value;
            if (plural == singular + "s" || LooksLikePluralOf(singular, plural))
            {
                return true;
            }
        }
        return false;
    }

    private static bool LooksLikePluralOf(string singular, string plural)
    {
        // Different plural spelling: "box" -> "boxes", "child" -> "children", etc.
        // Heuristic: the plural is longer and starts with the singular.
        return plural.Length > singular.Length && plural.StartsWith(singular, StringComparison.Ordinal);
    }

    private static string DashboardRoot() => MarkupGuards.DashboardRoot();
}