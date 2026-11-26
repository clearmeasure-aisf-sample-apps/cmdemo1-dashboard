using System.Text.Json;

namespace Dashboard.Health;

/// <summary>The CRAP score (change risk anti-patterns): complexity that tests do not cover.</summary>
public sealed record CrapFacts(double? Max, double? Threshold, int? OverThreshold)
{
    internal static CrapFacts? Read(JsonElement? section)
    {
        var crap = new CrapFacts(JsonRead.Number(section, "max"), JsonRead.Number(section, "threshold"), JsonRead.Count(section, "overThreshold"));
        return MeasuredFacts.OrNone(crap, crap.MeasuredNothing);
    }

    internal bool MeasuredNothing => MeasuredFacts.None(Max, Threshold, OverThreshold);
}
