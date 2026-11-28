using System.Text.Json;
using Dashboard.Health;
using Dashboard.Runtime;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="NodeRole.Read" /> is the one reading of a node's role from a file: the file's <c>role</c> word in lower
/// case, null when there is none. Both the topology and the runtime manifest ask it, so they cannot disagree about
/// the same node.
/// </summary>
public class NodeRoleReadTests
{
    [Theory]
    [InlineData("""{"role":"Primary"}""", "primary")]
    [InlineData("""{"role":"STANDBY"}""", "standby")]
    [InlineData("""{"role":"standby"}""", "standby")]
    public void TheRoleIsReadInLowerCase(string json, string expected)
    {
        var element = ParseObject(json);

        Assert.Equal(expected, NodeRole.Read(element));
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("""{"role":null}""")]
    [InlineData("""{"role":""}""")]
    [InlineData("""{"role":"  "}""")]
    [InlineData("""{"role":42}""")]
    public void AMissingNullOrBlankOrNonStringRoleIsReadAsNull(string json)
    {
        var element = ParseObject(json);

        Assert.Null(NodeRole.Read(element));
    }

    [Fact]
    public void ATopologyNodeAndARuntimeManifestNodeWithTheSameMixedCaseRoleEndUpWithTheSameRoleWord()
    {
        const string MixedCase = "PrImArY";

        var topologyNode = TopologyParser.Parse($$"""
            { "environments": [ { "name": "tdd", "deployables": [ { "nodes": [
              { "name": "app", "region": "westus3", "role": "{{MixedCase}}", "url": "https://a.example.net" } ] } ] } ] }
            """).Topology!.Environments[0].Deployables[0].Nodes[0];

        var manifestNode = RuntimeManifestParser.ParseManifest($$"""
            { "environment": "tdd", "nodes": [
              { "alias": "app", "kind": "webapp", "name": "app", "region": "westus3", "role": "{{MixedCase}}" } ] }
            """).Value!.Nodes[0];

        Assert.Equal(topologyNode.Role, manifestNode.Role);
        Assert.Equal(NodeInfo.PrimaryRole, topologyNode.Role);
        Assert.True(topologyNode.IsPrimary);
    }

    [Fact]
    public void ATopologyNodeWithNoRoleStillGetsTheDefault()
    {
        var nodes = TopologyParser.Parse("""
            { "environments": [ { "name": "tdd", "deployables": [ { "nodes": [
              { "url": "https://a.example.net" }, { "url": "https://b.example.net" } ] } ] } ] }
            """).Topology!.Environments[0].Deployables[0].Nodes;

        Assert.Equal(NodeInfo.PrimaryRole, nodes[0].Role);
        Assert.Equal(NodeInfo.StandbyRole, nodes[1].Role);
    }

    [Fact]
    public void ARuntimeManifestNodeWithNoRoleHasNoRoleAndNoFallback()
    {
        var node = RuntimeManifestParser.ParseManifest("""
            { "environment": "tdd", "nodes": [ { "alias": "app", "kind": "webapp" } ] }
            """).Value!.Nodes[0];

        Assert.Null(node.Role);
        Assert.Null(node.Region);
    }

    private static JsonElement ParseObject(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
