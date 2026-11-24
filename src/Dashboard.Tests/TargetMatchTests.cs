using Dashboard.Health;
using Dashboard.Runtime;

namespace Dashboard.Tests;

/// <summary>
/// Which monitor target a node of the manifest is: a web app and a static site are nodes of the topology, a Front Door
/// endpoint is the public address, by kind and address together; any other node, and one without an address, is none.
/// </summary>
public class TargetMatchTests
{
    private static readonly Uri Address = new("https://app-cmdemo1-uat-centralus.azurewebsites.net/");

    private static RuntimeNode Node(RuntimeNodeKind kind, Uri? url = null) => new("n", kind, "name", url);

    [Theory]
    [InlineData(RuntimeNodeKind.WebApp, TargetKind.Node)]
    [InlineData(RuntimeNodeKind.StaticSite, TargetKind.Node)]
    [InlineData(RuntimeNodeKind.FrontDoor, TargetKind.FrontDoor)]
    public void AMonitoredKindWithAnAddressIsTheTargetOfItsKindAtThatAddress(RuntimeNodeKind kind, TargetKind expected)
    {
        Assert.Equal(new TargetMatch(expected, Address), TargetMatch.Of(Node(kind, Address)));
    }

    [Theory]
    [InlineData(RuntimeNodeKind.Other)]
    [InlineData(RuntimeNodeKind.Person)]
    [InlineData(RuntimeNodeKind.Sql)]
    [InlineData(RuntimeNodeKind.Dependency)]
    [InlineData(RuntimeNodeKind.ContainerApp)]
    [InlineData(RuntimeNodeKind.Resource)]
    public void AnyOtherKindIsNoTargetEvenWithAnAddress(RuntimeNodeKind kind)
    {
        Assert.Null(TargetMatch.Of(Node(kind, Address)));
    }

    [Theory]
    [InlineData(RuntimeNodeKind.WebApp)]
    [InlineData(RuntimeNodeKind.StaticSite)]
    [InlineData(RuntimeNodeKind.FrontDoor)]
    public void ANodeWithoutAnAddressIsNoTarget(RuntimeNodeKind kind)
    {
        Assert.Null(TargetMatch.Of(Node(kind)));
    }

    [Fact]
    public void ItMatchesTheTargetOfTheSameKindAndAddressOnly()
    {
        var match = TargetMatch.Of(Node(RuntimeNodeKind.WebApp, Address))!;

        Assert.True(match.Matches(new TargetStatus(TargetKind.Node, "app", new Uri(Address.AbsoluteUri))));
        Assert.False(match.Matches(new TargetStatus(TargetKind.FrontDoor, TargetStatus.FrontDoorName, Address)));
        Assert.False(match.Matches(new TargetStatus(TargetKind.Node, "app", new Uri("https://app-cmdemo1-uat-eastus2.azurewebsites.net/"))));
    }

    [Fact]
    public void ItRefusesNulls()
    {
        Assert.Throws<ArgumentNullException>(() => TargetMatch.Of(null!));
        Assert.Throws<ArgumentNullException>(() => new TargetMatch(TargetKind.Node, Address).Matches(null!));
    }
}
