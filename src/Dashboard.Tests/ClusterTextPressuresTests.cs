namespace Dashboard.Tests;

/// <summary>
/// The tests of <see cref="ClusterText.Pressures"/>: the words of a node's list of conditions, joined in the order
/// given. The single condition is pinned in <see cref="ClusterTextTests"/> (<see cref="ClusterText.Pressure"/>) and
/// here the list is said whole: two and more conditions in their order, the one condition on its own, the empty list
/// that says nothing, the condition the view does not know, and the argument that is nothing.
/// </summary>
public class ClusterTextPressuresTests
{
    /// <summary>The two conditions a node under memory and disk pressure is said in: the words joined in the order given.</summary>
    [Fact]
    public void TwoConditionsAreSaidInTheirOrderWithTheListSeparator() =>
        Assert.Equal("memory pressure, disk pressure", ClusterText.Pressures(["MemoryPressure", "DiskPressure"]));

    /// <summary>Three conditions keep their order and their separator.</summary>
    [Fact]
    public void ThreeConditionsAreSaidInTheOrderGiven() =>
        Assert.Equal("memory pressure, disk pressure, PID pressure",
            ClusterText.Pressures(["MemoryPressure", "DiskPressure", "PIDPressure"]));

    /// <summary>A single condition gives just its word, without a separator.</summary>
    [Fact]
    public void ASingleConditionGivesJustItsWord() =>
        Assert.Equal("memory pressure", ClusterText.Pressures(["MemoryPressure"]));

    /// <summary>An empty list says nothing.</summary>
    [Fact]
    public void AnEmptyListSaysNothing() =>
        Assert.Equal(string.Empty, ClusterText.Pressures(Array.Empty<string>()));

    /// <summary>The condition the view does not know passes through unchanged.</summary>
    [Fact]
    public void AnUnknownConditionPassesThroughUnchanged() =>
        Assert.Equal("SomethingNew", ClusterText.Pressures(["SomethingNew"]));

    /// <summary>The argument that is nothing throws, as the class's other public members do.</summary>
    [Fact]
    public void ANullArgumentThrows() =>
        Assert.Equal("conditions", Assert.Throws<ArgumentNullException>(() => ClusterText.Pressures(null!)).ParamName);
}
