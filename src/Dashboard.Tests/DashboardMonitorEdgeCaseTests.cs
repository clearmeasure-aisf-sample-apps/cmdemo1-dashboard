using System.Net;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="DashboardMonitor" />: the page's monitor of its
/// topology - the targets it checks, the readings it keeps of the system's files, the marks and activity lines it
/// draws from the deployments file, and the container apps and Resource Health the runtime view asks it to watch.
/// <see cref="DashboardMonitorTests" /> and its siblings pin the round of checks - every target probed, the pinned
/// versions read, and the delivery facts, the work items, the cost, the deployments in flight, the dashboard's own
/// build facts and the cluster's two files, each on its own interval, with the changed a round raises - and what
/// they leave open is pinned here: the intervals the page and the site agree on, the monitor at its making, asked
/// for its topology and only for it, keeping the topology it was given and standing with its own log and no
/// cluster; the readings that stand null before the first round, the work items not asked yet; the topology with
/// no environment, which has no first environment, no target and no version summary, and whose round checks
/// nothing, asks nothing and still ends; the round the caller cancels, which throws and stands where it stood; the
/// watch and the app asked for and only for it, each naming the argument; and Resource Health, read every five
/// minutes while a box shows it, not once the box is gone, a failed reading of which keeps the last good one - and
/// the watch for it the topology without a delivery address says is nothing new.
/// <para>
/// Nothing here reaches a network or asks a clock what time it is: every answer is one the test hands back, and
/// every moment is one the test brings.
/// </para>
/// </summary>
public class DashboardMonitorEdgeCaseTests
{
    private const string OneNode = """
        {
          "system": { "slug": "demo", "name": "Demo" },
          "environments": [
            { "name": "tdd", "deployables": [ { "name": "ui", "nodes": [ { "name": "tdd-west", "region": "westus3", "role": "primary", "url": "https://tdd-west.example.net" } ] } ] }
          ]
        }
        """;

    private const string ContainersPath = "/org/demo-system/status/containers.json";

    private readonly SignallingTimeProvider _time = new();

    private DashboardMonitor Monitor(StubHandler handler, string topology = Optics.Topology)
    {
        var http = new HttpClient(handler);
        return new DashboardMonitor(
            TopologyParser.Parse(topology).Topology!,
            new NodeProber(http, _time),
            new PinnedVersionsReader(http, _time),
            _time);
    }

    // ----- the intervals -----

    /// <summary>The intervals the page and the site agree on: the delivery facts, the cost and the dashboard's own
    /// build facts are each read every five minutes. The deployments file is read every minute; that one stands in
    /// <see cref="DeploymentsTests" />.</summary>
    [Fact]
    public void TheDeliveryCostAndDashboardBuildIntervalsAreFiveMinutes()
    {
        Assert.Equal(TimeSpan.FromMinutes(5), DashboardMonitor.DeliveryInterval);
        Assert.Equal(TimeSpan.FromMinutes(5), DashboardMonitor.CostInterval);
        Assert.Equal(TimeSpan.FromMinutes(5), DashboardMonitor.DashboardBuildRetry);
    }

    // ----- the monitor at its making -----

    /// <summary>The monitor is asked for its topology, and only for it: without one it is said so, naming the
    /// argument - and made with no log of its own and no reader of a cluster the topology does not name, it keeps
    /// the topology it was given, stands with a log of its own and no cluster, and its readings stand null.</summary>
    [Fact]
    public void TheMonitorIsAskedForItsTopologyAndOnlyForIt()
    {
        var error = Assert.Throws<ArgumentNullException>(() => new DashboardMonitor(null!, null!, null!, null!));

        Assert.Equal("topology", error.ParamName);

        var topology = TopologyParser.Parse(Optics.Topology).Topology!;
        var monitor = new DashboardMonitor(topology, null!, null!, null!);

        Assert.Same(topology, monitor.Topology);
        Assert.NotNull(monitor.Events);
        Assert.Empty(monitor.Events.Newest);
        Assert.Null(monitor.Cluster);
    }

    /// <summary>The monitor is asked for the watch, and for the app it is asked about, and only for them: without
    /// the watch it is said so, naming the argument, and the app the page never showed is said so, naming that
    /// argument - at the making of the answer, before and after every round.</summary>
    [Fact]
    public void TheWatchAndTheAppAreAskedForAndOnlyForThem()
    {
        var monitor = Monitor(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound)));

        var watch = Assert.Throws<ArgumentNullException>(() => monitor.Watch(null!));
        Assert.Equal("watch", watch.ParamName);

        var target = Assert.Throws<ArgumentNullException>(() => monitor.ContainerAppStatusOf(null!));
        Assert.Equal("target", target.ParamName);
    }

    // ----- the readings -----

    /// <summary>Before the first round, every reading stands null and the work items are not asked yet: the
    /// monitor keeps the last good reading of each file, and the last good reading of no reading is nothing.</summary>
    [Fact]
    public void BeforeTheFirstRoundEveryReadingStandsNull()
    {
        var monitor = Monitor(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound)));

        Assert.Null(monitor.LastRefresh);
        Assert.Null(monitor.Delivery);
        Assert.Null(monitor.WorkItems);
        Assert.False(monitor.WorkItemsAsked);
        Assert.Null(monitor.Cost);
        Assert.Null(monitor.Deployments);
        Assert.Null(monitor.DashboardBuild);
        Assert.Null(monitor.ResourceHealth);
        Assert.Null(monitor.Cluster);
    }

    // ----- the topology with no environment -----

    /// <summary>The topology with no environment has no first environment, no target, no version summary, and no
    /// summary but "no nodes": its round checks nothing and asks nothing, and still ends, raising the changed
    /// once and stamping the refresh.</summary>
    [Fact]
    public async Task ATopologyWithNoEnvironmentsHasNoFirstEnvironmentAndItsRoundStillEnds()
    {
        var handler = new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound));
        var monitor = Monitor(handler, """
            { "system": { "slug": "demo", "name": "Demo" }, "environments": [] }
            """);
        var changes = 0;
        monitor.Changed += () => Interlocked.Increment(ref changes);

        Assert.Null(monitor.FirstEnvironment);
        Assert.Empty(monitor.Environments);
        Assert.Empty(monitor.Targets);
        Assert.Equal("No nodes in the topology", monitor.Summary.Text);
        Assert.False(monitor.VersionSummary.Differs);
        Assert.Null(monitor.VersionSummary.Text);

        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);

        Assert.Equal(1, changes);
        Assert.Equal(_time.GetUtcNow(), monitor.LastRefresh);
        Assert.Empty(handler.Requests);
    }

    // ----- CheckAllAsync -----

    /// <summary>A round the caller cancels throws the caller's cancellation - the only throw of the monitor's
    /// round, the endpoints themselves never throwing - and stands where it stood: no target has its result, the
    /// round has not ended and nothing was raised.</summary>
    [Fact]
    public async Task ARoundTheCallerCancelsThrowsAndStandsWhereItStood()
    {
        var monitor = Monitor(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.OK, "Healthy")), OneNode);
        var changes = 0;
        monitor.Changed += () => Interlocked.Increment(ref changes);

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => monitor.CheckAllAsync(ProbeKind.Health, cancellation.Token));

        Assert.Null(monitor.LastRefresh);
        Assert.Equal(0, changes);
        Assert.All(monitor.Targets, target => Assert.Equal(HealthState.Pending, target.State));
    }

    // ----- Watch and Resource Health -----

    /// <summary>Resource Health is read beside the delivery facts every five minutes while a box shows it - at
    /// the fifth minute, not before - and not once the box is gone; a reading that fails keeps the last good one,
    /// for the file says when it was read.</summary>
    [Fact]
    public async Task ResourceHealthIsReadEveryFiveMinutesWhileABoxShowsItAndNotAfterItIsShownNoMore()
    {
        string? containers = """
            { "resources": { "nonprod": [ { "id": "/s/kv", "health": { "status": "available", "source": "resourceHealth", "checkedAt": "2026-10-04T21:20:00Z" } } ] } }
            """;
        var handler = new StubHandler(request => request.RequestUri!.AbsolutePath == ContainersPath
            ? containers is null ? StubHandler.Answer(HttpStatusCode.NotFound) : StubHandler.Answer(HttpStatusCode.OK, containers)
            : StubHandler.Answer(HttpStatusCode.NotFound));
        var monitor = Monitor(handler);
        static int Asked(StubHandler handler) => handler.Requests.Count(request => request.RequestUri!.AbsolutePath == ContainersPath);

        Assert.True(monitor.Watch(new RuntimeWatch([], true)));

        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);
        Assert.Equal(1, Asked(handler));
        Assert.Equal("available", monitor.ResourceHealth!.Find("/s/kv")!.Status);

        _time.Advance(TimeSpan.FromMinutes(4));
        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);
        Assert.Equal(1, Asked(handler));

        _time.Advance(TimeSpan.FromMinutes(1));
        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);
        Assert.Equal(2, Asked(handler));

        containers = null;
        _time.Advance(TimeSpan.FromMinutes(5));
        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);
        Assert.Equal(3, Asked(handler));
        Assert.Equal("available", monitor.ResourceHealth.Find("/s/kv")!.Status);

        Assert.False(monitor.Watch(RuntimeWatch.None));
        _time.Advance(TimeSpan.FromMinutes(5));
        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);

        Assert.Equal(3, Asked(handler));
        Assert.Equal("available", monitor.ResourceHealth.Find("/s/kv")!.Status);
    }

    /// <summary>The watch for Resource Health the topology without a delivery address says is nothing new - there
    /// is no <c>containers.json</c> beside a <c>delivery.json</c> there is not - and its round reads nothing for
    /// it: the report stands null, and the box fails no check.</summary>
    [Fact]
    public async Task AResourceHealthWatchWithoutADeliveryAddressIsNothingNewAndReadsNothing()
    {
        var handler = new StubHandler(request => Optics.Answer(request));
        var monitor = Monitor(handler, Optics.Topology.Replace("deliveryUrl", "ignored", StringComparison.Ordinal));

        Assert.False(monitor.Watch(new RuntimeWatch([], true)));

        await monitor.CheckAllAsync(ProbeKind.Health, CancellationToken.None);

        Assert.Null(monitor.ResourceHealth);
        Assert.DoesNotContain(handler.Requests, request => request.RequestUri!.AbsolutePath.EndsWith("containers.json", StringComparison.Ordinal));
        Assert.Equal("All 4 nodes healthy", monitor.Summary.Text);
    }
}
