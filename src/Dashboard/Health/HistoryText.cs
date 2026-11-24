using System.Globalization;

namespace Dashboard.Health;

/// <summary>The history strip in words, for readers who do not see it.</summary>
public static class HistoryText
{
    /// <summary>
    /// The states a history is counted by, in the order the sentence reads them: healthy first, then the two ways
    /// a check can fail - an answer that was not healthy, then no answer at all. <see cref="HealthState.Pending" />
    /// is left out on purpose: a check not yet answered counts in the total but names no state.
    /// </summary>
    private static readonly HealthState[] Counted = [HealthState.Healthy, HealthState.Unhealthy, HealthState.Unreachable];

    /// <summary><c>Last 12 checks: 10 healthy, 2 unreachable</c>.</summary>
    public static string Describe(IReadOnlyCollection<ProbeResult> history)
    {
        ArgumentNullException.ThrowIfNull(history);
        if (history.Count == 0)
        {
            return "No checks yet";
        }

        var parts = Counted
            .Select(state => (State: state, Count: history.Count(result => result.State == state)))
            .Where(entry => entry.Count > 0)
            .Select(entry => string.Create(CultureInfo.InvariantCulture, $"{entry.Count} {HealthClassifier.Sentence(entry.State)}"));
        var noun = Plural.Noun(history.Count, "check");
        var header = history.Count == 1 ? "Last" : $"Last {history.Count}";
        return $"{header} {noun}: {string.Join(", ", parts)}";
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

    /// <summary>The HTTP status and latency parts of a check result, joined by <paramref name="separator"/>.
    /// Empty when neither is present.</summary>
    public static string ResultFacts(ProbeResult result, string separator)
    {
        var parts = new System.Collections.Generic.List<string>();
        if (result.StatusCode is { } status)
        {
            parts.Add($"HTTP {status}");
        }

        if (result.LatencyMs is { } latency)
        {
            parts.Add($"{latency} ms");
        }

        return string.Join(separator, parts);
    }
}
