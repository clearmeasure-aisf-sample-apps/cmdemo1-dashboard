using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// Holds the shared <see cref="ApplicationNames" /> to the bound <see cref="ApplicationText" /> declares (#1118): the
/// longest name is exactly as long as the class's private <c>LongestName</c>, it is a name, and the one past it is
/// one character longer and is not.
/// </summary>
public class ApplicationNamesObjectMotherTests
{
    [Fact]
    public void TheLongestNameIsAsLongAsTheBoundTheClassDeclaresAndIsTaken()
    {
        var field = typeof(ApplicationText).GetField("LongestName", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(field);
        Assert.Equal(64, ApplicationNames.Longest.Length);
        Assert.Equal(field!.GetRawConstantValue(), ApplicationNames.Longest.Length);
        Assert.True(ApplicationText.IsName(ApplicationNames.Longest));
    }

    [Fact]
    public void TheNamePastTheLongestIsOneCharacterLongerAndIsRefused()
    {
        Assert.Equal(65, ApplicationNames.LongestPlusOne.Length);
        Assert.StartsWith(ApplicationNames.Longest, ApplicationNames.LongestPlusOne, StringComparison.Ordinal);
        Assert.False(ApplicationText.IsName(ApplicationNames.LongestPlusOne));
    }
}
