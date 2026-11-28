namespace Dashboard.Tests;

/// <summary>
/// Characterization tests for the words the refactored <see cref="HistoryText" /> now keeps as private consts -
/// the list separator and the <c>Last</c> lead word - pinning the exact sentences each spelling produces, so a
/// later edit that changes either const fails here before it reaches a reader.
/// </summary>
public class HistoryTextSeparatorTests
{
    private static readonly DateTimeOffset Time = new(2026, 10, 4, 15, 4, 5, TimeSpan.Zero);

    private static ProbeResult Check(HealthState state, int? statusCode = null, int? latencyMs = null) =>
        new(state, statusCode, latencyMs, Time);

    [Fact]
    public void TheMultiStateJoinUsesTheCommaSpaceSeparator() =>
        Assert.Equal(
            "Last 12 checks: 10 healthy, 2 unreachable",
            HistoryText.Describe(
            [
                Check(HealthState.Healthy), Check(HealthState.Healthy), Check(HealthState.Healthy),
                Check(HealthState.Healthy), Check(HealthState.Healthy), Check(HealthState.Healthy),
                Check(HealthState.Healthy), Check(HealthState.Healthy), Check(HealthState.Healthy),
                Check(HealthState.Healthy),
                Check(HealthState.Unreachable), Check(HealthState.Unreachable),
            ]));

    [Fact]
    public void TheOneCheckLeadHasNoDigit() =>
        Assert.Equal("Last check: 1 healthy", HistoryText.Describe([Check(HealthState.Healthy)]));

    [Fact]
    public void TheOneCheckLineWithBothFacts() =>
        Assert.Equal("15:04:05 Healthy, HTTP 200, 123 ms", HistoryText.Describe(Check(HealthState.Healthy, 200, 123), TimeZoneInfo.Utc));

    [Fact]
    public void TheOneCheckLineWithOnlyLatency() =>
        Assert.Equal("15:04:05 Unreachable, 85 ms", HistoryText.Describe(Check(HealthState.Unreachable, latencyMs: 85), TimeZoneInfo.Utc));
}
