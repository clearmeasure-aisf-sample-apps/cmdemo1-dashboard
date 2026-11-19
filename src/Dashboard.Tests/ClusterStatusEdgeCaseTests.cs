namespace Dashboard.Tests;

/// <summary>
/// <see cref="ClusterStatus" /> is the whole of what the collector inside the cluster writes to <c>cluster.json</c>:
/// the moment it wrote it, how often it writes, the version of the API server, the nodes and the namespaces. What the
/// type owes its readers is pinned where it belongs - <see cref="ClusterParserTests" /> reads the shipped sample part
/// for part, says that every part but the names is optional, that a value of another type is no value and never an
/// exception, and answers a malformed file with the problem in words; <see cref="ClusterAssessmentTests" /> steps
/// <see cref="ClusterStatus.IsStale" /> across 60 and 61 and 120 and 121 seconds and says that a file which does not
/// say when is never stale; and <see cref="ClusterLivenessEdgeCaseTests" /> pins the tick the page turns on at that
/// same minute.
/// <para>
/// What is pinned here is the edge each of those readings sits next to, and only that: the two numbers the stale rule
/// is made of to their value (<see cref="ClusterStatus.StaleIntervals" />,
/// <see cref="ClusterStatus.MinimumStaleAfter" />), and <see cref="ClusterStatus.StaleAfter" /> at an interval absent,
/// zero, negative, one, a hair under the floor and a hair over it, at the hour and the day, and at the biggest
/// interval the number holds; the tick past <see cref="ClusterStatus.StaleAfter" /> a file that writes once an hour
/// goes stale at, and that file spelled an ocean away, since the rule counts instants and not the words a clock
/// writes; <see cref="ClusterStatus.AsOf" /> at a clock that stands behind the cluster's and at the two ends of the
/// number, and the offset it keeps; the order, the pairs and the nothing of <see cref="ClusterStatus.Pods" />, and the
/// sixty-three pods a node counts about itself which are no pod of the status; <see cref="ClusterStatus.Parse" /> at
/// the array that is empty, which is an array, against the name that holds no array at all; and the record underneath
/// - its five parts in the order they are declared, the equality its two lists take part in, and the one line it says
/// about itself.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: no member of this type does either, every moment
/// is one the test brings, every file is text handed over, and every status is one the test builds or reads.
/// </para>
/// </summary>
public class ClusterStatusEdgeCaseTests
{
    /// <summary>The moment the sample file was written; every age below is counted from it.</summary>
    private static readonly DateTimeOffset Generated = ClusterFixture.Generated;

    /// <summary>A file that says the three facts and an empty list of nodes, in the words the two readings are made
    /// from - the same text twice over is two readings of it.</summary>
    private const string OneFile = """{ "generated": "2026-10-06T20:15:30Z", "intervalSeconds": 15, "kubernetesVersion": "v1.33.3", "nodes": [] }""";

    /// <summary>The least a file may say and still be a file: one empty list, and nothing else.</summary>
    private const string NoNodes = """{ "nodes": [] }""";

    // ------------------------------------------------------------------------ the stale rule, to its numbers

    /// <summary>The two numbers the stale rule is made of, to their value: four of the collector's intervals, and a
    /// minute at least. Until now they were pinned only through the ages another test steps across, a long way round
    /// for two constants. The quarter of the floor they agree at is the sample's own interval.</summary>
    [Fact]
    public void TheStaleRuleIsFourIntervalsAndAMinuteAtLeast()
    {
        Assert.Equal(4, ClusterStatus.StaleIntervals);
        Assert.Equal(TimeSpan.FromSeconds(60), ClusterStatus.MinimumStaleAfter);
        Assert.Equal(TimeSpan.FromMinutes(1), ClusterStatus.MinimumStaleAfter);

        var agrees = ClusterFixture.Status() with { IntervalSeconds = (int)ClusterStatus.MinimumStaleAfter.TotalSeconds / ClusterStatus.StaleIntervals };

        Assert.Equal(15, agrees.IntervalSeconds);
        Assert.Equal(ClusterStatus.MinimumStaleAfter, agrees.StaleAfter);
    }

    /// <summary>The age a file is held to, at the ends of the rule: an interval absent, no interval at all, a negative
    /// one the floor answers for, one second, the four intervals that stop a minute short of the floor and the one
    /// that is the floor itself, the next second past it, and the hour and the day the collector may write on.</summary>
    [Theory]
    [InlineData(null, 60)]
    [InlineData(0, 60)]
    [InlineData(-15, 60)]
    [InlineData(1, 60)]
    [InlineData(14, 60)]
    [InlineData(15, 60)]
    [InlineData(16, 64)]
    [InlineData(30, 120)]
    [InlineData(150, 600)]
    [InlineData(900, 3600)]
    [InlineData(86400, 345600)]
    public void StaleAfterIsFourIntervalsOrAMinuteWhicheverIsLonger(int? interval, int expectedSeconds)
    {
        var status = ClusterFixture.Status() with { IntervalSeconds = interval };

        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), status.StaleAfter);
        Assert.True(status.StaleAfter >= ClusterStatus.MinimumStaleAfter, $"an interval of {interval} s says {status.StaleAfter}");
    }

    /// <summary>The age a file is held to weighs nothing but its interval: not the moment the file says, not the
    /// version it names, not the nodes and the namespaces it lists - and a status built by hand with nothing in it at
    /// all is held to the same minute as the sample, so an empty file is never a file with no rule.</summary>
    [Fact]
    public void StaleAfterWeighsNoMomentAndNothingElseInTheFile()
    {
        var hourly = ClusterFixture.Status() with { IntervalSeconds = 900 };

        Assert.Equal(TimeSpan.FromMinutes(60), hourly.StaleAfter);
        Assert.Equal(hourly.StaleAfter, (hourly with { Generated = null, KubernetesVersion = null, Nodes = [], Namespaces = [] }).StaleAfter);
        Assert.Equal(ClusterStatus.MinimumStaleAfter, new ClusterStatus(Generated, null, null, [], []).StaleAfter);
        Assert.Equal(ClusterStatus.MinimumStaleAfter, new ClusterStatus(null, 0, null, [ClusterFixture.Node()], []).StaleAfter);

        // No moment, no interval, no node and no namespace: still a status with an age of its own.
        Assert.Equal(ClusterStatus.MinimumStaleAfter, new ClusterStatus(null, null, null, [], []).StaleAfter);
    }

    /// <summary>A collector that writes once an hour is given the hour and not the minute: a minute past its moment is
    /// nothing, the hour itself is not yet stale - the rule is <em>older than</em> - and one tick past the hour, and
    /// one tick of the hour's own clock, the file is stale.</summary>
    [Fact]
    public void AFileThatWritesOnceAnHourGoesStaleOneTickPastTheHour()
    {
        var status = ClusterFixture.Status() with { IntervalSeconds = 900 };
        var hour = TimeSpan.FromMinutes(60);

        Assert.False(status.IsStale(Generated.AddSeconds(60)));
        Assert.False(status.IsStale(Generated + hour - TimeSpan.FromSeconds(1)));
        Assert.False(status.IsStale(Generated + hour));
        Assert.True(status.IsStale(Generated + hour + TimeSpan.FromSeconds(1)));
        Assert.True(status.IsStale(Generated + hour + TimeSpan.FromTicks(1)));
    }

    /// <summary>The interval at the top of the number: four of them are near a hundred thousand days, which is a
    /// TimeSpan and no overflow, and the file stays fresh over all of it and goes stale one second past - the age the
    /// rule says is the age the rule keeps.</summary>
    [Fact]
    public void TheBiggestIntervalIsFourOfItselfAndNoOverflow()
    {
        var status = ClusterFixture.Status() with { IntervalSeconds = int.MaxValue };
        var after = TimeSpan.FromSeconds(4d * int.MaxValue);

        Assert.Equal(after, status.StaleAfter);
        Assert.False(status.IsStale(Generated + after));
        Assert.True(status.IsStale(Generated + after + TimeSpan.FromSeconds(1)));
    }

    /// <summary>The rule counts instants, not the words a clock writes them in: a file whose moment is spelled two
    /// hours east, or five hours west, or fourteen, goes stale at the same age as one spelled in UTC, whichever way
    /// the asking clock is written - and the moment it is asked about comes back as the file wrote it, offset and
    /// all.</summary>
    [Theory]
    [InlineData(0, 59, false)]
    [InlineData(2, 59, false)]
    [InlineData(2, 60, false)]
    [InlineData(2, 61, true)]
    [InlineData(-5, 61, true)]
    [InlineData(-5, 119, true)]
    [InlineData(14, 61, true)]
    [InlineData(-12, 60, false)]
    public void TheInstantIsWhatGoesStaleNotTheWordsOfTheClock(int hours, int ageSeconds, bool stale)
    {
        var offset = TimeSpan.FromHours(hours);
        var status = ClusterFixture.Status() with { Generated = Generated.ToOffset(offset) };
        var now = new DateTimeOffset(Generated.UtcDateTime.AddSeconds(ageSeconds), TimeSpan.Zero);

        Assert.Equal(Generated.UtcDateTime, status.Generated!.Value.UtcDateTime);
        Assert.Equal(offset, status.Generated.Value.Offset);
        Assert.Equal(stale, status.IsStale(now));
        Assert.Equal(stale, status.IsStale(now.ToOffset(offset)));

        var asOf = status.AsOf(now);

        Assert.Equal(Generated.UtcDateTime, asOf.UtcDateTime);
        Assert.Equal(offset, asOf.Offset);
    }

    // --------------------------------------------------------------------------------------------- AsOf

    /// <summary>The moment the facts are of is the collector's own, whatever the clock asks about it: ten minutes
    /// after it and ten minutes before it, the two ends of the number and the epoch, a clock spelled three hours
    /// further east - the answer is the file's moment every time, in the offset the file was written with and not the
    /// clock's.</summary>
    [Fact]
    public void TheFactsAreOfTheCollectorsMomentWhateverTheClockAsks()
    {
        var status = ClusterFixture.Status();

        Assert.Equal(Generated, status.AsOf(Generated.AddMinutes(10)));
        Assert.Equal(Generated, status.AsOf(Generated.AddMinutes(-10)));
        Assert.Equal(Generated, status.AsOf(DateTimeOffset.UnixEpoch));
        Assert.Equal(Generated, status.AsOf(DateTimeOffset.MinValue));
        Assert.Equal(Generated, status.AsOf(DateTimeOffset.MaxValue));

        var east = status.AsOf(new DateTimeOffset(2026, 10, 6, 23, 15, 30, TimeSpan.FromHours(3)));

        Assert.Equal(Generated.UtcDateTime, east.UtcDateTime);
        Assert.Equal(TimeSpan.Zero, east.Offset);
    }

    /// <summary>A file that does not say when it was written is as of the clock it is asked at, that very clock and no
    /// other: its own sample moment, half an hour either side of it, the epoch, the bottom of the number and a moment
    /// spelled two hours east all answer themselves, offset kept - and none of them is stale, which is what the same
    /// silence costs.</summary>
    [Fact]
    public void AFileThatDoesNotSayWhenIsAsOfWhateverClockAsks()
    {
        var silent = ClusterFixture.Status() with { Generated = null };
        DateTimeOffset[] clocks =
        [
            Generated,
            Generated.AddMinutes(-30),
            Generated.AddMinutes(30),
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.MinValue,
            new(2026, 10, 6, 22, 15, 30, TimeSpan.FromHours(2)),
        ];

        foreach (var now in clocks)
        {
            Assert.Equal(now, silent.AsOf(now));
            Assert.Equal(now.Offset, silent.AsOf(now).Offset);
            Assert.False(silent.IsStale(now));
        }
    }

    // --------------------------------------------------------------------------------------------- Pods

    /// <summary>Every pod with its namespace, in the order of the file: the namespaces first and the pods of each in
    /// theirs, a namespace that lists no pod contributing no pair at all, and each pair holding the very namespace and
    /// the very pod the lists hold rather than a copy of either.</summary>
    [Fact]
    public void ThePodsAreTheNamespacesPodsInTheOrderTheyAreListed()
    {
        var ui = ClusterFixture.Pod("ui");
        var db = ClusterFixture.Pod("db");
        var apps = ClusterFixture.Space("apps", ui, db);
        var jobs = ClusterFixture.Space("jobs");
        var redis = ClusterFixture.Pod("redis");
        var platform = ClusterFixture.Space("platform", redis);
        var status = ClusterFixture.Status([ClusterFixture.Node()], apps, jobs, platform);

        Assert.Equal(
            [("apps", "ui-0"), ("apps", "db-0"), ("platform", "redis-0")],
            status.Pods.Select(pair => (pair.Namespace.Name, pair.Pod.Name)));

        var pairs = status.Pods.ToArray();

        Assert.Same(apps, pairs[0].Namespace);
        Assert.Same(ui, pairs[0].Pod);
        Assert.Same(apps, pairs[1].Namespace);
        Assert.Same(db, pairs[1].Pod);
        Assert.Same(platform, pairs[^1].Namespace);
        Assert.Same(redis, pairs[^1].Pod);
        Assert.DoesNotContain(pairs, pair => ReferenceEquals(pair.Namespace, jobs));
    }

    /// <summary>The pods of a status come from its namespaces alone: no namespaces is no pod, a namespace with
    /// nothing in it is no pod, and the sixty-three pods a node counts about itself are no pod of the status at
    /// all - the file that reads with an empty array is empty in the same way.</summary>
    [Fact]
    public void ThePodsOfANodeAreNoPodsOfTheStatus()
    {
        var noSpaces = ClusterFixture.Status();
        var emptySpaces = ClusterFixture.Status([], ClusterFixture.Space("apps"), ClusterFixture.Space("jobs"));
        var parsed = Read("""{ "nodes": [ { "name": "node-1", "pods": { "count": 63, "capacity": 110 } } ], "namespaces": [ { "name": "apps", "pods": [] } ] }""");

        Assert.Equal(63, Assert.Single(noSpaces.Nodes).Pods);
        Assert.Empty(noSpaces.Pods);

        Assert.Equal(2, emptySpaces.Namespaces.Count);
        Assert.Empty(emptySpaces.Pods);

        var node = Assert.Single(parsed.Nodes);
        Assert.Equal((63, 110), (node.Pods, node.PodCapacity));
        Assert.Empty(Assert.Single(parsed.Namespaces).Pods);
        Assert.Empty(parsed.Pods);
    }

    // -------------------------------------------------------------------------------------------- Parse

    /// <summary>An empty array is an array: a file whose <c>nodes</c> is <c>[]</c>, or whose <c>namespaces</c> is, or
    /// whose both are, is read rather than refused - it names a list, and the empty one is a list. What the same file
    /// says of itself besides comes through with it, the blank version included, and an interval of zero seconds is a
    /// number the status keeps and not the nothing it is next to.</summary>
    [Fact]
    public void AnEmptyArrayIsStillAnArrayOfNodesOrOfNamespaces()
    {
        var nodes = Read(NoNodes);
        var spaces = Read("""{ "namespaces": [] }""");
        var both = Read("""{ "nodes": [], "namespaces": [] }""");
        var facts = Read(OneFile);

        Assert.Empty(nodes.Nodes);
        Assert.Empty(nodes.Namespaces);
        Assert.Empty(spaces.Nodes);
        Assert.Empty(spaces.Namespaces);
        Assert.Empty(both.Pods);
        Assert.Empty(both.Nodes);
        Assert.Empty(both.Namespaces);

        Assert.Null(both.Generated);
        Assert.Null(both.IntervalSeconds);
        Assert.Null(both.KubernetesVersion);

        Assert.Equal(Generated, facts.Generated);
        Assert.Equal(15, facts.IntervalSeconds);
        Assert.Equal("v1.33.3", facts.KubernetesVersion);
        Assert.Empty(facts.Nodes);

        var zero = Read("""{ "intervalSeconds": 0, "kubernetesVersion": "  ", "namespaces": [] }""");

        Assert.Equal(0, zero.IntervalSeconds);
        Assert.Null(zero.KubernetesVersion);
        Assert.Equal(ClusterStatus.MinimumStaleAfter, zero.StaleAfter);

        // One list of the two empty, and the other name holding no array at all, is still a file that is read.
        var half = Read("""{ "nodes": [], "namespaces": null }""");

        Assert.Empty(half.Nodes);
        Assert.Empty(half.Namespaces);
    }

    /// <summary>A name where the array should be is no array of either kind, whatever stands there: the object and the
    /// string the parser already pins, and the JSON <c>null</c>, the two bools, the two numbers and the quoted array
    /// besides. A file that says plenty and lists neither list is refused for that, not read as an empty one.</summary>
    [Theory]
    [InlineData("""{ "nodes": null, "namespaces": null }""")]
    [InlineData("""{ "nodes": true, "namespaces": false }""")]
    [InlineData("""{ "nodes": 3, "namespaces": 4.5 }""")]
    [InlineData("""{ "nodes": "[]", "namespaces": 0 }""")]
    [InlineData("""{ "generated": "2026-10-06T20:15:30Z", "intervalSeconds": 15 }""")]
    public void ANameThatHoldsNoArrayIsNoNodesAndNoNamespaces(string json)
    {
        var parsed = ClusterStatus.Parse(json);

        Assert.Null(parsed.Value);
        Assert.Equal("the file has neither nodes nor namespaces", parsed.Problem);
    }

    /// <summary>The reading of a file is one of two things and never both: the problem the reader says comes with no
    /// status at all, and the status it hands over comes with nothing said against it - the
    /// <see cref="Parsed{T}.Problem" /> a reading leaves out is <see langword="null" />, not the empty string, which is
    /// what a test that reads a good file would otherwise mistake for a small complaint.</summary>
    [Fact]
    public void AFileThatReadsSaysNoProblemAndAProblemCarriesNoFile()
    {
        var status = ClusterFixture.Status();
        var read = new Parsed<ClusterStatus>(status);
        var refused = new Parsed<ClusterStatus>(null, "the file is empty");

        Assert.Same(status, read.Value);
        Assert.Null(read.Problem);
        Assert.Null(refused.Value);
        Assert.Equal("the file is empty", refused.Problem);
        Assert.NotEqual(read, refused);

        var (held, problem) = read;

        Assert.Same(status, held);
        Assert.Null(problem);
        Assert.Equal(read, new Parsed<ClusterStatus>(status));
        Assert.NotEqual(read, new Parsed<ClusterStatus>(status, "the file is old"));

        var parsed = ClusterStatus.Parse(OneFile);

        Assert.Null(parsed.Problem);
        Assert.NotNull(parsed.Value);
        Assert.Equal(Generated, parsed.Value!.Generated);
        Assert.Equal(15, parsed.Value.IntervalSeconds);
        Assert.Equal("v1.33.3", parsed.Value.KubernetesVersion);
        Assert.Empty(parsed.Value.Nodes);
        Assert.Empty(parsed.Value.Namespaces);
    }

    // ---------------------------------------------------------------------------------------- the record

    /// <summary>The constructor's five parts, in the order the record declares them, each come back as the property of
    /// the same name and as the very lists they were handed - a status copies nothing of what it is given, which is
    /// what <see cref="Parse" /> relies on when it hands the file over position by position.</summary>
    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        ClusterNode[] nodes = [ClusterFixture.Node("aks-a-000001")];
        ClusterNamespace[] spaces = [ClusterFixture.Space("apps", ClusterFixture.Pod("ui"))];

        var status = new ClusterStatus(Generated, 900, "v1.34.1", nodes, spaces);

        Assert.Equal(Generated, status.Generated);
        Assert.Equal(900, status.IntervalSeconds);
        Assert.Equal("v1.34.1", status.KubernetesVersion);
        Assert.Same(nodes, status.Nodes);
        Assert.Same(spaces, status.Namespaces);

        var (generated, interval, version, listed, namespaces) = status;

        Assert.Equal((Generated, 900, "v1.34.1"), (generated, interval, version));
        Assert.Same(nodes, listed);
        Assert.Same(spaces, namespaces);
    }

    /// <summary>
    /// The status is its five parts, the two lists compared as the objects they are: two statuses round one and the
    /// same pair of lists are one status and hash alike, a <c>with</c> that names no part or names one and changes it
    /// to what it already was changes nothing, and each of the five parts on its own is enough to tell two apart. What
    /// does <em>not</em> come out as one status is the same file read twice, or a list copied element for element - the
    /// nodes of a status are the list the reader was handed, and two readings hold two of them, empty lists and all.
    /// </summary>
    [Fact]
    public void TwoStatusesAreOneStatusWhenEveryPartAndTheirListsAre()
    {
        ClusterNode[] nodes = [ClusterFixture.Node("aks-a-000001")];
        ClusterNamespace[] spaces = [ClusterFixture.Space("apps", ClusterFixture.Pod("ui"))];

        var one = Built(nodes, spaces);

        Assert.Equal(one, Built(nodes, spaces));
        Assert.True(one.Equals(Built(nodes, spaces)));
        Assert.True(one == Built(nodes, spaces));
        Assert.False(one != Built(nodes, spaces));
        Assert.Equal(one.GetHashCode(), Built(nodes, spaces).GetHashCode());
        Assert.Equal(one, one with { });
        Assert.NotSame(one, one with { });
        Assert.Equal(one, one with { IntervalSeconds = one.IntervalSeconds, KubernetesVersion = one.KubernetesVersion });
        Assert.False(one.Equals(null));
        Assert.False(one.Equals("not a status"));

        ClusterStatus[] different =
        [
            one with { Generated = null },
            one with { Generated = Generated.AddSeconds(1) },
            one with { IntervalSeconds = null },
            one with { IntervalSeconds = 16 },
            one with { KubernetesVersion = null },
            one with { KubernetesVersion = "v1.34.1" },
            one with { Nodes = [] },
            one with { Namespaces = [] },
            one with { Nodes = [ClusterFixture.Node("aks-a-000002")] },
            one with { Namespaces = [ClusterFixture.Space("jobs")] },
        ];

        foreach (var other in different)
        {
            Assert.NotEqual(one, other);
            Assert.False(one.Equals(other));
            Assert.True(one != other);
        }

        // The lists are the lists they are: a copy of one, element for element, is another list and another status.
        Assert.NotEqual(one, Built([.. nodes], spaces));
        Assert.NotEqual(one, Built(nodes, [.. spaces]));

        // Two readings of one file are two statuses, and so are two readings of the file that lists nothing: the
        // lists a reader hands over are its own, empty as they are, and an empty list is still a list of its own.
        Assert.NotEqual(Read(OneFile), Read(OneFile));
        Assert.NotEqual(Read(NoNodes), Read(NoNodes));
    }

    /// <summary>The one line the record says about itself - the line a failing test prints - names the type, then its
    /// five parts in the order they are declared, and says the two rules the parts add up to with them: a reader
    /// comparing two of the lines can see which part moved and what age the file is held to. A part that is nothing is
    /// said as nothing, not as a word.</summary>
    [Fact]
    public void TheRecordSaysItsPartsAndItsRuleInOneLineOfItsOwn()
    {
        var text = Built([ClusterFixture.Node()], [ClusterFixture.Space("apps", ClusterFixture.Pod("ui"))]).ToString();

        Assert.StartsWith("ClusterStatus { ", text, StringComparison.Ordinal);
        Assert.Contains("Generated = ", text, StringComparison.Ordinal);
        Assert.Contains("IntervalSeconds = 15", text, StringComparison.Ordinal);
        Assert.Contains("KubernetesVersion = v1.33.3", text, StringComparison.Ordinal);
        Assert.Contains("Nodes = ", text, StringComparison.Ordinal);
        Assert.Contains("Namespaces = ", text, StringComparison.Ordinal);
        Assert.Contains("StaleAfter = 00:01:00", text, StringComparison.Ordinal);

        // The order the parts are declared in is the order they are said in, and the rule they add up to comes after.
        Assert.True(
            text.IndexOf("Generated =", StringComparison.Ordinal) < text.IndexOf("Namespaces =", StringComparison.Ordinal),
            text);
        Assert.True(
            text.IndexOf("KubernetesVersion =", StringComparison.Ordinal) < text.IndexOf("StaleAfter =", StringComparison.Ordinal),
            text);

        var nothing = new ClusterStatus(null, null, null, [], []).ToString();

        Assert.Contains("Generated = ,", nothing, StringComparison.Ordinal);
        Assert.Contains("IntervalSeconds = ,", nothing, StringComparison.Ordinal);
        Assert.Contains("KubernetesVersion = ,", nothing, StringComparison.Ordinal);
        Assert.Contains("StaleAfter = 00:01:00", nothing, StringComparison.Ordinal);
    }

    // ----------------------------------------------------------------------------------------- helpers

    /// <summary>A whole status round the two lists it is handed, so that two of them can be given one and the same
    /// pair of lists.</summary>
    private static ClusterStatus Built(IReadOnlyList<ClusterNode> nodes, IReadOnlyList<ClusterNamespace> spaces) =>
        new(Generated, 15, "v1.33.3", nodes, spaces);

    /// <summary>The status a file holds. A file the reader answers with a problem fails here with that problem to
    /// read, so the tests above say what the status is rather than that there is one.</summary>
    private static ClusterStatus Read(string json)
    {
        var parsed = ClusterStatus.Parse(json);

        Assert.NotNull(parsed.Value);
        Assert.Null(parsed.Problem);

        return parsed.Value!;
    }
}
