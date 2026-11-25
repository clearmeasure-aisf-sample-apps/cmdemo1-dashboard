namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="DatabaseDiagram" />: the record
/// <see cref="RuntimeLoader.LoadDatabaseAsync" /> hands over with one environment's database view - the
/// <see cref="DatabaseManifest" /> and the SVG it read, and the reasons there is none of either - and the one verdict
/// it draws of its own, <see cref="DatabaseDiagram.IsValid" />.
/// <para>
/// <see cref="RuntimeFilesPairTests" /> already pins the loader's shapes (both parts and no reasons, or neither part
/// and every reason). What follows is the record on its own: the parts held as given, the verdict at its one-part
/// corners the loader never builds, and the members every record is asked.
/// </para>
/// </summary>
public class DatabaseDiagramEdgeCaseTests
{
    private const string Picture = "<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>";

    private const string Reason = "The server answered HTTP 404 for runtime/tdd.database.svg.";

    private static readonly DatabaseManifest Manifest = new("tdd", "ChurchBulletin", null, null, false, [], 0);

    private static readonly string[] NoReasons = [];

    private static readonly string[] OneReason = [Reason];

    private static DatabaseDiagram Full => new(Manifest, Picture, NoReasons);

    private static DatabaseDiagram NoPicture => new(Manifest, null, OneReason);

    private static DatabaseDiagram NoManifest => new(null, Picture, OneReason);

    private static DatabaseDiagram Neither => new(null, null, OneReason);

    private static DatabaseDiagram Nothing => new(null, null, null!);

    private static DatabaseDiagram EmptyWords => new(null, string.Empty, []);

    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        var reasons = new List<string> { Reason };

        var diagram = new DatabaseDiagram(Manifest, Picture, reasons);

        Assert.Same(Manifest, diagram.Manifest);
        Assert.Equal(Picture, diagram.Svg);
        Assert.Same(reasons, diagram.Errors);
    }

    [Fact]
    public void TheRecordRefusesNothingItIsGiven()
    {
        var nothing = Nothing;

        Assert.Null(nothing.Manifest);
        Assert.Null(nothing.Svg);
        Assert.Null(nothing.Errors);
        Assert.False(nothing.IsValid);

        var empty = EmptyWords;

        Assert.Equal(string.Empty, empty.Svg);
        Assert.Empty(empty.Errors);

        Assert.Equal("   ", new DatabaseDiagram(null, "   ", NoReasons).Svg);
    }

    [Fact]
    public void ItIsValidOnlyWhenBothPartsStand()
    {
        Assert.True(Full.IsValid);
        Assert.False(NoPicture.IsValid);
        Assert.False(NoManifest.IsValid);
        Assert.False(Neither.IsValid);
        Assert.False(Nothing.IsValid);
    }

    [Fact]
    public void TheVerdictIgnoresTheReasonsAndWhatThePictureSays()
    {
        // Reasons do not move the verdict: both parts standing is valid even with something said.
        Assert.True(new DatabaseDiagram(Manifest, Picture, OneReason).IsValid);
        Assert.True(new DatabaseDiagram(Manifest, Picture, null!).IsValid);

        // An empty picture, or one that is no SVG at all, is still a picture to the record (the loader judges that).
        Assert.True(new DatabaseDiagram(Manifest, string.Empty, NoReasons).IsValid);
        Assert.True(new DatabaseDiagram(Manifest, "not a picture", NoReasons).IsValid);

        // An empty manifest - no tables drawn, truncated or not - still stands.
        Assert.True(new DatabaseDiagram(Manifest with { Truncated = true, Drawn = -1 }, Picture, NoReasons).IsValid);

        // No parts and no reasons is still invalid.
        Assert.False(new DatabaseDiagram(null, null, NoReasons).IsValid);
    }

    [Fact]
    public void TwoDiagramsOfTheSamePartsAreEqualAndHashAlike()
    {
        Assert.Equal(Full, new DatabaseDiagram(Manifest, Picture, NoReasons));
        Assert.True(Full == new DatabaseDiagram(Manifest, Picture, NoReasons));
        Assert.Equal(Full.GetHashCode(), new DatabaseDiagram(Manifest, Picture, NoReasons).GetHashCode());

        Assert.Equal(Nothing, new DatabaseDiagram(null, null, null!));
        Assert.Equal(Nothing.GetHashCode(), Nothing.GetHashCode());

        // An equal-but-distinct manifest compares by value.
        Assert.Equal(Full, new DatabaseDiagram(Manifest with { }, Picture, NoReasons));
    }

    [Fact]
    public void ADiagramDiffersInAnyOneOfItsParts()
    {
        var diagram = Full;

        Assert.NotEqual(diagram, diagram with { Manifest = null });
        Assert.NotEqual(diagram, diagram with { Manifest = Manifest with { Environment = "uat" } });
        Assert.NotEqual(diagram, diagram with { Svg = null });
        Assert.NotEqual(diagram, diagram with { Svg = string.Empty });
        Assert.NotEqual(diagram, diagram with { Errors = OneReason });
        Assert.NotEqual(diagram, diagram with { Errors = null! });
        Assert.True(diagram != diagram with { Svg = null });

        // The reasons are compared as the list instance, not by their words.
        Assert.NotEqual(
            new DatabaseDiagram(null, null, new List<string> { Reason }),
            new DatabaseDiagram(null, null, new List<string> { Reason }));

        // The empty word and the nothing are two different pictures.
        Assert.NotEqual(EmptyWords, new DatabaseDiagram(null, null, EmptyWords.Errors));
    }

    [Fact]
    public void ADiagramIsNeverEqualToNothingOrAnotherKind()
    {
        var diagram = Full;

        Assert.False(diagram == null);
        Assert.True(null != diagram);
        Assert.False(diagram.Equals((object?)null));
        Assert.False(diagram.Equals((DatabaseDiagram?)null));
        Assert.False(diagram.Equals(Manifest));
        Assert.False(diagram.Equals(Picture));
        Assert.False(diagram.Equals(new RuntimeDiagram("tdd", null, Picture, NoReasons)));
    }

    [Fact]
    public void AWithCopiesTheDiagramAndTurnsOnlyThePartItNames()
    {
        var reasons = new List<string> { Reason };
        var diagram = new DatabaseDiagram(Manifest, null, reasons);

        var copy = diagram with { };

        Assert.Equal(diagram, copy);
        Assert.NotSame(diagram, copy);
        Assert.Same(reasons, copy.Errors);
        Assert.False(copy.IsValid);

        var drawn = diagram with { Svg = Picture };

        Assert.True(drawn.IsValid);
        Assert.Same(Manifest, drawn.Manifest);
        Assert.Same(reasons, drawn.Errors);
        Assert.False(diagram.IsValid);
    }

    [Fact]
    public void TheThreePartsFallOutInTheirOrder()
    {
        var (manifest, svg, errors) = Full;

        Assert.Same(Manifest, manifest);
        Assert.Equal(Picture, svg);
        Assert.Same(NoReasons, errors);

        var (noManifest, noSvg, noErrors) = Nothing;

        Assert.Null(noManifest);
        Assert.Null(noSvg);
        Assert.Null(noErrors);
    }

    [Fact]
    public void TheDiagramSaysItsPartsAndItsVerdict()
    {
        Assert.Equal("DatabaseDiagram { Manifest = , Svg = , Errors = , IsValid = False }", Nothing.ToString());
        Assert.Equal(
            "DatabaseDiagram { Manifest = , Svg = , Errors = System.String[], IsValid = False }",
            EmptyWords.ToString());
        Assert.StartsWith("DatabaseDiagram { Manifest = DatabaseManifest { ", Full.ToString(), StringComparison.Ordinal);
        Assert.EndsWith($", Svg = {Picture}, Errors = System.String[], IsValid = True }}", Full.ToString(), StringComparison.Ordinal);

        ViewerCulture.German(() =>
            Assert.EndsWith(", IsValid = True }", Full.ToString(), StringComparison.Ordinal));
    }

    [Fact]
    public void TheDiagramHasThreeInitOnlyPartsAndOneReadOnlyVerdict()
    {
        Assert.True(RecordShape.IsPlainSealedRecord<DatabaseDiagram>());
        Assert.Equal(["Manifest", "Svg", "Errors", "IsValid"], [.. RecordSurface.PropertyNamesOf<DatabaseDiagram>()]);
        Assert.Equal(
            ["Deconstruct", "Equals", "Errors", "GetHashCode", "IsValid", "Manifest", "Svg", "ToString", "op_Equality", "op_Inequality"],
            RecordSurface.Of<DatabaseDiagram>());

        RecordSurface.AssertPartsAreInitOnly(typeof(DatabaseDiagram), "Manifest", "Svg", "Errors");

        var properties = RecordSurface.PropertiesOf<DatabaseDiagram>();

        Assert.Equal(typeof(DatabaseManifest), properties["Manifest"].PropertyType);
        Assert.Equal(typeof(string), properties["Svg"].PropertyType);
        Assert.Equal(typeof(IReadOnlyList<string>), properties["Errors"].PropertyType);
        Assert.Equal(typeof(bool), properties["IsValid"].PropertyType);
        Assert.Null(properties["IsValid"].SetMethod);

        var parameters = RecordSurface.ParametersOf<DatabaseDiagram>();

        Assert.Equal(["Manifest", "Svg", "Errors"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.All(parameters, parameter => Assert.False(parameter.HasDefaultValue));
        Assert.Equal([true, true, false], RecordSurface.NullableParts<DatabaseDiagram>());
    }
}
