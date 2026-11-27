using System.Globalization;

namespace Dashboard.Health;

/// <summary>
/// A sparkline: the last readings of one number, oldest first, each as its height between 0 (the bottom: zero) and 1
/// (the largest reading), with the same in words for readers who do not see it. The scale starts at zero, so a line
/// that stays low is a number that stays small.
/// </summary>
/// <param name="Points">One height per reading, 0 to 1.</param>
/// <param name="Title">The text alternative: <c>requests per minute, last 12 checks: 0 to 45, now 12</c>.</param>
public sealed record Trend(IReadOnlyList<double> Points, string Title)
{
    /// <summary>
    /// How many readings a sparkline is drawn over: half an hour at the default interval. What a buffer keeps is named
    /// where it is kept (<see cref="TargetStatus.SampleLength"/>, <see cref="Cluster.ClusterMonitor.SampleLength"/>).
    /// </summary>
    public const int Length = 60;

    /// <summary>
    /// The trend of the readings that are there (a round without a reading is left out); null with fewer than two,
    /// which make no line.
    /// </summary>
    /// <param name="what">What the number is: <c>requests per minute</c>.</param>
    /// <param name="unit">The unit after the numbers of the title, with its leading space or empty.</param>
    public static Trend? Of(IEnumerable<double?> readings, string what, string unit = "")
    {
        ArgumentNullException.ThrowIfNull(readings);
        var values = readings.OfType<double>().Where(double.IsFinite).ToList();
        if (values.Count < 2)
        {
            return null;
        }

        var max = values.Max();
        var points = values.Select(value => Height(value, max)).ToList();
        var title = string.Create(
            CultureInfo.InvariantCulture,
            $"{what}, last {values.Count} checks: {Number(values.Min())} to {Number(max)}{unit}, now {Number(values[^1])}{unit}");
        return new Trend(points, title);
    }

    /// <summary>The points of an SVG polyline in a box of the given size, with the line kept inside by <paramref name="inset"/>.</summary>
    public string Polyline(double width, double height, double inset = 1)
    {
        var step = Points.Count > 1 ? (width - 2 * inset) / (Points.Count - 1) : 0;
        return string.Join(' ', Points.Select((point, index) => string.Create(
            CultureInfo.InvariantCulture,
            $"{Coordinate(inset + index * step)},{Coordinate(YOf(point, height, inset))}")));
    }

    /// <summary>Where the last reading is in that box: the dot that marks "now".</summary>
    public (double X, double Y) Last(double width, double height, double inset = 1) =>
        (Math.Round(width - inset, CoordinateDigits), Math.Round(YOf(Points[^1], height, inset), CoordinateDigits));

    /// <summary>
    /// The SVG y of a point in a box of the given height: 0 is the bottom of the box and 1 the top, with the line kept
    /// <paramref name="inset"/> away from both edges.
    /// </summary>
    private static double YOf(double point, double height, double inset) => height - inset - point * (height - 2 * inset);

    /// <summary>
    /// The 0 to 1 height of one reading: the reading over the largest, clamped to the box; 0 when there is no
    /// largest to divide by (every reading is zero).
    /// </summary>
    private static double Height(double value, double max) => max > 0 ? Math.Round(Math.Clamp(value / max, 0, 1), HeightDigits) : 0;

    /// <summary>
    /// How many decimals a point's 0 to 1 height keeps: finer than <see cref="CoordinateDigits"/>, so the SVG
    /// coordinates do the final rounding.
    /// </summary>
    private const int HeightDigits = 3;

    /// <summary>At most one decimal; a whole reading is written without one.</summary>
    private const string NumberFormat = "0.#";

    private static string Number(double value) => value.ToString(NumberFormat, CultureInfo.InvariantCulture);

    /// <summary>
    /// How many decimals an SVG coordinate keeps: the polyline and the dot must round alike, or the dot leaves the line.
    /// </summary>
    private const int CoordinateDigits = 2;

    /// <summary>
    /// Writes <see cref="CoordinateDigits"/> decimals at most (a whole or exact coordinate is written without trailing
    /// zeros); must change with <see cref="CoordinateDigits"/>, or the polyline and the dot no longer agree.
    /// </summary>
    private const string CoordinateFormat = "0.##";

    private static string Coordinate(double value) => value.ToString(CoordinateFormat, CultureInfo.InvariantCulture);
}
