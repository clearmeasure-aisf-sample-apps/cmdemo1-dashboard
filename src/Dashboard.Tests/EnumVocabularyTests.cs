using Xunit.Sdk;

namespace Dashboard.Tests;

/// <summary>
/// The tests of <see cref="EnumVocabulary{T}" />, the BCL half of an enum's pin written once: that it passes for an
/// enum that keeps the house's claim, and - just as much - that it fails, naming the enum, for one that does not, so
/// that a pin handed to it can never pass by saying nothing.
/// <para>
/// The enums the helper is tried against are declared here, each one breaking exactly one part of the claim; the
/// house's own enums are run through it in <see cref="EnumVocabularyHouseEnumsTests" />.
/// </para>
/// </summary>
public class EnumVocabularyTests
{
    private enum Three
    {
        First,
        Second,
        Third,
    }

    [Flags]
    private enum Set
    {
        None = 0,
        One = 1,
        Two = 2,
    }

    private enum Wide : long
    {
        Only,
    }

    private enum Gapped
    {
        Low = 0,
        High = 2,
    }

    private enum StartsAtOne
    {
        One = 1,
        Two = 2,
    }

    private enum ReachesTheTop
    {
        Bottom = 0,
        Top = int.MaxValue,
    }

    // ----- a vocabulary that keeps the claim passes -----

    [Fact]
    public void AVocabularyThatKeepsTheClaimPassesEveryPart()
    {
        EnumVocabulary<Three>.AssertWholeVocabulary([Three.First, Three.Second, Three.Third], ["First", "Second", "Third"], Three.First);
        EnumVocabulary<Three>.AssertUnderlyingTypeAndNotFlags();
        EnumVocabulary<Three>.AssertNothingOutsideTheVocabularyIsDefined(Three.First, Three.Third);
        EnumVocabulary<Three>.AssertSaysItsOwnNumberOutsideTheVocabulary();
    }

    // ----- the whole vocabulary -----

    [Fact]
    public void AVocabularyMissingAValueFailsAndNamesTheEnum()
    {
        var failure = Assert.ThrowsAny<XunitException>(() =>
            EnumVocabulary<Three>.AssertWholeVocabulary([Three.First, Three.Second], ["First", "Second"], Three.First));

        Assert.Contains(nameof(Three), failure.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AVocabularyInAnotherOrderFails()
    {
        Assert.ThrowsAny<XunitException>(() =>
            EnumVocabulary<Three>.AssertWholeVocabulary([Three.Second, Three.First, Three.Third], ["Second", "First", "Third"], Three.First));
    }

    [Fact]
    public void ANameInAnotherCaseOrSpellingFails()
    {
        Assert.ThrowsAny<XunitException>(() =>
            EnumVocabulary<Three>.AssertWholeVocabulary([Three.First, Three.Second, Three.Third], ["first", "Second", "Third"], Three.First));
        Assert.ThrowsAny<XunitException>(() =>
            EnumVocabulary<Three>.AssertWholeVocabulary([Three.First, Three.Second, Three.Third], ["First", "Second"], Three.First));
    }

    [Fact]
    public void AnUnsetValueThatIsNotTheFirstFails()
    {
        var failure = Assert.ThrowsAny<XunitException>(() =>
            EnumVocabulary<Three>.AssertWholeVocabulary([Three.First, Three.Second, Three.Third], ["First", "Second", "Third"], Three.Second));

        Assert.Contains("unset", failure.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AVocabularyThatSkipsANumberOrStartsPastZeroFails()
    {
        Assert.ThrowsAny<XunitException>(() =>
            EnumVocabulary<Gapped>.AssertWholeVocabulary([Gapped.Low, Gapped.High], ["Low", "High"], Gapped.Low));
        Assert.ThrowsAny<XunitException>(() =>
            EnumVocabulary<StartsAtOne>.AssertWholeVocabulary([StartsAtOne.One, StartsAtOne.Two], ["One", "Two"], default));
    }

    [Fact]
    public void NoListHandedIsRefused()
    {
        Assert.Throws<ArgumentNullException>(() => EnumVocabulary<Three>.AssertWholeVocabulary(null!, ["First"], Three.First));
        Assert.Throws<ArgumentNullException>(() => EnumVocabulary<Three>.AssertWholeVocabulary([Three.First], null!, Three.First));
    }

    // ----- an int underneath, and no set -----

    [Fact]
    public void ASetFailsAndNamesTheEnum()
    {
        var failure = Assert.ThrowsAny<XunitException>(EnumVocabulary<Set>.AssertUnderlyingTypeAndNotFlags);

        Assert.Contains(nameof(Set), failure.Message, StringComparison.Ordinal);
        Assert.Contains("Flags", failure.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEnumOnALongFailsAndNamesTheEnum()
    {
        var failure = Assert.ThrowsAny<XunitException>(EnumVocabulary<Wide>.AssertUnderlyingTypeAndNotFlags);

        Assert.Contains(nameof(Wide), failure.Message, StringComparison.Ordinal);
        Assert.Contains(nameof(Int64), failure.Message, StringComparison.Ordinal);
    }

    // ----- nothing outside the vocabulary -----

    [Fact]
    public void ABoundaryDrawnInsideTheVocabularyFails()
    {
        // Handed Second as the last, the helper finds Third defined one above it.
        var failure = Assert.ThrowsAny<XunitException>(() =>
            EnumVocabulary<Three>.AssertNothingOutsideTheVocabularyIsDefined(Three.First, Three.Second));

        Assert.Contains("outside", failure.Message, StringComparison.Ordinal);

        // Handed Second as the first, the helper finds First defined one below it.
        Assert.ThrowsAny<XunitException>(() =>
            EnumVocabulary<Three>.AssertNothingOutsideTheVocabularyIsDefined(Three.Second, Three.Third));
    }

    [Fact]
    public void AGapInsideTheVocabularyFails()
    {
        var failure = Assert.ThrowsAny<XunitException>(() =>
            EnumVocabulary<Gapped>.AssertNothingOutsideTheVocabularyIsDefined(Gapped.Low, Gapped.High));

        Assert.Contains("inside", failure.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AVocabularyThatReachesTheTopOfIntFailsAtTheTopAndStepsNoFurther()
    {
        // The top of int is defined, so it is not "outside"; the helper does not step past it into an overflow.
        Assert.ThrowsAny<XunitException>(() =>
            EnumVocabulary<ReachesTheTop>.AssertNothingOutsideTheVocabularyIsDefined(ReachesTheTop.Top, ReachesTheTop.Top));
        Assert.ThrowsAny<XunitException>(EnumVocabulary<ReachesTheTop>.AssertSaysItsOwnNumberOutsideTheVocabulary);
    }

    // ----- a value outside says its own number -----

    [Fact]
    public void AnEnumThatNamesTheTopOfIntDoesNotSayItsNumberThere()
    {
        var failure = Assert.ThrowsAny<XunitException>(EnumVocabulary<ReachesTheTop>.AssertSaysItsOwnNumberOutsideTheVocabulary);

        Assert.Contains(nameof(ReachesTheTop), failure.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheEndsOfIntAreSpelledAsTheBclSpellsThem()
    {
        Assert.Equal("-2147483648", ((Three)int.MinValue).ToString());
        Assert.Equal("2147483647", ((Three)int.MaxValue).ToString());
        Assert.Equal("3", ((Three)3).ToString());
        Assert.Equal("-1", ((Three)(-1)).ToString());
    }
}
