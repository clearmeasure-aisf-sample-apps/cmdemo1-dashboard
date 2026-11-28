using System.Globalization;
using System.Text.Json;
using Dashboard.Health;

namespace Dashboard.Runtime;

/// <summary>
/// Reads the <c>tables</c> of the database view's manifest (<see cref="DatabaseParser.ParseManifest"/>), one element at
/// a time: a table with its alias, qualified name and name is kept; one without them is refused with its own problem,
/// named by its place in the list. Only the first <see cref="DatabaseParser.MostTables"/> elements are read.
/// </summary>
public static class DatabaseTableReader
{
    /// <summary>The problem of a table without one of its required fields, after its <see cref="Path"/>.</summary>
    public const string RequiredFields = "alias, qualifiedName and name are required.";

    /// <summary>Where a table is in the manifest: <c>tables[0]</c>.</summary>
    public static string Path(int index) => string.Create(CultureInfo.InvariantCulture, $"tables[{index}]");

    /// <summary>One element of the list: the table, or (never both) the problem that refuses it.</summary>
    /// <param name="element">The element as the file has it.</param>
    /// <param name="index">Its place in the list, for the problem's <see cref="Path"/>.</param>
    public static (DatabaseTable? Table, string? Error) Read(JsonElement element, int index)
    {
        var alias = JsonRead.Text(element, "alias");
        var qualifiedName = JsonRead.Text(element, "qualifiedName");
        var name = JsonRead.Text(element, DatabaseParser.TableNameField);
        if (alias is null || qualifiedName is null || name is null)
        {
            return (null, $"{Path(index)}: {RequiredFields}");
        }

        return (new DatabaseTable(alias, qualifiedName, JsonRead.Text(element, DatabaseParser.SchemaField) ?? string.Empty, name, JsonRead.Long(element, DatabaseParser.RowsField), JsonRead.Count(element, DatabaseParser.ColumnsField)), null);
    }

    /// <summary>The tables of the manifest's list, in its order; the problem of every refused one goes to <paramref name="errors"/>.</summary>
    internal static IReadOnlyList<DatabaseTable> ReadAll(JsonElement root, List<string> errors)
    {
        var tables = new List<DatabaseTable>();
        var index = 0;
        foreach (var element in JsonRead.Items(root, DatabaseParser.TablesField).Take(DatabaseParser.MostTables))
        {
            var (table, error) = Read(element, index++);
            if (table is not null)
            {
                tables.Add(table);
            }
            else
            {
                errors.Add(error!);
            }
        }

        return tables;
    }
}
