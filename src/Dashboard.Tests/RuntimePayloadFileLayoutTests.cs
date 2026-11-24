using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Dashboard.Runtime;

namespace Dashboard.Tests;

/// <summary>
/// Each record of the runtime payload has a file of its own under <c>src/Dashboard/Runtime</c>, named after it, so
/// a record's tests have a file to be named after; <c>RuntimePayload.cs</c> keeps just the payload and the
/// <see cref="RuntimePayloadJson"/> context that writes it (#1263).
/// </summary>
public class RuntimePayloadFileLayoutTests
{
    private static readonly Regex PublicType = new(
        @"^public[ \t]+(?:(?:sealed|static|partial|abstract)[ \t]+)*(?:class|record|struct|enum|interface)[ \t]+([A-Za-z_][A-Za-z0-9_]*)",
        RegexOptions.Multiline | RegexOptions.Compiled);

    public static TheoryData<string> PartsOfThePayload() => new()
    {
        nameof(RuntimeTile),
        nameof(RuntimeDeployment),
        nameof(RuntimeLink),
        nameof(RuntimeTextPart),
        nameof(RuntimeTrend),
        nameof(RuntimeTileLine),
        nameof(RuntimeCheckMark),
        nameof(RuntimeRegionMark),
        nameof(RuntimeName),
        nameof(RuntimeEdgeMark),
    };

    [Theory]
    [MemberData(nameof(PartsOfThePayload))]
    public void EachPartOfThePayloadIsTheOneTypeOfAFileNamedAfterIt(string record)
    {
        var path = Path.Combine(RuntimeRoot(), record + ".cs");

        Assert.True(File.Exists(path), $"{record} has no file of its own at {path}");
        Assert.Equal([record], TypesIn(path));
        Assert.Contains("namespace Dashboard.Runtime;", File.ReadAllText(path), StringComparison.Ordinal);
    }

    [Fact]
    public void ThePayloadFileKeepsJustThePayloadAndItsJsonContext()
    {
        Assert.Equal(
            [nameof(RuntimePayload), nameof(RuntimePayloadJson)],
            TypesIn(Path.Combine(RuntimeRoot(), "RuntimePayload.cs")));
    }

    [Fact]
    public void TheContextStillWritesThePayload()
    {
        var serializable = typeof(RuntimePayloadJson)
            .GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonSerializableAttribute), false)
            .Cast<System.Text.Json.Serialization.JsonSerializableAttribute>()
            .ToList();

        Assert.NotEmpty(serializable);
        Assert.NotNull(RuntimePayloadJson.Default.GetTypeInfo(typeof(RuntimePayload)));
    }

    private static List<string> TypesIn(string path) =>
        PublicType.Matches(File.ReadAllText(path)).Select(match => match.Groups[1].Value).ToList();

    private static string RuntimeRoot([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Runtime"));
}
