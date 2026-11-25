using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>
/// The array form of a traffic reading is decoded by name, the order written once in
/// <see cref="Dashboard.Health.TrafficCount.From" /> (#1773): each of the five positions into the member named for it,
/// so a number that arrived in its neighbour's place reads as its neighbour - a failed one as failed, not as one
/// given up with no answer in time, the difference between "the app is broken" and "the app is slow" on a live page.
/// The object leg of the reading stands in <see cref="TrafficCountNamedReadingTests" />; these tests stand where the
/// five positions cross into the record.
/// </summary>
public class TrafficCountFromNamedDecodingTests
{
    /// <summary>
    /// A reading in which all five numbers are different, so a swap of any two positions would be caught: each
    /// position of the array reads into the member named for it, and the sending flag reads its own position.
    /// </summary>
    [Fact]
    public void EachPositionReadsIntoTheMemberNamedForIt()
    {
        var count = TrafficCount.From([119, 97, 13, 6, 1]);

        Assert.Equal(119, count.Sent);
        Assert.Equal(97, count.Answered);
        Assert.Equal(13, count.Failed);
        Assert.Equal(6, count.Unanswered);
        Assert.True(count.Sending);
    }

    /// <summary>
    /// The swap the reading is kept from: three in the third place says failed, three in the fourth says given up
    /// with no answer in time - the same number, the sentence named for the member it took.
    /// </summary>
    [Fact]
    public void AFailedNumberDoesNotReadAsUnansweredAndNeitherWayAbout()
    {
        var broken = TrafficCount.From([10, 7, 3, 0, 0]);
        var slow = TrafficCount.From([10, 7, 0, 3, 0]);

        Assert.Equal("10 sent, 7 answered, 3 failed", broken.Counts);
        Assert.Equal("10 sent, 7 answered, 3 with no answer within 10 s", slow.Counts);
    }

    /// <summary>The sending flag is the fifth position and only the fifth: nonzero sends, zero does not.</summary>
    [Theory]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(0, false)]
    public void TheSendingFlagReadsTheFifthPositionAndOnlyIt(int sending, bool expected)
    {
        var count = TrafficCount.From([1, 1, 0, 0, sending]);

        Assert.Equal(expected, count.Sending);
    }

    /// <summary>
    /// The order is written once, in the decode itself: each position named for the member it fills, so a reversion
    /// to five bare positions - the order held only by a comment again - is caught here.
    /// </summary>
    [Fact]
    public void TheDecodeNamesEachPosition()
    {
        var source = File.ReadAllText(MarkupGuards.SourcePath("Health/TrafficCount.cs"));

        Assert.Equal(1, MarkupGuards.CountOccurrences(source, "Sent: status[0]"));
        Assert.Equal(1, MarkupGuards.CountOccurrences(source, "Answered: status[1]"));
        Assert.Equal(1, MarkupGuards.CountOccurrences(source, "Failed: status[2]"));
        Assert.Equal(1, MarkupGuards.CountOccurrences(source, "Unanswered: status[3]"));
        Assert.Equal(1, MarkupGuards.CountOccurrences(source, "Sending: status[4] != 0"));
    }

    /// <summary>
    /// The array the decode takes and the object the browser answers are the same reading: the named decode of the
    /// five positions and the named binding of the object each bind the same number to the same member, under the
    /// web JSON defaults the interop binder reads with.
    /// </summary>
    [Fact]
    public void TheArrayReadingIsTheSameReadingAsTheObjectOne()
    {
        var fromArray = TrafficCount.From([119, 105, 3, 6, 1]);
        var fromObject = JsonSerializer.Deserialize<TrafficCount>(
            """{ "sent": 119, "answered": 105, "failed": 3, "unanswered": 6, "sending": true }""",
            JsonSerializerOptions.Web);

        Assert.Equal(fromArray, fromObject);
    }
}
