using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="WorkPullRequest" />, the record declared in
/// <c>src/Dashboard/Work/WorkItemsReport.cs</c> that names a pull request opened for a work item: its number, its
/// page, its state, when it merged, the commit it merged as, and what its checks concluded.
/// <para>
/// What the other files already pin is left alone. The taking of a pull request out of <c>workitems.json</c> is
/// <c>WorkPullRequest.Read</c>, which is internal to an app that declares no <c>InternalsVisibleTo</c> and is
/// therefore pinned through <see cref="WorkItemsReport.Parse" />: <see cref="WorkItemsTests.EveryPartOfAnItemIsOptionalAndABadPartIsLeftOut" />
/// says that a pull request without a number is still a pull request; <see cref="WorkItemsTests.TheSampleFileIsTheBoardTheViewShows" />
/// that a merged pull request with passed checks arrives with all five of its parts; and
/// <see cref="WorkItemsTests.ThePublishedWorkItemsFileParses" /> that the live feed's pull requests carry a checks word.
/// What a pull request is worth to the page - the <c>#9 · merged · checks passed</c> of a line - is
/// <see cref="WorkBotText.PullLine" /> and <see cref="WorkItemsText.PullLabel" />, and is pinned in those files.
/// </para>
/// <para>
/// What is left, and is pinned here, is the record itself: the constructor handing each part to the property of its
/// name and holding the very instance it was given; the eight ways the two optional parts (checks and checksUrl) can
/// be filled; a pull request with nothing said about its checks being another value than no pull request at all; the
/// equality and hash of two pull requests that differ only in the part of the checks that the page does not show; the
/// words the pull request says its checks in, which are the words the checks gave and follow whoever reads a date;
/// and the public surface, so that the day a part is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a pull request holds the parts the
/// file gave it and calls nothing at all, so every value below is one the test brought, and every call says the same
/// thing twice.
/// </para>
/// </summary>
public class WorkPullRequestEdgeCaseTests
{
    private static readonly DateTimeOffset MergedAt = new(2026, 11, 9, 16, 0, 0, TimeSpan.Zero);

    /// <summary>A pull request with every part filled.</summary>
    private static WorkPullRequest Full => new(9, new Uri("https://github.com/org/repo/pull/9"), "merged", MergedAt, "abcdef1234567890", "passed", new Uri("https://github.com/org/repo/pull/9/checks"));

    /// <summary>A pull request with no optional parts.</summary>
    private static WorkPullRequest Minimal => new(9, null, null, null, null);

    // --------------------------------- the parts, by the names of their own properties

    /// <summary>The constructor's seven arguments, position by position, come back as the property of the same name:
    /// number first, url beside it, state next, mergedAt, mergeSha, checks last - the order the page shows them.
    /// Each is held as the very value the pull request was built of, never as a copy of it, and a new pull request
    /// of the same seven parts is another pull request, not the one it was built of.</summary>
    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        var url = new Uri("https://github.com/org/repo/pull/9");
        var checksUrl = new Uri("https://github.com/org/repo/pull/9/checks");
        var pull = new WorkPullRequest(9, url, "merged", MergedAt, "abcdef1234567890", "passed", checksUrl);

        Assert.Equal(9, pull.Number);
        Assert.Same(url, pull.Url);
        Assert.Equal("merged", pull.State);
        Assert.Equal(MergedAt, pull.MergedAt);
        Assert.Equal("abcdef1234567890", pull.MergeSha);
        Assert.Equal("passed", pull.Checks);
        Assert.Same(checksUrl, pull.ChecksUrl);

        var again = new WorkPullRequest(9, url, "merged", MergedAt, "abcdef1234567890", "passed", checksUrl);

        Assert.Equal(pull, again);
        Assert.NotSame(pull, again);
        Assert.Same(url, again.Url);

        // A pull request with no optional parts is another value than the same pull request with them.
        var noChecks = new WorkPullRequest(9, url, "merged", MergedAt, "abcdef1234567890");

        Assert.NotEqual(pull, noChecks);
        Assert.Null(noChecks.Checks);
        Assert.Null(noChecks.ChecksUrl);
    }

    /// <summary>Each optional part stands on its own property and nowhere else: setting checks does not set
    /// checksUrl, and vice versa. The two defaults (both null) are one pull request, and each part set while the
    /// other is not is another.</summary>
    [Theory]
    [InlineData("passed", null)]
    [InlineData("failed", null)]
    [InlineData(null, "https://github.com/org/repo/pull/9/checks")]
    [InlineData("passed", "https://github.com/org/repo/pull/9/checks")]
    public void EachOptionalPartStandsOnItsOwnPropertyAndNowhereElse(string? checks, string? checksUrl)
    {
        var url = checksUrl is null ? null : new Uri(checksUrl);
        var pull = new WorkPullRequest(9, null, null, null, null, checks, url);

        Assert.Equal(checks, pull.Checks);
        Assert.Equal(url, pull.ChecksUrl);
    }

    /// <summary>The two optional parts give four possible pull requests from the same five required parts: none,
    /// checks alone, checksUrl alone, and both. Each is its own value, and none is confused with another.</summary>
    [Fact]
    public void TheFourFillingsOfTwoOptionalPartsAreFourPullRequests()
    {
        var none = new WorkPullRequest(9, null, null, null, null);
        var checksOnly = new WorkPullRequest(9, null, null, null, null, "passed");
        var urlOnly = new WorkPullRequest(9, null, null, null, null, null, new Uri("https://github.com/org/repo/pull/9/checks"));
        var both = new WorkPullRequest(9, null, null, null, null, "passed", new Uri("https://github.com/org/repo/pull/9/checks"));

        Assert.NotEqual(none, checksOnly);
        Assert.NotEqual(none, urlOnly);
        Assert.NotEqual(none, both);
        Assert.NotEqual(checksOnly, urlOnly);
        Assert.NotEqual(checksOnly, both);
        Assert.NotEqual(urlOnly, both);

        var noneAgain = new WorkPullRequest(9, null, null, null, null);
        var checksOnlyAgain = new WorkPullRequest(9, null, null, null, null, "passed");
        var urlOnlyAgain = new WorkPullRequest(9, null, null, null, null, null, new Uri("https://github.com/org/repo/pull/9/checks"));
        var bothAgain = new WorkPullRequest(9, null, null, null, null, "passed", new Uri("https://github.com/org/repo/pull/9/checks"));

        Assert.Equal(none, noneAgain);
        Assert.Equal(checksOnly, checksOnlyAgain);
        Assert.Equal(urlOnly, urlOnlyAgain);
        Assert.Equal(both, bothAgain);
    }

    /// <summary>A pull request with no checks said is another pull request than one with checks that concluded
    /// nothing. The first has no checks part; the second has a checks part that says no word. Both are valid
    /// pull requests, and the page treats them the same way (it shows no checks word), but they are not one.</summary>
    [Fact]
    public void APullWithNoChecksSaidIsStillAPull()
    {
        var noChecks = new WorkPullRequest(9, null, null, null, null);
        var emptyChecks = new WorkPullRequest(9, null, null, null, null, "");

        Assert.NotEqual(noChecks, emptyChecks);
        Assert.Null(noChecks.Checks);
        Assert.Equal("", emptyChecks.Checks);
    }

    // --------------------------------- the parts, together

    /// <summary>Two pull requests with the same five required parts but different checks are not one. The checks
    /// part is not decorative; it is part of the value. Two pull requests with the same five required parts and
    /// the same checks but different checksUrl are not one either.</summary>
    [Fact]
    public void TwoPullsAreOneWhileTheirSevenPartsAre()
    {
        var url = new Uri("https://github.com/org/repo/pull/9");
        var checksUrl = new Uri("https://github.com/org/repo/pull/9/checks");

        var pull = new WorkPullRequest(9, url, "merged", MergedAt, "abcdef1234567890", "passed", checksUrl);
        var pullAgain = new WorkPullRequest(9, url, "merged", MergedAt, "abcdef1234567890", "passed", checksUrl);

        Assert.Equal(pull, pullAgain);
        Assert.Equal(pull.GetHashCode(), pullAgain.GetHashCode());

        var differentChecks = new WorkPullRequest(9, url, "merged", MergedAt, "abcdef1234567890", "failed", checksUrl);
        var differentChecksUrl = new WorkPullRequest(9, url, "merged", MergedAt, "abcdef1234567890", "passed", new Uri("https://example.com"));

        Assert.NotEqual(pull, differentChecks);
        Assert.NotEqual(pull, differentChecksUrl);

        // No pull is one with no pull, and not with any other thing.
        Assert.NotNull(pull);
        Assert.NotEqual((object)"not a pull", (object)pull);

        // The hash stands still from one call to the next.
        Assert.Equal(pull.GetHashCode(), pull.GetHashCode());
        Assert.Equal(pullAgain.GetHashCode(), pullAgain.GetHashCode());
    }

    /// <summary>A pull request that differs in one part is another pull request. The page depends on this to tell
    /// apart two pull requests for the same item (one open, one merged), and on the hash to put the two in a set
    /// without confusing them.</summary>
    [Fact]
    public void APullThatDiffersInOnePartIsAnotherPull()
    {
        var url = new Uri("https://github.com/org/repo/pull/9");
        var checksUrl = new Uri("https://github.com/org/repo/pull/9/checks");

        var pull = new WorkPullRequest(9, url, "merged", MergedAt, "abcdef1234567890", "passed", checksUrl);
        var differentNumber = new WorkPullRequest(10, url, "merged", MergedAt, "abcdef1234567890", "passed", checksUrl);
        var differentUrl = new WorkPullRequest(9, new Uri("https://example.com"), "merged", MergedAt, "abcdef1234567890", "passed", checksUrl);
        var differentState = new WorkPullRequest(9, url, "open", MergedAt, "abcdef1234567890", "passed", checksUrl);
        var differentMergedAt = new WorkPullRequest(9, url, "merged", MergedAt.AddHours(1), "abcdef1234567890", "passed", checksUrl);
        var differentMergeSha = new WorkPullRequest(9, url, "merged", MergedAt, "0987654zyxwvu", "passed", checksUrl);
        var differentChecks = new WorkPullRequest(9, url, "merged", MergedAt, "abcdef1234567890", "failed", checksUrl);
        var differentChecksUrl = new WorkPullRequest(9, url, "merged", MergedAt, "abcdef1234567890", "passed", new Uri("https://example.com"));

        Assert.NotEqual(pull, differentNumber);
        Assert.NotEqual(pull, differentUrl);
        Assert.NotEqual(pull, differentState);
        Assert.NotEqual(pull, differentMergedAt);
        Assert.NotEqual(pull, differentMergeSha);
        Assert.NotEqual(pull, differentChecks);
        Assert.NotEqual(pull, differentChecksUrl);

        Assert.NotEqual(pull.GetHashCode(), differentNumber.GetHashCode());
        Assert.NotEqual(pull.GetHashCode(), differentUrl.GetHashCode());
        Assert.NotEqual(pull.GetHashCode(), differentState.GetHashCode());
        Assert.NotEqual(pull.GetHashCode(), differentMergedAt.GetHashCode());
        Assert.NotEqual(pull.GetHashCode(), differentMergeSha.GetHashCode());
        Assert.NotEqual(pull.GetHashCode(), differentChecks.GetHashCode());
        Assert.NotEqual(pull.GetHashCode(), differentChecksUrl.GetHashCode());
    }

    /// <summary>No pull request is one with no pull request, and not with any other thing. A set of pull requests
    /// may hold null (it is a different value) and may hold things that are not pull requests without confusing
    /// them with pull requests.</summary>
    [Fact]
    public void NoPullIsOneWithNoPullAndNotWithAnyOtherThing()
    {
        var pull = new WorkPullRequest(9, null, null, null, null);

        Assert.NotNull(pull);
        Assert.NotEqual((object)"", (object)pull);
        Assert.NotEqual((object)9, (object)pull);
        Assert.NotEqual((object)true, (object)pull);

        var set = new HashSet<WorkPullRequest?>([null, pull, new WorkPullRequest(10, null, null, null, null)]);

        Assert.Equal(3, set.Count);
    }

    // --------------------------------- the hash, the set, the operators

    /// <summary>The hash follows the parts: two pull requests that are one have the same hash, and two that are
    /// not one have different hashes. A set of pull requests holds one copy of each value, and the two operators
    /// follow the parts.</summary>
    [Fact]
    public void TheHashFollowsThePartsAndStandsStillFromOneCallToTheNext()
    {
        var url = new Uri("https://github.com/org/repo/pull/9");
        var checksUrl = new Uri("https://github.com/org/repo/pull/9/checks");

        var pull = new WorkPullRequest(9, url, "merged", MergedAt, "abcdef1234567890", "passed", checksUrl);
        var pullAgain = new WorkPullRequest(9, url, "merged", MergedAt, "abcdef1234567890", "passed", checksUrl);
        var differentPull = new WorkPullRequest(9, url, "open", null, null, "pending");

        Assert.Equal(pull.GetHashCode(), pullAgain.GetHashCode());
        Assert.Equal(pull.GetHashCode(), pull.GetHashCode());

        var set = new HashSet<WorkPullRequest>([pull, pullAgain, differentPull]);

        Assert.Equal(2, set.Count);
        Assert.Contains(pull, set);
        Assert.Contains(differentPull, set);

        Assert.True(pull == pullAgain);
        Assert.True(pull != differentPull);
    }

    // --------------------------------- the words the pull says

    /// <summary>The pull request says its seven parts in the order they are named: number, url, state, mergedAt,
    /// mergeSha, checks, checksUrl. This is the record's own voice, for the debugger and for the log, and it
    /// follows the parts as they change.</summary>
    [Fact]
    public void TheRecordSaysItsSevenPartsInTheOrderTheyAreNamed()
    {
        var url = new Uri("https://github.com/org/repo/pull/9");
        var checksUrl = new Uri("https://github.com/org/repo/pull/9/checks");

        var pull = new WorkPullRequest(9, url, "merged", MergedAt, "abcdef1234567890", "passed", checksUrl);

        var text = pull.ToString();

        Assert.Contains("Number = 9", text, StringComparison.Ordinal);
        Assert.Contains("Url = ", text, StringComparison.Ordinal);
        Assert.Contains("State = merged", text, StringComparison.Ordinal);
        Assert.Contains("MergedAt = ", text, StringComparison.Ordinal);
        Assert.Contains("MergeSha = abcdef1234567890", text, StringComparison.Ordinal);
        Assert.Contains("Checks = passed", text, StringComparison.Ordinal);
        Assert.Contains("ChecksUrl = ", text, StringComparison.Ordinal);
    }

    /// <summary>The pull request's own words follow the reader while the parts themselves do not: the moment
    /// written in the text is the reader's moment, not the one the pull request carries. A pull request carried
    /// across a border says its parts in the same words.</summary>
    [Fact]
    public void TheRecordsOwnWordsFollowTheReaderWhileThePartsThemselvesDoNot()
    {
        var pull = new WorkPullRequest(9, null, "merged", MergedAt, null, null, null);

        var english = CultureScope.Says(new CultureInfo("en-US"), () => pull.ToString());
        Assert.Contains("11/9/2026", english, StringComparison.Ordinal);

        var german = CultureScope.Says(new CultureInfo("de-DE"), () => pull.ToString());
        Assert.Contains("9.11.2026", german, StringComparison.Ordinal);
    }

    // --------------------------------- the surface

    /// <summary>The record has five required parts, two optional parts, and the members every record is asked:
    /// the constructor, the properties, ToString, Equals, GetHashCode, the == and != operators, and the copy
    /// constructor. No more, no less. The day a part is added or taken away this test says so.</summary>
    [Fact]
    public void TheRecordHasSevenPartsAndTheMembersEveryRecordIsAsked()
    {
        var type = typeof(WorkPullRequest);

        var constructor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .Single(c => c.GetParameters().Length == 7);

        Assert.Equal("Number", constructor.GetParameters()[0].Name);
        Assert.Equal("Url", constructor.GetParameters()[1].Name);
        Assert.Equal("State", constructor.GetParameters()[2].Name);
        Assert.Equal("MergedAt", constructor.GetParameters()[3].Name);
        Assert.Equal("MergeSha", constructor.GetParameters()[4].Name);
        Assert.Equal("Checks", constructor.GetParameters()[5].Name);
        Assert.Equal("ChecksUrl", constructor.GetParameters()[6].Name);
    }
}