using System.Globalization;

namespace Dashboard.Runtime;

/// <param name="Text">
/// The words of a line without pieces. A line with pieces says what its pieces say: <see cref="Text"/> is worked out
/// of <see cref="Parts"/> whenever there are any, so a line never says one thing and carries another.
/// </param>
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
    private readonly string words = Text;

    /// <summary>The words: the pieces in a row when the line has pieces, else the words it was given.</summary>
    public string Text
    {
        get => Parts is null ? words : WordsOf(Parts);
        init => words = value;
    }

    // The other four parts are declared here only so that the five keep their order (in ToString, in the JSON, in
    // reflection) now that Text is written out above; each is the auto-property the record would have made.
    public string Tone { get; init; } = Tone;

    public IReadOnlyList<RuntimeTextPart>? Parts { get; init; } = Parts;

    public RuntimeTrend? Trend { get; init; } = Trend;

    public IReadOnlyList<RuntimeCheckMark>? Marks { get; init; } = Marks;

    /// <summary>A line of pieces: its text is the pieces in a row, and the pieces are kept only when one is a link.</summary>
    /// <exception cref="ArgumentNullException">No list of pieces is given.</exception>
    /// <exception cref="ArgumentException">A slot of the list holds no piece.</exception>
    public static RuntimeTileLine Of(string tone, IReadOnlyList<RuntimeTextPart> parts, RuntimeTrend? trend = null)
    {
        RefuseAMissingPiece(parts);
        return new RuntimeTileLine(WordsOf(parts), tone, parts.Any(part => part.Link is not null) ? parts : null, trend);
    }

    /// <summary>The words of the pieces in a row; a piece of no words adds none.</summary>
    private static string WordsOf(IReadOnlyList<RuntimeTextPart> parts) => string.Concat(parts.Select(part => part?.Text));

    private static void RefuseAMissingPiece(IReadOnlyList<RuntimeTextPart> parts)
    {
        ArgumentNullException.ThrowIfNull(parts);
        for (var index = 0; index < parts.Count; index++)
        {
            if (parts[index] is null)
            {
                throw new ArgumentException(
                    string.Create(CultureInfo.InvariantCulture, $"The pieces of a line hold no piece at {index}."),
                    nameof(parts));
            }
        }
    }
}
