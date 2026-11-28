using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The three board words an item and its pull requests are both described by, pinned in one place: the consts
/// <c>WorkItemIdentity.NumberWord</c>, <c>WorkItemIdentity.UrlWord</c> and
/// <c>WorkItemIdentity.StateWord</c>. <see cref="WorkItemsReport.Parse" /> reads an item's identity, each
/// pull request and a pull request's checks object through them, so a file written in those words reads back whole.
/// </summary>
public class WorkBoardWordsTests
{
    private static readonly string NumberWord = Word("NumberWord");
    private static readonly string UrlWord = Word("UrlWord");
    private static readonly string StateWord = Word("StateWord");

    /// <summary>The three words are what the board file has always said them: number, url, state - three different words.</summary>
    [Fact]
    public void TheThreeWordsAreTheBoardFileNames()
    {
        Assert.Equal("number", NumberWord);
        Assert.Equal("url", UrlWord);
        Assert.Equal("state", StateWord);

        Assert.Equal(3, new[] { NumberWord, UrlWord, StateWord }
            .Distinct(StringComparer.Ordinal).Count());
    }

    /// <summary>An item and its pull request written in the three words come back with their number, page and state.</summary>
    [Fact]
    public void AnItemAndItsPullRequestWrittenInTheWordsReadBack()
    {
        var report = WorkItemsReport.Parse($$"""
            { "items": [ {
                "{{NumberWord}}": 2232,
                "{{UrlWord}}": "https://github.com/acme/board/issues/2232",
                "{{StateWord}}": "open",
                "prs": [ {
                    "{{NumberWord}}": 2233,
                    "{{UrlWord}}": "https://github.com/acme/board/pull/2233",
                    "{{StateWord}}": "merged" } ] } ] }
            """)!;

        var item = Assert.Single(report.Items);
        Assert.Equal(2232, item.Identity.Number);
        Assert.Equal(new Uri("https://github.com/acme/board/issues/2232"), item.Identity.Url);
        Assert.Equal("open", item.Identity.State);

        var pull = Assert.Single(item.PullRequests);
        Assert.Equal(2233, pull.Number);
        Assert.Equal(new Uri("https://github.com/acme/board/pull/2233"), pull.Url);
        Assert.Equal("merged", pull.State);
    }

    /// <summary>A pull request whose checks object gives only a state and a page keeps both: the state word is
    /// turned into the page's checks word and the page is the checks page.</summary>
    [Fact]
    public void AChecksObjectWithOnlyStateAndUrlReadsBack()
    {
        var report = WorkItemsReport.Parse($$"""
            { "items": [ { "number": 2232, "prs": [ { "checks": {
                "{{StateWord}}": "success",
                "{{UrlWord}}": "https://github.com/acme/board/pull/2233/checks" } } ] } ] }
            """)!;

        var pull = Assert.Single(Assert.Single(report.Items).PullRequests);
        Assert.Equal(WorkPullRequest.PassedChecks, pull.Checks);
        Assert.Equal(new Uri("https://github.com/acme/board/pull/2233/checks"), pull.ChecksUrl);
        Assert.Null(pull.Number);
        Assert.Null(pull.Url);
        Assert.Null(pull.State);
    }

    /// <summary>The internal const of that name on <see cref="WorkItemIdentity" />, read the way the other word tests read internal words.</summary>
    private static string Word(string name) =>
        (string)typeof(WorkItemIdentity).GetField(name, BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
}
