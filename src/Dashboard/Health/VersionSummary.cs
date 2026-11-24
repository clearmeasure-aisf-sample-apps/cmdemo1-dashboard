namespace Dashboard.Health;

/// <summary>The second line of the header: shown only when the versions differ somewhere.</summary>
public sealed record VersionSummary(int EnvironmentsThatDiffer)
{
    public bool Differs => EnvironmentsThatDiffer > 0;

    /// <summary><c>Versions differ in 1 environment</c>, counted through <see cref="Plural" />; null when they differ nowhere.</summary>
    public string? Text => EnvironmentsThatDiffer switch
    {
        <= 0 => null,
        _ => $"Versions differ in {Plural.Of(EnvironmentsThatDiffer, "environment")}",
    };
}
