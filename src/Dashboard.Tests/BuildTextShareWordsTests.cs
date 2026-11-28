namespace Dashboard.Tests;

/// <summary>
/// The phrase for one share of the "Code" card's language bar: <see cref="BuildText.ShareWords" /> says the share's
/// name and its percent, <c>C# 73 %</c>, and <see cref="BuildText.Describe" /> joins the shares of the bar with the
/// bar's one list separator.
/// </summary>
public sealed class BuildTextShareWordsTests
{
    [Fact]
    public void ShareWordsSaysTheNameAndThePercent() =>
        Assert.Equal("C# 73 %", BuildText.ShareWords(new LanguageShare("C#", 61234, 73.4, 1)));

    [Fact]
    public void ShareWordsSaysLessThanOnePercentForASmallShare() =>
        Assert.Equal("YAML <1 %", BuildText.ShareWords(new LanguageShare("YAML", 3, 0.4, 1)));

    [Fact]
    public void ShareWordsUsesTheInvariantCultureUnderAGermanViewer() =>
        ViewerCulture.German(() => Assert.Equal("C# 73 %", BuildText.ShareWords(new LanguageShare("C#", 61234, 73.4, 1))));

    [Fact]
    public void ShareWordsRefusesANullShare() =>
        Assert.Throws<ArgumentNullException>(() => BuildText.ShareWords(null!));

    [Fact]
    public void DescribeJoinsTheSharesWithTheListSeparator()
    {
        LanguageShare[] shares = [
            new("C#", 61234, 73.4, 1),
            new("Razor", 9000, 11.2, 2),
            new("YAML", 3, 0.4, 3),
            new("Other", 1976, 15, 0),
        ];

        Assert.Equal(string.Join(", ", shares.Select(BuildText.ShareWords)), BuildText.Describe(shares));
    }
}
