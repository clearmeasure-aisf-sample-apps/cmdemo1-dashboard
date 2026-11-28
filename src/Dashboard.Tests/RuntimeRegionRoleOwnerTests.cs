using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="RuntimeRegionRole.Primary"/> and <see cref="RuntimeRegionRole.Standby"/> are the node role words of
/// <see cref="NodeInfo"/>: the manifest's region roles and the topology's node roles are one spelling.
/// </summary>
public class RuntimeRegionRoleOwnerTests
{
    [Fact]
    public void ThePrimaryAndStandbyRegionRolesAreTheNodeRoleWords()
    {
        Assert.Equal(NodeInfo.PrimaryRole, RuntimeRegionRole.Primary);
        Assert.Equal(NodeInfo.StandbyRole, RuntimeRegionRole.Standby);
    }
}
