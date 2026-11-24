using System.Text.Json;

namespace Dashboard.Health;

/// <summary>
/// What is being deployed: the content of the file at <c>system.deploymentsUrl</c> of the topology
/// (<c>deployments.json</c>, which a workflow of the system repository publishes). The system reads Octopus Deploy
/// once for all of it, and no application reports its own deployment: the deployments that are queued, executing or
/// waiting for a person, and those that ended lately, whatever the environment. Minutes old, never live.
/// </summary>
/// <param name="Generated">When the system read Octopus Deploy. It is not the time of a check.</param>
/// <param name="Deployments">The deployments, in the file's order: in flight first.</param>
public sealed record DeploymentsReport(DateTimeOffset? Generated, IReadOnlyList<Deployment> Deployments)
{
    /// <summary>
    /// The last deployment that ended of each project in each environment (<c>recent</c>), the last one first; empty
    /// for a file from before the field.
    /// </summary>
    public IReadOnlyList<RecentDeployment> Recent { get; init; } = [];

    /// <summary>
    /// The deployment freezes that cover the system now or soon (<c>freezes</c>); empty for a file from before the
    /// field, and when the system could not read them (<see cref="Missing"/> then names <c>freezes</c>).
    /// </summary>
    public IReadOnlyList<DeploymentFreeze> Freezes { get; init; } = [];

    private static readonly string[] ActivityLists = ["recent", "freezes", "missing"];

    /// <summary>The parts the system could not read from Octopus Deploy (<c>missing</c>), in the file's words.</summary>
    public IReadOnlyList<string> Missing { get; init; } = [];

    /// <summary>
    /// True for a file that has one of the lists <c>recent</c>, <c>freezes</c> or <c>missing</c>: the system writes
    /// what the page says in words. A file from before them says none of it, and the page then draws what it drew
    /// before: the marks, and no activity line.
    /// </summary>
    public bool SaysActivity { get; init; }

    /// <summary>
    /// How long a deployment that ended stays marked: one of a few minutes can end before the page saw it start.
    /// </summary>
    public static readonly TimeSpan EndedFor = TimeSpan.FromMinutes(10);

    /// <summary>
    /// What is marked in an environment at <paramref name="now"/>: every deployment in flight, and one that ended
    /// until <see cref="EndedFor"/> after it did. The one a person has to act on comes first (waiting), then
    /// executing and queued, then what ended: failed, canceled, succeeded.
    /// </summary>
    /// <param name="slug">
    /// The system's slug: the Octopus project of a deployable is <c>&lt;slug&gt;-&lt;deployable&gt;</c>, and
    /// <c>&lt;slug&gt;-system</c> is the system's own.
    /// </param>
    /// <param name="deployables">
    /// The deployables a mark may belong to: those of the topology's environment, or those a diagram draws a node of.
    /// </param>
    public IReadOnlyList<DeploymentMark> Marks(string environment, string slug, IEnumerable<string> deployables, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(slug);
        ArgumentNullException.ThrowIfNull(deployables);
        var known = deployables.ToList();
        return [.. Deployments
            .Where(deployment => string.Equals(deployment.Environment, environment, StringComparison.OrdinalIgnoreCase) && deployment.IsMarkedAt(now))
            .OrderBy(Rank)
            .Select(deployment => DeploymentMark.Of(deployment, slug, known, now))];
    }

    /// <summary>
    /// The file's content; null when it is not a JSON object with a <c>deployments</c> list. An empty list is a
    /// report: nothing is being deployed.
    /// </summary>
    public static DeploymentsReport? Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            return root.ValueKind == JsonValueKind.Object && root.TryGetProperty("deployments", out var list) && list.ValueKind == JsonValueKind.Array
                ? new DeploymentsReport(JsonRead.Time(root, "generated"), [.. JsonRead.Items(root, "deployments").Select(Deployment.Read).OfType<Deployment>()])
                {
                    Recent = ReadRecent(root),
                    Freezes = ReadFreezes(root),
                    Missing = ReadMissing(root),
                    SaysActivity = ReadSaysActivity(root),
                }
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>The last deployments that ended, newest first; empty for a file from before the field.</summary>
    private static IReadOnlyList<RecentDeployment> ReadRecent(JsonElement root) =>
        [.. JsonRead.Items(root, "recent").Select(RecentDeployment.Read).OfType<RecentDeployment>().OrderByDescending(recent => recent.Finished)];

    /// <summary>The deployment freezes, in the file's order; empty for a file from before the field.</summary>
    private static IReadOnlyList<DeploymentFreeze> ReadFreezes(JsonElement root) =>
        [.. JsonRead.Items(root, "freezes").Select(DeploymentFreeze.Read).OfType<DeploymentFreeze>()];

    /// <summary>The parts the system could not read, in the file's words; empty for a file from before the field.</summary>
    private static IReadOnlyList<string> ReadMissing(JsonElement root) =>
        [.. JsonRead.Items(root, "missing").Where(item => item.ValueKind == JsonValueKind.String).Select(item => item.GetString()!)];

    /// <summary>True when the file has one of the activity lists: recent, freezes or missing.</summary>
    private static bool ReadSaysActivity(JsonElement root) => ActivityLists.Any(name => JsonRead.HasArray(root, name));

    /// <summary>Where a deployment stands among the marks: a <see cref="MarkPlace"/>, which is a place in the marks and
    /// not a number of <see cref="DeploymentState"/>.</summary>
    internal static MarkPlace Rank(Deployment deployment) => deployment.State switch
    {
        DeploymentState.Waiting => MarkPlace.Waiting,
        DeploymentState.Executing => MarkPlace.Executing,
        DeploymentState.Queued => MarkPlace.Queued,
        DeploymentState.Failed => MarkPlace.Failed,
        DeploymentState.Canceled => MarkPlace.Canceled,
        DeploymentState.Succeeded => MarkPlace.Succeeded,
        _ => deployment.InFlight ? MarkPlace.UnknownInFlight : MarkPlace.UnknownEnded,
    };
}

/// <summary>
/// The order of the marks, first to last: what a person has to act on, then what runs, then what ended. A state the
/// page does not know stands after the ones it knows on its side of the boundary: after queued while the file gives
/// it no end, after succeeded once it does. Adding a state is putting it where it belongs.
/// <para>
/// These places are a numbering of the marks and nothing else. They are not the numbers of
/// <see cref="DeploymentState"/>, which orders the same states its own way and is not consulted here: the enum's
/// <see cref="DeploymentState.Waiting"/> is the fourth of the seven and stands first among the marks, and its
/// <see cref="DeploymentState.Unknown"/> is the first of the seven and stands twice among them - <see
/// cref="UnknownInFlight"/> while the file gives no end, <see cref="UnknownEnded"/> once it does. Nothing converts
/// between the two: a state is matched by name to the place it belongs to.
/// </para>
/// </summary>
internal enum MarkPlace
{
    // In flight.
    Waiting,
    Executing,
    Queued,
    UnknownInFlight,

    // Ended.
    Failed,
    Canceled,
    Succeeded,
    UnknownEnded,
}
