using System.Net;
using Dashboard.Health;

namespace Dashboard.Runtime;

/// <summary>Loads the files of <c>runtime/</c> from the dashboard's own address, as it loads <c>topology.json</c>.</summary>
public sealed class RuntimeLoader(HttpClient http)
{
    public const string Folder = "runtime/";
    public const string IndexFile = "index.json";

    public async Task<RuntimeIndexResult> LoadIndexAsync(CancellationToken cancellationToken)
    {
        var (status, text, error) = await GetAsync(IndexFile, cancellationToken);
        if (status == HttpStatusCode.NotFound || IsServedByTheFallbackRoute(status, text))
        {
            return RuntimeIndexResult.NotDeployed;
        }

        if (error is not null)
        {
            return RuntimeIndexResult.Failed([error]);
        }

        var parsed = RuntimeManifestParser.ParseIndex(text);
        return parsed.Value is { } index
            ? RuntimeIndexResult.Read(index)
            : RuntimeIndexResult.Failed(Prefixed(IndexFile, parsed.Errors));
    }

    public async Task<RuntimeDiagram> LoadDiagramAsync(RuntimeIndexEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);
        var read = await ReadPairAsync(entry.Manifest, entry.Svg, RuntimeManifestParser.ParseManifest, cancellationToken);
        return new RuntimeDiagram(entry.Name, read.Manifest, read.Svg, read.Errors);
    }

    public async Task<DatabaseDiagram> LoadDatabaseAsync(RuntimeDatabaseFiles files, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(files);
        var read = await ReadPairAsync(files.Manifest, files.Svg, DatabaseParser.ParseManifest, cancellationToken);
        return new DatabaseDiagram(read.Manifest, read.Svg, read.Errors);
    }

    /// <summary>
    /// The live file of a database view, from another origin. It may be absent, refused or late: then there is none,
    /// and the view keeps what it shows. Only the caller's cancellation throws.
    /// </summary>
    public async Task<DatabaseLive?> ReadLiveDatabaseAsync(Uri source, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        using var timeout = new CancellationTokenSource(NodeProber.DefaultTimeout);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        try
        {
            using var request = NodeProber.NewRequest(source);
            using var response = await http.SendAsync(request, linked.Token);
            return response.IsSuccessStatusCode ? DatabaseParser.ParseLive(await response.Content.ReadAsStringAsync(linked.Token)) : null;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    /// <summary>
    /// The assemblies of one version of a deployable, from its repository (another origin). HTTP 404 says that the
    /// repository published none of that version; anything else that is no JSON object could not be read. Only the
    /// caller's cancellation throws.
    /// </summary>
    public async Task<ApplicationRead> ReadArchitectureAsync(Uri address, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(address);
        using var timeout = new CancellationTokenSource(NodeProber.DefaultTimeout);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        try
        {
            using var request = NodeProber.NewRequest(address);
            using var response = await http.SendAsync(request, linked.Token);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return ApplicationRead.Missing;
            }

            return response.IsSuccessStatusCode && ApplicationParser.Parse(await response.Content.ReadAsStringAsync(linked.Token)) is { } architecture
                ? new ApplicationRead(ApplicationReadState.Read, architecture)
                : ApplicationRead.Failed;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return ApplicationRead.Failed;
        }
        catch (HttpRequestException)
        {
            return ApplicationRead.Failed;
        }
    }

    /// <summary>
    /// A manifest and its SVG, both asked for at once: each file that cannot be read, parsed or is no SVG adds its
    /// reasons, in that order (the manifest's first).
    /// </summary>
    private async Task<RuntimeFilesRead<TManifest>> ReadPairAsync<TManifest>(
        string manifestFile, string svgFile, Func<string?, RuntimeParseResult<TManifest>> parse, CancellationToken cancellationToken)
        where TManifest : class
    {
        var manifestTask = GetAsync(manifestFile, cancellationToken);
        var svgTask = GetAsync(svgFile, cancellationToken);
        var (_, manifestText, manifestError) = await manifestTask;
        var (_, svg, svgError) = await svgTask;

        var errors = new List<string>();
        TManifest? manifest = null;
        if (manifestError is not null)
        {
            errors.Add(manifestError);
        }
        else
        {
            var parsed = parse(manifestText);
            manifest = parsed.Value;
            errors.AddRange(Prefixed(manifestFile, parsed.Errors));
        }

        if (svgError is not null)
        {
            errors.Add(svgError);
        }
        else if (!IsSvg(svg))
        {
            errors.AddRange(Prefixed(svgFile, ["not an SVG."]));
        }

        return RuntimeFilesRead<TManifest>.Of(manifest, svg, errors);
    }

    /// <summary>The text of a file of <c>runtime/</c> is a picture only when it holds an <c>&lt;svg</c> element.</summary>
    private static bool IsSvg(string? text) => text is not null && text.Contains("<svg", StringComparison.Ordinal);

    /// <summary>Each problem of a file of <c>runtime/</c>, said with the file's path in front.</summary>
    private static IReadOnlyList<string> Prefixed(string file, IEnumerable<string> problems) =>
        [.. problems.Select(problem => $"{Folder}{file}: {problem}")];

    /// <summary>
    /// A host that answers every unknown address with the page itself (a fallback route) serves no JSON either.
    /// </summary>
    private static bool IsServedByTheFallbackRoute(HttpStatusCode? status, string? text) =>
        status == HttpStatusCode.OK && text is not null && text.TrimStart().StartsWith('<');

    /// <summary>
    /// The status and the text of a file of <c>runtime/</c>, a file of the dashboard's own site read through
    /// <see cref="ProbeUrl.ResolveRelative"/>; an error text when it could not be read, or when its path names no file
    /// of this site (then nothing is asked for, and the file is reported as bad).
    /// </summary>
    private async Task<(HttpStatusCode? Status, string? Text, string? Error)> GetAsync(string file, CancellationToken cancellationToken)
    {
        var path = Folder + file;
        var address = ProbeUrl.ResolveRelative(http.BaseAddress, path);
        if (address is null)
        {
            return (null, null, $"{path} is no path of this site.");
        }

        try
        {
            using var request = NodeProber.NewRequest(address);
            using var response = await http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return (response.StatusCode, null, $"The server answered HTTP {(int)response.StatusCode} for {path}.");
            }

            return (response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken), null);
        }
        catch (HttpRequestException exception)
        {
            return (null, null, $"{path} could not be loaded: {exception.Message}");
        }
    }
}
