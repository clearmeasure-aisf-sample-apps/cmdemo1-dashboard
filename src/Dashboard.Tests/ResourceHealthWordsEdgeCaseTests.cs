using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ResourceHealthWords" />, the four words a box of the
/// runtime diagram wears for what Azure Resource Health last said of the resources it stands for, declared beside the
/// reading they come from in <c>src/Dashboard/Health/ResourceHealth.cs</c>: the badge, the line under it, the tone the
/// page dresses them in, and the tooltip they open.
/// <para>
/// What the other files already pin is walked past here on purpose. That a box of one resource is told
/// <c>Available</c> / <c>Resource Health, 40 min ago</c> / <c>plain</c>, that a box of several shows its worst word and
/// counts the rest, that a box without a report says <c>Not known</c> / <c>no Resource Health read yet</c> /
/// <c>muted</c>, that the age on the line is the oldest reading's, that a box read over two hours ago is muted, that a
/// box without ids has no words at all and that the tooltip names the resource, its state, where the words came from
/// and when - all of that is <see cref="ResourceHealthTextTests" /> and <see cref="ResourceHealthTests" />, which ask
/// <see cref="ResourceHealthText.Of" /> and read the four words back. The words that leave the box and reach the rest
/// of the page are pinned where they land: <see cref="UnplacedTests" /> and <see cref="UnplacedRowTests" /> for the
/// row of the unplaced list, <see cref="RuntimePayloadBuilderTests" /> for the tile. The record each of them is handed
/// - how it holds its four words, how it compares them, how it says them - is what those tests use without ever naming
/// and is what is pinned below.
/// </para>
/// <para>
/// So: the constructor handing each of its four arguments to the property of its name and holding the very string it
/// was given; every word kept exactly as it is given - the word of no characters, the word of whitespace, a word
/// padded, a word in another case, a word of a thousand letters - and refused by nothing, since the cutting of a long
/// summary to 200 characters belongs to <see cref="ResourceHealthReading" /> and not here; a tooltip of several
/// resources, which is several lines, held as the one word it is; the empty word and the absence of one being two
/// different nothings and neither of them the dash the page says elsewhere; the four parts coming back out in the
/// order they went in, and the copy that turns one while it keeps the other three; the two words being one while
/// their four parts are, with the hash, the set and the four ways of asking following that, and the nothing on either
/// side of each; the words the record says itself, which say the parts and nothing of the page's meaning - no colour
/// refused, no placeholder invented, no markup added - and follow no reader's culture; and the public surface, so that
/// the day a fifth word is added the list below says so. The one boundary of the making that no test held is held
/// last: <see cref="ResourceHealthText.Of" /> refuses a zone that is not there before it looks at the ids at all.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: the record is four strings, calls
/// nothing at all, holds no moment, takes no address and hands back nothing to wait on - every value below is one the
/// test brought. The page does ask Azure for nothing either, and this file does not ask it for anything: no
/// <see cref="HttpClient" /> and no <c>containers.json</c> is opened, and where a zone is named it is the one the test
/// made.
/// </para>
/// </summary>
public class ResourceHealthWordsEdgeCaseTests
{
    /// <summary>The badge of a box whose one resource is well, written out here rather than taken from the app: a
    /// word the page shows a reader is a word a change to has to break.</summary>
    private const string Badge = "Available";

    /// <summary>The line under that badge - where the words came from and how long ago.</summary>
    private const string Line = "Resource Health, 40 min ago";

    /// <summary>The tone of a box read within the hour, which the page dresses the two words in.</summary>
    private const string Tone = "plain";

    /// <summary>The tooltip of that one resource, on the page's own first words and one line.</summary>
    private const string Tip = "kv: available (by Resource Health), read 2026-10-04 21:37 +00:00 (40 min ago). No incidents reported.";

    /// <summary>The id of one box's resource, for the one test below that asks the page to make the words.</summary>
    private const string KeyVault = "/subscriptions/s/resourceGroups/rg/providers/Microsoft.KeyVault/vaults/kv";

    /// <summary>A box of one resource, well, read within the hour: four words none of which is another's, so that a
    /// part landing on another part's place is seen rather than quietly matching a third of the same record.</summary>
    private static ResourceHealthWords Full => new(Badge, Line, Tone, Tip);

    // ---------------------------------- the four words, by the names of their own properties

    /// <summary>The constructor's four arguments, position by position, come back as the property of the same name:
    /// the badge first, the line beside it, the tone under that, the tooltip last - the order a box is drawn in, and
    /// the order <c>RuntimeTile</c> and <c>UnplacedRow</c> take them. Each word is held as the very string it was
    /// given, copied by nothing, turned by nothing and shortened by nothing.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var badge = Own(Badge);
        var line = Own(Line);
        var tone = Own(Tone);
        var tip = Own(Tip);
        var words = new ResourceHealthWords(badge, line, tone, tip);

        Assert.Same(badge, words.Label);
        Assert.Same(line, words.Line);
        Assert.Same(tone, words.Tone);
        Assert.Same(tip, words.Title);
        Assert.Equal((Badge, Line, Tone, Tip), (words.Label, words.Line, words.Tone, words.Title));

        // Each word stands on its own: none of the four is read out of another, and a box of four alike words is
        // another box than one of four different ones.
        Assert.Equal(new ResourceHealthWords("Not known", Line, Tone, Tip), new("Not known", Line, Tone, Tip));
        Assert.NotEqual(words, new ResourceHealthWords(Tone, Tone, Tone, Tone));
        Assert.NotEqual(words, new ResourceHealthWords(Badge, Badge, Badge, Badge));

        // The record the page makes of a box is a record like any other: what it is given is what it holds.
        var box = new ResourceHealthWords("Not known", "no Resource Health read yet", "muted", Tip);

        Assert.Equal(("Not known", "no Resource Health read yet", "muted"), (box.Label, box.Line, box.Tone));
        Assert.Equal(Tip, box.Title);
        Assert.NotSame(words, box);
        Assert.NotEqual(words, box);
    }

    /// <summary>Either word is kept in its exact letters: the word of no characters, the word that is only whitespace,
    /// a word padded at both ends, a word in another case, a word of the page's own dash and its own <c>n/a</c>, a word
    /// of another language's letters and a word with a line break in it all stand where they stand. Nothing here
    /// trims, folds, lowers, shortens or refuses - and the four words of a box are the same four letters whichever of
    /// the four places they are put in.</summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   \t ")]
    [InlineData("\n")]
    [InlineData("Available")]
    [InlineData("available")]
    [InlineData("  Not known  ")]
    [InlineData("Resource Health, 40 min ago")]
    [InlineData("1 failed · 1 degraded · 1 available, 77 min ago")]
    [InlineData("—")]
    [InlineData("n/a")]
    [InlineData("plain")]
    [InlineData("muted")]
    [InlineData("red")]
    [InlineData("İyi günler")]
    [InlineData("a\r\nb")]
    public void AWordIsHeldExactlyAsItIsGiven(string word)
    {
        var held = Own(word);
        var words = new ResourceHealthWords(held, held, held, held);

        // The empty word is the interned one whatever a test does, so the length says what the reference cannot.
        Assert.Same(held, words.Label);
        Assert.Equal(word, words.Label);
        Assert.Equal(word.Length, words.Label.Length);
        Assert.All(new[] { words.Label, words.Line, words.Tone, words.Title }, part => Assert.Equal(word, part));
        Assert.Equal(words.GetHashCode(), new ResourceHealthWords(word, word, word, word).GetHashCode());

        // The letters are the letters it was given: nothing trimmed from either end, nothing folded, no case turned,
        // no line break taken out and no whitespace run down to a single space.
        Assert.Equal(word, new string([.. words.Label]));
        Assert.Equal(word.ToCharArray(), words.Title.ToCharArray());
        Assert.StartsWith(word.Length == 0 ? word : word[..1], words.Label, StringComparison.Ordinal);
        Assert.EndsWith(word.Length == 0 ? word : word[^1..], words.Tone, StringComparison.Ordinal);
    }

    /// <summary>A word of any length is held whole, to a thousand letters and back: the record cuts nothing, so a box's
    /// tooltip that runs past the 200 characters <see cref="ResourceHealthReading" /> cuts its summary to is said whole
    /// - the cut belongs to the reading of the file, pinned in <see cref="ResourceHealthTests" />, and nothing of it
    /// reaches the words a box is drawn from. The empty word, the other way round, is a word of no characters at all,
    /// and a box built of four of them is a box with four parts said as nothing rather than a box with none.</summary>
    [Fact]
    public void AWordOfAnyLengthIsHeldWholeAndCutByNothing()
    {
        foreach (var length in new[] { 0, 1, 2, 199, 200, 201, 999, 1_000 })
        {
            var word = new string('x', length);
            var words = new ResourceHealthWords(word, word, word, word);

            Assert.Same(word, words.Label);
            Assert.All(new[] { words.Label, words.Line, words.Tone, words.Title }, part => Assert.Equal(length, part.Length));
            Assert.Equal(4 * length, words.ToString().Count(character => character == 'x'));
            Assert.Equal(words, new ResourceHealthWords(new string('x', length), word, word, word));
            Assert.NotEqual(words, new ResourceHealthWords(new string('x', length + 1), word, word, word));
        }

        // The whole of the page's tooltip, tooltip-words and all, is a part and not a limit.
        var tip = string.Join('\n', Enumerable.Repeat(ResourceHealthText.Source, 9));
        var box = new ResourceHealthWords(Badge, Line, Tone, tip);

        Assert.Equal(tip, box.Title);
        Assert.Equal(9 * ResourceHealthText.Source.Length + 8, box.Title.Length);
    }

    /// <summary>The tooltip of a box of several resources is several lines joined by the page with <c>\n</c>, and the
    /// record holds it as the one word it is: the lines are still there to be counted, they are said as they were
    /// joined, and the same lines joined another way - with a space, with <c>\r\n</c>, or in another order - are
    /// another title to this record and to its hash. Nothing here splits, re-joins or re-orders a title, which is why
    /// the page is free to put one line per resource in it.</summary>
    [Fact]
    public void ATooltipOfSeveralLinesIsHeldWholeAndSaidWhole()
    {
        var lines = new[] { ResourceHealthText.Source, "kv: available (by Resource Health), read 2026-10-04 21:37 +00:00.", "id: succeeded (neither Resource Health nor a provisioning state answered).", "And 3 more." };
        var tip = string.Join('\n', lines);
        var words = new ResourceHealthWords("Not known", "1 available · 1 not known", "muted", tip);

        Assert.Equal(tip, words.Title);
        Assert.Equal(4, words.Title.Split('\n').Length);
        Assert.Equal(lines, words.Title.Split('\n'));
        Assert.DoesNotContain("\r", words.Title, StringComparison.Ordinal);

        // The same lines, told another way, are another title - and another record.
        Assert.NotEqual(words, new ResourceHealthWords(words.Label, words.Line, words.Tone, string.Join(' ', lines)));
        Assert.NotEqual(words, new ResourceHealthWords(words.Label, words.Line, words.Tone, string.Join("\r\n", lines)));
        Assert.NotEqual(words, new ResourceHealthWords(words.Label, words.Line, words.Tone, string.Join('\n', lines.Reverse())));
        Assert.NotEqual(words.GetHashCode(), new ResourceHealthWords(words.Label, words.Line, words.Tone, string.Join(' ', lines)).GetHashCode());
        Assert.Equal(4, new HashSet<ResourceHealthWords> { words, words with { Title = string.Join(' ', lines) }, words with { Title = string.Join("\r\n", lines) }, words with { Title = string.Join('\n', lines.Reverse()) } }.Count);

        // And the record says them as it was given them: the line breaks are in its own words, not escaped away.
        Assert.Contains("kv: available", words.ToString(), StringComparison.Ordinal);
        Assert.Contains("And 3 more.", words.ToString(), StringComparison.Ordinal);
        Assert.Equal(4, words.ToString().Split('\n').Length);
        Assert.Equal(lines.Length - 1, words.ToString().Count(character => character == '\n'));
    }

    /// <summary>The record refuses nothing: a word that is not there at all stands as that, and it is not the empty
    /// word - which in turn is not the dash a card says for a thing it did not measure. A box of nothing is still a
    /// box of four parts, and the reference that is no box at all is another nothing beside them.</summary>
    [Fact]
    public void AWordThatIsNotGivenIsKeptAsTheNothingItIs()
    {
        var noBadge = new ResourceHealthWords(null!, Line, Tone, Tip);

        Assert.NotNull(noBadge);
        Assert.Null(noBadge.Label);
        Assert.Equal(Line, noBadge.Line);

        var noLine = new ResourceHealthWords(Badge, null!, Tone, Tip);
        var noTone = new ResourceHealthWords(Badge, Line, null!, Tip);
        var noTip = new ResourceHealthWords(Badge, Line, Tone, null!);

        Assert.Null(noLine.Line);
        Assert.Null(noTone.Tone);
        Assert.Null(noTip.Title);
        Assert.Equal(Badge, noLine.Label);
        Assert.Equal(Tone, noTip.Tone);

        var neither = new ResourceHealthWords(null!, null!, null!, null!);

        Assert.All(new[] { neither.Label, neither.Line, neither.Tone, neither.Title }, part => Assert.Null(part));
        Assert.NotNull(neither);
        Assert.Null(default(ResourceHealthWords));
        Assert.Equal("ResourceHealthWords { Label = , Line = , Tone = , Title =  }", neither.ToString());

        // Nothing, the empty word and the page's own dash are three different words to this record, in every one of
        // the four places - and the record that settles which of them a box shows is the making, not the record.
        Assert.NotEqual(neither, new ResourceHealthWords("", "", "", ""));
        Assert.NotEqual(noBadge, new ResourceHealthWords("", Line, Tone, Tip));
        Assert.NotEqual(noTone, new ResourceHealthWords(Badge, Line, "—", Tip));
        Assert.True(neither != new ResourceHealthWords("", "", "", ""));
        Assert.True(neither.Equals(new ResourceHealthWords(null!, null!, null!, null!)));
        Assert.False(noBadge == noLine);
    }

    // ------------------------------- the four words coming out again, and the copy that turns one

    /// <summary>The four words come back out in the order they went in - the badge, the line, the tone, the tooltip -
    /// whether they are asked for at once in a <c>var</c> pattern or one after the other, and what came out goes back
    /// in the same way: the box the four words fall apart into is the box they were. The empty word, the whitespace,
    /// the line break and the nothing fall apart as they stand, turned to nothing by nobody.</summary>
    [Fact]
    public void TheFourPartsComeOutAgainInTheOrderTheyWentIn()
    {
        var (badge, line, tone, tip) = Full;

        Assert.Equal(Badge, badge);
        Assert.Equal(Line, line);
        Assert.Equal(Tone, tone);
        Assert.Equal(Tip, tip);
        Assert.Equal(Full, new ResourceHealthWords(badge, line, tone, tip));

        var there = Full;
        there.Deconstruct(out var outBadge, out var outLine, out var outTone, out var outTip);

        Assert.Equal(badge, outBadge);
        Assert.Equal(line, outLine);
        Assert.Equal(tone, outTone);
        Assert.Equal(tip, outTip);
        Assert.Same(there.Label, outBadge);

        var (noBadge, noLine, noTone, noTip) = new ResourceHealthWords(null!, null!, null!, null!);

        Assert.Null(noBadge);
        Assert.Null(noLine);
        Assert.Null(noTone);
        Assert.Null(noTip);
        Assert.Equal(new ResourceHealthWords(null!, null!, null!, null!), new ResourceHealthWords(noBadge!, noLine!, noTone!, noTip!));

        var (blank, space, lineBreak, nothing) = new ResourceHealthWords("", "   ", "\n", null!);

        Assert.Equal("", blank);
        Assert.Equal("   ", space);
        Assert.Equal("\n", lineBreak);
        Assert.Null(nothing);
        Assert.NotEqual(blank, space);
        Assert.NotEqual(blank, lineBreak);
    }

    /// <summary>Either of the four words may be turned as the box is made: the parts it does not name stay the very
    /// strings the box was built of, and a box whose parts are turned back on is the box it started from in another
    /// reference - never the same one. A word turned to nothing is the nothing and not the empty word.</summary>
    [Fact]
    public void APartTurnedAsTheWordsAreMadeStandsWhereItIsTurnedTo()
    {
        var tip = Own(Tip);
        var words = new ResourceHealthWords(Badge, Line, Tone, tip) { Label = "Not known" };

        Assert.Equal("Not known", words.Label);
        Assert.Equal(Line, words.Line);
        Assert.Equal(Tone, words.Tone);
        Assert.Same(tip, words.Title);
        Assert.NotSame(Full, words);
        Assert.NotEqual(Full, words);

        var muted = new ResourceHealthWords(Badge, Line, Tone, tip) { Tone = "muted" };

        Assert.Equal("muted", muted.Tone);
        Assert.Equal(Tone, Full.Tone);
        Assert.Same(tip, muted.Title);

        var allFour = new ResourceHealthWords(null!, null!, null!, null!) { Label = Badge, Line = Line, Tone = Tone, Title = tip };

        Assert.Equal(Full, allFour);
        Assert.NotSame(Full, allFour);
        Assert.Same(tip, allFour.Title);

        var turnedToNothing = new ResourceHealthWords(Badge, Line, Tone, tip) { Title = null! };

        Assert.Null(turnedToNothing.Title);
        Assert.NotEqual(turnedToNothing, new ResourceHealthWords(Badge, Line, Tone, ""));
    }

    /// <summary>The copy that turns nothing is the same four words in another reference; the copy that turns one word
    /// keeps the other three as the very strings the original was built of, and leaves the original where it stood. A
    /// copy that names all four words as they stand is the box it started from without being it, and a word turned in
    /// a copy is seen in that copy's own words as well as in its parts.</summary>
    [Fact]
    public void ACopyTurnsThePartItNamesAndKeepsTheOtherThree()
    {
        var tip = Own(Tip);
        var words = new ResourceHealthWords(Badge, Line, Tone, tip);

        var unchanged = words with { };

        Assert.Equal(words, unchanged);
        Assert.NotSame(words, unchanged);
        Assert.Same(tip, unchanged.Title);

        var aged = words with { Line = "Resource Health, 2 h ago", Tone = "muted" };

        Assert.Equal(Badge, aged.Label);
        Assert.Equal("Resource Health, 2 h ago", aged.Line);
        Assert.Equal("muted", aged.Tone);
        Assert.Same(tip, aged.Title);
        Assert.Equal(Line, words.Line);
        Assert.Equal(Tone, words.Tone);
        Assert.NotEqual(words, aged);

        var worst = words with { Label = "Unavailable" };

        Assert.Equal(new ResourceHealthWords("Unavailable", Line, Tone, Tip), worst);
        Assert.StartsWith("ResourceHealthWords { Label = Unavailable, Line = ", worst.ToString(), StringComparison.Ordinal);

        var emptied = words with { Label = "", Line = "", Tone = "", Title = "" };

        Assert.Equal(new ResourceHealthWords("", "", "", ""), emptied);
        Assert.NotEqual(words, emptied);
        Assert.Equal("ResourceHealthWords { Label = , Line = , Tone = , Title =  }", emptied.ToString());

        var repacked = words with { Label = words.Label, Line = words.Line, Tone = words.Tone, Title = words.Title };

        Assert.Equal(words, repacked);
        Assert.NotSame(words, repacked);
        Assert.Same(tip, repacked.Title);
        Assert.NotSame(tip, (words with { Title = Own(Tip) }).Title);
    }

    // --------------------------------------------- the four words, the equality and the hash

    /// <summary>A box is one with another while its four words are one, whichever of the four ways of asking is used:
    /// the record's own <c>Equals</c>, the one of <see cref="object" />, <c>==</c> and <c>!=</c>. The hash follows the
    /// equality, boxed or unboxed and from one call to the next, and the letters are compared rather than the
    /// references - so the two boxes the page makes of two readings of one <c>containers.json</c> are one box and one
    /// entry of a set, although no string in them is shared.</summary>
    [Fact]
    public void TwoWordsAreOneWhileTheirFourPartsAre()
    {
        var one = Full;
        var two = new ResourceHealthWords(Badge, Line, Tone, Tip);

        Assert.True(one.Equals(two));
        Assert.True(one.Equals(one));
        Assert.True(one.Equals((object)two));
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.Equal(one, two);
        Assert.NotSame(one, two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Single(new HashSet<ResourceHealthWords> { one, two });

        var writtenAgain = new ResourceHealthWords(Own(Badge), Own(Line), Own(Tone), Own(Tip));

        Assert.NotSame(one.Label, writtenAgain.Label);
        Assert.NotSame(one.Title, writtenAgain.Title);
        Assert.Equal(one, writtenAgain);
        Assert.True(one == writtenAgain);
        Assert.Equal(one.GetHashCode(), writtenAgain.GetHashCode());
        Assert.Single(new HashSet<ResourceHealthWords> { one, two, writtenAgain });

        // Two boxes of nothing at all are one box, and so are two boxes of four empty words.
        Assert.Equal(new ResourceHealthWords(null!, null!, null!, null!), new ResourceHealthWords(null!, null!, null!, null!));
        Assert.Equal(new ResourceHealthWords("", "", "", ""), new ResourceHealthWords(Own(""), "", "", ""));
        Assert.Single(new HashSet<ResourceHealthWords> { new(null!, null!, null!, null!), new(null!, null!, null!, null!) });
    }

    /// <summary>A difference in any one of the four words is another box, and so is the same four words told in
    /// another place: a letter turned in the badge, another age in the line, the other tone, a tooltip with one more
    /// resource named in it, the case of a word turned, a space added at either end, and the badge and the tone
    /// exchanged between the properties. A set of boxes that differ one part each holds them all apart - which is what
    /// lets a page tell the box of an available vault from the box of a degraded one at all.</summary>
    [Fact]
    public void WordsThatDifferInOnePartAreTwoBoxes()
    {
        var one = Full;

        Assert.NotEqual(one, new ResourceHealthWords("Degraded", Line, Tone, Tip));
        Assert.NotEqual(one, new ResourceHealthWords("available", Line, Tone, Tip));
        Assert.NotEqual(one, new ResourceHealthWords(" Available", Line, Tone, Tip));
        Assert.NotEqual(one, new ResourceHealthWords(Badge, "Resource Health, 41 min ago", Tone, Tip));
        Assert.NotEqual(one, new ResourceHealthWords(Badge, "", Tone, Tip));
        Assert.NotEqual(one, new ResourceHealthWords(Badge, Line, "muted", Tip));
        Assert.NotEqual(one, new ResourceHealthWords(Badge, Line, "PLAIN", Tip));
        Assert.NotEqual(one, new ResourceHealthWords(Badge, Line, Tone, Tip + " And 1 more."));
        Assert.NotEqual(one, new ResourceHealthWords(Badge, Line, Tone, Tip[..^1]));
        Assert.NotEqual(one, new ResourceHealthWords(null!, Line, Tone, Tip));
        Assert.NotEqual(one, new ResourceHealthWords(Badge, Line, Tone, null!));

        // The same four words in another order are four other words: nothing pairs a part with its name by meaning.
        Assert.NotEqual(one, new ResourceHealthWords(Line, Badge, Tone, Tip));
        Assert.NotEqual(one, new ResourceHealthWords(Badge, Line, Tip, Tone));
        Assert.True(one != new ResourceHealthWords(Line, Badge, Tone, Tip));
        Assert.False(one.Equals((object)new ResourceHealthWords(Badge, Line, Tip, Tone)));

        Assert.Equal(
            5,
            new HashSet<ResourceHealthWords>
            {
                one,
                one with { Label = "Degraded" },
                one with { Line = "provisioning state" },
                one with { Tone = "muted" },
                one with { Title = "And 3 more." },
            }.Count);
    }

    /// <summary>The hash says the four words and nothing else: the same four words hashed again are the same hash,
    /// whatever the references behind them, a word turned is another hash, and the three boxes of nothing, of empty
    /// words and of every word said stand apart in a set of their own - however the record's own words say the first
    /// two alike.</summary>
    [Fact]
    public void TheHashFollowsThePartsAndStandsStillFromOneCallToTheNext()
    {
        var full = Full;
        var empty = new ResourceHealthWords("", "", "", "");
        var nothing = new ResourceHealthWords(null!, null!, null!, null!);
        var noTip = new ResourceHealthWords(Badge, Line, Tone, "");

        Assert.Equal(full.GetHashCode(), new ResourceHealthWords(Own(Badge), Own(Line), Own(Tone), Own(Tip)).GetHashCode());
        Assert.Equal(empty.GetHashCode(), empty.GetHashCode());
        Assert.Equal(nothing.GetHashCode(), new ResourceHealthWords(null!, null!, null!, null!).GetHashCode());
        Assert.Equal(noTip.GetHashCode(), (full with { Title = "" }).GetHashCode());
        Assert.NotEqual(full.GetHashCode(), empty.GetHashCode());
        Assert.NotEqual(full.GetHashCode(), nothing.GetHashCode());
        Assert.NotEqual(full.GetHashCode(), noTip.GetHashCode());
        Assert.NotEqual(full.GetHashCode(), (full with { Tone = "muted" }).GetHashCode());
        Assert.Equal(3, new HashSet<ResourceHealthWords> { full, empty, nothing }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two boxes that are no box at all
    /// are one, a box and no box are two - the equality and the inequality each saying it, from either side - and the
    /// box asked about itself says no to the nothing, to a word it is not, to a bare number, to the anonymous thing
    /// that happens to carry its four words, and to the two-word part of a cost line spelled with the same letters.</summary>
    [Fact]
    public void NoWordsAreOneWithNoWordsAndNotWithAnyOtherThing()
    {
        ResourceHealthWords? nothing = null;
        ResourceHealthWords? alsoNothing = null;
        var there = Full;

        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == (ResourceHealthWords?)null);
        Assert.True(there != (ResourceHealthWords?)null);
        Assert.False(there.Equals(nothing));
        Assert.False(there.Equals((object?)null));
        Assert.True(there.Equals(there));
        Assert.Null(default(ResourceHealthWords));
        Assert.Null(((ResourceHealthWords?)null)?.Tone);

        // A box of nothing at all is still a box: what turns it into no box is the making's answer, pinned in
        // ResourceHealthTextTests, and not the record.
        Assert.NotNull(new ResourceHealthWords(null!, null!, null!, null!));

        Assert.False(there.Equals("ResourceHealthWords { Label = , Line = , Tone = , Title =  }"));
        Assert.False(there.Equals((object)40));
        Assert.False(there.Equals(new { Label = Badge, Line = Line, Tone = Tone, Title = Tip }));
        Assert.False(there.Equals(new CostPart(Badge, Tone)));
        Assert.Equal(2, new HashSet<ResourceHealthWords?> { there, nothing, alsoNothing }.Count);
    }

    // ------------------------------------------------------------- the words the record says itself

    /// <summary>The words the box says its four parts in: its own name at the front, then the badge, the line, the
    /// tone and the tooltip in the order the constructor names them, the words as the letters they are - whitespace,
    /// tab, line break and all - and a part nobody said as the empty place between the mark and the comma. The whole
    /// tooltip of a box of several is said in those words too, since the record keeps what it was given.</summary>
    [Fact]
    public void TheRecordSaysItsFourPartsInTheOrderTheyAreNamed()
    {
        Assert.Equal(
            $"ResourceHealthWords {{ Label = {Badge}, Line = {Line}, Tone = {Tone}, Title = {Tip} }}",
            Full.ToString());
        Assert.Equal("ResourceHealthWords { Label = , Line = , Tone = , Title =  }", new ResourceHealthWords(null!, null!, null!, null!).ToString());
        Assert.Equal("ResourceHealthWords { Label = , Line = , Tone = , Title =  }", new ResourceHealthWords("", "", "", "").ToString());
        Assert.Equal(
            "ResourceHealthWords { Label =   , Line = \t, Tone = , Title = \n }",
            new ResourceHealthWords("  ", "\t", "", "\n").ToString());
        Assert.Equal(
            "ResourceHealthWords { Label = Not known, Line = , Tone = muted, Title =  }",
            new ResourceHealthWords("Not known", "", "muted", "").ToString());
        Assert.Equal(
            "ResourceHealthWords { Label = Available, Line = Resource Health, Tone = plain, Title = kv: available }",
            new ResourceHealthWords("Available", "Resource Health", "plain", "kv: available").ToString());

        // A box said twice is said the same, and the saying is the parts: not a reference, not a hash, not a type name.
        Assert.Equal(Full.ToString(), new ResourceHealthWords(Badge, Line, Tone, Tip).ToString());
        Assert.DoesNotContain("HashCode", Full.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("@", Full.ToString(), StringComparison.Ordinal);
    }

    /// <summary>The words are the record's own and none of the page's: no colour for the tone it is handed - a tone of
    /// <c>red</c> stands as plainly as <c>plain</c> does, since the rule that Resource Health is shown in neutral words
    /// is the making's and nothing in this record enforces it - and no placeholder, no markup, no dash and no invented
    /// <c>Not known</c> for a part nobody said. An empty badge is said as nothing, not refused and not replaced.</summary>
    [Fact]
    public void TheRecordsOwnWordsSayNothingOfThePagesMeaning()
    {
        var nothing = new ResourceHealthWords(null!, null!, null!, null!);
        var colour = new ResourceHealthWords(Badge, Line, "red", Tip);

        Assert.Equal("red", colour.Tone);
        Assert.Equal(new ResourceHealthWords(Badge, Line, "red", Tip), colour with { Tone = "red" });
        Assert.NotEqual(colour, colour with { Tone = "green" });
        Assert.Contains("Tone = red", colour.ToString(), StringComparison.Ordinal);

        Assert.DoesNotContain("—", nothing.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("n/a", nothing.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("Not known", nothing.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("muted", nothing.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("<span", nothing.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("class=", nothing.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("Azure", nothing.ToString(), StringComparison.Ordinal);

        // The record keeps no word of its own either: no constant, no default tone, no fallback badge. The words a
        // reader of a box sees are the making's, and they are pinned in ResourceHealthTextTests.
        Assert.Empty(typeof(ResourceHealthWords).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Empty(typeof(ResourceHealthWords).GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
    }

    /// <summary>The words of a box follow no reader at all: a badge, a line, a tone and a tooltip are letters, so the
    /// parts, their comparison, their hash and the record's own words are one thing whatever culture the thread asking
    /// happens to speak - a reader of German, and a reader whose language folds an <c>i</c> to a dotted capital, find
    /// the same box and the same words for it. Nothing is turned into a number, a date or a word of another
    /// language, and no length is measured in another reader's letters.</summary>
    [Fact]
    public void TheWordsFollowNoReaderAtAll()
    {
        var box = new ResourceHealthWords("İyi günler", "1,204.80 · 40 min ago", "plain", "kv: i, İ, ǅ\nAnd 3 more.");
        var same = new ResourceHealthWords("İyi günler", "1,204.80 · 40 min ago", "plain", "kv: i, İ, ǅ\nAnd 3 more.");

        Assert.Equal(box, same);
        Assert.Equal(box.GetHashCode(), same.GetHashCode());

        foreach (var reader in new[] { (CultureInfo)CultureInfo.InvariantCulture.Clone(), new CultureInfo("de-DE"), new CultureInfo("tr-TR") })
        {
            Assert.Equal(box.Label, Under(reader, () => box.Label));
            Assert.Equal(box.ToString(), Under(reader, () => box.ToString())!);
            Assert.Equal(box.GetHashCode(), Under(reader, () => box.GetHashCode()));
            Assert.Equal(box.Title.Length, Under(reader, () => box.Title.Length));
            Assert.True(Under(reader, () => box == same));
            Assert.False(Under(reader, () => box != same));
            Assert.False(Under(reader, () => box.Equals(new ResourceHealthWords("i̇yi günler", box.Line, box.Tone, box.Title))));
            Assert.Equal(
                "ResourceHealthWords { Label = İyi günler, Line = 1,204.80 · 40 min ago, Tone = plain, Title = kv: i, İ, ǅ\nAnd 3 more. }",
                Under(reader, () => new ResourceHealthWords("İyi günler", "1,204.80 · 40 min ago", "plain", "kv: i, İ, ǅ\nAnd 3 more.").ToString()));
        }

        ViewerCulture.German(() => Assert.Equal(Full.ToString(), new ResourceHealthWords(Badge, Line, Tone, Tip).ToString()));
    }

    // ------------------------------------------------------------ the box's declared shape

    /// <summary>The box's whole public surface, as it stands: the four words named in the order the constructor names
    /// them - the badge, the line, the tone, the tooltip - and the members every record is asked, falling apart,
    /// comparing, hashing and saying itself, and nothing else to call. All four are words, all four are init-only, and
    /// none of them is a moment, a duration, an address or a zone: that is why no test of a box needs a clock and none
    /// can reach the network. A fifth word added to the record has to be written here as well, which is the point: the
    /// day a box is given a colour to wear, this list says so instead of the page quietly wearing one.</summary>
    [Fact]
    public void TheBoxHasFourWordsAndTheMembersEveryRecordIsAsked()
    {
        var type = typeof(ResourceHealthWords);

        Assert.True(type.IsSealed);
        Assert.False(type.IsValueType);
        Assert.Equal([typeof(IEquatable<ResourceHealthWords>)], type.GetInterfaces());
        Assert.Equal(typeof(object), type.BaseType);

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();

        Assert.Equal(["Label", "Line", "Tone", "Title"], properties.Select(property => property.Name));
        Assert.All(properties, property =>
        {
            Assert.Equal(typeof(string), property.PropertyType);
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.NotNull(property.SetMethod);
            Assert.True(property.GetMethod!.IsPublic);

            // The four words are init-only: they are said once, at the making or at the copy, and never turned after.
            Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
        });

        // The contract every record compares by, and the words it prints its parts in, are the compiler's own: no part
        // of the box a caller can reach.
        Assert.Null(type.GetProperty("EqualityContract", BindingFlags.Public | BindingFlags.Instance));
        Assert.NotNull(type.GetProperty("EqualityContract", BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.Null(type.GetMethod("PrintMembers", BindingFlags.Public | BindingFlags.Instance));
        Assert.NotNull(type.GetMethod("PrintMembers", BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.DoesNotContain("EqualityContract", PublicSurface(), StringComparer.Ordinal);
        Assert.DoesNotContain("PrintMembers", PublicSurface(), StringComparer.Ordinal);

        // The four words, then the members every record is asked - in the reader-free order the list is written in,
        // which puts the record's words before its tone because a capital "S" comes before a lower-case "n".
        Assert.Equal(
            ["Deconstruct", "Equals", "GetHashCode", "Label", "Line", "Title", "ToString", "Tone", "op_Equality", "op_Inequality"],
            PublicSurface());

        var constructor = Assert.Single(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        var arguments = constructor.GetParameters();

        Assert.Equal(4, arguments.Length);
        Assert.Equal(["Label", "Line", "Tone", "Title"], arguments.Select(argument => argument.Name));
        Assert.All(arguments, argument => Assert.Equal(typeof(string), argument.ParameterType));
        Assert.All(arguments, argument => Assert.False(argument.HasDefaultValue, $"{argument.Name} may be left out of a box"));

        // The compiler's own copy constructor, and the two overloads of the equality plus the one Deconstruct.
        var copy = Assert.Single(type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.Equal(type, copy.GetParameters().Single().ParameterType);
        Assert.Equal(2, type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Count(method => method.Name == "Equals"));
        var parts = type.GetMethod("Deconstruct")!;
        Assert.Equal(["Label", "Line", "Tone", "Title"], parts.GetParameters().Select(parameter => parameter.Name));
        Assert.All(parts.GetParameters(), parameter => Assert.True(parameter.IsOut));

        // Nothing of a moment, a zone, an address or a wait on it: the record is the four words and nothing to call.
        var everything = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).ToList();

        Assert.DoesNotContain(everything, method => method.ReturnType == typeof(Task) || method.ReturnType.Name.StartsWith("ValueTask", StringComparison.Ordinal));
        Assert.DoesNotContain(everything, method => method.GetParameters().Any(parameter => parameter.ParameterType == typeof(CancellationToken)));
        Assert.DoesNotContain(properties, property => IsMoment(property.PropertyType));
        Assert.DoesNotContain(arguments, argument => IsMoment(argument.ParameterType));
        Assert.DoesNotContain(arguments, argument => argument.ParameterType == typeof(TimeZoneInfo) || argument.ParameterType == typeof(Uri));
    }

    // -------------------------------------------------------- the one way the page makes these words

    /// <summary>The zone a box's tooltip tells its reading times in is refused before anything else is asked: a call
    /// with no zone is refused with the name of the part that is not there, whatever the ids are - none of them,
    /// nothing at all, or a box the page has no report for - and the making refuses before it decides what a box
    /// without ids has to say. Every other answer of the making, and every word it puts in these four parts, is
    /// <see cref="ResourceHealthTextTests" />; this one boundary of it was held by nobody, and the record itself has no
    /// say in it. Nothing here reaches the network and no clock is asked: the time is the test's own.</summary>
    [Fact]
    public void TheMakingRefusesABoxWhoseZoneIsNotThereBeforeItLooksAtTheIds()
    {
        var now = new DateTimeOffset(2026, 10, 4, 22, 17, 0, TimeSpan.Zero);
        var report = ResourceHealthReport.Parse($$"""
            { "resources": { "nonprod": [
              { "id": "{{KeyVault}}", "health": { "status": "available", "source": "resourceHealth", "checkedAt": "2026-10-04T21:37:00Z", "summary": "No incidents reported." } } ] } }
            """)!;

        Assert.Equal("zone", Assert.Throws<ArgumentNullException>(() => ResourceHealthText.Of([KeyVault], report, now, null!)).ParamName);
        Assert.Equal("zone", Assert.Throws<ArgumentNullException>(() => ResourceHealthText.Of(null, report, now, null!)).ParamName);
        Assert.Equal("zone", Assert.Throws<ArgumentNullException>(() => ResourceHealthText.Of([], null, null, null!)).ParamName);

        // The same calls with a zone the test made answer as they always did: a box with no ids is still no words at
        // all, and a box of one is a box with its four parts said - which words they are is ResourceHealthTextTests'.
        Assert.Null(ResourceHealthText.Of([], report, now, TimeZoneInfo.Utc));
        Assert.Null(ResourceHealthText.Of(null, report, now, TimeZoneInfo.Utc));

        var box = ResourceHealthText.Of([KeyVault], report, now, TimeZoneInfo.Utc);

        Assert.NotNull(box);
        Assert.All(new[] { box!.Label, box.Line, box.Tone, box.Title }, part => Assert.False(string.IsNullOrEmpty(part), "a box made of a reading said one of its four parts as nothing"));
    }

    // ------------------------------------------------------------------------------ the machinery

    /// <summary>The public members the box declares itself: its properties and the methods that are not the get and the
    /// set of one - the two operators kept although the compiler names them specially, since they are the <c>==</c>
    /// and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The constructor is no
    /// method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(ResourceHealthWords)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>True for the kinds of moment a box would have to be told about, or would have to go and ask a clock
    /// for - none of which a box holds, takes, or hands back.</summary>
    private static bool IsMoment(Type type) =>
        type == typeof(DateTimeOffset) || type == typeof(DateTime) || type == typeof(TimeSpan) || type == typeof(DateOnly) || type == typeof(TimeOnly);

    /// <summary>The letters of a word gathered into a string of its own, which no other word in the test shares a
    /// reference with: what the record does with a word it is given - keeping the one and not another that spells the
    /// same - is only seen when the two are apart. (The empty word is the interned one whatever this does, which is why
    /// the tests beside it say the length as well as the reference.)</summary>
    private static string Own(string word) => new(word.ToCharArray());

    /// <summary>Asking a box for a word, its words or its hash under another reader - and handing the thread's own
    /// culture back afterwards, whatever the asking did.</summary>
    private static T Under<T>(CultureInfo culture, Func<T> what)
    {
        ArgumentNullException.ThrowIfNull(culture);
        ArgumentNullException.ThrowIfNull(what);

        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = culture;
            return what();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }
}
