namespace Dashboard.Tests;

/// <summary>
/// The one no-mark keep-the-head cut, <c>Clip.At</c>: the text as it stands when it is at most the length, otherwise
/// its first <paramref name="length" /> characters. The cut adds no mark and trims nothing, so the head is kept whole.
/// <c>Clip</c> is internal; the test project compiles its one file (see <c>Dashboard.Tests.csproj</c>).
/// </summary>
public class ClipTests
{
    /// <summary>
    /// A text shorter than the length comes back unchanged and is the same instance: the cut does not copy a text it
    /// does not cut, so the reference the caller holds is the one the cut returns.
    /// </summary>
    [Fact]
    public void ATextShorterThanTheLengthComesBackUnchangedAndIsTheSameInstance()
    {
        var text = "abc";
        var result = Clip.At(text, 5);

        Assert.Equal("abc", result);
        Assert.Same(text, result);
    }

    /// <summary>
    /// A text of exactly the length comes back whole: the boundary the three hand-written copies of the rule agreed on
    /// - a text of N characters is kept whole, not cut to N-1, and no mark is added.
    /// </summary>
    [Fact]
    public void ATextOfExactlyTheLengthComesBackWhole()
    {
        const int Length = 300;
        var text = new string('x', Length);

        Assert.Equal(text, Clip.At(text, Length));
        Assert.Equal(Length, Clip.At(text, Length).Length);
    }

    /// <summary>
    /// A text one character longer comes back with exactly the length characters: the first <paramref name="length" />
    /// are kept and the rest dropped, with no mark at the cut.
    /// </summary>
    [Fact]
    public void ATextOneCharacterLongerComesBackWithExactlyTheLengthCharacters()
    {
        const int Length = 300;
        var text = new string('x', Length) + "y";

        var result = Clip.At(text, Length);

        Assert.Equal(new string('x', Length), result);
        Assert.Equal(Length, result.Length);
    }

    /// <summary>
    /// A length of 0 gives an empty string: the cut keeps no characters and adds no mark, so even a non-empty text
    /// comes back as the empty word.
    /// </summary>
    [Fact]
    public void ALengthOfZeroGivesAnEmptyString()
    {
        Assert.Equal(string.Empty, Clip.At("hello", 0));
        Assert.Equal(0, Clip.At("hello", 0).Length);
    }
}
