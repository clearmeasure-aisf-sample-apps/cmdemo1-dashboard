namespace Dashboard.Tests;

/// <summary>
/// The failover line as its parts: <see cref="DeliveryText.Failover" /> collects the environment and the age in a
/// <see cref="CardParts" /> collector - a part the file left out comes in as null and is left out, the rest are
/// joined with <c>", "</c> - and hangs the seconds clause after <c>": "</c> when the joined parts are not empty.
/// Characterization of the words and the order, part for part, the way the card says them today.
/// </summary>
public class DeliveryTextFailoverPartsTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Every combination of the parts the file may give: none at all is the empty line, the environment and the age
    /// stand with a comma between them, and the seconds clause follows a colon after whatever stands before it - or
    /// stands alone when nothing does.
    /// </summary>
    [Theory]
    [InlineData(null, null, null, "")]
    [InlineData("uat", null, null, "uat")]
    [InlineData(null, 17d, null, "17 h ago")]
    [InlineData(null, null, 44d, "the standby answered after 44 s")]
    [InlineData("uat", 17d, null, "uat, 17 h ago")]
    [InlineData("uat", null, 44d, "uat: the standby answered after 44 s")]
    [InlineData(null, 17d, 44d, "17 h ago: the standby answered after 44 s")]
    [InlineData("uat", 17d, 44d, "uat, 17 h ago: the standby answered after 44 s")]
    public void EveryCombinationOfThePartsSaysOnlyWhatTheFileGave(string? environment, double? hoursAgo, double? seconds, string expected)
    {
        var test = new FailoverTest(environment, hoursAgo is { } h ? Now.AddHours(-h) : null, seconds);

        Assert.Equal(expected, DeliveryText.Failover(test, Now));
    }

    /// <summary>
    /// An empty environment name joins to the empty text: the seconds clause then stands alone with no colon in
    /// front of it, and the empty name with no seconds says the empty line.
    /// </summary>
    [Fact]
    public void TheSecondsClauseStandsAloneWhenTheJoinedPartsAreEmpty()
    {
        Assert.Equal("the standby answered after 44 s", DeliveryText.Failover(new FailoverTest(string.Empty, null, 44), Now));
        Assert.Equal(string.Empty, DeliveryText.Failover(new FailoverTest(string.Empty, null, null), Now));
    }

    /// <summary>
    /// The age is counted from the moment the card is asked, in the words <see cref="TimeText.Ago" /> says: a test of
    /// the very moment is <c>just now</c>, and the environment and the age stand with the comma the collector joins
    /// them with, the seconds clause after the colon.
    /// </summary>
    [Theory]
    [InlineData(0d, "uat, just now: the standby answered after 44 s")]
    [InlineData(2d, "uat, 2 h ago: the standby answered after 44 s")]
    [InlineData(48d, "uat, 2 d ago: the standby answered after 44 s")]
    public void TheAgeStandsWhereTheCollectorPutIt(double hoursAgo, string expected) =>
        Assert.Equal(expected, DeliveryText.Failover(new FailoverTest("uat", Now.AddHours(-hoursAgo), 44), Now));
}
