using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// An inline &lt;svg&gt; says nothing on its own, so every one of them either steps out of the way of a screen reader
/// (aria-hidden="true" focusable="false", because the words stand next to it) or says what it shows (role="img" with
/// aria-label or aria-labelledby) (#108). A guard walks every component and refuses an icon that does neither, so the
/// next one added cannot be forgotten; the tag is read to its own ">", past the Razor expressions and line breaks it
/// carries.
/// </summary>
public class SvgIconGuardTests
{
    private const string SvgTagStart = "<svg";

    private static string DashboardSourceDirectory([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard"));

    [Fact]
    public void EveryInlineSvgIsHiddenOrNamed()
    {
        var offenders = new List<string>();
        var found = 0;

        foreach (var file in RazorFiles(DashboardSourceDirectory()))
        {
            var text = File.ReadAllText(file);

            for (var start = text.IndexOf(SvgTagStart, StringComparison.Ordinal); start >= 0;
                 start = text.IndexOf(SvgTagStart, start + SvgTagStart.Length, StringComparison.Ordinal))
            {
                var after = start + SvgTagStart.Length;

                // "<svg" only opens a tag when the name ends there: "<svgfoo" is another element altogether.
                if (after >= text.Length || !(char.IsWhiteSpace(text[after]) || text[after] == '>'))
                {
                    continue;
                }

                found++;

                var tag = TagOf(text, start);

                var hidden = tag.Contains("aria-hidden=\"true\"", StringComparison.Ordinal)
                             && tag.Contains("focusable=\"false\"", StringComparison.Ordinal);
                var named = tag.Contains("role=\"img\"", StringComparison.Ordinal)
                            && (tag.Contains("aria-label=", StringComparison.Ordinal)
                                || tag.Contains("aria-labelledby=", StringComparison.Ordinal));

                if (!hidden && !named)
                {
                    var line = text.AsSpan(0, start).Count('\n') + 1;
                    offenders.Add($"{Path.GetFileName(file)}:{line}");
                }
            }
        }

        Assert.True(found > 0, $"No inline <svg> was read under {DashboardSourceDirectory()}: the guard found nothing to guard.");
        Assert.Empty(offenders);
    }

    /// <summary>
    /// The tag itself, from its "&lt;svg" to the first "&gt;" that is not inside parentheses: a tag spans lines and
    /// carries Razor, as in `@VersionLine.Level(State)`, whose parentheses close before the tag does.
    /// </summary>
    private static string TagOf(string text, int start)
    {
        var depth = 0;

        for (var at = start; at < text.Length; at++)
        {
            var c = text[at];

            if (c == '(')
            {
                depth++;
            }
            else if (c == ')')
            {
                depth = Math.Max(0, depth - 1);
            }
            else if (c == '>' && depth == 0)
            {
                return text[start..(at + 1)];
            }
        }

        return text[start..];
    }

    /// <summary>Every component of the app, the generated files of a build left out.</summary>
    private static IEnumerable<string> RazorFiles(string root)
    {
        var skipped = new[] { Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar,
                              Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar };

        return Directory.EnumerateFiles(root, "*.razor", SearchOption.AllDirectories)
            .Where(file => !skipped.Any(where => file.Contains(where, StringComparison.Ordinal)));
    }
}
