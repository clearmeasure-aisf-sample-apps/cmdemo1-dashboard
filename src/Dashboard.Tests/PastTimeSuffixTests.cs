using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// #2150: the past-time suffix <c> ago</c> has one home, <see cref="TimeText"/>, and every "how long ago" answer
/// (<see cref="TimeText.Ago"/>, <see cref="DeploymentText.Ago"/>, <see cref="VitalsText.Uptime"/>) says it the same way.
/// </summary>
public class PastTimeSuffixTests
{
    private const string Suffix = " ago";

    private static readonly DateTimeOffset Now = new(2026, 11, 27, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void AMomentThreeMinutesBackIsThreeMinutesAgo() =>
        Assert.Equal("3 min ago", TimeText.Ago(Now - TimeSpan.FromMinutes(3), Now));

    [Fact]
    public void AMomentAheadOfTheClockIsStillJustNow() =>
        Assert.Equal("just now", TimeText.Ago(Now + TimeSpan.FromMinutes(3), Now));

    [Fact]
    public void AFinishedDeploymentFiveMinutesBackIsFiveMinutesAgo() =>
        Assert.Equal("5 min ago", DeploymentText.Ago(TimeSpan.FromMinutes(5)));

    [Fact]
    public void AYoungProcessEndsWithAgoAndASettledOneDoesNot()
    {
        Assert.EndsWith(Suffix, VitalsText.Uptime(299), StringComparison.Ordinal);
        Assert.DoesNotContain(Suffix, VitalsText.Uptime(300), StringComparison.Ordinal);
    }

    [Fact]
    public void EveryPastTimeAnswerSharesTheSameEnding()
    {
        string[] answers =
        [
            TimeText.Ago(Now - TimeSpan.FromHours(3), Now),
            DeploymentText.Ago(TimeSpan.FromHours(3)),
            VitalsText.Uptime(120),
        ];

        Assert.All(answers, answer => Assert.EndsWith(Suffix, answer, StringComparison.Ordinal));
        Assert.Equal(["3 h ago", "3 h ago", "restarted 2 min ago"], answers);
    }
}
