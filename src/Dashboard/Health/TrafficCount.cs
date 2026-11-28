using System.Globalization;

namespace Dashboard.Health;

/// <summary>
/// What the traffic button counted at one reading (<c>status</c> of <c>js/traffic.js</c>). Every request sent is in
/// one of four places: answered, failed (the browser reports a network failure), given up after
/// <see cref="TrafficRun.Values.AnswerWithinSeconds"/> s with no answer (said as <see cref="ProbeDefaults.NoAnswerWithin"/> says it), or still on its way. A run is over when nothing more is sent and
/// nothing is on its way, not when the sending stops: an answer that arrives after the last request was sent is
/// counted like any other, so a slow answer never reads as a lost one.
/// </summary>
/// <param name="Unanswered">Requests given up with no answer in time.</param>
/// <param name="Sending">True while requests are still being sent: until the run's seconds are over or it is stopped.</param>
public sealed record TrafficCount(int Sent, int Answered, int Failed, int Unanswered, bool Sending)
{
    /// <summary>Before the first press: nothing sent, nothing running.</summary>
    public static readonly TrafficCount None = new(0, 0, 0, 0, Sending: false);

    /// <summary>A run that was just started, before its first reading.</summary>
    public static readonly TrafficCount Started = None with { Sending = true };

    /// <summary>Requests that have neither an answer nor a failure yet, and were not given up.</summary>
    public int OnTheirWay => Math.Max(0, Sent - Answered - Failed - Unanswered);

    /// <summary>True when nothing more is sent and every request sent is accounted for.</summary>
    public bool Over => !Sending && OnTheirWay == 0;

    /// <summary>The count word of the requests that were sent: <c>119 sent</c>.</summary>
    private const string SentWord = "sent";

    /// <summary>The count word of the requests that were answered: <c>99 answered</c>.</summary>
    private const string AnsweredWord = "answered";

    /// <summary>The count word of the requests that failed: <c>7 failed</c>.</summary>
    private const string FailedWord = "failed";

    /// <summary>The lead of the given-up part, which <see cref="ProbeDefaults.NoAnswerWithin" /> finishes: <c>with no answer within 10 s</c>.</summary>
    private const string UnansweredLead = "with ";

    /// <summary>The agreement word of one request still in flight: <c>1 on its way</c>.</summary>
    private const string OnItsWayWords = "on its way";

    /// <summary>The agreement word of every other number of requests still in flight: <c>20 on their way</c>.</summary>
    private const string OnTheirWayWords = "on their way";

    /// <summary>The agreement word the panel waits for when one request is in flight: <c>waiting for its answer</c>.</summary>
    private const string ItsAnswerWords = "its answer";

    /// <summary>The agreement word the panel waits for when every other number of requests is in flight: <c>waiting for their answers</c>.</summary>
    private const string TheirAnswersWords = "their answers";

    /// <summary>The word of a run stopped by the button, not run out: <c>Traffic stopped</c>.</summary>
    private const string StoppedWord = "stopped";

    /// <summary>The word of a run that ran out: <c>Traffic ended</c>.</summary>
    private const string EndedWord = "ended";

    /// <summary>
    /// The numbers, which add up to the first: <c>119 sent, 99 answered, 20 on their way</c>. What is zero is left
    /// out, except the answered.
    /// </summary>
    public string Counts => new CardParts()
        .Add(Part(Sent, SentWord))
        .Add(Part(Answered, AnsweredWord))
        .Add(Failed > 0 ? Part(Failed, FailedWord) : null)
        .Add(Unanswered > 0 ? Part(Unanswered, UnansweredLead + ProbeDefaults.NoAnswerWithin(TrafficPlan.AnswerWithin)) : null)
        .Add(OnTheirWay > 0 ? Part(OnTheirWay, Plural.Agreement(OnTheirWay, OnItsWayWords, OnTheirWayWords)) : null)
        .Join();

    /// <summary>One number of <see cref="Counts" /> and its words, the number in the invariant culture: <c>119 sent</c>.</summary>
    private static string Part(int count, string words) =>
        string.Create(CultureInfo.InvariantCulture, $"{count} {words}");

    /// <summary>
    /// The line next to the button: the numbers with the seconds left while requests are sent, then what the panel
    /// waits for, then <c>Done: 119 sent, 119 answered.</c>
    /// </summary>
    public string Status(int secondsLeft) =>
        Over ? $"Done: {Counts}."
        : Sending ? string.Create(CultureInfo.InvariantCulture, $"{Counts}; {secondsLeft} {TimeText.SecondsUnit} left.")
        : $"{Counts}; waiting for {Plural.Agreement(OnTheirWay, ItsAnswerWords, TheirAnswersWords)}.";

    /// <summary>The event at the end of a run: <c>Traffic ended: 119 sent, 119 answered</c>.</summary>
    public string Ended(bool stopped) => $"Traffic {(stopped ? StoppedWord : EndedWord)}: {Counts}";

    /// <summary>
    /// The length of a reading: five numbers. The refusal of <see cref="From(IReadOnlyList{int})" /> says the same
    /// count in words - "five numbers".
    /// </summary>
    private const int ReadingLength = 5;

    /// <summary>
    /// A reading written as five numbers: sent, answered, failed, given up with no answer in time, and 1 while
    /// requests are still being sent. The browser does not answer this way: <c>status</c> of <c>js/traffic.js</c>
    /// answers one named object (<c>{ sent, answered, failed, unanswered, sending }</c>) that is read into this record
    /// by name (#1771), so a swap of two counts cannot compile into a wrong reading. The array itself is decoded the
    /// same way, each position into the member named for it, the order written once here (#1773), so a number that
    /// arrives in its neighbour's place still reads as the neighbour - failed as failed, and not as one given up with
    /// no answer in time.
    /// </summary>

    public static TrafficCount From(IReadOnlyList<int> status)
    {
        ArgumentNullException.ThrowIfNull(status);
        return status.Count == ReadingLength
            ? new TrafficCount(Sent: status[0], Answered: status[1], Failed: status[2], Unanswered: status[3], Sending: status[4] != 0)
            : throw new ArgumentException($"A reading of the traffic has five numbers, not {status.Count}.", nameof(status));
    }
}
