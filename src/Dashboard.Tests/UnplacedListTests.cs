using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The unplaced resources Address link opens in a new tab. The title says so, but screen readers do not reliably
/// announce titles, so the link's accessible name must include hidden text after the visible host (no aria-label,
/// which would replace it).
/// </summary>
public class UnplacedListTests
{
    private static string UnplacedListComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "UnplacedList.razor"));

    [Fact]
    public void TheAddressLinkAnnouncesANewTabAfterItsVisibleHost()
    {
        var text = File.ReadAllText(UnplacedListComponent());

        const string Ordered =
            """@site.Host<span class="visually-hidden"> (opens in a new tab)</span>""";
        var open = text.IndexOf("""<a class="deep" href="@site.AbsoluteUri" """, StringComparison.Ordinal);
        var close = text.IndexOf("</a>", open, StringComparison.Ordinal);
        Assert.True(open >= 0 && close > open);
        var anchor = text[open..(close + "</a>".Length)];
        Assert.Contains(Ordered, anchor, StringComparison.Ordinal);
        Assert.Contains("""href="@site.AbsoluteUri" """, anchor, StringComparison.Ordinal);
        Assert.Contains("""target="_blank" """, anchor, StringComparison.Ordinal);
        Assert.Contains("""rel="noopener" """, anchor, StringComparison.Ordinal);
        Assert.Contains("""class="deep" """, anchor, StringComparison.Ordinal);
        Assert.Contains("title=\"@($\"{entry.Name} at {site.Host} (opens in a new tab)\")\"", anchor, StringComparison.Ordinal);
        Assert.DoesNotContain("aria-label", anchor, StringComparison.Ordinal);
    }
}
