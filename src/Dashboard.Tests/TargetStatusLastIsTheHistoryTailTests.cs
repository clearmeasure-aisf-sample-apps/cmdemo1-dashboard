namespace Dashboard.Tests;

/// <summary>
/// <see cref="TargetStatus.Last" /> as the one check it is: the newest entry of <see cref="TargetStatus.History" />,
/// read through it rather than kept beside it. <see cref="TargetStatus.Record" /> writes a check once, at the end of
/// the history, so the two homes the page reads - the tile's own <c>Last</c> and the tail of its <c>History</c> - are
/// one fact, and they cannot drift: the history holds the last <see cref="TargetStatus.HistoryLength" /> checks and so
/// ends with the check written last whatever it dropped at the other end to make room.
/// <para>
/// What the readers of that fact are pinned to see is pinned here, at the three places the page reads it: the tile
/// itself (<see cref="TargetStatus.State" />, <see cref="TargetStatus.ToNodeHealth()" /> and
/// <see cref="TargetStatus.ToNodeVersion()" />, which are the last check's state said again), the events
/// (<see cref="NodeObservation.Of" />, which hands the last check to <see cref="EventDetector" /> as the thing that
/// changed) and the diagram (<see cref="RuntimePayloadBuilder" />, whose tile states its facts, its tooltip and
/// its strip of states from that same check). Every one of them is pinned to the tail of the history, so a change to
/// one home of the check cannot leave the others reading the other.
/// </para>
/// <para>
/// The edge the two homes used to mean different things at is pinned too: the drop at
/// <see cref="TargetStatus.HistoryLength" />. A tile checked past the length of its history has lost its earliest
/// checks and still stands at the check it answered last - the count of the history holds at the length while
/// <c>Last</c> steps to the check that came - and a tile nobody ever checked has no last check and an empty history,
/// which is the same nothing to every reader and not a state of its own (a check that answered
/// <see cref="HealthState.Pending" /> shares the state of a tile nobody asked and is known apart by its last check
/// being there). <see cref="TargetStatusTests" /> and <see cref="TargetStatusEdgeCaseTests" /> pin the tile's own
/// readings and the history's own length; <see cref="HistoryBufferLastTests" /> pins the newest item of the ring this
/// reads through; what is added here is that the two are the one check, before the first check, at every check, past
/// the drop, and at every reader.
/// </para>
/// </summary>
public class TargetStatusLastIsTheHistoryTailTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private static readonly Uri West = new("https://app-west.example.net");
    private static readonly Uri Page = new("http://localhost:5210/");

    private static ProbeResult Check(HealthState state, int second, string? version = null) =>
        new(state, state == HealthState.Unreachable ? null : 200, state == HealthState.Unreachable ? null : 41, Start.AddSeconds(second), version);

    private static TargetStatus Node() => new(TargetKind.Node, "app-west", West, "westus3", "primary");

    // ----- the tile and its own readings

    /// <summary>A tile nobody checked has no last check and no history: <see langword="null" /> read as the last
    /// check, an empty history, and the state neither of them can say on their own - pending, which a tile that was
    /// never asked and a check that said "not yet" share. Every reading of the tile answers the nothing, and the
    /// first check fills both ends of it at once.</summary>
    [Fact]
    public void ATileNobodyCheckedHasNoLastCheckAndNoHistory()
    {
        var tile = Node();

        Assert.Null(tile.Last);
        Assert.Empty(tile.History);
        Assert.Equal(HealthState.Pending, tile.State);
        Assert.Equal(HealthState.Pending, tile.ToNodeHealth().State);
        Assert.Equal(HealthState.Pending, tile.ToNodeVersion().State);
        Assert.Null(tile.Version);
    }

    /// <summary>The last check is the tail of the history at every check of a tile: the same instance the check was
    /// handed as, at the last index of what the history keeps, before the history is full, at the very check that
    /// fills it, and at every check past that - through the drop of the oldest check, which moves the tail about the
    /// ring and leaves the tile standing at the check it answered last. The count of the history holds at the length
    /// past it while the last check keeps stepping, and the state, the health and the version the tile says are that
    /// one check's.</summary>
    [Fact]
    public void TheLastCheckIsTheHistoryTailAtEveryCheckAndAcrossTheDrop()
    {
        var tile = Node();

        for (var second = 0; second <= TargetStatus.HistoryLength + 10; second++)
        {
            var expected = Check(second % 2 == 0 ? HealthState.Healthy : HealthState.Unhealthy, second, $"2.4.{second}");
            tile.Record(expected);

            var tail = tile.History[tile.History.Count - 1];

            Assert.Same(expected, tile.Last);
            Assert.Same(expected, tail);
            Assert.Same(tile.Last, tile.History[^1]);
            Assert.Equal(Math.Min(second + 1, TargetStatus.HistoryLength), tile.History.Count);
            Assert.Equal(expected.State, tile.State);
            Assert.Equal(expected.State, tile.ToNodeHealth().State);
            Assert.Equal(expected.State, tile.ToNodeVersion().State);
            Assert.Equal(expected.Version, tile.Version);
        }

        // Past the length of its history the tile has lost its earliest checks and still stands at its last one.
        Assert.Equal(TargetStatus.HistoryLength, tile.History.Count);
        Assert.Equal(Start.AddSeconds(TargetStatus.HistoryLength + 10), tile.Last!.CheckedAt);
        Assert.Equal(Start.AddSeconds(11), tile.History[0].CheckedAt);
    }

    /// <summary>A tile that was never asked and a tile whose one check said <see cref="HealthState.Pending" /> are the
    /// same state and two different tiles, and what tells them apart is the last check being there or not - which is
    /// the same as the history having a tail or not: the pending check stands at the end of the history as any other
    /// check does, so the two readers of the one fact say the one thing about it.</summary>
    [Fact]
    public void APendingCheckIsAtTheEndOfTheHistoryAsAnyOtherCheckIs()
    {
        var never = Node();
        var pending = Node();
        var answer = new ProbeResult(HealthState.Pending, null, null, Start);

        pending.Record(answer);

        Assert.Equal(never.State, pending.State);
        Assert.Null(never.Last);
        Assert.Same(answer, pending.Last);
        Assert.Same(answer, pending.History[^1]);
        Assert.Empty(never.History);
        Assert.Single(pending.History);
    }

    // ----- the events' reading of the last check

    /// <summary>The observation the events are decided from carries the tile's last check, and carries the tail of the
    /// tile's history: what <see cref="EventDetector" /> compares from one check to the next is that one instance,
    /// before the first check (nothing to compare) and past the drop of the oldest check alike.</summary>
    [Fact]
    public void TheObservationTheEventsAreDecidedFromCarriesTheHistoryTail()
    {
        var tile = Node();

        var before = NodeObservation.Of(tile);

        Assert.Null(before.Last);
        Assert.Empty(tile.History);

        for (var second = 0; second <= TargetStatus.HistoryLength + 3; second++)
        {
            tile.Record(Check(HealthState.Healthy, second));
        }

        var after = NodeObservation.Of(tile);

        Assert.Same(tile.Last, after.Last);
        Assert.Same(tile.History[^1], after.Last);
        Assert.Equal(tile.State, after.State);
        Assert.Equal(tile.History[^1].CheckedAt, after.Last!.CheckedAt);
    }

    // ----- the diagram's reading of the last check

    /// <summary>The diagram a tile is drawn into is drawn from that same check: its facts are the HTTP status and
    /// latency of the history's tail, its tooltip says the clock of the tail, and its strip of states is the history -
    /// the whole thirty of them past the drop, ending at the tail the facts are read from.</summary>
    [Fact]
    public void TheDiagramIsDrawnFromTheHistoryTail()
    {
        var (primary, tile) = UatChecked(TargetStatus.HistoryLength + 10);
        var tail = primary.History[^1];

        Assert.Equal("HTTP 503 · 41 ms", tile.Facts);
        Assert.Equal(HealthState.Unhealthy, tail.State);
        Assert.Same(tail, primary.Last);
        Assert.Contains($"Last check {TimeText.Clock(tail.CheckedAt, TimeZoneInfo.Utc)}", tile.Title, StringComparison.Ordinal);
        Assert.Equal(TargetStatus.HistoryLength, tile.History!.Count);
        Assert.Equal("unhealthy", tile.History[^1]);
        Assert.Equal(TargetStatus.HistoryLength - 1, tile.History.Count(state => state == "healthy"));
    }

    /// <summary>A node the page has not checked yet is drawn as "not checked yet" with no strip of checks at all, and
    /// nothing else about the diagram changes for it: the reader of the last check sees the same nothing the tile
    /// sees, which is the one edge at which the two homes of the check used to have to agree by hand.</summary>
    [Fact]
    public void ANodeNobodyCheckedIsDrawnAsNotCheckedYet()
    {
        var (primary, tile) = UatChecked(0);

        Assert.Empty(primary.History);
        Assert.Null(primary.Last);
        Assert.Equal("not checked yet", tile.Facts);
        Assert.Empty(tile.History!);
        Assert.Equal("checking", tile.State);
    }

    /// <summary>The uat diagram of a primary node checked <paramref name="checks" /> times, healthy until the last
    /// check, which answers HTTP 503: the tile of that node as the payload builder draws it.</summary>
    private static (TargetStatus Primary, RuntimeTile Tile) UatChecked(int checks)
    {
        var topology = TopologyParser.Parse(WebAppDiagram.Topology).Topology!;
        var manifest = RuntimeManifestParser.ParseManifest(WebAppDiagram.Sample("uat.json")).Value!;
        var environment = new EnvironmentStatus(topology.Environments.Single(entry => entry.Name == "uat"));
        var primary = environment.Deployables[0].Nodes.Single(node => node.Role == NodeInfo.PrimaryRole);

        for (var second = 0; second < checks; second++)
        {
            var last = second == checks - 1;
            primary.Record(new ProbeResult(
                last ? HealthState.Unhealthy : HealthState.Healthy,
                last ? 503 : 200,
                41,
                Start.AddSeconds(second),
                "2.4.21"));
        }

        var payload = RuntimePayloadBuilder.Build(manifest, environment, Page, TimeZoneInfo.Utc);

        return (primary, payload.Nodes.Single(node => node.Alias == "app_ui_primary"));
    }
}
