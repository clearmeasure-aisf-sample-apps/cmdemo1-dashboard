namespace Dashboard.Tests;

/// <summary>
/// <see cref="SitePath" />, the one home of the leading-slash rule of a deployable's paths (#1041), and the decision
/// that the traffic paths keep their spaces, named here as deliberate.
/// </summary>
public class SitePathTests
{
    [Theory]
    [InlineData("health", "/health")]
    [InlineData(" health ", "/health")]
    [InlineData("/health", "/health")]
    [InlineData("//health", "//health")]
    [InlineData(" /health/x ", "/health/x")]
    public void AWordIsTrimmedAndTakesTheFrontItLacks(string text, string expected) =>
        Assert.Equal(expected, SitePath.Parse(text));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void AWordOfNothingIsNoPath(string? text) => Assert.Null(SitePath.Parse(text));

    [Theory]
    [InlineData("api", "/api")]
    [InlineData(" api ", "/ api ")]
    [InlineData("/", "/")]
    [InlineData("//x", "//x")]
    public void WithFrontAddsTheSlashAndChangesNothingElse(string path, string expected) =>
        Assert.Equal(expected, SitePath.WithFront(path));

    /// <summary>The other half of the rule (#1541): a path is asked at from the root, every front slash dropped
    /// for the joiner's one, and nothing after the front changed.</summary>
    [Theory]
    [InlineData("health", "health")]
    [InlineData("/health", "health")]
    [InlineData("//health", "health")]
    [InlineData("////", "")]
    [InlineData("/x///y/", "x///y/")]
    [InlineData("/ api ", " api ")]
    public void FromRootDropsEveryFrontSlashAndChangesNothingElse(string path, string expected) =>
        Assert.Equal(expected, SitePath.FromRoot(path));

    /// <summary>The two halves meet (#1541): a run of front slashes written in the file is recorded as written,
    /// and the address the probe asks at takes the path from the root, with the one slash the join adds.</summary>
    [Fact]
    public void APathWithARunOfFrontSlashesIsRecordedAsWrittenAndAskedAtFromTheRoot()
    {
        var result = TopologyParser.Parse("""
            { "environments": [ { "name": "prod", "deployables": [ { "healthPath": "//health" } ] } ] }
            """);

        var deployable = Assert.Single(Assert.Single(result.Topology!.Environments).Deployables);
        Assert.Equal("//health", deployable.HealthPath);
        Assert.Equal(
            "https://app.example.net/health",
            ProbeUrl.Combine(new Uri("https://app.example.net/"), deployable.HealthPath).AbsoluteUri);
    }

    /// <summary>The traffic paths are the one list of paths the reader does not trim: a padded entry is kept as the
    /// path it was written as, on purpose, while every other path of the deployable is read over its spaces.</summary>
    [Fact]
    public void APaddedTrafficPathIsKeptAsWrittenDeliberatelyWhileTheOtherPathsAreTrimmed()
    {
        var result = TopologyParser.Parse("""
            { "environments": [ { "name": "prod", "deployables": [ {
                "healthPath": " health ", "telemetryPath": " _telemetry ", "trafficPaths": [ " api " ] } ] } ] }
            """);

        var deployable = Assert.Single(Assert.Single(result.Topology!.Environments).Deployables);
        Assert.Equal("/health", deployable.HealthPath);
        Assert.Equal("/_telemetry", deployable.TelemetryPath);
        Assert.Equal(["/ api "], deployable.TrafficPaths!);
    }
}
