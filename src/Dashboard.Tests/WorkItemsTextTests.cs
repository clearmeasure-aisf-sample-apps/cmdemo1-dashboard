namespace Dashboard.Tests;

/// <summary>
/// The words of the work items view, read from <see cref="WorkItemsText"/> itself rather than through a render. The
/// sentences are copied here on purpose, in the file's own wording: a change to one of them is a change a reader
/// sees, so it has to be said twice before it happens.
/// </summary>
public class WorkItemsTextTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 22, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ThreeHoursAgo = new(2026, 11, 9, 19, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset MergedAt = new(2026, 11, 9, 22, 0, 0, TimeSpan.Zero);

    private static WorkItem Item(
        int? number = null,
        string? state = null,
        string? column = null,
        IReadOnlyList<ColumnVisit>? history = null,
        WorkItemEnvironments? environments = null) =>
        new(number, "An item", null, state, column, history ?? [], null, null, [], [], null, environments ?? WorkItemEnvironments.None, null);

    // ---- AudienceHash ----

    [Theory]
    [InlineData(WorkAudience.All, "")]
    [InlineData(WorkAudience.Bot, "bot")]
    [InlineData(WorkAudience.Team, "team")]
    public void TheAddressSegmentOfAFilterIsItsOwnWord(WorkAudience audience, string hash) =>
        Assert.Equal(hash, WorkItemsText.AudienceHash(audience));

    [Fact]
    public void AFilterThatIsNoneOfTheThreeHasNoAddress() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => WorkItemsText.AudienceHash((WorkAudience)7));

    // ---- AudienceLabel ----

    [Theory]
    [InlineData(WorkAudience.All, "All")]
    [InlineData(WorkAudience.Bot, "Bot")]
    [InlineData(WorkAudience.Team, "Team")]
    public void EveryFilterIsNamedOnTheStrip(WorkAudience audience, string label) =>
        Assert.Equal(label, WorkItemsText.AudienceLabel(audience));

    [Fact]
    public void AFilterThatIsNoneOfTheThreeHasNoName() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => WorkItemsText.AudienceLabel((WorkAudience)7));

    // ---- AudienceOf ----

    [Theory]
    [InlineData(null, WorkAudience.All)]
    [InlineData("", WorkAudience.All)]
    [InlineData("   ", WorkAudience.All)]
    [InlineData("all", WorkAudience.All)]
    [InlineData("everything", WorkAudience.All)]
    [InlineData("bott", WorkAudience.All)]
    [InlineData("bot", WorkAudience.Bot)]
    [InlineData("BOT", WorkAudience.Bot)]
    [InlineData("  bot  ", WorkAudience.Bot)]
    [InlineData("team", WorkAudience.Team)]
    [InlineData("Team", WorkAudience.Team)]
    [InlineData("\tTEAM\n", WorkAudience.Team)]
    public void TheFilterIsTheWordInTheAddressAndAnythingElseIsAll(string? segment, WorkAudience audience) =>
        Assert.Equal(audience, WorkItemsText.AudienceOf(segment));

    [Fact]
    public void AFilterRoundTripsThroughItsAddress()
    {
        foreach (var audience in new[] { WorkAudience.All, WorkAudience.Bot, WorkAudience.Team })
        {
            Assert.Equal(audience, WorkItemsText.AudienceOf(WorkItemsText.AudienceHash(audience)));
        }
    }

    // ---- Entered ----

    [Fact]
    public void TheTimeAnItemEnteredAColumnIsTheTimeItsHistoryGives()
    {
        var item = Item(
            number: 12,
            column: WorkColumns.Review,
            history: [new ColumnVisit(WorkColumns.Todo, new DateTimeOffset(2026, 11, 7, 8, 0, 0, TimeSpan.Zero)), new ColumnVisit(WorkColumns.Review, new DateTimeOffset(2026, 11, 8, 8, 0, 0, TimeSpan.Zero))]);

        Assert.Equal(new DateTimeOffset(2026, 11, 8, 8, 0, 0, TimeSpan.Zero), WorkItemsText.Entered(item, WorkColumns.Review));
        Assert.Equal(new DateTimeOffset(2026, 11, 7, 8, 0, 0, TimeSpan.Zero), WorkItemsText.Entered(item, WorkColumns.Todo));
    }

    [Fact]
    public void AColumnIsMatchedHoweverTheFileSpelledIt()
    {
        var item = Item(history: [new ColumnVisit("review", ThreeHoursAgo)]);

        Assert.Equal(ThreeHoursAgo, WorkItemsText.Entered(item, WorkColumns.Review));
        Assert.Equal(ThreeHoursAgo, WorkItemsText.Entered(item, "REVIEW"));
    }

    [Fact]
    public void AColumnTheHistoryNeverMentionsHasNoTime()
    {
        var item = Item(history: [new ColumnVisit(WorkColumns.Todo, ThreeHoursAgo)]);

        Assert.Null(WorkItemsText.Entered(item, WorkColumns.Done));
        Assert.Null(WorkItemsText.Entered(item, WorkColumns.Unplaced));
    }

    [Fact]
    public void AVisitWithoutATimeIsNoTimeAndAColumnNamedNowhereIsNeither()
    {
        var item = Item(history: [new ColumnVisit(WorkColumns.Done, null), new ColumnVisit(null, ThreeHoursAgo)]);

        Assert.Null(WorkItemsText.Entered(item, WorkColumns.Done));
        Assert.Null(WorkItemsText.Entered(item, "Anything"));
    }

    [Fact]
    public void TheLastVisitToAColumnIsTheOneThatCounts()
    {
        var item = Item(history:
        [
            new ColumnVisit(WorkColumns.Done, new DateTimeOffset(2026, 11, 8, 8, 0, 0, TimeSpan.Zero)),
            new ColumnVisit(WorkColumns.Done, new DateTimeOffset(2026, 11, 9, 18, 0, 0, TimeSpan.Zero)),
        ]);

        Assert.Equal(new DateTimeOffset(2026, 11, 9, 18, 0, 0, TimeSpan.Zero), WorkItemsText.Entered(item, WorkColumns.Done));
    }

    [Fact]
    public void AnItemWithNoHistoryEnteredNothing() =>
        Assert.Null(WorkItemsText.Entered(Item(number: 1, column: WorkColumns.Todo), WorkColumns.Todo));

    [Fact]
    public void AnItemThatIsNothingHasEnteredNothing() =>
        Assert.Throws<ArgumentNullException>(() => WorkItemsText.Entered(null!, WorkColumns.Done));

    // ---- IssueTitle ----

    [Theory]
    [InlineData(490, "Open", "Work item #490, Open (opens in a new tab)")]
    [InlineData(490, "closed", "Work item #490, closed (opens in a new tab)")]
    [InlineData(490, null, "Work item #490 (opens in a new tab)")]
    [InlineData(0, "Open", "Work item #0, Open (opens in a new tab)")]
    [InlineData(null, "Open", "Work item, Open (opens in a new tab)")]
    [InlineData(null, null, "Work item (opens in a new tab)")]
    public void TheItemTitleSaysTheNumberAndTheStateItHas(int? number, string? state, string title) =>
        Assert.Equal(title, WorkItemsText.IssueTitle(Item(number: number, state: state)));

    [Fact]
    public void AnItemThatIsNothingHasNoTitle() =>
        Assert.Throws<ArgumentNullException>(() => WorkItemsText.IssueTitle(null!));

    // ---- IsMerged ----

    [Theory]
    [InlineData("merged", true)]
    [InlineData("MERGED", true)]
    [InlineData("Merged", true)]
    [InlineData("open", false)]
    [InlineData("closed", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void AMergedPullIsTheWordMergedHoweverItIsCased(string? state, bool merged) =>
        Assert.Equal(merged, WorkItemsText.IsMerged(state));

    // ---- PullLabel ----

    [Theory]
    [InlineData(9, "#9")]
    [InlineData(0, "#0")]
    [InlineData(null, "Pull request")]
    public void APullIsItsNumberOrTheWordsPullRequest(int? number, string label) =>
        Assert.Equal(label, WorkItemsText.PullLabel(new WorkPullRequest(number, null, null, null, null)));

    // ---- PullTitle ----

    [Theory]
    [InlineData(9, true, "Pull request #9, merged 2026-11-09T22:00:00Z (opens in a new tab)")]
    [InlineData(9, false, "Pull request #9 (opens in a new tab)")]
    [InlineData(null, true, "Pull request, merged 2026-11-09T22:00:00Z (opens in a new tab)")]
    [InlineData(null, false, "Pull request (opens in a new tab)")]
    public void ThePullTitleNamesItAndSaysWhenItMerged(int? number, bool merged, string title) =>
        Assert.Equal(title, WorkItemsText.PullTitle(new WorkPullRequest(number, null, null, merged ? MergedAt : null, null)));

    [Theory]
    [InlineData("success", "Pull request #9 (opens in a new tab)")]
    [InlineData("failed", "Pull request #9 (opens in a new tab)")]
    [InlineData(null, "Pull request #9 (opens in a new tab)")]
    public void WhatAPullChecksHasNothingToDoWithItsTitle(string? checks, string title) =>
        Assert.Equal(title, WorkItemsText.PullTitle(new WorkPullRequest(9, null, "open", null, "abcdef1234567890", checks)));

    [Fact]
    public void TheMergeTimeInTheTitleIsUtcToTheSecond() =>
        Assert.Equal(
            "Pull request #9, merged 2026-11-09T22:30:00Z (opens in a new tab)",
            WorkItemsText.PullTitle(new WorkPullRequest(9, null, "merged", new DateTimeOffset(2026, 11, 10, 0, 30, 0, TimeSpan.FromHours(2)), null)));

    [Fact]
    public void APullThatIsNothingHasNoTitle() =>
        Assert.Throws<ArgumentNullException>(() => WorkItemsText.PullTitle(null!));

    // ---- SessionTitle ----

    [Theory]
    [InlineData(1, 1, "Cursor session (opens in a new tab)")]
    [InlineData(1, 2, "Cursor session 1 (opens in a new tab)")]
    [InlineData(2, 2, "Cursor session 2 (opens in a new tab)")]
    [InlineData(3, 3, "Cursor session 3 (opens in a new tab)")]
    public void OneSessionIsJustASessionAndManyAreNumbered(int index, int count, string title) =>
        Assert.Equal(title, WorkItemsText.SessionTitle(index, count));

    // ---- WorkerLabel ----

    [Theory]
    [InlineData(WorkWorker.Bot, "bot")]
    [InlineData(WorkWorker.Team, "team")]
    [InlineData(WorkWorker.Unknown, null)]
    public void AWorkerIsNamedOrSaidNothingAbout(WorkWorker worker, string? label) =>
        Assert.Equal(label, WorkItemsText.WorkerLabel(worker));

    [Fact]
    public void AWorkerThatIsNoneOfTheThreeHasNoName() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => WorkItemsText.WorkerLabel((WorkWorker)7));

    // ---- PlaceTitle ----

    [Theory]
    [InlineData("2.4.86", true, "Deployed to prod as 2.4.86, 3 h ago")]
    [InlineData("2.4.86", false, "Deployed to prod as 2.4.86")]
    [InlineData(null, true, "Deployed to prod, 3 h ago")]
    [InlineData(null, false, "Deployed to prod")]
    public void APlaceSaysTheVersionAndTheTimeItHas(string? version, bool deployed, string title) =>
        Assert.Equal(title, WorkItemsText.PlaceTitle(new WorkPlace("prod", version, deployed ? ThreeHoursAgo : null), Now));

    [Fact]
    public void EachPlaceIsTitledInItsOwnEnvironmentOrder()
    {
        var item = Item(
            number: 12,
            environments: new WorkItemEnvironments(
                new WorkDeployment("2.4.86", new DateTimeOffset(2026, 11, 9, 20, 30, 0, TimeSpan.Zero)),
                new WorkDeployment("2.4.86", ThreeHoursAgo),
                new WorkDeployment(null, null)));

        Assert.Equal(
            ["tdd", "uat", "prod"],
            WorkItemsText.Places(item).Select(place => place.Name));
        Assert.Equal(
            [
                "Deployed to tdd as 2.4.86, 2 h ago",
                "Deployed to uat as 2.4.86, 3 h ago",
                "Deployed to prod",
            ],
            WorkItemsText.Places(item).Select(place => WorkItemsText.PlaceTitle(place, Now)));
    }

    [Fact]
    public void APlaceThatIsNothingHasNoTitle() =>
        Assert.Throws<ArgumentNullException>(() => WorkItemsText.PlaceTitle(null!, Now));

    // ---- CandidateTitle ----

    [Theory]
    [InlineData(0, "Octopus release, cut just now")]
    [InlineData(59, "Octopus release, cut just now")]
    [InlineData(60, "Octopus release, cut 1 min ago")]
    [InlineData(180, "Octopus release, cut 3 min ago")]
    [InlineData(10800, "Octopus release, cut 3 h ago")]
    [InlineData(172800, "Octopus release, cut 2 d ago")]
    public void AReleaseSaysWhenItWasCut(int secondsAgo, string title) =>
        Assert.Equal(title, WorkItemsText.CandidateTitle(new ReleaseCandidate("2.4.86", Now - TimeSpan.FromSeconds(secondsAgo)), Now));

    [Fact]
    public void AReleaseAheadOfTheClockIsCutJustNow() =>
        Assert.Equal("Octopus release, cut just now", WorkItemsText.CandidateTitle(new ReleaseCandidate("2.4.86", Now + TimeSpan.FromHours(3)), Now));

    [Fact]
    public void AReleaseThatDoesNotSayWhenIsStillARelease()
    {
        Assert.Equal("Octopus release", WorkItemsText.CandidateTitle(new ReleaseCandidate("2.4.86", null), Now));
        Assert.Equal("Octopus release", WorkItemsText.CandidateTitle(new ReleaseCandidate(null, null), Now));
    }

    [Fact]
    public void NoReleaseHasNoTitle() =>
        Assert.Throws<ArgumentNullException>(() => WorkItemsText.CandidateTitle(null!, Now));
}
