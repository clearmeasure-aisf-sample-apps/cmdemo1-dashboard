using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The Generate traffic button explains itself only in its title, which screen readers do not reliably announce. A
/// visually-hidden description linked with aria-describedby keeps the visible text as the accessible name.
/// The Environment select is disabled while traffic runs; its title then says why, and is absent when idle. The Stop
/// traffic button and that lock are described the same way, the lock's span rendered only while traffic runs so the
/// select never points at an id that is not in the page.
/// </summary>
public class TrafficPanelTests
{
    private static string TrafficPanelComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "TrafficPanel.razor"));

    private static string AppComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "App.razor"));

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
            """<select value="@_environment" disabled="@Running" title="@(Running ? EnvironmentLockedHelp : null)" aria-describedby="@(Running ? "traffic-environment-locked-help" : null)" @onchange="OnEnvironmentChanged">""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("<span>Environment</span>", text, StringComparison.Ordinal);

        // The hidden description is there only while the lock is on, so the select never points at an absent id.
        Assert.Contains(
            """<span id="traffic-environment-locked-help" class="visually-hidden">@EnvironmentLockedHelp</span>""",
            text,
            StringComparison.Ordinal);
        var locked = text.IndexOf("id=\"traffic-environment-locked-help\"", StringComparison.Ordinal);
        var running = text.LastIndexOf("@if (Running)", locked, StringComparison.Ordinal);
        Assert.True(running >= 0);
        Assert.Equal(1, CountOccurrences(text, "id=\"traffic-environment-locked-help\""));
        Assert.Equal(1, CountOccurrences(text, "aria-describedby=\"@(Running ? \"traffic-environment-locked-help\" : null)\""));
    }

    [Fact]
    public void TheStopTrafficButtonExplainsWhatItDoesOnHover()
    {
        var text = File.ReadAllText(TrafficPanelComponent());
        const string Button =
            """<button type="button" class="button" disabled="@(!_count.Sending)" title="@StopHelp" aria-describedby="traffic-stop-help" @onclick="StopAsync">Stop traffic</button>""";

        Assert.Contains(
            """private const string StopHelp = "Stops sending requests now; answers already on their way still count";""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(Button, text, StringComparison.Ordinal);
        Assert.DoesNotContain("aria-label", Button, StringComparison.Ordinal);
    }

    [Fact]
    public void TheStopTrafficButtonIsDescribedByAVisuallyHiddenHelpSpan()
    {
        var text = File.ReadAllText(TrafficPanelComponent());

        Assert.Contains(
            """<span id="traffic-stop-help" class="visually-hidden">@StopHelp</span>""",
            text,
            StringComparison.Ordinal);

        // The span sits with the button, which only the running branch renders: the id appears once and while it is used.
        var span = text.IndexOf("id=\"traffic-stop-help\"", StringComparison.Ordinal);
        var button = text.IndexOf("aria-describedby=\"traffic-stop-help\"", StringComparison.Ordinal);
        var generate = text.IndexOf("@if (Running)", StringComparison.Ordinal);
        var elseBranch = text.IndexOf("\n        else\n", generate, StringComparison.Ordinal);
        Assert.True(generate >= 0 && elseBranch > generate);
        Assert.True(button > generate && button < span && span < elseBranch);
        Assert.Equal(1, CountOccurrences(text, "id=\"traffic-stop-help\""));
        Assert.Equal(1, CountOccurrences(text, "aria-describedby=\"traffic-stop-help\""));
    }

    [Fact]
    public void TheHelpIdsOfTheTrafficPanelAreUniqueAndEveryDescribedByNamesOne()
    {
        var text = File.ReadAllText(TrafficPanelComponent());

        foreach (var id in new[] { "traffic-generate-help", "traffic-stop-help", "traffic-environment-locked-help" })
        {
            Assert.Equal(1, CountOccurrences(text, $"id=\"{id}\""));

            // The lock is named conditionally, the buttons plainly: either way the control points at the span.
            var plain = CountOccurrences(text, $"aria-describedby=\"{id}\"");
            var conditional = CountOccurrences(text, $"aria-describedby=\"@(Running ? \"{id}\" : null)\"");
            Assert.Equal(1, plain + conditional);
        }

        // The panel is one section of the page: its ids are not the header's, and the header's are not its own
        // (see HeaderControlHelpTests, which counts the same ids in App.razor).
        var header = File.ReadAllText(AppComponent());

        foreach (var id in new[] { "traffic-generate-help", "traffic-stop-help", "traffic-environment-locked-help" })
        {
            Assert.DoesNotContain($"id=\"{id}\"", header, StringComparison.Ordinal);
        }

        foreach (var id in new[] { "interval-help", "probe-help", "pause-help" })
        {
            Assert.DoesNotContain($"id=\"{id}\"", text, StringComparison.Ordinal);
        }
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        for (var index = 0; (index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0; index += value.Length)
        {
            count++;
        }

        return count;
    }
}
