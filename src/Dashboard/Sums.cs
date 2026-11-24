namespace Dashboard;

/// <summary>
/// The cluster's one rule for adding numbers some of which are unknown: add what is known, and say nothing (null) when
/// nothing is known. A zero is known; a null is not. <see cref="Cluster.NamespaceGroup" />, <see cref="Cluster.ClusterTotals" /> and
/// <see cref="Cluster.ClusterSample" /> all add this way.
/// </summary>
internal static class Sums
{
    /// <summary>The known numbers added in order from zero; null when none is known.</summary>
    internal static double? Of(IEnumerable<double?> numbers)
    {
        ArgumentNullException.ThrowIfNull(numbers);
        var known = numbers.OfType<double>().ToList();
        return known.Count == 0 ? null : known.Sum();
    }

    /// <summary>The known counts added in an <see cref="int" />; null when none is known.</summary>
    /// <exception cref="OverflowException">The counts add up to more than an <see cref="int" /> holds.</exception>
    internal static int? Of(IEnumerable<int?> counts)
    {
        ArgumentNullException.ThrowIfNull(counts);
        var known = counts.OfType<int>().ToList();
        return known.Count == 0 ? null : known.Sum();
    }

    /// <summary>
    /// The sum <see cref="Of(IEnumerable{double?})" /> gives, kept one number at a time: <paramref name="number" />
    /// added to <paramref name="sum" /> when it is known, and the sum left as it was (null until one is known) when not.
    /// </summary>
    internal static double? Add(double? sum, double? number) =>
        number is { } known ? (sum ?? 0d) + known : sum;
}
