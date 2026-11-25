using System.Text.Json;
using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// The read back of the traffic button is named, like the call into it (#1742, #1771): <c>status</c> of
/// <c>js/traffic.js</c> answers one object whose keys are the parameter names of <see cref="TrafficCount" />, and the
/// panel reads it into that record by name, so a swap of <c>answered</c> and <c>failed</c> on either side can no
/// longer compile into a failing system reported as an answering one.
/// </summary>
public partial class TrafficCountNamedReadingTests
{
    /// <summary>The five numbers of a reading, in the order the record declares them.</summary>
    private static readonly string[] PartsOfAReading = ["Sent", "Answered", "Failed", "Unanswered", "Sending"];

    [Fact]
    public void TheRecordDeclaresTheFivePartsOfAReading() =>
        Assert.Equal(
            PartsOfAReading,
            typeof(TrafficCount).GetConstructors().Single(constructor => constructor.GetParameters().Length == 5)
                .GetParameters().Select(parameter => parameter.Name));

    /// <summary>Both readings of the module - before the first press and during a run - answer the record's names,
    /// each key read from the run field of the same name, and not an array.</summary>
    [Fact]
    public void StatusAnswersOneObjectNamedForTheRecord()
    {
        var returns = StatusReturns().Matches(TrafficJsText());

        Assert.Equal(2, returns.Count);
        foreach (Match answer in returns)
        {
            var entries = answer.Groups[1].Value.Split(',').Select(entry => entry.Split(':')[0].Trim());
            Assert.Equal(PartsOfAReading, entries, StringComparer.OrdinalIgnoreCase);
        }

        Assert.Contains(
            "return { sent: run.sent, answered: run.answered, failed: run.failed, unanswered: run.unanswered, sending: run.sending };",
            TrafficJsText(),
            StringComparison.Ordinal);
        Assert.DoesNotContain("return [", TrafficJsText(), StringComparison.Ordinal);
    }

    /// <summary>The panel reads the browser's answer as the record itself, not as numbers it decodes by position.</summary>
    [Fact]
    public void ThePanelReadsTheReadingByName()
    {
        var text = MarkupGuards.ComponentText("TrafficPanel");

        Assert.Equal(1, MarkupGuards.CountOccurrences(text, "InvokeAsync<TrafficCount>(\"status\""));
        Assert.DoesNotContain("InvokeAsync<int[]>(\"status\"", text, StringComparison.Ordinal);
    }

    /// <summary>
    /// The object reaches the record by name under the web JSON defaults JS interop reads with, whatever order its
    /// keys come in: a failing run reads as failing, and the numbers say what the positional reading always said.
    /// </summary>
    [Theory]
    [InlineData("""{ "sent": 120, "answered": 0, "failed": 120, "unanswered": 0, "sending": false }""", "Done: 120 sent, 0 answered, 120 failed.")]
    [InlineData("""{ "failed": 120, "sending": false, "answered": 0, "unanswered": 0, "sent": 120 }""", "Done: 120 sent, 0 answered, 120 failed.")]
    [InlineData("""{ "sent": 10, "answered": 7, "failed": 2, "unanswered": 0, "sending": true }""", "10 sent, 7 answered, 2 failed, 1 on its way; 55 s left.")]
    [InlineData("""{ "sent": 0, "answered": 0, "failed": 0, "unanswered": 0, "sending": false }""", "Done: 0 sent, 0 answered.")]
    public void AReadingIsBoundByName(string json, string line)
    {
        var count = JsonSerializer.Deserialize<TrafficCount>(json, JsonSerializerOptions.Web);

        Assert.NotNull(count);
        Assert.Equal(line, count.Status(55));
    }

    [Fact]
    public void ANamedReadingIsTheSameCountAsThePositionalOne()
    {
        var named = JsonSerializer.Deserialize<TrafficCount>(
            """{ "sent": 119, "answered": 105, "failed": 0, "unanswered": 6, "sending": true }""",
            JsonSerializerOptions.Web);

        Assert.Equal(TrafficCount.From([119, 105, 0, 6, 1]), named);
        Assert.Equal(TrafficCount.None, JsonSerializer.Deserialize<TrafficCount>(
            """{ "sent": 0, "answered": 0, "failed": 0, "unanswered": 0, "sending": false }""",
            JsonSerializerOptions.Web));
    }

    private static string TrafficJsText() =>
        File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("wwwroot", "js", "traffic.js")));

    [GeneratedRegex(@"return \{([^}]*)\};")]
    private static partial Regex StatusReturns();
}
