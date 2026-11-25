namespace Dashboard.Health;

/// <summary>
/// One press of the traffic button as one value: how many requests a second, for how long, and how long each
/// request waits for its answer. The three numbers of a press are declared on the run itself, in
/// <see cref="Values"/> (#1777): every caller - the browser call, the countdown, the start sentence, the sentence
/// of <see cref="TrafficCount"/> about a request given up - reads them through the run, and the run is the only
/// way to get them, so a press cannot reach <c>start</c> of <c>js/traffic.js</c> as three ints taken from
/// somewhere else: carrying them as one named value is what keeps a press from sending for as long as a single
/// request was meant to wait, which three bare ints in a row could not (#1742). How often the page checks while
/// traffic runs is not part of a run and stays <see cref="TrafficPlan.CheckEvery"/>: a run is what the browser is
/// asked to send, not what the page does while it sends.
/// </summary>
/// <param name="PerSecond">Requests a second while the run lasts.</param>
/// <param name="Seconds">How long one press sends.</param>
/// <param name="AnswerWithinSeconds">How long one request waits for its answer before the browser gives it up.</param>
public sealed record TrafficRun(int PerSecond, int Seconds, int AnswerWithinSeconds)
{
    /// <summary>
    /// The three numbers of a press, declared on the run itself (#1777): how many requests a second, for how long,
    /// and how long each request waits for its answer. Consts, so no caller keeps a copy of the numbers; the const
    /// the shared-wait guard pins on <see cref="TrafficPlan"/> names the wait declared here.
    /// </summary>
    public static class Values
    {
        /// <summary>Requests per second while the button runs: enough to move every number, far below any limit.</summary>
        public const int PerSecond = 2;

        /// <summary>How long one press sends: the counters' window is one minute.</summary>
        public const int Seconds = 60;

        /// <summary>
        /// How long a request waits for its answer before the browser gives it up: as long as a check of the page
        /// waits (the shared <see cref="ProbeDefaults.DefaultTimeoutSeconds"/>). So a run is over this long after its
        /// last request at the latest. Whole seconds, because the browser call and the sentence about it take it
        /// that way.
        /// </summary>
        public const int AnswerWithinSeconds = ProbeDefaults.DefaultTimeoutSeconds;
    }

    /// <summary>
    /// The run of the button: the three numbers of <see cref="Values"/>, gathered into the one value the panel
    /// passes. Written from the run's own numbers, so the value a press sends is the value the run declares.
    /// </summary>
    public static TrafficRun Default { get; } = new(Values.PerSecond, Values.Seconds, Values.AnswerWithinSeconds);
}
