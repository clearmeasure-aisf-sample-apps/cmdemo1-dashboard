using System.Collections;

namespace Dashboard.Health;

/// <summary>A ring buffer: keeps the last <see cref="Capacity"/> items, oldest first.</summary>
public sealed class HistoryBuffer<T> : IReadOnlyList<T>
{
    private readonly T[] _items;
    private int _start;

    public HistoryBuffer(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);
        _items = new T[capacity];
    }

    public int Capacity => _items.Length;

    public int Count { get; private set; }

    private int Slot(int offset) => (_start + offset) % Capacity;

    /// <summary>The item at <paramref name="index"/>, where 0 is the oldest item kept.</summary>
    public T this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);
            return _items[Slot(index)];
        }
    }

    /// <summary>
    /// The newest item kept: the one <see cref="Add"/> wrote last, the item at index <see cref="Count"/> minus one.
    /// A buffer that holds nothing has no newest item and answers with <see langword="default"/>, the same answer a
    /// buffer whose newest item is <see langword="default"/> itself gives - <see cref="Count"/> tells them apart.
    /// </summary>
    public T? Last => Count > 0 ? this[Count - 1] : default;

    /// <summary>Adds an item; when the buffer is full, the oldest item is dropped.</summary>
    public void Add(T item)
    {
        _items[Slot(Count)] = item;
        if (Count < Capacity)
        {
            Count++;
        }
        else
        {
            _start = Slot(1);
        }
    }

    /// <summary>
    /// Records the reading of one check in a trend, where null is a gap: a reading is always added, a gap only once
    /// the buffer holds something for it to be a gap in - so a trend never starts with a gap, and the checks that
    /// read nothing before the first reading store nothing. The buffer still keeps at most <see cref="Capacity"/>.
    /// </summary>
    public void Record(T reading)
    {
        if (reading is not null || Count > 0)
        {
            Add(reading);
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (var index = 0; index < Count; index++)
        {
            yield return this[index];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
