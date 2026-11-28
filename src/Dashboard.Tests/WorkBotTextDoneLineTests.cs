namespace Dashboard.Tests;

/// <summary>
/// The done-only line <see cref="WorkBotText.DoneLine" /> says of a throughput row: the agent's name, the count, and
/// the one word <c>done</c>, written in the invariant culture no matter who is looking.
/// </summary>
public class WorkBotTextDoneLineTests
{
    [Fact]
    public void OneDoneIsSaidWithTheSingularWord() =>
        Assert.Equal("alpha: 1 done", WorkBotText.DoneLine("alpha", 1));

    [Fact]
    public void ZeroDoneIsSaidWithTheSameWord() =>
        Assert.Equal("alpha: 0 done", WorkBotText.DoneLine("alpha", 0));

    [Fact]
    public void ALargeCountHasNoGroupSeparator() =>
        Assert.Equal("alpha: 12345 done", WorkBotText.DoneLine("alpha", 12345));

    [Fact]
    public void TheLineIsTheSameUnderANonInvariantCulture()
    {
        var line = CultureScope.Says("de-DE", () => WorkBotText.DoneLine("alpha", 42));

        Assert.Equal("alpha: 42 done", line);
    }

    [Fact]
    public void ThroughputLineForADoneOnlyRowEqualsDoneLine()
    {
        var row = new WorkThroughput("beta", null, null, 7);

        Assert.Equal(WorkBotText.DoneLine("beta", 7), WorkBotText.ThroughputLine(row));
    }
}
