namespace Dashboard.Tests;

/// <summary>
/// The element words the parser reads by its named consts (#2128): a node, a region and a frame are named by their
/// alias without a <c>name</c>, a frame without a <c>kind</c> holds the empty word, a node's <c>url</c> that is no
/// absolute http or https address gets the refusal of the place it was asked at, and an unplaced entry falls back to
/// its <c>url</c> without <c>links.site</c>.
/// </summary>
public class RuntimeManifestParserElementWordTests
{
    [Theory]
    [InlineData("nodes", "a")]
    [InlineData("regions", "r")]
    [InlineData("frames", "f")]
    public void AnElementWithoutANameIsNamedByItsAlias(string list, string alias)
    {
        var json = $$"""{ "environment": "tdd", "{{list}}": [ { "alias": "{{alias}}" } ] }""";

        var result = RuntimeManifestParser.ParseManifest(json);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        if (list == "nodes")
        {
            Assert.Equal(alias, Assert.Single(result.Value!.Nodes).Name);
        }
        else if (list == "regions")
        {
            Assert.Equal(alias, Assert.Single(result.Value!.Regions).Name);
        }
        else
        {
            Assert.Equal(alias, Assert.Single(result.Value!.Frames!).Name);
        }
    }

    [Fact]
    public void AFrameWithoutAKindHoldsTheEmptyWord()
    {
        const string json = """{ "environment": "tdd", "frames": [ { "alias": "f" } ] }""";

        var result = RuntimeManifestParser.ParseManifest(json);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Equal(string.Empty, Assert.Single(result.Value!.Frames!).Kind);
    }

    [Fact]
    public void ANodeUrlThatIsNoAbsoluteHttpOrHttpsAddressGetsTheRefusalOfItsPlace()
    {
        const string json = """{ "environment": "tdd", "nodes": [ { "alias": "a", "url": "not an address" } ] }""";

        var result = RuntimeManifestParser.ParseManifest(json);

        Assert.False(result.IsValid);
        Assert.Equal(["nodes[0].url: not an absolute http or https address."], result.Errors);
    }

    [Fact]
    public void AnUnplacedEntryWithoutASiteLinkFallsBackToItsUrl()
    {
        const string json = """{ "environment": "tdd", "unplaced": [ { "name": "n", "url": "https://example.com/" } ] }""";

        var result = RuntimeManifestParser.ParseManifest(json);

        Assert.True(result.IsValid);
        var unplaced = Assert.Single(result.Value!.Unplaced!);
        Assert.Equal("n", unplaced.Name);
        Assert.Equal(new Uri("https://example.com/"), unplaced.Site);
        Assert.Null(unplaced.Portal);
    }
}
