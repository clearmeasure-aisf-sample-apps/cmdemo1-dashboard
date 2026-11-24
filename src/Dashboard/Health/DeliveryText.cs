using System.Globalization;

namespace Dashboard.Health;

/// <summary>
/// The hourly health reports in the words of the page: <c>Healthy in 23 of 24 hourly checks (95.8 %) in 24 hours ·
/// 164 of 166 in 7 days · last failure 9 h ago</c>.
/// </summary>
public static class AvailabilityText
{
    /// <summary>Said next to the numbers, so nobody reads them as monitoring.</summary>
    public const string Note = "Hourly checks by the pipeline, not continuous monitoring.";

    /// <summary>The tooltip: who counts, what a check is, and what the count misses.</summary>
    public static string Help(DateTimeOffset? changed, TimeZoneInfo zone)
    {
        ArgumentNullException.ThrowIfNull(zone);
        const string What = "Counted by the pipeline: once an hour the runbook \"Health report\" in Octopus Deploy asks every node of the environment and its public address, and the report fails when one is not healthy. It is no continuous monitoring: an outage between two reports is not counted.";
        return changed is { } at ? $"{What} As of the last change of the delivery facts: {TimeText.DateAndTime(at, zone)}." : What;
    }

    /// <summary>
    /// The parts of the line, in order: the last 24 hours, the last seven days, the last failure. A part the file
    /// does not have is left out; none at all is an empty list.
    /// </summary>
    public static IReadOnlyList<AvailabilityPart> Parts(HealthReports reports, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(reports);
        var parts = new List<AvailabilityPart>();
        if (reports.Last24Hours is { } day)
        {
            parts.Add(day.Reports == 0
                ? new AvailabilityPart("No hourly check ended in ", "24 hours", string.Empty)
                : new AvailabilityPart("Healthy in ", Count(day), $" hourly {(day.Reports == 1 ? "check" : "checks")} ({Percent(day)}) in 24 hours"));
        }

        if (reports.Last7Days is { } week)
        {
            // After the 24 hours the words are short; alone, the part says what it counts.
            parts.Add((parts.Count > 0, week.Reports) switch
            {
                (true, 0) => new AvailabilityPart(string.Empty, "none", " in 7 days"),
                (true, _) => new AvailabilityPart(string.Empty, Count(week), " in 7 days", $"{Percent(week)} of the hourly checks of the last 7 days"),
                (false, 0) => new AvailabilityPart("No hourly check ended in ", "7 days", string.Empty),
                _ => new AvailabilityPart("Healthy in ", Count(week), $" hourly {(week.Reports == 1 ? "check" : "checks")} ({Percent(week)}) in 7 days"),
            });
        }

        if (reports.LastFailure is { } failed)
        {
            parts.Add(new AvailabilityPart("last failure ", TimeText.Ago(failed, now), string.Empty, When: failed));
        }
        else if (reports.Last7Days is { Reports: > 0, AllHealthy: true })
        {
            parts.Add(new AvailabilityPart("no failure in ", "7 days", string.Empty));
        }

        return parts;
    }

    /// <summary>The whole line in one text, its parts joined with a dot.</summary>
    public static string Line(HealthReports reports, DateTimeOffset now) =>
        string.Join(" · ", Parts(reports, now).Select(part => $"{part.Before}{part.Strong}{part.After}"));

    /// <summary><c>23 of 24</c>.</summary>
    public static string Count(HealthWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);
        return string.Create(CultureInfo.InvariantCulture, $"{window.Healthy} of {window.Reports}");
    }

    /// <summary>
    /// <c>95.8 %</c>, <c>100 %</c>: one decimal, and never 100 while a report failed (<c>99.9 %</c>) nor 0 while one
    /// passed (<c>0.1 %</c>).
    /// </summary>
    public static string Percent(HealthWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);
        if (window.Reports == 0)
        {
            return "–";
        }

        var percent = Math.Round(100.0 * window.Healthy / window.Reports, 1);
        percent = window.Healthy < window.Reports ? Math.Min(percent, 99.9) : percent;
        percent = window.Healthy > 0 ? Math.Max(percent, 0.1) : percent;
        return string.Create(CultureInfo.InvariantCulture, $"{percent:0.#} %");
    }
}

/// <summary>One part of the availability line: words, the number in the middle that carries it, words.</summary>
/// <param name="Title">The part's own tooltip; null without one.</param>
/// <param name="When">The moment the part names, for a <c>time</c> element; null when it names none.</param>
public sealed record AvailabilityPart(string Before, string Strong, string After, string? Title = null, DateTimeOffset? When = null);

/// <summary>The delivery facts in the words of the "Delivery" card.</summary>
public static class DeliveryText
{
    /// <summary><c>5.2 h</c>; under an hour <c>40 min</c>; from two days on <c>3.5 d</c>.</summary>
    public static string LeadTime(double hours) =>
        hours < 1 ? string.Create(CultureInfo.InvariantCulture, $"{Math.Round(hours * 60):0} min")
        : hours < 48 ? string.Create(CultureInfo.InvariantCulture, $"{hours:0.#} h")
        : string.Create(CultureInfo.InvariantCulture, $"{hours / 24:0.#} d");

    /// <summary>Whose delivery a card shows: <c>ui in uat</c>; the system project by what it is.</summary>
    public static string Context(string name, string environment) =>
        DeliveryReport.IsSystem(name) ? $"the system (infrastructure and pipeline) in {environment}" : $"{name} in {environment}";

    /// <summary>What "Last by a person" is, for the tooltip.</summary>
    public const string PersonSignOffHelp =
        "The newest deployment here that a person signed off; automation signs off the others with a recorded reason";

    /// <summary>What "behind" counts, for the tooltip.</summary>
    public static string BehindHelp(string first) =>
        $"Versions: the distance in the project's list of releases. Days: since this environment and {first} last ran the same release.";

    /// <summary>
    /// <c>same as tdd</c>, <c>2 versions, 3 days behind tdd</c>; null for the first environment itself and when the
    /// file does not say.
    /// </summary>
    public static string? Behind(BehindFirst? behind, string environment, string? first)
    {
        if (behind is null || first is null || string.Equals(environment, first, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (behind.Versions == 0)
        {
            return $"same as {first}";
        }

        var parts = new List<string>();
        if (behind.Versions is { } versions && versions > 0)
        {
            parts.Add(Plural.Of(versions, "version"));
        }

        if (behind.Days is { } days && Math.Round(days) >= 1)
        {
            var whole = (int)Math.Round(days);
            parts.Add(Plural.Of(whole, "day"));
        }

        return parts.Count > 0 ? $"{string.Join(", ", parts)} behind {first}" : behind.Days is not null ? $"same as {first}" : null;
    }

    /// <summary><c>4 deployments, none failed</c>; null when the file does not count them.</summary>
    public static string? Frequency(DeliveryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (entry.DeploymentsLast7Days is not { } count)
        {
            return null;
        }

        var deployments = Plural.Of(count, "deployment");
        return entry.FailedLast7Days switch
        {
            null => deployments,
            0 => count == 0 ? deployments : $"{deployments}, none failed",
            var failed => string.Create(CultureInfo.InvariantCulture, $"{deployments}, {failed} failed"),
        };
    }

    /// <summary><c>uat, 17 h ago: traffic moved to the standby in 44 s</c>.</summary>
    public static string Failover(FailoverTest test, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(test);
        var parts = new List<string>();
        if (test.Environment is { } environment)
        {
            parts.Add(environment);
        }

        if (test.At is { } at)
        {
            parts.Add(TimeText.Ago(at, now));
        }

        var text = string.Join(", ", parts);
        if (test.Seconds is { } seconds)
        {
            var took = string.Create(CultureInfo.InvariantCulture, $"the standby answered after {seconds:0.#} s");
            text = text.Length > 0 ? $"{text}: {took}" : took;
        }

        return text;
    }
}
