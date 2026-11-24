using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="ClusterPod" /> is one pod as the collector wrote it down: thirteen parts, the five words the phase
/// is said in, and the one word the page calls the pod by.
/// <see cref="ParsedEdgeCaseTests" /> pins the five words of <see cref="ClusterPod.Pending" /> to
/// <see cref="ClusterPod.Unknown" /> at the words Kubernetes uses, <see cref="ClusterPod.Label" /> at the workload
/// that is the empty word, the word of spaces and the nothing that falls back to the name, the
/// <see cref="ClusterPod.Restarts" /> at the two ends of the number and the zero it is, and two pods at one while
/// their parts are - the restarts gained and the phase turned; <see cref="ClusterParserTests" /> pins the sample
/// file's pods part by part, the pod the file barely names at the nothing every other part falls back to, and the
/// phase of a file that says none; <see cref="ClusterAssessmentTests" /> and <see cref="PodStateEdgeCaseTests" />
/// pin what the phase, the readiness and the started moment add up to in <see cref="PodRules" />; and
/// <see cref="PodRowEdgeCaseTests" /> and <see cref="ClusterTextEdgeCaseTests" /> pin the words the page says a pod
/// in.
/// <para>
/// What is pinned here is what they leave open, and only that: the thirteen parts as the very values the pod is
/// given - the words as the words, at the empty word, at the spaces about a word and at the length a word stands
/// on, none of them trimmed and none of them dropped, since the pod is not the parser - the nothing it is given
/// standing the nothing it is and not the zero or the empty word it is not; a phase that is none of the five words
/// standing where it stands; the two container numbers at the ends of the number, at the zero and at the ready
/// that outnumbers the containers; the started moment at the two ends of the moment and with the offset it is
/// written with kept; the cpu and the memory as the very resources they are and compared as the values they are;
/// the pod falling apart into its thirteen parts; the one line the record says about itself, the label said with
/// them and the nothing and the empty word said the same way; the copy that turns one part and keeps the rest; the
/// parts that each turn two pods apart, the empty word apart from the nothing; and the surface itself, the thirteen
/// parts, the label, the five words and the members every record is asked for.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every pod is one the test builds, every
/// moment one the test brings, and the one file read is the sample that lies next to this test.
/// </para>
/// </summary>
public class ClusterPodEdgeCaseTests
{
    /// <summary>The moment the pod below started, in the words the sample file writes it.</summary>
    private static readonly DateTimeOffset Started = new(2026, 10, 6, 20, 1, 12, TimeSpan.Zero);

    /// <summary>A pod of a crashed deployment: every part a word, a number or a moment of its own, none of them the
    /// fallback the parser reaches for.</summary>
    private static ClusterPod One() =>
        new(
            "ui-7f6b5c4d3-m8n2p",
            "ui",
            "Deployment",
            "aks-system-12148983-vmss000000",
            ClusterPod.Running,
            false,
            1,
            0,
            7,
            "CrashLoopBackOff",
            Started,
            new ResourceUse(0, 100, 500),
            new ResourceUse(21000000, 268435456, 536870912));

    // ----------------------------------------------------------------- the surface the pod owes its readers

    /// <summary>
    /// The public surface of the pod: its thirteen parts in the order the record declares them and the label the
    /// parts make, the members every record is asked for, and the five words of the phase as words a caller may
    /// stand on. A fourteenth part, or a word of the phase lost, moves this line and nothing else.
    /// </summary>
    [Fact]
    public void ThePodHasThirteenPartsTheLabelAndTheFiveWordsOfThePhase()
    {
        Assert.Equal(
            [
                "Name", "Workload", "Kind", "Node", "Phase", "Ready", "Containers", "ContainersReady",
                "Restarts", "Reason", "StartedAt", "Cpu", "Memory", "Label",
            ],
            [.. typeof(ClusterPod)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            [
                "Containers", "ContainersReady", "Cpu", "Deconstruct", "Equals", "GetHashCode", "Kind", "Label",
                "Memory", "Name", "Node", "Phase", "Ready", "Reason", "Restarts", "StartedAt", "ToString",
                "Workload", "op_Equality", "op_Inequality",
            ],
            PublicSurface());

        Assert.Equal(
            ["Failed", "Pending", "Running", "Succeeded", "Unknown"],
            [.. typeof(ClusterPod)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(field => field.IsLiteral)
                .Select(field => field.Name)
                .Order(StringComparer.Ordinal)]);

        var constructor = Assert.Single(typeof(ClusterPod).GetConstructors());

        Assert.Equal(
            [
                typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool),
                typeof(int?), typeof(int?), typeof(int), typeof(string), typeof(DateTimeOffset?),
                typeof(ResourceUse), typeof(ResourceUse),
            ],
            [.. constructor.GetParameters().Select(parameter => parameter.ParameterType)]);
    }

    /// <summary>The part the page calls the pod by is worked out of the two it stands on and is given nothing of
    /// its own: the label is read and never written, it is no part of the constructor, and it is the one word of the
    /// pod a copy cannot turn - which is why the tests below turn it only by turning the workload or the name.</summary>
    [Fact]
    public void TheLabelIsReadOfThePartsAndIsNoPartOfItsOwn()
    {
        var label = Assert.Single(
            typeof(ClusterPod).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            property => property.Name.Equals("Label", StringComparison.Ordinal));

        Assert.Null(label.GetSetMethod());
        Assert.Equal(typeof(string), label.PropertyType);
        string[] parts = [.. typeof(ClusterPod).GetConstructors().Single().GetParameters().Select(parameter => parameter.Name!)];

        Assert.Equal(13, parts.Length);
        Assert.DoesNotContain("Label", parts);
    }

    // --------------------------------------------------------------------- the parts the pod keeps

    /// <summary>
    /// The pod keeps the very parts it is given: the five words as the words they are, the readiness as the
    /// readiness it is, the four numbers as the numbers they stand on, the moment as the moment, and the cpu and
    /// the memory as the very resources it was handed - the same instances, not copies of them.
    /// </summary>
    [Fact]
    public void APodKeepsTheVeryPartsItIsGiven()
    {
        var cpu = new ResourceUse(0, 100, 500);
        var memory = new ResourceUse(21000000, 268435456, 536870912);
        var pod = new ClusterPod(
            "ui-7f6b5c4d3-m8n2p",
            "ui",
            "Deployment",
            "aks-system-12148983-vmss000000",
            ClusterPod.Failed,
            true,
            2,
            1,
            7,
            "Evicted",
            Started,
            cpu,
            memory);

        Assert.Equal("ui-7f6b5c4d3-m8n2p", pod.Name);
        Assert.Equal("ui", pod.Workload);
        Assert.Equal("Deployment", pod.Kind);
        Assert.Equal("aks-system-12148983-vmss000000", pod.Node);
        Assert.Equal(ClusterPod.Failed, pod.Phase);
        Assert.True(pod.Ready);
        Assert.Equal(2, pod.Containers);
        Assert.Equal(1, pod.ContainersReady);
        Assert.Equal(7, pod.Restarts);
        Assert.Equal("Evicted", pod.Reason);
        Assert.Equal(Started, pod.StartedAt);
        Assert.Same(cpu, pod.Cpu);
        Assert.Same(memory, pod.Memory);
    }

    /// <summary>
    /// The pod keeps the nothing it is given as the nothing it is: the workload, the kind, the node, the reason and
    /// the started moment stand the nothing they are given, the two container numbers stand the nothing that is not
    /// a zero, the readiness stands the false that is not the nothing it is not, the restarts are the non-nullable
    /// number they are and stand at zero, and the cpu and the memory stand the resource that says no number at all.
    /// </summary>
    [Fact]
    public void APodKeepsTheNothingItIsGiven()
    {
        var pod = new ClusterPod(
            "web-1", null, null, null, ClusterPod.Unknown, false, null, null, 0, null, null,
            ResourceUse.None, ResourceUse.None);

        Assert.Equal("web-1", pod.Name);
        Assert.Null(pod.Workload);
        Assert.Null(pod.Kind);
        Assert.Null(pod.Node);
        Assert.Equal(ClusterPod.Unknown, pod.Phase);
        Assert.False(pod.Ready);
        Assert.Null(pod.Containers);
        Assert.Null(pod.ContainersReady);
        Assert.Equal(0, pod.Restarts);
        Assert.Null(pod.Reason);
        Assert.Null(pod.StartedAt);
        Assert.Equal(ResourceUse.None, pod.Cpu);
        Assert.Equal(ResourceUse.None, pod.Memory);
    }

    /// <summary>
    /// The empty word the pod is given stands the empty word it is, and the spaces about a word are kept as the
    /// spaces they are: the name, the workload, the kind, the node, the phase and the reason each keep what they
    /// were handed - the pod is not the parser, and does not drop what is empty and does not trim what has spaces
    /// about it, and does not fall back from an empty workload to the name it is next to.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ui  ")]
    [InlineData("a")]
    public void AWordThePodKeepsStandsTheWordItIsAtTheEndsTheWordStandsIn(string word)
    {
        var pod = One() with
        {
            Name = word,
            Workload = word,
            Kind = word,
            Node = word,
            Phase = word,
            Reason = word,
        };

        Assert.Equal(word, pod.Name);
        Assert.Equal(word, pod.Workload);
        Assert.Equal(word, pod.Kind);
        Assert.Equal(word, pod.Node);
        Assert.Equal(word, pod.Phase);
        Assert.Equal(word, pod.Reason);
        Assert.Equal(word, pod.Label);
    }

    /// <summary>
    /// The length a word stands on is the length the pod keeps it at: a name the full 253 characters Kubernetes
    /// allows a pod, a reason of a thousand, a workload and a node at the length they may reach - none of them
    /// shortened, none of them wrapped, and all of them still the words the pod was given.
    /// </summary>
    [Fact]
    public void AWordThePodKeepsStandsAtTheLengthItStandsOn()
    {
        var name = $"{new string('u', 63)}-{new string('v', 63)}-{new string('w', 63)}-{new string('x', 61)}";
        Assert.Equal(253, name.Length);

        var workload = new string('r', 253);
        var kind = new string('k', 63);
        var node = new string('n', 253);
        var reason = new string('e', 1024);
        var pod = new ClusterPod(name, workload, kind, node, ClusterPod.Running, true, 1, 1, 0, reason, Started, ResourceUse.None, ResourceUse.None);

        Assert.Equal(name, pod.Name);
        Assert.Equal(workload, pod.Workload);
        Assert.Equal(kind, pod.Kind);
        Assert.Equal(node, pod.Node);
        Assert.Equal(reason, pod.Reason);
        Assert.Equal(workload, pod.Label);
    }

    /// <summary>
    /// The five words of the phase are the words the file is written in and no fence the pod keeps its phase
    /// behind: the empty word, the word of spaces, the phase in a case the file never writes and a word nobody
    /// knows each stand as the words they are given - and the pod that stands at one of them is another pod from
    /// the one that stands at <see cref="ClusterPod.Running" />.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("running")]
    [InlineData("ContainerCreating")]
    [InlineData("Terminating")]
    [InlineData("\u00e9\u2603")]
    public void APhaseThatIsNoneOfTheFiveWordsStandsTheWordItIs(string phase)
    {
        var pod = ClusterFixture.Pod("ui") with { Phase = phase };

        Assert.Equal(phase, pod.Phase);
        Assert.NotEqual(ClusterPod.Running, pod.Phase);
        Assert.NotEqual(ClusterFixture.Pod("ui"), pod);
    }

    /// <summary>
    /// The two container numbers stand where they are given: at the nothing that is no number, at the zero that is
    /// one, at the two ends of the number the number stands in, at the ready that outnumbers the containers and at
    /// the containers that outnumber what is ready - the pod counts what the file says and works out nothing of it.
    /// </summary>
    [Theory]
    [InlineData(null, null)]
    [InlineData(0, 0)]
    [InlineData(-1, -1)]
    [InlineData(0, 3)]
    [InlineData(3, 0)]
    [InlineData(1, 1)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public void TheContainerNumbersStandAtTheEndsOfTheNumberTheyAreGiven(int? containers, int? ready)
    {
        var pod = One() with { Containers = containers, ContainersReady = ready };

        Assert.Equal(containers, pod.Containers);
        Assert.Equal(ready, pod.ContainersReady);
    }

    /// <summary>
    /// The started moment stands where it is given, at the two ends of the moment the moment stands in and with the
    /// offset it was written with: the pod says back the instant and the offset it was handed, two hours east or
    /// five hours west, and a pod that says nothing about its start keeps the nothing and not the moment the page
    /// happens to be read at. The instant is what the pod is weighed on: one instant spelled in three offsets is
    /// the same moment to the record, and so the same pod, while the moment it says back keeps the offset it came
    /// in with.
    /// </summary>
    [Fact]
    public void AStartedMomentStandsWhereItIsGivenWithTheOffsetItIsWrittenWith()
    {
        var east = new DateTimeOffset(2026, 10, 6, 22, 1, 12, TimeSpan.FromHours(2));
        var west = new DateTimeOffset(2026, 10, 6, 15, 1, 12, TimeSpan.FromHours(-5));

        Assert.Equal(Started, One().StartedAt);
        Assert.Equal(DateTimeOffset.MinValue, (One() with { StartedAt = DateTimeOffset.MinValue }).StartedAt);
        Assert.Equal(DateTimeOffset.MaxValue, (One() with { StartedAt = DateTimeOffset.MaxValue }).StartedAt);
        Assert.Null((One() with { StartedAt = null }).StartedAt);

        foreach (var moment in new[] { Started, east, west })
        {
            var pod = One() with { StartedAt = moment };

            Assert.Equal(moment, pod.StartedAt);
            Assert.Equal(moment.UtcDateTime, pod.StartedAt!.Value.UtcDateTime);
            Assert.Equal(moment.Offset, pod.StartedAt.Value.Offset);
        }

        // One instant spelled three ways: the pod is the instant, and the offset comes back with it.
        Assert.Equal(Started.UtcDateTime, east.UtcDateTime);
        Assert.Equal(Started, west);
        Assert.Equal(One() with { StartedAt = Started }, One() with { StartedAt = east });
        Assert.Equal(One() with { StartedAt = east }, One() with { StartedAt = west });
        Assert.Equal((One() with { StartedAt = east }).GetHashCode(), (One() with { StartedAt = west }).GetHashCode());
        Assert.NotEqual((One() with { StartedAt = east }).StartedAt!.Value.Offset, (One() with { StartedAt = west }).StartedAt!.Value.Offset);

        // And a pod that says nothing about its start is another pod from a pod that says one instant.
        Assert.NotEqual(One() with { StartedAt = east }, One() with { StartedAt = null });
        Assert.NotEqual(One() with { StartedAt = east }, One() with { StartedAt = Started.AddSeconds(1) });
    }

    /// <summary>
    /// The cpu and the memory are the pod's own parts and nothing else's: a pod given the two resources keeps the
    /// very instances it was given, and the resource that says no number is the one it keeps for a pod the file
    /// says nothing of. Two instances of the same two numbers make one pod with the other - the resources are
    /// weighed as the values they are, unlike the lists a namespace keeps, which are weighed as the lists they are.
    /// </summary>
    [Fact]
    public void TheResourcesAreTheVeryOnesGivenAndWeighedAsTheValuesTheyAre()
    {
        var cpu = new ResourceUse(10, 100, 500);
        var pod = ClusterFixture.Pod("ui") with { Cpu = cpu };

        var twin = pod with { Cpu = new ResourceUse(10, 100, 500) };

        Assert.Same(cpu, pod.Cpu);
        Assert.NotSame(cpu, twin.Cpu);
        Assert.Same(ResourceUse.None, (pod with { Memory = ResourceUse.None }).Memory);

        Assert.Equal(ClusterFixture.Pod("ui"), pod with { Cpu = new ResourceUse(10, 100, 500) });
        Assert.Equal(
            ClusterFixture.Pod("ui") with { Cpu = ResourceUse.None },
            ClusterFixture.Pod("ui") with { Cpu = new ResourceUse(null, null, null, null) });
        Assert.NotEqual(ClusterFixture.Pod("ui"), ClusterFixture.Pod("ui") with { Cpu = ResourceUse.None });
    }

    /// <summary>
    /// The pod a file says and the pod a test builds are one pod, part for part: the sample's crashed ui pod reads
    /// as the thirteen parts written out here, which is what every reading of this type rests on.
    /// </summary>
    [Fact]
    public void ThePodAFileSaysIsThePodATestBuilds()
    {
        var fromTheFile = ClusterFixture.SampleStatus.Namespaces[2].Pods[0];

        Assert.Equal(One(), fromTheFile);
        Assert.Equal(One().GetHashCode(), fromTheFile.GetHashCode());
        Assert.NotSame(One(), fromTheFile);
    }

    // --------------------------------------------------------- the parts the pod falls apart into

    /// <summary>
    /// The pod falls apart into its thirteen parts, each as the value it stands on, in the order the pod names
    /// them - the resources as the very resources they are - and the parts the pod was not given come out the
    /// nothing they are, the restarts coming out the zero they are not.
    /// </summary>
    [Fact]
    public void APodFallsApartIntoItsThirteenParts()
    {
        var cpu = new ResourceUse(0, 100, 500);
        var pod = One() with { Cpu = cpu };

        var (name, workload, kind, node, phase, ready, containers, containersReady, restarts, reason, startedAt, podCpu, podMemory) = pod;

        Assert.Equal("ui-7f6b5c4d3-m8n2p", name);
        Assert.Equal("ui", workload);
        Assert.Equal("Deployment", kind);
        Assert.Equal("aks-system-12148983-vmss000000", node);
        Assert.Equal(ClusterPod.Running, phase);
        Assert.False(ready);
        Assert.Equal(1, containers);
        Assert.Equal(0, containersReady);
        Assert.Equal(7, restarts);
        Assert.Equal("CrashLoopBackOff", reason);
        Assert.Equal(Started, startedAt);
        Assert.Same(cpu, podCpu);
        Assert.Equal(new ResourceUse(21000000, 268435456, 536870912), podMemory);

        var (bareName, bareWorkload, bareKind, bareNode, barePhase, bareReady, bareContainers, bareContainersReady, bareRestarts, bareReason, bareStartedAt, bareCpu, bareMemory) =
            new ClusterPod("web-1", null, null, null, ClusterPod.Unknown, false, null, null, 0, null, null, ResourceUse.None, ResourceUse.None);

        Assert.Equal("web-1", bareName);
        Assert.Null(bareWorkload);
        Assert.Null(bareKind);
        Assert.Null(bareNode);
        Assert.Equal(ClusterPod.Unknown, barePhase);
        Assert.False(bareReady);
        Assert.Null(bareContainers);
        Assert.Null(bareContainersReady);
        Assert.Equal(0, bareRestarts);
        Assert.Null(bareReason);
        Assert.Null(bareStartedAt);
        Assert.Equal(ResourceUse.None, bareCpu);
        Assert.Equal(ResourceUse.None, bareMemory);
    }

    // ----------------------------------------------------------- the words the pod says its parts in

    /// <summary>
    /// The words the pod says its parts in: the record's own words, in the order the parts are named, the name at
    /// the front and the label the parts make at the end, each part said as the value it stands on - the nested
    /// resource as the words it says itself, the moment as the moment, the false as a false - and the nothing said
    /// as nothing, so that a reader comparing two of these lines can see which part moved.
    /// </summary>
    [Fact]
    public void APodSaysItsPartsInTheWordsTheRecordSaysThem()
    {
        Assert.Equal(
            "ClusterPod { Name = ui-7f6b5c4d3-m8n2p, Workload = ui, Kind = Deployment, Node = aks-system-12148983-vmss000000, "
            + "Phase = Running, Ready = False, Containers = 1, ContainersReady = 0, Restarts = 7, Reason = CrashLoopBackOff, "
            + "StartedAt = 10/06/2026 20:01:12 +00:00, "
            + "Cpu = ResourceUse { Usage = 0, Requests = 100, Limits = 500, Allocatable =  }, "
            + "Memory = ResourceUse { Usage = 21000000, Requests = 268435456, Limits = 536870912, Allocatable =  }, Label = ui }",
            Words(One()));

        Assert.Equal(
            "ClusterPod { Name = web-1, Workload = , Kind = , Node = , Phase = Unknown, Ready = False, Containers = , "
            + "ContainersReady = , Restarts = 0, Reason = , StartedAt = , "
            + "Cpu = ResourceUse { Usage = , Requests = , Limits = , Allocatable =  }, "
            + "Memory = ResourceUse { Usage = , Requests = , Limits = , Allocatable =  }, Label = web-1 }",
            Words(new ClusterPod("web-1", null, null, null, ClusterPod.Unknown, false, null, null, 0, null, null, ResourceUse.None, ResourceUse.None)));
    }

    /// <summary>
    /// The words say a nothing and an empty word the same way where the pod tells them apart: the reason that is
    /// nothing and the reason that is the empty word make one line and two pods - the line is the record's own
    /// shorthand, not the equality underneath it. Only a part the label is made of shows the difference in the line,
    /// and it shows it at the end, where the label falls back to the name for the nothing and stands the empty word
    /// for the empty word.
    /// </summary>
    [Fact]
    public void ANothingAndAnEmptyWordAreSaidTheSameWayByTwoPodsThatAreNotOne()
    {
        var noReason = ClusterFixture.Pod("ui") with { Reason = null };
        var emptyReason = ClusterFixture.Pod("ui") with { Reason = string.Empty };

        Assert.Equal(Words(noReason), Words(emptyReason));
        Assert.Contains("Reason = ,", Words(noReason), StringComparison.Ordinal);
        Assert.NotEqual(noReason, emptyReason);
        Assert.True(noReason != emptyReason);

        var noWorkload = ClusterFixture.Pod("ui") with { Workload = null };
        var emptyWorkload = ClusterFixture.Pod("ui") with { Workload = string.Empty };

        Assert.Contains("Workload = ,", Words(noWorkload), StringComparison.Ordinal);
        Assert.Contains("Workload = ,", Words(emptyWorkload), StringComparison.Ordinal);
        Assert.EndsWith("Label = ui-0 }", Words(noWorkload), StringComparison.Ordinal);
        Assert.EndsWith("Label =  }", Words(emptyWorkload), StringComparison.Ordinal);
        Assert.NotEqual(Words(noWorkload), Words(emptyWorkload));
    }

    // ------------------------------------------------------------------------------ the copies

    /// <summary>
    /// The copy of a pod that turns one of the parts it keeps keeps the other twelve: the name turned alone keeps
    /// the workload, the phase, the moment and the resources it does not turn, and the workload, the kind, the node,
    /// the phase, the readiness, the two container numbers, the reason, the started moment, the cpu and the memory
    /// each turned is turned alone - the parts a copy does not turn stand as they stood, and the copy is not the pod
    /// it is the copy of.
    /// </summary>
    [Fact]
    public void ACopyThatTurnsOneOfThePartsItKeepsKeepsTheRest()
    {
        var pod = One();

        var renamed = pod with { Name = "db-0" };
        Assert.Equal("db-0", renamed.Name);
        Assert.Equal(pod.Workload, renamed.Workload);
        Assert.Equal(pod.Kind, renamed.Kind);
        Assert.Equal(pod.Node, renamed.Node);
        Assert.Equal(pod.Phase, renamed.Phase);
        Assert.Equal(pod.Ready, renamed.Ready);
        Assert.Equal(pod.Containers, renamed.Containers);
        Assert.Equal(pod.ContainersReady, renamed.ContainersReady);
        Assert.Equal(pod.Restarts, renamed.Restarts);
        Assert.Equal(pod.Reason, renamed.Reason);
        Assert.Equal(pod.StartedAt, renamed.StartedAt);
        Assert.Same(pod.Cpu, renamed.Cpu);
        Assert.Same(pod.Memory, renamed.Memory);
        Assert.NotSame(pod, renamed);

        Assert.Equal(pod.Name, (pod with { Workload = null }).Name);
        Assert.Equal(pod.Name, (pod with { Kind = "Job" }).Name);
        Assert.Equal(pod.Name, (pod with { Node = null }).Name);
        Assert.Equal(pod.Name, (pod with { Phase = ClusterPod.Failed }).Name);
        Assert.Equal(pod.Name, (pod with { Ready = true }).Name);
        Assert.Equal(pod.Name, (pod with { Containers = null }).Name);
        Assert.Equal(pod.Name, (pod with { ContainersReady = 1 }).Name);
        Assert.Equal(pod.Name, (pod with { Restarts = 0 }).Name);
        Assert.Equal(pod.Name, (pod with { Reason = null }).Name);
        Assert.Equal(pod.Name, (pod with { StartedAt = null }).Name);
        Assert.Equal(pod.Name, (pod with { Cpu = ResourceUse.None }).Name);
        Assert.Equal(pod.Name, (pod with { Memory = new ResourceUse(1, 2, 3) }).Name);

        // A copy that turns a part to what it already was is the pod again in everything but its reference.
        Assert.Equal(pod, pod with { Phase = ClusterPod.Running, Containers = 1, Restarts = 7, StartedAt = Started });
        Assert.NotSame(pod, pod with { Phase = ClusterPod.Running });
    }

    // ------------------------------------------------------ the pods that are one and the pods that are not

    /// <summary>
    /// The pod asked of as an object is the one its parts make it: itself, and another pod built the same way from
    /// words of its own, are it - and the nothing it is not and the word it is not are not it, and two pods that are
    /// one are never the one reference.
    /// </summary>
    [Fact]
    public void APodAskedOfAsAnObjectIsTheOneItsPartsMakeIt()
    {
        var one = One();
        var two = One();

        Assert.True(one.Equals(one));
        Assert.True(one.Equals(two));
        Assert.True(one.Equals(two as object));
        Assert.True(one == two);
        Assert.False(one != two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.NotSame(one, two);
        Assert.False(one.Equals(null));
        Assert.False(one.Equals("ui-7f6b5c4d3-m8n2p"));
        Assert.False(one.Equals(ClusterFixture.Node()));
        Assert.False(one.Equals(ClusterFixture.Pod("ui")));
    }

    /// <summary>
    /// Each of the thirteen parts on its own is enough to turn two pods apart: the name, the workload, the kind, the
    /// node, the phase, the readiness, the two container numbers, the restarts, the reason, the started moment, the
    /// cpu and the memory - each of them turned at the nothing, the empty word or another value of its own - and the
    /// label the parts make is no part to turn, only a word that moves with the workload and the name.
    /// </summary>
    [Fact]
    public void EachOfTheThirteenPartsTurnsTwoPodsApartOnItsOwn()
    {
        var one = One();

        ClusterPod[] different =
        [
            one with { Name = "ui-7f6b5c4d3-m8n2q" },
            one with { Name = string.Empty },
            one with { Workload = null },
            one with { Workload = string.Empty },
            one with { Workload = "db" },
            one with { Kind = null },
            one with { Kind = "StatefulSet" },
            one with { Node = null },
            one with { Node = "aks-system-12148983-vmss000001" },
            one with { Phase = string.Empty },
            one with { Phase = ClusterPod.Pending },
            one with { Ready = true },
            one with { Containers = null },
            one with { Containers = 0 },
            one with { ContainersReady = null },
            one with { ContainersReady = 1 },
            one with { Restarts = 6 },
            one with { Reason = null },
            one with { Reason = string.Empty },
            one with { Reason = "ImagePullBackOff" },
            one with { StartedAt = null },
            one with { StartedAt = Started.AddSeconds(1) },
            one with { Cpu = ResourceUse.None },
            one with { Cpu = new ResourceUse(1, 100, 500) },
            one with { Memory = new ResourceUse(0, 0, 0) },
        ];

        foreach (var other in different)
        {
            Assert.NotEqual(one, other);
            Assert.False(one.Equals(other));
            Assert.True(one != other);
            Assert.False(one == other);
        }

        // The empty word and the nothing are two words, whatever the line the record says them in.
        Assert.NotEqual(one with { Workload = null }, one with { Workload = string.Empty });
        Assert.NotEqual(one with { Reason = null }, one with { Reason = string.Empty });

        // The label moves with the two parts it is made of and with nothing else.
        Assert.Equal("ui", one.Label);
        Assert.Equal(string.Empty, (one with { Workload = string.Empty }).Label);
        Assert.Equal("db", (one with { Workload = "db" }).Label);
        Assert.Equal(one.Label, (one with { Phase = ClusterPod.Failed, Reason = null, StartedAt = null }).Label);
    }

    // --------------------------------------------------------------------- the shared words

    /// <summary>A pod whose resource parts are the nothing it was given says the one word the all-nothing use says:
    /// the constant that every edge-case file names instead of repeating the string twice. The test builds a pod
    /// with no resource parts said and checks that its words carry the constant where the resource parts stand.</summary>
    [Fact]
    public void APodWithNoResourcePartsSaidSaysTheEmptyResourceUseConstant()
    {
        var pod = new ClusterPod("web-1", null, null, null, ClusterPod.Unknown, false, null, null, 0, null, null, ResourceUse.None, ResourceUse.None);

        Assert.Contains($"Cpu = {RecordWords.EmptyResourceUse}", Words(pod), StringComparison.Ordinal);
        Assert.Contains($"Memory = {RecordWords.EmptyResourceUse}", Words(pod), StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------------------------- the machinery

    /// <summary>The public members the pod declares itself: its properties and the methods that are not the get and
    /// the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(ClusterPod)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words a pod says its parts in, taken in the invariant culture whatever the reader of this test
    /// happens to be: the parts themselves outlast any reader, which is pinned on its own further up this file, so
    /// the strings there are the point and the culture only the way to say them once.</summary>
    private static string Words(ClusterPod pod)
    {
        ArgumentNullException.ThrowIfNull(pod);

        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            return pod.ToString();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }
}
