using System.Globalization;
using Dashboard.Health;

namespace Dashboard.Work;

/// <summary>The duration and rework measures of the stats strip, in words. <see cref="WorkItemsText"/> says them.</summary>
internal static class WorkMeasureText
{
    /// <summary>A rate at or below this is a fraction (see <see cref="WorkRework.Rate"/>) and is shown as a percent.</summary>
    private const double LargestFraction = 1.0;

    /// <summary>The percent scale: a whole is this many percent.</summary>
    private const double PercentPerWhole = 100.0;

    /// <summary>A percent keeps this many decimals; <see cref="PercentFormat"/> says the same.</summary>
    private const int PercentDecimals = 1;

    /// <summary>A percent is shown to one decimal, without a trailing zero.</summary>
    private const string PercentFormat = "0.#";

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

    /// <summary>The parts a measure has, joined by a middle dot; <see cref="WorkItemsText.NotMetered"/> when it has none.</summary>
    private static string Joined(List<string> parts) => parts.Count == 0 ? WorkItemsText.NotMetered : string.Join(" · ", parts);

    private static string Rate(double rate) =>
        rate <= LargestFraction
            ? Math.Round(rate * PercentPerWhole, PercentDecimals).ToString(PercentFormat, CultureInfo.InvariantCulture) + " %"
            : rate.ToString(KeptFormat, CultureInfo.InvariantCulture);
}
