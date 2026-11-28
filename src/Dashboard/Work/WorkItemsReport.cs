using System.Text.Json;
using Dashboard.Health;

namespace Dashboard.Work;

internal static class ReadParts
{
    /// <summary>True when at least one of the parts the caller names was given. A <c>Read</c> drops a record whose named parts are all unsaid.</summary>
    internal static bool Said(params object?[] parts) => parts.Any(part => part is not null);
}

/// <summary>
/// Which work items the system's bots are handling: the content of <c>workitems.json</c>, published next to the
/// delivery facts (<see cref="FileName"/>). The page reads it and only reads it. Every part is optional, and a file
/// that is not there is no report.
/// </summary>
/// <param name="GeneratedAt">When the file was produced. It is not the time of a check.</param>
/// <param name="Scope">Which board the items come from; null when the file does not say.</param>
/// <param name="Stats">The counts across the items. An empty file still has stats: every part of them may be empty.</param>
/// <param name="Items">The work items, in the file's order.</param>
/// <param name="Releases">The Octopus releases and the work items each one carries.</param>
/// <param name="BotStats">The same counts for bot work only; null when the file does not split them out.</param>
/// <param name="TeamStats">The same counts for team work only; null when the file does not split them out.</param>
public sealed record WorkItemsReport(
    DateTimeOffset? GeneratedAt,
    WorkItemsScope? Scope,
    WorkItemsStats Stats,
    IReadOnlyList<WorkItem> Items,
    IReadOnlyList<WorkRelease> Releases,
    WorkItemsStats? BotStats = null,
    WorkItemsStats? TeamStats = null)
{
    /// <summary>The file's name, next to <c>delivery.json</c> on the system repository's <c>status</c> branch.</summary>
    public const string FileName = "workitems.json";

    /// <summary>
    /// The file's content. Null when there is no JSON object: the file is not there, or it is not this file. An
    /// object whose parts are missing is a report with nothing in it, so a file that has started arriving is shown.
    /// </summary>
    public static WorkItemsReport? Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var stats = JsonRead.Section(root, "stats");
            var (all, bot, team) = WorkItemsStats.ReadSet(stats);
            return new WorkItemsReport(
                JsonRead.Time(root, "generatedAt") ?? JsonRead.Time(root, "generated"),
                WorkItemsScope.Read(JsonRead.Section(root, "scope")),
                all,
                [.. JsonRead.Items(root, "items").Select(WorkItem.Read).OfType<WorkItem>()],
                [.. JsonRead.Items(root, "releases").Select(WorkRelease.Read).OfType<WorkRelease>()],
                bot,
                team);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

/// <summary>Which board the items were read from.</summary>
/// <param name="Repo">The repository the board belongs to: an address, or a name when the file gives no address.</param>
/// <param name="Project">The project number on that board (cmdemo1's is 679); null when the file does not say.</param>
/// <param name="Rules">How a bot may take an item, in the file's own words; null when it does not say.</param>
public sealed record WorkItemsScope(string? Repo, int? Project, string? Rules)
{
    internal static WorkItemsScope? Read(JsonElement? section)
    {
        var scope = new WorkItemsScope(JsonRead.Text(section, "repo"), JsonRead.Count(section, "project"), ReadRules(section));
        return ReadParts.Said(scope.Repo, scope.Project, scope.Rules) ? scope : null;
    }

    /// <summary>A string, or the sentences of an array joined. The feed publishes an array.</summary>
    private static string? ReadRules(JsonElement? section)
    {
        if (JsonRead.Text(section, "rules") is { } text)
        {
            return text;
        }

        var lines = JsonRead.Items(section, "rules")
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString()?.Trim())
            .Where(line => !string.IsNullOrEmpty(line))
            .Cast<string>()
            .ToList();
        return lines.Count == 0 ? null : string.Join(" ", lines);
    }
}

/// <summary>The counts the file gives for the whole board. Each part is null when the file does not give it.</summary>
/// <param name="WipByColumn">How many items sit in each column. The key is the column's name. Done may be absent: it is not work in progress.</param>
/// <param name="CycleTimeHours">How long an item takes from the start of the work to done.</param>
/// <param name="LeadTimeHours">How long an item takes from when it was opened to done.</param>
/// <param name="PerWeek">One entry per week, oldest first: how many items merged, and how many of those reached prod.</param>
/// <param name="Rework">Functional-test failures, reverts, and the rate of either.</param>
/// <param name="Cost">What the Cursor agents cost for these items; null when the file does not say.</param>
/// <param name="TimePerColumnHours">How long bot items wait in each column. Null when the file does not say.</param>
/// <param name="Throughput">How much each bot identity finished. Null when the file does not say.</param>
/// <param name="MergeToProdHours">From a pull request merging to the production deployment. Null when the file does not say.</param>
public sealed record WorkItemsStats(
    IReadOnlyDictionary<string, int> WipByColumn,
    WorkDuration? CycleTimeHours,
    WorkDuration? LeadTimeHours,
    IReadOnlyList<WorkWeek> PerWeek,
    WorkRework? Rework,
    WorkCost? Cost,
    IReadOnlyDictionary<string, WorkDuration>? TimePerColumnHours = null,
    IReadOnlyList<WorkThroughput>? Throughput = null,
    WorkDuration? MergeToProdHours = null)
{
    public static WorkItemsStats Empty { get; } = new(new Dictionary<string, int>(), null, null, [], null, null);

    /// <summary>True when the object carries any count. An empty object is treated as absent, so the page can compute the split from the items.</summary>
    public bool HasAny =>
        WipByColumn.Count > 0 || CycleTimeHours is not null || LeadTimeHours is not null || PerWeek.Count > 0
        || Rework is not null || Cost is not null || TimePerColumnHours is { Count: > 0 } || Throughput is { Count: > 0 }
        || MergeToProdHours is not null;

    /// <summary>
    /// <paramref name="section"/> is <c>stats</c>: the whole board, which of its two shapes is meant is
    /// <see cref="WholeBoard"/>, and the <c>bot</c> and <c>team</c> split of it - each of the three null-ready parts
    /// read by <see cref="ReadSlice"/>, so an object that says nothing is absent.
    /// </summary>
    internal static (WorkItemsStats All, WorkItemsStats? Bot, WorkItemsStats? Team) ReadSet(JsonElement? section)
    {
        if (section is null)
        {
            return (Empty, null, null);
        }

        return (WholeBoard(section), ReadSlice(section, "bot"), ReadSlice(section, "team"));
    }

    /// <summary>
    /// Which object of <c>stats</c> is the whole board, named in one place: the nested <c>stats.all</c> when it
    /// carries a count, and the same fields written beside it (the older shape - <c>stats.wipByColumn</c> and the
    /// rest) when it carries none or is not there at all.
    /// <para>
    /// A mixed file is therefore read from beside it: <c>{ "stats": { "all": { }, "wipByColumn": { "Todo": 2 } } }</c>
    /// is a board with two items in Todo, because the empty <c>all</c> says nothing and the counts beside it answer.
    /// The same file with nothing beside it - <c>{ "stats": { "all": { } } }</c> - is an empty count: the file did
    /// start arriving, so the page shows it and computes what it can from the items.
    /// </para>
    /// </summary>
    private static WorkItemsStats WholeBoard(JsonElement? stats) => ReadSlice(stats, "all") ?? Read(stats);

    /// <summary>
    /// The counts of the object of <paramref name="name"/> under <paramref name="stats"/> - the whole board's
    /// <c>all</c>, or one side of the <c>bot</c>/<c>team</c> split; null when there is no such object or it says
    /// nothing. An object that says nothing is treated as absent, so the page can compute that part from the items.
    /// </summary>
    private static WorkItemsStats? ReadSlice(JsonElement? stats, string name)
    {
        if (JsonRead.Section(stats, name) is not { } section)
        {
            return null;
        }

        var slice = Read(section);
        return slice.HasAny ? slice : null;
    }

    internal static WorkItemsStats Read(JsonElement? section)
    {
        if (section is null)
        {
            return Empty;
        }

        return new WorkItemsStats(
            WipByColumn: Counts(section, "wipByColumn"),
            CycleTimeHours: Duration(section, "cycleTimeHours"),
            LeadTimeHours: Duration(section, "leadTimeHours"),
            PerWeek: [.. JsonRead.Items(section, "perWeek").Select(WorkWeek.Read).OfType<WorkWeek>()],
            Rework: WorkRework.Read(JsonRead.Section(section, WorkRework.SectionWord)),
            Cost: WorkCost.Read(JsonRead.Section(section, "cost")),
            TimePerColumnHours: ColumnTimes(section),
            Throughput: ReadThroughput(section),
            MergeToProdHours: Duration(section, "mergeToProdHours"));
    }

    /// <summary>The duration object of <paramref name="name"/> under <paramref name="section"/>; null when it says nothing.</summary>
    private static WorkDuration? Duration(JsonElement? section, string name) => WorkDuration.Read(JsonRead.Section(section, name));

    /// <summary>
    /// The done count per agent: <c>throughputByAgent</c> (agent to count), most done first and then by the agent's
    /// name; the older <c>throughput</c> array when that object is absent or no agent in it has a count; null when
    /// neither has a row. Agents whose names differ only in case stay separate rows.
    /// </summary>
    private static List<WorkThroughput>? ReadThroughput(JsonElement? section)
    {
        var rows = Keyed(JsonRead.Section(section, "throughputByAgent"), CountOf)
            .Select(entry => new WorkThroughput(entry.Key, null, null, entry.Value))
            .ToList();
        if (rows.Count > 0)
        {
            return [.. rows.OrderByDescending(row => row.Done ?? 0).ThenBy(row => row.Agent, StringComparer.OrdinalIgnoreCase)];
        }

        var listed = JsonRead.Items(section, "throughput").Select(WorkThroughput.Read).OfType<WorkThroughput>().ToList();
        return listed.Count == 0 ? null : listed;
    }

    /// <summary>
    /// The time spent in each column, looked up without regard to case; null when the file has no such object or no
    /// column in it reads as a duration (see <see cref="DurationOf"/>).
    /// </summary>
    private static Dictionary<string, WorkDuration>? ColumnTimes(JsonElement? parent)
    {
        var times = new Dictionary<string, WorkDuration>(StringComparer.OrdinalIgnoreCase);
        foreach (var (column, duration) in Keyed(ColumnTimesSection(parent), (_, member) => DurationOf(member.Value)))
        {
            times[column] = duration!;
        }

        return times.Count == 0 ? null : times;
    }

    /// <summary>The two file shapes of the column times: <c>hoursByColumn</c>, else the older <c>timePerColumnHours</c>.</summary>
    private static JsonElement? ColumnTimesSection(JsonElement? parent) =>
        JsonRead.Section(parent, "hoursByColumn") ?? JsonRead.Section(parent, "timePerColumnHours");

    /// <summary>
    /// A column's duration, written either way: an object of durations (<c>median</c>, <c>p85</c>, <c>n</c>), or a plain
    /// number of hours that is finite and not negative. A duration read from a number is the median alone: it carries
    /// no p85 and no count. Anything else is null.
    /// </summary>
    private static WorkDuration? DurationOf(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Object => WorkDuration.Read(value),
        JsonValueKind.Number when value.TryGetDouble(out var hours) && double.IsFinite(hours) && hours >= 0 => new WorkDuration(hours, null, null),
        _ => null,
    };

    /// <summary>
    /// The count of each key of the object of <paramref name="name"/>, looked up without regard to case; an empty
    /// table (never null) when there is no such object or no key in it holds a count.
    /// </summary>
    private static Dictionary<string, int> Counts(JsonElement? parent, string name)
    {
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, count) in Keyed(JsonRead.Section(parent, name), CountOf))
        {
            counts[key] = count!.Value;
        }

        return counts;
    }

    /// <summary>A member's count as <see cref="JsonRead.Count"/> reads it, looked up by the member's own name in <paramref name="section"/>.</summary>
    private static int? CountOf(JsonElement section, JsonProperty member) => JsonRead.Count(section, member.Name);

    /// <summary>
    /// The one walk of a JSON object used as a dictionary: each member in the order written, its name trimmed, its
    /// value made by <paramref name="read"/> (the strategy that says what a value is). A member with a blank name, or
    /// whose value reads as null, is left out. None when there is no object. The callers choose their own table and
    /// their own answer for "nothing survived".
    /// </summary>
    private static IEnumerable<(string Key, T Value)> Keyed<T>(JsonElement? section, Func<JsonElement, JsonProperty, T> read)
    {
        if (section is not { } keyed)
        {
            yield break;
        }

        foreach (var member in keyed.EnumerateObject())
        {
            if (!string.IsNullOrWhiteSpace(member.Name) && read(keyed, member) is { } value)
            {
                yield return (member.Name.Trim(), value);
            }
        }
    }
}

/// <summary>A duration in hours, as a median, an 85th percentile and how many items were counted.</summary>
public sealed record WorkDuration(double? Median, double? P85, int? N)
{
    internal static WorkDuration? Read(JsonElement? section)
    {
        var duration = new WorkDuration(JsonRead.Number(section, "median"), JsonRead.Number(section, "p85"), JsonRead.Count(section, "n"));
        return ReadParts.Said(duration.Median, duration.P85, duration.N) ? duration : null;
    }
}

/// <summary>One week of the board: when it started, how many pull requests merged, how many items reached prod.</summary>
public sealed record WorkWeek(DateTimeOffset? WeekStart, int? Merged, int? DeployedProd)
{
    internal static WorkWeek? Read(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var week = new WorkWeek(JsonRead.Time(element, "weekStart"), JsonRead.Count(element, WorkThroughput.MergedWord), JsonRead.Count(element, WorkThroughput.DeployedProdWord));
        return ReadParts.Said(week.WeekStart, week.Merged, week.DeployedProd) ? week : null;
    }
}

/// <summary>Work that had to be done again.</summary>
/// <param name="FtFailures">How many items failed functional testing.</param>
/// <param name="Reverts">How many changes were reverted.</param>
/// <param name="Rate">The share of finished items that were either, as a fraction from 0 to 1. A number above 1 is kept as the file gave it.</param>
public sealed record WorkRework(int? FtFailures, int? Reverts, double? Rate)
{
    /// <summary>The word the work-items file uses for the functional-test failure count.</summary>
    internal const string FtFailuresWord = "ftFailures";

    /// <summary>The word the work-items file uses for the revert count.</summary>
    internal const string RevertsWord = "reverts";

    /// <summary>The word the work-items file uses for the rework section.</summary>
    internal const string SectionWord = "rework";

    /// <summary>The word the work-items file uses for the rework rate.</summary>
    private const string RateWord = "rate";

    internal static WorkRework? Read(JsonElement? section)
    {
        var rework = new WorkRework(JsonRead.Count(section, FtFailuresWord), JsonRead.Count(section, RevertsWord), JsonRead.Number(section, RateWord));
        return ReadParts.Said(rework.FtFailures, rework.Reverts, rework.Rate) ? rework : null;
    }
}

/// <summary>What the Cursor agents cost. <paramref name="TotalUsd"/> is null when the spend is not known; the note says why.</summary>
public sealed record WorkCost(double? TotalUsd, string? Note)
{
    internal static WorkCost? Read(JsonElement? section)
    {
        var cost = new WorkCost(JsonRead.Number(section, "totalUsd"), JsonRead.Text(section, "note"));
        return ReadParts.Said(cost.TotalUsd, cost.Note) ? cost : null;
    }
}

/// <summary>Whether the work items view has a file to show.</summary>
public enum WorkItemsAvailability
{
    /// <summary>The address is known and the first read has not finished.</summary>
    Loading,

    /// <summary>The topology names no delivery file, so there is no sibling to read.</summary>
    NoAddress,

    /// <summary>The read found nothing: the file is not published yet, or it is not this file.</summary>
    NotPublished,

    /// <summary>A file was read.</summary>
    Ready,
}

/// <summary>Who is doing the work. Unknown when the file does not say: the item stays in All and in neither filter.</summary>
public enum WorkWorker
{
    Unknown,
    Bot,
    Team,
}

/// <summary>Whose work a filter shows. All is the whole file.</summary>
public enum WorkAudience
{
    All,
    Bot,
    Team,
}

/// <summary>How much one bot identity finished.</summary>
/// <param name="Agent">The bot's name. <see cref="Unnamed"/> when the file counts the work and does not name the bot.</param>
/// <param name="Merged">Pull requests that merged.</param>
/// <param name="DeployedProd">Items that reached production.</param>
/// <param name="Done">Items that reached Done, from <c>throughputByAgent</c>. Null for the older merged and deployed counts.</param>
public sealed record WorkThroughput(string Agent, int? Merged, int? DeployedProd, int? Done = null)
{
    /// <summary>The agent name a row is shown under when nothing names the bot.</summary>
    public const string Unnamed = "unnamed";

    /// <summary>The word the work-items file uses for the merged count.</summary>
    internal const string MergedWord = "merged";

    /// <summary>The word the work-items file uses for the count of items that reached production.</summary>
    internal const string DeployedProdWord = "deployedProd";

    internal static WorkThroughput? Read(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var name = JsonRead.Text(element, "agent") ?? JsonRead.Text(element, "name") ?? JsonRead.Text(element, "identity");
        var merged = JsonRead.Count(element, MergedWord);
        var deployed = JsonRead.Count(element, DeployedProdWord);
        return ReadParts.Said(name, merged, deployed) ? new WorkThroughput(name ?? Unnamed, merged, deployed) : null;
    }
}

/// <summary>A short note the bot left: its latest status, or the comment that says what blocked it.</summary>
/// <param name="Kind"><c>status</c> or <c>blocker</c> when the feed says which. Null for an older note.</param>
public sealed record WorkNote(string? Text, Uri? Url, DateTimeOffset? At, string? Kind = null)
{
    /// <summary>The feed's kind word for a note that says what blocked the item.</summary>
    internal const string BlockerKind = "blocker";

    /// <summary>The feed's kind word for a note that gives the bot's latest status.</summary>
    internal const string StatusKind = "status";

    /// <summary>The heading shown over a blocker note.</summary>
    internal const string BlockerHeading = "Blocker";

    /// <summary>The heading shown over a status note, and over a note of any other kind.</summary>
    internal const string StatusHeading = "Status";

    /// <summary>The feed's word for the note's words.</summary>
    internal const string TextWord = "text";

    /// <summary>The feed's word for the note's page. The note is not a board item, so it names its own page rather than <see cref="WorkItemIdentity.UrlWord" />.</summary>
    internal const string UrlWord = "url";

    /// <summary>The feed's word for when the note was written.</summary>
    internal const string AtWord = "at";

    /// <summary>The feed's word for whether the note is a status or a blocker.</summary>
    internal const string KindWord = "kind";

    internal static WorkNote? Read(JsonElement? section)
    {
        var text = JsonRead.Text(section, TextWord) ?? JsonRead.Text(section, "comment") ?? JsonRead.Text(section, "summary");
        var url = JsonRead.Address(section, UrlWord);
        var at = JsonRead.Time(section, AtWord);
        return ReadParts.Said(text, url, at) ? new WorkNote(text, url, at) : null;
    }
}

/// <summary>Which item on the board this is: its number, title, page, state word and column.</summary>
/// <param name="Number">Its number on the board; null when the file does not give one.</param>
/// <param name="Title">Its title.</param>
/// <param name="Url">Its page.</param>
/// <param name="State">The board's own state word, when it is not the column.</param>
/// <param name="Column">The column it is in now.</param>
public sealed record WorkItemIdentity(int? Number, string? Title, Uri? Url, string? State, string? Column)
{
    /// <summary>True when the file names no item: no number, title, page or column. A state word alone names none.</summary>
    public bool IsUnsaid => Number is null && Title is null && Url is null && Column is null;

    /// <summary>The file's word for an item's or pull request's number on the board.</summary>
    internal const string NumberWord = "number";

    /// <summary>The file's word for an item's or pull request's page, and for a checks page.</summary>
    internal const string UrlWord = "url";

    /// <summary>The file's word for an item's or pull request's state word, and a checks state.</summary>
    internal const string StateWord = "state";

    internal static WorkItemIdentity Read(JsonElement element) =>
        new(
            JsonRead.Count(element, NumberWord),
            JsonRead.Text(element, "title"),
            JsonRead.Address(element, UrlWord),
            JsonRead.Text(element, StateWord),
            JsonRead.Text(element, "column"));
}

/// <summary>One work item on the board.</summary>
/// <remarks>An item's identity is <c>Number</c>, <c>Title</c>, <c>Url</c> and <c>Column</c>. An item that says none of them is left out, even when it carries a cost, pull requests or a release candidate. An item that says only a column is kept.</remarks>
/// <param name="Identity">Which item it is: its number, title, page, state word and column.</param>
/// <param name="ColumnHistory">When it entered each column, in the file's order.</param>
/// <param name="Assignee">Who it is assigned to, a person or a bot.</param>
/// <param name="Agent">The bot handling it, when one is.</param>
/// <param name="CursorSessions">The Cursor sessions that worked on it.</param>
/// <param name="PullRequests">The pull requests opened for it.</param>
/// <param name="ReleaseCandidate">The Octopus release cut for it (<c>2.4.&lt;build&gt;</c>); null when none has been.</param>
/// <param name="Environments">Where that work is deployed. A missing environment has not got it.</param>
/// <param name="CostUsd">What the Cursor agent cost for this item, in US dollars; null when it was not metered.</param>
/// <param name="Worker">Bot or team. Unknown when the file does not say.</param>
/// <param name="Rework">Functional-test failures and reverts of this item; null when the file does not say.</param>
/// <param name="Blocker">The bot's latest status, or the comment that says what blocked it; null when the file does not say.</param>
public sealed record WorkItem(
    WorkItemIdentity Identity,
    IReadOnlyList<ColumnVisit> ColumnHistory,
    string? Assignee,
    string? Agent,
    IReadOnlyList<Uri> CursorSessions,
    IReadOnlyList<WorkPullRequest> PullRequests,
    ReleaseCandidate? ReleaseCandidate,
    WorkItemEnvironments Environments,
    double? CostUsd,
    WorkWorker Worker = WorkWorker.Unknown,
    WorkRework? Rework = null,
    WorkNote? Blocker = null)
{
    internal static WorkItem? Read(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var item = new WorkItem(
            WorkItemIdentity.Read(element),
            [.. JsonRead.Items(element, "columnHistory").Select(ColumnVisit.Read).OfType<ColumnVisit>()],
            JsonRead.Text(element, "assignee"),
            JsonRead.Text(element, "agent"),
            Addresses(element, "cursorSessions"),
            [.. JsonRead.Items(element, "prs").Select(WorkPullRequest.Read).OfType<WorkPullRequest>()],
            ReleaseCandidate.Read(JsonRead.Section(element, "releaseCandidate")),
            WorkItemEnvironments.Read(JsonRead.Section(element, "environments")),
            JsonRead.Number(element, "costUsd"),
            ReadWorker(element),
            ItemRework(element),
            Note(element));
        return ReadParts.Said(item.Identity.Number, item.Identity.Title, item.Identity.Url, item.Identity.Column) ? item : null;
    }

    private static List<Uri> Addresses(JsonElement parent, string name)
    {
        var addresses = new List<Uri>();
        foreach (var item in JsonRead.Items(parent, name))
        {
            if (item.ValueKind == JsonValueKind.String && JsonRead.Address(item.GetString()) is { } address)
            {
                addresses.Add(address);
            }
        }

        return addresses;
    }

    private static WorkWorker ReadWorker(JsonElement element) => WorkAudienceWords.WorkerOf(JsonRead.Text(element, "worker"));

    /// <summary><see cref="WorkRework.FtFailuresWord"/> and <see cref="WorkRework.RevertsWord"/> on the item. The older <see cref="WorkRework.SectionWord"/> object is used when those are absent.</summary>
    private static WorkRework? ItemRework(JsonElement element)
    {
        var failures = JsonRead.Count(element, WorkRework.FtFailuresWord);
        var reverts = JsonRead.Count(element, WorkRework.RevertsWord);
        return failures is not null || reverts is not null
            ? new WorkRework(failures, reverts, null)
            : WorkRework.Read(JsonRead.Section(element, WorkRework.SectionWord));
    }

    /// <summary>The first note found, in order: <c>latestStatusComment</c>, <c>blocker</c>, <c>comment</c>, then <c>status</c> with <c>statusUrl</c> and <c>statusAt</c>.</summary>
    private static WorkNote? Note(JsonElement element) =>
        StatusComment(JsonRead.Section(element, "latestStatusComment"))
        ?? WorkNote.Read(JsonRead.Section(element, "blocker"))
        ?? WorkNote.Read(JsonRead.Section(element, "comment"))
        ?? (JsonRead.Text(element, "status") is { } status
            ? new WorkNote(status, JsonRead.Address(element, "statusUrl"), JsonRead.Time(element, "statusAt"))
            : null);

    /// <summary><c>latestStatusComment</c>: a link, when it was written, and whether it is a status or a blocker.</summary>
    private static WorkNote? StatusComment(JsonElement? section)
    {
        var kind = JsonRead.Text(section, WorkNote.KindWord);
        var url = JsonRead.Address(section, WorkNote.UrlWord);
        var at = JsonRead.Time(section, WorkNote.AtWord);
        var text = JsonRead.Text(section, WorkNote.TextWord);
        if (!ReadParts.Said(kind, url, at, text))
        {
            return null;
        }

        var label = kind?.ToLowerInvariant() switch
        {
            WorkNote.BlockerKind => WorkNote.BlockerHeading,
            WorkNote.StatusKind => WorkNote.StatusHeading,
            null => text,
            _ => kind,
        };
        return new WorkNote(text ?? label, url, at, kind);
    }
}

/// <summary>When an item entered a column.</summary>
/// <param name="LeftAt">When the item left the column. Null while it is still there.</param>
/// <param name="Hours">How long it spent there, including up to the run while it is still there.</param>
public sealed record ColumnVisit(string? Column, DateTimeOffset? EnteredAt, DateTimeOffset? LeftAt = null, double? Hours = null)
{
    internal static ColumnVisit? Read(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var visit = new ColumnVisit(
            JsonRead.Text(element, "column"),
            JsonRead.Time(element, "enteredAt"),
            JsonRead.Time(element, "leftAt"),
            JsonRead.Number(element, "hours"));
        return ReadParts.Said(visit.Column, visit.EnteredAt, visit.LeftAt, visit.Hours) ? visit : null;
    }
}

/// <summary>A pull request opened for a work item.</summary>
/// <remarks>A pull request is kept when it says any of <c>Number</c>, <c>Url</c>, <c>State</c>, <c>Checks</c> or <c>ChecksUrl</c>. One that says only <c>MergedAt</c> and/or <c>MergeSha</c> is left out.</remarks>
/// <param name="Checks">What the pull request's checks concluded: <see cref="PassedChecks"/>, <see cref="FailedChecks"/>, <see cref="PendingChecks"/>, or the file's own word.</param>
/// <param name="ChecksUrl">The checks page, when the file gives one.</param>
public sealed record WorkPullRequest(int? Number, Uri? Url, string? State, DateTimeOffset? MergedAt, string? MergeSha, string? Checks = null, Uri? ChecksUrl = null)
{
    /// <summary>The word the page shows when the checks passed.</summary>
    public const string PassedChecks = "passed";

    /// <summary>The word the page shows when the checks failed.</summary>
    public const string FailedChecks = "failed";

    /// <summary>The word the page shows when the checks are still pending.</summary>
    public const string PendingChecks = "pending";
    internal static WorkPullRequest? Read(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var (checks, checksUrl) = ChecksOf(element);
        var pull = new WorkPullRequest(
            JsonRead.Count(element, WorkItemIdentity.NumberWord),
            JsonRead.Address(element, WorkItemIdentity.UrlWord),
            JsonRead.Text(element, WorkItemIdentity.StateWord),
            JsonRead.Time(element, "mergedAt"),
            JsonRead.Text(element, "mergeSha"),
            checks,
            checksUrl);
        return ReadParts.Said(pull.Number, pull.Url, pull.State, checks, checksUrl) ? pull : null;
    }

    /// <summary>The field the file uses for the checks object or flat word.</summary>
    private const string ChecksField = "checks";

    private static (string? Result, Uri? Url) ChecksOf(JsonElement element)
    {
        if (JsonRead.Section(element, ChecksField) is { } section)
        {
            var word = JsonRead.Text(section, "result") ?? JsonRead.Text(section, WorkItemIdentity.StateWord) ?? JsonRead.Text(section, "conclusion");
            return (CheckWord(word), JsonRead.Address(section, WorkItemIdentity.UrlWord));
        }

        return (CheckWord(JsonRead.Text(element, ChecksField) ?? JsonRead.Text(element, "checksResult")), JsonRead.Address(element, "checksUrl"));
    }

    private static string? CheckWord(string? word) => word?.ToLowerInvariant() switch
    {
        null => null,
        "success" or "succeeded" or "pass" or "passed" => PassedChecks,
        "failure" or "failed" or "fail" => FailedChecks,
        "pending" or "queued" or "in_progress" or "in progress" => PendingChecks,
        _ => word!.Trim(),
    };
}

/// <summary>The Octopus release cut for an item, <c>2.4.&lt;build&gt;</c>, and when it was created.</summary>
public sealed record ReleaseCandidate(string? Version, DateTimeOffset? CreatedAt)
{
    internal static ReleaseCandidate? Read(JsonElement? section)
    {
        var candidate = new ReleaseCandidate(VersionText.Read(section), JsonRead.Time(section, "createdAt"));
        return ReadParts.Said(candidate.Version, candidate.CreatedAt) ? candidate : null;
    }
}

/// <summary>Where an item is deployed. Null for an environment that does not have it.</summary>
public sealed record WorkItemEnvironments(WorkDeployment? Tdd, WorkDeployment? Uat, WorkDeployment? Prod)
{
    public static WorkItemEnvironments None { get; } = new(null, null, null);

    /// <summary>The word the work-items file uses for the tdd environment, and the place name the page shows for it.</summary>
    public const string TddWord = "tdd";

    /// <summary>The word the work-items file uses for the uat environment, and the place name the page shows for it.</summary>
    public const string UatWord = "uat";

    /// <summary>The word the work-items file uses for the prod environment, and the place name the page shows for it.</summary>
    public const string ProdWord = "prod";

    internal static WorkItemEnvironments Read(JsonElement? section) =>
        new(Deployment(section, TddWord), Deployment(section, UatWord), Deployment(section, ProdWord));

    private static WorkDeployment? Deployment(JsonElement? parent, string name)
    {
        if (parent is not { ValueKind: JsonValueKind.Object } value)
        {
            return null;
        }

        foreach (var property in value.EnumerateObject())
        {
            if (!string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return property.Value.ValueKind == JsonValueKind.Object ? WorkDeployment.Read(property.Value) : null;
        }

        return null;
    }
}

/// <summary>The release an environment is running for one work item, and when it arrived.</summary>
public sealed record WorkDeployment(string? Version, DateTimeOffset? DeployedAt)
{
    internal static WorkDeployment Read(JsonElement element) =>
        new(VersionText.Read(element), JsonRead.Time(element, "deployedAt"));
}

/// <summary>One Octopus release and the work items it carries.</summary>
/// <param name="Version">The release version, <c>2.4.&lt;build&gt;</c>.</param>
/// <param name="Sha">The commit the release was cut from.</param>
/// <param name="Items">The work item numbers in the release, in the file's order.</param>
public sealed record WorkRelease(string? Version, string? Sha, IReadOnlyList<int> Items)
{
    internal static WorkRelease? Read(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var items = new List<int>();
        foreach (var item in JsonRead.Items(element, "items"))
        {
            if (item.ValueKind == JsonValueKind.Number && item.TryGetInt32(out var number) && number >= 0)
            {
                items.Add(number);
            }
            else if (item.ValueKind == JsonValueKind.Number && item.TryGetDouble(out var real) && double.IsFinite(real) && real >= 0 && real <= int.MaxValue)
            {
                items.Add((int)Math.Round(real));
            }
        }

        var release = new WorkRelease(VersionText.Read(element), JsonRead.Text(element, "sha"), items);
        return release.Version is null && items.Count == 0 ? null : release;
    }
}
