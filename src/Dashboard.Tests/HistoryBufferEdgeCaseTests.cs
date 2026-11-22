using System.Collections;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="HistoryBuffer{T}" />, the ring the page keeps its
/// last readings in - the oldest first, the last <c>Capacity</c> items and no more - declared in
/// <c>src/Dashboard/Health/HistoryBuffer.cs</c> and standing behind a target's <c>History</c> and <c>Samples</c>,
/// behind the cluster monitor's trend samples, behind <see cref="EventLog" /> and behind the strip the page draws of
/// any of them.
/// <para>
/// <see cref="HistoryBufferTests" /> already pins the buffer of <c>int</c> at its own: a buffer made new standing
/// empty and knowing the capacity it was made with, the items it keeps standing oldest first, the oldest gone and the
/// last three kept when a buffer of three is filled with five, the last thirty of a hundred items walked through a
/// buffer of thirty, an index outside what is kept refused and a capacity of zero refused.
/// <see cref="EventLogEdgeCaseTests" /> pins, through the log that decorates the buffer, the capacity that is no
/// capacity and a log of one and of two slots dropping their oldest.
/// </para>
/// <para>
/// What those two leave open is pinned here, and only that: the two refusals said by the name of the argument each is
/// about - a capacity below one, which is no capacity, at zero, a negative and the smallest int, and an index outside
/// what the buffer keeps, at the count, at the capacity, at the first index of a buffer that holds nothing and at the
/// smallest index there is; the count of a buffer made with nothing in it, which is nothing to count, nothing to walk
/// and nothing to read at any index; the capacity standing where the buffer was made, fresh, with one item, full and
/// wrapped about again and again, while the count rises one at a time and then stands at it; the walk of the items
/// across the wrap point - every item stepping down one index when a full buffer takes another, the items kept being
/// the last written, oldest first, at capacities whose items cross the end of what the buffer holds twice and three
/// times over, and the walk saying the items the indexes say, ending at the count and moving nothing on the way; the
/// buffer standing as a list of items nobody changes through, that list being all of it a caller gets hold of, with no
/// list and no collection to anyone who asks to put into it; and the item type being nothing at all asked of - the
/// item at the ends of the number line, the empty word, a state the enum does not name, a reference held as the very
/// instance it was given, a value read out being the value and not a handle on the buffer, and a none kept as an item
/// of its own: counted, read, walked and dropped when its turn comes, the buffer refusing no item.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every item is one the test brings, and
/// the buffer holds no moment of its own.
/// </para>
/// </summary>
public class HistoryBufferEdgeCaseTests
{
    // ----- the buffers the tests are made of

    /// <summary>A buffer of the capacity it is given, written through with the items numbered from one up to
    /// <paramref name="count" />: the last <c>capacity</c> of them stand, oldest first, wherever the wrap point came
    /// to rest and however many times it was crossed - the way the page's own buffers stand once it has been reading
    /// for a while. The window each of them ends up holding is written out where the test says it, not worked out from
    /// the two numbers here.</summary>
    private static HistoryBuffer<int> Written(int capacity, int count)
    {
        var buffer = new HistoryBuffer<int>(capacity);
        for (var item = 1; item <= count; item++)
        {
            buffer.Add(item);
        }

        return buffer;
    }

    /// <summary>The items the buffer's indexes hold, read from the first index to the last of them.</summary>
    private static T[] ReadIndexes<T>(HistoryBuffer<T> buffer) =>
        [.. Enumerable.Range(0, buffer.Count).Select(index => buffer[index])];

    // ----- the capacity the buffer is made with

    /// <summary>A capacity below one is no capacity: zero, a negative and the smallest int are all refused, each said
    /// by the name <c>capacity</c>. A buffer is never made with nothing to hold, so nothing that holds one afterwards
    /// has to ask whether it was.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void ACapacityBelowOneIsRefusedNamingTheArgument(int capacity)
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => new HistoryBuffer<string>(capacity));

        Assert.Equal("capacity", error.ParamName);
    }

    /// <summary>The capacity stands where the buffer was made: fresh, with one item in it, full, and wrapped about
    /// again as many times as items come after - of the capacity the buffer was made with and never of the items it
    /// holds. While the buffer fills the count stands under the capacity; from the item that filled it the two stand
    /// as one, and no further item moves either. The page may size a buffer once and draw a trend of it without asking
    /// again how much room it has.</summary>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(64)]
    public void TheCapacityStandsWhereTheBufferWasMade(int capacity)
    {
        var buffer = new HistoryBuffer<int>(capacity);

        Assert.Equal(capacity, buffer.Capacity);
        Assert.Empty(buffer);

        buffer.Add(1);

        Assert.Equal(capacity, buffer.Capacity);
        Assert.Single(buffer);

        for (var item = 2; item <= capacity * 4 + 3; item++)
        {
            buffer.Add(item);

            Assert.Equal(capacity, buffer.Capacity);
            Assert.Equal(Math.Min(item, capacity), buffer.Count);
        }
    }

    // ----- a buffer made with nothing in it

    /// <summary>A buffer made with nothing in it holds nothing, at every capacity: it counts nothing, it walks as no
    /// items at all - held as the buffer and held as the sequence nobody knows the type of - and every index is
    /// refused, the first index among them, by the name <c>index</c>. The first item to come takes the first index and
    /// is the only thing there is to read.</summary>
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(50)]
    public void ABufferMadeWithNothingInItHoldsNothing(int capacity)
    {
        var buffer = new HistoryBuffer<string>(capacity);

        Assert.Empty(buffer);
        Assert.Empty(ReadIndexes(buffer));
        Assert.Empty((IReadOnlyList<string>)buffer);
        Assert.Empty((IEnumerable)buffer);
        Assert.False(buffer.GetEnumerator().MoveNext());

        var atTheFirstIndex = Assert.Throws<ArgumentOutOfRangeException>(() => _ = buffer[0]);
        Assert.Equal("index", atTheFirstIndex.ParamName);

        var atTheCapacity = Assert.Throws<ArgumentOutOfRangeException>(() => _ = buffer[capacity]);
        Assert.Equal("index", atTheCapacity.ParamName);

        buffer.Add("first");

        Assert.Equal(["first"], ReadIndexes(buffer));
        Assert.Equal(["first"], buffer);
    }

    // ----- the index across the wrap point

    /// <summary>An index outside what the buffer keeps is refused by the name <c>index</c>: the index at the count,
    /// the index at the capacity (one and the same index once the buffer is full, and past what is kept either way)
    /// and the smallest index there is, which stands for every negative one the way zero stands for every capacity
    /// below one. The last index of what the buffer keeps, one below the count, holds the last item written - whether
    /// the buffer still fills, stands full at its capacity, or has wrapped about again.</summary>
    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 2)]
    [InlineData(3, 5)]
    [InlineData(5, 4)]
    [InlineData(5, 5)]
    [InlineData(7, 100)]
    public void AnIndexOutsideWhatTheBufferKeepsIsRefusedNamingTheArgument(int capacity, int written)
    {
        var buffer = Written(capacity, written);

        Assert.Equal(Math.Min(capacity, written), buffer.Count);
        if (written > 0)
        {
            Assert.Equal(written, buffer[buffer.Count - 1]);
        }

        var atCount = Assert.Throws<ArgumentOutOfRangeException>(() => _ = buffer[buffer.Count]);
        Assert.Equal("index", atCount.ParamName);

        var atCapacity = Assert.Throws<ArgumentOutOfRangeException>(() => _ = buffer[capacity]);
        Assert.Equal("index", atCapacity.ParamName);

        var atSmallestIndex = Assert.Throws<ArgumentOutOfRangeException>(() => _ = buffer[int.MinValue]);
        Assert.Equal("index", atSmallestIndex.ParamName);
    }

    /// <summary>What an index held before an item came, it holds again after it - or holds what the item before it
    /// held: while the buffer fills every item stands at the index it stood at and the item that came takes the last
    /// index of what the buffer keeps; once the buffer is full every item steps down one index, the oldest leaves, and
    /// the item that came takes the last. The same shift at every add, wherever the wrap point stands then - at the
    /// start of what the buffer holds, at its end, or between it. The capacities are those whose items cross the end
    /// of what the buffer holds, and the walk of them is from the first item to the last.</summary>
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(7)]
    public void EveryItemStepsDownOneIndexWhenTheBufferIsFull(int capacity)
    {
        var buffer = new HistoryBuffer<int>(capacity);

        for (var item = 1; item <= capacity * 3 + 1; item++)
        {
            var before = ReadIndexes(buffer);

            buffer.Add(item);

            var after = ReadIndexes(buffer);

            // Full before the item came, everything steps down one index and the oldest is gone; still filling,
            // every item stands where it stood. Either way the item that came stands at the last index, and the
            // count rose to the capacity and no further.
            Assert.Equal(before.Skip(before.Length == capacity ? 1 : 0).Append(item), after);
            Assert.Equal(item, after[^1]);
            Assert.Equal(Math.Min(item, capacity), buffer.Count);
        }
    }

    /// <summary>The items the buffer keeps, read from the first index to the last and walked in the same breath, are
    /// the last <c>capacity</c> of the items written, in the order they were written: a buffer still filling keeps all
    /// of them, a buffer full keeps exactly its capacity of them, and a buffer whose wrap point has crossed the end
    /// of what it holds two and three times over keeps the one unbroken run that ends with the last item written -
    /// no item of it said twice and none left out.</summary>
    [Theory]
    [InlineData(1, 1, new[] { 1 })]
    [InlineData(1, 7, new[] { 7 })]
    [InlineData(2, 2, new[] { 1, 2 })]
    [InlineData(3, 5, new[] { 3, 4, 5 })]
    [InlineData(4, 10, new[] { 7, 8, 9, 10 })]
    [InlineData(5, 13, new[] { 9, 10, 11, 12, 13 })]
    [InlineData(6, 20, new[] { 15, 16, 17, 18, 19, 20 })]
    public void TheBufferKeepsTheLastItemsWrittenOldestFirstAcrossTheWrapPoint(int capacity, int written, int[] kept)
    {
        var buffer = Written(capacity, written);

        Assert.Equal(kept, ReadIndexes(buffer));
        Assert.Equal(kept, buffer);
        Assert.Equal(kept, buffer.ToArray());
        Assert.Equal(kept.Length, buffer.Count);
        Assert.Equal(kept.Length, kept.Distinct().Count());
    }

    // ----- the buffer as a list of items nobody changes through

    /// <summary>The buffer stands as a list of items nobody changes through: the count of that list is the buffer's
    /// count and its indexer the buffer's indexer, both read across the wrap point, so a caller that knows nothing of
    /// the buffer but that it is a list reads the whole of it in the order the buffer holds it - by the count and the
    /// indexer, by the walk of it, by <c>ToArray</c>, by the ends <c>Skip</c> and <c>Reverse</c> leave it at and by
    /// the least and greatest of it. And it is a list to the reading and no list at all to anyone who asks to put an
    /// item into it or take one out of it: neither <c>IList</c> nor <c>ICollection</c>, so the read-only list is all
    /// of the buffer such a caller gets hold of.</summary>
    [Fact]
    public void TheBufferStandsAsAListOfItemsNobodyChangesThrough()
    {
        var buffer = Written(4, 10);

        // The count and the indexer read through the list rather than through the buffer, and the whole of it read
        // by a caller that knows nothing of the buffer but that it is a list.
        Assert.Equal(4, ((IReadOnlyList<int>)buffer).Count);
        Assert.Equal(4, ((IReadOnlyCollection<int>)buffer).Count);
        Assert.Equal(7, ((IReadOnlyList<int>)buffer)[0]);
        Assert.Equal(10, ((IReadOnlyList<int>)buffer)[^1]);
        // A caller that knows nothing of the buffer but that it is a list reads the whole of it by the count and the
        // indexer alone - the first index to the last, across the wrap point.
        var readThroughTheList = new int[((IReadOnlyList<int>)buffer).Count];
        for (var index = 0; index < readThroughTheList.Length; index++)
        {
            readThroughTheList[index] = ((IReadOnlyList<int>)buffer)[index];
        }

        Assert.Equal([7, 8, 9, 10], readThroughTheList);
        Assert.Equal([7, 8, 9, 10], (IReadOnlyList<int>)buffer);
        Assert.Equal([7, 8, 9, 10], ((IReadOnlyList<int>)buffer).ToArray());
        Assert.Equal([8, 9, 10], ((IReadOnlyList<int>)buffer).Skip(1));
        Assert.Equal([7, 8, 9], ((IReadOnlyList<int>)buffer).Take(3));
        Assert.Equal([10, 9, 8, 7], ((IReadOnlyList<int>)buffer).Reverse());
        Assert.Equal(7, ((IReadOnlyList<int>)buffer).Min());
        Assert.Equal(10, ((IReadOnlyList<int>)buffer).Max());

        // The buffer as a thing the test holds rather than as a buffer: no list of items and no collection of
        // items, so nobody who holds it gets hold of a way to put an item into it or take one out of it.
        object held = buffer;

        Assert.IsAssignableFrom<IReadOnlyList<int>>(held);
        Assert.False(held is IList<int>);
        Assert.False(held is ICollection<int>);
    }

    // ----- the walk

    /// <summary>The walk says the items the indexes hold, at every capacity whose items cross the end of what the
    /// buffer holds: it ends where the count ends, and stepped by hand it says the item of each index in turn, says
    /// once that it has ended and keeps saying so after that, moving nothing of the buffer on the way - so the same
    /// buffer walked twice, and walked again after another item came, says the walks alike.</summary>
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(6)]
    public void TheWalkSaysTheItemsTheIndexesHoldAndEndsWithThem(int capacity)
    {
        var buffer = Written(capacity, capacity * 3);
        var held = ReadIndexes(buffer);

        Assert.NotEmpty(held);
        Assert.Equal(held, buffer);
        Assert.Equal(held, buffer.ToList());

        var moved = 0;
        var enumerator = buffer.GetEnumerator();
        while (enumerator.MoveNext())
        {
            Assert.Equal(held[moved], enumerator.Current);
            moved++;
        }

        Assert.Equal(buffer.Count, moved);
        Assert.False(enumerator.MoveNext());
        Assert.False(enumerator.MoveNext());

        // The walk moved nothing: the buffer says the items it said before it was walked, and says them again
        // walked a second time.
        Assert.Equal(held, ReadIndexes(buffer));
        Assert.Equal(held, buffer);

        buffer.Add(capacity * 3 + 1);

        Assert.Equal([.. held.Skip(1), capacity * 3 + 1], buffer);
    }

    /// <summary>A buffer held as the sequence nobody knows the type of - the sequence a strip of the page walks - is
    /// walked the same way as a buffer held as itself, empty and full of wrapped items alike: the items it holds,
    /// oldest first, across the end of what it holds, and nothing else.</summary>
    [Fact]
    public void ABufferHeldAsTheUntypedSequenceIsWalkedTheSameWay()
    {
        var empty = new HistoryBuffer<string>(3);
        Assert.Empty((IEnumerable)empty);

        var buffer = new HistoryBuffer<string>(3) { "first", "second", "third", "fourth" };
        IEnumerable untyped = buffer;

        Assert.Equal(["second", "third", "fourth"], untyped.Cast<string>());
        Assert.Equal(["second", "third", "fourth"], (IEnumerable<string>)buffer);
    }

    // ----- the item type, which is nothing at all asked of

    /// <summary>A buffer of one slot keeps the last item and nothing else, whatever the buffer is made of: the
    /// smallest item and the greatest of a number, a number at each end of the number line, a word and a word of no
    /// characters, a state the enum names and a state it does not. The second item takes the slot, the first is gone -
    /// counted once, read once, walked once, and said nowhere in the buffer.</summary>
    [Theory]
    [InlineData(1, 2)]
    [InlineData(0, int.MaxValue)]
    [InlineData(int.MaxValue, int.MinValue)]
    [InlineData(int.MinValue, 0)]
    [InlineData("first", "")]
    [InlineData("", "kept")]
    [InlineData("kept", "kept again")]
    [InlineData(HealthState.Pending, HealthState.Healthy)]
    [InlineData(HealthState.Healthy, (HealthState)(-1))]
    public void ABufferOfOneSlotKeepsTheLastItemOfWhateverItIsMadeOf<T>(T first, T second)
    {
        var buffer = new HistoryBuffer<T>(1);

        buffer.Add(first);

        Assert.Single(buffer);
        Assert.Equal(first, buffer[0]);

        buffer.Add(second);

        Assert.Single(buffer);
        Assert.Equal(1, buffer.Capacity);
        Assert.Equal(second, buffer[0]);
        Assert.Equal([second], buffer.ToArray());
        Assert.DoesNotContain(first, buffer);
    }

    /// <summary>The buffer holds the very instance it was given and not a copy of what it was given: read at its
    /// index it is that instance, read beside another item it is that one and the other the other, and when its turn
    /// comes the instance leaves the buffer - what is kept after the wrap is the instances written last, and the one
    /// dropped is nowhere in what the buffer says of itself.</summary>
    [Fact]
    public void ABufferOfAReferenceHoldsTheInstanceItWasGiven()
    {
        var first = new Holds("the word the buffer holds");
        var second = new Holds("another word");
        var third = new Holds("a third word");
        var buffer = new HistoryBuffer<Holds>(2);

        buffer.Add(first);

        Assert.Same(first, buffer[0]);

        buffer.Add(second);
        buffer.Add(third);

        Assert.Same(second, buffer[0]);
        Assert.Same(third, buffer[1]);
        Assert.DoesNotContain(first, buffer);
        Assert.DoesNotContain(first, buffer.ToArray());
        Assert.Equal([second.Word, third.Word], buffer.Select(item => item.Word));
    }

    /// <summary>The value read out of a buffer of values is the value and not a handle on what the buffer holds:
    /// moved in hand, it leaves the buffer as it was and the buffer says the value it was given again, at the index it
    /// stands at and at the ones about it. And the value the buffer holds is one with a value of the very same parts
    /// the test made apart from it - the buffer compares items by what they are and not by where they stand.</summary>
    [Fact]
    public void AValueReadOutOfABufferIsTheValueAndNotAHandleOnIt()
    {
        var buffer = new HistoryBuffer<Mutable>(2);

        buffer.Add(new Mutable(7));
        buffer.Add(new Mutable(11));

        var read = buffer[0];
        read.Turns = 99;

        Assert.Equal(7, buffer[0].Turns);
        Assert.Equal(11, buffer[1].Turns);
        Assert.True(buffer[0].Equals(new Mutable(7)));
        Assert.False(buffer[0].Equals(new Mutable(8)));
        Assert.Equal([7, 11], buffer.Select(item => item.Turns));
    }

    /// <summary>The none of a reference is an item the buffer keeps - as the page's own <c>Samples</c> hold one, for
    /// a check that reported no telemetry: counted among the items about it, read at its index, walked among them,
    /// and dropped when its turn comes. It is neither a gap nor a refusal, and a buffer whose one item is a none is
    /// not empty: it holds one item, and that item is the none.</summary>
    [Fact]
    public void ANoneIsAnItemTheBufferCountsReadsAndWalks()
    {
        var buffer = new HistoryBuffer<string?>(2);

        buffer.Add("first");
        buffer.Add(null);

        Assert.Equal(2, buffer.Count);
        Assert.Equal("first", buffer[0]);
        Assert.Null(buffer[1]);
        Assert.Equal(["first", null], buffer);
        Assert.Equal(["first", null], (IEnumerable<string?>)ReadIndexes(buffer));
        Assert.Contains(null, buffer);

        buffer.Add("third");

        Assert.Equal(2, buffer.Count);
        Assert.Equal(2, buffer.Capacity);
        Assert.Null(buffer[0]);
        Assert.Equal("third", buffer[1]);
        Assert.Equal([null, "third"], buffer);
        Assert.DoesNotContain("first", buffer);
        Assert.False(buffer.All(item => item is not null));
    }

    /// <summary>A buffer whose only item is a none holds one item: it is not empty, its walk says the none, its
    /// index 0 reads as the none and a buffer of one slot takes the none the same way it takes any other item - and
    /// loses it to the next item just as well.</summary>
    [Fact]
    public void ABufferOfNothingButANoneIsNotEmpty()
    {
        var buffer = new HistoryBuffer<string?>(1);

        buffer.Add(null);

        Assert.NotEmpty(buffer);
        Assert.Null(Assert.Single(buffer));
        Assert.Null(buffer[0]);
        Assert.Equal([null], (IEnumerable<string?>)buffer.ToArray());
        Assert.Equal([null], (IEnumerable<string?>)ReadIndexes(buffer));

        buffer.Add(null);

        Assert.Single(buffer);
        Assert.Null(buffer[0]);
    }

    /// <summary>A value the test moves in hand: the buffer holds the value it is given, and a move of a value read
    /// out of the buffer moves the read and not the buffer.</summary>
    private struct Mutable
    {
        public Mutable(int turns) => Turns = turns;

        public int Turns { get; set; }
    }

    /// <summary>A thing of the reference the buffer holds the instance of.</summary>
    private sealed record Holds(string Word);
}
