using System.Globalization;
using Dashboard.Health;

namespace Dashboard.Work;

/// <summary>
/// Where the board was read from and what a release carries: the header's repo and project line, and the
/// <c>carries #12, #15</c> line of a version. <see cref="WorkItemsText"/> is the page's door to these.
/// </summary>
public static class WorkScopeText
{
    public static WorkScopeLine? Scope(WorkItemsScope? scope)
    {
        if (scope is null)
        {
            return null;
        }

        var url = JsonRead.Address(scope.Repo);
        var label = url is null ? scope.Repo : RepoLabel(url);
        var project = scope.Project is { } number ? number.ToString(CultureInfo.InvariantCulture) : null;
        return label is null && project is null ? null : new WorkScopeLine(label, url, project);
    }

    /// <summary>
    /// <c>carries #12, #15</c>, the work items of the release this version is, lowest number first. Null when the
    /// work items file names no such release, or the release carries none. The <c>+</c> build metadata of a version
    /// is ignored, as it is everywhere else on the page.
    /// </summary>
    public static string? Carries(WorkItemsReport? report, string? version)
    {
        var shown = VersionText.Display(version);
        if (report is null || shown is null)
        {
            return null;
        }

        var numbers = report.Releases
            .Where(release => string.Equals(VersionText.Display(release.Version), shown, StringComparison.OrdinalIgnoreCase))
            .SelectMany(release => release.Items)
            .Distinct()
            .Order()
            .ToList();
        return numbers.Count == 0 ? null : "carries " + string.Join(", ", numbers.Select(number => $"#{number.ToString(CultureInfo.InvariantCulture)}"));
    }

    private static string RepoLabel(Uri url)
    {
        var path = url.AbsolutePath.Trim('/');
        return path.Length > 0 ? path : url.Host;
    }
}
