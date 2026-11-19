using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// A <c>&lt;button&gt;</c> that names no <c>type</c> is a submit button - that is what HTML makes of it by default -
/// and a submit button sends the form it belongs to. Half the buttons of this dashboard sit beside a form or inside
/// one, and one pressed by accident reloads the page, stops the polling and loses what the reader was looking at; a
/// button that only runs code has to say so. So every button of the app writes out its <c>type</c> from the three
/// HTML offers: <c>button</c> for the ones that only act, <c>submit</c> and <c>reset</c> for the ones that mean to
/// send or clear a form (#138). A guard walks every component and refuses a button that leaves it out, so the next
/// one added cannot change the meaning of the page by silence. The tag is read to the "&gt;" that closes it, past
/// the Razor expressions, the quoted "&gt;" of a title, the "=&gt;" of a handler and the line breaks it carries.
/// </summary>
public class ButtonTypeGuardTests
{
    private const string ButtonTagStart = "<button";

    /// <summary>The three types a <c>&lt;button&gt;</c> may be; anything else is no button type at all.</summary>
    private static readonly string[] AllowedTypes = ["button", "submit", "reset"];

    /// <summary>
    /// A button of the app: where to print it, and what its tag declares as its type. <c>WritesType</c> says whether
    /// a <c>type</c> stands there at all; <c>Type</c> is the plain word written there, or <c>null</c> when no word
    /// can be read - an empty value, or one Razor builds at runtime.
    /// </summary>
    private sealed record Button(string File, int Line, bool WritesType, string? Type)
    {
        /// <summary>Why the guard refuses this button, in the words a failure prints, or <c>null</c> when the button
        /// declares one of the three types HTML offers.</summary>
        public string? Offence =>
            !WritesType
                ? "has no type attribute, so the browser makes it a submit button"
                : Type is null
                    ? "has a type with no word in it - left empty, or built by Razor at runtime - and the browser falls back to submit"
                    : AllowedTypes.Contains(Type, StringComparer.OrdinalIgnoreCase)
                        ? null
                        : $"has type=\"{Type}\", which is not one of {string.Join(", ", AllowedTypes)}";
    }

    /// <summary>Every button of every component declares the type it is: <c>button</c>, <c>submit</c> or
    /// <c>reset</c>, spelled out in the tag.</summary>
    [Fact]
    public void EveryButtonDeclaresItsType()
    {
        var offenders = Buttons()
            .Where(b => b.Offence is not null)
            .Select(b => $"{b.File}:{b.Line} <button> {b.Offence}")
            .ToList();

        Assert.Empty(offenders);
    }

    /// <summary>
    /// A guard that found nothing has seen nothing: the app draws about fifteen buttons today - the pause and check
    /// now of the header, the view tabs, the back buttons of the drill-down views, the audience and fit switches -
    /// so a scan that reaches far fewer never read the markup, and the clean verdict above would mean nothing.
    /// </summary>
    [Fact]
    public void TheGuardReadsTheButtonsOfTheApp()
    {
        var buttons = Buttons();

        Assert.True(
            buttons.Count >= 10,
            $"only {buttons.Count} <button> tag(s) were read under {DashboardRoot()}: the guard is checking nothing");

        Assert.Contains(buttons, b => b.File.Equals("App.razor", StringComparison.Ordinal));
    }

    /// <summary>The rule refuses the button that would submit by silence: none at all, one with nothing in it, one
    /// whose type Razor builds, one of a word HTML does not read on a button - and one that only looks named, since
    /// <c>data-type</c> is another attribute and owes the button nothing.</summary>
    [Fact]
    public void TheRuleRefusesAButtonThatNamesNoType()
    {
        Assert.Equal(
            "has no type attribute, so the browser makes it a submit button",
            OffenceOf("""<button class="button" @onclick="Clear">Clear</button>"""));

        Assert.Contains("no word in it", OffenceOf("""<button type="" @onclick="Clear">Clear</button>"""), StringComparison.Ordinal);
        Assert.Contains("no word in it", OffenceOf("""<button type="@_kind" @onclick="Clear">Clear</button>"""), StringComparison.Ordinal);
        Assert.Contains("not one of", OffenceOf("""<button type="clear" @onclick="Clear">Clear</button>"""), StringComparison.Ordinal);
        Assert.Contains("no type attribute", OffenceOf("""
            <button class="button"
                    data-type="submit"
                    @onclick="Clear">
            """), StringComparison.Ordinal);
    }

    /// <summary>The rule accepts each of the three types, however it is written: double quoted, single quoted,
    /// unquoted, in either case, and among whatever else the tag carries.</summary>
    [Fact]
    public void TheRuleAcceptsEachOfTheThreeTypes()
    {
        Assert.Null(OffenceOf("""<button type="button" class="button" @onclick="Clear">Clear</button>"""));
        Assert.Null(OffenceOf("""<button type='submit' class="button">Send</button>"""));
        Assert.Null(OffenceOf("<button type=reset class=\"button\">Clear the filters</button>"));
        Assert.Null(OffenceOf("""<BUTTON TYPE="BUTTON">Clear</BUTTON>"""));
    }

    /// <summary>A tag ends at the "&gt;" that closes it, not at the first "&gt;" of the file: a title may hold the
    /// "&gt;" of a Razor comparison, a handler the "=&gt;" of a lambda, and the attributes may spread over lines -
    /// the shape of the buttons of <c>Components/RuntimeView.razor</c>. The type read is the button's own, and the
    /// markup it lands on is not read as part of the tag.</summary>
    [Fact]
    public void TheTagReadRunsPastRazorAndLineBreaks()
    {
        var text = """
            <button type="button" class="button @(selected ? "button--selected" : "button--quiet")"
                    title="@(inFlight.Count > 0 ? string.Join('\n', inFlight.Select(DeploymentText.Title)) : null)"
                    @onclick="() => SelectAsync(entry.Name)">
                <span class="badge" type="submit">@count</span>
            </button>
            """;

        var start = ButtonStarts(text).Single();
        var tag = TagOf(text, start);

        Assert.EndsWith(">", tag, StringComparison.Ordinal);
        Assert.Contains("SelectAsync", tag, StringComparison.Ordinal);
        Assert.DoesNotContain("</button", tag, StringComparison.Ordinal);
        Assert.DoesNotContain("badge", tag, StringComparison.Ordinal);

        var (written, type) = DeclaredType(tag);

        Assert.True(written);
        Assert.Equal("button", type);
    }

    /// <summary>The report names the file and the line the tag opens on - the first of them, not the line its last
    /// attribute sits on - and reads every button of a file on its own.</summary>
    [Fact]
    public void TheReportNamesTheFileAndTheLineOfTheTag()
    {
        var text = """
            <div class="toolbar">
                <button class="views__tab"
                        @onclick="() => Pick(view)">Health</button>
                <button type="submit">Send</button>
            </div>
            """;

        var buttons = Scan("Sample.razor", text);

        Assert.Equal(2, buttons.Count);
        Assert.Equal(2, buttons[0].Line);
        Assert.Contains("no type attribute", buttons[0].Offence, StringComparison.Ordinal);
        Assert.Equal(4, buttons[1].Line);
        Assert.Null(buttons[1].Offence);
    }

    /// <summary><c>&lt;button-row&gt;</c> and <c>&lt;buttongroup&gt;</c> are other elements and <c>&lt;/button&gt;</c>
    /// is the end of this one, so only whitespace, "&gt;" or "/" closes the name of a button tag.</summary>
    [Fact]
    public void AButtonNameIsTheWholeOfItsElementName()
    {
        var text = """
            <buttongroup><button-row data-x="1">no</button-row></buttongroup>
            <button/>
            """;

        Assert.Single(ButtonStarts(text));
    }

    /// <summary>Every button of the app, in the order the files write them.</summary>
    private static List<Button> Buttons() =>
        [.. RazorMarkup().SelectMany(markup => Scan(markup.File, markup.Text))];

    /// <summary>The buttons of one file, each with what its tag declares for a type.</summary>
    private static List<Button> Scan(string file, string text)
    {
        var buttons = new List<Button>();

        foreach (var start in ButtonStarts(text))
        {
            var (written, type) = DeclaredType(TagOf(text, start));

            buttons.Add(new Button(file, LineOf(text, start), written, type));
        }

        return buttons;
    }

    /// <summary>Where the text opens a <c>&lt;button&gt;</c>, in the order the text writes them. The name is read
    /// without regard to case, since HTML spells an element name the same whichever way its letters go and a
    /// <c>&lt;BUTTON&gt;</c> defaults to submitting just the same.</summary>
    private static List<int> ButtonStarts(string text)
    {
        var starts = new List<int>();

        for (var at = text.IndexOf(ButtonTagStart, StringComparison.OrdinalIgnoreCase); at >= 0;
             at = text.IndexOf(ButtonTagStart, at + ButtonTagStart.Length, StringComparison.OrdinalIgnoreCase))
        {
            if (EndsTheName(text, at + ButtonTagStart.Length))
            {
                starts.Add(at);
            }
        }

        return starts;
    }

    private static bool EndsTheName(string text, int at) =>
        at < text.Length && (char.IsWhiteSpace(text[at]) || text[at] == '>' || text[at] == '/');

    /// <summary>Whether the tag writes a <c>type</c> at all, and the plain word standing there - <c>null</c> when
    /// the value is empty or built by Razor, which is no type a reader or a browser can be given.</summary>
    private static (bool Written, string? Type) DeclaredType(string tag)
    {
        const string Name = "type=";

        for (var at = tag.IndexOf(Name, StringComparison.OrdinalIgnoreCase); at >= 0;
             at = tag.IndexOf(Name, at + Name.Length, StringComparison.OrdinalIgnoreCase))
        {
            // Only a tag's own whitespace opens an attribute: otherwise a data-type or an aria-type is read as one.
            if (at == 0 || !char.IsWhiteSpace(tag[at - 1]))
            {
                continue;
            }

            return (true, WordOf(tag, at + Name.Length));
        }

        return (false, null);
    }

    /// <summary>The word an attribute value holds, or <c>null</c> when none can be read of it: an empty value, a
    /// quote the tag never closes, or a value Razor builds. The value may be double quoted, single quoted or - one
    /// word with nothing around it, which HTML allows - bare.</summary>
    private static string? WordOf(string tag, int from)
    {
        var quote = from < tag.Length && (tag[from] == '"' || tag[from] == '\'') ? tag[from] : '\0';
        var start = quote == '\0' ? from : from + 1;

        var end = start;

        while (end < tag.Length && (quote == '\0' ? !(char.IsWhiteSpace(tag[end]) || tag[end] == '>') : tag[end] != quote))
        {
            end++;
        }

        if (quote != '\0' && end >= tag.Length)
        {
            return null;
        }

        var value = tag[start..end];

        return value.Length == 0 || value.Contains('@') ? null : value;
    }

    /// <summary>How the first button of a sample is judged - the shape the tests above read the rule through.</summary>
    private static string? OffenceOf(string text)
    {
        var start = ButtonStarts(text).FirstOrDefault(-1);

        Assert.True(start >= 0, $"the sample holds no {ButtonTagStart}> tag: the sample checks nothing");

        var (written, type) = DeclaredType(TagOf(text, start));

        return new Button("Sample.razor", LineOf(text, start), written, type).Offence;
    }

    /// <summary>How the whole opening tag is read, from its "&lt;" to the "&gt;" that closes it - or to the end of
    /// the file when nothing does. An attribute value is run through rather than read to its first quote, because
    /// Razor quotes inside it: <c>title="@(_fit ? "shown at its actual size" : "fitted")"</c> closes with the
    /// fourth quote, not the second, and the "&gt;" of a comparison between them ends nothing. The parentheses of an
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
