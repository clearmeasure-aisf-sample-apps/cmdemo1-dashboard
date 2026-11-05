namespace Dashboard.Tests;

/// <summary>
/// The multi-region web app diagram this dashboard used to ship, kept so the Front Door and web app checks still have
/// a diagram. The files the app ships are cmdemo1 after app2-ui is gone: <see cref="RuntimeManifestParserTests.Sample"/>.
/// </summary>
internal static class WebAppDiagram
{
    internal static string Topology { get; } = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "webapp.topology.json"));

    internal static string Sample(string file) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "webapp.sample", file));
}
