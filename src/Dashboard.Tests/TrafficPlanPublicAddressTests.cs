namespace Dashboard.Tests;

/// <summary>
/// Which address <see cref="TrafficPlan.For" /> calls on each deployable and which paths it calls there, built from the
/// records directly: Front Door before any node, the primary node before the first, the first node when none is
/// primary, nothing without an address; the start page without traffic paths and no call at all with an empty list.
/// </summary>
public class TrafficPlanPublicAddressTests
{
    private static readonly Uri Door = new("https://fd.example.net");
    private static readonly NodeInfo West = new("west", "westus", NodeInfo.StandbyRole, new Uri("https://west.example.net"));
    private static readonly NodeInfo East = new("east", "eastus", NodeInfo.PrimaryRole, new Uri("https://east.example.net"));
    private static readonly NodeInfo North = new("north", "northus", NodeInfo.StandbyRole, new Uri("https://north.example.net"));

    private static DeployableInfo Deployable(string name, Uri? frontDoor, IReadOnlyList<NodeInfo> nodes, IReadOnlyList<string>? paths = null) =>
        new(name, frontDoor, DeployableInfo.DefaultHealthPath, DeployableInfo.DefaultAlivePath, DeployableInfo.DefaultVersionPath, nodes, TrafficPaths: paths);

    private static TrafficPlan? PlanOf(params DeployableInfo[] deployables) => TrafficPlan.For(new EnvironmentInfo("uat", null, deployables));

    [Fact]
    public void FrontDoorIsCalledBeforeAnyNode()
    {
        var plan = PlanOf(Deployable("ui", Door, [West, East]))!;

        Assert.Equal(["https://fd.example.net/"], plan.Addresses.Select(address => address.AbsoluteUri));
        Assert.Equal(["ui at fd.example.net"], plan.Targets);
    }

    [Fact]
    public void WithoutFrontDoorThePrimaryNodeIsCalledEvenWhenItIsNotFirst()
    {
        var plan = PlanOf(Deployable("ui", null, [West, East]))!;

        Assert.Equal(["https://east.example.net/"], plan.Addresses.Select(address => address.AbsoluteUri));
        Assert.Equal(["ui at east.example.net"], plan.Targets);
    }

    [Fact]
    public void WithoutFrontDoorOrPrimaryTheFirstNodeIsCalled()
    {
        var plan = PlanOf(Deployable("ui", null, [West, North]))!;

        Assert.Equal(["https://west.example.net/"], plan.Addresses.Select(address => address.AbsoluteUri));
        Assert.Equal(["ui at west.example.net"], plan.Targets);
    }

    [Fact]
    public void ADeployableWithNoAddressIsLeftOutAndAloneMakesNoPlan()
    {
        Assert.Null(PlanOf(Deployable("ui", null, [])));

        var plan = PlanOf(Deployable("gone", null, []), Deployable("ui", null, [East]))!;
        Assert.Equal(["ui at east.example.net"], plan.Targets);
    }

    [Fact]
    public void AnEmptyListOfPathsLeavesTheDeployableOutAndConfiguredPathsAreCalledInOrder()
    {
        var plan = PlanOf(
            Deployable("dashboard", Door, [East], []),
            Deployable("ui", null, [West], ["/", "/api/summary"]))!;

        Assert.Equal(["https://west.example.net/", "https://west.example.net/api/summary"], plan.Addresses.Select(address => address.AbsoluteUri));
        Assert.Equal(["ui at west.example.net"], plan.Targets);
        Assert.Null(PlanOf(Deployable("dashboard", Door, [East], [])));
    }
}
