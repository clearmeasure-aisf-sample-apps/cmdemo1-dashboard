namespace Dashboard.Health;

/// <summary>
/// Loads <c>topology.json</c> from the dashboard's own address (next to <c>index.html</c>), resolved through
/// <see cref="ProbeUrl.ResolveRelative"/>. A refused path is a bad file, not an empty one: the page says why it shows
/// no topology rather than showing none in silence.
/// </summary>
public sealed class TopologyLoader(HttpClient http)
{
    public const string FileName = "topology.json";

    public async Task<TopologyParseResult> LoadAsync(CancellationToken cancellationToken)
    {
        var address = ProbeUrl.ResolveRelative(http.BaseAddress, FileName);
        if (address is null)
        {
            return TopologyParseResult.Failed($"{FileName} is no path of this site.");
        }

        try
        {
            using var request = NodeProber.NewRequest(address);
            using var response = await http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return TopologyParseResult.Failed($"The server answered HTTP {(int)response.StatusCode} for {FileName}.");
            }

            return TopologyParser.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        }
        catch (HttpRequestException exception)
        {
            return TopologyParseResult.Failed($"The file could not be loaded: {exception.Message}");
        }
    }
}
