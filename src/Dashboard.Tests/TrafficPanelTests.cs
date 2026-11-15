using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The Generate traffic button explains itself only in its title, which screen readers do not reliably announce. A
/// visually-hidden description linked with aria-describedby keeps the visible text as the accessible name.
/// The Environment select is disabled while traffic runs; its title then says why, and is absent when idle.
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

    [Fact]
    public void TheEnvironmentSelectExplainsWhyItIsLockedOnlyWhileTrafficRuns()
    {
        var text = File.ReadAllText(TrafficPanelComponent());

        Assert.Contains(
            """private const string EnvironmentLockedHelp = "Stop traffic to choose another environment";""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            """<select value="@_environment" disabled="@Running" title="@(Running ? EnvironmentLockedHelp : null)" @onchange="OnEnvironmentChanged">""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("<span>Environment</span>", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheStopTrafficButtonExplainsWhatItDoesOnHover()
    {
        var text = File.ReadAllText(TrafficPanelComponent());
        const string Button =
            """<button type="button" class="button" disabled="@(!_count.Sending)" title="@StopHelp" @onclick="StopAsync">Stop traffic</button>""";

        Assert.Contains(
            """private const string StopHelp = "Stops sending requests now; answers already on their way still count";""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(Button, text, StringComparison.Ordinal);
        Assert.DoesNotContain("aria-label", Button, StringComparison.Ordinal);
    }
}
