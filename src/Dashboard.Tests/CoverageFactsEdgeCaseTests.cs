using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="CoverageFacts" /> is the build's own measure of how far its tests reach: the share of the lines and the
/// share of the branches, each said when the build measured it and left unsaid when it did not. The two numbers are
/// pinned where the page reads them and says them: <see cref="BuildInfoTests" /> pins the pair a deployable's build
/// endpoint answers (<c>81.2</c> and <c>70.1</c>) and the pair the dashboard's own build reports (<c>75.6</c> and
/// <c>65.2</c>), the branch standing with no line beside it, the section that is a word rather than a section, and the
/// answer that measures nothing at all; <see cref="DeployableFactsTests" /> and <see cref="DashboardBuildTests" /> pin
/// the same pair on a deployable's card and the coverage a build that measures none leaves out; and
/// <see cref="HealthHelperCoverageTests" /> pins the card's line built out of the two percentages.
/// <para>
/// What is pinned here is the record behind those numbers, and only what they leave open: the two parts at the ends the
/// number runs to, at the zero that is a number and at the nothing that is not one, and kept as they are given even
/// when no reader of a build file would let them through; the part turned as the record is made and the part a copy
/// turns while it keeps the other; the two parts the record falls apart into; the words it says them in, in the
/// reader's own culture, which is what sets them apart from the card's; the equality, the hash and the operators that
/// make two records one record or two - one number written two ways being the one record with the two words; and the
/// coverage an answer reports as nothing, as one number only, as more than the whole, or as a number nobody means.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every share is one the test brings or one it reads
/// from an answer of its own making, and <see cref="CoverageFacts" /> holds no moment and calls nothing.
/// </para>
/// </summary>
public class CoverageFactsEdgeCaseTests
{
    // ----- the two parts the record keeps -----

    /// <summary>The record keeps the two parts it is given, each as the very number it stands on: the line share first
    /// and the branch share after it, a fraction kept to the last digit it was given and a whole number kept whole
    /// rather than rounded to the percent a viewer reads.</summary>
    [Fact]
    public void TheTwoPartsStandAsTheNumbersTheyAreGiven()
    {
        var reported = new CoverageFacts(81.2, 70.1);

        Assert.Equal(81.2, reported.LinePercent);
        Assert.Equal(70.1, reported.BranchPercent);

        var exact = new CoverageFacts(81.234567, 70.05);

        Assert.Equal(81.234567, exact.LinePercent);
        Assert.Equal(70.05, exact.BranchPercent);

        var whole = new CoverageFacts(70, 0.5);

        Assert.Equal(70d, whole.LinePercent);
        Assert.Equal(0.5, whole.BranchPercent);
    }

    /// <summary>The two parts stand where they are put, at the ends the number runs to and at the zero that is a
    /// number: the most a double holds and the least, the smallest share above zero and the whole of one, a share over
    /// the whole of it, and the negative no card would ever show - the record is not the reader of a build file, and it
    /// keeps the sign it is given.</summary>
    [Theory]
    [InlineData(0d, 0d)]
    [InlineData(0d, 100d)]
    [InlineData(100d, 0d)]
    [InlineData(120.5d, 0.001d)]
    [InlineData(-1d, -100d)]
    [InlineData(-0.5d, 99.999d)]
    [InlineData(double.Epsilon, double.MaxValue)]
    [InlineData(double.MinValue, double.MaxValue)]
    public void TheTwoPartsStandAtTheEndsTheNumberRunsToAndTheZeroThatIsANumber(double line, double branch)
    {
        var coverage = new CoverageFacts(line, branch);

        Assert.Equal(line, coverage.LinePercent);
        Assert.Equal(branch, coverage.BranchPercent);
    }

    /// <summary>Neither part said is the record saying zero of both: the two parts stand unsaid, the record that says
    /// nothing is a record and not no record, and one part said with the other left out is the pair it stands for -
    /// while the caller that has no record at all has the reference that is none.</summary>
    [Fact]
    public void CoverageThatSaysNeitherPartSaysUnsaidAndNotZero()
    {
        var unsaid = new CoverageFacts(null, null);

        Assert.Null(unsaid.LinePercent);
        Assert.Null(unsaid.BranchPercent);
        Assert.Equal(new CoverageFacts(null, null), unsaid);
        Assert.NotEqual(new CoverageFacts(0, 0), unsaid);

        var linesOnly = new CoverageFacts(42.5, null);

        Assert.Equal(42.5, linesOnly.LinePercent);
        Assert.Null(linesOnly.BranchPercent);

        var branchesOnly = new CoverageFacts(null, 33.75);

        Assert.Null(branchesOnly.LinePercent);
        Assert.Equal(33.75, branchesOnly.BranchPercent);

        Assert.NotEqual(linesOnly, branchesOnly);
        Assert.Null(default(CoverageFacts));
    }

    /// <summary>A share the reader of a build file would refuse is kept for all that, because the record is not where
    /// the refusing happens: the share beyond every number, the share that is no share, and the largest number there
    /// are are kept as they are given, and the refusal of the first two is said once, in the words of the answer.</summary>
    [Fact]
    public void AShareNoReaderWouldLetThroughIsStillKeptAsItIsGiven()
    {
        var beyond = new CoverageFacts(double.PositiveInfinity, double.NegativeInfinity);

        Assert.True(double.IsPositiveInfinity(beyond.LinePercent!.Value));
        Assert.True(double.IsNegativeInfinity(beyond.BranchPercent!.Value));

        var missing = new CoverageFacts(double.NaN, double.NaN);

        Assert.True(double.IsNaN(missing.LinePercent!.Value));
        Assert.Equal(new CoverageFacts(double.NaN, double.NaN), missing);
        Assert.Equal(new CoverageFacts(double.NaN, double.NaN).GetHashCode(), missing.GetHashCode());

        var ends = new CoverageFacts(double.MaxValue, double.MinValue);

        Assert.Equal(double.MaxValue, ends.LinePercent);
        Assert.Equal(double.MinValue, ends.BranchPercent);
    }

    // ----- the parts turned, and the copies -----

    /// <summary>Each part may be turned as the record is made, and stands where it is turned to - to another number, to
    /// the zero and to the nothing - while the part the turn does not name stays the number the record was given: the
    /// record turned being another record than the one it would have stood as, and never the one reference.</summary>
    [Fact]
    public void APartTurnedAsTheRecordIsMadeStandsWhereItIsTurnedTo()
    {
        var reported = new CoverageFacts(81.2, 70.1);

        var lines = new CoverageFacts(81.2, 70.1) { LinePercent = 90 };

        Assert.Equal(90d, lines.LinePercent);
        Assert.Equal(70.1, lines.BranchPercent);
        Assert.NotEqual(reported, lines);
        Assert.NotSame(reported, lines);

        var branches = new CoverageFacts(81.2, 70.1) { BranchPercent = null };

        Assert.Equal(81.2, branches.LinePercent);
        Assert.Null(branches.BranchPercent);
        Assert.Equal(new CoverageFacts(81.2, null), branches);

        var zeroed = new CoverageFacts(81.2, 70.1) { LinePercent = 0, BranchPercent = 0 };

        Assert.Equal(new CoverageFacts(0, 0), zeroed);

        var unsaid = new CoverageFacts(81.2, 70.1) { LinePercent = null, BranchPercent = null };

        Assert.Equal(new CoverageFacts(null, null), unsaid);
        Assert.NotEqual(reported, unsaid);
    }

    /// <summary>The copy that turns nothing is the record again though not the record itself, and the copy that turns
    /// one part keeps the other: the line turned alone, the branch turned to the nothing the zero is not, and the copy
    /// that names both parts as they stand - each a new reference, and the one that names them as they are being the
    /// record it started from.</summary>
    [Fact]
    public void ACopyThatTurnsOnePartKeepsTheOther()
    {
        var reported = new CoverageFacts(81.2, 70.1);

        var copy = reported with { };

        Assert.Equal(reported, copy);
        Assert.NotSame(reported, copy);

        var lines = reported with { LinePercent = 82 };

        Assert.Equal(82d, lines.LinePercent);
        Assert.Equal(70.1, lines.BranchPercent);
        Assert.NotEqual(reported, lines);

        var branches = reported with { BranchPercent = null };

        Assert.Equal(81.2, branches.LinePercent);
        Assert.Null(branches.BranchPercent);
        Assert.NotEqual(reported, branches);

        var repack = reported with { LinePercent = reported.LinePercent, BranchPercent = reported.BranchPercent };

        Assert.Equal(reported, repack);
        Assert.NotSame(reported, repack);
    }

    // ----- the parts the record falls apart into -----

    /// <summary>The record falls apart into its two parts, in the order it names them and each as the number it stands
    /// on: the pair of shares, the pair of nothings, and the nothing beside the zero that is a number.</summary>
    [Fact]
    public void TheRecordFallsApartIntoItsTwoParts()
    {
        var (lines, branches) = new CoverageFacts(81.2, 70.1);

        Assert.Equal(81.2, lines);
        Assert.Equal(70.1, branches);

        var (noLines, noBranches) = new CoverageFacts(null, null);

        Assert.Null(noLines);
        Assert.Null(noBranches);

        var (unsaid, zero) = new CoverageFacts(null, 0);

        Assert.Null(unsaid);
        Assert.Equal(0d, zero);
    }

    // ----- the words the record says its parts in -----

    /// <summary>The words the record says its two parts in: its own name at the front, the line share first and the
    /// branch share after it, each as the plain number it stands on - the whole number written without a point, the
    /// zero as <c>0</c>, the largest number there are in the digits that round-trip it - and the part nobody said as the
    /// empty place between the mark and the comma.</summary>
    [Fact]
    public void TheRecordSaysItsTwoPartsInTheOrderTheyAreNamed()
    {
        Assert.Equal("CoverageFacts { LinePercent = 81.2, BranchPercent = 70.1 }", Words(new CoverageFacts(81.2, 70.1)));
        Assert.Equal("CoverageFacts { LinePercent = , BranchPercent =  }", Words(new CoverageFacts(null, null)));
        Assert.Equal("CoverageFacts { LinePercent = 100, BranchPercent =  }", Words(new CoverageFacts(100, null)));
        Assert.Equal("CoverageFacts { LinePercent = 0, BranchPercent = 65.2 }", Words(new CoverageFacts(0, 65.2)));
        Assert.Equal("CoverageFacts { LinePercent = -1.5, BranchPercent = 0.5 }", Words(new CoverageFacts(-1.5, 0.5)));
        Assert.Equal("CoverageFacts { LinePercent = 81.234567, BranchPercent = 1E+300 }", Words(new CoverageFacts(81.234567, 1e300)));

        // The record that says a share of nothing and the record that says no share are two records, said apart.
        Assert.NotEqual(Words(new CoverageFacts(0, 0)), Words(new CoverageFacts(null, null)));
        Assert.DoesNotContain("1E+300", Words(new CoverageFacts(81.2, 70.1)), StringComparison.Ordinal);
    }

    /// <summary>The numbers of these words follow whoever reads them, which is what sets the record's own words apart
    /// from the card's: a viewer of German is told <c>81,2</c> by the record and <c>81.2 %</c> by the page, whose line
    /// is written for the dashboard and not for whoever looks.</summary>
    [Fact]
    public void TheRecordsOwnWordsFollowTheReaderWhileTheCardsWordsDoNot()
    {
        var coverage = new CoverageFacts(81.2, 70.1);

        ViewerCulture.German(() =>
        {
            Assert.Equal("CoverageFacts { LinePercent = 81,2, BranchPercent = 70,1 }", coverage.ToString());
            Assert.Equal("81.2 % of lines, 70.1 % of branches", BuildText.Coverage(coverage));
        });

        Assert.Equal("CoverageFacts { LinePercent = 81.2, BranchPercent = 70.1 }", Words(coverage));
    }

    // ----- the records that are one, and the records that are two -----

    /// <summary>Two records are one while their two parts are, each part as the number it stands on - the nothing among
    /// them included - and the two are not the one reference they are one with: the equality of the record, of the
    /// object and of the operators, the hash that follows them, and the set that keeps one place for the records that
    /// are one.</summary>
    [Fact]
    public void TwoRecordsAreOneWhileTheirTwoPartsAre()
    {
        var one = new CoverageFacts(81.2, 70.1);
        var two = new CoverageFacts(81.2, 70.1);

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.True(one.Equals(one));
        Assert.True(one.Equals((object)two));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.NotSame(one, two);

        Assert.Equal(new CoverageFacts(null, null), new CoverageFacts(null, null));
        Assert.Equal(new CoverageFacts(0, 0), new CoverageFacts(0, 0));
        Assert.Equal(new CoverageFacts(0, null).GetHashCode(), new CoverageFacts(0, null).GetHashCode());

        Assert.Single(new HashSet<CoverageFacts> { one, two, new(81.2, 70.1) });
        Assert.Equal(2, new HashSet<CoverageFacts> { one, new(81.2, null) }.Count);
    }

    /// <summary>A difference in either part is another record, and so is the same pair of numbers told in the other
    /// order: the line turned by a tenth, the branch turned by nothing at all, either part made the nothing the zero is
    /// not and the zero the nothing is not, and the two shares swapped - which is the record saying another thing, not
    /// the same thing again.</summary>
    [Fact]
    public void APartTurnedOrTurnedAboutIsAnotherRecord()
    {
        var one = new CoverageFacts(81.2, 70.1);

        Assert.NotEqual(one, new CoverageFacts(81.3, 70.1));
        Assert.NotEqual(one, new CoverageFacts(81.2, 70.2));
        Assert.NotEqual(one, new CoverageFacts(81.2, null));
        Assert.NotEqual(one, new CoverageFacts(null, 70.1));
        Assert.NotEqual(one, new CoverageFacts(null, null));
        Assert.NotEqual(one, new CoverageFacts(0, 0));
        Assert.NotEqual(one, new CoverageFacts(81.2, 0));
        Assert.NotEqual(one, new CoverageFacts(70.1, 81.2));

        Assert.True(one != new CoverageFacts(81.2, null));
        Assert.False(one == new CoverageFacts(81.2, null));
        Assert.False(one.Equals(new CoverageFacts(81.2, 0)));
        Assert.Equal(3, new HashSet<CoverageFacts> { one, one with { BranchPercent = null }, one with { BranchPercent = 0 } }.Count);
    }

    /// <summary>One number written two ways is one record said two ways: the share of nothing and the share below it by
    /// the sign of the division are the two parts of one record, with one hash - and the words that say them differ, so
    /// the words are no measure of the record. A share that is no number at all is one with itself, as the record
    /// compares its parts rather than ordering them.</summary>
    [Fact]
    public void OneNumberWrittenTwoWaysIsOneRecordSaidTwoWays()
    {
        var zero = new CoverageFacts(0, 0);
        var signed = new CoverageFacts(-0d, -0d);

        Assert.Equal(zero, signed);
        Assert.True(zero == signed);
        Assert.False(zero != signed);
        Assert.Equal(zero.GetHashCode(), signed.GetHashCode());
        Assert.Equal("CoverageFacts { LinePercent = 0, BranchPercent = 0 }", Words(zero));
        Assert.Equal("CoverageFacts { LinePercent = -0, BranchPercent = -0 }", Words(signed));

        var missing = new CoverageFacts(double.NaN, null);

        Assert.Equal(missing, new CoverageFacts(double.NaN, null));
        Assert.True(missing == new CoverageFacts(double.NaN, null));
        Assert.NotEqual(missing, new CoverageFacts(0, null));
    }

    /// <summary>The nothing on either side of the operators: two records that are no record at all are one, a record
    /// and no record are two - the equality and the inequality each saying it - and the record asked about itself as an
    /// object says no to the nothing, to a word it is not, and to the other record of the build that wears the same two
    /// numbers.</summary>
    [Fact]
    public void NoRecordIsOneWithNoRecordAndNotWithARecord()
    {
        CoverageFacts? nothing = null;
        CoverageFacts? alsoNothing = null;
        CoverageFacts? there = new CoverageFacts(0, 0);

        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == nothing);
        Assert.True(there != nothing);
        Assert.True(there is not null);
        Assert.Null(default(CoverageFacts));

        var coverage = new CoverageFacts(81.2, 70.1);

        Assert.False(coverage.Equals(null));
        Assert.False(coverage.Equals("81.2"));
        Assert.False(coverage.Equals((object)81.2));
        Assert.False(coverage.Equals(new ComplexityFacts(81.2, 70, 1)));
        Assert.False(there!.Equals(nothing));
    }

    // ----- the coverage a build endpoint reports -----

    /// <summary>The two numbers of an answer are the two parts of the record, each kept as the file writes it and in
    /// the place it writes it in, and the share the answer does not name is the part that stands unsaid rather than the
    /// part that stands at zero.</summary>
    [Fact]
    public void TheTwoNumbersOfTheAnswerAreTheTwoPartsTheRecordKeeps()
    {
        var both = CoverageOf("""{ "linePercent": 42.5, "branchPercent": 33.75 }""")!;

        Assert.Equal(new CoverageFacts(42.5, 33.75), both);
        Assert.Equal(42.5, both.LinePercent);
        Assert.Equal(33.75, both.BranchPercent);

        var linesOnly = CoverageOf("""{ "linePercent": 42.5 }""")!;

        Assert.Equal(42.5, linesOnly.LinePercent);
        Assert.Null(linesOnly.BranchPercent);
        Assert.Equal(new CoverageFacts(42.5, null), linesOnly);

        var branchesOnly = CoverageOf("""{ "branchPercent": 70.1 }""")!;

        Assert.Null(branchesOnly.LinePercent);
        Assert.Equal(70.1, branchesOnly.BranchPercent);
        Assert.NotEqual(linesOnly, branchesOnly);
    }

    /// <summary>The build that measured nothing of its code is said, and said as a record: a share of zero is a number
    /// the answer named, the part stands at zero rather than unsaid, and the record is there for the card to say -
    /// which is the one place the nothing and the zero part company.</summary>
    [Fact]
    public void ACoverageOfNothingIsARecordAndNotNoRecord()
    {
        var nothing = CoverageOf("""{ "linePercent": 0, "branchPercent": 0 }""")!;

        Assert.Equal(new CoverageFacts(0, 0), nothing);
        Assert.NotNull(nothing.LinePercent);
        Assert.Equal(0d, nothing.LinePercent!.Value);
        Assert.Equal(0d, nothing.BranchPercent!.Value);
        Assert.NotEqual(new CoverageFacts(null, null), nothing);
        Assert.Equal("CoverageFacts { LinePercent = 0, BranchPercent = 0 }", Words(nothing));

        var noLines = CoverageOf("""{ "linePercent": 0 }""")!;

        Assert.Equal(0d, noLines.LinePercent!.Value);
        Assert.Null(noLines.BranchPercent);
    }

    /// <summary>A section that names no number is no coverage at all: the empty object, the part that is a word or a
    /// flag, a list rather than a section and a section that is no section make the record the build does not carry -
    /// and the answer that carries nothing else is no answer either.</summary>
    [Fact]
    public void ASectionThatSaysNoNumberSaysNoCoverage()
    {
        Assert.Null(CoverageOf("{ }"));
        Assert.Null(CoverageOf("null"));
        Assert.Null(CoverageOf("[]"));
        Assert.Null(CoverageOf("""[ 81.2, 70.1 ]"""));
        Assert.Null(CoverageOf("81.2"));
        Assert.Null(CoverageOf("true"));
        Assert.Null(CoverageOf("""{ "linePercent": null, "branchPercent": null }"""));
        Assert.Null(CoverageOf("""{ "linePercent": { }, "branchPercent": [ ] }"""));
        Assert.Null(CoverageOf("""{ "line": 81.2, "branch": 70.1 }"""));

        // Nothing at all about the code is nothing to say about the build either.
        Assert.Null(BuildInfo.Parse("""{ "coverage": { "linePercent": null } }"""));
    }

    /// <summary>What the reader of the answer refuses never reaches the record, and what it lets through is kept whole:
    /// a share over the whole of it stands as it was written because nothing caps what a build says about itself, while
    /// the share below zero, the share too large for the number and the share written as a word are left out of the
    /// pair - the part beside them standing on its own.</summary>
    [Fact]
    public void AShareNoReaderWouldLetThroughIsLeftOutOfThePairTheRecordIsMadeOf()
    {
        Assert.Equal(new CoverageFacts(120.5, null), CoverageOf("""{ "linePercent": 120.5 }"""));
        Assert.Equal(new CoverageFacts(100, double.Epsilon), CoverageOf("""{ "linePercent": 100, "branchPercent": 5e-324 }"""));
        Assert.Equal(new CoverageFacts(double.MaxValue, null), CoverageOf("""{ "linePercent": 1.7976931348623157e308 }"""));

        Assert.Equal(new CoverageFacts(null, 70.1), CoverageOf("""{ "linePercent": -1, "branchPercent": 70.1 }"""));
        Assert.Equal(new CoverageFacts(null, 70.1), CoverageOf("""{ "linePercent": -0.001, "branchPercent": 70.1 }"""));
        Assert.Equal(new CoverageFacts(null, 70.1), CoverageOf("""{ "linePercent": 1e400, "branchPercent": 70.1 }"""));
        Assert.Equal(new CoverageFacts(null, 70.1), CoverageOf("""{ "linePercent": "81.2", "branchPercent": 70.1 }"""));
        Assert.Equal(new CoverageFacts(null, 70.1), CoverageOf("""{ "linePercent": true, "branchPercent": 70.1 }"""));
        Assert.Equal(new CoverageFacts(81.2, null), CoverageOf("""{ "linePercent": 81.2, "branchPercent": false }"""));

        // Both shares refused is no coverage, and no coverage is what the card has nothing to say about.
        Assert.Null(CoverageOf("""{ "linePercent": -1, "branchPercent": -70.1 }"""));
    }

    /// <summary>The coverage of the build file the shipped sample holds, as the very record it stands for: the two
    /// numbers the sample writes, the parts they are kept in, the pair the record falls apart into, the words it says
    /// them in and the set that holds them once - every one of them a fixed value of the file rather than a number this
    /// test made up.</summary>
    [Fact]
    public void TheCoverageOfTheSampleBuildIsTheRecordItStandsFor()
    {
        var coverage = BuildInfo.Parse(Optics.Build)!.Coverage!;

        Assert.Equal(81.2, coverage.LinePercent);
        Assert.Equal(70.1, coverage.BranchPercent);

        var (lines, branches) = coverage;

        Assert.Equal(81.2, lines);
        Assert.Equal(70.1, branches);

        Assert.Equal("CoverageFacts { LinePercent = 81.2, BranchPercent = 70.1 }", Words(coverage));
        Assert.Single(new HashSet<CoverageFacts> { coverage, new(81.2, 70.1) });

        // The branch is the part a build may leave out; turning it off leaves the line the sample says.
        Assert.Equal(new CoverageFacts(81.2, null), coverage with { BranchPercent = null });
        Assert.NotEqual(coverage, coverage with { BranchPercent = null });
    }

    // ----- the machinery -----

    /// <summary>The coverage one answer reports, as the dashboard reads it, the section being the text given - a
    /// section, a word, a list or the nothing, as each test writes it.</summary>
    private static CoverageFacts? CoverageOf(string coverage) => BuildAnswers.Of("coverage", coverage).Coverage;

    /// <summary>The words a record says its two parts in, taken in the invariant culture whatever the reader of this
    /// test happens to be: the record's own words follow whoever reads them, which is pinned on its own below, so the
    /// strings above are the point and the culture only the way to say them once.</summary>
    private static string Words(CoverageFacts coverage) => ViewerCulture.Invariant(() => coverage.ToString()!);
}
