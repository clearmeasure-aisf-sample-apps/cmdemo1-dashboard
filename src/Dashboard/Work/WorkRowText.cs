using System.Globalization;
using Dashboard.Health;

namespace Dashboard.Work;

/// <summary>
/// The words of one work-item row: its number and link title, who works it, its pull requests and Cursor sessions,
/// the environments it has reached, and its release candidate. <see cref="WorkItemsText"/> is the page's door to these.
/// </summary>
public static class WorkRowText
{
    public static string Number(WorkItem item) =>
        item.Number is { } number ? "#" + number.ToString(CultureInfo.InvariantCulture) : "Item";

    public static string IssueTitle(WorkItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var name = item.Number is { } number ? $"Work item #{number.ToString(CultureInfo.InvariantCulture)}" : "Work item";
        return item.State is { } state ? $"{name}, {state}{LinkText.GitHubSuffix}" : name + LinkText.GitHubSuffix;
    }

    /// <summary>The bot handling the item, or who it is assigned to when no bot is named.</summary>
    public static string Agent(WorkItem item) => item.Agent ?? item.Assignee ?? WorkItemsText.Missing;

    /// <summary>Who it is assigned to, when that is someone other than the bot; null otherwise.</summary>
    public static string? Assignee(WorkItem item) =>
        item.Assignee is { } who && !string.Equals(who, item.Agent, StringComparison.Ordinal) ? who : null;

    public static string PullLabel(WorkPullRequest pull) =>
        pull.Number is { } number ? "#" + number.ToString(CultureInfo.InvariantCulture) : "Pull request";

    public static string PullTitle(WorkPullRequest pull)
    {
        ArgumentNullException.ThrowIfNull(pull);
        var name = pull.Number is { } number ? $"Pull request #{number.ToString(CultureInfo.InvariantCulture)}" : "Pull request";
        var when = pull.MergedAt is { } merged ? $", merged {TimeText.Iso(merged)}" : string.Empty;
        return name + when + LinkText.GitHubSuffix;
    }

    public static bool IsMerged(string? state) => string.Equals(state, "merged", StringComparison.OrdinalIgnoreCase);

    public static string SessionTitle(int index, int count) =>
        (count == 1 ? "Cursor session" : string.Create(CultureInfo.InvariantCulture, $"Cursor session {index}")) + LinkText.GitHubSuffix;

    public static string SessionLabel(int index, int count) =>
        count == 1 ? "Session" : string.Create(CultureInfo.InvariantCulture, $"Session {index}");

    /// <summary>tdd, then uat, then prod, and only the environments the item has reached.</summary>
    public static IReadOnlyList<WorkPlace> Places(WorkItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var places = new List<WorkPlace>(3);
        Add(places, "tdd", item.Environments.Tdd);
        Add(places, "uat", item.Environments.Uat);
        Add(places, "prod", item.Environments.Prod);
        return places;
    }

    public static string PlaceLabel(WorkPlace place) =>
        place.Version is { } version ? $"{place.Name} {version}" : place.Name;

    public static string PlaceTitle(WorkPlace place, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(place);
        var version = place.Version is { } deployed ? $" as {deployed}" : string.Empty;
        var when = place.DeployedAt is { } at ? $", {TimeText.Ago(at, now)}" : string.Empty;
        return $"Deployed to {place.Name}{version}{when}";
    }

    public static string? CandidateTitle(ReleaseCandidate candidate, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        return candidate.CreatedAt is { } at
            ? $"Octopus release, cut {TimeText.Ago(at, now)}"
            : "Octopus release";
    }

    private static void Add(List<WorkPlace> places, string name, WorkDeployment? deployment)
    {
        if (deployment is not null)
        {
            places.Add(new WorkPlace(name, deployment.Version, deployment.DeployedAt));
        }
    }
}
