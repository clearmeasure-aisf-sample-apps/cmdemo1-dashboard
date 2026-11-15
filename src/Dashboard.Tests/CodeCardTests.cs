using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// Each Code card's "build run" link must name its deployable and environment in its accessible name
/// (WCAG 2.4.4 Link Purpose), so a screen-reader user's links list can tell many identical "build run" links apart.
/// </summary>
public class CodeCardTests
{
    private static string CodeCardComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "CodeCard.razor"));

    [Fact]
    public void TheBuildRunLinkNameStartsWithBuildRunUsesContextAndEndsWithOpensInANewTab()
    {
        var text = File.ReadAllText(CodeCardComponent());
        const string Visible = "build run";
        const string Name = "build run of {Context} (opens in a new tab)";

        Assert.Contains(
            """"aria-label="@($"build run of {Context} (opens in a new tab)")"""",
            text,
            StringComparison.Ordinal);
        Assert.Contains($">{Visible}</a>", text, StringComparison.Ordinal);
        Assert.Contains("{Context}", Name, StringComparison.Ordinal);
        Assert.StartsWith(Visible, Name, StringComparison.Ordinal);
        Assert.EndsWith("(opens in a new tab)", Name, StringComparison.Ordinal);
    }
}
