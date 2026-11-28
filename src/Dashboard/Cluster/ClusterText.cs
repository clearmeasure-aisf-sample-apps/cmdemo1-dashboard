using System.Globalization;
using Dashboard.Health;

namespace Dashboard.Cluster;

/// <summary>The cluster's numbers and names in the words of the cluster view.</summary>
public static class ClusterText
{
    /// <summary>From this share of its limit on, a meter is marked as high.</summary>
    public const double HighPercent = 90;

    /// <summary>The low end of the meter's scale, in percent: nothing is said of a share below it.</summary>
    public const int MeterEmpty = 0;

    /// <summary>The high end of the meter's scale, in percent: a share past it fills the bar.</summary>
    public const int MeterFull = 100;

    /// <summary>
    /// Where the bar stands, as the whole number of its scale: the share clamped to the scale, rounded to a whole
    /// number, in the invariant culture. A share past either end stands on that end.
    /// </summary>
    public static string MeterValue(double percent) =>
        Math.Round(Math.Clamp(percent, MeterEmpty, MeterFull)).ToString(CultureInfo.InvariantCulture);

    /// <summary>The millicores in one core: the cluster reads CPU in millicores and says it in cores.</summary>
    internal const double MillicoresPerCore = 1000;

    /// <summary>The format of a whole number: a count of MiB or KiB, a percentage.</summary>
    private const string WholeFormat = "0";

    /// <summary>The format of a pod's CPU below one core: a whole number of millicores.</summary>
    private const string MillicoreFormat = WholeFormat;

    /// <summary>The unit a number of millicores is said with: <c>14 m</c>.</summary>
    private const string MillicoreUnit = "m";

    /// <summary>The format of a number of cores: at most two digits after the point.</summary>
    private const string CoresFormat = "0.##";

    /// <summary>The word a quantity of CPU is said in: <c>0.81 cores</c>, <c>1.5 cores</c>.</summary>
    internal const string CoresUnit = "cores";

    /// <summary>The word the quantity of exactly one core is said in: <c>1 core</c>.</summary>
    private const string CoreUnit = "core";

    /// <summary>The format of a number of GiB: at most one digit after the point.</summary>
    private const string GibibyteFormat = "0.#";

    /// <summary>The unit a number of GiB is said with: <c>9.2 GiB</c>.</summary>
    private const string GibibyteUnit = "GiB";

    /// <summary>The unit a number of MiB is said with: <c>258 MiB</c>.</summary>
    private const string MebibyteUnit = "MiB";

    /// <summary>The unit a number of KiB is said with: <c>512 KiB</c>.</summary>
    private const string KibibyteUnit = "KiB";

    /// <summary>The suffix a percentage is said with: <c>63 %</c>.</summary>
    private const string PercentSign = " %";

    /// <summary>The separator between the parts of one cluster line: <c>3 of 3 ready · 1 restart</c>.</summary>
    private const string PartSeparator = " · ";

    /// <summary>The separator between the words of a list: <c>Running, Succeeded</c>.</summary>
    private const string ListSeparator = ", ";

    /// <summary>The cell a cluster table shows when the cluster file does not give that fact.</summary>
    public const string NotGiven = "—";

    private const double Kibibyte = 1024;
    private const double Mebibyte = Kibibyte * 1024;

    /// <summary>The bytes in one GiB (1024³): the cluster reads memory in bytes and says it, from a GiB on, in GiB.</summary>
    internal const double Gibibyte = Mebibyte * 1024;

    /// <summary>
    /// The binary units memory is said in, largest first: a number of bytes is said in the first unit it is at least
    /// one of, and in the last one - the KiB - when it is none of them (a number below a KiB, a negative, a NaN).
    /// </summary>
    private static readonly MemoryUnit[] MemoryUnits =
    [
        new(Gibibyte, GibibyteFormat, GibibyteUnit),
        new(Mebibyte, WholeFormat, MebibyteUnit),
        new(Kibibyte, WholeFormat, KibibyteUnit),
    ];

    /// <summary>Millicores as cores, without the unit: <c>0.81</c>, <c>3.86</c>, <c>12.5</c>.</summary>
    public static string Cores(double millicores) => (millicores / MillicoresPerCore).ToString(CoresFormat, CultureInfo.InvariantCulture);

    /// <summary>The CPU of a node or of the cluster, in cores: <c>0.81 of 3.86 cores</c>.</summary>
    public static string CoresOf(double usedMillicores, double ofMillicores) =>
        $"{Cores(usedMillicores)} of {Cores(ofMillicores)} {CoresUnitOf(ofMillicores)}";

    /// <summary>The CPU of a pod: <c>14 m</c> (millicores, a thousandth of a core) below one core, then <c>1.25 cores</c>.</summary>
    public static string Cpu(double millicores) =>
        millicores < MillicoresPerCore
            ? $"{Millicores(millicores)} {MillicoreUnit}"
            : $"{Cores(millicores)} {CoresUnitOf(millicores)}";

    /// <summary>A pod's CPU against its limit: <c>14 of 500 m</c>, <c>38 m of 1 core</c>, <c>1.2 of 2 cores</c>.</summary>
    public static string CpuOf(double usedMillicores, double limitMillicores) =>
        (usedMillicores < MillicoresPerCore, limitMillicores < MillicoresPerCore) switch
        {
            (true, true) => $"{Millicores(usedMillicores)} of {Millicores(limitMillicores)} {MillicoreUnit}",
            (false, false) => CoresOf(usedMillicores, limitMillicores),
            _ => $"{Cpu(usedMillicores)} of {Cpu(limitMillicores)}",
        };

    /// <summary>A pod's CPU for its row: against its limit where it has one, the plain number otherwise; null without a usage.</summary>
    public static string? CpuUse(ResourceUse cpu)
    {
        ArgumentNullException.ThrowIfNull(cpu);
        return UseAgainst(cpu.Usage, cpu.Limits, CpuOf, Cpu);
    }

    /// <summary>A pod's memory for its row, by the same rule.</summary>
    public static string? MemoryUse(ResourceUse memory)
    {
        ArgumentNullException.ThrowIfNull(memory);
        return UseAgainst(memory.Usage, memory.Limits, MemoryOf, Memory);
    }

    /// <summary>The CPU of a node or of all nodes against what they offer, in cores; null without a usage.</summary>
    public static string? NodeCpu(ResourceUse cpu)
    {
        ArgumentNullException.ThrowIfNull(cpu);
        return UseAgainst(cpu.Usage, cpu.Allocatable, CoresOf, used => $"{Cores(used)} {CoresUnit}");
    }

    /// <summary>The memory of a node or of all nodes against what they offer; null without a usage.</summary>
    public static string? NodeMemory(ResourceUse memory)
    {
        ArgumentNullException.ThrowIfNull(memory);
        return UseAgainst(memory.Usage, memory.Allocatable, MemoryOf, Memory);
    }

    /// <summary><c>12 of 110</c>; <c>12</c> without a capacity; null without a count.</summary>
    public static string? NodePods(ClusterNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return (node.Pods, node.PodCapacity) switch
        {
            ({ } count, { } capacity) => Of(count, capacity),
            ({ } count, null) => BuildText.Count(count),
            _ => null,
        };
    }

    /// <summary><c>Phase Running; 2 of 2 containers ready</c>; <c>Phase Running</c> without container counts.</summary>
    public static string PodContainers(ClusterPod pod)
    {
        ArgumentNullException.ThrowIfNull(pod);
        return (pod.ContainersReady, pod.Containers) switch
        {
            ({ } ready, { } all) => $"Phase {pod.Phase}; {Of(ready, all)} containers ready",
            _ => $"Phase {pod.Phase}",
        };
    }

    /// <summary>What the pods ask for of what the nodes offer: <c>63 % requested</c>; null without both numbers.</summary>
    public static string? Requested(ResourceUse use)
    {
        ArgumentNullException.ThrowIfNull(use);
        return Share(use.Requests, use.Allocatable) is { } share ? $"{Percent(share)} requested" : null;
    }

    /// <summary>Bytes in binary units: <c>512 KiB</c>, <c>258 MiB</c>, from 1024 MiB on <c>9.2 GiB</c>.</summary>
    public static string Memory(double bytes)
    {
        var (number, unit) = MemoryParts(bytes);
        return $"{number} {unit}";
    }

    /// <summary>Memory against a limit or a capacity: <c>258 of 512 MiB</c>, <c>9.2 of 12.5 GiB</c>, <c>258 MiB of 2 GiB</c>.</summary>
    public static string MemoryOf(double usedBytes, double ofBytes)
    {
        var (used, usedUnit) = MemoryParts(usedBytes);
        var (of, ofUnit) = MemoryParts(ofBytes);
        return usedUnit == ofUnit ? $"{used} of {of} {ofUnit}" : $"{used} {usedUnit} of {of} {ofUnit}";
    }

    /// <summary>
    /// The share of a whole in percent, by the <see cref="Shares" /> rule - the part over the whole, then the
    /// scale; null without both numbers or for a whole of zero.
    /// </summary>
    public static double? Share(double? part, double? whole) =>
        part is { } value && whole is { } of && of > 0 ? Shares.Of(value, of) : null;

    /// <summary>A whole percentage: <c>63 %</c>.</summary>
    public static string Percent(double percent) => Math.Round(percent).ToString(WholeFormat, CultureInfo.InvariantCulture) + PercentSign;

    /// <summary>True from <see cref="HighPercent"/> on: the meter and its number are marked.</summary>
    public static bool IsHigh(double? percent) => percent >= HighPercent;


    /// <summary><c>1 restart</c>, <c>7 restarts</c>.</summary>
    public static string Restarts(int restarts) => Plural.Of(restarts, "restart");

    /// <summary>How old something is at the moment the facts are of: <c>47 min</c>, <c>3 d</c>; null when the file does not say since when.</summary>
    public static string? Age(DateTimeOffset? since, DateTimeOffset asOf) => since is { } start ? TimeText.Span(asOf - start) : null;

    /// <summary><c>n of m</c>.</summary>
    public static string Of(int part, int whole) => string.Create(CultureInfo.InvariantCulture, $"{part} of {whole}");

    /// <summary>
    /// A node's name for a sentence: its first part and its last six characters, <c>aks-…000000</c> for
    /// <c>aks-system-12148983-vmss000000</c>. A short name is kept whole. The page shows the full name next to it or in the title.
    /// </summary>
    public static string ShortNode(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        const int Tail = 6;
        var dash = name.IndexOf('-', StringComparison.Ordinal);
        var head = dash + 1;
        return dash > 0 && name.Length > head + Ellipsis.Mark.Length + Tail ? $"{name[..head]}{Ellipsis.Mark}{name[^Tail..]}" : name;
    }

    /// <summary>A node as a sentence names it: <c>Node aks-…000000</c>.</summary>
    public static string NodeIn(string name) => $"Node {ShortNode(name)}";

    /// <summary>A node condition in words: <c>memory pressure</c> for MemoryPressure.</summary>
    public static string Pressure(string condition) => condition switch
    {
        "MemoryPressure" => "memory pressure",
        "DiskPressure" => "disk pressure",
        "PIDPressure" => "PID pressure",
        "NetworkUnavailable" => "network unavailable",
        _ => condition,
    };

    /// <summary>A node's conditions in words, joined in the order given: <c>memory pressure, disk pressure</c>.</summary>
    public static string Pressures(IEnumerable<string> conditions)
    {
        ArgumentNullException.ThrowIfNull(conditions);
        return string.Join(ListSeparator, conditions.Select(Pressure));
    }

    /// <summary><c>as of 15:10:04 (5 min ago)</c>.</summary>
    public static string AsOf(DateTimeOffset generated, DateTimeOffset now, TimeZoneInfo zone) =>
        $"as of {TimeText.Clock(generated, zone)} ({TimeText.Ago(generated, now)})";

    /// <summary>The note under Azure's facts when they are old; null while they are not.</summary>
    public static string? OldFacts(AksService service, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(service);
        return service.IsOld(now) && service.Generated is { } generated
            ? $"Azure's facts are {TimeText.Span(now - generated)} old: the workflow that publishes them may not be running."
            : null;
    }

    /// <summary>A pod and where it is: <c>ui in cmdemo3-tdd</c>.</summary>
    public static string PodIn(ClusterPod pod, string space)
    {
        ArgumentNullException.ThrowIfNull(pod);
        return $"{pod.Label} in {space}";
    }

    /// <summary>The word the badge says when a pod is ready and the word a ready node's badge says.</summary>
    private const string ReadyWord = "Ready";

    /// <summary>The Kubernetes condition word the badge says when a node is not ready.</summary>
    private const string NotReadyWord = "NotReady";

    /// <summary>
    /// A pod's state in the words of its row: <c>Ready</c>, <c>Finished</c>, <c>Starting</c>, and for an unhealthy one
    /// its reason, or what its phase says (<c>CrashLoopBackOff</c>, <c>Pending for 12 min</c>, <c>Not ready</c>).
    /// </summary>
    public static string PodWords(ClusterPod pod, PodState state, DateTimeOffset asOf)
    {
        ArgumentNullException.ThrowIfNull(pod);
        var phase = new PodPhase(pod.Phase);
        return state switch
        {
            PodState.Ready => ReadyWord,
            PodState.Finished => "Finished",
            PodState.Starting => $"Starting{Reason(pod.Reason)}",
            _ when phase.Is(PodPhaseKind.Pending) => Age(pod.StartedAt, asOf) is { } age
                ? $"Pending for {age}{Reason(pod.Reason)}"
                : TextReason.Said(pod.Reason) ?? ClusterPod.Pending,
            _ when phase.Is(PodPhaseKind.Running) => TextReason.Said(pod.Reason) ?? "Not ready",
            _ => TextReason.Said(pod.Reason) ?? pod.Phase,
        };
    }

    /// <summary>
    /// The state of a pod's badge: healthy when ready, checking while starting, unhealthy otherwise - and neutral
    /// for a finished job, which is no problem.
    /// </summary>
    public static ClusterState PodBadge(PodState state) => state switch
    {
        PodState.Ready => ClusterState.Healthy,
        PodState.Starting => ClusterState.Checking,
        PodState.Unhealthy => ClusterState.Unhealthy,
        _ => ClusterState.Neutral,
    };

    /// <summary>The state of a node's badge: healthy when the node is ready, unreachable when it is not.</summary>
    public static ClusterState NodeBadge(ClusterNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return node.Ready ? ClusterState.Healthy : ClusterState.Unreachable;
    }

    /// <summary>The word of a node's badge: <c>Ready</c> or the Kubernetes condition word <c>NotReady</c>.</summary>
    public static string NodeWords(ClusterNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return node.Ready ? ReadyWord : NotReadyWord;
    }

    /// <summary>
    /// The line over a namespace's pods: <c>3 of 3 ready · 1 finished · 1 restart · CPU 53 m · memory 1.5 GiB</c>. A
    /// part with nothing to say is left out.
    /// </summary>
    public static string Summary(NamespaceGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        var tally = new PodTally(group.Pods);
        var parts = new List<string> { $"{Of(tally.Ready, tally.Total)} ready" };
        if (tally.Finished > 0)
        {
            parts.Add(string.Create(CultureInfo.InvariantCulture, $"{tally.Finished} finished"));
        }

        parts.Add(Restarts(tally.Restarts));
        if (tally.CpuUsage is { } cpu)
        {
            parts.Add($"CPU {Cpu(cpu)}");
        }

        if (tally.MemoryUsage is { } memory)
        {
            parts.Add($"memory {Memory(memory)}");
        }

        return string.Join(PartSeparator, parts);
    }

    /// <summary>The line of the group of every namespace that belongs to no environment: <c>Platform: 9 namespaces, 54 of 54 pods ready</c>.</summary>
    public static string PlatformSummary(ClusterGroups groups)
    {
        ArgumentNullException.ThrowIfNull(groups);
        var count = groups.Platform.Count;
        var spaces = Plural.Of(count, "namespace");
        var tallies = groups.Platform.Select(space => new PodTally(space.Pods)).ToList();
        return $"Platform: {spaces}, {Of(tallies.Sum(tally => tally.Ready), tallies.Sum(tally => tally.Total))} pods ready";
    }

    /// <summary>A volume claim: <c>data-db-0 8 GiB, Bound</c>.</summary>
    public static string Volume(ClusterVolume volume)
    {
        ArgumentNullException.ThrowIfNull(volume);
        var size = volume.Capacity is { } capacity ? $"{volume.Name} {Memory(capacity)}" : volume.Name;
        return volume.Phase is { } phase ? $"{size}, {phase}" : size;
    }

    /// <summary>The line of a namespace's volume claims, joined in the order given: <c>data-db-0 8 GiB, Bound · logs 1 GiB</c>.</summary>
    public static string Volumes(IEnumerable<ClusterVolume> volumes)
    {
        ArgumentNullException.ThrowIfNull(volumes);
        return string.Join(PartSeparator, volumes.Select(Volume));
    }

    /// <summary>
    /// Azure's power and provisioning states of a node pool, power first: <c>Running, Succeeded</c>; null when Azure
    /// names neither.
    /// </summary>
    public static string? PoolState(AksPool pool)
    {
        ArgumentNullException.ThrowIfNull(pool);
        if (pool.PowerState is null && pool.ProvisioningState is null)
        {
            return null;
        }
        return string.Join(ListSeparator, new[] { pool.PowerState, pool.ProvisioningState }.OfType<string>());
    }

    /// <summary>A node pool as Azure describes it: <c>1 × Standard_D4as_v6</c>; null when Azure names neither.</summary>
    public static string? PoolSize(AksPool pool)
    {
        ArgumentNullException.ThrowIfNull(pool);
        return (pool.Count, pool.Size) switch
        {
            ({ } count, { } size) => string.Create(CultureInfo.InvariantCulture, $"{count} × {size}"),
            ({ } count, null) => Plural.Of(count, "node"),
            (null, { } size) => size,
            _ => null,
        };
    }

    /// <summary>What Azure's three numbers are an average of: <c>average of the last 15 minutes</c>.</summary>
    public static string MetricsWindow(AksMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        return metrics.WindowMinutes switch
        {
            null => "average, as Azure Monitor reports it",
            1 => "average of the last minute",
            var minutes => string.Create(CultureInfo.InvariantCulture, $"average of the last {minutes} minutes"),
        };
    }

    /// <summary>The tooltip of the link to the AKS cluster in the Azure portal.</summary>
    public static string PortalTitle(string? cluster) =>
        LinkText.Portal(LinkText.InPortal(cluster is null ? "The AKS cluster" : $"The AKS cluster {cluster}"));

    /// <summary>The tooltip of the link to the cluster's workloads in the Azure portal.</summary>
    public static string WorkloadsTitle(string? cluster) =>
        LinkText.Portal(cluster is null ? "The cluster's workloads in the Azure portal" : $"The workloads of {cluster} in the Azure portal: deployments, pods and their logs");

    /// <summary>
    /// A reason as the end of what comes before it: <c>: CrashLoopBackOff</c>, trimmed by <see cref="Words"/>; empty
    /// when there is nothing to say. See <see cref="TextReason.Reason"/>.
    /// </summary>
    internal static string Reason(string? detail) => TextReason.Reason(detail);

    /// <summary>
    /// The words of a reason without the spaces around them and without the full stops they end in. See
    /// <see cref="TextReason.Words"/>.
    /// </summary>
    internal static string Words(string? detail) => TextReason.Words(detail);

    /// <summary>
    /// The unit a quantity of CPU is said in: <c>core</c> for a quantity of exactly one core (1000 millicores),
    /// <c>cores</c> for every other - 0, 0.5, 1.5, NaN. The word agrees with a quantity, not with a count, so it is
    /// deliberately not <see cref="Plural"/>: there is no "count of one" in half a core.
    /// </summary>
    private static string CoresUnitOf(double millicores) => millicores == MillicoresPerCore ? CoreUnit : CoresUnit;

    /// <summary>A number of millicores in its format, without the unit: <c>14</c>, <c>1000</c>.</summary>
    private static string Millicores(double millicores) => millicores.ToString(MillicoreFormat, CultureInfo.InvariantCulture);

    /// <summary>
    /// A use against a whole: null without a usage, said against the whole when it is more than zero, plain otherwise.
    /// The four members that call it are only the choice of unit they make.
    /// </summary>
    private static string? UseAgainst(double? usage, double? whole, Func<double, double, string> against, Func<double, string> plain) =>
        usage is not { } used ? null : whole is { } of && of > 0 ? against(used, of) : plain(used);

    private static (string Number, string Unit) MemoryParts(double bytes)
    {
        var unit = Array.Find(MemoryUnits, candidate => bytes >= candidate.Bytes) ?? MemoryUnits[^1];
        return ((bytes / unit.Bytes).ToString(unit.Format, CultureInfo.InvariantCulture), unit.Unit);
    }

    /// <summary>A binary unit of memory: how many bytes it is, the format of its number and its name.</summary>
    private sealed record MemoryUnit(double Bytes, string Format, string Unit);
}
