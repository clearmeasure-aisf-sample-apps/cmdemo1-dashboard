using System.Globalization;
using System.Text.Json;

namespace Dashboard.Health;

/// <summary>
/// What a web app says about its own process in the <c>process</c> object of its telemetry endpoint. Every value is
/// optional: an app reports what it measures.
/// </summary>
/// <param name="CpuPercent">The process's share of the machine's processors over the last interval.</param>
/// <param name="WorkingSetMb">The memory the process holds, in megabytes.</param>
/// <param name="GcHeapMb">Of that, the managed heap.</param>
/// <param name="Threads">Threads of the process.</param>
/// <param name="InFlight">Requests being answered at this moment.</param>
/// <param name="ExceptionsPerMinute">Exceptions thrown in the last minute, handled or not.</param>
/// <param name="UptimeSeconds">How long the process has run.</param>
public sealed record ProcessVitals(
    double? CpuPercent,
    double? WorkingSetMb,
    double? GcHeapMb,
    int? Threads,
    int? InFlight,
    int? ExceptionsPerMinute,
    long? UptimeSeconds)
{
    /// <summary>An uptime below this is shown as a restart.</summary>
    public static readonly TimeSpan RecentRestart = TimeSpan.FromMinutes(5);

    /// <summary>The reading of a <c>process</c> object that holds no value: every part unmeasured.</summary>
    private static readonly ProcessVitals Nothing = new(null, null, null, null, null, null, null);

    public bool RestartedRecently => UptimeSeconds is { } seconds && IsRecentRestart(seconds);

    /// <summary>The one statement of the rule: an uptime of fewer seconds than <see cref="RecentRestart" /> is a restart.</summary>
    internal static bool IsRecentRestart(double uptimeSeconds) => uptimeSeconds < RecentRestart.TotalSeconds;

    /// <summary>The <c>process</c> object; null when the answer has none (an older app) or it holds no value.</summary>
    internal static ProcessVitals? Read(JsonElement root)
    {
        var process = JsonRead.Section(root, "process");
        if (process is null)
        {
            return null;
        }

        var vitals = new ProcessVitals(
            JsonRead.Number(process, "cpuPercent"),
            JsonRead.Number(process, "workingSetMb"),
            JsonRead.Number(process, "gcHeapMb"),
            JsonRead.Count(process, "threads"),
            JsonRead.Count(process, "inFlight"),
            JsonRead.Count(process, "exceptionsPerMinute"),
            JsonRead.Long(process, "uptimeSeconds"));
        return vitals == Nothing ? null : vitals;
    }
}

/// <summary>A process's vitals in the words of the tiles.</summary>
public static class VitalsText
{
    /// <summary>The CPU percent below which the tile keeps one decimal: <see cref="Cpu" /> says <c>3.2 %</c> not <c>3 %</c>.</summary>
    private const double OneDecimalBelow = 10;

    /// <summary>The megabytes in one gigabyte: <see cref="Memory" /> switches from MB to GB at this point and divides by it.</summary>
    private const double MegabytesPerGigabyte = 1024;

    /// <summary>The digits the tile keeps for the one decimal it says below <see cref="OneDecimalBelow" />: <see cref="OneDecimalFormat" /> keeps the same count.</summary>
    private const int OneDecimalDigits = 1;

    /// <summary>The digits of a number the tile says to one decimal, <c>3.2</c>: the one decimal of <see cref="OneDecimalDigits" />.</summary>
    private const string OneDecimalFormat = "0.0";

    /// <summary>The digits of a number the tile says whole, <c>3</c>, <c>412</c>: no decimal.</summary>
    private const string WholeFormat = "0";

    /// <summary>The unit the tile says after a cpu percent, <c> %</c>: the same sign <see cref="Trends.Cpu" /> gives its trend.</summary>
    internal const string PercentSign = " %";

    /// <summary>The unit the tile says after the gigabytes of <see cref="Memory" />, <c> GB</c>: the word the memory switches to at <see cref="MegabytesPerGigabyte" /> MB.</summary>
    private const string GigabyteUnit = " GB";

    /// <summary>The unit the tile says after the megabytes of <see cref="Memory" />, <c> MB</c>: the word the memory says below <see cref="MegabytesPerGigabyte" /> MB.</summary>
    private const string MegabyteUnit = " MB";

    /// <summary>The words <see cref="Uptime" /> says before the length of a young process, <c>restarted </c>: below <see cref="ProcessVitals.RecentRestart" />.</summary>
    private const string RestartedLead = "restarted ";

    /// <summary>The word <see cref="Uptime" /> says before the length of a settled process, <c>up </c>: from <see cref="ProcessVitals.RecentRestart" /> on.</summary>
    private const string UpLead = "up ";

    /// <summary>The words <see cref="MemoryDetail" /> says before the working set, <c>Working set </c>: the first clause of the memory tooltip.</summary>
    private const string WorkingSetLead = "Working set ";

    /// <summary>The words <see cref="MemoryDetail" /> says between the working set and the heap within it, <c>, of which </c>.</summary>
    private const string OfWhichLead = ", of which ";

    /// <summary>The words <see cref="MemoryDetail" /> says before the heap after <see cref="OfWhichLead" />, <c>managed heap </c>.</summary>
    private const string HeapWords = "managed heap ";

    /// <summary>The words <see cref="MemoryDetail" /> says before the heap when no working set leads it, <c>Managed heap </c>: <see cref="HeapWords" /> opening the clause.</summary>
    private const string ManagedHeapLead = "Managed heap ";

    /// <summary>The word <see cref="MemoryDetail" /> says after the thread count, <c> threads</c>.</summary>
    private const string ThreadsWord = " threads";

    /// <summary><c>3 %</c>; one decimal below <see cref="OneDecimalBelow" />.</summary>
    public static string Cpu(double percent) =>
        percent.ToString(percent < OneDecimalBelow && Math.Round(percent, OneDecimalDigits) != Math.Round(percent)
            ? OneDecimalFormat
            : WholeFormat, CultureInfo.InvariantCulture) + PercentSign;

    /// <summary><c>412 MB</c>, and from <see cref="MegabytesPerGigabyte" /> MB on <c>1.4 GB</c>.</summary>
    public static string Memory(double megabytes) =>
        megabytes >= MegabytesPerGigabyte
            ? (megabytes / MegabytesPerGigabyte).ToString(OneDecimalFormat, CultureInfo.InvariantCulture) + GigabyteUnit
            : megabytes.ToString(WholeFormat, CultureInfo.InvariantCulture) + MegabyteUnit;

    /// <summary><c>up 2 h</c>; below <see cref="ProcessVitals.RecentRestart" /> <c>restarted 3 min ago</c>.</summary>
    public static string Uptime(long seconds)
    {
        var span = TimeSpan.FromSeconds(seconds);
        var length = TimeText.Span(span);
        return ProcessVitals.IsRecentRestart(span.TotalSeconds) ? RestartedLead + TimeText.Before(length) : UpLead + length;
    }

    /// <summary>The memory in full, for the tooltip: <c>Working set 412 MB, of which managed heap 96 MB; 41 threads</c>.</summary>
    public static string? MemoryDetail(ProcessVitals vitals)
    {
        ArgumentNullException.ThrowIfNull(vitals);
        var parts = new List<string>();
        if (vitals.WorkingSetMb is { } workingSet)
        {
            var said = WorkingSetLead + Memory(workingSet);
            parts.Add(vitals.GcHeapMb is { } heap ? said + OfWhichLead + HeapWords + Memory(heap) : said);
        }
        else if (vitals.GcHeapMb is { } heap)
        {
            parts.Add(ManagedHeapLead + Memory(heap));
        }

        if (vitals.Threads is { } threads)
        {
            parts.Add(threads.ToString(CultureInfo.InvariantCulture) + ThreadsWord);
        }

        return TooltipClauses.Join(parts);
    }
}
