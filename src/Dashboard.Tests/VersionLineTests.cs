using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The version line's Logs link must start its accessible name with the visible text "Requests in Logs"
/// (WCAG 2.5.3 Label in Name), so speech-input users can say that phrase to activate it.
/// </summary>
public class VersionLineTests
{
    private static string VersionLineComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "VersionLine.razor"));

    [Fact]
    public void TheRequestsInLogsLinkNameStartsWithItsVisibleTextAndEndsWithOpensInANewTab()
    {
        var text = File.ReadAllText(VersionLineComponent());
        const string Visible = "Requests in Logs";
        const string Name = "Requests in Logs of {DeployableName} in {EnvironmentName}, Application Insights (opens in a new tab)";

        Assert.Contains(
            """"aria-label="@($"Requests in Logs of {DeployableName} in {EnvironmentName}, Application Insights (opens in a new tab)")"""",
            text,
            StringComparison.Ordinal);
        Assert.Contains($">{Visible}</a>", text, StringComparison.Ordinal);
        Assert.StartsWith(Visible, Name, StringComparison.Ordinal);
        Assert.EndsWith("(opens in a new tab)", Name, StringComparison.Ordinal);
    }
}
