namespace Dashboard.Tests;

/// <summary>
/// The fixed words each <see cref="ServingState" /> is worth in each of the three shapes a
/// <see cref="ServingAssessment" /> can take: the plain deployable (neither a rotation nor one node), the only node of
/// a deployable without a Front Door endpoint, and the rotation. The enum's own surface is pinned in
/// <see cref="ServingStateEdgeCaseTests" /> and <see cref="ServingStateBoundaryEdgeCaseTests" />; what
/// <see cref="ServingAssessment.Assess" /> decides is pinned in <see cref="ServingAssessmentTests" /> and
/// <see cref="RotationTests" />. This file pins the whole state-by-shape matrix of the headline, the failover detail,
/// the serving list and the failover flag, built from records with fixed nodes, so no state falls through to a
/// shape's catch-all word unnoticed. Nothing here reaches the network or the clock.
/// </summary>
public class ServingStateShapeEdgeCaseTests
{
    private static readonly NodeHealth West = new("app-west", "westus3", true, HealthState.Healthy);
    private static readonly NodeHealth East = new("app-east", "eastus2", true, HealthState.Unhealthy);
    private static readonly NodeHealth Standby = new("app-standby", "centralus", false, HealthState.Healthy);
    private static readonly NodeHealth One = new("app-one", null, true, HealthState.Healthy);

    private static ServingAssessment Plain(ServingState state) =>
        new(state, state is ServingState.Primary or ServingState.FailedOver ? (state == ServingState.Primary ? West : Standby) : null,
            state == ServingState.FailedOver ? East : null, FrontDoorAgreement.NotPresent, null);

    private static ServingAssessment OnlyNode(ServingState state) =>
        new(state, state is ServingState.Primary or ServingState.FailedOver ? One : null, null, FrontDoorAgreement.NotPresent, null, One);

    private static ServingAssessment Rotation(ServingState state) =>
        new(state, state switch
        {
            ServingState.Primary => West,
            ServingState.FailedOver => Standby,
            _ => null,
        }, null, FrontDoorAgreement.Agrees, HealthState.Healthy, null, [West, East]);

    [Theory]
    [InlineData(ServingState.NoNodes, "No nodes in the topology")]
    [InlineData(ServingState.Pending, "Checking which region serves traffic")]
    [InlineData(ServingState.Primary, "Expected to serve traffic: westus3 (primary)")]
    [InlineData(ServingState.FailedOver, "Failed over to centralus")]
    [InlineData(ServingState.Down, "No healthy node: nothing can serve traffic")]
    [InlineData((ServingState)(-1), "No healthy node: nothing can serve traffic")]
    public void APlainDeployableHasOneHeadlinePerState(ServingState state, string headline)
    {
        var assessment = Plain(state);

        Assert.Equal(headline, assessment.Headline);
        Assert.False(assessment.IsRotation);
        Assert.Empty(assessment.OutOfRotation);
    }

    [Theory]
    [InlineData(ServingState.NoNodes, "Not serving: app-one")]
    [InlineData(ServingState.Pending, "Checking app-one")]
    [InlineData(ServingState.Primary, "Serves traffic: app-one")]
    [InlineData(ServingState.FailedOver, "Serves traffic: app-one")]
    [InlineData(ServingState.Down, "Not serving: app-one")]
    [InlineData((ServingState)5, "Not serving: app-one")]
    public void TheOnlyNodeServesOrNotWhateverTheStateIsCalled(ServingState state, string headline)
    {
        var assessment = OnlyNode(state);

        Assert.Equal(headline, assessment.Headline);

        // One node has nothing to fail over from: even FailedOver gives no detail, though the flag still follows the state.
        Assert.Null(assessment.FailoverDetail);
        Assert.Equal(state == ServingState.FailedOver, assessment.IsFailedOver);
        Assert.Null(assessment.FrontDoorText);
    }

    [Theory]
    [InlineData(ServingState.NoNodes, "No region is healthy")]
    [InlineData(ServingState.Pending, "Checking which regions serve traffic")]
    [InlineData(ServingState.Primary, "Front Door rotates over 1 of 2 regions: eastus2 is not healthy")]
    [InlineData(ServingState.FailedOver, "Failed over to centralus")]
    [InlineData(ServingState.Down, "No region is healthy")]
    [InlineData((ServingState)5, "No region is healthy")]
    public void ARotationHasOneHeadlinePerState(ServingState state, string headline)
    {
        var assessment = Rotation(state);

        Assert.Equal(headline, assessment.Headline);
        Assert.True(assessment.IsRotation);
        Assert.Equal(["eastus2"], assessment.OutOfRotation.Select(node => node.Label));
        Assert.Equal("Front Door agrees: it is healthy.", assessment.FrontDoorText);
    }

    [Theory]
    [InlineData(ServingState.NoNodes, null)]
    [InlineData(ServingState.Pending, null)]
    [InlineData(ServingState.Primary, null)]
    [InlineData(ServingState.FailedOver, "Primary eastus2 is unhealthy; centralus is expected to serve traffic.")]
    [InlineData(ServingState.Down, null)]
    public void OnlyFailedOverExplainsItselfOnAPlainDeployable(ServingState state, string? detail)
    {
        Assert.Equal(detail, Plain(state).FailoverDetail);
    }

    [Fact]
    public void ARotationAndAPlainDeployableFailOverInTheSameWords()
    {
        Assert.Equal(Plain(ServingState.FailedOver).Headline, Rotation(ServingState.FailedOver).Headline);
        Assert.Equal("Failed over to centralus", Plain(ServingState.FailedOver).Headline);
        Assert.Equal("Serves traffic: app-one", OnlyNode(ServingState.FailedOver).Headline);
    }

    [Fact]
    public void ARotationThatFailedOverNamesNoPrimaryInItsDetail()
    {
        Assert.Equal("No primary node is healthy; centralus is expected to serve traffic.", Rotation(ServingState.FailedOver).FailoverDetail);
    }

    [Theory]
    [InlineData(ServingState.NoNodes, new string[0])]
    [InlineData(ServingState.Pending, new string[0])]
    [InlineData(ServingState.Primary, new[] { "westus3" })]
    [InlineData(ServingState.FailedOver, new[] { "centralus" })]
    [InlineData(ServingState.Down, new string[0])]
    public void APlainDeployableServesFromItsExpectedNodeOnly(ServingState state, string[] serving)
    {
        Assert.Equal(serving, Plain(state).Serving.Select(node => node.Label));
    }

    [Theory]
    [InlineData(ServingState.NoNodes, new string[0])]
    [InlineData(ServingState.Pending, new string[0])]
    [InlineData(ServingState.Primary, new[] { "westus3" })]
    [InlineData(ServingState.FailedOver, new[] { "centralus" })]
    [InlineData(ServingState.Down, new string[0])]
    public void ARotationServesFromEveryHealthyPrimaryOnlyInThePrimaryState(ServingState state, string[] serving)
    {
        Assert.Equal(serving, Rotation(state).Serving.Select(node => node.Label));
    }

    [Fact]
    public void ARotationInThePrimaryStateServesEveryHealthyPrimaryNotJustTheExpectedOne()
    {
        var second = new NodeHealth("app-north", "northeurope", true, HealthState.Healthy);
        var assessment = Rotation(ServingState.Primary) with { Rotation = [West, East, second] };

        Assert.Equal(["westus3", "northeurope"], assessment.Serving.Select(node => node.Label));
        Assert.Equal("Front Door rotates over 2 of 3 regions: eastus2 is not healthy", assessment.Headline);
    }
}
