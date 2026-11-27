using System.Globalization;

namespace Dashboard.Health;

/// <summary>
/// A deployment freeze in the words of the page, word for word: the brief dot sentence on a node, the sentence
/// under an environment's name, and the tooltip. A deployment's words sit in <see cref="DeploymentText"/>; the
/// freeze's sit here, in one place.
/// </summary>
public static class FreezeText
{
    /// <summary>
    /// The brief dot sentence on a node, the node already naming the deployable and the environment:
    /// <c>frozen until Sun 19:00: prod weekend</c>, <c>freeze from Sat 19:00: prod weekend</c>.
    /// </summary>
    public static string Brief(DeploymentFreeze freeze, DateTimeOffset now, TimeZoneInfo zone)
    {
        ArgumentNullException.ThrowIfNull(freeze);
        ArgumentNullException.ThrowIfNull(zone);
        return freeze.InForceAt(now)
            ? $"frozen until {DeploymentActivity.When(freeze.To, now, zone)}: {freeze.Name}"
            : $"freeze from {DeploymentActivity.When(Begins(freeze), now, zone)}: {freeze.Name}";
    }

    /// <summary>
    /// The sentence under an environment's name, naming the first freeze that covers the environment - in force
    /// before the one that comes - and, where more cover it, how many: <c>deployment freeze prod weekend: demo-ui
    /// frozen until Mon 00:00, and 1 more</c>, <c>... frozen from Sat 19:00 until Sun 19:00</c>.
    /// </summary>
    /// <param name="freeze">The first freeze to say: the one in force before the one that comes.</param>
    /// <param name="count">How many freezes cover the environment; more than one is said as <c>, and N more</c>.</param>
    public static string Sentence(DeploymentFreeze freeze, int count, DateTimeOffset now, TimeZoneInfo zone)
    {
        ArgumentNullException.ThrowIfNull(freeze);
        ArgumentNullException.ThrowIfNull(zone);
        var more = count > 1 ? string.Create(CultureInfo.InvariantCulture, $", and {count - 1} more") : string.Empty;
        var from = freeze.InForceAt(now) ? string.Empty : $" from {DeploymentActivity.When(Begins(freeze), now, zone)}";
        return $"deployment freeze {freeze.Name}: {Words(freeze.Projects)} frozen{from} until {DeploymentActivity.When(freeze.To, now, zone)}{more}";
    }

    /// <summary>
    /// The tooltip: the freeze in a full sentence, with its times in the viewer's time zone, and the words every
    /// tooltip of a freeze ends with: <c>Deployment freeze prod weekend: no deployment of demo-ui to prod from
    /// 2026-10-09 00:00 +00:00 until 2026-10-12 00:00 +00:00. In force now. A freeze belongs to the Octopus Deploy
    /// instance; a person with the right may override it for one deployment.</c>
    /// </summary>
    public static string Title(DeploymentFreeze freeze, DateTimeOffset now, TimeZoneInfo zone)
    {
        ArgumentNullException.ThrowIfNull(freeze);
        ArgumentNullException.ThrowIfNull(zone);
        var from = freeze.From is { } begins ? $" from {TimeText.DateAndTime(begins, zone)}" : string.Empty;
        var state = freeze.InForceAt(now)
            ? "In force now"
            : $"Begins in {TimeText.Span(Begins(freeze) - now)}";
        return $"Deployment freeze {freeze.Name}: no deployment of {Words(freeze.Projects)} to {Words(freeze.Environments)}{from} until {TimeText.DateAndTime(freeze.To, zone)}. {state}. "
            + "A freeze belongs to the Octopus Deploy instance; a person with the right may override it for one deployment.";
    }

    /// <summary>
    /// The start of a freeze the page says of that is not in force: one of <see cref="DeploymentActivity"/>'s
    /// freezes, so one that <see cref="DeploymentFreeze.ComesAt"/> the moment, which only a freeze with a known
    /// <see cref="DeploymentFreeze.From"/> does. The caller holds that; the record does not.
    /// </summary>
    private static DateTimeOffset Begins(DeploymentFreeze freeze) => freeze.From!.Value;

    /// <summary>
    /// The words of a list of names: the names joined, or <c>the system</c> when the list names nothing - a freeze
    /// that names no projects covers the system as a whole.
    /// </summary>
    private static string Words(IReadOnlyList<string> names) => names.Count == 0 ? "the system" : string.Join(", ", names);
}
