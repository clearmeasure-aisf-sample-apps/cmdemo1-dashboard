namespace Dashboard.Tests;

/// <summary>
/// Holds the shared <see cref="ApplicationNames" /> to the bound <see cref="ApplicationText" /> declares (#1118): the
/// longest name is exactly as long as the class's <c>LongestName</c> (asked through <see cref="ApplicationBounds" />), it is a name, and the one past it is
/// one character longer and is not.
/// </summary>
public class ApplicationNamesObjectMotherTests
{
    [Fact]
    public void TheLongestNameIsAsLongAsTheBoundTheClassDeclaresAndIsTaken()
    {
        var field = ApplicationBounds.Field(nameof(ApplicationBounds.LongestName));

        Assert.NotNull(field);
        Assert.Equal(64, ApplicationNames.Longest.Length);
        Assert.Equal(field!.GetRawConstantValue(), ApplicationNames.Longest.Length);
        Assert.Equal(ApplicationBounds.LongestName, ApplicationNames.Longest.Length);
        Assert.True(ApplicationText.IsName(ApplicationNames.Longest));
    }

    [Fact]
    public void TheNamePastTheLongestIsOneCharacterLongerAndIsRefused()
    {
        Assert.Equal(65, ApplicationNames.LongestPlusOne.Length);
        Assert.Equal(ApplicationBounds.LongestName + 1, ApplicationNames.LongestPlusOne.Length);
        Assert.StartsWith(ApplicationNames.Longest, ApplicationNames.LongestPlusOne, StringComparison.Ordinal);
        Assert.False(ApplicationText.IsName(ApplicationNames.LongestPlusOne));
    }
}
