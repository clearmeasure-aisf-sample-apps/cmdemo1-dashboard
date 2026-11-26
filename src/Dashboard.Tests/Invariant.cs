using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// The half of a <see cref="CultureScope" /> that always asks the invariant reader: for the assertions whose point is
/// what a value or a record says about itself in digits and letters the culture was never asked about. Finnish writes
/// the minus sign as <c>−</c> (U+2212), Russian turns the group of three digits, and a Finnish <see cref="DateTime" />
/// spells <c>6.10.2026 8.30.00</c>; an assertion that compares such words against <c>"-1"</c>, <c>"1.9"</c> or
/// <c>"10/04/2026 22:00:00 +00:00"</c> is a lie about the reader, not about the code, unless the reader is pinned.
/// <para>
/// Use <see cref="Words" /> for the words a record says of itself, <see cref="Says" /> and <see cref="Runs" /> for a
/// block that asserts several things, and <see cref="ViewerCulture" /> where the
/// point is that a reader does turn the words.
/// </para>
/// </summary>
internal static class Invariant
{
    /// <summary>The words <paramref name="item" /> says of itself to the invariant reader.</summary>
    public static string Words(object item) => CultureScope.Words(item);

    /// <summary>The value a block of reads returns, taken under the invariant reader.</summary>
    public static T Says<T>(Func<T> asks) => CultureScope.Says(CultureInfo.InvariantCulture, asks);

    /// <summary>A block of assertions taken under the invariant reader.</summary>
    public static void Runs(Action asserts) => CultureScope.Runs(CultureInfo.InvariantCulture, asserts);
}
