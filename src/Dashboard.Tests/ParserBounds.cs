using System.Reflection;
using System.Text;

namespace Dashboard.Tests;

/// <summary>
/// The three bounds of <see cref="ApplicationParser" /> - the file the application view reads - written once in the
/// test project and named by what they decide, with the builders that stand a file at them. The parser keeps them
/// private; <see cref="Held" /> reads one back by its name, so a test can pin the number here against the class.
/// </summary>
internal static class ParserBounds
{
    /// <summary>The longest word of the file that is kept whole: a longer one is cut to it.</summary>
    public const int LongestWord = 300;

    /// <summary>The most assemblies of one file that are kept: the ones after them are not read.</summary>
    public const int MostAssemblies = 2000;

    /// <summary>The most references and the most packages of one assembly that are kept.</summary>
    public const int MostEntries = 1000;

    /// <summary>How far past a bound "way past it" is: a hundred times the bound.</summary>
    public const int WayPast = 100;

    /// <summary>The name of each bound as <see cref="ApplicationParser" /> spells its constant.</summary>
    public static IReadOnlyDictionary<string, int> ByProductionName { get; } = new Dictionary<string, int>
    {
        ["LongestText"] = LongestWord,
        ["MostAssemblies"] = MostAssemblies,
        ["MostEntries"] = MostEntries,
    };

    /// <summary>The value the parser holds under the given constant's name.</summary>
    public static int Held(string name) =>
        (int)typeof(ApplicationParser)
            .GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)!
            .GetRawConstantValue()!;

    /// <summary>The counts a test asks a bound at: one under it, at it, one over it and a hundred times over.</summary>
    public static int[] Around(int bound) => [bound - 1, bound, bound + 1, bound * WayPast];

    /// <summary>What the parser keeps of <paramref name="count" /> when it keeps at most <paramref name="bound" />.</summary>
    public static int Kept(int count, int bound) => Math.Min(count, bound);

    /// <summary>A word of the given number of letters.</summary>
    public static string AWord(int letters) => new('x', letters);

    /// <summary>A file whose process is a word of the given number of letters.</summary>
    public static string AFileWithAProcessOf(int letters) => $$"""{ "process": "{{AWord(letters)}}" }""";

    /// <summary>A file of one assembly whose name and only reference are words of the given number of letters.</summary>
    public static string AFileWithAnAssemblyNamedBy(int letters) =>
        $$"""{ "assemblies": [ { "name": "{{AWord(letters)}}", "references": [ "{{AWord(letters)}}" ] } ] }""";

    /// <summary>A file that lists the given number of assemblies, each named <c>A&lt;number&gt;</c>.</summary>
    public static string AFileOfAssemblies(int assemblies) =>
        Json("""{ "assemblies": [""", Enumerable.Range(0, assemblies).Select(number => $$"""{"name":"A{{number}}"}"""), "] }");

    /// <summary>A file of one assembly that references the given number of others.</summary>
    public static string AFileWithReferences(int references) =>
        Json("""{ "assemblies": [ { "name": "Ui", "references": [""", Enumerable.Range(0, references).Select(number => $"\"R{number}\""), "] } ] }");

    /// <summary>A file of one assembly that uses the given number of packages.</summary>
    public static string AFileWithPackages(int packages) =>
        Json("""{ "assemblies": [ { "name": "Ui", "packages": [""", Enumerable.Range(0, packages).Select(number => $$"""{"name":"P{{number}}"}"""), "] } ] }");

    /// <summary>An architecture, as the parser gives it, of the given number of assemblies and no entry.</summary>
    public static ApplicationArchitecture AnArchitectureOf(int assemblies) =>
        new(null, null, null, [.. Enumerable.Range(0, assemblies).Select(number => new ApplicationAssembly($"A{number}", Entry: false, [], []))], false);

    private static string Json(string start, IEnumerable<string> items, string end) =>
        new StringBuilder(start).AppendJoin(',', items).Append(end).ToString();
}
