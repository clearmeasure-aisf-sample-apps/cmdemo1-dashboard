using Dashboard.Health;

namespace Dashboard.Work;

/// <summary>The metered Cursor agent cost of the work items, in words. <see cref="WorkItemsText"/> says them.</summary>
internal static class WorkCostText
{
    /// <summary>The middle dot that joins a cost line's parts: <c>$2.00 per item · $18.00 total</c>.</summary>
    private const string PartSeparator = " · ";

    /// <summary>The word before the per-item amount.</summary>
    private const string PerItem = "per item";

    /// <summary>The word before the total amount.</summary>
    private const string Total = "total";

    /// <summary>True when at least one item carries a metered cost; a note without an amount is not a cost source.</summary>
    internal static bool HasMeteredCost(IReadOnlyList<WorkItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return items.Any(item => item.CostUsd is not null);
    }

    /// <summary>
    /// <c>$2.00 per item · $18.00 total</c> when at least one item was metered; null otherwise. The published total
    /// wins over the sum of the item amounts, and the file's note is kept only with a published total.
    /// </summary>
    internal static string? Cost(WorkCost? cost, IReadOnlyList<WorkItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (!HasMeteredCost(items))
        {
            return null;
        }

        var total = cost?.TotalUsd ?? items.Sum(item => item.CostUsd ?? 0);
        var amount = Dollars(total);
        var text = $"{Dollars(total / items.Count)} {PerItem}{PartSeparator}{amount} {Total}";
        return cost?.TotalUsd is not null && cost.Note is { } why ? $"{text}{PartSeparator}{why}" : text;
    }

    /// <summary>What an item's own cost cell says: the amount, or <see cref="WorkItemsText.NotMetered"/>.</summary>
    internal static string ItemCost(double? costUsd) => costUsd is { } cost ? Dollars(cost) : WorkItemsText.NotMetered;

    /// <summary>A dollar amount in words: <c>$2.00</c>, never <c>2.00 USD</c> - the work page bills in dollars.</summary>
    private static string Dollars(double? amount) => CostText.Money(amount, CostText.Dollars);
}
