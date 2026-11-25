namespace Dashboard.Tests;

/// <summary>
/// Every sentence of <see cref="SummaryWords.Text" /> follows one singular/plural rule: a total of exactly one says
/// "node", any other total says "nodes". These tests pin that rule across all four levels - including the all-healthy
/// and empty sentences, which once spelled the noun out by hand - and pin the sentences of
/// <see cref="HealthSummary.Text" /> unchanged through the summary itself.
/// </summary>
public class SummaryWordsPluralizationTests
{
    /// <summary>The all-healthy sentence keeps its two shapes: the singular "only node" for one, the counted plural for
    /// every other total, large ones included.</summary>
    [Theory]
    [InlineData(1, "The only node is healthy")]
    [InlineData(2, "All 2 nodes healthy")]
    [InlineData(10, "All 10 nodes healthy")]
    [InlineData(1234, "All 1234 nodes healthy")]
    public void TheAllHealthySentenceTakesTheNounOfItsTotal(int total, string expected) =>
        Assert.Equal(expected, SummaryWords.Text(SummaryLevel.AllHealthy, total, total, 0));

    /// <summary>An empty topology still says the plural, since a total of zero is not one.</summary>
    [Fact]
    public void TheEmptySentenceSaysThePlural() =>
        Assert.Equal("No nodes in the topology", SummaryWords.Text(SummaryLevel.Empty, 0, 0, 0));

    /// <summary>For a total of one, no level's sentence says "nodes"; for any total above one, every level's sentence
    /// that counts says "nodes" and never the bare singular.</summary>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(7)]
    public void EveryLevelAgreesOnTheNounForTheSameTotal(int total)
    {
        string[] sentences =
        [
            SummaryWords.Text(SummaryLevel.AllHealthy, total, total, 0),
            SummaryWords.Text(SummaryLevel.Problems, total, 0, 0),
            SummaryWords.Text(SummaryLevel.Pending, total, 0, total),
        ];

        foreach (var sentence in sentences)
        {
            var words = sentence.Split(' ', ',');
            if (total == 1)
            {
                Assert.Contains("node", words);
                Assert.DoesNotContain("nodes", words);
            }
            else
            {
                Assert.Contains("nodes", words);
                Assert.DoesNotContain("node", words);
            }
        }
    }

    /// <summary>The summary's own text is unchanged for every level, read through <see cref="HealthSummary" />.</summary>
    [Theory]
    [InlineData(0, 0, 0, "No nodes in the topology")]
    [InlineData(1, 1, 0, "The only node is healthy")]
    [InlineData(3, 3, 0, "All 3 nodes healthy")]
    [InlineData(1, 0, 0, "1 of 1 node not healthy")]
    [InlineData(4, 1, 0, "3 of 4 nodes not healthy")]
    [InlineData(1, 0, 1, "Checking 1 node")]
    [InlineData(4, 2, 2, "2 of 4 nodes healthy, 2 being checked")]
    public void TheSummaryTextIsUnchanged(int total, int healthy, int pending, string expected) =>
        Assert.Equal(expected, new HealthSummary(total, healthy, pending).Text);

    /// <summary>
    /// The AllHealthy sentence's count-against-1 word choice is routed through Plural, not written by hand.
    /// Count 0 is tested via the Empty level (0 nodes is never AllHealthy).
    /// </summary>
    [Theory]
    [InlineData(SummaryLevel.Empty, 0, 0, 0, "No nodes in the topology")]
    [InlineData(SummaryLevel.AllHealthy, 1, 1, 0, "The only node is healthy")]
    [InlineData(SummaryLevel.AllHealthy, 2, 2, 0, "All 2 nodes healthy")]
    [InlineData(SummaryLevel.AllHealthy, 1000, 1000, 0, "All 1000 nodes healthy")]
    public void TheAllHealthySentenceRoutesThroughPlural(SummaryLevel level, int total, int healthy, int pending, string expected)
    {
        Assert.Equal(expected, SummaryWords.Text(level, total, healthy, pending));
    }
}
