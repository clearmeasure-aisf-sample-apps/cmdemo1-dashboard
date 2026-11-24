namespace Dashboard.Runtime;

/// <param name="Name">The environment's name, as in <c>topology.json</c>.</param>
/// <param name="Manifest">The manifest's file, relative to <c>runtime/</c>.</param>
/// <param name="Svg">The diagram's file, relative to <c>runtime/</c>.</param>
/// <param name="Database">
/// The files of the environment's database view; null where the deployment recorded no schema (the database's box
/// then only leads to the Azure portal).
/// </param>
public sealed record RuntimeIndexEntry(string Name, string Manifest, string Svg, RuntimeDatabaseFiles? Database = null);
