using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// Only <see cref="RuntimePayload.ToJson" /> writes the update <c>js/runtime.js</c> is handed. The camelCase,
/// nulls-left-out contract lives in the <c>[JsonSourceGenerationOptions]</c> beside the records, not on the records,
/// so a caller that reaches for <c>JsonSerializer</c> on its own, or builds a <see cref="RuntimePayloadJson" /> of
/// options of its own, writes bytes the page silently fails to draw (<see cref="RuntimePayloadSurfaceEdgeCaseTests" />
/// pins both sets of bytes). The guard below reads every <c>*.cs</c> of <c>src/Dashboard</c> and refuses, outside the
/// files named in <see cref="AllowedWriters" />: a <c>JsonSerializer</c> call in a file that names a payload record
/// (the source cannot tell the type of an argument, so naming one is what counts), a hand-built
/// <see cref="RuntimePayloadJson" />, and any <c>JsonSerializerOptions</c> constructed at all. It prints the file and
/// the line it refuses, the way <see cref="RuntimeWorkHelperCoverageTests" /> does. A caller that ever needs the
/// bytes legitimately (a build step, a fixture generator) is named in the allowed list, never "any file".
/// </summary>
public class RuntimePayloadWriterGuardTests
{
    /// <summary>The files that may write a payload, each with why - and nothing else.</summary>
    private static readonly IReadOnlyDictionary<string, string> AllowedWriters = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["Runtime/RuntimePayload.cs"] = "RuntimePayload.ToJson, the one writer of the update, beside the options it is declared with",
    };

    private static readonly Regex SerializerCall = new(@"\bJsonSerializer\s*\.\s*(Serialize|Deserialize)\w*\s*[<(]", RegexOptions.CultureInvariant);

    private static readonly Regex HandBuiltContext = new(
        @"\bnew\s+(?:[\w.]+\.)?RuntimePayloadJson\s*\(|\bRuntimePayloadJson\s+\w+\s*=\s*new\s*\(", RegexOptions.CultureInvariant);

    private static readonly Regex OptionsConstruction = new(
        @"\bnew\s+(?:[\w.]+\.)?JsonSerializerOptions\s*[({]|\bJsonSerializerOptions\??\s+\w+\s*=\s*new\s*\(", RegexOptions.CultureInvariant);

    [Fact]
    public void NoFileOfTheAppButTheAllowedOnesWritesAPayload()
    {
        var refused = AppFiles()
            .Where(file => !AllowedWriters.ContainsKey(file))
            .SelectMany(file => Refusals(file, File.ReadAllText(Path.Combine(DashboardRoot(), file))))
            .ToList();

        Assert.True(
            refused.Count == 0,
            "only RuntimePayload.ToJson may write the update js/runtime.js reads (or name the file in AllowedWriters):\n"
            + string.Join("\n", refused));
    }

    [Fact]
    public void TheGuardReadsTheAppAndFindsTheOneWriter()
    {
        var files = AppFiles();

        Assert.True(files.Count >= 50, $"only {files.Count} file(s) were read under {DashboardRoot()}: the guard reads nothing");
        Assert.Contains("Runtime/RuntimePayloadBuilder.cs", files);

        foreach (var allowed in AllowedWriters.Keys)
        {
            Assert.Contains(allowed, files);
        }

        // The one writer is where the guard expects it, and the guard would refuse it anywhere else.
        var writer = File.ReadAllText(Path.Combine(DashboardRoot(), "Runtime/RuntimePayload.cs"));
        Assert.Contains("JsonSerializer.Serialize(this, RuntimePayloadJson.Default.RuntimePayload)", writer, StringComparison.Ordinal);
        Assert.Single(Refusals("Runtime/RuntimePayload.cs", writer));

        // The records the guard knows are the ones the writer is declared for.
        var records = PayloadRecordNames();
        Assert.True(records.Count >= 10, $"only {records.Count} payload record(s) were read from RuntimePayloadJson");
        Assert.Contains(nameof(RuntimePayload), records);
        Assert.Contains(nameof(RuntimeTile), records);
        Assert.Contains(nameof(RuntimeLink), records);
        Assert.DoesNotContain(nameof(RuntimeManifest), records);
    }

    [Theory]
    [InlineData("var json = JsonSerializer.Serialize(tile, new RuntimePayloadJson(new JsonSerializerOptions()).RuntimeTile); // RuntimeTile", 3)]
    [InlineData("RuntimeTile tile = Make(); var json = JsonSerializer.Serialize(tile);", 1)]
    [InlineData("var back = JsonSerializer.Deserialize<RuntimePayload>(json);", 1)]
    [InlineData("var back = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(payload); // RuntimePayload", 1)]
    [InlineData("RuntimePayloadJson context = new(options);", 1)]
    [InlineData("JsonSerializerOptions options = new() { WriteIndented = true };", 1)]
    [InlineData("var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };", 1)]
    [InlineData("var json = JsonSerializer.Serialize(manifest); // RuntimeManifest only", 0)]
    [InlineData("var json = payload.ToJson(); // RuntimePayload", 0)]
    public void TheGuardRefusesEachDoorItNames(string line, int refusals)
    {
        Assert.Equal(refusals, Refusals("Runtime/Elsewhere.cs", line).Count);
    }

    [Fact]
    public void TheGuardPrintsTheFileAndTheLine()
    {
        var refused = Assert.Single(Refusals("Runtime/Elsewhere.cs", "namespace X;\n\nvar o = new JsonSerializerOptions();\n"));

        Assert.StartsWith("src/Dashboard/Runtime/Elsewhere.cs:3: ", refused, StringComparison.Ordinal);
    }

    /// <summary>What a file does that only an allowed writer may, each as <c>file:line: what</c>.</summary>
    private static List<string> Refusals(string file, string text)
    {
        var refused = new List<string>();

        void Refuse(Regex door, string what)
        {
            foreach (Match match in door.Matches(text))
            {
                refused.Add($"src/Dashboard/{file}:{LineOf(text, match.Index)}: {what}");
            }
        }

        var names = PayloadRecordNames().Where(name => Regex.IsMatch(text, $@"\b{name}\b")).ToList();

        if (names.Count > 0)
        {
            Refuse(SerializerCall, $"JsonSerializer called in a file that names the payload record(s) {string.Join(", ", names)}");
        }

        Refuse(HandBuiltContext, "a RuntimePayloadJson built by hand writes bytes of other options");
        Refuse(OptionsConstruction, "a JsonSerializerOptions built here is not the contract declared beside the records");

        return refused;
    }

    /// <summary>The records the update is made of: every type of the app <see cref="RuntimePayloadJson" /> has a
    /// member for, or a list of.</summary>
    private static List<string> PayloadRecordNames() =>
        typeof(RuntimePayloadJson).GetProperties()
            .Select(property => property.PropertyType)
            .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(JsonTypeInfo<>))
            .Select(type => type.GetGenericArguments()[0])
            .SelectMany(type => type.IsGenericType ? type.GetGenericArguments() : [type])
            .Where(type => type.Assembly == typeof(RuntimePayload).Assembly)
            .Select(type => type.Name)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

    private static int LineOf(string text, int at) => text.AsSpan(0, at).Count('\n') + 1;

    /// <summary>Every file of the app, as a path under <c>src/Dashboard</c> with forward slashes, the build's
    /// copies under <c>bin</c> and <c>obj</c> walked past.</summary>
    private static List<string> AppFiles()
    {
        var root = DashboardRoot();

        return Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/'))
            .Where(relative => !relative.Split('/').Any(part =>
                part.Equals("bin", StringComparison.OrdinalIgnoreCase) || part.Equals("obj", StringComparison.OrdinalIgnoreCase)))
            .OrderBy(relative => relative, StringComparer.Ordinal)
            .ToList();
    }

    private static string DashboardRoot([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard"));
}
