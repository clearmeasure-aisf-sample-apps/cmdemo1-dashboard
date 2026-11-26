using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="CostEstimate" /> is an environment's estimated part of what the environments share: a share of 0 to 1
/// that the file's publisher may or may not say, and the three sums that part carries - the last complete UTC day,
/// the seven days that end with it, and the month so far up to it. The estimate is the one part of an entry the
/// entry may do without, and where it stands it stands as a share and its amounts, not as a sum.
/// <para>
/// What the file makes of it is already pinned where the reading happens: <see cref="CostReportEdgeCaseTests" />
/// says that the share stands on its two ends and none past them - the whole and the nothing, with the hair past
/// them no share - and that the estimate with no amount, with no part at all, or of a kind that is no object is no
/// estimate; and <see cref="CostTests" /> pins the same record as the <c>tdd</c> estimate of the file the optics
/// read, while <see cref="CostTests" /> and <see cref="EdgeCaseTextTests" /> pin the words the page says of it,
/// which is where the share becomes a percent and the month without an amount no words at all. <c>Read</c> is
/// internal to an app that declares no <c>InternalsVisibleTo</c>, so it is pinned through
/// <see cref="CostReport.Parse" /> there rather than here.
/// </para>
/// <para>
/// What follows is the record itself, which all of that walks past: the two parts read by the names of their own
/// properties rather than as one whole compared with another, the share at every end of the number it stands on and
/// at the nothing that is not a zero, the amounts as the very reference the record was given and the missing
/// reference kept as it is; a part turned as the record is made and the copies
/// that turn one part while they keep the other; the two parts coming back out in the order they went in; and the
/// public surface, so that the day a third part is added the list below says so. Then the members every assertion
/// above leans on without ever saying a word about them: the equality of the typed record, of the boxed object and
/// of <c>==</c> and <c>!=</c>, the hash that follows them, the null on either side of each, the share that is no
/// number at all being one record with itself and nothing else, and the one share written two ways - the zero and
/// the zero below it - being one record with two sets of words. Then the words the record says itself, which follow
/// whoever reads them, unlike the page's.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: the record holds a share and three sums, no
/// moment and no address, and calls nothing at all - every value below is one the test brought. The member that
/// would need a <c>now</c> is <see cref="CostText.Estimate" />, and it is pinned on the words it says, not on the
/// record it reads.
/// </para>
/// </summary>
public class CostEstimateEdgeCaseTests
{
    /// <summary>The estimate the shipped sample file gives to <c>tdd</c>: a share below the whole and the three sums
    /// that part carries - a share, a week and a month, each another number, so that a part landing on another part's
    /// number is seen rather than quietly matching it.</summary>
    private static CostEstimate Full => new(0.2048, new CostAmounts(1.17, 2.69, 2.69));

    // ------------------------------------------- the two parts, by the names of their own properties

    /// <summary>The constructor's two arguments, position by position, come back as the property of the same name:
    /// the share first, the sums that part carries last - the order the file names them in and the order the page
    /// says them. The sample's own estimate stands at the front, and beside it the whole and the half of a share and
    /// the sums they are not.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var amounts = new CostAmounts(1.17, 2.69, 2.69);
        var tdd = new CostEstimate(0.2048, amounts);

        Assert.Equal(0.2048, tdd.Share);
        Assert.Same(amounts, tdd.Amounts);

        // The whole of a share is a share the record stands on, and a week of sums is the week the record was given.
        var whole = new CostEstimate(1, new CostAmounts(0, 0, 13.44));

        Assert.Equal(1d, whole.Share);
        Assert.Equal(new CostAmounts(0, 0, 13.44), whole.Amounts);

        var half = new CostEstimate(0.5, new CostAmounts(null, 1.5, null));

        Assert.Equal(0.5, half.Share);
        Assert.Equal(new CostAmounts(null, 1.5, null), half.Amounts);
    }

    /// <summary>The share is the number it was given, at the ends the number runs to: the zero and the whole the
    /// reading may stand on, the hair past them the reading refuses, a share below the zero, the smallest number
    /// above zero, the largest there are and the least, the two infinities, and a month's bill turned to a share.
    /// The record adds nothing to a share, rounds nothing, caps nothing and refuses nothing - it is the reading of
    /// the file, pinned beside it, that decides what a share may stand for.</summary>
    [Theory]
    [InlineData(0d)]
    [InlineData(1d)]
    [InlineData(0.5d)]
    [InlineData(0.005d)]
    [InlineData(-0.5d)]
    [InlineData(1.0000001d)]
    [InlineData(double.Epsilon)]
    [InlineData(double.MaxValue)]
    [InlineData(double.MinValue)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(double.NaN)]
    public void TheShareStandsAtTheEndsTheNumberRunsTo(double share)
    {
        var estimate = new CostEstimate(share, new CostAmounts(0, 0, 0));

        Assert.Equal(share, estimate.Share);
    }

    /// <summary>The share the file never gave stands unsaid, which is not the share standing at zero: the record
    /// whose share is nothing is a record and not no record, and the sums beside the unsaid share are the sums they
    /// were given. It is what lets the page say the part's amount without the percent the file did not write.</summary>
    [Fact]
    public void AShareThatIsNotGivenStandsUnsaidAndNotAtZero()
    {
        var unsaid = new CostEstimate(null, new CostAmounts(1.17, 2.69, 2.69));

        Assert.NotNull(unsaid);
        Assert.Null(unsaid.Share);
        Assert.Equal(new CostAmounts(1.17, 2.69, 2.69), unsaid.Amounts);

        // Zero is a share the file gave; the nothing is the share that is not there.
        Assert.NotEqual(new CostEstimate(0, new CostAmounts(1.17, 2.69, 2.69)), unsaid);
        Assert.Null(default(CostEstimate));
    }

    /// <summary>The amounts come back as the very amounts the record was given - the reference, not a copy -
    /// whatever the sums they hold, down to the three sums that are none, and the reference that is none is kept as
    /// the missing reference it is: the record checks nothing and refuses nothing, so it is the caller, and the
    /// reading that builds the record, that decides what a missing reference may stand for.</summary>
    [Fact]
    public void TheAmountsComeBackAsTheyWereGivenAndTheMissingReferenceIsKeptAsItIs()
    {
        var amounts = new CostAmounts(1.17, 2.69, 2.69);
        var estimate = new CostEstimate(0.2048, amounts);

        Assert.Same(amounts, estimate.Amounts);
        Assert.Equal(amounts, estimate.Amounts);

        var none = new CostAmounts(null, null, null);

        Assert.Same(none, new CostEstimate(null, none).Amounts);

        // The record does no guard of its own: the missing reference stands where it was given, beside the share it is.
        var missing = new CostEstimate(0.5, null!);

        Assert.Equal(0.5, missing.Share);
        Assert.Null(missing.Amounts);
    }

    // ------------------------------------------------------------- a part turned, and the copy

    /// <summary>Each part may be turned as the record is made - the share to another number, to the whole and to the
    /// nothing, and the sums to another three - and stands where it is turned to, while the part the turn does not
    /// name stays the number it was given: a turned record is another record and never the same reference.</summary>
    [Fact]
    public void APartTurnedAsTheRecordIsMadeStandsWhereItIsTurnedTo()
    {
        var original = new CostEstimate(0.2, new CostAmounts(1.5, 2.5, 2.6));

        var share = original with { Share = 0.8 };

        Assert.Equal(0.8, share.Share);
        Assert.Equal(new CostAmounts(1.5, 2.5, 2.6), share.Amounts);

        var unsaid = original with { Share = null };

        Assert.Null(unsaid.Share);
        Assert.Equal(new CostAmounts(1.5, 2.5, 2.6), unsaid.Amounts);

        var sums = original with { Amounts = new CostAmounts(0, 0, 0.4) };

        Assert.Equal(0.2, sums.Share);
        Assert.Equal(new CostAmounts(0, 0, 0.4), sums.Amounts);

        Assert.NotEqual(original, share);
        Assert.NotEqual(original, unsaid);
        Assert.NotEqual(original, sums);
    }

    /// <summary>The copy that turns nothing is the same two parts in a new record, and the copy that turns one part
    /// keeps the other: the original stands as it stood while the copy carries the turned part, the copy that names
    /// both parts as they stand is the record it started from without being it, and the sums the copy keeps are the
    /// very sums the record was given.</summary>
    [Fact]
    public void ACopyChangesThePartItNamesAndKeepsTheOther()
    {
        var original = Full;

        var unchanged = original with { };

        Assert.Equal(original, unchanged);
        Assert.NotSame(original, unchanged);
        Assert.Same(original.Amounts, unchanged.Amounts);

        var share = original with { Share = 0.5 };

        Assert.Equal(0.2048, original.Share);
        Assert.Equal(0.5, share.Share);
        Assert.Same(original.Amounts, share.Amounts);
        Assert.NotEqual(original, share);
        Assert.NotSame(original, share);

        var sums = original with { Amounts = new CostAmounts(0, 0, 0.4) };

        Assert.Equal(0.2048, sums.Share);
        Assert.Equal(new CostAmounts(0, 0, 0.4), sums.Amounts);
        Assert.NotEqual(original, sums);

        var repacked = original with { Share = 0.2048, Amounts = new CostAmounts(1.17, 2.69, 2.69) };

        Assert.Equal(original, repacked);
        Assert.NotSame(original, repacked);

        var fromNothing = new CostEstimate(null, new CostAmounts(null, null, null)) with { Share = 1 };

        Assert.Equal(1d, fromNothing.Share);
        Assert.Equal(new CostAmounts(null, null, null), fromNothing.Amounts);
    }

    /// <summary>The copies made with the <c>with</c> that name one part by its own name turn that part: the share named
    /// keeps the sums the record held, and the sums named keep the share the record held, in both cases the kept part
    /// as the very reference the record carried - and the copy that names both parts together is the record of the
    /// two it named.</summary>
    [Fact]
    public void TheCopyThatNamesOnePartIsTheCopyItNames()
    {
        var estimate = Full;

        var byShare = estimate with { Share = 1 };

        Assert.Equal(1d, byShare.Share);
        Assert.Same(estimate.Amounts, byShare.Amounts);
        Assert.NotSame(estimate, byShare);

        var bySums = estimate with { Amounts = new CostAmounts(0, 0, 0.4) };

        Assert.Equal(0.2048, bySums.Share);
        Assert.Equal(new CostAmounts(0, 0, 0.4), bySums.Amounts);
        Assert.NotSame(estimate, bySums);

        var both = estimate with { Share = 0.5, Amounts = new CostAmounts(1, 2, 3) };

        Assert.Equal(new CostEstimate(0.5, new CostAmounts(1, 2, 3)), both);
        Assert.NotSame(estimate, both);
    }

    // ----------------------------------------------------- the record falling apart, and its own shape

    /// <summary>The two parts come back out in the order they went in - the share first, the sums last - and the
    /// share that is nothing comes out as nothing rather than as the zero it is not; what came out goes back in the
    /// same way, so the record the two parts fell apart into is the record they were.</summary>
    [Fact]
    public void TheTwoPartsComeOutAgainInTheOrderTheyWentIn()
    {
        var (share, sums) = Full;

        Assert.Equal(0.2048, share);
        Assert.Equal(new CostAmounts(1.17, 2.69, 2.69), sums);
        Assert.Equal(Full, new CostEstimate(share, sums));

        var (noShare, noSums) = new CostEstimate(null, new CostAmounts(0, 0, 0));

        Assert.Null(noShare);
        Assert.Equal(new CostAmounts(0, 0, 0), noSums);
        Assert.Equal(new CostEstimate(null, new CostAmounts(0, 0, 0)), new CostEstimate(noShare, noSums));
    }

    /// <summary>The record's whole public surface, as it stands: the two parts named in the order the file names
    /// them, and the members every assertion in this file leans on - falling apart, comparing, hashing and saying
    /// itself - and nothing else to call. A third part added to the record has to be written here as well, which is
    /// the point: the page cannot be shown a number nobody decided what it means.</summary>
    [Fact]
    public void TheRecordHasTwoPartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Share", "Amounts"],
            [.. typeof(CostEstimate)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["Amounts", "Deconstruct", "Equals", "GetHashCode", "Share", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());

        Assert.All(
            typeof(CostEstimate).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            property => Assert.True(
                property.PropertyType == typeof(double?) || property.PropertyType == typeof(CostAmounts),
                $"{property.Name} is not a part the estimate carries"));
    }

    // --------------------------------------------------------------- the words the record says itself

    /// <summary>The words the record says its two parts in: its own name at the front, the share as the plain number
    /// it stands on - the zero as <c>0</c>, the whole as <c>1</c>, a share below the zero with its sign, the smallest
    /// number above zero in the digits that round-trip it, the largest there are no differently, the endless as
    /// <c>Infinity</c> and the number that is none as <c>NaN</c> - and the sums in the words their own record says
    /// them, the sum nobody said as the empty place between the mark and the comma. These are the record's words and
    /// not the page's: no dash, no percent, no currency.</summary>
    [Fact]
    public void TheRecordSaysItsTwoPartsInTheOrderTheyAreNamed()
    {
        Assert.Equal("CostEstimate { Share = 0.2048, Amounts = CostAmounts { Yesterday = 1.17, Last7Days = 2.69, MonthToDate = 2.69 } }", Words(Full));
        Assert.Equal("CostEstimate { Share = , Amounts = CostAmounts { Yesterday = 1.17, Last7Days = 2.69, MonthToDate = 2.69 } }", Words(new CostEstimate(null, new CostAmounts(1.17, 2.69, 2.69))));
        Assert.Equal("CostEstimate { Share = 0, Amounts = CostAmounts { Yesterday = 0, Last7Days = 0, MonthToDate = 0 } }", Words(new CostEstimate(0, new CostAmounts(0, 0, 0))));
        Assert.Equal("CostEstimate { Share = 1, Amounts = CostAmounts { Yesterday = , Last7Days = , MonthToDate =  } }", Words(new CostEstimate(1, new CostAmounts(null, null, null))));
        Assert.Equal("CostEstimate { Share = -0.5, Amounts = CostAmounts { Yesterday = -0.3, Last7Days = , MonthToDate = 22.1 } }", Words(new CostEstimate(-0.5, new CostAmounts(-0.3, null, 22.1))));
        Assert.Equal("CostEstimate { Share = 5E-324, Amounts = CostAmounts { Yesterday = 1E+300, Last7Days = 1.7976931348623157E+308, MonthToDate = NaN } }", Words(new CostEstimate(double.Epsilon, new CostAmounts(1e300, double.MaxValue, double.NaN))));
        Assert.Equal("CostEstimate { Share = NaN, Amounts = CostAmounts { Yesterday = Infinity, Last7Days = -Infinity, MonthToDate =  } }", Words(new CostEstimate(double.NaN, new CostAmounts(double.PositiveInfinity, double.NegativeInfinity, null))));

        // The words are no measure of the record: the share that is unsaid and the share of nothing are said apart,
        // and no word of the page is in them.
        Assert.NotEqual(Words(new CostEstimate(0, new CostAmounts(0, 0, 0))), Words(new CostEstimate(null, new CostAmounts(0, 0, 0))));
        Assert.DoesNotContain("—", Words(new CostEstimate(null, new CostAmounts(null, null, null))), StringComparison.Ordinal);
        Assert.DoesNotContain("$", Words(Full), StringComparison.Ordinal);
        Assert.DoesNotContain("%", Words(Full), StringComparison.Ordinal);
    }

    /// <summary>The numbers of these words follow whoever reads them, which is what sets the record's own words
    /// apart from the page's: a reader of German is told <c>0,2048</c> by the record and <c>20 %</c> by
    /// <see cref="CostText.Estimate" />, whose words are written for the dashboard and not for whoever looks.</summary>
    [Fact]
    public void TheRecordsOwnWordsFollowTheReaderWhileThePagesWordsDoNot()
    {
        var estimate = Full;

        ViewerCulture.German(() =>
        {
            Assert.Equal("CostEstimate { Share = 0,2048, Amounts = CostAmounts { Yesterday = 1,17, Last7Days = 2,69, MonthToDate = 2,69 } }", estimate.ToString());
            Assert.Equal(
                "plus about $2.69 this month of what the environments share (20 % of what all pods request)",
                CostText.Estimate(estimate, new CostReport(null, "USD", new DateOnly(2026, 10, 3), null, []), new DateTimeOffset(2026, 10, 4, 22, 0, 0, TimeSpan.Zero)));
        });

        Assert.Equal("CostEstimate { Share = 0.2048, Amounts = CostAmounts { Yesterday = 1.17, Last7Days = 2.69, MonthToDate = 2.69 } }", Words(estimate));
    }

    // ------------------------------------------------------------------- the equality, the hash, the two

    /// <summary>Two records are one while their two parts are - the nothing among them included - whichever of the
    /// four ways of asking is used: the record's own <c>Equals</c>, the one of <see cref="object" />, <c>==</c> and
    /// <c>!=</c>. The hash follows the equality, boxed or unboxed and from one call to the next, so the records that
    /// are one are one entry of a set and the estimate a file's reading hands over is interchangeable with the same
    /// share and sums written out by hand.</summary>
    [Fact]
    public void TwoEstimatesAreOneWhileTheirTwoPartsAre()
    {
        var one = Full;
        var two = new CostEstimate(0.2048, new CostAmounts(1.17, 2.69, 2.69));

        Assert.True(one.Equals(two));
        Assert.True(one.Equals(one));
        Assert.True(one.Equals((object)two));
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.Equal(one, two);
        Assert.NotSame(one, two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());

        var unsaid = new CostEstimate(null, new CostAmounts(0, 0, 0));
        var alsoUnsaid = new CostEstimate(null, new CostAmounts(0, 0, 0));

        Assert.Equal(unsaid, alsoUnsaid);
        Assert.True(unsaid == alsoUnsaid);
        Assert.False(unsaid != alsoUnsaid);
        Assert.Equal(unsaid.GetHashCode(), alsoUnsaid.GetHashCode());

        // The copies that turn nothing and the records that are one: two of them, and no more.
        Assert.Equal(2, new HashSet<CostEstimate> { one, two, unsaid, alsoUnsaid, one with { } }.Count);

        var read = CostReport.Parse(
            """{ "environments": [ { "name": "tdd", "estimate": { "share": 0.2048, "yesterday": 1.17, "last7Days": 2.69, "monthToDate": 2.69 } } ] }""")!;
        var estimate = read.Find("tdd")!.Estimate!;

        Assert.Equal(one, estimate);
        Assert.True(one == estimate);
        Assert.Equal(one.GetHashCode(), estimate.GetHashCode());
    }

    /// <summary>A difference in either part is another record: the share turned by a hair of a percent, the share
    /// turned to the zero an unsaid share is not, a share said left out and a share unsaid filled in, the sums turned
    /// in one of their three parts, and the share the reading refuses past its ends standing beside the same sums -
    /// which is the record saying another thing, not the same thing again. A set of the records that differ in one
    /// way each holds them all apart.</summary>
    [Fact]
    public void APartTurnedOrTurnedAboutIsAnotherRecord()
    {
        var one = Full;

        Assert.NotEqual(one, new CostEstimate(0.2049, new CostAmounts(1.17, 2.69, 2.69)));
        Assert.NotEqual(one, new CostEstimate(0.2048, new CostAmounts(1.18, 2.69, 2.69)));
        Assert.NotEqual(one, new CostEstimate(null, new CostAmounts(1.17, 2.69, 2.69)));
        Assert.NotEqual(one, new CostEstimate(0, new CostAmounts(1.17, 2.69, 2.69)));
        Assert.NotEqual(one, new CostEstimate(0.2048, new CostAmounts(null, null, null)));
        Assert.NotEqual(one, new CostEstimate(0.2048, new CostAmounts(1.17, 2.69, 2.69) with { MonthToDate = null }));
        Assert.NotEqual(one, new CostEstimate(1.17, new CostAmounts(1.17, 2.69, 2.69)));

        Assert.True(one != new CostEstimate(0.5, new CostAmounts(1.17, 2.69, 2.69)));
        Assert.False(one == new CostEstimate(0.5, new CostAmounts(1.17, 2.69, 2.69)));
        Assert.False(one.Equals((object)(new CostEstimate(0.2048, new CostAmounts(1.17, 2.69, 2.69)) with { Share = null })));

        // The two parts are two places: turning either of them alone is another record, and all five stand apart.
        Assert.Equal(
            5,
            new HashSet<CostEstimate>
            {
                one,
                one with { Share = null },
                one with { Share = 0 },
                one with { Amounts = new CostAmounts(1.17, 2.69, 2.69) with { Last7Days = null } },
                new CostEstimate(1.17, new CostAmounts(1.17, 2.69, 2.69)),
            }.Count);
    }

    /// <summary>The share that is no number at all: a share said as not-a-number is one record with itself, hashed
    /// with itself, and another record than the one at zero and than every record but its own - the record compares
    /// its share where the number would not, and the two records the comparison says are one are one entry of a
    /// set.</summary>
    [Fact]
    public void AShareThatIsNoNumberIsOneWithItselfAndNothingElse()
    {
        var missing = new CostEstimate(double.NaN, new CostAmounts(0, 0, 0));
        var also = new CostEstimate(double.NaN, new CostAmounts(0, 0, 0));

        Assert.True(missing.Equals(also));
        Assert.True(missing == also);
        Assert.False(missing != also);
        Assert.Equal(missing.GetHashCode(), also.GetHashCode());
        Assert.Single(new HashSet<CostEstimate> { missing, also });

        Assert.NotEqual(missing, Full);
        Assert.NotEqual(missing, new CostEstimate(0, new CostAmounts(0, 0, 0)));
        Assert.False(missing.Equals(Full));

        // The record compares the share where the number would not: the two shares are not the same number, though
        // they are the same part of the same record.
        Assert.False(missing.Share == also.Share);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two records that are no record
    /// at all are one, a record and no record are two - the equality and the inequality each saying it, from either
    /// side - and the record asked about itself says no to the nothing, to a word it is not, to a bare number, to
    /// the anonymous thing that happens to carry its two parts and to a cost record of another kind that holds the
    /// very sums one of them carries.</summary>
    [Fact]
    public void NoRecordIsOneWithNoRecordAndNotWithARecord()
    {
        CostEstimate? nothing = null;
        CostEstimate? alsoNothing = null;
        CostEstimate? there = Full;

        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == nothing);
        Assert.True(there != nothing);
        Assert.False(there!.Equals(nothing));
        Assert.True(there.Equals(there));
        Assert.Null(default(CostEstimate));

        Assert.False(Full.Equals(null));
        Assert.False(Full.Equals((CostEstimate?)null));
        Assert.False(Full.Equals((object?)null));
        Assert.False(Full.Equals("CostEstimate { Share = 0.2048, Amounts = CostAmounts { Yesterday = 1.17, Last7Days = 2.69, MonthToDate = 2.69 } }"));
        Assert.False(Full.Equals((object)0.2048));
        Assert.False(Full.Equals(new { Share = 0.2048, Amounts = new CostAmounts(1.17, 2.69, 2.69) }));
        Assert.False(Full.Equals(new CostAmounts(1.17, 2.69, 2.69)));
        Assert.False(Full.Equals(new CostService("tdd", 1.17)));
    }

    /// <summary>The one share written two ways is one record said two ways: the zero and the zero below it are the
    /// same two parts, equal with one hash, and the words that say them are not the same words - so the words are no
    /// measure of the record. A share below the zero stands apart from the share it is not and from the nothing a
    /// share the file did not give.</summary>
    [Fact]
    public void OneShareWrittenTwoWaysIsOneRecordSaidTwoWays()
    {
        var sums = new CostAmounts(1.5, 2.5, 2.6);
        var zero = new CostEstimate(0, sums);
        var signed = new CostEstimate(-0d, sums);

        Assert.Equal(zero, signed);
        Assert.True(zero == signed);
        Assert.False(zero != signed);
        Assert.Equal(zero.GetHashCode(), signed.GetHashCode());
        Assert.Single(new HashSet<CostEstimate> { zero, signed });
        Assert.NotEqual(Words(zero), Words(signed));
        Assert.Equal("CostEstimate { Share = 0, Amounts = CostAmounts { Yesterday = 1.5, Last7Days = 2.5, MonthToDate = 2.6 } }", Words(zero));
        Assert.Equal("CostEstimate { Share = -0, Amounts = CostAmounts { Yesterday = 1.5, Last7Days = 2.5, MonthToDate = 2.6 } }", Words(signed));

        var below = new CostEstimate(-0.5, sums);

        Assert.NotEqual(below, zero);
        Assert.NotEqual(below, new CostEstimate(0.5, sums));
        Assert.NotEqual(below, new CostEstimate(null, sums));
    }

    // ------------------------------------------------------------------------- the machinery

    /// <summary>The public members the record declares itself: its properties and the methods that are not the get
    /// and the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes, and the falling apart that gives the parts back. The constructor
    /// is no method, and the copies a caller makes with the <c>with</c> are no members at all, so neither is named by
    /// this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(CostEstimate)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words a record says its two parts in, taken in the invariant culture whatever the reader of
    /// this test happens to be: the record's own words follow whoever reads them, which is pinned on its own further
    /// up this file, so the strings there are the point and the culture only the way to say them once.</summary>
    private static string Words(CostEstimate estimate) => ViewerCulture.Invariant(() => estimate.ToString()!);
}
