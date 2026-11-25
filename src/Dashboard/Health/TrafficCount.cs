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

    /// <summary>
    /// The numbers, which add up to the first: <c>119 sent, 99 answered, 20 on their way</c>. What is zero is left
    /// out, except the answered.
    /// </summary>
    public string Counts
    {
        get
        {
            var parts = new List<string> { Part(Sent, "sent"), Part(Answered, "answered") };
            if (Failed > 0)
            {
                parts.Add(Part(Failed, "failed"));
            }

            if (Unanswered > 0)
            {
                parts.Add(Part(Unanswered, "with " + ProbeDefaults.NoAnswerWithin(TrafficPlan.AnswerWithin)));
            }

            if (OnTheirWay > 0)
            {
                parts.Add(Part(OnTheirWay, Plural.Noun(OnTheirWay, "on its way", "on their way")));
            }

            return string.Join(", ", parts);
        }
    }

    /// <summary>One number of <see cref="Counts" /> and its words, the number in the invariant culture: <c>119 sent</c>.</summary>
    private static string Part(int count, string words) =>
        string.Create(CultureInfo.InvariantCulture, $"{count} {words}");

    /// <summary>
    /// The line next to the button: the numbers with the seconds left while requests are sent, then what the panel
    /// waits for, then <c>Done: 119 sent, 119 answered.</c>
    /// </summary>
    public string Status(int secondsLeft) =>
        Over ? $"Done: {Counts}."
        : Sending ? string.Create(CultureInfo.InvariantCulture, $"{Counts}; {secondsLeft} s left.")
        : $"{Counts}; waiting for {Plural.Noun(OnTheirWay, "its answer", "their answers")}.";

    /// <summary>The event at the end of a run: <c>Traffic ended: 119 sent, 119 answered</c>.</summary>
    public string Ended(bool stopped) => $"Traffic {(stopped ? "stopped" : "ended")}: {Counts}";

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
        return status.Count == 5
            ? new TrafficCount(Sent: status[0], Answered: status[1], Failed: status[2], Unanswered: status[3], Sending: status[4] != 0)
            : throw new ArgumentException($"A reading of the traffic has five numbers, not {status.Count}.", nameof(status));
    }
}
