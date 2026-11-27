using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The entry words of the deployments file that <see cref="Deployment" /> and <see cref="RecentDeployment" /> share -
/// <c>project</c>, <c>environment</c>, <c>release</c>, <c>finished</c>, <c>startedBy</c> and <c>url</c> - are named
/// once on <c>Deployment</c> as <c>...Word</c> consts and read by both <c>Read</c> methods, so a rename in the file's
/// contract cannot silently lose a part in one list while the other still reads it.
/// <para>
/// The six shared consts are internal and the app declares no <c>InternalsVisibleTo</c> (as
/// <c>Dashboard.Tests.csproj</c> notes of <c>JsonRead</c>), so their text is pinned by reflection under each name; the
/// agreement is pinned through <see cref="DeploymentsReport.Parse" />, which reads a <c>deployments</c> entry and a
/// <c>recent</c> entry built with the same six values and checks that both records carry them.
/// </para>
/// </summary>
public class DeploymentEntryWordsTests
{
    /// <summary>The text of one internal const of <see cref="Deployment" />, reached by reflection under its own name.</summary>
    private static string WordOf(string name)
    {
        var field = typeof(Deployment).GetField(name, BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(field);
        Assert.True(field!.IsLiteral, $"{name} is a const.");
        Assert.True(field.IsAssembly, $"{name} is internal, so RecentDeployment.Read can read it.");
        Assert.Null(typeof(Deployment).GetField(name, BindingFlags.Public | BindingFlags.Static));
        return (string)field.GetRawConstantValue()!;
    }

    /// <summary>Each of the six shared entry words is the text the file contract states, and not a different spelling.</summary>
    [Theory]
    [InlineData("ProjectWord", "project")]
    [InlineData("EnvironmentWord", "environment")]
    [InlineData("ReleaseWord", "release")]
    [InlineData("FinishedWord", "finished")]
    [InlineData("StartedByWord", "startedBy")]
    [InlineData("UrlWord", "url")]
    public void EachSharedConstIsTheTextOfItsEntryWord(string name, string word)
    {
        Assert.Equal(word, WordOf(name));
    }

    /// <summary>
    /// A <c>deployments</c> entry and a <c>recent</c> entry written with the same <c>project</c>, <c>environment</c>,
    /// <c>release</c>, <c>finished</c>, <c>startedBy</c> and <c>url</c> are read by their two <c>Read</c> methods, and
    /// both records carry the six parts.
    /// </summary>
    [Fact]
    public void BothListsCarryTheSixSharedParts()
    {
        var project = "demo-ui";
        var environment = "uat";
        var release = "2.4.43";
        var finished = "2026-10-09T07:00:00Z";
        var startedBy = "ai-ops";
        var url = "https://octopus.example.invalid/app#/Spaces-5";

        var deploymentEntry = $$"""{ "project": "{{project}}", "environment": "{{environment}}", "release": "{{release}}", "state": "succeeded", "finished": "{{finished}}", "url": "{{url}}", "startedBy": "{{startedBy}}" }""";
        var recentEntry = $$"""{ "project": "{{project}}", "environment": "{{environment}}", "release": "{{release}}", "result": "succeeded", "finished": "{{finished}}", "url": "{{url}}", "startedBy": "{{startedBy}}" }""";

        var report = DeploymentsReport.Parse($$"""{ "deployments": [ {{deploymentEntry}} ], "recent": [ {{recentEntry}} ] }""");

        Assert.NotNull(report);
        var deployment = Assert.Single(report!.Deployments);
        var recent = Assert.Single(report.Recent);

        Assert.Equal(project, deployment.Project);
        Assert.Equal(environment, deployment.Environment);
        Assert.Equal(release, deployment.Release);
        Assert.Equal(new DateTimeOffset(2026, 10, 9, 7, 0, 0, TimeSpan.Zero), deployment.Finished);
        Assert.Equal(startedBy, deployment.StartedBy);
        Assert.Equal(new Uri(url), deployment.Url);

        Assert.Equal(project, recent.Project);
        Assert.Equal(environment, recent.Environment);
        Assert.Equal(release, recent.Release);
        Assert.Equal(new DateTimeOffset(2026, 10, 9, 7, 0, 0, TimeSpan.Zero), recent.Finished);
        Assert.Equal(startedBy, recent.StartedBy);
        Assert.Equal(new Uri(url), recent.Url);
    }
}
