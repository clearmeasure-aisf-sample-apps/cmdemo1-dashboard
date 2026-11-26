using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The two length bounds a deployable's name and a version are held to, as <see cref="ApplicationText" /> decides
/// them: the longest name the address takes and the longest version that may name a file. The bounds live as
/// private constants in the class - <c>LongestName</c> and <c>LongestVersion</c> - and the app declares no
/// <c>InternalsVisibleTo</c>, so they are read by reflection and pinned here by name and by value, and the
/// behaviour at each bound is pinned with the named value rather than a bare literal: a change to either bound
/// is found here, where the test says "the longest name" and "the longest version" and not "64" and "40".
/// </summary>
public class ApplicationTextBoundsTests
{
    /// <summary>The two private constants of <see cref="ApplicationText" />, read by name: the longest letters of a
    /// deployable's name and the longest letters of a version. The values are pinned as well, so a change to either
    /// is a change the test sees, and the bound is the one the class decided, not one a test spelled again.</summary>
    [Theory]
    [InlineData("LongestName", 64)]
    [InlineData("LongestVersion", 40)]
    public void TheTwoBoundsAreThePrivateConstantsOfTheClass(string name, int value)
    {
        var field = typeof(ApplicationText).GetField(name, BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(field);
        Assert.True(field!.IsLiteral);
        Assert.True(field.IsPrivate);
        Assert.Equal(value, field.GetRawConstantValue());
        // The bound is the class's own business: no public member of the same name stands in for it.
        Assert.Null(typeof(ApplicationText).GetField(name, BindingFlags.Public | BindingFlags.Static));
    }

    /// <summary>A helper that reads the two bounds by name and hands them to the tests that pin the behaviour at
    /// each side of the bound: the word of the bound is taken, the word one letter over is refused, and the word
    /// one letter short is taken. The tests name the bound as "the longest name" and "the longest version" rather
    /// than writing the number again.</summary>
    private static (int LongestName, int LongestVersion) Bounds()
    {
        int Bound(string name)
        {
            var field = typeof(ApplicationText).GetField(name, BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException($"ApplicationText has no private static constant {name}.");
            return (int)field.GetRawConstantValue()!;
        }

        return (Bound("LongestName"), Bound("LongestVersion"));
    }

    /// <summary>The longest name the address takes is the one the constant says it is: a name of exactly that many
    /// letters is a name, one letter more is not, and one letter fewer is still a name. The letters are the
    /// simplest the guard allows - all digits - so the bound is the only thing the test is asking about.</summary>
    [Fact]
    public void ANameOfTheLongestLettersIsTakenAndOneMoreIsRefused()
    {
        var (longestName, _) = Bounds();

        Assert.True(ApplicationText.IsName(new string('7', longestName)));
        Assert.True(ApplicationText.IsName(new string('7', longestName - 1)));
        Assert.False(ApplicationText.IsName(new string('7', longestName + 1)));
        Assert.False(ApplicationText.IsName(new string('7', longestName * 100)));
    }

    /// <summary>The longest version that may name a file is the one the constant says it is: a version of exactly
    /// that many letters is a version, one letter more is not, and one letter fewer is still a version. The
    /// longest such version also names a file - the base, a slash, the letters and the extension - and the one
    /// letter over names none at all: the bound is the limit of the address the view ever asks, not just a guard
    /// on a word.</summary>
    [Fact]
    public void AVersionOfTheLongestLettersNamesAFileAndOneMoreNamesNone()
    {
        var (_, longestVersion) = Bounds();
        const string uri = "https://raw.githubusercontent.com/org/app/architecture/ui";

        Assert.True(ApplicationText.IsVersion(new string('7', longestVersion)));
        Assert.True(ApplicationText.IsVersion(new string('7', longestVersion - 1)));
        Assert.False(ApplicationText.IsVersion(new string('7', longestVersion + 1)));
        Assert.False(ApplicationText.IsVersion(new string('7', longestVersion * 100)));

        var longest = new string('7', longestVersion);
        var oneOver = new string('7', longestVersion + 1);

        Assert.Equal($"{uri}/{longest}.svg", ApplicationText.File(new Uri(uri), longest, "svg")!.AbsoluteUri);
        Assert.Equal($"{uri}/{longest}.json", ApplicationText.File(new Uri(uri), longest, "json")!.AbsoluteUri);
        Assert.Null(ApplicationText.File(new Uri(uri), oneOver, "svg"));
    }

    /// <summary>The two bounds are not the same: a word of the longest name's letters is a name and no version,
    /// and a word of the longest version's letters is a version and no name only when the two differ. The bound
    /// of the name is the longer of the two, so the gap between them is where a word is one and not the other -
    /// pinned here with the named values so the gap is the one the two constants decide.</summary>
    [Fact]
    public void TheTwoBoundsKeepTheirOwnLengthAndTheGapBetweenThem()
    {
        var (longestName, longestVersion) = Bounds();

        // The name is the longer bound: a word of its letters is a name and no version.
        Assert.True(ApplicationText.IsName(new string('7', longestName)));
        Assert.False(ApplicationText.IsVersion(new string('7', longestName)));

        // The version's bound is shorter than the name's: a word of the version's letters is a version, and a
        // word one letter over the version's bound is still a name while it is under the name's bound.
        Assert.True(ApplicationText.IsVersion(new string('7', longestVersion)));
        Assert.False(ApplicationText.IsVersion(new string('7', longestVersion + 1)));

        // The gap: every length between the version's bound and the name's bound, from one letter over the
        // version's bound up to the name's bound itself, is a name and no version - the word is too long for a
        // file but still a name the address takes. The name's bound is the longest of the two, so the gap is
        // non-empty and the walk below covers it whole.
        for (var letters = longestVersion + 1; letters <= longestName; letters++)
        {
            var word = new string('7', letters);
            Assert.True(ApplicationText.IsName(word), $"a word of {letters} letters is a name");
            Assert.False(ApplicationText.IsVersion(word), $"a word of {letters} letters is no version");
        }

        // And one letter over the name's bound is neither: the word is no name and no version.
        Assert.False(ApplicationText.IsName(new string('7', longestName + 1)));
        Assert.False(ApplicationText.IsVersion(new string('7', longestName + 1)));
    }
}
