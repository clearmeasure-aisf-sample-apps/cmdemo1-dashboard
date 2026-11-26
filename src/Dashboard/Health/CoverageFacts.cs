using System.Text.Json;

namespace Dashboard.Health;

public sealed record CoverageFacts(double? LinePercent, double? BranchPercent)
{
    internal static CoverageFacts? Read(JsonElement? section)
    {
        var coverage = new CoverageFacts(JsonRead.Number(section, "linePercent"), JsonRead.Number(section, "branchPercent"));
        return MeasuredFacts.OrNone(coverage, coverage.MeasuredNothing);
    }

    internal bool MeasuredNothing => MeasuredFacts.None(LinePercent, BranchPercent);
}
