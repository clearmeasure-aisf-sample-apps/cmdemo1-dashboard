using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The shared probe wait, as the tests read it: the one value <see cref="NodeProber" />,
/// <see cref="PinnedVersionsReader" /> and <see cref="ClusterReader" /> all hold by default -
/// <c>ProbeDefaults.DefaultTimeout</c>. The class that names it is internal to an app that declares no
/// <c>InternalsVisibleTo</c>, so this reaches it by name under <see cref="NodeProber" />'s assembly, the way
/// <see cref="ProbeDefaultsEdgeCaseTests" /> reaches it.
/// </summary>
internal static class ProbeWait
{
    /// <summary>
    /// The shared wait the probes hold by default, read off <c>ProbeDefaults.DefaultTimeout</c> - the field the
    /// app names in the internal class that holds it, the one <see cref="ProbeDefaultsEdgeCaseTests" /> pins by
    /// name and <see cref="ProbeWaitSupportTests" /> pins through this reader.
    /// </summary>
    public static TimeSpan SharedWait()
    {
        var type = typeof(NodeProber).Assembly.GetType("Dashboard.Health.ProbeDefaults", throwOnError: true)!;
        var field = type.GetField("DefaultTimeout", BindingFlags.Public | BindingFlags.Static)!;
        return (TimeSpan)field.GetValue(null)!;
    }
}
