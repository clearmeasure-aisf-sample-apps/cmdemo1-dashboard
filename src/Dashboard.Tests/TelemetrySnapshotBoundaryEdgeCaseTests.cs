namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="TelemetrySnapshot" /> that the files beside it leave
/// open. <see cref="TelemetrySnapshotEdgeCaseTests" /> pins the fifteen parts landing on the properties of their
/// names, the counts at both ends of <see cref="int" />, the two words the SQL parts make, an answer that is no JSON
/// and no object, a name that is no number, a number below zero, the rounding of a count told in parts and a number
/// past the reach of an <see cref="int" /> saying the whole of one, equality of two readings and their use as a key,
/// the copy, the words and the public surface; <see cref="TelemetrySnapshotNameEdgeCaseTests" /> pins how names are
/// matched (case, a name said twice, unknown names, exponent form, loose JSON, whitespace);
/// <see cref="TelemetryTests" /> and <see cref="ProcessVitalsTests" /> pin the shipped answer read whole, its
/// <c>process</c> object and the <c>startedAt</c> beside it; and <see cref="JsonReadTests" /> pins the shared reader
/// of a part on a name of its own.
/// <para>
/// What none of those says, and what follows: the numbers beyond the reach of a <see cref="double" /> at both ends -
/// one no double can hold above zero is heard as the whole of an <see cref="int" /> by this type's own reader, which
/// checks a sign and never a limit, while the vitals beside it, read by the shared reader, are refused for the very
/// same number; a number so small that a double underflows it is the zero it underflows to and not the silence of a
/// name the answer never wrote; an answer nested deeper than a reader will follow, which is no answer rather than a
/// stack going out; <see cref="TelemetrySnapshot.StartedAt" /> at the two ends of a moment, with the hours it carries
/// and the blanks around it, refused as nothing when it is no moment, and looked for at the top of an answer only;
/// the two moments of a reading kept apart and neither judged for standing the wrong way round; a moment compared by
/// the instant it stands on and only said by the hours it carries; and the record's own two operators held at nothing
/// on either side.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every number, every moment and every answer
/// below is one the test brought, and <see cref="TelemetrySnapshot.Parse" /> is handed the moment a reading stands on
/// rather than going to find one.
/// </para>
/// </summary>
public sealed class TelemetrySnapshotBoundaryEdgeCaseTests
{
    /// <summary>The moment every reading below stands on: one the test brought, not the clock.</summary>
    private static readonly DateTimeOffset Read = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>The moment the app of the answers below says its process started, two hours short of the reading.</summary>
    private static readonly DateTimeOffset Started = new(2026, 10, 4, 20, 31, 19, TimeSpan.Zero);

    /// <summary>The whole of an <see cref="int" /> under every one of the twelve names an answer counts, for three
    /// numbers no <see cref="double" /> can hold the value of: the top of a double, the first number past it, and one
    /// a long way past. This type's reader of a part asks only that a number be not below zero - never that it be
    /// finite, and never that an <see cref="int" /> be able to hold it - so the cast lands on the top of an
    /// <see cref="int" /> and the two halves of the SQL turn into a telling-apart of it.
    /// <see cref="TelemetrySnapshotEdgeCaseTests" /> pins the same saturation for <c>1e21</c>, which a double does
    /// hold; what is added is the number that one does not, which is the number no counter of a real node reaches but
    /// a hand-written or corrupted answer carries. The moment and the vitals are unsaid, since the answer names
    /// neither.</summary>
    [Theory]
    [InlineData("1.7976931348623157e308")]
    [InlineData("1e309")]
    [InlineData("1e400")]
    public void ANumberBeyondTheReachOfADoubleIsHeardAsTheWholeOfAnInt(string number)
    {
        var read = Reading($$"""
            { "requests": { "perMinute": {{number}}, "frontDoor": {{number}}, "direct": {{number}}, "errors": {{number}}, "p95Ms": {{number}} },
              "probes": { "perMinute": {{number}}, "frontDoor": {{number}} },
              "sql": { "perMinute": {{number}}, "p95Ms": {{number}}, "requests": {{number}}, "background": {{number}} },
              "http": { "perMinute": {{number}} } }
            """);

        Assert.Equal(int.MaxValue, read.Requests);
        Assert.Equal(int.MaxValue, read.FromFrontDoor);
        Assert.Equal(int.MaxValue, read.Direct);
        Assert.Equal(int.MaxValue, read.Errors);
        Assert.Equal(int.MaxValue, read.P95Ms);
        Assert.Equal(int.MaxValue, read.Probes);
        Assert.Equal(int.MaxValue, read.FrontDoorProbes);
        Assert.Equal(int.MaxValue, read.Sql);
        Assert.Equal(int.MaxValue, read.SqlP95Ms);
        Assert.Equal(int.MaxValue, read.Http);
        Assert.Equal(int.MaxValue, read.SqlRequests);
        Assert.Equal(int.MaxValue, read.SqlBackground);
        Assert.Equal(Read, read.ReadAt);
        Assert.True(read.SplitsSql);
        Assert.Equal(int.MaxValue, read.SqlOfTraffic);
        Assert.Null(read.StartedAt);
        Assert.Null(read.Process);
    }

    /// <summary>A number so small that a double underflows it is the zero it underflows to, and that zero is a
    /// number the app did say: the traffic counts nothing, the percentile of it is <c>0 ms</c> rather than the
    /// silence of a name never written, and the two halves of the SQL round onto nothing together - which is a
    /// telling-apart whose traffic is nothing. Both ends of a very small number say the same: <c>1e-400</c>, which no
    /// double holds at all, and <c>5e-324</c>, the smallest one holds, are each rounded onto nothing. A count an
    /// answer wrote too small to hold and a count it left out stay two different things - the first is the zero the
    /// tiles show and the second the dash they leave out.</summary>
    [Fact]
    public void ANumberTooSmallForADoubleIsTheZeroItUnderflowsToAndNotTheNothingItLooksLike()
    {
        var read = Reading("""
            { "requests": { "perMinute": 1e-400, "frontDoor": 5e-324, "p95Ms": 1e-400 },
              "sql": { "requests": 1e-400, "background": 5e-324 },
              "http": { "perMinute": 1e-400 } }
            """);

        Assert.Equal(0, read.Requests);
        Assert.Equal(0, read.FromFrontDoor);
        Assert.Equal(0, read.Http);
        Assert.NotNull(read.P95Ms); // said, as the nothing the number underflows to - not left unsaid
        Assert.Equal(0, read.P95Ms);
        Assert.Null(read.SqlP95Ms); // the name the answer never wrote, beside the one it wrote too small to hold
        Assert.Equal(0, read.SqlRequests);
        Assert.Equal(0, read.SqlBackground);
        Assert.True(read.SplitsSql);
        Assert.Equal(0, read.SqlOfTraffic);
        Assert.Equal(
            new TelemetrySnapshot(0, 0, 0, 0, 0, 0, 0, 0, null, 0, Read),
            read with { SqlRequests = null, SqlBackground = null });
    }

    /// <summary>A number no double can hold below zero is a number below zero like any other, and so is said as the
    /// silence its reader makes of it: every count nothing, both percentiles and both halves of the SQL unsaid, the
    /// telling-apart gone with them and the traffic falling back on the total - which is the zero of a name the
    /// answer said negatively, and not the whole of an <see cref="int" /> the same answer said positively.</summary>
    [Fact]
    public void ANumberBeyondTheReachOfADoubleBelowZeroIsSaidAsSilenceLikeAnyOtherNegative()
    {
        var read = Reading("""
            { "requests": { "perMinute": -1e400, "frontDoor": -1e400, "direct": -1e400, "errors": -1e400, "p95Ms": -1e400 },
              "probes": { "perMinute": -1e400, "frontDoor": -1e400 },
              "sql": { "perMinute": -1e400, "p95Ms": -1e400, "requests": -1e400, "background": -1e400 },
              "http": { "perMinute": -1e400 } }
            """);

        Assert.Equal(new TelemetrySnapshot(0, 0, 0, 0, null, 0, 0, 0, null, 0, Read), read);
        Assert.Null(read.SqlRequests);
        Assert.Null(read.SqlBackground);
        Assert.False(read.SplitsSql);
        Assert.Equal(0, read.SqlOfTraffic);
    }

    /// <summary>The same impossible number is two different things inside one answer: this type's own reader of a
    /// count saturates it to the whole of an <see cref="int" />, while the shared reader the process vitals go
    /// through refuses a number that is no finite number at all - so an app whose vitals are nothing but impossible
    /// numbers says nothing of its process while still counting impossibly much traffic, and one that adds a thread
    /// count it can hold has a process of that one vital. <see cref="ProcessVitalsTests" /> pins the vitals of a
    /// value a number cannot be; what is pinned here is the two readers of one <see cref="TelemetrySnapshot.Parse" />
    /// parting company over the number both of them are handed.</summary>
    [Fact]
    public void TheImpossibleNumberIsSaturationInACountAndRefusalInAVital()
    {
        var beyondEveryVital = Reading("""
            { "requests": { "perMinute": 1e400 },
              "process": { "cpuPercent": 1e400, "workingSetMb": 1e400, "gcHeapMb": 1e400, "uptimeSeconds": 1e400 } }
            """);

        Assert.Equal(int.MaxValue, beyondEveryVital.Requests);
        Assert.Null(beyondEveryVital.P95Ms); // unsaid by an answer that never wrote the name, whatever its other numbers are
        Assert.Null(beyondEveryVital.Process);

        var oneVitalHeld = Reading("""{ "requests": { "perMinute": 1e400 }, "process": { "cpuPercent": 1e400, "threads": 7 } }""");

        Assert.Equal(new ProcessVitals(null, null, null, 7, null, null, null), oneVitalHeld.Process);
        Assert.Equal(int.MaxValue, oneVitalHeld.Requests);
        Assert.Null(oneVitalHeld.Process?.CpuPercent); // the number beside the one held, refused and not saturated
    }

    /// <summary>An answer nested further than a reader will follow is no answer at all: the whole of a document is
    /// read before a single name is looked at, so forty arrays under a name the reader passes over are an answer with
    /// its traffic in them and a hundred of them are nothing - whether they stand under a name nobody reads, under
    /// <c>sql</c>, or under <c>requests</c> itself, where the shape rule would have turned a shallow array into no
    /// reading anyway. A body from something that is not the endpoint - a gateway's own document, an answer truncated
    /// in transport - leaves the dashboard with the nothing a tile shows rather than with an exception out of the
    /// read.</summary>
    [Fact]
    public void AnAnswerNestedDeeperThanAReaderWillFollowIsNoReading()
    {
        static string Nest(int depth) => string.Concat(new string('[', depth), new string(']', depth));

        Assert.Equal(11, Reading($$"""{ "requests": { "perMinute": 11, "extra": {{Nest(40)}} } }""").Requests);

        Assert.Null(TelemetrySnapshot.Parse($$"""{ "requests": { "perMinute": 11, "extra": {{Nest(100)}} } }""", Read));
        Assert.Null(TelemetrySnapshot.Parse($$"""{ "requests": { "perMinute": 11 }, "sql": {{Nest(100)}} }""", Read));
        Assert.Null(TelemetrySnapshot.Parse($$"""{ "requests": {{Nest(100)}} }""", Read));
    }

    /// <summary>The app's own moment at the two ends of <see cref="DateTimeOffset" />, with the hours it was carried
    /// in and the blanks around it trimmed off, said twice and heard as the last saying, and looked for at the top of
    /// an answer only: a moment named inside <c>requests</c> or inside <c>sql</c> is a part of nothing and leaves the
    /// reading without one, however much else those sections say. The hours an answer carries are kept rather than
    /// normalised, and are nothing to the instant the moment stands on - which is all a restart, read as one moment
    /// after another, turns on.</summary>
    [Fact]
    public void TheAppsMomentIsReadAtTheEndsOfAMomentAndWhereverItsHoursAreCarried()
    {
        Assert.Equal(DateTimeOffset.MinValue, Reading("""{ "requests": { }, "startedAt": "0001-01-01T00:00:00Z" }""").StartedAt);
        Assert.Equal(DateTimeOffset.MaxValue, Reading("""{ "requests": { }, "startedAt": "9999-12-31T23:59:59.9999999Z" }""").StartedAt);

        var carried = Reading("""{ "requests": { }, "startedAt": "2026-10-04T23:01:19+02:30" }""").StartedAt;

        Assert.Equal(TimeSpan.FromMinutes(150), carried?.Offset);
        Assert.Equal(Started, carried?.ToUniversalTime());
        Assert.Equal(Started, carried); // one instant, two sayings of it, and the hours kept in the saying

        Assert.Equal(Started, Reading("""{ "requests": { }, "startedAt": " \t 2026-10-04T20:31:19Z " }""").StartedAt);
        Assert.Equal(
            new DateTimeOffset(2026, 10, 4, 21, 0, 0, TimeSpan.Zero),
            Reading("""{ "requests": { }, "startedAt": "2026-10-04T20:31:19Z", "startedAt": "2026-10-04T21:00:00Z" }""").StartedAt);

        var hidden = Reading("""
            { "requests": { "perMinute": 11, "startedAt": "2026-10-04T20:31:19Z" },
              "sql": { "perMinute": 3, "startedAt": "2026-10-04T20:31:19Z" } }
            """);

        Assert.Null(hidden.StartedAt);
        Assert.Equal(11, hidden.Requests);
        Assert.Equal(3, hidden.Sql);
        Assert.Equal(3, hidden.SqlOfTraffic);
        Assert.Equal(Read, hidden.ReadAt); // the reader's moment is the one part an answer cannot supply
    }

    /// <summary>A moment that is no moment is left unsaid, and leaves the rest of the reading whole: an empty string,
    /// a blank, text of some other shape, a month and a day out of range, an hour out of it, a year no moment
    /// reaches, a number of seconds, a flag, nothing at all, an object and an array each say no start - while the
    /// traffic beside them is read as the answer that had every right to be read, its eight other counts at nothing
    /// and its two words made of the silence.</summary>
    [Theory]
    [InlineData(@"""""")]
    [InlineData(@""" """)]
    [InlineData(@"""last Tuesday""")]
    [InlineData(@"""2026-13-45T00:00:00Z""")]
    [InlineData(@"""2026-10-04T25:00:00Z""")]
    [InlineData(@"""0000-01-01T00:00:00Z""")]
    [InlineData("5321")]
    [InlineData("true")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("[]")]
    public void AMomentThatIsNoMomentIsUnsaidAndLeavesTheRestWhole(string startedAt)
    {
        var read = Reading($$"""{ "requests": { "perMinute": 11 }, "startedAt": {{startedAt}} }""");

        Assert.Null(read.StartedAt);
        Assert.Equal(new TelemetrySnapshot(11, 0, 0, 0, null, 0, 0, 0, null, 0, Read), read);
        Assert.Null(read.Process);
        Assert.False(read.SplitsSql);
        Assert.Equal(0, read.SqlOfTraffic);
    }

    /// <summary>The two moments of one reading are two parts and neither is judged: the app's start put at the
    /// reader's moment and the reader's moment put at the app's start is a reading of its own, unequal to the first
    /// and holding each moment on its own property, and a copy that names both turns it back. An answer that says an
    /// app started after the dashboard read it - which no clock of an app agrees to - is a reading all the same,
    /// since the record compares nothing, converts nothing to UTC and asks no clock when it was written.</summary>
    [Fact]
    public void TheTwoMomentsOfAReadingAreKeptApartAndNeitherIsJudged()
    {
        var read = new TelemetrySnapshot(11, 7, 3, 5, 13, 17, 19, 10, 29, 31, Read) { StartedAt = Started };
        var turned = read with { StartedAt = Read, ReadAt = Started };

        Assert.NotEqual(read, turned);
        Assert.Equal(Read, turned.StartedAt);
        Assert.Equal(Started, turned.ReadAt);
        Assert.Equal(read, turned with { StartedAt = Started, ReadAt = Read });

        var late = new TelemetrySnapshot(11, 7, 3, 5, 13, 17, 19, 10, 29, 31, Started) { StartedAt = Read };

        Assert.True(late.StartedAt > late.ReadAt);
        Assert.Equal(Read, late.StartedAt);
        Assert.Equal(Started, late.ReadAt);
        Assert.False(late.SplitsSql);
        Assert.Equal(10, late.SqlOfTraffic);
    }

    /// <summary>A moment is compared by the instant it stands on and only said by the hours it carries: the same
    /// minute told in UTC and two hours east of it is one reading - by the record's equals, by its hash, as a key of
    /// a set and as a key of a dictionary - and two sentences, since the words keep the offset. The same wall clock
    /// at another offset is another instant and so another reading, which is what keeps two nodes in two zones from
    /// being mistaken for one another; the app's own moment is compared the same way.</summary>
    [Fact]
    public void AMomentIsComparedByItsInstantAndOnlySaidByTheHoursItCarries()
    {
        var utc = new TelemetrySnapshot(11, 7, 3, 5, 13, 17, 19, 10, 29, 31, new DateTimeOffset(2026, 10, 4, 20, 0, 0, TimeSpan.Zero));
        var east = utc with { ReadAt = new DateTimeOffset(2026, 10, 4, 22, 0, 0, TimeSpan.FromHours(2)) };
        var west = utc with { ReadAt = new DateTimeOffset(2026, 10, 4, 22, 0, 0, TimeSpan.FromHours(-2)) };

        Assert.True(utc == east);
        Assert.False(utc != east);
        Assert.True(utc.Equals(east));
        Assert.Equal(utc.GetHashCode(), east.GetHashCode());
        Assert.Equal(TimeSpan.FromHours(2), east.ReadAt.Offset);
        Assert.Equal(utc.ReadAt.UtcTicks, east.ReadAt.UtcTicks);
        Assert.NotEqual(utc.ReadAt.Ticks, east.ReadAt.Ticks);

        var readings = new HashSet<TelemetrySnapshot> { utc };
        readings.Add(east);
        Assert.Single(readings);

        var keyed = new Dictionary<TelemetrySnapshot, string> { [utc] = "told in utc" };
        keyed[east] = "told two hours east";

        Assert.Single(keyed);
        Assert.Equal("told two hours east", keyed[utc]);

        // The two readings are one and say two sentences, since the only part either holds is the moment and the
        // hours it was carried in: the sentence keeps the offset the value compares without.
        Assert.NotEqual(utc.ToString(), east.ToString());
        Assert.Equal((utc with { ReadAt = east.ReadAt }).ToString(), east.ToString());

        Assert.NotEqual(utc, west);
        Assert.True(utc != west);

        var startedUtc = utc with { StartedAt = new DateTimeOffset(2026, 10, 4, 20, 31, 19, TimeSpan.Zero) };
        var startedEast = startedUtc with { StartedAt = new DateTimeOffset(2026, 10, 4, 22, 31, 19, TimeSpan.FromHours(2)) };

        Assert.Equal(startedUtc, startedEast);
        Assert.Equal(startedUtc.GetHashCode(), startedEast.GetHashCode());
        Assert.Equal(TimeSpan.FromHours(2), startedEast.StartedAt?.Offset);
        Assert.NotEqual(startedUtc.StartedAt?.Offset, startedEast.StartedAt?.Offset);
    }

    /// <summary>The two operators the record is given stand at nothing on either side: no reading is one with no
    /// reading and is not one with a reading, from either side of the call, and a reading is not nothing by the
    /// method borrowed from <see cref="object" /> either. Two readings of no numbers at all, made apart, are one -
    /// the same nothing hashed and said twice, the nothing an endpoint that never answered leaves a node with - and a
    /// different reading from that same nothing at a moment the reader brought.</summary>
    [Fact]
    public void TheTwoOperatorsStandAtNothingOnEitherSideAndAtTwoReadingsOfNothing()
    {
        TelemetrySnapshot? none = null;
        TelemetrySnapshot? other = null;
        TelemetrySnapshot? reading = new TelemetrySnapshot(11, 7, 3, 5, 13, 17, 19, 10, 29, 31, Read);

        Assert.True(none == other);
        Assert.False(none != other);
        Assert.False(reading == null);
        Assert.True(reading != null);
        Assert.False(null == reading);
        Assert.True(null != reading);
        Assert.False(reading.Equals(none));
        Assert.False(reading.Equals((object?)none));
        Assert.False(none?.Equals(reading) ?? false);

        var quiet = new TelemetrySnapshot(0, 0, 0, 0, null, 0, 0, 0, null, 0, default);
        var alsoQuiet = new TelemetrySnapshot(0, 0, 0, 0, null, 0, 0, 0, null, 0, default);

        Assert.True(quiet == alsoQuiet);
        Assert.False(quiet != alsoQuiet);
        Assert.Equal(quiet.GetHashCode(), alsoQuiet.GetHashCode());
        Assert.Equal(quiet.ToString(), alsoQuiet.ToString());
        Assert.True(quiet != reading);
        Assert.NotEqual(quiet, quiet with { ReadAt = Read });
    }

    /// <summary>An answer read at the moment named, or at the one every reading above stands on when the test names
    /// none: a moment the test brings rather than one a clock is asked for. The nothing
    /// <see cref="TelemetrySnapshot.Parse" /> makes of an answer that is no answer fails here, loudly and with the
    /// text that failed to read, rather than as a null reference two assertions later.</summary>
    private static TelemetrySnapshot Reading(string body, DateTimeOffset? at = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        return TelemetrySnapshot.Parse(body, at ?? Read)
               ?? throw new InvalidOperationException($"the answer was read as no reading: {body}");
    }
}
