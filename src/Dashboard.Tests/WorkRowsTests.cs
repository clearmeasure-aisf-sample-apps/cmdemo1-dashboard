using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// Each pull request's "checks" link must name that pull request in its accessible name (WCAG 2.4.4 Link Purpose),
/// so a screen-reader user's links list can tell many identical "checks" links apart.
/// </summary>
public class WorkRowsTests
{
    /// <summary>The words a screen reader hears after the visible text of a link that opens a new tab (#134).</summary>
    private const string NewTabHint = """<span class="visually-hidden"> (opens in a new tab)</span>""";

    private static string WorkRowsComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "WorkRows.razor"));

    [Fact]
    public void TheChecksLinkNameStartsWithChecksMentionsThePullNumberAndEndsWithOpensInANewTab()
    {
        var text = File.ReadAllText(WorkRowsComponent());
        const string Visible = "checks";
        const string Name = "checks of pull request #{n} (opens in a new tab)";

        Assert.Contains(
            """"aria-label="@(pull.Number is { } n ? $"checks of pull request #{n} (opens in a new tab)" : "checks (opens in a new tab)")"""",
            text,
            StringComparison.Ordinal);
        Assert.Contains($"""{Visible}{NewTabHint}</a>""", text, StringComparison.Ordinal);
        Assert.Contains("pull.Number", text, StringComparison.Ordinal);
        Assert.StartsWith(Visible, Name, StringComparison.Ordinal);
        Assert.EndsWith("(opens in a new tab)", Name, StringComparison.Ordinal);
    }

    /// <summary>
    /// The blocker status link opens GitHub in a new tab; its accessible name must start with the visible text
    /// (blocker.Text ?? "Status") and end with "(opens in a new tab)" so screen readers announce that.
    /// </summary>
    [Fact]
    public void TheBlockerLinkNameStartsWithItsVisibleTextAndEndsWithOpensInANewTab()
    {
        var text = File.ReadAllText(WorkRowsComponent());
        const string Visible = "blocker.Text ?? \"Status\"";
        const string Name = "{blocker.Text ?? \"Status\"} (opens in a new tab)";

        Assert.Contains(
            """"aria-label="@($"{blocker.Text ?? "Status"} (opens in a new tab)")"""",
            text,
            StringComparison.Ordinal);
        Assert.Contains($"@({Visible}){NewTabHint}</a>", text, StringComparison.Ordinal);
        Assert.Contains(Visible, Name, StringComparison.Ordinal);
        Assert.EndsWith("(opens in a new tab)", Name, StringComparison.Ordinal);
    }
}
