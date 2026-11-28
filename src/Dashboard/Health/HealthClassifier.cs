namespace Dashboard.Health;

public static class HealthClassifier
{
    private const int Ok = 200;

    /// <summary>The word for <see cref="HealthState.Pending" /> on a badge.</summary>
    private const string CheckingWord = "Checking";

    /// <summary>The dashboard's badge word for <see cref="HealthState.Healthy" />: the single copy; the detailed-health and container-app badges read it from here.</summary>
    internal const string HealthyWord = "Healthy";

    /// <summary>The dashboard's badge word for <see cref="HealthState.Unhealthy" />: the single copy; the detailed-health and container-app badges read it from here.</summary>
    internal const string UnhealthyWord = "Unhealthy";

    /// <summary>The word for <see cref="HealthState.Unreachable" />.</summary>
    private const string UnreachableWord = "Unreachable";

    /// <summary>The sentence's word for a state not yet checked: not the lowercase of <see cref="CheckingWord" />.</summary>
    private const string NotCheckedWord = "not checked";

    /// <summary>Healthy is HTTP 200 and nothing else; every other status is an answer, but not a healthy one.</summary>
    public static HealthState FromStatusCode(int statusCode) =>
        statusCode == Ok ? HealthState.Healthy : HealthState.Unhealthy;

    /// <summary>The state's label on a badge, one arm per state so a state the enum does not name fails loud.</summary>
    public static string Label(HealthState state) => state switch
    {
        HealthState.Pending => CheckingWord,
        HealthState.Healthy => HealthyWord,
        HealthState.Unhealthy => UnhealthyWord,
        HealthState.Unreachable => UnreachableWord,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
    };

    /// <summary>The state in the words of a sentence: the lowercase of the label, except the unchecked one, which is not checking.</summary>
    public static string Sentence(HealthState? state) => state switch
    {
        HealthState.Healthy or HealthState.Unhealthy or HealthState.Unreachable => Label(state.Value).ToLowerInvariant(),
        _ => NotCheckedWord,
    };
}
