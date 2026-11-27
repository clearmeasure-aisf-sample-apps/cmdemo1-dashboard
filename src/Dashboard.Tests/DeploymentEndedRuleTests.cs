using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// The set of the ways a deployment ends is named: <c>Deployment.Ended</c>, the one internal predicate that says
/// <c>succeeded</c>, <c>failed</c> or <c>canceled</c>. The app declares no <c>InternalsVisibleTo</c>, so this pins the
/// rule where the page says it in words: parse one <c>recent</c> entry per word, and check the activity line the
/// node draws of it - the <see cref="DeploymentActivity"/> <c>Brief</c> path says the word, and the line's kind says
/// whether the page counts the result as an end. An ending word is said in the page's own word (<c>deployed</c>,
/// <c>failed</c>, <c>canceled</c>); a word that is no end - a state in flight, or one the page does not know - is
/// said in the file's own word and drawn as a deployment that did not finish (kind <c>canceled</c>).
/// </summary>
public class DeploymentEndedRuleTests
{
    /// <summary>Friday noon, UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 9, 12, 0, 0, TimeSpan.Zero);
    private const string Slug = "demo";
    private const string Release = "2.4.43";
    private const string Finished = "2026-10-09T07:00:00Z";

    /// <summary>
    /// Each of the three ways a deployment ended is said in the page's own word, and its line's kind is that end:
    /// <c>deployed</c>, <c>failed</c> or <c>canceled</c> - the <c>ActivityKind.OfResult</c> words, reached only
    /// because the page settled the word as an end.
    /// </summary>
    [Theory]
    [InlineData("succeeded", "deployed", "2.4.43 deployed 5 h ago", "reached uat")]
    [InlineData("failed", "failed", "2.4.43 failed 5 h ago", "failed in uat")]
    [InlineData("canceled", "canceled", "2.4.43 canceled 5 h ago", "was canceled in uat")]
    public void AnEndedResultIsSaidInThePagesWordAndItsKindIsThatEnd(string word, string kind, string text, string sentenceOfTheEnd)
    {
        var report = Parse(recent: $$"""{ "project": "demo-ui", "environment": "uat", "release": "{{Release}}", "result": "{{word}}", "finished": "{{Finished}}" }""");

        var line = Assert.Single(DeploymentActivity.OfNode(report, "uat", Slug, "ui", Now, TimeZoneInfo.Utc));
        Assert.Equal(kind, line.Kind);
        Assert.Equal(text, line.Text);
        Assert.Contains(sentenceOfTheEnd, line.Title);
    }

    /// <summary>
    /// A word that is a state of a deployment in flight is not a way it ended: the line says the file's own word
    /// (the <c>Brief</c> path) and its kind is <c>canceled</c> - the one decision the page makes of a result that is
    /// no end, drawn as a deployment that did not finish.
    /// </summary>
    [Theory]
    [InlineData("queued")]
    [InlineData("executing")]
    [InlineData("waiting")]
    public void AnInFlightResultIsSaidInTheFilesWordAndDrawnAsNotFinished(string word)
    {
        var report = Parse(recent: $$"""{ "project": "demo-ui", "environment": "uat", "release": "{{Release}}", "result": "{{word}}", "finished": "{{Finished}}" }""");

        var line = Assert.Single(DeploymentActivity.OfNode(report, "uat", Slug, "ui", Now, TimeZoneInfo.Utc));
        Assert.Equal(ActivityKind.Canceled, line.Kind);
        Assert.Contains($" {word} 5 h ago", line.Text);
    }

    /// <summary>
    /// A word the page does not know at all is no end either: the line says the file's own word and its kind is
    /// <c>canceled</c>, as for a state in flight.
    /// </summary>
    [Theory]
    [InlineData("timed out")]
    [InlineData("paused")]
    public void AnUnknownResultIsSaidInTheFilesWordAndDrawnAsNotFinished(string word)
    {
        var report = Parse(recent: $$"""{ "project": "demo-ui", "environment": "uat", "release": "{{Release}}", "result": "{{word}}", "finished": "{{Finished}}" }""");

        var line = Assert.Single(DeploymentActivity.OfNode(report, "uat", Slug, "ui", Now, TimeZoneInfo.Utc));
        Assert.Equal(ActivityKind.Canceled, line.Kind);
        Assert.Contains($" {word} 5 h ago", line.Text);
    }

    private static DeploymentsReport Parse(string deployments = "", string recent = "", string freezes = "") =>
        DeploymentsReport.Parse($$"""{ "deployments": [ {{deployments}} ], "recent": [ {{recent}} ], "freezes": [ {{freezes}} ], "missing": [] }""")!;
}
