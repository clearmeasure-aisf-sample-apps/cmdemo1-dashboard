using System.Globalization;

namespace Dashboard.Health;

/// <summary>
/// The defaults every probe of the dashboard shares: a node's check (<see cref="NodeProber" />), the pinned versions
/// (<see cref="PinnedVersionsReader" />) and the cluster reading (<see cref="Cluster.ClusterReader" />) all wait as long,
/// and all say the same words when that wait ends with no answer (<see cref="NoAnswerWithin" />).
/// </summary>
internal static class ProbeDefaults
{
    /// <summary>
    /// The shared probe wait in its whole-second form: ten, the number <see cref="DefaultTimeout" /> is made of. A
    /// caller that cannot hold a <see cref="TimeSpan" /> reads the wait here - the words that say it
    /// (<see cref="NoAnswerWithin" /> says it in whole seconds too, rounded, in the invariant culture) and a browser
    /// call that takes whole seconds, as the traffic button's <see cref="TrafficRun.Values.AnswerWithinSeconds" /> does.
    /// </summary>
    public const int DefaultTimeoutSeconds = 10;

    /// <summary>How long a probe may take to answer before it counts as no answer: <see cref="DefaultTimeoutSeconds" /> seconds.</summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(DefaultTimeoutSeconds);

    /// <summary>
    /// The clause a probe reports when <paramref name="timeout" /> passed with no answer: <c>no answer within 10 s</c>.
    /// The timeout reads in whole seconds, rounded (2.5 s reads "3 s"), formatted in the invariant culture.
    /// </summary>
    public static string NoAnswerWithin(TimeSpan timeout) =>
        string.Create(CultureInfo.InvariantCulture, $"no answer within {timeout.TotalSeconds:0} s");
}
