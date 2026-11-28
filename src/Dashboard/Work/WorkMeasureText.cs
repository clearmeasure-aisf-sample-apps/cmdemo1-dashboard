using System.Globalization;
using Dashboard.Health;

namespace Dashboard.Work;

/// <summary>The duration and rework measures of the stats strip, in words. <see cref="WorkItemsText"/> says them.</summary>
internal static class WorkMeasureText
{
    /// <summary>A rate at or below this is a fraction (see <see cref="WorkRework.Rate"/>) and is shown as a percent.</summary>
    private const double LargestFraction = 1.0;

    /// <summary>A percent keeps this many decimals; <see cref="PercentFormat"/> says the same.</summary>
    private const int PercentDecimals = 1;

    /// <summary>A percent is shown to one decimal, without a trailing zero.</summary>
    private const string PercentFormat = "0.#";

    /// <summary>The unit a percent rate is written with, after a space: <c>12.5 %</c>.</summary>
    private const string PercentSuffix = " %";

    /// <summary>The middle dot that joins a measure's parts: <c>median 18.5 h · p85 3 d · 11 items</c>.</summary>
    private const string PartSeparator = " · ";

    /// <summary>A rate above one is shown as the file gave it, to two decimals.</summary>
    private const string KeptFormat = "0.##";

    internal static string Duration(WorkDuration? duration)
    {
        if (duration is null)
        {
            return WorkItemsText.NotMetered;
        }

        var parts = new List<string>();
        if (duration.Median is { } median)
        {
            parts.Add($"median {DeliveryText.LeadTime(median)}");
        }

        if (duration.P85 is { } p85)
        {
            parts.Add($"p85 {DeliveryText.LeadTime(p85)}");
        }

        if (duration.N is { } counted)
        {
            parts.Add(Plural.Of(counted, "item"));
        }

        return Joined(parts);
    }

    internal static string Rework(WorkRework? rework)
    {
        if (rework is null)
        {
            return WorkItemsText.NotMetered;
        }

        var parts = new List<string>();
        if (rework.FtFailures is { } failures)
        {
            parts.Add(Plural.Of(failures, "functional-test failure"));
        }

        if (rework.Reverts is { } reverts)
        {
            parts.Add(Plural.Of(reverts, "revert"));
        }

        if (rework.Rate is { } rate)
        {
            parts.Add(Rate(rate));
        }

        return Joined(parts);
    }

    /// <summary>The parts a measure has, joined by <see cref="PartSeparator"/>; <see cref="WorkItemsText.NotMetered"/> when it has none.</summary>
    private static string Joined(List<string> parts) => parts.Count == 0 ? WorkItemsText.NotMetered : string.Join(PartSeparator, parts);

    private static string Rate(double rate) =>
        rate <= LargestFraction
            ? Math.Round(Shares.Of(rate, LargestFraction), PercentDecimals).ToString(PercentFormat, CultureInfo.InvariantCulture) + PercentSuffix
            : rate.ToString(KeptFormat, CultureInfo.InvariantCulture);
}
