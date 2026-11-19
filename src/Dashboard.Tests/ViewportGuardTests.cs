using System.Globalization;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// A viewport that is not written is a page that arrives on a phone at 980 CSS pixels wide: every tile of the
/// dashboard shrunk to a thumbnail, and the viewer left to pinch and drag through a layout that was drawn for a
/// desktop. <c>&lt;meta name="viewport" content="width=device-width, initial-scale=1.0"&gt;</c> is what says "lay the
/// page out at the width of this device", and it is the only place a static Blazor WebAssembly host page can say it -
/// no component, no style sheet, nothing that runs after the first paint. This guard reads the markup of the app the
/// way <see cref="TimeElementGuardTests" /> and <see cref="NewTabLinkGuardTests" /> read theirs - every
/// <c>.razor</c> and every <c>.html</c> under <c>src/Dashboard</c>, skipping the build's copies under <c>bin</c> and
/// <c>obj</c> - and reports the file and the line the tag opens on, so neither the start page nor a later component
/// can lose the line without a word (#198).
/// <para>
/// Two things are refused, and both are refused in the <c>content</c> of every viewport the walk finds. First, a
/// viewport that never says <c>width=device-width</c>: <c>initial-scale</c> alone still leaves the desktop width, and
/// a <c>content</c> left bare, written with no value at all, or holding nothing but the whitespace a template drifts
/// between the quotes, says nothing about the layout at all. Second, a viewport that takes the zoom back out of the
/// viewer's hands: <c>user-scalable=no</c> and <c>maximum-scale=1</c> - the same order in the two other spellings
/// browsers read, <c>user-scalable=0</c> and <c>maximum-scale=1.0</c> - pin the page at the size it first arrived
/// in, which is the one thing a small-text reader, a low-vision reader and anyone who wants to read a number in a
/// crowded tile cannot forgive. A <c>maximum-scale</c> whose value no browser can read as a number is refused for the
/// same reason as one that is too small: the guard cannot tell that it lets the viewer zoom, and a viewport it cannot
/// read is a viewport it cannot pass.
/// </para>
/// <para>
/// The attribute is matched whole and case-free, the way HTML reads it: a <c>data-viewport</c>, a <c>name="Viewport"</c>
/// (HTML leaves a value of a known keyword case-free too) and a <c>name = "viewport"</c> spread over spaces are the
/// meta this rule is about, while <c>&lt;meta name="color-scheme"&gt;</c> beside it is another meta altogether and is
/// passed by. A <c>content</c> Razor builds at runtime is refused rather than guessed at - the host page states its
/// viewport in words, and one that arrives from a field can arrive as <c>user-scalable=no</c>. A
/// <c>name="@which"</c> is not a viewport the guard pretends to recognise: the tag's own name is what opens it, and a
/// name no markup states is nothing this rule can call a viewport.
/// </para>
/// <para>
/// Markup that breaks the rule today is held in <see cref="AllowedAsThingsStand" /> by file and line rather than
/// fixed here, since this test changes no production markup; the list is empty as things stand, and
/// <see cref="EveryAllowedViolationIsOneTheScanActuallyReads" /> refuses an entry the walk no longer finds, so a
/// permission cannot outlive the tag it excused or quietly cover the next one added at the same place.
/// </para>
/// </summary>
public class ViewportGuardTests
{
    /// <summary>The element this rule is about, as the tag spells it in lower case.</summary>
    private const string MetaTag = "meta";

    /// <summary>The attribute that says which meta this is.</summary>
    private const string NameAttribute = "name";

    /// <summary>The attribute that says what the meta declares.</summary>
    private const string ContentAttribute = "content";

    /// <summary>The <c>name</c> this rule is about, as the keyword spells it.</summary>
    private const string Viewport = "viewport";

    /// <summary>The start page of the app: the one file that must write a viewport.</summary>
    private static readonly string HostPage = Path.Combine("wwwroot", "index.html");

    /// <summary>The file name samples of markup are reported under.</summary>
    private const string SampleFile = "Sample.html";

    /// <summary>
    /// Viewport tags that break the rule as this is written, excused by file and line so the guard can go in without
    /// touching the markup. Empty today: the start page writes <c>width=device-width, initial-scale=1.0</c> and
    /// locks nothing. An entry is stale - and refused - the moment the tag it excuses is fixed or removed.
    /// </summary>
    private static readonly (string File, int Line)[] AllowedAsThingsStand = [];

    /// <summary>
    /// One <c>&lt;meta name="viewport"&gt;</c> of the markup: the file to print, the line its tag opens on, whether
    /// the tag writes a <c>content</c> at all, and the value it wrote - <c>null</c> when the attribute is written bare
    /// with no value after it, an empty string when it is written with nothing between its quotes.
    /// </summary>
    private sealed record ViewportMeta(string File, int Line, bool WritesContent, string? Content)
    {
        /// <summary>True of a value this guard passes over rather than reads: one Razor builds at runtime.</summary>
        public static bool BuiltByRazor(string? value) =>
            value is not null && value.TrimStart().StartsWith('@');

        /// <summary>The <c>content</c> read as the directives it lists: each key as written, trimmed of the spaces a
        /// template drifts around the <c>=</c>, and its value - <c>null</c> for a directive written without one.</summary>
        public List<(string Key, string? Value)> Directives =>
            Content is null
                ? []
                : [.. Content
                    .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(part =>
                    {
                        var at = part.IndexOf('=');

                        return at < 0
                            ? (part.Trim(), (string?)null)
                            : (part[..at].Trim(), (string?)part[(at + 1)..].Trim());
                    })];

        /// <summary>True when the directives name the device's own width, in any case and with any spaces.</summary>
        public bool NamesTheDeviceWidth =>
            Directives.Any(d =>
                d.Key.Equals("width", StringComparison.OrdinalIgnoreCase)
                && d.Value is not null
                && d.Value.Equals("device-width", StringComparison.OrdinalIgnoreCase));

        /// <summary>Why the guard refuses this viewport, one sentence per fault - empty when it lets the page fit the
        /// device and the viewer fit their eyes. Every fault of the tag is reported at once, so a fixer is not sent
        /// back for the second one by the next run.</summary>
        public List<string> Faults
        {
            get
            {
                var faults = new List<string>();

                if (!WritesContent)
                {
                    faults.Add("writes no content at all, so the browser lays the page out at its own desktop width");

                    return faults;
                }

                if (Content is null)
                {
                    faults.Add($"writes {Written()} with no value at all, which says no more than no content at all");

                    return faults;
                }

                if (BuiltByRazor(Content))
                {
                    faults.Add($"writes {Written()} built by Razor, so no one can read the viewport before it ships");

                    return faults;
                }

                if (Directives.Count == 0)
                {
                    faults.Add($"writes {Written()} holding nothing but spaces, which says no more than no content at all");

                    return faults;
                }

                if (!NamesTheDeviceWidth)
                {
                    faults.Add($"writes {Written()} without width=device-width, so a phone gets the 980px page scaled down");
                }

                foreach (var directive in Directives)
                {
                    if (directive.Key.Equals("user-scalable", StringComparison.OrdinalIgnoreCase)
                        && directive.Value is not null
                        && (directive.Value.Equals("no", StringComparison.OrdinalIgnoreCase)
                            || directive.Value == "0"))
                    {
                        faults.Add($"writes {Written()} with user-scalable={directive.Value}, which takes pinch zoom out of the viewer's hands");
                    }

                    if (directive.Key.Equals("maximum-scale", StringComparison.OrdinalIgnoreCase) && LocksThePage(directive.Value))
                    {
                        faults.Add($"writes {Written()} with maximum-scale={Written(directive.Value)}, which pins the page at the size it arrived in");
                    }
                }

                return faults;
            }
        }

        /// <summary>The offence of the viewport, or <c>null</c> when the tag passes the rule.</summary>
        public string? Offence => Faults.Count == 0 ? null : string.Join("; ", Faults);

        /// <summary>Where the fault stands: the file as the walk prints it, the line the tag opened on, and why.</summary>
        public string Report => $"{File}:{Line} <{MetaTag} name=\"{Viewport}\"> {Offence}";

        /// <summary>Whether this tag's place is written into the allow-list above.</summary>
        public bool Excused => ViewportGuardTests.AllowedAsThingsStand.Contains((File, Line));

        /// <summary>The <c>content</c> as the tag wrote it, value and all.</summary>
        private string Written() =>
            Content is null ? ContentAttribute : $"{ContentAttribute}=\"{Content}\"";

        /// <summary>The value of a directive as the report prints it: <c>nothing</c> for one written bare.</summary>
        private static string Written(string? value) => value is null ? "nothing" : $"\"{value}\"";

        /// <summary>True when a <c>maximum-scale</c> takes the zoom away: a ceiling of 1 or below - <c>1</c>,
        /// <c>1.0</c>, <c>0.5</c>, which browsers read alike - the attribute written with no value at all, and a
        /// value no browser can read as a number, which this guard cannot tell lets the viewer zoom.</summary>
        private static bool LocksThePage(string? value) =>
            value is null
            || !double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var scale)
            || scale <= 1d;
    }

    /// <summary>Every <c>&lt;meta name="viewport"&gt;</c> of every markup file of the app fits the device's width and
    /// leaves the viewer their zoom. Markup that does not today is excused by name in
    /// <see cref="AllowedAsThingsStand" />, never by a rule that reads less.</summary>
    [Fact]
    public void EveryViewportFitsTheDeviceAndLetsTheViewerZoom()
    {
        var offenders = ReportsOf(Viewports());

        Assert.Empty(offenders);
    }

    /// <summary>
    /// The start page is the one file that owes the line: without a viewport of its own the dashboard opens on a
    /// phone as a shrunk desktop page, and no component can put it right later. It writes exactly one - a second
    /// <c>&lt;meta name="viewport"&gt;</c> is a second opinion browsers resolve by their own rules, not this page's.
    /// </summary>
    [Fact]
    public void TheStartPageWritesExactlyOneViewport()
    {
        var page = MarkupFiles().SingleOrDefault(f => f.File.Equals(HostPage, StringComparison.Ordinal));

        Assert.True(page.Text is not null, $"{HostPage} was not read under {DashboardRoot()}: the guard looks in the wrong place");

        var viewports = Scan(page.File, page.Text);

        Assert.Single(viewports);
        Assert.NotNull(viewports[0].Content);
        Assert.True(viewports[0].NamesTheDeviceWidth, $"{viewports[0].Report}");
        Assert.Null(viewports[0].Offence);
    }

    /// <summary>
    /// A guard that found nothing has seen nothing. The walk reaches forty-five markup files today - forty-four
    /// <c>.razor</c> components and pages plus the start page - and the start page writes the viewport the app ships
    /// with; a scan that reads far fewer files, or no viewport at all, never opened the markup and the clean verdict
    /// above would mean nothing. The floors are set well below those counts, so the day the app gains a component the
    /// test still says what it reads.
    /// </summary>
    [Fact]
    public void TheGuardReadsTheMarkupOfTheApp()
    {
        var files = MarkupFiles();
        var viewports = Viewports();

        Assert.True(
            files.Count >= 30,
            $"only {files.Count} markup file(s) were read under {DashboardRoot()}: the walk does not reach the app");

        Assert.Contains(files, f => f.File.Equals(HostPage, StringComparison.Ordinal));

        Assert.True(
            viewports.Count >= 1,
            $"no <{MetaTag} name=\"{Viewport}\"> was read in {files.Count} markup file(s): the guard is checking nothing");

        Assert.Contains(viewports, v => v.File.Equals(HostPage, StringComparison.Ordinal));

        // The page's other metas are read as the other metas they are: the guard is not matching on "<meta".
        Assert.DoesNotContain(viewports, v => (v.Content ?? string.Empty).Contains("color-scheme", StringComparison.Ordinal));
        Assert.All(viewports, v => Assert.False(string.IsNullOrEmpty(v.File)));

        // Nothing is excused as things stand, and the walk finds nothing to excuse.
        Assert.Empty(AllowedAsThingsStand);
        Assert.DoesNotContain(Viewports(), v => v.Excused);
    }

    /// <summary>The rule refuses the viewport that never names the device's width: the <c>content</c> left out, the
    /// <c>content</c> written bare with no value at all, a pair of quotes, a value of nothing but whitespace, the
    /// <c>initial-scale</c> alone, and a width of anything else.</summary>
    [Fact]
    public void TheRuleRefusesAViewportThatNeverNamesTheDeviceWidth()
    {
        Assert.Contains("writes no content", OffenceOf("""<meta name="viewport" />"""), StringComparison.Ordinal);
        Assert.Contains("with no value at all", OffenceOf("<meta name=viewport content>"), StringComparison.Ordinal);
        Assert.Contains("content=\"\" holding nothing", OffenceOf("""<meta name="viewport" content="" />"""), StringComparison.Ordinal);
        Assert.Contains("nothing but spaces", OffenceOf("""<meta name="viewport" content="  ">"""), StringComparison.Ordinal);
        Assert.Contains("without width=device-width", OffenceOf("""<meta name="viewport" content="initial-scale=1.0" />"""), StringComparison.Ordinal);
        Assert.Contains("without width=device-width", OffenceOf("""<meta name="viewport" content="width=1024, initial-scale=1.0" />"""), StringComparison.Ordinal);
        Assert.Contains("built by Razor", OffenceOf("""<meta name="viewport" content="@ViewportText" />"""), StringComparison.Ordinal);
    }

    /// <summary>The rule refuses the viewport that takes the zoom back: <c>user-scalable=no</c> and the
    /// <c>user-scalable=0</c> browsers read as the same order, <c>maximum-scale=1</c> and the
    /// <c>maximum-scale=1.0</c> that means it, a <c>maximum-scale</c> below 1, and a <c>maximum-scale</c> no browser
    /// can read as a number at all. A tag that offends twice is reported twice, both faults in one line.</summary>
    [Fact]
    public void TheRuleRefusesAViewportThatLocksTheZoom()
    {
        var lockedShut = OffenceOf("""<meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no" />""");

        Assert.Contains("user-scalable=no", lockedShut, StringComparison.Ordinal);
        Assert.Contains("user-scalable", OffenceOf("""<meta name="viewport" content="width=device-width, USER-SCALABLE=NO" />"""), StringComparison.Ordinal);
        Assert.Contains("pinch zoom", OffenceOf("""<meta name="viewport" content="width=device-width, user-scalable=0" />"""), StringComparison.Ordinal);
        Assert.Contains("maximum-scale=\"1\"", OffenceOf("""<meta name="viewport" content="width=device-width, maximum-scale=1" />"""), StringComparison.Ordinal);
        Assert.Contains("maximum-scale", OffenceOf("""<meta name="viewport" content="width=device-width, maximum-scale=1.0" />"""), StringComparison.Ordinal);
        Assert.Contains("maximum-scale", OffenceOf("""<meta name="viewport" content="width=device-width, maximum-scale=0.5" />"""), StringComparison.Ordinal);
        Assert.Contains("maximum-scale", OffenceOf("""<meta name="viewport" content="width=device-width, maximum-scale" />"""), StringComparison.Ordinal);

        // Both faults at once, in one report: a locked zoom beside a missing width is not half an offence.
        var locked = Scan(SampleFile, """<meta name="viewport" content="maximum-scale=1, user-scalable=no" />""");

        Assert.Single(locked);
        Assert.Equal(3, locked[0].Faults.Count);
        Assert.Contains("width=device-width", locked[0].Offence!, StringComparison.Ordinal);
        Assert.Contains("user-scalable=no", locked[0].Offence!, StringComparison.Ordinal);
    }

    /// <summary>The rule accepts the viewport the app writes and the ones it may: the words in another order, the
    /// keyword in another case, the spaces and the single quotes a template drifts around the <c>=</c>, a zoom
    /// ceiling above 1, a <c>user-scalable=yes</c> that says the viewer keeps their pinch, and the other metas of
    /// the page - which are not this rule at all.</summary>
    [Fact]
    public void TheRuleAcceptsAViewportThatFitsTheDevice()
    {
        var accepted = new[]
        {
            """<meta name="viewport" content="width=device-width, initial-scale=1.0" />""",
            """<meta name="viewport" content="width = device-width , initial-scale = 1"/>""",
            """<meta NAME="VIEWPORT" CONTENT="width=device-width, initial-scale=1.0">""",
            """<meta name='viewport' content='width=device-width, minimum-scale=1, maximum-scale=5' >""",
            """<meta name="viewport" content="width=device-width, user-scalable=yes, maximum-scale=10.0" />""",
            """<meta name="viewport" content="width=device-width; initial-scale=1.0; maximum-scale=2.0" />""",
        };

        foreach (var text in accepted)
        {
            var viewports = Scan(SampleFile, text);

            Assert.Single(viewports);
            Assert.True(viewports[0].NamesTheDeviceWidth, viewports[0].Report);
            Assert.Null(viewports[0].Offence);
        }

        // The other metas of the page are passed by, and so is a Razor-built name, which this rule cannot call a
        // viewport: the tag's own name is what opens it.
        Assert.Empty(Scan(SampleFile, """<meta name="color-scheme" content="dark" />"""));
        Assert.Empty(Scan(SampleFile, """<meta name="viewport-width-helper" content="maximum-scale=1" />"""));
        Assert.Empty(Scan(SampleFile, """<meta name="@which" content="user-scalable=no" />"""));
    }

    /// <summary>The name and the content are read as the whole attributes they are: a <c>data-viewport</c> names
    /// nothing, a <c>content</c> of another meta beside a viewport leaves the viewport's own <c>content</c> what the
    /// rule reads, and the attribute written twice is read twice - the offending one cannot hide beside a good one.</summary>
    [Fact]
    public void OnlyTheViewportOwnAttributesCount()
    {
        var text = """
            <head>
                <meta data-viewport="width=device-width" content="user-scalable=no" />
                <meta name="color-scheme" content="dark" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            </head>
            """;

        var viewports = Scan(SampleFile, text);

        Assert.Single(viewports);
        Assert.Equal("width=device-width, initial-scale=1.0", viewports[0].Content);
        Assert.Null(viewports[0].Offence);
        Assert.Equal(4, viewports[0].Line);

        // A tag that writes the same content twice is read twice, so the locked one is reported.
        var twice = Scan(SampleFile, """<meta name="viewport" content="width=device-width" content="maximum-scale=1" />""");

        Assert.Single(twice);
        Assert.Contains("maximum-scale", twice[0].Offence!, StringComparison.Ordinal);
    }

    /// <summary>A tag ends at the <c>&gt;</c> that closes it, not at the first <c>&gt;</c> it meets, and a viewport
    /// spread over several lines is reported where it opened: the line a fixer is sent to is the line of the tag.</summary>
    [Fact]
    public void TheTagReadRunsPastQuotesAndLineBreaks()
    {
        var text = """
            <head>
                <meta
                      name="viewport"
                      content="width=device-width, initial-scale=1.0" />
                <meta name="viewport" content="width=device-width, maximum-scale=1" />
            </head>
            """;

        var viewports = Scan(SampleFile, text);

        Assert.Equal(2, viewports.Count);

        Assert.Equal(2, viewports[0].Line);
        Assert.Null(viewports[0].Offence);

        Assert.Equal(5, viewports[1].Line);
        Assert.Contains("maximum-scale", viewports[1].Offence!, StringComparison.Ordinal);
        Assert.Contains($"{SampleFile}:5 ", viewports[1].Report, StringComparison.Ordinal);
    }

    /// <summary>The report names the file the way the walk prints it and the line the tag opens on, so a failure
    /// points at the markup rather than at a count.</summary>
    [Fact]
    public void TheReportNamesTheFileAndTheLineOfTheTag()
    {
        var text = """
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1, user-scalable=no" />
            </head>
            """;

        var offenders = ReportsOf(Scan(Path.Combine("wwwroot", "Page.razor"), text));

        Assert.Single(offenders);
        Assert.StartsWith(Path.Combine("wwwroot", "Page.razor") + ":3 ", offenders[0], StringComparison.Ordinal);
        Assert.Contains("<meta name=\"viewport\">", offenders[0], StringComparison.Ordinal);
        Assert.Contains("maximum-scale", offenders[0], StringComparison.Ordinal);
        Assert.Contains("user-scalable=no", offenders[0], StringComparison.Ordinal);
    }

    /// <summary>An allow-list may only cover a violation the walk really reads: an entry left behind by a fixed tag
    /// would sit ready to excuse the next viewport added at that file and line.</summary>
    [Fact]
    public void EveryAllowedViolationIsOneTheScanActuallyReads()
    {
        var allowed = Viewports()
            .Where(v => v.Offence is not null && v.Excused)
            .Select(v => $"{v.File}:{v.Line}")
            .ToList();

        var stale = AllowedAsThingsStand
            .Where(entry => !allowed.Contains($"{entry.File}:{entry.Line}"))
            .Select(entry => $"{entry.File}:{entry.Line}")
            .ToList();

        Assert.Empty(stale);
    }

    /// <summary>Every viewport of every markup file, in the order the files write them.</summary>
    private static List<ViewportMeta> Viewports() =>
        [.. MarkupFiles().SelectMany(file => Scan(file.File, file.Text))];

    /// <summary>The <c>&lt;meta name="viewport"&gt;</c> tags a piece of markup writes: every start tag whose element
    /// is <c>meta</c> and whose <c>name</c> says <c>viewport</c>. A whole component, the start page and a sample of
    /// markup the tests above read the rule through are scanned the same way.</summary>
    private static List<ViewportMeta> Scan(string file, string text)
    {
        var viewports = new List<ViewportMeta>();

        foreach (var start in OpeningTags(text))
        {
            var tag = TagOf(text, start);

            if (NameOf(tag) != MetaTag)
            {
                continue;
            }

            var attributes = AttributesOf(tag);
            var name = ValueOf(attributes, NameAttribute);

            if (name is null || !name.Trim().Equals(Viewport, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            viewports.Add(new ViewportMeta(file, LineOf(text, start), WritesContent(attributes), ValueOf(attributes, ContentAttribute)));
        }

        return viewports;
    }

    /// <summary>Why the viewports are refused, one line each - the file, the line of the tag and what it wrote -
    /// with the tags the allow-list excuses left out.</summary>
    private static List<string> ReportsOf(IEnumerable<ViewportMeta> viewports) =>
        [.. viewports
            .Where(viewport => viewport.Offence is not null && !viewport.Excused)
            .Select(viewport => viewport.Report)];

    /// <summary>The offence of the sample's one viewport, refusing a sample that writes no viewport at all so a test
    /// cannot pass on markup the rule never read.</summary>
    private static string OffenceOf(string text)
    {
        var viewports = Scan(SampleFile, text);

        Assert.Single(viewports);
        Assert.NotNull(viewports[0].Offence);

        return viewports[0].Offence!;
    }

    /// <summary>The value a tag writes for an attribute, matched case-free the way HTML matches a name: the last
    /// occurrence wins, so a tag that writes the attribute twice is still read - and read again by
    /// <see cref="AttributesOf" />, which keeps both.</summary>
    private static string? ValueOf(IEnumerable<TagAttribute> attributes, string name) =>
        attributes
            .Where(attribute => attribute.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            .Select(attribute => attribute.Value)
            .LastOrDefault();

    /// <summary>True when the tag writes the attribute at all, whatever stands after it - the difference between a
    /// <c>content</c> of nothing and a <c>content</c> nobody wrote.</summary>
    private static bool WritesContent(IEnumerable<TagAttribute> attributes) =>
        attributes.Any(attribute => attribute.Name.Equals(ContentAttribute, StringComparison.OrdinalIgnoreCase));

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
    /// Razor or of prose - <c>@(count &lt; 1)</c>, "if &lt; 2px" - is followed by a space, a digit or punctuation and
    /// opens nothing.</summary>
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

    /// <summary>The element a tag opens, in lower case: HTML spells an element name the same whichever way its
    /// letters go, and a <c>&lt;META&gt;</c> owes a viewport just the same.</summary>
    private static string NameOf(string tag)
    {
        var end = 1;

        while (end < tag.Length && (char.IsLetterOrDigit(tag[end]) || tag[end] is '-' or '_' or ':'))
        {
            end++;
        }

        return tag[1..end].ToLowerInvariant();
    }

    /// <summary>Every <c>name</c>/<c>content</c> pair a tag writes, past its element name: each name as written and
    /// the value after it, if any. A quoted value is run through to the quote that closes it, Razor's own quotes
    /// included, and the "/" that opens a self-closing tag ends the tag rather than naming an attribute.</summary>
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

    /// <summary>The value that stands at <paramref name="at"/>, read from its opening quote to the closing one, or
    /// to the whitespace that ends a bare one. The index is left after the value. A pair of quotes with nothing
    /// between them is read as the empty string, and an attribute written without a value at all as <c>null</c>.</summary>
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

    /// <summary>How the whole opening tag is read, from its "&lt;" to the "&gt;" that closes it - or to the end of
    /// the file when nothing does. An attribute value is run through rather than read to its first quote, because
    /// Razor quotes inside it and a value may hold a "&gt;" of its own between them. The parentheses of an
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

    /// <summary>Every markup file of the app - each <c>.razor</c> component and page and each <c>.html</c> file the
    /// site is built from - as the path the assertion prints relative to <c>src/Dashboard</c> and its text. The
    /// build's copies under <c>bin</c> and <c>obj</c> hold the same markup twice, so those directories are walked
    /// past.</summary>
    private static List<(string File, string Text)> MarkupFiles()
    {
        var root = DashboardRoot();
        var files = new List<(string File, string Text)>();

        foreach (var pattern in new[] { "*.razor", "*.html" })
        {
            foreach (var path in Directory.EnumerateFiles(root, pattern, SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
            {
                var relative = Path.GetRelativePath(root, path);

                if (relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(part =>
                        part.Equals("bin", StringComparison.OrdinalIgnoreCase) || part.Equals("obj", StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                files.Add((relative, File.ReadAllText(path)));
            }
        }

        return files.OrderBy(f => f.File, StringComparer.Ordinal).ToList();
    }

    /// <summary>One attribute of a tag: its name as written and the value after it, if any - <c>null</c> when the
    /// attribute is written bare, which HTML reads as an empty value.</summary>
    private sealed record TagAttribute(string Name, string? Value);
}
