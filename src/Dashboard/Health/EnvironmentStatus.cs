namespace Dashboard.Health;

/// <summary>An environment with the state of its deployables and the versions the deployments pinned for it in Git.</summary>
public sealed class EnvironmentStatus
{
    public EnvironmentStatus(EnvironmentInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);
        Info = info;
        Deployables = [.. info.Deployables.Select(deployable => new DeployableStatus(deployable))];
        Pinned = info.VersionsUrl is null ? PinnedVersions.NotTracked : PinnedVersions.Pending;
    }

    public EnvironmentInfo Info { get; }

    public string Name => Info.Name;

    public string? Tier => Info.Tier;

    public IReadOnlyList<DeployableStatus> Deployables { get; }

    /// <summary>The last reading of the environment's <c>versions.json</c>; a failed reading replaces a good one.</summary>
    public PinnedVersions Pinned { get; private set; }

    /// <summary>True when a deployable of this environment has a node that runs another version than the pinned one.</summary>
    public bool VersionsDiffer => Deployables.Any(deployable => AssessVersions(deployable) is { Differs: true });

    /// <summary>The last reading that succeeded: what a new reading is compared with to find a new pin.</summary>
    public PinnedVersions? LastReadPinned { get; private set; }

    public void Record(PinnedVersions pinned)
    {
        ArgumentNullException.ThrowIfNull(pinned);
        Pinned = pinned;
    }

    public void RememberPinned(PinnedVersions pinned) => LastReadPinned = pinned;

    /// <summary>
    /// The pinned version of a deployable next to the versions its nodes run: from the deployable's own pin where the
    /// topology names one (<c>pinUrl</c>), from the environment's <c>versions.json</c> otherwise; null when the
    /// topology names neither.
    /// </summary>
    public VersionAssessment? AssessVersions(DeployableStatus deployable)
    {
        ArgumentNullException.ThrowIfNull(deployable);
        var nodes = deployable.Nodes.Select(node => node.ToNodeVersion());
        if (deployable.Pinned is { } own)
        {
            return VersionAssessment.Assess(own, deployable.Info.Name, nodes, deployable.Info.PinFile);
        }

        return Info.VersionsUrl is null ? null : VersionAssessment.Assess(Pinned, deployable.Info.Name, nodes);
    }

    /// <summary>
    /// The page with the history of a deployable's pin: the deployable's own (<c>pinHistoryUrl</c>), or else the
    /// history of the environment's <c>versions.json</c>; null without both.
    /// </summary>
    public Uri? PinHistoryOf(DeployableStatus deployable)
    {
        ArgumentNullException.ThrowIfNull(deployable);
        return deployable.Info.PinHistoryUrl ?? Info.VersionsHistoryUrl;
    }
}
