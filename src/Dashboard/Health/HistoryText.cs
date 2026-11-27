using System.Globalization;

namespace Dashboard.Health;

/// <summary>The history strip in words, for readers who do not see it.</summary>
public static class HistoryText
{
    /// <summary>
    /// The no-check wording, owned here once and read by both the strip's empty-slot hover title and
    /// <see cref="Describe(IReadOnlyCollection{ProbeResult})" /> for an empty history: <c>No checks yet</c>.
    /// Zero checks takes the plural, and the strip's title and the aria-label say the same words, so an
    /// empty history never has two different sentences depending on where you read it.
    /// </summary>
    public const string NoChecksYet = "No checks yet";

    /// <summary>
    /// The states a history is counted by, in the order the sentence reads them: healthy first, then the two ways
    /// a check can fail - an answer that was not healthy, then no answer at all. <see cref="HealthState.Pending" />
    /// is left out on purpose: a check not yet answered counts in the total but names no state.
    /// </summary>
    private static readonly HealthState[] Counted = [HealthState.Healthy, HealthState.Unhealthy, HealthState.Unreachable];

    /// <summary>
    /// <c>Last 12 checks: 10 healthy, 2 unreachable</c>; the single check is said without its digit, <c>Last check:
    /// 1 healthy</c>. The word after <c>Last</c> is <see cref="Plural.Noun(int, string, string?)" />'s, so the
    /// sentence carries no second copy of the agreement; the digit it counts with is written only when there is more
    /// than one to count, in the invariant culture.
    /// </summary>
    public static string Describe(IReadOnlyCollection<ProbeResult> history)
    {
        ArgumentNullException.ThrowIfNull(history);
        if (history.Count == 0)
        {
            return NoChecksYet;
        }

        var parts = Counted
            .Select(state => (State: state, Count: history.Count(result => result.State == state)))
            .Where(entry => entry.Count > 0)
            .Select(entry => string.Create(CultureInfo.InvariantCulture, $"{entry.Count} {HealthClassifier.Sentence(entry.State)}"));
        var noun = Plural.Noun(history.Count, "check");
        var last = history.Count == 1 ? "Last" : string.Create(CultureInfo.InvariantCulture, $"Last {history.Count}");
        return $"{last} {noun}: {string.Join(", ", parts)}";
    }

    /// <summary>One check in words: <c>15:04:05 Healthy, HTTP 200, 123 ms</c>.</summary>
    public static string Describe(ProbeResult result, TimeZoneInfo zone)
    {
        ArgumentNullException.ThrowIfNull(result);
        var text = $"{TimeText.Clock(result.CheckedAt, zone)} {HealthClassifier.Label(result.State)}";
        var facts = ResultFacts(result, ", ");
        if (facts.Length > 0)
        {
            text += $", {facts}";
        }

        return text;
    }

    /// <summary>The HTTP status word of a probe answer: <c>HTTP 200</c>.</summary>
    public static string Status(int code) => string.Create(CultureInfo.InvariantCulture, $"HTTP {code}");

    /// <summary>The millisecond word of a probe answer: <c>85 ms</c>.</summary>
    public static string Milliseconds(int ms) => string.Create(CultureInfo.InvariantCulture, $"{ms} ms");

    /// <summary>The HTTP status and latency parts of a check result, joined by <paramref name="separator"/>.
    /// Empty when neither is present.</summary>
    public static string ResultFacts(ProbeResult result, string separator)
    {
        var parts = new List<string>();
        if (result.StatusCode is { } status)
        {
            parts.Add(Status(status));
        }

        if (result.LatencyMs is { } latency)
        {
            parts.Add(Milliseconds(latency));
        }

        return string.Join(separator, parts);
    }
}
