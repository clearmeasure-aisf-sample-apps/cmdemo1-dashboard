namespace Dashboard.Health;

/// <summary>
/// The version the deployment pinned in Git for one deployable of one environment, next to the versions its nodes
/// run. Versions are compared without build metadata (the part from <c>+</c>) and without regard to case. A node whose
/// version is not known (unreachable, not checked yet, no version reported) is left out of the comparison: it never
/// makes the versions differ.
/// </summary>
public sealed record VersionAssessment(
    VersionState State,
    string? Pinned,
    IReadOnlyList<NodeVersion> Matching,
    IReadOnlyList<NodeVersion> Differing,
    IReadOnlyList<NodeVersion> Unknown,
    string Headline,
    string? Detail)
{
    private const string InSyncWord = "In sync";
    private const string DiffersWord = "Differs";
    private const string NotComparedWord = "Not compared";
    private const string NoPinnedVersion = "No pinned version";
    private const string NodeSeparator = ", ";

    public bool Differs => State == VersionState.Differs;

    /// <summary>The whole line in words, for a tooltip and for tests.</summary>
    public string Text => Detail is null ? Headline : $"{Headline}. {Detail}";

    /// <param name="pinned">The last reading of the file that holds the deployable's pin.</param>
    /// <param name="deployable">The deployable's name: its key in the reading.</param>
    /// <param name="nodes">What the deployable's nodes run.</param>
    /// <param name="file">The name of that file, for the words; <c>versions.json</c> when not given.</param>
    public static VersionAssessment Assess(PinnedVersions pinned, string deployable, IEnumerable<NodeVersion> nodes, string? file = null)
    {
        ArgumentNullException.ThrowIfNull(pinned);
        ArgumentNullException.ThrowIfNull(deployable);
        ArgumentNullException.ThrowIfNull(nodes);
        file ??= PinnedVersions.FileName;

        switch (pinned.State)
        {
            case PinnedVersionsState.NotTracked:
            case PinnedVersionsState.Pending:
                return Without(VersionState.Pending, "Reading the pinned version", null);
            case PinnedVersionsState.Missing:
                return Without(
                    VersionState.NotDeployed,
                    NoPinnedVersion,
                    $"{file} was not found: nothing was deployed here yet, or the repository is not public.");
            case PinnedVersionsState.Unavailable:
                return Without(
                    VersionState.PinnedUnknown,
                    "Pinned version not known",
                    $"{file} could not be read: {pinned.Detail ?? "no reason given"}.");
        }

        if (VersionText.Display(pinned.Of(deployable)) is not { } version)
        {
            return Without(
                VersionState.NotDeployed,
                NoPinnedVersion,
                $"{file} has no entry for {deployable}: it was not deployed here yet.");
        }

        var tally = Tally(nodes, version);
        return new VersionAssessment(
            tally.State,
            version,
            tally.Matching,
            tally.Differing,
            tally.Unknown,
            $"Pinned {version}",
            Describe(tally, version));
    }

    private static VersionAssessment Without(VersionState state, string headline, string? detail) =>
        new(state, null, [], [], [], headline, detail);

    private static bool Same(string? running, string pinned) =>
        string.Equals(running, pinned, StringComparison.OrdinalIgnoreCase);

    /// <summary>Walks the nodes once, reads each node's running version once and puts the node in its list.</summary>
    private static NodeTally Tally(IEnumerable<NodeVersion> nodes, string pinned)
    {
        var tally = new NodeTally([], [], []);
        foreach (var node in nodes)
        {
            var list = node.Running switch
            {
                null => tally.Unknown,
                var running when Same(running, pinned) => tally.Matching,
                _ => tally.Differing,
            };
            list.Add(node);
        }

        return tally;
    }

    /// <summary>The node clause a single node says: its label and the version it runs.</summary>
    private static string Runs(string label, string version) => $"{label} runs {version}";

    /// <summary>Joins a list of node clauses with the separator a sentence of nodes uses.</summary>
    private static string Nodes(IEnumerable<string> clauses) => string.Join(NodeSeparator, clauses);

    private static string Describe(NodeTally tally, string version)
    {
        var (matching, differing, unknown) = tally;
        var (total, state) = (tally.Total, tally.State);
        var known = state switch
        {
            // A node in Differing always has a non-null Running: Tally only puts a node there when Running is non-null.
            VersionState.Differs => $"{DiffersWord}: {Nodes(differing.Select(node => Runs(node.Label, node.Running!)))}",
            VersionState.InSync when matching.Count == 1 => $"{InSyncWord}: {Runs(matching[0].Label, version)}",
            VersionState.InSync when matching.Count == total => $"{InSyncWord}: all {total} nodes run {version}",
            VersionState.InSync => $"{InSyncWord}: {matching.Count} of {total} nodes run {version}",
            _ when total == 0 => $"{NotComparedWord}: the deployable has no nodes",
            _ => NotComparedWord,
        };
        if (unknown.Count == 0)
        {
            return $"{known}.";
        }

        var separator = state == VersionState.NodesUnknown ? ": " : "; ";
        return $"{known}{separator}{Nodes(unknown.Select(node => $"{node.Label} {node.UnknownReason}"))}.";
    }

    /// <summary>
    /// The nodes sorted, in one walk, by what each runs next to the pinned version: every node stands in exactly one
    /// list, in the order it was handed, and the total is the three lists together.
    /// </summary>
    private sealed record NodeTally(List<NodeVersion> Matching, List<NodeVersion> Differing, List<NodeVersion> Unknown)
    {
        public int Total => Matching.Count + Differing.Count + Unknown.Count;

        public VersionState State => Differing.Count > 0 ? VersionState.Differs
            : Matching.Count > 0 ? VersionState.InSync
            : VersionState.NodesUnknown;
    }
}
