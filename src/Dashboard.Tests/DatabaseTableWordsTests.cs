using Dashboard.Runtime;

namespace Dashboard.Tests;

/// <summary>
/// The words of a table in the manifest and in the live file — <c>name</c>, <c>schema</c>, <c>rows</c> and
/// <c>columns</c> — are named once on <see cref="DatabaseParser"/> and both readers use the same consts, so a table
/// built from the same element carries the same values in both.
/// </summary>
public class DatabaseTableWordsTests
{
    [Fact]
    public void AManifestTableAndALiveTableFromTheSameElementCarryTheSameValues()
    {
        const string manifestElement = """{ "alias": "t_dbo_Employee", "qualifiedName": "dbo.Employee", "schema": "dbo", "name": "Employee", "rows": 42, "columns": [ { "name": "Id", "type": "int", "nullable": false, "key": "pk" } ] }""";
        const string liveElement = """{ "schema": "dbo", "name": "Employee", "rows": 42, "columns": [ { "name": "Id", "type": "int", "nullable": false, "key": "pk" } ] }""";

        var manifest = DatabaseParser.ParseManifest($$"""{ "environment": "tdd", "database": "db", "tables": [ {{manifestElement}} ] }""");
        var live = DatabaseParser.ParseLive($$"""{ "tables": [ {{liveElement}} ] }""");

        var manifestTable = Assert.Single(manifest.Value!.Tables);
        var liveTable = Assert.Single(live!.Tables);

        Assert.Equal(manifestTable.Schema, liveTable.Schema);
        Assert.Equal(manifestTable.Name, liveTable.Name);
        Assert.Equal(manifestTable.Rows, liveTable.Rows);
        Assert.Equal("dbo", manifestTable.Schema);
        Assert.Equal("Employee", manifestTable.Name);
        Assert.Equal(42, manifestTable.Rows);
        Assert.Single(liveTable.Columns);
    }

    [Fact]
    public void AManifestTableWithoutSchemaReadsEmptyAndSoDoesALiveTable()
    {
        var manifest = DatabaseParser.ParseManifest("""{ "environment": "tdd", "database": "db", "tables": [ { "alias": "t", "qualifiedName": "q", "name": "Employee" } ] }""");
        var live = DatabaseParser.ParseLive("""{ "tables": [ { "name": "Employee" } ] }""");

        Assert.Equal(string.Empty, Assert.Single(manifest.Value!.Tables).Schema);
        Assert.Equal(string.Empty, Assert.Single(live!.Tables).Schema);
    }

    [Fact]
    public void ALiveTableWithoutANameIsSkippedAndSoIsALiveColumnWithoutAName()
    {
        const string json = """{ "tables": [ { "schema": "dbo", "rows": 5 }, { "name": "Employee", "columns": [ { "type": "int" } ] } ] }""";
        var live = DatabaseParser.ParseLive(json)!;

        var table = Assert.Single(live.Tables);
        Assert.Equal("Employee", table.Name);
        Assert.Empty(table.Columns);
    }
}
