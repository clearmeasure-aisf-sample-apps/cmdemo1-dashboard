namespace Dashboard.Tests;

/// <summary>
/// <see cref="BuildInfo" /> is what a deployable says about the build it runs at its build endpoint: the version and
/// the commit, the two addresses that lead to them, the moment the build ran, and the six facts the build measured
/// about its code - its size, its tests, its coverage, its complexity, its CRAP score and the problems Qodana left.
/// <see cref="CodeSize" /> is the first of those seven and the type <see cref="BuildInfo.Code" /> stands on, and it is
/// the one record of the file the other edge-case files leave open.
/// <para>
/// What the shipped fixtures and the other files already pin is left alone. <see cref="BuildInfoTests" /> pins the
/// eleven parts read from the sample answer and from a build that measures only part of itself, every section left
/// out or said as the wrong kind, an answer that is not JSON, and the <c>code</c> and <c>tests</c> and
/// <c>coverage</c> and <c>crap</c> sections of an answer that holds nothing else;
/// <see cref="DeployableFactsTests" /> and <see cref="DashboardBuildTests" /> pin the same read on a deployable's
/// card and on the dashboard's own. The readers of the
/// answer's parts - text trimmed and empty made nothing, an address absolute http or https only, a moment at the
/// offset it says and no other time at all, a count rounded and a number not negative - are
/// <see cref="JsonReadTests" />'s, and <c>version</c> losing its build metadata is <see cref="VersionTextTests" />'s;
/// the records a build is made of hold their own parts and say their own words, and each of those is pinned by
/// its file (<see cref="TestCountsEdgeCaseTests" />, <see cref="CoverageFactsEdgeCaseTests" />,
/// <see cref="ComplexityFactsEdgeCaseTests" />, <see cref="CrapFactsEdgeCaseTests" />,
/// <see cref="LanguageSizeEdgeCaseTests" /> and <see cref="LanguageShareEdgeCaseTests" />), as is the card's
/// vocabulary (<see cref="BuildTextEdgeCaseTests" />).
/// </para>
/// <para>
/// What is pinned here is the build itself and the size within it, and only that: the eleven parts a build is built
/// with, each landing on the property of its own name and standing absent when the caller leaves it out, the words
/// and the numbers and the addresses kept as they are given - the record guarding nothing the reader guards; the
/// answer's edges that belong to the build rather than to a part of it, which roots of JSON are no answer, that its
/// names are the exact ones, that a fact in the wrong place is a fact unsaid, which one of the ten facts alone makes
/// a build at all and - the edge the answer's own words sit next to - that the commit's page alone does not; the
/// equality, the hash, the words, the falling apart and the copy of a build, including that a build holds its size
/// and the size holds its languages by the instance they stand on, so the same answer read twice is not one build;
/// and the size's own three parts, at the ends they run to, with the empty list of languages and the list that is
/// absent, and what an answer has to say for a section before it is a size at all.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: an address is named and never fetched, and
/// every moment is one the test brings or one the fixed answer says.
/// </para>
/// </summary>
public class BuildInfoEdgeCaseTests
{
    private static readonly Uri CommitPage = new("https://github.example.net/o/r/commit/0a1b2c3d4e5f60718293a4b5c6d7e8f901234567");
    private static readonly Uri Run = new("https://github.example.net/o/r/actions/runs/1");
    private static readonly DateTimeOffset Built = new(2026, 10, 4, 20, 0, 0, TimeSpan.Zero);

    private static readonly CodeSize Size = new(84_210, 1_203, [new LanguageSize("C#", 61_234, 800)]);
    private static readonly TestCounts Counts = new(1_009, 240, 168);
    private static readonly CoverageFacts Coverage = new(81.2, 70.1);
    private static readonly ComplexityFacts Complexity = new(1.9, 34, 5_210);
    private static readonly CrapFacts Crap = new(28.5, 30, 0);

    /// <summary>The eleven labels, in the order the record declares its parts.</summary>
    private static readonly string[] Labels =
    [
        "Version", "Commit", "CommitUrl", "BuiltAt", "BuildUrl", "Code", "Tests", "Coverage", "Complexity", "Crap", "QodanaProblems",
    ];

    /// <summary>A build with every part named and every part a value apart from the others, so that an argument
    /// landing on another property is seen rather than quietly matching a second part of the record.</summary>
    private static BuildInfo Full => new(
        "2.4.15", "0a1b2c3d4e5f60718293a4b5c6d7e8f901234567", CommitPage, Built, Run, Size, Counts, Coverage, Complexity, Crap, 0);

    /// <summary>A build named by nothing at all: the eleven parts as they stand when nobody names them.</summary>
    private static BuildInfo None => new(null, null, null, null, null, null, null, null, null, null, null);

    /// <summary>The build's eleven parts, in the order the record declares them.</summary>
    private static object?[] Parts(BuildInfo build) =>
        [build.Version, build.Commit, build.CommitUrl, build.BuiltAt, build.BuildUrl, build.Code, build.Tests, build.Coverage, build.Complexity, build.Crap, build.QodanaProblems];

    // ------------------------------------------------------------------ the eleven parts a build is built with

    /// <summary>The constructor's eleven parts, position by position, each back as the property of its own name -
    /// the words as the words they are, the addresses, the sections and the count as the very instances they stand
    /// on - and in the order the record declares them.</summary>
    [Fact]
    public void TheConstructorHandsEachOfTheElevenPartsToThePropertyOfItsName()
    {
        var build = Full;

        Assert.Equal("2.4.15", build.Version);
        Assert.Equal("0a1b2c3d4e5f60718293a4b5c6d7e8f901234567", build.Commit);
        Assert.Same(CommitPage, build.CommitUrl);
        Assert.Equal(Built, build.BuiltAt);
        Assert.Same(Run, build.BuildUrl);
        Assert.Same(Size, build.Code);
        Assert.Same(Counts, build.Tests);
        Assert.Same(Coverage, build.Coverage);
        Assert.Same(Complexity, build.Complexity);
        Assert.Same(Crap, build.Crap);
        Assert.Equal(0, build.QodanaProblems);
    }

    /// <summary>A build named by nothing is still a build: the eleven parts stand the nothing they were given,
    /// kept rather than made a zero or an empty word - the count of problems the nothing and not none of them,
    /// the moment the nothing and not now - and the record's own default is no build at all, which is what an
    /// answer that says nothing comes to.</summary>
    [Fact]
    public void ABuildNamedByNothingHoldsTheNothingOfEveryPartAndIsNotNoBuild()
    {
        var build = None;

        Assert.NotNull(build);
        Assert.All(Parts(build), Assert.Null);
        Assert.Equal(11, Parts(build).Length);
        Assert.Null(default(BuildInfo));

        Assert.NotNull(BuildInfo.Parse("""{ "version": "1.0.0" }"""));
        Assert.Null(BuildInfo.Parse("{ }"));
    }

    /// <summary>The build keeps the words it is given and changes none of them: the record is not the reader of an
    /// answer, so the spaces about a version stay about it, a version that is nothing but its build metadata is a
    /// version, the empty word is a word and not the nothing it is not, and only the nothing the caller hands over
    /// stands as nothing.</summary>
    [Fact]
    public void ABuildKeepsItsWordsAsTheyAreGivenThem()
    {
        var build = new BuildInfo("  2.4.15+0a1b2c3  ", "+0a1b2c3", null, null, null, null, null, null, null, null, null);

        Assert.Equal("  2.4.15+0a1b2c3  ", build.Version);
        Assert.Equal("+0a1b2c3", build.Commit);

        var emptied = new BuildInfo(string.Empty, string.Empty, null, null, null, null, null, null, null, null, null);

        Assert.Equal(string.Empty, emptied.Version);
        Assert.Equal(string.Empty, emptied.Commit);
        Assert.NotEqual(emptied, None);

        var oneSided = new BuildInfo(null, null, CommitPage, null, null, null, null, null, null, null, null);

        Assert.Null(oneSided.Version);
        Assert.Null(oneSided.Commit);
        Assert.Same(CommitPage, oneSided.CommitUrl);
    }

    /// <summary>The count of problems stands where it is put, at the two ends of the number it runs to and at the
    /// zero that is a number and the negative a build endpoint would never report: the record is not the reader,
    /// and keeps the count it is given, the sign and all.</summary>
    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void TheCountOfProblemsStandsWhereItIsPut(int? problems)
    {
        var build = None with { QodanaProblems = problems };

        Assert.Equal(problems, build.QodanaProblems);
        Assert.Equal(None with { QodanaProblems = problems }, build);
    }

    // ------------------------------------------------------------------ the answer, at its edges

    /// <summary>An answer that is no object of facts is no build, however it comes: nothing at all, the empty body
    /// and the body of nothing but spaces, a root that is a number or a word or a flag or the nothing, an array of
    /// the very facts a build would say, and JSON that does not hold together at all - a brace left open, a comma
    /// left hanging, a second document, a word after the object, a byte-order mark before it.</summary>
    [Theory]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    [InlineData("5")]
    [InlineData("2.4")]
    [InlineData("\"2.4.15\"")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("null")]
    [InlineData("[ \"2.4.15\", \"0a1b2c3d\" ]")]
    [InlineData("""[ { "version": "2.4.15" } ]""")]
    [InlineData("{oops")]
    [InlineData("""{ "version": "2.4.15", }""")]
    [InlineData("""{ "version": "2.4.15" } { "commit": "0a1b2c3d" }""")]
    [InlineData("""{ "version": "2.4.15" } trailing""")]
    [InlineData("\uFEFF{ \"version\": \"2.4.15\" }")]
    public void AnAnswerThatIsNoObjectOfFactsIsNoBuild(string answer) => Assert.Null(BuildInfo.Parse(answer));

    /// <summary>The names an answer uses are the names the build reads, letter for letter: the answer that capitals
    /// a name says nothing under it, and so is no build at all, while the same facts under the names the build asks
    /// are a build of them. (The empty answer and the answer of another shape are the test above; an error page is
    /// <see cref="BuildInfoTests" />'s.)</summary>
    [Fact]
    public void AnAnswerNamesItsPartsExactlyOrSaysNothing()
    {
        Assert.Null(BuildInfo.Parse("""{ "Version": "2.4.15" }"""));
        Assert.Null(BuildInfo.Parse("""{ "COMMIT": "0a1b2c3d" }"""));
        Assert.Null(BuildInfo.Parse("""{ "BuiltAt": "2026-10-04T20:00:00Z" }"""));
        Assert.Null(BuildInfo.Parse("""{ "Code": { "Files": 1203 } }"""));
        Assert.Null(BuildInfo.Parse("""{ "analysis": { "QodanaProblems": 3 } }"""));

        var build = BuildInfo.Parse("""{ "version": "2.4.15", "code": { "files": 1203 }, "analysis": { "qodanaProblems": 3 } }""")!;

        Assert.Equal("2.4.15", build.Version);
        Assert.Equal(1203, build.Code!.Files);
        Assert.Equal(3, build.QodanaProblems);
    }

    /// <summary>A fact said in the wrong place is a fact unsaid: the count of problems belongs under
    /// <c>analysis</c> and the lines under <c>code</c> and the tests under <c>tests</c>, and an answer that puts one
    /// of them beside the whole build, or into another build's section, holds no build at all - the reader does not
    /// go looking.</summary>
    [Fact]
    public void AFactSaidInTheWrongPlaceIsNotSaid()
    {
        Assert.Null(BuildInfo.Parse("""{ "qodanaProblems": 3 }"""));
        Assert.Null(BuildInfo.Parse("""{ "analysis": 3 }"""));
        Assert.Null(BuildInfo.Parse("""{ "linesOfCode": 84210, "files": 1203 }"""));
        Assert.Null(BuildInfo.Parse("""{ "code": { "unit": 1009 } }"""));
        Assert.Null(BuildInfo.Parse("""{ "tests": { "qodanaProblems": 3, "linePercent": 81.2 } }"""));

        var misplaced = BuildInfo.Parse("""{ "version": "2.4.15", "code": { "languages": { "name": "C#", "lines": 61234 } } }""")!;

        Assert.Equal("2.4.15", misplaced.Version);
        Assert.Null(misplaced.Code);
    }

    /// <summary>Any one of the ten facts the build asks about is a build on its own - and a build of that fact
    /// alone, the other nine parts standing absent - the two words, the moment, the run's address, and each of the
    /// measures said as the zero that is a number and not as the nothing it is not.</summary>
    [Fact]
    public void EachFactOfTheAnswerLandsOnItsOwnPropertyAndOnNoOther()
    {
        Solo("""{ "version": "2.4.15" }""", build => build.Version, "2.4.15");
        Solo("""{ "commit": "0a1b2c3d" }""", build => build.Commit, "0a1b2c3d");
        Solo("""{ "builtAt": "2026-10-04T20:00:00Z" }""", build => build.BuiltAt, Built);
        Solo("""{ "buildUrl": "https://github.example.net/o/r/actions/runs/1" }""", build => build.BuildUrl, Run);
        Solo("""{ "code": { "files": 0 } }""", build => build.Code!.Files, 0);
        Solo("""{ "tests": { "unit": 0 } }""", build => build.Tests, new TestCounts(0, null, null));
        Solo("""{ "coverage": { "linePercent": 0 } }""", build => build.Coverage, new CoverageFacts(0, null));
        Solo("""{ "complexity": { "average": 0 } }""", build => build.Complexity, new ComplexityFacts(0, null, null));
        Solo("""{ "crap": { "max": 0 } }""", build => build.Crap, new CrapFacts(0, null, null));
        Solo("""{ "analysis": { "qodanaProblems": 0 } }""", build => build.QodanaProblems, 0);
    }

    /// <summary>The one part of the answer the build does not ask about when it asks whether anything is there: a
    /// commit and its page are two parts, and the page alone - however absolute, however good a link - says no
    /// build, where the run's address alone does. The rest are the parts that say nothing when they are said at
    /// all: a word of spaces, a version that is nothing but the metadata the page drops, a moment that is no
    /// moment, a count below the least a build can report - an answer of nothing but one of them is the answer
    /// that says nothing.</summary>
    [Theory]
    [InlineData("""{ "commitUrl": "https://github.example.net/o/r/commit/0a1b2c3d" }""")]
    [InlineData("""{ "commitUrl": "http://github.example.net/o/r/commit/0a1b2c3d" }""")]
    [InlineData("""{ "commitUrl": "not an address" }""")]
    [InlineData("""{ "version": "" }""")]
    [InlineData("""{ "version": "   " }""")]
    [InlineData("""{ "version": "+0a1b2c3" }""")]
    [InlineData("""{ "commit": "   " }""")]
    [InlineData("""{ "builtAt": "last tuesday" }""")]
    [InlineData("""{ "builtAt": 20261004 }""")]
    [InlineData("""{ "analysis": { "qodanaProblems": -1 } }""")]
    [InlineData("""{ "analysis": { "qodanaProblems": null } }""")]
    [InlineData("""{ "crap": { "max": -1 } }""")]
    public void AnAnswerThatSaysNothingAtAllIsNoBuild(string answer) => Assert.Null(BuildInfo.Parse(answer));

    /// <summary>A build read from an answer is the build the same eleven words and numbers stand for, the eleven
    /// parts each landed on its own property; and the one part the read cannot be handed over as the very same
    /// record is the size, which comes with a language list of its own - the build holds its size, the size holds
    /// its languages, and both by the instance they stand on.</summary>
    [Fact]
    public void ABuildReadIsTheBuildTheSameWordsStandForButHoldsItsOwnSize()
    {
        var read = BuildInfo.Parse(Optics.Build)!;
        var built = Full;

        Assert.NotEqual(built, read);
        Assert.Equal(built with { Code = read.Code }, read);
        Assert.Equal(read.GetHashCode(), (built with { Code = read.Code }).GetHashCode());

        Assert.Equal(built.Version, read.Version);
        Assert.Equal(built.Commit, read.Commit);
        Assert.Equal(built.CommitUrl, read.CommitUrl);
        Assert.Equal(built.BuiltAt, read.BuiltAt);
        Assert.Equal(built.BuildUrl, read.BuildUrl);
        Assert.Equal(built.Code!.LinesOfCode, read.Code!.LinesOfCode);
        Assert.Equal(built.Code.Files, read.Code.Files);
        Assert.Equal(built.Tests, read.Tests);
        Assert.Equal(built.Coverage, read.Coverage);
        Assert.Equal(built.Complexity, read.Complexity);
        Assert.Equal(built.Crap, read.Crap);
        Assert.Equal(built.QodanaProblems, read.QodanaProblems);
    }

    // ------------------------------------------------------------------ the build's own words, parts and copies

    /// <summary>The words a build says its eleven parts in: its own name at the front, the parts in the order the
    /// record declares them, each under its own label, the address said as the absolute address it is, the section
    /// that is not there said as the empty word - and the build of nothing said with eleven empty words, which is
    /// still words and not nothing.</summary>
    [Fact]
    public void ABuildSaysItsPartsInTheOrderTheRecordDeclaresThem()
    {
        Assert.Equal(
            """BuildInfo { Version = 2.4.15, Commit = 0a1b2c3d, CommitUrl = https://github.example.net/o/r/commit/0a1b2c3d4e5f60718293a4b5c6d7e8f901234567, BuiltAt = , BuildUrl = https://github.example.net/o/r/actions/runs/1, Code = , Tests = , Coverage = , Complexity = , Crap = , QodanaProblems =  }""",
            new BuildInfo("2.4.15", "0a1b2c3d", CommitPage, null, Run, null, null, null, null, null, null).ToString());

        Assert.Equal(
            "BuildInfo { Version = , Commit = , CommitUrl = , BuiltAt = , BuildUrl = , Code = , Tests = , Coverage = , Complexity = , Crap = , QodanaProblems =  }",
            None.ToString());

        Assert.True(PartsInOrder(Full));
        Assert.True(PartsInOrder(None));
    }

    /// <summary>The build falls apart into the eleven parts it holds, in the order it declares them and each as the
    /// value it stands on - the words, the two addresses, the moment, the five sections, and the count last.</summary>
    [Fact]
    public void ABuildFallsApartIntoItsElevenParts()
    {
        var (version, commit, commitUrl, builtAt, buildUrl, code, tests, coverage, complexity, crap, problems) = Full;

        Assert.Equal("2.4.15", version);
        Assert.Equal("0a1b2c3d4e5f60718293a4b5c6d7e8f901234567", commit);
        Assert.Same(CommitPage, commitUrl);
        Assert.Equal(Built, builtAt);
        Assert.Same(Run, buildUrl);
        Assert.Same(Size, code);
        Assert.Same(Counts, tests);
        Assert.Same(Coverage, coverage);
        Assert.Same(Complexity, complexity);
        Assert.Same(Crap, crap);
        Assert.Equal(0, problems);

        var (noVersion, noCommit, noCommitUrl, noBuiltAt, noBuildUrl, noCode, noTests, noCoverage, noComplexity, noCrap, noProblems) = None;

        Assert.Null(noVersion);
        Assert.Null(noCommit);
        Assert.Null(noCommitUrl);
        Assert.Null(noBuiltAt);
        Assert.Null(noBuildUrl);
        Assert.Null(noCode);
        Assert.Null(noTests);
        Assert.Null(noCoverage);
        Assert.Null(noComplexity);
        Assert.Null(noCrap);
        Assert.Null(noProblems);
    }

    /// <summary>The copy that turns nothing is the build again though not the build itself - the size and its
    /// languages carried over as the one instance they stand on - and the copy that turns one of the eleven parts
    /// keeps the ten it does not name, the part turned to another value and to the nothing the value is not.</summary>
    [Fact]
    public void ACopyThatTurnsOnePartKeepsTheOtherTen()
    {
        var build = Full;

        var copy = build with { };
        Assert.Equal(build, copy);
        Assert.NotSame(build, copy);
        Assert.Same(build.Code, copy.Code);

        var retagged = build with { Version = "2.4.16" };
        Assert.Equal("2.4.16", retagged.Version);
        Assert.Equal(build.Commit, retagged.Commit);
        Assert.Same(build.Code, retagged.Code);
        Assert.NotEqual(build, retagged);

        var unpinned = build with { CommitUrl = null };
        Assert.Null(unpinned.CommitUrl);
        Assert.Equal(build.Commit, unpinned.Commit);

        var unmeasured = build with { Code = null, Tests = null, Coverage = null, Complexity = null, Crap = null };
        Assert.Null(unmeasured.Code);
        Assert.Null(unmeasured.Tests);
        Assert.Null(unmeasured.Coverage);
        Assert.Null(unmeasured.Complexity);
        Assert.Null(unmeasured.Crap);
        Assert.Equal("2.4.15", unmeasured.Version);
        Assert.Equal(0, unmeasured.QodanaProblems);

        var emptied = None with { QodanaProblems = 0 };
        Assert.NotEqual(None, emptied);
    }

    /// <summary>Two builds are one while their eleven parts are - the words, the addresses and the moment each by
    /// their own equality, the sections by theirs, and the size by the list it holds - and a difference in any one
    /// part, a part turned or left unsaid, is another build. The hash follows the equality and stands from call to
    /// call, and no build is one with a word or a number.</summary>
    [Fact]
    public void TwoBuildsAreOneWhileTheirElevenPartsAre()
    {
        var one = Full;
        var two = Full;

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.True(one.Equals((object)two));
        Assert.True(one.Equals(one));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.NotSame(one, two);
        Assert.False(one.Equals(null));
        Assert.False(one.Equals("2.4.15"));
        Assert.False(one.Equals((object)0));

        Assert.NotEqual(one, one with { Version = null });
        Assert.NotEqual(one, one with { Commit = "0a1b2c3" });
        Assert.NotEqual(one, one with { CommitUrl = Run });
        Assert.NotEqual(one, one with { BuiltAt = Built.AddSeconds(1) });
        Assert.NotEqual(one, one with { BuildUrl = null });
        Assert.NotEqual(one, one with { Code = null });
        Assert.NotEqual(one, one with { Tests = new TestCounts(1_009, 240, 169) });
        Assert.NotEqual(one, one with { Coverage = new CoverageFacts(81.2, null) });
        Assert.NotEqual(one, one with { Complexity = new ComplexityFacts(1.9, 34, 5_211) });
        Assert.NotEqual(one, one with { Crap = new CrapFacts(28.5, 30, 1) });
        Assert.NotEqual(one, one with { QodanaProblems = null });
        Assert.NotEqual(one, one with { QodanaProblems = 1 });

        Assert.Single(new HashSet<BuildInfo> { one, two, Full });
        Assert.Equal(2, new HashSet<BuildInfo> { one, one with { QodanaProblems = 3 } }.Count);
    }

    /// <summary>The nothing on either side of the operators: two builds that are no build at all are one, a build
    /// and no build are two however the question is asked, and the build of nothing that a caller names is not the
    /// no build an answer leaves the page with.</summary>
    [Fact]
    public void NoBuildIsOneWithNoBuildAndNotWithABuild()
    {
        BuildInfo? nothing = null;
        BuildInfo? alsoNothing = null;
        var one = Full;

        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.True(nothing != one);
        Assert.False(nothing == one);
        Assert.False(one == nothing);
        Assert.True(one != nothing);
        Assert.False(one.Equals(null));
        Assert.False(one.Equals((object)Run));

        var named = None;
        Assert.True(named != nothing);
        Assert.False(named == nothing);
    }

    /// <summary>The same answer read twice is two builds: each read brings its own size with its own list of
    /// languages, and a build is one with another only while its size is the size the other holds - so the two
    /// reads, equal in every word and number and list content, are not one build and do not hash as one. An answer
    /// that says no size, and so holds no list, reads to one build twice over.</summary>
    [Fact]
    public void TheSameAnswerReadTwiceIsTwoBuilds()
    {
        var one = BuildInfo.Parse(Optics.Build)!;
        var two = BuildInfo.Parse(Optics.Build)!;

        Assert.NotSame(one, two);
        Assert.NotEqual(one, two);
        Assert.False(one == two);
        Assert.True(one != two);
        Assert.NotSame(one.Code, two.Code);
        Assert.Equal(one.Code!.Languages, two.Code!.Languages);

        var worded = BuildInfo.Parse("""{ "version": "2.4.15", "commit": "0a1b2c3d" }""")!;
        var againWorded = BuildInfo.Parse("""{ "version": "2.4.15", "commit": "0a1b2c3d" }""")!;

        Assert.Equal(worded, againWorded);
        Assert.Equal(worded.GetHashCode(), againWorded.GetHashCode());
    }

    // ------------------------------------------------------------------ the size: the record a build's Code stands on

    /// <summary>The size keeps the three parts it is given: the two numbers as the values they stand on and the
    /// languages as the very list it was handed - never copied, never sorted again, never emptied of what the
    /// caller put in it.</summary>
    [Fact]
    public void ASizeKeepsTheThreePartsItIsGiven()
    {
        IReadOnlyList<LanguageSize> languages = [new LanguageSize("C#", 61_234, 800), new LanguageSize("Razor", 9_000, 120)];
        var size = new CodeSize(84_210, 1_203, languages);

        Assert.Equal(84_210L, size.LinesOfCode);
        Assert.Equal(1_203, size.Files);
        Assert.Same(languages, size.Languages);
        Assert.Equal(["C#", "Razor"], size.Languages.Select(language => language.Name));

        var nothingSaid = new CodeSize(null, null, languages);

        Assert.Null(nothingSaid.LinesOfCode);
        Assert.Null(nothingSaid.Files);
        Assert.Equal(2, nothingSaid.Languages.Count);
    }

    /// <summary>The two numbers of a size stand where they are put, at the ends of the number each runs to and at
    /// the zero that is a number: the most lines a repository can report and the least, one file and none, and the
    /// negative neither of them would ever say - the record is not the reader of an answer and keeps what it is
    /// given.</summary>
    [Theory]
    [InlineData(long.MinValue, int.MinValue)]
    [InlineData(-84_210L, -1_203)]
    [InlineData(0L, 0)]
    [InlineData(1L, 1)]
    [InlineData(long.MaxValue, int.MaxValue)]
    public void TheTwoNumbersOfASizeStandAtTheEndsTheyRunTo(long? lines, int? files)
    {
        var size = new CodeSize(lines, files, []);

        Assert.Equal(lines, size.LinesOfCode);
        Assert.Equal(files, size.Files);
        Assert.Empty(size.Languages);
    }

    /// <summary>The empty list of languages and the list that is not there at all are two things: a size of no
    /// languages still holds the empty list it was given, and the size whose list nobody named holds the nothing it
    /// was given rather than an empty list of its own - the two sizes are not one size.</summary>
    [Fact]
    public void AnEmptyListOfLanguagesIsNotTheListThatIsAbsent()
    {
        var emptied = new CodeSize(1, 1, []);
        var absent = new CodeSize(1, 1, null!);

        Assert.Empty(emptied.Languages);
        Assert.Null(absent.Languages);
        Assert.NotEqual(emptied, absent);
        Assert.NotEqual(absent, emptied);
    }

    /// <summary>The size falls apart into its three parts and a copy of it is the size again: the copy that turns
    /// nothing carries the one list it was built with, and the copies that turn the lines, the files, the list and
    /// the nothing of them keep the rest as they were given.</summary>
    [Fact]
    public void ASizeFallsApartIntoItsThreePartsAndACopyIsTheSizeAgain()
    {
        var (lines, files, languages) = Size;

        Assert.Equal(84_210L, lines);
        Assert.Equal(1_203, files);
        Assert.Same(Size.Languages, languages);

        var (noLines, noFiles, noNames) = new CodeSize(null, null, []);

        Assert.Null(noLines);
        Assert.Null(noFiles);
        Assert.Empty(noNames);

        var copy = Size with { };
        Assert.Equal(Size, copy);
        Assert.NotSame(Size, copy);
        Assert.Same(Size.Languages, copy.Languages);

        Assert.Equal(90_000L, (Size with { LinesOfCode = 90_000 }).LinesOfCode);
        Assert.Equal(1_203, (Size with { LinesOfCode = 90_000 }).Files);
        Assert.Null((Size with { Files = null }).Files);
        Assert.Equal(84_210L, (Size with { Files = null }).LinesOfCode);

        var uncounted = new CodeSize(null, null, Size.Languages);
        Assert.Equal(Size.Languages, uncounted.Languages);
        Assert.NotEqual(Size, uncounted);
    }

    /// <summary>The size says its parts in the order the record declares them, each under its own label and the two
    /// numbers as the plain numbers they are, with no mark between the digits and the sign they carry; a number
    /// that is not there is said as the empty word.</summary>
    [Fact]
    public void ASizeSaysItsPartsInTheOrderTheRecordDeclaresThem()
    {
        Assert.StartsWith("CodeSize { LinesOfCode = 84210, Files = 1203, Languages = ", Size.ToString(), StringComparison.Ordinal);
        Assert.EndsWith(" }", Size.ToString(), StringComparison.Ordinal);
        Assert.StartsWith("CodeSize { LinesOfCode = , Files = , Languages = ", new CodeSize(null, null, []).ToString(), StringComparison.Ordinal);
        Assert.StartsWith(
            "CodeSize { LinesOfCode = -9223372036854775808, Files = -2147483648, Languages = ",
            new CodeSize(long.MinValue, int.MinValue, []).ToString(),
            StringComparison.Ordinal);
        Assert.StartsWith(
            "CodeSize { LinesOfCode = 9223372036854775807, Files = 2147483647, Languages = ",
            new CodeSize(long.MaxValue, int.MaxValue, []).ToString(),
            StringComparison.Ordinal);
    }

    /// <summary>Two sizes are one while their three parts are - and their list is the one list: contents that
    /// match do not make two lists one size, while the list handed over does. The hash follows, each part turned
    /// makes another size, and the record's own default is no size at all.</summary>
    [Fact]
    public void TwoSizesAreOneWhileTheirPartsAndTheirListAre()
    {
        var one = Size;
        var two = new CodeSize(84_210, 1_203, one.Languages);

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.False(one != two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.NotSame(one, two);

        var other = new CodeSize(84_210, 1_203, [new LanguageSize("C#", 61_234, 800)]);

        Assert.Equal(one.Languages, other.Languages);
        Assert.NotEqual(one, other);
        Assert.True(one != other);

        Assert.NotEqual(one, new CodeSize(84_211, 1_203, one.Languages));
        Assert.NotEqual(one, new CodeSize(null, 1_203, one.Languages));
        Assert.NotEqual(one, new CodeSize(84_210, 1_202, one.Languages));
        Assert.NotEqual(one, new CodeSize(84_210, null, one.Languages));
        Assert.NotEqual(one, one with { Languages = [new LanguageSize("C#", 61_234, 800)] });
        Assert.NotEqual(one, one with { Languages = [new LanguageSize("Razor", 9_000, 120)] });

        Assert.Null(default(CodeSize));
    }

    // ------------------------------------------------------------------ what an answer makes a size

    /// <summary>The section that says none of its three numbers is no size, and an answer of nothing but that
    /// section is no build: the empty object, the object that names only parts of its own it does not have, the
    /// list of languages with nothing in it and the list said as another kind, the lines said as a word or as a
    /// negative, the language with no name or with no lines or with lines that are not a number - each of them
    /// nothing said.</summary>
    [Theory]
    [InlineData("""{ "code": { } }""")]
    [InlineData("""{ "code": { "names": "C#" } }""")]
    [InlineData("""{ "code": { "languages": [ ] } }""")]
    [InlineData("""{ "code": { "languages": null } }""")]
    [InlineData("""{ "code": { "languages": { "name": "C#", "lines": 61234 } } }""")]
    [InlineData("""{ "code": { "languages": [ "C#" ] } }""")]
    [InlineData("""{ "code": { "languages": [ { "name": "   ", "lines": 61234 } ] } }""")]
    [InlineData("""{ "code": { "languages": [ { "lines": 61234 } ] } }""")]
    [InlineData("""{ "code": { "languages": [ { "name": "C#", "lines": 0 } ] } }""")]
    [InlineData("""{ "code": { "languages": [ { "name": "C#", "lines": -5 } ] } }""")]
    [InlineData("""{ "code": { "languages": [ { "name": "C#", "lines": "61234" } ] } }""")]
    [InlineData("""{ "code": { "linesOfCode": "84210" } }""")]
    [InlineData("""{ "code": { "linesOfCode": -1 } }""")]
    [InlineData("""{ "code": { "files": -1 } }""")]
    [InlineData("""{ "code": { "files": [ 1203 ] } }""")]
    public void ASectionThatSaysNoneOfItsThreeNumbersIsNoSize(string answer) => Assert.Null(BuildInfo.Parse(answer));

    /// <summary>Any one of the three numbers makes a size, and the zero is a number: no lines and no languages but
    /// the file's own count of files is a size of none of them; the lines the file says win over the lines the
    /// languages add up to, even for the zero the file says and the languages that say otherwise; the files a
    /// language does not name are the nothing the size keeps and not zero; and the languages are said largest
    /// first, two of one size staying in the order the file named them and a name said twice staying twice.</summary>
    [Fact]
    public void ASizeIsMadeByAnyOneOfItsThreeNumbers()
    {
        var filesOnly = BuildInfo.Parse("""{ "code": { "files": 0 } }""")!.Code!;

        Assert.Null(filesOnly.LinesOfCode);
        Assert.Equal(0, filesOnly.Files);
        Assert.Empty(filesOnly.Languages);

        var counted = BuildInfo.Parse("""{ "code": { "linesOfCode": 0, "languages": [ { "name": "C#", "lines": 5 } ] } }""")!.Code!;

        Assert.Equal(0L, counted.LinesOfCode);
        Assert.Single(counted.Languages);

        var ordered = BuildInfo.Parse(
            """{ "code": { "languages": [ { "name": "B", "lines": 10 }, { "name": "A", "lines": 10 }, { "name": "C", "lines": 12 }, { "name": "B", "lines": 9 } ] } }""")!.Code!;

        Assert.Equal(["C", "B", "A", "B"], ordered.Languages.Select(language => language.Name));
        Assert.Equal(41L, ordered.LinesOfCode);
        Assert.Null(ordered.Files);

        var unfiled = BuildInfo.Parse("""{ "code": { "languages": [ { "name": "C#", "lines": 10, "files": { "count": 3 } }, { "name": "SQL", "lines": 4, "files": 0 } ] } }""")!.Code!;

        Assert.Null(unfiled.Languages[0].Files);
        Assert.Equal(0, unfiled.Languages[1].Files);
        Assert.Equal(14L, unfiled.LinesOfCode);
    }

    // ------------------------------------------------------------------ helpers

    /// <summary>The answer says the one fact named here, and says nothing else: the value comes back as the
    /// property that stands for it - a fact the reader put on another property would leave this one the nothing it
    /// is - and exactly one of the build's eleven parts is there, the other ten standing absent.</summary>
    private static void Solo(string answer, Func<BuildInfo, object?> fact, object? expected)
    {
        var build = BuildInfo.Parse(answer);

        Assert.NotNull(build);
        Assert.Equal(expected, fact(build!));
        Assert.Single(Parts(build!), value => value is not null);
    }

    /// <summary>True when the build's words name its eleven labels one after another, in the order the record
    /// declares them.</summary>
    private static bool PartsInOrder(BuildInfo build)
    {
        var spoken = build.ToString();
        var at = -1;

        foreach (var label in Labels)
        {
            var next = spoken.IndexOf($"{label} = ", at + 1, StringComparison.Ordinal);
            if (next <= at)
            {
                return false;
            }

            at = next;
        }

        return true;
    }
}
