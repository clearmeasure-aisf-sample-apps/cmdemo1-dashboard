namespace Dashboard.Runtime;

/// <summary>
/// The name of an environment of the topology. Two names are the same environment only when they are equal ordinally,
/// so a dictionary keyed by names keeps "UAT" and "uat" apart; the page's address, typed by hand, picks an environment
/// case-insensitively through <see cref="Matches"/>. A null name is the empty name, and so is the default value.
/// </summary>
public readonly record struct EnvironmentName
{
    private readonly string? value;

    /// <summary>The name; null is the empty name.</summary>
    public EnvironmentName(string? value) => this.value = value ?? string.Empty;

    /// <summary>The name as written; never null, even for the default value.</summary>
    public string Value => value ?? string.Empty;

    /// <summary>True when the page's environment, in any case, is this one; a missing page environment matches none.</summary>
    public bool Matches(string? page) => string.Equals(Value, page, StringComparison.OrdinalIgnoreCase);

    /// <summary>Ordinal identity: the same name, case for case.</summary>
    public bool Equals(EnvironmentName other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <summary>The ordinal hash of the name, consistent with <see cref="Equals(EnvironmentName)"/>.</summary>
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>The name as written.</summary>
    public override string ToString() => Value;
}
