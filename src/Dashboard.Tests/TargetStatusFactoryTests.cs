namespace Dashboard.Tests;

/// <summary>How a tile is built from a node and from a deployable (#1006): each part lands where it belongs.</summary>
public class TargetStatusFactoryTests
{
    private static readonly Uri Door = new("https://ui.example.net");

    private static readonly Uri NodeUrl = new("https://app-westus3.example.net");

    private static LinkSet Links(string url) => new([new KeyValuePair<string, Uri>(LinkSet.Portal, new Uri(url))]);

    private static DeployableInfo Deployable(Uri? frontDoor, LinkSet? links = null, params NodeInfo[] nodes) =>
        new("ui", frontDoor, "/_healthcheck", "/alive", "/_version", nodes, Links: links);

    [Fact]
    public void ANodeTileTakesEachPartFromItsOwnPartOfTheNode()
    {
        var links = Links("https://portal.example.net/node");
        var node = new NodeInfo("app-westus3", "westus3", NodeInfo.StandbyRole, NodeUrl, links);

        var target = TargetStatus.ForNode(node);

        Assert.Equal(TargetKind.Node, target.Kind);
        Assert.Equal("app-westus3", target.Name);
        Assert.Equal(NodeUrl, target.Url);
        Assert.Equal("westus3", target.Region);
        Assert.Equal(NodeInfo.StandbyRole, target.Role);
        Assert.False(target.IsPrimary);
        Assert.Same(links, target.Links);
        Assert.Equal("westus3", target.Label);
    }

    [Fact]
    public void ANodeWithoutRegionOrLinksIsLabelledByItsName()
    {
        var target = TargetStatus.ForNode(new NodeInfo("app", null, NodeInfo.PrimaryRole, NodeUrl));

        Assert.Null(target.Region);
        Assert.True(target.IsPrimary);
        Assert.Same(LinkSet.None, target.Links);
        Assert.Equal("app", target.Label);
    }

    [Fact]
    public void AFrontDoorTileIsNamedOnceAndCarriesTheDeployablesLinks()
    {
        var links = Links("https://portal.example.net/profile");

        var target = TargetStatus.ForFrontDoor(Deployable(Door, links));

        Assert.NotNull(target);
        Assert.Equal(TargetKind.FrontDoor, target.Kind);
        Assert.Equal(TargetStatus.FrontDoorName, target.Name);
        Assert.Equal("Front Door", target.Label);
        Assert.Equal(Door, target.Url);
        Assert.Null(target.Region);
        Assert.Null(target.Role);
        Assert.Same(links, target.Links);
    }

    [Fact]
    public void ADeployableWithoutFrontDoorHasNoFrontDoorTile() =>
        Assert.Null(TargetStatus.ForFrontDoor(Deployable(null)));

    [Fact]
    public void TheFactoriesRejectNull()
    {
        Assert.Throws<ArgumentNullException>(() => TargetStatus.ForNode(null!));
        Assert.Throws<ArgumentNullException>(() => TargetStatus.ForFrontDoor(null!));
    }

    [Fact]
    public void ADeployableStatusBuildsItsTilesWithTheFactories()
    {
        var primary = new NodeInfo("app-eastus2", "eastus2", NodeInfo.PrimaryRole, new Uri("https://app-eastus2.example.net"));
        var standby = new NodeInfo("app-westus3", "westus3", NodeInfo.StandbyRole, NodeUrl);

        var status = new DeployableStatus(Deployable(Door, null, primary, standby));

        Assert.Equal(TargetStatus.FrontDoorName, status.FrontDoor?.Name);
        Assert.Equal(["eastus2", "westus3"], status.Nodes.Select(node => node.Region));
        Assert.Equal([NodeInfo.PrimaryRole, NodeInfo.StandbyRole], status.Nodes.Select(node => node.Role));
        Assert.Equal(["Front Door", "eastus2", "westus3"], status.Targets.Select(target => target.Label));
    }

    [Fact]
    public void NoCallSiteHandsARegionAndARolePositionally()
    {
        var source = File.ReadAllText(SourcePath("src", "Dashboard", "Health", "TargetStatus.cs"));

        Assert.DoesNotContain("node.Region, node.Role", source, StringComparison.Ordinal);
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(source, "\"Front Door\""));
    }

    private static string SourcePath(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Dashboard.sln")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return Path.Combine([directory.FullName, .. parts]);
    }
}
