namespace Dashboard.Health;

/// <summary>A deployable of an environment with the state of its endpoints.</summary>
public sealed class DeployableStatus
{
    public DeployableStatus(DeployableInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);
        Info = info;
        FrontDoor = TargetStatus.ForFrontDoor(info);
        Nodes = [.. info.Nodes.Select(TargetStatus.ForNode)];
        BuildSource = info.BuildPath is null ? null : Nodes.FirstOrDefault(node => node.IsPrimary) ?? (Nodes.Count > 0 ? Nodes[0] : null);
        Pinned = info.PinUrl is null ? null : PinnedVersions.Pending;
    }

    /// <summary>
    /// The last reading of the deployable's own pin (<c>pinUrl</c>, a Kustomize file); a failed reading replaces a good
    /// one. Null when the topology names none: the environment's <c>versions.json</c> then holds the pin.
    /// </summary>
    public PinnedVersions? Pinned { get; private set; }

    /// <summary>The last reading of the deployable's own pin that succeeded: what a new reading is compared with to find a new pin.</summary>
    public PinnedVersions? LastReadPinned { get; private set; }

    public void RecordPin(PinnedVersions pinned)
    {
        ArgumentNullException.ThrowIfNull(pinned);
        Pinned = pinned;
    }

    public void RememberPin(PinnedVersions pinned) => LastReadPinned = pinned;

    /// <summary>The node asked for the build facts: the primary; null when the topology names no <c>buildPath</c>.</summary>
    public TargetStatus? BuildSource { get; }

    /// <summary>The build the deployable runs in this environment, as its primary node reports it; null when it reports none.</summary>
    public BuildInfo? Build { get; private set; }

    /// <summary>True once the build endpoint was asked.</summary>
    public bool BuildWasRead { get; private set; }

    /// <summary>The version the node ran when the build endpoint was asked: another version is another build.</summary>
    public string? BuildReadForVersion { get; private set; }

    /// <summary>
    /// The three parts above as the one read they are, and the only way they are written: the rule of when the build
    /// is due, and what a read leaves behind, belong to <see cref="Health.BuildRead"/>.
    /// </summary>
    private BuildRead BuildRead
    {
        get => BuildWasRead ? BuildRead.Read(Build, BuildReadForVersion) : BuildRead.NotYet;
        set => (Build, BuildReadForVersion, BuildWasRead) = (value.Build, value.Version, value.WasRead);
    }

    /// <summary>The serving decision at the end of the last round of checks; null before the first.</summary>
    public ServingAssessment? LastServing { get; private set; }

    /// <summary>True when the build endpoint should be asked now: see <see cref="BuildRead.IsDue"/>.</summary>
    public bool BuildIsDue(string? version) => BuildRead.IsDue(version);

    public void RecordBuild(BuildInfo? build, string? version) => BuildRead = BuildRead.Read(build, version);

    public void RecordServing(ServingAssessment assessment) => LastServing = assessment;

    public DeployableInfo Info { get; }

    public TargetStatus? FrontDoor { get; }

    /// <summary>True when the deployable has a Front Door endpoint: what a tile says about Front Door is shown only then.</summary>
    public bool HasFrontDoor => FrontDoor is not null;

    public IReadOnlyList<TargetStatus> Nodes { get; }

    /// <summary>The Front Door endpoint first, then the nodes in the order of the topology.</summary>
    public IEnumerable<TargetStatus> Targets => FrontDoor is null ? Nodes : Nodes.Prepend(FrontDoor);

    public ServingAssessment Assess() => Assess(out _);

    /// <summary>The serving decision, and the tile of the node it expects to serve the traffic (if any).</summary>
    public ServingAssessment Assess(out TargetStatus? expected)
    {
        var assessment = AssessServing(out IReadOnlyList<TargetStatus> serving);
        expected = serving.Count > 0 ? serving[0] : null;
        return assessment;
    }

    /// <summary>
    /// The serving decision, and the tiles of the nodes it expects to serve the traffic: every healthy primary of a
    /// rotation, and otherwise one node or none.
    /// </summary>
    public ServingAssessment AssessServing(out IReadOnlyList<TargetStatus> serving)
    {
        var healths = Nodes.Select(node => node.ToNodeHealth()).ToList();
        var assessment = ServingAssessment.Assess(healths, FrontDoor?.State);
        var expected = assessment.Serving;
        serving = [.. Nodes.Where((_, index) => expected.Any(health => ReferenceEquals(health, healths[index])))];
        return assessment;
    }
}
