using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="HealthDetailText" /> - the words the tiles and the
/// diagram make of a node's detailed health check, as <c>src/Dashboard/Health/HealthDetail.cs</c> writes them: the
/// word of a state (<see cref="HealthDetailText.Label" />), its place in the order of the worst first
/// (<see cref="HealthDetailText.Severity" />), the shape of its mark (<see cref="HealthDetailText.Icon" />), the
/// tooltip of a mark (<see cref="HealthDetailText.Title" />), a length of checking in words
/// (<see cref="HealthDetailText.Duration" />), the one short line of a whole answer
/// (<see cref="HealthDetailText.Summary" />), a cut to what a line of the diagram holds
/// (<see cref="HealthDetailText.Brief" />) and the list of who is the reason
/// (<see cref="HealthDetailText.Failed" />).
/// <para>
/// What the other files already pin is left alone. <see cref="HealthDetailTests" /> pins the four state words for the
/// four states, the tooltip of an entry with its words and its milliseconds and of one with neither, five lengths of
/// checking from <c>0.0006 ms</c> to <c>4.2 s</c>, the short line of one, three and five checks, of the one entry
/// that is not healthy and of a name of thirty-one characters, four cuts of a text to thirty-six characters, and the
/// reason of the shipped answer's two failing entries word for word.
/// <see cref="HealthDetailTextHelperTests" /> pins the shape and the place of each of the four states, and that the
/// four make four different marks in four different places. <see cref="CheckStateEdgeCaseTests" /> pins the states
/// themselves, <see cref="HealthCheckEntryEdgeCaseTests" /> the entry the words are made of - including the tooltip
/// of a healthy entry and a second's worth of milliseconds read by a German.
/// <see cref="TargetTileClusterCardEnvironmentDeliveryTests" /> pins the tooltip of an entry whose words end in no
/// full stop, the reason of two failing entries and the line of four healthy ones, and where on the tile each word
/// stands; <see cref="DependencyTests" /> and <see cref="RuntimePayloadBuilderTests" /> the cut of a description to
/// thirty-six characters and of a name to fourteen, in the diagram's lines; and
/// <see cref="EventDetectorEdgeCaseTests" /> that the events of the entries say an empty description as the blank it
/// is - which is the events' own wording, not this class's.
/// </para>
/// <para>
/// What is left, and is pinned here, is the edge of each of those sayers: a state the enum does not name, which is
/// said, sorted, drawn, tooltiped, summarised and listed exactly as the state that is not known; the parts an entry
/// leaves unsaid, and the full stop the tooltip adds to the words - one, only where one is missing, and never to
/// whitespace; the milliseconds at the two ends of the tenth of a millisecond the words stop counting, the tenth a
/// number is rounded to, the thousandth millisecond the second begins at, the milliseconds the rounding carries onto
/// <c>1000 ms</c> without ever crossing onto a second, the seconds the words never carry further than, the number
/// that is no number and the two that are no length, and the largest number a real holds said out in full; the short
/// line of an answer that names nothing, of one where every entry fails, of a name of exactly the twenty characters
/// a line holds and of one character more, of a name in no letters of its own, and the answer that is not there; the
/// cut of the brief at nothing, at one character and at the length a sentence is exactly as long as, the full stop
/// that splits a sentence only when a space follows it, the blank that is trimmed off before the ellipsis, the half
/// of a symbol an ellipsis lands after, the text that is nothing and the length that is less than nothing; the list
/// of who failed when an entry's words are the empty word, a blank or nothing but full stops, and when three of four
/// entries fail; and the whole public surface of the class, so that the day a ninth word is added the list below
/// says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: none of these words counts an age -
/// the moment below only stands in the record the words are made of, and every state, text and number is one the
/// test brings.
/// </para>
/// </summary>
public class HealthDetailTextEdgeCaseTests
{
    /// <summary>A fixed moment: none of these words is counted from it, and it is here only to make an answer.</summary>
    private static readonly DateTimeOffset ReadAt = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>An entry of the four parts a caller names, the last two left out when the caller names none.</summary>
    private static HealthCheckEntry Entry(string name, CheckState state, string? description = null, double? durationMs = null) =>
        new(name, state, description, durationMs);

    /// <summary>An answer of the entries given, in the order given, read at the fixed moment above.</summary>
    private static HealthDetail Detail(params HealthCheckEntry[] entries) => new(entries, ReadAt);

    // ------------------------------------------------- a state the enum does not name, in every one of its words

    /// <summary>A number the enum does not name is a state to every one of the three sayers of a state, and it is
    /// said, sorted and drawn as the state that is not known: the word <c>Not known</c>, the last of the four places
    /// (the same number <see cref="CheckState.Healthy" /> stands on, so a state nobody named never leads the worst
    /// first), and the shape of nothing answered yet. The four named states and their words are pinned where they
    /// are said; what no test held was that the switch's own <c>default</c> arms land all three on the unknown.</summary>
    [Theory]
    [InlineData((CheckState)(-1))]
    [InlineData((CheckState)4)]
    [InlineData((CheckState)7)]
    [InlineData((CheckState)int.MaxValue)]
    [InlineData((CheckState)int.MinValue)]
    public void AStateTheEnumDoesNotNameIsSaidAndSortedAndDrawnAsTheStateThatIsNotKnown(CheckState state)
    {
        Assert.Equal("Not known", HealthDetailText.Label(state));
        Assert.Equal(HealthDetailText.Label(CheckState.Unknown), HealthDetailText.Label(state));

        Assert.Equal(3, HealthDetailText.Severity(state));
        Assert.Equal(HealthDetailText.Severity(CheckState.Healthy), HealthDetailText.Severity(state));
        Assert.All(Enum.GetValues<CheckState>(), named => Assert.True(HealthDetailText.Severity(state) >= HealthDetailText.Severity(named)));

        Assert.Equal(HealthState.Pending, HealthDetailText.Icon(state));
        Assert.Equal(HealthDetailText.Icon(CheckState.Unknown), HealthDetailText.Icon(state));
    }

    /// <summary>The state that is not known is said the same way in the three longer words: the tooltip of an entry
    /// of it ends at the state when the entry says no more, the short line names the entry and the lower-cased word,
    /// and the list of who failed wraps the word in brackets with nothing after the state.</summary>
    [Fact]
    public void AStateNobodyNamedIsSaidInTheLongerWordsAsTheStateThatIsNotKnown()
    {
        var odd = Entry("Odd", (CheckState)4);

        Assert.Equal("Odd: Not known.", HealthDetailText.Title(odd));
        Assert.Equal("Odd not known", HealthDetailText.Summary(Detail(odd)));
        Assert.Equal("Not healthy: Odd (Not known)", HealthDetailText.Failed(Detail(odd)));
    }

    // ------------------------------------------------------------------------ the tooltip of a mark

    /// <summary>The tooltip refuses an entry that is not there, by the name the guard keeps; the empty answer is no
    /// tooltip either, so a caller cannot be shown a mark of nothing.</summary>
    [Fact]
    public void TheTooltipRefusesAnEntryThatIsNotThere() =>
        Assert.Equal("entry", Assert.Throws<ArgumentNullException>(() => HealthDetailText.Title(null!)).ParamName);

    /// <summary>The two parts an entry may leave unsaid are left unsaid and not said as blanks: the words without the
    /// milliseconds end at the words, the milliseconds without the words follow the state, an entry of neither is the
    /// state and nothing else, and an entry whose words are the empty word is said with a full stop of its own - the
    /// empty text is a text this class has to put somewhere, and it lands between two full stops.</summary>
    [Fact]
    public void TheTooltipSaysThePartsAnEntryLeavesUnsaidAsUnsaid()
    {
        Assert.Equal("DataAccess: Unhealthy. Login failed.", HealthDetailText.Title(Entry("DataAccess", CheckState.Unhealthy, "Login failed")));
        Assert.Equal("DataAccess: Unhealthy. Took 42.1 ms.", HealthDetailText.Title(Entry("DataAccess", CheckState.Unhealthy, null, 42.05)));
        Assert.Equal("DataAccess: Unhealthy.", HealthDetailText.Title(Entry("DataAccess", CheckState.Unhealthy)));
        Assert.Equal("API: Healthy. .", HealthDetailText.Title(Entry("API", CheckState.Healthy, string.Empty)));
        Assert.Equal("API: Healthy. . Took 0.4 ms.", HealthDetailText.Title(Entry("API", CheckState.Healthy, string.Empty, 0.44)));
    }

    /// <summary>The full stop the tooltip puts behind the entry's words is put there only when the words have none:
    /// one is kept, a row of them is kept whole, a space after the last of them is no full stop at the end so one is
    /// added behind the space, and a blank of spaces is a text that gets its own.</summary>
    [Fact]
    public void TheTooltipAddsOneFullStopToTheWordsAndOnlyOne()
    {
        Assert.Equal("API: Healthy. Done.", HealthDetailText.Title(Entry("API", CheckState.Healthy, "Done.")));
        Assert.Equal("API: Healthy. ok...", HealthDetailText.Title(Entry("API", CheckState.Healthy, "ok...")));
        Assert.Equal("API: Healthy. ok. .", HealthDetailText.Title(Entry("API", CheckState.Healthy, "ok. ")));
        Assert.Equal("API: Healthy.    .", HealthDetailText.Title(Entry("API", CheckState.Healthy, "   ")));
        Assert.Equal("Queue: Not known. ...", HealthDetailText.Title(Entry("Queue", CheckState.Unknown, "...")));
    }

    /// <summary>An entry named nothing is still tooltiped: the name is said as the blank it is, with the colon that
    /// follows it, and no word is invented for the absence - neither by the record nor by these words.</summary>
    [Fact]
    public void TheTooltipOfAnEntryNamedNothingSaysTheBlankItIs()
    {
        Assert.Equal(": Healthy.", HealthDetailText.Title(Entry(null!, CheckState.Healthy)));
        Assert.Equal(": Healthy.", HealthDetailText.Title(Entry(string.Empty, CheckState.Healthy)));
    }

    /// <summary>The milliseconds in a tooltip at the ends no answer reaches: the negative number and the number that
    /// is no number and the two that are no length, which the reading of one refuses - what a check hands over by
    /// hand is said as the words say it, and the negative with the nothing under a tenth of a millisecond.</summary>
    [Theory]
    [InlineData(-15002.3, "API: Healthy. Took under 0.1 ms.")]
    [InlineData(-0.0006, "API: Healthy. Took under 0.1 ms.")]
    [InlineData(double.NaN, "API: Healthy. Took NaN s.")]
    [InlineData(double.PositiveInfinity, "API: Healthy. Took Infinity s.")]
    [InlineData(double.NegativeInfinity, "API: Healthy. Took under 0.1 ms.")]
    public void TheTooltipSaysANumberNoAnswerGives(double milliseconds, string expected) =>
        Assert.Equal(expected, HealthDetailText.Title(Entry("API", CheckState.Healthy, null, milliseconds)));

    // -------------------------------------------------------------------------------- a length of checking

    /// <summary>Everything below the twentieth of a millisecond is the nothing the words say <c>under 0.1 ms</c>
    /// of: the two ends of a real number that are below it, the negative and the no-length, the zero the check that
    /// answered before it measured itself, the smallest number a real holds and the largest one under the tenth.</summary>
    [Theory]
    [InlineData(double.MinValue)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(-1d)]
    [InlineData(0d)]
    [InlineData(double.Epsilon)]
    [InlineData(0.04)]
    [InlineData(0.0499999999)]
    public void EverythingBelowATwentiethOfAMillisecondIsSaidAsNothing(double milliseconds) =>
        Assert.Equal("under 0.1 ms", HealthDetailText.Duration(milliseconds));

    /// <summary>The tenth of a millisecond, and the numbers under it that the tenth is rounded up to, are said as
    /// the tenth and not as the nothing below it: the boundary is the value itself, one step over it and one step
    /// under it on either side of the nothing above.</summary>
    [Theory]
    [InlineData(0.05)]
    [InlineData(0.050001)]
    [InlineData(0.0999999)]
    [InlineData(0.1)]
    public void TheFirstTenthOfAMillisecondIsSaidAndNotUnderIt(double milliseconds) =>
        Assert.Equal("0.1 ms", HealthDetailText.Duration(milliseconds));

    /// <summary>The milliseconds are said to a tenth, rounded away from nothing, and never carried onto a second:
    /// the forty-fifth of a hundredth goes up to the half, and the milliseconds the rounding carries to a thousand
    /// are said as a thousand milliseconds while the thousandth millisecond itself is already a second.</summary>
    [Theory]
    [InlineData(0.44, "0.4 ms")]
    [InlineData(0.45, "0.5 ms")]
    [InlineData(1.05, "1.1 ms")]
    [InlineData(999.94, "999.9 ms")]
    [InlineData(999.95, "1000 ms")]
    [InlineData(999.99, "1000 ms")]
    public void TheMillisecondsAreRoundedToATenthAndNeverOntoASecond(double milliseconds, string expected) =>
        Assert.Equal(expected, HealthDetailText.Duration(milliseconds));

    /// <summary>The second begins at the thousandth millisecond and is said to a tenth of itself from there: the
    /// thousand and the thousand and a hundredth are the one second, the thousand and forty-nine point nine and the
    /// thousand and forty-nine point nine-five are still under the half, and the thousand and fifty are the tenth
    /// over.</summary>
    [Theory]
    [InlineData(1000d, "1 s")]
    [InlineData(1000.04, "1 s")]
    [InlineData(1049.9, "1 s")]
    [InlineData(1049.95, "1 s")]
    [InlineData(1050d, "1.1 s")]
    [InlineData(1499d, "1.5 s")]
    public void TheSecondBeginsAtTheThousandthMillisecond(double milliseconds, string expected) =>
        Assert.Equal(expected, HealthDetailText.Duration(milliseconds));

    /// <summary>The second is the largest unit these words know: a minute, an hour and a day are all said as
    /// seconds, and the tenth of a second a number carries is rounded to nothing rather than carried into a bigger
    /// unit - the minutes and hours of the page are <see cref="TimeText" />'s business and pinned there.</summary>
    [Theory]
    [InlineData(60000d, "60 s")]
    [InlineData(123456.7, "123.5 s")]
    [InlineData(1_000_000d, "1000 s")]
    [InlineData(86_400_000d, "86400 s")]
    public void TheSecondIsTheLargestUnitTheseWordsKnow(double milliseconds, string expected) =>
        Assert.Equal(expected, HealthDetailText.Duration(milliseconds));

    /// <summary>The number that is no number and the number that is no length fall past both comparisons and land
    /// on the seconds: the seconds of a number that is no number are no number either, said with the word the
    /// invariant culture gives it, and the infinity over the top is said as an infinity of seconds while the one
    /// under the bottom is the nothing under a tenth of a millisecond - the one place the two infinities are not
    /// said the same way.</summary>
    [Fact]
    public void TheNumberThatIsNoNumberAndTheTwoThatAreNoLengthAreSaidAsSecondsOrAsNothing()
    {
        Assert.Equal("NaN s", HealthDetailText.Duration(double.NaN));
        Assert.Equal("Infinity s", HealthDetailText.Duration(double.PositiveInfinity));
        Assert.Equal("under 0.1 ms", HealthDetailText.Duration(double.NegativeInfinity));
        Assert.DoesNotContain("ms", HealthDetailText.Duration(double.NaN), StringComparison.Ordinal);
    }

    /// <summary>The largest number a real holds is a length of seconds said out in full: fifteen digits of it, then
    /// the places to the three hundred and fifth, and the unit - no exponent, no separator and no collapse onto a
    /// shorter unit, since the words are written with the one decimal place and nothing else.</summary>
    [Fact]
    public void TheLargestNumberIsSaidOutInFullAndNotWithAnExponent()
    {
        var said = HealthDetailText.Duration(double.MaxValue);

        Assert.StartsWith("179769313486232", said, StringComparison.Ordinal);
        Assert.EndsWith(" s", said, StringComparison.Ordinal);
        Assert.DoesNotContain("E", said, StringComparison.Ordinal);
        Assert.Equal(new string('0', 291), said[15..^2]);
        Assert.Equal(308, said.Length);
    }

    // --------------------------------------------------------------------------- the short line of an answer

    /// <summary>The short line refuses an answer that is not there, by the name the guard keeps.</summary>
    [Fact]
    public void TheShortLineRefusesAnAnswerThatIsNotThere() =>
        Assert.Equal("detail", Assert.Throws<ArgumentNullException>(() => HealthDetailText.Summary(null!)).ParamName);

    /// <summary>An answer that names no entry is an answer of none, and the none is plural: nothing stands behind
    /// the singular the one check takes, and no failing entry is named, so the line says the count it holds.</summary>
    [Fact]
    public void TheShortLineOfAnAnswerThatNamesNothingSaysNoneInItsOwnWords()
    {
        Assert.Empty(Detail().Entries);
        Assert.Equal("0 checks healthy", HealthDetailText.Summary(Detail()));
    }

    /// <summary>Two entries of a two-entry answer and three of a three-entry answer are the whole of it failing,
    /// and the line says the whole rather than naming them: the count of those that are not healthy is the count of
    /// the answer, and the one-entry form is what carries a name. The unknown-numbered state counts with the
    /// failing ones, being neither healthy nor a state the words pass over.</summary>
    [Theory]
    [InlineData(2, 2, "2 of 2 checks not healthy")]
    [InlineData(3, 3, "3 of 3 checks not healthy")]
    [InlineData(3, 2, "2 of 3 checks not healthy")]
    [InlineData(12, 4, "4 of 12 checks not healthy")]
    public void TheShortLineCountsEveryEntryThatIsNotHealthyAgainstTheWholeAnswer(int entries, int failing, string expected)
    {
        var states = new[] { CheckState.Unhealthy, CheckState.Degraded, CheckState.Unknown, (CheckState)4 };
        var detail = Detail([.. Enumerable.Range(0, entries).Select(index => Entry($"Check{index}", index < failing ? states[index % states.Length] : CheckState.Healthy))]);

        Assert.Equal(expected, HealthDetailText.Summary(detail));
    }

    /// <summary>The name in the short line is cut to the twenty characters a line of the diagram holds, and one
    /// character under that it is kept whole: the name of exactly twenty stands as the app registered it, the one of
    /// twenty-two loses its last two letters to the ellipsis, and a name that breaks in a space gives the space up
    /// with them.</summary>
    [Fact]
    public void TheShortLineCutsANameToTheTwentiethCharacterAndNoFurther()
    {
        Assert.Equal("TwentyCharsExactly!! degraded", HealthDetailText.Summary(Detail(Entry("TwentyCharsExactly!!", CheckState.Degraded))));
        Assert.Equal("TwentyOneCharsExact… degraded", HealthDetailText.Summary(Detail(Entry("TwentyOneCharsExactly!", CheckState.Degraded))));
        Assert.Equal("Twenty Two Chars!!… unhealthy", HealthDetailText.Summary(Detail(Entry("Twenty Two Chars!! OK", CheckState.Unhealthy))));

        // The cut is twenty characters of the name, the ellipsis and the state's word; nothing longer.
        var said = HealthDetailText.Summary(Detail(Entry(new string('A', 1_000), CheckState.Unhealthy)));

        Assert.Equal($"{new string('A', 19)}… unhealthy", said);
        Assert.Equal(30, said.Length);
    }

    /// <summary>The short line keeps a name's letters as the app wrote them and lowers only the word of the state:
    /// the name in capitals stays in capitals, the word <c>Not known</c> is lowered to two words of nothing between
    /// them, and a name of no letters - or of nothing but spaces - is said as the blank it is, with the space the
    /// two words are joined by standing at the front of the line.</summary>
    [Theory]
    [InlineData("DATAACCESS", CheckState.Unhealthy, "DATAACCESS unhealthy")]
    [InlineData("api", CheckState.Unhealthy, "api unhealthy")]
    [InlineData("DataAccess", CheckState.Unknown, "DataAccess not known")]
    [InlineData("", CheckState.Degraded, " degraded")]
    [InlineData("   ", CheckState.Unknown, "    not known")]
    public void TheShortLineKeepsTheNamesLettersAndLowersTheStatesWord(string name, CheckState state, string expected) =>
        Assert.Equal(expected, HealthDetailText.Summary(Detail(Entry(name, state))));

    // ------------------------------------------------------------------------------------ a cut to a line

    /// <summary>The cut refuses a text that is not there, by the name the guard keeps, and asks nothing of a text
    /// that is the empty word: nothing of it is cut, at no length at all - not even the length of nothing - since
    /// there is nothing left to reach for.</summary>
    [Fact]
    public void TheCutRefusesNoTextAndCutsNothingAtAnyLength()
    {
        Assert.Equal("text", Assert.Throws<ArgumentNullException>(() => HealthDetailText.Brief(null!, 20)).ParamName);

        foreach (var length in new[] { 0, 1, 5, 20, 1_000 })
        {
            Assert.Equal(string.Empty, HealthDetailText.Brief(string.Empty, length));
        }
    }

    /// <summary>The length is asked for as a number of characters and never checked as one: the cut of a text that
    /// is not itself nothing at the length of one character is the ellipsis alone, and at the length of nothing -
    /// or under it - the cut reaches behind the start of the text and says by the name of which argument, and by
    /// what value of it, that there is no such place.</summary>
    [Fact]
    public void TheCutAtTheLengthOfNothingRefusesAndAtTheLengthOfOneIsTheEllipsisAlone()
    {
        Assert.Equal("…", HealthDetailText.Brief("abc", 1));
        Assert.Equal("…", HealthDetailText.Brief("   ", 1));
        Assert.Equal("…", HealthDetailText.Brief("One. Two", 1));

        var zero = Assert.Throws<ArgumentOutOfRangeException>(() => HealthDetailText.Brief("abc", 0));
        Assert.Equal("length", zero.ParamName);
        Assert.Equal(-1, (int)zero.ActualValue!);

        var below = Assert.Throws<ArgumentOutOfRangeException>(() => HealthDetailText.Brief("abc", -1));
        Assert.Equal("length", below.ParamName);
        Assert.Equal(-2, (int)below.ActualValue!);
    }

    /// <summary>A sentence the line holds is kept whole at the very length it fills and cut one character before
    /// it: the twenty-five characters of <c>Process memory is healthy</c> stand at the twenty-five they are, and at
    /// twenty-four the last five letters go and the ellipsis takes their place - the ellipsis standing inside the
    /// length, never after it. A cut that lands in a space gives the space up, so the line ends on a word.</summary>
    [Theory]
    [InlineData("Process memory is healthy.", 25, "Process memory is healthy")]
    [InlineData("Process memory is healthy.", 24, "Process memory is healt…")]
    [InlineData("Process has 24 threads", 22, "Process has 24 threads")]
    [InlineData("Process has 24 threads", 21, "Process has 24 threa…")]
    [InlineData("Process has 24 threads", 13, "Process has…")]
    [InlineData("Process has 24 threads", 12, "Process has…")]
    [InlineData("Process has 24 threads", 11, "Process ha…")]
    public void TheCutKeepsASentenceItHoldsWholeAndCutsASpaceAwayWithTheRest(string text, int length, string expected)
    {
        var brief = HealthDetailText.Brief(text, length);

        Assert.Equal(expected, brief);
        Assert.True(brief.Length <= length, $"\"{brief}\" of {length} characters is {brief.Length} long");
    }

    /// <summary>Only the first sentence is ever kept, however much room there is for the rest: four sentences in a
    /// text of a hundred characters are said as the one letter of the first, and a text cut before the end of its
    /// first sentence is cut out of the whole text, so the letters it keeps are the first of it and no sentence
    /// boundary is looked for in the part that was thrown away.</summary>
    [Theory]
    [InlineData("a. b. c. d", 100, "a")]
    [InlineData("One. Two. Three.", 6, "One")]
    [InlineData("Chat client is not configured. Set the following", 30, "Chat client is not configured")]
    [InlineData("Chat client is not configured. Set the following", 10, "Chat clie…")]
    public void TheCutSaysTheFirstSentenceAndNothingAfterIt(string text, int length, string expected) =>
        Assert.Equal(expected, HealthDetailText.Brief(text, length));

    /// <summary>A sentence ends at a full stop with a space after it, and at nothing else: the full stop of
    /// <c>One. Two</c> ends it, while the same full stop before a non-breaking space, a tab or a line break leaves
    /// the text whole - the two characters are looked for as characters, not as whatever a reader counts as a
    /// space. A text whose first two characters are that ending has no first sentence at all, and so is said as
    /// nothing; a text of nothing but full stops has its last ones trimmed away and is nothing too.</summary>
    [Theory]
    [InlineData("One. Two", 20, "One")]
    [InlineData("One.\u00A0Two", 20, "One.\u00A0Two")]
    [InlineData("One.\tTwo", 20, "One.\tTwo")]
    [InlineData("One.\nTwo", 20, "One.\nTwo")]
    [InlineData(". Second sentence", 20, "")]
    [InlineData("...", 5, "")]
    [InlineData(".....", 5, "")]
    public void TheCutSplitsOnAFullStopAndASpaceAndOnNoOtherSpace(string text, int length, string expected) =>
        Assert.Equal(expected, HealthDetailText.Brief(text, length));

    /// <summary>The length counts characters and not what a reader sees: a symbol of two characters is cut in
    /// half, and the lone half of it stands in the line before the ellipsis - five characters of the text of the
    /// same letters cut one character later hold the whole symbol instead. Nothing here composes or repairs a text:
    /// the ellipsis is the one character the cut adds, and the tooltips hold the text whole.</summary>
    [Fact]
    public void TheCutCountsCharactersAndNotWhatAReaderSees()
    {
        var half = HealthDetailText.Brief("ab\U0001F3AFdefgh", 4);
        var whole = HealthDetailText.Brief("ab\U0001F3AFdefgh", 5);

        Assert.Equal(4, half.Length);
        Assert.Equal('a', half[0]);
        Assert.Equal('b', half[1]);
        Assert.True(char.IsHighSurrogate(half[2]));
        Assert.Equal('…', half[3]);
        Assert.Equal("ab" + half[2] + "…", half, StringComparer.Ordinal);

        Assert.Equal(5, whole.Length);
        Assert.Equal("ab\U0001F3AF…", whole, StringComparer.Ordinal);
        Assert.DoesNotContain("...", half, StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------------------- the reason a check failed

    /// <summary>An answer that names no entry is no reason: the guard that asks for entries the way a caller does
    /// lands on none, and says nothing - the same answer as the one where every entry is healthy and the same as no
    /// answer at all, which is what lets a page write <c>Failed(detail) is { } failed</c> and stay out of the way.</summary>
    [Fact]
    public void TheReasonOfAnAnswerThatNamesNothingIsNoWords()
    {
        Assert.Null(HealthDetailText.Failed(Detail()));
        Assert.Null(HealthDetailText.Failed(Detail(Entry("API", CheckState.Healthy, "API layer is healthy", 1.5))));
        Assert.Null(HealthDetailText.Failed(null));
    }

    /// <summary>An entry whose words are the empty word, a blank or nothing but full stops is named with its state
    /// alone: the one trimming the words are joined by (#1167) leaves nothing of them, and an entry that is left
    /// with nothing is named the way the entry that says no words at all is - with its state and no colon, and no
    /// blank standing behind it.</summary>
    [Theory]
    [InlineData("", "Not healthy: Queue (Unhealthy)")]
    [InlineData("   ", "Not healthy: Queue (Unhealthy)")]
    [InlineData(".", "Not healthy: Queue (Unhealthy)")]
    [InlineData(". .", "Not healthy: Queue (Unhealthy)")]
    [InlineData("...", "Not healthy: Queue (Unhealthy)")]
    [InlineData("Login failed.", "Not healthy: Queue (Unhealthy: Login failed)")]
    [InlineData("Login failed....", "Not healthy: Queue (Unhealthy: Login failed)")]
    [InlineData("Login failed. ", "Not healthy: Queue (Unhealthy: Login failed)")]
    [InlineData("  Login failed", "Not healthy: Queue (Unhealthy: Login failed)")]
    public void TheReasonSaysAnEntryWhoseWordsAreBlankOrNothingButFullStops(string description, string expected) =>
        Assert.Equal(expected, HealthDetailText.Failed(Detail(Entry("Queue", CheckState.Unhealthy, description))));

    /// <summary>Every entry that is not healthy is named, the worst first and none of the healthy ones, each in
    /// brackets with its own word: three failing entries of an answer of four are three names and two commas, the
    /// entry with no words of its own is named with its state alone and the ellipsis-free brackets of the entry
    /// whose state is not known say <c>Not known</c> as the marks' word does.</summary>
    [Fact]
    public void TheReasonNamesEveryEntryThatIsNotHealthyTheWorstFirst()
    {
        var failed = HealthDetailText.Failed(Detail(
            Entry("c", CheckState.Unknown, "C", 1),
            Entry("h", CheckState.Healthy, "H", 1),
            Entry("a", CheckState.Unhealthy, null, 1),
            Entry("d", CheckState.Degraded, "D.", 1)));

        Assert.Equal("Not healthy: a (Unhealthy), d (Degraded: D), c (Not known: C)", failed);
        Assert.DoesNotContain("h (", failed!, StringComparison.Ordinal);
        Assert.Equal(3, failed!.Split(',', StringSplitOptions.None).Length);
        Assert.Equal("a (Unhealthy), d (Degraded: D), c (Not known: C)", failed![(failed!.IndexOf(':') + 2)..]);
    }

    // ---------------------------------------------------------------------- what the class declares itself

    /// <summary>The whole public surface of the words of a detailed health check, as the running assembly holds it:
    /// a static class of no constructor and no instance member, its eight sayers - the word of a state, its place in
    /// the order of the worst first, the shape of its mark, the tooltip of an entry, a length of checking, the short
    /// line of an answer, a cut to a line and the reason of a failure - each of them public, static and single, and
    /// nothing else to call. A ninth word added to the class has to be written here as well, which is the point: a
    /// mark cannot be drawn with a word nobody decided what it means.</summary>
    [Fact]
    public void TheClassIsTheEightSayersOfThePagesWordsAndNothingElse()
    {
        var type = typeof(HealthDetailText);

        Assert.True(type.IsAbstract && type.IsSealed, "the words are written as a static class");
        Assert.Empty(type.GetConstructors());
        Assert.Empty(type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.Equal(
            ["Brief", "Duration", "Failed", "Icon", "Label", "Severity", "Summary", "Title"],
            methods.Select(method => method.Name).Order(StringComparer.Ordinal));
        Assert.Equal(8, methods.Length);
        Assert.All(methods, method => Assert.True(method.IsStatic));
        Assert.Equal(8, methods.Select(method => method.Name).Distinct(StringComparer.Ordinal).Count());

        Assert.Equal(typeof(string), type.GetMethod("Label")!.ReturnType);
        Assert.Equal(typeof(int), type.GetMethod("Severity")!.ReturnType);
        Assert.Equal(typeof(HealthState), type.GetMethod("Icon")!.ReturnType);
        Assert.Equal(typeof(string), type.GetMethod("Title")!.ReturnType);
        Assert.Equal(typeof(string), type.GetMethod("Duration")!.ReturnType);
        Assert.Equal(typeof(string), type.GetMethod("Summary")!.ReturnType);
        Assert.Equal(typeof(string), type.GetMethod("Brief")!.ReturnType);
        Assert.Equal(typeof(string), type.GetMethod("Failed")!.ReturnType);

        Assert.Equal(
            [typeof(CheckState)],
            type.GetMethod("Label")!.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            [typeof(HealthCheckEntry)],
            type.GetMethod("Title")!.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            [typeof(double)],
            type.GetMethod("Duration")!.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            [typeof(HealthDetail)],
            type.GetMethod("Summary")!.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            [typeof(string), typeof(int)],
            type.GetMethod("Brief")!.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            [typeof(HealthDetail)],
            type.GetMethod("Failed")!.GetParameters().Select(parameter => parameter.ParameterType));
    }
}
