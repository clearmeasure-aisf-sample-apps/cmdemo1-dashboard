namespace Dashboard.Tests;

/// <summary>
/// The edges of <see cref="ServingShape" />, the one place the shape of a deployable is said (#971): which of the two
/// optional parts of a <see cref="ServingAssessment" /> makes which shape, that a rotation outranks an only node, the
/// nodes each shape carries, and the two lists a rotation splits its primaries into. The words each shape says are
/// pinned in <see cref="ServingStateShapeEdgeCaseTests" />.
/// </summary>
public class ServingShapeEdgeCaseTests
{
    private static readonly NodeHealth West = new("app-west", "westus3", true, HealthState.Healthy);
    private static readonly NodeHealth East = new("app-east", "eastus2", true, HealthState.Unhealthy);
    private static readonly NodeHealth North = new("app-north", "northeurope", true, HealthState.Pending);
    private static readonly NodeHealth One = new("app-one", null, true, HealthState.Healthy);

    [Fact]
    public void NeitherPartIsThePlainShape()
    {
        var shape = ServingShape.Of(null, null);

        Assert.IsType<ServingShape.Plain>(shape);
        Assert.Equal(new ServingShape.Plain(), shape);
    }

    [Fact]
    public void AnOnlyNodeIsTheOnlyShapeAndCarriesThatVeryNode()
    {
        var only = Assert.IsType<ServingShape.Only>(ServingShape.Of(One, null));

        Assert.Same(One, only.Node);
    }

    [Fact]
    public void ARotationIsTheRotationShapeAndCarriesThatVeryList()
    {
        IReadOnlyList<NodeHealth> primaries = [West, East];

        var rotation = Assert.IsType<ServingShape.Rotation>(ServingShape.Of(null, primaries));

        Assert.Same(primaries, rotation.Primaries);
    }

    [Fact]
    public void ARotationOutranksAnOnlyNode()
    {
        IReadOnlyList<NodeHealth> primaries = [West, East];

        var rotation = Assert.IsType<ServingShape.Rotation>(ServingShape.Of(One, primaries));

        Assert.Same(primaries, rotation.Primaries);
    }

    [Fact]
    public void AnEmptyRotationIsStillARotationWithNothingInEitherList()
    {
        var rotation = Assert.IsType<ServingShape.Rotation>(ServingShape.Of(null, []));

        Assert.Empty(rotation.Primaries);
        Assert.Empty(rotation.Healthy);
        Assert.Empty(rotation.NotHealthy);
    }

    [Fact]
    public void ARotationSplitsItsPrimariesIntoHealthyAndNotHealthyInTheOrderOfTheTopology()
    {
        var rotation = new ServingShape.Rotation([East, West, North, One]);

        Assert.Equal([West, One], rotation.Healthy);
        Assert.Equal([East, North], rotation.NotHealthy);
        Assert.Equal(rotation.Primaries.Count, rotation.Healthy.Count + rotation.NotHealthy.Count);
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public void TheAssessmentIsARotationExactlyWhenItsShapeIs(bool onlyNode, bool rotation, bool isRotation)
    {
        var only = onlyNode ? One : null;
        IReadOnlyList<NodeHealth>? primaries = rotation ? [West, East] : null;
        var assessment = new ServingAssessment(ServingState.Primary, West, null, null, only, primaries);

        Assert.Equal(isRotation, ServingShape.Of(only, primaries) is ServingShape.Rotation);
        Assert.Equal(isRotation, assessment.IsRotation);
    }

    [Fact]
    public void TheShapesAreClosedAndEachCaseIsSealed()
    {
        var type = typeof(ServingShape);

        Assert.True(type.IsAbstract);
        Assert.Empty(type.GetConstructors());
        Assert.All(
            [typeof(ServingShape.Rotation), typeof(ServingShape.Only), typeof(ServingShape.Plain)],
            shape =>
            {
                Assert.True(shape.IsSealed);
                Assert.Same(type, shape.BaseType);
            });
    }
}
