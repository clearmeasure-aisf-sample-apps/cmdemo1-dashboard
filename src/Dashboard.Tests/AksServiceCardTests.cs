using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The AKS service card's Azure portal link opens in a new tab. The title says so, but screen readers do not
/// reliably announce titles, so the link's accessible name must include hidden text after the visible text (no
/// aria-label, which would replace it).
/// </summary>
public class AksServiceCardTests
{
    private static string AksServiceCardComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "AksServiceCard.razor"));

    [Fact]
    public void ThePortalLinkAnnouncesANewTabAfterItsVisibleText()
    {
        var text = File.ReadAllText(AksServiceCardComponent());

        const string Ordered =
            """AKS cluster in the Azure portal<span class="visually-hidden"> (opens in a new tab)</span>""";
        var open = text.IndexOf("""<a class="cluster-card__link" """, StringComparison.Ordinal);
        var close = text.IndexOf("</a>", open, StringComparison.Ordinal);
        Assert.True(open >= 0 && close > open);
        var anchor = text[open..(close + "</a>".Length)];
        Assert.Contains(Ordered, anchor, StringComparison.Ordinal);
        Assert.Contains("""href="@portal.AbsoluteUri" """, anchor, StringComparison.Ordinal);
        Assert.Contains("""target="_blank" """, anchor, StringComparison.Ordinal);
        Assert.Contains("""rel="noopener" """, anchor, StringComparison.Ordinal);
        Assert.Contains("""class="cluster-card__link" """, anchor, StringComparison.Ordinal);
        Assert.Contains("""title="@ClusterText.PortalTitle(Name)""", anchor, StringComparison.Ordinal);
        Assert.DoesNotContain("aria-label", anchor, StringComparison.Ordinal);
    }
}
