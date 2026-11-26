namespace Dashboard.Tests;

/// <summary>
/// The rule that settles a recent entry's result is named now: <c>ResultOf</c>, the one private helper of
/// <see cref="RecentDeployment" />, says that a recent entry's result is one of the three ways a deployment ends -
/// <c>succeeded</c>, <c>failed</c> or <c>canceled</c> in any casing - and is <see cref="DeploymentState.Unknown" />
/// for any other word, a state of a deployment in flight included. The helper is private and the app declares no
/// <c>InternalsVisibleTo</c>, so this pins the rule where the reading happens: through
/// <see cref="DeploymentsReport.Parse" />, one <c>recent</c> entry per word, each asserting the entry's
/// <c>Result</c> and <c>ResultText</c> - the file's own word is kept whatever the page makes of it.
/// </summary>
public class RecentDeploymentResultOfTests
{
    /// <summary>
    /// The three words that say a deployment ended, each in the casing the file writes, settle on their state and
    /// keep the word beside them.
    /// </summary>
    [Theory]
    [InlineData("succeeded", DeploymentState.Succeeded)]
    [InlineData("SUCCEEDED", DeploymentState.Succeeded)]
    [InlineData("Failed", DeploymentState.Failed)]
    [InlineData("failed", DeploymentState.Failed)]
    [InlineData("canceled", DeploymentState.Canceled)]
    [InlineData("CANCELED", DeploymentState.Canceled)]
    public void TheThreeEndedWordsSettleOnTheirStateAndKeepTheirWord(string word, DeploymentState expected)
    {
        var report = Parse(recent: $$"""{ "project": "demo-ui", "environment": "uat", "release": "2.4.43", "result": "{{word}}", "finished": "2026-10-09T07:00:00Z" }""");

        var entry = Assert.Single(report.Recent);

        Assert.Equal(expected, entry.Result);
        Assert.Equal(word, entry.ResultText);
    }

    /// <summary>
    /// A state of a deployment in flight is not a way it ended: the page settles the entry on
    /// <see cref="DeploymentState.Unknown" /> and keeps the file's own word, so the reason the word becomes
    /// <c>Unknown</c> is pinned by the word's name.
    /// </summary>
    [Theory]
    [InlineData("queued")]
    [InlineData("executing")]
    [InlineData("waiting")]
    public void AnInFlightWordIsSettledOnUnknownAndKept(string word)
    {
        var report = Parse(recent: $$"""{ "project": "demo-ui", "environment": "uat", "release": "2.4.43", "result": "{{word}}", "finished": "2026-10-09T07:00:00Z" }""");

        var entry = Assert.Single(report.Recent);

        Assert.Equal(DeploymentState.Unknown, entry.Result);
        Assert.Equal(word, entry.ResultText);
    }

    /// <summary>
    /// A word the page does not know is <c>Unknown</c> and kept beside it. (An empty word never reaches the rule:
    /// <c>JsonRead.Text</c> is null for it, and <see cref="RecentDeployment" />'s reading leaves an entry without its
    /// five necessary parts out of the list altogether - pinned in <see cref="DeploymentActivityTests" />.)
    /// </summary>
    [Theory]
    [InlineData("timed out")]
    [InlineData("paused")]
    public void AnUnknownWordIsSettledOnUnknownAndKept(string word)
    {
        var report = Parse(recent: $$"""{ "project": "demo-ui", "environment": "uat", "release": "2.4.43", "result": "{{word}}", "finished": "2026-10-09T07:00:00Z" }""");

        var entry = Assert.Single(report.Recent);

        Assert.Equal(DeploymentState.Unknown, entry.Result);
        Assert.Equal(word, entry.ResultText);
    }

    private static DeploymentsReport Parse(string deployments = "", string recent = "", string freezes = "") =>
        DeploymentsReport.Parse($$"""{ "deployments": [ {{deployments}} ], "recent": [ {{recent}} ], "freezes": [ {{freezes}} ], "missing": [] }""")!;
}
