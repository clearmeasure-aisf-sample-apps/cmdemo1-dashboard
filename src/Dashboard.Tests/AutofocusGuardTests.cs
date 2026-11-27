using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// <c>autofocus</c> is the attribute that moves the reader without asking. A page that loads, re-renders or polls its
/// way to a fresh tile takes the focus to whichever tag wrote it, and the keyboard user is teleported there while the
/// screen reader is still saying the heading it was on: the words that follow belong to a control nobody chose, the
/// tab order the viewer had built up in their head is gone, and on a page this one refreshes on a timer the jump can
/// happen again every few seconds, under the reader's own hands (#202). Nothing in this dashboard earns it: every
/// control is one the viewer reaches themselves - the buttons of the work filter group of <c>WorkItemsView.razor</c>,
/// the interval select of <c>App.razor</c>, the scroll regions of the tables - and each says what it is where it
/// stands, so a cursor placed there before anyone asked is a cursor in the wrong place.
/// <para>
/// So no start tag of this app writes <c>autofocus</c>, whatever value stands after it. A bare attribute, an empty
/// pair of quotes, the <c>"autofocus"</c> and <c>"true"</c> a template inherits from old markup, a value of nothing
/// but the whitespace a template drifts between the quotes, and a value Razor builds are all the same order to the
/// browser: HTML reads a bare attribute as the empty string, and the empty string focuses just as well as
/// <c>"true"</c>. Unlike the attributes other guards read, a value built at runtime is refused rather than passed
/// over - a flag that arrives from code can arrive set, and the guard cannot tell that it does not.
/// </para>
/// <para>
/// A guard walks every component and refuses a tag that carries the attribute, listing the file and the line the tag
/// opens on, so the next field added cannot steal the focus quietly. The tag is read to the <c>&gt;</c> that closes
/// it, past the Razor expressions, the quoted values that carry a <c>&gt;</c> of their own and the line breaks they
/// spread over - the way <see cref="TimeElementGuardTests" />, <see cref="ViewportGuardTests" /> and
/// <see cref="FocusableRegionGuardTests" /> read them - and its attributes are read as name/value pairs, so the whole
/// name is what counts: <c>data-autofocus</c>, <c>autofocus-on-load</c> and <c>x-autofocus</c> are other attributes,
/// while <c>AUTOFOCUS</c> is this one, HTML leaving a name case-free. The Razor bind form of it,
/// <c>@bind-autofocus</c>, is refused with the attribute it binds, since the tag that writes it writes the focus too.
/// The word among the words of a value or of prose - <c>class="field autofocus"</c>, a comment warning the next author
/// off - is not the attribute at all, and a component parameter spelled with the same name, <c>AutoFocus="true"</c>,
/// is: it is the tag of this app that carries it, and it reaches the markup as an attribute.
/// </para>
/// <para>
/// Markup that breaks the rule today would be held in <see cref="AllowedAsThingsStand" /> by file and line rather
/// than fixed here, since this test changes no production markup; the list is empty as things stand, and
/// <see cref="EveryAllowedViolationIsOneTheScanActuallyReads" /> refuses an entry the walk no longer finds, so a
/// permission cannot outlive the tag it excused or quietly cover the next one added at the same place.
/// </para>
/// </summary>
public class AutofocusGuardTests
{
    /// <summary>The attribute this rule is about, as the tag spells it in lower case.</summary>
    private const string Autofocus = "autofocus";

    /// <summary>The Razor bind form of the same attribute, which writes the focus it binds.</summary>
    private const string BoundAutofocus = "@bind-autofocus";

    /// <summary>The one character that opens a Razor expression, a bind or an event handler in an attribute name.</summary>
    private const char Razor = '@';

    /// <summary>The file name samples of markup are reported under.</summary>
    private const string SampleFile = "Sample.razor";

    /// <summary>
    /// Tags that break the rule as this is written, excused by file and line so the guard can go in without touching
    /// the markup. Empty today: no component writes <c>autofocus</c>, and no other guard's markup hints at it. An
    /// entry is stale - and refused - the moment the tag it excuses is fixed or removed, and the walk finding
    /// nothing to excuse while this list holds an entry is the same fault.
    /// </summary>
    private static readonly (string File, int Line)[] AllowedAsThingsStand = [];

    /// <summary>
    /// One start tag of the markup: the file to print, the line its tag opens on, its element name and every
    /// attribute it writes - once per occurrence, so a tag that writes <c>autofocus</c> twice is read twice and the
    /// second one cannot hide beside the first.
    /// </summary>
    private sealed record MarkupTag(string File, int Line, string Element, List<TagAttribute> Attributes)
    {
        /// <summary>The <c>autofocus</c> attributes of the tag, in any case, with any value - and the Razor bind form
        /// of it, which writes the same focus. A <c>data-autofocus</c> is another attribute and is left alone.</summary>
        public List<TagAttribute> Carried =>
            [.. Attributes.Where(attribute =>
                attribute.Name.Equals(Autofocus, StringComparison.OrdinalIgnoreCase)
                || attribute.Name.Equals(BoundAutofocus, StringComparison.OrdinalIgnoreCase))];

        /// <summary>The faults of the tag: one sentence per <c>autofocus</c> it writes - empty when the tag leaves
        /// the focus where the reader put it.</summary>
        public List<string> Faults => [.. Carried.Select(attribute => $"<{Element}> writes {Written(attribute)}: {Reason}")];

        /// <summary>The offence of the tag, or <c>null</c> when it leaves the focus where the reader put it. Both
        /// faults of a tag are reported at once, so a fixer is not sent back for the second one by the next run.</summary>
        public string? Offence => Faults.Count == 0 ? null : string.Join("; ", Faults);

        /// <summary>Where the fault stands: the file as the walk prints it, the line the tag opens on, and why.</summary>
        public string Report => $"{File}:{Line} {Offence}";

        /// <summary>Whether this tag's place is written into the allow-list below.</summary>
        public bool Excused => AllowedAsThingsStand.Contains((File, Line));

        /// <summary>The attribute as the tag wrote it: a bare one is named as HTML reads it, and a value - empty,
        /// whitespace, <c>"true"</c> or one Razor builds - is printed as it stands.</summary>
        private static string Written(TagAttribute attribute) =>
            attribute.Value is null
                ? $"{attribute.Name} with no value at all"
                : $"{attribute.Name}=\"{attribute.Value}\"";

        /// <summary>Why the attribute is refused, in the words a reader or a keyboard user would use.</summary>
        private const string Reason =
            "autofocus takes the page to this tag unasked, so the screen reader loses the words it was saying and a keyboard user loses the place they had tabbed to";
    }

    /// <summary>One attribute of a tag: its name as written and the value after it, if any - <c>null</c> when the
    /// attribute is written bare, which HTML reads as an empty value.</summary>
    private sealed record TagAttribute(string Name, string? Value);

    /// <summary>
    /// No start tag of any component writes <c>autofocus</c>, in any case, with any value. Markup that does today is
    /// excused by name in <see cref="AllowedAsThingsStand" />, never by a rule that reads less.
    /// </summary>
    [Fact]
    public void NoStartTagWritesAutofocus()
    {
        Assert.Empty(ReportsOf(Tags()));
    }

    /// <summary>
    /// A guard that found nothing has seen nothing: the walk reaches forty-four <c>.razor</c> files today -
    /// <c>App.razor</c>, <c>_Imports.razor</c> and the forty-two components under <c>Components</c> - and those files
    /// open over a thousand start tags carrying over a thousand and five hundred attributes: the <c>class</c> and
    /// <c>role</c> of <c>App.razor</c>, the <c>tabindex</c> and <c>scope</c> of <c>PodTable.razor</c>, the
    /// <c>datetime</c> of <c>DeliveryCard.razor</c>, the <c>@onclick</c>, <c>@onchange</c>, <c>@onkeydown</c> and
    /// <c>@key</c> a Razor template writes with an <c>@</c> in front of the name. A scan that reads far fewer never
    /// opened the markup, and the clean verdict above would mean nothing; the floors are set well below those counts,
    /// so the day the app gains a component the test still says what it reads.
    /// </summary>
    [Fact]
    public void TheGuardReadsTheMarkupOfTheApp()
    {
        var files = RazorMarkup();
        var tags = Tags();
        var attributes = tags.Sum(tag => tag.Attributes.Count);

        Assert.True(
            files.Count >= 35,
            $"only {files.Count} .razor file(s) were read under {DashboardRoot()}: the walk does not reach the app");

        Assert.Contains(files, file => file.File.Equals("App.razor", StringComparison.Ordinal));
        Assert.Contains(files, file => file.File.Equals("_Imports.razor", StringComparison.Ordinal));
        Assert.Contains(files, file => file.File.Equals(Path.Combine("Components", "PodTable.razor"), StringComparison.Ordinal));
        Assert.Contains(files, file => file.File.StartsWith("Components", StringComparison.Ordinal));
        Assert.DoesNotContain(files, file => file.File.Contains("obj", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(files, file => file.File.Contains("bin", StringComparison.OrdinalIgnoreCase));

        Assert.True(
            tags.Count >= 700,
            $"only {tags.Count} start tag(s) were read in {files.Count} .razor file(s): the guard is checking nothing");

        Assert.True(
            attributes >= 900,
            $"only {attributes} attribute(s) were read on {tags.Count} start tag(s): the attribute names are never parsed");

        // The attributes the app really writes are read as the attributes they are, the <c>@</c> names included - so
        // the empty verdict above is a verdict about names this parser does meet, not about markup it skips past.
        foreach (var name in new[] { "class", "role", "aria-label", "tabindex", "datetime", "scope" })
        {
            Assert.Contains(tags, tag => tag.Attributes.Any(attribute => attribute.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
        }

        Assert.Contains(tags, tag => tag.Attributes.Any(attribute => attribute.Name.StartsWith(Razor) && attribute.Name.Length > 1));

        Assert.All(tags, tag =>
        {
            Assert.NotEmpty(tag.File);
            Assert.NotEmpty(tag.Element);
            Assert.True(tag.Line >= 1, $"{tag.File}: a tag was read on line {tag.Line}");
        });

        // Nothing is excused as things stand, and the walk finds nothing to excuse.
        Assert.Empty(AllowedAsThingsStand);
        Assert.DoesNotContain(tags, tag => tag.Excused);
        Assert.All(tags, tag => Assert.Null(tag.Offence));
    }

    /// <summary>The rule refuses the <c>autofocus</c> in every form a tag may write it: the attribute bare, an empty
    /// pair of quotes, the words a template inherits from old markup, a value of nothing but whitespace, the name in
    /// another case, the attribute at the end of a long tag, a second one beside a first, and the same name as a
    /// component's parameter.</summary>
    [Fact]
    public void TheRuleRefusesATagThatWritesAutofocus()
    {
        Assert.Contains($"{Autofocus} with no value at all", OffenceOf("""<input autofocus>"""), StringComparison.Ordinal);
        Assert.Contains($"{Autofocus}=\"\"", OffenceOf("""<input autofocus="" />"""), StringComparison.Ordinal);
        Assert.Contains($"{Autofocus}=\"autofocus\"", OffenceOf("""<input autofocus="autofocus">"""), StringComparison.Ordinal);
        Assert.Contains($"{Autofocus}=\"true\"", OffenceOf("""<input type="search" autofocus="true" />"""), StringComparison.Ordinal);
        Assert.Contains($"{Autofocus}=\"   \"", OffenceOf("""<input autofocus="   ">"""), StringComparison.Ordinal);
        Assert.Contains($"{Autofocus}=\"\t\"", OffenceOf("<input autofocus=\"\t\">"), StringComparison.Ordinal);
        Assert.Contains($"{Autofocus}=\"\u00a0\"", OffenceOf("<input autofocus=\"\u00a0\">"), StringComparison.Ordinal);
        Assert.Contains("<input>", OffenceOf("""<input autofocus />"""), StringComparison.Ordinal);
        Assert.Contains("<input>", OffenceOf("""<INPUT AUTOFOCUS>"""), StringComparison.Ordinal);
        Assert.Contains("<input>", OffenceOf("""<Input class="field__input" AutoFocus />"""), StringComparison.Ordinal);
        Assert.Contains("<textarea>", OffenceOf("""<textarea class="note" rows="3" autofocus></textarea>"""), StringComparison.Ordinal);
        Assert.Contains("<select>", OffenceOf("""<select autofocus>"""), StringComparison.Ordinal);

        // The single-quoted value is a value like any other, and so is an empty one.
        Assert.Contains($"{Autofocus}=\"\"", OffenceOf("<input autofocus=''>"), StringComparison.Ordinal);

        // The last attribute of a long tag is read as well as the first, and a tag that offends twice is read twice.
        Assert.Contains($"{Autofocus}=\"true\"", OffenceOf("""<button type="button" class="button" title="Run the check now" @onclick="Run" autofocus="true">Check</button>"""), StringComparison.Ordinal);

        var twice = Scan(SampleFile, """<input autofocus autofocus="">""");

        Assert.Single(twice);
        Assert.Equal(2, twice[0].Faults.Count);
    }

    /// <summary>A value Razor builds is no excuse: the attribute arrives at the browser with whatever the code
    /// decided, and a flag that can be set can be set. The bind form of the name is refused with the attribute it
    /// binds, and the name written with the spaces a template drifts around the <c>=</c> is the same attribute.</summary>
    [Fact]
    public void TheRuleRefusesAnAutofocusWhoseValueComesFromCode()
    {
        Assert.Contains($"{Autofocus}=\"@shouldFocus\"", OffenceOf("""<input autofocus="@shouldFocus" />"""), StringComparison.Ordinal);
        Assert.Contains($"{Autofocus}=\"@Settings.Focus\"", OffenceOf("""<input class="field" autofocus="@Settings.Focus" />"""), StringComparison.Ordinal);
        Assert.Contains($"{Autofocus}=\"@(ShouldFocus())\"", OffenceOf("""<input autofocus="@(ShouldFocus())" />"""), StringComparison.Ordinal);
        Assert.Contains(BoundAutofocus, OffenceOf("""<SearchBox @bind-autofocus="true" />"""), StringComparison.Ordinal);
        Assert.Contains(Autofocus, OffenceOf("""<input autofocus = "" />"""), StringComparison.Ordinal);
    }

    /// <summary>The word <c>autofocus</c> is refused as an attribute name and passed as anything else: the value of
    /// another attribute, the prose of the page, the words of a comment, and the other attributes whose names merely
    /// carry it - a <c>data-autofocus</c>, an <c>autofocus-on-load</c>, an <c>x-autofocus</c> or an
    /// <c>@onfocus</c> handler are none of them the attribute this rule is about.</summary>
    [Fact]
    public void OnlyTheAutofocusOwnNameIsTheAttribute()
    {
        var accepted = new[]
        {
            """<input class="field autofocus" type="search" />""",
            """<input data-autofocus="true" />""",
            """<input autofocus-on-load="true" />""",
            """<input x-autofocus="" />""",
            """<input autofocus-foo="autofocus" />""",
            """<input @onfocus="OnFocus" @onkeydown="OnKey" />""",
            """<input title="Do not add autofocus here" />""",
            """<p>The old markup wrote autofocus, and this one does not.</p>""",
            """<div class="@(focus ? "field--focus" : null)">@Text</div>""",
            """<SearchBox Label="Filter" FocusOnLoad="false" />""",
            """<input />""",
            """<input type="search" aria-label="Filter" @bind-value="query" />""",
        };

        foreach (var text in accepted)
        {
            var tags = Scan(SampleFile, text);

            Assert.NotEmpty(tags);
            Assert.All(tags, tag => Assert.Null(tag.Offence));
        }

        // A word among the words of a Razor comment opens no tag at all: there is nothing here for the rule to read.
        Assert.Empty(Scan(SampleFile, "@* autofocus is refused, and this comment is the only place it is named *@"));

        // The near names were read as the other attributes they are, not missed altogether.
        var read = Scan(SampleFile, """<input data-autofocus="true" autofocus-foo="1" />""");

        Assert.Single(read);
        Assert.Equal(2, read[0].Attributes.Count);
        Assert.Equal(["data-autofocus", "autofocus-foo"], read[0].Attributes.Select(attribute => attribute.Name));
    }

    /// <summary>A tag ends at the <c>&gt;</c> that closes it, not at the first <c>&gt;</c> it meets, and a tag spread
    /// over lines of its own is reported where it opened: the line a fixer is sent to is the line of the tag. The
    /// attributes below a Razor comparison, an interpolated string holding a <c>&gt;</c> of its own and an arrow
    /// handler are still inside the tag, and the tag after this one is not.</summary>
    [Fact]
    public void TheTagReadRunsPastRazorAndLineBreaks()
    {
        var text = """
            <div class="filters">
                <input class="field__input @(late ? "field__input--late" : null)"
                       title="@($"3 checks > the last minute")"
                       @oninput="e => Query = e.Value?.ToString()"
                       autofocus />
                <label class="field">
                    <span>Interval</span>
                    <select value="@Seconds" />
                </label>
            </div>
            """;

        var offenders = ReportsOf(Scan(SampleFile, text));

        Assert.Single(offenders);
        Assert.StartsWith($"{SampleFile}:2 ", offenders[0], StringComparison.Ordinal);
        Assert.Contains("<input>", offenders[0], StringComparison.Ordinal);

        // The same markup with the attribute left out says nothing at all, so the read is not reporting the file for
        // finding the tag: four tags were read, none of them offended.
        var clean = Scan(SampleFile, text.Replace("autofocus ", string.Empty, StringComparison.Ordinal));

        Assert.Equal(5, clean.Count);
        Assert.Empty(ReportsOf(clean));
    }

    /// <summary>The report names the file the way the walk prints it and the line the tag opens on - the first of
    /// them, once per fault of that tag - so a failure points at the markup rather than at a count.</summary>
    [Fact]
    public void TheReportNamesTheFileAndTheLineOfTheTag()
    {
        var text = """
            <main>
                <h1>Dashboard</h1>
                <input autofocus />
                <span>between the two</span>
                <INPUT AUTOFOCUS="autofocus" />
            </main>
            """;

        var offenders = ReportsOf(Scan(Path.Combine("Components", "Sample.razor"), text));

        Assert.Equal(2, offenders.Count);
        Assert.StartsWith(Path.Combine("Components", "Sample.razor") + ":3 ", offenders[0], StringComparison.Ordinal);
        Assert.Contains("autofocus with no value at all", offenders[0], StringComparison.Ordinal);
        Assert.Contains("screen reader", offenders[0], StringComparison.Ordinal);
        Assert.Contains(":5 ", offenders[1], StringComparison.Ordinal);
        Assert.Contains("<input>", offenders[1], StringComparison.Ordinal);

        // The attribute is printed as the tag wrote it, name and all - the name here in capitals.
        Assert.Contains("AUTOFOCUS=\"autofocus\"", offenders[1], StringComparison.Ordinal);
    }

    /// <summary>The walk's own markup, injected with the attribute it refuses, is reported under the file the walk
    /// read and the line the tag opened on: the rule bites the shipped components, not only the samples above, and
    /// the same text as it stands says nothing at all.</summary>
    [Fact]
    public void TheRuleBitesTheMarkupTheWalkReads()
    {
        var page = RazorMarkup().Single(file => file.File.Equals("App.razor", StringComparison.Ordinal));
        var at = page.Text.IndexOf("<select", StringComparison.Ordinal);

        Assert.True(at >= 0, "App.razor writes no <select> to read the rule through");

        var line = LineOf(page.Text, at);
        var marked = page.Text.Insert(at + "<select".Length, " autofocus");

        var report = Assert.Single(ReportsOf(Scan(page.File, marked)));

        Assert.StartsWith($"App.razor:{line} ", report, StringComparison.Ordinal);
        Assert.Contains("<select>", report, StringComparison.Ordinal);
        Assert.Contains("autofocus with no value at all", report, StringComparison.Ordinal);
        Assert.Contains("screen reader", report, StringComparison.Ordinal);

        // The page as it ships offends nowhere, so the report is about the attribute and not about the file.
        Assert.Empty(ReportsOf(Scan(page.File, page.Text)));
    }

    /// <summary>An allow-list may only cover a violation the walk really reads: an entry left behind by a fixed tag
    /// would sit ready to excuse the next tag added at that file and line, and a violation that is not written down
    /// by name is reported whether or not the list holds other places.</summary>
    [Fact]
    public void EveryAllowedViolationIsOneTheScanActuallyReads()
    {
        var allowed = Tags()
            .Where(tag => tag.Offence is not null && tag.Excused)
            .Select(tag => $"{tag.File}:{tag.Line}")
            .ToList();

        var stale = AllowedAsThingsStand
            .Where(entry => !allowed.Contains($"{entry.File}:{entry.Line}"))
            .Select(entry => $"{entry.File}:{entry.Line}")
            .ToList();

        Assert.Empty(stale);

        // The excuse is held by file and line alone, so a sample of markup never lands on a real entry by accident.
        Assert.DoesNotContain(allowed, place => place.StartsWith(SampleFile, StringComparison.Ordinal));
    }

    /// <summary>Every start tag of every component, in the order the files write them.</summary>
    private static List<MarkupTag> Tags() =>
        [.. RazorMarkup().SelectMany(markup => Scan(markup.File, markup.Text))];

    /// <summary>The start tags a piece of markup opens: every <c>&lt;</c> that opens an element, with every
    /// attribute that tag writes. A whole component and a sample of markup the tests above read the rule through are
    /// scanned the same way, so the rule is proved against the markup as the walk meets it.</summary>
    private static List<MarkupTag> Scan(string file, string text)
    {
        var tags = new List<MarkupTag>();

        foreach (var start in MarkupScan.OpeningTags(text))
        {
            var tag = MarkupScan.TagOf(text, start);

            tags.Add(new MarkupTag(file, LineOf(text, start), NameOf(tag), AttributesOf(tag)));
        }

        return tags;
    }

    /// <summary>Why the tags are refused, one line each: the file, the line of the tag and the attribute it wrote -
    /// with the tags the allow-list excuses left out.</summary>
    private static List<string> ReportsOf(IEnumerable<MarkupTag> tags) =>
        [.. tags
            .Where(tag => tag.Offence is not null && !tag.Excused)
            .Select(tag => tag.Report)];

    /// <summary>The offence of the sample's first tag that carries one, refusing a sample that writes no
    /// <c>autofocus</c> at all so a test cannot pass on markup the rule never read.</summary>
    private static string OffenceOf(string text)
    {
        var offenders = Tags(text);

        Assert.NotEmpty(offenders);

        return offenders[0].Offence!;
    }

    /// <summary>The tags of a sample that offend.</summary>
    private static List<MarkupTag> Tags(string text) =>
        [.. Scan(SampleFile, text).Where(tag => tag.Offence is not null)];

    /// <summary>The element a tag opens, in lower case: HTML spells an element name the same whichever way its
    /// letters go, and a <c>&lt;INPUT&gt;</c> owes the reader an explanation just the same.</summary>
    private static string NameOf(string tag)
    {
        var end = 1;

        while (end < tag.Length && (char.IsLetterOrDigit(tag[end]) || tag[end] is '-' or '_' or ':'))
        {
            end++;
        }

        return tag[1..end].ToLowerInvariant();
    }

    /// <summary>The attributes a tag writes, past its element name: each name as written and the value after it, if
    /// any. A quoted value is run through to the quote that closes it, Razor's own quotes included, and the <c>/</c>
    /// that opens a self-closing tag ends the tag rather than naming an attribute.</summary>
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

    /// <summary>The value that stands at <paramref name="at"/>, read from its opening quote to the closing one that
    /// Razor does not trick the read with, or to the whitespace that ends a bare one. The index is left after the
    /// value. A pair of quotes with nothing between them is read as the empty string, and an attribute written
    /// without a value at all as <c>null</c>.</summary>
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

    /// <summary>Every <c>.razor</c> file of the app, as the path the assertion prints relative to
    /// <c>src/Dashboard</c> and its text. The build's copies under <c>bin</c> and <c>obj</c> hold the same markup
    /// twice, so those directories are walked past.</summary>
    private static List<(string File, string Text)> RazorMarkup()
    {
        var root = DashboardRoot();
        var files = new List<(string File, string Text)>();

        foreach (var path in Directory.EnumerateFiles(root, "*.razor", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
        {
            var relative = Path.GetRelativePath(root, path);

            if (relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(part =>
                    part.Equals("bin", StringComparison.OrdinalIgnoreCase) || part.Equals("obj", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            files.Add((relative, File.ReadAllText(path)));
        }

        return files;
    }
}
