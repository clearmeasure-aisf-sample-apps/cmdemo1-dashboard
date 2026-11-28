namespace Dashboard.Tests;

/// <summary>
/// The three ended-deployment sentences live in one place (<see cref="DeploymentText.Ended"/>): the mark's sentence
/// (<see cref="DeploymentText.Sentence"/>) and the last-deployment mark's title (<see cref="DeploymentActivity"/>'s
/// <c>Sentence</c>, read through <see cref="DeploymentActivity.OfNode"/>) say them alike, word for word, for the same
/// project, release, environment and age. This file pins that agreement through the public entry points: a
/// <see cref="Deployment"/> for the mark's sentence, and a <c>recent</c> entry for the last-deployment title.
/// </summary>
public class DeploymentEndedSentenceAgreementTests
{
    /// <summary>Friday noon, UTC: the moment the page is asked.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 9, 12, 0, 0, TimeSpan.Zero);

    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    private const string Project = "demo-ui";
    private const string Release = "2.4.43";
    private const string Environment = "uat";
    private const string Slug = "demo";

    /// <summary>A deployment that ended five hours before the page is asked.</summary>
    private static readonly DateTimeOffset Finished = new(2026, 10, 9, 7, 0, 0, TimeSpan.Zero);

    /// <summary>The mark's sentence, through the public <see cref="DeploymentText.Sentence"/>.</summary>
    private static string MarkSentence(DeploymentState state, DateTimeOffset? finished)
    {
        var deployment = new Deployment(Project, Environment, Release, state, state.ToString().ToLowerInvariant(), finished?.AddMinutes(-10), finished, null);
        return DeploymentText.Sentence(deployment, Now);
    }

    /// <summary>The last-deployment sentence, read out of the <see cref="ActivityLine.Title"/> produced by <see cref="DeploymentActivity.OfNode"/>.</summary>
    private static string LastDeploymentSentence(string result)
    {
        var report = DeploymentsReport.Parse(
            $$"""{ "deployments": [], "recent": [ { "project": "{{Project}}", "release": "{{Release}}", "environment": "{{Environment}}", "result": "{{result}}", "finished": "{{Finished:yyyy-MM-ddTHH:mm:ssZ}}" } ], "freezes": [], "missing": [] }""")!;
        var lines = DeploymentActivity.OfNode(report, Environment, Slug, "ui", Now, Utc);
        var title = Assert.Single(lines).Title;

        // The title is "The last deployment of {project} to {environment} that ended: {sentence} ({date})."
        // Extract the sentence between "that ended: " and " (".
        const string Prefix = "that ended: ";
        var start = title.IndexOf(Prefix, StringComparison.Ordinal) + Prefix.Length;
        var end = title.IndexOf(" (", start, StringComparison.Ordinal);
        return title[start..end];
    }

    [Theory]
    [InlineData(DeploymentState.Succeeded, "demo-ui 2.4.43 reached uat 5 h ago")]
    [InlineData(DeploymentState.Failed, "demo-ui 2.4.43 failed in uat 5 h ago")]
    [InlineData(DeploymentState.Canceled, "demo-ui 2.4.43 was canceled in uat 5 h ago")]
    public void TheMarkSentenceIsTheExpectedSentence(DeploymentState state, string expected) =>
        Assert.Equal(expected, MarkSentence(state, Finished));

    [Theory]
    [InlineData(DeploymentState.Succeeded, "demo-ui 2.4.43 reached uat")]
    [InlineData(DeploymentState.Failed, "demo-ui 2.4.43 failed in uat")]
    [InlineData(DeploymentState.Canceled, "demo-ui 2.4.43 was canceled in uat")]
    public void TheMarkSentenceWithoutFinishedSaysNoAge(DeploymentState state, string expected) =>
        Assert.Equal(expected, MarkSentence(state, null));

    [Theory]
    [InlineData("succeeded")]
    [InlineData("failed")]
    [InlineData("canceled")]
    public void TheLastDeploymentSentenceIsTheExpectedSentence(string result)
    {
        var expected = result switch
        {
            "succeeded" => "demo-ui 2.4.43 reached uat 5 h ago",
            "failed" => "demo-ui 2.4.43 failed in uat 5 h ago",
            _ => "demo-ui 2.4.43 was canceled in uat 5 h ago",
        };
        Assert.Equal(expected, LastDeploymentSentence(result));
    }

    [Theory]
    [InlineData(DeploymentState.Succeeded, "succeeded")]
    [InlineData(DeploymentState.Failed, "failed")]
    [InlineData(DeploymentState.Canceled, "canceled")]
    public void TheMarkSentenceAndTheLastDeploymentSentenceAgreeWordForWord(DeploymentState state, string result)
    {
        var mark = MarkSentence(state, Finished);
        var last = LastDeploymentSentence(result);
        Assert.Equal(mark, last);
    }
}
