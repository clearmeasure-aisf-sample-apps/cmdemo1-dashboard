namespace Dashboard.Tests;

/// <summary>
/// The one no-mark keep-the-head cut, <c>Clip.At</c>: the text as it stands when it fits, otherwise its first
/// <paramref name="length"/> characters. <see cref="Ellipsis.Cut"/> is the rule with a mark; <c>Clip.At</c> adds
/// none. <c>Clip</c> is internal; the test project compiles its one file (see <c>Dashboard.Tests.csproj</c>).
/// </summary>
public class ClipTests
{
    /// <summary>A text shorter than the length comes back unchanged and is the same instance: the cut is not made,
    /// so the string reference is preserved.</summary>
    [Fact]
    public void ATextShorterThanTheLengthComesBackUnchangedAndIsTheSameInstance()
    {
        const string Text = "short";

        var result = Clip.At(Text, 10);

        Assert.Same(Text, result);
        Assert.Equal("short", result);
    }

    /// <summary>A text of exactly the length comes back whole: the boundary is inclusive, so no character is lost
    /// at the edge.</summary>
    [Fact]
    public void ATextOfExactlyTheLengthComesBackWhole()
    {
        const string Text = "abcdef";

        Assert.Equal(Text, Clip.At(Text, 6));
    }

    /// <summary>A text one character longer comes back with exactly <paramref name="length"/> characters: the
    /// extra character is dropped and no mark is added.</summary>
    [Fact]
    public void ATextOneCharacterLongerComesBackWithExactlyLengthCharacters()
    {
        const string Text = "abcdef";

        var result = Clip.At(Text, 5);

        Assert.Equal("abcde", result);
        Assert.Equal(5, result.Length);
    }

    /// <summary>A length of 0 gives the empty string: even an empty input comes back as the empty string, and a
    /// non-empty input is cut to nothing.</summary>
    [Fact]
    public void ALengthOfZeroGivesAnEmptyString()
    {
        Assert.Equal(string.Empty, Clip.At("abc", 0));
        Assert.Equal(string.Empty, Clip.At(string.Empty, 0));
    }
}
