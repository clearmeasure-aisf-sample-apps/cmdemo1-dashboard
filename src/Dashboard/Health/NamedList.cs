using System.Globalization;

namespace Dashboard.Health;

/// <summary>
/// A list a headline names only the first few of: the first <c>most</c> names, then how many more there are. Each
/// caller keeps its own number and its own words around the list (<c>Front Door rotates over ...</c>,
/// <c>And 2 more.</c>); this is only the cutting, the counting and the joining, written once.
/// </summary>
public static class NamedList
{
    /// <summary>
    /// The first <paramref name="most"/> of <paramref name="names"/>, in order. Only those are read, so a caller
    /// that words each item on the way in words no more than it names.
    /// </summary>
    public static IReadOnlyList<string> Named(IEnumerable<string> names, int most)
    {
        ArgumentNullException.ThrowIfNull(names);
        ArgumentOutOfRangeException.ThrowIfNegative(most);
        return [.. names.Take(most)];
    }

    /// <summary>The rest of <paramref name="count"/> items after the first <paramref name="most"/>, as words: <c>2 more</c>; null when none is left.</summary>
    public static string? More(int count, int most)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(most);
        return count > most ? string.Create(CultureInfo.InvariantCulture, $"{count - most} more") : null;
    }

    /// <summary>
    /// The first <paramref name="most"/> names, then <c>N more</c> when some are left, joined as a sentence does:
    /// <c>a</c>, <c>a and b</c>, <c>a, b and 2 more</c>.
    /// </summary>
    public static string Words(IReadOnlyList<string> names, int most)
    {
        var words = Named(names, most).ToList();
        if (More(names.Count, most) is { } more)
        {
            words.Add(more);
        }

        return Join(words);
    }

    /// <summary>Names joined as a sentence does: <c>a</c>, <c>a and b</c>, <c>a, b and c</c>; empty for none.</summary>
    public static string Join(IReadOnlyList<string> names)
    {
        ArgumentNullException.ThrowIfNull(names);
        return names.Count switch
        {
            0 => string.Empty,
            1 => names[0],
            _ => $"{string.Join(", ", names.Take(names.Count - 1))} and {names[^1]}",
        };
    }
}
