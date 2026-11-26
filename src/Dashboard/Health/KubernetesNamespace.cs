namespace Dashboard.Health;

/// <summary>
/// The name of a Kubernetes namespace, as the topology names an environment's namespace and the cluster status names
/// the namespaces it lists. Kubernetes names are case-sensitive, so two names are the same namespace only when they are
/// equal ordinally: this is the one comparison rule every cluster caller uses, through <see cref="Equals(KubernetesNamespace)"/>,
/// <see cref="Is"/> or <see cref="Comparer"/>. A null name is the empty name, and so is the default value.
/// </summary>
public readonly record struct KubernetesNamespace
{
    /// <summary>The comparer of namespace names as plain text: ordinal, case for case.</summary>
    public static readonly StringComparer Comparer = StringComparer.Ordinal;

    private readonly string? value;

    /// <summary>The name; null is the empty name.</summary>
    public KubernetesNamespace(string? value) => this.value = value ?? string.Empty;

    /// <summary>The name as written; never null, even for the default value.</summary>
    public string Value => value ?? string.Empty;

    /// <summary>True when the named namespace, case for case, is this one; a missing name is none.</summary>
    public bool Is(string? name) => name is not null && Comparer.Equals(Value, name);

    /// <summary>Ordinal identity: the same name, case for case.</summary>
    public bool Equals(KubernetesNamespace other) => Comparer.Equals(Value, other.Value);

    /// <summary>The ordinal hash of the name, consistent with <see cref="Equals(KubernetesNamespace)"/>.</summary>
    public override int GetHashCode() => Comparer.GetHashCode(Value);

    /// <summary>The name as written.</summary>
    public override string ToString() => Value;
}
