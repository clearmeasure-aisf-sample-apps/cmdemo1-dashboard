namespace Dashboard.Tests;

/// <summary>
/// The small words helpers of the application and database views: which deployable name the address takes, how a
/// package and a nullable column are named, what the note of a refused version says, and where a version came from.
/// </summary>
public class RuntimeTextHelperTests
{
    [Theory]
    [InlineData("ui", true)]
    [InlineData("UI", true)]
    [InlineData("ui-server", true)]
    [InlineData("ui_server", true)]
    [InlineData("ui2", true)]
    [InlineData("-_-", true)]
    [InlineData(ApplicationNames.Longest, true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(ApplicationNames.LongestPlusOne, false)]
    [InlineData("ui.server", false)]
    [InlineData("ui+1", false)]
    [InlineData("ui server", false)]
    [InlineData(" ui", false)]
    [InlineData("ui ", false)]
    [InlineData("ui/server", false)]
    [InlineData("ui\\server", false)]
    [InlineData("ui%2fserver", false)]
    [InlineData("ui<script>", false)]
    [InlineData("../ui", false)]
    [InlineData("café", false)]
    [InlineData("１", false)]
    public void ADeployableIsNamedByLettersDigitsDashAndUnderscoreOfUpTo64Characters(string? deployable, bool isName) =>
        Assert.Equal(isName, ApplicationText.IsName(deployable));

    [Fact]
    public void ARefusedVersionNamesNoFileAndTheViewSaysSo()
    {
        Assert.Equal("The version of ui names no file: no diagram is asked for.", ApplicationText.RefusedLine("ui"));
        Assert.Equal("The version of ui-server names no file: no diagram is asked for.", ApplicationText.RefusedLine("ui-server"));
    }

    [Theory]
    [InlineData(true, "The version a node of ui in uat reported; the primary's when the nodes run several.")]
    [InlineData(false, "No node of ui in uat reported a version: this is the one the environment pins in Git.")]
    public void TheTitleSaysWhichOfTheTwoVersionsTheViewShows(bool runs, string title) =>
        Assert.Equal(title, ApplicationText.VersionTitle("ui", "uat", new ApplicationVersion("1.0.42", runs)));

    [Fact]
    public void TheTitleOfNoKnownVersionSaysNothingIsAskedFor() =>
        Assert.Equal(
            "No node of ui in uat reported a version, and the environment pins none: no diagram is asked for.",
            ApplicationText.VersionTitle("ui", "uat", null));

    [Fact]
    public void TheTitleNamesTheDeployableAndTheEnvironmentAndNeverTheVersion()
    {
        // The version text is nowhere in the title, so only the two names and the state matter to it.
        Assert.Equal(
            ApplicationText.VersionTitle("ui", "uat", new ApplicationVersion("1.0.42", Runs: true)),
            ApplicationText.VersionTitle("ui", "uat", new ApplicationVersion("9.9.9-rc_1+7", Runs: true)));

        Assert.Equal(
            "The version a node of api in pre-prod reported; the primary's when the nodes run several.",
            ApplicationText.VersionTitle("api", "pre-prod", new ApplicationVersion("3.1.0", Runs: true)));
        Assert.Equal(
            "No node of api in pre-prod reported a version: this is the one the environment pins in Git.",
            ApplicationText.VersionTitle("api", "pre-prod", new ApplicationVersion("3.1.0", Runs: false)));
    }

    [Theory]
    [InlineData("Serilog", "4.0.1", "Serilog 4.0.1")]
    [InlineData("Serilog", "8.0.0-preview.7", "Serilog 8.0.0-preview.7")]
    [InlineData("MediatR", null, "MediatR")]
    // ApplicationParser never yields an empty version (JsonRead.Text leaves no empty text), but a package built
    // without one keeps the space the version would have filled.
    [InlineData("MediatR", "", "MediatR ")]
    public void APackageIsNamedWithItsVersionWhenItHasOne(string name, string? version, string text) =>
        Assert.Equal(text, ApplicationText.Package(new ApplicationPackage(name, version)));

    [Fact]
    public void ThereIsNoPackageToNameWithoutAPackage() =>
        Assert.Throws<ArgumentNullException>(() => ApplicationText.Package(null!));

    [Fact]
    public void ThePackagesTheFileGivesAreNamedByTheListTheSameWay()
    {
        var architecture = ApplicationParser.Parse("""
            {
              "entry": "Ui.Server",
              "assemblies": [ { "name": "Ui.Server", "packages": [
                { "name": "Serilog", "version": "4.0.1" },
                { "name": "MediatR" },
                { "name": "Dapper", "version": "" },
                { "name": "Polly", "version": " 8.4.1 " },
                { "name": 4 },
                { "version": "1.0.0" } ] } ]
            }
            """)!;

        Assert.Equal(
            [new ApplicationPackage("Serilog", "4.0.1"), new ApplicationPackage("MediatR", null), new ApplicationPackage("Dapper", null), new ApplicationPackage("Polly", "8.4.1")],
            architecture.Assemblies[0].Packages);
        Assert.Equal(["Serilog 4.0.1", "MediatR", "Dapper", "Polly 8.4.1"], architecture.Assemblies[0].Packages.Select(ApplicationText.Package));
    }

    [Theory]
    [InlineData(true, "yes")]
    [InlineData(false, "no")]
    [InlineData(null, "–")]
    public void AColumnTakesNullOrDoesNotOrTheFileDoesNotSay(bool? nullable, string word) =>
        Assert.Equal(word, DatabaseText.Nullable(nullable));

    [Fact]
    public void TheWordForANotSaidNullableIsTheDashAndNotAHyphen()
    {
        Assert.Equal("–", DatabaseText.Nullable(null));
        Assert.Equal("\u2013", DatabaseText.Nullable(null));
        Assert.NotEqual("-", DatabaseText.Nullable(null));
    }

    [Theory]
    [InlineData("dbo", "Employee", "dbo.Employee")]
    [InlineData("", "Employee", "Employee")]
    [InlineData("Sales", "Order Detail", "Sales.Order Detail")]
    [InlineData("dbo", "", "dbo.")]
    [InlineData("HROrg", "Employee", "HROrg.Employee")]
    public void ATableIsNamedByItsSchemaAndItsName(string schema, string name, string qualified) =>
        Assert.Equal(qualified, DatabaseText.QualifiedTable(schema, name));

    [Fact]
    public void ATableOfTheFileWithoutASchemaIsNamedByItsNameAlone()
    {
        var manifest = DatabaseParser.ParseManifest("""
            {
              "environment": "tdd", "database": "sqldb-cmdemo1-tdd",
              "tables": [
                { "alias": "t_dbo_Employee", "qualifiedName": "db.t_dbo_Employee", "schema": "dbo", "name": "Employee" },
                { "alias": "t__Orphan", "qualifiedName": "db.t__Orphan", "name": "Orphan" } ]
            }
            """).Value!;

        Assert.Equal(["dbo", ""], manifest.Tables.Select(table => table.Schema));
        Assert.Equal("dbo.Employee", DatabaseText.QualifiedTable(manifest.Tables[0].Schema, manifest.Tables[0].Name));
        Assert.Equal("Orphan", DatabaseText.QualifiedTable(manifest.Tables[1].Schema, manifest.Tables[1].Name));
        Assert.Equal(["dbo.Employee", "Orphan"], DatabaseText.List(manifest, live: null).Select(row => row.Name));
    }
}
