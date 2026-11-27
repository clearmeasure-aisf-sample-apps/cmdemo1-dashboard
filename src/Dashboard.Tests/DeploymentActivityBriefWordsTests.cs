using System.Globalization;
using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// The words of the last-deployment line (<see cref="DeploymentActivity.Brief"/>, reached through
/// <see cref="DeploymentActivity.OfNode"/>): for each of the three known results the word is the mark's kind
/// (<see cref="ActivityKind.Deployed"/>, <see cref="ActivityKind.Failed"/>, <see cref="ActivityKind.Canceled"/>),
/// and for a result the page does not know it is the file's own word while the kind stays
/// <see cref="ActivityKind.Canceled"/>.
/// </summary>
public class DeploymentActivityBriefWordsTests
{
    /// <summary>Friday noon, UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 9, 12, 0, 0, TimeSpan.Zero);
    private const string Slug = "demo";

    /// <summary>The file word for a result the page does not know.</summary>
    private const string UnknownResultText = "skipped";

    private static ActivityLine LastLine(string result, string? startedBy = null)
    {
        var startedByPart = startedBy is null ? string.Empty : $", \"startedBy\": \"{startedBy}\"";
        var json = $$"""{ "deployments": [ ], "recent": [ { "project": "demo-ui", "release": "2.4.43", "environment": "uat", "result": "{{result}}", "finished": "2026-10-09T07:00:00Z"{{startedByPart}} } ], "freezes": [ ], "missing": [] }""";
        var report = DeploymentsReport.Parse(json)!;
        return Assert.Single(DeploymentActivity.OfNode(report, "uat", Slug, "ui", Now, TimeZoneInfo.Utc));
    }

    [Fact]
    public void SucceededSaysDeployedAsKindAndWord()
    {
        Invariant.Runs(() =>
        {
            var line = LastLine("succeeded");
            Assert.Equal(ActivityKind.Deployed, line.Kind);
            Assert.StartsWith($"2.4.43 {ActivityKind.Deployed} ", line.Text, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void FailedSaysFailedAsKindAndWord()
    {
        Invariant.Runs(() =>
        {
            var line = LastLine("failed");
            Assert.Equal(ActivityKind.Failed, line.Kind);
            Assert.StartsWith($"2.4.43 {ActivityKind.Failed} ", line.Text, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void CanceledSaysCanceledAsKindAndWord()
    {
        Invariant.Runs(() =>
        {
            var line = LastLine("canceled");
            Assert.Equal(ActivityKind.Canceled, line.Kind);
            Assert.StartsWith($"2.4.43 {ActivityKind.Canceled} ", line.Text, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void UnknownResultKeepsTheFilesWordAndSaysCanceledAsKind()
    {
        Invariant.Runs(() =>
        {
            var line = LastLine(UnknownResultText);
            Assert.Equal(ActivityKind.Canceled, line.Kind);
            Assert.StartsWith($"2.4.43 {UnknownResultText} ", line.Text, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void TheFullBriefLineIsPinned()
    {
        Invariant.Runs(() =>
        {
            var succeeded = LastLine("succeeded", "ai-ops");
            Assert.Equal("2.4.43 deployed 5 h ago by ai-ops", succeeded.Text);

            var failed = LastLine("failed", "ai-ops");
            Assert.Equal("2.4.43 failed 5 h ago by ai-ops", failed.Text);

            var canceled = LastLine("canceled");
            Assert.Equal("2.4.43 canceled 5 h ago", canceled.Text);

            var unknown = LastLine(UnknownResultText);
            Assert.Equal($"2.4.43 {UnknownResultText} 5 h ago", unknown.Text);
        });
    }
}
