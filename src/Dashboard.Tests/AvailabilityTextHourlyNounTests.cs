namespace Dashboard.Tests;

/// <summary>
/// The noun of the hourly part of <see cref="AvailabilityText" />: the word that follows <c>hourly</c> is
/// <see cref="Plural.Noun(int, string, string?)" />'s and not a singular/plural pair written again inside the
/// sentence (#1593). One report of a window is <c>1 of 1 hourly check</c> and every other count - two, the
/// twenty-four of a day, a count past a thousand, the end of <see cref="int" /> - is <c>… hourly checks</c>.
/// <para>
/// What was already pinned elsewhere and is not repeated as a claim here: <see cref="AvailabilityTests" /> pins the
/// whole line of a day and a week read from a file, and <see cref="AvailabilityTextEdgeCaseTests" /> and
/// <see cref="AvailabilityTextWindowWordsTests" /> pin the two windows, the short week and the percents. Added here
/// because #1593 moved this one word to <see cref="Plural" />: the words are pinned again at the counts the noun
/// turns on, and the word itself is held to what the rule says for the count, so the sentence and the rule cannot
/// drift apart.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every moment is one the test brings.
/// </para>
/// </summary>
public class AvailabilityTextHourlyNounTests
{
    /// <summary>The moment the page is asked, and every part below is counted from.</summary>
    private static readonly DateTimeOffset Now = new(2026, 11, 24, 12, 0, 0, TimeSpan.Zero);

    /// <summary>The day counted alone, which is the one place the hourly noun is said.</summary>
    private static AvailabilityPart Day(HealthWindow window) =>
        AvailabilityText.Parts(new HealthReports(window, null, null), Now).Single();

    /// <summary>The one report is singular and every other count plural, word for word, in the part and in the
    /// line - the counts unchanged from before the noun came to <see cref="Plural" />.</summary>
    [Theory]
    [InlineData(1, 1, " hourly check (100 %) in 24 hours", "Healthy in 1 of 1 hourly check (100 %) in 24 hours")]
    [InlineData(2, 2, " hourly checks (100 %) in 24 hours", "Healthy in 2 of 2 hourly checks (100 %) in 24 hours")]
    [InlineData(24, 23, " hourly checks (95.8 %) in 24 hours", "Healthy in 23 of 24 hourly checks (95.8 %) in 24 hours")]
    [InlineData(1234, 1233, " hourly checks (99.9 %) in 24 hours", "Healthy in 1233 of 1234 hourly checks (99.9 %) in 24 hours")]
    public void TheHourlyPartIsSingularForTheOneReportAndPluralForEveryOther(
        int reports,
        int healthy,
        string after,
        string line)
    {
        var window = new HealthWindow(reports, healthy);

        Assert.Equal(after, Day(window).After);
        Assert.Equal(line, AvailabilityText.Line(new HealthReports(window, null, null), Now));
    }

    /// <summary>The word after <c>hourly</c> is the noun <see cref="Plural.Noun(int, string, string?)" /> gives that
    /// very count, at the one that turns it singular, past it, past a thousand and at the end of
    /// <see cref="int" />: the sentence holds no second copy of the rule and so cannot part from it.</summary>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(24)]
    [InlineData(1234)]
    [InlineData(int.MaxValue)]
    public void TheWordAfterHourlyIsTheNounPluralGivesThatCount(int reports)
    {
        var window = new HealthWindow(reports, reports);

        Assert.Equal(
            $" hourly {Plural.Noun(reports, "check")} ({AvailabilityText.Percent(window)}) in 24 hours",
            Day(window).After);
    }

    /// <summary>The short seven-day part carries no noun at all, so the only noun the refactor moved is the
    /// window's own: a day of one report and the short week after it still say exactly the words they said
    /// before, the week's tooltip included.</summary>
    [Fact]
    public void TheWeekAfterTheDayStillCarriesNoNoun()
    {
        var parts = AvailabilityText.Parts(new HealthReports(new HealthWindow(1, 1), new HealthWindow(2, 2), null), Now);

        Assert.Equal(" hourly check (100 %) in 24 hours", parts[0].After);
        Assert.Equal(
            new AvailabilityPart(string.Empty, "2 of 2", " in 7 days", "100 % of the hourly checks of the last 7 days"),
            parts[1]);
    }

    /// <summary>The noun and the digits about it are the dashboard's and not the viewer's: a reader of German is
    /// given the singular for the one report, the plural for the thousand and no thousands separator, as
    /// <see cref="Plural" /> says them.</summary>
    [Fact]
    public void TheHourlyNounAndItsDigitsDoNotChangeWithTheViewersCulture() =>
        ViewerCulture.German(() =>
        {
            Assert.Equal(
                "Healthy in 1 of 1 hourly check (100 %) in 24 hours",
                AvailabilityText.Line(new HealthReports(new HealthWindow(1, 1), null, null), Now));
            Assert.Equal(
                "Healthy in 1233 of 1234 hourly checks (99.9 %) in 24 hours",
                AvailabilityText.Line(new HealthReports(new HealthWindow(1234, 1233), null, null), Now));
        });

    /// <summary>#1593's acceptance criterion for this site, as a check: the file that says the hourly line asks
    /// <see cref="Plural.Noun(int, string, string?)" /> for the word after <c>hourly</c>, and holds no
    /// singular/plural pair of its own to drift.</summary>
    [Fact]
    public void TheFileThatSaysTheHourlyLineAsksPluralForTheWord()
    {
        var source = File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("Health", "DeliveryText.cs")));

        Assert.Contains("Plural.Noun(window.Reports, \"check\")", source, StringComparison.Ordinal);
        Assert.DoesNotContain("? \"check\"", source, StringComparison.Ordinal);
        Assert.DoesNotContain("\"checks\"", source, StringComparison.Ordinal);
    }
}
