using System.Globalization;
using System.Net;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ClusterMonitor" />: the cluster view's part of the
/// page's monitor, the last reading of the cluster's two files, the readings the trends are drawn from and the
/// events between two readings. <see cref="ClusterMonitorTests" /> pins the page's round of checks - both files read
/// every round without the browser's cache, the liveness the page observes, the trends at the sample's numbers, the
/// changed a check raises - and what it leaves open is pinned here: the monitor at its making, the info it is asked
/// for and only for; the reading the topology does not name, absent from the start and not pending; the check with
/// no address named, which reads nothing and says nothing; the check the caller cancels, which throws and stands
/// where it stood; the reading a check makes, good or bad, over the one it replaces; the answer that is not the
/// file, with its reasons; the changed the one file that is named raises with every reading, good or bad; the check
/// that names no environment, which says no environment in its events; the status that stops being live, which is
/// a gap in the samples and no line; the node that says no cpu usage, which is no point of the cpu trend, and the
/// usage that is zero, which is a reading; and the samples that keep only the length of the trend.
/// <para>
/// Nothing here reaches a network or asks a clock what time it is: every answer is one the test hands back, and
/// every moment is one the test brings.
/// </para>
/// </summary>
public class ClusterMonitorEdgeCaseTests
{
    private const string StatusUrl = "https://cluster.example.net/cluster.json";
    private const string ServiceUrl = "https://raw.example.net/org/repo/cluster-status/aks.json";

    private readonly SignallingTimeProvider _time = new();
    private readonly EventLog _events = new();

    private ClusterMonitor Monitor(ClusterInfo info, StubHandler handler, IReadOnlyList<EnvironmentInfo>? environments = null) =>
        new(info, environments ?? [], new ClusterReader(new HttpClient(handler), _time), _time, _events);

    /// <summary>The sample status as the collector wrote it at the test's clock: live, not stale.</summary>
    private string Status() =>
        ClusterFixture.Sample("cluster.json")
            .Replace("2026-10-06T20:15:30Z", TimeText.Iso(_time.GetUtcNow()), StringComparison.Ordinal);

    /// <summary>A status of one node, its cpu in millicores and its memory in bytes; null where the node does not say.</summary>
    private static string OneNode(double? cpu, double? memory) =>
        """
        {
          "generated": "2026-10-06T20:15:30Z",
          "intervalSeconds": 15,
          "nodes": [ { "name": "aks-system-12148983-vmss000000", "ready": true,
                       "cpu": { "usage": @cpu@ },
                       "memory": { "usage": @memory@ } } ],
          "namespaces": []
        }
        """
        .Replace("@cpu@", Number(cpu), StringComparison.Ordinal)
        .Replace("@memory@", Number(memory), StringComparison.Ordinal);

    private static string Number(double? value) => value is { } number ? number.ToString(CultureInfo.InvariantCulture) : "null";

    private static HttpResponseMessage Answer(HttpRequestMessage request, string? status, string? service) =>
        request.RequestUri!.AbsoluteUri switch
        {
            StatusUrl => status is null ? StubHandler.Answer(HttpStatusCode.NotFound) : StubHandler.Answer(HttpStatusCode.OK, status),
            ServiceUrl => service is null ? StubHandler.Answer(HttpStatusCode.NotFound) : StubHandler.Answer(HttpStatusCode.OK, service),
            _ => StubHandler.Answer(HttpStatusCode.OK),
        };

    // ----- the monitor at its making -----

    /// <summary>The monitor is asked for its info, and only for it: without one it is said so, naming the
    /// argument - and made with none of the rest, the environments, the reader, the clock and the log, it keeps
    /// the info it was given, stands the readings the topology names pending, and stands the samples empty.</summary>
    [Fact]
    public void TheMonitorIsAskedForItsInfoAndOnlyForIt()
    {
        var error = Assert.Throws<ArgumentNullException>(() => new ClusterMonitor(null!, [], null!, null!, null!));

        Assert.Equal("info", error.ParamName);

        var info = new ClusterInfo("aks-demo", new Uri(StatusUrl), new Uri(ServiceUrl));
        var monitor = new ClusterMonitor(info, null!, null!, null!, null!);

        Assert.Same(info, monitor.Info);
        Assert.Equal(new SourceReading<ClusterStatus>(SourceState.Pending), monitor.Status);
        Assert.Equal(new SourceReading<AksService>(SourceState.Pending), monitor.Service);
        Assert.Empty(monitor.Samples);
    }

    /// <summary>The reading the topology does not name stands absent from the start, not pending: the monitor
    /// without a serviceUrl has no service reading and the one without a statusUrl has no status reading, while
    /// the other stands pending. The samples stand empty with their room for the trend, and the trends of no
    /// reading stand absent.</summary>
    [Fact]
    public void TheReadingTheTopologyDoesNotNameStandsAbsentFromTheStart()
    {
        var withStatus = Monitor(new ClusterInfo("aks-demo", new Uri(StatusUrl), null), new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound)));
        var withService = Monitor(new ClusterInfo("aks-demo", null, new Uri(ServiceUrl)), new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound)));
        var withNeither = Monitor(new ClusterInfo("aks-demo"), new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound)));

        Assert.Null(withStatus.Service);
        Assert.Equal(new SourceReading<ClusterStatus>(SourceState.Pending), withStatus.Status);

        Assert.Null(withService.Status);
        Assert.Equal(new SourceReading<AksService>(SourceState.Pending), withService.Service);

        Assert.Null(withNeither.Status);
        Assert.Null(withNeither.Service);
        Assert.Empty(withStatus.Samples);
        Assert.Equal(Trend.Length, withStatus.Samples.Capacity);
        Assert.Null(withStatus.CpuTrend);
        Assert.Null(withStatus.MemoryTrend);
    }

    // ----- CheckAsync -----

    /// <summary>A check the topology names no address for reads nothing, says nothing and changes nothing: it
    /// answers without a request, a reading, an event or a changed, and the monitor stands where it stood.</summary>
    [Fact]
    public async Task ACheckWithNoAddressNamedReadsNothingAndSaysNothing()
    {
        var handler = new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound));
        var monitor = Monitor(new ClusterInfo("aks-demo"), handler);
        var changes = 0;
        monitor.Changed += () => Interlocked.Increment(ref changes);

        await monitor.CheckAsync(CancellationToken.None);

        Assert.Empty(handler.RequestedUrls);
        Assert.Null(monitor.Status);
        Assert.Null(monitor.Service);
        Assert.Empty(monitor.Samples);
        Assert.Empty(_events.Newest);
        Assert.Equal(0, changes);
        Assert.Null(monitor.CpuTrend);
        Assert.Null(monitor.MemoryTrend);
    }

    /// <summary>A check the caller cancels throws the caller's cancellation - the only throw of the monitor's
    /// check, the files themselves never throwing - and stands where it stood: the readings stand pending as
    /// before, nothing was raised and nothing was observed.</summary>
    [Fact]
    public async Task ACheckTheCallerCancelsThrowsAndStandsWhereItStood()
    {
        var handler = new StubHandler((request, cancellation) => StubHandler.NeverAsync(cancellation));
        var monitor = Monitor(new ClusterInfo("aks-demo", new Uri(StatusUrl), new Uri(ServiceUrl)), handler);
        var changes = 0;
        monitor.Changed += () => Interlocked.Increment(ref changes);

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => monitor.CheckAsync(cancellation.Token));

        Assert.Equal(new SourceReading<ClusterStatus>(SourceState.Pending), monitor.Status);
        Assert.Equal(new SourceReading<AksService>(SourceState.Pending), monitor.Service);
        Assert.Empty(monitor.Samples);
        Assert.Empty(_events.Newest);
        Assert.Equal(0, changes);
    }

    /// <summary>The file the topology names alone raises the changed with every reading it has, good or bad: the
    /// reading that reads the file, and the one that finds it not there - and the service the topology does not
    /// name stands absent throughout.</summary>
    [Fact]
    public async Task TheOneFileThatIsNamedRaisesChangedWithEveryReading()
    {
        string? status = Status();
        var monitor = Monitor(new ClusterInfo("aks-demo", new Uri(StatusUrl), null), new StubHandler(request => Answer(request, status, null)));
        var changes = 0;
        monitor.Changed += () => Interlocked.Increment(ref changes);

        await monitor.CheckAsync(CancellationToken.None);
        Assert.Equal(1, changes);

        status = null;
        await monitor.CheckAsync(CancellationToken.None);

        Assert.Equal(2, changes);
        Assert.Equal(new SourceReading<ClusterStatus>(SourceState.Missing, Detail: "the address answered HTTP 404"), monitor.Status);
        Assert.Null(monitor.Service);
    }

    // ----- the readings -----

    /// <summary>The reading a check makes is the reading that stands, good or bad: the answer that is not a good
    /// one replaces the good reading it found, with the reason the address gave - the HTTP state and the missing
    /// file alike - and a check that reads the file stands the good reading over it again.</summary>
    [Fact]
    public async Task TheReadingTheCheckMakesStandsGoodOrBad()
    {
        string? status = Status();
        string? service = ClusterFixture.Sample("aks.json");
        var handler = new StubHandler(request =>
        {
            bool isStatus = request.RequestUri!.AbsoluteUri == StatusUrl;
            string? body = isStatus ? status : service;
            return body is null ? StubHandler.Answer(isStatus ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.InternalServerError)
                : StubHandler.Answer(HttpStatusCode.OK, body);
        });
        var monitor = Monitor(new ClusterInfo("aks-demo", new Uri(StatusUrl), new Uri(ServiceUrl)), handler);

        await monitor.CheckAsync(CancellationToken.None);
        Assert.Equal(SourceState.Read, monitor.Status!.State);
        Assert.Equal(SourceState.Read, monitor.Service!.State);

        status = null;
        service = null;
        await monitor.CheckAsync(CancellationToken.None);
        Assert.Equal(new SourceReading<ClusterStatus>(SourceState.Unavailable, Detail: "the address answered HTTP 503"), monitor.Status);
        Assert.Equal(new SourceReading<AksService>(SourceState.Unavailable, Detail: "the address answered HTTP 500"), monitor.Service);

        status = Status();
        service = ClusterFixture.Sample("aks.json");
        await monitor.CheckAsync(CancellationToken.None);
        Assert.Equal(SourceState.Read, monitor.Status!.State);
        Assert.Equal(SourceState.Read, monitor.Service!.State);
    }

    /// <summary>An answer that is not the file is a reading that says why: the status that has neither nodes nor
    /// namespaces, the answer that is not valid JSON, and the answer that is empty - each with its reason, and
    /// the samples standing empty, for a reading that is not the file is no reading of the file.</summary>
    [Fact]
    public async Task AnAnswerThatIsNotTheFileIsAReadingThatSaysWhy()
    {
        string body = "{}";
        var monitor = Monitor(new ClusterInfo("aks-demo", new Uri(StatusUrl), null), new StubHandler(_ => StubHandler.Answer(HttpStatusCode.OK, body)));

        await monitor.CheckAsync(CancellationToken.None);
        Assert.Equal(new SourceReading<ClusterStatus>(SourceState.Malformed, Detail: "the file has neither nodes nor namespaces"), monitor.Status);

        body = "<!DOCTYPE html>";
        await monitor.CheckAsync(CancellationToken.None);
        Assert.Equal(new SourceReading<ClusterStatus>(SourceState.Malformed, Detail: "the file is not valid JSON"), monitor.Status);

        body = string.Empty;
        await monitor.CheckAsync(CancellationToken.None);

        Assert.Equal(new SourceReading<ClusterStatus>(SourceState.Malformed, Detail: "the file is empty"), monitor.Status);
        Assert.Empty(monitor.Samples);
    }

    /// <summary>The topology that names no environment says no environment in its events: a pod that restarts in
    /// a space none of the environments names is an event of the cluster alone, at the moment the check read it.</summary>
    [Fact]
    public async Task ACheckThatNamesNoEnvironmentSaysNoEnvironmentInItsEvents()
    {
        string? status = Status();
        var monitor = Monitor(new ClusterInfo("aks-demo", new Uri(StatusUrl), null), new StubHandler(request => Answer(request, status, null)));

        await monitor.CheckAsync(CancellationToken.None);
        status = Status().Replace("\"restarts\": 7", "\"restarts\": 9", StringComparison.Ordinal);
        await monitor.CheckAsync(CancellationToken.None);

        Assert.Equal(
            new DashboardEvent(_time.GetUtcNow(), EventKind.Cluster, EventLevel.Warning, null, "cluster", "ui in cmdemo3-tdd restarted (9 restarts): CrashLoopBackOff"),
            _events.Newest.Single());
    }

    // ----- the samples and the trends -----

    /// <summary>A check that reads a status the collector wrote too long ago is a gap in the samples, not a
    /// reading: the stale file stands read, no sample stands for it, and the trend of the one live reading and
    /// the one gap stands absent.</summary>
    [Fact]
    public async Task AStatusThatStopsBeingLiveLeavesAGapInTheSamples()
    {
        var status = Status();
        var monitor = Monitor(new ClusterInfo("aks-demo", new Uri(StatusUrl), null), new StubHandler(request => Answer(request, status, null)));

        await monitor.CheckAsync(CancellationToken.None);
        _time.Advance(TimeSpan.FromSeconds(61));
        await monitor.CheckAsync(CancellationToken.None);

        Assert.Equal([new ClusterSample(812, 9876543210), null], monitor.Samples);
        Assert.Equal(SourceState.Read, monitor.Status!.State);
        Assert.Null(monitor.CpuTrend);
        Assert.Null(monitor.MemoryTrend);
    }

    /// <summary>A node that does not say how much cpu it uses is no point of the cpu trend: the sample stands
    /// with its memory and its cpu unsaid, the trend of the one live reading stands absent, and the memory trend
    /// stands on the two readings it has, at the numbers the node said.</summary>
    [Fact]
    public async Task ANodeThatSaysNoCpuUsageIsNoPointOfTheCpuTrend()
    {
        string? status = OneNode(null, 2147483648);
        var monitor = Monitor(new ClusterInfo("aks-demo", new Uri(StatusUrl), null), new StubHandler(request => Answer(request, status, null)));

        await monitor.CheckAsync(CancellationToken.None);
        status = OneNode(400, 2147483648);
        await monitor.CheckAsync(CancellationToken.None);

        Assert.Equal([new ClusterSample(null, 2147483648), new ClusterSample(400, 2147483648)], monitor.Samples);
        Assert.Null(monitor.CpuTrend);
        var memory = Assert.IsType<Trend>(monitor.MemoryTrend);
        Assert.Equal("Memory used in the cluster, last 2 checks: 2 to 2 GiB, now 2 GiB", memory.Title);
        Assert.Equal([1d, 1], memory.Points);
    }

    /// <summary>A node that says it uses no cpu says a number, and the number stands in the trend: the cpu that
    /// is zero and the cpu that is not, the two points the line stands on and the words that say the two - a
    /// reading of zero, not a gap.</summary>
    [Fact]
    public async Task AUsageThatIsZeroIsAReadingInTheTrend()
    {
        string? status = OneNode(0, 2147483648);
        var monitor = Monitor(new ClusterInfo("aks-demo", new Uri(StatusUrl), null), new StubHandler(request => Answer(request, status, null)));

        await monitor.CheckAsync(CancellationToken.None);
        status = OneNode(800, 2147483648);
        await monitor.CheckAsync(CancellationToken.None);

        Assert.Equal([new ClusterSample(0, 2147483648), new ClusterSample(800, 2147483648)], monitor.Samples);
        var cpu = Assert.IsType<Trend>(monitor.CpuTrend);
        Assert.Equal("CPU used in the cluster, last 2 checks: 0 to 0.8 cores, now 0.8 cores", cpu.Title);
        Assert.Equal([0d, 1], cpu.Points);
    }

    /// <summary>The samples keep the last trend length of readings and no more: the first of the checks that run
    /// the length over stands dropped, and the buffer stands full of the readings it keeps.</summary>
    [Fact]
    public async Task TheSamplesKeepOnlyTheTrendLengthOfReadings()
    {
        string? status = OneNode(50, 2147483648);
        var monitor = Monitor(new ClusterInfo("aks-demo", new Uri(StatusUrl), null), new StubHandler(request => Answer(request, status, null)));

        await monitor.CheckAsync(CancellationToken.None);
        status = OneNode(100, 2147483648);
        for (var round = 0; round < Trend.Length; round++)
        {
            await monitor.CheckAsync(CancellationToken.None);
        }

        Assert.Equal(Trend.Length, monitor.Samples.Count);
        Assert.Equal(100, monitor.Samples[0]!.CpuMillicores);
        Assert.Equal(100, monitor.Samples[^1]!.CpuMillicores);
    }
}
