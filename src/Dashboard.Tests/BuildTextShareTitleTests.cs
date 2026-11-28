namespace Dashboard.Tests;

/// <summary>
/// The hover text and accessible name of the "Code" card's language bar: <see cref="BuildText.LinesTitle" /> says a
/// share's line count in the same unit word <see cref="BuildText.Size" /> says, <see cref="BuildText.ShareTitle" />
/// says a share's name, line count and percent, and <see cref="BuildText.BarLabel" /> says the bar's accessible name
/// - the lead word plus <see cref="BuildText.Describe" />.
/// </summary>
public sealed class BuildTextShareTitleTests
{
    [Fact]
    public void LinesTitleSaysTheCountAndTheUnitWord() =>
        Assert.Equal("1,234 lines", BuildText.LinesTitle(new LanguageShare("C#", 1234, 73, 1)));

    [Fact]
    public void ShareTitleSaysNameLinesAndPercent() =>
        Assert.Equal("C#: 1,234 lines, 73 %", BuildText.ShareTitle(new LanguageShare("C#", 1234, 73, 1)));

    [Fact]
    public void ShareTitleSaysLessThanOnePercentForASmallShare() =>
        Assert.Equal("YAML: 3 lines, <1 %", BuildText.ShareTitle(new LanguageShare("YAML", 3, 0.4, 1)));

    [Fact]
    public void BarLabelSaysTheLeadAndTheBarInWords()
    {
        LanguageShare[] shares = [
            new("C#", 61234, 73, 1),
            new("Razor", 9000, 11, 2),
            new("SQL", 5000, 6, 3),
            new("JavaScript", 4000, 5, 4),
            new("CSS", 3000, 4, 5),
            new("Other", 1976, 2, 0),
        ];

        Assert.Equal(
            "Lines of code by language: C# 73 %, Razor 11 %, SQL 6 %, JavaScript 5 %, CSS 4 %, Other 2 %",
            BuildText.BarLabel(shares));
    }

    [Fact]
    public void BarLabelSaysOnlyTheLeadWhenThereAreNoShares() =>
        Assert.Equal("Lines of code by language: ", BuildText.BarLabel([]));

    [Fact]
    public void LinesTitleRefusesANullShare() =>
        Assert.Throws<ArgumentNullException>(() => BuildText.LinesTitle(null!));

    [Fact]
    public void ShareTitleRefusesANullShare() =>
        Assert.Throws<ArgumentNullException>(() => BuildText.ShareTitle(null!));

    [Fact]
    public void BarLabelRefusesNullShares() =>
        Assert.Throws<ArgumentNullException>(() => BuildText.BarLabel(null!));
}
