namespace Dashboard.Health;

/// <summary>
/// When a reading is asked again: the one rule the dashboard's interval reads share. Nothing is due before it was first
/// asked, and once it was, it is due again only when its interval has passed since the last asking. The boundary is
/// inclusive, and stated here once: a read exactly one interval later goes ahead. The monitor's file reads
/// (<see cref="DashboardMonitor" />), the container app probe (<see cref="ContainerAppProbe" />, both when it is due and when a probe starts a new run) and the database
/// view's live-file read (<see cref="Runtime.DatabaseRefresh" />, beside the page's own <c>active</c>
/// gate) all compose it. The version-based reads (<see cref="BuildRead.IsDue(string?)" />,
/// <see cref="Runtime.ApplicationRefresh.IsDue" />) are a different rule and do not.
/// </summary>
internal static class IntervalDue
{
    /// <summary>
    /// True when nothing was asked yet, or at least <paramref name="interval"/> has passed since <paramref name="last"/>.
    /// The boundary is inclusive: a read exactly one interval later goes ahead.
    /// </summary>
    public static bool IsDue(DateTimeOffset? last, DateTimeOffset now, TimeSpan interval) =>
        last is not { } at || now - at >= interval;
}
