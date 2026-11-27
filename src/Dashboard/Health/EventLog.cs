namespace Dashboard.Health;

/// <summary>The last events this page observed since it was opened, newest first.</summary>
public sealed class EventLog
{
    public const int DefaultCapacity = 50;

    private readonly Lock _gate = new();
    private readonly HistoryBuffer<DashboardEvent> _events;
    private List<DashboardEvent>? _newest;

    public EventLog(int capacity = DefaultCapacity)
    {
        _events = new HistoryBuffer<DashboardEvent>(capacity);
    }

    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _events.Count;
            }
        }
    }

    /// <summary>
    /// The newest event, null while the log holds none - the first element of <see cref="Newest"/>, read by its own end
    /// rather than a second ordering of the log, so a reader that wants one event takes the one the page reads, not a
    /// sort of its own. It is the newest by the moment the event carries, not the last written: an event written last
    /// whose moment is the earliest is not the newest.
    /// </summary>
    public DashboardEvent? Latest
    {
        get
        {
            lock (_gate)
            {
                var newest = NewestLocked();
                return newest.Count > 0 ? newest[0] : null;
            }
        }
    }

    /// <summary>
    /// Newest first; events of the same moment in the order they were observed. The sort is load-bearing, not a courtesy
    /// to the reader: the events come in out of chronological order, because a round's events all carry the round's
    /// <c>now</c> and the serving, pin and cluster events are added after the checks they belong to, so the log must be
    /// re-ordered before the page reads it - do not "optimise" the sort away, it would change the order the page reads.
    /// The ordered copy is kept until the next <see cref="Add"/>, so drawing the strip several times between additions
    /// orders the log once.
    /// </summary>
    public IReadOnlyList<DashboardEvent> Newest
    {
        get
        {
            lock (_gate)
            {
                return NewestLocked();
            }
        }
    }

    /// <summary>The order the page reads: newest first, same moment in the order observed. Kept until the next <see cref="Add"/>.</summary>
    private List<DashboardEvent> NewestLocked()
    {
        return _newest ??= [.. _events.Select((entry, index) => (entry, index)).OrderByDescending(item => item.entry.At).ThenBy(item => item.index).Select(item => item.entry)];
    }

    public void Add(DashboardEvent entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        lock (_gate)
        {
            _events.Add(entry);
            _newest = null;
        }
    }

    public void AddRange(IEnumerable<DashboardEvent> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        foreach (var entry in entries)
        {
            Add(entry);
        }
    }
}
