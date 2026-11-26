namespace Dashboard.Runtime;

/// <summary>
/// The words the deployment writes for a <see cref="RuntimeFrame"/>'s <see cref="RuntimeFrame.Kind"/>: the one
/// vocabulary of the frame's word, held here so the reader, the box link and the doc comments say it in one place.
/// The word is compared as it is written (ordinal, case-sensitive), as the deployment writes it; a word the
/// vocabulary does not know - one of another casing, with a space on it, or the empty word the parser gives a frame
/// that names no kind - is linked all the same, with the frame's name alone in the link's title
/// (<see cref="RuntimeBoxLink"/>). <see cref="Runtime"/> is one the deployment writes, and it is said with the
/// frame's name alone, as a word the vocabulary does not know.
/// </summary>
public static class RuntimeFrameKinds
{
    /// <summary>A subscription: the box's portal title is the words alone, with no name after them.</summary>
    public const string Subscription = "subscription";

    /// <summary>A resource group: the box's portal title is the words and the group's name.</summary>
    public const string ResourceGroup = "resourceGroup";

    /// <summary>A Front Door profile: the box's portal title is the words and the profile's name.</summary>
    public const string FrontDoorProfile = "frontDoorProfile";

    /// <summary>An App Service plan: the box's portal title is the words and the plan's name.</summary>
    public const string Plan = "plan";

    /// <summary>An AKS cluster: the box's portal title is the words and the cluster's name.</summary>
    public const string Cluster = "cluster";

    /// <summary>The boundary of an application with its own runtime: <see cref="RuntimeBoxLink"/> says it with the
    /// frame's name alone, as a word the vocabulary does not know.</summary>
    public const string Runtime = "runtime";
}
