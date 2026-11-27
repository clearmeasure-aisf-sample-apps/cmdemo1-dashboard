namespace Dashboard.Tests;

/// <summary>
/// The entries of a cost report and what the report does with them: the shared one is found by its name, the ones
/// the topology no longer knows are the others, and an entry Azure gave no number to has none.
/// </summary>
public class CostReportTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    private static CostAmounts Amounts(double? yesterday, double? last7Days, double? monthToDate) =>
        new(yesterday, last7Days, monthToDate);

    [Fact]
    public void TheSharedEntryIsFoundByNameWithoutACareForCase()
    {
        Assert.True(CostReport.IsShared("shared"));
        Assert.True(CostReport.IsShared("Shared"));
        Assert.True(CostReport.IsShared("SHARED"));
        Assert.False(CostReport.IsShared("tdd"));
        Assert.False(CostReport.IsShared("shares"));
        Assert.False(CostReport.IsShared(""));

        var shared = new CostEntry("SHARED", Amounts(1.1, 7.7, 8.2), []);
        var report = new CostReport(null, "USD", null, null, [
            new CostEntry("tdd", Amounts(0, 1.5, 1.59), []),
            shared,
        ]);

        Assert.Same(shared, report.Shared);
        Assert.Same(shared, report.Find("shared"));
        Assert.Same(shared, report.Find("shArEd"));
        Assert.Equal(Amounts(1.1, 7.7, 8.2), report.Shared!.Amounts);
    }

    [Fact]
    public void AReportWithoutAnEntryNamedSharedHasNone()
    {
        var report = new CostReport(null, "USD", null, null, [
            new CostEntry("tdd", Amounts(0, 1.5, 1.59), []),
            new CostEntry("uat", Amounts(1.52, 9.8, 11.02), []),
        ]);

        Assert.Null(report.Shared);
        Assert.Null(report.Find("SHARED"));
    }

    [Fact]
    public void OthersAreTheEntriesTheShownEnvironmentsDoNotOwnAndTheSharedIsNeverOne()
    {
        var report = new CostReport(null, "USD", null, null, [
            new CostEntry("tdd", Amounts(0, 1.5, 1.59), []),
            new CostEntry("uat", Amounts(1.52, 9.8, 11.02), []),
            new CostEntry("retired", Amounts(null, 0.2, 0.2), []),
            new CostEntry("Shared", Amounts(1.1, 7.7, 8.2), []),
            new CostEntry("staging", Amounts(0.4, 2.1, 3.7), []),
        ]);

        // The shown environments are recognized without a care for case, the shared entry never counts, and the
        // order of the file is kept.
        var others = report.Others(["TDD", "UAT"]);
        Assert.Equal(["retired", "staging"], others.Select(entry => entry.Name));
        Assert.Equal(Amounts(null, 0.2, 0.2), others[0].Amounts);
        Assert.Equal(Amounts(0.4, 2.1, 3.7), others[1].Amounts);
        Assert.Equal(["staging"], report.Others(["tdd", "uat", "retired", "shared"]).Select(entry => entry.Name));
        Assert.Equal(["tdd", "uat", "retired", "staging"], report.Others([]).Select(entry => entry.Name));
        Assert.Throws<ArgumentNullException>(() => report.Others(null!));
    }

    [Fact]
    public void AnEntryAzureGaveNoNumberToHasNoAmountAndNoServices()
    {
        var report = CostReport.Parse(
            """
            { "currency": "USD", "asOf": "2026-10-03",
              "environments": [
                { "name": "ghost" },
                { "name": "half", "last7Days": 9.8 },
                { "name": "refund", "yesterday": -0.3, "topServices": [ { "name": "Azure Front Door Service" }, { "monthToDate": 4.2 }, 7 ] } ] }
            """)!;

        Assert.Equal(["ghost", "half", "refund"], report.Entries.Select(entry => entry.Name));

        var ghost = report.Find("ghost")!;
        Assert.Equal(Amounts(null, null, null), ghost.Amounts);
        Assert.Empty(ghost.TopServices);
        Assert.Null(ghost.Estimate);

        Assert.Equal(Amounts(null, 9.8, null), report.Find("half")!.Amounts);

        var refund = report.Find("refund")!;
        Assert.Equal(Amounts(-0.3, null, null), refund.Amounts);
        Assert.Equal([new CostService("Azure Front Door Service", null)], refund.TopServices);

        // A line without numbers is all dashes; a refund keeps its sign.
        Assert.Equal("— yesterday · — in 7 days · — this month · as of 2026-10-03 (UTC)", CostText.Line(ghost.Amounts, report, Now));
        Assert.Equal("-$0.30 yesterday · — in 7 days · — this month · as of 2026-10-03 (UTC)", CostText.Line(refund.Amounts, report, Now));
    }

    [Fact]
    public void AnElementWithoutAnEnvironmentNameIsNotAnEntry()
    {
        var report = CostReport.Parse("""
            { "environments": [ { }, "tdd", { "name": "  " }, { "name": "uat", "yesterday": 1.52 } ] }
            """)!;

        Assert.Equal(["uat"], report.Entries.Select(entry => entry.Name));
        Assert.Equal(Amounts(1.52, null, null), report.Find("uat")!.Amounts);
    }

    [Theory]
    [InlineData("  ")]
    [InlineData("{}")]
    [InlineData("""{ "environments": [] }""")]
    [InlineData("""{ "generated": "2026-10-04T05:00:00Z", "currency": "USD", "asOf": "2026-10-03", "system": null, "environments": [] }""")]
    public void AFileThatNamesNoNumberIsNoReport(string json) =>
        Assert.Null(CostReport.Parse(json));

    [Fact]
    public void AFileThatNamesOnlyTheSystemIsStillAReportWithoutEntries()
    {
        var report = CostReport.Parse("""
            { "currency": "USD", "asOf": "2026-10-03", "system": { "yesterday": 3.41, "last7Days": 22.1, "monthToDate": 1204.8 } }
            """)!;

        Assert.Equal(Amounts(3.41, 22.1, 1204.8), report.System);
        Assert.Empty(report.Entries);
        Assert.Null(report.Shared);
        Assert.Empty(report.Others([]));
    }

    [Fact]
    public void AnEmptyReportHasNoSharedAndNoOthers()
    {
        var report = new CostReport(null, null, null, null, []);

        Assert.Empty(report.Entries);
        Assert.Null(report.System);
        Assert.Null(report.Shared);
        Assert.Null(report.Find("shared"));
        Assert.Empty(report.Others(["tdd", "uat"]));
    }

    [Fact]
    public void AnEntryAndItsServiceEachReadTheirMonthToDateFromTheSameWord()
    {
        var report = CostReport.Parse(
            """
            { "environments": [ { "name": "uat", "monthToDate": 11.02, "topServices": [ { "name": "SQL Database", "monthToDate": 6.10 } ] } ] }
            """)!;

        var entry = report.Find("uat")!;
        Assert.Equal(11.02, entry.Amounts.MonthToDate);
        Assert.Equal(6.10, entry.TopServices[0].MonthToDate);
    }
}
