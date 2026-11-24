using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="CardParts" /> is the one collector behind every line of the "Code" card that lists what a build
/// measured: a part not measured comes in as null and is left out, the rest are joined with <c>", "</c>, and an aside
/// such as <c>(5,210 methods)</c> follows after a space - or stands alone when nothing came before it.
/// </summary>
public class CardPartsTests
{
    [Fact]
    public void NoPartsJoinToNothing()
    {
        var parts = new CardParts();

        Assert.Equal(0, parts.Count);
        Assert.Equal(string.Empty, parts.Join());
        Assert.Equal(string.Empty, parts.Join(null));
    }

    [Fact]
    public void ANullPartIsLeftOut()
    {
        var parts = new CardParts().Add(null).Add("worst 34").Add(null);

        Assert.Equal(1, parts.Count);
        Assert.Equal("worst 34", parts.Join());
    }

    [Fact]
    public void PartsAreJoinedWithACommaInTheOrderAdded()
    {
        var parts = new CardParts().Add("average 1.9").Add(null).Add("worst 34").Add("third");

        Assert.Equal(3, parts.Count);
        Assert.Equal("average 1.9, worst 34, third", parts.Join());
    }

    [Fact]
    public void AddReturnsTheSameCollector()
    {
        var parts = new CardParts();

        Assert.Same(parts, parts.Add("one"));
        Assert.Same(parts, parts.Add(null));
    }

    [Fact]
    public void AnAsideFollowsThePartsAfterASpace() =>
        Assert.Equal("average 1.9, worst 34 (5,210 methods)", new CardParts().Add("average 1.9").Add("worst 34").Join("(5,210 methods)"));

    [Fact]
    public void AnAsideWithoutPartsStandsAloneWithNoLeadingSpace() =>
        Assert.Equal("(5,210 methods)", new CardParts().Add(null).Join("(5,210 methods)"));

    [Fact]
    public void NoAsideIsThePartsAlone() =>
        Assert.Equal("worst 34", new CardParts().Add("worst 34").Join(null));

    [Fact]
    public void AnEmptyPartIsStillAPart()
    {
        var parts = new CardParts().Add(string.Empty).Add("worst 34");

        Assert.Equal(2, parts.Count);
        Assert.Equal(", worst 34", parts.Join());
    }

    // ----- The four card lines built on it keep their words

    [Theory]
    [InlineData(null, null, null, "0: ")]
    [InlineData(12, null, null, "12 unit")]
    [InlineData(null, 3, null, "3 integration")]
    [InlineData(null, null, 1, "1 acceptance")]
    [InlineData(1009, 240, 168, "1,417: 1,009 unit, 240 integration, 168 acceptance")]
    [InlineData(5, null, 2, "7: 5 unit, 2 acceptance")]
    public void TestsSaysOnlyTheKindsCounted(int? unit, int? integration, int? acceptance, string expected) =>
        Assert.Equal(expected, BuildText.Tests(new TestCounts(unit, integration, acceptance)));

    [Theory]
    [InlineData(null, null, "")]
    [InlineData(81.2d, null, "81.2 % of lines")]
    [InlineData(null, 70.1d, "70.1 % of branches")]
    [InlineData(81.2d, 70.1d, "81.2 % of lines, 70.1 % of branches")]
    public void CoverageSaysOnlyThePercentsMeasured(double? lines, double? branches, string expected) =>
        Assert.Equal(expected, BuildText.Coverage(new CoverageFacts(lines, branches)));

    [Theory]
    [InlineData(null, null, null, "")]
    [InlineData(null, null, 5210, "(5,210 methods)")]
    [InlineData(1.9d, null, null, "average 1.9")]
    [InlineData(null, 34, 5210, "worst 34 (5,210 methods)")]
    [InlineData(1.9d, 34, 5210, "average 1.9, worst 34 (5,210 methods)")]
    public void ComplexitySaysOnlyWhatWasMeasured(double? average, int? max, int? methods, string expected) =>
        Assert.Equal(expected, BuildText.Complexity(new ComplexityFacts(average, max, methods)));

    [Theory]
    [InlineData(null, null, null, "")]
    [InlineData(28.5d, null, null, "worst 28.5")]
    [InlineData(null, 30d, null, "threshold 30")]
    [InlineData(28.5d, 30d, null, "worst 28.5, threshold 30")]
    [InlineData(28.5d, 30d, 0, "worst 28.5, none over 30")]
    [InlineData(41d, 30d, 1, "worst 41, 1 method over 30")]
    [InlineData(41d, 30d, 3, "worst 41, 3 methods over 30")]
    [InlineData(null, null, 3, "3 methods over the threshold")]
    [InlineData(null, null, 0, "none over the threshold")]
    public void CrapSaysOnlyWhatWasMeasured(double? max, double? threshold, int? over, string expected) =>
        Assert.Equal(expected, BuildText.Crap(new CrapFacts(max, threshold, over)));
}
