using Dashboard.Work;

namespace Dashboard.Health;

/// <param name="Slug">The system's short name.</param>
/// <param name="Name">The name the header shows.</param>
/// <param name="Repository">The system repository on GitHub, where the deployments pin the versions.</param>
/// <param name="DeliveryUrl">Where the browser reads the system's delivery facts (<c>delivery.json</c>); null without them.</param>
/// <param name="CostUrl">Where the browser reads what the system cost in Azure (<c>cost.json</c>); null without it.</param>
/// <param name="Dashboard">The dashboard itself and where it serves its own build facts; null when the topology does not say.</param>
/// <param name="DeploymentsUrl">
/// Where the browser reads the system's deployments in flight (<c>deployments.json</c>); null without them, and
/// nothing is then marked as being deployed.
/// </param>
public sealed record SystemInfo(
    string Slug,
    string Name,
    Uri? Repository = null,
    Uri? DeliveryUrl = null,
    Uri? CostUrl = null,
    DashboardInfo? Dashboard = null,
    Uri? DeploymentsUrl = null)
{
    /// <summary>The name of the file Resource Health's readings are published in, next to <c>delivery.json</c>.</summary>
    internal const string ContainersFileName = "containers.json";

    /// <summary>
    /// Where the browser reads what Resource Health said of the system's resources: <c>containers.json</c>, which the
    /// same workflow publishes next to the delivery facts; null without them.
    /// </summary>
    public Uri? ContainersUrl => FileBeside(DeliveryUrl, ContainersFileName);

    /// <summary>
    /// Where the browser reads the work items the system's bots are handling: <c>workitems.json</c>, published next
    /// to the delivery facts; null without them. The work items view is empty until that file is there.
    /// </summary>
    public Uri? WorkItemsUrl => FileBeside(DeliveryUrl, WorkItemsReport.FileName);

    /// <summary>The file of the given name beside the delivery facts; null without them.</summary>
    private static Uri? FileBeside(Uri? delivery, string fileName) =>
        delivery is null ? null : new Uri(delivery, fileName);
}
