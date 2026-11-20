using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="WorkScopeLine" />, the one line the page puts above
/// the board, declared in <c>src/Dashboard/Work/WorkItemsText.cs</c>: what the repository is called, where that name
/// leads, and the project number it stands on - with the one member the line works out for itself,
/// <see cref="WorkScopeLine.Title" />, the words a reader of that link hears.
/// <para>
/// What the other files already pin is left alone. That the line the shipped file is turned into is the label cut out
/// of the address, that address, and the number 679 is
/// <see cref="WorkItemsTests.TheSampleFileIsTheBoardTheViewShows" />, which builds one line by hand and asks the read
/// for the other; that a hand-built line is no <see cref="WorkItemsScope" /> is
/// <see cref="WorkItemsScopeEdgeCaseTests.NoScopeIsOneWithNoScopeAndNotWithAnyOtherThing" />; that the page hangs a
/// line's own title on the link and says the new tab again in words a reader hears after the label is
/// <see cref="WorkItemsViewTests.TheScopeLinkAnnouncesANewTabAfterItsVisibleLabel" />, which reads the markup; and
/// which three parts a line is made of, out of what a file wrote - the label cut of an address, the number told as a
/// word, a line left unsaid when neither is there - is <see cref="WorkItemsText.Scope" /> and the reading of
/// <see cref="WorkItemsScope" /> themselves, pinned in <see cref="WorkItemsScopeEdgeCaseTests" /> and in
/// <see cref="WorkItemsTests" />.
/// </para>
/// <para>
/// What is left, and is pinned here, is the line as it stands once it is made: the constructor handing each of its
/// three arguments to the property of its name and holding the very word and the very address it was given; the name
/// kept in its exact letters - nothing, the word of no characters, a word of whitespace, a word padded, a word in
/// another case, a word of a thousand of them - and the title made of those letters and of nobody's trimming; the
/// title standing only while both the name and the address stand, and falling to nothing when either is the absence
/// of one, which is what lets the page leave the words off a link it cannot make; the number of the project stood on
/// as the word it is, never read back as a number and never re-told for whoever is looking; the three parts coming
/// back out in the order they went in, and the copy that turns one part while it keeps the other two; the two lines
/// being one while their three parts are, with the hash, the set and the operators following that, and the nothing on
/// either side of each; an address compared as an address is compared and never opened; the words the line says its
/// parts and its worked-out title in, and those words telling nothing of which of the two nothings a left-out part
/// is; and the public surface, so that the day a fourth part is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a line holds two words and an address,
/// no moment and no count, and calls nothing at all - every value below is one the test brought. The address is held
/// as the <see cref="Uri" /> it was given and is never opened, and one address below is named on a host that resolves
/// nowhere, so nothing there is a board a test could reach even if a member tried: every answer comes out of the
/// parts, and none of them waits on anything.
/// </para>
/// </summary>
public class WorkScopeLineEdgeCaseTests
{
    /// <summary>The board of the shipped <c>workitems.json</c>, as the page writes its address.</summary>
    private const string Address = "https://github.com/clearmeasure-aisf-sample-apps/cmdemo1-system";

    /// <summary>The same board named rather than addressed, which is what a line's label is.</summary>
    private const string Name = "clearmeasure-aisf-sample-apps/cmdemo1-system";

    /// <summary>The project the board is numbered by, in the digits the page tells it in.</summary>
    private const string Number = "679";

    /// <summary>The words the page puts after a link that leaves the tab. Written out here rather than taken from
    /// <c>LinkText.GitHubSuffix</c>: a change to them is a change a reader hears, so it has to be said twice.</summary>
    private const string NewTab = " (opens in a new tab)";

    /// <summary>A line with every part said, so that a part landing on another part's place is seen rather than
    /// quietly matching a third of the same record.</summary>
    private static WorkScopeLine Full => new(Name, new Uri(Address), Number);

    /// <summary>A word of the test's own, so that a part held as the very instance it was given is proved by the
    /// holding and not by the interning of a literal.</summary>
    private static string Own(string word) => new(word.ToCharArray());

    /// <summary>The address of a line, or the absence of one when the word that names it is not there.</summary>
    private static Uri? UrlOf(string? address) => address is null ? null : new Uri(address);

    // --------------------------------------- the three parts, by the names of their own properties

    /// <summary>The constructor's three arguments, position by position, come back as the property of the same name:
    /// the name first, the address beside it, the number last - the order the page says them in. The word and the
    /// address are held as the very instances the line was built of, and a new line of the same three parts is
    /// another line, not the one it was built of.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var label = Own(Name);
        var address = new Uri(Address);
        var project = Own(Number);
        var line = new WorkScopeLine(label, address, project);

        Assert.Same(label, line.Label);
        Assert.Same(address, line.Url);
        Assert.Same(project, line.Project);
        Assert.Equal(Name, line.Label);
        Assert.Equal(Address, line.Url!.ToString());
        Assert.Equal(Number, line.Project);

        // A board named and never addressed, and a board addressed and never numbered: each part stands on its own,
        // and the line that leaves a part out is still a line.
        var named = new WorkScopeLine("cmdemo1-workorders", null, null);
        var numbered = new WorkScopeLine(null, address, "679");

        Assert.Equal("cmdemo1-workorders", named.Label);
        Assert.Null(named.Url);
        Assert.Null(named.Project);
        Assert.Null(numbered.Label);
        Assert.Same(address, numbered.Url);
        Assert.Equal("679", numbered.Project);
        Assert.NotEqual(line, named);
        Assert.NotEqual(line, numbered);

        var again = new WorkScopeLine(label, new Uri(Address), project);

        Assert.Equal(line, again);
        Assert.NotSame(line, again);
    }

    /// <summary>Either word is kept in its exact letters: the word of no characters, the word that is only
    /// whitespace, a word padded at both ends, a word in another case, a word that is the page's own dash or its own
    /// <c>n/a</c>, and a word that is no name at all all stand where they stand. Nothing here trims, lowers, folds or
    /// shortens - the cutting of a name out of an address is the read's, pinned where the reading happens - and the
    /// title a line is asked for is built of those same untouched letters.</summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   \t ")]
    [InlineData("cmdemo1-system")]
    [InlineData("CMDemo1-System")]
    [InlineData("  cmdemo1-system  ")]
    [InlineData("clearmeasure-aisf-sample-apps/cmdemo1-workorders")]
    [InlineData("not a name at all")]
    [InlineData("—")]
    [InlineData("n/a")]
    public void AWordIsHeldExactlyAsItIsGiven(string word)
    {
        var held = Own(word);
        var line = new WorkScopeLine(held, new Uri(Address), held);

        Assert.Same(held, line.Label);
        Assert.Same(held, line.Project);
        Assert.Equal(word, line.Label);
        Assert.Equal(word + NewTab, line.Title);
        Assert.StartsWith(word, line.Title, StringComparison.Ordinal);
        Assert.EndsWith(NewTab, line.Title, StringComparison.Ordinal);
        Assert.Equal(word.Length + NewTab.Length, line.Title!.Length);
    }

    /// <summary>A name of a thousand characters is kept whole and titled whole: the line does not shorten what the
    /// page was given to show, and no title is cut to a width. The empty word, the other way round, is a name of no
    /// characters at all, and a line built of it has a title of nothing but the page's own words after it.</summary>
    [Fact]
    public void ANameOfAWholeThousandLettersIsTitledWhole()
    {
        var longName = new string('c', 1000);
        var line = new WorkScopeLine(longName, new Uri(Address), Number);

        Assert.Same(longName, line.Label);
        Assert.Equal(1000, line.Label!.Length);
        Assert.Equal(1000 + NewTab.Length, line.Title!.Length);
        Assert.Contains(longName, line.Title, StringComparison.Ordinal);
        Assert.EndsWith(NewTab, line.Title, StringComparison.Ordinal);
        Assert.Contains(longName, line.ToString(), StringComparison.Ordinal);
        Assert.Equal(line, new WorkScopeLine(new string('c', 1000), new Uri(Address), Number));

        var nothingToName = new WorkScopeLine(string.Empty, new Uri(Address), Number);

        Assert.Equal(string.Empty, nothingToName.Label);
        Assert.Equal(" (opens in a new tab)", nothingToName.Title);
    }

    // ------------------------------------------------------------------ the title, and the two parts it stands on

    /// <summary>The title is the name and the page's words after it, and it is that only while both the name and the
    /// address stand: a name with nowhere to lead has no title, an address with no name to show has no title, and a
    /// line with neither has none - the record never settles what the page should show instead, it only refuses to
    /// promise a link that it cannot name. The words are the same however the address is written, since an address
    /// gives no words of its own to the title.</summary>
    [Theory]
    [InlineData(Name, Address, Name + " (opens in a new tab)")]
    [InlineData(Name, "https://github.com/o/r", Name + " (opens in a new tab)")]
    [InlineData(Name, "https://work-items.invalid.example/cmdemo1-system", Name + " (opens in a new tab)")]
    [InlineData(Name, null, null)]
    [InlineData(null, Address, null)]
    [InlineData(null, null, null)]
    [InlineData("", Address, " (opens in a new tab)")]
    [InlineData(" ", Address, "  (opens in a new tab)")]
    [InlineData("cmdemo1-system", Address, "cmdemo1-system (opens in a new tab)")]
    [InlineData("CMDemo1-SYSTEM", Address, "CMDemo1-SYSTEM (opens in a new tab)")]
    public void TheTitleStandsOnlyWhileBothTheNameAndTheAddressStand(string? label, string? address, string? title)
    {
        Assert.Equal(title, new WorkScopeLine(label, UrlOf(address), Number).Title);
    }

    /// <summary>A padded name keeps its spaces on both sides of the page's words, and the title says the name it was
    /// given and nothing else: no address, no markup, no project number and no dash for the parts that are there.</summary>
    [Fact]
    public void TheTitleSaysTheNameAndTheNewTabWordsAndNoPartOfAnAddress()
    {
        var line = new WorkScopeLine("  cmdemo1-system  ", new Uri(Address), Number);

        // The name's own two marks at either end, and the suffix's leading space between them and its words.
        Assert.Equal("  cmdemo1-system   (opens in a new tab)", line.Title);
        Assert.DoesNotContain("github.com", line.Title, StringComparison.Ordinal);
        Assert.DoesNotContain("679", line.Title, StringComparison.Ordinal);
        Assert.DoesNotContain("<a", line.Title, StringComparison.Ordinal);
        Assert.DoesNotContain("href", line.Title, StringComparison.Ordinal);
        Assert.DoesNotContain("\n", line.Title, StringComparison.Ordinal);

        // The words are the ones every other link of the page wears, and a line asked twice says them again.
        Assert.Equal(Name + LinkText.GitHubSuffix, Full.Title);
        Assert.EndsWith(" (opens in a new tab)", line.Title, StringComparison.Ordinal);
        Assert.Equal(Full.Title, new WorkScopeLine(Full.Label, Full.Url, Full.Project).Title);
    }

    /// <summary>The title is worked out of the two parts it stands on and is no part of its own: it is never stored,
    /// so it follows whichever part a copy turns - a name turned is titled anew, an address left out takes the title
    /// with it, and a title cannot be set to anything at all.</summary>
    [Fact]
    public void TheTitleFollowsWhicheverOfTheTwoPartsItStandsOnIsTurned()
    {
        var line = Full;

        Assert.Equal(Name + NewTab, line.Title);
        Assert.Equal("cmdemo1-workorders" + NewTab, (line with { Label = "cmdemo1-workorders" }).Title);
        Assert.Null((line with { Label = null }).Title);
        Assert.Null((line with { Url = null }).Title);
        Assert.Equal(line.Title, (line with { Project = null }).Title);
        Assert.Equal(line.Title, (line with { Url = new Uri("https://github.com/o/r") }).Title);
        Assert.Equal(line.Title, new WorkScopeLine(Name, new Uri("https://github.com/anything-else"), "0").Title);
    }

    // ------------------------------------------------------------- the number, held as the word it is

    /// <summary>The project number stands as the word it was given, and is never read back as a number: the digits of
    /// the page, the empty word, the word of whitespace, a number below zero, a number with a separator in it, a
    /// number too many digits to be one, and the page's own word for a thing not metered all hold their letters. The
    /// turning of a number into a word is the read's, pinned in <see cref="WorkItemsScopeEdgeCaseTests" />.</summary>
    [Theory]
    [InlineData("679")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("-5")]
    [InlineData("1,234")]
    [InlineData("2147483648")]
    [InlineData("٦٧٩")]
    [InlineData("679 ")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("n/a")]
    public void TheProjectStandsAsTheWordItWasGivenAndIsNeverReadAsANumber(string project)
    {
        var held = Own(project);
        var line = new WorkScopeLine(Name, new Uri(Address), held);

        Assert.Same(held, line.Project);
        Assert.Equal(project, line.Project);
        Assert.Contains($"Project = {project}, Title", line.ToString(), StringComparison.Ordinal);

        // The number has nothing to do with the title, which stands on the name and the address alone.
        Assert.Equal(Name + NewTab, line.Title);
    }

    /// <summary>The empty word and the absence of one are two projects: a line that says the board is numbered by
    /// nothing at all is another line than one that does not say what number the board is, and the record does not
    /// settle which of the two the page should leave out - that is the read's decision, pinned elsewhere. The line's
    /// own words, however, say the two alike, since an empty place and a place holding the empty word are both
    /// nothing to write.</summary>
    [Fact]
    public void AnEmptyWordIsAWordAndNotTheAbsenceOfOne()
    {
        var empty = new WorkScopeLine(string.Empty, null, string.Empty);
        var unsaid = new WorkScopeLine(null, null, null);

        Assert.Equal(string.Empty, empty.Label);
        Assert.Equal(string.Empty, empty.Project);
        Assert.Null(unsaid.Label);
        Assert.Null(unsaid.Project);
        Assert.Null(unsaid.Title);

        Assert.NotEqual(empty, unsaid);
        Assert.False(empty == unsaid);
        Assert.True(empty != unsaid);
        Assert.Equal(2, new HashSet<WorkScopeLine> { empty, unsaid }.Count);

        // The words are no measure of the line: these two are said alike, and the address is what tells them apart here.
        Assert.Equal(unsaid.ToString(), empty.ToString());
        Assert.NotEqual(Full.ToString(), unsaid.ToString());
    }

    // ------------------------------------------ the line falling apart, the copy, and its own declared shape

    /// <summary>The three parts come back out in the order they went in - the name, the address, the number - and the
    /// parts nobody gave come out as nothing rather than as the empty word and the address of nowhere they are not.
    /// What came out goes back in the same way: the line the three parts fall apart into is the line they were.</summary>
    [Fact]
    public void TheThreePartsComeOutAgainInTheOrderTheyWentIn()
    {
        var (label, url, project) = Full;

        Assert.Equal(Name, label);
        Assert.Equal(Address, url!.ToString());
        Assert.Equal(Number, project);
        Assert.Equal(Full, new WorkScopeLine(label, url, project));

        var (noLabel, noUrl, noProject) = new WorkScopeLine(null, null, null);

        Assert.Null(noLabel);
        Assert.Null(noUrl);
        Assert.Null(noProject);

        var (named, addressed, _) = new WorkScopeLine("cmdemo1-workorders", new Uri("https://github.com/o/r"), null);

        Assert.Equal("cmdemo1-workorders", named);
        Assert.Equal("https://github.com/o/r", addressed!.ToString());
        Assert.Equal(new WorkScopeLine("cmdemo1-workorders", addressed, "679"), new WorkScopeLine(named, addressed, project));
    }

    /// <summary>Each part may be turned as the line is made - the name to an address's owner, the address to the
    /// nothing and the number to the empty word - and stands where it is turned to, while the parts the turn does not
    /// name stay the very instances the line was built of. A line with its parts turned back on is another line and
    /// never the same reference.</summary>
    [Fact]
    public void APartTurnedAsTheLineIsMadeStandsWhereItIsTurnedTo()
    {
        var project = Own(Number);
        var line = new WorkScopeLine(Name, new Uri(Address), project);

        var renamed = new WorkScopeLine(Name, new Uri(Address), project) { Label = "cmdemo1-workorders" };

        Assert.Equal("cmdemo1-workorders", renamed.Label);
        Assert.Equal(Address, renamed.Url!.ToString());
        Assert.Same(project, renamed.Project);
        Assert.NotSame(line, renamed);
        Assert.NotEqual(line, renamed);

        var unaddressed = new WorkScopeLine(Name, new Uri(Address), project) { Url = null };

        Assert.Null(unaddressed.Url);
        Assert.Null(unaddressed.Title);
        Assert.Equal(Name, unaddressed.Label);
        Assert.Equal(new WorkScopeLine(Name, null, Number), unaddressed);

        var unsaid = new WorkScopeLine(null, null, null) { Label = Name, Url = new Uri(Address), Project = string.Empty };

        Assert.Equal(new WorkScopeLine(Name, new Uri(Address), string.Empty), unsaid);
        Assert.NotEqual(new WorkScopeLine(null, null, null), unsaid);
    }

    /// <summary>The copy that turns nothing is the same three parts in a new line, and the copy that turns one part
    /// keeps the other two as the very instances the original was built of: the original stands as it stood while the
    /// copy carries the turned part, and a copy that names all three parts as they stand is the line it started from
    /// without being it.</summary>
    [Fact]
    public void ACopyTurnsThePartItNamesAndKeepsTheOtherTwo()
    {
        var project = Own(Number);
        var line = new WorkScopeLine(Name, new Uri(Address), project);

        var unchanged = line with { };

        Assert.Equal(line, unchanged);
        Assert.NotSame(line, unchanged);
        Assert.Same(project, unchanged.Project);

        var otherBoard = line with { Label = "cmdemo1-workorders" };

        Assert.Equal(Name, line.Label);
        Assert.Equal("cmdemo1-workorders", otherBoard.Label);
        Assert.Equal(Address, otherBoard.Url!.ToString());
        Assert.Same(project, otherBoard.Project);
        Assert.NotSame(line, otherBoard);
        Assert.NotEqual(line, otherBoard);

        var noNumber = line with { Project = null };

        Assert.Null(noNumber.Project);
        Assert.Equal(Name, noNumber.Label);
        Assert.Equal(Address, noNumber.Url!.ToString());
        Assert.Equal(new WorkScopeLine(Name, new Uri(Address), null), noNumber);

        var repacked = line with { Label = line.Label, Url = line.Url, Project = line.Project };

        Assert.Equal(line, repacked);
        Assert.NotSame(line, repacked);
        Assert.Same(project, repacked.Project);
        Assert.Same(line.Url, repacked.Url);
    }

    /// <summary>The line's whole public surface, as it stands: the three parts named in the order the constructor
    /// names them, the title the line works out for itself, and the members every record is asked - falling apart,
    /// comparing, hashing and saying itself - and nothing else to call. The constructor is no method, so it is named
    /// by the test below rather than by that list; the title has no setter, because it is never a part of its own.
    /// Nothing any of them is asked takes a moment or hands one back, and nothing returns a task to wait on: that is
    /// why no test of a line needs a clock and none can reach the network, and a fourth part added to the record has
    /// to be written here as well, which is the point.</summary>
    [Fact]
    public void TheLineHasThreePartsAndATitleAndTheMembersEveryRecordIsAsked()
    {
        var properties = typeof(WorkScopeLine)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(["Label", "Url", "Project", "Title"], [.. properties.Keys]);

        Assert.Equal(
            ["Deconstruct", "Equals", "GetHashCode", "Label", "Project", "Title", "ToString", "Url", "op_Equality", "op_Inequality"],
            PublicSurface());

        Assert.Equal(typeof(string), properties["Label"].PropertyType);
        Assert.Equal(typeof(Uri), properties["Url"].PropertyType);
        Assert.Equal(typeof(string), properties["Project"].PropertyType);
        Assert.Equal(typeof(string), properties["Title"].PropertyType);
        Assert.All(
            new[] { properties["Label"], properties["Url"], properties["Project"] },
            property => Assert.True(property is { CanRead: true, CanWrite: true }, $"{property.Name} is not a part a caller can read and turn"));
        Assert.False(properties["Title"].CanWrite);

        Assert.True(typeof(WorkScopeLine).IsSealed);
        Assert.Contains(typeof(IEquatable<WorkScopeLine>), typeof(WorkScopeLine).GetInterfaces());
        Assert.Equal([typeof(string), typeof(Uri), typeof(string)], [.. Constructor().GetParameters().Select(parameter => parameter.ParameterType)]);
        Assert.Equal(["Label", "Url", "Project"], [.. Constructor().GetParameters().Select(parameter => parameter.Name!)]);

        // No part is a moment, nothing is asked for one, and no member hands back something to wait on.
        var members = typeof(WorkScopeLine)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .ToList();
        Assert.DoesNotContain(members, method => method.ReturnType == typeof(Task) || method.ReturnType.Name.StartsWith("ValueTask", StringComparison.Ordinal));
        Assert.DoesNotContain(members, method => method.GetParameters().Any(parameter => IsMoment(parameter.ParameterType) || parameter.ParameterType == typeof(CancellationToken)));
        Assert.DoesNotContain(properties.Values, property => IsMoment(property.PropertyType));
    }

    // -------------------------------------------------------------- the equality, the hash, and the two

    /// <summary>Two lines are one while their three parts are - each word as the letters it is, the address as the
    /// address it parses to, the nothing among them included - whichever of the four ways of asking is used: the
    /// record's own <c>Equals</c>, the one of <see cref="object" />, <c>==</c> and <c>!=</c>. The hash follows the
    /// equality, boxed or unboxed and from one call to the next, so the lines that are one are one entry of a set; and
    /// two words written apart are one word to the record, which compares letters and not references - as it compares
    /// two addresses written apart, and holds neither instance it was not given.</summary>
    [Fact]
    public void TwoLinesAreOneWhileTheirThreePartsAre()
    {
        var one = Full;
        var two = new WorkScopeLine(Name, new Uri(Address), Number);

        Assert.True(one.Equals(two));
        Assert.True(one.Equals(one));
        Assert.True(one.Equals((object)two));
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.Equal(one, two);
        Assert.NotSame(one, two);
        Assert.NotSame(one.Url, two.Url);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Equal(one.Title, two.Title);
        Assert.Single(new HashSet<WorkScopeLine> { one, two });

        var written = new WorkScopeLine(Own(Name), new Uri(Address), Own(Number));

        Assert.NotSame(one.Label, written.Label);
        Assert.NotSame(one.Project, written.Project);
        Assert.Equal(one, written);
        Assert.Equal(one.GetHashCode(), written.GetHashCode());
        Assert.Single(new HashSet<WorkScopeLine> { one, written });

        var nothing = new WorkScopeLine(null, null, null);
        var alsoNothing = new WorkScopeLine(null, null, null);

        Assert.Equal(nothing, alsoNothing);
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.Equal(nothing.GetHashCode(), alsoNothing.GetHashCode());
        Assert.Null(nothing.Title);
        Assert.Null(alsoNothing.Title);
    }

    /// <summary>A difference in either part is another line, and so is the same three parts told in another place: the
    /// name left out, emptied, renamed or told in other letters, the address left out or turned to another board, the
    /// number left out, emptied, or told as another word, and the name and the number swapped about. A set of the
    /// lines that differ in one part each holds them all apart.</summary>
    [Fact]
    public void ALineThatDiffersInOnePartIsAnotherLine()
    {
        var one = Full;

        Assert.NotEqual(one, new WorkScopeLine(null, new Uri(Address), Number));
        Assert.NotEqual(one, new WorkScopeLine(string.Empty, new Uri(Address), Number));
        Assert.NotEqual(one, new WorkScopeLine("cmdemo1-system", new Uri(Address), Number));
        Assert.NotEqual(one, new WorkScopeLine(Name.ToUpperInvariant(), new Uri(Address), Number));
        Assert.NotEqual(one, new WorkScopeLine(" " + Name, new Uri(Address), Number));
        Assert.NotEqual(one, new WorkScopeLine(Name, null, Number));
        Assert.NotEqual(one, new WorkScopeLine(Name, new Uri("https://github.com/o/r"), Number));
        Assert.NotEqual(one, new WorkScopeLine(Name, new Uri(Address + "/"), Number));
        Assert.NotEqual(one, new WorkScopeLine(Name, new Uri(Address), null));
        Assert.NotEqual(one, new WorkScopeLine(Name, new Uri(Address), string.Empty));
        Assert.NotEqual(one, new WorkScopeLine(Name, new Uri(Address), "680"));
        Assert.NotEqual(one, new WorkScopeLine(new Uri(Address).ToString(), new Uri(Address), Number));

        Assert.True(one != new WorkScopeLine(Name, new Uri(Address), null));
        Assert.False(one == new WorkScopeLine(Name, null, Number));
        Assert.False(one.Equals((object)new WorkScopeLine(null, new Uri(Address), Number)));
        Assert.False(one.Equals(one with { Url = null }));

        Assert.Equal(
            4,
            new HashSet<WorkScopeLine>
            {
                one,
                one with { Label = null },
                one with { Url = null },
                one with { Project = null },
            }.Count);
    }

    /// <summary>The address is held as the <see cref="Uri" /> the line was built of and never opened: no part of a
    /// line asks a host what it resolves to, and a line named on an address that leads nowhere answers every member
    /// the same way one named on a board that does. Two addresses written apart - a host in another case, a default
    /// port spelled out, a fragment left on the end - are one address to the record's comparing and to its hashing,
    /// because that is how an address compares and the line adds nothing to it and takes nothing away; a path in
    /// another case, and a slash at the end, are another address. The words a line says of its address are the
    /// address's own: the fragment it was written with is said, the case of the host and the default port are not,
    /// and the <c>href</c> the page builds from the same address is its absolute form, which the markup is pinned on
    /// elsewhere.</summary>
    [Fact]
    public void AnAddressIsHeldAsAnAddressAndNeverAskedAnything()
    {
        var address = new Uri("https://work-items.invalid.example/cmdemo1-system");
        var line = new WorkScopeLine("cmdemo1-system", address, Number);

        Assert.Same(address, line.Url);
        Assert.Equal("cmdemo1-system" + NewTab, line.Title);

        var spelled = new Uri("https://WORK-ITEMS.INVALID.EXAMPLE:443/cmdemo1-system#board");
        var anotherWay = new WorkScopeLine("cmdemo1-system", spelled, Number);

        Assert.True(line.Equals(anotherWay));
        Assert.True(line == anotherWay);
        Assert.False(line != anotherWay);
        Assert.Equal(line.GetHashCode(), anotherWay.GetHashCode());
        Assert.Single(new HashSet<WorkScopeLine> { line, anotherWay });

        // The words are the address's own and not the letters it was typed in: the fragment is said, although it
        // divides nothing, while the host's case and the default port - which divide nothing either - are not said.
        Assert.Contains(
            "Url = https://work-items.invalid.example/cmdemo1-system#board",
            anotherWay.ToString(),
            StringComparison.Ordinal);
        Assert.DoesNotContain(":443", anotherWay.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("WORK-ITEMS", anotherWay.ToString(), StringComparison.Ordinal);

        // A path is part of the address: the same host named for another repository is another line.
        var slashed = line with { Url = new Uri("https://work-items.invalid.example/cmdemo1-system/") };
        var plain = line with { Url = new Uri("http://work-items.invalid.example/cmdemo1-system") };
        var folded = line with { Url = new Uri("https://work-items.invalid.example/Cmdemo1-system") };

        Assert.NotEqual(line, slashed);
        Assert.NotEqual(line, plain);
        Assert.NotEqual(line, folded);
        Assert.Equal(4, new HashSet<WorkScopeLine> { line, slashed, plain, folded }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two lines that are no line at
    /// all are one, a line and no line are two - the equality and the inequality each saying it, from either side -
    /// and the line asked about itself says no to the nothing, to a word it is not, to a bare number, to the
    /// anonymous thing that happens to carry its three parts, and to the board the file named rather than showed.</summary>
    [Fact]
    public void NoLineIsOneWithNoLineAndNotWithAnyOtherThing()
    {
        WorkScopeLine? nothing = null;
        WorkScopeLine? alsoNothing = null;
        var there = Full;

        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == (WorkScopeLine?)null);
        Assert.True(there != (WorkScopeLine?)null);
        Assert.False(there.Equals(nothing));
        Assert.False(there.Equals((object?)null));
        Assert.True(there.Equals(there));
        Assert.Null(default(WorkScopeLine));
        Assert.Null(((WorkScopeLine?)null)?.Title);

        // A line of nothing at all is still a line: what turns it into no line at all is the read of the file, pinned
        // in WorkItemsTests, and not the record.
        Assert.NotNull(new WorkScopeLine(null, null, null));
        Assert.False(there.Equals((WorkScopeLine?)null));

        Assert.False(there.Equals("WorkScopeLine { Label = , Url = , Project = , Title =  }"));
        Assert.False(there.Equals((object)679));
        Assert.False(there.Equals(new { Label = Name, Url = new Uri(Address), Project = Number }));
        Assert.False(there.Equals(new WorkItemsScope(Address, 679, "A bot may take an item from Todo.")));
        Assert.Equal(2, new HashSet<WorkScopeLine?> { there, nothing, alsoNothing }.Count);
    }

    /// <summary>The hash says the parts and nothing else: the same three parts hashed again are the same hash, a part
    /// turned is another hash, and the three lines of nothing, of empty words and of every part said stand apart in a
    /// set of their own - whatever the record's words make of the first two, which are said alike.</summary>
    [Fact]
    public void TheHashFollowsThePartsAndStandsStillFromOneCallToTheNext()
    {
        var full = Full;
        var empty = new WorkScopeLine(string.Empty, null, string.Empty);
        var nothing = new WorkScopeLine(null, null, null);
        var named = new WorkScopeLine(Name, null, Number);

        Assert.Equal(full.GetHashCode(), new WorkScopeLine(Name, new Uri(Address), Number).GetHashCode());
        Assert.Equal(empty.GetHashCode(), empty.GetHashCode());
        Assert.Equal(nothing.GetHashCode(), new WorkScopeLine(null, null, null).GetHashCode());
        Assert.Equal(named.GetHashCode(), (full with { Url = null }).GetHashCode());
        Assert.NotEqual(full.GetHashCode(), empty.GetHashCode());
        Assert.NotEqual(full.GetHashCode(), named.GetHashCode());
        Assert.NotEqual(full.GetHashCode(), (full with { Url = null }).GetHashCode());
        Assert.Equal(3, new HashSet<WorkScopeLine> { full, empty, nothing }.Count);
    }

    // ------------------------------------------------------------ the words the record says itself

    /// <summary>The words the line says its parts in, and the title it works out among them: its own name at the
    /// front, the parts in the order they are named and the title last of all, the words as the letters they are -
    /// whitespace, tab and all - and the address in the words the address itself says. The title is said as well as
    /// the name it is made of, so a line written out says the page's words after the name once for the part and once
    /// for the title, and a part nobody said is the empty place between the mark and the comma.</summary>
    [Fact]
    public void TheRecordSaysItsPartsAndItsTitleInTheOrderTheyAreNamed()
    {
        Assert.Equal(
            "WorkScopeLine { Label = clearmeasure-aisf-sample-apps/cmdemo1-system, Url = https://github.com/clearmeasure-aisf-sample-apps/cmdemo1-system, Project = 679, Title = clearmeasure-aisf-sample-apps/cmdemo1-system (opens in a new tab) }",
            Full.ToString());
        Assert.Equal("WorkScopeLine { Label = , Url = , Project = , Title =  }", new WorkScopeLine(null, null, null).ToString());
        Assert.Equal("WorkScopeLine { Label = , Url = , Project = 0, Title =  }", new WorkScopeLine(null, null, "0").ToString());
        Assert.Equal(
            "WorkScopeLine { Label = cmdemo1-system, Url = https://github.com/o/r, Project = , Title = cmdemo1-system (opens in a new tab) }",
            new WorkScopeLine("cmdemo1-system", new Uri("https://github.com/o/r"), null).ToString());
        Assert.Equal(
            "WorkScopeLine { Label = cmdemo1-system, Url = , Project = 679, Title =  }",
            new WorkScopeLine("cmdemo1-system", null, "679").ToString());
        Assert.Equal(
            "WorkScopeLine { Label =    , Url = , Project = \t, Title =  }",
            new WorkScopeLine("   ", null, "\t").ToString());

        // A name of nothing at all with an address beside it is titled, and says so: the title is the page's words
        // with no name in front of them, which is the line the read never makes and the record cannot refuse.
        Assert.Equal(
            "WorkScopeLine { Label = , Url = https://github.com/o/r, Project = , Title =  (opens in a new tab) }",
            new WorkScopeLine(string.Empty, new Uri("https://github.com/o/r"), null).ToString());
    }

    /// <summary>The words are the record's own and none of the page's: no dash for a part that is nothing, no
    /// <c>n/a</c> for a thing not metered, no markup and no address turned into a label - all of which are what the
    /// page makes of the same three parts, pinned in <see cref="WorkItemsTests" /> and in
    /// <see cref="WorkItemsViewTests" />.</summary>
    [Fact]
    public void TheRecordsOwnWordsSayNothingOfThePagesWords()
    {
        var line = new WorkScopeLine(null, null, null);

        Assert.DoesNotContain("—", line.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("n/a", line.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("<a", line.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("href", line.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("(opens in a new tab)", line.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("(opens in a new tab)", new WorkScopeLine(Name, null, Number).ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("https", new WorkScopeLine(Name, null, Number).ToString(), StringComparison.Ordinal);
    }

    /// <summary>The words of a line follow no reader at all: a number with a separator in it, a number below zero and
    /// a name with the letters of it that another reader would fold are said in the very letters they were written
    /// in, and so is the title made of them. A line is three parts held and one of them joined to a literal, and
    /// nothing is turned into a number, a date or a word of another language.</summary>
    [Fact]
    public void TheWordsOfALineFollowNoReaderAtAll()
    {
        var line = new WorkScopeLine("-Iİ-5", new Uri("https://work-items.invalid.example/-5"), "-1,234.5");

        Assert.Equal("-Iİ-5" + NewTab, line.Title);
        Assert.Equal(
            Reader(SignedAndGroupedDifferently, () => line.ToString()!),
            line.ToString());
        Assert.Equal("-1,234.5", Reader(SignedAndGroupedDifferently, () => line.Project!));
        Assert.Equal(line.GetHashCode(), Reader(SignedAndGroupedDifferently, () => line.GetHashCode()));
        Assert.Equal(line.Title, Reader(SignedAndGroupedDifferently, () => line.Title));
    }

    // --------------------------------------------------------------------------- the machinery

    /// <summary>The public members the line declares itself: its properties and the methods that are not the get and
    /// the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(WorkScopeLine)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The one public constructor of the record: the three parts, in the order the page names them.</summary>
    private static ConstructorInfo Constructor() =>
        typeof(WorkScopeLine).GetConstructor([typeof(string), typeof(Uri), typeof(string)])!;

    /// <summary>True for the kinds of moment a line would have to be told about, or would have to go and ask a clock
    /// for - none of which a line holds, takes, or hands back.</summary>
    private static bool IsMoment(Type type) =>
        type == typeof(DateTimeOffset) || type == typeof(DateTime) || type == typeof(TimeSpan) || type == typeof(DateOnly) || type == typeof(TimeOnly);

    /// <summary>A reader that writes a negative number with another sign and groups thousands with a space: the
    /// difference between one culture's words for a number and another's this file can rely on, since a reader built
    /// out of the invariant one carries no machine's tables at all.</summary>
    private static CultureInfo SignedAndGroupedDifferently
    {
        get
        {
            var reader = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            reader.NumberFormat.NegativeSign = "~";
            reader.NumberFormat.NumberGroupSeparator = " ";
            reader.NumberFormat.NumberDecimalSeparator = ",";
            return reader;
        }
    }

    /// <summary>Asking a line for its parts, its title, its words or its hash under another reader - and handing the
    /// thread's own culture back afterwards, whatever the asking did.</summary>
    private static T Reader<T>(CultureInfo culture, Func<T> what)
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
