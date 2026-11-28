namespace Dashboard.Tests;

/// <summary>
/// The three named leads to who started a deployment, said from the outside: <c>, started by</c> on the last ended
/// deployment's line and its tooltip alike, <c>, by</c> on the line of a deployment that is executing, and <c> by</c>
/// on a mark's brief words - and, where the file does not say who, no lead left behind in any of the three places.
/// </summary>
public class DeploymentActivityByLeadTests
{
    /// <summary>Friday noon, UTC: the moment the page is asked.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 9, 12, 0, 0, TimeSpan.Zero);

    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    private static DeploymentsReport Parse(string deployments = "", string recent = "") =>
        DeploymentsReport.Parse($$"""{ "deployments": [ {{deployments}} ], "recent": [ {{recent}} ], "freezes": [ ], "missing": [] }""")!;

    /// <summary>The last ended deployment's line and its tooltip say <c>, started by {who}</c> the same way, for the
    /// same <c>startedBy</c> the file gives.</summary>
    [Fact]
    public void TheLastLineAndItsTitleBothSayStartedByTheSameWay()
    {
        var report = Parse(recent: """{ "project": "demo-ui", "release": "2.4.43", "environment": "uat", "result": "succeeded", "finished": "2026-10-09T07:00:00Z", "startedBy": "ai-ops" }""");

        var line = Assert.Single(DeploymentActivity.OfEnvironment(report, "uat", [], Now, Utc));

        Assert.EndsWith(", started by ai-ops", line.Text);
        Assert.Contains(", started by ai-ops", line.Title);
    }

    /// <summary>The line of a deployment that is executing ends with <c>, by {who}</c>.</summary>
    [Fact]
    public void AnExecutingDeploymentLineEndsWithBy()
    {
        var report = Parse(deployments: """{ "project": "demo-ui", "environment": "prod", "release": "2.4.44", "state": "executing", "startedBy": "pat" }""");

        var line = Assert.Single(DeploymentActivity.OfNode(report, "prod", "demo", "ui", Now, Utc));

        Assert.Equal("deploying 2.4.44, by pat", line.Text);
    }

    /// <summary>A mark's brief words say <c> by {who}</c>.</summary>
    [Fact]
    public void AMarksBriefSaysBy()
    {
        var report = Parse(recent: """{ "project": "demo-ui", "release": "2.4.43", "environment": "uat", "result": "succeeded", "finished": "2026-10-09T07:00:00Z", "startedBy": "ai-ops" }""");

        var line = Assert.Single(DeploymentActivity.OfNode(report, "uat", "demo", "ui", Now, Utc));

        Assert.Equal("2.4.43 deployed 5 h ago by ai-ops", line.Text);
    }

    /// <summary>Where the file does not say who started a deployment, none of the three lines leaves a lead behind:
    /// the last ended line, its tooltip, the executing line and the mark's brief all say nothing where no name is.</summary>
    [Fact]
    public void EveryLineDropsTheClauseWhenTheFileDoesNotSayWho()
    {
        // The last ended line and its tooltip.
        var last = Parse(recent: """{ "project": "demo-ui", "release": "2.4.43", "environment": "uat", "result": "succeeded", "finished": "2026-10-09T07:00:00Z" }""");
        var environmentLine = Assert.Single(DeploymentActivity.OfEnvironment(last, "uat", [], Now, Utc));

        Assert.Equal("last: demo-ui 2.4.43 reached uat 5 h ago", environmentLine.Text);
        Assert.DoesNotContain("by", environmentLine.Text);
        Assert.Equal("The last deployment of demo-ui to uat that ended: demo-ui 2.4.43 reached uat 5 h ago (2026-10-09 07:00 +00:00).", environmentLine.Title);
        Assert.DoesNotContain("by", environmentLine.Title);

        // The executing line.
        var executing = Parse(deployments: """{ "project": "demo-ui", "environment": "prod", "release": "2.4.44", "state": "executing" }""");
        Assert.Equal("deploying 2.4.44", Assert.Single(DeploymentActivity.OfNode(executing, "prod", "demo", "ui", Now, Utc)).Text);

        // The mark's brief.
        var mark = Parse(recent: """{ "project": "demo-ui", "release": "2.4.43", "environment": "uat", "result": "succeeded", "finished": "2026-10-09T07:00:00Z" }""");
        Assert.Equal("2.4.43 deployed 5 h ago", Assert.Single(DeploymentActivity.OfNode(mark, "uat", "demo", "ui", Now, Utc)).Text);
    }
}
