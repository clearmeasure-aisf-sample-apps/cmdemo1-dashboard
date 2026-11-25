namespace Dashboard.Runtime;

/// <summary>The outcome of loading <c>runtime/index.json</c>.</summary>
public sealed record RuntimeIndexResult(RuntimeAvailability Availability, RuntimeIndex? Index, IReadOnlyList<string> Errors)
{
    public static RuntimeIndexResult NotDeployed { get; } = new(RuntimeAvailability.NotDeployed, null, []);

    /// <summary>The index was read: available, with no errors.</summary>
    internal static RuntimeIndexResult Read(RuntimeIndex index) => new(RuntimeAvailability.Available, index, []);

    /// <summary>The files are there and cannot be read: no index, the reasons why.</summary>
    internal static RuntimeIndexResult Failed(IReadOnlyList<string> errors) => new(RuntimeAvailability.Failed, null, errors);

    /// <summary>The site has no <c>runtime/</c>: the page says so, not as a problem.</summary>
    internal bool IsNotDeployed => Availability == RuntimeAvailability.NotDeployed;

    /// <summary>There is an index to show (it may list no environment); otherwise <see cref="Errors"/> say why not.</summary>
    internal bool IsValid => Index is not null;

    /// <summary>The index lists at least one environment to pick and draw.</summary>
    internal bool HasEnvironments => Index is { Environments.Count: > 0 };

    /// <summary>Whether the index records a database view for the environment of that exact name.</summary>
    internal bool RecordsDatabaseOf(string environment) =>
        Index?.Environments.Any(entry => entry.Database is not null && string.Equals(entry.Name, environment, StringComparison.Ordinal)) ?? false;
}
