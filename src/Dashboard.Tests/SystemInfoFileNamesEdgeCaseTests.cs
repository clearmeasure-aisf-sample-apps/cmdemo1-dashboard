using System.Reflection;
using Dashboard.Work;

namespace Dashboard.Tests;

/// <summary>
/// The names of the two files <see cref="SystemInfo" /> derives beside the delivery facts: <c>workitems.json</c> is
/// the very name <see cref="WorkItemsReport.FileName" /> spells, and <c>containers.json</c> is the name of an internal
/// constant of the record (internal, so the record's public surface stays free of fields). The app declares no
/// <c>InternalsVisibleTo</c>, so the constant and the one derivation are found by reflection.
/// </summary>
public class SystemInfoFileNamesEdgeCaseTests
{
    private static readonly Uri Delivery = new("https://raw.example.net/org/status/delivery.json");

    [Fact]
    public void TheWorkItemsFileIsTheNameTheWorkItemsReportSpells()
    {
        var system = new SystemInfo("demo", "Demo", DeliveryUrl: Delivery);

        Assert.Equal(new Uri(Delivery, WorkItemsReport.FileName), system.WorkItemsUrl);
        Assert.EndsWith("/" + WorkItemsReport.FileName, system.WorkItemsUrl!.AbsoluteUri, StringComparison.Ordinal);
    }

    [Fact]
    public void TheContainersFileIsNamedByAnInternalConstantOfTheRecord()
    {
        var field = typeof(SystemInfo).GetField("ContainersFileName", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(field);
        Assert.True(field!.IsLiteral);
        Assert.True(field.IsAssembly);
        Assert.Equal("containers.json", field.GetRawConstantValue());

        var system = new SystemInfo("demo", "Demo", DeliveryUrl: Delivery);
        Assert.Equal("https://raw.example.net/org/status/containers.json", system.ContainersUrl!.AbsoluteUri);
    }

    [Fact]
    public void BothFilesAskOnePrivateDerivation()
    {
        var method = typeof(SystemInfo).GetMethod("FileBeside", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        Assert.True(method!.IsPrivate);
        Assert.Equal([typeof(Uri), typeof(string)], method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Null(method.Invoke(null, [null, "containers.json"]));
        Assert.Equal(new Uri(Delivery, "any.json"), method.Invoke(null, [Delivery, "any.json"]));
    }

    [Fact]
    public void WithoutDeliveryFactsNeitherFileIsNamed()
    {
        var system = new SystemInfo("demo", "Demo");

        Assert.Null(system.ContainersUrl);
        Assert.Null(system.WorkItemsUrl);
    }
}
