namespace Dashboard.Tests;

/// <summary>
/// A <see cref="NamespaceGroup" /> is one namespace as the cluster view shows it: the five parts the reader hands it -
/// its name, the environment it belongs to if any, whether the status file listed it at all, its
/// <see cref="PodRow" />s in the order of the table and its volumes - and the seven sums the page counts off those
/// rows. Where those parts come from is pinned where it belongs: <see cref="NamespaceGroupTests" /> and
/// <see cref="ClusterGroupsTests" /> walk <see cref="ClusterGroups.Of" /> through the order the rows come out in
/// (unhealthy first, then starting, then ready, then finished, each by label and name), the counts of a mixed
/// namespace, the namespace the status file does not list, and a namespace whose pods report no usage at all.
/// <para>
/// What follows is the rest, which none of that holds: the five parts handed to the constructor position by position
/// and given back by <c>Deconstruct</c>, the lists kept as the very lists the group was given rather than copied,
/// sorted or filtered, a name taken exactly as it came - empty, spaced, or one whose case means something, since the
/// group compares it with case while the platform orders it without - and the parts that are nothing: the absent
/// environment, the empty group, and the group built of nothing at all, which the record neither refuses nor fills
/// out. Then each of the four counts on a group of one row, so that a row lands on exactly one of them and on the
/// <see cref="NamespaceGroup.Total" /> the finished row does not; the restarts a finished row carries, which the
/// mixed namespace's finished row never had; the ceiling an <see cref="int" /> puts under their sum, and the
/// largest double the two usage sums still add exactly; and the two usages at the edge the sample never sits on: a
/// pod that reports exactly nothing used, which is a measurement, beside one that reports nothing at all, which is
/// not - and a pod that measures one of the two resources only, which leaves the other sum alone. Last the members
/// every record gets and no test of this one has ever called: whether two groups are one group, what a clone
/// changes, and the single line a group says about itself.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: the state of every row is one the test
/// hands the <see cref="PodRow" /> itself, so <c>PodRules.StateOf</c> is never consulted and no age enters. The two
/// members that do the ordering and the adding - <c>NamespaceGroup.Of</c> and <c>NamespaceGroup.Sum</c> - are
/// internal to an app that declares no <c>InternalsVisibleTo</c> (as <c>Dashboard.Tests.csproj</c> notes of
/// <c>JsonRead</c>), so they are pinned through what the page can reach: <see cref="ClusterGroups.Of" /> for the
/// order of the rows, and the two usage sums for the adding. The cluster card reaches the same internal sum through
/// <c>ClusterTotals.Of</c> and <c>ClusterSample.Of</c>, which <see cref="ClusterGroupsTests" /> holds.
/// </para>
/// </summary>
public class NamespaceGroupEdgeCaseTests
{
    private static readonly EnvironmentInfo Prod = ClusterFixture.Environment("prod", "cmdemo3-prod", "prod");

    // ------------------------------------------------------------------- the five parts

    /// <summary>
    /// The constructor's five parts, in the order the record declares them, come back as the five properties of the
    /// same name - the order <see cref="ClusterGroups.Of" /> relies on when it builds a group, and the order the
    /// page's own line prints them in.
    /// </summary>
    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        PodRow[] rows = [Row("web", PodState.Ready), Row("broken", PodState.Unhealthy, restarts: 4)];
        ClusterVolume[] volumes = [new ClusterVolume("data", 8589934592d, "Bound")];

        var group = new NamespaceGroup("cmdemo3-prod", Prod, true, rows, volumes);

        Assert.Equal("cmdemo3-prod", group.Name);
        Assert.Equal(Prod, group.Environment);
        Assert.True(group.Found);
        Assert.Equal(rows, group.Pods);
        Assert.Equal(volumes, group.Volumes);
        Assert.Same(rows, group.Pods);
        Assert.Same(volumes, group.Volumes);
    }

    /// <summary>
    /// The group is the one place the view's order is <em>not</em> made: it keeps the rows and the volumes as the list
    /// it was handed, in that order, with nothing dropped and nothing sorted - the finished row stays where it was put
    /// rather than going last, a row twice over is held twice, and a volume whose size and phase the file never says
    /// keeps both as the nothing it was given. The order belongs to <see cref="ClusterGroups.Of" />, which
    /// <see cref="NamespaceGroupTests" /> pins; what is pinned here is that the record itself never touches it.
    /// </summary>
    [Fact]
    public void TheRowsAndTheVolumesAreKeptAsTheyWereGiven()
    {
        var web = Row("web", PodState.Ready);
        var late = Row("backup", PodState.Finished);
        var first = Row("api", PodState.Starting);
        PodRow[] rows = [late, web, first, web];
        ClusterVolume[] volumes =
        [
            new ClusterVolume("zeta", null, null),
            new ClusterVolume("alpha", 0, "Pending"),
            new ClusterVolume("data", 8589934592d, "Bound"),
        ];

        var group = new NamespaceGroup("apps", null, true, rows, volumes);

        Assert.Equal(["backup-0", "web-0", "api-0", "web-0"], group.Pods.Select(row => row.Pod.Name));
        Assert.Equal(
            [PodState.Finished, PodState.Ready, PodState.Starting, PodState.Ready],
            group.Pods.Select(row => row.State));
        Assert.Same(rows[1], group.Pods[1]);
        Assert.Same(rows[3], group.Pods[3]);

        Assert.Equal(["zeta", "alpha", "data"], group.Volumes.Select(volume => volume.Name));
        Assert.Equal(
            [0, 8589934592d],
            group.Volumes.Where(volume => volume.Capacity is not null).Select(volume => volume.Capacity!.Value));
        Assert.Equal(
            ["Pending", "Bound"],
            group.Volumes.Where(volume => volume.Phase is not null).Select(volume => volume.Phase!));
        Assert.Null(group.Volumes[0].Capacity);
        Assert.Null(group.Volumes[0].Phase);

        // The rows are counted as they stand, in the order they stand, with the duplicate counted twice and the
        // finished one left out of the total alone.
        Assert.Equal((2, 3, 1, 0), (group.Ready, group.Total, group.Finished, group.Unhealthy));
    }

    /// <summary>The five parts come back out in the order they were declared, which is the order the page reads them
    /// in: a group taken apart and put back is the group it was, list and all.</summary>
    [Fact]
    public void AGroupTakenApartGivesItsPartsBackInTheOrderTheyWereDeclared()
    {
        PodRow[] rows = [Row("web", PodState.Ready)];
        ClusterVolume[] volumes = [new ClusterVolume("data", 1024, "Bound")];
        var group = new NamespaceGroup("cmdemo3-prod", Prod, false, rows, volumes);

        var (name, environment, found, pods, spaces) = group;

        Assert.Equal("cmdemo3-prod", name);
        Assert.Equal(Prod, environment);
        Assert.False(found);
        Assert.Same(rows, pods);
        Assert.Same(volumes, spaces);
        Assert.Equal(group, new NamespaceGroup(name, environment, found, pods, spaces));
    }

    /// <summary>
    /// A name is the name: the group trims nothing, fills nothing out and judges nothing. The empty string is a name
    /// the group carries as it carries any other, the platform's name is a name like an environment's, and the case
    /// stands - <c>apps</c> and <c>APPS</c> being two groups, whatever
    /// <see cref="ClusterGroups.Of" /> does when it <em>orders</em> namespaces without regard to their case
    /// (<see cref="NamespaceGroupTests" />).
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("apps")]
    [InlineData("  cmdemo3-tdd  ")]
    [InlineData("APPS")]
    [InlineData("kube-system")]
    public void ANameIsTheNameTheGroupWasGiven(string name)
    {
        var group = new NamespaceGroup(name, null, true, [Row("web", PodState.Ready)], []);

        Assert.Equal(name, group.Name);
        Assert.Equal(1, group.Total);
    }

    /// <summary>Two names that differ by case, or by nothing at all, or by the spaces around them: the name is
    /// compared as the string it is, ordinal, so the empty name is a name of its own and one group of spaces is the
    /// equal of another.</summary>
    [Fact]
    public void ANameOfNothingOrOfSpacesIsStillANameOfItsOwn()
    {
        var empty = new NamespaceGroup("", null, true, [], []);
        var spaces = new NamespaceGroup("   ", null, true, [], []);
        var lower = new NamespaceGroup("apps", null, true, [], []);
        var upper = new NamespaceGroup("APPS", null, true, [], []);

        Assert.Equal(empty, new NamespaceGroup("", null, true, [], []));
        Assert.NotEqual(empty, spaces);
        Assert.NotEqual(spaces, lower);
        Assert.NotEqual(lower, upper);
        Assert.Equal(lower, new NamespaceGroup("apps", null, true, [], []));
    }

    /// <summary>Nothing about the five parts is refused or invented: a group with no pods and no volumes is a group
    /// that counts nothing and says it, and the empty lists the collection expression hands over are the one empty
    /// array every such group shares. The record does no argument check of any kind - the parts it is given are the
    /// parts it keeps, even when they are nothing at all.</summary>
    [Fact]
    public void AGroupBuiltOfNothingIsAGroupAndNotAnError()
    {
        var empty = new NamespaceGroup("", null, false, [], []);
        var also = new NamespaceGroup("", null, false, [], []);

        Assert.Empty(empty.Pods);
        Assert.Empty(empty.Volumes);
        Assert.Same(empty.Pods, also.Pods);
        Assert.Same(empty.Volumes, also.Volumes);

        var bare = new NamespaceGroup(null!, null, false, null!, null!);

        Assert.Null(bare.Name);
        Assert.Null(bare.Environment);
        Assert.False(bare.Found);
        Assert.Null(bare.Pods);
        Assert.Null(bare.Volumes);
    }

    // ---------------------------------------------------------------------- the four counts

    /// <summary>
    /// One row, of each of the four states, counted four ways: the row lands on exactly one of
    /// <see cref="NamespaceGroup.Ready" />, <see cref="NamespaceGroup.Finished" /> and
    /// <see cref="NamespaceGroup.Unhealthy" />, and on <see cref="NamespaceGroup.Total" /> too - except the finished
    /// one, which the total leaves out and the finished count takes on its own. The four numbers of a mixed namespace
    /// stand in <see cref="NamespaceGroupTests" />; these are the four points they are made of, where each count can
    /// turn on a single row.
    /// </summary>
    [Theory]
    [InlineData(PodState.Ready, 1, 1, 0, 0)]
    [InlineData(PodState.Starting, 0, 1, 0, 0)]
    [InlineData(PodState.Unhealthy, 0, 1, 0, 1)]
    [InlineData(PodState.Finished, 0, 0, 1, 0)]
    public void OneRowOfEachStateIsCountedOnceAndWhereItShouldBe(
        PodState state,
        int ready,
        int total,
        int finished,
        int unhealthy)
    {
        var group = new NamespaceGroup("apps", null, true, [Row("web", state)], []);

        Assert.Equal(ready, group.Ready);
        Assert.Equal(total, group.Total);
        Assert.Equal(finished, group.Finished);
        Assert.Equal(unhealthy, group.Unhealthy);
        Assert.Equal(ready + (state == PodState.Starting ? 1 : 0) + unhealthy, group.Total);

        var row = Assert.Single(group.Pods);
        Assert.Equal(state, row.State);
    }

    /// <summary>The counts read the rows and nothing else: the same two rows count the same whether the namespace
    /// was found in the status file or is one the file never listed, whether it belongs to an environment, to the
    /// platform, or to an environment that names no namespace at all, and whether the namespace holds three volumes
    /// or none. <see cref="NamespaceGroup.Found" /> is a word about the status file, not a part of a sum.</summary>
    [Fact]
    public void TheCountsReadTheRowsAndNothingElse()
    {
        PodRow[] rows = [Row("web", PodState.Ready), Row("api", PodState.Unhealthy, restarts: 3)];
        ClusterVolume[] volumes =
        [
            new ClusterVolume("data", 8589934592d, "Bound"),
            new ClusterVolume("cache", null, null),
            new ClusterVolume("logs", 0, "Pending"),
        ];
        var namespaceless = ClusterFixture.Environment("prod", null, tier: null);

        var found = new NamespaceGroup("apps", Prod, true, rows, volumes);
        var missing = new NamespaceGroup("apps", namespaceless, false, rows, volumes);
        var platform = new NamespaceGroup("apps", null, true, rows, []);

        Assert.Equal((1, 2, 0, 1, 3), (found.Ready, found.Total, found.Finished, found.Unhealthy, found.Restarts));
        Assert.Equal((1, 2, 0, 1, 3), (missing.Ready, missing.Total, missing.Finished, missing.Unhealthy, missing.Restarts));
        Assert.Equal((1, 2, 0, 1, 3), (platform.Ready, platform.Total, platform.Finished, platform.Unhealthy, platform.Restarts));

        Assert.True(found.Found);
        Assert.False(missing.Found);
        Assert.Equal(namespaceless, missing.Environment);
        Assert.Null(missing.Environment!.Namespace);
        Assert.Null(platform.Environment);
        Assert.Equal(["data", "cache", "logs"], found.Volumes.Select(volume => volume.Name));
        Assert.Empty(platform.Volumes);
    }

    // ------------------------------------------------------------------ restarts and usages

    /// <summary>A finished row is out of the total and in the restarts: a job that ran to its end after four
    /// restarts leaves the namespace with three pods counted and seven restarts counted. The mixed namespace of
    /// <see cref="NamespaceGroupTests" /> has a finished pod that restarted never, so this is the edge of its fourth
    /// number; every other state carries its restarts too, and a row that says nothing says zero rather than
    /// nothing.</summary>
    [Fact]
    public void RestartsAreCountedOnEveryRowEvenTheFinished()
    {
        var group = new NamespaceGroup(
            "jobs",
            null,
            true,
            [
                Row("web", PodState.Ready),
                Row("api", PodState.Starting, restarts: 1),
                Row("broken", PodState.Unhealthy, restarts: 2),
                Row("backup", PodState.Finished, restarts: 4),
            ],
            []);

        Assert.Equal(7, group.Restarts);
        Assert.Equal((1, 3, 1, 1), (group.Ready, group.Total, group.Finished, group.Unhealthy));
        Assert.Equal(0, new NamespaceGroup("apps", null, true, [Row("web", PodState.Ready)], []).Restarts);
    }

    /// <summary>The restarts of a namespace are counted in an <see cref="int" /> and no wider: the largest count one
    /// pod may report is the largest sum the group can give back, and one restart past it is a sum that throws
    /// rather than one that wraps into a negative. Nothing the cluster reports comes near it - a pod restarts
    /// thousands of times at most - but the number the page prints is an <see cref="int" />, and this is where it
    /// stops being one.</summary>
    [Fact]
    public void RestartsAreCountedInAnIntAndRefuseToGoPastIt()
    {
        var largest = new NamespaceGroup("apps", null, true, [Row("web", PodState.Ready, restarts: int.MaxValue)], []);
        var over = new NamespaceGroup(
            "apps",
            null,
            true,
            [
                Row("web", PodState.Ready, restarts: int.MaxValue),
                Row("api", PodState.Ready, restarts: 1),
            ],
            []);

        Assert.Equal(int.MaxValue, largest.Restarts);
        Assert.Throws<OverflowException>(() => over.Restarts);
    }

    /// <summary>The line between nothing used and nothing said: a pod that reports its CPU and memory used as zero
    /// <em>does</em> report, so the group has both sums and each of them is zero - the same fact the sample's
    /// <c>cmdemo3-tdd</c> says in words, as <c>CPU 0 m</c> (<see cref="ClusterGroupsTests" />), and here as the
    /// number - while a pod that reports nothing at all leaves a sum with nothing to add, and the group with no sum
    /// to show. The two are told apart by one row each.</summary>
    [Fact]
    public void AUsageOfZeroIsAMeasurementAndAnAbsentOneIsNot()
    {
        var zero = new NamespaceGroup(
            "apps",
            null,
            true,
            [Row("web", PodState.Ready, cpu: new ResourceUse(0, 100, 500), memory: new ResourceUse(0, 268435456, null))],
            []);
        var silent = new NamespaceGroup(
            "apps",
            null,
            true,
            [Row("web", PodState.Ready, cpu: ResourceUse.None, memory: ResourceUse.None)],
            []);

        Assert.Equal(0d, zero.CpuUsage);
        Assert.Equal(0d, zero.MemoryUsage);
        Assert.NotNull(zero.CpuUsage);
        Assert.NotNull(zero.MemoryUsage);

        Assert.Null(silent.CpuUsage);
        Assert.Null(silent.MemoryUsage);
    }

    /// <summary>The two sums tell nothing to each other: a row that measures CPU and not memory adds to the first
    /// and not the second, a row that measures memory and not CPU the other way round, and a row that measures
    /// neither is walked past by both - yet it stays a row of the namespace, counted in its total and in its
    /// unhealthy. Every row is added whatever its state, the finished ones included (<see cref="ClusterGroupsTests" />
    /// pins that across a whole namespace); what is pinned here is that a number missing from one resource leaves
    /// the other resource's sum untouched, and that parts with nothing to add leave a sum absent rather than turning
    /// it into a zero.</summary>
    [Fact]
    public void TheTwoSumsAddTheirOwnRowsAndNothingElse()
    {
        var group = new NamespaceGroup(
            "apps",
            null,
            true,
            [
                Row("web", PodState.Ready, cpu: new ResourceUse(2.5, null, null), memory: ResourceUse.None),
                Row("api", PodState.Starting, cpu: ResourceUse.None, memory: new ResourceUse(1024, null, null)),
                Row("broken", PodState.Unhealthy, cpu: ResourceUse.None, memory: ResourceUse.None),
                Row("backup", PodState.Finished, cpu: new ResourceUse(0, null, null), memory: new ResourceUse(2048, null, null)),
            ],
            []);

        Assert.Equal(2.5, group.CpuUsage);
        Assert.Equal(3072d, group.MemoryUsage);
        Assert.Equal(
            [PodState.Ready, PodState.Starting, PodState.Unhealthy, PodState.Finished],
            group.Pods.Select(row => row.State));
    }

    /// <summary>The sums are the doubles the file gave, in the units the file gave them in: millicores and bytes
    /// added without rounding and without scaling, so two fractions make their exact whole and a byte count of four
    /// petabytes adds to the last byte. And where the restarts stop at an <see cref="int" />, the usages do not
    /// stop at all: a sum past the largest double the file can carry is infinity, said rather than thrown - the one
    /// way in which the two usage sums differ from the restart count beside them.</summary>
    [Fact]
    public void TheSumsAreTheNumbersTheyWereGivenInTheUnitsTheyWereGivenIn()
    {
        var group = new NamespaceGroup(
            "apps",
            null,
            true,
            [
                Row("a", PodState.Ready, cpu: new ResourceUse(0.5, null, null), memory: new ResourceUse(4503599627370496d, null, null)),
                Row("b", PodState.Ready, cpu: new ResourceUse(0.25, null, null), memory: new ResourceUse(4503599627370496d, null, null)),
            ],
            []);

        Assert.Equal(0.75, group.CpuUsage);
        Assert.Equal(9007199254740992d, group.MemoryUsage);

        var past = new NamespaceGroup(
            "apps",
            null,
            true,
            [
                Row("a", PodState.Ready, cpu: new ResourceUse(double.MaxValue, null, null), memory: ResourceUse.None),
                Row("b", PodState.Ready, cpu: new ResourceUse(double.MaxValue, null, null), memory: ResourceUse.None),
            ],
            []);

        Assert.Equal(double.PositiveInfinity, past.CpuUsage!.Value);
        Assert.False(double.IsFinite(past.CpuUsage!.Value));
    }

    // ------------------------------------------------------------------------ the record

    /// <summary>Two groups are one group when all five parts are - the name, the environment, the flag, and the two
    /// lists <em>as the lists they are</em>: the same array twice over makes one group, a copy of it element for
    /// element makes another, and the two empty lists every collection expression hands out are the one array, so
    /// two groups of nothing are equal where two groups of one pod each are not. Every one of the five parts is
    /// enough to tell two apart on its own, and none of the seven sums is part of the comparison: they are read off
    /// the rows, so two groups with the same rows cannot disagree about them.</summary>
    [Fact]
    public void TwoGroupsAreOneGroupWhenEveryPartAndTheirListsAre()
    {
        PodRow[] rows = [Row("web", PodState.Ready)];
        ClusterVolume[] volumes = [new ClusterVolume("data", 1024, "Bound")];
        var one = new NamespaceGroup("apps", Prod, true, rows, volumes);

        Assert.Equal(one, one with { Name = one.Name });
        Assert.True(one.Equals(one with { Environment = Prod }));
        Assert.True(one == one with { Found = true });
        Assert.False(one != one with { Name = "apps" });
        Assert.Equal(one.GetHashCode(), (one with { Found = true }).GetHashCode());
        Assert.True(one.Equals((object)one));
        Assert.False(one.Equals(null));
        Assert.False(one.Equals("apps"));

        NamespaceGroup[] different =
        [
            new NamespaceGroup("cmdemo3-prod", Prod, true, rows, volumes),
            new NamespaceGroup("apps", null, true, rows, volumes),
            new NamespaceGroup("apps", ClusterFixture.Environment("prod", "apps"), true, rows, volumes),
            new NamespaceGroup("apps", Prod, false, rows, volumes),
            new NamespaceGroup("apps", Prod, true, [Row("web", PodState.Ready)], volumes),
            new NamespaceGroup("apps", Prod, true, [], volumes),
            new NamespaceGroup("apps", Prod, true, rows, [new ClusterVolume("data", 1024, "Bound")]),
            new NamespaceGroup("apps", Prod, true, rows, []),
        ];

        foreach (var other in different)
        {
            Assert.NotEqual(one, other);
            Assert.False(one.Equals(other));
            Assert.True(one != other);
        }

        // A row copied value for value is another row, and a group holding it is another group: the list is compared
        // as the object it is, never element by element.
        Assert.NotEqual(one, new NamespaceGroup("apps", Prod, true, [Row("web", PodState.Ready)], volumes));
        Assert.Equal(one, new NamespaceGroup("apps", Prod, true, rows, volumes));
    }

    /// <summary>A clone changes the one part it is told to and hands on the rest, lists included: a group with
    /// another name is another group, and a group whose rows were only re-handled is the same group holding the same
    /// rows. The sums are no part of a clone - they are read off the rows afterwards, which is why changing one row
    /// of the list a clone shares changes the numbers of both.</summary>
    [Fact]
    public void ACloneCarriesTheListsOfTheGroupItCameFrom()
    {
        List<PodRow> rows = [Row("web", PodState.Ready)];
        var one = new NamespaceGroup("apps", Prod, true, rows, []);
        var renamed = one with { Name = "cmdemo3-prod" };

        Assert.NotEqual(one, renamed);
        Assert.Equal(Prod, renamed.Environment);
        Assert.True(renamed.Found);
        Assert.Same(one.Pods, renamed.Pods);
        Assert.Same(one.Volumes, renamed.Volumes);

        // The list is shared, not copied: what is added to it is in both groups, and they stay equal as they were.
        rows.Add(Row("api", PodState.Unhealthy, restarts: 5));

        Assert.Equal([PodState.Ready, PodState.Unhealthy], one.Pods.Select(row => row.State));
        Assert.Equal((1, 2, 0, 1, 5), (one.Ready, one.Total, one.Finished, one.Unhealthy, one.Restarts));
        Assert.Equal(
            (1, 2, 0, 1, 5),
            (renamed.Ready, renamed.Total, renamed.Finished, renamed.Unhealthy, renamed.Restarts));
        Assert.True(one.Equals(one with { Name = "apps" }));
    }

    /// <summary>The one line a group says about itself - what a failing test prints - names the type, then its five
    /// parts in the order they are declared, then the four counts and the two sums: every property the page reads is
    /// on the line, a number that is nothing is said as nothing between its name and the comma, and a number that is
    /// zero is said as zero.</summary>
    [Fact]
    public void TheGroupSaysItselfInOneLineOfItsPartsAndItsSums()
    {
        var text = new NamespaceGroup(
            "apps",
            null,
            true,
            [Row("web", PodState.Ready, cpu: new ResourceUse(0, null, null), memory: ResourceUse.None)],
            []).ToString();

        Assert.StartsWith("NamespaceGroup { Name = apps, ", text, StringComparison.Ordinal);
        Assert.Contains("Environment = , ", text, StringComparison.Ordinal);
        Assert.Contains("Found = True, ", text, StringComparison.Ordinal);
        Assert.Contains("Ready = 1, ", text, StringComparison.Ordinal);
        Assert.Contains("Total = 1, ", text, StringComparison.Ordinal);
        Assert.Contains("Finished = 0, ", text, StringComparison.Ordinal);
        Assert.Contains("Unhealthy = 0, ", text, StringComparison.Ordinal);
        Assert.Contains("Restarts = 0, ", text, StringComparison.Ordinal);
        Assert.Contains("CpuUsage = 0, ", text, StringComparison.Ordinal);
        // The last part is a sum that is nothing, so the line ends with its name, the space of the value it does not
        // have, and the brace.
        Assert.EndsWith("MemoryUsage =  }", text, StringComparison.Ordinal);

        // The order the parts are declared in is the order they are said in, and the sums come after all five.
        string[] members =
        [
            "Name = ", "Environment = ", "Found = ", "Pods = ", "Volumes = ",
            "Ready = ", "Total = ", "Finished = ", "Unhealthy = ", "Restarts = ", "CpuUsage = ", "MemoryUsage = ",
        ];
        var at = members.Select(member => text.IndexOf(member, StringComparison.Ordinal)).ToList();

        Assert.All(at, index => Assert.True(index >= 0, text));
        for (var i = 1; i < at.Count; i++)
        {
            Assert.True(at[i - 1] < at[i], $"{members[i - 1]} before {members[i]}: {text}");
        }
    }

    // ---------------------------------------------------------------------------- helpers

    /// <summary>A row of a workload, a state the test decides and the numbers the test asks for: the state is handed
    /// to the row rather than worked out of the pod, so that no group here needs a moment to be counted at. What the
    /// pod reports of itself is the fixture's, unless a usage or a restart count is asked for - and
    /// <see cref="ResourceUse.None" /> asks for a resource the pod says nothing about.</summary>
    private static PodRow Row(
        string workload,
        PodState state,
        int restarts = 0,
        ResourceUse? cpu = null,
        ResourceUse? memory = null) =>
        new(
            ClusterFixture.Pod(workload, restarts: restarts) with
            {
                Cpu = cpu ?? new ResourceUse(10, 100, 500),
                Memory = memory ?? new ResourceUse(104857600, 268435456, 536870912),
            },
            state);
}
