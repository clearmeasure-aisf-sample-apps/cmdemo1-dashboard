namespace Dashboard.Tests;

/// <summary>
/// <see cref="Sums" /> is the cluster's one rule for adding numbers some of which are unknown: add what is known, and
/// say nothing when nothing is known. <see cref="NamespaceGroupEdgeCaseTests" /> and
/// <see cref="ClusterTotalsEdgeCaseTests" /> reach it through a namespace and the cluster card; these call it directly.
/// </summary>
public sealed class SumsTests
{
    [Fact]
    public void NothingAtAllSumsToNothing()
    {
        Assert.Null(Sums.Of(Array.Empty<double?>()));
        Assert.Null(Sums.Of(Array.Empty<int?>()));
    }

    [Fact]
    public void OnlyUnknownsSumToNothingNotToZero()
    {
        Assert.Null(Sums.Of(new double?[] { null, null, null }));
        Assert.Null(Sums.Of(new int?[] { null, null }));
    }

    [Fact]
    public void OneZeroAmongTheUnknownsIsAKnownZero()
    {
        Assert.Equal(0d, Sums.Of([null, 0d, null]));
        Assert.Equal(0, Sums.Of(new int?[] { null, 0, null }));
    }

    [Fact]
    public void TheKnownNumbersAreAddedAndTheUnknownsSkipped()
    {
        Assert.Equal(350d, Sums.Of([100d, null, 250d]));
        Assert.Equal(7, Sums.Of(new int?[] { 3, null, 4 }));
    }

    [Fact]
    public void TwoLargestDoublesAddPastTheLargestToInfinity()
    {
        Assert.Equal(double.PositiveInfinity, Sums.Of([double.MaxValue, null, double.MaxValue]));
        Assert.Equal(double.MaxValue, Sums.Of([double.MaxValue, null]));
    }

    [Fact]
    public void CountsAreAddedInAnIntWithoutARoundTripThroughDouble()
    {
        // int.MaxValue - 1 and 1 add exactly to int.MaxValue, and the largest int stays itself.
        Assert.Equal(int.MaxValue, Sums.Of(new int?[] { int.MaxValue - 1, null, 1 }));
        Assert.Equal(int.MaxValue, Sums.Of(new int?[] { int.MaxValue }));
    }

    [Fact]
    public void CountsPastAnIntThrowRatherThanWrap()
    {
        Assert.Throws<OverflowException>(() => Sums.Of(new int?[] { int.MaxValue, 1 }));
    }

    [Fact]
    public void AddingOneAtATimeGivesTheSameSum()
    {
        double?[] numbers = [null, 0d, 2.5, null, 4d];
        double? sum = null;
        foreach (var number in numbers)
        {
            sum = Sums.Add(sum, number);
        }

        Assert.Equal(Sums.Of(numbers), sum);
        Assert.Null(Sums.Add(null, null));
        Assert.Equal(0d, Sums.Add(null, 0d));
        Assert.Equal(1d, Sums.Add(1d, null));
    }

    [Fact]
    public void AMissingSequenceIsRefused()
    {
        Assert.Throws<ArgumentNullException>(() => Sums.Of((IEnumerable<double?>)null!));
        Assert.Throws<ArgumentNullException>(() => Sums.Of((IEnumerable<int?>)null!));
    }
}
