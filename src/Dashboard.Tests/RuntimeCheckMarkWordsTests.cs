using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// The words a detailed health check's entry carries as a mark (<see cref="RuntimePayloadBuilder.MarkOf"/>): the four
/// mark words are named once (<see cref="RuntimePayloadBuilder.Healthy"/> and the check-mark consts of
/// <see cref="RuntimeStateWords"/>) and the mark of each state is that word.
/// </summary>
public sealed class RuntimeCheckMarkWordsTests
{
    [Theory]
    [InlineData(CheckState.Healthy, "healthy")]
    [InlineData(CheckState.Degraded, "degraded")]
    [InlineData(CheckState.Unhealthy, "failed")]
    public void TheMarkOfANamedStateIsTheLiteralWordTheScriptDrawsBy(CheckState state, string word) =>
        Assert.Equal(word, RuntimePayloadBuilder.MarkOf(state));

    [Fact]
    public void TheMarkOfAnOutOfRangeStateIsTheLiteralWordUnknown() =>
        Assert.Equal("unknown", RuntimePayloadBuilder.MarkOf((CheckState)999));

    [Theory]
    [InlineData(CheckState.Healthy)]
    [InlineData(CheckState.Degraded)]
    [InlineData(CheckState.Unhealthy)]
    public void TheMarkOfANamedStateIsThatStatesNamedConst(CheckState state)
    {
        var named = state switch
        {
            CheckState.Healthy => RuntimePayloadBuilder.Healthy,
            CheckState.Degraded => RuntimeStateWords.Degraded,
            CheckState.Unhealthy => RuntimeStateWords.Failed,
            _ => throw new ArgumentOutOfRangeException(nameof(state)),
        };
        Assert.Equal(named, RuntimePayloadBuilder.MarkOf(state));
    }

    [Fact]
    public void TheMarkOfAnOutOfRangeStateIsTheNamedConstUnknown() =>
        Assert.Equal(RuntimeStateWords.Unknown, RuntimePayloadBuilder.MarkOf((CheckState)999));

    [Fact]
    public void TheFourMarkWordsAreDistinct()
    {
        var words = new[]
        {
            RuntimePayloadBuilder.MarkOf(CheckState.Healthy),
            RuntimePayloadBuilder.MarkOf(CheckState.Degraded),
            RuntimePayloadBuilder.MarkOf(CheckState.Unhealthy),
            RuntimePayloadBuilder.MarkOf((CheckState)999),
        };
        Assert.Equal(words.Length, words.Distinct().Count());
    }
}
