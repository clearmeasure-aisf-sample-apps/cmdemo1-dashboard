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
}
