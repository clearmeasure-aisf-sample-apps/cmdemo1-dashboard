namespace Dashboard.Runtime;

/// <summary>The look of a node's box in the runtime view; written into <see cref="RuntimeTile.State"/> by <see cref="RuntimeStateWords.Word(TileState)"/>.</summary>
public enum TileState
{
    Healthy,
    Unhealthy,
    Unreachable,
    Checking,

    /// <summary>Not probed: the box says in words why.</summary>
    Neutral,
}

/// <summary>The look of a relationship's arrow in the runtime view; written into <see cref="RuntimeEdgeMark.State"/> by <see cref="RuntimeStateWords.Word(EdgeState)"/>.</summary>
public enum EdgeState
{
    /// <summary>Carries the traffic.</summary>
    Active,
    Idle,
    Down,
    Checking,
    Neutral,
}

/// <summary>
/// The words of the tile and edge states at the JSON boundary, the words <c>js/runtime.js</c> draws by: one arm per
/// state so a new state fails the build, and a value the enum does not name fails loud.
/// </summary>
public static class RuntimeStateWords
{
    public const string Active = "active";
    public const string Idle = "idle";
    public const string Down = "down";

    /// <summary>The word of a tile's state, as the payload carries it.</summary>
    public static string Word(this TileState state) => state switch
    {
        TileState.Healthy => RuntimePayloadBuilder.Healthy,
        TileState.Unhealthy => RuntimePayloadBuilder.Unhealthy,
        TileState.Unreachable => RuntimePayloadBuilder.Unreachable,
        TileState.Checking => RuntimePayloadBuilder.Checking,
        TileState.Neutral => RuntimePayloadBuilder.Neutral,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
    };

    /// <summary>The word of an arrow's state, as the payload carries it.</summary>
    public static string Word(this EdgeState state) => state switch
    {
        EdgeState.Active => Active,
        EdgeState.Idle => Idle,
        EdgeState.Down => Down,
        EdgeState.Checking => RuntimePayloadBuilder.Checking,
        EdgeState.Neutral => RuntimePayloadBuilder.Neutral,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
    };

    /// <summary>The sentence an arrow's tooltip says of its state.</summary>
    public static string Sentence(this EdgeState state) => state switch
    {
        EdgeState.Active => "Carries the traffic.",
        EdgeState.Idle => "Idle: no traffic expected.",
        EdgeState.Down => "Down: its end is not healthy.",
        EdgeState.Checking => "Being checked.",
        EdgeState.Neutral => "Not checked.",
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
    };
}
