using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The cluster's copy of the one rule a detail is joined with the words it explains (#1167) delegates to
/// <see cref="Dashboard.Health.TextReason"/> (#1434): <c>ClusterText.Reason</c> and <c>ClusterText.Words</c> say
/// exactly what <c>TextReason.Reason</c> and <c>TextReason.Words</c> say, for the boundary values the rule was
/// settled on. Both are internal and the app declares no <c>InternalsVisibleTo</c>, so the test reaches them by
/// reflection, the way the app's other internals are pinned.
/// </summary>
public class ClusterTextDelegatesToTextReasonTests
{
    private static readonly MethodInfo ClusterReason =
        typeof(Dashboard.Cluster.ClusterText).GetMethod("Reason", BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo ClusterWords =
        typeof(Dashboard.Cluster.ClusterText).GetMethod("Words", BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly Type TextReasonType =
        typeof(Dashboard.Cluster.ClusterText).Assembly.GetType("Dashboard.Health.TextReason")!;

    private static readonly MethodInfo TextReasonReason =
        TextReasonType.GetMethod("Reason", BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo TextReasonWords =
        TextReasonType.GetMethod("Words", BindingFlags.NonPublic | BindingFlags.Static)!;

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" . ")]
    [InlineData(".")]
    [InlineData(". . .")]
    [InlineData("a. .")]
    [InlineData("Login failed.")]
    [InlineData("v1.2 failed")]
    public void ClusterReasonAndWordsMatchTextReason(string? detail)
    {
        Assert.Equal(TextReasonReason.Invoke(null, [detail]), ClusterReason.Invoke(null, [detail]));
        Assert.Equal(TextReasonWords.Invoke(null, [detail]), ClusterWords.Invoke(null, [detail]));
    }
}
