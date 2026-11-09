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
public sealed record WorkItemsReport(
    DateTimeOffset? GeneratedAt,
    WorkItemsScope? Scope,
    WorkItemsStats Stats,
    IReadOnlyList<WorkItem> Items,
    IReadOnlyList<WorkRelease> Releases)
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

            return new WorkItemsReport(
                JsonRead.Time(root, "generatedAt") ?? JsonRead.Time(root, "generated"),
                WorkItemsScope.Read(JsonRead.Section(root, "scope")),
                WorkItemsStats.Read(JsonRead.Section(root, "stats")),
                [.. JsonRead.Items(root, "items").Select(WorkItem.Read).OfType<WorkItem>()],
                [.. JsonRead.Items(root, "releases").Select(WorkRelease.Read).OfType<WorkRelease>()]);
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
        var scope = new WorkItemsScope(JsonRead.Text(section, "repo"), JsonRead.Count(section, "project"), JsonRead.Text(section, "rules"));
        return scope == new WorkItemsScope(null, null, null) ? null : scope;
    }
}

/// <summary>The counts the file gives for the whole board. Each part is null when the file does not give it.</summary>
/// <param name="WipByColumn">How many items sit in each column. The key is the column's name. Done may be absent: it is not work in progress.</param>
/// <param name="CycleTimeHours">How long an item takes from the start of the work to done.</param>
/// <param name="LeadTimeHours">How long an item takes from when it was opened to done.</param>
/// <param name="PerWeek">One entry per week, oldest first: how many items merged, and how many of those reached prod.</param>
/// <param name="Rework">Functional-test failures, reverts, and the rate of either.</param>
/// <param name="Cost">What the Cursor agents cost for these items; null when the file does not say.</param>
public sealed record WorkItemsStats(
    IReadOnlyDictionary<string, int> WipByColumn,
    WorkDuration? CycleTimeHours,
    WorkDuration? LeadTimeHours,
    IReadOnlyList<WorkWeek> PerWeek,
    WorkRework? Rework,
    WorkCost? Cost)
{
    public static WorkItemsStats Empty { get; } = new(new Dictionary<string, int>(), null, null, [], null, null);

    internal static WorkItemsStats Read(JsonElement? section) =>
        section is null
            ? Empty
            : new WorkItemsStats(
                Counts(section, "wipByColumn"),
                WorkDuration.Read(JsonRead.Section(section, "cycleTimeHours")),
                WorkDuration.Read(JsonRead.Section(section, "leadTimeHours")),
                [.. JsonRead.Items(section, "perWeek").Select(WorkWeek.Read).OfType<WorkWeek>()],
                WorkRework.Read(JsonRead.Section(section, "rework")),
                WorkCost.Read(JsonRead.Section(section, "cost")));

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
    double? CostUsd)
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
            JsonRead.Number(element, "costUsd"));
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
}

/// <summary>When an item entered a column.</summary>
public sealed record ColumnVisit(string? Column, DateTimeOffset? EnteredAt)
{
    internal static ColumnVisit? Read(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var visit = new ColumnVisit(JsonRead.Text(element, "column"), JsonRead.Time(element, "enteredAt"));
        return visit.Column is null && visit.EnteredAt is null ? null : visit;
    }
}

/// <summary>A pull request opened for a work item.</summary>
public sealed record WorkPullRequest(int? Number, Uri? Url, string? State, DateTimeOffset? MergedAt, string? MergeSha)
{
    internal static WorkPullRequest? Read(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var pull = new WorkPullRequest(
            JsonRead.Count(element, "number"),
            JsonRead.Address(element, "url"),
            JsonRead.Text(element, "state"),
            JsonRead.Time(element, "mergedAt"),
            JsonRead.Text(element, "mergeSha"));
        return pull.Number is null && pull.Url is null && pull.State is null ? null : pull;
    }
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
