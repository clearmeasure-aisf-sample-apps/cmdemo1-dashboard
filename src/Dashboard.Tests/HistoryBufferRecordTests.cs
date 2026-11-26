namespace Dashboard.Tests;

/// <summary>HistoryBuffer.Record: the one home of the "no leading gap" rule both trend producers record through.</summary>
public class HistoryBufferRecordTests
{
    [Fact]
    public void GapsBeforeTheFirstReadingStoreNothing()
    {
        var samples = new HistoryBuffer<string?>(3);

        samples.Record(null);
        samples.Record(null);

        Assert.Empty(samples);
    }

    [Fact]
    public void AReadingIsAlwaysStored()
    {
        var samples = new HistoryBuffer<string?>(3);

        samples.Record(null);
        samples.Record("first");

        Assert.Equal(["first"], samples);
    }

    [Fact]
    public void AGapAfterAReadingIsStoredAsNull()
    {
        var samples = new HistoryBuffer<string?>(3);

        samples.Record("first");
        samples.Record(null);
        samples.Record(null);

        Assert.Equal(["first", null, null], samples);
    }

    [Fact]
    public void AGapStaysStoredOnceTheReadingBeforeItHasRolledOut()
    {
        var samples = new HistoryBuffer<string?>(2);

        samples.Record("first");
        samples.Record(null);
        samples.Record(null);

        Assert.Equal([null, null], samples);
        Assert.Equal(2, samples.Count);
    }

    [Fact]
    public void TheHistoryStillCapsAtTheTrendLength()
    {
        var samples = new HistoryBuffer<string?>(Trend.Length);

        for (var check = 0; check < Trend.Length + 5; check++)
        {
            samples.Record(check % 2 == 0 ? $"reading {check}" : null);
        }

        samples.Record("newest");

        Assert.Equal(Trend.Length, samples.Count);
        Assert.Equal("newest", samples.Last);
    }

    [Fact]
    public void AValueThatCannotBeNullIsAlwaysARecordedReading()
    {
        var samples = new HistoryBuffer<int>(3);

        samples.Record(0);
        samples.Record(7);

        Assert.Equal([0, 7], samples);
    }
}
