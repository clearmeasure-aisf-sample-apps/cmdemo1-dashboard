using System.Net;

namespace Dashboard.Tests;

/// <summary>
/// The one way a test hands a <see cref="RuntimeLoader"/> a site to read: the dashboard's own address, which every
/// path of <c>runtime/</c> is resolved against by <see cref="ProbeUrl.ResolveRelative"/>, and the answers the test
/// decides. #1684 gathered two copies of this - <see cref="RuntimeLoaderTests"/>'s <c>Loader</c> and <c>Serve</c>,
/// <see cref="RuntimeFilesPairTests"/>'s <c>Loader</c> and <c>Serving</c> - which said the same base address and the
/// same <c>/runtime/</c> routing in words that had already drifted apart.
/// <para>
/// The routings are data rather than a switch: a caller names the files it wants answered and everything else the
/// site is asked for is 404, the answer a site with no <c>runtime/</c> folder gives - which is itself a thing the
/// loader has to read as "not deployed" and not as a failure.
/// </para>
/// </summary>
internal static class RuntimeLoaders
{
    /// <summary>The dashboard's own site, the address the client is built on: a file named <c>uat.svg</c> is asked
    /// for at <c>/runtime/uat.svg</c> of this site and of no other.</summary>
    internal const string Site = "https://dashboard.example.net/";

    /// <summary>The absolute path a file of <c>runtime/</c> is asked for at, built from the folder the app itself
    /// names - the address <see cref="ProbeUrl.ResolveRelative"/> gives for the file, and the routing answers at.</summary>
    internal static string RuntimePath(string file) => "/" + RuntimeLoader.Folder + file;

    /// <summary>A loader of the dashboard's own site whose every answer the routing named gives.</summary>
    internal static RuntimeLoader From(Func<HttpRequestMessage, HttpResponseMessage> serve) => From(new StubHandler(serve));

    /// <summary>A loader of the dashboard's own site over the handler named - for the test that reads back the
    /// requests the loader made (<see cref="StubHandler.RequestedUrls"/>).</summary>
    internal static RuntimeLoader From(StubHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return new RuntimeLoader(new HttpClient(handler) { BaseAddress = new Uri(Site) });
    }

    /// <summary>The routing of the files named: each answers with its body where the site is asked for it, and
    /// every other address is 404. The paths are absolute, as <see cref="RuntimePath"/> writes them, because that
    /// is what a handler sees - <see cref="HttpRequestMessage.RequestUri"/> resolved against <see cref="Site"/>.</summary>
    internal static Func<HttpRequestMessage, HttpResponseMessage> Serving(params (string Path, string Body)[] files)
    {
        ArgumentNullException.ThrowIfNull(files);
        foreach (var (path, _) in files)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
        }

        return request =>
        {
            var asked = request.RequestUri!.AbsolutePath;
            foreach (var (path, body) in files)
            {
                if (string.Equals(asked, path, StringComparison.Ordinal))
                {
                    return StubHandler.Answer(HttpStatusCode.OK, body);
                }
            }

            return StubHandler.Answer(HttpStatusCode.NotFound);
        };
    }

    /// <summary>The routing of the samples the app ships: each file named is answered at
    /// <c>/runtime/&lt;file&gt;</c> with the text of the shipped file, so a test says which files its site holds
    /// without writing the folder or the sample folder's path.</summary>
    internal static Func<HttpRequestMessage, HttpResponseMessage> ServingSamples(params string[] files)
    {
        ArgumentNullException.ThrowIfNull(files);
        return Serving([.. files.Select(file => (RuntimePath(file), RuntimeManifestParserTests.Sample(file)))]);
    }

    /// <summary>
    /// The routing of one view's pair (#1437): the manifest at the path named, one picture for every SVG asked
    /// for - whatever its name, since a view asks for the SVG its index entry names and the test has one picture
    /// for it - and 404 everywhere else.
    /// </summary>
    internal static Func<HttpRequestMessage, HttpResponseMessage> ServingPair(string manifestPath, string manifest, string svg)
    {
        ArgumentException.ThrowIfNullOrEmpty(manifestPath);
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(svg);

        return request =>
            request.RequestUri!.AbsolutePath == manifestPath ? StubHandler.Answer(HttpStatusCode.OK, manifest)
            : request.RequestUri!.AbsolutePath.EndsWith(".svg", StringComparison.Ordinal) ? StubHandler.Answer(HttpStatusCode.OK, svg)
            : StubHandler.Answer(HttpStatusCode.NotFound);
    }
}
