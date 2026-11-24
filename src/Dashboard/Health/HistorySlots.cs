namespace Dashboard.Health;

/// <summary>
/// A snapshot of a check history, right-aligned into a fixed-width strip: null for a slot with no check yet,
/// the result otherwise. Exactly <c>Capacity</c> slots, oldest on the left.
/// </summary>
public static class HistorySlots
{
    /// <summary>Returns exactly <paramref name="buffer"/>.Capacity slots, with null for empty slots on the left.</summary>
    public static IReadOnlyList<ProbeResult?> Of(HistoryBuffer<ProbeResult> buffer)
    {
        var slots = new ProbeResult?[buffer.Capacity];
        var leftEmpty = buffer.Capacity - buffer.Count;
        for (var slot = 0; slot < buffer.Capacity; slot++)
        {
            var index = slot - leftEmpty;
            slots[slot] = index >= 0 ? buffer[index] : null;
        }

        return slots;
    }
}
