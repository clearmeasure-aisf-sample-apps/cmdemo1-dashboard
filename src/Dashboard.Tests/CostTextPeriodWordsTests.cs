namespace Dashboard.Tests;

/// <summary>
/// The middle period word of a cost and the separator of a cost line, named in <see cref="CostText" />: the seven
/// days the file's <c>last7Days</c> sum covers are always said as "in 7 days", whatever day the file names or the
/// rendering stands on, and a line joins its parts with the middle-dot separator, appending the as-of day last when
/// the file names one.
/// </summary>
public class CostTextPeriodWordsTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void MiddlePeriodIsInSevenDaysWhenTheFileNamesADay()
    {
        var amounts = new CostAmounts(1.52, 9.80, 11.02);

        var middle = CostText.Parts(amounts, "USD", new DateOnly(2026, 11, 25), Now)[1];

        Assert.Equal("$9.80", middle.Amount);
        Assert.Equal("in 7 days", middle.Period);
    }

    [Fact]
    public void MiddlePeriodIsInSevenDaysWhenTheFileNamesNoDay()
    {
        var amounts = new CostAmounts(1.52, 9.80, 11.02);

        var middle = CostText.Parts(amounts, "USD", null, Now)[1];

        Assert.Equal("$9.80", middle.Amount);
        Assert.Equal("in 7 days", middle.Period);
    }

    [Fact]
    public void LineJoinsTheThreePartsWithTheSeparatorWhenTheFileNamesNoDay()
    {
        var amounts = new CostAmounts(1.52, 9.80, 11.02);
        var report = new CostReport(null, "USD", null, null, []);

        Assert.Equal(
            "$1.52 in the last full day · $9.80 in 7 days · $11.02 this month",
            CostText.Line(amounts, report, Now));
    }

    [Fact]
    public void LineAppendsTheAsOfDayLastWhenTheFileNamesADay()
    {
        var amounts = new CostAmounts(1.52, 9.80, 11.02);
        var report = new CostReport(null, "USD", new DateOnly(2026, 10, 6), null, []);

        Assert.Equal(
            "$1.52 on 2026-10-06 · $9.80 in 7 days · $11.02 in October · as of 2026-10-06 (UTC)",
            CostText.Line(amounts, report, Now));
    }
}
