namespace Dashboard.Tests;

/// <summary>
/// The behind line as its parts: <see cref="DeliveryText.Behind" /> collects the versions part and the days part in
/// a <see cref="CardParts" /> collector - a part the distance does not say comes in as null and is left out, the
/// rest are joined with <c>", "</c>, and the whole is hung before <c>behind {first}</c> - then answers
/// <c>same as {first}</c> when the days were known but said no part, and nothing at all when they were not.
/// Characterization of the words, the rounding and the order, part for part, the way the card says them today.
/// </summary>
public class DeliveryTextBehindPartsTests
{
    /// <summary>
    /// Every combination of the two parts the distance may give: no part is the silent or same-as answer, the
    /// versions stand alone, the days stand alone, and both stand with a comma between them before
    /// <c>behind tdd</c>.
    /// </summary>
    [Theory]
    [InlineData(null, null, null)]
    [InlineData(2, null, "2 versions behind tdd")]
    [InlineData(1, null, "1 version behind tdd")]
    [InlineData(3, 5d, "3 versions, 5 days behind tdd")]
    [InlineData(1, 1d, "1 version, 1 day behind tdd")]
    public void EveryCombinationOfThePartsSaysOnlyWhatTheDistanceGave(int? versions, double? days, string? expected) =>
        Assert.Equal(expected, DeliveryText.Behind(new BehindFirst(versions, days), "uat", "tdd"));

    /// <summary>
    /// The days part rounds to whole days before it is said, banker's style, and says nothing under one day:
    /// <c>0.4</c> and <c>0.5</c> round to 0, <c>1.4</c> to 1, <c>2.6</c> to 3. With no versions the answer is the
    /// days part alone, or <c>same as tdd</c> when the days round to nothing.
    /// </summary>
    [Theory]
    [InlineData(null, null, null)]
    [InlineData(null, 0.4, "same as tdd")]
    [InlineData(null, 0.5, "same as tdd")]
    [InlineData(null, 1.4, "1 day behind tdd")]
    [InlineData(null, 2.6, "3 days behind tdd")]
    public void TheDaysPartRoundsToWholeDaysAndSaysNothingUnderOne(int? versions, double? days, string? expected) =>
        Assert.Equal(expected, DeliveryText.Behind(new BehindFirst(versions, days), "uat", "tdd"));

    /// <summary>
    /// The days that do not round to a day the card says: a negative distance rounds negative and is left out, and
    /// the not-a-number and the infinities cast to zero and are left out too - the versions stand alone, or the
    /// answer is <c>same as tdd</c> when nothing does.
    /// </summary>
    [Theory]
    [InlineData(null, -3d, "same as tdd")]
    [InlineData(null, double.NaN, "same as tdd")]
    [InlineData(null, double.NegativeInfinity, "same as tdd")]
    [InlineData(2, -3d, "2 versions behind tdd")]
    [InlineData(2, double.NaN, "2 versions behind tdd")]
    [InlineData(1, double.NegativeInfinity, "1 version behind tdd")]
    public void TheDaysTheCardDoesNotSayAreLeftOut(int? versions, double? days, string expected) =>
        Assert.Equal(expected, DeliveryText.Behind(new BehindFirst(versions, days), "uat", "tdd"));

    /// <summary>
    /// The positive infinity: the cast of <see cref="Math.Round" />'s positive infinity to an int saturates to
    /// <c>int.MaxValue</c>, which is a whole day the card says - a pre-existing quirk the rounding and the cast keep,
    /// pinned here so it is never read as a feature. The negative infinity, by contrast, casts to
    /// <c>int.MinValue</c> and rounds under one day, so it is left out.
    /// </summary>
    [Theory]
    [InlineData(null, "2147483647 days behind tdd")]
    [InlineData(2, "2 versions, 2147483647 days behind tdd")]
    public void ThePositiveInfinitySaysTheMaxWholeDay(int? versions, string expected) =>
        Assert.Equal(expected, DeliveryText.Behind(new BehindFirst(versions, double.PositiveInfinity), "uat", "tdd"));

    /// <summary>
    /// Neither part said with the days known answers <c>same as tdd</c> - the environments are level - while
    /// neither part said with no days at all answers nothing, and the same silence stands for a null
    /// <see cref="BehindFirst" />, a null first, and the first environment itself, said case-insensitively.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("tdd")]
    [InlineData("TDD")]
    public void TheSilentAnswersStandForNothingKnownAndForTheFirstItself(string? environment) =>
        Assert.Null(DeliveryText.Behind(new BehindFirst(null, null), environment ?? "uat", "tdd"));

    /// <summary>
    /// The two exits the parts do not reach: a null <see cref="BehindFirst" /> and a null first both say nothing,
    /// and a distance of zero versions says <c>same as tdd</c> before the days are read.
    /// </summary>
    [Fact]
    public void TheNullExitsAndTheZeroVersionsExitStandWhereTheyAlwaysDid()
    {
        Assert.Null(DeliveryText.Behind(null, "uat", "tdd"));
        Assert.Null(DeliveryText.Behind(new BehindFirst(2, 3), "uat", null));
        Assert.Equal("same as tdd", DeliveryText.Behind(new BehindFirst(0, 5), "uat", "tdd"));
    }
}
