using System.Text.Json;

namespace Dashboard.Health;

/// <summary>
/// How the system is delivered: the content of the file at <c>system.deliveryUrl</c> of the topology
/// (<c>delivery.json</c>, which a workflow of the system repository publishes). Per environment and deployable the
/// last deployment, per environment what its hourly health reports found, and for the system the last failover test.
/// Every part is optional.
/// </summary>
/// <param name="Generated">When the file's content last changed: the facts are as of then. It is not the time of a check.</param>
public sealed record DeliveryReport(DateTimeOffset? Generated, IReadOnlyList<DeliveryEnvironment> Environments, FailoverTest? Failover)
{
    private DeliveryEnvironment? EnvironmentOf(string environment) =>
        Environments.FirstOrDefault(candidate => string.Equals(candidate.Name, environment, StringComparison.OrdinalIgnoreCase));

    /// <summary>The entry of a deployable in an environment; null when the file has none.</summary>
    public DeliveryEntry? Find(string environment, string deployable) =>
        EnvironmentOf(environment)
            ?.Deployables.FirstOrDefault(candidate => string.Equals(candidate.Name, deployable, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// The entries of an environment that belong to no deployable the page shows: the system project itself (named
    /// <see cref="SystemName"/>: the infrastructure and the pipeline) first, then the others in the file's order, such
    /// as the dashboard.
    /// </summary>
    public IReadOnlyList<DeliveryEntry> Others(string environment, IEnumerable<string> shown)
    {
        ArgumentNullException.ThrowIfNull(shown);
        var names = shown.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var entries = EnvironmentOf(environment)
            ?.Deployables.Where(deployable => !names.Contains(deployable.Name)) ?? [];
        return [.. entries.OrderBy(deployable => IsSystem(deployable.Name) ? 0 : 1)];
    }

    /// <summary>What the hourly health reports of an environment found; null when the file does not say.</summary>
    public HealthReports? HealthOf(string environment) =>
        EnvironmentOf(environment)?.Health;

    /// <summary>
    /// What the hourly health reports found for the container apps of an environment; null when the file does not say
    /// (a file from before it did, or one whose system could not read the reports' logs).
    /// </summary>
    public IReadOnlyList<ContainerAppReports>? ContainerAppsOf(string environment) => HealthOf(environment)?.ContainerApps;

    /// <summary>The name of the entry of the system project itself.</summary>
    public const string SystemName = "system";

    public static bool IsSystem(string name) => string.Equals(name, SystemName, StringComparison.OrdinalIgnoreCase);

    /// <summary>The file's content; null when it is not a JSON object with <c>environments</c> or <c>failover</c>.</summary>
    public static DeliveryReport? Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var environments = JsonRead.Items(root, "environments")
                .Select(environment => (Name: JsonRead.Text(environment, DeliveryFileWords.Name), Element: environment))
                .Where(environment => environment.Name is not null)
                .Select(environment => new DeliveryEnvironment(
                    environment.Name!,
                    [.. JsonRead.Items(environment.Element, "deployables").Select(DeliveryEntry.Read).OfType<DeliveryEntry>()],
                    HealthReports.Read(JsonRead.Section(environment.Element, "health"))))
                .ToList();
            var failover = FailoverTest.Read(JsonRead.Section(root, "failover"));
            return environments.Count == 0 && failover is null ? null : new DeliveryReport(JsonRead.Time(root, "generated"), environments, failover);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

/// <param name="Health">What the environment's hourly health reports found (<c>health</c>); null when the file does not say.</param>
public sealed record DeliveryEnvironment(string Name, IReadOnlyList<DeliveryEntry> Deployables, HealthReports? Health = null);

/// <summary>
/// What the pipeline's hourly health report found in an environment (<c>health</c> of an environment in
/// <c>delivery.json</c>): the runbook asks every node once an hour and fails when one is not healthy. A count of
/// hourly checks, not continuous monitoring: an outage between two checks is not in it.
/// </summary>
/// <param name="Last24Hours">The reports of the last 24 hours.</param>
/// <param name="Last7Days">The reports of the last seven days.</param>
/// <param name="LastFailure">When the last report that failed ended; null when none did in seven days.</param>
/// <param name="ContainerApps">
/// What the reports found for each container app of the environment (<c>containerApps</c>); null when the file does
/// not say, also where the environment has no container app.
/// </param>
public sealed record HealthReports(HealthWindow? Last24Hours, HealthWindow? Last7Days, DateTimeOffset? LastFailure, IReadOnlyList<ContainerAppReports>? ContainerApps = null)
{
    internal static HealthReports? Read(JsonElement? section)
    {
        var day = HealthWindow.Read(JsonRead.Section(section, HealthWindow.DayWord));
        var week = HealthWindow.Read(JsonRead.Section(section, HealthWindow.WeekWord));
        return day is null && week is null ? null : new HealthReports(day, week, JsonRead.Time(section, "lastFailure"), ContainerAppReports.ReadAll(section));
    }
}

/// <param name="Reports">How many health reports ended in the window.</param>
/// <param name="Healthy">How many of them found every node healthy.</param>
public sealed record HealthWindow(int Reports, int Healthy)
{
    /// <summary>The window of the last 24 hours, as the file writes it.</summary>
    internal const string DayWord = "last24Hours";

    /// <summary>The window of the last seven days, as the file writes it.</summary>
    internal const string WeekWord = "last7Days";

    private const string ReportsWord = "reports";
    private const string HealthyWord = "healthy";

    public bool AllHealthy => Healthy == Reports;

    /// <summary>The window; null unless both counts are there and no more reports are healthy than there were.</summary>
    internal static HealthWindow? Read(JsonElement? section) =>
        JsonRead.Count(section, ReportsWord) is { } reports && JsonRead.Count(section, HealthyWord) is { } healthy && healthy <= reports
            ? new HealthWindow(reports, healthy)
            : null;
}

/// <summary>The last deployment of a deployable to an environment.</summary>
/// <param name="Version">The version it deployed.</param>
/// <param name="DeployedAt">When it ended.</param>
/// <param name="SignedOffBy">Who signed it off.</param>
/// <param name="Reason">The reason given with the sign-off.</param>
/// <param name="Commit">The commit of the version.</param>
/// <param name="CommitAt">When that commit was made.</param>
/// <param name="LeadTimeHours">From the commit to this deployment.</param>
/// <param name="Behind">How far the environment is behind the first one.</param>
/// <param name="DeploymentsLast7Days">Deployments of the deployable to the environment in the last seven days.</param>
/// <param name="FailedLast7Days">Of those, the failed ones.</param>
/// <param name="ReleaseUrl">The release's page.</param>
/// <param name="LastPersonSignOff">
/// The newest deployment a person signed off: this one or an older one. Null when no person signed one off, in the
/// first environment and when the file does not say.
/// </param>
public sealed record DeliveryEntry(
    string Name,
    string? Version,
    DateTimeOffset? DeployedAt,
    string? SignedOffBy,
    string? Reason,
    string? Commit,
    DateTimeOffset? CommitAt,
    double? LeadTimeHours,
    BehindFirst? Behind,
    int? DeploymentsLast7Days,
    int? FailedLast7Days,
    Uri? ReleaseUrl,
    PersonSignOff? LastPersonSignOff = null)
{
    internal static DeliveryEntry? Read(JsonElement element)
    {
        if (JsonRead.Text(element, DeliveryFileWords.Name) is not { } name)
        {
            return null;
        }

        var behind = JsonRead.Section(element, "behindFirst");
        return new DeliveryEntry(
            name,
            VersionText.Read(element),
            JsonRead.Time(element, "deployedAt"),
            JsonRead.Text(element, "signedOffBy"),
            JsonRead.Text(element, DeliveryFileWords.Reason),
            JsonRead.Text(element, "commit"),
            JsonRead.Time(element, "commitAt"),
            JsonRead.Number(element, "leadTimeHours"),
            behind is null ? null : new BehindFirst(JsonRead.Count(behind, "versions"), JsonRead.Number(behind, "days")),
            JsonRead.Count(element, "deploymentsLast7Days"),
            JsonRead.Count(element, "failedLast7Days"),
            JsonRead.Address(element, DeliveryFileWords.ReleaseUrl),
            PersonSignOff.Read(JsonRead.Section(element, "lastPersonSignOff")));
    }
}

/// <summary>
/// The newest successful deployment of a deployable to an environment that a person signed off
/// (<c>lastPersonSignOff</c> of an entry in <c>delivery.json</c>). Automation signs off most deployments, and the next
/// one would take a person's sign-off off the card: this keeps it.
/// </summary>
/// <param name="Version">The version that deployment deployed.</param>
/// <param name="By">The person, by their user name in Octopus Deploy.</param>
/// <param name="At">When they signed it off.</param>
/// <param name="Reason">The note they gave with it; null when they gave none.</param>
/// <param name="ReleaseUrl">The page of that release.</param>
public sealed record PersonSignOff(string? Version, string? By, DateTimeOffset? At, string? Reason, Uri? ReleaseUrl)
{
    /// <summary>The sign-off; null unless it names a version or a person.</summary>
    internal static PersonSignOff? Read(JsonElement? section)
    {
        var version = VersionText.Read(section);
        var by = JsonRead.Text(section, "by");
        return version is null && by is null
            ? null
            : new PersonSignOff(version, by, JsonRead.Time(section, DeliveryFileWords.At), JsonRead.Text(section, DeliveryFileWords.Reason), JsonRead.Address(section, DeliveryFileWords.ReleaseUrl));
    }
}

/// <param name="Versions">How many releases the first environment is ahead, in the project's list of releases.</param>
/// <param name="Days">
/// Days since this environment and the first one last ran the same release; null when they never did.
/// </param>
public sealed record BehindFirst(int? Versions, double? Days);

/// <summary>The last test of the failover to the standby region.</summary>
/// <param name="Seconds">How long the public address took to answer from the standby.</param>
public sealed record FailoverTest(string? Environment, DateTimeOffset? At, double? Seconds)
{
    internal static FailoverTest? Read(JsonElement? section)
    {
        var test = new FailoverTest(JsonRead.Text(section, "environment"), JsonRead.Time(section, DeliveryFileWords.At), JsonRead.Number(section, "seconds"));
        return test == new FailoverTest(null, null, null) ? null : test;
    }
}

/// <summary>
/// The words of <c>delivery.json</c> that more than one reader of the file types by hand: naming them once keeps
/// the readers from drifting apart, so a rename in one cannot silently blank a card line in another.
/// </summary>
internal static class DeliveryFileWords
{
    /// <summary>The environment's name in <c>Parse</c>, and the deployable's name in <c>DeliveryEntry.Read</c>.</summary>
    internal const string Name = "name";

    /// <summary>The deployable's note in <c>DeliveryEntry.Read</c>, and the sign-off's note in <c>PersonSignOff.Read</c>.</summary>
    internal const string Reason = "reason";

    /// <summary>The deployable's release page in <c>DeliveryEntry.Read</c>, and the sign-off's in <c>PersonSignOff.Read</c>.</summary>
    internal const string ReleaseUrl = "releaseUrl";

    /// <summary>When the sign-off happened in <c>PersonSignOff.Read</c>, and when the failover test ended in <c>FailoverTest.Read</c>.</summary>
    internal const string At = "at";
}
