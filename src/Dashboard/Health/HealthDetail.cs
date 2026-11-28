using System.Globalization;
using System.Text.Json;

namespace Dashboard.Health;

/// <summary>The state of one entry of a detailed health check, as ASP.NET Core's health checks name them.</summary>
public enum CheckState
{
    Healthy,

    /// <summary>It works, with a reservation: the app answers its health check with HTTP 200 all the same.</summary>
    Degraded,

    Unhealthy,

    /// <summary>A status this dashboard does not know.</summary>
    Unknown,
}

/// <summary>One entry of a detailed health check: one thing the app depends on and checks.</summary>
/// <param name="Name">The entry's name, as the app registered the check (<c>DataAccess</c>).</param>
/// <param name="State">What the check found.</param>
/// <param name="Description">The check's own words; null when it gave none.</param>
/// <param name="DurationMs">How long the check took, in milliseconds.</param>
public sealed record HealthCheckEntry(string Name, CheckState State, string? Description = null, double? DurationMs = null)
{
    public bool IsHealthy => State == CheckState.Healthy;
}

/// <summary>
/// What a node's health check found, entry by entry: the answer of its detailed health check
/// (<c>deployables[].healthDetailPath</c> of the topology, for example <c>/_healthcheck/detailed</c>). The health
/// check itself answers with a status only; this says which dependency is the reason.
/// </summary>
/// <param name="Entries">The entries, in the order of the answer.</param>
/// <param name="ReadAt">When the dashboard read it.</param>
public sealed record HealthDetail(IReadOnlyList<HealthCheckEntry> Entries, DateTimeOffset ReadAt)
{
    /// <summary>The answer's word of the list of checks.</summary>
    private const string EntriesField = "entries";

    /// <summary>The word of an entry's name; an entry without it is left out.</summary>
    private const string NameField = "name";

    /// <summary>The word of an entry's status, matched against the <c>Said*</c> words.</summary>
    private const string StatusField = "status";

    /// <summary>The word of the check's own words.</summary>
    private const string DescriptionField = "description";

    /// <summary>The word of how long the check took, in milliseconds.</summary>
    private const string DurationField = "durationMs";

    /// <summary>The status ASP.NET Core writes for <see cref="CheckState.Healthy" />, compared after <c>ToLowerInvariant()</c>.</summary>
    private const string SaidHealthy = "healthy";

    /// <summary>The status ASP.NET Core writes for <see cref="CheckState.Degraded" />, compared after <c>ToLowerInvariant()</c>.</summary>
    private const string SaidDegraded = "degraded";

    /// <summary>The status ASP.NET Core writes for <see cref="CheckState.Unhealthy" />, compared after <c>ToLowerInvariant()</c>.</summary>
    private const string SaidUnhealthy = "unhealthy";

    /// <summary>The entries that are not healthy, the worst first (unhealthy, degraded, unknown), then in the answer's order.</summary>
    public IReadOnlyList<HealthCheckEntry> NotHealthy =>
        [.. Entries.Where(entry => !entry.IsHealthy).OrderBy(entry => HealthDetailText.Severity(entry.State))];

    /// <summary>The entry of that name, whatever its case; null when the answer has none.</summary>
    public HealthCheckEntry? Find(string? name) =>
        name is null ? null : Entries.FirstOrDefault(entry => string.Equals(entry.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// The endpoint's answer; null when it is not a JSON object with <c>entries</c> that name at least one check (an
    /// older app, an error page). An entry without a name is left out; a status that is absent or not known is
    /// <see cref="CheckState.Unknown"/>.
    /// </summary>
    public static HealthDetail? Parse(string? json, DateTimeOffset readAt)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var entries = JsonRead.Items(document.RootElement, EntriesField)
                .Select(entry => (Name: JsonRead.Text(entry, NameField), Element: entry))
                .Where(entry => entry.Name is not null)
                .Select(entry => new HealthCheckEntry(
                    entry.Name!,
                    StateOf(JsonRead.Text(entry.Element, StatusField)),
                    JsonRead.Text(entry.Element, DescriptionField),
                    JsonRead.Number(entry.Element, DurationField)))
                .ToList();
            return entries.Count == 0 ? null : new HealthDetail(entries, readAt);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static CheckState StateOf(string? status) => status?.ToLowerInvariant() switch
    {
        SaidHealthy => CheckState.Healthy,
        SaidDegraded => CheckState.Degraded,
        SaidUnhealthy => CheckState.Unhealthy,
        _ => CheckState.Unknown,
    };
}

/// <summary>The entries of a detailed health check in the words of the tiles.</summary>
public static class HealthDetailText
{
    /// <summary>The milliseconds in one second: the boundary and the divisor are the same fact.</summary>
    private const double MillisecondsPerSecond = 1000;

    /// <summary>
    /// The smallest duration shown in digits: below this <see cref="OneDecimalFormat" /> rounds to 0.1, so these
    /// durations are said as <see cref="UnderSmallestShown" /> instead - half of 0.1, the tie to the format and the
    /// word in the text is explicit here.
    /// </summary>
    private const double SmallestShown = 0.05;

    /// <summary>
    /// The format <see cref="OneDecimal" /> says a duration in: at most one decimal, none for a whole number.
    /// <see cref="SmallestShown" /> is half of its last digit, and <see cref="UnderSmallestShown" /> says the
    /// durations below it.
    /// </summary>
    private const string OneDecimalFormat = "0.#";

    /// <summary>The word a duration under a second is said with: <c>42.1 ms</c>.</summary>
    private const string MillisecondsUnit = "ms";

    /// <summary>The word a duration from a second on is said with: <c>1.2 s</c>.</summary>
    private const string SecondsUnit = "s";

    /// <summary>
    /// What <see cref="Duration" /> says below <see cref="SmallestShown" />: the floor phrase is built from
    /// <see cref="MillisecondsUnit" />, so the words move together with the unit.
    /// </summary>
    private const string UnderSmallestShown = "under 0.1 " + MillisecondsUnit;

    /// <summary>How many characters of a failing entry's name the one-line summary holds.</summary>
    private const int NameLength = 20;

    /// <summary>
    /// The word for a state the checks say nothing about: <c>CheckState.Unknown</c> and any status the dashboard
    /// does not know. Owned by <see cref="HealthDetailText" /> so that <c>ResourceHealthText</c> and
    /// <c>RuntimePayloadBuilder</c> read it from here instead of writing their own copy (#2055).
    /// </summary>
    internal const string NotKnown = "Not known";

    /// <summary>The word for <see cref="CheckState.Degraded" />.</summary>
    private const string DegradedWord = "Degraded";

    /// <summary>The lead words of <see cref="Failed" />'s sentence, before the list of the entries that failed.</summary>
    private const string NotHealthyLead = "Not healthy: ";

    /// <summary>The separator between the entries in <see cref="Failed" />'s list.</summary>
    private const string ListSeparator = ", ";

    /// <summary>The state's word: a mark is never colour alone.</summary>
    public static string Label(CheckState state) => state switch
    {
        CheckState.Healthy => HealthClassifier.HealthyWord,
        CheckState.Degraded => DegradedWord,
        CheckState.Unhealthy => HealthClassifier.UnhealthyWord,
        _ => NotKnown,
    };

    /// <summary>The order of the states, the worst first: unhealthy, degraded, not known, healthy.</summary>
    public static int Severity(CheckState state) => state switch
    {
        CheckState.Unhealthy => 0,
        CheckState.Degraded => 1,
        CheckState.Unknown => 2,
        _ => 3,
    };

    /// <summary>The icon of a mark, by shape: check, warning triangle, cross, dots.</summary>
    public static HealthState Icon(CheckState state) => state switch
    {
        CheckState.Healthy => HealthState.Healthy,
        CheckState.Degraded => HealthState.Unhealthy,
        CheckState.Unhealthy => HealthState.Unreachable,
        _ => HealthState.Pending,
    };

    /// <summary>A mark's tooltip: <c>DataAccess: Healthy. Database connection successful. Took 42.1 ms.</c></summary>
    public static string Title(HealthCheckEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        var parts = new List<string> { $"{entry.Name}: {Label(entry.State)}." };
        if (entry.Description is { } description)
        {
            parts.Add(description.EndsWith('.') ? description : $"{description}.");
        }

        if (entry.DurationMs is { } duration)
        {
            parts.Add($"Took {Duration(duration)}.");
        }

        return string.Join(' ', parts);
    }

    /// <summary><c>42.1 ms</c>; under a tenth of a millisecond <c>under 0.1 ms</c>; from a second on <c>1.2 s</c>.</summary>
    public static string Duration(double milliseconds) =>
        milliseconds < SmallestShown ? UnderSmallestShown
        : milliseconds < MillisecondsPerSecond ? OneDecimal(milliseconds, MillisecondsUnit)
        : OneDecimal(milliseconds / MillisecondsPerSecond, SecondsUnit);

    /// <summary>A number to one decimal place in the invariant culture, with its unit: <c>42.1 ms</c>, <c>1.5 s</c>.</summary>
    private static string OneDecimal(double value, string unit) =>
        value.ToString(OneDecimalFormat, CultureInfo.InvariantCulture) + " " + unit;

    /// <summary>
    /// One short line for all entries: <c>4 checks healthy</c>; the one that is not, by name, <c>DataAccess
    /// unhealthy</c>; or how many are not, <c>2 of 4 checks not healthy</c> (the marks' tooltips name them).
    /// </summary>
    public static string Summary(HealthDetail detail)
    {
        ArgumentNullException.ThrowIfNull(detail);
        var failing = detail.NotHealthy;
        var count = detail.Entries.Count;
        return failing.Count switch
        {
            0 => Plural.Of(count, "check healthy", "checks healthy"),
            1 => $"{Brief(failing[0].Name, NameLength)} {Label(failing[0].State).ToLowerInvariant()}",
            _ => string.Create(CultureInfo.InvariantCulture, $"{failing.Count} of {count} checks not healthy"),
        };
    }

    /// <summary>
    /// Words cut to what a line of the diagram holds: the first sentence when that fits, otherwise the start with an
    /// ellipsis. The tooltip has them in full.
    /// </summary>
    public static string Brief(string text, int length)
    {
        ArgumentNullException.ThrowIfNull(text);
        var sentence = text.IndexOf(". ", StringComparison.Ordinal) is var end and >= 0 ? text[..end] : text.TrimEnd('.');
        return sentence.Length <= length ? sentence : Ellipsis.Cut(text, length);
    }

    /// <summary>
    /// One entry's phrase in <see cref="Failed" />'s list: its name in brackets with its state and its own words:
    /// <c>DataAccess (Unhealthy: Login failed)</c>.
    /// </summary>
    public static string Failing(HealthCheckEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return $"{entry.Name} ({Label(entry.State)}{TextReason.Reason(entry.Description)})";
    }

    /// <summary>
    /// Which entries are the reason a node's health check fails, each with its state and its own words:
    /// <c>Not healthy: DataAccess (Unhealthy: Login failed), LlmGateway (Degraded)</c>. Null when every entry is healthy.
    /// </summary>
    public static string? Failed(HealthDetail? detail)
    {
        if (detail is null || detail.NotHealthy is not { Count: > 0 } failing)
        {
            return null;
        }

        return NotHealthyLead + string.Join(ListSeparator, failing.Select(Failing));
    }
}
