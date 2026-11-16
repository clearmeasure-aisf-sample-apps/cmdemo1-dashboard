using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The week chart legend keys explain Merged and Deployed to prod on hover without changing their visible text.
/// </summary>
public class WeekChartTests
{
    private static string WeekChartComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "WeekChart.razor"));

    [Fact]
    public void TheLegendKeysExplainThemselvesOnHoverAndKeepTheirVisibleText()
    {
        var text = File.ReadAllText(WeekChartComponent());

        Assert.Contains(
            """<span class="weeks__key weeks__key--merged" title="Work items merged that week">Merged</span>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            """<span class="weeks__key weeks__key--deployed" title="Work items deployed to prod that week">Deployed to prod</span>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(">Merged</span>", text, StringComparison.Ordinal);
        Assert.Contains(">Deployed to prod</span>", text, StringComparison.Ordinal);
    }
}
