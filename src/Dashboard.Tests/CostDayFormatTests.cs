using System.Globalization;
using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// The day of a cost is read and printed in one format, <see cref="CostText.DayFormat" />: the day the page prints is
/// the day the file gave, and a day the file writes any other way is no day at all, whatever the server's culture.
/// </summary>
public class CostDayFormatTests
{
    /// <summary>The smallest cost file that is a report, with <paramref name="asOf" /> as its day.</summary>
    private static string File(string asOf) => "{\"asOf\":\"" + asOf + "\",\"system\":{}}";

    [Fact]
    public void TheDayFormatIsTheIsoCalendarDate() => Assert.Equal("yyyy-MM-dd", CostText.DayFormat);

    [Theory]
    [InlineData(2026, 1, 1)]
    [InlineData(2026, 12, 31)]
    [InlineData(2024, 2, 29)]
    [InlineData(1, 1, 1)]
    [InlineData(9999, 12, 31)]
    public void ADayPrintedByTheTextIsReadBackByTheReport(int year, int month, int dayOfMonth)
    {
        var day = new DateOnly(year, month, dayOfMonth);
        var report = CostReport.Parse(File(CostText.Date(day)));
        Assert.NotNull(report);
        Assert.Equal(day, report.AsOf);
    }

    [Theory]
    [InlineData("2026/10/06")]
    [InlineData("06-10-2026")]
    [InlineData("2026-10-06T00:00:00Z")]
    public void ADayNotWrittenInTheDayFormatIsNoDayInAnyCulture(string asOf)
    {
        var report = CultureScope.Says(CultureInfo.GetCultureInfo("fi-FI"),
            () => CostReport.Parse(File(asOf)));
        Assert.NotNull(report);
        Assert.Null(report.AsOf);
    }
}
