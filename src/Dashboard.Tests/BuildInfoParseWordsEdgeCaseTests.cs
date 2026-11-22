namespace Dashboard.Tests;

/// <summary>
/// The edges of <see cref="BuildInfo.Parse" /> that <see cref="BuildInfoTests" />, <see cref="BuildInfoEdgeCaseTests" />
/// and <see cref="BuildInfoParseBoundaryEdgeCaseTests" /> leave open: the words of an answer trimmed and stripped of
/// build metadata on the way into the build, the part that says nothing next to a part that does, a section said as
/// the wrong kind or holding only values the reader does not take next to a fact, and the run's address that is not a
/// web address. Every input is a fixed string; nothing reaches the network or reads the clock.
/// </summary>
public class BuildInfoParseWordsEdgeCaseTests
{
    private const string Fact = "\"commit\": \"0a1b2c3d\"";

    /// <summary>The version and the commit lose the spaces around them, and the version loses its build metadata
    /// and the spaces before the metadata too.</summary>
    [Theory]
    [InlineData("  2.4.15  ", "2.4.15")]
    [InlineData("2.4.15+0a1b2c3", "2.4.15")]
    [InlineData(" 2.4.15 +0a1b2c3 ", "2.4.15")]
    [InlineData("2.4.15-rc.1+0a1b2c3+again", "2.4.15-rc.1")]
    public void TheVersionIsTrimmedAndLosesItsBuildMetadata(string said, string expected)
    {
        var build = BuildInfo.Parse($$"""{ "version": "{{said}}" }""");

        Assert.NotNull(build);
        Assert.Equal(expected, build!.Version);
    }

    [Fact]
    public void TheCommitIsTrimmedButKeptWhole()
    {
        var build = BuildInfo.Parse("""{ "commit": "\t 0a1b2c3d4e5f60718293a4b5c6d7e8f901234567 \n" }""")!;

        Assert.Equal("0a1b2c3d4e5f60718293a4b5c6d7e8f901234567", build.Commit);
        Assert.Null(build.Version);
    }

    /// <summary>A version that is nothing but metadata, or said as another kind, is an absent version, and the
    /// build stands on the commit beside it.</summary>
    [Theory]
    [InlineData("\"+0a1b2c3\"")]
    [InlineData("\"   \"")]
    [InlineData("2.4")]
    [InlineData("null")]
    [InlineData("[ \"2.4.15\" ]")]
    public void AVersionThatSaysNothingLeavesTheBuildOfTheOtherFacts(string version)
    {
        var build = BuildInfo.Parse($$"""{ "version": {{version}}, {{Fact}} }""");

        Assert.NotNull(build);
        Assert.Null(build!.Version);
        Assert.Equal("0a1b2c3d", build.Commit);
    }

    /// <summary>Each section said as something other than an object is the absent section, and the build stands
    /// on the fact beside it.</summary>
    [Theory]
    [InlineData("code")]
    [InlineData("tests")]
    [InlineData("coverage")]
    [InlineData("complexity")]
    [InlineData("crap")]
    [InlineData("analysis")]
    public void ASectionOfTheWrongKindIsAbsent(string section)
    {
        foreach (var value in new[] { "[ ]", "[ { } ]", "5", "\"5\"", "true", "null" })
        {
            var build = BuildInfo.Parse($$"""{ "{{section}}": {{value}}, {{Fact}} }""")!;

            Assert.Equal("0a1b2c3d", build.Commit);
            Assert.Null(build.Code);
            Assert.Null(build.Tests);
            Assert.Null(build.Coverage);
            Assert.Null(build.Complexity);
            Assert.Null(build.Crap);
            Assert.Null(build.QodanaProblems);
        }
    }

    /// <summary>A section whose values are all null, words, flags or negatives is the absent section, not a
    /// record of nothing.</summary>
    [Fact]
    public void ASectionOfValuesTheReaderDoesNotTakeIsAbsent()
    {
        var build = BuildInfo.Parse($$"""
            { {{Fact}},
              "tests": { "unit": null, "integration": "240", "acceptance": -1 },
              "coverage": { "linePercent": "81.2", "branchPercent": true },
              "complexity": { "average": null, "max": [ 34 ], "methods": -5210 },
              "crap": { "max": "28.5", "threshold": false, "overThreshold": -1 },
              "analysis": { "qodanaProblems": "3" } }
            """)!;

        Assert.Equal("0a1b2c3d", build.Commit);
        Assert.Null(build.Tests);
        Assert.Null(build.Coverage);
        Assert.Null(build.Complexity);
        Assert.Null(build.Crap);
        Assert.Null(build.QodanaProblems);
    }

    /// <summary>One good value in a section is enough: the section is a record of that value and absent for the
    /// rest.</summary>
    [Fact]
    public void OneGoodValueMakesTheSectionAndTheRestStayAbsent()
    {
        var build = BuildInfo.Parse("""
            { "tests": { "unit": null, "integration": "240", "acceptance": 168 },
              "coverage": { "linePercent": "81.2", "branchPercent": 70.1 },
              "complexity": { "average": null, "max": 34, "methods": -1 },
              "crap": { "max": "28.5", "threshold": 30, "overThreshold": -1 } }
            """)!;

        Assert.Equal(new TestCounts(null, null, 168), build.Tests);
        Assert.Equal(new CoverageFacts(null, 70.1), build.Coverage);
        Assert.Equal(new ComplexityFacts(null, 34, null), build.Complexity);
        Assert.Equal(new CrapFacts(null, 30, null), build.Crap);
        Assert.Null(build.Version);
        Assert.Null(build.Code);
        Assert.Null(build.QodanaProblems);
    }

    /// <summary>The run's address is a web address or nothing: an address of another scheme, or a relative one,
    /// alone is no build, and next to a commit is the absent address.</summary>
    [Theory]
    [InlineData("ftp://github.example.net/o/r/actions/runs/1")]
    [InlineData("file:///tmp/run")]
    [InlineData("javascript:alert(1)")]
    [InlineData("/o/r/actions/runs/1")]
    [InlineData("github.example.net/o/r/actions/runs/1")]
    [InlineData("")]
    public void ARunAddressThatIsNotHttpIsNoAddress(string address)
    {
        Assert.Null(BuildInfo.Parse($$"""{ "buildUrl": "{{address}}" }"""));

        var build = BuildInfo.Parse($$"""{ "buildUrl": "{{address}}", {{Fact}} }""")!;

        Assert.Null(build.BuildUrl);
        Assert.Equal("0a1b2c3d", build.Commit);
    }

    /// <summary>The run's address over plain http, and with padding, is kept as the absolute address it says.</summary>
    [Fact]
    public void ARunAddressOverHttpIsKeptTrimmed()
    {
        var build = BuildInfo.Parse("""{ "buildUrl": "  http://ci.example.net/runs/7?attempt=2  " }""")!;

        Assert.Equal(new Uri("http://ci.example.net/runs/7?attempt=2"), build.BuildUrl);
        Assert.Null(build.Commit);
    }
}
