using System.Reflection;
using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// One press of the traffic button is a value of its own, <see cref="TrafficRun" /> (#1742), and not three bare
/// consts that reach <c>start</c> of <c>js/traffic.js</c> as three positional ints where every one of them is "how
/// many of what": the run is written from the three consts of <see cref="TrafficPlan" />, so each number keeps the
/// one home #1724 gave it; it carries exactly the three numbers of a press and not the interval the page checks on,
/// which is a different rule; and the panel hands its properties to <c>start</c> in the order that function names
/// them, which is what a swap of <c>Seconds</c> and <c>AnswerWithinSeconds</c> used to survive while changing what
/// a run does.
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
        Assert.Equal(
            new TrafficRun(TrafficPlan.PerSecond, TrafficPlan.Seconds, TrafficPlan.AnswerWithinSeconds),
            TrafficRun.Default);

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
    /// The three parameters <c>start</c> of <c>js/traffic.js</c> takes after the addresses, and the three properties
    /// of <see cref="TrafficRun" />, are the same three numbers by the same names: the value the panel passes is
    /// named for the call it is passed to.
    /// </summary>
    [Fact]
    public void TheRunIsNamedForTheParametersOfTheCallItIsPassedTo() =>
        Assert.Equal(
            StartParametersOfTheModule()[1..],
            NumbersOfARun,
            StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// What a press asks the browser for, in the order the browser asks for it: the addresses first, then the three
    /// numbers of one run value, each argument read from a member named for the parameter it lands on. Swapping
    /// <c>Run.Seconds</c> and <c>Run.AnswerWithinSeconds</c> here - which compiles, and sends for the time a single
    /// request was meant to wait - fails this, and so does passing a bare const again.
    /// </summary>
    [Fact]
    public void TheBrowserCallPassesTheRunsThreeNumbersInTheOrderTrafficJsNamesThem()
    {
        var parameters = StartParametersOfTheModule();
        var arguments = StartArgumentsOfThePanel();

        // Same JS arguments in the same order: the identifier, then the addresses, then three numbers.
        Assert.Equal(4, parameters.Length);
        Assert.Equal(parameters.Length, arguments.Length - 1);
        Assert.Equal("\"start\"", arguments[0]);
        Assert.Contains("addresses", arguments[1], StringComparison.Ordinal);

        // The JS parameter at index i lands on the panel's argument at index i + 1, which is one past the identifier.
        for (var index = 1; index < parameters.Length; index++)
        {
            var argument = arguments[index + 1];
            var property = argument[(argument.LastIndexOf('.') + 1)..];

            Assert.Equal(parameters[index], property, StringComparer.OrdinalIgnoreCase);

            // And read from the run, not from a bare const: the qualifier is the run value, wherever it is held.
            Assert.Contains("run", argument[..^(property.Length + 1)], StringComparison.OrdinalIgnoreCase);
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

    /// <summary>The parameter names of <c>start</c>, as the module declares them: addresses first, then the numbers.</summary>
    private static string[] StartParametersOfTheModule()
    {
        var declared = StartParameterList().Match(TrafficJsText());

        Assert.True(declared.Success, "start of js/traffic.js was not read: the guard would be checking nothing");

        return [.. declared.Groups[1].Value.Split(',').Select(name => name.Trim())];
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
