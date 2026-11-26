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
        new(name, ready, pressures, false, "system", "Standard_D4as_v6", null, "v1.33.3", Generated.AddDays(-3), new ResourceUse(812, 2450, 9100, 3860), new ResourceUse(9876543210, 6200000000, 11800000000, 13400000000), 63, 110);

    /// <summary>
    /// The pod the tests build, named by the parts a pod is named by: its workload (and the name it carries),
    /// the word its phase stands on, its restarts and its reason. It is ready and it started an hour before
    /// <see cref="Generated"/>, and nothing about it is said by a flag: the readiness and the start a test
    /// wants otherwise are the pod-level steps of <see cref="PodSteps"/> - <c>NotReady()</c>,
    /// <c>Started(age)</c>, <c>StartedAt(moment)</c> - which a pod carries like the parts they set.
    /// </summary>
    /// <param name="workload">The workload the pod belongs to; the pod is named <c>{workload}-0</c> without a name.</param>
    /// <param name="phase">The word the pod's phase stands on; <see cref="ClusterPod.Running"/> without one.</param>
    /// <param name="restarts">How often its containers were started again.</param>
    /// <param name="reason">Why a container that is not ready waits or ended; null without one.</param>
    /// <param name="name">The name the pod carries; <c>{workload}-0</c> without one.</param>
    public static ClusterPod Pod(
        string workload,
        string phase = ClusterPod.Running,
        int restarts = 0,
        string? reason = null,
        string? name = null) =>
        new(
            name ?? $"{workload}-0",
            workload,
            "Deployment",
            "aks-system-12148983-vmss000000",
            phase,
            true,
            1,
            1,
            restarts,
            reason,
            Generated - TimeSpan.FromHours(1),
            new ResourceUse(10, 100, 500),
            new ResourceUse(100 * 1024 * 1024, 268435456, 536870912));

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

/// <summary>
/// The steps a test takes on a pod the fixture built, said on the pod rather than as the flags a factory used
/// to gate: the readiness the containers stand in and the moment the pod started. Each step is the copy of the
/// pod with the one thing it says, so the words a pod is said in and the parts it stands on cannot be told apart
/// by a flag that stands for another.
/// </summary>
internal static class PodSteps
{
    /// <summary>The pod is ready: its one container is, which is what the readiness stands in for.</summary>
    public static ClusterPod Ready(this ClusterPod pod) => pod with { Ready = true, ContainersReady = 1 };

    /// <summary>The pod is not ready: no container is, which is what the readiness stands in for.</summary>
    public static ClusterPod NotReady(this ClusterPod pod) => pod with { Ready = false, ContainersReady = 0 };

    /// <summary>
    /// The pod started <paramref name="age"/> before <see cref="ClusterFixture.Generated"/>, the moment the facts
    /// are of: a negative age is a start after it, the pod the clock has not reached.
    /// </summary>
    public static ClusterPod Started(this ClusterPod pod, TimeSpan age) =>
        pod with { StartedAt = ClusterFixture.Generated - age };

    /// <summary>The moment the pod started, said outright - or the nothing it is not said with.</summary>
    public static ClusterPod StartedAt(this ClusterPod pod, DateTimeOffset? moment) => pod with { StartedAt = moment };
}
