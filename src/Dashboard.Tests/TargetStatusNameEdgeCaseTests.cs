namespace Dashboard.Tests;

/// <summary>
/// The two naming rules <see cref="TargetStatus" /> owns beside its <see cref="TargetStatus.Label" />: the web
/// app's name (<see cref="TargetStatus.ResourceName" />, the first label of the Front Door endpoint's host, a
/// node's own name) and the word in the tile's chip (<see cref="TargetStatus.Chip" />, <c>public address</c> for
/// the endpoint, the node's role or else <c>node</c>). Pinned here because no rendered-markup test can reach them
/// and nothing did before #2156 moved them out of <c>TargetTile.razor</c>.
/// </summary>
public sealed class TargetStatusNameEdgeCaseTests
{
    private static readonly Uri FrontDoor = new("https://fd-web-abc.z01.azurefd.net/");
    private static readonly Uri Dotless = new("https://localhost/");
    private static readonly Uri West = new("https://app-west.example.net");

    [Fact]
    public void TheFrontDoorEndpointIsNamedByTheFirstLabelOfItsHost()
    {
        var tile = new TargetStatus(TargetKind.FrontDoor, TargetStatus.FrontDoorName, FrontDoor);

        Assert.Equal("fd-web-abc", tile.ResourceName);
    }

    [Fact]
    public void ADotlessHostGivesTheWholeHost()
    {
        var tile = new TargetStatus(TargetKind.FrontDoor, TargetStatus.FrontDoorName, Dotless);

        Assert.Equal("localhost", tile.ResourceName);
    }

    [Fact]
    public void ANodeGivesItsNameWhateverItsUrl()
    {
        var tile = new TargetStatus(TargetKind.Node, "app-west", new Uri("https://some.other.host.example.com/deep/path"));

        Assert.Equal("app-west", tile.ResourceName);
    }

    [Fact]
    public void TheFrontDoorChipSaysPublicAddress()
    {
        var tile = new TargetStatus(TargetKind.FrontDoor, TargetStatus.FrontDoorName, FrontDoor);

        Assert.Equal("public address", tile.Chip);
    }

    [Fact]
    public void ANodeWithARoleSaysTheRole()
    {
        var tile = new TargetStatus(TargetKind.Node, "app-west", West, role: "primary");

        Assert.Equal("primary", tile.Chip);
    }

    [Fact]
    public void ANodeWithNoRoleSaysNode()
    {
        var tile = new TargetStatus(TargetKind.Node, "app-west", West);

        Assert.Equal("node", tile.Chip);
    }

    [Fact]
    public void ANodeWithAnEmptyRoleSaysTheEmptyString()
    {
        var tile = new TargetStatus(TargetKind.Node, "app-west", West, role: string.Empty);

        Assert.Equal(string.Empty, tile.Chip);
    }

    [Fact]
    public void AnUndeclaredTargetKindTakesTheNodeArm()
    {
        var tile = new TargetStatus((TargetKind)99, "mystery", West, role: "standby");

        Assert.Equal("mystery", tile.ResourceName);
        Assert.Equal("standby", tile.Chip);

        var noRole = new TargetStatus((TargetKind)99, "mystery", West);

        Assert.Equal("node", noRole.Chip);
    }
}
