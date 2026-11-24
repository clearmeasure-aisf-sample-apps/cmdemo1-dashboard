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
    public string ToJson() => JsonSerializer.Serialize(this, RuntimePayloadJson.Default.RuntimePayload);
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(RuntimePayload))]
public sealed partial class RuntimePayloadJson : JsonSerializerContext;
