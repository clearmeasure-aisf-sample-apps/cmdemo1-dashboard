using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Dashboard.Health;
using Microsoft.Extensions.Time.Testing;

namespace Dashboard.Tests;

/// <summary>An HTTP handler whose answers the test decides; it records the requests it saw.</summary>
internal sealed class StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
    : HttpMessageHandler
{
    private readonly List<HttpRequestMessage> _requests = [];

    public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
        : this((request, _) => Task.FromResult(respond(request)))
    {
    }

    public IReadOnlyList<HttpRequestMessage> Requests
    {
        get
        {
            lock (_requests)
            {
                return [.. _requests];
            }
        }
    }

    public IEnumerable<string> RequestedUrls => Requests.Select(request => request.RequestUri!.ToString());

    public static HttpResponseMessage Answer(HttpStatusCode status, string body = "") =>
        new(status) { Content = new StringContent(body) };

    /// <summary>An answer that never comes: the request ends when it is cancelled.</summary>
    public static async Task<HttpResponseMessage> NeverAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        throw new InvalidOperationException("An infinite delay ended without cancellation.");
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        lock (_requests)
        {
            _requests.Add(request);
        }

        return respond(request, cancellationToken);
    }
}

/// <summary>Fake time that tells the test when code under test started to wait on a timer.</summary>
internal sealed class SignallingTimeProvider : FakeTimeProvider
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);
    private readonly Channel<TimeSpan> _timers = Channel.CreateUnbounded<TimeSpan>();

    public SignallingTimeProvider()
        : base(new DateTimeOffset(2026, 10, 4, 22, 0, 0, TimeSpan.Zero))
    {
    }

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        var timer = base.CreateTimer(callback, state, dueTime, period);
        _timers.Writer.TryWrite(dueTime);
        return timer;
    }

    /// <summary>Waits until a timer was created that no earlier call of this method consumed.</summary>
    public async Task TimerCreatedAsync() =>
        Assert.True(await TimerCreatedWithinAsync(Patience), "No timer was created: the code under test is not waiting.");

    /// <summary>True when a timer is created within <paramref name="window"/> (real time).</summary>
    public async Task<bool> TimerCreatedWithinAsync(TimeSpan window)
    {
        using var patience = new CancellationTokenSource(window);
        try
        {
            await _timers.Reader.ReadAsync(patience.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}

/// <summary>An answer of one check of one endpoint: the state, the status, the latency, the moment, the version and the word for why - each one call away from its default.</summary>
public static class ProbeResults
{
    private static readonly DateTimeOffset DefaultNow = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>A healthy check that answered 200 in 40 ms, reporting the version named.</summary>
    public static ProbeResult Healthy(string? version = null, DateTimeOffset? at = null, int latencyMs = 40, int statusCode = 200) =>
        new(HealthState.Healthy, statusCode, latencyMs, at ?? DefaultNow, version);

    /// <summary>An unhealthy check: the status named, the time to the answer named, a word for why named - or the defaults of 503 in 40 ms.</summary>
    public static ProbeResult Unhealthy(int statusCode = 503, string? detail = null, DateTimeOffset? at = null, int latencyMs = 40) =>
        new(HealthState.Unhealthy, statusCode, latencyMs, at ?? DefaultNow, null, detail);

    /// <summary>An endpoint that did not answer: no status, no time to the answer, a word for why it was not there - or the default of ten seconds of silence.</summary>
    public static ProbeResult Unreachable(string detail = "No answer within 10 s.", DateTimeOffset? at = null) =>
        new(HealthState.Unreachable, null, null, at ?? DefaultNow, null, detail);

    /// <summary>A check that has not been made yet.</summary>
    public static ProbeResult Pending(DateTimeOffset? at = null) =>
        new(HealthState.Pending, null, null, at ?? DefaultNow);

    /// <summary>A check that answered with the status named, deriving the state from it.</summary>
    public static ProbeResult WithStatus(int statusCode, string? version = null, ProbeKind probe = ProbeKind.Health, DateTimeOffset? at = null) =>
        new(HealthClassifier.FromStatusCode(statusCode), statusCode, 40, at ?? DefaultNow, version, Probe: probe);

    /// <summary>A check of the state named, at the second offset from the default now; an unreachable endpoint has no status and no time to the answer, every other state has 200 in 12 ms.</summary>
    public static ProbeResult State(HealthState state, int second = 0, string? version = null) =>
        new(state, state == HealthState.Unreachable ? null : 200, state == HealthState.Unreachable ? null : 12, DefaultNow.AddSeconds(second), version);

    /// <summary>The check made at the fixed moment: the state named, the HTTP status and the time to the answer named or absent, and nothing invented where nobody named them.</summary>
    public static ProbeResult Check(HealthState state, int? statusCode = null, int? latencyMs = null, DateTimeOffset? at = null) =>
        new(state, statusCode, latencyMs, at ?? DefaultNow);

    /// <summary>A check of the state named, at the fixed moment: an unreachable endpoint has no status and no latency, a healthy one has 200, an unhealthy one has 503, and every other state has 40 ms to the answer.</summary>
    public static ProbeResult Of(HealthState state, string? version = null, DateTimeOffset? at = null) =>
        new(state, state == HealthState.Unreachable ? null : state == HealthState.Healthy ? 200 : 503, state == HealthState.Unreachable ? null : 40, at ?? DefaultNow, version);

    /// <summary>A check made by a detector: the state named, the status named or absent, the time to the answer named or absent - and no time to the answer where the caller named no status.</summary>
    public static ProbeResult Detected(HealthState state, int? status = null, string? detail = null, string? version = null, DateTimeOffset? at = null) =>
        new(state, status, status is null ? null : 40, at ?? DefaultNow, version, detail);
}

/// <summary>
/// The one way a test builds the regional nodes of a serving decision: a primary and a standby of the state, the
/// region and the name named - each one call away from its default, the region the default of the role it plays
/// and the name the app the region runs - and the read-out of a list of nodes: the labels they say. The serving
/// tests used to declare their own copies of these builders, and the copies drifted: a standby that stood in
/// japaneast in one file and eastus2 in another, a region one file named and another made required, and a node one
/// file named app-westus3 while another named it west. The defaults the mother keeps are the ones the majority
/// of the callers stood on, and a caller that stands elsewhere names the part it moves.
/// </summary>
public static class NodeHealths
{
    /// <summary>A primary node of the state, the region and the name named; the region defaults to westus3 and the name to app-{region}.</summary>
    public static NodeHealth Primary(HealthState state, string? region = "westus3", string? name = null) =>
        new(name ?? $"app-{region}", region, true, state);

    /// <summary>A standby node of the state, the region and the name named; the region defaults to eastus2 and the name to app-{region}.</summary>
    public static NodeHealth Standby(HealthState state, string? region = "eastus2", string? name = null) =>
        new(name ?? $"app-{region}", region, false, state);

    /// <summary>One primary per state, in the regions named, in the order the states stand.</summary>
    public static NodeHealth[] Primaries(IReadOnlyList<string> regions, params HealthState[] states) =>
        [.. states.Select((state, index) => Primary(state, regions[index]))];

    /// <summary>The labels the nodes say: the region when the topology gives one, the name when it does not.</summary>
    public static string[] Names(IEnumerable<NodeHealth> nodes) => [.. nodes.Select(node => node.Label)];
}

/// <summary>
/// The one way a test builds what the event detector sees of one endpoint at one check: the state, the version, the
/// telemetry reading (the uptime, the start, or neither) and the answer (the status and the word for why) - each
/// one call away from its default. A Pending state has no answer yet, which is the part the detector reads. The
/// event tests used to declare their own copies of this builder, and the copies drifted: one parsed the start
/// from text, the other had no start at all and kept a reading builder of its own beside it.
/// </summary>
public static class NodeObservations
{
    private static readonly DateTimeOffset DefaultNow = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>One endpoint at one check: the state and version named; telemetry only when the uptime or the
    /// start is named; and, unless the state is Pending, the answer with the status and the words named.</summary>
    public static NodeObservation Seen(HealthState state, string? version = "2.4.14", long? uptime = null, DateTimeOffset? startedAt = null, int? status = null, string? detail = null, DateTimeOffset? at = null)
    {
        var moment = at ?? DefaultNow;
        var telemetry = uptime is null && startedAt is null ? null : Reading(uptime, startedAt, moment);
        return new NodeObservation(state, version, telemetry, state == HealthState.Pending ? null : ProbeResults.Detected(state, status, detail, version, moment));
    }

    /// <summary>One telemetry reading: the uptime, the moment the process started, or neither.</summary>
    public static TelemetrySnapshot Reading(long? uptime = null, DateTimeOffset? startedAt = null, DateTimeOffset? at = null) =>
        TelemetrySnapshots.Quiet(at ?? DefaultNow) with
        {
            Process = uptime is null ? null : new ProcessVitals(null, null, null, null, null, null, uptime),
            StartedAt = startedAt,
        };
}

/// <summary>
/// The one way a test calls the node detector: the events of one endpoint between two observations,
/// given the moment and whether the endpoint is a node or its Front Door. The environment and deployable
/// default to the words the event tests have always used; a test that needs other words names them.
/// The two event test files used to declare their own copies of this call, and the copies were letter
/// for letter the same - so a third event test would write the copy again, and the day the audience
/// or the region changed the two files would say different things about the same endpoint.
/// </summary>
public static class EventCalls
{
    /// <summary>
    /// The events of one endpoint between <paramref name="before" /> and <paramref name="after" /> at
    /// <paramref name="at" />, for the environment and deployable named. A Front Door endpoint reports
    /// its state only: no restart, no version, no health check.
    /// </summary>
    public static IReadOnlyList<DashboardEvent> Node(
        NodeObservation before,
        NodeObservation after,
        bool isNode = true,
        DateTimeOffset at = default,
        string environment = "uat",
        string deployable = "ui") =>
        EventDetector.Node(before, after, environment, deployable, isNode ? "westus3" : "Front Door", isNode, at);
}

/// <summary>
/// The one way a test asks a question as a reader of another culture: the thread's culture is set for the
/// lifetime of the scope and put back as it was found when the scope ends, whatever the question did.
/// </summary>
internal sealed class CultureScope : IDisposable
{
    private readonly System.Globalization.CultureInfo _reader = Thread.CurrentThread.CurrentCulture;

    /// <summary>Makes <paramref name="culture" /> the thread's culture until the scope is disposed.</summary>
    public CultureScope(System.Globalization.CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);
        Thread.CurrentThread.CurrentCulture = culture;
    }

    /// <summary>Makes the culture named <paramref name="cultureName" /> the thread's culture until the scope is
    /// disposed; the empty name is the invariant culture.</summary>
    public CultureScope(string cultureName)
        : this(System.Globalization.CultureInfo.GetCultureInfo(cultureName))
    {
    }

    /// <summary>Puts back the culture the thread had before the scope.</summary>
    public void Dispose() => Thread.CurrentThread.CurrentCulture = _reader;

    /// <summary>What <paramref name="what" /> says to a reader of <paramref name="culture" />.</summary>
    public static T Says<T>(System.Globalization.CultureInfo culture, Func<T> what)
    {
        ArgumentNullException.ThrowIfNull(what);
        using var scope = new CultureScope(culture);
        return what();
    }

    /// <summary>What <paramref name="what" /> says to a reader of the culture named <paramref name="cultureName" />.</summary>
    public static T Says<T>(string cultureName, Func<T> what) =>
        Says(System.Globalization.CultureInfo.GetCultureInfo(cultureName), what);

    /// <summary>Runs <paramref name="what" /> as a reader of <paramref name="culture" />.</summary>
    public static void Runs(System.Globalization.CultureInfo culture, Action what)
    {
        ArgumentNullException.ThrowIfNull(what);
        using var scope = new CultureScope(culture);
        what();
    }

    /// <summary>Runs <paramref name="what" /> as a reader of the culture named <paramref name="cultureName" />.</summary>
    public static void Runs(string cultureName, Action what) =>
        Runs(System.Globalization.CultureInfo.GetCultureInfo(cultureName), what);

    /// <summary>What <paramref name="item" /> says to a reader of the invariant culture: the object's string
    /// form with no culture-specific formatting to confuse a test that is checking words rather than numerals.</summary>
    internal static string Words(object item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return CultureScope.Says(System.Globalization.CultureInfo.InvariantCulture, () => item.ToString()!);
    }
}

/// <summary>
/// The one way a test builds a telemetry reading: every counter is named at the call, so a value that lands on
/// the wrong property shows as the swap it is rather than matching a count that happens to be the same. The eleven
/// values of one reading are written as the reading they are and not as a list to be counted through.
/// </summary>
public static class TelemetrySnapshots
{
    /// <summary>A reading that says no numbers: every count zero, both percentiles and both halves of the SQL
    /// unsaid. The reading of an app that counted a quiet minute, beside the nothing a node that reported no
    /// telemetry at all lands on.</summary>
    public static TelemetrySnapshot Quiet(DateTimeOffset at) => new(0, 0, 0, 0, null, 0, 0, 0, null, 0, at);

    /// <summary>A reading with traffic but no other numbers: the requests that are not checks, all forwarded
    /// by Front Door, no errors, no percentiles. The shape of a reading that has only one thing to say.</summary>
    public static TelemetrySnapshot Traffic(int requests, DateTimeOffset at) => new(requests, requests, 0, 0, null, 0, 0, 0, null, 0, at);

    /// <summary>A reading with traffic and outgoing HTTP calls.</summary>
    public static TelemetrySnapshot WithHttp(int requests, int http, DateTimeOffset at) => new(requests, requests, 0, 0, null, 0, 0, 0, null, http, at);

    /// <summary>A reading with traffic and SQL commands.</summary>
    public static TelemetrySnapshot WithSql(int requests, int sql, DateTimeOffset at) => new(requests, requests, 0, 0, null, 0, 0, sql, null, 0, at);

    /// <summary>A reading with traffic, SQL commands and the app's own process vitals: the uptime it says it has
    /// been running, and nothing else of the vitals.</summary>
    public static TelemetrySnapshot WithUptime(long uptime, DateTimeOffset at) =>
        Quiet(at) with { Process = new ProcessVitals(null, null, null, null, null, null, uptime) };

    /// <summary>A reading with traffic and the moment the app's process started: an app that says when it started
    /// and nothing else about itself.</summary>
    public static TelemetrySnapshot WithStartedAt(DateTimeOffset startedAt, DateTimeOffset at) =>
        Quiet(at) with { StartedAt = startedAt };

    /// <summary>A reading with traffic and the moment the app's process started, and the uptime it says it has
    /// been running: both of the two ways an app can say when it started, and nothing else about itself.</summary>
    public static TelemetrySnapshot WithStartAndUptime(long uptime, DateTimeOffset startedAt, DateTimeOffset at) =>
        Quiet(at) with
        {
            Process = new ProcessVitals(null, null, null, null, null, null, uptime),
            StartedAt = startedAt,
        };

    /// <summary>A reading that tells the SQL of its requests from the SQL it runs in the background, beside the
    /// total of all its SQL commands.</summary>
    public static TelemetrySnapshot WithSplitSql(int requests, int totalSql, int sqlRequests, int sqlBackground, DateTimeOffset at) =>
        WithSql(requests, totalSql, at) with { SqlRequests = sqlRequests, SqlBackground = sqlBackground };
}

/// <summary>
/// The one way a test reads a rotation's serving events: the two assessments, the audience and the moment the
/// round was asked in, and the events the detector names between them, each said as the level and the text the
/// page says. A test says what changed - the node that left, the failover, the nothing that serves - and nothing
/// about how the events are spelled.
/// </summary>
public static class ServingEventReaders
{
    /// <summary>The serving events between <paramref name="before" /> and <paramref name="after" /> for the
    /// audience and the moment named, each said as its level and its text.</summary>
    public static string[] Events(ServingAssessment? before, ServingAssessment after, string environment, DateTimeOffset at) =>
        [.. EventDetector.ServingChanges(before, after, environment, "ui", at).Select(entry => $"{entry.Level}: {entry.Text}")];
}

/// <summary>
/// The two questions every markup guard asks and every markup guard used to answer with a copy of its own: how many
/// times a word is said in a file, and where the file a test reads stands. The walk of <see cref="CountOccurrences" />
/// was copied verbatim into ten guard files and the <c>[CallerFilePath]</c> navigation of
/// <see cref="ComponentPath" /> into sixty-odd of them - the same twelve lines, the same
/// <c>"..", "Dashboard", "Components"</c>, each with a name of its own and a drift waiting to happen. A guard says
/// what it counts and which component it reads, and nothing about where the source tree is.
/// <para>
/// The anchor is unchanged on purpose: every path here is found from the file that asks, exactly as the copies found
/// it, so a call site moves over one line at a time and reads the very text it always read. The equivalence of the
/// two ways of asking is itself pinned - see <see cref="MarkupGuardSupportTests" />, which walks the assembly, invokes
/// every copy still in it, and holds each to the answer given here.
/// </para>
/// </summary>
internal static class MarkupGuards
{
    /// <summary>The folder of the tests, which the file that asks stands in - the way the guards have always found
    /// the folder they read.</summary>
    public static string TestsRoot([CallerFilePath] string here = "") => Path.GetFullPath(Path.GetDirectoryName(here)!);

    /// <summary>The app's own source folder, <c>src/Dashboard</c>, as the test's folder stands beside it.</summary>
    public static string DashboardRoot([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(TestsRoot(here), "..", "Dashboard"));

    /// <summary>A file or a folder of the app's source, named as a path inside <c>src/Dashboard</c> - <c>Health</c>,
    /// <c>wwwroot/index.html</c>, <c>Runtime/RuntimePayloadBuilder.cs</c> - the one way the guards reach anything of
    /// the app that is not a component.</summary>
    public static string SourcePath(string relative, [CallerFilePath] string here = "")
    {
        ArgumentException.ThrowIfNullOrEmpty(relative);
        return Path.GetFullPath(Path.Combine(DashboardRoot(here), relative));
    }

    /// <summary>A component of the page, <c>src/Dashboard/Components/&lt;component&gt;.razor</c>, by the name the page
    /// writes for it - <c>WorkItemsView</c>, <c>StateBadge</c>, <c>DeployableFacts</c> - without a guard repeating
    /// where the components live.</summary>
    public static string ComponentPath(string component, [CallerFilePath] string here = "")
    {
        ArgumentException.ThrowIfNullOrEmpty(component);
        return SourcePath(Path.Combine("Components", component + ".razor"), here);
    }

    /// <summary>A component's markup, as the file holds it.</summary>
    public static string ComponentText(string component, [CallerFilePath] string here = "") =>
        File.ReadAllText(ComponentPath(component, here));

    /// <summary>The page itself, <c>src/Dashboard/App.razor</c>, which holds the header and every control of it.</summary>
    public static string AppPath([CallerFilePath] string here = "") => SourcePath("App.razor", here);

    /// <summary>The page's markup, as the file holds it.</summary>
    public static string AppText([CallerFilePath] string here = "") => File.ReadAllText(AppPath(here));

    /// <summary>How many times <paramref name="value" /> is said in <paramref name="text" />, walking past each
    /// occurrence by its own length so a word inside a word is not counted twice. Ordinal, so the reader's culture
    /// has nothing to say about it, and the count a guard pins is the count of letters and not of meanings.
    /// <para>
    /// The walk steps by <paramref name="value" />'s length, so an empty word would never move and the count would
    /// never end; a caller that asks for the count of nothing is refused where it stands rather than hung, and
    /// neither argument is ever nothing.</p>
    /// </summary>
    public static int CountOccurrences(string text, string value)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(value);
        if (value.Length == 0)
        {
            throw new ArgumentException("The word to count is empty, and the walk steps by its length.", nameof(value));
        }

        var count = 0;
        for (var index = 0; (index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0; index += value.Length)
        {
            count++;
        }

        return count;
    }

    /// <summary>
    /// The one-based line <paramref name="at" /> stands on in <paramref name="text" />: the number of line
    /// breaks before it, plus the first line, so the offender message that prints <c>file:line</c> points at the
    /// line the offending letter is written on and not at the line above it.
    /// </summary>
    public static int LineOf(string text, int at) => text.AsSpan(0, at).Count('\n') + 1;
}

/// <summary>
/// The one way a test builds and reads an <see cref="ActivityLine" />, and the moment and the zone its ages are
/// counted from: the moment the page is asked (Friday noon, 2026, UTC) and the zone it says the moments in, the
/// system the deployments file is read for, the line a test builds from its four words, the default line with all
/// four of its parts and the line without its address, and the questions a test asks of a line - the kind it says,
/// whether it carries the address of the task, whether its kind is one of the in flight, and what its title says.
/// The sayers the lines come from (<see cref="DeploymentActivity.OfNode" /> and
/// <see cref="DeploymentActivity.OfEnvironment" />) are reached with the file parsed the way the tests parse it and
/// the names the file was read for, so a test names the file's words and nothing of where they are read from.
/// </summary>
public static class ActivityLines
{
    /// <summary>Friday noon, UTC: the moment the page is asked, and every age is counted from it.</summary>
    public static readonly DateTimeOffset Now = new(2026, 10, 9, 12, 0, 0, TimeSpan.Zero);

    /// <summary>The zone the moments are said in, as the page says them.</summary>
    public static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    /// <summary>The system the deployments file is read for.</summary>
    public const string Slug = "demo";

    /// <summary>The deployable the ui's node is drawn for.</summary>
    public static readonly string[] Ui = ["ui"];

    /// <summary>A line of the four words named, the address left out being the absence of one rather than a placeholder for one.</summary>
    public static ActivityLine Line(string kind, string text, string title, Uri? url = null) =>
        new(kind, text, title, url);

    /// <summary>The default line: a deployment stopped for a person, on the task it waits for - the line with all four of its parts.</summary>
    public static ActivityLine Full() =>
        Line("waiting", "2.4.43 waits for demo approvers", "demo-ui 2.4.43 waits for a sign-off in prod", new Uri("https://octopus.example.invalid/app#/Spaces-5"));

    /// <summary>A line without its address: the freeze's line, or the line of a file that names no task.</summary>
    public static ActivityLine WithoutAddress() =>
        Line("queued", "2.4.44 queued, 3 min", "demo-ui 2.4.44 is queued in uat");

    /// <summary>The deployments file as a test writes it: the entries named in the three lists, and nothing the test did not name.</summary>
    public static DeploymentsReport Parse(string deployments = "", string recent = "", string freezes = "") =>
        DeploymentsReport.Parse($$"""{ "deployments": [ {{deployments}} ], "recent": [ {{recent}} ], "freezes": [ {{freezes}} ], "missing": [] }""")!;

    /// <summary>The lines a node of the report says for the environment and the deployable named, at the moment named or at <see cref="Now" /> in <see cref="Utc" />.</summary>
    public static IReadOnlyList<ActivityLine> Node(DeploymentsReport report, string environment, string deployable = "ui", DateTimeOffset? now = null) =>
        DeploymentActivity.OfNode(report, environment, Slug, deployable, now ?? Now, Utc);

    /// <summary>The lines under the environment's name, with the report's own marks of it, at the moment named or at <see cref="Now" /> in <see cref="Utc" />.</summary>
    public static IReadOnlyList<ActivityLine> Environment(DeploymentsReport report, string environment, DateTimeOffset? now = null) =>
        DeploymentActivity.OfEnvironment(report, environment, report.Marks(environment, Slug, Ui, now ?? Now), now ?? Now, Utc);

    /// <summary>The kind the line says.</summary>
    public static string KindOf(ActivityLine line) => line.Kind;

    /// <summary>True when the line carries the address of the task, and leads a reader to it.</summary>
    public static bool HasLink(ActivityLine line) => line.Url is not null;

    /// <summary>
    /// True for the kinds a deployment that is still in flight says - waiting, deploying, queued - and not for the
    /// freezes, and not for what ended: the kinds in the order <see cref="ActivityKind.All" /> says them, with the
    /// three that are in flight first.
    /// </summary>
    public static bool IsInFlight(ActivityLine line) =>
        line.Kind is ActivityKind.Waiting or ActivityKind.Deploying or ActivityKind.Queued;

    /// <summary>The sentence of the line's tooltip.</summary>
    public static string TitleOf(ActivityLine line) => line.Title;

    /// <summary>True when the line's title names the word, as the words compare themselves: in the case the word is written in, and ordinal.</summary>
    public static bool TitleMentions(ActivityLine line, string word) =>
        line.Title.Contains(word, StringComparison.Ordinal);
}

/// <summary>
/// The one way a health test names the node's states and tallies them into a summary: the four states
/// by the word the tests say them - <see cref="Healthy" />, <see cref="Unhealthy" />, <see cref="Pending" />
/// and <see cref="Unreachable" /> - the heap of answers a test builds from the answers given,
/// <see cref="States(HealthState[])" />, and the heap of <see cref="States(HealthState, int)" /> answers that
/// all say the one state, and the summary the counts make, <see cref="Summary(int, int, int)" />, beside
/// the summary the states make, <see cref="Summary(HealthState[])" />, counted through
/// <see cref="HealthSummary.Of" />. This is the one place a <see cref="HealthState" /> is named in the tests:
/// the health and version files that used to declare their own four aliases and, in
/// <see cref="HealthSummaryEdgeCaseTests" />, their own <c>Summary</c> and <c>States</c> builders, now name the
/// states, the summary and the heap builders through <see cref="HealthStates" /> - so a new <c>HealthState</c>,
/// or a moved default, is written here once and not in every file. The node-version fixtures that stand on the
/// states - the regional builders, the one-past sentinel and the pinned reading - are held by <see cref="Nodes" />
/// in <c>Fixtures/Nodes.cs</c>. The <c>Unknown</c> a serving test casts for an absent state is that file's own word
/// and is not kept here; the <c>Unnamed</c> of the version tests is the number one past the four states and is held
/// by <see cref="Nodes" />.
/// </summary>
public static class HealthStates
{
    /// <summary>The endpoint answered, and answered well.</summary>
    public const HealthState Healthy = HealthState.Healthy;

    /// <summary>The endpoint answered, and answered anything but well.</summary>
    public const HealthState Unhealthy = HealthState.Unhealthy;

    /// <summary>The check has not been made yet.</summary>
    public const HealthState Pending = HealthState.Pending;

    /// <summary>The endpoint did not answer at all.</summary>
    public const HealthState Unreachable = HealthState.Unreachable;

    /// <summary>A heap of answers built from the answers given, in the order given.</summary>
    public static HealthState[] States(params HealthState[] states) => states;

    /// <summary>A heap of <paramref name="count" /> answers that all say the one <paramref name="state" />.</summary>
    public static HealthState[] States(HealthState state, int count) =>
        [.. Enumerable.Repeat(state, count)];

    /// <summary>The summary built of the three counts the header weighs: the total, the healthy count, the pending count.</summary>
    public static HealthSummary Summary(int total, int healthy, int pending) =>
        new(total, healthy, pending);

    /// <summary>The summary the answers make, counted through <see cref="HealthSummary.Of" />: one total for each,
    /// the healthy and the pending of them counted as they say themselves.</summary>
    public static HealthSummary Summary(params HealthState[] states) =>
        HealthSummary.Of(states);
}

/// <summary>
/// The deployable-name boundary the address and <see cref="ApplicationText.IsName(string)" /> hold a name to: the
/// longest name taken, sixty-four letters and digits, and the name one character past it. Three test files spelled
/// these out on their own - a 64-character literal twice and a builder once (#1118) - so a change to the bound had to
/// be chased into each; they now share these two, and <see cref="ApplicationNamesObjectMotherTests" /> holds them to
/// the bound the class itself declares. They are constants so an <c>[InlineData]</c> may name them.
/// </summary>
public static class ApplicationNames
{
    /// <summary>The longest deployable name the address takes - 64 characters of letters and digits.</summary>
    public const string Longest = "n123456789012345678901234567890123456789012345678901234567890123";

    /// <summary>One character past the longest deployable name the address takes.</summary>
    public const string LongestPlusOne = Longest + "4";
}

/// <summary>
/// The one way a test builds a <see cref="Deployment" />: the four parts a file must give - the project,
/// the environment, the release, and the state with the word the file wrote for it - and nothing else, so a
/// deployment the test means to be bare is bare in every file alike. The seven test files that used to build
/// the record by hand each declared their own private builder under a name of their own, and the copies drifted:
/// a <c>StateText</c> one file said was <c>"paused"</c> and another said was <c>"as the file wrote it"</c> and a
/// third said was the state's own word, a <c>Since</c> one file left as nothing and another set to its own moment,
/// and the two <c>DateTimeOffset?</c> - <c>Since</c> and <c>Finished</c> - sitting side by side positionally so a
/// swap of them was silent. A caller that wants a part beyond the four names it, and the record's own
/// <c>with {}</c> handles the rest, so the positional order is written here once and not in every file.
/// </summary>
public static class Deployments
{
    /// <summary>
    /// A deployment of the four parts a file must give - the <paramref name="project" /> in the
    /// <paramref name="environment" />, the <paramref name="release" />, in the <paramref name="state" /> with the
    /// <paramref name="stateText" /> the file wrote - and nothing else: no start, no end, no address, no account
    /// that started it, no wait. The defaults are the ones the majority of the callers stood on.
    /// </summary>
    public static Deployment Bare(
        string project = "demo-ui",
        string environment = "uat",
        string release = "1",
        DeploymentState state = DeploymentState.Unknown,
        string stateText = "paused") =>
        new(project, environment, release, state, stateText, null, null, null);

    /// <summary>
    /// A deployment in the <paramref name="state" />, the <paramref name="finished" /> and the
    /// <paramref name="since" /> named, for the flight and marking questions: a deployment the test means to be in
    /// a state with the moments named, and nothing else. The <paramref name="stateText" /> defaults to the state's
    /// own word in lower case, the way the file says it. Named parameters keep <c>Since</c> and <c>Finished</c>
    /// apart: a swap of them is a compile error here, not a silent swap.
    /// </summary>
    public static Deployment InState(
        DeploymentState state,
        DateTimeOffset? finished = null,
        DateTimeOffset? since = null,
        string stateText = "",
        string project = "demo-ui",
        string environment = "uat",
        string release = "1",
        Uri? url = null,
        string? startedBy = null,
        DeploymentWait? waitsFor = null) =>
        new(project, environment, release, state, stateText.Length == 0 ? state.ToString().ToLowerInvariant() : stateText, since, finished, url, startedBy, waitsFor);

    /// <summary>
    /// A deployment in flight - <see cref="DeploymentState.Executing" /> - the <paramref name="since" /> named or
    /// three minutes before <paramref name="finished" />, and no end: the shape a mark of an in-flight deployment
    /// is built from.
    /// </summary>
    public static Deployment InFlight(
        string environment = "uat",
        string project = "demo-ui",
        string release = "1",
        DateTimeOffset? since = null,
        DateTimeOffset? finished = null) =>
        InState(DeploymentState.Executing, finished: null, since: since ?? finished?.AddMinutes(-3) ?? null, project: project, environment: environment, release: release);

    /// <summary>
    /// A deployment that ended - the <paramref name="state" /> named, the <paramref name="finished" /> named - the
    /// shape a mark of an ended deployment is built from.
    /// </summary>
    public static Deployment Ended(
        DeploymentState state,
        DateTimeOffset finished,
        string environment = "uat",
        string project = "demo-api",
        string release = "2") =>
        InState(state, finished: finished, since: finished.AddMinutes(-10), project: project, environment: environment, release: release);
}

/// <summary>
/// The one mother for the health tile: the tile itself (a node or a Front Door endpoint), the check of it
/// (a <see cref="ProbeResult" /> at the moment every health test stands at), and the reading of its telemetry
/// (a <see cref="TelemetrySnapshot" />). The six test files that each declared their own tile factory —
/// <c>TargetStatusTests.Node</c>, <c>TrendTests.Node</c>, <c>TrendsHelperTests.Node</c>,
/// <c>HealthHelperCoverageTests.Node</c>, <c>TargetTileClusterCardEnvironmentDeliveryTests.Node</c> and
/// <c>TargetStatusEdgeCaseTests.Node</c> — now share the shape the mother keeps, and the eight files that
/// each built their own <c>ProbeResult</c> or <c>TelemetrySnapshot</c> by hand can name the part they move
/// rather than re-declaring the whole record.
/// <para>
/// The moment is the one the health tests have always stood at — 2026-10-04T22:00Z, the same instant as
/// <see cref="ProbeResults" />'s and <see cref="NodeObservations" />'s default now — so a check built here
/// and a check built there are the check of the same instant, and the fact that an unreachable node holds no
/// status and no latency is written once, in <see cref="Check" />, and not in two files and left out in five.
/// </para>
/// </summary>
public static class Tiles
{
    /// <summary>The moment every health test stands at: 2026-10-04T22:00Z, UTC.</summary>
    public static readonly DateTimeOffset At = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// The tile of a node: its name, region and role, each named or the default, the address derived from the name
    /// the way every health test has always derived it — <c>https://{name}.example.net</c>.
    /// </summary>
    public static TargetStatus Node(string name = "app-west", string? region = "westus3", string? role = "primary") =>
        new(TargetKind.Node, name, new Uri($"https://{name}.example.net"), region, role);

    /// <summary>The tile of a deployable's Front Door endpoint, named or the default of <see cref="TargetStatus.FrontDoorName" />.</summary>
    public static TargetStatus FrontDoor(string name = TargetStatus.FrontDoorName) =>
        new(TargetKind.FrontDoor, name, new Uri($"https://{name.Replace(' ', '-')}.example.net"));

    /// <summary>
    /// The check of a tile at the <paramref name="second" /> offset from <see cref="At" />: an unreachable endpoint
    /// holds no status and no latency, every other state has 200 in 12 ms, and the version named or the default of
    /// nothing. The shape <c>TargetStatusTests.Result</c> and <c>TargetStatusEdgeCaseTests.Result</c> each wrote for
    /// themselves, written here once.
    /// </summary>
    public static ProbeResult Check(HealthState state, int second = 0, string? version = null) =>
        new(state, state == HealthState.Unreachable ? null : 200, state == HealthState.Unreachable ? null : 12, At.AddSeconds(second), version);

    /// <summary>
    /// The telemetry reading of a tile at <see cref="At" />: the requests and SQL named, and the CPU named as the
    /// process's vitals with an uptime of 600 — the shape <c>TrendTests.Sample</c> wrote for itself, written here
    /// once. A reading with no CPU says no process at all.
    /// </summary>
    public static TelemetrySnapshot Reading(int requests = 12, int sql = 0, double? cpu = null) =>
        new(requests, requests, 0, 0, null, 0, 0, sql, null, 0, At)
        {
            Process = cpu is null ? null : new ProcessVitals(cpu, null, null, null, null, null, 600),
        };
}
