namespace Dashboard.Tests;

/// <summary>
/// The one delivery fixture the delivery tests share: the moment the last report that failed ended, the moment
/// the page is asked, the two health windows the page reads from the file, the <see cref="HealthReports" />
/// builder the tests use to build a report of the parts it is told, the <see cref="ContainerAppReports" />
/// builder, and the canonical four-environment <c>delivery.json</c> text with its parsed report.
/// <para>
/// Six test files re-declared the same failure moment and four wrote their own <c>delivery.json</c>; this is the
/// home those facts were waiting for. A test that wants its own numbers - a window at the ends of
/// <see cref="int" />, a moment one tick away, a file with a different <c>generated</c> - keeps its own value;
/// this is where the copies stop being word for word. The fixture carries the facts; each test keeps its
/// assertions where they are.
/// </para>
/// </summary>
public static class DeliveryFixtures
{
    /// <summary>
    /// The moment the last report that failed ended, on the meridian: the fixture's moment of
    /// <see cref="AvailabilityTests" />. Nine hours and seventeen minutes before <see cref="Now" />, so the line
    /// says <c>9 h ago</c> and not a whole number of hours.
    /// </summary>
    public static readonly DateTimeOffset Failed = new(2026, 10, 6, 16, 13, 0, TimeSpan.Zero);

    /// <summary>
    /// The very instant of <see cref="Failed" />, five hours east of the meridian: the same failure spelled in
    /// the hours the viewer speaks, pinned by <see cref="HealthReportsEdgeCaseTests" /> and
    /// <see cref="HealthReportsSurfaceEdgeCaseTests" />.
    /// </summary>
    public static readonly DateTimeOffset FailedInOtherHours = new(2026, 10, 6, 21, 13, 0, TimeSpan.FromHours(5));

    /// <summary>
    /// The moment the page is asked, and every "ago" the tests count from: a fixed moment, not the clock.
    /// Nine hours and seventeen minutes after <see cref="Failed" />.
    /// </summary>
    public static readonly DateTimeOffset Now = new(2026, 10, 7, 1, 30, 0, TimeSpan.Zero);

    /// <summary>
    /// The moment the delivery facts last changed, in the hours the file writes them in: the
    /// <c>generated</c> of the canonical <see cref="Delivery" /> text.
    /// </summary>
    public static readonly DateTimeOffset Generated = new(2026, 10, 7, 1, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// The window of the last 24 hours as the file gives it for the environment with a failure:
    /// 23 of 24 hourly checks healthy - not all, since one failed.
    /// </summary>
    public static readonly HealthWindow Day = new(24, 23);

    /// <summary>
    /// The window of the last seven days as the file gives it for the environment with a failure:
    /// 164 of 166 healthy.
    /// </summary>
    public static readonly HealthWindow Week = new(166, 164);

    /// <summary>
    /// A report of the parts it is told, the parts left out the way the file leaves them out. The default
    /// windows are the fixture's <see cref="Day" /> and <see cref="Week" />, the default failure is
    /// <see cref="Failed" />, and the apps are no apps.
    /// </summary>
    public static HealthReports Reports(
        HealthWindow? day = null,
        HealthWindow? week = null,
        DateTimeOffset? failure = null,
        IReadOnlyList<ContainerAppReports>? apps = null) =>
        new(day, week, failure, apps);

    /// <summary>
    /// The fixture's report: the two windows, the moment the failure ended, and no container apps - the report
    /// <see cref="AvailabilityTests" /> reads from the canonical <see cref="Delivery" /> text for the
    /// <c>prod</c> environment.
    /// </summary>
    public static HealthReports Prod => new(Day, Week, Failed);

    /// <summary>
    /// A container-app report of the parts it is told, the parts left out the way the file leaves them out.
    /// </summary>
    public static ContainerAppReports ContainerApp(
        string name,
        HourlyCheck? last = null,
        HealthWindow? day = null,
        HealthWindow? week = null) =>
        new(name, last, day, week);

    /// <summary>
    /// The canonical four-environment <c>delivery.json</c> text: <c>tdd</c> all healthy, <c>prod</c> with a
    /// failure nine hours before <see cref="Now" />, <c>uat</c> with no health section, and <c>dev</c> with no
    /// health section. The <c>generated</c> moment is <see cref="Generated" />.
    /// </summary>
    public const string Delivery = """
        { "generated": "2026-10-07T01:00:00Z",
          "environments": [
            { "name": "tdd", "deployables": [ { "name": "ui", "version": "2.4.15" } ],
              "health": { "last24Hours": { "reports": 24, "healthy": 24 }, "last7Days": { "reports": 168, "healthy": 168 }, "lastFailure": null } },
            { "name": "prod", "deployables": [],
              "health": { "last24Hours": { "reports": 24, "healthy": 23 }, "last7Days": { "reports": 166, "healthy": 164 }, "lastFailure": "2026-10-06T16:13:00Z" } },
            { "name": "uat", "deployables": [ { "name": "ui", "version": "2.4.14" } ], "health": null },
            { "name": "dev", "deployables": [ { "name": "ui", "version": "2.4.14" } ] } ] }
        """;

    /// <summary>
    /// The parsed report of the canonical <see cref="Delivery" /> text: four environments, no failover test.
    /// </summary>
    public static DeliveryReport Report => DeliveryReport.Parse(Delivery)!;
}
