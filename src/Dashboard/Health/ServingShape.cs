namespace Dashboard.Health;

/// <summary>
/// The shape of a deployable, which decides the words its <see cref="ServingAssessment"/> says: a
/// <see cref="Rotation"/> of primary nodes behind a Front Door endpoint, the <see cref="Only"/> node of a deployable
/// without one, or a <see cref="Plain"/> primary-with-standby deployable. Said once, with the nodes each shape names,
/// so no member has to infer it again from two nullable parts.
/// </summary>
public abstract record ServingShape
{
    private ServingShape()
    {
    }

    /// <summary>
    /// The shape of the two optional parts of an assessment: a rotation outranks an only node, and an assessment with
    /// neither is plain.
    /// </summary>
    public static ServingShape Of(NodeHealth? onlyNode, IReadOnlyList<NodeHealth>? rotation) =>
        rotation is not null ? new Rotation(rotation)
        : onlyNode is not null ? new Only(onlyNode)
        : new Plain();

    /// <summary>A Front Door endpoint rotating over several primary nodes, in the order of the topology.</summary>
    public sealed record Rotation(IReadOnlyList<NodeHealth> Primaries) : ServingShape
    {
        /// <summary>The primary nodes that are healthy, in the order of the topology.</summary>
        public IReadOnlyList<NodeHealth> Healthy => [.. Primaries.Where(node => node.State == HealthState.Healthy)];

        /// <summary>The primary nodes that are not healthy, in the order of the topology.</summary>
        public IReadOnlyList<NodeHealth> NotHealthy => [.. Primaries.Where(node => node.State != HealthState.Healthy)];
    }

    /// <summary>The only node of a deployable without a Front Door endpoint: it serves, or it does not.</summary>
    public sealed record Only(NodeHealth Node) : ServingShape;

    /// <summary>One node, or primaries with standbys behind them: one node is expected to serve.</summary>
    public sealed record Plain : ServingShape;
}
