using System.Globalization;
using System.Text.Json;
using Dashboard.Health;

namespace Dashboard.Runtime;

/// <summary>
/// <c>runtime/&lt;env&gt;.database.json</c>: the schema of the environment's database as the deployment read it, and
/// which drawn table of <c>runtime/&lt;env&gt;.database.svg</c> is which.
/// </summary>
/// <param name="Environment">The environment's name.</param>
/// <param name="Database">The database's name.</param>
/// <param name="ReadAt">When the deployment read the schema; null when the file does not say.</param>
/// <param name="Source">
/// The live file's address (<see cref="DatabaseLive"/>), on another origin; null without one, and for anything but an
/// absolute https address.
/// </param>
/// <param name="Truncated">The database has more tables than the diagram draws.</param>
/// <param name="Tables">The tables the diagram draws.</param>
/// <param name="Drawn">How many tables the diagram draws; as many as <paramref name="Tables"/> when the file does not say.</param>
public sealed record DatabaseManifest(
    string Environment,
    string Database,
    DateTimeOffset? ReadAt,
    Uri? Source,
    bool Truncated,
    IReadOnlyList<DatabaseTable> Tables,
    int Drawn);

/// <param name="Alias">The table's name in the PlantUML source (<see cref="DatabaseText.Alias"/>).</param>
/// <param name="QualifiedName">The <c>data-qualified-name</c> of the table's group in the SVG.</param>
/// <param name="Rows">The rows counted when the schema was read; null when the file has no count.</param>
/// <param name="Columns">How many columns the table has; null when the file does not say.</param>
public sealed record DatabaseTable(string Alias, string QualifiedName, string Schema, string Name, long? Rows, int? Columns);

/// <summary>
/// The live file (<see cref="DatabaseManifest.Source"/>): the same schema read again later, with every column. It is
/// not the dashboard's own file, so every part of it is optional.
/// </summary>
/// <param name="Generated">When it was written; null when the file does not say.</param>
public sealed record DatabaseLive(DateTimeOffset? Generated, IReadOnlyList<DatabaseLiveTable> Tables, bool Truncated);

public sealed record DatabaseLiveTable(string Schema, string Name, long? Rows, IReadOnlyList<DatabaseColumn> Columns);

/// <param name="Key">Empty, <c>pk</c>, <c>fk</c> or <c>pk,fk</c>.</param>
public sealed record DatabaseColumn(string Name, string Type, bool? Nullable, string Key);

/// <summary>The runtime view asks for the database view of an environment, or for its diagram again.</summary>
/// <param name="Shown">The database view is to be shown.</param>
/// <param name="Step">The change is a step of the browser's history: its Back undoes it.</param>
public sealed record RuntimeDatabaseChange(string Environment, bool Shown, bool Step);

/// <summary>A drawn table and the words of its row count: what <c>js/database.js</c> writes into the diagram.</summary>
public sealed record DatabaseCount(string QualifiedName, string Text);

/// <summary>One line of "Tables as a list".</summary>
public sealed record DatabaseListRow(string Name, int? Columns, long? Rows);

/// <summary>
/// Reads the database view's two files. The manifest is the deployment's (<c>deploy-staticwebapp.ps1</c>). Required:
/// its environment, its database and, of every table, alias, qualified name and name. Unknown fields are ignored. The
/// live file is somebody else's: a file that cannot be read is no file, never an error.
/// </summary>
public static class DatabaseParser
{
    /// <summary>The most tables and the most columns of one table that are kept.</summary>
    internal const int MostTables = 2000;
    private const int MostColumns = 1000;

    /// <summary>The file word of the list of tables, in the manifest and in the live file.</summary>
    internal const string TablesField = "tables";

    /// <summary>
    /// The file word of a table's name (and of a column's name), in the manifest and in the live file. The manifest
    /// table and the live table are matched by schema and name (<see cref="DatabaseText"/>), so both readers must use
    /// the same word.
    /// </summary>
    internal const string TableNameField = "name";

    /// <summary>The file word of a table's schema, in the manifest and in the live file.</summary>
    internal const string SchemaField = "schema";

    /// <summary>The file word of a table's row count, in the manifest and in the live file.</summary>
    internal const string RowsField = "rows";

    /// <summary>The file word of a table's column list, in the manifest and in the live file.</summary>
    internal const string ColumnsField = "columns";

    /// <summary>The file word of the flag that the database has more tables than are listed.</summary>
    private const string TruncatedField = "truncated";

    /// <summary>The file word of the environment's name, required in the manifest.</summary>
    private const string EnvironmentField = "environment";

    /// <summary>The file word of the database's name, required in the manifest.</summary>
    private const string DatabaseField = "database";

    /// <summary>The file word of when the deployment read the schema.</summary>
    private const string ReadAtField = "readAt";

    /// <summary>The file word of how many tables the diagram draws.</summary>
    private const string DrawnField = "drawn";

    /// <summary>The file word of the live file's address.</summary>
    private const string SourceField = "source";

    /// <summary>The file word of when the live file was written.</summary>
    private const string GeneratedField = "generated";

    /// <summary>The manifest: its shape checked here, its names, its tables and the rest each read in their own step.</summary>
    public static RuntimeParseResult<DatabaseManifest> ParseManifest(string? json)
    {
        if (!RuntimeManifestParser.TryOpen(json, out var document, out var problem))
        {
            return RuntimeParseResult<DatabaseManifest>.Failed(problem);
        }

        using (document)
        {
            var root = document.RootElement;
            var errors = new List<string>();
            var names = ReadManifestFields(root, errors);
            if (root.TryGetProperty(TablesField, out var array) && array.ValueKind is not (JsonValueKind.Array or JsonValueKind.Null))
            {
                errors.Add($"{TablesField}: not an array.");
            }

            var tables = DatabaseTableReader.ReadAll(root, errors);
            return RuntimeParseResult<DatabaseManifest>.Collected(errors, () => names is { } read ? Build(root, read, tables) : null);
        }
    }

    /// <summary>The manifest's two required names; null, with a problem for each one missing, when either is.</summary>
    private static ManifestNames? ReadManifestFields(JsonElement root, List<string> errors)
    {
        var environment = JsonRead.Text(root, EnvironmentField);
        var database = JsonRead.Text(root, DatabaseField);
        if (environment is null)
        {
            errors.Add(FileProblems.MissingOrEmpty(EnvironmentField));
        }

        if (database is null)
        {
            errors.Add(FileProblems.MissingOrEmpty(DatabaseField));
        }

        return environment is null || database is null ? null : new ManifestNames(environment, database);
    }

    /// <summary>The manifest of the read names and tables, with the optional fields of the file.</summary>
    private static DatabaseManifest Build(JsonElement root, ManifestNames names, IReadOnlyList<DatabaseTable> tables) =>
        new(
            names.Environment,
            names.Database,
            JsonRead.Time(root, ReadAtField),
            Source(root),
            JsonRead.Flag(root, TruncatedField) ?? false,
            tables,
            JsonRead.Count(root, DrawnField) ?? tables.Count);

    private readonly record struct ManifestNames(string Environment, string Database);

    /// <summary>The live file; null for anything that is no JSON object with a list of tables.</summary>
    public static DatabaseLive? ParseLive(string? json)
    {
        if (!RuntimeManifestParser.TryOpen(json, out var document, out _))
        {
            return null;
        }

        using (document)
        {
            var root = document.RootElement;
            if (!JsonRead.HasArray(root, TablesField))
            {
                return null;
            }

            var tables = new List<DatabaseLiveTable>();
            foreach (var element in JsonRead.Items(root, TablesField).Take(MostTables))
            {
                if (JsonRead.Text(element, TableNameField) is not { } name)
                {
                    continue;
                }

                var columns = JsonRead.Items(element, ColumnsField)
                    .Take(MostColumns)
                    .Select(ReadColumn)
                    .OfType<DatabaseColumn>()
                    .ToList();
                tables.Add(new DatabaseLiveTable(JsonRead.Text(element, SchemaField) ?? string.Empty, name, JsonRead.Long(element, RowsField), columns));
            }

            return new DatabaseLive(JsonRead.Time(root, GeneratedField), tables, JsonRead.Flag(root, TruncatedField) ?? false);
        }
    }

    /// <summary>A column of the live file; null for one without a name, which is left out.</summary>
    private static DatabaseColumn? ReadColumn(JsonElement column) =>
        JsonRead.Text(column, TableNameField) is { } name
            ? new DatabaseColumn(
                name,
                JsonRead.Text(column, "type") ?? string.Empty,
                JsonRead.Flag(column, "nullable"),
                JsonRead.Text(column, "key")?.ToLowerInvariant() ?? string.Empty)
            : null;

    /// <summary>The live file is fetched by the page itself: only an absolute https address is kept.</summary>
    private static Uri? Source(JsonElement root) =>
        JsonRead.Address(root, SourceField) is { } address && address.Scheme == Uri.UriSchemeHttps ? address : null;
}

/// <summary>The words of the database view, and what it decides: which count a table shows, whether the schema changed.</summary>
public static class DatabaseText
{
    /// <summary>
    /// The database list's word for a value the live file does not give: a nullable nobody stated, a count nobody
    /// counted. U+2013 (EN DASH), not a hyphen.
    /// </summary>
    public const string NotSaid = "\u2013";

    /// <summary>A count as the list writes it: <c>1,234</c>, thousands grouped, no decimals, invariant culture.</summary>
    private const string CountFormat = "#,0";

    /// <summary>The line the view shows when the live file's tables are not the diagram's.</summary>
    public const string SchemaChanged = "The schema changed since this diagram was drawn: deploy the dashboard again.";

    /// <summary>
    /// A row count as the diagram writes it: <c>1,234 rows</c>, <c>1 row</c>. (In the SVG the space is a no-break
    /// space, which <c>js/database.js</c> reads and writes.)
    /// </summary>
    public static string Rows(long rows) => Plural.Of(rows, "row");

    /// <summary>
    /// A table's alias: <c>t_</c>, the schema, <c>_</c> and the name, with every character outside
    /// <c>[A-Za-z0-9_]</c> replaced by <c>_</c>.
    /// </summary>
    public static string Alias(string schema, string name) =>
        string.Concat($"t_{schema}_{name}".Select(character => char.IsAsciiLetterOrDigit(character) || character == '_' ? character : '_'));

    /// <summary>A table as the list names it: <c>dbo.Employee</c>.</summary>
    public static string QualifiedTable(string schema, string name) => schema.Length == 0 ? name : $"{schema}.{name}";

    /// <summary>
    /// The node of the runtime diagram that opens the database view: the environment's database, the first node of
    /// kind <c>sql</c>; null in a diagram without one.
    /// </summary>
    public static RuntimeNode? Box(RuntimeManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        return manifest.Nodes.FirstOrDefault(node => node.Kind == RuntimeNodeKind.Sql);
    }

    /// <summary>The accessible name of the database's box in the runtime diagram.</summary>
    public static string BoxLabel(RuntimeNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return $"Database {node.Name}: open its tables";
    }

    /// <summary>
    /// The links of the view's heading row: where the name of the database's box led before the box zoomed, as
    /// <see cref="Health.LinkText.OpenInPortal"/>. That is the environment's link to its database in the topology, else the link the
    /// deployment wrote for the node into the manifest; none without either.
    /// </summary>
    /// <param name="box">The database's node of the runtime diagram.</param>
    /// <param name="environment">The links of the environment in the topology; null when it has none.</param>
    public static IReadOnlyList<ViewLink> Links(RuntimeNode box, Health.LinkSet? environment)
    {
        ArgumentNullException.ThrowIfNull(box);
        var link = RuntimeLink.To(environment?[Health.LinkSet.Database], Health.LinkText.For(Health.LinkSet.Database, box.Name)) ?? RuntimeBoxLink.Of(box);
        if (link is null)
        {
            return [];
        }

        var portal = link.Title.EndsWith(Health.LinkText.PortalSuffix, StringComparison.Ordinal);
        return [new ViewLink(portal ? Health.LinkText.OpenInPortal : Health.LinkText.Open(box.Name), link.Href, link.Title)];
    }

    /// <summary>
    /// The count every drawn table shows: the live file's where it has the table with a count, else the one the
    /// deployment drew. A table neither counted is left as drawn.
    /// </summary>
    public static IReadOnlyList<DatabaseCount> Counts(DatabaseManifest manifest, DatabaseLive? live)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        var counted = LiveByAlias(live);
        return
        [
            .. manifest.Tables
                .Select(table => (table.QualifiedName, Rows: counted.TryGetValue(table.Alias, out var found) && found.Rows is { } rows ? rows : table.Rows))
                .Where(count => count.Rows is not null)
                .Select(count => new DatabaseCount(count.QualifiedName, Rows(count.Rows!.Value))),
        ];
    }

    /// <summary>
    /// Whether the live file's tables are other tables than the diagram's: one the diagram draws is gone, or there is
    /// one it does not draw. A diagram that draws only the first tables (<see cref="DatabaseManifest.Truncated"/>)
    /// lacks tables by design, so there only a drawn table that is gone counts.
    /// </summary>
    public static bool HasSchemaChanged(DatabaseManifest manifest, DatabaseLive? live)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        if (live is null)
        {
            return false;
        }

        var drawn = manifest.Tables.Select(table => table.Alias).ToHashSet(StringComparer.Ordinal);
        var counted = LiveByAlias(live);
        return drawn.Any(alias => !counted.ContainsKey(alias))
            || (!manifest.Truncated && counted.Keys.Any(alias => !drawn.Contains(alias)));
    }

    /// <summary>
    /// The moment the view's facts are of: when the deployment read the schema, or the live file's when that is
    /// newer; and whether it is the live file's.
    /// </summary>
    public static (DateTimeOffset? At, bool Live) ReadAt(DatabaseManifest manifest, DatabaseLive? live)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        return live?.Generated is { } generated && (manifest.ReadAt is not { } readAt || generated > readAt)
            ? (generated, true)
            : (manifest.ReadAt, false);
    }

    /// <summary>The line under the heading: <c>Schema read 2 h ago (at the environment's last deployment)</c>.</summary>
    public static string? ReadLine(DatabaseManifest manifest, DatabaseLive? live, DateTimeOffset now) =>
        ReadAt(manifest, live) switch
        {
            ({ } at, true) => $"Schema read {TimeText.Ago(at, now)}",
            ({ } at, false) => $"Schema read {TimeText.Ago(at, now)} (at the environment's last deployment)",
            _ => null,
        };

    /// <summary>The line of a diagram that draws only the first tables; null for one that draws all.</summary>
    public static string? TruncatedLine(DatabaseManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        return manifest.Truncated
            ? string.Create(CultureInfo.InvariantCulture, $"The diagram draws the first {manifest.Drawn} tables.")
            : null;
    }

    /// <summary>"Tables as a list": the live file's tables when it is there, else the diagram's.</summary>
    public static IReadOnlyList<DatabaseListRow> List(DatabaseManifest manifest, DatabaseLive? live)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        return live is not null
            ? [.. live.Tables.Select(table => new DatabaseListRow(QualifiedTable(table.Schema, table.Name), table.Columns.Count, table.Rows))]
            : [.. manifest.Tables.Select(table => new DatabaseListRow(QualifiedTable(table.Schema, table.Name), table.Columns, table.Rows))];
    }

    /// <summary>A column's key in words: <c>primary key</c>, <c>foreign key</c>, both, or nothing.</summary>
    public static string Key(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        var parts = key.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return string.Join(", ", parts.Select(part => part switch
        {
            "pk" => "primary key",
            "fk" => "foreign key",
            _ => part,
        }));
    }

    /// <summary>Whether a column takes null, in a word; <see cref="NotSaid" /> where the live file does not say.</summary>
    public static string Nullable(bool? nullable) => nullable switch
    {
        true => "yes",
        false => "no",
        null => NotSaid,
    };

    /// <summary>A count for the list: <c>1,234</c>; <see cref="NotSaid" /> for one nobody counted.</summary>
    public static string Number(long? number) =>
        number is { } value ? value.ToString(CountFormat, CultureInfo.InvariantCulture) : NotSaid;

    private static Dictionary<string, DatabaseLiveTable> LiveByAlias(DatabaseLive? live)
    {
        var tables = new Dictionary<string, DatabaseLiveTable>(StringComparer.Ordinal);
        foreach (var table in live?.Tables ?? [])
        {
            tables.TryAdd(Alias(table.Schema, table.Name), table);
        }

        return tables;
    }
}

/// <summary>
/// When the live file is read again: with a check round of the page, so only while the page is neither paused nor
/// hidden (and only while the database view is shown: nothing else asks), and never more often than once a minute.
/// </summary>
public static class DatabaseRefresh
{
    public static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    /// <summary>The page's own gate and the one interval rule (<see cref="Health.IntervalDue" />) with the view's <see cref="Interval"/>, on the stamp the read's home (<see cref="Health.IntervalRead{T}"/>) keeps.</summary>
    /// <param name="active">The page checks: it is neither paused nor hidden.</param>
    /// <param name="lastRead">When the live file was last asked for; null when it never was.</param>
    public static bool IsDue(bool active, DateTimeOffset? lastRead, DateTimeOffset now) =>
        active && IntervalDue.IsDue(lastRead, now, Interval);
}
