using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// One press of the traffic button is a value of its own, <see cref="TrafficRun" /> (#1742), and not three bare
/// consts that reach <c>start</c> of <c>js/traffic.js</c> as three positional ints where every one of them is "how
/// many of what": the run is written from the three consts of <see cref="TrafficPlan" />, so each number keeps the
/// one home #1724 gave it; it carries exactly the three numbers of a press and not the interval the page checks on,
/// which is a different rule; and the panel hands the run to <c>start</c> whole, which reads it by name (#1776), so
/// a swap of <c>Seconds</c> and <c>AnswerWithinSeconds</c> - which three positional ints used to survive while
/// changing what a run does - has no order left to happen in.
/// </summary>
/// <remarks>
/// The two readings are the app's own: the JS module for the names and the number of the parameters, and
/// <c>TrafficPanel.razor</c> for the call, the countdown and the sentence - read as source, the way
/// <see cref="TrafficPanelTests" /> reads that file, because the call is exactly where the mixing up happens and no
/// compiled type is involved. The numbers themselves stay pinned by
/// <see cref="ProbeDefaultsSharedWaitSecondsEdgeCaseTests" /> and <see cref="TelemetryTests" />, which these tests
/// join rather than replace.
/// </remarks>
public partial class TrafficRunNamedValueTests
{
    /// <summary>The three numbers of a press, in the order a run carries them and the browser call takes them.</summary>
    private static readonly string[] NumbersOfARun = ["PerSecond", "Seconds", "AnswerWithinSeconds"];

    // ---------------------------------------------------------------- what the run is written from

    /// <summary>
    /// The run of the button is the three consts of <see cref="TrafficPlan" />, named one for one: a run is a
    /// naming of them, never a second copy - which is why <see cref="TrafficRun" /> declares no literal of its own.
    /// </summary>
    [Fact]
    public void TheRunIsTheThreeConstsOfThePlanAndKeepsNoCopyOfThem()
    {
        Assert.Equal(TrafficRun.Values.PerSecond, TrafficRun.Default.PerSecond);
        Assert.Equal(TrafficRun.Values.Seconds, TrafficRun.Default.Seconds);
        Assert.Equal(TrafficPlan.AnswerWithinSeconds, TrafficRun.Default.AnswerWithinSeconds);

        Assert.DoesNotContain(
            typeof(TrafficRun).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly),
            field => field.IsLiteral);
    }

    /// <summary>The run sends what the button always sent: two requests a second for a minute, every one of them
    /// given up after the shared ten seconds of a probe. Same numbers, same timeouts, whatever they are named by.</summary>
    [Fact]
    public void TheRunSendsTheNumbersTheButtonAlwaysSent()
    {
        Assert.Equal(2, TrafficRun.Default.PerSecond);
        Assert.Equal(60, TrafficRun.Default.Seconds);
        Assert.Equal(10, TrafficRun.Default.AnswerWithinSeconds);
        Assert.Equal(NodeProber.DefaultTimeout, TimeSpan.FromSeconds(TrafficRun.Default.AnswerWithinSeconds));
    }

    /// <summary>A run carries the three numbers of a press and nothing else: how often the page checks while traffic
    /// runs is not part of it (#1742), and stays the separate rule <see cref="TrafficPlan.IntervalNote" /> says.</summary>
    [Fact]
    public void TheRunCarriesTheThreeNumbersOfAPressAndNotTheIntervalOfThePage()
    {
        Assert.Equal(
            NumbersOfARun,
            typeof(TrafficRun).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Select(property => property.Name));

        Assert.DoesNotContain("CheckEverySeconds", NumbersOfARun, StringComparer.Ordinal);
        Assert.Equal(10, TrafficPlan.CheckEverySeconds);
        Assert.Equal("every 10 s while traffic runs", TrafficPlan.IntervalNote(running: true, TimeSpan.FromSeconds(30)));
    }

    // ---------------------------------------------------------------- how the run reaches the browser

    /// <summary>
    /// The three names <c>start</c> of <c>js/traffic.js</c> destructures from its run, and the three properties
    /// of <see cref="TrafficRun" />, are the same three numbers by the same names: the value the panel passes is
    /// named for the call it is passed to.
    /// </summary>
    [Fact]
    public void TheRunIsNamedForTheParametersOfTheCallItIsPassedTo() =>
        Assert.Equal(
            RunNamesOfTheModule(),
            NumbersOfARun,
            StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The run crosses to the browser by property name (#1776): serialised with the camel-case web naming the
    /// interop uses, the default run is one object whose keys are exactly the names <c>start</c> destructures,
    /// carrying the numbers the button always sent.
    /// </summary>
    [Fact]
    public void TheRunSerialisesToTheNamesTheModuleDestructures()
    {
        var json = JsonSerializer.Serialize(TrafficRun.Default, JsonSerializerOptions.Web);

        Assert.Equal("""{"perSecond":2,"seconds":60,"answerWithinSeconds":10}""", json);

        using var document = JsonDocument.Parse(json);

        Assert.Equal(
            RunNamesOfTheModule(),
            document.RootElement.EnumerateObject().Select(property => property.Name),
            StringComparer.Ordinal);
    }

    /// <summary>
    /// What a press asks the browser for (#1776): the addresses, then the run itself as one value that
    /// <c>start</c> destructures by name, so the three numbers have no order to be swapped in on either side.
    /// Passing <c>Run.Seconds</c> and <c>Run.AnswerWithinSeconds</c> as loose numbers again - where a swap compiles
    /// and sends for the time a single request was meant to wait - fails this, and so does passing a bare const.
    /// </summary>
    [Fact]
    public void TheBrowserCallPassesTheRunsThreeNumbersInTheOrderTrafficJsNamesThem()
    {
        var parameters = StartParametersOfTheModule();
        var arguments = StartArgumentsOfThePanel();

        // The identifier, then the addresses, then the run: two JS parameters, two arguments after the identifier.
        Assert.Equal(2, parameters.Length);
        Assert.Equal(parameters.Length, arguments.Length - 1);
        Assert.Equal("\"start\"", arguments[0]);
        Assert.Contains("addresses", arguments[1], StringComparison.Ordinal);

        // The module takes the run as one object and names its numbers, not as a list of positions.
        Assert.StartsWith("{", parameters[1], StringComparison.Ordinal);
        Assert.Equal(NumbersOfARun.Length, RunNamesOfTheModule().Length);

        // The panel hands over the run value itself, never one of its numbers picked out by position.
        Assert.Equal("Run", arguments[2]);

        foreach (var number in NumbersOfARun)
        {
            Assert.DoesNotContain(arguments, argument => argument.Contains($".{number}", StringComparison.Ordinal));
        }
    }

    /// <summary>The panel asks the browser for a press and for nothing else: <c>start</c> is the one call that takes
    /// the run, and its arguments are the only place the run's numbers are written out again.</summary>
    [Fact]
    public void ThePanelNamesTheRunOnceAndTheBareConstsNotAtAll()
    {
        var text = MarkupGuards.ComponentText("TrafficPanel");

        Assert.Equal(1, MarkupGuards.CountOccurrences(text, "InvokeVoidAsync(\"start\""));
        Assert.Equal(1, MarkupGuards.CountOccurrences(text, "TrafficRun Run { get; } = TrafficRun.Default;"));

        foreach (var number in NumbersOfARun)
        {
            Assert.DoesNotContain($"TrafficPlan.{number}", text, StringComparison.Ordinal);
        }

        // The interval is not part of a run, and the panel still reads it from the plan, once, in its own rule (#1742).
        Assert.Contains("++ticks % TrafficPlan.CheckEverySeconds == 0", text, StringComparison.Ordinal);
    }

    /// <summary>The sentences a press writes and the countdown beside the button read the same run as the browser
    /// call does, so the words a viewer reads and the requests the browser sends cannot come apart.</summary>
    [Fact]
    public void TheSentenceAndTheCountdownReadTheSameRunAsTheCall()
    {
        var text = MarkupGuards.ComponentText("TrafficPanel");

        Assert.Contains(
            """$"Traffic started: {Run.PerSecond} requests a second for {Run.Seconds} s to """,
            text,
            StringComparison.Ordinal);
        Assert.Contains("Math.Max(0, Run.Seconds - ", text, StringComparison.Ordinal);

        // And the words are the ones the button always said, written from the run: two a second for a minute.
        Assert.Equal(
            "Traffic started: 2 requests a second for 60 s to ui at uat.example.net",
            $"Traffic started: {TrafficRun.Default.PerSecond} requests a second for {TrafficRun.Default.Seconds} s to ui at uat.example.net");
    }

    // ---------------------------------------------------------------- the machinery

    /// <summary>The module a press is handed to, as the app ships it: the shared read of the app's source, the way
    /// every other guard reaches a file of <c>src/Dashboard</c> that is not a component.</summary>
    private static string TrafficJsText() =>
        File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("wwwroot", "js", "traffic.js")));

    /// <summary>The parameters of <c>start</c>, as the module declares them: addresses first, then the run, each on
    /// one line (the run as its destructuring pattern, braces and all).</summary>
    private static string[] StartParametersOfTheModule()
    {
        var declared = StartParameterList().Match(TrafficJsText());

        Assert.True(declared.Success, "start of js/traffic.js was not read: the guard would be checking nothing");

        return SplitArguments(declared.Groups[1].Value);
    }

    /// <summary>The names <c>start</c> destructures from the run it is passed, in the order the module writes them.</summary>
    private static string[] RunNamesOfTheModule()
    {
        var run = StartParametersOfTheModule()[^1];

        Assert.True(
            run.StartsWith('{') && run.EndsWith('}'),
            $"start of js/traffic.js does not destructure its run by name: '{run}'");

        return [.. run[1..^1].Split(',').Select(name => name.Trim())];
    }

    /// <summary>The arguments of the panel's <c>InvokeVoidAsync("start", ...)</c>, the identifier first: the call is
    /// found by its identifier and taken whole, brackets balanced, so a call wrapped over lines is read as the call
    /// it is.</summary>
    private static string[] StartArgumentsOfThePanel()
    {
        var code = MarkupGuards.ComponentText("TrafficPanel");
        var at = code.IndexOf("\"start\"", StringComparison.Ordinal);

        Assert.True(at >= 0, "the panel's call of start was not read: the guard would be checking nothing");

        var open = code.LastIndexOf('(', at);
        var depth = 0;
        var close = -1;

        for (var index = open; index < code.Length; index++)
        {
            if (code[index] == '(')
            {
                depth++;
            }
            else if (code[index] == ')' && --depth == 0)
            {
                close = index;
                break;
            }
        }

        Assert.True(close > open, $"the call of start opened at {open} and was never closed: the read is lost");

        return SplitArguments(code[(open + 1)..close]);
    }

    /// <summary>The comma-separated parts of one call, on one line each: commas inside brackets or quotes belong to
    /// the argument they stand in, not to the split.</summary>
    private static string[] SplitArguments(string call)
    {
        var parts = new List<string>();
        var depth = 0;
        var quote = '\0';
        var start = 0;

        for (var index = 0; index < call.Length; index++)
        {
            var character = call[index];

            if (quote != '\0')
            {
                if (character == quote)
                {
                    quote = '\0';
                }
            }
            else if (character is '"' or '\'')
            {
                quote = character;
            }
            else if (character is '(' or '[' or '{')
            {
                depth++;
            }
            else if (character is ')' or ']' or '}')
            {
                depth--;
            }
            else if (character == ',' && depth == 0)
            {
                parts.Add(OneLine(call[start..index]));
                start = index + 1;
            }
        }

        parts.Add(OneLine(call[start..]));

        return [.. parts];
    }

    private static string OneLine(string text) =>
        string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    [GeneratedRegex(@"export function start\(([^)]*)\)")]
    private static partial Regex StartParameterList();
}
