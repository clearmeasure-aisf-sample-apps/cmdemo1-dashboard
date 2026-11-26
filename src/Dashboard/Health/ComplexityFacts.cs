using System.Text.Json;

namespace Dashboard.Health;

/// <summary>Cyclomatic complexity of the methods.</summary>
public sealed record ComplexityFacts(double? Average, int? Max, int? Methods)
{
    internal static ComplexityFacts? Read(JsonElement? section)
    {
        var complexity = new ComplexityFacts(JsonRead.Number(section, "average"), JsonRead.Count(section, "max"), JsonRead.Count(section, "methods"));
        return MeasuredFacts.OrNone(complexity, complexity.MeasuredNothing);
    }

    internal bool MeasuredNothing => MeasuredFacts.None(Average, Max, Methods);
}
