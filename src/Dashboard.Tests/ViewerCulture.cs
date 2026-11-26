using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// Runs what the page formats as a viewer whose culture writes dates and numbers differently: the dashboard's own
/// words are the invariant culture's, so a number or a day never changes with whoever looks.
/// </summary>
internal static class ViewerCulture
{
    /// <summary>Runs <paramref name="what" /> as a reader of the German culture.</summary>
    public static void German(Action what) => CultureScope.Runs("de-DE", what);

    /// <summary>The value <paramref name="asks" /> returns, taken under the invariant reader: the words a record
    /// says of itself in digits and letters, with no culture to turn them.</summary>
    public static string Invariant(Func<string> asks) => CultureScope.Says(CultureInfo.InvariantCulture, asks);

    /// <summary>A block of assertions taken under the invariant reader.</summary>
    public static void Invariant(Action asserts) => CultureScope.Runs(CultureInfo.InvariantCulture, asserts);
}
