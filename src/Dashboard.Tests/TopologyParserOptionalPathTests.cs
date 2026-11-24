namespace Dashboard.Tests;

/// <summary>
/// The one rule of an optional path in <see cref="TopologyParser" /> - a word becomes a path with its front slash, and
/// anything else is no path and no error - holds for the <c>buildPath</c> of <c>system.dashboard</c> as it does for
/// the three optional paths of a deployable (<see cref="TopologyParserEdgeCaseTests" />): both are read by one reader.
/// </summary>
public class TopologyParserOptionalPathTests
{
    [Theory]
    [InlineData("null")]
    [InlineData("\"\"")]
    [InlineData("\"   \"")]
    [InlineData("7")]
    [InlineData("true")]
    [InlineData("[]")]
    [InlineData("{}")]
    public void ADashboardBuildPathOfNothingIsNoDashboardAndNoError(string path)
    {
        var result = WithSystem($$"""{ "dashboard": { "name": "Board", "buildPath": {{path}} } }""");

        Assert.Empty(result.Errors);
        Assert.Null(result.Topology!.System.Dashboard);
    }

    [Theory]
    [InlineData("\"_build\"", "/_build")]
    [InlineData("\" _build \"", "/_build")]
    [InlineData("\"/already/here\"", "/already/here")]
    [InlineData("\"//twice\"", "//twice")]
    public void ADashboardBuildPathTakesItsSlashAndItsTrimLikeEveryOptionalPath(string path, string expected)
    {
        var deployable = TopologyParser.Parse($$"""
            { "environments": [ { "name": "tdd", "deployables": [ { "buildPath": {{path}} } ] } ] }
            """).Topology!.Environments[0].Deployables[0];
        var dashboard = WithSystem($$"""{ "dashboard": { "buildPath": {{path}} } }""").Topology!.System.Dashboard!;

        Assert.Equal(expected, dashboard.BuildPath);
        Assert.Equal(deployable.BuildPath, dashboard.BuildPath);
    }

    private static TopologyParseResult WithSystem(string system) =>
        TopologyParser.Parse($$"""{ "system": {{system}}, "environments": [] }""");
}
