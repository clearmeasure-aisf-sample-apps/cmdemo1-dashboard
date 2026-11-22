namespace Dashboard.Tests;

/// <summary>
/// The edges of <see cref="BuildInfo.Parse" /> that <see cref="BuildInfoTests" /> and
/// <see cref="BuildInfoEdgeCaseTests" /> leave open: the white space around a whole answer, the JSON an answer may
/// not use (comments, nesting deeper than the reader goes), a name said twice, the counts at the very ends of the
/// ranges a build reports them in, a fractional count, and the moment's own offset. Every input is a fixed string;
/// nothing reaches the network or reads the clock.
/// </summary>
public class BuildInfoParseBoundaryEdgeCaseTests
{
    /// <summary>White space before and after the object is no part of the answer: the object is read as it is.</summary>
    [Fact]
    public void WhiteSpaceAroundTheObjectIsNotPartOfTheAnswer()
    {
        var build = BuildInfo.Parse("\r\n\t  { \"version\": \"2.4.15\", \"commit\": \"0a1b2c3d\" }  \n\t");

        Assert.NotNull(build);
        Assert.Equal("2.4.15", build!.Version);
        Assert.Equal("0a1b2c3d", build.Commit);
    }

    /// <summary>Comments are not JSON: an answer that carries one is no answer at all, even around good facts.</summary>
    [Theory]
    [InlineData("""{ /* the version */ "version": "2.4.15" }""")]
    [InlineData("{ \"version\": \"2.4.15\" // trailing\n }")]
    public void AnAnswerWithACommentIsNoBuild(string answer) => Assert.Null(BuildInfo.Parse(answer));

    /// <summary>An answer nested deeper than the reader's limit is no build, while the same fact beside a shallow
    /// nest is read.</summary>
    [Fact]
    public void AnAnswerNestedBeyondTheReadersDepthIsNoBuild()
    {
        var tooDeep = "{ \"version\": \"2.4.15\", \"x\": " + new string('[', 100) + new string(']', 100) + " }";
        var shallow = "{ \"version\": \"2.4.15\", \"x\": " + new string('[', 10) + new string(']', 10) + " }";

        Assert.Null(BuildInfo.Parse(tooDeep));
        Assert.Equal("2.4.15", BuildInfo.Parse(shallow)!.Version);
    }

    /// <summary>Names the build does not ask about are left alone, whatever they hold.</summary>
    [Fact]
    public void NamesTheBuildDoesNotAskAboutAreIgnored()
    {
        var build = BuildInfo.Parse(
            """{ "status": "Healthy", "version": "2.4.15", "extra": { "code": { "files": 9 } }, "list": [ 1, 2, 3 ] }""")!;

        Assert.Equal("2.4.15", build.Version);
        Assert.Null(build.Code);
        Assert.Null(build.Commit);
    }

    /// <summary>A name said twice is still an answer: the build holds one of the two values, never both and never
    /// nothing.</summary>
    [Fact]
    public void ANameSaidTwiceStillMakesABuildOfOneOfItsValues()
    {
        var build = BuildInfo.Parse("""{ "version": "1.0.0", "version": "2.0.0" }""");

        Assert.NotNull(build);
        Assert.True(build!.Version is "1.0.0" or "2.0.0", build.Version);
    }

    /// <summary>The count of problems runs to the largest <see cref="int" /> and no further: one past it is no
    /// count, and an answer of nothing but that is no build.</summary>
    [Fact]
    public void TheCountOfProblemsEndsAtTheLargestInt()
    {
        Assert.Equal(int.MaxValue, BuildInfo.Parse("""{ "analysis": { "qodanaProblems": 2147483647 } }""")!.QodanaProblems);
        Assert.Null(BuildInfo.Parse("""{ "analysis": { "qodanaProblems": 2147483648 } }"""));
        Assert.Null(BuildInfo.Parse("""{ "analysis": { "qodanaProblems": 1e300 } }"""));
    }

    /// <summary>A fractional count is rounded to the nearest whole, a half to the even neighbour.</summary>
    [Theory]
    [InlineData("0.4", 0)]
    [InlineData("0.5", 0)]
    [InlineData("1.5", 2)]
    [InlineData("2.5", 2)]
    [InlineData("2.6", 3)]
    [InlineData("3e1", 30)]
    public void AFractionalCountOfProblemsIsRounded(string said, int expected) =>
        Assert.Equal(expected, BuildInfo.Parse($$"""{ "analysis": { "qodanaProblems": {{said}} } }""")!.QodanaProblems);

    /// <summary>The lines of code run to half the largest <see cref="long" />: at that end they are kept, beyond it
    /// they are no number, and the size falls back to what its languages add up to.</summary>
    [Fact]
    public void TheLinesOfCodeEndWhereTheReaderStops()
    {
        var huge = BuildInfo.Parse("""{ "code": { "linesOfCode": 1e30, "languages": [ { "name": "C#", "lines": 7 } ] } }""")!.Code!;

        Assert.Equal(7L, huge.LinesOfCode);

        var large = BuildInfo.Parse("""{ "code": { "linesOfCode": 1e15 } }""")!.Code!;

        Assert.Equal(1_000_000_000_000_000L, large.LinesOfCode);
    }

    /// <summary>The moment keeps the offset the answer says; a moment said without one is taken as universal time.</summary>
    [Fact]
    public void TheMomentKeepsItsOffsetAndAMomentWithoutOneIsUniversal()
    {
        var offset = BuildInfo.Parse("""{ "builtAt": "2026-10-04T15:00:00-05:00" }""")!.BuiltAt!.Value;

        Assert.Equal(TimeSpan.FromHours(-5), offset.Offset);
        Assert.Equal(new DateTimeOffset(2026, 10, 4, 20, 0, 0, TimeSpan.Zero), offset);

        var bare = BuildInfo.Parse("""{ "builtAt": "2026-10-04T20:00:00" }""")!.BuiltAt!.Value;

        Assert.Equal(TimeSpan.Zero, bare.Offset);
        Assert.Equal(new DateTimeOffset(2026, 10, 4, 20, 0, 0, TimeSpan.Zero), bare);
    }

    /// <summary>Escapes in the words are read as the letters they stand for, and the words are trimmed after.</summary>
    [Fact]
    public void EscapedWordsAreReadAsTheirLetters()
    {
        var build = BuildInfo.Parse("""{ "version": "2.4.15", "commit": "\t0a1b2c3d\n" }""")!;

        Assert.Equal("2.4.15", build.Version);
        Assert.Equal("0a1b2c3d", build.Commit);
    }

    /// <summary>The commit's page is kept beside another fact even though it does not make a build alone; an
    /// address of another scheme is not kept.</summary>
    [Fact]
    public void TheCommitsPageIsKeptBesideAnotherFactButOnlyOverHttp()
    {
        var kept = BuildInfo.Parse("""{ "commit": "0a1b2c3d", "commitUrl": "https://github.example.net/o/r/commit/0a1b2c3d" }""")!;

        Assert.Equal(new Uri("https://github.example.net/o/r/commit/0a1b2c3d"), kept.CommitUrl);

        var dropped = BuildInfo.Parse("""{ "commit": "0a1b2c3d", "commitUrl": "ftp://github.example.net/o/r/commit/0a1b2c3d" }""")!;

        Assert.Null(dropped.CommitUrl);
        Assert.Equal("0a1b2c3d", dropped.Commit);
    }
}
