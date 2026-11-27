using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// The CRAP marking rule on <see cref="CrapFacts" /> itself: <see cref="CrapFacts.IsOver" /> holds the one copy of
/// the rule the card marks on - the count decides when the build gives one, and the worst score against the
/// threshold when it does not - and <see cref="BuildText.CrapIsOver" /> only null-checks and delegates to it, so the
/// two cannot drift apart.
/// </summary>
public class CrapFactsIsOverTests
{
    /// <summary>Marking a method as over the threshold is decided by the count when the build gives one - so the
    /// worst score of the fleet standing above the threshold does not overrule <c>none over 30</c>, and a count
    /// below zero is no count over it either - and by the worst score against the threshold when it does not. That
    /// comparison holds only past the threshold: a worst score exactly on it is not over it, one tenth past it is,
    /// and the two numbers that are not there are never more than each other. The rule is asked of the record
    /// itself, not of the words of the card.</summary>
    [Theory]
    [InlineData(30d, 30d, null, false)]
    [InlineData(30.1d, 30d, null, true)]
    [InlineData(29.9d, 30d, null, false)]
    [InlineData(41d, 30d, 0, false)]
    [InlineData(41d, 30d, -3, false)]
    [InlineData(41d, 30d, 1, true)]
    [InlineData(0d, 0d, 0, false)]
    [InlineData(null, null, null, false)]
    [InlineData(null, 30d, null, false)]
    [InlineData(41d, null, null, false)]
    [InlineData(-1d, -2d, null, true)]
    public void TheRecordSaysWhenItIsOverTheThreshold(double? max, double? threshold, int? over, bool expected) =>
        Assert.Equal(expected, new CrapFacts(max, threshold, over).IsOver());

    /// <summary>The words of the card and the record itself answer the one question the same way, row by row: the
    /// card's <see cref="BuildText.CrapIsOver" /> delegates to <see cref="CrapFacts.IsOver" />, so the two cannot
    /// drift apart.</summary>
    [Theory]
    [InlineData(30d, 30d, null, false)]
    [InlineData(30.1d, 30d, null, true)]
    [InlineData(29.9d, 30d, null, false)]
    [InlineData(41d, 30d, 0, false)]
    [InlineData(41d, 30d, -3, false)]
    [InlineData(41d, 30d, 1, true)]
    [InlineData(0d, 0d, 0, false)]
    [InlineData(null, null, null, false)]
    [InlineData(null, 30d, null, false)]
    [InlineData(41d, null, null, false)]
    [InlineData(-1d, -2d, null, true)]
    public void TheCardAndTheRecordSayTheSameThing(double? max, double? threshold, int? over, bool expected)
    {
        var facts = new CrapFacts(max, threshold, over);

        Assert.Equal(expected, facts.IsOver());
        Assert.Equal(facts.IsOver(), BuildText.CrapIsOver(facts));
    }

    /// <summary>The sample build's own numbers: a worst score of <c>28.5</c> against a threshold of <c>30</c> with
    /// nothing over it is not marked, and a worst of <c>41</c> with three methods over <c>30</c> is.</summary>
    [Fact]
    public void TheSampleNumbersMarkAndDoNotMark()
    {
        Assert.False(new CrapFacts(28.5, 30, 0).IsOver());
        Assert.True(new CrapFacts(41, 30, 3).IsOver());
    }
}
