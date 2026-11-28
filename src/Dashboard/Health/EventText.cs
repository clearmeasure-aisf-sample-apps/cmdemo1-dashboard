namespace Dashboard.Health;

/// <summary>The words and the icon of the events strip: what an event says about where it is, its level and its mark.</summary>
public static class EventText
{
    /// <summary>The word an event of the page itself is said to be at: <c>page</c>.</summary>
    private const string PageWord = "page";

    /// <summary>The separator between the environment and the node of an event's place: <c>production · eu-west1</c>.</summary>
    private const string Separator = " · ";

    /// <summary>
    /// Where the event is, in words: both parts joined by a separator, the one that is there on its own, and the
    /// page's own word when an event is of the page itself - the <c>null</c> <see cref="DashboardEvent.Environment" />
    /// and <c>null</c> <see cref="DashboardEvent.Node" /> that the record's contract says mean "no place but the page".
    /// </summary>
    public static string Where(DashboardEvent entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return (entry.Environment, entry.Node) switch
        {
            ({ } environment, { } node) => $"{environment}{Separator}{node}",
            ({ } environment, null) => environment,
            (null, { } node) => node,
            _ => PageWord,
        };
    }

    /// <summary>
    /// The tooltip word of the event's mark: <c>Good news</c>, <c>Worth a look</c>, <c>A problem</c>, and
    /// <c>For information</c> for an <see cref="EventLevel" /> the detectors do not hand out.
    /// </summary>
    public static string LevelWord(EventLevel level) => level switch
    {
        EventLevel.Good => "Good news",
        EventLevel.Warning => "Worth a look",
        EventLevel.Problem => "A problem",
        _ => "For information",
    };

    /// <summary>
    /// The icon state of the event's mark: <see cref="EventLevel.Good" /> stands on
    /// <see cref="HealthState.Healthy" />, <see cref="EventLevel.Warning" /> on <see cref="HealthState.Unhealthy" />,
    /// <see cref="EventLevel.Problem" /> on <see cref="HealthState.Unreachable" />, and every other level on
    /// <see cref="HealthState.Pending" />.
    /// </summary>
    public static HealthState Icon(EventLevel level) => level switch
    {
        EventLevel.Good => HealthState.Healthy,
        EventLevel.Warning => HealthState.Unhealthy,
        EventLevel.Problem => HealthState.Unreachable,
        _ => HealthState.Pending,
    };
}
