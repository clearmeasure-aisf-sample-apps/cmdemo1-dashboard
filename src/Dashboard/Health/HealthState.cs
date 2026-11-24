namespace Dashboard.Health;

public enum HealthState
{
    /// <summary>Not checked yet.</summary>
    Pending,

    /// <summary>The endpoint answered HTTP 200.</summary>
    Healthy,

    /// <summary>The endpoint answered with any other HTTP status.</summary>
    Unhealthy,

    /// <summary>The browser got no answer: network failure, CORS refusal or timeout.</summary>
    Unreachable,
}
