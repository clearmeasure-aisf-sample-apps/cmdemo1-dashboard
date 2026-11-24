namespace Dashboard.Health;

/// <summary>
/// The optional <c>links</c> of an environment, a deployable or a node in <c>topology.json</c>: where a number or a
/// name of the page leads, by key. The deployment writes what it knows; a key that is absent is no link, and the
/// number stays plain text.
/// </summary>
public sealed class LinkSet
{
    /// <summary>A node: the web app in the Azure portal. The cluster: the AKS cluster in the Azure portal.</summary>
    public const string Portal = "portal";

    /// <summary>The cluster: its workloads in the Azure portal.</summary>
    public const string Workloads = "workloads";

    /// <summary>A node: Live Metrics of the environment's Application Insights.</summary>
    public const string LiveMetrics = "liveMetrics";

    /// <summary>A node: the Performance blade (requests).</summary>
    public const string Performance = "performance";

    /// <summary>A node: the Failures blade.</summary>
    public const string Failures = "failures";

    /// <summary>A node: the dependency calls (SQL) of the app's role, as a Logs query.</summary>
    public const string Dependencies = "dependencies";

    /// <summary>A deployable: the Front Door profile in the Azure portal.</summary>
    public const string FrontDoor = "frontDoor";

    /// <summary>A deployable: the requests of the app's role, as a Logs query.</summary>
    public const string Logs = "logs";

    /// <summary>An environment: its Application Insights.</summary>
    public const string ApplicationInsights = "applicationInsights";

    /// <summary>An environment: the application map of its Application Insights.</summary>
    public const string ApplicationMap = "applicationMap";

    /// <summary>An environment: its database in the Azure portal.</summary>
    public const string Database = "database";

    /// <summary>An environment: its resource group in the Azure portal.</summary>
    public const string ResourceGroup = "resourceGroup";

    private readonly Dictionary<string, Uri> _links;

    public LinkSet(IEnumerable<KeyValuePair<string, Uri>> links)
    {
        ArgumentNullException.ThrowIfNull(links);
        _links = new Dictionary<string, Uri>(links, StringComparer.Ordinal);
    }

    public static LinkSet None { get; } = new([]);

    public int Count => _links.Count;

    public IEnumerable<string> Keys => _links.Keys;

    /// <summary>The address of a key; null when the topology has none.</summary>
    public Uri? this[string key] => _links.GetValueOrDefault(key);
}

/// <summary>
/// The words of a link's tooltip: where it goes, that it opens a new tab, and that the destination asks for a sign-in
/// (the dashboard itself holds no credential and calls none of these places).
/// </summary>
public static class LinkText
{
    public const string PortalSuffix = " (opens in a new tab; the Azure portal asks you to sign in)";
    public const string OctopusSuffix = " (opens in a new tab; Octopus Deploy asks you to sign in)";
    public const string GitHubSuffix = " (opens in a new tab)";

    public static string Portal(string what) => what + PortalSuffix;

    /// <summary>
    /// The one table of a key of <see cref="LinkSet"/> to the sentence its tooltip says of a subject. A new link is one
    /// row here; a key with no row - <see cref="LinkSet.Workloads"/> among them - is said from the key by
    /// <see cref="SaidFromTheKey"/>.
    /// </summary>
    private static readonly Dictionary<string, Func<string, string>> Sentences =
        new Dictionary<string, Func<string, string>>(StringComparer.Ordinal)
        {
            [LinkSet.Portal] = subject => $"The web app {subject} in the Azure portal",
            [LinkSet.LiveMetrics] = subject => $"Live Metrics of Application Insights for {subject}: requests, failures and servers as they happen",
            [LinkSet.Performance] = subject => $"Performance in Application Insights for {subject}: requests and their duration",
            [LinkSet.Failures] = subject => $"Failures in Application Insights for {subject}: failed requests and exceptions",
            [LinkSet.Dependencies] = subject => $"Dependency calls of {subject} (SQL and HTTP) of the last hour, a query in Application Insights Logs",
            [LinkSet.FrontDoor] = subject => $"The Front Door profile of {subject} in the Azure portal",
            [LinkSet.Logs] = subject => $"Requests of {subject} of the last hour, a query in Application Insights Logs",
            [LinkSet.ApplicationInsights] = subject => $"Application Insights of {subject} in the Azure portal",
            [LinkSet.ApplicationMap] = subject => $"The application map of {subject} in Application Insights",
            [LinkSet.Database] = subject => $"The database of {subject} in the Azure portal",
            [LinkSet.ResourceGroup] = subject => $"The resource group of {subject} in the Azure portal",
        };

    /// <summary>The keys the table has a sentence for, matched by their exact spelling.</summary>
    public static IReadOnlyCollection<string> KeysWithASentence => Sentences.Keys;

    /// <summary>The tooltip of a link of <see cref="LinkSet"/> by its key, for the named resource.</summary>
    public static string For(string key, string subject) =>
        Portal(key is not null && Sentences.TryGetValue(key, out var sentence) ? sentence(subject) : SaidFromTheKey(key, subject));

    /// <summary>
    /// The deliberate words of a key the table has no row for: the key as it was written, then the subject. A key the
    /// topology invented, a key in the wrong case, or no key at all is answered rather than refused.
    /// </summary>
    private static string SaidFromTheKey(string? key, string subject) => $"{key} of {subject}";

    /// <summary>
    /// The page of one release in Octopus Deploy, from the project's page and the version a node runs:
    /// <c>&lt;projectUrl&gt;/deployments/releases/&lt;version&gt;</c>. Null without either.
    /// <para>
    /// A project address that is relative to a page is refused by the address itself, naming no argument, and that is
    /// the rule for every place a <em>link</em> is built (#1556): <see cref="Uri.AbsoluteUri"/> answers with its own
    /// <see cref="InvalidOperationException"/>, <c>This operation is not supported for a relative URI.</c> A link is
    /// the address the deployment wrote and nothing else - <see cref="LinkText"/> holds no <see cref="UriKind"/> of
    /// its own and no page of its own to resolve against, so it has no better word for the address than the address
    /// has, and guessing a release under a host nobody named is the one thing a link on this page must never do. The
    /// other half of the decision is a <em>probe</em>: there the caller owns the base it requests at, which is why
    /// #1544 would have <see cref="ProbeUrl.Combine"/> name <c>baseAddress</c>. Both builders of a link say this one
    /// sentence, and <see cref="Dashboard.Runtime.RuntimeLink.To"/> says it for the diagram.
    /// </para>
    /// </summary>
    /// <param name="projectUrl">The project's page in Octopus Deploy; null when the topology names none, and a
    /// <em>relative</em> one is refused by <see cref="Uri"/> itself rather than by a guess of this class.</param>
    /// <param name="version">The version a node runs; null, empty or nothing but spaces is no version.</param>
    public static Uri? Release(Uri? projectUrl, string? version) =>
        projectUrl is null || string.IsNullOrWhiteSpace(version)
            ? null
            : new Uri($"{projectUrl.AbsoluteUri.TrimEnd('/')}/deployments/releases/{Uri.EscapeDataString(version)}");

    public static string ReleaseTitle(string deployable, string version) =>
        $"Release {version} of {deployable} in Octopus Deploy{OctopusSuffix}";

    public static string CommitTitle(string commit) => $"Commit {BuildText.ShortCommit(commit)} on GitHub{GitHubSuffix}";
}
