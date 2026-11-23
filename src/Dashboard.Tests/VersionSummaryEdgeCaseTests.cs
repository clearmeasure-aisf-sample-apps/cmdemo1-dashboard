using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="VersionSummary.Text" /> says its count through <see cref="Plural" />, the one place the
/// singular/plural rule lives: only exactly one environment is singular - eleven, twenty-one and the long count
/// are plural all the same - and the count is written digit by digit with no separator, whatever the viewer's
/// culture writes between thousands.
/// </summary>
public class VersionSummaryEdgeCaseTests
{
    [Theory]
    [InlineData(1, "Versions differ in 1 environment")]
    [InlineData(2, "Versions differ in 2 environments")]
    [InlineData(11, "Versions differ in 11 environments")]
    [InlineData(21, "Versions differ in 21 environments")]
    [InlineData(1234, "Versions differ in 1234 environments")]
    public void OnlyOneEnvironmentIsSingular(int environments, string expected) =>
        Assert.Equal(expected, new VersionSummary(environments).Text);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void NoCountAboveZeroSaysNothing(int environments) =>
        Assert.Null(new VersionSummary(environments).Text);

    [Fact]
    public void TheSentenceIsPluralsPhraseForTheCount()
    {
        foreach (var environments in new[] { 1, 2, 11, 21, 1234, int.MaxValue })
        {
            Assert.Equal($"Versions differ in {Plural.Of(environments, "environment")}", new VersionSummary(environments).Text);
        }
    }

    [Fact]
    public void ALongCountHasNoSeparatorInAnotherCulture()
    {
        var culture = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
            Assert.Equal("Versions differ in 1234 environments", new VersionSummary(1234).Text);
            Assert.Equal("Versions differ in 2147483647 environments", new VersionSummary(int.MaxValue).Text);
            Assert.Equal("Versions differ in 1 environment", new VersionSummary(1).Text);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = culture;
        }
    }
}
