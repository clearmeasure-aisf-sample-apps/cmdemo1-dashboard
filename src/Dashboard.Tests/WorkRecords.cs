using Dashboard.Work;

namespace Dashboard.Tests;

/// <summary>
/// One home for the work-item records the edge-case files stand on: the same fully-filled
/// <see cref="WorkItem" />, <see cref="WorkItemsScope" />, <see cref="WorkItemEnvironments" />,
/// <see cref="WorkDuration" /> and <see cref="WorkItemsReport" /> each of those files used to build for itself,
/// made here once with every part said with a word, a number, a moment or an address of its own, so that a part
/// landing on another part's place is seen rather than quietly matching a third of the same record.
/// <para>
/// This is the shared record-fixture helper the per-record edge-case files grow out of. The scaffolding a test
/// still keeps beside it - the culture-pinned <see cref="Words"/> of a record and the reflected
/// <see cref="RecordSurface.Of{T}"/> of its public surface - already stands once in <see cref="CultureScope"/> and
/// <see cref="RecordSurface"/>, so a file that reaches for them names the shared helper rather than copying it.
/// What was never shared - the <see cref="Own"/> string-instance and the fully-filled record itself - stands here,
/// the way <see cref="Targets"/>, <see cref="RuntimeChanges"/> and <see cref="ProbeResults"/> do for their records.
/// The records are held as the very instances they are built of and nothing is asked what time it is: every moment
/// is one the fixture brought, and the addresses are held and said as letters only.
/// </para>
/// </summary>
internal static class WorkRecords
{
    /// <summary>A word of the test's own: the same letters as <paramref name="word" />, but another instance, so
    /// that a part held as the very instance it was given is proved by the holding and not by the interning of a
    /// literal. The one string-instance helper the word-pinning edge-case files all used to write for themselves.</summary>
    public static string Own(string word)
    {
        ArgumentNullException.ThrowIfNull(word);
        return new(word.ToCharArray());
    }

    /// <summary>A moment of the fixture's own, so that no part of a record is ever asked what time it is.</summary>
    public static DateTimeOffset At(int day, int hour = 0, int minute = 0) => new(2026, 11, day, hour, minute, 0, TimeSpan.Zero);

    // ------------------------------- the sub-records a work item is made of

    /// <summary>A pull request opened for a work item, said with its own number, address, state, moment and checks.</summary>
    public static WorkPullRequest PullRequest(
        int? number = 912,
        string? url = "https://github.com/clearmeasure-aisf-sample-apps/cmdemo1-system/pull/912",
        string? state = "closed",
        DateTimeOffset? mergedAt = null,
        string? mergeSha = "abcdef1234567890",
        string? checks = "failed",
        Uri? checksUrl = null) =>
        new(number, string.IsNullOrEmpty(url) ? null : new Uri(url), state, mergedAt ?? At(9, 22), mergeSha, checks, checksUrl);

    /// <summary>The Octopus release cut for an item, <c>2.4.&lt;build&gt;</c>, and when it was created.</summary>
    public static ReleaseCandidate Candidate(string? version = "2.4.86", DateTimeOffset? createdAt = null) =>
        new(version, createdAt ?? At(9, 23));

    /// <summary>The release an environment is running for one work item, and when it arrived.</summary>
    public static WorkDeployment Deployment(string? version, DateTimeOffset? deployedAt) => new(version, deployedAt);

    /// <summary>Where an item is deployed: the three places, each a deployment handed over or the nothing a place
    /// that has not got it stands on - no place is filled on the caller's behalf, so a place the caller leaves
    /// out stands out. The filled road the <see cref="Item"/> fixture stands on is <see cref="FullEnvironments"/>.</summary>
    public static WorkItemEnvironments Environments(WorkDeployment? tdd = null, WorkDeployment? uat = null, WorkDeployment? prod = null) =>
        new(tdd, uat, prod);

    /// <summary>The road an item takes through the places as the shipped sample writes it: tdd and prod said with
    /// their own release and their own moment, uat the place that has not got it, and the one an item fixture
    /// stands on by default.</summary>
    public static WorkItemEnvironments FullEnvironments() =>
        Environments(Deployment("2.4.86", At(9, 23, 4)), null, Deployment("2.4.84", At(8, 6)));

    /// <summary>Work that had to be done again: the failures, the reverts and the rate of either.</summary>
    public static WorkRework Rework(int? ftFailures = 1, int? reverts = 0, double? rate = 0.125) =>
        new(ftFailures, reverts, rate);

    /// <summary>The bot's latest status, or the comment that says what blocked it.</summary>
    public static WorkNote Note(
        string? text = "Functional tests failed",
        string? url = "https://github.com/clearmeasure-aisf-sample-apps/cmdemo1-system/issues/490#issuecomment-1",
        DateTimeOffset? at = null,
        string? kind = "blocker") =>
        new(text, string.IsNullOrEmpty(url) ? null : new Uri(url), at ?? At(10), kind);

    /// <summary>When an item entered a column, in the file's order.</summary>
    public static ColumnVisit Visit(
        string? column = WorkColumns.Todo,
        DateTimeOffset? enteredAt = null,
        DateTimeOffset? leftAt = null,
        double? hours = null) =>
        new(column, enteredAt ?? At(9, 8), leftAt, hours);

    /// <summary>The counts of a slice of the board: the one a filled <see cref="WorkItemsReport"/> stands on.</summary>
    public static WorkItemsStats Stats(
        IReadOnlyDictionary<string, int>? wipByColumn = null,
        WorkDuration? cycleTimeHours = null,
        WorkDuration? leadTimeHours = null,
        IReadOnlyList<WorkWeek>? perWeek = null,
        WorkRework? rework = null,
        WorkCost? cost = null) =>
        new(
            wipByColumn ?? new Dictionary<string, int> { ["Todo"] = 2, ["Review"] = 1 },
            cycleTimeHours ?? Duration(6, 10.5, 3),
            leadTimeHours ?? Duration(12, 20.25, 3),
            perWeek ?? [],
            rework ?? Rework(),
            cost ?? new WorkCost(9.75, null));

    /// <summary>A duration in hours: the median, the 85th percentile and how many items were counted.</summary>
    public static WorkDuration Duration(double? median = 6, double? p85 = 10.5, int? n = 3) => new(median, p85, n);

    /// <summary>One week of the board: when it started, how many merged, how many reached prod.</summary>
    public static WorkWeek Week(DateTimeOffset? weekStart = null, int? merged = 4, int? deployedProd = 2) =>
        new(weekStart ?? At(2), merged, deployedProd);

    // --------------------------------------- the fully-filled records of the five files

    /// <summary>The board of the shipped <c>workitems.json</c>: its address, its project and its one sentence.</summary>
    public static WorkItemsScope Scope(
        string? repo = "https://github.com/clearmeasure-aisf-sample-apps/cmdemo1-system",
        int? project = 679,
        string? rules = "A bot may take an item from Todo. Done means the release is deployed to prod.") =>
        new(repo, project, rules);

    /// <summary>One item on the board, every part said with a word, a number, a moment or an address of its own: the
    /// record the tests turn a part of, and hand back whole wherever the equality of a list is not what is asked
    /// about. Each list is a fresh instance, so an item and its <c>with</c> copy share the very lists while two
    /// separately built items are two items.</summary>
    public static WorkItem Item(
        int? number = 490,
        string? title = "Pin the board",
        string? url = "https://github.com/clearmeasure-aisf-sample-apps/cmdemo1-system/issues/490",
        string? state = "Active",
        string? column = WorkColumns.Review,
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
            string.IsNullOrEmpty(url) ? null : new Uri(url),
            state,
            column,
            history ?? [Visit()],
            assignee,
            agent,
            sessions ?? [new Uri("https://cursor.com/agents/bc-490")],
            pulls ?? [PullRequest()],
            candidate ?? Candidate(),
            environments ?? FullEnvironments(),
            cost,
            worker,
            rework ?? Rework(),
            blocker ?? Note());

    /// <summary>The report a filled <c>workitems.json</c> reads as: its moment, its board, its counts and its two
    /// items, with the release each one carries. The bot and team splits stand said, the way a split file gives
    /// them, and each is a separate slice of the same counts.</summary>
    public static WorkItemsReport Report(
        DateTimeOffset? generatedAt = null,
        WorkItemsScope? scope = null,
        WorkItemsStats? stats = null,
        IReadOnlyList<WorkItem>? items = null,
        IReadOnlyList<WorkRelease>? releases = null,
        WorkItemsStats? botStats = null,
        WorkItemsStats? teamStats = null) =>
        new(
            generatedAt ?? At(9, 22),
            scope ?? Scope(),
            stats ?? Stats(),
            items ?? [Item(number: 490), Item(number: 491, column: WorkColumns.InProgress, worker: WorkWorker.Team)],
            releases ?? [new WorkRelease("2.4.86", "bbccddeeff00112233445566778899aabbccdde0", [490, 491])],
            botStats ?? Stats(),
            teamStats ?? Stats());
}
