namespace Dashboard.Health;

/// <summary>
/// The paths the page's checks ask, owned once: <see cref="DefaultHealthPath"/> is the one word
/// <see cref="DeployableInfo.DefaultHealthPath"/> and <see cref="ContainerAppTarget.DefaultHealthPath"/> both stand
/// on, so a deployable file that names no health path and a container app nobody declared are asked at the same
/// path by construction, and a rewording changes both at once rather than leaving one of them asking a path the
/// app does not serve.
/// </summary>
public static class HealthPaths
{
    /// <summary>The health path a deployable or a container app is asked on when it names none of its own: <c>/_healthcheck</c>.</summary>
    public const string DefaultHealthPath = "/_healthcheck";
}
