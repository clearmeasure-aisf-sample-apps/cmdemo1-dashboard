using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// One rule for "nothing measured" across the build's fact records: a section whose every part comes back without a
/// value is no record, and any one part that does is the record - for each of the five sections alike.
/// </summary>
public class BuildInfoMeasuredNothingTests
{
    private const string Commit = "\"commit\": \"0a1b2c3\"";

    [Theory]
    [InlineData("code", """{ "linesOfCode": null, "files": null, "languages": [] }""")]
    [InlineData("tests", """{ "unit": null, "integration": null, "acceptance": null }""")]
    [InlineData("coverage", """{ "linePercent": null, "branchPercent": null }""")]
    [InlineData("complexity", """{ "average": null, "max": null, "methods": null }""")]
    [InlineData("crap", """{ "max": null, "threshold": null, "overThreshold": null }""")]
    public void ASectionWhoseEveryPartIsNothingIsNoRecord(string section, string parts)
    {
        var build = BuildInfo.Parse($$"""{ {{Commit}}, "{{section}}": {{parts}} }""")!;

        Assert.Null(Record(build, section));
    }

    [Theory]
    [InlineData("code", """{ "files": 3 }""")]
    [InlineData("code", """{ "languages": [ { "name": "C#", "lines": 10 } ] }""")]
    [InlineData("tests", """{ "acceptance": 0 }""")]
    [InlineData("coverage", """{ "branchPercent": 0 }""")]
    [InlineData("complexity", """{ "methods": 1 }""")]
    [InlineData("crap", """{ "threshold": 30 }""")]
    public void ASectionWithAnyOnePartMeasuredIsARecord(string section, string parts)
    {
        var build = BuildInfo.Parse($$"""{ {{Commit}}, "{{section}}": {{parts}} }""")!;

        Assert.NotNull(Record(build, section));
    }

    [Theory]
    [InlineData("code", """{ "files": 3 }""")]
    [InlineData("tests", """{ "unit": 1 }""")]
    [InlineData("coverage", """{ "linePercent": 50 }""")]
    [InlineData("complexity", """{ "max": 2 }""")]
    [InlineData("crap", """{ "max": 4 }""")]
    public void OneMeasuredSectionAloneIsABuild(string section, string parts) =>
        Assert.NotNull(Record(BuildInfo.Parse($$"""{ "{{section}}": {{parts}} }""")!, section));

    private static object? Record(BuildInfo build, string section) => section switch
    {
        "code" => build.Code,
        "tests" => build.Tests,
        "coverage" => build.Coverage,
        "complexity" => build.Complexity,
        "crap" => build.Crap,
        _ => throw new ArgumentOutOfRangeException(nameof(section), section, null),
    };
}
