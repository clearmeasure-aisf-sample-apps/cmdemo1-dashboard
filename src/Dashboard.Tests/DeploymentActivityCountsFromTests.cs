namespace Dashboard.Tests;

/// <summary>
/// The age a node's line gives a deployment in flight is the age of the moment the deployment counts from (#1642):
/// its start, whatever end the file happens to write beside it, and no age at all when the file gives no start it
/// can read. Nothing here asks the clock: every moment is one the test brings.
/// </summary>
public class DeploymentActivityCountsFromTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 9, 12, 0, 0, TimeSpan.Zero);

    private static IReadOnlyList<ActivityLine> Node(string deployment) =>
        DeploymentActivity.OfNode(
            DeploymentsReport.Parse($$"""{ "deployments": [ {{deployment}} ], "recent": [], "freezes": [], "missing": [] }""")!,
            "uat", "demo", "ui", Now, TimeZoneInfo.Utc);

    [Theory]
    [InlineData("executing", "deploying 2, 3 min")]
    [InlineData("queued", "2 queued, 3 min")]
    public void TheLineSaysTheAgeOfTheStartNotOfAnEndTheFileAlsoGives(string state, string expected)
    {
        var line = Assert.Single(Node($$"""
            { "project": "demo-ui", "environment": "uat", "release": "2", "state": "{{state}}", "since": "2026-10-09T11:57:00Z", "finished": "2026-10-09T09:00:00Z" }
            """));

        Assert.Equal(expected, line.Text);
    }

    [Fact]
    public void AWaitWithNoAskedTimeSaysTheAgeOfTheStart()
    {
        var line = Assert.Single(Node("""
            { "project": "demo-ui", "environment": "uat", "release": "2", "state": "waiting", "since": "2026-10-09T10:00:00Z", "finished": "2026-10-09T11:59:00Z", "waitsFor": { "kind": "sign-off" } }
            """));

        Assert.Equal("2 waits for a sign-off, 2 h", line.Text);
    }

    [Theory]
    [InlineData("executing", "deploying 2")]
    [InlineData("queued", "2 queued")]
    public void NoStartTheFileCanReadMeansNoAge(string state, string expected)
    {
        var line = Assert.Single(Node($$"""
            { "project": "demo-ui", "environment": "uat", "release": "2", "state": "{{state}}", "since": "a while ago", "finished": "2026-10-09T11:59:00Z" }
            """));

        Assert.Equal(expected, line.Text);
    }
}
