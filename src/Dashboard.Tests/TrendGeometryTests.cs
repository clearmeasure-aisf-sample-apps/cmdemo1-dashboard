using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// Pins where <see cref="Trend.Polyline"/> and <see cref="Trend.Last"/> put a point in the box, in the box
/// <c>TrendLine.razor</c> draws (52 x 16, inset 2.5), so the "now" dot stays on the end of the line.
/// </summary>
public class TrendGeometryTests
{
    private const double Width = 52;
    private const double Height = 16;
    private const double Inset = 2.5;

    private static string Text(double value) => value.ToString(CultureInfo.InvariantCulture);

    [Theory]
    [InlineData(new double[] { 0, 10, 40, 20 })]
    [InlineData(new double[] { 3, 7, 9, 4 })]
    [InlineData(new double[] { 9, 1, 3 })]
    [InlineData(new double[] { 5, 5, 0 })]
    public void TheLastPairOfTheLineIsTheNowDotInTheComponentsBox(double[] readings)
    {
        var trend = Trend.Of(readings.Select(value => (double?)value), "x")!;

        var lastPair = trend.Polyline(Width, Height, Inset).Split(' ')[^1];
        var (x, y) = trend.Last(Width, Height, Inset);

        Assert.Equal($"{Text(x)},{Text(y)}", lastPair);
    }

    [Fact]
    public void AReadingOfZeroSitsInsetAboveTheBottomAndTheLargestInsetBelowTheTop()
    {
        var rising = Trend.Of([0, 40], "x")!;
        var falling = Trend.Of([40, 0], "x")!;

        Assert.Equal("2.5,13.5 49.5,2.5", rising.Polyline(Width, Height, Inset));
        Assert.Equal((49.5, Inset), rising.Last(Width, Height, Inset));
        Assert.Equal("2.5,2.5 49.5,13.5", falling.Polyline(Width, Height, Inset));
        Assert.Equal((49.5, Height - Inset), falling.Last(Width, Height, Inset));
    }

    [Fact]
    public void AFractionalInsetGivesTheseExactPoints()
    {
        var trend = Trend.Of([0, 10, 40, 20], "x")!;

        Assert.Equal("2.5,13.5 18.17,10.75 33.83,2.5 49.5,8", trend.Polyline(Width, Height, Inset));
        Assert.Equal((49.5, 8), trend.Last(Width, Height, Inset));
    }

    [Fact]
    public void PointsWithThreeDecimalsAreRoundedToTwoInBothMembers()
    {
        var trend = Trend.Of([3, 7, 9, 3], "x")!;

        Assert.Equal("2.5,9.84 18.17,4.94 33.83,2.5 49.5,9.84", trend.Polyline(Width, Height, Inset));
        Assert.Equal((49.5, 9.84), trend.Last(Width, Height, Inset));
    }

    [Fact]
    public void AnXStepWithThreeDecimalsStillPrintsTwoAndTheDotStillMatchesTheLineEnd()
    {
        // Four points across width 52 with inset 2.5 give a step of 47/3 = 18.1666…, so the named coordinate
        // constants, not the raw step, decide what is printed.
        var trend = Trend.Of([0, 10, 40, 20], "x")!;

        Assert.Equal("2.5,13.5 18.17,10.75 33.83,2.5 49.5,8", trend.Polyline(Width, Height, Inset));

        var lastPair = trend.Polyline(Width, Height, Inset).Split(' ')[^1];
        var (x, y) = trend.Last(Width, Height, Inset);
        Assert.Equal(x, double.Parse(lastPair.Split(',')[0], CultureInfo.InvariantCulture));
        Assert.Equal(y, double.Parse(lastPair.Split(',')[1], CultureInfo.InvariantCulture));
    }
}
