using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>The per-table reader of the database view's manifest (#1426): one home for the path, the required fields and the limit.</summary>
public partial class DatabaseViewTests
{
    private static (DatabaseTable? Table, string? Error) ReadTable(string json, int index)
    {
        using var document = JsonDocument.Parse(json);
        return DatabaseTableReader.Read(document.RootElement, index);
    }

    [Fact]
    public void ATableWithItsRequiredFieldsIsReadWithNoProblem()
    {
        var (table, error) = ReadTable("""{ "alias": "t_dbo_A", "qualifiedName": "db.t_dbo_A", "schema": "dbo", "name": "A", "rows": 5, "columns": 3 }""", 0);

        Assert.Equal(new DatabaseTable("t_dbo_A", "db.t_dbo_A", "dbo", "A", 5, 3), table);
        Assert.Null(error);
    }

    [Theory]
    [InlineData("""{ "qualifiedName": "db.t_dbo_A", "name": "A" }""", 0, "tables[0]: alias, qualifiedName and name are required.")]
    [InlineData("""{ "alias": "t_dbo_A", "name": "A" }""", 7, "tables[7]: alias, qualifiedName and name are required.")]
    [InlineData("""{ "alias": "t_dbo_A", "qualifiedName": "db.t_dbo_A", "name": "" }""", 12, "tables[12]: alias, qualifiedName and name are required.")]
    [InlineData("3", 1999, "tables[1999]: alias, qualifiedName and name are required.")]
    public void ATableWithoutARequiredFieldIsRefusedWithItsOwnProblem(string json, int index, string problem)
    {
        var (table, error) = ReadTable(json, index);

        Assert.Null(table);
        Assert.Equal(problem, error);
        Assert.Equal($"{DatabaseTableReader.Path(index)}: {DatabaseTableReader.RequiredFields}", error);
    }

    [Fact]
    public void EveryRefusedTableIsAProblemInTheOrderOfTheList()
    {
        var result = DatabaseParser.ParseManifest("""
            { "database": "db", "tables": [ { "alias": "a" }, { "alias": "t_dbo_B", "qualifiedName": "db.t_dbo_B", "name": "B" }, { } ] }
            """);

        Assert.False(result.IsValid);
        Assert.Equal(
            ["environment: missing or empty.", "tables[0]: alias, qualifiedName and name are required.", "tables[2]: alias, qualifiedName and name are required."],
            result.Errors);
    }

    [Fact]
    public void OnlyTheFirstTwoThousandTablesAreRead()
    {
        var json = new StringBuilder("""{ "environment": "tdd", "database": "db", "tables": [""");
        for (var index = 0; index < 2000; index++)
        {
            json.Append(CultureInfo.InvariantCulture, $$"""{ "alias": "t_dbo_T{{index}}", "qualifiedName": "db.t_dbo_T{{index}}", "name": "T{{index}}" },""");
        }

        json.Append("{ } ] }");

        var result = DatabaseParser.ParseManifest(json.ToString());

        Assert.True(result.IsValid);
        Assert.Equal(2000, result.Value!.Tables.Count);
        Assert.Equal(2000, result.Value.Drawn);
        Assert.Equal("T1999", result.Value.Tables[^1].Name);
    }
}
