namespace Dashboard.Health;

/// <summary>
/// A deployment in the words of the page. The sentences are the fleet's dashboard's, word for word, so a person
/// reads the same on both walls: they name the project and the environment in full.
/// </summary>
public static class DeploymentText
{
    /// <summary>Where a mark's link goes, for its tooltip.</summary>
    public const string TaskTitle = "The deployment's task in Octopus Deploy" + LinkText.OctopusSuffix;

    /// <summary>An age is said in minutes below this many of them.</summary>
    private const int MinutesBeforeHours = 60;

    /// <summary>An age is said in hours below this many of them (two days), then in days.</summary>
    private const int HoursBeforeDays = 48;

    /// <summary>An age is never said as less than a minute.</summary>
    private const int FewestMinutes = 1;

    /// <summary>
    /// <c>deploying cmdemo2-ui 2.4.43 to uat</c>, <c>cmdemo2-ui 2.4.43 is queued for uat</c>, <c>cmdemo2-ui 2.4.43
    /// waits for a sign-off in uat</c>, <c>cmdemo2-ui 2.4.43 reached uat 5 min ago</c> (<c>failed in</c>, <c>was
    /// canceled in</c>); a state the page does not know in the file's own word (<c>cmdemo2-ui 2.4.43 in uat:
    /// paused</c>).
    /// </summary>
    public static string Sentence(Deployment deployment, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(deployment);
        var what = What(deployment.Project, deployment.Release);
        var where = deployment.Environment;
        var ago = deployment.Finished is { } ended ? $" {Ago(now - ended)}" : string.Empty;
        return deployment.State switch
        {
            DeploymentState.Queued => $"{what} is queued for {where}",
            DeploymentState.Executing => $"deploying {what} to {where}",
            DeploymentState.Waiting => DeploymentWait.Asks(deployment.WaitsFor, what, where),
            _ when Ended(deployment.State, what, where, ago) is { } sentence => sentence,
            _ => $"{what} in {where}: {deployment.StateText}",
        };
    }

    /// <summary>
    /// The <c>project release</c> a deployment names: <c>cmdemo2-ui 2.4.43</c>.
    /// </summary>
    internal static string What(string project, string release) => $"{project} {release}";

    /// <summary>
    /// The sentence of a deployment that has ended: <c>cmdemo2-ui 2.4.43 reached uat 5 min ago</c>, <c>cmdemo2-ui 2.4.43
    /// failed in uat 5 min ago</c>, <c>cmdemo2-ui 2.4.43 was canceled in uat 5 min ago</c>. Null for a state the page
    /// does not know.
    /// </summary>
    internal static string? Ended(DeploymentState state, string what, string where, string ago) => state switch
    {
        DeploymentState.Succeeded => $"{what} reached {where}{ago}",
        DeploymentState.Failed => $"{what} failed in {where}{ago}",
        DeploymentState.Canceled => $"{what} was canceled in {where}{ago}",
        _ => null,
    };

    /// <summary>
    /// <c>5 min</c> under an hour and never less than <c>1 min</c>, <c>5 h</c> under two days, then <c>3 d</c>. The
    /// boundaries are the deployment's own; the words of a count of whole units are <see cref="TimeText"/>'s.
    /// </summary>
    public static string Age(TimeSpan span)
    {
        if (span.TotalMinutes < MinutesBeforeHours)
        {
            return TimeText.Whole(Math.Max(FewestMinutes, span.TotalMinutes), TimeText.MinutesUnit);
        }

        return span.TotalHours < HoursBeforeDays
            ? TimeText.Whole(span.TotalHours, TimeText.HoursUnit)
            : TimeText.Whole(span.TotalDays, TimeText.DaysUnit);
    }

    /// <summary>An age that has ended: <c>5 min ago</c>.</summary>
    public static string Ago(TimeSpan span) => TimeText.Before(Age(span));

    /// <summary>An age still running, for a deployment in flight: <c>3 min so far</c>.</summary>
    public static string SoFar(int minutes) => $"{Age(TimeSpan.FromMinutes(minutes))} so far";

    /// <summary>
    /// The sentence, and for a deployment in flight for how long it has been so (<c>deploying cmdemo2-ui 2.4.43 to
    /// uat (3 min so far)</c>): a mark's tooltip. Where the file says more (<see cref="DeploymentMark.Detail"/>), that
    /// follows on a line of its own.
    /// </summary>
    public static string Title(DeploymentMark mark)
    {
        ArgumentNullException.ThrowIfNull(mark);
        var sentence = mark is { InFlight: true, Minutes: { } minutes } ? $"{mark.Sentence} ({SoFar(minutes)})" : mark.Sentence;
        return mark.Detail is { } detail ? $"{sentence}\n{detail}" : sentence;
    }

    /// <summary>
    /// What the file says about a deployment beyond its state: <c>the sign-off is with cmdemo2 approvers; asked 2 h
    /// ago; started by jeffrey</c>. A title Octopus Deploy gives the question is quoted when it says more than the
    /// kind does. Null when the file says none of it.
    /// </summary>
    public static string? Detail(Deployment deployment, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(deployment);
        var parts = new List<string>();
        if (deployment is { State: DeploymentState.Waiting, WaitsFor: { } wait })
        {
            if (wait.Responsible is { } responsible)
            {
                parts.Add($"{wait.Question()} is with {responsible}");
            }
            else if (wait.Title is not null && !wait.TitleIsTheKind)
            {
                parts.Add(wait.Question());
            }

            if (wait.Since is { } asked)
            {
                parts.Add($"asked {Ago(now - asked)}");
            }
        }

        if (deployment.StartedBy is { } startedBy)
        {
            parts.Add($"started by {startedBy}");
        }

        return TooltipClauses.Join(parts);
    }

    /// <summary>The shape of a mark's dot that is executing: filled, and pulsing where motion is welcome.</summary>
    public const string ExecutingShape = "executing";

    /// <summary>The shape of a mark's dot that is queued: hollow.</summary>
    public const string QueuedShape = "queued";

    /// <summary>The shape of a mark's dot that is waiting for a person: a dot in a ring.</summary>
    public const string WaitingShape = "waiting";

    /// <summary>
    /// The shape of a mark's dot that ended: small and still; also the shape of a state the page does not know.
    /// </summary>
    public const string EndedShape = "ended";

    /// <summary>
    /// The shape of a mark's dot, as the word its class and the runtime payload carry: <see cref="ExecutingShape"/>,
    /// <see cref="QueuedShape"/>, <see cref="WaitingShape"/> and <see cref="EndedShape"/>.
    /// </summary>
    public static string Shape(DeploymentState state) => state switch
    {
        DeploymentState.Executing => ExecutingShape,
        DeploymentState.Queued => QueuedShape,
        DeploymentState.Waiting => WaitingShape,
        _ => EndedShape,
    };

    /// <summary>What the marks are and how old, for the tooltip of a list of them.</summary>
    public static string Help(DateTimeOffset? generated, TimeZoneInfo zone)
    {
        ArgumentNullException.ThrowIfNull(zone);
        var asOf = generated is { } at ? $", as of {TimeText.DateAndTime(at, zone)}" : string.Empty;
        return $"Not live: what the system itself read from Octopus Deploy{asOf}. A deployment that started shows within about one to six minutes "
            + "and its end as soon, one that is only queued within half an hour; one that ended stays marked for ten minutes.";
    }
}
