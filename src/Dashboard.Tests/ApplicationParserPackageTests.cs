namespace Dashboard.Tests;

/// <summary>
/// The package reading of <see cref="ApplicationParser" />: a package without a <c>name</c>, or with a <c>name</c>
/// that is no string, is dropped and its neighbours are kept in order; a <c>name</c> longer than
/// <see cref="ParserBounds.LongestWord" /> is cut to it; a package without a <c>version</c> is kept with a null
/// <see cref="ApplicationPackage.Version" />; and the <see cref="ParserBounds.MostEntries" /> bound still counts
/// nameless entries, so a thousand nameless packages followed by one named package gives no packages.
/// </summary>
public class ApplicationParserPackageTests
{
    /// <summary>A package with no <c>name</c> is dropped and its neighbours are kept in order.</summary>
    [Fact]
    public void ANamelessPackageIsDroppedAndItsNeighboursAreKeptInOrder()
    {
        var architecture = ApplicationParser.Parse("""
            {
              "assemblies": [
                { "name": "One", "packages": [ { "name": "Alpha" }, { }, { "name": "Beta" } ] },
                { "name": "Two", "packages": [ { "version": "9.9.9" } ] }
              ]
            }
            """)!;

        Assert.Equal(["One", "Two"], architecture.Assemblies.Select(assembly => assembly.Name));
        Assert.Equal(["Alpha", "Beta"], architecture.Assemblies[0].Packages.Select(package => package.Name));
        Assert.Empty(architecture.Assemblies[1].Packages);
    }

    /// <summary>A <c>name</c> that is no string is dropped and its neighbours are kept in order.</summary>
    [Fact]
    public void ANameThatIsNoStringIsDroppedAndItsNeighboursAreKeptInOrder()
    {
        var architecture = ApplicationParser.Parse("""
            {
              "assemblies": [
                { "name": "One", "packages": [ { "name": 4 }, { "name": "Alpha" }, { "name": "" }, { "name": null }, { "name": "Beta" } ] }
              ]
            }
            """)!;

        var assembly = Assert.Single(architecture.Assemblies);
        Assert.Equal(["Alpha", "Beta"], assembly.Packages.Select(package => package.Name));
    }

    /// <summary>A <c>name</c> longer than the longest text is cut to it.</summary>
    [Fact]
    public void ALongPackageNameIsCutToTheLongestText()
    {
        var word = new string('x', ParserBounds.LongestWord + 100);
        var file = "{ \"assemblies\": [ { \"name\": \"One\", \"packages\": [ { \"name\": \"" + word + "\" } ] } ] }";
        var architecture = ApplicationParser.Parse(file)!;

        var read = Assert.Single(Assert.Single(architecture.Assemblies).Packages);
        Assert.Equal(ParserBounds.AWord(ParserBounds.LongestWord), read.Name);
    }

    /// <summary>A package without a <c>version</c> is kept with a null <see cref="ApplicationPackage.Version" />.</summary>
    [Fact]
    public void APackageWithoutAVersionIsKeptWithANullVersion()
    {
        var architecture = ApplicationParser.Parse("""
            {
              "assemblies": [ { "name": "One", "packages": [ { "name": "Alpha" }, { "name": "Beta", "version": "9.9.9" } ] } ]
            }
            """)!;

        var packages = Assert.Single(architecture.Assemblies).Packages;
        Assert.Equal([new ApplicationPackage("Alpha", null), new ApplicationPackage("Beta", "9.9.9")], packages);
    }

    /// <summary>The <see cref="ParserBounds.MostEntries" /> bound still counts nameless entries: a thousand nameless
    /// packages followed by one named package gives no packages.</summary>
    [Fact]
    public void TheMostEntriesBoundCountsNamelessEntries()
    {
        var nameless = Enumerable.Range(0, ParserBounds.MostEntries).Select(number => "{ }").ToArray();
        var packages = string.Join(", ", nameless) + ", { \"name\": \"Last\" }";
        var file = "{ \"assemblies\": [ { \"name\": \"One\", \"packages\": [ " + packages + " ] } ] }";
        var architecture = ApplicationParser.Parse(file)!;

        Assert.Empty(Assert.Single(architecture.Assemblies).Packages);
    }
}
