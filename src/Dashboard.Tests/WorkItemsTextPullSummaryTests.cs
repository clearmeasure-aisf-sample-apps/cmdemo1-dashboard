namespace Dashboard.Tests;

/// <summary>
/// The pull-request summary line of a work item, read from <see cref="WorkItemsText.PullSummary"/> itself rather
/// than through a render: the dash when an item opened no pull request, a pull's label alone when the file gives
/// it no state, the label and state together when it does, the words <c>Pull request</c> when a pull has no number,
/// and the ", " join of many in the file's order. The number mark stays in the invariant culture whatever the
/// thread reads as, the way every other mark on this page does.
/// </summary>
public class WorkItemsTextPullSummaryTests
{
    private const string Culture = "fi-FI";

    private static WorkItem Item(params WorkPullRequest[] pulls) =>
        new(new WorkItemIdentity(1, "An item", null, null, null), [], null, null, [], pulls, null, WorkItemEnvironments.None, null);

    private static WorkPullRequest Pull(int? number, string? state) =>
        new(number, null, state, null, null);

    // ---- no pulls: the dash ----

    [Fact]
    public void AnItemThatOpenedNoPullRequestIsTheMissingDash()
    {
        Assert.Equal(WorkItemsText.Missing, WorkItemsText.PullSummary(Item()));
    }

    // ---- one pull: the label, and the state when there is one ----

    [Fact]
    public void ANumberedPullWithAStateIsItsMarkAndTheState()
    {
        Assert.Equal("#181 merged", WorkItemsText.PullSummary(Item(Pull(181, "merged"))));
    }

    [Fact]
    public void ANumberedPullWithNoStateIsItsMarkAlone()
    {
        Assert.Equal("#181", WorkItemsText.PullSummary(Item(Pull(181, null))));
    }

    // ---- a pull with no number: the words Pull request ----

    [Fact]
    public void APullWithNoNumberIsTheWordsPullRequestAndItsStateWhenThereIsOne()
    {
        Assert.Equal("Pull request open", WorkItemsText.PullSummary(Item(Pull(null, "open"))));
        Assert.Equal("Pull request", WorkItemsText.PullSummary(Item(Pull(null, null))));
    }

    // ---- many pulls: the comma join, in the file's order ----

    [Fact]
    public void TwoPullsAreJoinedWithCommaAndSpaceInTheFilesOrder()
    {
        Assert.Equal("#181 merged, #182 open", WorkItemsText.PullSummary(Item(Pull(181, "merged"), Pull(182, "open"))));
    }

    // ---- the number mark is the invariant culture's, not the thread's ----

    [Fact]
    public void APullNumberIsHashThenInvariantDigitsUnderAnOtherCulture()
    {
        Assert.Equal("#181 merged", CultureScope.Says(Culture, () => WorkItemsText.PullSummary(Item(Pull(181, "merged")))));
    }
}
