using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The header's controls explain themselves in their titles, which screen readers do not reliably announce. Like the
/// traffic button (see <see cref="TrafficPanelTests"/>), each one is linked with aria-describedby to a
/// visually-hidden span carrying the same help constant, so the description is announced without changing the
/// visible text or the accessible name. The titles stay: they are what a mouse reader sees. The Pause button is one
/// of them, although its label changes to Resume: the help is the constant, not the text.
/// The topology-problem reload button points at the header's span because the header is always rendered, so the id
/// appears once in the page.
/// </summary>
public class HeaderControlHelpTests
{
    private static string AppComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "App.razor"));

    [Fact]
    public void TheIntervalSelectIsDescribedByAVisuallyHiddenHelpSpan()
    {
        var text = File.ReadAllText(AppComponent());

        Assert.Contains(
            """<select value="@IntervalSeconds" title="@IntervalHelp" aria-describedby="interval-help" @onchange="OnIntervalChanged">""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            """<span id="interval-help" class="visually-hidden">@IntervalHelp</span>""",
            text,
            StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(text, "id=\"interval-help\""));
    }

    [Fact]
    public void TheProbeSelectIsDescribedByAVisuallyHiddenHelpSpan()
    {
        var text = File.ReadAllText(AppComponent());

        Assert.Contains(
            """<select value="@_probe" title="@ProbeHelp" aria-describedby="probe-help" @onchange="OnProbeChanged">""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            """<span id="probe-help" class="visually-hidden">@ProbeHelp</span>""",
            text,
            StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(text, "id=\"probe-help\""));
    }

    [Fact]
    public void TheCheckNowButtonIsDescribedByAVisuallyHiddenHelpSpan()
    {
        var text = File.ReadAllText(AppComponent());

        const string Button =
            """<button type="button" class="button button--quiet" title="@CheckNowHelp" aria-describedby="check-now-help" @onclick="CheckNow">Check now</button>""";
        Assert.Contains(Button, text, StringComparison.Ordinal);
        Assert.Contains(
            """<span id="check-now-help" class="visually-hidden">@CheckNowHelp</span>""",
            text,
            StringComparison.Ordinal);
        Assert.DoesNotContain("aria-label", Button, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(text, "id=\"check-now-help\""));
    }

    [Fact]
    public void ThePauseButtonIsDescribedByAVisuallyHiddenHelpSpan()
    {
        var text = File.ReadAllText(AppComponent());

        // The title stays: it is what a mouse reader sees. The label changes between Pause and Resume, so the help is
        // the constant the button names, not its text.
        const string Button =
            """<button type="button" class="button" title="@PauseHelp" aria-describedby="pause-help" @onclick="TogglePause">""";
        Assert.Contains(Button, text, StringComparison.Ordinal);
        Assert.Contains("""@(_poller.IsPaused ? "Resume" : "Pause")""", text, StringComparison.Ordinal);
        Assert.Contains(
            """<span id="pause-help" class="visually-hidden">@PauseHelp</span>""",
            text,
            StringComparison.Ordinal);
        Assert.DoesNotContain("aria-label", Button, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(text, "id=\"pause-help\""));
        Assert.Equal(1, CountOccurrences(text, "aria-describedby=\"pause-help\""));
    }

    [Fact]
    public void TheHeaderReloadTopologyButtonIsDescribedByAVisuallyHiddenHelpSpan()
    {
        var text = File.ReadAllText(AppComponent());

        const string Button =
            """<button type="button" class="button button--quiet" title="@ReloadTopologyHelp" aria-describedby="reload-topology-help" disabled="@_loading" aria-busy="@(_loading ? "true" : null)" @onclick="ReloadTopologyAsync">@ReloadTopologyLabel</button>""";
        Assert.Contains(Button, text, StringComparison.Ordinal);
        Assert.Contains(
            """<span id="reload-topology-help" class="visually-hidden">@ReloadTopologyHelp</span>""",
            text,
            StringComparison.Ordinal);
        Assert.DoesNotContain("aria-label", Button, StringComparison.Ordinal);
    }

    [Fact]
    public void TheProblemReloadTopologyButtonIsDescribedByTheHeaderSpanBecauseTheHeaderIsAlwaysRendered()
    {
        var text = File.ReadAllText(AppComponent());

        const string Button =
            """<button type="button" class="button" title="@ReloadTopologyHelp" aria-describedby="reload-topology-help" disabled="@_loading" aria-busy="@(_loading ? "true" : null)" @onclick="ReloadTopologyAsync">@ReloadTopologyLabel</button>""";
        Assert.Contains(Button, text, StringComparison.Ordinal);

        // The header opens before any of it, unconditionally: its span is in the page whenever the problem shows.
        var header = text.IndexOf("""<header class="page-header">""", StringComparison.Ordinal);
        var main = text.IndexOf("<main>", header, StringComparison.Ordinal);
        Assert.True(header >= 0 && main > header);
        Assert.DoesNotContain("@if(", text[..header], StringComparison.Ordinal);
        Assert.DoesNotContain("@if (", text[..header], StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(text, "id=\"reload-topology-help\""));
        Assert.Equal(2, CountOccurrences(text, "aria-describedby=\"reload-topology-help\""));
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
