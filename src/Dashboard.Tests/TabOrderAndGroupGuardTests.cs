using System.Globalization;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// Two things about this markup a viewer cannot defend themselves against, and neither of them can be settled by
/// reading one component on its own: where Tab goes next, and what a group answers with when a reader asks what it
/// holds.
/// <para>
/// A <c>tabindex</c> above zero takes an element out of the order of the document and puts it in front of everything
/// written after it (#147). One "helpful" number on a card buried in the middle of a page therefore reorders the Tab
/// stops of the whole page - the header's pause, interval and probe first among them - and every later number has to
/// be raised to beat it, until the order is a scoreboard nobody can hold in their head. HTML already says where an
/// element belongs: <c>tabindex="0"</c> keeps it in the document's own order, which is what the scroll boxes and the
/// diagram canvases of this app write, and <c>tabindex="-1"</c> keeps it out of Tab while script may still move the
/// focus to it. Nothing above zero is wanted here.
/// </para>
/// <para>
/// <c>role="group"</c> says "these controls belong together" and, on its own, nothing else: a reader reaches it and
/// says "group", which tells a viewer nothing in a page that groups the traffic controls, the legend, the checks, the
/// filters and the links alike. So every group pays for a name in one of the two coins HTML offers - an
/// <c>aria-label</c> written on it, or an <c>aria-labelledby</c> pointing at the heading that names it - and a name
/// Razor builds at runtime names it just the same. <c>aria-describedby</c> is help text, not a name, and
/// <c>role="radiogroup"</c>, <c>role="region"</c> are other roles with other rules.
/// </para>
/// <para>
/// A guard walks every component and refuses both faults, listing the file and the line the tag opens on, so the next
/// control added cannot be forgotten. A tag is read to the "&gt;" that closes it, past the Razor expressions, the
/// quoted values that carry a "&gt;" of their own and the line breaks they spread over, the way the other markup
/// guards read them - and its attributes are read as pairs, so a word of a value is never taken for an attribute and
/// <c>data-tabindex</c> is never taken for <c>tabindex</c>.
/// </para>
/// </summary>
public class TabOrderAndGroupGuardTests
{
    private const string TabindexAttribute = "tabindex";
    private const string RoleAttribute = "role";
    private const string GroupRole = "group";

    /// <summary>The coins a group may pay its name with.</summary>
    private static readonly string[] NamingAttributes = ["aria-label", "aria-labelledby"];

    /// <summary>
    /// One element of the markup these two rules have a word about: it writes a <c>tabindex</c>, or its own
    /// <c>role</c> calls it a group - or both. The file to print, the line its tag opens on, its element name in
    /// lower case, the tag itself, and what the two rules ask of it.
    /// </summary>
    private sealed record Element(string File, int Line, string Name, string Tag)
    {
        /// <summary>The tag's own <c>tabindex</c>, one entry per attribute written there, as the words between the
        /// quotes - Razor and all; <c>null</c> when nothing readable follows the "=".</summary>
        public required List<string?> Tabindex { get; init; }

        /// <summary>True when the tag's own <c>role</c> holds the <c>group</c> token among its words.</summary>
        public required bool IsGroup { get; init; }

        /// <summary>True when a group is named by a non-empty <c>aria-label</c> or <c>aria-labelledby</c> of its
        /// own - a name Razor builds counts, an empty one does not.</summary>
        public required bool HasName { get; init; }

        /// <summary>The <c>tabindex</c> this tag writes that are a number above zero, as the tag spells them - empty
        /// when every one of them leaves the document's own order alone.</summary>
        public List<string> JumpsTheOrder => [.. Tabindex.Where(value => JumpOf(value) is not null).Select(value => value!)];

        /// <summary>Why the guard refuses this element, one sentence per fault - empty when it is guilty of
        /// neither. Both rules are reported at once, so a fixer is not sent back for the second one.</summary>
        public List<string> Faults
        {
            get
            {
                var faults = new List<string>();

                foreach (var value in JumpsTheOrder)
                {
                    faults.Add(
                        $"{TabindexAttribute}=\"{value}\" puts <{Name}> ahead of everything written after it, so Tab walks the page in an order nobody reads in");
                }

                if (IsGroup && !HasName)
                {
                    faults.Add(
                        $"<{Name} {RoleAttribute}=\"{GroupRole}\"> has neither a non-empty aria-label nor aria-labelledby of its own, so a reader hears \"{GroupRole}\" and nothing else");
                }

                return faults;
            }
        }

        /// <summary>The fault of the element, or <c>null</c> when there is none.</summary>
        public string? Offence => Faults.Count == 0 ? null : string.Join("; ", Faults);

        /// <summary>Where the fault stands: the file as the walk prints it, the line the tag opens on, and why.</summary>
        public string Report => $"{File}:{Line} {Offence}";
    }

    /// <summary>
    /// No element of any component jumps the Tab order: every <c>tabindex</c> written in the app reads as a number
    /// that leaves the document's own order alone - <c>0</c> - or asks out of it - <c>-1</c>.
    /// </summary>
    [Fact]
    public void NothingIsTabbedAheadOfTheDocument()
    {
        var offenders = Elements()
            .Where(e => e.JumpsTheOrder.Count > 0)
            .Select(e => e.Report)
            .ToList();

        Assert.Empty(offenders);
    }

    /// <summary>
    /// Every group of every component names itself: a non-empty <c>aria-label</c> or <c>aria-labelledby</c> stands on
    /// the very tag that writes <c>role="group"</c>.
    /// </summary>
    [Fact]
    public void EveryGroupNamesItself()
    {
        var offenders = Elements()
            .Where(e => e.IsGroup && !e.HasName)
            .Select(e => e.Report)
            .ToList();

        Assert.Empty(offenders);
    }

    /// <summary>
    /// A guard that found nothing has seen nothing: the app writes ten <c>tabindex</c> attributes - the scroll boxes
    /// of <c>PodTable.razor</c> and <c>ClusterNodes.razor</c>, the canvases of <c>RuntimeView.razor</c>,
    /// <c>DatabaseView.razor</c> and <c>ApplicationView.razor</c>, the log of <c>EventsStrip.razor</c> - and names
    /// nine groups, from the traffic controls of <c>TrafficPanel.razor</c> to the checks of <c>App.razor</c>. A scan
    /// that reaches far fewer never read the markup, and the two clean verdicts above would mean nothing; the floors
    /// are set well below those counts, so the day the app gains a stop or a group the test still says what it reads.
    /// </summary>
    [Fact]
    public void TheGuardReadsTheTabOrderAndTheGroupsOfTheApp()
    {
        var elements = Elements();
        List<(Element Element, string? Value)> tabindexes = [.. elements.SelectMany(e => e.Tabindex.Select(value => (Element: e, Value: value)))];
        var groups = elements.Where(e => e.IsGroup).ToList();

        Assert.True(
            tabindexes.Count >= 5,
            $"only {tabindexes.Count} tabindex attribute(s) were read under {DashboardRoot()}: the guard is checking nothing");

        Assert.True(
            groups.Count >= 5,
            $"only {groups.Count} role=\"{GroupRole}\" element(s) were read under {DashboardRoot()}: the guard is checking nothing");

        var stopFiles = tabindexes.Select(t => t.Element.File).ToHashSet(StringComparer.Ordinal);
        var groupFiles = groups.Select(g => g.File).ToHashSet(StringComparer.Ordinal);

        Assert.True(
            stopFiles.Count >= 3,
            $"the {tabindexes.Count} tabindex attribute(s) read under {DashboardRoot()} live in only {stopFiles.Count} file(s): the walk does not reach the app");

        Assert.True(
            groupFiles.Count >= 3,
            $"the {groups.Count} group(s) read under {DashboardRoot()} live in only {groupFiles.Count} file(s): the walk does not reach the app");

        Assert.Contains(tabindexes, t => t.Element.File.Equals(Path.Combine("Components", "PodTable.razor"), StringComparison.Ordinal));
        Assert.Contains(tabindexes, t => t.Element.File.Equals(Path.Combine("Components", "EventsStrip.razor"), StringComparison.Ordinal));
        Assert.Contains(groups, g => g.File.Equals(Path.Combine("Components", "TrafficPanel.razor"), StringComparison.Ordinal));
        Assert.Contains(groups, g => g.File.Equals("App.razor", StringComparison.Ordinal));

        // What the guard reads of the app is what the markup writes: every stop of today keeps the document's order.
        Assert.All(tabindexes, t => Assert.True(JumpOf(t.Value) is null, $"{t.Element.Report}: a tabindex of the app jumps the Tab order"));
    }

    /// <summary>The rule refuses a tabindex that is a number above zero, however the tag is spelled: the number
    /// stands in the report that refuses it as the tag writes it - with a leading "+", padded with spaces, spread
    /// over lines of its own - and a value that carries Razor's quotes and a "&gt;" of its own must not hide the one
    /// written after it.</summary>
    [Fact]
    public void TheRuleRefusesATabindexThatJumpsTheOrder()
    {
        Assert.Contains("tabindex=\"2\"", OffenceOf("""<div class="card" tabindex="2" role="region" aria-label="Facts">—</div>"""), StringComparison.Ordinal);
        Assert.Contains("tabindex=\"1\"", OffenceOf("""<button type="button" tabindex="1">Go</button>"""), StringComparison.Ordinal);
        Assert.Contains("tabindex=\"+3\"", OffenceOf("""<a href="/work" tabindex="+3">Work</a>"""), StringComparison.Ordinal);
        Assert.Contains("tabindex=\" 7 \"", OffenceOf("""<input type="search" aria-label="Filter" tabindex=" 7 " />"""), StringComparison.Ordinal);
        Assert.Contains("tabindex=\"5\"", OffenceOf("""<div tabindex="5"><span tabindex="5">twice the same fault</span></div>"""), StringComparison.Ordinal);
        Assert.Contains("tabindex=\"4\"", OffenceOf("""
            <div class="canvas @(wide ? "canvas--wide" : null)"
                 tabindex="4">
                the diagram
            </div>
            """), StringComparison.Ordinal);

        // A value that carries Razor's own quotes and a "&gt;" of its own must hide neither the tabindex written
        // after it nor the role of the group that follows.
        Assert.Contains("tabindex=\"6\"", OffenceOf("""<div title="@($"3 requests > the last minute")" tabindex="6">…</div>"""), StringComparison.Ordinal);
        Assert.Contains("aria-label", OffenceOf("""<div class="legend @(dark ? "legend--dark" : null)" title="more > than a name" role="group">…</div>"""), StringComparison.Ordinal);
    }

    /// <summary>A tag that writes its <c>tabindex</c> twice owes both of them a word, and both rules answer at once:
    /// a jump of the order and an unnamed group are reported together, so a fixer is not sent back for the second one
    /// by the next run.</summary>
    [Fact]
    public void EveryFaultOfATagIsReported()
    {
        var scanned = Scan("Sample.razor", """<div tabindex="2" tabindex="4" role="group" aria-label="">one tag, three faults</div>""");

        Assert.Single(scanned);
        Assert.Equal(2, scanned[0].JumpsTheOrder.Count);
        Assert.Equal(3, scanned[0].Faults.Count);
        Assert.Contains("tabindex=\"2\"", scanned[0].Offence!, StringComparison.Ordinal);
        Assert.Contains("tabindex=\"4\"", scanned[0].Offence!, StringComparison.Ordinal);
        Assert.Contains("aria-label", scanned[0].Offence!, StringComparison.Ordinal);
    }

    /// <summary>The rule accepts the tabindex that leaves the order alone and the value no one can read yet:
    /// <c>0</c> keeps an element where the document puts it, <c>-1</c> takes it out of Tab, a value Razor builds is
    /// nobody's guess, a word that is no number is no number, and an attribute that merely carries the name -
    /// <c>data-tabindex</c>, <c>tabindexfoo</c> - is another attribute.</summary>
    [Fact]
    public void TheRuleAcceptsATabindexThatKeepsTheDocumentOrder()
    {
        Assert.Null(OffenceOf("""<div class="table-scroll" role="region" aria-label="Tables" tabindex="0">…</div>"""));
        Assert.Null(OffenceOf("""<div class="events__log" role="log" aria-labelledby="events-title" tabindex="-1">…</div>"""));
        Assert.Null(OffenceOf("""<div tabindex="@_stop" role="region" aria-label="Canvas">…</div>"""));
        Assert.Null(OffenceOf("""<div tabindex="@(_quiet ? "-1" : "0")" role="region" aria-label="Canvas">…</div>"""));
        Assert.Null(OffenceOf("""<div tabindex="" role="region" aria-label="Canvas">…</div>"""));
        Assert.Null(OffenceOf("""<div tabindex role="region" aria-label="Canvas">…</div>"""));
        Assert.Null(OffenceOf("""<div tabindex="ten" role="region" aria-label="Canvas">…</div>"""));
        Assert.Null(OffenceOf("""<div tabindex="1.5" role="region" aria-label="Canvas">…</div>"""));
        Assert.Null(OffenceOf("""<span role="group" aria-label="Filters" data-tabindex="9">…</span>"""));
        Assert.Null(OffenceOf("""<span role="group" aria-label="Filters" tabindexfoo="9">…</span>"""));
        Assert.Null(OffenceOf("""<span role="group" aria-label="The title says tabindex=&quot;9&quot;">…</span>"""));
    }

    /// <summary>The rule refuses a group that pays it no name: nothing at all, an empty label, a label of spaces, a
    /// pointer to nothing, a pointer written as another attribute, and a description, which is help text rather than
    /// a name.</summary>
    [Fact]
    public void TheRuleRefusesAGroupThatNamesNothing()
    {
        Assert.Contains("aria-label", OffenceOf("""<div class="controls" role="group"><button type="button">Pause</button></div>"""), StringComparison.Ordinal);
        Assert.Contains("aria-label", OffenceOf("""<div role="group" aria-label="">Filters</div>"""), StringComparison.Ordinal);
        Assert.Contains("aria-label", OffenceOf("""<div role="group" aria-label="   ">Filters</div>"""), StringComparison.Ordinal);
        Assert.Contains("aria-label", OffenceOf("""<div role="group" aria-labelledby="">Filters</div>"""), StringComparison.Ordinal);
        Assert.Contains("aria-label", OffenceOf("""<div role="group" data-aria-label="Filters">Filters</div>"""), StringComparison.Ordinal);
        Assert.Contains("aria-label", OffenceOf("""<div role="group" aria-describedby="filters-hint">Filters</div>"""), StringComparison.Ordinal);
        Assert.Contains("aria-label", OffenceOf("""<p class="resource-links" role="group">links</p>"""), StringComparison.Ordinal);
        Assert.Contains("role=\"group\"", OffenceOf("""
            <fieldset class="runtime-legend"
                      role="group">
                <span>Healthy</span>
            </fieldset>
            """), StringComparison.Ordinal);
    }

    /// <summary>The rule accepts the groups the app actually draws, in either coin and in either spelling of the
    /// value: a name in words, a name Razor builds, a pointer to a heading, and a pointer whose id Razor assembles.
    /// The token is what counts, so a role that holds <c>group</c> among its words owes a name too.</summary>
    [Fact]
    public void TheRuleAcceptsAGroupThatNamesItself()
    {
        Assert.Null(OffenceOf("""<div class="controls" role="group" aria-label="Traffic">…</div>"""));
        Assert.Null(OffenceOf("""<div class="work__filter" aria-label="Whose work" role="group">…</div>"""));
        Assert.Null(OffenceOf("""<p class="resource-links" role="group" aria-label="@Label">…</p>"""));
        Assert.Null(OffenceOf("""<div class="facts-row" role="group" aria-label="@($"Other releases in {Environment.Name}")">…</div>"""));
        Assert.Null(OffenceOf("""<div class="cluster-card__block" role="group" aria-labelledby="cluster-service-metrics">…</div>"""));
        Assert.Null(OffenceOf("""<div role="group" aria-labelledby="@($"{IdPrefix}-metrics")">…</div>"""));
        Assert.Null(OffenceOf("""<div role="toolbar group" aria-label="Both words at once">…</div>"""));
        Assert.Null(OffenceOf("""
            <div class="runtime__environments @(wide ? "runtime__environments--wide" : null)"
                 role="group"
                 aria-label="Environment">
                …
            </div>
            """));
    }

    /// <summary>Only the element's own tag names it: an <c>aria-label</c> on a child is that child's business, and a
    /// <c>role="radiogroup"</c> is another role, which this rule does not read as <c>group</c> however much the words
    /// overlap.</summary>
    [Fact]
    public void OnlyTheGroupsOwnAttributesNameIt()
    {
        Assert.Contains("aria-label", OffenceOf("""
            <div role="group">
                <span aria-label="The child's own name">2</span>
                <span aria-labelledby="somewhere-else">other</span>
            </div>
            """), StringComparison.Ordinal);

        // A radiogroup is interesting here only for the tabindex it writes; its name owes this rule nothing.
        var radiogroup = Scan("Sample.razor", """<div role="radiogroup" tabindex="0">…</div>""");

        Assert.Single(radiogroup);
        Assert.False(radiogroup[0].IsGroup);
        Assert.Null(radiogroup[0].Offence);
    }

    /// <summary>HTML spells a tag, an attribute and a role the same whichever way its letters go, so both rules read
    /// them that way: an uppercase <c>TABINDEX</c> jumps the order and a <c>ROLE="GROUP"</c> owes a name, and a value
    /// quoted with single quotes - which HTML allows - is the same value.</summary>
    [Fact]
    public void TheRulesReadTheSpellingOfTheSameWords()
    {
        var scanned = Scan("Sample.razor", """<DIV TABINDEX="2" ROLE="GROUP">…</DIV>""");

        Assert.Equal("div", scanned[0].Name);
        Assert.True(scanned[0].IsGroup);
        Assert.Equal(2, scanned[0].Faults.Count);

        Assert.Contains("tabindex=\"1\"", OffenceOf("""<span tabindex = '1' role = 'group' aria-label = 'Filters'>…</span>"""), StringComparison.Ordinal);
    }

    /// <summary>A tag ends at the "&gt;" that closes it, not at the first "&gt;" of the file: a class may hold a
    /// comparison, a title a "&gt;" of its own inside the quotes Razor carries, and the attributes - among them
    /// <c>tabindex</c> and <c>role</c> - may spread over lines. One element is read per opening tag, never the
    /// <c>&lt;/div&gt;</c> that closes it, and the attribute of each is its own: what stands between the quotes of a
    /// value says nothing to these rules.</summary>
    [Fact]
    public void TheTagReadRunsPastRazorAndLineBreaks()
    {
        var text = """
            <main>
                <div class="canvas @(wide ? "canvas--wide" : null)"
                     title="@($"3 requests > the last minute")"
                     tabindex="0">
                    <path d="M2 8h12" />
                </div>
                <div class="legend"
                     title="more > than a name"
                     role="group">
                    <span>Healthy</span>
                </div>
            </main>
            """;

        var scanned = Scan("Sample.razor", text);

        Assert.Equal(2, scanned.Count);

        Assert.Equal("div", scanned[0].Name);
        Assert.Equal(2, scanned[0].Line);
        Assert.DoesNotContain("<path", scanned[0].Tag, StringComparison.Ordinal);
        Assert.Null(scanned[0].Offence);

        Assert.Equal(7, scanned[1].Line);
        Assert.True(scanned[1].IsGroup);
        Assert.Contains("role=\"group\"", scanned[1].Offence!, StringComparison.Ordinal);
    }

    /// <summary>The report names the file the way the walk prints it and the line the tag opens on - the first of
    /// them, not the line its <c>tabindex</c> or its <c>role</c> sits on - and reads every element of a file on its
    /// own.</summary>
    [Fact]
    public void TheReportNamesTheFileAndTheLineOfTheTag()
    {
        var text = """
            <div class="toolbar">
                <div class="controls"
                     role="group">
                    <button type="button">Go</button>
                </div>
                <a href="/work"
                   tabindex="3">Work</a>
            </div>
            """;

        var scanned = Scan(Path.Combine("Components", "Sample.razor"), text);

        Assert.Equal(2, scanned.Count);

        Assert.Equal(2, scanned[0].Line);
        Assert.StartsWith("Components", scanned[0].Report, StringComparison.Ordinal);
        Assert.Contains("aria-label", scanned[0].Report, StringComparison.Ordinal);

        Assert.Equal(6, scanned[1].Line);
        Assert.Contains("tabindex=\"3\"", scanned[1].Report, StringComparison.Ordinal);
    }

    /// <summary>Every element of every component that writes a <c>tabindex</c> or calls itself a group, in the order
    /// the files write them.</summary>
    private static List<Element> Elements() =>
        [.. RazorMarkup().SelectMany(markup => Scan(markup.File, markup.Text))];

    /// <summary>The elements of one file these two rules have a word about: those whose own tag writes a
    /// <c>tabindex</c> and those whose own <c>role</c> holds the <c>group</c> token. The whole of a component and a
    /// sample of markup the tests above read the rules through are read the same way.</summary>
    private static List<Element> Scan(string file, string text)
    {
        var elements = new List<Element>();

        foreach (var start in OpeningTags(text))
        {
            var tag = TagOf(text, start);
            var attributes = AttributesOf(tag);

            var tabindex = ValuesOf(attributes, TabindexAttribute);
            var group = IsGroup(attributes);

            if (tabindex.Count == 0 && !group)
            {
                continue;
            }

            elements.Add(new Element(file, LineOf(text, start), NameOf(tag), tag)
            {
                Tabindex = tabindex,
                IsGroup = group,
                HasName = group && NamesItself(attributes),
            });
        }

        return elements;
    }

    /// <summary>The offence of the sample's first element these rules are about - <c>null</c> when it offends in no
    /// way - refusing a sample that holds nothing to check, so a test cannot pass on markup the rules never read.</summary>
    private static string? OffenceOf(string text)
    {
        var scanned = Scan("Sample.razor", text);

        Assert.NotEmpty(scanned);

        return scanned[0].Offence;
    }

    /// <summary>The number a <c>tabindex</c> jumps the order with, or <c>null</c> when it holds no such number. A
    /// value Razor builds is not written until the page runs and a word that is no integer is no number: HTML sends
    /// both to the document's own order, and neither is this guard's guess. A <c>0</c> keeps that order and a
    /// <c>-1</c> leaves Tab alone.</summary>
    private static int? JumpOf(string? value)
    {
        if (value is not { Length: > 0 } || value.Contains('@', StringComparison.Ordinal))
        {
            return null;
        }

        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) && number > 0
            ? number
            : null;
    }

    /// <summary>True when the role an element writes holds the <c>group</c> token among its words. A value Razor
    /// builds writes no role this guard can read, <c>radiogroup</c> is another word, and <c>group</c> standing among
    /// other words - <c>role="toolbar group"</c> - is still the role: only a token of its own counts.</summary>
    private static bool IsGroup(List<(string Name, string? Value)> attributes) =>
        ValuesOf(attributes, RoleAttribute)
            .Any(value => value is not null && RoleTokens(value).Any(token => token.Equals(GroupRole, StringComparison.OrdinalIgnoreCase)));

    private static string[] RoleTokens(string value) =>
        value.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

    /// <summary>True when a non-empty <c>aria-label</c> or <c>aria-labelledby</c> stands on the tag itself. A name
    /// Razor builds - <c>@Label</c>, <c>@($"Other releases in {name}")</c> - names the group the same as one written
    /// in words; a value of nothing, or of spaces, names it with nothing.</summary>
    private static bool NamesItself(List<(string Name, string? Value)> attributes) =>
        NamingAttributes.Any(name => ValuesOf(attributes, name).Any(value => !string.IsNullOrWhiteSpace(value)));

    /// <summary>The values written for an attribute of the tag's own, one per occurrence - the words between the
    /// quotes, or up to the next space when HTML allows them bare, as the tag spells them; <c>null</c> when nothing
    /// readable follows the "=".</summary>
    private static List<string?> ValuesOf(List<(string Name, string? Value)> attributes, string name) =>
        [.. attributes.Where(attribute => attribute.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            .Select(attribute => attribute.Value)];

    /// <summary>The attributes an opening tag writes, as name and value, in the order the tag writes them. Each is
    /// read whole, quoted values and all - a value is closed by its own quote, not by the first "&gt;" it holds - so
    /// a word of a value is never taken for an attribute and only the tag's own whitespace opens the next one. The
    /// "<c>@</c>" of a Razor attribute (<c>@onclick</c>, <c>@bind:format</c>, <c>@ref</c>) is not a name character:
    /// the name after it is read as written, since none of them is an attribute of these rules.</summary>
    private static List<(string Name, string? Value)> AttributesOf(string tag)
    {
        var attributes = new List<(string Name, string? Value)>();
        var at = tag.IndexOf('<', StringComparison.Ordinal);

        if (at < 0)
        {
            return attributes;
        }

        at++;

        // The element name, which is no attribute.
        while (at < tag.Length && IsNameChar(tag[at]))
        {
            at++;
        }

        while (at < tag.Length)
        {
            while (at < tag.Length && (char.IsWhiteSpace(tag[at]) || tag[at] == '/'))
            {
                at++;
            }

            if (at >= tag.Length || tag[at] == '>')
            {
                break;
            }

            var start = at;

            while (at < tag.Length && IsNameChar(tag[at]))
            {
                at++;
            }

            if (at == start)
            {
                // Neither whitespace nor a name: a stray character of Razor inside the tag.
                at++;
                continue;
            }

            var name = tag[start..at];
            var probe = at;

            while (probe < tag.Length && char.IsWhiteSpace(tag[probe]))
            {
                probe++;
            }

            if (probe >= tag.Length || tag[probe] != '=')
            {
                attributes.Add((name, null));
                continue;
            }

            probe++;

            while (probe < tag.Length && char.IsWhiteSpace(tag[probe]))
            {
                probe++;
            }

            if (probe < tag.Length && (tag[probe] == '"' || tag[probe] == '\''))
            {
                var quote = tag[probe];

                // A double quoted value is run through the way the tag itself is read, because Razor quotes inside
                // it and a title may hold a "&gt;" of its own between them: a plain search for the next quote would
                // stop at the first of Razor's and then take the "&gt;" of the value for the end of the tag.
                var end = quote == '"' ? EndOfValue(tag, probe) : tag.IndexOf('\'', probe + 1);

                if (end < 0 || end >= tag.Length)
                {
                    // The value is never closed: what is read of the tag ends here rather than guessing on.
                    attributes.Add((name, null));
                    break;
                }

                attributes.Add((name, tag[(probe + 1)..end]));
                at = end + 1;
                continue;
            }

            var bare = probe;

            while (probe < tag.Length && !(char.IsWhiteSpace(tag[probe]) || tag[probe] is '>' or '/'))
            {
                probe++;
            }

            attributes.Add((name, tag[bare..probe]));
            at = probe;
        }

        return attributes;
    }

    private static bool IsNameChar(char c) => char.IsLetterOrDigit(c) || c is '-' or '_' or ':';

    /// <summary>Where the text opens a tag - "&lt;" followed by a name, the "&lt;/" of a closing tag left out - in
    /// the order the text writes them.</summary>
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

    /// <summary>The element name a tag opens, in lower case: HTML spells an element name the same whichever way its
    /// letters go, and a <c>&lt;SPAN&gt;</c> jumps the Tab order just the same.</summary>
    private static string NameOf(string tag)
    {
        var end = 1;

        while (end < tag.Length && IsNameChar(tag[end]))
        {
            end++;
        }

        return tag[1..end].ToLowerInvariant();
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
