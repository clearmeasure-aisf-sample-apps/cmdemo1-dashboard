using Dashboard.Health;

namespace Dashboard.Cluster;

/// <summary>The states of the cluster view: the page's vocabulary, with a warning and a neutral state next to it.</summary>
public enum ClusterState
{
    /// <summary>Not read yet, or something is starting: the dots.</summary>
    Checking,

    /// <summary>The check mark.</summary>
    Healthy,

    /// <summary>Worth a look, in words: the warning triangle, without the wash of Unhealthy.</summary>
    Warning,

    /// <summary>An answer that says something is wrong: the warning triangle.</summary>
    Unhealthy,

    /// <summary>No answer, or one that is too old to count: the cross.</summary>
    Unreachable,

    /// <summary>Neither good nor bad: stopped on purpose, finished, or not known. The bar.</summary>
    Neutral,
}

/// <summary>What the view says about one pod.</summary>
public enum PodState
{
    /// <summary>Running, and every container is ready.</summary>
    Ready,

    /// <summary>Not ready yet, and young enough for that to be normal.</summary>
    Starting,

    /// <summary>Failed, pending for too long, or running without being ready.</summary>
    Unhealthy,

    /// <summary>A job that ran to its end (phase Succeeded): not a problem, and not counted.</summary>
    Finished,
}

/// <summary>Whether the cluster's own status reaches the page.</summary>
public enum ClusterLiveness
{
    /// <summary>Not read yet.</summary>
    Pending,

    /// <summary>The file answers and the collector wrote it recently.</summary>
    Live,

    /// <summary>The file answers, but the collector wrote it too long ago.</summary>
    Stale,

    /// <summary>The address does not answer, or answers without the file.</summary>
    Silent,

    /// <summary>The address answers something that is not the file.</summary>
    Unreadable,
}

/// <summary>The rules that turn a pod's phase, readiness and age into its state, and the one its container counts must keep.</summary>
public static class PodRules
{
    /// <summary>A pod may be Pending this long before it counts as unhealthy.</summary>
    public static readonly TimeSpan PendingGrace = TimeSpan.FromMinutes(5);

    /// <summary>A running pod may be not ready this long before it counts as unhealthy.</summary>
    public static readonly TimeSpan StartingGrace = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Finished for the phase Succeeded. Unhealthy for Failed, for Pending longer than <see cref="PendingGrace"/> and
    /// for Running without being ready, unless the pod is younger than <see cref="StartingGrace"/>: then it is
    /// starting, and so is a Pending pod whose start is not known. A pod in a phase the page does not know is ready
    /// when it says so and unhealthy otherwise.
    /// </summary>
    /// <param name="asOf">The moment the facts are of (<see cref="ClusterStatus.AsOf"/>): the pod's age is counted from its start to it.</param>
    public static PodState StateOf(ClusterPod pod, DateTimeOffset asOf)
    {
        ArgumentNullException.ThrowIfNull(pod);
        var phase = new PodPhase(pod.Phase);
        var age = pod.StartedAt is { } started ? asOf - started : (TimeSpan?)null;
        return phase.Kind switch
        {
            PodPhaseKind.Succeeded => PodState.Finished,
            PodPhaseKind.Failed => PodState.Unhealthy,
            PodPhaseKind.Pending => age > PendingGrace ? PodState.Unhealthy : PodState.Starting,
            _ when pod.Ready => PodState.Ready,
            PodPhaseKind.Running => age < StartingGrace ? PodState.Starting : PodState.Unhealthy,
            _ => PodState.Unhealthy,
        };
    }

    /// <summary>
    /// True when the file says something impossible of the pod's containers: more of them ready
    /// (<see cref="ClusterPod.ContainersReady"/>) than the pod has (<see cref="ClusterPod.Containers"/>). A pod that
    /// does not say either number says nothing impossible. The pod still holds the numbers as the file gave them;
    /// this is the one place that names the rule they break.
    /// </summary>
    public static bool ReadyOutnumbersContainers(ClusterPod pod)
    {
        ArgumentNullException.ThrowIfNull(pod);
        return pod.ContainersReady is { } ready && pod.Containers is { } all && ready > all;
    }
}

/// <summary>A state of the cluster view with its words: the badge's label, the headline and what stands under it.</summary>
/// <param name="Label">The word of the state's badge.</param>
/// <param name="Headline">One sentence, without its full stop.</param>
/// <param name="Detail">What stands under the headline, in whole sentences; null without any.</param>
public sealed record ClusterAssessment(ClusterState State, string Label, string Headline, string? Detail = null)
{
    /// <summary>How many problems a headline names before it counts the rest.</summary>
    public const int NamedProblems = 3;

    /// <summary>The separator between the named problems of a list, and before its <c>and N more</c> tail: <c>; </c>.</summary>
    private const string ProblemSeparator = "; ";

    // The headline words this file says twice, named once each (#2048): a word is one word here, so a typo at a call
    // site cannot silently split the page's vocabulary. The cluster card and the AKS service card sit side by side
    // on the cluster view, and both say these words; the const name carries the suffix because the private methods
    // that build the other copy of each word keep the plain name (Unreadable, Stopped).

    /// <summary>The headline the cluster card and the AKS service card both say while their reading is still pending.</summary>
    private const string CheckingHeadline = "Checking";

    /// <summary>The headline the cluster card says when the status file is unreadable and the AKS service card says when its facts are malformed.</summary>
    private const string UnreadableHeadline = "Unreadable";

    /// <summary>The headline the cluster card says when the file is silent and Azure says stopped, and the AKS service card says when Azure reports the service stopped.</summary>
    private const string StoppedHeadline = "Stopped";

    // The AKS-service sentences the cluster card and the AKS service card both say, named once each (#2163): the same
    // words as the headlines above, so a rewording here cannot silently split the page's vocabulary.

    /// <summary>The words both cards say when Azure's facts are not published yet.</summary>
    private const string NotPublished = "Azure's facts about the AKS service are not published yet";

    /// <summary>The words both cards say when Azure's facts could not be read.</summary>
    private const string NotRead = "Azure's facts about the AKS service could not be read";

    /// <summary>The lead of the sentence both cards say when Azure reports a verdict, with the space the verdict follows.</summary>
    private const string ReportsLead = "Azure reports the AKS service ";

    /// <summary>
    /// The label of the AKS service card when its facts are not known: the reading that could not be read, or the
    /// reading that is read and holds nothing. Read from <see cref="HealthDetailText.NotKnown" /> (#2055) so the word
    /// the Resource Health box and the runtime tiles say cannot drift from the word this card says.
    /// </summary>
    private const string NotKnownLabel = HealthDetailText.NotKnown;

    /// <summary>Whether the cluster's own status reaches the page, at the page's clock.</summary>
    public static ClusterLiveness LivenessOf(SourceReading<ClusterStatus> reading, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(reading);
        return reading.State switch
        {
            SourceState.Pending => ClusterLiveness.Pending,
            SourceState.Read when reading.Value is { } status => status.IsStale(now) ? ClusterLiveness.Stale : ClusterLiveness.Live,
            SourceState.Malformed => ClusterLiveness.Unreadable,
            _ => ClusterLiveness.Silent,
        };
    }

    /// <summary>
    /// The state of the cluster as it reports itself, next to what Azure says where the two can differ:
    /// healthy when every node is ready under no pressure and every pod that is not finished is ready; starting while
    /// the only pods that are not ready are starting; unhealthy otherwise, with what is wrong. A file the collector
    /// wrote too long ago is stale. A file that does not answer is the cluster stopped when Azure says so, and
    /// unreachable otherwise.
    /// </summary>
    /// <param name="service">Azure's facts about the AKS service; null when the topology names no address for them.</param>
    public static ClusterAssessment Live(SourceReading<ClusterStatus> reading, SourceReading<AksService>? service, DateTimeOffset now, TimeZoneInfo zone)
    {
        ArgumentNullException.ThrowIfNull(reading);
        switch (LivenessOf(reading, now))
        {
            case ClusterLiveness.Pending:
                return new ClusterAssessment(ClusterState.Checking, CheckingHeadline, "Reading the cluster's status file");
            case ClusterLiveness.Unreadable:
                return Unreadable(reading, service, now, zone);
            case ClusterLiveness.Silent when service is { State: SourceState.Read, Value: { IsStopped: true } stopped }:
                return Stopped(stopped, now, zone);
            case ClusterLiveness.Silent:
                return Unreachable(reading, service, now, zone);
        }

        var status = reading.Value!;
        if (status.IsStale(now) && status.Generated is { } generated)
        {
            return Stale(generated, service, now, zone);
        }

        return FromStatus(status, now, zone);
    }

    /// <summary>The file answers but not with the status: what is wrong, next to what Azure says.</summary>
    private static ClusterAssessment Unreadable(SourceReading<ClusterStatus> reading, SourceReading<AksService>? service, DateTimeOffset now, TimeZoneInfo zone)
    {
        return new ClusterAssessment(
            ClusterState.Unhealthy,
            UnreadableHeadline,
            "The cluster's status file could not be read",
            AzureAndUs(Sentence(reading.Detail), service, now, zone));
    }

    /// <summary>The file does not answer and Azure says the cluster is stopped: the cluster is stopped.</summary>
    private static ClusterAssessment Stopped(AksService stopped, DateTimeOffset now, TimeZoneInfo zone)
    {
        return new ClusterAssessment(
            ClusterState.Neutral,
            StoppedHeadline,
            "The cluster is stopped",
            Join(
                "The pods and the pages served from inside the cluster do not run while it is stopped.",
                stopped.Generated is { } read ? $"Azure reports the power state Stopped, {ClusterText.AsOf(read, now, zone)}." : null));
    }

    /// <summary>The file does not answer and Azure does not say stopped: the cluster is unreachable.</summary>
    private static ClusterAssessment Unreachable(SourceReading<ClusterStatus> reading, SourceReading<AksService>? service, DateTimeOffset now, TimeZoneInfo zone)
    {
        return new ClusterAssessment(
            ClusterState.Unreachable,
            "Unreachable",
            "The cluster's status file does not answer",
            AzureAndUs(Sentence(reading.Detail), service, now, zone));
    }

    /// <summary>The file answers but the collector wrote it too long ago: stale, with what Azure says.</summary>
    private static ClusterAssessment Stale(DateTimeOffset generated, SourceReading<AksService>? service, DateTimeOffset now, TimeZoneInfo zone)
    {
        return new ClusterAssessment(
            ClusterState.Unreachable,
            "Stale",
            $"The collector in the cluster last wrote {TimeText.Clock(generated, zone)} ({TimeText.Ago(generated, now)})",
            AzureAndUs("What is shown below is as of then.", service, now, zone));
    }

    /// <summary>What the status itself reports: problems, starting pods, or healthy.</summary>
    private static ClusterAssessment FromStatus(ClusterStatus status, DateTimeOffset now, TimeZoneInfo zone)
    {
        var asOf = status.AsOf(now);
        var problems = Problems(status, asOf);
        if (problems.Count > 0)
        {
            return new ClusterAssessment(ClusterState.Unhealthy, "Unhealthy", Named(problems));
        }

        var starting = status.Pods
            .Where(entry => PodRules.StateOf(entry.Pod, asOf) == PodState.Starting)
            .Select(entry => ClusterText.PodIn(entry.Pod, entry.Namespace.Name))
            .ToList();
        return starting.Count switch
        {
            0 => new ClusterAssessment(ClusterState.Healthy, "Healthy", "Every node is ready and under no pressure, and every pod is ready"),
            var count => new ClusterAssessment(
                ClusterState.Checking,
                "Starting",
                $"{Plural.Of(count, "pod")} {Plural.Agreement(count, "is", "are")} starting: {Named(starting)}"),
        };
    }

    /// <summary>What the file says, then what Azure says: the join every silent branch needs.</summary>
    private static string? AzureAndUs(string? us, SourceReading<AksService>? service, DateTimeOffset now, TimeZoneInfo zone)
    {
        return Join(us, AzureReports(service, now, zone));
    }

    /// <summary>
    /// The state of the AKS service as Azure reports it: healthy when it is Available, Running and Succeeded; neutral
    /// when it is stopped or Azure has no verdict; a warning when it is Degraded or being changed; unhealthy when it
    /// is Unavailable. A file that is not published yet is said calmly: it is no error of the page. A reading marked
    /// read that holds no facts is not known, as one that could not be read.
    /// </summary>
    public static ClusterAssessment Service(SourceReading<AksService> reading)
    {
        ArgumentNullException.ThrowIfNull(reading);
        const string Again = "The page reads them again with every check.";
        return reading.State switch
        {
            SourceState.Pending => new ClusterAssessment(ClusterState.Checking, CheckingHeadline, "Reading Azure's facts about the AKS service"),
            SourceState.Missing => new ClusterAssessment(
                ClusterState.Neutral,
                "Not published",
                NotPublished,
                $"A scheduled workflow publishes them several times an hour; until its first run the address answers HTTP 404. {Again}"),
            SourceState.Malformed => new ClusterAssessment(ClusterState.Warning, UnreadableHeadline, NotRead, Sentence(reading.Detail)),
            SourceState.Read when reading.Value is { } service => Reported(service),
            _ => new ClusterAssessment(ClusterState.Neutral, NotKnownLabel, NotRead, Join(Sentence(reading.Detail), Again)),
        };
    }

    /// <summary>The state of the AKS service in the facts Azure reports about it.</summary>
    private static ClusterAssessment Reported(AksService service)
    {
        var verdict = new AzureState(service.Availability?.State);
        var summary = service.Availability?.Summary;
        if (service.IsStopped)
        {
            return new ClusterAssessment(ClusterState.Neutral, StoppedHeadline, "The AKS service is stopped", summary);
        }

        if (verdict.Is(AksAvailability.Unavailable))
        {
            return new ClusterAssessment(ClusterState.Unhealthy, AksAvailability.Unavailable, $"{ReportsLead}unavailable", summary);
        }

        if (verdict.Is(AksAvailability.Degraded))
        {
            return new ClusterAssessment(ClusterState.Warning, AksAvailability.Degraded, $"{ReportsLead}degraded", summary);
        }

        if (service.ProvisioningState is { } provisioning && !service.IsProvisioned)
        {
            return new ClusterAssessment(
                ClusterState.Warning,
                provisioning,
                $"The provisioning state of the AKS service is {provisioning}, not {AksService.Succeeded}",
                summary);
        }

        if (verdict.IsMissing || verdict.Is(AksAvailability.Unknown))
        {
            return new ClusterAssessment(ClusterState.Neutral, AksAvailability.Unknown, "Azure Resource Health has no verdict on the AKS service at the moment", summary);
        }

        return verdict.Is(AksAvailability.Available)
            ? new ClusterAssessment(ClusterState.Healthy, AksAvailability.Available, $"{ReportsLead}available and running", summary)
            : new ClusterAssessment(ClusterState.Neutral, verdict.ToString(), $"{ReportsLead}{verdict}", summary);
    }

    /// <summary>
    /// What is wrong in the cluster, most severe first: nodes that are not ready, nodes under pressure, failed pods,
    /// running pods that are not ready (the most restarts first), pods pending for too long, then any other.
    /// </summary>
    public static IReadOnlyList<string> Problems(ClusterStatus status, DateTimeOffset asOf)
    {
        ArgumentNullException.ThrowIfNull(status);
        var problems = new List<string>();
        if (status.Nodes.Count == 0)
        {
            problems.Add("The status file lists no node");
        }

        problems.AddRange(status.Nodes.Where(node => !node.Ready).Select(node => $"{ClusterText.NodeIn(node.Name)} is not ready"));
        problems.AddRange(status.Nodes
            .Where(node => node.Ready && node.Pressures.Count > 0)
            .Select(node => $"{ClusterText.NodeIn(node.Name)}: {ClusterText.Pressures(node.Pressures)}"));
        problems.AddRange(status.Pods
            .Where(entry => PodRules.StateOf(entry.Pod, asOf) == PodState.Unhealthy)
            .OrderBy(entry => Severity(entry.Pod))
            .ThenByDescending(entry => entry.Pod.Restarts)
            .Select(entry => PodProblem(entry.Pod, entry.Namespace.Name, asOf)));
        return problems;
    }

    /// <summary><c>ui in cmdemo3-tdd: CrashLoopBackOff, 7 restarts</c>.</summary>
    public static string PodProblem(ClusterPod pod, string space, DateTimeOffset asOf)
    {
        ArgumentNullException.ThrowIfNull(pod);
        var words = ClusterText.PodWords(pod, PodState.Unhealthy, asOf);
        var text = $"{ClusterText.PodIn(pod, space)}: {(new PodPhase(pod.Phase).Is(PodPhaseKind.Running) && pod.Reason is null ? "not ready" : words)}";
        return pod.Restarts > 0 ? $"{text}, {ClusterText.Restarts(pod.Restarts)}" : text;
    }

    /// <summary>The first <see cref="NamedProblems"/> of a list, then <c>and N more</c>.</summary>
    public static string Named(IReadOnlyList<string> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var named = string.Join(ProblemSeparator, NamedList.Named(items, NamedProblems));
        return NamedList.More(items.Count, NamedProblems) is { } more
            ? $"{named}{ProblemSeparator}and {more}"
            : named;
    }

    /// <summary>
    /// What Azure says, for the place where the cluster itself says nothing: <c>Azure reports the AKS service Available
    /// and Running, as of 15:10:04 (5 min ago).</c> Null when the topology names no address for Azure's facts, and
    /// before they were read.
    /// </summary>
    public static string? AzureReports(SourceReading<AksService>? service, DateTimeOffset now, TimeZoneInfo zone)
    {
        switch (service?.State)
        {
            case null or SourceState.Pending:
                return null;
            case SourceState.Missing:
                return $"{NotPublished}.";
            case SourceState.Read when service.Value is { } facts:
                var states = new[] { facts.Availability?.State, facts.PowerState }.OfType<string>().ToList();
                var said = states.Count > 0 ? string.Join(" and ", states) : facts.ProvisioningState;
                var asOf = facts.Generated is { } generated ? $", {ClusterText.AsOf(generated, now, zone)}" : string.Empty;
                return said is null ? null : $"{ReportsLead}{said}{asOf}.";
            default:
                return $"{NotRead} either.";
        }
    }

    /// <summary>A reading's reason (the end of a sentence) as a sentence of its own.</summary>
    internal static string? Sentence(string? detail)
    {
        var words = ClusterText.Words(detail);
        return words.Length == 0 ? null : $"{char.ToUpperInvariant(words[0])}{words[1..]}.";
    }

    private static string? Join(params string?[] sentences)
    {
        var said = sentences.OfType<string>().ToList();
        return said.Count == 0 ? null : string.Join(' ', said);
    }

    private static int Severity(ClusterPod pod) => new PodPhase(pod.Phase).Kind switch
    {
        PodPhaseKind.Failed => 0,
        PodPhaseKind.Running => 1,
        PodPhaseKind.Pending => 2,
        _ => 3,
    };
}
