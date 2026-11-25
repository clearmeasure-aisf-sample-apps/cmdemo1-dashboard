namespace Dashboard.Health;

/// <summary>
/// The one home of the dashboard's interval file reads: the stamp of the last asking, the due question
/// (<see cref="IntervalDue" /> with the read's interval), the read, and what a read that answers nothing leaves
/// standing. The asking is stamped before the read, so a slow answer does not buy a second asking; a read that
/// answers nothing keeps the last good answer, the one the view keeps showing. The database view's live-file read
/// and the monitor's file reads are on it, so the rule stands in this one place, beside the page's and the
/// monitor's own gates.
/// </summary>
/// <typeparam name="T">The answer the read keeps; null is the no-answer.</typeparam>
public sealed class IntervalRead<T>(TimeSpan interval) where T : class
{
    /// <summary>When the read was last asked for; null when it never was. The stamp stands while the answer is out.</summary>
    public DateTimeOffset? LastAsked { get; private set; }

    /// <summary>The last answer the read kept; null before the first kept one. A read that answers nothing keeps it.</summary>
    public T? Last { get; private set; }

    /// <summary>
    /// True once a reading has come back - asked, and answered, kept or not - and never unset; the view can tell
    /// "loading" from "not published". A reading that never came back has not been made.
    /// </summary>
    public bool Asked { get; private set; }

    /// <summary>Whether the read is due at <paramref name="now"/>: the one interval rule (<see cref="IntervalDue" />) with the read's interval.</summary>
    public bool IsDue(DateTimeOffset now) => IntervalDue.IsDue(LastAsked, now, interval);

    /// <summary>
    /// Reads, where due: it stamps the asking at <paramref name="now"/>, reads, and keeps the answer where it
    /// answers, else the last good one. The answer it kept; null where it was not due or the read answered nothing.
    /// </summary>
    public async Task<T?> ReadIfDueAsync(DateTimeOffset now, Func<CancellationToken, Task<T?>> read, CancellationToken cancellationToken)
    {
        if (!IsDue(now))
        {
            return null;
        }

        LastAsked = now;
        var answer = await read(cancellationToken);
        Asked = true;
        if (answer is null)
        {
            return null;
        }

        Last = answer;
        return answer;
    }

    /// <summary>
    /// Asks, where due: it stamps the asking at <paramref name="now"/> and takes the answer as it is, kept or not -
    /// the read whose no-answer is itself the answer, a file that is gone saying nothing is deploying. The answer,
    /// kept or not; null where it was not due or the read answered nothing.
    /// </summary>
    public async Task<T?> AskIfDueAsync(DateTimeOffset now, Func<CancellationToken, Task<T?>> read, CancellationToken cancellationToken)
    {
        if (!IsDue(now))
        {
            return null;
        }

        LastAsked = now;
        var answer = await read(cancellationToken);
        Asked = true;
        return answer;
    }
}
