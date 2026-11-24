namespace Dashboard.Tests;

/// <summary>
/// The house's own enums run through <see cref="EnumVocabulary{T}" />: the fourteen vocabularies whose edge-case
/// files pin the ends of <see cref="int" /> by hand, each with its names, in its declared order, and its unset value.
/// The per-enum files keep their own prose and their own claims; this file is the one place the BCL half of the claim
/// is made for all of them at once, so a new word slipped into one of them, a <see cref="FlagsAttribute" /> put on
/// one, or a number written beside one shows up here as well as in the file that reads it.
/// </summary>
public class EnumVocabularyHouseEnumsTests
{

    /// <summary>The 2 words of <see cref="TargetKind" />, and nothing outside them.</summary>
    [Fact]
    public void TargetKindIsAClosedIntVocabulary()
    {
        EnumVocabulary<TargetKind>.AssertWholeVocabulary([TargetKind.FrontDoor, TargetKind.Node], ["FrontDoor", "Node"], TargetKind.FrontDoor);
        EnumVocabulary<TargetKind>.AssertUnderlyingTypeAndNotFlags();
        EnumVocabulary<TargetKind>.AssertNothingOutsideTheVocabularyIsDefined(TargetKind.FrontDoor, TargetKind.Node);
        EnumVocabulary<TargetKind>.AssertSaysItsOwnNumberOutsideTheVocabulary();
    }

    /// <summary>The 4 words of <see cref="SummaryLevel" />, and nothing outside them.</summary>
    [Fact]
    public void SummaryLevelIsAClosedIntVocabulary()
    {
        EnumVocabulary<SummaryLevel>.AssertWholeVocabulary([SummaryLevel.Empty, SummaryLevel.Pending, SummaryLevel.AllHealthy, SummaryLevel.Problems], ["Empty", "Pending", "AllHealthy", "Problems"], SummaryLevel.Empty);
        EnumVocabulary<SummaryLevel>.AssertUnderlyingTypeAndNotFlags();
        EnumVocabulary<SummaryLevel>.AssertNothingOutsideTheVocabularyIsDefined(SummaryLevel.Empty, SummaryLevel.Problems);
        EnumVocabulary<SummaryLevel>.AssertSaysItsOwnNumberOutsideTheVocabulary();
    }

    /// <summary>The 4 words of <see cref="CheckState" />, and nothing outside them.</summary>
    [Fact]
    public void CheckStateIsAClosedIntVocabulary()
    {
        EnumVocabulary<CheckState>.AssertWholeVocabulary([CheckState.Healthy, CheckState.Degraded, CheckState.Unhealthy, CheckState.Unknown], ["Healthy", "Degraded", "Unhealthy", "Unknown"], CheckState.Healthy);
        EnumVocabulary<CheckState>.AssertUnderlyingTypeAndNotFlags();
        EnumVocabulary<CheckState>.AssertNothingOutsideTheVocabularyIsDefined(CheckState.Healthy, CheckState.Unknown);
        EnumVocabulary<CheckState>.AssertSaysItsOwnNumberOutsideTheVocabulary();
    }

    /// <summary>The 5 words of <see cref="ServingState" />, and nothing outside them.</summary>
    [Fact]
    public void ServingStateIsAClosedIntVocabulary()
    {
        EnumVocabulary<ServingState>.AssertWholeVocabulary([ServingState.NoNodes, ServingState.Pending, ServingState.Primary, ServingState.FailedOver, ServingState.Down], ["NoNodes", "Pending", "Primary", "FailedOver", "Down"], ServingState.NoNodes);
        EnumVocabulary<ServingState>.AssertUnderlyingTypeAndNotFlags();
        EnumVocabulary<ServingState>.AssertNothingOutsideTheVocabularyIsDefined(ServingState.NoNodes, ServingState.Down);
        EnumVocabulary<ServingState>.AssertSaysItsOwnNumberOutsideTheVocabulary();
    }

    /// <summary>The 6 words of <see cref="ContainerAppState" />, and nothing outside them.</summary>
    [Fact]
    public void ContainerAppStateIsAClosedIntVocabulary()
    {
        EnumVocabulary<ContainerAppState>.AssertWholeVocabulary([ContainerAppState.Probing, ContainerAppState.Waking, ContainerAppState.Responding, ContainerAppState.Healthy, ContainerAppState.Unhealthy, ContainerAppState.NotResponding], ["Probing", "Waking", "Responding", "Healthy", "Unhealthy", "NotResponding"], ContainerAppState.Probing);
        EnumVocabulary<ContainerAppState>.AssertUnderlyingTypeAndNotFlags();
        EnumVocabulary<ContainerAppState>.AssertNothingOutsideTheVocabularyIsDefined(ContainerAppState.Probing, ContainerAppState.NotResponding);
        EnumVocabulary<ContainerAppState>.AssertSaysItsOwnNumberOutsideTheVocabulary();
    }

    /// <summary>The 4 words of <see cref="DashboardView" />, and nothing outside them.</summary>
    [Fact]
    public void DashboardViewIsAClosedIntVocabulary()
    {
        EnumVocabulary<DashboardView>.AssertWholeVocabulary([DashboardView.Health, DashboardView.Runtime, DashboardView.Cluster, DashboardView.WorkItems], ["Health", "Runtime", "Cluster", "WorkItems"], DashboardView.Health);
        EnumVocabulary<DashboardView>.AssertUnderlyingTypeAndNotFlags();
        EnumVocabulary<DashboardView>.AssertNothingOutsideTheVocabularyIsDefined(DashboardView.Health, DashboardView.WorkItems);
        EnumVocabulary<DashboardView>.AssertSaysItsOwnNumberOutsideTheVocabulary();
    }

    /// <summary>The 7 words of <see cref="DeploymentState" />, and nothing outside them.</summary>
    [Fact]
    public void DeploymentStateIsAClosedIntVocabulary()
    {
        EnumVocabulary<DeploymentState>.AssertWholeVocabulary([DeploymentState.Unknown, DeploymentState.Queued, DeploymentState.Executing, DeploymentState.Waiting, DeploymentState.Succeeded, DeploymentState.Failed, DeploymentState.Canceled], ["Unknown", "Queued", "Executing", "Waiting", "Succeeded", "Failed", "Canceled"], DeploymentState.Unknown);
        EnumVocabulary<DeploymentState>.AssertUnderlyingTypeAndNotFlags();
        EnumVocabulary<DeploymentState>.AssertNothingOutsideTheVocabularyIsDefined(DeploymentState.Unknown, DeploymentState.Canceled);
        EnumVocabulary<DeploymentState>.AssertSaysItsOwnNumberOutsideTheVocabulary();
    }

    /// <summary>The 8 words of <see cref="EventKind" />, and nothing outside them.</summary>
    [Fact]
    public void EventKindIsAClosedIntVocabulary()
    {
        EnumVocabulary<EventKind>.AssertWholeVocabulary([EventKind.Health, EventKind.HealthCheck, EventKind.Restart, EventKind.Version, EventKind.Serving, EventKind.Pinned, EventKind.Traffic, EventKind.Cluster], ["Health", "HealthCheck", "Restart", "Version", "Serving", "Pinned", "Traffic", "Cluster"], EventKind.Health);
        EnumVocabulary<EventKind>.AssertUnderlyingTypeAndNotFlags();
        EnumVocabulary<EventKind>.AssertNothingOutsideTheVocabularyIsDefined(EventKind.Health, EventKind.Cluster);
        EnumVocabulary<EventKind>.AssertSaysItsOwnNumberOutsideTheVocabulary();
    }

    /// <summary>The 4 words of <see cref="EventLevel" />, and nothing outside them.</summary>
    [Fact]
    public void EventLevelIsAClosedIntVocabulary()
    {
        EnumVocabulary<EventLevel>.AssertWholeVocabulary([EventLevel.Info, EventLevel.Good, EventLevel.Warning, EventLevel.Problem], ["Info", "Good", "Warning", "Problem"], EventLevel.Info);
        EnumVocabulary<EventLevel>.AssertUnderlyingTypeAndNotFlags();
        EnumVocabulary<EventLevel>.AssertNothingOutsideTheVocabularyIsDefined(EventLevel.Info, EventLevel.Problem);
        EnumVocabulary<EventLevel>.AssertSaysItsOwnNumberOutsideTheVocabulary();
    }

    /// <summary>The 4 words of <see cref="FrontDoorAgreement" />, and nothing outside them.</summary>
    [Fact]
    public void FrontDoorAgreementIsAClosedIntVocabulary()
    {
        EnumVocabulary<FrontDoorAgreement>.AssertWholeVocabulary([FrontDoorAgreement.NotPresent, FrontDoorAgreement.Pending, FrontDoorAgreement.Agrees, FrontDoorAgreement.Disagrees], ["NotPresent", "Pending", "Agrees", "Disagrees"], FrontDoorAgreement.NotPresent);
        EnumVocabulary<FrontDoorAgreement>.AssertUnderlyingTypeAndNotFlags();
        EnumVocabulary<FrontDoorAgreement>.AssertNothingOutsideTheVocabularyIsDefined(FrontDoorAgreement.NotPresent, FrontDoorAgreement.Disagrees);
        EnumVocabulary<FrontDoorAgreement>.AssertSaysItsOwnNumberOutsideTheVocabulary();
    }

    /// <summary>The 4 words of <see cref="PodState" />, and nothing outside them.</summary>
    [Fact]
    public void PodStateIsAClosedIntVocabulary()
    {
        EnumVocabulary<PodState>.AssertWholeVocabulary([PodState.Ready, PodState.Starting, PodState.Unhealthy, PodState.Finished], ["Ready", "Starting", "Unhealthy", "Finished"], PodState.Ready);
        EnumVocabulary<PodState>.AssertUnderlyingTypeAndNotFlags();
        EnumVocabulary<PodState>.AssertNothingOutsideTheVocabularyIsDefined(PodState.Ready, PodState.Finished);
        EnumVocabulary<PodState>.AssertSaysItsOwnNumberOutsideTheVocabulary();
    }

    /// <summary>The 3 words of <see cref="RuntimeAvailability" />, and nothing outside them.</summary>
    [Fact]
    public void RuntimeAvailabilityIsAClosedIntVocabulary()
    {
        EnumVocabulary<RuntimeAvailability>.AssertWholeVocabulary([RuntimeAvailability.Available, RuntimeAvailability.NotDeployed, RuntimeAvailability.Failed], ["Available", "NotDeployed", "Failed"], RuntimeAvailability.Available);
        EnumVocabulary<RuntimeAvailability>.AssertUnderlyingTypeAndNotFlags();
        EnumVocabulary<RuntimeAvailability>.AssertNothingOutsideTheVocabularyIsDefined(RuntimeAvailability.Available, RuntimeAvailability.Failed);
        EnumVocabulary<RuntimeAvailability>.AssertSaysItsOwnNumberOutsideTheVocabulary();
    }

    /// <summary>The 5 words of <see cref="SourceState" />, and nothing outside them.</summary>
    [Fact]
    public void SourceStateIsAClosedIntVocabulary()
    {
        EnumVocabulary<SourceState>.AssertWholeVocabulary([SourceState.Pending, SourceState.Read, SourceState.Missing, SourceState.Unavailable, SourceState.Malformed], ["Pending", "Read", "Missing", "Unavailable", "Malformed"], SourceState.Pending);
        EnumVocabulary<SourceState>.AssertUnderlyingTypeAndNotFlags();
        EnumVocabulary<SourceState>.AssertNothingOutsideTheVocabularyIsDefined(SourceState.Pending, SourceState.Malformed);
        EnumVocabulary<SourceState>.AssertSaysItsOwnNumberOutsideTheVocabulary();
    }

    /// <summary>The 3 words of <see cref="WorkAudience" />, and nothing outside them.</summary>
    [Fact]
    public void WorkAudienceIsAClosedIntVocabulary()
    {
        EnumVocabulary<WorkAudience>.AssertWholeVocabulary([WorkAudience.All, WorkAudience.Bot, WorkAudience.Team], ["All", "Bot", "Team"], WorkAudience.All);
        EnumVocabulary<WorkAudience>.AssertUnderlyingTypeAndNotFlags();
        EnumVocabulary<WorkAudience>.AssertNothingOutsideTheVocabularyIsDefined(WorkAudience.All, WorkAudience.Team);
        EnumVocabulary<WorkAudience>.AssertSaysItsOwnNumberOutsideTheVocabulary();
    }
}
