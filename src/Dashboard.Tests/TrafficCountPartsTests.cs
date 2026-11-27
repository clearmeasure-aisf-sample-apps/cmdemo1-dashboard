namespace Dashboard.Tests;

/// <summary>
/// The traffic count line as its parts: <see cref="TrafficCount.Counts" /> says sent and answered always, and adds
/// failed, the no-answer count and the requests on their way only when they are above zero, in that order, joined
/// with <c>", "</c> by the one collector <see cref="CardParts" /> says the joining rule once. Characterization of the
/// words and the order, part for part, the way the panel says them today.
/// </summary>
public class TrafficCountPartsTests
{
    private static TrafficCount Reading(int sent, int answered, int failed, int unanswered, bool sending) =>
        new(sent, answered, failed, unanswered, sending);

    /// <summary>Before the first press: nothing sent, nothing running - the two always-said parts alone.</summary>
    [Fact]
    public void NothingSentIsTwoPartsAlone()
    {
        Assert.Equal("0 sent, 0 answered", TrafficCount.None.Counts);
        Assert.Equal("0 sent, 0 answered", TrafficCount.Started.Counts);
    }

    /// <summary>
    /// Every optional part, alone and all together: sent and answered stand first, then failed, then the no-answer
    /// count in the words <see cref="ProbeDefaults.NoAnswerWithin" /> says them, then the requests on their way with
    /// the singular or plural word <see cref="Plural.Agreement" /> gives - and the sending flag changes nothing of
    /// the count's words.
    /// </summary>
    [Theory]
    [InlineData(119, 99, 0, 0, false, "119 sent, 99 answered, 20 on their way")]
    [InlineData(119, 99, 0, 20, false, "119 sent, 99 answered, 20 with no answer within 10 s")]
    [InlineData(119, 99, 5, 0, false, "119 sent, 99 answered, 5 failed, 15 on their way")]
    [InlineData(119, 99, 5, 20, false, "119 sent, 99 answered, 5 failed, 20 with no answer within 10 s")]
    [InlineData(119, 80, 5, 10, false, "119 sent, 80 answered, 5 failed, 10 with no answer within 10 s, 24 on their way")]
    [InlineData(119, 80, 5, 10, true, "119 sent, 80 answered, 5 failed, 10 with no answer within 10 s, 24 on their way")]
    [InlineData(119, 119, 0, 0, false, "119 sent, 119 answered")]
    [InlineData(119, 109, 10, 0, false, "119 sent, 109 answered, 10 failed")]
    public void EachOptionalPartStandsWhereTheCollectorPutIt(int sent, int answered, int failed, int unanswered, bool sending, string expected) =>
        Assert.Equal(expected, Reading(sent, answered, failed, unanswered, sending).Counts);

    /// <summary>
    /// The on-their-way word agrees with its count: one request says <c>1 on its way</c>, any other count says
    /// <c>on their way</c> - and zero requests on their way says nothing of them at all.
    /// </summary>
    [Theory]
    [InlineData(5, 4, 0, 0, "5 sent, 4 answered, 1 on its way")]
    [InlineData(5, 3, 0, 0, "5 sent, 3 answered, 2 on their way")]
    [InlineData(5, 5, 0, 0, "5 sent, 5 answered")]
    public void TheOnTheirWayPartAgreesWithItsCount(int sent, int answered, int failed, int unanswered, string expected) =>
        Assert.Equal(expected, Reading(sent, answered, failed, unanswered, sending: false).Counts);
}
