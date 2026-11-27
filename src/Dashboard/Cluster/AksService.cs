using System.Text.Json;
using Dashboard.Health;

namespace Dashboard.Cluster;

/// <summary>
/// Azure's own facts about the AKS service: the content of the file at <c>cluster.serviceUrl</c> of the topology
/// (<c>aks.json</c>), which a scheduled workflow publishes several times an hour (GitHub starts a ten-minute schedule every 10 to 45 minutes). Slow by design: what Azure says,
/// next to what the cluster says about itself (<see cref="ClusterStatus"/>). Every part is optional.
/// </summary>
/// <param name="Generated">When the workflow read the facts from Azure: they are as of then.</param>
/// <param name="Location">The Azure region.</param>
/// <param name="Availability">The verdict of Azure Resource Health.</param>
/// <param name="PowerState">Running or Stopped.</param>
/// <param name="ProvisioningState">Succeeded, or what Azure is doing to the service (Updating, Starting, Stopping, Failed).</param>
/// <param name="Tier">The pricing tier of the control plane.</param>
public sealed record AksService(
    DateTimeOffset? Generated,
    string? Name,
    string? ResourceGroup,
    string? Location,
    AksAvailability? Availability,
    string? PowerState,
    string? ProvisioningState,
    string? KubernetesVersion,
    string? Tier,
    IReadOnlyList<AksPool> Pools,
    AksMetrics? Metrics)
{
    public const string Running = "Running";
    public const string Stopped = "Stopped";
    public const string Succeeded = "Succeeded";

    /// <summary>The facts are old from this age on: the workflow that publishes them may not be running.</summary>
    public static readonly TimeSpan OldAfter = TimeSpan.FromMinutes(90);

    /// <summary>True when Azure says the service is powered off: its power state is <see cref="Stopped"/>, in any case.</summary>
    public bool IsStopped => new AzureState(PowerState).Is(Stopped);

    /// <summary>True when Azure says the service is powered on: its power state is <see cref="Running"/>, in any case.</summary>
    public bool IsRunning => new AzureState(PowerState).Is(Running);

    /// <summary>
    /// True when Azure has finished with the service: its provisioning state is <see cref="Succeeded"/>, in any case.
    /// False when Azure is doing something to it (Updating, Starting, Stopping, Failed) or says nothing about it.
    /// </summary>
    public bool IsProvisioned => new AzureState(ProvisioningState).Is(Succeeded);

    /// <summary>True when the workflow read the facts longer ago than <see cref="OldAfter"/>.</summary>
    public bool IsOld(DateTimeOffset now) => Generated is { } generated && now - generated > OldAfter;

    /// <summary>
    /// Reads the file. It must be a JSON object that says something about the service: its availability, its power
    /// state or its provisioning state. Unknown fields are ignored.
    /// </summary>
    public static Parsed<AksService> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Parsed<AksService>(null, FileProblems.Empty);
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return new Parsed<AksService>(null, FileProblems.NotAnObject);
            }

            var service = new AksService(
                Generated: JsonRead.Time(root, "generated"),
                Name: JsonRead.Text(root, "name"),
                ResourceGroup: JsonRead.Text(root, "resourceGroup"),
                Location: JsonRead.Text(root, "location"),
                Availability: AksAvailability.Read(JsonRead.Section(root, "availability")),
                PowerState: JsonRead.Text(root, AksFields.PowerState),
                ProvisioningState: JsonRead.Text(root, AksFields.ProvisioningState),
                KubernetesVersion: JsonRead.Text(root, AksFields.KubernetesVersion),
                Tier: JsonRead.Text(root, "tier"),
                Pools: JsonRead.Named(root, "pools", AksPool.Read),
                Metrics: AksMetrics.Read(JsonRead.Section(root, "metrics")));
            return service is { Availability: null, PowerState: null, ProvisioningState: null }
                ? new Parsed<AksService>(null, $"the file says nothing about the service: no availability, no {AksFields.PowerState} and no {AksFields.ProvisioningState}")
                : new Parsed<AksService>(service);
        }
        catch (JsonException)
        {
            return new Parsed<AksService>(null, FileProblems.Broken);
        }
    }
}

/// <summary>The verdict of Azure Resource Health on the service.</summary>
/// <param name="State">Available, Unavailable, Degraded or Unknown.</param>
/// <param name="Summary">Azure's sentence about it.</param>
/// <param name="Reason">Why, where Azure says so.</param>
/// <param name="OccurredAt">Since when.</param>
public sealed record AksAvailability(string State, string? Summary, string? Reason, DateTimeOffset? OccurredAt)
{
    public const string Available = "Available";
    public const string Unavailable = "Unavailable";
    public const string Degraded = "Degraded";
    public const string Unknown = "Unknown";

    internal static AksAvailability? Read(JsonElement? section) =>
        JsonRead.Identified(section, "state", static (state, parts) =>
            new AksAvailability(state, JsonRead.Text(parts, "summary"), JsonRead.Text(parts, "reason"), JsonRead.Time(parts, "occurredAt")));
}

/// <summary>One node pool, as Azure describes it.</summary>
/// <param name="Mode">System or User.</param>
/// <param name="Count">How many nodes.</param>
/// <param name="Size">The size of their virtual machines.</param>
public sealed record AksPool(
    string Name,
    string? Mode,
    int? Count,
    string? Size,
    int? OsDiskGb,
    string? PowerState,
    string? ProvisioningState,
    string? KubernetesVersion)
{
    internal static AksPool Read(string name, JsonElement parts) =>
        new(
            name,
            JsonRead.Text(parts, "mode"),
            JsonRead.Count(parts, "count"),
            JsonRead.Text(parts, "size"),
            JsonRead.Count(parts, "osDiskGb"),
            JsonRead.Text(parts, AksFields.PowerState),
            JsonRead.Text(parts, AksFields.ProvisioningState),
            JsonRead.Text(parts, AksFields.KubernetesVersion));
}

/// <summary>
/// The field words the service and its pools share in the file: they describe the same Azure fields at both levels.
/// File-scoped, so the public surface is unchanged; the casing must stay exact, the lookup is ordinal and case-sensitive.
/// </summary>
file static class AksFields
{
    public const string PowerState = "powerState";
    public const string ProvisioningState = "provisioningState";
    public const string KubernetesVersion = "kubernetesVersion";
}

/// <summary>
/// Azure Monitor's numbers, each the average of the last <see cref="WindowMinutes"/> in percent: the nodes, and the
/// API server of the control plane. Each may be absent: Azure does not emit every metric for every cluster.
/// </summary>
public sealed record AksMetrics(
    int? WindowMinutes,
    double? NodeCpuPercent,
    double? NodeMemoryPercent,
    double? NodeDiskPercent,
    double? ApiServerCpuPercent = null,
    double? ApiServerMemoryPercent = null)
{
    /// <summary>True when Azure reports one number at least: without any, the card has no block of them.</summary>
    public bool HasAny => (NodeCpuPercent ?? NodeMemoryPercent ?? NodeDiskPercent ?? ApiServerCpuPercent ?? ApiServerMemoryPercent) is not null;

    internal static AksMetrics? Read(JsonElement? section) =>
        JsonRead.Present(section, static parts => new AksMetrics(
            JsonRead.Count(parts, "windowMinutes"),
            JsonRead.Number(parts, "nodeCpuPercent"),
            JsonRead.Number(parts, "nodeMemoryPercent"),
            JsonRead.Number(parts, "nodeDiskPercent"),
            JsonRead.Number(parts, "apiServerCpuPercent"),
            JsonRead.Number(parts, "apiServerMemoryPercent")));
}

/// <summary>
/// A state word Azure reports about the AKS service (its power state, its provisioning state, the verdict of
/// Resource Health), as a value the page asks about: compared in any case, and keeping the word as the file held it
/// for the label the page shows. A word that is not there (<see cref="IsMissing"/>) is not the empty word: the empty
/// string is a word, and matches none of Azure's.
/// </summary>
/// <param name="Text">The word as the file held it; null when the file does not say.</param>
public readonly record struct AzureState(string? Text)
{
    /// <summary>The word as the file held it; null when the file does not say.</summary>
    public string? Text { get; } = Text;

    /// <summary>True when the file does not say.</summary>
    public bool IsMissing => Text is null;

    /// <summary>True when the word is <paramref name="word"/>, in any case.</summary>
    public bool Is(string word) => string.Equals(Text, word, StringComparison.OrdinalIgnoreCase);

    /// <summary>True when both say a word and it is the same, in any case.</summary>
    public bool IsSameAs(AzureState other) => Text is not null && other.Text is not null && Is(other.Text);

    public override string ToString() => Text ?? string.Empty;
}
