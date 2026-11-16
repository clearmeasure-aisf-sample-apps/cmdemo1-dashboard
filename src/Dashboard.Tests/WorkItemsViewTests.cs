using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The work items board scope link opens in a new tab. The title says so, but screen readers do not reliably
/// announce titles, so the link's accessible name must include hidden text after the visible label (no aria-label,
/// which would replace it).
/// The "Whose work" filter buttons show a bare count; a visually-hidden " work items" after the count makes the
/// accessible name e.g. "Bot 12 work items" without an aria-label that would replace the visible text.
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
    public void TheWhoseWorkFilterButtonsAnnounceWorkItemsAfterTheCount()
    {
        var text = File.ReadAllText(WorkItemsViewComponent());

        const string Ordered =
            """<span class="work__count">@count</span><span class="visually-hidden"> work items</span>""";
        var open = text.IndexOf(
            """<button type="button" class="button @(Audience == audience ? "button--selected" : null)" aria-pressed=""",
            StringComparison.Ordinal);
        var close = text.IndexOf("</button>", open, StringComparison.Ordinal);
        Assert.True(open >= 0 && close > open);
        var button = text[open..(close + "</button>".Length)];
        Assert.Contains(Ordered, button, StringComparison.Ordinal);
        Assert.Contains(
            "aria-pressed=\"@(Audience == audience ? \"true\" : \"false\")\"",
            button,
            StringComparison.Ordinal);
        Assert.DoesNotContain("aria-label", button, StringComparison.Ordinal);
    }
}
