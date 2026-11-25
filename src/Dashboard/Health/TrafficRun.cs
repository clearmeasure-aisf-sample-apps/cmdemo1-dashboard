namespace Dashboard.Health;

/// <summary>
/// One press of the traffic button as one value: how many requests a second, for how long, and how long each
/// request waits for its answer. These are the three numbers <c>start</c> of <c>js/traffic.js</c> takes after the
/// addresses, in that order, and carrying them as one named value is what keeps a press from sending for as long as
/// a single request was meant to wait - which three bare ints in a row could not (#1742). How often the page checks
/// while traffic runs is not part of a run and stays <see cref="TrafficPlan.CheckEvery"/>: a run is what the browser
/// is asked to send, not what the page does while it sends.
/// </summary>
/// <param name="PerSecond">Requests a second while the run lasts.</param>
/// <param name="Seconds">How long one press sends.</param>
/// <param name="AnswerWithinSeconds">How long one request waits for its answer before the browser gives it up.</param>
public sealed record TrafficRun(int PerSecond, int Seconds, int AnswerWithinSeconds)
{
    /// <summary>
    /// The run of the button: the three consts of <see cref="TrafficPlan"/>, gathered into the one value the panel
    /// passes. Each number keeps its single home in <see cref="TrafficPlan"/> (the wait there is
    /// <see cref="ProbeDefaults.DefaultTimeoutSeconds"/> by way of <see cref="TrafficPlan.AnswerWithinSeconds"/>),
    /// so a run is a naming of them and never a second copy of them.
    /// </summary>
    public static TrafficRun Default { get; } = new(
        PerSecond: TrafficPlan.PerSecond,
        Seconds: TrafficPlan.Seconds,
        AnswerWithinSeconds: TrafficPlan.AnswerWithinSeconds);
}
