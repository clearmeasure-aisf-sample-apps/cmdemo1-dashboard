using System.Globalization;

namespace Dashboard.Health;

/// <summary>
/// A count and the noun it counts, as one phrase: <c>1 restart</c>, <c>7 restarts</c>. Only a count of exactly one
/// takes the singular; every other count - zero, two, the ends of the number - takes the plural, which by default is
/// the singular with an <c>s</c> on it, and which a noun that does not pluralise that way is given as the third word.
/// The words are the caller's and are counted in the invariant culture, so neither the noun chosen nor the separator
/// of a long count changes with whoever looks.
/// </summary>
public static class Plural
{
    /// <summary>
    /// A count a page makes of the things it can see, written digit by digit with no separator - the way
    /// <see cref="ClusterText.Restarts(int)" /> writes restarts.
    /// </summary>
    /// <param name="count">How many there are.</param>
    /// <param name="singular">The noun of the one: <c>restart</c>.</param>
    /// <param name="plural">The noun of every other count, when it is not the singular plus an <c>s</c>: <c>assemblies</c>.</param>
    public static string Of(int count, string singular, string? plural = null) =>
        count == 1 ? string.Create(CultureInfo.InvariantCulture, $"1 {singular}") : string.Create(CultureInfo.InvariantCulture, $"{count} {plural ?? singular + "s"}");

    /// <summary>
    /// A count a database reports of the rows of a table, grouped in thousands the way
    /// <see cref="DatabaseText.Rows(long)" /> groups them - the one that is exactly one staying singular all the same.
    /// </summary>
    /// <param name="count">How many there are.</param>
    /// <param name="singular">The noun of the one: <c>row</c>.</param>
    /// <param name="plural">The noun of every other count, when it is not the singular plus an <c>s</c>.</param>
    public static string Of(long count, string singular, string? plural = null) =>
        count == 1 ? string.Create(CultureInfo.InvariantCulture, $"{count:#,0} {singular}") : string.Create(CultureInfo.InvariantCulture, $"{count:#,0} {plural ?? singular + "s"}");
}
