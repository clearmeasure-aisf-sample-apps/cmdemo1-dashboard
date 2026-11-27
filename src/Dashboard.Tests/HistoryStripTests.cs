using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The history strip's markup keeps its guard in one place: the empty-slot title reads the shared
/// <see cref="HistoryText.NoChecksYet" /> constant, and every <see cref="HealthState" /> member the enum
/// names has its own <c>BarHeight</c> arm, so a new state cannot be silently absorbed by a catch-all.
/// </summary>
public class HistoryStripTests
{
    private static string HistoryStripComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "HistoryStrip.razor"));

    [Fact]
    public void EmptySlotTitleReadsTheSharedNoChecksYetConstantAndFilledBarsKeepTheirTitles()
    {
        var text = File.ReadAllText(HistoryStripComponent());

        Assert.Contains(
            """<rect class="history__bar history__bar--empty" x="@x" y="@(Height - 2)" width="@BarWidth" height="2" rx="1">""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("<title>@HistoryText.NoChecksYet</title>", text, StringComparison.Ordinal);
        Assert.DoesNotContain("<title>No check</title>", text, StringComparison.Ordinal);
        Assert.Contains("<title>@HistoryText.Describe(result, Zone)</title>", text, StringComparison.Ordinal);
        Assert.Contains("aria-label=\"@HistoryText.Describe(History)\"", text, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryHealthStateTheEnumNamesHasItsOwnBarHeightArm()
    {
        var text = File.ReadAllText(HistoryStripComponent());

        // Each named arm is the full member qualification, so a substring match cannot be fooled by a
        // comment mentioning a state name in another form.
        Assert.Contains("HealthState.Healthy => Height", text, StringComparison.Ordinal);
        Assert.Contains("HealthState.Unhealthy => Height / 2", text, StringComparison.Ordinal);
        Assert.Contains("HealthState.Pending => 5", text, StringComparison.Ordinal);
        Assert.Contains("HealthState.Unreachable => 5", text, StringComparison.Ordinal);
        // The catch-all must throw, not pick a height, so a future state forces a decision.
        Assert.Contains("_ => throw new InvalidOperationException", text, StringComparison.Ordinal);
        Assert.DoesNotContain("_ => 5", text, StringComparison.Ordinal);
    }
}
