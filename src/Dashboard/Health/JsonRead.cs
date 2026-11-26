using System.Globalization;
using System.Text.Json;

namespace Dashboard.Health;

/// <summary>
/// Reads the optional parts of an answer the dashboard does not own (a node's telemetry and build facts, the delivery
/// file, the cluster's files): a part that is absent or of another type is null, never an error.
/// </summary>
internal static class JsonRead
{
    /// <summary>The object under <paramref name="name"/>; null when there is none.</summary>
    public static JsonElement? Section(JsonElement? parent, string name) =>
        parent is { ValueKind: JsonValueKind.Object } value && value.TryGetProperty(name, out var section) && section.ValueKind == JsonValueKind.Object
            ? section
            : null;

    /// <summary>
    /// The record an object stands for, made by <paramref name="read"/> from the text under <paramref name="identity"/>
    /// and the object itself; null when there is no object or no such text, the one part without which there is no record.
    /// </summary>
    public static T? Identified<T>(JsonElement? parent, string identity, Func<string, JsonElement, T> read)
        where T : class =>
        parent is { } value && Text(value, identity) is { } key ? read(key, value) : null;

    /// <summary>The record an object stands for, made by <paramref name="read"/>; null when there is no object.</summary>
    public static T? Present<T>(JsonElement? parent, Func<JsonElement, T> read)
        where T : class =>
        parent is { ValueKind: JsonValueKind.Object } value ? read(value) : null;

    /// <summary>The elements of the array under <paramref name="name"/>; none when there is no array.</summary>
    public static IEnumerable<JsonElement> Items(JsonElement? parent, string name) =>
        ArrayUnder(parent, name) is { } array ? [.. array.EnumerateArray()] : [];

    /// <summary>
    /// The strings of the list under <paramref name="name"/>, trimmed, the empty ones left out, in the order written;
    /// null without the list, and when it is no strings at all - an empty word is no word, as in <see cref="Text" />.
    /// </summary>
    public static List<string>? Strings(JsonElement? parent, string name)
    {
        if (ArrayUnder(parent, name) is not { } array)
        {
            return null;
        }

        var strings = array.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString()?.Trim())
            .Where(item => !string.IsNullOrEmpty(item))
            .Select(item => item!)
            .ToList();
        return strings.Count == 0 ? null : strings;
    }

    /// <summary>
    /// The names of the list under <paramref name="name"/>, in the order written, kept in the file's own letters -
    /// nothing is trimmed and nothing is cased: an empty word is no name, as in <see cref="Text" />, and what is not
    /// a word is left out. An empty list without the array, or when it names nothing: the list is the reading, and
    /// the page says the names back, so the record it stands for keeps the very list it is handed.
    /// </summary>
    public static List<string> Names(JsonElement? parent, string name) =>
        [.. Items(parent, name).Where(item => item.ValueKind == JsonValueKind.String).Select(item => item.GetString()!).Where(text => text.Length > 0)];

    /// <summary>A number that is not negative; null for anything else.</summary>
    public static double? Number(JsonElement? parent, string name) =>
        Signed(parent, name) is { } real and >= 0 ? real : null;

    /// <summary>Any finite number, also below zero (such as a refund); null for anything else.</summary>
    public static double? Signed(JsonElement? parent, string name) =>
        Real(parent, name) is { } real && double.IsFinite(real) ? real : null;

    /// <summary>True or false; null for anything else.</summary>
    public static bool? Flag(JsonElement? parent, string name) =>
        parent is { ValueKind: JsonValueKind.Object } value && value.TryGetProperty(name, out var flag)
            ? flag.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => null,
            }
            : null;

    /// <summary>
    /// A count: a number that is not negative, rounded to the whole number nearest it - an exact half away from zero,
    /// so 2.5 is 3 and 14.5 is 15, as <see cref="Whole" /> says.
    /// </summary>
    public static int? Count(JsonElement? parent, string name) =>
        Number(parent, name) is { } number && number <= int.MaxValue ? (int)Whole(number) : null;

    /// <summary>
    /// A count as a node's telemetry says it: a number that is not negative, rounded (a half to the even number), and one past the reach of an
    /// <see cref="int" /> - even one too large for a <see cref="double" /> - held at <see cref="int.MaxValue" />
    /// rather than refused as <see cref="Count" /> refuses it.
    /// </summary>
    public static int? SaturatedCount(JsonElement? parent, string name) =>
        Real(parent, name) is { } real and >= 0 ? (int)Math.Min(Math.Round(real), int.MaxValue) : null;

    /// <summary>A large count, such as seconds or lines of code, rounded as <see cref="Count" /> rounds (a half away from zero).</summary>
    public static long? Long(JsonElement? parent, string name) =>
        Number(parent, name) is { } number && number <= long.MaxValue / 2 ? (long)Whole(number) : null;

    /// <summary>
    /// The one rounding rule of <see cref="Count" /> and <see cref="Long" />: the whole number nearest, and an exact half
    /// away from zero - the rounding a reader assumes, not <see cref="Math.Round(double)" />'s half to the even number.
    /// (<see cref="SaturatedCount" />, a node's telemetry, still rounds a half to the even number.)
    /// </summary>
    private static double Whole(double number) => Math.Round(number, MidpointRounding.AwayFromZero);

    /// <summary>Text that is not empty, trimmed.</summary>
    public static string? Text(JsonElement? parent, string name)
    {
        if (parent is not { ValueKind: JsonValueKind.Object } value || !value.TryGetProperty(name, out var text) || text.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var trimmed = text.GetString()?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    public static DateTimeOffset? Time(JsonElement? parent, string name) =>
        DateTimeOffset.TryParse(Text(parent, name), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var time) ? time : null;

    /// <summary>An absolute http or https address: the only kind the dashboard links to.</summary>
    public static Uri? Address(JsonElement? parent, string name) => Address(Text(parent, name));

    /// <summary>The number under <paramref name="name"/> as a <see cref="double" />, infinite when it is too large for one; null for anything else.</summary>
    private static double? Real(JsonElement? parent, string name) =>
        parent is { ValueKind: JsonValueKind.Object } value && value.TryGetProperty(name, out var number) && number.ValueKind == JsonValueKind.Number
            && number.TryGetDouble(out var real)
            ? real
            : null;

    /// <summary>True when the value under <paramref name="name"/> is an array; false when there is no such name or it is not an array.</summary>
    public static bool HasArray(JsonElement? parent, string name) => ArrayUnder(parent, name) is not null;

    /// <summary>
    /// The array under <paramref name="name"/>, empty or not; null when the parent is no object, or the name is absent
    /// or holds anything else. The one place that says what a list of a file is: <see cref="Items" />,
    /// <see cref="Strings" /> and <see cref="HasArray" /> all ask it.
    /// </summary>
    private static JsonElement? ArrayUnder(JsonElement? parent, string name) =>
        parent is { ValueKind: JsonValueKind.Object } value && value.TryGetProperty(name, out var array) && array.ValueKind == JsonValueKind.Array
            ? array
            : null;

    public static Uri? Address(string? text) =>
        Uri.TryCreate(text, UriKind.Absolute, out var address) && (address.Scheme == Uri.UriSchemeHttp || address.Scheme == Uri.UriSchemeHttps)
            ? address
            : null;
}
