namespace Dashboard.Runtime;

/// <param name="Text">The words.</param>
/// <param name="Tone">
/// One of <see cref="RuntimeLineTone"/>: <c>strong</c> (the running version), <c>plain</c>, <c>muted</c>, <c>serving</c>,
/// <c>ok</c>, <c>warn</c>, and for the comparison with the pinned version <c>insync</c>, <c>differs</c> or <c>unknown</c> (drawn with the dashboard's =, ≠ and dots). An activity
/// line (what is being deployed to the node's deployable, a deployment freeze, the last deployment that ended) has
/// its kind as its tone: <c>waiting</c>, <c>deploying</c>, <c>queued</c>, <c>frozen</c>, <c>freeze</c>,
/// <c>deployed</c>, <c>failed</c> or <c>canceled</c> (<see cref="Health.ActivityKind"/>).
/// </param>
/// <param name="Parts">
/// The same words in pieces, where a piece is a link; null for a line without links, which is drawn from
/// <paramref name="Text"/>.
/// </param>
/// <param name="Trend">The sparkline after the words; null without one.</param>
/// <param name="Marks">
/// Small marks before the words, one per entry of the node's detailed health check; null for a line without them.
/// </param>
public sealed record RuntimeTileLine(
    string Text,
    string Tone = RuntimeLineTone.Plain,
    IReadOnlyList<RuntimeTextPart>? Parts = null,
    RuntimeTrend? Trend = null,
    IReadOnlyList<RuntimeCheckMark>? Marks = null)
{
    /// <summary>A line of pieces: its text is the pieces in a row, and the pieces are kept only when one is a link.</summary>
    public static RuntimeTileLine Of(string tone, IReadOnlyList<RuntimeTextPart> parts, RuntimeTrend? trend = null)
    {
        ArgumentNullException.ThrowIfNull(parts);
        return new RuntimeTileLine(string.Concat(parts.Select(part => part.Text)), tone, parts.Any(part => part.Link is not null) ? parts : null, trend);
    }
}
