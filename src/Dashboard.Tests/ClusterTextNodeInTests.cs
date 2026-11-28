namespace Dashboard.Tests;

public class ClusterTextNodeInTests
{
    private static readonly DateTimeOffset Now = ClusterFixture.Generated;

    [Fact]
    public void ALongNodeNameIsShortenedInTheSentence()
    {
        Assert.Equal("Node aks-…000000", ClusterText.NodeIn("aks-system-12148983-vmss000000"));
    }

    [Fact]
    public void AShortNodeNameIsKeptWholeInTheSentence()
    {
        Assert.Equal("Node node1", ClusterText.NodeIn("node1"));
    }

    [Fact]
    public void ANullNameThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ClusterText.NodeIn(null!));
    }

    [Fact]
    public void ProblemsForANotReadyNodeStartWithNodeIn()
    {
        var status = ClusterFixture.Status(nodes:
        [
            ClusterFixture.Node(name: "aks-system-12148983-vmss000000", ready: false),
        ]);

        var problems = ClusterAssessment.Problems(status, Now);

        Assert.Contains(problems, p => p.StartsWith(ClusterText.NodeIn("aks-system-12148983-vmss000000"), StringComparison.Ordinal));
        Assert.Contains(problems, p => p.StartsWith("Node aks-…000000 is not ready", StringComparison.Ordinal));
    }

    [Fact]
    public void ProblemsForAReadyNodeUnderMemoryPressureStartWithNodeIn()
    {
        var status = ClusterFixture.Status(nodes:
        [
            ClusterFixture.Node("aks-system-12148983-vmss000000", ready: true, "MemoryPressure"),
        ]);

        var problems = ClusterAssessment.Problems(status, Now);

        Assert.Contains(problems, p => p.StartsWith(ClusterText.NodeIn("aks-system-12148983-vmss000000"), StringComparison.Ordinal));
        Assert.Contains(problems, p => p.StartsWith("Node aks-…000000: memory pressure", StringComparison.Ordinal));
    }
}
