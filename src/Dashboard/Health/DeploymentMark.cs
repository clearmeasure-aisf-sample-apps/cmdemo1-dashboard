namespace Dashboard.Health;

/// <summary>
/// One deployment as the page marks it: on the environment it deploys to, and on the tiles and nodes of its
/// deployable when it has one.
/// </summary>
/// <param name="Project">The Octopus project.</param>
/// <param name="Deployable">
/// The deployable the project deploys. Null for the system's own project (<paramref name="OfSystem"/>), which belongs
/// to the environment as a whole and to no application, and for a project that is none of the deployables asked for.
/// </param>
/// <param name="Sentence">The mark in words: the same as on the fleet's dashboard.</param>
/// <param name="Minutes">
/// Whole minutes since it started (or was queued) while it is in flight, since it ended otherwise; null when the
/// file's time does not parse.
/// </param>
/// <param name="Url">The task in Octopus Deploy; null when the file gives no address.</param>
/// <param name="InFlight">True while it has not ended.</param>
/// <param name="OfSystem">True for the system's own release: its infrastructure and configuration.</param>
/// <param name="Detail">
/// What the file says beyond the state, after the sentence: who is responsible for what it waits for and since when,
/// and who started it. Null where the file says none of it (a file from before the fields).
/// </param>
public sealed record DeploymentMark(
    string Project,
    string? Deployable,
    string Environment,
    string Release,
    DeploymentState State,
    string Sentence,
    int? Minutes,
    Uri? Url,
    bool InFlight,
    bool OfSystem,
    string? Detail = null)
{
    internal static DeploymentMark Of(Deployment deployment, string slug, IReadOnlyList<string> deployables, DateTimeOffset now)
    {
        var name = NameOf(deployment.Project, slug);
        var system = name is not null && DeliveryReport.IsSystem(name);
        return new DeploymentMark(
            deployment.Project,
            system ? null : deployables.FirstOrDefault(deployable => string.Equals(deployable, name, StringComparison.OrdinalIgnoreCase)),
            deployment.Environment,
            deployment.Release,
            deployment.State,
            DeploymentText.Sentence(deployment, now),
            (deployment.InFlight ? deployment.Since : deployment.Finished) is { } then ? (int)Math.Max(0, (now - then).TotalMinutes) : null,
            deployment.Url,
            deployment.InFlight,
            system,
            DeploymentText.Detail(deployment, now));
    }

    /// <summary>
    /// What a project is called after the system's slug (<c>ui</c> of <c>cmdemo2-ui</c>); null for a project that is
    /// not named so.
    /// </summary>
    public static string? NameOf(string project, string slug)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(slug);
        return slug.Length > 0 && project.Length > slug.Length + 1 && project.StartsWith($"{slug}-", StringComparison.OrdinalIgnoreCase)
            ? project[(slug.Length + 1)..]
            : null;
    }
}
