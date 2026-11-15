using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The Nodes table must share the section heading as its accessible name (via aria-labelledby), matching the Pods
/// table pattern, so screen-reader table navigation announces "Nodes" rather than an unnamed table.
/// </summary>
public class ClusterNodesTests
{
    private static string ClusterNodesComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "ClusterNodes.razor"));

    [Fact]
    public void TheNodesTableIsNamedByTheNodesHeading()
    {
        var text = File.ReadAllText(ClusterNodesComponent());

        Assert.Contains("""id="cluster-nodes-title">""", text, StringComparison.Ordinal);
        Assert.Contains("""<table class="grid" aria-labelledby="cluster-nodes-title">""", text, StringComparison.Ordinal);
    }
}
