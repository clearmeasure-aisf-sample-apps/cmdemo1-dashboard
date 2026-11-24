using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// "A record of nothing is no record" has one owner (#1334): a measure section said but empty is no record, the build
/// of only empty sections is no build, and an empty section does not hide a measured one beside it.
/// </summary>
public class BuildInfoMeasuredFactsRuleTests
{
    [Fact]
    public void EverySectionSaidButEmptyIsNoBuild() =>
        Assert.Null(BuildInfo.Parse("""
            { "code": {}, "tests": {}, "coverage": {}, "complexity": {}, "crap": {}, "analysis": {} }
            """));

    [Fact]
    public void AnEmptySectionBesideAMeasuredOneIsNoRecordWhileTheOtherIs()
    {
        var build = BuildInfo.Parse("""{ "coverage": {}, "crap": { "overThreshold": 0 } }""")!;

        Assert.Null(build.Coverage);
        Assert.Equal(new CrapFacts(null, null, 0), build.Crap);
    }

    [Theory]
    [InlineData("tests", """{ "unit": 0, "integration": null }""")]
    [InlineData("coverage", """{ "linePercent": 0 }""")]
    [InlineData("complexity", """{ "average": 0 }""")]
    [InlineData("crap", """{ "max": 0 }""")]
    public void AZeroIsAMeasureNotNothing(string section, string parts)
    {
        var build = BuildInfo.Parse($$"""{ "{{section}}": {{parts}} }""");

        Assert.NotNull(build);
        Assert.NotNull(section switch
        {
            "tests" => (object?)build.Tests,
            "coverage" => build.Coverage,
            "complexity" => build.Complexity,
            _ => build.Crap,
        });
    }
}
