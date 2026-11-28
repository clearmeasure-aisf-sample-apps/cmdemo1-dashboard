using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="WorkItemsScope" />, the three parts that say which
/// board the work items came from, declared in <c>src/Dashboard/Work/WorkItemsReport.cs</c>: the repository the board
/// belongs to - an address, or a name when the file gives no address - the project number on it, and the rules of the
/// board in the file's own words.
/// <para>
/// What the other files already pin is left alone. The reading of the three parts out of <c>workitems.json</c> is
/// <c>WorkItemsScope.Read</c>, which is internal to an app that declares no <c>InternalsVisibleTo</c> (as
/// <c>Dashboard.Tests.csproj</c> notes of <c>JsonRead</c>) and is therefore pinned through
/// <see cref="WorkItemsReport.Parse" />: <see cref="WorkItemsTests.TheSampleFileIsTheBoardTheViewShows" /> says that
/// the shipped sample's scope is the hand-built record of its address, its 679 and its one sentence;
/// <see cref="WorkItemsTests.TheFeedSampleUsesThePublishedNames" /> that a board named rather than addressed keeps its
/// name and that the array of sentences arrives as one string; and
/// <see cref="WorkItemsTests.AnEmptyObjectIsAReportWithNothingInIt" /> that a file with no <c>scope</c> is a report
/// with none. The trimming of a word, the turning of a number that is no number and the leaving out of a scope that
/// says nothing are that reading's, not this record's. The words the page makes of a scope - the label cut out of the
/// address, the number told as a word, the rules repeated under the board - are <see cref="WorkItemsText.Scope" />'s
/// and <c>WorkItemsView.razor</c>'s, pinned on those words in <see cref="WorkItemsTests" /> and in
/// <see cref="WorkItemsViewTests" />.
/// </para>
/// <para>
/// What is left, and is pinned here, is the record itself: the constructor handing each of its three arguments to the
/// property of its name and holding the very word it was given; the two words kept in their exact letters - nothing,
/// the word of no characters, a word of whitespace, a word padded, a word in another case, a word a thousand
/// characters long - and compared in those letters rather than as a reader hears them; the number stood on as it is
/// given, at the ends an <see cref="int" /> runs to, below zero and at zero, and the number nobody gave standing
/// unsaid rather than at the zero it is not; a part turned as the scope is made and the copy that turns one part while
/// it keeps the other two; the three parts coming back out in the order they went in; the two scopes being one while
/// their three parts are, with the hash, the set and the operators following that, and the nothing on either side of
/// each; the words the scope says its three parts in, and those words following whoever reads a number; and the public
/// surface, so that the day a fourth part is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a scope holds two words and a number, no
/// moment and no count, and calls nothing at all - every value below is one the test brought. The address is kept and
/// compared as the letters it is written in and is never opened, and the board it points at is never asked anything.
/// </para>
/// </summary>
public class WorkItemsScopeEdgeCaseTests
{
    /// <summary>The board of the shipped <c>workitems.json</c>: its address, its project and its one sentence.</summary>
    private const string Address = "https://github.com/clearmeasure-aisf-sample-apps/cmdemo1-system";

    private const string Sentence = "A bot may take an item from Todo. Done means the release is deployed to prod.";

    /// <summary>A scope with every part said, so that a part landing on another part's place is seen rather than
    /// quietly matching a third of the same record.</summary>
    private static WorkItemsScope Full => new(Address, 679, Sentence);

    /// <summary>A word of the test's own, so that a part held as the very instance it was given is proved by the
    /// holding and not by the interning of a literal.</summary>
    private static string Own(string word) => new(word.ToCharArray());

    // --------------------------------- the three parts, by the names of their own properties

    /// <summary>The constructor's three arguments, position by position, come back as the property of the same name:
    /// the repository first, the project beside it, the rules last - the order the file names them in and the order
    /// the page says them. The two words are held as the very instances the scope was built of, and a new scope of the
    /// same three parts is another scope, not the one it was built of.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var repo = Own(Address);
        var rules = Own(Sentence);
        var scope = new WorkItemsScope(repo, 679, rules);

        Assert.Same(repo, scope.Repo);
        Assert.Same(rules, scope.Rules);
        Assert.Equal(679, scope.Project);
        Assert.Equal(Address, scope.Repo);
        Assert.Equal(Sentence, scope.Rules);

        // A board named rather than addressed: the first part holds the name exactly as it is given, and whether a
        // name is an address is the page's asking and not this record's.
        var named = new WorkItemsScope("clearmeasure-aisf-sample-apps/cmdemo1-workorders", 679, "worker bot");

        Assert.Equal("clearmeasure-aisf-sample-apps/cmdemo1-workorders", named.Repo);
        Assert.Equal("worker bot", named.Rules);
        Assert.Equal(679, named.Project);
        Assert.NotEqual(scope, named);

        var again = new WorkItemsScope(repo, 679, rules);

        Assert.Equal(scope, again);
        Assert.NotSame(scope, again);
    }

    /// <summary>The number of the project stands where it is put, at the ends an <see cref="int" /> runs to and at
    /// both sides of zero: the record adds nothing to it, rounds nothing, caps nothing and refuses nothing - not even
    /// a number no board could be numbered by, which the page tells as the file gave it.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(679)]
    [InlineData(-1)]
    [InlineData(-2147483648)]
    [InlineData(2147483647)]
    public void TheProjectStandsAtTheEndsANumberRunsTo(int project)
    {
        var scope = new WorkItemsScope(Address, project, Sentence);

        Assert.Equal(project, scope.Project);
        Assert.Equal(new WorkItemsScope(Address, project, Sentence), scope);
        Assert.NotEqual(new WorkItemsScope(Address, null, Sentence), scope);
        Assert.Equal(project, (new WorkItemsScope(Address, project, Sentence) with { }).Project);
    }

    /// <summary>The project a file does not give stands unsaid, which is not a project of zero and not a project of
    /// the number that is nothing: the record whose number is nothing is a record, and it is another record than the
    /// one whose number is zero, told apart by its equality, its operators and its words. It is what lets the page
    /// leave the number out of its line rather than tell a reader that the board is numbered nought.</summary>
    [Fact]
    public void AProjectNobodyGaveStandsUnsaidAndNotAtZero()
    {
        var unsaid = new WorkItemsScope(Address, null, Sentence);
        var zero = new WorkItemsScope(Address, 0, Sentence);

        Assert.Null(unsaid.Project);
        Assert.Equal(Address, unsaid.Repo);
        Assert.Equal(Sentence, unsaid.Rules);

        Assert.NotEqual(unsaid, zero);
        Assert.False(unsaid == zero);
        Assert.True(unsaid != zero);
        Assert.NotEqual(Words(unsaid), Words(zero));

        // The number that is not there and the number of nothing are two scopes and are stood in two places of a set,
        // whatever hash the record gives them: a nullable number with nothing in it is hashed as the number zero, so
        // a set separates these two on their equality and not on their hash. That is why the hash is asked below only
        // that one scope say the same hash twice, and that a scope with a part turned say another.
        Assert.Equal(2, new HashSet<WorkItemsScope> { unsaid, zero }.Count);

        var turnOff = Full with { Project = null };

        Assert.Null(turnOff.Project);
        Assert.Equal(unsaid, turnOff);
    }

    /// <summary>Either word is kept in its exact letters: the word of no characters, the word that is only
    /// whitespace, a word padded at both ends, a word in another case and a word that is no address at all all
    /// stand where they stand. Nothing here trims, lowers, folds or shortens: the trimming is the file's reading,
    /// pinned where the reading happens, and the scope the record is built of says the word its builder gave it,
    /// nothing more and nothing less.</summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   \t ")]
    [InlineData("cmdemo1-system")]
    [InlineData("CMDemo1-System")]
    [InlineData("  cmdemo1-system  ")]
    [InlineData("clearmeasure-aisf-sample-apps/cmdemo1-workorders")]
    [InlineData("not an address at all")]
    [InlineData("https://github.com/o/r")]
    [InlineData("A bot may take an item from Todo. Done means the release is deployed to prod.")]
    public void AWordIsKeptExactlyAsItIsGiven(string word)
    {
        Assert.Equal(word, new WorkItemsScope(word, 679, word).Repo);
        Assert.Equal(word, new WorkItemsScope(null, null, word).Rules);
        Assert.Same(word, new WorkItemsScope(word, null, null).Repo);
    }

    /// <summary>The empty word is a word and not the absence of one: a repository said as nothing at all and a
    /// repository not said are two different scopes, one of them with an empty place and the other with no place
    /// filled, and the record does not settle which of the two the page should leave out - that is the reading's
    /// decision, and the reading is pinned in <see cref="WorkItemsTests" />.</summary>
    [Fact]
    public void AnEmptyWordIsAWordAndNotTheAbsenceOfOne()
    {
        var empty = new WorkItemsScope(string.Empty, 0, string.Empty);
        var unsaid = new WorkItemsScope(null, null, null);

        Assert.Equal(string.Empty, empty.Repo);
        Assert.Equal(string.Empty, empty.Rules);
        Assert.Null(unsaid.Repo);
        Assert.Null(unsaid.Rules);
        Assert.Null(unsaid.Project);

        Assert.NotEqual(empty, unsaid);
        Assert.NotEqual(empty.GetHashCode(), unsaid.GetHashCode());
        Assert.Equal(2, new HashSet<WorkItemsScope> { empty, unsaid }.Count);
        Assert.NotEqual(Words(empty), Words(unsaid));
    }

    /// <summary>A rule of a thousand characters is kept whole, and said whole: the record does not shorten what the
    /// file wrote, and the cutting to a line is the page's, done where the page is pinned.</summary>
    [Fact]
    public void ALongBoardOfRulesIsKeptWhole()
    {
        var longRules = new string('r', 1000);
        var scope = new WorkItemsScope(Address, 679, longRules);

        Assert.Same(longRules, scope.Rules);
        Assert.Equal(1000, scope.Rules!.Length);
        Assert.EndsWith("rrr", scope.Rules, StringComparison.Ordinal);
        Assert.Contains(longRules, Words(scope), StringComparison.Ordinal);
        Assert.Equal(scope, new WorkItemsScope(Address, 679, new string('r', 1000)));
    }

    /// <summary>The two words of a board are compared in their own letters and not as a reader hears them: the same
    /// name in another case and the same name with a space before it are another board, whichever culture is reading.
    /// The lookup that finds an entry whatever case it is named in is elsewhere in the app; a scope is not looked up,
    /// it is stood on.</summary>
    [Fact]
    public void ABoardIsComparedInItsOwnLettersAndNotAsAReaderHearsIt()
    {
        var scope = new WorkItemsScope("cmdemo1-system", 679, "A bot may take an item from Todo.");

        Assert.True(StringComparer.OrdinalIgnoreCase.Equals("cmdemo1-system", scope.Repo));
        Assert.NotEqual(scope, new WorkItemsScope("CMDemo1-System", 679, scope.Rules));
        Assert.NotEqual(scope, new WorkItemsScope(" cmdemo1-system", 679, scope.Rules));
        Assert.NotEqual(scope, new WorkItemsScope("cmdemo1-system", 679, "a bot may take an item from todo."));
        Assert.Equal(
            3,
            new HashSet<WorkItemsScope>
            {
                scope,
                scope with { Repo = "CMDemo1-System" },
                scope with { Repo = " cmdemo1-system" },
            }.Count);
    }

    // ------------------------------------------------------- a part turned, and the copy that turns it

    /// <summary>Each part may be turned as the scope is made - the address to a name, the number to the nothing and
    /// the rules to the empty word - and stands where it is turned to, while the parts the turn does not name stay
    /// what they were. A scope with its parts turned back on is another scope and never the same reference.</summary>
    [Fact]
    public void APartTurnedAsTheScopeIsMadeStandsWhereItIsTurnedTo()
    {
        var scope = Full;

        var named = new WorkItemsScope(Address, 679, Sentence) { Repo = "cmdemo1-system" };

        Assert.Equal("cmdemo1-system", named.Repo);
        Assert.Equal(679, named.Project);
        Assert.Equal(Sentence, named.Rules);
        Assert.NotSame(scope, named);
        Assert.NotEqual(scope, named);

        var noProject = new WorkItemsScope(Address, 679, Sentence) { Project = null };

        Assert.Null(noProject.Project);
        Assert.Equal(Address, noProject.Repo);
        Assert.Equal(new WorkItemsScope(Address, null, Sentence), noProject);

        var said = new WorkItemsScope(null, null, null) { Repo = Address, Project = 0, Rules = string.Empty };

        Assert.Equal(new WorkItemsScope(Address, 0, string.Empty), said);
        Assert.NotEqual(new WorkItemsScope(null, null, null), said);
    }

    /// <summary>The copy that turns nothing is the same three parts in a new scope, and the copy that turns one part
    /// keeps the other two as the very instances the original was built of: the original stands as it stood while the
    /// copy carries the turned part, and a copy that names all three parts as they stand is the scope it started from
    /// without being it.</summary>
    [Fact]
    public void ACopyTurnsThePartItNamesAndKeepsTheOtherTwo()
    {
        var rules = Own(Sentence);
        var scope = new WorkItemsScope(Address, 679, rules);

        var unchanged = scope with { };

        Assert.Equal(scope, unchanged);
        Assert.NotSame(scope, unchanged);

        var otherBoard = scope with { Repo = "clearmeasure-aisf-sample-apps/cmdemo1-workorders" };

        Assert.Equal(Address, scope.Repo);
        Assert.Equal("clearmeasure-aisf-sample-apps/cmdemo1-workorders", otherBoard.Repo);
        Assert.Equal(679, otherBoard.Project);
        Assert.Same(rules, otherBoard.Rules);
        Assert.NotSame(scope, otherBoard);
        Assert.NotEqual(scope, otherBoard);

        var noRules = scope with { Rules = null };

        Assert.Null(noRules.Rules);
        Assert.Equal(Address, noRules.Repo);
        Assert.Equal(679, noRules.Project);
        Assert.Equal(new WorkItemsScope(Address, 679, null), noRules);

        var repacked = scope with { Repo = scope.Repo, Project = scope.Project, Rules = scope.Rules };

        Assert.Equal(scope, repacked);
        Assert.NotSame(scope, repacked);
        Assert.Same(rules, repacked.Rules);
    }

    // --------------------------------------------- the record falling apart, and its own declared shape

    /// <summary>The three parts come back out in the order they went in - the repository, the number, the rules - and
    /// the parts nobody gave come out as nothing rather than as the empty word and the zero they are not. What came
    /// out goes back in the same way: the scope the three parts fall apart into is the scope they were.</summary>
    [Fact]
    public void TheThreePartsComeOutAgainInTheOrderTheyWentIn()
    {
        var (repo, project, rules) = Full;

        Assert.Equal(Address, repo);
        Assert.Equal(679, project);
        Assert.Equal(Sentence, rules);
        Assert.Equal(Full, new WorkItemsScope(repo, project, rules));

        var (noRepo, noProject, noRules) = new WorkItemsScope(null, null, null);

        Assert.Null(noRepo);
        Assert.Null(noProject);
        Assert.Null(noRules);

        var (name, number, _) = new WorkItemsScope("cmdemo1-workorders", -7, Sentence);

        Assert.Equal("cmdemo1-workorders", name);
        Assert.Equal(-7, number);
        Assert.Equal(new WorkItemsScope("cmdemo1-workorders", -7, Sentence), new WorkItemsScope(name, number, rules));
    }

    /// <summary>The scope's whole public surface, as it stands: the three parts named in the order the constructor
    /// names them, and the members every record is asked - falling apart, comparing, hashing and saying itself - and
    /// nothing else to call. The three parts are two words and a number, none of them a moment, so nothing the record
    /// holds can move between one call and the next: that is why no test below needs a clock, and a fourth part added
    /// to the record has to be written here as well, which is the point.</summary>
    [Fact]
    public void TheRecordHasThreePartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Repo", "Project", "Rules"],
            [.. typeof(WorkItemsScope)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        var properties = typeof(WorkItemsScope)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(
            ["Deconstruct", "Equals", "GetHashCode", "Project", "Repo", "Rules", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());

        Assert.Equal(typeof(string), properties["Repo"].PropertyType);
        Assert.Equal(typeof(int?), properties["Project"].PropertyType);
        Assert.Equal(typeof(string), properties["Rules"].PropertyType);
        Assert.All(
            properties.Values,
            property => Assert.True(property is { CanRead: true, CanWrite: true }, $"{property.Name} is not a part a caller can read and turn"));
        Assert.All(
            properties.Values,
            property => Assert.False(
                property.PropertyType == typeof(DateTimeOffset) || property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(Uri),
                $"{property.Name} would stand on a moment or an address the record would have to go and look up"));

        Assert.True(typeof(WorkItemsScope).IsSealed);
        Assert.Equal([typeof(string), typeof(int?), typeof(string)], [.. Constructor().GetParameters().Select(parameter => parameter.ParameterType)]);
        Assert.Equal(["Repo", "Project", "Rules"], [.. Constructor().GetParameters().Select(parameter => parameter.Name!)]);
    }

    // -------------------------------------------------------------- the equality, the hash, and the two

    /// <summary>Two scopes are one while their three parts are - each word as the letters it is, the number as the
    /// number it stands on, the nothing among them included - whichever of the four ways of asking is used: the
    /// record's own <c>Equals</c>, the one of <see cref="object" />, <c>==</c> and <c>!=</c>. The hash follows the
    /// equality, boxed or unboxed and from one call to the next, so the scopes that are one are one entry of a set;
    /// and two words written apart are one word to the record, which compares letters and not references.</summary>
    [Fact]
    public void TwoScopesAreOneWhileTheirThreePartsAre()
    {
        var one = Full;
        var two = new WorkItemsScope(Address, 679, Sentence);

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
        Assert.Single(new HashSet<WorkItemsScope> { one, two });

        var written = new WorkItemsScope(Own(Address), 679, Own(Sentence));

        Assert.NotSame(one.Repo, written.Repo);
        Assert.Equal(one, written);
        Assert.Equal(one.GetHashCode(), written.GetHashCode());
        Assert.Single(new HashSet<WorkItemsScope> { one, written });

        var nothing = new WorkItemsScope(null, null, null);
        var alsoNothing = new WorkItemsScope(null, null, null);

        Assert.Equal(nothing, alsoNothing);
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.Equal(nothing.GetHashCode(), alsoNothing.GetHashCode());
    }

    /// <summary>A difference in either part is another scope, and so is the same three parts told in another place:
    /// the address left out or emptied or renamed, the number turned to the nothing a missing number is not, turned to
    /// zero, turned by one, the rules cut of its final mark, and the address standing in the rules' place. A set of the
    /// scopes that differ in one part each holds them all apart.</summary>
    [Fact]
    public void AScopeThatDiffersInOnePartIsAnotherScope()
    {
        var one = Full;

        Assert.NotEqual(one, new WorkItemsScope(null, 679, Sentence));
        Assert.NotEqual(one, new WorkItemsScope(string.Empty, 679, Sentence));
        Assert.NotEqual(one, new WorkItemsScope("cmdemo1-system", 679, Sentence));
        Assert.NotEqual(one, new WorkItemsScope(Address, null, Sentence));
        Assert.NotEqual(one, new WorkItemsScope(Address, 0, Sentence));
        Assert.NotEqual(one, new WorkItemsScope(Address, 680, Sentence));
        Assert.NotEqual(one, new WorkItemsScope(Address, -679, Sentence));
        Assert.NotEqual(one, new WorkItemsScope(Address, 679, null));
        Assert.NotEqual(one, new WorkItemsScope(Address, 679, "A bot may take an item from Todo"));
        Assert.NotEqual(one, new WorkItemsScope("679", null, Address));

        Assert.True(one != new WorkItemsScope(Address, 679, null));
        Assert.False(one == new WorkItemsScope(Address, 679, null));
        Assert.False(one.Equals((object)new WorkItemsScope(Address, null, Sentence)));
        Assert.False(one.Equals(one with { Rules = null }));

        Assert.Equal(
            4,
            new HashSet<WorkItemsScope>
            {
                one,
                one with { Repo = null },
                one with { Project = null },
                one with { Rules = null },
            }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two scopes that are no scope at
    /// all are one, a scope and no scope are two - the equality and the inequality each saying it, from either side -
    /// and the scope asked about itself says no to the nothing, to a word it is not, to a bare number, to the
    /// anonymous thing that happens to carry its three parts and to the board line the page makes of it.</summary>
    [Fact]
    public void NoScopeIsOneWithNoScopeAndNotWithAnyOtherThing()
    {
        WorkItemsScope? nothing = null;
        WorkItemsScope? alsoNothing = null;
        var there = Full;

        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == (WorkItemsScope?)null);
        Assert.True(there != (WorkItemsScope?)null);
        Assert.False(there.Equals(nothing));
        Assert.False(there.Equals((object?)null));
        Assert.True(there.Equals(there));
        Assert.Null(default(WorkItemsScope));

        // A scope of nothing at all is still a scope: what turns it into no scope is the reading of the file, pinned
        // in WorkItemsTests, and not the record.
        Assert.NotNull(new WorkItemsScope(null, null, null));
        Assert.False(there.Equals((WorkItemsScope?)null));

        Assert.False(there.Equals("WorkItemsScope { Repo = , Project = , Rules =  }"));
        Assert.False(there.Equals((object)679));
        Assert.False(there.Equals(new { Repo = Address, Project = 679, Rules = Sentence }));
        Assert.False(there.Equals(new WorkScopeLine("clearmeasure-aisf-sample-apps/cmdemo1-system", new Uri(Address), "679")));
        Assert.Equal(2, new HashSet<WorkItemsScope?> { there, nothing, alsoNothing }.Count);
    }

    /// <summary>The hash says the parts and nothing else: the same three parts hashed again are the same hash, a part
    /// turned is another hash, and the three scopes of nothing, of empty words and of every part said stand apart in
    /// a set of their own. The ends of a number are hashed with the number, not instead of it.</summary>
    [Fact]
    public void TheHashFollowsThePartsAndStandsStillFromOneCallToTheNext()
    {
        var full = Full;
        var empty = new WorkItemsScope(string.Empty, 0, string.Empty);
        var nothing = new WorkItemsScope(null, null, null);
        var least = new WorkItemsScope(Address, int.MinValue, Sentence);
        var greatest = new WorkItemsScope(Address, int.MaxValue, Sentence);

        Assert.Equal(full.GetHashCode(), new WorkItemsScope(Address, 679, Sentence).GetHashCode());
        Assert.Equal(empty.GetHashCode(), empty.GetHashCode());
        Assert.Equal(least.GetHashCode(), new WorkItemsScope(Address, int.MinValue, Sentence).GetHashCode());
        Assert.NotEqual(full.GetHashCode(), empty.GetHashCode());
        Assert.NotEqual(empty.GetHashCode(), nothing.GetHashCode());
        Assert.NotEqual(least.GetHashCode(), greatest.GetHashCode());
        Assert.NotEqual(full.GetHashCode(), (full with { Project = int.MaxValue }).GetHashCode());
        Assert.Equal(3, new HashSet<WorkItemsScope> { full, empty, nothing }.Count);
        Assert.Equal(2, new HashSet<WorkItemsScope> { least, greatest }.Count);
    }

    // ------------------------------------------------------------ the words the record says itself

    /// <summary>The words the scope says its three parts in: its own name at the front, the parts in the order they
    /// are named, the words as the letters they are - whitespace and all - and the number as the plain digits it
    /// stands on, with its sign below zero and with no thousand's separator. A part nobody said is the empty place
    /// between the mark and the comma or the closing brace. These are the record's words and not the page's: no dash
    /// and no link, which are what the page makes of the same three parts.</summary>
    [Fact]
    public void TheRecordSaysItsThreePartsInTheOrderTheyAreNamed()
    {
        Assert.Equal($"WorkItemsScope {{ Repo = {Address}, Project = 679, Rules = {Sentence} }}", Words(Full));
        Assert.Equal("WorkItemsScope { Repo = , Project = , Rules =  }", Words(new WorkItemsScope(null, null, null)));
        Assert.Equal("WorkItemsScope { Repo = , Project = 0, Rules =  }", Words(new WorkItemsScope(null, 0, null)));
        Assert.Equal("WorkItemsScope { Repo = , Project = 1234567, Rules =  }", Words(new WorkItemsScope(null, 1234567, null)));
        Assert.Equal("WorkItemsScope { Repo = cmdemo1-system, Project = -1234, Rules =  }", Words(new WorkItemsScope("cmdemo1-system", -1234, null)));
        Assert.Equal("WorkItemsScope { Repo =    , Project = , Rules = \t }", Words(new WorkItemsScope("   ", null, "\t")));
        Assert.Equal("WorkItemsScope { Repo = , Project = -2147483648, Rules =  }", Words(new WorkItemsScope(null, int.MinValue, null)));
        Assert.Equal("WorkItemsScope { Repo = , Project = 2147483647, Rules =  }", Words(new WorkItemsScope(null, int.MaxValue, null)));

        // The words are no measure of the scope: the empty word and the word that is not there are said apart, and
        // the page's own word for a part that is nothing is a dash this record has never heard of.
        Assert.NotEqual(Words(new WorkItemsScope(string.Empty, 0, string.Empty)), Words(new WorkItemsScope(null, null, null)));
        Assert.DoesNotContain("—", Words(new WorkItemsScope(null, null, null)), StringComparison.Ordinal);
        Assert.DoesNotContain("$", Words(Full), StringComparison.Ordinal);
        Assert.DoesNotContain("<a", Words(Full), StringComparison.Ordinal);
    }

    /// <summary>The number of these words follows whoever reads them, which is what sets the record's own words apart
    /// from the page's: a reader that writes a negative number with another sign is told <c>~5</c> by the scope and
    /// <c>-5</c> by <see cref="WorkItemsText.Scope" />, whose words are written for the dashboard and not for whoever
    /// looks. The two words of a scope follow no reader at all: letters are letters in every culture.</summary>
    [Fact]
    public void TheRecordsOwnWordsFollowTheReaderWhileThePagesWordsDoNot()
    {
        var scope = new WorkItemsScope("cmdemo1-system", -5, null);

        Assert.Equal("WorkItemsScope { Repo = cmdemo1-system, Project = ~5, Rules =  }", Reader(SignedDifferently, () => scope.ToString()!));
        Assert.Equal("WorkItemsScope { Repo = cmdemo1-system, Project = -5, Rules =  }", Words(scope));
        Assert.Equal("-5", Reader(SignedDifferently, () => WorkItemsText.Scope(scope)!.Project!));
        Assert.Equal("cmdemo1-system", Reader(SignedDifferently, () => WorkItemsText.Scope(scope)!.Label!));

        // The same scope read by two readers says the same words about its two parts and another only about its
        // number: the one sign apart, the two whole strings are the same.
        Assert.Equal(Words(scope), Reader(SignedDifferently, () => scope.ToString()!.Replace("~", "-", StringComparison.Ordinal)));
    }

    // --------------------------------------------------------------------------- the machinery

    /// <summary>The public members the scope declares itself: its properties and the methods that are not the get and
    /// the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the test of the surface rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(WorkItemsScope)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The one public constructor of the record: the three parts, in the order the file names them.</summary>
    private static ConstructorInfo Constructor() =>
        typeof(WorkItemsScope).GetConstructor([typeof(string), typeof(int?), typeof(string)])!;

    /// <summary>The words a scope says its three parts in, taken in the invariant culture whatever the reader of this
    /// test happens to be: the record's own words follow whoever reads them, which is pinned on its own further up
    /// this file, so the strings there are the point and the culture only the way to say them once.</summary>
    private static string Words(WorkItemsScope scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return Reader(CultureInfo.InvariantCulture, () => scope.ToString()!);
    }

    /// <summary>A reader that writes a negative number with another sign: the one difference between one culture's
    /// words for a project number and another's this file can rely on, since a plain number carries no group of three
    /// digits and German, English and the invariant culture all write <c>-5</c> the same way.</summary>
    private static CultureInfo SignedDifferently
    {
        get
        {
            var reader = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            reader.NumberFormat.NegativeSign = "~";
            return reader;
        }
    }

    /// <summary>Asking a scope for its parts, or itself, under another reader - and handing the thread's own culture
    /// back afterwards, whatever the asking did.</summary>
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

    // --------------------------------------------- the rules read out of a whole file, through the parser

    /// <summary>An array of sentences with padded and blank entries: the trimmed, non-blank lines are joined with one
    /// space, in the order written, through <c>JsonRead.Strings</c>.</summary>
    [Fact]
    public void RulesAsAnArrayOfPaddedAndBlankEntriesJoinsTheTrimmedNonBlankLinesWithOneSpace()
    {
        var report = WorkItemsReport.Parse("""
            { "scope": { "repo": "cmdemo1-system", "rules": ["  Take an item from Todo.  ", "", "  ", "Done means deployed to prod.", "   "] } }
            """);

        Assert.NotNull(report);
        Assert.Equal("Take an item from Todo. Done means deployed to prod.", report!.Scope!.Rules);
    }

    /// <summary>An array of only blanks, numbers and nulls: no string survives the trim, so <c>Rules</c> is null, and
    /// the scope is still said by its other parts.</summary>
    [Fact]
    public void RulesAsAnArrayOfOnlyBlanksNumbersAndNullsGivesNullRules()
    {
        var report = WorkItemsReport.Parse("""
            { "scope": { "repo": "cmdemo1-system", "project": 679, "rules": ["  ", "", 42, null] } }
            """);

        Assert.NotNull(report);
        Assert.NotNull(report!.Scope);
        Assert.Null(report.Scope.Rules);
    }

    /// <summary>An empty array: <c>Rules</c> is null, not the empty word.</summary>
    [Fact]
    public void RulesAsAnEmptyArrayGivesNullRules()
    {
        var report = WorkItemsReport.Parse("""
            { "scope": { "repo": "cmdemo1-system", "project": 679, "rules": [] } }
            """);

        Assert.NotNull(report);
        Assert.NotNull(report!.Scope);
        Assert.Null(report.Scope.Rules);
    }

    /// <summary><c>rules</c> as an object or a number: the array read finds no array, so <c>Rules</c> is null.</summary>
    [Theory]
    [InlineData("{ \"a\": 1 }")]
    [InlineData("7")]
    public void RulesAsAnObjectOrANumberGivesNullRules(string rulesValue)
    {
        var report = WorkItemsReport.Parse($$"""
            { "scope": { "repo": "cmdemo1-system", "project": 679, "rules": {{rulesValue}} } }
            """);

        Assert.NotNull(report);
        Assert.NotNull(report!.Scope);
        Assert.Null(report.Scope.Rules);
    }

    /// <summary><c>rules</c> as a plain string is taken as-is ahead of the array read: the string arm wins.</summary>
    [Fact]
    public void RulesAsAPlainStringIsTakenAsIsAheadOfTheArrayRead()
    {
        var report = WorkItemsReport.Parse("""
            { "scope": { "repo": "cmdemo1-system", "rules": "Take an item from Todo. Done means deployed to prod." } }
            """);

        Assert.NotNull(report);
        Assert.Equal("Take an item from Todo. Done means deployed to prod.", report!.Scope!.Rules);
    }
}
