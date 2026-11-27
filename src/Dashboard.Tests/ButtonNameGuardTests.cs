using System.Runtime.CompilerServices;
using System.Text;

namespace Dashboard.Tests;

/// <summary>
/// A button is announced by its name, and a button with no name is a viewer pressing "button" and hearing nothing but
/// "button".
/// <para>
/// The name is either written on the tag - <c>aria-label</c>, <c>aria-labelledby</c> or <c>title</c>, with something in
/// it - or it stands between the tags: words of its own, a Razor expression such as <c>@ReloadTopologyLabel</c>, or
/// the text of a child element, be it a <c>&lt;span&gt;</c> of the label or the <c>.visually-hidden</c> word that goes
/// with an icon. An icon drawn alone is no name: whatever an element hides with <c>aria-hidden="true"</c> a reader
/// walks past, so a button whose whole content sits under a hidden element says as little as an empty one. A name
/// Razor builds - <c>title="@PauseHelp"</c>, <c>aria-label="@($"Pin history of {HistoryOf}")"</c> - counts as said,
/// because whether it comes out empty belongs to the component's own tests, not to the markup (#158).
/// </para>
/// <para>
/// A <c>title</c> is the weakest of the three: it is announced only by some combinations of browser and reader, which
/// is why the new-tab links of <c>VersionLine.razor</c> write an <c>aria-label</c> beside it. Every button of this app
/// therefore carries its words between its tags as well, and a second rule refuses the one that leans on a
/// <c>title</c> alone.
/// </para>
/// <para>
/// A guard walks every component and refuses both faults, listing the file and the line the tag opens on, so the next
/// button added cannot be forgotten. The tag is read to the "&gt;" that closes it - past the Razor expressions, the
/// quoted values that carry a "&gt;" of their own and the line breaks they spread over - its attributes are read as
/// name/value pairs, so <c>data-title</c> is never taken for <c>title</c> and <c>&lt;buttongroup&gt;</c>,
/// <c>&lt;button-row&gt;</c> are other elements; and its content runs to the <c>&lt;/button&gt;</c> that matches it, so
/// a word of the next button is never taken for this one's name. This is how <c>LinkGuardTests</c>,
/// <c>ButtonTypeGuardTests</c>, <c>ImageNameGuardTests</c> and <c>AriaHiddenFocusGuardTests</c> read the same markup.
/// </para>
/// </summary>
public class ButtonNameGuardTests
{
    private const string ButtonElement = "button";

    /// <summary>The attributes that name a button from outside its content.</summary>
    private static readonly string[] NameAttributes = ["aria-label", "aria-labelledby", "title"];

    /// <summary>The words HTML renders as nothing a reader could hear, so a button padded with them stays silent.</summary>
    private static readonly string[] Blanks = ["&nbsp;", "&#160;", "&#xA0;", "&emsp;", "&thinsp;", "&zwnj;", "&zwj;", "&#8203;", "&#xFEFF;", "&shy;"];

    /// <summary>The elements that write no closing tag, so a hidden one of these owns nothing to read past.</summary>
    private static readonly string[] VoidElements =
        ["area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "param", "source", "track", "wbr"];

    /// <summary>
    /// One button of one component: the file to print, the line its tag opens on, the tag itself, what stands between
    /// it and the <c>&lt;/button&gt;</c> that closes it (<c>null</c> when nothing does), and the attributes the tag
    /// writes.
    /// </summary>
    private sealed record Button(string File, int Line, string Tag, string? Content, IReadOnlyList<TagAttribute> Attributes)
    {
        /// <summary>True when the tag is closed by a "/&gt;" of its own, which leaves it no content to be named by.</summary>
        private bool IsSelfClosed => SelfClosing(Tag);

        /// <summary>The first of the three naming attributes the tag writes a non-empty value into, or <c>null</c> when
        /// it names itself by none of them - a name Razor builds counts as said.</summary>
        public string? NamedBy =>
            NameAttributes.FirstOrDefault(name => SaysSomething(ValueOf(name)));

        /// <summary>True when something between the tags is said out loud once the markup an <c>aria-hidden="true"</c>
        /// element hides is taken away: plain words, a Razor expression such as <c>@label</c>, or the text of a child
        /// element.</summary>
        public bool NamedByContent => Content is not null && SaysWords(WithoutHiddenMarkup(Content));

        /// <summary>True when the button has a name either way.</summary>
        private bool Speaks => NamedBy is not null || NamedByContent;

        /// <summary>Why the guard refuses this button, in the words a failure prints - <c>null</c> when the button says
        /// what it is.</summary>
        public string? Offence
        {
            get
            {
                if (Speaks)
                {
                    return null;
                }

                const string Missing = "no non-empty aria-label, aria-labelledby or title of its own";

                return IsSelfClosed
                    ? $"<{ButtonElement}> is closed by a \"/>\" of its own, so it has no content to be named by, and writes {Missing}"
                    : Content is null
                        ? $"<{ButtonElement}> is never closed by a </{ButtonElement}>, so it has no content of its own to name it and writes {Missing}"
                        : HoldsOnlyHiddenMarkup
                            ? $"<{ButtonElement}> writes {Missing}, and all its content stands under an element hidden with aria-hidden=\"true\", which a reader walks past"
                            : $"<{ButtonElement}> has no accessible name: {Missing} and nothing between its tags and its </{ButtonElement}> that a reader hears, so a reader hears \"{ButtonElement}\" and nothing else";
            }
        }

        /// <summary>True of the button whose whole content sits under a hidden element: the report says so, since the
        /// fix there is to let a word through rather than to invent one.</summary>
        private bool HoldsOnlyHiddenMarkup =>
            Content is not null && !NamedByContent && HoldsHidden(Content);

        /// <summary>True when a <c>title</c> is the button's only name: some browser and reader combinations never read
        /// one aloud, so the app's buttons carry their words between their tags as well.</summary>
        public bool ReliesOnATitleAlone => NamedBy is "title" && !NamedByContent;

        /// <summary>Where the fault stands: the file as the walk prints it, the line the tag opens on, and why.</summary>
        public string Report => $"{File}:{Line} {Offence}";

        /// <summary>The value the tag's first attribute of that name holds, unquoted; <c>null</c> when the attribute is
        /// written without a value at all, and the name itself absent when the tag writes no such attribute.</summary>
        private string? ValueOf(string name) =>
            Attributes.FirstOrDefault(attribute => attribute.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value;
    }

    /// <summary>One attribute of a tag: its name as written and its value without the quotes around it - <c>null</c>
    /// when the attribute is written bare, which HTML reads as an empty value.</summary>
    private sealed record TagAttribute(string Name, string? Value);

    /// <summary>Every button of every component says what it presses: a non-empty <c>aria-label</c>,
    /// <c>aria-labelledby</c> or <c>title</c> on its tag, or words between its tags that a reader is not told to walk
    /// past.</summary>
    [Fact]
    public void EveryButtonSaysWhatItIs()
    {
        Assert.Empty(ReportsOf(Buttons()));
    }

    /// <summary>
    /// A guard that found nothing has seen nothing: the app writes fifteen buttons today - the pause and check now of
    /// <c>App.razor</c>, the four view tabs, the back buttons of <c>ApplicationView.razor</c> and
    /// <c>DatabaseView.razor</c>, the environment and fit switches of <c>RuntimeView.razor</c>, the audience switches
    /// of <c>WorkItemsView.razor</c> and the two of <c>TrafficPanel.razor</c> - so a scan that reaches far fewer never
    /// read the markup, and the clean verdict above would mean nothing. The floor is set well below that count, so the
    /// day the app gains a button the test still says what it reads.
    /// </summary>
    [Fact]
    public void TheGuardReadsTheButtonsOfTheApp()
    {
        var buttons = Buttons();
        var files = buttons.Select(button => button.File).Distinct(StringComparer.Ordinal).ToList();

        Assert.True(
            buttons.Count >= 10,
            $"only {buttons.Count} <{ButtonElement}> start tag(s) were read under {DashboardRoot()}: the guard is checking nothing");

        Assert.True(
            files.Count >= 3,
            $"the {buttons.Count} button(s) read under {DashboardRoot()} live in only {files.Count} file(s): the walk does not reach the app");

        Assert.Contains(buttons, button => button.File.Equals("App.razor", StringComparison.Ordinal));
        Assert.Contains(buttons, button => button.File.Equals(Path.Combine("Components", "RuntimeView.razor"), StringComparison.Ordinal));
        Assert.Contains(buttons, button => button.File.Equals(Path.Combine("Components", "TrafficPanel.razor"), StringComparison.Ordinal));

        // Both ways of naming a button are read as the app actually uses them: the tabs of App.razor are named by the
        // word under them, the back buttons by a title Razor builds - and none of the app's buttons gives the guard a
        // reason to speak.
        Assert.Contains(buttons, button => button.NamedByContent && button.NamedBy is null);
        Assert.Contains(buttons, button => button.NamedBy is not null);
        Assert.All(buttons, button => Assert.True(button.Offence is null, $"{button.Report}: a button of the app is refused"));
    }

    /// <summary>The rule refuses the button that says nothing: no content, whitespace only, the blanks an
    /// <c>&amp;nbsp;</c> renders, a comment, an empty or blank name written over the silence, a name left bare, a
    /// <c>@</c> of an expression that never came, a name buried in a child that holds no word of its own, the tag
    /// closed by a "/&gt;" of its own and a button the file never closes.</summary>
    [Fact]
    public void TheRuleRefusesAButtonThatSaysNothing()
    {
        Assert.Contains("no accessible name", OffenceOf("""<button type="button"></button>"""), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("<button type=\"button\">   \n\t  </button>"), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("""<button type="button">&nbsp;&#160;</button>"""), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("""<button type="button">@* a comment is not read out *@</button>"""), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("""<button type="button"><!-- nor is a comment --></button>"""), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("""<button type="button" aria-label="" aria-labelledby="" title=""></button>"""), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("""<button type="button" aria-label="   " aria-labelledby="   " title="   "> </button>"""), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("""<button type="button" aria-label aria-labelledby title> </button>"""), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("""<button type="button">@</button>"""), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("""<button type="button">@()</button>"""), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("""<button type="button" data-aria-label="Not a name" data-title="Nor this"></button>"""), StringComparison.Ordinal);
        Assert.Contains("no accessible name", OffenceOf("""<button type="button"><span aria-label="A child's own name"></span></button>"""), StringComparison.Ordinal);
        Assert.Contains("closed by a \"/>\"", OffenceOf("""<button type="button" class="icon" />"""), StringComparison.Ordinal);
        Assert.Contains("never closed by a </button>", OffenceOf("""<div><button type="button" aria-label=""></div>"""), StringComparison.Ordinal);
    }

    /// <summary>Markup a reader is told to walk past names nothing: the icon alone, the icon and its own nested
    /// children, a hidden <c>&lt;span&gt;</c> of words, and the hidden element spread over lines. The word written
    /// beside the hidden mark is the button's name, and so is the child that hides nothing.</summary>
    [Fact]
    public void MarkupHiddenFromAReaderIsNoName()
    {
        Assert.Contains("aria-hidden=\"true\"", OffenceOf("""<button type="button"><svg viewBox="0 0 16 16" aria-hidden="true" focusable="false"><path d="M5 8h6" /></svg></button>"""), StringComparison.Ordinal);
        Assert.Contains("aria-hidden=\"true\"", OffenceOf("""<button type="button"><span aria-hidden="true">—</span></button>"""), StringComparison.Ordinal);
        Assert.Contains("aria-hidden=\"true\"", OffenceOf("""
            <button type="button">
                <span class="icon" aria-hidden="true">
                    <svg viewBox="0 0 16 16"><path d="M5 8h6" /></svg>
                </span>
            </button>
            """), StringComparison.Ordinal);

        Assert.Null(OffenceOf("""<button type="button"><svg viewBox="0 0 16 16" aria-hidden="true" focusable="false"><path d="M5 8h6" /></svg>Pause</button>"""));
        Assert.Null(OffenceOf("""<button type="button"><span aria-hidden="true">—</span><span class="visually-hidden">No measurement</span></button>"""));

        // Only the element's own attribute hides it: "false" says the opposite, a value Razor builds says nothing yet,
        // and a data-aria-hidden is another attribute.
        Assert.Null(OffenceOf("""<button type="button"><span aria-hidden="false">Dash</span></button>"""));
        Assert.Null(OffenceOf("""<button type="button"><span aria-hidden="@(_quiet)">Dash</span></button>"""));
        Assert.Null(OffenceOf("""<button type="button"><span data-aria-hidden="true">Dash</span></button>"""));
    }

    /// <summary>The rule accepts the names the app writes and the ones HTML offers: a word of its own, a name Razor
    /// builds, the text of a child element, a child that is a component of the app, each of the three attributes on an
    /// otherwise empty button, a bare attribute beside real words and the whole in either case.</summary>
    [Fact]
    public void TheRuleAcceptsAButtonThatSaysWhatItIs()
    {
        Assert.Null(OffenceOf("""<button type="button">Check now</button>"""));
        Assert.Null(OffenceOf("""<button type="button">@ReloadTopologyLabel</button>"""));
        Assert.Null(OffenceOf("""<button type="button">@(_fit ? "Actual size" : "Fit to width")</button>"""));
        Assert.Null(OffenceOf("""<button type="button"><span class="visually-hidden"> work items</span></button>"""));
        Assert.Null(OffenceOf("""<button type="button">@Audience <span class="work__count">@count</span></button>"""));
        Assert.Null(OffenceOf("""<button type="button"><DeploymentDot State="state" />Deploying</button>"""));
        Assert.Null(OffenceOf("""<button type="button" aria-label="Pause the polling"></button>"""));
        Assert.Null(OffenceOf("""<button type="button" aria-labelledby="runtime-title"></button>"""));
        Assert.Null(OffenceOf("""<button type="button" title="@PauseHelp"></button>"""));
        Assert.Null(OffenceOf("""<button type="button" aria-label="@($"Pin history of {HistoryOf}")"></button>"""));
        Assert.Null(OffenceOf("""<button type="button" aria-label>Pause</button>"""));
        Assert.Null(OffenceOf("""<BUTTON TYPE="BUTTON" TITLE="Pause">Pause the polling</BUTTON>"""));
    }

    /// <summary>A <c>title</c> is the weakest of the three names - some browser and reader combinations never read it
    /// aloud - so the app's buttons carry their words between their tags as well and none of them leans on a title
    /// alone.</summary>
    [Fact]
    public void NoButtonOfTheAppIsNamedByATitleAlone()
    {
        var offenders = Buttons()
            .Where(button => button.ReliesOnATitleAlone)
            .Select(button => $"{button.File}:{button.Line} <{ButtonElement}> is named by its title alone, which some browser and reader combinations never read aloud")
            .ToList();
        Assert.Empty(offenders);
    }

    /// <summary>Only the attribute of that very name is the button's own: a <c>data-title</c> is no name, an
    /// <c>aria-labelledbyte</c> is no name, and an element whose name merely begins with the word
    /// <c>button</c> - <c>&lt;buttongroup&gt;</c>, <c>&lt;button-row&gt;</c> - is another element that owes this rule
    /// nothing, however little it says. <c>&lt;/button&gt;</c> is the end of this one, not the start of the next.</summary>
    [Fact]
    public void OnlyTheButtonsOwnAttributesAndTagsCount()
    {
        var text = """
            <main>
                <buttongroup><button-row data-title="Not a name"></button-row></buttongroup>
                <button type="button" data-title="Not a name" aria-labelledbyte="not-it" aria-label="A name after all">Pause</button>
                <button type="button" data-aria-label="Not a name"></button>
            </main>
            """;

        var buttons = Scan("Sample.razor", text);

        Assert.Equal(2, buttons.Count);

        Assert.Null(buttons[0].Offence);
        Assert.Equal("aria-label", buttons[0].NamedBy);
        Assert.Contains(buttons[0].Attributes, attribute => attribute.Name == "data-title");

        Assert.Equal(4, buttons[1].Line);
        Assert.Contains("no accessible name", buttons[1].Offence, StringComparison.Ordinal);
    }

    /// <summary>A tag ends at the "&gt;" that closes it, not at the first "&gt;" of the file: a class may hold a
    /// comparison, a title a "&gt;" of its own inside the quotes Razor carries, a handler the <c>"=&gt;"</c> of a
    /// lambda, and the attributes - among them <c>title</c> - may spread over lines. Read short, the name below the
    /// first line would be outside the tag and the content would start in the middle of an attribute.</summary>
    [Fact]
    public void TheTagReadRunsPastRazorAndLineBreaks()
    {
        var text = """
            <main>
                <button type="button" class="button @(selected ? "button--selected" : "button--quiet")"
                        title="@(inFlight.Count > 0 ? string.Join('\n', inFlight.Select(DeploymentText.Title)) : null)"
                        @onclick="() => SelectAsync(entry.Name)">
                    @entry.Name
                </button>
                <button type="button" class="button" aria-label=""></button>
            </main>
            """;

        var buttons = Scan("Sample.razor", text);

        Assert.Equal(2, buttons.Count);

        Assert.Equal(2, buttons[0].Line);
        Assert.Null(buttons[0].Offence);
        Assert.Equal("title", buttons[0].NamedBy);
        Assert.DoesNotContain("</button", buttons[0].Tag, StringComparison.Ordinal);
        Assert.DoesNotContain("@entry.Name", buttons[0].Tag, StringComparison.Ordinal);
        Assert.Contains("@entry.Name", buttons[0].Content!, StringComparison.Ordinal);
        Assert.Contains(buttons[0].Attributes, attribute => attribute.Name == "@onclick");
        Assert.Contains(buttons[0].Attributes, attribute => attribute.Name == "title" && attribute.Value!.Contains('>'));

        Assert.Equal(7, buttons[1].Line);
        Assert.Contains("no accessible name", buttons[1].Offence, StringComparison.Ordinal);
    }

    /// <summary>The content of a button runs to the <c>&lt;/button&gt;</c> that matches it and no further: a button is
    /// named by what stands in it, never by the words of the one written after it, and the buttons nested in it - which
    /// HTML does not allow but a template may write - are read whole.</summary>
    [Fact]
    public void TheContentOfAButtonEndsAtItsOwnClosingTag()
    {
        var text = """
            <div class="views">
                <button type="button">
                    <span>the first</span>
                </button>
                <button type="button"></button>
                <button type="button">the third</button>
            </div>
            """;

        var buttons = Scan("Sample.razor", text);

        Assert.Equal(3, buttons.Count);

        Assert.Null(buttons[0].Offence);
        Assert.Contains("the first", buttons[0].Content!, StringComparison.Ordinal);
        Assert.DoesNotContain("the third", buttons[0].Content!, StringComparison.Ordinal);

        Assert.Equal(5, buttons[1].Line);
        Assert.Contains("no accessible name", buttons[1].Offence, StringComparison.Ordinal);
        Assert.DoesNotContain("the third", buttons[1].Content!, StringComparison.Ordinal);

        Assert.Null(buttons[2].Offence);
    }

    /// <summary>The report names the file the way the walk prints it and the line the tag opens on - the first of them,
    /// not the line its missing name or its last attribute sits on - and reads every button of a file on its own.</summary>
    [Fact]
    public void TheReportNamesTheFileAndTheLineOfTheTag()
    {
        var text = """
            <div class="toolbar">
                <button class="views__tab"
                        @onclick="() => Pick(view)"></button>
                <button type="button"
                        title>
                </button>
            </div>
            """;

        var offenders = ReportsOf(Scan(Path.Combine("Components", "Sample.razor"), text));

        Assert.Equal(2, offenders.Count);
        Assert.StartsWith(Path.Combine("Components", "Sample.razor") + ":2 <button> ", offenders[0], StringComparison.Ordinal);
        Assert.Contains("no accessible name", offenders[0], StringComparison.Ordinal);
        Assert.Contains(":4 <button> ", offenders[1], StringComparison.Ordinal);
        Assert.Contains("no accessible name", offenders[1], StringComparison.Ordinal);
    }

    /// <summary>Every <c>&lt;button&gt;</c> start tag of every component, in the order the files write them.</summary>
    private static List<Button> Buttons() =>
        [.. RazorMarkup().SelectMany(markup => Scan(markup.File, markup.Text))];

    /// <summary>The buttons of one file, each with the attributes its tag writes and the content up to the
    /// <c>&lt;/button&gt;</c> that closes it. The whole of a component and the samples of markup the tests above read
    /// the rules through are read the same way.</summary>
    private static List<Button> Scan(string file, string text)
    {
        var buttons = new List<Button>();

        foreach (var start in MarkupScan.OpeningTags(text))
        {
            var tag = MarkupScan.TagOf(text, start);

            if (!NameOf(tag).Equals(ButtonElement, StringComparison.Ordinal))
            {
                continue;
            }

            // A tag closed by a "/>" of its own holds nothing, and the "</button>" of the markup after it is no answer
            // to its own content.
            var content = SelfClosing(tag)
                ? string.Empty
                : ContentOf(text, start + tag.Length, ButtonElement);

            buttons.Add(new Button(file, LineOf(text, start), tag, content, AttributesOf(tag)));
        }

        return buttons;
    }

    /// <summary>Why the buttons are refused, one line each: the file, the line of the tag and the offence.</summary>
    private static List<string> ReportsOf(IEnumerable<Button> buttons) =>
        [.. buttons
            .Where(button => button.Offence is not null)
            .Select(button => button.Report)];

    /// <summary>The offence of the sample's first button - <c>null</c> when it names itself - refusing a sample that
    /// holds no button at all, so a test cannot pass on markup the rules never read.</summary>
    private static string? OffenceOf(string text)
    {
        var scanned = Scan("Sample.razor", text);

        Assert.NotEmpty(scanned);

        return scanned[0].Offence;
    }

    /// <summary>True when a name written on the tag says something: a word of its own, or one Razor builds at runtime.
    /// A name of nothing, of spaces, or of the <c>@</c> of an expression that never came, says as little as no
    /// attribute at all.</summary>
    private static bool SaysSomething(string? value) => value is not null && NotSilence(value.Trim());

    /// <summary>True when a piece of content says something out loud once the hidden markup is taken away: a word
    /// between the tags, the text of a child element, or a Razor expression such as <c>@label</c> or
    /// <c>@ChildContent</c>.</summary>
    /// <param name="content">The content, already read of its hidden markup - see
    /// <see cref="WithoutHiddenMarkup" />.</param>
    private static bool SaysWords(string content) => NotSilence(Unblank(WordsOf(content)).Trim());

    /// <summary>True when the markup holds an element that hides itself from a reader, whatever stands in it.</summary>
    private static bool HoldsHidden(string markup)
    {
        foreach (var start in MarkupScan.OpeningTags(markup))
        {
            var tag = MarkupScan.TagOf(markup, start);

            if (NameOf(tag).Length > 0 && Hides(tag))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>True of words a reader hears. Empty is empty, and the <c>@</c> of an expression that named nothing - on
    /// its own, or as an empty <c>@()</c> - is as silent as the whitespace it stands in.</summary>
    private static bool NotSilence(string words) =>
        words.Length > 0
        && !words.Equals("@", StringComparison.Ordinal)
        && !words.Equals("@()", StringComparison.Ordinal);

    /// <summary>The markup with every element an <c>aria-hidden="true"</c> hides taken out of it, content included -
    /// what is left is what a reader of the button hears.</summary>
    private static string WithoutHiddenMarkup(string markup)
    {
        var kept = new StringBuilder();

        for (var at = 0; at < markup.Length;)
        {
            var next = MarkupScan.NextTag(markup, at);

            if (next < 0)
            {
                kept.Append(markup[at..]);
                break;
            }

            // The text before the tag is the button's own and stays.
            kept.Append(markup[at..next]);

            if (markup[next + 1] == '/')
            {
                at = EndOfClosingTag(markup, next);
                continue;
            }

            var tag = MarkupScan.TagOf(markup, next);
            var name = NameOf(tag);

            if (name.Length > 0 && Hides(tag))
            {
                at = SelfClosing(tag) || VoidElements.Contains(name, StringComparer.Ordinal)
                    ? next + tag.Length
                    : EndOfElement(markup, next + tag.Length, name);
                continue;
            }

            kept.Append(tag);
            at = next + tag.Length;
        }

        return kept.ToString();
    }

    /// <summary>True when the tag hides its element from a reader: an <c>aria-hidden</c> of the tag's own - the
    /// attribute boundary is the tag's whitespace or its "&lt;", so <c>data-aria-hidden</c> is another attribute -
    /// whose value is "true". A value Razor builds hides nothing this guard can read, the same way
    /// <c>AriaHiddenFocusGuardTests</c> reads it.</summary>
    private static bool Hides(string tag)
    {
        const string Name = "aria-hidden";

        for (var at = tag.IndexOf(Name, StringComparison.OrdinalIgnoreCase); at >= 0;
             at = tag.IndexOf(Name, at + Name.Length, StringComparison.OrdinalIgnoreCase))
        {
            if (at > 0 && !(char.IsWhiteSpace(tag[at - 1]) || tag[at - 1] == '<'))
            {
                continue;
            }

            var value = ValueAfter(tag, at + Name.Length);

            if (value is not null && value.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

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

    /// <summary>The words with every entity that renders as nothing replaced by the space it draws, so a button padded
    /// with <c>&amp;nbsp;</c> is read as the silence it is.</summary>
    private static string Unblank(string words)
    {
        foreach (var blank in Blanks)
        {
            words = words.Replace(blank, " ", StringComparison.OrdinalIgnoreCase);
        }

        return words;
    }

    /// <summary>The content of the element whose tag ends at <c>from</c>: up to the <c>&lt;/name&gt;</c> that matches
    /// it, or <c>null</c> when the file never closes the element.</summary>
    private static string? ContentOf(string text, int from, string name)
    {
        var end = EndOfElement(text, from, name);

        return end < 0 ? null : text[from..end];
    }

    /// <summary>Just past the <c>&lt;/name&gt;</c> that closes the element whose content begins at
    /// <paramref name="from"/>, or -1 when nothing closes it. The elements of the same name nested inside are counted,
    /// so an inner closing tag ends nothing and a self-closing one owns no content.</summary>
    private static int EndOfElement(string text, int from, string name)
    {
        var depth = 1;
        var at = from;

        while (true)
        {
            var next = MarkupScan.NextTag(text, at);

            if (next < 0)
            {
                return -1;
            }

            if (text[next + 1] == '/')
            {
                var end = EndOfClosingTag(text, next);

                if (ClosingNameOf(text, next) == name)
                {
                    depth--;

                    if (depth == 0)
                    {
                        return end;
                    }
                }

                at = end;
                continue;
            }

            var tag = MarkupScan.TagOf(text, next);

            if (NameOf(tag) == name && !SelfClosing(tag))
            {
                depth++;
            }

            at = next + tag.Length;
        }
    }

    /// <summary>The element name a tag opens, in lower case: HTML leaves an element name case-free, and a
    /// <c>&lt;BUTTON&gt;</c> owes a reader a name the same as a <c>&lt;button&gt;</c>.</summary>
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

    /// <summary>True when the tag closes itself: the "/" that stands before its "&gt;". A value that ends in a slash -
    /// <c>href="/"</c> - keeps its quote between the two and closes nothing.</summary>
    private static bool SelfClosing(string tag) => tag.Length >= 2 && tag[^2] == '/';

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

    /// <summary>The value an attribute holds where its name ends: <c>null</c> when none can be read of it - the
    /// attribute written bare, with no "=" at all.</summary>
    private static string? ValueAfter(string markup, int at)
    {
        var value = MarkupScan.SkipSpace(markup, at);

        if (value >= markup.Length || markup[value] != '=')
        {
            return null;
        }

        value = MarkupScan.SkipSpace(markup, value + 1);

        return ValueAt(markup, ref value);
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
