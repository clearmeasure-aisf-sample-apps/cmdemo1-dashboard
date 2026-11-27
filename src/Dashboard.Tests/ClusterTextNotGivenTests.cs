namespace Dashboard.Tests;

/// <summary>
/// Pins <see cref="ClusterText.NotGiven"/>: the em dash a cluster table shows in a cell the cluster file
/// does not fill, one character, the exact code point.
/// </summary>
public class ClusterTextNotGivenTests
{
    /// <summary>The value is the em dash U+2014.</summary>
    [Fact]
    public void NotGivenIsTheEmDash() => Assert.Equal("—", ClusterText.NotGiven);

    /// <summary>It is exactly one character: the em dash, code point 0x2014.</summary>
    [Fact]
    public void NotGivenIsASingleCharacter()
    {
        Assert.Equal(1, ClusterText.NotGiven.Length);
        Assert.Equal('\u2014', ClusterText.NotGiven[0]);
    }
}
