using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// The one end-of-line cut, <c>Ellipsis.Cut</c>: one character fewer than the line holds, trailing whitespace trimmed,
/// then the mark. <see cref="HealthDetailText.Brief" /> and <see cref="DeploymentActivity.Cut" /> both cut through it,
/// so a text they both cut comes out the same from each. <c>Ellipsis</c> is internal; the test project compiles its
/// one file (see <c>Dashboard.Tests.csproj</c>).
/// </summary>
public class EllipsisCutTests
{
    [Fact]
    public void TheMarkIsTheOneCharacterEllipsis()
    {
        Assert.Equal("…", Ellipsis.Mark);
        Assert.Equal(1, Ellipsis.Mark.Length);
    }

    [Fact]
    public void TheCutTrimsTrailingWhitespaceBeforeTheMark()
    {
        Assert.Equal("abcd…", Ellipsis.Cut("abcd \t ", 6));
    }

    [Fact]
    public void TheCutAtTheLengthOfOneIsTheMarkAlone()
    {
        Assert.Equal("…", Ellipsis.Cut("abc", 1));
    }

    [Theory]
    [InlineData("abcdefghij", 1)]
    [InlineData("abcdefghij", 5)]
    [InlineData("abcdefghij", 9)]
    [InlineData("a b c d e f", 4)]
    [InlineData("word      word", 8)]
    public void TheCutIsNeverLongerThanTheLength(string text, int length)
    {
        var cut = Ellipsis.Cut(text, length);

        Assert.True(cut.Length <= length, $"\"{cut}\" is longer than {length}");
        Assert.EndsWith(Ellipsis.Mark, cut, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCutAtTheLengthOfNothingRefusesFromTheSliceAsBriefDoes()
    {
        var zero = Assert.Throws<ArgumentOutOfRangeException>(() => Ellipsis.Cut("abc", 0));
        Assert.Equal("length", zero.ParamName);
        Assert.Equal(-1, zero.ActualValue);

        var brief = Assert.Throws<ArgumentOutOfRangeException>(() => HealthDetailText.Brief("abc", 0));
        Assert.Equal(zero.ParamName, brief.ParamName);
        Assert.Equal(zero.ActualValue, brief.ActualValue);
    }

    [Theory]
    [InlineData("A line with no full stop and a space in it", 10)]
    [InlineData("trailing   whitespace   at the cut", 12)]
    [InlineData("abcdef", 1)]
    [InlineData("abcdef", 5)]
    public void BriefAndTheNodeCutAgreeOnATextWithNoSentenceBreak(string text, int length)
    {
        var expected = Ellipsis.Cut(text, length);

        Assert.Equal(expected, HealthDetailText.Brief(text, length));
        Assert.Equal(expected, DeploymentActivity.Cut(text, length));
    }
}
