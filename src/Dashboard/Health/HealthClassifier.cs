namespace Dashboard.Health;

public static class HealthClassifier
{
    private const int Ok = 200;

    /// <summary>Healthy is HTTP 200 and nothing else; every other status is an answer, but not a healthy one.</summary>
    public static HealthState FromStatusCode(int statusCode) =>
        statusCode == Ok ? HealthState.Healthy : HealthState.Unhealthy;

    /// <summary>The state's label on a badge, one arm per state so a state the enum does not name fails loud.</summary>
    public static string Label(HealthState state) => state switch
    {
        HealthState.Pending => "Checking",
        HealthState.Healthy => "Healthy",
        HealthState.Unhealthy => "Unhealthy",
        HealthState.Unreachable => "Unreachable",
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
    };

    /// <summary>The state in the words of a sentence: the lowercase of the label, except the unchecked one, which is not checking.</summary>
    public static string Sentence(HealthState? state) => state switch
    {
        HealthState.Healthy => "healthy",
        HealthState.Unhealthy => "unhealthy",
        HealthState.Unreachable => "unreachable",
        _ => "not checked",
    };
}
