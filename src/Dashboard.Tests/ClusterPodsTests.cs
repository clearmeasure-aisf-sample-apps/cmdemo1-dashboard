using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The Pods section's Workloads link must name the cluster and that it opens in a new tab in its accessible name
/// (WCAG 2.4.4 Link Purpose / 2.5.3 Label in Name), so screen readers hear more than the title alone.
/// The pods table's State and Age headers explain themselves on hover like Restarts, CPU and Memory.
/// </summary>
public class ClusterPodsTests
{
    /// <summary>The words a screen reader hears after the visible text of a link that opens a new tab (#134).</summary>
    private const string NewTabHint = """<span class="visually-hidden"> (opens in a new tab)</span>""";

    private static string ClusterPodsComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "ClusterPods.razor"));

    private static string PodTableComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "PodTable.razor"));

    [Fact]
    public void TheWorkloadsLinkNameStartsWithItsVisibleTextUsesNameAndEndsWithOpensInANewTab()
    {
        var text = File.ReadAllText(ClusterPodsComponent());
        const string Visible = "Workloads in the Azure portal";
        const string Name = "Workloads in the Azure portal of {Name} (opens in a new tab)";

        Assert.Contains(
            """"aria-label="@($"Workloads in the Azure portal of {Name} (opens in a new tab)")"""",
            text,
            StringComparison.Ordinal);
        Assert.Contains($">{Visible}{NewTabHint}</a>", text, StringComparison.Ordinal);
        Assert.Contains("{Name}", Name, StringComparison.Ordinal);
        Assert.StartsWith(Visible, Name, StringComparison.Ordinal);
        Assert.EndsWith("(opens in a new tab)", Name, StringComparison.Ordinal);
    }

    [Fact]
    public void TheStateAndAgeHeadersExplainThemselvesOnHoverAndKeepTheirVisibleText()
    {
        var text = File.ReadAllText(PodTableComponent());

        Assert.Contains(
            """<th scope="col" title="Whether the pod is ready, not ready, or finished">State</th>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            """<th scope="col" title="How long ago the pod started">Age</th>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(">State</th>", text, StringComparison.Ordinal);
        Assert.Contains(">Age</th>", text, StringComparison.Ordinal);
    }
}
