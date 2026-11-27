using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// <c>aria-hidden="true"</c> tells a screen reader to walk past an element and everything it holds - which is right
/// for the decoration of this dashboard: the icon of <c>StateIcon.razor</c> that stands beside the word "Healthy",
/// the swatch of <c>CodeCard.razor</c> beside the language's name, the dash of <c>UseCell.razor</c> beside a
/// <c>visually-hidden</c> "No measurement". What it does not do is take the keyboard with it: HTML leaves a link, a
/// button, a field, a textarea or a positive <c>tabindex</c> inside that element exactly where it was, so a reader
/// hears nothing at all while Tab still stops there and the keys they press act on a control they cannot perceive
/// (#143). Anything hidden is therefore out of the tab order too, or it is not hidden.
/// <para>
/// So a guard walks every component, takes each element whose own opening tag writes <c>aria-hidden="true"</c> with
/// the whole of its content - up to the <c>&lt;/name&gt;</c> that closes that very element, counting the nested
/// elements of the same name on the way, a self-closing tag owning nothing - and refuses that content when it holds
/// an <c>&lt;a&gt;</c>, a <c>&lt;button&gt;</c>, an <c>&lt;input&gt;</c>, a <c>&lt;select&gt;</c>, a
/// <c>&lt;textarea&gt;</c> or a <c>tabindex</c> that is not <c>-1</c>. The element itself owes the same: a
/// <c>&lt;button aria-hidden="true"&gt;</c> is the same fault written on the hiding tag instead of under it, and so
/// is a <c>tabindex="0"</c> there. A tag is read to the "&gt;" that closes it, past the Razor expressions, the quoted
/// values that carry a "&gt;" of their own and the line breaks they spread over, the way the other markup guards
/// read them.
/// </para>
/// </summary>
public class AriaHiddenFocusGuardTests
{
    private const string HiddenAttribute = "aria-hidden";

    /// <summary>The value that hides an element from a reader, however it is spelled.</summary>
    private const string HiddenValue = "true";

    /// <summary>Tags that hold the keyboard. An <c>&lt;a&gt;</c> is looked for by its whole name - a "&lt;abbr&gt;" is
    /// another element - because the search for the others already ends in a letter of their own.</summary>
    private static readonly string[] FocusableTags = ["<button", "<input", "<select", "<textarea"];

    /// <summary>The same tags as element names, for the hidden element that is one of them rather than their host.</summary>
    private static readonly string[] FocusableElements = ["a", "button", "input", "select", "textarea"];

    /// <summary>The tabindex a hidden element may carry: reachable by script, never by Tab.</summary>
    private const string HiddenTabindex = "-1";

    /// <summary>Elements HTML leaves unclosed: they own no content and write no closing tag, so none is looked for.</summary>
    private static readonly string[] VoidElements =
        ["area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "param", "source", "track", "wbr"];

    /// <summary>
    /// One hidden element of the app: the file to print, the line its tag opens on, the tag itself, its element name
    /// in lower case, and the content that stands between its tag and the closing tag of its own.
    /// </summary>
    private sealed record HiddenElement(string File, int Line, string Tag, string Name, string Content)
    {
        /// <summary>Why the guard refuses this element, in the words a failure prints, or <c>null</c> when nothing
        /// inside it takes the keyboard.</summary>
        public string? Offence =>
            FocusableElements.Contains(Name, StringComparer.Ordinal)
                ? $"<{Name}> is a focus stop of its own while its own tag tells a reader to walk past it"
                : FaultOf(Tag, "its own tag") ?? FaultOf(Content, "its content");
    }

    /// <summary>
    /// No element of any component hides the keyboard: what an <c>aria-hidden="true"</c> tag holds carries no link,
    /// no button, no field and no <c>tabindex</c> but <c>-1</c>.
    /// </summary>
    [Fact]
    public void NothingHiddenFromAReaderHoldsAFocusStop()
    {
        var offenders = Hidden()
            .Where(h => h.Offence is not null)
            .Select(h => $"{h.File}:{h.Line} <{h.Name}> {h.Offence}")
            .ToList();

        Assert.Empty(offenders);
    }

    /// <summary>
    /// A guard that found nothing has seen nothing: the app hides about fourteen elements today - the icons of
    /// <c>StateIcon.razor</c>, <c>ActivityIcon.razor</c> and <c>VersionIcon.razor</c>, the swatches of
    /// <c>RuntimeLegend.razor</c> and <c>CodeCard.razor</c>, the dash of <c>UseCell.razor</c> - so a scan that
    /// reaches far fewer never read the markup, and the clean verdict above would mean nothing.
    /// </summary>
    [Fact]
    public void TheGuardFindsTheHiddenElementsOfTheApp()
    {
        var hidden = Hidden();

        Assert.True(
            hidden.Count >= 10,
            $"only {hidden.Count} aria-hidden element(s) were read under {DashboardRoot()}: the guard is checking nothing");

        Assert.Contains(hidden, h => h.Name == "svg");
        Assert.Contains(hidden, h => h.Name == "span");

        var files = hidden.Select(h => h.File).ToHashSet(StringComparer.Ordinal);

        Assert.True(
            files.Count >= 5,
            $"the {hidden.Count} hidden element(s) read under {DashboardRoot()} live in only {files.Count} file(s): the walk does not reach the app");

        Assert.Contains(hidden, h => h.File.Equals(Path.Combine("Components", "RuntimeLegend.razor"), StringComparison.Ordinal));
        Assert.Contains(hidden, h => h.File.Equals(Path.Combine("Components", "UseCell.razor"), StringComparison.Ordinal));
    }

    /// <summary>The rule refuses what a reader is told to skip while Tab stops on it: an anchor, a button, a field, a
    /// select, a textarea, and a tabindex that is not <c>-1</c> - the tabindex written on the hidden element itself
    /// as surely as on one of its children.</summary>
    [Fact]
    public void TheRuleRefusesAFocusStopInsideAHiddenElement()
    {
        Assert.Contains("holds <a>", OffenceOf("""<span aria-hidden="true"><a href="/work">Work</a></span>"""), StringComparison.Ordinal);
        Assert.Contains("holds <button>", OffenceOf("""<span aria-hidden="true"><button type="button">Go</button></span>"""), StringComparison.Ordinal);
        Assert.Contains("holds <input>", OffenceOf("""<span aria-hidden="true"><input type="search" aria-label="Filter" /></span>"""), StringComparison.Ordinal);
        Assert.Contains("holds <select>", OffenceOf("""<span aria-hidden="true"><select aria-label="Cluster"><option>a</option></select></span>"""), StringComparison.Ordinal);
        Assert.Contains("holds <textarea>", OffenceOf("""<span aria-hidden="true"><textarea aria-label="Note"></textarea></span>"""), StringComparison.Ordinal);
        Assert.Contains("tabindex", OffenceOf("""<span aria-hidden="true"><span tabindex="0">Scrolls</span></span>"""), StringComparison.Ordinal);
        Assert.Contains("tabindex", OffenceOf("""<span aria-hidden="true" tabindex="0">A box</span>"""), StringComparison.Ordinal);
        Assert.Contains("tabindex", OffenceOf("""<span aria-hidden="true" tabindex="@_stop">A box</span>"""), StringComparison.Ordinal);
        Assert.Contains("focus stop of its own", OffenceOf("""<button aria-hidden="true" type="button">Go</button>"""), StringComparison.Ordinal);
    }

    /// <summary>The rule accepts the decoration the app actually hides: a name in words beside the mark, an empty
    /// swatch, a self-closing tag that owns nothing, and the <c>tabindex="-1"</c> that keeps an element out of the
    /// tab order. A tabindex that is not the hidden element's own - <c>data-tabindex</c> - is another attribute and
    /// owes this rule nothing.</summary>
    [Fact]
    public void TheRuleAcceptsAHiddenElementThatHoldsNothingFocusable()
    {
        Assert.Null(OffenceOf("""<span class="use-cell__none" aria-hidden="true">—</span>"""));
        Assert.Null(OffenceOf("""
            <svg class="state-icon" viewBox="0 0 16 16" width="16" height="16" aria-hidden="true" focusable="false">
                <path d="M5 8h6" />
            </svg>
            """));
        Assert.Null(OffenceOf("""<span aria-hidden="true" />"""));
        Assert.Null(OffenceOf("""<span aria-hidden="true"><span tabindex="-1">Read only</span></span>"""));
        Assert.Null(OffenceOf("""<span aria-hidden="true" data-tabindex="0" data-x="a button here">—</span>"""));
    }

    /// <summary>An element is hidden only by its own tag: <c>data-aria-hidden</c> says nothing to a reader, a value
    /// of <c>false</c> says the opposite, and a value Razor builds says nothing yet - so none of them puts the
    /// markup around it under this rule.</summary>
    [Fact]
    public void OnlyTheElementsOwnAttributeHidesIt()
    {
        var text = """
            <div class="card" data-aria-hidden="true"><a href="/work">Work</a></div>
            <div class="card" aria-hidden="false"><a href="/work">Work</a></div>
            <div class="card" aria-hidden="@(_quiet)"><a href="/work">Work</a></div>
            """;

        Assert.Empty(Scan("Sample.razor", text));
    }

    /// <summary>The content read is the element's own: a nested <c>&lt;div&gt;</c> is counted, so the content runs
    /// past the inner closing tag to the one that closes the hidden element - and stops there, so a focus stop that
    /// stands after it is nobody's business but its own.</summary>
    [Fact]
    public void TheContentReadRunsToTheClosingTagOfItsOwnElement()
    {
        var nested = """
            <div aria-hidden="true">
                <div>the legend</div>
                <p><a href="/work">Work</a></p>
            </div>
            """;

        var scanned = Scan("Sample.razor", nested);

        Assert.Single(scanned);
        Assert.Contains("<a href=\"/work\">", scanned[0].Content, StringComparison.Ordinal);
        Assert.Contains("holds <a>", scanned[0].Offence, StringComparison.Ordinal);
        Assert.Contains("the legend</div>", scanned[0].Content, StringComparison.Ordinal);
        Assert.EndsWith("</p>", scanned[0].Content.TrimEnd(), StringComparison.Ordinal);

        var outside = """
            <div aria-hidden="true">the dash</div>
            <a href="/work">Work</a>
            """;

        Assert.Null(OffenceOf(outside));
    }

    /// <summary>A tag ends at the "&gt;" that closes it, not at the first "&gt;" of the file: a class may hold a
    /// comparison, a title a "&gt;" of its own inside the quotes Razor carries, and the attributes - among them
    /// <c>aria-hidden</c> - may spread over lines. The hidden element found is one per opening tag, never the
    /// <c>&lt;/svg&gt;</c> that closes it, and its name is the whole of the name: a <c>&lt;spanish&gt;</c> is
    /// another element.</summary>
    [Fact]
    public void TheTagReadRunsPastRazorAndLineBreaks()
    {
        var text = """
            <main>
                <svg class="icon @(shown ? "icon--shown" : null)"
                     title="@($"3 requests > the last minute")"
                     aria-hidden="true"
                     focusable="false">
                    <path d="M2 8h12" />
                </svg>
                <spanish aria-hidden="true">text</spanish>
                <a href="/work">Work</a>
            </main>
            """;

        var scanned = Scan("Sample.razor", text);

        Assert.Equal(2, scanned.Count);

        Assert.Equal("svg", scanned[0].Name);
        Assert.Equal(2, scanned[0].Line);
        Assert.Contains("focusable", scanned[0].Tag, StringComparison.Ordinal);
        Assert.DoesNotContain("</svg>", scanned[0].Tag, StringComparison.Ordinal);
        Assert.Contains("M2 8h12", scanned[0].Content, StringComparison.Ordinal);
        Assert.Null(scanned[0].Offence);

        Assert.Equal("spanish", scanned[1].Name);
        Assert.Null(scanned[1].Offence);
    }

    /// <summary>The report names the file the way the walk prints it and the line the tag opens on - the first of
    /// them, not the line its <c>aria-hidden</c> sits on - and reads every hidden element of a file on its own.</summary>
    [Fact]
    public void TheReportNamesTheFileAndTheLineOfTheTag()
    {
        var text = """
            <div class="toolbar">
                <span class="icon"
                      aria-hidden="true">—</span>
                <span aria-hidden="true"><button type="button">Go</button></span>
            </div>
            """;

        var scanned = Scan(Path.Combine("Components", "Sample.razor"), text);

        Assert.Equal(2, scanned.Count);
        Assert.Equal(2, scanned[0].Line);
        Assert.Null(scanned[0].Offence);
        Assert.Equal(4, scanned[1].Line);
        Assert.Contains("holds <button>", scanned[1].Offence, StringComparison.Ordinal);
        Assert.StartsWith("Components", scanned[1].File, StringComparison.Ordinal);
    }

    /// <summary>Every hidden element of every component, in the order the files write them.</summary>
    private static List<HiddenElement> Hidden() =>
        [.. RazorMarkup().SelectMany(markup => Scan(markup.File, markup.Text))];

    /// <summary>The hidden elements of one file, each with the content its tag opens.</summary>
    private static List<HiddenElement> Scan(string file, string text)
    {
        var hidden = new List<HiddenElement>();

        foreach (var start in MarkupScan.OpeningTags(text))
        {
            var tag = TagOf(text, start);

            if (!Hides(tag))
            {
                continue;
            }

            var name = NameOf(tag);

            if (name.Length == 0)
            {
                continue;
            }

            // A self-closing tag owns nothing, and a void element writes no closing tag to read up to.
            var content = SelfClosing(tag) || VoidElements.Contains(name, StringComparer.Ordinal)
                ? string.Empty
                : ContentOf(text, start + tag.Length, name);

            hidden.Add(new HiddenElement(file, LineOf(text, start), tag, name, content));
        }

        return hidden;
    }

    /// <summary>Why a piece of markup is refused under a hidden element, in the words a failure prints - or
    /// <c>null</c> when it holds nothing the keyboard can stop on.</summary>
    private static string? FaultOf(string markup, string where)
    {
        foreach (var tag in FocusableTags)
        {
            if (markup.Contains(tag, StringComparison.OrdinalIgnoreCase))
            {
                return $"holds {tag}> in {where}: a reader walks past it while Tab stops on it";
            }
        }

        var anchor = AnchorOf(markup);

        if (anchor >= 0)
        {
            return $"holds <a> in {where}: a reader walks past it while Tab stops on it";
        }

        foreach (var value in TabindexesOf(markup))
        {
            if (value is HiddenTabindex)
            {
                continue;
            }

            return $"holds a tabindex{(value is null ? " with no word in it" : $"=\"{value}\"")} in {where}: a reader walks past it while Tab stops on it";
        }

        return null;
    }

    /// <summary>True when the tag hides its element from a reader: an <c>aria-hidden</c> of the tag's own - the
    /// attribute boundary is the tag's whitespace or its "&lt;", so <c>data-aria-hidden</c> is another attribute -
    /// whose value is "true". A value Razor builds hides nothing this guard can read.</summary>
    private static bool Hides(string tag)
    {
        for (var at = tag.IndexOf(HiddenAttribute, StringComparison.OrdinalIgnoreCase); at >= 0;
             at = tag.IndexOf(HiddenAttribute, at + HiddenAttribute.Length, StringComparison.OrdinalIgnoreCase))
        {
            if (at > 0 && !(char.IsWhiteSpace(tag[at - 1]) || tag[at - 1] == '<'))
            {
                continue;
            }

            if (ValueOf(tag, at + HiddenAttribute.Length) is { } value && value.Equals(HiddenValue, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Every <c>tabindex</c> a piece of markup carries, as the word written there - <c>null</c> when none
    /// can be read of it: no value at all, an empty one, or one Razor builds. Only a tag's own whitespace or its
    /// "&lt;" opens the attribute, so <c>data-tabindex</c> is another attribute and says nothing to a keyboard.</summary>
    private static List<string?> TabindexesOf(string markup)
    {
        var values = new List<string?>();

        for (var at = markup.IndexOf("tabindex", StringComparison.OrdinalIgnoreCase); at >= 0;
             at = markup.IndexOf("tabindex", at + "tabindex".Length, StringComparison.OrdinalIgnoreCase))
        {
            if (at > 0 && !(char.IsWhiteSpace(markup[at - 1]) || markup[at - 1] == '<'))
            {
                continue;
            }

            values.Add(ValueOf(markup, at + "tabindex".Length));
        }

        return values;
    }

    /// <summary>Where the markup opens an <c>&lt;a&gt;</c>, or -1 when it holds none. The name is read whole, so an
    /// <c>&lt;abbr&gt;</c> is another element: only whitespace, "&gt;" or "/" ends it.</summary>
    private static int AnchorOf(string markup)
    {
        for (var at = markup.IndexOf("<a", StringComparison.OrdinalIgnoreCase); at >= 0;
             at = markup.IndexOf("<a", at + 2, StringComparison.OrdinalIgnoreCase))
        {
            if (EndsTheName(markup, at + 2))
            {
                return at;
            }
        }

        return -1;
    }

    /// <summary>The content of the element that opens at <paramref name="from"/>, up to the <c>&lt;/name&gt;</c>
    /// that closes it: the elements of the same name nested inside are counted, so an inner closing tag ends
    /// nothing. When the file never closes the element, what stands after its tag is taken as its content.</summary>
    private static string ContentOf(string text, int from, string name)
    {
        var depth = 1;
        var at = from;

        while (true)
        {
            var next = MarkupScan.NextTag(text, at);

            if (next < 0)
            {
                break;
            }

            if (text[next + 1] == '/')
            {
                if (NameAt(text, next + 2) == name)
                {
                    depth--;

                    if (depth == 0)
                    {
                        return text[from..next];
                    }
                }

                at = next + 1;
                continue;
            }

            if (NameAt(text, next + 1) == name)
            {
                var tag = TagOf(text, next);

                if (!SelfClosing(tag))
                {
                    depth++;
                }

                at = next + tag.Length;
                continue;
            }

            at = next + 1;
        }

        return text[from..];
    }

    /// <summary>The element name a tag opens, in lower case: HTML spells an element name the same whichever way its
    /// letters go, and a <c>&lt;SPAN&gt;</c> hides its content just the same.</summary>
    private static string NameOf(string tag) => NameAt(tag, 1);

    /// <summary>The element name standing at <paramref name="at"/>, read to the first character that cannot be in
    /// one, in lower case - empty when no name stands there.</summary>
    private static string NameAt(string text, int at)
    {
        var end = at;

        while (end < text.Length && (char.IsLetterOrDigit(text[end]) || text[end] is '-' or '_' or ':'))
        {
            end++;
        }

        return end == at ? string.Empty : text[at..end].ToLowerInvariant();
    }

    private static bool EndsTheName(string text, int at) =>
        at < text.Length && (char.IsWhiteSpace(text[at]) || text[at] == '>' || text[at] == '/');

    /// <summary>True when the tag closes itself: the "/" that stands before its "&gt;". A value that ends in a slash
    /// - <c>href="/"</c> - keeps its quote between the two and closes nothing.</summary>
    private static bool SelfClosing(string tag) => tag.Length >= 2 && tag[^2] == '/';

    /// <summary>The value an attribute holds at <paramref name="at"/>, where its name ends: null when no word can be
    /// read of it - no "=", an empty value, or one Razor builds at runtime. The value may be double quoted, single
    /// quoted or - one word with nothing around it, which HTML allows - bare.</summary>
    private static string? ValueOf(string markup, int at)
    {
        while (at < markup.Length && char.IsWhiteSpace(markup[at]))
        {
            at++;
        }

        if (at >= markup.Length || markup[at] != '=')
        {
            return null;
        }

        at++;

        while (at < markup.Length && char.IsWhiteSpace(markup[at]))
        {
            at++;
        }

        var quote = at < markup.Length && (markup[at] == '"' || markup[at] == '\'') ? markup[at] : '\0';
        var start = quote == '\0' ? at : at + 1;
        var end = start;

        while (end < markup.Length && (quote == '\0' ? !(char.IsWhiteSpace(markup[end]) || markup[end] is '>' or '/') : markup[end] != quote))
        {
            end++;
        }

        if (quote != '\0' && end >= markup.Length)
        {
            return null;
        }

        var value = markup[start..end];

        return value.Length == 0 || value.Contains('@') ? null : value;
    }

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

    /// <summary>The offence of the sample's first hidden element - the shape the tests above read the rule
    /// through - refusing a sample that hides nothing at all.</summary>
    private static string? OffenceOf(string text)
    {
        var scanned = Scan("Sample.razor", text);

        Assert.NotEmpty(scanned);

        return scanned[0].Offence;
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
