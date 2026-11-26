using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The two length bounds <see cref="ApplicationText" /> holds a deployable's name and a version to, asked of the class
/// by the very words it uses - <c>LongestName</c> and <c>LongestVersion</c> - rather than spelled again as numbers in
/// the tests (#1388). The constants are read public or private, so the tests keep asking the same thing the day the
/// class states them publicly. <see cref="Name" /> and <see cref="Version" /> build a word of as many letters as asked,
/// the simplest the guards allow (all digits), so a test at either side of a bound says only the bound.
/// </summary>
public static class ApplicationBounds
{
    /// <summary>The longest deployable name the address takes, as <see cref="ApplicationText" /> declares it.</summary>
    public static int LongestName => Bound(nameof(LongestName));

    /// <summary>The longest version that may name a file, as <see cref="ApplicationText" /> declares it.</summary>
    public static int LongestVersion => Bound(nameof(LongestVersion));

    /// <summary>The static field of <see cref="ApplicationText" /> of the given name, public or private, or <c>null</c>.</summary>
    public static FieldInfo? Field(string name) =>
        typeof(ApplicationText).GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

    /// <summary>A name of exactly <paramref name="letters" /> digits.</summary>
    public static string Name(int letters) => new('7', letters);

    /// <summary>A version of exactly <paramref name="letters" /> digits.</summary>
    public static string Version(int letters) => new('7', letters);

    private static int Bound(string name) =>
        (int)(Field(name)?.GetRawConstantValue()
            ?? throw new InvalidOperationException($"ApplicationText has no static constant {name}."));
}
