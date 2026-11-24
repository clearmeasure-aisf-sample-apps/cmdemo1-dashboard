using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The one walk over a record that says what its public surface is, so every <c>*EdgeCaseTests</c> file keeps only the
/// list it pins and not its own copy of the machinery that reads the type. When the rules of what counts change - a new
/// compiler-written member, an operator named differently - they change here, once.
/// </summary>
internal static class RecordSurface
{
    private const BindingFlags Declared =
        BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    /// <summary>The public members a record declares itself: its parts and the methods that are not a get or a set of one,
    /// the two operators kept, and the compiler's clone and print walk past.</summary>
    public static List<string> Of<T>() => Of(typeof(T));

    /// <summary>The public members the given type declares itself, by the same rules as <see cref="Of{T}" />, in ordinal
    /// order and each name once.</summary>
    public static List<string> Of(Type type) =>
        [.. type
            .GetMembers(Declared)
            .Where(IsSurface)
            .Where(member => !IsCompilerNamed(member))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>Both positional parts are read once and written only as the record is made.</summary>
    public static void AssertPartsAreInitOnly(Type type, params string[] names)
    {
        Assert.NotEmpty(names);
        Assert.All(names, name =>
        {
            var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            Assert.NotNull(property);
            Assert.True(property.CanRead);
            Assert.NotNull(property.SetMethod);
            Assert.Contains(typeof(IsExternalInit), property.SetMethod.ReturnParameter.GetRequiredCustomModifiers());
        });
    }

    /// <summary>A property, or a method that is no accessor - an operator kept, since a caller writes it.</summary>
    private static bool IsSurface(MemberInfo member) =>
        member is PropertyInfo
        || member is MethodInfo method && (!method.IsSpecialName || IsOperator(method));

    private static bool IsOperator(MethodInfo method) => method.Name.StartsWith("op_", StringComparison.Ordinal);

    /// <summary>The members the compiler names with a <c>&lt;</c> - the clone method - that no caller can write.</summary>
    private static bool IsCompilerNamed(MemberInfo member) => member.Name.Contains('<', StringComparison.Ordinal);
}
