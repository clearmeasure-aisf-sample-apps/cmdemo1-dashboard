namespace Dashboard.Tests;

/// <summary>
/// <see cref="RecordShape.SurfaceOf(Type)" /> and <see cref="RecordSurface.Of(Type)" /> are the same walk over a
/// record's public surface (#1681). These tests pin that they answer the same list for every public type the Dashboard
/// assembly declares and for the record counterexamples of <see cref="RecordShapeTests" />, so retiring
/// <c>RecordShape.SurfaceOf</c> in favour of <c>RecordSurface.Of</c> changes no test's expectation.
/// </summary>
public class RecordShapeSurfaceAgreementTests
{
    public static TheoryData<Type> DashboardTypes =>
        [.. typeof(RuntimeDiagram).Assembly.GetExportedTypes().OrderBy(type => type.FullName, StringComparer.Ordinal)];

    public static TheoryData<Type> Counterexamples =>
    [
        typeof(RecordShapeTests.Plain),
        typeof(RecordShapeTests.Unsealed),
        typeof(RecordShapeTests.Struct),
        typeof(RecordShapeTests.NotARecord),
        typeof(RecordShapeTests.Named),
        typeof(RecordShapeTests.Derived),
        typeof(RecordShapeTests.WithExtra),
        typeof(RecordShapeTests.Mutable),
        typeof(RecordShapeTests.Defaulted),
        typeof(RecordShapeTests.TwoConstructors),
    ];

    [Theory]
    [MemberData(nameof(DashboardTypes))]
    public void BothWalksSayTheSameSurfaceForEveryDashboardType(Type type) =>
        Assert.Equal(RecordSurface.Of(type), RecordShape.SurfaceOf(type));

    [Theory]
    [MemberData(nameof(Counterexamples))]
    public void BothWalksSayTheSameSurfaceForEveryCounterexample(Type type) =>
        Assert.Equal(RecordSurface.Of(type), RecordShape.SurfaceOf(type));

    [Fact]
    public void TheDashboardAssemblyGivesTheAgreementSomethingToCheck() =>
        Assert.Contains(typeof(RuntimeApplicationChange), typeof(RuntimeDiagram).Assembly.GetExportedTypes());

    [Fact]
    public void TheGenericFormsAgreeOnTheRuntimeChangeRecords()
    {
        Assert.Equal(RecordSurface.Of<RuntimeApplicationChange>(), RecordShape.SurfaceOf<RuntimeApplicationChange>());
        Assert.Equal(RecordSurface.Of<RuntimeDatabaseChange>(), RecordShape.SurfaceOf<RuntimeDatabaseChange>());
    }
}
