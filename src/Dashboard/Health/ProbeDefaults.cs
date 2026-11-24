using System.Globalization;

namespace Dashboard.Health;

/// <summary>
/// The defaults every probe of the dashboard shares: a node's check (<see cref="NodeProber" />), the pinned versions
/// (<see cref="PinnedVersionsReader" />) and the cluster reading (<see cref="Cluster.ClusterReader" />) all wait as long,
/// and all say the same words when that wait ends with no answer (<see cref="NoAnswerWithin" />).
/// </summary>
internal static class ProbeDefaults
{
    /// <summary>How long a probe may take to answer before it counts as no answer.</summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// The clause a probe reports when <paramref name="timeout" /> passed with no answer: <c>no answer within 10 s</c>.
    /// The timeout reads in whole seconds, rounded (2.5 s reads "3 s"), formatted in the invariant culture.
    /// </summary>
    public static string NoAnswerWithin(TimeSpan timeout) =>
        string.Create(CultureInfo.InvariantCulture, $"no answer within {timeout.TotalSeconds:0} s");
}
