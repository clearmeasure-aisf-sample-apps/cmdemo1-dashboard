using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// Every container a keyboard user can tab into is announced: the scroll boxes of the tables, the diagram canvases
/// and the events log all carry <c>tabindex="0"</c>, and each of them names itself with a <c>role</c> plus an
/// <c>aria-label</c> or <c>aria-labelledby</c>. A focus stop with no role and no name is silence: the screen reader
/// says nothing about what scrolls there (#105). A guard walks every component and refuses a focusable tag without
/// both, so the next scrolling region added cannot be forgotten.
/// </summary>
public class FocusableRegionGuardTests
{
    private const string FocusableAttribute = "tabindex=\"0\"";

    private static string DashboardSourceDirectory([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard"));

    [Fact]
    public void EveryFocusableContainerHasARoleAndAName()
    {
        var offenders = new List<string>();
        var focusable = 0;

        foreach (var file in RazorFiles(DashboardSourceDirectory()))
        {
            var text = File.ReadAllText(file);

            for (var at = text.IndexOf(FocusableAttribute, StringComparison.Ordinal); at >= 0;
                 at = text.IndexOf(FocusableAttribute, at + FocusableAttribute.Length, StringComparison.Ordinal))
            {
                focusable++;

                // The tag is the whole opening element, from its '<' to the '>' that closes it. It spans lines and
                // carries Razor expressions - `@(_fit ? "runtime__canvas--fit" : null)` - whose own '>' would end a
                // comparison, not the tag, so the search counts parentheses and stops at the first '>' at depth zero.
                var start = text.LastIndexOf('<', at);

                if (start < 0)
                {
                    offenders.Add($"{Path.GetFileName(file)}:1");
                    continue;
                }

                var end = EndOfTag(text, start);
                var tag = end < 0 ? text[start..] : text[start..(end + 1)];

                if (!tag.Contains("role=", StringComparison.Ordinal)
                    || !(tag.Contains("aria-label=", StringComparison.Ordinal)
                         || tag.Contains("aria-labelledby=", StringComparison.Ordinal)))
                {
                    offenders.Add($"{Path.GetFileName(file)}:{text.AsSpan(0, start).Count('\n') + 1}");
                }
            }
        }

        // A guard that found nothing has seen nothing: it refuses the run rather than passing by absence.
        Assert.True(focusable > 0, $"{DashboardSourceDirectory()} holds no {FocusableAttribute}: the guard found no focusable container to check.");
        Assert.Empty(offenders);
    }

    /// <summary>The index of the '>' that closes the tag starting at <paramref name="start"/>, or -1 when the file
    /// ends inside it. Parentheses are counted because a Razor expression may hold one.</summary>
    private static int EndOfTag(string text, int start)
    {
        var depth = 0;

        for (var i = start; i < text.Length; i++)
        {
            switch (text[i])
            {
                case '(':
                    depth++;
                    break;
                case ')' when depth > 0:
                    depth--;
                    break;
                case '>' when depth == 0:
                    return i;
            }
        }

        return -1;
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
