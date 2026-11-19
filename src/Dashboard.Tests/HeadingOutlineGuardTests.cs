using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// A page has one title, and HTML says so with one <c>&lt;h1&gt;</c>: a reader who asks for the headings of a page
/// gets an outline to jump through, and the first line of that outline is what the page is about. A component that
/// brings its own <c>&lt;h1&gt;</c> breaks that outline twice over - it buries the name of the system the shell
/// prints under the name of a panel, and it leaves two lines claiming to be the top of the page, which is how a
/// screen reader's heading list turns into a wall of <c>h1</c>. So <c>App.razor</c> holds the page's one
/// <c>&lt;h1&gt;</c> and every other component starts at <c>&lt;h2&gt;</c> under it (#142). A guard walks every
/// component and refuses an <c>&lt;h1&gt;</c> that is not the shell's, so the next heading added cannot start a
/// second outline; the shell itself is refused when it stops printing exactly one.
/// <para>
/// What this guard reads is the markup as written, the way <see cref="SectionNameGuardTests"/>,
/// <see cref="ButtonTypeGuardTests"/> and <see cref="SvgIconGuardTests"/> read theirs: no Razor comment is stripped,
/// no <c>@if</c> is followed, so a heading inside <c>@* ... *@</c> counts just as one inside a branch the page never
/// takes does. That is deliberate - a comment is markup the next reader will move back into place, and the guards of
/// this suite all refuse the shape rather than the reachability.
/// </para>
/// </summary>
public class HeadingOutlineGuardTests
{
    private const string H1TagStart = "<h1";

    /// <summary>The component allowed the page's <c>&lt;h1&gt;</c>: the app shell, at the root of the project.</summary>
    private const string AppShell = "App.razor";

    /// <summary>
    /// A heading of the app: the file to print and the line the tag opens on. An <c>&lt;h1&gt;</c> of a component
    /// other than the shell is the offence this guard refuses, and the shell printing none - or two - is the other.
    /// </summary>
    private sealed record Heading(string File, int Line)
    {
        /// <summary>Where the print of a failure names this heading.</summary>
        public string At => $"{File}:{Line}";
    }

    /// <summary>
    /// No component but the app shell opens an <c>&lt;h1&gt;</c>: every panel, section, card and view starts its own
    /// outline at <c>&lt;h2&gt;</c> or deeper, under the one page title <c>App.razor</c> prints.
    /// </summary>
    [Fact]
    public void NoComponentButTheAppShellOpensAnH1()
    {
        var offenders = Headings()
            .Where(h => !h.File.Equals(AppShell, StringComparison.Ordinal))
            .Select(h => $"{h.At} opens an <h1> outside the app shell: the page already has one title in App.razor, and a heading is a line of the outline a reader jumps through")
            .ToList();

        Assert.Empty(offenders);
    }

    /// <summary>
    /// The app shell prints exactly one <c>&lt;h1&gt;</c> - the name of the system, in the header above every view.
    /// None and two are the same failure seen from the other side: a page whose outline has no top, or more than one.
    /// </summary>
    [Fact]
    public void TheAppShellOpensExactlyOneH1()
    {
        var shell = Headings().Where(h => h.File.Equals(AppShell, StringComparison.Ordinal)).ToList();

        Assert.True(
            shell.Count == 1,
            $"{AppShell} opens {shell.Count} <h1> tag(s) ({string.Join(", ", shell.Select(h => h.At))}); the page needs exactly one, so its title is the first line of the outline");
    }

    /// <summary>
    /// A guard that found nothing has seen nothing: the walk is shown to reach the components this app is built of -
    /// far more than twenty <c>.razor</c> files, the shell and the drill-down views among them - before the clean
    /// verdict above means anything. Were the markup ever moved out of reach, the scan would refuse the run rather
    /// than pass by absence.
    /// </summary>
    [Fact]
    public void TheGuardReadsTheRazorFilesOfTheApp()
    {
        var files = RazorMarkup().ToList();

        Assert.True(
            files.Count >= 20,
            $"only {files.Count} .razor file(s) were read under {DashboardRoot()}: the guard is checking nothing");

        Assert.Contains(files, f => f.File.Equals(AppShell, StringComparison.Ordinal));
        Assert.Contains(files, f => f.File.Equals(Path.Combine("Components", "ClusterView.razor"), StringComparison.Ordinal));
    }

    /// <summary>The rule refuses the heading that starts a second outline, wherever in a component it stands - a
    /// panel of its own, a section, or a view reached by a link - and names the file and the line it opens on.</summary>
    [Fact]
    public void TheRuleRefusesAH1OutsideTheAppShell()
    {
        var text = """
            <div class="cluster">
                <h2 id="cluster-title">Cluster</h2>
                <h1>Nodes</h1>
            </div>
            """;

        var headings = Scan(Path.Combine("Components", "ClusterView.razor"), text);

        var offender = Assert.Single(headings, h => !h.File.Equals(AppShell, StringComparison.Ordinal));

        Assert.Equal(3, offender.Line);
        Assert.Equal(Path.Combine("Components", "ClusterView.razor") + ":3", offender.At);
    }

    /// <summary>The rule accepts every other level: the shell's own title and the <c>&lt;h2&gt;</c>, <c>&lt;h3&gt;</c>
    /// and <c>&lt;h4&gt;</c> the components outline themselves with - a heading one level deeper is what this app
    /// asks a component for instead of an <c>&lt;h1&gt;</c>.</summary>
    [Fact]
    public void TheRuleLeavesEveryOtherHeadingAlone()
    {
        var text = """
            <h1>The system</h1>
            <h2>Cluster</h2>
            <h3>Nodes</h3>
            <h4 class="note">Pods</h4>
            """;

        var headings = Scan(AppShell, text);

        Assert.Single(headings);
        Assert.Equal(AppShell, headings[0].File);
        Assert.Equal(1, headings[0].Line);
    }

    /// <summary><c>&lt;h10&gt;</c> and <c>&lt;h1-lead&gt;</c> are other elements and <c>&lt;/h1&gt;</c> is the end of
    /// this one, so only whitespace or "&gt;" closes the name of an <c>h1</c> tag - and the name is read whichever
    /// way its letters go, since HTML spells an element name the same either way and a <c>&lt;H1&gt;</c> starts a
    /// second outline just the same.</summary>
    [Fact]
    public void AH1NameIsTheWholeOfItsElementName()
    {
        var text = """
            <h10>not a heading of level one</h10>
            <h1-lead class="page">no</h1-lead>
            <p>Under the <h1> of the shell</p>
            <H1>LOUD</H1>
            <h1>quiet</h1>
            """;

        var headings = Scan("Sample.razor", text);

        Assert.Equal(3, headings.Count);
        Assert.Equal([3, 4, 5], headings.Select(h => h.Line));
    }

    /// <summary>An <c>&lt;h1&gt;</c> the file builds on a line of its own, with attributes spread below it, is still
    /// one heading read on the line its tag opened - which is the line a failure prints.</summary>
    [Fact]
    public void ATagThatSpreadsOverLinesIsOneHeadingOnTheLineItOpens()
    {
        var text = """
            <main>
                <h1
                   class="page-header__name"
                   id="system-title">@SystemName</h1>
                <h2>Views</h2>
            </main>
            """;

        var headings = Scan(AppShell, text);

        Assert.Single(headings);
        Assert.Equal(2, headings[0].Line);
    }

    /// <summary>The headings of every component, in the order the files are walked and the tags are written.</summary>
    private static List<Heading> Headings() =>
        [.. RazorMarkup().SelectMany(markup => Scan(markup.File, markup.Text))];

    /// <summary>The <c>&lt;h1&gt;</c> tags of one file, in the order the file writes them.</summary>
    private static List<Heading> Scan(string file, string text) =>
        [.. H1Starts(text).Select(start => new Heading(file, LineOf(text, start)))];

    /// <summary>Where the text opens an <c>&lt;h1&gt;</c>, in the order the text writes them.</summary>
    private static List<int> H1Starts(string text)
    {
        var starts = new List<int>();

        for (var at = text.IndexOf(H1TagStart, StringComparison.OrdinalIgnoreCase); at >= 0;
             at = text.IndexOf(H1TagStart, at + H1TagStart.Length, StringComparison.OrdinalIgnoreCase))
        {
            if (EndsTheName(text, at + H1TagStart.Length))
            {
                starts.Add(at);
            }
        }

        return starts;
    }

    /// <summary>True when the element name read at <paramref name="at"/> ends there: an "<c>h10</c>" is a longer
    /// name, and only whitespace or "&gt;" closes the name of the heading this rule is about.</summary>
    private static bool EndsTheName(string text, int at) =>
        at < text.Length && (char.IsWhiteSpace(text[at]) || text[at] == '>');

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
