using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dashboard.Runtime;

/// <summary>
/// What <c>js/runtime.js</c> draws into the diagram on one update: one tile per node, one mark per region and one per
/// relationship, keyed by the manifest's aliases and ids. The C# side decides every word and every state; the script
/// only draws. Serialized with <see cref="RuntimePayloadJson"/> (camelCase, nulls left out).
/// </summary>
/// <param name="Names">
/// The boxes that have a name and no tile or mark (the frames that are no region, the browser), each with where its
/// name leads; null when the manifest has a link for none of them.
/// </param>
public sealed record RuntimePayload(
    IReadOnlyList<RuntimeTile> Nodes,
    IReadOnlyList<RuntimeRegionMark> Regions,
    IReadOnlyList<RuntimeEdgeMark> Edges,
    IReadOnlyList<RuntimeName>? Names = null)
{
    /// <summary>
    /// The update as the script reads it. The finite-number rule of a sparkline lives here, with the one writer: the
    /// records hold whatever heights they are handed, and JSON has no word for NaN or an infinity, so a height that is
    /// no finite number is refused for the whole update - with the node and line, or the relationship, that carried it.
    /// </summary>
    /// <exception cref="ArgumentException">A trend of the update holds a height that is no finite number.</exception>
    public string ToJson()
    {
        foreach (var (where, trend) in Trends())
        {
            RefuseAHeightThatIsNoFiniteNumber(where, trend);
        }

        return JsonSerializer.Serialize(this, RuntimePayloadJson.Default.RuntimePayload);
    }

    /// <summary>Every trend the update carries, with the words that say where it is drawn.</summary>
    private IEnumerable<(string Where, RuntimeTrend Trend)> Trends()
    {
        foreach (var tile in Nodes ?? [])
        {
            var lines = tile?.Lines ?? [];
            for (var index = 0; index < lines.Count; index++)
            {
                if (lines[index]?.Trend is { } trend)
                {
                    yield return (string.Create(CultureInfo.InvariantCulture, $"line {index + 1} of node '{tile!.Alias}'"), trend);
                }
            }
        }

        foreach (var edge in Edges ?? [])
        {
            if (edge?.Trend is { } trend)
            {
                yield return ($"relationship '{edge.Id}'", trend);
            }
        }
    }

    private static void RefuseAHeightThatIsNoFiniteNumber(string where, RuntimeTrend trend)
    {
        var points = trend.Points ?? [];
        for (var index = 0; index < points.Count; index++)
        {
            if (!double.IsFinite(points[index]))
            {
                throw new ArgumentException(string.Create(
                    CultureInfo.InvariantCulture,
                    $"The trend of {where} holds a height that is no finite number ({points[index]} at {index}); JSON cannot carry it, so the update is not written."));
            }
        }
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(RuntimePayload))]
public sealed partial class RuntimePayloadJson : JsonSerializerContext;
