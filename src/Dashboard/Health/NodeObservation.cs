namespace Dashboard.Health;

/// <summary>What the page knew about one endpoint at one check: the input of <see cref="EventDetector"/>.</summary>
/// <param name="Checks">The entries of the node's detailed health check; null when the check read none.</param>
public sealed record NodeObservation(HealthState State, string? Version, TelemetrySnapshot? Telemetry, ProbeResult? Last = null, HealthDetail? Checks = null)
{
    public static NodeObservation Of(TargetStatus target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return new NodeObservation(target.State, target.Version, target.Telemetry, target.Last, target.HealthDetail);
    }
}
