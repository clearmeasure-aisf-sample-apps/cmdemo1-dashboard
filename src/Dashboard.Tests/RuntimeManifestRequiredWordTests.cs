namespace Dashboard.Tests;

/// <summary>
/// The one "required word of an element" read the four lists of a manifest share (#1813): the nodes, regions and
/// frames each need an <c>alias</c>, an edge needs its <c>id</c>, <c>from</c> and <c>to</c>. A missing or blank word
/// drops the element and adds the refusal of the place it was asked at, once per element, in the words the page says.
/// </summary>
public class RuntimeManifestRequiredWordTests
{
    [Theory]
    [InlineData("nodes")]
    [InlineData("regions")]
    [InlineData("frames")]
    public void ABlankAliasIsRefusedAtItsOwnPlaceAndTheOthersAreKept(string list)
    {
        var json = $$"""
            { "environment": "tdd", "{{list}}": [ { "alias": "a" }, { "alias": "  " }, { "alias": "c" } ] }
            """;

        var result = RuntimeManifestParser.ParseManifest(json);

        Assert.False(result.IsValid);
        Assert.Equal([$"{list}[1].alias: missing or empty."], result.Errors);
    }

    [Theory]
    [InlineData("""{ }""")]
    [InlineData("""{ "id": "e" }""")]
    [InlineData("""{ "from": "a", "to": "b" }""")]
    [InlineData("""{ "id": "e", "from": "a", "to": " " }""")]
    public void AnEdgeMissingAnyOfItsThreeWordsIsRefusedOnce(string edge)
    {
        var result = RuntimeManifestParser.ParseManifest($$"""{ "environment": "tdd", "edges": [ {{edge}} ] }""");

        Assert.Equal(["edges[0]: id, from and to are required."], result.Errors);
    }

    [Fact]
    public void TheRefusalsOfTheFourListsComeInTheOrderTheListsAreRead()
    {
        const string json = """
            {
              "environment": "tdd",
              "edges": [ { "id": "e", "from": "a", "to": "b" }, { "id": "f" } ],
              "frames": [ { } ],
              "regions": [ { "alias": "r" }, { } ],
              "nodes": [ { }, { "alias": "a" }, { "alias": "" } ]
            }
            """;

        var result = RuntimeManifestParser.ParseManifest(json);

        Assert.Equal(
            [
                "nodes[0].alias: missing or empty.",
                "nodes[2].alias: missing or empty.",
                "regions[1].alias: missing or empty.",
                "frames[0].alias: missing or empty.",
                "edges[1]: id, from and to are required.",
            ],
            result.Errors);
    }

    [Fact]
    public void EveryRequiredWordPresentReadsTheTrimmedWordsIntoTheManifest()
    {
        const string json = """
            {
              "environment": "tdd",
              "nodes": [ { "alias": " a " }, { "alias": "b" } ],
              "regions": [ { "alias": "r" } ],
              "frames": [ { "alias": "f" } ],
              "edges": [ { "id": " e ", "from": "a", "to": "b" } ]
            }
            """;

        var result = RuntimeManifestParser.ParseManifest(json);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Equal(["a", "b"], result.Value!.Nodes.Select(node => node.Alias));
        Assert.Equal("r", Assert.Single(result.Value.Regions).Alias);
        Assert.Equal("f", Assert.Single(result.Value.Frames!).Alias);
        var edge = Assert.Single(result.Value.Edges);
        Assert.Equal(("e", "a", "b"), (edge.Id, edge.From, edge.To));
    }
}
