using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// Empty slots in the check history strip explain themselves on hover; filled bars already name their check.
/// </summary>
public class HistoryStripTests
{
    private static string HistoryStripComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "HistoryStrip.razor"));

    [Fact]
    public void EmptySlotsExplainThemselvesOnHoverAndFilledBarsKeepTheirTitles()
    {
        var text = File.ReadAllText(HistoryStripComponent());

        Assert.Contains(
            """class="history__bar history__bar--empty" x="@x" y="@(Height - 2)" width="@BarWidth" height="2" rx="1">""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("<title>No check yet</title>", text, StringComparison.Ordinal);
        Assert.Contains("<title>@HistoryText.Describe(result, Zone)</title>", text, StringComparison.Ordinal);
        Assert.Contains("""aria-label="@HistoryText.Describe(History)">""", text, StringComparison.Ordinal);
    }
}
