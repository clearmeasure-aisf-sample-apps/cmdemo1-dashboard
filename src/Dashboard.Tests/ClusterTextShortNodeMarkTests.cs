namespace Dashboard.Tests;

/// <summary>
/// <see cref="Cluster.ClusterText.ShortNode" /> reads the mark from <see cref="Ellipsis.Mark" /> and names the kept
/// head, so a change to the mark or to the head rule is caught here.
/// </summary>
public class ClusterTextShortNodeMarkTests
{
    /// <summary>A shortened node name contains the mark exactly once, between the head and the last six characters.</summary>
    [Fact]
    public void TheShortenedNameContainsTheMarkExactlyOnceBetweenTheHeadAndTheLastSixCharacters()
    {
        var expected = "aks-" + Ellipsis.Mark + "000000";

        Assert.Equal(expected, ClusterText.ShortNode("aks-system-12148983-vmss000000"));
    }

    /// <summary>The mark in the shortened name is the one from <see cref="Ellipsis.Mark" />, not a literal.</summary>
    [Fact]
    public void TheShortenedNameUsesTheEllipsisMark()
    {
        var result = ClusterText.ShortNode("aks-system-12148983-vmss000000");

        Assert.Contains(Ellipsis.Mark, result, StringComparison.Ordinal);
        Assert.Equal(1, result.Count(c => c == Ellipsis.Mark[0]));
    }

    /// <summary>A name exactly head + mark + tail long is kept whole; one character longer is cut.</summary>
    [Fact]
    public void ANameExactlyHeadMarkTailLongIsKeptWholeAndOneCharacterLongerIsCut()
    {
        // head = "a-" (2), mark = 1, tail = 6, total = 9: "a-1234567" is kept whole.
        Assert.Equal("a-1234567", ClusterText.ShortNode("a-1234567"));

        // One character longer: head = "a-" (2), mark = 1, tail = 6, total = 10: "a-12345678" is cut.
        Assert.Equal("a-" + Ellipsis.Mark + "345678", ClusterText.ShortNode("a-12345678"));
    }
}
