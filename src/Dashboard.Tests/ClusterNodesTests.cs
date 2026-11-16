using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The Nodes table must share the section heading as its accessible name (via aria-labelledby), matching the Pods
/// table pattern, so screen-reader table navigation announces "Nodes" rather than an unnamed table.
/// Kubelet and Age headers explain themselves on hover, matching the CPU / Memory / Pods title style.
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

    [Fact]
    public void TheKubeletAndAgeHeadersHaveTitlesAndKeepTheirVisibleText()
    {
        var text = File.ReadAllText(ClusterNodesComponent());

        Assert.Contains(
            """<th scope="col" title="The Kubernetes version the node runs">Kubelet</th>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            """<th scope="col" title="How long ago the node joined the cluster">Age</th>""",
            text,
            StringComparison.Ordinal);
    }
}
