namespace Dashboard.Tests;

/// <summary>
/// The order <see cref="HistoryText.Describe(IReadOnlyCollection{ProbeResult})" /> counts the states in: healthy,
/// then unhealthy, then unreachable, whatever order the checks were made in; and each state named in the words of
/// a sentence, the lowercase of its label.
/// </summary>
public class HistoryTextCountedOrderTests
{
    private static readonly DateTimeOffset Time = new(2026, 10, 4, 22, 0, 5, TimeSpan.Zero);

    private static ProbeResult Check(HealthState state) => new(state, null, null, Time);

    /// <summary>The checks made in the reverse of the reading order are still said healthy first, unhealthy
    /// second and unreachable last.</summary>
    [Fact]
    public void TheStatesAreSaidHealthyThenUnhealthyThenUnreachableWhateverTheOrderOfTheChecks() =>
        Assert.Equal(
            "Last 4 checks: 1 healthy, 1 unhealthy, 2 unreachable",
            HistoryText.Describe([Check(HealthState.Unreachable), Check(HealthState.Unhealthy), Check(HealthState.Unreachable), Check(HealthState.Healthy)]));

    /// <summary>Every order of one check per state says the same sentence: the history is counted, not read.</summary>
    [Fact]
    public void EveryOrderOfTheThreeStatesSaysTheSameSentence()
    {
        HealthState[][] orders =
        [
            [HealthState.Healthy, HealthState.Unhealthy, HealthState.Unreachable],
            [HealthState.Healthy, HealthState.Unreachable, HealthState.Unhealthy],
            [HealthState.Unhealthy, HealthState.Healthy, HealthState.Unreachable],
            [HealthState.Unhealthy, HealthState.Unreachable, HealthState.Healthy],
            [HealthState.Unreachable, HealthState.Healthy, HealthState.Unhealthy],
            [HealthState.Unreachable, HealthState.Unhealthy, HealthState.Healthy],
        ];

        foreach (var order in orders)
        {
            Assert.Equal("Last 3 checks: 1 healthy, 1 unhealthy, 1 unreachable", HistoryText.Describe(order.Select(Check).ToList()));
        }
    }

    /// <summary>Unhealthy is said before unreachable when no check was healthy.</summary>
    [Fact]
    public void UnhealthyIsSaidBeforeUnreachable() =>
        Assert.Equal("Last 2 checks: 1 unhealthy, 1 unreachable", HistoryText.Describe([Check(HealthState.Unreachable), Check(HealthState.Unhealthy)]));

    /// <summary>Each counted state is named by the lowercase of its badge label, the words of a sentence.</summary>
    [Theory]
    [InlineData(HealthState.Healthy)]
    [InlineData(HealthState.Unhealthy)]
    [InlineData(HealthState.Unreachable)]
    public void EachCountedStateIsNamedByTheLowercaseOfItsLabel(HealthState state) =>
        Assert.Equal($"Last check: 1 {HealthClassifier.Label(state).ToLowerInvariant()}", HistoryText.Describe([Check(state)]));
}
