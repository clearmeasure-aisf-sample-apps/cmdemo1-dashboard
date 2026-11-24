namespace Dashboard;

/// <summary>
/// The moment of one rendering. The page reads the clock once per render (<see cref="Pin" />) and every component of
/// that pass gets the same <c>Now</c>, so two tiles of one rendering never disagree about the day. It is not a
/// <see cref="TimeProvider" />: what needs a running clock (polling, timers) is given the clock itself.
/// </summary>
public sealed class RenderClock
{
    private readonly TimeProvider _clock;
    private DateTimeOffset _now;

    public RenderClock(TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        _clock = clock;
        Pin();
    }

    /// <summary>Reads the clock for a new rendering; until the next pin, <see cref="GetUtcNow" /> says this instant.</summary>
    public void Pin() => _now = _clock.GetUtcNow();

    /// <summary>The instant of the current rendering, the same however often it is asked.</summary>
    public DateTimeOffset GetUtcNow() => _now;
}
