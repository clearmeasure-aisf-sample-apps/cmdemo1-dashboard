using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="CrapFacts" /> is the build's CRAP score in three numbers: the worst score the build measured, the
/// threshold it measured it against, and the count of the methods that went over it, each said when the build
/// measured it and left unsaid when it did not. The record's public members - the constructor and its three parts,
/// the <c>get</c> of each and the <c>init</c> that turns it as the record is made, the copy, the falling apart, the
/// words the record says itself in, the equality, the hash and the two operators - are pinned where they are used
/// and where they stand apart: <see cref="BuildInfoTests" /> and <see cref="CrapFactsReadEdgeCaseTests" /> pin them
/// read from a build endpoint's answer, <see cref="DeployableFactsTests" /> and
/// <see cref="HealthHelperCoverageTests" /> pin the words the card says of them, and
/// <see cref="CrapFactsEdgeCaseTests" /> pins the record itself at the numbers a build would report - 28.5, 30, 0,
/// 41, 3, the nothing of each part, the ends of the number and the count, the order the parts are said in and the
/// records that are one or two.
/// <para>
/// What is pinned here is only what those leave open, and only at the boundary: the three parts turned as the record
/// is made (the <c>init</c> the record is built with, which no other test turns) and the copy that turns a part to
/// the endless while leaving the record it turned from as it was; the words the record says of the numbers that are
/// no ordinary number - the endless, the not-a-number, the negative zero and the one that is written in power form;
/// the words following whoever reads them while the card's words do not; the parts falling apart at the ends and
/// being put back into the record they fell from; the endless and the not-a-number being one record with themselves
/// and another than every number they run past; the two numbers turned about being another record; and the record
/// not being the other record of the build that wears its three numbers.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every number is one the test brings, and
/// <see cref="CrapFacts" /> holds no moment and calls nothing.
/// </para>
/// </summary>
public class CrapFactsBoundaryEdgeCaseTests
{
    // ----- the parts turned as the record is made, and the copy at the ends

    /// <summary>A part turned as the record is made stands where it is turned to and the two the turn does not name
    /// stay as they were given: the worst turned to another score, the threshold turned to the nothing the zero is
    /// not, all three turned at once to the endless, the largest number and the least count - and the record built
    /// with its parts named in another order is the record the card reads, though no one reference.</summary>
    [Fact]
    public void APartTurnedAsTheRecordIsMadeStandsWhereItIsTurnedTo()
    {
        var said = new CrapFacts(28.5, 30, 0);

        var worst = new CrapFacts(28.5, 30, 0) { Max = 41 };

        Assert.Equal(41.0, worst.Max);
        Assert.Equal(30.0, worst.Threshold);
        Assert.Equal(0, worst.OverThreshold);
        Assert.NotEqual(said, worst);
        Assert.NotSame(said, worst);

        var thresholdless = new CrapFacts(28.5, 30, 0) { Threshold = null };

        Assert.Equal(28.5, thresholdless.Max);
        Assert.Null(thresholdless.Threshold);
        Assert.Equal(0, thresholdless.OverThreshold);

        var endless = new CrapFacts(null, null, null)
        {
            Max = double.PositiveInfinity,
            Threshold = double.MaxValue,
            OverThreshold = int.MinValue,
        };

        Assert.True(double.IsPositiveInfinity(endless.Max!.Value));
        Assert.Equal(double.MaxValue, endless.Threshold);
        Assert.Equal(int.MinValue, endless.OverThreshold);

        var named = new CrapFacts(null, 30, 0) { Max = 28.5 };

        Assert.Equal(said, named);
        Assert.NotSame(said, named);
    }

    /// <summary>The copy that turns a part to the endless or to the not-a-number is another record and leaves the
    /// record it was turned from exactly as it stood: the copy names the rest as they are, and the copy that names
    /// every part as it stands is the record again though no one reference.</summary>
    [Fact]
    public void ACopyThatTurnsAPartToTheEndlessLeavesTheRecordItTurnedFrom()
    {
        var facts = new CrapFacts(28.5, 30, 3);

        var notANumber = facts with { Max = double.NaN };

        Assert.True(double.IsNaN(notANumber.Max!.Value));
        Assert.Equal(30.0, notANumber.Threshold);
        Assert.Equal(3, notANumber.OverThreshold);
        Assert.NotEqual(facts, notANumber);

        var zeroed = facts with { Max = 0, OverThreshold = 0 };

        Assert.Equal(new CrapFacts(0, 30, 0), zeroed);

        // The record the copies were turned from never moved while they were.
        Assert.Equal(28.5, facts.Max);
        Assert.Equal(30.0, facts.Threshold);
        Assert.Equal(3, facts.OverThreshold);

        var repack = facts with { Max = facts.Max, Threshold = facts.Threshold, OverThreshold = facts.OverThreshold };

        Assert.Equal(facts, repack);
        Assert.NotSame(facts, repack);
    }

    // ----- the words the record says of the numbers that are no ordinary number

    /// <summary>The words the record says itself in, at the numbers no build would report and no other test says:
    /// the not-a-number as <c>NaN</c>, the endless as <c>Infinity</c> and <c>-Infinity</c>, the number too large for
    /// the digits as the power it rounds to, the least of the count with its sign - and each part nobody said as the
    /// empty place between the mark and the comma, which is a word of its own and not the word of the endless.</summary>
    [Fact]
    public void TheEndlessAndTheNotANumberAreSaidAsTheNumberSaysItself()
    {
        Assert.Equal("CrapFacts { Max = NaN, Threshold = , OverThreshold =  }", Words(new CrapFacts(double.NaN, null, null)));
        Assert.Equal(
            "CrapFacts { Max = Infinity, Threshold = -Infinity, OverThreshold = 0 }",
            Words(new CrapFacts(double.PositiveInfinity, double.NegativeInfinity, 0)));
        Assert.Equal(
            "CrapFacts { Max = 1E+300, Threshold = 30.25, OverThreshold = -2147483648 }",
            Words(new CrapFacts(1e300, 30.25, int.MinValue)));
        Assert.Equal("CrapFacts { Max = 0.5, Threshold = , OverThreshold =  }", Words(new CrapFacts(0.5, null, null)));

        // The endless, the not-a-number and the nothing are three words; the nothing is not said as either of the others.
        Assert.NotEqual(Words(new CrapFacts(double.NaN, null, null)), Words(new CrapFacts(null, null, null)));
        Assert.NotEqual(Words(new CrapFacts(double.PositiveInfinity, null, null)), Words(new CrapFacts(double.NaN, null, null)));
    }

    /// <summary>The zero and the negative zero are one record said two ways: equal as the numbers are equal, one
    /// hash, one place in the set - and yet the record says the sign it stands on, so the words are two. The same
    /// holds of the threshold alone, and of the nothing beside the zero, which the words and the record both say
    /// apart.</summary>
    [Fact]
    public void TheZeroAndTheNegativeZeroAreOneRecordSaidTwoWays()
    {
        var zero = new CrapFacts(0, 0, 0);
        var signed = new CrapFacts(-0.0, -0.0, 0);

        Assert.Equal(zero, signed);
        Assert.True(zero == signed);
        Assert.False(zero != signed);
        Assert.Equal(zero.GetHashCode(), signed.GetHashCode());
        Assert.Single(new HashSet<CrapFacts> { zero, signed });

        Assert.Equal("CrapFacts { Max = 0, Threshold = 0, OverThreshold = 0 }", Words(zero));
        Assert.Equal("CrapFacts { Max = -0, Threshold = -0, OverThreshold = 0 }", Words(signed));
        Assert.NotEqual(Words(zero), Words(signed));

        Assert.Equal(new CrapFacts(28.5, 0, null), new CrapFacts(28.5, -0.0, null));

        // The nothing of a part and the zero of it are two records, said apart.
        Assert.NotEqual(zero, new CrapFacts(null, null, null));
        Assert.NotEqual(Words(zero), Words(new CrapFacts(null, null, null)));
    }

    /// <summary>The words the record says itself in follow whoever reads them, which is what sets them apart from
    /// the words of the card: a viewer of German is told <c>28,5</c> by the record and <c>worst 28.5</c> by the
    /// page, whose line is written for the dashboard and not for whoever looks.</summary>
    [Fact]
    public void TheRecordsOwnWordsFollowTheReaderWhileTheCardsWordsDoNot()
    {
        var crap = new CrapFacts(28.5, 30, 0);

        ViewerCulture.German(() =>
        {
            Assert.Equal("CrapFacts { Max = 28,5, Threshold = 30, OverThreshold = 0 }", crap.ToString());
            Assert.Equal("worst 28.5, none over 30", BuildText.Crap(crap));
        });

        Assert.Equal("CrapFacts { Max = 28.5, Threshold = 30, OverThreshold = 0 }", Words(crap));
    }

    // ----- the parts the record falls apart into, at the ends

    /// <summary>The facts fall apart into the three parts they hold at the ends no answer brings them to - the
    /// endless score, the least number, the most of the count, the not-a-number and the nothing beside it, the
    /// negative zero and the zero beside it - and the parts put back in the order they fell out make the one record
    /// they fell from.</summary>
    [Fact]
    public void TheFactsFallApartIntoTheirPartsAtTheEndsAndArePutBackAsTheyWere()
    {
        var (max, threshold, over) = new CrapFacts(double.PositiveInfinity, double.MinValue, int.MaxValue);

        Assert.True(double.IsPositiveInfinity(max!.Value));
        Assert.Equal(double.MinValue, threshold);
        Assert.Equal(int.MaxValue, over);

        var (notANumber, noThreshold, noCount) = new CrapFacts(double.NaN, null, null);

        Assert.True(double.IsNaN(notANumber!.Value));
        Assert.Null(noThreshold);
        Assert.Null(noCount);

        var (negativeZero, zero, unsaid) = new CrapFacts(-0.0, 0.0, null);

        Assert.True(double.IsNegative(negativeZero!.Value));
        Assert.True(zero is { } theZero && !double.IsNegative(theZero));
        Assert.Null(unsaid);

        var facts = new CrapFacts(41, 30, 3);
        var (first, second, third) = facts;

        Assert.Equal(facts, new CrapFacts(first, second, third));
    }

    // ----- the records that are one, and the records that are two, at the ends

    /// <summary>The endless and the not-a-number are one with themselves - the equality of the record, of the
    /// object, of the operators, and the hash that follows - and another record than every number they run past:
    /// the endless than the largest number there is, the not-a-number than the zero it is nearest to in the words,
    /// either endless than the nothing and than the other endless. The set keeps one place for each record that is
    /// one and a place of its own for every record that is two.</summary>
    [Fact]
    public void TheEndlessAreOneWithThemselvesAndTwoToEveryNumberTheyRunPast()
    {
        var endless = new CrapFacts(double.PositiveInfinity, 30, 0);
        var alsoEndless = new CrapFacts(double.PositiveInfinity, 30, 0);

        Assert.Equal(endless, alsoEndless);
        Assert.True(endless == alsoEndless);
        Assert.False(endless != alsoEndless);
        Assert.True(endless.Equals((object)alsoEndless));
        Assert.Equal(endless.GetHashCode(), alsoEndless.GetHashCode());
        Assert.False(double.IsFinite(endless.Max!.Value));

        Assert.NotEqual(endless, new CrapFacts(double.MaxValue, 30, 0));
        Assert.NotEqual(endless, new CrapFacts(double.NegativeInfinity, 30, 0));
        Assert.NotEqual(endless, new CrapFacts(double.NaN, 30, 0));
        Assert.NotEqual(endless, new CrapFacts(null, 30, 0));
        Assert.NotEqual(new CrapFacts(double.NegativeInfinity, null, null), new CrapFacts(double.MinValue, null, null));
        Assert.NotEqual(new CrapFacts(double.NaN, null, null), new CrapFacts(0, null, null));
        Assert.True(new CrapFacts(double.NaN, null, null) != new CrapFacts(0, null, null));

        Assert.Equal(
            7,
            new HashSet<CrapFacts>
            {
                new(double.NaN, null, null),
                new(double.PositiveInfinity, null, null),
                new(double.NegativeInfinity, null, null),
                new(double.MaxValue, null, null),
                new(double.MinValue, null, null),
                new(0, null, null),
                new(null, null, null),
            }.Count);
    }

    /// <summary>The two numbers turned about are another record: the worst stood where the threshold stands and the
    /// threshold where the worst stands is a different thing said, though the same three numbers - the card says
    /// them apart, and the set keeps both of them.</summary>
    [Fact]
    public void TheTwoNumbersTurnedAboutAreAnotherRecord()
    {
        var worst = new CrapFacts(28.5, 30, 0);
        var turnedAbout = new CrapFacts(30, 28.5, 0);

        Assert.NotEqual(worst, turnedAbout);
        Assert.True(worst != turnedAbout);
        Assert.False(worst == turnedAbout);
        Assert.Equal(2, new HashSet<CrapFacts> { worst, turnedAbout }.Count);

        Assert.Equal(28.5, worst.Max);
        Assert.Equal(30.0, worst.Threshold);
        Assert.Equal(30.0, turnedAbout.Max);
        Assert.Equal(28.5, turnedAbout.Threshold);
    }

    /// <summary>The record is one with itself however the question is asked - as itself and as an object - and is no
    /// other record at all, though the other record of the build wears the very three numbers: the complexity and the
    /// coverage asked of it say no, and so do a number, a word and the nothing.</summary>
    [Fact]
    public void TheRecordIsNoOtherRecordWearingItsNumbers()
    {
        var crap = new CrapFacts(28.5, 30, 0);

        Assert.True(crap.Equals(crap));
        Assert.True(crap.Equals((object)crap));

        Assert.False(crap.Equals(new ComplexityFacts(28.5, 30, 0)));
        Assert.False(crap.Equals((object)new ComplexityFacts(28.5, 30, 0)));
        Assert.False(crap.Equals(new CoverageFacts(28.5, 30)));
        Assert.False(crap.Equals(28.5));
        Assert.False(crap.Equals("worst 28.5"));
        Assert.False(crap.Equals(null));

        // The other record of the build that wears the same numbers is not the facts, and the set says so.
        Assert.Equal(2, new HashSet<object> { crap, new ComplexityFacts(28.5, 30, 0) }.Count);
    }

    // ----- the machinery

    /// <summary>The words a record says its three parts in, taken in the invariant culture whatever the reader of
    /// this test happens to be: the record's own words follow whoever reads them, which is pinned on its own below,
    /// so the strings above are the point and the culture only the way to say them once.</summary>
    private static string Words(CrapFacts crap) => ViewerCulture.Invariant(() => crap.ToString()!);
}
