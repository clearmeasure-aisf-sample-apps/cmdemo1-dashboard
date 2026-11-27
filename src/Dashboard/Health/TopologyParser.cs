using System.Text.Json;
using Dashboard.Runtime;

namespace Dashboard.Health;

/// <summary>
/// The outcome of reading <c>topology.json</c>: a topology, or the reasons there is none. The health page's name for
/// the app's one value-or-problems record, <see cref="RuntimeParseResult{T}"/> of a <see cref="Health.Topology"/>
/// (#1811): both ways of making one go through that record, so the rules of "no value, and here is why" are written
/// once for every file reader.
/// </summary>
public sealed record TopologyParseResult(Topology? Topology, IReadOnlyList<string> Errors)
{
    public bool IsValid => Topology is not null;

    /// <summary>No topology, the problems as they were given: <see cref="RuntimeParseResult{T}.Failed"/>.</summary>
    public static TopologyParseResult Failed(params string[] errors) => From(RuntimeParseResult<Topology>.Failed(errors));

    /// <summary>
    /// The one way the reader of <c>topology.json</c> hands over the problems it gathered while reading:
    /// <see cref="RuntimeParseResult{T}.Collected"/>, the same way as for <c>runtime/</c>. Any problem means no
    /// topology and the problems as they are, in their order; no problem means the topology, made only then.
    /// </summary>
    internal static TopologyParseResult Collected(IReadOnlyList<string> errors, Func<Topology> topology) =>
        From(RuntimeParseResult<Topology>.Collected(errors, topology));

    /// <summary>The shared record, under the name and the parts the health page reads.</summary>
    internal static TopologyParseResult From(RuntimeParseResult<Topology> result) => new(result.Value, result.Errors);
}

/// <summary>
/// Reads <c>topology.json</c>. Required: <c>environments</c> (an array), each environment's <c>name</c> and each
/// node's <c>url</c>. Everything else is optional and gets a default; unknown fields are ignored. An address that is
/// present must be an absolute http or https address: the dashboard calls it or links to it.
/// </summary>
public static class TopologyParser
{
    public static TopologyParseResult Parse(string? json)
    {
        // The app's one opener (#1784): the same three refusals, in the same words, and the same options as runtime/.
        if (!RuntimeManifestParser.TryOpen(json, out var document, out var problem))
        {
            return TopologyParseResult.Failed(problem);
        }

        using (document)
        {
            var root = document.RootElement;
            var errors = new List<string>();
            var system = ReadSystem(root, errors);
            var generated = JsonRead.Time(root, "generated");
            var environments = ReadEnvironments(root, errors);
            var cluster = ReadCluster(root, errors);
            return TopologyParseResult.Collected(errors, () => new Topology(system, generated, environments, cluster));
        }
    }

    private static SystemInfo ReadSystem(JsonElement root, List<string> errors)
    {
        const string Path = "system";
        var slug = string.Empty;
        string? name = null;
        Uri? repository = null;
        Uri? delivery = null;
        Uri? cost = null;
        Uri? deployments = null;
        DashboardInfo? dashboard = null;
        if (root.TryGetProperty(Path, out var system) && system.ValueKind == JsonValueKind.Object)
        {
            slug = JsonRead.Text(system, "slug") ?? string.Empty;
            name = JsonRead.Text(system, "name");
            repository = ReadOptionalAddress(system, "repository", Path, errors);
            delivery = ReadOptionalAddress(system, "deliveryUrl", Path, errors);
            cost = ReadOptionalAddress(system, "costUrl", Path, errors);
            deployments = ReadOptionalAddress(system, "deploymentsUrl", Path, errors);
            dashboard = ReadDashboard(system);
        }

        return new SystemInfo(
            Slug: slug,
            Name: name ?? (slug.Length > 0 ? slug : SystemInfo.DefaultName),
            Repository: repository,
            DeliveryUrl: delivery,
            CostUrl: cost,
            Dashboard: dashboard,
            DeploymentsUrl: deployments);
    }

    /// <summary>
    /// The optional <c>system.dashboard</c>: the dashboard itself, with the path of its own build facts. Null without
    /// a <c>buildPath</c>, and the page then reads nothing about itself. Like a <c>buildPath</c> of a deployable it is
    /// a courtesy: anything but an object with a path is "not there", never an error.
    /// </summary>
    private static DashboardInfo? ReadDashboard(JsonElement system)
    {
        if (!system.TryGetProperty("dashboard", out var dashboard) || dashboard.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return ReadOptionalPath(dashboard, "buildPath") is { } build
            ? new DashboardInfo(JsonRead.Text(dashboard, "name") ?? DashboardInfo.DefaultName, build)
            : null;
    }

    private static List<EnvironmentInfo> ReadEnvironments(JsonElement root, List<string> errors)
    {
        const string Section = "environments";
        var environments = new List<EnvironmentInfo>();
        if (!root.TryGetProperty(Section, out var array) || array.ValueKind != JsonValueKind.Array)
        {
            errors.Add($"{Section}: missing or not an array.");
            return environments;
        }

        foreach (var (element, path, _) in Objects(array, Section, errors))
        {
            var name = JsonRead.Text(element, "name");
            if (name is null)
            {
                errors.Add($"{path}.name: missing or empty.");
                continue;
            }

            var versions = ReadOptionalAddress(element, "versionsUrl", path, errors);
            var history = ReadOptionalAddress(element, "versionsHistoryUrl", path, errors);
            var deployables = ReadDeployables(element, path, errors);
            environments.Add(new EnvironmentInfo(
                name,
                JsonRead.Text(element, "tier"),
                deployables,
                versions,
                history,
                ReadLinks(element),
                JsonRead.Text(element, "namespace"),
                ReadPages(element)));
        }

        return environments;
    }

    /// <summary>
    /// The optional <c>pages</c> of an environment: every entry with a name and an absolute http or https address,
    /// in the order of the file. An entry without either is left out, like a link that is no address
    /// (<see cref="ReadLinks"/>): a link is a courtesy, never a reason to show no dashboard. Null without a single page.
    /// </summary>
    private static List<PageLink>? ReadPages(JsonElement parent)
    {
        const string Pages = "pages";
        if (!JsonRead.HasArray(parent, Pages))
        {
            return null;
        }

        var found = new List<PageLink>();
        foreach (var page in JsonRead.Items(parent, Pages))
        {
            if (page.ValueKind == JsonValueKind.Object
                && JsonRead.Text(page, "name") is { } name
                && JsonRead.Address(page, "url") is { } address)
            {
                found.Add(new PageLink(name, address, JsonRead.Text(page, "deployable")));
            }
        }

        return found.Count == 0 ? null : found;
    }

    /// <summary>
    /// The optional <c>cluster</c>: null when it is absent or <c>null</c>, and the page then has no cluster view. Its
    /// addresses are held to the rule of every address the page calls or links to; so are the links it knows
    /// (<see cref="LinkSet.Portal"/>, <see cref="LinkSet.Workloads"/>), and any other key of <c>links</c> is ignored.
    /// </summary>
    private static ClusterInfo? ReadCluster(JsonElement root, List<string> errors)
    {
        const string Path = "cluster";
        if (!root.TryGetProperty(Path, out var cluster) || cluster.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (cluster.ValueKind != JsonValueKind.Object)
        {
            errors.Add($"{Path}: not an object.");
            return null;
        }

        var status = ReadOptionalAddress(cluster, "statusUrl", Path, errors);
        var service = ReadOptionalAddress(cluster, "serviceUrl", Path, errors);
        var found = new Dictionary<string, Uri>(StringComparer.Ordinal);
        if (JsonRead.Section(cluster, Links) is { } links)
        {
            foreach (var key in new[] { LinkSet.Portal, LinkSet.Workloads })
            {
                if (ReadOptionalAddress(links, key, $"{Path}.links", errors) is { } address)
                {
                    found[key] = address;
                }
            }
        }

        return new ClusterInfo(JsonRead.Text(cluster, "name"), status, service, found.Count == 0 ? null : new LinkSet(found));
    }

    private static List<DeployableInfo> ReadDeployables(JsonElement environment, string environmentPath, List<string> errors)
    {
        const string Section = "deployables";
        var deployables = new List<DeployableInfo>();
        if (!TryReadOptionalArray(environment, Section, environmentPath, errors, out var array))
        {
            return deployables;
        }

        foreach (var (element, path, _) in Objects(array, $"{environmentPath}.{Section}", errors))
        {
            var project = ReadOptionalAddress(element, "projectUrl", path, errors);
            var frontDoor = ReadOptionalAddress(element, "frontDoor", path, errors);
            var pin = ReadOptionalAddress(element, "pinUrl", path, errors);
            var pinHistory = ReadOptionalAddress(element, "pinHistoryUrl", path, errors);
            var nodes = ReadNodes(element, path, errors);
            deployables.Add(new DeployableInfo(
                Name: JsonRead.Text(element, "name") ?? "app",
                FrontDoor: frontDoor,
                HealthPath: ReadPath(element, "healthPath", HealthPaths.DefaultHealthPath),
                AlivePath: ReadPath(element, "alivePath", DeployableInfo.DefaultAlivePath),
                VersionPath: ReadPath(element, "versionPath", DeployableInfo.DefaultVersionPath),
                Nodes: nodes,
                ProjectUrl: project,
                TelemetryPath: ReadOptionalPath(element, "telemetryPath"),
                TrafficPaths: ReadPaths(element, "trafficPaths", path, errors),
                BuildPath: ReadOptionalPath(element, "buildPath"),
                Links: ReadLinks(element),
                PinUrl: pin,
                PinHistoryUrl: pinHistory,
                HealthDetailPath: ReadOptionalPath(element, "healthDetailPath")));
        }

        return deployables;
    }

    /// <summary>Null when the address is absent or <c>null</c> (fine); an error when it is anything but an address.</summary>
    private static Uri? ReadOptionalAddress(JsonElement parent, string property, string parentPath, List<string> errors)
    {
        if (!parent.TryGetProperty(property, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var address = JsonRead.Address(value);
        if (address is null)
        {
            errors.Add(Messages.Address(parentPath, property, required: false));
        }

        return address;
    }

    private static List<NodeInfo> ReadNodes(JsonElement deployable, string deployablePath, List<string> errors)
    {
        const string Section = "nodes";
        var nodes = new List<NodeInfo>();
        if (!TryReadOptionalArray(deployable, Section, deployablePath, errors, out var array))
        {
            return nodes;
        }

        foreach (var (element, path, _) in Objects(array, $"{deployablePath}.{Section}", errors))
        {
            var url = element.TryGetProperty("url", out var value) ? JsonRead.Address(value) : null;
            if (url is null)
            {
                errors.Add(Messages.Address(path, "url", required: true));
                continue;
            }

            // Without a role, the order of the nodes read decides: the first node read is the primary.
            var role = JsonRead.Text(element, "role")?.ToLowerInvariant() ?? NodeRole.Default(nodes.Count);
            nodes.Add(new NodeInfo(JsonRead.Text(element, "name") ?? url.Host, JsonRead.Text(element, "region"), role, url, ReadLinks(element)));
        }

        return nodes;
    }

    /// <summary>
    /// The optional <c>links</c> of an element: every entry whose value is an absolute http or https address. Anything
    /// else is left out, and the number it belongs to stays plain text: a link is a courtesy, never a reason to show
    /// no dashboard. Null without a single link.
    /// </summary>
    private static LinkSet? ReadLinks(JsonElement parent)
    {
        if (JsonRead.Section(parent, Links) is not { } links)
        {
            return null;
        }

        var found = new Dictionary<string, Uri>(StringComparer.Ordinal);
        foreach (var property in links.EnumerateObject())
        {
            if (JsonRead.Address(property.Value) is { } address)
            {
                found[property.Name] = address;
            }
        }

        return found.Count == 0 ? null : new LinkSet(found);
    }

    /// <summary>
    /// The objects of an array, each with the place it stands in (<c>{arrayPath}[{index}]</c>) and its index among all
    /// the entries; an entry that is no object is refused by that place, once, and not handed on.
    /// </summary>
    private static IEnumerable<(JsonElement Element, string Path, int Index)> Objects(
        JsonElement array,
        string arrayPath,
        List<string> errors)
    {
        var index = 0;
        foreach (var element in array.EnumerateArray())
        {
            var position = index++;
            var path = $"{arrayPath}[{position}]";
            if (element.ValueKind != JsonValueKind.Object)
            {
                errors.Add($"{path}: not an object.");
                continue;
            }

            yield return (element, path, position);
        }
    }

    /// <summary>The element word the links of a cluster, an environment, a deployable or a node live under.</summary>
    private const string Links = "links";

    /// <summary>The sentences the page says a refused part of the file in; the tests pin them letter for letter.</summary>
    private static class Messages
    {
        /// <summary>
        /// An address that is not one: a required one may also be missing, an optional one that is missing is fine.
        /// </summary>
        internal static string Address(string parentPath, string property, bool required) => required
            ? $"{parentPath}.{property}: missing or not an absolute http or https address."
            : $"{parentPath}.{property}: not an absolute http or https address.";
    }

    /// <summary>False when the array is absent (fine: it is empty) or of the wrong type (an error).</summary>
    private static bool TryReadOptionalArray(
        JsonElement parent,
        string property,
        string parentPath,
        List<string> errors,
        out JsonElement array)
    {
        if (!parent.TryGetProperty(property, out array) || array.ValueKind == JsonValueKind.Null)
        {
            return false;
        }

        if (array.ValueKind != JsonValueKind.Array)
        {
            errors.Add($"{parentPath}.{property}: not an array.");
            return false;
        }

        return true;
    }

    private static string ReadPath(JsonElement parent, string property, string fallback) =>
        ReadOptionalPath(parent, property) ?? fallback;

    /// <summary>An optional path, or none: <see cref="JsonRead.Text"/> answers null for anything but a word.</summary>
    private static string? ReadOptionalPath(JsonElement parent, string property) =>
        SitePath.Parse(JsonRead.Text(parent, property));

    /// <summary>An optional array of paths; null when absent, an error for anything but strings.</summary>
    private static List<string>? ReadPaths(JsonElement parent, string property, string parentPath, List<string> errors)
    {
        if (!TryReadOptionalArray(parent, property, parentPath, errors, out var array))
        {
            return null;
        }

        var paths = new List<string>();
        foreach (var element in array.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(element.GetString()))
            {
                errors.Add($"{parentPath}.{property}: every entry must be a path.");
                return null;
            }

            // Deliberately untrimmed (#1041): a padded entry stays the path it was written as.
            paths.Add(SitePath.WithFront(element.GetString()!));
        }

        return paths;
    }
}
