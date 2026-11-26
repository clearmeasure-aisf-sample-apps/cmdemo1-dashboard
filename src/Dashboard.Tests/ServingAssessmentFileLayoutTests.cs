using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// Each type of the serving decision has a file of its own under <c>src/Dashboard/Health</c>, named after it, so the
/// per-type tests of <see cref="ServingAssessmentTests" />, <see cref="RotationTests" /> and
/// <see cref="NodeHealthEdgeCaseTests" /> say where a member lives by the file they read (#968). The same for the
/// state of a check and the helper that reads a status code into it: <c>HealthState.cs</c> keeps the state,
/// <c>HealthClassifier.cs</c> the helper - as <see cref="RuntimePayloadFileLayoutTests" /> pins for the payload.
/// </summary>
public class ServingAssessmentFileLayoutTests
{
    private static readonly Regex PublicType = new(
        @"^public[ \t]+(?:(?:sealed|static|partial|abstract)[ \t]+)*(?:class|record|struct|enum|interface)[ \t]+([A-Za-z_][A-Za-z0-9_]*)",
        RegexOptions.Multiline | RegexOptions.Compiled);

    public static TheoryData<string> TypesOfTheServingDecision() => new()
    {
        nameof(NodeHealth),
        nameof(ServingState),
        nameof(FrontDoorAgreement),
        nameof(ServingAssessment),
        nameof(ServingShape),
        nameof(HealthState),
        nameof(HealthClassifier),
    };

    [Theory]
    [MemberData(nameof(TypesOfTheServingDecision))]
    public void EachTypeIsTheOneTypeOfAFileNamedAfterIt(string type)
    {
        var path = Path.Combine(HealthRoot(), type + ".cs");

        Assert.True(File.Exists(path), $"{type} has no file of its own at {path}");
        Assert.Equal([type], TypesIn(path));
        Assert.Contains("namespace Dashboard.Health;", File.ReadAllText(path), StringComparison.Ordinal);
    }

    [Fact]
    public void TheServingFileHoldsNothingButTheAssessment()
    {
        // The three types the issue moved out are gone from the file, and not left behind as a second home.
        var source = File.ReadAllText(Path.Combine(HealthRoot(), "ServingAssessment.cs"));

        foreach (var moved in new[] { nameof(NodeHealth), nameof(ServingState), nameof(FrontDoorAgreement) })
        {
            Assert.DoesNotContain($"public sealed record {moved}", source, StringComparison.Ordinal);
            Assert.DoesNotContain($"public enum {moved}", source, StringComparison.Ordinal);
        }
    }

    private static List<string> TypesIn(string path) =>
        PublicType.Matches(File.ReadAllText(path)).Select(match => match.Groups[1].Value).ToList();

    private static string HealthRoot([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Health"));
}
