namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="EventLog" />, the log the page keeps of the last
/// events it observed, declared in <c>src/Dashboard/Health/DashboardEvents.cs</c>. <see cref="EventDetectorTests" />
/// already pins the log at its default: that <c>DefaultCapacity</c> is 50, that a log made without a capacity keeps
/// the last 50 of 60 written by <c>Add</c>, that <c>Count</c> says 50 when the log is full, and that <c>Newest</c>
/// stands newest first, two events of one moment in the order they were observed. What it leaves open is pinned
/// here: a log made with a capacity of its own, as few as one slot, the capacity that is no capacity refused by
/// name, and the log made for two, full at two and dropping its oldest at three; a log made with nothing in it;
/// <c>Count</c> rising one at a time until it stands at the capacity; <c>Add</c> keeping one event word for word
/// and refusing no event by its own parameter name; <c>Latest</c> - none while the log is empty, the newest by the
/// moment the event carries and not the last written, two events of one moment giving theirs to the one observed
/// first, and standing as the first of <c>Newest</c>; <c>Newest</c> sorting by the moment the event carries rather
/// than by the moment the test wrote it, and standing as a list the log does not move afterwards; and
/// <c>AddRange</c> - nothing changing nothing, the log filled to the brim in order, more than it fits keeping the
/// last, more than it fits after what stood in the log, a lazy sequence walked one event at a time, a sequence
/// that stops midway keeping what it walked, and a none in the middle refused by the name <c>Add</c> gives it.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every moment is one the test brings.
/// </para>
/// </summary>
public class EventLogEdgeCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 20, 6, 0, 0, TimeSpan.Zero);

    /// <summary>An event the test writes itself: one fixed moment a second past the last, and its words where
    /// the test needs them.</summary>
    private static DashboardEvent Event(int second, string text = "") =>
        new(Now.AddSeconds(second), EventKind.Health, EventLevel.Info, "uat", "westus3", text);

    // ----- a log made with a capacity of its own -----

    /// <summary>A capacity below one is no capacity: zero, a negative and the smallest int are all refused, and
    /// the refusal names the argument.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void ACapacityBelowOneIsRefusedNamingTheArgument(int capacity)
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => new EventLog(capacity));

        Assert.Equal("capacity", error.ParamName);
    }

    /// <summary>A log of one slot keeps one: whatever else comes, it holds only the event it holds now.</summary>
    [Fact]
    public void ALogOfOneSlotAlwaysHoldsTheLastWord()
    {
        var log = new EventLog(1);
        log.Add(Event(1, "first"));
        log.Add(Event(2, "second"));
        log.Add(Event(3, "third"));

        Assert.Equal(1, log.Count);
        Assert.Equal("third", Assert.Single(log.Newest).Text);
    }

    /// <summary>The boundary of a log of two: full at two, keeping both newest first, and dropping the oldest at
    /// three without growing.</summary>
    [Fact]
    public void ALogOfTwoSlotsIsFullAtTwoAndDropsItsOldestAtThree()
    {
        var log = new EventLog(2);
        log.Add(Event(1, "first"));
        log.Add(Event(2, "second"));

        Assert.Equal(2, log.Count);
        Assert.Equal(["second", "first"], log.Newest.Select(entry => entry.Text));

        log.Add(Event(3, "third"));

        Assert.Equal(2, log.Count);
        Assert.Equal(["third", "second"], log.Newest.Select(entry => entry.Text));
    }

    // ----- Count -----

    /// <summary>A log made with nothing in it has no events: the one made without a capacity, and the ones made
    /// with one of their own.</summary>
    [Fact]
    public void AFreshLogHasNoEventsWhateverItsCapacity()
    {
        Assert.Equal(0, new EventLog().Count);
        Assert.Equal(0, new EventLog(1).Count);
        Assert.Equal(0, new EventLog(3).Count);
    }

    /// <summary>The count rises one at a time while the log is not full, and stands at the capacity from then
    /// on, however many more events come.</summary>
    [Fact]
    public void TheCountRisesOneAtATimeAndStandsAtTheCapacity()
    {
        var log = new EventLog(3);

        for (var second = 1; second <= 5; second++)
        {
            log.Add(Event(second));
            Assert.Equal(Math.Min(second, 3), log.Count);
        }
    }

    // ----- Add -----

    /// <summary>No event is no event: the refusal names the argument, and the log stands as it stood.</summary>
    [Fact]
    public void AddRefusesNoEventNamingTheArgumentAndKeepsTheLogAsItStood()
    {
        var log = new EventLog(2);
        log.Add(Event(1, "first"));
        log.Add(Event(2, "second"));

        var error = Assert.Throws<ArgumentNullException>(() => log.Add(null!));

        Assert.Equal("entry", error.ParamName);
        Assert.Equal(2, log.Count);
        Assert.Equal(["second", "first"], log.Newest.Select(entry => entry.Text));
    }

    /// <summary>A log keeps the event it is given word for word: its moment, its kind, its level, its
    /// environment, its node and its words, each one held and not a one of them chosen.</summary>
    [Fact]
    public void AddKeepsTheEventWordForWord()
    {
        var log = new EventLog();
        var event_ = new DashboardEvent(Now, EventKind.Version, EventLevel.Good, "prod", "Front Door", "ui: 2.4.14 → 2.4.15, deployed");
        log.Add(event_);

        var held = Assert.Single(log.Newest);
        Assert.Equal(event_, held);
        Assert.Equal(Now, held.At);
        Assert.Equal(EventKind.Version, held.Kind);
        Assert.Equal(EventLevel.Good, held.Level);
        Assert.Equal("prod", held.Environment);
        Assert.Equal("Front Door", held.Node);
        Assert.Equal("ui: 2.4.14 → 2.4.15, deployed", held.Text);
    }

    // ----- Latest -----

    /// <summary>A log made with nothing in it has no newest event, whatever its capacity: the answer is null, and
    /// the empty log says it with its count, the same way a log whose newest event is none would say it.</summary>
    [Fact]
    public void AFreshLogHasNoLatestWhateverItsCapacity()
    {
        Assert.Null(new EventLog().Latest);
        Assert.Null(new EventLog(1).Latest);
        Assert.Null(new EventLog(3).Latest);
    }

    /// <summary>Latest is the newest by the moment the event carries, not the last written: an event written last
    /// whose moment is the earliest is not the newest, and an event written first whose moment is the latest is.</summary>
    [Fact]
    public void LatestIsTheNewestByTheMomentTheEventCarriesNotTheLastWritten()
    {
        var log = new EventLog(3);
        log.Add(Event(3, "third"));
        log.Add(Event(1, "first"));
        log.Add(Event(2, "second"));

        Assert.Equal("third", log.Latest!.Text);
    }

    /// <summary>Two events of one moment give their newest to the one observed first, the one the page reads first:
    /// the moment does not tell them apart, the order observed does.</summary>
    [Fact]
    public void TwoEventsOfOneMomentGiveTheirLatestToTheOneObservedFirst()
    {
        var log = new EventLog(3);
        log.Add(new DashboardEvent(Now, EventKind.Traffic, EventLevel.Info, "uat", null, "first seen"));
        log.Add(new DashboardEvent(Now, EventKind.Cluster, EventLevel.Info, "uat", "cluster", "second seen"));

        Assert.Equal("first seen", log.Latest!.Text);
    }

    /// <summary>Latest stands as the first of Newest: the one event a reader takes by name is the one the page
    /// reads first, whatever the log holds, and both are none while the log is empty.</summary>
    [Fact]
    public void LatestStandsAsTheFirstOfNewest()
    {
        var log = new EventLog(3);
        Assert.Null(log.Latest);
        Assert.Empty(log.Newest);

        log.Add(Event(2, "second"));
        log.Add(Event(3, "third"));
        log.Add(Event(1, "first"));

        Assert.Same(log.Newest[0], log.Latest);
        Assert.Equal("third", log.Latest!.Text);
    }

    /// <summary>The newest the log still keeps, after it has dropped: a log of two that took a third keeps the two
    /// newest it holds, and Latest is the first of them, not the one it dropped.</summary>
    [Fact]
    public void LatestIsTheNewestTheLogStillKeepsAfterItDrops()
    {
        var log = new EventLog(2);
        log.Add(Event(1, "first"));
        log.Add(Event(2, "second"));
        log.Add(Event(3, "third"));

        Assert.Equal(2, log.Count);
        Assert.Equal("third", log.Latest!.Text);
        Assert.Equal(["third", "second"], log.Newest.Select(entry => entry.Text));
    }

    // ----- Newest -----

    /// <summary>Newest is sorted by the moment the event carries, not by the moment it was written: an event
    /// written last whose moment is the earliest stands last.</summary>
    [Fact]
    public void NewestSortsByTheMomentTheEventCarriesNotByWhenItWasWritten()
    {
        var log = new EventLog(3);
        log.Add(Event(3, "third"));
        log.Add(Event(1, "first"));
        log.Add(Event(2, "second"));

        Assert.Equal(["third", "second", "first"], log.Newest.Select(entry => entry.Text));
    }

    /// <summary>Newest stands as a list the log does not move afterwards: what was read stays as it was read
    /// while the log goes on, and a new reading is a new list.</summary>
    [Fact]
    public void NewestStandsAsAListTheLogDoesNotMoveAfterwards()
    {
        var log = new EventLog(3);
        log.Add(Event(1, "first"));
        log.Add(Event(2, "second"));
        log.Add(Event(3, "third"));

        var held = log.Newest;
        log.Add(Event(4, "four"));

        Assert.Equal(["third", "second", "first"], held.Select(entry => entry.Text));
        Assert.Equal(["four", "third", "second"], log.Newest.Select(entry => entry.Text));
        Assert.NotSame((object)held, (object)log.Newest);
    }

    // ----- AddRange -----

    /// <summary>No sequence is no sequence: the refusal names the argument, and the log stands as it stood.</summary>
    [Fact]
    public void AddRangeRefusesNoSequenceNamingTheArgumentAndKeepsTheLogAsItStood()
    {
        var log = new EventLog(2);
        log.Add(Event(1, "first"));

        var error = Assert.Throws<ArgumentNullException>(() => log.AddRange(null!));

        Assert.Equal("entries", error.ParamName);
        Assert.Equal(1, log.Count);
        Assert.Equal(["first"], log.Newest.Select(entry => entry.Text));
    }

    /// <summary>The empty changes nothing: an empty array and a sequence that yields no events both leave a
    /// fresh log with no events in it.</summary>
    [Fact]
    public void AddRangeOfNothingChangesNothing()
    {
        var log = new EventLog(2);
        log.AddRange([]);
        log.AddRange(Enumerable.Empty<DashboardEvent>());

        Assert.Equal(0, log.Count);
        Assert.Empty(log.Newest);
    }

    /// <summary>A range that fits exactly fills the log to the brim, in the order it was read: the newest of the
    /// range stands first.</summary>
    [Fact]
    public void AddRangeFillsALogToTheBrimInOrder()
    {
        var log = new EventLog(3);
        log.AddRange([Event(1, "first"), Event(2, "second"), Event(3, "third")]);

        Assert.Equal(3, log.Count);
        Assert.Equal(["third", "second", "first"], log.Newest.Select(entry => entry.Text));
    }

    /// <summary>More than the log fits keeps the last of the range, newest first: two and a half times over, the
    /// first four of seven are gone and the last three stand.</summary>
    [Fact]
    public void AddRangeKeepsTheLastOfMoreThanItFits()
    {
        var log = new EventLog(3);
        log.AddRange(Enumerable.Range(1, 7).Select(second => Event(second, $"word {second}")));

        Assert.Equal(3, log.Count);
        Assert.Equal(["word 7", "word 6", "word 5"], log.Newest.Select(entry => entry.Text));
    }

    /// <summary>A range that overflows drops what stood in the log too: the event written before the range is
    /// as old as any, and older.</summary>
    [Fact]
    public void AddRangeDropsWhatStoodInTheLogWhenMoreComesThanItFits()
    {
        var log = new EventLog(3);
        log.Add(Event(0, "standing"));
        log.AddRange([Event(1, "first"), Event(2, "second"), Event(3, "third")]);

        Assert.Equal(3, log.Count);
        Assert.Equal(["third", "second", "first"], log.Newest.Select(entry => entry.Text));
    }

    /// <summary>A range that has not been walked yet is walked one event at a time: a lazy sequence, nothing
    /// copied ahead of the walk, fills the log in the order it yields.</summary>
    [Fact]
    public void AddRangeWalksALazySequenceOneEventAtATime()
    {
        var log = new EventLog(4);
        log.AddRange(Enumerable.Range(1, 4).Select(second => Event(second, $"lazy {second}")));

        Assert.Equal(4, log.Count);
        Assert.Equal(["lazy 4", "lazy 3", "lazy 2", "lazy 1"], log.Newest.Select(entry => entry.Text));
    }

    /// <summary>A range that stops midway throws where it stops, and keeps what it already walked: two events
    /// of three are in the log, and the third never came.</summary>
    [Fact]
    public void AddRangeOfASequenceThatStopsMidwayKeepsWhatItAlreadyWalked()
    {
        var log = new EventLog(4);

        var error = Assert.Throws<InvalidOperationException>(() => log.AddRange(StopsAfterTwo(
            Event(1, "first"), Event(2, "second"), Event(3, "third"))));

        Assert.Equal("the sequence stopped", error.Message);
        Assert.Equal(2, log.Count);
        Assert.Equal(["second", "first"], log.Newest.Select(entry => entry.Text));
    }

    /// <summary>A none in the middle of the range is no event: it is refused by the name <c>Add</c> gives it,
    /// and the events walked before it stand in the log.</summary>
    [Fact]
    public void AddRangeRefusesANoneInTheRangeNamingTheEntryAndKeepsWhatItWalked()
    {
        var log = new EventLog(4);

        var error = Assert.Throws<ArgumentNullException>(() => log.AddRange([Event(1, "first"), null!]));

        Assert.Equal("entry", error.ParamName);
        Assert.Equal(1, log.Count);
        Assert.Equal(["first"], log.Newest.Select(entry => entry.Text));
    }

    /// <summary>The events a range brings are kept word for word: three different kinds, levels, environments
    /// and words, each held as it was written, newest first.</summary>
    [Fact]
    public void AddRangeKeepsEachEventWordForWord()
    {
        var log = new EventLog(3);
        List<DashboardEvent> events = [
            new DashboardEvent(Now, EventKind.Traffic, EventLevel.Info, "uat", null, "Traffic started"),
            new DashboardEvent(Now.AddSeconds(1), EventKind.Pinned, EventLevel.Info, "uat", "ui", "ui: pinned 2.4.14 → 2.4.15 in Git"),
            new DashboardEvent(Now.AddSeconds(2), EventKind.Serving, EventLevel.Warning, "uat", "ui", "Failover: westus3 → eastus2."),
        ];
        log.AddRange(events);

        Assert.Equal([events[2], events[1], events[0]], log.Newest);
    }

    /// <summary>A sequence of three events that stops where it stops: two yielded, then the stop.</summary>
    private static IEnumerable<DashboardEvent> StopsAfterTwo(params DashboardEvent[] events)
    {
        foreach (var entry in events.Take(2))
        {
            yield return entry;
        }

        throw new InvalidOperationException("the sequence stopped");
    }
}
