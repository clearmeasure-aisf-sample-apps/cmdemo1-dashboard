using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The cluster view's and events strip's empty-state messages have role="status" so a refresh that leaves nothing to
/// show is announced (#98).
/// </summary>
public class EmptyStateStatusRegionTests
{
    private static string PodTableComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "PodTable.razor"));

    private static string ClusterPodsComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "ClusterPods.razor"));

    private static string ClusterNodesComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "ClusterNodes.razor"));

    private static string ClusterViewComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "ClusterView.razor"));

    private static string EventsStripComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "EventsStrip.razor"));

    [Fact]
    public void ThePodTablesEmptyStatesAreStatusRegions()
    {
        var text = File.ReadAllText(PodTableComponent());

        Assert.Contains(
            """<p class="empty" role="status">The status file lists no namespace <code>@Group.Name</code>.</p>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            """<p class="empty" role="status">No pods.</p>""",
            text,
            StringComparison.Ordinal);
    }

    [Fact]
    public void TheClusterPodsEmptyStateIsAStatusRegion()
    {
        var text = File.ReadAllText(ClusterPodsComponent());

        Assert.Contains(
            """<p class="empty" role="status">The status file lists no namespace.</p>""",
            text,
            StringComparison.Ordinal);
    }

    [Fact]
    public void TheClusterNodesEmptyStateIsAStatusRegion()
    {
        var text = File.ReadAllText(ClusterNodesComponent());

        Assert.Contains(
            """<p class="empty" role="status">The status file lists no node.</p>""",
            text,
            StringComparison.Ordinal);
    }

    [Fact]
    public void TheClusterViewEmptyStateIsAStatusRegion()
    {
        var text = File.ReadAllText(ClusterViewComponent());

        Assert.Contains(
            """<p class="empty" role="status">The topology names the cluster without an address to read: neither <code>statusUrl</code> nor <code>serviceUrl</code>.</p>""",
            text,
            StringComparison.Ordinal);
    }

    [Fact]
    public void TheEventsStripEmptyStateIsAStatusRegion()
    {
        var text = File.ReadAllText(EventsStripComponent());

        Assert.Contains(
            """<p class="empty" role="status">Nothing yet: every endpoint is as the first check found it.</p>""",
            text,
            StringComparison.Ordinal);
    }
}
