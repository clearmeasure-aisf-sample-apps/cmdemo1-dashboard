using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="WorkItem" />, one item on the board: the sixteen
/// parts a line of <c>workitems.json</c> lands in, declared beside its file in
/// <c>src/Dashboard/Work/WorkItemsReport.cs</c> - the number on the board, the title, the address, the board's own
/// state word, the column it stands in now, the history of the columns it passed through, who it is assigned to, which
/// bot is handling it, the sessions that worked on it, the pull requests opened for it, the release cut for it, where
/// that release is deployed, what the agent cost, whose work it is, the work that had to be done again, and the note
/// that says what blocks it.
/// <para>
/// What the other files already pin is left alone. The reading of an item out of the file is
/// <c>WorkItem.Read</c>, which is internal to an app that declares no <c>InternalsVisibleTo</c> (as
/// <c>Dashboard.Tests.csproj</c> notes of <c>JsonRead</c>) and is therefore pinned through
/// <see cref="WorkItemsReport.Parse" />: <see cref="WorkItemsTests.EveryPartOfAnItemIsOptionalAndABadPartIsLeftOut" />
/// says that a title arrives trimmed, that a relative address and a scripted session are refused, that an item of no
/// number, no title, no address and no column is no item at all, that a release loses its build suffix, that an
/// environment is found whatever case it is named in and that a negative cost is not read;
/// <see cref="WorkItemsTests.TheFeedSampleUsesThePublishedNames" /> that the published names arrive as the worker, the
/// rework, the blocker's kind and the deployment moments of the items they belong to; and
/// <see cref="WorkItemsTests.TheSampleFileIsTheBoardTheViewShows" /> the same parts of the shipped sample. The words
/// the page makes of an item - the <c>#490</c>, the agent, the assignee, the places, the timeline, the cost - are
/// <see cref="WorkItemsText" /> and <see cref="WorkBotText" />, pinned in <see cref="WorkItemsTextTests" />,
/// <see cref="WorkBotTextTests" /> and on the board itself in <see cref="WorkItemsViewTests" />. The record beside it,
/// the board a file came from, is <see cref="WorkItemsScopeEdgeCaseTests" />.
/// </para>
/// <para>
/// What is left, and is pinned here, is the record itself: the constructor handing each of its sixteen arguments to
/// the property of its name and holding the very instance it was given; the three parts a caller may leave out
/// falling to <see cref="WorkWorker.Unknown" />, and to the nothing that is not an empty note or a rework of nought;
/// nothing refused - the item built on nothing at all is an item, with the lists unsaid as the unsaid thing rather
/// than the empty one; the five words kept in their exact letters, neither trimmed, padded, cut nor folded; the
/// number at the ends an <see cref="int" /> runs to and the cost at the ends a number runs to, the number that is no
/// number included; the address held as the instance it stands on and never opened, not even asked whether it is an
/// address; the three lists held as the instances they are, which is why two items written out of the same letters
/// are two items while a copy of one item is the same item; the parts coming back out in the order they went in and
/// the copy turning the one part it names while it keeps the other fifteen; the
/// two items being one while their parts are, with the hash, the set and the operators following that, and the
/// nothing on either side of each; the words the item says its sixteen parts in, and those words following whoever
/// reads a number or a moment; and the public surface, so that the day a seventeenth part is added the list below
/// says so.
/// </para>
/// <para>
/// Beside the record, the file's reading of the item's parts is pinned here through <see cref="WorkItemsReport.Parse" />
/// the way <see cref="WorkItemsTests.EveryPartOfAnItemIsOptionalAndABadPartIsLeftOut" /> says a bad part is left out:
/// the item that says only its number, only its title or only its column; a number of another type, sign or reach, and
/// the float the number it rounds to; a title or a column of another type; an address that is no address; a session, a
/// pull request, a release candidate, an environments, a cost, a worker, a rework or a note that is not one; and the
/// item that is no object at all.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every moment below is one the test
/// brought, and the addresses - the issue, the session, the pull request, the note - are held, said and compared as
/// letters only. The record opens nothing on them; that the page links to an item's address is
/// <c>WorkRows.razor</c>'s, and the addresses of the board itself are the scope's.
/// </para>
/// </summary>
public class WorkItemEdgeCaseTests
{
    private const string Issue = "https://github.com/clearmeasure-aisf-sample-apps/cmdemo1-system/issues/490";
    private const string Session = "https://cursor.com/agents/bc-490";
    private const string PullAddress = "https://github.com/clearmeasure-aisf-sample-apps/cmdemo1-system/pull/912";
    private const string Comment = "https://github.com/clearmeasure-aisf-sample-apps/cmdemo1-system/issues/490#issuecomment-1";

    /// <summary>A moment of the test's own, so that no part of an item is ever asked what time it is.</summary>
    private static DateTimeOffset At(int day, int hour = 0, int minute = 0) => new(2026, 11, day, hour, minute, 0, TimeSpan.Zero);

    /// <summary>A word of the test's own, so that a part held as the very instance it was given is proved by the
    /// holding and not by the interning of a literal.</summary>
    private static string Own(string word) => new(word.ToCharArray());

    /// <summary>An item with every part said with a word, a number, a moment or an address of its own, so that a part
    /// landing on another part's place is seen rather than quietly matching a third of the same record.</summary>
    private static WorkItem Item(
        int? number = 490,
        string? title = "Pin the board",
        Uri? url = null,
        string? state = "Active",
        string? column = "Review",
        IReadOnlyList<ColumnVisit>? history = null,
        string? assignee = "cm-ai-ops",
        string? agent = "dashboard-bot",
        IReadOnlyList<Uri>? sessions = null,
        IReadOnlyList<WorkPullRequest>? pulls = null,
        ReleaseCandidate? candidate = null,
        WorkItemEnvironments? environments = null,
        double? cost = 4.5,
        WorkWorker worker = WorkWorker.Bot,
        WorkRework? rework = null,
        WorkNote? blocker = null) =>
        new(
            number,
            title,
            url ?? new Uri(Issue),
            state,
            column,
            history ?? [],
            assignee,
            agent,
            sessions ?? [],
            pulls ?? [],
            candidate,
            environments ?? WorkItemEnvironments.None,
            cost,
            worker,
            rework,
            blocker);

    /// <summary>The sixteen parts of an item, each one said: the record the tests below turn a part of, and hand back
    /// whole wherever the equality of a list is not what is being asked about.</summary>
    private static WorkItem Full() =>
        Item(
            history: [new ColumnVisit(WorkColumns.Todo, At(9, 8))],
            sessions: [new Uri(Session)],
            pulls: [new WorkPullRequest(912, new Uri(PullAddress), "closed", At(9, 22), "abcdef1234567890", "failed", null)],
            candidate: new ReleaseCandidate("2.4.86", At(9, 23)),
            environments: new WorkItemEnvironments(new WorkDeployment("2.4.86", At(9, 23, 4)), null, new WorkDeployment("2.4.84", At(8, 6))),
            rework: new WorkRework(1, 0, 0.125),
            blocker: new WorkNote("Functional tests failed", new Uri(Comment), At(10), "blocker"));

    // ------------------------- the constructor, handing each argument to the property of its name

    /// <summary>The constructor's sixteen arguments, position by position, come back as the property of the same
    /// name, in the order the file names them. Reference-typed parts are held as the very instances the item was
    /// built of - the words, the address, the three lists, the four records - and none of them is copied, trimmed or
    /// rebuilt on the way in; the numbers and the worker stand on the values they were given. A second item of the
    /// same sixteen parts is another item, never the one it was built from.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var title = Own("Pin the board");
        var state = Own("Active");
        var column = Own("Review");
        var assignee = Own("cm-ai-ops");
        var agent = Own("dashboard-bot");
        var url = new Uri(Issue);
        List<ColumnVisit> history = [new ColumnVisit(WorkColumns.Todo, At(9, 8))];
        List<Uri> sessions = [new Uri(Session)];
        List<WorkPullRequest> pulls = [new WorkPullRequest(912, null, "closed", null, null)];
        var candidate = new ReleaseCandidate("2.4.86", At(9, 23));
        var environments = new WorkItemEnvironments(new WorkDeployment("2.4.86", At(9, 23, 4)), null, null);
        var rework = new WorkRework(1, 0, 0.125);
        var blocker = new WorkNote("Functional tests failed", null, At(10), "blocker");

        var item = new WorkItem(
            490,
            title,
            url,
            state,
            column,
            history,
            assignee,
            agent,
            sessions,
            pulls,
            candidate,
            environments,
            4.5,
            WorkWorker.Bot,
            rework,
            blocker);

        Assert.Equal(490, item.Number);
        Assert.Same(title, item.Title);
        Assert.Same(url, item.Url);
        Assert.Same(state, item.State);
        Assert.Same(column, item.Column);
        Assert.Same(history, item.ColumnHistory);
        Assert.Same(assignee, item.Assignee);
        Assert.Same(agent, item.Agent);
        Assert.Same(sessions, item.CursorSessions);
        Assert.Same(pulls, item.PullRequests);
        Assert.Same(candidate, item.ReleaseCandidate);
        Assert.Same(environments, item.Environments);
        Assert.Equal(4.5, item.CostUsd);
        Assert.Equal(WorkWorker.Bot, item.Worker);
        Assert.Same(rework, item.Rework);
        Assert.Same(blocker, item.Blocker);

        Assert.Equal("Pin the board", item.Title);
        Assert.Equal(Issue, item.Url!.AbsoluteUri);
        Assert.Equal(WorkColumns.Todo, Assert.Single(item.ColumnHistory).Column);
        Assert.Equal(Session, Assert.Single(item.CursorSessions).AbsoluteUri);
        Assert.Equal(912, Assert.Single(item.PullRequests).Number);
        Assert.Equal("2.4.86", item.ReleaseCandidate!.Version);
        Assert.Equal(new DateTimeOffset(2026, 11, 9, 23, 4, 0, TimeSpan.Zero), item.Environments.Tdd!.DeployedAt);
        Assert.Null(item.Environments.Uat);
        Assert.Equal(0.125, item.Rework!.Rate);
        Assert.Equal("blocker", item.Blocker!.Kind);

        var again = new WorkItem(
            490,
            title,
            url,
            state,
            column,
            history,
            assignee,
            agent,
            sessions,
            pulls,
            candidate,
            environments,
            4.5,
            WorkWorker.Bot,
            rework,
            blocker);

        Assert.NotSame(item, again);
        Assert.Equal(item, again);
    }

    /// <summary>The last three parts are the three a caller may leave out, and they fall as they fall: whose work it
    /// is comes down as <see cref="WorkWorker.Unknown" /> - which is a word of its own and not the absence of one -
    /// while the rework and the blocker stand unsaid rather than at a rework of nought or an empty note. An item that
    /// says the three of them as they fall is the item it would be without saying them, and another item as soon as
    /// any of the three is said otherwise.</summary>
    [Fact]
    public void TheThreePartsACallerMayLeaveOutFallToUnknownAndToNothing()
    {
        var left = new WorkItem(490, "Pin the board", null, null, "Review", [], null, null, [], [], null, WorkItemEnvironments.None, null);
        var said = new WorkItem(
            490,
            "Pin the board",
            null,
            null,
            "Review",
            [],
            null,
            null,
            [],
            [],
            null,
            WorkItemEnvironments.None,
            null,
            WorkWorker.Unknown,
            null,
            null);

        Assert.Equal(WorkWorker.Unknown, left.Worker);
        Assert.Null(left.Rework);
        Assert.Null(left.Blocker);
        Assert.Equal(left, said);
        Assert.Equal(left.GetHashCode(), said.GetHashCode());
        Assert.Single(new HashSet<WorkItem> { left, said });

        // Unknown is a word and not the nothing of the other two: a worker is always said, and the two parts that are
        // not are the parts the file may leave out entirely.
        Assert.Equal(0, (int)WorkWorker.Unknown);
        Assert.NotEqual(left, left with { Worker = WorkWorker.Bot });
        Assert.NotEqual(left, left with { Worker = WorkWorker.Team });
        Assert.NotEqual(left, left with { Rework = new WorkRework(null, null, null) });
        Assert.NotEqual(left, left with { Blocker = new WorkNote(null, null, null) });
    }

    /// <summary>The record guards nothing and refuses nothing: an item may be built on the nothing of every one of its
    /// parts, with the three lists unsaid as well - and it stands as it was built, saying the nothing it was given and
    /// keeping the worker's fall. The unsaid list is another item than the list with nothing in it, which is what lets
    /// a caller tell an item whose file wrote no <c>prs</c> from one whose file wrote an empty array; the reading that
    /// makes both of them empty is <c>WorkItem.Read</c>, pinned in
    /// <see cref="WorkItemsTests" />. What the page shows of such an item is the page's, and
    /// <see cref="WorkItemsText.Number" /> says it <c>Item</c>.</summary>
    [Fact]
    public void AnItemBuiltOnNothingIsStillAnItemAndRefusesNothing()
    {
        var nothing = new WorkItem(null, null, null, null, null, null!, null, null, null!, null!, null, null!, null);

        Assert.Null(nothing.Number);
        Assert.Null(nothing.Title);
        Assert.Null(nothing.Url);
        Assert.Null(nothing.State);
        Assert.Null(nothing.Column);
        Assert.Null(nothing.ColumnHistory);
        Assert.Null(nothing.Assignee);
        Assert.Null(nothing.Agent);
        Assert.Null(nothing.CursorSessions);
        Assert.Null(nothing.PullRequests);
        Assert.Null(nothing.ReleaseCandidate);
        Assert.Null(nothing.Environments);
        Assert.Null(nothing.CostUsd);
        Assert.Equal(WorkWorker.Unknown, nothing.Worker);
        Assert.Null(nothing.Rework);
        Assert.Null(nothing.Blocker);

        Assert.Equal(nothing, new WorkItem(null, null, null, null, null, null!, null, null, null!, null!, null, null!, null));
        Assert.True(nothing == new WorkItem(null, null, null, null, null, null!, null, null, null!, null!, null, null!, null));
        Assert.Equal("WorkItem { Number = , Title = , Url = , State = , Column = , ColumnHistory = , Assignee = , Agent = , CursorSessions = , PullRequests = , ReleaseCandidate = , Environments = , CostUsd = , Worker = Unknown, Rework = , Blocker =  }", Words(nothing));

        // The empty lists are a list with nothing in it: another item than the one whose lists are not there at all,
        // and the one the file's <c>"prs": []</c> arrives as.
        var empty = new WorkItem(null, null, null, null, null, [], null, null, [], [], null, WorkItemEnvironments.None, null);

        Assert.NotNull(empty.ColumnHistory);
        Assert.Empty(empty.ColumnHistory);
        Assert.Empty(empty.CursorSessions);
        Assert.Empty(empty.PullRequests);
        Assert.Equal(WorkItemEnvironments.None, empty.Environments);
        Assert.Same(WorkItemEnvironments.None, empty.Environments);
        Assert.NotEqual(nothing, empty);
        Assert.True(nothing != empty);
        Assert.Equal(2, new HashSet<WorkItem> { nothing, empty }.Count);
        Assert.NotEqual(Words(nothing), Words(empty));
    }

    // ---------------------------------- the five words, kept exactly as they are given

    /// <summary>Each of the five words of an item - the title, the board's own state word, the column, the assignee
    /// and the agent - is kept in its exact letters: the word of no characters, the word that is only whitespace, a
    /// word padded at both ends, a word in another case, a word with two spaces between its parts and a word that is
    /// no column of the board at all all stand where they stand. Nothing here trims, lowers, folds, shortens or turns
    /// a column into the board's canonical name: the trimming and the canonicalising are the file's reading and the
    /// page's saying, pinned in <see cref="WorkItemsTests" /> and in <see cref="WorkItemsTextTests" />, and the record
    /// repeats what it was told.</summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   \t ")]
    [InlineData("Pin the board")]
    [InlineData("Pin  the  board")]
    [InlineData("  Pin the board  ")]
    [InlineData("pin the board")]
    [InlineData("PIN THE BOARD")]
    [InlineData("Ünknown ✓")]
    [InlineData("blocked")]
    [InlineData("cm-ai-ops")]
    [InlineData("Dashboard-Bot")]
    public void AWordOfAnItemIsKeptExactlyAsItIsGiven(string word)
    {
        var own = Own(word);
        var item = new WorkItem(490, own, null, own, own, [], own, own, [], [], null, WorkItemEnvironments.None, null);

        Assert.Same(own, item.Title);
        Assert.Same(own, item.State);
        Assert.Same(own, item.Column);
        Assert.Same(own, item.Assignee);
        Assert.Same(own, item.Agent);
        Assert.Equal(word, item.Title);
        Assert.Equal(word, item.Column);

        // The board's own column words are letters like any other to the record: an item may stand on "todo" while
        // the board spells it "Todo", and the two are two items. Which column an item is counted in is
        // WorkColumns' asking, pinned in WorkItemsTests.
        Assert.NotEqual(new WorkItem(490, null, null, null, WorkColumns.Todo, [], null, null, [], [], null, WorkItemEnvironments.None, null), item);
        Assert.Contains(word, Words(new WorkItem(490, word, null, null, null, [], null, null, [], [], null, WorkItemEnvironments.None, null)), StringComparison.Ordinal);
    }

    /// <summary>The empty word is a word and not the absence of one: an item titled nothing at all and an item with no
    /// title are two items, one of them with an empty place and the other with no place filled, and the record does
    /// not settle which of the two the page should leave out. The same holds of the state, the column, the assignee
    /// and the agent - and of a title of a thousand letters, which is kept whole and said whole because the cutting to
    /// a line is the page's. Only the equality, the hash and the set tell the empty word from no word at all: the
    /// record says a word as the letters it has, and there are none to say.</summary>
    [Fact]
    public void AnEmptyWordIsAWordAndNotTheAbsenceOfOne()
    {
        var empty = new WorkItem(490, string.Empty, null, string.Empty, string.Empty, [], string.Empty, string.Empty, [], [], null, WorkItemEnvironments.None, null);
        var unsaid = new WorkItem(490, null, null, null, null, [], null, null, [], [], null, WorkItemEnvironments.None, null);

        Assert.Equal(string.Empty, empty.Title);
        Assert.Equal(string.Empty, empty.Column);
        Assert.Equal(string.Empty, empty.Assignee);
        Assert.Equal(string.Empty, empty.Agent);
        Assert.Equal(string.Empty, empty.State);
        Assert.Null(unsaid.Title);
        Assert.Null(unsaid.State);
        Assert.Null(unsaid.Column);
        Assert.Null(unsaid.Assignee);
        Assert.Null(unsaid.Agent);

        Assert.NotEqual(empty, unsaid);
        Assert.False(empty == unsaid);
        Assert.True(empty != unsaid);
        Assert.NotEqual(empty.GetHashCode(), unsaid.GetHashCode());
        Assert.Equal(2, new HashSet<WorkItem> { empty, unsaid }.Count);

        // The record's own words say the empty word and no word the one way: it is the equality, the hash and the set
        // that keep them apart, and the page's word for a part that is nothing is a dash this record has never heard
        // of - which part the page leaves out is the reading's, pinned in WorkItemsTests.
        Assert.Equal(Words(empty), Words(unsaid));
        Assert.DoesNotContain("—", Words(unsaid), StringComparison.Ordinal);
        Assert.Equal("WorkItem { Number = 490, Title = , Url = , State = , Column = , ColumnHistory = Dashboard.Work.ColumnVisit[], Assignee = , Agent = , CursorSessions = System.Uri[], PullRequests = Dashboard.Work.WorkPullRequest[], ReleaseCandidate = , Environments = WorkItemEnvironments { Tdd = , Uat = , Prod =  }, CostUsd = , Worker = Unknown, Rework = , Blocker =  }", Words(unsaid));

        var longTitle = new string('t', 1000);
        var longItem = unsaid with { Title = longTitle };

        Assert.Same(longTitle, longItem.Title);
        Assert.Equal(1000, longItem.Title!.Length);
        Assert.Contains(longTitle, Words(longItem), StringComparison.Ordinal);
        Assert.DoesNotContain("…", Words(longItem), StringComparison.Ordinal);
        Assert.Equal(longItem, new WorkItem(490, new string('t', 1000), null, null, null, [], null, null, [], [], null, WorkItemEnvironments.None, null));
    }

    /// <summary>The five words of an item are compared in their own letters and not as a reader hears them: the same
    /// title in another case, padded with a space, or with one space instead of two is another item, whichever reader
    /// is reading. The lookup that finds a column whatever case it is named in is elsewhere in the app - a column's
    /// counts are keyed that way - and an item is not looked up, it is stood on.</summary>
    [Fact]
    public void TheWordsOfAnItemAreComparedInTheirOwnLettersAndNotAsAReaderHearsThem()
    {
        var item = Item();

        Assert.True(StringComparer.OrdinalIgnoreCase.Equals("dashboard-bot", item.Agent));
        Assert.NotEqual(item, item with { Agent = "DASHBOARD-BOT" });
        Assert.NotEqual(item, item with { Agent = " dashboard-bot" });
        Assert.NotEqual(item, item with { Title = "pin the board" });
        Assert.NotEqual(item, item with { Title = "Pin the  board" });
        Assert.NotEqual(item, item with { State = "active" });
        Assert.NotEqual(item, item with { Column = "review" });
        Assert.NotEqual(item, item with { Assignee = "CM-AI-OPS" });

        Assert.Equal(
            5,
            new HashSet<WorkItem>
            {
                item,
                item with { Agent = "DASHBOARD-BOT" },
                item with { Title = "pin the board" },
                item with { Column = WorkColumns.Review.ToLowerInvariant() },
                item with { Assignee = "cm-ai-ops " },
            }.Count);
    }

    // ------------------------------------------- the number of the item, at the ends a number runs to

    /// <summary>The number on the board stands where it is put, at the ends an <see cref="int" /> runs to, below zero
    /// and at zero: the record adds nothing to it, rounds nothing, caps nothing and refuses nothing - not even a
    /// number no board could be numbered by. The reading that refuses a negative number and rounds a fraction is
    /// <c>JsonRead.Count</c>, pinned in <see cref="WorkItemsTests" /> and in
    /// <see cref="ParsedEdgeCaseTests" />; the item itself keeps the number it was built on.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(490)]
    [InlineData(-1)]
    [InlineData(-2147483648)]
    [InlineData(2147483647)]
    public void TheNumberStandsAtTheEndsANumberRunsTo(int number)
    {
        var item = Item(number: number);

        Assert.Equal(number, item.Number);
        Assert.Equal(number, (Item(number: number) with { }).Number);
        Assert.Equal(item, Item(number: number));
        Assert.NotEqual(item, Item(number: null));
        Assert.NotEqual(item, Item(number: number == 0 ? 1 : 0));
        Assert.Contains($"Number = {number.ToString(CultureInfo.InvariantCulture)}", Words(item), StringComparison.Ordinal);
    }

    /// <summary>The number of an item that has none stands unsaid, which is not the number nought: the record whose
    /// number is nothing is a record, and another record than the one numbered nought - told apart by its equality,
    /// its operators, its words and its place in a set. That is what lets the page say <c>Item</c> rather than
    /// <c>#0</c>, which is <see cref="WorkItemsText.Number" /> and is pinned in
    /// <see cref="RuntimeWorkHelperCoverageTests" />.</summary>
    [Fact]
    public void ANumberNobodyGaveStandsUnsaidAndNotAtNought()
    {
        var unsaid = Item(number: null);
        var nought = Item(number: 0);

        Assert.Null(unsaid.Number);
        Assert.Equal(0, nought.Number);
        Assert.NotEqual(unsaid, nought);
        Assert.False(unsaid == nought);
        Assert.True(unsaid != nought);
        Assert.NotEqual(Words(unsaid), Words(nought));
        Assert.Equal(2, new HashSet<WorkItem> { unsaid, nought }.Count);
        Assert.StartsWith("WorkItem { Number = ,", Words(unsaid), StringComparison.Ordinal);
        Assert.StartsWith("WorkItem { Number = 0,", Words(nought), StringComparison.Ordinal);

        var turnOff = Item(number: 490) with { Number = null };

        Assert.Null(turnOff.Number);
        Assert.Equal(unsaid, turnOff);
        Assert.NotEqual(nought, turnOff);
    }

    // ----------------------------- the cost of the item, at the ends a real number runs to

    /// <summary>The dollars an agent cost for an item stand as they were given, at the ends a number runs to and past
    /// the ends of counting: nought - which is a metered item that cost nothing and not an item nobody metered - a
    /// thousandth of a cent, the largest and the least number there are, the number below zero that the file's reading
    /// refuses, and the two numbers that are no number at all. Nothing rounds, caps or clamps: the money's words are
    /// <see cref="WorkItemsText.ItemCost" />, pinned in <see cref="WorkItemsTests" />.</summary>
    [Fact]
    public void TheCostStandsAtTheEndsANumberRunsTo()
    {
        var nought = Item(cost: 0);
        Assert.Equal(0d, nought.CostUsd);
        Assert.NotEqual(nought, Item(cost: null));

        var below = Item(cost: -4);
        Assert.Equal(-4d, below.CostUsd);

        var small = Item(cost: 0.0000001);
        Assert.Equal(1e-07, small.CostUsd);
        Assert.NotEqual(0d, small.CostUsd);

        var ends = Item(cost: double.MaxValue);
        Assert.Equal(double.MaxValue, ends.CostUsd);
        Assert.NotEqual(ends, Item(cost: double.MinValue));
        Assert.Equal(double.MinValue, Item(cost: double.MinValue).CostUsd);

        // The number that is no number is held as it is given, and the record's own equality says an item is itself
        // even on it - which is what a set of items has to be able to do, since a meter that never answered writes
        // exactly that.
        var notANumber = Item(cost: double.NaN);
        Assert.True(double.IsNaN(notANumber.CostUsd!.Value));
        Assert.Equal(notANumber, Item(cost: double.NaN));
        Assert.Single(new HashSet<WorkItem> { notANumber, Item(cost: double.NaN) });

        var beyond = Item(cost: double.PositiveInfinity);
        Assert.True(double.IsPositiveInfinity(beyond.CostUsd!.Value));
        Assert.NotEqual(beyond, Item(cost: double.NegativeInfinity));
        Assert.NotEqual(beyond, notANumber);
    }

    /// <summary>The nought that is below itself and the nought that is not are one cost to the record and two words:
    /// the equality follows the number, which knows no difference, and the item's own words repeat the sign the number
    /// carries. Neither of them is the cost nobody said.</summary>
    [Fact]
    public void ANegativeNoughtIsOneCostWithThePlainNoughtAndSaidApart()
    {
        var below = Item(cost: -0.0);
        var plain = Item(cost: 0.0);
        var unsaid = Item(cost: null);

        Assert.Equal(0d, below.CostUsd);
        Assert.Equal(below, plain);
        Assert.True(below == plain);
        Assert.Equal(below.GetHashCode(), plain.GetHashCode());
        Assert.Single(new HashSet<WorkItem> { below, plain });

        Assert.Contains("CostUsd = -0", Words(below), StringComparison.Ordinal);
        Assert.Contains("CostUsd = 0,", Words(plain), StringComparison.Ordinal);
        Assert.NotEqual(Words(below), Words(plain));

        Assert.NotEqual(below, unsaid);
        Assert.NotEqual(plain, unsaid);
    }

    // --------------------------------------------------- the address, held and never opened

    /// <summary>The item's page is held as the very <see cref="Uri" /> the item was built on, and nothing is asked of
    /// it beyond the letters it says itself: the record does not check that it is an address the dashboard may link
    /// to - that is <c>JsonRead.Address</c>, which refuses a relative address and a scripted one, pinned in
    /// <see cref="WorkItemsTests" /> - so an address written as a path, a name of another scheme, or one that would
    /// not open at all is kept exactly as it was handed over. Two addresses written apart are one address to the
    /// record, since an address compares as the address it is; two paths of the same letters in another case are not.
    /// No test below opens one, and none of them has a host to reach.</summary>
    [Fact]
    public void TheUrlIsHeldAsTheAddressItWasGivenAndNeverOpened()
    {
        var address = new Uri(Issue);
        var item = new WorkItem(490, null, address, null, null, [], null, null, [], [], null, WorkItemEnvironments.None, null);

        Assert.Same(address, item.Url);
        Assert.Equal(Issue, item.Url!.AbsoluteUri);
        Assert.Equal(item, new WorkItem(490, null, new Uri(Issue), null, null, [], null, null, [], [], null, WorkItemEnvironments.None, null));
        Assert.Equal(item.GetHashCode(), new WorkItem(490, null, new Uri(Issue), null, null, [], null, null, [], [], null, WorkItemEnvironments.None, null).GetHashCode());

        // Another address is another item, whichever part of it differs: the scheme, the host's letters, or the path.
        Assert.NotEqual(item, new WorkItem(490, null, new Uri("http://github.com/clearmeasure-aisf-sample-apps/cmdemo1-system/issues/490"), null, null, [], null, null, [], [], null, WorkItemEnvironments.None, null));
        Assert.NotEqual(item, new WorkItem(490, null, new Uri("https://github.com/clearmeasure-aisf-sample-apps/cmdemo1/issues/490"), null, null, [], null, null, [], [], null, WorkItemEnvironments.None, null));
        Assert.NotEqual(item, new WorkItem(490, null, new Uri("https://github.com/clearmeasure-aisf-sample-apps/cmdemo1-system/issues/491"), null, null, [], null, null, [], [], null, WorkItemEnvironments.None, null));
        Assert.NotEqual(item, new WorkItem(490, null, null, null, null, [], null, null, [], [], null, WorkItemEnvironments.None, null));

        // The host's letters are the address's own canonicalising, not the item's doing, and the path's are not.
        Assert.Equal(item, new WorkItem(490, null, new Uri("https://GitHub.COM/clearmeasure-aisf-sample-apps/cmdemo1-system/issues/490"), null, null, [], null, null, [], [], null, WorkItemEnvironments.None, null));
        Assert.NotEqual(item, new WorkItem(490, null, new Uri("https://github.com/clearmeasure-aisf-sample-apps/cmdemo1-system/issues/490".ToUpperInvariant()), null, null, [], null, null, [], [], null, WorkItemEnvironments.None, null));

        // The address the file's reading refuses is an address to the record, and it is said in the item's own words.
        var relative = new WorkItem(490, null, new Uri("/issues/490", UriKind.RelativeOrAbsolute), null, null, [], null, null, [], [], null, WorkItemEnvironments.None, null);

        Assert.False(relative.Url!.IsAbsoluteUri);
        Assert.Equal("/issues/490", relative.Url.ToString());
        Assert.Contains("Url = /issues/490,", Words(relative), StringComparison.Ordinal);
        Assert.Equal(relative, new WorkItem(490, null, new Uri("/issues/490", UriKind.RelativeOrAbsolute), null, null, [], null, null, [], [], null, WorkItemEnvironments.None, null));

        var scripted = new WorkItem(490, null, new Uri("javascript:alert(1)", UriKind.RelativeOrAbsolute), null, null, [], null, null, [], [], null, WorkItemEnvironments.None, null);

        Assert.Equal("javascript:alert(1)", scripted.Url!.ToString());
        Assert.NotEqual(item, scripted);
    }

    // ------------------------------- the three lists, held as the instances they stand on

    /// <summary>The columns an item passed through, the sessions that worked on it and the pull requests opened for it
    /// are the very lists the item was built on, in the file's own order and with nothing added to them: the visits
    /// come back first in first out, as do the addresses and the pulls, and a list with nothing in it is an empty list
    /// rather than the nothing of a list nobody gave. The record neither sorts, trims, deduplicates nor copies them - a
    /// session the file wrote twice is held twice, and a list turned after the item was built is the very list seen
    /// through the item - which is what the timeline, the waits and the places are built on, and
    /// <see cref="WorkBotText" /> and <see cref="WorkItemsText.Places" /> are pinned in
    /// <see cref="WorkBotTextTests" />.</summary>
    [Fact]
    public void TheThreeListsAreHeldAsTheInstancesTheyStandOn()
    {
        List<ColumnVisit> history =
        [
            new ColumnVisit(WorkColumns.Todo, At(9, 6)),
            new ColumnVisit(WorkColumns.InProgress, At(9, 8), At(9, 12), 4),
            new ColumnVisit(WorkColumns.Review, At(9, 12)),
        ];
        List<Uri> sessions = [new Uri("https://cursor.com/agents/bc-1"), new Uri("https://cursor.com/agents/bc-2"), new Uri("https://cursor.com/agents/bc-1")];
        List<WorkPullRequest> pulls =
        [
            new WorkPullRequest(912, new Uri(PullAddress), "closed", At(9, 22), "abcdef1234567890", "failed", null),
            new WorkPullRequest(913, null, "open", null, null),
        ];

        var item = Item(history: history, sessions: sessions, pulls: pulls);

        Assert.Same(history, item.ColumnHistory);
        Assert.Same(sessions, item.CursorSessions);
        Assert.Same(pulls, item.PullRequests);

        Assert.Equal(3, item.ColumnHistory.Count);
        Assert.Equal([WorkColumns.Todo, WorkColumns.InProgress, WorkColumns.Review], item.ColumnHistory.Select(visit => visit.Column));
        Assert.Null(item.ColumnHistory[0].LeftAt);
        Assert.Equal(4d, item.ColumnHistory[1].Hours);
        Assert.Equal(At(9, 12), item.ColumnHistory[2].EnteredAt);

        Assert.Equal(3, item.CursorSessions.Count);
        Assert.Equal("https://cursor.com/agents/bc-1", item.CursorSessions[0].AbsoluteUri);
        Assert.Equal("https://cursor.com/agents/bc-2", item.CursorSessions[1].AbsoluteUri);
        Assert.NotSame(item.CursorSessions[0], item.CursorSessions[2]);
        Assert.Equal(item.CursorSessions[0], item.CursorSessions[2]);

        Assert.Equal(2, item.PullRequests.Count);
        Assert.Equal([912, 913], item.PullRequests.Select(pull => pull.Number));
        Assert.Equal("failed", item.PullRequests[0].Checks);
        Assert.Null(item.PullRequests[1].Checks);

        // The empty list is a list, and it is held as it is given: an item with no pulls at all is an item, and a set
        // of two such items is one item, since the empty list of its type is the one list.
        var bare = Item(history: [], sessions: [], pulls: []);

        Assert.Empty(bare.ColumnHistory);
        Assert.Empty(bare.CursorSessions);
        Assert.Empty(bare.PullRequests);
        Assert.Equal(bare, Item());

        // A list turned after the item was built is the very same list, seen through the item.
        history.Add(new ColumnVisit(WorkColumns.Merged, At(9, 20)));

        Assert.Equal(4, item.ColumnHistory.Count);
        Assert.Equal(WorkColumns.Merged, item.ColumnHistory[3].Column);
    }

    /// <summary>A list is compared as the instance it stands on, and not as the parts it holds: two items written out
    /// of one file at two readings are two items, although every one of their parts says the same thing, because the
    /// lists they carry are two lists - while an item copied with <c>with</c>, which carries the very lists of the
    /// original, is the item it was copied from. A part that is a record rather than a list - the release candidate,
    /// the environments, the rework, the blocker - is compared as the record it is, whichever instance it stands in.
    /// This is what the page never asks of an item - it shows the item it read - and what the tests of this file rely on
    /// for telling one item from another: the identity of the lists, while the parts themselves are pinned where the
    /// file is read.</summary>
    [Fact]
    public void TwoItemsWrittenApartAreTwoItemsBecauseTheirListsAreTwoLists()
    {
        const string Board = """{ "items": [ { "number": 1, "column": "Todo", "prs": [ { "number": 9 } ] }, { "number": 2, "column": "Todo" } ] }""";
        var first = WorkItemsReport.Parse(Board)!.Items;
        var second = WorkItemsReport.Parse(Board)!.Items;

        Assert.Equal(first.Select(item => item.Number), second.Select(item => item.Number));
        Assert.Equal(first.Select(item => item.Column), second.Select(item => item.Column));
        Assert.Equal(Assert.Single(first[0].PullRequests).Number, Assert.Single(second[0].PullRequests).Number);
        Assert.NotEqual(first[0], second[0]);
        Assert.NotEqual(first[1], second[1]);
        Assert.Equal(4, new HashSet<WorkItem>(first.Concat(second)).Count);

        // The same item read twice is another item; the same item copied is the item it was.
        var one = Full();
        var copy = one with { };

        Assert.NotSame(one, copy);
        Assert.Equal(one, copy);
        Assert.Equal(one.GetHashCode(), copy.GetHashCode());
        Assert.Same(one.ColumnHistory, copy.ColumnHistory);
        Assert.Same(one.PullRequests, copy.PullRequests);
        Assert.Single(new HashSet<WorkItem> { one, copy });

        // A second list of the same visits is another list, and so another item, while the very list shared between
        // two items is one list and makes one item of them - and the visits themselves are one visit either way.
        List<ColumnVisit> history = [new ColumnVisit(WorkColumns.Todo, At(9, 8))];
        var left = Item(history: history);
        var right = Item(history: history);
        var apart = Item(history: [new ColumnVisit(WorkColumns.Todo, At(9, 8))]);

        Assert.Same(left.ColumnHistory, right.ColumnHistory);
        Assert.Equal(left, right);
        Assert.NotSame(left.ColumnHistory, apart.ColumnHistory);
        Assert.Equal(Assert.Single(left.ColumnHistory), Assert.Single(apart.ColumnHistory));
        Assert.NotEqual(left, apart);
        Assert.Equal(one, one with { ColumnHistory = one.ColumnHistory });
        Assert.NotEqual(one, one with { ColumnHistory = [new ColumnVisit(WorkColumns.Todo, At(9, 8))] });

        // A part that is a record rather than a list is compared as the record it is, in whichever instance it stands.
        Assert.Equal(one, one with { ReleaseCandidate = new ReleaseCandidate("2.4.86", At(9, 23)) });
        Assert.NotEqual(one, one with { ReleaseCandidate = new ReleaseCandidate("2.4.87", At(9, 23)) });
        Assert.Equal(one, one with { Environments = new WorkItemEnvironments(new WorkDeployment("2.4.86", At(9, 23, 4)), null, new WorkDeployment("2.4.84", At(8, 6))) });
    }

    // --------------------- the four records an item is made of, and the two that may be unsaid

    /// <summary>The release cut for an item, the environments running it, the work that had to be done again and the
    /// note that says what blocks it are held as the very records the item was built on, each part of them standing
    /// where it stands: the release's version as it was written - the record knows nothing of the build suffix the
    /// file's reading cuts - and its moment, the three places of the deployment with the two the file did not write
    /// standing unsaid, the two counts and the rate of a rework, and the four parts of a note. None of the four is
    /// worked out of the others, and none of them is asked what time it is.</summary>
    [Fact]
    public void TheFourRecordsAreHeldAsTheRecordsTheyWereGiven()
    {
        var candidate = new ReleaseCandidate("2.4.86+bbccdd", At(9, 23));
        var environments = new WorkItemEnvironments(new WorkDeployment("2.4.86", At(9, 23, 4)), null, new WorkDeployment("2.4.84", At(8, 6)));
        var rework = new WorkRework(1, 2, 0.125);
        var blocker = new WorkNote("Functional tests failed", new Uri(Comment), At(10), "blocker");

        var item = Item(candidate: candidate, environments: environments, rework: rework, blocker: blocker);

        Assert.Same(candidate, item.ReleaseCandidate);
        Assert.Same(environments, item.Environments);
        Assert.Same(rework, item.Rework);
        Assert.Same(blocker, item.Blocker);

        Assert.Equal("2.4.86+bbccdd", item.ReleaseCandidate!.Version);
        Assert.Equal(At(9, 23), item.ReleaseCandidate.CreatedAt);

        Assert.Equal("2.4.86", item.Environments.Tdd!.Version);
        Assert.Equal(At(9, 23, 4), item.Environments.Tdd.DeployedAt);
        Assert.Null(item.Environments.Uat);
        Assert.Equal("2.4.84", item.Environments.Prod!.Version);

        Assert.Equal(1, item.Rework!.FtFailures);
        Assert.Equal(2, item.Rework.Reverts);
        Assert.Equal(0.125, item.Rework.Rate);

        Assert.Equal("Functional tests failed", item.Blocker!.Text);
        Assert.Equal(Comment, item.Blocker.Url!.AbsoluteUri);
        Assert.Equal(At(10), item.Blocker.At);
        Assert.Equal("blocker", item.Blocker.Kind);

        // Each of the four is said once by the part the item holds: another instance of the same letters is the same
        // part, and a part turned by one of its own members is another item.
        Assert.Equal(item, Item(candidate: new ReleaseCandidate("2.4.86+bbccdd", At(9, 23)), environments: environments, rework: rework, blocker: blocker));
        Assert.NotEqual(item, item with { ReleaseCandidate = null });
        Assert.NotEqual(item, item with { Environments = WorkItemEnvironments.None });
        Assert.NotEqual(item, item with { Rework = new WorkRework(1, 2, 0.125) with { Reverts = 0 } });
        Assert.NotEqual(item, item with { Blocker = new WorkNote("Functional tests failed", new Uri(Comment), At(10), "status") });
    }

    /// <summary>A record of nothing at all is a record, and another item's part than the part that is not there: the
    /// item with no rework and the item with a rework whose three counts are all unsaid are two items - which is what
    /// lets the page leave the tile out rather than tell a reader that nothing failed and nothing was reverted. The
    /// same holds of the note, the release candidate and the three places of a deployment.</summary>
    [Fact]
    public void ARecordOfNothingAtAllIsAnotherItemThanAPartNobodySaid()
    {
        var unsaid = Item(rework: null, blocker: null, candidate: null, environments: WorkItemEnvironments.None);
        var saidEmpty = Item(rework: new WorkRework(null, null, null), blocker: new WorkNote(null, null, null), candidate: new ReleaseCandidate(null, null), environments: new WorkItemEnvironments(null, null, null));

        Assert.Null(unsaid.Rework);
        Assert.Null(unsaid.Blocker);
        Assert.Null(unsaid.ReleaseCandidate);
        Assert.Null(unsaid.Environments.Tdd);
        Assert.NotNull(saidEmpty.Rework);
        Assert.Null(saidEmpty.Rework!.FtFailures);
        Assert.Null(saidEmpty.Blocker!.Kind);
        Assert.Null(saidEmpty.ReleaseCandidate!.Version);
        Assert.Equal(WorkItemEnvironments.None, saidEmpty.Environments);

        Assert.NotEqual(unsaid, saidEmpty);
        Assert.Equal(2, new HashSet<WorkItem> { unsaid, saidEmpty }.Count);
        Assert.NotEqual(Words(unsaid), Words(saidEmpty));
        Assert.NotEqual(unsaid, unsaid with { Environments = null! });
        Assert.Equal(new WorkItemEnvironments(null, null, null), WorkItemEnvironments.None);
    }

    // ------------------------------------------------------------------ the worker, said as it is given

    /// <summary>Whose work an item is stands as it was said, whichever of the three it is, and the record does not
    /// guess it out of the agent, the assignee or the file's word for it: a bot's item with no agent and a person's
    /// item with one are two items and neither is worked out of the other. A worker that is none of the three is held
    /// as the number it stands on and said as that number, which is what the file's reading never arrives at - it
    /// falls to <see cref="WorkWorker.Unknown" /> for any word it does not know, pinned in
    /// <see cref="WorkItemsTests" />.</summary>
    [Theory]
    [InlineData(WorkWorker.Unknown)]
    [InlineData(WorkWorker.Bot)]
    [InlineData(WorkWorker.Team)]
    [InlineData((WorkWorker)7)]
    public void TheWorkerIsSaidAsItIsGiven(WorkWorker worker)
    {
        var item = Item(worker: worker);

        Assert.Equal(worker, item.Worker);
        Assert.Equal(item, Item(worker: worker));
        Assert.Equal(item.GetHashCode(), Item(worker: worker).GetHashCode());
        Assert.Single(new HashSet<WorkItem> { item, Item(worker: worker) });
        Assert.Contains($"Worker = {worker}", Words(item), StringComparison.Ordinal);

        foreach (var other in new[] { WorkWorker.Unknown, WorkWorker.Bot, WorkWorker.Team, (WorkWorker)7, (WorkWorker)(-1) })
        {
            if (other == worker)
            {
                continue;
            }

            Assert.NotEqual(item, Item(worker: other));
            Assert.True(item != Item(worker: other));
        }

        // The worker is no one else's part: an item named for a bot and one named for a person, alike but for whose
        // work it is, are two items, and neither is counted for the other by the record.
        Assert.NotEqual(Item(worker: WorkWorker.Bot, agent: null, assignee: null), Item(worker: WorkWorker.Team, agent: null, assignee: null));
        Assert.Null(Item(worker: WorkWorker.Bot, agent: null, assignee: null).Agent);
    }

    // --------------------------------- the sixteen parts coming back out, and the copies between them

    /// <summary>The sixteen parts of an item come back out in the order they went in - the number, the title, the
    /// address, the state, the column, the history, the assignee, the agent, the sessions, the pulls, the release, the
    /// places, the cost, the worker, the rework and the note - each of them the very instance the item was built of,
    /// and what came out goes back in the same way: the item the sixteen parts fall apart into is the item they were,
    /// one item and one place in a set. An item built on nothing falls apart to the nothing it was built on, the worker
    /// the only part that cannot be unsaid.</summary>
    [Fact]
    public void TheItemFallsApartInTheOrderItWasMade()
    {
        var one = Full();

        var (number, title, url, state, column, history, assignee, agent, sessions, pulls, candidate, environments, cost, worker, rework, blocker) = one;

        Assert.Equal(490, number);
        Assert.Equal("Pin the board", title);
        Assert.Equal(Issue, url!.AbsoluteUri);
        Assert.Equal("Active", state);
        Assert.Equal("Review", column);
        Assert.Equal(WorkColumns.Todo, Assert.Single(history).Column);
        Assert.Equal("cm-ai-ops", assignee);
        Assert.Equal("dashboard-bot", agent);
        Assert.Equal(Session, Assert.Single(sessions).AbsoluteUri);
        Assert.Equal(912, Assert.Single(pulls).Number);
        Assert.Equal("2.4.86", candidate!.Version);
        Assert.Equal("2.4.86", environments.Tdd!.Version);
        Assert.Equal(4.5, cost);
        Assert.Equal(WorkWorker.Bot, worker);
        Assert.Equal(1, rework!.FtFailures);
        Assert.Equal("blocker", blocker!.Kind);

        Assert.Same(one.ColumnHistory, history);
        Assert.Same(one.CursorSessions, sessions);
        Assert.Same(one.PullRequests, pulls);
        Assert.Same(one.ReleaseCandidate, candidate);
        Assert.Same(one.Environments, environments);
        Assert.Same(one.Rework, rework);
        Assert.Same(one.Blocker, blocker);

        var again = new WorkItem(number, title, url, state, column, history, assignee, agent, sessions, pulls, candidate, environments, cost, worker, rework, blocker);

        Assert.Equal(one, again);
        Assert.Single(new HashSet<WorkItem> { one, again });

        var (noNumber, noTitle, noUrl, noState, noColumn, noHistory, noAssignee, noAgent, noSessions, noPulls, noCandidate, noEnvironments, noCost, noWorker, noRework, noBlocker) =
            new WorkItem(null, null, null, null, null, null!, null, null, null!, null!, null, null!, null);

        Assert.Null(noNumber);
        Assert.Null(noTitle);
        Assert.Null(noUrl);
        Assert.Null(noState);
        Assert.Null(noColumn);
        Assert.Null(noHistory);
        Assert.Null(noAssignee);
        Assert.Null(noAgent);
        Assert.Null(noSessions);
        Assert.Null(noPulls);
        Assert.Null(noCandidate);
        Assert.Null(noEnvironments);
        Assert.Null(noCost);
        Assert.Equal(WorkWorker.Unknown, noWorker);
        Assert.Null(noRework);
        Assert.Null(noBlocker);
    }

    /// <summary>Each part may be turned as the item is copied - the number to another number, the title to nothing, the
    /// address to another page, a list to a list of one, a record to a record of another word, the worker to the other
    /// two - and stands where it is turned to, while the fifteen parts the turn does not name stay the very instances
    /// the item was built of. A copy that names no part at all is the item it was copied from, in its parts, in its
    /// hash and in its words, and another reference; the original stands as it stood whichever way the copies went.</summary>
    [Fact]
    public void ACopyTurnsThePartItNamesAndKeepsTheOtherFifteen()
    {
        var one = Full();

        var unchanged = one with { };

        Assert.Equal(one, unchanged);
        Assert.NotSame(one, unchanged);
        Assert.Equal(one.GetHashCode(), unchanged.GetHashCode());
        Assert.Equal(Words(one), Words(unchanged));
        Assert.Same(one.Title, unchanged.Title);
        Assert.Same(one.Environments, unchanged.Environments);

        var numbered = one with { Number = 491 };

        Assert.Equal(491, numbered.Number);
        Assert.Equal(490, one.Number);
        Assert.Equal(4.5, numbered.CostUsd);
        Assert.Same(one.ColumnHistory, numbered.ColumnHistory);
        Assert.NotEqual(one, numbered);

        var titledOff = one with { Title = null };

        Assert.Null(titledOff.Title);
        Assert.Equal("Review", titledOff.Column);
        Assert.NotEqual(one, titledOff);

        var addressed = one with { Url = new Uri("https://github.com/clearmeasure-aisf-sample-apps/cmdemo1-system/issues/491") };

        Assert.NotSame(one.Url, addressed.Url);
        Assert.Equal(Issue, one.Url!.AbsoluteUri);
        Assert.NotEqual(one, addressed);

        var histories = one with { ColumnHistory = [new ColumnVisit(WorkColumns.Todo, At(9, 8)), new ColumnVisit(WorkColumns.Review, At(9, 12))] };

        Assert.Equal(2, histories.ColumnHistory.Count);
        Assert.Single(one.ColumnHistory);
        Assert.NotEqual(one, histories);
        Assert.Same(one.PullRequests, histories.PullRequests);

        var repacked = one with
        {
            Number = one.Number,
            Title = one.Title,
            Url = one.Url,
            State = one.State,
            Column = one.Column,
            ColumnHistory = one.ColumnHistory,
            Assignee = one.Assignee,
            Agent = one.Agent,
            CursorSessions = one.CursorSessions,
            PullRequests = one.PullRequests,
            ReleaseCandidate = one.ReleaseCandidate,
            Environments = one.Environments,
            CostUsd = one.CostUsd,
            Worker = one.Worker,
            Rework = one.Rework,
            Blocker = one.Blocker,
        };

        Assert.Equal(one, repacked);
        Assert.NotSame(one, repacked);
        Assert.Same(one.Blocker, repacked.Blocker);

        // The parts stand as they were whichever way the copies went.
        Assert.Equal(490, one.Number);
        Assert.Equal("Pin the board", one.Title);
        Assert.Equal(WorkWorker.Bot, one.Worker);
        Assert.Equal(4.5, one.CostUsd);
    }

    // -------------------------------------------------- the equality, the hash, and the two operators

    /// <summary>Two items are one while their sixteen parts are - each word as the letters it is, the address as the
    /// address it compares to, the records as the records they are, the numbers as the numbers they stand on, the
    /// worker as the word it was said as, and the three lists as the one list they stand on - whichever of the ways of
    /// asking is used, from either side. The hash follows the equality, boxed or unboxed and from one call to the
    /// next, so the items that are one are one item of a set: which is how an item the test writes is found again in
    /// the board the file made, and how the same item turned out of it is not.</summary>
    [Fact]
    public void TwoItemsAreOneWhileTheirSixteenPartsAre()
    {
        List<ColumnVisit> history = [new ColumnVisit(WorkColumns.Todo, At(9, 8))];
        List<Uri> sessions = [new Uri(Session)];
        List<WorkPullRequest> pulls = [new WorkPullRequest(912, null, "closed", null, null)];
        var environments = new WorkItemEnvironments(new WorkDeployment("2.4.86", At(9, 23, 4)), null, null);

        var one = Item(history: history, sessions: sessions, pulls: pulls, environments: environments);
        var two = Item(history: history, sessions: sessions, pulls: pulls, environments: environments);

        Assert.True(one.Equals(two));
        Assert.True(one.Equals(one));
        Assert.True(one.Equals((object)two));
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.Equal(one, two);
        Assert.NotSame(one, two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Single(new HashSet<WorkItem> { one, two });
        Assert.Contains(two, new HashSet<WorkItem> { one });
        Assert.Contains(Item(history: history, sessions: sessions, pulls: pulls, environments: environments), new HashSet<WorkItem> { one });

        // The words are compared as the letters they are and not as the instances they stand on, and the records an
        // item carries are compared as the records they are, whichever instance each of them stands in.
        Assert.NotSame(Own("Pin the board"), one.Title);
        Assert.Equal(Item(title: Own("Pin the board"), history: history, sessions: sessions, pulls: pulls, environments: environments), one);

        var records = Item(candidate: new ReleaseCandidate("2.4.9", null), rework: new WorkRework(1, 2, 0.5), blocker: new WorkNote("n", null, null, "status"));
        var alsoRecords = Item(candidate: new ReleaseCandidate("2.4.9", null), rework: new WorkRework(1, 2, 0.5), blocker: new WorkNote("n", null, null, "status"));

        Assert.NotSame(records.ReleaseCandidate, alsoRecords.ReleaseCandidate);
        Assert.NotSame(records.Rework, alsoRecords.Rework);
        Assert.NotSame(records.Blocker, alsoRecords.Blocker);
        Assert.Equal(records, alsoRecords);
        Assert.Single(new HashSet<WorkItem> { records, alsoRecords });
    }

    /// <summary>A difference in one part is another item, and every one of the sixteen is such a part: the number
    /// turned by one or turned to the nothing it is not, the title emptied, the address moved to another issue, the
    /// state and the column and the assignee and the agent turned in their letters, a second visit added to the
    /// history, the pull requests emptied, the sessions written out again as the one session they hold, the release
    /// turned to the next build, the
    /// deployment turned to another environment, the cost turned by a cent, the worker turned to the other two, the
    /// rework arrived at where it was not, and the note arrived at where it was not. A set of the items that differ in
    /// one part each holds them all apart.</summary>
    [Fact]
    public void AnItemThatDiffersInOnePartIsAnotherItem()
    {
        var one = Full();

        Assert.NotEqual(one, one with { Number = null });
        Assert.NotEqual(one, one with { Number = 491 });
        Assert.NotEqual(one, one with { Title = null });
        Assert.NotEqual(one, one with { Title = string.Empty });
        Assert.NotEqual(one, one with { Url = null });
        Assert.NotEqual(one, one with { State = null });
        Assert.NotEqual(one, one with { Column = WorkColumns.Todo });
        Assert.NotEqual(one, one with { Assignee = null });
        Assert.NotEqual(one, one with { Agent = null });
        Assert.NotEqual(one, one with { ReleaseCandidate = null });
        Assert.NotEqual(one, one with { Environments = WorkItemEnvironments.None });
        Assert.NotEqual(one, one with { CostUsd = null });
        Assert.NotEqual(one, one with { CostUsd = 4.51 });
        Assert.NotEqual(one, one with { Worker = WorkWorker.Team });
        Assert.NotEqual(one, one with { Rework = null });
        Assert.NotEqual(one, one with { Blocker = null });

        Assert.False(one.Equals(one with { ColumnHistory = [new ColumnVisit(WorkColumns.Todo, At(9, 8)), new ColumnVisit(WorkColumns.Review, At(9, 12))] }));
        Assert.False(one.Equals((object)(one with { PullRequests = [] })));
        Assert.True(one != one with { CursorSessions = [new Uri(Session)] });
        Assert.False(one == one with { CursorSessions = [new Uri(Session)] });

        Assert.Equal(
            7,
            new HashSet<WorkItem>
            {
                one,
                one with { Number = null },
                one with { Title = null },
                one with { Url = null },
                one with { CostUsd = null },
                one with { Worker = WorkWorker.Team },
                one with { Blocker = null },
            }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two items that are no item at all
    /// are one, an item and no item are two - the equality and the inequality each saying it, from either side - and the
    /// item asked about itself says no to the word of its title, to its own parts in the order they are said, to the
    /// number it is numbered by, to the address it stands on, to the board the item belongs to and to the anonymous
    /// thing that happens to carry all sixteen of its parts. That a file's reading turns an item of nothing into no item
    /// at all is <c>WorkItem.Read</c>'s, pinned in <see cref="WorkItemsTests" />, and not the record's.</summary>
    [Fact]
    public void NoItemIsOneWithNoItemAndNotWithAnyOtherThing()
    {
        WorkItem? nothing = null;
        WorkItem? alsoNothing = null;
        var there = Item();

        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == (WorkItem?)null);
        Assert.True(there != (WorkItem?)null);
        Assert.False(there.Equals(nothing));
        Assert.False(there.Equals((object?)null));
        Assert.False(there.Equals(null!));
        Assert.True(there.Equals(there));
        Assert.Null(default(WorkItem));

        Assert.NotNull(new WorkItem(null, null, null, null, null, null!, null, null, null!, null!, null, null!, null));
        Assert.False(there.Equals((WorkItem?)null));

        Assert.False(there.Equals("Pin the board"));
        Assert.False(there.Equals(490));
        Assert.False(there.Equals(new Uri(Issue)));
        Assert.False(there.Equals(WorkWorker.Bot));
        Assert.False(there.Equals(WorkItemEnvironments.None));
        Assert.False(there.Equals(new { Number = 490, Title = "Pin the board" }));
        Assert.Equal(2, new HashSet<WorkItem?> { there, nothing }.Count);
    }

    // ------------------------------------------------------------ the words the item says itself

    /// <summary>The words the item says its sixteen parts in: its own name at the front, the parts in the order the
    /// constructor names them, the words as the letters they are, the address as the address says itself, the numbers
    /// as plain digits with their sign below zero, the worker by its word, and a part nobody said as the empty place
    /// between the mark and the comma. A list is said as the list says itself - the type it stands on, and not the
    /// visits, the sessions or the pulls it carries - and the records an item holds are said whole, in their own
    /// names. These are the record's words and not the page's: no <c>#</c>, no <c>$</c>, no <c>·</c>, no dash and no
    /// link, which are what <see cref="WorkItemsText" /> makes of the same sixteen parts.</summary>
    [Fact]
    public void TheItemSaysItsSixteenPartsInTheOrderTheyAreNamed()
    {
        var bare = new WorkItem(12, "Keep it", null, null, "Todo", [], null, null, [], [], null, WorkItemEnvironments.None, null);

        Assert.Equal(
            "WorkItem { Number = 12, Title = Keep it, Url = , State = , Column = Todo, ColumnHistory = Dashboard.Work.ColumnVisit[], Assignee = , Agent = , CursorSessions = System.Uri[], PullRequests = Dashboard.Work.WorkPullRequest[], ReleaseCandidate = , Environments = WorkItemEnvironments { Tdd = , Uat = , Prod =  }, CostUsd = , Worker = Unknown, Rework = , Blocker =  }",
            Words(bare));

        var said = new WorkItem(
            490,
            "Pin the board",
            new Uri(Issue),
            "Active",
            "Review",
            [],
            "cm-ai-ops",
            "dashboard-bot",
            [],
            [],
            new ReleaseCandidate("2.4.86", At(9, 23)),
            new WorkItemEnvironments(new WorkDeployment("2.4.86", At(9, 23, 4)), null, null),
            4.5,
            WorkWorker.Bot,
            new WorkRework(1, 0, 0.125),
            new WorkNote("Functional tests failed", null, At(10), "blocker"));

        var words = Words(said);

        Assert.StartsWith("WorkItem { Number = 490, Title = Pin the board, Url = " + Issue + ", State = Active, Column = Review, ", words, StringComparison.Ordinal);
        Assert.Contains("ReleaseCandidate = ReleaseCandidate { Version = 2.4.86, CreatedAt = 11/09/2026 23:00:00 +00:00 }, ", words, StringComparison.Ordinal);
        Assert.Contains("Environments = WorkItemEnvironments { Tdd = WorkDeployment { Version = 2.4.86, DeployedAt = 11/09/2026 23:04:00 +00:00 }, Uat = , Prod =  }, ", words, StringComparison.Ordinal);
        Assert.Contains("CostUsd = 4.5, Worker = Bot, ", words, StringComparison.Ordinal);
        Assert.Contains("Rework = WorkRework { FtFailures = 1, Reverts = 0, Rate = 0.125 }, ", words, StringComparison.Ordinal);
        Assert.EndsWith("Blocker = WorkNote { Text = Functional tests failed, Url = , At = 11/10/2026 00:00:00 +00:00, Kind = blocker } }", words, StringComparison.Ordinal);

        // The sixteen parts are said in the order the constructor takes them, and one time each.
        string[] names =
        [
            "Number = ", "Title = ", "Url = ", "State = ", "Column = ", "ColumnHistory = ", "Assignee = ", "Agent = ",
            "CursorSessions = ", "PullRequests = ", "ReleaseCandidate = ", "Environments = ", "CostUsd = ", "Worker = ",
            "Rework = ", "Blocker = ",
        ];

        var at = -1;

        for (var index = 0; index < names.Length; index++)
        {
            var needle = index == 0 ? "{ " + names[index] : ", " + names[index];
            var found = words.IndexOf(needle, StringComparison.Ordinal);

            Assert.True(found > at, $"the item does not say {names[index]} after the part before it: {words}");
            at = found;
        }

        Assert.Equal(16, names.Count(name => words.Contains(name, StringComparison.Ordinal)));

        // No word of the page is among the record's own.
        foreach (var word in new[] { "#490", "$4.50", " · ", " so far", "—", "<a ", "opens in a new tab" })
        {
            Assert.DoesNotContain(word, words, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("WorkItem { WorkItem", Words(Item()), StringComparison.Ordinal);
    }

    /// <summary>The numbers and the moments in the item's own words follow whoever reads them, which is what sets them
    /// apart from the page's: a German reader of the item is told <c>CostUsd = 4,5</c> and the note's moment written
    /// day-first, while the sixteen part names, the words, the address, the version, the worker's word and the two
    /// marks of a record of nothing are the same letters to whichever reader - and two items the two readers see
    /// differently worded are one item still, for the words are no part of the equality.</summary>
    [Fact]
    public void TheWordsOfAnItemFollowTheReaderWhileThePartsThemselvesDoNot()
    {
        var item = new WorkItem(
            -490,
            "Pin the board",
            new Uri(Issue),
            null,
            null,
            [],
            null,
            null,
            [],
            [],
            null,
            WorkItemEnvironments.None,
            4.5,
            WorkWorker.Team,
            new WorkRework(1, 0, 0.125),
            new WorkNote("Blocked", null, At(9, 22, 30), null));

        Assert.Contains("Number = -490,", Words(item), StringComparison.Ordinal);
        Assert.Contains("CostUsd = 4.5,", Words(item), StringComparison.Ordinal);
        Assert.Contains("At = 11/09/2026 22:30:00 +00:00", Words(item), StringComparison.Ordinal);
        Assert.Contains("Rate = 0.125", Words(item), StringComparison.Ordinal);

        ViewerCulture.German(() =>
        {
            Assert.Contains("CostUsd = 4,5,", item.ToString()!, StringComparison.Ordinal);
            Assert.Contains("At = 09.11.2026 22:30:00 +00:00", item.ToString()!, StringComparison.Ordinal);
            Assert.Contains("Rate = 0,125", item.ToString()!, StringComparison.Ordinal);
            Assert.Contains("Number = -490,", item.ToString()!, StringComparison.Ordinal);
            Assert.Contains($"Url = {Issue},", item.ToString()!, StringComparison.Ordinal);
            Assert.Contains("Worker = Team,", item.ToString()!, StringComparison.Ordinal);
        });

        // One reader's item and another's are one item: the words say the parts, they are not parts.
        Assert.Equal(item, Item(-490, "Pin the board", new Uri(Issue), null, null, [], null, null, [], [], null, WorkItemEnvironments.None, 4.5, WorkWorker.Team, new WorkRework(1, 0, 0.125), new WorkNote("Blocked", null, At(9, 22, 30), null)));
    }

    /// <summary>The words at the ends the two numbers run to: the number of the board below zero and at the ends of an
    /// <see cref="int" />, and the cost at the ends of a real number - the record saying each as the plain number it
    /// stands on, with no unit, no currency and no dash, and the number that is no number by its own word.</summary>
    [Theory]
    [InlineData(0.0, "CostUsd = 0,")]
    [InlineData(0.001, "CostUsd = 0.001,")]
    [InlineData(1e-07, "CostUsd = 1E-07,")]
    [InlineData(2147483648, "CostUsd = 2147483648,")]
    [InlineData(-4, "CostUsd = -4,")]
    [InlineData(double.NaN, "CostUsd = NaN,")]
    [InlineData(double.PositiveInfinity, "CostUsd = Infinity,")]
    [InlineData(1.7976931348623157e+308, "CostUsd = 1.7976931348623157E+308,")]
    public void TheWordsSayTheCostAtWhateverEndItStandsOn(double cost, string said) =>
        Assert.Contains(said, Words(Item(cost: cost)), StringComparison.Ordinal);

    /// <summary>The words of the two ends of an <see cref="int" /> and of the worker that is none of the three: the
    /// number said as the plain digits it stands on, with its sign and with no group of three, and a worker no file
    /// ever writes said as the number it is rather than as a word the record has none of.</summary>
    [Fact]
    public void TheWordsSayTheNumberAtEitherEndAndAWorkerThatIsNoneOfTheThree()
    {
        Assert.Contains("Number = -2147483648,", Words(Item(number: int.MinValue)), StringComparison.Ordinal);
        Assert.Contains("Number = 2147483647,", Words(Item(number: int.MaxValue)), StringComparison.Ordinal);
        Assert.Contains("Number = 1234567,", Words(Item(number: 1234567)), StringComparison.Ordinal);
        Assert.DoesNotContain(",", Words(Item(number: 1234567)).Split("Number = ")[1].Split(',')[0], StringComparison.Ordinal);
        Assert.Contains("Worker = 7,", Words(Item(worker: (WorkWorker)7)), StringComparison.Ordinal);
        Assert.Contains("Worker = -1,", Words(Item(worker: (WorkWorker)(-1))), StringComparison.Ordinal);
        Assert.StartsWith("WorkItem { Number = ,", Words(Item(number: null)), StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------- the record's own declared shape

    /// <summary>The item's whole public surface, as the running assembly holds it: its sixteen parts named in the
    /// order the constructor names them, and the members every record is asked - comparing, hashing, saying itself and
    /// falling apart, the two operators kept although the compiler names them specially, since they are the <c>==</c>
    /// and the <c>!=</c> a caller writes - and nothing else to call. One public way in: the sixteen parts, of which the
    /// last three may be left out, and the copy the <c>with</c> walks is the compiler's private one. A seventeenth part
    /// added to the item has to be written here as well as in every list above, which is the point: the board cannot be
    /// shown a part nobody decided what it means.</summary>
    [Fact]
    public void TheItemHasSixteenPartsAndTheMembersEveryRecordIsAsked()
    {
        var type = typeof(WorkItem);

        Assert.True(type.IsSealed);
        Assert.False(type.IsValueType);
        Assert.Equal([typeof(IEquatable<WorkItem>)], type.GetInterfaces());
        Assert.Equal(typeof(object), type.BaseType);
        Assert.Equal(nameof(WorkItem), type.Name);
        Assert.Equal("Dashboard.Work", type.Namespace);

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.Equal(
            [
                "Number", "Title", "Url", "State", "Column", "ColumnHistory", "Assignee", "Agent", "CursorSessions",
                "PullRequests", "ReleaseCandidate", "Environments", "CostUsd", "Worker", "Rework", "Blocker",
            ],
            properties.Select(property => property.Name));

        Assert.Equal(
            [
                typeof(int?), typeof(string), typeof(Uri), typeof(string), typeof(string),
                typeof(IReadOnlyList<ColumnVisit>), typeof(string), typeof(string), typeof(IReadOnlyList<Uri>),
                typeof(IReadOnlyList<WorkPullRequest>), typeof(ReleaseCandidate), typeof(WorkItemEnvironments),
                typeof(double?), typeof(WorkWorker), typeof(WorkRework), typeof(WorkNote),
            ],
            properties.Select(property => property.PropertyType));

        Assert.All(
            properties,
            property =>
            {
                Assert.True(property is { CanRead: true, CanWrite: true }, $"{property.Name} is not a part a caller can read and turn");
                Assert.NotNull(property.SetMethod);
                Assert.True(property.GetMethod!.IsPublic);

                // The parts are init-only: they are said once, at the making or at the copy, and never turned after.
                Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
            });

        // The contract every record compares by is the compiler's private one: no public part of the item.
        Assert.Null(type.GetProperty("EqualityContract", BindingFlags.Public | BindingFlags.Instance));
        Assert.NotNull(type.GetProperty("EqualityContract", BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.DoesNotContain("EqualityContract", PublicSurface(), StringComparer.Ordinal);

        Assert.Equal(
            [
                "Agent", "Assignee", "Blocker", "Column", "ColumnHistory", "CostUsd", "CursorSessions", "Deconstruct",
                "Environments", "Equals", "GetHashCode", "Number", "PullRequests", "ReleaseCandidate", "Rework",
                "State", "Title", "ToString", "Url", "Worker", "op_Equality", "op_Inequality",
            ],
            PublicSurface());

        var arguments = type.GetConstructors().Single().GetParameters();

        Assert.Equal(16, arguments.Length);
        Assert.Equal(
            [
                "Number", "Title", "Url", "State", "Column", "ColumnHistory", "Assignee", "Agent", "CursorSessions",
                "PullRequests", "ReleaseCandidate", "Environments", "CostUsd", "Worker", "Rework", "Blocker",
            ],
            arguments.Select(argument => argument.Name));
        Assert.All(arguments.Take(13), argument => Assert.False(argument.HasDefaultValue, $"{argument.Name} may be left out"));
        Assert.True(arguments[13].HasDefaultValue);
        Assert.Equal<object>(WorkWorker.Unknown, arguments[13].DefaultValue!);
        Assert.True(arguments[14].HasDefaultValue);
        Assert.Null(arguments[14].DefaultValue);
        Assert.True(arguments[15].HasDefaultValue);
        Assert.Null(arguments[15].DefaultValue);

        Assert.Single(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        var copy = Assert.Single(type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.Equal(type, copy.GetParameters().Single().ParameterType);

        // The two overloads of the equality every record is asked with, and the one Deconstruct of sixteen parts.
        Assert.Equal(2, type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Count(method => method.Name == "Equals"));
        var parts = type.GetMethod("Deconstruct")!;
        Assert.Equal(
            [
                "Number", "Title", "Url", "State", "Column", "ColumnHistory", "Assignee", "Agent", "CursorSessions",
                "PullRequests", "ReleaseCandidate", "Environments", "CostUsd", "Worker", "Rework", "Blocker",
            ],
            parts.GetParameters().Select(parameter => parameter.Name));
        Assert.All(parts.GetParameters(), parameter => Assert.True(parameter.IsOut));

        // Nothing to call but the parts: the item is sixteen held values and no asking, and no part of it stands on a
        // moment the record would have to be asked the time of.
        Assert.DoesNotContain(
            type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            method => !method.IsSpecialName
                      && !method.Name.Contains('<', StringComparison.Ordinal)
                      && method.Name != "Deconstruct"
                      && method.Name != "Equals"
                      && method.Name != "GetHashCode"
                      && method.Name != "ToString");
        Assert.Empty(type.GetEvents());
        Assert.Empty(type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.All(
            properties,
            property => Assert.False(
                property.PropertyType == typeof(DateTimeOffset) || property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(TimeSpan),
                $"{property.Name} would stand on a moment the record would have to be asked the time of"));
    }

    // -------------------------------------------- the file's reading of the item's parts

    /// <summary>The report of a file whose only said parts are its items, in the order they are said: a
    /// <c>workitems.json</c> body nothing else is said of, so that each test turns the one item it names alone, the
    /// way <see cref="WorkItemsTests.EveryPartOfAnItemIsOptionalAndABadPartIsLeftOut" /> pins the reading a bad part is
    /// left out of.</summary>
    private static WorkItemsReport? ItemsOf(params string[] itemBodies) =>
        WorkItemsReport.Parse("{ \"items\": [ " + string.Join(", ", itemBodies) + " ] }");

    /// <summary>The item a file says of only by its number is the item of that number: the number the file says, and
    /// every one of the other fifteen parts as the nothing the record stands on when the file does not say it - the
    /// title, the address, the state and the column as the no word they are, the history and the sessions and the pull
    /// requests as the no list they are, the candidate and the cost and the rework and the note as the no record they
    /// are, the environments as the none they are, and the worker as the unknown it is not named.</summary>
    [Fact]
    public void AnItemThatSaysOnlyItsNumberIsTheItemOfThatNumberAndOfNothingElse()
    {
        var item = ItemsOf("""{ "number": 490 }""")!.Items.Single();

        Assert.Equal(490, item.Number);
        Assert.Null(item.Title);
        Assert.Null(item.Url);
        Assert.Null(item.State);
        Assert.Null(item.Column);
        Assert.Empty(item.ColumnHistory);
        Assert.Null(item.Assignee);
        Assert.Null(item.Agent);
        Assert.Empty(item.CursorSessions);
        Assert.Empty(item.PullRequests);
        Assert.Null(item.ReleaseCandidate);
        Assert.Equal(WorkItemEnvironments.None, item.Environments);
        Assert.Null(item.CostUsd);
        Assert.Equal(WorkWorker.Unknown, item.Worker);
        Assert.Null(item.Rework);
        Assert.Null(item.Blocker);
    }

    /// <summary>The item a file says of only by its title is the item of that title: the word the file says, trimmed
    /// the way the reading says a title, and every one of the other fifteen parts as the nothing the record stands on
    /// when the file does not say it.</summary>
    [Fact]
    public void AnItemThatSaysOnlyItsTitleIsTheItemOfThatTitleAndOfNothingElse()
    {
        var item = ItemsOf("""{ "title": "  Keep it  " }""")!.Items.Single();

        Assert.Equal("Keep it", item.Title);
        Assert.Null(item.Number);
        Assert.Null(item.Url);
        Assert.Null(item.State);
        Assert.Null(item.Column);
        Assert.Empty(item.ColumnHistory);
        Assert.Null(item.Assignee);
        Assert.Null(item.Agent);
        Assert.Empty(item.CursorSessions);
        Assert.Empty(item.PullRequests);
        Assert.Null(item.ReleaseCandidate);
        Assert.Equal(WorkItemEnvironments.None, item.Environments);
        Assert.Null(item.CostUsd);
        Assert.Equal(WorkWorker.Unknown, item.Worker);
        Assert.Null(item.Rework);
        Assert.Null(item.Blocker);
    }

    /// <summary>The item a file says of only by its column is the item of that column: the word the file says, and
    /// every one of the other fifteen parts as the nothing the record stands on when the file does not say it.</summary>
    [Fact]
    public void AnItemThatSaysOnlyItsColumnIsTheItemOfThatColumnAndOfNothingElse()
    {
        var item = ItemsOf("""{ "column": "Todo" }""")!.Items.Single();

        Assert.Equal("Todo", item.Column);
        Assert.Null(item.Number);
        Assert.Null(item.Title);
        Assert.Null(item.Url);
        Assert.Null(item.State);
        Assert.Empty(item.ColumnHistory);
        Assert.Null(item.Assignee);
        Assert.Null(item.Agent);
        Assert.Empty(item.CursorSessions);
        Assert.Empty(item.PullRequests);
        Assert.Null(item.ReleaseCandidate);
        Assert.Equal(WorkItemEnvironments.None, item.Environments);
        Assert.Null(item.CostUsd);
        Assert.Equal(WorkWorker.Unknown, item.Worker);
        Assert.Null(item.Rework);
        Assert.Null(item.Blocker);
    }

    /// <summary>A number the file says that is no number - a word, a negative or a reach an int does not run to - is
    /// no number: the item keeps the other parts it says and says no number of its own, the way the reading leaves a
    /// bad part out.</summary>
    [Theory]
    [InlineData("""{ "number": "490", "title": "Keep it" }""")]
    [InlineData("""{ "number": -4, "title": "Keep it" }""")]
    [InlineData("""{ "number": 1e300, "title": "Keep it" }""")]
    public void ANumberThatIsNoNumberIsNoNumber(string item)
    {
        var parsed = ItemsOf(item)!.Items.Single();

        Assert.Null(parsed.Number);
        Assert.Equal("Keep it", parsed.Title);
    }

    /// <summary>A number the file says as a float is the integer it rounds to - the even one when it lands midway -
    /// and the item keeps it with the other parts the file says; a float no int runs to is no number at all.</summary>
    [Theory]
    [InlineData("4.5", 4)]
    [InlineData("4.6", 5)]
    [InlineData("0.4", 0)]
    public void AFloatNumberIsTheIntegerItRoundsTo(string number, int expected)
    {
        var parsed = ItemsOf($$"""{ "number": {{number}}, "title": "Keep it" }""")!.Items.Single();

        Assert.Equal(expected, parsed.Number);
        Assert.Equal("Keep it", parsed.Title);
    }

    /// <summary>An item whose only part is a number that is no number is no item at all: the file's reading is the
    /// report of no items, the number being no number and nothing else being said, the item having no number, no
    /// title, no address and no column.</summary>
    [Theory]
    [InlineData("""{ "number": "490" }""")]
    [InlineData("""{ "number": -4 }""")]
    [InlineData("""{ "number": 1e300 }""")]
    public void AnItemWhoseOnlyPartIsANumberThatIsNoNumberIsNoItemAtAll(string item) =>
        Assert.Empty(ItemsOf(item)!.Items);

    /// <summary>A title the file says of another type is no title: the item keeps the parts it says of the right type
    /// and says no title of its own.</summary>
    [Theory]
    [InlineData("""{ "title": 490, "column": "Todo" }""")]
    [InlineData("""{ "title": [ "Keep it" ], "column": "Todo" }""")]
    [InlineData("""{ "title": true, "column": "Todo" }""")]
    [InlineData("""{ "title": null, "column": "Todo" }""")]
    public void ATitleOfAnotherTypeIsNoTitle(string item)
    {
        var parsed = ItemsOf(item)!.Items.Single();

        Assert.Null(parsed.Title);
        Assert.Equal("Todo", parsed.Column);
    }

    /// <summary>An item whose only part is a title of another type is no item at all: the file's reading is the report
    /// of no items, the title being no title and nothing else being said.</summary>
    [Fact]
    public void AnItemWhoseOnlyPartIsATitleOfAnotherTypeIsNoItemAtAll() =>
        Assert.Empty(ItemsOf("""{ "title": 490 }""")!.Items);

    /// <summary>A column the file says of another type is no column: the item keeps the parts it says of the right
    /// type and says no column of its own.</summary>
    [Theory]
    [InlineData("""{ "column": 7, "title": "Keep it" }""")]
    [InlineData("""{ "column": [ "Todo" ], "title": "Keep it" }""")]
    [InlineData("""{ "column": null, "title": "Keep it" }""")]
    public void AColumnOfAnotherTypeIsNoColumn(string item)
    {
        var parsed = ItemsOf(item)!.Items.Single();

        Assert.Null(parsed.Column);
        Assert.Equal("Keep it", parsed.Title);
    }

    /// <summary>An address the file says that is no absolute http or https address is no address: the item keeps the
    /// parts it says of the right type and says no address of its own - the scripted, the relative, the other-scheme
    /// and the other-type.</summary>
    [Theory]
    [InlineData("""{ "url": "javascript:alert(1)", "number": 490 }""")]
    [InlineData("""{ "url": "/issues/490", "number": 490 }""")]
    [InlineData("""{ "url": "ftp://host/issues/490", "number": 490 }""")]
    [InlineData("""{ "url": 490, "number": 490 }""")]
    [InlineData("""{ "url": "   ", "number": 490 }""")]
    public void AnAddressThatIsNoAbsoluteHttpOrHttpsAddressIsNoAddress(string item)
    {
        var parsed = ItemsOf(item)!.Items.Single();

        Assert.Null(parsed.Url);
        Assert.Equal(490, parsed.Number);
    }

    /// <summary>An address the file says that is an absolute http or https address is the address: the item keeps it
    /// with the other parts it says, and an item that says an address and no other part is the item of that address.</summary>
    [Fact]
    public void AnItemThatSaysAnAddressIsTheItemOfThatAddress()
    {
        var both = ItemsOf($$"""{ "number": 490, "url": "{{Issue}}" }""")!.Items.Single();
        var only = ItemsOf($$"""{ "url": "{{Issue}}" }""")!.Items.Single();

        Assert.Equal(new Uri(Issue), both.Url);
        Assert.Equal(490, both.Number);
        Assert.Equal(new Uri(Issue), only.Url);
        Assert.Null(only.Number);
    }

    /// <summary>A session the file says that is no session - not a word, or a word that is no absolute http or https
    /// address - is no session: the item says no sessions of its own, whatever else it says. A list that is no list
    /// and nothing at all are the same: no sessions.</summary>
    [Theory]
    [InlineData("""[ "javascript:alert(1)", null, 7, true, " " ]""")]
    [InlineData("""[ ]""")]
    [InlineData("""{ "url": "https://cursor.com/agents/bc-1" }""")]
    [InlineData("""null""")]
    public void ASessionsThatAreNoSessionsAreNoSessions(string sessions)
    {
        var parsed = ItemsOf($$"""{ "number": 490, "cursorSessions": {{sessions}} }""")!.Items.Single();

        Assert.Empty(parsed.CursorSessions);
        Assert.Equal(490, parsed.Number);
    }

    /// <summary>A session the file says that is an absolute http or https address is the session: the item keeps it
    /// with the other parts it says, and keeps the ones it does not say of another type out.</summary>
    [Fact]
    public void ASessionsThatAreSessionsAreTheSessions()
    {
        var parsed = ItemsOf($$"""{ "number": 490, "cursorSessions": [ "javascript:alert(1)", "{{Session}}" ] }""")!.Items.Single();

        Assert.Equal([new Uri(Session)], parsed.CursorSessions);
        Assert.Equal(490, parsed.Number);
    }

    /// <summary>A pull request the file says that is no pull request - not an object, an object that says no number,
    /// no address, no state and no checks, or an object that says them of another type - is no pull request: the item
    /// says no pull requests of its own. A list that is no list and nothing at all are the same: no pull requests.</summary>
    [Theory]
    [InlineData("""[ 7, "pulls/1", null, true, { }, { "url": "not an address" }, { "state": 7 } ]""")]
    [InlineData("""{ "number": 912 }""")]
    [InlineData("""null""")]
    public void APullRequestsThatAreNoPullRequestsAreNoPullRequests(string pulls)
    {
        var parsed = ItemsOf($$"""{ "number": 490, "prs": {{pulls}} }""")!.Items.Single();

        Assert.Empty(parsed.PullRequests);
        Assert.Equal(490, parsed.Number);
    }

    /// <summary>A pull request the file says that is a pull request is the pull request: the number, the address, the
    /// state and the checks it says, and the ones it says of another type left out of it and of the item.</summary>
    [Fact]
    public void APullRequestsThatArePullRequestsAreThePullRequests()
    {
        var parsed = ItemsOf($$"""{ "number": 490, "prs": [ 7, "pulls/1", { "state": "open" }, { "number": 912, "url": "{{PullAddress}}", "state": "merged" } ] }""")!.Items.Single();

        Assert.Equal(
            [new WorkPullRequest(null, null, "open", null, null, null, null), new WorkPullRequest(912, new Uri(PullAddress), "merged", null, null, null, null)],
            parsed.PullRequests);
        Assert.Equal(490, parsed.Number);
    }

    /// <summary>A release candidate the file says that is no candidate - not an object, an object that says no version
    /// and no moment, or one that says them of another type - is no candidate: the item says no candidate of its own.
    /// Nothing at all is the same: no candidate.</summary>
    [Theory]
    [InlineData("""{ "number": 490, "releaseCandidate": "2.4.9+abc123" }""")]
    [InlineData("""{ "number": 490, "releaseCandidate": { } }""")]
    [InlineData("""{ "number": 490, "releaseCandidate": { "version": 490, "createdAt": "not a time" } }""")]
    [InlineData("""{ "number": 490, "releaseCandidate": null }""")]
    public void AReleaseCandidateThatIsNoCandidateIsNoCandidate(string item)
    {
        var parsed = ItemsOf(item)!.Items.Single();

        Assert.Null(parsed.ReleaseCandidate);
        Assert.Equal(490, parsed.Number);
    }

    /// <summary>A release candidate the file says that is a candidate is the candidate: the version with its build
    /// suffix the reading loses, and the moment it says, with the other parts the item says kept.</summary>
    [Fact]
    public void AReleaseCandidateThatIsACandidateIsTheCandidate()
    {
        var parsed = ItemsOf("""{ "number": 490, "releaseCandidate": { "version": "2.4.9+abc123", "createdAt": "2026-11-09T21:00:00Z" } }""")!.Items.Single();

        Assert.Equal(new ReleaseCandidate("2.4.9", At(9, 21)), parsed.ReleaseCandidate);
        Assert.Equal(490, parsed.Number);
    }

    /// <summary>An environments the file says that is no environments - a word, an object that says no deployment in
    /// any of its three places, or a place that says no deployment - is the none: the item says no environments of its
    /// own. Nothing at all is the same: the none.</summary>
    [Theory]
    [InlineData("""{ "number": 490, "environments": "prod" }""")]
    [InlineData("""{ "number": 490, "environments": { } }""")]
    [InlineData("""{ "number": 490, "environments": { "tdd": "2.4.86", "uat": null, "prod": [ ] } }""")]
    [InlineData("""{ "number": 490, "environments": null }""")]
    public void AnEnvironmentsThatIsNoEnvironmentsIsTheNone(string item)
    {
        var parsed = ItemsOf(item)!.Items.Single();

        Assert.Equal(WorkItemEnvironments.None, parsed.Environments);
        Assert.Equal(490, parsed.Number);
    }

    /// <summary>An environments the file says that is a deployment is the deployment: the place the file names in
    /// whatever case, the version and the moment it says - the version and the moment of another type lost, and the
    /// place not named as the none it is not said in.</summary>
    [Fact]
    public void AnEnvironmentsThatIsADeploymentIsTheDeployment()
    {
        var parsed = ItemsOf("""{ "number": 490, "environments": { "TDD": { "version": 4, "deployedAt": "not a time" }, "prod": { "version": "2.4.84", "deployedAt": "2026-11-08T06:00:00Z" } } }""")!.Items.Single();

        Assert.Equal(
            new WorkItemEnvironments(new WorkDeployment(null, null), null, new WorkDeployment("2.4.84", At(8, 6))),
            parsed.Environments);
        Assert.Equal(490, parsed.Number);
    }

    /// <summary>A cost the file says that is no cost - a word, a negative, a flag or nothing - is no cost: the item
    /// says no cost of its own.</summary>
    [Theory]
    [InlineData("""{ "number": 490, "costUsd": "4.50" }""")]
    [InlineData("""{ "number": 490, "costUsd": -4 }""")]
    [InlineData("""{ "number": 490, "costUsd": true }""")]
    [InlineData("""{ "number": 490, "costUsd": null }""")]
    public void ACostThatIsNoCostIsNoCost(string item)
    {
        var parsed = ItemsOf(item)!.Items.Single();

        Assert.Null(parsed.CostUsd);
        Assert.Equal(490, parsed.Number);
    }

    /// <summary>A cost the file says that is a cost is the cost: the number it says, with the other parts the item
    /// says kept.</summary>
    [Fact]
    public void ACostThatIsACostIsTheCost()
    {
        var parsed = ItemsOf("""{ "number": 490, "costUsd": 4.5 }""")!.Items.Single();

        Assert.Equal(4.5, parsed.CostUsd);
        Assert.Equal(490, parsed.Number);
    }

    /// <summary>A worker the file says that is no worker - not a word, or a word that is not a worker, or a word of
    /// nothing but whitespace - is the unknown: the item says no worker of its own, whatever else it says.</summary>
    [Theory]
    [InlineData("""{ "number": 490, "worker": 4 }""")]
    [InlineData("""{ "number": 490, "worker": "robot" }""")]
    [InlineData("""{ "number": 490, "worker": "   " }""")]
    [InlineData("""{ "number": 490, "worker": null }""")]
    public void AWorkerThatIsNoWorkerIsTheUnknown(string item)
    {
        var parsed = ItemsOf(item)!.Items.Single();

        Assert.Equal(WorkWorker.Unknown, parsed.Worker);
        Assert.Equal(490, parsed.Number);
    }

    /// <summary>A worker the file says that is a worker is the worker: the bot and the team, in whatever case the file
    /// says them, with the other parts the item says kept.</summary>
    [Fact]
    public void AWorkerThatIsAWorkerIsTheWorker()
    {
        var bot = ItemsOf("""{ "number": 490, "worker": "BOT" }""")!.Items.Single();
        var team = ItemsOf("""{ "number": 490, "worker": "team" }""")!.Items.Single();

        Assert.Equal(WorkWorker.Bot, bot.Worker);
        Assert.Equal(WorkWorker.Team, team.Worker);
    }

    /// <summary>A rework the file says that is no rework - a count of another type, a rework of another type, or a
    /// rework that says nothing - is no rework: the item says no rework of its own, the counts being no counts and the
    /// rework no rework.</summary>
    [Theory]
    [InlineData("""{ "number": 490, "ftFailures": "1", "reverts": -1 }""")]
    [InlineData("""{ "number": 490, "ftFailures": [ 1 ], "reverts": "2" }""")]
    [InlineData("""{ "number": 490, "rework": "high" }""")]
    [InlineData("""{ "number": 490, "rework": { } }""")]
    [InlineData("""{ "number": 490, "rework": { "rate": "high" } }""")]
    [InlineData("""{ "number": 490, "rework": null }""")]
    public void AReworkThatIsNoReworkIsNoRework(string item)
    {
        var parsed = ItemsOf(item)!.Items.Single();

        Assert.Null(parsed.Rework);
        Assert.Equal(490, parsed.Number);
    }

    /// <summary>A rework the file says that is a rework is the rework: the failures and the reverts the file says, the
    /// rate unsaid, and the older rework object the file says when those are unsaid, with its rate and its own
    /// counts.</summary>
    [Fact]
    public void AReworkThatIsAReworkIsTheRework()
    {
        var byCounts = ItemsOf("""{ "number": 490, "ftFailures": 1, "reverts": 2 }""")!.Items.Single();
        var byObject = ItemsOf("""{ "number": 490, "rework": { "rate": 0.25, "ftFailures": 0, "reverts": 1 } }""")!.Items.Single();

        Assert.Equal(new WorkRework(1, 2, null), byCounts.Rework);
        Assert.Equal(new WorkRework(0, 1, 0.25), byObject.Rework);
    }

    /// <summary>A note the file says that is no note - a latest status comment that is no comment, a blocker of
    /// another type, a comment of another type, and a status of another type - is no note: the item says no blocker of
    /// its own, whatever else it says.</summary>
    [Theory]
    [InlineData("""{ "number": 490, "latestStatusComment": "It failed" }""")]
    [InlineData("""{ "number": 490, "latestStatusComment": { "comment": 7 } }""")]
    [InlineData("""{ "number": 490, "blocker": 4 }""")]
    [InlineData("""{ "number": 490, "comment": "It failed" }""")]
    [InlineData("""{ "number": 490, "status": 490 }""")]
    public void ABlockerThatIsNoBlockerIsNoBlocker(string item)
    {
        var parsed = ItemsOf(item)!.Items.Single();

        Assert.Null(parsed.Blocker);
        Assert.Equal(490, parsed.Number);
    }

    /// <summary>A note the file says that is a note is the note: the word it says, the address and the moment it says
    /// - the kind the file says kept where the reading keeps it, and the moment of another type lost.</summary>
    [Fact]
    public void ABlockerThatIsABlockerIsTheBlocker()
    {
        var fromStatus = ItemsOf("""{ "number": 490, "status": "Blocked" }""")!.Items.Single();
        var fromBlocker = ItemsOf($$"""{ "number": 490, "blocker": { "text": "It failed", "url": "{{Comment}}", "at": "2026-11-10T00:00:00Z" } }""")!.Items.Single();
        var fromComment = ItemsOf($$"""{ "number": 490, "latestStatusComment": { "text": "It failed", "kind": "blocker", "at": "not a time" } }""")!.Items.Single();

        Assert.Equal(new WorkNote("Blocked", null, null, null), fromStatus.Blocker);
        Assert.Equal(new WorkNote("It failed", new Uri(Comment), At(10), null), fromBlocker.Blocker);
        Assert.Equal(new WorkNote("It failed", null, null, "blocker"), fromComment.Blocker);
    }

    /// <summary>An item the file says that is no object - a number, a word, a flag, a list or nothing - is no item:
    /// the file's reading is the report of no items, and the empty object is no item either, saying no number, no
    /// title, no address and no column.</summary>
    [Theory]
    [InlineData("7")]
    [InlineData("\"x\"")]
    [InlineData("null")]
    [InlineData("true")]
    [InlineData("[ ]")]
    [InlineData("{ }")]
    public void AnItemThatIsNotAnObjectIsNoItem(string item) =>
        Assert.Empty(ItemsOf(item)!.Items);

    // ------------------------------------------------------------------------------- the machinery

    /// <summary>The public members the item declares itself: its sixteen properties and the methods that are not the
    /// get and the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the test of the surface rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(WorkItem)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words an item says its sixteen parts in, taken in the invariant culture whatever the reader of this
    /// test happens to be: the record's own words follow whoever reads them, which is pinned on its own further up this
    /// file, so the strings here are the point and the culture only the way to say them once.</summary>
    private static string Words(WorkItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            return item.ToString()!;
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }
}
