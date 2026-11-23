namespace Dashboard.Tests;

/// <summary>
/// The edges of <see cref="ClusterStatus" /> that its other readings leave open, all of them in how the file is
/// <em>spelled</em> rather than in what it says. <see cref="ClusterStatusEdgeCaseTests" /> pins the two numbers of the
/// stale rule to their value, <see cref="ClusterStatus.StaleAfter" /> from an interval absent to
/// <see cref="int.MaxValue" />, <see cref="ClusterStatus.IsStale" /> one tick on either side of an hour,
/// <see cref="ClusterStatus.AsOf" /> under a dozen clocks, the order and the nothing of <see cref="ClusterStatus.Pods" />,
/// the empty array and the name that holds none in <see cref="ClusterStatus.Parse" />, and the record underneath;
/// <see cref="ClusterStatusBoundaryEdgeCaseTests" /> pins the interval at the bottom of the number, a moment handed
/// over at the two ends of the number of moments, a pod listed twice, the byte-order mark, a moment written with an
/// offset or with no zone, the interval given in parts of a second and the age the record says of itself;
/// <see cref="ClusterStatusParseEdgeCaseTests" /> pins the blank text, the empty object, the names spelled in another
/// case, what strict JSON refuses and the depth of a file; <see cref="ClusterParserTests" /> reads the shipped sample
/// and answers a value of another type with no value; <see cref="ClusterAssessmentTests" /> and
/// <see cref="ClusterLivenessEdgeCaseTests" /> step an age across the minute and the hour and name the two constants;
/// and <see cref="ParsedEdgeCaseTests" /> pins the reading a file hands over.
/// <para>
/// The name this issue asked for stands - <see cref="ClusterStatusEdgeCaseTests" /> holds it, as
/// <see cref="SummaryLevelSurfaceEdgeCaseTests" /> explains of its own type - so what none of those reaches is set down
/// here, in a file of its own: the name a file gives twice, which is the name it gives last, whether what stands under
/// it is a fact or the array that decides whether the file has a list at all; the array whose elements are no node and
/// no namespace, which is the empty list and no problem, and the named standing among the unnamed; the moment spelled
/// with a space for its <c>T</c>, padded with blanks, to the half second and to nine digits of it, and spelled as a day
/// with no time of the day, each of them the very instant it names - and the age of the file counted from the ticks of
/// that instant rather than from the second it falls in, which is where <see cref="ClusterStatus.IsStale" /> turns
/// over, a tick on either side of the minute and of the hour; the two ends of the number of moments read out of the
/// words rather than handed over as values; the interval a file gives in exact halves of a second, which are rounded to
/// the even second and carry <see cref="ClusterStatus.StaleAfter" /> with them; and the volumes of a namespace, which
/// are no pod of the status however many of them the space lists.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is. One spelling of <c>generated</c> is left out on
/// purpose: a moment that names no day - <c>"20:15:30"</c> - is read as the day the reader stands in, so the only
/// answer it has is today's, and testing it would be testing the real clock.
/// </para>
/// </summary>
public class ClusterStatusSpellingEdgeCaseTests
{
    /// <summary>The sample's own moment to the half second: the instant every fractional age below is counted from.</summary>
    private static readonly DateTimeOffset Half = Moment(20, 15, 30, 5_000_000, 0);

    // ------------------------------------------------------------- the name a file gives twice

    /// <summary>The name a file gives twice is the name it gives last, whichever of the two a reader would rather have
    /// taken: the moment, the interval and the version all answer the second spelling, and the <c>nodes</c> listed
    /// first are gone. A fact said twice is one fact and no complaint, the file reads, and the age it is held to
    /// follows the interval that was kept rather than the one that was written over.</summary>
    [Fact]
    public void TheLastNameAFileGivesIsTheFactItGives()
    {
        var status = Read(
            """
            { "generated": "2026-10-06T20:15:30Z", "intervalSeconds": 900, "kubernetesVersion": "v1.33.3", "nodes": [ { "name": "node-1" } ],
              "generated": "2026-10-06T21:15:30Z", "intervalSeconds": 15, "kubernetesVersion": "v1.34.1", "nodes": [ { "name": "node-2" } ] }
            """);

        Assert.Equal(new DateTimeOffset(2026, 10, 6, 21, 15, 30, TimeSpan.Zero), status.Generated);
        Assert.Equal(15, status.IntervalSeconds);
        Assert.Equal("v1.34.1", status.KubernetesVersion);
        Assert.Equal(["node-2"], status.Nodes.Select(node => node.Name));
        Assert.Empty(status.Namespaces);

        Assert.Equal(TimeSpan.FromSeconds(60), status.StaleAfter);
        Assert.False(status.IsStale(new DateTimeOffset(2026, 10, 6, 21, 16, 30, TimeSpan.Zero)));
        Assert.True(status.IsStale(new DateTimeOffset(2026, 10, 6, 21, 16, 31, TimeSpan.Zero)));
    }

    /// <summary>The array that decides whether a file lists anything is the array under the <em>last</em> spelling of
    /// the name, and not the one that happens to be an array: the object given first and the array given last is a
    /// file, and the array given first and the object given last is a file with neither list in it - refused for that,
    /// not read as an empty one. The same holds of the other list's name, of a <c>null</c> given the last word, and of
    /// a name given twice with neither spelling an array at all.</summary>
    [Theory]
    [InlineData("""{ "nodes": {}, "nodes": [] }""", true, "")]
    [InlineData("""{ "nodes": [ { "name": "node-1" } ], "nodes": [] }""", true, "")]
    [InlineData("""{ "namespaces": "none", "namespaces": [] }""", true, "")]
    [InlineData("""{ "nodes": null, "nodes": [ { "name": "node-1" } ] }""", true, "node-1")]
    [InlineData("""{ "nodes": {}, "nodes": [ { "name": "node-1" }, { "name": "node-2" } ] }""", true, "node-1,node-2")]
    [InlineData("""{ "nodes": [], "nodes": {} }""", false, "")]
    [InlineData("""{ "nodes": [ { "name": "node-1" } ], "nodes": null }""", false, "")]
    [InlineData("""{ "namespaces": [], "namespaces": {} }""", false, "")]
    [InlineData("""{ "nodes": 3, "nodes": 4 }""", false, "")]
    public void TheLastArrayUnderANameIsTheListTheFileHas(string json, bool read, string names)
    {
        var parsed = ClusterStatus.Parse(json);

        if (!read)
        {
            Assert.Null(parsed.Value);
            Assert.Equal("the file has neither nodes nor namespaces", parsed.Problem);
            return;
        }

        var status = Assert.IsType<ClusterStatus>(parsed.Value);

        Assert.Null(parsed.Problem);
        Assert.Equal(names.Split(',', StringSplitOptions.RemoveEmptyEntries), status.Nodes.Select(node => node.Name));
        Assert.Empty(status.Namespaces);
        Assert.Empty(status.Pods);
    }

    // --------------------------------------------------------- the array that holds nothing of the kind

    /// <summary>Everything an array of nodes or of namespaces may hold that is no node and no namespace - the number,
    /// the text, the two bools, the JSON <c>null</c>, the nested array even one holding a whole node, the object with
    /// nothing in it, and the object whose <c>name</c> is a number, a bool, an array, an object or blank text - is left
    /// out, and the file is read with an empty list where the elements stood. A file that lists nothing of the kind is
    /// a file that lists nothing, which is a file and not a refusal: the name holds an array.</summary>
    [Fact]
    public void AnArrayOfThingsThatAreNoNodeAndNoNamespaceIsAnEmptyList()
    {
        var status = Read(
            """
            { "generated": "2026-10-06T20:15:30Z", "intervalSeconds": 15, "kubernetesVersion": "v1.33.3",
              "nodes": [ 1, 4.5, "text", true, false, null, [], [ { "name": "node-1" } ], {},
                         { "name": 5 }, { "name": true }, { "name": [] }, { "name": { "first": "node-1" } }, { "name": "   " } ],
              "namespaces": [ 0, "", null, true, [], {}, { "name": "\t" } ] }
            """);

        Assert.Empty(status.Nodes);
        Assert.Empty(status.Namespaces);
        Assert.Empty(status.Pods);
        Assert.Equal(ClusterFixture.Generated, status.Generated);
        Assert.Equal(15, status.IntervalSeconds);
        Assert.Equal("v1.33.3", status.KubernetesVersion);
    }

    /// <summary>The unnamed and the impossible do not carry the named down with them: the one node and the two
    /// namespaces standing among the junk are what the file lists, in the order it listed them and with nothing added
    /// for the elements that were left out. A space whose every pod and every volume is left out is a space the status
    /// holds and <see cref="ClusterStatus.Pods" /> says nothing of; a pod that reads is a pod of the space it stands
    /// in.</summary>
    [Fact]
    public void TheNamedStandAmongTheUnnamedAndAreAllTheFileHas()
    {
        var status = Read(
            """
            { "nodes": [ 7, { "name": "node-1" }, null, "text", { "ready": true }, [ { "name": "node-2" } ] ],
              "namespaces": [ {}, { "name": "apps", "pods": [ 1, { "phase": "Running" }, [] ], "volumes": [ "data", 5 ] },
                              { "name": "jobs", "pods": [ { "name": "ui-7" } ] } ] }
            """);

        Assert.Equal(["node-1"], status.Nodes.Select(node => node.Name));
        Assert.Equal(["apps", "jobs"], status.Namespaces.Select(space => space.Name));
        Assert.Empty(status.Namespaces[0].Pods);
        Assert.Empty(status.Namespaces[0].Volumes);
        Assert.Equal(["ui-7"], status.Namespaces[1].Pods.Select(pod => pod.Name));
        Assert.Equal([("jobs", "ui-7")], status.Pods.Select(pair => (pair.Namespace.Name, pair.Pod.Name)));
    }

    // --------------------------------------------------------------- the moment, spelled otherwise

    /// <summary>The moment a file gives is the instant it means however it is spelled: with a space where the <c>T</c>
    /// stands, with that space and no zone at all, padded with blanks on either side, to the half second, to seven
    /// digits of it and to nine - which are rounded to the tick, not cut - with an offset kept, and as a day with no
    /// time of the day, which is the midnight of it. Every one of them is the same instant, so every one of them is
    /// held to the same age; and the moment <see cref="ClusterStatus.AsOf" /> answers with is the one the file
    /// spelled, in the offset it spelled it in.</summary>
    [Theory]
    [InlineData("\"2026-10-06T20:15:30.500Z\"", 20, 15, 30, 5_000_000, 0)]
    [InlineData("\"2026-10-06T20:15:30.5000000Z\"", 20, 15, 30, 5_000_000, 0)]
    [InlineData("\"2026-10-06T20:15:30.5Z\"", 20, 15, 30, 5_000_000, 0)]
    [InlineData("\"2026-10-06T20:15:30.9999999Z\"", 20, 15, 30, 9_999_999, 0)]
    [InlineData("\"2026-10-06T20:15:30.123456789Z\"", 20, 15, 30, 1_234_568, 0)]
    [InlineData("\"2026-10-06 20:15:30Z\"", 20, 15, 30, 0, 0)]
    [InlineData("\"2026-10-06 20:15:30\"", 20, 15, 30, 0, 0)]
    [InlineData("\"  2026-10-06T20:15:30Z  \"", 20, 15, 30, 0, 0)]
    [InlineData("\"2026-10-06T22:15:30.5+02:00\"", 22, 15, 30, 5_000_000, 2)]
    [InlineData("\"2026-10-06T15:15:30.5-05:00\"", 15, 15, 30, 5_000_000, -5)]
    [InlineData("\"2026-10-06\"", 0, 0, 0, 0, 0)]
    public void AMomentSpelledOtherwiseIsTheInstantItNames(string written, int hour, int minute, int second, int ticks, int offsetHours)
    {
        var expected = Moment(hour, minute, second, ticks, offsetHours);
        var status = Read($$"""{ "generated": {{written}}, "intervalSeconds": 15, "nodes": [] }""");

        Assert.NotNull(status.Generated);
        Assert.Equal(expected.UtcDateTime, status.Generated!.Value.UtcDateTime);
        Assert.Equal(expected.Offset, status.Generated.Value.Offset);
        Assert.Equal(TimeSpan.FromSeconds(60), status.StaleAfter);
        Assert.Equal(expected.UtcDateTime, status.AsOf(ClusterFixture.Generated).UtcDateTime);
        Assert.Equal(expected.Offset, status.AsOf(ClusterFixture.Generated).Offset);

        // One instant is one age: the half second a file names makes it neither nearer stale nor further from it.
        Assert.False(status.IsStale(status.Generated.Value + TimeSpan.FromSeconds(60)));
        Assert.True(status.IsStale(status.Generated.Value + TimeSpan.FromSeconds(61)));
    }

    /// <summary>A day with no time of the day is the midnight that opens it, in UTC - and not the day the reader
    /// stands in, which is what a moment that names no day at all would have been, and the reason none is tested
    /// here. A collector that writes a date only is held to the same minute as any other file, and the page's own
    /// sample moment is twenty hours and odd minutes past the midnight such a file names, so it is stale at that
    /// clock and fresh at the midnight itself; a day the file names that has not come yet is no age at all, and a
    /// clock standing behind the file is never stale however far behind it stands.</summary>
    [Fact]
    public void ADayWithNoTimeOfTheDayIsTheMidnightThatOpensIt()
    {
        var status = Read("""{ "generated": "2026-10-06", "intervalSeconds": 15, "nodes": [] }""");
        var midnight = new DateTimeOffset(2026, 10, 6, 0, 0, 0, TimeSpan.Zero);

        Assert.Equal(midnight, status.Generated);
        Assert.Equal(TimeSpan.Zero, status.Generated!.Value.Offset);
        Assert.Equal(midnight, status.AsOf(ClusterFixture.Generated));
        Assert.False(status.IsStale(midnight));
        Assert.False(status.IsStale(midnight + TimeSpan.FromMinutes(1)));
        Assert.True(status.IsStale(midnight + TimeSpan.FromMinutes(2)));
        Assert.True(status.IsStale(ClusterFixture.Generated));
        Assert.False(status.IsStale(midnight - TimeSpan.FromMinutes(1)));

        Assert.True(Read("""{ "generated": "2026-10-05", "nodes": [] }""").IsStale(midnight));
        Assert.False(Read("""{ "generated": "2026-10-07", "nodes": [] }""").IsStale(midnight));
    }

    /// <summary>The two ends of the number of moments, read out of the words rather than handed over as values: the
    /// first instant a file may name comes back as <see cref="DateTimeOffset.MinValue" /> and the last as
    /// <see cref="DateTimeOffset.MaxValue" /> to the tick, each keeping the offset it was spelled with. The age between
    /// them is a <see cref="TimeSpan" /> and no overflow, and the rule judges each of them by its own instant: the
    /// first file is stale at the sample's moment and turns over on its own minute, and the last is fresh at the top of
    /// the number, where its age is nothing, as it is at the bottom of it, where the clock has not reached it.</summary>
    [Fact]
    public void TheEndsOfTheNumberOfMomentsComeOutOfAFileAsTheyGoIn()
    {
        var first = Read("""{ "generated": "0001-01-01T00:00:00.0000000+00:00", "intervalSeconds": 15, "nodes": [] }""");
        var last = Read("""{ "generated": "9999-12-31T23:59:59.9999999Z", "intervalSeconds": 15, "nodes": [] }""");

        Assert.Equal(DateTimeOffset.MinValue, first.Generated);
        Assert.Equal(DateTimeOffset.MaxValue, last.Generated);
        Assert.Equal(TimeSpan.Zero, first.Generated!.Value.Offset);
        Assert.Equal(TimeSpan.Zero, last.Generated!.Value.Offset);
        Assert.Equal(TimeSpan.FromSeconds(60), first.StaleAfter);
        Assert.Equal(TimeSpan.FromSeconds(60), last.StaleAfter);

        Assert.True(first.IsStale(ClusterFixture.Generated));
        Assert.False(first.IsStale(DateTimeOffset.MinValue + TimeSpan.FromSeconds(60)));
        Assert.True(first.IsStale(DateTimeOffset.MinValue + TimeSpan.FromSeconds(61)));
        Assert.False(last.IsStale(ClusterFixture.Generated));
        Assert.False(last.IsStale(DateTimeOffset.MaxValue));

        Assert.Equal(DateTimeOffset.MinValue, first.AsOf(DateTimeOffset.MaxValue));
        Assert.Equal(DateTimeOffset.MaxValue, last.AsOf(DateTimeOffset.MinValue));

        // The whole of the number between the two files' moments is a TimeSpan of its own, and no overflow.
        Assert.Equal(3_155_378_975_999_999_999L, last.Generated!.Value.Ticks);
        Assert.Equal(new TimeSpan(3_155_378_975_999_999_999L), last.Generated!.Value - first.Generated!.Value);
    }

    // ------------------------------------------------------- the age, counted to the tick of the moment

    /// <summary>The age is counted from the ticks of the moment the file names and not from the second it falls in: a
    /// file written at half past the second is fresh at the age itself, fresh one tick under it, stale one tick past
    /// it, and stale a second past it - the rule is <em>older than</em>, whatever the words of the file were and
    /// however often the collector writes. The minute is the floor an absent interval stands at, the minute four
    /// quarters of it make, and the hour four quarters of an hour.</summary>
    [Theory]
    [InlineData("15", 60)]
    [InlineData("900", 3600)]
    [InlineData("", 60)]
    public void TheAgeTurnsOverOnATickOfTheFilesOwnHalfSecond(string interval, int staleAfterSeconds)
    {
        var named = interval.Length == 0 ? "" : $$""" "intervalSeconds": {{interval}}, """;
        var status = Read($$"""{ "generated": "2026-10-06T20:15:30.5Z", {{named}} "nodes": [] }""");
        var age = TimeSpan.FromSeconds(staleAfterSeconds);

        Assert.Equal(Half, status.Generated);
        Assert.Equal(5_000_000L, status.Generated!.Value.Ticks % TimeSpan.TicksPerSecond);
        Assert.Equal(age, status.StaleAfter);

        Assert.False(status.IsStale(Half + age));
        Assert.False(status.IsStale(Half + age - TimeSpan.FromTicks(1)));
        Assert.False(status.IsStale(Half + age - TimeSpan.FromSeconds(1)));
        Assert.True(status.IsStale(Half + age + TimeSpan.FromTicks(1)));
        Assert.True(status.IsStale(Half + age + TimeSpan.FromSeconds(1)));

        // The instant the facts are of is the file's own, half second and all, at the tick the file turns stale at.
        Assert.Equal(Half, status.AsOf(Half + age + TimeSpan.FromTicks(1)));
    }

    // ---------------------------------------------------- the interval a file gives in halves of a second

    /// <summary>The interval a file gives in exact halves of a second is rounded to the even second, which is what
    /// <see cref="Math.Round(double)" /> does and not what a reader who assumed the common rounding would say: the
    /// half under the first interval whose four are more than the minute is rounded down to it and stands at the
    /// minute, the half over it is rounded up, and <see cref="ClusterStatus.StaleAfter" /> follows the second kept
    /// rather than the number written. A half that is negative is no interval at all, being below nothing, and leaves
    /// the file at the minute.</summary>
    [Theory]
    [InlineData("0.5", 0, 60)]
    [InlineData("1.5", 2, 60)]
    [InlineData("2.5", 2, 60)]
    [InlineData("14.5", 14, 60)]
    [InlineData("15.5", 16, 64)]
    [InlineData("16.5", 16, 64)]
    [InlineData("17.5", 18, 72)]
    [InlineData("20.5", 20, 80)]
    [InlineData("21.5", 22, 88)]
    [InlineData("899.5", 900, 3600)]
    [InlineData("900.5", 900, 3600)]
    [InlineData("-0.5", null, 60)]
    public void AHalfSecondOfAnIntervalIsRoundedToTheEvenSecond(string written, int? expected, int staleAfterSeconds)
    {
        var status = Read($$"""{ "generated": "2026-10-06T20:15:30Z", "intervalSeconds": {{written}}, "nodes": [] }""");
        var age = TimeSpan.FromSeconds(staleAfterSeconds);

        Assert.Equal(ClusterFixture.Generated, status.Generated);
        Assert.Equal(expected, status.IntervalSeconds);
        Assert.Equal(age, status.StaleAfter);
        Assert.True(status.StaleAfter >= ClusterStatus.MinimumStaleAfter);

        // The age the file is held to is the age it turns on, counted from a moment the same file names.
        Assert.False(status.IsStale(ClusterFixture.Generated + age));
        Assert.True(status.IsStale(ClusterFixture.Generated + age + TimeSpan.FromSeconds(1)));
    }

    // ------------------------------------------------------------------- the volumes of a namespace are no pods

    /// <summary>A namespace's volumes are no pod of the status, however many of them it lists and whatever one of
    /// them is called: the space of volumes and no pod lists no pair, the pairs of a space that lists both are its
    /// pods in their order, and the namespace each pair carries is the space itself - its volumes still on it, the
    /// very list - rather than a copy of it with the volumes taken off. A space with neither is held to the same
    /// nothing as a status with no space at all.</summary>
    [Fact]
    public void TheVolumesOfANamespaceAreNoPodsOfTheStatus()
    {
        ClusterVolume[] volumes = [new("data", 8589934592d, "Bound"), new("logs", null, "Pending")];
        var stored = new ClusterNamespace("stateful", [], volumes);
        var ui = ClusterFixture.Pod("ui");
        var db = ClusterFixture.Pod("db");
        var mixed = new ClusterNamespace("apps", [ui, db], volumes);
        var empty = new ClusterNamespace("jobs", [], []);
        var status = ClusterFixture.Status([], stored, mixed, empty);

        Assert.Equal(2, stored.Volumes.Count);
        Assert.Empty(stored.Pods);
        Assert.Equal([("apps", "ui-0"), ("apps", "db-0")], status.Pods.Select(pair => (pair.Namespace.Name, pair.Pod.Name)));
        Assert.Equal(2, status.Pods.Count());
        Assert.DoesNotContain(status.Pods, pair => pair.Namespace.Name != "apps");
        Assert.All(
            status.Pods.ToArray(),
            pair =>
            {
                Assert.Same(mixed, pair.Namespace);
                Assert.Same(volumes, pair.Namespace.Volumes);
                Assert.Equal(2, pair.Namespace.Volumes.Count);
            });
    }

    // ------------------------------------------------------------------------------------------------ helpers

    /// <summary>The day every moment below stands in, the time of it, the ticks past that second and the offset it is
    /// spelled in: what the words of a file mean, built out of the calendar rather than out of a parser.</summary>
    private static DateTimeOffset Moment(int hour, int minute, int second, int ticks, int offsetHours) =>
        new DateTimeOffset(2026, 10, 6, hour, minute, second, TimeSpan.FromHours(offsetHours)).AddTicks(ticks);

    /// <summary>The status a file holds. A file the reader answers with a problem fails here with that problem to
    /// read, so the tests above say what the status is rather than that there is one.</summary>
    private static ClusterStatus Read(string json)
    {
        var parsed = ClusterStatus.Parse(json);

        Assert.Null(parsed.Problem);
        Assert.NotNull(parsed.Value);

        return parsed.Value!;
    }
}
