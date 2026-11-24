using System.Globalization;

namespace Dashboard.Health;

/// <summary>
/// The one rule a count and its noun follow, asked two ways: as one phrase (<see cref="Of(int, string, string?)" />,
/// <c>1 restart</c>, <c>7 restarts</c>) or as the noun on its own (<see cref="Noun(int, string, string?)" />), for
/// the sentences that weave the noun between their own other words. Only a count of exactly one takes the singular;
/// every other count - zero, two, the ends of the number - takes the plural, which by default is the singular with an
/// <c>s</c> on it, and which a noun that does not pluralise that way is given as the third word. The rule is said
/// once, below, and every member here asks it, so no caller has to write the singular/plural choice again.
/// <para>
/// The words are the caller's and are counted in the invariant culture, so neither the noun chosen nor the separator
/// of a long count changes with whoever looks.
/// </para>
/// </summary>
public static class Plural
{
    /// <summary>
    /// The noun of a count, without the count: <c>node</c> for one, <c>nodes</c> for every other count. For the
    /// sentences that already carry their number - <c>3 of 4 nodes not healthy</c>, <c>No nodes in the topology</c> -
    /// and so need only the word that agrees with it.
    /// </summary>
    /// <param name="count">How many there are.</param>
    /// <param name="singular">The noun of the one: <c>node</c>.</param>
    /// <param name="plural">The noun of every other count, when it is not the singular plus an <c>s</c>: <c>assemblies</c>.</param>
    public static string Noun(int count, string singular, string? plural = null) => NounOf(count, singular, plural);

    /// <summary>
    /// A count a page makes of the things it can see, written digit by digit with no separator - the way
    /// <see cref="ClusterText.Restarts(int)" /> writes restarts.
    /// </summary>
    /// <param name="count">How many there are.</param>
    /// <param name="singular">The noun of the one: <c>restart</c>.</param>
    /// <param name="plural">The noun of every other count, when it is not the singular plus an <c>s</c>: <c>assemblies</c>.</param>
    public static string Of(int count, string singular, string? plural = null) =>
        string.Create(CultureInfo.InvariantCulture, $"{count} {Noun(count, singular, plural)}");

    /// <summary>
    /// A count a database reports of the rows of a table, grouped in thousands the way
    /// <see cref="DatabaseText.Rows(long)" /> groups them - the one that is exactly one staying singular all the same.
    /// </summary>
    /// <param name="count">How many there are.</param>
    /// <param name="singular">The noun of the one: <c>row</c>.</param>
    /// <param name="plural">The noun of every other count, when it is not the singular plus an <c>s</c>.</param>
    public static string Of(long count, string singular, string? plural = null) =>
        string.Create(CultureInfo.InvariantCulture, $"{count:#,0} {NounOf(count, singular, plural)}");

    /// <summary>
    /// The rule itself, said once for the whole repository: a count of exactly one takes the singular, every other
    /// count takes the given plural or, with none given, the singular plus an <c>s</c>. Private, so that no caller
    /// can write the count-and-noun sentence or the noun alone without coming through this sentence.
    /// </summary>
    private static string NounOf(long count, string singular, string? plural) =>
        count == 1 ? singular : plural ?? singular + "s";
}
