namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of the groups the cluster's view keeps - the types of
/// <c>ClusterGroups.cs</c>: the <see cref="PodRow"/> of a namespace's table, the <see cref="NamespaceGroup"/> of
/// one namespace with its sums, the <see cref="ClusterGroups"/> the page shows and its <c>Of</c> that sorts the
/// namespaces of a status into the environments' groups and the platform's, the <see cref="ClusterTotals"/> of the
/// cluster card and its <c>Of</c>, and the <see cref="ClusterSample"/> the page keeps of a reading for its trends
/// and its <c>Of</c>. The behaviour the page shows is pinned where it belongs - <see cref="ClusterGroupsTests"/>
/// and <see cref="NamespaceGroupTests"/> for the order of the groups, the states and sums of a namespace's pods
/// and the platform's need for a look, and <see cref="PodRowEdgeCaseTests"/> for the row of the table and the pod
/// and state it holds - and what they leave open is pinned here: each record keeping the very parts it is given,
/// the two records that are one while their parts are and the one a difference in any part makes, the record's
/// words in the order it declares its parts, the <c>Of</c> that is given no status or no environments, the status
/// that lists nothing, the environment whose namespace differs from the status' in case alone and the one whose
/// namespace is the empty string, the two environments on one namespace, the status that lists one namespace
/// twice, and the sample and the totals a status without nodes stands on.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every status is one the test builds, and the
/// moment the facts are of is the fixed moment the test's fixture stands on.
/// </para>
/// </summary>
public class ClusterGroupsEdgeCaseTests
{
    private static readonly DateTimeOffset Generated = ClusterFixture.Generated;

    // ----- the parts each record keeps

    /// <summary>The group keeps the very five parts it is given: the name, the environment as the same instance,
    /// the found as the one it was told, the pods and the volumes as the very lists it was handed - and the group
    /// with the empty name and no environment and no pods keeps those as the nothing they are.</summary>
    [Fact]
    public void ANamespaceGroupKeepsItsFivePartsAsGiven()
    {
        var environment = ClusterFixture.Environment("tdd", "apps");
        IReadOnlyList<PodRow> rows =
        [
            new PodRow(ClusterFixture.Pod("ui"), PodState.Ready),
            new PodRow(ClusterFixture.Pod("job", ClusterPod.Succeeded, reason: "Completed").NotReady(), PodState.Finished),
        ];
        IReadOnlyList<ClusterVolume> volumes = [new ClusterVolume("data", 8589934592d, "Bound")];

        var group = new NamespaceGroup("apps", environment, true, rows, volumes);

        Assert.Equal("apps", group.Name);
        Assert.NotNull(group.Environment);
        Assert.Same(environment, group.Environment);
        Assert.True(group.Found);
        Assert.Same(rows, group.Pods);
        Assert.Same(volumes, group.Volumes);

        var empty = new NamespaceGroup(string.Empty, null, false, [], []);
        Assert.Equal(string.Empty, empty.Name);
        Assert.Null(empty.Environment);
        Assert.False(empty.Found);
        Assert.Empty(empty.Pods);
        Assert.Empty(empty.Volumes);
    }

    /// <summary>The groups keep the very two lists they are given, and a platform whose one group holds an
    /// unhealthy pod needs a look: the fixed value the property stands on, read off the record itself.</summary>
    [Fact]
    public void ClusterGroupsKeepsItsTwoListsAndTheUnhealthyPlatformNeedsALook()
    {
        IReadOnlyList<NamespaceGroup> environments =
        [
            new NamespaceGroup("apps", ClusterFixture.Environment("tdd", "apps"), true, [new PodRow(ClusterFixture.Pod("ui"), PodState.Ready)], []),
        ];
        IReadOnlyList<NamespaceGroup> platform =
        [
            new NamespaceGroup("argocd", null, true, [new PodRow(ClusterFixture.Pod("server", restarts: 3, reason: "CrashLoopBackOff").NotReady(), PodState.Unhealthy)], []),
        ];

        var groups = new ClusterGroups(environments, platform);

        Assert.Same(environments, groups.Environments);
        Assert.Same(platform, groups.Platform);
        Assert.True(groups.PlatformNeedsALook);
    }

    /// <summary>The totals keep the very ten parts they are given: the six counts as the numbers they were told,
    /// the cpu and the memory as the same instances, and the two node counts as the numbers or the nothing - a
    /// totals that is given no node counts keeps them as the nothing they are.</summary>
    [Fact]
    public void ClusterTotalsKeepTheirTenPartsAsGiven()
    {
        var cpu = new ResourceUse(812, 3450, 9100, 7720);
        var memory = new ResourceUse(9876543210, 6200000000, 11800000000, 13400000000);

        var totals = new ClusterTotals(1, 2, 4, 5, 1, cpu, memory, 63, 110, 8);

        Assert.Equal((1, 2, 4, 5, 1), (totals.NodesReady, totals.Nodes, totals.PodsReady, totals.Pods, totals.PodsFinished));
        Assert.Same(cpu, totals.Cpu);
        Assert.Same(memory, totals.Memory);
        Assert.Equal((63, 110, 8), (totals.PodsOnNodes, totals.PodCapacity, totals.Restarts));

        var noNodes = new ClusterTotals(0, 0, 0, 0, 0, ResourceUse.None, ResourceUse.None, null, null, 0);
        Assert.Null(noNodes.PodsOnNodes);
        Assert.Null(noNodes.PodCapacity);
    }

    /// <summary>The sample keeps the very two numbers it is given - the fixed ones, and the nothing - and a
    /// number that is not there is not the zero it is not: the sample without a number is not the sample that
    /// says zero.</summary>
    [Fact]
    public void ClusterSampleKeepsItsTwoNumbersAndNothingIsNotZero()
    {
        var withNumbers = new ClusterSample(812, 2 * 9876543210d);
        Assert.Equal(812, withNumbers.CpuMillicores);
        Assert.Equal(2 * 9876543210d, withNumbers.MemoryBytes);

        var without = new ClusterSample(null, null);
        Assert.Null(without.CpuMillicores);
        Assert.Null(without.MemoryBytes);

        Assert.NotEqual(without, new ClusterSample(0, null));
        Assert.NotEqual(withNumbers, new ClusterSample(null, 2 * 9876543210d));
    }

    // ----- the two that are one

    /// <summary>Two groups are one while all five parts are - the environment's own equality, the lists' - and a
    /// difference in any one part, the name or the found turned or the environment or the pods or the volumes
    /// changed, is another one. The hash code follows the equality, and neither no group nor a word is a
    /// group's equal.</summary>
    [Fact]
    public void TwoNamespaceGroupsAreOneWhenAllFivePartsAre()
    {
        var environment = ClusterFixture.Environment("tdd", "apps");
        IReadOnlyList<PodRow> rows = [new PodRow(ClusterFixture.Pod("ui"), PodState.Ready)];
        IReadOnlyList<ClusterVolume> volumes = [new ClusterVolume("data", 8589934592d, "Bound")];

        var one = new NamespaceGroup("apps", environment, true, rows, volumes);
        var two = new NamespaceGroup("apps", environment, true, rows, volumes);

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.False(one != two);
        Assert.True(one.Equals((object)two));
        Assert.True(two.Equals((object)one));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.False(one.Equals("apps"));

        Assert.NotEqual(one, new NamespaceGroup("other", environment, true, rows, volumes));
        Assert.NotEqual(one, new NamespaceGroup("apps", ClusterFixture.Environment("dev", "apps"), true, rows, volumes));
        Assert.NotEqual(one, new NamespaceGroup("apps", environment, false, rows, volumes));
        Assert.NotEqual(one, new NamespaceGroup("apps", environment, true, [new PodRow(one.Pods[0].Pod, PodState.Starting)], volumes));
        Assert.NotEqual(one, new NamespaceGroup("apps", environment, true, rows, [new ClusterVolume("logs", 1073741824d, "Pending")]));
        Assert.Equal(one, new NamespaceGroup(one.Name, one.Environment, one.Found, one.Pods, one.Volumes));
    }

    /// <summary>Two groups of namespaces are one while the two lists are, and a difference in either list is
    /// another one: the hash code follows the equality, and neither no groups nor a word is the groups'
    /// equal.</summary>
    [Fact]
    public void TwoClusterGroupsAreOneWhileTheirTwoListsAre()
    {
        IReadOnlyList<NamespaceGroup> environments = [];
        IReadOnlyList<NamespaceGroup> platform = [];

        var one = new ClusterGroups(environments, platform);
        var two = new ClusterGroups(environments, platform);

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.True(one.Equals((object)two));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.False(one.Equals("Platform"));

        Assert.NotEqual(one, new ClusterGroups([new NamespaceGroup("apps", null, true, [], [])], platform));
        Assert.NotEqual(one, new ClusterGroups(environments, [new NamespaceGroup("argocd", null, true, [], [])]));
        Assert.Equal(one, new ClusterGroups(one.Environments, one.Platform));
    }

    /// <summary>Two totals are one while all ten parts are, and a difference in any one of them is another one:
    /// the hash code follows the equality, and neither no totals nor a number is the totals' equal.</summary>
    [Fact]
    public void TwoClusterTotalsAreOneWhenAllTenPartsAre()
    {
        var cpu = new ResourceUse(812, 3450, 9100, 7720);
        var memory = new ResourceUse(9876543210, 6200000000, 11800000000, 13400000000);

        var one = new ClusterTotals(1, 2, 4, 5, 1, cpu, memory, 63, 110, 8);
        var two = new ClusterTotals(1, 2, 4, 5, 1, cpu, memory, 63, 110, 8);

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.True(one.Equals((object)two));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.False(one.Equals(812));

        Assert.NotEqual(one, one with { NodesReady = 2 });
        Assert.NotEqual(one, one with { Nodes = 1 });
        Assert.NotEqual(one, one with { PodsReady = 5 });
        Assert.NotEqual(one, one with { Pods = 4 });
        Assert.NotEqual(one, one with { PodsFinished = 0 });
        Assert.NotEqual(one, one with { Cpu = ResourceUse.None });
        Assert.NotEqual(one, one with { Memory = ResourceUse.None });
        Assert.NotEqual(one, one with { PodsOnNodes = null });
        Assert.NotEqual(one, one with { PodCapacity = null });
        Assert.NotEqual(one, one with { Restarts = 7 });
        Assert.Equal(one, one with { });
    }

    /// <summary>Two samples are one while the two numbers are - the number the sample does not have counted as
    /// the nothing it is - and a difference in either number is another one: the hash code follows the
    /// equality.</summary>
    [Fact]
    public void TwoClusterSamplesAreOneWhileTheirTwoNumbersAre()
    {
        var one = new ClusterSample(812, 2 * 9876543210d);
        var two = new ClusterSample(812, 2 * 9876543210d);

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.False(one != two);
        Assert.True(one.Equals((object)two));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.False(one.Equals("812"));

        Assert.NotEqual(one, new ClusterSample(813, 2 * 9876543210d));
        Assert.NotEqual(one, new ClusterSample(812, 1d));
        Assert.Equal(new ClusterSample(null, null), new ClusterSample(null, null));
        Assert.Equal(one, new ClusterSample(one.CpuMillicores, one.MemoryBytes));
    }

    // ----- the record falls apart and is copied

    /// <summary>The group falls apart into its five parts and a copy of it is it again: the <c>with</c> that
    /// names no part and the <c>with</c> that turns the parts it names keep the rest, and not the same
    /// reference.</summary>
    [Fact]
    public void ANamespaceGroupFallsApartIntoItsPartsAndACopyIsTheGroupAgain()
    {
        var environment = ClusterFixture.Environment("tdd", "apps");
        var group = new NamespaceGroup("apps", environment, true, [], []);

        var copy = group with { };
        Assert.Equal(group, copy);
        Assert.NotSame(group, copy);
        Assert.Same(group.Environment, copy.Environment);

        var turned = group with { Name = "jobs", Found = false };
        Assert.Equal("jobs", turned.Name);
        Assert.False(turned.Found);
        Assert.Same(group.Environment, turned.Environment);
        Assert.NotEqual(group, turned);

        var (name, env, found, pods, volumes) = group;
        Assert.Equal("apps", name);
        Assert.NotNull(env);
        Assert.Same(environment, env);
        Assert.True(found);
        Assert.Empty(pods);
        Assert.Empty(volumes);
    }

    /// <summary>The groups fall apart into their two lists and a copy of them is them again: the <c>with</c>
    /// that names no part keeps both, and the <c>with</c> that turns the platform keeps the environments it
    /// does not name.</summary>
    [Fact]
    public void ClusterGroupsFallApartIntoTheirListsAndACopyIsTheGroupsAgain()
    {
        IReadOnlyList<NamespaceGroup> environments = [new NamespaceGroup("apps", null, true, [], [])];
        IReadOnlyList<NamespaceGroup> platform = [];

        var groups = new ClusterGroups(environments, platform);

        var copy = groups with { };
        Assert.Equal(groups, copy);
        Assert.NotSame(groups, copy);

        var turned = groups with { Platform = [new NamespaceGroup("argocd", null, true, [], [])] };
        Assert.Same(groups.Environments, turned.Environments);
        Assert.Equal("argocd", Assert.Single(turned.Platform).Name);
        Assert.NotEqual(groups, turned);

        var (keptEnvironments, keptPlatform) = groups;
        Assert.Same(environments, keptEnvironments);
        Assert.Same(platform, keptPlatform);
    }

    /// <summary>The totals fall apart into their ten parts and a copy of them is them again: the <c>with</c>
    /// that names no part keeps all ten, the parts the totals does not count said as the nothing they
    /// are.</summary>
    [Fact]
    public void ClusterTotalsFallApartIntoTheirPartsAndACopyIsTheTotalsAgain()
    {
        var totals = new ClusterTotals(0, 0, 0, 0, 0, ResourceUse.None, ResourceUse.None, null, null, 0);

        var copy = totals with { };
        Assert.Equal(totals, copy);
        Assert.NotSame(totals, copy);
        Assert.Same(totals.Cpu, copy.Cpu);
        Assert.Same(totals.Memory, copy.Memory);

        var turned = totals with { Restarts = 7 };
        Assert.Equal(7, turned.Restarts);
        Assert.Same(totals.Cpu, turned.Cpu);
        Assert.NotEqual(totals, turned);

        var (nodesReady, nodes, podsReady, pods, podsFinished, cpu, memory, podsOnNodes, podCapacity, restarts) = totals;
        Assert.Equal(0, nodesReady);
        Assert.Equal(0, nodes);
        Assert.Equal(0, podsReady);
        Assert.Equal(0, pods);
        Assert.Equal(0, podsFinished);
        Assert.Equal(ResourceUse.None, cpu);
        Assert.Equal(ResourceUse.None, memory);
        Assert.Null(podsOnNodes);
        Assert.Null(podCapacity);
        Assert.Equal(0, restarts);
    }

    // ----- the words each record says

    /// <summary>The group says its parts in the order the record declares them - the counts and the sums after
    /// the lists, each as the value it stands on: the environment that is not there and the sums that are not
    /// there said as the empty word, the counts as the zeros they are.</summary>
    [Fact]
    public void ANamespaceGroupSaysItsPartsInTheOrderItDeclaresThem() =>
        Assert.Equal(
            "NamespaceGroup { Name = apps, Environment = , Found = True, Pods = Dashboard.Cluster.PodRow[], " +
                "Volumes = Dashboard.Cluster.ClusterVolume[], Ready = 0, Total = 0, Finished = 0, Unhealthy = 0, " +
                "Restarts = 0, CpuUsage = , MemoryUsage =  }",
            new NamespaceGroup("apps", null, true, [], []).ToString());

    /// <summary>The groups say their parts in the order the record declares them: the two lists and, after
    /// them, the look the platform needs - the empty platform said as the no it is.</summary>
    [Fact]
    public void ClusterGroupsSayTheirPartsInTheOrderTheyDeclareThem() =>
        Assert.Equal(
            "ClusterGroups { Environments = Dashboard.Cluster.NamespaceGroup[], Platform = Dashboard.Cluster.NamespaceGroup[], " +
                "PlatformNeedsALook = False }",
            new ClusterGroups([], []).ToString());

    /// <summary>The totals say their parts in the order the record declares them: the six counts, the cpu and
    /// the memory with the numbers they do not report said as the empty word, and the two node counts that are
    /// not there said as the nothing they are.</summary>
    [Fact]
    public void ClusterTotalsSayTheirPartsInTheOrderTheyDeclareThem() =>
        Assert.Equal(
            "ClusterTotals { NodesReady = 0, Nodes = 0, PodsReady = 0, Pods = 0, PodsFinished = 0, " +
                "Cpu = ResourceUse { Usage = , Requests = , Limits = , Allocatable =  }, " +
                "Memory = ResourceUse { Usage = , Requests = , Limits = , Allocatable =  }, " +
                "PodsOnNodes = , PodCapacity = , Restarts = 0 }",
            new ClusterTotals(0, 0, 0, 0, 0, ResourceUse.None, ResourceUse.None, null, null, 0).ToString());

    /// <summary>The sample says its parts in the order the record declares them: the fixed numbers as the
    /// numbers they stand on, and the number that is not there said as the empty word.</summary>
    [Fact]
    public void ClusterSampleSaysItsPartsInTheOrderItDeclaresThem()
    {
        Assert.Equal("ClusterSample { CpuMillicores = 812, MemoryBytes = 19753086420 }", new ClusterSample(812, 2 * 9876543210d).ToString());
        Assert.Equal("ClusterSample { CpuMillicores = , MemoryBytes =  }", new ClusterSample(null, null).ToString());
    }

    // ----- the Of that is given what is not there

    /// <summary>The groups of no status and the groups of no environments are no groups: the rule says so
    /// before it says anything else, each with the name of the part that is not there.</summary>
    [Fact]
    public void AnOfGivenNoStatusOrNoEnvironmentsIsNoGroups()
    {
        var noStatus = Assert.Throws<ArgumentNullException>(() => ClusterGroups.Of(null!, [], Generated));
        Assert.Equal("status", noStatus.ParamName);

        var noEnvironments = Assert.Throws<ArgumentNullException>(() => ClusterGroups.Of(ClusterFixture.Status(), null!, Generated));
        Assert.Equal("environments", noEnvironments.ParamName);
    }

    /// <summary>The totals of no status are no totals: the rule says so before it says anything
    /// else.</summary>
    [Fact]
    public void AnOfGivenNoStatusIsNoTotals()
    {
        var error = Assert.Throws<ArgumentNullException>(() => ClusterTotals.Of(null!, Generated));
        Assert.Equal("status", error.ParamName);
    }

    /// <summary>The sample of no status is no sample: the rule says so before it says anything
    /// else.</summary>
    [Fact]
    public void AnOfGivenNoStatusIsNoSample()
    {
        var error = Assert.Throws<ArgumentNullException>(() => ClusterSample.Of(null!));
        Assert.Equal("status", error.ParamName);
    }

    // ----- the ends Of sorts by

    /// <summary>The environment's namespace is read as the word it is, not as the word it looks like: the
    /// namespace that differs from it in case alone is not its namespace - the environment stands as the
    /// namespace the status does not list, and the namespace stays in the platform it is part of.</summary>
    [Fact]
    public void AnEnvironmentWhoseNamespaceDiffersOnlyInCaseIsNotFoundAndItsNamespaceStaysInThePlatform()
    {
        var status = ClusterFixture.Status(null, ClusterFixture.Space("CMDemo3-prod", ClusterFixture.Pod("ui")));

        var groups = ClusterGroups.Of(status, [ClusterFixture.Environment("prod", "cmdemo3-prod", "prod")], Generated);

        var missing = Assert.Single(groups.Environments);
        Assert.Equal("cmdemo3-prod", missing.Name);
        Assert.NotNull(missing.Environment);
        Assert.Equal("prod", missing.Environment.Name);
        Assert.False(missing.Found);
        Assert.Empty(missing.Pods);
        Assert.Empty(missing.Volumes);
        Assert.Equal("CMDemo3-prod", Assert.Single(groups.Platform).Name);
    }

    /// <summary>The environment whose namespace is the empty string is still an environment: it stands as the
    /// group of the namespace no status lists, and the empty name takes no namespace from the platform it
    /// would otherwise have been one of.</summary>
    [Fact]
    public void AnEnvironmentWhoseNamespaceIsTheEmptyStringIsNotFoundAndTakesNothingFromThePlatform()
    {
        var status = ClusterFixture.Status(null, ClusterFixture.Space("apps", ClusterFixture.Pod("ui")));

        var groups = ClusterGroups.Of(status, [ClusterFixture.Environment("weird", string.Empty)], Generated);

        var empty = Assert.Single(groups.Environments);
        Assert.Equal(string.Empty, empty.Name);
        Assert.False(empty.Found);
        Assert.Empty(empty.Pods);
        Assert.Empty(empty.Volumes);
        Assert.Equal("apps", Assert.Single(groups.Platform).Name);
    }

    /// <summary>The two environments on one namespace stand as two groups, each with the pods of the
    /// namespace the status does list, and the platform the namespace is not in: the name one environment
    /// takes, the other does not give back.</summary>
    [Fact]
    public void TwoEnvironmentsOnOneNamespaceStandAsTwoGroupsAndThePlatformLosesTheNamespace()
    {
        var status = ClusterFixture.Status(null, ClusterFixture.Space("apps", ClusterFixture.Pod("ui")));

        var groups = ClusterGroups.Of(
            status,
            [ClusterFixture.Environment("tdd", "apps"), ClusterFixture.Environment("dev", "apps")],
            Generated);

        Assert.Equal(2, groups.Environments.Count);
        Assert.Equal(["tdd", "dev"], groups.Environments.Select(group => group.Environment!.Name));
        Assert.All(groups.Environments, group => Assert.True(group.Found));
        Assert.Equal(["ui-0", "ui-0"], groups.Environments.SelectMany(group => group.Pods).Select(row => row.Pod.Name));
        Assert.Empty(groups.Platform);
    }

    /// <summary>A status that lists the same namespace twice stands as two platform groups: the groups are
    /// made of what the status lists, in the order it lists it, and the name one group holds, the other does
    /// not give back.</summary>
    [Fact]
    public void AStatusThatListsTheSameNamespaceTwiceStandsAsTwoPlatformGroups()
    {
        var status = ClusterFixture.Status(
            null,
            ClusterFixture.Space("apps", ClusterFixture.Pod("ui")),
            ClusterFixture.Space("apps", ClusterFixture.Pod("db")));

        var groups = ClusterGroups.Of(status, [], Generated);

        Assert.Equal(2, groups.Platform.Count);
        Assert.Equal(["apps", "apps"], groups.Platform.Select(group => group.Name));
        Assert.Equal(["ui-0", "db-0"], groups.Platform.SelectMany(group => group.Pods).Select(row => row.Pod.Name));
    }

    // ----- the ends of the reading

    /// <summary>The status that lists without naming - no node, no namespace, nothing - stands on nothing:
    /// every count is the zero it counts, every sum the nothing none of its nodes reports, and the two node
    /// counts the no node does not say.</summary>
    [Fact]
    public void AStatusThatListsWithoutNamingStandsOnNothing()
    {
        var totals = ClusterTotals.Of(ClusterFixture.Status([], []), Generated);

        Assert.Equal((0, 0, 0, 0, 0), (totals.NodesReady, totals.Nodes, totals.PodsReady, totals.Pods, totals.PodsFinished));
        Assert.Equal(new ResourceUse(null, null, null, null), totals.Cpu);
        Assert.Equal(new ResourceUse(null, null, null, null), totals.Memory);
        Assert.Null(totals.PodsOnNodes);
        Assert.Null(totals.PodCapacity);
        Assert.Equal(0, totals.Restarts);
    }

    /// <summary>The reading without nodes says nothing to the trends: no node reports a usage, so the sample
    /// the page keeps of the reading is the one without a number - as opposed to the node that reports no
    /// usage, which the page's sums already stand on.</summary>
    [Fact]
    public void AStatusWithoutNodesHasNoSample() =>
        Assert.Equal(new ClusterSample(null, null), ClusterSample.Of(ClusterFixture.Status([], [])));

    /// <summary>The totals that are not there say the one word the all-nothing use says: the constant that every
    /// edge-case file names instead of repeating the string twice. The test builds the totals of a status with no
    /// nodes and checks that its words carry the constant where the resource sums stand.</summary>
    [Fact]
    public void TheTotalsOfNoNodesSayTheEmptyResourceUseConstant()
    {
        var totals = ClusterTotals.Of(ClusterFixture.Status([], []), Generated);

        Assert.Contains($"Cpu = {RecordWords.EmptyResourceUse}", totals.ToString(), StringComparison.Ordinal);
        Assert.Contains($"Memory = {RecordWords.EmptyResourceUse}", totals.ToString(), StringComparison.Ordinal);
    }
}
