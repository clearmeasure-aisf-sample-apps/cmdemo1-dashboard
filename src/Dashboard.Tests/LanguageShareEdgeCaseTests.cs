namespace Dashboard.Tests;

/// <summary>
/// A <see cref="LanguageShare"/> is one part of the language bar: a language - or "Other" for the small ones
/// together - with its lines, its share of the lines of all languages, and the bar colour it stands on. The shares
/// the bar computes are pinned where <see cref="BuildInfoTests"/> names the five largest and sums the rest - the
/// names, the slots, the Other's lines, the shares that add to the whole - and what they leave open is pinned here:
/// the parts the share holds exactly as given, the parts turned, the name that is not there, the share that is not
/// there, the two shares that are one while the four parts are, the share's words in the order the record declares
/// the parts, the share falling apart into its parts and coming back together as a copy, and the ends of the lines,
/// of the slot and of the percent as the share holds them.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every share is one the test builds.
/// </para>
/// </summary>
public class LanguageShareEdgeCaseTests
{
    // ----- the parts the share holds

    /// <summary>The share keeps the very parts it is given: the name the same instance the share was built with,
    /// and the lines, the percent and the slot as given - the fixed facts a bar part stands on.</summary>
    [Fact]
    public void TheShareKeepsThePartsItIsGiven()
    {
        var name = "Razor";
        var share = new LanguageShare(name, 4152, 11.5, 2);

        Assert.Same(name, share.Name);
        Assert.Equal(4152L, share.Lines);
        Assert.Equal(11.5d, share.Percent);
        Assert.Equal(2, share.Slot);
    }

    /// <summary>The share's parts are given once, in the share: the name renamed in the initializer, the lines
    /// turned to the end of the count, the percent turned to no number and the slot turned to the end of its count
    /// - the share holds the parts as given, equal to the share built with the same parts and not to the share
    /// built with the parts it overrode.</summary>
    [Fact]
    public void APartGivenInTheShareIsKeptAsGiven()
    {
        var share = new LanguageShare("C#", 61234, 73, 1)
        {
            Name = "Razor",
            Lines = long.MaxValue,
            Percent = double.NaN,
            Slot = int.MinValue,
        };

        Assert.Equal("Razor", share.Name);
        Assert.Equal(long.MaxValue, share.Lines);
        Assert.True(double.IsNaN(share.Percent));
        Assert.Equal(int.MinValue, share.Slot);
        Assert.Equal(share, new LanguageShare("Razor", long.MaxValue, double.NaN, int.MinValue));
        Assert.NotEqual(share, new LanguageShare("C#", 61234, 73, 1));
    }

    /// <summary>The share takes the name it is given on trust and keeps it as it stands: a share of no name is not
    /// refused and not a share that names nothing - it is a share whose name is not there, the rest of the parts it
    /// was told kept as given; the share that calls itself nothing is a share whose name is the empty word, and no
    /// share is the share that is not there.</summary>
    [Fact]
    public void ANameThatIsNotThereIsKeptAsNotThere()
    {
        var noName = new LanguageShare(null!, 4152, 11.5, 2);

        Assert.Null(noName.Name);
        Assert.Equal(4152L, noName.Lines);
        Assert.Equal(11.5d, noName.Percent);
        Assert.Equal(2, noName.Slot);

        var nothing = new LanguageShare("", 0, 0, 0);

        Assert.Equal("", nothing.Name);
        Assert.Equal(0, nothing.Lines);
        Assert.Equal(0d, nothing.Percent);
        Assert.Equal(0, nothing.Slot);

        Assert.Null(default(LanguageShare));
    }

    // ----- the ends of the numbers

    /// <summary>The lines the share says are its to hold at the ends of the count: the zero the share of no lines
    /// stands on, and the two ends of the count between which no bar part has ever stood - the share keeps them as
    /// the counts are apart.</summary>
    [Fact]
    public void TheEndsOfTheLinesAreKeptApart()
    {
        var zero = new LanguageShare("C#", 0, 0, 1);
        var bottom = new LanguageShare("C#", long.MinValue, 0, 1);
        var top = new LanguageShare("C#", long.MaxValue, 0, 1);

        Assert.Equal(0, zero.Lines);
        Assert.Equal(long.MinValue, bottom.Lines);
        Assert.Equal(long.MaxValue, top.Lines);
        Assert.NotEqual(zero, bottom);
        Assert.NotEqual(zero, top);
        Assert.NotEqual(bottom, top);
    }

    /// <summary>The slot the share carries is whatever number it was given: the 1 the largest language stands on
    /// and the 0 the "Other" stands on, the one before the first the bar names, and the two ends of the count - the
    /// share holds them as the numbers are apart.</summary>
    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void TheSlotIsKeptAsTheNumberItStandsOn(int slot)
    {
        var share = new LanguageShare("C#", 100, 50, slot);

        Assert.Equal(slot, share.Slot);
        Assert.Equal(share, new LanguageShare("C#", 100, 50, slot));
    }

    /// <summary>The percent the share says is its to hold: the 0 that is no share of the lines and the 100 that is
    /// all of them, the smallest number that is not zero, no number at all, and the two ends of the number - the
    /// share keeps them as the numbers are.</summary>
    [Theory]
    [InlineData(0d)]
    [InlineData(100d)]
    [InlineData(double.Epsilon)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(double.MinValue)]
    [InlineData(double.MaxValue)]
    public void ThePercentIsKeptAsTheNumberItStandsOn(double percent)
    {
        var share = new LanguageShare("C#", 100, percent, 1);

        Assert.Equal(percent, share.Percent);
        Assert.Equal(share, new LanguageShare("C#", 100, percent, 1));
    }

    // ----- the share that is not there

    /// <summary>A share that is not there is nothing: the record's default is no share, and a share is never equal
    /// to no share however the question is asked - the operators, and <c>Equals</c> asked about the no-share - while
    /// the share that is there is a share to both of them.</summary>
    [Fact]
    public void AShareThatIsNotThereIsNoShare()
    {
        Assert.Null(default(LanguageShare));

        LanguageShare? none = null;
        var there = new LanguageShare("C#", 61234, 73, 1);

        Assert.False(none == there);
        Assert.True(none != there);
        Assert.False(there == none);
        Assert.True(there != none);
        Assert.False(there.Equals(none));
        Assert.False(there.Equals(null));
        Assert.True(there is not null);
    }

    // ----- the share and its equal

    /// <summary>Two shares are one while the four parts are - the name's own equality, the lines, the percent and
    /// the slot - and a difference in any one of them is another one. The hash code follows the equality, and
    /// neither no share nor a word is a share's equal.</summary>
    [Fact]
    public void TwoSharesAreOneWhileTheFourPartsAre()
    {
        var one = new LanguageShare("C#", 61234, 73, 1);
        var two = new LanguageShare("C#", 61234, 73, 1);

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.True(one.Equals((object)two));
        Assert.True(two.Equals((object)one));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.False(one.Equals("C#"));

        Assert.NotEqual(one, new LanguageShare("Razor", 61234, 73, 1));
        Assert.NotEqual(one, new LanguageShare("C#", 61235, 73, 1));
        Assert.NotEqual(one, new LanguageShare("C#", 61234, 73.5, 1));
        Assert.NotEqual(one, new LanguageShare("C#", 61234, 73, 2));
    }

    /// <summary>A percent that is no number is the share's to hold as no number: the share that says no number is
    /// one with its equal while the rest of the parts are - the equality the number gives - and not one with the
    /// share that says a number, however small, or the one that says no end.</summary>
    [Fact]
    public void ASshareThatSaysNoNumberIsOneWithItsEqual()
    {
        var one = new LanguageShare("C#", 1, double.NaN, 1);
        var two = new LanguageShare("C#", 1, double.NaN, 1);

        Assert.True(double.IsNaN(one.Percent));
        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.NotEqual(one, new LanguageShare("C#", 1, 0, 1));
        Assert.NotEqual(one, new LanguageShare("C#", 1, double.Epsilon, 1));
        Assert.NotEqual(one, new LanguageShare("C#", 1, double.PositiveInfinity, 1));
    }

    /// <summary>The share whose name is not there is one with the share whose name is not there - the no-name the
    /// equality of the parts gives - and not one with the share that calls itself nothing: the no-name and the
    /// empty word are apart as the parts are apart, and the hash code follows the equality.</summary>
    [Fact]
    public void ASshareWithNoNameIsOneWithItsEqualAndNoneWithTheEmptyName()
    {
        var noName = new LanguageShare(null!, 1, 1, 1);
        var emptyName = new LanguageShare("", 1, 1, 1);

        Assert.Equal(noName, new LanguageShare(null!, 1, 1, 1));
        Assert.True(noName == new LanguageShare(null!, 1, 1, 1));
        Assert.Equal(noName.GetHashCode(), new LanguageShare(null!, 1, 1, 1).GetHashCode());
        Assert.NotEqual(noName, emptyName);
        Assert.True(noName != emptyName);
    }

    // ----- the share's own words

    /// <summary>The share says its parts in the order the record declares them - the name first, the lines, the
    /// percent and the slot after, each by the word the type says: the no-name said as the empty word, the ends of
    /// the count said as the ends stand, and the percent that is no number said as no number.</summary>
    [Fact]
    public void TheShareSaysItsPartsInTheOrderTheRecordDeclaresThem()
    {
        Assert.Equal(
            "LanguageShare { Name = C#, Lines = 61234, Percent = 73, Slot = 1 }",
            new LanguageShare("C#", 61234, 73, 1).ToString());

        Assert.Equal(
            "LanguageShare { Name = , Lines = 0, Percent = 0, Slot = 0 }",
            new LanguageShare(null!, 0, 0, 0).ToString());

        Assert.Equal(
            "LanguageShare { Name = , Lines = 0, Percent = 0, Slot = 0 }",
            new LanguageShare("", 0, 0, 0).ToString());

        Assert.Equal(
            "LanguageShare { Name = C#, Lines = -9223372036854775808, Percent = NaN, Slot = -2147483648 }",
            new LanguageShare("C#", long.MinValue, double.NaN, int.MinValue).ToString());
    }

    // ----- the share falls apart and is copied

    /// <summary>The share falls apart into the four parts it holds - the name the same instance the share was built
    /// with - and a copy of the share is the share again: the <c>with</c> that names no part, and the <c>with</c>
    /// that turns a part keeping the rest, at the ends of the count - equal to the share while the parts are, and
    /// never the same reference.</summary>
    [Fact]
    public void TheShareFallsApartIntoItsPartsAndACopyIsTheShareAgain()
    {
        var name = "C#";
        var share = new LanguageShare(name, 61234, 73, 1);

        var (partsName, partsLines, partsPercent, partsSlot) = share;
        Assert.Same(name, partsName);
        Assert.Equal(61234L, partsLines);
        Assert.Equal(73d, partsPercent);
        Assert.Equal(1, partsSlot);

        var copy = share with { };
        Assert.Equal(share, copy);
        Assert.NotSame(share, copy);
        Assert.Equal("C#", copy.Name);
        Assert.Equal(61234L, copy.Lines);
        Assert.Equal(73d, copy.Percent);
        Assert.Equal(1, copy.Slot);

        var named = share with { Name = "Razor" };
        Assert.NotSame(share, named);
        Assert.Equal("Razor", named.Name);
        Assert.Equal(61234L, named.Lines);
        Assert.Equal(73d, named.Percent);
        Assert.Equal(1, named.Slot);

        var sized = share with { Lines = long.MaxValue, Slot = int.MinValue };
        Assert.Equal(long.MaxValue, sized.Lines);
        Assert.Equal(int.MinValue, sized.Slot);
        Assert.Equal("C#", sized.Name);
        Assert.Equal(73d, sized.Percent);

        var unnamed = named with { Name = null! };
        Assert.Null(unnamed.Name);
        Assert.Equal(61234L, unnamed.Lines);
        Assert.Equal(73d, unnamed.Percent);
        Assert.Equal(1, unnamed.Slot);
    }
}
