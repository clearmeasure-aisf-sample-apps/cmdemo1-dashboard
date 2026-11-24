namespace Dashboard.Tests;

/// <summary>
/// The guard of <see cref="ClusterNamespace" />'s constructor: a null list of pods or of volumes is refused at
/// construction with an <see cref="ArgumentNullException" /> naming the part, instead of a later
/// <see cref="NullReferenceException" />; every non-null list, the empty one included, is kept as it is.
/// </summary>
public class ClusterNamespaceValidationTests
{
    [Fact]
    public void NullPodsAreRefusedWithTheirName()
    {
        var thrown = Assert.Throws<ArgumentNullException>(() => new ClusterNamespace("apps", null!, []));

        Assert.Equal("Pods", thrown.ParamName);
    }

    [Fact]
    public void NullVolumesAreRefusedWithTheirName()
    {
        var thrown = Assert.Throws<ArgumentNullException>(() => new ClusterNamespace("apps", [], null!));

        Assert.Equal("Volumes", thrown.ParamName);
    }

    [Fact]
    public void BothNullRefuseThePodsFirst()
    {
        var thrown = Assert.Throws<ArgumentNullException>(() => new ClusterNamespace("apps", null!, null!));

        Assert.Equal("Pods", thrown.ParamName);
    }

    [Fact]
    public void EmptyListsAreKeptAsTheyAre()
    {
        IReadOnlyList<ClusterPod> pods = [];
        IReadOnlyList<ClusterVolume> volumes = [];

        var space = new ClusterNamespace("apps", pods, volumes);

        Assert.Same(pods, space.Pods);
        Assert.Same(volumes, space.Volumes);
    }

    [Fact]
    public void TheNamespaceStillDeconstructsAndCopies()
    {
        IReadOnlyList<ClusterPod> pods = [ClusterFixture.Pod("ui")];
        IReadOnlyList<ClusterVolume> volumes = [new ClusterVolume("data", 1d, "Bound")];
        var space = new ClusterNamespace("apps", pods, volumes);

        var (name, keptPods, keptVolumes) = space;
        var copy = space with { Name = "jobs" };

        Assert.Equal("apps", name);
        Assert.Same(pods, keptPods);
        Assert.Same(volumes, keptVolumes);
        Assert.Same(pods, copy.Pods);
        Assert.Same(volumes, copy.Volumes);
        Assert.Equal(space, new ClusterNamespace("apps", pods, volumes));
    }

    [Fact]
    public void ParsedNamespacesWithoutPodsOrVolumesGetEmptyLists()
    {
        var parsed = ClusterStatus.Parse("""{"namespaces":[{"name":"apps"}]}""");

        var space = Assert.Single(parsed.Value!.Namespaces);
        Assert.Empty(space.Pods);
        Assert.Empty(space.Volumes);
    }
}
