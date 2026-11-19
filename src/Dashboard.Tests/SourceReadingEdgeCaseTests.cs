using System.Net;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <c>ClusterReader.cs</c>: the five <see cref="SourceState" />s,
/// the <see cref="SourceReading{T}" /> record itself - what a reading keeps, what it says about itself, how two of
/// them compare - and the members of <see cref="ClusterReader" /> that produce it: the timeout as it is and as it
/// is set, and the answers that are not the file. <see cref="ClusterMonitorTests" /> pins the paths the page takes
/// round after round - the file read, the 404 of the status, the network failure, the answer that never comes
/// within the ten seconds the node has, and the 503 and the non-JSON of the service - and <see cref="ClusterParserTests" />
/// pins the two parses themselves; what they leave open is pinned here: the states and their order, a reading with
/// only a state, the empty detail and the empty value that stand as they stand, the record that says itself in the
/// order it is declared, the equality and the copy, the timeout the reader has before it is set and the one it
/// takes, the success that ends at no content and the answer that starts after it, and the answers that are empty,
/// that are not objects and that name neither node nor namespace, for the status as well as for the service.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every answer is one the stub gives, and the
/// one moment that moves is advanced by the test.
/// </para>
/// </summary>
public class SourceReadingEdgeCaseTests
{
    private const string StatusUrl = "https://cluster.example.net/cluster.json";
    private const string ServiceUrl = "https://raw.example.net/org/repo/cluster-status/aks.json";
    private static readonly Uri StatusAddress = new(StatusUrl);
    private static readonly Uri ServiceAddress = new(ServiceUrl);

    private readonly SignallingTimeProvider _time = new();

    // ----- The states of a reading -----

    /// <summary>The five states a reading stands in, in the order the enum declares them: the page compares the
    /// readings against these values, so their names and their order are the contract, and no sixth state stands
    /// behind them.</summary>
    [Fact]
    public void TheFiveStatesOfASourceReadingAreTheFiveInTheOrderTheyAreDeclared()
    {
        Assert.Equal(0, (int)SourceState.Pending);
        Assert.Equal(1, (int)SourceState.Read);
        Assert.Equal(2, (int)SourceState.Missing);
        Assert.Equal(3, (int)SourceState.Unavailable);
        Assert.Equal(4, (int)SourceState.Malformed);

        Assert.Equal(["Pending", "Read", "Missing", "Unavailable", "Malformed"], Enum.GetNames<SourceState>());
        Assert.Equal(5, Enum.GetValues<SourceState>().Length);
    }

    // ----- The reading itself -----

    /// <summary>A reading named with its state alone: the value the reading was not read with stands null, and so
    /// does the reason that would stand where the value is.</summary>
    [Fact]
    public void AReadingWithOnlyAStateHasNoValueAndNoDetail()
    {
        var reading = new SourceReading<ClusterStatus>(SourceState.Pending);

        Assert.Equal(SourceState.Pending, reading.State);
        Assert.Null(reading.Value);
        Assert.Null(reading.Detail);

        var missing = new SourceReading<AksService>(SourceState.Missing, Detail: "the address answered HTTP 404");

        Assert.Null(missing.Value);
        Assert.Equal("the address answered HTTP 404", missing.Detail);
    }

    /// <summary>The reading keeps its three parts as it was given them, each of them the value it stood on: the
    /// read one, the one whose detail is the empty string - kept, not read as absent - and the one whose value is
    /// the empty string - kept, not read as absent.</summary>
    [Fact]
    public void TheReadingKeepsItsPartsAsGiven()
    {
        var status = ClusterFixture.SampleStatus;
        var read = new SourceReading<ClusterStatus>(SourceState.Read, status);

        Assert.Equal(SourceState.Read, read.State);
        Assert.Equal(status, read.Value);
        Assert.Null(read.Detail);

        Assert.Equal(string.Empty, new SourceReading<ClusterStatus>(SourceState.Malformed, Detail: string.Empty).Detail);
        Assert.Null(new SourceReading<ClusterStatus>(SourceState.Unavailable, Detail: null).Detail);
        Assert.Equal(string.Empty, new SourceReading<string>(SourceState.Read, string.Empty).Value);
    }

    /// <summary>Two readings are one while all three parts are - the equality of the operators and of
    /// <c>Equals</c>, and the hash code that follows them - and a difference in any one part, the detail made
    /// absent included, is another one; the reading of the other file is another one while every part is the same
    /// name.</summary>
    [Fact]
    public void TwoReadingsAreOneWhenAllThreePartsAre()
    {
        var one = new SourceReading<ClusterStatus>(SourceState.Missing, Detail: "the address answered HTTP 404");
        var two = new SourceReading<ClusterStatus>(SourceState.Missing, Detail: "the address answered HTTP 404");

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.True(one.Equals((object)two));
        Assert.True(two.Equals((object)one));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.False(one != two);

        Assert.NotEqual(one, one with { State = SourceState.Unavailable });
        Assert.NotEqual(one, one with { Detail = "the address answered HTTP 503" });
        Assert.NotEqual(one, one with { Detail = null });
        Assert.NotEqual(one, one with { Value = ClusterFixture.SampleStatus });
        Assert.NotEqual(ClusterFixture.Read(ClusterFixture.SampleStatus), ClusterFixture.Read(ClusterFixture.Status()));
        Assert.False(one.Equals((object)new SourceReading<AksService>(SourceState.Missing, Detail: "the address answered HTTP 404")));
    }

    /// <summary><c>with</c> names the parts it changes and leaves the rest - a copy is equal and not the same -
    /// and the part the copy does not name stands as it stood.</summary>
    [Fact]
    public void WithChangesOnlyThePartsItNamesAndTheCopyIsEqualAndNotTheSame()
    {
        var missing = new SourceReading<ClusterStatus>(SourceState.Missing, Detail: "the address answered HTTP 404");
        var copy = missing with { };

        Assert.Equal(missing, copy);
        Assert.NotSame(missing, copy);
        Assert.Null(copy.Value);

        var changed = missing with { State = SourceState.Unavailable, Detail = "no answer within 10 s" };

        Assert.Equal(new SourceReading<ClusterStatus>(SourceState.Unavailable, Detail: "no answer within 10 s"), changed);
    }

    /// <summary>The record says its parts in the order they are declared, the name of each with the value it
    /// stands on: the part the reading does not have says its name and nothing else, and the reading of the type
    /// of one file says its name with the one the type carries.</summary>
    [Fact]
    public void TheReadingSaysItsPartsInTheOrderTheyAreDeclared()
    {
        Assert.Equal(
            "SourceReading { State = Read, Value = , Detail =  }",
            new SourceReading<ClusterStatus>(SourceState.Read).ToString());
        Assert.Equal(
            "SourceReading { State = Missing, Value = , Detail = the address answered HTTP 404 }",
            new SourceReading<ClusterStatus>(SourceState.Missing, Detail: "the address answered HTTP 404").ToString());
    }

    /// <summary>The reading deconstructs to the parts it was built with, in the order they are declared: the read
    /// one, the value and the reason with it, and the one whose parts are not there, the nothing they stand as.</summary>
    [Fact]
    public void TheReadingDeconstructsToItsParts()
    {
        var status = ClusterFixture.SampleStatus;
        var (state, value, detail) = ClusterFixture.Read(status);

        Assert.Equal(SourceState.Read, state);
        Assert.Equal(status, value);
        Assert.Null(detail);

        var (missingState, missingValue, missingDetail) = new SourceReading<AksService>(SourceState.Missing, Detail: "the address answered HTTP 404");

        Assert.Equal(SourceState.Missing, missingState);
        Assert.Null(missingValue);
        Assert.Equal("the address answered HTTP 404", missingDetail);
    }

    // ----- The reader's timeout -----

    /// <summary>The timeout the reader has before it is set: the node's, to the number - and the timeout it takes
    /// when the caller says what it is, as long as it says.</summary>
    [Fact]
    public void TheTimeoutIsTheNodesDefaultUntilItIsSet()
    {
        var http = new HttpClient(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.OK)));

        Assert.Equal(TimeSpan.FromSeconds(10), new ClusterReader(http, _time).Timeout);
        Assert.Equal(NodeProber.DefaultTimeout, new ClusterReader(http, _time).Timeout);
        Assert.Equal(TimeSpan.FromSeconds(2), new ClusterReader(http, _time) { Timeout = TimeSpan.FromSeconds(2) }.Timeout);
    }

    /// <summary>An answer that is still coming when the timeout the caller set is up is the reading Unavailable,
    /// with the words of the timeout it stood on: the ten seconds the page knows are pinned by
    /// <see cref="ClusterMonitorTests" />, and here the two seconds the caller gave it.</summary>
    [Fact]
    public async Task AnAnswerThatIsStillComingWhenTheSetTimeoutIsUpIsUnavailableWithinIt()
    {
        var reader = new ClusterReader(new HttpClient(new StubHandler((_, cancellationToken) => StubHandler.NeverAsync(cancellationToken))), _time)
        { Timeout = TimeSpan.FromSeconds(2) };

        var read = reader.ReadStatusAsync(StatusAddress, CancellationToken.None);
        await _time.TimerCreatedAsync();
        _time.Advance(TimeSpan.FromSeconds(2));

        Assert.Equal(new SourceReading<ClusterStatus>(SourceState.Unavailable, Detail: "no answer within 2 s"), await read);
    }

    /// <summary>The one throw the reader makes: the caller's cancellation, not a reading. The answer that never
    /// comes while the caller cancels is not the Unavailable with a reason, it is the cancellation itself.</summary>
    [Fact]
    public async Task TheCallersCancellationIsNotAResultEither()
    {
        var reader = new ClusterReader(new HttpClient(new StubHandler((_, cancellationToken) => StubHandler.NeverAsync(cancellationToken))), _time);
        using var cancellation = new CancellationTokenSource();

        var read = reader.ReadStatusAsync(StatusAddress, cancellation.Token);
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => read);
    }

    // ----- What the reader reads -----

    /// <summary>A file that is the file is the reading Read with its value and no reason: the whole reading, not
    /// only its state - for the status as well as for the service, the value the page holds is the file as it
    /// stands.</summary>
    [Fact]
    public async Task AFileThatIsTheFileIsReadWithItsValueAndNoDetail()
    {
        var handler = new StubHandler(request => request.RequestUri!.AbsoluteUri switch
        {
            StatusUrl => StubHandler.Answer(HttpStatusCode.OK, ClusterFixture.Sample("cluster.json")),
            ServiceUrl => StubHandler.Answer(HttpStatusCode.OK, ClusterFixture.Sample("aks.json")),
            _ => StubHandler.Answer(HttpStatusCode.NotFound),
        });
        var reader = new ClusterReader(new HttpClient(handler), _time);

        var status = await reader.ReadStatusAsync(StatusAddress, CancellationToken.None);

        Assert.Equal(SourceState.Read, status.State);
        Assert.Null(status.Detail);
        Assert.Equal(ClusterFixture.Generated, status.Value!.Generated);
        Assert.Equal(15, status.Value.IntervalSeconds);
        Assert.Equal("v1.33.3", status.Value.KubernetesVersion);
        Assert.Single(status.Value.Nodes);
        Assert.Equal(["argocd", "cmdemo3-prod", "cmdemo3-tdd"], status.Value.Namespaces.Select(space => space.Name));
        Assert.Equal(6, status.Value.Pods.Count());

        var service = await reader.ReadServiceAsync(ServiceAddress, CancellationToken.None);

        Assert.Equal(SourceState.Read, service.State);
        Assert.Null(service.Detail);
        Assert.Equal("aks-cmdemo3", service.Value!.Name);
        Assert.Equal("Available", service.Value.Availability!.State);
        Assert.Equal("Running", service.Value.PowerState);
    }

    /// <summary>
    /// The answer that is not the file is the reading Malformed with the word for why, for the status: the one
    /// that ends at no content - the success that says nothing - the empty one, the one that is only whitespace,
    /// the one that is not an object, and the object that names neither node nor namespace.
    /// </summary>
    [Theory]
    [InlineData(HttpStatusCode.NoContent, "", "the file is empty")]
    [InlineData(HttpStatusCode.OK, "", "the file is empty")]
    [InlineData(HttpStatusCode.OK, "   ", "the file is empty")]
    [InlineData(HttpStatusCode.OK, "[]", "the file does not contain a JSON object")]
    [InlineData(HttpStatusCode.OK, "{}", "the file has neither nodes nor namespaces")]
    public async Task AStatusAnswerThatIsNotTheFileIsMalformedWithTheWordForWhy(HttpStatusCode status, string body, string problem)
    {
        var reader = new ClusterReader(new HttpClient(new StubHandler(_ => StubHandler.Answer(status, body))), _time);

        Assert.Equal(new SourceReading<ClusterStatus>(SourceState.Malformed, Detail: problem), await reader.ReadStatusAsync(StatusAddress, CancellationToken.None));
    }

    /// <summary>The answer that is not the file is the reading Malformed with the word for why, for the service:
    /// the empty one, the one that is not an object, and the object that says nothing about the service - no
    /// availability, no power state, no provisioning state.</summary>
    [Theory]
    [InlineData("", "the file is empty")]
    [InlineData("[]", "the file does not contain a JSON object")]
    [InlineData("{}", "the file says nothing about the service: no availability, no powerState and no provisioningState")]
    public async Task AServiceAnswerThatIsNotAzuresFactsIsMalformedWithTheWordForWhy(string body, string problem)
    {
        var reader = new ClusterReader(new HttpClient(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.OK, body))), _time);

        Assert.Equal(new SourceReading<AksService>(SourceState.Malformed, Detail: problem), await reader.ReadServiceAsync(ServiceAddress, CancellationToken.None));
    }

    /// <summary>
    /// The end of the success and the start of the rest: a redirect is not the file, whatever it would lead to -
    /// the reader follows none of them, and the reading is Unavailable with the number the answer stood on.
    /// </summary>
    [Fact]
    public async Task ARedirectIsNotTheFileAndIsUnavailableWithItsNumber()
    {
        var reader = new ClusterReader(new HttpClient(new StubHandler(_ => StubHandler.Answer((HttpStatusCode)300))), _time);

        Assert.Equal(new SourceReading<ClusterStatus>(SourceState.Unavailable, Detail: "the address answered HTTP 300"), await reader.ReadStatusAsync(StatusAddress, CancellationToken.None));
    }

    /// <summary>The 404 of the service is the reading Missing with the word for it, the whole reading: the state
    /// is pinned by <see cref="ClusterMonitorTests" />, and here the reason that stands in the reading with it.</summary>
    [Fact]
    public async Task AnAddressThatAnswersNotFoundIsMissingForTheServiceWithTheWordForIt()
    {
        var reader = new ClusterReader(new HttpClient(new StubHandler(_ => StubHandler.Answer(HttpStatusCode.NotFound))), _time);

        Assert.Equal(new SourceReading<AksService>(SourceState.Missing, Detail: "the address answered HTTP 404"), await reader.ReadServiceAsync(ServiceAddress, CancellationToken.None));
    }
}
