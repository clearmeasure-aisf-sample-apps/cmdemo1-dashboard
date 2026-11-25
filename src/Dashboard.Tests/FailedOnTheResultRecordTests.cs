using System.Reflection;
using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// The one way a reader of a file says "there is no value, and here is why": <c>RuntimeParseResult&lt;T&gt;.Failed</c>
/// on the runtime record and <see cref="TopologyParseResult.Failed" /> on the health one. Before this, each of the two
/// readers of <c>runtime/</c> kept a copy of its own - a private <c>Failed&lt;T&gt;</c> in <see
/// cref="RuntimeManifestParser" /> and a private <c>Failed(string)</c> in <see cref="DatabaseParser" /> - so a reader
/// that wanted to say one problem had to know which of the three its parser kept. The words of a refusal are pinned
/// where they are said (see <see cref="RuntimeIndexFrameAndParseResultTests" />, <see cref="TopologyParseResultTests" />
/// and the readers' own tests); what is pinned here is that each record has the one constructor, that it says nothing
/// but what it is given, and that no parser keeps a copy of it. The record keeps its constructor to the folder, as
/// <c>TryOpen</c> does, so the test reaches it by name, the way the tests reach every other internal of the folder.
/// </summary>
public class FailedOnTheResultRecordTests
{
    /// <summary>The constructor of a <c>RuntimeParseResult</c>, as the app writes it.</summary>
    private const string Declaration = @"\binternal\s+static\s+RuntimeParseResult<[^>]*>\s+Failed\s*\(";

    /// <summary>A reader of <c>runtime/</c> asking the record for the failure rather than making one of its own.</summary>
    private const string Call = @"\bRuntimeParseResult<[^>]*>\s*\.\s*Failed\s*\(";

    [Fact]
    public void TheRuntimeRecordSaysThereIsNoValueAndWhy()
    {
        var result = Failed<RuntimeIndex>("environments[0].name: missing or empty.");

        Assert.False(result.IsValid);
        Assert.Null(result.Value);
        Assert.Equal(["environments[0].name: missing or empty."], result.Errors);
    }

    [Fact]
    public void TheProblemsKeepTheOrderTheReaderFoundThem()
    {
        var result = Failed<RuntimeManifest>(
            "environment: missing or empty.",
            "nodes[1].alias: missing or empty.",
            "edges[0]: id, from and to are required.");

        Assert.Equal(
            [
                "environment: missing or empty.",
                "nodes[1].alias: missing or empty.",
                "edges[0]: id, from and to are required.",
            ],
            result.Errors);
    }

    /// <summary>The parity with the topology record's own way, which <see cref="TopologyParseResultTests" /> pins: a
    /// failure without a single problem is still no value.</summary>
    [Fact]
    public void AFailureWithoutAnyProblemIsStillNoValue()
    {
        var runtime = Failed<DatabaseManifest>();

        Assert.False(runtime.IsValid);
        Assert.Null(runtime.Value);
        Assert.Empty(runtime.Errors);

        Assert.False(TopologyParseResult.Failed().IsValid);
    }

    /// <summary>The two records wear the same vocabulary: the same word for the one constructor, the same answer to
    /// the page's one question, the same messages for a file that is nothing.</summary>
    [Fact]
    public void TheTwoRecordsSayNoValueTheSameWay()
    {
        var health = TopologyParseResult.Failed("The file is empty.");
        var runtime = Failed<RuntimeIndex>("The file is empty.");

        Assert.Null(health.Topology);
        Assert.Null(runtime.Value);
        Assert.False(health.IsValid);
        Assert.False(runtime.IsValid);
        Assert.Equal(health.Errors, runtime.Errors);
    }

    /// <summary>The three reads of <c>runtime/</c> answer an empty file the same way, and the way is the record's:
    /// no value and the opener's one sentence.</summary>
    [Fact]
    public void EveryReaderOfRuntimeRefusesThroughTheRecordsOwnConstructor()
    {
        var index = RuntimeManifestParser.ParseIndex(string.Empty);
        var manifest = RuntimeManifestParser.ParseManifest(null);
        var database = DatabaseParser.ParseManifest("   ");

        NoValue(index, "the index");
        NoValue(manifest, "the manifest");
        NoValue(database, "the database manifest");
    }

    [Theory]
    [InlineData(typeof(RuntimeManifestParser))]
    [InlineData(typeof(DatabaseParser))]
    public void AParserKeepsNoCopyOfTheConstructor(Type parser)
    {
        var kept = parser
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(method => method.Name == "Failed")
            .Select(method => $"{parser.Name}.{method}");

        Assert.Empty(kept);
    }

    /// <summary>The record wears the one constructor: no public static of its own - the record's value and the
    /// messages say what a read is, and the constructor says it for the folder that makes the reads.</summary>
    [Fact]
    public void TheRuntimeRecordWearsItsOneFailed()
    {
        var record = typeof(RuntimeParseResult<>);

        // A public static of the record would stand alone in the folder's coverage read, so the record keeps it to
        // the folder; the one non-public static is the one that says "no value".
        var publicStatics = record.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        Assert.Equal(["op_Equality", "op_Inequality"], publicStatics.Select(method => method.Name).Order().ToArray());

        var failed = Assert.Single(
            record.GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly),
            method => method.Name == "Failed");
        var problems = Assert.Single(failed.GetParameters());

        Assert.True(failed.IsAssembly);
        Assert.Equal(record, failed.ReturnType);
        Assert.Equal(typeof(string[]), problems.ParameterType);
        Assert.Equal("problems", problems.Name);
        Assert.True(problems.IsDefined(typeof(ParamArrayAttribute), false));
    }

    /// <summary>The folder's own guard, the way <see cref="RuntimeOpenerSharedTests" /> guards the opener the folder
    /// shares: the constructor is declared once in <c>src/Dashboard/Runtime</c>, on the record, and both parsers say
    /// no value through it instead of keeping a way of their own.</summary>
    [Fact]
    public void TheRuntimeFolderDeclaresTheConstructorOnceAndCallsItFromBothParsers()
    {
        var declaredIn = new List<string>();
        foreach (var path in Directory.EnumerateFiles(MarkupGuards.SourcePath("Runtime"), "*.cs", SearchOption.TopDirectoryOnly))
        {
            if (Regex.IsMatch(File.ReadAllText(path), Declaration))
            {
                declaredIn.Add(Path.GetFileName(path));
            }
        }

        Assert.Equal(["RuntimeManifestParser.cs"], declaredIn.Order().ToList());

        var offenders = new List<string>();
        foreach (var parser in new[] { "RuntimeManifestParser.cs", "DatabaseView.cs" })
        {
            if (!Regex.IsMatch(File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("Runtime", parser))), Call))
            {
                offenders.Add($"{parser}: no call to RuntimeParseResult<...>.Failed");
            }
        }

        Assert.Empty(offenders);

        // A guard that found nothing has seen nothing: the constructor it holds the folder to is the one that is there.
        var record = File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("Runtime", "RuntimeManifestParser.cs")));
        Assert.Contains("internal static RuntimeParseResult<T> Failed(params string[] problems)", record, StringComparison.Ordinal);
    }

    /// <summary>The record's one constructor, kept to the folder like the opener <c>TryOpen</c>: the test reaches it
    /// by name, under the record's own type, the way the tests reach the parser's bounds by their names.</summary>
    private static RuntimeParseResult<T> Failed<T>(params string[] problems)
        where T : class
    {
        var type = typeof(RuntimeParseResult<>).MakeGenericType(typeof(T));
        var make = type.GetMethod("Failed", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (RuntimeParseResult<T>)make.Invoke(null, [problems])!;
    }

    /// <summary>A reader's answer that there is no value: the record says so, with nothing but the opener's line.</summary>
    private static void NoValue<T>(RuntimeParseResult<T> result, string which)
        where T : class
    {
        Assert.False(result.IsValid, $"{which}: an empty file is refused");
        Assert.True(result.Value is null, $"{which}: an empty file carries no value");
        Assert.Equal(["The file is empty."], result.Errors);
    }
}
