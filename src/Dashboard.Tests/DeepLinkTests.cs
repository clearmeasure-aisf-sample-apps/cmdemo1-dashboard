using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// Every deep link opens in a new tab. The title says so, but screen readers do not reliably announce titles, so the
/// link's accessible name must include hidden text after the visible content (no aria-label, which would replace it).
/// </summary>
public class DeepLinkTests
{
    private static string DeepLinkComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "DeepLink.razor"));

    [Fact]
    public void TheLinkBranchAnnouncesANewTabAfterItsVisibleContentAndThePlainBranchDoesNot()
    {
        var text = File.ReadAllText(DeepLinkComponent());
        var ifIndex = text.IndexOf("@if (Href", StringComparison.Ordinal);
        var elseIndex = text.IndexOf("else", ifIndex, StringComparison.Ordinal);
        Assert.True(ifIndex >= 0 && elseIndex > ifIndex);

        var linkBranch = text[ifIndex..elseIndex];
        var plainBranch = text[elseIndex..];

        const string Ordered =
            """@ChildContent<span class="visually-hidden"> (opens in a new tab)</span>""";
        var open = linkBranch.IndexOf("<a ", StringComparison.Ordinal);
        var close = linkBranch.IndexOf("</a>", StringComparison.Ordinal);
        Assert.True(open >= 0 && close > open);
        var anchor = linkBranch[open..(close + "</a>".Length)];
        Assert.Contains(Ordered, anchor, StringComparison.Ordinal);
        Assert.Contains("""target="_blank""", anchor, StringComparison.Ordinal);
        Assert.Contains("""rel="noopener""", anchor, StringComparison.Ordinal);
        Assert.Contains("""title="@Title""", anchor, StringComparison.Ordinal);
        Assert.Contains("href=\"@address.AbsoluteUri\"", anchor, StringComparison.Ordinal);
        Assert.Contains("""class="deep @Class""", anchor, StringComparison.Ordinal);
        Assert.DoesNotContain("aria-label", linkBranch, StringComparison.Ordinal);
        Assert.DoesNotContain("visually-hidden", plainBranch, StringComparison.Ordinal);
    }
}
