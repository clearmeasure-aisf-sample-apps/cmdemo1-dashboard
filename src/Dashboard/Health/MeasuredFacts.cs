namespace Dashboard.Health;

/// <summary>The one rule of the build's facts for "nothing measured": every part came back without a value.</summary>
internal static class MeasuredFacts
{
    /// <summary>
    /// The record as read; null when it measured nothing, so the card shows nothing for it: a record of nothing is no
    /// record. The record says what "nothing" is for its parts; this says what follows from it, once.
    /// </summary>
    internal static T? OrNone<T>(T facts, bool measuredNothing)
        where T : class => measuredNothing ? null : facts;

    /// <summary>True when every part is null: the build measured nothing there, and there is no record.</summary>
    internal static bool None(params ReadOnlySpan<object?> parts)
    {
        foreach (var part in parts)
        {
            if (part is not null)
            {
                return false;
            }
        }

        return true;
    }
}
