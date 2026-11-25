using System.Reflection;
using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// The one way a reader that gathers problems hands them over (#1810): <c>RuntimeParseResult&lt;T&gt;.Collected</c> on
/// the runtime record and <c>TopologyParseResult.Collected</c> on the health one, beside the one-problem
/// <c>Failed</c> that <see cref="FailedOnTheResultRecordTests" /> pins. Before this, the four readers that gather
/// problems - the index, the manifest, the database view and <c>topology.json</c> - each spelled the failure out of
/// the record's constructor. Both records keep the way to the assembly, so the test reaches it by name.
/// </summary>
public class CollectedOnTheResultRecordTests
{
    /// <summary>A tail that spells the failure out of a record's constructor instead of handing the list over.</summary>
    private const string HandRolledTail = @"new\s+(RuntimeParseResult<[^>]*>|TopologyParseResult)\s*\(\s*null\s*,";

    [Fact]
    public void AnyProblemMeansNoValueAndTheProblemsAsTheyAre()
    {
        List<string> problems = ["environment: missing or empty.", "nodes[1].alias: missing or empty."];
        var built = false;

        var result = RuntimeCollected<RuntimeIndex>(problems, () =>
        {
            built = true;
            return new RuntimeIndex([], null);
        });

        Assert.False(result.IsValid);
        Assert.Null(result.Value);
        Assert.Equal(["environment: missing or empty.", "nodes[1].alias: missing or empty."], result.Errors);
        Assert.False(built, "a refused read never builds its value");
    }

    [Fact]
    public void NoProblemMeansTheValueAndNoProblems()
    {
        var index = new RuntimeIndex([], "plantuml");

        var result = RuntimeCollected<RuntimeIndex>([], () => index);

        Assert.True(result.IsValid);
        Assert.Same(index, result.Value);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void NoProblemButNoValueIsStillNoValue()
    {
        var result = RuntimeCollected<RuntimeIndex>([], () => null);

        Assert.False(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void TheTopologyRecordHandsTheProblemsOverTheSameWay()
    {
        var built = false;
        var refused = TopologyCollected(["environments: missing or not an array."], () =>
        {
            built = true;
            return TopologyParser.Parse("""{"environments":[]}""").Topology!;
        });

        Assert.False(refused.IsValid);
        Assert.Equal(["environments: missing or not an array."], refused.Errors);
        Assert.False(built, "a refused read never builds its topology");

        var topology = TopologyParser.Parse("""{"environments":[]}""").Topology!;
        var read = TopologyCollected([], () => topology);

        Assert.True(read.IsValid);
        Assert.Same(topology, read.Topology);
        Assert.Empty(read.Errors);
    }

    [Fact]
    public void BothRecordsKeepTheWayToTheAssembly()
    {
        var runtime = Assert.Single(
            typeof(RuntimeParseResult<>).GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly),
            method => method.Name == "Collected");
        var topology = Assert.Single(
            typeof(TopologyParseResult).GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly),
            method => method.Name == "Collected");

        Assert.True(runtime.IsAssembly);
        Assert.True(topology.IsAssembly);
        Assert.Equal(typeof(IReadOnlyList<string>), runtime.GetParameters()[0].ParameterType);
        Assert.Equal(typeof(IReadOnlyList<string>), topology.GetParameters()[0].ParameterType);
    }

    /// <summary>The four tails hand the list over through the record; none spells the failure out of the constructor.</summary>
    [Theory]
    [InlineData("Runtime/RuntimeManifestParser.cs", "RuntimeParseResult<RuntimeIndex>.Collected(")]
    [InlineData("Runtime/RuntimeManifestParser.cs", "RuntimeParseResult<RuntimeManifest>.Collected(")]
    [InlineData("Runtime/DatabaseView.cs", "RuntimeParseResult<DatabaseManifest>.Collected(")]
    [InlineData("Health/TopologyParser.cs", "TopologyParseResult.Collected(")]
    public void EveryReaderThatGathersProblemsHandsThemOverThroughTheRecord(string file, string call)
    {
        var source = File.ReadAllText(MarkupGuards.SourcePath(file));

        Assert.Contains(call, source, StringComparison.Ordinal);

        // The record's own declarations say "new(null, ...)" without the type name; only a hand-rolled tail names it.
        Assert.DoesNotMatch(new Regex(HandRolledTail), source);
    }

    [Theory]
    [InlineData("""{"environments":[{"manifest":"a.json"},{"name":"prod","svg":"/abs.svg"},{}]}""")]
    public void TheIndexStillHandsOverEveryProblemInOrder(string json)
    {
        var result = RuntimeManifestParser.ParseIndex(json);

        Assert.False(result.IsValid);
        Assert.Equal("environments[0].name: missing or empty.", result.Errors[0]);
        Assert.Equal("environments[2].name: missing or empty.", result.Errors[^1]);
    }

    private static RuntimeParseResult<T> RuntimeCollected<T>(IReadOnlyList<string> problems, Func<T?> value)
        where T : class
    {
        var type = typeof(RuntimeParseResult<>).MakeGenericType(typeof(T));
        var collected = type.GetMethod("Collected", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (RuntimeParseResult<T>)collected.Invoke(null, [problems, value])!;
    }

    private static TopologyParseResult TopologyCollected(IReadOnlyList<string> errors, Func<Topology> topology)
    {
        var collected = typeof(TopologyParseResult).GetMethod("Collected", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (TopologyParseResult)collected.Invoke(null, [errors, topology])!;
    }
}
