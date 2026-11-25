namespace Dashboard.Tests;

/// <summary>
/// The four "use against a whole" members of <see cref="ClusterText"/> share one rule - null without a usage, against
/// the whole when it is more than zero, plain otherwise - and the memory units are one table, largest first, whose
/// last row (KiB) also says what is none of the units. These tests pin that the rule and the table say the same
/// thing for every member and every unit end.
/// </summary>
public class ClusterTextUseAgainstTests
{
    private const double Kibibyte = 1024;
    private const double Mebibyte = Kibibyte * 1024;
    private const double Gibibyte = Mebibyte * 1024;

    /// <summary>No usage is nothing to say for each of the four, whatever the whole.</summary>
    [Fact]
    public void EachOfTheFourSaysNothingWithoutAUsage()
    {
        var use = new ResourceUse(null, 100, 500, 1000);
        Assert.Null(ClusterText.CpuUse(use));
        Assert.Null(ClusterText.MemoryUse(use));
        Assert.Null(ClusterText.NodeCpu(use));
        Assert.Null(ClusterText.NodeMemory(use));
    }

    /// <summary>A whole of zero, a negative one, a NaN or none is not a whole: each of the four says the plain number.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(double.NaN)]
    public void EachOfTheFourSaysThePlainNumberWithoutAWholeMoreThanZero(double? whole)
    {
        Assert.Equal("1.5 cores", ClusterText.CpuUse(new ResourceUse(1500, null, whole)));
        Assert.Equal("2 MiB", ClusterText.MemoryUse(new ResourceUse(2 * Mebibyte, null, whole)));
        Assert.Equal("1.5 cores", ClusterText.NodeCpu(new ResourceUse(1500, null, null, whole)));
        Assert.Equal("2 MiB", ClusterText.NodeMemory(new ResourceUse(2 * Mebibyte, null, null, whole)));
    }

    /// <summary>The node's plain CPU says "cores" even for one whole core, unlike the pod's.</summary>
    [Fact]
    public void TheNodesPlainCpuIsAlwaysInCores()
    {
        Assert.Equal("1 cores", ClusterText.NodeCpu(new ResourceUse(1000, null, null)));
        Assert.Equal("1 core", ClusterText.CpuUse(new ResourceUse(1000, null, null)));
    }

    /// <summary>
    /// The decision of #1745, pinned beside the pin that first named it: the node's plain CPU stays deliberately
    /// always-plural, unlike the pod's. The node's line says its CPU in cores, and the word of that reading follows
    /// the whole where the line says one ("1 of 1 core") and stands as the unit word "cores" where it does not; the
    /// pod's line says its CPU in the quantity's own unit and takes the singular for exactly one core. Exactly one
    /// core is the one quantity the two readings word apart - every other quantity takes the word <c>CoresUnitOf</c>
    /// would give all the same.
    /// </summary>
    [Theory]
    [InlineData(0.0, "0 cores")]
    [InlineData(500, "0.5 cores")]
    [InlineData(1000, "1 cores")]
    [InlineData(1500, "1.5 cores")]
    [InlineData(2000, "2 cores")]
    [InlineData(12500, "12.5 cores")]
    [InlineData(-1000, "-1 cores")]
    [InlineData(double.NaN, "NaN cores")]
    [InlineData(double.PositiveInfinity, "Infinity cores")]
    [InlineData(double.NegativeInfinity, "-Infinity cores")]
    public void TheNodesPlainCpuSaysThePluralWordForEveryQuantity(double millicores, string expected) =>
        Assert.Equal(expected, ClusterText.NodeCpu(new ResourceUse(millicores, null, null)));

    /// <summary>The pod's members read the limit and not what the node offers; the node's read what it offers and not the limit.</summary>
    [Fact]
    public void ThePodReadsItsLimitAndTheNodeWhatItOffers()
    {
        var use = new ResourceUse(500, null, 1000, 4000);
        Assert.Equal("500 m of 1 core", ClusterText.CpuUse(use));
        Assert.Equal("0.5 of 4 cores", ClusterText.NodeCpu(use));

        var memory = new ResourceUse(Mebibyte, null, 2 * Mebibyte, Gibibyte);
        Assert.Equal("1 of 2 MiB", ClusterText.MemoryUse(memory));
        Assert.Equal("1 MiB of 1 GiB", ClusterText.NodeMemory(memory));
    }

    /// <summary>Each unit starts at one of it, and the byte below is said in the unit beneath; below a KiB and
    /// what is none of the units are said in KiB.</summary>
    [Theory]
    [InlineData(Gibibyte, "1 GiB")]
    [InlineData(Gibibyte - 1, "1024 MiB")]
    [InlineData(Mebibyte, "1 MiB")]
    [InlineData(Mebibyte - 1, "1024 KiB")]
    [InlineData(Kibibyte, "1 KiB")]
    [InlineData(Kibibyte - 1, "1 KiB")]
    [InlineData(0.0, "0 KiB")]
    [InlineData(-Kibibyte, "-1 KiB")]
    [InlineData(double.NaN, "NaN KiB")]
    [InlineData(double.PositiveInfinity, "Infinity GiB")]
    [InlineData(double.NegativeInfinity, "-Infinity KiB")]
    public void EachMemoryUnitStartsAtOneOfIt(double bytes, string expected) =>
        Assert.Equal(expected, ClusterText.Memory(bytes));

    /// <summary>Only the GiB keeps a digit after the point; the MiB and the KiB are whole.</summary>
    [Fact]
    public void OnlyTheGibibyteKeepsADigit()
    {
        Assert.Equal("1.5 GiB", ClusterText.Memory(1.5 * Gibibyte));
        Assert.Equal("2 MiB", ClusterText.Memory(1.5 * Mebibyte));
        Assert.Equal("2 KiB", ClusterText.Memory(1.5 * Kibibyte));
    }
}
