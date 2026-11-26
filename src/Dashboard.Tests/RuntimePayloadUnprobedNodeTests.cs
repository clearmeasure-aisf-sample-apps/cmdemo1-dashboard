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

    /// <summary>
    /// The fallback with a reason (#1709): unlike the reason-free case above, a node the page has worked out a reason
    /// for is titled with its name, a colon and that reason, and keeps its own label and line.
    /// </summary>
    [Theory]
    [InlineData(RuntimeNodeKind.ContainerApp, "No address", "no public address known", "ca-internal: a container app without a public address this page knows: the browser cannot ask it.")]
    [InlineData(RuntimeNodeKind.Sql, "Not probed", "not probed from the browser", "ca-internal: the database takes no call from a browser, and this page checks no web app that uses it.")]
    public void ANodeWithAReasonIsTitledWithItsNameAndThatReason(RuntimeNodeKind kind, string label, string line, string title)
    {
        var manifest = new RuntimeManifest(
            "uat",
            [
                new RuntimeNode("browser", RuntimeNodeKind.Person, "Browser"),
                new RuntimeNode("thing", kind, "ca-internal"),
            ],
            [],
            []);

        var tile = RuntimePayloadBuilder.Build(manifest, null, Page, TimeZoneInfo.Utc).Nodes.Single(node => node.Alias == "thing");

        Assert.Equal(("neutral", label), (tile.State, tile.Label));
        Assert.Equal(new RuntimeTileLine(line, "muted"), Assert.Single(tile.Lines));
        Assert.Equal(title, tile.Title);
        Assert.StartsWith("ca-internal: ", tile.Title, StringComparison.Ordinal);
    }
}
