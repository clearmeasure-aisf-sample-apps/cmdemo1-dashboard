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

    /// <summary>The arrow between what was and what is now in a change of the AKS service: <c>Available → Degraded</c>.</summary>
    private const string Arrow = " → ";

    /// <summary>The words the change of the AKS service starts with: <c>the AKS service: </c>.</summary>
    private const string ServiceLead = "the AKS service: ";

    /// <summary>The words a node or a pod is said with when it is ready again after not being: <c> is ready again</c>.</summary>
    private const string ReadyAgain = " is ready again";

    /// <summary>The words a running pod is said in when it is not ready and names no reason: <c>not ready</c>.</summary>
    private const string NotReady = "not ready";

    /// <summary>The words a liveness of the cluster's status file starts with: <c>The cluster's status file</c>.</summary>
    private const string StatusFile = "The cluster's status file";

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
            ClusterLiveness.Silent => new ClusterNotice(EventLevel.Problem, $"{StatusFile} stopped answering{reason}"),
            ClusterLiveness.Unreadable => new ClusterNotice(EventLevel.Problem, $"{StatusFile} could not be read{reason}"),
            ClusterLiveness.Stale => new ClusterNotice(EventLevel.Problem, "The collector in the cluster stopped writing: the status file is stale"),
            _ when before == ClusterLiveness.Stale => new ClusterNotice(EventLevel.Good, "The collector in the cluster writes again"),
            _ => new ClusterNotice(EventLevel.Good, $"{StatusFile} answers again"),
        };
        return ClusterEvent(at, notice);
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
        var index = EnvironmentIndex.Of(environments);
        var events = new List<DashboardEvent>(NodeChanges(before, after, at));
        var changes = PodChanges(before, after, index.NameForNamespace, at);
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
                var shortName = ClusterText.ShortNode(node.Name);
                yield return node.Ready
                    ? ClusterEvent(at, new ClusterNotice(EventLevel.Good, $"Node {shortName}{ReadyAgain}"))
                    : ClusterEvent(at, new ClusterNotice(EventLevel.Problem, $"Node {shortName} is {NotReady}"));
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
                changes.Add(ClusterEvent(at, notice, environment: names.GetValueOrDefault(space.Name)));
            }
        }

        return changes;
    }

    /// <summary>The pod changes named one by one: the first <see cref="NamedPods"/>, counted over the pods alone - nodes never count against them.</summary>
    private static IEnumerable<DashboardEvent> NamedPodChanges(List<DashboardEvent> changes) => changes.Take(NamedPods);

    /// <summary>The one event that counts the pod changes past the <see cref="NamedPods"/> named; null when none is left over.</summary>
    private static DashboardEvent? Overflow(List<DashboardEvent> changes, DateTimeOffset at) =>
        changes.Count > NamedPods
            ? ClusterEvent(at, new ClusterNotice(EventLevel.Info, string.Create(CultureInfo.InvariantCulture, $"{changes.Count - NamedPods} more pods changed in this check")))
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
            events.Add(ServiceEvent(at, new ClusterNotice(level, $"Azure's verdict on {ServiceLead}{verdict}{Arrow}{now}")));
        }

        if (new AzureState(before.PowerState) is { IsMissing: false } power && new AzureState(after.PowerState) is { IsMissing: false } current && !power.IsSameAs(current))
        {
            events.Add(ServiceEvent(at, new ClusterNotice(after.IsRunning ? EventLevel.Good : EventLevel.Info, $"The power state of {ServiceLead}{power}{Arrow}{current}")));
        }

        return events;
    }

    /// <summary>An event of the cluster as a whole, in the words of its notice; only a pod's change names an environment.</summary>
    private static DashboardEvent ClusterEvent(DateTimeOffset at, ClusterNotice notice, string? environment = null) =>
        new(at, EventKind.Cluster, notice.Level, environment, ClusterPlace, notice.Text);

    /// <summary>An event of the AKS service, in the words of its notice: it belongs to no environment.</summary>
    private static DashboardEvent ServiceEvent(DateTimeOffset at, ClusterNotice notice) =>
        new(at, EventKind.Cluster, notice.Level, null, ServicePlace, notice.Text);

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
            return new ClusterNotice(EventLevel.Warning, $"{name} is unhealthy: {(new PodPhase(after.Phase).Is(PodPhaseKind.Running) && after.Reason is null ? NotReady : words)}");
        }

        return was == PodState.Unhealthy && now == PodState.Ready ? new ClusterNotice(EventLevel.Good, $"{name}{ReadyAgain}") : null;
    }
}
