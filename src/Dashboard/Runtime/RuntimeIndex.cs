namespace Dashboard.Runtime;

/// <summary>
/// <c>runtime/index.json</c>: the environments whose runtime diagram the deployment rendered, in the order of
/// <c>system.json</c>.
/// </summary>
/// <param name="Environments">One entry per environment.</param>
/// <param name="PlantUml">The PlantUML version that rendered the diagrams.</param>
public sealed record RuntimeIndex(IReadOnlyList<RuntimeIndexEntry> Environments, string? PlantUml = null);
