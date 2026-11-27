using System.Globalization;
using System.Threading;
using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// Pins what <see cref="TimeText" />'s three format members print for one fixed moment, in a fixed zone five hours
/// west and under a Finnish current culture, so the invariant culture the members apply is the one that decides the
/// words: the colons of <c>17:00:00</c>, the hyphens and the <c>-05:00</c> of the dated line and the
/// <c>T</c> and the <c>Z</c> of the instant are not a viewer's. The expected strings are literals, not the class's
/// format consts, so a renamed or retyped const that still formats the same way is not mistaken for a fix.
/// </summary>
public class TimeTextFormatTests
{
    /// <summary>The instant every format below is asked for, in UTC.</summary>
    private static readonly DateTimeOffset Moment = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>A zone five hours west of UTC, made by the test so no machine's time zone database decides a value.</summary>
    private static readonly TimeZoneInfo WestFive = TimeZoneInfo.CreateCustomTimeZone(
        "west-five-format", TimeSpan.FromHours(-5), "West Five", "West Five");

    /// <summary>The three members say one fixed moment the dashboard's way, whatever culture the viewer speaks:
    /// the clock to the second, the dated line to the minute with its offset from UTC, and the instant a
    /// <c>&lt;time&gt;</c> element hands a machine.</summary>
    [Fact]
    public void TheThreeFormatsAreTheDashboardSaidNotTheViewers()
    {
        var reader = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("fi-FI");
        try
        {
            Assert.Equal("17:00:00", TimeText.Clock(Moment, WestFive));
            Assert.Equal("2026-10-04 17:00 -05:00", TimeText.DateAndTime(Moment, WestFive));
            Assert.Equal("2026-10-04T22:00:00Z", TimeText.Iso(Moment));
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }
}
