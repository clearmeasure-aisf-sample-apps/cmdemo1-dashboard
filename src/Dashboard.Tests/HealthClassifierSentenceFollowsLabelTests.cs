using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// The sentence word of a checked state is the lowercase of its badge label by construction (#2091), so a
/// change to a badge word carries into the sentence; the unchecked state alone says "not checked".
/// </summary>
public class HealthClassifierSentenceFollowsLabelTests
{
    [Theory]
    [InlineData(HealthState.Healthy)]
    [InlineData(HealthState.Unhealthy)]
    [InlineData(HealthState.Unreachable)]
    public void ACheckedStatesSentenceIsTheLowercaseOfItsLabel(HealthState state) =>
        Assert.Equal(HealthClassifier.Label(state).ToLowerInvariant(), HealthClassifier.Sentence(state));

    [Fact]
    public void ThePendingSentenceIsNotCheckedAndNotTheLowercaseOfItsLabel()
    {
        var sentence = HealthClassifier.Sentence(HealthState.Pending);

        Assert.Equal("not checked", sentence);
        Assert.NotEqual(HealthClassifier.Label(HealthState.Pending).ToLowerInvariant(), sentence);
    }

    [Theory]
    [InlineData("tr-TR", HealthState.Healthy)]
    [InlineData("tr-TR", HealthState.Unhealthy)]
    [InlineData("tr-TR", HealthState.Unreachable)]
    [InlineData("fi-FI", HealthState.Unreachable)]
    [InlineData("ru-RU", HealthState.Unhealthy)]
    public void UnderAnotherCultureTheSentenceStillFollowsTheLabel(string cultureName, HealthState state)
    {
        using var scope = new CultureScope(cultureName);

        Assert.Equal(HealthClassifier.Label(state).ToLowerInvariant(), HealthClassifier.Sentence(state));
    }

    [Fact]
    public void UnderTurkishTheSentenceWordsAreSpelledAsBefore()
    {
        using var scope = new CultureScope("tr-TR");

        Assert.Equal("unhealthy", HealthClassifier.Sentence(HealthState.Unhealthy));
        Assert.Equal("unreachable", HealthClassifier.Sentence(HealthState.Unreachable));
        Assert.Equal("not checked", HealthClassifier.Sentence(HealthState.Pending));
    }
}
