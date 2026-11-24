using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// Each deployment fact has one home under <c>src/Dashboard/Health</c> (#642): <c>DeploymentsReport.cs</c> keeps the
/// file and its <c>Parse</c>/<c>Marks</c>/<c>Rank</c>, <c>Deployment.cs</c> one entry beside the
/// <see cref="DeploymentState"/> it reads, <c>DeploymentMark.cs</c> the mark with its <c>Of</c> and <c>NameOf</c>, and
/// <c>DeploymentText.cs</c> the words of the page - as <see cref="ServingAssessmentFileLayoutTests" /> pins for the
/// serving decision.
/// </summary>
public class DeploymentsReportFileLayoutTests
{
    private static readonly Regex PublicType = new(
        @"^public[ \t]+(?:(?:sealed|static|partial|abstract)[ \t]+)*(?:class|record|struct|enum|interface)[ \t]+([A-Za-z_][A-Za-z0-9_]*)",
        RegexOptions.Multiline | RegexOptions.Compiled);

    public static TheoryData<string, string[]> FilesOfTheDeployments() => new()
    {
        { "DeploymentsReport.cs", [nameof(DeploymentsReport)] },
        { "Deployment.cs", [nameof(Deployment), nameof(DeploymentState)] },
        { "DeploymentMark.cs", [nameof(DeploymentMark)] },
        { "DeploymentText.cs", [nameof(DeploymentText)] },
    };

    [Theory]
    [MemberData(nameof(FilesOfTheDeployments))]
    public void EachFileHoldsItsOwnTypesAndNoOthers(string file, string[] types)
    {
        var path = Path.Combine(HealthRoot(), file);

        Assert.True(File.Exists(path), $"{file} is missing at {path}");
        Assert.Equal(types, TypesIn(path));
        Assert.Contains("namespace Dashboard.Health;", File.ReadAllText(path), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("DeploymentsReport.cs", "public static DeploymentsReport? Parse(")]
    [InlineData("DeploymentsReport.cs", "public IReadOnlyList<DeploymentMark> Marks(")]
    [InlineData("DeploymentsReport.cs", "internal static MarkPlace Rank(")]
    [InlineData("Deployment.cs", "internal static Deployment? Read(")]
    [InlineData("DeploymentMark.cs", "internal static DeploymentMark Of(")]
    [InlineData("DeploymentMark.cs", "public static string? NameOf(")]
    [InlineData("DeploymentText.cs", "public static string Sentence(")]
    public void EachMemberLivesBesideItsType(string file, string member)
    {
        Assert.Contains(member, File.ReadAllText(Path.Combine(HealthRoot(), file)), StringComparison.Ordinal);
    }

    private static List<string> TypesIn(string path) =>
        PublicType.Matches(File.ReadAllText(path)).Select(match => match.Groups[1].Value).ToList();

    private static string HealthRoot([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Health"));
}
