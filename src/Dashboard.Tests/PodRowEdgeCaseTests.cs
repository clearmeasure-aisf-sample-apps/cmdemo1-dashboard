namespace Dashboard.Tests;

/// <summary>
/// A <see cref="PodRow"/> is the row of a namespace's table: the pod as the file holds it and the state the rule
/// has landed it on, and nothing else. The row's parts are pinned where the page shows them -
/// <see cref="NamespaceGroupTests"/> and <see cref="ClusterGroupsTests"/> pin the state a row is worth in a
/// namespace's counts, its order in the table and its sums, and <see cref="PodStateEdgeCaseTests"/> the state it
/// carries - and what they leave open is pinned here: the row keeping the very pod and the very state it is given,
/// the pod as the row holds it when the pod says little, the row that is not there, the row of the pod that is not
/// there and its words, the two rows that are one while the pod and the state are, the row's words in the order the
/// record says them, the row falling apart into its parts and coming back together as a copy, the copy of the row
/// of the pod that is not there, the copy that names the pod as not there and its words, and the ends of the state
/// and of the restart count as the row holds them.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every pod is one the test builds, and the
/// moment it says it started is the fixed moment the test's fixture stands on.
/// </para>
/// </summary>
public class PodRowEdgeCaseTests
{
    /// <summary>The whole word the row of a silent pod says up to its state: every part of the pod said as the record
    /// says it, in the order the record declares them, and the separator that goes before the state. The bytes are
    /// the pinned bytes the two literal pins of <see cref="TheRowSaysItsPodAndItsStateInTheirOrder"/> hold verbatim,
    /// so the state alone is what a pin of the silent row has to name.</summary>
    private const string SilentPodWords =
        "PodRow { Pod = ClusterPod { Name = , Workload = , Kind = , Node = , Phase = Running, "
        + "Ready = False, Containers = , ContainersReady = , Restarts = 0, Reason = , StartedAt = , "
        + "Cpu = ResourceUse { Usage = , Requests = , Limits = , Allocatable =  }, "
        + "Memory = ResourceUse { Usage = , Requests = , Limits = , Allocatable =  }, Label =  }, ";

    // ----- the parts the row holds

    /// <summary>The row keeps the very pod and the very state it is given: the pod is the instance the row was
    /// built with, the same reference and not an equal one, and the state is the one it was told - each of the four
    /// the page knows, and the fixed facts the fixture pod says are said by the row as well.</summary>
    [Fact]
    public void TheRowKeepsTheVeryPodAndTheVeryStateItIsGiven()
    {
        var pod = ClusterFixture.Pod("ui", restarts: 3);

        Assert.Same(pod, new PodRow(pod, PodState.Ready).Pod);

        foreach (var state in new[] { PodState.Ready, PodState.Starting, PodState.Unhealthy, PodState.Finished })
        {
            Assert.Equal(state, new PodRow(pod, state).State);
        }

        var row = new PodRow(pod, PodState.Starting);
        Assert.Equal("ui-0", row.Pod.Name);
        Assert.Equal("ui", row.Pod.Label);
        Assert.Equal(ClusterPod.Running, row.Pod.Phase);
        Assert.True(row.Pod.Ready);
        Assert.Equal(3, row.Pod.Restarts);
        Assert.Equal(new ResourceUse(10, 100, 500), row.Pod.Cpu);
        Assert.Equal(new ResourceUse(100 * 1024 * 1024, 268435456, 536870912), row.Pod.Memory);
    }

    /// <summary>The pod's own words are the row's words, whatever the pod says: a pod without a workload is the
    /// row's pod without a workload, and its label is its own name; a pod that calls itself nothing, that names no
    /// phase and no reason, and that says nothing about its start or its containers is none of that in the row, and
    /// a pod that says nothing about what it asks for asks for nothing in the row.</summary>
    [Fact]
    public void APodThatSaysLittleIsLittleInTheRowToo()
    {
        var solo = ClusterFixture.Pod("solo") with { Workload = null };
        Assert.Equal("solo-0", new PodRow(solo, PodState.Ready).Pod.Label);

        var row = new PodRow(ClusterFixture.SilentPod("") with { Phase = "", Reason = "" }, PodState.Unhealthy);

        Assert.Equal("", row.Pod.Name);
        Assert.Null(row.Pod.Workload);
        Assert.Null(row.Pod.Kind);
        Assert.Null(row.Pod.Node);
        Assert.Equal("", row.Pod.Phase);
        Assert.False(row.Pod.Ready);
        Assert.Null(row.Pod.Containers);
        Assert.Null(row.Pod.ContainersReady);
        Assert.Equal(0, row.Pod.Restarts);
        Assert.Equal("", row.Pod.Reason);
        Assert.Null(row.Pod.StartedAt);
        Assert.Equal(ResourceUse.None, row.Pod.Cpu);
        Assert.Equal(ResourceUse.None, row.Pod.Memory);
    }

    // ----- the row that is not there

    /// <summary>A row that is not there is nothing: the record's default is no row, and a row is never equal to no
    /// row however the question is asked - the operators, and <c>Equals</c> asked about the no-row - while the row
    /// that is there is a row to both of them.</summary>
    [Fact]
    public void ARowThatIsNotThereIsNoRow()
    {
        Assert.Null(default(PodRow));

        PodRow? none = null;
        var there = new PodRow(ClusterFixture.SilentPod("ui-0"), PodState.Ready);

        Assert.False(none == there);
        Assert.True(none != there);
        Assert.False(there.Equals(none));
        Assert.False(there == none);
        Assert.True(there != none);
        Assert.True(there is not null);
    }

    // ----- the pod that is not there

    /// <summary>The row takes the pod it is given on trust and keeps it as it stands: a row of no pod is not
    /// refused and not a row that is starting - it is a row whose pod is not there, the state it was told kept as
    /// given, and two such rows are one while the state is, as a row with a pod is none of them.</summary>
    [Fact]
    public void ARowOfNoPodKeepsNoPodAndTheStateItWasTold()
    {
        var row = new PodRow(null!, PodState.Ready);

        Assert.Null(row.Pod);
        Assert.Equal(PodState.Ready, row.State);
        Assert.Equal(row, new PodRow(null!, PodState.Ready));
        Assert.True(row == new PodRow(null!, PodState.Ready));
        Assert.Equal(row.GetHashCode(), new PodRow(null!, PodState.Ready).GetHashCode());
        Assert.False(row.Equals(null));
        Assert.True(row != new PodRow(null!, PodState.Starting));
        Assert.True(row != new PodRow(ClusterFixture.SilentPod("ui-0"), PodState.Ready));
    }

    /// <summary>The words of the row of the pod that is not there are the row's own: the pod not there is said as the
    /// empty word and the state is said by the name it gives, in the order the record declares them.</summary>
    [Fact]
    public void TheRowOfNoPodSaysItsWords()
    {
        Assert.Equal("PodRow { Pod = , State = Unhealthy }", new PodRow(null!, PodState.Unhealthy).ToString());
    }

    // ----- the row and its equal

    /// <summary>Two rows are one while the pod and the state are - the pod's own equality, which is the equality of
    /// its parts, and the state's - and a difference in either, the state turned or the pod in any one of its parts
    /// changed, is another one. The hash code follows the equality, and neither no row nor a word is a row's
    /// equal.</summary>
    [Fact]
    public void TwoRowsAreOneWhileThePodAndTheStateAre()
    {
        var one = new PodRow(ClusterFixture.Pod("ui"), PodState.Ready);
        var two = new PodRow(ClusterFixture.Pod("ui"), PodState.Ready);

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.True(one.Equals((object)two));
        Assert.True(one.Equals((object)one));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.False(one.Equals("ui-0"));

        Assert.NotEqual(one, new PodRow(one.Pod, PodState.Starting));
        Assert.NotEqual(one, new PodRow(one.Pod with { Name = "other-0" }, one.State));
        Assert.NotEqual(one, new PodRow(one.Pod with { Restarts = 1 }, one.State));
        Assert.NotEqual(one, new PodRow(one.Pod with { Cpu = ResourceUse.None }, one.State));
        Assert.Equal(one, new PodRow(one.Pod with { Name = one.Pod.Name, Workload = one.Pod.Workload }, one.State));
    }

    // ----- the row's own words

    /// <summary>The row says its parts in the order the record declares them - the pod first and the state after,
    /// each by the word the type says: the pod's own words inside the row's, the parts the pod does not have said
    /// as the empty word, the pod's label the words it says, and the state's name when the state is one the enum
    /// names and its number when it is not.</summary>
    [Fact]
    public void TheRowSaysItsPodAndItsStateInTheirOrder()
    {
        Assert.Equal(
            "PodRow { Pod = ClusterPod { Name = , Workload = , Kind = , Node = , Phase = Running, " +
                "Ready = False, Containers = , ContainersReady = , Restarts = 0, Reason = , StartedAt = , " +
                "Cpu = ResourceUse { Usage = , Requests = , Limits = , Allocatable =  }, " +
                "Memory = ResourceUse { Usage = , Requests = , Limits = , Allocatable =  }, Label =  }, " +
                "State = Finished }",
            new PodRow(ClusterFixture.SilentPod(""), PodState.Finished).ToString());

        Assert.Equal(
            "PodRow { Pod = ClusterPod { Name = ui-0, Workload = ui, Kind = Deployment, Node = " +
                "aks-system-12148983-vmss000000, Phase = Running, Ready = True, Containers = 1, " +
                "ContainersReady = 1, Restarts = 0, Reason = , StartedAt = , " +
                "Cpu = ResourceUse { Usage = 10, Requests = 100, Limits = 500, Allocatable =  }, " +
                "Memory = ResourceUse { Usage = 104857600, Requests = 268435456, Limits = 536870912, " +
                "Allocatable =  }, Label = ui }, State = Unhealthy }",
            new PodRow(ClusterFixture.Pod("ui").StartedAt(null), PodState.Unhealthy).ToString());

        Assert.Equal(
            "PodRow { Pod = ClusterPod { Name = , Workload = , Kind = , Node = , Phase = Running, " +
                "Ready = False, Containers = , ContainersReady = , Restarts = 0, Reason = , StartedAt = , " +
                "Cpu = ResourceUse { Usage = , Requests = , Limits = , Allocatable =  }, " +
                "Memory = ResourceUse { Usage = , Requests = , Limits = , Allocatable =  }, Label =  }, " +
                "State = 4 }",
            new PodRow(ClusterFixture.SilentPod(""), (PodState)4).ToString());
    }

    /// <summary>The shared block of the silent row and the state alone make the row's words: the same pinned bytes the
    /// literal pins of <see cref="TheRowSaysItsPodAndItsStateInTheirOrder"/> hold, said once as the block and again
    /// with the state named - the word the enum names and the number the enum does not - so a state that moved shows
    /// as the one difference and the block is not copied a third time as the file grows.</summary>
    [Fact]
    public void TheSilentRowSaysTheSharedBlockAndTheStateItWasGiven()
    {
        Assert.Equal(SilentPodWords + "State = Finished }", new PodRow(ClusterFixture.SilentPod(""), PodState.Finished).ToString());
        Assert.Equal(SilentPodWords + "State = 4 }", new PodRow(ClusterFixture.SilentPod(""), (PodState)4).ToString());
    }

    /// <summary>The shared block stops where the state begins: it says no state of its own, opens with the row's own
    /// word and ends on the separator the record puts between two parts - so a pin that interpolates a state says the
    /// one it names and not a state the block carried along unnoticed.</summary>
    [Fact]
    public void TheSharedBlockOfTheSilentRowSaysNoStateOfItsOwn()
    {
        Assert.DoesNotContain("State =", SilentPodWords, StringComparison.Ordinal);
        Assert.StartsWith("PodRow { Pod = ClusterPod { ", SilentPodWords, StringComparison.Ordinal);
        Assert.EndsWith(", ", SilentPodWords, StringComparison.Ordinal);
    }

    /// <summary>The silent pod's two resource parts say the one word the all-nothing use says: the constant that
    /// every edge-case file names instead of repeating the string twice. The file's own block is built from the
    /// constant, so the bytes the block says are the bytes the constant says - a change to the record's print
    /// shape moves the constant and the block moves with it.</summary>
    [Fact]
    public void TheSilentPodWordsContainTheEmptyResourceUseConstantTwice()
    {
        var cpuPart = $"Cpu = {RecordWords.EmptyResourceUse}, ";
        var memoryPart = $"Memory = {RecordWords.EmptyResourceUse}, Label =  ";

        Assert.Contains(cpuPart, SilentPodWords, StringComparison.Ordinal);
        Assert.Contains(memoryPart, SilentPodWords, StringComparison.Ordinal);
    }

    /// <summary>A row whose pod is not there still says it is a row: the pod is said as the empty word and the
    /// state is said as the row holds it - by the name it gives when the enum names it and by its number when it
    /// does not.</summary>
    [Fact]
    public void TheRowOfNoPodSaysItsWordsWithThePodSaidAsNothing()
    {
        Assert.Equal("PodRow { Pod = , State = Ready }", new PodRow(null!, PodState.Ready).ToString());
        Assert.Equal("PodRow { Pod = , State = 4 }", new PodRow(null!, (PodState)4).ToString());
    }

    // ----- the row falls apart and is copied

    /// <summary>The row falls apart into the pod and the state it holds - the pod the same instance the row was
    /// built with - and a copy of the row is the row again: the <c>with</c> that names no part and the <c>with</c>
    /// that names the part it turns keeps the rest, the same pod inside, and not the same reference.</summary>
    [Fact]
    public void TheRowFallsApartIntoItsPartsAndACopyIsTheRowAgain()
    {
        var pod = ClusterFixture.Pod("ui");
        var row = new PodRow(pod, PodState.Ready);

        var (partsPod, partsState) = row;
        Assert.Same(pod, partsPod);
        Assert.Equal(PodState.Ready, partsState);

        var copy = row with { };
        Assert.Equal(row, copy);
        Assert.NotSame(row, copy);
        Assert.Same(row.Pod, copy.Pod);
        Assert.Equal(row.State, copy.State);

        var unchanged = row with { Pod = row.Pod };
        Assert.Equal(row, unchanged);
        Assert.NotSame(row, unchanged);

        var turned = row with { State = PodState.Unhealthy };
        Assert.Equal(PodState.Unhealthy, turned.State);
        Assert.Same(row.Pod, turned.Pod);
        Assert.NotEqual(row, turned);

        var repack = row with { Pod = row.Pod, State = row.State };
        Assert.Equal(row, repack);
        Assert.NotSame(row, repack);
    }

    /// <summary>The copy of the row of the pod that is not there is the row again: the <c>with</c> that names no part
    /// keeps the state the row was told, the pod is still not there in the copy, and the copy is not the same
    /// reference, is the row's equal, and stands on the row's hash code.</summary>
    [Fact]
    public void TheCopyOfTheRowOfNoPodIsTheRowAgain()
    {
        var row = new PodRow(null!, PodState.Ready);
        var copy = row with {};

        Assert.NotSame(row, copy);
        Assert.Equal(row, copy);
        Assert.Null(copy.Pod);
        Assert.Equal(row.State, copy.State);
        Assert.Equal(row.GetHashCode(), copy.GetHashCode());
    }

    /// <summary>The copy that names the pod as not there is the row of the pod that is not there: the <c>with</c>
    /// that names the pod no pod leaves the state as told and not the same reference, stands on the no-pod row's
    /// equality and hash code, and says the record's words with the pod as the empty word.</summary>
    [Fact]
    public void TheCopyThatNamesThePodNotThereIsTheRowOfNoPod()
    {
        var row = new PodRow(ClusterFixture.Pod("ui"), PodState.Ready);
        var copy = row with { Pod = null! };

        Assert.NotSame(row, copy);
        Assert.Null(copy.Pod);
        Assert.Equal(row.State, copy.State);

        var noPod = new PodRow(null!, row.State);
        Assert.True(copy == noPod);
        Assert.False(copy != noPod);
        Assert.Equal(noPod, copy);
        Assert.Equal(copy.GetHashCode(), noPod.GetHashCode());
        Assert.False(copy == row);
        Assert.Equal("PodRow { Pod = , State = Ready }", copy.ToString());
    }

    /// <summary>The copy that names the pod as not there is the no-pod row of the row's state, and the row stands
    /// unchanged: the <c>with</c> that names the pod no pod leaves the state as told and the pod not there in the
    /// copy, the copy is the no-pod row of that state - the <c>==</c> and <c>Equals</c> say so and the hash code
    /// stands on it - it is not the no-pod row of another state and not the row itself, and the row keeps the very
    /// pod it was built with, the same reference.</summary>
    [Fact]
    public void TheCopyThatNamesThePodNotThereKeepsTheRowUnchanged()
    {
        var pod = ClusterFixture.Pod("ui");
        var row = new PodRow(pod, PodState.Ready);
        var copy = row with { Pod = null! };

        Assert.Null(copy.Pod);
        Assert.Equal(row.State, copy.State);

        var noPod = new PodRow(null!, row.State);
        Assert.True(copy == noPod);
        Assert.True(copy.Equals((object)noPod));
        Assert.Equal(copy.GetHashCode(), noPod.GetHashCode());

        Assert.NotEqual(copy, new PodRow(null!, PodState.Starting));
        Assert.False(copy == row);

        Assert.Same(pod, row.Pod);
    }

    // ----- the ends

    /// <summary>The state the row carries is whatever number it was given: the four the enum names the row holds
    /// as states, and the numbers the enum does not name - the one before the first, the one after the last, and
    /// the two ends of the count - the row holds as numbers it keeps apart as the numbers are apart, a row of the
    /// same nameless number being the row's equal while the pod is.</summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void AStateTheEnumDoesNotNameIsHeldAsTheNumberItStandsOn(int number)
    {
        var row = new PodRow(ClusterFixture.SilentPod("ui-0"), (PodState)number);

        Assert.Equal(number, (int)row.State);
        Assert.False(Enum.IsDefined(row.State));
        Assert.Equal(row, new PodRow(ClusterFixture.SilentPod("ui-0"), (PodState)number));
        Assert.True(row == new PodRow(ClusterFixture.SilentPod("ui-0"), (PodState)number));
    }

    /// <summary>The restart count the pod says is the row's to hold: the zero the pod that says nothing stands on,
    /// and the two ends of the count between which no pod of the cluster has ever stood - the row keeps them as
    /// the counts are apart.</summary>
    [Fact]
    public void TheRestartCountStandsInTheRowAtTheEndsOfTheCount()
    {
        var zero = new PodRow(ClusterFixture.SilentPod("ui-0"), PodState.Ready);
        var bottom = new PodRow(ClusterFixture.SilentPod("ui-0") with { Restarts = int.MinValue }, PodState.Ready);
        var top = new PodRow(ClusterFixture.SilentPod("ui-0") with { Restarts = int.MaxValue }, PodState.Ready);

        Assert.Equal(0, zero.Pod.Restarts);
        Assert.Equal(int.MinValue, bottom.Pod.Restarts);
        Assert.Equal(int.MaxValue, top.Pod.Restarts);
        Assert.NotEqual(zero, bottom);
        Assert.NotEqual(zero, top);
        Assert.NotEqual(bottom, top);
    }

    /// <summary>The moment the pod says it started is the row's to hold as it stands: a start at either end of the
    /// clock stands in the row as that end, the row never asked how old the pod is.</summary>
    [Fact]
    public void AStartAtEitherEndOfTheClockStandsInTheRow()
    {
        foreach (var at in new[] { DateTimeOffset.MinValue, DateTimeOffset.MaxValue })
        {
            var row = new PodRow(ClusterFixture.SilentPod("ui-0") with { StartedAt = at }, PodState.Ready);

            Assert.Equal(at, row.Pod.StartedAt);
        }
    }
}
