namespace Dashboard.Tests;

/// <summary>
/// The history strip query: HistorySlots.Of(buffer) right-aligns the buffer's items into a fixed-width strip
/// with nulls for empty slots on the left, so the view walks slots without ring-buffer arithmetic.
/// </summary>
public class HistorySlotsTests
{
    private static ProbeResult Healthy(int ms) =>
        new(HealthState.Healthy, 200, ms, DateTimeOffset.Now);

    private static ProbeResult Unhealthy(int ms) =>
        new(HealthState.Unhealthy, 500, ms, DateTimeOffset.Now);

    private static ProbeResult Unreachable() =>
        new(HealthState.Unreachable, null, null, DateTimeOffset.Now);

    [Fact]
    public void ANewEmptyBufferProducesAllNullSlots()
    {
        var buffer = new HistoryBuffer<ProbeResult>(5);
        var slots = HistorySlots.Of(buffer);

        Assert.Equal(5, slots.Count);
        Assert.All(slots, slot => Assert.Null(slot));
    }

    [Fact]
    public void APartiallyFilledBufferRightAlignsItemsWithEmptySlotsOnTheLeft()
    {
        var buffer = new HistoryBuffer<ProbeResult>(5)
        {
            Healthy(10),
            Unhealthy(20),
            Healthy(30)
        };
        var slots = HistorySlots.Of(buffer);

        Assert.Equal(5, slots.Count);
        Assert.Null(slots[0]);
        Assert.Null(slots[1]);
        Assert.NotNull(slots[2]);
        Assert.Equal(10, slots[2]!.LatencyMs);
        Assert.NotNull(slots[3]);
        Assert.Equal(20, slots[3]!.LatencyMs);
        Assert.NotNull(slots[4]);
        Assert.Equal(30, slots[4]!.LatencyMs);
    }

    [Fact]
    public void AFullBufferHasNoNullSlots()
    {
        var buffer = new HistoryBuffer<ProbeResult>(3)
        {
            Healthy(10),
            Unhealthy(20),
            Unreachable()
        };
        var slots = HistorySlots.Of(buffer);

        Assert.Equal(3, slots.Count);
        Assert.All(slots, slot => Assert.NotNull(slot));
        Assert.Equal(10, slots[0]!.LatencyMs);
        Assert.Equal(20, slots[1]!.LatencyMs);
        Assert.Equal(HealthState.Unreachable, slots[2]!.State);
    }

    [Fact]
    public void AWrappedBufferProducesTheCorrectSlots()
    {
        var buffer = new HistoryBuffer<ProbeResult>(3);
        buffer.Add(Healthy(1));
        buffer.Add(Healthy(2));
        buffer.Add(Healthy(3));
        buffer.Add(Healthy(4));
        buffer.Add(Healthy(5));

        // Buffer keeps last 3: 3, 4, 5 (indices 0, 1, 2)
        var slots = HistorySlots.Of(buffer);

        Assert.Equal(3, slots.Count);
        Assert.NotNull(slots[0]);
        Assert.Equal(3, slots[0]!.LatencyMs);
        Assert.NotNull(slots[1]);
        Assert.Equal(4, slots[1]!.LatencyMs);
        Assert.NotNull(slots[2]);
        Assert.Equal(5, slots[2]!.LatencyMs);
    }

    [Fact]
    public void SlotsCountEqualsCapacityForEveryBufferState()
    {
        var buffer = new HistoryBuffer<ProbeResult>(7);

        Assert.Equal(7, HistorySlots.Of(buffer).Count);

        buffer.Add(Healthy(1));
        Assert.Equal(7, HistorySlots.Of(buffer).Count);

        buffer.Add(Healthy(2));
        buffer.Add(Healthy(3));
        buffer.Add(Healthy(4));
        buffer.Add(Healthy(5));
        buffer.Add(Healthy(6));
        buffer.Add(Healthy(7));
        Assert.Equal(7, HistorySlots.Of(buffer).Count);

        buffer.Add(Healthy(8));
        buffer.Add(Healthy(9));
        Assert.Equal(7, HistorySlots.Of(buffer).Count);
    }
}
