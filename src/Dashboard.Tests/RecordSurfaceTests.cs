using System.Reflection;
using Xunit.Sdk;

namespace Dashboard.Tests;

/// <summary>
/// The tests of <see cref="RecordSurface" />: that its one walk over a type says, for every record an
/// <c>*EdgeCaseTests</c> file pins, exactly what that file's own private copy of the walk says - so a file may call the
/// helper and keep its expected list as the pin - and that it keeps and drops the members the rules name.
/// </summary>
public class RecordSurfaceTests
{
    [Theory]
    [InlineData(typeof(ApplicationVersionEdgeCaseTests), typeof(ApplicationVersion))]
    [InlineData(typeof(AvailabilityPartEdgeCaseTests), typeof(AvailabilityPart))]
    [InlineData(typeof(BehindFirstEdgeCaseTests), typeof(BehindFirst))]
    [InlineData(typeof(ClusterPodEdgeCaseTests), typeof(ClusterPod))]
    [InlineData(typeof(ColumnWaitEdgeCaseTests), typeof(ColumnWait))]
    [InlineData(typeof(ComplexityFactsSurfaceEdgeCaseTests), typeof(ComplexityFacts))]
    [InlineData(typeof(CostAmountsEdgeCaseTests), typeof(CostAmounts))]
    [InlineData(typeof(CostEntryEdgeCaseTests), typeof(CostEntry))]
    [InlineData(typeof(CostEstimateEdgeCaseTests), typeof(CostEstimate))]
    [InlineData(typeof(CostPartEdgeCaseTests), typeof(CostPart))]
    [InlineData(typeof(CostServiceEdgeCaseTests), typeof(CostService))]
    [InlineData(typeof(CrapFactsSurfaceEdgeCaseTests), typeof(CrapFacts))]
    [InlineData(typeof(DashboardEventEdgeCaseTests), typeof(DashboardEvent))]
    [InlineData(typeof(DeliveryEntryEdgeCaseTests), typeof(DeliveryEntry))]
    [InlineData(typeof(DeployableInfoEdgeCaseTests), typeof(DeployableInfo))]
    [InlineData(typeof(DeploymentEdgeCaseTests), typeof(Deployment))]
    [InlineData(typeof(FailoverTestEdgeCaseTests), typeof(FailoverTest))]
    [InlineData(typeof(HealthCheckEntryEdgeCaseTests), typeof(HealthCheckEntry))]
    [InlineData(typeof(HealthReportsSurfaceEdgeCaseTests), typeof(HealthReports))]
    [InlineData(typeof(HealthWindowEdgeCaseTests), typeof(HealthWindow))]
    [InlineData(typeof(NodeInfoEdgeCaseTests), typeof(NodeInfo))]
    [InlineData(typeof(PersonSignOffEdgeCaseTests), typeof(PersonSignOff))]
    [InlineData(typeof(RecentDeploymentEdgeCaseTests), typeof(RecentDeployment))]
    [InlineData(typeof(ResourceHealthWordsEdgeCaseTests), typeof(ResourceHealthWords))]
    [InlineData(typeof(RuntimeApplicationChangeEdgeCaseTests), typeof(RuntimeApplicationChange))]
    [InlineData(typeof(RuntimeEdgeEdgeCaseTests), typeof(RuntimeEdge))]
    [InlineData(typeof(RuntimeFrameEdgeCaseTests), typeof(RuntimeFrame))]
    [InlineData(typeof(RuntimeIndexEntryEdgeCaseTests), typeof(RuntimeIndexEntry))]
    [InlineData(typeof(RuntimeNameEdgeCaseTests), typeof(RuntimeName))]
    [InlineData(typeof(RuntimeNodeEdgeCaseTests), typeof(RuntimeNode))]
    [InlineData(typeof(RuntimeRegionEdgeCaseTests), typeof(RuntimeRegion))]
    [InlineData(typeof(RuntimeTrendEdgeCaseTests), typeof(RuntimeTrend))]
    [InlineData(typeof(WorkDurationEdgeCaseTests), typeof(WorkDuration))]
    [InlineData(typeof(WorkItemEnvironmentsEdgeCaseTests), typeof(WorkItemEnvironments))]
    [InlineData(typeof(WorkItemsScopeEdgeCaseTests), typeof(WorkItemsScope))]
    [InlineData(typeof(WorkScopeLineEdgeCaseTests), typeof(WorkScopeLine))]
    public void OfSaysWhatEachFilesOwnCopyOfTheWalkSays(Type tests, Type record)
    {
        var copy = tests.GetMethod("PublicSurface", BindingFlags.NonPublic | BindingFlags.Static, Type.EmptyTypes);
        Assert.NotNull(copy);

        var expected = Assert.IsType<List<string>>(copy.Invoke(null, null));

        Assert.Equal(expected, RecordSurface.Of(record));
    }

    [Fact]
    public void OfTIsOfTheType()
    {
        var type = typeof(RuntimeName);

        Assert.Equal(RecordSurface.Of(type), RecordSurface.Of<RuntimeName>());
    }

    [Fact]
    public void OfKeepsPartsMethodsAndTheTwoOperatorsAndWalksPastAccessorsCloneAndPrint()
    {
        Assert.Equal(
            ["Count", "Deconstruct", "Equals", "GetHashCode", "Left", "Right", "ToString", "Twice", "op_Equality", "op_Inequality"],
            RecordSurface.Of<Sample>());
    }

    [Fact]
    public void OfWalksPastWhatTheTypeDoesNotDeclareItselfAndWhatIsNotPublic()
    {
        // The derived record declares its new part and its own operators; the part it hands down to its base it does not.
        Assert.Contains("Extra", RecordSurface.Of<DerivedSample>());
        Assert.Contains("op_Equality", RecordSurface.Of<DerivedSample>());
        Assert.DoesNotContain("Left", RecordSurface.Of<DerivedSample>());
        Assert.DoesNotContain("Hidden", RecordSurface.Of<Sample>());
    }

    [Fact]
    public void OfListsEachNameOnceInOrdinalOrder()
    {
        var surface = RecordSurface.Of<Sample>();

        Assert.Equal(surface.Distinct(StringComparer.Ordinal), surface);
        Assert.Equal(surface.Order(StringComparer.Ordinal), surface);
    }

    [Fact]
    public void AssertPartsAreInitOnlyPassesForThePositionalPartsOfARecord()
    {
        RecordSurface.AssertPartsAreInitOnly(typeof(Sample), "Left", "Right");
        RecordSurface.AssertPartsAreInitOnly(typeof(RuntimeName), "Alias", "NameLink");
    }

    [Fact]
    public void AssertPartsAreInitOnlyFailsForAPartThatCanBeSetAfterTheRecordIsMade()
    {
        Assert.ThrowsAny<XunitException>(() => RecordSurface.AssertPartsAreInitOnly(typeof(Settable), "Value"));
    }

    [Fact]
    public void AssertPartsAreInitOnlyFailsForAPartThatCannotBeWrittenAtAll()
    {
        Assert.ThrowsAny<XunitException>(() => RecordSurface.AssertPartsAreInitOnly(typeof(Sample), "Twice"));
        Assert.ThrowsAny<XunitException>(() => RecordSurface.AssertPartsAreInitOnly(typeof(Sample), "Count"));
    }

    [Fact]
    public void AssertPartsAreInitOnlyFailsForANameTheTypeDoesNotHaveAndForNoNames()
    {
        Assert.ThrowsAny<XunitException>(() => RecordSurface.AssertPartsAreInitOnly(typeof(Sample), "Missing"));
        Assert.ThrowsAny<XunitException>(() => RecordSurface.AssertPartsAreInitOnly(typeof(Sample)));
    }

    private sealed record Sample(string Left, int Right)
    {
        public string Twice() => Left + Left;

        public int Count => Right;

        internal string Hidden() => Left;
    }

    private record BaseSample(string Left);

    private sealed record DerivedSample(string Left, int Extra) : BaseSample(Left);

    private sealed record Settable
    {
        public string Value { get; set; } = "";
    }
}
