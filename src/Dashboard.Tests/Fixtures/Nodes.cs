namespace Dashboard.Tests;

/// <summary>
/// One home for the node-version fixtures the version tests stand on: the two regional nodes a deployable's version
/// is compared across (<see cref="West" />, <see cref="East" />) and the third region the counts are worked out of
/// (<see cref="Central" />), each a <see cref="NodeVersion" /> of the state and the version named; the number one
/// past the four states a check leaves behind (<see cref="Unnamed" />, a state the enum does not name that a caller
/// can still hand the record); the reading of the pinned-version file the assessments are made against
/// (<see cref="Pinned" />, which pins both of the deployables it names); and the one shorthand the version tests
/// assess with over that reading and the <c>ui</c> deployable (<see cref="Assess" />). The state words themselves are held by
/// <see cref="HealthStates" /> in <c>TestSupport.cs</c>; this mother holds the nodes, the one-past sentinel and the
/// reading that stand on them. The version files used to declare their own copies of the builders, the sentinel and
/// the reading - the same <c>westus3</c>/<c>eastus2</c> builders over <c>(state, version)</c>, the same
/// <c>(HealthState)4</c> sentinel and the same <c>dashboard</c>/<c>ui</c> reading - and <see cref="NodesObjectMotherTests" />
/// holds these answers to the record and the reading they say.
/// </summary>
public static class Nodes
{
    /// <summary>The number one step past the four states a check leaves behind: no state of the enum, and one a
    /// caller can still hand the record.</summary>
    public const HealthState Unnamed = (HealthState)4;

    /// <summary>The reading of the pinned-version file the version assessments are made against: <c>dashboard</c>
    /// pinned to 1.0.1 and <c>ui</c> pinned to 2.4.7, as the reader parses the file.</summary>
    public static readonly PinnedVersions Pinned = PinnedVersions.Parse("""{ "dashboard": "1.0.1", "ui": "2.4.7" }""");

    /// <summary>The assessment of the <see cref="Pinned" /> reading for the <c>ui</c> deployable over the nodes a
    /// test hands it: the one shorthand the version tests assess with, so a moved pin is written once and a test
    /// that reads it says which reading it means.</summary>
    public static VersionAssessment Assess(params NodeVersion[] nodes) => VersionAssessment.Assess(Pinned, "ui", nodes);

    /// <summary>A <see cref="NodeVersion" /> of the westus3 region, the state and the version named.</summary>
    public static NodeVersion West(HealthState state, string? version) => new("westus3", state, version);

    /// <summary>A <see cref="NodeVersion" /> of the eastus2 region, the state and the version named.</summary>
    public static NodeVersion East(HealthState state, string? version) => new("eastus2", state, version);

    /// <summary>A <see cref="NodeVersion" /> of the centralus region, the state and the version named.</summary>
    public static NodeVersion Central(HealthState state, string? version) => new("centralus", state, version);
}
