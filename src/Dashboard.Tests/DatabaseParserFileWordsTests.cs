using Dashboard.Runtime;

namespace Dashboard.Tests;

/// <summary>Pins the file words <c>tables</c>, <c>truncated</c> and a live column's <c>name</c> that <see cref="DatabaseParser"/> reads.</summary>
public class DatabaseParserFileWordsTests
{
    private static string LiveWithColumns(string columns) =>
        $$"""{ "tables": [ { "schema": "dbo", "name": "Employee", "columns": [ {{columns}} ] } ] }""";

    [Fact]
    public void ALiveColumnWithoutANameIsSkipped()
    {
        var live = DatabaseParser.ParseLive(LiveWithColumns("""{ "type": "int" }, { "name": "Id", "type": "int", "nullable": false, "key": "PK" }, { "name": "" }"""))!;

        var column = Assert.Single(Assert.Single(live.Tables).Columns);
        Assert.Equal(new DatabaseColumn("Id", "int", false, "pk"), column);
    }

    [Fact]
    public void TheColumnCapCountsRawElementsBeforeUnnamedOnesAreSkipped()
    {
        var raw = Enumerable.Range(0, 1001).Select(index => index % 2 == 0 ? $$"""{ "name": "c{{index}}" }""" : """{ "type": "int" }""");
        var live = DatabaseParser.ParseLive(LiveWithColumns(string.Join(", ", raw)))!;

        var columns = Assert.Single(live.Tables).Columns;
        Assert.Equal(500, columns.Count);
        Assert.Equal("c0", columns[0].Name);
        Assert.Equal("c998", columns[^1].Name);
    }

    [Fact]
    public void TruncatedIsReadInBothTheManifestAndTheLiveFile()
    {
        var manifest = DatabaseParser.ParseManifest("""{ "environment": "tdd", "database": "db", "truncated": true, "tables": [] }""");
        var live = DatabaseParser.ParseLive("""{ "truncated": true, "tables": [] }""");

        Assert.True(manifest.Value!.Truncated);
        Assert.True(live!.Truncated);
    }

    [Fact]
    public void ANonArrayTablesIsTheSameRefusalInTheManifestAndNoLiveFile()
    {
        var manifest = DatabaseParser.ParseManifest("""{ "environment": "tdd", "database": "db", "tables": 3 }""");

        Assert.Null(manifest.Value);
        Assert.Equal(["tables: not an array."], manifest.Errors);
        Assert.Null(DatabaseParser.ParseLive("""{ "tables": 3 }"""));
    }
}
