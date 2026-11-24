using System.Globalization;
using System.Reflection;
using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// #1644: "whole units, invariant culture, in words" has one home, <see cref="TimeText"/>, and
/// <see cref="DeploymentText.Age(TimeSpan)"/> says its count through it while keeping its own unit boundaries.
/// </summary>
public class DeploymentTextAgeWholeHomeTests
{
    private const BindingFlags AnyStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

    [Fact]
    public void TheWordsOfAWholeCountLiveInTimeTextAlone()
    {
        Assert.Single(typeof(TimeText).GetMethods(AnyStatic), method => method.Name == "Whole");
        Assert.DoesNotContain(typeof(DeploymentText).GetMethods(AnyStatic), method => method.Name == "Whole");
    }

    [Theory]
    [InlineData(0.0, "1 min")]
    [InlineData(59.999, "59 min")]
    [InlineData(60.0, "1 h")]
    [InlineData(330.0, "5 h")]
    [InlineData(2879.999, "47 h")]
    [InlineData(2880.0, "2 d")]
    [InlineData(5000.0, "3 d")]
    public void AnAgeIsCutNotRoundedWithItsOwnBoundaries(double minutes, string expected) =>
        Assert.Equal(expected, DeploymentText.Age(TimeSpan.FromMinutes(minutes)));

    [Theory]
    [InlineData("fi-FI")]
    [InlineData("ar-SA")]
    [InlineData("hi-IN")]
    public void AnAgeIsSaidTheSameInEveryCulture(string cultureName)
    {
        var minutes = new[] { 0.5, 5.5, 90.0, 330.0, 4321.0 };
        var invariant = minutes.Select(m => DeploymentText.Age(TimeSpan.FromMinutes(m))).ToArray();
        using var scope = new CultureScope(new CultureInfo(cultureName));
        Assert.Equal(invariant, minutes.Select(m => DeploymentText.Age(TimeSpan.FromMinutes(m))).ToArray());
        Assert.Equal(["1 min", "5 min", "1 h", "5 h", "3 d"], invariant);
    }

    [Fact]
    public void AnAgeAndASpanSayTheSameCountTheSameWayWhereTheirUnitsAgree()
    {
        Assert.Equal(TimeText.Span(TimeSpan.FromMinutes(5.5)), DeploymentText.Age(TimeSpan.FromMinutes(5.5)));
        Assert.Equal(TimeText.Span(TimeSpan.FromHours(5.5)), DeploymentText.Age(TimeSpan.FromHours(5.5)));
        Assert.Equal(TimeText.Span(TimeSpan.FromDays(3.9)), DeploymentText.Age(TimeSpan.FromDays(3.9)));
    }
}
