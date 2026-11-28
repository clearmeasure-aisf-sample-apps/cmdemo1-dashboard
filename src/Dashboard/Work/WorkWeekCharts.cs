using System.Globalization;

namespace Dashboard.Work;

/// <summary>The weekly merged and deployed-to-prod chart: its bars, their heights, and the newest week in words.</summary>
internal static class WorkWeekCharts
{
    /// <summary>The height of the chart, in its own units. A bar of the largest count fills it.</summary>
    internal const double Height = 36;

    /// <summary>
    /// How many decimals <see cref="BarHeight" /> keeps. The razor's <see cref="PixelFormat" /> writes exactly this
    /// many, so the two must agree: change one and the bar's drawn height no longer matches its number.
    /// </summary>
    internal const int HeightDecimals = 2;

    /// <summary>
    /// The format the razor writes every bar coordinate with: at most <see cref="HeightDecimals" /> decimals, a whole
    /// number without a point. It keeps exactly <see cref="HeightDecimals" /> places, so it must change with
    /// <see cref="HeightDecimals" /> or a bar's number and its drawn height disagree.
    /// </summary>
    internal const string PixelFormat = "0.##";

    /// <summary>The lead sentence of the chart's accessible title, before the bars' own sentences.</summary>
    private const string ChartLead = "Merged and deployed to prod per week. ";

    /// <summary>The separator between the bars' sentences in the chart's accessible title.</summary>
    private const string BarSeparator = " ";

    /// <summary>The lead of a bar's label for a week that has a start date.</summary>
    private const string WeekLead = "Week of ";

    /// <summary>The date format a week start is written in, under the invariant culture.</summary>
    private const string WeekStartFormat = "yyyy-MM-dd";

    /// <summary>The label of a bar whose week has no start date.</summary>
    private const string NoStartLabel = "Week";

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
        return new WorkWeekChart(bars, scale, ChartLead + string.Join(BarSeparator, bars.Select(bar => bar.Title)));
    }

    /// <summary>How tall a bar is in the chart's units. Zero stays zero, so a week with nothing is a gap, not a sliver.</summary>
    internal static double BarHeight(int value, int scale) =>
        scale <= 0 || value <= 0 ? 0 : Math.Round(Height * value / scale, HeightDecimals);

    /// <summary>
    /// The one sentence the bar tooltip, the line under the chart, and the bot throughput line all say: who, how
    /// many merged, how many deployed to prod. The caller names the culture, so the split between invariant (the bar
    /// tooltip) and current (the other two) is visible at each call site rather than hidden in which overload was used.
    /// </summary>
    internal static string MergedAndDeployed(IFormatProvider culture, string who, int merged, int deployed) =>
        string.Create(culture, $"{who}: {merged} merged, {deployed} deployed to prod");

    /// <summary>The newest week, in words under the chart.</summary>
    internal static string LatestWeek(WorkWeekChart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);
        var last = chart.Bars[^1];
        return MergedAndDeployed(CultureInfo.CurrentCulture, last.Label, last.Merged, last.DeployedProd);
    }

    private static WorkWeekBar Bar(WorkWeek week)
    {
        var merged = week.Merged ?? 0;
        var deployed = week.DeployedProd ?? 0;
        var label = week.WeekStart is { } start
            ? string.Create(CultureInfo.InvariantCulture, $"{WeekLead}{start.UtcDateTime.ToString(WeekStartFormat, CultureInfo.InvariantCulture)}")
            : NoStartLabel;
        return new WorkWeekBar(
            label,
            merged,
            deployed,
            MergedAndDeployed(CultureInfo.InvariantCulture, label, merged, deployed));
    }
}
