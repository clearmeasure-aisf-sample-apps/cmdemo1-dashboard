using Dashboard.Work;

namespace Dashboard.Tests;

/// <summary>
/// The three check-answer words <see cref="WorkPullRequest.PassedChecks"/>, <see cref="WorkPullRequest.FailedChecks"/>
/// and <see cref="WorkPullRequest.PendingChecks"/>, and the mapping of every file alias to its answer, through both
/// the <c>checks</c> object form (<c>result</c>/<c>state</c>/<c>conclusion</c>) and the flat <c>checks</c>/<c>checksResult</c>
/// form. Unknown words come back trimmed; a missing word is null. The const values pin the file-and-page contract.
/// </summary>
public class WorkPullRequestCheckWordsTests
{
    // --------------------------------------------------------------- the const values

    /// <summary>The three consts equal the words the page has always shown, which pins the file-and-page contract.</summary>
    [Fact]
    public void TheThreeConstsEqualTheWordsThePageShows()
    {
        Assert.Equal("passed", WorkPullRequest.PassedChecks);
        Assert.Equal("failed", WorkPullRequest.FailedChecks);
        Assert.Equal("pending", WorkPullRequest.PendingChecks);
    }

    // --------------------------------------------------- aliases through the object form

    /// <summary>Every passed alias in the <c>checks</c> object form (<c>result</c>, <c>state</c>, <c>conclusion</c>)
    /// gives <see cref="WorkPullRequest.PassedChecks"/>.</summary>
    [Theory]
    [InlineData("success")]
    [InlineData("succeeded")]
    [InlineData("pass")]
    [InlineData("passed")]
    [InlineData("SUCCESS")]
    public void PassedAliasesInObjectFormGivePassedChecks(string word)
    {
        foreach (var field in new[] { "result", "state", "conclusion" })
        {
            var json = $$"""{ "items": [ { "number": 1, "prs": [ { "number": 9, "checks": { "{{field}}": "{{word}}" } } ] } ] }""";
            var report = WorkItemsReport.Parse(json)!;
            var pull = report.Items.Single().PullRequests.Single();
            Assert.Equal(WorkPullRequest.PassedChecks, pull.Checks);
        }
    }

    /// <summary>Every failed alias in the <c>checks</c> object form gives <see cref="WorkPullRequest.FailedChecks"/>.</summary>
    [Theory]
    [InlineData("failure")]
    [InlineData("failed")]
    [InlineData("fail")]
    [InlineData("FAILURE")]
    public void FailedAliasesInObjectFormGiveFailedChecks(string word)
    {
        foreach (var field in new[] { "result", "state", "conclusion" })
        {
            var json = $$"""{ "items": [ { "number": 1, "prs": [ { "number": 9, "checks": { "{{field}}": "{{word}}" } } ] } ] }""";
            var report = WorkItemsReport.Parse(json)!;
            var pull = report.Items.Single().PullRequests.Single();
            Assert.Equal(WorkPullRequest.FailedChecks, pull.Checks);
        }
    }

    /// <summary>Every pending alias in the <c>checks</c> object form gives <see cref="WorkPullRequest.PendingChecks"/>.</summary>
    [Theory]
    [InlineData("pending")]
    [InlineData("queued")]
    [InlineData("in_progress")]
    [InlineData("in progress")]
    [InlineData("PENDING")]
    public void PendingAliasesInObjectFormGivePendingChecks(string word)
    {
        foreach (var field in new[] { "result", "state", "conclusion" })
        {
            var json = $$"""{ "items": [ { "number": 1, "prs": [ { "number": 9, "checks": { "{{field}}": "{{word}}" } } ] } ] }""";
            var report = WorkItemsReport.Parse(json)!;
            var pull = report.Items.Single().PullRequests.Single();
            Assert.Equal(WorkPullRequest.PendingChecks, pull.Checks);
        }
    }

    // --------------------------------------------------- aliases through the flat form

    /// <summary>Every passed alias in the flat <c>checks</c> form gives <see cref="WorkPullRequest.PassedChecks"/>.</summary>
    [Theory]
    [InlineData("success")]
    [InlineData("succeeded")]
    [InlineData("pass")]
    [InlineData("passed")]
    [InlineData("SUCCESS")]
    public void PassedAliasesInFlatFormGivePassedChecks(string word)
    {
        var json = $$"""{ "items": [ { "number": 1, "prs": [ { "number": 9, "checks": "{{word}}" } ] } ] }""";
        var report = WorkItemsReport.Parse(json)!;
        Assert.Equal(WorkPullRequest.PassedChecks, report.Items.Single().PullRequests.Single().Checks);
    }

    /// <summary>Every passed alias in the flat <c>checksResult</c> form gives <see cref="WorkPullRequest.PassedChecks"/>.</summary>
    [Theory]
    [InlineData("success")]
    [InlineData("succeeded")]
    [InlineData("pass")]
    [InlineData("passed")]
    [InlineData("SUCCESS")]
    public void PassedAliasesInChecksResultFormGivePassedChecks(string word)
    {
        var json = $$"""{ "items": [ { "number": 1, "prs": [ { "number": 9, "checksResult": "{{word}}" } ] } ] }""";
        var report = WorkItemsReport.Parse(json)!;
        Assert.Equal(WorkPullRequest.PassedChecks, report.Items.Single().PullRequests.Single().Checks);
    }

    /// <summary>Every failed alias in the flat <c>checks</c> form gives <see cref="WorkPullRequest.FailedChecks"/>.</summary>
    [Theory]
    [InlineData("failure")]
    [InlineData("failed")]
    [InlineData("fail")]
    [InlineData("FAILURE")]
    public void FailedAliasesInFlatFormGiveFailedChecks(string word)
    {
        var json = $$"""{ "items": [ { "number": 1, "prs": [ { "number": 9, "checks": "{{word}}" } ] } ] }""";
        var report = WorkItemsReport.Parse(json)!;
        Assert.Equal(WorkPullRequest.FailedChecks, report.Items.Single().PullRequests.Single().Checks);
    }

    /// <summary>Every failed alias in the flat <c>checksResult</c> form gives <see cref="WorkPullRequest.FailedChecks"/>.</summary>
    [Theory]
    [InlineData("failure")]
    [InlineData("failed")]
    [InlineData("fail")]
    [InlineData("FAILURE")]
    public void FailedAliasesInChecksResultFormGiveFailedChecks(string word)
    {
        var json = $$"""{ "items": [ { "number": 1, "prs": [ { "number": 9, "checksResult": "{{word}}" } ] } ] }""";
        var report = WorkItemsReport.Parse(json)!;
        Assert.Equal(WorkPullRequest.FailedChecks, report.Items.Single().PullRequests.Single().Checks);
    }

    /// <summary>Every pending alias in the flat <c>checks</c> form gives <see cref="WorkPullRequest.PendingChecks"/>.</summary>
    [Theory]
    [InlineData("pending")]
    [InlineData("queued")]
    [InlineData("in_progress")]
    [InlineData("in progress")]
    [InlineData("PENDING")]
    public void PendingAliasesInFlatFormGivePendingChecks(string word)
    {
        var json = $$"""{ "items": [ { "number": 1, "prs": [ { "number": 9, "checks": "{{word}}" } ] } ] }""";
        var report = WorkItemsReport.Parse(json)!;
        Assert.Equal(WorkPullRequest.PendingChecks, report.Items.Single().PullRequests.Single().Checks);
    }

    /// <summary>Every pending alias in the flat <c>checksResult</c> form gives <see cref="WorkPullRequest.PendingChecks"/>.</summary>
    [Theory]
    [InlineData("pending")]
    [InlineData("queued")]
    [InlineData("in_progress")]
    [InlineData("in progress")]
    [InlineData("PENDING")]
    public void PendingAliasesInChecksResultFormGivePendingChecks(string word)
    {
        var json = $$"""{ "items": [ { "number": 1, "prs": [ { "number": 9, "checksResult": "{{word}}" } ] } ] }""";
        var report = WorkItemsReport.Parse(json)!;
        Assert.Equal(WorkPullRequest.PendingChecks, report.Items.Single().PullRequests.Single().Checks);
    }

    // --------------------------------------------------- unknown and missing

    /// <summary>An unknown word such as <c>" skipped "</c> comes back trimmed, not mapped.</summary>
    [Fact]
    public void AnUnknownWordComesBackTrimmed()
    {
        var json = """{ "items": [ { "number": 1, "prs": [ { "number": 9, "checks": " skipped " } ] } ] }""";
        var report = WorkItemsReport.Parse(json)!;
        Assert.Equal("skipped", report.Items.Single().PullRequests.Single().Checks);
    }

    /// <summary>An unknown word in the object form also comes back trimmed.</summary>
    [Fact]
    public void AnUnknownWordInTheObjectFormComesBackTrimmed()
    {
        var json = """{ "items": [ { "number": 1, "prs": [ { "number": 9, "checks": { "result": "  skipped  " } } ] } ] }""";
        var report = WorkItemsReport.Parse(json)!;
        Assert.Equal("skipped", report.Items.Single().PullRequests.Single().Checks);
    }

    /// <summary>A missing checks word gives null: no <c>checks</c> object, no flat <c>checks</c>, no <c>checksResult</c>.</summary>
    [Fact]
    public void AMissingChecksWordGivesNull()
    {
        var json = """{ "items": [ { "number": 1, "prs": [ { "number": 9 } ] } ] }""";
        var report = WorkItemsReport.Parse(json)!;
        Assert.Null(report.Items.Single().PullRequests.Single().Checks);
    }

    /// <summary>A <c>checks</c> object with none of <c>result</c>, <c>state</c> or <c>conclusion</c> gives null for the word.</summary>
    [Fact]
    public void AChecksObjectWithNoKnownFieldGivesNull()
    {
        var json = """{ "items": [ { "number": 1, "prs": [ { "number": 9, "checks": { "url": "https://example.com" } } ] } ] }""";
        var report = WorkItemsReport.Parse(json)!;
        Assert.Null(report.Items.Single().PullRequests.Single().Checks);
    }
}
