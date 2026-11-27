using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// The duration unit words live once on <see cref="TimeText" />; <see cref="TimeText.Span" />,
/// <see cref="DeploymentText.Age" /> and <see cref="DeliveryText.LeadTime" /> all end in the same word for the same band.
/// </summary>
public class TimeUnitWordsTests
{
    [Fact]
    public void TheUnitWordsAreTheShortForms()
    {
        Assert.Equal("s", TimeText.SecondsUnit);
        Assert.Equal("min", TimeText.MinutesUnit);
        Assert.Equal("h", TimeText.HoursUnit);
        Assert.Equal("d", TimeText.DaysUnit);
    }

    [Fact]
    public void SpanSaysSecondsInTheSecondsUnit() =>
        Assert.Equal($"45 {TimeText.SecondsUnit}", TimeText.Span(TimeSpan.FromSeconds(45)));

    [Theory]
    [InlineData(30, TimeText.MinutesUnit)]
    [InlineData(5 * 60, TimeText.HoursUnit)]
    [InlineData(4 * 24 * 60, TimeText.DaysUnit)]
    public void SpanAgeAndLeadTimeEndInTheSameUnitWord(int minutes, string unit)
    {
        var span = TimeSpan.FromMinutes(minutes);

        Assert.EndsWith($" {unit}", TimeText.Span(span), StringComparison.Ordinal);
        Assert.EndsWith($" {unit}", DeploymentText.Age(span), StringComparison.Ordinal);
        Assert.EndsWith($" {unit}", DeliveryText.LeadTime(span.TotalHours), StringComparison.Ordinal);
    }
}
