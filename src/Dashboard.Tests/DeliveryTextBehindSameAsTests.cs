using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// The invariant-culture pins of the two "same as" exits of <see cref="DeliveryText.Behind" /> and the banker's
/// rounding at the 1-day edge, in the new test file the refactoring of the duplicated literal and the double
/// rounding called for (#2083): the early return for <c>Versions == 0</c>, the second exit when the days round to
/// under one, the versions check winning over the days, the half-day going to the even whole beside it, and the
/// environment equal to the first said nothing at all.
/// </summary>
public class DeliveryTextBehindSameAsTests
{
    /// <summary>
    /// The first exit: <c>Versions == 0</c> gives <c>same as tdd</c> whatever the days are - five, forty-seven,
    /// negative, or absent - because the versions check comes before the days are even read.
    /// </summary>
    [Theory]
    [InlineData(5d)]
    [InlineData(47.9d)]
    [InlineData(-3d)]
    [InlineData(null)]
    public void VersionsZeroGivesSameAsWhateverTheDaysAre(double? days) =>
        Assert.Equal("same as tdd", DeliveryText.Behind(new BehindFirst(0, days), "uat", "tdd"));

    /// <summary>
    /// The second exit: <c>Versions</c> null and <c>Days</c> 0.4 rounds to 0, which is under 1, so no part is added
    /// and the answer falls through to "same as" - the days were present, so the card says the environments are
    /// level rather than saying nothing.
    /// </summary>
    [Fact]
    public void VersionsNullAndDaysRoundingUnderOneGivesSameAs() =>
        Assert.Equal("same as tdd", DeliveryText.Behind(new BehindFirst(null, 0.4), "uat", "tdd"));

    /// <summary>
    /// The versions check wins: <c>Versions</c> 0 and <c>Days</c> 5 gives <c>same as tdd</c> - the early return
    /// for zero versions fires before the days are read, so five days behind is not said.
    /// </summary>
    [Fact]
    public void VersionsZeroBeatsDaysFive() =>
        Assert.Equal("same as tdd", DeliveryText.Behind(new BehindFirst(0, 5), "uat", "tdd"));

    /// <summary>
    /// Banker's rounding at the 1-day edge, pinned in the invariant culture: 0.5 rounds to 0 (the even whole
    /// beside it), so no part is added and the answer is "same as tdd"; 1.5 rounds to 2, so the part is "2 days"
    /// and the answer is "2 days behind tdd". Both pin <see cref="MidpointRounding.ToEven" /> applied to the same
    /// <see cref="double" />.
    /// </summary>
    [Fact]
    public void BankersRoundingAtTheOneDayEdge()
    {
        using var _ = new InvariantCultureScope();

        Assert.Equal("same as tdd", DeliveryText.Behind(new BehindFirst(null, 0.5), "uat", "tdd"));
        Assert.Equal("2 days behind tdd", DeliveryText.Behind(new BehindFirst(null, 1.5), "uat", "tdd"));
    }

    /// <summary>
    /// Neither number is a number: <c>Versions</c> null and <c>Days</c> null means the file does not say, and the
    /// card says nothing - <c>null</c>, not "same as".
    /// </summary>
    [Fact]
    public void VersionsNullAndDaysNullGivesNull() =>
        Assert.Null(DeliveryText.Behind(new BehindFirst(null, null), "uat", "tdd"));

    /// <summary>
    /// The environment equal to the first, compared ignoring case, gives <c>null</c>: the comparison never reaches
    /// the numbers because the guard at the top of <c>Behind</c> says the card is for the first environment itself.
    /// </summary>
    [Theory]
    [InlineData("tdd", "tdd")]
    [InlineData("TDD", "tdd")]
    [InlineData("tdd", "Tdd")]
    [InlineData("Tdd", "TDD")]
    public void TheEnvironmentEqualIgnoringCaseGivesNull(string environment, string first) =>
        Assert.Null(DeliveryText.Behind(new BehindFirst(2, 3), environment, first));

    /// <summary>
    /// The first environment named <c>null</c> gives <c>null</c>: the file does not name the environment the card
    /// is compared with, so the card says nothing rather than "same as".
    /// </summary>
    [Fact]
    public void FirstNullGivesNull() =>
        Assert.Null(DeliveryText.Behind(new BehindFirst(2, 3), "uat", null));

    /// <summary>
    /// The distance itself <c>null</c> gives <c>null</c>: the file does not compare this environment at all.
    /// </summary>
    [Fact]
    public void BehindNullGivesNull() =>
        Assert.Null(DeliveryText.Behind(null, "uat", "tdd"));

    /// <summary>Forces the invariant culture for the duration of the test, restoring the previous one.</summary>
    private sealed class InvariantCultureScope : IDisposable
    {
        private readonly CultureInfo _previous = CultureInfo.CurrentCulture;

        public InvariantCultureScope() => CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        public void Dispose() => CultureInfo.CurrentCulture = _previous;
    }
}
