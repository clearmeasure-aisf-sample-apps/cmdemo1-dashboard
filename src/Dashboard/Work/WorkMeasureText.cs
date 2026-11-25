using System.Globalization;
using Dashboard.Health;

namespace Dashboard.Work;

/// <summary>
/// The stats strip's measures in words: cycle and lead time, rework, and the metered Cursor cost. Amounts are said
/// by <see cref="CostText"/>; <see cref="WorkItemsText"/> is the page's door to these.
/// </summary>
public static class WorkMeasureText
{
    /// <summary><c>median 18.5 h · p85 3 d · 11 items</c>, or <see cref="WorkItemsText.NotMetered"/> when the file gives none of it.</summary>
    public static string Duration(WorkDuration? duration)
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

        return parts.Count == 0 ? WorkItemsText.NotMetered : string.Join(" · ", parts);
    }

    /// <summary>
    /// <c>2 functional-test failures · 1 revert · 12.5 %</c>. <see cref="WorkRework.Rate"/> at most 1 is a fraction and
    /// is shown as a percent; a larger number is shown as the file gave it. <see cref="WorkItemsText.NotMetered"/> when
    /// nothing is there.
    /// </summary>
    public static string Rework(WorkRework? rework)
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

        return parts.Count == 0 ? WorkItemsText.NotMetered : string.Join(" · ", parts);
    }

    /// <summary>
    /// True when at least one item carries a metered Cursor agent cost. The cost tile is shown only then: a note
    /// without a metered amount is not a cost source.
    /// </summary>
    public static bool HasMeteredCost(IReadOnlyList<WorkItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return items.Any(item => item.CostUsd is not null);
    }

    /// <summary>
    /// <c>$2.00 per item · $18.00 total</c> when at least one of <paramref name="items"/> was metered. Null when none
    /// was: the tile stays off, and a note without a total is not shown as "n/a". Per item is the total divided by how
    /// many items the filter lists, Done included. The published total wins; without one the metered item amounts are
    /// summed. The file's note is kept only with a published total.
    /// </summary>
    public static string? Cost(WorkCost? cost, IReadOnlyList<WorkItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (!HasMeteredCost(items))
        {
            return null;
        }

        var total = cost?.TotalUsd ?? items.Sum(item => item.CostUsd ?? 0);
        var amount = CostText.Money(total, "USD");
        var text = $"{CostText.Money(total / items.Count, "USD")} per item · {amount} total";
        return cost?.TotalUsd is not null && cost.Note is { } why ? $"{text} · {why}" : text;
    }

    /// <summary>What an item's own cost cell says: the amount, or <see cref="WorkItemsText.NotMetered"/>.</summary>
    public static string ItemCost(double? costUsd) => costUsd is { } cost ? CostText.Money(cost, "USD") : WorkItemsText.NotMetered;

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

    private static string Rate(double rate) =>
        rate <= LargestFraction
            ? Math.Round(rate * PercentPerWhole, PercentDecimals).ToString(PercentFormat, CultureInfo.InvariantCulture) + " %"
            : rate.ToString(KeptFormat, CultureInfo.InvariantCulture);
}
