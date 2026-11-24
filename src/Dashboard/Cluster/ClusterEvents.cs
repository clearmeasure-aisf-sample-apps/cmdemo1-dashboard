using System.Globalization;
using Dashboard.Health;

namespace Dashboard.Cluster;

/// <summary>
/// Finds the events of the cluster between two readings, as <see cref="EventDetector"/> does for the endpoints: what
/// changed from one check to the next, in words. The first reading is no event.
/// </summary>
public static class ClusterEventDetector
{
    /// <summary>What an event of the cluster as a whole names as its place.</summary>
    public const string ClusterPlace = "cluster";

    /// <summary>What an event of the AKS service names as its place.</summary>
    public const string ServicePlace = "AKS";

    /// <summary>
    /// How many pods a round names one by one: a node that fails takes every pod with it, and the log keeps fifty
    /// events. The others are counted in one more event.
    /// </summary>
    public const int NamedPods = 10;

    /// <summary>
    /// The cluster's status file stopped answering or answers again, or the collector stopped writing or writes
    /// again. Null when nothing changed, and at the first reading.
    /// </summary>
    /// <param name="detail">Why the file was not read, as the end of a sentence.</param>
    public static DashboardEvent? Liveness(ClusterLiveness before, ClusterLiveness after, string? detail, DateTimeOffset at)
    {
        if (before == after || before == ClusterLiveness.Pending || after == ClusterLiveness.Pending)
        {
            return null;
        }

        var reason = ClusterText.Reason(detail);
        var notice = after switch
        {
            ClusterLiveness.Silent => new ClusterNotice(EventLevel.Problem, $"The cluster's status file stopped answering{reason}"),
            ClusterLiveness.Unreadable => new ClusterNotice(EventLevel.Problem, $"The cluster's status file could not be read{reason}"),
            ClusterLiveness.Stale => new ClusterNotice(EventLevel.Problem, "The collector in the cluster stopped writing: the status file is stale"),
            _ when before == ClusterLiveness.Stale => new ClusterNotice(EventLevel.Good, "The collector in the cluster writes again"),
            _ => new ClusterNotice(EventLevel.Good, "The cluster's status file answers again"),
        };
        return new DashboardEvent(at, EventKind.Cluster, notice.Level, null, ClusterPlace, notice.Text);
    }

    /// <summary>
    /// What changed in the cluster between the last status the page read and this one: a node that is no longer ready
    /// or is ready again, and per pod one event at most: its restart count rose, or else it became unhealthy, or else
    /// it is ready again. A node or a pod the last status did not have is no event, unless the pod is unhealthy.
    /// </summary>
    /// <param name="environments">The environments of the topology: a pod of an environment's namespace is an event of that environment.</param>
    public static IReadOnlyList<DashboardEvent> Status(
        ClusterStatus before,
        ClusterStatus after,
        IEnumerable<EnvironmentInfo> environments,
        DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(before);
        ArgumentNullException.ThrowIfNull(after);
        ArgumentNullException.ThrowIfNull(environments);
        var events = new List<DashboardEvent>(NodeChanges(before, after, at));
        var changes = PodChanges(before, after, EnvironmentNames(environments), at);
        events.AddRange(NamedPodChanges(changes));
        if (Overflow(changes, at) is { } overflow)
        {
            events.Add(overflow);
        }

        return events;
    }

    /// <summary>Each node of this reading whose readiness differs from its first reading in the last one, in this reading's order.</summary>
    private static IEnumerable<DashboardEvent> NodeChanges(ClusterStatus before, ClusterStatus after, DateTimeOffset at)
    {
        var nodes = FirstReadingOfNodes(before);
        foreach (var node in after.Nodes)
        {
            if (nodes.TryGetValue(node.Name, out var known) && known.Ready != node.Ready)
            {
                yield return node.Ready
                    ? new DashboardEvent(at, EventKind.Cluster, EventLevel.Good, null, ClusterPlace, $"Node {ClusterText.ShortNode(node.Name)} is ready again")
                    : new DashboardEvent(at, EventKind.Cluster, EventLevel.Problem, null, ClusterPlace, $"Node {ClusterText.ShortNode(node.Name)} is not ready");
            }
        }
    }

    /// <summary>The nodes of a reading by name: of a name listed twice, the first reading is the one compared.</summary>
    private static Dictionary<string, ClusterNode> FirstReadingOfNodes(ClusterStatus status) =>
        status.Nodes
            .GroupBy(node => node.Name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

    /// <summary>The pods of a reading by namespace and name: of a pod listed twice, the first reading is the one compared.</summary>
    private static Dictionary<(string Namespace, string Pod), ClusterPod> FirstReadingOfPods(ClusterStatus status) =>
        status.Pods
            .GroupBy(entry => (entry.Namespace.Name, entry.Pod.Name))
            .ToDictionary(group => group.Key, group => group.First().Pod);

    /// <summary>The environment's name per namespace: of a namespace two environments share, the first one names it.</summary>
    private static Dictionary<string, string> EnvironmentNames(IEnumerable<EnvironmentInfo> environments) =>
        environments
            .Where(environment => environment.Namespace is not null)
            .GroupBy(environment => environment.Namespace!, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First().Name, StringComparer.Ordinal);

    /// <summary>The one event of each pod that changed, in this reading's order, named for its namespace's environment.</summary>
    private static List<DashboardEvent> PodChanges(ClusterStatus before, ClusterStatus after, Dictionary<string, string> names, DateTimeOffset at)
    {
        var (beforeAt, afterAt) = (before.AsOf(at), after.AsOf(at));
        var pods = FirstReadingOfPods(before);
        var changes = new List<DashboardEvent>();
        foreach (var (space, pod) in after.Pods)
        {
            var known = pods.GetValueOrDefault((space.Name, pod.Name));
            if (Pod(known, known is null ? null : PodRules.StateOf(known, beforeAt), pod, PodRules.StateOf(pod, afterAt), space.Name, afterAt) is { } notice)
            {
                changes.Add(new DashboardEvent(at, EventKind.Cluster, notice.Level, names.GetValueOrDefault(space.Name), ClusterPlace, notice.Text));
            }
        }

        return changes;
    }

    /// <summary>The pod changes named one by one: the first <see cref="NamedPods"/>, counted over the pods alone - nodes never count against them.</summary>
    private static IEnumerable<DashboardEvent> NamedPodChanges(List<DashboardEvent> changes) => changes.Take(NamedPods);

    /// <summary>The one event that counts the pod changes past the <see cref="NamedPods"/> named; null when none is left over.</summary>
    private static DashboardEvent? Overflow(List<DashboardEvent> changes, DateTimeOffset at) =>
        changes.Count > NamedPods
            ? new DashboardEvent(
                at,
                EventKind.Cluster,
                EventLevel.Info,
                null,
                ClusterPlace,
                string.Create(CultureInfo.InvariantCulture, $"{changes.Count - NamedPods} more pods changed in this check"))
            : null;

    /// <summary>
    /// Azure's verdict on the AKS service or its power state changed between the last facts the page read and these.
    /// </summary>
    public static IReadOnlyList<DashboardEvent> Service(AksService before, AksService after, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(before);
        ArgumentNullException.ThrowIfNull(after);
        var events = new List<DashboardEvent>();
        if (new AzureState(before.Availability?.State) is { IsMissing: false } verdict && new AzureState(after.Availability?.State) is { IsMissing: false } now && !verdict.IsSameAs(now))
        {
            var level = now switch
            {
                _ when now.Is(AksAvailability.Available) => EventLevel.Good,
                _ when now.Is(AksAvailability.Degraded) => EventLevel.Warning,
                _ when now.Is(AksAvailability.Unavailable) => EventLevel.Problem,
                _ => EventLevel.Info,
            };
            events.Add(new DashboardEvent(at, EventKind.Cluster, level, null, ServicePlace, $"Azure's verdict on the AKS service: {verdict} → {now}"));
        }

        if (new AzureState(before.PowerState) is { IsMissing: false } power && new AzureState(after.PowerState) is { IsMissing: false } current && !power.IsSameAs(current))
        {
            events.Add(new DashboardEvent(
                at,
                EventKind.Cluster,
                after.IsRunning ? EventLevel.Good : EventLevel.Info,
                null,
                ServicePlace,
                $"The power state of the AKS service: {power} → {current}"));
        }

        return events;
    }

    /// <summary>The one event of a pod in a round, by what matters most: a restart, then unhealthy, then ready again.</summary>
    private static ClusterNotice? Pod(ClusterPod? before, PodState? was, ClusterPod after, PodState now, string space, DateTimeOffset asOf)
    {
        var name = ClusterText.PodIn(after, space);
        if (before is not null && after.Restarts > before.Restarts)
        {
            var reason = now != PodState.Ready ? ClusterText.Reason(after.Reason) : string.Empty;
            return new ClusterNotice(EventLevel.Warning, $"{name} restarted ({ClusterText.Restarts(after.Restarts)}){reason}");
        }

        if (now == PodState.Unhealthy && was != PodState.Unhealthy)
        {
            var words = ClusterText.PodWords(after, now, asOf);
            return new ClusterNotice(EventLevel.Warning, $"{name} is unhealthy: {(new PodPhase(after.Phase).Is(PodPhaseKind.Running) && after.Reason is null ? "not ready" : words)}");
        }

        return was == PodState.Unhealthy && now == PodState.Ready ? new ClusterNotice(EventLevel.Good, $"{name} is ready again") : null;
    }
}
