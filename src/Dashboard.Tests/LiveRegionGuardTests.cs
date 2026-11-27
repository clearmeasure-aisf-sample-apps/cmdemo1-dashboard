using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// A live region is the one piece of markup a page may change under a reader's hands: the empty state that fills in
/// when the pods arrive, the banner that appears when a check fails, the log that gains a line every minute. What a
/// reader hears when it changes is written by one attribute, <c>aria-live</c>, whose three words are the whole of the
/// policy - <c>polite</c> waits for a pause in what the reader is saying, <c>assertive</c> cuts in at once,
/// <c>off</c> says nothing - and by a role that carries one implicitly: <c>role="alert"</c> already means
/// <c>assertive</c>, while <c>role="status"</c> and <c>role="log"</c> already mean <c>polite</c>.
/// <para>
/// A policy that is none of those words is no policy: the browser falls back to <c>off</c>, so the summary that
/// updates itself every thirty seconds never says a word to a reader who is not looking at it (#151). A
/// <c>role="alert"</c> tagged <c>aria-live="polite"</c> disagrees with itself about how soon the words are read, and
/// which half wins is the browser's business, not the component's. <c>aria-live="assertive"</c> on the empty-state
/// note of <c>role="status"</c> interrupts whatever the reader is saying for a sentence that changed while nobody was
/// looking at it - the fault a screen-reader user hears most, since this app fills a whole view with those notes.
/// And the two flags that finish a live region, <c>aria-atomic</c> (read the whole region again, or only what
/// changed) and <c>aria-busy</c> (hold the announcement, the content is arriving), are booleans: a bare attribute, an
/// empty value or <c>yes</c> reads as neither answer.
/// </para>
/// <para>
/// A guard walks every component and refuses all four faults, listing the file and the line the tag opens on, so the
/// next region added cannot be forgotten. A tag is read to the "&gt;" that closes it, past the Razor expressions, the
/// quoted values that carry a "&gt;" of their own and the line breaks they spread over, and its attributes are read
/// as pairs, so a word of a value is never taken for an attribute and <c>data-aria-live</c> is never taken for
/// <c>aria-live</c>. A value the tag does not write yet - <c>aria-live="@Policy"</c>,
/// <c>aria-busy="@(_loading ? "true" : null)"</c> - is nobody's guess and is left alone: the component's own bUnit
/// tests are where the words it builds are checked.
/// </para>
/// </summary>
public class LiveRegionGuardTests
{
    private const string LiveAttribute = "aria-live";
    private const string AtomicAttribute = "aria-atomic";
    private const string BusyAttribute = "aria-busy";
    private const string RoleAttribute = "role";

    private const string PolitePolicy = "polite";
    private const string AssertivePolicy = "assertive";
    private const string OffPolicy = "off";

    /// <summary>The whole of what <c>aria-live</c> may be told.</summary>
    private static readonly string[] LivePolicies = [PolitePolicy, AssertivePolicy, OffPolicy];

    /// <summary>The two words a boolean ARIA flag answers with.</summary>
    private static readonly string[] BooleanWords = ["true", "false"];

    /// <summary>The role that already announces at once, and the two roles that already wait their turn.</summary>
    private const string AlertRole = "alert";

    private const string StatusRole = "status";

    private const string LogRole = "log";

    /// <summary>The roles whose implicit policy is <c>polite</c>: a reader reaches them on purpose, and a change in
    /// one is never worth an interrupt.</summary>
    private static readonly string[] QuietRoles = [StatusRole, LogRole];

    /// <summary>
    /// One element these rules have a word about: it writes <c>aria-live</c>, <c>aria-atomic</c> or
    /// <c>aria-busy</c>, or its own <c>role</c> calls it a live region. The file to print, the line its tag opens on,
    /// its element name in lower case, the tag itself, and what it writes - one entry per occurrence of each
    /// attribute, as the words between the quotes (Razor and all), <c>null</c> when nothing readable follows the
    /// "=". A role built by Razor writes no token, so the guard cannot know what the tag calls itself.
    /// </summary>
    private sealed record Region(string File, int Line, string Name, string Tag)
    {
        public required List<string?> Live { get; init; }

        public required List<string?> Atomic { get; init; }

        public required List<string?> Busy { get; init; }

        /// <summary>The role tokens the tag writes in words, one entry per token of every literal role.</summary>
        public required List<string> Roles { get; init; }

        /// <summary>True when the tag calls itself an alert, which already means <c>assertive</c>.</summary>
        public bool IsAlert => Writes(AlertRole);

        /// <summary>The quiet role the tag writes - <c>status</c> or <c>log</c>, which already mean
        /// <c>polite</c> - or <c>null</c> when it writes neither.</summary>
        public string? QuietRole =>
            Roles.FirstOrDefault(token => QuietRoles.Any(quiet => quiet.Equals(token, StringComparison.OrdinalIgnoreCase)));

        /// <summary>True when the tag is a live region in the eye of these rules: it writes an <c>aria-live</c> of its
        /// own, in words or as Razor builds it, or a role that announces without one being written.</summary>
        public bool IsLiveRegion => Live.Count > 0 || IsAlert || QuietRole is not null;

        /// <summary>The policies the tag writes in words a rule can act on: every <c>aria-live</c> that is not built
        /// by Razor and holds one of the three policies, lower-cased and trimmed. A value the first rule already
        /// refuses is left out here, so one attribute is never reported twice for the same word.</summary>
        public List<string> ReadablePolicies =>
            [.. Live
                .Where(value => !IsRazor(value) && value is not null)
                .Select(value => value!.Trim().ToLowerInvariant())
                .Where(policy => LivePolicies.Any(word => word.Equals(policy, StringComparison.OrdinalIgnoreCase)))];

        /// <summary>Rule one: the <c>aria-live</c> of this tag that names no policy at all.</summary>
        public List<string> WrongPolicyFaults
        {
            get
            {
                var faults = new List<string>();

                foreach (var value in Live)
                {
                    if (IsRazor(value))
                    {
                        continue;
                    }

                    var policy = (value ?? string.Empty).Trim();

                    if (LivePolicies.Any(word => word.Equals(policy, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    faults.Add(value is null
                        ? $"<{Name}> writes {LiveAttribute} with no value at all, so the region announces by a policy nobody can read"
                        : $"<{Name}> writes {LiveAttribute}=\"{value}\", which is none of {Said(LivePolicies)}, so the region announces with no policy: a reader hears nothing when it changes");
                }

                return faults;
            }
        }

        /// <summary>Rule two: an alert that writes a policy of its own and names a quieter one than its role.</summary>
        public List<string> AlertContradictionFaults =>
            IsAlert
                ? [.. ReadablePolicies
                    .Where(policy => !policy.Equals(AssertivePolicy, StringComparison.OrdinalIgnoreCase))
                    .Select(policy => $"<{Name} {RoleAttribute}=\"{AlertRole}\"> writes {LiveAttribute}=\"{policy}\", which contradicts the role: an alert is read at once, so the tag disagrees with itself about how soon these words arrive")]
                : [];

        /// <summary>Rule three: a status or a log that cuts in at once.</summary>
        public List<string> ShoutingFaults =>
            QuietRole is { } quiet
                ? [.. ReadablePolicies
                    .Where(policy => policy.Equals(AssertivePolicy, StringComparison.OrdinalIgnoreCase))
                    .Select(policy => $"<{Name} {RoleAttribute}=\"{quiet}\"> writes {LiveAttribute}=\"{policy}\", so a note that only changes when the page settles interrupts whatever the reader is saying")]
                : [];

        /// <summary>Rule four: an <c>aria-atomic</c> or an <c>aria-busy</c> that is neither of the two words.</summary>
        public List<string> BooleanFlagFaults
        {
            get
            {
                var faults = new List<string>();

                faults.AddRange(FlagFaults(Name, AtomicAttribute, Atomic));
                faults.AddRange(FlagFaults(Name, BusyAttribute, Busy));

                return faults;
            }
        }

        /// <summary>Why the guard refuses this element, one sentence per fault - empty when it is guilty of neither.
        /// Every rule answers at once, so a fixer is not sent back for the next one by the following run.</summary>
        public List<string> Faults =>
            [.. WrongPolicyFaults, .. AlertContradictionFaults, .. ShoutingFaults, .. BooleanFlagFaults];

        /// <summary>The offence of the element, or <c>null</c> when there is none.</summary>
        public string? Offence => Faults.Count == 0 ? null : string.Join("; ", Faults);

        /// <summary>Where the fault stands: the file as the walk prints it, the line the tag opens on, and why.</summary>
        public string Report => $"{File}:{Line} {Offence}";

        private bool Writes(string token) =>
            Roles.Any(role => role.Equals(token, StringComparison.OrdinalIgnoreCase));

        private static IEnumerable<string> FlagFaults(string element, string attribute, List<string?> values)
        {
            foreach (var value in values)
            {
                if (IsRazor(value))
                {
                    continue;
                }

                var word = (value ?? string.Empty).Trim();

                if (!BooleanWords.Any(flag => flag.Equals(word, StringComparison.OrdinalIgnoreCase)))
                {
                    yield return value is null
                        ? $"<{element}> writes {attribute} with no value at all, which is neither true nor false"
                        : $"<{element}> writes {attribute}=\"{value}\", which is neither true nor false";
                }
            }
        }
    }

    /// <summary>
    /// No live region of any component writes a policy HTML does not know: every <c>aria-live</c> spelled in words
    /// reads as <c>polite</c>, <c>assertive</c> or <c>off</c>, so the region announces by the policy its author chose
    /// rather than by the browser's silent fallback to <c>off</c>.
    /// </summary>
    [Fact]
    public void EveryLiveRegionNamesOneOfTheThreePolicies()
    {
        var offenders = Regions()
            .Where(r => r.WrongPolicyFaults.Count > 0)
            .Select(r => r.Report)
            .ToList();

        Assert.Empty(offenders);
    }

    /// <summary>
    /// Every <c>role="alert"</c> that writes a policy of its own agrees with its role: an alert is read at once, so
    /// the only <c>aria-live</c> that suits it is <c>assertive</c>.
    /// </summary>
    [Fact]
    public void EveryAlertAgreesWithThePolicyItWrites()
    {
        var offenders = Regions()
            .Where(r => r.AlertContradictionFaults.Count > 0)
            .Select(r => r.Report)
            .ToList();

        Assert.Empty(offenders);
    }

    /// <summary>
    /// No <c>role="status"</c> and no <c>role="log"</c> shouts: the empty states, the summaries and the event log of
    /// this app change while the reader is elsewhere, and an interrupt for each of them would make the page unreadable.
    /// </summary>
    [Fact]
    public void NoStatusOrLogRegionInterrupts()
    {
        var offenders = Regions()
            .Where(r => r.ShoutingFaults.Count > 0)
            .Select(r => r.Report)
            .ToList();

        Assert.Empty(offenders);
    }

    /// <summary>
    /// Every <c>aria-atomic</c> and every <c>aria-busy</c> of every component answers with one of the two words it is
    /// asked with, <c>true</c> or <c>false</c>: a bare attribute, an empty value or <c>yes</c> leaves the reader's
    /// software to guess whether the content is still arriving.
    /// </summary>
    [Fact]
    public void EveryAtomicAndBusyFlagIsABoolean()
    {
        var offenders = Regions()
            .Where(r => r.BooleanFlagFaults.Count > 0)
            .Select(r => r.Report)
            .ToList();

        Assert.Empty(offenders);
    }

    /// <summary>
    /// A guard that found nothing has seen nothing. The app is full of live regions - thirty-odd empty states, notes
    /// and summaries of <c>role="status"</c>, the problem banners of <c>role="alert"</c>, the event log of
    /// <c>EventsStrip.razor</c> and the one summary that writes its policy out in full - and the rules above are worth
    /// exactly as much as the walk that feeds them. The floor is set well below what the markup holds today, the file
    /// floor below the thirteen files it reaches, and the named elements below are the ones whose behaviour a reader
    /// of this page would notice first; the day the guard reads far less than this, it is reading the wrong directory
    /// and its four clean verdicts mean nothing.
    /// </summary>
    [Fact]
    public void TheGuardReadsTheLiveRegionsOfTheApp()
    {
        var regions = LiveRegions();
        var files = regions.Select(r => r.File).ToHashSet(StringComparer.Ordinal);

        Assert.True(
            regions.Count >= 20,
            $"only {regions.Count} live region(s) were read under {DashboardRoot()}: the guard is checking nothing");

        Assert.True(
            files.Count >= 5,
            $"the {regions.Count} live region(s) read under {DashboardRoot()} live in only {files.Count} file(s): the walk does not reach the app");

        // The policy written in words, the quiet role and the shouting one it must not carry, and the alert.
        Assert.Contains(regions, r => r.File.Equals("App.razor", StringComparison.Ordinal)
            && r.QuietRole == StatusRole && r.ReadablePolicies.Contains(PolitePolicy));
        Assert.Contains(regions, r => r.IsAlert
            && r.File.Equals(Path.Combine("Components", "RuntimeView.razor"), StringComparison.Ordinal));
        Assert.Contains(regions, r => r.IsAlert
            && r.File.Equals(Path.Combine("Components", "DatabaseView.razor"), StringComparison.Ordinal));
        Assert.Contains(regions, r => r.QuietRole == LogRole
            && r.File.Equals(Path.Combine("Components", "EventsStrip.razor"), StringComparison.Ordinal));

        // The flag the app builds at runtime is read as a flag, and skipped as a value: the reload button's busy.
        Assert.Contains(Regions(), r => r.File.Equals("App.razor", StringComparison.Ordinal)
            && r.Busy.Any(IsRazor) && r.BooleanFlagFaults.Count == 0);

        // And what the guard reads of the app today is clean, rule by rule and all of them at once.
        Assert.All(Regions(), r => Assert.Null(r.Offence));
    }

    /// <summary>The rule refuses a policy that is no policy, however the tag is spelled: a word of its own, the word
    /// <c>false</c> (a live region is not a switch), an empty value and a bare attribute, and the policy of a tag whose
    /// attributes spread over lines of their own.</summary>
    [Fact]
    public void TheRuleRefusesAPolicyThatIsNoPolicy()
    {
        Assert.Contains("aria-live=\"loud\"", OffenceOf("""<div class="summary" role="status" aria-live="loud">…</div>"""), StringComparison.Ordinal);
        Assert.Contains("aria-live=\"false\"", OffenceOf("""<span class="polling" aria-live="false">Polling</span>"""), StringComparison.Ordinal);
        Assert.Contains("aria-live=\"rude\"", OffenceOf("""<section class="problem" role="alert" aria-live="rude">Down</section>"""), StringComparison.Ordinal);
        Assert.Contains("aria-live=\"\"", OffenceOf("""<p class="empty" role="status" aria-live="">Nothing yet</p>"""), StringComparison.Ordinal);
        Assert.Contains("no value at all", OffenceOf("""<p class="empty" role="status" aria-live>Nothing yet</p>"""), StringComparison.Ordinal);
        Assert.Contains("aria-live=\"polite ly\"", OffenceOf("""
            <p class="summary"
               role="status"
               aria-live="polite ly">
                All 7 nodes healthy
            </p>
            """), StringComparison.Ordinal);
    }

    /// <summary>The rule refuses the alert that writes a quieter policy than its own role: <c>polite</c> waits for a
    /// pause in the reader's speech, which is exactly what a "the database is down" banner may not do, and
    /// <c>off</c> hides it altogether. A role that holds <c>alert</c> among its words owes the same answer.</summary>
    [Fact]
    public void TheRuleRefusesAnAlertWhosePolicyQuietsIt()
    {
        Assert.Contains("role=\"alert\"", OffenceOf("""<section class="problem" role="alert" aria-live="polite">Front Door is unhealthy</section>"""), StringComparison.Ordinal);
        Assert.Contains("contradicts", OffenceOf("""<p class="problem" role="alert" aria-live="off">@problem</p>"""), StringComparison.Ordinal);
        Assert.Contains("aria-live=\"polite\"", OffenceOf("""<div role="status alert" aria-live="polite">both words at once</div>"""), StringComparison.Ordinal);
        Assert.Contains("aria-live=\"off\"", OffenceOf("""<div role="ALERT" aria-live="OFF">shouted and muted</div>"""), StringComparison.Ordinal);
    }

    /// <summary>The rule refuses the quiet role that cuts in at once - the empty state, the version note, the event
    /// log - in either spelling and with either padding, since HTML reads the keyword the same whichever way its
    /// letters go.</summary>
    [Fact]
    public void TheRuleRefusesAQuietRoleThatShouts()
    {
        Assert.Contains("interrupts", OffenceOf("""<p class="empty" role="status" aria-live="assertive">No pods.</p>"""), StringComparison.Ordinal);
        Assert.Contains("role=\"log\"", OffenceOf("""<div class="events__log" role="log" aria-live="assertive">…</div>"""), StringComparison.Ordinal);
        Assert.Contains("aria-live=\"assertive\"", OffenceOf("""<span class="traffic__status" role="status" aria-live="ASSERTIVE">@StatusText</span>"""), StringComparison.Ordinal);
        Assert.Contains("role=\"status\"", OffenceOf("""<p role="status" aria-live=" assertive ">Loading</p>"""), StringComparison.Ordinal);
    }

    /// <summary>The rule refuses a boolean flag that answers with anything but the two words: a word of its own,
    /// <c>1</c>, an empty value, a bare attribute, and both flags of one tag refusing at once.</summary>
    [Fact]
    public void TheRuleRefusesAFlagThatIsNeitherWord()
    {
        Assert.Contains("aria-atomic=\"yes\"", OffenceOf("""<p class="empty" role="status" aria-atomic="yes">Nothing yet</p>"""), StringComparison.Ordinal);
        Assert.Contains("aria-busy=\"1\"", OffenceOf("""<div role="log" aria-busy="1">…</div>"""), StringComparison.Ordinal);
        Assert.Contains("aria-busy=\"\"", OffenceOf("""<div role="status" aria-busy="">…</div>"""), StringComparison.Ordinal);
        Assert.Contains("aria-atomic with no value at all", OffenceOf("""<div class="canvas" aria-atomic>…</div>"""), StringComparison.Ordinal);

        var both = OffenceOf("""<div class="summary" role="status" aria-atomic="maybe" aria-busy="yes">…</div>""")!;

        Assert.Contains("aria-atomic=\"maybe\"", both, StringComparison.Ordinal);
        Assert.Contains("aria-busy=\"yes\"", both, StringComparison.Ordinal);
    }

    /// <summary>The rule accepts what a live region may say: the three policies, in either case and padded with
    /// spaces, the quiet roles that write no policy at all because their role is one, the alert that agrees with
    /// itself, the two flags as the word, an attribute that merely carries the name of one - <c>data-aria-live</c>,
    /// <c>aria-live-region</c> - and a role that is none of the three live roles, whose policy is its own business.</summary>
    [Fact]
    public void TheRuleAcceptsARegionThatSaysOneOfTheThreeThings()
    {
        Assert.Null(OffenceOf("""<p class="empty" role="status">Loading the tables</p>"""));
        Assert.Null(OffenceOf("""<div class="events__log" role="log" aria-labelledby="events-title">…</div>"""));
        Assert.Null(OffenceOf("""<p class="summary" role="status" aria-live="polite">All 7 nodes healthy</p>"""));
        Assert.Null(OffenceOf("""<section class="problem" role="alert" aria-live="assertive">Down</section>"""));
        Assert.Null(OffenceOf("""<span class="polling" role="status" aria-live="off">Paused</span>"""));
        Assert.Null(OffenceOf("""<span class="polling" role="status" aria-live=" Polite ">Paused</span>"""));
        Assert.Null(OffenceOf("""<div role="region" aria-live="polite" aria-atomic="TRUE" aria-busy="False">…</div>"""));
        Assert.Null(OffenceOf("""<div role="status" data-aria-live="rude" aria-live-region="assertive">…</div>"""));
        Assert.Null(OffenceOf("""<div class="facts" role="group" aria-live="off" aria-atomic="false">…</div>"""));
    }

    /// <summary>A value Razor builds is read as a value and left alone: the policy, the flag and the role the tag
    /// assembles at runtime are checked by the component's own tests, where the fields that fill them are known. An
    /// <c>aria-live</c> written as an expression still makes the tag a live region, so the guard keeps reading it.</summary>
    [Fact]
    public void TheRuleSkipsAValueRazorBuilds()
    {
        var scanned = Scan("Sample.razor", """
            <p class="summary" role="status" aria-live="@Policy">
                All 7 nodes healthy
            </p>
            <button type="button" aria-busy="@(_loading ? "true" : null)">Reload</button>
            <div class="card" role="@Kind" aria-live="@(shouting ? "assertive" : "polite")" aria-atomic="@Atomic">…</div>
            """);

        Assert.Equal(3, scanned.Count);
        Assert.All(scanned, region => Assert.Null(region.Offence));
        Assert.Contains(scanned[1].Busy, value => IsRazor(value));
        Assert.False(scanned[2].IsAlert);
        Assert.True(scanned[2].IsLiveRegion, "a policy still being built is an aria-live of its own all the same");
    }

    /// <summary>Only the tag's own attributes say anything: the policy of a child is that child's business and never
    /// reaches the parent that holds it - a <c>role="status"</c> with a shouting child stays quiet in the eye of the
    /// rule - and a flag of a child is refused on the child's own line.</summary>
    [Fact]
    public void OnlyTheRegionsOwnAttributesCount()
    {
        var scanned = Scan("Sample.razor", """
            <div role="status" aria-live="polite">
                <span aria-live="assertive">the child shouts for itself</span>
                <span aria-atomic="yes">the child's flag is the child's</span>
            </div>
            """);

        Assert.Equal(3, scanned.Count);

        Assert.Null(scanned[0].Offence);
        Assert.Equal(StatusRole, scanned[0].QuietRole);
        Assert.Equal(PolitePolicy, Assert.Single(scanned[0].ReadablePolicies));

        // The child's own assertive names no role, so it offends no rule; the grandchild's flag does.
        Assert.Null(scanned[1].Offence);
        Assert.False(scanned[1].IsAlert);
        Assert.Null(scanned[1].QuietRole);
        Assert.Contains("aria-atomic=\"yes\"", scanned[2].Offence!, StringComparison.Ordinal);
    }

    /// <summary>Every rule answers at once, so a fixer is not sent back for the next fault by the following run: a
    /// policy that is none of the three, a role that contradicts the one it writes and two flags that answer in the
    /// wrong words are all reported on the line of the tag that carries them.</summary>
    [Fact]
    public void EveryFaultOfATagIsReported()
    {
        var scanned = Scan("Sample.razor", """<div role="log" aria-live="assertive" aria-busy="yes">…</div>""");

        Assert.Single(scanned);
        Assert.Equal(2, scanned[0].Faults.Count);
        Assert.Contains("role=\"log\"", scanned[0].Offence!, StringComparison.Ordinal);
        Assert.Contains("aria-busy=\"yes\"", scanned[0].Offence!, StringComparison.Ordinal);

        scanned = Scan("Sample.razor", """<p role="alert status" aria-live="loud" aria-atomic="1" aria-busy="2">…</p>""");

        Assert.Single(scanned);
        Assert.Equal(3, scanned[0].Faults.Count);
        Assert.Contains("aria-live=\"loud\"", scanned[0].Offence!, StringComparison.Ordinal);
        Assert.Contains("aria-atomic=\"1\"", scanned[0].Offence!, StringComparison.Ordinal);
        Assert.Contains("aria-busy=\"2\"", scanned[0].Offence!, StringComparison.Ordinal);
    }

    /// <summary>A tag ends at the "&gt;" that closes it, not at the first "&gt;" of the file: a class may hold a
    /// comparison, a title may carry a "&gt;" of its own between the quotes Razor writes inside it, and the attributes
    /// - among them <c>role</c> and <c>aria-live</c> - may spread over lines. One element is read per opening tag,
    /// never the <c>&lt;/p&gt;</c> that closes it.</summary>
    [Fact]
    public void TheTagReadRunsPastRazorAndLineBreaks()
    {
        var text = """
            <main>
                <p class="summary @(quiet ? "summary--quiet" : null)"
                   title="@($"3 requests > the last minute")"
                   role="status"
                   aria-live="polite">All 7 nodes healthy</p>
                <div class="legend"
                     title="more > than a policy"
                     role="alert"
                     aria-live="polite">Down</div>
                <span aria-busy="@(_loading ? "true" : null)">Reload</span>
            </main>
            """;

        var scanned = Scan("Sample.razor", text);

        Assert.Equal(3, scanned.Count);

        Assert.Equal(2, scanned[0].Line);
        Assert.DoesNotContain("</p", scanned[0].Tag, StringComparison.Ordinal);
        Assert.Null(scanned[0].Offence);

        Assert.Equal(6, scanned[1].Line);
        Assert.True(scanned[1].IsAlert);
        Assert.Contains("role=\"alert\"", scanned[1].Offence!, StringComparison.Ordinal);

        Assert.Equal(10, scanned[2].Line);
        Assert.Null(scanned[2].Offence);
    }

    /// <summary>The report names the file the way the walk prints it and the line the tag opens on - the first of
    /// them, not the line its <c>role</c> or its <c>aria-live</c> sits on - and reads every region of a file on its
    /// own.</summary>
    [Fact]
    public void TheReportNamesTheFileAndTheLineOfTheTag()
    {
        var text = """
            <div class="views">
                <p role="status"
                   aria-live="loud">@LoadingText</p>
                <div role="log"
                     aria-live="assertive">…</div>
            </div>
            """;

        var scanned = Scan(Path.Combine("Components", "Sample.razor"), text);

        Assert.Equal(2, scanned.Count);

        Assert.Equal(2, scanned[0].Line);
        Assert.StartsWith("Components", scanned[0].Report, StringComparison.Ordinal);
        Assert.Contains("aria-live=\"loud\"", scanned[0].Report, StringComparison.Ordinal);

        Assert.Equal(4, scanned[1].Line);
        Assert.Contains("role=\"log\"", scanned[1].Report, StringComparison.Ordinal);
    }

    /// <summary>HTML spells a tag, an attribute and a role the same whichever way its letters go, so every rule reads
    /// them that way: an uppercase <c>ROLE="STATUS"</c> with an <c>ARIA-LIVE="ASSERTIVE"</c> shouts just the same,
    /// and a value quoted with single quotes - which HTML allows - is the same value.</summary>
    [Fact]
    public void TheRulesReadTheSpellingOfTheSameWords()
    {
        Assert.Contains("interrupts", OffenceOf("""<DIV ROLE="STATUS" ARIA-LIVE="ASSERTIVE">…</DIV>"""), StringComparison.Ordinal);
        Assert.Contains("interrupts", OffenceOf("""<p role = 'log' aria-live = 'assertive'>…</p>"""), StringComparison.Ordinal);
        Assert.Contains("aria-busy=\"maybe\"", OffenceOf("""<p role='status' aria-busy = 'maybe'>…</p>"""), StringComparison.Ordinal);

        var scanned = Scan("Sample.razor", """<SPAN ROLE = 'Status' aria-live = 'off'>…</SPAN>""");

        Assert.Equal("span", scanned[0].Name);
        Assert.Equal(StatusRole, scanned[0].QuietRole?.ToLowerInvariant());
        Assert.Null(scanned[0].Offence);
    }

    /// <summary>Every element of every component that writes a live region: the file to print, the line its tag opens
    /// on and what the four rules ask of it.</summary>
    private static List<Region> Regions() =>
        [.. RazorMarkup().SelectMany(markup => Scan(markup.File, markup.Text))];

    /// <summary>The elements these rules call live regions, in the words of their own role or of an
    /// <c>aria-live</c> they write.</summary>
    private static List<Region> LiveRegions() => [.. Regions().Where(region => region.IsLiveRegion)];

    /// <summary>The regions of one file: every opening tag that writes <c>aria-live</c>, <c>aria-atomic</c> or
    /// <c>aria-busy</c>, or whose own role calls it a live region. A whole component and a sample of markup the tests
    /// above read the rules through are scanned the same way.</summary>
    private static List<Region> Scan(string file, string text)
    {
        var regions = new List<Region>();

        foreach (var start in MarkupScan.OpeningTags(text))
        {
            var tag = TagOf(text, start);
            var attributes = AttributesOf(tag);

            var live = ValuesOf(attributes, LiveAttribute);
            var atomic = ValuesOf(attributes, AtomicAttribute);
            var busy = ValuesOf(attributes, BusyAttribute);
            var roles = RoleTokensOf(attributes);

            var region = new Region(file, LineOf(text, start), NameOf(tag), tag)
            {
                Live = live,
                Atomic = atomic,
                Busy = busy,
                Roles = roles,
            };

            // A tag that writes its policy, either flag, or a live role is read; a tag whose role is another word and
            // which writes none of the three attributes is no business of these rules.
            if (region.IsLiveRegion || atomic.Count > 0 || busy.Count > 0)
            {
                regions.Add(region);
            }
        }

        return regions;
    }

    /// <summary>The offence of the sample's first region - <c>null</c> when it offends in no way - refusing a sample
    /// that holds nothing to check, so a test cannot pass on markup the rules never read.</summary>
    private static string? OffenceOf(string text)
    {
        var scanned = Scan("Sample.razor", text);

        Assert.NotEmpty(scanned);

        return scanned[0].Offence;
    }

    /// <summary>The role tokens the tag writes in words, one per word of every literal <c>role</c> it writes. A role
    /// Razor builds writes no token: the guard does not guess what the tag will call itself.</summary>
    private static List<string> RoleTokensOf(List<(string Name, string? Value)> attributes) =>
        [.. ValuesOf(attributes, RoleAttribute)
            .Where(value => !IsRazor(value) && value is not null)
            .SelectMany(value => value!.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries))];

    /// <summary>The values written for an attribute of the tag's own, one per occurrence - the words between the
    /// quotes, or up to the next space when HTML allows them bare, as the tag spells them; <c>null</c> when nothing
    /// readable follows the "=".</summary>
    private static List<string?> ValuesOf(List<(string Name, string? Value)> attributes, string name) =>
        [.. attributes.Where(attribute => attribute.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            .Select(attribute => attribute.Value)];

    private static bool IsRazor(string? value) =>
        value is not null && value.StartsWith('@');

    /// <summary>How a list of allowed words is named in a report.</summary>
    private static string Said(string[] words) =>
        string.Join(", ", words[..^1].Select(word => $"\"{word}\"")) + $" or \"{words[^1]}\"";

    /// <summary>The attributes an opening tag writes, as name and value, in the order the tag writes them - read the
    /// way the other markup guards read them, so a word of a value is never taken for an attribute and
    /// <c>data-aria-live</c> is never taken for <c>aria-live</c>.</summary>
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

    /// <summary>The element name a tag opens, in lower case: HTML spells an element name the same whichever way its
    /// letters go, and a <c>&lt;SPAN&gt;</c> shouts just the same.</summary>
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
