using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// The link words of a work item or pull request, pinned through <see cref="WorkItemsText"/> itself rather than
/// through a render: the words <c>Work item</c> and <c>Pull request</c> appear once in the production file, and
/// the <c>", "</c> separator appears once. These tests pin the output of <see cref="WorkItemsText.IssueTitle"/>,
/// <see cref="WorkItemsText.PullTitle"/>, <see cref="WorkItemsText.Carries"/> and <see cref="WorkItemsText.PullSummary"/>
/// under a non-invariant thread culture so a culture drift (or a word drift) is caught before it reaches the page.
/// The invariant-culture behavior is pinned by <see cref="WorkItemsTextTests"/> and
/// <see cref="WorkItemsTextNumberMarkCultureTests"/> and left alone.
/// </summary>
public class WorkItemsTextLinkWordsTests
{
    private const string Culture = "de-DE";

    private static readonly DateTimeOffset MergedAt = new(2026, 11, 9, 22, 0, 0, TimeSpan.Zero);

    private static WorkItem Item(int? number = null, string? state = null) =>
        new(new WorkItemIdentity(number, "An item", null, state, null), [], null, null, [], [], null, WorkItemEnvironments.None, null);

    private static WorkPullRequest Pull(int? number, DateTimeOffset? mergedAt = null) =>
        new(number, null, null, mergedAt, null);

    private static WorkItemsReport Report(params int[] numbers) =>
        new(null, null, WorkItemsStats.Empty, [], [new WorkRelease("2.4.90", null, numbers)], null, null);

    private static WorkItem ItemWithPulls(params WorkPullRequest[] pulls) =>
        new(new WorkItemIdentity(1, "An item", null, null, null), [], null, null, [], pulls, null, WorkItemEnvironments.None, null);

    // ---- IssueTitle: the words "Work item" with and without a number ----

    [Fact]
    public void AnIssueTitleWithANumberAndStateSaysWorkItemNumberState()
    {
        var title = CultureScope.Says(Culture, () => WorkItemsText.IssueTitle(Item(5, "open")));
        Assert.Equal("Work item #5, open (opens in a new tab)", title);
    }

    [Fact]
    public void AnIssueTitleWithoutANumberAndStateSaysWorkItemAlone()
    {
        var title = CultureScope.Says(Culture, () => WorkItemsText.IssueTitle(Item(null, null)));
        Assert.Equal("Work item (opens in a new tab)", title);
    }

    [Fact]
    public void AnIssueTitleWithoutANumberButWithAStateSaysWorkItemAndState()
    {
        var title = CultureScope.Says(Culture, () => WorkItemsText.IssueTitle(Item(null, "open")));
        Assert.Equal("Work item, open (opens in a new tab)", title);
    }

    [Fact]
    public void AnIssueTitleWithANumberButNoStateSaysWorkItemNumber()
    {
        var title = CultureScope.Says(Culture, () => WorkItemsText.IssueTitle(Item(5, null)));
        Assert.Equal("Work item #5 (opens in a new tab)", title);
    }

    // ---- PullTitle: the words "Pull request" with no number and a merge time ----

    [Fact]
    public void APullTitleWithNoNumberAndAMergeTimeSaysPullRequestMergedWhen()
    {
        var title = CultureScope.Says(Culture, () => WorkItemsText.PullTitle(Pull(null, MergedAt)));
        Assert.Equal("Pull request, merged 2026-11-09T22:00:00Z (opens in a new tab)", title);
    }

    [Fact]
    public void APullTitleWithNoNumberAndNoMergeTimeSaysPullRequestAlone()
    {
        var title = CultureScope.Says(Culture, () => WorkItemsText.PullTitle(Pull(null, null)));
        Assert.Equal("Pull request (opens in a new tab)", title);
    }

    // ---- Carries: two numbers joined with ", " ----

    [Fact]
    public void CarriesWithTwoNumbersJoinsThemWithCommaAndSpace()
    {
        var line = CultureScope.Says(Culture, () => WorkItemsText.Carries(Report(3, 7), "2.4.90"));
        Assert.Equal("carries #3, #7", line);
    }

    // ---- PullSummary: two numbered pulls joined with ", " ----

    [Fact]
    public void PullSummaryWithTwoNumberedPullsJoinsThemWithCommaAndSpace()
    {
        var summary = CultureScope.Says(Culture, () =>
            WorkItemsText.PullSummary(ItemWithPulls(Pull(181), Pull(182))));
        Assert.Equal("#181, #182", summary);
    }
}
