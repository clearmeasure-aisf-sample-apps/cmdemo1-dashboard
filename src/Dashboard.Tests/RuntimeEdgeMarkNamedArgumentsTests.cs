using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Dashboard.Runtime;

namespace Dashboard.Tests;

/// <summary>
/// #1511: <see cref="RuntimeEdgeMark"/> takes four adjacent strings that may be nothing (Number, Unit, Text) before its
/// Title, so a positional build that swaps two of them compiles. The builder names every argument of every edge mark
/// it makes, so a swap reads wrong at the call site; and the wire shape of a mark stays what the script reads.
/// </summary>
public class RuntimeEdgeMarkNamedArgumentsTests
{
    private static readonly List<string> Parts = ["Id", "State", "Number", "Unit", "Text", "Title"];

    private static readonly string[] Optional = ["Link", "Trend"];

    [Fact]
    public void TheBuilderNamesEveryArgumentOfEveryEdgeMarkItMakes()
    {
        var builds = EdgeMarkBuilds(File.ReadAllText(BuilderPath()));

        Assert.Equal(7, builds.Count);
        Assert.All(builds, arguments => Assert.Equal(Parts, NamesOf(arguments).Take(Parts.Count).ToList()));
        Assert.All(builds, arguments => Assert.All(NamesOf(arguments).Skip(Parts.Count), name => Assert.Contains(name, Optional)));
    }

    [Fact]
    public void AFullEdgeMarkIsWrittenInCamelCaseInTheOrderOfItsParts()
    {
        var mark = new RuntimeEdgeMark(
            Id: "e",
            State: "active",
            Number: "12",
            Unit: "req/min",
            Text: "role",
            Title: "t",
            Link: new RuntimeLink("https://example.test/", "logs"),
            Trend: new RuntimeTrend([0.5], "0.5 req/min"));

        Assert.Equal(
            """{"nodes":[],"regions":[],"edges":[{"id":"e","state":"active","number":"12","unit":"req/min","text":"role","title":"t","link":{"href":"https://example.test/","title":"logs"},"trend":{"points":[0.5],"title":"0.5 req/min"}}]}""",
            new RuntimePayload([], [], [mark]).ToJson());
    }

    [Fact]
    public void AnEdgeMarkWithoutANumberLineLeavesTheTripleOutOfTheWire()
    {
        var mark = new RuntimeEdgeMark(Id: "e", State: "neutral", Number: null, Unit: null, Text: null, Title: "t");

        Assert.Equal("""{"nodes":[],"regions":[],"edges":[{"id":"e","state":"neutral","title":"t"}]}""", new RuntimePayload([], [], [mark]).ToJson());
    }

    /// <summary>The text between the parentheses of each <c>new RuntimeEdgeMark(</c>, matched to its closing one.</summary>
    private static List<string> EdgeMarkBuilds(string source)
    {
        const string Opening = "new RuntimeEdgeMark(";
        var builds = new List<string>();
        for (var at = source.IndexOf(Opening, StringComparison.Ordinal); at >= 0; at = source.IndexOf(Opening, at + 1, StringComparison.Ordinal))
        {
            var start = at + Opening.Length;
            var depth = 1;
            var end = start;
            for (; depth > 0; end++)
            {
                depth += source[end] switch { '(' => 1, ')' => -1, _ => 0 };
            }

            builds.Add(source[start..(end - 1)]);
        }

        return builds;
    }

    /// <summary>The name each argument line starts with: the builder writes one argument per line, each one named.</summary>
    private static List<string> NamesOf(string arguments) =>
        [.. arguments
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .Select(line => Regex.Match(line, @"^([A-Z][A-Za-z]*): ") is { Success: true } named ? named.Groups[1].Value : "<positional>")];

    private static string BuilderPath([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Runtime", "RuntimePayloadBuilder.cs"));
}
