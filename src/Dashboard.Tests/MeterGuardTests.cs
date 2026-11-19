using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// A meter of its own says nothing: <c>role="meter"</c> tells a reader that something is measured and no more, so the
/// tag that carries it has to say what it measures - <c>aria-label</c> or <c>aria-labelledby</c> - and the whole of the
/// reading: <c>aria-valuemin</c>, <c>aria-valuemax</c>, <c>aria-valuenow</c> and the value in words,
/// <c>aria-valuetext</c>, since the number a sighted reader sees beside the bar is nowhere in the tag itself (#130).
/// A guard walks every component and refuses a meter that says less, so the next bar added cannot be forgotten. The
/// tag is read to the ">" that closes it, past the Razor expressions and line breaks it carries - the meter of
/// <c>Components/Meter.razor</c> spreads its attributes over three lines.
/// </summary>
public class MeterGuardTests
{
    private const string MeterRole = "role=\"meter\"";

    /// <summary>How the offending part is named in a report: either of the two attributes gives the meter its name.</summary>
    private const string NameAttribute = "aria-label or aria-labelledby";

    /// <summary>The reading a meter owes a reader: the ends of its scale, where the value stands, and the value in words.</summary>
    private static readonly string[] ReadingAttributes =
    [
        "aria-valuemin",
        "aria-valuemax",
        "aria-valuenow",
        "aria-valuetext",
    ];

    private static string DashboardSourceDirectory([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard"));

    [Fact]
    public void EveryMeterHasANameAndAFullReading()
    {
        var offenders = new List<string>();

        foreach (var meter in Meters())
        {
            var missing = MissingFrom(meter.Tag).ToArray();

            if (missing.Length > 0)
            {
                offenders.Add($"{meter.File}:{meter.Line} missing {string.Join(", ", missing)}");
            }
        }

        Assert.Empty(offenders);
    }

    /// <summary>
    /// A guard that found nothing has seen nothing: the scan is shown to reach a real meter, the one this app draws
    /// bars with, before its clean verdict means anything.
    /// </summary>
    [Fact]
    public void TheScanFindsTheMeterTheAppDrawsWith()
    {
        var meters = Meters();

        Assert.NotEmpty(meters);
        Assert.Contains(meters, meter => meter.File.Equals("Meter.razor", StringComparison.Ordinal));
    }

    /// <summary>The rule refuses the meter that would read as bare silence: no name, no ends of the scale, no value.</summary>
    [Fact]
    public void TheRuleRefusesAMeterThatSaysNothing()
    {
        var tag = """
            <span class="meter" role="meter"
                  aria-valuenow="@Whole(percent)">
            """;

        Assert.Equal(
            new[] { NameAttribute, "aria-valuemin", "aria-valuemax", "aria-valuetext" },
            MissingFrom(tag).ToArray());
    }

    /// <summary>The rule accepts a named meter, however it is named - by a label of its own or by the words beside it.</summary>
    [Fact]
    public void TheRuleAcceptsAMeterThatSaysEverything()
    {
        var labelled = """
            <span class="meter" role="meter" aria-label="@Label"
                  aria-valuemin="0" aria-valuemax="100" aria-valuenow="@Whole(percent)" aria-valuetext="@ValueText">
            """;
        var describedByOtherMarkup = """
            <span role="meter" aria-labelledby="cpu-title" aria-valuemin="0" aria-valuemax="@Cores"
                  aria-valuenow="@Used" aria-valuetext="@($"{Used} of {Cores} cores")"></span>
            """;

        Assert.Empty(MissingFrom(labelled));
        Assert.Empty(MissingFrom(describedByOtherMarkup));
    }

    /// <summary>A tag ends at the ">" that closes it, not at the first ">" of the file: a Razor expression may hold a
    /// comparison, and the meter's own class carries calls whose parentheses close before the tag does.</summary>
    [Fact]
    public void TheTagReadRunsPastRazorAndLineBreaks()
    {
        var text = """
            <span class="meter @(ClusterText.IsHigh(percent > 50) ? "meter--high" : null)" role="meter"
                  aria-label="@Label" aria-valuemin="0" aria-valuemax="100"
                  aria-valuenow="@Whole(percent)" aria-valuetext="@ValueText"></span>
            """;

        var end = EndOfTag(text, 0);
        var tag = text[..(end + 1)];

        Assert.True(end > 0, "the tag of the sample never closes: the read ran past it");
        Assert.DoesNotContain("</span", tag, StringComparison.Ordinal);
        Assert.Empty(MissingFrom(tag));
    }

    /// <summary>Every meter of the app: the file to print, the line its tag opens on and the tag itself.</summary>
    private static List<(string File, int Line, string Tag)> Meters()
    {
        var meters = new List<(string File, int Line, string Tag)>();

        foreach (var file in RazorFiles(DashboardSourceDirectory()))
        {
            var text = File.ReadAllText(file);

            for (var at = text.IndexOf(MeterRole, StringComparison.Ordinal); at >= 0;
                 at = text.IndexOf(MeterRole, at + MeterRole.Length, StringComparison.Ordinal))
            {
                // The tag is the whole opening element, from its '<' to the '>' that closes it. A role found outside
                // any tag - a stray "<" already read as text - keeps an empty tag and so names every part it lacks.
                var start = text.LastIndexOf('<', at);
                var end = start < 0 ? -1 : EndOfTag(text, start);
                var tag = start < 0
                    ? string.Empty
                    : end < 0 ? text[start..] : text[start..(end + 1)];

                meters.Add((Path.GetFileName(file), start < 0 ? 1 : LineOf(text, start), tag));
            }
        }

        return meters;
    }

    /// <summary>What a meter tag lacks of what a reader needs: its name, then the parts of its reading.</summary>
    private static IEnumerable<string> MissingFrom(string tag)
    {
        if (!tag.Contains("aria-label=", StringComparison.Ordinal)
            && !tag.Contains("aria-labelledby=", StringComparison.Ordinal))
        {
            yield return NameAttribute;
        }

        foreach (var attribute in ReadingAttributes)
        {
            if (!tag.Contains(attribute + "=", StringComparison.Ordinal))
            {
                yield return attribute;
            }
        }
    }

    /// <summary>The index of the '>' that closes the tag starting at <paramref name="start"/>, or -1 when the file
    /// ends inside it. Parentheses are counted because a Razor expression may hold one.</summary>
    private static int EndOfTag(string text, int start)
    {
        var depth = 0;

        for (var i = start; i < text.Length; i++)
        {
            switch (text[i])
            {
                case '(':
                    depth++;
                    break;
                case ')' when depth > 0:
                    depth--;
                    break;
                case '>' when depth == 0:
                    return i;
            }
        }

        return -1;
    }

    private static int LineOf(string text, int at) => text.AsSpan(0, at).Count('\n') + 1;

    /// <summary>Every component of the app, the generated files of a build left out.</summary>
    private static IEnumerable<string> RazorFiles(string root)
    {
        var skipped = new[] { Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar,
                              Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar };

        return Directory.EnumerateFiles(root, "*.razor", SearchOption.AllDirectories)
            .Where(file => !skipped.Any(where => file.Contains(where, StringComparison.Ordinal)));
    }
}
