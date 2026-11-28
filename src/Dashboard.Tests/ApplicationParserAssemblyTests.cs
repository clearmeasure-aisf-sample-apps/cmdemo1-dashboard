namespace Dashboard.Tests;

/// <summary>
/// The assembly reading of <see cref="ApplicationParser" />: an assembly without a <c>name</c> is dropped and its
/// named neighbours are kept in order; the <c>entry</c> flag falls back to a name match with the root <c>entry</c>
/// when the element has no <c>entry</c> field, and an explicit <c>entry: false</c> wins over the name match; and the
/// <see cref="ParserBounds.MostAssemblies" /> bound still counts unnamed elements.
/// </summary>
public class ApplicationParserAssemblyTests
{
    /// <summary>An assembly with no <c>name</c> is dropped and its named neighbours are kept in order.</summary>
    [Fact]
    public void ANamelessAssemblyIsDroppedAndItsNamedNeighboursAreKeptInOrder()
    {
        var architecture = ApplicationParser.Parse("""
            {
              "assemblies": [
                { "name": "One" },
                { },
                { "name": "Two" },
                { "references": ["One"] },
                { "name": "Three" }
              ]
            }
            """)!;

        Assert.Equal(["One", "Two", "Three"], architecture.Assemblies.Select(assembly => assembly.Name));
    }

    /// <summary>An <c>entry</c> field that is no boolean is treated as absent: the name match still applies.</summary>
    [Fact]
    public void AnEntryFieldThatIsNoBooleanFallsBackToTheNameMatch()
    {
        var architecture = ApplicationParser.Parse("""
            {
              "entry": "Ui.Server",
              "assemblies": [ { "name": "Ui.Server", "entry": "false" }, { "name": "Core" } ]
            }
            """)!;

        Assert.Equal([true, false], architecture.Assemblies.Select(assembly => assembly.Entry));
    }

    /// <summary>An explicit <c>entry: false</c> wins over the root <c>entry</c> name match.</summary>
    [Fact]
    public void AnExplicitFalseEntryWinsOverTheNameMatch()
    {
        var architecture = ApplicationParser.Parse("""
            {
              "entry": "Ui.Server",
              "assemblies": [ { "name": "Ui.Server", "entry": false }, { "name": "Core" } ]
            }
            """)!;

        Assert.Equal([false, false], architecture.Assemblies.Select(assembly => assembly.Entry));
    }

    /// <summary>An explicit <c>entry: true</c> marks the assembly as the entry even when its name does not match
    /// the root <c>entry</c>.</summary>
    [Fact]
    public void AnExplicitTrueEntryMarksAnAssemblyWhoseNameDoesNotMatch()
    {
        var architecture = ApplicationParser.Parse("""
            {
              "entry": "Ui.Server",
              "assemblies": [ { "name": "Other", "entry": true } ]
            }
            """)!;

        Assert.Equal([true], architecture.Assemblies.Select(assembly => assembly.Entry));
    }

    /// <summary>The <see cref="ParserBounds.MostAssemblies" /> bound counts unnamed elements: a cap-worth of unnamed
    /// assemblies followed by one named assembly gives no assemblies.</summary>
    [Fact]
    public void TheMostAssembliesBoundCountsUnnamedElements()
    {
        var nameless = string.Join(", ", Enumerable.Range(0, ParserBounds.MostAssemblies).Select(_ => "{ }"));
        var file = "{ \"assemblies\": [ " + nameless + ", { \"name\": \"Last\" } ] }";
        var architecture = ApplicationParser.Parse(file)!;

        Assert.Empty(architecture.Assemblies);
    }
}
