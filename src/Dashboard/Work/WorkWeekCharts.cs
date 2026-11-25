using System.Globalization;

namespace Dashboard.Work;

/// <summary>The weekly merged and deployed-to-prod chart: its bars, their heights, and the newest week in words.</summary>
internal static class WorkWeekCharts
{
    /// <summary>The height of the chart, in its own units. A bar of the largest count fills it.</summary>
    internal const double Height = 36;

    /// <summary>The weekly chart; null when the file gives no week.</summary>
    internal static WorkWeekChart? Chart(IReadOnlyList<WorkWeek> weeks)
    {
        ArgumentNullException.ThrowIfNull(weeks);
        if (weeks.Count == 0)
        {
            return null;
        }

        var bars = weeks.Select(Bar).ToList();
        var scale = Math.Max(1, bars.Max(bar => Math.Max(bar.Merged, bar.DeployedProd)));
        return new WorkWeekChart(bars, scale, "Merged and deployed to prod per week. " + string.Join(" ", bars.Select(bar => bar.Title)));
    }

    /// <summary>How tall a bar is in the chart's units. Zero stays zero, so a week with nothing is a gap, not a sliver.</summary>
    internal static double BarHeight(int value, int scale) =>
        scale <= 0 || value <= 0 ? 0 : Math.Round(Height * value / scale, 2);

    /// <summary>The newest week, in words under the chart.</summary>
    internal static string LatestWeek(WorkWeekChart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);
        var last = chart.Bars[^1];
        return $"{last.Label}: {last.Merged} merged, {last.DeployedProd} deployed to prod";
    }

    private static WorkWeekBar Bar(WorkWeek week)
    {
        var merged = week.Merged ?? 0;
        var deployed = week.DeployedProd ?? 0;
        var label = week.WeekStart is { } start
            ? string.Create(CultureInfo.InvariantCulture, $"Week of {start.UtcDateTime:yyyy-MM-dd}")
            : "Week";
        return new WorkWeekBar(
            label,
            merged,
            deployed,
            string.Create(CultureInfo.InvariantCulture, $"{label}: {merged} merged, {deployed} deployed to prod"));
    }
}
