namespace Dashboard.Tests;

public class HealthClassifierTests
{
    [Fact]
    public void Status200IsHealthy() =>
        Assert.Equal(HealthState.Healthy, HealthClassifier.FromStatusCode(200));

    [Theory]
    [InlineData(201)]
    [InlineData(204)]
    [InlineData(301)]
    [InlineData(401)]
    [InlineData(404)]
    [InlineData(500)]
    [InlineData(503)]
    public void EveryOtherStatusIsUnhealthy(int status) =>
        Assert.Equal(HealthState.Unhealthy, HealthClassifier.FromStatusCode(status));

    [Theory]
    [InlineData(HealthState.Healthy, "Healthy")]
    [InlineData(HealthState.Unhealthy, "Unhealthy")]
    [InlineData(HealthState.Unreachable, "Unreachable")]
    [InlineData(HealthState.Pending, "Checking")]
    public void EveryStateHasATextLabel(HealthState state, string label) =>
        Assert.Equal(label, HealthClassifier.Label(state));

    /// <summary>The lowercase words a state is said in, as the sentences of the page need them. The unchecked one is
    /// not checking: it has not been checked yet, so it cannot be said to be checking.</summary>
    [Theory]
    [InlineData(HealthState.Healthy, "healthy")]
    [InlineData(HealthState.Unhealthy, "unhealthy")]
    [InlineData(HealthState.Unreachable, "unreachable")]
    [InlineData(HealthState.Pending, "not checked")]
    public void EveryStateHasASentenceWord(HealthState state, string word) =>
        Assert.Equal(word, HealthClassifier.Sentence(state));

    /// <summary>A null state is the same as a state the enum does not name: not checked, because no check settled it.</summary>
    [Fact]
    public void AStateThatIsNotThereIsNotChecked() =>
        Assert.Equal("not checked", HealthClassifier.Sentence(null));
}
