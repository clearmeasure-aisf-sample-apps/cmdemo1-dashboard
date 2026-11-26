using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The two length bounds a deployable's name and a version are held to, as <see cref="ApplicationText" /> decides
/// them: the longest name the address takes and the longest version that may name a file. The bounds live as
/// private constants in the class - <c>LongestName</c> and <c>LongestVersion</c> - and the app declares no
/// <c>InternalsVisibleTo</c>, so they are read through <see cref="ApplicationBounds" /> and pinned here by name and by value, and the
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
        var field = ApplicationBounds.Field(name);

        Assert.NotNull(field);
        Assert.True(field!.IsLiteral);
        Assert.True(field.IsPrivate);
        Assert.Equal(value, field.GetRawConstantValue());
        // The bound is the class's own business: no public member of the same name stands in for it.
        Assert.Null(typeof(ApplicationText).GetField(name, BindingFlags.Public | BindingFlags.Static));
    }

    /// <summary>The shared <see cref="ApplicationBounds" /> asks the class and nothing else: each bound is the
    /// constant of the same name, a name it does not hold is no field, and its builders make a word of exactly the
    /// letters asked.</summary>
    [Fact]
    public void TheSharedBoundsAreTheOnesTheClassHolds()
    {
        Assert.Equal(ApplicationBounds.Field("LongestName")!.GetRawConstantValue(), ApplicationBounds.LongestName);
        Assert.Equal(ApplicationBounds.Field("LongestVersion")!.GetRawConstantValue(), ApplicationBounds.LongestVersion);
        Assert.Null(ApplicationBounds.Field("LongestWord"));
        Assert.Equal(ApplicationBounds.LongestName, ApplicationBounds.Name(ApplicationBounds.LongestName).Length);
        Assert.Equal(ApplicationBounds.LongestVersion, ApplicationBounds.Version(ApplicationBounds.LongestVersion).Length);
    }

    /// <summary>The longest name the address takes is the one the constant says it is: a name of exactly that many
    /// letters is a name, one letter more is not, and one letter fewer is still a name. The letters are the
    /// simplest the guard allows - all digits - so the bound is the only thing the test is asking about.</summary>
    [Fact]
    public void ANameOfTheLongestLettersIsTakenAndOneMoreIsRefused()
    {
        var longestName = ApplicationBounds.LongestName;

        Assert.True(ApplicationText.IsName(ApplicationBounds.Name(longestName)));
        Assert.True(ApplicationText.IsName(ApplicationBounds.Name(longestName - 1)));
        Assert.False(ApplicationText.IsName(ApplicationBounds.Name(longestName + 1)));
        Assert.False(ApplicationText.IsName(ApplicationBounds.Name(longestName * 100)));
    }

    /// <summary>The longest version that may name a file is the one the constant says it is: a version of exactly
    /// that many letters is a version, one letter more is not, and one letter fewer is still a version. The
    /// longest such version also names a file - the base, a slash, the letters and the extension - and the one
    /// letter over names none at all: the bound is the limit of the address the view ever asks, not just a guard
    /// on a word.</summary>
    [Fact]
    public void AVersionOfTheLongestLettersNamesAFileAndOneMoreNamesNone()
    {
        var longestVersion = ApplicationBounds.LongestVersion;
        const string uri = "https://raw.githubusercontent.com/org/app/architecture/ui";

        Assert.True(ApplicationText.IsVersion(ApplicationBounds.Version(longestVersion)));
        Assert.True(ApplicationText.IsVersion(ApplicationBounds.Version(longestVersion - 1)));
        Assert.False(ApplicationText.IsVersion(ApplicationBounds.Version(longestVersion + 1)));
        Assert.False(ApplicationText.IsVersion(ApplicationBounds.Version(longestVersion * 100)));

        var longest = ApplicationBounds.Version(longestVersion);
        var oneOver = ApplicationBounds.Version(longestVersion + 1);

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
        var (longestName, longestVersion) = (ApplicationBounds.LongestName, ApplicationBounds.LongestVersion);

        // The name is the longer bound: a word of its letters is a name and no version.
        Assert.True(ApplicationText.IsName(ApplicationBounds.Name(longestName)));
        Assert.False(ApplicationText.IsVersion(ApplicationBounds.Version(longestName)));

        // The version's bound is shorter than the name's: a word of the version's letters is a version, and a
        // word one letter over the version's bound is still a name while it is under the name's bound.
        Assert.True(ApplicationText.IsVersion(ApplicationBounds.Version(longestVersion)));
        Assert.False(ApplicationText.IsVersion(ApplicationBounds.Version(longestVersion + 1)));

        // The gap: every length between the version's bound and the name's bound, from one letter over the
        // version's bound up to the name's bound itself, is a name and no version - the word is too long for a
        // file but still a name the address takes. The name's bound is the longest of the two, so the gap is
        // non-empty and the walk below covers it whole.
        for (var letters = longestVersion + 1; letters <= longestName; letters++)
        {
            var word = ApplicationBounds.Name(letters);
            Assert.True(ApplicationText.IsName(word), $"a word of {letters} letters is a name");
            Assert.False(ApplicationText.IsVersion(word), $"a word of {letters} letters is no version");
        }

        // And one letter over the name's bound is neither: the word is no name and no version.
        Assert.False(ApplicationText.IsName(ApplicationBounds.Name(longestName + 1)));
        Assert.False(ApplicationText.IsVersion(ApplicationBounds.Version(longestName + 1)));
    }
}
