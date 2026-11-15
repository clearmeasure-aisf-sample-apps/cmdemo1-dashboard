using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The header's system repository link opens in a new tab. The title says so, but screen readers do not reliably
/// announce titles, so the link's accessible name must include hidden text after the visible slug (no aria-label,
/// which would replace it). The Interval options spell out "seconds" so screen readers do not announce the letter s.
/// The polling-state span has role="status" so Live / Hidden / Paused changes are announced; Last refresh stays
/// outside it so every poll is not announced.
/// Last refresh shows the clock; its title carries the full date and time like every other time on the page.
/// Reload topology buttons share one label that becomes "Reloading topology…" with aria-busy while loading,
/// and a title that says they re-read the topology while Check now only re-checks health.
/// Check now has a title that says it checks every node at once without waiting for the interval.
/// Interval has a title that says how often every node is checked while the checks run.
/// </summary>
public class AppHeaderTests
{
    private static string AppComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "App.razor"));

    [Fact]
    public void TheSystemRepositoryLinkAnnouncesANewTabAfterItsVisibleSlug()
    {
        var text = File.ReadAllText(AppComponent());

        const string Ordered =
            """@slug<span class="visually-hidden"> (opens in a new tab)</span>""";
        var open = text.IndexOf("""<a href="@repository.AbsoluteUri" """, StringComparison.Ordinal);
        var close = text.IndexOf("</a>", open, StringComparison.Ordinal);
        Assert.True(open >= 0 && close > open);
        var anchor = text[open..(close + "</a>".Length)];
        Assert.Contains(Ordered, anchor, StringComparison.Ordinal);
        Assert.Contains("""href="@repository.AbsoluteUri" """, anchor, StringComparison.Ordinal);
        Assert.Contains("""target="_blank" """, anchor, StringComparison.Ordinal);
        Assert.Contains("rel=\"noopener\"", anchor, StringComparison.Ordinal);
        Assert.Contains("title=\"The system repository on GitHub (opens in a new tab)\"", anchor, StringComparison.Ordinal);
        Assert.DoesNotContain("aria-label", anchor, StringComparison.Ordinal);
    }

    [Fact]
    public void TheIntervalOptionsSpellOutSeconds()
    {
        var text = File.ReadAllText(AppComponent());

        Assert.Contains("""<option value="@seconds">@seconds seconds</option>""", text, StringComparison.Ordinal);
        Assert.DoesNotContain("@seconds s<", text, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePollingStateSpanIsAStatusRegionThatDoesNotIncludeLastRefresh()
    {
        var text = File.ReadAllText(AppComponent());

        const string Open = """<span class="polling polling--@PollingState.ToLowerInvariant()" role="status">""";
        var open = text.IndexOf(Open, StringComparison.Ordinal);
        Assert.True(open >= 0);
        var close = text.IndexOf("</span>", open, StringComparison.Ordinal);
        Assert.True(close > open);
        var span = text[open..(close + "</span>".Length)];
        Assert.Contains("role=\"status\"", span, StringComparison.Ordinal);
        Assert.Contains("class=\"polling polling--@PollingState.ToLowerInvariant()\"", span, StringComparison.Ordinal);
        Assert.Contains("@PollingState", span, StringComparison.Ordinal);
        Assert.DoesNotContain("Last refresh", span, StringComparison.Ordinal);
        Assert.DoesNotContain("<time", span, StringComparison.Ordinal);
    }

    [Fact]
    public void TheLastRefreshTimeCarriesTheFullDateAndTimeInItsTitle()
    {
        var text = File.ReadAllText(AppComponent());

        Assert.Contains(
            """<time datetime="@TimeText.Iso(refreshed)" title="@TimeText.DateAndTime(refreshed, Zone)">@TimeText.Clock(refreshed, Zone)</time>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("<span>not yet</span>", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCheckNowButtonHasATitleAndKeepsItsVisibleText()
    {
        var text = File.ReadAllText(AppComponent());

        const string Button =
            """<button type="button" class="button button--quiet" title="@CheckNowHelp" @onclick="CheckNow">Check now</button>""";
        Assert.Contains(Button, text, StringComparison.Ordinal);

        const string HelpOpen = "private const string CheckNowHelp =";
        var help = text.IndexOf(HelpOpen, StringComparison.Ordinal);
        Assert.True(help >= 0);
        var end = text.IndexOf(';', help);
        Assert.True(end > help);
        var value = text[(help + HelpOpen.Length)..end];
        Assert.False(string.IsNullOrWhiteSpace(value.Trim().Trim('"', '+')));
    }

    [Fact]
    public void TheIntervalSelectHasATitleAndKeepsItsVisibleLabel()
    {
        var text = File.ReadAllText(AppComponent());

        const string Select =
            """<select value="@IntervalSeconds" title="@IntervalHelp" @onchange="OnIntervalChanged">""";
        Assert.Contains(Select, text, StringComparison.Ordinal);
        Assert.Contains("<span>Interval</span>", text, StringComparison.Ordinal);

        const string HelpOpen = "private const string IntervalHelp =";
        var help = text.IndexOf(HelpOpen, StringComparison.Ordinal);
        Assert.True(help >= 0);
        var end = text.IndexOf(';', help);
        Assert.True(end > help);
        var value = text[(help + HelpOpen.Length)..end];
        Assert.False(string.IsNullOrWhiteSpace(value.Trim().Trim('"', '+')));
    }

    [Fact]
    public void TheReloadTopologyButtonsShareABusyLabelAndAriaBusyWhileLoading()
    {
        var text = File.ReadAllText(AppComponent());

        Assert.Contains(
            """private string ReloadTopologyLabel => _loading ? "Reloading topology…" : "Reload topology";""",
            text,
            StringComparison.Ordinal);

        const string Controls =
            """<button type="button" class="button button--quiet" title="@ReloadTopologyHelp" disabled="@_loading" aria-busy="@(_loading ? "true" : null)" @onclick="ReloadTopologyAsync">@ReloadTopologyLabel</button>""";
        const string Problem =
            """<button type="button" class="button" title="@ReloadTopologyHelp" disabled="@_loading" aria-busy="@(_loading ? "true" : null)" @onclick="ReloadTopologyAsync">@ReloadTopologyLabel</button>""";

        Assert.Contains(Controls, text, StringComparison.Ordinal);
        Assert.Contains(Problem, text, StringComparison.Ordinal);
        Assert.Equal(2, CountOccurrences(text, "@ReloadTopologyLabel</button>"));
        Assert.DoesNotContain(">Reload topology</button>", text, StringComparison.Ordinal);
        Assert.DoesNotContain(">Reloading topology…</button>", text, StringComparison.Ordinal);

        const string HelpOpen = "private const string ReloadTopologyHelp =";
        var help = text.IndexOf(HelpOpen, StringComparison.Ordinal);
        Assert.True(help >= 0);
        var end = text.IndexOf(';', help);
        Assert.True(end > help);
        var value = text[(help + HelpOpen.Length)..end];
        Assert.False(string.IsNullOrWhiteSpace(value.Trim().Trim('"', '+')));
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
