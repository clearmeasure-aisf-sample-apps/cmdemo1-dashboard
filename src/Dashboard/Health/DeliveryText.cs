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

    /// <summary>The number of decimal places the percent is rounded to: one tenth of a percent.</summary>
    private const int PercentDecimals = 1;

    /// <summary>What the percent says when the window holds no report: the dash, the answer the count no report
    /// stands behind is no share.</summary>
    private const string NoReports = "–";

    /// <summary>
    /// The display format of the percent: <c>0.#</c>, at most one decimal, none for a whole number. It must show
    /// <see cref="PercentDecimals" /> decimal places - the rounding and the format agree on the one decimal the
    /// percent says - and a whole number is said without the decimal that has nothing to say (<c>100 %</c>,
    /// <c>99 %</c>, not <c>100.0 %</c> or <c>99.0 %</c>).
    /// </summary>
    private const string PercentFormat = "0.#";

    /// <summary>What every percent word ends with: a space and the sign, <c>95.8 %</c>.</summary>
    private const string PercentSuffix = " %";

    /// <summary>
    /// The highest percent said while a report failed: one tenth below the whole, so the rounding of a single
    /// failure in many reports never carries the share onto all.
    /// </summary>
    private const double BelowWhole = 99.9;

    /// <summary>
    /// The lowest percent said while a report passed: one tenth above nothing, so the rounding of a single
    /// passing report in many never carries the share onto none.
    /// </summary>
    private const double AboveNothing = 0.1;

    /// <summary>
    /// What a window with no ended report says in place of <see cref="Count"/>: the word the count stands in for
    /// while no report has ended in the window.
    /// </summary>
    internal const string NoneCounted = "none";

    /// <summary>
    /// The day's window word: the last 24 hours, as the hourly reports count them (<c>ContainerAppReports</c> names
    /// the same window <c>Day</c>).
    /// </summary>
    private const string DayWindow = "24 hours";

    /// <summary>
    /// The week's window word: the last seven days (<c>ContainerAppReports</c> names the same window <c>Week</c>).
    /// </summary>
    private const string WeekWindow = "7 days";

    /// <summary>The words between the parts of the line, the dot the parts hang on: <c>·</c>.</summary>
    private const string PartSeparator = " · ";

    /// <summary>The lead word of the last failure's part, the words the age of the failure hangs after.</summary>
    private const string LastFailureLead = "last failure ";

    /// <summary>The lead word of the healthy week's part: a week that held reports and no failure.</summary>
    private const string NoFailureLead = "no failure in ";

    /// <summary>The lead word of a window's counted part, the words the healthy count hangs after.</summary>
    private const string HealthyLead = "Healthy in ";

    /// <summary>The lead word of a window that held no ended report at all.</summary>
    private const string NoCheckEndedLead = "No hourly check ended in ";

    /// <summary>The tail of a part that says its number in a window, the words the window word hangs after.</summary>
    private static string InWindow(string window) => $" in {window}";

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
            parts.Add(day.Reports == 0 ? NoCheckEnded(DayWindow) : Hourly(day, DayWindow));
        }

        if (reports.Last7Days is { } week)
        {
            parts.Add(WeekPart(afterTheDay: parts.Count > 0, week));
        }

        if (reports.LastFailure is { } failed)
        {
            parts.Add(new AvailabilityPart(LastFailureLead, TimeText.Ago(failed, now), string.Empty, When: failed));
        }
        else if (reports.Last7Days is { Reports: > 0, AllHealthy: true })
        {
            parts.Add(new AvailabilityPart(NoFailureLead, WeekWindow, string.Empty));
        }

        return parts;
    }

    /// <summary>
    /// The seven days' part. After the 24 hours the words are short (<c>164 of 166 in 7 days</c>, its percent in the
    /// tooltip); alone, the part says what it counts, the way the 24 hours do.
    /// </summary>
    private static AvailabilityPart WeekPart(bool afterTheDay, HealthWindow week)
    {
        var noneEnded = week.Reports == 0;
        if (afterTheDay)
        {
            return noneEnded
                ? new AvailabilityPart(string.Empty, NoneCounted, InWindow(WeekWindow))
                : new AvailabilityPart(string.Empty, Count(week), InWindow(WeekWindow), $"{Percent(week)} of the hourly checks of the last {WeekWindow}");
        }

        return noneEnded ? NoCheckEnded(WeekWindow) : Hourly(week, WeekWindow);
    }

    /// <summary>
    /// <c>Healthy in 23 of 24 hourly checks (95.8 %) in 24 hours</c>; the one report of a window is said
    /// <c>Healthy in 1 of 1 hourly check (100 %) in 24 hours</c>. The word after <c>hourly</c> is
    /// <see cref="Plural.Noun(int, string, string?)" />'s, so the line carries no second copy of the agreement.
    /// </summary>
    private static AvailabilityPart Hourly(HealthWindow window, string inWhat) =>
        new(HealthyLead, Count(window), $" hourly {Plural.Noun(window.Reports, "check")} ({Percent(window)}) in {inWhat}");

    /// <summary><c>No hourly check ended in 7 days</c>.</summary>
    private static AvailabilityPart NoCheckEnded(string inWhat) => new(NoCheckEndedLead, inWhat, string.Empty);

    /// <summary>The whole line in one text, its parts joined with a dot.</summary>
    public static string Line(HealthReports reports, DateTimeOffset now) =>
        string.Join(PartSeparator, Parts(reports, now).Select(part => $"{part.Before}{part.Strong}{part.After}"));

    /// <summary><c>23 of 24</c>.</summary>
    public static string Count(HealthWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);
        return string.Create(CultureInfo.InvariantCulture, $"{window.Healthy} of {window.Reports}");
    }

    /// <summary>
    /// <c>95.8 %</c>, <c>100 %</c>: one decimal, and never 100 while a report failed (<c>99.9 %</c>) nor 0 while one
    /// passed (<c>0.1 %</c>). The share is cut by the <see cref="Dashboard.Shares" /> rule - the healthy over the
    /// reports, then the scale - in the one order the dashboard's percents of a part of a whole are cut.
    /// </summary>
    public static string Percent(HealthWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);
        if (window.Reports == 0)
        {
            return NoReports;
        }

        var percent = Math.Round(Shares.Of(window.Healthy, window.Reports), PercentDecimals);
        percent = window.Healthy < window.Reports ? Math.Min(percent, BelowWhole) : percent;
        percent = window.Healthy > 0 ? Math.Max(percent, AboveNothing) : percent;
        return string.Create(CultureInfo.InvariantCulture, $"{percent.ToString(PercentFormat, CultureInfo.InvariantCulture)}{PercentSuffix}");
    }
}

/// <summary>One part of the availability line: words, the number in the middle that carries it, words.</summary>
/// <remarks>
/// The words are the caller's: a part is a value object the line builds out of literals, and
/// <see cref="AvailabilityText.Parts" /> is its one producer. <see cref="Before" />, <see cref="Strong" /> and
/// <see cref="After" /> are never null by contract - the empty string is the word that is not there - and the record
/// does not refuse a null on purpose: a null is a caller's mistake, not a file's silence, and the page would print it
/// as nothing. <see cref="CostPart" /> holds its words the same way.
/// </remarks>
/// <param name="Before">The words before the number; the empty string for none. Never null.</param>
/// <param name="Strong">The number that carries the part. Never null.</param>
/// <param name="After">The words after the number; the empty string for none. Never null.</param>
/// <param name="Title">The part's own tooltip; null without one.</param>
/// <param name="When">The moment the part names, for a <c>time</c> element; null when it names none.</param>
public sealed record AvailabilityPart(string Before, string Strong, string After, string? Title = null, DateTimeOffset? When = null);

/// <summary>The delivery facts in the words of the "Delivery" card.</summary>
public static class DeliveryText
{
    /// <summary>A lead time is said in minutes below this many hours.</summary>
    private const double OneHour = 1.0;

    /// <summary>A lead time is said in hours below this many hours (two days), then in days.</summary>
    private const double HoursBeforeDays = 48.0;

    /// <summary>Minutes per hour, for converting lead times under one hour.</summary>
    private const double MinutesPerHour = 60.0;

    /// <summary>Hours per day, for converting lead times of two days or more.</summary>
    private const double HoursPerDay = 24.0;

    /// <summary>
    /// The tenth format: at most one decimal, and a whole number is written without it (<c>5.2 h</c>, <c>2 h</c>).
    /// Both lead-time units (hours and days) and the failover seconds use it.
    /// </summary>
    private const string TenthFormat = "0.#";

    /// <summary>The whole-minute format: the already-rounded minutes of a lead time, written without decimals.</summary>
    private const string WholeFormat = "0";

    /// <summary>
    /// <c>5.2 h</c>; under an hour <c>40 min</c>; from two days on <c>3.5 d</c>. Unlike
    /// <see cref="DeploymentText.Age" />, a lead time keeps its tenth (<see cref="TenthFormat" />) rather than
    /// rounding to whole units.
    /// </summary>
    public static string LeadTime(double hours) => hours switch
    {
        < OneHour => $"{Math.Round(hours * MinutesPerHour).ToString(WholeFormat, CultureInfo.InvariantCulture)} {TimeText.MinutesUnit}",
        < HoursBeforeDays => $"{Tenth(hours)} {TimeText.HoursUnit}",
        _ => $"{Tenth(hours / HoursPerDay)} {TimeText.DaysUnit}",
    };

    /// <summary>A number in <see cref="TenthFormat" />, in the invariant culture: <c>5.2</c>, <c>44</c>.</summary>
    private static string Tenth(double value) => value.ToString(TenthFormat, CultureInfo.InvariantCulture);

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
            return SameAs(first);
        }

        var parts = new List<string>();
        if (behind.Versions is { } versions && versions > 0)
        {
            parts.Add(Plural.Of(versions, "version"));
        }

        if (behind.Days is { } days)
        {
            var whole = (int)Math.Round(days);
            if (whole >= 1)
            {
                parts.Add(Plural.Of(whole, "day"));
            }
        }

        if (parts.Count > 0)
        {
            return $"{string.Join(", ", parts)} behind {first}";
        }

        return behind.Days is not null ? SameAs(first) : null;
    }

    /// <summary>The one home of the answer "this environment is level with the first": <c>same as tdd</c>.</summary>
    private static string SameAs(string first) => $"same as {first}";

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
            var took = $"the standby answered after {Tenth(seconds)} {TimeText.SecondsUnit}";
            text = text.Length > 0 ? $"{text}: {took}" : took;
        }

        return text;
    }
}
