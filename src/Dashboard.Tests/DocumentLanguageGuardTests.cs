using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The one attribute of a document that tells a reader - human or screen reader - which language its words are
/// written in. The dashboard's page opens <c>&lt;html lang="en"&gt;</c> in <c>wwwroot/index.html</c>, and the
/// language is the one thing a reader cannot be left to guess: the same letter forms are read aloud in the voice
/// of one language and mispronounced in another, so a reader takes up the right voice and a search engine the
/// right language only when the document says which. An <c>&lt;html&gt;</c> that writes no <c>lang</c> - or
/// writes a <c>lang</c> that holds nothing, an empty pair of quotes, nothing between them but the spaces, tabs
/// and line breaks a template drifts in - says no language any more plainly than no attribute at all, and is
/// refused for the same reason.
/// <para>
/// A guard walks the document itself and every <c>.razor</c> file of the app, refusing an <c>&lt;html&gt;</c>
/// start tag that names no language, and listing the file and the line the tag opens on, so a language that is
/// dropped or a document root that is grown elsewhere cannot be forgotten. The tag is read to the "&gt;" that
/// closes it, past the Razor expressions, the quoted values that carry a "&gt;" of their own and the line breaks
/// they spread over - the way <see cref="TimeElementGuardTests" /> and <see cref="NewTabLinkGuardTests" /> read
/// them - and its attributes are read as name/value pairs, so the whole name is what counts: a <c>data-lang</c>,
/// an <c>xml:lang</c> or a <c>lang-foo</c> is another attribute, while <c>LANG</c> is this one, HTML leaving an
/// attribute name case-free. The element is matched whole too, so a <c>&lt;htmlfoo&gt;</c> or a component named
/// <c>&lt;HtmlCard&gt;</c> is never read as an <c>&lt;html&gt;</c>, and the <c>&lt;/html&gt;</c> that closes one
/// writes no attribute at all.
/// </para>
/// <para>
/// The app writes exactly one <c>&lt;html&gt;</c> today - the root of the document in <c>wwwroot/index.html</c>,
/// with <c>lang="en"</c> - and none of its components writes an <c>&lt;html&gt;</c> of its own, so the guard
/// finds no violations to allowlist, and the empty verdict is the right one to stand on.
/// </para>
/// </summary>
public class DocumentLanguageGuardTests
{
    /// <summary>The element this rule is about, as the tag spells it in lower case.</summary>
    private const string HtmlTag = "html";

    /// <summary>The attribute that names the language the document's words are written in.</summary>
    private const string LangAttribute = "lang";

    /// <summary>The one character that says a value is built at Razor runtime and is not this guard's to read.</summary>
    private const char Razor = '@';

    /// <summary>
    /// One <c>&lt;html&gt;</c> of the markup: the file to print, the line its tag opens on, its element name, and
    /// every <c>lang</c> the tag writes - once per occurrence, so a tag that writes the attribute twice is read
    /// twice and the empty one left beside a real one cannot hide.
    /// </summary>
    private sealed record HtmlElement(string File, int Line, string Element, List<string?> Langs)
    {
        /// <summary>True of the value this guard passes over: one Razor builds, which the markup cannot say more about.</summary>
        public static bool BuiltByRazor(string? value) =>
            value is not null && value.TrimStart().StartsWith(Razor);

        /// <summary>True when the tag writes no <c>lang</c> at all.</summary>
        public bool WritesNone => Langs.Count == 0;

        /// <summary>The <c>lang</c> attributes of the tag that say nothing: left bare, a pair of quotes, or
        /// nothing between them but whitespace. A value Razor builds is a value present.</summary>
        public List<string?> Silent => [.. Langs.Where(value => !BuiltByRazor(value) && string.IsNullOrWhiteSpace(value))];

        /// <summary>Why the guard refuses this element, one sentence per fault - empty when it names a language.
        /// Both faults are reported at once, so a fixer is not sent back for the second one by the next run.</summary>
        public List<string> Faults
        {
            get
            {
                var faults = new List<string>();

                if (WritesNone)
                {
                    faults.Add(
                        $"<{Element}> writes no {LangAttribute}: the document's words carry no language a reader or screen reader could take up");
                }

                foreach (var value in Silent)
                {
                    faults.Add(
                        $"<{Element}> writes {Written(value)}: a {LangAttribute} of nothing names no language, which is worse than the attribute left out");
                }

                return faults;
            }
        }

        /// <summary>The offence of the element, or <c>null</c> when it names its language.</summary>
        public string? Offence => Faults.Count == 0 ? null : string.Join("; ", Faults);

        /// <summary>Where the fault stands: the file as the walk prints it, the line the tag opens on, and why.</summary>
        public string Report => $"{File}:{Line} {Offence}";

        /// <summary>The attribute as the tag wrote it, value and all.</summary>
        private static string Written(string? value) =>
            value is null ? $"{LangAttribute} with no value at all" : $"{LangAttribute}=\"{value}\"";
    }

    /// <summary>One attribute of a tag: its name as written and the value after it, if any - <c>null</c> when the
    /// attribute is written bare, which HTML reads as an empty value.</summary>
    private sealed record TagAttribute(string Name, string? Value);

    /// <summary>
    /// Every <c>&lt;html&gt;</c> of the app declares a <c>lang</c> that holds a language: the attribute stands on
    /// the tag itself, and it holds a literal of words or one Razor builds.
    /// </summary>
    [Fact]
    public void EveryHtmlTagDeclaresALanguage()
    {
        Assert.Empty(ReportsOf(HtmlTags()));
    }

    /// <summary>
    /// A guard that found nothing has seen nothing: the app writes its one document in
    /// <c>wwwroot/index.html</c>, whose <c>&lt;html&gt;</c> opens on the second line under the doctype and says
    /// <c>lang="en"</c>, and it writes forty-four <c>.razor</c> files under <c>src/Dashboard</c>, none of them an
    /// <c>&lt;html&gt;</c>. A scan that reads far fewer files, that misses the tag, or that reads a file that is
    /// not the dashboard's document would pass the verdict above on nothing it examined; the floors and the
    /// markers below are set so the test says what it read.
    /// </summary>
    [Fact]
    public void TheGuardReadsTheDocumentOfTheApp()
    {
        var files = RazorMarkup().ToList();
        var tags = HtmlTags();

        Assert.True(
            files.Count >= 30,
            $"only {files.Count} .razor file(s) were read under {DashboardRoot()}: the walk does not reach the app");

        // The walk reaches the app at its root and in its components, the two places its components live.
        Assert.Contains(files, file => file.File.Equals("App.razor", StringComparison.Ordinal));
        Assert.Contains(files, file => file.File.Equals(Path.Combine("Components", "DeliveryCard.razor"), StringComparison.Ordinal));

        // One document, one <html>: the app keeps its document root in wwwroot/index.html alone, so a second tag
        // means the guard read the wrong file, or the app grew a second root this rule has not seen.
        Assert.Single(tags);

        var tag = tags[0];
        Assert.Equal(Path.Combine("wwwroot", "index.html"), tag.File);
        Assert.Equal(2, tag.Line);
        Assert.Equal(HtmlTag, tag.Element);

        // What the app writes is read as the app writes it: the document's one language today.
        Assert.Single(tag.Langs);
        Assert.Equal("en", tag.Langs[0]);

        // The file the guard read is the dashboard's document, not a stray: its doctype, its body, and the
        // script that boots the app stand in it.
        var document = File.ReadAllText(IndexPage());
        Assert.StartsWith("<!DOCTYPE html>", document, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<body", document, StringComparison.Ordinal);
        Assert.Contains("_framework/blazor.webassembly", document, StringComparison.Ordinal);
        Assert.Contains("</html>", document, StringComparison.Ordinal);
    }

    /// <summary>The rule refuses the <c>&lt;html&gt;</c> that writes no <c>lang</c>: nothing at all, the element
    /// padded with other attributes, and the same element spelled with the name in capitals, since HTML spells an
    /// element name the same whichever way its letters go.</summary>
    [Fact]
    public void TheRuleRefusesAnHtmlThatWritesNoLang()
    {
        Assert.Contains("no lang", OffenceOf("""<html></html>"""), StringComparison.Ordinal);
        Assert.Contains("no lang", OffenceOf("""<html class="doc"><body>the words</body></html>"""), StringComparison.Ordinal);
        Assert.Contains("<html>", OffenceOf("""<html><body>the words</body></html>"""), StringComparison.Ordinal);
        Assert.Contains("no lang", OffenceOf("<HTML><BODY>the words</BODY></HTML>"), StringComparison.Ordinal);
        Assert.Contains("with no value at all", OffenceOf("<html lang><body>the words</body></html>"), StringComparison.Ordinal);
        Assert.Contains(":2 ", ReportOf("""
            <!DOCTYPE html>
            <html>
            </html>
            """), StringComparison.Ordinal);
    }

    /// <summary>The rule refuses the <c>lang</c> that holds nothing: the attribute written bare, a pair of
    /// quotes, and a value of nothing but the whitespace a template drifts between them - the space, the tab, the
    /// line break and the non-breaking space, which a reader hears as silence.</summary>
    [Fact]
    public void TheRuleRefusesALangThatHoldsNothing()
    {
        Assert.Contains($"{LangAttribute}=\"\"", OffenceOf("""<html lang=""></html>"""), StringComparison.Ordinal);
        Assert.Contains("with no value at all", OffenceOf("<html lang></html>"), StringComparison.Ordinal);
        Assert.Contains($"{LangAttribute}=\"   \"", OffenceOf("""<html lang="   "></html>"""), StringComparison.Ordinal);
        Assert.Contains($"{LangAttribute}=\"\t\"", OffenceOf("<html lang=\"\t\"></html>"), StringComparison.Ordinal);
        Assert.Contains($"{LangAttribute}=\"\r\n  \"", OffenceOf("<html lang=\"\r\n  \"></html>"), StringComparison.Ordinal);
        Assert.Contains($"{LangAttribute}=\"\u00a0\"", OffenceOf("<html lang=\"\u00a0\"></html>"), StringComparison.Ordinal);
        Assert.Contains($"{LangAttribute}=\" \"", OffenceOf("<html lang=' '></html>"), StringComparison.Ordinal);
    }

    /// <summary>The rule accepts the languages the app actually writes and the ones it may: the bare language, a
    /// region after it, a script or variant after that, the single-quoted value, the name spelled in another
    /// case, the attribute standing among others, and every value Razor builds, which the component's own code
    /// writes and this guard does not guess at.</summary>
    [Fact]
    public void TheRuleAcceptsALangThatNamesALanguage()
    {
        var accepted = new[]
        {
            """<html lang="en"><body>the words</body></html>""",
            """<html lang="en-GB"></html>""",
            """<html lang="en-GB-oxendict"></html>""",
            """<html lang='fr'></html>""",
            "<html LANG=\"en\"></html>",
            """<html class="doc" lang="en"></html>""",
            """<html lang="@DocumentLanguage"></html>""",
        };

        foreach (var text in accepted)
        {
            var tags = Scan("Sample.razor", text);

            Assert.Single(tags);
            Assert.Null(tags[0].Offence);

            // The sample was read as the element this rule is about, not skipped past.
            Assert.Equal(HtmlTag, tags[0].Element);
        }

        // A literal is a literal, a Razor value is a value: the guard passes over only what the component's own
        // code builds.
        Assert.False(HtmlElement.BuiltByRazor("en"));
        Assert.True(HtmlElement.BuiltByRazor("@DocumentLanguage"));
    }

    /// <summary>Only the <c>&lt;html&gt;</c> element is read, and only by its own name: a <c>&lt;htmlfoo&gt;</c>
    /// opens with a longer name and is another element, a component named <c>&lt;HtmlCard&gt;</c> is a component,
    /// a <c>&lt;header&gt;</c> is the head of the body, the <c>&lt;/html&gt;</c> that closes one writes no
    /// attributes, and the <c>&lt;</c> of a Razor comparison opens nothing at all.</summary>
    [Fact]
    public void OnlyTheHtmlElementIsReadAsAnHtml()
    {
        var text = """
            <main>
                <htmlfoo class="not-a-document">a longer name is another element</htmlfoo>
                <HtmlCard Title="A component">a component of this app</HtmlCard>
                <header class="topbar">the head of the body</header>
                <div class="count">written @(items.Count < 10) times</div>
            </main>
            """;

        Assert.Empty(Scan("Sample.razor", text));

        // The closing tag is never read as a start tag.
        Assert.Empty(Scan("Sample.razor", "</html>"));
    }

    /// <summary>Only the attribute of that very name names the language: a <c>data-lang</c>, an <c>xml:lang</c>
    /// or a <c>lang-foo</c> is another attribute, and none of them rescues the <c>&lt;html&gt;</c> that writes no
    /// <c>lang</c> of its own - while the same name spelled in another case is the attribute, since HTML leaves a
    /// name case-free.</summary>
    [Fact]
    public void OnlyTheLangAttributeCounts()
    {
        var text = """
            <div>
                <html data-lang="en" xml:lang="en" lang-foo="en"></html>
                <html LANG="en"></html>
            </div>
            """;

        var tags = Scan("Sample.razor", text);

        Assert.Equal(2, tags.Count);
        Assert.Empty(tags[0].Langs);
        Assert.True(tags[0].WritesNone);
        Assert.Contains("no lang", tags[0].Offence!, StringComparison.Ordinal);
        Assert.Null(tags[1].Offence);
        Assert.Equal(3, tags[1].Line);
    }

    /// <summary>A tag that offends more than once is reported once per <c>lang</c>, so a fixer is sent back with
    /// the whole of the tag rather than one attribute at a time.</summary>
    [Fact]
    public void EveryFaultOfATagIsReported()
    {
        var tags = Scan("Sample.razor", """<html lang="" lang="   "><body>the words</body></html>""");

        Assert.Single(tags);
        Assert.Equal(2, tags[0].Silent.Count);
        Assert.Equal(2, tags[0].Faults.Count);
        Assert.Contains($"{LangAttribute}=\"\"", tags[0].Offence!, StringComparison.Ordinal);
        Assert.Contains($"{LangAttribute}=\"   \"", tags[0].Offence!, StringComparison.Ordinal);

        // The whole tag sits on one line, so both faults print the same place - the line the tag opened on.
        Assert.All(ReportsOf(tags), report => Assert.Contains(":1 ", report, StringComparison.Ordinal));
    }

    /// <summary>A tag ends at the "&gt;" that closes it, not at the first "&gt;" of the file: a class may hold a
    /// Razor ternary with its own quotes, a value may be one Razor builds, and the attributes may spread over
    /// lines of their own. Read short, the <c>lang</c> below the break would be outside the tag and the guard
    /// would cry over a document that quite clearly says its language - and read short the other way, a tag of
    /// several lines is reported where it opened.</summary>
    [Fact]
    public void TheTagReadRunsPastRazorAndLineBreaks()
    {
        var text = """
            <!DOCTYPE html>
            <html class="doc @(isDark ? "doc--dark" : null)"
                  lang="@DocumentLanguage">
                <body>the words</body>
            </html>
            """;

        var tags = Scan("Sample.razor", text);

        Assert.Single(tags);
        Assert.Equal(2, tags[0].Line);
        Assert.Equal(HtmlTag, tags[0].Element);
        Assert.Single(tags[0].Langs);
        Assert.Null(tags[0].Offence);

        // The report prints the line the tag opened on, not the line its lang sits on.
        Assert.Contains(":2 ", ReportOf("""
            <!DOCTYPE html>
            <html class="doc"
                  lang="">
            </html>
            """), StringComparison.Ordinal);
    }

    /// <summary>The report names the file the way the walk prints it and the line the tag opens on - the first of
    /// them - and reads every tag of a file on its own.</summary>
    [Fact]
    public void TheReportNamesTheFileAndTheLineOfTheTag()
    {
        var text = """
            <!DOCTYPE html>
            <html lang="">
                <body>the words</body>
            </html>
            """;

        var offenders = ReportsOf(Scan(Path.Combine("wwwroot", "index.html"), text));

        Assert.Single(offenders);
        Assert.StartsWith(Path.Combine("wwwroot", "index.html") + ":2 ", offenders[0], StringComparison.Ordinal);
        Assert.Contains($"{LangAttribute}=\"\"", offenders[0], StringComparison.Ordinal);
    }

    /// <summary>Every <c>&lt;html&gt;</c> of the app, in the order the files write them: the document first, then
    /// the components, so the one tag the app keeps in its document is the one read first.</summary>
    private static List<HtmlElement> HtmlTags() =>
        [.. Sources().SelectMany(source => Scan(source.File, source.Text))];

    /// <summary>The document and every component of the app, as the path the assertion prints and its text.</summary>
    private static IEnumerable<(string File, string Text)> Sources()
    {
        // The document itself comes first: the rule is about the page the app serves, and the walk of the
        // components is the guard against an <html> grown anywhere else in the app.
        yield return (Path.Combine("wwwroot", "index.html"), File.ReadAllText(IndexPage()));

        foreach (var (file, text) in RazorMarkup())
        {
            yield return (file, text);
        }
    }

    /// <summary>The <c>&lt;html&gt;</c> elements a piece of markup opens: every start tag whose element name is
    /// <c>html</c>, with the <c>lang</c> attributes that tag writes. The whole document and a sample of markup
    /// the tests above read the rule through are scanned the same way.</summary>
    private static List<HtmlElement> Scan(string file, string text)
    {
        var tags = new List<HtmlElement>();

        for (var at = HtmlAt(text, 0); at >= 0; at = HtmlAt(text, at + 1))
        {
            var tag = MarkupScan.TagOf(text, at);
            var element = NameOf(tag);

            if (element != HtmlTag)
            {
                continue;
            }

            List<string?> langs = [.. AttributesOf(tag)
                .Where(attribute => attribute.Name.Equals(LangAttribute, StringComparison.OrdinalIgnoreCase))
                .Select(attribute => attribute.Value)];

            tags.Add(new HtmlElement(file, LineOf(text, at), element, langs));
        }

        return tags;
    }

    /// <summary>Why the tags are refused, one line each: the file, the line of the tag and the attribute it
    /// lacks.</summary>
    private static List<string> ReportsOf(IEnumerable<HtmlElement> tags) =>
        [.. tags
            .Where(tag => tag.Offence is not null)
            .Select(tag => tag.Report)];

    /// <summary>The offence of the sample's first <c>&lt;html&gt;</c> - <c>null</c> when it names its language -
    /// refusing a sample that writes no html element at all, so a test cannot pass on markup the rule never read.</summary>
    private static string? OffenceOf(string text)
    {
        var tags = Scan("Sample.razor", text);

        Assert.NotEmpty(tags);

        return tags[0].Offence;
    }

    /// <summary>The report of the sample's first offending <c>&lt;html&gt;</c>, refusing a sample the rule finds
    /// nothing to say about.</summary>
    private static string ReportOf(string text)
    {
        var offenders = ReportsOf(Scan("Sample.razor", text));

        Assert.Single(offenders);

        return offenders[0];
    }

    /// <summary>The next <c>&lt;html</c> at or after <paramref name="from"/>, outside a quoted attribute value,
    /// or -1 when none follows. The name has to end there: <c>&lt;htmlfoo&gt;</c> is another element - and HTML
    /// spells the name the same whichever way its letters go, so <c>&lt;HTML&gt;</c> is read the same way.</summary>
    private static int HtmlAt(string text, int from)
    {
        var quoted = '\0';

        for (var at = from; at < text.Length; at++)
        {
            var c = text[at];

            if (quoted != '\0')
            {
                if (c == quoted)
                {
                    quoted = '\0';
                }
            }
            else if (c is '"' or '\'')
            {
                quoted = c;
            }
            else if (c == '<'
                     && at + 6 <= text.Length
                     && string.Equals(text[(at + 1)..(at + 5)], HtmlTag, StringComparison.OrdinalIgnoreCase)
                     && (char.IsWhiteSpace(text[at + 5]) || text[at + 5] == '>'))
            {
                return at;
            }
        }

        return -1;
    }

    /// <summary>The element a tag opens, in lower case: HTML spells an element name the same whichever way its
    /// letters go, and a <c>&lt;HTML&gt;</c> owes a language just the same.</summary>
    private static string NameOf(string tag)
    {
        var end = 1;

        while (end < tag.Length && (char.IsLetterOrDigit(tag[end]) || tag[end] is '-' or '_' or ':'))
        {
            end++;
        }

        return tag[1..end].ToLowerInvariant();
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
            at = MarkupScan.SkipSpace(tag, at);

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

            at = MarkupScan.SkipSpace(tag, at);

            string? value = null;

            if (at < tag.Length && tag[at] == '=')
            {
                at = MarkupScan.SkipSpace(tag, at + 1);
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
        var close = MarkupScan.EndOfValue(tag, open, quote);

        at = close < tag.Length ? close + 1 : close;

        return close <= open + 1 ? string.Empty : tag[(open + 1)..close];
    }

    private static int LineOf(string text, int at) => text.AsSpan(0, at).Count('\n') + 1;

    private static string DashboardRoot([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard"));

    private static string IndexPage([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "wwwroot", "index.html"));

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
