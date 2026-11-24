using System.Text.Json;

namespace Dashboard.Health;

/// <summary>One deployment task of the system's Octopus space.</summary>
/// <param name="Project">
/// The Octopus project: <c>&lt;slug&gt;-&lt;deployable&gt;</c>, and <c>&lt;slug&gt;-system</c> for the system's own
/// release.
/// </param>
/// <param name="StateText">The state in the file's own word.</param>
/// <param name="Since">When it started, or when it was queued while it has not started; null when the file's time does not parse.</param>
/// <param name="Finished">When it ended; null while it has not, and when the file's time does not parse.</param>
/// <param name="Url">The task in Octopus Deploy; null when the file gives no address.</param>
/// <param name="StartedBy">Who started it, as Octopus Deploy names the account; null when the file does not say.</param>
/// <param name="WaitsFor">What it waits for while it is waiting; null otherwise, and for a file from before the field.</param>
public sealed record Deployment(
    string Project,
    string Environment,
    string Release,
    DeploymentState State,
    string StateText,
    DateTimeOffset? Since,
    DateTimeOffset? Finished,
    Uri? Url,
    string? StartedBy = null,
    DeploymentWait? WaitsFor = null)
{
    /// <summary>
    /// True while it has not ended: queued, executing or waiting. A state the page does not know counts while the
    /// file gives it no end.
    /// </summary>
    public bool InFlight => State switch
    {
        DeploymentState.Queued or DeploymentState.Executing or DeploymentState.Waiting => true,
        DeploymentState.Unknown => Finished is null,
        _ => false,
    };

    /// <summary>
    /// In flight, always; ended, until <see cref="DeploymentsReport.EndedFor"/> after its end. One that ended at a
    /// time the page cannot read is not marked: nothing says for how long it should be.
    /// </summary>
    public bool IsMarkedAt(DateTimeOffset now) => InFlight || (Finished is { } ended && now - ended < DeploymentsReport.EndedFor);

    /// <summary>An entry without a project, an environment, a release or a state is left out.</summary>
    internal static Deployment? Read(JsonElement element) =>
        JsonRead.Text(element, "project") is { } project && JsonRead.Text(element, "environment") is { } environment
            && JsonRead.Text(element, "release") is { } release && JsonRead.Text(element, "state") is { } state
            && Of(state) is var meant
            ? new Deployment(
                project,
                environment,
                release,
                meant,
                state,
                JsonRead.Time(element, "since"),
                JsonRead.Time(element, "finished"),
                JsonRead.Address(element, "url"),
                JsonRead.Text(element, "startedBy"),
                meant == DeploymentState.Waiting ? DeploymentWait.Read(JsonRead.Section(element, "waitsFor")) : null)
            : null;

    /// <summary>The state a word of the file means; <see cref="DeploymentState.Unknown"/> for one the page does not know.</summary>
    internal static DeploymentState Of(string state) => state.ToLowerInvariant() switch
    {
        "queued" => DeploymentState.Queued,
        "executing" => DeploymentState.Executing,
        "waiting" => DeploymentState.Waiting,
        "succeeded" => DeploymentState.Succeeded,
        "failed" => DeploymentState.Failed,
        "canceled" => DeploymentState.Canceled,
        _ => DeploymentState.Unknown,
    };
}

public enum DeploymentState
{
    /// <summary>A state this page does not know: said in the file's own word.</summary>
    Unknown,

    /// <summary>Waiting behind another task of the environment, or for the instance's task limit.</summary>
    Queued,

    /// <summary>Running now.</summary>
    Executing,

    /// <summary>Stopped for a person: the sign-off, or a question of a guided failure.</summary>
    Waiting,

    Succeeded,

    Failed,

    Canceled,
}
