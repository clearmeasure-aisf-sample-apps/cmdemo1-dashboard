using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace Dashboard;

/// <summary>
/// The one read of a select's change into a value of the control's own type: the text the browser sent, parsed the
/// way the caller names, and kept only when it is a value the control offers (#1816). The page's interval and probe
/// selects both decode their change through it, so a value none of the options offers is ignored the same way.
/// </summary>
internal static class SelectChange
{
    /// <summary>A parse of an option's text into the control's type, in the shape of <c>int.TryParse</c>.</summary>
    internal delegate bool Parse<T>(string? text, out T value);

    /// <summary>
    /// The value the change names, when <paramref name="parse"/> reads it and the control <paramref name="offered"/>
    /// it; otherwise false, and the control keeps what it had.
    /// </summary>
    internal static bool TryRead<T>(ChangeEventArgs change, Parse<T> parse, Func<T, bool> offered, out T value) =>
        parse(change.Value?.ToString(), out value) && offered(value);

    /// <summary>An option's whole number, written the invariant way Blazor renders an <see cref="int"/> value.</summary>
    internal static bool Whole(string? text, out int value) =>
        int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
}
