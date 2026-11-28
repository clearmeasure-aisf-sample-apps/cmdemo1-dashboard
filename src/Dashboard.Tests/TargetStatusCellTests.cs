namespace Dashboard.Tests;

/// <summary>
/// The two cells of the tile's last check — HTTP status and latency — read as <see cref="TargetStatus.StatusCell" />
/// and <see cref="TargetStatus.LatencyCell" />: the dash when the tile has no check or the check got no answer, the
/// invariant-culture code or the millisecond word otherwise. The dash itself is <see cref="TargetStatus.NotGiven" />,
/// owned here rather than hand-typed in the razor file.
/// </summary>
public class TargetStatusCellTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private static readonly Uri West = new("https://app-west.example.net");

    private static TargetStatus Node() =>
        new(TargetKind.Node, "app-west", West, "westus3", "primary");

    private static ProbeResult Answer(HealthState state, int? statusCode = 200, int? latencyMs = 12) =>
        new(state, statusCode, latencyMs, Start);

    [Fact]
    public void NotGivenIsTheEmDash() =>
        Assert.Equal("—", TargetStatus.NotGiven);

    [Fact]
    public void BothCellsAreNotGivenBeforeAnyCheck()
    {
        var tile = Node();

        Assert.Equal(TargetStatus.NotGiven, tile.StatusCell);
        Assert.Equal(TargetStatus.NotGiven, tile.LatencyCell);
    }

    [Fact]
    public void StatusCellIsTheInvariantCultureCodeAfterARecordedCheck()
    {
        var tile = Node();
        tile.Record(Answer(HealthState.Unhealthy, statusCode: 503, latencyMs: 85));

        Assert.Equal("503", tile.StatusCell);
    }

    [Fact]
    public void StatusCellIsInvariantUnderANonInvariantCurrentCulture()
    {
        var tile = Node();
        tile.Record(Answer(HealthState.Unhealthy, statusCode: 503, latencyMs: 85));

        using var scope = new CultureScope("fi-FI");
        Assert.Equal("503", tile.StatusCell);
    }

    [Fact]
    public void StatusCellIsNotGivenForACheckWithNoStatusCode()
    {
        var tile = Node();
        tile.Record(Answer(HealthState.Unreachable, statusCode: null, latencyMs: null));

        Assert.Equal(TargetStatus.NotGiven, tile.StatusCell);
    }

    [Fact]
    public void LatencyCellIsTheMillisecondWordAfterACheckWithALatency()
    {
        var tile = Node();
        tile.Record(Answer(HealthState.Healthy, statusCode: 200, latencyMs: 85));

        Assert.Equal(HistoryText.Milliseconds(85), tile.LatencyCell);
    }

    [Fact]
    public void LatencyCellIsNotGivenWhenTheLatencyIsNull()
    {
        var tile = Node();
        tile.Record(Answer(HealthState.Unreachable, statusCode: null, latencyMs: null));

        Assert.Equal(TargetStatus.NotGiven, tile.LatencyCell);
    }

    [Fact]
    public void LatencyCellIsNotGivenBeforeAnyCheck()
    {
        var tile = Node();

        Assert.Equal(TargetStatus.NotGiven, tile.LatencyCell);
    }
}
