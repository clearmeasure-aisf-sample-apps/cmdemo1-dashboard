namespace Dashboard.Health;

/// <summary>
/// The parts of one line of a card, collected in the order they are said: a part the build did not measure is given
/// as null and is left out, so the line says only what was measured. The parts are joined with <c>", "</c>, once,
/// here: <c>81.2 % of lines, 70.1 % of branches</c>.
/// </summary>
public sealed class CardParts
{
    private readonly List<string> phrases = [];

    /// <summary>How many parts the line says.</summary>
    public int Count => phrases.Count;

    /// <summary>Adds a part to the line; a null part (nothing measured) is left out.</summary>
    public CardParts Add(string? phrase)
    {
        if (phrase is not null)
        {
            phrases.Add(phrase);
        }

        return this;
    }

    /// <summary>The parts joined with <c>", "</c>; empty when there are none.</summary>
    public string Join() => string.Join(", ", phrases);

    /// <summary>
    /// The parts joined, then an aside after a space - <c>average 1.9, worst 34 (5,210 methods)</c>. Without parts the
    /// line is the aside alone; without an aside it is the parts alone.
    /// </summary>
    public string Join(string? aside) => aside is null ? Join() : Count == 0 ? aside : $"{Join()} {aside}";
}
