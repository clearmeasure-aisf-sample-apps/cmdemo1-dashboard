namespace Dashboard.Runtime;

/// <summary>A resource of the system's resource groups that no environment tag places. Every string is untrusted text.</summary>
/// <param name="Name">The resource's name.</param>
/// <param name="Type">What it is, in words; null when the manifest does not say.</param>
/// <param name="Group">Its resource group.</param>
/// <param name="Words">Why it is in no environment: <c>no environment tag</c>, or <c>tagged zzz: no such environment</c>.</param>
/// <param name="Ids">Its resource ids, by which the page finds what Resource Health said of it; null without one.</param>
/// <param name="Portal">Its page in the Azure portal; null without an absolute https address.</param>
/// <param name="Site">
/// Its own public address (<c>links.site</c>, else <c>url</c>); null without an absolute https address.
/// </param>
public sealed record RuntimeUnplaced(
    string Name,
    string? Type = null,
    string? Group = null,
    string? Words = null,
    IReadOnlyList<string>? Ids = null,
    Uri? Portal = null,
    Uri? Site = null);
