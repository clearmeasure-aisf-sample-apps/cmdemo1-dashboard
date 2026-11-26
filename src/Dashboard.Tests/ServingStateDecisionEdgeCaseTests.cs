using static Dashboard.Tests.HealthStates;

namespace Dashboard.Tests;

/// <summary>
/// Which <see cref="ServingState" /> <see cref="ServingAssessment.Assess" /> lands on when the priority order has more
/// than two nodes to walk. The enum's own surface is pinned in <see cref="ServingStateEdgeCaseTests" /> and
/// <see cref="ServingStateBoundaryEdgeCaseTests" />, the words of each state in <see cref="ServingStateShapeEdgeCaseTests" />,
/// and the two-node decisions in <see cref="ServingAssessmentTests" />; what is left open, and pinned here, is a third
/// node behind the first two: an unchecked standby that holds back the healthy one after it, the second of two primaries
/// with no Front Door endpoint, and a rotation with no healthy primary that falls to its standbys. Every node and every
/// endpoint answer below is fixed; nothing reaches the network or the clock.
/// </summary>
public class ServingStateDecisionEdgeCaseTests
{
    private static NodeHealth Primary(HealthState state, string region) => new($"app-{region}", region, true, state);

    private static NodeHealth Standby(HealthState state, string region) => new($"app-{region}", region, false, state);

    [Theory]
    [InlineData(Pending, Healthy, ServingState.Pending, null)]
    [InlineData(Unhealthy, Healthy, ServingState.FailedOver, "northeurope")]
    [InlineData(Unreachable, Healthy, ServingState.FailedOver, "northeurope")]
    [InlineData(Healthy, Pending, ServingState.FailedOver, "eastus2")]
    [InlineData(Unhealthy, Pending, ServingState.Pending, null)]
    [InlineData(Unhealthy, Unreachable, ServingState.Down, null)]
    public void AStandbyEarlierInTheTopologyDecidesBeforeTheOnesAfterIt(
        HealthState first, HealthState second, ServingState state, string? expected)
    {
        var assessment = ServingAssessment.Assess(
            [Primary(Unhealthy, "westus3"), Standby(first, "eastus2"), Standby(second, "northeurope")], frontDoor: null);

        Assert.Equal(state, assessment.State);
        Assert.Equal(expected, assessment.Expected?.Label);
        Assert.Equal(state == ServingState.FailedOver ? "westus3" : null, assessment.FailedPrimary?.Label);
        Assert.Null(assessment.OnlyNode);
        Assert.False(assessment.IsRotation);
    }

    [Fact]
    public void AFailoverPastAnUncheckedStandbyNeverHappensAndNamesNoNode()
    {
        var assessment = ServingAssessment.Assess(
            [Primary(Unhealthy, "westus3"), Standby(Pending, "eastus2"), Standby(Healthy, "northeurope")], frontDoor: Healthy);

        Assert.Equal(ServingState.Pending, assessment.State);
        Assert.Equal(FrontDoorAgreement.Pending, assessment.FrontDoor);
        Assert.Empty(assessment.Serving);
        Assert.Null(assessment.FailoverDetail);
        Assert.Equal("Checking which region serves traffic", assessment.Headline);
        Assert.Equal("Front Door is being checked.", assessment.FrontDoorText);
    }

    [Fact]
    public void WithoutAFrontDoorTheSecondPrimaryServesWhenTheFirstIsNotHealthy()
    {
        var assessment = ServingAssessment.Assess(
            [Primary(Unhealthy, "westus3"), Primary(Healthy, "centralus"), Standby(Healthy, "eastus2")], frontDoor: null);

        Assert.Equal(ServingState.Primary, assessment.State);
        Assert.Equal("centralus", assessment.Expected?.Label);
        Assert.Null(assessment.FailedPrimary);
        Assert.False(assessment.IsRotation);
        Assert.Equal("Expected to serve traffic: centralus (primary)", assessment.Headline);
        Assert.Null(assessment.FailoverDetail);
    }

    [Fact]
    public void WithoutAFrontDoorAFailoverFromTwoPrimariesBlamesTheFirst()
    {
        var assessment = ServingAssessment.Assess(
            [Primary(Unreachable, "westus3"), Primary(Unhealthy, "centralus"), Standby(Healthy, "eastus2")], frontDoor: null);

        Assert.Equal(ServingState.FailedOver, assessment.State);
        Assert.Equal("westus3", assessment.FailedPrimary?.Label);
        Assert.Equal("Failed over to eastus2", assessment.Headline);
        Assert.Equal("Primary westus3 is unreachable; eastus2 is expected to serve traffic.", assessment.FailoverDetail);
    }

    [Theory]
    [InlineData(Pending, Healthy, ServingState.Pending, FrontDoorAgreement.Pending, "Checking which regions serve traffic")]
    [InlineData(Unhealthy, Healthy, ServingState.FailedOver, FrontDoorAgreement.Agrees, "Failed over to northeurope")]
    [InlineData(Unreachable, Unhealthy, ServingState.Down, FrontDoorAgreement.Disagrees, "No region is healthy")]
    public void ARotationWithNoHealthyPrimaryTakesItsStandbysInTopologyOrder(
        HealthState first, HealthState second, ServingState state, FrontDoorAgreement agreement, string headline)
    {
        var assessment = ServingAssessment.Assess(
            [Primary(Unhealthy, "westus3"), Primary(Unreachable, "centralus"), Standby(first, "eastus2"), Standby(second, "northeurope")],
            frontDoor: Healthy);

        Assert.Equal(state, assessment.State);
        Assert.Equal(agreement, assessment.FrontDoor);
        Assert.Equal(headline, assessment.Headline);
        Assert.True(assessment.IsRotation);
        Assert.Null(assessment.FailedPrimary);
        Assert.Equal(["westus3", "centralus"], assessment.OutOfRotation.Select(node => node.Label));
    }

    [Fact]
    public void ARotationThatIsDownWhileItsFrontDoorIsHealthySaysNoNodeIs()
    {
        var assessment = ServingAssessment.Assess(
            [Primary(Unhealthy, "westus3"), Primary(Unhealthy, "centralus"), Standby(Unhealthy, "eastus2")], frontDoor: Healthy);

        Assert.Equal(ServingState.Down, assessment.State);
        Assert.Empty(assessment.Serving);
        Assert.Null(assessment.FailoverDetail);
        Assert.Equal("Front Door disagrees: it is healthy although no node is.", assessment.FrontDoorText);
    }
}
