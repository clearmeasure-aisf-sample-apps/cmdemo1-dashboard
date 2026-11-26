namespace Dashboard.Tests;

/// <summary>
/// <see cref="AksMetrics" /> is the block of Azure Monitor numbers on the cluster card. What a file makes of it is
/// pinned in <see cref="ClusterParserTests" /> and <see cref="AksServiceEdgeCaseTests" />, and which one of the five
/// numbers is enough for <see cref="AksMetrics.HasAny" /> is pinned there too. What follows is the record itself: the
/// six parts in the order they are declared, the two API server parts a caller may leave out, the numbers that are
/// not numbers (<see cref="double.NaN" />, the infinities) and the extremes of the window, equality part by part,
/// <c>with</c>, deconstruction and the one line the record says about itself. Nothing here reaches the network or a
/// clock.
/// </summary>
public class AksMetricsEdgeCaseTests
{
    /// <summary>A block with every part set to a distinct value, so a part handed to the wrong property shows.</summary>
    private static AksMetrics Full => new(15, 21.4, 63, 18.2, 4.6, 31.5);

    /// <summary>The six parts, in the order the record declares them, each come back as the property of their name.</summary>
    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        var metrics = Full;

        Assert.Equal(15, metrics.WindowMinutes);
        Assert.Equal(21.4, metrics.NodeCpuPercent);
        Assert.Equal(63, metrics.NodeMemoryPercent);
        Assert.Equal(18.2, metrics.NodeDiskPercent);
        Assert.Equal(4.6, metrics.ApiServerCpuPercent);
        Assert.Equal(31.5, metrics.ApiServerMemoryPercent);
    }

    /// <summary>The API server's two numbers are optional: left out, they are null, and the block is the very block
    /// that says null for them out loud.</summary>
    [Fact]
    public void TheApiServerNumbersDefaultToNothing()
    {
        var metrics = new AksMetrics(15, 21.4, 63, 18.2);

        Assert.Null(metrics.ApiServerCpuPercent);
        Assert.Null(metrics.ApiServerMemoryPercent);
        Assert.Equal(new AksMetrics(15, 21.4, 63, 18.2, null, null), metrics);
        Assert.True(metrics.HasAny);
    }

    /// <summary>The block of nothing at all: every part null, and no number to draw.</summary>
    [Fact]
    public void ABlockOfNothingHasNoPartAndNoNumber()
    {
        var none = new AksMetrics(null, null, null, null);

        Assert.Null(none.WindowMinutes);
        Assert.Null(none.NodeCpuPercent);
        Assert.Null(none.NodeMemoryPercent);
        Assert.Null(none.NodeDiskPercent);
        Assert.Null(none.ApiServerCpuPercent);
        Assert.Null(none.ApiServerMemoryPercent);
        Assert.False(none.HasAny);
    }

    /// <summary>The window is never a number of the block, whatever its value - the extremes included.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void NoWindowIsEnoughForTheBlockToBeShown(int window)
    {
        var metrics = new AksMetrics(window, null, null, null, null, null);

        Assert.Equal(window, metrics.WindowMinutes);
        Assert.False(metrics.HasAny);
    }

    /// <summary><see cref="AksMetrics.HasAny" /> asks whether a number is there, not whether it is a sensible one: a
    /// value that is not a number, an infinity, the smallest and the largest double are each a number that is there,
    /// in the first place and in the last.</summary>
    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(double.Epsilon)]
    [InlineData(double.MaxValue)]
    [InlineData(double.MinValue)]
    [InlineData(-0.0)]
    public void AnyValueThatIsThereCountsAsANumber(double value)
    {
        var first = new AksMetrics(null, value, null, null, null, null);
        var last = new AksMetrics(null, null, null, null, null, value);

        Assert.True(first.HasAny);
        Assert.True(last.HasAny);
        Assert.Equal(value, first.NodeCpuPercent!.Value);
        Assert.Equal(value, last.ApiServerMemoryPercent!.Value);
    }

    /// <summary>Two blocks of the same six parts are one block and hash alike; each part on its own tells two apart,
    /// and a zero is not the nothing a null is.</summary>
    [Fact]
    public void TwoBlocksAreOneWhenEveryPartIs()
    {
        var one = Full;

        Assert.Equal(one, Full);
        Assert.True(one == Full);
        Assert.False(one != Full);
        Assert.Equal(one.GetHashCode(), Full.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.Equal(one, one with { WindowMinutes = 15 });

        AksMetrics[] different =
        [
            one with { WindowMinutes = null },
            one with { WindowMinutes = 16 },
            one with { NodeCpuPercent = null },
            one with { NodeMemoryPercent = null },
            one with { NodeDiskPercent = null },
            one with { ApiServerCpuPercent = null },
            one with { ApiServerMemoryPercent = null },
            one with { NodeCpuPercent = 21.5 },
        ];

        foreach (var other in different)
        {
            Assert.NotEqual(one, other);
            Assert.True(one != other);
        }

        Assert.NotEqual(new AksMetrics(0, 0, 0, 0, 0, 0), new AksMetrics(null, null, null, null, null, null));

        // A record compares doubles with double.Equals, so NaN is the same as NaN here, unlike under ==.
        Assert.Equal(new AksMetrics(null, double.NaN, null, null), new AksMetrics(null, double.NaN, null, null));
    }

    /// <summary><c>with</c> changes the part it names and leaves the five others, and the original, as they were.</summary>
    [Fact]
    public void WithChangesOnlyThePartItNames()
    {
        var one = Full;
        var changed = one with { ApiServerCpuPercent = null, NodeDiskPercent = 99.9 };

        Assert.Equal(new AksMetrics(15, 21.4, 63, 99.9, null, 31.5), changed);
        Assert.Equal(Full, one);
        Assert.True(changed.HasAny);
    }

    /// <summary>The record deconstructs into its six parts in the order they are declared.</summary>
    [Fact]
    public void TheBlockDeconstructsInTheOrderItIsDeclared()
    {
        var (window, nodeCpu, nodeMemory, nodeDisk, apiCpu, apiMemory) = Full;

        Assert.Equal(15, window);
        Assert.Equal(21.4, nodeCpu);
        Assert.Equal(63, nodeMemory);
        Assert.Equal(18.2, nodeDisk);
        Assert.Equal(4.6, apiCpu);
        Assert.Equal(31.5, apiMemory);
    }

    /// <summary>The one line the record says about itself names the type, then its parts in declared order, then
    /// <see cref="AksMetrics.HasAny" />; a part that is nothing says nothing.</summary>
    [Fact]
    public void TheRecordSaysWhatItIsInOneLineOfItsOwnParts()
    {
        var text = new AksMetrics(15, null, null, null, null, null).ToString();

        Assert.StartsWith("AksMetrics { ", text, StringComparison.Ordinal);
        Assert.Contains("WindowMinutes = 15", text, StringComparison.Ordinal);
        Assert.Contains("NodeCpuPercent = ,", text, StringComparison.Ordinal);
        Assert.Contains("HasAny = False", text, StringComparison.Ordinal);
        Assert.True(
            text.IndexOf("WindowMinutes =", StringComparison.Ordinal) < text.IndexOf("ApiServerMemoryPercent =", StringComparison.Ordinal),
            text);
        Assert.True(
            text.IndexOf("ApiServerMemoryPercent =", StringComparison.Ordinal) < text.IndexOf("HasAny =", StringComparison.Ordinal),
            text);
    }

    // ------------------------------------------------------------------ the record's own shape

    /// <summary>
    /// The whole public surface of <see cref="AksMetrics" /> as it stands: its six parts in the order the constructor
    /// names them, the one computed property the block makes of them (<see cref="AksMetrics.HasAny" />), and the
    /// members every record is asked - falling apart, comparing, hashing and saying itself - and nothing else. A
    /// seventh part, a second computed property, or a static helper added to the record without a test is refused
    /// here by name.
    /// </summary>
    [Fact]
    public void TheMetricsHaveSixPartsAndOneDecisionAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["WindowMinutes", "NodeCpuPercent", "NodeMemoryPercent", "NodeDiskPercent", "ApiServerCpuPercent", "ApiServerMemoryPercent", "HasAny"],
            RecordSurface.PropertyNamesOf<AksMetrics>());

        Assert.Equal(
            [
                "ApiServerCpuPercent", "ApiServerMemoryPercent", "Deconstruct", "Equals", "GetHashCode",
                "HasAny", "NodeCpuPercent", "NodeDiskPercent", "NodeMemoryPercent", "ToString", "WindowMinutes",
                "op_Equality", "op_Inequality",
            ],
            RecordSurface.Of<AksMetrics>());

        var parameters = RecordSurface.ParametersOf<AksMetrics>();
        Assert.Equal(
            ["WindowMinutes", "NodeCpuPercent", "NodeMemoryPercent", "NodeDiskPercent", "ApiServerCpuPercent", "ApiServerMemoryPercent"],
            [.. parameters.Select(p => p.Name!)]);

        // The first four are required, the last two optional with a default of null.
        Assert.Equal([true, true, true, true, true, true], RecordSurface.NullableParts<AksMetrics>());
        Assert.Equal([false, false, false, false, true, true], [.. parameters.Select(p => p.HasDefaultValue)]);
    }
}
