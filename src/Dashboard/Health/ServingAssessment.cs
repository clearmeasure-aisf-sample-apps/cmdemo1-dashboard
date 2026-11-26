namespace Dashboard.Health;

/// <summary>
/// Which node is expected to serve a deployable's traffic: the first healthy node in priority order (primary nodes,
/// then the others, each in the order of the topology), and whether the Front Door endpoint agrees. A deployable with
/// a Front Door endpoint and more than one primary node is a rotation: the Front Door rotates over every healthy
/// primary node, and a standby serves only when no primary node is healthy.
/// </summary>
/// <param name="OnlyNode">
/// The only node of a deployable without a Front Door endpoint (one app at one public address, as in a cluster); null
/// for every other deployable. Nothing can take over from such a node, so its words name no primary, no standby, no
/// region and no failover: it serves, or it does not.
/// </param>
/// <param name="Rotation">
/// The primary nodes the Front Door rotates over, in the order of the topology; null for a deployable that is not a
/// rotation. <see cref="Expected"/> is then the first of them that is healthy.
/// </param>
public sealed record ServingAssessment(
    ServingState State,
    NodeHealth? Expected,
    NodeHealth? FailedPrimary,
    FrontDoorAgreement FrontDoor,
    HealthState? FrontDoorState,
    NodeHealth? OnlyNode = null,
    IReadOnlyList<NodeHealth>? Rotation = null)
{
    /// <summary>How many nodes that left a rotation the headline names before it counts the rest.</summary>
    public const int MostNamed = 3;

    public bool IsFailedOver => State == ServingState.FailedOver;

    /// <summary>
    /// The shape of the deployable, said once from the two optional parts (see <see cref="ServingShape.Of"/>): every
    /// member below reads it instead of asking the parts again. Private, so the public surface and the words of the
    /// record stay its own.
    /// </summary>
    private ServingShape Shape => ServingShape.Of(OnlyNode, Rotation);

    /// <summary>True when the Front Door rotates over several primary nodes.</summary>
    public bool IsRotation => Shape is ServingShape.Rotation;

    /// <summary>
    /// The nodes expected to serve the traffic: every healthy primary node of a rotation, and otherwise the one node
    /// <see cref="Expected"/> names (none when nothing serves or nothing is decided).
    /// </summary>
    public IReadOnlyList<NodeHealth> Serving =>
        Shape is ServingShape.Rotation rotation && State == ServingState.Primary ? rotation.Healthy
        : Expected is null ? []
        : [Expected];

    /// <summary>The primary nodes of a rotation that are not healthy, in the order of the topology; none without a rotation.</summary>
    public IReadOnlyList<NodeHealth> OutOfRotation =>
        Shape is ServingShape.Rotation rotation ? rotation.NotHealthy : [];

    /// <summary>The headline in the words of the deployable's shape: a rotation, an only node, or a plain deployable.</summary>
    public string Headline => Shape switch
    {
        ServingShape.Rotation rotation => RotationWords(rotation),
        ServingShape.Only only => OnlyNodeWords(only.Node),
        _ => PlainWords(),
    };

    /// <summary>A rotation names regions in the plural: the Front Door rotates over several.</summary>
    private string RotationWords(ServingShape.Rotation rotation) => State switch
    {
        ServingState.Pending => "Checking which regions serve traffic",
        ServingState.Primary => RotationHeadline(rotation.Primaries.Count, rotation.NotHealthy),
        ServingState.FailedOver => FailedOverWords(),
        _ => "No region is healthy",
    };

    /// <summary>The only node names no primary, no standby and no failover: it serves, or it does not.</summary>
    private string OnlyNodeWords(NodeHealth only) => State switch
    {
        ServingState.Pending => $"Checking {only.Label}",
        ServingState.Primary or ServingState.FailedOver => $"Serves traffic: {only.Label}",
        _ => $"Not serving: {only.Label}",
    };

    /// <summary>A plain deployable names the one node expected to serve.</summary>
    private string PlainWords() => State switch
    {
        ServingState.NoNodes => "No nodes in the topology",
        ServingState.Pending => "Checking which region serves traffic",
        ServingState.Primary => $"Expected to serve traffic: {Expected!.Label} (primary)",
        ServingState.FailedOver => FailedOverWords(),
        _ => "No healthy node: nothing can serve traffic",
    };

    /// <summary>A rotation and a plain deployable fail over in the same words.</summary>
    private string FailedOverWords() => $"Failed over to {Expected!.Label}";

    /// <summary>
    /// Why the deployable failed over; null in every other state, and for a single node. It asks for the only node
    /// part itself rather than <see cref="Shape"/>, where a rotation outranks it: an assessment handed both a rotation
    /// and an only node (which <see cref="Assess"/> never builds) has always told no detail.
    /// </summary>
    public string? FailoverDetail => State != ServingState.FailedOver || OnlyNode is not null
        ? null
        : FailedPrimary is null
            ? $"No primary node is healthy; {Expected!.Label} is expected to serve traffic."
            : $"Primary {FailedPrimary.Label} is {HealthClassifier.Sentence(FailedPrimary.State)}; {Expected!.Label} is expected to serve traffic.";

    /// <summary>Whether the Front Door endpoint shows what the nodes predict; null without a Front Door endpoint.</summary>
    public string? FrontDoorText => FrontDoor switch
    {
        FrontDoorAgreement.NotPresent => null,
        FrontDoorAgreement.Pending => "Front Door is being checked.",
        FrontDoorAgreement.Agrees when FrontDoorState == HealthState.Healthy => "Front Door agrees: it is healthy.",
        FrontDoorAgreement.Agrees => $"Front Door agrees: it is {HealthClassifier.Sentence(FrontDoorState)} too.",
        _ when FrontDoorState == HealthState.Healthy => "Front Door disagrees: it is healthy although no node is.",
        _ => $"Front Door disagrees: it is {HealthClassifier.Sentence(FrontDoorState)} although {Expected?.Label ?? "a node"} is healthy.",
    };

    public static ServingAssessment Assess(IEnumerable<NodeHealth> nodes, HealthState? frontDoor)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        // OrderBy is stable: within primaries and within standbys, the order of the topology is the priority.
        var byPriority = nodes.OrderBy(node => node.IsPrimary ? 0 : 1).ToList();
        var primaries = byPriority.Where(node => node.IsPrimary).ToList();
        if (frontDoor is not null && primaries.Count > 1)
        {
            var (rotating, first) = DecideRotation(primaries, byPriority);
            // No primary is named as the failed one: the detail says that none is healthy.
            return Decided(rotating, first, frontDoor, rotation: primaries);
        }

        var (state, expected) = Decide(byPriority);
        var failedPrimary = state == ServingState.FailedOver ? byPriority.FirstOrDefault(node => node.IsPrimary) : null;
        var single = frontDoor is null && byPriority.Count == 1 ? byPriority[0] : null;
        return Decided(state, expected, frontDoor, failedPrimary, onlyNode: single);
    }

    /// <summary>
    /// The one way a decision becomes an assessment: the Front Door parts are compared and handed over the same for
    /// every shape, and the shape is the one of <paramref name="onlyNode"/> and <paramref name="rotation"/> that is set
    /// (<paramref name="onlyNode"/> for a lone node without a Front Door endpoint, <paramref name="rotation"/> for the
    /// primary nodes a Front Door rotates over, neither for a plain deployable).
    /// </summary>
    private static ServingAssessment Decided(
        ServingState state,
        NodeHealth? expected,
        HealthState? frontDoor,
        NodeHealth? failedPrimary = null,
        NodeHealth? onlyNode = null,
        IReadOnlyList<NodeHealth>? rotation = null) =>
        new(
            State: state,
            Expected: expected,
            FailedPrimary: failedPrimary,
            FrontDoor: Compare(state, frontDoor),
            FrontDoorState: frontDoor,
            OnlyNode: onlyNode,
            Rotation: rotation);

    /// <summary>
    /// A rotation is decided when every primary node is checked: the healthy ones serve. With none healthy, the
    /// standbys are taken in the order of the topology, as for one primary.
    /// </summary>
    private static (ServingState State, NodeHealth? Expected) DecideRotation(List<NodeHealth> primaries, List<NodeHealth> byPriority)
    {
        if (primaries.Any(node => node.State == HealthState.Pending))
        {
            return (ServingState.Pending, null);
        }

        return primaries.FirstOrDefault(node => node.State == HealthState.Healthy) is { } healthy
            ? (ServingState.Primary, healthy)
            : Decide(byPriority);
    }

    /// <summary><c>Front Door rotates over 10 of 11 regions: wus3 is not healthy</c>.</summary>
    private static string RotationHeadline(int count, IReadOnlyList<NodeHealth> stopped)
    {
        if (stopped.Count == 0)
        {
            return $"Front Door rotates over {count} regions: all healthy";
        }

        var listed = NamedList.Words([.. stopped.Select(node => node.Label)], MostNamed);
        return $"Front Door rotates over {count - stopped.Count} of {count} regions: {listed} {Plural.Agreement(stopped.Count, "is", "are")} not healthy";
    }

    private static (ServingState State, NodeHealth? Expected) Decide(List<NodeHealth> byPriority)
    {
        if (byPriority.Count == 0)
        {
            return (ServingState.NoNodes, null);
        }

        foreach (var node in byPriority)
        {
            if (node.State == HealthState.Healthy)
            {
                return (node.IsPrimary ? ServingState.Primary : ServingState.FailedOver, node);
            }

            // A node with a higher priority than every healthy one is still unchecked: no decision yet.
            if (node.State == HealthState.Pending)
            {
                return (ServingState.Pending, null);
            }
        }

        return (ServingState.Down, null);
    }

    private static FrontDoorAgreement Compare(ServingState state, HealthState? frontDoor)
    {
        if (frontDoor is null)
        {
            return FrontDoorAgreement.NotPresent;
        }

        if (frontDoor == HealthState.Pending || state is ServingState.Pending or ServingState.NoNodes)
        {
            return FrontDoorAgreement.Pending;
        }

        var nodeServes = state is ServingState.Primary or ServingState.FailedOver;
        return nodeServes == (frontDoor == HealthState.Healthy) ? FrontDoorAgreement.Agrees : FrontDoorAgreement.Disagrees;
    }

}
