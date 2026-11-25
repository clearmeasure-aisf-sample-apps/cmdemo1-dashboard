namespace Dashboard.Runtime;

/// <summary>
/// The tones a runtime tile line (<see cref="RuntimeTileLine.Tone"/>) is written with: <c>js/runtime.js</c> dresses
/// the line in the class <c>rt-line--{tone}</c> and draws the icon <c>LINE_ICON[tone]</c> before it. An activity line
/// carries its <see cref="Health.ActivityKind"/> word instead, and is not named here. <see cref="Plain"/> and
/// <see cref="Muted"/> spell the same words as <see cref="Health.HealthTone"/> but are kept apart on purpose: Resource
/// Health's two words say how fresh a reading is, these are the page's line classes, and the two sets change apart.
/// </summary>
public static class RuntimeLineTone
{
    /// <summary>An ordinary line: drawn as it is, with no icon.</summary>
    public const string Plain = "plain";

    /// <summary>A quiet line (a host, a count, something not read yet): drawn dimmer, with no icon.</summary>
    public const string Muted = "muted";

    /// <summary>The running version: drawn larger and bolder, with no icon.</summary>
    public const string Strong = "strong";

    /// <summary>A line about the node that serves the traffic: drawn with the serving icon.</summary>
    public const string Serving = "serving";

    /// <summary>A line that says all is well: drawn with the ok icon.</summary>
    public const string Ok = "ok";

    /// <summary>A line that says something is wrong: drawn with the warning icon.</summary>
    public const string Warn = "warn";

    /// <summary>The running version matches the pinned one: drawn with the dashboard's =.</summary>
    public const string InSync = "insync";

    /// <summary>The running version differs from the pinned one: drawn with the dashboard's ≠.</summary>
    public const string Differs = "differs";

    /// <summary>The comparison with the pinned version is not known: drawn with the dashboard's dots.</summary>
    public const string Unknown = "unknown";
}
