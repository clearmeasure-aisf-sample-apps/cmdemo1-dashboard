using System.Text.Json;
using Dashboard.Health;

namespace Dashboard.Runtime;

/// <summary>
/// The outcome of reading a file of the app: the value, or the reasons there is none. The one value-or-problems record
/// of the file readers (#1811): the readers of <c>runtime/</c> hand it back as it is, and the reader of
/// <c>topology.json</c> makes its <see cref="TopologyParseResult"/> through it. The cluster's
/// <see cref="Cluster.Parsed{T}"/> stays apart: it carries one problem, the end of a sentence, not a list.
/// </summary>
public sealed record RuntimeParseResult<T>(T? Value, IReadOnlyList<string> Errors)
    where T : class
{
    public bool IsValid => Value is not null;

    /// <summary>
    /// The one way a reader of <c>runtime/</c> says "there is no value, and here is why": no value, only the
    /// problems. The readers of the index, of a manifest and of a database view all say it through here, as
    /// <see cref="Health.TopologyParseResult.Failed"/> is the one way of the reader of <c>topology.json</c>. Kept to
    /// the folder, like the opener <see cref="RuntimeManifestParser.TryOpen"/> the readers share: a caller of one of
    /// them reads the result it handed back, it does not make one.
    /// </summary>
    /// <param name="problems">Why there is no value, in the order the reader found them.</param>
    internal static RuntimeParseResult<T> Failed(params string[] problems) => new(null, problems);

    /// <summary>
    /// The one way a reader of <c>runtime/</c> hands over the problems it gathered while reading: any problem means no
    /// value, and the problems are handed over as they are - the same words, their number and the order the reader
    /// found them; no problem means the value. The value is made only when there is no problem, so a refused file
    /// never builds one. <see cref="Health.TopologyParseResult.Collected"/> is the same way for <c>topology.json</c>.
    /// </summary>
    /// <param name="problems">What the reader found wrong, in the order it found it.</param>
    /// <param name="value">The value the reader read, asked for only when <paramref name="problems"/> is empty.</param>
    internal static RuntimeParseResult<T> Collected(IReadOnlyList<string> problems, Func<T?> value) =>
        problems.Count == 0 && value() is { } read ? new(read, []) : new(null, problems);
}

/// <summary>
/// Reads <c>runtime/index.json</c> and <c>runtime/&lt;env&gt;.json</c>. The deployment writes both next to the SVG it
/// rendered (<c>deploy-staticwebapp.ps1</c>). Required: the index's environments with their name, manifest and SVG
/// (an entry's <c>database</c> is optional: kept when it names two files of <c>runtime/</c>, else left out);
/// the manifest's nodes with alias and kind, the regions with alias, the relationships with id. Unknown fields and
/// kinds are kept apart, not refused: a newer diagram still shows. Optional: the frames (with alias), and the
/// <c>links</c> of a node, a region or a frame, of which only absolute https addresses are kept; a node's
/// <c>healthPath</c> (a plain path, else left out), its <c>ids</c> (the strings of the list) and, of a web app or a
/// container app, its <c>architecture</c> (an absolute https address, else left out); and the manifest's
/// <c>unplaced</c>, the resources in no environment: an entry with a name is kept, anything else of the member is
/// left out and never an error.
/// </summary>
public static class RuntimeManifestParser
{
    private static readonly JsonDocumentOptions Options = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip,
    };

    public static RuntimeParseResult<RuntimeIndex> ParseIndex(string? json)
    {
        if (!TryOpen(json, out var document, out var problem))
        {
            return RuntimeParseResult<RuntimeIndex>.Failed(problem);
        }

        using (document)
        {
            var root = document.RootElement;
            if (!root.TryGetProperty(Environments, out var array) || array.ValueKind != JsonValueKind.Array)
            {
                return RuntimeParseResult<RuntimeIndex>.Failed($"{Environments}: missing or not an array.");
            }

            var errors = new List<string>();
            var entries = new List<RuntimeIndexEntry>();
            var index = 0;
            foreach (var element in array.EnumerateArray())
            {
                var path = $"{Environments}[{index++}]";
                var name = element.ValueKind == JsonValueKind.Object ? JsonRead.Text(element, Name) : null;
                if (name is null)
                {
                    errors.Add(FileProblems.MissingOrEmpty($"{path}.{Name}"));
                    continue;
                }

                var manifest = RelativeFile(element, ManifestWord, $"{name}.json", path, errors);
                var svg = RelativeFile(element, SvgWord, $"{name}.svg", path, errors);
                entries.Add(new RuntimeIndexEntry(name, manifest, svg, Database(element)));
            }

            return RuntimeParseResult<RuntimeIndex>.Collected(errors, () => new RuntimeIndex(entries, JsonRead.Text(root, "plantuml")));
        }
    }

    public static RuntimeParseResult<RuntimeManifest> ParseManifest(string? json)
    {
        if (!TryOpen(json, out var document, out var problem))
        {
            return RuntimeParseResult<RuntimeManifest>.Failed(problem);
        }

        using (document)
        {
            var root = document.RootElement;
            var errors = new List<string>();
            var environment = JsonRead.Text(root, EnvironmentWord);
            if (environment is null)
            {
                errors.Add(FileProblems.MissingOrEmpty(EnvironmentWord));
            }

            var nodes = ReadArray(root, "nodes", errors, (element, path) =>
            {
                if (RequiredWord(element, path, Alias, errors) is not { } alias)
                {
                    return null;
                }

                var url = JsonRead.Address(element, Url);
                if (url is null && JsonRead.Text(element, Url) is not null)
                {
                    errors.Add($"{path}.{Url}: not an absolute http or https address.");
                }

                var kind = NodeKind(JsonRead.Text(element, Kind));
                return new RuntimeNode(
                    alias,
                    kind,
                    TextOr(element, Name, alias),
                    url,
                    JsonRead.Text(element, "deployable"),
                    JsonRead.Text(element, "role")?.ToLowerInvariant(),
                    JsonRead.Text(element, "region"),
                    JsonRead.Text(element, "regionAlias"),
                    JsonRead.Text(element, "healthCheck"),
                    JsonRead.Text(element, "dependencyKind"),
                    Links(element),
                    HealthPath(element),
                    Ids(element),
                    Architecture(element, kind));
            });
            var regions = ReadArray(root, "regions", errors, (element, path) =>
            {
                if (RequiredWord(element, path, Alias, errors) is not { } alias)
                {
                    return null;
                }

                var roles = element.TryGetProperty("roles", out var array) && array.ValueKind == JsonValueKind.Array
                    ? array.EnumerateArray().Where(role => role.ValueKind == JsonValueKind.String).Select(role => role.GetString()!).ToList()
                    : [];
                return new RuntimeRegion(alias, TextOr(element, Name, alias), roles, Links(element));
            });
            var frames = ReadArray(root, "frames", errors, (element, path) =>
            {
                if (RequiredWord(element, path, Alias, errors) is not { } alias)
                {
                    return null;
                }

                return new RuntimeFrame(alias, TextOr(element, Kind, string.Empty), TextOr(element, Name, alias), Links(element));
            });
            var edges = ReadArray(root, "edges", errors, (element, path) =>
            {
                if (RequiredWords(element, $"{path}: id, from and to are required.", errors, "id", "from", "to") is not [var id, var from, var to])
                {
                    return null;
                }

                int? priority = element.TryGetProperty("priority", out var number) && number.TryGetInt32(out var value) ? value : null;
                return new RuntimeEdge(id, from, to, EdgeKind(JsonRead.Text(element, Kind)), priority);
            });

            return RuntimeParseResult<RuntimeManifest>.Collected(
                errors,
                () => environment is null ? null : new RuntimeManifest(environment, nodes, regions, edges, frames, Unplaced(root)));
        }
    }

    /// <summary>
    /// The one opener of the files of <c>runtime/</c>, shared by every reader of the folder (this parser, the database
    /// view's two readers and the application view's) and by the reader of <c>topology.json</c>
    /// (<see cref="Dashboard.Health.TopologyParser.Parse"/>), so the app opens a file it reads in one way: an empty or blank file, a file that is no JSON and a file whose
    /// root is no object are each refused here, in these words and nowhere else, and a document that is refused is
    /// disposed here. The options it parses with (trailing commas and comments) are the folder's, so every reader of
    /// the folder reads the same file the same way.
    /// </summary>
    /// <param name="json">The file's text, or null for a caller that read nothing.</param>
    /// <param name="document">The opened document when the answer is true; null when it is false.</param>
    /// <param name="problem">Why the file is refused when the answer is false; empty when it is true.</param>
    internal static bool TryOpen(string? json, out JsonDocument document, out string problem)
    {
        document = null!;
        problem = string.Empty;
        if (string.IsNullOrWhiteSpace(json))
        {
            problem = "The file is empty.";
            return false;
        }

        try
        {
            document = JsonDocument.Parse(json, Options);
        }
        catch (JsonException exception)
        {
            problem = $"The file is not valid JSON (line {exception.LineNumber + 1}, position {exception.BytePositionInLine + 1}).";
            return false;
        }

        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            document.Dispose();
            problem = "The file must contain a JSON object.";
            return false;
        }

        return true;
    }

    private static List<T> ReadArray<T>(JsonElement root, string property, List<string> errors, Func<JsonElement, string, T?> read)
        where T : class
    {
        var items = new List<T>();
        if (!root.TryGetProperty(property, out var array) || array.ValueKind == JsonValueKind.Null)
        {
            return items;
        }

        if (array.ValueKind != JsonValueKind.Array)
        {
            errors.Add($"{property}: not an array.");
            return items;
        }

        var index = 0;
        foreach (var element in array.EnumerateArray())
        {
            var path = $"{property}[{index++}]";
            if (element.ValueKind != JsonValueKind.Object)
            {
                errors.Add($"{path}: not an object.");
                continue;
            }

            if (read(element, path) is { } item)
            {
                items.Add(item);
            }
        }

        return items;
    }

    /// <summary>
    /// The one required word of an element of a manifest's list: its text, or null with the refusal
    /// <see cref="FileProblems.MissingOrEmpty"/> added for the place it was asked at (<c>{path}.{word}</c>).
    /// </summary>
    private static string? RequiredWord(JsonElement element, string path, string word, List<string> errors) =>
        RequiredWords(element, FileProblems.MissingOrEmpty($"{path}.{word}"), errors, word) is [var text] ? text : null;

    /// <summary>
    /// The required words of an element of a manifest's list, in the order named: all their texts, or null with
    /// <paramref name="refusal"/> added once when any of them is missing or empty.
    /// </summary>
    private static string[]? RequiredWords(JsonElement element, string refusal, List<string> errors, params string[] words)
    {
        var texts = words.Select(word => JsonRead.Text(element, word)).ToArray();
        if (texts.Any(text => text is null))
        {
            errors.Add(refusal);
            return null;
        }

        return texts!;
    }

    /// <summary>
    /// The one "a word the deployment left out" rule of a manifest element: the element's word, or the fallback the
    /// element stands for without it. A node, a region and a frame are called by their alias without a <c>name</c>
    /// (the box is drawn by the alias, so a manifest that names nothing is named all the same), and a frame without a
    /// <c>kind</c> holds the empty word (a frame without a kind is still a frame, linked with its name alone). The
    /// record cannot state the fallback: <c>RuntimeFrame</c>'s <c>Name</c> is non-nullable, so it lives here.
    /// </summary>
    private static string TextOr(JsonElement element, string property, string fallback) =>
        JsonRead.Text(element, property) ?? fallback;

    /// <summary>A file next to the index: a plain name, never an address or a path that leaves <c>runtime/</c>.</summary>
    private static string RelativeFile(JsonElement element, string property, string fallback, string path, List<string> errors)
    {
        var file = JsonRead.Text(element, property) ?? fallback;
        if (!IsFileName(file))
        {
            errors.Add($"{path}.{property}: not a file name in runtime/.");
        }

        return file;
    }

    /// <summary>
    /// The files of an environment's database view: both plain names in <c>runtime/</c>. Anything else is left out,
    /// never an error: the runtime diagram shows all the same, and the database's box behaves as without the files.
    /// </summary>
    private static RuntimeDatabaseFiles? Database(JsonElement entry) =>
        JsonRead.Section(entry, DatabaseWord) is { } section
            && JsonRead.Text(section, SvgWord) is { } svg && IsFileName(svg)
            && JsonRead.Text(section, ManifestWord) is { } manifest && IsFileName(manifest)
            ? new RuntimeDatabaseFiles(svg, manifest)
            : null;

    private static bool IsFileName(string file) =>
        !(file.Contains('/', StringComparison.Ordinal) || file.Contains('\\', StringComparison.Ordinal) || file.Contains(':', StringComparison.Ordinal) || file.StartsWith('.'));

    /// <summary>
    /// The <c>links</c> of an element: a map of a key to an address. Only an absolute https address is kept (a link
    /// is opened by a click, and nothing else belongs behind one); anything else is left out, never an error.
    /// </summary>
    private static Health.LinkSet? Links(JsonElement parent)
    {
        if (JsonRead.Section(parent, LinksWord) is not { } links)
        {
            return null;
        }

        var found = new Dictionary<string, Uri>(StringComparer.Ordinal);
        foreach (var property in links.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.String
                && Uri.TryCreate(property.Value.GetString()?.Trim(), UriKind.Absolute, out var address)
                && address.Scheme == Uri.UriSchemeHttps)
            {
                found[property.Name] = address;
            }
        }

        return found.Count == 0 ? null : new Health.LinkSet(found);
    }

    /// <summary>The element words the manifest and the index write, named once: a rename here moves every read and every
    /// refusal with it, and nothing can drift from the file.</summary>
    private const string Alias = "alias";
    private const string Name = "name";
    private const string Kind = "kind";
    private const string Url = "url";
    private const string Environments = "environments";
    private const string LinksWord = "links";
    private const string EnvironmentWord = "environment";
    private const string ManifestWord = "manifest";
    private const string SvgWord = "svg";
    private const string DatabaseWord = "database";

    /// <summary>The longest text kept of an entry of <c>unplaced</c>, and the most entries.</summary>
    public const int LongestUnplacedText = 300;
    public const int MostUnplaced = 500;
    private const int LongestUnplacedId = LongestUnplacedText * 2;
    private const int LongestAddress = 2000;

    /// <summary>
    /// The manifest's <c>unplaced</c>: the objects of the list that have a name, the first <see cref="MostUnplaced"/>.
    /// A text is cut at <see cref="LongestUnplacedText"/> characters; <c>links.portal</c>, <c>links.site</c> and
    /// <c>url</c> are kept only as absolute https addresses. Null without the member (a manifest from before it), and
    /// where it is no list: nothing here is an error, the diagram shows all the same.
    /// </summary>
    private static List<RuntimeUnplaced>? Unplaced(JsonElement root)
    {
        if (!root.TryGetProperty("unplaced", out var array) || array.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var found = new List<RuntimeUnplaced>();
        foreach (var element in array.EnumerateArray())
        {
            if (found.Count == MostUnplaced)
            {
                break;
            }

            if (element.ValueKind != JsonValueKind.Object || Cut(element, Name) is not { } name)
            {
                continue;
            }

            var links = JsonRead.Section(element, LinksWord);
            found.Add(new RuntimeUnplaced(
                name,
                Cut(element, "type"),
                Cut(element, "group"),
                Cut(element, "words"),
                Ids(element)?.Select(id => Clip.At(id, LongestUnplacedId)).ToList(),
                links is { } portal ? Secure(portal, "portal") : null,
                (links is { } site ? Secure(site, "site") : null) ?? Secure(element, Url)));
        }

        return found;
    }

    /// <summary>A text of an entry of <c>unplaced</c>, cut at <see cref="LongestUnplacedText"/> characters.</summary>
    private static string? Cut(JsonElement parent, string property) =>
        JsonRead.Text(parent, property) is { } text ? Clip.At(text, LongestUnplacedText) : null;

    /// <summary>An absolute https address without a user name, of a sensible length; anything else is null.</summary>
    private static Uri? Secure(JsonElement parent, string property) =>
        JsonRead.Text(parent, property) is { Length: <= LongestAddress } text
        && Uri.TryCreate(text, UriKind.Absolute, out var address)
        && address.Scheme == Uri.UriSchemeHttps
        && address.UserInfo.Length == 0
            ? address
            : null;

    /// <summary>The longest health path kept, and the most ids of one box.</summary>
    private const int LongestPath = 200;
    private const int MostIds = 200;

    /// <summary>
    /// A node's health path: a path of letters, digits and <c>/ . _ ~ -</c> that starts with a slash. Anything else is
    /// left out, never an error: the page then asks the app's root.
    /// </summary>
    private static string? HealthPath(JsonElement parent) =>
        JsonRead.Text(parent, "healthPath") is { Length: <= LongestPath } path && path[0] == '/' && !path.StartsWith("//", StringComparison.Ordinal)
            && path.All(character => char.IsAsciiLetterOrDigit(character) || "/._~-".Contains(character, StringComparison.Ordinal))
            ? path
            : null;

    /// <summary>
    /// Where the diagrams of a node's deployable are published: an absolute https address without a query, of a web
    /// app or a container app. The page builds the addresses it asks from it. Anything else is left out, never an
    /// error: the box then behaves as without one.
    /// </summary>
    private static Uri? Architecture(JsonElement parent, RuntimeNodeKind kind) =>
        kind is RuntimeNodeKind.WebApp or RuntimeNodeKind.ContainerApp
        && JsonRead.Text(parent, "architecture") is { Length: <= LongestPath * 2 } text
        && Uri.TryCreate(text, UriKind.Absolute, out var address)
        && address.Scheme == Uri.UriSchemeHttps
        && address.Query.Length == 0 && address.Fragment.Length == 0 && address.UserInfo.Length == 0
            ? address
            : null;

    /// <summary>A node's resource ids: the first <see cref="MostIds"/> of the strings of its list; null without one.</summary>
    private static List<string>? Ids(JsonElement parent) =>
        JsonRead.Strings(parent, "ids") is { } strings ? strings.Take(MostIds).ToList() : null;

    private static RuntimeNodeKind NodeKind(string? kind) => kind?.ToLowerInvariant() switch
    {
        "person" => RuntimeNodeKind.Person,
        "frontdoor" => RuntimeNodeKind.FrontDoor,
        "webapp" => RuntimeNodeKind.WebApp,
        "sql" => RuntimeNodeKind.Sql,
        "staticsite" => RuntimeNodeKind.StaticSite,
        "dependency" => RuntimeNodeKind.Dependency,
        "containerapp" => RuntimeNodeKind.ContainerApp,
        "resource" => RuntimeNodeKind.Resource,
        _ => RuntimeNodeKind.Other,
    };

    private static RuntimeEdgeKind EdgeKind(string? kind) => kind?.ToLowerInvariant() switch
    {
        "public" => RuntimeEdgeKind.Public,
        "origin" => RuntimeEdgeKind.Origin,
        "sql" => RuntimeEdgeKind.Sql,
        "dashboard" => RuntimeEdgeKind.Dashboard,
        "dependency" => RuntimeEdgeKind.Dependency,
        _ => RuntimeEdgeKind.Other,
    };
}
