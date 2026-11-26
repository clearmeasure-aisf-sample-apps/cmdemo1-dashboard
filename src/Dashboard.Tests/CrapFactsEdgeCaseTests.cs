namespace Dashboard.Tests;

/// <summary>
/// <see cref="CrapFacts" /> is the build's CRAP score in three numbers: the worst score the build measured, the
/// threshold the build measured it against, and the count of the methods the score put over it, each one said when
/// the build measured it. <see cref="BuildInfoTests" /> pins them read from the build endpoint's answer - the
/// sample's <c>28.5</c>, <c>30</c> and <c>0</c>, and no facts where the answer holds none of the parts - and it,
/// <see cref="DeployableFactsTests" /> and <see cref="HealthHelperCoverageTests" /> pin the words the card says of
/// them and whether the card marks the build; the same facts pinned here are the record's own.
/// <para>
/// What is pinned here is the edge each of those parts sits next to, and only that: the parts the record keeps and
/// the order it says them in, with a part left out of the parts and of the words alike; the numbers at the ends of
/// the number each stands in, at the zero that is a number and at the one that is the least of them; the parts that
/// are not said, the record that carries the nothing of every part and the no record that is not one; the falling
/// apart and the copy; and the parts that make two records one record or two, with the nothing and the no number
/// beside them.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every fact is one the test brings.
/// </para>
/// </summary>
public class CrapFactsEdgeCaseTests
{
    // ----- the record's parts

    /// <summary>The facts keep the very parts they are given, each in the place it is given in, and say them in
    /// the order the record declares them: the worst score first, the threshold next, the count of the methods over
    /// it last.</summary>
    [Fact]
    public void TheFactsKeepThePartsTheyAreGivenAndSayThemInTheirOrder()
    {
        var crap = new CrapFacts(28.5, 30, 0);

        Assert.Equal(28.5, crap.Max);
        Assert.Equal(30.0, crap.Threshold);
        Assert.Equal(0, crap.OverThreshold);
        Assert.Equal("CrapFacts { Max = 28.5, Threshold = 30, OverThreshold = 0 }", CultureScope.Words(crap));
    }

    /// <summary>The two numbers and the count stand where they are put, at the ends of the number each stands in
    /// and at the zero that is a number: the most and the least score a build can say, the one threshold and the
    /// none, the most methods a build can put over and the least it can name - the record is not the build, and
    /// keeps the numbers it is given, the sign and all, and the count it is given, past the most a build would
    /// report.</summary>
    [Theory]
    [InlineData(double.MinValue, double.MinValue, int.MinValue)]
    [InlineData(-1.0, 0.0, 0)]
    [InlineData(0.0, 0.0, 0)]
    [InlineData(1.0, 1.0, 1)]
    [InlineData(28.5, 30.0, 0)]
    [InlineData(41.0, 30.0, int.MaxValue)]
    [InlineData(double.MaxValue, double.MaxValue, int.MaxValue)]
    public void TheNumbersStandAtTheEndsOfTheNumberTheyRunTo(double? max, double? threshold, int? overThreshold)
    {
        var crap = new CrapFacts(max, threshold, overThreshold);

        Assert.Equal(max, crap.Max);
        Assert.Equal(threshold, crap.Threshold);
        Assert.Equal(overThreshold, crap.OverThreshold);
    }

    /// <summary>A part the record was not given stands the nothing it is, kept rather than made a zero: the worst
    /// score left unsaid, the threshold left unsaid, the count left unsaid - each with the two parts it does not
    /// leave standing as they are - and the facts with every part left unsaid are still facts: a record carrying
    /// the nothing of every part, and not no facts at all. The record's default, like no facts read from an
    /// answer, is no facts.</summary>
    [Fact]
    public void APartThatIsNotSaidStandsTheNothingItIsAndTheRecordIsNotTheNothing()
    {
        Assert.Null(new CrapFacts(null, 30, 0).Max);
        Assert.Equal(30.0, new CrapFacts(null, 30, 0).Threshold);
        Assert.Equal(0, new CrapFacts(null, 30, 0).OverThreshold);

        Assert.Equal(28.5, new CrapFacts(28.5, null, 0).Max);
        Assert.Null(new CrapFacts(28.5, null, 0).Threshold);
        Assert.Equal(0, new CrapFacts(28.5, null, 0).OverThreshold);

        Assert.Equal(28.5, new CrapFacts(28.5, 30, null).Max);
        Assert.Equal(30.0, new CrapFacts(28.5, 30, null).Threshold);
        Assert.Null(new CrapFacts(28.5, 30, null).OverThreshold);

        var none = new CrapFacts(null, null, null);
        Assert.Null(none.Max);
        Assert.Null(none.Threshold);
        Assert.Null(none.OverThreshold);
        Assert.NotNull(none);

        Assert.Null(default(CrapFacts));
    }

    // ----- the words the record says its parts in

    /// <summary>The words the record says its parts in: its own name at the front, the parts in the order they are
    /// named, each as the value it stands on - the score and the threshold said as the plain number they are, at
    /// the ends of the number and with no mark between the digits - and the nothing of a part said as the nothing
    /// of its word: the three records that carry a part or the nothing of it are said as three words of their
    /// own, and the record that carries the nothing of every part is said with the empty word of its own, not no
    /// words at all.</summary>
    [Fact]
    public void ARecordSaysItsPartsInTheWordsTheRecordSaysThem()
    {
        Assert.Equal("CrapFacts { Max = 28.5, Threshold = 30, OverThreshold = 0 }", CultureScope.Words(new CrapFacts(28.5, 30, 0)));
        Assert.Equal("CrapFacts { Max = 41, Threshold = 30, OverThreshold = 3 }", CultureScope.Words(new CrapFacts(41, 30, 3)));
        Assert.Equal("CrapFacts { Max = , Threshold = 30, OverThreshold = 0 }", CultureScope.Words(new CrapFacts(null, 30, 0)));
        Assert.Equal("CrapFacts { Max = 28.5, Threshold = , OverThreshold = 0 }", CultureScope.Words(new CrapFacts(28.5, null, 0)));
        Assert.Equal("CrapFacts { Max = 28.5, Threshold = 30, OverThreshold =  }", CultureScope.Words(new CrapFacts(28.5, 30, null)));
        Assert.Equal("CrapFacts { Max = , Threshold = , OverThreshold =  }", CultureScope.Words(new CrapFacts(null, null, null)));
        Assert.Equal(
            "CrapFacts { Max = 1.7976931348623157E+308, Threshold = -1.7976931348623157E+308, OverThreshold = 2147483647 }",
            CultureScope.Words(new CrapFacts(double.MaxValue, double.MinValue, int.MaxValue)));
        Assert.Equal("CrapFacts { Max = -1, Threshold = 0, OverThreshold = -2147483648 }", CultureScope.Words(new CrapFacts(-1, 0, int.MinValue)));
    }

    // ----- the parts the record falls apart into and the copies

    /// <summary>The facts fall apart into the three parts they hold, in the order they name them - the unsaid
    /// parts falling apart as unsaid - and a copy of the facts is the facts again: the copy that turns no part is
    /// the facts though not the same reference, and the copy that turns one of the three parts keeps the two it
    /// does not name - the score to another score and to the nothing, the threshold to the nothing the number is
    /// not, the count to the one and to the nothing the zero is not.</summary>
    [Fact]
    public void TheFactsFallApartIntoTheirPartsAndACopyIsTheFactsAgain()
    {
        var (max, threshold, over) = new CrapFacts(28.5, 30, 0);
        Assert.Equal(28.5, max);
        Assert.Equal(30.0, threshold);
        Assert.Equal(0, over);

        var (noMax, noThreshold, noOver) = new CrapFacts(null, null, null);
        Assert.Null(noMax);
        Assert.Null(noThreshold);
        Assert.Null(noOver);

        var facts = new CrapFacts(28.5, 30, 0);

        var copy = facts with { };
        Assert.Equal(facts, copy);
        Assert.NotSame(facts, copy);

        var turnedMax = facts with { Max = 41 };
        Assert.Equal(41.0, turnedMax.Max);
        Assert.Equal(30.0, turnedMax.Threshold);
        Assert.Equal(0, turnedMax.OverThreshold);
        Assert.NotEqual(facts, turnedMax);

        var maxless = facts with { Max = null };
        Assert.Null(maxless.Max);
        Assert.Equal(30.0, maxless.Threshold);
        Assert.Equal(0, maxless.OverThreshold);
        Assert.NotEqual(facts, maxless);

        var thresholdless = new CrapFacts(28.5, 30, null) with { Threshold = null };
        Assert.Equal(28.5, thresholdless.Max);
        Assert.Null(thresholdless.Threshold);
        Assert.Null(thresholdless.OverThreshold);

        var oneOver = new CrapFacts(28.5, 30, null) with { OverThreshold = 1 };
        Assert.Equal(28.5, oneOver.Max);
        Assert.Equal(30.0, oneOver.Threshold);
        Assert.Equal(1, oneOver.OverThreshold);
    }

    // ----- the records that are one

    /// <summary>Two facts are one while their three parts are - each a number's own equality, the nothing among
    /// them included - and the record that holds the nothing of every part is one with the other such record: the
    /// equality of the record, of the object and of the operators, the hash that follows, and the set that keeps
    /// one entry for the records that are one. The two scores that are no number at all are one too: the nothing
    /// the number is not is the nothing it is, when the record asks it.</summary>
    [Fact]
    public void TwoFactsAreOneWhileTheirPartsAre()
    {
        var one = new CrapFacts(28.5, 30, 0);
        var two = new CrapFacts(28.5, 30, 0);

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.True(one.Equals((object)two));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.False(one.Equals("CrapFacts"));
        Assert.NotSame(one, two);

        var none = new CrapFacts(null, null, null);
        var alsoNone = new CrapFacts(null, null, null);
        Assert.Equal(none, alsoNone);
        Assert.True(none == alsoNone);
        Assert.Equal(none.GetHashCode(), alsoNone.GetHashCode());

        var noScore = new CrapFacts(double.NaN, null, null);
        var alsoNoScore = new CrapFacts(double.NaN, null, null);
        Assert.Equal(noScore, alsoNoScore);
        Assert.True(noScore == alsoNoScore);

        Assert.Single(new HashSet<CrapFacts> { one, two, new CrapFacts(28.5, 30, 0) });
        Assert.Equal(2, new HashSet<CrapFacts> { one, new CrapFacts(28.5, 30, 1) }.Count);
    }

    /// <summary>Facts that are not there are no facts: the record's default is no facts, and no facts is one with
    /// no facts and not with the facts that are there - the operators each saying it - while the facts that are
    /// there are facts to both of them, and asked of as an object say no to the nothing and no to a word they are
    /// not.</summary>
    [Fact]
    public void FactsThatAreNotThereAreNoFacts()
    {
        Assert.Null(default(CrapFacts));

        CrapFacts? none = null;
        CrapFacts? alsoNone = null;
        var there = new CrapFacts(0, 0, 0);

        Assert.True(none == alsoNone);
        Assert.False(none != alsoNone);
        Assert.False(none == there);
        Assert.True(none != there);
        Assert.False(there.Equals(none));
        Assert.False(there == none);
        Assert.True(there != none);
        Assert.True(there is not null);

        Assert.False(there.Equals(null));
        Assert.False(there.Equals("worst 28.5"));
        Assert.False(there.Equals((object)30));
    }

    // ----- the records that are not one

    /// <summary>A difference in any one of the three parts is another record: the score turned by one and turned
    /// about in sign, the threshold turned by one, the count turned by one, turned to the zero the nothing is not
    /// and the nothing the zero is not, and the nothing a part was not given against the number it was - each
    /// another record, the operators saying it and the set keeping its entry.</summary>
    [Fact]
    public void APartTurnedIsAnotherRecord()
    {
        var one = new CrapFacts(28.5, 30, 0);

        Assert.NotEqual(one, new CrapFacts(28.4, 30, 0));
        Assert.NotEqual(one, new CrapFacts(28.6, 30, 0));
        Assert.NotEqual(one, new CrapFacts(-28.5, 30, 0));
        Assert.NotEqual(one, new CrapFacts(28.5, 29, 0));
        Assert.NotEqual(one, new CrapFacts(28.5, 31, 0));
        Assert.NotEqual(one, new CrapFacts(28.5, 30, 1));
        Assert.NotEqual(one, new CrapFacts(28.5, 30, -1));
        Assert.NotEqual(one, new CrapFacts(28.5, 30, null));
        Assert.NotEqual(one, new CrapFacts(null, 30, 0));
        Assert.NotEqual(one, new CrapFacts(28.5, null, 0));
        Assert.NotEqual(new CrapFacts(null, null, null), new CrapFacts(0, 0, 0));
        Assert.NotEqual(new CrapFacts(0, 0, 0), new CrapFacts(null, null, null));

        Assert.True(one != new CrapFacts(28.5, 30, 1));
        Assert.False(one == new CrapFacts(28.5, 30, 1));
        Assert.Equal(3, new HashSet<CrapFacts> { one, one with { OverThreshold = null }, one with { OverThreshold = 1 } }.Count);
    }
}
