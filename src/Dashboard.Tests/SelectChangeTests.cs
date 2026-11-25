using Microsoft.AspNetCore.Components;

namespace Dashboard.Tests;

/// <summary>
/// The one read of a select's change (#1816): <see cref="SelectChange.TryRead{T}"/> parses the text the browser sent
/// the way the caller names and keeps it only when the control offers it. The page's interval select reads with
/// <see cref="SelectChange.Whole"/> and its choices (10, 30, 60), the probe select with <c>Enum.TryParse</c> and
/// <c>Enum.IsDefined</c>; both keep exactly the values they offer and ignore any other.
/// </summary>
public class SelectChangeTests
{
    private static readonly int[] IntervalChoices = [10, 30, 60];

    private static ChangeEventArgs Change(object? value) => new() { Value = value };

    // ---------------------------------------------------------------- the interval select

    [Theory]
    [InlineData("10", 10)]
    [InlineData("30", 30)]
    [InlineData("60", 60)]
    public void TheIntervalReadKeepsEveryChoiceItOffers(string text, int expected)
    {
        Assert.True(SelectChange.TryRead<int>(Change(text), SelectChange.Whole, IntervalChoices.Contains, out var seconds));
        Assert.Equal(expected, seconds);
    }

    [Theory]
    [InlineData("15")]
    [InlineData("0")]
    [InlineData("-10")]
    [InlineData("thirty")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("1e1")]
    [InlineData("10.0")]
    public void TheIntervalReadIgnoresAValueNoChoiceOffers(string? text)
    {
        Assert.False(SelectChange.TryRead<int>(Change(text), SelectChange.Whole, IntervalChoices.Contains, out _));
    }

    [Fact]
    public void TheIntervalReadTakesTheValueAsTheBrowserSendsItNotOnlyAsAString()
    {
        Assert.True(SelectChange.TryRead<int>(Change(60), SelectChange.Whole, IntervalChoices.Contains, out var seconds));
        Assert.Equal(60, seconds);
    }

    [Fact]
    public void TheWholeNumberIsReadTheInvariantWayWhateverTheCulture()
    {
        using var scope = new CultureScope(System.Globalization.CultureInfo.GetCultureInfo("de-DE"));

        Assert.True(SelectChange.Whole("30", out var thirty));
        Assert.Equal(30, thirty);
        Assert.False(SelectChange.Whole("30,0", out _));
    }

    // ---------------------------------------------------------------- the probe select

    [Theory]
    [InlineData("Health", ProbeKind.Health)]
    [InlineData("Liveness", ProbeKind.Liveness)]
    public void TheProbeReadKeepsEveryProbeItOffers(string text, ProbeKind expected)
    {
        Assert.True(SelectChange.TryRead<ProbeKind>(Change(text), Enum.TryParse, Enum.IsDefined, out var probe));
        Assert.Equal(expected, probe);
    }

    [Theory]
    [InlineData("Readiness")]
    [InlineData("health")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("5")]
    public void TheProbeReadIgnoresAValueNoOptionOffers(string? text)
    {
        Assert.False(SelectChange.TryRead<ProbeKind>(Change(text), Enum.TryParse, Enum.IsDefined, out _));
    }

    // ---------------------------------------------------------------- the page reads through it

    [Fact]
    public void BothHandlersOfThePageDecodeTheirChangeThroughTheOneRead()
    {
        var text = MarkupGuards.AppText();

        Assert.Contains(
            "SelectChange.TryRead<int>(change, SelectChange.Whole, IntervalChoices.Contains, out var seconds)",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            "SelectChange.TryRead<ProbeKind>(change, Enum.TryParse, Enum.IsDefined, out var probe)",
            text,
            StringComparison.Ordinal);
        Assert.DoesNotContain("change.Value?.ToString()", text, StringComparison.Ordinal);
    }
}
