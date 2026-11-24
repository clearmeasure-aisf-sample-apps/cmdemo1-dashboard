namespace Dashboard.Runtime;

/// <summary>The two files of an environment's database view, relative to <c>runtime/</c>.</summary>
/// <param name="Svg">The diagram of the tables.</param>
/// <param name="Manifest">Which drawn table is which (<see cref="DatabaseManifest"/>).</param>
public sealed record RuntimeDatabaseFiles(string Svg, string Manifest);
