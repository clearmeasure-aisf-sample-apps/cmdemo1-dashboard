using System.Globalization;
using Dashboard.Health;
using Entry = Dashboard.Runtime.RuntimePayloadBuilder.Entry;

namespace Dashboard.Runtime;

/// <summary>
/// The marks of the runtime diagram's arrows, one builder per kind of arrow (an origin, the database, a dependency, a
/// public address, a dashboard, and any other), moved out of <see cref="RuntimePayloadBuilder"/> unchanged (#1829, step 2
/// of #534). The numbers are the web apps' own counts of the last minute; a dash where a web app reports none, and on a
/// container app's arrow to the database, whose counts the page does not ask for. The counting sentences of the
/// tooltips have one home here (#1734), and every mark is built with named arguments (#1511).
/// </summary>
internal static class RuntimeEdgeMarks
{
    internal static RuntimeEdgeMark Edge(RuntimeEdge edge, RuntimeDiagramIndex<Entry> index, Dictionary<string, ContainerAppStatus> probed)
    {
        var from = index.Entry(edge.From);
        var to = index.Entry(edge.To);
        return edge.Kind switch
        {
            RuntimeEdgeKind.Origin => OriginEdge(edge, to),
            RuntimeEdgeKind.Sql => SqlEdge(edge, from, index, probed),
            RuntimeEdgeKind.Dependency => DependencyEdge(edge, from, index),
            RuntimeEdgeKind.Public => PublicEdge(edge, to ?? from, index),
            RuntimeEdgeKind.Dashboard when to is not null => DashboardEdge(edge, to),
            _ => UnknownEdge(edge),
        };
    }

    /// <summary>Front Door's arrow to one of its origins: its role in the origin group and the calls it forwarded.</summary>
    private static RuntimeEdgeMark OriginEdge(RuntimeEdge edge, Entry? to)
    {
        // In a rotation the origins of the primary nodes share the traffic: none of them is the first.
        var role = to is { Assessment.IsRotation: true, Target.IsPrimary: true } ? "in the rotation, while healthy"
            : edge.IsFirstOrigin ? "first, while healthy"
            : "when priority 1 is down";
        var state = to is null ? EdgeState.Neutral : Carries(to);
        var telemetry = to?.Target.Telemetry;
        var text = telemetry is { FrontDoorProbes: > 0 } ? string.Create(CultureInfo.InvariantCulture, $"{role}{RuntimePayloadBuilder.PartSeparator}{telemetry.FrontDoorProbes} probes") : role;
        var counted = Counted(CallsWord, telemetry is null
            ? null
            : string.Create(CultureInfo.InvariantCulture, $"{telemetry.FromFrontDoor} requests forwarded by Front Door, {telemetry.FrontDoorProbes} Front Door health probes"));
        return new RuntimeEdgeMark(
            Id: edge.Id,
            State: state.Word(),
            Number: Number(telemetry?.FromFrontDoor),
            Unit: RuntimePayloadBuilder.CallsUnit,
            Text: text,
            Title: $"Front Door to {edge.To}, origin priority {edge.Priority?.ToString(CultureInfo.InvariantCulture) ?? "not known"}: {role}. {state.Sentence()} {counted}",
            Link: to is null || telemetry is null ? null : RuntimeTargetLink.To(to.Target, LinkSet.Performance, to.Target.Name),
            Trend: to is null ? null : RuntimeTrend.Of(Trends.FromFrontDoor(to.Target)));
    }

    /// <summary>A web app's (or a container app's) arrow to the database: the SQL commands the app counted.</summary>
    private static RuntimeEdgeMark SqlEdge(RuntimeEdge edge, Entry? from, RuntimeDiagramIndex<Entry> index, Dictionary<string, ContainerAppStatus> probed)
    {
        if (from is null && index.Node(edge.From) is { Kind: RuntimeNodeKind.ContainerApp } app)
        {
            return ContainerSqlEdge(edge, probed.GetValueOrDefault(app.Alias));
        }

        var state = CallerState(from);
        var telemetry = from?.Target.Telemetry;
        var latency = telemetry?.SqlP95Ms is { } p95 ? string.Create(CultureInfo.InvariantCulture, $", p95 {HistoryText.Milliseconds(p95)}") : string.Empty;
        var counted = Counted(QueriesWord, telemetry switch
        {
            null => null,
            { SplitsSql: true } => string.Create(CultureInfo.InvariantCulture, $"{telemetry.SqlRequests} SQL commands while handling requests (the number shown: what traffic causes) and {telemetry.SqlBackground} {RuntimePayloadBuilder.BackgroundSql}, {telemetry.Sql} in all{latency}; its health checks query the database too"),
            _ => string.Create(CultureInfo.InvariantCulture, $"{telemetry.Sql} SQL commands{latency}; its health checks query the database too"),
        });
        // Short words: they stand under the number, in the narrow column between two frames of the diagram.
        const string Queries = "app queries";
        return new RuntimeEdgeMark(
            Id: edge.Id,
            State: state.Word(),
            Number: Number(telemetry?.SqlOfTraffic),
            Unit: RuntimePayloadBuilder.CallsUnit,
            Text: telemetry is { SplitsSql: true } ? string.Create(CultureInfo.InvariantCulture, $"{Queries}{RuntimePayloadBuilder.PartSeparator}{telemetry.SqlBackground} background") : Queries,
            Title: $"{edge.From} to the database. {state.Sentence()} {counted}",
            Link: from is null || telemetry is null ? null : RuntimeTargetLink.To(from.Target, LinkSet.Dependencies, from.Target.Name),
            Trend: from is null ? null : RuntimeTrend.Of(Trends.Sql(from.Target)));
    }

    /// <summary>A web app's arrow to an outside dependency: its outgoing HTTP calls, when they are this dependency's alone.</summary>
    private static RuntimeEdgeMark DependencyEdge(RuntimeEdge edge, Entry? from, RuntimeDiagramIndex<Entry> index)
    {
        // As to the database: a web app that is down calls nothing, and the dependency is not the reason.
        var state = CallerState(from);
        var telemetry = from?.Target.Telemetry;

        // The app counts its outgoing HTTP calls as one number: it is this dependency's only when the web app
        // has no other one.
        var only = index.DependenciesFrom(edge.From) == 1;
        var counted = only
            ? Counted(CallsWord, telemetry is null
                ? null
                : string.Create(CultureInfo.InvariantCulture, $"{telemetry.Http} outgoing HTTP calls, all of them (it has this one dependency)"))
            : PerMinute(CallsWord, "the web app counts its outgoing HTTP calls as one number, and it has more than one dependency.");
        return new RuntimeEdgeMark(
            Id: edge.Id,
            State: state.Word(),
            Number: only ? Number(telemetry?.Http) : RuntimePayloadBuilder.NoNumber,
            Unit: RuntimePayloadBuilder.CallsUnit,
            Text: only ? "outgoing HTTP calls" : "not counted apart",
            Title: $"{edge.From} to {index.Node(edge.To)?.Name ?? edge.To}. {state.Sentence()} {counted}",
            Link: null,
            Trend: only && from is not null ? RuntimeTrend.Of(Trends.Http(from.Target)) : null);
    }

    /// <summary>The browser's arrow to a public address: whether it answers, and the calls it took.</summary>
    private static RuntimeEdgeMark PublicEdge(RuntimeEdge edge, Entry? target, RuntimeDiagramIndex<Entry> index)
    {
        var state = target is null ? EdgeState.Neutral : AnswerState(target);
        var (number, counted, trend) = PublicCalls(edge, index);
        var requests = number == RuntimePayloadBuilder.NoNumber || target is null
            ? null
            : RuntimeLink.To(target.Deployable.Info.Links?[LinkSet.Logs], LinkText.For(LinkSet.Logs, target.Deployable.Info.Name));
        return new RuntimeEdgeMark(
            Id: edge.Id,
            State: state.Word(),
            Number: number,
            Unit: RuntimePayloadBuilder.CallsUnit,
            Text: null,
            Title: $"The browser to {edge.To}: {state.Sentence()} {counted}",
            Link: requests,
            Trend: RuntimeTrend.Of(trend));
    }

    /// <summary>
    /// The browser's arrow to a dashboard the topology lists as a node (another home of this page): whether it answers.
    /// </summary>
    private static RuntimeEdgeMark DashboardEdge(RuntimeEdge edge, Entry to)
    {
        var state = AnswerState(to);
        var words = state switch
        {
            EdgeState.Active => "It answers.",
            EdgeState.Checking => "Being checked.",
            _ => "Down: it does not answer.",
        };
        return new RuntimeEdgeMark(
            Id: edge.Id,
            State: state.Word(),
            Number: null,
            Unit: null,
            Text: null,
            Title: $"The browser to {to.Target.Name}: {words}");
    }

    /// <summary>An arrow this page has nothing to say about: neutral, named by its two ends.</summary>
    private static RuntimeEdgeMark UnknownEdge(RuntimeEdge edge) =>
        new RuntimeEdgeMark(
            Id: edge.Id,
            State: EdgeState.Neutral.Word(),
            Number: null,
            Unit: null,
            Text: null,
            Title: $"{edge.From} to {edge.To}");

    /// <summary>
    /// The state of a caller's arrow (to the database, to a dependency): a caller that is down sends nothing, and the
    /// callee is not the reason, so the line is idle rather than down.
    /// </summary>
    private static EdgeState CallerState(Entry? from)
    {
        var carries = from is null ? EdgeState.Neutral : Carries(from);
        return carries == EdgeState.Down ? EdgeState.Idle : carries;
    }

    /// <summary>Whether an address the browser calls answers, by its health state.</summary>
    private static EdgeState AnswerState(Entry target) => target.Target.State switch
    {
        HealthState.Healthy => EdgeState.Active,
        HealthState.Pending => EdgeState.Checking,
        _ => EdgeState.Down,
    };

    /// <summary>
    /// The calls to a public address: for a Front Door endpoint, the sum of what its origins counted as forwarded by
    /// Front Door (no caching rule is set, so every call reaches an origin); for a web app's own address, its direct calls.
    /// </summary>
    private static (string Number, string Words, Trend? Trend) PublicCalls(RuntimeEdge edge, RuntimeDiagramIndex<Entry> index)
    {
        var origins = index.OriginsOf(edge.To)
            .Select(other => index.Entry(other.To)?.Target)
            .ToList();
        if (origins.Count > 0)
        {
            var counted = origins.Select(origin => origin?.Telemetry).OfType<TelemetrySnapshot>().ToList();
            return counted.Count == 0
                ? (RuntimePayloadBuilder.NoNumber, PerMinute(CallsWord, "no origin reports them."), null)
                : (Number(counted.Sum(telemetry => telemetry.FromFrontDoor)), PerMinute(CallsWord, "the sum of what its origins counted from Front Door."), Trends.Sum(origins.OfType<TargetStatus>()));
        }

        var direct = index.Entry(edge.To)?.Target;
        return direct?.Telemetry is not { } own
            ? (RuntimePayloadBuilder.NoNumber, ReportsNone(CallsWord), null)
            : (Number(own.Direct), PerMinute(CallsWord, "counted by the web app."), Trends.Direct(direct));
    }

    /// <summary>A count as the number line says it: <see cref="RuntimePayloadBuilder.NoNumber"/> where there is none.</summary>
    private static string Number(int? value) => value?.ToString(CultureInfo.InvariantCulture) ?? RuntimePayloadBuilder.NoNumber;

    // The counting words of an edge's tooltip (#1734): the unit word opens a "per minute" sentence, and a number the
    // web app counted is framed as the last minute's. One home for each, so the edges cannot drift apart.
    private const string CallsWord = "Calls";
    private const string QueriesWord = "Queries";

    /// <summary>A sentence about the unit's rate: the unit word, "per minute", then the words.</summary>
    private static string PerMinute(string unit, string words) => $"{unit} per minute: {words}";

    /// <summary>The unit's sentence where the web app reports no number.</summary>
    private static string ReportsNone(string unit) => PerMinute(unit, "the web app reports none.");

    /// <summary>
    /// The tooltip sentence of a counted edge: the unit's "none" sentence where the web app reports no number, else
    /// the last minute's frame with the counted clause (given without its full stop).
    /// </summary>
    private static string Counted(string unit, string? counted) =>
        counted is null ? ReportsNone(unit) : $"Last minute, counted by the web app: {counted}.";

    /// <summary>
    /// A container app's arrow to the database. The page does not ask the app for its call counts, so the number is a
    /// dash. A healthy health check carries the arrow; an app that is down sends nothing, and the database is not why.
    /// </summary>
    private static RuntimeEdgeMark ContainerSqlEdge(RuntimeEdge edge, ContainerAppStatus? status)
    {
        var state = status is null ? EdgeState.Checking : SqlState(status.State);
        return new RuntimeEdgeMark(
            Id: edge.Id,
            State: state.Word(),
            Number: RuntimePayloadBuilder.NoNumber,
            Unit: RuntimePayloadBuilder.CallsUnit,
            Text: null,
            Title: $"{edge.From} to the database. {state.Sentence()} {PerMinute(QueriesWord, "this page does not count a container app's SQL commands.")}");
    }

    /// <summary>The state of a container app's SQL arrow, one arm per state so a new state fails the build.</summary>
    private static EdgeState SqlState(ContainerAppState state) => state switch
    {
        ContainerAppState.Healthy => EdgeState.Active,
        ContainerAppState.Unhealthy => EdgeState.Idle,
        ContainerAppState.NotResponding => EdgeState.Idle,
        ContainerAppState.Probing => EdgeState.Checking,
        ContainerAppState.Waking => EdgeState.Checking,
        ContainerAppState.Responding => EdgeState.Neutral,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
    };

    /// <summary>Whether the traffic of a node's deployable goes through this node, by the serving decision.</summary>
    private static EdgeState Carries(Entry entry) =>
        entry.Serves ? EdgeState.Active
        : entry.Target.State == HealthState.Healthy ? EdgeState.Idle
        : entry.Target.State == HealthState.Pending ? EdgeState.Checking
        : EdgeState.Down;

    internal static bool SameSite(Uri site, Uri page) =>
        string.Equals(site.Host, page.Host, StringComparison.OrdinalIgnoreCase) && site.Port == page.Port;
}
