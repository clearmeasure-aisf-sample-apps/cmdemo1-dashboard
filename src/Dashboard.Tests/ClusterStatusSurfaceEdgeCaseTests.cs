namespace Dashboard.Tests;

/// <summary>
/// The edges of <see cref="ClusterStatus" /> that its other readings leave open. The name this issue asked for
/// stands - <see cref="ClusterStatusEdgeCaseTests" /> holds it, as <see cref="ClusterStatusSpellingEdgeCaseTests" />
/// explains of its own type - so what none of them reaches is set down here, in a file of its own.
/// <para>
/// The rest of the type is pinned elsewhere and not repeated: <see cref="ClusterStatusEdgeCaseTests" /> pins
/// <see cref="ClusterStatus.StaleIntervals" /> and <see cref="ClusterStatus.MinimumStaleAfter" /> to their values,
/// <see cref="ClusterStatus.StaleAfter" /> from an interval absent to <see cref="int.MaxValue" />,
/// <see cref="ClusterStatus.IsStale" /> one tick on either side of an hour, <see cref="ClusterStatus.AsOf" /> under a
/// dozen clocks, the order and the pairs of <see cref="ClusterStatus.Pods" />, the empty array and the name that holds
/// none, and the record's equality and its line; <see cref="ClusterStatusBoundaryEdgeCaseTests" /> pins the interval at
/// the bottom of the number, the two ends of the number of moments, a pod listed twice and the list a status holds
/// rather than a copy of it; <see cref="ClusterStatusParseEdgeCaseTests" /> and <see cref="ClusterParserTests" /> pin
/// the blank text, the empty object, the names spelled in another case, what strict JSON refuses, the depth of a file,
/// the shipped sample and a value of another type; <see cref="ClusterStatusSpellingEdgeCaseTests" /> pins the name a
/// file gives twice, the array of things that are no node, the moment spelled otherwise and the volumes of a namespace;
/// and <see cref="ParsedEdgeCaseTests" />, <see cref="ClusterAssessmentTests" /> and
/// <see cref="ClusterLivenessEdgeCaseTests" /> pin the reading a file hands over, the age stepped across the minute and
/// the hour, and the JSON that is a value - <c>null</c>, <c>123</c>, <c>true</c> - rather than a file.
/// </para>
/// <para>
/// What is pinned here is only the remainder: the root that is a number, a bool, a text with nothing in it or an array
/// of anything, which is no object whichever way it is spelled; the lists a file may name that the reader reads
/// nowhere, which make no file and no pod; each of the three facts given as something that is not that fact; the
/// interval past the biggest number there is and the zero that is written negative; the two statuses that are one
/// status in a set, in a hash map and asked through <see cref="object" />; the five parts of a file that says nothing,
/// taken apart; the line the record says, which names both computed parts and tells nothing of what the two lists
/// hold; the namespace whose own array of pods <see cref="ClusterStatus.Pods" /> reads through to; and the one edge a
/// status with no lists at all does not cross.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every moment is one the test brings, every file
/// is text handed over and every status is one the test builds.
/// </para>
/// </summary>
public class ClusterStatusSurfaceEdgeCaseTests
{
    /// <summary>The moment the sample file was written; every age below is counted from it.</summary>
    private static readonly DateTimeOffset Generated = ClusterFixture.Generated;

    // ------------------------------------------------------------------- the root that is no object

    /// <summary>The object the reader looks for is the only root it takes, and it says so in the same words whatever
    /// stands instead: the second bool, the three numbers (the zero that is negative among them), the text that holds
    /// no word and the text that holds one, the array of anything - of nothing, of arrays, of the JSON <c>null</c> or
    /// of a whole node the file never named - are refused for what they are and not as an empty file, with or without
    /// blanks about them. The <c>null</c>, the <c>123</c>, the <c>true</c> and the bare <c>[]</c> are pinned by the
    /// readings of the record and of the sample; the kinds none of them reaches are pinned here.</summary>
    [Theory]
    [InlineData("false")]
    [InlineData("  false  ")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("4.5")]
    [InlineData("\"\"")]
    [InlineData("\" \"")]
    [InlineData("\"text\"")]
    [InlineData("[[]]")]
    [InlineData("[null]")]
    [InlineData("""[ { "name": "node-1" } ]""")]
    public void ARootThatIsNoObjectIsRefusedInEverySpelling(string json)
    {
        var parsed = ClusterStatus.Parse(json);

        Assert.Null(parsed.Value);
        Assert.Equal("the file does not contain a JSON object", parsed.Problem);
    }

    // --------------------------------------------------- the lists a file may name and the reader never reads

    /// <summary>The two names the reader looks at are the two a file has to be read by: a file whose only list is the
    /// pods at its root, or its volumes, or both, lists no node and no namespace, and is refused for that. The pods of
    /// a cluster are said by its namespaces, nowhere else, and a collector that writes them at the root writes a file
    /// the reader cannot read rather than a file with nothing in it.</summary>
    [Theory]
    [InlineData("""{ "pods": [ { "name": "ui-1" } ] }""")]
    [InlineData("""{ "volumes": [ { "name": "data" } ] }""")]
    [InlineData("""{ "pods": [ { "name": "ui-1" } ], "volumes": [ { "name": "data" } ] }""")]
    [InlineData("""{ "generated": "2026-10-06T20:15:30Z", "intervalSeconds": 15, "pods": [] }""")]
    public void AFileWhoseOnlyListsAreNamesTheReaderNeverReadsIsNoFile(string json)
    {
        var parsed = ClusterStatus.Parse(json);

        Assert.Null(parsed.Value);
        Assert.Equal("the file has neither nodes nor namespaces", parsed.Problem);
    }

    /// <summary>The pods of a status are its namespaces' pods and nothing else: a file that lists two pods and a volume
    /// at its root, beside the two lists the reader does read, is read, and lists no pod at all - the names it does not
    /// know carry nothing into the status, not a node, not a pod, and not the volume a namespace holds under its own
    /// name. The age the file is held to comes through unchanged, since what the unknown names say is nothing.</summary>
    [Fact]
    public void ThePodsAFileListsAtItsRootAreNoPodsOfTheStatus()
    {
        var status = Read(
            """
            { "generated": "2026-10-06T20:15:30Z", "intervalSeconds": 900, "nodes": [], "kubernetesVersion": "v1.33.3",
              "pods": [ { "name": "ui-1" }, { "name": "db-1" } ], "volumes": [ { "name": "data" } ],
              "namespaces": [ { "name": "apps", "volumes": [ { "name": "logs" } ] } ] }
            """);

        Assert.Equal(Generated, status.Generated);
        Assert.Equal(900, status.IntervalSeconds);
        Assert.Equal("v1.33.3", status.KubernetesVersion);
        Assert.Empty(status.Nodes);
        Assert.Empty(status.Pods);
        Assert.Equal(TimeSpan.FromMinutes(60), status.StaleAfter);

        var space = Assert.Single(status.Namespaces);
        Assert.Equal("apps", space.Name);
        Assert.Empty(space.Pods);
        Assert.Equal("logs", Assert.Single(space.Volumes).Name);
    }

    // ------------------------------------------------------- a fact of another kind is no fact at all

    /// <summary>A file whose three facts are all of them the wrong kind of thing - an object where the moment stands,
    /// an array where the interval stands, a bool where the version stands - is still a file: it lists nodes, and each
    /// fact is simply not there. Nothing is said against it, the empty status it hands over is held to the minute like
    /// any other, is never stale, and is as of whatever clock asks.</summary>
    [Fact]
    public void AFileWhoseEveryFactIsOfAnotherKindIsAFileWithNoFacts()
    {
        var status = Read("""{ "generated": {}, "intervalSeconds": [], "kubernetesVersion": true, "nodes": [] }""");

        Assert.Null(status.Generated);
        Assert.Null(status.IntervalSeconds);
        Assert.Null(status.KubernetesVersion);
        Assert.Empty(status.Nodes);
        Assert.Empty(status.Namespaces);
        Assert.Equal(ClusterStatus.MinimumStaleAfter, status.StaleAfter);
        Assert.False(status.IsStale(Generated.AddYears(50)));
        Assert.Equal(Generated, status.AsOf(Generated));
    }

    /// <summary>One fact given as something else is that fact missing and the rest of the file untouched: the moment
    /// spelled as an array, an object or a bool says no moment; the interval spelled as an object, an array, a bool or
    /// a word says no interval, and the file is held to the minute rather than to an age invented out of the number the
    /// word was next to; and the version spelled as an array, an object or a number says no version. The
    /// <c>generated</c> given as a number, the <c>intervalSeconds</c> given as a word and the version given as
    /// <c>null</c> are pinned by the reading of the sample; the kinds left out there are pinned here.</summary>
    [Theory]
    [InlineData("""{ "generated": [], "nodes": [] }""")]
    [InlineData("""{ "generated": { "day": 6 }, "nodes": [] }""")]
    [InlineData("""{ "generated": false, "nodes": [] }""")]
    [InlineData("""{ "intervalSeconds": {}, "nodes": [] }""")]
    [InlineData("""{ "intervalSeconds": [ 900 ], "nodes": [] }""")]
    [InlineData("""{ "intervalSeconds": true, "nodes": [] }""")]
    [InlineData("""{ "kubernetesVersion": [], "nodes": [] }""")]
    [InlineData("""{ "kubernetesVersion": { "v": 33 }, "nodes": [] }""")]
    [InlineData("""{ "kubernetesVersion": 33, "nodes": [] }""")]
    public void APartOfAFileOfAnotherKindLeavesThatPartNothingAndTheRestItsOwn(string json)
    {
        var status = Read(json);

        Assert.Null(status.Generated);
        Assert.Null(status.IntervalSeconds);
        Assert.Null(status.KubernetesVersion);
        Assert.Empty(status.Nodes);
        Assert.Empty(status.Pods);
        Assert.Equal(ClusterStatus.MinimumStaleAfter, status.StaleAfter);
    }

    // ------------------------------------------------------------- the interval at the ends of the number

    /// <summary>The interval a file gives is a number the reader can hold or no interval at all: past the biggest
    /// double there is - ten to the four hundredth power, and the top of that number grown by another power of ten - it
    /// is nothing, which leaves the file at the minute rather than at an age no <see cref="TimeSpan" /> could hold and
    /// no collector writes on. The zero written negative is the zero the reader keeps, and it stands at the minute like
    /// every other interval that adds up to less than it.</summary>
    [Theory]
    [InlineData("1e400", null, 60)]
    [InlineData("1.7976931348623157e309", null, 60)]
    [InlineData("1.7976931348623157e308", null, 60)]
    [InlineData("-0.0", 0, 60)]
    public void AnIntervalTheNumberOfDoublesCannotHoldIsNoInterval(string written, int? expected, int staleAfterSeconds)
    {
        var status = Read($$"""{ "generated": "2026-10-06T20:15:30Z", "intervalSeconds": {{written}}, "nodes": [] }""");

        Assert.Equal(Generated, status.Generated);
        Assert.Equal(expected, status.IntervalSeconds);
        Assert.Equal(TimeSpan.FromSeconds(staleAfterSeconds), status.StaleAfter);
        Assert.Equal(ClusterStatus.MinimumStaleAfter, status.StaleAfter);
        Assert.False(status.IsStale(Generated + status.StaleAfter));
        Assert.True(status.IsStale(Generated + status.StaleAfter + TimeSpan.FromSeconds(1)));
    }

    // ------------------------------------------------------ the file that names half a character is no file at all

    /// <summary>The reader answers a file that does not parse with the problem in words, and that answer is what
    /// <see cref="ClusterStatus.Parse" /> catches: a <c>\u</c> escape that names one half of a surrogate pair is not
    /// among the complaints the reader holds, so the file comes out as a throw and not as words - pinned as it stands,
    /// because it is the edge of the one catch the reader makes. An escape that names a whole character is a file, and
    /// the character is the node's name.</summary>
    [Fact]
    public void AnEscapeThatNamesHalfACharacterIsNeitherAFileNorAProblemInTheWords()
    {
        Assert.Throws<InvalidOperationException>(() => ClusterStatus.Parse("""{ "nodes": [ { "name": "a\ud800b" } ] }"""));

        var status = Read("""{ "nodes": [ { "name": "aéb" } ], "namespaces": [] }""");

        Assert.Equal("aéb", Assert.Single(status.Nodes).Name);
        Assert.Empty(status.Pods);
    }

    // ----------------------------------------------------------- the two statuses that are one in every collection

    /// <summary>Two statuses that are one status are one key, however the asking is made: through the record's own
    /// <see cref="object.Equals(object?)" />, through the boxed asking a collection's comparer makes, through
    /// <see cref="EqualityComparer{T}" /> and through the <c>==</c> the record synthesizes. One of them lands a hash set
    /// and one row of a hash map, the map answers to the other by value, and a status whose list is another list is
    /// another key even when the two lists say the same names - which is what the same lists given twice are not. The
    /// asking is stable: the same status hashed twice is one hash and the record's null answers with the reference
    /// operators too.</summary>
    [Fact]
    public void TwoStatusesOfOneValueAreOneKeyInASetAndOneRowOfAHashMap()
    {
        IReadOnlyList<ClusterNode> nodes = [ClusterFixture.Node("aks-a-000001")];
        IReadOnlyList<ClusterNamespace> spaces = [ClusterFixture.Space("apps", ClusterFixture.Pod("ui"))];
        var one = Built(nodes, spaces);
        var two = Built(nodes, spaces);

        Assert.True(one.Equals((object)two));
        Assert.True(EqualityComparer<ClusterStatus>.Default.Equals(one, two));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());

        object? nothing = null;
        ClusterStatus? missing = null;

        Assert.False(one.Equals(nothing));
        Assert.False(one.Equals(new ClusterNamespace("apps", [], [])));
        Assert.True(missing == (ClusterStatus?)null);
        Assert.False((ClusterStatus?)one == null);
        Assert.True((ClusterStatus?)two == one);
        Assert.False(one != two);

        var set = new HashSet<ClusterStatus> { one, two };
        var rows = new Dictionary<ClusterStatus, string> { [one] = "the collector's file" };

        Assert.Single(set);
        Assert.Contains(two, set);
        Assert.Equal("the collector's file", rows[two]);

        // Another list of the same one node is another status and another key; the same two lists, a third time, are not.
        Assert.True(set.Add(Built([ClusterFixture.Node("aks-a-000001")], spaces)));
        Assert.Equal(2, set.Count);
        Assert.False(set.Add(Built(nodes, spaces)));
        Assert.Equal(2, set.Count);
    }

    // ------------------------------------------------------------------- the five parts of a file that says nothing

    /// <summary>The five parts a status that says nothing is made of, taken apart by the record's own deconstruction,
    /// are the three nothings and the two empty lists it was handed - the lists it holds rather than copies of them,
    /// the emptiness of each read the same way twice. Nothing about such a status is missing a rule: it is held to the
    /// minute, is never stale at any clock, is as of whatever clock asks, and lists no pod.</summary>
    [Fact]
    public void AStatusThatSaysNothingDeconstructsToThreeNothingsAndTwoEmptyLists()
    {
        ClusterNode[] nodes = [];
        ClusterNamespace[] spaces = [];
        var silent = new ClusterStatus(null, null, null, nodes, spaces);
        var (generated, interval, version, listed, namespaces) = silent;

        Assert.Null(generated);
        Assert.Null(interval);
        Assert.Null(version);
        Assert.Empty(listed);
        Assert.Empty(namespaces);
        Assert.Same(nodes, listed);
        Assert.Same(spaces, namespaces);
        Assert.Same(silent.Nodes, listed);
        Assert.Same(silent.Namespaces, namespaces);

        Assert.Equal(ClusterStatus.MinimumStaleAfter, silent.StaleAfter);
        Assert.False(silent.IsStale(Generated));
        Assert.False(silent.IsStale(Generated.AddYears(500)));
        Assert.Equal(Generated, silent.AsOf(Generated));
        Assert.Empty(silent.Pods);
    }

    // ------------------------------------------------------------------------ the line the record says, and what it hides

    /// <summary>The one line the record says of itself names its two computed parts beside its five given ones -
    /// <see cref="ClusterStatus.StaleAfter" /> and <see cref="ClusterStatus.Pods" /> both come after the lists, the age
    /// the rule adds up to and the pairs themselves only as the thing they are read out of - and says neither of the
    /// two numbers the rule is made of, which belong to the type and not to the file. It is the same line every time it
    /// is asked, and it is coarser than the record: a status and another with a different node and a different
    /// namespace in their places say one line, because the two lists are named and never opened. What a reader
    /// compares, then, is the three facts and the age; two files that differ only in what they list differ in
    /// <see cref="object.Equals(object?)" /> and not in the line.</summary>
    [Fact]
    public void TheLineTheRecordSaysNamesTheRulesAndOpensNeitherList()
    {
        ClusterNode[] these = [ClusterFixture.Node("a-node-the-line-never-says")];
        ClusterNode[] those = [ClusterFixture.Node("another-node-the-line-never-says")];
        ClusterNamespace[] here = [ClusterFixture.Space("a-space-the-line-never-says", ClusterFixture.Pod("a-workload-the-line-never-says"))];
        ClusterNamespace[] there = [ClusterFixture.Space("another-space-the-line-never-says", ClusterFixture.Pod("another-workload-the-line-never-says"))];

        var text = Built(these, here).ToString();

        Assert.StartsWith("ClusterStatus { ", text, StringComparison.Ordinal);
        Assert.EndsWith(" }", text, StringComparison.Ordinal);
        Assert.Contains("StaleAfter = 00:01:00", text, StringComparison.Ordinal);
        Assert.Contains("Pods = ", text, StringComparison.Ordinal);
        Assert.Contains("IntervalSeconds = 15", text, StringComparison.Ordinal);
        Assert.Contains("KubernetesVersion = v1.33.3", text, StringComparison.Ordinal);

        // The two numbers of the rule are the type's, not the file's, and are said nowhere in a status's own line.
        Assert.DoesNotContain("StaleIntervals", text, StringComparison.Ordinal);
        Assert.DoesNotContain("MinimumStaleAfter", text, StringComparison.Ordinal);

        // The lists are named and never opened: no name of a node, a namespace or a pod stands in the line.
        Assert.Contains("Nodes = ", text, StringComparison.Ordinal);
        Assert.Contains("Namespaces = ", text, StringComparison.Ordinal);
        Assert.DoesNotContain("the-line-never-says", text, StringComparison.Ordinal);

        Assert.Equal(text, Built(these, here).ToString());
        Assert.Equal(text, Built(those, there).ToString());
        Assert.NotEqual(Built(these, here), Built(those, there));

        // The age in the line is the instance's own: another interval says another age in the same shape of line.
        Assert.DoesNotContain("StaleAfter = 00:01:00", (Built(these, here) with { IntervalSeconds = 900 }).ToString(), StringComparison.Ordinal);
        Assert.Contains("StaleAfter = 01:00:00", (Built(these, here) with { IntervalSeconds = 900 }).ToString(), StringComparison.Ordinal);
    }

    // -------------------------------------------------------------------- the pods are read through to the last list

    /// <summary><see cref="ClusterStatus.Pods" /> reads through the status's namespaces to the arrays those hold: a pod
    /// swapped into the array a namespace was built with is the pod the status answers with next, in the namespace it
    /// stands in, with no new status and no new namespace - the property is a reading made each time it is made, over
    /// the very lists the record keeps. The same array asked twice answers the same pod, and the volumes of the space
    /// stay out of the answer.</summary>
    [Fact]
    public void ThePodsAreReadThroughTheNamespacesToTheArraysThoseHold()
    {
        ClusterPod[] pods = [ClusterFixture.Pod("ui")];
        var apps = new ClusterNamespace("apps", pods, []);
        var status = ClusterFixture.Status([], apps);

        Assert.Same(pods, apps.Pods);
        Assert.Same(apps, Assert.Single(status.Namespaces));
        Assert.Equal(["apps/ui-0"], status.Pods.Select(pair => $"{pair.Namespace.Name}/{pair.Pod.Name}"));

        var db = ClusterFixture.Pod("db");
        pods[0] = db;

        Assert.Equal(["apps/db-0"], status.Pods.Select(pair => $"{pair.Namespace.Name}/{pair.Pod.Name}"));
        var pair = Assert.Single(status.Pods);
        Assert.Same(db, pair.Pod);
        Assert.Same(apps, pair.Namespace);
        Assert.Empty(pair.Namespace.Volumes);
    }

    // --------------------------------------------------------------------------- the lists that are not there

    /// <summary>A status handed no lists at all keeps every rule that does not look at them: the age it is held to, the
    /// staleness counted from its own moment to that age and one second past it, the moment the facts are of, its
    /// equality with another such status and the hash they share. <see cref="ClusterStatus.Pods" /> is the one member
    /// that reads them, and it says what it found the way LINQ does - an <see cref="ArgumentNullException" /> for the
    /// source that is not there - and the line the record says follows it, since the line names the pods too. A status
    /// that lacks only its nodes says its line and lists no pod: <see cref="ClusterStatus.Pods" /> never looks at
    /// nodes.</summary>
    [Fact]
    public void AStatusWithNoListsAtAllKeepsEveryRuleButTheOneThatReadsThem()
    {
        var bare = new ClusterStatus(Generated, 900, null, null!, null!);
        var other = new ClusterStatus(Generated, 900, null, null!, null!);

        Assert.Null(bare.Nodes);
        Assert.Null(bare.Namespaces);
        Assert.Equal(TimeSpan.FromMinutes(60), bare.StaleAfter);
        Assert.False(bare.IsStale(Generated + TimeSpan.FromMinutes(60)));
        Assert.True(bare.IsStale(Generated + TimeSpan.FromMinutes(60) + TimeSpan.FromSeconds(1)));
        Assert.Equal(Generated, bare.AsOf(Generated.AddHours(5)));
        Assert.Equal(bare, other);
        Assert.Equal(bare.GetHashCode(), other.GetHashCode());

        var pods = Assert.Throws<ArgumentNullException>(() => bare.Pods.ToList());
        Assert.Equal("source", pods.ParamName);
        Assert.Throws<ArgumentNullException>(() => bare.ToString());

        var noNodes = new ClusterStatus(null, null, null, null!, []);

        Assert.Null(noNodes.Nodes);
        Assert.Empty(noNodes.Pods);
        Assert.Contains("Nodes = ,", noNodes.ToString(), StringComparison.Ordinal);
        Assert.Contains("StaleAfter = 00:01:00", noNodes.ToString(), StringComparison.Ordinal);
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

        Assert.Null(parsed.Problem);
        Assert.NotNull(parsed.Value);

        return parsed.Value!;
    }
}
