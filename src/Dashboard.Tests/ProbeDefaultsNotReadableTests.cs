using System.Net;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The clause the readers say when the browser could not read an answer at all - a network failure, or an origin that
/// CORS refuses - named once on <c>ProbeDefaults.NotReadable</c> (#2224): the pinned versions reader and the cluster
/// reader used to type it by hand, one copy per reader, and a rewording of one could leave the other behind. The class
/// that holds it, <c>ProbeDefaults</c>, is internal to an app that declares no <c>InternalsVisibleTo</c>, so the test
/// reaches the const by name, the way <see cref="ProbeDefaultsEdgeCaseTests" /> and <see cref="ProbeWait" /> do.
/// </summary>
public class ProbeDefaultsNotReadableTests
{
    /// <summary>The internal <c>Dashboard.Health.ProbeDefaults.NotReadable</c> const, reached by name.</summary>
    private static string NotReadable()
    {
        var type = typeof(NodeProber).Assembly.GetType("Dashboard.Health.ProbeDefaults", throwOnError: true)!;
        var field = type.GetField("NotReadable", BindingFlags.Public | BindingFlags.Static)!;
        Assert.True(field.IsLiteral, "NotReadable is a const.");
        return (string)field.GetRawConstantValue()!;
    }

    [Fact]
    public void TheNotReadableClauseIsTheWordsTheReadersSay()
    {
        Assert.Equal("the browser could not read an answer (network or CORS)", NotReadable());
    }

    /// <summary>The pinned versions reader whose request throws <c>HttpRequestException</c> answers Unavailable with
    /// the shared clause: the network the browser could not get through is no exception, it is the reading, with the
    /// reason the <c>ProbeDefaults</c> names.</summary>
    [Fact]
    public async Task APinnedVersionsReaderThatCannotReadAnAnswerIsUnavailableWithTheSharedClause()
    {
        var handler = new StubHandler(_ => throw new HttpRequestException("Failed to fetch"));
        var reader = new PinnedVersionsReader(new HttpClient(handler), new SignallingTimeProvider());

        var pinned = await reader.ReadAsync(new Uri("https://raw.example.net/org/repo/environments/tdd/versions.json"), CancellationToken.None);

        Assert.Equal(PinnedVersionsState.Unavailable, pinned.State);
        Assert.Equal(NotReadable(), pinned.Detail);
        Assert.Single(handler.RequestedUrls);
    }

    /// <summary>The same, through the reader's Kustomize reading: the pin of one deployable is read by the same
    /// request, so the browser that cannot read an answer leaves the same Unavailable with the same reason.</summary>
    [Fact]
    public async Task AKustomizationReadingThatCannotReadAnAnswerIsUnavailableWithTheSharedClause()
    {
        var handler = new StubHandler(_ => throw new HttpRequestException("Failed to fetch"));
        var reader = new PinnedVersionsReader(new HttpClient(handler), new SignallingTimeProvider());

        var pinned = await reader.ReadKustomizationAsync(new Uri("https://raw.example.net/org/repo/environments/tdd/ui/kustomization.yaml"), "ui", CancellationToken.None);

        Assert.Equal(PinnedVersionsState.Unavailable, pinned.State);
        Assert.Equal(NotReadable(), pinned.Detail);
    }

    /// <summary>The cluster reader whose request throws <c>HttpRequestException</c> answers Unavailable with the
    /// shared clause as its reason, for the status file the cluster writes.</summary>
    [Fact]
    public async Task AClusterStatusReadingThatCannotReadAnAnswerIsUnavailableWithTheSharedClause()
    {
        var handler = new StubHandler(_ => throw new HttpRequestException("Failed to fetch"));
        var reader = new ClusterReader(new HttpClient(handler), new SignallingTimeProvider());

        var reading = await reader.ReadStatusAsync(new Uri("https://cluster.example.net/cluster.json"), CancellationToken.None);

        Assert.Equal(SourceState.Unavailable, reading.State);
        Assert.Null(reading.Value);
        Assert.Equal(NotReadable(), reading.Detail);
    }

    /// <summary>The same, for Azure's facts about the AKS service: the two files the cluster view reads stand
    /// Unavailable with the one reason, whatever the browser could not read.</summary>
    [Fact]
    public async Task AClusterServiceReadingThatCannotReadAnAnswerIsUnavailableWithTheSharedClause()
    {
        var handler = new StubHandler(_ => throw new HttpRequestException("Failed to fetch"));
        var reader = new ClusterReader(new HttpClient(handler), new SignallingTimeProvider());

        var reading = await reader.ReadServiceAsync(new Uri("https://raw.example.net/org/repo/cluster-status/aks.json"), CancellationToken.None);

        Assert.Equal(SourceState.Unavailable, reading.State);
        Assert.Null(reading.Value);
        Assert.Equal(NotReadable(), reading.Detail);
    }

    /// <summary>The shared clause is spelled once in the app: the <c>ProbeDefaults</c> that names it is the only file
    /// of <c>src/Dashboard</c> that says the words, the way <see cref="TrafficCountProbeClauseTests" /> holds the no
    /// answer clause to its one writer - a reader that types the clause by hand again is a copy the test sees.</summary>
    [Fact]
    public void TheNotReadableClauseIsSpelledOnceInTheApp()
    {
        var clause = NotReadable();
        var src = Path.Combine(RepoRoot(), "src", "Dashboard");
        var writers = Directory.EnumerateFiles(src, "*.cs", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(file => File.ReadLines(file).Any(line => !line.TrimStart().StartsWith("///", StringComparison.Ordinal)
                && line.Contains(clause, StringComparison.Ordinal)))
            .Select(Path.GetFileName)
            .ToList();

        Assert.Equal(["ProbeDefaults.cs"], writers);
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Dashboard.sln")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Dashboard.sln not found above the test output.");
    }
}
