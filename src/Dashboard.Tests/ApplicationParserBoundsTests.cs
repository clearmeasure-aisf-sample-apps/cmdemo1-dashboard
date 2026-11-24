namespace Dashboard.Tests;

/// <summary>
/// The three bounds of <see cref="ApplicationParser" />, asked by name rather than by number: the longest word of
/// the file, the most assemblies and the most references or packages of one assembly - each at the bound, one under,
/// one over and a hundred times over. The numbers live once, in <see cref="ParserBounds" />, and are pinned here
/// against the class that holds them.
/// </summary>
public class ApplicationParserBoundsTests
{
    public static TheoryData<string, int> Bounds()
    {
        var data = new TheoryData<string, int>();
        foreach (var (name, value) in ParserBounds.ByProductionName)
        {
            data.Add(name, value);
        }

        return data;
    }

    public static TheoryData<int> AroundTheLongestWord() => new(ParserBounds.Around(ParserBounds.LongestWord));

    public static TheoryData<int> AroundTheMostAssemblies() => new(ParserBounds.Around(ParserBounds.MostAssemblies));

    public static TheoryData<int> AroundTheMostEntries() => new(ParserBounds.Around(ParserBounds.MostEntries));

    /// <summary>Each number of <see cref="ParserBounds" /> is the one the parser holds under its constant's name, read
    /// by reflection so the pin holds whether the constant stays private or is stated publicly.</summary>
    [Theory]
    [MemberData(nameof(Bounds))]
    public void TheBoundsAreTheOnesTheClassHolds(string name, int bound) =>
        Assert.Equal(bound, ParserBounds.Held(name));

    /// <summary>The bounds the tests rely on are the three the parser has: a new one is to be named here too.</summary>
    [Fact]
    public void EveryBoundOfTheParserIsNamed()
    {
        var held = typeof(ApplicationParser)
            .GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(int))
            .Select(field => field.Name)
            .Order(StringComparer.Ordinal);

        Assert.Equal(ParserBounds.ByProductionName.Keys.Order(StringComparer.Ordinal), held);
    }

    [Theory]
    [MemberData(nameof(AroundTheLongestWord))]
    public void TheProcessIsCutAtTheLongestWord(int letters)
    {
        var architecture = ApplicationParser.Parse(ParserBounds.AFileWithAProcessOf(letters))!;

        Assert.Equal(ParserBounds.AWord(ParserBounds.Kept(letters, ParserBounds.LongestWord)), architecture.Process);
    }

    [Theory]
    [MemberData(nameof(AroundTheLongestWord))]
    public void AnAssemblysNameAndReferenceAreCutAtTheLongestWord(int letters)
    {
        var assembly = Assert.Single(ApplicationParser.Parse(ParserBounds.AFileWithAnAssemblyNamedBy(letters))!.Assemblies);
        var kept = ParserBounds.AWord(ParserBounds.Kept(letters, ParserBounds.LongestWord));

        Assert.Equal(kept, assembly.Name);
        Assert.Equal(kept, Assert.Single(assembly.References));
    }

    [Theory]
    [MemberData(nameof(AroundTheMostAssemblies))]
    public void AFileKeepsTheMostAssembliesAndNoMore(int assemblies)
    {
        var architecture = ApplicationParser.Parse(ParserBounds.AFileOfAssemblies(assemblies))!;
        var kept = ParserBounds.Kept(assemblies, ParserBounds.MostAssemblies);

        Assert.Equal(kept, architecture.Assemblies.Count);
        Assert.Equal($"A{kept - 1}", architecture.Assemblies[^1].Name);
    }

    [Theory]
    [MemberData(nameof(AroundTheMostEntries))]
    public void AnAssemblyKeepsTheMostReferencesAndNoMore(int references)
    {
        var assembly = Assert.Single(ApplicationParser.Parse(ParserBounds.AFileWithReferences(references))!.Assemblies);
        var kept = ParserBounds.Kept(references, ParserBounds.MostEntries);

        Assert.Equal(kept, assembly.References.Count);
        Assert.Equal($"R{kept - 1}", assembly.References[^1]);
    }

    [Theory]
    [MemberData(nameof(AroundTheMostEntries))]
    public void AnAssemblyKeepsTheMostPackagesAndNoMore(int packages)
    {
        var assembly = Assert.Single(ApplicationParser.Parse(ParserBounds.AFileWithPackages(packages))!.Assemblies);
        var kept = ParserBounds.Kept(packages, ParserBounds.MostEntries);

        Assert.Equal(kept, assembly.Packages.Count);
        Assert.Equal($"P{kept - 1}", assembly.Packages[^1].Name);
    }

    /// <summary>The count of the most assemblies a file ever lists is said whole in the diagram's alternative text,
    /// and so is the count of the most entries of one assembly.</summary>
    [Fact]
    public void TheAltTextSaysTheBoundsWhole()
    {
        Assert.Equal(
            $"Assemblies of ui 1.0.42: {ParserBounds.MostAssemblies} assemblies",
            ApplicationText.AltText("ui", "1.0.42", ParserBounds.AnArchitectureOf(ParserBounds.MostAssemblies)));
        Assert.Equal(
            $"Assemblies of ui 1.0.42: {ParserBounds.MostEntries} assemblies",
            ApplicationText.AltText("ui", "1.0.42", ParserBounds.AnArchitectureOf(ParserBounds.MostEntries)));
    }

    /// <summary>The most assemblies a file can give the view is what its alternative text counts: a file a hundred
    /// times over the bound is said as the bound.</summary>
    [Fact]
    public void AFileWayPastTheMostAssembliesIsCountedAtTheBound()
    {
        var architecture = ApplicationParser.Parse(ParserBounds.AFileOfAssemblies(ParserBounds.MostAssemblies * ParserBounds.WayPast))!;

        Assert.Equal($"Assemblies of ui 1.0.42: {ParserBounds.MostAssemblies} assemblies", ApplicationText.AltText("ui", "1.0.42", architecture));
    }
}
