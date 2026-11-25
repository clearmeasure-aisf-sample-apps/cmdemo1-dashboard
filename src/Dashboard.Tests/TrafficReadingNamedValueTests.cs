using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>
/// The reading the traffic panel takes every second is one object, not five positions: <c>js/traffic.js</c> answers
/// <c>status()</c> with the numbers named, <c>{ sent, answered, failed, unanswered, sending }</c>, and the panel's
/// interop binder fills the <see cref="Dashboard.Health.TrafficCount" /> from the names, so a number that arrived
/// out of order cannot read as its neighbour's - a failed one reads as failed, not as one given up with no answer in
/// time. These tests stand where the reading crosses the boundary: the object the module answers, with each key
/// named and each value read from the member of <c>run</c> named the same; the panel's one line that asks the binder
/// for a <see cref="Dashboard.Health.TrafficCount" /> and no longer for five positions; the decode of the named
/// reading, under the options the WASM runtime uses for interop results, in which the same name binds the same
/// number whether the reading came from the object or from the array the standing tests still hold; and the
/// <see cref="Dashboard.Health.TrafficCount.From" /> decode, whose order is written once, each number into the member
/// named for it.
/// </summary>
public class TrafficReadingNamedValueTests
{
    /// <summary>
    /// The options the WASM runtime (<c>Microsoft.JSInterop</c>'s <c>JSRuntime</c>, inherited by
    /// <c>WebAssemblyJSRuntime</c>) gives interop results: camelCase keys, matched without regard to case - so the
    /// keys of the object the module answers bind onto <see cref="Dashboard.Health.TrafficCount" />'s members.
    /// </summary>
    private static readonly JsonSerializerOptions BinderOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>The decode the interop binder applies to a reading: <see cref="System.Text.Json.JsonSerializer" /> under
    /// those options.</summary>
    private static TrafficCount BinderReading(string reading) =>
        JsonSerializer.Deserialize<TrafficCount>(reading, BinderOptions)
            ?? throw new InvalidOperationException("The binder answered no reading.");

    /// <summary>
    /// The module answers one object per reading, not five positions: the first, before the press, is nothing - all
    /// the keys present, every value nothing, and <c>sending</c> a real false - and every later one reads each
    /// number from the member of <c>run</c> named the same, <c>sending</c> the flag itself and not a 1 or a 0.
    /// </summary>
    [Fact]
    public void TheReadingIsOneObjectWhoseKeysNameTheNumbers()
    {
        var source = File.ReadAllText(MarkupGuards.SourcePath("wwwroot/js/traffic.js"));

        Assert.Equal(1, MarkupGuards.CountOccurrences(source, "export function status() {"));
        Assert.Equal(1, MarkupGuards.CountOccurrences(source, "if (!run) return { sent: 0, answered: 0, failed: 0, unanswered: 0, sending: false };"));
        Assert.Equal(1, MarkupGuards.CountOccurrences(source, "return { sent: run.sent, answered: run.answered, failed: run.failed, unanswered: run.unanswered, sending: run.sending };"));
        Assert.Equal(0, MarkupGuards.CountOccurrences(source, "[run.sent, run.answered, run.failed, run.unanswered, run.sending ? 1 : 0]"));
        Assert.Equal(0, MarkupGuards.CountOccurrences(source, "[0, 0, 0, 0, 0]"));
    }

    /// <summary>
    /// The panel asks the binder for a <see cref="Dashboard.Health.TrafficCount" /> from <c>status</c> and not for five
    /// positions to decode itself: the one line is the named read, and the positional read stands nowhere.
    /// </summary>
    [Fact]
    public void ThePanelReadsStatusIntoTrafficCountByTheBinder()
    {
        var source = MarkupGuards.ComponentText("TrafficPanel");

        Assert.Equal(1, MarkupGuards.CountOccurrences(source, "_count = await _module!.InvokeAsync<TrafficCount>(\"status\", _stopping.Token);"));
        Assert.Equal(0, MarkupGuards.CountOccurrences(source, "InvokeAsync<int[]>(\"status\""));
        Assert.Equal(0, MarkupGuards.CountOccurrences(source, "TrafficCount.From(await"));
    }

    /// <summary>
    /// The named reading decodes into the count by its names: each key binds the member of the same name, and
    /// <c>sending</c> binds the flag, so the reading the module answers is the count the panel holds.
    /// </summary>
    [Fact]
    public void TheBinderReadingNamesEveryNumber()
    {
        var count = BinderReading("{ \"sent\": 119, \"answered\": 99, \"failed\": 6, \"unanswered\": 4, \"sending\": true }");

        Assert.Equal(119, count.Sent);
        Assert.Equal(99, count.Answered);
        Assert.Equal(6, count.Failed);
        Assert.Equal(4, count.Unanswered);
        Assert.True(count.Sending);
    }

    /// <summary>
    /// A number that arrives in its neighbour's place reads as the member named for it, not as a count with the
    /// others: three failed says <c>3 failed</c>, and three given up with no answer in time says <c>3 with no answer
    /// within 10 s</c> - the same number, the sentence named for the member it took.
    /// </summary>
    [Fact]
    public void AFailedNumberReadsAsFailedAndNotAsUnanswered()
    {
        var broken = BinderReading("{ \"sent\": 10, \"answered\": 7, \"failed\": 3, \"unanswered\": 0, \"sending\": false }");
        var slow = BinderReading("{ \"sent\": 10, \"answered\": 7, \"failed\": 0, \"unanswered\": 3, \"sending\": false }");

        Assert.Equal("10 sent, 7 answered, 3 failed", broken.Counts);
        Assert.Equal("10 sent, 7 answered, 3 with no answer within 10 s", slow.Counts);
    }

    /// <summary>
    /// The named reading and the standing array reading answer the same count: the object the binder fills and the
    /// five positions <see cref="Dashboard.Health.TrafficCount.From" /> decodes are the same values, name for name.
    /// </summary>
    [Fact]
    public void TheBinderReadingAgreesWithTheArrayReading()
    {
        var fromBinder = BinderReading("{ \"sent\": 119, \"answered\": 99, \"failed\": 6, \"unanswered\": 4, \"sending\": true }");

        Assert.Equal(TrafficCount.From([119, 99, 6, 4, 1]), fromBinder);
    }

    /// <summary>
    /// The <see cref="Dashboard.Health.TrafficCount.From" /> decode reads each position into the member named for it -
    /// the order written once, in the one place the decode stands - so a reading of five numbers can read no number
    /// as its neighbour's.
    /// </summary>
    [Fact]
    public void FromReadsEachNumberIntoTheMemberNamedForIt()
    {
        var source = File.ReadAllText(MarkupGuards.SourcePath("Health/TrafficCount.cs"));

        Assert.Equal(1, MarkupGuards.CountOccurrences(source, "Sent: status[0]"));
        Assert.Equal(1, MarkupGuards.CountOccurrences(source, "Answered: status[1]"));
        Assert.Equal(1, MarkupGuards.CountOccurrences(source, "Failed: status[2]"));
        Assert.Equal(1, MarkupGuards.CountOccurrences(source, "Unanswered: status[3]"));
        Assert.Equal(1, MarkupGuards.CountOccurrences(source, "Sending: status[4] != 0"));
    }
}
