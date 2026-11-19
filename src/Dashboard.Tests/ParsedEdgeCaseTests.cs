namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of the types <c>ClusterStatus.cs</c> reads the cluster's
/// status file into: the two parts of <see cref="Parsed{ClusterStatus}" /> and when two of them are one;
/// the two numbers of <see cref="ClusterStatus" /> the staleness is counted in, the age at which a file is
/// stale at the ends the count runs to, the moment the facts are of, and the order the pods stand in; the
/// four numbers of <see cref="ResourceUse" /> and the nothing where no number is;
/// <see cref="ClusterNode.IsHealthy" /> at every corner of the ready and the pressure; and the words and
/// the words' ends the <see cref="ClusterPod" /> and the <see cref="ClusterVolume" /> stand on: the five
/// words of the phase, the label where the workload is the empty word, the restarts at the two ends of the
/// number, the capacity at its ends, and the parts the records keep and the copies that are them again.
/// <see cref="ClusterParserTests" /> pins the sample file and the file every part of but the names is
/// optional at, every part as the value it stands on, and the files that are not the file as the problems
/// they are in words; <see cref="ClusterAssessmentTests" /> pins the tick the status turns stale at, a
/// status that does not say when it was written, and a pod's state from its phase, its readiness and its
/// age; and <see cref="ClusterLivenessEdgeCaseTests" /> pins the liveness the page observes at those ticks.
/// What they leave open is pinned here: the problem a parse does not carry, a parse that is the same one
/// while its parts are; the two numbers the staleness is counted in, to their value, and the age at which
/// the file is stale at each end of the interval - the interval of no file, of zero, of a negative, of the
/// one whose four are the minute at least and of the first whose four are more; the moment the facts are of
/// where the status says when and where it does not; the pods in the order the namespaces list them, and the
/// status and the space that list no pod; the JSON that is a value and not an object, the file whose lists
/// are empty, and the name that is only spaces, beside the one that stands trimmed; the number a resource
/// keeps - the zero that is a number and not the nothing it is not, the top and the bottom of the number -
/// and the parts it falls apart into; the node that is ready and under no pressure, and the cordon that
/// does not make it otherwise; the five words of the phase as the words Kubernetes uses, the workload that
/// is the empty word and is no fallback to the name, and the restarts at the two ends of the number; and
/// the volume's capacity at its ends and the phase that is the empty word.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every moment is one the test brings,
/// and every status is one the test builds or the parser reads at a fixed file.
/// </para>
/// </summary>
public class ParsedEdgeCaseTests
{
    private static readonly DateTimeOffset Generated = ClusterFixture.Generated;

    // ----- Parsed<T> -----

    /// <summary>A parse that does not carry a word for why its value is not there carries no word: the part
    /// the parse was not given stands the nothing it is, not the empty word it is not.</summary>
    [Fact]
    public void AParseThatCarriesNoWordForWhyCarriesNone()
    {
        var parsed = new Parsed<ClusterStatus>(null);

        Assert.Null(parsed.Value);
        Assert.Null(parsed.Problem);
    }

    /// <summary>The parse falls apart into its two parts - the value or the nothing, and the word or the
    /// nothing, each as the value it stands on - and a copy of it is it again: the <c>with</c> that names no
    /// part keeps both, and the <c>with</c> that turns one of them keeps the other it does not name, and not
    /// the same reference.</summary>
    [Fact]
    public void AParseFallsApartIntoItsPartsAndACopyIsTheParseAgain()
    {
        var status = ClusterFixture.Status();
        var parsed = new Parsed<ClusterStatus>(status, "the file is empty");

        var (value, problem) = parsed;
        Assert.Same(status, value);
        Assert.Equal("the file is empty", problem);

        var copy = parsed with { };
        Assert.Equal(parsed, copy);
        Assert.NotSame(parsed, copy);
        Assert.Same(status, copy.Value);

        var onlyValue = parsed with { Problem = null };
        Assert.Same(status, onlyValue.Value);
        Assert.Null(onlyValue.Problem);

        var onlyProblem = parsed with { Value = null };
        Assert.Null(onlyProblem.Value);
        Assert.Equal("the file is empty", onlyProblem.Problem);
    }

    /// <summary>Two parses are one while their two parts are - the value the same, the word the same - and a
    /// difference in any one of them, the value turned or made absent included, is another one: the equality
    /// of the operators and of <c>Equals</c>, and the hash code that follows them.</summary>
    [Fact]
    public void TwoParsesAreOneWhileTheirPartsAre()
    {
        var status = ClusterFixture.Status();
        var one = new Parsed<ClusterStatus>(status, "the file is empty");
        var two = new Parsed<ClusterStatus>(status, "the file is empty");

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.True(one.Equals((object)two));
        Assert.True(two.Equals((object)one));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.False(one.Equals(null));

        Assert.NotEqual(one, new Parsed<ClusterStatus>(status, "the file is not valid JSON"));
        Assert.NotEqual(one, new Parsed<ClusterStatus>(status with { KubernetesVersion = "v9.9.9" }, "the file is empty"));
        Assert.NotEqual(one, new Parsed<ClusterStatus>(null, "the file is empty"));
        Assert.Equal(one, one with { Value = one.Value, Problem = one.Problem });
    }

    // ----- ClusterStatus: the numbers the staleness is counted in -----

    /// <summary>The two numbers the staleness is counted in, to their value: the intervals the age runs to
    /// four of, and the minute at least no file younger than is stale, however often the collector writes.</summary>
    [Fact]
    public void TheTwoNumbersTheStalenessIsCountedInStandAtTheirValues()
    {
        Assert.Equal(4, ClusterStatus.StaleIntervals);
        Assert.Equal(TimeSpan.FromSeconds(60), ClusterStatus.MinimumStaleAfter);
    }

    /// <summary>The age at which the file is stale, at the ends the count runs to: the interval that is not
    /// there, the zero and the negative that make no age at all, and the first of them whose four are the
    /// minute at least, stand at the minute; the first interval whose four are more than the minute stands at
    /// its four, and the long interval at the four it says.</summary>
    [Theory]
    [InlineData(null, 60)]
    [InlineData(0, 60)]
    [InlineData(-15, 60)]
    [InlineData(15, 60)]
    [InlineData(16, 64)]
    [InlineData(30, 120)]
    public void TheAgeAtWhichTheFileIsStaleIsFourIntervalsAndAMinuteAtLeast(int? interval, double seconds)
    {
        var status = ClusterFixture.Status() with { IntervalSeconds = interval };

        Assert.Equal(TimeSpan.FromSeconds(seconds), status.StaleAfter);
    }

    /// <summary>The moment the facts are of: the moment the status says it wrote them, however far the page's
    /// clock has run past it, and the page's clock itself where the status does not say when - the same
    /// moment the clock stands at, said twice.</summary>
    [Fact]
    public void TheFactsAreOfTheMomentTheStatusSaysAndOfThePagesClockWhereItDoesNotSay()
    {
        var status = ClusterFixture.Status();

        Assert.Equal(Generated, status.AsOf(Generated.AddDays(3)));

        var silent = status with { Generated = null };
        Assert.Equal(Generated.AddSeconds(90), silent.AsOf(Generated.AddSeconds(90)));
        Assert.Equal(Generated, silent.AsOf(Generated));
    }

    /// <summary>Two statuses are one while their parts are: the lists the same they were given, the moments
    /// and the words the same - and a difference in any one of them, the lists made none and the namespaces
    /// gained included, is another one: the equality of the operators and the hash code that follows
    /// them.</summary>
    [Fact]
    public void TwoStatusesAreOneWhileTheirPartsAre()
    {
        IReadOnlyList<ClusterNode> nodes = [ClusterFixture.Node()];
        var one = new ClusterStatus(Generated, 15, "v1.33.3", nodes, []);
        var two = new ClusterStatus(Generated, 15, "v1.33.3", nodes, []);

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());

        Assert.NotEqual(one, one with { Generated = Generated.AddSeconds(1) });
        Assert.NotEqual(one, one with { IntervalSeconds = 30 });
        Assert.NotEqual(one, one with { KubernetesVersion = "v9.9.9" });
        Assert.NotEqual(one, one with { Nodes = [] });
        Assert.NotEqual(one, one with { Namespaces = [ClusterFixture.Space("apps", ClusterFixture.Pod("ui"))] });
    }

    // ----- ClusterStatus: the pods, in the order of the file -----

    /// <summary>The pods stand in the order the namespaces list them: every pod of the first space before the
    /// first of the second, the pair of each the space it stands in and the pod it is, in the order the file
    /// stood on.</summary>
    [Fact]
    public void ThePodsStandInTheOrderTheNamespacesListThem()
    {
        var first = ClusterFixture.Space("apps", ClusterFixture.Pod("ui"), ClusterFixture.Pod("db"));
        var second = ClusterFixture.Space("jobs", ClusterFixture.Pod("backup", ClusterPod.Succeeded, false, reason: "Completed"));
        var status = ClusterFixture.Status([], first, second);

        Assert.Equal(
            new[]
            {
                (first, first.Pods[0]),
                (first, first.Pods[1]),
                (second, second.Pods[0]),
            },
            status.Pods);
    }

    /// <summary>The status without a namespace and the namespace without a pod list no pod: the flattened
    /// list is the empty one, and the space that is not there contributes no pair at all.</summary>
    [Fact]
    public void AStatusWithoutNamespacesAndANamespaceWithoutPodsListNoPod()
    {
        Assert.Empty(ClusterFixture.Status([]).Pods);
        Assert.Empty(ClusterFixture.Status([], ClusterFixture.Space("apps")).Pods);
    }

    // ----- ClusterStatus.Parse: the files the other tests leave open -----

    /// <summary>A JSON that is a value and not an object - the null, the number and the boolean the file
    /// could stand as - is the problem in the words the page stands on, beside the array and the text the
    /// other tests pin.</summary>
    [Theory]
    [InlineData("null")]
    [InlineData("123")]
    [InlineData("true")]
    public void AJsonThatIsAValueAndNotAnObjectIsAProblemInWords(string json)
    {
        var parsed = ClusterStatus.Parse(json);

        Assert.Null(parsed.Value);
        Assert.Equal("the file does not contain a JSON object", parsed.Problem);
    }

    /// <summary>A file whose lists are the empty ones is a status that names nothing: a <c>nodes</c> that is
    /// the empty array is an array, so the file is the file, and what it names is nothing - no node, no
    /// namespace, no pod.</summary>
    [Fact]
    public void AFileWhoseListsAreEmptyIsAStatusThatNamesNothing()
    {
        var noNodes = ClusterStatus.Parse("""{ "nodes": [] }""").Value!;
        var noNamespaces = ClusterStatus.Parse("""{ "namespaces": [] }""").Value!;
        var neither = ClusterStatus.Parse("""{ "nodes": [], "namespaces": [] }""").Value!;

        foreach (var status in new[] { noNodes, noNamespaces, neither })
        {
            Assert.Empty(status.Nodes);
            Assert.Empty(status.Namespaces);
            Assert.Empty(status.Pods);
        }
    }

    /// <summary>A name that is only spaces is no name: the node, the namespace, the pod and the volume that
    /// carry it are left out, and the name that stands with spaces about it stands trimmed - the word it is,
    /// not the word it looks like.</summary>
    [Fact]
    public void ANameThatIsOnlySpacesIsNoNameAndANameWithSpacesStandsTrimmed()
    {
        var status = ClusterStatus.Parse("""
            { "nodes": [ { "name": "   " }, { "name": "  node-1  " } ],
              "namespaces": [
                { "name": "\t" },
                { "name": "apps", "pods": [ { "name": " " } ], "volumes": [ { "name": "  " } ] }
              ] }
            """).Value!;

        var node = Assert.Single(status.Nodes);
        Assert.Equal("node-1", node.Name);

        var space = Assert.Single(status.Namespaces);
        Assert.Equal("apps", space.Name);
        Assert.Empty(space.Pods);
        Assert.Empty(space.Volumes);
    }

    // ----- ResourceUse -----

    /// <summary>The resource keeps the very numbers it is given, at the two ends of the number the number
    /// stands in and at the zero that is a number: the fourth part the resource was not given stands the
    /// nothing it is, and the zero that is a number is kept as the number it is, not the nothing it is not -
    /// the usage that uses nothing is not the usage that says no usage.</summary>
    [Fact]
    public void AResourceKeepsTheNumbersItIsGivenAndTheZeroThatIsANumber()
    {
        var use = new ResourceUse(1.5, 2.5, 3.5, 4.5);
        Assert.Equal(1.5, use.Usage);
        Assert.Equal(2.5, use.Requests);
        Assert.Equal(3.5, use.Limits);
        Assert.Equal(4.5, use.Allocatable);

        var withoutAllocatable = new ResourceUse(0, 0, 0);
        Assert.Null(withoutAllocatable.Allocatable);
        Assert.NotEqual(ResourceUse.None, withoutAllocatable);

        var ends = new ResourceUse(double.MaxValue, double.MinValue, 0, null);
        Assert.Equal(double.MaxValue, ends.Usage);
        Assert.Equal(double.MinValue, ends.Requests);
        Assert.Equal(0d, ends.Limits);
        Assert.Null(ends.Allocatable);

        Assert.Equal(new ResourceUse(null, null, null, null), ResourceUse.None);
    }

    /// <summary>The resource falls apart into its four parts, each as the value it stands on - the numbers
    /// or the nothing - and a copy of it is it again: the <c>with</c> that names no part keeps all four,
    /// and the <c>with</c> that turns one of them keeps the other it does not name, and not the same
    /// reference.</summary>
    [Fact]
    public void AResourceFallsApartIntoItsPartsAndACopyIsTheResourceAgain()
    {
        var use = new ResourceUse(10, 100, 500, 2000);

        var (usage, requests, limits, allocatable) = use;
        Assert.Equal(10d, usage);
        Assert.Equal(100d, requests);
        Assert.Equal(500d, limits);
        Assert.Equal(2000d, allocatable);

        var (noUsage, noRequests, noLimits, noAllocatable) = new ResourceUse(null, null, null, null);
        Assert.Null(noUsage);
        Assert.Null(noRequests);
        Assert.Null(noLimits);
        Assert.Null(noAllocatable);

        var copy = use with { };
        Assert.Equal(use, copy);
        Assert.NotSame(use, copy);

        var withoutLimits = use with { Limits = null };
        Assert.Null(withoutLimits.Limits);
        Assert.Equal(2000d, withoutLimits.Allocatable);
        Assert.NotEqual(use, withoutLimits);
    }

    /// <summary>Two resources are one while their four numbers are, the nothing the number is not included:
    /// a difference in any one of the four, the zero that is not the nothing it is not included, is another
    /// one - the equality of the operators and the hash code that follows them.</summary>
    [Fact]
    public void TwoResourcesAreOneWhileTheirNumbersAre()
    {
        var one = new ResourceUse(1.5, null, 3.5, null);
        var two = new ResourceUse(1.5, null, 3.5, null);

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.True(one.Equals((object)two));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.False(one.Equals(null));

        Assert.NotEqual(one, new ResourceUse(2.5, null, 3.5, null));
        Assert.NotEqual(one, new ResourceUse(1.5, 0d, 3.5, null));
        Assert.NotEqual(one, new ResourceUse(1.5, null, 4.5, null));
        Assert.NotEqual(one, new ResourceUse(1.5, null, 3.5, 0d));
    }

    // ----- ClusterNode -----

    /// <summary>Ready and under no pressure, at every corner of the two: the ready node with no pressure is
    /// the healthy one, the ready node under a pressure is not, and the node that is not ready is not,
    /// under a pressure or without one.</summary>
    [Theory]
    [InlineData(true, 0, true)]
    [InlineData(true, 1, false)]
    [InlineData(true, 2, false)]
    [InlineData(false, 0, false)]
    [InlineData(false, 2, false)]
    public void ANodeIsHealthyWhenItIsReadyAndUnderNoPressure(bool ready, int pressures, bool healthy)
    {
        var node = ClusterFixture.Node() with
        {
            Ready = ready,
            Pressures = pressures switch
            {
                1 => ["MemoryPressure"],
                2 => ["MemoryPressure", "DiskPressure"],
                _ => [],
            },
        };

        Assert.Equal(healthy, node.IsHealthy);
    }

    /// <summary>The cordon does not make the healthy node otherwise: the node that takes no new pod, ready
    /// and under no pressure, stands as the healthy one it is.</summary>
    [Fact]
    public void ACordonedNodeIsStillAHealthyOne()
    {
        var cordoned = ClusterFixture.Node() with { Unschedulable = true };

        Assert.True(cordoned.IsHealthy);
    }

    /// <summary>Two nodes are one while their parts are, each as the value it stands on - the pressures the
    /// node is not under included - and a difference in any one of them is another one: the readiness turned,
    /// the pressure gained, the cordon set.</summary>
    [Fact]
    public void TwoNodesAreOneWhileTheirPartsAre()
    {
        var one = ClusterFixture.Node();
        var two = ClusterFixture.Node();

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());

        Assert.NotEqual(one, one with { Ready = false });
        Assert.NotEqual(one, one with { Pressures = ["MemoryPressure"] });
        Assert.NotEqual(one, one with { Unschedulable = true });

        var copy = one with { };
        Assert.Equal(one, copy);
        Assert.NotSame(one, copy);
    }

    // ----- ClusterNamespace -----

    /// <summary>The namespace without a pod and without a volume says so: the two lists stand the empty
    /// ones they are, and the namespace is one while its three parts are - the pod gained and the volume
    /// gained, each, another one.</summary>
    [Fact]
    public void ANamespaceWithoutPodsAndWithoutVolumesSaysSo()
    {
        var space = new ClusterNamespace("apps", [], []);

        Assert.Equal("apps", space.Name);
        Assert.Empty(space.Pods);
        Assert.Empty(space.Volumes);

        Assert.Equal(space, new ClusterNamespace("apps", [], []));
        Assert.NotEqual(space, new ClusterNamespace("apps", [ClusterFixture.Pod("ui")], []));
        Assert.NotEqual(space, new ClusterNamespace("apps", [], [new ClusterVolume("data", null, null)]));

        var copy = space with { };
        Assert.Equal(space, copy);
        Assert.NotSame(space, copy);
    }

    // ----- ClusterPod -----

    /// <summary>The five words of the phase, as the words Kubernetes uses them: the two the sample file
    /// stands on, the words it says them with, and the three it does not, the words the page and the file
    /// both stand on.</summary>
    [Fact]
    public void ThePhasesStandAsTheWordsKubernetesUses()
    {
        Assert.Equal("Pending", ClusterPod.Pending);
        Assert.Equal("Running", ClusterPod.Running);
        Assert.Equal("Succeeded", ClusterPod.Succeeded);
        Assert.Equal("Failed", ClusterPod.Failed);
        Assert.Equal("Unknown", ClusterPod.Unknown);
    }

    /// <summary>The label the page calls the pod by, at the word the workload is: the workload that is the
    /// empty word is still a word, and stands where it stands - no fallback to the name it is not - the
    /// spaces about it kept as the spaces they are, and the name the fallback stands at, empty where the
    /// name is.</summary>
    [Fact]
    public void AWorkloadThatIsAnEmptyWordIsStillAWordThePodStandsBy()
    {
        var empty = ClusterFixture.Pod("ui") with { Workload = "" };
        Assert.Equal(string.Empty, empty.Label);

        var padded = ClusterFixture.Pod("ui") with { Workload = "  " };
        Assert.Equal("  ", padded.Label);

        var nameless = ClusterFixture.Pod("ui") with { Workload = null, Name = "" };
        Assert.Equal(string.Empty, nameless.Label);
    }

    /// <summary>The restarts stand where they stand, at the two ends of the number the number stands in and
    /// at the zero that is a number: the pod that was started again the most times the top, the one the
    /// file says nothing sensible about the bottom, and the one that was not the zero it is, kept as the
    /// number it is.</summary>
    [Fact]
    public void TheRestartsStandAtTheEndsOfTheNumberAndTheZeroItIs()
    {
        var top = ClusterFixture.Pod("ui") with { Restarts = int.MaxValue };
        var bottom = ClusterFixture.Pod("ui") with { Restarts = int.MinValue };
        var none = ClusterFixture.Pod("ui") with { Restarts = 0 };

        Assert.Equal(int.MaxValue, top.Restarts);
        Assert.Equal(int.MinValue, bottom.Restarts);
        Assert.Equal(0, none.Restarts);
    }

    /// <summary>Two pods are one while their parts are, each as the value it stands on, and a difference in
    /// any one of them is another one: the restarts gained, the phase turned.</summary>
    [Fact]
    public void TwoPodsAreOneWhileTheirPartsAre()
    {
        var one = ClusterFixture.Pod("ui");
        var two = ClusterFixture.Pod("ui");

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());

        Assert.NotEqual(one, one with { Restarts = 7 });
        Assert.NotEqual(one, one with { Phase = ClusterPod.Pending });

        var copy = one with { };
        Assert.Equal(one, copy);
        Assert.NotSame(one, copy);
    }

    // ----- ClusterVolume -----

    /// <summary>The volume keeps the capacity it is given, at the zero that is a number and at the top of
    /// the number, and says nothing where none is given: the phase the volume was not given stands the
    /// nothing it is, and the phase that is the empty word stands the empty word it is, kept, not read as
    /// absent.</summary>
    [Fact]
    public void AVolumeKeepsItsCapacityAtTheEndsAndSaysNothingWhereNoneIsGiven()
    {
        var noCapacity = new ClusterVolume("data", null, null);
        Assert.Null(noCapacity.Capacity);
        Assert.Null(noCapacity.Phase);

        var zero = new ClusterVolume("data", 0, "Bound");
        Assert.Equal(0d, zero.Capacity);
        Assert.Equal("Bound", zero.Phase);

        var top = new ClusterVolume("data", double.MaxValue, string.Empty);
        Assert.Equal(double.MaxValue, top.Capacity);
        Assert.Equal(string.Empty, top.Phase);

        Assert.Equal(noCapacity, new ClusterVolume("data", null, null));
        Assert.NotEqual(noCapacity, new ClusterVolume("data", 0, null));
        Assert.NotEqual(zero, new ClusterVolume("data", 0, "Pending"));

        var copy = noCapacity with { };
        Assert.Equal(noCapacity, copy);
        Assert.NotSame(noCapacity, copy);
    }
}
