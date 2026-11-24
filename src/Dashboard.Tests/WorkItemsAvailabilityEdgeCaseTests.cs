using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="WorkItemsAvailability" /> - the four words for what
/// the work items view has to show - and of the two members of <see cref="WorkItemsText" /> that live on them:
/// <see cref="WorkItemsText.Availability(bool, bool, WorkItemsReport?)" />, which settles the three facts of a read
/// on one state, and <see cref="WorkItemsText.Unavailable(WorkItemsAvailability)" />, which says the sentence a
/// state stands for.
/// <para>
/// What is pinned where it belongs is left alone. <see cref="WorkItemsTests.TheEmptyStateSaysWhyThereIsNothing" />
/// pins three rows of the table - no address and nothing asked, an address and nothing asked, an address and an
/// ask that found nothing - each with the sentence the view shows for it;
/// <see cref="WorkItemsTests.AReadReportIsReadyEvenBeforeTheAskIsRemembered" /> that a report in hand is Ready
/// before the ask is remembered; <see cref="WorkItemsTests.AFileThatIsNotThereYetLeavesTheViewEmptyAndFailsNoCheck" />
/// and <see cref="WorkItemsTests.WithoutADeliveryAddressNothingIsReadForWorkItems" /> that the same two states come
/// out of a real round of the monitor; <see cref="WorkItemsTests.AnEmptyObjectIsAReportWithNothingInIt" /> what a
/// file with nothing in it holds; and <see cref="EmptyStateStatusRegionTests" /> that the paragraph the view writes
/// for a state is a status region. The enum's vocabulary is the sibling of
/// <see cref="WorkAudienceEdgeCaseTests" />, which pins the three filters and nothing of these four states.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the four states as the numbers, the words and the order they are
/// declared in, the state a value of the type that was never set stands on - which is the sentence the page shows
/// first - nothing outside the four, the words and the numbers that are and are not a state, the format of every
/// state by name and by digit, the two ends of <see cref="int" />, and the public surface the enum declares. The
/// rows of the table the pinned three leave out: that a report of any size is Ready whatever the address and the
/// ask say - the address having gone away between two rounds, the ask not remembered, the file having arrived with
/// nothing in it - that without an address the ask makes no difference, and that the three facts answer with every
/// one of the four states and with nothing else. And the sentences themselves, which no test has ever written out:
/// the three fixed words letter for letter, that only <see cref="WorkItemsAvailability.Ready" /> says nothing, that
/// no two states say the same thing, that the words are the same in any culture, and that a state which is none of
/// the four is refused by name.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is. Both reports below are read out of a
/// string this file wrote, the twelve rows of the table are that string against two booleans, and a sentence is a
/// fixed word: the same word in German as in English, and the same word at midnight as at noon.
/// </para>
/// </summary>
public class WorkItemsAvailabilityEdgeCaseTests
{
    /// <summary>A file that was read: one item, in a column, with nothing else said.</summary>
    private static readonly WorkItemsReport Given = WorkItemsReport.Parse(
        """{ "generatedAt": "2026-11-09T22:00:00Z", "items": [ { "number": 7, "column": "Todo", "title": "One" } ] }""")!;

    /// <summary>A file that was read and says nothing at all: an empty object is a report, not an absence.</summary>
    private static readonly WorkItemsReport EmptyFile = WorkItemsReport.Parse("{ }")!;

    /// <summary>The four states, in the order the enum declares them: the order a read goes through.</summary>
    private static readonly WorkItemsAvailability[] States =
        [WorkItemsAvailability.Loading, WorkItemsAvailability.NoAddress, WorkItemsAvailability.NotPublished, WorkItemsAvailability.Ready];

    /// <summary>The three answers a read can hand the page: nothing, a file of one item, a file that says nothing.</summary>
    private static readonly WorkItemsReport?[] Reports = [null, Given, EmptyFile];

    // ----- the four states, as the numbers and the words they are

    /// <summary>The four states in the order the enum declares them, on the numbers 0 to 3, of an
    /// <see cref="int" /> underneath, sealed and a choice rather than a set: it wears no
    /// <see cref="FlagsAttribute" />, so no state is built on another.</summary>
    [Fact]
    public void TheFourStatesAreTheWholeVocabularyInTheirDeclaredOrder()
    {
        Assert.Equal(["Loading", "NoAddress", "NotPublished", "Ready"], Enum.GetNames<WorkItemsAvailability>());
        Assert.Equal([0, 1, 2, 3], States.Select(state => (int)state));
        Assert.Equal(4, Enum.GetValues<WorkItemsAvailability>().Length);
        Assert.Equal(States, Enum.GetValues<WorkItemsAvailability>());
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(typeof(WorkItemsAvailability)));

        var type = typeof(WorkItemsAvailability);
        Assert.True(type.IsEnum);
        Assert.True(type.IsValueType);
        Assert.True(type.IsSealed);
        Assert.False(type.IsPrimitive);
        Assert.False(type.IsDefined(typeof(FlagsAttribute), false));
    }

    /// <summary>Each state on its own number and its own word, both ways round: the number is the one the name
    /// stands on, the word is the one that number answers to, and every one of the four is defined however it is
    /// arrived at.</summary>
    [Theory]
    [InlineData(WorkItemsAvailability.Loading, 0, "Loading")]
    [InlineData(WorkItemsAvailability.NoAddress, 1, "NoAddress")]
    [InlineData(WorkItemsAvailability.NotPublished, 2, "NotPublished")]
    [InlineData(WorkItemsAvailability.Ready, 3, "Ready")]
    public void EachStateStandsOnItsOwnNumberAndItsOwnWord(WorkItemsAvailability state, int number, string name)
    {
        Assert.Equal(number, (int)state);
        Assert.Equal(state, (WorkItemsAvailability)number);
        Assert.Equal(name, state.ToString());
        Assert.Equal(name, Enum.GetName(state));
        Assert.True(Enum.IsDefined(state));
        Assert.Equal(TypeCode.Int32, state.GetTypeCode());
    }

    /// <summary>The first state is <see cref="WorkItemsAvailability.Loading" />: the state a value of the type that
    /// was never set stands on. That is the safe end of the boundary - a monitor field that has not been given a
    /// state yet says the page is reading, and not the sentence of a state it has never reached - so the word a
    /// default says out loud is pinned with the default itself.</summary>
    [Fact]
    public void AStateNeverSetStandsOnLoadingAndIsSaidAsLoading()
    {
        Assert.Equal(WorkItemsAvailability.Loading, default);
        Assert.Equal(WorkItemsAvailability.Loading, (WorkItemsAvailability)0);
        Assert.Equal(0, (int)default(WorkItemsAvailability));

        Assert.Equal(WorkItemsText.Loading, WorkItemsText.Unavailable(default));
        Assert.Equal("Loading work items", WorkItemsText.Unavailable(default));
    }

    /// <summary>Nothing outside the four is a state: neither one below the first nor one above the last nor either
    /// end of <see cref="int" />; a value the enum does not name says its number, in decimal in both its letters
    /// and its name form and in the eight hexadecimal digits of the <see cref="int" /> underneath it, and it is no
    /// name at all to <see cref="Enum.GetName{TEnum}(TEnum)" />.</summary>
    [Theory]
    [InlineData(-1, "-1", "FFFFFFFF")]
    [InlineData(4, "4", "00000004")]
    [InlineData(int.MinValue, "-2147483648", "80000000")]
    [InlineData(int.MaxValue, "2147483647", "7FFFFFFF")]
    public void NothingOutsideTheFourIsAState(int number, string digits, string hexadecimal)
    {
        var state = (WorkItemsAvailability)number;

        Assert.False(Enum.IsDefined(state));
        Assert.Null(Enum.GetName(state));
        Assert.Equal(digits, state.ToString());
        Assert.Equal(digits, state.ToString("G"));
        Assert.Equal(digits, state.ToString("D"));
        Assert.Equal(hexadecimal, state.ToString("X"));
    }

    /// <summary>Every state says its own word, by name or by number, in the format letters the enum answers to -
    /// the single and the full name, the number in decimal and in hexadecimal, the absent format and the empty one
    /// both reading as the name - and a letter the enum answers to no one is refused. The two ends of the clock of
    /// a read are <see cref="WorkItemsAvailability.Loading" /> and <see cref="WorkItemsAvailability.Ready" />, and
    /// each is named by its own word alone.</summary>
    [Theory]
    [InlineData(WorkItemsAvailability.Loading, "Loading", "0", "00000000")]
    [InlineData(WorkItemsAvailability.NoAddress, "NoAddress", "1", "00000001")]
    [InlineData(WorkItemsAvailability.NotPublished, "NotPublished", "2", "00000002")]
    [InlineData(WorkItemsAvailability.Ready, "Ready", "3", "00000003")]
    public void EachStateFormatsByNameOrByNumber(WorkItemsAvailability state, string name, string number, string hexadecimal)
    {
        Assert.Equal(name, state.ToString());
        Assert.Equal(name, state.ToString("G"));
        Assert.Equal(name, state.ToString("g"));
        Assert.Equal(name, state.ToString("F"));
        Assert.Equal(name, state.ToString("f"));
        Assert.Equal(name, state.ToString((string?)null));
        Assert.Equal(name, state.ToString(string.Empty));
        Assert.Equal(number, state.ToString("D"));
        Assert.Equal(number, state.ToString("d"));
        Assert.Equal(hexadecimal, state.ToString("X"));
        Assert.Equal(hexadecimal, state.ToString("x"));
        Assert.Throws<FormatException>(() => state.ToString("Q"));

        Assert.Equal(name, typeof(WorkItemsAvailability).GetMethod("ToString", [typeof(IFormatProvider)])!
            .Invoke(state, [CultureInfo.InvariantCulture]));
        Assert.Equal(name, typeof(WorkItemsAvailability).GetMethod("ToString", [typeof(string), typeof(IFormatProvider)])!
            .Invoke(state, ["G", CultureInfo.InvariantCulture]));
    }

    /// <summary>The four states are apart from one another and sort by their numbers, as the declaration orders
    /// them - the order of a read, from the ask to the answer. A boxed <see cref="int" /> on the same number is no
    /// state's equal, and neither is a box of nothing, nor a word, nor a state of another enum standing on the
    /// same number.</summary>
    [Fact]
    public void TheStatesAreApartFromOneAnotherAndSortByTheirNumber()
    {
        Assert.True(WorkItemsAvailability.Loading < WorkItemsAvailability.NoAddress);
        Assert.True(WorkItemsAvailability.NoAddress < WorkItemsAvailability.NotPublished);
        Assert.True(WorkItemsAvailability.NotPublished < WorkItemsAvailability.Ready);

        Assert.Equal(1, WorkItemsAvailability.Ready.CompareTo(WorkItemsAvailability.NoAddress));
        Assert.Equal(-1, WorkItemsAvailability.Loading.CompareTo(WorkItemsAvailability.Ready));
        Assert.Equal(0, WorkItemsAvailability.NotPublished.CompareTo((object)WorkItemsAvailability.NotPublished));
        Assert.Equal(1, WorkItemsAvailability.NotPublished.CompareTo(null));
        Assert.Throws<ArgumentException>(() => WorkItemsAvailability.Ready.CompareTo(3));
        Assert.Throws<ArgumentException>(() => WorkItemsAvailability.Ready.CompareTo("Ready"));

        Assert.False(WorkItemsAvailability.Ready.Equals(null));
        Assert.False(WorkItemsAvailability.Ready.Equals((object)3));
        Assert.False(WorkItemsAvailability.Ready.Equals((object)WorkAudience.Team));
        Assert.True(WorkItemsAvailability.Ready.Equals((object)WorkItemsAvailability.Ready));
        Assert.Equal(4, States.Distinct().Count());
        Assert.Equal(WorkItemsAvailability.Loading, States.Min());
        Assert.Equal(WorkItemsAvailability.Ready, States.Max());
        Assert.All(States, state => Assert.Equal(state.GetHashCode(), ((object)state).GetHashCode()));
    }

    /// <summary>A state is a choice and not a set, so the operators work on the numbers underneath - and on these
    /// four numbers the work they do is nonsense the type will not catch: the bits of
    /// <see cref="WorkItemsAvailability.NoAddress" /> and <see cref="WorkItemsAvailability.NotPublished" /> make
    /// exactly <see cref="WorkItemsAvailability.Ready" />, which is defined and so answers to
    /// <see cref="Enum.IsDefined{TEnum}(TEnum)" />; their intersection is the zero of
    /// <see cref="WorkItemsAvailability.Loading" />, which a flag test therefore says of every state; and one step
    /// past either end is a number the enum does not name.</summary>
    [Fact]
    public void TwoStatesCombinedAreANumberAndNotAReading()
    {
        var sum = WorkItemsAvailability.NoAddress | WorkItemsAvailability.NotPublished;
        Assert.Equal(WorkItemsAvailability.Ready, sum);
        Assert.True(Enum.IsDefined(sum));
        Assert.Equal(WorkItemsAvailability.Loading, WorkItemsAvailability.NoAddress & WorkItemsAvailability.NotPublished);
        Assert.Equal(WorkItemsAvailability.NoAddress, WorkItemsAvailability.NotPublished ^ WorkItemsAvailability.Ready);

        Assert.Equal(WorkItemsAvailability.Ready, WorkItemsAvailability.Loading | WorkItemsAvailability.Ready);
        Assert.Equal(WorkItemsAvailability.Loading, WorkItemsAvailability.Loading | WorkItemsAvailability.Loading);
        Assert.Equal((WorkItemsAvailability)4, WorkItemsAvailability.Ready + 1);
        Assert.False(Enum.IsDefined((WorkItemsAvailability)4));
        Assert.Equal((WorkItemsAvailability)(-1), WorkItemsAvailability.Loading - 1);
        Assert.False(Enum.IsDefined((WorkItemsAvailability)(-1)));

        Assert.True(WorkItemsAvailability.Ready.HasFlag(WorkItemsAvailability.Loading));
        Assert.True(WorkItemsAvailability.Ready.HasFlag(WorkItemsAvailability.NoAddress));
        Assert.False(WorkItemsAvailability.Loading.HasFlag(WorkItemsAvailability.Ready));
    }

    // ----- text that is or is not a state

    /// <summary>Text that names a state in the enum's own case, its surrounding spaces walked past, or holds its
    /// number is that state - and a number with a cipher in front of it is still the small number it says.</summary>
    [Theory]
    [InlineData("Loading", WorkItemsAvailability.Loading)]
    [InlineData("NoAddress", WorkItemsAvailability.NoAddress)]
    [InlineData("NotPublished", WorkItemsAvailability.NotPublished)]
    [InlineData("Ready", WorkItemsAvailability.Ready)]
    [InlineData("  Ready  ", WorkItemsAvailability.Ready)]
    [InlineData("\tNotPublished\n", WorkItemsAvailability.NotPublished)]
    [InlineData("0", WorkItemsAvailability.Loading)]
    [InlineData("3", WorkItemsAvailability.Ready)]
    [InlineData("01", WorkItemsAvailability.NoAddress)]
    public void AStateTextIsTheStateItNamesOrTheStateItNumbers(string text, WorkItemsAvailability expected)
    {
        Assert.True(Enum.TryParse<WorkItemsAvailability>(text, out var state));
        Assert.Equal(expected, state);
        Assert.True(Enum.IsDefined(state));
        Assert.Equal(expected, Enum.Parse<WorkItemsAvailability>(text));
    }

    /// <summary>Text that is no state is refused - <see langword="null" />, the empty string, a blank of spaces or
    /// of a tab, a name in a case the enum does not spell, a name with something behind it, a plural, a fraction, a
    /// hexadecimal, a number the <see cref="int" /> underneath cannot hold, and a word of the page's own - and a
    /// refused parse leaves its answer standing on <see cref="WorkItemsAvailability.Loading" />, the first of the
    /// four: the <c>false</c> is the whole answer, and a caller that reads the value anyway is shown the state a
    /// page takes before it has asked anything.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("loading")]
    [InlineData("READY")]
    [InlineData("Ready!")]
    [InlineData("Loadings")]
    [InlineData("Not Published")]
    [InlineData("1.0")]
    [InlineData("0x1")]
    [InlineData("2147483648")]
    [InlineData("workitems")]
    public void TextThatIsNoStateIsRefusedAndLeavesLoadingBehind(string? text)
    {
        Assert.False(Enum.TryParse<WorkItemsAvailability>(text, out var state));
        Assert.Equal(WorkItemsAvailability.Loading, state);
    }

    /// <summary>A parse that must answer says what is wrong: <see langword="null" /> is no text at all, the empty
    /// string and a blank are text without a state in them, and a name in a case the enum does not spell is
    /// refused - until the parse is told to forgive the case, which is the one way "ready" becomes
    /// <see cref="WorkItemsAvailability.Ready" />.</summary>
    [Fact]
    public void ParsingTextThatIsNoStateThrows()
    {
        Assert.Throws<ArgumentNullException>(() => Enum.Parse<WorkItemsAvailability>(null!));
        Assert.Throws<ArgumentException>(() => Enum.Parse<WorkItemsAvailability>(string.Empty));
        Assert.Throws<ArgumentException>(() => Enum.Parse<WorkItemsAvailability>("   "));
        Assert.Throws<ArgumentException>(() => Enum.Parse<WorkItemsAvailability>("ready"));

        Assert.False(Enum.TryParse<WorkItemsAvailability>("ready", out _));
        Assert.True(Enum.TryParse<WorkItemsAvailability>("ready", ignoreCase: true, out var forgiven));
        Assert.Equal(WorkItemsAvailability.Ready, forgiven);
        Assert.True(Enum.TryParse<WorkItemsAvailability>("NOTPUBLISHED", ignoreCase: true, out var whole));
        Assert.Equal(WorkItemsAvailability.NotPublished, whole);
    }

    // ----- which state the three facts settle on

    /// <summary>A report in hand is <see cref="WorkItemsAvailability.Ready" /> whatever the other two facts say -
    /// the rows <see cref="WorkItemsTests.AReadReportIsReadyEvenBeforeTheAskIsRemembered" /> leaves out. The
    /// address having gone away between two rounds does not un-read a file, and an ask that has not been
    /// remembered says nothing against an answer that has: the report is asked first, and its own size last - the
    /// file that arrived and says nothing is a file that was read, not one that is not published.</summary>
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void AReportInHandIsReadyWhateverTheAddressAndTheAskSay(bool hasAddress, bool asked)
    {
        Assert.Equal(WorkItemsAvailability.Ready, WorkItemsText.Availability(hasAddress, asked, Given));
        Assert.Equal(WorkItemsAvailability.Ready, WorkItemsText.Availability(hasAddress, asked, EmptyFile));
    }

    /// <summary>A file with nothing in it has been read: it is
    /// <see cref="WorkItemsAvailability.Ready" /> and not <see cref="WorkItemsAvailability.NotPublished" />, so the
    /// page is left to say that the board is empty rather than that the file is missing. The boundary is the
    /// report, not what the report holds.</summary>
    [Fact]
    public void AFileThatSaysNothingIsAFileThatWasRead()
    {
        Assert.Empty(EmptyFile.Items);
        Assert.Null(EmptyFile.Scope);

        Assert.Equal(WorkItemsAvailability.Ready, WorkItemsText.Availability(true, true, EmptyFile));
        Assert.NotEqual(WorkItemsAvailability.NotPublished, WorkItemsText.Availability(true, true, EmptyFile));
        Assert.Equal(WorkItemsAvailability.Ready, WorkItemsText.Availability(true, true, Given));
    }

    /// <summary>Where the topology names no file there is nothing to ask and nothing to wait for: the row the
    /// pinned table leaves out - no address, and the ask remembered - is
    /// <see cref="WorkItemsAvailability.NoAddress" /> too, so the second fact never reaches a page that has no
    /// address. Without an address the answer is the same whether the ask happened or not, and it is never the
    /// loading of a read under way nor the absence of a read that found nothing.</summary>
    [Fact]
    public void WithoutAnAddressTheAskMakesNoDifference()
    {
        Assert.Equal(WorkItemsAvailability.NoAddress, WorkItemsText.Availability(false, true, null));

        foreach (var asked in new[] { false, true })
        {
            Assert.Equal(WorkItemsAvailability.NoAddress, WorkItemsText.Availability(false, asked, null));
        }

        Assert.Equal(WorkItemsText.Availability(false, false, null), WorkItemsText.Availability(false, true, null));
        Assert.NotEqual(WorkItemsAvailability.Loading, WorkItemsText.Availability(false, false, null));
        Assert.NotEqual(WorkItemsAvailability.NotPublished, WorkItemsText.Availability(false, true, null));
    }

    /// <summary>The three facts answer with every one of the four states and with nothing else: the twelve rows -
    /// no report, a report of one item, a report of nothing, each against an address and an ask in both their
    /// values - hold no gap and no fifth answer: every row is asked, every row answers with a state the enum
    /// names, every row's state has a sentence to show and only Ready's has none, and the four states are all
    /// four answered. What each row settles on is pinned row by row above and in <see cref="WorkItemsTests" />;
    /// what is pinned here is that there is nothing the table can hand the view that the view has no sentence
    /// for.</summary>
    [Fact]
    public void TheThreeFactsAnswerWithEveryStateAndWithNothingElse()
    {
        var answers = new HashSet<WorkItemsAvailability>();
        var rows = 0;

        foreach (var report in Reports)
        {
            foreach (var hasAddress in new[] { false, true })
            {
                foreach (var asked in new[] { false, true })
                {
                    var state = WorkItemsText.Availability(hasAddress, asked, report);
                    Assert.True(Enum.IsDefined(state), $"({hasAddress}, {asked}) answers {state.ToString("D")}, which is no state.");

                    var sentence = WorkItemsText.Unavailable(state);
                    if (state == WorkItemsAvailability.Ready)
                    {
                        Assert.Equal(string.Empty, sentence);
                    }
                    else
                    {
                        Assert.False(string.IsNullOrWhiteSpace(sentence));
                    }

                    answers.Add(state);
                    rows++;
                }
            }
        }

        Assert.Equal(12, rows);
        Assert.Equal(4, answers.Count);
        Assert.All(States, state => Assert.Contains(state, answers));
    }

    // ----- the sentence a state says

    /// <summary>The sentence of each state, word for word: the three fixed words of the class and, at
    /// <see cref="WorkItemsAvailability.Ready" />, nothing at all. The words are written out here on purpose, in
    /// the file's own wording - a change to one of them is a change a reader sees, so it has to be said twice
    /// before it happens; <see cref="WorkItemsTests.TheEmptyStateSaysWhyThereIsNothing" /> pins that each state
    /// says the constant of its name, which is the pairing, not the wording.</summary>
    [Theory]
    [InlineData(WorkItemsAvailability.Loading, "Loading work items")]
    [InlineData(WorkItemsAvailability.NoAddress, "The topology names no delivery file, so there is nowhere to read work items.")]
    [InlineData(WorkItemsAvailability.NotPublished, "Work items are not published yet. The page reads workitems.json next to the delivery facts, and that file is not there yet.")]
    [InlineData(WorkItemsAvailability.Ready, "")]
    public void EachStateSaysItsOwnSentenceWordForWord(WorkItemsAvailability state, string sentence) =>
        Assert.Equal(sentence, WorkItemsText.Unavailable(state));

    /// <summary>The three words are the fixed values they are, letter for letter, as the constants the class gives
    /// them: <see cref="WorkItemsText.Loading" /> is the one of the three that ends on a word rather than on a full
    /// stop, because a page that is still reading has not got a sentence about a file, and the two that explain
    /// name what they explain - the topology, and <c>workitems.json</c> beside the delivery facts. Each is one
    /// line, trimmed, and free of a double space.</summary>
    [Fact]
    public void TheThreeWordsAreTheFixedValuesOfTheClass()
    {
        Assert.Equal("Loading work items", WorkItemsText.Loading);
        Assert.Equal("The topology names no delivery file, so there is nowhere to read work items.", WorkItemsText.NoAddress);
        Assert.Equal(
            "Work items are not published yet. The page reads workitems.json next to the delivery facts, and that file is not there yet.",
            WorkItemsText.NotPublished);

        var words = new[] { WorkItemsText.Loading, WorkItemsText.NoAddress, WorkItemsText.NotPublished };
        Assert.All(words, word =>
        {
            Assert.False(string.IsNullOrWhiteSpace(word));
            Assert.Equal(word, word.Trim());
            Assert.DoesNotContain("\n", word, StringComparison.Ordinal);
            Assert.DoesNotContain("  ", word, StringComparison.Ordinal);
        });

        Assert.Equal('s', WorkItemsText.Loading[^1]);
        Assert.Equal('.', WorkItemsText.NoAddress[^1]);
        Assert.Equal('.', WorkItemsText.NotPublished[^1]);
        Assert.StartsWith("Loading", WorkItemsText.Loading, StringComparison.Ordinal);
        Assert.Contains("topology", WorkItemsText.NoAddress, StringComparison.Ordinal);
        Assert.Contains("workitems.json", WorkItemsText.NotPublished, StringComparison.Ordinal);
    }

    /// <summary>The empty sentence belongs to one state and to no other:
    /// <see cref="WorkItemsAvailability.Ready" /> says nothing - the empty string, not a space, not a line, not
    /// <see langword="null" />, because the view has the board to show - and each of the other three says a word
    /// that is its own, trimmed to the letters it is written in.</summary>
    [Fact]
    public void OnlyReadyHasNoSentence()
    {
        var nothing = WorkItemsText.Unavailable(WorkItemsAvailability.Ready);

        Assert.Equal(string.Empty, nothing);
        Assert.True(string.IsNullOrEmpty(nothing));
        Assert.DoesNotContain(" ", nothing, StringComparison.Ordinal);
        Assert.DoesNotContain(".", nothing, StringComparison.Ordinal);

        foreach (var state in States.Where(state => state != WorkItemsAvailability.Ready))
        {
            var sentence = WorkItemsText.Unavailable(state);
            Assert.False(string.IsNullOrWhiteSpace(sentence));
            Assert.Equal(sentence, sentence.Trim());
        }
    }

    /// <summary>No two states say the same thing, in the letters they are said in or in any other case, and no one
    /// of the three is a piece of another: the view chooses a state and shows its sentence, so two states that
    /// read the same would be a state the page cannot tell apart.</summary>
    [Fact]
    public void NoTwoStatesSayTheSameThing()
    {
        var sentences = States.Select(WorkItemsText.Unavailable).ToList();

        Assert.Equal(4, sentences.Count);
        Assert.Equal(4, sentences.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(4, sentences.Distinct(StringComparer.OrdinalIgnoreCase).Count());

        for (var first = 0; first < sentences.Count; first++)
        {
            for (var second = first + 1; second < sentences.Count; second++)
            {
                Assert.NotEqual(sentences[first], sentences[second]);
                if (sentences[first].Length > 0 && sentences[second].Length > 0)
                {
                    Assert.DoesNotContain(sentences[first], sentences[second], StringComparison.Ordinal);
                }
            }
        }
    }

    /// <summary>The sentence is a fixed word and not a formatting job: nothing in it follows the reader's culture,
    /// so a reader of German - a culture that writes a decimal point as a comma and quotes differently - is shown
    /// the same English letters, and a state asked twice is answered with the same word twice.</summary>
    [Fact]
    public void ASentenceIsTheSameWordInAnyCulture()
    {
        var before = States.Select(WorkItemsText.Unavailable).ToList();

        ViewerCulture.German(() =>
        {
            Assert.Equal(before, States.Select(WorkItemsText.Unavailable).ToList());
            Assert.Equal("Loading work items", WorkItemsText.Loading);
        });

        Assert.Equal(before, States.Select(WorkItemsText.Unavailable).ToList());
    }

    /// <summary>A state that is none of the four has no sentence, and the class says so rather than inventing one:
    /// <see cref="ArgumentOutOfRangeException" /> naming the argument it was given, holding the very value it
    /// refused, and saying in its message that a work items state has no sentence - one below the first, one above
    /// the last, and either end of <see cref="int" />.</summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void AStateThatIsNoneOfTheFourHasNoSentence(int number)
    {
        var state = (WorkItemsAvailability)number;

        var error = Assert.Throws<ArgumentOutOfRangeException>(() => WorkItemsText.Unavailable(state));

        Assert.Equal("availability", error.ParamName);
        Assert.Equal<object>(state, error.ActualValue!);
        Assert.Contains("A work items state has no sentence.", error.Message, StringComparison.Ordinal);
    }

    // ----- the surface, so that a fifth state is noticed where it is added

    /// <summary>The whole of what <see cref="WorkItemsAvailability" /> declares itself, as the running assembly
    /// holds it: an <see cref="int" /> underneath, sealed and no set, and exactly its four states and nothing else -
    /// each one a public static literal of its own type, in the order the declaration writes them, on the number it
    /// stands on; no instance member of its own, so a state carries nothing beyond the number it is. A fifth state
    /// has to be written here as well, which is the point: the view cannot be left with a state it has no sentence
    /// for.</summary>
    [Fact]
    public void TheEnumDeclaresItsFourStatesAndNothingElse()
    {
        var type = typeof(WorkItemsAvailability);

        var instance = type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Single(instance);
        Assert.Equal("value__", instance[0].Name);
        Assert.Equal(typeof(int), ((FieldInfo)instance[0]).FieldType);

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.Equal(["Loading", "NoAddress", "NotPublished", "Ready"], fields.Select(field => field.Name));
        Assert.Equal([0, 1, 2, 3], fields.Select(field => (int)field.GetRawConstantValue()!));
        Assert.All(fields, field =>
        {
            Assert.Equal(type, field.FieldType);
            Assert.True(field.IsPublic);
            Assert.True(field.IsStatic);
            Assert.True(field.IsLiteral);
            Assert.False(field.IsInitOnly);
        });
    }

    /// <summary>The words of the states live on the class that decides them, and only there: the three constants of
    /// the state's names - <c>Loading</c>, <c>NoAddress</c>, <c>NotPublished</c> - as the public string literals
    /// they are, and no constant for <c>Ready</c>, whose sentence is nothing and is written where it is said. Two
    /// members of <see cref="WorkItemsText" /> touch the state at all, the pair of them: the settling and the
    /// saying, both static, both public, with the arguments the page gives them named and in the order it gives
    /// them.</summary>
    [Fact]
    public void TheWordsAndTheTwoMembersAreTheWholeSurfaceOfTheState()
    {
        var fields = typeof(WorkItemsText)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(field => Enum.TryParse(field.Name, out WorkItemsAvailability _))
            .ToList();

        Assert.Equal(["Loading", "NoAddress", "NotPublished"], fields.Select(field => field.Name).Order(StringComparer.Ordinal));
        Assert.All(fields, field =>
        {
            Assert.Equal(typeof(string), field.FieldType);
            Assert.True(field.IsLiteral);
            Assert.Equal(WorkItemsText.Unavailable(Enum.Parse<WorkItemsAvailability>(field.Name)), (string)field.GetRawConstantValue()!);
        });
        Assert.DoesNotContain("Ready", fields.Select(field => field.Name), StringComparer.Ordinal);

        var methods = typeof(WorkItemsText)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Where(method => method.ReturnType == typeof(WorkItemsAvailability)
                             || method.GetParameters().Any(parameter => parameter.ParameterType == typeof(WorkItemsAvailability)))
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToList();

        Assert.Equal(["Availability", "Unavailable"], methods);

        var settle = typeof(WorkItemsText).GetMethod("Availability", [typeof(bool), typeof(bool), typeof(WorkItemsReport)])!;
        Assert.True(settle.IsStatic);
        Assert.True(settle.IsPublic);
        Assert.Equal(typeof(WorkItemsAvailability), settle.ReturnType);
        Assert.Equal(["hasAddress", "asked", "report"], settle.GetParameters().Select(parameter => parameter.Name));

        var say = typeof(WorkItemsText).GetMethod("Unavailable", [typeof(WorkItemsAvailability)])!;
        Assert.True(say.IsStatic);
        Assert.True(say.IsPublic);
        Assert.Equal(typeof(string), say.ReturnType);
        Assert.Equal(["availability"], say.GetParameters().Select(parameter => parameter.Name));
    }

    /// <summary>The shape of the member that says the sentences, read out of the file it is written in: the refusal
    /// of a value that is no state comes first, and the switch after it names the four states and answers for no
    /// other value at all.</summary>
    /// <para>
    /// Why the shape and not the behaviour, which the rows above already pin: it is the compiler that has to notice
    /// a fifth state, not the reader, who would first have to remember what the four were. Every state is therefore
    /// named by its own arm, and there is no <c>_ =&gt;</c> to take in what the names do not: a state added to
    /// <see cref="WorkItemsAvailability" /> is a switch that has stopped being exhaustive, which warns CS8509, and a
    /// warning is an error in this repository (<c>Directory.Build.props</c>), so the addition cannot be merged with
    /// its sentence unwritten. An arm for anything else would say nothing to the compiler and, worse, say something
    /// to the page: a sentence invented for a state nobody wrote one for.
    /// </para>
    /// <para>
    /// The refusal of a value the enum does not name comes before the switch, and is pinned letter for letter with
    /// it - the argument, the value held on to, the words <c>A work items state has no sentence.</c> - because
    /// something still has to answer for the numbers the four are not (<see
    /// cref="AStateThatIsNoneOfTheFourHasNoSentence(int)" /> pins what it answers), and the switch answers for none
    /// of them by design. Either the guard comes first or the switch has to answer for such a value itself; what it
    /// must not do is run past its own arms and land on a default. So from the switch to the end of the body there
    /// is neither a <c>_ =&gt;</c> nor a <c>throw</c> waiting under it.</para>
    [Fact]
    public void TheSwitchNamesEveryStateAndHasNoArmForAnyOther()
    {
        var source = File.ReadAllText(WorkItemsTextSource());
        var body = MethodBody(source, "public static string Unavailable(WorkItemsAvailability availability)");

        Assert.Contains("if (!Enum.IsDefined(availability))", body, StringComparison.Ordinal);
        Assert.Contains(
            """throw new ArgumentOutOfRangeException(nameof(availability), availability, "A work items state has no sentence.");""",
            body,
            StringComparison.Ordinal);

        var refusal = body.IndexOf("Enum.IsDefined", StringComparison.Ordinal);
        var arms = body.IndexOf("switch", StringComparison.Ordinal);
        Assert.True(
            refusal >= 0 && arms >= 0 && refusal < arms,
            "the refusal of a value that is no state has to come before the switch, or the switch has to answer for such a value itself");

        foreach (var name in Enum.GetNames<WorkItemsAvailability>())
        {
            Assert.Contains($"WorkItemsAvailability.{name} =>", body, StringComparison.Ordinal);
        }

        var fromTheSwitch = body[arms..];
        Assert.DoesNotContain("_ =>", fromTheSwitch, StringComparison.Ordinal);
        Assert.DoesNotContain("throw", fromTheSwitch, StringComparison.Ordinal);
    }

    /// <summary>The view asks the three facts once, and shows the state's own sentence in the paragraph it has a
    /// status region for: no wording of its own, so the day a sentence moves the page moves with it. The state the
    /// view asks for is the one the monitor's three facts answer with - the address it has, the ask it remembers,
    /// the report it holds - and the paragraph stands while the state is anything but
    /// <see cref="WorkItemsAvailability.Ready" />.</summary>
    [Fact]
    public void TheViewAsksTheStateOnceAndSaysItsSentence()
    {
        var text = File.ReadAllText(WorkItemsViewComponent());

        Assert.Equal(1, CountOccurrences(text, "WorkItemsText.Availability("));
        Assert.Contains("var availability = WorkItemsText.Availability(", text, StringComparison.Ordinal);
        Assert.Contains("var report = Monitor.WorkItems;", text, StringComparison.Ordinal);
        Assert.Contains("""@if (availability != WorkItemsAvailability.Ready || report is null)""", text, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(text, "WorkItemsText.Unavailable("));
        Assert.Contains("""<p class="empty" role="status">@WorkItemsText.Unavailable(availability)</p>""", text, StringComparison.Ordinal);
    }

    /// <summary>How many times a word is said in a file - the count the two lines above are pinned at, so a second
    /// place that settles a state or says one is a second place a rewording can be forgotten in. The same local
    /// helper as <see cref="AppHeaderTests" /> and the other markup guards, in the name they all use for it.</summary>
    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        for (var index = 0; (index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0; index += value.Length)
        {
            count++;
        }

        return count;
    }

    /// <summary>The component that places these words, read the way
    /// <see cref="WorkItemsViewTests" /> reads it.</summary>
    private static string WorkItemsViewComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "WorkItemsView.razor"));

    /// <summary>The file the words of a state are written in, read the way <see cref="WorkItemsViewComponent" /> reads
    /// the file they are placed in. What is guarded here is the shape of a member, which reflection cannot see: the
    /// arms it writes, in what order, and what it leaves unwritten.</summary>
    private static string WorkItemsTextSource([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Work", "WorkItemsText.cs"));

    /// <summary>The text of one method: from its signature to the brace that closes it, with the braces counted, so a
    /// block the method opens belongs to the body and the body ends where that method ends rather than at the first
    /// brace that happens to look like it. A signature the file does not have is said out loud - a member that moved
    /// or was renamed is a guard that reads nothing, and silence here would read as a shape that holds.</summary>
    private static string MethodBody(string source, string signature)
    {
        var start = source.IndexOf(signature, StringComparison.Ordinal);
        Assert.True(start >= 0, $"{signature} was not found where the sentences are written");

        var open = source.IndexOf('{', start);
        Assert.True(open > start, $"the body of {signature} was not found");

        var depth = 0;
        for (var index = open; index < source.Length; index++)
        {
            if (source[index] == '{')
            {
                depth++;
            }
            else if (source[index] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return source[start..(index + 1)];
                }
            }
        }

        throw new InvalidOperationException($"the body of {signature} never closes");
    }
}
