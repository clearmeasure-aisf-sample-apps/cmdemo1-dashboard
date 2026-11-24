using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// A shared read of the running assembly: the public static methods it declares, by name, filtered to the
/// namespace the caller names. The three coverage guards (Cluster, Health, Runtime/Work) all read the
/// declarations from source, and all need this one read from the assembly to hold each other to the same
/// standard. An attribute in front of the <c>public static</c>, a return type the source pattern cannot
/// cross, a type written in a form the regex does not know - the source scan would miss them, but the
/// assembly read does not; and the source scan would read internal helpers the assembly read skips, which
/// is the other way a discrepancy shows up. The caller asserts the two sets are equal, both ways.
/// </summary>
public static class SourceScan
{
    /// <summary>
    /// The public static methods the running assembly declares in the namespace the caller names, by their
    /// type and their name. The anchor is a type the caller knows is in that namespace; the assembly is read
    /// from it. Only public types are read, declared only, with no special names - the same rules the
    /// Cluster guard already held, now shared by all three.
    /// </summary>
    /// <param name="anchor">A type in the target namespace, used to find the assembly.</param>
    /// <param name="namespaceName">The namespace to filter to.</param>
    /// <returns>The fully-qualified names of the methods, one per entry, as <c>Type.Name</c>.</returns>
    public static List<string> DeclaredInAssembly(Type anchor, string namespaceName)
    {
        var methods = new List<string>();

        foreach (var type in anchor.Assembly.GetTypes().Where(t => t.IsPublic && t.Namespace == namespaceName))
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
            {
                methods.Add($"{type.Name}.{method.Name}");
            }
        }

        return methods.OrderBy(name => name, StringComparer.Ordinal).ToList();
    }
}
