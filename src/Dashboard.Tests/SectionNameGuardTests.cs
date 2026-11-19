using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// A <c>&lt;section&gt;</c> is a landmark: a screen reader lists the regions of a page by their names, and a section
/// with no name is no region at all - its heading, its tables and its buttons arrive as an unnamed lump in the middle
/// of the reading order, with nothing to jump to. So every section of the app pays for a name in one of the three
/// coins HTML offers (#139):
/// <list>
///   <item><c>aria-labelledby</c>, pointing at the heading it holds - the shape this app draws its regions with,
///   <c>&lt;section class="cluster" aria-labelledby="cluster-title"&gt;&lt;h2 id="cluster-title"&gt;</c>;</item>
///   <item><c>aria-label</c>, a name written on the section itself, for a name built at runtime that no heading
///   carries - <c>&lt;section class="work__stats" aria-label="Work item statistics"&gt;</c>;</item>
///   <item>a <c>role</c>, which says what the region is when a name would say no more - the load failures of
///   <c>RuntimeView.razor</c> are <c>role="alert"</c>: a live region is announced whatever it is named by.</item>
/// </list>
/// A guard walks every component and refuses a section that pays in none of them, so the next region added cannot be
/// forgotten. The tag is read to the "&gt;" that closes it, past the Razor expressions, the quoted values that hold a
/// "&gt;" of their own and the line breaks they spread over.
/// <para>
/// What this guard deliberately does not check: whether an <c>aria-labelledby</c> points at an <c>id</c> the same
/// file actually writes. <see cref="AriaReferenceGuardTests"/> already walks every <c>aria-labelledby</c> and
/// <c>aria-describedby</c> of every component - the sections' own pointers among them - and refuses a literal
/// pointer whose id is nowhere in the file, so that half is not done twice. Here a pointer counts as a name,
/// present or absent; a section named by a pointer that leads nowhere fails over there.
/// </para>
/// </summary>
public class SectionNameGuardTests
{
    private const string SectionTag = "<section";

    /// <summary>What a section may pay with, in the order the message names them.</summary>
    private static readonly string[] NamingAttributes = ["aria-labelledby", "aria-label", "role"];

    /// <summary>
    /// A section of the app: the file to print, the line its tag opens on, the tag itself, and how it is named -
    /// <c>null</c> when nothing names it, which is what the guard refuses.
    /// </summary>
    private sealed record Section(string File, int Line, string Tag, string? NamedBy);

    /// <summary>
    /// Every section of every component names itself or says what it is: an <c>aria-labelledby</c>, an
    /// <c>aria-label</c> or a <c>role</c> on its own opening tag.
    /// </summary>
    [Fact]
    public void EverySectionHasANameOrARole()
    {
        var offenders = Sections()
            .Where(s => s.NamedBy is null)
            .Select(s => $"{s.File}:{s.Line} <section> names itself with none of {string.Join(", ", NamingAttributes)}: a reader reaches an unnamed region")
            .ToList();

        Assert.Empty(offenders);
    }

    /// <summary>
    /// A guard that found nothing has seen nothing: the scan is shown to reach the regions this app actually draws -
    /// far more than twenty of them, spread over most of its components, the environment sections of
    /// <c>App.razor</c> and the cluster section of <c>Components/ClusterView.razor</c> among them - before its clean
    /// verdict means anything. The count is the floor the app sits well above today; were the markup ever moved out
    /// of reach, the guard would refuse the run rather than pass by absence.
    /// </summary>
    [Fact]
    public void TheGuardFindsTheSectionsItClaimsToRead()
    {
        var sections = Sections();

        Assert.True(
            sections.Count >= 20,
            $"only {sections.Count} section(s) were read under {DashboardRoot()}: the guard is checking nothing");

        var files = sections.Select(s => s.File).ToHashSet(StringComparer.Ordinal);

        Assert.True(
            files.Count >= 10,
            $"the {sections.Count} section(s) read under {DashboardRoot()} live in only {files.Count} file(s): the walk does not reach the app");

        Assert.Contains(sections, s => s.File.Equals("App.razor", StringComparison.Ordinal));
        Assert.Contains(sections, s => s.File.Equals(
            Path.Combine("Components", "ClusterView.razor"), StringComparison.Ordinal));
    }

    /// <summary>The rule refuses the section a reader would find as an unnamed lump: no pointer to a heading, no name
    /// of its own, no role - and it refuses it however long the tag is.</summary>
    [Fact]
    public void TheRuleRefusesASectionNothingNames()
    {
        Assert.Null(NamingOf("""<section class="card">"""));
        Assert.Null(NamingOf("""<section></section>"""));
        Assert.Null(NamingOf("""
            <section class="deployable @(Entering ? "deployable--entering" : null)"
                     @ref="_section">
            """));
        Assert.Null(NamingOf("""<section aria-labelledby="">"""));
    }

    /// <summary>The rule accepts a section, however it pays: by a pointer to its heading, by a name of its own built
    /// at runtime, or by a role that says what the region is.</summary>
    [Fact]
    public void TheRuleAcceptsASectionThatNamesItself()
    {
        var labelledByHeading = """
            <section class="cluster" aria-labelledby="cluster-title">
                <h2 id="cluster-title">Cluster</h2>
            </section>
            """;
        var namedByRazor = """<section class="card card--code" aria-labelledby="@($"{IdPrefix}-code")">""";
        var namedOnTheTag = """<section class="work__stats" aria-label="Work item statistics">""";
        var aLiveRegion = """<section class="problem" role="alert">""";

        Assert.NotNull(NamingOf(labelledByHeading));
        Assert.NotNull(NamingOf(namedByRazor));
        Assert.NotNull(NamingOf(namedOnTheTag));
        Assert.NotNull(NamingOf(aLiveRegion));
    }

    /// <summary>An attribute is only the section's own when the tag's whitespace opens it: <c>data-role</c>,
    /// <c>data-aria-label</c> and <c>roles</c> say nothing to a reader, so none of them buys a pass.</summary>
    [Fact]
    public void OnlyTheSectionsOwnAttributesNameIt()
    {
        Assert.Null(NamingOf("""<section class="card" data-role="alert">"""));
        Assert.Null(NamingOf("""<section class="card" data-aria-label="Cluster">"""));
        Assert.Null(NamingOf("""<section class="card" roles="region">"""));
    }

    /// <summary>A tag ends at the "&gt;" that closes it, not at the first "&gt;" of the file: a Razor expression may
    /// hold a comparison, a title may hold a "&gt;" of its own inside the quotes Razor carries, and the attributes
    /// may spread over lines. The name is read wherever in the tag it stands.</summary>
    [Fact]
    public void TheTagReadRunsPastRazorAndLineBreaks()
    {
        var text = """
            <section class="environment @(Entering ? "environment--entering" : null)"
                     title="@($"Deployed {Build} > see the pipeline")"
                     aria-labelledby="@($"{IdPrefix}-title")"
                     @ref="_section">
                <h2 id="@($"{IdPrefix}-title")">Deployable</h2>
            </section>
            """;

        var start = SectionStarts(text).Single();
        var tag = TagOf(text, start);

        Assert.EndsWith(">", tag, StringComparison.Ordinal);
        Assert.DoesNotContain("<h2", tag, StringComparison.Ordinal);
        Assert.Equal("aria-labelledby", Naming(tag));
    }

    /// <summary><c>&lt;sections&gt;</c> and <c>&lt;sectional&gt;</c> are not the element this rule is about, so the
    /// search keeps only the tags whose name ends at whitespace, at "/" or at "&gt;" - and one section per opening
    /// tag, never the <c>&lt;/section&gt;</c> that closes it.</summary>
    [Fact]
    public void ASectionNameIsTheWholeOfItsElementName()
    {
        var text = """
            <sections id="other">nothing</sections>
            <sectional-panel />
            <section aria-label="One">
                <p>Inner markup, and a closing tag below.</p>
            </section>
            """;

        var scanned = Scan("Sample.razor", text);

        Assert.Single(scanned);
        Assert.Equal(3, scanned[0].Line);
        Assert.Equal("aria-label", scanned[0].NamedBy);
    }

    /// <summary>A section whose name lives on a line of its own, below the one its tag opened on, is still named:
    /// the read runs to the "&gt;" that closes the tag, so the line reported is where the tag starts.</summary>
    [Fact]
    public void ANameOnALaterLineStillNamesTheSection()
    {
        var text = """
            <main>
                <section class="runtime__facts"
                         aria-label="Availability, cost, code and delivery">
                </section>
            </main>
            """;

        var scanned = Scan("Sample.razor", text);

        Assert.Single(scanned);
        Assert.Equal(2, scanned[0].Line);
        Assert.Contains("aria-label", scanned[0].Tag, StringComparison.Ordinal);
        Assert.Equal("aria-label", scanned[0].NamedBy);
    }

    /// <summary>Every section of every component, each with the way it is named - or <c>null</c>.</summary>
    private static List<Section> Sections() =>
        [.. RazorMarkup().SelectMany(markup => Scan(markup.File, markup.Text))];

    /// <summary>The sections of one file, in the order the file writes them.</summary>
    private static List<Section> Scan(string file, string text)
    {
        var sections = new List<Section>();

        foreach (var start in SectionStarts(text))
        {
            var tag = TagOf(text, start);
            sections.Add(new Section(file, LineOf(text, start), tag, Naming(tag)));
        }

        return sections;
    }

    /// <summary>How a section is named, from its opening tag, in the words a failure prints - or <c>null</c> when
    /// nothing names it. The whole tag is searched, not its first line: the attributes of a section often spread.
    /// <c>aria-labelledby</c> is asked for as a name only - whether the id it points at exists is
    /// <see cref="AriaReferenceGuardTests"/>'s business, not this guard's.</summary>
    private static string? Naming(string tag)
    {
        foreach (var attribute in NamingAttributes)
        {
            if (HasAttribute(tag, attribute))
            {
                return attribute;
            }
        }

        return null;
    }

    /// <summary>True when the tag carries an attribute of exactly this name with a value of its own: only the tag's
    /// whitespace or its "&lt;" opens an attribute, so <c>data-role</c> is not <c>role</c> and the name has to be
    /// followed by "=".</summary>
    private static bool HasAttribute(string tag, string name)
    {
        var opening = name + "=\"";

        for (var at = tag.IndexOf(opening, StringComparison.Ordinal); at >= 0;
             at = tag.IndexOf(opening, at + opening.Length, StringComparison.Ordinal))
        {
            if (at > 0 && !(char.IsWhiteSpace(tag[at - 1]) || tag[at - 1] == '<'))
            {
                continue;
            }

            // An attribute whose value is empty names nothing: aria-labelledby="" points at no id at all.
            if (at + opening.Length < tag.Length && tag[at + opening.Length] != '"')
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Where the file opens a <c>&lt;section&gt;</c>, in the order the file writes them.</summary>
    private static List<int> SectionStarts(string text)
    {
        var starts = new List<int>();

        for (var at = text.IndexOf(SectionTag, StringComparison.Ordinal); at >= 0;
             at = text.IndexOf(SectionTag, at + SectionTag.Length, StringComparison.Ordinal))
        {
            if (EndsTheName(text, at + SectionTag.Length))
            {
                starts.Add(at);
            }
        }

        return starts;
    }

    /// <summary>True when the element name read at <paramref name="at"/> ends there: a "<c>sections</c>" is a longer
    /// name, and only whitespace, "&gt;" or "/" closes the name of the element this rule is about.</summary>
    private static bool EndsTheName(string text, int at) =>
        at < text.Length && (char.IsWhiteSpace(text[at]) || text[at] == '>' || text[at] == '/');

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

    /// <summary>The naming of the sample's first section - the shape the rule is read through above.</summary>
    private static string? NamingOf(string text)
    {
        var start = SectionStarts(text).FirstOrDefault(-1);

        Assert.True(start >= 0, $"the sample holds no {SectionTag} tag: the sample checks nothing");

        return Naming(TagOf(text, start));
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
