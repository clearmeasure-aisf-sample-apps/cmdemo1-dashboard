namespace Dashboard;

/// <summary>
/// The one rule of the dashboard for the percent a part holds of a whole: the part over the whole, on the
/// <see cref="PercentScale" />. The language bar of the "Code" card, the meters and the "requested" line of the
/// cluster view, the estimate of the cost card and the availability line of the "Delivery" card all cut their
/// percents by it, in one order: the part over the whole first, then the scale. The other order - the scale on the
/// part first - differs from it by the last ulp at the rounding boundary, so a share that stands exactly on a half
/// percent can say a different word cut in the other order; this one is chosen for the order the cluster's own
/// share has always been cut in, and because the whole then brings the part down before the scale lifts it, so a part no double holds never overflows into the
/// scale. The rule says the number; how a card rounds it to a word - to the even whole, away from zero, to a
/// tenth - is the card's own, pinned beside its words.
/// </summary>
internal static class Shares
{
    /// <summary>The hundred a share of a whole is a percent of.</summary>
    internal const double PercentScale = 100d;

    /// <summary>
    /// The percent a part holds of a whole: <c>part / whole * <see cref="PercentScale" /></c> - the part over the
    /// whole first, then the scale, in the order the rule says it.
    /// </summary>
    internal static double Of(double part, double whole) => part / whole * PercentScale;
}
