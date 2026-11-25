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
/// <para>
/// #1706 added the answers a per-file edge case needs and <see cref="Serving" /> cannot give: a file answered with a
/// status of its own and a file that cannot be read at all, both as <see cref="Served" /> entries of
/// <see cref="Routing" />. <see cref="Serving" />, <see cref="ServingSamples" /> and
/// <see cref="ServingPair" /> answer exactly as they did before - the two answers stand beside them, named at the
/// call of a test that needs one.
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

    /// <summary>
    /// The routing of the answers named: each is given where the site is asked for its path - a
    /// <see cref="Served.File" /> answer with the status and the body it carries, a <see cref="Served.Failing" />
    /// answer by the request failing with an <see cref="HttpRequestException" /> of its reason - and every other
    /// address is 404, the answer <see cref="Serving" /> gives too.
    /// </summary>
    /// <remarks>
    /// #1706, for the per-file edge cases a routing of <see cref="Serving" /> has no word for: one file answering
    /// 500, one answered the page at a status that is not the fallback route's own, one that cannot be read while
    /// its pair is fine. Each of those is one more entry here; written any other way each cost a handler of its own,
    /// its own base address and its own routing - the <see cref="StubHandler" /> a test drops to when the fixture has
    /// nowhere to say the one answer it added. It is a separate routing rather than an overload of
    /// <see cref="Serving" /> because two <c>params</c> overloads make <c>Serving()</c> and <c>Serving(null!)</c>,
    /// both already written, ambiguous: <see cref="Serving" /> keeps its answers and its lines as they are.
    /// </remarks>
    internal static Func<HttpRequestMessage, HttpResponseMessage> Routing(params Served[] answers)
    {
        ArgumentNullException.ThrowIfNull(answers);
        foreach (var answer in answers)
        {
            ArgumentException.ThrowIfNullOrEmpty(answer.Path);
        }

        return request =>
        {
            var asked = request.RequestUri!.AbsolutePath;
            foreach (var answer in answers)
            {
                if (string.Equals(asked, answer.Path, StringComparison.Ordinal))
                {
                    return answer.Reply();
                }
            }

            return StubHandler.Answer(HttpStatusCode.NotFound);
        };
    }
}

/// <summary>
/// One answer of the site <see cref="RuntimeLoaders" /> builds (#1706): the path it is answered at, and what
/// answering there does - either the body with a status of its own, or the reason no answer comes at all. The two
/// factories are the two answers a test has to name; the routing asks one question of either, so it says nothing of
/// which of the two it is holding.
/// </summary>
/// <param name="Path">The absolute path the answer stands at, as <see cref="RuntimeLoaders.RuntimePath" /> writes it
/// and as <see cref="HttpRequestMessage.RequestUri" /> shows it to a handler.</param>
/// <param name="Body">The text answered with. Empty of meaning when the answer is a failing one, which answers
/// nothing.</param>
/// <param name="Status">The status answered with, HTTP 200 by the factory unless the test names another. Of no
/// meaning when the answer is a failing one, which answers no status.</param>
/// <param name="Failure">The reason the request fails with an <see cref="HttpRequestException" /> - what a host that
/// drops the request looks like to the loader - or <see langword="null" /> for the answer that does answer.</param>
internal readonly record struct Served(string Path, string Body, HttpStatusCode Status, string? Failure)
{
    /// <summary>The answer of a file the site holds: its body, with the status named - HTTP 200 unless the test says
    /// otherwise, which is the only answer <see cref="RuntimeLoaders.Serving" /> knows how to give.</summary>
    internal static Served File(string path, string body, HttpStatusCode status = HttpStatusCode.OK)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(body);
        return new Served(path, body, status, null);
    }

    /// <summary>The answer of a file that cannot be read: the request asked at the path fails with an
    /// <see cref="HttpRequestException" /> of the reason named, so the loader says its own
    /// <c>could not be loaded</c> words for that one file and keeps answering the rest of the site.</summary>
    internal static Served Failing(string path, string reason)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentException.ThrowIfNullOrEmpty(reason);
        return new Served(path, string.Empty, HttpStatusCode.OK, reason);
    }

    /// <summary>The reply to a request asked at this entry's path: the status and body of a file, or the failure of
    /// an answer that never comes.</summary>
    internal HttpResponseMessage Reply() =>
        Failure is null ? StubHandler.Answer(Status, Body) : throw new HttpRequestException(Failure);
}
