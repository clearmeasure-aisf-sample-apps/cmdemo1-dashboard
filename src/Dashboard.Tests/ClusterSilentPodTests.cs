namespace Dashboard.Tests;

/// <summary>
/// <see cref="ClusterFixture.SilentPod(string, string)" /> is the one home of the pod that says as little as the
/// record allows - the all-nothing pod <see cref="PodRowEdgeCaseTests" /> has been building for its rows and
/// <see cref="ClusterPodEdgeCaseTests" /> has been building again, thirteen arguments at a time, with nothing to say
/// which argument is which. The two are apart in one part only, and that part is the one the factory hands its
/// callers: the row of a silent pod stands on <see cref="ClusterPod.Running" />, while the pod a file barely names
/// stands on the parser's fallback, <see cref="ClusterPod.Unknown" /> - a word two of the strings pinned below
/// carry, so it is said out loud rather than defaulted under a caller.
/// <para>
/// Pinned here: the thirteen parts as the nothing they are and the one name the pod brings, the phase as the word the
/// caller stands on, the label falling back to that name, the two resources as the one shared resource that says no
/// number, the two all-nothing pods of the two files as the very pods this factory makes - byte for byte, part for
/// part, so the day either file calls this instead of writing its thirteen out again nothing moves - and the words
/// the record says both of them in, at the phase each stands on.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: the pod is built, never read, and says no
/// moment at all.
/// </para>
/// </summary>
public class ClusterSilentPodTests
{
    /// <summary>The silent pod's thirteen parts, at the phase the row of a pod stands on: the name it was given and
    /// nothing else - no workload, no kind, no node, the readiness the false that is no nothing, both container
    /// counts the nothing that is no zero, the restarts the zero that is no nothing, no reason, no start, and both
    /// resources the resource that says no number at all.</summary>
    [Fact]
    public void ASilentPodBringsItsNameAndNothingElse()
    {
        var pod = ClusterFixture.SilentPod("ui-0");

        Assert.Equal("ui-0", pod.Name);
        Assert.Null(pod.Workload);
        Assert.Null(pod.Kind);
        Assert.Null(pod.Node);
        Assert.Equal(ClusterPod.Running, pod.Phase);
        Assert.False(pod.Ready);
        Assert.Null(pod.Containers);
        Assert.Null(pod.ContainersReady);
        Assert.Equal(0, pod.Restarts);
        Assert.Null(pod.Reason);
        Assert.Null(pod.StartedAt);
        Assert.Equal(ResourceUse.None, pod.Cpu);
        Assert.Equal(ResourceUse.None, pod.Memory);
    }

    /// <summary>The name is the only part the pod brings, and the page calls the pod by it: with no workload behind
    /// it the label falls back to the name, and the two silent pods of two names are two pods - as apart as their
    /// names, in the parts, in the label, in the words and in the hash code.</summary>
    [Fact]
    public void TheSilentPodIsItsNameAndTwoNamesAreTwoPods()
    {
        var one = ClusterFixture.SilentPod("ui-0");
        var two = ClusterFixture.SilentPod("db-0");

        Assert.Equal("ui-0", one.Label);
        Assert.Equal("db-0", two.Label);

        Assert.NotEqual(one, two);
        Assert.True(one != two);
        Assert.NotEqual(Words(one), Words(two));
        Assert.NotEqual(one.GetHashCode(), two.GetHashCode());
    }

    /// <summary>The phase is the one part a caller of the all-nothing pod names, and it is named: the default is the
    /// word the row of a pod stands on, every one of the five words the record knows stands where it is put, and a
    /// word of a phase the record does not know is kept as the word it is - the pod is not the parser.</summary>
    [Fact]
    public void TheDefaultPhaseOfASilentPodIsTheWordTheRowStandsOn() =>
        Assert.Equal(ClusterPod.Running, ClusterFixture.SilentPod("ui-0").Phase);

    [Theory]
    [InlineData(ClusterPod.Pending)]
    [InlineData(ClusterPod.Running)]
    [InlineData(ClusterPod.Succeeded)]
    [InlineData(ClusterPod.Failed)]
    [InlineData(ClusterPod.Unknown)]
    [InlineData("ContainerCreating")]
    [InlineData("")]
    public void ASilentPodStandsAtThePhaseItsCallerStandsOn(string phase)
    {
        var pod = ClusterFixture.SilentPod("web-1", phase);

        Assert.Equal(phase, pod.Phase);
        Assert.Equal("web-1", pod.Name);
        Assert.Null(pod.Workload);
        Assert.Null(pod.Kind);
        Assert.Null(pod.Node);
        Assert.False(pod.Ready);
        Assert.Null(pod.Containers);
        Assert.Null(pod.ContainersReady);
        Assert.Equal(0, pod.Restarts);
        Assert.Null(pod.Reason);
        Assert.Null(pod.StartedAt);
        Assert.Equal(ResourceUse.None, pod.Cpu);
        Assert.Equal(ResourceUse.None, pod.Memory);
    }

    /// <summary>The two all-nothing pods the two files build today are the pods this factory makes: each written out
    /// at the call as the thirteen positional arguments it has been written as, and the factory's pod one with it -
    /// parts, words and hash code. This is the pin that says the lift moves nothing.</summary>
    [Fact]
    public void TheSilentPodIsTheAllNothingPodBothFilesAlreadyBuild()
    {
        var asTheRowBuildsIt =
            new ClusterPod("ui-0", null, null, null, ClusterPod.Running, false, null, null, 0, null, null, ResourceUse.None, ResourceUse.None);
        var asThePodFileBuildsIt =
            new ClusterPod("web-1", null, null, null, ClusterPod.Unknown, false, null, null, 0, null, null, ResourceUse.None, ResourceUse.None);

        Assert.Equal(asTheRowBuildsIt, ClusterFixture.SilentPod("ui-0"));
        Assert.Equal(asTheRowBuildsIt.GetHashCode(), ClusterFixture.SilentPod("ui-0").GetHashCode());
        Assert.Equal(Words(asTheRowBuildsIt), Words(ClusterFixture.SilentPod("ui-0")));

        Assert.Equal(asThePodFileBuildsIt, ClusterFixture.SilentPod("web-1", ClusterPod.Unknown));
        Assert.Equal(asThePodFileBuildsIt.GetHashCode(), ClusterFixture.SilentPod("web-1", ClusterPod.Unknown).GetHashCode());
        Assert.Equal(Words(asThePodFileBuildsIt), Words(ClusterFixture.SilentPod("web-1", ClusterPod.Unknown)));
    }

    /// <summary>The words of the two silent pods, pinned as the bytes the two files already pin them at: the pod a
    /// file barely names, at the parser's own word, is the line <see cref="ClusterPodEdgeCaseTests" /> pins, and
    /// the pod a row holds is the line <see cref="PodRowEdgeCaseTests" /> pins inside its row's words.</summary>
    [Fact]
    public void ASilentPodSaysItsPartsInTheWordsTheRecordSaysThem()
    {
        Assert.Equal(
            "ClusterPod { Name = web-1, Workload = , Kind = , Node = , Phase = Unknown, Ready = False, Containers = , "
            + "ContainersReady = , Restarts = 0, Reason = , StartedAt = , "
            + "Cpu = ResourceUse { Usage = , Requests = , Limits = , Allocatable =  }, "
            + "Memory = ResourceUse { Usage = , Requests = , Limits = , Allocatable =  }, Label = web-1 }",
            Words(ClusterFixture.SilentPod("web-1", ClusterPod.Unknown)));

        Assert.Equal(
            "PodRow { Pod = ClusterPod { Name = , Workload = , Kind = , Node = , Phase = Running, " +
                "Ready = False, Containers = , ContainersReady = , Restarts = 0, Reason = , StartedAt = , " +
                "Cpu = ResourceUse { Usage = , Requests = , Limits = , Allocatable =  }, " +
                "Memory = ResourceUse { Usage = , Requests = , Limits = , Allocatable =  }, Label =  }, " +
                "State = Finished }",
            new PodRow(ClusterFixture.SilentPod(""), PodState.Finished).ToString());

        Assert.Equal(
            "PodRow { Pod = ClusterPod { Name = , Workload = , Kind = , Node = , Phase = Running, " +
                "Ready = False, Containers = , ContainersReady = , Restarts = 0, Reason = , StartedAt = , " +
                "Cpu = ResourceUse { Usage = , Requests = , Limits = , Allocatable =  }, " +
                "Memory = ResourceUse { Usage = , Requests = , Limits = , Allocatable =  }, Label =  }, " +
                "State = 4 }",
            new PodRow(ClusterFixture.SilentPod(""), (PodState)4).ToString());
    }

    /// <summary>The cpu and the memory of a silent pod are the one shared resource that says no number - which is
    /// exactly why the factory names them: two pods that swapped the two would be one pod, and only the word at the
    /// call says which is which. A copy that turns one of them leaves the other the shared nothing.</summary>
    [Fact]
    public void TheTwoResourcesOfASilentPodAreTheSharedNothingAndEachTurnsAlone()
    {
        var pod = ClusterFixture.SilentPod("ui-0");

        Assert.Same(ResourceUse.None, pod.Cpu);
        Assert.Same(ResourceUse.None, pod.Memory);

        var cpu = pod with { Cpu = new ResourceUse(10, 100, 500) };
        Assert.Equal(new ResourceUse(10, 100, 500), cpu.Cpu);
        Assert.Same(ResourceUse.None, cpu.Memory);
        Assert.NotEqual(pod, cpu);

        var memory = pod with { Memory = new ResourceUse(1, 2, 3) };
        Assert.Same(ResourceUse.None, memory.Cpu);
        Assert.Equal(new ResourceUse(1, 2, 3), memory.Memory);
        Assert.NotEqual(pod, memory);
        Assert.NotEqual(cpu, memory);
    }

    /// <summary>A silent pod copied is the silent pod again - the <c>with</c> that names no part, and the
    /// <c>with</c> that names a part it leaves as it was - and a silent pod that is given a workload is no longer
    /// silent in the one way the page reads: the label stops falling back to the name and says the workload.</summary>
    [Fact]
    public void ACopyOfASilentPodIsASilentPodUnlessItNamesAWorkload()
    {
        var pod = ClusterFixture.SilentPod("ui-0");

        var copy = pod with { };
        Assert.Equal(pod, copy);
        Assert.NotSame(pod, copy);

        var kept = pod with { Name = pod.Name, Phase = pod.Phase, Restarts = pod.Restarts };
        Assert.Equal(pod, kept);

        var owned = pod with { Workload = "ui" };
        Assert.Equal("ui", owned.Label);
        Assert.Equal("ui-0", owned.Name);
        Assert.NotEqual(pod, owned);
    }

    /// <summary>The words a pod says its parts in, taken in the invariant culture whatever the reader of this test
    /// happens to be: a silent pod says no number that a reader could say differently, but the line it says is
    /// pinned against the lines of the two files it lifts, and those are read once, the same way, everywhere.</summary>
    private static string Words(ClusterPod pod) => CultureScope.Words(pod);
}
