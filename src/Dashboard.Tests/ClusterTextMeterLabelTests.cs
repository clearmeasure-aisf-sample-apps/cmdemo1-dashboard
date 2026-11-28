namespace Dashboard.Tests;

/// <summary>
/// The five meter labels the two cluster tables pass as <c>UseCell.Label</c>, pinned byte-for-byte on
/// <see cref="ClusterText"/> where the value rules of the same cells live. The label is the meter's accessible name
/// and its tooltip prefix (<c>{Label}: {ValueText}</c> in <c>Meter.razor</c>), so a typo in one copy would change
/// both what a screen reader hears and what the hover shows. Each sentence is asserted against the literal the
/// markup produced before the move, and the whole node or pod name is kept - a long AKS node name is not shortened
/// to <see cref="ClusterText.ShortNode" />.
/// </summary>
public class ClusterTextMeterLabelTests
{
    // ----- Node labels: "… used of what it offers" -----

    /// <summary>The fixture node's CPU meter name, read through the fixture as the table reads it.</summary>
    [Fact]
    public void TheFixtureNodeSaysItsCpuMeterName()
    {
        Assert.Equal(
            "CPU of aks-system-12148983-vmss000000, used of what it offers",
            ClusterText.NodeCpuLabel(ClusterFixture.Node()));
    }

    /// <summary>The fixture node's memory meter name.</summary>
    [Fact]
    public void TheFixtureNodeSaysItsMemoryMeterName()
    {
        Assert.Equal(
            "Memory of aks-system-12148983-vmss000000, used of what it offers",
            ClusterText.NodeMemoryLabel(ClusterFixture.Node()));
    }

    /// <summary>The fixture node's pods meter name.</summary>
    [Fact]
    public void TheFixtureNodeSaysItsPodsMeterName()
    {
        Assert.Equal(
            "Pods on aks-system-12148983-vmss000000, of how many it can take",
            ClusterText.NodePodsLabel(ClusterFixture.Node()));
    }

    /// <summary>A node with a changed name is said by that name, whole: the label is not the shortened one.</summary>
    [Fact]
    public void AChangedNodeNameIsKeptWholeInTheLabel()
    {
        var longName = "aks-system-12148983-vmss000000-very-long-node-name";
        var node = ClusterFixture.Node(name: longName);

        Assert.Equal($"CPU of {longName}, used of what it offers", ClusterText.NodeCpuLabel(node));
        Assert.Equal($"Memory of {longName}, used of what it offers", ClusterText.NodeMemoryLabel(node));
        Assert.Equal($"Pods on {longName}, of how many it can take", ClusterText.NodePodsLabel(node));

        // The label is not the shortened name: the whole name is kept.
        Assert.NotEqual($"CPU of {ClusterText.ShortNode(longName)}, used of what it offers", ClusterText.NodeCpuLabel(node));
    }

    /// <summary>The null argument is named at the throw.</summary>
    [Fact]
    public void ANullNodeIsNamed()
    {
        var thrown = Assert.Throws<ArgumentNullException>(() => ClusterText.NodeCpuLabel(null!));
        Assert.Equal("node", thrown.ParamName);

        thrown = Assert.Throws<ArgumentNullException>(() => ClusterText.NodeMemoryLabel(null!));
        Assert.Equal("node", thrown.ParamName);

        thrown = Assert.Throws<ArgumentNullException>(() => ClusterText.NodePodsLabel(null!));
        Assert.Equal("node", thrown.ParamName);
    }

    // ----- Pod labels: "… against its limit" -----

    /// <summary>The fixture pod's CPU meter name.</summary>
    [Fact]
    public void TheFixturePodSaysItsCpuMeterName()
    {
        Assert.Equal(
            "CPU of ui-0 against its limit",
            ClusterText.PodCpuLabel(ClusterFixture.Pod("ui")));
    }

    /// <summary>The fixture pod's memory meter name.</summary>
    [Fact]
    public void TheFixturePodSaysItsMemoryMeterName()
    {
        Assert.Equal(
            "Memory of ui-0 against its limit",
            ClusterText.PodMemoryLabel(ClusterFixture.Pod("ui")));
    }

    /// <summary>A pod with a changed name is said by that name, whole.</summary>
    [Fact]
    public void AChangedPodNameIsKeptWholeInTheLabel()
    {
        var pod = ClusterFixture.Pod("ui", name: "ui-6d5f7c9b8-x2k4q");

        Assert.Equal("CPU of ui-6d5f7c9b8-x2k4q against its limit", ClusterText.PodCpuLabel(pod));
        Assert.Equal("Memory of ui-6d5f7c9b8-x2k4q against its limit", ClusterText.PodMemoryLabel(pod));
    }

    /// <summary>The null argument is named at the throw.</summary>
    [Fact]
    public void ANullPodIsNamed()
    {
        var thrown = Assert.Throws<ArgumentNullException>(() => ClusterText.PodCpuLabel(null!));
        Assert.Equal("pod", thrown.ParamName);

        thrown = Assert.Throws<ArgumentNullException>(() => ClusterText.PodMemoryLabel(null!));
        Assert.Equal("pod", thrown.ParamName);
    }
}
