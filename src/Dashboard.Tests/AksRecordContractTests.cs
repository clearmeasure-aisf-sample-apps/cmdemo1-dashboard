using Xunit.Sdk;

namespace Dashboard.Tests;

/// <summary>
/// Two rows of constructor arguments for each of the four Aks records, in declared order, that differ in every part.
/// This is the fixed-value ObjectMother <see cref="RecordContract" /> is fed from. A new part of a record is one new
/// value in each row here, not five new lines in each edge-case file.
/// </summary>
internal static class AksRecordMother
{
    private static readonly DateTimeOffset Occurred = new(2026, 10, 6, 19, 42, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset Generated = new(2026, 10, 6, 20, 10, 4, TimeSpan.Zero);

    public static object?[] Availability => [AksAvailability.Degraded, "Slow.", "Upgrading.", Occurred];

    public static object?[] OtherAvailability => [AksAvailability.Available, "No known problems.", null, null];

    public static object?[] Pool => ["pool-alpha", "System", 7, "Standard_D8s_v2", 200, "Running", "Succeeded", "1.31.7"];

    public static object?[] OtherPool => ["pool-beta", null, null, null, null, null, null, null];

    public static object?[] Metrics => [15, 21.4, 63.0, 18.2, 4.6, 31.5];

    public static object?[] OtherMetrics => [null, null, null, null, null, null];

    /// <summary>The pools are a list held by reference: the record compares the list it was handed, not the pools in it,
    /// so both rows keep one list each for as long as the test runs.</summary>
    public static object?[] Service { get; } =
    [
        Generated, "aks-cmdemo3", "rg-cmdemo3", "southcentralus",
        new AksAvailability(AksAvailability.Available, "No known problems.", null, null),
        AksService.Running, AksService.Succeeded, "1.33.3", "Free",
        new List<AksPool> { new("system", "System", 1, "Standard_D4as_v6", 128, "Running", "Succeeded", "1.33.3") },
        new AksMetrics(15, 21.4, 63, 18.2),
    ];

    public static object?[] OtherService { get; } =
        [null, null, null, null, null, null, null, null, null, new List<AksPool>(), null];
}

/// <summary>
/// The record contract of <see cref="RecordContract" />, held for each Aks record: <see cref="AksAvailability" />,
/// <see cref="AksPool" />, <see cref="AksMetrics" /> and <see cref="AksService" />. The hand-written forms of the same
/// sentences in their edge-case files stay where they are. This file says them once, from reflection, so a part added
/// to a record is covered by adding one value to each row of <see cref="AksRecordMother" />. The last two tests hold
/// the helper itself to failing when a record breaks its contract.
/// </summary>
public class AksRecordContractTests
{
    [Fact]
    public void AnAvailabilityKeepsTheRecordContract() =>
        RecordContract.Holds<AksAvailability>(AksRecordMother.Availability, AksRecordMother.OtherAvailability);

    [Fact]
    public void APoolKeepsTheRecordContract() =>
        RecordContract.Holds<AksPool>(AksRecordMother.Pool, AksRecordMother.OtherPool);

    [Fact]
    public void AMetricsBlockKeepsTheRecordContract() =>
        RecordContract.Holds<AksMetrics>(AksRecordMother.Metrics, AksRecordMother.OtherMetrics);

    [Fact]
    public void AServiceKeepsTheRecordContract() =>
        RecordContract.Holds<AksService>(AksRecordMother.Service, AksRecordMother.OtherService);

    /// <summary>The rows cover every part of each record. A part added to the constructor without a value in the
    /// mother fails here by name, before the contract is checked.</summary>
    [Fact]
    public void EachRowOfTheMotherNamesEveryPart()
    {
        Assert.Equal(RecordSurface.ParametersOf<AksAvailability>().Length, AksRecordMother.Availability.Length);
        Assert.Equal(RecordSurface.ParametersOf<AksPool>().Length, AksRecordMother.Pool.Length);
        Assert.Equal(RecordSurface.ParametersOf<AksMetrics>().Length, AksRecordMother.Metrics.Length);
        Assert.Equal(RecordSurface.ParametersOf<AksService>().Length, AksRecordMother.Service.Length);
    }

    /// <summary>A record whose constructor hands each argument to the other part's property breaks the first sentence.
    /// The helper says so instead of passing.</summary>
    [Fact]
    public void TheHelperSeesAConstructorThatSwapsTwoParts() =>
        Assert.ThrowsAny<XunitException>(() => RecordContract.Holds<Swapped>(["left", "right"], ["l", "r"]));

    /// <summary>Two rows that agree on a part cannot show that part landing anywhere, so the helper refuses them.</summary>
    [Fact]
    public void TheHelperRefusesRowsThatAgreeOnAPart() =>
        Assert.ThrowsAny<XunitException>(() => RecordContract.Holds<AksAvailability>(
            AksRecordMother.Availability, [AksAvailability.Available, "Slow.", null, null]));

    private sealed record Swapped(string Left, string Right)
    {
        public string Left { get; init; } = Right;

        public string Right { get; init; } = Left;
    }
}
