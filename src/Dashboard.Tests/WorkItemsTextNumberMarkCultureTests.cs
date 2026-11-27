namespace Dashboard.Tests;

/// <summary>
/// The GitHub number mark of a work item or pull request, pinned under a non-invariant culture.
/// <para>
/// The page shows a work item or pull request number as <c>#</c> and the digits in the invariant culture
/// (<c>#1234</c>), whatever culture the thread runs in. These tests run the five members that write that mark -
/// <see cref="WorkItemsText.Number"/>, <see cref="WorkItemsText.PullLabel"/>, <see cref="WorkItemsText.IssueTitle"/>,
/// <see cref="WorkItemsText.PullTitle"/> and <see cref="WorkItemsText.Carries"/> - under <c>fi-FI</c> so a mark that
/// drifts to the thread's culture (or drops the <c>#</c>) is caught: the label, the tooltip and the <c>carries</c>
/// line must keep matching. The invariant-culture behavior is pinned by the other work items files and left alone.
/// </para>
/// </summary>
public class WorkItemsTextNumberMarkCultureTests
{
    private const string Culture = "fi-FI";

    private static WorkItem Item(int number) =>
        new(new WorkItemIdentity(number, "An item", null, null, null), [], null, null, [], [], null, WorkItemEnvironments.None, null);

    private static WorkPullRequest Pull(int number) =>
        new(number, null, null, null, null);

    private static WorkItemsReport Report(int[] items, string version) =>
        new(null, null, WorkItemsStats.Empty, [], [new WorkRelease(version, null, items)], null, null);

    [Fact]
    public void AWorkItemNumberIsHashThenInvariantDigitsUnderAnOtherCulture()
    {
        Assert.Equal("#1234", CultureScope.Says(Culture, () => WorkItemsText.Number(Item(1234))));
    }

    [Fact]
    public void APullRequestNumberIsHashThenInvariantDigitsUnderAnOtherCulture()
    {
        Assert.Equal("#1234", CultureScope.Says(Culture, () => WorkItemsText.PullLabel(Pull(1234))));
    }

    [Fact]
    public void AnIssueTitleNamesItsWorkItemNumberUnderAnOtherCulture()
    {
        var title = CultureScope.Says(Culture, () => WorkItemsText.IssueTitle(Item(1234)));
        Assert.StartsWith("Work item #1234", title);
    }

    [Fact]
    public void APullTitleNamesItsPullRequestNumberUnderAnOtherCulture()
    {
        var title = CultureScope.Says(Culture, () => WorkItemsText.PullTitle(Pull(1234)));
        Assert.StartsWith("Pull request #1234", title);
    }

    [Fact]
    public void AReleaseCarriesItsItemNumbersAsInvariantMarksUnderAnOtherCulture()
    {
        // The release carries 12 and 3; Carries orders them lowest first, so the line is "carries #3, #12".
        Assert.Equal("carries #3, #12", CultureScope.Says(Culture, () => WorkItemsText.Carries(Report([12, 3], "2.4.90"), "2.4.90")));
    }
}
