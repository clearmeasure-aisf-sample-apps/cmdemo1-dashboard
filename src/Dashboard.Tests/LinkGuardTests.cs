using System.Runtime.CompilerServices;
using System.Text;

namespace Dashboard.Tests;

/// <summary>
/// Two promises a link makes and a viewer cannot check for themselves: where it goes, and what it says it is.
/// <para>
/// An <c>&lt;a&gt;</c> with no <c>href</c> is no link at all: Tab walks past it, a reader says "text" rather than
/// "link", and a viewer who does not read the markup still sees whatever the tag holds and clicks it expecting a
/// page. An
/// <c>href="#"</c> is worse than honest: it looks like a link, and answers with a jump to the top of the page. An
/// <c>href="javascript:..."</c> is no address either - it runs script out of the markup, which this app does not do
/// from a tag and a viewer cannot tell is coming. So every link writes an <c>href</c> that holds something, is not a
/// bare <c>#</c>, and is no <c>javascript:</c> (#150). A value Razor builds - <c>href="@x.AbsoluteUri"</c>,
/// <c>href="@link.Href"</c> - is an address as far as the markup goes: which address it is belongs to the
/// component's own tests.
/// </para>
/// <para>
/// The other half is the name. A reader announces a link by its name, and the name is either what stands between the
/// tags or an <c>aria-label</c> / <c>aria-labelledby</c> written on the link itself. A link drawn from an icon
/// alone, an empty one, or one padded with <c>&amp;nbsp;</c>, is a viewer pressing "link" and hearing nothing but
/// "link"; a <c>title</c> is no answer, since it is announced only by some combinations of browser and reader -
/// which is why the new-tab links of <c>VersionLine.razor</c> and <c>WorkRows.razor</c> write an
/// <c>aria-label</c> beside their <c>title</c>. A name Razor builds - <c>@text</c>, <c>@ChildContent</c>,
/// <c>aria-label="@($"Pin history of {HistoryOf}")"</c> - counts as said, because whether it comes out empty is the
/// component's business, not the markup's.
/// </para>
/// <para>
/// A guard walks every component and refuses both faults at once, listing the file and the line the tag opens on, so
/// the next link added cannot be forgotten. The tag is read to the "&gt;" that closes it - past the Razor
/// expressions, the quoted values that carry a "&gt;" of their own and the line breaks they spread over, the way
/// <c>ImageNameGuardTests</c>, <c>MeterGuardTests</c> and <c>TabOrderAndGroupGuardTests</c> read them - its
/// attributes are read as name/value pairs, so <c>data-href</c> is never taken for <c>href</c> and <c>&lt;abbr&gt;</c>
/// and <c>&lt;article&gt;</c> are other elements; and its content runs to the <c>&lt;/a&gt;</c> that matches it, so a
/// word of the next link is never taken for this one's name.
/// </para>
/// </summary>
public class LinkGuardTests
{
    private const string AnchorElement = "a";
    private const string HrefAttribute = "href";

    /// <summary>The address that leads nowhere but back to the top of the page.</summary>
    private const string Placeholder = "#";

    /// <summary>The scheme that runs script instead of naming a place to go.</summary>
    private const string ScriptScheme = "javascript:";

    /// <summary>The attributes that name a link from outside its content.</summary>
    private static readonly string[] NameAttributes = ["aria-label", "aria-labelledby"];

    /// <summary>The words HTML renders as nothing a reader could hear, so a link padded with them stays silent.</summary>
    private static readonly string[] Blanks = ["&nbsp;", "&#160;", "&#xA0;", "&emsp;", "&thinsp;", "&zwnj;", "&zwj;", "&#8203;", "&#xFEFF;", "&shy;"];

    /// <summary>
    /// One link of one component: the file to print, the line its tag opens on, the tag itself, what stands between
    /// it and the <c>&lt;/a&gt;</c> that closes it (<c>null</c> when nothing does), and the attributes the tag writes.
    /// </summary>
    private sealed record Link(string File, int Line, string Tag, string? Content, IReadOnlyList<TagAttribute> Attributes)
    {
        /// <summary>True when the tag writes an <c>href</c> at all - bare or filled, which is another question.</summary>
        private bool WritesHref => Attributes.Any(attribute => attribute.Name.Equals(HrefAttribute, StringComparison.OrdinalIgnoreCase));

        /// <summary>The address as the tag spells it: the value of the first <c>href</c> written there.</summary>
        private string? Href => ValueOf(HrefAttribute);

        /// <summary>True when the tag is closed by a "/&gt;" of its own, which leaves it no content to be named by.</summary>
        private bool IsSelfClosed => Tag.EndsWith("/>", StringComparison.Ordinal);

        /// <summary>True when the link names itself: a non-empty <c>aria-label</c> or <c>aria-labelledby</c> of its
        /// own - a name Razor builds counts - or something between its tags that is not whitespace, be it words, a
        /// Razor expression such as <c>@ChildContent</c> or the text of a child element.</summary>
        private bool Speaks =>
            NameAttributes.Any(name => SaysSomething(ValueOf(name)))
            || (Content is not null && SaysWords(Content));

        /// <summary>Why the guard refuses this link, one sentence per fault - empty when it offends in neither way.
        /// Both rules answer at once, so a fixer is not sent back for the second one by the next run.</summary>
        public List<string> Faults
        {
            get
            {
                var faults = new List<string>();

                if (!WritesHref)
                {
                    faults.Add($"<{AnchorElement}> writes no {HrefAttribute}: it leads nowhere and a reader is not even told a link stands here");
                }
                else if (!SaysSomething(Href))
                {
                    faults.Add($"<{AnchorElement} {HrefAttribute}=\"{Href}\"> writes no address: it is drawn as a link and leads nowhere");
                }
                else if (Href!.Trim() == Placeholder)
                {
                    faults.Add($"<{AnchorElement} {HrefAttribute}=\"{Href}\"> is a bare \"{Placeholder}\": the link answers with a jump to the top of the page");
                }
                else if (Href!.Trim().StartsWith(ScriptScheme, StringComparison.OrdinalIgnoreCase))
                {
                    faults.Add($"<{AnchorElement} {HrefAttribute}=\"{Href}\"> runs script instead of leading to an address");
                }

                if (!Speaks)
                {
                    faults.Add(
                        IsSelfClosed
                            ? $"<{AnchorElement}> is closed by a \"/>\" of its own and names itself with neither a non-empty aria-label nor aria-labelledby, so a reader hears \"{AnchorElement}\" and nothing else"
                            : $"<{AnchorElement}> has no accessible name: no non-empty aria-label or aria-labelledby of its own and no words between its tags and its </{AnchorElement}>, so a reader hears \"{AnchorElement}\" and nothing else");
                }

                return faults;
            }
        }

        /// <summary>The offence of the link, or <c>null</c> when there is none.</summary>
        public string? Offence => Faults.Count == 0 ? null : string.Join("; ", Faults);

        /// <summary>Where the fault stands: the file as the walk prints it, the line the tag opens on, and why.</summary>
        public string Report => $"{File}:{Line} {Offence}";

        /// <summary>The value the tag's first attribute of that name holds, unquoted; <c>null</c> when the attribute
        /// is written without a value at all, and the name itself absent when the tag writes no such attribute.</summary>
        private string? ValueOf(string name) =>
            Attributes.FirstOrDefault(attribute => attribute.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value;
    }

    /// <summary>One attribute of a tag: its name as written and its value without the quotes around it - <c>null</c>
    /// when the attribute is written bare, which HTML reads as an empty value.</summary>
    private sealed record TagAttribute(string Name, string? Value);

    /// <summary>
    /// Every link of every component writes an <c>href</c> that leads somewhere and says what it is: the address is
    /// neither absent, empty, a bare <c>#</c> nor <c>javascript:</c>, and a reader is given words to hear.
    /// </summary>
    [Fact]
    public void EveryLinkLeadsSomewhereAndSaysWhatItIs()
    {
        Assert.Empty(ReportsOf(Links()));
    }

    /// <summary>
    /// A guard that found nothing has seen nothing: the app writes sixteen links today, from
    /// <c>ViewLinks.razor</c> and <c>DeepLink.razor</c> to the run of <c>CodeCard.razor</c> and the cluster of
    /// <c>AksServiceCard.razor</c>, so a scan that reaches far fewer never read the markup and the clean verdict
    /// above would mean nothing. The floor is set well below that count, so the day the app gains a link the test
    /// still says what it reads.
    /// </summary>
    [Fact]
    public void TheGuardReadsTheLinksOfTheApp()
    {
        var links = Links();
        var files = links.Select(link => link.File).Distinct(StringComparer.Ordinal).ToList();

        Assert.True(
            links.Count >= 10,
            $"only {links.Count} <{AnchorElement}> start tag(s) were read under {DashboardRoot()}: the guard is checking nothing");

        Assert.True(
            files.Count >= 5,
            $"the {links.Count} link(s) read under {DashboardRoot()} live in only {files.Count} file(s): the walk does not reach the app");

        Assert.Contains(links, link => link.File.Equals(Path.Combine("Components", "ViewLinks.razor"), StringComparison.Ordinal));
        Assert.Contains(links, link => link.File.Equals(Path.Combine("Components", "DeepLink.razor"), StringComparison.Ordinal));
        Assert.Contains(links, link => link.File.Equals(Path.Combine("Components", "EnvironmentLinks.razor"), StringComparison.Ordinal));

        // The links the app writes are read as links: every one of them carries an href of its own, and none of them
        // gives the guard a reason to speak.
        Assert.Contains(links, link => link.Tag.Contains($"{HrefAttribute}=\"@address.AbsoluteUri\"", StringComparison.Ordinal));
        Assert.All(links, link => Assert.True(link.Offence is null, $"{link.Report}: a link of the app is refused"));
    }

    /// <summary>The rule refuses the address that is no address: nothing written at all, an attribute left bare, a
    /// value of quotes, a value of spaces, the <c>@</c> of an expression that never came, the bare <c>#</c> - padded
    /// with spaces, which HTML trims just the same - and the scheme that runs script.</summary>
    [Fact]
    public void TheRuleRefusesALinkThatLeadsNowhere()
    {
        Assert.Contains("writes no href", OffenceOf("""<a target="_blank">Portal</a>"""), StringComparison.Ordinal);
        Assert.Contains("writes no address", OffenceOf("""<a href>Portal</a>"""), StringComparison.Ordinal);
        Assert.Contains("writes no address", OffenceOf("""<a href="">Portal</a>"""), StringComparison.Ordinal);
        Assert.Contains("writes no address", OffenceOf("""<a href="   ">Portal</a>"""), StringComparison.Ordinal);
        Assert.Contains("writes no address", OffenceOf("""<a href="@">Portal</a>"""), StringComparison.Ordinal);
        Assert.Contains("bare \"#\"", OffenceOf("""<a href="#">Portal</a>"""), StringComparison.Ordinal);
        Assert.Contains("bare \"#\"", OffenceOf("""<a href=" # ">Portal</a>"""), StringComparison.Ordinal);
        Assert.Contains("runs script", OffenceOf("""<a href="javascript:openPortal()">Portal</a>"""), StringComparison.Ordinal);
        Assert.Contains("runs script", OffenceOf("""<a href="JaVaScRiPt:alert(1)">Portal</a>"""), StringComparison.Ordinal);
        Assert.Contains("runs script", OffenceOf("""<a href=" javascript:openPortal() ">Portal</a>"""), StringComparison.Ordinal);
        Assert.Contains("runs script", OffenceOf("""<a HREF="JavaScript:alert(1)">Portal</a>"""), StringComparison.Ordinal);
    }

    /// <summary>The rule accepts the addresses the app actually writes and the ones HTML offers: an absolute address
    /// Razor builds, a link the component hands over, a page of the app, a section of the page, another fragment than
    /// the bare <c>#</c>, and the schemes that reach a person rather than a script.</summary>
    [Fact]
    public void TheRuleAcceptsALinkThatLeadsSomewhere()
    {
        Assert.Null(OffenceOf("""<a href="@address.AbsoluteUri">Portal</a>"""));
        Assert.Null(OffenceOf("""<a href="@link.Href">@link.Text</a>"""));
        Assert.Null(OffenceOf("""<a href="@($"{Base}/runs/{Run}")">Run</a>"""));
        Assert.Null(OffenceOf("""<a href="/work">Work</a>"""));
        Assert.Null(OffenceOf("""<a href="#events-title">Skip to the events</a>"""));
        Assert.Null(OffenceOf("""<a href="mailto:team@example.net">Write to the team</a>"""));
        Assert.Null(OffenceOf("""<a href="tel:+441234567890">Call the desk</a>"""));
        Assert.Null(OffenceOf("""<a href='https://portal.example.net/x'>Portal</a>"""));
    }

    /// <summary>The rule refuses the link that says nothing: nothing between its tags, whitespace only, an empty name
    /// written over the silence, a name of spaces, a pointer to nothing, an <c>aria-label</c> left bare, a name
    /// buried in a child, a <c>title</c> - which is not a name - and the blanks an <c>&amp;nbsp;</c> renders.</summary>
    [Fact]
    public void TheRuleRefusesALinkThatSaysNothing()
    {
        Assert.Contains("no accessible name", OffenceOf("""<a href="/work"></a>"""), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("<a href=\"/work\">   \n\t  </a>"), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("""<a href="/work">&nbsp;&nbsp;</a>"""), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("""<a href="/work">@* a comment is not read out *@</a>"""), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("""<a href="/work"><!-- nor is a comment --></a>"""), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("""<a href="/work"><svg viewBox="0 0 8 8" aria-hidden="true"><path d="M2 8h12" /></svg></a>"""), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("""<a href="/work" title="The work of the week"></a>"""), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("""<a href="/work" aria-label="" aria-labelledby=""></a>"""), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("""<a href="/work" aria-label="   " aria-labelledby="   "> </a>"""), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("""<a href="/work" aria-label></a>"""), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("""<a href="/work" data-aria-label="Not a name"></a>"""), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("""<a href="/work">@</a>"""), StringComparison.Ordinal);
        Assert.Contains("closed by a \"/>\"", OffenceOf("""<a class="icon" href="/work" aria-label="" />"""), StringComparison.Ordinal);

        // The name of a child names the child; the link is still silent, and so is a link the file leaves unclosed -
        // there is no "</a>" to look between, and the words of the markup after it are no answer.
        Assert.Contains("no accessible name", OffenceOf("""<a href="/work"><span aria-label="A child's own name"></span></a>"""), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("""<p><a href="/work"></p>"""), StringComparison.Ordinal);
    }

    /// <summary>The rule accepts the names the app writes: words of its own, a name Razor builds, a name handed in as
    /// child content, a visually-hidden span, the icon and the word drawn together, a name in either attribute and a
    /// name in either spelling of the tag.</summary>
    [Fact]
    public void TheRuleAcceptsALinkThatSaysWhatItIs()
    {
        Assert.Null(OffenceOf("""<a href="/work">Work</a>"""));
        Assert.Null(OffenceOf("""<a href="@address.AbsoluteUri">@text</a>"""));
        Assert.Null(OffenceOf("""<a class="deep" href="@address.AbsoluteUri">@ChildContent<span class="visually-hidden"> (opens in a new tab)</span></a>"""));
        Assert.Null(OffenceOf("""<a href="/work" aria-label="The work of the week"></a>"""));
        Assert.Null(OffenceOf("""<a href="/work" aria-label="@($"Pin history of {HistoryOf} (opens in a new tab)")"></a>"""));
        Assert.Null(OffenceOf("""<a href="/work" aria-labelledby="work-heading"></a>"""));
        Assert.Null(OffenceOf("""<a href="/work" aria-labelledby="@($"{IdPrefix}-work")"></a>"""));
        Assert.Null(OffenceOf("""<a href="/work" aria-label>Work</a>"""));
        Assert.Null(OffenceOf("""<A HREF="/work">Work</A>"""));
        Assert.Null(OffenceOf("""<a href="/work">@if (wide) { <span>wider</span> } else { <span>narrower</span> }</a>"""));
        Assert.Null(OffenceOf("""<a href="/work"><svg viewBox="0 0 8 8" aria-hidden="true"><path d="M2 8h12" /></svg>Checks</a>"""));
    }

    /// <summary>Only the attribute of that very name is the link's own: a <c>data-href</c> is no address, an
    /// <c>hreffoo</c> is no address, a <c>data-aria-label</c> is no name, and an element whose name merely begins
    /// with the letter <c>a</c> - <c>&lt;abbr&gt;</c>, <c>&lt;article&gt;</c>, <c>&lt;aside&gt;</c> - is no link at
    /// all, however little it says.</summary>
    [Fact]
    public void OnlyTheLinksOwnHrefAndNameCount()
    {
        var text = """
            <main>
                <abbr title="Time to live">TTL</abbr>
                <article>the week in one card</article>
                <aside data-aria-label="Not a name">notes</aside>
                <a data-href="/work" data-aria-label="Not an address, not a name">Work</a>
                <a hreffoo="/work" aria-labelfoo="Work">Work</a>
            </main>
            """;

        var links = Scan("Sample.razor", text);

        Assert.Equal(2, links.Count);
        Assert.All(links, link => Assert.Contains("writes no href", link.Offence!, StringComparison.Ordinal));
        Assert.All(links, link => Assert.DoesNotContain("no accessible name", link.Offence!, StringComparison.Ordinal));

        // An attribute of another name is read as itself, so its value never stands in for an href.
        Assert.Contains(links[0].Attributes, attribute => attribute.Name == "data-href");
    }

    /// <summary>A link that offends twice is reported twice over: an address that leads nowhere and a name of nothing
    /// are both written to the line a fixer reads, so the next run does not send them back for the second one.</summary>
    [Fact]
    public void EveryFaultOfALinkIsReported()
    {
        var scanned = Scan("Sample.razor", """<a href="#" aria-label=""></a>""");

        Assert.Single(scanned);
        Assert.Equal(2, scanned[0].Faults.Count);
        Assert.Contains("bare \"#\"", scanned[0].Offence!, StringComparison.Ordinal);
        Assert.Contains("no accessible name", scanned[0].Offence!, StringComparison.Ordinal);
    }

    /// <summary>A tag that writes its <c>href</c> twice answers with the first of them, the way a browser reads it -
    /// a bare <c>#</c> written before a real address is the fault reported, not the address that follows it.</summary>
    [Fact]
    public void TheFirstHrefOfATagIsItsAddress()
    {
        var scanned = Scan("Sample.razor", """<a href="#" href="https://portal.example.net/x">Portal</a>""");

        Assert.Single(scanned);
        Assert.Contains("bare \"#\"", scanned[0].Offence!, StringComparison.Ordinal);
        Assert.Single(scanned[0].Faults);
    }

    /// <summary>A tag ends at the "&gt;" that closes it, not at the first "&gt;" of the file: a class may hold a
    /// comparison, a title a "&gt;" of its own inside the quotes Razor carries, an event handler an arrow whose
    /// "=&gt;" is read as text, and the attributes - among them <c>href</c> - may spread over lines of their own.
    /// Read short, the <c>href</c> below them would be outside the tag and the guard would cry over a link that leads
    /// quite clearly somewhere.</summary>
    [Fact]
    public void TheTagReadRunsPastRazorAndLineBreaks()
    {
        var text = """
            <main>
                <a class="deep @(wide ? "deep--wide" : null)"
                   href="@checks.AbsoluteUri"
                   title="@($"3 requests > the last minute")"
                   aria-label="@($"Checks of {Name}")"
                   @onclick="() => _open = true">checks<span class="visually-hidden"> (opens in a new tab)</span></a>
                <a href="@history.AbsoluteUri">history</a>
            </main>
            """;

        var links = Scan("Sample.razor", text);

        Assert.Equal(2, links.Count);

        Assert.Equal(2, links[0].Line);
        Assert.Null(links[0].Offence);
        Assert.DoesNotContain("<span", links[0].Tag, StringComparison.Ordinal);
        Assert.Contains(links[0].Attributes, attribute => attribute.Name == "@onclick");
        Assert.Contains(links[0].Attributes, attribute => attribute.Name == "title" && attribute.Value!.Contains('>'));
        Assert.Contains("checks", links[0].Content!, StringComparison.Ordinal);

        Assert.Equal(7, links[1].Line);
        Assert.Null(links[1].Offence);
    }

    /// <summary>The content of a link runs to the <c>&lt;/a&gt;</c> that matches it and no further: a link is named
    /// by what stands in it, never by the words of the one written after it, and the tags it holds - a span of its
    /// own, a nested element closed over several lines - are read whole.</summary>
    [Fact]
    public void TheContentOfALinkEndsAtItsOwnClosingTag()
    {
        var text = """
            <p class="resource-links" role="group" aria-label="Links">
                <a href="/a">
                    <span>the first</span>
                </a>
                <a href="/b"></a>
                <a href="/c">the third</a>
            </p>
            """;

        var links = Scan("Sample.razor", text);

        Assert.Equal(3, links.Count);
        Assert.Null(links[0].Offence);
        Assert.Contains("the first", links[0].Content!, StringComparison.Ordinal);
        Assert.DoesNotContain("the third", links[0].Content!, StringComparison.Ordinal);

        Assert.Equal(5, links[1].Line);
        Assert.Contains("no accessible name", links[1].Offence!, StringComparison.Ordinal);
        Assert.DoesNotContain("the third", links[1].Content!, StringComparison.Ordinal);
        Assert.Null(links[2].Offence);
    }

    /// <summary>The report names the file the way the walk prints it and the line the tag opens on - the first of
    /// them, not the line its missing <c>href</c> or its name would have sat on.</summary>
    [Fact]
    public void TheReportNamesTheFileAndTheLineOfTheTag()
    {
        var text = """
            <div class="links">
                <a class="deep"
                   target="_blank"></a>
                <a
                   href="#">
                </a>
            </div>
            """;

        var offenders = ReportsOf(Scan(Path.Combine("Components", "Sample.razor"), text));

        Assert.Equal(2, offenders.Count);
        Assert.StartsWith(Path.Combine("Components", "Sample.razor") + ":2 ", offenders[0], StringComparison.Ordinal);
        Assert.Contains("writes no href", offenders[0], StringComparison.Ordinal);
        Assert.Contains("no accessible name", offenders[0], StringComparison.Ordinal);
        Assert.Contains(":4 ", offenders[1], StringComparison.Ordinal);
        Assert.Contains("bare \"#\"", offenders[1], StringComparison.Ordinal);
        Assert.Contains("no accessible name", offenders[1], StringComparison.Ordinal);
    }

    /// <summary>Every <c>&lt;a&gt;</c> start tag of every component, in the order the files write them.</summary>
    private static List<Link> Links() =>
        [.. RazorMarkup().SelectMany(markup => Scan(markup.File, markup.Text))];

    /// <summary>The links of one file, each with the attributes its tag writes and the content up to the
    /// <c>&lt;/a&gt;</c> that closes it. The whole of a component and a sample of markup the tests above read the
    /// rules through are read the same way.</summary>
    private static List<Link> Scan(string file, string text)
    {
        var links = new List<Link>();

        foreach (var start in MarkupScan.OpeningTags(text))
        {
            var tag = MarkupScan.TagOf(text, start);

            if (!NameOf(tag).Equals(AnchorElement, StringComparison.Ordinal))
            {
                continue;
            }

            var after = start + tag.Length;

            // A tag closed by a "/>" of its own holds nothing, and the "</a>" of the markup after it is no answer
            // to its own content.
            var content = tag.EndsWith("/>", StringComparison.Ordinal)
                ? string.Empty
                : ContentOf(text, after);

            links.Add(new Link(file, LineOf(text, start), tag, content, AttributesOf(tag)));
        }

        return links;
    }

    /// <summary>Why the links are refused, one line each: the file, the line of the tag and the offence.</summary>
    private static List<string> ReportsOf(IEnumerable<Link> links) =>
        [.. links
            .Where(link => link.Offence is not null)
            .Select(link => link.Report)];

    /// <summary>The offence of the sample's first link - <c>null</c> when it offends in no way - refusing a sample
    /// that holds no link at all, so a test cannot pass on markup the rules never read.</summary>
    private static string? OffenceOf(string text)
    {
        var scanned = Scan("Sample.razor", text);

        Assert.NotEmpty(scanned);

        return scanned[0].Offence;
    }

    /// <summary>True when a value says something: a word of its own, or one Razor builds at runtime. A value of
    /// nothing, of spaces, or of the <c>@</c> of an expression that carries no name, says as little as no attribute
    /// at all.</summary>
    private static bool SaysSomething(string? value) => value is not null && NotSilence(value.Trim());

    /// <summary>True when the content of a link says something out loud: a word between its tags, the text of a child
    /// element, or a Razor expression such as <c>@text</c> or <c>@ChildContent</c> - and neither a comment, nor an
    /// <c>&amp;nbsp;</c>, nor the shape of an icon that carries no words of its own.</summary>
    private static bool SaysWords(string content) => NotSilence(Unblank(WordsOf(content)).Trim());

    /// <summary>True of words a reader hears. Empty is empty, and the <c>@</c> of an expression that named nothing -
    /// on its own, or as an empty <c>@()</c> - is as silent as the whitespace it stands in.</summary>
    private static bool NotSilence(string words) =>
        words.Length > 0
        && !words.Equals("@", StringComparison.Ordinal)
        && !words.Equals("@()", StringComparison.Ordinal);

    /// <summary>The words a piece of markup holds, with every tag read out of it, every comment left where it is and
    /// the blanks HTML spells with an entity folded into a space. What is left is what a reader hears.</summary>
    private static string WordsOf(string content)
    {
        var words = new StringBuilder();

        for (var at = 0; at < content.Length;)
        {
            var c = content[at];

            if (c == '@' && at + 1 < content.Length && content[at + 1] == '*')
            {
                var end = content.IndexOf("*@", at + 2, StringComparison.Ordinal);
                at = end < 0 ? content.Length : end + 2;
                continue;
            }

            if (c == '<')
            {
                if (content.AsSpan(at).StartsWith("<!--", StringComparison.Ordinal))
                {
                    var end = content.IndexOf("-->", at + 4, StringComparison.Ordinal);
                    at = end < 0 ? content.Length : end + 3;
                    continue;
                }

                if (at + 1 < content.Length && content[at + 1] == '/')
                {
                    at = EndOfClosingTag(content, at);
                    continue;
                }

                if (at + 1 < content.Length && char.IsLetter(content[at + 1]))
                {
                    // A "<" that opens nothing - the comparison of "@(count < 1)" - is text like any other.
                    at += MarkupScan.TagOf(content, at).Length;
                    continue;
                }
            }

            words.Append(c);
            at++;
        }

        return words.ToString();
    }

    /// <summary>The words with every entity that renders as nothing replaced by the space it draws, so a link padded
    /// with <c>&amp;nbsp;</c> is read as the silence it is.</summary>
    private static string Unblank(string words)
    {
        foreach (var blank in Blanks)
        {
            words = words.Replace(blank, " ", StringComparison.OrdinalIgnoreCase);
        }

        return words;
    }

    /// <summary>The content of the link whose tag ends at <c>from</c>: up to the <c>&lt;/a&gt;</c> that closes it,
    /// past the anchors nested in it - which HTML does not allow but a template may write - or <c>null</c> when the
    /// file leaves the link unclosed.</summary>
    private static string? ContentOf(string text, int from)
    {
        var open = 1;
        var at = from;

        while (at < text.Length)
        {
            var next = MarkupScan.NextTag(text, at);
            if (next < 0)
            {
                return null;
            }

            if (text[next + 1] == '/')
            {
                var end = EndOfClosingTag(text, next);

                if (ClosingNameOf(text, next) == AnchorElement)
                {
                    open--;

                    if (open == 0)
                    {
                        return text[from..next];
                    }
                }

                at = end;
                continue;
            }

            var tag = MarkupScan.TagOf(text, next);

            if (NameOf(tag) == AnchorElement && !tag.EndsWith("/>", StringComparison.Ordinal))
            {
                open++;
            }

            at = next + tag.Length;
        }

        return null;
    }

    /// <summary>The element name a tag opens, in lower case: HTML leaves an element name case-free, and a
    /// <c>&lt;A&gt;</c> owes an address and a name the same as an <c>&lt;a&gt;</c>.</summary>
    private static string NameOf(string tag) => NameChars(tag, 1);

    /// <summary>The element name a closing tag closes, in lower case.</summary>
    private static string ClosingNameOf(string text, int start) => NameChars(text, start + 2);

    private static string NameChars(string text, int from)
    {
        var end = from;

        while (end < text.Length && (char.IsLetterOrDigit(text[end]) || text[end] is '-' or '_' or ':'))
        {
            end++;
        }

        return text[from..end].ToLowerInvariant();
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
    /// value.</summary>
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

    /// <summary>Just past the "&gt;" that closes the closing tag at <paramref name="start"/>, or the end of the text
    /// when nothing does.</summary>
    private static int EndOfClosingTag(string text, int start)
    {
        var end = text.IndexOf('>', start);

        return end < 0 ? text.Length : end + 1;
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
