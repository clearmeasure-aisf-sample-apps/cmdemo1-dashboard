using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ActivityLine" /> - the one record
/// <c>src/Dashboard/Health/DeploymentActivity.cs</c> declares for a single line of what is happening, or just
/// happened: the word its class and the runtime payload carry, the short words, the sentence of the tooltip, and the
/// address of the task in Octopus Deploy that may be absent. The record is a carrier of those four and of nothing
/// else: it is sealed, it has one constructor, and it works out nothing - every word of it the page is given.
/// <para>
/// What is pinned where it belongs, and is not repeated here: <see cref="DeploymentActivityTests" /> and
/// <see cref="DeploymentActivityEdgeCaseTests" /> pin the four parts as the sayers of <see cref="DeploymentActivity" />
/// hand them over - the kind a state of the file is turned into, the words a node's line cut to thirty-six characters,
/// the sentence with the times and the responsibility in it, the address of the task where the file gives one and its
/// absence for a freeze - and they pin a line made of one kind, <c>waiting</c>, being no line made of another;
/// <see cref="ActivityKindEdgeCaseTests" /> pins the eight words a kind is one of, as the fixed values they are;
/// and <see cref="RuntimePayloadBuilderTests" /> pins a line read of its four parts to become a line of the runtime
/// payload - the kind as the tile's tone, the words as the tile's text, the sentence and the address as its link.
/// </para>
/// <para>
/// What all of that leaves open, and what is pinned here, is the record as it stands once it is made: the four
/// arguments handed to the property of their name and held as they were held out - the very words, in their own
/// letters, with nothing cut off either end, no trimming, no case changed and no re-writing of the address, and the
/// fourth argument left out being the absence of one rather than a placeholder for one; nothing asked of any of the
/// four, so that the empty word, a word of nothing but whitespace, a word of a thousand letters and a word that is
/// not there at all are each kept, and a relative address is as much an address as an absolute one; the line fallen
/// apart into its four parts in the order they went in, and the copy that turns one part while it keeps the other
/// three; two lines being one line while their four parts are - the three words compared as words, in the case they
/// are written in, and the address as the address compares itself, which leaves the fragment out; no line ever being
/// a line that is not there, a word, a number or a record of another file; the hash code following that equality into
/// a set and under a dictionary; the words a line says of itself, the four labels in the order the record declares
/// them, nothing quoted and nothing escaped, and nothing said of which of the four is the absence of one; and the
/// public surface, so that the day a fifth part is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a line holds three words and an
/// address, no moment and no count, and calls nothing at all. Every address below is named on
/// <c>octopus.example.invalid</c>, a host that resolves nowhere, and no member of the record opens one - the
/// <see cref="Uri" /> is kept as it was given and is never asked for its answer.
/// </para>
/// </summary>
public class ActivityLineEdgeCaseTests
{
    /// <summary>An address named on a host that resolves nowhere: the task in Octopus Deploy a line may carry.</summary>
    private const string Address = "https://octopus.example.invalid/app#/Spaces-5";

    /// <summary>The task above as the address a line carries - the one address the tests below hold.</summary>
    private static readonly Uri Task = new(Address);

    /// <summary>Another task of the same instance: another path, so another address however the fragment runs.</summary>
    private static readonly Uri OtherTask = new("https://octopus.example.invalid/app/tasks/4");

    /// <summary>The four labels a line says its parts under, in the order the record declares them.</summary>
    private static readonly string[] Labels = ["Kind = ", "Text = ", "Title = ", "Url = "];

    /// <summary>The kind's word, in the pieces it is joined from while the test runs.</summary>
    private static readonly string[] KindPieces = ["wait", "ing"];

    /// <summary>The short words of a deployment stopped for a person, in the pieces they are joined from.</summary>
    private static readonly string[] WordsPieces = ["2.4.43", "waits", "for", "demo", "approvers"];

    /// <summary>The sentence of the same deployment, in the pieces it is joined from.</summary>
    private static readonly string[] SentencePieces = ["demo-ui", "2.4.43", "waits", "for", "a", "sign-off", "in", "prod"];

    /// <summary>A line with all four of its parts: a deployment stopped for a person, on the task it waits for.</summary>
    private static ActivityLine Full() =>
        new("waiting", "2.4.43 waits for demo approvers", "demo-ui 2.4.43 waits for a sign-off in prod", Task);

    // ------------------------------ the four parts, as they are handed over and as they are held

    /// <summary>The constructor hands each of its four arguments to the property of its name, in the order the record
    /// declares them, and holds the very word and the very address it was handed: the words are the strings the
    /// caller made, not copies of them, and the address is the <see cref="Uri" /> the caller made, not the text it was
    /// written from.</summary>
    [Fact]
    public void TheFourPartsGoToThePropertyOfTheirNameAndAreHeldAsTheyWereGiven()
    {
        var kind = string.Concat(KindPieces);
        var words = string.Join(' ', WordsPieces);
        var sentence = string.Join(' ', SentencePieces);
        var line = new ActivityLine(kind, words, sentence, Task);

        Assert.False(ReferenceEquals(kind, "waiting"));
        Assert.False(ReferenceEquals(words, "2.4.43 waits for demo approvers"));
        Assert.False(ReferenceEquals(sentence, "demo-ui 2.4.43 waits for a sign-off in prod"));
        Assert.Equal("waiting", kind);
        Assert.Same(kind, line.Kind);
        Assert.Same(words, line.Text);
        Assert.Same(sentence, line.Title);
        Assert.Same(Task, line.Url);
        Assert.Equal(
            ("waiting", "2.4.43 waits for demo approvers", "demo-ui 2.4.43 waits for a sign-off in prod", Address),
            (line.Kind, line.Text, line.Title, line.Url!.ToString()));
    }

    /// <summary>The words a line is given are kept in their own letters: a word padded either end, a word in another
    /// case, a word of nothing but a space or a tab, a word of letters separated, a word of letters no reader of the
    /// page reads, the ellipsis the node's cut leaves behind and a word of a whole sentence - none of them trimmed,
    /// re-cased, shortened or otherwise re-written, and not one of them a kind the class says.</summary>
    [Theory]
    [InlineData(" waiting ")]
    [InlineData("Waiting")]
    [InlineData(" ")]
    [InlineData("WAITING")]
    [InlineData("w a i t i n g")]
    [InlineData("waiting\t")]
    [InlineData("\u2026")]
    [InlineData("\u00e9\u2603")]
    [InlineData("the second sign-off question of the release week")]
    public void TheWordsAreHeldInTheirsOwnLettersWithNothingCutOffEitherEnd(string words)
    {
        var line = new ActivityLine(words, words, words, null);

        Assert.Equal(words, line.Kind);
        Assert.Equal(words, line.Text);
        Assert.Equal(words, line.Title);
        Assert.Same(words, line.Text);
        Assert.Equal(words.Length, line.Text.Length);
    }

    /// <summary>A word of a thousand letters is held whole, and is no word the record shortens for a reader: the line
    /// keeps no length of its own, since the thirty-six characters a node holds are the cut's work and not the
    /// record's - so a line that is never cut says every letter it was given, holds them in the very string it was
    /// given them in, and shortens nothing on the way to its own words.</summary>
    [Fact]
    public void AWordOfAThousandLettersIsHeldWholeAndNeverShortened()
    {
        var thousand = new string('x', 1_000);
        var line = new ActivityLine(thousand, thousand, thousand, null);

        Assert.Same(thousand, line.Text);
        Assert.Equal(1_000, line.Kind.Length);
        Assert.Equal(1_000, line.Text.Length);
        Assert.Equal(1_000, line.Title.Length);
        Assert.DoesNotContain("\u2026", line.Text);
        Assert.DoesNotContain("\u2026", line.ToString());
        Assert.Equal("ActivityLine { Kind = " + thousand + ", Text = " + thousand + ", Title = " + thousand + ", Url =  }", line.ToString());
    }

    /// <summary>A word of nothing is a word the line keeps: the empty word in each of the three places, held as the
    /// empty word and not as the absence of one, and a line of three empty words and no address is a line rather than
    /// no line. The empty word is no kind the class says, and the line carries it anyway; a word of one space is no
    /// empty word to it.</summary>
    [Fact]
    public void AWordOfNothingIsAWordTheLineKeeps()
    {
        var line = new ActivityLine(string.Empty, string.Empty, string.Empty, null);

        Assert.Same(string.Empty, line.Kind);
        Assert.Empty(line.Text);
        Assert.Empty(line.Title);
        Assert.Null(line.Url);
        Assert.Equal(line, new ActivityLine("", "", ""));
        Assert.NotEqual(line, new ActivityLine(" ", " ", " "));
    }

    /// <summary>The line asks nothing of the three words it is given: they are declared as words that are not to be
    /// absent, and no guard stands between that declaration and a caller, so a line of three words that are not there
    /// is made, is held, is equal to another such line, is copied, and says its four labels with nothing after any of
    /// them - which is what the page would have to be careful of, were anything able to hand such a line over.</summary>
    [Fact]
    public void TheLineAsksNothingOfTheThreeWordsItIsGiven()
    {
        var line = new ActivityLine(null!, null!, null!, null);

        Assert.Null(line.Kind);
        Assert.Null(line.Text);
        Assert.Null(line.Title);
        Assert.Null(line.Url);
        Assert.Equal(line, line with { });
        Assert.Equal(line, new ActivityLine(null!, null!, null!, null));
        Assert.Equal(line.GetHashCode(), new ActivityLine(null!, null!, null!, null).GetHashCode());
        Assert.Equal("ActivityLine { Kind = , Text = , Title = , Url =  }", line.ToString());
    }

    /// <summary>The fourth part left out is the absence of an address rather than a placeholder for one: a line of
    /// three arguments carries no address, is the very line the same three arguments and an explicit absence make,
    /// and is no line of any address - which is how a freeze's line, and the line of a file that names no task, stand
    /// apart from the lines that lead a reader to Octopus Deploy.</summary>
    [Fact]
    public void TheAddressLeftOutIsNoAddressRatherThanAPlaceholder()
    {
        var without = new ActivityLine("queued", "2.4.44 queued, 3 min", "demo-ui 2.4.44 is queued in uat");

        Assert.Null(without.Url);
        Assert.Equal(without, new ActivityLine("queued", "2.4.44 queued, 3 min", "demo-ui 2.4.44 is queued in uat", null));
        Assert.Equal(without.GetHashCode(), new ActivityLine("queued", "2.4.44 queued, 3 min", "demo-ui 2.4.44 is queued in uat", null).GetHashCode());
        Assert.NotEqual(without, without with { Url = Task });
        Assert.NotEqual(without, new ActivityLine("queued", "2.4.44 queued, 3 min", "demo-ui 2.4.44 is queued in uat", OtherTask));
    }

    /// <summary>The line asks nothing of the address it is given either: a relative address, an address of a port the
    /// scheme already implies and an address of an instance named with no path at all are each held as the
    /// <see cref="Uri" /> that was handed over - the record opens none of them and asks none to be absolute - and each
    /// is told in its own words, which are not always the words it was written in: the port the scheme implies is
    /// gone from the address, and the instance named with no path says the path of one.</summary>
    [Fact]
    public void TheLineAsksNothingOfTheAddressItIsGiven()
    {
        var relative = new ActivityLine("frozen", "frozen until Mon 00:00: weekend", "Deployment freeze weekend", new Uri("/app", UriKind.Relative));
        var impliedPort = new ActivityLine("frozen", "frozen until Mon 00:00: weekend", "Deployment freeze weekend", new Uri("http://octopus.example.invalid:80/app"));
        var instance = new ActivityLine("frozen", "frozen until Mon 00:00: weekend", "Deployment freeze weekend", new Uri("https://octopus.example.invalid"));

        Assert.False(relative.Url!.IsAbsoluteUri);
        Assert.Equal("/app", relative.Url.ToString());
        Assert.True(impliedPort.Url!.IsAbsoluteUri);
        Assert.Equal("http://octopus.example.invalid/app", impliedPort.Url.ToString());
        Assert.Equal(80, impliedPort.Url.Port);
        Assert.NotEqual(relative.Url, impliedPort.Url);
        Assert.NotEqual(relative, impliedPort);
        Assert.Equal("https://octopus.example.invalid/", instance.Url!.ToString());
        Assert.Contains("Url = https://octopus.example.invalid/ }", instance.ToString(), StringComparison.Ordinal);
    }

    // --------------------------------------- the line fallen apart, and the line copied

    /// <summary>The line falls apart into its four parts, in the order the record declares them and each under the
    /// name it is declared under - the kind, the words, the sentence, the address - and a line whose parts are not
    /// there falls apart as the absence of them, which is how a caller reads a line without asking the record.</summary>
    [Fact]
    public void TheLineFallsApartIntoItsFourPartsInTheOrderTheyWereGiven()
    {
        var (kind, words, sentence, address) = Full();

        Assert.Equal("waiting", kind);
        Assert.Equal("2.4.43 waits for demo approvers", words);
        Assert.Equal("demo-ui 2.4.43 waits for a sign-off in prod", sentence);
        Assert.Equal(Task, address);

        var (noKind, noWords, noSentence, noAddress) = new ActivityLine(null!, null!, null!, null);

        Assert.Null(noKind);
        Assert.Null(noWords);
        Assert.Null(noSentence);
        Assert.Null(noAddress);
    }

    /// <summary>A copy of a line is the line it was copied from, and is not the same line: the copy that names no part
    /// holds all four and stands on its own, and the copy that names one part turns that part and hands the other
    /// three over unturned - which is how the node's short line is made from the full one, and how a test turns a
    /// single part of a line to ask the equality what it makes of it.</summary>
    [Fact]
    public void ACopyIsTheLineWithOnePartTurnedAndTheOtherThreeKept()
    {
        var line = Full();

        var copy = line with { };
        Assert.Equal(line, copy);
        Assert.NotSame(line, copy);

        var turnedKind = line with { Kind = "deploying" };
        Assert.Equal(("deploying", line.Text, line.Title, line.Url), (turnedKind.Kind, turnedKind.Text, turnedKind.Title, turnedKind.Url));

        var turnedWords = line with { Text = "deploying 2.4.43, 3 min" };
        Assert.Equal((line.Kind, "deploying 2.4.43, 3 min", line.Title, line.Url), (turnedWords.Kind, turnedWords.Text, turnedWords.Title, turnedWords.Url));

        var turnedSentence = line with { Title = "deploying demo-ui 2.4.43 to uat" };
        Assert.Equal((line.Kind, line.Text, "deploying demo-ui 2.4.43 to uat", line.Url), (turnedSentence.Kind, turnedSentence.Text, turnedSentence.Title, turnedSentence.Url));

        var turnedAddress = line with { Url = OtherTask };
        Assert.Equal((line.Kind, line.Text, line.Title, OtherTask), (turnedAddress.Kind, turnedAddress.Text, turnedAddress.Title, turnedAddress.Url));

        var erased = line with { Url = null };
        Assert.Null(erased.Url);
        Assert.Equal((line.Kind, line.Text, line.Title), (erased.Kind, erased.Text, erased.Title));
        Assert.NotEqual(line, erased);

        var repacked = line with { Kind = line.Kind, Text = line.Text, Title = line.Title, Url = line.Url };
        Assert.Equal(line, repacked);
        Assert.NotSame(line, repacked);
    }

    // ------------------------------------------------------------- the lines that are one line

    /// <summary>Two lines are one line while their four parts are, whoever holds them: two built apart from the same
    /// four arguments are one, in the record's own equality, in the equality asked of an <see cref="object" />, in
    /// both operators and in the hash code - and a difference in any one of the four, an address turned, an address
    /// that was there left out, or a word re-written, is another line.</summary>
    [Fact]
    public void TwoLinesAreOneWhileTheirFourPartsAreAndAnotherWhenAnyOneOfThemIs()
    {
        var one = Full();
        var two = new ActivityLine("waiting", "2.4.43 waits for demo approvers", "demo-ui 2.4.43 waits for a sign-off in prod", new Uri(Address));

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.True(one.Equals(two));
        Assert.True(one.Equals((object)two));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());

        var turned = new[]
        {
            one with { Kind = "deploying" },
            one with { Text = "2.4.43 waits for a sign-off" },
            one with { Title = "demo-ui 2.4.43 waits for a sign-off in uat" },
            one with { Url = OtherTask },
            one with { Url = null },
        };

        Assert.All(turned, other =>
        {
            Assert.NotEqual(one, other);
            Assert.True(one != other);
            Assert.True(other != one);
            Assert.False(one.Equals(other));
            Assert.False(other.Equals(one));
            Assert.False(one.Equals((object)other));
        });
    }

    /// <summary>No line is a line that is not there, however the question is put: the record's own equality, the
    /// equality asked of an <see cref="object" /> and both operators all say so; the absence of one is the absence of
    /// one to itself, and is no line's equal; and no hash code and no word is asked of it, which is what lets the page
    /// ask whether a node has a line at all before it reads one.</summary>
    [Fact]
    public void ALineIsNeverALineThatIsNotThere()
    {
        ActivityLine? none = null;
        ActivityLine? another = null;
        var line = Full();

        Assert.False(line.Equals(null));
        Assert.False(line.Equals((object?)null));
        Assert.False(line == none);
        Assert.False(none == line);
        Assert.True(line != none);
        Assert.True(none != line);
        Assert.True(none == another);
        Assert.False(none != another);
        Assert.True(line is not null);
        Assert.False(Equals(none, line));
        Assert.True(Equals(none, another));
        Assert.False(Equals(line, none));
    }

    /// <summary>A line is no word, no number, no address, no tuple of its own four parts and no record of another
    /// file, whatever those hold: the equality asked of an <see cref="object" /> answers no to everything that is not
    /// a line, so a line and the single word of its own kind are two things even when the word is the word the line
    /// carries.</summary>
    [Fact]
    public void ALineIsNoWordNoNumberAndNoOtherRecord()
    {
        var line = Full();

        Assert.False(line.Equals("waiting"));
        Assert.False(line.Equals((object)"waiting"));
        Assert.False(line.Equals(5));
        Assert.False(line.Equals(Task));
        Assert.False(line.Equals(("waiting", "2.4.43 waits for demo approvers", "demo-ui 2.4.43 waits for a sign-off in prod", Task)));
        Assert.False(line.Equals(new DeploymentWait("sign-off", "Sign-off", null, "demo approvers")));
        Assert.False(Equals(line, "waiting"));
        Assert.True(line.Equals(line with { }));
    }

    /// <summary>The three words of a line are compared as words and not as what they stand for: in the case they are
    /// written in, so <c>Waiting</c> is no <c>waiting</c>; with the whitespace in them and at their ends, so a word of
    /// one space is no empty word; and by their absence, so a word that is not there is no empty word either. The
    /// eight kinds the page says are eight lines for it, and the two words of a freeze stand apart on their single
    /// letter.</summary>
    [Fact]
    public void TheWordsOfALineAreComparedAsWordsAndNotAsWhatTheyStandFor()
    {
        var line = Full();

        Assert.NotEqual(line, line with { Kind = "Waiting" });
        Assert.NotEqual(line, line with { Kind = "waiting " });
        Assert.NotEqual(line, line with { Text = "2.4.43 waits for demo approvers " });
        Assert.NotEqual(line, line with { Text = "2.4.43 Waits for demo approvers" });
        Assert.NotEqual(line, line with { Title = string.Empty });
        Assert.NotEqual(line, new ActivityLine(null!, line.Text, line.Title, line.Url));
        Assert.NotEqual(new ActivityLine(" ", " ", " ", null), new ActivityLine(string.Empty, string.Empty, string.Empty, null));

        List<ActivityLine> eight = [.. ActivityKind.All.Select(kind => new ActivityLine(kind, "the same words", "the same sentence"))];

        Assert.Equal(8, eight.Distinct().Count());
        Assert.NotEqual(new ActivityLine(ActivityKind.Frozen, "w", "t"), new ActivityLine(ActivityKind.Freeze, "w", "t"));
        Assert.NotEqual(new ActivityLine(ActivityKind.Deployed, "w", "t"), new ActivityLine(ActivityKind.Deploying, "w", "t"));
    }

    /// <summary>The address is compared as an address compares itself, and is never opened - and an address's own
    /// comparison is not the whole text of it: two tasks of one Octopus instance that differ only in the fragment
    /// after the <c>#</c> are one address to the <see cref="Uri" />, so the two lines that carry them are one line and
    /// share a hash code - the same address asked with its fragment left out of the text is one address too - while
    /// another path or another query is another address and another line. The page is shown one line where the file
    /// named two tasks; <c>#232</c> is the same trap on an address the page hangs a reader on, and the same note
    /// stands here: a line that means to tell two tasks apart tells them by their paths, which is what the shipped
    /// file writes.</summary>
    [Fact]
    public void TheAddressIsComparedAsAnAddressOfItsOwnWhichLeavesTheFragmentOut()
    {
        var line = Full();
        var anotherFragment = line with { Url = new Uri("https://octopus.example.invalid/app#/Spaces-4") };
        var noFragment = line with { Url = new Uri("https://octopus.example.invalid/app") };
        var anotherPath = line with { Url = OtherTask };
        var anotherQuery = line with { Url = new Uri("https://octopus.example.invalid/app?filter=x#/Spaces-5") };

        Assert.NotEqual(Address, "https://octopus.example.invalid/app#/Spaces-4");
        Assert.NotEqual(Address, "https://octopus.example.invalid/app");
        Assert.Equal(Task, new Uri(Address));

        Assert.Equal(line, anotherFragment);
        Assert.Equal(line.GetHashCode(), anotherFragment.GetHashCode());
        Assert.True(line == anotherFragment);
        Assert.False(line != anotherFragment);

        Assert.Equal(line, noFragment);
        Assert.Equal(line.GetHashCode(), noFragment.GetHashCode());
        Assert.NotEqual(line.ToString(), noFragment.ToString());

        Assert.NotEqual(line, anotherPath);
        Assert.NotEqual(line, anotherQuery);

        // The address is held as the address and never as the text it was written in, so two texts the address
        // makes one are one line: the instance named with no path, and the same instance named with the path of one.
        Assert.Equal(
            new ActivityLine("frozen", "w", "t", new Uri("https://octopus.example.invalid")),
            new ActivityLine("frozen", "w", "t", new Uri("https://octopus.example.invalid/")));
    }

    /// <summary>Lines that are one line are one in a set, one under a dictionary, one to a list's search and one to
    /// <c>Distinct</c>: the copies of a line and the lines built apart from its four parts add nothing, and the
    /// address that is not there is a part like any other - a line of no address and a line of one are two, which is
    /// what keeps a node's two lines from folding into one.</summary>
    [Fact]
    public void LinesThatAreOneLineAreOneInASetAndUnderADictionary()
    {
        var line = Full();
        var set = new HashSet<ActivityLine> { line, line with { }, Full(), new ActivityLine("waiting", "2.4.43 waits for demo approvers", "demo-ui 2.4.43 waits for a sign-off in prod", new Uri(Address)) };

        Assert.Single(set);
        Assert.Contains(line with { }, set);
        Assert.Equal(2, new HashSet<ActivityLine> { line, line with { Url = null } }.Count);
        Assert.Equal(8, ActivityKind.All.Select(kind => new ActivityLine(kind, "w", "t")).ToHashSet().Count);

        var folded = new[] { line, line with { }, line with { Url = OtherTask } }.Distinct().ToArray();

        Assert.Equal(2, folded.Length);
        Assert.Same(line, folded[0]);
        Assert.Equal(line with { Url = OtherTask }, folded[1]);
        Assert.Contains(line with { }, new[] { line, line with { Url = OtherTask } });

        var keys = new Dictionary<ActivityLine, int> { [line] = 1 };

        Assert.Equal(1, keys[Full()]);
        Assert.Equal(1, keys[new ActivityLine(line.Kind, line.Text, line.Title, new Uri(Address))]);
        Assert.False(keys.ContainsKey(line with { Url = null }));
    }

    // ------------------------------------------------- the words a line says of itself

    /// <summary>A line says its four parts of its own: each under the label of the name the record declares it under,
    /// in that order - the kind, the words, the sentence, the address - the whole in the type's name and a pair of
    /// braces, the parts divided by a comma and a space, and every word said in its own letters with nothing around
    /// it.</summary>
    [Fact]
    public void ALineSaysItsFourPartsByTheirNamesInTheOrderTheRecordDeclaresThem()
    {
        Assert.Equal(
            "ActivityLine { Kind = waiting, Text = 2.4.43 waits for demo approvers, Title = demo-ui 2.4.43 waits for a sign-off in prod, Url = https://octopus.example.invalid/app#/Spaces-5 }",
            Full().ToString());
        Assert.Equal(
            "ActivityLine { Kind = deployed, Text = 1.0.3 reached uat 4 min ago, Title = demo-scorecard 1.0.3 reached uat 4 min ago, Url =  }",
            new ActivityLine("deployed", "1.0.3 reached uat 4 min ago", "demo-scorecard 1.0.3 reached uat 4 min ago").ToString());
    }

    /// <summary>A line whose words are nothing still says its four labels, and says nothing after each of them: the
    /// empty word, the word of nothing but whitespace and the word that is not there are said the same way but not in
    /// the same number of characters, and the line made of four arguments that say nothing is no default - a line is
    /// never not a line, and no line stands about until one is made.</summary>
    [Fact]
    public void ALineWhoseWordsAreNothingSaysItsFourLabelsAnyway()
    {
        Assert.Equal("ActivityLine { Kind = , Text = , Title = , Url =  }", new ActivityLine(string.Empty, string.Empty, string.Empty, null).ToString());
        Assert.Equal("ActivityLine { Kind =  , Text =  , Title =  , Url =  }", new ActivityLine(" ", " ", " ", null).ToString());
        Assert.Equal("ActivityLine { Kind = , Text = , Title = , Url =  }", new ActivityLine(null!, null!, null!, null).ToString());
        Assert.Null(default(ActivityLine));
    }

    /// <summary>A line escapes nothing in its own words: the comma that divides its parts is said inside a part as the
    /// comma it is, a line break of the tooltip is said as a line break, the braces that close the whole are said
    /// inside a word, and the letters no reader of the page reads are said as themselves - so the words a line says of
    /// itself are for a reader and are no format, and nothing can be read back out of them.</summary>
    [Fact]
    public void ALineEscapesNothingInItsOwnWords()
    {
        var line = new ActivityLine("a, b", "c\nd", "e {f} g", new Uri("https://octopus.example.invalid/x?y=1&z=2#frag"));

        Assert.Equal("ActivityLine { Kind = a, b, Text = c\nd, Title = e {f} g, Url = https://octopus.example.invalid/x?y=1&z=2#frag }", line.ToString());
        Assert.Equal(
            "ActivityLine { Kind = \u2026, Text = \u00e9\u2603, Title = \u2026\u00e9\u2603, Url =  }",
            new ActivityLine("\u2026", "\u00e9\u2603", "\u2026\u00e9\u2603").ToString());
        Assert.DoesNotContain("\"", line.ToString(), StringComparison.Ordinal);

        // The comma divides the parts and is no escape: a word with one in it is said whole, so the four labels, and
        // only they, are what a reader can count.
        Assert.Contains("Kind = a, b,", line.ToString(), StringComparison.Ordinal);
        Assert.Equal(4, Labels.Count(label => line.ToString().Contains(label, StringComparison.Ordinal)));
    }

    /// <summary>The words a line says tell nothing of which of its four parts is the absence of one: a line of three
    /// empty words and no address and a line of three words that are not there and no address say themselves
    /// identically, while the equality - and the page's link, which asks whether an address is there before it hangs
    /// one - tells them apart.</summary>
    [Fact]
    public void TheWordsOfALineSayNothingOfWhichOfItsPartsAreAbsent()
    {
        var empty = new ActivityLine(string.Empty, string.Empty, string.Empty, null);
        var absent = new ActivityLine(null!, null!, null!, null);

        Assert.Equal(empty.ToString(), absent.ToString());
        Assert.NotEqual(empty, absent);
        Assert.False(empty.Equals(absent));
        Assert.NotEqual(empty, new ActivityLine(string.Empty, string.Empty, string.Empty, new Uri("https://octopus.example.invalid/")));
        Assert.NotEqual(absent, new ActivityLine(null!, null!, null!, new Uri("https://octopus.example.invalid/")));
    }

    // ------------------------------------------------------------- the record's own shape

    /// <summary>The whole public surface of the record, as it stands: the four parts, their readers and their
    /// writers, the one constructor - whose fourth argument is the optional one, defaulted to the absence of an
    /// address - the copy a <c>with</c> calls, the four parts fallen apart, the two equality questions and the words
    /// and the number the page asks a line, the two operators, and nothing else to call. A member added to the record
    /// has to be written here as well, which is the point: a line the page reads is a line somebody decided what it
    /// holds.</summary>
    [Fact]
    public void TheSurfaceIsTheFourPartsTheirReadersTheirCopyTheirWordsAndTheTwoOperators()
    {
        Assert.Equal(
            [
                ".ctor", "<Clone>$", "Deconstruct", "Equals", "GetHashCode", "Kind", "Text", "Title", "ToString", "Url", "get_Kind", "get_Text", "get_Title", "get_Url",
                "op_Equality", "op_Inequality", "set_Kind", "set_Text", "set_Title", "set_Url",
            ],
            PublicSurface());
        Assert.Equal(["Kind", "Text", "Title", "Url"], PropertyNames());
        Assert.DoesNotContain(typeof(ActivityLine).GetFields(BindingFlags.Instance | BindingFlags.DeclaredOnly), field => !field.IsPrivate);
        Assert.False(typeof(ActivityLine).IsDefined(typeof(CompilerGeneratedAttribute), false));
    }

    /// <summary>The four parts are written as the line is made and read ever after: each a public property of its own
    /// name, with a public reader no one may extend and a writer only the making of a line reaches, the three words
    /// declared as words that are not to be absent and the address as the one part that may be - the promise the
    /// record makes, which as the tests above say it keeps nobody.</summary>
    [Fact]
    public void TheFourPartsAreWrittenAtTheMakingAndReadEverAfter()
    {
        var nullability = new NullabilityInfoContext();

        foreach (var name in new[] { "Kind", "Text", "Title" })
        {
            var part = typeof(ActivityLine).GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;

            Assert.Equal(typeof(string), part.PropertyType);
            Assert.True(part.GetMethod!.IsPublic);
            Assert.False(part.GetMethod.IsVirtual);
            Assert.True(part.CanWrite);
            Assert.True(part.SetMethod!.IsPublic);
            Assert.False(part.SetMethod.IsVirtual);
            Assert.Contains(typeof(IsExternalInit), part.SetMethod.ReturnParameter.GetRequiredCustomModifiers());
            Assert.Equal(NullabilityState.NotNull, nullability.Create(part).WriteState);
        }

        var address = typeof(ActivityLine).GetProperty("Url", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;

        Assert.Equal(typeof(Uri), address.PropertyType);
        Assert.True(address.GetMethod!.IsPublic);
        Assert.True(address.CanWrite);
        Assert.Contains(typeof(IsExternalInit), address.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
        Assert.Equal(NullabilityState.Nullable, nullability.Create(address).WriteState);

        var made = Assert.Single(typeof(ActivityLine).GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        var arguments = made.GetParameters();

        Assert.Equal(["Kind", "Text", "Title", "Url"], [.. arguments.Select(argument => argument.Name!)]);
        Assert.False(arguments[0].HasDefaultValue);
        Assert.False(arguments[1].HasDefaultValue);
        Assert.False(arguments[2].HasDefaultValue);
        Assert.True(arguments[3].HasDefaultValue);
        Assert.Null(arguments[3].DefaultValue);
        Assert.Equal(NullabilityState.Nullable, nullability.Create(arguments[3]).WriteState);
    }

    /// <summary>The line is sealed, so nothing is a line on top of it: it stands on <see cref="object" /> alone and
    /// wears one interface, the typed equality of its own kind. That equality is the one the page's questions go
    /// through and no one may answer it again; the words a line says of itself are private to it, since a sealed line
    /// has nothing under it to say them for, and the number it stands on is not; the copy a <c>with</c> is made of is
    /// the private constructor of the line it copies, so no one outside the record can rebuild a line by hand; and
    /// the equality contract the record wears is nobody else's to read.</summary>
    [Fact]
    public void TheLineIsSealedItsEqualityIsTypedAndItsWordsAreItsOwn()
    {
        var type = typeof(ActivityLine);

        Assert.True(type.IsSealed);
        Assert.False(type.IsAbstract);
        Assert.Equal(typeof(object), type.BaseType);
        Assert.Same(typeof(IEquatable<ActivityLine>), Assert.Single(type.GetInterfaces()));

        var typed = type.GetMethod("Equals", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, [typeof(ActivityLine)], null)!;

        Assert.True(typed.IsVirtual);
        Assert.True(typed.IsFinal);
        Assert.Equal(typeof(bool), typed.ReturnType);

        var words = type.GetMethod("PrintMembers", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly, null, [typeof(StringBuilder)], null)!;

        Assert.True(words.IsPrivate);
        Assert.False(words.IsPublic);
        Assert.Null(type.GetMethod("PrintMembers", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

        var contract = type.GetProperty("EqualityContract", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

        Assert.True(contract is null || contract.GetMethod is { IsPublic: false });
        Assert.DoesNotContain("EqualityContract", PublicSurface());

        var copy = Assert.Single(
            type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance),
            constructor => constructor.GetParameters().Length == 1);

        Assert.True(copy.IsPrivate);
        Assert.Equal(type, copy.GetParameters()[0].ParameterType);
        Assert.True(copy.IsDefined(typeof(CompilerGeneratedAttribute), false));

        var clone = type.GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;

        Assert.Equal(type, clone.ReturnType);
        Assert.Empty(clone.GetParameters());

        Assert.Equal(
            ["Equals", "GetHashCode", "ToString"],
            [.. type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => !method.IsSpecialName && !method.Name.StartsWith("op_", StringComparison.Ordinal) && method.Name is not ("<Clone>$" or "Deconstruct"))
                .Select(method => method.Name)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)]);
        Assert.Equal(
            ["op_Equality", "op_Inequality"],
            [.. type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(method => method.Name)
                .Order(StringComparer.Ordinal)]);
    }

    /// <summary>The public members the record declares itself: the four parts, their readers and their writers, the
    /// constructor, the copy, the four parts fallen apart, the two equality questions, the words, the number and the
    /// two operators - in the order the names are said.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(ActivityLine)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The four parts, as the public properties they are.</summary>
    private static List<string> PropertyNames() =>
        [.. typeof(ActivityLine)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)];
}
