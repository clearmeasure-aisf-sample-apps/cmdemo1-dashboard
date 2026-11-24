using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// Each of the three levels of the model of a tile has a file of its own under <c>src/Dashboard/Health</c>, named
/// after it (#1005): <c>TargetKind.cs</c> and <c>TargetStatus.cs</c> for an endpoint, <c>DeployableStatus.cs</c> for a
/// deployable, <c>EnvironmentStatus.cs</c> for an environment - as <see cref="ServingAssessmentFileLayoutTests" /> pins
/// for the serving decision. The tests of <see cref="TargetStatusTests" />, <see cref="TargetKindEdgeCaseTests" />,
/// <see cref="TargetStatusEdgeCaseTests" />, <see cref="DeployableStatusEdgeCaseTests" /> and
/// <see cref="EnvironmentStatusEdgeCaseTests" /> read the type of the file named after it.
/// </summary>
public class TargetStatusFileLayoutTests
{
    private static readonly Regex PublicType = new(
        @"^public[ \t]+(?:(?:sealed|static|partial|abstract)[ \t]+)*(?:class|record|struct|enum|interface)[ \t]+([A-Za-z_][A-Za-z0-9_]*)",
        RegexOptions.Multiline | RegexOptions.Compiled);

    public static TheoryData<string> TypesOfATile() => new()
    {
        nameof(TargetKind),
        nameof(TargetStatus),
        nameof(DeployableStatus),
        nameof(EnvironmentStatus),
    };

    [Theory]
    [MemberData(nameof(TypesOfATile))]
    public void EachTypeIsTheOneTypeOfAFileNamedAfterIt(string type)
    {
        var path = Path.Combine(HealthRoot(), type + ".cs");

        Assert.True(File.Exists(path), $"{type} has no file of its own at {path}");
        Assert.Equal([type], TypesIn(path));
        Assert.Contains("namespace Dashboard.Health;", File.ReadAllText(path), StringComparison.Ordinal);
    }

    [Fact]
    public void TheTargetStatusFileHoldsNothingButTheTile()
    {
        // The three types the issue moved out are gone from the file, and not left behind as a second home.
        var source = File.ReadAllText(Path.Combine(HealthRoot(), "TargetStatus.cs"));

        Assert.DoesNotContain($"public enum {nameof(TargetKind)}", source, StringComparison.Ordinal);
        Assert.DoesNotContain($"public sealed class {nameof(DeployableStatus)}", source, StringComparison.Ordinal);
        Assert.DoesNotContain($"public sealed class {nameof(EnvironmentStatus)}", source, StringComparison.Ordinal);
    }

    private static List<string> TypesIn(string path) =>
        PublicType.Matches(File.ReadAllText(path)).Select(match => match.Groups[1].Value).ToList();

    private static string HealthRoot([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Health"));
}
