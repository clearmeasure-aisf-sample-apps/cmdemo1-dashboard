namespace Dashboard.Tests;

/// <summary>Label names every state the enum has, and refuses one it does not name instead of calling it checking.</summary>
public class HealthClassifierLabelExhaustiveTests
{
    public static TheoryData<HealthState> EveryState() => new(Enum.GetValues<HealthState>());

    [Theory]
    [MemberData(nameof(EveryState))]
    public void EveryDeclaredStateHasALabel(HealthState state) =>
        Assert.False(string.IsNullOrWhiteSpace(HealthClassifier.Label(state)));

    [Fact]
    public void EveryDeclaredStateHasItsOwnLabel() =>
        Assert.Equal(
            Enum.GetValues<HealthState>().Length,
            Enum.GetValues<HealthState>().Select(HealthClassifier.Label).Distinct(StringComparer.Ordinal).Count());

    [Fact]
    public void AStateTheEnumDoesNotNameIsRefused()
    {
        var unnamed = (HealthState)99;

        var thrown = Assert.Throws<ArgumentOutOfRangeException>(() => HealthClassifier.Label(unnamed));

        Assert.Equal("state", thrown.ParamName);
    }
}
