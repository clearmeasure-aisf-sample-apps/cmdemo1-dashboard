using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="RuntimeApplicationChange" /> is the word the runtime diagram says when it zooms into the box of a
/// deployable, back out of it, or gives up on an address that names a box it has none of: which environment the view
/// stands in, which deployable's assemblies are to be shown - and nothing at all when the diagram is to be shown
/// again - and whether the change is a step of the browser's history. Three members, no logic of its own, nothing to
/// refuse and nothing to ask.
/// <para>
/// What is pinned where it belongs is walked past here on purpose: <see cref="ApplicationViewTests" /> pins
/// <see cref="ApplicationText.IsName" /> and what a refused deployable name is worth to an address and to the boxes
/// of a diagram, and <see cref="ViewAddressTests" /> pins the addresses a view writes and parses; the components that
/// send a change - the zoom into a box, the back out of an application view and the correction of an address the
/// diagram has no box for - are the three places a change is made, and they are markup, pinned by the guards that
/// walk it. Those tests hand a change's two names to something else and ask that other thing what it decides.
/// </para>
/// <para>
/// What follows is the change itself, at the ends those tests leave open: the constructor handing each argument to
/// the member of its name and holding the very word it was given, and the three members coming back out in the order
/// they went in; the names kept exactly as they are given - the empty word, the word of spaces, a word in another
/// case, a word outside the ASCII and a word of the marks a deployable's name is not allowed - and cut by nothing, so
/// that a name at the sixty-four letters the address stops at, one letter over it and ten thousand of them are three
/// whole names; the nothing that is no name at all, which the change holds as its nothing rather than turning it into
/// the empty word or refusing it, on either of the two names it carries; the flag holding either of its two answers
/// and no third, which is the whole of what a step of the history is here; the copy that turns one member while it
/// keeps the other two; the equality of the typed record, of the boxed <see cref="object" /> and of <c>==</c> and
/// <c>!=</c>, and the hash that follows them into a set and a dictionary; the words the change says itself, which
/// name the three members in the order they are named and say all three as they stand; that the record decides
/// nothing - it asks no name whether it may open a view, and trims nothing - which is why its caller may hand it a
/// hostile word and find it waiting there whole; and the public surface, so that the day a fourth member is added the
/// list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a change holds two words and a flag,
/// calls nothing at all, and every value below is one the test brought. Of the types of the file the change stands
/// in, only <see cref="RuntimeLoader" /> and its reading of a version's file ask the repository anything, and they
/// are pinned with a stubbed answer in <see cref="ApplicationViewTests" />.
/// </para>
/// </summary>
public class RuntimeApplicationChangeEdgeCaseTests
{
    /// <summary>The longest deployable name the address takes: <see cref="ApplicationText.IsName" />'s own bound,
    /// written out here rather than read off it, so that a moved bound breaks a test instead of quietly moving along
    /// with it. The change cuts nothing at either of these lengths, which is the point of pinning them.</summary>
    private const int LongestName = 64;

    /// <summary>Zooming into the box of a deployable: the environment, the deployable, and a step of the history the
    /// browser's Back undoes.</summary>
    private static RuntimeApplicationChange Zoomed => new("uat", "ui", Step: true);

    /// <summary>Backing out of an application view to its environment's diagram: the deployable is the nothing that
    /// says there is no view to show.</summary>
    private static RuntimeApplicationChange Back => new("uat", null, Step: true);

    /// <summary>An address the diagram refused, corrected in place: the same words as the back out, and no step, so
    /// that the correction never takes the reader's Back button.</summary>
    private static RuntimeApplicationChange Corrected => new("uat", null, Step: false);

    /// <summary>A change of two empty names: nothing to say, and a change the record still holds.</summary>
    private static RuntimeApplicationChange Blank => new(string.Empty, string.Empty, Step: false);

    /// <summary>A change whose names are not there at all - the nothing a record answers with when the caller names
    /// neither of its two words.</summary>
    private static RuntimeApplicationChange Nothing => new(null!, null!, Step: false);

    // ----------------------------- the three members, by the names of their own properties

    /// <summary>The constructor's three arguments come back as the member of the same name - the environment first,
    /// the deployable in the middle and the step last, the order the diagram says them in - whichever order the
    /// caller names them in. Each word is held as the very string it was given, copied by nothing and turned by
    /// nothing, and the three changes the page really sends stand in their three members exactly as they were
    /// written.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToTheMemberOfItsName()
    {
        var environment = Own("uat");
        var deployable = Own("ui");
        var change = new RuntimeApplicationChange(environment, deployable, Step: true);

        Assert.Same(environment, change.Environment);
        Assert.Same(deployable, change.Deployable);
        Assert.Equal("uat", change.Environment);
        Assert.Equal("ui", change.Deployable);
        Assert.True(change.Step);

        // Named in the other order, in the same three places.
        var named = new RuntimeApplicationChange(Step: false, Deployable: "api", Environment: "prod");

        Assert.Equal("prod", named.Environment);
        Assert.Equal("api", named.Deployable);
        Assert.False(named.Step);
        Assert.Equal(new RuntimeApplicationChange("prod", "api", Step: false), named);

        // The change the page really sends when a box is zoomed, in its three members.
        Assert.Equal("uat", Zoomed.Environment);
        Assert.Equal("ui", Zoomed.Deployable);
        Assert.True(Zoomed.Step);

        // The back out and the correction: the deployable's place holds the nothing, and the two differ by the flag
        // and nothing else.
        Assert.Equal("uat", Back.Environment);
        Assert.Null(Back.Deployable);
        Assert.True(Back.Step);
        Assert.Equal("uat", Corrected.Environment);
        Assert.Null(Corrected.Deployable);
        Assert.False(Corrected.Step);

        // A name of the marks a deployable is spelled with, held in the deployable's place.
        var underscored = new RuntimeApplicationChange("tdd", "Work-Order_api2", Step: true);

        Assert.Equal("tdd", underscored.Environment);
        Assert.Equal("Work-Order_api2", underscored.Deployable);
        Assert.True(underscored.Step);

        // Two calls of the same letters are two references to the same letters, and the record keeps the one it was
        // given: only a word written on its own can tell the holding from a copying.
        Assert.NotSame(change.Environment, new RuntimeApplicationChange("uat", "ui", Step: true).Environment);
        Assert.NotSame(change.Deployable, new RuntimeApplicationChange("uat", "ui", Step: true).Deployable);
        Assert.Equal(change, new RuntimeApplicationChange("uat", "ui", Step: true));
    }

    /// <summary>Every pair of names is kept in its exact letters: the empty word, the word of spaces, the word with
    /// its whitespace about it, a word in another case, a word of digits only, a name of the marks a deployable is
    /// spelled with, a name of the marks it is not, a name outside the ASCII and the nothing of a word that is not
    /// there. Nothing here trims, nothing here changes a case, nothing here cuts a name short, nothing here turns the
    /// empty word into the nothing and nothing here asks a name whether it may open a view - the change says back what
    /// it was handed, in the very strings it was handed, with the flag it was handed beside it.</summary>
    [Theory]
    [InlineData("uat", "ui", true)]
    [InlineData("prod-uk", "Ui.Server", false)]
    [InlineData("", "", true)]
    [InlineData(" ", "  ", false)]
    [InlineData("  uat  ", " ui", true)]
    [InlineData("\tuat", "ui\t", false)]
    [InlineData("UAT", "UI", true)]
    [InlineData("app", "app", false)]
    [InlineData("0", "1", true)]
    [InlineData("pröd", "уи", false)]
    [InlineData("uat", "ui.server", true)]
    [InlineData("uat", "ui/x", false)]
    [InlineData("uat", "ui x", true)]
    [InlineData("uat", "<script>alert(1)<script>", false)]
    [InlineData("uat", "..", true)]
    [InlineData("uat", "ui%20x", false)]
    [InlineData(null, null, true)]
    [InlineData(null, "ui", false)]
    [InlineData("uat", null, true)]
    public void TheTwoNamesAreKeptExactlyAsTheyAreGiven(string? environment, string? deployable, bool step)
    {
        var change = new RuntimeApplicationChange(environment!, deployable, step);

        Assert.Equal(environment, change.Environment);
        Assert.Equal(deployable, change.Deployable);
        Assert.Equal(step, change.Step);
        Assert.Equal(environment?.Length ?? 0, change.Environment?.Length ?? 0);
        Assert.Equal(deployable?.Length ?? 0, change.Deployable?.Length ?? 0);

        // The very strings it was given, held as they stand; the nothing held as the nothing it is.
        if (environment is null)
        {
            Assert.Null(change.Environment);
        }
        else
        {
            Assert.Same(environment, change.Environment);
        }

        if (deployable is null)
        {
            Assert.Null(change.Deployable);
        }
        else
        {
            Assert.Same(deployable, change.Deployable);
        }

        // The nothing and the empty word are two different names on either of the two places, and neither is a word
        // of spaces.
        Assert.NotEqual(change, new RuntimeApplicationChange(environment is null ? string.Empty : string.Concat(environment, " "), deployable, step));
        Assert.NotEqual(change, new RuntimeApplicationChange(environment!, deployable is null ? string.Empty : string.Concat(deployable, " "), step));

        // The two places are two places: the same change spelled with its two names exchanged is another change,
        // unless the two names were one name to begin with.
        if (environment != deployable)
        {
            Assert.NotEqual(change, new RuntimeApplicationChange(deployable!, environment, step));
        }
    }

    /// <summary>A name of any length is held whole and said whole, on either of the two places. The address stops
    /// taking a deployable name longer than sixty-four letters - that is <see cref="ApplicationText.IsName" />, and
    /// it is pinned there - but the change cuts nothing: the sixty-four, the sixty-five and the ten thousand are
    /// three whole names, one letter fewer is another change told apart in every way of asking, and the same letters
    /// written again in another string are the same change.</summary>
    [Fact]
    public void ANameOfAnyLengthIsHeldWhole()
    {
        var sixtyFour = Own(Name(LongestName));
        var sixtyFive = Own(Name(LongestName + 1));
        var tenThousand = Own(new string('9', 10_000));

        Assert.Same(sixtyFour, new RuntimeApplicationChange(sixtyFour, sixtyFour, Step: true).Environment);
        Assert.Same(sixtyFive, new RuntimeApplicationChange(sixtyFive, sixtyFive, Step: true).Environment);
        Assert.Same(tenThousand, new RuntimeApplicationChange("uat", tenThousand, Step: false).Deployable);
        Assert.Equal(LongestName, new RuntimeApplicationChange(sixtyFour, "ui", Step: true).Environment.Length);
        Assert.Equal(LongestName + 1, new RuntimeApplicationChange(sixtyFive, "ui", Step: true).Environment.Length);
        Assert.Equal(10_000, new RuntimeApplicationChange("uat", tenThousand, Step: false).Deployable!.Length);

        // One letter fewer at either end of the bound is another change: held apart, compared apart and said apart,
        // with no prefix of the longer name standing in for the whole of it.
        var sixtyThree = Name(LongestName - 1);
        var atTheBound = new RuntimeApplicationChange(sixtyFour, "ui", Step: true);

        Assert.NotEqual(atTheBound, new RuntimeApplicationChange(sixtyThree, "ui", Step: true));
        Assert.NotEqual(atTheBound, new RuntimeApplicationChange(sixtyFive, "ui", Step: true));
        Assert.True(atTheBound != new RuntimeApplicationChange(sixtyThree, "ui", Step: true));
        Assert.Equal(
            3,
            new HashSet<RuntimeApplicationChange>
            {
                atTheBound,
                new RuntimeApplicationChange(sixtyThree, "ui", Step: true),
                new RuntimeApplicationChange(sixtyFive, "ui", Step: true),
            }.Count);
        Assert.DoesNotContain(Words(atTheBound), Words(new RuntimeApplicationChange(sixtyThree, "ui", Step: true)), StringComparison.Ordinal);

        // A long name in the deployable's place is a name as whole as one in the environment's, and the two places
        // are two places.
        Assert.Equal(10_000, new RuntimeApplicationChange(tenThousand, "ui", Step: true).Environment!.Length);
        Assert.NotEqual(new RuntimeApplicationChange(tenThousand, "ui", Step: true), new RuntimeApplicationChange("uat", tenThousand, Step: true));

        // The same letters gathered into another string: another reference, and one change.
        var writtenAgain = new RuntimeApplicationChange(Own(Name(LongestName)), "ui", Step: true);

        Assert.NotSame(atTheBound.Environment, writtenAgain.Environment);
        Assert.Equal(atTheBound, writtenAgain);
        Assert.Equal(atTheBound.GetHashCode(), writtenAgain.GetHashCode());
        Assert.Equal(Words(atTheBound), Words(writtenAgain));
        Assert.Single(new HashSet<RuntimeApplicationChange> { atTheBound, writtenAgain });
    }

    /// <summary>The record refuses nothing it is handed, and the nothing a change is handed for either of its names
    /// stands as that: the change is there, its flag is there, one word or both are not, and neither is the empty
    /// word either - which in turn is not a word of spaces. The nothing is hashed, compared, copied and said without
    /// a word of complaint, on either of the two places and on both of them at once.</summary>
    [Fact]
    public void AChangeWhoseNamesAreNotThereIsAChangeStill()
    {
        var noNames = Nothing;

        Assert.NotNull(noNames);
        Assert.Null(noNames.Environment);
        Assert.Null(noNames.Deployable);
        Assert.False(noNames.Step);

        // The nothing, the empty word and the word of spaces are three environments, and three deployables.
        Assert.NotEqual(noNames, Blank);
        Assert.NotEqual(Back, Blank);
        Assert.NotEqual(noNames, new RuntimeApplicationChange(" ", " ", Step: false));
        Assert.Equal(
            4,
            new HashSet<RuntimeApplicationChange>
            {
                noNames,
                Blank,
                new RuntimeApplicationChange(" ", "  ", Step: false),
                new RuntimeApplicationChange("uat", "ui", Step: false),
            }.Count);
        Assert.True(noNames != Blank);
        Assert.False(noNames == Blank);

        // Two changes whose names are both nothing are one change, in their flag as in their absence.
        Assert.Equal(noNames, new RuntimeApplicationChange(null!, null!, Step: false));
        Assert.True(noNames == new RuntimeApplicationChange(null!, null!, Step: false));
        Assert.Equal(noNames.GetHashCode(), new RuntimeApplicationChange(null!, null!, Step: false).GetHashCode());
        Assert.False(noNames.Equals(new RuntimeApplicationChange(null!, null!, Step: true)));

        // The back out and the correction are one pair of names and two changes: only the flag tells them apart.
        Assert.Equal(Back.Environment, Corrected.Environment);
        Assert.Equal(Back.Deployable, Corrected.Deployable);
        Assert.NotEqual(Back, Corrected);

        // A name of nothing on one place only is a third change, and it is the one the page sends when it leaves an
        // application view: nothing but the environment's name is there.
        Assert.NotEqual(noNames with { Environment = "uat" }, Back);
        Assert.Equal(Back, noNames with { Environment = "uat", Step = true });
        Assert.Equal(new RuntimeApplicationChange("uat", "ui", Step: false), noNames with { Environment = "uat", Deployable = "ui" });
        Assert.Equal(string.Empty, (noNames with { Deployable = string.Empty }).Deployable);
    }

    /// <summary>The flag holds either of its two answers and no third: the zoom and the back out are a step of the
    /// history, the correction of an address is not, and the two changes of the same two names are the two changes -
    /// apart in the equality, in a set and in the words the change says itself. The flag comes back out of the copy
    /// and the deconstruction as the flag it went in as, whatever the two names beside it are, and a name turned
    /// after the flag is turned leaves the flag where it was put.</summary>
    [Fact]
    public void TheFlagHoldsEitherOfItsTwoAnswersAndNoThird()
    {
        var stepped = Zoomed;
        var replaced = new RuntimeApplicationChange("uat", "ui", Step: false);

        Assert.True(stepped.Step);
        Assert.False(replaced.Step);
        Assert.Equal(stepped.Environment, replaced.Environment);
        Assert.Equal(stepped.Deployable, replaced.Deployable);
        Assert.NotEqual(stepped, replaced);
        Assert.True(stepped != replaced);
        Assert.False(stepped.Equals(replaced));
        Assert.Equal(2, new HashSet<RuntimeApplicationChange> { stepped, replaced }.Count);
        Assert.Equal("RuntimeApplicationChange { Environment = uat, Deployable = ui, Step = True }", Words(stepped));
        Assert.Equal("RuntimeApplicationChange { Environment = uat, Deployable = ui, Step = False }", Words(replaced));

        // The two answers the flag has are the two answers: turned in a copy, they are the two changes, and turning
        // it twice is the change it started from.
        Assert.Equal(replaced, stepped with { Step = false });
        Assert.Equal(stepped, replaced with { Step = true });
        Assert.Equal(stepped, stepped with { Step = true });
        Assert.Equal(stepped, stepped with { Step = false } with { Step = true });

        // The flag rides along with the names it stands beside: on the nothing of both, on the nothing of one, and
        // on the empty word.
        var (environment, deployable, flag) = replaced;

        Assert.Equal("uat", environment);
        Assert.Equal("ui", deployable);
        Assert.False(flag);
        Assert.False(Nothing.Step);
        Assert.False(Blank.Step);
        Assert.True(Zoomed.Step);
        Assert.True(Back.Step);
        Assert.False(Corrected.Step);
        Assert.Equal(Nothing with { Step = true }, new RuntimeApplicationChange(null!, null!, Step: true));
    }

    // ------------------------- the three members coming out again, and the copy that turns one

    /// <summary>The three members come back out in the order they went in - the environment first, the deployable in
    /// the middle and the step last - whether they are taken at once in a deconstruction or asked for one after the
    /// other, and what came out goes back in the same way: the change the three members fall apart into is the change
    /// they were. The nothing, the empty word and the word of spaces fall apart as they stand, turned to nothing by
    /// nobody, and a name never lands on another name's place.</summary>
    [Fact]
    public void TheChangeFallsApartIntoItsThreeMembersInTheOrderTheyWentIn()
    {
        var (environment, deployable, step) = Zoomed;

        Assert.Equal("uat", environment);
        Assert.Equal("ui", deployable);
        Assert.True(step);
        Assert.Equal(Zoomed, new RuntimeApplicationChange(environment, deployable, step));
        Assert.Equal(Zoomed, new RuntimeApplicationChange(Environment: environment, Deployable: deployable, Step: step));

        Zoomed.Deconstruct(out var again, out var againDeployable, out var againStep);

        Assert.Same(Zoomed.Environment, again);
        Assert.Same(Zoomed.Deployable, againDeployable);
        Assert.Equal(environment, again);
        Assert.Equal(deployable, againDeployable);
        Assert.Equal(step, againStep);

        var (backEnvironment, backDeployable, backStep) = Back;

        Assert.Equal("uat", backEnvironment);
        Assert.Null(backDeployable);
        Assert.True(backStep);
        Assert.Equal(Back, new RuntimeApplicationChange(backEnvironment, backDeployable, backStep));
        Assert.NotEqual(Back, new RuntimeApplicationChange(backEnvironment, string.Empty, backStep));

        // The three places are three places: the environment's name never turns up in the deployable's, and a name
        // that is not there comes out as the nothing and not as the empty word.
        var (noEnvironment, noDeployable, noStep) = Nothing;

        Assert.Null(noEnvironment);
        Assert.Null(noDeployable);
        Assert.False(noStep);
        Assert.Equal(Nothing, new RuntimeApplicationChange(noEnvironment!, noDeployable, noStep));
        Assert.NotEqual(new RuntimeApplicationChange(noEnvironment!, noDeployable, noStep), new RuntimeApplicationChange(string.Empty, string.Empty, noStep));

        var (blankEnvironment, blankDeployable, blankStep) = new RuntimeApplicationChange("  ", " ", Step: true);

        Assert.Equal("  ", blankEnvironment);
        Assert.Equal(" ", blankDeployable);
        Assert.True(blankStep);
        Assert.NotEqual(blankEnvironment, blankDeployable);
        Assert.NotEqual(string.Empty, blankEnvironment);
        Assert.NotEqual(new RuntimeApplicationChange(blankEnvironment, blankDeployable, blankStep), new RuntimeApplicationChange(string.Empty, string.Empty, blankStep));

        // The same three members read out one after the other are the three the deconstruction gives.
        Assert.Equal((environment, deployable, step), (Zoomed.Environment, Zoomed.Deployable, Zoomed.Step));
    }

    /// <summary>Any one of the three members may be turned as the copy is made, and the copy that names nothing is
    /// the change it started from in another reference: the members that are not named keep the very words and the
    /// very flag they stood at, the original is left where it stood, a name turned to nothing is the nothing and not
    /// the empty word, and the record's own constructor is what the copy goes through - so a copy holds a name as
    /// exactly as the page's own change does.</summary>
    [Fact]
    public void ACopyTurnsTheMemberItNamesAndKeepsTheOthers()
    {
        var original = Zoomed;

        var unchanged = original with { };

        Assert.Equal(original, unchanged);
        Assert.NotSame(original, unchanged);
        Assert.Equal(Words(original), Words(unchanged));
        Assert.Equal(original.GetHashCode(), unchanged.GetHashCode());
        Assert.Same(original.Environment, unchanged.Environment);

        var environmentTurned = original with { Environment = "prod" };

        Assert.Equal("prod", environmentTurned.Environment);
        Assert.Equal("ui", environmentTurned.Deployable);
        Assert.True(environmentTurned.Step);
        Assert.Equal("uat", original.Environment);
        Assert.NotEqual(original, environmentTurned);

        var deployableTurned = original with { Deployable = "api" };

        Assert.Equal(new RuntimeApplicationChange("uat", "api", Step: true), deployableTurned);
        Assert.Equal("uat", deployableTurned.Environment);
        Assert.Equal(Zoomed, deployableTurned with { Deployable = "ui" });

        var stepTurned = original with { Step = false };

        Assert.Equal(new RuntimeApplicationChange("uat", "ui", Step: false), stepTurned);
        Assert.Equal("uat", stepTurned.Environment);
        Assert.Equal("ui", stepTurned.Deployable);
        Assert.NotEqual(Corrected, stepTurned);

        var allTurned = original with { Environment = "tdd", Deployable = null, Step = false };

        Assert.Equal(new RuntimeApplicationChange("tdd", null, Step: false), allTurned);
        Assert.Equal(new RuntimeApplicationChange("uat", "ui", Step: true), original);

        // A name turned to nothing is the nothing, and the nothing is not the empty word.
        var toNothing = original with { Deployable = null };

        Assert.Null(toNothing.Deployable);
        Assert.Equal("uat", toNothing.Environment);
        Assert.True(toNothing.Step);
        Assert.Equal(Back, toNothing);
        Assert.NotEqual(toNothing, original with { Deployable = string.Empty });
        Assert.Equal(new RuntimeApplicationChange(null!, null!, Step: true), toNothing with { Environment = null!, Step = true });

        // A name turned to the name it already holds is the same change in another reference - and the name of a
        // literal is the one word those letters are written with.
        var sameNames = original with { Environment = "uat", Deployable = "ui" };

        Assert.Equal(original, sameNames);
        Assert.NotSame(original, sameNames);
        Assert.Same(original.Environment, sameNames.Environment);
        Assert.Same(original.Deployable, sameNames.Deployable);
    }

    // -------------------------------------------------------------- the equality and the hash

    /// <summary>A change is one with another while its three members are one, whichever of the four ways of asking is
    /// used: the record's own <c>Equals</c>, the one of <see cref="object" />, <c>==</c> and <c>!=</c>. The hash
    /// follows the equality, boxed or unboxed and from one call to the next, and the letters are compared rather than
    /// the references - so the change one component makes is the change another makes, one entry of a set and one key
    /// of a dictionary, and the two ways of naming the same members are one change.</summary>
    [Fact]
    public void TwoChangesAreOneWhileTheirThreeMembersAre()
    {
        var one = Zoomed;
        var two = new RuntimeApplicationChange(Own("uat"), Own("ui"), Step: true);

        Assert.True(one.Equals(two));
        Assert.True(one.Equals(one));
        Assert.True(one.Equals((object)two));
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.True(one == two with { });
        Assert.False(one != two with { Step = true });
        Assert.Equal(one, two);
        Assert.NotSame(one, two);
        Assert.NotSame(one.Environment, two.Environment);
        Assert.NotSame(one.Deployable, two.Deployable);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Single(new HashSet<RuntimeApplicationChange> { one, two });

        // Named in either order, in either of the two spellings of the same letters: one change.
        Assert.Equal(one, new RuntimeApplicationChange(Deployable: "ui", Environment: "uat", Step: true));
        Assert.Equal(one, new RuntimeApplicationChange(Own("uat"), Own("ui"), Step: true));

        // The same letters, spelled again: another reference, and one key.
        var shown = new Dictionary<RuntimeApplicationChange, string> { [one] = "the assemblies of ui" };

        Assert.Equal("the assemblies of ui", shown[two]);
        Assert.Equal("the assemblies of ui", shown[one with { }]);
        Assert.Single(shown);

        var otherStep = two with { Step = false };

        Assert.False(shown.ContainsKey(otherStep));

        // The empty word, the nothing and the back out, spelled twice each, are three changes and no more.
        Assert.Equal(Blank, new RuntimeApplicationChange(Own(string.Empty), Own(string.Empty), Step: false));
        Assert.Single(new HashSet<RuntimeApplicationChange> { Blank, new RuntimeApplicationChange(string.Empty, string.Empty, Step: false) });
        Assert.Single(new HashSet<RuntimeApplicationChange> { Nothing, new RuntimeApplicationChange(null!, null!, Step: false) });
        Assert.Single(new HashSet<RuntimeApplicationChange> { Back, new RuntimeApplicationChange("uat", null, Step: true) });
    }

    /// <summary>A difference in either member is another change: a letter turned in the environment's name, a letter
    /// turned in the deployable's, the two names exchanged, a letter added at either end of either, a letter dropped,
    /// the whitespace at either end, the case, the empty word for the nothing and the nothing for the empty word -
    /// and the flag turned while both names stand where they did. The comparison is of the letters one after
    /// another, so neither a trimmed name nor a name of another case is the name it stands beside, and the three
    /// members are three places: a change is told from another by either of them alone.</summary>
    [Fact]
    public void AChangeTurnedInEitherMemberIsAnotherChange()
    {
        var one = Zoomed;

        Assert.NotEqual(one, new RuntimeApplicationChange("prod", "ui", Step: true));
        Assert.NotEqual(one, new RuntimeApplicationChange("uat", "api", Step: true));
        Assert.NotEqual(one, new RuntimeApplicationChange("ui", "uat", Step: true));
        Assert.NotEqual(one, new RuntimeApplicationChange("uat", "ui", Step: false));
        Assert.NotEqual(one, new RuntimeApplicationChange("uatx", "ui", Step: true));
        Assert.NotEqual(one, new RuntimeApplicationChange("ua", "ui", Step: true));
        Assert.NotEqual(one, new RuntimeApplicationChange(" uat", "ui", Step: true));
        Assert.NotEqual(one, new RuntimeApplicationChange("uat ", "ui", Step: true));
        Assert.NotEqual(one, new RuntimeApplicationChange("uat", " ui", Step: true));
        Assert.NotEqual(one, new RuntimeApplicationChange("uat", "ui ", Step: true));
        Assert.NotEqual(one, new RuntimeApplicationChange("uat", "ui\t", Step: true));
        Assert.NotEqual(one, new RuntimeApplicationChange("uat", "Ui", Step: true));
        Assert.NotEqual(one, new RuntimeApplicationChange("Uat", "ui", Step: true));
        Assert.NotEqual(one, new RuntimeApplicationChange("uat", "ui-server", Step: true));
        Assert.NotEqual(one, new RuntimeApplicationChange("uat", "ui_server", Step: true));
        Assert.NotEqual(one, new RuntimeApplicationChange("uat", string.Empty, Step: true));
        Assert.NotEqual(one, new RuntimeApplicationChange(string.Empty, "ui", Step: true));
        Assert.NotEqual(one, new RuntimeApplicationChange("uat", null, Step: true));
        Assert.NotEqual(one, new RuntimeApplicationChange(null!, "ui", Step: true));

        Assert.True(one != new RuntimeApplicationChange("prod", "ui", Step: true));
        Assert.True(one != new RuntimeApplicationChange("uat", "ui", Step: false));
        Assert.False(one == new RuntimeApplicationChange("uat", "ui", Step: false));
        Assert.False(one.Equals((object)new RuntimeApplicationChange("uat", null, Step: true)));
        Assert.False(one.Equals(one with { Deployable = null }));
        Assert.False(one.Equals(one with { Environment = "prod" }));
        Assert.False(one.Equals(one with { Step = false }));

        // Eight settings of the three members, and eight changes apart.
        Assert.Equal(
            8,
            new HashSet<RuntimeApplicationChange>
            {
                one,
                one with { Environment = "prod" },
                one with { Deployable = "api" },
                one with { Step = false },
                one with { Deployable = string.Empty },
                one with { Deployable = null },
                one with { Environment = string.Empty },
                one with { Environment = "uat " },
            }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two changes that are no change
    /// at all are one, a change and no change are two - the equality and the inequality each saying it, from either
    /// side - and the change asked about itself says no to the nothing, to the two words it was made of, to the
    /// change of another record that happens to carry the same environment and the same flag, and to the anonymous
    /// thing that carries all three.</summary>
    [Fact]
    public void NoChangeIsOneWithNoChangeAndNotWithAnyOtherThing()
    {
        RuntimeApplicationChange? nothing = null;
        RuntimeApplicationChange? alsoNothing = null;
        var there = Zoomed;

        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == nothing);
        Assert.True(there != nothing);
        Assert.True(there!.Equals(there));
        Assert.Null(default(RuntimeApplicationChange));

        Assert.False(Zoomed.Equals(null));
        Assert.False(Zoomed.Equals((RuntimeApplicationChange?)null));
        Assert.False(Zoomed.Equals((object?)null));
        Assert.False(Zoomed.Equals("uat"));
        Assert.False(Zoomed.Equals("ui"));
        Assert.False(Zoomed.Equals(true));
        Assert.False(Zoomed.Equals(new RuntimeDatabaseChange("uat", Shown: false, Step: true)));
        Assert.False(Zoomed.Equals(new RuntimeDatabaseChange("uat", Shown: true, Step: true)));
        Assert.False(Zoomed.Equals(new ApplicationVersion("uat", Runs: false)));
        Assert.False(Zoomed.Equals(new ViewAddress(DashboardView.Runtime, "uat", Application: "ui")));
        Assert.False(Zoomed.Equals(new { Environment = "uat", Deployable = "ui", Step = true }));
        Assert.False(Zoomed.Equals(Back));
        Assert.False(Zoomed.Equals(Corrected));
        Assert.False(Zoomed.Equals(Blank));
        Assert.False(Zoomed.Equals(Nothing));
    }

    /// <summary>The hash follows the equality at the ends of what a change holds: two changes that are one hash one,
    /// however they were written, boxed or unboxed, twice in a row and whichever reader asks - and the nothing of a
    /// name that is not there is hashed as well as any name, one with another nothing and apart from the empty word.
    /// What a change hashes to is no promise about which changes share it, so it is followed here rather than written
    /// out: the numbers a run gives are not the numbers of the next.</summary>
    [Fact]
    public void TheHashFollowsTheEqualityAndNeitherTheNothingNameNorTheReaderTurnsIt()
    {
        var one = Zoomed;
        var two = new RuntimeApplicationChange(Own("uat"), Own("ui"), Step: true);

        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), (one with { }).GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(two.GetHashCode(), two.GetHashCode());
        Assert.Equal(Blank.GetHashCode(), new RuntimeApplicationChange(Own(string.Empty), Own(string.Empty), Step: false).GetHashCode());
        Assert.Equal(Nothing.GetHashCode(), new RuntimeApplicationChange(null!, null!, Step: false).GetHashCode());
        Assert.Equal(Nothing.GetHashCode(), Nothing.GetHashCode());
        Assert.Equal(Back.GetHashCode(), new RuntimeApplicationChange("uat", null, Step: true).GetHashCode());

        // The nothing hashes without a complaint, and with the name it is not: two nothings are one entry, and the
        // empty word beside them is another.
        Assert.Single(new HashSet<RuntimeApplicationChange> { Nothing, new RuntimeApplicationChange(null!, null!, Step: false) });
        Assert.Equal(2, new HashSet<RuntimeApplicationChange> { Nothing, Blank }.Count);
        Assert.Equal(2, new HashSet<RuntimeApplicationChange> { Back, Blank }.Count);
        Assert.Equal(2, new HashSet<RuntimeApplicationChange> { one, one with { Step = false } }.Count);
        Assert.Equal(2, new HashSet<RuntimeApplicationChange> { one, one with { Environment = null! } }.Count);

        // A name is one key's whole story: the same letters in another reference find the entry, and a name turned
        // does not.
        var keys = new Dictionary<RuntimeApplicationChange, int>();

        keys[one] = 1;
        keys[two] = 2;

        Assert.Single(keys);
        Assert.Equal(2, keys[new RuntimeApplicationChange(Own("uat"), Own("ui"), Step: true)]);

        // The reader of German hashes the changes the invariant reader hashes, and keys a dictionary the same way.
        ViewerCulture.German(() =>
        {
            Assert.Equal(one.GetHashCode(), two.GetHashCode());
            Assert.Equal(
                "the assemblies of ui",
                new Dictionary<RuntimeApplicationChange, string> { [one] = "the assemblies of ui" }[two]);
        });
    }

    // ---------------------------------------------------------- the words the change says itself

    /// <summary>The words a change says its three members in: its own name at the front, the environment, the
    /// deployable and the step as they are named, each said as it stands and none of them quoted - the nothing of a
    /// name that is not there and the empty word both leaving the same empty place between the mark and the comma,
    /// the whitespace of a name standing in the middle of it, the flag in the record's own letters of <c>True</c> and
    /// <c>False</c>, and a name of any length said whole. These are the record's words and not the page's: the page
    /// says an address <c>runtime/uat/app/ui</c>, and no line of it comes from here.</summary>
    [Fact]
    public void TheChangeSaysItsThreeMembersInTheOrderTheyAreNamed()
    {
        Assert.Equal("RuntimeApplicationChange { Environment = uat, Deployable = ui, Step = True }", Words(Zoomed));
        Assert.Equal("RuntimeApplicationChange { Environment = uat, Deployable = , Step = True }", Words(Back));
        Assert.Equal("RuntimeApplicationChange { Environment = uat, Deployable = , Step = False }", Words(Corrected));
        Assert.Equal("RuntimeApplicationChange { Environment = , Deployable = , Step = False }", Words(Blank));
        Assert.Equal("RuntimeApplicationChange { Environment = , Deployable = , Step = False }", Words(Nothing));
        Assert.Equal("RuntimeApplicationChange { Environment =   uat  , Deployable =  ui, Step = True }", Words(new RuntimeApplicationChange("  uat  ", " ui", Step: true)));
        Assert.Equal("RuntimeApplicationChange { Environment = \tuat, Deployable = ui\t, Step = False }", Words(new RuntimeApplicationChange("\tuat", "ui\t", Step: false)));
        Assert.Equal("RuntimeApplicationChange { Environment = UAT, Deployable = UI, Step = True }", Words(new RuntimeApplicationChange("UAT", "UI", Step: true)));
        Assert.Equal("RuntimeApplicationChange { Environment = 0, Deployable = 1, Step = False }", Words(new RuntimeApplicationChange("0", "1", Step: false)));
        Assert.Equal("RuntimeApplicationChange { Environment = pröd, Deployable = уи, Step = True }", Words(new RuntimeApplicationChange("pröd", "уи", Step: true)));
        Assert.Equal($"RuntimeApplicationChange {{ Environment = ui, Deployable = {Name(LongestName + 1)}, Step = False }}", Words(new RuntimeApplicationChange("ui", Name(LongestName + 1), Step: false)));

        // The nothing of a name and the empty word leave the same empty place: only the equality tells them apart.
        Assert.Equal(Words(Nothing), Words(Blank));
        Assert.NotEqual(Nothing, Blank);
        Assert.Equal(Words(Back), Words(new RuntimeApplicationChange("uat", string.Empty, Step: true)));
        Assert.NotEqual(Back, new RuntimeApplicationChange("uat", string.Empty, Step: true));

        // The three members are said and nothing else: no line of the page's, no quoting of a name, no word of what
        // the flag means, and the two flags said apart.
        Assert.DoesNotContain("runtime/uat", Words(Zoomed), StringComparison.Ordinal);
        Assert.DoesNotContain("assemblies", Words(Zoomed), StringComparison.Ordinal);
        Assert.DoesNotContain("as it runs", Words(Zoomed), StringComparison.Ordinal);
        Assert.DoesNotContain("'", Words(Zoomed), StringComparison.Ordinal);
        Assert.StartsWith("RuntimeApplicationChange { Environment = ", Words(Zoomed), StringComparison.Ordinal);
        Assert.NotEqual(Words(Zoomed), Words(Zoomed with { Step = false }));
        Assert.NotEqual(Words(Zoomed), Words(Zoomed with { Deployable = null }));
    }

    /// <summary>The three members are two names and a flag, so no reader's culture turns them, the words the change
    /// says, the equality that follows the letters or the hash that follows the equality: a change a German reader
    /// makes is the change an invariant reader makes, said in the invariant record's letters with the same
    /// <c>True</c> and <c>False</c> the English record writes.</summary>
    [Fact]
    public void TheWordsAreTheSameWhateverTheReaderSpeaks()
    {
        Assert.Equal("RuntimeApplicationChange { Environment = uat, Deployable = ui, Step = True }", Words(Zoomed));

        ViewerCulture.German(() =>
        {
            Assert.Equal("RuntimeApplicationChange { Environment = uat, Deployable = ui, Step = True }", Zoomed.ToString());
            Assert.Equal("RuntimeApplicationChange { Environment = uat, Deployable = , Step = False }", Corrected.ToString());
            Assert.Equal("RuntimeApplicationChange { Environment = , Deployable = , Step = False }", Nothing.ToString());
            var germanStepTurned = Zoomed with { Step = false };

            Assert.Equal("RuntimeApplicationChange { Environment = uat, Deployable = ui, Step = False }", germanStepTurned.ToString());

            // A change made under the German reader is the change made under any reader.
            var german = new RuntimeApplicationChange(Own("uat"), Own("ui"), Step: true);

            Assert.Equal(Zoomed, german);
            Assert.True(Zoomed == german);
            Assert.Equal(Zoomed.GetHashCode(), german.GetHashCode());
            Assert.Equal(Words(Zoomed), german.ToString());
        });

        Assert.Equal(Words(Zoomed), Words(new RuntimeApplicationChange(Own("uat"), Own("ui"), Step: true)));
    }

    // ------------------------ what the change decides about the names it holds, which is nothing

    /// <summary>The change asks nothing of either name it is handed: it refuses no name and accepts no name for what
    /// it names. The environment's name is not asked whether it is one of the topology's, the deployable's name is
    /// not asked whether the address takes it - the empty word, the word of spaces, a name of sixty-five letters, a
    /// name of points, a slash, a space or angle brackets are all held whole, untrimmed and uncased - and nothing is
    /// thrown on the way. What a name is worth is decided by the classes that read a change afterwards, and it is
    /// pinned with them: <see cref="ApplicationText.IsName" /> and the address and the boxes it governs, in
    /// <see cref="ApplicationViewTests" /> and <see cref="ViewAddressTests" />. What is said here is only that a name
    /// those classes refuse reaches them whole and is still held afterwards - the day the record grows a check of its
    /// own, this test says so.</summary>
    [Fact]
    public void TheChangeNeitherRefusesANameNorTrimsOneItIsGiven()
    {
        foreach (var name in new[] { string.Empty, " ", "ui ", " UI", "ui.server", "ui/x", "ui x", "..", "%20", "<script>alert(1)<script>", Name(LongestName + 1), new string('9', 10_000) })
        {
            var change = new RuntimeApplicationChange(name, name, Step: true);

            Assert.Same(name, change.Environment);
            Assert.Same(name, change.Deployable);
            Assert.Equal(name, change.Environment);
            Assert.Equal(name.Length, change.Deployable!.Length);
            Assert.True(change.Step);
        }

        // The names the address refuses are held as they came, so the caller can tell the reader what it was asked
        // for; the record's part ends at the holding. The two words beside the fixture below are the witness that it
        // is a refused name that is held and not a taken one - the rule they lean on is pinned with IsName.
        var hostile = new RuntimeApplicationChange("uat", "<script>alert(1)<script>", Step: true);

        Assert.Equal("<script>alert(1)<script>", hostile.Deployable);
        Assert.True(ApplicationText.IsName(hostile.Environment));
        Assert.False(ApplicationText.IsName(hostile.Deployable));

        // No name of any shape is refused on the way in: the record throws at nothing, and neither the nothing nor a
        // name of ten thousand letters is an argument it turns down.
        Assert.NotNull(new RuntimeApplicationChange(null!, null!, Step: false));
        Assert.NotNull(new RuntimeApplicationChange(new string('a', 10_000), new string('b', 10_000), Step: false));

        // And the change that carried a refused name is not the change that carried nothing, whatever the page makes
        // of either: the record tells the two apart in every way of asking.
        Assert.NotEqual(hostile, Back);
        Assert.True(hostile != Back);
        Assert.Equal(2, new HashSet<RuntimeApplicationChange> { hostile, Back }.Count);
    }

    // -------------------------------------------------------------- the change's own shape

    /// <summary>The whole public surface of a change, as it stands: the three members in the order the constructor
    /// names them - the environment, the deployable and the step - all three of them written once, at the making and
    /// never after; the three arguments the constructor takes, all three of them required; and the members every
    /// record is asked, falling apart, comparing, hashing and saying itself, and nothing else to call. The name an
    /// address takes, the boxes that lead into the view and the version it shows are the decisions of the classes
    /// they belong to, pinned there: a change decides nothing. A fourth member added to the record has to be written
    /// here as well, which is the point: the diagram cannot be sent a member nobody decided what it means.</summary>
    [Fact]
    public void TheChangeHasThreeMembersAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Environment", "Deployable", "Step"],
            [.. typeof(RuntimeApplicationChange)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["Deconstruct", "Deployable", "Environment", "Equals", "GetHashCode", "Step", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());

        var properties = typeof(RuntimeApplicationChange)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(string), properties["Environment"].PropertyType);
        Assert.Equal(typeof(string), properties["Deployable"].PropertyType);
        Assert.Equal(typeof(bool), properties["Step"].PropertyType);
        Assert.All(properties.Values, property => Assert.True(property.CanRead));
        Assert.All(properties.Values, property => Assert.True(property.CanWrite));

        // All three members are written at the making and closed: the change the diagram sent is the change the page
        // keeps with it until another change is made.
        Assert.All(properties.Values, property => Assert.True(IsWrittenOnlyAtTheMaking(property), $"{property.Name} may be turned after the change was made"));

        var nullability = new NullabilityInfoContext();
        var declared = new[] { properties["Environment"], properties["Deployable"], properties["Step"] };

        // The environment is a name the caller must supply and the deployable is one it may leave out - the only two
        // places of the record where the nothing is written into the type as well as into the test.
        Assert.Equal(
            [false, true, false],
            [.. declared.Select(property => nullability.Create(property).WriteState == NullabilityState.Nullable)]);
        Assert.Equal(
            [false, true, false],
            [.. declared.Select(property => nullability.Create(property).ReadState == NullabilityState.Nullable)]);

        // Three arguments, in the members' own order and types, and none of them optional.
        var parameters = typeof(RuntimeApplicationChange).GetConstructors().Single().GetParameters();

        Assert.Equal(["Environment", "Deployable", "Step"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal([typeof(string), typeof(string), typeof(bool)], [.. parameters.Select(parameter => parameter.ParameterType)]);
        Assert.All(parameters, parameter => Assert.False(parameter.IsOptional));
        Assert.All(parameters, parameter => Assert.False(parameter.HasDefaultValue));

        // The three places come out as the three places they went in, in the members' own types.
        var deconstruct = typeof(RuntimeApplicationChange).GetMethod("Deconstruct")!;

        Assert.True(deconstruct.IsPublic);
        Assert.Equal(
            [typeof(string).MakeByRefType(), typeof(string).MakeByRefType(), typeof(bool).MakeByRefType()],
            [.. deconstruct.GetParameters().Select(parameter => parameter.ParameterType)]);
        Assert.Equal(["Environment", "Deployable", "Step"], [.. deconstruct.GetParameters().Select(parameter => parameter.Name!)]);
        Assert.All(deconstruct.GetParameters(), parameter => Assert.True(parameter.IsOut));

        // The three ways of asking whether two changes are one: the record's own, the one every object is asked, and
        // the two operators that say the same in either direction.
        Assert.Equal(
            ["Object", "RuntimeApplicationChange"],
            [.. typeof(RuntimeApplicationChange)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => method.Name.Equals("Equals", StringComparison.Ordinal))
                .Select(method => method.GetParameters()[0].ParameterType.Name)
                .Order(StringComparer.Ordinal)]);

        Assert.Equal(
            ["op_Equality", "op_Inequality"],
            [.. typeof(RuntimeApplicationChange)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(method => method.Name.StartsWith("op_", StringComparison.Ordinal))
                .Select(method => method.Name)
                .Order(StringComparer.Ordinal)]);
        Assert.All(
            [.. typeof(RuntimeApplicationChange).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(method => method.Name.StartsWith("op_", StringComparison.Ordinal))],
            method => Assert.Equal(2, method.GetParameters().Length));

        // A change is a reference that says what it is equal to, and no more of it may be written about.
        Assert.True(typeof(RuntimeApplicationChange).IsSealed);
        Assert.False(typeof(RuntimeApplicationChange).IsValueType);
        Assert.Equal(typeof(object), typeof(RuntimeApplicationChange).BaseType);
        Assert.Equal([typeof(IEquatable<RuntimeApplicationChange>)], [.. typeof(RuntimeApplicationChange).GetInterfaces()]);
    }

    // ---------------------------------------------------------------------------------- the machinery

    /// <summary>The public members the change declares itself: its three properties and the methods that are not the
    /// get and the set of one - the two operators kept although the compiler names them specially, since they are
    /// the <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone and its printed
    /// members walked past. The constructor is no method, so it is named by the tests above rather than by this
    /// list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(RuntimeApplicationChange)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>Whether a member may be written only while the change is being made: the record's init-only
    /// accessor, which the compiler marks with the modreq that a <c>with</c> carries and no assignment does.</summary>
    private static bool IsWrittenOnlyAtTheMaking(PropertyInfo property) =>
        property.SetMethod is { } setter
        && setter.ReturnParameter.GetRequiredCustomModifiers().Any(modifier =>
            modifier.FullName is { } name && name.Equals("System.Runtime.CompilerServices.IsExternalInit", StringComparison.Ordinal));

    /// <summary>The words a change says its three members in, taken in the invariant culture whatever the reader of
    /// this test happens to be: the point below is the words, and the reader's own culture is pinned apart.</summary>
    private static string Words(RuntimeApplicationChange change)
    {
        ArgumentNullException.ThrowIfNull(change);

        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            return change.ToString()!;
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }

    /// <summary>A deployable's name of the given number of letters, the <c>a</c> the address takes and the
    /// <c>7</c> it does not care about: the boundary seen from the side of the change that holds the word.</summary>
    private static string Name(int letters) => new('a', letters);

    /// <summary>The letters of a word gathered into a string of its own, which no other word in the test shares a
    /// reference with: what the change does with a word it is given - keeping the one and not another that spells the
    /// same - is only seen when the two are apart.</summary>
    private static string Own(string word) => new(word.ToCharArray());
}
