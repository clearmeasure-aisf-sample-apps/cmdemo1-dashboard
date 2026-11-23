namespace Dashboard.Tests;

/// <summary>
/// The corners of <see cref="ServingState" /> that the other ServingState files leave open: an assessment built by
/// hand whose parts do not match its state. <see cref="ServingAssessment.Assess" /> never makes one, but the record
/// trusts what it is handed, so these tests pin what each state is worth when the expected node is missing where the
/// state needs one, or present where the state names none. The enum's surface is pinned in
/// <see cref="ServingStateEdgeCaseTests" /> and <see cref="ServingStateBoundaryEdgeCaseTests" />, and the
/// state-by-shape matrix with matching parts in <see cref="ServingStateShapeEdgeCaseTests" />. Nothing here reaches
/// the network or the clock.
/// </summary>
public class ServingStateRecordEdgeCaseTests
{
    private static readonly NodeHealth West = new("app-west", "westus3", true, HealthState.Healthy);
    private static readonly NodeHealth East = new("app-east", "eastus2", true, HealthState.Unhealthy);
    private static readonly NodeHealth North = new("app-north", "northeurope", true, HealthState.Healthy);

    private static ServingAssessment Plain(ServingState state, NodeHealth? expected, NodeHealth? failedPrimary = null) =>
        new(state, expected, failedPrimary, FrontDoorAgreement.NotPresent, null);

    private static ServingAssessment Rotation(ServingState state, NodeHealth? expected) =>
        new(state, expected, null, FrontDoorAgreement.Agrees, HealthState.Healthy, null, [West, East, North]);

    /// <summary>The two serving states read the expected node's label for their headline, so a plain assessment that
    /// names no expected node cannot say its headline at all.</summary>
    [Theory]
    [InlineData(ServingState.Primary)]
    [InlineData(ServingState.FailedOver)]
    public void APlainServingStateWithNoExpectedNodeCannotSayItsHeadline(ServingState state)
    {
        var assessment = Plain(state, null, East);

        Assert.Throws<NullReferenceException>(() => assessment.Headline);
        Assert.Empty(assessment.Serving);
    }

    [Fact]
    public void APlainFailoverWithNoExpectedNodeCannotSayItsDetailWithOrWithoutABlamedPrimary()
    {
        Assert.Throws<NullReferenceException>(() => Plain(ServingState.FailedOver, null, East).FailoverDetail);
        Assert.Throws<NullReferenceException>(() => Plain(ServingState.FailedOver, null).FailoverDetail);
    }

    /// <summary>The states that serve nothing never read the expected node for their words, yet the serving list is
    /// the expected node whenever one is handed: the list follows the part, the headline follows the state.</summary>
    [Theory]
    [InlineData(ServingState.NoNodes, "No nodes in the topology")]
    [InlineData(ServingState.Pending, "Checking which region serves traffic")]
    [InlineData(ServingState.Down, "No healthy node: nothing can serve traffic")]
    [InlineData((ServingState)5, "No healthy node: nothing can serve traffic")]
    [InlineData((ServingState)(-1), "No healthy node: nothing can serve traffic")]
    public void ANonServingStateWithAnExpectedNodeStillListsItButDoesNotNameIt(ServingState state, string headline)
    {
        var assessment = Plain(state, West, East);

        Assert.Equal(headline, assessment.Headline);
        Assert.Equal(["westus3"], assessment.Serving.Select(node => node.Label));
        Assert.Null(assessment.FailoverDetail);
        Assert.False(assessment.IsFailedOver);
        Assert.Empty(assessment.OutOfRotation);
    }

    /// <summary>The primary state of a rotation reads the rotation, not the expected node: with none named it still
    /// says its headline and serves every healthy primary.</summary>
    [Fact]
    public void ARotationInThePrimaryStateNeedsNoExpectedNode()
    {
        var assessment = Rotation(ServingState.Primary, null);

        Assert.Equal("Front Door rotates over 2 of 3 regions: eastus2 is not healthy", assessment.Headline);
        Assert.Equal(["westus3", "northeurope"], assessment.Serving.Select(node => node.Label));
        Assert.Null(assessment.FailoverDetail);
    }

    [Fact]
    public void ARotationThatFailedOverWithNoExpectedNodeCannotSayItsWords()
    {
        var assessment = Rotation(ServingState.FailedOver, null);

        Assert.Throws<NullReferenceException>(() => assessment.Headline);
        Assert.Throws<NullReferenceException>(() => assessment.FailoverDetail);
        Assert.Empty(assessment.Serving);
        Assert.True(assessment.IsFailedOver);
    }

    /// <summary>Outside the primary state a rotation serves only the expected node it is handed, even a state outside
    /// the five; the nodes out of the rotation do not move with the state.</summary>
    [Theory]
    [InlineData(ServingState.NoNodes, "No region is healthy")]
    [InlineData(ServingState.Pending, "Checking which regions serve traffic")]
    [InlineData(ServingState.Down, "No region is healthy")]
    [InlineData((ServingState)(-1), "No region is healthy")]
    public void ARotationOutsideThePrimaryStateServesOnlyTheExpectedNodeItIsHanded(ServingState state, string headline)
    {
        var assessment = Rotation(state, North);

        Assert.Equal(headline, assessment.Headline);
        Assert.Equal(["northeurope"], assessment.Serving.Select(node => node.Label));
        Assert.Equal(["eastus2"], assessment.OutOfRotation.Select(node => node.Label));
        Assert.Null(assessment.FailoverDetail);
        Assert.False(assessment.IsFailedOver);
    }
}
