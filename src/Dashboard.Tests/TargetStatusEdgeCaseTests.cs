namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="TargetStatus" />, one tile of the dashboard: an
/// endpoint of a deployable, the checks it has answered and the words of the topology it was built from, declared in
/// <c>src/Dashboard/Health/TargetStatus.cs</c>.
/// <para>
/// What is pinned where it belongs is left alone. <see cref="TargetStatusTests" /> pins that a new tile is
/// <see cref="HealthState.Pending" /> with nothing behind it, that the state is the last check's, that the history
/// holds the last thirty checks oldest first, that a node which stops answering keeps its last known version, and
/// which end of a deployable a tile is built for; <see cref="TargetKindEdgeCaseTests" /> pins
/// <see cref="TargetKind" /> itself, <see cref="TargetKindBoundaryEdgeCaseTests" /> the kinds combined, stepped and
/// absent, and the kind a tile carries; <see cref="TrendTests" /> pins
/// <see cref="TargetStatus.RecordTelemetry" />, <see cref="TargetStatus.Samples" /> and
/// <see cref="TargetStatus.Telemetry" /> - the gap a round
/// without a reading leaves once the node has reported, the nothing before the first report and the sixty readings
/// kept; <see cref="HealthHelperCoverageTests" /> pins what <see cref="NodeObservation" /> makes of a tile and that a
/// detailed check which stops answering leaves <see cref="TargetStatus.LastReadHealthDetail" /> standing;
/// <see cref="HistoryBufferTests" /> pins the ring buffer the history is; <see cref="NodeInfoEdgeCaseTests" /> and
/// <see cref="NodeHealthEdgeCaseTests" /> pin the node the tile is built from and the health it is turned into;
/// <see cref="LinkTests" /> pins the links out of the topology file, and
/// <see cref="TargetTileClusterCardEnvironmentDeliveryTests" /> and <see cref="ViewLinksTests" /> the markup that
/// reads a tile's <see cref="TargetStatus.Links" />; and the deployable and the environment the same file
/// declares are pinned by
/// <see cref="TargetStatusTests" />, <see cref="RotationTests" /> and the monitor's own tests.
/// </para>
/// <para>
/// What all of that leaves open, and what follows: the five parts and the links a tile is built with, kept as the
/// words and the instance they were handed - at their ends, the empty word, the blank and the part nobody named -
/// and the fact that the tile validates none of them; <see cref="TargetStatus.IsPrimary" /> read off the role
/// alone, in every
/// case, with the padded word, the near word and no word outside it; the history at the length exactly, one under it
/// and one past it; a check that is not there refused by name; the state of a check that says
/// <see cref="HealthState.Pending" /> next to the state of no check at all; a version kept as the node said it, the
/// empty word included; the three recordings each touching their own part of the tile and no other; a detailed check
/// that answered with no entries at all being an answer; <see cref="TargetStatus.TrendOf" /> refusing a number
/// that is not there,
/// never asking a selector for readings a tile does not hold, and forwarding the words of its title; and the two
/// records the tile is asked for, <see cref="TargetStatus.ToNodeHealth" /> standing still while the tile moves and
/// <see cref="TargetStatus.ToNodeVersion" /> labelling a node that has no region by its own name.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a tile holds no timer and calls
/// nothing, and every moment below - the <see cref="ProbeResult.CheckedAt" /> of a check, the
/// <see cref="HealthDetail.ReadAt" /> of a detailed answer, the <see cref="TelemetrySnapshot.ReadAt" /> of a reading -
/// is one the test brought.
/// </para>
/// </summary>
public class TargetStatusEdgeCaseTests
{
    /// <summary>The moment every check below was made from, so that no test asks the clock.</summary>
    private static readonly DateTimeOffset Start = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>An address no check is made to: the one every tile is built with.</summary>
    private static readonly Uri West = new("https://app-west.example.net");

    private static readonly Uri Portal = new("https://portal.example.net/#app-west");
    private static readonly Uri LiveMetrics = new("https://portal.example.net/#live");

    /// <summary>A check of the state named, at the second named, reporting the version named - and reporting no
    /// status and no latency when it got no answer at all, as a check of an unreachable node is made.</summary>
    private static ProbeResult Result(HealthState state, int second = 0, string? version = null) =>
        new(state, state == HealthState.Unreachable ? null : 200, state == HealthState.Unreachable ? null : 12, Start.AddSeconds(second), version);

    /// <summary>A node's tile of the region and the role named, the name and the address standing apart from both.</summary>
    private static TargetStatus Node(string? region = "westus3", string? role = "primary") =>
        new(TargetKind.Node, "app-west", West, region, role);

    /// <summary>A reading of the traffic named, every other number of it zero - the least a round can report.</summary>
    private static TelemetrySnapshot Sample(int requests) => new(requests, requests, 0, 0, null, 0, 0, 0, null, 0, Start);

    // ----- the parts a tile is built with

    /// <summary>The five parts of the topology stand on the tile as they were handed, the address being the very
    /// instance it was built with rather than a copy of it, and none of them moved by a check the endpoint goes on to
    /// answer: the tile is the place the words of the file and the answers of the network are kept together, and it
    /// rewrites neither.</summary>
    [Fact]
    public void ThePartsOfTheTileStandAsTheTopologyWroteThem()
    {
        var links = new LinkSet([new KeyValuePair<string, Uri>(LinkSet.Portal, Portal)]);
        var tile = new TargetStatus(TargetKind.Node, "app-west", West, "westus3", "primary", links);

        Assert.Equal("app-west", tile.Name);
        Assert.Same(West, tile.Url);
        Assert.Equal(West, tile.Url);
        Assert.Equal("westus3", tile.Region);
        Assert.Equal("primary", tile.Role);
        Assert.Same(links, tile.Links);

        // A check answers the endpoint; it says nothing about who the endpoint is.
        tile.Record(Result(HealthState.Unreachable, 30));

        Assert.Equal("app-west", tile.Name);
        Assert.Same(West, tile.Url);
        Assert.Equal("westus3", tile.Region);
        Assert.Equal("primary", tile.Role);
        Assert.Same(links, tile.Links);
        Assert.Equal(TargetKind.Node, tile.Kind);
    }

    /// <summary>A tile validates none of the words it is built with, because the file that wrote them already did:
    /// the empty word is a name and a region, a blank is a blank kept whole, and a part may even be handed as nothing
    /// to a property the class does not say is absent. The optional three stand as nothing when nobody names them,
    /// which is what a Front Door tile - an address with no region, no role and no node behind it - looks like.</summary>
    [Fact]
    public void ATileKeepsTheWordsItIsGivenEvenWhenTheySayNothing()
    {
        var empty = new TargetStatus(TargetKind.Node, string.Empty, West);

        Assert.Equal(string.Empty, empty.Name);
        Assert.Null(empty.Region);
        Assert.Null(empty.Role);
        Assert.False(empty.IsPrimary);

        var blank = new TargetStatus(TargetKind.Node, " ", West, " ", " ");

        Assert.Equal(" ", blank.Name);
        Assert.Equal(" ", blank.Region);
        Assert.Equal(" ", blank.Role);
        Assert.Equal(" ", blank.ToNodeVersion().Label);
        Assert.False(blank.IsPrimary);

        // Nothing was refused: the parts that are not there are the parts that were not given.
        var nothing = new TargetStatus(TargetKind.Node, null!, null!);

        Assert.Null(nothing.Name);
        Assert.Null(nothing.Url);
        Assert.Null(nothing.Region);
        Assert.Null(nothing.Role);
        Assert.False(nothing.IsPrimary);
        Assert.Null(nothing.Last);
        Assert.Equal(HealthState.Pending, nothing.State);
    }

    // ----- the role that makes a tile a primary

    /// <summary>The one word <see cref="NodeInfo.PrimaryRole" /> is what a tile is primary for, in any case and in
    /// that case only: the comparison is by sense of case and by nothing else, so the word padded with a space is not
    /// the word, the word with something stuck to either end is not the word, the empty word and the blank are no
    /// word, and no word at all is no word - and every one of them stands outside the primary.</summary>
    [Theory]
    [InlineData("primary", true)]
    [InlineData("Primary", true)]
    [InlineData("PRIMARY", true)]
    [InlineData("pRiMaRy", true)]
    [InlineData("standby", false)]
    [InlineData("STANDBY", false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData(" primary", false)]
    [InlineData("primary ", false)]
    [InlineData("primaryly", false)]
    [InlineData("nonprimary", false)]
    [InlineData("pri", false)]
    [InlineData(null, false)]
    public void ATileIsPrimaryForTheWordOfItsRoleInAnyCaseAndForNothingElse(string? role, bool expected)
    {
        Assert.Equal(expected, Node(role: role).IsPrimary);
    }

    /// <summary>The role alone decides: the two words the topology is written with decide it both ways, a tile built
    /// with no role at all is not primary, and neither the end of the deployable the tile stands for, nor the region
    /// it is in, nor the state its last check left it in has a say.</summary>
    [Fact]
    public void IsPrimaryReadsTheRoleAndNothingElse()
    {
        Assert.True(Node(role: NodeInfo.PrimaryRole).IsPrimary);
        Assert.False(Node(role: NodeInfo.StandbyRole).IsPrimary);
        Assert.False(new TargetStatus(TargetKind.Node, "app", West).IsPrimary);
        Assert.False(new TargetStatus(TargetKind.FrontDoor, "Front Door", West).IsPrimary);

        var tile = new TargetStatus(TargetKind.Node, "app-west", West, region: null, role: "PRIMARY");

        Assert.True(tile.IsPrimary);
        Assert.Null(tile.Region);

        // The state of the endpoint is its last check's, and says nothing about the part it plays.
        tile.Record(Result(HealthState.Unreachable, 30));

        Assert.True(tile.IsPrimary);
        Assert.Equal(HealthState.Unreachable, tile.State);
        Assert.True(tile.ToNodeHealth().IsPrimary);
    }

    // ----- the links a tile's numbers lead to

    /// <summary>A tile nobody gave links to carries the empty set - the one shared instance of it, which is why a
    /// thousand tiles of a plain topology hold one set rather than a thousand - and a tile given a set carries that
    /// very set, keys and all. A key the file does not hold is answered with nothing rather than with an error, by
    /// its exact spelling, and an empty set the caller made itself is just as empty as the shared one and is kept as
    /// it was given.</summary>
    [Fact]
    public void ATileWithoutLinksCarriesTheEmptySetAndOneWithThemTheSetItWasGiven()
    {
        var bare = Node();

        Assert.Same(LinkSet.None, bare.Links);
        Assert.Empty(bare.Links.Keys);
        Assert.Equal(0, bare.Links.Count);
        Assert.Null(bare.Links[LinkSet.Portal]);
        Assert.Same(Node().Links, bare.Links);

        var links = new LinkSet([
            new KeyValuePair<string, Uri>(LinkSet.Portal, Portal),
            new KeyValuePair<string, Uri>(LinkSet.LiveMetrics, LiveMetrics),
        ]);
        var tile = new TargetStatus(TargetKind.Node, "app-west", West, "westus3", "primary", links);

        Assert.Same(links, tile.Links);
        Assert.Equal(2, tile.Links.Count);
        Assert.Equal([LinkSet.Portal, LinkSet.LiveMetrics], tile.Links.Keys);
        Assert.Equal(Portal, tile.Links[LinkSet.Portal]);
        Assert.Equal(LiveMetrics, tile.Links[LinkSet.LiveMetrics]);
        Assert.Null(tile.Links[LinkSet.Performance]);
        Assert.Null(tile.Links["Portal"]);
        Assert.Null(tile.Links[string.Empty]);

        // An empty set of links is an empty set, whoever made it - and not the shared instance.
        var empty = new LinkSet([]);
        var given = new TargetStatus(TargetKind.Node, "app-west", West, links: empty);

        Assert.NotSame(LinkSet.None, empty);
        Assert.Same(empty, given.Links);
        Assert.Equal(0, given.Links.Count);
        Assert.Null(given.Links[LinkSet.Portal]);
    }

    // ----- the history of a tile's own checks

    /// <summary>The history of a tile is a ring of exactly <see cref="TargetStatus.HistoryLength" /> places, and the
    /// three moments around that number are the three a reader of a sparkline of checks cares about: one under the
    /// length it holds every check and none the less, at the length it still holds every check oldest first, and one
    /// past it the first check is gone, the rest have stepped one place forward and the count has not moved. The
    /// checks kept are the checks that were made, in the order they were made, whatever the state of each.</summary>
    [Fact]
    public void TheHistoryHoldsItsOwnLengthOfChecksAndDropsTheOldestOnePastThem()
    {
        var tile = Node();

        Assert.Equal(30, TargetStatus.HistoryLength);
        Assert.Equal(TargetStatus.HistoryLength, tile.History.Capacity);
        Assert.Empty(tile.History);

        for (var second = 0; second < TargetStatus.HistoryLength - 1; second++)
        {
            tile.Record(Result(HealthState.Healthy, second));
        }

        Assert.Equal(29, tile.History.Count);
        Assert.Equal(Start, tile.History[0].CheckedAt);
        Assert.Equal(Start.AddSeconds(28), tile.History[28].CheckedAt);

        // The length exactly: nothing has been dropped yet.
        tile.Record(Result(HealthState.Healthy, 29));

        Assert.Equal(30, tile.History.Count);
        Assert.Equal(Start, tile.History[0].CheckedAt);
        Assert.Equal(Start.AddSeconds(29), tile.History[^1].CheckedAt);

        // One past it: the oldest goes, and the count says the length again.
        tile.Record(Result(HealthState.Unreachable, 30));

        Assert.Equal(30, tile.History.Count);
        Assert.Equal(Start.AddSeconds(1), tile.History[0].CheckedAt);
        Assert.Equal(Start.AddSeconds(30), tile.History[^1].CheckedAt);
        Assert.DoesNotContain(Start, tile.History.Select(result => result.CheckedAt));
        Assert.Equal(HealthState.Unreachable, tile.History[^1].State);
        Assert.Equal(29, tile.History.Count(result => result.State == HealthState.Healthy));
    }

    // ----- the checks a tile is told about

    /// <summary>A check that is not there is refused by name, and the refusal costs the tile nothing: the tile stood
    /// at no checks before it and at one after, and the answer it was not handed left no half-recorded check behind -
    /// no <see cref="TargetStatus.Last" />, no version, no entry in the history.</summary>
    [Fact]
    public void ACheckThatIsNotThereIsRefusedByNameAndChangesNothing()
    {
        var tile = Node();

        Assert.Equal("result", Assert.Throws<ArgumentNullException>(() => tile.Record(null!)).ParamName);

        Assert.Null(tile.Last);
        Assert.Equal(HealthState.Pending, tile.State);
        Assert.Empty(tile.History);
        Assert.Null(tile.Version);

        tile.Record(Result(HealthState.Healthy, 0, "2.4.21"));

        Assert.Equal("result", Assert.Throws<ArgumentNullException>(() => tile.Record(null!)).ParamName);
        Assert.Single(tile.History);
        Assert.Equal(HealthState.Healthy, tile.State);
        Assert.Equal("2.4.21", tile.Version);
    }

    /// <summary>The state of a tile is the state of its last check and the state of nothing else, every one of the
    /// four named states and the numbers the enum does not name among them: the tile invents no state of its own,
    /// softens none of the four and refuses no number a caller arrived with - which is what lets a check that is
    /// <see cref="HealthState.Unhealthy" /> answer with its HTTP status reach the page as itself.</summary>
    [Theory]
    [InlineData(HealthState.Pending)]
    [InlineData(HealthState.Healthy)]
    [InlineData(HealthState.Unhealthy)]
    [InlineData(HealthState.Unreachable)]
    [InlineData(Nodes.Unnamed)]
    [InlineData((HealthState)(-1))]
    public void TheStateIsTheLastChecksStateAndNoStateOfItsOwn(HealthState state)
    {
        var tile = Node();
        var check = new ProbeResult(state, null, null, Start);

        tile.Record(check);

        Assert.Equal(state, tile.State);
        Assert.Same(check, tile.Last);
        Assert.Equal(state, tile.Last!.State);
        Assert.Equal(state, tile.History[^1].State);
    }

    /// <summary>A check that says nothing is decided yet and no check at all are the same state and two different
    /// tiles: <see cref="TargetStatus.State" /> cannot tell them apart, because <c>Pending</c> is both the state of
    /// the tile nobody asked and a state an answer can carry - which is why everything else about the tile has to be
    /// read to know which of the two the page is looking at.</summary>
    [Fact]
    public void ACheckThatSaidPendingAndNoCheckAtAllShareAStateAndNothingElse()
    {
        var never = Node();
        var pending = Node();

        pending.Record(new ProbeResult(HealthState.Pending, null, null, Start));

        Assert.Equal(never.State, pending.State);
        Assert.Equal(HealthState.Pending, pending.State);

        Assert.Null(never.Last);
        Assert.NotNull(pending.Last);
        Assert.Empty(never.History);
        Assert.Single(pending.History);
        Assert.NotEqual(never.Last, pending.Last);
    }

    /// <summary>The version a tile knows is the version its node last said, said the way it was said: nothing is
    /// trimmed, nothing is cut at the build metadata and nothing is lower-cased on the way in, because the page cuts
    /// and compares elsewhere. The empty word is a version the node said and replaces the nothing that came before
    /// it, and only the absence of a version - <see langword="null" /> - is silence, which a tile answers by keeping
    /// what it knew.</summary>
    [Fact]
    public void AVersionIsKeptAsTheNodeSaidItAndTheEmptyWordIsAVersion()
    {
        var tile = Node();

        Assert.Null(tile.Version);

        tile.Record(new ProbeResult(HealthState.Healthy, 200, 12, Start, string.Empty));

        Assert.Equal(string.Empty, tile.Version);

        tile.Record(new ProbeResult(HealthState.Unreachable, null, null, Start.AddSeconds(30)));

        Assert.Equal(string.Empty, tile.Version);

        tile.Record(new ProbeResult(HealthState.Healthy, 200, 12, Start.AddSeconds(60), " 2.4.22+ABC "));

        Assert.Equal(" 2.4.22+ABC ", tile.Version);
        Assert.Equal("2.4.22", VersionText.Display(tile.Version));

        // A version that was never reported is the absence of one, check after check.
        var silent = Node();
        silent.Record(Result(HealthState.Healthy, 0));
        silent.Record(Result(HealthState.Unhealthy, 30));

        Assert.Null(silent.Version);
    }

    /// <summary>The three recordings are three separate memories: a reading of the telemetry records no check, a
    /// check reports no telemetry, and neither of them reads a detailed check - the state, the history, the version,
    /// the telemetry, the samples and the entries each stand where their own recording left them and nowhere
    /// else.</summary>
    [Fact]
    public void EachRecordingTouchesItsOwnPartOfTheTileAndNoOther()
    {
        var tile = Node();

        // A reading of the telemetry is no check: the tile is still unchecked.
        tile.RecordTelemetry(Sample(12));

        Assert.Equal(12, tile.Telemetry!.Requests);
        Assert.Single(tile.Samples);
        Assert.Null(tile.Last);
        Assert.Equal(HealthState.Pending, tile.State);
        Assert.Empty(tile.History);
        Assert.Null(tile.Version);
        Assert.Null(tile.HealthDetail);
        Assert.Null(tile.LastReadHealthDetail);

        // A check is no reading: the telemetry the node reported stands where it was.
        tile.Record(Result(HealthState.Healthy, 0, "2.4.21"));

        Assert.Equal(12, tile.Telemetry!.Requests);
        Assert.Single(tile.Samples);
        Assert.Equal("2.4.21", tile.Version);
        Assert.Single(tile.History);
        Assert.Null(tile.HealthDetail);
        Assert.Null(tile.LastReadHealthDetail);

        // And a detailed check moves neither of them.
        var checks = new HealthDetail([new HealthCheckEntry("DataAccess", CheckState.Healthy)], Start);
        tile.RecordHealthDetail(checks);

        Assert.Same(checks, tile.HealthDetail);
        Assert.Equal(12, tile.Telemetry!.Requests);
        Assert.Single(tile.Samples);
        Assert.Single(tile.History);
        Assert.Equal("2.4.21", tile.Version);
        Assert.Equal(HealthState.Healthy, tile.State);
    }

    // ----- the detailed health check a tile remembers

    /// <summary>Before anyone reads a node's detailed check there is nothing to show and nothing remembered, and a
    /// first answer of <see langword="null" /> is the same nothing rather than a read of nothing: the memory of the
    /// last read stays empty until an answer actually comes. An answer that names no dependency at all is an answer
    /// - the node did read its checks and found none to report - so it stands as the current entries and as the last
    /// read, and the round that reads nothing afterwards is compared with it.</summary>
    [Fact]
    public void AnAnswerWithoutEntriesIsAnAnswerAndAnAnswerOfNothingIsNot()
    {
        var tile = Node();

        Assert.Null(tile.HealthDetail);
        Assert.Null(tile.LastReadHealthDetail);

        tile.RecordHealthDetail(null);

        Assert.Null(tile.HealthDetail);
        Assert.Null(tile.LastReadHealthDetail);

        tile.RecordHealthDetail(new HealthDetail([], Start));

        var empty = tile.HealthDetail;

        Assert.NotNull(empty);
        Assert.Same(empty, tile.LastReadHealthDetail);
        Assert.Empty(empty!.Entries);
        Assert.Empty(empty.NotHealthy);
        Assert.Null(empty.Find("DataAccess"));
        Assert.Null(empty.Find(null));

        tile.RecordHealthDetail(null);

        Assert.Null(tile.HealthDetail);
        Assert.Same(empty, tile.LastReadHealthDetail);
    }

    // ----- the trend of one number of a tile's readings

    /// <summary>The number a trend is asked for is asked for by name: a selector that is not there is refused with
    /// <see cref="ArgumentNullException" /> naming <c>number</c>, before a single reading is looked at, and the
    /// readings the tile holds stand untouched afterwards.</summary>
    [Fact]
    public void ATrendWithoutANumberIsRefusedByName()
    {
        var tile = Node();
        tile.RecordTelemetry(Sample(10));
        tile.RecordTelemetry(Sample(20));

        Assert.Equal("number", Assert.Throws<ArgumentNullException>(() => tile.TrendOf(null!, "Requests per minute")).ParamName);
        Assert.Equal("number", Assert.Throws<ArgumentNullException>(() => tile.TrendOf(null!, "Requests per minute", " req")).ParamName);
        Assert.Equal(2, tile.Samples.Count);
        Assert.Equal(20, tile.Telemetry!.Requests);
    }

    /// <summary>A tile with no readings has no trend, and its number is never asked for: the selector is not called
    /// once for a node that never reported telemetry - not for one round, not for three, since a round without a
    /// reading is not recorded until the node has reported something - and one reading is still no line.</summary>
    [Fact]
    public void ATileThatReportedNothingTrendsNothingAndIsNeverAskedForItsNumber()
    {
        var tile = Node();
        var asked = new List<TelemetrySnapshot>();

        double? Requests(TelemetrySnapshot telemetry)
        {
            asked.Add(telemetry);
            return telemetry.Requests;
        }

        Assert.Null(tile.TrendOf(Requests, "Requests per minute"));
        Assert.Empty(asked);
        Assert.Empty(tile.Samples);

        for (var round = 0; round < 3; round++)
        {
            tile.RecordTelemetry(null);
        }

        Assert.Empty(tile.Samples);
        Assert.Null(tile.Telemetry);
        Assert.Null(tile.TrendOf(Requests, "Requests per minute"));
        Assert.Empty(asked);

        tile.RecordTelemetry(Sample(10));

        Assert.Single(tile.Samples);
        Assert.Null(tile.TrendOf(Requests, "Requests per minute"));

        // One reading is asked for - there is simply no line to be made of one.
        Assert.Single(asked);
        Assert.Equal(10, asked[0].Requests);
    }

    /// <summary>The readings of a tile become a trend through <see cref="TargetStatus.TrendOf" /> itself: the rounds
    /// the node reported nothing for are asked for nothing and left out of the count, the numbers are the readings
    /// that are there, the words of the title and the unit are the ones the caller named - the unit being optional and
    /// empty when nobody names it - and a number the node never reports is no trend at all over as many readings as it
    /// holds.</summary>
    [Fact]
    public void TheTrendOfATileSkipsTheRoundsThatReportedNothingAndSaysItsUnit()
    {
        var tile = Node();
        tile.RecordTelemetry(Sample(10));
        tile.RecordTelemetry(null);
        tile.RecordTelemetry(Sample(30));

        Assert.Equal(3, tile.Samples.Count);

        var withUnit = tile.TrendOf(telemetry => telemetry.Requests, "Requests per minute", " req");

        Assert.NotNull(withUnit);
        Assert.Equal([0.333, 1], withUnit!.Points);
        Assert.Equal("Requests per minute, last 2 checks: 10 to 30 req, now 30 req", withUnit.Title);

        // The unit is the caller's, and without it the numbers stand alone.
        Assert.Equal(
            "Requests per minute, last 2 checks: 10 to 30, now 30",
            tile.TrendOf(telemetry => telemetry.Requests, "Requests per minute")!.Title);

        // A number the node reports nothing for is a trend of nothing, however many readings it has.
        Assert.Null(tile.TrendOf(telemetry => (double?)null, "SQL commands per minute"));

        // And the words of the title are forwarded as they were given, the empty word among them.
        Assert.Equal(", last 2 checks: 10 to 30, now 30", tile.TrendOf(telemetry => telemetry.Requests, string.Empty)!.Title);
    }

    // ----- the two records a tile is asked for

    /// <summary>A tile's health is its four parts as they stand at the call - the name, the region and the role the
    /// topology wrote, the state its last check left it in - and the record stands still from then on: the checks
    /// that come after it move the tile and not the health already handed out, which is what lets the serving
    /// decision of one round be compared with the next. A region the topology did not write is handed on as the
    /// absence of one, and the empty word as the word it is.</summary>
    [Fact]
    public void TheHealthOfATileIsItsFourPartsAsTheyStandAtTheCall()
    {
        var tile = new TargetStatus(TargetKind.Node, "app-west", West, null, "PRIMARY");

        var before = tile.ToNodeHealth();

        Assert.Equal("app-west", before.Name);
        Assert.Null(before.Region);
        Assert.True(before.IsPrimary);
        Assert.Equal(HealthState.Pending, before.State);

        tile.Record(Result(HealthState.Unreachable, 30));

        Assert.Equal(HealthState.Pending, before.State);
        Assert.Equal(HealthState.Unreachable, tile.ToNodeHealth().State);

        // Two calls over one another's stillness are the same health, and a health made each time.
        Assert.Equal(before, tile.ToNodeHealth() with { State = HealthState.Pending });
        Assert.Equal(tile.ToNodeHealth(), tile.ToNodeHealth());
        Assert.NotSame(tile.ToNodeHealth(), tile.ToNodeHealth());

        // The end of the deployable a tile stands for, and the region it stands in, say nothing about its health.
        var address = new TargetStatus(TargetKind.FrontDoor, "Front Door", West).ToNodeHealth();

        Assert.Equal("Front Door", address.Name);
        Assert.Null(address.Region);
        Assert.False(address.IsPrimary);
        Assert.Equal(HealthState.Pending, address.State);

        Assert.Equal(string.Empty, new TargetStatus(TargetKind.Node, "app", West, string.Empty, NodeInfo.StandbyRole).ToNodeHealth().Region);
    }

    /// <summary>A tile's version is labelled by its region - and by its own name when the topology gave it no
    /// region, the empty word being a region it wrote - and carries the state of its last check next to the version it
    /// last heard. The two parts are not read together: a node that stops answering keeps the version it reported and
    /// stops reporting it as the version it runs, which is the difference between the dashboard remembering a build
    /// and claiming a node is running it.</summary>
    [Fact]
    public void TheVersionOfATileIsLabelledByItsRegionOrItsName()
    {
        var tile = Node();
        tile.Record(Result(HealthState.Healthy, 0, "2.4.21+abc123"));

        var version = tile.ToNodeVersion();

        Assert.Equal("westus3", version.Label);
        Assert.Equal(HealthState.Healthy, version.State);
        Assert.Equal("2.4.21+abc123", version.Version);
        Assert.Equal("2.4.21", version.Running);

        tile.Record(Result(HealthState.Unreachable, 30));

        var silent = tile.ToNodeVersion();

        Assert.Equal("westus3", silent.Label);
        Assert.Equal(HealthState.Unreachable, silent.State);
        Assert.Equal("2.4.21+abc123", silent.Version);
        Assert.Null(silent.Running);

        // The region is the label when there is one, and the name when the topology wrote none.
        Assert.Equal("app-west", new TargetStatus(TargetKind.Node, "app-west", West).ToNodeVersion().Label);
        Assert.Equal(string.Empty, new TargetStatus(TargetKind.Node, "app-west", West, string.Empty).ToNodeVersion().Label);
        Assert.Equal(HealthState.Pending, new TargetStatus(TargetKind.Node, "app-west", West).ToNodeVersion().State);
        Assert.Null(new TargetStatus(TargetKind.Node, "app-west", West).ToNodeVersion().Version);

        // A Front Door tile is labelled the words it was built with, its name being all it has.
        Assert.Equal("Front Door", new TargetStatus(TargetKind.FrontDoor, "Front Door", West).ToNodeVersion().Label);
    }
}
