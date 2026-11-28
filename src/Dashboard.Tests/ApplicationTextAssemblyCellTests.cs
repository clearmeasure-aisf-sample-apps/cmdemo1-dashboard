namespace Dashboard.Tests;

/// <summary>
/// The assembly-table cells of the application view: <see cref="ApplicationText.References"/> and
/// <see cref="ApplicationText.Packages" /> join an assembly's lists the way the table's words say them - with
/// <c>", "</c>, in the order the file put them in, empty when there are none.
/// </summary>
public class ApplicationTextAssemblyCellTests
{
    private static ApplicationAssembly Assembly(
        string name = "Ui.Server",
        IReadOnlyList<string> references = null!,
        IReadOnlyList<ApplicationPackage> packages = null!) =>
        new(name, Entry: false, references ?? [], packages ?? []);

    // ------------------------------------------------------------------ References

    /// <summary>An assembly with no references renders the empty string, not a separator or a word for the nothing.</summary>
    [Fact]
    public void NoReferencesRenderAnEmptyString() =>
        Assert.Equal("", ApplicationText.References(Assembly(references: [])));

    /// <summary>One reference is said alone, with no separator about it.</summary>
    [Fact]
    public void OneReferenceIsSaidAlone() =>
        Assert.Equal("Core", ApplicationText.References(Assembly(references: ["Core"])));

    /// <summary>Several references are joined with <c>", "</c> in the order the file put them in.</summary>
    [Fact]
    public void SeveralReferencesAreJoinedInOrder() =>
        Assert.Equal("Core, DataAccess, Logging", ApplicationText.References(Assembly(references: ["Core", "DataAccess", "Logging"])));

    // ------------------------------------------------------------------ Packages

    /// <summary>An assembly with no packages renders the empty string, not a separator or a word for the nothing.</summary>
    [Fact]
    public void NoPackagesRenderAnEmptyString() =>
        Assert.Equal("", ApplicationText.Packages(Assembly(packages: [])));

    /// <summary>A package with a version is named <c>Name Version</c>.</summary>
    [Fact]
    public void APackageWithAVersionIsNamedWithItsVersion() =>
        Assert.Equal("Serilog 4.0.1", ApplicationText.Packages(Assembly(packages: [new ApplicationPackage("Serilog", "4.0.1")])));

    /// <summary>A package without a version is named by its name alone, with no trailing space.</summary>
    [Fact]
    public void APackageWithoutAVersionIsNamedByItsNameAlone() =>
        Assert.Equal("MediatR", ApplicationText.Packages(Assembly(packages: [new ApplicationPackage("MediatR", null)])));

    /// <summary>Several packages are joined with <c>", "</c> in the order the file put them in, each named as the list names it.</summary>
    [Fact]
    public void SeveralPackagesAreJoinedInOrder() =>
        Assert.Equal(
            "Serilog 4.0.1, MediatR, Dapper 2.1.35",
            ApplicationText.Packages(Assembly(packages: [new("Serilog", "4.0.1"), new("MediatR", null), new("Dapper", "2.1.35")])));

    // ------------------------------------------------------------------ Null refusal

    /// <summary>A null assembly is refused by the name of the argument, in both cells.</summary>
    [Fact]
    public void ANullAssemblyIsRefusedByNameInBothCells()
    {
        Assert.Equal("assembly", Assert.Throws<ArgumentNullException>(() => ApplicationText.References(null!)).ParamName);
        Assert.Equal("assembly", Assert.Throws<ArgumentNullException>(() => ApplicationText.Packages(null!)).ParamName);
    }

    // ------------------------------------------------------------------ Culture independence

    /// <summary>The cells' words do not depend on the reader's culture: the separator is the literal <c>", "</c>, not a
    /// culture's list separator, and the names are said as the file spelled them.</summary>
    [Fact]
    public void TheCellsDoNotDependOnCulture()
    {
        var assembly = Assembly(
            references: ["Core", "DataAccess"],
            packages: [new ApplicationPackage("Serilog", "4.0.1"), new ApplicationPackage("MediatR", null)]);

        CultureScope.Runs("fi-FI", () =>
        {
            Assert.Equal("Core, DataAccess", ApplicationText.References(assembly));
            Assert.Equal("Serilog 4.0.1, MediatR", ApplicationText.Packages(assembly));
        });

        CultureScope.Runs("ru-RU", () =>
        {
            Assert.Equal("Core, DataAccess", ApplicationText.References(assembly));
            Assert.Equal("Serilog 4.0.1, MediatR", ApplicationText.Packages(assembly));
        });
    }
}
