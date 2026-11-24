namespace Dashboard.Runtime;

/// <summary>A sparkline next to a number: heights from 0 to 1, oldest first, and the same in words.</summary>
public sealed record RuntimeTrend(IReadOnlyList<double> Points, string Title)
{
    public static RuntimeTrend? Of(Health.Trend? trend) => trend is null ? null : new RuntimeTrend(trend.Points, trend.Title);
}
