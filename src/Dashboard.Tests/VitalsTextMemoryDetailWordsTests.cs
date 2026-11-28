namespace Dashboard.Tests;

/// <summary>
/// The words of the memory tooltip (<see cref="VitalsText.MemoryDetail" />): each clause said alone and together,
/// the switch to gigabytes in both memory clauses, and a thread count said with invariant digits for a reader whose
/// own culture groups thousands.
/// </summary>
public class VitalsTextMemoryDetailWordsTests
{
    [Fact]
    public void TheWorkingSetAloneIsSaidWithItsLead() =>
        Assert.Equal("Working set 412 MB", VitalsText.MemoryDetail(new ProcessVitals(null, 412, null, null, null, null, null)));

    [Fact]
    public void TheHeapAloneIsSaidWithTheCapitalizedLead() =>
        Assert.Equal("Managed heap 96 MB", VitalsText.MemoryDetail(new ProcessVitals(null, null, 96, null, null, null, null)));

    [Fact]
    public void TheWorkingSetAndTheHeapAreSaidInOneClause() =>
        Assert.Equal(
            "Working set 412 MB, of which managed heap 96 MB",
            VitalsText.MemoryDetail(new ProcessVitals(null, 412, 96, null, null, null, null)));

    [Fact]
    public void TheThreadsAloneAreSaidWithTheirWord() =>
        Assert.Equal("41 threads", VitalsText.MemoryDetail(new ProcessVitals(null, null, null, 41, null, null, null)));

    [Fact]
    public void TheHeapAndTheThreadsAreJoinedAsTwoClauses() =>
        Assert.Equal(
            "Managed heap 96 MB; 41 threads",
            VitalsText.MemoryDetail(new ProcessVitals(null, null, 96, 41, null, null, null)));

    [Theory]
    [InlineData(1024.0, 1024.0, "Working set 1.0 GB, of which managed heap 1.0 GB")]
    [InlineData(1433.6, 2048.0, "Working set 1.4 GB, of which managed heap 2.0 GB")]
    [InlineData(1023.0, 1024.0, "Working set 1023 MB, of which managed heap 1.0 GB")]
    public void BothMemoryClausesSwitchToGigabytesAtOneGigabyte(double workingSetMb, double gcHeapMb, string expected) =>
        Assert.Equal(expected, VitalsText.MemoryDetail(new ProcessVitals(null, workingSetMb, gcHeapMb, null, null, null, null)));

    [Fact]
    public void TheHeapAloneSwitchesToGigabytes() =>
        Assert.Equal("Managed heap 1.5 GB", VitalsText.MemoryDetail(new ProcessVitals(null, null, 1536, null, null, null, null)));

    [Fact]
    public void ALargeThreadCountKeepsInvariantDigitsForAFinnishReader() =>
        CultureScope.Runs("fi-FI", () =>
        {
            Assert.Equal("1234567 threads", VitalsText.MemoryDetail(new ProcessVitals(null, null, null, 1234567, null, null, null)));
            Assert.Equal(
                "Working set 1.4 GB, of which managed heap 96 MB; 1234567 threads",
                VitalsText.MemoryDetail(new ProcessVitals(null, 1433.6, 96, 1234567, null, null, null)));
        });
}
