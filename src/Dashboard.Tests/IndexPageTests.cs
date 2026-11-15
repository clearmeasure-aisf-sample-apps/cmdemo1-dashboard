using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The top bar's Clear Measure logo link opens in a new tab. The title says so, but screen readers do not reliably
/// announce titles, so the link's accessible name must include hidden text after the logo img (no aria-label, which
/// would replace the alt text).
/// </summary>
public class IndexPageTests
{
    private static string IndexPage([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "wwwroot", "index.html"));

    [Fact]
    public void TheBrandLinkAnnouncesANewTabAfterTheLogoImage()
    {
        var text = File.ReadAllText(IndexPage());

        const string Logo =
            """<img src="clear-measure-logo-white.png" alt="Clear Measure" width="160" height="40" />""";
        const string Hidden =
            """<span class="visually-hidden"> (opens in a new tab)</span>""";
        var open = text.IndexOf("""<a class="topbar__brand" """, StringComparison.Ordinal);
        var close = text.IndexOf("</a>", open, StringComparison.Ordinal);
        Assert.True(open >= 0 && close > open);
        var anchor = text[open..(close + "</a>".Length)];
        var logoAt = anchor.IndexOf(Logo, StringComparison.Ordinal);
        var hiddenAt = anchor.IndexOf(Hidden, StringComparison.Ordinal);
        Assert.True(logoAt >= 0 && hiddenAt > logoAt);
        Assert.Contains("""href="https://www.clear-measure.com/" """, anchor, StringComparison.Ordinal);
        Assert.Contains("""target="_blank" """, anchor, StringComparison.Ordinal);
        Assert.Contains("""rel="noopener noreferrer" """, anchor, StringComparison.Ordinal);
        Assert.Contains("""class="topbar__brand" """, anchor, StringComparison.Ordinal);
        Assert.Contains("title=\"Clear Measure (opens in a new tab)\"", anchor, StringComparison.Ordinal);
        Assert.Contains("""alt="Clear Measure" """, anchor, StringComparison.Ordinal);
        Assert.DoesNotContain("aria-label", anchor, StringComparison.Ordinal);
    }
}
