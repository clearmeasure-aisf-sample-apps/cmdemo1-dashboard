using System.Net;
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
}
