using System.Runtime.CompilerServices;
using System.Text;

namespace Dashboard.Tests;

/// <summary>
/// A <c>title</c> is a second voice for the element: a screen reader reads the element's words, and reads the
/// <c>title</c> with them, so a <c>title</c> that holds the very words the element already shows says each word
/// twice and adds nothing the text does not already say (#212). The <c>title</c> that earns its place is the one
/// that says something the words do not - the full date behind "3 minutes ago", the explanation behind "CPU" -
/// and this guard walks every component looking for the other kind: a <c>title</c> whose words are exactly the
/// words the element holds, so that the reader hears the sentence once as text and once as title.
/// <para>
/// Only a <c>title</c> written as words in the markup is read: <c>title="From the commit to this deployment"</c>
/// is a literal the guard can compare, while <c>title="@DeliveryText.BehindHelp(First!)"</c> and
/// <c>title="@($"Started {TimeText.DateAndTime(started, Zone)}")"</c> are written by the component's own code,
/// and whether that code repeats the words on the page is the code's tests' business, not this guard's. An
/// <c>@</c> anywhere in the value marks one - at the start it opens an expression, among the words it interpolates
/// them in - so a value carrying an <c>@</c> is not the markup's to read. The same holds for the element's text:
/// words a Razor expression renders are not words in the markup, and the comparison is passed over rather than
/// made against the <c>@</c> that stands for them.
/// </para>
/// <para>
/// The words the element holds are the words a reader hears: the text it stands between its tags, the tags of
/// its children stripped away, and the words inside an <c>aria-hidden="true"</c> child dropped, since a reader
/// does not hear them. Whitespace at the ends a template drifts in is not a word either, so the words are read
/// trimmed, and the comparison is the exact one: "No measurement" is "No measurement", and nothing else is. An
/// empty <c>title</c> says nothing and is not this guard's fault to find, and a <c>data-title</c>, a
/// <c>title-foo</c> or an attribute the component spells <c>Title</c> with a value the component builds is read
/// like any other attribute - by its whole name, HTML leaving an attribute name case-free - and none of them
/// rescues or indicts the element. The <c>&lt;title&gt;</c> of an SVG diagram is an element named title, not an
/// attribute, and opens nothing this guard reads.
/// </para>
/// <para>
/// The app carries two such redundancies today: the "No measurement" of <c>UseCell.razor</c> and
/// <c>UseRow.razor</c>, each of which shows an em dash to the sighted and says "No measurement" to the reader in
/// a visually-hidden span, and then writes the same words again as a <c>title</c> the reader will hear on top of
/// them. The guard allows them exactly - the file and line of each - so that the next redundancy added is refused
/// here instead of reaching a viewer as a sentence read twice, and so that the allowance shrinks the day the
/// markup is fixed.
/// </para>
/// </summary>
public class RedundantTitleGuardTests
{
    /// <summary>The attribute this rule is about, as HTML spells it case-free.</summary>
    private const string TitleAttribute = "title";

    /// <summary>The attribute that takes the words a reader would otherwise hear.</summary>
    private const string AriaHiddenAttribute = "aria-hidden";

    /// <summary>The one character that says a value or a stretch of text is built at Razor runtime and is not this
    /// guard's to read.</summary>
    private const char Razor = '@';

    /// <summary>
    /// The violations the app carries today, the file the walk prints and the line the tag opens on: the
    /// "No measurement" of <c>UseCell.razor</c> and <c>UseRow.razor</c>. The list is exact in both directions - a
    /// violation not on it fails the guard, and an entry on it that the markup no longer carries fails it too, so
    /// the allowance cannot outlive the fault.
    /// </summary>
    private static readonly (string File, int Line)[] Allowlist =
    [
        (Path.Combine("Components", "UseCell.razor"), 18),
        (Path.Combine("Components", "UseRow.razor"), 17),
    ];

    /// <summary>
    /// One <c>title</c> of the markup: the file to print, the line its tag opens on, the element it stands on, the
    /// value the tag writes - <c>null</c> when the attribute is written bare - and the words the element holds.
    /// </summary>
    private sealed record TitledElement(string File, int Line, string Element, string? Title, string? Text)
    {
        /// <summary>
        /// True of the value this guard can read as words: something in it, and no <c>@</c> anywhere - at the start
        /// the <c>@</c> opens an expression, among the words it interpolates them in - so the words the value holds
        /// are the words it writes.
        /// </summary>
        public bool Literal => Title is { } value && !string.IsNullOrWhiteSpace(value) && !value.Contains(Razor);

        /// <summary>
        /// True when the <c>title</c> is a literal and the element's own words are exactly its words: the reader
        /// hears them as text and hears them again as the <c>title</c>. Words Razor renders, or a <c>title</c>
        /// Razor builds, are not the markup's to know, and an empty side reads nothing to repeat.
        /// </summary>
        public bool Redundant =>
            Literal && Text is { Length: > 0 } && Text == Title!.Trim();

        /// <summary>Where the fault stands: the file as the walk prints it, the line the tag opens on, and the
        /// words the <c>title</c> repeats.</summary>
        public string Report =>
            $"{File}:{Line} <{Element}> title=\"{(Title ?? string.Empty)}\" repeats the text of its element, which a reader hears twice";
    }

    /// <summary>One attribute of a tag: its name as written and the value after it, if any - <c>null</c> when the
    /// attribute is written bare, which HTML reads as an empty value.</summary>
    private sealed record TagAttribute(string Name, string? Value);

    /// <summary>
    /// No <c>title</c> of the app repeats the words of the element it stands on, except the two the allowlist
    /// holds: every redundancy the walk finds is one the allowlist names, and every name the allowlist holds is
    /// still a redundancy in the markup, so the list can neither hide a new fault nor outlive an old one.
    /// </summary>
    [Fact]
    public void NoTitleRepeatsTheTextOfItsElement()
    {
        var reported = TitledElements()
            .Where(title => title.Redundant)
            .Select(title => (File: title.File, Line: title.Line, Report: title.Report))
            .ToList();

        var unlisted = reported
            .Where(found => Allowlist.All(listed => listed.File != found.File || listed.Line != found.Line))
            .ToList();

        Assert.True(
            unlisted.Count == 0,
            "titles that repeat the text of their element: " + string.Join("; ", unlisted.Select(found => found.Report)));

        var stale = Allowlist
            .Where(listed => reported.All(found => found.File != listed.File || found.Line != listed.Line))
            .Select(listed => $"{listed.File}:{listed.Line}")
            .ToList();

        Assert.True(
            stale.Count == 0,
            "the allowlist names a redundancy the markup no longer carries - shrink it: " + string.Join("; ", stale));
    }

    /// <summary>
    /// A guard that found nothing has seen nothing: the app writes well over a hundred <c>title</c> attributes
    /// across its components today, of which the plain-literal ones - the ones this rule can read - are a third.
    /// A scan that reaches far fewer never read the markup, and the clean verdict above would mean nothing; the
    /// floors are set well below those counts, so the day the app gains a title the test still says what it reads.
    /// The walk is also checked against the files the rule lives or dies on: the two the allowlist holds and a
    /// component whose literal <c>title</c>s all add words, so a walk that reads the wrong place fails here.
    /// </summary>
    [Fact]
    public void TheGuardReadsTheTitlesOfTheApp()
    {
        var titles = TitledElements();
        var files = titles.Select(title => title.File).Distinct(StringComparer.Ordinal).ToList();
        var literals = titles.Count(title => title.Literal);

        Assert.True(
            titles.Count >= 60,
            $"only {titles.Count} start tag(s) with a title were read under {DashboardRoot()}: the guard is checking nothing");

        Assert.True(
            files.Count >= 15,
            $"the {titles.Count} titled tag(s) read live in {files.Count} file(s): the walk does not reach the app");

        Assert.True(
            literals >= 20,
            $"only {literals} of the {titles.Count} title(s) read are literals the rule can compare: the walk does not reach the app's markup");

        Assert.Contains(titles, title => title.File.Equals(Path.Combine("Components", "UseCell.razor"), StringComparison.Ordinal));
        Assert.Contains(titles, title => title.File.Equals(Path.Combine("Components", "UseRow.razor"), StringComparison.Ordinal));
        Assert.Contains(titles, title => title.File.Equals(Path.Combine("Components", "PodTable.razor"), StringComparison.Ordinal));

        // The rule is not blind either: the two the allowlist holds are found as redundancies, so the exactness of
        // the allowlist above rests on a rule that really reads the app.
        Assert.Contains(titles, title => title.Redundant);
        Assert.Equal(2, titles.Count(title => title.Redundant));
    }

    /// <summary>
    /// The rule refuses the <c>title</c> whose words are the element's words: plainly, in the shape the app
    /// writes - an em dash hidden from the reader and the words in a visually-hidden span, the title then saying
    /// them again - with the whitespace a template drifts at the ends, with the attribute spelled in capitals,
    /// with the value single-quoted, and across a tag that spreads over lines.
    /// </summary>
    [Fact]
    public void TheRuleRefusesATitleThatRepeatsTheText()
    {
        Assert.True(OffenceOf("""<span title="No measurement">No measurement</span>"""));

        // The shape of UseCell.razor and UseRow.razor: the dash a reader does not hear, the words a reader does,
        // and the title that says them a second time.
        Assert.True(OffenceOf(
            """<span class="use-cell__none" title="No measurement"><span aria-hidden="true">—</span><span class="visually-hidden">No measurement</span></span>"""));

        // The words on their own lines, the tag spread over them: the drift is not a word, and the title says
        // the words again.
        Assert.True(OffenceOf("""
            <button type="button" class="button" title="Stop traffic">
                Stop traffic
            </button>
            """));

        Assert.True(OffenceOf("""<span TITLE="No measurement">No measurement</span>"""));
        Assert.True(OffenceOf("<span title='No measurement'>No measurement</span>"));

        Assert.True(OffenceOf("""
            <span class="use-cell__none"
                  title="No measurement"><span aria-hidden="true">—</span><span class="visually-hidden">No measurement</span></span>
            """));
    }

    /// <summary>
    /// The rule accepts the <c>title</c> that says something the words do not: the explanation behind a short
    /// label, a value Razor builds, words Razor renders, words that are not quite the same, another attribute
    /// that happens to end in <c>title</c>, an empty <c>title</c>, an element with no words, and the case where
    /// only the hidden words match - the words a reader hears do not, and the reader is who is read.
    /// </summary>
    [Fact]
    public void TheRuleAcceptsATitleThatAddsWords()
    {
        var accepted = new[]
        {
            """<th scope="col" title="How many rows the table holds">Rows</th>""",
            """<dt title="From the commit to this deployment">Lead time</dt>""",
            """<span title="@DeliveryText.BehindHelp(First!)">Compared</span>""",
            """<span title="@($"Started {TimeText.DateAndTime(started, Zone)}")">@ClusterText.Age(started, AsOf)</span>""",
            """<span title="No measurement">@NoMeasurement</span>""",
            """<span title="No measurement">no measurement</span>""",
            """<span title="No measurement">No measurements</span>""",
            """<span data-title="No measurement" title="Not these words">No measurement</span>""",
            """<span title="Not these words" title-foo="No measurement">No measurement</span>""",
            """<span title="">No measurement</span>""",
            """<span title="No measurement"></span>""",
            """<span title="No measurement"><span aria-hidden="true">No measurement</span><span class="visually-hidden">Not measured at all</span></span>""",
            """<a href="@url" title="@link.Title">@link.Text</a>""",
        };

        foreach (var text in accepted)
        {
            var titles = Scan("Sample.razor", text);

            Assert.Single(titles);
            Assert.False(titles[0].Redundant, $"{titles[0].Report}: the rule reads words a title does not repeat");
        }
    }

    /// <summary>
    /// Only the attribute of that very name indicts an element, and only on the element that carries it: the
    /// <c>&lt;title&gt;</c> of an SVG diagram is an element named title - it has no <c>title</c> attribute and
    /// stands for no other element's words - while the <c>data-title</c> written beside a redundant
    /// <c>title</c> is another attribute, read by its whole name, and the <c>&lt;/span&gt;</c> that closes a
    /// span writes no attribute at all.
    /// </summary>
    [Fact]
    public void OnlyTheTitleAttributeOfAnElementCounts()
    {
        var text = """
            <svg class="diagram" role="img">
                <title>No measurement</title>
            </svg>
            <span title="No measurement" data-title="No measurement">No measurement</span>
            """;

        var titles = Scan("Sample.razor", text);

        // The svg's <title> element opens no attribute, and the closing tag of anything writes none: one title.
        Assert.Single(titles);
        Assert.Equal("span", titles[0].Element);
        Assert.True(titles[0].Redundant);
    }

    /// <summary>
    /// The words of an element run to the <c>&lt;/name&gt;</c> that closes it, past the children of its own
    /// name: the outer span holds the inner span's words and its own, and the inner span holds only its own, so
    /// the inner one - whose <c>title</c> is its own two words - is the redundancy, and the outer one, whose
    /// words are more than its <c>title</c>, is not read as one at all.
    /// </summary>
    [Fact]
    public void TheTextRunsToTheClosingTagOfItsOwnElement()
    {
        var offenders = ReportsOf(Scan("Sample.razor", """
            <span title="Inner words">
                <span title="inner">inner</span>
                more
            </span>
            """));

        Assert.Single(offenders);
        Assert.Contains("<span>", offenders[0], StringComparison.Ordinal);

        // And the reverse nesting: an inner span of words inside an outer one whose words are exactly its
        // title, the inner span writing no title of its own.
        var nested = Scan("Sample.razor", """<span title="inner"><span>inner</span></span>""");

        Assert.Single(nested);
        Assert.True(nested[0].Redundant);
    }

    /// <summary>
    /// The report names the file the way the walk prints it and the line the tag opens on - the first of them,
    /// not the line a title drifted to on a multi-line tag - so the one who fixes the markup is sent to the tag.
    /// </summary>
    [Fact]
    public void TheReportNamesTheFileAndTheLineOfTheTag()
    {
        var text = """
            <div class="use-cell">
                <span>before</span>
                <span class="use-cell__none"
                      title="No measurement"><span aria-hidden="true">—</span><span class="visually-hidden">No measurement</span></span>
            </div>
            """;

        var offenders = ReportsOf(Scan(Path.Combine("Components", "Sample.razor"), text));

        Assert.Single(offenders);
        Assert.StartsWith(Path.Combine("Components", "Sample.razor") + ":3 ", offenders[0], StringComparison.Ordinal);
        Assert.Contains("<span>", offenders[0], StringComparison.Ordinal);
        Assert.Contains("title=\"No measurement\"", offenders[0], StringComparison.Ordinal);
        Assert.Contains("hears twice", offenders[0], StringComparison.Ordinal);
    }

    /// <summary>Every <c>title</c> of every component, in the order the files write them.</summary>
    private static List<TitledElement> TitledElements() =>
        [.. RazorMarkup().SelectMany(markup => Scan(markup.File, markup.Text))];

    /// <summary>The <c>title</c> attributes a piece of markup writes: every start tag that stands one, with its
    /// value as written and the words the element holds. A whole component and a sample of markup the tests above
    /// read the rule through are scanned the same way.</summary>
    private static List<TitledElement> Scan(string file, string text)
    {
        var titles = new List<TitledElement>();

        foreach (var start in OpeningTags(text))
        {
            var tag = TagOf(text, start);
            var element = NameOf(tag);
            var attributes = AttributesOf(tag);
            var title = attributes.FirstOrDefault(attribute => attribute.Name.Equals(TitleAttribute, StringComparison.OrdinalIgnoreCase));

            if (title is null)
            {
                continue;
            }

            titles.Add(new TitledElement(
                file,
                LineOf(text, start),
                element,
                title.Value,
                AccessibleText(text, start + tag.Length, element)));
        }

        return titles;
    }

    /// <summary>The words the element holds, trimmed of the whitespace a template drifts at the ends: the text
    /// between its tags, the tags of its children stripped away, and the words inside an
    /// <c>aria-hidden="true"</c> child dropped, since a reader does not hear them. <c>null</c> when the words
    /// carry a Razor expression - the words the markup does not hold - and the comparison the rule makes is not
    /// the comparison the markup can be made on.</summary>
    private static string? AccessibleText(string text, int from, string element)
    {
        var words = new StringBuilder();
        var hidden = new List<bool>();
        var depth = 1;
        var at = from;

        while (true)
        {
            var next = NextTag(text, at);
            if (next < 0)
            {
                break;
            }

            var chunk = text[at..next];

            if (hidden.All(flag => !flag) && chunk.Length > 0)
            {
                if (chunk.Contains(Razor))
                {
                    return null;
                }

                words.Append(chunk);
            }

            var isClose = text[next + 1] == '/';
            var tag = TagOf(text, next);
            var name = isClose ? CloseNameOf(tag) : NameOf(tag);
            at = next + tag.Length;

            if (name == element)
            {
                if (isClose)
                {
                    depth--;
                    if (depth == 0)
                    {
                        break;
                    }
                }
                else if (!SelfClosing(tag) && !VoidElement(name))
                {
                    depth++;
                }
            }

            if (isClose)
            {
                if (hidden.Count > 0)
                {
                    hidden.RemoveAt(hidden.Count - 1);
                }
            }
            else if (!SelfClosing(tag) && !VoidElement(name))
            {
                hidden.Add(HiddenElement(tag));
            }
        }

        return words.ToString().Trim();
    }

    /// <summary>Why the titles are refused, one line each: the file, the line of the tag and the words the
    /// <c>title</c> repeats.</summary>
    private static List<string> ReportsOf(IEnumerable<TitledElement> titles) =>
        [.. titles
            .Where(title => title.Redundant)
            .Select(title => title.Report)];

    /// <summary>The first <c>title</c> the sample writes, refusing a sample the rule finds nothing to say about,
    /// and whether it is one the rule refuses.</summary>
    private static bool OffenceOf(string text)
    {
        var titles = Scan("Sample.razor", text);

        Assert.NotEmpty(titles);

        return titles[0].Redundant;
    }

    /// <summary>True of the element that ends and never opens: HTML's void elements, which stand no
    /// <c>&lt;/name&gt;</c> to balance against.</summary>
    private static bool VoidElement(string name) => name is
        "area" or "base" or "br" or "col" or "embed" or "hr" or "img" or "input" or "link" or "meta" or "param" or "source" or "track" or "wbr";

    /// <summary>True of the tag that ends in <c>/&gt;</c>.</summary>
    private static bool SelfClosing(string tag) => tag.Length >= 2 && tag[^2] == '/' && tag[^1] == '>'; 

    /// <summary>True of the tag that writes <c>aria-hidden</c> as true: the value spelled true in any case, or
    /// the attribute written bare, which ARIA reads as true. A value of false hides nothing.</summary>
    private static bool HiddenElement(string tag) =>
        AttributesOf(tag).Any(attribute =>
            attribute.Name.Equals(AriaHiddenAttribute, StringComparison.OrdinalIgnoreCase)
            && (attribute.Value is null
                || (attribute.Value is { } value && value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase))));

    /// <summary>Every "&lt;" that opens a tag, closing tags left out.</summary>
    private static List<int> OpeningTags(string text)
    {
        var starts = new List<int>();

        for (var at = NextTag(text, 0); at >= 0; at = NextTag(text, at + 1))
        {
            if (text[at + 1] != '/')
            {
                starts.Add(at);
            }
        }

        return starts;
    }

    /// <summary>The next "&lt;" that opens or closes a tag at or after <paramref name="from"/>, or -1. A "&lt;" of
    /// Razor - <c>@(count &lt; 1)</c> - is followed by a space or a digit and opens nothing.</summary>
    private static int NextTag(string text, int from)
    {
        for (var at = text.IndexOf('<', from); at >= 0; at = text.IndexOf('<', at + 1))
        {
            if (at + 1 < text.Length && (text[at + 1] == '/' || char.IsLetter(text[at + 1])))
            {
                return at;
            }
        }

        return -1;
    }

    /// <summary>The element a start tag opens, in lower case: HTML spells an element name the same whichever way
    /// its letters go.</summary>
    private static string NameOf(string tag)
    {
        var end = 1;

        while (end < tag.Length && (char.IsLetterOrDigit(tag[end]) || tag[end] is '-' or '_' or ':'))
        {
            end++;
        }

        return tag[1..end].ToLowerInvariant();
    }

    /// <summary>The element a closing tag ends, in lower case.</summary>
    private static string CloseNameOf(string tag)
    {
        var end = 2;

        while (end < tag.Length && (char.IsLetterOrDigit(tag[end]) || tag[end] is '-' or '_' or ':'))
        {
            end++;
        }

        return tag[2..end].ToLowerInvariant();
    }

    /// <summary>The attributes a tag writes, past its element name: each name as written and the value after it,
    /// if any. A quoted value is run through to the quote that closes it, Razor's own quotes included, and the
    /// "/" that opens a self-closing tag ends the tag rather than naming an attribute.</summary>
    private static List<TagAttribute> AttributesOf(string tag)
    {
        var attributes = new List<TagAttribute>();
        var at = 1;

        while (at < tag.Length && !(char.IsWhiteSpace(tag[at]) || tag[at] == '>'))
        {
            at++;
        }

        while (true)
        {
            at = SkipSpace(tag, at);

            if (at >= tag.Length || tag[at] is '>' or '/')
            {
                break;
            }

            var name = at;

            while (at < tag.Length && !(char.IsWhiteSpace(tag[at]) || tag[at] is '=' or '>' or '/'))
            {
                at++;
            }

            var attributeName = tag[name..at];

            at = SkipSpace(tag, at);

            string? value = null;

            if (at < tag.Length && tag[at] == '=')
            {
                at = SkipSpace(tag, at + 1);
                value = ValueAt(tag, ref at);
            }

            attributes.Add(new TagAttribute(attributeName, value));
        }

        return attributes;
    }

    /// <summary>The value that stands at <paramref name="at"/>, read from its opening quote to the closing one
    /// that Razor does not trick the read with, or to the whitespace that ends a bare one. The index is left
    /// after the value. A pair of quotes with nothing between them is read as the empty string, and an attribute
    /// written without a value at all as <c>null</c>.</summary>
    private static string? ValueAt(string tag, ref int at)
    {
        var quote = at < tag.Length && (tag[at] == '"' || tag[at] == '\'') ? tag[at] : '\0';

        if (quote == '\0')
        {
            var bare = at;

            while (at < tag.Length && !(char.IsWhiteSpace(tag[at]) || tag[at] == '>'))
            {
                at++;
            }

            return tag[bare..at];
        }

        var open = at;
        var close = EndOfValue(tag, open, quote);

        at = close < tag.Length ? close + 1 : close;

        return close <= open + 1 ? string.Empty : tag[(open + 1)..close];
    }

    private static int SkipSpace(string text, int at)
    {
        while (at < text.Length && char.IsWhiteSpace(text[at]))
        {
            at++;
        }

        return at;
    }

    /// <summary>How the whole tag is read, from its "&lt;" to the "&gt;" that closes it - or to the end of the
    /// file when nothing does. An attribute value is run through rather than read to its first quote, because
    /// Razor quotes inside it, and a title may hold a "&gt;" of its own between them. The parentheses of an
    /// expression outside any value are counted too, the way the other guards count them.</summary>
    private static string TagOf(string text, int start)
    {
        var depth = 0;

        for (var at = start; at < text.Length; at++)
        {
            var c = text[at];

            if (c is '"' or '\'')
            {
                at = EndOfValue(text, at, c);
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
    private static int EndOfValue(string text, int open, char quote)
    {
        for (var at = open + 1; at < text.Length; at++)
        {
            var c = text[at];

            if (c == quote)
            {
                return at;
            }

            if (c == Razor && at + 1 < text.Length && text[at + 1] == '(')
            {
                at = EndOfExpression(text, at + 1);
            }
        }

        return text.Length;
    }

    /// <summary>The <c>)</c> that closes the <c>(</c> at <paramref name="open"/>, or the end of the text. The
    /// quotes of the expression's own string literals are run through, since a literal may hold anything a plain
    /// read would stop at.</summary>
    private static int EndOfExpression(string text, int open)
    {
        var depth = 0;

        for (var at = open; at < text.Length; at++)
        {
            var c = text[at];

            if (c is '"' or '\'')
            {
                var closing = text.IndexOf(c, at + 1);
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
