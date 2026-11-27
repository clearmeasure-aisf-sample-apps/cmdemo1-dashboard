namespace Dashboard.Tests;

/// <summary>
/// The ends of the numbers <see cref="ClusterStatus" /> is made of, which the readings of it leave open.
/// <see cref="ClusterStatusEdgeCaseTests" /> pins the two constants of the stale rule, <see cref="ClusterStatus.StaleAfter" />
/// from an interval absent to <see cref="int.MaxValue" />, <see cref="ClusterStatus.IsStale" /> one tick on either side
/// of an hour, <see cref="ClusterStatus.AsOf" /> under a dozen clocks, the order and the nothing of
/// <see cref="ClusterStatus.Pods" />, the empty array and the name that holds none in <see cref="ClusterStatus.Parse" />,
/// and the record underneath; <see cref="ClusterStatusParseEdgeCaseTests" /> pins the blank text, the empty object, the
/// names spelled another case, what strict JSON refuses and the depth of a file; <see cref="ClusterParserTests" />
/// reads the shipped sample part for part and answers a value of another type with no value;
/// <see cref="ClusterAssessmentTests" /> steps an age across 60 and 61 and 120 and 121; and
/// <see cref="ParsedEdgeCaseTests" /> pins the floor and the minute the two agree at.
/// <para>
/// What is pinned here is only what none of those reach: the interval at the bottom of the number, which is a negative
/// age the minute answers for and no overflow; a file's own moment at the two ends of the number of moments, where the
/// age is the whole of it and where it is nothing because the clock has not reached the file; the pod listed under two
/// namespaces, which is two pairs, and the list a status holds rather than a copy of it, which
/// <see cref="ClusterStatus.Pods" /> reads again every time it is read; the byte-order mark, which is neither blank text
/// nor a file; a moment written with an offset, or with no zone at all, or with words, in the file itself; the interval
/// a file gives in parts of a second or in more than the number holds; and the age the record says of itself at the
/// ends of the interval.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every moment is one the test brings and every
/// file is text handed over.
/// </para>
/// </summary>
public class ClusterStatusBoundaryEdgeCaseTests
{
    /// <summary>The moment the sample file was written; every age below is counted from it.</summary>
    private static readonly DateTimeOffset Generated = ClusterFixture.Generated;

    // -------------------------------------------------------------------------------------------- StaleAfter

    /// <summary>The interval at the bottom of the number: four of them are an age behind nothing, which the minute
    /// answers for. The rule counts them in a double, so the whole of the number is a <see cref="TimeSpan" /> and no
    /// overflow, and a file that says the bottom of it is held to the same minute as a file that says no interval at
    /// all - which is also the minute the next instant past it goes stale at.</summary>
    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(int.MinValue + 1)]
    [InlineData(-1)]
    public void AnIntervalAtTheBottomOfTheNumberIsStillAMinute(int interval)
    {
        var status = ClusterFixture.Status() with { IntervalSeconds = interval };

        Assert.Equal(interval, status.IntervalSeconds);
        Assert.Equal(ClusterStatus.MinimumStaleAfter, status.StaleAfter);
        Assert.True(TimeSpan.FromSeconds(4d * interval) < ClusterStatus.MinimumStaleAfter);
        Assert.False(status.IsStale(Generated + ClusterStatus.MinimumStaleAfter));
        Assert.True(status.IsStale(Generated + ClusterStatus.MinimumStaleAfter + TimeSpan.FromSeconds(1)));
    }

    // --------------------------------------------------------------------------------------- IsStale and AsOf

    /// <summary>A file whose own moment is the end of the number of moments: the first file there is at the bottom of
    /// it, so it is fresh at that instant, stale one second past its minute, and stale at the top of the number - the
    /// age between the two ends is a <see cref="TimeSpan" /> and no overflow. The last file stands at the top of it,
    /// so a clock at the bottom has not reached it and none of the number is a minute past it; and each of them is as
    /// of its own end, whichever clock asks.</summary>
    [Fact]
    public void AFileAtEitherEndOfTheNumberOfMomentsIsJudgedByItsOwnInstant()
    {
        var first = ClusterFixture.Status() with { Generated = DateTimeOffset.MinValue };
        var last = ClusterFixture.Status() with { Generated = DateTimeOffset.MaxValue };

        Assert.False(first.IsStale(DateTimeOffset.MinValue));
        Assert.False(first.IsStale(DateTimeOffset.MinValue + ClusterStatus.MinimumStaleAfter));
        Assert.True(first.IsStale(DateTimeOffset.MinValue + ClusterStatus.MinimumStaleAfter + TimeSpan.FromSeconds(1)));
        Assert.True(first.IsStale(DateTimeOffset.MaxValue));
        Assert.Equal(DateTimeOffset.MinValue, first.AsOf(DateTimeOffset.MaxValue));

        Assert.False(last.IsStale(DateTimeOffset.MinValue));
        Assert.False(last.IsStale(DateTimeOffset.MaxValue));
        Assert.Equal(DateTimeOffset.MaxValue, last.AsOf(DateTimeOffset.MinValue));

        // The two ends are two instants a whole world of age apart, and each status is as of the moment it says
        // rather than of the age that moment has run into.
        Assert.NotEqual(first, last);
        Assert.Equal(first.AsOf(DateTimeOffset.MinValue), first.AsOf(DateTimeOffset.MaxValue));
        Assert.Equal(last.AsOf(DateTimeOffset.MaxValue), last.AsOf(DateTimeOffset.MinValue));
    }

    // ------------------------------------------------------------------------------------------------ Pods

    /// <summary>The pairs are the namespaces' pods with no de-duplication and no counting of them: the very pod listed
    /// under two namespaces is a pair of each, and the order is the order of the namespaces then of the pods - three
    /// pairs out of two namespaces and two pods, with the shared one standing twice.</summary>
    [Fact]
    public void APodListedUnderTwoNamespacesIsAPairOfEach()
    {
        var shared = ClusterFixture.Pod("ui");
        var apps = ClusterFixture.Space("apps", shared);
        var jobs = ClusterFixture.Space("jobs", shared, ClusterFixture.Pod("db"));
        var status = ClusterFixture.Status([], apps, jobs);

        Assert.Equal(
            [("apps", "ui-0"), ("jobs", "ui-0"), ("jobs", "db-0")],
            status.Pods.Select(pair => (pair.Namespace.Name, pair.Pod.Name)));

        var pairs = status.Pods.ToArray();

        Assert.Equal(2, pairs.Count(pair => ReferenceEquals(pair.Pod, shared)));
        Assert.Same(apps, pairs[0].Namespace);
        Assert.Same(jobs, pairs[1].Namespace);
        Assert.Same(shared, pairs[0].Pod);
        Assert.Same(shared, pairs[1].Pod);
    }

    /// <summary>A status keeps the list it was handed rather than a copy of it, and
    /// <see cref="ClusterStatus.Pods" /> is a reading of that list made each time it is read: the same status asked
    /// twice answers the same pairs, the very instances, and a namespace swapped into the list the status holds is in
    /// the answer to the next asking - which is what <see cref="ClusterStatus.Parse" /> relies on when it hands a
    /// status the array it filled itself.</summary>
    [Fact]
    public void ThePodsAreReadFromTheNamespacesTheStatusHoldsEachTimeTheyAreRead()
    {
        var ui = ClusterFixture.Pod("ui");
        ClusterNamespace[] spaces = [ClusterFixture.Space("apps", ui)];
        var status = ClusterFixture.Status([], spaces);

        Assert.Same(spaces, status.Namespaces);
        Assert.Equal(["apps/ui-0"], status.Pods.Select(pair => $"{pair.Namespace.Name}/{pair.Pod.Name}"));

        var first = status.Pods.ToArray();
        var second = status.Pods.ToArray();

        Assert.Equal(first.Select(pair => $"{pair.Namespace.Name}/{pair.Pod.Name}"), second.Select(pair => $"{pair.Namespace.Name}/{pair.Pod.Name}"));
        Assert.Same(first[0].Namespace, second[0].Namespace);
        Assert.Same(ui, second[0].Pod);

        var redis = ClusterFixture.Pod("redis");
        spaces[0] = ClusterFixture.Space("jobs", redis);

        Assert.Equal(["jobs/redis-0"], status.Pods.Select(pair => $"{pair.Namespace.Name}/{pair.Pod.Name}"));
        Assert.Same(redis, Assert.Single(status.Pods).Pod);
        Assert.Same(ui, first[0].Pod);
    }

    // ----------------------------------------------------------------------------------------------- Parse

    /// <summary>The empty file is the text that is all of it whitespace, and a byte-order mark is not whitespace: a
    /// file that begins with one, however empty the rest of it is, is refused as JSON that does not read rather than
    /// as a file with nothing in it - the collector that writes a BOM is answered with a problem, not with an empty
    /// status.</summary>
    [Theory]
    [InlineData("\uFEFF", "the file is empty")]
    [InlineData("\uFEFF ", "the file is empty")]
    [InlineData("\uFEFF{ \"nodes\": [] }", null)]
    [InlineData("\uFEFF{ \"nodes\": [], \"namespaces\": [] }", null)]
    public void AByteOrderMarkIsNeitherTheEmptyFileNorAFile(string json, string? withoutTheMark)
    {
        var parsed = ClusterStatus.Parse(json);

        Assert.Null(parsed.Value);
        Assert.Equal("the file is not valid JSON", parsed.Problem);

        // The very same text without the mark is the empty file or a file, depending on nothing but the mark.
        var bare = ClusterStatus.Parse(json.TrimStart('\uFEFF'));

        Assert.Equal(withoutTheMark, bare.Problem);
        Assert.Equal(withoutTheMark is null, bare.Value is not null);
    }

    /// <summary>A moment in a file is an instant, whatever words it is spelled in: the same instant written two hours
    /// east keeps that offset and goes stale at the same age as one written in UTC, and one written with no zone at
    /// all is read as UTC rather than as the reader's own hour - the rule counts instants and a file that arrives
    /// from a collector in another zone is not an hour nearer to stale or further from it.</summary>
    [Theory]
    [InlineData("""{ "generated": "2026-10-06T20:15:30Z", "intervalSeconds": 900, "nodes": [] }""", 0)]
    [InlineData("""{ "generated": "2026-10-06T22:15:30+02:00", "intervalSeconds": 900, "nodes": [] }""", 2)]
    [InlineData("""{ "generated": "2026-10-06T15:15:30-05:00", "intervalSeconds": 900, "nodes": [] }""", -5)]
    [InlineData("""{ "generated": "2026-10-06T20:15:30", "intervalSeconds": 900, "nodes": [] }""", 0)]
    public void AMomentInAFileIsTheInstantItMeansAndKeepsTheOffsetItWasWrittenWith(string json, int offsetHours)
    {
        var status = ClusterFixture.Read(json);

        Assert.Equal(Generated, status.Generated);
        Assert.Equal(Generated.UtcDateTime, status.Generated!.Value.UtcDateTime);
        Assert.Equal(TimeSpan.FromHours(offsetHours), status.Generated.Value.Offset);
        Assert.Equal(TimeSpan.FromMinutes(60), status.StaleAfter);

        Assert.False(status.IsStale(Generated + status.StaleAfter));
        Assert.True(status.IsStale(Generated + status.StaleAfter + TimeSpan.FromSeconds(1)));
        Assert.Equal(TimeSpan.FromHours(offsetHours), status.AsOf(Generated.AddYears(1)).Offset);
    }

    /// <summary>A <c>generated</c> that says no moment is a file that says nothing about when it was written, and
    /// still a file: the reader keeps the rest of it, answers with no problem, holds the minute as the age the file is
    /// held to, is never stale at any clock, and is as of whatever clock asks - the silence costs the staleness and
    /// nothing else. Words, a date out of the calendar, a day out of the month and the empty text are all of them
    /// that silence.</summary>
    [Theory]
    [InlineData("""{ "generated": "written after lunch", "nodes": [] }""")]
    [InlineData("""{ "generated": "2026-13-06T20:15:30Z", "nodes": [] }""")]
    [InlineData("""{ "generated": "2026-02-30T20:15:30Z", "nodes": [] }""")]
    [InlineData("""{ "generated": "2026-10-06T20:15:30+25:00", "nodes": [] }""")]
    [InlineData("""{ "generated": "", "nodes": [] }""")]
    public void AMomentThatSaysNoMomentLeavesAFileThatSaysNoMoment(string json)
    {
        var status = ClusterFixture.Read(json);

        Assert.Null(status.Generated);
        Assert.Null(status.IntervalSeconds);
        Assert.Equal(ClusterStatus.MinimumStaleAfter, status.StaleAfter);
        Assert.Empty(status.Nodes);
        Assert.False(status.IsStale(Generated));
        Assert.False(status.IsStale(Generated.AddYears(100)));
        Assert.Equal(Generated, status.AsOf(Generated));
        Assert.Equal(Generated.AddYears(100), status.AsOf(Generated.AddYears(100)));
    }

    /// <summary>The interval a file gives is a whole number of seconds or no interval at all: parts of a second are
    /// rounded to the nearest one, and a number below zero or past the top of the count is no number - which leaves
    /// the file held to the minute rather than to an age the reader invented. The age follows the interval that was
    /// kept, so the rounding is the age the file is held to.</summary>
    [Theory]
    [InlineData("-15", null, 60)]
    [InlineData("-0.4", null, 60)]
    [InlineData("4000000000", null, 60)]
    [InlineData("1e+10", null, 60)]
    [InlineData("2147483648", null, 60)]
    [InlineData("2147483647", 2147483647, 8589934588)]
    [InlineData("14.4", 14, 60)]
    [InlineData("15.6", 16, 64)]
    [InlineData("899.6", 900, 3600)]
    public void TheIntervalAFileGivesIsWholeSecondsOrNone(string seconds, int? expected, long staleAfterSeconds)
    {
        var status = ClusterFixture.Read($$"""{ "intervalSeconds": {{seconds}}, "nodes": [] }""");

        Assert.Equal(expected, status.IntervalSeconds);
        Assert.Equal(TimeSpan.FromSeconds(staleAfterSeconds), status.StaleAfter);
    }

    // --------------------------------------------------------------------------------------- the line it says

    /// <summary>The one line the record says about itself carries the age the rule adds up to at the ends of the
    /// interval, not the interval itself: the bottom of the number is said as the minute, an hour of an interval as
    /// the hour, and the top of it as the hundred thousand days it is - so a reader comparing two of the lines sees
    /// the age the two files are held to.</summary>
    [Fact]
    public void TheLineTheRecordSaysCarriesTheAgeAtTheEndsOfTheInterval()
    {
        Assert.Contains("StaleAfter = 00:01:00", (ClusterFixture.Status() with { IntervalSeconds = int.MinValue }).ToString(), StringComparison.Ordinal);
        Assert.Contains("StaleAfter = 01:00:00", (ClusterFixture.Status() with { IntervalSeconds = 900 }).ToString(), StringComparison.Ordinal);
        Assert.Contains("StaleAfter = 99420.12:56:28", (ClusterFixture.Status() with { IntervalSeconds = int.MaxValue }).ToString(), StringComparison.Ordinal);
    }
}
