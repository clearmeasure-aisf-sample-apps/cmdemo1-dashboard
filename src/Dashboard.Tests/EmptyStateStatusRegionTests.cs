using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// Every empty-state message has role="status" so a refresh that leaves nothing to show is announced: the cluster
/// view, the events strip (#98), the app's own environment and deployable messages, the deployable section's
/// "no target" message and the runtime view's "no environment" message (#100). A guard walks every component and
/// refuses an empty-state paragraph without it, so the next one added cannot be forgotten.
/// </summary>
public class EmptyStateStatusRegionTests
{
    private const string EmptyParagraphTag = "<p class=\"empty\"";
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

    private static string AppComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "App.razor"));

    private static string DeployableSectionComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "DeployableSection.razor"));

    private static string RuntimeViewComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "RuntimeView.razor"));

    private static string DashboardSourceDirectory([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard"));

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

    [Fact]
    public void TheAppsEmptyStatesAreStatusRegions()
    {
        var text = File.ReadAllText(AppComponent());

        Assert.Contains(
            """<p class="empty" role="status">The topology has no environments.</p>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            """<p class="empty" role="status">This environment has no deployables in the topology.</p>""",
            text,
            StringComparison.Ordinal);
    }

    [Fact]
    public void TheDeployableSectionsEmptyStateIsAStatusRegion()
    {
        var text = File.ReadAllText(DeployableSectionComponent());

        // The message itself is a ternary across the next lines; only its opening tag announces it.
        Assert.Contains(
            """<p class="empty" role="status">@(Monitor.Topology.HasFrontDoor""",
            text,
            StringComparison.Ordinal);
    }

    [Fact]
    public void TheRuntimeViewsEmptyStateIsAStatusRegion()
    {
        var text = File.ReadAllText(RuntimeViewComponent());

        Assert.Contains(
            """<p class="empty" role="status">The deployment rendered no runtime diagram: system.json has no environment.</p>""",
            text,
            StringComparison.Ordinal);
    }

    [Fact]
    public void EveryEmptyStateParagraphIsAStatusRegion()
    {
        var offenders = new List<string>();

        foreach (var file in RazorFiles(DashboardSourceDirectory()))
        {
            var text = File.ReadAllText(file);

            for (var found = text.IndexOf(EmptyParagraphTag, StringComparison.Ordinal); found >= 0;
                 found = text.IndexOf(EmptyParagraphTag, found + EmptyParagraphTag.Length, StringComparison.Ordinal))
            {
                var end = text.IndexOf('>', found);
                var tag = end < 0 ? text[found..] : text[found..(end + 1)];

                if (!tag.Contains("role=\"status\"", StringComparison.Ordinal))
                {
                    var line = text.AsSpan(0, found).Count('\n') + 1;
                    offenders.Add($"{Path.GetFileName(file)}:{line}");
                }
            }
        }

        Assert.Empty(offenders);
    }

    /// <summary>Every component of the app, the generated files of a build left out.</summary>
    private static IEnumerable<string> RazorFiles(string root)
    {
        var skipped = new[] { Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar,
                              Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar };

        return Directory.EnumerateFiles(root, "*.razor", SearchOption.AllDirectories)
            .Where(file => !skipped.Any(where => file.Contains(where, StringComparison.Ordinal)));
    }
}
