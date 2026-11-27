using Dashboard.Health;
using Entry = Dashboard.Runtime.RuntimePayloadBuilder.Entry;

namespace Dashboard.Runtime;

/// <summary>
/// The marks of the runtime diagram's regions, moved out of <see cref="RuntimePayloadBuilder"/> unchanged (#1830, step
/// 3 of #534). A region with no web app the topology has is not checked; a region whose apps all serve traffic is
/// serving; one whose apps are all healthy but none serves is standby; one still being checked is checking; and one
/// where no app is healthy or pending is not serving. The words of a region with no web app come from
/// <see cref="RuntimeRegionRole.Unprobed"/>, which names the roles in the page's words and says the database is
/// reachable when its health check answered.
/// </summary>
internal static class RuntimeRegionMarks
{
    /// <summary>
    /// The mark of one region of the diagram: the web apps the topology has in it, read through the index, and the
    /// state their health checks say. A region with no web app the topology has is not checked; one with web apps is
    /// judged by their health states and serving decision.
    /// </summary>
    internal static RuntimeRegionMark Region(RuntimeRegion region, RuntimeDiagramIndex<Entry> index, bool databaseReachable)
    {
        var apps = index.WebAppsIn(region.Alias)
            .Select(node => index.Entry(node.Alias))
            .ToList();
        if (apps.Count == 0)
        {
            return new RuntimeRegionMark(region.Alias, RegionState.Neutral.Word(), RuntimeRegionRole.Unprobed(region.Roles, databaseReachable));
        }

        // A web app the topology does not have is left out; a region with none the topology has is not checked.
        var known = apps.OfType<Entry>().ToList();
        return known.Count == 0 ? new RuntimeRegionMark(region.Alias, RegionState.Neutral.Word(), "not checked") : Region(region.Alias, known);
    }

    /// <summary>
    /// The mark of a region that has web apps the topology knows: serving if any app serves, standby if all are
    /// healthy but none serves, checking if any is still being checked, and down otherwise.
    /// </summary>
    private static RuntimeRegionMark Region(string alias, List<Entry> apps)
    {
        if (apps.Any(entry => entry.Serves))
        {
            return new RuntimeRegionMark(alias, RegionState.Serving.Word(), "serving traffic");
        }

        if (apps.All(entry => entry.Target.State == HealthState.Healthy))
        {
            return new RuntimeRegionMark(alias, RegionState.Standby.Word(), "standby: ready");
        }

        return apps.Any(entry => entry.Target.State == HealthState.Pending)
            ? new RuntimeRegionMark(alias, RegionState.Checking.Word(), "checking")
            : new RuntimeRegionMark(alias, RegionState.Down.Word(), "not serving");
    }
}
