using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Dashboard.Health;

/// <summary>
/// What the pipeline's hourly health reports found for one container app of an environment: an entry of
/// <c>health.containerApps</c> in <c>delivery.json</c>, which the system's workflow reads from the line the runbook
/// "Health report" writes for the app. The page probes the app only while it shows it (<see cref="ContainerAppProbe"/>);
/// this is the history under that probe. Hourly checks, not monitoring.
/// </summary>
/// <param name="Name">The deployable the app runs, as <c>system.json</c> names it.</param>
/// <param name="Last">The newest report that asked it; null when none did in seven days.</param>
/// <param name="Last24Hours">
/// The reports of the last 24 hours that asked it, and those of them that found it healthy; null when the file does
/// not count them (a report of that time whose log was not read).
/// </param>
/// <param name="Last7Days">The same for the last seven days.</param>
public sealed record ContainerAppReports(string Name, HourlyCheck? Last, HealthWindow? Last24Hours, HealthWindow? Last7Days)
{
    /// <summary>The entries of <c>containerApps</c> of an environment's <c>health</c>; null without such an array.</summary>
    internal static IReadOnlyList<ContainerAppReports>? ReadAll(JsonElement? health)
    {
        if (health is not { ValueKind: JsonValueKind.Object } section
            || !section.TryGetProperty("containerApps", out var array)
            || array.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        return [.. array.EnumerateArray().Select(Read).OfType<ContainerAppReports>()];
    }

    /// <summary>An entry; null without a name.</summary>
    private static ContainerAppReports? Read(JsonElement element) =>
        JsonRead.Text(element, "name") is { } name
            ? new ContainerAppReports(
                name,
                HourlyCheck.Read(JsonRead.Section(element, "last")),
                HealthWindow.Read(JsonRead.Section(element, "last24Hours")),
                HealthWindow.Read(JsonRead.Section(element, "last7Days")))
            : null;
}

/// <summary>
/// What one hourly health report found for a container app: the three results the runbook "Health report" writes,
/// and nothing else. The word the file writes and the box says is <see cref="HourlyCheckResultWords.Word"/>.
/// </summary>
public enum HourlyCheckResult
{
    Healthy,
    Unhealthy,
    Unreachable,
}

/// <summary>The lower-case words of <see cref="HourlyCheckResult"/>, as the file writes them and the box says them.</summary>
public static class HourlyCheckResultWords
{
    /// <summary><c>healthy</c>, <c>unhealthy</c> or <c>unreachable</c>; any other value is refused.</summary>
    public static string Word(this HourlyCheckResult result) => result switch
    {
        HourlyCheckResult.Healthy => "healthy",
        HourlyCheckResult.Unhealthy => "unhealthy",
        HourlyCheckResult.Unreachable => "unreachable",
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, "An hourly check is healthy, unhealthy or unreachable."),
    };

    /// <summary>The result a word of the file names, in any case; null for any other word, or none.</summary>
    internal static HourlyCheckResult? Of(string? word) => word?.ToLowerInvariant() switch
    {
        "healthy" => HourlyCheckResult.Healthy,
        "unhealthy" => HourlyCheckResult.Unhealthy,
        "unreachable" => HourlyCheckResult.Unreachable,
        _ => null,
    };
}

/// <summary>What one hourly health report found for a container app, and when.</summary>
/// <param name="Result"><see cref="Healthy"/>, <see cref="Unhealthy"/> or <see cref="Unreachable"/>.</param>
/// <param name="At">When that report ended.</param>
public sealed record HourlyCheck(HourlyCheckResult Result, DateTimeOffset At)
{
    public const HourlyCheckResult Healthy = HourlyCheckResult.Healthy;
    public const HourlyCheckResult Unhealthy = HourlyCheckResult.Unhealthy;
    public const HourlyCheckResult Unreachable = HourlyCheckResult.Unreachable;

    /// <summary>The result; null unless it is one of the three words and has its time.</summary>
    internal static HourlyCheck? Read(JsonElement? section) =>
        HourlyCheckResultWords.Of(JsonRead.Text(section, "result")) is { } result && JsonRead.Time(section, "at") is { } at
            ? new HourlyCheck(result, at)
            : null;
}

/// <summary>
/// The hourly health report on a container app's box, in neutral words: <c>hourly: healthy, 40 min ago</c>, then
/// <c>24 h: 23 of 24 · 7 d: 160 of 168</c>. Never a state of the box: the page's own probe is, and a report says how
/// the app answered once, up to two hours ago.
/// </summary>
public static class HourlyCheckText
{
    /// <summary>
    /// How old the newest result may be and still be said: the runbook runs hourly, and the delivery facts are
    /// published hourly, so a result is up to two hours old when the box shows it. The two hours are an age, not a
    /// signed difference: <see cref="RecentResult"/> bounds them both ways, so a result is said while its moment
    /// stands within the two hours on either side of the moment asked. A moment more than two hours ahead of it is
    /// no news either - a report stamped a day in the future by a host with a bad clock, or an offset written that
    /// is not the one the report ended at, is no more "just now" than a report that stopped. Older, or further
    /// ahead, the box says <see cref="NoRecentCheck"/>: a report that stopped, or one that never was, is no news
    /// about the app.
    /// </summary>
    public static readonly TimeSpan Recent = TimeSpan.FromHours(2);

    /// <summary>The words of a box without a result of the last two hours.</summary>
    public const string NoRecentCheck = "no recent hourly check";

    /// <summary>
    /// The newest result when its moment stands within <see cref="Recent"/> of the moment asked, either side - at
    /// most <see cref="Recent"/> old and at most <see cref="Recent"/> ahead; null otherwise.
    /// </summary>
    public static HourlyCheck? RecentResult(ContainerAppReports? reports, DateTimeOffset now) =>
        reports?.Last is { } last && (now - last.At).Duration() <= Recent ? last : null;

    /// <summary><c>hourly: healthy, 40 min ago</c>; <see cref="NoRecentCheck"/> without a result of the last two hours.</summary>
    public static string Result(ContainerAppReports? reports, DateTimeOffset now) =>
        RecentResult(reports, now) is { } last ? $"hourly: {last.Result.Word()}, {TimeText.Ago(last.At, now)}" : NoRecentCheck;

    /// <summary>
    /// <c>24 h: 23 of 24 · 7 d: 160 of 168</c>: of the reports that asked the app, those that found it healthy. A time
    /// the file does not count is left out, and one without a report says "none"; null when neither is counted.
    /// </summary>
    public static string? Counts(ContainerAppReports? reports)
    {
        var parts = Counted(reports).Select(counted => $"{counted.Kind.Label}: {Count(counted.Window)}").ToList();
        return parts.Count == 0 ? null : string.Join(" · ", parts);
    }

    /// <summary>The tooltip: what the report is, what it last found and when, and what it counted.</summary>
    public static string Title(ContainerAppReports? reports, DateTimeOffset now, TimeZoneInfo zone)
    {
        ArgumentNullException.ThrowIfNull(zone);
        var words = new List<string> { HourlyReportWords.Intro };
        if (reports?.Last is { } last)
        {
            var when = string.Format(CultureInfo.InvariantCulture, HourlyReportWords.When, TimeText.DateAndTime(last.At, zone), TimeText.Ago(last.At, now));
            var found = RecentResult(reports, now) is null ? HourlyReportWords.StaleReport : HourlyReportWords.RecentReport;
            words.Add(string.Format(CultureInfo.InvariantCulture, found, when, last.Result.Word()));
        }
        else
        {
            words.Add(reports is null ? HourlyReportWords.NoReportYet : HourlyReportWords.NotAskedThisWeek);
        }

        words.AddRange(Counted(reports).Select(counted => string.Format(
            CultureInfo.InvariantCulture, HourlyReportWords.HealthyOf, counted.Window.Healthy, counted.Window.Reports, counted.Kind.InSentence)));
        words.Add(AvailabilityText.Note);
        return string.Join(' ', words);
    }

    private static string Count(HealthWindow window) => window.Reports == 0 ? "none" : AvailabilityText.Count(window);

    /// <summary>The last 24 hours: <c>24 h</c> on the box, <c>24 hours</c> in the tooltip.</summary>
    private static readonly CountedWindow Day = new("24 h", "24 hours", reports => reports.Last24Hours);

    /// <summary>The last seven days: <c>7 d</c> on the box, <c>7 days</c> in the tooltip.</summary>
    private static readonly CountedWindow Week = new("7 d", "7 days", reports => reports.Last7Days);

    /// <summary>The windows the report counts, in the order the box and the tooltip say them.</summary>
    private static readonly CountedWindow[] Windows = [Day, Week];

    /// <summary>The windows the file counts for the app, with their counts; none without reports.</summary>
    private static IEnumerable<(CountedWindow Kind, HealthWindow Window)> Counted(ContainerAppReports? reports) =>
        reports is null
            ? []
            : Windows.Select(kind => (Kind: kind, Window: kind.Of(reports)))
                .Where(counted => counted.Window is not null)
                .Select(counted => (counted.Kind, counted.Window!));

    /// <summary>A window the hourly report counts: its short label on the box, its words in a sentence, and where the report keeps it.</summary>
    private sealed record CountedWindow(string Label, string InSentence, Func<ContainerAppReports, HealthWindow?> Of);

    /// <summary>
    /// The sentences of <see cref="Title"/>, named so the tooltip's template reads in one place and
    /// <see cref="Title"/> only chooses which of them are said.
    /// </summary>
    private static class HourlyReportWords
    {
        /// <summary>What the hourly report is; said first, always.</summary>
        public const string Intro =
            "Once an hour the pipeline's runbook \"Health report\" in Octopus Deploy asks the app whether it answers, also while no page is open: the history, under the page's own probe.";

        /// <summary>When the newest report ended: {0} the date and time in the viewer's zone, {1} how long ago.</summary>
        public static readonly CompositeFormat When = CompositeFormat.Parse("{0} ({1})");

        /// <summary>The newest report while it is still news: {0} when it ended, {1} what it found.</summary>
        public static readonly CompositeFormat RecentReport =
            CompositeFormat.Parse("The last report that asked it ended {0} and found it {1}.");

        /// <summary>The newest report once it is older than <see cref="Recent"/>: {0} when it ended, {1} what it found.</summary>
        public static readonly CompositeFormat StaleReport =
            CompositeFormat.Parse($"The last report that asked it ended {{0}} and found it {{1}}: none in the last two hours, so the box says {NoRecentCheck}.");

        /// <summary>An app the delivery facts have no reports for at all.</summary>
        public const string NoReportYet = "The delivery facts name no report of it yet.";

        /// <summary>An app the reports of the last seven days did not ask.</summary>
        public static readonly string NotAskedThisWeek = $"No report of the last {Week.InSentence} asked it.";

        /// <summary>A counted window: {0} healthy, of {1} reports, in {2} the window's words.</summary>
        public static readonly CompositeFormat HealthyOf =
            CompositeFormat.Parse("Healthy in {0} of the {1} reports of {2} that asked it.");
    }
}
