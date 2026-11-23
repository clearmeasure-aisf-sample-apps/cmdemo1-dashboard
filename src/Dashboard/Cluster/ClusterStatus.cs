using System.Text.Json;
using Dashboard.Health;

namespace Dashboard.Cluster;

/// <summary>What a file's content held: the facts, or why there are none (<paramref name="Problem"/>, the end of a sentence).</summary>
public sealed record Parsed<T>(T? Value, string? Problem = null)
    where T : class;

/// <summary>
/// The live status of the cluster: the content of the file at <c>cluster.statusUrl</c> of the topology
/// (<c>cluster.json</c>), which a collector inside the cluster writes every <see cref="IntervalSeconds"/>. CPU is in
/// millicores, memory and the capacity of a volume in bytes. Every part but the names is optional.
/// </summary>
/// <param name="Generated">When the collector wrote the file: the facts are as of then.</param>
/// <param name="IntervalSeconds">How often the collector writes.</param>
/// <param name="KubernetesVersion">The version of the cluster's API server.</param>
public sealed record ClusterStatus(
    DateTimeOffset? Generated,
    int? IntervalSeconds,
    string? KubernetesVersion,
    IReadOnlyList<ClusterNode> Nodes,
    IReadOnlyList<ClusterNamespace> Namespaces)
{
    /// <summary>A file is stale when it is older than this many of the collector's intervals.</summary>
    public const int StaleIntervals = 4;

    /// <summary>A file younger than this is never stale, however often the collector writes.</summary>
    public static readonly TimeSpan MinimumStaleAfter = TimeSpan.FromSeconds(60);

    /// <summary>The age from which the file is stale: four intervals of the collector, and a minute at least.</summary>
    public TimeSpan StaleAfter
    {
        get
        {
            var intervals = TimeSpan.FromSeconds((double)StaleIntervals * (IntervalSeconds ?? 0));
            return intervals > MinimumStaleAfter ? intervals : MinimumStaleAfter;
        }
    }

    /// <summary>Every pod with its namespace, in the order of the file.</summary>
    public IEnumerable<(ClusterNamespace Namespace, ClusterPod Pod)> Pods =>
        from space in Namespaces
        from pod in space.Pods
        select (space, pod);

    /// <summary>True when the collector wrote the file longer ago than <see cref="StaleAfter"/>. A file that does not say when is not stale.</summary>
    public bool IsStale(DateTimeOffset now) => Generated is { } generated && now - generated > StaleAfter;

    /// <summary>The moment the facts are of: ages and the rules about them are counted from it. The page's clock for a file that does not say.</summary>
    public DateTimeOffset AsOf(DateTimeOffset now) => Generated ?? now;

    /// <summary>
    /// Reads the file. It must be a JSON object with <c>nodes</c> or <c>namespaces</c>; a node, a namespace, a pod or
    /// a volume without a name is left out, and unknown fields are ignored.
    /// </summary>
    public static Parsed<ClusterStatus> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Parsed<ClusterStatus>(null, "the file is empty");
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return new Parsed<ClusterStatus>(null, "the file does not contain a JSON object");
            }

            if (!HasArray(root, "nodes") && !HasArray(root, "namespaces"))
            {
                return new Parsed<ClusterStatus>(null, "the file has neither nodes nor namespaces");
            }

            return new Parsed<ClusterStatus>(new ClusterStatus(
                JsonRead.Time(root, "generated"),
                JsonRead.Count(root, "intervalSeconds"),
                JsonRead.Text(root, "kubernetesVersion"),
                ReadNamed(root, "nodes", ClusterNode.Read),
                ReadNamed(root, "namespaces", ClusterNamespace.Read)));
        }
        catch (JsonException)
        {
            return new Parsed<ClusterStatus>(null, "the file is not valid JSON");
        }
    }

    /// <summary>
    /// The rows of the array under <paramref name="name"/>, each made by <paramref name="read"/> from its name and its
    /// object. The one place of the rule that a row without a name is left out: a row type gets it by construction.
    /// </summary>
    internal static IReadOnlyList<T> ReadNamed<T>(JsonElement? parent, string name, Func<string, JsonElement, T> read)
        where T : class =>
        [.. JsonRead.Items(parent, name).Select(item => JsonRead.Identified(item, RowName, read)).OfType<T>()];

    /// <summary>The field that names a node, a namespace, a pod or a volume: the one part without which there is no row.</summary>
    private const string RowName = "name";

    private static bool HasArray(JsonElement root, string name) =>
        root.TryGetProperty(name, out var array) && array.ValueKind == JsonValueKind.Array;
}

/// <summary>One resource (CPU in millicores, or memory in bytes) of a node or a pod. Each number may be absent.</summary>
/// <param name="Usage">What is used now; null while the cluster has no measurement (no metrics yet, a finished pod).</param>
/// <param name="Requests">What the pods ask for.</param>
/// <param name="Limits">What the pods may use at most; null when none is set.</param>
/// <param name="Allocatable">A node only: what it offers to pods.</param>
public sealed record ResourceUse(double? Usage, double? Requests, double? Limits, double? Allocatable = null)
{
    public static ResourceUse None { get; } = new(null, null, null);

    internal static ResourceUse Read(JsonElement? section) =>
        section is null
            ? None
            : new ResourceUse(
                JsonRead.Number(section, "usage"),
                JsonRead.Number(section, "requests"),
                JsonRead.Number(section, "limits"),
                JsonRead.Number(section, "allocatable"));
}

/// <summary>One node of the cluster.</summary>
/// <param name="Ready">The node's Ready condition; false when the file does not say.</param>
/// <param name="Pressures">
/// The conditions that are true among MemoryPressure, DiskPressure, PIDPressure and NetworkUnavailable.
/// </param>
/// <param name="Unschedulable">The node is cordoned: it takes no new pod.</param>
/// <param name="Pool">The node pool it belongs to.</param>
/// <param name="Size">The size of its virtual machine.</param>
/// <param name="Zone">Its availability zone; null where the region or the pool has none.</param>
/// <param name="Pods">How many pods run on it.</param>
/// <param name="PodCapacity">How many it can take.</param>
public sealed record ClusterNode(
    string Name,
    bool Ready,
    IReadOnlyList<string> Pressures,
    bool Unschedulable,
    string? Pool,
    string? Size,
    string? Zone,
    string? KubeletVersion,
    DateTimeOffset? CreatedAt,
    ResourceUse Cpu,
    ResourceUse Memory,
    int? Pods,
    int? PodCapacity)
{
    /// <summary>Ready and under no pressure. A cordoned node is still a healthy one.</summary>
    public bool IsHealthy => Ready && Pressures.Count == 0;

    internal static ClusterNode Read(string name, JsonElement element)
    {
        var pods = JsonRead.Section(element, "pods");
        return new ClusterNode(
            name,
            JsonRead.Flag(element, "ready") ?? false,
            [.. JsonRead.Items(element, "pressures")
                .Where(pressure => pressure.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(pressure.GetString()))
                .Select(pressure => pressure.GetString()!.Trim())],
            JsonRead.Flag(element, "unschedulable") ?? false,
            JsonRead.Text(element, "pool"),
            JsonRead.Text(element, "size"),
            JsonRead.Text(element, "zone"),
            JsonRead.Text(element, "kubeletVersion"),
            JsonRead.Time(element, "createdAt"),
            ResourceUse.Read(JsonRead.Section(element, "cpu")),
            ResourceUse.Read(JsonRead.Section(element, "memory")),
            JsonRead.Count(pods, "count"),
            JsonRead.Count(pods, "capacity"));
    }
}

/// <summary>One namespace with its pods and its volume claims.</summary>
public sealed record ClusterNamespace(string Name, IReadOnlyList<ClusterPod> Pods, IReadOnlyList<ClusterVolume> Volumes)
{
    internal static ClusterNamespace Read(string name, JsonElement element) =>
        new(
            name,
            ClusterStatus.ReadNamed(element, "pods", ClusterPod.Read),
            ClusterStatus.ReadNamed(element, "volumes", ClusterVolume.Read));
}

/// <summary>One pod.</summary>
/// <param name="Workload">The name of the workload that owns it; null for a pod of its own.</param>
/// <param name="Kind">The kind of that workload: Deployment, StatefulSet, DaemonSet, Job or Pod.</param>
/// <param name="Node">The node it runs on.</param>
/// <param name="Phase">The pod phase of Kubernetes: Pending, Running, Succeeded, Failed or Unknown (also when the file does not say).</param>
/// <param name="Ready">Every container is ready; false when the file does not say.</param>
/// <param name="Restarts">How often its containers were started again.</param>
/// <param name="Reason">
/// Why a container that is not ready waits or ended, or the pod's own reason: CrashLoopBackOff, ImagePullBackOff,
/// Evicted, Completed. Null without one.
/// </param>
/// <param name="StartedAt">When the pod started: its age is counted from it.</param>
public sealed record ClusterPod(
    string Name,
    string? Workload,
    string? Kind,
    string? Node,
    string Phase,
    bool Ready,
    int? Containers,
    int? ContainersReady,
    int Restarts,
    string? Reason,
    DateTimeOffset? StartedAt,
    ResourceUse Cpu,
    ResourceUse Memory)
{
    public const string Pending = "Pending";
    public const string Running = "Running";
    public const string Succeeded = "Succeeded";
    public const string Failed = "Failed";
    public const string Unknown = "Unknown";

    /// <summary>What the page calls the pod: its workload, or its own name without one.</summary>
    public string Label => Workload ?? Name;

    internal static ClusterPod Read(string name, JsonElement element) =>
        new(
            name,
            JsonRead.Text(element, "workload"),
            JsonRead.Text(element, "kind"),
            JsonRead.Text(element, "node"),
            JsonRead.Text(element, "phase") ?? Unknown,
            JsonRead.Flag(element, "ready") ?? false,
            JsonRead.Count(element, "containers"),
            JsonRead.Count(element, "containersReady"),
            JsonRead.Count(element, "restarts") ?? 0,
            JsonRead.Text(element, "reason"),
            JsonRead.Time(element, "startedAt"),
            ResourceUse.Read(JsonRead.Section(element, "cpu")),
            ResourceUse.Read(JsonRead.Section(element, "memory")));
}

/// <summary>One volume claim of a namespace.</summary>
/// <param name="Capacity">Its size in bytes.</param>
/// <param name="Phase">Bound, Pending or Lost.</param>
public sealed record ClusterVolume(string Name, double? Capacity, string? Phase)
{
    internal static ClusterVolume Read(string name, JsonElement element) =>
        new(name, JsonRead.Number(element, "capacity"), JsonRead.Text(element, "phase"));
}

/// <summary>The pod phases of Kubernetes the page tells apart; <see cref="Other"/> for any other word.</summary>
public enum PodPhaseKind
{
    /// <summary>A word the page does not know: neither of the five below.</summary>
    Other,

    Pending,

    Running,

    Succeeded,

    Failed,

    /// <summary>Kubernetes' own Unknown, which is also the phase of a pod whose file does not say.</summary>
    Unknown,
}

/// <summary>
/// A pod's phase as a value the page asks about instead of a string it compares: parsed once, in the exact case
/// Kubernetes writes it (<c>running</c> is a word the page does not know), and keeping the word as the file held it
/// for the label the page shows.
/// </summary>
/// <param name="Text">The word as the file held it.</param>
public readonly record struct PodPhase(string Text)
{
    /// <summary>The word as the file held it.</summary>
    public string Text { get; } = Text;

    /// <summary>Which of the phases the page knows the word is; <see cref="PodPhaseKind.Other"/> for any other.</summary>
    public PodPhaseKind Kind { get; } = Parse(Text);

    /// <summary>True for a word the page does not know, and for Kubernetes' own Unknown.</summary>
    public bool IsUnknown => Kind is PodPhaseKind.Other or PodPhaseKind.Unknown;

    /// <summary>True when the phase is <paramref name="kind"/>.</summary>
    public bool Is(PodPhaseKind kind) => Kind == kind;

    public override string ToString() => Text;

    private static PodPhaseKind Parse(string? text) => text switch
    {
        ClusterPod.Pending => PodPhaseKind.Pending,
        ClusterPod.Running => PodPhaseKind.Running,
        ClusterPod.Succeeded => PodPhaseKind.Succeeded,
        ClusterPod.Failed => PodPhaseKind.Failed,
        ClusterPod.Unknown => PodPhaseKind.Unknown,
        _ => PodPhaseKind.Other,
    };
}
