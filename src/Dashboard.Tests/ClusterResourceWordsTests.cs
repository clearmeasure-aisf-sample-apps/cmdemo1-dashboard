namespace Dashboard.Tests;

/// <summary>
/// Pins the words the cluster's file uses for a node's or a pod's resource objects (#2240): the CPU and the memory
/// of a node and of a pod read the same <see cref="ResourceUse" />, the node also reads its allocatable, a resource
/// the file does not say - or says in the wrong case, or says as anything but an object - reads
/// <see cref="ResourceUse.None" />, and the ordinal word <c>CPU</c> is not the word the file gives its CPU in.
/// The readers are internal, so every reading here goes through the public <see cref="ClusterStatus.Parse" />.
/// </summary>
public class ClusterResourceWordsTests
{
    private static ClusterStatus Read(string json) => ClusterFixture.Read(json);

    /// <summary>
    /// A node and a pod in the same file, each with a <c>cpu</c> and a <c>memory</c> object, read the same
    /// <see cref="ResourceUse.Usage" />/<see cref="ResourceUse.Requests" />/<see cref="ResourceUse.Limits" />, and
    /// the node - and only the node - also reads its <see cref="ResourceUse.Allocatable" />.
    /// </summary>
    [Fact]
    public void ANodeAndAPodReadTheSameResourceUnderTheSameWordsAndTheNodeAlsoReadsItsAllocatable()
    {
        var status = Read("""
            { "nodes": [ { "name": "node-1",
                           "cpu":    { "usage": 812, "requests": 2450, "limits": 9100, "allocatable": 3860 },
                           "memory": { "usage": 9876543210, "requests": 6200000000, "limits": 11800000000, "allocatable": 13400000000 } } ],
              "namespaces": [ { "name": "apps", "pods": [ { "name": "web-1",
                                                            "cpu":    { "usage": 812, "requests": 2450, "limits": 9100 },
                                                            "memory": { "usage": 9876543210, "requests": 6200000000, "limits": 11800000000 } } ] } ] }
            """);

        var node = Assert.Single(status.Nodes);
        var pod = Assert.Single(status.Pods).Pod;

        Assert.Equal(ResourceUses.Node(usage: 812, requests: 2450, allocatable: 3860, limits: 9100), node.Cpu);
        Assert.Equal(ResourceUses.Node(usage: 9876543210, requests: 6200000000, allocatable: 13400000000, limits: 11800000000), node.Memory);
        Assert.Equal(ResourceUses.Pod(usage: 812, requests: 2450, limit: 9100), pod.Cpu);
        Assert.Equal(ResourceUses.Pod(usage: 9876543210, requests: 6200000000, limit: 11800000000), pod.Memory);
    }

    /// <summary>The words the file uses for its resources are ordinal: <c>CPU</c> and <c>Memory</c> are not the words
    /// the file gives its CPU and memory in, so the resource under a wrong-case spelling is read as nothing. The
    /// resource under the correctly-spelled word beside it is still read - the two readings do not leak into each
    /// other.</summary>
    [Theory]
    [InlineData("CPU", "memory", false, true)]
    [InlineData("cpu", "Memory", true, false)]
    [InlineData("CPU", "Memory", false, false)]
    public void AResourceWordInTheWrongCaseIsNotTheResourceItIsSpelledLike(string cpuWord, string memoryWord, bool cpuReads, bool memoryReads)
    {
        var status = Read($$"""
            { "nodes": [ { "name": "node-1", "{{cpuWord}}": { "usage": 1 }, "{{memoryWord}}": { "usage": 2 } } ],
              "namespaces": [ { "name": "apps", "pods": [ { "name": "web-1", "{{cpuWord}}": { "usage": 1 }, "{{memoryWord}}": { "usage": 2 } } ] } ] }
            """);

        var expectedCpu = cpuReads ? ResourceUses.Pod(usage: 1) : ResourceUse.None;
        var expectedMemory = memoryReads ? ResourceUses.Pod(usage: 2) : ResourceUse.None;
        var node = Assert.Single(status.Nodes);
        var pod = Assert.Single(status.Pods).Pod;

        Assert.Equal(expectedCpu, node.Cpu);
        Assert.Equal(expectedMemory, node.Memory);
        Assert.Equal(expectedCpu, pod.Cpu);
        Assert.Equal(expectedMemory, pod.Memory);
    }

    /// <summary>A resource the file does not say, or says as anything but an object (a number, a string, an array),
    /// reads <see cref="ResourceUse.None" />: the node and the pod alike, and for both words at once.</summary>
    [Theory]
    [InlineData("4", "\"text\"")]
    [InlineData("[ 1 ]", "{}")]
    public void AResourceOfAnotherTypeIsNoResource(string cpuValue, string memoryValue)
    {
        var status = Read($$"""
            { "nodes": [ { "name": "node-1", "cpu": {{cpuValue}}, "memory": {{memoryValue}} } ],
              "namespaces": [ { "name": "apps", "pods": [ { "name": "web-1", "cpu": {{cpuValue}}, "memory": {{memoryValue}} } ] } ] }
            """);

        var node = Assert.Single(status.Nodes);
        var pod = Assert.Single(status.Pods).Pod;

        Assert.Equal(ResourceUse.None, node.Cpu);
        Assert.Equal(ResourceUse.None, node.Memory);
        Assert.Equal(ResourceUse.None, pod.Cpu);
        Assert.Equal(ResourceUse.None, pod.Memory);
    }

    /// <summary>A node or a pod with no <c>cpu</c> or <c>memory</c> member at all reads <see cref="ResourceUse.None" />.</summary>
    [Fact]
    public void ANodeAndAPodWithoutAResourceMemberReadNoResource()
    {
        var status = Read("""
            { "nodes": [ { "name": "node-1" } ],
              "namespaces": [ { "name": "apps", "pods": [ { "name": "web-1" } ] } ] }
            """);

        var node = Assert.Single(status.Nodes);
        var pod = Assert.Single(status.Pods).Pod;

        Assert.Equal(ResourceUse.None, node.Cpu);
        Assert.Equal(ResourceUse.None, node.Memory);
        Assert.Equal(ResourceUse.None, pod.Cpu);
        Assert.Equal(ResourceUse.None, pod.Memory);
    }

    /// <summary>The node's <c>pods</c> section keeps its own count and capacity reads, which are not resource reads
    /// and so are not the words this change names.</summary>
    [Fact]
    public void ANodePodsCountAndCapacityAreStillReadFromItsPodsSection()
    {
        var status = Read("""
            { "nodes": [ { "name": "node-1", "pods": { "count": 63, "capacity": 110 } } ] }
            """);

        var node = Assert.Single(status.Nodes);

        Assert.Equal((63, 110), (node.Pods, node.PodCapacity));
    }
}
