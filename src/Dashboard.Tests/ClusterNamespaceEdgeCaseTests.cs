namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ClusterNamespace" />: the parts the namespace
/// keeps of what it is given, the parts it falls apart into, the words it says its parts in, the copies
/// that turn one of them, and the parts that are one and the parts that are not.
/// <see cref="ParsedEdgeCaseTests" /> pins the namespace without a pod and without a volume - the two empty
/// lists it says so with, and the name it stands on - and that the namespace is one while its three parts
/// are, and the pod gained and the volume gained, each, is another one, and the copy that names no part is
/// it again, not the same reference; <see cref="ClusterParserTests" /> pins the sample file's namespaces at
/// the values they stand on and the namespace the parser leaves out where the file names no name; and
/// <see cref="NamespaceGroupTests" /> pins the groups the namespaces make, not the namespace itself.
/// <para>
/// What is pinned here is what they leave open, and only that: the name the namespace keeps as the word it
/// is, at the ends the word stands in and at the length it stands on - the empty word kept as the empty
/// word it is, not the nothing it is not, and the spaces about a word kept as the spaces they are, since
/// the namespace is not the parser, and does not drop what is empty and does not trim what has spaces
/// about it - and the pods and the volumes as the very lists they are, in the order they stand in, not
/// copies of them; the three parts it falls apart into, each as the value it stands on, the empty word and
/// the empty list coming out as they stand; the words the record says its parts in - the type the list
/// stands on, not the pods or the volumes in it, and the empty word and the empty list said the way it
/// says them; the copy that turns one of the parts it keeps and keeps the rest it does not; and the parts
/// that turn two namespaces apart - the name, the pods and the volumes each turned - and the list the
/// namespace keeps: the same list is one, and the different list is another namespace while the parts it
/// keeps are the same, the word in the list turned, the order of the list turned about and the list made
/// empty each included.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every namespace is one the test
/// builds.
/// </para>
/// </summary>
public class ClusterNamespaceEdgeCaseTests
{
    // ----- The parts the namespace keeps -----

    /// <summary>
    /// The namespace keeps the very parts it is given: the name as the word it is, and the pods and the
    /// volumes as the very lists they are - the same lists, not copies of them - in the order they stand
    /// in, each pod and each volume the one it is.
    /// </summary>
    [Fact]
    public void ANamespaceKeepsTheVeryPartsItIsGiven()
    {
        IReadOnlyList<ClusterPod> pods = [ClusterFixture.Pod("ui"), ClusterFixture.Pod("db")];
        IReadOnlyList<ClusterVolume> volumes = [new ClusterVolume("data", 8589934592d, "Bound"), new ClusterVolume("cache", null, "Pending")];
        var space = new ClusterNamespace("apps", pods, volumes);

        Assert.Equal("apps", space.Name);
        Assert.Same(pods, space.Pods);
        Assert.Same(volumes, space.Volumes);
        Assert.Equal(2, space.Pods.Count);
        Assert.Equal(2, space.Volumes.Count);
        Assert.Same(pods[0], space.Pods[0]);
        Assert.Same(pods[1], space.Pods[1]);
        Assert.Same(volumes[0], space.Volumes[0]);
        Assert.Same(volumes[1], space.Volumes[1]);
    }

    /// <summary>
    /// The name stands at the word it is, at the ends the word stands in: the empty word and the word that
    /// is only the space it is kept as the word they are - the namespace is not the parser, and does not
    /// drop what is empty and does not trim what has spaces about it.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  apps  ")]
    public void ANameStandsAtTheWordItIsAtTheEndsTheWordStandsIn(string name)
    {
        var space = new ClusterNamespace(name, [], []);

        Assert.Equal(name, space.Name);
    }

    /// <summary>
    /// The name stands at the length it stands on: the long word the name holds kept as the word it is,
    /// neither shortened nor wrapped, and the one letter it is kept as the letter it is.
    /// </summary>
    [Fact]
    public void ANameStandsAtTheLengthItStandsOn()
    {
        var longName = new string('n', 128);
        var one = new ClusterNamespace("a", [], []);

        Assert.Equal(longName, new ClusterNamespace(longName, [], []).Name);
        Assert.Equal("a", one.Name);
    }

    // ----- The parts the namespace falls apart into -----

    /// <summary>
    /// The namespace falls apart into its three parts, each as the value it stands on, in the order the
    /// namespace names them - the word it stands on and the lists as the very lists they are - and the
    /// empty word and the empty list come out as they stand, not the nothing they are not.
    /// </summary>
    [Fact]
    public void ANamespaceFallsApartIntoItsThreeParts()
    {
        IReadOnlyList<ClusterPod> pods = [ClusterFixture.Pod("ui")];
        IReadOnlyList<ClusterVolume> volumes = [new ClusterVolume("data", 8589934592d, "Bound")];
        var space = new ClusterNamespace("apps", pods, volumes);

        var (name, spacePods, spaceVolumes) = space;

        Assert.Equal("apps", name);
        Assert.Same(pods, spacePods);
        Assert.Same(volumes, spaceVolumes);

        var (emptyName, noPods, noVolumes) = new ClusterNamespace("", [], []);

        Assert.Equal(string.Empty, emptyName);
        Assert.Empty(noPods);
        Assert.Empty(noVolumes);
    }

    // ----- The words the namespace says its parts in -----

    /// <summary>
    /// The words the namespace says its parts in: the record's words, in the order the parts are named,
    /// the namespace's own name at the front, and the lists it was given as the words they stand on - the
    /// type the list stands on, not the pods or the volumes in it - and the empty word said the way it
    /// says it, the name that is nothing said with nothing after it.
    /// </summary>
    [Fact]
    public void ANamespaceSaysItsPartsInTheWordsTheRecordSaysThem()
    {
        ClusterPod[] pods = { ClusterFixture.Pod("ui"), ClusterFixture.Pod("db") };
        ClusterVolume[] volumes = { new ClusterVolume("data", 8589934592d, "Bound") };
        var space = new ClusterNamespace("apps", pods, volumes);

        Assert.Equal(
            "ClusterNamespace { Name = apps, Pods = Dashboard.Cluster.ClusterPod[], Volumes = Dashboard.Cluster.ClusterVolume[] }",
            space.ToString());

        Assert.Equal(
            "ClusterNamespace { Name = , Pods = Dashboard.Cluster.ClusterPod[], Volumes = Dashboard.Cluster.ClusterVolume[] }",
            new ClusterNamespace("", [], []).ToString());
    }

    // ----- The copies -----

    /// <summary>
    /// The copy of a namespace that turns one of the parts it keeps keeps the rest: the name turned alone
    /// keeps the lists it does not turn, and the pods turned and the volumes turned each keeps the rest it
    /// does not turn - the parts the copy does not turn stand as they stood, and the copy is not the
    /// namespace it is the copy of.
    /// </summary>
    [Fact]
    public void ACopyThatTurnsOneOfThePartsItKeepsKeepsTheRest()
    {
        IReadOnlyList<ClusterPod> pods = [ClusterFixture.Pod("ui")];
        IReadOnlyList<ClusterVolume> volumes = [new ClusterVolume("data", null, null)];
        var space = new ClusterNamespace("apps", pods, volumes);

        var renamed = space with { Name = "kube-system" };
        Assert.Equal("kube-system", renamed.Name);
        Assert.Same(pods, renamed.Pods);
        Assert.Same(volumes, renamed.Volumes);
        Assert.NotSame(space, renamed);

        var repodded = space with { Pods = [] };
        Assert.Empty(repodded.Pods);
        Assert.Equal("apps", repodded.Name);
        Assert.Same(volumes, repodded.Volumes);
        Assert.NotSame(space, repodded);

        var revolumed = space with { Volumes = [new ClusterVolume("cache", 0d, "Pending")] };
        Assert.Single(revolumed.Volumes);
        Assert.Equal("apps", revolumed.Name);
        Assert.Same(pods, revolumed.Pods);
        Assert.NotSame(space, revolumed);
    }

    // ----- The namespaces that are one and the namespaces that are not -----

    /// <summary>
    /// The namespaces that are one are one: the three parts the same - the name the word it is, the pods
    /// and the volumes the very lists they are - and the operators and the hash code that follow them, and
    /// the namespace asked of as an object the one its parts make it, and the nothing it is not asked of is
    /// not it.
    /// </summary>
    [Fact]
    public void TheNamespacesThatAreOneAreOneAndTheHashCodeFollows()
    {
        IReadOnlyList<ClusterPod> pods = [ClusterFixture.Pod("ui")];
        IReadOnlyList<ClusterVolume> volumes = [new ClusterVolume("data", 8589934592d, "Bound")];
        var one = new ClusterNamespace("apps", pods, volumes);
        var two = new ClusterNamespace("apps", pods, volumes);

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.True(one.Equals(two as object));
        Assert.True(two.Equals(one as object));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.False(one.Equals(null));
    }

    /// <summary>
    /// The parts that turn two namespaces apart: the name turned, the pods turned and the volumes turned,
    /// each, is another namespace - and the list the namespace keeps is the list it is: the lists whose
    /// parts stand the same way are not the list, and so the namespaces that keep them are not one, the
    /// word in the list turned, the order of the list turned about and the list made empty each included.
    /// </summary>
    [Fact]
    public void APartTurnedIsAnotherNamespaceAndADifferentListIsAnotherWhileItsPartsAreTheSame()
    {
        var one = ClusterFixture.Space("apps", ClusterFixture.Pod("ui"));

        Assert.True(one != one with { Name = "jobs" });
        Assert.NotEqual(one, one with { Name = "jobs" });
        Assert.NotEqual(one, one with { Pods = [ClusterFixture.Pod("ui")] });
        Assert.NotEqual(one, one with { Pods = [ClusterFixture.Pod("db")] });
        Assert.NotEqual(one, one with { Pods = [] });
        Assert.NotEqual(one, one with { Volumes = [new ClusterVolume("data", null, null)] });

        var first = new ClusterVolume("data", 8589934592d, "Bound");
        Assert.NotEqual(
            new ClusterNamespace("apps", [ClusterFixture.Pod("ui")], [first]),
            new ClusterNamespace("apps", [ClusterFixture.Pod("ui")], [new ClusterVolume("data", 8589934592d, "Bound")]));

        IReadOnlyList<ClusterPod> order = [ClusterFixture.Pod("ui"), ClusterFixture.Pod("db")];
        Assert.NotEqual(
            new ClusterNamespace("apps", order, []),
            new ClusterNamespace("apps", [ClusterFixture.Pod("db"), ClusterFixture.Pod("ui")], []));
    }
}
