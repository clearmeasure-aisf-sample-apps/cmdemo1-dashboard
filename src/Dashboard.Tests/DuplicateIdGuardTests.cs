using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// An <c>id</c> is a name the whole page shares: one element answers to it, and everything that reaches for it -
/// an <c>aria-labelledby</c> or <c>aria-describedby</c>, a fragment link, the <c>getElementById</c> of a script -
/// is answered by the <em>first</em> element written with that name. A second one is not a second name but an
/// element nobody can point at: the heading a section claims as its own is the other one, the hint that should read
/// out under a button sits on markup the viewer never reaches, and the deep link jumps to wherever the document
/// began. Nothing about the page looks wrong, which is exactly why it cannot be left to be noticed (#154).
/// <para>
/// The scope of an <c>id</c> is the rendered document, and a Blazor page is rendered out of many components, so a
/// guard that reads one file at a time would miss the duplicate that matters most: the heading id a card writes and
/// the id the page writes over it. This one collects every literal <c>id</c> of every component and refuses a name
/// written twice, whether the two of them stand in the same file or in two files that render into one page.
/// </para>
/// <para>
/// What is read is what can be read without running Razor: the <c>id</c> attribute of a start tag - matched as the
/// attribute of that very name, so a <c>data-id</c>, an <c>aria-labelledby</c>, an <c>xlink:id</c> or the words of a
/// tag are never taken for one - holding a plain value. An <c>id</c> Razor builds at runtime (<c>id="@HeadingId"</c>,
/// <c>id="@($"row-{Index}")"</c>) is not in the collection: two of them may name two elements quite correctly, and
/// which they name belongs to the component's own tests.
/// </para>
/// <para>
/// A tag is read to the "&gt;" that closes it, past the Razor expressions, the quoted values that carry a "&gt;" of
/// their own and the line breaks they spread over, the way <c>LinkGuardTests</c>, <c>TabOrderAndGroupGuardTests</c>
/// and <c>ImageNameGuardTests</c> read them, and the report names the line the tag opens on - the line a fixer finds
/// in the file - for both occurrences of the name it refuses.
/// </para>
/// </summary>
public class DuplicateIdGuardTests
{
    /// <summary>The attribute this rule reads. Only that name, spelled as an attribute of its own, writes an id.</summary>
    private const string IdAttribute = "id";

    /// <summary>
    /// One literal <c>id</c> one component writes: the file to print, the line the tag that writes it opens on, and
    /// the name itself as the markup spells it.
    /// </summary>
    private sealed record IdUse(string File, int Line, string Value)
    {
        /// <summary>Where the name stands, the way every guard of this suite prints a place.</summary>
        public string At => $"{File}:{Line}";
    }

    /// <summary>
    /// No <c>id</c> is written twice: every literal name of every component answers to one element of the page,
    /// whether the two writings of a name stand in one file or in two.
    /// </summary>
    [Fact]
    public void EveryIdNamesOneElementOfThePage()
    {
        Assert.Empty(ReportsOf(IdsOfTheApp()));
    }

    /// <summary>
    /// A guard that found nothing has seen nothing: the app writes twenty-five literal <c>id</c>s today, from the
    /// headings of <c>App.razor</c> and <c>TrafficPanel.razor</c> to the hints of <c>ApplicationView.razor</c> and
    /// the column of <c>WorkItemsView.razor</c>, spread over a dozen files. A scan that reaches far fewer never read
    /// the markup, and the clean verdict above would mean nothing; the floor is set well below that count, so the day
    /// the app gains an id the test still says what it reads.
    /// </summary>
    [Fact]
    public void TheGuardReadsTheIdsOfTheApp()
    {
        var ids = IdsOfTheApp();
        var files = ids.Select(id => id.File).Distinct(StringComparer.Ordinal).ToList();

        Assert.True(
            ids.Count >= 15,
            $"only {ids.Count} literal {IdAttribute} attribute(s) were read under {DashboardRoot()}: the guard is checking nothing");

        Assert.True(
            files.Count >= 5,
            $"the {ids.Count} id(s) read under {DashboardRoot()} live in only {files.Count} file(s): the walk does not reach the app");

        Assert.Contains(ids, id => id.File.Equals("App.razor", StringComparison.Ordinal));
        Assert.Contains(ids, id => id.File.Equals(Path.Combine("Components", "TrafficPanel.razor"), StringComparison.Ordinal));
        Assert.Contains(ids, id => id.File.Equals(Path.Combine("Components", "ApplicationView.razor"), StringComparison.Ordinal));
        Assert.Contains(ids, id => id.Value.Equals("pause-help", StringComparison.Ordinal));
        Assert.Contains(ids, id => id.Value.Equals("events-title", StringComparison.Ordinal));

        // The names the app writes are read as names: no two of them are the same name, so none gives the guard a
        // reason to speak.
        Assert.Empty(ReportsOf(ids));
    }

    /// <summary>The rule refuses a name written twice in one file, and reports both of the places it stands in - the
    /// first occurrence against every later one, so three writings of a name are refused twice over and a fixer sees
    /// both of the lines that owe it a change.</summary>
    [Fact]
    public void TheRuleRefusesAnIdOneFileWritesTwice()
    {
        var text = """
            <div id="traffic-title">the first</div>
            <h2 id="traffic-title">the second</h2>
            """;

        var reports = ReportsOf(text);

        Assert.Single(reports);
        Assert.StartsWith("Sample.razor:1 and Sample.razor:2 both write id=\"traffic-title\"", reports[0], StringComparison.Ordinal);
        Assert.Contains("one file", reports[0], StringComparison.Ordinal);
    }

    /// <summary>Two writings of the same name in one file are refused once per extra occurrence, each of them paired
    /// with the first, so a third one cannot hide behind a second that is already reported.</summary>
    [Fact]
    public void EveryOccurrenceAfterTheFirstIsRefused()
    {
        var text = """
            <h2 id="cluster-title">one</h2>
            <h3 id="cluster-title">two</h3>
            <p id="cluster-title">three</p>
            """;

        var reports = ReportsOf(text);

        Assert.Equal(2, reports.Count);
        Assert.Contains(reports, report => report.Contains("Sample.razor:1 and Sample.razor:2", StringComparison.Ordinal));
        Assert.Contains(reports, report => report.Contains("Sample.razor:1 and Sample.razor:3", StringComparison.Ordinal));
    }

    /// <summary>A tag may write its <c>id</c> twice itself, and HTML keeps both: the second of them names nothing, so
    /// the fault is reported even when it never leaves the one line.</summary>
    [Fact]
    public void TheRuleRefusesAnIdATagWritesTwice()
    {
        var scanned = Scan("Sample.razor", """<div id="x" class="card" id="x">one tag, two of the same name</div>""");

        Assert.Equal(2, scanned.Count);

        var reports = ReportsOf(scanned);

        Assert.Single(reports);
        Assert.Contains("Sample.razor:1 and Sample.razor:1", reports[0], StringComparison.Ordinal);
    }

    /// <summary>The rule refuses the duplicate that no single file shows: a card and the page it renders into write
    /// the same heading name, and the reader is answered by whichever of them the document meets first.</summary>
    [Fact]
    public void TheRuleRefusesAnIdTwoComponentsBothWrite()
    {
        var reports = ReportsOf(
            """<section aria-labelledby="cluster-title"><h2 id="cluster-title">Cluster</h2></section>""",
            """<h2 id="cluster-title">Another cluster</h2>""");

        Assert.Single(reports);
        Assert.Contains("First.razor:1", reports[0], StringComparison.Ordinal);
        Assert.Contains("Second.razor:1", reports[0], StringComparison.Ordinal);
        Assert.Contains("two components", reports[0], StringComparison.Ordinal);
    }

    /// <summary>The rule accepts the ids the app writes: names that differ, in either element and either spelling, and
    /// an <c>id</c> in one file beside a pointer at it in another, which is what a name is for.</summary>
    [Fact]
    public void TheRuleAcceptsIdsThatDiffer()
    {
        Assert.Empty(ReportsOf("""<h2 id="work-title">Work</h2><h2 id="events-title">Events</h2>"""));
        Assert.Empty(ReportsOf("""<span id="pause-help">a</span><span id="PAUSE-HELP">b</span>"""));
        Assert.Empty(ReportsOf("""<section aria-labelledby="traffic-title"><h2 id="traffic-title">Traffic</h2></section>"""));

        // Two files of two pages are refused as a pair all the same: the guard cannot tell which pages a component
        // reaches, and an id that collides somewhere in a document is worth more than the certainty it does not.
        // The app keeps to this rule today - every one of its 25 literal ids is written once, in one file.
        Assert.Single(ReportsOf("""<h2 id="traffic-title">Traffic</h2>""", """<h2 id="traffic-title">Traffic again</h2>"""));
    }

    /// <summary>An <c>id</c> Razor builds at runtime is not a name the markup spells: two of them may name two
    /// elements quite correctly, so the guard reads none of them - as it reads nothing of an <c>id</c> left empty or
    /// written bare, which names no element at all.</summary>
    [Fact]
    public void AnIdRazorBuildsIsNoNameToCompare()
    {
        Assert.Empty(Scan("Sample.razor", """<h2 id="@HeadingId">one</h2><h2 id="@HeadingId">two</h2>"""));
        Assert.Empty(Scan("Sample.razor", """<tr id="@($"row-{Index}")"><td id="@($"row-{Index}")">x</td></tr>"""));
        Assert.Empty(Scan("Sample.razor", """<h2 id="@(_quiet ? "events-title" : "work-title")">one</h2>"""));
        Assert.Empty(Scan("Sample.razor", """<h2 id="">nothing</h2><h2 id>nothing</h2>"""));

        // A literal name beside a runtime one is still read - the expressions do not blind the guard to the rest.
        Assert.Single(Scan("Sample.razor", """<h2 id="events-title">one</h2><h2 id="@HeadingId">two</h2>"""));
    }

    /// <summary>Only the attribute of that very name writes an id: a <c>data-id</c>, an <c>aria-labelledby</c>, an
    /// <c>aria-describedby</c>, an <c>idfoo</c>, an <c>xlink:id</c> and the words between the tags are none of them,
    /// however often the same name stands in them.</summary>
    [Fact]
    public void OnlyTheOwnIdOfATagIsItsId()
    {
        var text = """
            <div data-id="dup" data-guid="dup" aria-labelledby="dup" aria-describedby="dup" idfoo="dup" xlink:id="dup" guid="dup">
                A hint that says id="dup" in words, and nothing that writes the attribute.
                <span id="dup">the one id of the file</span>
            </div>
            """;

        var scanned = Scan("Sample.razor", text);

        Assert.Single(scanned);
        Assert.Equal("dup", scanned[0].Value);
        Assert.Equal(3, scanned[0].Line);
        Assert.Empty(ReportsOf(scanned));
    }

    /// <summary>A tag ends at the "&gt;" that closes it, not at the first "&gt;" of the file, and its attributes may
    /// spread over lines of their own: a class holds a comparison, a title a "&gt;" of its own inside the quotes
    /// Razor carries, an event handler an arrow. Read short, the <c>id</c> below them would be outside the tag and
    /// the name would go uncounted - and a name written after a "&gt;" of Razor's is the same name.</summary>
    [Fact]
    public void TheTagReadRunsPastRazorAndLineBreaks()
    {
        var text = """
            <main>
                <h2 class="title @(wide ? "title--wide" : null)"
                    title="@($"3 requests > the last minute")"
                    @onmouseover="() => _hover = true"
                    id="traffic-title">Traffic</h2>
                <section role="group" aria-labelledby="traffic-title"
                         id="traffic-title">twice the same name</section>
            </main>
            """;

        var scanned = Scan("Sample.razor", text);

        Assert.Equal(2, scanned.Count);

        // The line the tag opens on, which is the line a fixer looks for, not the one its id sits on.
        Assert.Equal(2, scanned[0].Line);
        Assert.Equal(6, scanned[1].Line);

        var reports = ReportsOf(scanned);

        Assert.Single(reports);
        Assert.Contains("Sample.razor:2 and Sample.razor:6", reports[0], StringComparison.Ordinal);
    }

    /// <summary>The report names both occurrences as the walk prints them - file and line - and says which of the
    /// two ways of colliding it found, so a fixer knows whether to look in one file or between two.</summary>
    [Fact]
    public void TheReportNamesThePlaceOfBothOccurrences()
    {
        var reports = ReportsOf(
            Scan(Path.Combine("Components", "TrafficPanel.razor"), """<h2 id="traffic-title">Traffic</h2>""")
            .Concat(Scan("App.razor", """<h2 id="traffic-title">Traffic again</h2>""")));

        Assert.Single(reports);
        Assert.StartsWith($"{Path.Combine("Components", "TrafficPanel.razor")}:1 and App.razor:1", reports[0], StringComparison.Ordinal);
        Assert.Contains("two components", reports[0], StringComparison.Ordinal);
        Assert.Contains("id=\"traffic-title\"", reports[0], StringComparison.Ordinal);
    }

    /// <summary>Every literal <c>id</c> of every component, in the order the files write them.</summary>
    private static List<IdUse> IdsOfTheApp() =>
        [.. RazorMarkup().SelectMany(markup => Scan(markup.File, markup.Text))];

    /// <summary>The ids one file writes: the value of every <c>id</c> attribute of every start tag, as the markup
    /// spells it. The whole of a component and a sample of markup the tests above read the rules through are read the
    /// same way.</summary>
    private static List<IdUse> Scan(string file, string text)
    {
        var ids = new List<IdUse>();

        foreach (var start in MarkupScan.OpeningTags(text))
        {
            var tag = TagOf(text, start);

            foreach (var (name, value) in AttributesOf(tag))
            {
                // Only the attribute of that very name, and only a value the markup already spells out: an id Razor
                // builds may name two elements correctly, and an empty or bare one names nothing to compare.
                if (!name.Equals(IdAttribute, StringComparison.OrdinalIgnoreCase)
                    || string.IsNullOrWhiteSpace(value)
                    || value.Contains('@', StringComparison.Ordinal))
                {
                    continue;
                }

                ids.Add(new IdUse(file, LineOf(text, start), value!));
            }
        }

        return ids;
    }

    /// <summary>Why the guard speaks: one line per duplicate, naming the file and the line of both occurrences and
    /// whether the two of them stand in one file or in two. The first writing of a name is never itself an offence;
    /// every later one is reported against it.</summary>
    private static List<string> ReportsOf(IEnumerable<IdUse> ids)
    {
        var reports = new List<string>();

        foreach (var written in ids.GroupBy(id => id.Value, StringComparer.Ordinal).OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            var occurrences = written.ToList();

            if (occurrences.Count < 2)
            {
                continue;
            }

            var first = occurrences[0];

            foreach (var later in occurrences.Skip(1))
            {
                var where = first.File.Equals(later.File, StringComparison.Ordinal)
                    ? "twice in one file"
                    : "in two components that render into one page";

                reports.Add(
                    $"{first.At} and {later.At} both write id=\"{written.Key}\" {where}: an id names one element of the"
                    + " page, so an aria-labelledby, a fragment link and a script that asks for it reach the first of"
                    + " them and never the second");
            }
        }

        return reports;
    }

    /// <summary>The reports of one sample of markup, read as one file.</summary>
    private static List<string> ReportsOf(string text) => ReportsOf(Scan("Sample.razor", text));

    /// <summary>The reports of two samples of markup, read as two components of the same page - the duplicate no
    /// single file shows, which is half of why this rule is walked over the app rather than file by file.</summary>
    private static List<string> ReportsOf(string first, string second) =>
        ReportsOf([.. Scan("First.razor", first), .. Scan("Second.razor", second)]);

    /// <summary>How the whole opening tag is read, from its "&lt;" to the "&gt;" that closes it - or to the end of
    /// the file when nothing does. An attribute value is run through rather than read to its first quote, because
    /// Razor quotes inside it, and a title may hold a "&gt;" of its own between them. The parentheses of an
    /// expression outside any value are counted too, the way the other guards count them.</summary>
    private static string TagOf(string text, int start)
    {
        var depth = 0;

        for (var at = start; at < text.Length; at++)
        {
            var c = text[at];

            if (c == '"')
            {
                at = EndOfValue(text, at);
                continue;
            }

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

    /// <summary>The quote that closes the value opened at <paramref name="open"/>, or the end of the text when the
    /// value is never closed. A <c>@(</c> inside the value is read to the <c>)</c> that matches it, the string
    /// literals of the expression and their quotes skipped over.</summary>
    private static int EndOfValue(string text, int open)
    {
        for (var at = open + 1; at < text.Length; at++)
        {
            var c = text[at];

            if (c == '"')
            {
                return at;
            }

            if (c == '@' && at + 1 < text.Length && text[at + 1] == '(')
            {
                at = EndOfExpression(text, at + 1);
            }
        }

        return text.Length;
    }

    /// <summary>The <c>)</c> that closes the <c>(</c> at <paramref name="open"/>, or the end of the text. The quotes
    /// of the expression's own string literals are run through, since a literal may hold anything a plain read would
    /// stop at.</summary>
    private static int EndOfExpression(string text, int open)
    {
        var depth = 0;

        for (var at = open; at < text.Length; at++)
        {
            var c = text[at];

            if (c == '"')
            {
                var closing = text.IndexOf('"', at + 1);
                at = closing < 0 ? text.Length : closing;
            }
            else if (c == '(')
            {
                depth++;
            }
            else if (c == ')')
            {
                depth--;

                if (depth == 0)
                {
                    return at;
                }
            }
        }

        return text.Length;
    }

    /// <summary>The attributes an opening tag writes, as name and value, in the order the tag writes them. Each is
    /// read whole, quoted values and all - a value is closed by its own quote, not by the first "&gt;" it holds - so
    /// a word of a value is never taken for an attribute and only the tag's own whitespace opens the next one. The
    /// "<c>@</c>" of a Razor attribute (<c>@onclick</c>, <c>@ref</c>) is not a name character: the name after it is
    /// read as written, since none of them writes an id.</summary>
    private static List<(string Name, string? Value)> AttributesOf(string tag)
    {
        var attributes = new List<(string Name, string? Value)>();
        var at = tag.IndexOf('<', StringComparison.Ordinal);

        if (at < 0)
        {
            return attributes;
        }

        at++;

        // The element name, which is no attribute.
        while (at < tag.Length && IsNameChar(tag[at]))
        {
            at++;
        }

        while (at < tag.Length)
        {
            while (at < tag.Length && (char.IsWhiteSpace(tag[at]) || tag[at] == '/'))
            {
                at++;
            }

            if (at >= tag.Length || tag[at] == '>')
            {
                break;
            }

            var start = at;

            while (at < tag.Length && IsNameChar(tag[at]))
            {
                at++;
            }

            if (at == start)
            {
                // Neither whitespace nor a name: a stray character of Razor inside the tag.
                at++;
                continue;
            }

            var name = tag[start..at];
            var probe = at;

            while (probe < tag.Length && char.IsWhiteSpace(tag[probe]))
            {
                probe++;
            }

            if (probe >= tag.Length || tag[probe] != '=')
            {
                attributes.Add((name, null));
                continue;
            }

            probe++;

            while (probe < tag.Length && char.IsWhiteSpace(tag[probe]))
            {
                probe++;
            }

            if (probe < tag.Length && (tag[probe] == '"' || tag[probe] == '\''))
            {
                var quote = tag[probe];

                // A double quoted value is run through the way the tag itself is read, because Razor quotes inside
                // it and a title may hold a "&gt;" of its own between them: a plain search for the next quote would
                // stop at the first of Razor's and then take the "&gt;" of the value for the end of the tag.
                var end = quote == '"' ? EndOfValue(tag, probe) : tag.IndexOf('\'', probe + 1);

                if (end < 0 || end >= tag.Length)
                {
                    // The value is never closed: what is read of the tag ends here rather than guessing on.
                    attributes.Add((name, null));
                    break;
                }

                attributes.Add((name, tag[(probe + 1)..end]));
                at = end + 1;
                continue;
            }

            var bare = probe;

            while (probe < tag.Length && !(char.IsWhiteSpace(tag[probe]) || tag[probe] is '>' or '/'))
            {
                probe++;
            }

            attributes.Add((name, tag[bare..probe]));
            at = probe;
        }

        return attributes;
    }

    private static bool IsNameChar(char c) => char.IsLetterOrDigit(c) || c is '-' or '_' or ':';

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
