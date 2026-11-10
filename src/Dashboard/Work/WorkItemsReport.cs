using System.Text.Json;
using Dashboard.Health;

namespace Dashboard.Work;

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
        return scope == new WorkItemsScope(null, null, null) ? null : scope;
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
    /// <paramref name="section"/> is <c>stats</c>. <c>stats.all</c> is the whole board, and the same fields written
    /// beside it (the older shape) are that same board. <c>stats.bot</c> and <c>stats.team</c> are the split; null
    /// when that object is missing or empty.
    /// </summary>
    internal static (WorkItemsStats All, WorkItemsStats? Bot, WorkItemsStats? Team) ReadSet(JsonElement? section)
    {
        if (section is null)
        {
            return (Empty, null, null);
        }

        var legacy = Read(section);
        var nested = JsonRead.Section(section, "all");
        var all = nested is null ? legacy : Read(nested);
        if (!all.HasAny && legacy.HasAny)
        {
            all = legacy;
        }

        return (all, Split(JsonRead.Section(section, "bot")), Split(JsonRead.Section(section, "team")));
    }

    private static WorkItemsStats? Split(JsonElement? section)
    {
        if (section is null)
        {
            return null;
        }

        var stats = Read(section);
        return stats.HasAny ? stats : null;
    }

    internal static WorkItemsStats Read(JsonElement? section)
    {
        if (section is null)
        {
            return Empty;
        }

        var throughput = ReadThroughput(section);
        return new WorkItemsStats(
            Counts(section, "wipByColumn"),
            WorkDuration.Read(JsonRead.Section(section, "cycleTimeHours")),
            WorkDuration.Read(JsonRead.Section(section, "leadTimeHours")),
            [.. JsonRead.Items(section, "perWeek").Select(WorkWeek.Read).OfType<WorkWeek>()],
            WorkRework.Read(JsonRead.Section(section, "rework")),
            WorkCost.Read(JsonRead.Section(section, "cost")),
            ColumnTimes(section),
            throughput,
            WorkDuration.Read(JsonRead.Section(section, "mergeToProdHours")));
    }

    private static List<WorkThroughput>? ReadThroughput(JsonElement? section)
    {
        if (JsonRead.Section(section, "throughputByAgent") is { } agents)
        {
            var rows = new List<WorkThroughput>();
            foreach (var property in agents.EnumerateObject())
            {
                if (JsonRead.Count(agents, property.Name) is { } done && !string.IsNullOrWhiteSpace(property.Name))
                {
                    rows.Add(new WorkThroughput(property.Name.Trim(), null, null, done));
                }
            }

            if (rows.Count > 0)
            {
                return [.. rows.OrderByDescending(row => row.Done ?? 0).ThenBy(row => row.Agent, StringComparer.OrdinalIgnoreCase)];
            }
        }

        var listed = JsonRead.Items(section, "throughput").Select(WorkThroughput.Read).OfType<WorkThroughput>().ToList();
        return listed.Count == 0 ? null : listed;
    }

    /// <summary><c>hoursByColumn</c> is the median hours per column. <c>timePerColumnHours</c> is the older object of durations.</summary>
    private static Dictionary<string, WorkDuration>? ColumnTimes(JsonElement? parent)
    {
        if ((JsonRead.Section(parent, "hoursByColumn") ?? JsonRead.Section(parent, "timePerColumnHours")) is not { } section)
        {
            return null;
        }

        var times = new Dictionary<string, WorkDuration>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in section.EnumerateObject())
        {
            if (string.IsNullOrWhiteSpace(property.Name))
            {
                continue;
            }

            var duration = property.Value.ValueKind == JsonValueKind.Object
                ? WorkDuration.Read(property.Value)
                : property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetDouble(out var hours) && double.IsFinite(hours) && hours >= 0
                    ? new WorkDuration(hours, null, null)
                    : null;
            if (duration is not null)
            {
                times[property.Name.Trim()] = duration;
            }
        }

        return times.Count == 0 ? null : times;
    }

    private static Dictionary<string, int> Counts(JsonElement? parent, string name)
    {
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (JsonRead.Section(parent, name) is not { } section)
        {
            return counts;
        }

        foreach (var property in section.EnumerateObject())
        {
            if (JsonRead.Count(section, property.Name) is { } count && !string.IsNullOrWhiteSpace(property.Name))
            {
                counts[property.Name.Trim()] = count;
            }
        }

        return counts;
    }
}

/// <summary>A duration in hours, as a median, an 85th percentile and how many items were counted.</summary>
public sealed record WorkDuration(double? Median, double? P85, int? N)
{
    internal static WorkDuration? Read(JsonElement? section)
    {
        var duration = new WorkDuration(JsonRead.Number(section, "median"), JsonRead.Number(section, "p85"), JsonRead.Count(section, "n"));
        return duration == new WorkDuration(null, null, null) ? null : duration;
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

        var week = new WorkWeek(JsonRead.Time(element, "weekStart"), JsonRead.Count(element, "merged"), JsonRead.Count(element, "deployedProd"));
        return week == new WorkWeek(null, null, null) ? null : week;
    }
}

/// <summary>Work that had to be done again.</summary>
/// <param name="FtFailures">How many items failed functional testing.</param>
/// <param name="Reverts">How many changes were reverted.</param>
/// <param name="Rate">The share of finished items that were either, as a fraction from 0 to 1. A number above 1 is kept as the file gave it.</param>
public sealed record WorkRework(int? FtFailures, int? Reverts, double? Rate)
{
    internal static WorkRework? Read(JsonElement? section)
    {
        var rework = new WorkRework(JsonRead.Count(section, "ftFailures"), JsonRead.Count(section, "reverts"), JsonRead.Number(section, "rate"));
        return rework == new WorkRework(null, null, null) ? null : rework;
    }
}

/// <summary>What the Cursor agents cost. <paramref name="TotalUsd"/> is null when the spend is not known; the note says why.</summary>
public sealed record WorkCost(double? TotalUsd, string? Note)
{
    internal static WorkCost? Read(JsonElement? section)
    {
        var cost = new WorkCost(JsonRead.Number(section, "totalUsd"), JsonRead.Text(section, "note"));
        return cost == new WorkCost(null, null) ? null : cost;
    }
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
/// <param name="Agent">The bot's name. <c>unnamed</c> when the file counts the work and does not name the bot.</param>
/// <param name="Merged">Pull requests that merged.</param>
/// <param name="DeployedProd">Items that reached production.</param>
/// <param name="Done">Items that reached Done, from <c>throughputByAgent</c>. Null for the older merged and deployed counts.</param>
public sealed record WorkThroughput(string Agent, int? Merged, int? DeployedProd, int? Done = null)
{
    internal static WorkThroughput? Read(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var name = JsonRead.Text(element, "agent") ?? JsonRead.Text(element, "name") ?? JsonRead.Text(element, "identity");
        var merged = JsonRead.Count(element, "merged");
        var deployed = JsonRead.Count(element, "deployedProd");
        return name is null && merged is null && deployed is null ? null : new WorkThroughput(name ?? "unnamed", merged, deployed);
    }
}

/// <summary>A short note the bot left: its latest status, or the comment that says what blocked it.</summary>
/// <param name="Kind"><c>status</c> or <c>blocker</c> when the feed says which. Null for an older note.</param>
public sealed record WorkNote(string? Text, Uri? Url, DateTimeOffset? At, string? Kind = null)
{
    internal static WorkNote? Read(JsonElement? section)
    {
        var text = JsonRead.Text(section, "text") ?? JsonRead.Text(section, "comment") ?? JsonRead.Text(section, "summary");
        var url = JsonRead.Address(section, "url");
        var at = JsonRead.Time(section, "at");
        return text is null && url is null && at is null ? null : new WorkNote(text, url, at);
    }
}

/// <summary>One work item on the board.</summary>
/// <param name="Number">Its number on the board; null when the file does not give one.</param>
/// <param name="Title">Its title.</param>
/// <param name="Url">Its page.</param>
/// <param name="State">The board's own state word, when it is not the column.</param>
/// <param name="Column">The column it is in now.</param>
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
    int? Number,
    string? Title,
    Uri? Url,
    string? State,
    string? Column,
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
            JsonRead.Count(element, "number"),
            JsonRead.Text(element, "title"),
            JsonRead.Address(element, "url"),
            JsonRead.Text(element, "state"),
            JsonRead.Text(element, "column"),
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
        return item.Number is null && item.Title is null && item.Url is null && item.Column is null ? null : item;
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

    private static WorkWorker ReadWorker(JsonElement element) => JsonRead.Text(element, "worker")?.ToLowerInvariant() switch
    {
        "bot" => WorkWorker.Bot,
        "team" => WorkWorker.Team,
        _ => WorkWorker.Unknown,
    };

    /// <summary><c>ftFailures</c> and <c>reverts</c> on the item. The older <c>rework</c> object is used when those are absent.</summary>
    private static WorkRework? ItemRework(JsonElement element)
    {
        var failures = JsonRead.Count(element, "ftFailures");
        var reverts = JsonRead.Count(element, "reverts");
        return failures is not null || reverts is not null
            ? new WorkRework(failures, reverts, null)
            : WorkRework.Read(JsonRead.Section(element, "rework"));
    }

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
        var kind = JsonRead.Text(section, "kind");
        var url = JsonRead.Address(section, "url");
        var at = JsonRead.Time(section, "at");
        var text = JsonRead.Text(section, "text");
        if (kind is null && url is null && at is null && text is null)
        {
            return null;
        }

        var label = kind?.ToLowerInvariant() switch
        {
            "blocker" => "Blocker",
            "status" => "Status",
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
        return visit.Column is null && visit.EnteredAt is null && visit.LeftAt is null && visit.Hours is null ? null : visit;
    }
}

/// <summary>A pull request opened for a work item.</summary>
/// <param name="Checks">What the pull request's checks concluded: <c>passed</c>, <c>failed</c>, <c>pending</c>, or the file's own word.</param>
/// <param name="ChecksUrl">The checks page, when the file gives one.</param>
public sealed record WorkPullRequest(int? Number, Uri? Url, string? State, DateTimeOffset? MergedAt, string? MergeSha, string? Checks = null, Uri? ChecksUrl = null)
{
    internal static WorkPullRequest? Read(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var (checks, checksUrl) = ChecksOf(element);
        var pull = new WorkPullRequest(
            JsonRead.Count(element, "number"),
            JsonRead.Address(element, "url"),
            JsonRead.Text(element, "state"),
            JsonRead.Time(element, "mergedAt"),
            JsonRead.Text(element, "mergeSha"),
            checks,
            checksUrl);
        return pull.Number is null && pull.Url is null && pull.State is null && pull.Checks is null && pull.ChecksUrl is null ? null : pull;
    }

    private static (string? Result, Uri? Url) ChecksOf(JsonElement element)
    {
        if (JsonRead.Section(element, "checks") is { } section)
        {
            var word = JsonRead.Text(section, "result") ?? JsonRead.Text(section, "state") ?? JsonRead.Text(section, "conclusion");
            return (CheckWord(word), JsonRead.Address(section, "url"));
        }

        return (CheckWord(JsonRead.Text(element, "checks") ?? JsonRead.Text(element, "checksResult")), JsonRead.Address(element, "checksUrl"));
    }

    private static string? CheckWord(string? word) => word?.ToLowerInvariant() switch
    {
        null => null,
        "success" or "succeeded" or "pass" or "passed" => "passed",
        "failure" or "failed" or "fail" => "failed",
        "pending" or "queued" or "in_progress" or "in progress" => "pending",
        _ => word!.Trim(),
    };
}

/// <summary>The Octopus release cut for an item, <c>2.4.&lt;build&gt;</c>, and when it was created.</summary>
public sealed record ReleaseCandidate(string? Version, DateTimeOffset? CreatedAt)
{
    internal static ReleaseCandidate? Read(JsonElement? section)
    {
        var candidate = new ReleaseCandidate(VersionText.Display(JsonRead.Text(section, "version")), JsonRead.Time(section, "createdAt"));
        return candidate == new ReleaseCandidate(null, null) ? null : candidate;
    }
}

/// <summary>Where an item is deployed. Null for an environment that does not have it.</summary>
public sealed record WorkItemEnvironments(WorkDeployment? Tdd, WorkDeployment? Uat, WorkDeployment? Prod)
{
    public static WorkItemEnvironments None { get; } = new(null, null, null);

    internal static WorkItemEnvironments Read(JsonElement? section) =>
        new(Deployment(section, "tdd"), Deployment(section, "uat"), Deployment(section, "prod"));

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
        new(VersionText.Display(JsonRead.Text(element, "version")), JsonRead.Time(element, "deployedAt"));
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

        var release = new WorkRelease(VersionText.Display(JsonRead.Text(element, "version")), JsonRead.Text(element, "sha"), items);
        return release.Version is null && items.Count == 0 ? null : release;
    }
}
