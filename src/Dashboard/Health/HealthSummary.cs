namespace Dashboard.Health;

public enum SummaryLevel
{
    /// <summary>The topology has no endpoint.</summary>
    Empty,

    /// <summary>Nothing is known to be wrong, and not every endpoint has been checked.</summary>
    Pending,

    /// <summary>Every endpoint has been checked, and every one of them is healthy.</summary>
    AllHealthy,

    /// <summary>At least one endpoint is not healthy: it answered anything but HTTP 200, or gave no answer at all.</summary>
    Problems,
}

/// <summary>The counts behind the header: every tile counts as one node (Front Door endpoints and web apps).</summary>
public sealed record HealthSummary(int Total, int Healthy, int Pending)
{
    public int NotHealthy => Total - Healthy - Pending;

    /// <summary>
    /// The level the counts fall on, asked in order of precedence: a total of nothing outranks a problem, and a problem
    /// outranks a pending check. The first ask that holds decides the level.
    /// </summary>
    public SummaryLevel Level => this switch
    {
        _ when Total == 0 => SummaryLevel.Empty,
        _ when NotHealthy > 0 => SummaryLevel.Problems,
        _ when Pending > 0 => SummaryLevel.Pending,
        _ => SummaryLevel.AllHealthy,
    };

    public string Text => SummaryWords.Text(Level, Total, Healthy, Pending);

    public static HealthSummary Of(IEnumerable<HealthState> states)
    {
        ArgumentNullException.ThrowIfNull(states);
        int total = 0, healthy = 0, pending = 0;
        foreach (var state in states)
        {
            total++;
            healthy += state == HealthState.Healthy ? 1 : 0;
            pending += state == HealthState.Pending ? 1 : 0;
        }

        return new HealthSummary(total, healthy, pending);
    }
}
