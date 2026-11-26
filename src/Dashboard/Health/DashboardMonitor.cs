using Dashboard.Cluster;
using Dashboard.Work;

namespace Dashboard.Health;

/// <summary>The state of every endpoint of a topology, and the action that checks them all.</summary>
public sealed class DashboardMonitor
{
    /// <summary>How often the delivery facts are read: they change with a deployment, and GitHub caches the file for minutes.</summary>
    public static readonly TimeSpan DeliveryInterval = TimeSpan.FromMinutes(5);

    /// <summary>How often the cost is read: its file changes a few times a day at most, and is read as the delivery facts are.</summary>
    public static readonly TimeSpan CostInterval = DeliveryInterval;

    /// <summary>
    /// How often the work items are read: they sit in <c>workitems.json</c> beside the delivery facts, so they are
    /// re-read on the delivery facts' cadence.
    /// </summary>
    public static readonly TimeSpan WorkItemsInterval = DeliveryInterval;

    /// <summary>
    /// How often Resource Health is read: it is read beside the delivery facts while a box of the runtime view
    /// shows it, so it re-reads on the delivery facts' cadence.
    /// </summary>
    public static readonly TimeSpan ResourceHealthInterval = DeliveryInterval;

    /// <summary>
    /// How often the deployments in flight are read: the file is small, and GitHub may serve it a few minutes old, so
    /// more often gains nothing.
    /// </summary>
    public static readonly TimeSpan DeploymentsInterval = TimeSpan.FromSeconds(60);

    /// <summary>How long the page waits before it asks again for its own build facts, while the file does not answer.</summary>
    public static readonly TimeSpan DashboardBuildRetry = DeliveryInterval;

    private readonly NodeProber _prober;
    private readonly PinnedVersionsReader _versions;
    private readonly TimeProvider _time;
    private readonly Uri _pageAddress;
    private readonly List<(EnvironmentStatus Environment, DeployableStatus Deployable, TargetStatus Target)> _targets;
    private readonly Dictionary<ContainerAppTarget, ContainerAppProbe> _containerApps = [];
    private readonly IntervalRead<DeliveryReport> _delivery = new(DeliveryInterval);
    private readonly IntervalRead<WorkItemsReport> _workItems = new(WorkItemsInterval);
    private readonly IntervalRead<CostReport> _cost = new(CostInterval);
    private readonly IntervalRead<DeploymentsReport> _deployments = new(DeploymentsInterval);
    private readonly IntervalRead<BuildInfo> _dashboardBuild = new(DashboardBuildRetry);
    private readonly IntervalRead<ResourceHealthReport> _resourceHealth = new(ResourceHealthInterval);
    private IReadOnlyList<ContainerAppProbe> _watched = [];
    private bool _resourceHealthWanted;
    private HealthSummary? _summaryCache;

    /// <param name="events">
    /// Where the monitor writes what it observes; the page keeps one log across reloads of the topology. A log of its
    /// own without one.
    /// </param>
    /// <param name="cluster">
    /// What reads the cluster's files, for a topology with <c>cluster</c>; without it, or without a cluster in the
    /// topology, the monitor has no cluster and reads nothing for one.
    /// </param>
    /// <param name="pageAddress">
    /// The address the page is served from: the dashboard's own build file is read from its site
    /// (<see cref="NodeProber.ReadOwnBuildAsync"/>), whatever the prober's client is set up to.
    /// </param>
    public DashboardMonitor(
        Topology topology,
        NodeProber prober,
        PinnedVersionsReader versions,
        TimeProvider time,
        Uri pageAddress,
        EventLog? events = null,
        ClusterReader? cluster = null)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(pageAddress);
        Topology = topology;
        _prober = prober;
        _versions = versions;
        _time = time;
        _pageAddress = pageAddress;
        Events = events ?? new EventLog();
        Environments = [.. topology.Environments.Select(environment => new EnvironmentStatus(environment))];
        _targets =
        [
            .. from environment in Environments
               from deployable in environment.Deployables
               from target in deployable.Targets
               select (environment, deployable, target),
        ];
        if (topology.Cluster is { } info && cluster is not null)
        {
            Cluster = new ClusterMonitor(info, topology.Environments, cluster, time, Events);
            Cluster.Changed += RaiseChanged;
        }
    }

    /// <summary>Raised whenever an endpoint's state changed and when a round of checks ended.</summary>
    public event Action? Changed;

    public Topology Topology { get; }

    public IReadOnlyList<EnvironmentStatus> Environments { get; }

    public IEnumerable<TargetStatus> Targets => _targets.Select(entry => entry.Target);

    public HealthSummary Summary => _summaryCache ??= HealthSummary.Of(Targets.Select(target => target.State));

    /// <summary>Invalidates the cached summary and raises <see cref="Changed" />.</summary>
    private void RaiseChanged()
    {
        _summaryCache = null;
        Changed?.Invoke();
    }

    /// <summary>In how many environments a node runs another version than the one pinned in Git.</summary>
    public VersionSummary VersionSummary => new(Environments.Count(environment => environment.VersionsDiffer));

    /// <summary>When the last round of checks ended; null before the first one.</summary>
    public DateTimeOffset? LastRefresh { get; private set; }

    /// <summary>What the page observed: state changes, restarts, deployments, failovers, pins.</summary>
    public EventLog Events { get; }

    /// <summary>
    /// The system's delivery facts, as last read; null when the topology names no <c>deliveryUrl</c> or the file was
    /// never read. A reading that fails keeps the last good one: the file says when it was generated.
    /// </summary>
    public DeliveryReport? Delivery => _delivery.Last;

    /// <summary>
    /// The work items the system's bots are handling, as last read; null when the topology names no delivery file
    /// (there is then no <c>workitems.json</c> next to it), the file was never read, or it is not there yet. A reading
    /// that fails keeps the last good one.
    /// </summary>
    public WorkItemsReport? WorkItems => _workItems.Last;

    /// <summary>True after a read of the work items was attempted, so the view can tell "loading" from "not published".</summary>
    public bool WorkItemsAsked => _workItems.Asked;

    /// <summary>The three facts of the work items read, by name: the address the topology names, the ask, the report.</summary>
    public WorkItemsRead WorkItemsRead => new(HasAddress: Topology.System.WorkItemsUrl is not null, Asked: WorkItemsAsked, Report: WorkItems);

    /// <summary>
    /// What the system cost in Azure, as last read; null when the topology names no <c>costUrl</c> or the file was
    /// never read. A reading that fails keeps the last good one: the file names the day its numbers are of.
    /// </summary>
    public CostReport? Cost => _cost.Last;

    /// <summary>
    /// The system's deployments in flight, as last read; null when the topology names no <c>deploymentsUrl</c>, the
    /// file was never read or the last reading failed. A reading that fails replaces a good one, unlike the cost: a
    /// deployment that is executing in a file the page can no longer read would stay marked for ever.
    /// </summary>
    public DeploymentsReport? Deployments { get; private set; }

    /// <summary>
    /// What is marked as being deployed in an environment at <paramref name="now"/> (<see cref="DeploymentsReport.Marks"/>);
    /// nothing without the file.
    /// </summary>
    /// <param name="deployables">The deployables a mark may belong to.</param>
    public IReadOnlyList<DeploymentMark> DeploymentMarks(string environment, IEnumerable<string> deployables, DateTimeOffset now) =>
        Deployments?.Marks(environment, Topology.System.Slug, deployables, now) ?? [];

    /// <summary>
    /// What the deployments file says in words about one deployable in an environment (<see cref="DeploymentActivity.OfNode"/>);
    /// empty without the file.
    /// </summary>
    public IReadOnlyList<ActivityLine> NodeActivity(string environment, string deployable, DateTimeOffset now, TimeZoneInfo zone) =>
        Deployments is { } report ? DeploymentActivity.OfNode(report, environment, Topology.System.Slug, deployable, now, zone) : [];

    /// <summary>
    /// What follows the marks under an environment's name (<see cref="DeploymentActivity.OfEnvironment"/>): a
    /// deployment freeze and the last deployment that ended; empty without the file.
    /// </summary>
    public IReadOnlyList<ActivityLine> EnvironmentActivity(string environment, IReadOnlyList<DeploymentMark> marks, DateTimeOffset now, TimeZoneInfo zone) =>
        Deployments is { } report ? DeploymentActivity.OfEnvironment(report, environment, marks, now, zone) : [];

    /// <summary>
    /// The build of the dashboard itself (this page), from the file its own site serves; null when the topology names
    /// no <c>system.dashboard</c> or the file was not read. It belongs to the release that serves the page, so it is
    /// read once: until it answers, every <see cref="DashboardBuildRetry"/>.
    /// </summary>
    public BuildInfo? DashboardBuild => _dashboardBuild.Last;

    /// <summary>
    /// The cluster the system runs in, read with every round of checks; null when the topology names none, and the
    /// page then has no cluster view.
    /// </summary>
    public ClusterMonitor? Cluster { get; }

    /// <summary>The environment the others are compared with: the first of the topology.</summary>
    public string? FirstEnvironment => Topology.Environments.Count > 0 ? Topology.Environments[0].Name : null;

    /// <summary>
    /// What Resource Health said of the system's resources, as last read; null while no box of the runtime view asks
    /// for it, without <c>system.deliveryUrl</c>, or before the file was read. A reading that fails keeps the last good one.
    /// </summary>
    public ResourceHealthReport? ResourceHealth => _resourceHealth.Last;

    /// <summary>
    /// The container apps and resources the runtime view shows: from the next round on, the page probes those apps
    /// (each every <see cref="ContainerAppProbe.Interval"/>) and reads Resource Health while a box asks for it. An app
    /// it showed before keeps what was seen of it. <see cref="RuntimeWatch.None"/> stops both.
    /// </summary>
    /// <returns>
    /// True when this call made something due at once: it shows an app never probed, or asks for Resource Health never
    /// read. The same watch again is false, so a caller that checks on true asks once.
    /// </returns>
    public bool Watch(RuntimeWatch watch)
    {
        ArgumentNullException.ThrowIfNull(watch);
        var watched = new List<ContainerAppProbe>();
        foreach (var target in watch.Apps)
        {
            if (!_containerApps.TryGetValue(target, out var probe))
            {
                probe = new ContainerAppProbe(target);
                _containerApps[target] = probe;
            }

            watched.Add(probe);
        }

        var newApps = !watched.SequenceEqual(_watched) && watched.Any(probe => probe.IsNew);
        var newHealth = watch.ResourceHealth && !_resourceHealthWanted && Topology.System.ContainersUrl is not null && _resourceHealth.LastAsked is null;
        _watched = watched;
        _resourceHealthWanted = watch.ResourceHealth;
        return newApps || newHealth;
    }

    /// <summary>What the page saw of a container app; null for one it never showed.</summary>
    public ContainerAppStatus? ContainerAppStatusOf(ContainerAppTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return _containerApps.GetValueOrDefault(target)?.Status;
    }

    /// <summary>
    /// Checks every endpoint at the same time. Each result is recorded as it arrives, so a node that hangs until its
    /// timeout delays neither the others nor their display. The pinned versions are read at the same time, once per
    /// environment, and once per deployable that has a pin of its own (<c>pinUrl</c>); with the probe Health check, a
    /// node whose deployable names a <c>healthDetailPath</c> is also asked for the entries of its health check. A file that cannot be read is a
    /// result like any other and fails no check. So are the delivery facts, every <see cref="DeliveryInterval"/>, the
    /// work items, every <see cref="WorkItemsInterval"/>, the cost, every <see cref="CostInterval"/>, the deployments in flight, every
    /// <see cref="DeploymentsInterval"/>, the
    /// dashboard's own build facts, once, and the two files of the cluster view, every round, where the topology names
    /// a cluster. The container apps the runtime view shows are probed when their interval is due, and Resource Health
    /// is read every <see cref="ResourceHealthInterval"/> while a box shows it (<see cref="Watch"/>).
    /// </summary>
    public async Task CheckAllAsync(ProbeKind probe, CancellationToken cancellationToken)
    {
        var checks = _targets.Select(entry => CheckAsync(entry.Environment, entry.Deployable, entry.Target, probe, cancellationToken));
        var readings = Environments.Select(environment => ReadPinnedVersionsAsync(environment, cancellationToken));
        var pins =
            from environment in Environments
            from deployable in environment.Deployables
            where deployable.Info.PinUrl is not null
            select ReadPinAsync(environment, deployable, cancellationToken);
        var started = _time.GetUtcNow();
        var containerApps = _watched.Where(app => app.IsDue(started)).Select(app => ProbeContainerAppAsync(app, probe, started, cancellationToken)).ToList();
        await Task.WhenAll(checks.Concat(readings).Concat(pins).Concat(containerApps)
            .Append(ReadDeliveryAsync(cancellationToken))
            .Append(ReadWorkItemsAsync(cancellationToken))
            .Append(ReadCostAsync(cancellationToken))
            .Append(ReadDeploymentsAsync(cancellationToken))
            .Append(ReadDashboardBuildAsync(cancellationToken))
            .Append(ReadResourceHealthAsync(cancellationToken))
            .Append(Cluster?.CheckAsync(cancellationToken) ?? Task.CompletedTask));
        var now = _time.GetUtcNow();
        AssessServingRound(now);

        LastRefresh = now;
        RaiseChanged();
    }

    /// <summary>
    /// Assesses and records the serving decision of every deployable. It runs once per round, when every node has
    /// its result: in the middle of a round it would mix this round's answers with the last one's, so each
    /// deployable's decision is compared with <see cref="DeployableStatus.LastServing"/> exactly here and nowhere
    /// else.
    /// </summary>
    /// <param name="now">When the round ended: the time every serving decision is stamped and compared at.</param>
    private void AssessServingRound(DateTimeOffset now)
    {
        foreach (var environment in Environments)
        {
            foreach (var deployable in environment.Deployables)
            {
                var assessment = deployable.Assess();
                Events.AddRange(EventDetector.ServingChanges(deployable.LastServing, assessment, environment.Name, deployable.Info.Name, now));

                deployable.RecordServing(assessment);
            }
        }
    }

    private async Task ReadPinnedVersionsAsync(EnvironmentStatus environment, CancellationToken cancellationToken)
    {
        if (environment.Info.VersionsUrl is not { } address)
        {
            return;
        }

        var before = environment.Pinned;
        var after = await _versions.ReadAsync(address, cancellationToken);
        environment.Record(after);
        if (after.State == PinnedVersionsState.Read)
        {
            // Compared with the last good reading: a reading that failed in between hides no change.
            var known = environment.LastReadPinned ?? before;
            var now = _time.GetUtcNow();
            Events.AddRange(environment.Deployables
                // A deployable with a pin of its own is not pinned in this file.
                .Where(deployable => deployable.Info.PinUrl is null)
                .Select(deployable => EventDetector.Pinned(known, after, environment.Name, deployable.Info.Name, now))
                .OfType<DashboardEvent>());
            environment.RecordGoodPinned(after);
        }

        RaiseChanged();
    }

    /// <summary>Reads the pin of a deployable that has one of its own: its Kustomize file, by the rules of <c>versions.json</c>.</summary>
    private async Task ReadPinAsync(EnvironmentStatus environment, DeployableStatus deployable, CancellationToken cancellationToken)
    {
        if (deployable.Info.PinUrl is not { } address)
        {
            return;
        }

        var before = deployable.Pinned;
        var after = await _versions.ReadKustomizationAsync(address, deployable.Info.Name, cancellationToken);
        deployable.RecordPin(after);
        if (after.State == PinnedVersionsState.Read)
        {
            // Compared with the last good reading: a reading that failed in between hides no change.
            if ((deployable.LastReadPinned ?? before) is { } known
                && EventDetector.Pinned(known, after, environment.Name, deployable.Info.Name, _time.GetUtcNow()) is { } change)
            {
                Events.Add(change);
            }

            deployable.RecordGoodPin(after);
        }

        RaiseChanged();
    }

    private async Task ReadDeliveryAsync(CancellationToken cancellationToken)
    {
        if (Topology.System.DeliveryUrl is not { } address)
        {
            return;
        }

        if (await _delivery.ReadIfDueAsync(_time.GetUtcNow(), token => _prober.ReadDeliveryAsync(address, token), cancellationToken) is not null)
        {
            RaiseChanged();
        }
    }

    /// <summary>
    /// Reads <c>workitems.json</c> from beside the delivery facts. A reading that fails keeps the last good one, and
    /// the view says the file is not published until the first one succeeds.
    /// </summary>
    private async Task ReadWorkItemsAsync(CancellationToken cancellationToken)
    {
        if (Topology.System.WorkItemsUrl is not { } address)
        {
            return;
        }

        if (await _workItems.ReadIfDueAsync(_time.GetUtcNow(), token => _prober.ReadWorkItemsAsync(address, token), cancellationToken) is not null)
        {
            RaiseChanged();
        }
    }

    private async Task ReadCostAsync(CancellationToken cancellationToken)
    {
        if (Topology.System.CostUrl is not { } address)
        {
            return;
        }

        if (await _cost.ReadIfDueAsync(_time.GetUtcNow(), token => _prober.ReadCostAsync(address, token), cancellationToken) is not null)
        {
            RaiseChanged();
        }
    }

    /// <summary>
    /// Reads what is being deployed. It marks, and nothing else: no event, no state of a node, no summary. A reading
    /// that fails marks nothing.
    /// </summary>
    private async Task ReadDeploymentsAsync(CancellationToken cancellationToken)
    {
        var now = _time.GetUtcNow();
        if (Topology.System.DeploymentsUrl is not { } address || !_deployments.IsDue(now))
        {
            return;
        }

        var before = Deployments;
        Deployments = await _deployments.AskIfDueAsync(now, token => _prober.ReadDeploymentsAsync(address, token), cancellationToken);
        if (before is not null || Deployments is not null)
        {
            RaiseChanged();
        }
    }

    /// <summary>One probe of a container app the runtime view shows; its result is shown as it arrives.</summary>
    private async Task ProbeContainerAppAsync(ContainerAppProbe app, ProbeKind probe, DateTimeOffset now, CancellationToken cancellationToken)
    {
        app.Start(now);
        app.Record(await _prober.ProbeContainerAppAsync(app.Target, probe, app.Status.CorsRefused, cancellationToken));
        RaiseChanged();
    }

    private async Task ReadResourceHealthAsync(CancellationToken cancellationToken)
    {
        if (!_resourceHealthWanted || Topology.System.ContainersUrl is not { } address)
        {
            return;
        }

        if (await _resourceHealth.ReadIfDueAsync(_time.GetUtcNow(), token => _prober.ReadResourceHealthAsync(address, token), cancellationToken) is not null)
        {
            RaiseChanged();
        }
    }

    private async Task ReadDashboardBuildAsync(CancellationToken cancellationToken)
    {
        if (Topology.System.Dashboard is not { } dashboard || DashboardBuild is not null)
        {
            return;
        }

        if (await _dashboardBuild.ReadIfDueAsync(_time.GetUtcNow(), token => _prober.ReadOwnBuildAsync(_pageAddress, dashboard.BuildPath, token), cancellationToken) is not null)
        {
            RaiseChanged();
        }
    }

    private async Task CheckAsync(EnvironmentStatus environment, DeployableStatus status, TargetStatus target, ProbeKind probe, CancellationToken cancellationToken)
    {
        var deployable = status.Info;
        var kind = target.Behavior;

        // A regional node also reports its calls; a Front Door address would only answer for the node behind it.
        var telemetry = kind.ReadsTelemetry && deployable.TelemetryPath is { } path
            ? _prober.ReadTelemetryAsync(target.Url, path, cancellationToken)
            : Task.FromResult<TelemetrySnapshot?>(null);

        // And what its health check found, entry by entry: only with the probe Health check, because the detailed
        // check connects to the database like the health check itself, and Liveness is there to leave it alone.
        var detail = kind.ReadsHealthDetail && probe == ProbeKind.Health && deployable.HealthDetailPath is { } detailPath
            ? _prober.ReadHealthDetailAsync(target.Url, detailPath, cancellationToken)
            : Task.FromResult<HealthDetail?>(null);
        var result = await _prober.ProbeAsync(target.Url, deployable.PathFor(probe), deployable.VersionPath, cancellationToken);
        var before = NodeObservation.Of(target) with { Checks = target.LastReadHealthDetail };
        target.Record(result with { Probe = probe });
        target.RecordTelemetry(await telemetry);
        target.RecordHealthDetail(await detail);
        Events.AddRange(EventDetector.Node(
            // A check without telemetry (the app was down) is compared with the last reading that had some; so are
            // the entries of the detailed health check (a check that read none, or the probe Liveness, hides no change).
            before with { Telemetry = before.Telemetry ?? target.Samples.Reverse().Skip(1).FirstOrDefault(sample => sample is not null) },
            NodeObservation.Of(target),
            environment.Name,
            deployable.Name,
            target.Label,
            kind.IsNode,
            _time.GetUtcNow()));
        RaiseChanged();

        // The build facts change only with a deployment: asked once, and again when the node reports another version.
        if (target == status.BuildSource && deployable.BuildPath is { } buildPath
            && result.State != HealthState.Unreachable && status.BuildIsDue(target.Version))
        {
            var version = target.Version;
            status.RecordBuild(await _prober.ReadBuildAsync(target.Url, buildPath, cancellationToken), version);
            RaiseChanged();
        }
    }
}
