namespace Dashboard.Tests;

/// <summary>
/// The two arms of the builder that share its private <c>UnprobedNodeTile</c>: a resource, and a node of a kind the
/// page does not check that has no address. Without Resource Health, both say "Not probed" under their name, no line.
/// </summary>
public class RuntimePayloadUnprobedNodeTests
{
    private static readonly Uri Page = new("http://localhost:5210/");

    [Theory]
    [InlineData(RuntimeNodeKind.Resource)]
    [InlineData(RuntimeNodeKind.Other)]
    public void ANodeWithNothingOfItsOwnToSayIsNotProbedUnderItsName(RuntimeNodeKind kind)
    {
        var manifest = new RuntimeManifest(
            "uat",
            [
                new RuntimeNode("browser", RuntimeNodeKind.Person, "Browser"),
                new RuntimeNode("thing", kind, "kv-cmdemo1"),
            ],
            [],
            []);

        var tile = RuntimePayloadBuilder.Build(manifest, null, Page, TimeZoneInfo.Utc).Nodes.Single(node => node.Alias == "thing");

        Assert.Equal(("neutral", "Not probed"), (tile.State, tile.Label));
        Assert.Empty(tile.Lines);
        Assert.Equal("kv-cmdemo1", tile.Title);
    }
}
