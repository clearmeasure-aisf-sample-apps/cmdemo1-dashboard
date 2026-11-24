namespace Dashboard.Tests;

/// <summary>
/// The edges of <see cref="CostText" />: the threshold where a sum keeps its minus sign, where it rounds to nothing,
/// where a day becomes "yesterday" and "this month" becomes "in October", and the arguments a method refuses by name
/// rather than inventing a word.
/// <para>
/// A sum is said to the cent, away from zero, with the sign only while there is something to put a sign in front of:
/// a refund of less than half a cent rounds to nothing and is said without the minus, which is the difference between
/// "the bill was corrected" and "you owe minus nothing". The thousands separator and the decimal point are the
/// invariant ones whoever reads the page, and the currency is what Azure billed in. A zero written with a minus on
/// it is still zero. A number Azure did not give is a dash; a number that is not a number at all is said as the words
/// the invariant format has for it. The currency is compared with <c>USD</c> by sense of case and by nothing else,
/// and an empty code is a code that is not <c>USD</c>.
/// </para>
/// <para>
/// "yesterday" is true of exactly one day: the day the file covers, the UTC day before the rendering. The day itself,
/// the day before that, a day still to come and the first day there is are all named by their date, so a cost file
/// nobody renewed cannot read as though it were this morning's. The day is the day in UTC, wherever the viewer stands.
/// </para>
/// <para>
/// The month a sum covers is "this month" only while the rendering stands in the month the day does - the same month
/// in another year is not this one, December seen from January has a year to name, and a day that is not there names
/// no month at all. A date of the page is written the same in whatever culture the server's thread happens to carry.
/// The three sums always come back as three parts, whether Azure gave the numbers or not. A whole line joins the parts
/// and appends the day when it has one. Services are listed while there is one. No service says nothing and a service
/// of no amount is still listed. An estimate says its share in whole percent. Nothing to estimate and an estimate
/// with no month in it both say nothing. The help and the tooltips take the day they name out of the file. The methods
/// that read a report refuse the one that is not there, by name.
/// </para>
/// </summary>
public class CostTextEdgeCaseTests
{
    /// <summary>The moment this rendering happens, and the instant every "ago" is counted from.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>The last complete UTC day a cost file covers: the day before <see cref="Now" />.</summary>
    private static readonly DateOnly Day = new(2026, 10, 3);

    private static readonly CostAmounts Amounts = new(1.52, 9.8, 11.02);
    private static readonly CostReport Report = new(Now, "USD", Day, Amounts, []);

    /// <summary>
    /// A sum is said to the cent, away from zero, with the sign only while there is something to put a sign in
    /// front of: a refund of less than half a cent rounds to nothing and is said without the minus, which is the
    /// difference between "the bill was corrected" and "you owe minus nothing". The thousands separator and the
    /// decimal point are the invariant ones whoever reads the page, and the currency is what Azure billed in - the
    /// dollar for USD however the code spells it, the code after the number for anything else, nothing at all for a
    /// file that named none, and a dash for a number the file never gave.
    /// </summary>
    [Theory]
    [InlineData(null, "USD", CostText.NoNumber)]
    [InlineData(null, null, CostText.NoNumber)]
    [InlineData(0d, "USD", "$0.00")]
    [InlineData(0d, null, "0.00")]
    [InlineData(0.001, "USD", "$0.00")]
    [InlineData(0.004, "USD", "$0.00")]
    [InlineData(0.005, "USD", "$0.01")]
    [InlineData(0.0051, "USD", "$0.01")]
    [InlineData(-0.0049, "USD", "$0.00")]
    [InlineData(-0.005, "USD", "-$0.01")]
    [InlineData(-0.3, "usd", "-$0.30")]
    [InlineData(999.994, "USD", "$999.99")]
    [InlineData(999.995, "USD", "$1,000.00")]
    [InlineData(-1000.005, "USD", "-$1,000.01")]
    [InlineData(1e21, "USD", "$1,000,000,000,000,000,000,000.00")]
    [InlineData(1.5, "EUR", "1.50 EUR")]
    [InlineData(1.5, "eur", "1.50 eur")]
    [InlineData(1.5, "usd ", "1.50 usd ")]
    [InlineData(1.5, "", "1.50 ")]
    public void ASumIsRoundedAwayFromZeroAndSignedOnlyWhenItRoundsToSomething(double? amount, string? currency, string expected) =>
        Assert.Equal(expected, CostText.Money(amount, currency));

    /// <summary>A zero written with a minus on it is still zero: the sign is only ever put in front of an amount
    /// that rounds to something, so the negative of nothing reads as nothing.</summary>
    [Fact]
    public void ANegativeZeroIsSaidWithoutASign()
    {
        Assert.Equal("$0.00", CostText.Money(-0.0, "USD"));
        Assert.Equal("0.00", CostText.Money(-0.0, null));
        Assert.Equal("0.00 EUR", CostText.Money(-0.0, "EUR"));
    }

    /// <summary>A number Azure did not give is a dash; a number that is not a number at all is said as the words the
    /// invariant format has for it. The cost monitor reads only finite numbers out of the file, so this is what the
    /// public method owes a caller that hands it the rest of the number line - a value pinned here rather than one
    /// quietly allowed to change.</summary>
    [Fact]
    public void ASumThatIsNoNumberIsSaidAsOne()
    {
        Assert.Equal("$NaN", CostText.Money(double.NaN, "USD"));
        Assert.Equal("$Infinity", CostText.Money(double.PositiveInfinity, "USD"));
        Assert.Equal("-$Infinity", CostText.Money(double.NegativeInfinity, "USD"));
        Assert.Equal("NaN", CostText.Money(double.NaN, null));
    }

    /// <summary>The currency is compared with <c>USD</c> by sense of case and by nothing else, and an empty code is
    /// a code that is not <c>USD</c>: the number stands with a trailing space and no symbol. Awkward, and the shape
    /// the page would print today - pinned so that a change of mind here is a change of the page's words.</summary>
    [Fact]
    public void ACurrencyOfNothingNamesNoSymbol()
    {
        Assert.Equal("0.00", CostText.Money(0, null));
        Assert.Equal("0.00 ", CostText.Money(0, string.Empty));
        Assert.Equal("$0.00", CostText.Money(0, "UsD"));
    }

    /// <summary>
    /// "yesterday" is true of exactly one day: the day the file covers, the UTC day before the rendering. The day
    /// itself, the day before that, a day still to come and the first day there is are all named by their date, so a
    /// cost file nobody renewed cannot read as though it were this morning's.
    /// </summary>
    [Theory]
    [InlineData(null, null, null, "in the last full day")]
    [InlineData(2026, 10, 4, "on 2026-10-04")]
    [InlineData(2026, 10, 3, "yesterday")]
    [InlineData(2026, 10, 2, "on 2026-10-02")]
    [InlineData(2026, 10, 5, "on 2026-10-05")]
    [InlineData(2026, 1, 1, "on 2026-01-01")]
    [InlineData(1, 1, 1, "on 0001-01-01")]
    [InlineData(9999, 12, 31, "on 9999-12-31")]
    public void TheDayOfASumIsYesterdayOnlyTheDayAfterIt(int? year, int? month, int? day, string expected) =>
        Assert.Equal(expected, CostText.Day(year is null ? null : new DateOnly(year.Value, month!.Value, day!.Value), Now));

    /// <summary>The day is the day in UTC, wherever the viewer stands: 22:00 on the 4th in UTC is already the 5th in
    /// Tokyo, and the file's "yesterday" is still the 3rd because its days are UTC days.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    [InlineData(-8)]
    public void YesterdayIsTheUtcDayBeforeWhateverTheViewersOffset(int hours) =>
        Assert.Equal("yesterday", CostText.Day(Day, Now.ToOffset(TimeSpan.FromHours(hours))));

    /// <summary>The month a sum covers is "this month" only while the rendering stands in the month the day does -
    /// the same month in another year is not this one, December seen from January has a year to name, and a day that
    /// is not there names no month at all.</summary>
    [Theory]
    [InlineData(null, null, null, "this month")]
    [InlineData(2026, 10, 3, "this month")]
    [InlineData(2026, 10, 31, "this month")]
    [InlineData(2026, 10, 1, "this month")]
    [InlineData(2026, 9, 30, "in September")]
    [InlineData(2026, 11, 1, "in November")]
    [InlineData(2026, 12, 31, "in December")]
    [InlineData(2025, 10, 3, "in October 2025")]
    [InlineData(2025, 12, 31, "in December 2025")]
    public void TheMonthOfASumIsNamedWhenItIsNoLongerThisOne(int? year, int? month, int? day, string expected) =>
        Assert.Equal(expected, CostText.Month(year is null ? null : new DateOnly(year.Value, month!.Value, day!.Value), Now));

    /// <summary>A date of the page is written the same in whatever culture the server's thread happens to carry:
    /// four digits, a month, a day, and the year never cut to two.</summary>
    [Theory]
    [InlineData(1, 1, 1, "0001-01-01")]
    [InlineData(2026, 1, 9, "2026-01-09")]
    [InlineData(2026, 10, 3, "2026-10-03")]
    [InlineData(9999, 12, 31, "9999-12-31")]
    public void ADayIsWrittenTheSameWhereverTheServerRuns(int year, int month, int day, string expected) =>
        Assert.Equal(expected, CostText.Date(new DateOnly(year, month, day)));

    /// <summary>The day the numbers stop at, or nothing when the file never said: the words carry the date and the
    /// zone the days are counted in.</summary>
    [Fact]
    public void TheAsOfNamesItsDayOrSaysNothing()
    {
        Assert.Null(CostText.AsOf(null));
        Assert.Equal("as of 2026-10-03 (UTC)", CostText.AsOf(Day));
        Assert.Equal("as of 0001-01-01 (UTC)", CostText.AsOf(DateOnly.MinValue));
        Assert.Equal("UTC", CostText.DayZone);
    }

    /// <summary>
    /// The three sums always come back as three parts, whether Azure gave the numbers or not: a period is never
    /// left out because its amount is missing, so the line keeps its shape from one refresh to the next and a reader
    /// learns the places rather than the words. The absent amounts are the dash of <see cref="CostText.NoNumber" />.
    /// </summary>
    [Fact]
    public void TheThreePartsAreAlwaysThree()
    {
        Assert.Equal(
            [new CostPart("$1.52", "yesterday"), new CostPart("$9.80", "in 7 days"), new CostPart("$11.02", "this month")],
            CostText.Parts(Amounts, "USD", Day, Now));

        Assert.Equal(
            [new CostPart(CostText.NoNumber, "yesterday"), new CostPart(CostText.NoNumber, "in 7 days"), new CostPart(CostText.NoNumber, "this month")],
            CostText.Parts(new CostAmounts(null, null, null), "USD", Day, Now));

        // A zero is a number Azure gave: it is said, and it is not the dash.
        Assert.Equal(
            [new CostPart("$0.00", "yesterday"), new CostPart("$0.00", "in 7 days"), new CostPart("$0.00", "this month")],
            CostText.Parts(new CostAmounts(0, 0, 0), "USD", Day, Now));

        // An undated file is of "the last full day" and of "this month", which are true whatever the day.
        Assert.Equal(
            [new CostPart("1.52", "in the last full day"), new CostPart("9.80", "in 7 days"), new CostPart("11.02", "this month")],
            CostText.Parts(Amounts, null, null, Now));
    }

    /// <summary>A whole line: the three parts, joined, and the day they stop at only when the file named one.</summary>
    [Fact]
    public void ALineJoinsItsPartsAndAppendsItsDayWhenItHasOne()
    {
        Assert.Equal(
            "$1.52 yesterday · $9.80 in 7 days · $11.02 this month · as of 2026-10-03 (UTC)",
            CostText.Line(Amounts, Report, Now));

        Assert.Equal(
            "— in the last full day · — in 7 days · — this month",
            CostText.Line(new CostAmounts(null, null, null), Report with { AsOf = null }, Now));
    }

    /// <summary>One service is a list of one and two are joined with a comma: the page is handed the whole list and
    /// says it in one run of words, the amounts following the code that billed them.</summary>
    [Theory]
    [InlineData(1, "Most this month: One $1.00")]
    [InlineData(2, "Most this month: One $1.00, Two $2.00")]
    public void ServicesAreListedWhileThereIsOne(int count, string expected)
    {
        CostService[] services =
        [
            new CostService("One", 1),
            new CostService("Two", 2),
        ];

        Assert.Equal(expected, CostText.Services(services[..count], Report, Now));
    }

    /// <summary>No service, an empty list and a service whose amount is missing: the first two say nothing at all,
    /// the third is still a service and its amount is the dash.</summary>
    [Fact]
    public void NoServiceSaysNothingAndAServiceOfNoAmountIsStillListed()
    {
        Assert.Null(CostText.Services(null, Report, Now));
        Assert.Null(CostText.Services([], Report, Now));
        Assert.Equal("Most this month: SQL Database —", CostText.Services([new CostService("SQL Database", null)], Report, Now));
    }

    /// <summary>An estimate is said when it has a month to be of: zero is an amount and is said, no amount at all is
    /// silent, and the share is rounded to whole percent away from zero - half a percent of what all pods request
    /// reads as 1 %, and a tenth of that reads as 0 %.</summary>
    [Theory]
    [InlineData(null, "plus about $0.50 this month of what the environments share")]
    [InlineData(0d, "plus about $0.50 this month of what the environments share (0 % of what all pods request)")]
    [InlineData(0.0049, "plus about $0.50 this month of what the environments share (0 % of what all pods request)")]
    [InlineData(0.005, "plus about $0.50 this month of what the environments share (1 % of what all pods request)")]
    [InlineData(0.2048, "plus about $0.50 this month of what the environments share (20 % of what all pods request)")]
    [InlineData(0.995, "plus about $0.50 this month of what the environments share (100 % of what all pods request)")]
    [InlineData(1d, "plus about $0.50 this month of what the environments share (100 % of what all pods request)")]
    public void AnEstimateSaysItsShareInWholePercent(double? share, string expected) =>
        Assert.Equal(expected, CostText.Estimate(new CostEstimate(share, new CostAmounts(0, 0, 0.5)), Report, Now));

    /// <summary>Nothing to estimate and an estimate with no month in it both say nothing; an estimate of zero for
    /// the month is said, because zero is a number Azure gave and not a number that is missing.</summary>
    [Fact]
    public void AnEstimateOfNoMonthSaysNothing()
    {
        Assert.Null(CostText.Estimate(null, Report, Now));
        Assert.Null(CostText.Estimate(new CostEstimate(0.2, new CostAmounts(1.1, 2.2, null)), Report, Now));
        Assert.Equal(
            "plus about $0.00 this month of what the environments share (20 % of what all pods request)",
            CostText.Estimate(new CostEstimate(0.2048, new CostAmounts(0, 0, 0)), Report, Now));
    }

    /// <summary>The help and the tooltips take the day they name out of the file, and say the shorter true thing
    /// when the file named none. A name of nothing is quoted all the same.</summary>
    [Fact]
    public void TheHelpNamesTheDaysItExplains()
    {
        Assert.StartsWith("Not live: what Azure Cost Management reported for complete UTC days up to 2026-10-03, by", CostText.Help(Report), StringComparison.Ordinal);
        Assert.StartsWith("Not live: what Azure Cost Management reported for complete UTC days, by", CostText.Help(Report with { AsOf = null }), StringComparison.Ordinal);
        Assert.Equal(
            """Azure bills resources with the tag "environment" = "", which is no environment of this topology.""",
            CostText.OtherHelp(string.Empty));
        Assert.Equal(
            """Azure bills resources with the tag "environment" = "retired", which is no environment of this topology.""",
            CostText.OtherHelp("retired"));
    }

    /// <summary>The methods that read a report refuse the one that is not there, by name, rather than answering a
    /// viewer with the words of an empty file: a missing amount is the dash, a missing report is a bug.</summary>
    [Fact]
    public void AReportOfNothingIsRefusedByName()
    {
        Assert.Equal("amounts", Assert.Throws<ArgumentNullException>(() => CostText.Parts(null!, "USD", Day, Now)).ParamName);
        Assert.Equal("amounts", Assert.Throws<ArgumentNullException>(() => CostText.Line(null!, Report, Now)).ParamName);
        Assert.Equal("report", Assert.Throws<ArgumentNullException>(() => CostText.Line(Amounts, null!, Now)).ParamName);
        Assert.Equal("report", Assert.Throws<ArgumentNullException>(() => CostText.Services([], null!, Now)).ParamName);
        Assert.Equal("report", Assert.Throws<ArgumentNullException>(() => CostText.Estimate(null, null!, Now)).ParamName);
        Assert.Equal("report", Assert.Throws<ArgumentNullException>(() => CostText.Help(null!)).ParamName);
    }
}
