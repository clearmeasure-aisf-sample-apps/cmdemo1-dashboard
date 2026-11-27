using System.Reflection;
using System.Text.Json;

namespace Dashboard.Health;

/// <summary>Versions as the dashboard shows them: <c>2.4.21+abc123</c> becomes <c>2.4.21</c>.</summary>
public static class VersionText
{
    /// <summary>The SemVer build-metadata mark that cuts the version: the <c>+</c> in <c>2.4.21+abc123</c>.</summary>
    private const char BuildMetadataMark = '+';

    /// <summary>The word the version endpoint answers with its version: the property name in <c>{"version":"2.4.21"}</c>.</summary>
    private const string VersionProperty = "version";

    /// <summary>The word the header says when the dashboard's build carries no version.</summary>
    private const string Unknown = "unknown";

    /// <summary>The part of a version before the build metadata (<c>+</c>); null when nothing is left.</summary>
    public static string? Display(string? version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return null;
        }

        var plus = version.IndexOf(BuildMetadataMark, StringComparison.Ordinal);
        var text = (plus >= 0 ? version[..plus] : version).Trim();
        return text.Length == 0 ? null : text;
    }

    /// <summary>The shown version of <paramref name="parent"/>'s <c>version</c> word (see <see cref="Display"/>); null when the object gives none.</summary>
    public static string? Read(JsonElement? parent) => Display(JsonRead.Text(parent, VersionProperty));

    /// <summary>Reads the body of a version endpoint: JSON <c>{"version":"2.4.21+sha"}</c>.</summary>
    public static string? FromVersionResponse(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in root.EnumerateObject())
                {
                    if (string.Equals(property.Name, VersionProperty, StringComparison.OrdinalIgnoreCase)
                        && property.Value.ValueKind == JsonValueKind.String)
                    {
                        return Display(property.Value.GetString());
                    }
                }
            }

            return root.ValueKind == JsonValueKind.String ? Display(root.GetString()) : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>The version of the dashboard itself: the assembly's informational version, set by <c>-p:Version</c>.</summary>
    public static string OfAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        return Display(informational) ?? Unknown;
    }
}
