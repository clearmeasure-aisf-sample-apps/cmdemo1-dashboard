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

    /// <summary>The public members the given type declares itself by the rules of <see cref="Of(Type)" />, and its
    /// public fields beside them: the walk of the files whose types carry fixed words as well as parts.</summary>
    public static List<string> WithFieldsOf(Type type) =>
        [.. type
            .GetMembers(Declared)
            .Where(member => member is FieldInfo || IsSurface(member))
            .Where(member => !IsCompilerNamed(member))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The public static members a static class or an enum declares itself: its fields, its properties and
    /// the methods that are not a get or a set of one, in ordinal order and each name once.</summary>
    public static List<string> StaticOf(Type type) =>
        [.. type
            .GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is FieldInfo
                             || member is PropertyInfo
                             || member is MethodInfo method && !method.IsSpecialName)
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The public properties a record declares itself, in the order they stand in it.</summary>
    public static string[] PropertyNamesOf<T>() =>
        [.. typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(property => property.Name)];

    /// <summary>The public properties a record declares itself, by name, so a test can ask one what type it is.</summary>
    public static Dictionary<string, PropertyInfo> PropertiesOf<T>() =>
        typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

    /// <summary>The parts a record's one constructor names, in the order it names them.</summary>
    public static ParameterInfo[] ParametersOf<T>() => typeof(T).GetConstructors().Single().GetParameters();

    /// <summary>The public static fields - the fixed words - a type declares itself, in ordinal order.</summary>
    public static string[] FieldNamesOf<T>() =>
        [.. typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(field => field.Name)
            .Order(StringComparer.Ordinal)];

    /// <summary>True when the signature says a part may be nothing.</summary>
    public static bool IsNullable(ParameterInfo parameter) =>
        new NullabilityInfoContext().Create(parameter).WriteState == NullabilityState.Nullable;

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
