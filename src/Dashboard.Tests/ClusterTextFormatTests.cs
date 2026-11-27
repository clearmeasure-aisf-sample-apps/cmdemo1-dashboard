namespace Dashboard.Tests;

/// <summary>
/// The boundaries the format constants of <see cref="ClusterText"/> govern: the byte that is one short of a GiB
/// still said as a whole number of MiB, the GiB itself said as the one digit of a GiB, the quarter of a GiB the
/// one-decimal cut rounds up, and the negative that rounds to no percent.
/// </summary>
public class ClusterTextFormatTests
{
    /// <summary>The byte one short of a GiB is still a whole number of MiB - the one-decimal format belongs to the
    /// GiB and no further down.</summary>
    [Fact]
    public void TheByteOneShortOfAGiBStaysInTheWholeMiB()
    {
        Assert.Equal("1023 MiB", ClusterText.Memory(1023 * 1024 * 1024));
    }

    /// <summary>The GiB itself is the first memory said with the one-decimal format: the whole number of it and
    /// the quarter of it the one-decimal cut rounds up, by the invariant culture.</summary>
    [Fact]
    public void TheGibibyteIsTheFirstMemorySaidInTheOneDecimalFormat()
    {
        Assert.Equal("1 GiB", ClusterText.Memory(1073741824));
        Assert.Equal("1.3 GiB", ClusterText.Memory(1.25 * 1073741824));
    }

    /// <summary>The negative that rounds to no percent is said as the no percent it rounds to: the whole-number
    /// percent keeps the negative zero of the rounding.</summary>
    [Fact]
    public void TheNegativeThatRoundsToNoPercentIsSaidAsTheNoPercentItRoundsTo()
    {
        Assert.Equal("-0 %", ClusterText.Percent(-0.4));
    }
}
