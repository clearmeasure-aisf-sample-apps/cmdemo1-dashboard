namespace Dashboard.Tests;

/// <summary>
/// The CPU's number formats and unit words stand in the named constants of <see cref="ClusterText"/> - the whole-number
/// format and the unit of the millicores, the two-decimal format of the cores, and the words a quantity is said in -
/// and are said in the invariant culture whatever the reader's is. These tests pin the moved formats under a
/// non-invariant culture, through the shared culture scope, and the ends of the two-decimal cut of the cores.
/// </summary>
public class ClusterTextCpuWordsTests
{
    /// <summary>
    /// A pod's CPU below one core is a whole number of millicores, and a number of cores keeps the invariant point:
    /// the reader of a comma-point culture reads the same words the invariant culture says.
    /// </summary>
    [Fact]
    public void TheCpuOfAPodIsSaidInTheInvariantCultureWhateverTheReadersIs()
    {
        Assert.Equal("1.23 cores", CultureScope.Says("de-DE", () => ClusterText.Cpu(1234.5)));
        Assert.Equal("14 of 500 m", CultureScope.Says("de-DE", () => ClusterText.CpuOf(14.4, 500)));
        Assert.Equal("14 of 500 m", CultureScope.Says("fr-FR", () => ClusterText.CpuOf(14.4, 500)));
    }

    /// <summary>
    /// The same for the cores alone and for a quantity of cores against a whole: the formats the constants name are
    /// the invariant ones, and the thread's culture does not reach them.
    /// </summary>
    [Fact]
    public void TheCoresOfAQuantityAreSaidInTheInvariantCultureWhateverTheReadersIs()
    {
        Assert.Equal("1.23", CultureScope.Says("de-DE", () => ClusterText.Cores(1234.5)));
        Assert.Equal("0.81 of 3.86 cores", CultureScope.Says("fr-FR", () => ClusterText.CoresOf(812, 3860)));
    }

    /// <summary>
    /// The two-decimal cut of the cores: the third digit rounds the second up where it is a half or more, and a zero
    /// after the point is not said.
    /// </summary>
    [Theory]
    [InlineData(1235, "1.24")]
    [InlineData(1200, "1.2")]
    [InlineData(1204, "1.2")]
    public void TheCoresAreCutAtTwoDigits(double millicores, string cores) =>
        Assert.Equal(cores, ClusterText.Cores(millicores));
}
