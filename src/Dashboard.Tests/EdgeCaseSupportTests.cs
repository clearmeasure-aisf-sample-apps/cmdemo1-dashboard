using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The shared edge-case helper <see cref="EdgeCaseSupport.Own" />, and the agreement of the copies the edge-case
/// files still carry with the shared answers: every private <c>Own</c> says what <see cref="EdgeCaseSupport.Own" />
/// says, and every private culture harness - the <c>Reader</c>, <c>Under</c> and <c>AsAReaderOf</c> of the files
/// that ask a record for its parts or its words under another reader - says what <see cref="CultureScope.Says" />
/// says and hands the thread's own culture back afterwards, whatever the asking did. A copy held here may be
/// retired without changing a test's expectation.
/// </summary>
public class EdgeCaseSupportTests
{
    private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Static;

    // ------------------------------- the shared word of its own

    /// <summary>The letters a word is given come back exactly: the empty word, the word of spaces, the word with
    /// its whitespace about it, a word in another case, the marks a version is spelled with and a letter outside
    /// the ASCII.</summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  1.0.42  ")]
    [InlineData("1.0.42")]
    [InlineData("1.0.42-beta.1")]
    [InlineData("Ui.Server")]
    [InlineData("1.0.42-世界")]
    public void OwnGivesBackTheVeryLettersItWasGiven(string word) =>
        Assert.Equal(word, EdgeCaseSupport.Own(word));

    /// <summary>A word of its own is no other word: the reference the test handed over is not the reference the
    /// helper hands back, and two calls of the same word hand back two references apart, so a record that keeps
    /// the word it was given is seen keeping one and not another that spells the same.</summary>
    [Fact]
    public void OwnGivesBackAReferenceNoOtherWordShares()
    {
        var word = "1.0.42";

        var own = EdgeCaseSupport.Own(word);
        var again = EdgeCaseSupport.Own(word);

        Assert.NotSame(word, own);
        Assert.NotSame(own, again);
        Assert.Equal(own, again);
    }

    /// <summary>The empty word comes back as the empty string: the length is what the test pins, since the empty
    /// word is the interned one whatever the helper does with it.</summary>
    [Fact]
    public void OwnOfTheEmptyWordIsTheEmptyWord()
    {
        var own = EdgeCaseSupport.Own(string.Empty);

        Assert.Equal(string.Empty, own);
        Assert.Equal(0, own.Length);
    }

    /// <summary>A word that is not there at all is refused where it stands: the helper names the absent word
    /// rather than answering it with a throw of the reference.</summary>
    [Fact]
    public void AnAbsentWordIsRefused() =>
        Assert.Throws<ArgumentNullException>(() => EdgeCaseSupport.Own(null!));

    // ------------------------------- the private copies of the word helper, held to the shared one

    public static TheoryData<Type> OwnCopies =>
    [
        typeof(ApplicationVersionEdgeCaseTests),
        typeof(AvailabilityPartEdgeCaseTests),
        typeof(CostPartEdgeCaseTests),
        typeof(DashboardInfoEdgeCaseTests),
        typeof(ResourceHealthReadingContractEdgeCaseTests),
        typeof(ResourceHealthReadingEdgeCaseTests),
        typeof(ResourceHealthWordsEdgeCaseTests),
        typeof(WorkItemEdgeCaseTests),
        typeof(WorkItemsScopeEdgeCaseTests),
        typeof(WorkScopeLineEdgeCaseTests),
    ];

    /// <summary>Every file that still carries its own private <c>Own</c> says what the shared one says - the same
    /// letters for the same word, and a reference apart from the word the test handed over wherever the word is
    /// not the empty one - so the copy may be retired without changing a test's expectation.</summary>
    [Theory]
    [MemberData(nameof(OwnCopies))]
    public void EachFilesPrivateCopyOfOwnSaysWhatTheSharedOneSays(Type tests)
    {
        var copy = tests.GetMethod("Own", Private, new[] { typeof(string) });
        Assert.NotNull(copy);

        foreach (var word in new[] { "1.0.42", "", " ", "  1.0.42  ", "Ui.Server", "1.0.42-世界" })
        {
            var answered = Assert.IsType<string>(copy!.Invoke(null, [word]));

            Assert.Equal(EdgeCaseSupport.Own(word), answered);
            if (word.Length > 0)
            {
                Assert.NotSame(word, answered);
            }
        }
    }

    // ------------------------------- the private culture harnesses, held to the shared one

    /// <summary>Every private harness that takes a culture says what <see cref="CultureScope.Says" /> says when
    /// both are asked the same question as a reader of that culture, and hands the thread's own culture back
    /// afterwards - after an answer, and after the asking throws. The harnesses: <c>Reader</c> in the two work
    /// files, <c>Under</c> in the two resource-health files.</summary>
    [Theory]
    [InlineData(typeof(WorkItemsScopeEdgeCaseTests), "Reader")]
    [InlineData(typeof(WorkScopeLineEdgeCaseTests), "Reader")]
    [InlineData(typeof(ResourceHealthReadingEdgeCaseTests), "Under")]
    [InlineData(typeof(ResourceHealthWordsEdgeCaseTests), "Under")]
    public void EachPrivateCultureInstanceHarnessSaysWhatTheSharedScopeSays(Type tests, string harness)
    {
        var copy = tests.GetMethod(harness, Private)!;
        copy = copy.MakeGenericMethod(typeof(string));

        var reader = Thread.CurrentThread.CurrentCulture;
        var german = CultureInfo.GetCultureInfo("de-DE");
        Func<string> what = () => CultureInfo.CurrentCulture.Name;
        Func<string> throwing = Throwing;

        Assert.Equal(CultureScope.Says(german, what), (string)copy.Invoke(null, [german, what])!);
        Assert.Same(reader, Thread.CurrentThread.CurrentCulture);

        Assert.Throws<TargetInvocationException>(() => copy.Invoke(null, [german, throwing]));
        Assert.Same(reader, Thread.CurrentThread.CurrentCulture);
    }

    /// <summary>Every private harness that takes a language name says what <see cref="CultureScope.Says" /> says
    /// for that name, and hands the thread's own culture back afterwards, whatever the asking did. The harnesses:
    /// <c>AsAReaderOf</c> in the two text files.</summary>
    [Theory]
    [InlineData(typeof(ResourceHealthTextEdgeCaseTests), "AsAReaderOf")]
    [InlineData(typeof(UnplacedTextEdgeCaseTests), "AsAReaderOf")]
    public void EachPrivateCultureNameHarnessSaysWhatTheSharedScopeSays(Type tests, string harness)
    {
        var copy = tests.GetMethod(harness, Private)!;
        copy = copy.MakeGenericMethod(typeof(string));

        var reader = Thread.CurrentThread.CurrentCulture;
        Func<string> what = () => CultureInfo.CurrentCulture.Name;
        Func<string> throwing = Throwing;

        Assert.Equal(CultureScope.Says("de-DE", what), (string)copy.Invoke(null, ["de-DE", what])!);
        Assert.Same(reader, Thread.CurrentThread.CurrentCulture);

        Assert.Throws<TargetInvocationException>(() => copy.Invoke(null, ["fr-FR", throwing]));
        Assert.Same(reader, Thread.CurrentThread.CurrentCulture);
    }

    private static string Throwing() => throw new InvalidOperationException("The asking that is meant to throw.");
}
