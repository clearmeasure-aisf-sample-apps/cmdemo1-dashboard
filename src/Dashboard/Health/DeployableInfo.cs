namespace Dashboard.Health;

/// <summary>
/// One deployable of an environment: its public address (Front Door), the nodes behind it and the page of the project
/// that deploys it (<c>ProjectUrl</c>, in Octopus Deploy).
/// </summary>
/// <param name="TelemetryPath">
/// Where a node reports its own calls of the last minute (<c>/_telemetry</c>); null when the app has no such endpoint.
/// </param>
/// <param name="TrafficPaths">The paths the traffic button calls: representative requests, the start page first.</param>
/// <param name="BuildPath">
/// Where the primary node reports the build it runs (<c>/_build</c>, see <see cref="BuildInfo"/>); null when the app
/// has no such endpoint.
/// </param>
/// <param name="Links">Where the deployable's resources are (<see cref="LinkSet"/>); null without links.</param>
/// <param name="PinUrl">
/// Where the browser reads the deployable's own pin: a Kustomize file whose first <c>newTag</c> is the version pinned
/// in Git. Null when the environment's <c>versions.json</c> holds the pin.
/// </param>
/// <param name="PinHistoryUrl">The page with the history of that file; null for the history of <c>versions.json</c>.</param>
/// <param name="HealthDetailPath">
/// Where a node answers its detailed health check (<c>/_healthcheck/detailed</c>, see <see cref="HealthDetail"/>): one
/// entry per dependency it checks; null when the app has no such endpoint.
/// </param>
public sealed record DeployableInfo(
    string Name,
    Uri? FrontDoor,
    string HealthPath,
    string AlivePath,
    string VersionPath,
    IReadOnlyList<NodeInfo> Nodes,
    Uri? ProjectUrl = null,
    string? TelemetryPath = null,
    IReadOnlyList<string>? TrafficPaths = null,
    string? BuildPath = null,
    LinkSet? Links = null,
    Uri? PinUrl = null,
    Uri? PinHistoryUrl = null,
    string? HealthDetailPath = null)
{
    public const string DefaultHealthPath = "/_healthcheck";
    public const string DefaultAlivePath = "/alive";
    public const string DefaultVersionPath = "/_version";

    /// <summary>The name of the file that holds the deployable's pin: its kustomization's, or <c>versions.json</c>.</summary>
    public string PinFile => PinUrl is null ? PinnedVersions.FileName : PinnedVersions.FileOf(PinUrl);

    /// <summary>The path the chosen probe calls on every node of this deployable.</summary>
    public string PathFor(ProbeKind probe) => probe == ProbeKind.Liveness ? AlivePath : HealthPath;
}
