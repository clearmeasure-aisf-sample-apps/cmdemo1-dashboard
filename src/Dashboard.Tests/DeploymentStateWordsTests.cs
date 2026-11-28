using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The six values of the deployments file's <c>state</c> entry - <c>queued</c>, <c>executing</c>, <c>waiting</c>,
/// <c>succeeded</c>, <c>failed</c>, <c>canceled</c> - are named once on <c>Deployment</c> as private consts beside the
/// entry words, so a rename in the file's contract cannot silently read <c>DeploymentState.Unknown</c> in
/// <see cref="Deployment.Of" /> while the true word sits untyped in the switch.
/// <para>
/// The consts are private and the app declares no <c>InternalsVisibleTo</c>, so their text is pinned by reflection
/// under each name (as <see cref="DeploymentEntryWordsTests" /> does for the shared entry words); the mapping is
/// pinned through <see cref="DeploymentsReport.Parse" />, which reads a <c>deployments</c> entry built with each
/// word and checks that the record carries the state <see cref="Deployment.Of" /> means.
/// </para>
/// <para>
/// <see cref="DeploymentStateWordEdgeCaseTests" /> already pins the seven words in any case and culture, near-miss
/// words, and non-text states through the same public path; these tests pin the consts themselves and the
/// case-insensitivity that <see cref="Deployment.Of" /> gives the six, plus one <c>recent</c> entry through
/// <c>RecentDeployment.ResultOf</c>.
/// </para>
/// </summary>
public class DeploymentStateWordsTests
{
    /// <summary>The text of one private const of <see cref="Deployment" />, reached by reflection under its own name.</summary>
    private static string StateOf(string name)
    {
        var field = typeof(Deployment).GetField(name, BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(field);
        Assert.True(field!.IsLiteral, $"{name} is a const.");
        Assert.True(field.IsPrivate, $"{name} is private, so Of is the only reader.");
        Assert.Null(typeof(Deployment).GetField(name, BindingFlags.Public | BindingFlags.Static));
        return (string)field.GetRawConstantValue()!;
    }

    /// <summary>
    /// Each of the six state words is the text the file contract states, and is private: the entry words are
    /// internal or public, the state words are the file's values and belong to <see cref="Deployment.Of" /> alone.
    /// </summary>
    [Theory]
    [InlineData("QueuedState", "queued")]
    [InlineData("ExecutingState", "executing")]
    [InlineData("WaitingState", "waiting")]
    [InlineData("SucceededState", "succeeded")]
    [InlineData("FailedState", "failed")]
    [InlineData("CanceledState", "canceled")]
    public void EachStateConstIsTheTextOfItsFileValue(string name, string word)
    {
        Assert.Equal(word, StateOf(name));
    }

    /// <summary>
    /// Each of the six words read as the file's <c>state</c> means its state: the record carries the state
    /// <see cref="Deployment.Of" /> means, and the file's own word is kept as written.
    /// </summary>
    [Theory]
    [InlineData("queued", DeploymentState.Queued)]
    [InlineData("executing", DeploymentState.Executing)]
    [InlineData("waiting", DeploymentState.Waiting)]
    [InlineData("succeeded", DeploymentState.Succeeded)]
    [InlineData("failed", DeploymentState.Failed)]
    [InlineData("canceled", DeploymentState.Canceled)]
    public void EachFileValueMeansItsState(string word, DeploymentState expected)
    {
        var report = DeploymentsReport.Parse($$"""{ "deployments": [ { "project": "demo-ui", "environment": "uat", "release": "1", "state": "{{word}}" } ] }""")!;

        var deployment = Assert.Single(report.Deployments);
        Assert.Equal(expected, deployment.State);
        Assert.Equal(word, deployment.StateText);
    }

    /// <summary>
    /// The file's words are compared in the invariant culture after lower-casing: upper and mixed case mean the
    /// same state as the lower-case word, and the file's own word is kept as written.
    /// </summary>
    [Theory]
    [InlineData("Succeeded", DeploymentState.Succeeded)]
    [InlineData("FAILED", DeploymentState.Failed)]
    [InlineData("Queued", DeploymentState.Queued)]
    [InlineData("EXECUTING", DeploymentState.Executing)]
    [InlineData("Waiting", DeploymentState.Waiting)]
    [InlineData("CANCELED", DeploymentState.Canceled)]
    [InlineData("sUcCeEdEd", DeploymentState.Succeeded)]
    [InlineData("fAiLeD", DeploymentState.Failed)]
    public void AFileValueInAnyCaseMeansTheSameState(string word, DeploymentState expected)
    {
        var report = DeploymentsReport.Parse($$"""{ "deployments": [ { "project": "demo-ui", "environment": "uat", "release": "1", "state": "{{word}}" } ] }""")!;

        var deployment = Assert.Single(report.Deployments);
        Assert.Equal(expected, deployment.State);
        Assert.Equal(word, deployment.StateText);
    }

    /// <summary>
    /// A word that is not one of the six file values means <see cref="DeploymentState.Unknown" />, and the file's
    /// own word is kept as written.
    /// </summary>
    [Theory]
    [InlineData("paused")]
    [InlineData("queued?")]
    [InlineData("cancelled")]
    [InlineData("in progress")]
    public void AWordThatIsNoFileValueMeansUnknown(string word)
    {
        var report = DeploymentsReport.Parse($$"""{ "deployments": [ { "project": "demo-ui", "environment": "uat", "release": "1", "state": "{{word}}" } ] }""")!;

        var deployment = Assert.Single(report.Deployments);
        Assert.Equal(DeploymentState.Unknown, deployment.State);
        Assert.Equal(word, deployment.StateText);
    }

    /// <summary>
    /// The empty string, reached by <see cref="Deployment.Of" /> directly (the file's reader trims it to null before
    /// <see cref="Deployment.Read" /> asks <see cref="Deployment.Of" />), means <see cref="DeploymentState.Unknown" />.
    /// </summary>
    [Fact]
    public void TheEmptyStringMeansUnknown()
    {
        var method = typeof(Deployment).GetMethod("Of", BindingFlags.NonPublic | BindingFlags.Static)!;
        Assert.True(method.IsAssembly, "Of is internal.");
        var result = method.Invoke(null, [string.Empty]);
        Assert.Equal(DeploymentState.Unknown, result);
    }

    /// <summary>
    /// A <c>recent</c> entry whose result is a file value that is no end - <c>queued</c>, <c>executing</c> or
    /// <c>waiting</c> - is read by <c>RecentDeployment.ResultOf</c> as <see cref="DeploymentState.Unknown" />, the
    /// one decision the page makes of a result that is no end. A file value that is an end is read as its state.
    /// </summary>
    [Theory]
    [InlineData("succeeded", DeploymentState.Succeeded)]
    [InlineData("failed", DeploymentState.Failed)]
    [InlineData("canceled", DeploymentState.Canceled)]
    [InlineData("queued", DeploymentState.Unknown)]
    [InlineData("executing", DeploymentState.Unknown)]
    [InlineData("waiting", DeploymentState.Unknown)]
    public void ARecentEntryReadsItsResultThroughOf(string word, DeploymentState expected)
    {
        var report = DeploymentsReport.Parse($$"""{ "deployments": [], "recent": [ { "project": "demo-ui", "environment": "uat", "release": "1", "result": "{{word}}", "finished": "2026-10-09T07:00:00Z" } ] }""")!;

        var recent = Assert.Single(report.Recent);
        Assert.Equal(expected, recent.Result);
        Assert.Equal(word, recent.ResultText);
    }
}
