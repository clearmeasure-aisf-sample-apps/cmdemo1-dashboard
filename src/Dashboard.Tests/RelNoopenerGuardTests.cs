using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// A start tag that writes <c>target="_blank"</c> opens its address in a new tab, and a page opened in a new tab
/// can reach back into the dashboard that opened it: without <c>noopener</c> the opened page keeps a
/// <c>window.opener</c> it can use to navigate the dashboard to a phishing address, and a <c>window.open</c> of
/// its own opens yet another tab in the dashboard's name (#196). The rule, then, is narrow: every start tag of
/// the app that writes <c>target="_blank"</c> writes a <c>rel</c>, and that <c>rel</c> carries
/// <c>noopener</c> among its tokens.
/// <para>
/// A value Razor builds is a value present: <c>rel="@RelFor(link)"</c> is written by the component's own code,
/// and whether that code resolves to a <c>rel</c> with a <c>noopener</c> is its tests' business, not this
/// guard's - the markup cannot say more. Only the <c>@</c> that opens an expression at the start of the value
/// marks one; a literal that carries an <c>@</c> among its words is read as the literal it is.
/// </para>
/// <para>
/// The guard walks every start tag of every <c>.razor</c> file of the app - not only the anchor, since any
/// element may open a new tab - and refuses a <c>target="_blank"</c> without a <c>noopener</c> <c>rel</c>,
/// listing the file and the line the tag opens on, so the next new-tab tag added cannot be forgotten. The tag
/// is read to the "&gt;" that closes it, past the Razor expressions, the quoted values that carry a "&gt;" of
/// their own and the line breaks they spread over - the way <see cref="NewTabLinkGuardTests" /> and
/// <see cref="TimeElementGuardTests" /> read them - and its attributes are read as name/value pairs, so the
/// whole name is what counts: a <c>data-target</c> or a <c>targetblank</c> is another attribute, while
/// <c>TARGET</c> is this one, HTML leaving a name case-free. The keyword <c>_blank</c> is matched the way a
/// browser matches it, case-free, and a value of nothing but whitespace around it is the keyword it is.
/// </para>
/// <para>
/// The app today writes a literal <c>rel="noopener"</c> on every one of its new-tab tags, so the allowlist the
/// main test checks against holds nothing - and if a tag is added without it, the guard names the tag, and the
/// fix is the markup's <c>rel</c>, not an entry in the list.
/// </para>
/// </summary>
public class RelNoopenerGuardTests
{
    /// <summary>The attribute whose value says where the tag's address opens.</summary>
    private const string Target = "target";

    /// <summary>The attribute whose tokens guard the tab the tag opens.</summary>
    private const string Rel = "rel";

    /// <summary>The one token that severs the page it opens from the opener it came from.</summary>
    private const string Noopener = "noopener";

    /// <summary>The navigation override keyword that opens a new tab, the way a browser spells it.</summary>
    private const string Blank = "_blank";

    /// <summary>The one element the other guard of new-tab links reads: here only the element of the report,
    /// since the rule reaches every start tag.</summary>
    private const string Anchor = "a";

    /// <summary>The one character that says a value is built at Razor runtime and is not this guard's to read.</summary>
    private const char Razor = '@';

    /// <summary>
    /// The current violations the rule would refuse, keyed as <c>file:line</c> of the tag: none. Every start tag
    /// of the shipped app that opens a new tab writes a literal <c>rel="noopener"</c>, so the list holds nothing
    /// - and it must keep holding nothing, since the fix for a new violation is the markup's <c>rel</c>, not an
    /// entry in this list.
    /// </summary>
    private static readonly HashSet<string> AllowedViolations = new(StringComparer.Ordinal);

    /// <summary>
    /// One start tag of the markup that opens a new tab: the file to print, the line its tag opens on, its
    /// element name, and every <c>rel</c> the tag writes - once per occurrence, so a tag that writes the
    /// attribute twice is read twice and a <c>rel</c> without a <c>noopener</c> left beside a real one cannot
    /// hide.
    /// </summary>
    private sealed record TargetBlankTag(string File, int Line, string Element, List<string?> Rels)
    {
        /// <summary>True of the value this guard passes over: one Razor builds, which the markup cannot say more about.</summary>
        public static bool BuiltByRazor(string? value) =>
            value is not null && value.TrimStart().StartsWith(Razor);

        /// <summary>True when the value's tokens carry the guard, whichever case they spell it in and whatever
        /// else sits beside it.</summary>
        public static bool CarriesNoopener(string? value) =>
            value is not null
            && value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                .Any(token => token.Equals(Noopener, StringComparison.OrdinalIgnoreCase));

        /// <summary>True when the tag writes no <c>rel</c> at all.</summary>
        public bool WritesNoRel => Rels.Count == 0;

        /// <summary>The <c>rel</c> attributes of the tag that leave the opened page able to reach back: a
        /// literal that names no <c>noopener</c> - left bare, a pair of quotes, or a word that is not the guard.
        /// A value Razor builds is a value present.</summary>
        public List<string?> Unsafe =>
            [.. Rels.Where(value => !BuiltByRazor(value) && !CarriesNoopener(value))];

        /// <summary>Why the guard refuses this tag, one sentence per fault - empty when it severs its opener.
        /// Both faults are reported at once, so a fixer is not sent back for the second one by the next run.</summary>
        public List<string> Faults
        {
            get
            {
                var faults = new List<string>();

                if (WritesNoRel)
                {
                    faults.Add(
                        $"<{Element}> opens a new tab but writes no rel, so the page it opens keeps its opener and can reach back through window.opener");
                }

                foreach (var value in Unsafe)
                {
                    faults.Add(value is null
                        ? $"<{Element}> writes {Rel} with no value at all, which carries no {Noopener}"
                        : $"<{Element}> writes {Rel}=\"{value}\", which carries no {Noopener}");
                }

                return faults;
            }
        }

        /// <summary>The offence of the tag, or <c>null</c> when it severs its opener.</summary>
        public string? Offence => Faults.Count == 0 ? null : string.Join("; ", Faults);

        /// <summary>Where the fault stands: the file as the walk prints it, the line the tag opens on, and why.</summary>
        public string Report => $"{File}:{Line} {Offence}";
    }

    /// <summary>Every start tag the walk reads: the file to print, the line the tag opens on, the element name
    /// in lower case, and every attribute the tag writes.</summary>
    private sealed record StartTag(string File, int Line, string Element, List<TagAttribute> Attributes);

    /// <summary>One attribute of a tag: its name as written and the value after it, if any - <c>null</c> when
    /// the attribute is written bare, which HTML reads as an empty value.</summary>
    private sealed record TagAttribute(string Name, string? Value);

    /// <summary>
    /// Every start tag of the app that opens a new tab severs it from the opener: a <c>rel</c> stands on the tag
    /// itself, and the literal it writes - or the one Razor builds - carries <c>noopener</c>.
    /// </summary>
    [Fact]
    public void EveryTargetBlankTagCarriesANoopenerRel()
    {
        List<string> offenders = [.. ReportsOf(TargetBlankTags())
            .Where(report => !AllowedViolations.Contains(KeyOf(report)))];

        Assert.Empty(offenders);
    }

    /// <summary>
    /// An allowlist that names a violation the app no longer carries is a lie the guard tells itself: the list
    /// is exact, so an entry that matches no offender of the day is refused, and the fix of a violation is the
    /// markup's <c>rel</c> leaving the list, not the list outliving the markup.
    /// </summary>
    [Fact]
    public void TheAllowlistNamesNoViolationTheAppDoesNotCarry()
    {
        var carried = ReportsOf(TargetBlankTags()).Select(KeyOf).ToHashSet(StringComparer.Ordinal);
        List<string> stale = [.. AllowedViolations.Where(key => !carried.Contains(key))];

        Assert.Empty(stale);
    }

    /// <summary>
    /// A guard that found nothing has seen nothing: the app writes a <c>target="_blank"</c> on fifteen start
    /// tags over eleven components today - the links of <c>ViewLinks.razor</c>, <c>CodeCard.razor</c> and
    /// <c>DeepLink.razor</c>, the environment pages of <c>EnvironmentLinks.razor</c>, the cluster and work rows
    /// of <c>ClusterPods.razor</c>, <c>WorkRows.razor</c> and <c>App.razor</c> - on a markup of a thousand
    /// start tags, most of them not anchors at all. A scan that reaches far fewer, or that reads only the
    /// anchor, never read the markup, and the clean verdict above would mean nothing; the floors are set well
    /// below those counts, so the day the app gains a new-tab tag the test still says what it reads.
    /// </summary>
    [Fact]
    public void TheGuardReadsTheTargetBlankTagsOfTheApp()
    {
        var startTags = StartTags();
        var tags = TargetBlankTags();
        var files = tags.Select(tag => tag.File).Distinct(StringComparer.Ordinal).ToList();
        var anchors = startTags.Count(tag => tag.Element == Anchor);

        Assert.True(
            startTags.Count >= 500,
            $"only {startTags.Count} start tag(s) were read under {DashboardRoot()}: the guard is checking nothing");

        Assert.True(
            startTags.Count - anchors >= 100,
            $"the {startTags.Count} start tag(s) read hold only {anchors} anchor(s): the walk does not read every start tag");

        Assert.True(
            tags.Count >= 10,
            $"only {tags.Count} start tag(s) with target=\"{Blank}\" were read under {DashboardRoot()}: the guard reads the wrong place");

        Assert.True(
            files.Count >= 5,
            $"the {tags.Count} start tag(s) read live in {files.Count} file(s): the walk does not reach the app");

        Assert.Contains(tags, tag => tag.File.Equals("App.razor", StringComparison.Ordinal));
        Assert.Contains(tags, tag => tag.File.Equals(Path.Combine("Components", "DeepLink.razor"), StringComparison.Ordinal));

        Assert.All(tags, tag => Assert.True(tag.Offence is null, $"{tag.Report}: a start tag of the app opens a new tab without a {Noopener} rel"));
    }

    /// <summary>The rule refuses the start tag that opens a new tab and writes no <c>rel</c> at all: the anchor,
    /// another element, and the same anchor spelled with the names in capitals, since HTML leaves them
    /// case-free.</summary>
    [Fact]
    public void TheRuleRefusesATargetBlankTagWithoutARel()
    {
        Assert.Contains("writes no rel", OffenceOf("""<a href="https://example.org/repo" target="_blank">the repo</a>"""), StringComparison.Ordinal);
        Assert.Contains("writes no rel", OffenceOf("""<iframe src="https://example.org/frame" target="_blank" />"""), StringComparison.Ordinal);
        Assert.Contains("writes no rel", OffenceOf("""<A HREF="https://example.org/" TARGET="_BLANK">no rel at all</A>"""), StringComparison.Ordinal);

        // The report prints the line the tag opened on, not the first line of the file.
        Assert.Contains(":2 ", ReportOf("""
            <main>
                <a href="https://example.org/" target="_blank">no rel</a>
            </main>
            """), StringComparison.Ordinal);
    }

    /// <summary>The rule refuses the <c>rel</c> that names no <c>noopener</c>: the attribute written bare, a
    /// pair of quotes, a value of nothing but the whitespace a template drifts between them, and a token that is
    /// not the guard - in whichever case the name is spelled, since HTML leaves a name case-free.</summary>
    [Fact]
    public void TheRuleRefusesARelThatCarriesNoNoopener()
    {
        Assert.Contains($"{Rel}=\"\"", OffenceOf("""<a href="https://example.org/" target="_blank" rel="">no substance</a>"""), StringComparison.Ordinal);
        Assert.Contains($"{Rel}=\"noreferrer\"", OffenceOf("""<a href="https://example.org/" target="_blank" rel="noreferrer">no guard</a>"""), StringComparison.Ordinal);
        Assert.Contains($"{Rel}=\"   \"", OffenceOf("""<a href="https://example.org/" target="_blank" rel="   ">whitespace only</a>"""), StringComparison.Ordinal);
        Assert.Contains("with no value at all", OffenceOf("<a href='https://example.org/' target='_blank' rel>no value</a>"), StringComparison.Ordinal);
        Assert.Contains($"{Rel}=\"no-referrer\"", OffenceOf("""<a href="https://example.org/" target="_blank" REL="no-referrer">the name is case-free</a>"""), StringComparison.Ordinal);
    }

    /// <summary>The rule accepts the <c>rel</c> the app actually writes and the ones it may: the guard alone,
    /// tokens beside it, the token and the names in another case, and every value Razor builds - whether it
    /// opens with a bare expression or a parenthesised one carrying a quoted word of its own.</summary>
    [Fact]
    public void TheRuleAcceptsARelThatCarriesNoopener()
    {
        var accepted = new[]
        {
            """<a href="https://example.org/" target="_blank" rel="noopener">the guard</a>""",
            """<a href="https://example.org/" target="_blank" rel="noopener noreferrer">tokens beside it</a>""",
            """<a href="https://example.org/" target="_blank" rel="NOOPENER">the token is case-free</a>""",
            """<A HREF="https://example.org/" TARGET="_BLANK" Rel="noopener">the names are case-free</A>""",
            """<a href="https://example.org/" target="_blank" rel="@RelFor(link)">built by razor</a>""",
            """<a href="https://example.org/" target="_blank" rel="@("noopener " + extra)">built with a word in it</a>""",
            """<iframe src="https://example.org/frame" target="_blank" rel="noopener" />""",
            """<a href="https://example.org/" target=" _BLANK " rel="noopener">the keyword is case-free</a>""",
        };

        foreach (var text in accepted)
        {
            var tags = ScanTargets("Sample.razor", text);

            // The sample was read as a start tag that opens a new tab, not skipped past: the scan found it and
            // the rule found nothing to refuse.
            Assert.Single(tags);
            Assert.Null(tags[0].Offence);
        }

        // A value Razor builds is a value present, and a literal is a literal: the "@" of a word is not the
        // "@" that opens an expression.
        Assert.False(TargetBlankTag.BuiltByRazor("noopener"));
        Assert.True(TargetBlankTag.BuiltByRazor("@RelFor(link)"));
        Assert.True(TargetBlankTag.BuiltByRazor(" @(\"noopener \" + extra)"));
    }

    /// <summary>Only the value that says a new tab is read: the <c>_self</c> and the <c>_top</c> stay, a value
    /// Razor builds is not the markup to read, a word without the underscore is another word, and a
    /// <c>data-target</c> is another attribute.</summary>
    [Fact]
    public void OnlyTheLiteralBlankTargetIsRead()
    {
        var text = """
            <main>
                <a href="https://example.org/" target="_self">stays put</a>
                <a href="https://example.org/" target="_top">goes up</a>
                <a href="https://example.org/" target="@("_blank")">a value razor builds</a>
                <a href="https://example.org/" target="blank">no underscore</a>
                <a data-target="_blank" href="https://example.org/">another attribute</a>
            </main>
            """;

        Assert.Empty(ScanTargets("Sample.razor", text));

        // Any element may open a new tab, so the element read is whatever the tag opens: an iframe that writes
        // a target="_blank" and no rel is read and refused like an anchor.
        Assert.Contains("writes no rel", OffenceOf("""<iframe src="https://example.org/" target="_blank" />"""), StringComparison.Ordinal);
    }

    /// <summary>A tag ends at the "&gt;" that closes it, not at the first "&gt;" of the file: a title may hold a
    /// "&gt;" of its own inside the quotes Razor carries, and the attributes may spread over lines of their own.
    /// Read short, the <c>rel</c> below them would be outside the tag and the guard would cry over a tag that
    /// severs its opener - and read short the other way, a tag of several lines is reported where it opened.</summary>
    [Fact]
    public void TheTagReadRunsPastRazorAndLineBreaks()
    {
        var text = """
            <main>
                <a class="deep @Class"
                   href="@address.AbsoluteUri"
                   target="_blank"
                   title="@($"Checks > the last minute")"
                   rel="noopener">the guard</a>
                <a href="@url.AbsoluteUri"
                   target="_blank"
                   rel="noreferrer">not guarded</a>
            </main>
            """;

        var tags = ScanTargets("Sample.razor", text);

        Assert.Equal(2, tags.Count);

        Assert.Equal(2, tags[0].Line);
        Assert.Equal(Anchor, tags[0].Element);
        Assert.Null(tags[0].Offence);

        Assert.Equal(7, tags[1].Line);
        Assert.Contains($"{Rel}=\"noreferrer\"", tags[1].Offence!, StringComparison.Ordinal);
    }

    /// <summary>The report names the file the way the walk prints it and the line the tag opens on - the first
    /// of them - and reads every start tag of a file on its own.</summary>
    [Fact]
    public void TheReportNamesTheFileAndTheLineOfTheTag()
    {
        var text = """
            <div class="links">
                <a href="https://example.org/repo" target="_blank">no rel at all</a>
                <span>between the two</span>
                <A HREF="https://example.org/" TARGET="_BLANK" REL="noreferrer">no guard either</A>
            </div>
            """;

        var offenders = ReportsOf(ScanTargets(Path.Combine("Components", "Sample.razor"), text));

        Assert.Equal(2, offenders.Count);
        Assert.StartsWith(Path.Combine("Components", "Sample.razor") + ":2 ", offenders[0], StringComparison.Ordinal);
        Assert.Contains("writes no rel", offenders[0], StringComparison.Ordinal);
        Assert.Contains(":4 ", offenders[1], StringComparison.Ordinal);
        Assert.Contains($"{Rel}=\"noreferrer\"", offenders[1], StringComparison.Ordinal);
    }

    /// <summary>Every start tag of every component, in the order the files write them.</summary>
    private static List<StartTag> StartTags() =>
        [.. RazorMarkup().SelectMany(markup => Scan(markup.File, markup.Text))];

    /// <summary>Every start tag of the app that writes a <c>target="_blank"</c>, with the <c>rel</c> attributes
    /// the tag writes and all.</summary>
    private static List<TargetBlankTag> TargetBlankTags() =>
        [.. StartTags().Where(OpensNewTab).Select(NewTargetBlankTag)];

    /// <summary>The start tag read as the element this rule is about, with every <c>rel</c> it writes.</summary>
    private static TargetBlankTag NewTargetBlankTag(StartTag tag) =>
        new(tag.File, tag.Line, tag.Element, [.. tag.Attributes
            .Where(attribute => attribute.Name.Equals(Rel, StringComparison.OrdinalIgnoreCase))
            .Select(attribute => attribute.Value)]);

    /// <summary>True when the tag's <c>target</c> is the value that opens a new tab: the keyword as a browser
    /// reads it, case-free and up to the whitespace a value may drift in.</summary>
    private static bool OpensNewTab(StartTag tag) =>
        tag.Attributes
            .Where(attribute => attribute.Name.Equals(Target, StringComparison.OrdinalIgnoreCase))
            .Select(attribute => attribute.Value)
            .Any(value => value is not null && value.Trim().Equals(Blank, StringComparison.OrdinalIgnoreCase));

    /// <summary>The start tags a piece of markup opens: every opening tag, with its element name in lower case
    /// and its attributes. A whole component and a sample of markup the tests above read the rule through are
    /// scanned the same way.</summary>
    private static List<StartTag> Scan(string file, string text)
    {
        var tags = new List<StartTag>();

        foreach (var start in OpeningTags(text))
        {
            var tag = TagOf(text, start);

            tags.Add(new StartTag(file, LineOf(text, start), NameOf(tag), AttributesOf(tag)));
        }

        return tags;
    }

    /// <summary>The start tags of the sample that open a new tab - refusing nothing, only reading, so a sample
    /// that writes no new-tab tag is read as nothing and the tests above say so.</summary>
    private static List<TargetBlankTag> ScanTargets(string file, string text) =>
        [.. Scan(file, text).Where(OpensNewTab).Select(NewTargetBlankTag)];

    /// <summary>Why the tags are refused, one line each: the file, the line of the tag and the <c>rel</c> it
    /// lacks.</summary>
    private static List<string> ReportsOf(IEnumerable<TargetBlankTag> tags) =>
        [.. tags
            .Where(tag => tag.Offence is not null)
            .Select(tag => tag.Report)];

    /// <summary>The <c>file:line</c> of a report, the key the allowlist holds.</summary>
    private static string KeyOf(string report) =>
        report[..report.IndexOf(' ', StringComparison.Ordinal)];

    /// <summary>The offence of the sample's first new-tab start tag - <c>null</c> when it severs its opener -
    /// refusing a sample that writes no new-tab tag at all, so a test cannot pass on markup the rule never
    /// read.</summary>
    private static string? OffenceOf(string text)
    {
        var tags = ScanTargets("Sample.razor", text);

        Assert.NotEmpty(tags);

        return tags[0].Offence;
    }

    /// <summary>The report of the sample's first offending new-tab start tag, refusing a sample the rule finds
    /// nothing to say about.</summary>
    private static string ReportOf(string text)
    {
        var offenders = ReportsOf(ScanTargets("Sample.razor", text));

        Assert.Single(offenders);

        return offenders[0];
    }

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

    /// <summary>The next "&lt;" that opens or closes a tag at or after <paramref name="from"/>, or -1. A "&lt;"
    /// of Razor - <c>@(count &lt; 1)</c> - is followed by a space or a digit and opens nothing.</summary>
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
    /// letters go, and an <c>&lt;A&gt;</c> owes a <c>noopener</c> just the same.</summary>
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
    /// after the value. A pair of quotes with nothing between them is read as the empty string, and an
    /// attribute written without a value at all as <c>null</c>.</summary>
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

    /// <summary>How the whole opening tag is read, from its "&lt;" to the "&gt;" that closes it - or to the end
    /// of the file when nothing does. An attribute value is run through rather than read to its first quote,
    /// because Razor quotes inside it, and a title may hold a "&gt;" of its own between them. The parentheses
    /// of an expression outside any value are counted too, the way the other guards count them.</summary>
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

    /// <summary>The quote that closes the value opened at <paramref name="open"/>, or the end of the text when
    /// the value is never closed. A <c>@(</c> inside the value is read to the <c>)</c> that matches it, the
    /// string literals of the expression and their quotes skipped over.</summary>
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

    /// <summary>Every <c>.razor</c> file of the app, as the path the assertion prints and its text. The
    /// build's copies under <c>bin</c> and <c>obj</c> hold the same markup twice, so those directories are
    /// walked past.</summary>
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
