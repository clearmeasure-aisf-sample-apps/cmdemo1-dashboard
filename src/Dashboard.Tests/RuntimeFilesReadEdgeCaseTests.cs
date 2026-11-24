using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The pair's own verdict, <c>RuntimeFilesRead&lt;TManifest&gt;.Of</c> (<c>src/Dashboard/Runtime/RuntimeLoader.cs</c>):
/// both parts and no errors, or neither part and every reason (#1683). <see cref="RuntimeLoaderTests" /> and
/// <see cref="RuntimeFilesPairTests" /> reach it only through <see cref="RuntimeLoader.LoadDiagramAsync" /> and
/// <see cref="RuntimeLoader.LoadDatabaseAsync" /> over HTTP; these tests pin each of its three refusal conditions -
/// errors standing, the manifest missing, the picture missing - as the record's own decision, for
/// <see cref="RuntimeManifest" /> and <see cref="DatabaseManifest" /> alike.
/// <para>
/// The record is internal and the app declares no <c>InternalsVisibleTo</c>, so it is found by its name in the
/// namespace where <see cref="RuntimeLoader" /> lives and its factory is called through reflection, as
/// <see cref="MarkPlaceOrderEdgeCaseTests" /> reads <c>MarkPlace</c>. Nothing here reaches the network.
/// </para>
/// </summary>
public class RuntimeFilesReadEdgeCaseTests
{
    private const string Picture = "<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>";

    private const string Reason = "The server answered HTTP 404 for runtime/uat.svg.";

    private static readonly RuntimeManifest Diagram = new("uat", [], [], []);

    private static readonly DatabaseManifest Database = new("tdd", "sqldb-cmdemo1-tdd", null, null, false, [], 0);

    /// <summary>What <c>Of</c> built: the three parts of the record, read by their property names.</summary>
    private sealed record Read(object? Manifest, string? Svg, IReadOnlyList<string> Errors);

    /// <summary><c>RuntimeFilesRead&lt;TManifest&gt;</c> closed over the manifest type.</summary>
    private static Type PairOf<TManifest>() =>
        typeof(RuntimeLoader).Assembly
            .GetTypes()
            .Single(type => type.Namespace == "Dashboard.Runtime" && type.Name == "RuntimeFilesRead`1")
            .MakeGenericType(typeof(TManifest));

    /// <summary>Calls <c>RuntimeFilesRead&lt;TManifest&gt;.Of(manifest, svg, errors)</c> and reads what it built.</summary>
    private static Read Of<TManifest>(TManifest? manifest, string? svg, IReadOnlyList<string> errors)
        where TManifest : class
    {
        var pair = PairOf<TManifest>();
        var of = pair.GetMethod("Of", BindingFlags.Static | BindingFlags.NonPublic)!;
        var built = of.Invoke(null, [manifest, svg, errors])!;

        Assert.IsType(pair, built);
        return new Read(
            pair.GetProperty("Manifest")!.GetValue(built),
            (string?)pair.GetProperty("Svg")!.GetValue(built),
            (IReadOnlyList<string>)pair.GetProperty("Errors")!.GetValue(built)!);
    }

    private static void BothPartsAndNoErrorsKeepBoth<TManifest>(TManifest manifest)
        where TManifest : class
    {
        var read = Of(manifest, Picture, []);

        Assert.Same(manifest, read.Manifest);
        Assert.Same(Picture, read.Svg);
        Assert.Empty(read.Errors);
    }

    private static void EitherPartMissingKeepsNeither<TManifest>(TManifest manifest)
        where TManifest : class
    {
        IReadOnlyList<string> errors = [Reason];

        foreach (var (m, svg) in new (TManifest?, string?)[] { (null, Picture), (manifest, null), (null, null) })
        {
            var read = Of(m, svg, errors);

            Assert.Null(read.Manifest);
            Assert.Null(read.Svg);
            Assert.Same(errors, read.Errors);
        }
    }

    private static void BothPartsAndOneErrorKeepNeither<TManifest>(TManifest manifest)
        where TManifest : class
    {
        IReadOnlyList<string> errors = [Reason];

        var read = Of(manifest, Picture, errors);

        Assert.Null(read.Manifest);
        Assert.Null(read.Svg);
        Assert.Same(errors, read.Errors);
        Assert.Equal([Reason], read.Errors);
    }

    [Fact]
    public void ADiagramPairWithBothPartsAndNoErrorsKeepsBothAndAnEmptyList() => BothPartsAndNoErrorsKeepBoth(Diagram);

    [Fact]
    public void ADatabasePairWithBothPartsAndNoErrorsKeepsBothAndAnEmptyList() => BothPartsAndNoErrorsKeepBoth(Database);

    [Fact]
    public void ADiagramPairMissingEitherPartKeepsNeitherAndTheErrorsItWasGiven() => EitherPartMissingKeepsNeither(Diagram);

    [Fact]
    public void ADatabasePairMissingEitherPartKeepsNeitherAndTheErrorsItWasGiven() => EitherPartMissingKeepsNeither(Database);

    [Fact]
    public void ADiagramPairWithBothPartsAndOneErrorKeepsNeitherAndTheErrorsItWasGiven() => BothPartsAndOneErrorKeepNeither(Diagram);

    [Fact]
    public void ADatabasePairWithBothPartsAndOneErrorKeepsNeitherAndTheErrorsItWasGiven() => BothPartsAndOneErrorKeepNeither(Database);

    /// <summary>A missing part with no reason given is still refused: <c>Of</c> keeps neither part and hands back the
    /// very (empty) list it was given rather than inventing a reason - saying why is the loader's job.</summary>
    [Fact]
    public void AMissingPartWithNoErrorsKeepsNeitherAndTheEmptyListItWasGiven()
    {
        IReadOnlyList<string> none = [];

        var read = Of<RuntimeManifest>(null, Picture, none);

        Assert.Null(read.Manifest);
        Assert.Null(read.Svg);
        Assert.Same(none, read.Errors);
    }

    /// <summary>The verdict asks whether the picture stands, not what it holds: an empty picture of a manifest with no
    /// errors is kept as given (what counts as an SVG is the loader's job, pinned in <see cref="RuntimeLoaderTests" />).</summary>
    [Fact]
    public void AnEmptyPictureIsAPartThatStands()
    {
        var read = Of(Database, string.Empty, []);

        Assert.Same(Database, read.Manifest);
        Assert.Equal(string.Empty, read.Svg);
        Assert.Empty(read.Errors);
    }

    /// <summary>The record is one generic of one type parameter, closed over both manifests the loader reads.</summary>
    [Fact]
    public void ThePairIsOneGenericClosedOverBothManifests()
    {
        Assert.Equal(typeof(RuntimeManifest), PairOf<RuntimeManifest>().GetGenericArguments().Single());
        Assert.Equal(typeof(DatabaseManifest), PairOf<DatabaseManifest>().GetGenericArguments().Single());
        Assert.Equal(PairOf<RuntimeManifest>().GetGenericTypeDefinition(), PairOf<DatabaseManifest>().GetGenericTypeDefinition());
    }
}
