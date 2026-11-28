using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// Every bound of the three readers of the runtime folder - <see cref="DatabaseParser" />, <see cref="ApplicationParser" />
/// and <see cref="RuntimeManifestParser" /> - named once in the test project, with one nested shape per reader and the
/// value each holds. The readers keep most of them private, so <see cref="HeldBy" /> reads one back by its name and
/// <see cref="BoundsOf" /> lists every <c>int</c> constant a reader states, whether private or public.
/// </summary>
internal static class RuntimeReaderBounds
{
    /// <summary>The bounds of <see cref="DatabaseParser" />, the reader of the database files.</summary>
    public static class Database
    {
        /// <summary>The most tables of one file that are kept.</summary>
        public const int MostTables = 2000;

        /// <summary>The most columns of one table that are kept.</summary>
        public const int MostColumns = 1000;
    }

    /// <summary>The bounds of <see cref="ApplicationParser" />, the reader of the application file.</summary>
    public static class Application
    {
        /// <summary>The most assemblies of one file that are kept.</summary>
        public const int MostAssemblies = ParserBounds.MostAssemblies;

        /// <summary>The most references and the most packages of one assembly that are kept.</summary>
        public const int MostEntries = ParserBounds.MostEntries;

        /// <summary>The longest text of the file that is kept whole.</summary>
        public const int LongestText = ParserBounds.LongestWord;
    }

    /// <summary>The bounds of <see cref="RuntimeManifestParser" />, the reader of the runtime manifest.</summary>
    public static class Manifest
    {
        /// <summary>The longest text kept of an entry of <c>unplaced</c>.</summary>
        public const int LongestUnplacedText = RuntimeManifestParser.LongestUnplacedText;

        /// <summary>The most entries of <c>unplaced</c> that are kept.</summary>
        public const int MostUnplaced = RuntimeManifestParser.MostUnplaced;

        /// <summary>The longest https address that is kept.</summary>
        public const int LongestAddress = 2000;

        /// <summary>The longest health path that is kept.</summary>
        public const int LongestPath = 200;

        /// <summary>The most ids of one box that are kept.</summary>
        public const int MostIds = 200;

        /// <summary>The longest id kept of an entry of <c>unplaced</c>: twice the longest text.</summary>
        public const int LongestUnplacedId = LongestUnplacedText * 2;
    }

    /// <summary>Each reader with the nested shape that names its bounds.</summary>
    public static IReadOnlyDictionary<Type, Type> ShapeOf { get; } = new Dictionary<Type, Type>
    {
        [typeof(DatabaseParser)] = typeof(Database),
        [typeof(ApplicationParser)] = typeof(Application),
        [typeof(RuntimeManifestParser)] = typeof(Manifest),
    };

    /// <summary>The value <paramref name="reader" /> holds under the given constant's name.</summary>
    public static int HeldBy(Type reader, string name) =>
        (int)reader
            .GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)!
            .GetRawConstantValue()!;

    /// <summary>Every <c>int</c> constant <paramref name="type" /> states, private or public, by name.</summary>
    public static IReadOnlyDictionary<string, int> BoundsOf(Type type) =>
        type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(int))
            .ToDictionary(field => field.Name, field => (int)field.GetRawConstantValue()!, StringComparer.Ordinal);
}
