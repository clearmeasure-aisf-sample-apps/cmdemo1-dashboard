namespace Dashboard.Health;

/// <summary>
/// What a page shows of one reading, and the memory of the last reading that succeeded: what a new reading is
/// compared with to find a change. The one rule they share is owned here, once: a reading that failed replaces
/// what the page shows but not what the next reading is compared with.
/// </summary>
/// <typeparam name="T">The thing read; <see langword="null" /> is "this round read nothing".</typeparam>
public sealed class Reading<T> where T : class
{
    /// <summary>What the page shows: this round's reading, good or not, replaces the last one.</summary>
    public T? Current { get; private set; }

    /// <summary>
    /// The last reading that succeeded: what a new reading is compared with. Null until a reading that is not
    /// nothing comes.
    /// </summary>
    public T? LastGood { get; private set; }

    /// <summary>
    /// One reading that decides both halves at once: it replaces what the page shows, and - when it is not
    /// nothing - becomes the memory the next reading is compared with. A reading of nothing shows nothing and
    /// leaves the memory standing.
    /// </summary>
    public void Record(T? reading)
    {
        Current = reading;
        if (reading is not null)
        {
            LastGood = reading;
        }
    }

    /// <summary>Replaces what the page shows, and remembers nothing: the memory stands where it was.</summary>
    public void Show(T? reading) => Current = reading;

    /// <summary>Replaces the memory of the last reading that succeeded, and shows nothing; nothing is refused -
    /// <see langword="null" /> clears it.</summary>
    public void Remember(T? good) => LastGood = good;
}
