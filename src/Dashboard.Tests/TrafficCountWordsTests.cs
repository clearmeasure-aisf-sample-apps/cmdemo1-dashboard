namespace Dashboard.Tests;

/// <summary>
/// The words of the traffic panel that the other tests leave unnamed: the singular agreement the panel waits for, the
/// failed count that is not zero, and the refusal a reading of the wrong length hears - the count said in words.
/// </summary>
public class TrafficCountWordsTests
{
    private static TrafficCount Reading(int sent, int answered, int failed, int unanswered, bool sending) =>
        new(sent, answered, failed, unanswered, sending);

    /// <summary>
    /// A run that is over with exactly one request still in flight waits for the singular: the panel says
    /// <c>1 on its way; waiting for its answer.</c> - not <c>their answers</c>.
    /// </summary>
    [Fact]
    public void AReadingWithOneRequestOnItsWayWaitsForItsAnswer()
    {
        var count = Reading(5, 4, 0, 0, sending: false);

        Assert.Equal("5 sent, 4 answered, 1 on its way; waiting for its answer.", count.Status(0));
    }

    /// <summary>
    /// The same panel with every other number of requests in flight waits for the plural: <c>their answers</c>.
    /// </summary>
    [Theory]
    [InlineData(5, 3)]
    [InlineData(5, 0)]
    public void AReadingWithOtherCountsOnTheirWayWaitsForTheirAnswers(int sent, int answered)
    {
        var count = Reading(sent, answered, 0, 0, sending: false);

        Assert.EndsWith("; waiting for their answers.", count.Status(0));
    }

    /// <summary>
    /// A failed count that is not zero stands in the count line as <c>N failed</c>, after the answered. Every request
    /// is accounted for (none on its way), so the line says only sent, answered and failed.
    /// </summary>
    [Theory]
    [InlineData(119, 118, 1, "119 sent, 118 answered, 1 failed")]
    [InlineData(119, 112, 7, "119 sent, 112 answered, 7 failed")]
    [InlineData(119, 0, 119, "119 sent, 0 answered, 119 failed")]
    public void AFailedCountThatIsNotZeroStandsInTheCounts(int sent, int answered, int failed, string expected)
    {
        var count = Reading(sent, answered, failed, 0, sending: false);

        Assert.Equal(expected, count.Counts);
    }

    /// <summary>
    /// A reading that is not five numbers is refused with the count said in words, the number the reading actually
    /// carried in digits: <c>A reading of the traffic has five numbers, not N.</c> The argument name is
    /// <c>status</c>, the parameter the decode takes.
    /// </summary>
    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    public void AReadingOfTheWrongLengthIsRefusedWithTheCountInWords(int length)
    {
        var status = new int[length];

        var ex = Assert.Throws<ArgumentException>(() => TrafficCount.From(status));

        Assert.StartsWith("A reading of the traffic has five numbers, not", ex.Message);
        Assert.EndsWith($". (Parameter '{nameof(status)}')", ex.Message);
        Assert.Contains($"five numbers, not {length}.", ex.Message);
        Assert.Equal(nameof(status), ex.ParamName);
    }
}
