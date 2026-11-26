namespace Dashboard.Tests;

/// <summary>
/// The ObjectMother of a <see cref="ResourceUse" />: the pod's reading and the node's reading named by what they are,
/// so a test chooses between "the limit" (what a pod may use at most) and "what the node offers" (its allocatable) by
/// the method it calls rather than by counting commas in the four positional slots of the record. A test that means a
/// use filling all four slots, or one in an order the cluster never writes, calls the record's constructor instead.
/// </summary>
public static class ResourceUses
{
    /// <summary>A pod's use: what it uses, asks for and may use at most. A pod offers nothing, so it has no allocatable.</summary>
    public static ResourceUse Pod(double? usage = null, double? requests = null, double? limit = null) =>
        new(Usage: usage, Requests: requests, Limits: limit, Allocatable: null);

    /// <summary>
    /// A node's use: what its pods use and ask for, and what the node offers them. The limits its pods set are left
    /// absent unless a test names them.
    /// </summary>
    public static ResourceUse Node(double? usage = null, double? requests = null, double? allocatable = null, double? limits = null) =>
        new(Usage: usage, Requests: requests, Limits: limits, Allocatable: allocatable);
}
