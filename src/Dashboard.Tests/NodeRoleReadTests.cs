using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="NodeRole.Read" /> is the one reader of a node's role word: the word under <c>role</c>, trimmed and
/// lowercased, null when the element names none. The reader of <c>topology.json</c> and the reader of the manifest
/// both ask it, so the two cannot drift on which node is the primary (#2281).
/// </summary>
public class NodeRoleReadTests
{
    private static JsonElement Element(string json) =>
        JsonDocument.Parse(json).RootElement;

    [Fact]
    public void TheRoleWordIsTrimmedAndLowercased()
    {
        Assert.Equal("primary", NodeRole.Read(Element("""{ "role": "Primary" }""")));
    }

    [Fact]
    public void ARolePaddedWithSpacesIsReadByItsWord()
    {
        Assert.Equal("standby", NodeRole.Read(Element("""{ "role": "  STANDBY  "}""")));
    }

    [Theory]
    [InlineData("""{}""")]
    [InlineData("""{ "role": "" }""")]
    [InlineData("""{ "role": "   " }""")]
    [InlineData("""{ "role": 7 }""")]
    public void ARoleTheElementNamesNoWordIsNull(string json)
    {
        Assert.Null(NodeRole.Read(Element(json)));
    }

    [Fact]
    public void AnElementThatIsNoObjectNamesNoRole()
    {
        Assert.Null(NodeRole.Read(Element("7")));
        Assert.Null(NodeRole.Read(Element("\"standby\"")));
    }

    [Fact]
    public void TheWordsAreTheWordsBothFilesWrite()
    {
        Assert.Equal("role", NodeRole.RoleWord);
        Assert.Equal("region", NodeRole.RegionWord);
    }

    /// <summary>The same node's role word, written once, gives the same role through the reader of <c>topology.json</c>
    /// and through the reader of the manifest: one read of the word, so the topology page and the runtime view cannot
    /// disagree on which node is the primary.</summary>
    [Fact]
    public void TheTopologyAndTheManifestReadTheSameRoleOutOfTheSameNode()
    {
        const string Role = "\"role\": \"  Primary  \"";
        var topologyRole = TopologyParser.Parse($$"""
            { "environments": [ { "name": "tdd", "deployables": [ { "nodes": [
              { "url": "https://app.example.net", {{Role}} } ] } ] } ] }
            """).Topology!.Environments[0].Deployables[0].Nodes[0].Role;

        var manifestRole = RuntimeManifestParser.ParseManifest($$"""
            { "environment": "tdd", "nodes": [ { "alias": "app", {{Role}} } ] }
            """).Value!.Nodes[0].Role;

        Assert.Equal(topologyRole, manifestRole);
        Assert.Equal("primary", topologyRole);
    }
}
