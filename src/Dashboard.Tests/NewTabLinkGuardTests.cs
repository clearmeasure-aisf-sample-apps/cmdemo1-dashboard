using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// A link that opens a new tab has to say so twice over (#134): once for safety, by <c>rel="noopener"</c>, so the
/// page opened cannot reach back into the dashboard; and once for the screen reader, by a visually-hidden
/// "(opens in a new tab)" span as the last thing inside the link, since a <c>title</c> is announced only by some
/// combinations of browser and reader. The guard reads the markup of every <c>.razor</c> file of the app, so the
/// next new-tab link added cannot be forgotten: an anchor written without either fails here instead of reaching a
/// viewer as a link that jumps to another window without a word of warning.
/// </summary>
public class NewTabLinkGuardTests
{
    private const string Anchor = "<a";
    private const string CloseAnchor = "</a>";
    private const string TargetBlank = "target=\"_blank\"";
    private const string Rel = "rel=\"";

    /// <summary>The span the app spells it with: the words of <c>ViewLinks.razor</c>, <c>DeepLink.razor</c> and the
    /// logo of <c>index.html</c> alike, so one wording covers every new-tab link.</summary>
    private const string NewTabHint = "<span class=\"visually-hidden\"> (opens in a new tab)</span>";

    [Fact]
    public void EveryNewTabLinkCarriesNoopenerAndSaysItOpensANewTab()
    {
        var offenders = new List<string>();

        foreach (var link in NewTabLinks())
        {
            var reasons = new List<string>();

            if (!OpensSafely(link.Tag))
            {
                reasons.Add("no rel with noopener");
            }

            if (!link.Content.Contains(NewTabHint, StringComparison.Ordinal))
            {
                reasons.Add("no visually-hidden \"(opens in a new tab)\" span before its " + CloseAnchor);
            }

            if (reasons.Count > 0)
            {
                offenders.Add($"{link.File}:{link.Line} ({string.Join(", ", reasons)})");
            }
        }

        Assert.Empty(offenders);
    }

    /// <summary>
    /// The guard reads the app: the shipped components open about fifteen links in a new tab, so a count far below
    /// that means the markup was not read at all and the test above would pass on nothing.
    /// </summary>
    [Fact]
    public void TheGuardReadsTheNewTabLinksOfTheApp()
    {
        var links = NewTabLinks();

        Assert.True(
            links.Count >= 10,
            $"only {links.Count} link(s) with {TargetBlank} were read under {DashboardRoot()}: the guard reads the wrong place");
    }

    /// <summary>Every <c>&lt;a&gt;</c> of the app that opens a new tab, with its tag, its content up to the
    /// <c>&lt;/a&gt;</c> that closes it, and the file and line the assertion prints.</summary>
    private static List<(string File, int Line, string Tag, string Content)> NewTabLinks()
    {
        var links = new List<(string File, int Line, string Tag, string Content)>();

        foreach (var (file, text) in RazorMarkup())
        {
            for (var at = AnchorAt(text, 0); at >= 0; at = AnchorAt(text, at + Anchor.Length))
            {
                var afterTag = EndOfTag(text, at);
                if (afterTag < 0)
                {
                    continue;
                }

                var tag = text[at..afterTag];
                if (!tag.Contains(TargetBlank, StringComparison.Ordinal))
                {
                    continue;
                }

                var close = EndOfContent(text, afterTag);
                var content = text[afterTag..(close < 0 ? text.Length : close)];

                links.Add((file, LineOf(text, at), tag, content));
            }
        }

        return links;
    }

    /// <summary>True when the tag's <c>rel</c> carries <c>noopener</c> among its tokens.</summary>
    private static bool OpensSafely(string tag)
    {
        var at = tag.IndexOf(Rel, StringComparison.OrdinalIgnoreCase);
        if (at < 0)
        {
            return false;
        }

        var from = at + Rel.Length;
        var end = tag.IndexOf('"', from);

        return end > from
               && tag[from..end]
                   .Split([' '], StringSplitOptions.RemoveEmptyEntries)
                   .Any(token => token.Equals("noopener", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>The next <c>&lt;a&gt;</c> at or after <c>from</c>, or -1 when none follows. The name has to end
    /// there: <c>&lt;article&gt;</c>, <c>&lt;aside&gt;</c> and <c>&lt;animation&gt;</c> are other elements.</summary>
    private static int AnchorAt(string text, int from)
    {
        for (var at = text.IndexOf(Anchor, from, StringComparison.Ordinal);
             at >= 0;
             at = text.IndexOf(Anchor, at + Anchor.Length, StringComparison.Ordinal))
        {
            var next = at + Anchor.Length;

            if (next < text.Length && (char.IsWhiteSpace(text[next]) || text[next] == '>'))
            {
                return at;
            }
        }

        return -1;
    }

    /// <summary>Just past the <c>&gt;</c> that ends the tag opening at <c>start</c>, or -1 when it never ends. The
    /// search ignores <c>&gt;</c> inside an attribute value, as a title carries one.</summary>
    private static int EndOfTag(string text, int start)
    {
        var quoted = false;

        for (var at = start; at < text.Length; at++)
        {
            var c = text[at];

            if (c == '"')
            {
                quoted = !quoted;
            }
            else if (c == '>' && !quoted)
            {
                return at + 1;
            }
        }

        return -1;
    }

    /// <summary>The <c>&lt;/a&gt;</c> that closes the anchor whose tag ends at <c>from</c>, past any anchor nested
    /// in it, or -1 when it is left unclosed.</summary>
    private static int EndOfContent(string text, int from)
    {
        var open = 1;

        for (var at = from; at < text.Length;)
        {
            var close = text.IndexOf(CloseAnchor, at, StringComparison.Ordinal);
            if (close < 0)
            {
                return -1;
            }

            var nested = AnchorAt(text, at);
            if (nested >= 0 && nested < close)
            {
                var afterNested = EndOfTag(text, nested);
                if (afterNested < 0)
                {
                    return -1;
                }

                open++;
                at = afterNested;
                continue;
            }

            open--;
            if (open == 0)
            {
                return close;
            }

            at = close + CloseAnchor.Length;
        }

        return -1;
    }

    private static int LineOf(string text, int at) => text.AsSpan(0, at).Count('\n') + 1;

    private static string DashboardRoot([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard"));

    /// <summary>Every <c>.razor</c> file of the app, as the path the assertion prints and its text. The build's
    /// copies under <c>bin</c> and <c>obj</c> hold the same markup twice, so those directories are walked past.</summary>
    private static IEnumerable<(string File, string Text)> RazorMarkup()
    {
        var root = DashboardRoot();

        foreach (var path in Directory.EnumerateFiles(root, "*.razor", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
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
