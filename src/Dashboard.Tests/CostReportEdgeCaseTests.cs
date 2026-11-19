namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of the cost report and the words it is said in. What is pinned
/// where it belongs: <see cref="CostReportTests" /> pins the entries a report holds - the shared one found by its
/// name, the ones the topology no longer knows being the others, an entry Azure gave no number to, a file that
/// names no number being no report, and the report without a shared one - and <see cref="CostTests" /> pins the
/// file the optics read, its words in the currency it bills, the day and the month its numbers cover with the
/// clock the test brings, the services that cost most, the estimate of one file, and the reading of the file by
/// the monitor. What is pinned here is the rest of the same members at their ends: the shared name in its exact
/// letters and the names the report is asked for that are not there, two entries that share a name and the one
/// that comes first in the file, the shown environments that repeat and the one that is empty; the file that is
/// cut off and the one whose root is a number, a word, a boolean or the word for nothing, the <c>environments</c>
/// and the <c>system</c> of a kind that is no array and no object, the <c>asOf</c> that is a day of no month and a
/// day with its time, and the <c>asOf</c> with its whitespace the file may still give, the <c>generated</c> that
/// is a date or a word, the amount in the exponential the file may write and the one of a kind that is no number,
/// the name read with its whitespace trimmed to the shared one, the estimate's share on its two ends and past
/// them, and the <c>topServices</c> of a kind that is no array; the money that rounds onto the next whole and the
/// one that carries into a new digit, the amount that is no number at all, and the currency that is empty or that
/// only looks like the dollar; the day that is today and the one at the end of the calendar, the month that comes
/// and the one whose name the year breaks, the dates at the ends of the calendar; the words that are constants,
/// word for word, and the help in its whole sentence; and the argument that is not there, refused by the name of
/// each parameter that is guarded.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every moment is one the test brings.
/// </para>
/// </summary>
public class CostReportEdgeCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    private static readonly CostReport Report = new(null, "USD", new DateOnly(2026, 10, 3), null, [
        new CostEntry("tdd", Amounts(0, 1.5, 1.59), []),
        new CostEntry("uat", Amounts(1.52, 9.8, 11.02), []),
        new CostEntry("retired", Amounts(null, 0.2, 0.2), []),
    ]);

    private static CostAmounts Amounts(double? yesterday, double? last7Days, double? monthToDate) =>
        new(yesterday, last7Days, monthToDate);

    // ----- the shared name, and the names the report is asked for

    /// <summary>The shared name is the word the shared entry is found by, in its exact letters and its exact
    /// case: the finding is without a care for case, the name itself is not.</summary>
    [Fact]
    public void TheSharedNameIsTheWordItSays() =>
        Assert.Equal("shared", CostReport.SharedName);

    /// <summary>The name that is not there, and the name that only looks like the shared one with its whitespace,
    /// are not the shared one: the comparison is of the whole name, not of a trimmed part of it.</summary>
    [Fact]
    public void TheNamesThatAreNotTheSharedOneAreNotShared()
    {
        Assert.False(CostReport.IsShared(null!));
        Assert.False(CostReport.IsShared(" shared "));
        Assert.False(CostReport.IsShared("Shared "));
        Assert.False(CostReport.IsShared("sharedness"));
    }

    /// <summary>The report without an entry of the name is asked for says none - the name that is not there and
    /// the one that is empty - and it says none for the shared name it holds no entry of.</summary>
    [Fact]
    public void TheNamesTheReportHasNoEntryOfAreFoundAsNone()
    {
        Assert.Null(Report.Find(null!));
        Assert.Null(Report.Find(""));
        Assert.Null(Report.Find("shared"));
        Assert.Null(Report.Shared);
    }

    /// <summary>Two entries that share a name, in the case they each say it, are the one that comes first in the
    /// file: the finding walks the file and keeps what it meets first, for the shared entry no differently.</summary>
    [Fact]
    public void TheEntriesThatShareANameAreKeptInTheOrderOfTheFile()
    {
        var first = new CostEntry("shared", Amounts(1, 2, 3), []);
        var second = new CostEntry("SHARED", Amounts(4, 5, 6), []);
        var report = new CostReport(null, "USD", null, null, [first, second]);

        Assert.Equal(["shared", "SHARED"], report.Entries.Select(entry => entry.Name));
        Assert.Same(first, report.Find("shared"));
        Assert.Same(first, report.Find("Shared"));
        Assert.Same(first, report.Shared);
        Assert.Equal(Amounts(1, 2, 3), report.Shared!.Amounts);
    }

    // ----- the others, at the ends of the shown environments

    /// <summary>The shown environments that repeat in the case they each say it are shown once, the shown
    /// environment that is empty shows nothing, and the shown environments that are all of them the report's own
    /// leave it with no others.</summary>
    [Fact]
    public void TheShownEnvironmentsMayRepeatBeEmptyAndBeAllOfThem()
    {
        Assert.Equal(["retired"], Report.Others(["TDD", "tdd", "UAT", ""]).Select(entry => entry.Name));
        Assert.Equal(["tdd", "uat", "retired"], Report.Others([""]).Select(entry => entry.Name));
        Assert.Empty(Report.Others(["tdd", "uat", "retired"]));
    }

    /// <summary>The shown environments that are not there are refused by the name of the parameter the guard
    /// keeps: the report does not take a missing list for one that shows nothing.</summary>
    [Fact]
    public void TheShownEnvironmentsThatAreNotThereAreRefusedByName() =>
        Assert.Equal("environments", Assert.Throws<ArgumentNullException>(() => Report.Others(null!)).ParamName);

    // ----- the file, at the ends of what a file may say

    /// <summary>The file that is cut off is no report, and neither is the file whose root is a number, a word, a
    /// boolean or the word for nothing: a report is the object the file may be, and the object is read for the
    /// parts it holds.</summary>
    [Theory]
    [InlineData("{")]
    [InlineData("[1, 2")]
    [InlineData("""{ "environments": [""")]
    [InlineData("nul")]
    [InlineData("123")]
    [InlineData("1.5")]
    [InlineData("true")]
    [InlineData("null")]
    [InlineData("\"cost\"")]
    public void AFileThatIsCutOffOrWhoseRootIsNoObjectIsNoReport(string json) =>
        Assert.Null(CostReport.Parse(json));

    /// <summary>The <c>environments</c> of a kind that is no array reads no entries - so with no <c>system</c> the
    /// file is no report, and with one it is a report the environments of which are none - and the <c>system</c> of
    /// a kind that is no object reads no system, while the environments beside it are read as they stand.</summary>
    [Fact]
    public void ThePartsOfAKindThatIsNotTheirsAreReadAsNone()
    {
        Assert.Null(CostReport.Parse("""{ "environments": { "name": "tdd" } }"""));

        var withSystem = CostReport.Parse("""{ "environments": 7, "system": { "yesterday": 1 } }""")!;
        Assert.Empty(withSystem.Entries);
        Assert.Equal(Amounts(1, null, null), withSystem.System);

        var withSystemOfNoObject = CostReport.Parse("""{ "system": [1], "environments": [ { "name": "tdd" } ] }""")!;
        Assert.Null(withSystemOfNoObject.System);
        Assert.Equal(["tdd"], withSystemOfNoObject.Entries.Select(entry => entry.Name));
    }

    /// <summary>The <c>asOf</c> that is a day of no month, a month that is no month, a day without its two digits,
    /// and a day that says its time is no day the file names - the report beside them is still a report, its day
    /// being what it does not say.</summary>
    [Theory]
    [InlineData("2026-02-30")]
    [InlineData("2026-13-01")]
    [InlineData("2026-1-03")]
    [InlineData("2026-10-03T00:00:00Z")]
    public void ADayThatIsNotTheFileSaysNoDay(string asOf)
    {
        var json = "{ \"asOf\": \"" + asOf + "\", \"environments\": [ { \"name\": \"tdd\", \"yesterday\": 1 } ] }";
        var report = CostReport.Parse(json)!;

        Assert.Null(report.AsOf);
        Assert.Equal(Amounts(1, null, null), report.Find("tdd")!.Amounts);
    }

    /// <summary>The <c>asOf</c> with its whitespace is the day it is: the day is read exact, and the whitespace a
    /// day may be given with is not part of the day it is not.</summary>
    [Fact]
    public void TheDayWithItsWhitespaceIsTheDayItIs()
    {
        var report = CostReport.Parse("""{ "asOf": " 2026-10-03 ", "environments": [ { "name": "tdd", "yesterday": 1 } ] }""")!;

        Assert.Equal(new DateOnly(2026, 10, 3), report.AsOf);
    }

    /// <summary>The <c>generated</c> that is a date is the midnight the date says, the one that is a word is no
    /// moment, and the one that is a moment is the moment it is: the moment is read as the words read it, and the
    /// day of a cost is no part of it.</summary>
    [Fact]
    public void TheMomentTheFileChangedIsTheMomentItIsSaid()
    {
        var byDate = CostReport.Parse("""{ "generated": "2026-10-04", "environments": [ { "name": "tdd" } ] }""")!;
        Assert.Equal(new DateTimeOffset(2026, 10, 4, 0, 0, 0, TimeSpan.Zero), byDate.Generated);

        var byTime = CostReport.Parse("""{ "generated": "2026-10-04 05:00:00Z", "environments": [ { "name": "tdd" } ] }""")!;
        Assert.Equal(new DateTimeOffset(2026, 10, 4, 5, 0, 0, TimeSpan.Zero), byTime.Generated);

        Assert.Null(CostReport.Parse("""{ "generated": "tomorrow", "environments": [ { "name": "tdd" } ] }""")!.Generated);
    }

    /// <summary>The amount in the exponential the file may write is the number it says, with its sign, and the
    /// amount of a kind that is no number - a truth, a list - is the amount Azure did not give.</summary>
    [Fact]
    public void TheAmountIsTheNumberItIsSaidAndNotTheKindItIsNot()
    {
        var report = CostReport.Parse("""
            { "system": { "yesterday": 1.5e2, "last7Days": -1.5e2, "monthToDate": true },
              "environments": [ { "name": "list", "yesterday": [1] } ] }
            """)!;

        Assert.Equal(Amounts(150, -150, null), report.System);
        Assert.Equal(Amounts(null, null, null), report.Find("list")!.Amounts);
    }

    /// <summary>The name the file gives with its whitespace is the name without it: read and trimmed, an entry
    /// named the shared one with its air around it is the shared entry - while the name as the file gave it,
    /// whitespace and all, is not the shared name the report is asked for by.</summary>
    [Fact]
    public void TheNameIsReadWithItsWhitespaceTrimmed()
    {
        var report = CostReport.Parse("""{ "environments": [ { "name": "  SHARED  ", "yesterday": 1 } ] }""")!;
        var entry = Assert.Single(report.Entries);

        Assert.Equal("SHARED", entry.Name);
        Assert.Same(entry, report.Shared);
        Assert.False(CostReport.IsShared("  SHARED  "));
    }

    /// <summary>The estimate's share is the share it is on the ends it may stand on and none past them: the whole
    /// and the nothing are the share, one more a hair than the whole and a hair below the nothing are no share the
    /// amount beside them standing, and the estimate with no amount, with no part at all, or of a kind that is no
    /// object is no estimate.</summary>
    [Fact]
    public void TheShareOfTheEstimateStandsOnItsEndsAndNonePastThem()
    {
        var report = CostReport.Parse("""
            { "environments": [
              { "name": "zero", "yesterday": 0, "estimate": { "share": 0, "monthToDate": 2 } },
              { "name": "whole", "yesterday": 0, "estimate": { "share": 1, "monthToDate": 3 } },
              { "name": "over", "yesterday": 0, "estimate": { "share": 1.0000001, "monthToDate": 4 } },
              { "name": "below", "yesterday": 0, "estimate": { "share": -1, "monthToDate": 5 } },
              { "name": "noamount", "yesterday": 0, "estimate": { "share": 0.5 } },
              { "name": "empty", "yesterday": 0, "estimate": { } },
              { "name": "array", "yesterday": 0, "estimate": [1] } ] }
            """)!;

        Assert.Equal(new CostEstimate(0, Amounts(null, null, 2)), report.Find("zero")!.Estimate);
        Assert.Equal(new CostEstimate(1, Amounts(null, null, 3)), report.Find("whole")!.Estimate);
        Assert.Equal(new CostEstimate(null, Amounts(null, null, 4)), report.Find("over")!.Estimate);
        Assert.Equal(new CostEstimate(null, Amounts(null, null, 5)), report.Find("below")!.Estimate);
        Assert.Null(report.Find("noamount")!.Estimate);
        Assert.Null(report.Find("empty")!.Estimate);
        Assert.Null(report.Find("array")!.Estimate);
    }

    /// <summary>The <c>topServices</c> of a kind that is no array names no service, and the service whose name is
    /// only whitespace is the service the file did not name: the ones with their names stand, in the order of the
    /// file.</summary>
    [Fact]
    public void TheServicesOfAKindThatIsNoArrayNameNoService()
    {
        var report = CostReport.Parse("""
            { "environments": [
              { "name": "words", "topServices": "none" },
              { "name": "whitespace", "topServices": [ { "name": "  " }, { "name": "SQL Database", "monthToDate": 1.25 } ] } ] }
            """)!;

        Assert.Empty(report.Find("words")!.TopServices);
        Assert.Equal([new CostService("SQL Database", 1.25)], report.Find("whitespace")!.TopServices);
    }

    // ----- the entries the report is built of, without the file

    /// <summary>The entry the report is built of without its estimate is the one the estimate is none on: the
    /// estimate is the part the entry may do without, and it is none, not the entry, that is absent.</summary>
    [Fact]
    public void TheEntryBuiltWithoutAnEstimateHasNone() =>
        Assert.Null(new CostEntry("tdd", Amounts(0, 0, 0), []).Estimate);

    /// <summary>The amounts that are the one zero and the other zero are the amounts they are said to be: the
    /// sign a zero may carry is not part of the amount it is not.</summary>
    [Fact]
    public void TheAmountsThatAreOneZeroOrTheOtherAreTheSame() =>
        Assert.Equal(Amounts(0.0, null, null), Amounts(-0.0, null, null));

    // ----- the money, at the ends of the number and the currency

    /// <summary>The money that rounds onto the next whole and the one that carries into a new digit: a half at the
    /// end of the two figures goes away from the zero, a thousandth that stands a hair below the half does not, and
    /// the carry of the last of one magnitude into the first of the next is the comma that stands where it stands.
    /// The currency that is empty is the currency the number is said in with its space, the one that only looks
    /// like the dollar is the dollar, and the one that is another is the code it is.</summary>
    [Theory]
    [InlineData(-0.005d, "USD", "-$0.01")]
    [InlineData(0.125d, null, "0.13")]
    [InlineData(0.375d, null, "0.38")]
    [InlineData(0.625d, null, "0.63")]
    [InlineData(-0.375d, "USD", "-$0.38")]
    [InlineData(999999.994d, "USD", "$999,999.99")]
    [InlineData(999999.999d, "USD", "$1,000,000.00")]
    [InlineData(-999999.999d, "USD", "-$1,000,000.00")]
    [InlineData(1000.004d, "USD", "$1,000.00")]
    [InlineData(1000.006d, "USD", "$1,000.01")]
    [InlineData(double.NaN, null, "NaN")]
    [InlineData(double.NaN, "USD", "$NaN")]
    [InlineData(double.NaN, "EUR", "NaN EUR")]
    [InlineData(double.PositiveInfinity, "USD", "$Infinity")]
    [InlineData(double.PositiveInfinity, null, "Infinity")]
    [InlineData(double.NegativeInfinity, "USD", "-$Infinity")]
    [InlineData(1.5d, "", "1.50 ")]
    [InlineData(1.5d, "uSd", "$1.50")]
    [InlineData(1.5d, "GBP", "1.50 GBP")]
    public void TheMoneyStandsAtTheEndsOfTheNumberAndTheKindsOfTheCurrency(double? amount, string? currency, string expected) =>
        Assert.Equal(expected, CostText.Money(amount, currency));

    // ----- the day and the month, at the ends of the calendar

    /// <summary>The day that is today is not yesterday and is said by its date, the day that is not yet is said
    /// by its date no differently, and the day at the end of the calendar is the date it is: the word for the day
    /// before the now is the one the test brings, and nothing else of the calendar is it.</summary>
    [Fact]
    public void TheDayThatIsTodayOrNotYetIsSaidByItsDate()
    {
        Assert.Equal("on 2026-10-04", CostText.Day(new DateOnly(2026, 10, 4), Now));
        Assert.Equal("on 2026-10-05", CostText.Day(new DateOnly(2026, 10, 5), Now));
        Assert.Equal("on 0001-01-01", CostText.Day(DateOnly.MinValue, Now));
    }

    /// <summary>The month that comes is the month it is, and the month whose name is the name of the now's month
    /// in another year is the year that breaks the name: the month is the month the day is in, and the year is
    /// said when it is not the year the now stands in.</summary>
    [Fact]
    public void TheMonthThatComesAndTheOneWhoseNameTheYearBreaksAreSaidAsTheyAre()
    {
        Assert.Equal("in November", CostText.Month(new DateOnly(2026, 11, 2), Now));
        Assert.Equal("in December", CostText.Month(new DateOnly(2026, 12, 31), Now));
        Assert.Equal("in October 2025", CostText.Month(new DateOnly(2025, 10, 15), Now));
        Assert.Equal("in October 2027", CostText.Month(new DateOnly(2027, 10, 15), Now));
    }

    /// <summary>The dates at the ends of the calendar are the dates they are, the year at the hundred said with
    /// its four figures, and the <c>as of</c> at the ends of the calendar the day it names and the zone the days
    /// of a cost are in.</summary>
    [Fact]
    public void TheDatesAtTheEndsOfTheCalendarAreSaidAsTheyAre()
    {
        Assert.Equal("0001-01-01", CostText.Date(DateOnly.MinValue));
        Assert.Equal("0100-01-01", CostText.Date(new DateOnly(100, 1, 1)));
        Assert.Equal("9999-12-31", CostText.Date(DateOnly.MaxValue));
        Assert.Equal("as of 0001-01-01 (UTC)", CostText.AsOf(DateOnly.MinValue));
        Assert.Equal("as of 9999-12-31 (UTC)", CostText.AsOf(DateOnly.MaxValue));
    }

    // ----- the words that are constants, and the help in its whole sentence

    /// <summary>The words that are constants are the words they are, word for word: the dash that stands for the
    /// number Azure did not give, the zone the days of a cost are in, and the two helps the tooltips are made of
    /// - what the estimate is and what the shared entry holds.</summary>
    [Fact]
    public void TheWordsThatAreConstantsAreTheWordsTheyAre()
    {
        Assert.Equal("—", CostText.NoNumber);
        Assert.Equal("UTC", CostText.DayZone);
        Assert.Equal(
            "An estimate, not a bill: the cost of the cluster that no environment's tag claims, times the share of CPU and memory "
                + "this environment's pods request of what all running pods request now. It stays part of what the environments share.",
            CostText.EstimateHelp);
        Assert.Equal(
            "What carries no tag \"environment\": the resources the environments share, such as the Front Door profile, the registry "
                + "and the Terraform state, and what Azure bills without tags.",
            CostText.SharedHelp);
    }

    /// <summary>The help is the sentence it is, with its day and without it: what the numbers are, how old, and
    /// how they arrive - the day the numbers are of where the file gives it, and the whole sentence the tooltips
    /// are made of.</summary>
    [Fact]
    public void TheHelpIsTheSentenceItIsWithItsDayAndWithoutIt()
    {
        Assert.Equal(
            "Not live: what Azure Cost Management reported for complete UTC days up to 2026-10-03, by the tag \"environment\" of the resources. "
                + "Azure's cost arrives hours late and is amended for a day or two; a workflow of the system repository reads it hourly "
                + "until the day is complete, then every six hours.",
            CostText.Help(Report));
        Assert.Equal(
            "Not live: what Azure Cost Management reported for complete UTC days, by the tag \"environment\" of the resources. "
                + "Azure's cost arrives hours late and is amended for a day or two; a workflow of the system repository reads it hourly "
                + "until the day is complete, then every six hours.",
            CostText.Help(Report with { AsOf = null }));
    }

    /// <summary>The word of the entry that belongs to no environment of the page, at the ends of the name: the
    /// name that is empty stands in its quotes, and the name that is the shared one stands in them no differently
    /// - the entry it is said of is the one Azure bills, whatever the topology says of it.</summary>
    [Fact]
    public void TheWordOfTheOtherEntryStandsAtTheEndsOfTheName()
    {
        Assert.Equal(
            "Azure bills resources with the tag \"environment\" = \"\", which is no environment of this topology.",
            CostText.OtherHelp(""));
        Assert.Equal(
            "Azure bills resources with the tag \"environment\" = \"shared\", which is no environment of this topology.",
            CostText.OtherHelp("shared"));
    }

    // ----- the words of the estimate, on the ends of the share

    /// <summary>The share of the estimate is said as the whole it rounds to, on the ends the whole takes: a hair
    /// below the half of one percent is no percent, the half is the percent it rounds away from the zero to, a hair
    /// below the whole is the whole the rounding takes it to, and the ends themselves stand on themselves.</summary>
    [Theory]
    [InlineData(0d, " (0 % of what all pods request)")]
    [InlineData(0.004d, " (0 % of what all pods request)")]
    [InlineData(0.005d, " (1 % of what all pods request)")]
    [InlineData(0.995d, " (100 % of what all pods request)")]
    [InlineData(1d, " (100 % of what all pods request)")]
    public void TheShareOfTheEstimateIsSaidOnItsEnds(double share, string expected) =>
        Assert.Equal(
            $"plus about $2.00 this month of what the environments share{expected}",
            CostText.Estimate(new CostEstimate(share, Amounts(null, null, 2)), Report, Now));

    /// <summary>The month that is the zero the estimate was given is the month the estimate says: the zero is the
    /// amount Azure gave, not the amount it did not, and it stands where the dash of the amount it did not is
    /// none.</summary>
    [Fact]
    public void TheMonthThatIsTheZeroTheEstimateWasGivenIsSaid() =>
        Assert.Equal(
            "plus about $0.00 this month of what the environments share",
            CostText.Estimate(new CostEstimate(null, Amounts(null, null, 0)), Report, Now));

    // ----- the arguments that are not there, refused by name

    /// <summary>The words that take the amounts or the report refuse the one that is not there, each by the name
    /// of its own parameter: none of them decides to say nothing in place of what it was not handed.</summary>
    [Fact]
    public void TheWordsThatTakeTheReportOrTheAmountsRefuseTheOneThatIsNotThere()
    {
        Assert.Equal("amounts", Assert.Throws<ArgumentNullException>(() => CostText.Parts(null!, "USD", null, Now)).ParamName);
        Assert.Equal("report", Assert.Throws<ArgumentNullException>(() => CostText.Line(Amounts(0, 0, 0), null!, Now)).ParamName);
        Assert.Equal("report", Assert.Throws<ArgumentNullException>(() => CostText.Services([], null!, Now)).ParamName);
        Assert.Equal("report", Assert.Throws<ArgumentNullException>(() => CostText.Estimate(null, null!, Now)).ParamName);
        Assert.Equal("report", Assert.Throws<ArgumentNullException>(() => CostText.Help(null!)).ParamName);
    }
}
