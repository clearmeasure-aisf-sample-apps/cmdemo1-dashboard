using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="TelemetrySnapshot" />: one node's own count of its
/// calls over the last minute, as its <c>/_telemetry</c> endpoint answers it. A reading holds eleven parts the
/// constructor always asks for - the counters of traffic and of checks, the two percentiles of a minute's work and
/// the moment it was read at - four more an app may leave out (<see cref="TelemetrySnapshot.Process" />,
/// <see cref="TelemetrySnapshot.StartedAt" />, <see cref="TelemetrySnapshot.SqlRequests" />,
/// <see cref="TelemetrySnapshot.SqlBackground" />), and the two words those four make
/// (<see cref="TelemetrySnapshot.SplitsSql" />, <see cref="TelemetrySnapshot.SqlOfTraffic" />).
/// <para>
/// What the other files already pin is walked past here. <see cref="TelemetryTests" /> reads the shipped answer
/// through <see cref="TelemetrySnapshot.Parse" /> and pins its eleven numbers as the eleven the answer says under
/// <c>requests</c>, <c>probes</c>, <c>sql</c> and <c>http</c>, the answer that is no JSON at all (an error page, an
/// array, an object with no <c>requests</c>), the sections an answer leaves out counting nothing and the latencies it
/// leaves out saying nothing, and the SQL of requests and of the background read where an app tells them apart - with
/// the total staying the whole of it for an app that tells neither and for an app that tells only one;
/// <see cref="ProcessVitalsTests" /> pins the <c>process</c> object and the <c>startedAt</c> beside it, and every
/// answer that carries no usable vitals;
/// <see cref="TargetTileClusterCardEnvironmentDeliveryTests" /> pins <see cref="SplitsSql" /> and
/// <see cref="SqlOfTraffic" /> on the two readings a database arrow is built from;
/// <see cref="NodeObservationEdgeCaseTests" /> pins that a reading of no numbers is still a reading and not the
/// nothing a node that never reported one lands on, that twelve requests are not thirteen and one error is not none,
/// that a reading is a key of its own value inside an observation, and that it opens that observation's words with
/// its own; and <see cref="JsonReadTests" /> pins the shared reader of an answer's parts - which this file's type
/// does not use, since <see cref="TelemetrySnapshot" /> keeps three private readers of its own.
/// </para>
/// <para>
/// What follows is the rest of every member. The eleven parts landing on the property of the same name, by position
/// and by name, and the four an app need not tell being nothing until they are told - a zero the app said and the
/// nothing it did not, kept apart; the seven counts held as the number they were given at both ends of
/// <see cref="int" /> and the two latencies at the ends of a number that may also be nothing, with nothing clamped,
/// signed or rounded by the record; the moment being the one the reader brought, at both ends of
/// <see cref="DateTimeOffset" /> and with the hours it carries, and never one the snapshot makes. Then the two words,
/// where the readings above leave them: a telling-apart of two zeros being a telling-apart whose traffic is nothing
/// although the total is ten, the queries of requests standing outside the total when an app says more of them than
/// of it, the whole of it falling back the moment either half is left unsaid - though a half said as zero is said,
/// and says a traffic of nothing - and neither word moved by the thirteen parts it does not read.
/// Then <see cref="Parse" /> at the ends its two readers leave open: an answer
/// that is no JSON whatever the shape of the nothing (nothing at all, blank, a byte-order mark, a sentence cut short,
/// text after the braces, a root that is a number or a flag or nothing), an answer that is refused by name before it
/// is read, an answer whose <c>requests</c> is there but is no object, a section that is an array or a text or a
/// flag, a name that holds no number, a number below zero, a count told in parts - rounded to the nearest whole and a
/// half to the even side of it, which leaves a percentile of half a millisecond saying zero rather than nothing -
/// and a number past the reach of an <see cref="int" /> saying the whole of one, where the shared reader says nothing.
/// Then the record the compiler made around the fifteen: two readings being one when all fifteen parts are, boxed
/// and unboxed and as a key, with each part alone telling them apart and a process that says the same seven numbers
/// being no reading; the eleven counters reading out in the order they went in; a copy changing the part it names and
/// carrying the other fourteen along, the nested vitals with them; the words a reading says itself naming all
/// seventeen of its parts - the fifteen it holds and the two it makes - and the moment among them following whoever
/// reads it while the numbers do not; and the public surface, so that the day a twelfth counter or a fifth optional
/// part is added, this file says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a reading holds the moment it was read
/// at as the caller gives it and makes none of its own, <see cref="Parse" /> is handed one rather than going to find
/// one, and every number below is one the test brought or an answer the test wrote.
/// </para>
/// </summary>
public class TelemetrySnapshotEdgeCaseTests
{
    /// <summary>The moment every reading below stands on: one the test brought, not the clock.</summary>
    private static readonly DateTimeOffset Read = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>The moment the app of the same answer says its process started: an hour, nine minutes and nine
    /// seconds before the reading, so that the two moments of a reading are never one another.</summary>
    private static readonly DateTimeOffset Started = new(2026, 10, 4, 20, 31, 19, TimeSpan.Zero);

    /// <summary>A reading that says all fifteen of its parts, every count a number no other part carries: a part
    /// handed to the wrong property shows as the swap it is instead of matching the part it landed on.</summary>
    private static TelemetrySnapshot Full => new(11, 7, 3, 5, 13, 17, 19, 10, 29, 31, Read)
    {
        Process = new ProcessVitals(3.2, 412, 96, 41, 2, 0, 5321),
        StartedAt = Started,
        SqlRequests = 6,
        SqlBackground = 4,
    };

    /// <summary>The fifteen parts as the record declares them, in that order: the list the surface test walks for
    /// the setter every part has and the two words computed out of them have not.</summary>
    private static readonly string[] Parts =
    [
        "Requests",
        "FromFrontDoor",
        "Direct",
        "Errors",
        "P95Ms",
        "Probes",
        "FrontDoorProbes",
        "Sql",
        "SqlP95Ms",
        "Http",
        "ReadAt",
        "Process",
        "StartedAt",
        "SqlRequests",
        "SqlBackground",
    ];

    /// <summary>The two words a reading makes of the parts it holds: the pair the surface test walks for the getter
    /// they have and the setter they have not.</summary>
    private static readonly string[] Computed = ["SplitsSql", "SqlOfTraffic"];

    /// <summary>An answer that says all of <see cref="Full" /> but its process, under the names the endpoint writes
    /// them in - the moment it was read at being the reader's own and not a name in the answer - for the tests that
    /// take a reading out of an answer rather than writing one out.</summary>
    private const string Traffic = """
        { "windowSeconds": 60, "startedAt": "2026-10-04T20:31:19Z",
          "requests": { "perMinute": 11, "frontDoor": 7, "direct": 3, "errors": 5, "p95Ms": 13 },
          "probes": { "perMinute": 17, "frontDoor": 19 },
          "sql": { "perMinute": 10, "p95Ms": 29, "requests": 6, "background": 4 },
          "http": { "perMinute": 31 } }
        """;

    // ----------------------------------------------------------- the fifteen parts a reading holds

    /// <summary>The eleven counters and the moment, position by position, each coming back as the property of the
    /// same name, and the four parts an app may leave out landing on theirs: the order <see cref="Parse" /> hands an
    /// answer's values over in and the tiles and the trends take them out in.</summary>
    [Fact]
    public void TheConstructorHandsEveryArgumentToThePropertyOfItsName()
    {
        var read = Full;

        Assert.Equal(11, read.Requests);
        Assert.Equal(7, read.FromFrontDoor);
        Assert.Equal(3, read.Direct);
        Assert.Equal(5, read.Errors);
        Assert.Equal(13, read.P95Ms);
        Assert.Equal(17, read.Probes);
        Assert.Equal(19, read.FrontDoorProbes);
        Assert.Equal(10, read.Sql);
        Assert.Equal(29, read.SqlP95Ms);
        Assert.Equal(31, read.Http);
        Assert.Equal(Read, read.ReadAt);
        Assert.Equal(new ProcessVitals(3.2, 412, 96, 41, 2, 0, 5321), read.Process);
        Assert.Equal(Started, read.StartedAt);
        Assert.Equal(6, read.SqlRequests);
        Assert.Equal(4, read.SqlBackground);
    }

    /// <summary>The same fifteen named at the call rather than standing in a row, and out of their order at that:
    /// every part lands on the property of the name it is given, which is how a test or a tile builds the one reading
    /// it means instead of counting places in a list of eleven.</summary>
    [Fact]
    public void TheConstructorTakesEveryPartByNameAsWellAsByPosition()
    {
        var read = new TelemetrySnapshot(
            ReadAt: Read,
            Http: 31,
            SqlP95Ms: 29,
            Sql: 10,
            FrontDoorProbes: 19,
            Probes: 17,
            P95Ms: 13,
            Errors: 5,
            Direct: 3,
            FromFrontDoor: 7,
            Requests: 11)
        {
            SqlBackground = 4,
            SqlRequests = 6,
            StartedAt = Started,
            Process = new ProcessVitals(3.2, 412, 96, 41, 2, 0, 5321),
        };

        Assert.Equal(Full, read);
        Assert.NotSame(Full, read);
        Assert.Equal(Read, read.ReadAt);
        Assert.Equal(13, read.P95Ms);
        Assert.Equal(4, read.SqlBackground);
        Assert.Equal(new ProcessVitals(3.2, 412, 96, 41, 2, 0, 5321), read.Process);
    }

    /// <summary>A reading of the eleven counters alone says nothing beyond them - no process, no start, and no
    /// telling the SQL of its requests from the rest - and every one of those nothings is kept apart from the zero an
    /// app that measured nothing would have said. The two percentiles are the two counters a node may keep to itself,
    /// and the keeping is a nothing the tiles leave out rather than a <c>0 ms</c> they would show.</summary>
    [Fact]
    public void TheFourPartsAnAppNeedNotTellAreNothingUntilTheyAreTold()
    {
        var counters = new TelemetrySnapshot(11, 7, 3, 5, null, 17, 19, 10, null, 31, Read);

        Assert.Null(counters.Process);
        Assert.Null(counters.StartedAt);
        Assert.Null(counters.SqlRequests);
        Assert.Null(counters.SqlBackground);
        Assert.Null(counters.P95Ms);
        Assert.Null(counters.SqlP95Ms);

        Assert.NotEqual(counters, counters with { P95Ms = 0 });
        Assert.NotEqual(counters, counters with { SqlP95Ms = 0 });
        Assert.NotEqual(counters, counters with { SqlRequests = 0 });
        Assert.NotEqual(counters, counters with { SqlBackground = 0 });
        Assert.NotEqual(counters, counters with { StartedAt = DateTimeOffset.MinValue });
        Assert.NotEqual(counters, counters with { Process = new ProcessVitals(null, null, null, null, null, null, null) });

        // Given all six of them at once, the reading is the one that says everything.
        Assert.Equal(
            Full,
            counters with { P95Ms = 13, SqlP95Ms = 29, Process = Full.Process, StartedAt = Started, SqlRequests = 6, SqlBackground = 4 });
    }

    /// <summary>The seven counts are the numbers they were given, at both ends of <see cref="int" /> and below zero
    /// at that, and the two percentiles are the same numbers or nothing at all: the record keeps a reading, it does
    /// not judge one. It adds no floor at zero, no ceiling at the top of the type and no sign, since it is the
    /// reading of the answer - not the record - that a count may not be negative.</summary>
    [Theory]
    [InlineData(0, 0, 0, 0, null, 0, 0, 0, null, 0)]
    [InlineData(-1, -1, -1, -1, null, -1, -1, -1, null, -1)]
    [InlineData(11, 7, 3, 5, 13, 17, 19, 10, 29, 31)]
    [InlineData(0, 0, 0, 0, 0, 0, 0, 0, 0, 0)] // the two latencies said as nothing are nothing said, not left unsaid
    [InlineData(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue)]
    [InlineData(int.MinValue, int.MinValue, int.MinValue, int.MinValue, int.MinValue, int.MinValue, int.MinValue, int.MinValue, int.MinValue, int.MinValue)]
    public void EveryCountIsTheNumberItWasGivenAtEveryEndOfInt(
        int requests,
        int fromFrontDoor,
        int direct,
        int errors,
        int? p95Ms,
        int probes,
        int frontDoorProbes,
        int sql,
        int? sqlP95Ms,
        int http)
    {
        var read = new TelemetrySnapshot(requests, fromFrontDoor, direct, errors, p95Ms, probes, frontDoorProbes, sql, sqlP95Ms, http, Read);

        Assert.Equal(requests, read.Requests);
        Assert.Equal(fromFrontDoor, read.FromFrontDoor);
        Assert.Equal(direct, read.Direct);
        Assert.Equal(errors, read.Errors);
        Assert.Equal(p95Ms, read.P95Ms);
        Assert.Equal(probes, read.Probes);
        Assert.Equal(frontDoorProbes, read.FrontDoorProbes);
        Assert.Equal(sql, read.Sql);
        Assert.Equal(sqlP95Ms, read.SqlP95Ms);
        Assert.Equal(http, read.Http);
        Assert.Equal(Read, read.ReadAt);
    }

    /// <summary>The moment a reading stands on is the one its reader brought and never one the snapshot goes and
    /// finds: at either end of <see cref="DateTimeOffset" /> and with the hours the moment was carried in - which the
    /// record neither normalises to UTC nor drops. Two readings of one answer two minutes apart are two readings,
    /// alike but for the moment, and the answer's own <c>startedAt</c> stays the app's moment whatever the reader's
    /// is.</summary>
    [Fact]
    public void TheMomentIsTheOneTheReaderBroughtAndNotOneTheSnapshotMakes()
    {
        DateTimeOffset[] moments =
        [
            DateTimeOffset.MinValue,
            DateTimeOffset.MaxValue,
            Read,
            new DateTimeOffset(2026, 10, 4, 22, 0, 0, TimeSpan.FromHours(2)),
            new DateTimeOffset(2026, 10, 4, 22, 0, 0, TimeSpan.FromMinutes(-330)),
        ];

        foreach (var moment in moments)
        {
            Assert.Equal(moment, new TelemetrySnapshot(11, 7, 3, 5, 13, 17, 19, 10, 29, 31, moment).ReadAt);

            var parsed = Reading("""{ "requests": { "perMinute": 11 }, "startedAt": "2026-10-04T20:31:19Z" }""", moment);

            Assert.Equal(moment, parsed.ReadAt);
            Assert.Equal(moment.Offset, parsed.ReadAt.Offset);
            Assert.Equal(moment.Ticks, parsed.ReadAt.Ticks);

            // The app's moment and the reader's are two parts of one reading, and neither stands in the other's place.
            Assert.Equal(Started, parsed.StartedAt);
            Assert.NotEqual(parsed.StartedAt, parsed.ReadAt);
        }

        var early = Reading(Traffic);
        var late = Reading(Traffic, Read.AddMinutes(2));

        Assert.NotEqual(early, late);
        Assert.Equal(early with { ReadAt = late.ReadAt }, late);
    }

    // ------------------------------------------------------------------ the two words the SQL parts make

    /// <summary>A telling-apart of two zeros is a telling-apart: an app that says its traffic caused none of its ten
    /// commands does tell them apart, and what its traffic causes is nothing - not the ten, which is what an app that
    /// says neither would have been held to. Take either half back and the word turns, on the very answer that says
    /// zero.</summary>
    [Fact]
    public void ATellApartOfTwoZerosIsATellApartAndItsTrafficIsNothing()
    {
        var idle = Full with { Sql = 10, SqlRequests = 0, SqlBackground = 0 };

        Assert.True(idle.SplitsSql);
        Assert.Equal(0, idle.SqlOfTraffic);
        Assert.NotEqual(idle.Sql, idle.SqlOfTraffic);

        Assert.False((idle with { SqlRequests = null }).SplitsSql);
        Assert.Equal(10, (idle with { SqlRequests = null }).SqlOfTraffic);
        Assert.False((idle with { SqlBackground = null }).SplitsSql);
        Assert.Equal(10, (idle with { SqlBackground = null }).SqlOfTraffic);
        Assert.False((idle with { SqlRequests = null, SqlBackground = null }).SplitsSql);

        // The same two zeros read out of an answer are the same telling-apart.
        var answered = Reading("""{ "requests": { }, "sql": { "perMinute": 10, "requests": 0, "background": 0 } }""");

        Assert.True(answered.SplitsSql);
        Assert.Equal(0, answered.SqlOfTraffic);
    }

    /// <summary>What the traffic causes is the number the app says under <c>sql.requests</c> and no measure of
    /// anything else: more of them than of the total it is a part of, the whole of an <see cref="int" /> past the
    /// total, and a negative the record was never asked to check. Only the two halves together decide the word - the
    /// total, the percentile and the thirteen other parts of the reading stand outside both of them.</summary>
    [Theory]
    [InlineData(4, 9, 0, true, 9)] // the requests' commands more than all of them, said as the app says them
    [InlineData(0, int.MaxValue, 0, true, int.MaxValue)] // a number no answer should reach, held as it was held
    [InlineData(10, -5, -2, true, -5)] // below zero, and still a telling-apart
    [InlineData(10, -1, 0, true, -1)]
    [InlineData(10, 0, 0, true, 0)]
    [InlineData(10, null, null, false, 10)]
    [InlineData(0, null, 4, false, 0)]
    [InlineData(0, 4, null, false, 0)]
    [InlineData(-7, null, null, false, -7)] // a total below zero stays the total when nothing tells it apart
    public void TheTrafficQueriesAreTheNumberSaidAndNotAMeasureOfTheTotal(
        int sql,
        int? requests,
        int? background,
        bool splits,
        int ofTraffic)
    {
        var read = Full with { Sql = sql, SqlRequests = requests, SqlBackground = background };

        Assert.Equal(splits, read.SplitsSql);
        Assert.Equal(ofTraffic, read.SqlOfTraffic);
    }

    /// <summary>Neither word reads anything but its two halves: the same two halves under eleven different readings -
    /// the traffic doubled, the errors raised, the percentile taken away, the process swapped for none and the moment
    /// moved - say one telling-apart and one number, and the words themselves ask nothing of a clock or an endpoint.</summary>
    [Fact]
    public void NeitherWordIsMovedByThePartsItDoesNotRead()
    {
        var splits = Full with { Sql = 10, SqlRequests = 6, SqlBackground = 4 };

        TelemetrySnapshot[] sameTwoHalves =
        [
            splits,
            splits with { Requests = 22, FromFrontDoor = 14, Direct = 6 },
            splits with { Errors = 9 },
            splits with { P95Ms = null },
            splits with { P95Ms = 4_000 },
            splits with { Probes = 1, FrontDoorProbes = 0 },
            splits with { SqlP95Ms = null },
            splits with { Http = 0 },
            splits with { ReadAt = Read.AddHours(-9) },
            splits with { Process = null },
            splits with { StartedAt = null },
            splits with { StartedAt = DateTimeOffset.MaxValue },
        ];

        Assert.All(sameTwoHalves, read =>
        {
            Assert.True(read.SplitsSql);
            Assert.Equal(6, read.SqlOfTraffic);
        });

        // And the same is true of the fallback: no part but the two halves turns an app that does not tell apart.
        Assert.All(sameTwoHalves, read =>
        {
            Assert.False((read with { SqlBackground = null }).SplitsSql);
            Assert.Equal(read.Sql, (read with { SqlBackground = null }).SqlOfTraffic);
        });
    }

    // ---------------------------------------------------------------------- an answer that is no answer

    /// <summary>An answer the endpoint never wrote is no reading, by either of the two ways there is of not being
    /// one: the text that is no JSON at all - nothing, blank, a byte-order mark in front of otherwise good braces, a
    /// sentence cut short, good JSON with a sentence after it - and good JSON that is no object: nothing at all where
    /// an answer should be, a flag, a number, a piece of text. The first of those is the reader's own catch of a bad
    /// answer, the second the shape rule the next test is about; both end on the same nothing a tile shows for an app
    /// without telemetry. An error page and an array, which the first of the two also catches,
    /// <see cref="TelemetryTests" /> pins.</summary>
    [Theory]
    [InlineData("")] // the empty body of an endpoint that answered with nothing
    [InlineData("   ")] // and one that answered with blanks
    [InlineData("\t\n")]
    [InlineData("\uFEFF{ \"requests\": { \"perMinute\": 11 } }")] // a marked answer is not an answer
    [InlineData("{")] // cut short
    [InlineData("{ \"requests\": ")]
    [InlineData("{ \"requests\": {} } extra")] // and read past its end
    [InlineData("null")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("3.5")]
    [InlineData("-1")]
    [InlineData("\"requests\"")]
    public void AnAnswerTheEndpointNeverWroteIsNoReading(string body) =>
        Assert.Null(TelemetrySnapshot.Parse(body, Read));

    /// <summary>An answer that is not there at all is refused before it is read, by the name of the part that is
    /// missing: the nothing of <see langword="null" /> is the caller's mistake to hear, and not the shape of an
    /// answer a node could have sent. The nothing a reader is left with after a call that went through is the other
    /// nothing - the test above.</summary>
    [Fact]
    public void AnAnswerThatIsNotThereAtAllIsRefusedByName()
    {
        var refused = Assert.Throws<ArgumentNullException>(() => TelemetrySnapshot.Parse(null!, Read));

        Assert.Equal("json", refused.ParamName);
    }

    /// <summary>The one part an answer must be an object about is there but is no object: the traffic named as
    /// nothing, as a number, as text, as a flag, or as the array an older node might answer with - and, at the other
    /// end of the same rule, an answer that names every section but the traffic. Each of them is no reading, however
    /// much of the rest it says: the counters of probes, of SQL and of the calls out have nothing to stand on.</summary>
    [Theory]
    [InlineData("{ \"requests\": null }")]
    [InlineData("{ \"requests\": 11 }")]
    [InlineData("{ \"requests\": true }")]
    [InlineData("{ \"requests\": \"busy\" }")]
    [InlineData("{ \"requests\": [ ] }")]
    [InlineData("{ \"requests\": [ { \"perMinute\": 11 } ] }")]
    [InlineData("""{ "probes": { "perMinute": 17 }, "sql": { "perMinute": 10 }, "http": { "perMinute": 31 } }""")]
    public void TheTrafficIsTheOnePartAnAnswerMustSayAsAnObject(string body) => Assert.Null(TelemetrySnapshot.Parse(body, Read));

    /// <summary>Of the three sections an answer may leave out, the ones it says as something other than an object are
    /// left out as surely as the ones that are missing: an array, an array with a reading in it, a number, a word,
    /// a flag and nothing at all each count nothing, say no percentile and tell nothing apart - while the traffic
    /// beside them is read as the answer that had every right to be read. The empty object, which counts the same
    /// nothing, <see cref="TelemetryTests" /> pins.</summary>
    [Theory]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("[ { \"perMinute\": 9 } ]")]
    [InlineData("\"busy\"")]
    [InlineData("9")]
    [InlineData("true")]
    public void ASectionThatIsNoObjectCountsNothing(string section)
    {
        var read = Reading($$"""{ "requests": { "perMinute": 11 }, "probes": {{section}}, "sql": {{section}}, "http": {{section}} }""");

        Assert.Equal(new TelemetrySnapshot(11, 0, 0, 0, null, 0, 0, 0, null, 0, Read), read);
        Assert.Null(read.SqlRequests);
        Assert.Null(read.SqlBackground);
        Assert.False(read.SplitsSql);
    }

    /// <summary>A name an answer says as something other than a number is a name it kept quiet: text of the right
    /// shape, a flag, nothing at all, an object and an array leave the eight counters at zero and the four numbers an
    /// answer may have no notion of - the two percentiles and the two halves of its SQL - at nothing. Every name in
    /// the answer is read by the same rule, so one answer that puts a wrong shape under all of them at once is enough
    /// to say it.</summary>
    [Fact]
    public void ANameThatIsNoNumberIsANameTheAppKeptQuiet()
    {
        var read = Reading("""
            { "requests": { "perMinute": "11", "frontDoor": true, "direct": null, "errors": { }, "p95Ms": [ ] },
              "probes": { "perMinute": "17", "frontDoor": false },
              "sql": { "perMinute": "10", "p95Ms": { "ms": 29 }, "requests": "6", "background": null },
              "http": { "perMinute": [31] } }
            """);

        Assert.Equal(new TelemetrySnapshot(0, 0, 0, 0, null, 0, 0, 0, null, 0, Read), read);
        Assert.Null(read.SqlRequests);
        Assert.Null(read.SqlBackground);
        Assert.False(read.SplitsSql);
        Assert.Null(read.StartedAt);
        Assert.Null(read.Process);
    }

    /// <summary>A number below zero is a number the reader refuses, and the refusal looks like silence: the counters
    /// the app says negatively count nothing, the percentiles it says negatively are unsaid, and the halves of the SQL
    /// it says negatively are unsaid - which takes the telling-apart away with them, so the traffic the answer causes
    /// falls back on the total. The record itself checks nothing of the kind: the same negative handed to it by a
    /// caller is the negative it holds, which the theory above pins.</summary>
    [Fact]
    public void ANumberBelowZeroIsSaidAsTheNothingItsReaderMakesOfIt()
    {
        var read = Reading("""
            { "requests": { "perMinute": -5, "frontDoor": -0.5, "direct": -1, "errors": -1, "p95Ms": -1 },
              "probes": { "perMinute": -1, "frontDoor": -1 },
              "sql": { "perMinute": -10, "p95Ms": -1, "requests": -6, "background": -4 },
              "http": { "perMinute": -1 } }
            """);

        Assert.Equal(new TelemetrySnapshot(0, 0, 0, 0, null, 0, 0, 0, null, 0, Read), read);
        Assert.Null(read.SqlRequests);
        Assert.Null(read.SqlBackground);
        Assert.False(read.SplitsSql);
        Assert.Equal(0, read.SqlOfTraffic);
    }

    // ---------------------------------------------------------------- the numbers an answer is made of

    /// <summary>A count told in parts is rounded to the whole number nearest it, and an exact half to whichever of
    /// the two whole numbers beside it is even: two and a half is two, three and a half is four, one and a half is
    /// two and a half of nothing is nothing. It is the rounding of <see cref="Math.Round(double)" /> with no second
    /// argument, which a percentile of half a millisecond shows - and shows as a zero, not as the nothing of a
    /// percentile the app did not say.</summary>
    [Theory]
    [InlineData("0", 0)]
    [InlineData("0.4", 0)]
    [InlineData("0.5", 0)]
    [InlineData("0.6", 1)]
    [InlineData("1.4", 1)]
    [InlineData("1.5", 2)]
    [InlineData("2.4", 2)]
    [InlineData("2.5", 2)]
    [InlineData("2.5000001", 3)]
    [InlineData("2.6", 3)]
    [InlineData("3.5", 4)]
    [InlineData("4.5", 4)]
    [InlineData("85.0", 85)]
    [InlineData("84.999999", 85)]
    public void ACountToldInPartsIsRoundedToTheWholeBesideIt(string number, int expected)
    {
        // Both readers of a number - the one that makes a missing count a zero and the one that lets a percentile
        // stay missing - go through the same rounding, so the two names say the same thing of the same number.
        Assert.Equal(expected, Reading($$"""{ "requests": { "perMinute": {{number}} } }""").Requests);
        Assert.Equal(expected, Reading($$"""{ "requests": { "p95Ms": {{number}} } }""").P95Ms);
    }

    /// <summary>The halves of every one of the eleven numbers an answer counts, said in the same answer: the traffic
    /// and its parts, both percentiles, the checks of the minute and Front Door's, the SQL, its two halves and the
    /// calls out. Each is rounded on its own to the even number beside it - thirteen and a half of them fourteen and
    /// twelve and a half of them twelve - and a count that rounds onto nothing is the nothing it rounds to, which the
    /// tiles show as zero and not as a dash since the app did say a number. The two halves of the SQL round into a
    /// telling-apart, so the traffic is read as the rounded half of it rather than the total.</summary>
    [Fact]
    public void EveryHalfOfAnAnswerRoundsOnItsOwn() =>
        Assert.Equal(
            new TelemetrySnapshot(2, 4, 0, 2, 12, 14, 2, 2, 2, 2, Read) { SqlRequests = 2, SqlBackground = 2 },
            Reading("""
                { "requests": { "perMinute": 2.5, "frontDoor": 3.5, "direct": 0.5, "errors": 1.5, "p95Ms": 12.5 },
                  "probes": { "perMinute": 13.5, "frontDoor": 2.5 },
                  "sql": { "perMinute": 2.5, "p95Ms": 2.5, "requests": 2.5, "background": 2.5 },
                  "http": { "perMinute": 1.5 } }
                """));

    /// <summary>A count past the reach of an <see cref="int" /> is the whole of one and not the nothing the answer
    /// cannot hold: the reader checks that a number is not below zero and never that it is small enough, so a node
    /// that counted a billion more calls than an <see cref="int" /> can say is heard as the top of one - the
    /// saturation the runtime makes of a cast that has nowhere to go. The shared reader of an answer's parts, which
    /// <see cref="JsonReadTests" /> pins, refuses the same number as too large instead; this file's type keeps a
    /// reader of its own, and the two part company here - which is why the number the tiles show for a node that
    /// counted impossibly much is the top of an <see cref="int" /> and not a dash.</summary>
    [Theory]
    [InlineData("2147483647")]
    [InlineData("2147483647.4")]
    [InlineData("3000000000")]
    [InlineData("1e12")]
    [InlineData("1e21")]
    public void ACountPastTheReachOfAnIntIsTheWholeOfOne(string number)
    {
        var body = $$"""
            { "requests": { "perMinute": {{number}}, "p95Ms": {{number}} },
              "sql": { "perMinute": {{number}}, "p95Ms": {{number}}, "requests": {{number}}, "background": {{number}} } }
            """;

        var read = Reading(body);

        Assert.Equal(int.MaxValue, read.Requests);
        Assert.Equal(int.MaxValue, read.P95Ms);
        Assert.Equal(int.MaxValue, read.Sql);
        Assert.Equal(int.MaxValue, read.SqlP95Ms);
        Assert.Equal(int.MaxValue, read.SqlRequests);
        Assert.Equal(int.MaxValue, read.SqlBackground);

        // Both halves are there, so the answer does tell its SQL apart - on a number no tile should ever show.
        Assert.True(read.SplitsSql);
        Assert.Equal(int.MaxValue, read.SqlOfTraffic);
    }

    /// <summary>An app that says nothing of SQL tells nothing apart and causes nothing of note: its total is the
    /// zero of a name the answer never wrote - a number, and not the nothing of the two percentiles beside it - so
    /// both words come out as the silence they were read from. The reading of both halves where an app does tell them
    /// apart, and the fallback where it tells one or neither, <see cref="TelemetryTests" /> pins.</summary>
    [Fact]
    public void AnAnswerThatSaysNoSqlTellsNothingApart()
    {
        var read = Reading("""{ "requests": { "perMinute": 11 } }""");

        Assert.Equal(0, read.Sql);
        Assert.Null(read.SqlP95Ms);
        Assert.Null(read.SqlRequests);
        Assert.Null(read.SqlBackground);
        Assert.False(read.SplitsSql);
        Assert.Equal(0, read.SqlOfTraffic);
    }

    // -------------------------------------------------------------- the record the compiler made around them

    /// <summary>Two readings are one when all fifteen of their parts are, whichever of the four ways of asking is
    /// used - the record's own <c>Equals</c>, the one borrowed from <see cref="object" />, <c>==</c> and <c>!=</c> -
    /// and a reading is one with itself. What is not one with it: the same reading with any one of the fifteen moved,
    /// dropped to nothing or raised from nothing, <see langword="null" />, a number, an anonymous shape carrying the
    /// same eleven counters and a <see cref="ProcessVitals" /> that says seven of them again - a reading is not a
    /// process that counts alike. The hash follows the value, boxed and unboxed, and the nothing of a percentile is
    /// hashed with the very same nothing.</summary>
    [Fact]
    public void TwoReadingsAreOneWhenAllFifteenPartsAre()
    {
        var one = Full;
        var same = new TelemetrySnapshot(11, 7, 3, 5, 13, 17, 19, 10, 29, 31, Read)
        {
            Process = new ProcessVitals(3.2, 412, 96, 41, 2, 0, 5321),
            StartedAt = Started,
            SqlRequests = 6,
            SqlBackground = 4,
        };

        Assert.True(one.Equals(same));
        Assert.True(one == same);
        Assert.False(one != same);
        Assert.True(one.Equals((object)same));
        Assert.Equal(one, same);
        Assert.NotSame(one, same);
        Assert.Equal(one.GetHashCode(), same.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.True(one.Equals(one));

        TelemetrySnapshot[] different =
        [
            one with { Requests = 12 },
            one with { FromFrontDoor = 8 },
            one with { Direct = 4 },
            one with { Errors = 6 },
            one with { P95Ms = null },
            one with { P95Ms = 0 },
            one with { Probes = 18 },
            one with { FrontDoorProbes = 20 },
            one with { Sql = 11 },
            one with { SqlP95Ms = null },
            one with { SqlP95Ms = 0 },
            one with { Http = 32 },
            one with { ReadAt = Read.AddSeconds(1) },
            one with { Process = null },
            one with { Process = new ProcessVitals(3.2, 412, 96, 41, 2, 0, 5322) },
            one with { StartedAt = null },
            one with { StartedAt = Read },
            one with { SqlRequests = null },
            one with { SqlRequests = 0 },
            one with { SqlBackground = null },
            one with { SqlBackground = 0 },
        ];

        foreach (var other in different)
        {
            Assert.NotEqual(one, other);
            Assert.False(one.Equals(other));
            Assert.False(one.Equals((object)other));
            Assert.True(one != other);
        }

        Assert.False(one.Equals(null));
        Assert.False(one.Equals((TelemetrySnapshot?)null));
        Assert.False(one.Equals(11));
        Assert.False(one.Equals(new { Requests = 11, FromFrontDoor = 7 }));
        Assert.False(one.Equals(new ProcessVitals(11d, 7d, 3d, 5, 13, 17, 19)));
        Assert.False(new TelemetrySnapshot(0, 0, 0, 0, null, 0, 0, 0, null, 0, default).Equals(one));
    }

    /// <summary>The same reading twice is one reading, whether the two come from one answer, from two answers or
    /// from a call that writes the numbers out again: what the history of a node keeps are values, so a reading heard
    /// twice is said of no more than once, and a reading with one part moved - two calls more, a percentile left unsaid,
    /// a moment two minutes on - is an entry of its own. The nothing an answer leaves out is a value too, apart from
    /// the zero an app that measured a quiet minute would have said.</summary>
    [Fact]
    public void AReadingIsAKeyOfItsOwnValue()
    {
        var readings = new HashSet<TelemetrySnapshot> { Full };

        // The fifteen parts heard a second time, from the answer and from a call, are the one entry they started on.
        readings.Add(Reading(Traffic) with { Process = Full.Process });
        readings.Add(new TelemetrySnapshot(11, 7, 3, 5, 13, 17, 19, 10, 29, 31, Read)
        {
            Process = new ProcessVitals(3.2, 412, 96, 41, 2, 0, 5321),
            StartedAt = Started,
            SqlRequests = 6,
            SqlBackground = 4,
        });

        Assert.Single(readings);

        readings.Add(Reading(Traffic)); // the same answer, saying no process
        readings.Add(Full with { Requests = 12 });
        readings.Add(Full with { P95Ms = null });
        readings.Add(Full with { ReadAt = Read.AddMinutes(2) });

        Assert.Equal(5, readings.Count);

        var keyed = new Dictionary<TelemetrySnapshot, string>
        {
            [new TelemetrySnapshot(0, 0, 0, 0, null, 0, 0, 0, null, 0, Read)] = "nothing said",
        };

        keyed[Reading("""{ "requests": { } }""")] = "still nothing said";
        keyed[Full] = "everything said";
        keyed[new TelemetrySnapshot(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, Read)] = "ten zeros";

        Assert.Equal(3, keyed.Count);
        Assert.Equal("still nothing said", keyed[Reading("""{ "requests": { "perMinute": 0 } }""")]);
        Assert.Equal("everything said", keyed[Reading(Traffic) with { Process = Full.Process }]);
        Assert.Equal("ten zeros", keyed[new TelemetrySnapshot(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, Read)]);
    }

    /// <summary>The eleven counters and the moment come back out in the order they went in - the order the record
    /// declares, which is the order <see cref="Parse" /> fills them in - and the two percentiles that were never
    /// said come out as the nothing they were left as rather than as the zero an app that measured a quiet minute
    /// would have said.</summary>
    [Fact]
    public void TheElevenCountersComeOutAgainInTheOrderTheyWentIn()
    {
        var (requests, fromFrontDoor, direct, errors, p95Ms, probes, frontDoorProbes, sql, sqlP95Ms, http, readAt) = Full;

        Assert.Equal(11, requests);
        Assert.Equal(7, fromFrontDoor);
        Assert.Equal(3, direct);
        Assert.Equal(5, errors);
        Assert.Equal(13, p95Ms);
        Assert.Equal(17, probes);
        Assert.Equal(19, frontDoorProbes);
        Assert.Equal(10, sql);
        Assert.Equal(29, sqlP95Ms);
        Assert.Equal(31, http);
        Assert.Equal(Read, readAt);

        var (quiet, _, _, _, quietP95, _, _, _, quietSqlP95, _, quietAt) =
            new TelemetrySnapshot(0, 0, 0, 0, null, 0, 0, 0, null, 0, Read);

        Assert.Equal(0, quiet);
        Assert.Null(quietP95);
        Assert.Null(quietSqlP95);
        Assert.Equal(Read, quietAt);
    }

    /// <summary>A copy changes the part it names and carries the other fourteen along: the reading it was copied
    /// from stands as it stood, the parts it does not name are the parts that were, and the part it does is the only
    /// one the words of the reading then says differently. A nested process is copied as the reference it is rather
    /// than rebuilt, a copy that names all fifteen parts is one with the reading it was made from and not the same
    /// instance, and the two words computed out of the parts follow whichever of them the copy moved.</summary>
    [Fact]
    public void ACopyChangesThePartItNamesAndCarriesTheOtherFourteen()
    {
        var original = Full;
        var copied = original with { Http = 44 };

        Assert.Equal(31, original.Http);
        Assert.Equal(44, copied.Http);
        Assert.Equal(11, copied.Requests);
        Assert.Equal(7, copied.FromFrontDoor);
        Assert.Equal(3, copied.Direct);
        Assert.Equal(5, copied.Errors);
        Assert.Equal(13, copied.P95Ms);
        Assert.Equal(17, copied.Probes);
        Assert.Equal(19, copied.FrontDoorProbes);
        Assert.Equal(10, copied.Sql);
        Assert.Equal(29, copied.SqlP95Ms);
        Assert.Equal(Read, copied.ReadAt);
        Assert.Equal(Started, copied.StartedAt);
        Assert.Equal(6, copied.SqlRequests);
        Assert.Equal(4, copied.SqlBackground);
        Assert.Same(original.Process, copied.Process);
        Assert.NotEqual(original, copied);

        var fromNothing = new TelemetrySnapshot(0, 0, 0, 0, null, 0, 0, 0, null, 0, Read) with { SqlRequests = 6 };

        Assert.Equal(6, fromNothing.SqlRequests);
        Assert.Null(fromNothing.SqlBackground);
        Assert.Null(fromNothing.P95Ms);
        Assert.Null(fromNothing.Process);
        Assert.Null(fromNothing.StartedAt);
        Assert.False(fromNothing.SplitsSql);
        Assert.Equal(0, fromNothing.SqlOfTraffic);

        // Naming every part the value it already has makes one reading with it, of its own.
        var everyPart = original with
        {
            Requests = 11,
            FromFrontDoor = 7,
            Direct = 3,
            Errors = 5,
            P95Ms = 13,
            Probes = 17,
            FrontDoorProbes = 19,
            Sql = 10,
            SqlP95Ms = 29,
            Http = 31,
            ReadAt = Read,
            Process = original.Process,
            StartedAt = Started,
            SqlRequests = 6,
            SqlBackground = 4,
        };

        Assert.Equal(original, everyPart);
        Assert.NotSame(original, everyPart);

        // And the words the parts make move with the parts, whichever of the two halves a copy takes away.
        Assert.Equal(6, original.SqlOfTraffic);
        Assert.Equal(10, (original with { SqlRequests = null }).SqlOfTraffic);
        Assert.Equal(9, (original with { SqlRequests = 9, SqlBackground = 99 }).SqlOfTraffic);
    }

    /// <summary>The words a reading says itself name seventeen parts by the names the record declares, in that
    /// order: the eleven the constructor always asks for, the four an app may leave out and the two computed words
    /// nobody sent - the last six of them so that a reader of the record's own sentence sees what an answer had said
    /// nothing of. The parts an app left out are said as nothing and not as zero, and the reading of an answer is
    /// said in the same words as the same numbers written out by hand, which is the equality above in the words the
    /// debug log prints.</summary>
    [Fact]
    public void AReadingSaysItsFifteenPartsAndTheTwoWordsItMakes()
    {
        Assert.Equal(
            "TelemetrySnapshot { Requests = 11, FromFrontDoor = 7, Direct = 3, Errors = 5, P95Ms = 13, Probes = 17, FrontDoorProbes = 19, Sql = 10, SqlP95Ms = 29, Http = 31, "
            + "ReadAt = 10/04/2026 22:00:00 +00:00, Process = ProcessVitals { CpuPercent = 3.2, WorkingSetMb = 412, GcHeapMb = 96, Threads = 41, InFlight = 2, ExceptionsPerMinute = 0, UptimeSeconds = 5321, RestartedRecently = False }, "
            + "StartedAt = 10/04/2026 20:31:19 +00:00, SqlRequests = 6, SqlBackground = 4, SplitsSql = True, SqlOfTraffic = 6 }",
            Words(Full));

        Assert.Equal(
            "TelemetrySnapshot { Requests = 0, FromFrontDoor = 0, Direct = 0, Errors = 0, P95Ms = , Probes = 0, FrontDoorProbes = 0, Sql = 0, SqlP95Ms = , Http = 0, "
            + "ReadAt = 10/04/2026 22:00:00 +00:00, Process = , StartedAt = , SqlRequests = , SqlBackground = , SplitsSql = False, SqlOfTraffic = 0 }",
            Words(new TelemetrySnapshot(0, 0, 0, 0, null, 0, 0, 0, null, 0, Read)));

        // The words follow the parts and nothing else: one letter of difference in a part, and the sentence differs.
        Assert.NotEqual(Words(Full), Words(Full with { Requests = 12 }));
        Assert.NotEqual(Words(Full), Words(new TelemetrySnapshot(11, 7, 3, 5, 13, 17, 19, 10, 29, 31, Read)));
        Assert.Equal(Words(Full), Words(Reading(Traffic) with { Process = Full.Process }));
        Assert.DoesNotContain("TelemetrySnapshot { perMinute", Words(Full), StringComparison.Ordinal);
    }

    /// <summary>The seventeen parts are numbers, and no reader's culture turns what a reading holds; it turns only
    /// the words the reading says of them, and among those the two moments it holds - a day and a month that change
    /// places, and the decimal point of the process's cpu that becomes a comma. A reader of German still compares a
    /// reading with the ones an invariant reader made, still hashes it with them and still reads the same two words
    /// out of the same two halves of its SQL.</summary>
    [Fact]
    public void ThePartsAreHeldAlikeWhateverTheReaderSpeaksAndOnlySaidApart()
    {
        var madeByAGerman = new TelemetrySnapshot(11, 7, 3, 5, 13, 17, 19, 10, 29, 31, Read)
        {
            Process = new ProcessVitals(3.2, 412, 96, 41, 2, 0, 5321),
            StartedAt = Started,
            SqlRequests = 6,
            SqlBackground = 4,
        };

        ViewerCulture.German(() =>
        {
            Assert.Contains("ReadAt = 04.10.2026 22:00:00 +00:00", Full.ToString(), StringComparison.Ordinal);
            Assert.Contains("CpuPercent = 3,2", Full.ToString(), StringComparison.Ordinal);
            Assert.Contains("StartedAt = 04.10.2026 20:31:19 +00:00", Full.ToString(), StringComparison.Ordinal);

            // The numbers themselves, the two words they add up to and the equality between two readings are
            // nothing for a reader to turn.
            Assert.Contains("Requests = 11, FromFrontDoor = 7", Full.ToString(), StringComparison.Ordinal);
            Assert.True(Full == madeByAGerman);
            Assert.Equal(Full.GetHashCode(), madeByAGerman.GetHashCode());
            Assert.True(Full.SplitsSql);
            Assert.Equal(6, Full.SqlOfTraffic);
        });

        Assert.Equal(Words(Full), Words(madeByAGerman));
    }

    /// <summary>The reading's whole public surface, as it stands: the fifteen parts, in the order the record
    /// declares them, the two words out of the SQL parts of them, the static <see cref="Parse" /> - the one member
    /// that reads an answer rather than holding one - and nothing else to call. Every part has a getter and an
    /// init-only setter, since a reading is written once by the reader and copied by no code at all; the two words
    /// have a getter only, being what the parts make of themselves. The private helpers that read the answer are no
    /// members of this list, and the record's compiler-named clone is walked past with the other names the compiler
    /// chose. A twelfth counter, a fifth optional part or a third computed word has to be written here as well,
    /// which is the point: the dashboard cannot be shown a number nobody decided what means.</summary>
    [Fact]
    public void AReadingIsFifteenPartsTwoWordsAndAnAnswerAndNothingElse()
    {
        Assert.Equal(
            [.. Parts, .. Computed],
            [.. typeof(TelemetrySnapshot)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            [
                "Deconstruct",
                "Direct",
                "Equals",
                "Errors",
                "FromFrontDoor",
                "FrontDoorProbes",
                "GetHashCode",
                "Http",
                "P95Ms",
                "Parse",
                "Probes",
                "Process",
                "ReadAt",
                "Requests",
                "SplitsSql",
                "Sql",
                "SqlBackground",
                "SqlOfTraffic",
                "SqlP95Ms",
                "SqlRequests",
                "StartedAt",
                "ToString",
                "op_Equality",
                "op_Inequality",
            ],
            PublicSurface(typeof(TelemetrySnapshot)));

        var properties = typeof(TelemetrySnapshot)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(Parts.Length + 2, properties.Count);
        Assert.Empty(typeof(TelemetrySnapshot).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly));

        Assert.Equal(typeof(int), properties["Requests"].PropertyType);
        Assert.Equal(typeof(int), properties["FromFrontDoor"].PropertyType);
        Assert.Equal(typeof(int), properties["Direct"].PropertyType);
        Assert.Equal(typeof(int), properties["Errors"].PropertyType);
        Assert.Equal(typeof(int?), properties["P95Ms"].PropertyType);
        Assert.Equal(typeof(int), properties["Probes"].PropertyType);
        Assert.Equal(typeof(int), properties["FrontDoorProbes"].PropertyType);
        Assert.Equal(typeof(int), properties["Sql"].PropertyType);
        Assert.Equal(typeof(int?), properties["SqlP95Ms"].PropertyType);
        Assert.Equal(typeof(int), properties["Http"].PropertyType);
        Assert.Equal(typeof(DateTimeOffset), properties["ReadAt"].PropertyType);
        Assert.Equal(typeof(ProcessVitals), properties["Process"].PropertyType);
        Assert.Equal(typeof(DateTimeOffset?), properties["StartedAt"].PropertyType);
        Assert.Equal(typeof(int?), properties["SqlRequests"].PropertyType);
        Assert.Equal(typeof(int?), properties["SqlBackground"].PropertyType);
        Assert.Equal(typeof(bool), properties["SplitsSql"].PropertyType);
        Assert.Equal(typeof(int), properties["SqlOfTraffic"].PropertyType);

        // The fifteen parts may be given a value, and given one only as the reading is made; the two words may not,
        // being nothing an answer sends.
        Assert.All(Parts, name =>
        {
            Assert.True(properties[name].CanRead);
            Assert.True(properties[name].CanWrite);
            Assert.Contains(
                typeof(global::System.Runtime.CompilerServices.IsExternalInit),
                properties[name].SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
        });

        Assert.False(properties["SplitsSql"].CanWrite);
        Assert.False(properties["SqlOfTraffic"].CanWrite);
        Assert.Null(properties["SplitsSql"].GetSetMethod());
        Assert.Null(properties["SqlOfTraffic"].GetSetMethod());
        Assert.All(Computed, name => Assert.False(properties[name].GetMethod!.IsStatic));

        // The eleven parts the constructor always asks for, in the order it asks for them, none of them optional at
        // the call - which is what makes a nothing a part says the app's own nothing rather than a default.
        var constructor = typeof(TelemetrySnapshot).GetConstructors().Single();

        Assert.Equal(
            ["Requests", "FromFrontDoor", "Direct", "Errors", "P95Ms", "Probes", "FrontDoorProbes", "Sql", "SqlP95Ms", "Http", "ReadAt"],
            [.. constructor.GetParameters().Select(parameter => parameter.Name!)]);
        Assert.All(constructor.GetParameters(), parameter => Assert.False(parameter.IsOptional));
        Assert.Equal(
            [
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(int?),
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(int?),
                typeof(int),
                typeof(DateTimeOffset),
            ],
            [.. constructor.GetParameters().Select(parameter => parameter.ParameterType)]);

        // The eleven come out as the eleven types they went in, in the order they went in.
        var deconstruct = typeof(TelemetrySnapshot).GetMethod("Deconstruct")!;

        Assert.True(deconstruct.IsPublic);
        Assert.Equal(
            [.. constructor.GetParameters().Select(parameter => parameter.ParameterType)],
            [.. deconstruct.GetParameters()
                .Select(parameter => parameter.ParameterType.IsByRef ? parameter.ParameterType.GetElementType()! : parameter.ParameterType)]);

        // The one member that reads rather than holds: an answer and the moment it was read at, in, and a reading or
        // no reading out. It is static, it is the only static, and it holds no clock of its own.
        var parse = typeof(TelemetrySnapshot).GetMethod("Parse")!;

        Assert.True(parse.IsStatic);
        Assert.True(parse.IsPublic);
        Assert.Single(
            typeof(TelemetrySnapshot).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly),
            method => method.Name == "Parse" && !method.IsSpecialName);
        Assert.Equal([typeof(string), typeof(DateTimeOffset)], [.. parse.GetParameters().Select(parameter => parameter.ParameterType)]);
        Assert.Equal(["json", "readAt"], [.. parse.GetParameters().Select(parameter => parameter.Name!)]);
        Assert.Equal(typeof(TelemetrySnapshot), parse.ReturnType);

        // A record of values: sealed, compared by what it holds, with no state to ask a network or a clock about.
        Assert.True(typeof(TelemetrySnapshot).IsSealed);
        Assert.Equal(typeof(object), typeof(TelemetrySnapshot).BaseType);
        Assert.IsAssignableFrom<IEquatable<TelemetrySnapshot>>(Full);
    }

    // ------------------------------------------------------------------------------------ the machinery

    /// <summary>An answer read at the moment named, or at the one every reading below stands on when the test names
    /// none: a moment the test brings rather than one a clock is asked for. The nothing <see cref="Parse" /> makes of
    /// an answer that is no answer fails here, loudly and with the text that failed to read, rather than as a null
    /// reference two assertions later.</summary>
    private static TelemetrySnapshot Reading(string body, DateTimeOffset? at = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        return TelemetrySnapshot.Parse(body, at ?? Read)
               ?? throw new InvalidOperationException($"the answer was read as no reading: {body}");
    }

    /// <summary>The public members a type declares itself: its properties and its public fields and the methods
    /// that are not the get and the set of one - the two operators kept although the compiler names them specially,
    /// since they are the <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone
    /// walked past. The constructor is no method of this list, so it is named by the test above rather than here.</summary>
    private static List<string> PublicSurface(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return [.. type
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is FieldInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
    }

    /// <summary>The words a reading says its fifteen parts and its two words in, taken in the invariant culture
    /// whatever the reader of this test happens to be: the parts are numbers and moments and the sentence is the
    /// point, so the culture is only the way to say it once - which of the two readers turns which part is pinned on
    /// its own above.</summary>
    private static string Words(TelemetrySnapshot read)
    {
        ArgumentNullException.ThrowIfNull(read);

        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            return read.ToString();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }
}
