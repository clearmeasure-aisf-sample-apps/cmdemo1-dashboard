using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The header's system repository link opens in a new tab. The title says so, but screen readers do not reliably
/// announce titles, so the link's accessible name must include hidden text after the visible slug (no aria-label,
/// which would replace it). The Interval options spell out "seconds" so screen readers do not announce the letter s.
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
}
