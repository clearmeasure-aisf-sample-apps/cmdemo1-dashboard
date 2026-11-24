namespace Dashboard.Tests;

/// <summary>
/// The month a cost's numbers cover is <em>named</em> for a viewer who stands in another one: the month by its full
/// name (<c>in September</c>), with the year when that differs too (<c>in September 2025</c>), each of them carried by
/// one <c>in </c> prefix. Both names are the invariant ones - the days of a cost are UTC days and so are their months,
/// whatever culture the reader or the server carries - and they are the same two names in every sentence of the page
/// that says a month: the sum of a line, the <c>Most …</c> of the month's services and the <c>plus about …</c> of an
/// estimate.
/// <para>
/// What <see cref="CostTextEdgeCaseTests" /> and <see cref="CostReportEdgeCaseTests" /> already pin is walked past
/// here: which month is <c>this month</c> and when the year has to be said. What none of them held is the shape of the
/// name itself - that it is the whole month name and not an abbreviation or a figure, that a reader of another culture
/// is still given the English one, that the prefix is said once, and that the page's three month sentences say it
/// alike. That is what follows.
/// </para>
/// </summary>
public class CostTextMonthWordsTests
{
    /// <summary>A rendering in the middle of November 2026 (UTC), so that no answer here turns on a first or a last
    /// day of a month.</summary>
    private static readonly DateTimeOffset Now = new(2026, 11, 15, 12, 0, 0, TimeSpan.Zero);

    /// <summary>The one month of the viewer's own year the page does not name, because it is the month the rendering
    /// stands in: the year is what brings a name to every other month of it.</summary>
    [Fact]
    public void TheViewersOwnMonthIsTheOneMonthOfItsYearLeftUnnamed() =>
        Assert.Equal("this month", CostText.Month(new DateOnly(2026, 11, 14), Now));

    /// <summary>A month of the viewer's year is the month's whole name, the twelve of them spelled out: not an
    /// abbreviation, not a figure, and not the year, which the viewer does not need telling.</summary>
    /// <param name="month">The month of the file's day, 1 to 12.</param>
    /// <param name="name">That month's whole name in the invariant culture.</param>
    [Theory]
    [InlineData(1, "January")]
    [InlineData(2, "February")]
    [InlineData(3, "March")]
    [InlineData(4, "April")]
    [InlineData(5, "May")]
    [InlineData(6, "June")]
    [InlineData(7, "July")]
    [InlineData(8, "August")]
    [InlineData(9, "September")]
    [InlineData(10, "October")]
    [InlineData(12, "December")]
    public void AMonthOfTheViewersYearIsNamedInFull(int month, string name) =>
        Assert.Equal($"in {name}", CostText.Month(new DateOnly(2026, month, 14), Now));

    /// <summary>A month of another year is its whole name with that year after it, the twelve of them spelled out:
    /// the year of a past month is said although the month has ended, and the year of a month still to come is said
    /// although its name is one the viewer has heard this year.</summary>
    /// <param name="month">The month of the file's day, 1 to 12.</param>
    /// <param name="name">That month's whole name in the invariant culture.</param>
    [Theory]
    [InlineData(1, "January")]
    [InlineData(2, "February")]
    [InlineData(3, "March")]
    [InlineData(4, "April")]
    [InlineData(5, "May")]
    [InlineData(6, "June")]
    [InlineData(7, "July")]
    [InlineData(8, "August")]
    [InlineData(9, "September")]
    [InlineData(10, "October")]
    [InlineData(11, "November")]
    [InlineData(12, "December")]
    public void AMonthOfAnotherYearIsNamedWithThatYear(int month, string name) =>
        Assert.Equal($"in {name} 2025", CostText.Month(new DateOnly(2025, month, 14), Now));

    /// <summary>The name of a month of a cost is the invariant one, so a reader of another culture is not told the
    /// month in figures (<c>ru-RU</c>, <c>ja-JP</c>), in that culture's own word for it (<c>de-DE</c>
    /// <c>September</c> vs. <c>september</c>, <c>tr-TR</c> <c>Eylül</c>) or in an inflected form
    /// (<c>fi-FI</c>); the empty name is the invariant culture itself.</summary>
    /// <param name="culture">The culture the reader is in.</param>
    [Theory]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("fr-FR")]
    [InlineData("fi-FI")]
    [InlineData("tr-TR")]
    [InlineData("ru-RU")]
    [InlineData("ja-JP")]
    [InlineData("")]
    public void TheMonthWordsAreTheInvariantOnesToEveryReader(string culture)
    {
        Assert.Equal("in September", CultureScope.Says(culture, () => CostText.Month(new DateOnly(2026, 9, 14), Now)));
        Assert.Equal("in September 2025", CultureScope.Says(culture, () => CostText.Month(new DateOnly(2025, 9, 14), Now)));
        Assert.Equal("this month", CultureScope.Says(culture, () => CostText.Month(new DateOnly(2026, 11, 14), Now)));
    }

    /// <summary>The prefix of a named month is said once, and the name it carries is a name and nothing else: no
    /// second <c>in</c>, no doubled space, nothing left over at either end.</summary>
    [Fact]
    public void ANamedMonthIsCarriedByTheOnePrefixAndNothingElse()
    {
        var days = new[] { new DateOnly(2026, 1, 4), new DateOnly(2026, 12, 20), new DateOnly(2025, 12, 31), new DateOnly(2027, 6, 3) };
        foreach (var day in days)
        {
            var words = CostText.Month(day, Now);
            Assert.StartsWith("in ", words, StringComparison.Ordinal);
            var named = words["in ".Length..];
            Assert.False(named.Contains("in ", StringComparison.Ordinal));
            Assert.False(named.Contains("  ", StringComparison.Ordinal));
            Assert.Equal(named.Trim(), named);
        }
    }

    /// <summary>The month is said the one way wherever the page says it: the sum's own period, the whole line, the
    /// <c>Most …</c> of the month's services and the <c>plus about …</c> of an estimate all name it with the same
    /// words, so a reader who has seen it once knows it everywhere.</summary>
    [Fact]
    public void EverySentenceOfThePageNamesAMonthTheOneWay()
    {
        var amounts = new CostAmounts(1.52, 9.8, 11.02);
        var report = new CostReport(null, "USD", new DateOnly(2025, 9, 15), amounts, []);
        var services = new[] { new CostService("Container Apps", 6.1) };
        var estimate = new CostEstimate(0.2, new CostAmounts(null, null, 2.69));

        Assert.Equal("in September 2025", CostText.Month(report.AsOf, Now));
        Assert.Equal(
            "$1.52 on 2025-09-15 · $9.80 in 7 days · $11.02 in September 2025 · as of 2025-09-15 (UTC)",
            CostText.Line(amounts, report, Now));
        Assert.Equal("Most in September 2025: Container Apps $6.10", CostText.Services(services, report, Now));
        Assert.Equal(
            "plus about $2.69 in September 2025 of what the environments share (20 % of what all pods request)",
            CostText.Estimate(estimate, report, Now));
    }
}
