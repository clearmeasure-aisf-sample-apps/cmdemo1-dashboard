using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The file readers of the app share one value-or-problems record (#1811): <see cref="RuntimeParseResult{T}" />. The
/// readers of <c>runtime/</c> hand it back as it is; the reader of <c>topology.json</c> makes its
/// <see cref="TopologyParseResult" /> - the health page's name and parts, pinned by <see cref="TopologyParseResultTests" />
/// and <see cref="TopologyParserEdgeCaseTests" /> - through it, so "no value, and here is why" is said by one record.
/// The cluster's <see cref="Parsed{T}" /> stays apart: one problem, not a list.
/// </summary>
public class OneValueOrProblemsRecordTests
{
    private const BindingFlags InternalStatic = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

    private static readonly Topology Read = TopologyParser.Parse("""{ "environments": [] }""").Topology!;

    [Fact]
    public void TheTopologyRecordIsMadeFromTheSharedRecordAndOnlyFromIt()
    {
        var from = Assert.Single(typeof(TopologyParseResult).GetMethods(InternalStatic), method => method.Name == "From");

        Assert.True(from.IsAssembly);
        Assert.Equal(typeof(TopologyParseResult), from.ReturnType);
        Assert.Equal([typeof(RuntimeParseResult<Topology>)], from.GetParameters().Select(parameter => parameter.ParameterType));
    }

    [Fact]
    public void BothWaysOfMakingATopologyResultGoThroughTheSharedRecord()
    {
        var source = File.ReadAllText(MarkupGuards.SourcePath("Health/TopologyParser.cs"));

        Assert.Contains("From(RuntimeParseResult<Topology>.Failed(", source, StringComparison.Ordinal);
        Assert.Contains("From(RuntimeParseResult<Topology>.Collected(", source, StringComparison.Ordinal);
    }

    [Fact]
    public void AFailureSaysTheSameAsTheSharedRecordSays()
    {
        string[][] cases = [[], ["The file is empty."], ["environments[0].name: missing or empty.", "cluster: not an object."]];

        foreach (var problems in cases)
        {
            var shared = Invoke<RuntimeParseResult<Topology>>("Failed", [problems]);
            var topology = TopologyParseResult.Failed(problems);

            Assert.Equal(shared.IsValid, topology.IsValid);
            Assert.Equal(shared.Value, topology.Topology);
            Assert.Equal(shared.Errors, topology.Errors);
        }
    }

    [Fact]
    public void GatheredProblemsSayTheSameAsTheSharedRecordSays()
    {
        List<string> problems = ["environments: missing or not an array.", "cluster: not an object."];

        foreach (var gathered in new IReadOnlyList<string>[] { problems, [] })
        {
            var shared = Invoke<RuntimeParseResult<Topology>>("Collected", [gathered, new Func<Topology?>(() => Read)]);
            var topology = TopologyCollected(gathered, () => Read);

            Assert.Equal(shared.IsValid, topology.IsValid);
            Assert.Same(shared.Value, topology.Topology);
            Assert.Equal(shared.Errors, topology.Errors);
        }
    }

    [Fact]
    public void FromKeepsBothPartsOfTheSharedRecordAsTheyAre()
    {
        IReadOnlyList<string> messages = ["a message"];

        var failed = From(new RuntimeParseResult<Topology>(null, messages));
        var read = From(new RuntimeParseResult<Topology>(Read, []));

        Assert.Null(failed.Topology);
        Assert.Same(messages, failed.Errors);
        Assert.False(failed.IsValid);
        Assert.Same(Read, read.Topology);
        Assert.Empty(read.Errors);
        Assert.True(read.IsValid);
    }

    [Fact]
    public void TheClusterTwinStaysTheOneProblemRecord()
    {
        var parts = typeof(Parsed<ClusterStatus>).GetConstructors().Single().GetParameters();

        Assert.Equal([typeof(ClusterStatus), typeof(string)], parts.Select(part => part.ParameterType));
    }

    private static T Invoke<T>(string name, object?[] arguments) =>
        (T)typeof(T).GetMethods(InternalStatic).Single(method => method.Name == name).Invoke(null, arguments)!;

    private static TopologyParseResult TopologyCollected(IReadOnlyList<string> errors, Func<Topology> topology) =>
        (TopologyParseResult)typeof(TopologyParseResult).GetMethod("Collected", InternalStatic)!.Invoke(null, [errors, topology])!;

    private static TopologyParseResult From(RuntimeParseResult<Topology> result) =>
        (TopologyParseResult)typeof(TopologyParseResult).GetMethod("From", InternalStatic)!.Invoke(null, [result])!;
}
