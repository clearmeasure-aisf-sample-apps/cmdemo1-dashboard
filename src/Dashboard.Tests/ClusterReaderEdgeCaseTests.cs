using System.Net;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ClusterReader" />: the cluster view's reading of
/// its two files, the live status the cluster writes and Azure's facts about the AKS service. <see cref="ClusterMonitorTests" />
/// and <see cref="ClusterMonitorEdgeCaseTests" /> pin the reading the page's rounds make - the file that is not
/// there, the answer the browser cannot read, the no answer at the node's ten seconds, the answer that is not
/// the file - and what they leave open is pinned here: the reader's own timeout, the ten seconds the node's
/// count gives by default and the shorter one the init names, a limit on the answer and no delay of it; the
/// seconds in the no answer's reason, the three the init names and the one and a half that says the two it
/// rounds to; the file that is there, read with its facts and no reason, the status the collector writes
/// and the facts Azure says, and the files that say nothing but an empty array or a stopped power state, read
/// with the rest unsaid; the answer that is not a JSON object and the answer that is only whitespace, malformed
/// with the parser's reason; the address that is not named, which the browser is not asked and is said so;
/// the reader made with no clock, which throws only where it asks the clock; and the check the caller cancels,
/// which is the only throw.
/// <para>
/// Nothing here reaches a network or asks a clock what time it is: every answer is one the test hands back, and
/// every moment is one the test brings.
/// </para>
/// </summary>
public class ClusterReaderEdgeCaseTests
{
    private const string StatusUrl = "https://cluster.example.net/cluster.json";
    private const string ServiceUrl = "https://raw.example.net/org/repo/cluster-status/aks.json";

    private readonly SignallingTimeProvider _time = new();

    private static ClusterReader Reader(StubHandler handler, TimeProvider time, TimeSpan? timeout = null) =>
        timeout is { } limit
            ? new ClusterReader(new HttpClient(handler), time) { Timeout = limit }
            : new ClusterReader(new HttpClient(handler), time);

    private static HttpResponseMessage Answer(HttpRequestMessage request, string? status, string? service) =>
        request.RequestUri!.AbsoluteUri switch
        {
            StatusUrl => status is null ? StubHandler.Answer(HttpStatusCode.NotFound) : StubHandler.Answer(HttpStatusCode.OK, status),
            ServiceUrl => service is null ? StubHandler.Answer(HttpStatusCode.NotFound) : StubHandler.Answer(HttpStatusCode.OK, service),
            _ => StubHandler.Answer(HttpStatusCode.OK),
        };

    // ----- the reader at its making

    /// <summary>The reader's timeout stands at the node's by default: the ten seconds a node may take to answer
    /// - and the init that names a shorter one stands the shorter one, not the node's.</summary>
    [Fact]
    public void TheReaderStandsItsTimeoutAtTheNodesAndTheInitSaysSo()
    {
        var reader = new ClusterReader(new HttpClient(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound))), _time);

        Assert.Equal(NodeProber.DefaultTimeout, reader.Timeout);
        Assert.Equal(TimeSpan.FromSeconds(10), reader.Timeout);

        var quicker = new ClusterReader(new HttpClient(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound))), _time)
        {
            Timeout = TimeSpan.FromSeconds(3),
        };
        Assert.Equal(TimeSpan.FromSeconds(3), quicker.Timeout);
    }

    /// <summary>The reader's default wait, said through the shared wait: a reader made without a named timeout
    /// stands at <see cref="ProbeWait.SharedWait" />, the same value the node prober and the other readers hold,
    /// beside the literal the test above still pins.</summary>
    [Fact]
    public void TheReadersDefaultTimeoutStandsAtTheSharedProbeWait()
    {
        var reader = new ClusterReader(new HttpClient(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound))), _time);

        Assert.Equal(ProbeWait.SharedWait(), reader.Timeout);
    }

    /// <summary>The reader made with no clock throws no error at its making: the error stands where the reader
    /// asks the clock for its timeout, the reading, naming the argument it was made without - and the browser is
    /// never asked, for the timeout is asked before the request is made.</summary>
    [Fact]
    public async Task AReaderMadeWithNoClockThrowsOnlyWhereItAsksTheClock()
    {
        var handler = new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound));
        var reader = new ClusterReader(new HttpClient(handler), null!);

        var error = await Assert.ThrowsAsync<ArgumentNullException>(() => reader.ReadStatusAsync(new Uri(StatusUrl), CancellationToken.None));

        Assert.Equal("timeProvider", error.ParamName);
        Assert.Empty(handler.RequestedUrls);
    }

    // ----- the timeout

    /// <summary>The timeout is a limit on the answer, not a delay of it: the answer that arrives while the
    /// reader waits is read, at the timeout that is one millisecond and not ten seconds, one reader for the
    /// status and the service both.</summary>
    [Fact]
    public async Task AnAnswerThatArrivesInTheTimeIsNotHeldForTheTimeout()
    {
        string status = ClusterFixture.Sample("cluster.json");
        string service = ClusterFixture.Sample("aks.json");
        var reader = Reader(new StubHandler(request => Answer(request, status, service)), _time, TimeSpan.FromMilliseconds(1));

        Assert.Equal(SourceState.Read, (await reader.ReadStatusAsync(new Uri(StatusUrl), CancellationToken.None)).State);
        Assert.Equal(SourceState.Read, (await reader.ReadServiceAsync(new Uri(ServiceUrl), CancellationToken.None)).State);
    }

    /// <summary>The answer that takes longer than the timeout is Unavailable at the seconds the timeout says:
    /// the three the init names says three, and the one and a half says the two it rounds to - the
    /// seconds the timeout stands on, not the moment the answer came.</summary>
    [Fact]
    public async Task AnAnswerThatTakesLongerThanTheTimeoutSaysTheSecondsTheTimeoutSays()
    {
        var never = new StubHandler((request, cancellation) => StubHandler.NeverAsync(cancellation));

        var atThree = Reader(never, _time, TimeSpan.FromSeconds(3));
        var status = atThree.ReadStatusAsync(new Uri(StatusUrl), CancellationToken.None);
        await _time.TimerCreatedAsync();
        _time.Advance(TimeSpan.FromSeconds(3));
        Assert.Equal(new SourceReading<ClusterStatus>(SourceState.Unavailable, Detail: "no answer within 3 s"), await status);

        var atOneAndHalf = Reader(never, _time, TimeSpan.FromSeconds(1.5));
        var service = atOneAndHalf.ReadServiceAsync(new Uri(ServiceUrl), CancellationToken.None);
        await _time.TimerCreatedAsync();
        _time.Advance(TimeSpan.FromSeconds(1.5));

        Assert.Equal(new SourceReading<AksService>(SourceState.Unavailable, Detail: "no answer within 2 s"), await service);
    }

    // ----- the status, read with its facts -----

    /// <summary>The status that is there is Read with its facts and no reason: the sample the collector writes
    /// stands as the reading stands - its moment, its interval, its version, its one node and its six pods -
    /// and the reason stands unsaid for a file that is the file.</summary>
    [Fact]
    public async Task AStatusThatIsThereIsReadWithItsFactsAndNoReason()
    {
        var reader = Reader(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.OK, ClusterFixture.Sample("cluster.json"))), _time);

        var reading = await reader.ReadStatusAsync(new Uri(StatusUrl), CancellationToken.None);

        Assert.Equal(SourceState.Read, reading.State);
        Assert.Null(reading.Detail);
        var status = reading.Value;
        Assert.NotNull(status);
        Assert.Equal(ClusterFixture.Generated, status!.Generated);
        Assert.Equal(15, status.IntervalSeconds);
        Assert.Equal("v1.33.3", status.KubernetesVersion);
        var node = Assert.Single(status.Nodes);
        Assert.Equal("aks-system-12148983-vmss000000", node.Name);
        Assert.Equal(3, status.Namespaces.Count);
        Assert.Equal(6, status.Pods.Count());
    }

    /// <summary>The status that says only an empty array of nodes is read, not malformed: the file that is a
    /// JSON object with its array stands Read, its numbers unsaid and its lists empty - the empty is a reading;
    /// the nothing the parser says is only where the file has no array at all.</summary>
    [Fact]
    public async Task AStatusThatSaysOnlyAnEmptyArrayIsReadWithTheRestUnsaid()
    {
        var reader = Reader(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.OK, """{ "nodes": [] }""")), _time);

        var reading = await reader.ReadStatusAsync(new Uri(StatusUrl), CancellationToken.None);

        Assert.Equal(SourceState.Read, reading.State);
        Assert.Null(reading.Detail);
        var status = reading.Value;
        Assert.NotNull(status);
        Assert.Null(status!.Generated);
        Assert.Null(status.IntervalSeconds);
        Assert.Null(status.KubernetesVersion);
        Assert.Empty(status.Nodes);
        Assert.Empty(status.Namespaces);
    }

    /// <summary>The status that is not a JSON object, and the status that is only whitespace, are Malformed
    /// with the parser's reason: the reason stands in the reading as it stands in the parser, and the value
    /// stands absent for a file that is not the file.</summary>
    [Fact]
    public async Task AStatusThatIsNotAnObjectOrOnlyWhitespaceIsMalformedWithItsReason()
    {
        string body = "[]";
        var reader = Reader(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.OK, body)), _time);

        Assert.Equal(
            new SourceReading<ClusterStatus>(SourceState.Malformed, Detail: "the file does not contain a JSON object"),
            await reader.ReadStatusAsync(new Uri(StatusUrl), CancellationToken.None));

        body = "  \n ";
        Assert.Equal(
            new SourceReading<ClusterStatus>(SourceState.Malformed, Detail: "the file is empty"),
            await reader.ReadStatusAsync(new Uri(StatusUrl), CancellationToken.None));
    }

    // ----- the service, read with its facts -----

    /// <summary>The service that is there is Read with its facts and no reason: the facts Azure says stand as
    /// the reading stands - its name, its group, its region, its availability, its power state, its one pool -
    /// and the reason stands unsaid for a file that is the file.</summary>
    [Fact]
    public async Task AServiceThatIsThereIsReadWithItsFactsAndNoReason()
    {
        var reader = Reader(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.OK, ClusterFixture.Sample("aks.json"))), _time);

        var reading = await reader.ReadServiceAsync(new Uri(ServiceUrl), CancellationToken.None);

        Assert.Equal(SourceState.Read, reading.State);
        Assert.Null(reading.Detail);
        var service = reading.Value;
        Assert.NotNull(service);
        Assert.Equal("aks-cmdemo3", service!.Name);
        Assert.Equal("rg-cmdemo3-cluster", service.ResourceGroup);
        Assert.Equal("southcentralus", service.Location);
        Assert.Equal(AksAvailability.Available, service.Availability!.State);
        Assert.Equal(AksService.Running, service.PowerState);
        Assert.Equal("Succeeded", service.ProvisioningState);
        var pool = Assert.Single(service.Pools);
        Assert.Equal("system", pool.Name);
    }

    /// <summary>The service that says only its power state is read, not malformed: the file that says one fact
    /// stands Read with the fact it says and the rest unsaid - and the stopped it says stands as the stopped
    /// the service knows.</summary>
    [Fact]
    public async Task AServiceThatSaysOneFactIsReadWithTheRestUnsaid()
    {
        var reader = Reader(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.OK, """{ "powerState": "Stopped" }""")), _time);

        var reading = await reader.ReadServiceAsync(new Uri(ServiceUrl), CancellationToken.None);

        Assert.Equal(SourceState.Read, reading.State);
        Assert.Null(reading.Detail);
        var service = reading.Value;
        Assert.NotNull(service);
        Assert.Equal(AksService.Stopped, service!.PowerState);
        Assert.True(service.IsStopped);
        Assert.Null(service.Generated);
        Assert.Null(service.Name);
        Assert.Null(service.Availability);
        Assert.Null(service.ProvisioningState);
        Assert.Empty(service.Pools);
        Assert.Null(service.Metrics);
    }

    /// <summary>The service that is not a JSON object, that says nothing about the service, and that is only
    /// whitespace, are Malformed with the parser's reason: each reason stands in the reading as it stands in
    /// the parser, and the value stands absent for a file that is not the file.</summary>
    [Fact]
    public async Task AServiceThatIsNotAnObjectOrSaysNothingOrOnlyWhitespaceIsMalformedWithItsReason()
    {
        string body = "[]";
        var reader = Reader(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.OK, body)), _time);

        Assert.Equal(
            new SourceReading<AksService>(SourceState.Malformed, Detail: "the file does not contain a JSON object"),
            await reader.ReadServiceAsync(new Uri(ServiceUrl), CancellationToken.None));

        body = """{ "name": "aks-cmdemo3" }""";
        Assert.Equal(
            new SourceReading<AksService>(SourceState.Malformed, Detail: "the file says nothing about the service: no availability, no powerState and no provisioningState"),
            await reader.ReadServiceAsync(new Uri(ServiceUrl), CancellationToken.None));

        body = "  \n ";

        Assert.Equal(
            new SourceReading<AksService>(SourceState.Malformed, Detail: "the file is empty"),
            await reader.ReadServiceAsync(new Uri(ServiceUrl), CancellationToken.None));
    }

    // ----- the address and the cancellation

    /// <summary>The address that is not named is not read: the browser is never asked, and the reading says so
    /// without one - the invalid request URI - for the status and the service alike; the address the browser
    /// is asked is one the caller made, and the reader reads only what is named.</summary>
    [Fact]
    public async Task AnAddressThatIsNotNamedIsNotReadAndSaysSo()
    {
        var handler = new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound));
        var reader = Reader(handler, _time);

        var statusError = await Assert.ThrowsAsync<InvalidOperationException>(() => reader.ReadStatusAsync(null!, CancellationToken.None));
        Assert.Contains("invalid request URI", statusError.Message, StringComparison.OrdinalIgnoreCase);

        await Assert.ThrowsAsync<InvalidOperationException>(() => reader.ReadServiceAsync(null!, CancellationToken.None));

        Assert.Empty(handler.RequestedUrls);
    }

    /// <summary>The check the caller cancels is the only throw of the reader's reading, and the only one: the
    /// caller that cancels in the wait gets the cancellation where the timeout stood waiting, the caller that
    /// cancels before it gets the cancellation at once - and the files themselves never throw, for the status
    /// and the service alike.</summary>
    [Fact]
    public async Task ACheckTheCallerCancelsThrowsTheCancellationAndOnlyTheCancellation()
    {
        var never = new StubHandler((request, cancellation) => StubHandler.NeverAsync(cancellation));
        var reader = Reader(never, _time);

        using var during = new CancellationTokenSource();
        var status = reader.ReadStatusAsync(new Uri(StatusUrl), during.Token);
        await _time.TimerCreatedAsync();
        during.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => status);

        using var before = new CancellationTokenSource();
        before.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => reader.ReadStatusAsync(new Uri(StatusUrl), before.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => reader.ReadServiceAsync(new Uri(ServiceUrl), before.Token));

        Assert.Equal([StatusUrl, StatusUrl, ServiceUrl], never.RequestedUrls);
    }
}
