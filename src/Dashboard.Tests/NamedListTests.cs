namespace Dashboard.Tests;

/// <summary>
/// <see cref="NamedList" /> is the one place a headline cuts a list after its first few names and joins what is left
/// as a sentence does - the rotation headline of <see cref="ServingAssessment" /> and the Resource Health tooltip
/// both use it, each with its own number and its own words around the list.
/// </summary>
public class NamedListTests
{
    [Theory]
    [InlineData(new string[0], "")]
    [InlineData(new[] { "a" }, "a")]
    [InlineData(new[] { "a", "b" }, "a and b")]
    [InlineData(new[] { "a", "b", "c" }, "a, b and c")]
    [InlineData(new[] { "a", "b", "c", "d" }, "a, b, c and d")]
    public void JoinWritesNamesAsASentence(string[] names, string expected) =>
        Assert.Equal(expected, NamedList.Join(names));

    [Theory]
    [InlineData(new[] { "a" }, 3, "a")]
    [InlineData(new[] { "a", "b", "c" }, 3, "a, b and c")]
    [InlineData(new[] { "a", "b", "c", "d" }, 3, "a, b, c and 1 more")]
    [InlineData(new[] { "a", "b", "c", "d", "e" }, 3, "a, b, c and 2 more")]
    [InlineData(new[] { "a", "b" }, 1, "a and 1 more")]
    public void WordsNamesTheFirstFewThenCountsTheRest(string[] names, int most, string expected) =>
        Assert.Equal(expected, NamedList.Words(names, most));

    [Fact]
    public void NamedKeepsTheFirstInOrder() =>
        Assert.Equal(["a", "b", "c"], NamedList.Named(["a", "b", "c", "d", "e"], 3));

    [Fact]
    public void NamedReadsNoMoreThanItNames()
    {
        var read = 0;
        var named = NamedList.Named(Enumerable.Range(0, 100).Select(index => { read++; return index.ToString(System.Globalization.CultureInfo.InvariantCulture); }), 8);
        Assert.Equal(8, named.Count);
        Assert.Equal(8, read);
    }

    [Theory]
    [InlineData(12, 3, "9 more")]
    [InlineData(9, 8, "1 more")]
    [InlineData(8, 8, null)]
    [InlineData(0, 3, null)]
    public void MoreCountsWhatIsLeftOrIsNull(int count, int most, string? expected) =>
        Assert.Equal(expected, NamedList.More(count, most));

    [Fact]
    public void TwoNamesJoinWithTheAndLeadAndNoComma() =>
        Assert.Equal("a and b", NamedList.Join(["a", "b"]));

    [Fact]
    public void ThreeNamesPutTheCommaOnlyBetweenTheLeadingNames() =>
        Assert.DoesNotContain(", and", NamedList.Join(["a", "b", "c"]));

    [Fact]
    public void ThreeNamesJoinWithTheCommaSeparatorAndTheAndLead() =>
        Assert.Equal("a, b and c", NamedList.Join(["a", "b", "c"]));

    [Fact]
    public void WordsWithARemainderNamesTheFirstTwoThenCountsTheRest() =>
        Assert.Equal("a, b and 2 more", NamedList.Words(["a", "b", "c", "d"], 2));

    [Fact]
    public void ANegativeMostIsRefused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => NamedList.Named(["a"], -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => NamedList.More(1, -1));
    }
}
