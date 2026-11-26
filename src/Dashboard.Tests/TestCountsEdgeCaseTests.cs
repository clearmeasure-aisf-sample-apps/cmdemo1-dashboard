namespace Dashboard.Tests;

/// <summary>
/// <see cref="TestCounts" /> is the build's test run in numbers: the unit, the integration and the acceptance
/// counts, each one said when the build measured it, and the total the three make. <see cref="BuildInfoTests" />
/// pins them read from the build endpoint's answer, and <see cref="DeployableFactsTests" /> pins the total two of
/// the three make, in the words the card says them in; the same counts, one of them only, are pinned where the
/// dashboard's own build reports unit tests and nothing else.
/// <para>
/// What is pinned here is the edge each of those sits next to, and only that: the counts that are not said and the
/// total that comes out of none of them; the counts at the ends of the count, and the total they make without
/// spilling over; the parts the record keeps and the order it says them in, with a number left out of the words;
/// the two counts that are one while the parts are, a said count left unsaid making another; the falling apart and
/// the copy; and the counts that are not there.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every count is one the test brings.
/// </para>
/// </summary>
public class TestCountsEdgeCaseTests
{
    // ----- the record's parts and its words

    /// <summary>The counts keep the very parts they are given, each in the place it is given in, and say them in
    /// the order the record declares them: the unit first, the integration next, the acceptance last, and the total
    /// the three make said of its own, last of all.</summary>
    [Fact]
    public void TheCountsKeepThePartsTheyAreGivenAndSayThemInTheirOrder()
    {
        var counts = new TestCounts(4102, 312, 90);

        Assert.Equal(4102, counts.Unit);
        Assert.Equal(312, counts.Integration);
        Assert.Equal(90, counts.Acceptance);
        Assert.Equal("TestCounts { Unit = 4102, Integration = 312, Acceptance = 90, Total = 4504 }", counts.ToString());
    }

    /// <summary>A count said at the end of the count is the end in the record and in its words: the top in the unit
    /// and the bottom in the acceptance, with the integration left out of the count and of the words alike.</summary>
    [Fact]
    public void TheCountsAtTheEndsOfTheCountSayTheEnds()
    {
        var counts = new TestCounts(int.MaxValue, null, int.MinValue);

        Assert.Equal(int.MaxValue, counts.Unit);
        Assert.Null(counts.Integration);
        Assert.Equal(int.MinValue, counts.Acceptance);
        Assert.Equal("TestCounts { Unit = 2147483647, Integration = , Acceptance = -2147483648, Total = -1 }", counts.ToString());
    }

    /// <summary>The counts that name nothing are still counts: every part left unsaid, the total the sum no count
    /// reported, and the words with nothing in them. The record's default, like no counts at all, is no counts.</summary>
    [Fact]
    public void CountsThatNameNothingSayNoCountAndTheDefaultIsNoCounts()
    {
        var counts = new TestCounts(null, null, null);

        Assert.Null(counts.Unit);
        Assert.Null(counts.Integration);
        Assert.Null(counts.Acceptance);
        Assert.Equal(0, counts.Total);
        Assert.Equal("TestCounts { Unit = , Integration = , Acceptance = , Total = 0 }", counts.ToString());
        Assert.Null(default(TestCounts));
    }

    // ----- the total

    /// <summary>The total is the sum of the counts that are said: every one of them, the two of them, the one of
    /// them, and - when none is said - the zero that counts nothing. A count of zero is a count, and it adds nothing
    /// and leaves nothing out of the question.</summary>
    [Theory]
    [InlineData(4102, 312, 90, 4504)]
    [InlineData(4102, null, 90, 4192)]
    [InlineData(4102, 312, null, 4414)]
    [InlineData(4102, null, null, 4102)]
    [InlineData(null, 312, 90, 402)]
    [InlineData(null, 312, null, 312)]
    [InlineData(null, null, 90, 90)]
    [InlineData(0, 0, 0, 0)]
    [InlineData(null, null, null, 0)]
    public void TheTotalIsTheSumOfTheCountsThatAreSaid(int? unit, int? integration, int? acceptance, int total) =>
        Assert.Equal(total, new TestCounts(unit, integration, acceptance).Total);

    /// <summary>The total stands at the ends of the count the counts stand in, wherever the end is said: the top in
    /// any of the three places is the top of the total, the bottom the bottom, and the top and the bottom together
    /// the one between them - the ends meeting, and never one past the other.</summary>
    [Theory]
    [InlineData(int.MaxValue, null, null, int.MaxValue)]
    [InlineData(null, int.MaxValue, null, int.MaxValue)]
    [InlineData(null, null, int.MaxValue, int.MaxValue)]
    [InlineData(int.MinValue, null, null, int.MinValue)]
    [InlineData(null, null, int.MinValue, int.MinValue)]
    [InlineData(int.MaxValue, int.MinValue, 0, -1)]
    [InlineData(int.MaxValue, 0, int.MinValue, -1)]
    [InlineData(0, int.MaxValue, int.MinValue, -1)]
    [InlineData(int.MaxValue, int.MinValue, int.MaxValue, 2147483646)]
    public void TheTotalStandsAtTheEndsOfTheCountWithoutSpilling(int? unit, int? integration, int? acceptance, int total) =>
        Assert.Equal(total, new TestCounts(unit, integration, acceptance).Total);

    /// <summary>
    /// The total the parts cannot make is pinned on purpose, the way <see cref="TestCounts" />'s doc says it is
    /// decided: added in the <c>int</c> the parts are held in, past the top the total comes back around - the top
    /// and one more is the bottom, the top twice over is <c>-2</c>, and the bottom twice over is nothing again.
    /// </summary>
    [Theory]
    [InlineData(int.MaxValue, 1, null, int.MinValue)]
    [InlineData(int.MaxValue, int.MaxValue, null, -2)]
    [InlineData(null, int.MaxValue, int.MaxValue, -2)]
    [InlineData(int.MinValue, int.MinValue, null, 0)]
    [InlineData(int.MinValue, null, int.MinValue, 0)]
    public void TheTotalPastTheTopComesBackAroundOnPurpose(int? unit, int? integration, int? acceptance, int total) =>
        Assert.Equal(total, new TestCounts(unit, integration, acceptance).Total);

    // ----- the record itself

    /// <summary>Two counts are one while their three parts are - each an integer's own equality - and a difference
    /// in any one of them, a count turned, or a count that was said left unsaid, is another one. The hash code
    /// follows the equality, and neither no counts nor a word is a count's equal.</summary>
    [Fact]
    public void TwoCountsAreOneWhileTheirPartsAre()
    {
        var one = new TestCounts(4102, 312, 90);
        var two = new TestCounts(4102, 312, 90);

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.True(one.Equals((object)two));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.False(one.Equals("TestCounts"));

        Assert.NotEqual(one, new TestCounts(4101, 312, 90));
        Assert.NotEqual(one, new TestCounts(4102, 311, 90));
        Assert.NotEqual(one, new TestCounts(4102, 312, 91));
        Assert.NotEqual(one, new TestCounts(null, 312, 90));
        Assert.NotEqual(one, new TestCounts(4102, null, 90));
        Assert.NotEqual(one, new TestCounts(4102, 312, null));
        Assert.NotEqual(one, new TestCounts(null, null, null));
        Assert.NotEqual(new TestCounts(null, null, null), new TestCounts(0, 0, 0));
    }

    /// <summary>The counts fall apart into the three parts they hold - the unsaid parts falling apart as unsaid -
    /// and a copy of the counts is the counts again: the <c>with</c> that names no part and the <c>with</c> that
    /// names the part it turns keep the rest, and not the same reference.</summary>
    [Fact]
    public void TheCountsFallApartIntoTheirPartsAndACopyIsTheCountsAgain()
    {
        var counts = new TestCounts(4102, 312, 90);

        var (unit, integration, acceptance) = counts;
        Assert.Equal(4102, unit);
        Assert.Equal(312, integration);
        Assert.Equal(90, acceptance);

        var (first, second, third) = new TestCounts(null, 1, null);
        Assert.Null(first);
        Assert.Equal(1, second);
        Assert.Null(third);

        var copy = counts with { };
        Assert.Equal(counts, copy);
        Assert.NotSame(counts, copy);

        var turned = counts with { Acceptance = 0 };
        Assert.Equal((4414, 4102, 312), (turned.Total, turned.Unit, turned.Integration));
        Assert.NotEqual(counts, turned);

        var repack = counts with { Unit = counts.Unit, Integration = counts.Integration, Acceptance = counts.Acceptance };
        Assert.Equal(counts, repack);
        Assert.NotSame(counts, repack);
    }

    /// <summary>Counts that are not there are no counts: the record's default is no counts, and counts are never
    /// equal to no counts however the question is asked - the operators, and <c>Equals</c> asked about the no
    /// counts - while the counts that are there are counts to both of them.</summary>
    [Fact]
    public void CountsThatAreNotThereAreNoCounts()
    {
        Assert.Null(default(TestCounts));

        TestCounts? none = null;
        var there = new TestCounts(0, 0, 0);

        Assert.False(none == there);
        Assert.True(none != there);
        Assert.False(there.Equals(none));
        Assert.False(there == none);
        Assert.True(there != none);
        Assert.True(there is not null);
    }
}
