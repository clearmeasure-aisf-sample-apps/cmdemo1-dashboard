namespace Dashboard.Tests;

/// <summary>
/// <see cref="Plural" /> writes a count and the noun it counts as one phrase: <c>1 restart</c>, <c>2 restarts</c>.
/// A count of exactly one takes the singular and every other count - zero, two, the ends of the number - takes the
/// plural, which by default is the singular with an <c>s</c> on it; a noun whose plural is not made that way is
/// given to the overload that takes both words.
/// <para>
/// The two ways a count is held are written two ways, on purpose: the small one (<see cref="int" />), which is what
/// a page counts of things it can see, is written digit by digit with no separator at all - the same way
/// <see cref="ClusterText.Restarts(int)" /> writes it (see <c>ClusterTextEdgeCaseTests</c>, which pins
/// <c>2147483647 restarts</c>); the long one, which is what a table of a database reports, is grouped in thousands
/// the way the diagram groups its row counts (see <c>DatabaseViewTests</c>).
/// </para>
/// <para>
/// The words are the dashboard's own: neither the noun nor the separator changes with whoever looks, whatever the
/// viewer's culture writes for a thousands separator or a plural.
/// </para>
/// </summary>
public class PluralTests
{
    /// <summary>One is the only singular: zero counts as many, two counts as many, and the noun in between them is
    /// the same noun every time.</summary>
    [Theory]
    [InlineData(0, "0 restarts")]
    [InlineData(1, "1 restart")]
    [InlineData(2, "2 restarts")]
    public void OnlyACountOfOneTakesTheSingularNoun(int count, string expected) =>
        Assert.Equal(expected, Plural.Of(count, "restart"));

    /// <summary>The plural a noun gets when nobody says otherwise is the singular with an <c>s</c> on it, at every
    /// count that is not one.</summary>
    [Theory]
    [InlineData(0, "node", "0 nodes")]
    [InlineData(2, "node", "2 nodes")]
    [InlineData(11, "pod", "11 pods")]
    [InlineData(999, "check", "999 checks")]
    public void TheDefaultPluralIsTheNounWithAnSOnIt(int count, string noun, string expected) =>
        Assert.Equal(expected, Plural.Of(count, noun));

    /// <summary>A noun whose plural is not its singular plus an <c>s</c> is said by the overload that is given both
    /// words: the singular on the one, the given plural on every other count.</summary>
    [Theory]
    [InlineData(0, "0 assemblies")]
    [InlineData(1, "1 assembly")]
    [InlineData(2, "2 assemblies")]
    [InlineData(1234567, "1234567 assemblies")]
    public void AnIrregularNounTakesThePluralItWasGiven(int count, string expected) =>
        Assert.Equal(expected, Plural.Of(count, "assembly", "assemblies"));

    /// <summary>The long count is grouped in thousands, the way the diagram writes the rows of a table: none below a
    /// thousand, one comma from a thousand up, and the one that is exactly one stays singular all the same.</summary>
    [Theory]
    [InlineData(1L, "1 row")]
    [InlineData(999L, "999 rows")]
    [InlineData(1234L, "1,234 rows")]
    [InlineData(12345678L, "12,345,678 rows")]
    public void ALongCountIsGroupedByThousands(long count, string expected) =>
        Assert.Equal(expected, Plural.Of(count, "row"));

    /// <summary>The small count is not grouped at all: the same number that the long overload writes with a comma is
    /// written digit by digit here, out to the ends of the number - which is how
    /// <see cref="ClusterText.Restarts(int)" /> says restarts, and this agrees with it.</summary>
    [Theory]
    [InlineData(1234, "1234 restarts")]
    [InlineData(1234567, "1234567 restarts")]
    [InlineData(int.MaxValue, "2147483647 restarts")]
    public void AnIntCountKeepsEveryDigitAndTakesNoSeparator(int count, string expected)
    {
        Assert.Equal(expected, Plural.Of(count, "restart"));
        Assert.Equal(expected, ClusterText.Restarts(count));
    }

    /// <summary>The words are the dashboard's and not the viewer's: under a culture that writes a thousands
    /// separator as a point, both the noun chosen and the separator of the long count stay as they were written.</summary>
    [Fact]
    public void TheWordsAndTheSeparatorDoNotChangeWithTheViewersCulture() =>
        ViewerCulture.German(() =>
        {
            Assert.Equal("1 restart", Plural.Of(1, "restart"));
            Assert.Equal("2 restarts", Plural.Of(2, "restart"));
            Assert.Equal("2 assemblies", Plural.Of(2, "assembly", "assemblies"));
            Assert.Equal("1,234 rows", Plural.Of(1234L, "row"));
            Assert.Equal("1234567 restarts", Plural.Of(1234567, "restart"));
        });

    // ----- The noun alone -----

    /// <summary>The noun asked on its own follows the same rule as the phrase, and writes no digit: only a count of
    /// exactly one is singular, while zero, two and the ends of the number are plural.</summary>
    [Theory]
    [InlineData(0, "nodes")]
    [InlineData(1, "node")]
    [InlineData(2, "nodes")]
    [InlineData(1234, "nodes")]
    public void TheNounAloneTakesTheSingularForACountOfOneOnly(int count, string expected) =>
        Assert.Equal(expected, Plural.Noun(count, "node"));

    /// <summary>The noun alone is given the irregular plural too: the singular on the one, the given plural on every
    /// other count, and never the singular plus an <c>s</c> that was declined.</summary>
    [Theory]
    [InlineData(0, "assemblies")]
    [InlineData(1, "assembly")]
    [InlineData(2, "assemblies")]
    public void TheNounAloneTakesTheIrregularPluralItWasGiven(int count, string expected) =>
        Assert.Equal(expected, Plural.Noun(count, "assembly", "assemblies"));

    /// <summary>The phrase and the noun alone are the one rule and cannot drift apart: writing the count in front of
    /// the noun alone is always what <see cref="Plural.Of(int, string, string?)" /> writes, grouped or not, out to the
    /// end of the number.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(1234)]
    [InlineData(int.MaxValue)]
    public void ThePhraseIsItsCountAndItsNounAlone(int count)
    {
        Assert.Equal(FormattableString.Invariant($"{count} {Plural.Noun(count, "restart")}"), Plural.Of(count, "restart"));
        Assert.Equal(FormattableString.Invariant($"{count:#,0} {Plural.Noun(count, "restart")}"), Plural.Of((long)count, "restart"));
    }

    /// <summary>The noun alone is the dashboard's word and not the viewer's: a reader of German is given the noun as
    /// it was written, singular for the one and with the <c>s</c> on every other count.</summary>
    [Fact]
    public void TheNounAloneDoesNotChangeWithTheViewersCulture() =>
        ViewerCulture.German(() =>
        {
            Assert.Equal("node", Plural.Noun(1, "node"));
            Assert.Equal("nodes", Plural.Noun(0, "node"));
            Assert.Equal("nodes", Plural.Noun(2, "node"));
            Assert.Equal("assemblies", Plural.Noun(2, "assembly", "assemblies"));
        });

    /// <summary>The sentences of the header take their noun from this rule rather than from a second copy of it: the
    /// words each level says for a total of one are singular and the words for any other total are plural, the empty
    /// sentence included - which for a total of one says "No node in the topology".</summary>
    [Theory]
    [InlineData(1, "The only node is healthy", "1 of 1 node not healthy", "Checking 1 node", "No node in the topology")]
    [InlineData(4, "All 4 nodes healthy", "4 of 4 nodes not healthy", "Checking 4 nodes", "No nodes in the topology")]
    public void TheSummarySentencesTakeTheirNounFromThisRule(int total, string allHealthy, string problems, string pending, string empty)
    {
        Assert.Equal(allHealthy, SummaryWords.Text(SummaryLevel.AllHealthy, total, total, 0));
        Assert.Equal(problems, SummaryWords.Text(SummaryLevel.Problems, total, 0, 0));
        Assert.Equal(pending, SummaryWords.Text(SummaryLevel.Pending, total, 0, total));
        Assert.Equal(empty, SummaryWords.Text(SummaryLevel.Empty, total, total, 0));
    }
}
