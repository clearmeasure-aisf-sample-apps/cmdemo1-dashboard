namespace Dashboard.Tests;

/// <summary>
/// The tile says an uptime in words (<see cref="VitalsText.Uptime" />) and marks it as a restart by a flag
/// (<see cref="ProcessVitals.RestartedRecently" />). Both ask the one rule against
/// <see cref="ProcessVitals.RecentRestart" />, which is internal to an app that declares no <c>InternalsVisibleTo</c>,
/// so the rule is pinned here through the two public members: over every uptime below, the words say "restarted"
/// exactly when the flag says so.
/// </summary>
public class ProcessVitalsRestartRuleTests
{
    [Theory]
    [InlineData(-3600L)]
    [InlineData(-1L)]
    [InlineData(0L)]
    [InlineData(1L)]
    [InlineData(299L)]
    [InlineData(300L)]
    [InlineData(301L)]
    [InlineData(3600L)]
    [InlineData(86_400L)]
    [InlineData(922_337_203_685L)]
    public void TheWordsAndTheFlagNeverDisagree(long seconds)
    {
        Assert.Equal(
            new ProcessVitals(null, null, null, null, null, null, seconds).RestartedRecently,
            VitalsText.Uptime(seconds).StartsWith("restarted ", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(299L, true)]
    [InlineData(300L, false)]
    public void TheWordsAndTheFlagTurnAtTheSameSecond(long seconds, bool restarted)
    {
        Assert.Equal(restarted, new ProcessVitals(null, null, null, null, null, null, seconds).RestartedRecently);
        Assert.Equal(restarted, VitalsText.Uptime(seconds).StartsWith("restarted ", StringComparison.Ordinal));
    }
}
