using Dashboard.Runtime;

namespace Dashboard.Health;

/// <summary>
/// The requests the traffic button sends in one environment: every deployable's representative paths
/// (<c>trafficPaths</c> of the topology; the start page without them) at its public address, which is the Front Door
/// endpoint where there is one and the primary web app's own address otherwise. So the calls take the path a user's
/// calls take: browser, Front Door, the origin that serves, the database.
///
/// The three numbers of a press - what a press sends - are declared on the run itself, in
/// <see cref="TrafficRun.Values"/> (#1777), and the run is the only way a caller gets them. What the plan keeps of
/// them: <see cref="AnswerWithinSeconds"/>, the const the shared-wait guard pins here, naming the wait the run
/// declares, with its duration <see cref="AnswerWithin"/> beside it the way <see cref="CheckEverySeconds"/> has
/// <see cref="CheckEvery"/>.
/// <see cref="CheckEverySeconds"/> is none of them, being how often the page checks while a run lasts - the plan's
/// own rule, read by <see cref="IntervalNote"/>.
/// </summary>
public sealed record TrafficPlan(string Environment, IReadOnlyList<Uri> Addresses, IReadOnlyList<string> Targets)
{
    public static readonly IReadOnlyList<string> DefaultPaths = ["/"];

    /// <summary>
    /// How long a request waits for its answer before the browser gives it up: as long as a check of the page
    /// waits (the shared <see cref="ProbeDefaults.DefaultTimeoutSeconds"/>). The number is declared on the run the
    /// callers read it from (<see cref="TrafficRun.Values.AnswerWithinSeconds"/>); this const names the same shared
    /// wait, where the guard that pinned it stands.
    /// </summary>
    public const int AnswerWithinSeconds = TrafficRun.Values.AnswerWithinSeconds;

    /// <summary>
    /// That same wait as a duration: the shared probe wait, <see cref="ProbeDefaults.DefaultTimeout"/>, the length
    /// <see cref="AnswerWithinSeconds"/> is made of - the shape <see cref="CheckEvery"/> has beside
    /// <see cref="CheckEverySeconds"/>. The one home of the rule a caller that holds a length reads: the sentence of
    /// <see cref="TrafficCount"/> says a request given up over it, and the words still read the whole seconds - so
    /// the wait is never rebuilt from its seconds wherever it is needed (#1795).
    /// </summary>
    public static readonly TimeSpan AnswerWithin = ProbeDefaults.DefaultTimeout;

    /// <summary>
    /// How often the page checks while the traffic runs, so the numbers follow it, in whole seconds: the number
    /// <see cref="CheckEvery"/> is made of. Not a number of the run: a run says what the browser sends
    /// (<see cref="TrafficRun"/>), this says what the page does while it sends.
    /// </summary>
    public const int CheckEverySeconds = 10;

    /// <summary>
    /// That same check as a duration: <see cref="CheckEverySeconds"/> seconds. The one home of the rule a caller that
    /// holds a length reads - <see cref="IntervalNote"/> compares the interval control with it, and the panel cuts its
    /// follow tick out of it (<c>TrafficPanel.ReadEvery</c>) - so the rhythm is never re-derived from the seconds
    /// wherever it is needed, the shape <see cref="ProbeDefaults"/> gives
    /// <see cref="ProbeDefaults.DefaultTimeoutSeconds"/> and <see cref="ProbeDefaults.DefaultTimeout"/>.
    /// </summary>
    public static readonly TimeSpan CheckEvery = TimeSpan.FromSeconds(CheckEverySeconds);

    /// <summary>
    /// The environment the panel's choice shows, among those that have something to call. The viewer's own choice
    /// stays, and so does everything while traffic runs; otherwise the choice follows the environment the page shows
    /// (the runtime view's), and is the first one where the page shows none. Null when no environment has a plan.
    /// </summary>
    /// <param name="current">What the choice shows now; null before the first decision.</param>
    /// <param name="shown">The environment the page shows, or last showed; null while it has shown none.</param>
    /// <param name="chosenByViewer">True once the viewer picked <paramref name="current"/> in the panel.</param>
    public static string? Choose(IReadOnlyList<TrafficPlan> plans, string? current, string? shown, bool chosenByViewer, bool running)
    {
        ArgumentNullException.ThrowIfNull(plans);
        string? Known(string? name) =>
            name is null ? null : plans.FirstOrDefault(plan => new EnvironmentName(plan.Environment).Matches(name))?.Environment;

        if (running)
        {
            return current;
        }

        return (chosenByViewer ? Known(current) : null) ?? Known(shown) ?? (plans.Count > 0 ? plans[0].Environment : null);
    }

    /// <summary>
    /// What stands next to the interval control while traffic runs: the page then checks every
    /// <see cref="CheckEvery"/>, whatever the control says. Null when no traffic runs, and when the chosen interval is
    /// no longer than that (the control is then right as it is). The comparison is the duration's, and only the words
    /// are read from <see cref="CheckEverySeconds"/>, which is how a sentence is written.
    /// </summary>
    public static string? IntervalNote(bool running, TimeSpan interval) =>
        running && interval > CheckEvery ? $"every {CheckEverySeconds} s while traffic runs" : null;

    /// <summary>The plan of an environment; null when it has nothing to call.</summary>
    public static TrafficPlan? For(EnvironmentInfo environment)
    {
        ArgumentNullException.ThrowIfNull(environment);
        var addresses = new List<Uri>();
        var targets = new List<string>();
        foreach (var deployable in environment.Deployables)
        {
            if (PublicAddressOf(deployable) is not { } publicAddress || PathsOf(deployable) is not { } paths)
            {
                continue;
            }

            targets.Add($"{deployable.Name} at {publicAddress.Host}");
            addresses.AddRange(paths.Select(path => ProbeUrl.Combine(publicAddress, path)));
        }

        return addresses.Count == 0 ? null : new TrafficPlan(environment.Name, addresses, targets);
    }

    /// <summary>
    /// Where a user reaches a deployable: the Front Door endpoint where there is one, the primary node's own address
    /// otherwise, and the first node's where none is primary. Null when it has neither Front Door nor nodes.
    /// </summary>
    private static Uri? PublicAddressOf(DeployableInfo deployable) =>
        deployable.FrontDoor
        ?? deployable.Nodes.FirstOrDefault(node => node.IsPrimary)?.Url
        ?? (deployable.Nodes.Count > 0 ? deployable.Nodes[0].Url : null);

    /// <summary>
    /// The paths the traffic button calls on a deployable: its traffic paths, or the start page without the key. Null
    /// when the list is empty, which is said on purpose: a deployable that takes no generated traffic (a dashboard the
    /// topology lists as a node).
    /// </summary>
    private static IReadOnlyList<string>? PathsOf(DeployableInfo deployable) =>
        deployable.TrafficPaths switch
        {
            null => DefaultPaths,
            { Count: 0 } => null,
            var configured => configured,
        };
}
