using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// Keeps the tables accessible (#104): every table says what it holds, by a <c>caption</c>, an <c>aria-label</c> or
/// an <c>aria-labelledby</c>, and every header cell says which way it heads, by a <c>scope</c>. Both guards read the
/// markup of every <c>.razor</c> file of the app, so a table added without a name or a <c>th</c> written without a
/// <c>scope</c> fails here instead of reaching a screen reader as an unnamed grid of unheaded cells.
/// </summary>
public class TableStructureGuardTests
{
    private const string Table = "<table";
    private const string HeaderCell = "<th";

    /// <summary>
    /// A table a screen reader can name: the opening tag carries <c>aria-labelledby</c> or <c>aria-label</c>, or a
    /// <c>caption</c> opens the table before its first row. The caption is looked for between the opening tag and the
    /// next <c>&lt;thead</c> or <c>&lt;tr</c>, whichever comes first, since a caption after a row names nothing.
    /// </summary>
    [Fact]
    public void EveryTableHasAName()
    {
        var offenders = new List<string>();
        var tables = 0;

        foreach (var (file, text) in RazorMarkup())
        {
            for (var at = text.IndexOf(Table, StringComparison.Ordinal);
                 at >= 0;
                 at = text.IndexOf(Table, at + Table.Length, StringComparison.Ordinal))
            {
                tables++;

                var end = text.IndexOf('>', at);
                if (end >= 0 && NamesTheTable(text, at, end))
                {
                    continue;
                }

                offenders.Add($"{file}:{LineOf(text, at)}");
            }
        }

        Assert.True(tables > 0, "no <table> in any .razor file: the guard reads the wrong place");
        Assert.Empty(offenders);
    }

    /// <summary>
    /// A header cell a screen reader can point at a row or a column with: its tag carries <c>scope</c>. The search
    /// for <c>&lt;th</c> keeps only the ones followed by whitespace or by <c>&gt;</c>, so <c>&lt;thead</c>, the
    /// header section, is not taken for a header cell.
    /// </summary>
    [Fact]
    public void EveryHeaderCellHasAScope()
    {
        var offenders = new List<string>();
        var cells = 0;

        foreach (var (file, text) in RazorMarkup())
        {
            for (var at = text.IndexOf(HeaderCell, StringComparison.Ordinal);
                 at >= 0;
                 at = text.IndexOf(HeaderCell, at + HeaderCell.Length, StringComparison.Ordinal))
            {
                var after = at + HeaderCell.Length;
                if (after >= text.Length || !(text[after] == '>' || char.IsWhiteSpace(text[after])))
                {
                    continue;
                }

                cells++;

                var end = text.IndexOf('>', after);
                if (end < 0 || !text[at..end].Contains("scope=", StringComparison.Ordinal))
                {
                    offenders.Add($"{file}:{LineOf(text, at)}");
                }
            }
        }

        Assert.True(cells > 0, "no <th> in any .razor file: the guard reads the wrong place");
        Assert.Empty(offenders);
    }

    /// <summary>True when the table opening at <paramref name="start"/> is named: by a label of its own tag, or by a
    /// caption before its first header section or row.</summary>
    private static bool NamesTheTable(string text, int start, int end)
    {
        var tag = text[start..end];
        if (tag.Contains("aria-labelledby=", StringComparison.Ordinal) || tag.Contains("aria-label=", StringComparison.Ordinal))
        {
            return true;
        }

        var from = end + 1;
        var next = FirstOf(text, from, "<thead", "<tr");
        var head = text[from..(next < 0 ? text.Length : next)];
        return head.Contains("<caption", StringComparison.Ordinal);
    }

    /// <summary>The first of the markers at or after <paramref name="from"/>, or -1 when none of them follows.</summary>
    private static int FirstOf(string text, int from, params string[] markers)
    {
        var first = -1;

        foreach (var marker in markers)
        {
            var at = text.IndexOf(marker, from, StringComparison.Ordinal);
            if (at >= 0 && (first < 0 || at < first))
            {
                first = at;
            }
        }

        return first;
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
