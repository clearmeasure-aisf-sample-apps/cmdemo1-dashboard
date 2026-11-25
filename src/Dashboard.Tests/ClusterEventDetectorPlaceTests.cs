namespace Dashboard.Tests;

/// <summary>
/// The kind, the place and the environment every event of <see cref="ClusterEventDetector"/> stands on: an event of
/// the cluster names <see cref="ClusterEventDetector.ClusterPlace"/>, an event of the AKS service names
/// <see cref="ClusterEventDetector.ServicePlace"/> and no environment, and only a pod's change in an environment's
/// namespace names that environment.
/// </summary>
public class ClusterEventDetectorPlaceTests
{
    private static readonly DateTimeOffset Now = ClusterFixture.Generated.AddSeconds(5);

    private static readonly EnvironmentInfo[] Environments = [ClusterFixture.Environment("tdd", "cmdemo3-tdd")];

    private static ClusterStatus With(IReadOnlyList<ClusterNode>? nodes, params ClusterPod[] pods) =>
        ClusterFixture.Status(nodes, ClusterFixture.Space("cmdemo3-tdd", pods));

    private static List<DashboardEvent> EveryClusterEvent()
    {
        var events = new List<DashboardEvent>();
        foreach (var (before, after) in new[]
                 {
                     (ClusterLiveness.Live, ClusterLiveness.Silent),
                     (ClusterLiveness.Live, ClusterLiveness.Unreadable),
                     (ClusterLiveness.Live, ClusterLiveness.Stale),
                     (ClusterLiveness.Stale, ClusterLiveness.Live),
                     (ClusterLiveness.Silent, ClusterLiveness.Live),
                 })
        {
            events.Add(ClusterEventDetector.Liveness(before, after, "timeout", Now)!);
        }

        var names = Enumerable.Range(1, 12).Select(index => $"app{index:00}").ToList();
        var ready = With([ClusterFixture.Node()], [.. names.Select(name => ClusterFixture.Pod(name))]);
        var failing = With([ClusterFixture.Node(ready: false)], [.. names.Select(name => ClusterFixture.Pod(name, ready: false))]);
        events.AddRange(ClusterEventDetector.Status(ready, failing, Environments, Now));
        events.AddRange(ClusterEventDetector.Status(failing, ready, Environments, Now));
        return events;
    }

    [Fact]
    public void EveryEventOfTheClusterIsAClusterEventAtTheClusterPlace()
    {
        var events = EveryClusterEvent();

        Assert.Equal(5 + 12 + 12, events.Count);
        Assert.All(events, e =>
        {
            Assert.Equal(EventKind.Cluster, e.Kind);
            Assert.Equal(ClusterEventDetector.ClusterPlace, e.Node);
            Assert.Equal(Now, e.At);
        });
    }

    [Fact]
    public void EveryEventOfTheServiceIsAClusterEventAtTheServicePlaceOfNoEnvironment()
    {
        var available = ClusterFixture.SampleService;
        var degraded = available with { Availability = new AksAvailability("Degraded", null, null, null), PowerState = "Stopped" };

        var events = ClusterEventDetector.Service(available, degraded, Now).Concat(ClusterEventDetector.Service(degraded, available, Now)).ToList();

        Assert.Equal(4, events.Count);
        Assert.All(events, e =>
        {
            Assert.Equal(EventKind.Cluster, e.Kind);
            Assert.Equal(ClusterEventDetector.ServicePlace, e.Node);
            Assert.Null(e.Environment);
        });
    }

    [Fact]
    public void OnlyAPodChangeInAnEnvironmentsNamespaceNamesThatEnvironment()
    {
        var names = Enumerable.Range(1, 12).Select(index => $"app{index:00}").ToList();
        var ready = With([ClusterFixture.Node()], [.. names.Select(name => ClusterFixture.Pod(name))]);
        var failing = With([ClusterFixture.Node(ready: false)], [.. names.Select(name => ClusterFixture.Pod(name, ready: false))]);

        var events = ClusterEventDetector.Status(ready, failing, Environments, Now);

        Assert.Equal(12, events.Count);
        Assert.Null(events[0].Environment);
        Assert.StartsWith("Node ", events[0].Text, StringComparison.Ordinal);
        Assert.All(events.Skip(1).Take(ClusterEventDetector.NamedPods), e => Assert.Equal("tdd", e.Environment));
        Assert.Null(events[^1].Environment);
        Assert.Equal("2 more pods changed in this check", events[^1].Text);
        Assert.Null(ClusterEventDetector.Liveness(ClusterLiveness.Live, ClusterLiveness.Silent, null, Now)!.Environment);
    }
}
