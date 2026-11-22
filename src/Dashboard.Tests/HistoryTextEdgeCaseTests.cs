namespace Dashboard.Tests;

/// <summary>
/// The degenerate wording of the two <see cref="HistoryText.Describe(IReadOnlyCollection{ProbeResult})" /> and
/// <see cref="HistoryText.Describe(ProbeResult, TimeZoneInfo)" /> overloads: a history that names no state at all,
/// the pending check that stands beside one that does, and the one check that is missing one of the parts a check
/// usually carries.
/// <para>
/// What is pinned where it belongs: <see cref="TextTests" /> pins the clock in the viewer's time zone, the empty
/// history, the four checks counted by state in one sentence, and the one check described with what is known;
/// <see cref="HistoryStripTests" /> pins the strip that says the same words - the hover title of a filled bar and the
/// aria-label of the strip - and nothing else.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the history that holds only <see cref="HealthState.Pending" />
/// results - no check answered, no state to name - said as <c>Last check: </c> and <c>Last 2 checks: </c>, the
/// trailing space in place of a state, no state named, and the pending checks counted in the total; the pending
/// check standing beside a healthy one, counted in the total and left out of the list: <c>Last 2
/// checks: 1 healthy</c>; and the one check said with only some of what it knows - the HTTP status without a time
/// to the answer, a time to the answer without an HTTP status, a healthy check with its status and its time to the
/// answer both, a time to the answer that is negative or the largest <see cref="int" /> it holds, said as it is
/// and not corrected, and the clock said in the zone the check is handed in rather than the one it was made in; and
/// the two arguments a describe cannot be made without, refused by the name of the argument: <c>result</c> and
/// <c>history</c>.
/// </para>
/// </summary>
public class HistoryTextEdgeCaseTests
{
    private static readonly DateTimeOffset Time = new(2026, 10, 4, 22, 0, 5, TimeSpan.Zero);

    /// <summary>The time zone the clock is asked to speak in: five hours behind the one the checks below were
    /// made in, and not the one they were made in.</summary>
    private static readonly TimeZoneInfo MinusFive = TimeZoneInfo.CreateCustomTimeZone("test", TimeSpan.FromHours(-5), "test", "test");

    /// <summary>A check made at the fixed moment: the state named, the HTTP status and the time to the answer
    /// named or absent, and nothing invented where nobody named them.</summary>
    private static ProbeResult Check(HealthState state, int? statusCode = null, int? latencyMs = null) =>
        new(state, statusCode, latencyMs, Time);

    // ----- The history that names no state

    /// <summary>A history holding one pending check is said as <c>Last check: </c> - the singular total, the
    /// trailing space where a state would stand, and nothing after it: the pending check is counted in the
    /// total and no state is named.</summary>
    [Fact]
    public void AHistoryOfOnePendingCheckIsSaidWithATrailingSpaceAndNoStateNamed() =>
        Assert.Equal("Last check: ", HistoryText.Describe([Check(HealthState.Pending)]));

    /// <summary>A history holding two pending checks is said as <c>Last 2 checks: </c> - the plural total the
    /// pending checks count to, the trailing space where a state would stand, and nothing after it.</summary>
    [Fact]
    public void AHistoryOfTwoPendingChecksIsSaidWithATrailingSpaceAndNoStateNamed() =>
        Assert.Equal("Last 2 checks: ", HistoryText.Describe([Check(HealthState.Pending), Check(HealthState.Pending)]));

    // ----- The pending check beside a named one

    /// <summary>A pending check standing beside a healthy one is said as <c>Last 2 checks: 1 healthy</c>:
    /// both checks count to the two of the total, and the list names only the state a check answered in - the
    /// same words with the pending check first or last, since the history is counted, not read in order.</summary>
    [Fact]
    public void APendingCheckBesideAHealthyOneIsCountedInTheTotalButNotNamed()
    {
        Assert.Equal("Last 2 checks: 1 healthy", HistoryText.Describe([Check(HealthState.Pending), Check(HealthState.Healthy, 200, 10)]));
        Assert.Equal("Last 2 checks: 1 healthy", HistoryText.Describe([Check(HealthState.Healthy, 200, 10), Check(HealthState.Pending)]));
    }

    // ----- One check, said with only what is known

    /// <summary>The HTTP status is said without a time to the answer when the check carries no time: the state and
    /// the status stand, and nothing follows them - the sentence does not fill the absent part with a zero.</summary>
    [Fact]
    public void AStatusIsSaidWithoutATimeToTheAnswer() =>
        Assert.Equal("22:00:05 Unhealthy, HTTP 503", HistoryText.Describe(Check(HealthState.Unhealthy, 503), TimeZoneInfo.Utc));

    /// <summary>A time to the answer is said without an HTTP status when the check carries no status: the state
    /// and the time stand, and the status part is not said at all - the two parts are added on their own, one for
    /// each the check carries.</summary>
    [Fact]
    public void ATimeToTheAnswerIsSaidWithoutAStatus() =>
        Assert.Equal("22:00:05 Unreachable, 12 ms", HistoryText.Describe(Check(HealthState.Unreachable, latencyMs: 12), TimeZoneInfo.Utc));

    /// <summary>A healthy check that answered HTTP 200 is said with its status and its time to the answer both,
    /// each in its own place: the state first, the status second and the time third, and the words of the state
    /// the classifier gives and not the number it was answered with.</summary>
    [Fact]
    public void AHealthyCheckIsSaidWithItsStatusAndItsTimeToTheAnswer() =>
        Assert.Equal("22:00:05 Healthy, HTTP 200, 10 ms", HistoryText.Describe(Check(HealthState.Healthy, 200, 10), TimeZoneInfo.Utc));

    /// <summary>A time to the answer that is negative is said as it is: the sentence takes the number the check
    /// carries and says it, and does not correct a time to the answer into a time it cannot be - the words are the
    /// check's and not the clock's.</summary>
    [Fact]
    public void ANegativeTimeToTheAnswerIsSaidAsItIs() =>
        Assert.Equal("22:00:05 Healthy, HTTP 200, -1 ms", HistoryText.Describe(Check(HealthState.Healthy, 200, -1), TimeZoneInfo.Utc));

    /// <summary>The largest time to the answer an <see cref="int" /> can hold is said as the number it is: the
    /// sentence does not overflow, round or name the limit - the full number stands between the comma and the
    /// <c>ms</c>.</summary>
    [Fact]
    public void TheLargestTimeToTheAnswerIsSaidAsItIs() =>
        Assert.Equal("22:00:05 Healthy, HTTP 200, 2147483647 ms", HistoryText.Describe(Check(HealthState.Healthy, 200, int.MaxValue), TimeZoneInfo.Utc));

    /// <summary>The clock of a check is said in the time zone the check is handed in and nowhere else: the same
    /// check made in the zone it was made in says one hour of day, and in the zone five hours behind that zone it
    /// says the other - the check's moment does not move, only the words do.</summary>
    [Fact]
    public void TheClockIsSaidInTheZoneTheCheckIsGivenIn()
    {
        var check = Check(HealthState.Healthy, 200, 10);

        Assert.Equal("22:00:05 Healthy, HTTP 200, 10 ms", HistoryText.Describe(check, TimeZoneInfo.Utc));
        Assert.Equal("17:00:05 Healthy, HTTP 200, 10 ms", HistoryText.Describe(check, MinusFive));
    }

    // ----- The arguments that are refused

    /// <summary>A check that is not there is refused by the name of the argument, whatever the time zone it would
    /// have been said in: the refusal is the describe's and not the clock's.</summary>
    [Fact]
    public void ACheckThatIsNotThereIsRefusedByName()
    {
        var error = Assert.Throws<ArgumentNullException>(() => HistoryText.Describe((ProbeResult)null!, TimeZoneInfo.Utc));

        Assert.Equal("result", error.ParamName);
    }

    /// <summary>A history that is not there is refused by the name of the argument, and not answered with the
    /// words of an empty history: the refusal stands where the empty history would have been said.</summary>
    [Fact]
    public void AHistoryThatIsNotThereIsRefusedByName()
    {
        var error = Assert.Throws<ArgumentNullException>(() => HistoryText.Describe((IReadOnlyCollection<ProbeResult>)null!));

        Assert.Equal("history", error.ParamName);
    }
}
