using Xunit;

namespace Dashboard.Tests;

/// <summary>
/// The words of <see cref="Dashboard.Runtime.DatabaseText.Key"/> and <see cref="Dashboard.Runtime.DatabaseText.Nullable"/>:
/// an empty key and a key of only commas or blanks give no words, an unknown word passes through, the file words are
/// case-sensitive, and a nullable nobody stated is <see cref="Dashboard.Runtime.DatabaseText.NotSaid"/>.
/// </summary>
public class DatabaseTextKeyWordsTests
{
    [Fact]
    public void AnEmptyKeyGivesNoWords() =>
        Assert.Equal(string.Empty, Dashboard.Runtime.DatabaseText.Key(string.Empty));

    [Theory]
    [InlineData(",")]
    [InlineData(" , ")]
    [InlineData(" , , ")]
    public void AKeyOfOnlyCommasOrBlanksGivesNoWords(string key) =>
        Assert.Equal(string.Empty, Dashboard.Runtime.DatabaseText.Key(key));

    [Fact]
    public void AnUnknownWordPassesThroughUnchanged() =>
        Assert.Equal("unique", Dashboard.Runtime.DatabaseText.Key("unique"));

    [Fact]
    public void TheFileWordsAreCaseSensitive() =>
        Assert.Equal("PK", Dashboard.Runtime.DatabaseText.Key("PK"));

    [Theory]
    [InlineData(true, "yes")]
    [InlineData(false, "no")]
    public void ANullableIsSaidInAWord(bool? nullable, string word) =>
        Assert.Equal(word, Dashboard.Runtime.DatabaseText.Nullable(nullable));

    [Fact]
    public void ANullableNobodyStatedIsNotSaid() =>
        Assert.Equal(Dashboard.Runtime.DatabaseText.NotSaid, Dashboard.Runtime.DatabaseText.Nullable(null));
}
