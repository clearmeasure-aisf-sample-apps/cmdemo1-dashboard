namespace Dashboard.Tests;

/// <summary>
/// The unit of a CPU quantity agrees with the quantity in millicores, not with a count of one: only exactly one core
/// (1000 m) is a <c>core</c>; nothing, half a core and one and a half cores are all <c>cores</c>. Against a whole, the
/// unit follows the whole, not the used number. These pin the rule so it is not "simplified" into the plural of a count.
/// </summary>
public class ClusterTextCoresUnitTests
{
    [Theory]
    [InlineData(1000, "1 core")]
    [InlineData(1500, "1.5 cores")]
    [InlineData(2000, "2 cores")]
    [InlineData(12500, "12.5 cores")]
    public void APodsCpuFromOneCoreOnIsSingularOnlyForExactlyOneCore(double millicores, string expected)
    {
        Assert.Equal(expected, ClusterText.Cpu(millicores));
    }

    [Theory]
    [InlineData(0, 1000, "0 of 1 core")]
    [InlineData(500, 1000, "0.5 of 1 core")]
    [InlineData(1000, 1000, "1 of 1 core")]
    [InlineData(1000, 500, "1 of 0.5 cores")]
    [InlineData(1000, 1500, "1 of 1.5 cores")]
    [InlineData(1000, 0, "1 of 0 cores")]
    public void AgainstAWholeTheUnitFollowsTheWholeNotTheUsedNumber(double used, double of, string expected)
    {
        Assert.Equal(expected, ClusterText.CoresOf(used, of));
    }

    [Fact]
    public void APodAgainstALimitOfOneCoreSaysTheSingularOnTheLimitSide()
    {
        Assert.Equal("1.5 of 1 core", ClusterText.CpuOf(1500, 1000));
        Assert.Equal("1 core of 500 m", ClusterText.CpuOf(1000, 500));
        Assert.Equal("1.5 cores of 500 m", ClusterText.CpuOf(1500, 500));
    }
}
