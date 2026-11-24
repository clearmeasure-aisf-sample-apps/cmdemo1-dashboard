namespace Dashboard.Tests;

/// <summary>
/// <see cref="HistoryBuffer{T}.Last" />, the newest item of the ring: the item <see cref="HistoryBuffer{T}.Add"/>
/// wrote last, which is the item at the last index of what the buffer keeps, and nothing at all when the buffer keeps
/// nothing. It is what lets a reader ask a history what came last - a tile asks it what its node answered last,
/// through <see cref="TargetStatus.Last" /> - without counting the items first or walking to the end of them.
/// <para>
/// <see cref="HistoryBufferTests" /> and <see cref="HistoryBufferEdgeCaseTests" /> pin the ring those lines stand on:
/// the items kept oldest first, the oldest gone once a capacity is full and every item stepped one place, the walk
/// across the end of what the buffer holds, and the refusals of a capacity below one and of an index outside what is
/// kept. The newest item is read through that same indexer at that same last index, which those tests pin to the last
/// item written, so what they say of the index this repeats for the name.
/// </para>
/// <para>
/// What is pinned here and nowhere before is the newest item itself: a buffer holding nothing having no newest item
/// and answering with the default of its item type - an absent reference, the zero of a number, the first state of an
/// enum - at every capacity rather than throwing the way its indexer does; the newest item standing at the item just
/// written while the buffer fills, at the very item that filled it, and at every item written past that, wherever the
/// wrap point came to rest and however many times it crossed the end of what the buffer holds; the newest item being
/// the instance given and not a copy of it, and the same instance at every reading; a none written last standing as
/// the newest item of a buffer that is not empty, which is the one thing the newest item cannot tell you and
/// <see cref="HistoryBuffer{T}.Count" /> can; and the reading of the newest item moving nothing - not the count, not
/// the capacity, not the walk, and not the item at any index.
/// </para>
/// </summary>
public class HistoryBufferLastTests
{
    /// <summary>A buffer of the capacity it is given, written through with the items numbered from one up to
    /// <paramref name="count" />: the last <c>capacity</c> of them stand, oldest first, so the item numbered
    /// <paramref name="count" /> is the newest thing in it whatever the wrap point did on the way.</summary>
    private static HistoryBuffer<int> Written(int capacity, int count)
    {
        var buffer = new HistoryBuffer<int>(capacity);
        for (var item = 1; item <= count; item++)
        {
            buffer.Add(item);
        }

        return buffer;
    }

    // ----- a buffer with nothing in it has no newest item

    /// <summary>A buffer that holds nothing has no newest item, at every capacity, and says so with the default of the
    /// type it holds rather than with the refusal its indexer gives at the same index: the absent reference of a
    /// buffer of references, the zero of a number, the first state of an enum, and the none of a nullable number. The
    /// first item written is the newest from then on.</summary>
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(30)]
    public void ABufferHoldingNothingHasNoNewestItemAndSaysItWithTheDefaultOfType(int capacity)
    {
        Assert.Null(new HistoryBuffer<string>(capacity).Last);
        Assert.Null(new HistoryBuffer<string?>(capacity).Last);
        Assert.Equal(0, new HistoryBuffer<int>(capacity).Last);
        Assert.Equal(HealthState.Pending, new HistoryBuffer<HealthState>(capacity).Last);

        // Nothing was refused and nothing was half-written: the buffer is as it was, and takes an item as readily
        // as before it was asked.
        var buffer = new HistoryBuffer<string>(capacity);

        Assert.Null(buffer.Last);
        Assert.Empty(buffer);

        buffer.Add("first");

        Assert.Equal("first", buffer.Last);
        Assert.Single(buffer);
    }

    // ----- the newest item, at every step and across every wrap

    /// <summary>The newest item is the item written last, whether the buffer still fills, stands full at its capacity
    /// or has wrapped about the end of what it holds again and again - and it is the very item at the last index of
    /// what the buffer keeps, the one the indexer reads there. The capacities are those the page's own buffers are
    /// made of: one slot, a handful, the thirty checks of a tile and the capacity of the event log.</summary>
    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 40)]
    [InlineData(2, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 2)]
    [InlineData(3, 4)]
    [InlineData(4, 20)]
    [InlineData(30, 1)]
    [InlineData(30, 29)]
    [InlineData(30, 30)]
    [InlineData(30, 31)]
    [InlineData(30, 100)]
    [InlineData(50, 50)]
    [InlineData(50, 250)]
    public void TheNewestItemIsTheItemWrittenLastWhereverTheWrapPointStands(int capacity, int written)
    {
        var buffer = Written(capacity, written);

        Assert.Equal(written, buffer.Last);
        Assert.Equal(buffer[buffer.Count - 1], buffer.Last);
        Assert.Equal(written, buffer[^1]);
        Assert.Equal(Math.Min(capacity, written), buffer.Count);
    }

    /// <summary>The newest item steps to each item as it comes, one add at a time: while the buffer fills it is the
    /// item that came and every earlier item is still somewhere under it; once the buffer is full it is the item that
    /// came, the oldest has left, and no earlier item takes the place again. The whole walk of it is watched, at a
    /// capacity the items cross the end of what the buffer holds twice over.</summary>
    [Fact]
    public void TheNewestItemStepsToEachItemAsItComes()
    {
        const int capacity = 4;
        var buffer = new HistoryBuffer<int>(capacity);

        for (var item = 1; item <= capacity * 3; item++)
        {
            buffer.Add(item);

            Assert.Equal(item, buffer.Last);
            Assert.Equal(Math.Min(item, capacity), buffer.Count);

            // Everything still kept is older than the newest item but the one, and the oldest kept is the oldest of
            // them: the newest item is the end of the run, not a place beside it.
            var kept = buffer.ToArray();

            Assert.Equal(item, kept[^1]);
            Assert.Equal(kept[^1], buffer.Last);
            Assert.True(kept.Length == 1 || kept[0] < buffer.Last);
        }
    }

    /// <summary>The newest item of a buffer of references is the very instance written last, before and after the
    /// items that follow it pushed the earlier ones out - and reading the newest item twice, and reading it beside the
    /// indexer, says the one instance rather than a copy made for the reading.</summary>
    [Fact]
    public void TheNewestItemIsTheVeryInstanceWrittenLast()
    {
        var first = new ProbeResult(HealthState.Healthy, 200, 12, new DateTimeOffset(2026, 10, 4, 22, 0, 0, TimeSpan.Zero));
        var second = new ProbeResult(HealthState.Unhealthy, 503, 40, first.CheckedAt.AddSeconds(1));
        var third = new ProbeResult(HealthState.Unreachable, null, null, first.CheckedAt.AddSeconds(2));
        var buffer = new HistoryBuffer<ProbeResult>(2);

        buffer.Add(first);

        Assert.Same(first, buffer.Last);

        buffer.Add(second);
        buffer.Add(third);

        Assert.Same(third, buffer.Last);
        Assert.Same(third, buffer[1]);
        Assert.Same(buffer.Last, buffer.Last);
        Assert.DoesNotContain(first, buffer);
    }

    /// <summary>The none of a reference written last stands as the newest item of a buffer that is not empty: the
    /// newest item answers as the none, and it is the count and not the newest item that says the buffer holds one
    /// item. This is what the page's own telemetry samples look like the moment a node stops reporting - a none at the
    /// end of readings that are still there.</summary>
    [Fact]
    public void ANoneWrittenLastIsTheNewestItemOfABufferThatIsNotEmpty()
    {
        var buffer = new HistoryBuffer<TelemetrySnapshot?>(3);
        var reading = new TelemetrySnapshot(10, 0, 10, 0, null, 0, 0, 0, null, 0, new DateTimeOffset(2026, 10, 4, 22, 0, 0, TimeSpan.Zero));

        buffer.Add(reading);
        buffer.Add(null);

        Assert.Null(buffer.Last);
        Assert.Equal(2, buffer.Count);
        Assert.NotEmpty(buffer);
        Assert.Equal(reading, buffer[0]);
        Assert.Null(buffer[1]);
    }

    /// <summary>Asking a buffer what its newest item is leaves the buffer exactly as it was: the count, the capacity,
    /// every index and the whole walk say the same afterwards, at a buffer filling and at one wrapped about the end of
    /// what it holds - so the newest item may be read as often as a page draws itself, and twice in the same
    /// sentence.</summary>
    [Fact]
    public void AskingForTheNewestItemMovesNothingOfTheBuffer()
    {
        foreach (var buffer in new[] { Written(5, 3), Written(5, 12) })
        {
            var kept = buffer.ToArray();
            var count = buffer.Count;
            var capacity = buffer.Capacity;

            var newest = buffer.Last;
            var again = buffer.Last;

            Assert.Equal(newest, again);
            Assert.Equal(kept, buffer.ToArray());
            Assert.Equal(count, buffer.Count);
            Assert.Equal(capacity, buffer.Capacity);
            Assert.Equal(kept, Enumerable.Range(0, count).Select(index => buffer[index]));
            Assert.Equal(kept[^1], newest);

            // And the item that comes next is the newest, with the one before it still kept under it.
            buffer.Add(count + 100);

            Assert.Equal(count + 100, buffer.Last);
        }
    }
}
