using System.Globalization;
using Dashboard.Health;

namespace Dashboard.Cluster;

/// <summary>The cluster's numbers and names in the words of the cluster view.</summary>
public static class ClusterText
{
    /// <summary>From this share of its limit on, a meter is marked as high.</summary>
    public const double HighPercent = 90;

    /// <summary>The millicores in one core: the cluster reads CPU in millicores and says it in cores.</summary>
    internal const double MillicoresPerCore = 1000;

    /// <summary>The format of a pod's CPU below one core: a whole number of millicores.</summary>
    private const string MillicoreFormat = "0";

    /// <summary>The unit a number of millicores is said with: <c>14 m</c>.</summary>
    private const string MillicoreUnit = "m";

    /// <summary>The format of a number of cores: at most two digits after the point.</summary>
    private const string CoresFormat = "0.##";

    /// <summary>The word a quantity of CPU is said in: <c>0.81 cores</c>, <c>1.5 cores</c>.</summary>
    internal const string CoresUnit = "cores";

    /// <summary>The word the quantity of exactly one core is said in: <c>1 core</c>.</summary>
    private const string CoreUnit = "core";

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
        new(Gibibyte, "0.#", "GiB"),
        new(Mebibyte, "0", "MiB"),
        new(Kibibyte, "0", "KiB"),
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
    public static string Percent(double percent) => string.Create(CultureInfo.InvariantCulture, $"{Math.Round(percent):0} %");

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
        return dash > 0 && name.Length > dash + 1 + Tail + 1 ? $"{name[..(dash + 1)]}…{name[^Tail..]}" : name;
    }

    /// <summary>A node condition in words: <c>memory pressure</c> for MemoryPressure.</summary>
    public static string Pressure(string condition) => condition switch
    {
        "MemoryPressure" => "memory pressure",
        "DiskPressure" => "disk pressure",
        "PIDPressure" => "PID pressure",
        "NetworkUnavailable" => "network unavailable",
        _ => condition,
    };

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
            PodState.Ready => "Ready",
            PodState.Finished => "Finished",
            PodState.Starting => $"Starting{Reason(pod.Reason)}",
            _ when phase.Is(PodPhaseKind.Pending) => Age(pod.StartedAt, asOf) is { } age
                ? $"Pending for {age}{Reason(pod.Reason)}"
                : Said(pod.Reason) ?? ClusterPod.Pending,
            _ when phase.Is(PodPhaseKind.Running) => Said(pod.Reason) ?? "Not ready",
            _ => Said(pod.Reason) ?? pod.Phase,
        };
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

        return string.Join(" · ", parts);
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
        LinkText.Portal(cluster is null ? "The AKS cluster in the Azure portal" : $"The AKS cluster {cluster} in the Azure portal");

    /// <summary>The tooltip of the link to the cluster's workloads in the Azure portal.</summary>
    public static string WorkloadsTitle(string? cluster) =>
        LinkText.Portal(cluster is null ? "The cluster's workloads in the Azure portal" : $"The workloads of {cluster} in the Azure portal: deployments, pods and their logs");

    /// <summary>
    /// A reason as the end of what comes before it: <c>: CrashLoopBackOff</c>, trimmed by <see cref="Words"/>; empty
    /// when there is nothing to say.
    /// </summary>
    internal static string Reason(string? detail) => Said(detail) is { } words ? $": {words}" : string.Empty;

    /// <summary>
    /// The words of a reason without the spaces around them and without the full stops they end in - a stop followed
    /// by a space is still a stop - so the one that closes the sentence is the only one said.
    /// </summary>
    internal static string Words(string? detail)
    {
        var words = (detail ?? string.Empty).Trim();
        while (words.EndsWith('.'))
        {
            words = words.TrimEnd('.').TrimEnd();
        }

        return words;
    }

    /// <summary>The <see cref="Words"/> of a reason; null when nothing is left of it.</summary>
    private static string? Said(string? detail) => Words(detail) is { Length: > 0 } words ? words : null;

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
