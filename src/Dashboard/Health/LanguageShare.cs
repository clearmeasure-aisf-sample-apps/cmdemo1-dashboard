namespace Dashboard.Health;

/// <summary>One part of the language bar: a language, or "Other" for the small ones together.</summary>
/// <param name="Slot">The part's colour, by position: 1 for the largest language; 0 for "Other".</param>
/// <param name="Percent">Its share of the lines of all languages.</param>
public sealed record LanguageShare(string Name, long Lines, double Percent, int Slot);
