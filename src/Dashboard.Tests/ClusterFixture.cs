namespace Dashboard.Tests;

/// <summary>The cluster view's two sample files, and the parts of a status the tests build their own from.</summary>
internal static class ClusterFixture
{
    /// <summary>The moment the sample <c>cluster.json</c> was written.</summary>
    public static readonly DateTimeOffset Generated = new(2026, 10, 6, 20, 15, 30, TimeSpan.Zero);

    public static string Sample(string file) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "cluster.sample", file));

    public static ClusterStatus SampleStatus => ClusterStatus.Parse(Sample("cluster.json")).Value!;

    public static AksService SampleService => AksService.Parse(Sample("aks.json")).Value!;

    public static ClusterNode Node(string name = "aks-system-12148983-vmss000000", bool ready = true, params string[] pressures) =>
        new(name, ready, pressures, false, "system", "Standard_D4as_v6", null, "v1.33.3", Generated.AddDays(-3), new ResourceUse(812, 2450, 9100, 3860), new ResourceUse(9876543210, 6200000000, 11800000000, 13400000000), 63, 110);

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
            name ?? $"{workload}-0",
            workload,
            "Deployment",
            "aks-system-12148983-vmss000000",
            phase,
            ready,
            1,
            ready ? 1 : 0,
            restarts,
            reason,
            started ? Generated - (age ?? TimeSpan.FromHours(1)) : null,
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

    public static ClusterStatus Status(IReadOnlyList<ClusterNode>? nodes = null, params ClusterNamespace[] spaces) =>
        new(Generated, 15, "v1.33.3", nodes ?? [Node()], spaces);

    public static SourceReading<ClusterStatus> Read(ClusterStatus status) => new(SourceState.Read, status);

    public static SourceReading<AksService> Read(AksService service) => new(SourceState.Read, service);

    public static EnvironmentInfo Environment(string name, string? space, string? tier = "nonprod") =>
        new(name, tier, [], Namespace: space);
}
