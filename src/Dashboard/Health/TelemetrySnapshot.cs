using System.Text.Json;

namespace Dashboard.Health;

/// <summary>
/// A node's own count of its calls over the last minute, as its telemetry endpoint (<c>/_telemetry</c>) reports them:
/// the counters the app keeps in its process, so the numbers are live and cost nothing outside it.
/// </summary>
/// <param name="Requests">Traffic: requests that are not checks, per minute.</param>
/// <param name="FromFrontDoor">Of those, the ones Front Door forwarded (header <c>X-Azure-FDID</c>).</param>
/// <param name="Direct">Of those, the ones to the web app's own address.</param>
/// <param name="Errors">Traffic answered with HTTP 500 or more.</param>
/// <param name="P95Ms">The 95th percentile of the traffic's duration, in milliseconds; null without traffic.</param>
/// <param name="Probes">The checks of dashboards and diagnostics (paths such as <c>/_healthcheck</c>).</param>
/// <param name="FrontDoorProbes">Front Door's health probes of this origin.</param>
/// <param name="Sql">SQL commands the process ran, whatever caused them.</param>
/// <param name="SqlP95Ms">The 95th percentile of their duration, in milliseconds; null without one.</param>
/// <param name="Http">Outgoing HTTP calls.</param>
/// <param name="ReadAt">When the dashboard read it.</param>
public sealed record TelemetrySnapshot(
    int Requests,
    int FromFrontDoor,
    int Direct,
    int Errors,
    int? P95Ms,
    int Probes,
    int FrontDoorProbes,
    int Sql,
    int? SqlP95Ms,
    int Http,
    DateTimeOffset ReadAt)
{
    /// <summary>The process's own vitals (<c>process</c>); null for an app that does not report them.</summary>
    public ProcessVitals? Process { get; init; }

    /// <summary>When the process started (<c>startedAt</c>): a later one than before is a restart.</summary>
    public DateTimeOffset? StartedAt { get; init; }

    /// <summary>Of the SQL commands, those run while handling an HTTP request (<c>sql.requests</c>); null for an app that does not tell them apart.</summary>
    public int? SqlRequests { get; init; }

    /// <summary>Of the SQL commands, all the others (<c>sql.background</c>): mostly the message bus polling the database.</summary>
    public int? SqlBackground { get; init; }

    /// <summary>True when the app tells the SQL commands of its requests from those in the background.</summary>
    public bool SplitsSql => SqlRequests is not null && SqlBackground is not null;

    /// <summary>
    /// The SQL commands the traffic causes: those of the requests where the app tells them apart, and all of them for
    /// an app that does not (an older one).
    /// </summary>
    public int SqlOfTraffic => SplitsSql ? SqlRequests!.Value : Sql;

    // The words of the node's /_telemetry answer, named once here so a mistype in one
    // section cannot silently read 0 while the true word sits untyped elsewhere.

    private const string RequestsSection = "requests";
    private const string ProbesSection = "probes";
    private const string SqlSection = "sql";
    private const string HttpSection = "http";
    private const string PerMinute = "perMinute";
    private const string FrontDoor = "frontDoor";
    private const string DirectWord = "direct";
    private const string ErrorsWord = "errors";
    private const string P95 = "p95Ms";
    private const string SqlRequestsWord = "requests";
    private const string Background = "background";
    private const string StartedAtWord = "startedAt";

    /// <summary>The endpoint's answer; null when it is not the expected JSON (an older app, an error page).</summary>
    public static TelemetrySnapshot? Parse(string json, DateTimeOffset readAt)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (JsonRead.Section(root, RequestsSection) is not { } requests)
            {
                return null;
            }

            var probes = JsonRead.Section(root, ProbesSection);
            var sql = JsonRead.Section(root, SqlSection);
            var http = JsonRead.Section(root, HttpSection);
            return new TelemetrySnapshot(
                Count(requests, PerMinute),
                Count(requests, FrontDoor),
                Count(requests, DirectWord),
                Count(requests, ErrorsWord),
                JsonRead.SaturatedCount(requests, P95),
                Count(probes, PerMinute),
                Count(probes, FrontDoor),
                Count(sql, PerMinute),
                JsonRead.SaturatedCount(sql, P95),
                Count(http, PerMinute),
                readAt)
            {
                Process = ProcessVitals.Read(root),
                SqlRequests = JsonRead.SaturatedCount(sql, SqlRequestsWord),
                SqlBackground = JsonRead.SaturatedCount(sql, Background),
                StartedAt = JsonRead.Time(root, StartedAtWord),
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>A count the record holds as a plain <see cref="int" />: a missing one is a zero, as the nullable reads stay null.</summary>
    private static int Count(JsonElement? section, string word) => JsonRead.SaturatedCount(section, word) ?? 0;
}
