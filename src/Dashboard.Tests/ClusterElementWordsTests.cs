using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// Pins the four shared element words of the cluster's status file (#2253): <c>ready</c> (a node's Ready condition
/// and a pod's all-containers-ready flag), <c>pods</c> (a node's pod-count object and a namespace's pod list),
/// <c>phase</c> (a pod's phase and a volume claim's phase) and <c>capacity</c> (a node's pod capacity and a volume
/// claim's size in bytes). They are named once on <see cref="ClusterStatus" /> as internal consts beside
/// <c>NodesWord</c> and <c>NamespacesWord</c>, so a mistyped copy in one reader cannot silently read
/// <c>false</c> / <c>ClusterPod.Unknown</c> / <c>null</c> while the true word sits untyped in the other.
/// <para>
/// The readers are internal and the app declares no <c>InternalsVisibleTo</c>, so every reading here goes through
/// the public <see cref="ClusterStatus.Parse" /> via <see cref="ClusterFixture.Read" />, as
/// <see cref="ClusterResourceWordsTests" /> does for the <c>cpu</c> and <c>memory</c> words; the consts' own text is
/// pinned by reflection, as <see cref="DeploymentStateWordsTests" /> does.
/// </para>
/// </summary>
public class ClusterElementWordsTests
{
    private static ClusterStatus Read(string json) => ClusterFixture.Read(json);

    /// <summary>The text of one internal const of <see cref="ClusterStatus" />, reached by reflection under its own name.</summary>
    private static string WordOf(string name)
    {
        var field = typeof(ClusterStatus).GetField(name, BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(field);
        Assert.True(field!.IsLiteral, $"{name} is a const.");
        Assert.False(field.IsPublic, $"{name} is internal, not public.");
        return (string)field.GetRawConstantValue()!;
    }

    /// <summary>
    /// A node and a pod in the same file, both with <c>"ready": true</c>, both read <see cref="ClusterNode.Ready" />
    /// and <see cref="ClusterPod.Ready" /> true. The one word stands behind both readings: without it, both would read
    /// the <c>false</c> the readers give a flag the file does not say.
    /// </summary>
    [Fact]
    public void ANodeAndAPodInTheSameFileBothReadTheirReadyFlag()
    {
        var status = Read("""
            { "nodes": [ { "name": "node-1", "ready": true } ],
              "namespaces": [ { "name": "apps", "pods": [ { "name": "web-1", "ready": true } ] } ] }
            """);

        var node = Assert.Single(status.Nodes);
        var pod = Assert.Single(status.Pods).Pod;

        Assert.True(node.Ready);
        Assert.True(pod.Ready);
    }

    /// <summary>A pod reads its <c>phase</c> (defaulting to <see cref="ClusterPod.Unknown" /> without the word), and a
    /// volume claim reads its <c>phase</c> (defaulting to <c>null</c> without it) - one word, two records, two
    /// different defaults: the pod that does not say its phase and the volume that does not say its are both in the
    /// same file.</summary>
    [Fact]
    public void APodAndAVolumeReadTheSamePhaseWordWithTheirOwnDefaults()
    {
        var status = Read("""
            { "namespaces": [ { "name": "apps",
                               "pods": [ { "name": "web-1", "phase": "Running" }, { "name": "quiet-1" } ],
                               "volumes": [ { "name": "data", "phase": "Bound" }, { "name": "silent" } ] } ] }
            """);

        var running = status.Pods.Single(p => p.Pod.Name == "web-1").Pod;
        var silentPod = status.Pods.Single(p => p.Pod.Name == "quiet-1").Pod;
        var bound = status.Namespaces.Single().Volumes.Single(v => v.Name == "data");
        var silentVolume = status.Namespaces.Single().Volumes.Single(v => v.Name == "silent");

        Assert.Equal("Running", running.Phase);
        Assert.Equal(ClusterPod.Unknown, silentPod.Phase);
        Assert.Equal("Bound", bound.Phase);
        Assert.Null(silentVolume.Phase);
    }

    /// <summary>
    /// A node's <c>pods</c> object reads its <c>count</c> and its <c>capacity</c>, a volume claim reads its
    /// <c>capacity</c> in bytes, and a namespace's <c>pods</c> list reads its pods: one word for the node's count
    /// object and the namespace's list, one word for the node's capacity and the volume's size.
    /// </summary>
    [Fact]
    public void ANodesPodsObjectAVolumeCapacityAndANamespacesPodsListAreAllRead()
    {
        var status = Read("""
            { "nodes": [ { "name": "node-1", "pods": { "count": 12, "capacity": 30 } } ],
              "namespaces": [ { "name": "apps",
                               "pods": [ { "name": "web-1" } ],
                               "volumes": [ { "name": "data", "capacity": 1073741824 } ] } ] }
            """);

        var node = Assert.Single(status.Nodes);
        var space = Assert.Single(status.Namespaces);
        var volume = Assert.Single(space.Volumes);

        Assert.Equal(12, node.Pods);
        Assert.Equal(30, node.PodCapacity);
        Assert.Equal(1073741824d, volume.Capacity);
        var pod = Assert.Single(space.Pods);
        Assert.Equal("web-1", pod.Name);
    }

    /// <summary>The words are ordinal: <c>"Ready"</c>, <c>"Phase"</c> and <c>"Capacity"</c> are not read by the node,
    /// the pod or the volume readers: the JSON words are looked up as the file spells them, so a
    /// wrong-case spelling reads as though the file did not say the word at all.</summary>
    [Theory]
    [InlineData("Ready")]
    [InlineData("Phase")]
    [InlineData("Capacity")]
    [InlineData("Pods")]
    public void AWordInTheWrongCaseIsNotRead(string wrongCaseWord)
    {
        var status = Read($$"""
            { "nodes": [ { "name": "node-1", "{{wrongCaseWord}}": true, "pods": { "count": 1, "{{wrongCaseWord}}": 30 } } ],
              "namespaces": [ { "name": "apps", "{{wrongCaseWord}}": [ { "name": "web-1", "{{wrongCaseWord}}": true, "{{wrongCaseWord}}": "Running" } ] } ] }
            """);

        var node = Assert.Single(status.Nodes);
        var space = Assert.Single(status.Namespaces);

        Assert.False(node.Ready);
        Assert.Null(node.PodCapacity);
        Assert.Empty(space.Pods);
    }

    /// <summary>The four consts hold the four words of the file contract: a rename of a const is a rename of the file
    /// word, so it shows up in the reading rather than hiding as an untyped literal in one reader.</summary>
    [Theory]
    [InlineData("ReadyWord", "ready")]
    [InlineData("PodsWord", "pods")]
    [InlineData("PhaseWord", "phase")]
    [InlineData("CapacityWord", "capacity")]
    public void EachElementWordConstIsTheTextOfItsFileWord(string name, string word)
    {
        Assert.Equal(word, WordOf(name));
    }
}
