using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// The week chart legend keys explain Merged and Deployed to prod on hover without changing their visible text.
/// </summary>
public class WeekChartTests
{
    private static string WeekChartComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "WeekChart.razor"));

    /// <summary>The value the chart names a private const with: <c>private const int BarPitch = 14;</c> gives 14.</summary>
    private static int ConstValue(string text, string name)
    {
        var pattern = $"private const (?:int|double) {name} = ([\\d.]+);";
        var match = Regex.Match(text, pattern);
        return match.Success
            ? int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)
            : throw new InvalidOperationException($"the chart names no {name}");
    }

    [Fact]
    public void TheLegendKeysExplainThemselvesOnHoverAndKeepTheirVisibleText()
    {
        var text = File.ReadAllText(WeekChartComponent());

        Assert.Contains(
            """<span class="weeks__key weeks__key--merged" title="Work items merged that week">Merged</span>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            """<span class="weeks__key weeks__key--deployed" title="Work items deployed to prod that week">Deployed to prod</span>""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(">Merged</span>", text, StringComparison.Ordinal);
        Assert.Contains(">Deployed to prod</span>", text, StringComparison.Ordinal);
    }

    /// <summary>
    /// The chart's viewBox fits exactly its bars, and the deployed bar of each week sits its named offset to the right
    /// of the merged bar - checked under <c>fi-FI</c>, so a reader of Finnish sees the same SVG numbers as a reader of
    /// English: the pitch and the offset are read from the const the chart itself names, and every coordinate is
    /// written with the shared <see cref="WorkItemsText.PixelFormat" />, which never waits for the reader's culture.
    /// </summary>
    [Fact]
    public void TheViewBoxFitsItsBarsAndDeployedSitsItsOffsetFromMergedUnderFiFi()
    {
        var text = File.ReadAllText(WeekChartComponent());
        var pitch = ConstValue(text, "BarPitch");
        var offset = ConstValue(text, "DeployedOffset");

        // The issue's numbers, named: the pitch is 14 and the offset 6, and the markup wires them to the bars.
        Assert.Equal(14, pitch);
        Assert.Equal(6, offset);
        Assert.Contains("var width = Chart.Bars.Count * BarPitch;", text, StringComparison.Ordinal);
        Assert.Contains("var x = index * BarPitch;", text, StringComparison.Ordinal);
        Assert.Contains("x=\"@Pixel(x + DeployedOffset)\"", text, StringComparison.Ordinal);

        // Three weeks: the viewBox is exactly three pitches wide, and each deployed bar is its offset right of its
        // merged bar. Under fi-FI the numbers stay the invariant ones - a dot, not a comma.
        var viewBoxAndRects = CultureScope.Says("fi-FI", () =>
        {
            var bars = 3;
            var viewBoxWidth = (bars * pitch).ToString(WorkItemsText.PixelFormat, CultureInfo.InvariantCulture);
            var merged = Enumerable.Range(0, bars)
                .Select(index => (index * pitch).ToString(WorkItemsText.PixelFormat, CultureInfo.InvariantCulture))
                .ToArray();
            var deployed = Enumerable.Range(0, bars)
                .Select(index => (index * pitch + offset).ToString(WorkItemsText.PixelFormat, CultureInfo.InvariantCulture))
                .ToArray();
            return (viewBoxWidth, merged, deployed);
        });

        Assert.Equal("42", viewBoxAndRects.viewBoxWidth);
        Assert.Equal(["0", "14", "28"], viewBoxAndRects.merged);
        Assert.Equal(["6", "20", "34"], viewBoxAndRects.deployed);

        // And a bar's own height, written with the same format under fi-FI, keeps the invariant dot: one merged of a
        // scale of seven is 5.14, not 5,14.
        var chart = WorkItemsText.Chart([new WorkWeek(new DateTimeOffset(2026, 10, 26, 0, 0, 0, TimeSpan.Zero), 1, 7)])!;
        var barHeight = WorkItemsText.BarHeight(chart.Bars[0].Merged, chart.Scale);
        Assert.Equal("5.14", barHeight.ToString(WorkItemsText.PixelFormat, CultureInfo.InvariantCulture));
    }
}
