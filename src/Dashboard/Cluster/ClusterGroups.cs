using Dashboard.Health;

namespace Dashboard.Cluster;

/// <summary>One row of a namespace's table: a pod and what the view says about it.</summary>
public sealed record PodRow(ClusterPod Pod, PodState State);

/// <summary>
/// The rows of a namespace's table counted in one walk: how many are in each state, the restarts they carry and the
/// CPU and memory they use. Every count and sum of a <see cref="NamespaceGroup" /> is read off one of these, so the
/// four counts come from one <c>switch</c> and cannot fall out of step with one another.
/// </summary>
public sealed class PodTally
{
    private readonly int restarts;
    private readonly bool restartsPastAnInt;

    /// <summary>Walks <paramref name="rows" /> once.</summary>
    public PodTally(IEnumerable<PodRow> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        foreach (var row in rows)
        {
            Rows++;
            switch (row.State)
            {
                case PodState.Ready: Ready++; break;
                case PodState.Starting: Starting++; break;
                case PodState.Unhealthy: Unhealthy++; break;
                case PodState.Finished: Finished++; break;
            }

            // A checked sum: past an int on the way is past it for good, as Enumerable.Sum would throw there.
            if (!restartsPastAnInt)
            {
                var sum = (long)restarts + row.Pod.Restarts;
                restartsPastAnInt = sum is > int.MaxValue or < int.MinValue;
                restarts = (int)sum;
            }

            CpuUsage = Add(CpuUsage, row.Pod.Cpu.Usage);
            MemoryUsage = Add(MemoryUsage, row.Pod.Memory.Usage);
        }
    }

    /// <summary>Every row, whatever its state.</summary>
    public int Rows { get; }

    public int Ready { get; }

    public int Starting { get; }

    public int Unhealthy { get; }

    /// <summary>The finished jobs: counted on their own, and in no <see cref="Total" />.</summary>
    public int Finished { get; }

    /// <summary>The pods that count: every pod but the finished ones.</summary>
    public int Total => Rows - Finished;

    /// <summary>The restarts of every row, counted in an <see cref="int" />.</summary>
    /// <exception cref="OverflowException">The count went past an <see cref="int" /> on the way.</exception>
    public int Restarts => restartsPastAnInt
        ? throw new OverflowException("The restarts of the rows add up to more than an int holds.")
        : restarts;

    /// <summary>The CPU the rows use, in millicores; null when none reports a usage.</summary>
    public double? CpuUsage { get; }

    /// <summary>The memory the rows use, in bytes; null when none reports a usage.</summary>
    public double? MemoryUsage { get; }

    /// <summary>The sum <see cref="NamespaceGroup.Sum" /> gives, kept one number at a time: the known numbers added
    /// in order from zero, and null until one is known.</summary>
    private static double? Add(double? sum, double? number) =>
        number is { } known ? (sum ?? 0d) + known : sum;
}

/// <summary>A namespace as the view shows it:its pods in the order of the table, its volumes and its sums.</summary>
/// <param name="Environment">The environment of the topology whose pods the namespace holds; null for a namespace of the platform.</param>
/// <param name="Found">False for an environment's namespace that the status file does not list.</param>
/// <param name="Pods">The pods that are not ready and not finished first, then the ready ones, then the finished; each by name.</param>
public sealed record NamespaceGroup(
    string Name,
    EnvironmentInfo? Environment,
    bool Found,
    IReadOnlyList<PodRow> Pods,
    IReadOnlyList<ClusterVolume> Volumes)
{
    // Each member below counts the rows afresh, in one walk, rather than keeping a count: the group is a record whose
    // equality, clones and shared lists the tests pin, and a kept count would take part in all three. A reader that
    // wants several numbers builds one PodTally of the Pods and reads them all off it (ClusterText.Summary does).

    /// <summary>The pods that are ready.</summary>
    public int Ready => new PodTally(Pods).Ready;

    /// <summary>The pods that count: every pod but the finished ones.</summary>
    public int Total => new PodTally(Pods).Total;

    /// <summary>The finished jobs: counted on their own, in neither number above.</summary>
    public int Finished => new PodTally(Pods).Finished;

    public int Unhealthy => new PodTally(Pods).Unhealthy;

    public int Restarts => new PodTally(Pods).Restarts;

    /// <summary>The CPU its pods use, in millicores; null when none reports a usage.</summary>
    public double? CpuUsage => new PodTally(Pods).CpuUsage;

    /// <summary>The memory its pods use, in bytes; null when none reports a usage.</summary>
    public double? MemoryUsage => new PodTally(Pods).MemoryUsage;

    internal static NamespaceGroup Of(ClusterNamespace space, EnvironmentInfo? environment, DateTimeOffset asOf) =>
        new(
            space.Name,
            environment,
            true,
            [.. space.Pods
                .Select(pod => new PodRow(pod, PodRules.StateOf(pod, asOf)))
                .OrderBy(row => Order(row.State))
                .ThenBy(row => row.Pod.Label, StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row.Pod.Name, StringComparer.OrdinalIgnoreCase)],
            space.Volumes);

    internal static double? Sum(IEnumerable<double?> numbers)
    {
        var known = numbers.OfType<double>().ToList();
        return known.Count == 0 ? null : known.Sum();
    }

    private static int Order(PodState state) => state switch
    {
        PodState.Unhealthy => 0,
        PodState.Starting => 1,
        PodState.Ready => 2,
        _ => 3,
    };
}

/// <summary>
/// The namespaces of the cluster in the order of the view: first those of the topology's environments, in the
/// topology's order, then every other namespace by name, as the platform.
/// </summary>
public sealed record ClusterGroups(IReadOnlyList<NamespaceGroup> Environments, IReadOnlyList<NamespaceGroup> Platform)
{
    /// <summary>True when a pod of the platform is unhealthy: its group is then shown open.</summary>
    public bool PlatformNeedsALook => Platform.Any(space => space.Unhealthy > 0);

    /// <param name="asOf">The moment the facts are of (<see cref="ClusterStatus.AsOf"/>).</param>
    public static ClusterGroups Of(ClusterStatus status, IEnumerable<EnvironmentInfo> environments, DateTimeOffset asOf)
    {
        ArgumentNullException.ThrowIfNull(status);
        ArgumentNullException.ThrowIfNull(environments);
        var index = EnvironmentIndex.Of(environments);
        return new ClusterGroups(
            [.. index.Named.Select(environment =>
                status.Namespaces.FirstOrDefault(space => string.Equals(space.Name, environment.Namespace, StringComparison.Ordinal)) is { } space
                    ? NamespaceGroup.Of(space, environment, asOf)
                    : new NamespaceGroup(environment.Namespace!, environment, false, [], []))],
            [.. status.Namespaces
                .Where(space => !index.Namespaces.Contains(space.Name))
                .OrderBy(space => space.Name, StringComparer.OrdinalIgnoreCase)
                .Select(space => NamespaceGroup.Of(space, null, asOf))]);
    }
}

/// <summary>The sums of the cluster card.</summary>
/// <param name="PodsReady">The pods that are ready.</param>
/// <param name="Pods">The pods that count: every pod but the finished ones.</param>
/// <param name="PodsFinished">The finished jobs.</param>
/// <param name="Cpu">Millicores over all nodes: used, requested and what the nodes offer.</param>
/// <param name="Memory">Bytes over all nodes.</param>
/// <param name="PodsOnNodes">The pods the nodes run, as the nodes count them.</param>
/// <param name="PodCapacity">How many the nodes can take.</param>
public sealed record ClusterTotals(
    int NodesReady,
    int Nodes,
    int PodsReady,
    int Pods,
    int PodsFinished,
    ResourceUse Cpu,
    ResourceUse Memory,
    int? PodsOnNodes,
    int? PodCapacity,
    int Restarts)
{
    /// <param name="asOf">The moment the facts are of (<see cref="ClusterStatus.AsOf"/>).</param>
    public static ClusterTotals Of(ClusterStatus status, DateTimeOffset asOf)
    {
        ArgumentNullException.ThrowIfNull(status);
        var states = status.Pods.Select(entry => PodRules.StateOf(entry.Pod, asOf)).ToList();
        return new ClusterTotals(
            status.Nodes.Count(node => node.Ready),
            status.Nodes.Count,
            states.Count(state => state == PodState.Ready),
            states.Count(state => state != PodState.Finished),
            states.Count(state => state == PodState.Finished),
            Sum(status.Nodes.Select(node => node.Cpu)),
            Sum(status.Nodes.Select(node => node.Memory)),
            (int?)NamespaceGroup.Sum(status.Nodes.Select(node => (double?)node.Pods)),
            (int?)NamespaceGroup.Sum(status.Nodes.Select(node => (double?)node.PodCapacity)),
            status.Pods.Sum(entry => entry.Pod.Restarts));
    }

    private static ResourceUse Sum(IEnumerable<ResourceUse> uses)
    {
        var all = uses.ToList();
        return new ResourceUse(
            NamespaceGroup.Sum(all.Select(use => use.Usage)),
            NamespaceGroup.Sum(all.Select(use => use.Requests)),
            NamespaceGroup.Sum(all.Select(use => use.Limits)),
            NamespaceGroup.Sum(all.Select(use => use.Allocatable)));
    }
}

/// <summary>What the page keeps of one reading of the cluster for its trends: the CPU and the memory the nodes use.</summary>
/// <param name="CpuMillicores">Millicores used over all nodes; null when no node reports a usage.</param>
/// <param name="MemoryBytes">Bytes used over all nodes.</param>
public sealed record ClusterSample(double? CpuMillicores, double? MemoryBytes)
{
    public static ClusterSample Of(ClusterStatus status)
    {
        ArgumentNullException.ThrowIfNull(status);
        return new ClusterSample(
            NamespaceGroup.Sum(status.Nodes.Select(node => node.Cpu.Usage)),
            NamespaceGroup.Sum(status.Nodes.Select(node => node.Memory.Usage)));
    }
}
