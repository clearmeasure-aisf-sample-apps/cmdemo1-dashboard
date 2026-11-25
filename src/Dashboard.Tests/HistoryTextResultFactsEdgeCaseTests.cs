namespace Dashboard.Tests;

/// <summary>
/// The boundaries of <see cref="HistoryText.ResultFacts" /> and the corners of the two <c>Describe</c> overloads
/// that <see cref="HistoryTextEdgeCaseTests" />, <see cref="TextTests" /> and the shared helper test in
/// <see cref="HealthHelperCoverageTests" /> leave open: a zero HTTP status or a zero time to the answer is still
/// said (present, not absent); the separator is used exactly as given, empty or <see langword="null" /> included;
/// a single pending check is said with the classifier's <c>Checking</c> label; and pending checks mixed into a
/// history of several states are counted in the total and never named.
/// </summary>
public class HistoryTextResultFactsEdgeCaseTests
{
    private static readonly DateTimeOffset Time = new(2026, 10, 4, 22, 0, 5, TimeSpan.Zero);

    private static ProbeResult Check(HealthState state, int? statusCode = null, int? latencyMs = null) =>
        new(state, statusCode, latencyMs, Time);

    // ----- ResultFacts: zero is a value, not an absence

    [Fact]
    public void AZeroStatusIsSaid() =>
        Assert.Equal("HTTP 0", HistoryText.ResultFacts(Check(HealthState.Unhealthy, statusCode: 0), " · "));

    [Fact]
    public void AZeroTimeToTheAnswerIsSaid() =>
        Assert.Equal("0 ms", HistoryText.ResultFacts(Check(HealthState.Healthy, latencyMs: 0), " · "));

    [Fact]
    public void AZeroStatusAndAZeroTimeAreBothSaid() =>
        Assert.Equal("HTTP 0, 0 ms", HistoryText.ResultFacts(Check(HealthState.Unhealthy, 0, 0), ", "));

    [Fact]
    public void TheLargestStatusIsSaidAsItIs() =>
        Assert.Equal("HTTP 2147483647", HistoryText.ResultFacts(Check(HealthState.Unhealthy, statusCode: int.MaxValue), ", "));

    // ----- ResultFacts: the separator is used exactly as given

    [Fact]
    public void AnEmptySeparatorJoinsThePartsWithNothingBetween() =>
        Assert.Equal("HTTP 20010 ms", HistoryText.ResultFacts(Check(HealthState.Healthy, 200, 10), string.Empty));

    [Fact]
    public void ANullSeparatorJoinsThePartsWithNothingBetween() =>
        Assert.Equal("HTTP 20010 ms", HistoryText.ResultFacts(Check(HealthState.Healthy, 200, 10), null!));

    [Fact]
    public void AMultiCharacterSeparatorIsUsedWhole() =>
        Assert.Equal("HTTP 200 | and | 10 ms", HistoryText.ResultFacts(Check(HealthState.Healthy, 200, 10), " | and | "));

    [Theory]
    [InlineData("")]
    [InlineData(" · ")]
    [InlineData(null)]
    public void OnePartIsSaidWithoutAnySeparator(string? separator)
    {
        Assert.Equal("HTTP 503", HistoryText.ResultFacts(Check(HealthState.Unhealthy, statusCode: 503), separator!));
        Assert.Equal("7 ms", HistoryText.ResultFacts(Check(HealthState.Unreachable, latencyMs: 7), separator!));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" · ")]
    [InlineData(null)]
    public void NoPartIsSaidAsEmptyWhateverTheSeparator(string? separator) =>
        Assert.Equal(string.Empty, HistoryText.ResultFacts(Check(HealthState.Unreachable), separator!));

    [Fact]
    public void TheStateDoesNotChangeTheFacts()
    {
        foreach (var state in Enum.GetValues<HealthState>())
        {
            Assert.Equal("HTTP 200, 10 ms", HistoryText.ResultFacts(Check(state, 200, 10), ", "));
        }
    }

    // ----- Describe(ProbeResult): the pending check and the zero values

    [Fact]
    public void APendingCheckIsSaidWithTheCheckingLabel() =>
        Assert.Equal("22:00:05 Checking", HistoryText.Describe(Check(HealthState.Pending), TimeZoneInfo.Utc));

    [Fact]
    public void ACheckWithZeroStatusAndZeroTimeSaysBoth() =>
        Assert.Equal("22:00:05 Unhealthy, HTTP 0, 0 ms", HistoryText.Describe(Check(HealthState.Unhealthy, 0, 0), TimeZoneInfo.Utc));

    // ----- Describe(history): pending checks among several named states

    [Fact]
    public void PendingChecksAmongSeveralStatesAreCountedButNotNamed() =>
        Assert.Equal(
            "Last 6 checks: 2 healthy, 1 unreachable",
            HistoryText.Describe(
            [
                Check(HealthState.Pending),
                Check(HealthState.Healthy),
                Check(HealthState.Unreachable),
                Check(HealthState.Pending),
                Check(HealthState.Healthy),
                Check(HealthState.Pending),
            ]));

    [Fact]
    public void AStateWithNoCheckIsLeftOutOfTheList() =>
        Assert.Equal(
            "Last 3 checks: 2 healthy, 1 unreachable",
            HistoryText.Describe([Check(HealthState.Healthy), Check(HealthState.Unreachable), Check(HealthState.Healthy)]));

    [Fact]
    public void ManyChecksOfOneStateAreSaidWithTheirFullCount() =>
        Assert.Equal(
            "Last 1000 checks: 1000 unhealthy",
            HistoryText.Describe(Enumerable.Range(0, 1000).Select(_ => Check(HealthState.Unhealthy)).ToList()));
}
