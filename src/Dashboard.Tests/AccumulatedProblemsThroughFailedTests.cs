using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// #1810: a reader that collects several problems in a list says "no value" the same way as a reader with one
/// problem, through the record's own <c>Failed</c>, and gives it the list (<c>Failed([.. errors])</c>). Before this, the
/// tails of <see cref="RuntimeManifestParser.ParseIndex" />, <see cref="RuntimeManifestParser.ParseManifest" />,
/// <see cref="DatabaseParser.ParseManifest" /> and <see cref="TopologyParser.Parse" /> each built the failure with
/// <c>new X(null, errors)</c>. Here: the words, how many there are and their order still come through, and no
/// production file builds a failed result by hand.
/// </summary>
public class AccumulatedProblemsThroughFailedTests
{
    /// <summary>A failed result built with the record's constructor instead of its <c>Failed</c>.</summary>
    private const string HandRolledFailure = @"\bnew\s+(TopologyParseResult|RuntimeParseResult<[^>]*>)\s*\(\s*null\b";

    [Fact]
    public void TheIndexHandsOverEveryProblemInTheOrderFound()
    {
        var result = RuntimeManifestParser.ParseIndex("""{"environments":[{},{"name":""}]}""");

        Assert.False(result.IsValid);
        Assert.Null(result.Value);
        Assert.Equal(
            ["environments[0].name: missing or empty.", "environments[1].name: missing or empty."],
            result.Errors);
    }

    [Fact]
    public void TheDatabaseManifestHandsOverEveryProblemInTheOrderFound()
    {
        var result = DatabaseParser.ParseManifest("""{"tables":[{}]}""");

        Assert.False(result.IsValid);
        Assert.Null(result.Value);
        Assert.Equal(
            [
                "environment: missing or empty.",
                "database: missing or empty.",
                "tables[0]: alias, qualifiedName and name are required.",
            ],
            result.Errors);
    }

    [Fact]
    public void TheManifestAndTheTopologyFailWithTheirProblems()
    {
        var manifest = RuntimeManifestParser.ParseManifest("{}");
        var topology = TopologyParser.Parse("{}");

        Assert.False(manifest.IsValid);
        Assert.Null(manifest.Value);
        Assert.NotEmpty(manifest.Errors);

        Assert.False(topology.IsValid);
        Assert.Null(topology.Topology);
        Assert.NotEmpty(topology.Errors);
    }

    [Fact]
    public void NoProductionFileBuildsAFailedResultByHand()
    {
        var offenders = Directory
            .EnumerateFiles(MarkupGuards.SourcePath("."), "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => Regex.IsMatch(File.ReadAllText(path), HandRolledFailure))
            .Select(Path.GetFileName)
            .ToList();

        Assert.Empty(offenders);

        // A guard that found nothing has seen something: the four tails are there and go through Failed.
        var runtime = File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("Runtime", "RuntimeManifestParser.cs")));
        var database = File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("Runtime", "DatabaseView.cs")));
        var topology = File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("Health", "TopologyParser.cs")));
        Assert.Contains("RuntimeParseResult<RuntimeIndex>.Failed([.. errors])", runtime, StringComparison.Ordinal);
        Assert.Contains("RuntimeParseResult<RuntimeManifest>.Failed([.. errors])", runtime, StringComparison.Ordinal);
        Assert.Contains("RuntimeParseResult<DatabaseManifest>.Failed([.. errors])", database, StringComparison.Ordinal);
        Assert.Contains("TopologyParseResult.Failed([.. errors])", topology, StringComparison.Ordinal);
    }
}
