using System.Globalization;
using System.Threading;
using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// Additive tests for the private Whole helper in TimeText.Span: each unit's first whole value is cut
/// (truncated) rather than rounded, and the culture of the viewer does not move the digits or the word.
/// </summary>
public class TimeTextWholeUnitTests
{
    /// <summary>
    /// The first whole value of each unit is cut rather than rounded, and a viewer whose culture writes
    /// a comma decimal or a different digit set reads the same words as the invariant culture.
    /// </summary>
    [Theory]
    [InlineData("fi-FI", "00:00:59.999", "59 s")]
    [InlineData("ar-SA", "00:00:59.999", "59 s")]
    [InlineData("fi-FI", "01:59:59.999", "119 min")]
    [InlineData("ar-SA", "01:59:59.999", "119 min")]
    [InlineData("fi-FI", "1.23:59:59.999", "47 h")]
    [InlineData("ar-SA", "1.23:59:59.999", "47 h")]
    public void WholeUnitsAreCutNotRoundedAndAreCultureInvariant(string cultureName, string span, string expected)
    {
        var invariant = TimeText.Span(TimeSpan.Parse(span, CultureInfo.InvariantCulture));
        Assert.Equal(expected, invariant);

        var culture = new CultureInfo(cultureName);
        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = culture;
            Assert.Equal(expected, TimeText.Span(TimeSpan.Parse(span, CultureInfo.InvariantCulture)));
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }

    /// <summary>
    /// The first moment outside the "just now" window says "1 min ago", confirming the named JustNow
    /// threshold in the refactored helper.
    /// </summary>
    [Fact]
    public void AgoAtExactlyOneMinuteIsFirstValueOutsideJustNow()
    {
        var now = new DateTimeOffset(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
        Assert.Equal("just now", TimeText.Ago(now.AddSeconds(-59), now));
        Assert.Equal("1 min ago", TimeText.Ago(now.AddSeconds(-60), now));
    }
}
