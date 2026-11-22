using System.Reflection;
using Dashboard.Work;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="WorkItemsReport" />, the content of
/// <c>workitems.json</c>, declared in <c>src/Dashboard/Work/WorkItemsReport.cs</c>.
/// <para>
/// What the other files already pin is left alone: <see cref="WorkItemsTests.AnythingButAnObjectIsNoReport" /> covers
/// null, the empty string, a 404 body, an array and text that is no JSON;
/// <see cref="WorkItemsTests.AnEmptyObjectIsAReportWithNothingInIt" /> covers <c>{ }</c>;
/// <see cref="WorkItemsTests.AGeneratedFieldFromAnOlderShapeIsStillTheTime" /> covers the older <c>generated</c>
/// field on its own; and the items, releases, stats and scope parts are pinned in <see cref="WorkItemsTests" />,
/// <see cref="WorkThroughputAndReleaseTests" /> and <see cref="WorkItemsScopeEdgeCaseTests" />.
/// </para>
/// <para>
/// What is pinned here: the file name's exact value; whitespace-only bodies, the other JSON values that are not an
/// object, and JSON the strict reader refuses (trailing commas, comments, a cut-off object, a second value after the
/// object); which of <c>generatedAt</c> and <c>generated</c> wins, and the fallback when the first is not a time; the
/// offset a time is written in; parts of the wrong type standing as absent; the bot and team split left null when it
/// says nothing; and the record's own constructor defaults, <c>with</c> copies and public surface. Nothing here reaches
/// the network or asks the clock what time it is.
/// </para>
/// </summary>
public class WorkItemsReportEdgeCaseTests
{
    private static readonly DateTimeOffset NovemberNinth = new(2026, 11, 9, 22, 0, 0, TimeSpan.Zero);

    // ------------------------------------------------------------------------------------------ FileName

    [Fact]
    public void TheFileNameIsWorkItemsJson()
    {
        Assert.Equal("workitems.json", WorkItemsReport.FileName);
        Assert.Equal(WorkItemsReport.FileName, WorkItemsReport.FileName.Trim());
        Assert.Equal(WorkItemsReport.FileName, WorkItemsReport.FileName.ToLowerInvariant());
    }

    // ------------------------------------------------------------------------- Parse: no report at all

    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    [InlineData(" \t\r\n ")]
    public void AWhitespaceOnlyBodyIsNoReport(string body) => Assert.Null(WorkItemsReport.Parse(body));

    [Theory]
    [InlineData("null")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("0")]
    [InlineData("42")]
    [InlineData("-1.5")]
    [InlineData("\"workitems\"")]
    [InlineData("\"\"")]
    [InlineData("[ { } ]")]
    public void AJsonValueThatIsNotAnObjectIsNoReport(string body) => Assert.Null(WorkItemsReport.Parse(body));

    [Theory]
    [InlineData("{")]
    [InlineData("}")]
    [InlineData("{ \"items\": [ }")]
    [InlineData("{ \"items\": [], }")]
    [InlineData("{ } { }")]
    [InlineData("{ } x")]
    [InlineData("{ /* comment */ }")]
    [InlineData("{ 'items': [] }")]
    [InlineData("{ items: [] }")]
    public void JsonTheStrictReaderRefusesIsNoReport(string body) => Assert.Null(WorkItemsReport.Parse(body));

    // --------------------------------------------------------------------- Parse: an empty-ish object

    [Theory]
    [InlineData("{}")]
    [InlineData("  {  }  ")]
    [InlineData("\r\n{\r\n}\r\n")]
    public void AnObjectSurroundedByWhitespaceIsAReportWithNothingInIt(string body)
    {
        var report = WorkItemsReport.Parse(body);

        Assert.NotNull(report);
        Assert.Null(report.GeneratedAt);
        Assert.Null(report.Scope);
        Assert.Empty(report.Items);
        Assert.Empty(report.Releases);
        Assert.Null(report.BotStats);
        Assert.Null(report.TeamStats);
        Assert.Same(WorkItemsStats.Empty, report.Stats);
        Assert.False(report.Stats.HasAny);
    }

    [Fact]
    public void PartsOfTheWrongTypeStandAsAbsent()
    {
        var report = WorkItemsReport.Parse("""
            { "generatedAt": 20261109, "scope": "cmdemo1", "stats": [ 1, 2 ], "items": { "number": 1 }, "releases": "2.4.90" }
            """);

        Assert.NotNull(report);
        Assert.Null(report.GeneratedAt);
        Assert.Null(report.Scope);
        Assert.Same(WorkItemsStats.Empty, report.Stats);
        Assert.Empty(report.Items);
        Assert.Empty(report.Releases);
        Assert.Null(report.BotStats);
        Assert.Null(report.TeamStats);
    }

    [Fact]
    public void NullPartsStandAsAbsent()
    {
        var report = WorkItemsReport.Parse("""
            { "generatedAt": null, "generated": null, "scope": null, "stats": null, "items": null, "releases": null }
            """);

        Assert.NotNull(report);
        Assert.Null(report.GeneratedAt);
        Assert.Null(report.Scope);
        Assert.Same(WorkItemsStats.Empty, report.Stats);
        Assert.Empty(report.Items);
        Assert.Empty(report.Releases);
    }

    [Fact]
    public void AnEmptyStatsObjectIsAnEmptyCountAndNoSplit()
    {
        var report = WorkItemsReport.Parse("""{ "stats": { "all": { }, "bot": { }, "team": { } } }""")!;

        Assert.NotSame(WorkItemsStats.Empty, report.Stats);
        Assert.False(report.Stats.HasAny);
        Assert.Empty(report.Stats.WipByColumn);
        Assert.Empty(report.Stats.PerWeek);
        Assert.Null(report.BotStats);
        Assert.Null(report.TeamStats);
    }

    [Fact]
    public void ASplitThatSaysSomethingIsKeptAndTheOtherStaysNull()
    {
        var report = WorkItemsReport.Parse("""{ "stats": { "bot": { "wipByColumn": { "Todo": 2 } } } }""")!;

        Assert.False(report.Stats.HasAny);
        Assert.NotNull(report.BotStats);
        Assert.Equal(2, report.BotStats.WipByColumn["Todo"]);
        Assert.Null(report.TeamStats);
    }

    [Fact]
    public void ItemsAndReleasesThatAreNotObjectsAreLeftOutAndTheRestKeepTheFileOrder()
    {
        var report = WorkItemsReport.Parse("""
            { "items": [ 1, null, "x", { "number": 3, "column": "Done" }, [], { "number": 1, "column": "Todo" }, { } ],
              "releases": [ "2.4.1", { "version": "2.4.2", "items": [ 3 ] }, null, { "version": "2.4.1", "items": [ 1 ] } ] }
            """)!;

        Assert.Equal([3, 1], report.Items.Select(item => item.Number));
        Assert.Equal(["Done", "Todo"], report.Items.Select(item => item.Column));
        Assert.Equal(["2.4.2", "2.4.1"], report.Releases.Select(release => release.Version));
    }

    // ------------------------------------------------------------------------------- Parse: the time

    [Fact]
    public void GeneratedAtWinsOverTheOlderGeneratedWhenBothAreGiven()
    {
        var report = WorkItemsReport.Parse("""{ "generatedAt": "2026-11-09T22:00:00Z", "generated": "2020-01-01T00:00:00Z" }""")!;

        Assert.Equal(NovemberNinth, report.GeneratedAt);
    }

    [Theory]
    [InlineData("\"not a time\"")]
    [InlineData("\"\"")]
    [InlineData("\"   \"")]
    [InlineData("12345")]
    [InlineData("{ }")]
    public void AGeneratedAtThatIsNoTimeFallsBackToGenerated(string generatedAt)
    {
        var report = WorkItemsReport.Parse($$"""{ "generatedAt": {{generatedAt}}, "generated": "2026-11-09T22:00:00Z" }""")!;

        Assert.Equal(NovemberNinth, report.GeneratedAt);
    }

    [Fact]
    public void TheOffsetTheTimeIsWrittenInIsKept()
    {
        var time = WorkItemsReport.Parse("""{ "generatedAt": "2026-11-09T16:00:00-06:00" }""")!.GeneratedAt;

        Assert.Equal(TimeSpan.FromHours(-6), time!.Value.Offset);
        Assert.Equal(NovemberNinth, time.Value);
        Assert.Equal(NovemberNinth.UtcDateTime, time.Value.UtcDateTime);
    }

    [Fact]
    public void ATimeWithoutAnOffsetIsTakenAsUtc()
    {
        var time = WorkItemsReport.Parse("""{ "generatedAt": "2026-11-09T22:00:00" }""")!.GeneratedAt;

        Assert.Equal(TimeSpan.Zero, time!.Value.Offset);
        Assert.Equal(NovemberNinth, time.Value);
    }

    [Fact]
    public void ATimePaddedWithWhitespaceIsStillTheTime() =>
        Assert.Equal(NovemberNinth, WorkItemsReport.Parse("""{ "generatedAt": "  2026-11-09T22:00:00Z  " }""")!.GeneratedAt);

    [Fact]
    public void TheEndsOfTheCalendarAreKept()
    {
        Assert.Equal(DateTimeOffset.MinValue, WorkItemsReport.Parse("""{ "generatedAt": "0001-01-01T00:00:00Z" }""")!.GeneratedAt);
        Assert.Equal(
            new DateTimeOffset(9999, 12, 31, 23, 59, 59, TimeSpan.Zero),
            WorkItemsReport.Parse("""{ "generatedAt": "9999-12-31T23:59:59Z" }""")!.GeneratedAt);
    }

    // ------------------------------------------------------------------------------- the record itself

    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsNameAndTheSplitDefaultsToNull()
    {
        var scope = new WorkItemsScope("cmdemo1", 679, null);
        List<WorkItem> items = [];
        List<WorkRelease> releases = [];

        var report = new WorkItemsReport(NovemberNinth, scope, WorkItemsStats.Empty, items, releases);

        Assert.Equal(NovemberNinth, report.GeneratedAt);
        Assert.Same(scope, report.Scope);
        Assert.Same(WorkItemsStats.Empty, report.Stats);
        Assert.Same(items, report.Items);
        Assert.Same(releases, report.Releases);
        Assert.Null(report.BotStats);
        Assert.Null(report.TeamStats);
    }

    [Fact]
    public void TheBotAndTeamStatsAreHeldWhereTheyAreGiven()
    {
        var bot = new WorkItemsStats(new Dictionary<string, int> { ["Todo"] = 1 }, null, null, [], null, null);
        var team = new WorkItemsStats(new Dictionary<string, int> { ["Done"] = 2 }, null, null, [], null, null);

        var report = new WorkItemsReport(null, null, WorkItemsStats.Empty, [], [], bot, team);

        Assert.Same(bot, report.BotStats);
        Assert.Same(team, report.TeamStats);
        Assert.Null((report with { BotStats = null }).BotStats);
        Assert.Same(team, (report with { BotStats = null }).TeamStats);
    }

    [Fact]
    public void TwoReportsOfTheSameInstancesAreEqualAndAWithCopyTurnsOnePart()
    {
        List<WorkItem> items = [];
        List<WorkRelease> releases = [];
        var report = new WorkItemsReport(NovemberNinth, null, WorkItemsStats.Empty, items, releases);
        var again = new WorkItemsReport(NovemberNinth, null, WorkItemsStats.Empty, items, releases);

        Assert.Equal(report, again);
        Assert.True(report == again);
        Assert.Equal(report.GetHashCode(), again.GetHashCode());

        var later = report with { GeneratedAt = NovemberNinth.AddTicks(1) };

        Assert.NotEqual(report, later);
        Assert.Same(items, later.Items);
        Assert.Null((report with { GeneratedAt = null }).GeneratedAt);
        Assert.NotEqual(report, report with { Items = [] });
    }

    [Fact]
    public void ThePublicSurfaceIsTheFileNameParseAndTheSevenParts()
    {
        Assert.Equal(
            ["BotStats", "Equals", "FileName", "GeneratedAt", "GetHashCode", "Items", "Parse", "Releases", "Scope", "Stats", "TeamStats", "ToString", "op_Equality", "op_Inequality"],
            [.. typeof(WorkItemsReport)
                .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(member => member is PropertyInfo or FieldInfo
                                 || member is MethodInfo method && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
                .Where(member => !member.Name.Contains('<', StringComparison.Ordinal) && member.Name != "Deconstruct")
                .Select(member => member.Name)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)]);
    }
}
