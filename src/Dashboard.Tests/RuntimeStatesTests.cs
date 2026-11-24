using Dashboard.Health;
using Dashboard.Runtime;

namespace Dashboard.Tests;

/// <summary>
/// The tile and edge states as enums (<see cref="TileState"/>, <see cref="EdgeState"/>), written as the words
/// <c>js/runtime.js</c> draws by only at the JSON boundary (<see cref="RuntimeStateWords"/>).
/// </summary>
public sealed class RuntimeStatesTests
{
    [Theory]
    [InlineData(TileState.Healthy, "healthy")]
    [InlineData(TileState.Unhealthy, "unhealthy")]
    [InlineData(TileState.Unreachable, "unreachable")]
    [InlineData(TileState.Checking, "checking")]
    [InlineData(TileState.Neutral, "neutral")]
    public void ATileStateIsWrittenAsTheWordTheScriptDrawsBy(TileState state, string word) =>
        Assert.Equal(word, state.Word());

    [Theory]
    [InlineData(EdgeState.Active, "active", "Carries the traffic.")]
    [InlineData(EdgeState.Idle, "idle", "Idle: no traffic expected.")]
    [InlineData(EdgeState.Down, "down", "Down: its end is not healthy.")]
    [InlineData(EdgeState.Checking, "checking", "Being checked.")]
    [InlineData(EdgeState.Neutral, "neutral", "Not checked.")]
    public void AnEdgeStateIsWrittenAsItsWordAndSaidAsItsSentence(EdgeState state, string word, string sentence) =>
        Assert.Equal((word, sentence), (state.Word(), state.Sentence()));

    [Fact]
    public void TheWordsAreTheSameCalledAsStaticMethods()
    {
        Assert.Equal("unreachable", RuntimeStateWords.Word(TileState.Unreachable));
        Assert.Equal(RuntimeStateWords.Idle, RuntimeStateWords.Word(EdgeState.Idle));
        Assert.Equal("Down: its end is not healthy.", RuntimeStateWords.Sentence(EdgeState.Down));
    }

    [Fact]
    public void EveryStateHasItsOwnWord()
    {
        Assert.Equal(Enum.GetValues<TileState>().Length, Enum.GetValues<TileState>().Select(state => state.Word()).Distinct().Count());
        Assert.Equal(Enum.GetValues<EdgeState>().Length, Enum.GetValues<EdgeState>().Select(state => state.Word()).Distinct().Count());
    }

    [Fact]
    public void AStateTheEnumDoesNotNameFailsLoud()
    {
        Assert.Equal("state", Assert.Throws<ArgumentOutOfRangeException>(() => ((TileState)99).Word()).ParamName);
        Assert.Equal("state", Assert.Throws<ArgumentOutOfRangeException>(() => ((EdgeState)99).Word()).ParamName);
        Assert.Equal("state", Assert.Throws<ArgumentOutOfRangeException>(() => ((EdgeState)99).Sentence()).ParamName);
    }

    [Fact]
    public void TheBuildersStateWordsAreTheWordsOfTheTileStates()
    {
        Assert.Equal(RuntimePayloadBuilder.Healthy, TileState.Healthy.Word());
        Assert.Equal(RuntimePayloadBuilder.Unhealthy, TileState.Unhealthy.Word());
        Assert.Equal(RuntimePayloadBuilder.Unreachable, TileState.Unreachable.Word());
        Assert.Equal(RuntimePayloadBuilder.Checking, TileState.Checking.Word());
        Assert.Equal(RuntimePayloadBuilder.Neutral, TileState.Neutral.Word());
    }

    [Theory]
    [InlineData(HealthState.Healthy, TileState.Healthy)]
    [InlineData(HealthState.Unhealthy, TileState.Unhealthy)]
    [InlineData(HealthState.Unreachable, TileState.Unreachable)]
    [InlineData(HealthState.Pending, TileState.Checking)]
    public void AHealthStateHasItsTileStateAndTheWordIsThatStatesWord(HealthState health, TileState tile)
    {
        Assert.Equal(tile, RuntimePayloadBuilder.TileStateOf(health));
        Assert.Equal(tile.Word(), RuntimePayloadBuilder.StateOf(health));
    }

    [Theory]
    [InlineData(ContainerAppState.Probing, TileState.Checking)]
    [InlineData(ContainerAppState.Waking, TileState.Checking)]
    [InlineData(ContainerAppState.Healthy, TileState.Healthy)]
    [InlineData(ContainerAppState.Responding, TileState.Healthy)]
    [InlineData(ContainerAppState.Unhealthy, TileState.Unhealthy)]
    [InlineData(ContainerAppState.NotResponding, TileState.Unreachable)]
    public void AContainerAppsProbeHasItsTileStateAndTheWordIsThatStatesWord(ContainerAppState probe, TileState tile)
    {
        Assert.Equal(tile, RuntimePayloadBuilder.TileStateOf(probe));
        Assert.Equal(tile.Word(), RuntimePayloadBuilder.StateOf(probe));
    }

    [Fact]
    public void EveryHealthAndProbeStateHasATileStateAndAnUnnamedOneFailsLoud()
    {
        Assert.All(Enum.GetValues<HealthState>(), state => Assert.True(Enum.IsDefined(RuntimePayloadBuilder.TileStateOf(state))));
        Assert.All(Enum.GetValues<ContainerAppState>(), state => Assert.True(Enum.IsDefined(RuntimePayloadBuilder.TileStateOf(state))));
        Assert.Throws<ArgumentOutOfRangeException>(() => RuntimePayloadBuilder.TileStateOf((HealthState)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => RuntimePayloadBuilder.TileStateOf((ContainerAppState)99));
    }
}
