using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The work items board scope link opens in a new tab. The title says so, but screen readers do not reliably
/// announce titles, so the link's accessible name must include hidden text after the visible label (no aria-label,
/// which would replace it). The empty-state messages have role="status" so a filter that leaves nothing to show is
/// announced.
/// </summary>
public class WorkItemsViewTests
{
    private static string WorkItemsViewComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "WorkItemsView.razor"));

    [Fact]
    public void TheScopeLinkAnnouncesANewTabAfterItsVisibleLabel()
    {
        var text = File.ReadAllText(WorkItemsViewComponent());

        const string Ordered =
            """@linked<span class="visually-hidden"> (opens in a new tab)</span>""";
        var open = text.IndexOf("""<a href="@url.AbsoluteUri" """, StringComparison.Ordinal);
        var close = text.IndexOf("</a>", open, StringComparison.Ordinal);
        Assert.True(open >= 0 && close > open);
        var anchor = text[open..(close + "</a>".Length)];
        Assert.Contains(Ordered, anchor, StringComparison.Ordinal);
        Assert.Contains("""href="@url.AbsoluteUri" """, anchor, StringComparison.Ordinal);
        Assert.Contains("""target="_blank" """, anchor, StringComparison.Ordinal);
        Assert.Contains("""rel="noopener" """, anchor, StringComparison.Ordinal);
        Assert.Contains("title=\"@scope.Title\"", anchor, StringComparison.Ordinal);
        Assert.DoesNotContain("aria-label", anchor, StringComparison.Ordinal);
    }

    [Fact]
    public void TheEmptyStateMessagesAreStatusRegions()
    {
        var text = File.ReadAllText(WorkItemsViewComponent());

        Assert.Contains(
            """<p class="empty" role="status">@WorkItemsText.NoneOf(Audience)</p>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            """<p class="empty" role="status">@WorkItemsText.NoActive</p>""",
            text,
            StringComparison.Ordinal);
    }
}
