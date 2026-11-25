using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="Trend" />: the sparkline drawn beside a number, asked
/// of the type itself rather than of the node whose readings made it. What is pinned here is <see cref="Trend.Of" />
/// refusing the one thing it cannot draw with and answering nothing for the readings that are no number, the two edges
/// of its scale (a range that never rises above zero is drawn flat at the bottom, a reading that never moves is drawn
/// at the top, a reading below the largest is clamped to the bottom and not below it, and the smallest of a huge range
/// is rounded onto the bottom), the name and the unit when they are nothing, and the readings being walked once however
/// many there are. Then the two draws of a box at its edges: no inset, half a box of inset, a box of nothing, a height
/// outside the scale, no heights at all and one height alone, and the one number where the line and the dot round
/// differently. Finally the two parts the record is made of and the members every record is asked of them.
/// <para>
/// What the other files already pin is left alone. <see cref="TrendTests" /> pins the scale of a real line, that fewer
/// than two readings make no trend at all, that a round without a reading is left out, and the length of a drawn line;
/// <see cref="SampleWindowIsNamedTests" /> pins that length's value and that the two buffers name it where they keep
/// their readings. <see cref="TrendGeometryTests" /> pins the component's own box (52 x 16, inset 2.5) and that a line
/// of three or more readings ends on its "now" dot; <see cref="TrendNumberEdgeCaseTests" /> the one decimal of the
/// title's numbers and their culture. <see cref="TrendsHelperTests" /> and <see cref="ClusterMonitorTrendUnitsTests" />
/// pin how a node's and a cluster's own numbers become a line. <see cref="RuntimeTrendEdgeCaseTests" /> pins the copy
/// the script is handed, not the trend it is copied from.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every reading below is one the test brought.
/// </para>
/// </summary>
public class TrendEdgeCaseTests
{
    /// <summary>The readings a node reported, oldest first and none of them a gap, as <see cref="Trend.Of" /> takes
    /// them: the line the component draws, with the test's own name in its title.</summary>
    private static Trend Line(params double[] readings) => Trend.Of(readings.Select(value => (double?)value), "x")!;

    /// <summary>Heights as the component is handed them, straight into the record and past the scale of a real
    /// line - the way a trend built by hand looks to <see cref="Trend.Polyline" /> and <see cref="Trend.Last" />.</summary>
    private static Trend Drawn(IReadOnlyList<double> heights) => new(heights, "x");

    // --------------------------------------------------------------------------------- Of: nothing to draw from

    [Fact]
    public void WithoutAnyReadingsThereIsNothingToDraw() => Assert.Throws<ArgumentNullException>(() => Trend.Of(null!, "x"));

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public void RoundsThatAreAllGapsAreNoLine(int gaps) => Assert.Null(Trend.Of(Enumerable.Repeat<double?>(null, gaps), "x"));

    [Fact]
    public void TheReadingsAreWalkedOnceAndNoMore()
    {
        var walks = 0;

        IEnumerable<double?> Once()
        {
            walks++;
            yield return 1d;
            yield return 3d;
        }

        var trend = Trend.Of(Once(), "x")!;

        Assert.Equal(1, walks);
        Assert.Equal([0.333, 1], trend.Points);
    }

    // -------------------------------------------------------------------------- Of: the name and the unit given

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ANameThatIsNothingOrEmptyIsLeftOutOfTheSentence(string? what) =>
        Assert.Equal(", last 2 checks: 1 to 2, now 2", Trend.Of([1, 2], what!)!.Title);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AUnitThatIsNothingOrEmptyFollowsTheNumbersAsNothing(string? unit) =>
        Assert.Equal("cpu, last 2 checks: 1 to 2, now 2", Trend.Of([1, 2], "cpu", unit!)!.Title);

    [Fact]
    public void ANameIsWrittenAsItStandsEvenWhenItLooksLikeAFormat() =>
        Assert.Equal("{0}, last 2 checks: 1 to 2 %, now 2 %", Trend.Of([1, 2], "{0}", " %")!.Title);

    // ------------------------------------------------------------------------- Of: the two edges of the scale

    [Theory]
    [InlineData(new double[] { -5, -1 })]
    [InlineData(new double[] { -1, 0 })]
    [InlineData(new double[] { -0.0, -0.0 })]
    public void ARangeThatNeverRisesAboveZeroIsDrawnFlatAtTheBottom(double[] readings) =>
        Assert.Equal([0d, 0d], Line(readings).Points);

    [Fact]
    public void ARangeThatNeverRisesAboveZeroStillSaysItsOwnNumbers() =>
        Assert.Equal("x, last 2 checks: -5 to -1, now -1", Line([-5, -1]).Title);

    [Fact]
    public void AReadingBelowTheLargestSitsAtTheBottomRatherThanBelowIt() =>
        Assert.Equal([0d, 0d, 1], Line([-10, 0, 10]).Points);

    [Theory]
    [InlineData(new double[] { 7, 7, 7 })]
    [InlineData(new double[] { 0.5, 0.5 })]
    public void ReadingsThatNeverMoveAreAFlatLineAtTheTop(double[] readings)
    {
        var points = Line(readings).Points;

        Assert.Equal(readings.Length, points.Count);
        Assert.All(points, height => Assert.Equal(1, height));
    }

    [Fact]
    public void TheSmallestOfAHugeRangeIsRoundedOntoTheBottom()
    {
        var trend = Line([1e-10, 1e9]);

        Assert.Equal([0d, 1], trend.Points);
        Assert.Equal("x, last 2 checks: 0 to 1000000000, now 1000000000", trend.Title);
    }

    [Fact]
    public void AThirdDecimalIsKeptAndAMidpointOfItIsRoundedToEven()
    {
        // The scale keeps three decimals of a reading's share of the largest; one sixteenth is the midpoint of the
        // third, and the share is rounded to the even side of it, which is the side the component then draws.
        Assert.Equal([0.333, 1], Line([1, 3]).Points);
        Assert.Equal([0.062, 1], Line([1, 16]).Points);
    }

    // ----------------------------------------------------------------------- Of: readings that are no number

    [Fact]
    public void AnInfiniteReadingIsLeftOutLikeTheOneThatIsNoNumber()
    {
        var trend = Line([1, double.PositiveInfinity, 3]);

        Assert.Equal([0.333, 1], trend.Points);
        Assert.Contains("last 2 checks", trend.Title, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(new double[] { double.NaN })]
    [InlineData(new double[] { double.PositiveInfinity })]
    [InlineData(new double[] { double.NegativeInfinity, double.PositiveInfinity })]
    public void ReadingsThatAreNoNumberLeaveFewerThanTwoBehind(double[] readings) =>
        Assert.Null(Trend.Of(readings.Select(value => (double?)value), "x"));

    // -------------------------------------------------------------------------- Of: how many readings it takes

    [Fact]
    public void MoreReadingsThanADrawnLineIsLongAreEveryOneOfThemDrawn()
    {
        // The cap on a line belongs to the buffers that keep the readings, not to the drawing of one.
        var trend = Trend.Of(Enumerable.Range(0, Trend.Length + 1).Select(index => (double?)index), "x")!;

        Assert.Equal(Trend.Length + 1, trend.Points.Count);
        Assert.Equal([0d, 1], [trend.Points[0], trend.Points[^1]]);
        Assert.Contains($"last {Trend.Length + 1} checks", trend.Title, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------------------- the box the line is in

    [Fact]
    public void TheDefaultInsetIsOneAwayFromEveryEdge()
    {
        var trend = Line([0, 5, 10]);

        Assert.Equal("1,15 24,8 47,1", trend.Polyline(48, 16));
        Assert.Equal((47, 1), trend.Last(48, 16));
        Assert.Equal(trend.Polyline(48, 16, 1), trend.Polyline(48, 16));
        Assert.Equal(trend.Last(48, 16, 1), trend.Last(48, 16));
    }

    [Fact]
    public void NoInsetLetsTheLineAndTheDotTouchTheBox()
    {
        var trend = Line([0, 40]);

        Assert.Equal("0,16 52,0", trend.Polyline(52, 16, 0));
        Assert.Equal((52, 0), trend.Last(52, 16, 0));
    }

    [Fact]
    public void AnInsetOfHalfTheHeightLeavesNoRoomAndFlattensTheLineOntoTheMiddle()
    {
        var trend = Line([0, 10]);

        Assert.Equal("8,8 44,8", trend.Polyline(52, 16, 8));
        Assert.Equal((44, 8), trend.Last(52, 16, 8));
    }

    [Fact]
    public void ABoxOfNothingStillSaysOnePairPerReading() => Assert.Equal("0,0 0,0", Line([0, 10]).Polyline(0, 0, 0));

    [Theory]
    [InlineData(new double[] { 0, 2 }, "2,14 50,-10")]
    [InlineData(new double[] { -1, 1 }, "2,26 50,2")]
    public void AHeightOutsideTheScaleIsDrawnOutsideTheBox(double[] heights, string pairs) =>
        Assert.Equal(pairs, Drawn(heights).Polyline(52, 16, 2));

    [Fact]
    public void TheNowDotFollowsTheLineOutOfTheBox() => Assert.Equal((50, -10), Drawn(new double[] { 0, 2 }).Last(52, 16, 2));

    [Fact]
    public void AtAMidpointTheLineRoundsAwayFromZeroWhereTheDotRoundsToEven()
    {
        // The line writes its numbers with "0.##", which rounds a midpoint away from zero; the dot rounds its two
        // numbers with Math.Round, which rounds a midpoint to even. An eighth of a box, 0.125, is the number that
        // shows it: the line says 0.13 and the dot 0.12 for the very same place.
        var trend = Drawn(new double[] { 0, 1 });

        Assert.Equal("1,1.5 0.13,1", trend.Polyline(1.125, 2.5, 1));
        Assert.Equal((0.12, 1), trend.Last(1.125, 2.5, 1));
    }

    // ------------------------------------------------------------------------- no heights at all, one height

    [Fact]
    public void ALineOfNoHeightsSaysNoPointAtAll() => Assert.Equal(string.Empty, Drawn([]).Polyline(52, 16, 2.5));

    [Fact]
    public void TheNowOfNoHeightsIsRefusedAsTheIndexItIs()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Drawn(new List<double>()).Last(52, 16, 2.5));
        Assert.Throws<ArgumentOutOfRangeException>(() => Drawn(Array.Empty<double>()).Last(52, 16, 2.5));
    }

    [Fact]
    public void AHeightAloneIsDrawnAtTheLeftWhileTheNowDotStaysAtTheRight()
    {
        // One height is no step, so the single point of the line sits at the left inset while the dot marks the right:
        // the agreement of a line's last pair with the dot is an agreement of two heights or more.
        var one = Drawn(new List<double> { 0.5 });

        Assert.Equal("2.5,8", one.Polyline(52, 16, 2.5));
        Assert.Equal((49.5, 8), one.Last(52, 16, 2.5));
    }

    // --------------------------------------------------------------------------------------- the two parts

    [Fact]
    public void TheRecordRefusesNothingItIsBuiltWith()
    {
        var nothing = new Trend(null!, null!);

        Assert.Null(nothing.Points);
        Assert.Null(nothing.Title);

        var empty = new Trend(new List<double>(), string.Empty);

        Assert.Empty(empty.Points);
        Assert.Equal(string.Empty, empty.Title);
        Assert.NotEqual(nothing, empty);

        var heights = new List<double> { 0 };

        Assert.NotEqual(new Trend(heights, string.Empty), new Trend(heights, null!));
        Assert.Equal([-1d, 0, 2], new Trend(new List<double> { -1, 0, 2 }, "x").Points);
    }

    [Fact]
    public void TheHeightsAreWeighedAsTheListTheyAreHeldIn()
    {
        var one = new Trend(new List<double> { 0, 1 }, "x");
        var two = new Trend(new List<double> { 0, 1 }, "x");

        Assert.Equal(one.Points, two.Points);
        Assert.NotEqual(one, two);
        Assert.False(one == two);
        Assert.Equal(2, new HashSet<Trend> { one, two }.Count);

        var shared = new Trend(one.Points, "x");

        Assert.True(one == shared);
        Assert.Equal(one.GetHashCode(), shared.GetHashCode());
        Assert.Single(new HashSet<Trend> { one, shared });

        // A list and an array of the same heights are two parts.
        Assert.NotEqual(one, new Trend(new double[] { 0, 1 }, "x"));
    }

    [Fact]
    public void NoTrendIsOneWithNoTrendAndATrendIsNotItsParts()
    {
        var trend = Drawn(new List<double> { 0, 1 });
        Trend? none = null;
        Trend? alsoNone = null;

        Assert.True(none == alsoNone);
        Assert.False(none == trend);
        Assert.True(trend != none);
        Assert.False(trend.Equals(null));
        Assert.False(trend.Equals((object?)null));
        Assert.False(trend.Equals("x"));
        Assert.False(trend.Equals(trend.Points));
        Assert.False(trend.Equals(new RuntimeTrend(trend.Points, "x")));
        Assert.True(trend.Equals((object)new Trend(trend.Points, "x")));
    }

    [Fact]
    public void WithChangesOnlyThePartItNames()
    {
        var trend = Drawn(new List<double> { 0, 1 });

        var copy = trend with { };

        Assert.Equal(trend, copy);
        Assert.NotSame(trend, copy);
        Assert.Same(trend.Points, copy.Points);

        var retitled = trend with { Title = "SQL commands per minute" };

        Assert.Same(trend.Points, retitled.Points);
        Assert.Equal("SQL commands per minute", retitled.Title);

        var emptied = trend with { Points = [] };

        Assert.Empty(emptied.Points);
        Assert.Equal("x", emptied.Title);
        Assert.Equal(2, trend.Points.Count);
    }

    [Fact]
    public void TheTwoPartsFallOutInTheOrderTheyWentIn()
    {
        var heights = new List<double> { 0, 1 };
        var trend = new Trend(heights, "x");
        var (points, title) = trend;

        Assert.Same(heights, points);
        Assert.Equal("x", title);
        Assert.Equal(trend, new Trend(points, title));

        var (noPoints, noTitle) = new Trend(null!, null!);

        Assert.Null(noPoints);
        Assert.Null(noTitle);
    }

    [Fact]
    public void ATrendSaysItsTwoPartsInTheirOrderWhateverTheReaderSpeaks()
    {
        const string Full = "Trend { Points = System.Collections.Generic.List`1[System.Double], Title = x }";

        Assert.Equal(Full, new Trend(new List<double> { 0, 1 }, "x").ToString());
        Assert.Equal("Trend { Points = , Title =  }", new Trend(null!, null!).ToString());
        Assert.Equal("Trend { Points = System.Double[], Title = x }", new Trend(new double[] { 0 }, "x").ToString());
        Assert.Equal(Full, CultureScope.Says("de-DE", () => new Trend(new List<double> { 0, 1 }, "x").ToString()));
    }

    // -------------------------------------------------------------------------------- the surface as it stands

    [Fact]
    public void TheTrendHasItsLengthItsFactoryItsTwoDrawsItsTwoPartsAndTheRecordMembers()
    {
        Assert.True(RecordShape.IsPlainSealedRecord<Trend>());
        Assert.Same(typeof(object), typeof(Trend).BaseType);
        Assert.Equal([typeof(IEquatable<Trend>)], typeof(Trend).GetInterfaces());

        Assert.Equal(
            [
                "Deconstruct", "Equals", "GetHashCode", "Last", "Length", "Of", "Points", "Polyline", "Title",
                "ToString", "op_Equality", "op_Inequality",
            ],
            RecordSurface.WithFieldsOf(typeof(Trend)));

        Assert.Equal(["Points", "Title"], [.. RecordSurface.PropertyNamesOf<Trend>()]);
        RecordSurface.AssertPartsAreInitOnly(typeof(Trend), "Points", "Title");

        // The length is a fixed number on the type; its value, and the two buffers that name it, are pinned by
        // SampleWindowIsNamedTests.
        var length = typeof(Trend).GetField(nameof(Trend.Length))!;

        Assert.True(length.IsStatic);
        Assert.True(length.IsLiteral);
        Assert.Equal(typeof(int), length.FieldType);

        Assert.Equal(
            [
                new RecordParameter("Points", typeof(IReadOnlyList<double>), Nullable: false, Optional: false),
                new RecordParameter("Title", typeof(string), Nullable: false, Optional: false),
            ],
            RecordShape.ConstructorOf<Trend>());
    }

    [Fact]
    public void TheFactoryTakesTheReadingsAndWordsThemAndMayAnswerNothing()
    {
        var of = typeof(Trend).GetMethod(nameof(Trend.Of))!;

        Assert.True(of.IsStatic);
        Assert.Equal(typeof(Trend), of.ReturnType);
        Assert.True(new NullabilityInfoContext().Create(of.ReturnParameter).ReadState is NullabilityState.Nullable);
        Assert.Equal(
            [
                new RecordParameter("readings", typeof(IEnumerable<double?>), Nullable: false, Optional: false),
                new RecordParameter("what", typeof(string), Nullable: false, Optional: false),
                new RecordParameter("unit", typeof(string), Nullable: false, Optional: true),
            ],
            [.. of.GetParameters().Select(parameter => new RecordParameter(
                parameter.Name!,
                parameter.ParameterType,
                RecordSurface.IsNullable(parameter),
                parameter.IsOptional || parameter.HasDefaultValue))]);
        Assert.Equal(string.Empty, of.GetParameters()[2].DefaultValue);
    }

    [Fact]
    public void TheTwoDrawsTakeABoxAndDefaultToAnInsetOfOne()
    {
        var polyline = typeof(Trend).GetMethod(nameof(Trend.Polyline))!;
        var last = typeof(Trend).GetMethod(nameof(Trend.Last))!;

        Assert.Equal(typeof(string), polyline.ReturnType);
        Assert.Equal(typeof(ValueTuple<double, double>), last.ReturnType);

        Assert.All(new[] { polyline, last }, draw =>
        {
            Assert.False(draw.IsStatic);
            Assert.Equal(["width", "height", "inset"], [.. draw.GetParameters().Select(parameter => parameter.Name!)]);
            Assert.All(draw.GetParameters(), parameter => Assert.Equal(typeof(double), parameter.ParameterType));
            Assert.False(draw.GetParameters()[0].HasDefaultValue);
            Assert.False(draw.GetParameters()[1].HasDefaultValue);
            Assert.True(draw.GetParameters()[2].HasDefaultValue);
            Assert.Equal(1d, draw.GetParameters()[2].DefaultValue);
        });
    }
}
