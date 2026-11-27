namespace Dashboard.Tests;

/// <summary>The cluster view's two sample files, and the parts of a status the tests build their own from.</summary>
internal static class ClusterFixture
{
    /// <summary>The moment the sample <c>cluster.json</c> was written.</summary>
    public static readonly DateTimeOffset Generated = new(2026, 10, 6, 20, 15, 30, TimeSpan.Zero);

    /// <summary>The moment the tests ask at: twenty seconds after <see cref="Generated"/>, a status still live.</summary>
    public static readonly DateTimeOffset Now = Generated.AddSeconds(20);

    /// <summary>The one zone the tests ask the words in, so no test leans on the machine's own.</summary>
    public static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    public static string Sample(string file) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "cluster.sample", file));

    public static ClusterStatus SampleStatus => ClusterStatus.Parse(Sample("cluster.json")).Value!;

    public static AksService SampleService => AksService.Parse(Sample("aks.json")).Value!;

    public static ClusterNode Node(string name = "aks-system-12148983-vmss000000", bool ready = true, params string[] pressures) =>
        new(
            Name: name,
            Ready: ready,
            Pressures: pressures,
            Unschedulable: false,
            Pool: "system",
            Size: "Standard_D4as_v6",
            Zone: null,
            KubeletVersion: "v1.33.3",
            CreatedAt: Generated.AddDays(-3),
            Cpu: ResourceUses.Node(usage: 812, requests: 2450, allocatable: 3860, limits: 9100),
            Memory: ResourceUses.Node(usage: 9876543210, requests: 6200000000, allocatable: 13400000000, limits: 11800000000),
            Pods: 63,
            PodCapacity: 110);

    /// <param name="age">How long before <see cref="Generated"/> the pod started; null for a pod that does not say.</param>
    public static ClusterPod Pod(
        string workload,
        string phase = ClusterPod.Running,
        bool ready = true,
        int restarts = 0,
        string? reason = null,
        TimeSpan? age = null,
        string? name = null,
        bool started = true) =>
        new(
            Name: name ?? $"{workload}-0",
            Workload: workload,
            Kind: "Deployment",
            Node: "aks-system-12148983-vmss000000",
            Phase: phase,
            Ready: ready,
            Containers: 1,
            ContainersReady: ready ? 1 : 0,
            Restarts: restarts,
            Reason: reason,
            StartedAt: started ? Generated - (age ?? TimeSpan.FromHours(1)) : null,
            Cpu: ResourceUses.Pod(usage: 10, requests: 100, limit: 500),
            Memory: ResourceUses.Pod(usage: 100 * 1024 * 1024, requests: 268435456, limit: 536870912));

    /// <summary>
    /// One row of a namespace's table: the fixture's <see cref="Pod" />, on the state the test decides. The state is
    /// handed to the row rather than worked out of the pod, so no row here needs a moment to be counted at. What the
    /// pod reports of itself is <see cref="Pod" />'s own, asked of it once rather than written again, unless a restart
    /// count or a usage is asked for - and <see cref="ResourceUse.None" /> asks for a resource the pod says nothing about.
    /// </summary>
    public static PodRow Row(
        string workload,
        PodState state,
        int restarts = 0,
        ResourceUse? cpu = null,
        ResourceUse? memory = null)
    {
        var pod = Pod(workload, restarts: restarts);
        return new PodRow(pod with { Cpu = cpu ?? pod.Cpu, Memory = memory ?? pod.Memory }, state);
    }

    /// <summary>
    /// A pod that says as little as the record allows: no workload, no kind, no node, no containers, no reason, no
    /// start and no measurement, so the only thing it brings is its name and the word its phase stands on. The one
    /// home of that pod, said with every argument named so a swap of two of the same type shows as the swap it is - the
    /// workload, the kind and the node are three words in a row and the cpu and the memory are two resources in a row.
    /// <para>
    /// The phase is the one part a caller names, because the two all-nothing pods the tests stand on are apart there:
    /// the row of a pod that says nothing holds the phase the pod is said to be in, <see cref="ClusterPod.Running"/>,
    /// while the pod a file barely names lands on the fallback the parser writes, <see cref="ClusterPod.Unknown"/>.
    /// Everything else is the nothing it is: the readiness the false that is no nothing, the restarts the zero that is
    /// no nothing, and both resources the resource that says no number at all.
    /// </para>
    /// </summary>
    /// <param name="name">The name the pod carries; the pod having no workload, it is the word the page calls it by.</param>
    /// <param name="phase">The word the phase is said in; <see cref="ClusterPod.Running"/> unless the caller stands on the parser's own fallback.</param>
    public static ClusterPod SilentPod(string name, string phase = ClusterPod.Running) =>
        new(
            Name: name,
            Workload: null,
            Kind: null,
            Node: null,
            Phase: phase,
            Ready: false,
            Containers: null,
            ContainersReady: null,
            Restarts: 0,
            Reason: null,
            StartedAt: null,
            Cpu: ResourceUse.None,
            Memory: ResourceUse.None);

    /// <summary>Eight GiB in the bytes the capacity stands in - the size the sample file's one volume claims.</summary>
    public const double EightGiB = 8589934592d;

    public static ClusterVolume Volume(string name = "data", double? capacity = EightGiB, string? phase = "Bound") =>
        new(name, capacity, phase);

    public static ClusterNamespace Space(string name, params ClusterPod[] pods) => new(name, pods, []);

    public static ClusterNamespace Space(string name, IReadOnlyList<ClusterVolume> volumes, params ClusterPod[] pods) => new(name, pods, volumes);

    public static ClusterStatus Status(IReadOnlyList<ClusterNode>? nodes = null, params ClusterNamespace[] spaces) =>
        new(Generated, 15, "v1.33.3", nodes ?? [Node()], spaces);

    public static SourceReading<ClusterStatus> Read(ClusterStatus status) => new(SourceState.Read, status);

    public static SourceReading<AksService> Read(AksService service) => new(SourceState.Read, service);

    /// <summary>A reading of the cluster's status that brought no status: the state it stands in and the reason it gives.</summary>
    public static SourceReading<ClusterStatus> Silent(SourceState state = SourceState.Unavailable, string? detail = "no answer within 10 s") =>
        new(state, Detail: detail);

    /// <summary>The service card's assessment of an AKS service read as it is.</summary>
    public static ClusterAssessment Service(AksService service) => ClusterAssessment.Service(Read(service));

    public static EnvironmentInfo Environment(string name, string? space, string? tier = "nonprod") =>
        new(name, tier, [], Namespace: space);
}
