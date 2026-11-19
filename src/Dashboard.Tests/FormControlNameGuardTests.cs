using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// A <c>&lt;select&gt;</c>, an <c>&lt;input&gt;</c> and a <c>&lt;textarea&gt;</c> are where a reader fills something
/// in, and a screen reader announces them by name before the value: "Interval, combo box, 30 seconds". With no name
/// there the words "30 seconds" arrive out of nowhere and the reader cannot tell one control of the header from the
/// next (#135). So every control of the app owes itself a name, and it may pay in any of the three coin HTML offers:
/// <list>
///   <item>the control stands inside an open <c>&lt;label&gt;</c>, which names everything it wraps - the shape this
///   app draws its fields with, <c>&lt;label class="field"&gt;&lt;span&gt;Interval&lt;/span&gt;&lt;select ...&gt;</c>;</item>
///   <item>the tag carries <c>aria-label</c> or <c>aria-labelledby</c>, a name written on the control itself;</item>
///   <item>the tag carries a literal <c>id="x"</c> and the same file writes a <c>&lt;label for="x"&gt;</c>, a name
///   reaching in from a label that stands away from the control it names.</item>
/// </list>
/// A guard walks every component and refuses a control that pays in none of them, so the next field added cannot be
/// forgotten. The tag is read to the ">" that closes it, past the Razor expressions, the quoted ">" of a title and
/// the line breaks it carries; an <c>input</c> of <c>type="hidden"</c> is passed over, since nothing is entered into
/// it and no reader is ever told it is there.
/// </summary>
public class FormControlNameGuardTests
{
    /// <summary>The elements that take something in, by the start of their tag.</summary>
    private static readonly string[] ControlNames = ["<select", "<input", "<textarea"];

    private const string LabelOpen = "<label";
    private const string LabelClose = "</label";
    private const string HiddenType = "type=\"hidden\"";

    /// <summary>A control of the app: where to print it, which element it is, and how it is named - <c>null</c> when
    /// nothing names it, which is what the guard refuses.</summary>
    private sealed record Control(string File, int Line, string Element, string? NamedBy);

    /// <summary>
    /// Every control of every component is named: wrapped in a <c>&lt;label&gt;</c>, or labelled by its own
    /// <c>aria-label</c> / <c>aria-labelledby</c>, or pointed at by a <c>&lt;label for&gt;</c> of the same file.
    /// </summary>
    [Fact]
    public void EveryFormControlHasAName()
    {
        var offenders = Controls()
            .Where(c => c.NamedBy is null)
            .Select(c => $"{c.File}:{c.Line} <{c.Element}> has no name: not inside a <label>, no aria-label or aria-labelledby, no <label for> pointing at its id")
            .ToList();

        Assert.Empty(offenders);
    }

    /// <summary>
    /// A guard that found nothing has seen nothing: the scan is shown to reach the controls this app actually draws
    /// - the interval select and the probe select of <c>App.razor</c>, the environment select of
    /// <c>Components/TrafficPanel.razor</c> - before its clean verdict means anything.
    /// </summary>
    [Fact]
    public void TheGuardFindsTheControlsItClaimsToRead()
    {
        var controls = Controls();

        Assert.True(
            controls.Count >= 3,
            $"only {controls.Count} form control(s) were read under {DashboardRoot()}: the guard is checking nothing");

        var inHeader = controls.Where(c => c.File.Equals("App.razor", StringComparison.Ordinal)).ToList();
        var inTraffic = controls.Where(c => c.File.EndsWith(
            Path.Combine("Components", "TrafficPanel.razor"), StringComparison.Ordinal)).ToList();

        Assert.Equal(2, inHeader.Count);
        Assert.Single(inTraffic);
    }

    /// <summary>The rule refuses the control that would read as a nameless box: no label around it, no aria name on
    /// it, no <c>&lt;label for&gt;</c> pointing at it - and it refuses it however long the tag is.</summary>
    [Fact]
    public void TheRuleRefusesAControlNothingNames()
    {
        Assert.Null(NamingOf("""<select value="@_probe" @onchange="OnProbeChanged"></select>"""));
        Assert.Null(NamingOf("""<input class="box" value="@Text" @oninput="OnText" />"""));
        Assert.Null(NamingOf("""<textarea rows="3" @bind="_note"></textarea>"""));
        Assert.Null(NamingOf("""
            <input type="search"
                   value="@Query"
                   @oninput="e => Query = e.Value?.ToString()" />
            """));
    }

    /// <summary>The rule accepts a named control, however it is named: wrapped in a label of its own, named on the
    /// tag, or described by other markup - and the label that wraps it may open on a line of its own above.</summary>
    [Fact]
    public void TheRuleAcceptsAControlThatIsLabelledOrNamed()
    {
        var wrapped = """
            <label class="field">
                <span>Interval</span>
                <select value="@IntervalSeconds" @onchange="OnIntervalChanged">
                    <option value="30">30 seconds</option>
                </select>
            </label>
            """;
        var named = """<input type="text" aria-label="Filter the pods" value="@Filter" />""";
        var described = """
            <p id="note-title">Note</p>
            <textarea aria-labelledby="note-title" @bind="_note"></textarea>
            """;

        Assert.NotNull(NamingOf(wrapped));
        Assert.NotNull(NamingOf(named));
        Assert.NotNull(NamingOf(described));
    }

    /// <summary>A label may stand away from the control it names and point at it by <c>id</c>; the rule accepts that
    /// only when both sides can be read here, so an id built at runtime names nothing and a <c>for</c> that points
    /// elsewhere leaves the control unnamed.</summary>
    [Fact]
    public void TheRuleAcceptsOnlyALiteralIdAPointingLabelWrites()
    {
        var pointedAt = """
            <label for="env">Environment</label>
            <select id="env" value="@_environment" @onchange="OnEnvironmentChanged"></select>
            """;
        var idBuiltByRazor = """
            <label for="env">Environment</label>
            <select id="@($"field-{_environment}")" value="@_environment"></select>
            """;
        var labelPointsAtAnother = """
            <label for="other">Environment</label>
            <input id="env" value="@_environment" />
            """;

        Assert.NotNull(NamingOf(pointedAt));
        Assert.Null(NamingOf(idBuiltByRazor));
        Assert.Null(NamingOf(labelPointsAtAnother));
    }

    /// <summary>Nothing is entered into a hidden input and no reader is told it is there, so it owes no name. The
    /// scan still runs past it: the control that follows it in the same markup is checked.</summary>
    [Fact]
    public void TheRuleSkipsAnInputThatIsHidden()
    {
        var text = """
            <form>
                <input type="hidden" name="returnUrl" value="@ReturnUrl" />
                <input type="text" value="@Name" />
            </form>
            """;

        var scanned = Scan("Sample.razor", text);

        // Only the box after the hidden field is read, and it is refused for want of a name.
        Assert.Single(scanned);
        Assert.Equal(3, scanned[0].Line);
        Assert.Equal("input", scanned[0].Element);
        Assert.Null(scanned[0].NamedBy);
    }

    /// <summary>A tag ends at the ">" that closes it, not at the first ">" of the file: a Razor expression may hold
    /// parentheses, a title may hold a ">" of its own inside the quotes Razor carries, and the attributes may spread
    /// over lines. The label around the control is counted where its tag opens, not where it closes.</summary>
    [Fact]
    public void TheTagReadRunsPastRazorAndLineBreaks()
    {
        var text = """
            <label class="field">
                <span>Environment</span>
                <select value="@_environment"
                        disabled="@(Running ? true : (object?)null)"
                        title="@(Running ? "Locked: stop traffic first > choose later" : null)"
                        aria-describedby="@(Running ? "traffic-environment-locked-help" : null)"
                        @onchange="OnEnvironmentChanged">
                    <option value="@plan.Environment">@plan.Environment</option>
                </select>
            </label>
            """;

        var start = ControlStarts(text).Single();
        var tag = TagOf(text, start);

        Assert.EndsWith(">", tag, StringComparison.Ordinal);
        Assert.Contains("OnEnvironmentChanged", tag, StringComparison.Ordinal);
        Assert.DoesNotContain("<option", tag, StringComparison.Ordinal);
        Assert.Equal("wrapped in a <label>", Naming(text, start, tag));
    }

    /// <summary>An open <c>&lt;label&gt;</c> names what it wraps; one that closed two lines ago names nothing, and a
    /// label of no attributes at all still wraps.</summary>
    [Fact]
    public void OnlyALabelStillOpenWhereTheControlStandsWrapsIt()
    {
        var after = """
            <label class="field"><span>Interval</span></label>
            <select value="@IntervalSeconds"></select>
            """;
        var around = """
            <label><select value="@IntervalSeconds"></select></label>
            """;

        Assert.Null(NamingOf(after));
        Assert.NotNull(NamingOf(around));
    }

    /// <summary><c>&lt;selection&gt;</c> and <c>&lt;inputs&gt;</c> are not the elements this rule is about, so the
    /// search for a control's tag keeps only the ones whose name ends at whitespace, at ">" or at "/".</summary>
    [Fact]
    public void AControlNameIsTheWholeOfItsElementName()
    {
        var text = """
            <selection range="3">nothing</selection>
            <inputdata>@Value</inputdata>
            <input/>
            """;

        Assert.Single(ControlStarts(text));
    }

    /// <summary>Every control of the app, hidden inputs left out, each with the way it is named - or <c>null</c>.</summary>
    private static List<Control> Controls() =>
        [.. RazorMarkup().SelectMany(markup => Scan(markup.File, markup.Text))];

    /// <summary>The controls of one file, in the order the file writes them.</summary>
    private static List<Control> Scan(string file, string text)
    {
        var controls = new List<Control>();

        foreach (var start in ControlStarts(text))
        {
            var tag = TagOf(text, start);

            // A hidden field takes nothing in and no reader reaches it: nothing to name.
            if (IsHidden(tag))
            {
                continue;
            }

            controls.Add(new Control(file, LineOf(text, start), ElementName(tag), Naming(text, start, tag)));
        }

        return controls;
    }

    /// <summary>How the control opening at <paramref name="start"/> is named, in the words a failure prints, or
    /// <c>null</c> when nothing names it.</summary>
    private static string? Naming(string text, int start, string tag)
    {
        if (InsideLabel(text, start))
        {
            return "wrapped in a <label>";
        }

        if (tag.Contains("aria-label=", StringComparison.Ordinal))
        {
            return "aria-label";
        }

        if (tag.Contains("aria-labelledby=", StringComparison.Ordinal))
        {
            return "aria-labelledby";
        }

        // A label standing elsewhere names the control by its id - and only a literal id, since the <label for>
        // written next to it in the same file is read by name here, not by what Razor makes of it at runtime.
        var id = LiteralValueOf(tag, "id");

        if (id is not null && PointsAt(text, id))
        {
            return $"a <label for=\"{id}\"> of the same file";
        }

        return null;
    }

    /// <summary>True when a <c>&lt;label&gt;</c> stands open where the control opens: the labels the file opens
    /// before this point outnumber the ones it closes there. A self-closing <c>&lt;label /&gt;</c> opens nothing.</summary>
    private static bool InsideLabel(string text, int start)
    {
        var open = 0;

        for (var at = text.IndexOf(LabelOpen, StringComparison.Ordinal); at >= 0 && at < start;
             at = text.IndexOf(LabelOpen, at + LabelOpen.Length, StringComparison.Ordinal))
        {
            if (!EndsTheName(text, at + LabelOpen.Length) || TagOf(text, at).EndsWith("/>", StringComparison.Ordinal))
            {
                continue;
            }

            open++;
        }

        var closed = 0;

        for (var at = text.IndexOf(LabelClose, StringComparison.Ordinal); at >= 0 && at < start;
             at = text.IndexOf(LabelClose, at + LabelClose.Length, StringComparison.Ordinal))
        {
            closed++;
        }

        return open > closed;
    }

    /// <summary>True when some <c>&lt;label&gt;</c> of the file points at the given id with its <c>for</c>.</summary>
    private static bool PointsAt(string text, string id)
    {
        var wanted = $"for=\"{id}\"";

        for (var at = text.IndexOf(LabelOpen, StringComparison.Ordinal); at >= 0;
             at = text.IndexOf(LabelOpen, at + LabelOpen.Length, StringComparison.Ordinal))
        {
            if (!EndsTheName(text, at + LabelOpen.Length))
            {
                continue;
            }

            if (TagOf(text, at).Contains(wanted, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Where the file opens a <c>&lt;select&gt;</c>, an <c>&lt;input&gt;</c> or a
    /// <c>&lt;textarea&gt;</c>, in the order the file writes them.</summary>
    private static List<int> ControlStarts(string text)
    {
        var starts = new List<int>();

        foreach (var name in ControlNames)
        {
            for (var at = text.IndexOf(name, StringComparison.Ordinal); at >= 0;
                 at = text.IndexOf(name, at + name.Length, StringComparison.Ordinal))
            {
                if (EndsTheName(text, at + name.Length))
                {
                    starts.Add(at);
                }
            }
        }

        starts.Sort();

        return starts;
    }

    /// <summary>True when the element name read at <paramref name="at"/> ends there: a "<c>selection</c>" is a
    /// longer name, and only whitespace, ">" or "/" closes the name of the element this rule is about.</summary>
    private static bool EndsTheName(string text, int at) =>
        at < text.Length && (char.IsWhiteSpace(text[at]) || text[at] == '>' || text[at] == '/');

    private static bool IsHidden(string tag) => tag.Contains(HiddenType, StringComparison.Ordinal);

    /// <summary>The element of a tag, for the message: the word between the "&lt;" and what follows it.</summary>
    private static string ElementName(string tag)
    {
        var at = 1;

        while (at < tag.Length && !char.IsWhiteSpace(tag[at]) && tag[at] != '>' && tag[at] != '/')
        {
            at++;
        }

        return tag[1..at];
    }

    /// <summary>The value of a plain attribute of the tag, or <c>null</c> when the tag writes none of that name or
    /// builds its value at Razor runtime.</summary>
    private static string? LiteralValueOf(string tag, string name)
    {
        var opening = name + "=\"";

        for (var at = tag.IndexOf(opening, StringComparison.Ordinal); at >= 0;
             at = tag.IndexOf(opening, at + opening.Length, StringComparison.Ordinal))
        {
            // Only a tag's own whitespace opens an attribute: otherwise a data-id or a guid= would count as one.
            if (at > 0 && !(char.IsWhiteSpace(tag[at - 1]) || tag[at - 1] == '<'))
            {
                continue;
            }

            var from = at + opening.Length;
            var end = tag.IndexOf('"', from);

            if (end < 0)
            {
                return null;
            }

            var value = tag[from..end];

            return value.Contains('@') ? null : value;
        }

        return null;
    }

    /// <summary>How the sample's first control is named - the shape the tests above read the rule through.</summary>
    private static string? NamingOf(string text)
    {
        var start = ControlStarts(text).FirstOrDefault(-1);

        Assert.True(start >= 0, $"the sample holds no {string.Join(", ", ControlNames)} tag: the sample checks nothing");

        return Naming(text, start, TagOf(text, start));
    }

    /// <summary>How the whole opening tag is read, from its "&lt;" to the "&gt;" that closes it - or to the end of
    /// the file when nothing does. An attribute value is run through rather than read to its first quote, because
    /// Razor quotes inside it: <c>title="@(Running ? "stop traffic first &gt; choose later" : null)"</c> closes with
    /// the fourth quote, not the second, and the "&gt;" between them ends nothing. The parentheses of an expression
    /// outside any value are counted too, the way the other guards count them.</summary>
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
    /// value is never closed. A <c>@(</c> inside the value is read to the <c>)</c> that matches it, string literals
    /// of the expression and their quotes skipped over.</summary>
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
