namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="NodeObservation" />, what the page knew of one
/// endpoint at one check: the five parts it holds and the record the compiler made around them.
/// <para>
/// What the other files already pin is left alone. <see cref="HealthHelperCoverageTests" /> pins
/// <see cref="NodeObservation.Of" /> in full - every part taken from the tile as it stands, the tile nobody has
/// checked observed as <c>Pending</c> with nothing behind it, the observation keeping its own check rather than
/// moving with the tile, and a target that is not there refused by name. <see cref="EventDetectorTests" /> pins what
/// the detectors read out of the parts: the state and the version they compare, the telemetry that says a restart,
/// the result that lends an event its HTTP status and its word for why, and the entries whose change is an event of
/// its own.
/// </para>
/// <para>
/// What is pinned here is the rest of every member: that the two last parts fall to nothing when the caller does not
/// name them and that the five stand where they are given, the very result carried along; that the state is the state
/// given and never one read out of the result under it, over all four states and over a number the enum does not
/// name; that a version of no characters is not a version of nothing and that nobody trims what the endpoint
/// reported; that a telemetry of no numbers is still the telemetry of a check, and a result that says neither a
/// status nor a latency and entries with nothing in them are still there; that an observation is one with nothing
/// that is not an observation; that its hash code follows its five parts and stands from call to call; that the five
/// read out of it in the order they stand in it; that <c>with</c> changes the part it names and carries the other
/// four along; and how an observation says its parts - the five of a check that reported nothing at all in full, and
/// the nested ones opened in their own words.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every moment is one the test brings,
/// and the words of a record that carries one are checked only up to the moment, which the viewer reads in their own
/// hours.
/// </para>
/// </summary>
public class NodeObservationEdgeCaseTests
{
    /// <summary>The one moment every part below stands on: a fixed moment, not the clock.</summary>
    private static readonly DateTimeOffset Opened = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>The outcome of one check: the state it ended in, the status and the forty milliseconds it took when
    /// the endpoint answered, and the word for why it did not when it did not.</summary>
    private static ProbeResult Result(HealthState state, int? status = null, string? version = null, string? detail = null) =>
        new(state, status, status is null ? null : 40, Opened, version, detail);

    /// <summary>The telemetry of an app that counts its own calls, and says no process and no percentile.</summary>
    private static TelemetrySnapshot Counted(int requests = 12, int errors = 0) =>
        new(requests, 10, 2, errors, null, 3, 1, 30, null, 4, Opened);

    /// <summary>The answer of a node's detailed health check, in the order the node answered it.</summary>
    private static HealthDetail Entries(params (string Name, CheckState State)[] entries) =>
        new([.. entries.Select(entry => new HealthCheckEntry(entry.Name, entry.State))], Opened);

    // ----- the five parts the observation keeps

    /// <summary>The two last parts are the two a caller need not name: an observation of three says no result and no
    /// entries, one of four says no entries, the parts named out of their order land where they stand, and every
    /// one of them is the same observation as the one that names its nothing explicitly.</summary>
    [Fact]
    public void TheTwoLastPartsFallToNothingWhenTheCallerDoesNotNameThem()
    {
        var three = new NodeObservation(HealthState.Healthy, "2.4.14", Counted());

        Assert.Equal(HealthState.Healthy, three.State);
        Assert.Equal("2.4.14", three.Version);
        Assert.NotNull(three.Telemetry);
        Assert.Null(three.Last);
        Assert.Null(three.Checks);

        var four = new NodeObservation(HealthState.Unreachable, null, null, Result(HealthState.Unreachable, detail: "No answer within 10 s."));

        Assert.Null(four.Checks);
        Assert.Equal(HealthState.Unreachable, four.Last!.State);
        Assert.Null(four.Telemetry);

        var named = new NodeObservation(HealthState.Healthy, null, null, Checks: Entries(("API", CheckState.Healthy), ("LlmGateway", CheckState.Degraded)));

        Assert.Null(named.Last);
        Assert.Equal(2, named.Checks!.Entries.Count);

        // Naming the nothings makes the same observation, not another.
        Assert.Equal(three, new NodeObservation(HealthState.Healthy, "2.4.14", three.Telemetry, null, null));
        Assert.Equal(four, new NodeObservation(HealthState.Unreachable, null, null, four.Last, null));
        Assert.Equal(named, new NodeObservation(HealthState.Healthy, null, null, null, named.Checks));
    }

    /// <summary>The five parts stand where they are given and nothing is copied or read again on the way in: the
    /// result an observation is made of is the very result it stands on, with the status, the latency, the version
    /// and the word for why that result carries.</summary>
    [Fact]
    public void TheObservationKeepsTheFivePartsWhereTheyStand()
    {
        var telemetry = Counted();
        var last = Result(HealthState.Unhealthy, status: 503, version: "2.4.15", detail: "HTTP 503");
        var checks = Entries(("DataAccess", CheckState.Unhealthy));

        var seen = new NodeObservation(HealthState.Unhealthy, "2.4.15", telemetry, last, checks);

        Assert.Equal(HealthState.Unhealthy, seen.State);
        Assert.Equal("2.4.15", seen.Version);
        Assert.Same(telemetry, seen.Telemetry);
        Assert.Same(last, seen.Last);
        Assert.Same(checks, seen.Checks);
        Assert.Same(checks.Entries[0], seen.Checks!.Entries[0]);
        Assert.Equal(503, seen.Last!.StatusCode);
        Assert.Equal(40, seen.Last.LatencyMs);
        Assert.Equal("2.4.15", seen.Last.Version);
        Assert.Equal("HTTP 503", seen.Last.Detail);
        Assert.Equal(ProbeKind.Health, seen.Last.Probe);
    }

    // ----- the state

    /// <summary>The state an observation says is the state it was given and never one read out of the result under
    /// it: every one of the four stands over a result that contradicts it - a <c>Healthy</c> of a check that got a
    /// 503, an <c>Unreachable</c> of a check that answered 200 - and over no result at all.</summary>
    [Theory]
    [InlineData(HealthState.Pending)]
    [InlineData(HealthState.Healthy)]
    [InlineData(HealthState.Unhealthy)]
    [InlineData(HealthState.Unreachable)]
    public void TheStateStandsAsGivenWhicheverResultStandsUnderIt(HealthState state)
    {
        ProbeResult?[] under =
        [
            Result(HealthState.Healthy, status: 200, version: "2.4.14"),
            Result(HealthState.Unhealthy, status: 503, detail: "HTTP 503"),
            Result(HealthState.Unreachable, detail: "No answer within 10 s."),
            null,
        ];

        foreach (var last in under)
        {
            var seen = new NodeObservation(state, "2.4.14", null, last);

            Assert.Equal(state, seen.State);
            Assert.Equal(last, seen.Last);
            Assert.NotEqual(seen, seen with { State = state == HealthState.Healthy ? HealthState.Unhealthy : HealthState.Healthy });
        }
    }

    /// <summary>An observation keeps the state it is given and asks nothing of it: a number the enum does not name
    /// stands as itself, says its own number in the record's words, and makes an observation apart from the four the
    /// enum names and apart from the number one step off it - the state is the page's word for one check, not a
    /// claim that the check was classifiable.</summary>
    [Theory]
    [InlineData((HealthState)(-1))]
    [InlineData((HealthState)4)]
    [InlineData((HealthState)int.MaxValue)]
    public void AStateTheEnumDoesNotNameIsKeptAsTheStateItIs(HealthState state)
    {
        var seen = new NodeObservation(state, "2.4.14", null);

        Assert.Equal(state, seen.State);
        Assert.StartsWith($"NodeObservation {{ State = {(int)state}, Version = 2.4.14", seen.ToString(), StringComparison.Ordinal);

        foreach (var named in Enum.GetValues<HealthState>())
        {
            Assert.NotEqual(seen, new NodeObservation(named, "2.4.14", null));
        }

        Assert.NotEqual(seen, seen with { State = (HealthState)((int)state - 1) });
    }

    // ----- the version

    /// <summary>The version is the text the endpoint reported, kept as it stands: nothing, the empty string, a
    /// blank, one character, a padded version and one with build metadata are each kept whole, each one making the
    /// same observation and saying the same number as another of the very same text.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("0")]
    [InlineData("2.4.14")]
    [InlineData(" 2.4.14 ")]
    [InlineData("2.4.14-beta+7.20261004.9999")]
    public void AVersionIsKeptExactlyAsTheEndpointReportedIt(string? version)
    {
        var seen = new NodeObservation(HealthState.Healthy, version, null);

        Assert.Equal(version, seen.Version);
        Assert.Equal(seen, new NodeObservation(HealthState.Healthy, version, null));
        Assert.Equal(seen.GetHashCode(), new NodeObservation(HealthState.Healthy, version, null).GetHashCode());
    }

    /// <summary>A version of no characters is a version the endpoint reported and not a version it kept silent
    /// about: nothing, the empty string and a blank are three observations apart, and the spaces around a padded
    /// version are never taken off - nobody reads a version out of a result either, whatever the result under it
    /// says.</summary>
    [Fact]
    public void AVersionOfNoCharactersIsNotAVersionOfNothing()
    {
        var none = new NodeObservation(HealthState.Healthy, null, null, Result(HealthState.Healthy, status: 200, version: "2.4.14"));
        var empty = new NodeObservation(HealthState.Healthy, string.Empty, null);
        var blank = new NodeObservation(HealthState.Healthy, "   ", null);
        var padded = new NodeObservation(HealthState.Healthy, " 2.4.14 ", null);

        Assert.Null(none.Version);
        Assert.Equal(string.Empty, empty.Version);
        Assert.Equal("   ", blank.Version);
        Assert.Equal(" 2.4.14 ", padded.Version);
        Assert.Equal(3, blank.Version!.Length);

        // The version the result under an observation reports is nobody's version but its own: an observation made
        // of a check that said "2.4.14" and given no version of its own says no version.
        Assert.Equal("2.4.14", none.Last!.Version);
        Assert.Null(none.Version);

        Assert.Equal(
            3,
            new[] { none.Version, empty.Version, blank.Version }.Distinct(StringComparer.Ordinal).Count());

        Assert.NotEqual(none, empty);
        Assert.NotEqual(empty, blank);
        Assert.NotEqual(none, blank);
        Assert.NotEqual(padded, new NodeObservation(HealthState.Healthy, "2.4.14", null));
        Assert.NotEqual(none, none with { Version = string.Empty });
    }

    // ----- the telemetry, the result and the entries

    /// <summary>A telemetry that says no numbers is still the telemetry of a check: every number zero and every
    /// optional number unsaid is kept as the snapshot it is, is another observation than the nothing a node that
    /// reported none lands on, and is one with another snapshot of the same numbers though the two are two
    /// instances - while a process's uptime under the counters makes another check of it.</summary>
    [Fact]
    public void ATelemetryThatSaysNoNumbersIsATelemetryAndNotNothing()
    {
        var quiet = new TelemetrySnapshot(0, 0, 0, 0, null, 0, 0, 0, null, 0, Opened);
        var heard = new NodeObservation(HealthState.Healthy, "2.4.14", quiet);
        var silent = new NodeObservation(HealthState.Healthy, "2.4.14", null);

        Assert.Same(quiet, heard.Telemetry);
        Assert.Equal(0, heard.Telemetry!.Requests);
        Assert.Null(heard.Telemetry.P95Ms);
        Assert.Null(heard.Telemetry.Process);
        Assert.Null(heard.Telemetry.StartedAt);
        Assert.Null(silent.Telemetry);

        Assert.NotEqual(heard, silent);
        Assert.Equal(heard, new NodeObservation(HealthState.Healthy, "2.4.14", new TelemetrySnapshot(0, 0, 0, 0, null, 0, 0, 0, null, 0, Opened)));
        Assert.Equal(
            heard.GetHashCode(),
            new NodeObservation(HealthState.Healthy, "2.4.14", new TelemetrySnapshot(0, 0, 0, 0, null, 0, 0, 0, null, 0, Opened)).GetHashCode());

        var up = heard with { Telemetry = quiet with { Process = new ProcessVitals(null, null, null, null, null, null, 9000) } };

        Assert.NotEqual(heard, up);
        Assert.Equal(9000, up.Telemetry!.Process!.UptimeSeconds);

        // A number of the counters is another observation as well: twelve requests are not thirteen.
        Assert.NotEqual(new NodeObservation(HealthState.Healthy, "2.4.14", Counted(requests: 12)), new NodeObservation(HealthState.Healthy, "2.4.14", Counted(requests: 13)));
        Assert.NotEqual(new NodeObservation(HealthState.Healthy, "2.4.14", Counted()), new NodeObservation(HealthState.Healthy, "2.4.14", Counted(errors: 1)));
    }

    /// <summary>A result that says neither a status nor a latency is a result all the same - the endpoint that was
    /// not reached has one - and an answer of the detailed health check that holds no entries is an answer: neither
    /// is the nothing a caller could have left out, and each of them is a part of the observation that makes it
    /// another.</summary>
    [Fact]
    public void AResultAndEntriesThatSayNothingAreStillThere()
    {
        var noAnswer = Result(HealthState.Unreachable, detail: "No answer within 10 s.");
        var seen = new NodeObservation(HealthState.Unreachable, null, null, noAnswer, Entries());

        Assert.Same(noAnswer, seen.Last);
        Assert.Null(seen.Last!.StatusCode);
        Assert.Null(seen.Last.LatencyMs);
        Assert.Null(seen.Last.Version);
        Assert.NotNull(seen.Checks);
        Assert.Empty(seen.Checks!.Entries);

        Assert.NotEqual(seen, seen with { Last = null });
        Assert.NotEqual(seen, seen with { Checks = null });
        Assert.NotEqual(seen, new NodeObservation(HealthState.Unreachable, null, null, noAnswer, Entries(("API", CheckState.Healthy))));
        Assert.NotEqual(seen, new NodeObservation(HealthState.Unreachable, null, null, null, Entries()));

        // The entries the answer had are kept in the order it had them, each with the state it said: the
        // observation re-reads nothing of them and leaves nothing out.
        var answered = seen with { Checks = Entries(("API", CheckState.Healthy), ("DataAccess", CheckState.Unhealthy), ("Queue", CheckState.Unknown)) };

        Assert.Equal(["API", "DataAccess", "Queue"], answered.Checks!.Entries.Select(entry => entry.Name));
        Assert.Equal(
            [CheckState.Healthy, CheckState.Unhealthy, CheckState.Unknown],
            answered.Checks.Entries.Select(entry => entry.State));
        Assert.NotEqual(answered, answered with { Checks = Entries() });
    }

    // ----- the record around them

    /// <summary>An observation is one with nothing that is not an observation of the same five parts: a word, a
    /// state, the very result and the very snapshot it is made of are all refused, the operator sees the nothing a
    /// caller holds, and an observation of nothing at all is still an observation and one with itself.</summary>
    [Fact]
    public void AnObservationIsNotOneWithAnythingThatIsNotAnObservation()
    {
        var last = Result(HealthState.Healthy, status: 200);
        var telemetry = Counted();
        var one = new NodeObservation(HealthState.Healthy, "2.4.14", telemetry, last, Entries(("API", CheckState.Healthy)));
        var bare = new NodeObservation(HealthState.Pending, null, null);
        NodeObservation? nothing = null;

        Assert.False(one.Equals("NodeObservation"));
        Assert.False(one.Equals(HealthState.Healthy));
        Assert.False(one.Equals(last));
        Assert.False(one.Equals(telemetry));
        Assert.False(one.Equals(one.Checks));
        Assert.False(one.Equals(null));
        Assert.False(bare.Equals(null));

        Assert.True(one.Equals(one));
        Assert.True(one.Equals(one with { }));
        Assert.True(one == one with { });
        Assert.False(one != one with { });
        Assert.False(one == nothing);
        Assert.True(one != nothing);
        Assert.True(one.Equals((object)(one with { })));
        Assert.False(nothing == bare);
        Assert.False(one == bare);
        Assert.True(bare == new NodeObservation(HealthState.Pending, null, null));
    }

    /// <summary>The hash code follows the five parts and stands where it stands: the same observation says the same
    /// number from call to call, two observations of the same parts in separate instances say one number, the
    /// nothing of every part says the number a pending endpoint says, and each part moved says another.</summary>
    [Fact]
    public void TheHashCodeFollowsTheFivePartsAndStandsFromCallToCall()
    {
        var one = new NodeObservation(HealthState.Unhealthy, "2.4.15", Counted(), Result(HealthState.Unhealthy, status: 503));
        var again = new NodeObservation(HealthState.Unhealthy, "2.4.15", Counted(errors: 0), Result(HealthState.Unhealthy, status: 503));

        Assert.Equal(one, again);
        Assert.NotSame(one.Telemetry, again.Telemetry);
        Assert.NotSame(one.Last, again.Last);
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Equal(one.GetHashCode(), again.GetHashCode());

        var nothing = new NodeObservation(HealthState.Pending, null, null);

        Assert.Equal(nothing.GetHashCode(), new NodeObservation(HealthState.Pending, null, null, null, null).GetHashCode());
        Assert.NotEqual(one.GetHashCode(), nothing.GetHashCode());

        Assert.NotEqual(one.GetHashCode(), (one with { State = HealthState.Healthy }).GetHashCode());
        Assert.NotEqual(one.GetHashCode(), (one with { Version = null }).GetHashCode());
        Assert.NotEqual(one.GetHashCode(), (one with { Telemetry = null }).GetHashCode());
        Assert.NotEqual(one.GetHashCode(), (one with { Last = null }).GetHashCode());
        Assert.NotEqual(one.GetHashCode(), (one with { Checks = Entries(("API", CheckState.Healthy)) }).GetHashCode());
    }

    /// <summary>The five parts read out of the observation in the order they stand in it - the state, the version,
    /// the telemetry, the result and the entries - the observation of a check nobody finished reading saying its
    /// state and four nothings.</summary>
    [Fact]
    public void TheFivePartsReadOutOfTheObservationInTheOrderTheyStand()
    {
        var telemetry = Counted();
        var last = Result(HealthState.Healthy, status: 200, version: "2.4.14");
        var checks = Entries(("API", CheckState.Healthy));

        var (state, version, read, result, entries) = new NodeObservation(HealthState.Healthy, "2.4.14", telemetry, last, checks);

        Assert.Equal(HealthState.Healthy, state);
        Assert.Equal("2.4.14", version);
        Assert.Same(telemetry, read);
        Assert.Same(last, result);
        Assert.Same(checks, entries);

        var (pending, noVersion, noTelemetry, noResult, noEntries) = new NodeObservation(HealthState.Pending, null, null);

        Assert.Equal(HealthState.Pending, pending);
        Assert.Null(noVersion);
        Assert.Null(noTelemetry);
        Assert.Null(noResult);
        Assert.Null(noEntries);
    }

    /// <summary><c>with</c> names the part it changes and carries the other four: the copy that names no part is the
    /// same observation and not the same instance, the observation left standing does not move when its copy is
    /// made, and the entries put on an observation that had none - which is what the monitor does to the check it
    /// compares against - make the observation the one of a check that had them.</summary>
    [Fact]
    public void WithChangesOnlyThePartItNamesAndCarriesTheOtherFourAlong()
    {
        var telemetry = Counted();
        var last = Result(HealthState.Healthy, status: 200, version: "2.4.14");
        var checks = Entries(("API", CheckState.Healthy));
        var one = new NodeObservation(HealthState.Healthy, "2.4.14", telemetry, last, checks);

        var copy = one with { };

        Assert.Equal(one, copy);
        Assert.NotSame(one, copy);
        Assert.Same(telemetry, copy.Telemetry);
        Assert.Same(last, copy.Last);
        Assert.Same(checks, copy.Checks);

        var degraded = one with { State = HealthState.Unhealthy };

        Assert.Equal(HealthState.Unhealthy, degraded.State);
        Assert.Equal("2.4.14", degraded.Version);
        Assert.Same(telemetry, degraded.Telemetry);
        Assert.Same(last, degraded.Last);
        Assert.Same(checks, degraded.Checks);
        Assert.Equal(HealthState.Healthy, one.State);

        var withoutVersion = one with { Version = null };

        Assert.Null(withoutVersion.Version);
        Assert.Same(last, withoutVersion.Last);
        Assert.NotEqual(one, withoutVersion);

        var withoutTelemetry = one with { Telemetry = null };

        Assert.Null(withoutTelemetry.Telemetry);
        Assert.Same(last, withoutTelemetry.Last);
        Assert.Same(checks, withoutTelemetry.Checks);

        var retired = one with { Last = Result(HealthState.Unreachable, detail: "No answer within 10 s.") };

        Assert.Equal("No answer within 10 s.", retired.Last!.Detail);
        Assert.Equal(HealthState.Healthy, retired.State);
        Assert.Equal("2.4.14", retired.Version);

        // The check before the one that is compared: entries put on it, and nothing else moved.
        var before = new NodeObservation(HealthState.Healthy, "2.4.14", telemetry, last) with { Checks = checks };

        Assert.Equal(one, before);
        Assert.Same(checks, before.Checks);
        Assert.NotEqual(one, before with { Checks = null });
    }

    // ----- the words

    /// <summary>An observation says its five parts in the order the record declares them, the ones a caller left out
    /// said as the nothing they are, and a state the enum does not name says its number where the four say their
    /// word.</summary>
    [Fact]
    public void AnObservationSaysItsFivePartsInTheirOrder()
    {
        Assert.Equal(
            "NodeObservation { State = Healthy, Version = 2.4.14, Telemetry = , Last = , Checks =  }",
            new NodeObservation(HealthState.Healthy, "2.4.14", null).ToString());

        Assert.Equal(
            "NodeObservation { State = Pending, Version = , Telemetry = , Last = , Checks =  }",
            new NodeObservation(HealthState.Pending, null, null).ToString());

        Assert.Equal(
            "NodeObservation { State = Unreachable, Version = , Telemetry = , Last = , Checks =  }",
            new NodeObservation(HealthState.Unreachable, null, null).ToString());

        Assert.StartsWith(
            "NodeObservation { State = -1, Version = ,",
            new NodeObservation((HealthState)(-1), null, null).ToString(),
            StringComparison.Ordinal);
    }

    /// <summary>An observation of a check that really did answer says its nested parts in their own words: the
    /// snapshot opens with the counters it counts, the result with the state it probed and the status it got, and
    /// the entries with the entries the answer had - each of them named by the property it stands on, and in the
    /// order the record declares them. (The moment each part stands on is the viewer's own, so the words are checked
    /// only up to it.)</summary>
    [Fact]
    public void ANestedPartIsSaidInTheWordsOfThePartItIs()
    {
        var words = new NodeObservation(
            HealthState.Unhealthy,
            "2.4.15",
            Counted(),
            Result(HealthState.Unhealthy, status: 503, version: "2.4.15"),
            Entries(("DataAccess", CheckState.Unhealthy))).ToString();

        Assert.StartsWith(
            "NodeObservation { State = Unhealthy, Version = 2.4.15, Telemetry = TelemetrySnapshot { Requests = 12,",
            words,
            StringComparison.Ordinal);
        Assert.Contains(", Last = ProbeResult { State = Unhealthy, StatusCode = 503, LatencyMs = 40,", words, StringComparison.Ordinal);
        Assert.Contains(", Checks = HealthDetail { Entries = ", words, StringComparison.Ordinal);
        Assert.EndsWith(" }", words, StringComparison.Ordinal);

        // The three nested parts say themselves, and the observation only names the property each of them stands on.
        Assert.Contains("Telemetry = TelemetrySnapshot {", words, StringComparison.Ordinal);
        Assert.DoesNotContain("Telemetry = ,", words, StringComparison.Ordinal);
        Assert.StartsWith("TelemetrySnapshot { Requests = ", Counted().ToString(), StringComparison.Ordinal);
        Assert.StartsWith("ProbeResult { State = Healthy, StatusCode = 200,", Result(HealthState.Healthy, status: 200).ToString(), StringComparison.Ordinal);
        Assert.StartsWith("HealthDetail { Entries = ", Entries(("API", CheckState.Healthy)).ToString(), StringComparison.Ordinal);
    }
}
