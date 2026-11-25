using Dashboard.Health;

namespace Dashboard.Work;

/// <summary>The metered Cursor agent cost of the work items, in words. <see cref="WorkItemsText"/> says them.</summary>
internal static class WorkCostText
{
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
        var amount = CostText.Money(total, "USD");
        var text = $"{CostText.Money(total / items.Count, "USD")} per item · {amount} total";
        return cost?.TotalUsd is not null && cost.Note is { } why ? $"{text} · {why}" : text;
    }

    /// <summary>What an item's own cost cell says: the amount, or <see cref="WorkItemsText.NotMetered"/>.</summary>
    internal static string ItemCost(double? costUsd) => costUsd is { } cost ? CostText.Money(cost, "USD") : WorkItemsText.NotMetered;
}
