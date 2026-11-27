using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// Three attributes say something about a tag that a viewer cannot see for themselves, and all three say it in words:
/// <c>aria-label</c> names the element to a screen reader, <c>aria-description</c> tells the reader what to say about
/// it, and <c>title</c> writes the hint a mouse reader waits for. Written empty, they are worse than absent: an
/// <c>aria-label=""</c> stands over the button like a sign with nothing on it, and some browsers and readers let an
/// empty <c>title=""</c> draw a tooltip box of the height of a single space - a flicker of nothing where the
/// explanation should be. So every one of the three that a tag of this app writes owes it a word (#155).
/// <para>
/// A guard walks every component and refuses the attribute that holds nothing: no value at all, a pair of quotes, or
/// nothing between the quotes but whitespace - the space, the tab and the non-breaking space a template drifts in. A
/// value Razor builds is left alone, since what it comes out to belongs to the component's own tests:
/// <c>title="@PauseHelp"</c>, <c>aria-label="@($"Pin history of {HistoryOf}")"</c> and
/// <c>aria-description="@($"3 requests &gt; the last minute")"</c> are as named as the markup can say. Only the
/// <c>@</c> that opens an expression at the start of the value marks one; a literal that carries a word of its own,
/// <c>title="Built @when"</c>, is read as the words it begins with.
/// </para>
/// <para>
/// The tag is read to the "&gt;" that closes it, past the Razor expressions, the quoted values that carry a "&gt;" of
/// their own and the line breaks they spread over - the way <c>ImageNameGuardTests</c>, <c>LinkGuardTests</c> and
/// <c>FormControlNameGuardTests</c> read them - and its attributes are read as name/value pairs, so the whole name is
/// what counts: <c>data-title</c>, <c>subtitle</c>, <c>aria-labelfoo</c> and <c>@bind-title</c> are other attributes,
/// while <c>Title</c> on a component parameter is this one, HTML leaving an attribute name case-free. What a tag
/// writes twice is read twice, so an <c>aria-label=""</c> left behind beside a real one cannot hide.
/// </para>
/// <para>
/// The two aria attributes that point rather than speak - <c>aria-labelledby</c> and <c>aria-describedby</c> - are
/// another guard's business: <see cref="AriaReferenceGuardTests" /> walks them and refuses a pointer whose id the
/// file never writes.
/// </para>
/// </summary>
public class EmptyLabelGuardTests
{
    /// <summary>The attribute a reader hears as the name of the element it stands on.</summary>
    private const string AriaLabel = "aria-label";

    /// <summary>The attribute a reader hears as the words about the element, beyond its name.</summary>
    private const string AriaDescription = "aria-description";

    /// <summary>The attribute a mouse reader waits for the element to be hovered to be told.</summary>
    private const string Title = "title";

    /// <summary>The attributes this rule asks for a word in.</summary>
    private static readonly string[] LabelAttributes = [AriaLabel, AriaDescription, Title];

    /// <summary>The one character that says a value is built at Razor runtime and is not this guard's to read.</summary>
    private const char Razor = '@';

    /// <summary>
    /// One label of one component: the file to print, the line its tag opens on, the element that writes it, the
    /// attribute as spelled there and its value without the quotes - <c>null</c> when the tag writes the attribute
    /// bare, which HTML reads as nothing at all.
    /// </summary>
    private sealed record Label(string File, int Line, string Element, string Name, string? Value)
    {
        /// <summary>True of the attribute of that name, in whichever spelling the tag used.</summary>
        public bool Is(string attribute) => Name.Equals(attribute, StringComparison.OrdinalIgnoreCase);

        /// <summary>True when the value is one Razor builds, so nothing here can say whether it comes out empty.</summary>
        public bool BuiltByRazor => Value is not null && Value.TrimStart().StartsWith(Razor);

        /// <summary>True when the label says nothing: a bare attribute, a pair of quotes, or nothing between them but
        /// the spaces, tabs and line breaks a template drifts in. A value Razor builds says as much as the markup
        /// can tell, so it is never silent here.</summary>
        public bool SaysNothing => !BuiltByRazor && (Value is null || Value.Trim().Length == 0);

        /// <summary>Why the guard refuses this label, in the words a failure prints, or <c>null</c> when it holds a
        /// word of its own or one Razor builds.</summary>
        public string? Offence => SaysNothing
            ? $"<{Element}> writes {Written}: a label of nothing is read as no label at all, so a reader reaches the element and hears no name"
            : null;

        /// <summary>Where the fault stands: the file as the walk prints it, the line the tag opens on, and why.</summary>
        public string Report => $"{File}:{Line} {Offence}";

        /// <summary>The attribute as the tag wrote it, value and all.</summary>
        private string Written => Value is null ? $"{Name} with no value at all" : $"{Name}=\"{Value}\"";
    }

    /// <summary>One attribute of a tag: its name as written and its value without the quotes around it - <c>null</c>
    /// when the attribute is written bare, which HTML reads as an empty value.</summary>
    private sealed record TagAttribute(string Name, string? Value);

    /// <summary>
    /// Every <c>aria-label</c>, <c>aria-description</c> and <c>title</c> of every component holds a word: not left
    /// bare, not empty, not padded with nothing but whitespace.
    /// </summary>
    [Fact]
    public void EveryLabelSaysSomething()
    {
        Assert.Empty(ReportsOf(Labels()));
    }

    /// <summary>
    /// A guard that found nothing has seen nothing: the app writes thirty-one <c>aria-label</c> attributes over
    /// twenty components and a hundred and thirty <c>title</c> attributes over thirty of them, in the words it spells
    /// itself and in the ones Razor builds - so a scan reaching far fewer never read the markup, and the clean
    /// verdict above would mean nothing. The floors are set well below those counts, so the day the app gains a
    /// label the test still says what it reads.
    /// </summary>
    [Fact]
    public void TheGuardReadsTheLabelsOfTheApp()
    {
        var labels = Labels();
        var named = labels.Where(label => label.Is(AriaLabel)).ToList();
        var hinted = labels.Where(label => label.Is(Title)).ToList();

        Assert.True(
            named.Count >= 20,
            $"only {named.Count} {AriaLabel} attribute(s) were read under {DashboardRoot()}: the guard is checking nothing");

        Assert.True(
            hinted.Count >= 50,
            $"only {hinted.Count} {Title} attribute(s) were read under {DashboardRoot()}: the guard is checking nothing");

        Assert.True(
            named.Select(label => label.File).Distinct(StringComparer.Ordinal).Count() >= 8,
            $"the {named.Count} {AriaLabel} attribute(s) read live in {named.Select(label => label.File).Distinct(StringComparer.Ordinal).Count()} file(s): the walk does not reach the app");

        Assert.True(
            hinted.Select(label => label.File).Distinct(StringComparer.Ordinal).Count() >= 15,
            $"the {hinted.Count} {Title} attribute(s) read live in {hinted.Select(label => label.File).Distinct(StringComparer.Ordinal).Count()} file(s): the walk does not reach the app");

        // Both ways the app writes a label are reached: a value spelled out in the markup, which this rule reads,
        // and a value Razor builds, which it passes over.
        Assert.Contains(named, label => !label.BuiltByRazor);
        Assert.Contains(named, label => label.BuiltByRazor);
        Assert.Contains(hinted, label => !label.BuiltByRazor);
        Assert.Contains(hinted, label => label.BuiltByRazor);

        // The labels of the app are read as labels, and none of them gives the guard a reason to speak.
        Assert.All(labels, label => Assert.True(label.Offence is null, $"{label.Report}: a label of the app is refused"));
    }

    /// <summary>The rule refuses the label that holds nothing: the attribute written bare, a pair of quotes, and a
    /// value of nothing but the spaces a template drifts between them.</summary>
    [Fact]
    public void TheRuleRefusesALabelThatHoldsNothing()
    {
        var text = """
            <div class="card">
                <span aria-label></span>
                <nav aria-description="">the week</nav>
                <button class="icon" title>Pause</button>
                <a href="/work" title="   ">work</a>
                <time datetime="2026-05-02T08:00Z" aria-description=" ">2 May</time>
            </div>
            """;

        var offenders = ReportsOf(Scan("Sample.razor", text));

        Assert.Equal(5, offenders.Count);

        Assert.Contains(":2 ", offenders[0], StringComparison.Ordinal);
        Assert.Contains($"{AriaLabel} with no value at all", offenders[0], StringComparison.Ordinal);

        Assert.Contains(":3 ", offenders[1], StringComparison.Ordinal);
        Assert.Contains($"{AriaDescription}=\"\"", offenders[1], StringComparison.Ordinal);

        Assert.Contains(":4 ", offenders[2], StringComparison.Ordinal);
        Assert.Contains($"{Title} with no value at all", offenders[2], StringComparison.Ordinal);

        Assert.Contains(":5 ", offenders[3], StringComparison.Ordinal);
        Assert.Contains($"{Title}=\"   \"", offenders[3], StringComparison.Ordinal);

        Assert.Contains(":6 ", offenders[4], StringComparison.Ordinal);
        Assert.Contains($"{AriaDescription}=\" \"", offenders[4], StringComparison.Ordinal);

        // A label of nothing says the same thing in either spelling of the tag, quoted or not.
        Assert.Contains("a label of nothing", offenders[0], StringComparison.Ordinal);
    }

    /// <summary>A value that is nothing but whitespace is refused whatever kind of whitespace it is: the space, the
    /// tab, the line break a multi-line tag drifts in, and the non-breaking space a template copies out of a page -
    /// which <c>char.IsWhiteSpace</c> counts, since it draws a space and a reader hears nothing in it.</summary>
    [Fact]
    public void TheRuleRefusesALabelOfNothingButWhitespace()
    {
        Assert.Contains("a label of nothing", OffenceOf("<span aria-label=\" \">x</span>"), StringComparison.Ordinal);
        Assert.Contains("a label of nothing", OffenceOf("<span aria-label=\"\t \">x</span>"), StringComparison.Ordinal);
        Assert.Contains("a label of nothing", OffenceOf("<span title=\" \">x</span>"), StringComparison.Ordinal);
        Assert.Contains("a label of nothing", OffenceOf("<span title=\"\r\n   \">x</span>"), StringComparison.Ordinal);
        Assert.Contains("a label of nothing", OffenceOf("<span title=\"\u00a0\">x</span>"), StringComparison.Ordinal);
        Assert.Contains("a label of nothing", OffenceOf("<span aria-description=\" \u00a0 \">x</span>"), StringComparison.Ordinal);

        // A single-quoted value is a value like any other, and nothing in it.
        Assert.Contains("a label of nothing", OffenceOf("<span aria-label='\t'>x</span>"), StringComparison.Ordinal);
    }

    /// <summary>The rule accepts the labels the app actually writes: a word of its own, a value padded with spaces
    /// around a word, a word that carries punctuation, and the values Razor builds - a field, a call, an expression
    /// in parentheses, an interpolated string holding a "&gt;" of its own.</summary>
    [Fact]
    public void TheRuleAcceptsALabelThatSaysSomething()
    {
        var text = """
            <div class="card">
                <span aria-label="The week at a glance">x</span>
                <button title=" Pause the checks " aria-description="Pauses the polling of every node">Pause</button>
                <a href="/work" title="Work" aria-label="@WorkText.Title(Week)" aria-description="@Help">w</a>
                <p aria-label="@Text" aria-description="@($"3 requests > the last minute")" title="Built @when">p</p>
                <svg viewBox="0 0 8 8" aria-label="Legend of the runtime diagram" title="Legend" />
            </div>
            """;

        var labels = Scan("Sample.razor", text);

        Assert.Equal(11, labels.Count);
        Assert.Empty(ReportsOf(labels));

        // A Razor value is passed over whether it opens with a bare expression, a parenthesised one or an
        // interpolated string, and a literal is read by the word it begins with.
        Assert.Contains(labels, label => label.BuiltByRazor && label.Value == "@Text");
        Assert.Contains(labels, label => label.BuiltByRazor && label.Value!.Contains('>'));
        Assert.Contains(labels, label => label.Is(Title) && label.Value == "Built @when" && !label.BuiltByRazor);
    }

    /// <summary>Only the attribute of that very name is read: a <c>data-title</c> is no title, a <c>subtitle</c> is
    /// no title, an <c>aria-labelfoo</c> and an <c>aria-labeldescription</c> are no name, an <c>@bind-title</c> is
    /// another attribute again, and the two attributes that point at other markup rather than speak are
    /// <see cref="AriaReferenceGuardTests" /> to read. A name spelled with another case is the same attribute, since
    /// HTML leaves a name case-free - which is how a component's <c>Title=""</c> parameter is read too.</summary>
    [Fact]
    public void OnlyTheLabelsOwnNamesCount()
    {
        var text = """
            <main>
                <div data-title="" subtitle="" data-aria-label="" aria-labelfoo="" aria-labeldescription="">x</div>
                <span @bind-title="" aria-labelledby="work-title" aria-describedby="work-help">y</span>
                <abbr TITLE="Time to live">TTL</abbr>
                <Metric Title="" aria-Description=" ">
                    <p title="Held by the metric">note</p>
                </Metric>
            </main>
            """;

        var offenders = ReportsOf(Scan("Sample.razor", text));

        // Both faults of the component parameter, and nothing from the look-alikes above it.
        Assert.Equal(2, offenders.Count);
        Assert.Contains(":5 ", offenders[0], StringComparison.Ordinal);
        Assert.Contains("<Metric>", offenders[0], StringComparison.Ordinal);
        Assert.Contains("Title=\"\"", offenders[0], StringComparison.Ordinal);
        Assert.Contains(":5 ", offenders[1], StringComparison.Ordinal);
        Assert.Contains("aria-Description=\" \"", offenders[1], StringComparison.Ordinal);

        // The look-alikes were read as themselves, and their values never stood in for a label of this rule - while
        // the title spelled in capitals and the one the component hands down were read as titles.
        var read = Scan("Sample.razor", text);

        Assert.Equal(4, read.Count);
        Assert.Contains(read, label => label.Is(Title) && label.Value == "Time to live" && label.Line == 4);
        Assert.Contains(read, label => label.Is(Title) && label.Value == "Held by the metric" && label.Line == 6);
        Assert.DoesNotContain(read, label => label.Name.Contains("data-title", StringComparison.Ordinal));
        Assert.DoesNotContain(read, label => label.Name == "subtitle" || label.Name == "aria-labelfoo");
        Assert.DoesNotContain(read, label => label.Name == "aria-labeldescription" || label.Name == "@bind-title");
        Assert.DoesNotContain(read, label => label.Name.StartsWith("aria-labelledby", StringComparison.Ordinal));
    }

    /// <summary>A tag ends at the "&gt;" that closes it, not at the first "&gt;" of the file: a class may hold a
    /// comparison, a title a "&gt;" of its own inside the quotes Razor carries, an event handler an arrow whose
    /// "=&gt;" is read as text, and the attributes may spread over lines of their own. Read short, the label below
    /// them would be outside the tag and the guard would cry over a component that says quite clearly what it is -
    /// and read short the other way, a tag of several lines is reported where it opened.</summary>
    [Fact]
    public void TheTagReadRunsPastRazorAndLineBreaks()
    {
        var text = """
            <main>
                <a class="deep @(wide ? "deep--wide" : null)"
                   href="@checks.AbsoluteUri"
                   title="@($"3 requests > the last minute")"
                   aria-label="@($"Checks of {Name}")"
                   aria-description="Checks run every interval"
                   @onclick="() => _open = true">checks</a>
                <button class="icon @(paused ? "icon--still" : null)"
                        title="@PauseHelp"
                        aria-label="">Pause</button>
            </main>
            """;

        var labels = Scan("Sample.razor", text);

        // The first tag's own three labels, the second tag's two, and nothing from between them.
        Assert.Equal(5, labels.Count);

        var longTag = labels.Where(label => label.Line == 2).ToList();

        Assert.Equal(3, longTag.Count);
        Assert.All(longTag, label => Assert.Null(label.Offence));
        Assert.Contains(longTag, label => label.Is(Title) && label.Value!.Contains('>') && label.BuiltByRazor);
        Assert.Contains(longTag, label => label.Is(AriaLabel) && label.BuiltByRazor);
        Assert.Contains(longTag, label => label.Is(AriaDescription) && label.Value == "Checks run every interval");

        // The empty label of the tag that opened on the line below it is reported at that line, the first of them.
        var offenders = ReportsOf(labels);

        Assert.Single(offenders);
        Assert.Contains(":8 ", offenders[0], StringComparison.Ordinal);
        Assert.Contains("<button>", offenders[0], StringComparison.Ordinal);
    }

    /// <summary>A tag that offends more than once is reported once per label, so a fixer is sent back with the whole
    /// of the tag rather than one attribute at a time - even when one of them says something and the tag writes the
    /// same attribute twice.</summary>
    [Fact]
    public void EveryEmptyLabelOfATagIsReported()
    {
        var text = """
            <span title="The work of the week" title="" aria-label="" aria-description="   ">work</span>
            """;

        var offenders = Scan("Sample.razor", text).Where(label => label.Offence is not null).ToList();

        Assert.Equal(3, offenders.Count);
        Assert.Contains(offenders, label => label.Is(Title));
        Assert.Contains(offenders, label => label.Is(AriaLabel));
        Assert.Contains(offenders, label => label.Is(AriaDescription));

        // The whole tag sits on one line, so all three faults print the same place - the line the tag opened on.
        Assert.All(ReportsOf(Scan("Sample.razor", text)), report => Assert.Contains(":1 ", report, StringComparison.Ordinal));
    }

    /// <summary>The report names the file the way the walk prints it and the line the tag opens on - the first of
    /// them, not the line the empty label would have sat on.</summary>
    [Fact]
    public void TheReportNamesTheFileAndTheLineOfTheTag()
    {
        var text = """
            <div class="toolbar">
                <span
                      class="state"
                      aria-label="">waiting</span>
                <button title=" ">Pause</button>
            </div>
            """;

        var offenders = ReportsOf(Scan(Path.Combine("Components", "Sample.razor"), text));

        Assert.Equal(2, offenders.Count);
        Assert.StartsWith(Path.Combine("Components", "Sample.razor") + ":2 ", offenders[0], StringComparison.Ordinal);
        Assert.Contains("aria-label", offenders[0], StringComparison.Ordinal);
        Assert.Contains(":5 ", offenders[1], StringComparison.Ordinal);
        Assert.Contains("title", offenders[1], StringComparison.Ordinal);
    }

    /// <summary>Every label of every component, in the order the files write them.</summary>
    private static List<Label> Labels() =>
        [.. RazorMarkup().SelectMany(markup => Scan(markup.File, markup.Text))];

    /// <summary>The labels a piece of markup writes: every <c>aria-label</c>, <c>aria-description</c> and
    /// <c>title</c> of every start tag it opens, in the order the markup writes them.</summary>
    private static List<Label> Scan(string file, string text)
    {
        var labels = new List<Label>();

        foreach (var start in MarkupScan.OpeningTags(text))
        {
            var tag = MarkupScan.TagOf(text, start);
            var element = ElementOf(tag);

            foreach (var attribute in AttributesOf(tag))
            {
                if (LabelAttributes.Any(name => name.Equals(attribute.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    labels.Add(new Label(file, LineOf(text, start), element, attribute.Name, attribute.Value));
                }
            }
        }

        return labels;
    }

    /// <summary>Why the labels are refused, one line each: the file, the line of the tag and the empty attribute.</summary>
    private static List<string> ReportsOf(IEnumerable<Label> labels) =>
        [.. labels
            .Where(label => label.Offence is not null)
            .Select(label => label.Report)];

    /// <summary>The offence of the sample's first label - <c>null</c> when it holds a word - refusing a sample that
    /// writes no label at all, so a test cannot pass on markup the rule never read.</summary>
    private static string? OffenceOf(string text)
    {
        var scanned = Scan("Sample.razor", text);

        Assert.NotEmpty(scanned);

        return scanned[0].Offence;
    }

    /// <summary>The element a tag opens, as the tag spells it - the name a failure prints.</summary>
    private static string ElementOf(string tag)
    {
        var end = 1;

        while (end < tag.Length && (char.IsLetterOrDigit(tag[end]) || tag[end] is '-' or '_' or ':'))
        {
            end++;
        }

        return tag[1..end];
    }

    /// <summary>The attributes a tag writes, past its element name: each name as written and the value after it, if
    /// any. A quoted value is run through to the quote that closes it, Razor's own quotes included, and the "/" that
    /// opens a self-closing tag ends the tag rather than naming an attribute.</summary>
    private static List<TagAttribute> AttributesOf(string tag)
    {
        var attributes = new List<TagAttribute>();
        var at = 1;

        while (at < tag.Length && !char.IsWhiteSpace(tag[at]))
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
