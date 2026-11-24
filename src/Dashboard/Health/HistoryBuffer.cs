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

    public IEnumerator<T> GetEnumerator()
    {
        for (var index = 0; index < Count; index++)
        {
            yield return this[index];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
