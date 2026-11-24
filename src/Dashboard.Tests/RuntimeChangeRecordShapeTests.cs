namespace Dashboard.Tests;

/// <summary>
/// The record contract of the runtime view's two "change" records, asserted through the shared
/// <see cref="RecordShape" /> helper. The expectations are the same ones RuntimeApplicationChangeEdgeCaseTests and
/// RuntimeDatabaseChangeEdgeCaseTests pin with their own reflection: the same member lists, the same nullability,
/// the same parameter order. They are stated here in the shared vocabulary, so the private copies can later be
/// retired without weakening anything.
/// </summary>
public class RuntimeChangeRecordShapeTests
{
    // ------------------------------------------------------------------------- RuntimeApplicationChange

    [Fact]
    public void TheApplicationChangeIsAPlainSealedRecordDeclaringOnlyItsPartsAndWhatEveryRecordIsAsked()
    {
        Assert.True(RecordShape.IsPlainSealedRecord<RuntimeApplicationChange>());
        Assert.Equal(
            ["Deconstruct", "Deployable", "Environment", "Equals", "GetHashCode", "Step", "ToString", "op_Equality", "op_Inequality"],
            RecordShape.SurfaceOf<RuntimeApplicationChange>());
    }

    [Fact]
    public void TheApplicationChangePartsAreTypedNullableAsDeclaredAndInitOnly()
    {
        Assert.Equal(
            [
                new RecordPart("Environment", typeof(string), Nullable: false, Readable: true, InitOnly: true),
                new RecordPart("Deployable", typeof(string), Nullable: true, Readable: true, InitOnly: true),
                new RecordPart("Step", typeof(bool), Nullable: false, Readable: true, InitOnly: true),
            ],
            RecordShape.PropertiesOf<RuntimeApplicationChange>());
    }

    [Fact]
    public void TheApplicationChangeConstructorTakesThePartsInOrderAndDefaultsNone()
    {
        Assert.Equal(
            [
                new RecordParameter("Environment", typeof(string), Nullable: false, Optional: false),
                new RecordParameter("Deployable", typeof(string), Nullable: true, Optional: false),
                new RecordParameter("Step", typeof(bool), Nullable: false, Optional: false),
            ],
            RecordShape.ConstructorOf<RuntimeApplicationChange>());
    }

    [Fact]
    public void TheApplicationChangeFixtureMakesTheUsualChangeAndOverridesOnlyWhatItIsTold()
    {
        Assert.Equal(new RuntimeApplicationChange("uat", "demo-ui", Step: true), RuntimeChanges.Application());
        Assert.Equal(new RuntimeApplicationChange("uat", null, Step: true), RuntimeChanges.Application(deployable: null));
        Assert.Equal(new RuntimeApplicationChange("prod", "demo-ui", Step: false), RuntimeChanges.Application("prod", step: false));
    }

    // ---------------------------------------------------------------------------- RuntimeDatabaseChange

    [Fact]
    public void TheDatabaseChangeIsAPlainSealedRecordDeclaringOnlyItsPartsAndWhatEveryRecordIsAsked()
    {
        Assert.True(RecordShape.IsPlainSealedRecord<RuntimeDatabaseChange>());
        Assert.Equal(
            ["Deconstruct", "Environment", "Equals", "GetHashCode", "Shown", "Step", "ToString", "op_Equality", "op_Inequality"],
            RecordShape.SurfaceOf<RuntimeDatabaseChange>());
    }

    [Fact]
    public void TheDatabaseChangePartsAreTypedNoneNullableAndInitOnly()
    {
        Assert.Equal(
            [
                new RecordPart("Environment", typeof(string), Nullable: false, Readable: true, InitOnly: true),
                new RecordPart("Shown", typeof(bool), Nullable: false, Readable: true, InitOnly: true),
                new RecordPart("Step", typeof(bool), Nullable: false, Readable: true, InitOnly: true),
            ],
            RecordShape.PropertiesOf<RuntimeDatabaseChange>());
    }

    [Fact]
    public void TheDatabaseChangeConstructorTakesThePartsInOrderAndDefaultsNone()
    {
        Assert.Equal(
            [
                new RecordParameter("Environment", typeof(string), Nullable: false, Optional: false),
                new RecordParameter("Shown", typeof(bool), Nullable: false, Optional: false),
                new RecordParameter("Step", typeof(bool), Nullable: false, Optional: false),
            ],
            RecordShape.ConstructorOf<RuntimeDatabaseChange>());
    }

    [Fact]
    public void TheDatabaseChangeFixtureMakesTheUsualChangeAndOverridesOnlyWhatItIsTold()
    {
        Assert.Equal(new RuntimeDatabaseChange("uat", Shown: true, Step: true), RuntimeChanges.Database());
        Assert.Equal(new RuntimeDatabaseChange("uat", Shown: false, Step: true), RuntimeChanges.Database(shown: false));
        Assert.Equal(new RuntimeDatabaseChange("prod", Shown: true, Step: false), RuntimeChanges.Database("prod", step: false));
    }
}
