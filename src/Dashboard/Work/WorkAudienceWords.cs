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

    internal static string Label(WorkAudience audience) =>
        WordsOf(audience)?.Label
        ?? throw new ArgumentOutOfRangeException(nameof(audience), audience, "A work filter has no name.");

    internal static string Hash(WorkAudience audience) =>
        WordsOf(audience)?.Segment
        ?? throw new ArgumentOutOfRangeException(nameof(audience), audience, "A work filter has no address.");

    /// <summary>The audience whose segment is <paramref name="segment"/>, trimmed and in any case; All otherwise.</summary>
    internal static WorkAudience AudienceOf(string? segment) => WordsFor(segment)?.Audience ?? WorkAudience.All;

    /// <summary>The worker whose word is <paramref name="word"/>, trimmed and in any case; <see cref="WorkWorker.Unknown"/> otherwise.</summary>
    internal static WorkWorker WorkerOf(string? word) => WordsFor(word)?.Worker ?? WorkWorker.Unknown;

    /// <summary>The worker's word on a row; null when the file does not say who.</summary>
    internal static string? WorkerLabel(WorkWorker worker) =>
        worker == WorkWorker.Unknown
            ? null
            : Array.Find(Audiences, words => words.Worker == worker)?.Segment
              ?? throw new ArgumentOutOfRangeException(nameof(worker), worker, "A worker has no name.");

    private static Words? WordsOf(WorkAudience audience) => Array.Find(Audiences, words => words.Audience == audience);

    private static Words? WordsFor(string? word)
    {
        var trimmed = word?.Trim();
        return Array.Find(Audiences, words => string.Equals(words.Segment, trimmed, StringComparison.OrdinalIgnoreCase));
    }
}
