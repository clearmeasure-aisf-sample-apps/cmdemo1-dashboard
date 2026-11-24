namespace Dashboard.Tests;

/// <summary>
/// <see cref="BuildText" /> is the "Code" card's vocabulary: every line a viewer reads about a build - the lines and
/// the files, the language bar, the tests, the coverage, the complexity, the CRAP score, the problems - is a
/// sentence this class decides out of the numbers the build endpoint answered. It holds no moment and calls nothing,
/// so every one of its members is reachable with a value the test brings; none of them needs the network or a clock,
/// and nothing here asks for either.
/// <para>
/// What the shipped fixtures already pin is left alone: <see cref="BuildInfoTests" /> pins the whole card of a
/// deployable's answer and of a build that measures part of itself, <see cref="DeployableFactsTests" /> and
/// <see cref="DashboardBuildTests" /> pin the same words on a deployable's card and on the dashboard's own,
/// <see cref="HealthHelperCoverageTests" /> pins <c>Percent</c> and <c>Number</c> one decimal at a time and
/// <see cref="HealthTextHelperTests" /> pins <c>SharePercent</c> from <c>0 %</c> to <c>&lt;1 %</c> and the bar in
/// words, <see cref="EdgeCaseTextTests" /> pins the seven characters of <c>ShortCommit</c> - one under the cut, on
/// it, one over, the empty string, and no string at all refused by name - through <c>LinkText.CommitTitle</c>, and
/// <see cref="TargetTileClusterCardEnvironmentDeliveryTests" /> pins the <c>0</c> and the <c>1,234</c> of
/// <c>Count</c>. The constants <c>ThisPage</c> and <c>DashboardHelp</c> are held word for word in
/// <see cref="DeployableFactsTests" /> and <c>Other</c> in the names <see cref="BuildInfoTests" /> reads off the bar.
/// </para>
/// <para>
/// What is pinned here is the edge those sentences sit next to and leave open: the ends of the numbers, the longest
/// count and the most negative one, the percentage that is no number and the one that is no end of it, the share
/// that rounds to a nothing it is signed for; the argument that is not there, refused by name by every member that
/// guards itself - each by its own parameter - and by the enumeration <c>Describe</c> walks past; the empty of each - no languages, no
/// parts, no kind of test, no fact measured at all - which the card says as nothing rather than as a zero it was
/// never told; the boundary of the bar, which names exactly <c>NamedLanguages</c> of them and folds a sixth, and a
/// sixth of no lines into nothing at all; the counts no build is expected to give, a total that spills over the
/// integer it is added in and a CRAP count below zero; that a percent is never grouped while a count always is; and
/// the one word chosen by comparison rather than by formatting, a worst score standing exactly on the threshold not
/// being over it.
/// </para>
/// </summary>
public class BuildTextEdgeCaseTests
{
    // ----- Count, Percent and Number: the numbers the card is written out of

    /// <summary>A count is grouped wherever the separator falls, on both sides of zero and out to the ends of the
    /// number: one thousand is the first comma and nine hundred ninety-nine none, the longest a build can report
    /// keeps all nineteen of its digits and its sign, and a negative is said with its minus rather than dropped -
    /// the same rule on the side of zero no build endpoint means.</summary>
    [Theory]
    [InlineData(-1L, "-1")]
    [InlineData(-999L, "-999")]
    [InlineData(-1000L, "-1,000")]
    [InlineData(-1234L, "-1,234")]
    [InlineData(1L, "1")]
    [InlineData(999L, "999")]
    [InlineData(1000L, "1,000")]
    [InlineData(999_999L, "999,999")]
    [InlineData(1_000_000L, "1,000,000")]
    [InlineData(long.MinValue, "-9,223,372,036,854,775,808")]
    [InlineData(long.MaxValue, "9,223,372,036,854,775,807")]
    public void ACountIsGroupedAtTheEndsAndOnBothSidesOfZero(long number, string expected) =>
        Assert.Equal(expected, BuildText.Count(number));

    /// <summary>A percentage and a plain number keep at most one decimal and no separator at all: a hundredth
    /// rounds up to a tenth and a forty-ninth hundredth falls to nothing, ninety-nine point nine nine is a hundred
    /// and a hundred and four hundredths is a hundred still, the negative keeps its sign, and the zero that is
    /// negative keeps a sign of its own - <c>-0 %</c>, which is what the card says for a share a build reported as
    /// less than nothing by a hair. Past a thousand the two parts of the rule show: a percent is written, not
    /// counted, and loses the comma a count would take.</summary>
    [Theory]
    [InlineData(0.049d, "0 %", "0")]
    [InlineData(0.05d, "0.1 %", "0.1")]
    [InlineData(0.0999d, "0.1 %", "0.1")]
    [InlineData(99.99d, "100 %", "100")]
    [InlineData(100.04d, "100 %", "100")]
    [InlineData(1234d, "1234 %", "1234")]
    [InlineData(-0.0d, "-0 %", "-0")]
    [InlineData(-0.04d, "-0 %", "-0")]
    [InlineData(-100d, "-100 %", "-100")]
    public void TheTwoSaidWithOneDecimalReachTheirEnds(double number, string percent, string plain)
    {
        Assert.Equal(percent, BuildText.Percent(number));
        Assert.Equal(plain, BuildText.Number(number));
    }

    /// <summary>A number that is no number is still said: neither helper refuses it, falls back to a dash, nor
    /// drops the unit - a build endpoint that answered <c>NaN</c> is shown as <c>NaN %</c>, which is a lie of
    /// omission no smaller than the made-up zero would be, and the word the card gives is the word the invariant
    /// culture has for it, on both infinities and at the smallest number a double holds alike.</summary>
    [Fact]
    public void ANumberThatIsNoNumberIsSaidAsOne()
    {
        Assert.Equal("NaN %", BuildText.Percent(double.NaN));
        Assert.Equal("NaN", BuildText.Number(double.NaN));
        Assert.Equal("Infinity %", BuildText.Percent(double.PositiveInfinity));
        Assert.Equal("Infinity", BuildText.Number(double.PositiveInfinity));
        Assert.Equal("-Infinity %", BuildText.Percent(double.NegativeInfinity));
        Assert.Equal("-Infinity", BuildText.Number(double.NegativeInfinity));

        // The smallest number above zero rounds to nothing, and the bar's own word for it says it is still there.
        Assert.Equal("0 %", BuildText.Percent(double.Epsilon));
        Assert.Equal("<1 %", BuildText.SharePercent(double.Epsilon));
    }

    /// <summary>None of them follows whoever reads the page: a viewer of German, whose culture writes thousands with
    /// a point and decimals with a comma, is told the very words the build published its facts in - the comma of
    /// <c>1,234</c> and the point of <c>28.5</c>, in a sentence and not only in a number.</summary>
    [Fact]
    public void TheNumbersOfTheCardFollowTheDashboardAndNotTheViewer() =>
        ViewerCulture.German(() =>
        {
            Assert.Equal("1,234", BuildText.Count(1234));
            Assert.Equal("9,223,372,036,854,775,807", BuildText.Count(long.MaxValue));
            Assert.Equal("28.5", BuildText.Number(28.5));
            Assert.Equal("1,000 problems", BuildText.Problems(1000));
            Assert.Equal("84,210 lines in 1,203 files", BuildText.Size(new CodeSize(84_210, 1203, [])));
            Assert.Equal("6,000: 1,000 unit, 2,000 integration, 3,000 acceptance", BuildText.Tests(new TestCounts(1000, 2000, 3000)));
            Assert.Equal("worst 28.5, 1,000 methods over 30", BuildText.Crap(new CrapFacts(28.5, 30, 1000)));
        });

    // ----- SharePercent: the whole shares the bar rounds, past the hundred and below the half

    /// <summary>A share is rounded to a whole and said with the unit, past the ends a bar is drawn between: a share
    /// over the whole is said as the number it is, and a negative one keeps its sign - rounding to the nothing it
    /// does not reach at a half below zero and past it to <c>-1 %</c>. A midpoint goes to the even whole, which is
    /// how the two halves of the same rule read: two and a half is <c>2 %</c>, where three and a half (pinned with
    /// the shipped bar) is <c>4 %</c>.</summary>
    [Theory]
    [InlineData(100.4d, "100 %")]
    [InlineData(120.6d, "121 %")]
    [InlineData(-0.4d, "-0 %")]
    [InlineData(-0.5d, "-0 %")]
    [InlineData(-0.6d, "-1 %")]
    [InlineData(-1.4d, "-1 %")]
    [InlineData(2.5d, "2 %")]
    public void AShareIsRoundedWhereTheWholeTakesIt(double percent, string expected) =>
        Assert.Equal(expected, BuildText.SharePercent(percent));

    /// <summary>The one exception is a window and not a rule: it opens above zero and closes at one, so the
    /// smallest number a double holds is <c>&lt;1 %</c>, the whole of one is <c>1 %</c>, nothing at all is
    /// <c>0 %</c> - the exception is for a language that is there and small, not for one that is not - and a share
    /// that is no number falls outside the window and is said as the word for one.</summary>
    [Fact]
    public void TheExceptionForASmallShareOpensAboveZeroAndClosesAtOne()
    {
        Assert.Equal("<1 %", BuildText.SharePercent(double.Epsilon));
        Assert.Equal("1 %", BuildText.SharePercent(1));
        Assert.Equal("0 %", BuildText.SharePercent(0));
        Assert.Equal("-0 %", BuildText.SharePercent(-0.0));
        Assert.Equal("NaN %", BuildText.SharePercent(double.NaN));
        Assert.Equal("Infinity %", BuildText.SharePercent(double.PositiveInfinity));
    }

    // ----- Size: the lines and the files, and neither

    /// <summary>The size line says what the build measured and nothing else: the two together, either alone, the
    /// zero a build measured and reported as nothing, the negative and the longest of both - and no plural rule
    /// anywhere, one line being said as <c>1 lines</c> the way a hundred are. That is the sentence as it stands
    /// today, and it is said here so the day someone gives it a plural the change is seen.</summary>
    [Theory]
    [InlineData(84_210L, 1203, "84,210 lines in 1,203 files")]
    [InlineData(0L, 0, "0 lines in 0 files")]
    [InlineData(1L, 1, "1 lines in 1 files")]
    [InlineData(-5L, -2, "-5 lines in -2 files")]
    [InlineData(700L, null, "700 lines")]
    [InlineData(0L, null, "0 lines")]
    [InlineData(null, 1_203, "1,203 files")]
    [InlineData(null, 0, "0 files")]
    [InlineData(long.MinValue, int.MinValue, "-9,223,372,036,854,775,808 lines in -2,147,483,648 files")]
    [InlineData(long.MaxValue, int.MaxValue, "9,223,372,036,854,775,807 lines in 2,147,483,647 files")]
    public void TheSizeSaysTheLinesAndTheFilesItWasGiven(long? lines, int? files, string expected) =>
        Assert.Equal(expected, BuildText.Size(new CodeSize(lines, files, [])));

    /// <summary>Neither number measured is no sentence at all: <c>null</c>, which lets the card leave the line out,
    /// and not the empty string, which would show a line of nothing. A language list changes none of that - the
    /// lines of the languages are added together where the answer is read, and a record handed no total and no
    /// files says nothing though it names a language, while the total it is handed it says.</summary>
    [Fact]
    public void ASizeThatMeasuredNeitherNumberSaysNothing()
    {
        Assert.Null(BuildText.Size(new CodeSize(null, null, [])));
        Assert.Null(BuildText.Size(new CodeSize(null, null, [new LanguageSize("C#", 500, null)])));
        Assert.Equal("500 lines", BuildText.Size(new CodeSize(500, null, [new LanguageSize("C#", 500, null)])));
    }

    /// <summary>A size that is not there is refused by name, the way every line of the card drawn out of a record
    /// is: the helper does not decide for the caller that a build measured nothing, it says the caller asked wrong.</summary>
    [Fact]
    public void ASizeThatIsNotThereIsRefusedByName()
    {
        var error = Assert.Throws<ArgumentNullException>(() => BuildText.Size(null!));

        Assert.Equal("code", error.ParamName);
    }

    // ----- Shares and Describe: the bar, its parts and its words

    /// <summary>The bar names as many languages as it has colours and folds the rest: five languages are five
    /// parts, each with the lines it holds, its share of the whole and a slot counting from one, and a sixth - one
    /// that has lines - becomes <c>Other</c> with the lines of all the rest together and the slot zero, the colour
    /// of nothing named. One language is a bar of the whole of itself.</summary>
    [Fact]
    public void TheBarNamesFiveLanguagesAndFoldsTheRestIntoOther()
    {
        Assert.Equal(5, BuildText.NamedLanguages);

        var five = BuildText.Shares(Bar(("C#", 500), ("Razor", 400), ("SQL", 300), ("JavaScript", 200), ("CSS", 200)));

        Assert.Equal(["C#", "Razor", "SQL", "JavaScript", "CSS"], five.Select(share => share.Name));
        Assert.Equal([1, 2, 3, 4, 5], five.Select(share => share.Slot));
        Assert.Equal([500L, 400L, 300L, 200L, 200L], five.Select(share => share.Lines).ToArray());
        Assert.Equal(31.25, five[0].Percent, 6);
        Assert.Equal(25, five[1].Percent, 6);
        Assert.Equal(18.75, five[2].Percent, 6);
        Assert.Equal(12.5, five[3].Percent, 6);
        Assert.Equal(12.5, five[4].Percent, 6);
        Assert.DoesNotContain(BuildText.Other, five.Select(share => share.Name), StringComparer.Ordinal);

        var six = BuildText.Shares(Bar(("C#", 500), ("Razor", 400), ("SQL", 300), ("JavaScript", 200), ("CSS", 100), ("PowerShell", 100)));

        Assert.Equal(["C#", "Razor", "SQL", "JavaScript", "CSS", "Other"], six.Select(share => share.Name));
        Assert.Equal([1, 2, 3, 4, 5, 0], six.Select(share => share.Slot));
        Assert.Equal([500L, 400L, 300L, 200L, 100L, 100L], six.Select(share => share.Lines).ToArray());
        Assert.Equal(100, six.Sum(share => share.Percent), 6);
        Assert.Equal(31.25, six[0].Percent, 6);
        Assert.Equal(6.25, six[4].Percent, 6);
        Assert.Equal(6.25, six[5].Percent, 6);

        var one = Assert.Single(BuildText.Shares(Bar(("C#", 42))));
        Assert.Equal("C#", one.Name);
        Assert.Equal(42L, one.Lines);
        Assert.Equal(100, one.Percent, 6);
        Assert.Equal(1, one.Slot);
    }

    /// <summary>What is folded away is the rest of the lines, and the rest of nothing is nothing: a sixth language
    /// that reported no line at all adds no <c>Other</c> part, so a card never draws an empty segment nor says a
    /// share of nought a viewer would read as a language with something in it.</summary>
    [Fact]
    public void ARestOfNoLinesIsNoOtherPart()
    {
        var shares = BuildText.Shares(Bar(("C#", 500), ("Razor", 400), ("SQL", 300), ("JavaScript", 200), ("CSS", 200), ("YAML", 0)));

        Assert.Equal(5, shares.Count);
        Assert.Equal([1, 2, 3, 4, 5], shares.Select(share => share.Slot));
        Assert.Equal(100, shares.Sum(share => share.Percent), 6);
    }

    /// <summary>The parts are the languages they were given, in the order they were given in, and their share is
    /// cut from the lines of the languages alone: the bar sorts nothing that the read already sorted, and the total
    /// of the record - the number the size line says - is not the whole a share is measured against, the two being
    /// counted by different tools of the build.</summary>
    [Fact]
    public void TheSharesAreCutFromTheLinesOfTheLanguagesAlone()
    {
        var shares = BuildText.Shares(new CodeSize(10_000, 7, [new LanguageSize("A", 100, null), new LanguageSize("B", 300, null)]));

        Assert.Equal(["A", "B"], shares.Select(share => share.Name));
        Assert.Equal([100L, 300L], shares.Select(share => share.Lines).ToArray());
        Assert.Equal(25, shares[0].Percent, 6);
        Assert.Equal(75, shares[1].Percent, 6);
        Assert.Equal("A 25 %, B 75 %", BuildText.Describe(shares));
    }

    /// <summary>No lines to share out is no bar: the whole the shares would be cut from is refused before a part is
    /// made, so a build that measured no language, one that measured a language of no lines, and one whose lines
    /// came out negative all draw an empty bar rather than a division by zero or a hundred split out of nothing -
    /// and a code size that is not there at all is refused by name.</summary>
    [Fact]
    public void WithoutLinesThereIsNoBar()
    {
        Assert.Empty(BuildText.Shares(new CodeSize(1_000, 3, [])));
        Assert.Empty(BuildText.Shares(new CodeSize(null, null, [])));
        Assert.Empty(BuildText.Shares(Bar(("C#", 0))));
        Assert.Empty(BuildText.Shares(Bar(("C#", -5))));

        var error = Assert.Throws<ArgumentNullException>(() => BuildText.Shares(null!));

        Assert.Equal("code", error.ParamName);
    }

    /// <summary>The bar in words joins the parts it is given with commas and says nothing else: no parts are the
    /// empty string, which leaves the line off the card rather than drawing a line of a space, one part stands
    /// without a comma beside it, and a name is said as it was given - the <c>Other</c> of the folded rest no
    /// differently from a language, each with the whole percentage the bar shows it at.</summary>
    [Fact]
    public void TheBarInWordsJoinsWhatItIsGivenAndSaysNothingOfNothing()
    {
        Assert.Equal(string.Empty, BuildText.Describe([]));
        Assert.Equal("C# 100 %", BuildText.Describe([new LanguageShare("C#", 42, 100, 1)]));
        Assert.Equal(
            "A 33 %, B 33 %, Other 33 %",
            BuildText.Describe([new LanguageShare("A", 1, 100 / 3d, 1), new LanguageShare("B", 1, 100 / 3d, 2), new LanguageShare(BuildText.Other, 1, 100 / 3d, 0)]));
    }

    /// <summary>The bar in words has no guard of its own - it is a <c>string.Join</c> over whatever the caller
    /// hands it - so the refusal of a bar that is not there is the enumeration's, named for the sequence it could
    /// not walk rather than for a parameter this helper ever looked at.</summary>
    [Fact]
    public void ABarThatIsNotThereIsRefusedByTheEnumeration()
    {
        var error = Assert.Throws<ArgumentNullException>(() => BuildText.Describe(null!));

        Assert.Equal("source", error.ParamName);
    }

    // ----- Tests, Coverage, Complexity and Crap: a line each, out of the parts that are there

    /// <summary>The tests line is the total before a colon and the kinds after it - and what it says when the build
    /// reported no kind at all is the total of none of them with nothing behind the colon: <c>"0: "</c>, the space
    /// and all. A kind counted at zero is a kind measured: alone it is said on its own, without a total and without
    /// a colon - zero being the answer the build gave and not the absence of one - and beside others it is counted
    /// among them, the total of the three added by the record rather than by the sentence.</summary>
    [Theory]
    [InlineData(null, null, null, "0: ")]
    [InlineData(0, 0, 0, "0: 0 unit, 0 integration, 0 acceptance")]
    [InlineData(0, null, null, "0 unit")]
    [InlineData(null, 0, null, "0 integration")]
    [InlineData(null, null, 0, "0 acceptance")]
    [InlineData(0, 0, null, "0: 0 unit, 0 integration")]
    [InlineData(1000, 2000, 3000, "6,000: 1,000 unit, 2,000 integration, 3,000 acceptance")]
    [InlineData(-5, 3, null, "-2: -5 unit, 3 integration")]
    public void TheTestsLineSaysTheKindsItWasGiven(int? unit, int? integration, int? acceptance, string expected) =>
        Assert.Equal(expected, BuildText.Tests(new TestCounts(unit, integration, acceptance)));

    /// <summary>The total of the kinds is added in the integer they are counted in, so the counts no build has -
    /// the top of an <c>int</c> and one more - come out at the bottom of it, with their sign and their grouping,
    /// while the kinds beside them are still said as they stand. The line is the arithmetic behind the number said
    /// out loud, which is what lets a reader see that a total of <c>-1</c> is not a test run.</summary>
    [Fact]
    public void TheTotalOfTheTestsIsAddedInTheIntegerItIsHeldIn()
    {
        Assert.Equal("-2,147,483,648: 2,147,483,647 unit, 1 integration, 0 acceptance", BuildText.Tests(new TestCounts(int.MaxValue, 1, 0)));
        Assert.Equal("2,147,483,647: 2,147,483,647 unit, 0 integration", BuildText.Tests(new TestCounts(int.MaxValue, 0, null)));
        Assert.Equal("-1: 2,147,483,647 unit, -2,147,483,648 acceptance", BuildText.Tests(new TestCounts(int.MaxValue, null, int.MinValue)));
        Assert.Equal("2,147,483,647 unit", BuildText.Tests(new TestCounts(int.MaxValue, null, null)));
    }

    /// <summary>Coverage the build did not measure says nothing: no percent of lines, no percent of branches, and
    /// no comma where the two would meet - the empty string, which leaves the line off the card. Either half alone
    /// stands, the zero that was measured is said as <c>0 %</c> rather than left out as if it had not been, and a
    /// percent past a thousand is written plain, without the comma the counts of the same card take.</summary>
    [Theory]
    [InlineData(null, null, "")]
    [InlineData(0d, 0d, "0 % of lines, 0 % of branches")]
    [InlineData(100d, null, "100 % of lines")]
    [InlineData(null, 0d, "0 % of branches")]
    [InlineData(-1.5d, 0d, "-1.5 % of lines, 0 % of branches")]
    [InlineData(12.34d, 0.049d, "12.3 % of lines, 0 % of branches")]
    [InlineData(1234d, null, "1234 % of lines")]
    public void TheCoverageLineSaysTheHalvesItWasGiven(double? line, double? branch, string expected) =>
        Assert.Equal(expected, BuildText.Coverage(new CoverageFacts(line, branch)));

    /// <summary>The complexity line is the parts there are, with the methods in brackets after them - and the space
    /// in front of those brackets trimmed away when nothing stands before them, so methods alone are a bracket of
    /// their own and not a bracket with a hole in front. No part at all is the empty string, an average alone has
    /// neither a comma nor a bracket, and the zero that was measured is said as <c>average 0, worst 0</c>.</summary>
    [Theory]
    [InlineData(null, null, null, "")]
    [InlineData(null, null, 5_210, "(5,210 methods)")]
    [InlineData(null, null, 0, "(0 methods)")]
    [InlineData(1.9d, null, null, "average 1.9")]
    [InlineData(1.9d, null, 0, "average 1.9 (0 methods)")]
    [InlineData(0d, 0, 0, "average 0, worst 0 (0 methods)")]
    [InlineData(-1.5d, -2, null, "average -1.5, worst -2")]
    [InlineData(double.NaN, null, 7, "average NaN (7 methods)")]
    public void TheComplexityLineSaysThePartsItWasGiven(double? average, int? max, int? methods, string expected) =>
        Assert.Equal(expected, BuildText.Complexity(new ComplexityFacts(average, max, methods)));

    /// <summary>The CRAP line says the worst score, then what stands over the threshold in the plural the count
    /// asks for; with no threshold the limit is named as <c>the threshold</c>, and a worst score alone stands
    /// alone. A threshold with no count beside it is said as <c>threshold 30</c>, so the number a card was built
    /// around is never dropped silently, a count below zero takes the plural and its sign, and no fact at all is
    /// the empty string.</summary>
    [Theory]
    [InlineData(null, null, null, "")]
    [InlineData(null, null, 0, "none over the threshold")]
    [InlineData(null, null, 1, "1 method over the threshold")]
    [InlineData(null, null, 3, "3 methods over the threshold")]
    [InlineData(null, 30d, null, "threshold 30")]
    [InlineData(41d, null, null, "worst 41")]
    [InlineData(null, 30d, 1_000, "1,000 methods over 30")]
    [InlineData(41d, 30d, -2, "worst 41, -2 methods over 30")]
    [InlineData(0d, 0d, 0, "worst 0, none over 0")]
    [InlineData(null, double.NaN, 0, "none over NaN")]
    public void TheCrapLineSaysTheFactsItWasGiven(double? max, double? threshold, int? over, string expected) =>
        Assert.Equal(expected, BuildText.Crap(new CrapFacts(max, threshold, over)));

    /// <summary>Marking a method as over the threshold is decided by the count when the build gives one - so the
    /// worst score of the fleet standing above the threshold does not overrule <c>none over 30</c>, and a count
    /// below zero is no count over it either - and by the worst score against the threshold when it does not. That
    /// comparison holds only past the threshold: a worst score exactly on it is not over it, one tenth past it is,
    /// and the two numbers that are not there are never more than each other.</summary>
    [Theory]
    [InlineData(30d, 30d, null, false)]
    [InlineData(30.1d, 30d, null, true)]
    [InlineData(29.9d, 30d, null, false)]
    [InlineData(41d, 30d, 0, false)]
    [InlineData(41d, 30d, -3, false)]
    [InlineData(41d, 30d, 1, true)]
    [InlineData(0d, 0d, 0, false)]
    [InlineData(null, null, null, false)]
    [InlineData(null, 30d, null, false)]
    [InlineData(41d, null, null, false)]
    [InlineData(-1d, -2d, null, true)]
    public void AMethodIsOverTheThresholdOnlyPastIt(double? max, double? threshold, int? over, bool expected) =>
        Assert.Equal(expected, BuildText.CrapIsOver(new CrapFacts(max, threshold, over)));

    /// <summary>Problems are counted in the three words the card has for none, one and more, out to the ends of the
    /// count: a negative is said with the plural and its sign - the words were chosen for the counts a build
    /// reports, and nothing makes <c>-1</c> a problem - and both ends of the integer keep their grouping.
    /// <c>no problems</c>, <c>1 problem</c> and <c>1,204 problems</c> are pinned where the fixtures answer them.</summary>
    [Theory]
    [InlineData(-1, "-1 problems")]
    [InlineData(-1000, "-1,000 problems")]
    [InlineData(2, "2 problems")]
    [InlineData(999, "999 problems")]
    [InlineData(1000, "1,000 problems")]
    [InlineData(int.MinValue, "-2,147,483,648 problems")]
    [InlineData(int.MaxValue, "2,147,483,647 problems")]
    public void TheProblemsAreCountedInTheWordsThatSayThem(int problems, string expected) =>
        Assert.Equal(expected, BuildText.Problems(problems));

    /// <summary>The five helpers that draw a card line out of a fact refuse a fact that is not there, each by the
    /// name of its own parameter: none of them decides to say nothing in place of a record nobody handed it, which
    /// is the caller's decision and the one a card gets wrong quietly.</summary>
    [Fact]
    public void TheLinesOfTheCardRefuseTheFactsThatAreNotThere()
    {
        Assert.Equal("tests", Assert.Throws<ArgumentNullException>(() => BuildText.Tests(null!)).ParamName);
        Assert.Equal("coverage", Assert.Throws<ArgumentNullException>(() => BuildText.Coverage(null!)).ParamName);
        Assert.Equal("complexity", Assert.Throws<ArgumentNullException>(() => BuildText.Complexity(null!)).ParamName);
        Assert.Equal("crap", Assert.Throws<ArgumentNullException>(() => BuildText.Crap(null!)).ParamName);
        Assert.Equal("crap", Assert.Throws<ArgumentNullException>(() => BuildText.CrapIsOver(null!)).ParamName);
    }

    // ----- the words of the dashboard's own card

    /// <summary>The context of the dashboard's own card is whoever is named with the page's own words after them
    /// in brackets: a name of its own, a name with a space in it, and the empty name and the name that is not
    /// there at all, which leave the bracket standing on a space - the helper guards nothing, because a context of
    /// nobody named is still a context and says so rather than throwing.</summary>
    [Fact]
    public void TheContextBracketsWhoeverIsNamedEvenNobody()
    {
        Assert.Equal("Demo Portal (this page)", BuildText.DashboardContext("Demo Portal"));
        Assert.Equal("x (this page)", BuildText.DashboardContext("x"));
        Assert.Equal(" (this page)", BuildText.DashboardContext(string.Empty));
        Assert.Equal(" (this page)", BuildText.DashboardContext(null!));

        // The words in the bracket are the chip's own, so the two can never be reworded apart.
        Assert.Equal($"x ({BuildText.ThisPage})", BuildText.DashboardContext("x"));
    }

    // ----- ShortCommit: the dashboard-wide short SHA

    /// <summary>A full forty-character SHA is shown as git's short form, its first seven characters.</summary>
    [Fact]
    public void AFullShaIsCutToItsFirstSevenCharacters() =>
        Assert.Equal("a1b2c3d", BuildText.ShortCommit("a1b2c3d4e5f6789012345678abcdef9012345678"));

    /// <summary>A commit of exactly seven characters is already short and comes back as it is, not cut; one of
    /// eight loses only its last.</summary>
    [Fact]
    public void ACommitOfExactlySevenCharactersIsNotCut()
    {
        Assert.Equal("5c0ffee", BuildText.ShortCommit("5c0ffee"));
        Assert.Equal("5c0ffee", BuildText.ShortCommit("5c0ffee1"));
        Assert.Equal("abc", BuildText.ShortCommit("abc"));
    }

    /// <summary>An empty commit comes back empty, and a commit that is not there is refused by name.</summary>
    [Fact]
    public void AnEmptyCommitIsEmptyAndNoCommitIsRefused()
    {
        Assert.Equal(string.Empty, BuildText.ShortCommit(string.Empty));
        Assert.Equal("commit", Assert.Throws<ArgumentNullException>(() => BuildText.ShortCommit(null!)).ParamName);
    }

    // ----- the fixture

    /// <summary>A code size whose bar is the languages it is given, in the order they are given in, of no lines of
    /// its own and no files - which is what the shares are cut from, and what they are not.</summary>
    private static CodeSize Bar(params (string Name, long Lines)[] languages) =>
        new(null, null, [.. languages.Select(language => new LanguageSize(language.Name, language.Lines, null))]);
}
