namespace Dashboard.Tests;

/// <summary>
/// <see cref="NodeRole.Default" /> is the one place that gives a node with no role of its own its role: the first node
/// read is the primary and every node read after it a standby. It counts the nodes that were read, not the entries
/// of the file the reader refused, so a refused first entry does not leave a deployable without a primary (#1040).
/// </summary>
public class NodeRoleDefaultTests
{
    [Fact]
    public void TheFirstNodeReadIsThePrimary()
    {
        Assert.Equal(NodeInfo.PrimaryRole, NodeRole.Default(0));
        Assert.True(NodeRole.IsPrimary(NodeRole.Default(0)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(17)]
    public void EveryNodeReadAfterTheFirstIsAStandby(int nodesReadBefore)
    {
        Assert.Equal(NodeInfo.StandbyRole, NodeRole.Default(nodesReadBefore));
        Assert.False(NodeRole.IsPrimary(NodeRole.Default(nodesReadBefore)));
    }

    /// <summary>A refused entry before the first good node is still an error of the file, named by its place in the
    /// file - and the file is refused whole, so no deployable is drawn without its primary.</summary>
    [Theory]
    [InlineData("7", "environments[0].deployables[0].nodes[0]: not an object.")]
    [InlineData("""{ "url": "ftp://a.example.net" }""",
        "environments[0].deployables[0].nodes[0].url: missing or not an absolute http or https address.")]
    public void ARefusedFirstEntryIsRefusedByItsPlaceInTheFile(string refused, string expected)
    {
        var result = TopologyParser.Parse($$"""
            { "environments": [ { "name": "tdd", "deployables": [ { "nodes": [
              {{refused}}, { "url": "https://b.example.net" } ] } ] } ] }
            """);

        Assert.False(result.IsValid);
        Assert.Equal([expected], result.Errors);
    }

    /// <summary>The nodes that are read keep the order of the file: with no role anywhere the first is the primary and
    /// the rest standbys, the same answer the count of the nodes read gives.</summary>
    [Fact]
    public void TheParserGivesTheRoleTheDefaultGives()
    {
        var nodes = TopologyParser.Parse("""
            { "environments": [ { "name": "tdd", "deployables": [ { "nodes": [
              { "url": "https://a.example.net" }, { "url": "https://b.example.net" }, { "url": "https://c.example.net" } ] } ] } ] }
            """).Topology!.Environments[0].Deployables[0].Nodes;

        Assert.Equal([NodeRole.Default(0), NodeRole.Default(1), NodeRole.Default(2)], nodes.Select(node => node.Role).ToArray());
        Assert.Single(nodes, node => node.IsPrimary);
    }
}
