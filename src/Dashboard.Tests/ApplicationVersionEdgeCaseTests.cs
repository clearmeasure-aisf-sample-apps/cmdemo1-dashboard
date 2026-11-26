using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="ApplicationVersion" /> is the version the application view shows of a deployable and the one bit of
/// where it came from: the version's own word and <see cref="ApplicationVersion.Runs" />, true when a node reported
/// it and false when it is the version the environment pins. Two members, no logic of its own, nothing to refuse
/// and nothing to ask.
/// <para>
/// What is pinned where it belongs is walked past here on purpose: <see cref="ApplicationViewTests" /> pins
/// <see cref="ApplicationText.Version(IEnumerable{ValueTuple{string, string}}, string)" /> - which node's version the
/// view shows, the primary's before the standby's, what is trimmed on the way in, and that nothing is known when
/// neither the nodes nor the environment has a version - and pins the same decision through the boxes of a diagram
/// and their environment; <see cref="RuntimeTextHelperTests" /> and <see cref="ApplicationViewTests" /> pin the two
/// lines a version is said in, <c>Version 1.0.42, as it runs</c> and its tooltip, which are where the view reads
/// <see cref="ApplicationVersion.Runs" />; and what a version's word may name a file of is pinned on
/// <see cref="ApplicationText.IsVersion" /> and <see cref="ApplicationText.File" />. Those tests hand a version to
/// something else and ask that other thing what it says.
/// </para>
/// <para>
/// What follows is the version itself, at the ends those tests leave open: the constructor handing each argument to
/// the member of its name and holding the very word it was given, and the two members coming back out in the order
/// they went in; the word kept exactly as it is given - the empty word, the word of spaces, the word with its
/// whitespace about it, a word in another case, a word of the marks a version is spelled with and a word outside the
/// ASCII - and cut by nothing, so that a text at the forty letters the page stops asking a file at, one letter over
/// it and ten thousand of them are three whole words; the nothing that is no word at all, which the version holds as
/// its nothing rather than turning it into the empty word or refusing; the flag holding either of its two answers
/// and no third; the copy that turns one member while it keeps the other; the equality of the typed record, of the
/// boxed <see cref="object" /> and of <c>==</c> and <c>!=</c>, and the hash that follows them into a set and a
/// dictionary; the words the version says itself, which name the two members in the order they are named and say
/// both as they stand; and the public surface, so that the day a third member is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: the version holds two members, calls
/// nothing at all, and every value below is one the test brought. Of the types of the file the version stands in,
/// <see cref="RuntimeLoader" /> and the reading it does are the only ones that ask the repository anything, and they
/// are pinned with a stubbed answer in <see cref="ApplicationViewTests" />.
/// </para>
/// </summary>
public class ApplicationVersionEdgeCaseTests
{
    /// <summary>The version a node reported: what the view shows while any node of the deployable answers.</summary>
    private static ApplicationVersion Run => new("1.0.42", Runs: true);

    /// <summary>The version the environment pins, which no node has reported.</summary>
    private static ApplicationVersion Pinned => new("1.0.41", Runs: false);

    /// <summary>A version whose word is the empty word: nothing to say, and a version the record still holds.</summary>
    private static ApplicationVersion Blank => new(string.Empty, Runs: true);

    /// <summary>A version whose word is not there at all - the nothing a record with a member the caller must name
    /// answers with when the caller names it nothing.</summary>
    private static ApplicationVersion Nothing => new(null!, Runs: false);

    /// <summary>A word of the given number of <c>7</c>: the boundary the page stops asking a file for at forty
    /// letters - what <see cref="ApplicationText.IsVersion" /> keeps, and is pinned there - seen from the side of the
    /// version that holds the word, at the forty, one letter under it and one letter over it.</summary>
    private static string AtTheBoundary(int letters) => new('7', letters);

    // ------------------------------- the two members, by the names of their own properties

    /// <summary>The constructor's two arguments come back as the member of the same name - the word first and the
    /// flag last, the order the page says them in - whichever order the caller names them in. The word is held as
    /// the very string it was given, copied by nothing and turned by nothing, and the fixed versions the page is
    /// shown stand in their two members exactly as they were written.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToTheMemberOfItsName()
    {
        var text = Own("1.0.42");
        var version = new ApplicationVersion(text, Runs: true);

        Assert.Same(text, version.Text);
        Assert.Equal("1.0.42", version.Text);
        Assert.True(version.Runs);

        // Named in the other order, in the same two places.
        var named = new ApplicationVersion(Runs: false, Text: "1.0.41");

        Assert.Equal("1.0.41", named.Text);
        Assert.False(named.Runs);
        Assert.Equal(Pinned, named);

        // The versions the page is really shown, each in its two members.
        Assert.Equal(("1.0.42", true), (Run.Text, Run.Runs));
        Assert.Equal(("1.0.41", false), (Pinned.Text, Pinned.Runs));
        Assert.Equal(("3.1.0", false), (new ApplicationVersion("3.1.0", Runs: false).Text, new ApplicationVersion("3.1.0", Runs: false).Runs));
        Assert.Equal(("2.4.21+0a1b2c3", true), (new ApplicationVersion("2.4.21+0a1b2c3", Runs: true).Text, new ApplicationVersion("2.4.21+0a1b2c3", Runs: true).Runs));
        Assert.Equal(("0.0.0-local", false), (new ApplicationVersion("0.0.0-local", Runs: false).Text, new ApplicationVersion("0.0.0-local", Runs: false).Runs));
        Assert.Equal(("1", true), (new ApplicationVersion("1", Runs: true).Text, new ApplicationVersion("1", Runs: true).Runs));

        // Two calls of the same word are two references to the same letters, and the record keeps the one it was
        // given: only a word written on its own can tell the holding from a copying.
        Assert.NotSame(version.Text, new ApplicationVersion("1.0.42", Runs: true).Text);
        Assert.Equal(version, new ApplicationVersion("1.0.42", Runs: true));
    }

    /// <summary>Every word is kept in its exact letters: the empty word, the word of spaces, the word with its
    /// whitespace about it, a word in another case, a word of digits only, the marks a version is spelled with and a
    /// letter outside the ASCII. Nothing here trims, nothing here changes a case, nothing here cuts a word short and
    /// nothing here turns the empty word into the nothing - the version says back what it was handed, in the very
    /// string it was handed, with the flag it was handed beside it.</summary>
    [Theory]
    [InlineData("", true)]
    [InlineData(" ", false)]
    [InlineData("   ", true)]
    [InlineData("  1.0.42  ", false)]
    [InlineData("\t1.0.42", true)]
    [InlineData("1.0.42", true)]
    [InlineData("1.0.42", false)]
    [InlineData("1.0.42-beta.1", true)]
    [InlineData("1.0.42+build_7", false)]
    [InlineData("1.0.42-x", true)]
    [InlineData("Ui.Server", false)]
    [InlineData("uAt", true)]
    [InlineData("0", false)]
    [InlineData("00.00.00", true)]
    [InlineData("1.0.42-世界", false)]
    [InlineData("1.0.42 – rc1", true)]
    [InlineData(null, true)]
    [InlineData(null, false)]
    public void TheVersionTextIsKeptExactlyAsItIsGiven(string? text, bool runs)
    {
        var version = new ApplicationVersion(text!, runs);

        Assert.Equal(text, version.Text);
        Assert.Equal(runs, version.Runs);
        Assert.Equal(text?.Length ?? 0, version.Text?.Length ?? 0);

        // The very string it was given, held as it stands; the nothing held as the nothing it is.
        if (text is null)
        {
            Assert.Null(version.Text);
        }
        else
        {
            Assert.Same(text, version.Text);
        }

        // The nothing and the empty word are two different versions, and neither is a word of spaces.
        Assert.NotEqual(version, new ApplicationVersion(text is null ? string.Empty : string.Concat(text, " "), runs));
    }

    /// <summary>A word of any length is held whole and said whole. The page stops asking a file for a version longer
    /// than forty letters - that is <see cref="ApplicationText.IsVersion" />, and it is pinned there - but the
    /// version itself cuts nothing: the forty, the forty-one and the ten thousand are three whole words, one letter
    /// fewer is another version told apart in every way of asking, and the same letters written again in another
    /// string are the same version.</summary>
    [Fact]
    public void AVersionTextOfAnyLengthIsHeldWhole()
    {
        var forty = Own(AtTheBoundary(40));
        var fortyOne = Own(AtTheBoundary(41));
        var tenThousand = Own(new string('9', 10_000));

        Assert.Same(forty, new ApplicationVersion(forty, Runs: true).Text);
        Assert.Same(fortyOne, new ApplicationVersion(fortyOne, Runs: true).Text);
        Assert.Same(tenThousand, new ApplicationVersion(tenThousand, Runs: false).Text);
        Assert.Equal(40, new ApplicationVersion(forty, Runs: true).Text.Length);
        Assert.Equal(41, new ApplicationVersion(fortyOne, Runs: true).Text.Length);
        Assert.Equal(10_000, new ApplicationVersion(tenThousand, Runs: false).Text.Length);

        // One letter fewer at either end of the boundary is another version: held apart, compared apart and said
        // apart, with no prefix of the longer word standing in for the whole of it.
        var shorter = AtTheBoundary(39);
        var longer = AtTheBoundary(42);
        var atForty = new ApplicationVersion(forty, Runs: true);

        Assert.NotEqual(atForty, new ApplicationVersion(shorter, Runs: true));
        Assert.NotEqual(atForty, new ApplicationVersion(longer, Runs: true));
        Assert.True(atForty != new ApplicationVersion(shorter, Runs: true));
        Assert.Equal(
            3,
            new HashSet<ApplicationVersion>
            {
                atForty,
                new ApplicationVersion(shorter, Runs: true),
                new ApplicationVersion(longer, Runs: true),
            }.Count);
        Assert.DoesNotContain(Words(atForty), Words(new ApplicationVersion(shorter, Runs: true)), StringComparison.Ordinal);

        // The same letters gathered into another string: another reference, and one version.
        var writtenAgain = new ApplicationVersion(Own(AtTheBoundary(40)), Runs: true);

        Assert.NotSame(atForty.Text, writtenAgain.Text);
        Assert.Equal(atForty, writtenAgain);
        Assert.Equal(atForty.GetHashCode(), writtenAgain.GetHashCode());
        Assert.Equal(Words(atForty), Words(writtenAgain));
        Assert.Single(new HashSet<ApplicationVersion> { atForty, writtenAgain });
    }

    /// <summary>The record refuses nothing it is handed, and the nothing a version is handed for its word stands as
    /// that: the version is there, its flag is there, its word is not, and it is not the empty word either - which in
    /// turn is not a word of spaces. The nothing is hashed, compared, copied and said without a word of complaint.</summary>
    [Fact]
    public void AVersionWhoseTextIsNotThereIsAVersionStill()
    {
        var noText = Nothing;

        Assert.NotNull(noText);
        Assert.Null(noText.Text);
        Assert.False(noText.Runs);

        // The nothing, the empty word and the word of spaces are three versions.
        Assert.NotEqual(noText, Blank);
        Assert.NotEqual(noText, new ApplicationVersion(" ", Runs: false));
        Assert.NotEqual(Blank, new ApplicationVersion(" ", Runs: false));
        Assert.Equal(
            4,
            new HashSet<ApplicationVersion>
            {
                noText,
                Blank,
                new ApplicationVersion(" ", Runs: false),
                new ApplicationVersion("  ", Runs: false),
            }.Count);
        Assert.True(noText != Blank);
        Assert.False(noText == Blank);

        // Two versions whose words are both nothing are one version, in their flag as in their absence.
        Assert.Equal(noText, new ApplicationVersion(null!, Runs: false));
        Assert.True(noText == new ApplicationVersion(null!, Runs: false));
        Assert.Equal(noText.GetHashCode(), new ApplicationVersion(null!, Runs: false).GetHashCode());
        Assert.Equal(noText, new ApplicationVersion(null!, Runs: false) with { Runs = false });
        Assert.False(noText.Equals(new ApplicationVersion(null!, Runs: true)));

        // And a version with a word is another version than the same one without it.
        Assert.NotEqual(new ApplicationVersion("1.0.42", Runs: false), noText with { Runs = false });
        Assert.Equal(new ApplicationVersion("1.0.42", Runs: false), noText with { Text = "1.0.42" });
        Assert.Equal(string.Empty, (noText with { Text = string.Empty }).Text);
    }

    /// <summary>The flag holds either of its two answers and no third: the version a node reported and the version the
    /// environment pins are the same word and two versions, apart in the equality, in a set and in the words the
    /// version says itself - and the flag comes back out of the copy and the deconstruction as the flag it went in
    /// as, whatever the word beside it is.</summary>
    [Fact]
    public void TheFlagHoldsEitherOfItsTwoAnswersAndNoThird()
    {
        var runs = Run;
        var pinned = new ApplicationVersion("1.0.42", Runs: false);

        Assert.True(runs.Runs);
        Assert.False(pinned.Runs);
        Assert.Equal("1.0.42", runs.Text);
        Assert.Equal("1.0.42", pinned.Text);
        Assert.NotEqual(runs, pinned);
        Assert.True(runs != pinned);
        Assert.False(runs.Equals(pinned));
        Assert.Equal(2, new HashSet<ApplicationVersion> { runs, pinned }.Count);
        Assert.Equal("ApplicationVersion { Text = 1.0.42, Runs = True }", Words(runs));
        Assert.Equal("ApplicationVersion { Text = 1.0.42, Runs = False }", Words(pinned));

        // The two answers the flag has are the two answers: turned in a copy, they are the two versions, and turning
        // it twice is the version it started from.
        Assert.Equal(pinned, runs with { Runs = false });
        Assert.Equal(runs, pinned with { Runs = true });
        Assert.Equal(runs, runs with { Runs = true });
        Assert.Equal(runs, runs with { Runs = false } with { Runs = true });

        // The flag rides along with the word it stands beside, in either of the two versions and in the nothing of a
        // word that is not there.
        var (text, flag) = pinned;

        Assert.Equal("1.0.42", text);
        Assert.False(flag);
        Assert.Null(Nothing.Text);
        Assert.False(Nothing.Runs);
        Assert.Equal(string.Empty, Blank.Text);
        Assert.True(Blank.Runs);
    }

    // ------------------------- the two members coming out again, and the copy that turns one

    /// <summary>The two members come back out in the order they went in - the word first and the flag last - whether
    /// they are taken at once in a deconstruction or asked for one after the other, and what came out goes back in
    /// the same way: the version the two members fall apart into is the version they were. The nothing, the empty
    /// word and the word of spaces fall apart as they stand, turned to nothing by nobody.</summary>
    [Fact]
    public void TheVersionFallsApartIntoItsTwoMembersInTheOrderTheyWentIn()
    {
        var (text, runs) = Run;

        Assert.Equal("1.0.42", text);
        Assert.True(runs);
        Assert.Equal(Run, new ApplicationVersion(text, runs));
        Assert.Equal(Run, new ApplicationVersion(Text: text, Runs: runs));

        Run.Deconstruct(out var again, out var againRuns);

        Assert.Same(Run.Text, again);
        Assert.Equal(text, again);
        Assert.Equal(runs, againRuns);

        var (pinnedText, pinnedRuns) = Pinned;

        Assert.Equal("1.0.41", pinnedText);
        Assert.False(pinnedRuns);
        Assert.NotEqual(text, pinnedText);
        Assert.NotEqual(runs, pinnedRuns);

        // The two places are two places: the word never lands on the flag's place, and a word that is not there
        // comes out as the nothing and not as the empty word.
        var (noText, noRuns) = Nothing;

        Assert.Null(noText);
        Assert.False(noRuns);
        Assert.Equal(Nothing, new ApplicationVersion(noText!, noRuns));
        Assert.NotEqual(new ApplicationVersion(noText!, noRuns), new ApplicationVersion(string.Empty, noRuns));

        var (blank, blankRuns) = new ApplicationVersion("   ", Runs: true);

        Assert.Equal("   ", blank);
        Assert.True(blankRuns);
        Assert.NotEqual(blank, string.Empty);
        Assert.NotEqual(new ApplicationVersion(blank, blankRuns), new ApplicationVersion(string.Empty, blankRuns));
    }

    /// <summary>Either member may be turned as the copy is made, and the copy that names nothing is the version it
    /// started from in another reference: the member that is not named keeps the very word and the very flag it
    /// stood at, the original is left where it stood, and a word turned to nothing is the nothing and not the empty
    /// word. These are the record's own way of turning a member - the page hands out versions it makes outright.</summary>
    [Fact]
    public void ACopyTurnsTheMemberItNamesAndKeepsTheOther()
    {
        var original = Run;

        var unchanged = original with { };

        Assert.Equal(original, unchanged);
        Assert.NotSame(original, unchanged);
        Assert.Equal(Words(original), Words(unchanged));
        Assert.Equal(original.GetHashCode(), unchanged.GetHashCode());

        var wordTurned = original with { Text = "1.0.43" };

        Assert.Equal("1.0.43", wordTurned.Text);
        Assert.True(wordTurned.Runs);
        Assert.Equal("1.0.42", original.Text);
        Assert.True(original.Runs);
        Assert.NotEqual(original, wordTurned);

        var flagTurned = original with { Runs = false };

        Assert.Equal(new ApplicationVersion("1.0.42", Runs: false), flagTurned);
        Assert.Equal("1.0.42", flagTurned.Text);
        Assert.Equal(Run, flagTurned with { Runs = true });
        Assert.Equal(original.Text, flagTurned.Text);

        var bothTurned = original with { Text = "2.0.0", Runs = false };

        Assert.Equal(new ApplicationVersion("2.0.0", Runs: false), bothTurned);
        Assert.Equal(new ApplicationVersion("1.0.42", Runs: true), original);

        // A word turned to nothing is the nothing, and the nothing is not the empty word.
        var toNothing = original with { Text = null! };

        Assert.Null(toNothing.Text);
        Assert.True(toNothing.Runs);
        Assert.NotEqual(toNothing, original with { Text = string.Empty });
        Assert.Equal(new ApplicationVersion(null!, Runs: false), toNothing with { Runs = false });

        // A word turned to the word it already holds is the same version in another reference - and the word of a
        // literal is the one word those letters are written with.
        var sameWord = original with { Text = "1.0.42" };

        Assert.Equal(original, sameWord);
        Assert.NotSame(original, sameWord);
        Assert.Same(original.Text, sameWord.Text);
    }

    // -------------------------------------------------------------- the equality and the hash

    /// <summary>A version is one with another while its two members are one, whichever of the four ways of asking is
    /// used: the record's own <c>Equals</c>, the one of <see cref="object" />, <c>==</c> and <c>!=</c>. The hash
    /// follows the equality, boxed or unboxed and from one call to the next, and the letters are compared rather than
    /// the references - so the version a test writes out is the version another test writes out again, one entry of a
    /// set and one key of a dictionary.</summary>
    [Fact]
    public void TwoVersionsAreOneWhileTheirTwoMembersAre()
    {
        var one = Run;
        var two = new ApplicationVersion(Own("1.0.42"), Runs: true);

        Assert.True(one.Equals(two));
        Assert.True(one.Equals(one));
        Assert.True(one.Equals((object)two));
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.True(one == two with { });
        Assert.False(one != two with { Runs = true });
        Assert.Equal(one, two);
        Assert.NotSame(one, two);
        Assert.NotSame(one.Text, two.Text);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Single(new HashSet<ApplicationVersion> { one, two });

        // The same letters, spelled again: another reference, and one key.
        var shown = new Dictionary<ApplicationVersion, string> { [one] = "as it runs" };

        Assert.Equal("as it runs", shown[two]);
        Assert.Equal("as it runs", shown[one with { }]);
        Assert.Single(shown);

        var otherFlag = two with { Runs = false };

        Assert.False(shown.ContainsKey(otherFlag));

        // The empty word and the nothing, spelled twice each, are two versions and no more.
        Assert.Equal(Blank, new ApplicationVersion(string.Empty, Runs: true));
        Assert.Single(new HashSet<ApplicationVersion> { Blank, new ApplicationVersion(Own(string.Empty), Runs: true) });
        Assert.Single(new HashSet<ApplicationVersion> { Nothing, new ApplicationVersion(null!, Runs: false) });
    }

    /// <summary>A difference in either member is another version: a letter turned in the word, a letter added at
    /// either end, a letter dropped, the whitespace at either end, the case, the word exchanged for another of the
    /// same length - and the flag turned while the word stands where it did. The comparison is of the letters one
    /// after another, so neither a trimmed word nor a word of another case is the word it stands beside, and the two
    /// members are two places: a version is told from another by either of them alone.</summary>
    [Fact]
    public void AVersionTurnedInEitherMemberIsAnotherVersion()
    {
        var one = Run;

        Assert.NotEqual(one, new ApplicationVersion("1.0.43", Runs: true));
        Assert.NotEqual(one, new ApplicationVersion("1.0.4", Runs: true));
        Assert.NotEqual(one, new ApplicationVersion("1.0.42x", Runs: true));
        Assert.NotEqual(one, new ApplicationVersion("x1.0.42", Runs: true));
        Assert.NotEqual(one, new ApplicationVersion(" 1.0.42", Runs: true));
        Assert.NotEqual(one, new ApplicationVersion("1.0.42 ", Runs: true));
        Assert.NotEqual(one, new ApplicationVersion("\t1.0.42", Runs: true));
        Assert.NotEqual(one, new ApplicationVersion("1.042", Runs: true));
        Assert.NotEqual(one, new ApplicationVersion("1-0-42", Runs: true));
        Assert.NotEqual(one, new ApplicationVersion("1.0.42+", Runs: true));
        Assert.NotEqual(one, new ApplicationVersion("1.0.42-", Runs: true));
        Assert.NotEqual(one, new ApplicationVersion("Ui.Server", Runs: true));
        Assert.NotEqual(one, new ApplicationVersion(string.Empty, Runs: true));
        Assert.NotEqual(one, new ApplicationVersion(null!, Runs: true));
        Assert.NotEqual(one, new ApplicationVersion("1.0.42", Runs: false));
        Assert.NotEqual(one, new ApplicationVersion("1.0.41", Runs: true));

        Assert.True(one != new ApplicationVersion("1.0.43", Runs: true));
        Assert.False(one == new ApplicationVersion("1.0.42", Runs: false));
        Assert.False(one.Equals((object)new ApplicationVersion("1.0.42", Runs: false)));
        Assert.False(one.Equals(one with { Text = null! }));
        Assert.False(one.Equals(one with { Runs = false }));

        // Six settings of the two members, and six versions apart.
        Assert.Equal(
            6,
            new HashSet<ApplicationVersion>
            {
                one,
                one with { Text = "1.0.41" },
                one with { Runs = false },
                one with { Text = string.Empty },
                one with { Text = null! },
                one with { Text = "1.0.42 " },
            }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two versions that are no
    /// version at all are one, a version and no version are two - the equality and the inequality each saying it,
    /// from either side - and the version asked about itself says no to the nothing, to the word it was made of, to
    /// the package of the same letters and to the anonymous thing that happens to carry its two members.</summary>
    [Fact]
    public void NoVersionIsOneWithNoVersionAndNotWithAnyOtherThing()
    {
        ApplicationVersion? nothing = null;
        ApplicationVersion? alsoNothing = null;
        var there = Run;

        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == nothing);
        Assert.True(there != nothing);
        Assert.True(there!.Equals(there));
        Assert.Null(default(ApplicationVersion));

        Assert.False(Run.Equals(null));
        Assert.False(Run.Equals((ApplicationVersion?)null));
        Assert.False(Run.Equals((object?)null));
        Assert.False(Run.Equals("1.0.42"));
        Assert.False(Run.Equals(true));
        Assert.False(Run.Equals(new ApplicationPackage("1.0.42", null)));
        Assert.False(Run.Equals(new ApplicationAssembly("1.0.42", Entry: true, [], [])));
        Assert.False(Run.Equals(new HealthWindow(24, 24)));
        Assert.False(Run.Equals(new { Text = "1.0.42", Runs = true }));
        Assert.False(Run.Equals(Pinned));
        Assert.False(Run.Equals(Blank));
        Assert.False(Run.Equals(Nothing));
    }

    /// <summary>The hash follows the equality at the ends of what a version holds: two versions that are one hash
    /// one, however they were written, boxed or unboxed, twice in a row and whichever reader asks - and the nothing
    /// of a word that is not there is hashed as well as any word, one with another nothing and apart from the empty
    /// word. What a version hashes to is no promise about which versions share it, so it is followed here rather than
    /// written out: the numbers a run gives are not the numbers of the next.</summary>
    [Fact]
    public void TheHashFollowsTheEqualityAndNeitherTheNothingTextNorTheReaderTurnsIt()
    {
        var one = Run;
        var two = new ApplicationVersion(Own("1.0.42"), Runs: true);

        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), (one with { }).GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(two.GetHashCode(), two.GetHashCode());
        Assert.Equal(Blank.GetHashCode(), new ApplicationVersion(Own(string.Empty), Runs: true).GetHashCode());
        Assert.Equal(Nothing.GetHashCode(), new ApplicationVersion(null!, Runs: false).GetHashCode());
        Assert.Equal(Nothing.GetHashCode(), Nothing.GetHashCode());

        // The nothing hashes without a complaint, and with the word it is not: two nothings are one entry, and the
        // empty word beside them is another.
        Assert.Single(new HashSet<ApplicationVersion> { Nothing, new ApplicationVersion(null!, Runs: false) });
        Assert.Equal(2, new HashSet<ApplicationVersion> { Nothing, Blank }.Count);
        Assert.Equal(2, new HashSet<ApplicationVersion> { one, one with { Runs = false } }.Count);

        // The reader of German hashes the versions the invariant reader hashes, and keys a dictionary the same way.
        ViewerCulture.German(() =>
        {
            Assert.Equal(one.GetHashCode(), two.GetHashCode());
            Assert.Equal(
                "as it runs",
                new Dictionary<ApplicationVersion, string> { [one] = "as it runs" }[two]);
        });
    }

    // ---------------------------------------------------------- the words the version says itself

    /// <summary>The words a version says its two members in: its own name at the front, the word and the flag as
    /// they are named, each said as it stands and none of them quoted - the nothing of a word that is not there and
    /// the empty word both leaving the same empty place between the mark and the comma, the whitespace of a word
    /// standing in the middle of it, the flag in the record's own letters of <c>True</c> and <c>False</c>, and a word
    /// of any length said whole. These are the record's words and not the page's: the page says a version
    /// <c>Version 1.0.42, as it runs</c>, and no line of it comes from here.</summary>
    [Fact]
    public void TheVersionSaysItsTwoMembersInTheOrderTheyAreNamed()
    {
        Assert.Equal("ApplicationVersion { Text = 1.0.42, Runs = True }", Words(Run));
        Assert.Equal("ApplicationVersion { Text = 1.0.41, Runs = False }", Words(Pinned));
        Assert.Equal("ApplicationVersion { Text = , Runs = True }", Words(Blank));
        Assert.Equal("ApplicationVersion { Text = , Runs = False }", Words(Nothing));
        Assert.Equal("ApplicationVersion { Text =   1.0.42  , Runs = True }", Words(new ApplicationVersion("  1.0.42  ", Runs: true)));
        Assert.Equal("ApplicationVersion { Text = \t1.0.42, Runs = False }", Words(new ApplicationVersion("\t1.0.42", Runs: false)));
        Assert.Equal("ApplicationVersion { Text = 1.0.42+build_7, Runs = True }", Words(new ApplicationVersion("1.0.42+build_7", Runs: true)));
        Assert.Equal("ApplicationVersion { Text = 0, Runs = False }", Words(new ApplicationVersion("0", Runs: false)));
        Assert.Equal("ApplicationVersion { Text = 1.0.42-世界, Runs = True }", Words(new ApplicationVersion("1.0.42-世界", Runs: true)));
        Assert.Equal($"ApplicationVersion {{ Text = {AtTheBoundary(41)}, Runs = False }}", Words(new ApplicationVersion(AtTheBoundary(41), Runs: false)));

        // The words say the two members and nothing else: no line of the page's, no quoting of the word, and the
        // two flags said apart.
        Assert.DoesNotContain("as it runs", Words(Run), StringComparison.Ordinal);
        Assert.DoesNotContain("as pinned", Words(Pinned), StringComparison.Ordinal);
        Assert.DoesNotContain("'", Words(Run), StringComparison.Ordinal);
        Assert.StartsWith("ApplicationVersion { Text = ", Words(Run), StringComparison.Ordinal);
        Assert.NotEqual(Words(Run), Words(Run with { Runs = false }));
        // The nothing of a word and the empty word leave the same empty place: only the equality tells them apart.
        Assert.Equal(Words(Blank), Words(new ApplicationVersion(null!, Runs: true)));
        Assert.NotEqual(Words(Blank), Words(new ApplicationVersion(" ", Runs: true)));
    }

    /// <summary>The two members are letters and a flag, so no reader's culture turns them, the words the version
    /// says, the equality that follows the letters or the hash that follows the equality: a version a German reader
    /// makes is the version an invariant reader makes, said in the invariant record's letters with the same
    /// <c>True</c> and <c>False</c> the English record writes.</summary>
    [Fact]
    public void TheWordsAreTheSameWhateverTheReaderSpeaks()
    {
        Assert.Equal("ApplicationVersion { Text = 1.0.42, Runs = True }", Words(Run));

        ViewerCulture.German(() =>
        {
            Assert.Equal("ApplicationVersion { Text = 1.0.42, Runs = True }", Run.ToString());
            Assert.Equal("ApplicationVersion { Text = 1.0.41, Runs = False }", Pinned.ToString());
            Assert.Equal("ApplicationVersion { Text = , Runs = False }", Nothing.ToString());
            var germanFlagTurned = Run with { Runs = false };

            Assert.Equal("ApplicationVersion { Text = 1.0.42, Runs = False }", germanFlagTurned.ToString());

            // A version made under the German reader is the version made under any reader.
            var german = new ApplicationVersion(Own("1.0.42"), Runs: true);

            Assert.Equal(Run, german);
            Assert.True(Run == german);
            Assert.Equal(Run.GetHashCode(), german.GetHashCode());
            Assert.Equal(Words(Run), german.ToString());
        });

        Assert.Equal(Words(Run), Words(new ApplicationVersion(Own("1.0.42"), Runs: true)));
    }

    // -------------------------------------------------------------- the version's own shape

    /// <summary>The whole public surface of a version, as it stands: the two members in the order the constructor
    /// names them - the word and the flag - both of them written once, at the making and never after; the two
    /// arguments the constructor takes, both of them required; and the members every record is asked, falling apart,
    /// comparing, hashing and saying itself, and nothing else to call. The two lines of the view, the file it asks
    /// for and the boxes that lead into it are the decisions of the classes they belong to, pinned there: a version
    /// decides nothing. A third member added to the record has to be written here as well, which is the point: the
    /// page cannot be shown a member nobody decided what it means.</summary>
    [Fact]
    public void TheVersionHasTwoMembersAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Text", "Runs"],
            [.. typeof(ApplicationVersion)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["Deconstruct", "Equals", "GetHashCode", "Runs", "Text", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());

        var properties = typeof(ApplicationVersion)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(string), properties["Text"].PropertyType);
        Assert.Equal(typeof(bool), properties["Runs"].PropertyType);
        Assert.All(properties.Values, property => Assert.True(property.CanRead));
        Assert.All(properties.Values, property => Assert.True(property.CanWrite));

        // Both members are written at the making and closed: the version the page was shown is the version the page
        // keeps showing until another version is made.
        Assert.All(properties.Values, property => Assert.True(IsWrittenOnlyAtTheMaking(property), $"{property.Name} may be turned after the version was made"));

        // Two arguments, in the members' own order and types, and neither of them optional.
        var parameters = typeof(ApplicationVersion).GetConstructors().Single().GetParameters();

        Assert.Equal(["Text", "Runs"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal([typeof(string), typeof(bool)], [.. parameters.Select(parameter => parameter.ParameterType)]);
        Assert.All(parameters, parameter => Assert.False(parameter.IsOptional));
        Assert.All(parameters, parameter => Assert.False(parameter.HasDefaultValue));

        // The two places come out as the two places they went in, in the members' own types.
        var deconstruct = typeof(ApplicationVersion).GetMethod("Deconstruct")!;

        Assert.True(deconstruct.IsPublic);
        Assert.Equal(
            [typeof(string).MakeByRefType(), typeof(bool).MakeByRefType()],
            [.. deconstruct.GetParameters().Select(parameter => parameter.ParameterType)]);

        // The two ways of asking whether two versions are one: the record's own, and the one every object is asked.
        Assert.Equal(
            ["ApplicationVersion", "Object"],
            [.. typeof(ApplicationVersion)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => method.Name.Equals("Equals", StringComparison.Ordinal))
                .Select(method => method.GetParameters()[0].ParameterType.Name)
                .Order(StringComparer.Ordinal)]);

        // A version is a reference that says what it is equal to, and no more of it may be written about.
        Assert.True(typeof(ApplicationVersion).IsSealed);
        Assert.False(typeof(ApplicationVersion).IsValueType);
        Assert.Equal(typeof(object), typeof(ApplicationVersion).BaseType);
        Assert.Equal([typeof(IEquatable<ApplicationVersion>)], [.. typeof(ApplicationVersion).GetInterfaces()]);
    }

    // ---------------------------------------------------------------------------------- the machinery

    /// <summary>The public members the version declares itself: its two properties and the methods that are not the
    /// get and the set of one - the two operators kept although the compiler names them specially, since they are
    /// the <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the tests above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(ApplicationVersion)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>Whether a member may be written only while the version is being made: the record's init-only
    /// accessor, which the compiler marks with the modreq that a <c>with</c> carries and no assignment does.</summary>
    private static bool IsWrittenOnlyAtTheMaking(PropertyInfo property) =>
        property.SetMethod is { } setter
        && setter.ReturnParameter.GetRequiredCustomModifiers().Any(modifier =>
            modifier.FullName is { } name && name.Equals("System.Runtime.CompilerServices.IsExternalInit", StringComparison.Ordinal));

    /// <summary>The words a version says its two members in, taken in the invariant culture whatever the reader of
    /// this test happens to be: the point below is the words, and the reader's own culture is pinned apart.</summary>
    private static string Words(ApplicationVersion version) => ViewerCulture.Invariant(() => version.ToString()!);

    /// <summary>The letters of a word gathered into a string of its own, which no other word in the test shares a
    /// reference with: what the version does with a word it is given - keeping the one and not another that spells
    /// the same - is only seen when the two are apart.</summary>
    private static string Own(string word) => new(word.ToCharArray());
}
