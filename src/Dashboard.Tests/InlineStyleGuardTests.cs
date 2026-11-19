using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// A literal <c>style="..."</c> in markup is a decision the stylesheet never took. The rest of the app says what a
/// thing is with a class and lets the stylesheet say what it looks like, so a theme, a print copy and a reader of
/// the source all see the same choice made in the one place; an inline style hides that choice inside the tag,
/// where it is neither with its other styles nor with the words of the page, and nothing else in the app can
/// answer for it. The app keeps exactly those inline styles whose value only the component's own code can know -
/// a bar's width from its percent, a segment's growth from its line count - and nothing more (#203).
/// <para>
/// So the guard holds the line on both sides of it: every literal <c>style</c> attribute of every <c>.razor</c>
/// file of the app must stand on the allowlist below, by the file it lives in and by the exact value it writes,
/// so the next inline style added is added to this test on purpose; and every entry of the allowlist must still
/// stand in the markup, so an entry the markup no longer writes is refused rather than left here, approving a
/// style that does not exist. The two styles the app carries today are the whole of the allowlist, and both are
/// values Razor builds inside the quoted literal - the attribute is written, its number is computed.
/// </para>
/// <para>
/// A guard walks every component and reports the file and the line the tag opens on, the way
/// <see cref="TimeElementGuardTests" />, <see cref="EmptyLabelGuardTests" /> and <see cref="MeterGuardTests" />
/// walk and report: the tag is read to the "&gt;" that closes it, past the Razor expressions, the quoted values
/// that carry a "&gt;" of their own and the line breaks they spread over. The attribute is read by its whole
/// name, so a <c>data-style</c> or a <c>stylefoo</c> is another attribute, while a <c>STYLE</c> is this one, HTML
/// leaving an attribute name case-free, and a <c>&lt;/span&gt;</c> that closes a tag writes no attribute at all.
/// </para>
/// </summary>
public class InlineStyleGuardTests
{
    /// <summary>The attribute this rule is about, as the tag spells it in lower case.</summary>
    private const string StyleAttribute = "style";

    /// <summary>
    /// The exact allowlist of the inline styles the app may carry: the file, as the walk prints it, and the value
    /// between the quotes, exactly as the tag wrote it - the Razor expressions in it and all. A value here is a
    /// value as written; the number a value Razor builds settles to at runtime is the component's own business,
    /// and this guard reads the words the tag chose, not the arithmetic behind them. Compared as the pair it is,
    /// so the same value in a file the list does not name is as refused as a value the list never wrote.
    /// </summary>
    private static readonly HashSet<(string File, string Value)> AllowedInlineStyles =
    [
        // The language bar of CodeCard: each segment stretches to its share of the lines, and the growth is a
        // number only the component's own code can name, so no class can hold it.
        (Path.Combine("Components", "CodeCard.razor"), "flex-grow: @share.Lines.ToString(CultureInfo.InvariantCulture)"),

        // The fill of Meter: the bar is the share of the whole, and its width is a percent the component
        // computes, not a width the stylesheet could name.
        (Path.Combine("Components", "Meter.razor"), "width: @Whole(percent)%"),
    ];

    /// <summary>
    /// One <c>style</c> attribute of the markup: the file to print, the line the tag it stands on opens, and the
    /// value between the quotes, exactly as the tag wrote it.
    /// </summary>
    private sealed record InlineStyle(string File, int Line, string Value)
    {
        /// <summary>True when this file and this value stand on the allowlist.</summary>
        public bool Allowed => AllowedInlineStyles.Contains((File, Value));

        /// <summary>Where the fault stands: the file as the walk prints it, the line the tag opens on, and the
        /// value the tag wrote, between the quotes it wrote them in.</summary>
        public string Report => $"{File}:{Line} style=\"{Value}\"";
    }

    /// <summary>
    /// Every literal <c>style</c> attribute of every component stands on the allowlist, by file and by the exact
    /// value it writes: a new inline style is not allowed to appear in the markup before it has been written down
    /// here, deliberately, with the reason it cannot be a class.
    /// </summary>
    [Fact]
    public void EveryInlineStyleIsOnTheAllowlist()
    {
        var offenders = Styles()
            .Where(style => !style.Allowed)
            .Select(style => $"{style.Report}: not on the inline style allowlist, and the allowlist is where a new one is added on purpose")
            .ToList();

        Assert.Empty(offenders);
    }

    /// <summary>
    /// Every entry of the allowlist still stands in the markup: the list approves what the app writes, and an
    /// entry the markup no longer writes approves nothing, so it is reported, by file and value, for the person
    /// who next edits this test to take it off rather than leave it standing.
    /// </summary>
    [Fact]
    public void EveryAllowlistEntryStillStandsInTheMarkup()
    {
        var written = Styles()
            .Select(style => (style.File, style.Value))
            .ToHashSet();

        var stale = AllowedInlineStyles
            .Where(allowed => !written.Contains(allowed))
            .Select(allowed => $"{allowed.File}: style=\"{allowed.Value}\" is on the allowlist but the markup no longer writes it")
            .OrderBy(message => message, StringComparer.Ordinal)
            .ToList();

        Assert.Empty(stale);
    }

    /// <summary>
    /// A guard that found nothing has seen nothing: the app writes two inline styles today, the language bar of
    /// <c>CodeCard.razor</c> and the fill of <c>Meter.razor</c>, a scan that reaches far fewer never read the
    /// markup, and the clean verdicts above would mean nothing; the floors are set well below the counts, so the
    /// day the app gains a style the test still says what it reads.
    /// </summary>
    [Fact]
    public void TheGuardReadsTheInlineStylesOfTheApp()
    {
        var styles = Styles();
        var files = styles.Select(style => style.File).Distinct(StringComparer.Ordinal).ToList();

        Assert.True(
            styles.Count >= 2,
            $"only {styles.Count} inline style(s) were read under {DashboardRoot()}: the guard is checking nothing");

        Assert.True(
            files.Count >= 2,
            $"the {styles.Count} inline style(s) read live in {files.Count} file(s): the walk does not reach the app");

        Assert.Contains(styles, style => style.File.Equals(Path.Combine("Components", "CodeCard.razor"), StringComparison.Ordinal));
        Assert.Contains(styles, style => style.File.Equals(Path.Combine("Components", "Meter.razor"), StringComparison.Ordinal));
    }

    /// <summary>The rule refuses the inline style the allowlist does not name: a value it never wrote, and the
    /// value it does write standing in a file the list does not name, since the file is part of the entry.</summary>
    [Fact]
    public void TheRuleRefusesAnInlineStyleTheAllowlistDoesNotName()
    {
        var unlisted = Scan(Path.Combine("Components", "Sample.razor"), """<div class="panel" style="color: red">text</div>""");

        Assert.Single(unlisted);
        Assert.False(unlisted[0].Allowed);
        Assert.Contains("style=\"color: red\"", unlisted[0].Report, StringComparison.Ordinal);

        var allowedValueElsewhere = Scan(
            Path.Combine("Components", "Sample.razor"),
            """<span style="width: @Whole(percent)%"></span>""");

        Assert.Single(allowedValueElsewhere);
        Assert.False(allowedValueElsewhere[0].Allowed);
    }

    /// <summary>The rule accepts the two styles the allowlist names, read as the app writes them: the value as
    /// the tag wrote it, Razor expression and all, from the file the allowlist names it of.</summary>
    [Fact]
    public void TheRuleAcceptsTheStylesTheAllowlistNames()
    {
        var codeCard = Scan(
            Path.Combine("Components", "CodeCard.razor"),
            """<span class="langbar__part langbar__part--@share.Slot" style="flex-grow: @share.Lines.ToString(CultureInfo.InvariantCulture)"></span>""");

        var meter = Scan(
            Path.Combine("Components", "Meter.razor"),
            """<span class="meter__fill" style="width: @Whole(percent)%"></span>""");

        Assert.Single(codeCard);
        Assert.Single(meter);
        Assert.True(codeCard[0].Allowed, codeCard[0].Report);
        Assert.True(meter[0].Allowed, meter[0].Report);
    }

    /// <summary>An inline style of nothing is refused like any other: the attribute written with an empty value
    /// stands in the markup as a style, and a style of nothing is not on the allowlist any more than a style of
    /// anything else.</summary>
    [Fact]
    public void TheRuleRefusesAnInlineStyleOfNothing()
    {
        var empty = Scan(Path.Combine("Components", "Sample.razor"), """<div class="panel" style=""></div>""");

        Assert.Single(empty);
        Assert.Equal(string.Empty, empty[0].Value);
        Assert.False(empty[0].Allowed);
    }

    /// <summary>Only the attribute of that very name is an inline style: a <c>data-style</c> or a
    /// <c>stylefoo</c> is another attribute, while the same name spelled in another case is this one, since HTML
    /// leaves an attribute name case-free, and a closing tag writes no attribute at all.</summary>
    [Fact]
    public void OnlyTheStyleOwnNameCounts()
    {
        var text = """
            <div class="panel">
                <span data-style="width: 50%" stylefoo="width: 50%" xstyle="width: 50%">one</span>
                <span STYLE="width: 50%">two</span>
            </div>
            """;

        var styles = Scan(Path.Combine("Components", "Sample.razor"), text);

        Assert.Single(styles);
        Assert.Equal(3, styles[0].Line);
    }

    /// <summary>A tag ends at the "&gt;" that closes it, not at the first "&gt;" of the file: a value may hold a
    /// Razor call with its parentheses, an interpolation with a "&gt;" of its own between its quotes, and the
    /// attributes may spread over lines of their own. Read short, the <c>style</c> below the first line would be
    /// outside the tag and the guard would miss it - and read short the other way, a tag of several lines is
    /// reported where it opened.</summary>
    [Fact]
    public void TheTagReadRunsPastRazorAndLineBreaks()
    {
        var text = """
            <main>
                <span class="langbar__part langbar__part--@share.Slot"
                      style="flex-grow: @share.Lines.ToString(CultureInfo.InvariantCulture)"
                      title="@($"{share.Name}: {BuildText.Count(share.Lines)} lines, {BuildText.SharePercent(share.Percent)}")"></span>
                <div class="panel"
                     style="width: @($"{Whole(percent)}%")"></div>
            </main>
            """;

        var styles = Scan(Path.Combine("Components", "Sample.razor"), text);

        Assert.Equal(2, styles.Count);

        Assert.Equal(2, styles[0].Line);
        Assert.Equal("flex-grow: @share.Lines.ToString(CultureInfo.InvariantCulture)", styles[0].Value);

        Assert.Equal(5, styles[1].Line);
        Assert.Equal("""width: @($"{Whole(percent)}%")""", styles[1].Value);

        // The report prints the line the tag opened on, not the line its style sits on.
        Assert.StartsWith(
            Path.Combine("Components", "Sample.razor") + ":2 ",
            styles[0].Report,
            StringComparison.Ordinal);
    }

    /// <summary>The report names the file the way the walk prints it and the line the tag opens on - the first of
    /// them - and reads every tag of a file on its own.</summary>
    [Fact]
    public void TheReportNamesTheFileAndTheLineOfTheTag()
    {
        var text = """
            <div class="events">
                <h2 id="events-title">Events</h2>
                <span class="event__time" style="color: red">marked</span>
                <p>between the two</p>
            </div>
            """;

        var styles = Scan(Path.Combine("Components", "Sample.razor"), text);

        Assert.Single(styles);
        Assert.StartsWith(Path.Combine("Components", "Sample.razor") + ":3 ", styles[0].Report, StringComparison.Ordinal);
        Assert.EndsWith("style=\"color: red\"", styles[0].Report, StringComparison.Ordinal);
    }

    /// <summary>Every literal <c>style</c> attribute of every component, in the order the files write them.</summary>
    private static List<InlineStyle> Styles() =>
        [.. RazorMarkup().SelectMany(markup => Scan(markup.File, markup.Text))];

    /// <summary>The <c>style</c> attributes a piece of markup opens: every start tag whose attributes include one
    /// named <c>style</c>, once per occurrence, so a tag that writes the attribute twice is read twice. A whole
    /// component and a sample of markup the tests above read the rule through are scanned the same way.</summary>
    private static List<InlineStyle> Scan(string file, string text)
    {
        var styles = new List<InlineStyle>();

        foreach (var start in OpeningTags(text))
        {
            var tag = TagOf(text, start);

            foreach (var attribute in AttributesOf(tag)
                         .Where(attribute => attribute.Name.Equals(StyleAttribute, StringComparison.OrdinalIgnoreCase)))
            {
                styles.Add(new InlineStyle(file, LineOf(text, start), attribute.Value ?? string.Empty));
            }
        }

        return styles;
    }

    /// <summary>One attribute of a tag: its name as written and the value after it, if any - <c>null</c> when the
    /// attribute is written bare, which HTML reads as an empty value.</summary>
    private sealed record TagAttribute(string Name, string? Value);

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

    /// <summary>The attributes a tag writes, past its element name: each name as written and the value after it, if
    /// any. A quoted value is run through to the quote that closes it, Razor's own quotes included, and the "/" that
    /// opens a self-closing tag ends the tag rather than naming an attribute.</summary>
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
        var close = EndOfValue(tag, open, quote);

        at = close < tag.Length ? close + 1 : close;

        return close <= open + 1 ? string.Empty : tag[(open + 1)..close];
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

    private static int SkipSpace(string text, int at)
    {
        while (at < text.Length && char.IsWhiteSpace(text[at]))
        {
            at++;
        }

        return at;
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
