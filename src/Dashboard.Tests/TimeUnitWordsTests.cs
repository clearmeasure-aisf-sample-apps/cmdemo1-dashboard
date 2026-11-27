using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// The duration unit words are named once in <see cref="TimeText"/>, and every way the dashboard says a length of
/// time (a span, a deployment's age, a delivery lead time) ends in that same word for the same range.
/// </summary>
public class TimeUnitWordsTests
{
    /// <summary>The named unit words are the short forms the page has always shown.</summary>
    [Fact]
    public void UnitWordsAreTheShortForms()
    {
        Assert.Equal("s", TimeText.SecondsUnit);
        Assert.Equal("min", TimeText.MinutesUnit);
        Assert.Equal("h", TimeText.HoursUnit);
        Assert.Equal("d", TimeText.DaysUnit);
    }

    /// <summary>A span under a minute ends in the seconds word.</summary>
    [Fact]
    public void SpanUnderAMinuteEndsInTheSecondsWord() =>
        Assert.Equal("45 " + TimeText.SecondsUnit, TimeText.Span(TimeSpan.FromSeconds(45)));

    /// <summary>
    /// For a minute-range, hour-range and day-range length, the span, the deployment age and the lead time all end
    /// in the same named word.
    /// </summary>
    [Theory]
    [InlineData(30.0, TimeText.MinutesUnit)]
    [InlineData(5.0 * 60, TimeText.HoursUnit)]
    [InlineData(3.0 * 24 * 60, TimeText.DaysUnit)]
    public void EveryDurationEndsInTheSameNamedWord(double minutes, string unit)
    {
        var span = TimeSpan.FromMinutes(minutes);
        var suffix = " " + unit;

        Assert.EndsWith(suffix, TimeText.Span(span), StringComparison.Ordinal);
        Assert.EndsWith(suffix, DeploymentText.Age(span), StringComparison.Ordinal);
        Assert.EndsWith(suffix, DeliveryText.LeadTime(span.TotalHours), StringComparison.Ordinal);
    }

    /// <summary>The rendered strings stay byte-for-byte what they were before the words were named.</summary>
    [Fact]
    public void RenderedDurationsAreUnchanged()
    {
        Assert.Equal("30 min", TimeText.Span(TimeSpan.FromMinutes(30)));
        Assert.Equal("30 min", DeploymentText.Age(TimeSpan.FromMinutes(30)));
        Assert.Equal("30 min", DeliveryText.LeadTime(0.5));
        Assert.Equal("5 h", TimeText.Span(TimeSpan.FromHours(5)));
        Assert.Equal("5 h", DeploymentText.Age(TimeSpan.FromHours(5)));
        Assert.Equal("5.2 h", DeliveryText.LeadTime(5.2));
        Assert.Equal("3 d", TimeText.Span(TimeSpan.FromDays(3)));
        Assert.Equal("3 d", DeploymentText.Age(TimeSpan.FromDays(3)));
        Assert.Equal("3.5 d", DeliveryText.LeadTime(84));
    }
}
