using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// The period words of a cost, each one owned by its own named branch of <see cref="CostText" />: this sweep pins
/// the closed set of the words, over the whole range of days a file can name. <see cref="CostTextEdgeCaseTests" />
/// pins the single days where the words turn (<c>yesterday</c> of one day only, <c>this month</c> of the month the
/// rendering stands in, and the date or name of every other day and month); this sweeps the days between and
/// around them and asserts a day of a cost says exactly one of its three words and a month exactly one of its
/// three, so no word none of the named branches settles on can be said of any day.
/// </summary>
public class CostPeriodWordsSweepTests
{
    /// <summary>The moment of the rendering, the same instant the pins in <see cref="CostTextEdgeCaseTests" /> count from.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>The month's name, the name <see cref="CostText.Month" /> says of a month.</summary>
    private static string MonthName(DateOnly day) => day.ToString("MMMM", CultureInfo.InvariantCulture);

    /// <summary>
    /// Over a four-year sweep around the rendering, the word of a day is one of exactly three: <c>in the last full
    /// day</c> when the file names no day, <c>yesterday</c> of the one UTC day before the rendering, and <c>on</c>
    /// the date of every other day. No day of the sweep says any other word.
    /// </summary>
    [Fact]
    public void TheDayWordOfEveryDayIsOneOfItsThreeWords()
    {
        var utcDay = DateOnly.FromDateTime(Now.UtcDateTime);
        for (var offset = -1500; offset <= 1500; offset++)
        {
            var day = utcDay.AddDays(offset);
            Assert.Equal(offset == -1 ? "yesterday" : $"on {CostText.Date(day)}", CostText.Day(day, Now));
        }

        Assert.Equal("in the last full day", CostText.Day(null, Now));
    }

    /// <summary>
    /// Over a four-year sweep around the rendering, the word of a month is one of exactly three: <c>this month</c>
    /// when the file names no day or names a day the rendering stands in, the month's name of the other months of
    /// that year, and that name with its year in any other year. No day of the sweep says any other word.
    /// </summary>
    [Fact]
    public void TheMonthWordOfEveryDayIsOneOfItsThreeWords()
    {
        var utcDay = DateOnly.FromDateTime(Now.UtcDateTime);
        for (var offset = -1500; offset <= 1500; offset++)
        {
            var day = utcDay.AddDays(offset);
            var expected =
                day.Year == utcDay.Year && day.Month == utcDay.Month ? "this month"
                : day.Year == utcDay.Year ? $"in {MonthName(day)}"
                : $"in {MonthName(day)} {day.Year}";
            Assert.Equal(expected, CostText.Month(day, Now));
        }

        Assert.Equal("this month", CostText.Month(null, Now));
    }
}
