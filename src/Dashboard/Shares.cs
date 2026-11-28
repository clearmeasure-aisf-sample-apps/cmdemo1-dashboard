using System.Globalization;

namespace Dashboard;

/// <summary>
/// The one rule of the dashboard for the percent a part holds of a whole: the part over the whole, on the
/// <see cref="PercentScale" />. The language bar of the "Code" card, the meters and the "requested" line of the
/// cluster view, the estimate of the cost card, the availability line of the "Delivery" card and the rework rate of
/// the Work stats strip (<see cref="Work.WorkMeasureText" />) all cut their percents by it, in one order: the part over the whole first, then the scale. The other order - the scale on the
/// part first - differs from it by the last ulp at the rounding boundary, so a share that stands exactly on a half
/// percent can say a different word cut in the other order; this one is chosen for the order the cluster's own
/// share has always been cut in, and because the whole then brings the part down before the scale lifts it, so a part no double holds never overflows into the
/// scale. The rule says the number. The two cards that round it to the even whole - the language bar and the
/// cluster view - share its word too, <see cref="WholePercent" />; the cost estimate, away from zero, and the
/// availability and rework lines, to a tenth, keep their own rounding, pinned beside their words.
/// </summary>
internal static class Shares
{
    /// <summary>The hundred a share of a whole is a percent of.</summary>
    internal const double PercentScale = 100d;

    /// <summary>The suffix every percent word ends with, a space and the sign: <c>63 %</c>.</summary>
    internal const string PercentSign = " %";

    /// <summary>The format of the whole number <see cref="WholePercent" /> writes after rounding: <c>63</c>.</summary>
    private const string WholeFormat = "0";

    /// <summary>
    /// The percent a part holds of a whole: <c>part / whole * <see cref="PercentScale" /></c> - the part over the
    /// whole first, then the scale, in the order the rule says it.
    /// </summary>
    internal static double Of(double part, double whole) => part / whole * PercentScale;

    /// <summary>
    /// A percent as a whole percentage: rounded half to the even whole, the way <see cref="Math.Round(double)" />
    /// does, in the invariant culture, and the <see cref="PercentSign" /> - <c>63 %</c>, and <c>-0 %</c> for
    /// negative zero.
    /// </summary>
    internal static string WholePercent(double percent) =>
        Math.Round(percent).ToString(WholeFormat, CultureInfo.InvariantCulture) + PercentSign;
}
