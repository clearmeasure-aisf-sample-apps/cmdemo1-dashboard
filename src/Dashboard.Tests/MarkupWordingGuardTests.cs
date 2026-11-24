using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// A word the page says about a moment or a sum is owned by the code that decides when to say it, and a component
/// that writes "as of", "yesterday", "min ago" or a sum of its own is a copy that drifts: the class moves a
/// threshold and the markup keeps on saying the old words, confidently, forever. A guard therefore walks every
/// <c>.razor</c> file of the app the way <see cref="TimeElementGuardTests" />, <see cref="NewTabLinkGuardTests" />
/// and <see cref="InlineStyleGuardTests" /> walk them - reading each file whole, passing over the comments that
/// quote the words on purpose, and reporting the file and the line a wording stands on - and the wording it looks
/// for is not typed out here but taken from the real returns of the methods above, so the guard follows a rewording
/// instead of arguing with it.
/// <para>
/// What the markup says today anyway is written down, exactly, in <see cref="AllowedSpeech" />: three lines that
/// name a day as plainly as <see cref="CostText" /> does, kept because taking them out would mean editing markup
/// this test has no business editing.
/// </para>
/// </summary>
public class MarkupWordingGuardTests
{
    /// <summary>The moment this rendering happens, and the instant every "ago" is counted from.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>The last complete UTC day a cost file covers: the day before <see cref="Now" />.</summary>
    private static readonly DateOnly Day = new(2026, 10, 3);

    private static readonly CostAmounts Amounts = new(1.52, 9.8, 11.02);
    private static readonly CostReport Report = new(Now, "USD", Day, Amounts, []);

    /// <summary>How a wording a component must not write is looked for.</summary>
    private enum Kind
    {
        /// <summary>The words themselves, with no letter glued in front of them and no letter or digit glued to
        /// their back - a line that says "was of" or "this monthly" says something else.</summary>
        Words,

        /// <summary>The words with a digit straight after them - the sign <c>CostText.Money</c> puts in front of a number.</summary>
        Lead,

        /// <summary>The words as a shape, every digit of the sample return standing for any digit at all - a day of
        /// the same length whatever day it is.</summary>
        Shape,
    }

    /// <summary>
    /// One wording the page says about a moment, a link or a sum, and the code that decides when it is true. The
    /// words are not typed out below: they are read back out of the real returns of <see cref="TimeText" />,
    /// <see cref="LinkText" /> and <see cref="CostText" />, so a rewording moves the guard with it instead of leaving
    /// a list of the old words behind, and a threshold moved is caught by the tests above rather than quietly
    /// tolerated here.
    /// </summary>
    private sealed record OwnedWord(string Words, string Owner, Kind Kind);

    /// <summary>The unit words <c>TimeText.Span</c> settles on, read off one span of each band.</summary>
    private static readonly string[] Units =
    [
        LastWord(TimeText.Span(TimeSpan.FromSeconds(45))),
        LastWord(TimeText.Span(TimeSpan.FromMinutes(3))),
        LastWord(TimeText.Span(TimeSpan.FromHours(2))),
        LastWord(TimeText.Span(TimeSpan.FromDays(4))),
    ];

    /// <summary>The word <c>TimeText.Ago</c> hangs on the end of a span.</summary>
    private static readonly string Ago = LastWord(TimeText.Ago(Now.AddMinutes(-5), Now));

    /// <summary>The day part of "as of 2026-10-03 (UTC)": the words before the date.</summary>
    private static readonly string AsOfWords = BeforeFirstDigit(CostText.AsOf(Day)!);

    /// <summary>The sign <c>CostText.Money</c> puts in front of a dollar amount.</summary>
    private static readonly string DollarSign = BeforeFirstDigit(CostText.Money(0, "USD"));

    /// <summary>The shape of a day as <c>CostText.Date</c> writes it - <c>####-##-##</c> for the file that writes
    /// <c>2026-10-03</c>, and the shape of whatever it writes on any other day.</summary>
    private static readonly string DayShape = Shape(CostText.Date(Day));

    /// <summary>The words a link's suffix says beyond "opens in a new tab", which the markup is required to write
    /// itself by <see cref="NewTabLinkGuardTests" />: the part that names who asks for a sign-in.</summary>
    private static readonly string[] SignIn = [Between(LinkText.PortalSuffix, "; ", ")"), Between(LinkText.OctopusSuffix, "; ", ")")];

    /// <summary>Every wording the text classes own, as the code returns it today.</summary>
    private static readonly OwnedWord[] Owned =
    [
        .. Units.Select(unit => new OwnedWord($"{unit} {Ago}", $"TimeText.Span counts an age out in {unit}s and TimeText.Ago hangs the word {Ago} on the end of it", Kind.Words)),
        new OwnedWord(TimeText.Ago(Now, Now), "TimeText.Ago says it of a moment under a minute old, and of one ahead of the clock", Kind.Words),
        new OwnedWord(CostText.Day(null, Now), "CostText.Day says it when the file names no day at all", Kind.Words),
        new OwnedWord(CostText.Day(Day, Now), "CostText.Day says it only of the UTC day before the rendering", Kind.Words),
        new OwnedWord(CostText.Month(null, Now), "CostText.Month says it only while the day named stands in this month", Kind.Words),
        new OwnedWord(CostText.Parts(Amounts, "USD", Day, Now)[1].Period, "CostText.Parts names the seven days a sum covers with it", Kind.Words),
        new OwnedWord(AsOfWords, "CostText.AsOf puts it in front of the day its numbers stop at", Kind.Words),
        new OwnedWord(DollarSign, "CostText.Money signs a US dollar amount with it", Kind.Lead),
        new OwnedWord(DayShape, "CostText.Date and TimeText.Iso write a day shaped like it", Kind.Shape),
        .. SignIn.Select(words => new OwnedWord(words, "LinkText ends a portal and an Octopus tooltip with it", Kind.Words)),
    ];

    /// <summary>
    /// The wording the markup writes for itself today, exactly: the file as the walk prints it and the words it
    /// speaks. Three lines name the day a card is of in their own words - <c>CostLine.razor</c> and
    /// <c>WorkItemsView.razor</c> so that the day stands inside a real <c>&lt;time&gt;</c> element that
    /// <see cref="TimeElementGuardTests" /> insists carries a <c>datetime</c>, and <c>DeliveryCard.razor</c> inside a
    /// sentence of its own about when the delivery facts changed. Each is a copy of <see cref="CostText.AsOf" />'s
    /// words, and each is listed rather than rewritten here: this test reads the markup, it does not edit it.
    /// Nothing else is on the list, and nothing joins it without a decision written down next to it.
    /// </summary>
    private static readonly (string File, string Words)[] AllowedSpeech =
    [
        (Path.Combine("Components", "CostLine.razor"), AsOfWords),
        (Path.Combine("Components", "DeliveryCard.razor"), AsOfWords),
        (Path.Combine("Components", "WorkItemsView.razor"), AsOfWords),
    ];

    /// <summary>One place the markup speaks a wording the code owns: the file to print, the line it stands on, the
    /// wording, and the line as the file wrote it.</summary>
    private sealed record SpokenWord(string File, int Line, OwnedWord Word, string Source)
    {
        /// <summary>The allowlist's pair: the file and the words, with no line number to drift.</summary>
        public (string File, string Words) Key => (File, Word.Words);

        /// <summary>Where the fault stands: the file as the walk prints it, the line the words stand on, the words
        /// as the markup wrote them, and the code that owns them.</summary>
        public string Report =>
            $"{File}:{Line} the markup writes \"{Source.Trim()}\": {Word.Owner}, which settles it against a number";
    }

    /// <summary>
    /// No component of the app says for itself what the text classes settle on: no age spelled out of a unit and
    /// "ago", no "yesterday", "this month" or "as of" that no comparison with the clock has earned, no dollar sign
    /// in front of a hand-written number, no day typed out in the shape <c>CostText.Date</c> writes and no
    /// promise of a sign-in that <c>LinkText</c> does not make. Anything the markup does say today is on the list
    /// above, file and words; a wording added without a line here fails, and the failure prints the line to look at.
    /// </summary>
    [Fact]
    public void NoComponentSpeaksWhatTheTextClassesDecide()
    {
        var offenders = OffendersOf(SpokenInApp());

        Assert.Empty(offenders);
    }

    /// <summary>The other direction of the same list: a wording the allowlist approves and the markup no longer
    /// speaks approves nothing, and is reported so that the person who fixes one of these three lines takes it off
    /// rather than leaves a permission standing for words nobody writes. Compared as the multiset it is, so a fourth
    /// "as of" in a file the list already names is refused and one entry less in the list than in the markup is too.</summary>
    [Fact]
    public void EveryAllowedWordingStillStandsInTheMarkup()
    {
        var spoken = SpokenInApp()
            .Select(spoken => spoken.Key)
            .OrderBy(key => key.File, StringComparer.Ordinal)
            .ThenBy(key => key.Words, StringComparer.Ordinal)
            .ToList();

        var allowed = AllowedSpeech
            .OrderBy(entry => entry.File, StringComparer.Ordinal)
            .ThenBy(entry => entry.Words, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(allowed, spoken);
    }

    /// <summary>
    /// A guard that found nothing has read nothing. The walk reaches forty-four files of markup; the wordings above
    /// are fourteen, every one of them read out of what the code returns; and those words do stand in the app -
    /// fourteen times as the files are written, three of them in the markup this rule is about and eleven inside the
    /// comments that quote what a card will read ("5 min ago", "yesterday" and "this month" are said only while they
    /// are true), which is where they belong and where this guard has to be blind. So the check is that the raw files
    /// speak far more often than the comment-blanked ones do: a walk that reached no file fails the counts, and one
    /// that stripped nothing fails the difference.
    /// </summary>
    [Fact]
    public void TheGuardReadsTheMarkupAndTheCommentsItPassesOver()
    {
        var markup = RazorMarkup().ToList();
        var files = markup.Select(part => part.File).ToList();

        Assert.True(files.Count >= 20, $"only {files.Count} .razor file(s) were read under {DashboardRoot()}: the walk does not reach the app");

        Assert.Contains(Path.Combine("Components", "CostLine.razor"), files, StringComparer.Ordinal);
        Assert.Contains(Path.Combine("Components", "WorkItemsView.razor"), files, StringComparer.Ordinal);
        Assert.Contains("App.razor", files, StringComparer.Ordinal);

        Assert.True(Owned.Length >= 12, $"only {Owned.Length} wording(s) are owned: the list stopped reading what the code returns");
        Assert.All(Owned, word => Assert.False(string.IsNullOrWhiteSpace(word.Words), $"{word.Owner}: the wording owns nothing at all"));

        // The list really is what the code returns today rather than an empty one left standing: the words the tests
        // above assert are the words the guard looks for, and the shape is the real day with its digits stood in for.
        // Reword a method and these fail - which is the day this guard has to be read again, not quietly ignored.
        Assert.Contains(Owned, word => word.Words == TimeText.Ago(Now, Now) && word.Words == "just now");
        Assert.Contains(Owned, word => word.Words == CostText.Day(Day, Now) && word.Words == "yesterday");
        Assert.Contains(Owned, word => word.Words == CostText.Month(null, Now) && word.Words == "this month");
        Assert.Contains(Owned, word => word.Words == AsOfWords && word.Words == "as of");
        Assert.Contains(Owned, word => word.Kind == Kind.Shape && word.Words == DayShape && DayShape == "####-##-##");
        Assert.Contains(Owned, word => word.Kind == Kind.Lead && word.Words == DollarSign);

        var raw = markup.Sum(part => CountOf(part.Text));
        var spoken = SpokenInApp();

        Assert.True(raw > spoken.Count, $"the comments held {raw - spoken.Count} of the {raw} occurrence(s): the scan read {spoken.Count} and stripped nothing");
        Assert.True(raw >= 10, $"only {raw} occurrence(s) of an owned wording were seen in {files.Count} file(s): the patterns match nothing");

        // And the three lines the allowlist approves really are the three the app writes.
        Assert.Equal(AllowedSpeech.Length, spoken.Count);
        Assert.All(spoken, word => Assert.Contains("as of", word.Report, StringComparison.Ordinal));
    }

    /// <summary>The rule refuses each kind of wording it owns, on the line the words stand on: an age spelled by
    /// hand, a "yesterday" no comparison with the clock earned, a sum signed in the markup, a day typed out, and a
    /// promise of a sign-in the tooltip does not carry from <c>LinkText</c>.</summary>
    [Fact]
    public void TheRuleRefusesWordsTheMarkupWritesForItself()
    {
        var text = """
            <main>
                <span>built 5 min ago</span>
                <span title="The Azure portal asks you to sign in">portal</span>
                <p>as of yesterday</p>
                <dd>$1.52</dd>
                <dd>2026-10-03</dd>
                <span class="cost__part cost__as-of">no wording here</span>
            </main>
            """;

        var spoken = Scan(Path.Combine("Components", "Sample.razor"), text);
        var lines = spoken.Select(word => word.Line).ToList();

        Assert.Equal([2, 3, 4, 4, 5, 6], lines);

        Assert.Contains(spoken, word => word.Word.Words == "min ago");
        Assert.Contains(spoken, word => word.Word.Words == "the Azure portal asks you to sign in");
        Assert.Contains(spoken, word => word.Word.Words == "as of");
        Assert.Contains(spoken, word => word.Word.Words == "yesterday");
        Assert.Contains(spoken, word => word.Word.Kind == Kind.Lead);
        Assert.Contains(spoken, word => word.Word.Kind == Kind.Shape);

        // The class name that carries the wording's own letters is not the wording: hyphens are not spaces.
        Assert.DoesNotContain(spoken, word => word.Line == 7);

        // Every report prints the file the way the walk prints it and the line the words stand on.
        Assert.All(spoken, word => Assert.StartsWith($"{Path.Combine("Components", "Sample.razor")}:{word.Line} ", word.Report, StringComparison.Ordinal));
    }

    /// <summary>The rule passes over what the code says and over the words of a comment: a Razor call writes the
    /// words at runtime and owns them there, and the doc comments of these very components quote what a card will
    /// read - which is the only way to explain a threshold to the next person without repeating it in markup.</summary>
    [Fact]
    public void TheRuleReadsWhatTheCodeSaysAndSkipsWhatACommentQuotes()
    {
        var text = """
            @* The card reads "5 min ago" and "as of yesterday" - words TimeText and CostText settle on. *@
            <div class="cost">
                <span>@CostText.AsOf(day)</span>
                <time datetime="@TimeText.Iso(at)">@TimeText.Ago(at, Now)</time>
                <span>@CostText.Money(amount, Report.Currency)</span>
                <!-- a note about "this month" that no viewer is shown -->
            </div>
            <!--
                <p>$1.52 as of 2026-10-03</p>
            -->
            @code {
                /// <summary>The moment of this rendering: "yesterday" is counted from it.</summary>
                private DateTimeOffset Now { get; set; }
            }
            """;

        Assert.Empty(Scan(Path.Combine("Components", "Sample.razor"), text));

        // The same text read with its comments left in speaks these words over and over: the sample really carries
        // them, the scan really passed them by, and neither half of the verdict above was reached by matching nothing.
        Assert.True(
            CountOf(text) >= 5,
            $"the sample writes only {CountOf(text)} owned wording(s), so the clean verdict above proves nothing");
    }

    /// <summary>A wording that stands in a file the list does not name is refused all the same, and one that stands
    /// on the list in its own file is not: the pair is what is approved.</summary>
    [Fact]
    public void AnAllowlistEntryApprovesItsOwnFileOnly()
    {
        var allowed = Scan(Path.Combine("Components", "CostLine.razor"), "<span>as of the last change</span>");
        var elsewhere = Scan(Path.Combine("Components", "OtherCard.razor"), "<span>as of the last change</span>");

        Assert.Single(allowed);
        Assert.Single(elsewhere);
        Assert.Empty(OffendersOf(allowed));
        Assert.Single(OffendersOf(elsewhere));

        // The line of the file is the line of the words, and the source is kept for the report.
        var words = Scan(Path.Combine("Components", "Sample.razor"), "one\n  two as of three\n");

        Assert.Single(words);
        Assert.Equal(2, words[0].Line);
        Assert.Contains("two as of three", words[0].Source, StringComparison.Ordinal);
    }

    // --------------------------------------------------------------------------------------------- how it reads

    /// <summary>Every wording the markup speaks, in every component, in the order the files walk.</summary>
    private static List<SpokenWord> SpokenInApp() =>
        [.. RazorMarkup().SelectMany(markup => Scan(markup.File, markup.Text))];

    /// <summary>
    /// The words the markup writes that no line of <see cref="AllowedSpeech" /> approves: an allowance is spent by
    /// one occurrence, so a wording the list names once is refused the second time it stands in the same file, and
    /// the message prints the file and the line to look at.
    /// </summary>
    private static List<string> OffendersOf(IEnumerable<SpokenWord> spoken)
    {
        var left = new Dictionary<(string File, string Words), int>();

        foreach (var entry in AllowedSpeech)
        {
            left.TryGetValue(entry, out var allowance);
            left[entry] = allowance + 1;
        }

        var offenders = new List<string>();

        foreach (var word in spoken)
        {
            if (left.TryGetValue(word.Key, out var allowance) && allowance > 0)
            {
                left[word.Key] = allowance - 1;
                continue;
            }

            offenders.Add(word.Report);
        }

        return offenders;
    }

    /// <summary>The owned wordings a piece of markup speaks, with the file to print and the line each stands on. A
    /// whole component and a sample the tests above read the rule through are scanned the same way, and the same way
    /// round: the comments go first, then the words are looked for, so a quoted "5 min ago" in a doc comment is as
    /// passed over in a sample as it is in the app.</summary>
    private static List<SpokenWord> Scan(string file, string text)
    {
        var written = text.Split('\n');
        var spoken = SpokenWordsOnly(text).Split('\n');
        var found = new List<SpokenWord>();

        for (var line = 0; line < spoken.Length; line++)
        {
            foreach (var word in Owned)
            {
                var at = At(spoken[line], word);

                for (var occurrence = 0; occurrence < at.Count; occurrence++)
                {
                    found.Add(new SpokenWord(file, line + 1, word, written[line].TrimEnd('\r')));
                }
            }
        }

        return found;
    }

    /// <summary>How often a piece of text speaks an owned wording, comments included - the count a scan that read no
    /// file, or matched nothing, cannot reach.</summary>
    private static int CountOf(string text)
    {
        var count = 0;

        foreach (var line in text.Split('\n'))
        {
            foreach (var word in Owned)
            {
                count += At(line, word).Count;
            }
        }

        return count;
    }

    /// <summary>Where a wording stands in one line: the index of every occurrence, read case-free because HTML prose
    /// starts a sentence with a capital and a tooltip may hold the same words in the middle of one. A wording is a
    /// wording and not the tail of a longer one, so a letter glued to either end means the line says something else:
    /// "was of" is not "as of" and "this monthly" is not "this month". A shape is the one exception at its own ends,
    /// where a digit outside it would only say the number is longer.</summary>
    private static List<int> At(string line, OwnedWord word)
    {
        var at = new List<int>();

        if (word.Kind == Kind.Shape)
        {
            for (var start = 0; start + word.Words.Length <= line.Length; start++)
            {
                if (Fits(line, start, word.Words)
                    && (start == 0 || !IsDigit(line[start - 1]))
                    && (start + word.Words.Length >= line.Length || !IsDigit(line[start + word.Words.Length])))
                {
                    at.Add(start);
                }
            }

            return at;
        }

        for (var start = line.IndexOf(word.Words, StringComparison.OrdinalIgnoreCase);
             start >= 0;
             start = start + 1 < line.Length ? line.IndexOf(word.Words, start + 1, StringComparison.OrdinalIgnoreCase) : -1)
        {
            var end = start + word.Words.Length;
            var alone = (start == 0 || !char.IsLetter(line[start - 1]))
                        && (word.Kind == Kind.Lead
                            ? end < line.Length && IsDigit(line[end])
                            : end >= line.Length || !(char.IsLetter(line[end]) || IsDigit(line[end])));

            if (alone)
            {
                at.Add(start);
            }
        }

        return at;
    }

    /// <summary>True when the line reads the shape from <paramref name="start"/> on: every <c>#</c> a digit, every
    /// other character itself - the dashes of a day, which no digit stands for.</summary>
    private static bool Fits(string line, int start, string shape)
    {
        for (var i = 0; i < shape.Length; i++)
        {
            if (shape[i] == '#')
            {
                if (!IsDigit(line[start + i]))
                {
                    return false;
                }
            }
            else if (line[start + i] != shape[i])
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsDigit(char c) => c is >= '0' and <= '9';

    /// <summary>The text a component writes for a viewer: its <c>@* Razor comments *@</c>, its HTML comments and the
    /// <c>//</c> and <c>///</c> lines of its own code blanked out. Everything else keeps its place in the file - the
    /// blanks hold their line breaks, so a wording is reported on the line the file wrote it on.</summary>
    private static string SpokenWordsOnly(string text)
    {
        var chars = Blank(Blank(text, "@*", "*@"), "<!--", "-->").ToCharArray();
        var start = 0;

        while (start < chars.Length)
        {
            var end = Array.IndexOf(chars, '\n', start);
            end = end < 0 ? chars.Length : end;

            if (new string(chars, start, end - start).TrimStart().StartsWith("//", StringComparison.Ordinal))
            {
                for (var at = start; at < end; at++)
                {
                    chars[at] = ' ';
                }
            }

            start = end + 1;
        }

        return new string(chars);
    }

    /// <summary>Every character between the marker that opens a comment and the one that closes it - to the end of
    /// the file when nothing does - turned into a space, its line breaks left where they were.</summary>
    private static string Blank(string text, string open, string close)
    {
        var chars = text.ToCharArray();
        var at = 0;

        while (true)
        {
            var from = text.IndexOf(open, at, StringComparison.Ordinal);

            if (from < 0)
            {
                break;
            }

            var closing = text.IndexOf(close, from + open.Length, StringComparison.Ordinal);
            var to = closing < 0 ? text.Length : closing + close.Length;

            for (var i = from; i < to; i++)
            {
                if (chars[i] != '\n')
                {
                    chars[i] = ' ';
                }
            }

            at = to;
        }

        return new string(chars);
    }

    // ------------------------------------------------------------------------------------- what the code returns

    /// <summary>The words a return puts in front of its first digit: <c>as of</c> out of <c>as of 2026-10-03 (UTC)</c>,
    /// <c>$</c> out of <c>$0.00</c>.</summary>
    private static string BeforeFirstDigit(string text)
    {
        var at = text.AsSpan().IndexOfAnyInRange('0', '9');

        return (at < 0 ? text.AsSpan() : text.AsSpan(0, at)).Trim().ToString();
    }

    /// <summary>The last word a return puts after its number: <c>min</c> out of <c>3 min</c>, <c>ago</c> out of
    /// <c>5 min ago</c>.</summary>
    private static string LastWord(string text) =>
        text.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)[^1];

    /// <summary>The same words with every digit stood in for by a <c>#</c>: the shape the code writes, whatever
    /// number it writes.</summary>
    private static string Shape(string text) =>
        string.Concat([.. text.Select(c => IsDigit(c) ? '#' : c)]);

    /// <summary>The words between two marks of a return: the sign-in a link's suffix promises, which stands between
    /// the <c>; </c> that opens it and the <c>)</c> that ends the whole parenthesis.</summary>
    private static string Between(string text, string open, string close)
    {
        var from = text.IndexOf(open, StringComparison.Ordinal);
        var to = text.IndexOf(close, from + open.Length, StringComparison.Ordinal);

        return text[(from + open.Length)..to];
    }

    // ------------------------------------------------------------------------------------------------- the walk

    private static string DashboardRoot([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard"));

    /// <summary>Every <c>.razor</c> file of the app, as the path the assertion prints and its text. The build's
    /// copies under <c>bin</c> and <c>obj</c> hold the same markup twice, so those directories are walked past.</summary>
    private static IEnumerable<(string File, string Text)> RazorMarkup()
    {
        var root = DashboardRoot();

        foreach (var path in Directory.EnumerateFiles(root, "*.razor", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
        {
            var relative = Path.GetRelativePath(root, path);

            if (relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(part =>
                    part.Equals("bin", StringComparison.OrdinalIgnoreCase) || part.Equals("obj", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            yield return (relative, File.ReadAllText(path));
        }
    }
}
