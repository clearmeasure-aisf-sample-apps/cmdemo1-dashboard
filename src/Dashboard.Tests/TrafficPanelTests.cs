using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The Generate traffic button explains itself only in its title, which screen readers do not reliably announce. A
/// visually-hidden description linked with aria-describedby keeps the visible text as the accessible name.
/// </summary>
public class TrafficPanelTests
{
    private static string TrafficPanelComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "TrafficPanel.razor"));

    [Fact]
    public void TheGenerateTrafficButtonIsDescribedByAVisuallyHiddenHelpSpan()
    {
        var text = File.ReadAllText(TrafficPanelComponent());
        const string Button =
            """<button type="button" class="button" disabled="@(Selected is null)" title="@ButtonHelp" aria-describedby="traffic-generate-help" @onclick="StartAsync">Generate traffic</button>""";

        Assert.Contains(Button, text, StringComparison.Ordinal);
        Assert.Contains(
            """<span id="traffic-generate-help" class="visually-hidden">@ButtonHelp</span>""",
            text,
            StringComparison.Ordinal);
        Assert.DoesNotContain("aria-label", Button, StringComparison.Ordinal);
    }
}
