namespace Dashboard.Tests;

/// <summary>
/// The number formats of <see cref="DeliveryText.LeadTime" /> and <see cref="DeliveryText.Failover" />: the tenth
/// keeps at most one decimal and drops it for a whole number, the minutes are whole, the rounding happens at the
/// unit boundary, the words are written in the invariant culture for every reader, and the failover seconds end
/// with the shared <see cref="TimeText.SecondsUnit" />.
/// </summary>
public class DeliveryTextFormatTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>A comma-decimal reader (fi-FI) still reads the lead time with a point.</summary>
    [Theory]
    [InlineData(5.2, "5.2 h")]
    [InlineData(84.0, "3.5 d")]
    [InlineData(0.5, "30 min")]
    public void LeadTimeWritesAPointForAFinnishReader(double hours, string expected) =>
        Assert.Equal(expected, CultureScope.Says("fi-FI", () => DeliveryText.LeadTime(hours)));

    /// <summary>A comma-decimal reader (fi-FI) still reads the failover seconds with a point.</summary>
    [Fact]
    public void FailoverWritesAPointForAFinnishReader() =>
        Assert.Equal(
            "the standby answered after 12.5 s",
            CultureScope.Says("fi-FI", () => DeliveryText.Failover(new FailoverTest(null, null, 12.5), Now)));

    /// <summary>A whole value is written without the decimal that has nothing to say.</summary>
    [Theory]
    [InlineData(2.0, "2 h")]
    [InlineData(72.0, "3 d")]
    public void AWholeLeadTimeDropsTheDecimal(double hours, string expected) =>
        Assert.Equal(expected, DeliveryText.LeadTime(hours));

    /// <summary>Whole failover seconds are written without a decimal.</summary>
    [Fact]
    public void WholeFailoverSecondsDropTheDecimal() =>
        Assert.Equal("the standby answered after 44 s", DeliveryText.Failover(new FailoverTest(null, null, 44), Now));

    /// <summary>The rounding happens at the boundary: a quarter hour is whole minutes, 47.96 hours rounds to 48.</summary>
    [Theory]
    [InlineData(0.25, "15 min")]
    [InlineData(47.96, "48 h")]
    [InlineData(0.999, "60 min")]
    public void LeadTimeRoundsAtTheBoundary(double hours, string expected) =>
        Assert.Equal(expected, DeliveryText.LeadTime(hours));

    /// <summary>The failover sentence ends with the shared seconds word, not a copy of its own.</summary>
    [Fact]
    public void FailoverEndsWithTheSharedSecondsUnit()
    {
        var said = DeliveryText.Failover(new FailoverTest("uat", Now.AddHours(-17), 44), Now);

        Assert.EndsWith(" " + TimeText.SecondsUnit, said, StringComparison.Ordinal);
        Assert.Equal("uat, 17 h ago: the standby answered after 44 s", said);
    }
}
