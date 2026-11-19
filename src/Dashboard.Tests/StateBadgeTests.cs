using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The badge is the page's word for a state, and the class is its machine form: "state state--&lt;lowercase state&gt;".
/// StateBadge draws the StateIcon and HealthClassifier's word for its state. ClusterBadge draws the caller's word
/// instead, drops the pill in a table's row (state--quiet, only while Quiet), and draws a bar for the neutral states
/// where StateIcon has no shape to say "neither good nor bad".
/// </summary>
public class StateBadgeTests
{
    private static string StateBadgeComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "StateBadge.razor"));

    private static string ClusterBadgeComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "ClusterBadge.razor"));

    [Fact]
    public void TheStateBadgeClassIsTheLowercasedStateNameAndTheLabelIsTheClassifiersWord()
    {
        var text = File.ReadAllText(StateBadgeComponent());

        Assert.Contains("""<span class="state state--@State.ToString().ToLowerInvariant()">""", text, StringComparison.Ordinal);
        Assert.Contains("<StateIcon State=\"State\" />", text, StringComparison.Ordinal);
        Assert.Contains("""<span class="state__label">@HealthClassifier.Label(State)</span>""", text, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(HealthState.Pending, "pending", "Checking")]
    [InlineData(HealthState.Healthy, "healthy", "Healthy")]
    [InlineData(HealthState.Unhealthy, "unhealthy", "Unhealthy")]
    [InlineData(HealthState.Unreachable, "unreachable", "Unreachable")]
    public void EveryHealthStateHasALowercasedClassAndTheClassifierSaysItsWord(HealthState state, string expectedCss, string label)
    {
        // The component builds the class from the state's name at render time; its CSS name is the lowercase word.
        Assert.Equal($"state state--{expectedCss}", $"state state--{state.ToString().ToLowerInvariant()}");
        Assert.Equal(label, HealthClassifier.Label(state));
    }

    [Fact]
    public void TheClusterBadgeClassIsTheLowercasedClusterState()
    {
        var text = File.ReadAllText(ClusterBadgeComponent());

        Assert.Contains("""<span class="state state--@Css @(Quiet ? "state--quiet" : null)">""", text, StringComparison.Ordinal);
        Assert.Contains("private string Css => State.ToString().ToLowerInvariant();", text, StringComparison.Ordinal);
        Assert.Contains("[Parameter, EditorRequired]", text, StringComparison.Ordinal);
        Assert.Contains("public string Label { get; set; } = default!;", text, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(ClusterState.Checking, "checking")]
    [InlineData(ClusterState.Healthy, "healthy")]
    [InlineData(ClusterState.Warning, "warning")]
    [InlineData(ClusterState.Unhealthy, "unhealthy")]
    [InlineData(ClusterState.Unreachable, "unreachable")]
    [InlineData(ClusterState.Neutral, "neutral")]
    public void EveryClusterStateHasALowercasedClass(ClusterState state, string expectedCss)
    {
        // The class is the enum member's name: renaming a member would silently break the CSS.
        Assert.Equal(expectedCss, state.ToString().ToLowerInvariant());
    }

    [Fact]
    public void TheClusterBadgeShowsTheLabelsWord()
    {
        var text = File.ReadAllText(ClusterBadgeComponent());

        Assert.Contains("""<span class="state__label">@Label</span>""", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheQuietClassAppearsOnlyWhileTheQuietParameterIsSet()
    {
        var text = File.ReadAllText(ClusterBadgeComponent());

        Assert.Contains("""@(Quiet ? "state--quiet" : null)""", text, StringComparison.Ordinal);
        Assert.Contains("[Parameter]\n    public bool Quiet { get; set; }", text, StringComparison.Ordinal);

        // The pill drops in a table's row; the class exists only in that ternary, nowhere else.
        Assert.Equal(1, CountOccurrences(text, "state--quiet"));
    }

    [Fact]
    public void TheNeutralStateDrawsTheBarIconAndEveryOtherStateUsesStateIcon()
    {
        var text = File.ReadAllText(ClusterBadgeComponent());

        Assert.Contains("@if (State == ClusterState.Neutral)", text, StringComparison.Ordinal);
        Assert.Contains(
            """<svg class="state-icon state-icon--neutral" viewBox="0 0 16 16" width="16" height="16" aria-hidden="true" focusable="false">""",
            text,
            StringComparison.Ordinal);

        // The bar: a single horizontal mark. The hidden svg says the state by shape, not by colour.
        Assert.Contains("""<path class="state-icon__mark" d="M5 8h6" />""", text, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(text, "state-icon--neutral"));

        var neutral = text.IndexOf("@if (State == ClusterState.Neutral)", StringComparison.Ordinal);
        var elseBranch = text.IndexOf("\n    else\n", neutral, StringComparison.Ordinal);
        var code = text.IndexOf("@code {", neutral, StringComparison.Ordinal);
        Assert.True(neutral >= 0 && elseBranch > neutral && code > elseBranch);

        // The bar svg is the neutral branch's; StateIcon is the other branch's, so no state draws both.
        var bar = text.IndexOf("state-icon--neutral", neutral, StringComparison.Ordinal);
        Assert.True(bar > neutral && bar < elseBranch);
        var icon = text.IndexOf("<StateIcon State=\"Icon\" />", elseBranch, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(text, "<StateIcon State=\"Icon\" />"));
        Assert.True(icon > elseBranch && icon < code);
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        for (var index = 0; (index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0; index += value.Length)
        {
            count++;
        }

        return count;
    }
}
