namespace Dashboard.Tests;

/// <summary>
/// Pins that <see cref="ResourceUses" /> puts each named number in its own slot of <see cref="ResourceUse" />: the
/// pod's limit in <see cref="ResourceUse.Limits" /> and never in <see cref="ResourceUse.Allocatable" />, what the
/// node offers in <see cref="ResourceUse.Allocatable" /> and never in <see cref="ResourceUse.Limits" /> - the
/// difference the pod's and the node's readings of <see cref="ClusterText" /> turn on.
/// </summary>
public class ResourceUsesObjectMotherTests
{
    [Fact]
    public void APodsUsePutsEachNumberInItsOwnSlotAndOffersNothing()
    {
        var use = ResourceUses.Pod(usage: 1, requests: 2, limit: 3);

        Assert.Equal(1, use.Usage);
        Assert.Equal(2, use.Requests);
        Assert.Equal(3, use.Limits);
        Assert.Null(use.Allocatable);
    }

    [Fact]
    public void ANodesUsePutsWhatItOffersInAllocatableAndNotInLimits()
    {
        var use = ResourceUses.Node(usage: 1, requests: 2, allocatable: 4);

        Assert.Equal(1, use.Usage);
        Assert.Equal(2, use.Requests);
        Assert.Null(use.Limits);
        Assert.Equal(4, use.Allocatable);
    }

    [Fact]
    public void ANodesUseKeepsTheLimitsItsPodsSetWhenNamed()
    {
        Assert.Equal(
            new ResourceUse(Usage: 1, Requests: 2, Limits: 3, Allocatable: 4),
            ResourceUses.Node(usage: 1, requests: 2, allocatable: 4, limits: 3));
    }

    [Fact]
    public void NothingNamedIsTheEmptyUse()
    {
        Assert.Equal(ResourceUse.None, ResourceUses.Pod());
        Assert.Equal(ResourceUse.None, ResourceUses.Node());
    }

    /// <summary>The same whole read as a pod's limit and as a node's offer lands in two different slots.</summary>
    [Fact]
    public void ThePodsLimitAndTheNodesOfferAreNotTheSameUse()
    {
        Assert.NotEqual(ResourceUses.Pod(usage: 1500, limit: 1000), ResourceUses.Node(usage: 1500, allocatable: 1000));
    }
}
