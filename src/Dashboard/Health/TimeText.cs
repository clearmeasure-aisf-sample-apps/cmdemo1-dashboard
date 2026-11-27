using System.Globalization;

namespace Dashboard.Health;

/// <summary>Times as the dashboard shows them, in the time zone of the viewer.</summary>
public static class TimeText
{
    /// <summary>Seconds before the unit rolls up to a minute.</summary>
    private const double SecondsUntilMinutes = 60;

    /// <summary>Minutes (two hours) before the unit rolls up to an hour.</summary>
    private const double MinutesUntilHours = 120;

    /// <summary>Hours (two days) before the unit rolls up to a day.</summary>
    private const double HoursUntilDays = 48;

    /// <summary>Moments newer than this are "just now".</summary>
    private static readonly TimeSpan JustNow = TimeSpan.FromMinutes(1);

    /// <summary>The time of day: <c>15:04:05</c>.</summary>
    private const string ClockFormat = "HH:mm:ss";

    /// <summary>Date and time to the minute with the offset from UTC: <c>2026-10-04 17:00 -05:00</c>.</summary>
    private const string DateAndTimeFormat = "yyyy-MM-dd HH:mm zzz";

    /// <summary>
    /// The UTC machine-readable form for a <c>time</c> element's <c>datetime</c> attribute: <c>2026-10-04T22:00:00Z</c>.
    /// </summary>
    private const string IsoFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";

    /// <summary>
    /// A length in one whole unit, truncated and in the invariant culture: <c>45 s</c>, <c>3 min</c>, <c>2 h</c>,
    /// <c>4 d</c> (<c>5 h</c> for five and a half hours). The one home of how a count of whole units is said, for
    /// <see cref="Span"/> and for <see cref="DeploymentText.Age"/>, each of which keeps its own unit boundaries.
    /// </summary>
    internal static string Whole(double amount, string unit) =>
        string.Create(CultureInfo.InvariantCulture, $"{(int)amount} {unit}");

    /// <summary>The time of day: <c>15:04:05</c>.</summary>
    public static string Clock(DateTimeOffset time, TimeZoneInfo zone) =>
        TimeZoneInfo.ConvertTime(time, zone).ToString(ClockFormat, CultureInfo.InvariantCulture);

    /// <summary>Date and time to the minute with the offset from UTC: <c>2026-10-04 17:00 -05:00</c>.</summary>
    public static string DateAndTime(DateTimeOffset time, TimeZoneInfo zone) =>
        TimeZoneInfo.ConvertTime(time, zone).ToString(DateAndTimeFormat, CultureInfo.InvariantCulture);

    /// <summary>The machine-readable form for the <c>datetime</c> attribute of a <c>time</c> element.</summary>
    public static string Iso(DateTimeOffset time) =>
        time.ToUniversalTime().ToString(IsoFormat, CultureInfo.InvariantCulture);

    /// <summary>
    /// A length of time in whole units: <c>45 s</c>, <c>3 min</c>, <c>2 h</c>, <c>4 d</c>. A unit is used until two of
    /// the next: minutes up to two hours, hours up to two days.
    /// </summary>
    public static string Span(TimeSpan span)
    {
        if (span < TimeSpan.Zero)
        {
            span = TimeSpan.Zero;
        }

        return span.TotalSeconds < SecondsUntilMinutes ? Whole(span.TotalSeconds, "s")
            : span.TotalMinutes < MinutesUntilHours ? Whole(span.TotalMinutes, "min")
            : span.TotalHours < HoursUntilDays ? Whole(span.TotalHours, "h")
            : Whole(span.TotalDays, "d");
    }

    /// <summary>How long ago a moment was: <c>just now</c> within a minute (and for a moment ahead of the clock), then <c>3 min ago</c>.</summary>
    public static string Ago(DateTimeOffset then, DateTimeOffset now) =>
        now - then < JustNow ? "just now" : $"{Span(now - then)} ago";
}
