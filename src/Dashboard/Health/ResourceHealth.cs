using System.Globalization;
using System.Text.Json;

namespace Dashboard.Health;

/// <summary>
/// What Azure Resource Health last said of the system's resources: the <c>health</c> of each record of
/// <c>containers.json</c>, which workflow <c>delivery</c> publishes hourly on branch <c>status</c> next to
/// <c>delivery.json</c> (<c>scripts/write-containers.ps1</c>, as the system's Reader identity). Found by resource id,
/// whatever its case.
/// </summary>
/// <param name="Generated">When the file's content last changed.</param>
public sealed record ResourceHealthReport(DateTimeOffset? Generated, IReadOnlyDictionary<string, ResourceHealthReading> Readings)
{
    public ResourceHealthReading? Find(string id) => Readings.GetValueOrDefault(id);

    /// <summary>The readings of the file; null when it is not a JSON object with <c>resources</c>.</summary>
    public static ResourceHealthReport? Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (JsonRead.Section(root, "resources") is not { } groups)
            {
                return null;
            }

            var readings = new Dictionary<string, ResourceHealthReading>(StringComparer.OrdinalIgnoreCase);
            foreach (var group in groups.EnumerateObject().Where(group => group.Value.ValueKind == JsonValueKind.Array))
            {
                foreach (var record in group.Value.EnumerateArray())
                {
                    if (JsonRead.Text(record, "id") is { } id && ResourceHealthReading.Read(JsonRead.Section(record, "health")) is { } reading)
                    {
                        readings[id] = reading;
                    }
                }
            }

            return new ResourceHealthReport(JsonRead.Time(root, "generated"), readings);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

/// <summary>What the pipeline read of one resource in one run.</summary>
/// <param name="Status">
/// From Resource Health: <c>available</c>, <c>degraded</c> or <c>unavailable</c>. Where it has no answer (a type it does
/// not cover, or "Unknown"): the provisioning state, as <c>succeeded</c>, <c>failed</c> and the like; <c>unknown</c>
/// without either.
/// </param>
/// <param name="Source"><see cref="FromResourceHealth"/>, <see cref="FromProvisioningState"/> or <see cref="None"/>.</param>
/// <param name="CheckedAt">When the pipeline read it.</param>
/// <param name="Summary">What Resource Health said in words; null without it.</param>
public sealed record ResourceHealthReading(string Status, string Source, DateTimeOffset CheckedAt, string? Summary = null)
{
    public const string FromResourceHealth = "resourceHealth";
    public const string FromProvisioningState = "provisioningState";
    public const string None = "none";

    /// <summary>
    /// The bounds belong to the reading of <c>containers.json</c>, not to the words of the box: a status longer than
    /// <see cref="LongestStatus"/> is not a word or two and is refused; a summary is cut to <see cref="LongestSummary"/>.
    /// <see cref="ResourceHealthText"/> puts no length bound on what it is given - its tooltip is one line per resource
    /// (past eight, the rest are counted aloud), and a length cap there would silently drop resources from a reader's
    /// tooltip. Do not "tidy" the asymmetry by capping the words.
    /// </summary>
    private const int LongestStatus = 40;

    private const int LongestSummary = 200;

    /// <summary>A reading; null without a status of a word or two, or without its time.</summary>
    internal static ResourceHealthReading? Read(JsonElement? section)
    {
        var status = JsonRead.Text(section, "status")?.ToLowerInvariant();
        if (status is not { Length: <= LongestStatus } || !status.All(character => char.IsAsciiLetterLower(character) || character == ' ')
            || JsonRead.Time(section, "checkedAt") is not { } checkedAt)
        {
            return null;
        }

        var source = JsonRead.Text(section, "source") switch
        {
            FromResourceHealth => FromResourceHealth,
            FromProvisioningState => FromProvisioningState,
            _ => None,
        };
        var summary = JsonRead.Text(section, "summary");
        return new ResourceHealthReading(status, source, checkedAt, summary is { Length: > LongestSummary } ? summary[..LongestSummary] : summary);
    }
}

/// <summary>The words of a box from what Resource Health said of the resources it stands for.</summary>
/// <param name="Label">The badge: the most urgent state of its resources.</param>
/// <param name="Line">Where the words come from and how long ago, or the count of each state for a box of several.</param>
/// <param name="Tone">
/// <see cref="HealthTone.Plain"/> while the oldest reading is at most two hours old, <see cref="HealthTone.Muted"/>
/// otherwise. A string, as the page writes it; the making only ever writes one of <see cref="HealthTone"/>'s words.
/// </param>
/// <param name="Title">
/// The tooltip: each resource, its state, where it comes from and when. Held whole, of any length: the length bounds
/// are the reading's (<see cref="ResourceHealthReading"/>), and a cap here would cut resources off the tooltip.
/// </param>
public sealed record ResourceHealthWords(string Label, string Line, string Tone, string Title);

/// <summary>
/// The tones the words of Resource Health are dressed in - two, and never a colour: the page's own word for each, as
/// <see cref="ResourceHealthWords.Tone"/> and <c>UnplacedRow.HealthTone</c> hold it.
/// </summary>
public static class HealthTone
{
    /// <summary>Words read recently: shown as they are.</summary>
    public const string Plain = "plain";

    /// <summary>Words read long ago, or not read at all: shown quieter.</summary>
    public const string Muted = "muted";

    /// <summary><see cref="Plain"/> for a recent reading, <see cref="Muted"/> otherwise.</summary>
    public static string Of(bool recent) => recent ? Plain : Muted;
}

/// <summary>
/// The badges of a box, most urgent first: the order is the declaration's, so a box of several shows the badge that
/// comes first here. Every word <see cref="ResourceHealthText.Label"/> makes has its place by name.
/// </summary>
public enum HealthBadge
{
    /// <summary>Resource Health said <c>unavailable</c>.</summary>
    Unavailable,

    /// <summary>
    /// A provisioning state other than <c>succeeded</c> (<c>Failed</c>, <c>Updating</c>, <c>Deleting</c> and the like),
    /// worded as Azure says it: less urgent than a resource that is down, more than one that is only degraded.
    /// </summary>
    OtherProvisioningState,

    /// <summary>Resource Health said <c>degraded</c>.</summary>
    Degraded,

    /// <summary>Nothing said where it stands: <c>Not known</c>.</summary>
    NotKnown,

    /// <summary>Resource Health had no answer and the provisioning state is <c>succeeded</c>.</summary>
    Provisioned,

    /// <summary>Resource Health said <c>available</c>.</summary>
    Available,
}

/// <summary>
/// Resource Health on a box, in neutral words, never a colour: <c>Available</c> and <c>Resource Health, 40 min ago</c>.
/// The pipeline reads it hourly; this page does not ask Azure.
/// </summary>
public static class ResourceHealthText
{
    /// <summary>The tooltip's first words.</summary>
    public const string Source = "Azure Resource Health, read hourly by the pipeline (workflow delivery, as the system's Reader identity); this page does not ask Azure.";

    private const string Unavailable = "Unavailable";
    private const string Degraded = "Degraded";
    private const string NotKnown = HealthDetailText.NotKnown;
    private const string Provisioned = "Provisioned";
    private const string Available = "Available";
    private const int MostNamed = 8;

    /// <summary>The words of a box of these resources; null for a box without ids (a manifest from before them).</summary>
    public static ResourceHealthWords? Of(IReadOnlyList<string>? ids, ResourceHealthReport? report, DateTimeOffset? now, TimeZoneInfo zone)
    {
        ArgumentNullException.ThrowIfNull(zone);
        if (ids is not { Count: > 0 })
        {
            return null;
        }

        if (report is null)
        {
            return new ResourceHealthWords(
                NotKnown,
                "no Resource Health read yet",
                HealthTone.Muted,
                $"{Source} The page has not read it: {SystemInfo.ContainersFileName} on branch status, next to delivery.json, is not there or not read yet.");
        }

        var readings = ids.Select(id => (Id: id, Reading: report.Find(id))).ToList();
        var labels = readings.Select(entry => Label(entry.Reading)).ToList();
        var oldest = Oldest(readings.Select(entry => entry.Reading));
        return new ResourceHealthWords(Badge(labels), Line(readings, labels, oldest, now), Tone(oldest, now), Title(readings, now, zone));
    }

    /// <summary>The badge's word for one reading: <c>Available</c>, <c>Provisioned</c>, <c>Failed</c>, <c>Not known</c>.</summary>
    public static string Label(ResourceHealthReading? reading) => reading switch
    {
        { Source: ResourceHealthReading.FromResourceHealth, Status: "available" or "degraded" or "unavailable" } => Capitalized(reading.Status),
        { Source: ResourceHealthReading.FromProvisioningState, Status: "succeeded" } => Provisioned,
        { Source: ResourceHealthReading.FromProvisioningState } => Capitalized(reading.Status),
        _ => NotKnown,
    };

    /// <summary>The badge of a box: the most urgent of its resources' words (<see cref="Rank"/>); the first of equals.</summary>
    private static string Badge(IEnumerable<string> labels) => labels.OrderBy(Rank).First();

    /// <summary>
    /// The line under the badge: where the words come from for a box of one, the count of each badge (most urgent first)
    /// for a box of several; then how long ago the oldest reading was, when there is one and a moment to measure from.
    /// </summary>
    private static string Line(List<(string Id, ResourceHealthReading? Reading)> readings, List<string> labels, DateTimeOffset? oldest, DateTimeOffset? now)
    {
        var age = oldest is { } at && now is { } moment ? $", {TimeText.Ago(at, moment)}" : string.Empty;
        var words = readings.Count == 1 ? Origin(readings[0].Reading) : Counted(labels);
        return words + age;
    }

    /// <summary>Where the words of a box of one come from.</summary>
    private static string Origin(ResourceHealthReading? reading) => reading?.Source switch
    {
        ResourceHealthReading.FromResourceHealth => "Resource Health",
        ResourceHealthReading.FromProvisioningState => "provisioning state",
        ResourceHealthReading.None => "no Resource Health",
        _ => "not in the hourly reading",
    };

    /// <summary>The count of each badge of a box of several, most urgent first: <c>1 degraded · 2 available</c>.</summary>
    private static string Counted(IEnumerable<string> labels) => string.Join(
        " · ",
        labels.GroupBy(word => word)
            .OrderBy(group => Rank(group.Key))
            .Select(group => string.Create(CultureInfo.InvariantCulture, $"{group.Count()} {group.Key.ToLowerInvariant()}")));

    /// <summary>When the oldest of the readings was taken; null when none of the resources was read.</summary>
    private static DateTimeOffset? Oldest(IEnumerable<ResourceHealthReading?> readings)
    {
        var read = readings.OfType<ResourceHealthReading>().ToList();
        return read.Count == 0 ? null : read.Min(reading => reading.CheckedAt);
    }

    /// <summary><see cref="HealthTone.Plain"/> while the oldest reading is within <see cref="HourlyCheckText.Recent"/> of now.</summary>
    private static string Tone(DateTimeOffset? oldest, DateTimeOffset? now) =>
        HealthTone.Of(oldest is { } since && now is { } current && current - since <= HourlyCheckText.Recent);

    /// <summary>The tooltip: the source, then up to <see cref="MostNamed"/> resources described, then how many more.</summary>
    private static string Title(List<(string Id, ResourceHealthReading? Reading)> readings, DateTimeOffset? now, TimeZoneInfo zone)
    {
        var title = new List<string> { Source };
        title.AddRange(NamedList.Named(readings.Select(entry => Describe(entry.Id, entry.Reading, now, zone)), MostNamed));
        if (NamedList.More(readings.Count, MostNamed) is { } more)
        {
            title.Add($"And {more}.");
        }

        return string.Join('\n', title);
    }

    /// <summary>
    /// The place of a badge's word among the badges (<see cref="HealthBadge"/>, most urgent first). Any word
    /// <see cref="Label"/> makes that is not one of the five fixed ones is a provisioning state it capitalized, and
    /// ranks as <see cref="HealthBadge.OtherProvisioningState"/>.
    /// </summary>
    private static HealthBadge Rank(string label) => label switch
    {
        Unavailable => HealthBadge.Unavailable,
        Degraded => HealthBadge.Degraded,
        NotKnown => HealthBadge.NotKnown,
        Provisioned => HealthBadge.Provisioned,
        Available => HealthBadge.Available,
        _ => HealthBadge.OtherProvisioningState,
    };

    private static string Describe(string id, ResourceHealthReading? reading, DateTimeOffset? now, TimeZoneInfo zone)
    {
        var name = id.TrimEnd('/').Split('/')[^1];
        if (reading is null)
        {
            return $"{name}: not in the hourly reading.";
        }

        var source = reading.Source switch
        {
            ResourceHealthReading.FromResourceHealth => "by Resource Health",
            ResourceHealthReading.FromProvisioningState => "Resource Health has no answer for it; its provisioning state",
            _ => "neither Resource Health nor a provisioning state answered",
        };
        var when = now is { } moment ? $"{TimeText.DateAndTime(reading.CheckedAt, zone)} ({TimeText.Ago(reading.CheckedAt, moment)})" : TimeText.DateAndTime(reading.CheckedAt, zone);
        var summary = reading.Summary is { } said ? $" {said}" : string.Empty;
        return $"{name}: {reading.Status} ({source}), read {when}.{summary}";
    }

    private static string Capitalized(string word) => word.Length == 0 ? word : char.ToUpperInvariant(word[0]) + word[1..];
}
