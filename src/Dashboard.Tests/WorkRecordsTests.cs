using System.Globalization;
using Dashboard.Work;

namespace Dashboard.Tests;

/// <summary>
/// The shared record-fixture helper <see cref="WorkRecords" /> is pinned here, the way its records are pinned in
/// their own edge-case files but for the one question those files each asked of their own copy: does the shared
/// fixture hand back the very instances it was given, is it fully filled so that a part landing on another part's
/// place is seen rather than quietly matching a third of the same record, and do its words stand still under a
/// reader of another culture? Nothing here reaches the network and nothing asks a clock what time it is: every
/// moment is one the fixture brought, and the addresses are held, said and compared as letters only.
/// <para>
/// What is left to the per-record files is left alone: the boundary, null/empty and fixed-value tests of each
/// record stand where they always did. This file pins only the shared helper and the records it builds, so that a
/// fixture that drifts from the record it stands in - a part left unsaid, an instance copied where one was held, a
/// word that moves between one culture and another - is named here once rather than rediscovered in five files.
/// </para>
/// </summary>
public class WorkRecordsTests
{
    // ------------------------------------------------------------------ the string-instance helper

    /// <summary>A word of the test's own is another instance of the same letters: it is not the literal it was
    /// built from, and it is not interned to it, yet it compares equal to it in its own letters. That is what lets
    /// a part held as the very instance it was given be proved by the holding and not by the interning of a
    /// literal, which is the question every word-pinning edge-case file asks of its own <c>Own</c>.</summary>
    [Fact]
    public void OwnMakesAnotherInstanceOfTheSameLetters()
    {
        var word = "Pin the board";
        var own = WorkRecords.Own(word);

        Assert.NotSame(word, own);
        Assert.Equal(word, own);
        Assert.Equal(word.Length, own.Length);
        Assert.Equal(word.GetHashCode(), own.GetHashCode());

        // The word of whitespace is its own instance too, and the letters stand as they are. The empty word is the
        // one exception: a string of no characters is the one string there is, so it stands as the shared nothing
        // rather than as a second instance, and the letters - of which there are none - stand as they are.
        Assert.NotSame("   \t ", WorkRecords.Own("   \t "));
        Assert.Equal("   \t ", WorkRecords.Own("   \t "));
        Assert.Same(string.Empty, WorkRecords.Own(string.Empty));
        Assert.Equal(string.Empty, WorkRecords.Own(string.Empty));
    }

    /// <summary>The fixture's own moment is one the test brought: the day, hour and minute it was asked for, at the
    /// fixed instant of the fixture's November, and not the moment the clock says it is now. A part that is asked
    /// what time it is would stand on the clock; the fixture's parts stand on the moment the fixture gave them.</summary>
    [Fact]
    public void AtIsTheMomentsFixtureBroughtAndNotTheClocks()
    {
        var at = WorkRecords.At(9, 22, 4);

        Assert.Equal(new DateTimeOffset(2026, 11, 9, 22, 4, 0, TimeSpan.Zero), at);
        Assert.Equal(TimeSpan.Zero, at.Offset);

        // Two asks for the same moment are the same moment, and an ask for another day is another moment.
        Assert.Equal(at, WorkRecords.At(9, 22, 4));
        Assert.NotEqual(at, WorkRecords.At(10));
    }

    // ------------------------------------------------------- the sub-records a work item is made of

    /// <summary>A part handed to a sub-record fixture comes back as the very instance it was given, and the parts
    /// not named stand at the filled defaults the fixture says. That is the question a constructor-handing test
    /// asks of its record, asked once of the shared fixture rather than five times of five copies.</summary>
    [Fact]
    public void TheSubRecordFixturesHoldTheInstancesTheyWereGiven()
    {
        var pull = WorkRecords.PullRequest(mergeSha: "00ff00ff00ff");

        Assert.Equal("00ff00ff00ff", pull.MergeSha);
        Assert.Equal(912, pull.Number);
        Assert.Equal("failed", pull.Checks);

        var candidate = WorkRecords.Candidate(version: "2.4.87");

        Assert.Equal("2.4.87", candidate.Version);
        Assert.Equal(WorkRecords.At(9, 23), candidate.CreatedAt);

        var deployment = WorkRecords.Deployment("2.4.86", WorkRecords.At(9, 23, 4));

        Assert.Equal("2.4.86", deployment.Version);
        Assert.Equal(WorkRecords.At(9, 23, 4), deployment.DeployedAt);

        var rework = WorkRecords.Rework(ftFailures: 2, reverts: 1, rate: 0.25);

        Assert.Equal(2, rework.FtFailures);
        Assert.Equal(1, rework.Reverts);
        Assert.Equal(0.25, rework.Rate);

        var note = WorkRecords.Note(text: "Blocked on the schema", kind: "status");

        Assert.Equal("Blocked on the schema", note.Text);
        Assert.Equal("status", note.Kind);
        Assert.Equal(WorkRecords.At(10), note.At);

        var visit = WorkRecords.Visit(column: WorkColumns.Review, leftAt: WorkRecords.At(9, 12), hours: 4);

        Assert.Equal(WorkColumns.Review, visit.Column);
        Assert.Equal(WorkRecords.At(9, 8), visit.EnteredAt);
        Assert.Equal(WorkRecords.At(9, 12), visit.LeftAt);
        Assert.Equal(4d, visit.Hours);
    }

    /// <summary>The environments fixture fills no place on the caller's behalf: a place handed a deployment stands
    /// as that deployment, and a place the caller leaves out stands as the nothing a place that has not got it
    /// stands on. The filled road is <see cref="WorkRecords.FullEnvironments"/>, the one the item fixture stands on
    /// by default: tdd and prod said with their own release and their own moment, uat the place that has not got
    /// it.</summary>
    [Fact]
    public void EnvironmentsFillsNoPlaceOnTheCallersBehalfAndTheFullRoadIsSaid()
    {
        // A place handed a deployment is held as that deployment, and a place left out stands out: the fixture is
        // a passthrough, not a filler, so it cannot fill a place the caller meant to leave out.
        var tdd = WorkRecords.Deployment("2.4.80", WorkRecords.At(9, 16));
        var uat = WorkRecords.Deployment("2.4.81", WorkRecords.At(9, 17, 15));
        var two = WorkRecords.Environments(tdd: tdd, uat: uat);

        Assert.Same(tdd, two.Tdd);
        Assert.Same(uat, two.Uat);
        Assert.Null(two.Prod);

        var none = WorkRecords.Environments();

        Assert.Equal(WorkItemEnvironments.None, none);
        Assert.NotEqual(two, none);

        // The filled road is the one the item fixture stands on by default: tdd and prod said, uat the place that
        // has not got it, each place with its own release and its own moment.
        var full = WorkRecords.FullEnvironments();

        Assert.NotNull(full.Tdd);
        Assert.Null(full.Uat);
        Assert.NotNull(full.Prod);
        Assert.Equal("2.4.86", full.Tdd!.Version);
        Assert.Equal(WorkRecords.At(9, 23, 4), full.Tdd.DeployedAt);
        Assert.Equal("2.4.84", full.Prod!.Version);
        Assert.Equal(WorkRecords.At(8, 6), full.Prod.DeployedAt);
        Assert.NotEqual(full, none);
    }

    // ------------------------------------------------------------- the fully-filled records

    /// <summary>The board fixture is the board of the shipped <c>workitems.json</c>: its address, its project and
    /// its one sentence, each held as the letters it was given, and a board told a part otherwise is another board.
    /// The record says its three parts in its own words, which stand still under a reader of another culture.</summary>
    [Fact]
    public void TheScopeFixtureIsTheShippedBoardAndSaysItsPartsInStableWords()
    {
        var scope = WorkRecords.Scope();

        Assert.Equal("https://github.com/clearmeasure-aisf-sample-apps/cmdemo1-system", scope.Repo);
        Assert.Equal(679, scope.Project);
        Assert.Equal("A bot may take an item from Todo. Done means the release is deployed to prod.", scope.Rules);

        // A board told a part otherwise is another board, told apart by its equality and its words.
        Assert.NotEqual(scope, WorkRecords.Scope(project: 680));
        Assert.NotEqual(scope, WorkRecords.Scope(repo: "cmdemo1-workorders"));

        // The words the board says its three parts in stand still under a reader of another culture, which is the
        // question the per-record files ask of their own culture-pinned words, asked here of the shared fixture.
        var german = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        german.NumberFormat.NegativeSign = "~";
        var negative = WorkRecords.Scope(project: -5);

        Assert.Equal("WorkItemsScope { Repo = https://github.com/clearmeasure-aisf-sample-apps/cmdemo1-system, Project = -5, Rules = A bot may take an item from Todo. Done means the release is deployed to prod. }", CultureScope.Says(CultureInfo.InvariantCulture, () => negative.ToString()!));
        Assert.NotEqual(CultureScope.Says(CultureInfo.InvariantCulture, () => negative.ToString()!), CultureScope.Says(german, () => negative.ToString()!));
        Assert.Equal(CultureScope.Says(CultureInfo.InvariantCulture, () => WorkRecords.Scope().ToString()!), CultureScope.Says(CultureInfo.InvariantCulture, () => scope.ToString()!));
    }

    /// <summary>The item fixture is the item with every part said: the number, the title, the address, the state,
    /// the column, the history, the assignee, the agent, the sessions, the pulls, the release, the places, the cost,
    /// the worker, the rework and the note, each one of them stood where it stands and none of them the nothing it
    /// is not. A part told otherwise is another item, and two items built apart are two items although their parts
    /// say the same things, because the lists they carry are two lists.</summary>
    [Fact]
    public void TheItemFixtureSaysEveryPartAndTwoBuiltApartAreTwoItems()
    {
        var item = WorkRecords.Item();

        Assert.Equal(490, item.Number);
        Assert.Equal("Pin the board", item.Title);
        Assert.Equal("https://github.com/clearmeasure-aisf-sample-apps/cmdemo1-system/issues/490", item.Url!.AbsoluteUri);
        Assert.Equal("Active", item.State);
        Assert.Equal(WorkColumns.Review, item.Column);
        Assert.Equal("cm-ai-ops", item.Assignee);
        Assert.Equal("dashboard-bot", item.Agent);
        Assert.Equal(4.5, item.CostUsd);
        Assert.Equal(WorkWorker.Bot, item.Worker);

        Assert.Single(item.ColumnHistory);
        Assert.Equal(WorkColumns.Todo, item.ColumnHistory[0].Column);
        Assert.Single(item.CursorSessions);
        Assert.Equal("https://cursor.com/agents/bc-490", item.CursorSessions[0].AbsoluteUri);
        Assert.Equal(912, Assert.Single(item.PullRequests).Number);
        Assert.Equal("2.4.86", item.ReleaseCandidate!.Version);
        Assert.NotNull(item.Environments.Tdd);
        Assert.NotNull(item.Rework);
        Assert.Equal("blocker", item.Blocker!.Kind);

        // A part told otherwise is another item: the number, the worker and the column each, and the record's own
        // equality says it.
        Assert.NotEqual(item, WorkRecords.Item(number: 491));
        Assert.NotEqual(item, WorkRecords.Item(worker: WorkWorker.Team));
        Assert.NotEqual(item, WorkRecords.Item(column: WorkColumns.InProgress));

        // Two items built apart are two items although their parts say the same things, because the lists they
        // carry are two lists; a copy of one item, which carries the very lists of the original, is the item it was.
        var apart = WorkRecords.Item();

        Assert.NotSame(item, apart);
        Assert.NotSame(item.ColumnHistory, apart.ColumnHistory);
        Assert.NotEqual(item, apart);
        Assert.Equal(item.ColumnHistory.Select(visit => visit.Column), apart.ColumnHistory.Select(visit => visit.Column));

        var copy = item with { };

        Assert.Equal(item, copy);
        Assert.NotSame(item, copy);
        Assert.Same(item.ColumnHistory, copy.ColumnHistory);
        Assert.Same(item.CursorSessions, copy.CursorSessions);
        Assert.Same(item.PullRequests, copy.PullRequests);
        Assert.Single(new HashSet<WorkItem> { item, copy });
    }

    /// <summary>A part of the item fixture may be handed its own instance and comes back as the very instance it
    /// was given: the address, the three lists and the four records, held and not copied, trimmed or rebuilt on the
    /// way in. That is the question a constructor-handing test asks of its record's reference-typed parts, asked
    /// here of the shared fixture.</summary>
    [Fact]
    public void TheItemFixtureHoldsTheInstancesItWasGivenAndNotCopies()
    {
        var title = WorkRecords.Own("Pin the board");
        var history = new List<ColumnVisit> { WorkRecords.Visit() };
        var sessions = new List<Uri> { new("https://cursor.com/agents/bc-490") };
        var pulls = new List<WorkPullRequest> { WorkRecords.PullRequest() };
        var candidate = WorkRecords.Candidate();
        var environments = WorkRecords.Environments();
        var rework = WorkRecords.Rework();
        var blocker = WorkRecords.Note();

        var item = WorkRecords.Item(
            title: title,
            history: history,
            sessions: sessions,
            pulls: pulls,
            candidate: candidate,
            environments: environments,
            rework: rework,
            blocker: blocker);

        Assert.Same(title, item.Title);
        Assert.Same(history, item.ColumnHistory);
        Assert.Same(sessions, item.CursorSessions);
        Assert.Same(pulls, item.PullRequests);
        Assert.Same(candidate, item.ReleaseCandidate);
        Assert.Same(environments, item.Environments);
        Assert.Same(rework, item.Rework);
        Assert.Same(blocker, item.Blocker);
    }

    /// <summary>The duration fixture says all three numbers, and a duration told a part otherwise is another
    /// duration; the nothing the file that says nothing lands on is another duration than the nought it is not,
    /// told apart by its equality and its words, which stand still under a reader of another culture.</summary>
    [Fact]
    public void TheDurationFixtureSaysAllThreeNumbersAndStandsStillUnderAReader()
    {
        var duration = WorkRecords.Duration();

        Assert.Equal(6d, duration.Median);
        Assert.Equal(10.5d, duration.P85);
        Assert.Equal(3, duration.N);

        Assert.NotEqual(duration, WorkRecords.Duration(median: 6.5));
        Assert.NotEqual(duration, WorkRecords.Duration(p85: 11));
        Assert.NotEqual(duration, WorkRecords.Duration(n: 4));

        // The nothing and the nought are two durations, told apart by their words, and the words stand still under
        // a reader of another culture: a plain number carries no group of three digits, so the only difference
        // between one culture's words and another's is the sign below zero.
        var nothing = WorkRecords.Duration(null, null, null);
        var nought = WorkRecords.Duration(0, 0, 0);

        Assert.NotEqual(nothing, nought);
        Assert.NotEqual(CultureScope.Words(nothing), CultureScope.Words(nought));
        Assert.StartsWith("WorkDuration { Median = , P85 = , N =  }", CultureScope.Words(nothing), StringComparison.Ordinal);
        Assert.StartsWith("WorkDuration { Median = 0, P85 = 0, N = 0", CultureScope.Words(nought), StringComparison.Ordinal);
    }

    /// <summary>The report fixture is the report a filled <c>workitems.json</c> reads as: its moment, its board,
    /// its counts and its two items, with the release each one carries, and the bot and team splits said as slices
    /// of the same counts. The items it builds are the item fixture's, so a part of an item stands where the item
    /// fixture says it stands, and the release stands beside the items it carries.</summary>
    [Fact]
    public void TheReportFixtureComposesTheItemAndScopeFixtures()
    {
        var report = WorkRecords.Report();

        Assert.Equal(WorkRecords.At(9, 22), report.GeneratedAt);
        Assert.NotNull(report.Scope);
        Assert.True(report.Stats.HasAny);
        Assert.Equal(2, report.Items.Count);
        Assert.Equal([490, 491], report.Items.Select(item => item.Number));
        Assert.Equal(WorkColumns.Review, report.Items[0].Column);
        Assert.Equal(WorkColumns.InProgress, report.Items[1].Column);
        Assert.Equal(WorkWorker.Bot, report.Items[0].Worker);
        Assert.Equal(WorkWorker.Team, report.Items[1].Worker);
        Assert.Single(report.Releases);
        Assert.Equal("2.4.86", report.Releases[0].Version);
        Assert.Equal([490, 491], report.Releases[0].Items);
        Assert.NotNull(report.BotStats);
        Assert.NotNull(report.TeamStats);
        Assert.True(report.BotStats.HasAny);
        Assert.True(report.TeamStats.HasAny);

        // A part told otherwise is another report, told apart by its equality: the moment, the board, the items and
        // the release each.
        Assert.NotEqual(report, WorkRecords.Report(generatedAt: WorkRecords.At(10)));
        Assert.NotEqual(report, WorkRecords.Report(scope: WorkRecords.Scope(project: 680)));
        Assert.NotEqual(report, WorkRecords.Report(items: [WorkRecords.Item(number: 490)]));

        // The report says its parts in its own words, which stand still under a reader of another culture, and the
        // moment it was given stands in them as the invariant culture writes it.
        var words = CultureScope.Words(report);

        Assert.Contains("WorkItemsReport {", words, StringComparison.Ordinal);
        Assert.Contains("GeneratedAt = 11/09/2026 22:00:00 +00:00", words, StringComparison.Ordinal);
        Assert.Contains("Scope = WorkItemsScope {", words, StringComparison.Ordinal);
        Assert.Contains("Items = ", words, StringComparison.Ordinal);
        Assert.Contains("Releases = ", words, StringComparison.Ordinal);
        Assert.Contains("BotStats = WorkItemsStats {", words, StringComparison.Ordinal);
        Assert.Contains("TeamStats = WorkItemsStats {", words, StringComparison.Ordinal);
    }

    /// <summary>The report fixture's items are the item fixture's and not copies of them: the report built with the
    /// items the item fixture makes is the report it says it is, and a report built with two apart items is another
    /// report than the one built with the same letters, because the items are two items and their lists are two
    /// lists. That is the record's own equality, and it is what lets the page and the tests tell one reading of a
    /// file from another.</summary>
    [Fact]
    public void AReportBuiltWithApartItemsIsAnotherReportThanOneBuiltWithTheSameLetters()
    {
        var first = WorkRecords.Report();
        var rebuilt = WorkRecords.Report(items: [WorkRecords.Item(number: 490), WorkRecords.Item(number: 491, column: WorkColumns.InProgress, worker: WorkWorker.Team)]);

        Assert.Equal(first.Items.Select(item => item.Number), rebuilt.Items.Select(item => item.Number));
        Assert.NotEqual(first.Items[0], rebuilt.Items[0]);
        Assert.NotEqual(first.Items[1], rebuilt.Items[1]);
        Assert.Equal(4, new HashSet<WorkItem>(first.Items.Concat(rebuilt.Items)).Count);
    }
}
