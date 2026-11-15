using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The Pods section's Workloads link must name the cluster and that it opens in a new tab in its accessible name
/// (WCAG 2.4.4 Link Purpose / 2.5.3 Label in Name), so screen readers hear more than the title alone.
/// </summary>
public class ClusterPodsTests
{
    private static string ClusterPodsComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "ClusterPods.razor"));

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
        Assert.Contains($">{Visible}</a>", text, StringComparison.Ordinal);
        Assert.Contains("{Name}", Name, StringComparison.Ordinal);
        Assert.StartsWith(Visible, Name, StringComparison.Ordinal);
        Assert.EndsWith("(opens in a new tab)", Name, StringComparison.Ordinal);
    }
}
