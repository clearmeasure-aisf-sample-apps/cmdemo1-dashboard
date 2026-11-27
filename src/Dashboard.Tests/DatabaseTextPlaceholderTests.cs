using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// The database list's one placeholder: <see cref="DatabaseText.NoValue"/> is the en dash the Nullable column and the
/// Columns and Rows counts all share, and <see cref="DatabaseText.Number"/> groups by thousands in the invariant
/// culture no matter the ambient one.
/// </summary>
public sealed class DatabaseTextPlaceholderTests
{
    [Fact]
    public void NoValueIsTheEnDashAndNotAHyphen()
    {
        Assert.Equal("\u2013", DatabaseText.NoValue);
        Assert.NotEqual("-", DatabaseText.NoValue);
        Assert.NotEqual("\u2014", DatabaseText.NoValue);
    }

    [Fact]
    public void TheNullableColumnSaysNoValueWhereTheLiveFileDoesNotSay() =>
        Assert.Equal(DatabaseText.NoValue, DatabaseText.Nullable(null));

    [Fact]
    public void TheCountSaysNoValueForOneNobodyCounted() =>
        Assert.Equal(DatabaseText.NoValue, DatabaseText.Number(null));

    [Fact]
    public void TheTwoPlaceholdersOfTheListAreOne() =>
        Assert.Equal(DatabaseText.Nullable(null), DatabaseText.Number(null));

    [Theory]
    [InlineData(0L, "0")]
    [InlineData(1L, "1")]
    [InlineData(1234567L, "1,234,567")]
    [InlineData(-1234567L, "-1,234,567")]
    public void ACountIsGroupedInThousandsInvariantly(long number, string text) =>
        Assert.Equal(text, DatabaseText.Number(number));

    [Theory]
    [InlineData("fi-FI")]
    [InlineData("de-DE")]
    public void ACountGroupsByTheCommaUnderANonInvariantCulture(string name)
    {
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name);
        try
        {
            // fi-FI and de-DE both write 1,234,567 as "1.234.567"; the invariant comma must win.
            Assert.Equal("1,234,567", DatabaseText.Number(1_234_567));
            Assert.Equal("-1,234,567", DatabaseText.Number(-1_234_567));
            Assert.Equal(DatabaseText.NoValue, DatabaseText.Number(null));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
