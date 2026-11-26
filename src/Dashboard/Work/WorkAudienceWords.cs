namespace Dashboard.Work;

/// <summary>
/// The words of each audience: its name on the strip, its segment of the address, and the worker whose items it
/// holds. The segment is also that worker's word on a row and in the file. All holds no one worker, so it has none.
/// Beside the report model because the file's <c>worker</c> word is read through the same table.
/// </summary>
internal static class WorkAudienceWords
{
    private sealed record Words(WorkAudience Audience, string Label, string Segment, WorkWorker? Worker);

    /// <summary>The one table of audience words. A new audience is one more row here.</summary>
    private static readonly Words[] Audiences =
    [
        new(WorkAudience.All, "All", string.Empty, Worker: null),
        new(WorkAudience.Bot, "Bot", "bot", WorkWorker.Bot),
        new(WorkAudience.Team, "Team", "team", WorkWorker.Team),
    ];

    internal static string Label(WorkAudience audience) => Of(audience, "A work filter has no name.").Label;

    internal static string Hash(WorkAudience audience) => Of(audience, "A work filter has no address.").Segment;

    /// <summary>The audience whose segment is <paramref name="segment"/>, trimmed and in any case; All otherwise.</summary>
    internal static WorkAudience AudienceOf(string? segment) => WordsFor(segment)?.Audience ?? WorkAudience.All;

    /// <summary>The worker whose word is <paramref name="word"/>, trimmed and in any case; <see cref="WorkWorker.Unknown"/> otherwise.</summary>
    internal static WorkWorker WorkerOf(string? word) => WordsFor(word)?.Worker ?? WorkWorker.Unknown;

    /// <summary>The worker's word on a row; null when the file does not say who.</summary>
    internal static string? WorkerLabel(WorkWorker worker) =>
        worker == WorkWorker.Unknown
            ? null
            : Row(words => words.Worker == worker, nameof(worker), worker, "A worker has no name.").Segment;

    /// <summary>The row of <paramref name="audience"/>; an audience the table does not hold is refused with <paramref name="refusal"/>.</summary>
    private static Words Of(WorkAudience audience, string refusal) =>
        Row(words => words.Audience == audience, nameof(audience), audience, refusal);

    /// <summary>
    /// The one lookup that must find its row, and the one refusal when it does not: the argument by name, the value
    /// it had, and the caller's reason.
    /// </summary>
    private static Words Row(Predicate<Words> match, string argument, object value, string refusal) =>
        Array.Find(Audiences, match) ?? throw new ArgumentOutOfRangeException(argument, value, refusal);

    private static Words? WordsFor(string? word)
    {
        var trimmed = word?.Trim();
        return Array.Find(Audiences, words => string.Equals(words.Segment, trimmed, StringComparison.OrdinalIgnoreCase));
    }
}
